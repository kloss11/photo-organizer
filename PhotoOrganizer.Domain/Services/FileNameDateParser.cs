using System.Text.RegularExpressions;

namespace PhotoOrganizer.Domain.Services;

/// <summary>
/// Ostatni krok łańcucha fallback: próbuje odczytać datę z nazwy pliku. Rozpoznaje popularne
/// schematy aparatów/telefonów/komunikatorów, np. <c>IMG_20230415_123456</c>, <c>VID-20230415-WA0012</c>,
/// <c>PXL_20230415_231502</c>, <c>2023-04-15 wakacje</c>, <c>Screenshot_2023-04-15</c>, <c>20230415123456</c>.
/// Świadomie konserwatywny: wyłącznie porządek rok-miesiąc-dzień (formaty dd-MM-yyyy są niejednoznaczne),
/// dopasowania walidowane kalendarzowo (miesiąc 1–12, dzień wg miesiąca), rok ograniczony do 1900–2099.
/// Polityka wiarygodności (od 1950) egzekwowana jest wyżej, przez <see cref="ValueObjects.CaptureDateBounds"/>.
/// </summary>
public static partial class FileNameDateParser
{
    // yyyy-MM-dd z separatorami (-, _, .), np. "2023-04-15", "2023.04.15_zdjecie".
    [GeneratedRegex(@"(?<!\d)(\d{4})[-._](\d{2})[-._](\d{2})(?!\d)")]
    private static partial Regex SeparatedPattern();

    // Zwarte yyyyMMdd otoczone nie-cyframi, np. "IMG_20230415_123456", "VID-20230415-WA0012".
    [GeneratedRegex(@"(?<!\d)(\d{4})(\d{2})(\d{2})(?!\d)")]
    private static partial Regex CompactPattern();

    // Zwarte yyyyMMddHHmmss (14 cyfr), np. "20230415123456".
    [GeneratedRegex(@"(?<!\d)(\d{4})(\d{2})(\d{2})\d{6}(?!\d)")]
    private static partial Regex CompactTimestampPattern();

    /// <summary>
    /// Próbuje odczytać datę z nazwy pliku (bez rozszerzenia). Zwraca pierwsze (od lewej)
    /// kalendarzowo poprawne dopasowanie.
    /// </summary>
    public static bool TryParse(string fileNameWithoutExtension, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
            return false;

        foreach (var pattern in (ReadOnlySpan<Regex>)[SeparatedPattern(), CompactPattern(), CompactTimestampPattern()])
        {
            foreach (Match match in pattern.Matches(fileNameWithoutExtension))
            {
                if (TryCreateDate(match, out date))
                    return true;
            }
        }

        return false;
    }

    private static bool TryCreateDate(Match match, out DateOnly date)
    {
        date = default;
        var year = int.Parse(match.Groups[1].ValueSpan);
        var month = int.Parse(match.Groups[2].ValueSpan);
        var day = int.Parse(match.Groups[3].ValueSpan);

        if (year is < 1900 or > 2099 || month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
            return false;

        date = new DateOnly(year, month, day);
        return true;
    }
}
