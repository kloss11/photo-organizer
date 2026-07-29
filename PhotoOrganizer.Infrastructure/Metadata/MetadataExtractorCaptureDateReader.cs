using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using PhotoOrganizer.Application.Abstractions;
using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.Services;
using PhotoOrganizer.Domain.ValueObjects;
using Directory = System.IO.Directory;

namespace PhotoOrganizer.Infrastructure.Metadata;

/// <summary>
/// Czyta datę wykonania biblioteką MetadataExtractor (obrazy: EXIF; wideo: QuickTime/MP4).
/// Łańcuch: EXIF Original → EXIF Digitized → QuickTime Created → data zapisu pliku →
/// data utworzenia pliku → data z nazwy pliku.
/// Każdy kandydat przechodzi kontrolę wiarygodności (<see cref="CaptureDateBounds"/>): daty sprzed 1950
/// (np. epoka QuickTime 1904-01-01 z wyzerowanego pola „creation time") i daty z przyszłości są odrzucane,
/// a łańcuch przechodzi do kolejnego źródła. NIGDY nie rzuca — błąd/uszkodzenie degraduje do kolejnego kroku.
/// </summary>
public sealed class MetadataExtractorCaptureDateReader : ICaptureDateReader
{
    private readonly IClock _clock;

    public MetadataExtractorCaptureDateReader(IClock clock)
    {
        _clock = clock;
    }

    public Task<CaptureDate> ReadAsync(FilePath path, MediaKind kind, bool allowMetadataRead, CancellationToken ct = default)
        => Task.FromResult(Read(path.Value, kind, allowMetadataRead));

    private CaptureDate Read(string file, MediaKind kind, bool allowMetadataRead)
    {
        // Daty EXIF/pliku to czas ścienny/lokalny, więc „dziś" również liczymy lokalnie.
        var today = DateOnly.FromDateTime(_clock.UtcNow.ToLocalTime().DateTime);

        if (allowMetadataRead)
        {
            var fromMetadata = kind == MediaKind.Image ? ReadImageDate(file) : ReadVideoDate(file);
            if (fromMetadata is { } captured && CaptureDateBounds.IsPlausible(captured.Value, today))
                return captured;
        }

        // Fallbacki na sygnaturach czasowych pliku (nie wymuszają pobrania placeholdera z chmury).
        if (TryGetFileDate(() => File.GetLastWriteTime(file)) is { } lastWrite &&
            CaptureDateBounds.IsPlausible(lastWrite, today))
            return CaptureDate.Dated(lastWrite, CaptureDateSource.FileLastWrite);

        if (TryGetFileDate(() => File.GetCreationTime(file)) is { } creation &&
            CaptureDateBounds.IsPlausible(creation, today))
            return CaptureDate.Dated(creation, CaptureDateSource.FileCreation);

        // Ostatnia deska ratunku: data zakodowana w nazwie pliku (IMG_20230415_… itp.).
        if (FileNameDateParser.TryParse(Path.GetFileNameWithoutExtension(file), out var fromName) &&
            CaptureDateBounds.IsPlausible(fromName, today))
            return CaptureDate.Dated(fromName, CaptureDateSource.FileName);

        return CaptureDate.Undated();
    }

    private static DateOnly? TryGetFileDate(Func<DateTime> read)
    {
        try
        {
            return DateOnly.FromDateTime(read());
        }
        catch
        {
            return null;
        }
    }

    private static CaptureDate? ReadImageDate(string file)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(file);
            var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (subIfd is null)
                return null;

            // EXIF DateTimeOriginal to czas ścienny — bez konwersji UTC (świadomie).
            if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var original))
                return CaptureDate.Dated(DateOnly.FromDateTime(original), CaptureDateSource.ExifOriginal);

            if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out var digitized))
                return CaptureDate.Dated(DateOnly.FromDateTime(digitized), CaptureDateSource.ExifDigitized);

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static CaptureDate? ReadVideoDate(string file)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(file);
            var header = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
            if (header is not null &&
                header.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out var created))
            {
                // Data QuickTime bywa w UTC (epoka 1904) — normalizacja do czasu lokalnego.
                var normalized = created.Kind == DateTimeKind.Utc ? created.ToLocalTime() : created;
                return CaptureDate.Dated(DateOnly.FromDateTime(normalized), CaptureDateSource.QuickTimeCreation);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
