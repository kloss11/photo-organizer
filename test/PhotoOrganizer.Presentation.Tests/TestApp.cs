using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using PhotoOrganizer.Presentation.Tests;

[assembly: AvaloniaTestApplication(typeof(TestApp))]
// Localizer to singleton — serializujemy testy w tym projekcie, by uniknąć wyścigów na wspólnym stanie.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]

namespace PhotoOrganizer.Presentation.Tests;

public sealed class TestApp : Avalonia.Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
