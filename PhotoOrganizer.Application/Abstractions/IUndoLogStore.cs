using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Application.Abstractions;

/// <summary>Trwałe przechowywanie logów cofania (JSONL w <c>.photoorganizer/undo</c> obok danych).</summary>
public interface IUndoLogStore
{
    Task<IUndoSession> BeginAsync(FilePath workingArea, Guid runId, CancellationToken ct = default);

    /// <summary>Najświeższy zatwierdzony log operacji dla danego folderu roboczego (lub <c>null</c>).</summary>
    Task<UndoLog?> LoadLatestAsync(FilePath workingArea, CancellationToken ct = default);

    Task<IReadOnlyList<UndoLog>> LoadAllAsync(FilePath workingArea, CancellationToken ct = default);
}

/// <summary>
/// Sesja zapisu logu cofania w trybie write-ahead. Wpis o przeniesieniu dopisywany jest DOPIERO
/// po udanym ruchu i natychmiast wypłukiwany na dysk (odporność na crash).
/// </summary>
public interface IUndoSession : IAsyncDisposable
{
    Guid RunId { get; }

    Task RecordCreatedFolderAsync(string relativeFolder, CancellationToken ct = default);

    Task RecordMovedAsync(UndoEntry entry, CancellationToken ct = default);

    /// <summary>Oznacza log jako kompletny/zatwierdzony.</summary>
    Task CommitAsync(CancellationToken ct = default);
}
