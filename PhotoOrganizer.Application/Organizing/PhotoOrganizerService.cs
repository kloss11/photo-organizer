using PhotoOrganizer.Application.Abstractions;
using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.Services;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Application.Organizing;

/// <summary>
/// Spina skaner, czysty planer i mover w trzy przypadki użycia. Sam nie dotyka systemu plików —
/// deleguje I/O do portów, a klasyfikację do <see cref="OrganizePlanner"/>.
/// </summary>
public sealed class PhotoOrganizerService : IPhotoOrganizer
{
    private readonly IMediaScanner _scanner;
    private readonly OrganizePlanner _planner;
    private readonly IFileMover _mover;
    private readonly IUndoLogStore _undoStore;
    private readonly IFileSystemProbe _fileSystem;
    private readonly IClock _clock;

    public PhotoOrganizerService(
        IMediaScanner scanner,
        OrganizePlanner planner,
        IFileMover mover,
        IUndoLogStore undoStore,
        IFileSystemProbe fileSystem,
        IClock clock)
    {
        _scanner = scanner;
        _planner = planner;
        _mover = mover;
        _undoStore = undoStore;
        _fileSystem = fileSystem;
        _clock = clock;
    }

    public async Task<OrganizePlan> PreviewAsync(
        FilePath workingArea,
        OrganizeSettings settings,
        IProgress<ScanProgress>? progress = null,
        CancellationToken ct = default)
    {
        var groups = await _scanner.ScanAsync(workingArea, settings, progress, ct).ConfigureAwait(false);

        // Czysta klasyfikacja — żadnych zapisów. Delegaty kierują I/O do Infrastructure.
        return _planner.CreatePlan(
            workingArea,
            groups,
            settings,
            targetExists: _fileSystem.Exists,
            pathCombine: _fileSystem.Combine,
            pathComparer: _fileSystem.PathComparer);
    }

    public async Task<OrganizeRun> ApplyAsync(
        OrganizePlan plan,
        IProgress<MoveProgress>? progress = null,
        CancellationToken ct = default)
    {
        var runId = Guid.NewGuid();
        var startedUtc = _clock.UtcNow;

        await using var session = await _undoStore.BeginAsync(plan.WorkingArea, runId, ct).ConfigureAwait(false);
        var results = await _mover.MoveAsync(plan, session, progress, ct).ConfigureAwait(false);
        await session.CommitAsync(ct).ConfigureAwait(false);

        return new OrganizeRun(runId, startedUtc, plan.WorkingArea, results, ct.IsCancellationRequested);
    }

    public async Task<OrganizeRun> UndoAsync(
        UndoLog log,
        IProgress<MoveProgress>? progress = null,
        CancellationToken ct = default)
    {
        var startedUtc = _clock.UtcNow;
        var results = await _mover.UndoAsync(log, progress, ct).ConfigureAwait(false);
        return new OrganizeRun(log.RunId, startedUtc, log.WorkingArea, results, ct.IsCancellationRequested);
    }
}
