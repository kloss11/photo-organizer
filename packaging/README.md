# Pakowanie PhotoOrganizer

Skrypty budują **self-contained, jednoplikowe** artefakty (bez potrzeby instalacji .NET u użytkownika).
Natywny `libuiohook` (SharpHook) jest dołączany do artefaktu (`IncludeNativeLibrariesForSelfExtract`).

| Platforma | Skrypt | Artefakt | Status |
|---|---|---|---|
| Windows | `windows/build-windows.ps1` | `bin/publish/win-x64/PhotoOrganizer.App.exe` | ✅ zbudowany i uruchomiony |
| Linux | `linux/build-linux.sh` | AppDir → AppImage | ✅ publikacja zweryfikowana (cross-build); AppImage buduj na Linux |
| macOS | `macos/build-macos.sh` | `PhotoOrganizer.app` (+ podpis/notaryzacja) | ✅ publikacja zweryfikowana (cross-build); bundle/podpis rób na macOS |

## Windows
```powershell
pwsh packaging/windows/build-windows.ps1
```
Opcjonalnie zapakuj katalog w instalator (NSIS/Inno) lub użyj Avalonia **Parcel**.

## Linux (uruchom na Linux)
```bash
bash packaging/linux/build-linux.sh
appimagetool bin/appdir/PhotoOrganizer.AppDir bin/PhotoOrganizer-x86_64.AppImage
```
- Globalny gest Esc+klik **nie działa** na Linux (Wayland + brak API menedżera plików) → aplikacja używa ręcznego wyboru folderu i drag&drop.
- Preferuj **AppImage** (mniej piaskownicy niż Flatpak, który dodatkowo blokuje globalny input).

## macOS (uruchom na macOS)
```bash
bash packaging/macos/build-macos.sh osx-arm64   # lub osx-x64
```
Następnie **podpis (hardened runtime, bez App Sandbox)** i **notaryzacja** — komendy wypisuje skrypt.
- `Info.plist` zawiera `NSAppleEventsUsageDescription` (wymagane do sterowania Finderem).
- `entitlements.plist`: `automation.apple-events` + `disable-library-validation`/`allow-jit` (dla libuiohook i .NET).
- Przy pierwszym uruchomieniu użytkownik nadaje **dwa** uprawnienia: **Accessibility** (gest) i **Automation→Finder** (odczyt folderu).

## Uwaga o macierzy funkcji
Gest „Esc+klik → odczyt folderu z menedżera plików" jest w pełni funkcjonalny na **Windows**,
działa na **macOS** (2 uprawnienia + notaryzacja), a na **Linux** degraduje do ręcznego wyboru folderu.
Silnik porządkowania (podgląd → zastosuj → cofnij) działa identycznie na wszystkich platformach.
