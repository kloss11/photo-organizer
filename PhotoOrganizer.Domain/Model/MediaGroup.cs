namespace PhotoOrganizer.Domain.Model;

/// <summary>
/// Grupa plików o tej samej nazwie bazowej, które mają trafić do jednego folderu daty
/// (np. RAW + JPEG + XMP jednego ujęcia). Datę folderu wyznacza <see cref="Primary"/>.
/// Gdy grupowanie jest wyłączone, scaner tworzy grupy jednoelementowe (tylko Primary).
/// </summary>
public sealed record MediaGroup(MediaFile Primary, IReadOnlyList<MediaFile> Companions)
{
    /// <summary>Wszystkie pliki grupy: najpierw plik główny, potem towarzyszące.</summary>
    public IEnumerable<MediaFile> AllFiles()
    {
        yield return Primary;
        foreach (var companion in Companions)
            yield return companion;
    }

    public static MediaGroup Single(MediaFile file) => new(file, []);
}
