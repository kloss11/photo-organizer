using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using PhotoOrganizer.Platform.Abstractions;

namespace PhotoOrganizer.Platform.MacOS;

/// <summary>
/// Uprawnienia macOS: Accessibility (dla globalnego hooka SharpHook) oraz Automation→Finder
/// (dla odczytu folderu). Accessibility sprawdzamy przez <c>AXIsProcessTrusted</c>; żądanie otwiera
/// właściwy panel Ustawień systemowych (Prywatność i bezpieczeństwo).
/// </summary>
public sealed partial class MacPermissionService : IPlatformPermissionService
{
    public IReadOnlyList<PlatformPermission> Required { get; } =
        [PlatformPermission.Accessibility, PlatformPermission.AutomationFinder];

    public Task<PermissionState> CheckAsync(PlatformPermission permission, CancellationToken ct = default)
    {
        if (!OperatingSystem.IsMacOS())
            return Task.FromResult(PermissionState.NotApplicable);

        return permission switch
        {
            PlatformPermission.Accessibility => Task.FromResult(IsAccessibilityTrusted()
                ? PermissionState.Granted
                : PermissionState.Denied),
            // Automation daje się wiarygodnie sprawdzić dopiero przy próbie sterowania Finderem
            // (reader zgłosi PermissionDenied przy -1743) — tu zwracamy „nieustalone".
            _ => Task.FromResult(PermissionState.NotDetermined)
        };
    }

    public Task RequestAsync(PlatformPermission permission, CancellationToken ct = default)
    {
        if (!OperatingSystem.IsMacOS())
            return Task.CompletedTask;

        var pane = permission == PlatformPermission.Accessibility
            ? "x-apple.systempreferences:com.apple.preference.security?Privacy_Accessibility"
            : "x-apple.systempreferences:com.apple.preference.security?Privacy_Automation";

        TryOpen(pane);
        return Task.CompletedTask;
    }

    private static void TryOpen(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo("open", url) { UseShellExecute = false });
        }
        catch
        {
            // best-effort — brak panelu nie może wywrócić aplikacji
        }
    }

    [SupportedOSPlatform("macos")]
    private static bool IsAccessibilityTrusted()
    {
        try
        {
            return AXIsProcessTrusted();
        }
        catch
        {
            return false;
        }
    }

    [SupportedOSPlatform("macos")]
    [LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AXIsProcessTrusted();
}
