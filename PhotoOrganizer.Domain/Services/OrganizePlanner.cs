using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Domain.Services;

/// <summary>
/// Czysta logika planowania: klasyfikuje każdy plik na akcję (przenieś/nadpisz/pomiń/już-na-miejscu).
/// Nie dotyka systemu plików — zależności I/O wstrzykiwane jako delegaty, dzięki czemu klasa jest
/// w 100% testowalna na zbiorze w pamięci. Wynik jest deterministyczny względem wejścia
/// (poza opaque <see cref="PlannedMove.GroupId"/>).
/// </summary>
public sealed class OrganizePlanner
{
    private readonly ITargetPathCalculator _calculator;

    public OrganizePlanner(ITargetPathCalculator calculator) => _calculator = calculator;

    /// <param name="targetExists">Czy plik o danej ścieżce bezwzględnej istnieje na dysku.</param>
    /// <param name="pathCombine">Łączenie dwóch segmentów ścieżki (opakowanie <c>Path.Combine</c>).</param>
    /// <param name="pathComparer">Komparator ścieżek (case-insensitive na Win/mac, sensitive na Linux).</param>
    public OrganizePlan CreatePlan(
        FilePath workingArea,
        IReadOnlyList<MediaGroup> groups,
        OrganizeSettings settings,
        Func<string, bool> targetExists,
        Func<string, string, string> pathCombine,
        StringComparer pathComparer)
    {
        var moves = new List<PlannedMove>();
        var claimed = new HashSet<string>(pathComparer);

        foreach (var group in groups)
        {
            var groupId = Guid.NewGuid();
            var date = group.Primary.CaptureDate;
            var undatedSkip = !date.HasDate && settings.UndatedPolicy == UndatedPolicy.Skip;

            var segments = _calculator.ResolveRelativeSegments(date, settings);
            var relativeFolder = string.Join('/', segments);

            var targetFolderAbs = workingArea.Value;
            foreach (var segment in segments)
                targetFolderAbs = pathCombine(targetFolderAbs, segment);

            foreach (var file in group.AllFiles())
            {
                var targetAbs = pathCombine(targetFolderAbs, file.FileName);
                FilePath.TryCreate(targetAbs, out var targetPath);

                var (disposition, reason) = Classify(
                    file, targetAbs, undatedSkip, settings, targetExists, claimed, pathComparer);

                if (disposition is MoveDisposition.WillMove or MoveDisposition.WillOverwrite)
                    claimed.Add(targetAbs);

                moves.Add(new PlannedMove(file, targetPath, relativeFolder, disposition, groupId, reason));
            }
        }

        return new OrganizePlan(workingArea, settings, moves);
    }

    private static (MoveDisposition Disposition, string? Reason) Classify(
        MediaFile file,
        string targetAbs,
        bool undatedSkip,
        OrganizeSettings settings,
        Func<string, bool> targetExists,
        HashSet<string> claimed,
        StringComparer pathComparer)
    {
        // 1) Już na miejscu — bezpieczna idempotencja ponownych uruchomień (bez heurystyk nazw folderów).
        if (pathComparer.Equals(file.Path.Value, targetAbs))
            return (MoveDisposition.AlreadyInPlace, "Już w docelowym folderze.");

        // 2) Brak daty + polityka „pomiń".
        if (undatedSkip)
            return (MoveDisposition.SkipUndated, "Brak daty wykonania (polityka: pomiń).");

        // 3) Plik „tylko online" — nie ruszamy placeholderów chmury.
        if (settings.SkipOnlineOnlyFiles && file.IsOnlineOnly)
            return (MoveDisposition.SkipOnlineOnly, "Plik tylko online (nie pobrano na dysk).");

        // 4) Link symboliczny — pomijany w v1 (ochrona przed pętlą/ucieczką poza obszar).
        if (file.IsSymlink)
            return (MoveDisposition.SkipSymlink, "Link symboliczny (pominięty w v1).");

        // 5) Kolizja nazwy (na dysku lub zarezerwowana w bieżącym planie).
        if (targetExists(targetAbs) || claimed.Contains(targetAbs))
        {
            return settings.CollisionPolicy == CollisionPolicy.Overwrite
                ? (MoveDisposition.WillOverwrite, "Nadpisze istniejący plik (kopia zapasowa w undo).")
                : (MoveDisposition.SkipCollision, "Plik o tej nazwie już istnieje w folderze docelowym.");
        }

        // 6) Zwykłe przeniesienie.
        return (MoveDisposition.WillMove, null);
    }
}
