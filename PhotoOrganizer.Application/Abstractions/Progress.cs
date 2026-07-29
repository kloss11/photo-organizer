namespace PhotoOrganizer.Application.Abstractions;

/// <summary>Postęp skanowania folderu roboczego.</summary>
public readonly record struct ScanProgress(int FilesSeen, int MediaFound, string? CurrentPath);

/// <summary>Postęp wykonywania/cofania operacji przenoszenia.</summary>
public readonly record struct MoveProgress(int Processed, int Total, string? CurrentFile);
