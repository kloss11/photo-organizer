using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace PhotoOrganizer.Presentation.Localization;

/// <summary>
/// Rozszerzenie znaczników do tłumaczeń: <c>{loc:Tr Klucz}</c>.
/// Wiąże się z NORMALNĄ właściwością <see cref="Localizer.CurrentLanguage"/> (która poprawnie zgłasza
/// zmianę), a konwerter zamienia klucz na tłumaczenie. Dzięki temu tekst odświeża się po zmianie języka
/// bez restartu (binding do indeksera nie jest obserwowany przez Avalonię i nie działałby).
/// </summary>
public sealed class TrExtension : MarkupExtension
{
    public TrExtension() { }

    public TrExtension(string key) => Key = key;

    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new Binding(nameof(Localizer.CurrentLanguage))
        {
            Source = Localizer.Instance,
            Mode = BindingMode.OneWay,
            Converter = TrConverter.Instance,
            ConverterParameter = Key
        };
}

/// <summary>Zamienia klucz (w parametrze) na tłumaczenie w bieżącym języku. Wartość wejściowa
/// (kod języka) służy tylko jako wyzwalacz ponownej ewaluacji bindingu.</summary>
public sealed class TrConverter : IValueConverter
{
    public static readonly TrConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Localizer.Instance[parameter as string ?? string.Empty];

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
