using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Presentation.Localization;

namespace PhotoOrganizer.Presentation.ViewModels;

/// <summary>
/// Wiersz podglądu (dry-run). Opisy akcji i źródła daty są tłumaczone w chwili utworzenia
/// (po zmianie języka wystarczy ponowić Podgląd, by odświeżyć listę).
/// </summary>
public sealed record PlannedMoveRow(
    string FileName,
    string Date,
    string DateSource,
    string Action,
    string TargetFolder)
{
    public static PlannedMoveRow From(PlannedMove move)
    {
        var capture = move.Source.CaptureDate;
        var loc = Localizer.Instance;
        return new PlannedMoveRow(
            move.Source.FileName,
            capture.HasDate ? capture.Value.ToString("yyyy-MM-dd") : "—",
            capture.HasDate ? loc[EnumKeys.Source(capture.Source)] : loc["Src_None"],
            loc[EnumKeys.Action(move.Disposition)],
            move.RelativeTargetFolder);
    }
}
