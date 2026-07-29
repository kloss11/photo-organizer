using Microsoft.Extensions.DependencyInjection;
using PhotoOrganizer.Presentation.ViewModels;
using PhotoOrganizer.Presentation.Views;

namespace PhotoOrganizer.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddSingleton(sp => new MainWindow
        {
            DataContext = sp.GetRequiredService<MainViewModel>()
        });

        return services;
    }
}
