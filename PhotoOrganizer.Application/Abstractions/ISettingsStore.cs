using PhotoOrganizer.Domain.Model;

namespace PhotoOrganizer.Application.Abstractions;

/// <summary>Trwałe ustawienia porządkowania (plik JSON w katalogu danych aplikacji).</summary>
public interface ISettingsStore
{
    Task<OrganizeSettings> LoadAsync(CancellationToken ct = default);

    Task SaveAsync(OrganizeSettings settings, CancellationToken ct = default);
}
