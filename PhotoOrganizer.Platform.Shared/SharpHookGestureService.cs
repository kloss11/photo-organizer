using PhotoOrganizer.Platform.Abstractions;
using SharpHook;
using SharpHook.Data;

namespace PhotoOrganizer.Platform.Shared;

/// <summary>
/// Globalny gest Esc+klik oparty o SharpHook (libuiohook) — współdzielony przez Windows i macOS.
/// Jedna instancja na proces (Singleton). ŚWIADOMIE nie pochłania kliknięcia (brak SuppressEvent) —
/// klik trafia też do menedżera plików, co jest bezpieczne (nie blokuje pulpitu) i naturalne.
/// Na macOS wymaga uprawnienia Accessibility (start hooka wtedy zawiedzie → zdarzenie <see cref="Faulted"/>).
/// </summary>
public sealed class SharpHookGestureService : IGlobalGestureService
{
    private const long DebounceMs = 400;

    private readonly object _sync = new();
    private IGlobalHook? _hook;
    private volatile bool _escDown;
    private long _lastFireTicks;

    public GestureAvailability Availability { get; private set; } = GestureAvailability.Available;

    public bool IsArmed { get; private set; }

    public event EventHandler<GestureTriggeredEventArgs>? GestureTriggered;

    public event EventHandler<GestureFaultEventArgs>? Faulted;

    public Task StartAsync(CancellationToken ct = default)
    {
        lock (_sync)
        {
            if (_hook is not null)
            {
                IsArmed = true;
                return Task.CompletedTask;
            }

            var hook = new TaskPoolGlobalHook();
            hook.KeyPressed += OnKeyPressed;
            hook.KeyReleased += OnKeyReleased;
            hook.MousePressed += OnMousePressed;
            _hook = hook;
            IsArmed = true;

            // RunAsync kończy się dopiero po Dispose; obserwujemy błąd startu (np. brak Accessibility na macOS).
            _ = hook.RunAsync().ContinueWith(OnHookStopped, TaskScheduler.Default);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        lock (_sync)
        {
            IsArmed = false;
            _hook?.Dispose();
            _hook = null;
        }

        return Task.CompletedTask;
    }

    private void OnHookStopped(Task task)
    {
        if (!task.IsFaulted)
            return;

        Availability = GestureAvailability.NeedsPermission;
        var message = task.Exception?.GetBaseException().Message ?? "Nie udało się uruchomić globalnego skrótu.";
        Faulted?.Invoke(this, new GestureFaultEventArgs(message, Availability));
    }

    private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        if (e.Data.KeyCode == KeyCode.VcEscape)
            _escDown = true;
    }

    private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        if (e.Data.KeyCode == KeyCode.VcEscape)
            _escDown = false;
    }

    private void OnMousePressed(object? sender, MouseHookEventArgs e)
    {
        if (!IsArmed || !_escDown || e.Data.Button != MouseButton.Button1)
            return;

        var now = Environment.TickCount64;
        if (now - Interlocked.Read(ref _lastFireTicks) < DebounceMs)
            return;
        Interlocked.Exchange(ref _lastFireTicks, now);

        GestureTriggered?.Invoke(this, new GestureTriggeredEventArgs(new ScreenPoint(e.Data.X, e.Data.Y)));
    }

    public async ValueTask DisposeAsync() => await StopAsync().ConfigureAwait(false);
}
