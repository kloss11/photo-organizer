namespace PhotoOrganizer.Domain.ValueObjects;

/// <summary>
/// Bezwzględna ścieżka do pliku/folderu jako value object. Domena nie używa <c>System.IO</c>,
/// więc walidujemy jedynie „niepustość"; ścieżki bezwzględne dostarcza warstwa Infrastructure.
/// Porównania z uwzględnieniem wielkości liter wykonuje się przez wstrzykiwany komparator
/// (system plików bywa case-sensitive na Linux, case-insensitive na Windows/macOS).
/// </summary>
public readonly record struct FilePath
{
    private FilePath(string value) => Value = value;

    public string Value { get; }

    public static bool TryCreate(string? path, out FilePath filePath)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            filePath = default;
            return false;
        }

        filePath = new FilePath(path.Trim());
        return true;
    }

    /// <summary>Tworzy ścieżkę bez walidacji (dla ścieżek już zweryfikowanych przez system plików).</summary>
    public static FilePath From(string path) =>
        TryCreate(path, out var fp)
            ? fp
            : throw new ArgumentException("Ścieżka nie może być pusta.", nameof(path));

    public override string ToString() => Value;
}
