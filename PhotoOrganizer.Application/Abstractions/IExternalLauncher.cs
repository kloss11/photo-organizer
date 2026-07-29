namespace PhotoOrganizer.Application.Abstractions;

/// <summary>
/// Otwiera zasoby zewnętrzne domyślnymi narzędziami systemu (przeglądarka, menedżer plików).
/// Implementacja jest cross-platform (Windows/macOS/Linux) i tłumi błędy (najlepszy wysiłek).
/// </summary>
public interface IExternalLauncher
{
    /// <summary>Otwiera adres URL w domyślnej przeglądarce.</summary>
    void OpenUrl(string url);

    /// <summary>Otwiera folder w menedżerze plików (tworzy go, jeśli nie istnieje).</summary>
    void OpenFolder(string path);
}
