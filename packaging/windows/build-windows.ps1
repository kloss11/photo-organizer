# Publikuje self-contained, jednoplikowy exe dla Windows.
# Użycie: pwsh ./build-windows.ps1 [-Rid win-x64]
param([string]$Rid = "win-x64")

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
$out = Join-Path $root "bin\publish\$Rid"

dotnet publish (Join-Path $root "PhotoOrganizer.App\PhotoOrganizer.App.csproj") `
    -c Release -r $Rid --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $out

Write-Host "Gotowe. Exe: $(Join-Path $out 'PhotoOrganizer.App.exe')"
Write-Host "Instalator (opcjonalnie): zbuduj NSIS/Inno z powyższego katalogu, lub użyj Avalonia Parcel."
