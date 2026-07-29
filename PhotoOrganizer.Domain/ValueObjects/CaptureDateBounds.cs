namespace PhotoOrganizer.Domain.ValueObjects;

/// <summary>
/// Okno wiarygodności daty wykonania. Fotografie katalogowane są od 1950 roku — wcześniejsze daty
/// to niemal zawsze śmieci z metadanych (np. wyzerowane pole „creation time" w nagłówku QuickTime/MP4
/// daje epokę 1904-01-01, a puste znaczniki FILETIME dają 1601-01-01). Daty z przyszłości
/// (poza 1-dniowym marginesem na strefy czasowe) również odrzucamy.
/// Niewiarygodna data nie przerywa działania — łańcuch fallback przechodzi do kolejnego źródła.
/// </summary>
public static class CaptureDateBounds
{
    /// <summary>Najwcześniejsza akceptowana data wykonania.</summary>
    public static readonly DateOnly Minimum = new(1950, 1, 1);

    /// <summary>Czy data mieści się w oknie [1950-01-01, dziś+1]?</summary>
    public static bool IsPlausible(DateOnly date, DateOnly today) =>
        date >= Minimum && date <= today.AddDays(1);
}
