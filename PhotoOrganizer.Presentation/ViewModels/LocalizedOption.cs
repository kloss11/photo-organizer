using System.ComponentModel;
using PhotoOrganizer.Presentation.Localization;

namespace PhotoOrganizer.Presentation.ViewModels;

/// <summary>
/// Opcja listy rozwijanej z tłumaczonym tekstem. <see cref="Value"/> to wartość enuma;
/// <see cref="Display"/> czyta tłumaczenie po kluczu. <see cref="Refresh"/> odświeża tekst po zmianie języka
/// (te same instancje pozostają zaznaczone).
/// </summary>
public sealed class LocalizedOption : INotifyPropertyChanged
{
    private readonly string _key;

    public LocalizedOption(object value, string key)
    {
        Value = value;
        _key = key;
    }

    public object Value { get; }

    public string Display => Localizer.Instance[_key];

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Display)));
}

/// <summary>Pozycja wyboru języka (kod + nazwa własna).</summary>
public sealed record LanguageChoice(string Code, string Name);
