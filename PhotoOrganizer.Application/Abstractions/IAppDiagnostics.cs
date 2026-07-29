namespace PhotoOrganizer.Application.Abstractions;

/// <summary>
/// Diagnostyka aplikacji: lokalizacja logów błędów i zapisywanie wyjątków do pliku.
/// W pełni lokalnie — nic nie jest wysyłane na zewnątrz (zgodnie z prywatnością aplikacji).
/// </summary>
public interface IAppDiagnostics
{
    /// <summary>Katalog, w którym zapisywane są pliki logów.</summary>
    string LogsDirectory { get; }

    /// <summary>Zapisuje błąd do dziennika. Nigdy nie rzuca wyjątkiem.</summary>
    void LogError(string context, Exception exception);
}
