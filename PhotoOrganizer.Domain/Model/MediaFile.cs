using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Domain.Model;

/// <summary>
/// Pojedynczy plik multimedialny wykryty w folderze roboczym.
/// <paramref name="BaseName"/> to nazwa bez rozszerzenia (klucz grupowania plików towarzyszących),
/// <paramref name="Extension"/> jest znormalizowane: małe litery, bez kropki (np. "jpg").
/// </summary>
public sealed record MediaFile(
    FilePath Path,
    string FileName,
    string BaseName,
    string Extension,
    MediaKind Kind,
    long SizeBytes,
    CaptureDate CaptureDate,
    bool IsOnlineOnly,
    bool IsSymlink);
