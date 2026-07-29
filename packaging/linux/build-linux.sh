#!/usr/bin/env bash
# Publikuje self-contained binarkę dla Linux i przygotowuje AppDir pod AppImage.
# Uruchom NA Linux (do AppImage potrzebny appimagetool). Użycie: ./build-linux.sh [linux-x64|linux-arm64]
set -euo pipefail

RID="${1:-linux-x64}"
CONFIG="Release"
HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/../.." && pwd)"
APP_PROJ="$ROOT/PhotoOrganizer.App/PhotoOrganizer.App.csproj"
OUT="$ROOT/bin/publish/$RID"
APPDIR="$ROOT/bin/appdir/PhotoOrganizer.AppDir"

echo ">> Publikacja self-contained ($RID)…"
dotnet publish "$APP_PROJ" -c "$CONFIG" -r "$RID" --self-contained true \
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "$OUT"

echo ">> Składanie AppDir…"
rm -rf "$APPDIR"
mkdir -p "$APPDIR/usr/bin"
cp -R "$OUT/." "$APPDIR/usr/bin/"
cp "$HERE/photoorganizer.desktop" "$APPDIR/PhotoOrganizer.desktop"
cp "$HERE/photoorganizer.png" "$APPDIR/photoorganizer.png"
cat > "$APPDIR/AppRun" <<'RUN'
#!/bin/sh
HERE="$(dirname "$(readlink -f "$0")")"
exec "$HERE/usr/bin/PhotoOrganizer.App" "$@"
RUN
chmod +x "$APPDIR/AppRun" "$APPDIR/usr/bin/PhotoOrganizer.App"

echo ">> AppDir gotowy: $APPDIR"
echo ">> AppImage:  appimagetool \"$APPDIR\" \"$ROOT/bin/PhotoOrganizer-x86_64.AppImage\""
echo ""
echo "UWAGA: globalny gest Esc+klik NIE działa na Linux (Wayland blokuje globalny input;"
echo "żaden menedżer plików nie udostępnia bieżącego folderu). Aplikacja używa ręcznego"
echo "wyboru folderu / drag&drop. Preferuj AppImage (mniej piaskownicy niż Flatpak)."
