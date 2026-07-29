using System.ComponentModel;

namespace PhotoOrganizer.Presentation.Localization;

/// <summary>
/// Singleton lokalizacji. Bindingi XAML używają indeksera <c>[klucz]</c>; zmiana języka podnosi
/// <c>PropertyChanged("Item[]")</c>, co odświeża wszystkie tłumaczone teksty bez restartu.
/// </summary>
public sealed class Localizer : INotifyPropertyChanged
{
    public static Localizer Instance { get; } = new();

    private IReadOnlyDictionary<string, string> _strings = Translations.For("pl");

    private Localizer() { }

    public string CurrentLanguage { get; private set; } = "pl";

    /// <summary>Zwraca tłumaczenie dla klucza (albo sam klucz, gdy brak — ułatwia diagnostykę).</summary>
    public string this[string key] => _strings.TryGetValue(key, out var value) ? value : key;

    /// <summary>Tłumaczenie z podstawieniem argumentów (String.Format).</summary>
    public string Format(string key, params object?[] args) => string.Format(this[key], args);

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Zgłaszane po zmianie języka — pozwala widokom przebudować listy zależne od języka.</summary>
    public event EventHandler? LanguageChanged;

    public void SetLanguage(string? code)
    {
        var normalized = string.IsNullOrWhiteSpace(code) ? "pl" : code;
        if (normalized == CurrentLanguage)
            return;

        _strings = Translations.For(normalized);
        CurrentLanguage = normalized;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
