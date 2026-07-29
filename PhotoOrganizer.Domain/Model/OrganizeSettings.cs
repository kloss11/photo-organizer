namespace PhotoOrganizer.Domain.Model;

/// <summary>
/// Konfiguracja porządkowania — wszystkie wybory użytkownika + sensowne wartości domyślne.
/// Rekord niemutowalny; zmiany przez <c>with</c>.
/// </summary>
public sealed record OrganizeSettings
{
    /// <summary>Kod języka interfejsu (pl/en/de/ru/es/fr). Preferencja UI utrwalana z ustawieniami.</summary>
    public string LanguageCode { get; init; } = "pl";

    public DateGranularity Granularity { get; init; } = DateGranularity.YearMonth;

    /// <summary>Czy dopełniać miesiąc/dzień zerem (03 zamiast 3). Rok zawsze 4-cyfrowy.</summary>
    public bool ZeroPadded { get; init; } = true;

    public UndatedPolicy UndatedPolicy { get; init; } = UndatedPolicy.MoveToFolder;

    public string UndatedFolderName { get; init; } = "Bez daty";

    public CollisionPolicy CollisionPolicy { get; init; } = CollisionPolicy.Skip;

    public ScanScope ScanScope { get; init; } = ScanScope.Recursive;

    public EmptyFolderCleanup EmptyFolderCleanup { get; init; } = EmptyFolderCleanup.Keep;

    /// <summary>Trzymać pliki o tej samej nazwie bazowej razem (data wg pliku głównego).</summary>
    public bool KeepCompanionsTogether { get; init; } = true;

    /// <summary>Pomijać pliki „tylko online" (placeholdery chmury) zamiast wymuszać pobranie.</summary>
    public bool SkipOnlineOnlyFiles { get; init; } = true;

    public IReadOnlySet<string> ImageExtensions { get; init; } = DefaultImageExtensions;

    public IReadOnlySet<string> VideoExtensions { get; init; } = DefaultVideoExtensions;

    /// <summary>Rozszerzenia plików towarzyszących (sidecar) grupowanych z plikiem głównym.</summary>
    public IReadOnlySet<string> CompanionExtensions { get; init; } = DefaultCompanionExtensions;

    public static readonly IReadOnlySet<string> DefaultImageExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "jpg", "jpeg", "png", "tif", "tiff", "heic", "heif",
            "cr2", "nef", "arw", "dng"
        };

    public static readonly IReadOnlySet<string> DefaultVideoExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "mp4", "mov", "m4v", "avi", "mts", "m2ts", "3gp"
        };

    public static readonly IReadOnlySet<string> DefaultCompanionExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "xmp", "aae", "thm"
        };

    /// <summary>Zwraca rodzaj mediów dla rozszerzenia albo <c>null</c>, gdy nierozpoznane.</summary>
    public MediaKind? ClassifyExtension(string extension)
    {
        if (ImageExtensions.Contains(extension))
            return MediaKind.Image;
        if (VideoExtensions.Contains(extension))
            return MediaKind.Video;
        return null;
    }

    public bool IsSupportedMedia(string extension) => ClassifyExtension(extension) is not null;

    public bool IsCompanionExtension(string extension) => CompanionExtensions.Contains(extension);
}
