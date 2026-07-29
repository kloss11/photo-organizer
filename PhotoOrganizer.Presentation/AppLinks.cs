namespace PhotoOrganizer.Presentation;

/// <summary>Stałe linki projektu używane w UI (repozytorium, zgłaszanie błędów, wsparcie).</summary>
public static class AppLinks
{
    public const string Repository = "https://github.com/kloss11/photo-organizer";

    /// <summary>Nowe zgłoszenie błędu (formularz). Aplikacja dokleja &amp;version= i &amp;os=.</summary>
    public const string NewBugReport = "https://github.com/kloss11/photo-organizer/issues/new?template=bug_report.yml";

    // TODO: ustawić po utworzeniu konta Buy Me a Coffee, a następnie dodać przycisk „Wesprzyj" (Btn_Support).
    public const string? Support = null;
}
