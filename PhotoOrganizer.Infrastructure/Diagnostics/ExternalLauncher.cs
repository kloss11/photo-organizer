using System.Diagnostics;
using PhotoOrganizer.Application.Abstractions;

namespace PhotoOrganizer.Infrastructure.Diagnostics;

/// <summary>
/// Otwiera URL-e i foldery domyślnymi narzędziami systemu. Wykrycie OS w czasie działania —
/// jedna implementacja dla Windows/macOS/Linux. Błędy są tłumione (brak przeglądarki/menedżera
/// plików nie może wywrócić aplikacji).
/// </summary>
public sealed class ExternalLauncher : IExternalLauncher
{
    public void OpenUrl(string url) => Launch(url);

    public void OpenFolder(string path)
    {
        try { Directory.CreateDirectory(path); }
        catch { /* najlepszy wysiłek */ }
        Launch(path);
    }

    private static void Launch(string target)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                // UseShellExecute=true: URL → domyślna przeglądarka, ścieżka → Eksplorator.
                Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
            }
            else
            {
                var opener = OperatingSystem.IsMacOS() ? "open" : "xdg-open";
                var psi = new ProcessStartInfo(opener) { UseShellExecute = false };
                psi.ArgumentList.Add(target); // ArgumentList radzi sobie ze spacjami w ścieżce
                Process.Start(psi);
            }
        }
        catch
        {
            // Najlepszy wysiłek — brak dostępnego narzędzia nie przerywa działania aplikacji.
        }
    }
}
