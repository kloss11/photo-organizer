#requires -Version 7
<#
.SYNOPSIS
    Buduje paczki .zip gotowe do wgrania na marketplace (Lemon Squeezy / Gumroad).

.DESCRIPTION
    Pakuje wcześniej ZBUDOWANE artefakty do bin/marketplace/PhotoOrganizer-<Version>-<platforma>.zip,
    dokładając do każdej paczki plik LICENSE oraz QUICKSTART.txt z krótką instrukcją uruchomienia.

    Preferuje artefakty z pełną integracją, z fallbackiem do "gołych" binariów:
      Windows : bin/publish/win-x64/PhotoOrganizer.App.exe
      macOS   : bin/bundle/PhotoOrganizer.app  (fallback: bin/publish/osx-arm64 | osx-x64)
      Linux   : bin/*.AppImage                 (fallback: bin/publish/linux-x64)

    NAJPIERW zbuduj artefakty skryptami z packaging/ (np. windows/build-windows.ps1),
    a bundle .app / AppImage na docelowym systemie (macOS / Linux).

.EXAMPLE
    pwsh packaging/marketplace/build-marketplace-zips.ps1 -Version 1.0.0
#>
param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..' '..')).Path
$publish  = Join-Path $repoRoot 'bin/publish'
$outDir   = Join-Path $repoRoot 'bin/marketplace'
$license  = Join-Path $repoRoot 'LICENSE'

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

function New-Package {
    param(
        [Parameter(Mandatory)] [string]   $PlatformLabel,   # np. win-x64
        [Parameter(Mandatory)] [string[]] $SourceItems,     # pliki/katalogi do spakowania
        [Parameter(Mandatory)] [string]   $Quickstart
    )
    $existing = @($SourceItems | Where-Object { $_ -and (Test-Path $_) })
    if ($existing.Count -eq 0) {
        Write-Host "  - pomijam $PlatformLabel (brak zbudowanego artefaktu)" -ForegroundColor DarkYellow
        return
    }

    $stage = Join-Path $outDir "_stage_$PlatformLabel"
    if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
    New-Item -ItemType Directory -Force -Path $stage | Out-Null

    foreach ($item in $existing) { Copy-Item $item -Destination $stage -Recurse }
    if (Test-Path $license) { Copy-Item $license -Destination (Join-Path $stage 'LICENSE.txt') }
    Set-Content -Path (Join-Path $stage 'QUICKSTART.txt') -Value $Quickstart -Encoding UTF8

    $zip = Join-Path $outDir "PhotoOrganizer-$Version-$PlatformLabel.zip"
    if (Test-Path $zip) { Remove-Item $zip -Force }
    Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip
    Remove-Item $stage -Recurse -Force

    $sizeMb = [math]::Round((Get-Item $zip).Length / 1MB, 1)
    Write-Host "  + $([System.IO.Path]::GetFileName($zip))  ($sizeMb MB)" -ForegroundColor Green
}

Write-Host "Pakowanie marketplace (v$Version) -> $outDir" -ForegroundColor Cyan

# --- Windows -----------------------------------------------------------------
New-Package -PlatformLabel 'win-x64' `
    -SourceItems @( (Join-Path $publish 'win-x64/PhotoOrganizer.App.exe') ) `
    -Quickstart @'
PhotoOrganizer - Windows
========================
1. Rozpakuj ten ZIP do dowolnego folderu.
2. Uruchom PhotoOrganizer.App.exe (podwojne klikniecie).
3. Jesli SmartScreen pokaze ostrzezenie (plik niepodpisany):
   kliknij "Wiecej informacji" -> "Uruchom mimo to".

Nie wymaga instalacji .NET. Dziekuje za wsparcie projektu!
Kod zrodlowy (MIT): <URL repozytorium>
'@

# --- macOS -------------------------------------------------------------------
$macApp  = Join-Path $repoRoot 'bin/bundle/PhotoOrganizer.app'
$macBare = if (Test-Path (Join-Path $publish 'osx-arm64/PhotoOrganizer.App')) {
    Join-Path $publish 'osx-arm64/PhotoOrganizer.App'
} else {
    Join-Path $publish 'osx-x64/PhotoOrganizer.App'
}
New-Package -PlatformLabel 'macos' `
    -SourceItems @( $macApp, $macBare | Select-Object -First 1 ) `
    -Quickstart @'
PhotoOrganizer - macOS
======================
1. Rozpakuj ZIP i przeciagnij PhotoOrganizer.app do /Applications
   (jesli w paczce jest samo binarium: chmod +x PhotoOrganizer.App).
2. Pierwsze uruchomienie: klik prawym -> "Otworz", aby ominac Gatekeeper.
3. Nadaj uprawnienia: Dostepnosc (gest) oraz Automatyzacja -> Finder.

Dziekuje za wsparcie projektu!
Kod zrodlowy (MIT): <URL repozytorium>
'@

# --- Linux -------------------------------------------------------------------
$appImage = Get-ChildItem -Path (Join-Path $repoRoot 'bin') -Filter '*.AppImage' -File -ErrorAction SilentlyContinue |
            Select-Object -First 1 -ExpandProperty FullName
$linuxBare = Join-Path $publish 'linux-x64/PhotoOrganizer.App'
New-Package -PlatformLabel 'linux-x64' `
    -SourceItems @( $appImage, $linuxBare | Where-Object { $_ } | Select-Object -First 1 ) `
    -Quickstart @'
PhotoOrganizer - Linux
======================
1. Rozpakuj ZIP.
2. Nadaj bit wykonywalny:  chmod +x PhotoOrganizer*.AppImage (lub PhotoOrganizer.App)
3. Uruchom:  ./PhotoOrganizer-x86_64.AppImage

Uwaga: globalny gest "Esc + klik" nie dziala na Linux (uzyj wyboru folderu / drag&drop).
Dziekuje za wsparcie projektu!
Kod zrodlowy (MIT): <URL repozytorium>
'@

Write-Host "Gotowe. Wgraj pliki .zip z $outDir na marketplace." -ForegroundColor Cyan
