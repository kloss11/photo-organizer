namespace PhotoOrganizer.Platform.Abstractions;

/// <summary>
/// Globalny gest Esc+klik (SharpHook). JEDNA instancja na proces (Singleton). Decyzja o pochłonięciu
/// zdarzenia myszy zapada synchronicznie w callbacku; sam odczyt folderu następuje później,
/// zmarshalowany na wątek UI. Uzbrajany tylko po wejściu w tryb „wskaż folder".
/// </summary>
public interface IGlobalGestureService : IAsyncDisposable
{
    GestureAvailability Availability { get; }

    bool IsArmed { get; }

    event EventHandler<GestureTriggeredEventArgs>? GestureTriggered;

    event EventHandler<GestureFaultEventArgs>? Faulted;

    Task StartAsync(CancellationToken ct = default);

    Task StopAsync(CancellationToken ct = default);
}
