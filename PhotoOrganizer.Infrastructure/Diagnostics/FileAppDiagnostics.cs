using System.Runtime.InteropServices;
using PhotoOrganizer.Application.Abstractions;

namespace PhotoOrganizer.Infrastructure.Diagnostics;

/// <summary>
/// Zapisuje błędy do pliku w katalogu danych aplikacji (obok ustawień). W pełni lokalnie — nic nie
/// jest wysyłane. Metody statyczne są używane przez globalny handler nieobsłużonych wyjątków
/// (który działa, zanim kontener DI jest gotowy).
/// </summary>
public sealed class FileAppDiagnostics : IAppDiagnostics
{
    /// <summary>Katalog logów: &lt;AppData&gt;/PhotoOrganizer/logs (spójnie z lokalizacją ustawień).</summary>
    public static string LogsDir { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PhotoOrganizer",
        "logs");

    public string LogsDirectory => LogsDir;

    public void LogError(string context, Exception exception) => Write(context, exception);

    /// <summary>Statyczny zapis błędu — dla globalnego handlera wyjątków. Nigdy nie rzuca.</summary>
    public static void Write(string context, Exception? exception)
    {
        try
        {
            Directory.CreateDirectory(LogsDir);
            var file = Path.Combine(LogsDir, $"error-{DateTime.Now:yyyyMMdd}.log");
            var version = typeof(FileAppDiagnostics).Assembly.GetName().Version?.ToString() ?? "?";
            var entry =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}{Environment.NewLine}" +
                $"  PhotoOrganizer {version} · {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture}){Environment.NewLine}" +
                $"  {exception}{Environment.NewLine}{Environment.NewLine}";
            File.AppendAllText(file, entry);
        }
        catch
        {
            // Logowanie nie może nigdy wywrócić aplikacji.
        }
    }
}
