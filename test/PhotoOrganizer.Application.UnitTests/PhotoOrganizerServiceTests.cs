using PhotoOrganizer.Application.Abstractions;
using PhotoOrganizer.Application.Organizing;
using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.Services;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Application.UnitTests;

public sealed class PhotoOrganizerServiceTests
{
    private static readonly FilePath Working = FilePath.From("C:/wa");
    private static readonly OrganizeSettings Ymd = new() { Granularity = DateGranularity.YearMonthDay };

    private static MediaFile Image(string path, DateOnly date)
    {
        var name = path[(path.LastIndexOf('/') + 1)..];
        return new MediaFile(FilePath.From(path), name, "x", "jpg", MediaKind.Image, 1,
            CaptureDate.Dated(date, CaptureDateSource.ExifOriginal), false, false);
    }

    private static PhotoOrganizerService Build(
        IReadOnlyList<MediaGroup> groups, IFileMover mover, FakeUndoSession session)
    {
        var store = new FakeUndoStore(session);
        return new PhotoOrganizerService(
            new FakeScanner(groups),
            new OrganizePlanner(new TargetPathCalculator()),
            mover,
            store,
            new FakeProbe(),
            new StubClock());
    }

    [Fact]
    public async Task PreviewAsync_classifies_scanned_groups_without_side_effects()
    {
        var groups = new[] { MediaGroup.Single(Image("C:/wa/a.jpg", new DateOnly(2024, 3, 15))) };
        var service = Build(groups, new FakeMover([]), new FakeUndoSession());

        var plan = await service.PreviewAsync(Working, Ymd);

        var move = Assert.Single(plan.Moves);
        Assert.Equal(MoveDisposition.WillMove, move.Disposition);
        Assert.Equal("C:/wa/2024/03/15/a.jpg", move.TargetPath.Value);
    }

    [Fact]
    public async Task ApplyAsync_commits_undo_session_and_reports_results()
    {
        var groups = new[] { MediaGroup.Single(Image("C:/wa/a.jpg", new DateOnly(2024, 3, 15))) };
        var session = new FakeUndoSession();
        var plan = new OrganizePlan(Working, Ymd,
        [
            new PlannedMove(groups[0].Primary, FilePath.From("C:/wa/2024/03/15/a.jpg"), "2024/03/15",
                MoveDisposition.WillMove, Guid.NewGuid())
        ]);
        var mover = new FakeMover([MoveResult.Moved(plan.Moves[0])]);
        var service = Build(groups, mover, session);

        var run = await service.ApplyAsync(plan);

        Assert.True(session.Committed);          // sesja undo została zatwierdzona
        Assert.True(mover.MoveCalled);
        Assert.Equal(1, run.MovedCount);
    }

    private sealed class FakeScanner(IReadOnlyList<MediaGroup> groups) : IMediaScanner
    {
        public Task<IReadOnlyList<MediaGroup>> ScanAsync(
            FilePath workingArea, OrganizeSettings settings, IProgress<ScanProgress>? progress = null, CancellationToken ct = default)
            => Task.FromResult(groups);
    }

    private sealed class FakeMover(IReadOnlyList<MoveResult> results) : IFileMover
    {
        public bool MoveCalled { get; private set; }

        public Task<IReadOnlyList<MoveResult>> MoveAsync(
            OrganizePlan plan, IUndoSession session, IProgress<MoveProgress>? progress = null, CancellationToken ct = default)
        {
            MoveCalled = true;
            return Task.FromResult(results);
        }

        public Task<IReadOnlyList<MoveResult>> UndoAsync(
            UndoLog log, IProgress<MoveProgress>? progress = null, CancellationToken ct = default)
            => Task.FromResult(results);
    }

    private sealed class FakeUndoStore(FakeUndoSession session) : IUndoLogStore
    {
        public Task<IUndoSession> BeginAsync(FilePath workingArea, Guid runId, CancellationToken ct = default)
            => Task.FromResult<IUndoSession>(session);

        public Task<UndoLog?> LoadLatestAsync(FilePath workingArea, CancellationToken ct = default)
            => Task.FromResult<UndoLog?>(null);

        public Task<IReadOnlyList<UndoLog>> LoadAllAsync(FilePath workingArea, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<UndoLog>>([]);
    }

    private sealed class FakeUndoSession : IUndoSession
    {
        public Guid RunId { get; } = Guid.NewGuid();
        public bool Committed { get; private set; }

        public Task RecordCreatedFolderAsync(string relativeFolder, CancellationToken ct = default) => Task.CompletedTask;
        public Task RecordMovedAsync(UndoEntry entry, CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken ct = default) { Committed = true; return Task.CompletedTask; }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class FakeProbe : IFileSystemProbe
    {
        public bool Exists(string absolutePath) => false;
        public string Combine(string path1, string path2) => $"{path1}/{path2}";
        public StringComparer PathComparer => StringComparer.OrdinalIgnoreCase;
    }

    private sealed class StubClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 7, 3, 0, 0, 0, TimeSpan.Zero);
    }
}
