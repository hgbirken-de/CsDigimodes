$ErrorActionPreference = "Stop"

# powershell -ExecutionPolicy Bypass -File build.ps1

# Pfad zur .csproj des AUSFÜHRBAREN Desktop-Hosts (nicht die UI-Bibliothek
# DigitalVoiceControlApp selbst - die hat keinen Einstiegspunkt/Main()).
$csproj = "..\DigitalVoiceControlApp.Desktop\DigitalVoiceControlApp.Desktop.csproj"

# Alten publish-Ordner löschen: Sonst bleiben Dateien einer früheren Fassung (z. B. DigitalVoiceControlApp.Desktop.exe)
# darin liegen und kommen mit in die MSI.
if (Test-Path .\publish) { Remove-Item .\publish -Recurse -Force }

Write-Host "1) Publishing app (self-contained, win-x64)..."
dotnet publish $csproj -c Release -r win-x64 --self-contained true -o .\publish

# Die Lizenztexte müssen im publish-Ordner liegen, sonst fehlen sie in der Installation
foreach ($f in @("LICENSE-GPL.txt", "LICENSE-mbelib.txt")) {
    if (-not (Test-Path ".\publish\$f")) {
        throw "$f fehlt im publish-Ordner. In DigitalVoiceControlApp.Desktop.csproj als None-Element mit CopyToPublishDirectory eintragen."
    }
}

# Die Version der MSI kommt aus der gebauten Exe (Dateiversion, vier Zahlen)
$exe = ".\publish\HamDigiModes.exe"
$version = (Get-Item $exe).VersionInfo.FileVersion
if (-not $version) { throw "Keine Dateiversion in $exe gefunden." }
Write-Host "   Version: $version"

Write-Host "2) Building MSI..."
# -arch x64: ohne diese Angabe baut WiX ein x86-Paket (Installation unter "Program Files (x86)")
wix build Installer.wxs -arch x64 -d "Version=$version" -o .\HamDigiModes-Setup.msi

Write-Host ""
Write-Host "Fertig: installer\HamDigiModes-Setup.msi (Version $version)"
