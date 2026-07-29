namespace PhotoOrganizer.Domain.Model;

/// <summary>Poziom zagnieżdżenia folderów budowanych z daty wykonania.</summary>
public enum DateGranularity
{
    Year,
    YearMonth,
    YearMonthDay
}

/// <summary>Rodzaj pliku multimedialnego.</summary>
public enum MediaKind
{
    Image,
    Video
}

/// <summary>Skąd pochodzi ustalona data wykonania (kolejność = priorytet w łańcuchu fallback).</summary>
public enum CaptureDateSource
{
    ExifOriginal,
    ExifDigitized,
    QuickTimeCreation,
    FileLastWrite,
    FileCreation,
    FileName,
    Unknown
}

/// <summary>Co robić z plikami, dla których nie udało się ustalić daty.</summary>
public enum UndatedPolicy
{
    MoveToFolder,
    Skip
}

/// <summary>Zachowanie przy kolizji nazwy w folderze docelowym (wybór użytkownika).</summary>
public enum CollisionPolicy
{
    Skip,
    Overwrite
}

/// <summary>Zakres skanowania folderu roboczego (wybór użytkownika).</summary>
public enum ScanScope
{
    Recursive,
    TopLevelOnly
}

/// <summary>Czy usuwać opustoszałe foldery źródłowe po przeniesieniu (wybór użytkownika).</summary>
public enum EmptyFolderCleanup
{
    Remove,
    Keep
}

/// <summary>Zaplanowana akcja dla pojedynczego pliku w podglądzie.</summary>
public enum MoveDisposition
{
    WillMove,
    WillOverwrite,
    SkipCollision,
    SkipUndated,
    SkipOnlineOnly,
    SkipSymlink,
    AlreadyInPlace
}

/// <summary>Rzeczywisty wynik operacji na pliku po wykonaniu.</summary>
public enum MoveOutcome
{
    Moved,
    Skipped,
    Failed
}
