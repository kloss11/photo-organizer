using Microsoft.Extensions.DependencyInjection;
using PhotoOrganizer.Platform.Abstractions;

namespace PhotoOrganizer.Platform.Linux;

public static class DependencyInjection
{
    /// <summary>Rejestruje adaptery Linux (tryb fallback: ręczny wybór folderu, bez globalnego gestu).</summary>
    public static IServiceCollection AddLinuxPlatform(this IServiceCollection services)
    {
        services.AddSingleton<IGlobalGestureService, LinuxNoopGestureService>();
        services.AddSingleton<IFileManagerLocationReader, LinuxLocationReader>();
        services.AddSingleton<IWorkingAreaProvider, LinuxWorkingAreaProvider>();
        services.AddSingleton<IPlatformPermissionService, LinuxPermissionService>();
        return services;
    }
}
