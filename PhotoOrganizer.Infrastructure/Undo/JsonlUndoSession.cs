using System.Text;
using System.Text.Json;
using PhotoOrganizer.Application.Abstractions;
using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Infrastructure.Undo;

/// <summary>
/// Sesja zapisu logu cofania. Każda linia jest wypłukiwana na dysk (<c>Flush(flushToDisk: true)</c>)
/// natychmiast po zapisie — wpis o przeniesieniu powstaje DOPIERO po udanym ruchu, więc log nigdy
/// nie twierdzi, że plik jest w miejscu, którego nie osiągnął.
/// </summary>
internal sealed class JsonlUndoSession : IUndoSession
{
    private readonly FilePath _workingArea;
    private readonly string _logPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly FileStream _stream;
    private readonly StreamWriter _writer;

    public JsonlUndoSession(Guid runId, FilePath workingArea, string logPath, JsonSerializerOptions jsonOptions)
    {
        RunId = runId;
        _workingArea = workingArea;
        _logPath = logPath;
        _jsonOptions = jsonOptions;
        _stream = new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        _writer = new StreamWriter(_stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    public Guid RunId { get; }

    public Task WriteHeaderAsync(CancellationToken ct) =>
        WriteLineAsync(new JsonlUndoLogStore.UndoLine(
            "header", RunId, DateTimeOffset.UtcNow, _workingArea.Value), ct);

    public Task RecordCreatedFolderAsync(string relativeFolder, CancellationToken ct = default) =>
        WriteLineAsync(new JsonlUndoLogStore.UndoLine("folder", RelativeFolder: relativeFolder), ct);

    public Task RecordMovedAsync(UndoEntry entry, CancellationToken ct = default) =>
        WriteLineAsync(new JsonlUndoLogStore.UndoLine(
            "move",
            MovedTo: entry.MovedTo.Value,
            OriginalPath: entry.OriginalPath.Value,
            Size: entry.Size,
            LastWriteUtc: entry.LastWriteUtc,
            Backup: entry.BackupOfOverwritten), ct);

    public Task CommitAsync(CancellationToken ct = default) =>
        WriteLineAsync(new JsonlUndoLogStore.UndoLine("commit"), ct);

    private async Task WriteLineAsync(JsonlUndoLogStore.UndoLine line, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(line, _jsonOptions);
        await _writer.WriteLineAsync(json.AsMemory(), ct).ConfigureAwait(false);
        await _writer.FlushAsync(ct).ConfigureAwait(false);
        _stream.Flush(flushToDisk: true); // Trwałość: wymuś zapis na nośnik (odporność na crash).
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _writer.FlushAsync().ConfigureAwait(false);
        }
        catch
        {
            // ignorujemy błędy przy zamknięciu — dane już wypłukane po każdej linii
        }

        await _writer.DisposeAsync().ConfigureAwait(false);
        await _stream.DisposeAsync().ConfigureAwait(false);
    }
}
