using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;

namespace PhotoOrganizer.Application.Abstractions;

/// <summary>
/// Orkiestrator przypadków użycia: podgląd (bez zapisów) → zastosuj → cofnij.
/// Fasada, z którą rozmawia warstwa prezentacji.
/// </summary>
public interface IPhotoOrganizer
{
    Task<OrganizePlan> PreviewAsync(
        FilePath workingArea,
        OrganizeSettings settings,
        IProgress<ScanProgress>? progress = null,
        CancellationToken ct = default);

    Task<OrganizeRun> ApplyAsync(
        OrganizePlan plan,
        IProgress<MoveProgress>? progress = null,
        CancellationToken ct = default);

    Task<OrganizeRun> UndoAsync(
        UndoLog log,
        IProgress<MoveProgress>? progress = null,
        CancellationToken ct = default);
}
