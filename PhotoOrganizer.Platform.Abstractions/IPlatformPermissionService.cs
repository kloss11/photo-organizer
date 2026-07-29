namespace PhotoOrganizer.Platform.Abstractions;

/// <summary>
/// Sprawdzanie i żądanie uprawnień systemowych. Windows/Linux zwykle zwracają
/// <see cref="PermissionState.NotApplicable"/>; macOS wymaga Accessibility + Automation(Finder).
/// </summary>
public interface IPlatformPermissionService
{
    IReadOnlyList<PlatformPermission> Required { get; }

    Task<PermissionState> CheckAsync(PlatformPermission permission, CancellationToken ct = default);

    Task RequestAsync(PlatformPermission permission, CancellationToken ct = default);
}
