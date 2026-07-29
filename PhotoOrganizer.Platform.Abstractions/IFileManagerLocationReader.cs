namespace PhotoOrganizer.Platform.Abstractions;

/// <summary>
/// Odczytuje ścieżkę folderu z okna menedżera plików. Windows: okno pod kursorem (Shell COM).
/// macOS: przednie okno Findera (AppleScript). Linux: nieobsługiwane (<see cref="IsSupported"/> = false).
/// </summary>
public interface IFileManagerLocationReader
{
    bool IsSupported { get; }

    Task<FolderLocationResult> ReadFolderAtAsync(ScreenPoint point, CancellationToken ct = default);
}
