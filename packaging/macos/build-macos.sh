#!/usr/bin/env bash
# Buduje bundle .app dla macOS. Uruchom NA macOS (podpis/notaryzacja wymagają Xcode + Developer ID).
# Użycie: ./build-macos.sh [osx-arm64|osx-x64]
set -euo pipefail

RID="${1:-osx-arm64}"
CONFIG="Release"
HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/../.." && pwd)"
APP_PROJ="$ROOT/PhotoOrganizer.App/PhotoOrganizer.App.csproj"
OUT="$ROOT/bin/publish/$RID"
BUNDLE="$ROOT/bin/bundle/PhotoOrganizer.app"

echo ">> Publikacja self-contained ($RID)…"
dotnet publish "$APP_PROJ" -c "$CONFIG" -r "$RID" --self-contained true \
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "$OUT"

echo ">> Składanie bundla .app…"
rm -rf "$BUNDLE"
mkdir -p "$BUNDLE/Contents/MacOS" "$BUNDLE/Contents/Resources"
cp "$HERE/Info.plist" "$BUNDLE/Contents/Info.plist"
cp -R "$OUT/." "$BUNDLE/Contents/MacOS/"
chmod +x "$BUNDLE/Contents/MacOS/PhotoOrganizer.App"
cp "$HERE/PhotoOrganizer.icns" "$BUNDLE/Contents/Resources/"

echo ">> Gotowe: $BUNDLE"
cat <<EOF

Następne kroki (wymagają konta Apple Developer):
  1) Podpis (hardened runtime, BEZ sandbox):
     codesign --deep --force --options runtime \\
       --entitlements "$HERE/entitlements.plist" \\
       --sign "Developer ID Application: <TWOJA NAZWA> (<TEAMID>)" "$BUNDLE"
  2) Notaryzacja:
     ditto -c -k --keepParent "$BUNDLE" "$ROOT/bin/PhotoOrganizer.zip"
     xcrun notarytool submit "$ROOT/bin/PhotoOrganizer.zip" \\
       --apple-id <APPLE_ID> --team-id <TEAMID> --password <APP_SPECIFIC_PW> --wait
     xcrun stapler staple "$BUNDLE"

Uwaga: przy pierwszym uruchomieniu użytkownik nadaje DWA uprawnienia —
Accessibility (dla gestu) i Automation→Finder (dla odczytu folderu).
EOF
