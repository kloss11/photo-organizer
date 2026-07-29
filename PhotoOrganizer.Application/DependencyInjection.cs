using Microsoft.Extensions.DependencyInjection;
using PhotoOrganizer.Application.Abstractions;
using PhotoOrganizer.Application.Organizing;
using PhotoOrganizer.Domain.Services;

namespace PhotoOrganizer.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Rejestruje serwisy warstwy Application oraz czyste serwisy Domeny (Domena nie zależy od DI,
    /// dlatego rejestrujemy je tutaj — najbliżej, gdzie wolno referować kontener).
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Czyste serwisy domeny (bezstanowe).
        services.AddSingleton<ITargetPathCalculator, TargetPathCalculator>();
        services.AddSingleton<OrganizePlanner>();

        // Orkiestrator przypadków użycia.
        services.AddSingleton<IPhotoOrganizer, PhotoOrganizerService>();

        return services;
    }
}
