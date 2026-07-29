using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Application.Abstractions;

/// <summary>
/// Skanuje folder roboczy zgodnie z <see cref="OrganizeSettings.ScanScope"/>, filtruje pliki obsługiwane,
/// grupuje pliki towarzyszące (gdy włączone), wykrywa placeholdery chmury i linki symboliczne
/// oraz ustala datę każdego pliku. Pomija katalog metadanych narzędzia (<c>.photoorganizer</c>).
/// </summary>
public interface IMediaScanner
{
    Task<IReadOnlyList<MediaGroup>> ScanAsync(
        FilePath workingArea,
        OrganizeSettings settings,
        IProgress<ScanProgress>? progress = null,
        CancellationToken ct = default);
}
