using PhotoOrganizer.Application.Abstractions;

namespace PhotoOrganizer.Infrastructure.FileSystem;

/// <summary>
/// Konkretny dostęp do systemu plików dla planera. Wrażliwość na wielkość liter dobrana wg OS
/// (domyślnie: Linux = case-sensitive, Windows/macOS = case-insensitive). W przyszłości można
/// wykrywać per-wolumin, ale ten domyślny wybór jest poprawny dla typowych konfiguracji.
/// </summary>
public sealed class FileSystemProbe : IFileSystemProbe
{
    public bool Exists(string absolutePath) =>
        File.Exists(absolutePath) || Directory.Exists(absolutePath);

    public string Combine(string path1, string path2) => Path.Combine(path1, path2);

    public StringComparer PathComparer { get; } =
        OperatingSystem.IsLinux() ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
}
