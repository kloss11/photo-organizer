namespace PhotoOrganizer.Presentation.Localization;

/// <summary>
/// Tłumaczenia UI. Każdy wpis to tablica w stałej kolejności języków: [pl, en, de, ru, es, fr].
/// Nieznany kod języka → polski (indeks 0).
/// </summary>
public static class Translations
{
    private static readonly string[] Codes = ["pl", "en", "de", "ru", "es", "fr"];

    /// <summary>Języki do wyboru (kod + nazwa własna).</summary>
    public static IReadOnlyList<(string Code, string Name)> Languages { get; } =
    [
        ("pl", "Polski"),
        ("en", "English"),
        ("de", "Deutsch"),
        ("ru", "Русский"),
        ("es", "Español"),
        ("fr", "Français")
    ];

    private static readonly Dictionary<string, string[]> Entries = new()
    {
        //                          pl,                                              en,                                          de,                                              ru,                                              es,                                          fr
        ["App_Title"] = ["PhotoOrganizer — porządkowanie zdjęć i wideo", "PhotoOrganizer — organize photos and videos", "PhotoOrganizer — Fotos und Videos ordnen", "PhotoOrganizer — упорядочивание фото и видео", "PhotoOrganizer — organiza fotos y vídeos", "PhotoOrganizer — organiser photos et vidéos"],
        ["Label_Language"] = ["Język", "Language", "Sprache", "Язык", "Idioma", "Langue"],

        ["Section_WorkingArea"] = ["Folder roboczy", "Working folder", "Arbeitsordner", "Рабочая папка", "Carpeta de trabajo", "Dossier de travail"],
        ["Btn_PickFolder"] = ["Wybierz folder…", "Choose folder…", "Ordner wählen…", "Выбрать папку…", "Elegir carpeta…", "Choisir un dossier…"],
        ["WorkingArea_None"] = ["(nie wybrano)", "(none selected)", "(nicht ausgewählt)", "(не выбрано)", "(sin seleccionar)", "(non sélectionné)"],
        ["Hint_Gesture"] = [
            "Windows: przytrzymaj Esc i kliknij lewym w oknie Eksploratora, aby wskazać folder.",
            "Windows: hold Esc and left-click in a File Explorer window to pick a folder.",
            "Windows: Esc gedrückt halten und mit links in ein Explorer-Fenster klicken, um einen Ordner zu wählen.",
            "Windows: удерживайте Esc и щёлкните левой кнопкой в окне Проводника, чтобы выбрать папку.",
            "Windows: mantén Esc y haz clic izquierdo en una ventana del Explorador para elegir una carpeta.",
            "Windows : maintenez Échap et cliquez gauche dans une fenêtre de l'Explorateur pour choisir un dossier."],

        ["Section_Settings"] = ["Ustawienia", "Settings", "Einstellungen", "Настройки", "Ajustes", "Paramètres"],
        ["Label_Granularity"] = ["Granularność", "Granularity", "Granularität", "Детализация", "Granularidad", "Granularité"],
        ["Label_Collision"] = ["Kolizje nazw", "Name collisions", "Namenskonflikte", "Конфликты имён", "Conflictos de nombre", "Conflits de noms"],
        ["Label_ScanScope"] = ["Zakres skanu", "Scan scope", "Scan-Umfang", "Область сканирования", "Alcance del escaneo", "Portée de l'analyse"],
        ["Label_Undated"] = ["Pliki bez daty", "Undated files", "Dateien ohne Datum", "Файлы без даты", "Archivos sin fecha", "Fichiers sans date"],
        ["Label_ZeroPad"] = ["Dopełniaj zerami (03)", "Zero-pad (03)", "Mit Null auffüllen (03)", "Дополнять нулём (03)", "Rellenar con cero (03)", "Compléter par zéro (03)"],

        ["Btn_Preview"] = ["Podgląd (dry-run)", "Preview (dry-run)", "Vorschau (Probelauf)", "Предпросмотр (без изменений)", "Vista previa (simulación)", "Aperçu (simulation)"],
        ["Btn_Apply"] = ["Zastosuj", "Apply", "Anwenden", "Применить", "Aplicar", "Appliquer"],
        ["Btn_Undo"] = ["Cofnij ostatnią operację", "Undo last operation", "Letzte Aktion rückgängig", "Отменить последнюю операцию", "Deshacer última operación", "Annuler la dernière opération"],

        ["Col_File"] = ["Plik", "File", "Datei", "Файл", "Archivo", "Fichier"],
        ["Col_Date"] = ["Data", "Date", "Datum", "Дата", "Fecha", "Date"],
        ["Col_Source"] = ["Źródło", "Source", "Quelle", "Источник", "Origen", "Source"],
        ["Col_Action"] = ["Akcja", "Action", "Aktion", "Действие", "Acción", "Action"],
        ["Col_Target"] = ["Folder docelowy", "Target folder", "Zielordner", "Целевая папка", "Carpeta destino", "Dossier cible"],

        ["Status_Initial"] = ["Wskaż folder roboczy, aby rozpocząć.", "Select a working folder to begin.", "Wähle einen Arbeitsordner, um zu beginnen.", "Выберите рабочую папку, чтобы начать.", "Selecciona una carpeta de trabajo para empezar.", "Sélectionnez un dossier de travail pour commencer."],
        ["Status_Scanning"] = ["Skanowanie i planowanie…", "Scanning and planning…", "Scannen und Planen…", "Сканирование и планирование…", "Escaneando y planificando…", "Analyse et planification…"],
        ["Status_PreviewReady"] = ["Podgląd gotowy. Sprawdź plan i kliknij „Zastosuj”.", "Preview ready. Review the plan and click \"Apply\".", "Vorschau bereit. Prüfe den Plan und klicke auf „Anwenden“.", "Предпросмотр готов. Проверьте план и нажмите «Применить».", "Vista previa lista. Revisa el plan y pulsa «Aplicar».", "Aperçu prêt. Vérifiez le plan et cliquez sur « Appliquer »."],
        ["Status_PreviewEmpty"] = ["Podgląd gotowy — brak plików do przeniesienia.", "Preview ready — nothing to move.", "Vorschau bereit — nichts zu verschieben.", "Предпросмотр готов — нечего перемещать.", "Vista previa lista — nada que mover.", "Aperçu prêt — rien à déplacer."],
        ["Status_Applying"] = ["Przenoszenie plików…", "Moving files…", "Dateien werden verschoben…", "Перемещение файлов…", "Moviendo archivos…", "Déplacement des fichiers…"],
        ["Status_Undoing"] = ["Cofanie ostatniej operacji…", "Undoing last operation…", "Letzte Aktion wird rückgängig gemacht…", "Отмена последней операции…", "Deshaciendo la última operación…", "Annulation de la dernière opération…"],

        ["WorkingArea_Set"] = ["Folder roboczy: {0}. Kliknij „Podgląd”, aby zaplanować.", "Working folder: {0}. Click \"Preview\" to plan.", "Arbeitsordner: {0}. Klicke auf „Vorschau“, um zu planen.", "Рабочая папка: {0}. Нажмите «Предпросмотр», чтобы спланировать.", "Carpeta de trabajo: {0}. Pulsa «Vista previa» para planificar.", "Dossier de travail : {0}. Cliquez sur « Aperçu » pour planifier."],
        ["Status_AppliedFmt"] = ["Gotowe. Przeniesiono: {0} • Pominięto: {1} • Błędy: {2}.", "Done. Moved: {0} • Skipped: {1} • Errors: {2}.", "Fertig. Verschoben: {0} • Übersprungen: {1} • Fehler: {2}.", "Готово. Перемещено: {0} • Пропущено: {1} • Ошибок: {2}.", "Listo. Movidos: {0} • Omitidos: {1} • Errores: {2}.", "Terminé. Déplacés : {0} • Ignorés : {1} • Erreurs : {2}."],
        ["Status_UndoAvailable"] = [" Możesz cofnąć tę operację.", " You can undo this operation.", " Du kannst diese Aktion rückgängig machen.", " Эту операцию можно отменить.", " Puedes deshacer esta operación.", " Vous pouvez annuler cette opération."],
        ["Status_UndoneFmt"] = ["Cofnięto. Przywrócono: {0} • Pominięto: {1}.", "Undone. Restored: {0} • Skipped: {1}.", "Rückgängig. Wiederhergestellt: {0} • Übersprungen: {1}.", "Отменено. Восстановлено: {0} • Пропущено: {1}.", "Deshecho. Restaurados: {0} • Omitidos: {1}.", "Annulé. Restaurés : {0} • Ignorés : {1}."],
        ["Err_PreviewFmt"] = ["Błąd podglądu: {0}", "Preview error: {0}", "Vorschaufehler: {0}", "Ошибка предпросмотра: {0}", "Error de vista previa: {0}", "Erreur d'aperçu : {0}"],
        ["Err_ApplyFmt"] = ["Błąd wykonania: {0}", "Apply error: {0}", "Fehler beim Anwenden: {0}", "Ошибка выполнения: {0}", "Error al aplicar: {0}", "Erreur d'application : {0}"],
        ["Err_UndoFmt"] = ["Błąd cofania: {0}", "Undo error: {0}", "Fehler beim Rückgängigmachen: {0}", "Ошибка отмены: {0}", "Error al deshacer: {0}", "Erreur d'annulation : {0}"],
        ["Err_GestureFmt"] = ["Nie udało się odczytać folderu z gestu: {0}", "Could not read folder from gesture: {0}", "Ordner konnte nicht aus der Geste gelesen werden: {0}", "Не удалось получить папку из жеста: {0}", "No se pudo leer la carpeta del gesto: {0}", "Impossible de lire le dossier depuis le geste : {0}"],

        ["Sum_ToMove"] = ["Do przeniesienia", "To move", "Zu verschieben", "К перемещению", "Para mover", "À déplacer"],
        ["Sum_Overwrite"] = ["Nadpisań", "Overwrites", "Überschreiben", "Перезаписей", "Sobrescrituras", "Écrasements"],
        ["Sum_InPlace"] = ["Już na miejscu", "Already in place", "Bereits am Platz", "Уже на месте", "Ya en su sitio", "Déjà en place"],
        ["Sum_Collision"] = ["Kolizje", "Collisions", "Konflikte", "Конфликты", "Conflictos", "Conflits"],
        ["Sum_Undated"] = ["Bez daty", "Undated", "Ohne Datum", "Без даты", "Sin fecha", "Sans date"],
        ["Sum_Online"] = ["Tylko online", "Online-only", "Nur online", "Только онлайн", "Solo en línea", "En ligne seulement"],

        ["Granularity_Year"] = ["Rok", "Year", "Jahr", "Год", "Año", "Année"],
        ["Granularity_YearMonth"] = ["Rok i miesiąc", "Year and month", "Jahr und Monat", "Год и месяц", "Año y mes", "Année et mois"],
        ["Granularity_YearMonthDay"] = ["Rok, miesiąc i dzień", "Year, month and day", "Jahr, Monat und Tag", "Год, месяц и день", "Año, mes y día", "Année, mois et jour"],

        ["Collision_Skip"] = ["Pomiń", "Skip", "Überspringen", "Пропустить", "Omitir", "Ignorer"],
        ["Collision_Overwrite"] = ["Nadpisz", "Overwrite", "Überschreiben", "Перезаписать", "Sobrescribir", "Écraser"],

        ["ScanScope_Recursive"] = ["Rekurencyjnie", "Recursive", "Rekursiv", "Рекурсивно", "Recursivo", "Récursif"],
        ["ScanScope_TopLevelOnly"] = ["Tylko najwyższy poziom", "Top level only", "Nur oberste Ebene", "Только верхний уровень", "Solo nivel superior", "Niveau supérieur uniquement"],

        ["Undated_MoveToFolder"] = ["Przenieś do „Bez daty”", "Move to \"Undated\"", "In „Ohne Datum“ verschieben", "В папку «Без даты»", "Mover a «Sin fecha»", "Déplacer vers « Sans date »"],
        ["Undated_Skip"] = ["Pomiń", "Skip", "Überspringen", "Пропустить", "Omitir", "Ignorer"],

        ["Src_ExifOriginal"] = ["EXIF (oryginał)", "EXIF (original)", "EXIF (Original)", "EXIF (оригинал)", "EXIF (original)", "EXIF (original)"],
        ["Src_ExifDigitized"] = ["EXIF (digitalizacja)", "EXIF (digitized)", "EXIF (digitalisiert)", "EXIF (оцифровка)", "EXIF (digitalizado)", "EXIF (numérisé)"],
        ["Src_QuickTime"] = ["wideo (QuickTime)", "video (QuickTime)", "Video (QuickTime)", "видео (QuickTime)", "vídeo (QuickTime)", "vidéo (QuickTime)"],
        ["Src_FileDate"] = ["data pliku", "file date", "Dateidatum", "дата файла", "fecha del archivo", "date du fichier"],
        ["Src_FileCreation"] = ["data utworzenia pliku", "file creation date", "Datei-Erstellungsdatum", "дата создания файла", "fecha de creación del archivo", "date de création du fichier"],
        ["Src_FileName"] = ["data z nazwy pliku", "date from file name", "Datum aus dem Dateinamen", "дата из имени файла", "fecha del nombre del archivo", "date d'après le nom du fichier"],
        ["Src_None"] = ["brak", "none", "keine", "нет", "ninguna", "aucune"],

        ["Act_WillMove"] = ["Przeniesienie", "Move", "Verschieben", "Перемещение", "Mover", "Déplacement"],
        ["Act_WillOverwrite"] = ["Nadpisanie", "Overwrite", "Überschreiben", "Перезапись", "Sobrescribir", "Écrasement"],
        ["Act_SkipCollision"] = ["Pominięcie (kolizja)", "Skip (collision)", "Übersprungen (Konflikt)", "Пропуск (конфликт)", "Omitir (conflicto)", "Ignoré (conflit)"],
        ["Act_SkipUndated"] = ["Pominięcie (brak daty)", "Skip (no date)", "Übersprungen (kein Datum)", "Пропуск (нет даты)", "Omitir (sin fecha)", "Ignoré (sans date)"],
        ["Act_SkipOnlineOnly"] = ["Pominięcie (tylko online)", "Skip (online-only)", "Übersprungen (nur online)", "Пропуск (только онлайн)", "Omitir (solo en línea)", "Ignoré (en ligne seulement)"],
        ["Act_SkipSymlink"] = ["Pominięcie (symlink)", "Skip (symlink)", "Übersprungen (Symlink)", "Пропуск (симлинк)", "Omitir (symlink)", "Ignoré (lien symbolique)"],
        ["Act_AlreadyInPlace"] = ["Już na miejscu", "Already in place", "Bereits am Platz", "Уже на месте", "Ya en su sitio", "Déjà en place"],

        // Ekran „O programie" / zgłaszanie problemów / wsparcie
        ["Btn_About"] = ["O programie", "About", "Über", "О программе", "Acerca de", "À propos"],
        ["About_Title"] = ["O programie", "About", "Über PhotoOrganizer", "О программе", "Acerca de", "À propos"],
        ["About_VersionFmt"] = ["Wersja {0}", "Version {0}", "Version {0}", "Версия {0}", "Versión {0}", "Version {0}"],
        ["About_Description"] = [
            "Porządkuje zdjęcia i wideo według daty — bezpiecznie i lokalnie.",
            "Organizes your photos and videos by date — safely and locally.",
            "Ordnet Fotos und Videos nach Datum — sicher und lokal.",
            "Упорядочивает фото и видео по дате — безопасно и локально.",
            "Organiza tus fotos y vídeos por fecha — de forma segura y local.",
            "Organise vos photos et vidéos par date — en toute sécurité et en local."],
        ["Btn_GitHub"] = ["Zobacz na GitHub", "View on GitHub", "Auf GitHub ansehen", "Открыть на GitHub", "Ver en GitHub", "Voir sur GitHub"],
        ["Btn_ReportProblem"] = ["Zgłoś problem", "Report a problem", "Problem melden", "Сообщить о проблеме", "Informar de un problema", "Signaler un problème"],
        ["Btn_OpenLogs"] = ["Otwórz folder logów", "Open logs folder", "Log-Ordner öffnen", "Открыть папку журналов", "Abrir carpeta de registros", "Ouvrir le dossier des journaux"],
        ["About_License"] = [
            "Darmowe i open source · Licencja MIT",
            "Free & open source · MIT License",
            "Kostenlos & Open Source · MIT-Lizenz",
            "Бесплатно и с открытым кодом · Лицензия MIT",
            "Gratis y de código abierto · Licencia MIT",
            "Gratuit et open source · Licence MIT"],
        ["Btn_Support"] = ["Wesprzyj ☕", "Support ☕", "Unterstützen ☕", "Поддержать ☕", "Apoyar ☕", "Soutenir ☕"]
    };

    /// <summary>Wszystkie klucze tłumaczeń (do walidacji kompletności).</summary>
    public static IReadOnlyCollection<string> Keys => Entries.Keys;

    /// <summary>Kody obsługiwanych języków.</summary>
    public static IReadOnlyList<string> SupportedCodes => Codes;

    public static IReadOnlyDictionary<string, string> For(string? code)
    {
        var index = Array.IndexOf(Codes, code ?? "pl");
        if (index < 0)
            index = 0;

        return Entries.ToDictionary(pair => pair.Key, pair => pair.Value[index]);
    }
}
