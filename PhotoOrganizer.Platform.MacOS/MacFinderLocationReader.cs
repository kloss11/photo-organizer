using System.Diagnostics;
using PhotoOrganizer.Platform.Abstractions;

namespace PhotoOrganizer.Platform.MacOS;

/// <summary>
/// Odczytuje folder z Findera przez AppleScript (osascript). Zwraca PRZEDNIE okno Findera
/// (nie okno pod kursorem) — udokumentowane ograniczenie v1; dlatego rozwiązana ścieżka jest
/// pokazywana użytkownikowi przed operacją. Wymaga uprawnienia Automation→Finder (błąd -1743).
/// </summary>
public sealed class MacFinderLocationReader : IFileManagerLocationReader
{
    private const string Script =
        "tell application \"Finder\"\n" +
        "if (count of windows) = 0 then return \"NO_WINDOW\"\n" +
        "try\n" +
        "return POSIX path of (target of front window as alias)\n" +
        "on error\n" +
        "return \"NOT_FOLDER\"\n" +
        "end try\n" +
        "end tell";

    public bool IsSupported => OperatingSystem.IsMacOS();

    public async Task<FolderLocationResult> ReadFolderAtAsync(ScreenPoint point, CancellationToken ct = default)
    {
        try
        {
            var psi = new ProcessStartInfo("osascript")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            psi.ArgumentList.Add("-e");
            psi.ArgumentList.Add(Script);

            using var process = Process.Start(psi);
            if (process is null)
                return FolderLocationResult.Fail(LocationReadStatus.Error, "Nie udało się uruchomić osascript.");

            var stdout = (await process.StandardOutput.ReadToEndAsync(ct).ConfigureAwait(false)).Trim();
            var stderr = await process.StandardError.ReadToEndAsync(ct).ConfigureAwait(false);
            await process.WaitForExitAsync(ct).ConfigureAwait(false);

            return Interpret(stdout, stderr);
        }
        catch (Exception ex)
        {
            return FolderLocationResult.Fail(LocationReadStatus.Error, ex.Message);
        }
    }

    /// <summary>Czysta interpretacja wyjścia osascript → status (łatwo testowalna).</summary>
    public static FolderLocationResult Interpret(string stdout, string stderr)
    {
        if (stderr.Contains("-1743") || stderr.Contains("Not authorized", StringComparison.OrdinalIgnoreCase))
            return FolderLocationResult.Fail(LocationReadStatus.PermissionDenied,
                "Brak uprawnienia Automation → Finder.");

        return stdout switch
        {
            "NO_WINDOW" => FolderLocationResult.Fail(LocationReadStatus.NoWindowAtPoint, "Brak otwartego okna Findera."),
            "NOT_FOLDER" => FolderLocationResult.Fail(LocationReadStatus.NotAFileManager, "Okno Findera nie wskazuje folderu."),
            "" => FolderLocationResult.Fail(LocationReadStatus.Error, string.IsNullOrWhiteSpace(stderr) ? "Puste wyjście osascript." : stderr.Trim()),
            _ => Directory.Exists(stdout)
                ? FolderLocationResult.Ok(stdout, "Finder")
                : FolderLocationResult.Fail(LocationReadStatus.NotAFileManager, "Ścieżka nie jest istniejącym folderem.")
        };
    }
}
