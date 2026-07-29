using PhotoOrganizer.Application.Abstractions;
using PhotoOrganizer.Application.Organizing;
using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.Services;
using PhotoOrganizer.Domain.ValueObjects;
using PhotoOrganizer.Infrastructure;
using PhotoOrganizer.Infrastructure.FileSystem;
using PhotoOrganizer.Infrastructure.Metadata;
using PhotoOrganizer.Infrastructure.Undo;

namespace PhotoOrganizer.Infrastructure.IntegrationTests;

/// <summary>
/// Testy end-to-end na realnym systemie plików w katalogu tymczasowym. Datę wyznacza data pliku
/// (deterministyczne, bez potrzeby generowania fixtur EXIF) — ścieżka silnika skan→plan→ruch→undo
/// jest identyczna niezależnie od źródła daty.
/// </summary>
public sealed class OrganizeEndToEndTests : IDisposable
{
    private readonly string _root;
    private readonly IUndoLogStore _undoStore = new JsonlUndoLogStore();
    private readonly IFileMover _mover = new FileSystemMover();
    private readonly IPhotoOrganizer _organizer;

    public OrganizeEndToEndTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "PhotoOrgTests", Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(_root);

        var probe = new FileSystemProbe();
        var scanner = new FileSystemMediaScanner(new MetadataExtractorCaptureDateReader(new SystemClock()), probe);
        _organizer = new PhotoOrganizerService(
            scanner, new OrganizePlanner(new TargetPathCalculator()), _mover, _undoStore, probe, new SystemClock());
    }

    private static readonly OrganizeSettings Ymd = new() { Granularity = DateGranularity.YearMonthDay };

    private FilePath Working => FilePath.From(_root);

    private string CreateFile(string relativePath, string content, DateTime date)
    {
        var full = Path.Combine(_root, relativePath);
        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
        File.SetLastWriteTime(full, date);
        return full;
    }

    private static bool SameContent(string path, string expected) =>
        File.Exists(path) && File.ReadAllText(path) == expected;

    [Fact]
    public async Task Preview_does_not_touch_the_filesystem()
    {
        CreateFile("a.jpg", "AAA", new DateTime(2024, 3, 15, 10, 0, 0));

        var plan = await _organizer.PreviewAsync(Working, Ymd);

        Assert.Equal(1, plan.WillMoveCount);
        Assert.True(File.Exists(Path.Combine(_root, "a.jpg")));            // źródło nietknięte
        Assert.False(System.IO.Directory.Exists(Path.Combine(_root, "2024"))); // nic nie utworzono
    }

    [Fact]
    public async Task Apply_then_undo_restores_exact_state()
    {
        CreateFile("a.jpg", "AAA", new DateTime(2024, 3, 15, 10, 0, 0));
        CreateFile("b.jpg", "BBB", new DateTime(2023, 12, 31, 8, 0, 0));

        var plan = await _organizer.PreviewAsync(Working, Ymd);
        var run = await _organizer.ApplyAsync(plan);

        Assert.Equal(2, run.MovedCount);
        Assert.True(SameContent(Path.Combine(_root, "2024", "03", "15", "a.jpg"), "AAA"));
        Assert.True(SameContent(Path.Combine(_root, "2023", "12", "31", "b.jpg"), "BBB"));
        Assert.False(File.Exists(Path.Combine(_root, "a.jpg")));

        var log = await _undoStore.LoadLatestAsync(Working);
        Assert.NotNull(log);
        var undo = await _organizer.UndoAsync(log!);

        Assert.Equal(2, undo.MovedCount);
        Assert.True(SameContent(Path.Combine(_root, "a.jpg"), "AAA"));
        Assert.True(SameContent(Path.Combine(_root, "b.jpg"), "BBB"));
        Assert.False(System.IO.Directory.Exists(Path.Combine(_root, "2024"))); // utworzone foldery sprzątnięte
    }

    [Fact]
    public async Task Collision_with_skip_policy_leaves_both_files_intact()
    {
        CreateFile("a.jpg", "SOURCE", new DateTime(2024, 3, 15, 10, 0, 0));
        CreateFile(Path.Combine("2024", "03", "15", "a.jpg"), "EXISTING", new DateTime(2024, 3, 15, 10, 0, 0));

        var plan = await _organizer.PreviewAsync(Working, Ymd with { CollisionPolicy = CollisionPolicy.Skip });
        var move = plan.Moves.Single(m => m.Source.Path.Value.EndsWith("a.jpg") && m.Disposition != MoveDisposition.AlreadyInPlace);
        Assert.Equal(MoveDisposition.SkipCollision, move.Disposition);

        await _organizer.ApplyAsync(plan);

        Assert.True(SameContent(Path.Combine(_root, "a.jpg"), "SOURCE"));                       // źródło zostaje
        Assert.True(SameContent(Path.Combine(_root, "2024", "03", "15", "a.jpg"), "EXISTING")); // cel nienaruszony
    }

    [Fact]
    public async Task Overwrite_policy_backs_up_and_undo_restores_overwritten_file()
    {
        CreateFile("a.jpg", "NEW", new DateTime(2024, 3, 15, 10, 0, 0));
        // Istniejący plik ma datę zgodną ze swoim folderem → jest „już na miejscu", nie jest reorganizowany.
        CreateFile(Path.Combine("2024", "03", "15", "a.jpg"), "OLD", new DateTime(2024, 3, 15, 9, 0, 0));

        var plan = await _organizer.PreviewAsync(Working, Ymd with { CollisionPolicy = CollisionPolicy.Overwrite });
        var run = await _organizer.ApplyAsync(plan);

        Assert.Equal(1, run.MovedCount);
        Assert.True(SameContent(Path.Combine(_root, "2024", "03", "15", "a.jpg"), "NEW")); // nadpisano

        var log = await _undoStore.LoadLatestAsync(Working);
        await _organizer.UndoAsync(log!);

        Assert.True(SameContent(Path.Combine(_root, "a.jpg"), "NEW"));                        // źródło wróciło
        Assert.True(SameContent(Path.Combine(_root, "2024", "03", "15", "a.jpg"), "OLD"));    // nadpisany plik odtworzony
    }

    [Fact]
    public async Task Identical_target_is_skipped_regardless_of_policy()
    {
        CreateFile("a.jpg", "SAME", new DateTime(2024, 3, 15, 10, 0, 0));
        CreateFile(Path.Combine("2024", "03", "15", "a.jpg"), "SAME", new DateTime(2024, 3, 15, 10, 0, 0));

        var plan = await _organizer.PreviewAsync(Working, Ymd with { CollisionPolicy = CollisionPolicy.Overwrite });
        var run = await _organizer.ApplyAsync(plan);

        Assert.Equal(0, run.MovedCount);
        Assert.True(SameContent(Path.Combine(_root, "a.jpg"), "SAME")); // duplikat nietknięty
    }

    [Fact]
    public async Task Undo_skips_file_modified_after_the_run()
    {
        CreateFile("a.jpg", "AAA", new DateTime(2024, 3, 15, 10, 0, 0));
        var plan = await _organizer.PreviewAsync(Working, Ymd);
        await _organizer.ApplyAsync(plan);

        // Użytkownik modyfikuje plik po operacji — cofnięcie nie może go ślepo przenieść z powrotem.
        var target = Path.Combine(_root, "2024", "03", "15", "a.jpg");
        File.WriteAllText(target, "MODIFIED-BY-USER");

        var log = await _undoStore.LoadLatestAsync(Working);
        var undo = await _organizer.UndoAsync(log!);

        Assert.Equal(0, undo.MovedCount);
        Assert.Equal(1, undo.SkippedCount);
        Assert.True(SameContent(target, "MODIFIED-BY-USER")); // zmodyfikowany plik pozostaje
    }

    [Fact]
    public async Task Undo_log_is_durable_before_commit_crash_recovery()
    {
        // Symulacja crashu: przenosimy plik i zapisujemy wpis undo, ale sesja NIE zostaje zatwierdzona.
        var source = CreateFile("a.jpg", "AAA", new DateTime(2024, 3, 15, 10, 0, 0));
        var runId = Guid.NewGuid();
        var target = Path.Combine(_root, "moved-a.jpg");

        await using (var session = await _undoStore.BeginAsync(Working, runId))
        {
            File.Move(source, target);
            var info = new FileInfo(target);
            await session.RecordMovedAsync(new UndoEntry(
                FilePath.From(target), FilePath.From(source), info.Length, info.LastWriteTimeUtc));
            // brak CommitAsync — udajemy przerwanie procesu
        }

        // Po „restarcie": log jest odczytywalny mimo braku commita, a cofnięcie odtwarza stan.
        var log = await _undoStore.LoadLatestAsync(Working);
        Assert.NotNull(log);
        Assert.Single(log!.Entries);

        var undo = await _mover.UndoAsync(log);
        Assert.Equal(1, undo.Count(r => r.Outcome == MoveOutcome.Moved));
        Assert.True(SameContent(source, "AAA"));
        Assert.False(File.Exists(target));
    }

    [Fact]
    public async Task Rerun_is_idempotent_already_organized_files_are_left_in_place()
    {
        CreateFile("a.jpg", "AAA", new DateTime(2024, 3, 15, 10, 0, 0));

        await _organizer.ApplyAsync(await _organizer.PreviewAsync(Working, Ymd));
        var secondPlan = await _organizer.PreviewAsync(Working, Ymd);

        Assert.Equal(0, secondPlan.ActionableCount);
        Assert.Equal(1, secondPlan.AlreadyInPlaceCount);
    }

    public void Dispose()
    {
        try
        {
            if (System.IO.Directory.Exists(_root))
                System.IO.Directory.Delete(_root, recursive: true);
        }
        catch
        {
            // sprzątanie best-effort
        }
    }
}
