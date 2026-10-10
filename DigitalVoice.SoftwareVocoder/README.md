# DigitalVoice.SoftwareVocoder: AMBE-Vocoder in Software (GPL)

Ein Vocoder für AMBE (DMR, YSF, FCS, NXDN, D-STAR) ohne Hardware: wandelt 20-ms-Frames in 160 PCM-Samples (8 kHz, 16 Bit) und
umgekehrt. Ein Projekt, alles zusammen:

| Teil | Datei(en) | Herkunft | Lizenz der Datei |
|---|---|---|---|
| Decoder (`AmbeSoftwareDecoder`) | `AmbeFrameDecoder`, `MbeLib`, `Ecc`, `AmbeTables` ... | Port von mbelib (in der Fassung aus DroidStar) | ISC |
| Quantisierung (`AmbeSoftwareEncoder`) | `AmbeSoftwareEncoder`, `AmbeEncodeTables` | Port aus DroidStar (`vocoder_plugin.cpp`) | ISC |
| Sprachanalyse (`ImbeAnalyzer`) | `Imbe\*` | Port von imbe_vocoder, Pavel Yazev | **GPL v3 oder neuer** |
| `AmbeSoftwareController` | `AmbeSupport\*` | eigen | wie das Projekt |

## Lizenz: GNU General Public License Version 3 oder neuer

Weil das Projekt GPL-Code enthält, steht es **als Ganzes** unter der GPL (Text: `LICENSE-GPL.txt`). Die Dateien aus mbelib
behalten ihren ISC-Hinweis im Dateikopf, er darf nicht entfernt werden (`LICENSE-mbelib.txt`).

Folge für die **Weitergabe**: Eine Anwendung, die dieses Projekt enthält und weitergegeben wird (auch über einen Store), steht als
Ganzes unter der GPL, und ihre Empfänger müssen den Quelltext der weitergegebenen Fassung bekommen können. Für die eigene
Nutzung entstehen keine Pflichten. AMBE ist ein Verfahren der Firma DVSI: Ob Patente oder Lizenzen zu beachten sind, ist eine
eigene Frage und nicht Gegenstand der GPL.

## Verwendung

```csharp
var controller = new AmbeSoftwareController(encoder: new AmbeSoftwareEncoder(new ImbeAnalyzer()));   // Hören und Senden
var listenOnly = new AmbeSoftwareController();                                                       // nur Hören
```

Die Einheiten lassen sich auch einzeln benutzen: `new AmbeSoftwareDecoder()`, `new AmbeSoftwareEncoder(new ImbeAnalyzer())`.

## Prüfung

`tools/SoftwareVocoderCheck` vergleicht Decoder, Kodierer und Controller mit dem C++-Original aus DroidStar (Decoder bis auf
1 LSB, Kodierer bitgleich). `dotnet run -c Release --project tools/SoftwareVocoderCheck`
