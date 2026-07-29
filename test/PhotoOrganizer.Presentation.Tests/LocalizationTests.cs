using System.Globalization;
using PhotoOrganizer.Presentation.Localization;

namespace PhotoOrganizer.Presentation.Tests;

public sealed class LocalizationTests
{
    [Theory]
    [InlineData("pl")]
    [InlineData("en")]
    [InlineData("de")]
    [InlineData("ru")]
    [InlineData("es")]
    [InlineData("fr")]
    public void Every_language_defines_all_keys_with_nonempty_values(string code)
    {
        var dictionary = Translations.For(code);

        Assert.Equal(Translations.Keys.Count, dictionary.Count);
        Assert.All(Translations.Keys, key =>
            Assert.False(string.IsNullOrWhiteSpace(dictionary[key]), $"Puste tłumaczenie: {code}/{key}"));
    }

    [Fact]
    public void All_six_languages_are_supported()
    {
        Assert.Equal(6, Translations.SupportedCodes.Count);
        Assert.Equal(Translations.SupportedCodes.Count, Translations.Languages.Count);
    }

    [Fact]
    public void Unknown_language_falls_back_to_polish()
    {
        var unknown = Translations.For("xx");
        var polish = Translations.For("pl");
        Assert.Equal(polish["Btn_Apply"], unknown["Btn_Apply"]);
    }

    [Fact]
    public void Localizer_switches_language_at_runtime()
    {
        Localizer.Instance.SetLanguage("en");
        Assert.Equal("Apply", Localizer.Instance["Btn_Apply"]);

        Localizer.Instance.SetLanguage("de");
        Assert.Equal("Anwenden", Localizer.Instance["Btn_Apply"]);

        Localizer.Instance.SetLanguage("fr");
        Assert.Equal("Appliquer", Localizer.Instance["Btn_Apply"]);

        Localizer.Instance.SetLanguage("pl");
        Assert.Equal("Zastosuj", Localizer.Instance["Btn_Apply"]);
    }

    [Fact]
    public void Missing_key_returns_the_key_itself()
    {
        Localizer.Instance.SetLanguage("pl");
        Assert.Equal("NoSuchKey_123", Localizer.Instance["NoSuchKey_123"]);
    }

    [Fact]
    public void TrConverter_reflects_current_language_after_switch()
    {
        // Tak działa binding etykiet {loc:Tr} — konwerter zwraca tłumaczenie dla bieżącego języka.
        Localizer.Instance.SetLanguage("pl");
        Assert.Equal("Zastosuj", TrConverter.Instance.Convert("pl", typeof(string), "Btn_Apply", CultureInfo.InvariantCulture));

        Localizer.Instance.SetLanguage("es");
        Assert.Equal("Aplicar", TrConverter.Instance.Convert("es", typeof(string), "Btn_Apply", CultureInfo.InvariantCulture));

        Localizer.Instance.SetLanguage("ru");
        Assert.Equal("Применить", TrConverter.Instance.Convert("ru", typeof(string), "Btn_Apply", CultureInfo.InvariantCulture));

        Localizer.Instance.SetLanguage("pl");
    }

    [Fact]
    public void SetLanguage_raises_language_changed_and_current_language_notifications()
    {
        Localizer.Instance.SetLanguage("pl");
        var languageChanged = false;
        var currentLanguageNotified = false;

        void OnChanged(object? s, EventArgs e) => languageChanged = true;
        void OnProp(object? s, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Localizer.CurrentLanguage))
                currentLanguageNotified = true;
        }

        Localizer.Instance.LanguageChanged += OnChanged;
        Localizer.Instance.PropertyChanged += OnProp;
        try
        {
            Localizer.Instance.SetLanguage("de");
            Assert.True(languageChanged);
            Assert.True(currentLanguageNotified);
        }
        finally
        {
            Localizer.Instance.LanguageChanged -= OnChanged;
            Localizer.Instance.PropertyChanged -= OnProp;
            Localizer.Instance.SetLanguage("pl");
        }
    }
}
