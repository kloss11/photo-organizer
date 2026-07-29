using PhotoOrganizer.Domain.ValueObjects;
using PhotoOrganizer.Platform.Abstractions;

namespace PhotoOrganizer.Platform.Linux;

// Na Linux globalny gest Esc+klik jest niedostępny (Wayland blokuje globalny input; żaden menedżer
// plików nie udostępnia bieżącego folderu). Aplikacja degraduje do ręcznego wyboru folderu (UI).

#pragma warning disable CS0067 // zdarzenia wymagane przez interfejsy, w trybie fallback nie są podnoszone

/// <summary>Brak globalnego gestu na Linux — no-op zgłaszający <see cref="GestureAvailability.Unsupported"/>.</summary>
public sealed class LinuxNoopGestureService : IGlobalGestureService
{
    public GestureAvailability Availability => GestureAvailability.Unsupported;
    public bool IsArmed => false;
    public event EventHandler<GestureTriggeredEventArgs>? GestureTriggered;
    public event EventHandler<GestureFaultEventArgs>? Faulted;
    public Task StartAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct = default) => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

/// <summary>Odczyt folderu z menedżera plików niedostępny na Linux.</summary>
public sealed class LinuxLocationReader : IFileManagerLocationReader
{
    public bool IsSupported => false;

    public Task<FolderLocationResult> ReadFolderAtAsync(ScreenPoint point, CancellationToken ct = default) =>
        Task.FromResult(FolderLocationResult.Fail(LocationReadStatus.Error,
            "Odczyt folderu z menedżera plików nie jest wspierany na Linux."));
}

/// <summary>Provider dla Linux: wyłącznie ręczny wybór folderu i drag&amp;drop (realizowane w UI).</summary>
public sealed class LinuxWorkingAreaProvider : IWorkingAreaProvider
{
    public WorkingAreaCapabilities Capabilities =>
        WorkingAreaCapabilities.ManualPicker | WorkingAreaCapabilities.DragDrop;

    public event EventHandler<WorkingAreaSelectedEventArgs>? WorkingAreaSelected;
    public event EventHandler<WorkingAreaSelectionFailedEventArgs>? SelectionFailed;

    public Task ArmGestureAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task DisarmGestureAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task<FilePath?> PickManuallyAsync(CancellationToken ct = default) => Task.FromResult<FilePath?>(null);
}

/// <summary>Linux nie wymaga specjalnych uprawnień dla trybu fallback.</summary>
public sealed class LinuxPermissionService : IPlatformPermissionService
{
    public IReadOnlyList<PlatformPermission> Required { get; } = [];
    public Task<PermissionState> CheckAsync(PlatformPermission permission, CancellationToken ct = default) =>
        Task.FromResult(PermissionState.NotApplicable);
    public Task RequestAsync(PlatformPermission permission, CancellationToken ct = default) => Task.CompletedTask;
}

#pragma warning restore CS0067
