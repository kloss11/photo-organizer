using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using PhotoOrganizer.Platform.Abstractions;
using PhotoOrganizer.Platform.Shared;

namespace PhotoOrganizer.Platform.Windows;

public static class DependencyInjection
{
    /// <summary>Rejestruje adaptery Windows (gest SharpHook + odczyt folderu z Eksploratora przez Shell COM).</summary>
    [SupportedOSPlatform("windows")]
    public static IServiceCollection AddWindowsPlatform(this IServiceCollection services)
    {
        services.AddSingleton<IGlobalGestureService, SharpHookGestureService>();
        services.AddSingleton<IFileManagerLocationReader, WindowsFileManagerLocationReader>();
        services.AddSingleton<IWorkingAreaProvider, GestureWorkingAreaProvider>();
        services.AddSingleton<IPlatformPermissionService, WindowsPermissionService>();
        return services;
    }
}
