using PhotoOrganizer.Application.Abstractions;
using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Infrastructure.FileSystem;

/// <summary>
/// Wykonuje i cofa przenoszenie zgodnie z kontraktem bezpieczeństwa:
/// <list type="bullet">
/// <item>nigdy nie nadpisuje bez uprzedniej kopii zapasowej (odwracalność trybu Overwrite),</item>
/// <item>przy przenoszeniu między woluminami: kopiuj → zweryfikuj → dopiero usuń źródło,</item>
/// <item>wpis do logu cofania powstaje DOPIERO po udanym ruchu,</item>
/// <item>identyczne pliki (rozmiar + zawartość) są pomijane niezależnie od polityki,</item>
/// <item>anulowanie przerywa czysto między plikami (spójny, częściowy, odwracalny stan).</item>
/// </list>
/// </summary>
public sealed class FileSystemMover : IFileMover
{
    public async Task<IReadOnlyList<MoveResult>> MoveAsync(
        OrganizePlan plan,
        IUndoSession session,
        IProgress<MoveProgress>? progress = null,
        CancellationToken ct = default)
    {
        var results = new List<MoveResult>(plan.Moves.Count);
        var createdFolders = new HashSet<string>(StringComparer.Ordinal);
        var total = plan.ActionableCount;
        var processed = 0;
        var backupDir = Path.Combine(
            plan.WorkingArea.Value, FileSystemMediaScanner.MetadataDirectoryName, "backup", session.RunId.ToString("N"));

        foreach (var move in plan.Moves)
        {
            if (ct.IsCancellationRequested)
                break; // Czyste przerwanie: zwracamy dotychczasowe wyniki (undo obejmie to, co przeniesiono).

            if (!move.IsActionable)
            {
                results.Add(MoveResult.Skipped(move, move.Reason));
                continue;
            }

            progress?.Report(new MoveProgress(processed, total, move.Source.FileName));

            try
            {
                results.Add(await ExecuteMoveAsync(move, session, createdFolders, backupDir, ct).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                results.Add(MoveResult.Failed(move, ex.Message));
            }

            processed++;
        }

        progress?.Report(new MoveProgress(processed, total, null));
        return results;
    }

    private async Task<MoveResult> ExecuteMoveAsync(
        PlannedMove move,
        IUndoSession session,
        HashSet<string> createdFolders,
        string backupDir,
        CancellationToken ct)
    {
        var source = move.Source.Path.Value;
        var target = move.TargetPath.Value;

        if (!File.Exists(source))
            return MoveResult.Failed(move, "Plik źródłowy zniknął przed przeniesieniem.");

        string? backupPath = null;

        if (File.Exists(target))
        {
            // Identyczny plik docelowy — przenoszenie duplikatu nic nie wnosi.
            if (FilesAreIdentical(source, target))
                return MoveResult.Skipped(move, "Plik docelowy jest identyczny (rozmiar + zawartość).");

            if (move.Disposition == MoveDisposition.WillMove)
                return MoveResult.Skipped(move, "Kolizja nazwy wykryta w trakcie wykonania.");

            // WillOverwrite: zabezpiecz istniejący plik przed nadpisaniem (odwracalność).
            backupPath = BackupExistingTarget(target, backupDir);
        }

        var targetDirectory = Path.GetDirectoryName(target)!;
        await EnsureDirectoryAsync(targetDirectory, session, createdFolders, ct).ConfigureAwait(false);

        MoveFile(source, target);

        // Zapis do logu cofania DOPIERO po sukcesie; tożsamość = rozmiar + czas modyfikacji pliku w celu.
        var moved = new FileInfo(target);
        FilePath.TryCreate(target, out var movedTo);
        FilePath.TryCreate(source, out var originalPath);
        await session.RecordMovedAsync(
            new UndoEntry(movedTo, originalPath, moved.Length, moved.LastWriteTimeUtc, backupPath), ct).ConfigureAwait(false);

        return MoveResult.Moved(move);
    }

    public async Task<IReadOnlyList<MoveResult>> UndoAsync(
        UndoLog log,
        IProgress<MoveProgress>? progress = null,
        CancellationToken ct = default)
    {
        var results = new List<MoveResult>(log.Entries.Count);
        var total = log.Entries.Count;
        var processed = 0;

        // Odwrotna kolejność — wycofujemy od ostatniego ruchu.
        for (var i = log.Entries.Count - 1; i >= 0; i--)
        {
            if (ct.IsCancellationRequested)
                break;

            var entry = log.Entries[i];
            progress?.Report(new MoveProgress(processed, total, Path.GetFileName(entry.MovedTo.Value)));
            processed++;

            var plannedBack = ToPlannedMove(entry);

            try
            {
                results.Add(UndoEntry(entry, plannedBack));
            }
            catch (Exception ex)
            {
                results.Add(MoveResult.Failed(plannedBack, ex.Message));
            }
        }

        CleanupCreatedFolders(log.CreatedFolders);
        progress?.Report(new MoveProgress(processed, total, null));
        return await Task.FromResult(results).ConfigureAwait(false);
    }

    private static MoveResult UndoEntry(UndoEntry entry, PlannedMove plannedBack)
    {
        var current = entry.MovedTo.Value;
        var original = entry.OriginalPath.Value;

        if (!File.Exists(current))
            return MoveResult.Skipped(plannedBack, "Plik do cofnięcia już nie istnieje w miejscu docelowym.");

        // Tożsamość: nie przenoś z powrotem pliku, który został po operacji podmieniony.
        var info = new FileInfo(current);
        if (info.Length != entry.Size || info.LastWriteTimeUtc != entry.LastWriteUtc)
            return MoveResult.Skipped(plannedBack, "Plik został zmodyfikowany po operacji — pomijam (konflikt).");

        if (File.Exists(original))
            return MoveResult.Skipped(plannedBack, "Ścieżka źródłowa jest zajęta — pomijam (konflikt).");

        Directory.CreateDirectory(Path.GetDirectoryName(original)!);
        MoveFile(current, original);

        // Odtwórz plik nadpisany (tryb Overwrite): kopia zapasowa wraca na miejsce docelowe.
        if (entry.BackupOfOverwritten is { } backup && File.Exists(backup))
            MoveFile(backup, current);

        return MoveResult.Moved(plannedBack);
    }

    private static void CleanupCreatedFolders(IReadOnlyList<string> createdFolders)
    {
        // Usuwamy tylko foldery utworzone przez narzędzie i tylko gdy są puste; najgłębsze najpierw.
        foreach (var folder in createdFolders.OrderByDescending(f => f.Length))
        {
            try
            {
                if (Directory.Exists(folder) && !Directory.EnumerateFileSystemEntries(folder).Any())
                    Directory.Delete(folder);
            }
            catch
            {
                // brak uprawnień / wyścig — pomijamy
            }
        }
    }

    private static async Task EnsureDirectoryAsync(
        string targetDirectory,
        IUndoSession session,
        HashSet<string> createdFolders,
        CancellationToken ct)
    {
        if (Directory.Exists(targetDirectory))
            return;

        var toCreate = new List<string>();
        var current = targetDirectory;
        while (!string.IsNullOrEmpty(current) && !Directory.Exists(current))
        {
            toCreate.Add(current);
            var parent = Path.GetDirectoryName(current);
            if (parent is null || string.Equals(parent, current, StringComparison.Ordinal))
                break;
            current = parent;
        }

        toCreate.Reverse(); // od najpłytszego
        foreach (var directory in toCreate)
        {
            Directory.CreateDirectory(directory);
            if (createdFolders.Add(directory))
                await session.RecordCreatedFolderAsync(directory, ct).ConfigureAwait(false);
        }
    }

    private static void MoveFile(string source, string target)
    {
        if (IsSameVolume(source, target))
        {
            File.Move(source, target); // overwrite:false — cel jest wolny na tym etapie
            return;
        }

        // Cross-volume: kopiuj → zweryfikuj rozmiar → dopiero usuń źródło (nigdy odwrotnie).
        File.Copy(source, target, overwrite: false);
        var sourceInfo = new FileInfo(source);
        var targetInfo = new FileInfo(target);
        if (targetInfo.Length != sourceInfo.Length)
        {
            TryDelete(target);
            throw new IOException("Kopia między woluminami jest niekompletna — źródło zachowane.");
        }

        File.Delete(source);
    }

    private static string BackupExistingTarget(string target, string backupDir)
    {
        Directory.CreateDirectory(backupDir);
        var backupPath = Path.Combine(backupDir, $"{Path.GetFileName(target)}.{Guid.NewGuid():N}.bak");
        File.Move(target, backupPath); // zwalnia ścieżkę docelową; oryginał zachowany w backupie
        return backupPath;
    }

    private static bool FilesAreIdentical(string a, string b)
    {
        var infoA = new FileInfo(a);
        var infoB = new FileInfo(b);
        if (infoA.Length != infoB.Length)
            return false;

        const int bufferSize = 64 * 1024;
        using var streamA = new FileStream(a, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);
        using var streamB = new FileStream(b, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);
        Span<byte> bufferA = new byte[bufferSize];
        Span<byte> bufferB = new byte[bufferSize];

        int readA;
        while ((readA = streamA.ReadAtLeast(bufferA, bufferSize, throwOnEndOfStream: false)) > 0)
        {
            var readB = streamB.ReadAtLeast(bufferB, readA, throwOnEndOfStream: false);
            if (readA != readB || !bufferA[..readA].SequenceEqual(bufferB[..readB]))
                return false;
        }

        return true;
    }

    private static bool IsSameVolume(string a, string b)
    {
        var rootA = Path.GetPathRoot(Path.GetFullPath(a));
        var rootB = Path.GetPathRoot(Path.GetFullPath(b));
        return string.Equals(rootA, rootB, StringComparison.OrdinalIgnoreCase);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // best-effort
        }
    }

    private static PlannedMove ToPlannedMove(UndoEntry entry)
    {
        FilePath.TryCreate(entry.MovedTo.Value, out var current);
        var dummy = new MediaFile(current, Path.GetFileName(entry.MovedTo.Value), string.Empty, string.Empty,
            MediaKind.Image, entry.Size, CaptureDate.Undated(), false, false);
        FilePath.TryCreate(entry.OriginalPath.Value, out var back);
        return new PlannedMove(dummy, back, string.Empty, MoveDisposition.WillMove, Guid.Empty);
    }
}
