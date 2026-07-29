using System.Globalization;
using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Domain.Services;

/// <summary>
/// Czysta, deterministyczna implementacja budowy segmentów daty. Formatowanie zawsze
/// <see cref="CultureInfo.InvariantCulture"/> (rok 4-cyfrowy; miesiąc/dzień dopełniane zerem
/// zależnie od <see cref="OrganizeSettings.ZeroPadded"/>).
/// </summary>
public sealed class TargetPathCalculator : ITargetPathCalculator
{
    public IReadOnlyList<string> ResolveRelativeSegments(CaptureDate date, OrganizeSettings settings)
    {
        if (!date.HasDate)
            return [settings.UndatedFolderName];

        var value = date.Value;
        var year = value.Year.ToString("D4", CultureInfo.InvariantCulture);
        var monthFormat = settings.ZeroPadded ? "D2" : "D1";

        return settings.Granularity switch
        {
            DateGranularity.Year => [year],
            DateGranularity.YearMonth =>
                [year, value.Month.ToString(monthFormat, CultureInfo.InvariantCulture)],
            DateGranularity.YearMonthDay =>
                [
                    year,
                    value.Month.ToString(monthFormat, CultureInfo.InvariantCulture),
                    value.Day.ToString(monthFormat, CultureInfo.InvariantCulture)
                ],
            _ => [year]
        };
    }
}
