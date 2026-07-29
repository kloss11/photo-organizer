using MetadataExtractor;
using MetadataExtractor.Formats.QuickTime;
using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;
using PhotoOrganizer.Infrastructure.Metadata;

namespace PhotoOrganizer.Infrastructure.IntegrationTests;

/// <summary>
/// Testy łańcucha fallback czytnika dat na realnych plikach w katalogu tymczasowym.
/// Kluczowy scenariusz regresyjny: wideo z wyzerowanym polem „creation time" w nagłówku
/// QuickTime/MP4 (epoka 1904) NIE może trafić do folderu 1904 — data ma przejść na kolejne źródło.
/// </summary>
public sealed class CaptureDateReaderFallbackTests : IDisposable
{
    private readonly string _root;
    private readonly MetadataExtractorCaptureDateReader _reader = new(new SystemClock());

    public CaptureDateReaderFallbackTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "PhotoOrgReaderTests", Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try { System.IO.Directory.Delete(_root, recursive: true); } catch { /* sprzątanie best-effort */ }
    }

    private static readonly DateTime Implausible = new(1904, 1, 1, 12, 0, 0);

    private string CreateFile(string name, byte[] content, DateTime? lastWrite = null, DateTime? creation = null)
    {
        var full = Path.Combine(_root, name);
        File.WriteAllBytes(full, content);
        if (creation is { } c)
            File.SetCreationTime(full, c);
        if (lastWrite is { } w)
            File.SetLastWriteTime(full, w);
        return full;
    }

    private Task<CaptureDate> ReadVideoAsync(string path, bool allowMetadata = true) =>
        _reader.ReadAsync(FilePath.From(path), MediaKind.Video, allowMetadata);

    [Fact]
    public async Task Video_with_zeroed_quicktime_creation_time_does_not_land_in_1904()
    {
        var path = CreateFile("zeroed.mp4", MinimalMp4WithZeroedCreationTime(),
            lastWrite: new DateTime(2022, 6, 1, 10, 0, 0));

        var date = await ReadVideoAsync(path);

        Assert.True(date.HasDate);
        Assert.Equal(new DateOnly(2022, 6, 1), date.Value);             // NIE 1904-01-01
        Assert.Equal(CaptureDateSource.FileLastWrite, date.Source);
    }

    [Fact]
    public async Task Implausible_last_write_falls_back_to_creation_time()
    {
        var path = CreateFile("clip.mp4", "nie-media"u8.ToArray(),
            creation: new DateTime(2021, 7, 3, 9, 0, 0), lastWrite: Implausible);

        var date = await ReadVideoAsync(path);

        Assert.Equal(new DateOnly(2021, 7, 3), date.Value);
        Assert.Equal(CaptureDateSource.FileCreation, date.Source);
    }

    [Fact]
    public async Task Future_last_write_is_rejected_in_favor_of_creation_time()
    {
        var path = CreateFile("clip.mp4", "nie-media"u8.ToArray(),
            creation: new DateTime(2020, 2, 2, 8, 0, 0), lastWrite: DateTime.Now.AddDays(30));

        var date = await ReadVideoAsync(path);

        Assert.Equal(new DateOnly(2020, 2, 2), date.Value);
        Assert.Equal(CaptureDateSource.FileCreation, date.Source);
    }

    [Fact]
    public async Task Implausible_file_times_fall_back_to_date_in_file_name()
    {
        var path = CreateFile("VID_20230415_120000.mp4", "nie-media"u8.ToArray(),
            creation: Implausible, lastWrite: Implausible);

        var date = await ReadVideoAsync(path);

        Assert.Equal(new DateOnly(2023, 4, 15), date.Value);
        Assert.Equal(CaptureDateSource.FileName, date.Source);
    }

    [Fact]
    public async Task No_plausible_source_yields_undated()
    {
        var path = CreateFile("clip.mp4", "nie-media"u8.ToArray(),
            creation: Implausible, lastWrite: Implausible);

        var date = await ReadVideoAsync(path);

        Assert.False(date.HasDate);
        Assert.Equal(CaptureDateSource.Unknown, date.Source);
    }

    [Fact]
    public async Task Online_only_file_skips_metadata_and_uses_file_date()
    {
        var path = CreateFile("zeroed.mp4", MinimalMp4WithZeroedCreationTime(),
            lastWrite: new DateTime(2022, 6, 1, 10, 0, 0));

        var date = await ReadVideoAsync(path, allowMetadata: false);

        Assert.Equal(new DateOnly(2022, 6, 1), date.Value);
        Assert.Equal(CaptureDateSource.FileLastWrite, date.Source);
    }

    [Fact]
    public void Fixture_sanity_minimal_mp4_really_reports_epoch_1904()
    {
        // Gwarancja, że test regresyjny ćwiczy ODRZUCENIE daty 1904 z metadanych,
        // a nie przypadkiem sam brak metadanych.
        var path = CreateFile("fixture.mp4", MinimalMp4WithZeroedCreationTime());

        var directories = ImageMetadataReader.ReadMetadata(path);
        var header = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();

        Assert.NotNull(header);
        Assert.True(header!.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out var created));
        Assert.Equal(1904, created.Year);
    }

    /// <summary>
    /// Minimalny plik ISO-BMFF (MP4): atom „ftyp" + „moov/mvhd" z creation_time = 0,
    /// czyli dokładnie przypadek dający epokę QuickTime 1904-01-01.
    /// </summary>
    private static byte[] MinimalMp4WithZeroedCreationTime()
    {
        using var ms = new MemoryStream();

        void U32(uint v)
        {
            ms.WriteByte((byte)(v >> 24));
            ms.WriteByte((byte)(v >> 16));
            ms.WriteByte((byte)(v >> 8));
            ms.WriteByte((byte)v);
        }

        void Tag(string fourCc)
        {
            foreach (var ch in fourCc)
                ms.WriteByte((byte)ch);
        }

        // ftyp (20 B): major brand isom, minor version 0, compatible isom.
        U32(20); Tag("ftyp"); Tag("isom"); U32(0); Tag("isom");

        // moov (8 + 108 B) zawierający wyłącznie mvhd w wersji 0.
        U32(116); Tag("moov");
        U32(108); Tag("mvhd");
        U32(0);          // wersja + flagi
        U32(0);          // creation_time = 0 → 1904-01-01
        U32(0);          // modification_time
        U32(1000);       // timescale
        U32(0);          // duration
        U32(0x00010000); // rate 1.0
        ms.WriteByte(0x01); ms.WriteByte(0x00); // volume 1.0
        for (var i = 0; i < 10; i++) ms.WriteByte(0); // reserved
        U32(0x00010000); U32(0); U32(0);              // macierz jednostkowa
        U32(0); U32(0x00010000); U32(0);
        U32(0); U32(0); U32(0x40000000);
        for (var i = 0; i < 6; i++) U32(0);           // pre_defined
        U32(1);                                        // next_track_id

        return ms.ToArray();
    }
}
