using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Domain.Services;

/// <summary>
/// Wyznacza względne segmenty folderu docelowego dla danej daty i ustawień
/// (np. ["2024","03","15"] albo ["Bez daty"]). Zwraca segmenty, a nie sklejoną ścieżkę,
/// aby pozostać niezależnym od separatora systemu plików.
/// </summary>
public interface ITargetPathCalculator
{
    IReadOnlyList<string> ResolveRelativeSegments(CaptureDate date, OrganizeSettings settings);
}
