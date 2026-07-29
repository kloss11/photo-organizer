using PhotoOrganizer.Domain.Services;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Domain.UnitTests;

public sealed class FileNameDateParserTests
{
    [Theory]
    [InlineData("IMG_20230415_123456", 2023, 4, 15)]
    [InlineData("VID-20230415-WA0012", 2023, 4, 15)]
    [InlineData("PXL_20230415_231502123", 2023, 4, 15)]
    [InlineData("2023-04-15 wakacje", 2023, 4, 15)]
    [InlineData("Screenshot_2023-04-15", 2023, 4, 15)]
    [InlineData("2023.04.15_zdjecie", 2023, 4, 15)]
    [InlineData("2023_04_15", 2023, 4, 15)]
    [InlineData("20230415123456", 2023, 4, 15)]
    [InlineData("IMG-19991231-WA0000", 1999, 12, 31)]
    public void Recognizes_common_camera_and_messenger_patterns(string name, int y, int m, int d)
    {
        Assert.True(FileNameDateParser.TryParse(name, out var date));
        Assert.Equal(new DateOnly(y, m, d), date);
    }

    [Theory]
    [InlineData("IMG_1234")]
    [InlineData("P1050001")]
    [InlineData("DSC01234")]
    [InlineData("20231345")]       // miesiąc 13
    [InlineData("20230230")]       // 30 lutego
    [InlineData("12345678")]       // rok 1234 — poza zakresem
    [InlineData("15-04-2023")]     // dd-MM-yyyy jest niejednoznaczne — świadomie nieobsługiwane
    [InlineData("zdjecie")]
    [InlineData("")]
    public void Rejects_names_without_an_unambiguous_date(string name)
    {
        Assert.False(FileNameDateParser.TryParse(name, out _));
    }

    [Fact]
    public void Picks_leftmost_valid_match_when_several_candidates_exist()
    {
        Assert.True(FileNameDateParser.TryParse("kopia 20230415 oraz 19991231", out var date));
        Assert.Equal(new DateOnly(2023, 4, 15), date);
    }
}

public sealed class CaptureDateBoundsTests
{
    private static readonly DateOnly Today = new(2026, 7, 5);

    [Fact]
    public void Quicktime_epoch_1904_is_implausible() =>
        Assert.False(CaptureDateBounds.IsPlausible(new DateOnly(1904, 1, 1), Today));

    [Fact]
    public void Filetime_epoch_1601_is_implausible() =>
        Assert.False(CaptureDateBounds.IsPlausible(new DateOnly(1601, 1, 1), Today));

    [Fact]
    public void Day_before_1950_is_implausible() =>
        Assert.False(CaptureDateBounds.IsPlausible(new DateOnly(1949, 12, 31), Today));

    [Fact]
    public void Minimum_1950_is_plausible() =>
        Assert.True(CaptureDateBounds.IsPlausible(new DateOnly(1950, 1, 1), Today));

    [Fact]
    public void Today_and_tomorrow_are_plausible()
    {
        Assert.True(CaptureDateBounds.IsPlausible(Today, Today));
        Assert.True(CaptureDateBounds.IsPlausible(Today.AddDays(1), Today));
    }

    [Fact]
    public void Further_future_is_implausible() =>
        Assert.False(CaptureDateBounds.IsPlausible(Today.AddDays(2), Today));
}
