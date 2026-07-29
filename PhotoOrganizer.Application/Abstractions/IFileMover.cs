using PhotoOrganizer.Domain.Model;

namespace PhotoOrganizer.Application.Abstractions;

/// <summary>
/// Wykonuje fizyczne przenoszenie i cofanie zgodnie z kontraktem bezpieczeństwa:
/// nigdy nie nadpisuje bez zapisu kopii zapasowej, nigdy nie usuwa źródła przed kompletną kopią
/// (cross-volume), wpis undo zapisuje dopiero po udanym ruchu, na anulowanie przerywa czysto.
/// </summary>
public interface IFileMover
{
    Task<IReadOnlyList<MoveResult>> MoveAsync(
        OrganizePlan plan,
        IUndoSession session,
        IProgress<MoveProgress>? progress = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<MoveResult>> UndoAsync(
        UndoLog log,
        IProgress<MoveProgress>? progress = null,
        CancellationToken ct = default);
}
