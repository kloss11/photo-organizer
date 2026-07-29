using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.Services;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Domain.UnitTests;

public sealed class OrganizePlannerTests
{
    private const string WorkingArea = "C:/wa";

    private readonly OrganizePlanner _planner = new(new TargetPathCalculator());

    // pathCombine niezależny od OS na potrzeby testów (zawsze '/').
    private static string Combine(string a, string b) => $"{a}/{b}";

    private OrganizePlan Plan(
        IReadOnlyList<MediaGroup> groups,
        OrganizeSettings settings,
        IEnumerable<string>? existing = null,
        StringComparer? comparer = null)
    {
        var onDisk = new HashSet<string>(existing ?? [], comparer ?? StringComparer.OrdinalIgnoreCase);
        return _planner.CreatePlan(
            FilePath.From(WorkingArea),
            groups,
            settings,
            targetExists: onDisk.Contains,
            pathCombine: Combine,
            pathComparer: comparer ?? StringComparer.OrdinalIgnoreCase);
    }

    private static MediaFile Image(string path, DateOnly? date, bool online = false, bool symlink = false)
    {
        var name = path[(path.LastIndexOf('/') + 1)..];
        var dot = name.LastIndexOf('.');
        var baseName = dot < 0 ? name : name[..dot];
        var ext = dot < 0 ? string.Empty : name[(dot + 1)..].ToLowerInvariant();
        var captured = date is { } d
            ? CaptureDate.Dated(d, CaptureDateSource.ExifOriginal)
            : CaptureDate.Undated();
        return new MediaFile(FilePath.From(path), name, baseName, ext, MediaKind.Image, 100, captured, online, symlink);
    }

    private static MediaGroup Single(MediaFile file) => MediaGroup.Single(file);

    private static readonly OrganizeSettings Ymd = new() { Granularity = DateGranularity.YearMonthDay };

    [Fact]
    public void Simple_file_is_planned_to_move_into_date_folder()
    {
        var file = Image("C:/wa/holiday.jpg", new DateOnly(2024, 3, 15));
        var plan = Plan([Single(file)], Ymd);

        var move = Assert.Single(plan.Moves);
        Assert.Equal(MoveDisposition.WillMove, move.Disposition);
        Assert.Equal("C:/wa/2024/03/15/holiday.jpg", move.TargetPath.Value);
        Assert.Equal("2024/03/15", move.RelativeTargetFolder);
    }

    [Fact]
    public void File_already_in_target_is_already_in_place()
    {
        var file = Image("C:/wa/2024/03/15/holiday.jpg", new DateOnly(2024, 3, 15));
        var plan = Plan([Single(file)], Ymd);

        Assert.Equal(MoveDisposition.AlreadyInPlace, plan.Moves[0].Disposition);
        Assert.Equal(0, plan.ActionableCount);
    }

    [Fact]
    public void Existing_target_with_skip_policy_is_skipped()
    {
        var file = Image("C:/wa/holiday.jpg", new DateOnly(2024, 3, 15));
        var plan = Plan([Single(file)], Ymd with { CollisionPolicy = CollisionPolicy.Skip },
            existing: ["C:/wa/2024/03/15/holiday.jpg"]);

        Assert.Equal(MoveDisposition.SkipCollision, plan.Moves[0].Disposition);
    }

    [Fact]
    public void Existing_target_with_overwrite_policy_will_overwrite()
    {
        var file = Image("C:/wa/holiday.jpg", new DateOnly(2024, 3, 15));
        var plan = Plan([Single(file)], Ymd with { CollisionPolicy = CollisionPolicy.Overwrite },
            existing: ["C:/wa/2024/03/15/holiday.jpg"]);

        Assert.Equal(MoveDisposition.WillOverwrite, plan.Moves[0].Disposition);
        Assert.Equal(1, plan.OverwriteCount);
    }

    [Fact]
    public void Two_sources_same_target_second_is_collision_within_plan()
    {
        // Dwa pliki z różnych podfolderów, ta sama nazwa i data → drugi koliduje (rezerwacja w planie).
        var a = Image("C:/wa/sub1/IMG_001.jpg", new DateOnly(2024, 3, 15));
        var b = Image("C:/wa/sub2/IMG_001.jpg", new DateOnly(2024, 3, 15));
        var plan = Plan([Single(a), Single(b)], Ymd);

        Assert.Equal(MoveDisposition.WillMove, plan.Moves[0].Disposition);
        Assert.Equal(MoveDisposition.SkipCollision, plan.Moves[1].Disposition);
    }

    [Fact]
    public void Undated_with_skip_policy_is_skipped()
    {
        var file = Image("C:/wa/mystery.jpg", date: null);
        var plan = Plan([Single(file)], Ymd with { UndatedPolicy = UndatedPolicy.Skip });

        Assert.Equal(MoveDisposition.SkipUndated, plan.Moves[0].Disposition);
    }

    [Fact]
    public void Undated_with_move_policy_goes_to_undated_folder()
    {
        var file = Image("C:/wa/mystery.jpg", date: null);
        var plan = Plan([Single(file)], Ymd with { UndatedPolicy = UndatedPolicy.MoveToFolder, UndatedFolderName = "Bez daty" });

        Assert.Equal(MoveDisposition.WillMove, plan.Moves[0].Disposition);
        Assert.Equal("C:/wa/Bez daty/mystery.jpg", plan.Moves[0].TargetPath.Value);
    }

    [Fact]
    public void Online_only_file_is_skipped_when_enabled()
    {
        var file = Image("C:/wa/cloud.jpg", new DateOnly(2024, 3, 15), online: true);
        var plan = Plan([Single(file)], Ymd with { SkipOnlineOnlyFiles = true });

        Assert.Equal(MoveDisposition.SkipOnlineOnly, plan.Moves[0].Disposition);
    }

    [Fact]
    public void Symlink_is_skipped()
    {
        var file = Image("C:/wa/link.jpg", new DateOnly(2024, 3, 15), symlink: true);
        var plan = Plan([Single(file)], Ymd);

        Assert.Equal(MoveDisposition.SkipSymlink, plan.Moves[0].Disposition);
    }

    [Fact]
    public void Companions_follow_primary_date_folder()
    {
        // RAW bez daty + JPEG z datą; grupa trafia do folderu wg pliku z najlepszą datą (JPEG).
        var raw = Image("C:/wa/IMG_007.dng", date: null);
        var jpg = Image("C:/wa/IMG_007.jpg", new DateOnly(2024, 3, 15));
        var xmp = new MediaFile(FilePath.From("C:/wa/IMG_007.xmp"), "IMG_007.xmp", "IMG_007", "xmp",
            MediaKind.Image, 10, CaptureDate.Undated(), false, false);

        var group = new MediaGroup(jpg, [raw, xmp]);
        var plan = Plan([group], Ymd);

        Assert.All(plan.Moves, m => Assert.StartsWith("C:/wa/2024/03/15/", m.TargetPath.Value));
        Assert.Equal(3, plan.Moves.Count);
        Assert.All(plan.Moves, m => Assert.Equal(MoveDisposition.WillMove, m.Disposition));
    }

    [Fact]
    public void Case_sensitive_comparer_treats_differing_case_as_no_collision()
    {
        var a = Image("C:/wa/sub1/IMG.jpg", new DateOnly(2024, 3, 15));
        var b = Image("C:/wa/sub2/img.jpg", new DateOnly(2024, 3, 15));

        var plan = Plan([Single(a), Single(b)], Ymd, comparer: StringComparer.Ordinal);

        // Na FS case-sensitive "IMG.jpg" i "img.jpg" to różne cele — oba się przenoszą.
        Assert.Equal(MoveDisposition.WillMove, plan.Moves[0].Disposition);
        Assert.Equal(MoveDisposition.WillMove, plan.Moves[1].Disposition);
    }
}
