using Avalonia;
using System;
using System.Threading.Tasks;
using PhotoOrganizer.Infrastructure.Diagnostics;

namespace PhotoOrganizer.App;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Globalny handler nieobsłużonych wyjątków — zapisuje crash do logu (lokalnie), zanim aplikacja padnie.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            FileAppDiagnostics.Write("UnhandledException", e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            FileAppDiagnostics.Write("UnobservedTaskException", e.Exception);
            e.SetObserved();
        };

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
