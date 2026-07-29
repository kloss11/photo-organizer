using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Platform.Abstractions;

/// <summary>
/// Jednolity sposób wskazania folderu roboczego niezależnie od platformy. Prezentacja rozmawia tylko
/// z tym portem; konkretny mechanizm (gest Esc+klik lub ręczny wybór/drag&drop) dobiera adapter OS.
/// </summary>
public interface IWorkingAreaProvider
{
    WorkingAreaCapabilities Capabilities { get; }

    event EventHandler<WorkingAreaSelectedEventArgs>? WorkingAreaSelected;

    event EventHandler<WorkingAreaSelectionFailedEventArgs>? SelectionFailed;

    Task ArmGestureAsync(CancellationToken ct = default);

    Task DisarmGestureAsync(CancellationToken ct = default);

    Task<FilePath?> PickManuallyAsync(CancellationToken ct = default);
}
