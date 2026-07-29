using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Platform.Abstractions;

/// <summary>Współrzędne ekranowe kliknięcia (piksele).</summary>
public readonly record struct ScreenPoint(int X, int Y);

/// <summary>Dostępność globalnego gestu na danej platformie/sesji.</summary>
public enum GestureAvailability
{
    Available,
    NeedsPermission,
    Unsupported
}

/// <summary>Wynik próby odczytu folderu spod kursora w menedżerze plików.</summary>
public enum LocationReadStatus
{
    Success,
    NotAFileManager,
    Desktop,
    NoWindowAtPoint,
    PermissionDenied,
    FileManagerNotScriptable,
    Error
}

/// <summary>Rezultat odczytu ścieżki folderu (status + ewentualna ścieżka + diagnostyka).</summary>
public readonly record struct FolderLocationResult(
    LocationReadStatus Status,
    string? Path,
    string? WindowClassOrApp = null,
    string? Diagnostic = null)
{
    public static FolderLocationResult Ok(string path, string? window = null) =>
        new(LocationReadStatus.Success, path, window);

    public static FolderLocationResult Fail(LocationReadStatus status, string? diagnostic = null) =>
        new(status, null, null, diagnostic);
}

/// <summary>Sposoby wskazania folderu roboczego oferowane przez daną platformę.</summary>
[Flags]
public enum WorkingAreaCapabilities
{
    None = 0,
    ManualPicker = 1,
    DragDrop = 2,
    GlobalGesture = 4
}

/// <summary>Uprawnienia systemowe wymagane przez funkcje natywne.</summary>
public enum PlatformPermission
{
    Accessibility,
    AutomationFinder
}

public enum PermissionState
{
    Granted,
    Denied,
    NotDetermined,
    NotApplicable
}

public sealed class GestureTriggeredEventArgs(ScreenPoint point) : EventArgs
{
    public ScreenPoint Point { get; } = point;
}

public sealed class GestureFaultEventArgs(string message, GestureAvailability availability) : EventArgs
{
    public string Message { get; } = message;
    public GestureAvailability Availability { get; } = availability;
}

public sealed class WorkingAreaSelectedEventArgs(FilePath path) : EventArgs
{
    public FilePath Path { get; } = path;
}

public sealed class WorkingAreaSelectionFailedEventArgs(LocationReadStatus status, string message) : EventArgs
{
    public LocationReadStatus Status { get; } = status;
    public string Message { get; } = message;
}
