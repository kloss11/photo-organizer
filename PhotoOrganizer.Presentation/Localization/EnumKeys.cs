using PhotoOrganizer.Domain.Model;

namespace PhotoOrganizer.Presentation.Localization;

/// <summary>Mapuje wartości enumów domeny na klucze tłumaczeń.</summary>
public static class EnumKeys
{
    public static string Of(DateGranularity value) => "Granularity_" + value;
    public static string Of(CollisionPolicy value) => "Collision_" + value;
    public static string Of(ScanScope value) => "ScanScope_" + value;
    public static string Of(UndatedPolicy value) => "Undated_" + value;
    public static string Action(MoveDisposition value) => "Act_" + value;

    public static string Source(CaptureDateSource value) => value switch
    {
        CaptureDateSource.ExifOriginal => "Src_ExifOriginal",
        CaptureDateSource.ExifDigitized => "Src_ExifDigitized",
        CaptureDateSource.QuickTimeCreation => "Src_QuickTime",
        CaptureDateSource.FileLastWrite => "Src_FileDate",
        CaptureDateSource.FileCreation => "Src_FileCreation",
        CaptureDateSource.FileName => "Src_FileName",
        _ => "Src_None"
    };
}
