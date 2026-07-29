namespace PhotoOrganizer.Application.Abstractions;

/// <summary>
/// Wąski dostęp do systemu plików potrzebny planerowi (istnienie celu, łączenie ścieżek, komparator).
/// Trzyma <c>System.IO</c> poza warstwami Domain/Application, zachowując czystość planera.
/// </summary>
public interface IFileSystemProbe
{
    bool Exists(string absolutePath);

    string Combine(string path1, string path2);

    /// <summary>Komparator zgodny z wrażliwością na wielkość liter systemu plików celu.</summary>
    StringComparer PathComparer { get; }
}
