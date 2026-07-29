using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using PhotoOrganizer.Platform.Abstractions;
using PhotoOrganizer.Platform.Shared;

namespace PhotoOrganizer.Platform.MacOS;

public static class DependencyInjection
{
    /// <summary>Rejestruje adaptery macOS (gest SharpHook + odczyt folderu z Findera przez osascript).</summary>
    [SupportedOSPlatform("macos")]
    public static IServiceCollection AddMacPlatform(this IServiceCollection services)
    {
        services.AddSingleton<IGlobalGestureService, SharpHookGestureService>();
        services.AddSingleton<IFileManagerLocationReader, MacFinderLocationReader>();
        services.AddSingleton<IWorkingAreaProvider, GestureWorkingAreaProvider>();
        services.AddSingleton<IPlatformPermissionService, MacPermissionService>();
        return services;
    }
}
