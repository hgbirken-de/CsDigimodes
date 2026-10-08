# SoftwareVocoder: AMBE-Decoder in C#

Wandelt AMBE-Frames (20 ms) in 160 PCM-Samples (8 kHz, 16 Bit). Port des Decoders der **mbelib** in der Fassung aus
DroidStar (ISC-Lizenz, siehe `LICENSE-mbelib.txt`). Der Kodierer (PCM nach AMBE) ist **nicht** enthalten.

```csharp
var decoder = new AmbeSoftwareDecoder();          // eine Instanz je Datenstrom
short[] pcm = new short[160];

decoder.Decode2450x1150(ambe9Bytes, pcm);   // DMR (AMBE+2 mit FEC, 9 Byte)
decoder.Decode2450(ambe7Bytes, pcm);        // YSF, FCS, NXDN (ohne FEC, 7 Byte), Bits in Luft-Reihenfolge
decoder.Decode2450DvsiOrder(chip7Bytes, pcm); // dasselbe, Bits in der Reihenfolge des DVSI-Chips (Channel-Paket der Clients)
decoder.Decode2400x1200(ambe9Bytes, pcm);   // D-STAR (mit FEC, 9 Byte)

decoder.Reset();                            // zu Beginn eines neuen Streams
```

* **Bitreihenfolge bei 49 Bit (YSF, FCS, NXDN):** Der DVSI-Chip erwartet die Bits in einer anderen Reihenfolge, als sie in der
  Luft übertragen werden. Die Clients ordnen sie für den Chip um (`NxdnCodec.Interleave`, Fusion-`AmbeExtractor`). Der
  `AmbeSoftwareController` ordnet sie zurück (`Decode2450DvsiOrder`). Bei 72 Bit (DMR, D-STAR) gibt es das Problem nicht.
* **D-STAR:** Die mbelib kennt für das ältere AMBE 3600x2400 nicht alle Original-Tabellen (im Quelltext stehen dafür "guess"- und
  "TODO: use correct table"-Hinweise). Die Sprachqualität ist dort deutlich schlechter als beim Chip.
* Keine Speicherzuteilung je Frame, kein nativer Code, plattformunabhängig (Windows, Android, ...).
* Nicht gleichzeitig aus mehreren Threads aufrufen (die Instanz hat den Zustand des vorigen Frames).
* Laufzeit etwa 0,4 ms je Frame (Budget 20 ms) auf einem 2-GHz-Xeon-Kern.

## Prüfung

`tools/SoftwareVocoderCheck` vergleicht den Decoder mit Referenzdaten, die mit dem C++-Original von DroidStar erzeugt wurden
(`dotnet run -c Release --project tools/SoftwareVocoderCheck`). Erlaubt ist eine Abweichung von höchstens 2 LSB (Unterschiede
der Cosinus-Implementierung der jeweiligen Plattform).

## Herkunft der Tabellen

`AmbeTables.cs` ist aus den Konstanten-Headern des C-Originals erzeugt (Werte als double gelesen und nach float gewandelt,
wie der C-Compiler es tut).
