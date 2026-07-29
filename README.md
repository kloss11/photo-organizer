# PhotoOrganizer

Porządkuje zdjęcia i wideo według **daty wykonania**, przenosząc je do folderów `RRRR/MM/DD`
(rok / rok+miesiąc / rok+miesiąc+dzień). Działa na **Windows, macOS i Linux** (Avalonia, .NET 10).
Bezpieczny schemat pracy: **Podgląd (dry-run) → Zastosuj → Cofnij**.

Interfejs dostępny w 6 językach: **Polski, English, Deutsch, Русский, Español, Français**.

---

## Instalacja

Aplikacja jest **self-contained** i **jednoplikowa** — nie wymaga instalowania .NET u użytkownika.
Artefakty budują skrypty z katalogu [`packaging/`](packaging/README.md) (wymagany .NET 10 SDK do budowania).

### Szybkie budowanie jednoplikowe (wszystkie platformy)

Same pliki wykonywalne można zbudować **cross-platform z dowolnego systemu** — jedną pętlą:
```bash
for rid in win-x64 linux-x64 osx-x64 osx-arm64; do
  dotnet publish PhotoOrganizer.App/PhotoOrganizer.App.csproj -c Release -r $rid \
    --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
    -o "bin/publish/$rid"
done
```
Wynik: jeden plik na platformę w `bin/publish/<rid>/` (`PhotoOrganizer.App.exe` dla Windows,
`PhotoOrganizer.App` dla Linux/macOS). Na Linux/macOS po przeniesieniu nadaj bit wykonywalny:
`chmod +x PhotoOrganizer.App`. To „gołe” binaria — pełną integrację z systemem (ikona w bundlu `.app`,
AppImage) dają skrypty per platforma poniżej.

### Windows

Budowanie artefaktu:
```powershell
pwsh packaging/windows/build-windows.ps1
# wynik: bin/publish/win-x64/PhotoOrganizer.App.exe
```

Uruchomienie / instalacja:
1. Skopiuj `PhotoOrganizer.App.exe` w dowolne miejsce (np. `C:\Program Files\PhotoOrganizer\`).
2. Uruchom podwójnym kliknięciem. Nie trzeba nic instalować.
3. Jeśli plik nie jest podpisany cyfrowo, SmartScreen może pokazać ostrzeżenie —
   wybierz **Więcej informacji → Uruchom mimo to**.
4. (Opcjonalnie) utwórz skrót na pulpicie / w menu Start, albo zbuduj instalator
   (NSIS/Inno lub Avalonia **Parcel**).

Ikona aplikacji jest osadzona w `.exe` i widoczna na pasku zadań oraz w pasku tytułu okna.

### macOS

Budowanie bundla (uruchom **na macOS**, wymaga Xcode do podpisu/notaryzacji):
```bash
bash packaging/macos/build-macos.sh osx-arm64   # lub: osx-x64
# wynik: bin/bundle/PhotoOrganizer.app (z ikoną PhotoOrganizer.icns)
```

Instalacja / pierwsze uruchomienie:
1. Przeciągnij `PhotoOrganizer.app` do folderu **Programy** (`/Applications`).
2. Podpis (hardened runtime, **bez** App Sandbox) i notaryzacja — komendy wypisuje skrypt
   po zakończeniu (wymagają konta Apple Developer). Bez podpisu przy pierwszym uruchomieniu
   kliknij aplikację prawym przyciskiem → **Otwórz**, aby ominąć Gatekeeper.
3. Przy pierwszym użyciu nadaj **dwa** uprawnienia (Ustawienia systemowe → Prywatność i bezpieczeństwo):
   - **Dostępność (Accessibility)** — dla globalnego gestu,
   - **Automatyzacja → Finder** — dla odczytu bieżącego folderu z Findera.

### Linux

Budowanie AppDir i AppImage (uruchom **na Linux**):
```bash
bash packaging/linux/build-linux.sh
appimagetool bin/appdir/PhotoOrganizer.AppDir bin/PhotoOrganizer-x86_64.AppImage
```

Uruchomienie:
```bash
chmod +x PhotoOrganizer-x86_64.AppImage
./PhotoOrganizer-x86_64.AppImage
```
- Preferuj **AppImage** (mniej piaskownicy niż Flatpak).
- **Uwaga:** globalny gest „Esc + klik” **nie działa na Linux** (Wayland blokuje globalny input,
  a menedżery plików nie udostępniają bieżącego folderu). Użyj ręcznego wyboru folderu lub przeciągnij
  go do okna. Silnik porządkowania (podgląd → zastosuj → cofnij) działa tak samo jak na pozostałych systemach.

---

## Instrukcja obsługi

1. **Uruchom aplikację.** W prawym górnym rogu możesz zmienić **język** interfejsu.

2. **Wskaż folder roboczy** (sekcja *Folder roboczy*) na jeden z trzech sposobów:
   - przycisk **„Wybierz folder…”**,
   - **przeciągnij** folder z menedżera plików do okna aplikacji,
   - **gest** (tylko Windows): **przytrzymaj `Esc` i kliknij lewym** w oknie Eksploratora — aplikacja
     odczyta ścieżkę aktualnie otwartego folderu. (macOS: analogicznie, po nadaniu uprawnień; Linux: brak gestu.)

3. **Ustaw opcje** (sekcja *Ustawienia*):
   | Opcja | Wartości | Domyślnie | Efekt |
   |---|---|---|---|
   | **Granularność** | Rok / Rok i miesiąc / Rok, miesiąc i dzień | Rok i miesiąc | Głębokość folderów: `2024`, `2024/03`, `2024/03/15` |
   | **Kolizje nazw** | Pomiń / Nadpisz | Pomiń | Co zrobić, gdy w folderze docelowym istnieje już plik o tej nazwie |
   | **Zakres skanu** | Rekurencyjnie / Tylko najwyższy poziom | Rekurencyjnie | Czy wchodzić w podfoldery |
   | **Pliki bez daty** | Przenieś do „Bez daty” / Pomiń | Przenieś do „Bez daty” | Los plików bez ustalonej daty |
   | **Dopełniaj zerami (03)** | wł. / wył. | wł. | `03` zamiast `3` w miesiącu/dniu (rok zawsze 4-cyfrowy) |

4. **Kliknij „Podgląd (dry-run)”.** Nic nie jest przenoszone — powstaje plan w tabeli z kolumnami:
   **Plik · Data · Źródło · Akcja · Folder docelowy**. Kolumna *Źródło* pokazuje, skąd wzięto datę —
   od EXIF, przez datę pliku, po datę z nazwy pliku (pełna lista i kolejność w sekcji
   [*Skąd brana jest data wykonania*](#skąd-brana-jest-data-wykonania)). Podsumowanie zlicza:
   *Do przeniesienia, Nadpisań, Już na miejscu, Kolizje, Bez daty, Tylko online*.

5. **Sprawdź plan** i kliknij **„Zastosuj”**, aby faktycznie przenieść pliki.

6. **„Cofnij ostatnią operację”** przywraca pliki na poprzednie miejsca (cofnięcie ostatniego uruchomienia,
   łącznie z odtworzeniem plików nadpisanych).

### Zachowania bezpieczeństwa

- **Podgląd (dry-run)** niczego nie zmienia — zawsze możesz sprawdzić plan przed wykonaniem.
- **Cofanie:** dziennik operacji zapisywany jest w folderze `.photoorganizer` wewnątrz obszaru roboczego;
  ten folder **nigdy nie jest skanowany** ani przenoszony.
- **Kolizja → Pomiń:** plik źródłowy pozostaje nietknięty. **Kolizja → Nadpisz:** cofanie odtwarza
  poprzednią zawartość pliku docelowego.
- **Duplikaty o identycznej treści** są pomijane (brak sensownego przeniesienia).
- **Pliki „tylko online”** (placeholdery OneDrive/chmury) są domyślnie **pomijane** — aplikacja nie wymusza
  ich pobrania.
- **Dowiązania symboliczne** (pliki i katalogi) są pomijane — ochrona przed pętlami i wyjściem poza obszar roboczy.
- **Pliki towarzyszące** (sidecar) trzymane są razem z plikiem głównym i dziedziczą jego datę.
- Katalogi bez uprawnień lub znikające w trakcie skanu są pomijane, a nie przerywają całej operacji.

---

## Obsługiwane formaty

**Zdjęcia:** `jpg`, `jpeg`, `png`, `tif`, `tiff`, `heic`, `heif`, `cr2`, `nef`, `arw`, `dng`

**Wideo:** `mp4`, `mov`, `m4v`, `avi`, `mts`, `m2ts`, `3gp`

**Pliki towarzyszące (sidecar)** — grupowane z plikiem głównym: `xmp`, `aae`, `thm`

### Skąd brana jest data wykonania

Aplikacja próbuje ustalić datę w kolejności (pierwsze trafienie wygrywa):

1. **EXIF – DateTimeOriginal** (oryginalna data zdjęcia),
2. **EXIF – DateTimeDigitized** (data digitalizacji),
3. **QuickTime „Created”** (dla wideo mp4/mov),
4. **data ostatniego zapisu pliku**,
5. **data utworzenia pliku**,
6. **data z nazwy pliku** — rozpoznawane schematy typu `IMG_20230415_123456`, `VID-20230415-WA0012`,
   `2023-04-15 wakacje`, `Screenshot_2023-04-15` (wyłącznie porządek rok-miesiąc-dzień),
7. jeśli nic nie uda się ustalić → plik traktowany jest jako **„bez daty”** (zgodnie z opcją *Pliki bez daty*).

**Okno wiarygodności:** akceptowane są wyłącznie daty z zakresu **1950-01-01 … dziś (+1 dzień marginesu)**.
Data spoza okna (np. epoka QuickTime **1904-01-01** z wyzerowanego pola „creation time” w wideo,
epoka FILETIME 1601, daty z przyszłości) jest odrzucana, a łańcuch przechodzi do kolejnego źródła —
dzięki temu nie powstają foldery typu `1904/01`.

> Odczyt metadanych nigdy nie przerywa działania — uszkodzone lub nietypowe metadane po prostu degradują
> do kolejnego kroku łańcucha.
