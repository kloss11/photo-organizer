using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Domain.Model;

/// <summary>
/// Zaplanowana akcja dla pojedynczego pliku. Pliki jednej grupy współdzielą <see cref="GroupId"/>
/// i ten sam folder docelowy (data wg pliku głównego grupy).
/// </summary>
public sealed record PlannedMove(
    MediaFile Source,
    FilePath TargetPath,
    string RelativeTargetFolder,
    MoveDisposition Disposition,
    Guid GroupId,
    string? Reason = null)
{
    /// <summary>Czy akcja faktycznie przeniesie plik (z ewentualnym nadpisaniem).</summary>
    public bool IsActionable =>
        Disposition is MoveDisposition.WillMove or MoveDisposition.WillOverwrite;
}

/// <summary>
/// Pełny podgląd operacji (dry-run) zwracany przez <c>PreviewAsync</c>. Nie wykonuje żadnych zapisów;
/// dokładnie ten obiekt jest później przekazywany do <c>ApplyAsync</c> („co widzisz, to wykonasz").
/// </summary>
public sealed record OrganizePlan(
    FilePath WorkingArea,
    OrganizeSettings Settings,
    IReadOnlyList<PlannedMove> Moves)
{
    public int WillMoveCount => Count(MoveDisposition.WillMove);
    public int OverwriteCount => Count(MoveDisposition.WillOverwrite);
    public int CollisionCount => Count(MoveDisposition.SkipCollision);
    public int UndatedSkippedCount => Count(MoveDisposition.SkipUndated);
    public int OnlineOnlyCount => Count(MoveDisposition.SkipOnlineOnly);
    public int SymlinkCount => Count(MoveDisposition.SkipSymlink);
    public int AlreadyInPlaceCount => Count(MoveDisposition.AlreadyInPlace);

    /// <summary>Liczba plików, które zostaną fizycznie przeniesione (z nadpisaniem włącznie).</summary>
    public int ActionableCount => Moves.Count(m => m.IsActionable);

    public bool HasActionableMoves => ActionableCount > 0;

    private int Count(MoveDisposition disposition) =>
        Moves.Count(m => m.Disposition == disposition);
}
