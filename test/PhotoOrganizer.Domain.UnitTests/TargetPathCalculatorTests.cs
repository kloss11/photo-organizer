using System.Globalization;
using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.Services;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Domain.UnitTests;

public sealed class TargetPathCalculatorTests
{
    private readonly TargetPathCalculator _calculator = new();

    private static CaptureDate On(int y, int m, int d) =>
        CaptureDate.Dated(new DateOnly(y, m, d), CaptureDateSource.ExifOriginal);

    [Fact]
    public void Year_granularity_returns_single_year_segment()
    {
        var segments = _calculator.ResolveRelativeSegments(
            On(2024, 3, 15), new OrganizeSettings { Granularity = DateGranularity.Year });

        Assert.Equal(["2024"], segments);
    }

    [Fact]
    public void YearMonth_zero_padded()
    {
        var segments = _calculator.ResolveRelativeSegments(
            On(2024, 3, 15), new OrganizeSettings { Granularity = DateGranularity.YearMonth, ZeroPadded = true });

        Assert.Equal(["2024", "03"], segments);
    }

    [Fact]
    public void YearMonthDay_zero_padded()
    {
        var segments = _calculator.ResolveRelativeSegments(
            On(2024, 3, 5), new OrganizeSettings { Granularity = DateGranularity.YearMonthDay, ZeroPadded = true });

        Assert.Equal(["2024", "03", "05"], segments);
    }

    [Fact]
    public void YearMonthDay_without_padding()
    {
        var segments = _calculator.ResolveRelativeSegments(
            On(2024, 3, 5), new OrganizeSettings { Granularity = DateGranularity.YearMonthDay, ZeroPadded = false });

        Assert.Equal(["2024", "3", "5"], segments);
    }

    [Fact]
    public void Undated_uses_configured_folder_name()
    {
        var segments = _calculator.ResolveRelativeSegments(
            CaptureDate.Undated(), new OrganizeSettings { UndatedFolderName = "Bez daty" });

        Assert.Equal(["Bez daty"], segments);
    }

    [Fact]
    public void Formatting_is_culture_invariant()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            // Kultura tr-TR ma nietypowe reguły — data folderów musi być niezależna od kultury.
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            var segments = _calculator.ResolveRelativeSegments(
                On(2024, 1, 9), new OrganizeSettings { Granularity = DateGranularity.YearMonthDay });

            Assert.Equal(["2024", "01", "09"], segments);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Boundary_dates_do_not_shift()
    {
        // Brak konwersji stref — 31 grudnia zostaje 31 grudnia.
        var newYearsEve = _calculator.ResolveRelativeSegments(
            On(2023, 12, 31), new OrganizeSettings { Granularity = DateGranularity.YearMonthDay });
        var leapDay = _calculator.ResolveRelativeSegments(
            On(2024, 2, 29), new OrganizeSettings { Granularity = DateGranularity.YearMonthDay });

        Assert.Equal(["2023", "12", "31"], newYearsEve);
        Assert.Equal(["2024", "02", "29"], leapDay);
    }
}
