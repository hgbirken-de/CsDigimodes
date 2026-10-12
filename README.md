# HamDigiModes

**A Digital Voice Amateur Radio Client** for DMR, D-STAR, System Fusion (YSF/FCS) and NXDN.

HamDigiModes connects to the digital voice networks over the internet, so you can listen and talk with a headset, a
microphone and speakers. It does **not need special hardware**: a built-in software vocoder converts the voice, or you
can use an AMBE3000 USB stick or a network AMBE server instead.

<!-- Add a screenshot: ![HamDigiModes](docs/screenshot.png) -->

## Features

- **Five digital voice modes** in one program: DMR, D-STAR, YSF, FCS and NXDN.
- **Three ways to code and decode the voice (AMBE):**
  - the built-in **software vocoder** (no hardware needed),
  - an **AMBE3000 USB stick** (for example ThumbDV or DVstick 30) on a serial port,
  - an **AMBE server** on your network.
- **DMR:** Homebrew and MMDVMHost protocols, master server list, colour code, time slot, talkgroup list with a built-in
  **talkgroup editor** (import and export included).
- **Last heard** list with the calls of the stations you hear.
- Settings dialog, dark theme, optional speech output of received calls (Windows only).
- **Windows** (Avalonia) and **Android** (.NET MAUI) apps that share the same protocol code.

## Supported modes

| Mode | Network protocol | Notes |
|---|---|---|
| DMR | Homebrew, MMDVMHost | talkgroups, time slot, colour code |
| D-STAR | DCS, XRF, REF reflectors | |
| System Fusion | YSF reflectors and FCS rooms | |
| NXDN | NXDN reflectors | |

## Installation

### Windows (x64, Windows 10 or 11)

Download `HamDigiModes-Setup.msi` from the [releases page](<RELEASES-URL>) and run it. The .NET runtime is included, you do
not have to install anything else.

### Android

Download the `.apk` from the [releases page](<RELEASES-URL>) and install it (you may have to allow installing apps from
this source). The app asks for the microphone permission, and for USB access if you use an AMBE stick.

## Quick start

1. Start HamDigiModes and open the menu (☰) → **Settings**.
2. Enter your **callsign**, choose the mode settings (for DMR: protocol, master server, your DMR ID) and the **AMBE
   source**: software vocoder, USB stick (choose the port, or `AUTO`) or network server.
3. Choose the mode in the toolbar and the talkgroup or reflector, then press the **connect** button.
4. Hold **PTT** to talk.

Talkgroups can be edited in the menu under **Talkgroups…** (disabled while connected).

## Where your data is stored

| | Location |
|---|---|
| Windows | `%APPDATA%\HamDigiModes` |
| Settings | `UserSettings.yaml` in that folder |
| DMR talkgroups | `Dmr\Data\DmrTalkGroups.csv` (created from a built-in default at the first start, never overwritten) |
| Log files | in that folder |

On Android the files live in the private storage of the app.

## Build from source

You need the **.NET 8 SDK**. The Android app also needs the MAUI Android workload
(`dotnet workload install maui-android`) and the Android SDK.

```
git clone <REPOSITORY-URL>
cd HamDigiModes

# Windows desktop app
dotnet run --project DigitalVoiceControlApp.Desktop

# everything (needs the MAUI Android workload)
dotnet build HamDigiModes.sln -c Release
```

**Windows installer** (needs [WiX Toolset](https://wixtoolset.org/) v5, `dotnet tool install --global wix`):

```
cd installer
powershell -ExecutionPolicy Bypass -File build.ps1
```

The script publishes the app for `win-x64` (self-contained) and builds `HamDigiModes-Setup.msi`.

**Android package:** `dotnet publish DigitalVoiceControlApp.Maui -f net8.0-android -c Release -p:AndroidPackageFormat=apk`
(plus the signing options for your keystore).

### Solution layout

| Project | Content |
|---|---|
| `DigitalVoice` | protocol clients (DMR, D-STAR, YSF/FCS, NXDN) |
| `FusionCodec`, `DigitalVoice.Common` | codecs and shared types |
| `DigitalVoice.SoftwareVocoder` | software AMBE vocoder (decoder and encoder) |
| `DigitalVoice.AmbeSupport`, `AmbeServer` | AMBE stick access and network AMBE server |
| `DigitalVoice.AudioSupport`, `Maui.AudioSupport` | audio for Windows (NAudio) and Android |
| `DigitalVoiceControlApp`, `…Desktop` | Avalonia user interface and the Windows start project |
| `DigitalVoiceControlApp.Maui` | Android app |
| `installer` | WiX installer and build script |

## Status and known limits

- The software **decoder** has been used for hours with DMR with no audible difference to the stick. The **encoder** (transmit
  with the software vocoder) has had less testing.
- With **D-STAR** the software vocoder sounds worse than the stick.
- Windows (x64) and Android only. Linux is not supported yet (audio and speech output need their own implementation).
- Text-to-speech uses the Windows speech engine.

## License

HamDigiModes is free software: you can redistribute it and modify it under the terms of the **GNU General Public License,
version 3 or (at your option) any later version**. It comes with **absolutely no warranty**. See
[`LICENSE-GPL.txt`](DigitalVoice.SoftwareVocoder/LICENSE-GPL.txt) and <https://www.gnu.org/licenses/gpl-3.0.html>.

Copyright © 2026 Hans-Gunther Birken, DL1HGB

### Third-party software

| Component | Used for | License |
|---|---|---|
| **mbelib** (© 2010 mbelib Author), in the version shipped with DroidStar | software vocoder: decoder and tables | ISC (`LICENSE-mbelib.txt`) |
| **imbe_vocoder** (© 2009 Pavel Yazev) | speech analysis of the software encoder | GPL v3 or later |
| **DroidStar** (Doug McLain, AD8DP) | origin of the vocoder quantisation and tables | ISC |
| Avalonia, NAudio, NLog, YamlDotNet, MsBox.Avalonia and other NuGet packages | user interface, audio, logging, settings | their own licenses |

### Notes

- **AMBE and IMBE** are voice coding methods of Digital Voice Systems, Inc. (DVSI). Whether patents or licenses apply to
  the use of a software implementation depends on your country. Please check this for yourself before using or passing on
  the software.
- DMR, D-STAR, System Fusion, NXDN and AMBE are names or trademarks of their owners. They are used here only to describe
  what the program works with. This project is not affiliated with any of them.
- If you pass on a binary of HamDigiModes, the GPL requires that the recipients can get the matching source code. The
  version and git commit are shown in the **About** dialog.

## Contact

Hans-Gunther Birken, DL1HGB, <CONTACT-OR-ISSUES-URL>

73!
