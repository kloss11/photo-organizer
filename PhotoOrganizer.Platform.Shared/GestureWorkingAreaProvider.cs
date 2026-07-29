using PhotoOrganizer.Domain.ValueObjects;
using PhotoOrganizer.Platform.Abstractions;

namespace PhotoOrganizer.Platform.Shared;

/// <summary>
/// Provider oparty o globalny gest — wspólny dla Windows i macOS. Po geście czyta ścieżkę folderu
/// właściwym dla platformy readerem i podnosi <see cref="WorkingAreaSelected"/> (lub
/// <see cref="SelectionFailed"/>). Zdarzenia mogą pochodzić z wątku w tle — subskrybent (App)
/// marshaluje je na wątek UI.
/// </summary>
public sealed class GestureWorkingAreaProvider : IWorkingAreaProvider
{
    private readonly IGlobalGestureService _gesture;
    private readonly IFileManagerLocationReader _reader;

    public GestureWorkingAreaProvider(IGlobalGestureService gesture, IFileManagerLocationReader reader)
    {
        _gesture = gesture;
        _reader = reader;
        _gesture.GestureTriggered += OnGestureTriggered;
    }

    public WorkingAreaCapabilities Capabilities =>
        WorkingAreaCapabilities.GlobalGesture | WorkingAreaCapabilities.ManualPicker;

    public event EventHandler<WorkingAreaSelectedEventArgs>? WorkingAreaSelected;

    public event EventHandler<WorkingAreaSelectionFailedEventArgs>? SelectionFailed;

    public Task ArmGestureAsync(CancellationToken ct = default) => _gesture.StartAsync(ct);

    public Task DisarmGestureAsync(CancellationToken ct = default) => _gesture.StopAsync(ct);

    // Ręczny wybór realizuje warstwa UI (systemowy picker wymaga uchwytu okna).
    public Task<FilePath?> PickManuallyAsync(CancellationToken ct = default) => Task.FromResult<FilePath?>(null);

    private async void OnGestureTriggered(object? sender, GestureTriggeredEventArgs e)
    {
        var result = await _reader.ReadFolderAtAsync(e.Point).ConfigureAwait(false);

        if (result.Status == LocationReadStatus.Success && FilePath.TryCreate(result.Path, out var path))
            WorkingAreaSelected?.Invoke(this, new WorkingAreaSelectedEventArgs(path));
        else
            SelectionFailed?.Invoke(this,
                new WorkingAreaSelectionFailedEventArgs(result.Status, result.Diagnostic ?? result.Status.ToString()));
    }
}
