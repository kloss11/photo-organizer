using System.Collections.Concurrent;
using PhotoOrganizer.Application.Abstractions;
using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Infrastructure.FileSystem;

/// <summary>
/// Skaner systemu plików: enumeruje pliki wg <see cref="ScanScope"/>, pomija katalog metadanych
/// narzędzia i dowiązania katalogów, wykrywa placeholdery chmury oraz linki symboliczne, grupuje
/// pliki towarzyszące (po katalogu + nazwie bazowej) i równolegle odczytuje daty.
/// </summary>
public sealed class FileSystemMediaScanner : IMediaScanner
{
    /// <summary>Katalog metadanych narzędzia (undo/backup) — nigdy nie skanowany.</summary>
    public const string MetadataDirectoryName = ".photoorganizer";

    private const int FileAttributeRecallOnOpen = 0x00040000;
    private const int FileAttributeRecallOnDataAccess = 0x00400000;

    private readonly ICaptureDateReader _dateReader;
    private readonly IFileSystemProbe _fileSystem;

    public FileSystemMediaScanner(ICaptureDateReader dateReader, IFileSystemProbe fileSystem)
    {
        _dateReader = dateReader;
        _fileSystem = fileSystem;
    }

    public async Task<IReadOnlyList<MediaGroup>> ScanAsync(
        FilePath workingArea,
        OrganizeSettings settings,
        IProgress<ScanProgress>? progress = null,
        CancellationToken ct = default)
    {
        var scanned = CollectCandidates(workingArea.Value, settings, progress, ct);

        // Równoległy odczyt dat tylko dla plików multimedialnych (sidecary dziedziczą datę grupy).
        var dated = new ConcurrentDictionary<string, MediaFile>(_fileSystem.PathComparer);
        var mediaCandidates = scanned.Where(s => s.Kind is not null).ToList();

        await Parallel.ForEachAsync(
            mediaCandidates,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = ct },
            async (candidate, token) =>
            {
                var allowMetadata = !(settings.SkipOnlineOnlyFiles && candidate.IsOnlineOnly);
                var date = await _dateReader
                    .ReadAsync(FilePath.From(candidate.Path), candidate.Kind!.Value, allowMetadata, token)
                    .ConfigureAwait(false);
                dated[candidate.Path] = candidate.ToMediaFile(date);
            }).ConfigureAwait(false);

        return BuildGroups(scanned, dated, settings);
    }

    private List<ScannedFile> CollectCandidates(
        string root,
        OrganizeSettings settings,
        IProgress<ScanProgress>? progress,
        CancellationToken ct)
    {
        var result = new List<ScannedFile>();
        var filesSeen = 0;
        var mediaFound = 0;

        foreach (var path in EnumerateFiles(root, settings.ScanScope, ct))
        {
            ct.ThrowIfCancellationRequested();
            filesSeen++;

            var extension = NormalizeExtension(Path.GetExtension(path));
            var kind = settings.ClassifyExtension(extension);
            var isCompanion = settings.IsCompanionExtension(extension);

            // Ignorujemy pliki, które nie są ani obsługiwanymi mediami, ani plikami towarzyszącymi.
            if (kind is null && !isCompanion)
                continue;

            if (!TryReadAttributes(path, out var size, out var isOnlineOnly, out var isSymlink))
                continue;

            var directory = Path.GetDirectoryName(path) ?? root;
            var fileName = Path.GetFileName(path);
            var baseName = Path.GetFileNameWithoutExtension(path);

            result.Add(new ScannedFile(path, directory, fileName, baseName, extension, kind, size, isOnlineOnly, isSymlink));
            if (kind is not null)
                mediaFound++;

            if (filesSeen % 128 == 0)
                progress?.Report(new ScanProgress(filesSeen, mediaFound, path));
        }

        progress?.Report(new ScanProgress(filesSeen, mediaFound, null));
        return result;
    }

    private IReadOnlyList<MediaGroup> BuildGroups(
        List<ScannedFile> scanned,
        ConcurrentDictionary<string, MediaFile> dated,
        OrganizeSettings settings)
    {
        if (!settings.KeepCompanionsTogether)
        {
            // Bez grupowania: każdy plik multimedialny to osobna grupa; sidecary są ignorowane.
            return scanned
                .Where(s => s.Kind is not null && dated.ContainsKey(s.Path))
                .Select(s => MediaGroup.Single(dated[s.Path]))
                .ToList();
        }

        var groups = new List<MediaGroup>();
        var byKey = new Dictionary<string, List<ScannedFile>>(_fileSystem.PathComparer);

        foreach (var file in scanned)
        {
            var key = _fileSystem.Combine(file.Directory, file.BaseName);
            if (!byKey.TryGetValue(key, out var list))
                byKey[key] = list = [];
            list.Add(file);
        }

        foreach (var (_, members) in byKey)
        {
            var mediaMembers = members.Where(m => m.Kind is not null && dated.ContainsKey(m.Path)).ToList();
            if (mediaMembers.Count == 0)
                continue; // Grupa bez pliku multimedialnego (np. osierocony XMP) — pomijamy.

            // Plik główny: najbardziej wiarygodne źródło daty, remis rozstrzyga nazwa (deterministycznie).
            var primaryScanned = mediaMembers
                .OrderBy(m => (int)dated[m.Path].CaptureDate.Source)
                .ThenBy(m => m.FileName, StringComparer.Ordinal)
                .First();

            var primary = dated[primaryScanned.Path];
            var companions = members
                .Where(m => !ReferenceEquals(m, primaryScanned))
                .Select(m => dated.TryGetValue(m.Path, out var mf) ? mf : m.ToMediaFile(CaptureDate.Undated()))
                .ToList();

            groups.Add(new MediaGroup(primary, companions));
        }

        return groups;
    }

    private IEnumerable<string> EnumerateFiles(string root, ScanScope scope, CancellationToken ct)
    {
        var stack = new Stack<string>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            ct.ThrowIfCancellationRequested();
            var directory = stack.Pop();

            foreach (var file in SafeEnumerate(() => System.IO.Directory.EnumerateFiles(directory)))
                yield return file;

            if (scope != ScanScope.Recursive)
                continue;

            foreach (var sub in SafeEnumerate(() => System.IO.Directory.EnumerateDirectories(directory)))
            {
                var name = Path.GetFileName(sub);
                if (string.Equals(name, MetadataDirectoryName, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Nie wchodzimy w dowiązania katalogów (ochrona przed pętlą / ucieczką poza obszar).
                try
                {
                    if (new DirectoryInfo(sub).LinkTarget is not null)
                        continue;
                }
                catch
                {
                    continue;
                }

                stack.Push(sub);
            }
        }
    }

    private static IEnumerable<string> SafeEnumerate(Func<IEnumerable<string>> enumerate)
    {
        // Katalogi bez dostępu / znikające w trakcie skanu pomijamy zamiast przerywać cały skan.
        IEnumerable<string> items;
        try
        {
            items = enumerate().ToList();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            return [];
        }

        return items;
    }

    private static bool TryReadAttributes(string path, out long size, out bool isOnlineOnly, out bool isSymlink)
    {
        size = 0;
        isOnlineOnly = false;
        isSymlink = false;

        try
        {
            var info = new FileInfo(path);
            var attributes = info.Attributes;
            var raw = (int)attributes;

            isOnlineOnly = attributes.HasFlag(FileAttributes.Offline)
                           || (raw & FileAttributeRecallOnDataAccess) != 0
                           || (raw & FileAttributeRecallOnOpen) != 0;

            // Prawdziwy symlink ma LinkTarget; placeholder chmury to reparse point BEZ LinkTarget.
            isSymlink = info.LinkTarget is not null;
            size = info.Length; // Dla placeholderów zwraca rozmiar logiczny bez pobierania.
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeExtension(string extension) =>
        string.IsNullOrEmpty(extension) ? string.Empty : extension.TrimStart('.').ToLowerInvariant();

    private sealed record ScannedFile(
        string Path,
        string Directory,
        string FileName,
        string BaseName,
        string Extension,
        MediaKind? Kind,
        long Size,
        bool IsOnlineOnly,
        bool IsSymlink)
    {
        public MediaFile ToMediaFile(CaptureDate date) =>
            new(FilePath.From(Path), FileName, BaseName, Extension, Kind ?? MediaKind.Image, Size, date, IsOnlineOnly, IsSymlink);
    }
}
