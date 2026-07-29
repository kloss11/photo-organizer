# Marketplace — gotowa treść oferty (Lemon Squeezy / Gumroad)

Ten plik to **materiał do skopiowania** do panelu sprzedawcy. Nic tu nie jest publikowane
automatycznie — wklejasz ręcznie w odpowiednie pola na platformie.

Model: **kod źródłowy jest darmowy i otwarty (MIT)**; sprzedajesz **gotową, przetestowaną
paczkę do uruchomienia** oraz **wsparcie dalszego rozwoju**. To uczciwe i zgodne z MIT — płaci
się za wygodę i wsparcie autora, nie za dostęp do kodu.

---

## Nazwa produktu
PhotoOrganizer — porządkowanie zdjęć i wideo według daty

## Krótki opis / tagline (1 zdanie)
**PL:** Automatycznie układa zdjęcia i wideo w foldery według daty wykonania — bezpiecznie, offline, na Windows/macOS/Linux.
**EN:** Automatically sorts your photos and videos into date-based folders — safe, offline, on Windows/macOS/Linux.

## Opis długi

### PL
**PhotoOrganizer** porządkuje Twoje zdjęcia i wideo według **daty wykonania**, przenosząc je do
czytelnych folderów `RRRR/MM/DD` (możesz wybrać sam rok, rok+miesiąc albo pełną datę).

Działa w pełni **offline** — Twoje pliki nigdy nie opuszczają komputera. Bezpieczny schemat pracy:
**Podgląd (dry-run) → Zastosuj → Cofnij** — najpierw widzisz dokładny plan, a każdą operację
możesz w całości cofnąć.

**Co dostajesz:**
- Gotowy do uruchomienia plik — **bez instalowania .NET** ani konfiguracji.
- Wersje na **Windows, macOS i Linux**.
- Interfejs w **6 językach** (polski, angielski, niemiecki, rosyjski, hiszpański, francuski).
- Datę program czyta z **EXIF**, a gdy jej brak — z daty pliku lub z nazwy pliku.
- Obsługa RAW (`cr2`, `nef`, `arw`, `dng`), HEIC/HEIF, wideo i plików towarzyszących (sidecar).
- Bezpieczne domyślne zachowania: pomijanie duplikatów, plików „tylko online" (OneDrive/chmura)
  i dowiązań symbolicznych; pełny dziennik operacji do cofania.

Kupując gotową paczkę **wspierasz rozwój** projektu. Kod jest otwarty (MIT) — jeśli wolisz,
możesz zbudować aplikację samodzielnie ze źródeł za darmo.

### EN
**PhotoOrganizer** sorts your photos and videos by their **capture date**, moving them into clean
`YYYY/MM/DD` folders (choose year, year+month, or full date).

It runs fully **offline** — your files never leave your computer. A safe workflow —
**Preview (dry-run) → Apply → Undo** — shows the exact plan first, and any run can be fully undone.

**What you get:**
- A ready-to-run build — **no .NET install** or setup required.
- Builds for **Windows, macOS and Linux**.
- UI in **6 languages** (Polish, English, German, Russian, Spanish, French).
- Dates read from **EXIF**, falling back to file date or the file name.
- RAW support (`cr2`, `nef`, `arw`, `dng`), HEIC/HEIF, video and sidecar files.
- Safe defaults: skips duplicates, cloud-only (OneDrive) placeholders and symlinks; full
  operation log for undo.

Buying the ready-made build **supports development**. The source is open (MIT) — you can always
build it yourself for free.

---

## Rekomendacja ceny

Nisza + narzędzie jednorazowego użytku → sugeruję **„zapłać ile chcesz" (pay what you want)**:
- **Cena sugerowana:** ~5 EUR / ~20 PLN
- **Minimum:** 0 (lub 1 EUR, jeśli chcesz odsiać przypadkowe pobrania)

Alternatywa: **stała cena 5–9 EUR**. Nie celuj wysoko — wartością jest wolumen i dobra wola,
a kto chce, i tak zbuduje ze źródeł.

## Nota o zwrotach / wsparciu (do pola „refund policy")
Produkt cyfrowy. W razie problemu z uruchomieniem napisz na <adres-email> — pomogę lub zwrócę
środki. Zgłoszenia błędów i sugestie: GitHub Issues.

---

## Checklista publikacji (Twoje kroki — poza repo)

1. Załóż konto na **Lemon Squeezy** (zalecane, Merchant of Record — rozlicza VAT UE) lub **Gumroad**.
2. Podepnij metodę wypłaty (PayPal / Wise / konto bankowe).
3. Nowy produkt → typ **cyfrowy / plik do pobrania**.
4. Wklej **nazwę**, **tagline**, **opis długi** (z tego pliku).
5. Ustaw **cenę** (patrz rekomendacja) i włącz „pay what you want", jeśli wybierasz ten model.
6. Zbuduj paczki: `pwsh packaging/marketplace/build-marketplace-zips.ps1 -Version 1.0.0`
   → pliki `.zip` powstaną w `bin/marketplace/`.
7. Wgraj `.zip`-y jako pliki produktu (osobny plik per platforma).
8. Dodaj okładkę/miniaturę (np. zrzut ekranu okna aplikacji).
9. Opublikuj i wklej link do sekcji „Wsparcie" w `README.md` (zastąp placeholder).

## Opcjonalnie, ale zwiększa zaufanie (Windows)
Niepodpisany `.exe` wywołuje ostrzeżenie **SmartScreen**, co psuje wrażenie „gotowego produktu".
Rozważ **podpis cyfrowy (code signing)**:
- **Azure Trusted Signing** — najtaniej (~10 USD/mies., wymaga zweryfikowanej tożsamości),
- lub certyfikat **OV** od CA (~200–400 EUR/rok).
Po podpisaniu ostrzeżenie znika. Nie jest to konieczne do startu, ale warte rozważenia.
