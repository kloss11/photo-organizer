using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoOrganizer.Application;
using PhotoOrganizer.Infrastructure;
using PhotoOrganizer.Platform.Abstractions;
using PhotoOrganizer.Platform.Linux;
using PhotoOrganizer.Platform.MacOS;
using PhotoOrganizer.Platform.Windows;
using PhotoOrganizer.Presentation;
using PhotoOrganizer.Presentation.Localization;
using PhotoOrganizer.Presentation.ViewModels;
using PhotoOrganizer.Presentation.Views;

namespace PhotoOrganizer.App;

public partial class App : Avalonia.Application
{
    private IServiceProvider? _services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _services = BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = _services.GetRequiredService<MainWindow>();
            mainWindow.Icon = new WindowIcon(
                AssetLoader.Open(new Uri("avares://PhotoOrganizer.App/Assets/logo.ico")));
            desktop.MainWindow = mainWindow;
            WireGlobalGesture(_services);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider BuildServiceProvider()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddPresentation();

        if (OperatingSystem.IsWindows())
            services.AddWindowsPlatform();
        else if (OperatingSystem.IsMacOS())
            services.AddMacPlatform();
        else
            services.AddLinuxPlatform();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Podpina globalny gest Esc+klik do modelu widoku. Zdarzenia gestu przychodzą z wątku w tle —
    /// marshalujemy je na wątek UI Avalonii.
    /// </summary>
    private static void WireGlobalGesture(IServiceProvider services)
    {
        var provider = services.GetService<IWorkingAreaProvider>();
        if (provider is null)
            return;

        var viewModel = services.GetRequiredService<MainViewModel>();

        provider.WorkingAreaSelected += (_, e) =>
            Dispatcher.UIThread.Post(() => viewModel.SetWorkingArea(e.Path.Value));

        provider.SelectionFailed += (_, e) =>
            Dispatcher.UIThread.Post(() =>
                viewModel.StatusMessage = Localizer.Instance.Format("Err_GestureFmt", e.Message));

        _ = provider.ArmGestureAsync();
    }
}
