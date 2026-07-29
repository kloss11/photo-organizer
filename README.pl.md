<div align="center">

<img src="PhotoOrganizer.App/Assets/logo.png" width="96" alt="Logo PhotoOrganizer">

# PhotoOrganizer

**Porządkuje zdjęcia i wideo według daty wykonania — bezpiecznie.**

Przenosi pliki do czytelnych folderów `RRRR/MM/DD`. Działa na **Windows, macOS i Linux**.
**W 100% offline** — pliki nigdy nie opuszczają Twojego komputera. Darmowy i open source.

[![Licencja](https://img.shields.io/github/license/kloss11/photo-organizer)](LICENSE)
[![Najnowsze wydanie](https://img.shields.io/github/v/release/kloss11/photo-organizer)](https://github.com/kloss11/photo-organizer/releases/latest)
![Platformy](https://img.shields.io/badge/platforma-Windows%20%7C%20macOS%20%7C%20Linux-blue)

🌐 **[Strona &amp; pobieranie →](https://kloss11.github.io/photo-organizer/)**

[English](README.md) · **Polski**

</div>

---

## Pobierz

Gotowa wersja do uruchomienia — **bez instalacji, bez .NET** (samodzielny, jednoplikowy artefakt):

| System | Pobierz |
|---|---|
| 🪟 **Windows** (x64) | [PhotoOrganizer-windows-x64.zip](https://github.com/kloss11/photo-organizer/releases/latest/download/PhotoOrganizer-windows-x64.zip) |
| 🐧 **Linux** (x64) | [PhotoOrganizer-linux-x64.zip](https://github.com/kloss11/photo-organizer/releases/latest/download/PhotoOrganizer-linux-x64.zip) |
| 🍎 **macOS** (Apple Silicon) | [PhotoOrganizer-macos-arm64.zip](https://github.com/kloss11/photo-organizer/releases/latest/download/PhotoOrganizer-macos-arm64.zip) |
| 🍎 **macOS** (Intel) | [PhotoOrganizer-macos-x64.zip](https://github.com/kloss11/photo-organizer/releases/latest/download/PhotoOrganizer-macos-x64.zip) |

Wszystkie wydania: **[github.com/kloss11/photo-organizer/releases](https://github.com/kloss11/photo-organizer/releases)**

**Pierwsze uruchomienie:**
- **Windows** — rozpakuj i kliknij dwukrotnie `PhotoOrganizer.App.exe`. Plik nie jest jeszcze podpisany cyfrowo, więc SmartScreen może ostrzec o „nieznanym wydawcy" — wybierz **Więcej informacji → Uruchom mimo to**.
- **macOS** — rozpakuj, przy pierwszym razie kliknij aplikację prawym → **Otwórz**, aby ominąć Gatekeeper.
- **Linux** — rozpakuj i nadaj bit wykonywalny: `chmod +x PhotoOrganizer.App`, potem uruchom.

---

## Instrukcja obsługi

1. **Uruchom aplikację.** W prawym górnym rogu możesz zmienić **język** interfejsu.
2. **Wskaż folder roboczy** na jeden z trzech sposobów:
   - przycisk **„Wybierz folder…”**,
   - **przeciągnij** folder z menedżera plików do okna aplikacji,
   - **gest** (tylko Windows): **przytrzymaj `Esc` i kliknij lewym** w oknie Eksploratora — aplikacja odczyta ścieżkę aktualnie otwartego folderu. *(macOS: analogicznie, po nadaniu uprawnień; Linux: brak gestu.)*
3. **Ustaw opcje:**

   | Opcja | Wartości | Domyślnie | Efekt |
   |---|---|---|---|
   | **Granularność** | Rok / Rok i miesiąc / Rok, miesiąc i dzień | Rok i miesiąc | Głębokość folderów: `2024`, `2024/03`, `2024/03/15` |
   | **Kolizje nazw** | Pomiń / Nadpisz | Pomiń | Co zrobić, gdy w folderze docelowym istnieje już plik o tej nazwie |
   | **Zakres skanu** | Rekurencyjnie / Tylko najwyższy poziom | Rekurencyjnie | Czy wchodzić w podfoldery |
   | **Pliki bez daty** | Przenieś do „Bez daty” / Pomiń | Przenieś do „Bez daty” | Los plików bez ustalonej daty |
   | **Dopełniaj zerami (03)** | wł. / wył. | wł. | `03` zamiast `3` w miesiącu/dniu (rok zawsze 4-cyfrowy) |

4. **Kliknij „Podgląd (dry-run)”.** Nic nie jest przenoszone — powstaje plan w tabeli z kolumnami **Plik · Data · Źródło · Akcja · Folder docelowy**. Kolumna *Źródło* pokazuje, skąd wzięto datę. Podsumowanie zlicza: *Do przeniesienia, Nadpisań, Już na miejscu, Kolizje, Bez daty, Tylko online*.
5. **Sprawdź plan** i kliknij **„Zastosuj”**, aby faktycznie przenieść pliki.
6. **„Cofnij ostatnią operację”** przywraca pliki na poprzednie miejsca (łącznie z odtworzeniem plików nadpisanych).

### Zachowania bezpieczeństwa

- **Podgląd (dry-run)** niczego nie zmienia — zawsze możesz sprawdzić plan przed wykonaniem.
- **Cofanie:** dziennik operacji zapisywany jest w folderze `.photoorganizer` wewnątrz obszaru roboczego; ten folder **nigdy nie jest skanowany** ani przenoszony.
- **Kolizja → Pomiń:** plik źródłowy pozostaje nietknięty. **Kolizja → Nadpisz:** cofanie odtwarza poprzednią zawartość pliku docelowego.
- **Duplikaty o identycznej treści** są pomijane (brak sensownego przeniesienia).
- **Pliki „tylko online”** (placeholdery OneDrive/chmury) są domyślnie **pomijane** — aplikacja nie wymusza ich pobrania.
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
6. **data z nazwy pliku** — schematy typu `IMG_20230415_123456`, `VID-20230415-WA0012`, `2023-04-15 wakacje`, `Screenshot_2023-04-15` (wyłącznie porządek rok-miesiąc-dzień),
7. jeśli nic nie uda się ustalić → plik traktowany jest jako **„bez daty”** (zgodnie z opcją *Pliki bez daty*).

**Okno wiarygodności:** akceptowane są wyłącznie daty z zakresu **1950-01-01 … dziś (+1 dzień marginesu)**. Data spoza okna (np. epoka QuickTime **1904-01-01**, epoka FILETIME 1601, daty z przyszłości) jest odrzucana, a łańcuch przechodzi do kolejnego źródła — dzięki temu nie powstają foldery typu `1904/01`.

> Odczyt metadanych nigdy nie przerywa działania — uszkodzone lub nietypowe metadane po prostu degradują do kolejnego kroku łańcucha.

---

## Budowanie ze źródeł

Wymaga **.NET 10 SDK**. Wszystkie platformy można zbudować cross-platform z dowolnego systemu:

```bash
for rid in win-x64 linux-x64 osx-x64 osx-arm64; do
  dotnet publish PhotoOrganizer.App/PhotoOrganizer.App.csproj -c Release -r $rid \
    --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
    -o "bin/publish/$rid"
done
```

Pakowanie per platforma (Windows `.exe`, macOS `.app`, Linux AppImage) jest w [`packaging/`](packaging/README.md). Testy: `dotnet test`.

---

## Wsparcie

PhotoOrganizer jest **darmowy i open source (MIT)**. Jeśli zaoszczędził Ci czas i chcesz wesprzeć dalszy rozwój:

☕ **[Buy Me a Coffee](https://buymeacoffee.com/kloss)** — dziękuję!

Znalazłeś błąd? **[Zgłoś go tutaj](https://github.com/kloss11/photo-organizer/issues/new?template=bug_report.yml)**. Możesz też pomóc: ⭐ zostaw gwiazdkę.

---

## Licencja

Wydane na [licencji MIT](LICENSE) — © 2026 Marcin Nadolny.
