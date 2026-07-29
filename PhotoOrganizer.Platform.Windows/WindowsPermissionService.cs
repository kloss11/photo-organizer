using PhotoOrganizer.Platform.Abstractions;

namespace PhotoOrganizer.Platform.Windows;

/// <summary>Windows nie wymaga specjalnych uprawnień do gestu ani odczytu folderu.</summary>
public sealed class WindowsPermissionService : IPlatformPermissionService
{
    public IReadOnlyList<PlatformPermission> Required { get; } = [];

    public Task<PermissionState> CheckAsync(PlatformPermission permission, CancellationToken ct = default) =>
        Task.FromResult(PermissionState.NotApplicable);

    public Task RequestAsync(PlatformPermission permission, CancellationToken ct = default) => Task.CompletedTask;
}
