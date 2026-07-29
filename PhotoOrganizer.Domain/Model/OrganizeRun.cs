using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Domain.Model;

/// <summary>Rzeczywisty wynik wykonania jednej zaplanowanej akcji.</summary>
public sealed record MoveResult(PlannedMove Plan, MoveOutcome Outcome, string? Error = null)
{
    public static MoveResult Moved(PlannedMove plan) => new(plan, MoveOutcome.Moved);
    public static MoveResult Skipped(PlannedMove plan, string? reason = null) => new(plan, MoveOutcome.Skipped, reason);
    public static MoveResult Failed(PlannedMove plan, string error) => new(plan, MoveOutcome.Failed, error);
}

/// <summary>Podsumowanie zakończonego (lub anulowanego) uruchomienia porządkowania.</summary>
public sealed record OrganizeRun(
    Guid RunId,
    DateTimeOffset StartedUtc,
    FilePath WorkingArea,
    IReadOnlyList<MoveResult> Results,
    bool WasCancelled = false)
{
    public int MovedCount => Results.Count(r => r.Outcome == MoveOutcome.Moved);
    public int SkippedCount => Results.Count(r => r.Outcome == MoveOutcome.Skipped);
    public int FailedCount => Results.Count(r => r.Outcome == MoveOutcome.Failed);
}
