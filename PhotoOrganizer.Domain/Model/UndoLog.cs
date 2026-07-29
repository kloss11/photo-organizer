using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Domain.Model;

/// <summary>
/// Zapis pojedynczego przeniesienia umożliwiający cofnięcie. Tożsamość (<paramref name="Size"/> +
/// <paramref name="LastWriteUtc"/>) chroni przed przeniesieniem z powrotem niewłaściwego pliku, gdyby
/// plik w <paramref name="MovedTo"/> został później podmieniony. <paramref name="BackupOfOverwritten"/>
/// wskazuje kopię zapasową pliku nadpisanego (tryb Overwrite), aby nadpisanie było odwracalne.
/// </summary>
public sealed record UndoEntry(
    FilePath MovedTo,
    FilePath OriginalPath,
    long Size,
    DateTime LastWriteUtc,
    string? BackupOfOverwritten = null);

/// <summary>Kompletny log jednej operacji porządkowania (odczytany z pliku JSONL).</summary>
public sealed record UndoLog(
    Guid RunId,
    DateTimeOffset CreatedUtc,
    FilePath WorkingArea,
    IReadOnlyList<UndoEntry> Entries,
    IReadOnlyList<string> CreatedFolders);
