namespace DigitalVoice.SoftwareVocoder;

/// <summary>
/// Software-Decoder für AMBE-Frames (DMR, YSF, FCS, NXDN und D-STAR): wandelt die Codec-Bytes eines 20-ms-Frames in 160
/// PCM-Samples (8 kHz, 16 Bit) um. Der Decoder ist ein Port der freien Bibliothek <b>mbelib</b> in der Fassung aus
/// DroidStar (Copyright (C) 2010 mbelib Author, ISC-Lizenz).
/// <para>
/// Jede Instanz hat den Zustand eines Datenstroms (die Parameter des vorigen Frames): Pro Datenstrom eine eigene Instanz
/// verwenden und nicht aus mehreren Threads gleichzeitig aufrufen. Zu Beginn eines neuen Streams <see cref="Reset"/>.
/// </para>
/// <para>
/// Hinweis: AMBE ist ein Verfahren der Firma DVSI. Wer dieses Programm weitergibt, muss selbst klären, ob dafür Patente oder
/// Lizenzen beachtet werden müssen.
/// </para>
/// </summary>
public sealed class AmbeSoftwareDecoder
{
    private const int UvQuality = 3;   // wie in DroidStar

    private readonly MbeParms _cur = new();
    private readonly MbeParms _prev = new();
    private readonly MbeParms _prevEnhanced = new();
    private readonly NoiseSource _noise;

    // Arbeitsspeicher (damit pro Frame nichts angelegt wird)
    private readonly int[] _frame = new int[AmbeFrameDecoder.FrameBits];
    private readonly int[] _ambeD = new int[49];
    private readonly float[] _audio = new float[160];
    private int _errs2;   // Fehler des letzten Frames: wird (wie im Original) vom nächsten 49-Bit-Frame ohne FEC wiederverwendet

    public AmbeSoftwareDecoder(NoiseSource? noise = null)
    {
        _noise = noise ?? new NoiseSource((uint)Environment.TickCount);
        Reset();
    }

    /// <summary>Setzt den Zustand auf den Anfang eines Datenstroms zurück.</summary>
    public void Reset()
    {
        MbeLib.InitParms(_cur, _prev, _prevEnhanced);
        _errs2 = 0;
        Array.Clear(_ambeD);
    }

    /// <summary>DMR: AMBE+2 3600x2450 mit FEC, 72 Bit (9 Byte) je Frame (<c>decode_2450x1150</c>).</summary>
    public void Decode2450x1150(ReadOnlySpan<byte> ambe, Span<short> pcm)
    {
        if (ambe.Length < 9) throw new ArgumentException("An AMBE frame with FEC has 9 bytes.", nameof(ambe));
        if (pcm.Length < 160) throw new ArgumentException("The PCM buffer must hold 160 samples.", nameof(pcm));

        Array.Clear(_frame);
        int[] rW = AmbeTables.rW, rX = AmbeTables.rX, rY = AmbeTables.rY, rZ = AmbeTables.rZ;

        int p = 0;
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 8; j += 2)
            {
                _frame[rY[p] * 24 + rZ[p]] = 1 & (ambe[i] >> (7 - (j + 1)));
                _frame[rW[p] * 24 + rX[p]] = 1 & (ambe[i] >> (7 - j));
                p++;
            }
        }

        ProcessFrame(true);
        ToPcm(pcm);
    }

    /// <summary>D-STAR: AMBE 3600x2400 mit FEC, 72 Bit (9 Byte) je Frame (<c>decode_2400x1200</c>).</summary>
    public void Decode2400x1200(ReadOnlySpan<byte> ambe, Span<short> pcm)
    {
        if (ambe.Length < 9) throw new ArgumentException("An AMBE frame with FEC has 9 bytes.", nameof(ambe));
        if (pcm.Length < 160) throw new ArgumentException("The PCM buffer must hold 160 samples.", nameof(pcm));

        Array.Clear(_frame);
        int[] dW = AmbeTables.dW, dX = AmbeTables.dX;

        int p = 0;
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                _frame[dW[p] * 24 + dX[p]] = 1 & (ambe[i] >> j);
                p++;
            }
        }

        ProcessFrame(false);
        ToPcm(pcm);
    }

    /// <summary>
    /// YSF, FCS und NXDN: AMBE+2 2450 ohne FEC, 49 Bit (7 Byte) je Frame (<c>decode_2450</c>). Die Bits stehen in der
    /// <b>Luft-Reihenfolge</b>, wie sie aus dem Funkframe kommen. Bekommt man die Frames in der Reihenfolge des DVSI-Chips
    /// (so wie sie die Clients an den Chip schicken), ist <see cref="Decode2450DvsiOrder"/> das Richtige.
    /// </summary>
    public void Decode2450(ReadOnlySpan<byte> ambe, Span<short> pcm)
    {
        if (ambe.Length < 7) throw new ArgumentException("An AMBE frame without FEC has 7 bytes.", nameof(ambe));
        if (pcm.Length < 160) throw new ArgumentException("The PCM buffer must hold 160 samples.", nameof(pcm));

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 8; j++)
                _ambeD[j + (8 * i)] = 1 & (ambe[i] >> (7 - j));
        }
        _ambeD[48] = 1 & (ambe[6] >> 7);

        AmbeFrameDecoder.ProcessData(true, _audio, _errs2, _ambeD, _cur, _prev, _prevEnhanced, UvQuality, _noise);
        ToPcm(pcm);
    }

    /// <summary>
    /// Wie <see cref="Decode2450"/>, die 49 Bits stehen aber in der Reihenfolge des DVSI-Chips (Channel-Paket der Clients bei
    /// YSF, FCS und NXDN). Die Bits werden zuerst in die Luft-Reihenfolge zurückgeordnet.
    /// </summary>
    public void Decode2450DvsiOrder(ReadOnlySpan<byte> dvsi, Span<short> pcm)
    {
        if (dvsi.Length < 7) throw new ArgumentException("An AMBE frame without FEC has 7 bytes.", nameof(dvsi));

        Span<byte> air = stackalloc byte[7];
        DvsiBitOrder.ToAirOrder(dvsi, air);
        Decode2450(air, pcm);
    }

    /// <summary>Fehlerkorrektur und Dekodierung eines Frames mit FEC (<c>mbe_processAmbe3600x2450Framef</c> und -2400).</summary>
    private void ProcessFrame(bool is2450)
    {
        int errs = AmbeFrameDecoder.EccC0(_frame);
        AmbeFrameDecoder.Demodulate(_frame);
        _errs2 = errs;
        _errs2 += AmbeFrameDecoder.EccData(_frame, _ambeD);

        AmbeFrameDecoder.ProcessData(is2450, _audio, _errs2, _ambeD, _cur, _prev, _prevEnhanced, UvQuality, _noise);
    }

    /// <summary>Begrenzt auf +-32760 und schneidet auf 16 Bit ab (<c>VocoderPlugin::processAudio</c>).</summary>
    private void ToPcm(Span<short> pcm)
    {
        for (int i = 0; i < 160; i++)
        {
            float v = _audio[i];
            if (float.IsNaN(v))
                v = 0f;                // auf allen Plattformen gleich (im C-Original undefiniert)
            else if (v > 32760f)
                v = 32760f;
            else if (v < -32760f)
                v = -32760f;

            pcm[i] = (short)v;
        }
    }
}
