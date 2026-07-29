using System.Text.Json;
using System.Text.Json.Serialization;
using PhotoOrganizer.Application.Abstractions;
using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;
using PhotoOrganizer.Infrastructure.FileSystem;

namespace PhotoOrganizer.Infrastructure.Undo;

/// <summary>
/// Log cofania zapisywany jako JSONL w <c>&lt;folder roboczy&gt;/.photoorganizer/undo/{RunId}.jsonl</c> —
/// obok danych, więc log „podróżuje" razem z nimi i przeżywa crash. Każda linia to jedno zdarzenie
/// (nagłówek / utworzony folder / przeniesienie / commit).
/// </summary>
public sealed class JsonlUndoLogStore : IUndoLogStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public async Task<IUndoSession> BeginAsync(FilePath workingArea, Guid runId, CancellationToken ct = default)
    {
        var undoDir = GetUndoDirectory(workingArea.Value);
        System.IO.Directory.CreateDirectory(undoDir);
        var logPath = Path.Combine(undoDir, $"{runId}.jsonl");

        var session = new JsonlUndoSession(runId, workingArea, logPath, JsonOptions);
        await session.WriteHeaderAsync(ct).ConfigureAwait(false);
        return session;
    }

    public async Task<UndoLog?> LoadLatestAsync(FilePath workingArea, CancellationToken ct = default)
    {
        var undoDir = GetUndoDirectory(workingArea.Value);
        if (!System.IO.Directory.Exists(undoDir))
            return null;

        var latest = new DirectoryInfo(undoDir)
            .EnumerateFiles("*.jsonl")
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .FirstOrDefault();

        return latest is null ? null : await ParseAsync(latest.FullName, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<UndoLog>> LoadAllAsync(FilePath workingArea, CancellationToken ct = default)
    {
        var undoDir = GetUndoDirectory(workingArea.Value);
        if (!System.IO.Directory.Exists(undoDir))
            return [];

        var logs = new List<UndoLog>();
        foreach (var file in System.IO.Directory.EnumerateFiles(undoDir, "*.jsonl"))
        {
            var log = await ParseAsync(file, ct).ConfigureAwait(false);
            if (log is not null)
                logs.Add(log);
        }

        return logs.OrderByDescending(l => l.CreatedUtc).ToList();
    }

    private static async Task<UndoLog?> ParseAsync(string path, CancellationToken ct)
    {
        Guid runId = default;
        DateTimeOffset createdUtc = default;
        string? workingArea = null;
        var entries = new List<UndoEntry>();
        var folders = new List<string>();

        foreach (var line in await File.ReadAllLinesAsync(path, ct).ConfigureAwait(false))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            UndoLine? record;
            try
            {
                record = JsonSerializer.Deserialize<UndoLine>(line, JsonOptions);
            }
            catch (JsonException)
            {
                continue; // Pomijamy niekompletną/uszkodzoną ostatnią linię (np. po crashu).
            }

            if (record is null)
                continue;

            switch (record.Type)
            {
                case "header":
                    runId = record.RunId ?? default;
                    createdUtc = record.CreatedUtc ?? default;
                    workingArea = record.WorkingArea;
                    break;
                case "folder" when record.RelativeFolder is not null:
                    folders.Add(record.RelativeFolder);
                    break;
                case "move" when record.MovedTo is not null && record.OriginalPath is not null:
                    if (FilePath.TryCreate(record.MovedTo, out var movedTo) &&
                        FilePath.TryCreate(record.OriginalPath, out var original))
                    {
                        entries.Add(new UndoEntry(
                            movedTo, original, record.Size ?? 0, record.LastWriteUtc ?? default, record.Backup));
                    }
                    break;
            }
        }

        if (workingArea is null || !FilePath.TryCreate(workingArea, out var area))
            return null;

        return new UndoLog(runId, createdUtc, area, entries, folders);
    }

    private static string GetUndoDirectory(string workingArea) =>
        Path.Combine(workingArea, FileSystemMediaScanner.MetadataDirectoryName, "undo");

    internal sealed record UndoLine(
        string Type,
        Guid? RunId = null,
        DateTimeOffset? CreatedUtc = null,
        string? WorkingArea = null,
        string? RelativeFolder = null,
        string? MovedTo = null,
        string? OriginalPath = null,
        long? Size = null,
        DateTime? LastWriteUtc = null,
        string? Backup = null);
}
