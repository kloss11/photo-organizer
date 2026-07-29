using PhotoOrganizer.Domain.Model;

namespace PhotoOrganizer.Domain.ValueObjects;

/// <summary>
/// Data wykonania jako value object. Przechowuje wyłącznie <see cref="DateOnly"/> (bez czasu i strefy) —
/// budujemy z niej foldery rok/miesiąc/dzień. Data EXIF to czas ścienny (bez konwersji UTC),
/// data QuickTime bywa normalizowana do czasu lokalnego już w readerze.
/// </summary>
public readonly record struct CaptureDate
{
    private CaptureDate(DateOnly value, CaptureDateSource source, bool hasDate)
    {
        Value = value;
        Source = source;
        HasDate = hasDate;
    }

    public DateOnly Value { get; }

    public CaptureDateSource Source { get; }

    public bool HasDate { get; }

    public static CaptureDate Dated(DateOnly value, CaptureDateSource source) =>
        new(value, source, true);

    public static CaptureDate Undated() =>
        new(default, CaptureDateSource.Unknown, false);
}
