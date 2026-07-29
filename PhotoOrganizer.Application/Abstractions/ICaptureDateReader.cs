using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Application.Abstractions;

/// <summary>
/// Ustala datę wykonania z łańcuchem fallback (EXIF Original → Digitized → QuickTime →
/// data zapisu pliku → data utworzenia pliku → data z nazwy pliku). Kandydaci spoza okna
/// wiarygodności (1950…dziś) są odrzucani na rzecz kolejnego źródła.
/// Implementacja NIGDY nie rzuca — uszkodzone/niepełne metadane traktuje jako „brak daty".
/// </summary>
public interface ICaptureDateReader
{
    /// <param name="allowMetadataRead">
    /// Gdy <c>false</c> (pliki „tylko online"), pomija odczyt metadanych, aby nie wymusić pobrania z chmury —
    /// data wyłącznie z sygnatury czasowej pliku.
    /// </param>
    Task<CaptureDate> ReadAsync(FilePath path, MediaKind kind, bool allowMetadataRead, CancellationToken ct = default);
}
