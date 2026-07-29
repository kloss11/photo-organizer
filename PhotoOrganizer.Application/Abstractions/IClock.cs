namespace PhotoOrganizer.Application.Abstractions;

/// <summary>Źródło czasu — wstrzykiwane dla testowalności (wzorzec z ProjectClub).</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
