$ErrorActionPreference = "Stop"

# Pfad zur .csproj des AUSFÜHRBAREN Desktop-Hosts (nicht die UI-Bibliothek
# DigitalVoiceControlApp selbst - die hat keinen Einstiegspunkt/Main()).
$csproj = "..\DigitalVoiceControlApp.Desktop\DigitalVoiceControlApp.Desktop.csproj"

Write-Host "1) Publishing app (self-contained, win-x64)..."
dotnet publish $csproj -c Release -r win-x64 --self-contained true -o .\publish

Write-Host "2) Building MSI..."
wix build Installer.wxs -o .\CsDigimodes-Setup.msi

Write-Host ""
Write-Host "Fertig: installer\CsDigimodes-Setup.msi"
