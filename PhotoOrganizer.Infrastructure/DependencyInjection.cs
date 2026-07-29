using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoOrganizer.Application.Abstractions;
using PhotoOrganizer.Infrastructure.FileSystem;
using PhotoOrganizer.Infrastructure.Metadata;
using PhotoOrganizer.Infrastructure.Settings;
using PhotoOrganizer.Infrastructure.Undo;

namespace PhotoOrganizer.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Rejestruje niezależne od OS implementacje portów (skan, EXIF/QuickTime, przenoszenie, undo, ustawienia).
    /// Sygnatura z <see cref="IConfiguration"/> dla spójności z konwencją repozytorium (ProjectClub).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IFileSystemProbe, FileSystemProbe>();
        services.AddSingleton<ICaptureDateReader, MetadataExtractorCaptureDateReader>();
        services.AddSingleton<IMediaScanner, FileSystemMediaScanner>();
        services.AddSingleton<IFileMover, FileSystemMover>();
        services.AddSingleton<IUndoLogStore, JsonlUndoLogStore>();

        var settingsPath = configuration["PhotoOrganizer:SettingsPath"];
        services.AddSingleton<ISettingsStore>(_ => new JsonSettingsStore(settingsPath));

        return services;
    }
}
