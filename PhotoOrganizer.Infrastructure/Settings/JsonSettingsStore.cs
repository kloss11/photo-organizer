using System.Text.Json;
using System.Text.Json.Serialization;
using PhotoOrganizer.Application.Abstractions;
using PhotoOrganizer.Domain.Model;

namespace PhotoOrganizer.Infrastructure.Settings;

/// <summary>
/// Ustawienia w pliku JSON w katalogu danych aplikacji. Mapowanie przez DTO, bo <see cref="OrganizeSettings"/>
/// używa <see cref="IReadOnlySet{T}"/> (interfejsu nie da się bezpośrednio zdeserializować).
/// </summary>
public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _settingsPath;

    public JsonSettingsStore(string? settingsPath = null)
    {
        _settingsPath = settingsPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PhotoOrganizer",
            "settings.json");
    }

    public async Task<OrganizeSettings> LoadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_settingsPath))
            return new OrganizeSettings();

        try
        {
            await using var stream = File.OpenRead(_settingsPath);
            var dto = await JsonSerializer.DeserializeAsync<SettingsDto>(stream, JsonOptions, ct).ConfigureAwait(false);
            return dto?.ToSettings() ?? new OrganizeSettings();
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            // Uszkodzony plik ustawień → wartości domyślne (nie wywracamy aplikacji).
            return new OrganizeSettings();
        }
    }

    public async Task SaveAsync(OrganizeSettings settings, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
        await using var stream = File.Create(_settingsPath);
        await JsonSerializer.SerializeAsync(stream, SettingsDto.FromSettings(settings), JsonOptions, ct).ConfigureAwait(false);
    }

    private sealed record SettingsDto
    {
        public string LanguageCode { get; init; } = "pl";
        public DateGranularity Granularity { get; init; } = DateGranularity.YearMonth;
        public bool ZeroPadded { get; init; } = true;
        public UndatedPolicy UndatedPolicy { get; init; } = UndatedPolicy.MoveToFolder;
        public string UndatedFolderName { get; init; } = "Bez daty";
        public CollisionPolicy CollisionPolicy { get; init; } = CollisionPolicy.Skip;
        public ScanScope ScanScope { get; init; } = ScanScope.Recursive;
        public EmptyFolderCleanup EmptyFolderCleanup { get; init; } = EmptyFolderCleanup.Keep;
        public bool KeepCompanionsTogether { get; init; } = true;
        public bool SkipOnlineOnlyFiles { get; init; } = true;
        public List<string>? ImageExtensions { get; init; }
        public List<string>? VideoExtensions { get; init; }
        public List<string>? CompanionExtensions { get; init; }

        public OrganizeSettings ToSettings() => new()
        {
            LanguageCode = LanguageCode,
            Granularity = Granularity,
            ZeroPadded = ZeroPadded,
            UndatedPolicy = UndatedPolicy,
            UndatedFolderName = UndatedFolderName,
            CollisionPolicy = CollisionPolicy,
            ScanScope = ScanScope,
            EmptyFolderCleanup = EmptyFolderCleanup,
            KeepCompanionsTogether = KeepCompanionsTogether,
            SkipOnlineOnlyFiles = SkipOnlineOnlyFiles,
            ImageExtensions = ToSet(ImageExtensions) ?? OrganizeSettings.DefaultImageExtensions,
            VideoExtensions = ToSet(VideoExtensions) ?? OrganizeSettings.DefaultVideoExtensions,
            CompanionExtensions = ToSet(CompanionExtensions) ?? OrganizeSettings.DefaultCompanionExtensions
        };

        public static SettingsDto FromSettings(OrganizeSettings settings) => new()
        {
            LanguageCode = settings.LanguageCode,
            Granularity = settings.Granularity,
            ZeroPadded = settings.ZeroPadded,
            UndatedPolicy = settings.UndatedPolicy,
            UndatedFolderName = settings.UndatedFolderName,
            CollisionPolicy = settings.CollisionPolicy,
            ScanScope = settings.ScanScope,
            EmptyFolderCleanup = settings.EmptyFolderCleanup,
            KeepCompanionsTogether = settings.KeepCompanionsTogether,
            SkipOnlineOnlyFiles = settings.SkipOnlineOnlyFiles,
            ImageExtensions = settings.ImageExtensions.ToList(),
            VideoExtensions = settings.VideoExtensions.ToList(),
            CompanionExtensions = settings.CompanionExtensions.ToList()
        };

        private static IReadOnlySet<string>? ToSet(List<string>? values) =>
            values is null ? null : new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
    }
}
