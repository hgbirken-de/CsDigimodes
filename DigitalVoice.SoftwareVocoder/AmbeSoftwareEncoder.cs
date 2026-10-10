namespace DigitalVoice.SoftwareVocoder;

/// <summary>
/// Software-Kodierer für AMBE-Frames (DMR, YSF, FCS, NXDN, D-STAR): wandelt 160 PCM-Samples (20 ms, 8 kHz, 16 Bit) in einen
/// AMBE-Frame um. Port von <c>encode_ambe</c> und den <c>encode_*</c>-Funktionen des Plugins aus DroidStar
/// (<c>mbe/vocoder_plugin.cpp</c>, Copyright (C) 2010 mbelib Author, ISC-Lizenz). Die eigentliche Sprachanalyse (Tonhöhe,
/// Stimmhaft/Stimmlos, Spektralamplituden) liefert ein <see cref="IImbeAnalyzer"/>; seine Implementierung <c>ImbeAnalyzer</c>
/// steht im Projekt <c>DigitalVoice.SoftwareVocoder.Imbe</c> (GPL).
/// <para>
/// Jede Instanz hat den Zustand eines Datenstroms (die Analyse hat Gedächtnis, und die Quantisierung sagt aus dem vorigen
/// Frame voraus). Pro Datenstrom eine eigene Instanz, nicht aus mehreren Threads gleichzeitig aufrufen, zu Beginn eines neuen
/// Streams <see cref="Reset"/>.
/// </para>
/// <para>
/// Die Qualität liegt hörbar unter der des DVSI-Chips. AMBE ist ein Verfahren der Firma DVSI: Ob Patente oder Lizenzen zu
/// beachten sind, wenn man dieses Programm weitergibt, muss man selbst klären.
/// </para>
/// </summary>
public sealed class AmbeSoftwareEncoder
{
    private static readonly float Sqrt2 = MathF.Sqrt(2.0f);

    private readonly IImbeAnalyzer _analyzer;
    private readonly ImbeParameters _imbe = new();
    private readonly MbeParms _cur = new();
    private readonly MbeParms _prev = new();
    private readonly MbeParms _prevEnhanced = new();
    private readonly int[] _b = new int[9];
    private readonly int[] _lastB = new int[9];

    public AmbeSoftwareEncoder(IImbeAnalyzer analyzer)
    {
        _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
        Reset();
    }

    /// <summary>Setzt den Zustand auf den Anfang eines Datenstroms zurück.</summary>
    public void Reset()
    {
        _analyzer.Reset();
        MbeLib.InitParms(_cur, _prev, _prevEnhanced);
        Array.Clear(_lastB);
    }

    /// <summary>DMR: AMBE+2 3600x2450 mit FEC, 72 Bit (9 Byte) je Frame (<c>encode_2450x1150</c>).</summary>
    public void Encode2450x1150(ReadOnlySpan<short> pcm, Span<byte> ambe)
    {
        CheckArguments(pcm, ambe, 9);

        Span<byte> tmp = stackalloc byte[9];
        tmp.Clear();
        ambe[..9].Clear();
        Encode49(pcm, tmp);

        uint aOrig = 0, bOrig = 0, cOrig = 0;
        uint mask = 0x000800u;
        for (int i = 0; i < 12; i++, mask >>= 1)
        {
            if (ReadBit(tmp, i)) aOrig |= mask;
            if (ReadBit(tmp, i + 12)) bOrig |= mask;
        }

        mask = 0x1000000u;
        for (int i = 0; i < 25; i++, mask >>= 1)
        {
            if (ReadBit(tmp, i + 24)) cOrig |= mask;
        }

        uint a = Golay24Encode(aOrig);

        // The PRNG
        uint p = AmbeEncodeTables.PrngTable[aOrig] >> 1;

        uint b = Golay24Encode(bOrig) >> 1;   // golay_23_encode
        b ^= p;

        mask = 0x800000u;
        for (int i = 0; i < 24; i++, mask >>= 1)
            WriteBit(ambe, (int)AmbeEncodeTables.ATable[i], (a & mask) != 0);

        mask = 0x400000u;
        for (int i = 0; i < 23; i++, mask >>= 1)
            WriteBit(ambe, (int)AmbeEncodeTables.BTable[i], (b & mask) != 0);

        mask = 0x1000000u;
        for (int i = 0; i < 25; i++, mask >>= 1)
            WriteBit(ambe, (int)AmbeEncodeTables.CTable[i], (cOrig & mask) != 0);
    }

    /// <summary>
    /// YSF, FCS und NXDN: AMBE+2 2450 ohne FEC, 49 Bit (7 Byte) je Frame (<c>encode_2450</c>) in der <b>Luft-Reihenfolge</b>.
    /// Für die Reihenfolge des DVSI-Chips (Channel-Paket der Clients) <see cref="Encode2450DvsiOrder"/> verwenden.
    /// </summary>
    public void Encode2450(ReadOnlySpan<short> pcm, Span<byte> ambe)
    {
        CheckArguments(pcm, ambe, 7);
        ambe[..7].Clear();
        Encode49(pcm, ambe);
    }

    /// <summary>Wie <see cref="Encode2450"/>, die 49 Bits stehen aber in der Reihenfolge des DVSI-Chips.</summary>
    public void Encode2450DvsiOrder(ReadOnlySpan<short> pcm, Span<byte> ambe)
    {
        CheckArguments(pcm, ambe, 7);

        Span<byte> air = stackalloc byte[7];
        air.Clear();
        Encode49(pcm, air);
        DvsiBitOrder.ToDvsiOrder(air, ambe);
    }

    /// <summary>D-STAR: AMBE 3600x2400 mit FEC, 72 Bit (9 Byte) je Frame (<c>encode_2400x1200</c>).</summary>
    public void Encode2400x1200(ReadOnlySpan<short> pcm, Span<byte> ambe)
    {
        CheckArguments(pcm, ambe, 9);
        ambe[..9].Clear();

        AnalyzeAndQuantize(pcm, dstar: true);

        Span<byte> tbuf = stackalloc byte[48];
        Span<byte> pbuf = stackalloc byte[48];
        Span<byte> preBuf = stackalloc byte[72];
        Span<byte> ambeFrame = stackalloc byte[72];

        int tbufp = 0;
        for (int i = 0; i < 9; i++)
        {
            StoreReg(_b[i], tbuf.Slice(tbufp, AmbeEncodeTables.BLengths[i]), AmbeEncodeTables.BLengths[i]);
            tbufp += AmbeEncodeTables.BLengths[i];
        }

        for (int i = 0; i < 48; i++)
            pbuf[i] = tbuf[AmbeEncodeTables.MList[i]];

        int u0 = LoadReg(pbuf[..12], 12);
        int u1 = LoadReg(pbuf.Slice(12, 12), 12);

        int m1 = unchecked((int)AmbeEncodeTables.PrngTable[u0]);
        int c0 = (int)Golay24Encode((uint)u0);
        int c1 = (int)Golay24Encode((uint)u1) ^ m1;

        StoreReg(c0, preBuf[..24], 24);
        StoreReg(c1, preBuf.Slice(24, 24), 24);
        pbuf.Slice(24, 24).CopyTo(preBuf.Slice(48, 24));

        for (int i = 0; i < 72; i++)
            ambeFrame[AmbeEncodeTables.DList[i]] = preBuf[i];

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 8; j++)
                ambe[i] |= (byte)(ambeFrame[(i * 8) + j] << j);
        }
    }

    // ------------------------------------------------------------------
    // 49 Bit (AMBE+2 2450, Luft-Reihenfolge)
    // ------------------------------------------------------------------

    private void Encode49(ReadOnlySpan<short> pcm, Span<byte> ambe)
    {
        AnalyzeAndQuantize(pcm, dstar: false);

        int[] b = _b;
        Span<byte> f = stackalloc byte[56];
        f.Clear();

        f[0] = (byte)((b[0] >> 6) & 1);
        f[1] = (byte)((b[0] >> 5) & 1);
        f[2] = (byte)((b[0] >> 4) & 1);
        f[3] = (byte)((b[0] >> 3) & 1);
        f[4] = (byte)((b[1] >> 4) & 1);
        f[5] = (byte)((b[1] >> 3) & 1);
        f[6] = (byte)((b[1] >> 2) & 1);
        f[7] = (byte)((b[1] >> 1) & 1);
        f[8] = (byte)((b[2] >> 4) & 1);
        f[9] = (byte)((b[2] >> 3) & 1);
        f[10] = (byte)((b[2] >> 2) & 1);
        f[11] = (byte)((b[2] >> 1) & 1);
        f[12] = (byte)((b[3] >> 8) & 1);
        f[13] = (byte)((b[3] >> 7) & 1);
        f[14] = (byte)((b[3] >> 6) & 1);
        f[15] = (byte)((b[3] >> 5) & 1);
        f[16] = (byte)((b[3] >> 4) & 1);
        f[17] = (byte)((b[3] >> 3) & 1);
        f[18] = (byte)((b[3] >> 2) & 1);
        f[19] = (byte)((b[3] >> 1) & 1);
        f[20] = (byte)((b[4] >> 6) & 1);
        f[21] = (byte)((b[4] >> 5) & 1);
        f[22] = (byte)((b[4] >> 4) & 1);
        f[23] = (byte)((b[4] >> 3) & 1);
        f[24] = (byte)((b[5] >> 4) & 1);
        f[25] = (byte)((b[5] >> 3) & 1);
        f[26] = (byte)((b[5] >> 2) & 1);
        f[27] = (byte)((b[5] >> 1) & 1);
        f[28] = (byte)((b[6] >> 3) & 1);
        f[29] = (byte)((b[6] >> 2) & 1);
        f[30] = (byte)((b[6] >> 1) & 1);
        f[31] = (byte)((b[7] >> 3) & 1);
        f[32] = (byte)((b[7] >> 2) & 1);
        f[33] = (byte)((b[7] >> 1) & 1);
        f[34] = (byte)((b[8] >> 2) & 1);
        f[35] = (byte)(b[1] & 1);
        f[36] = (byte)(b[2] & 1);
        f[37] = (byte)((b[0] >> 2) & 1);
        f[38] = (byte)((b[0] >> 1) & 1);
        f[39] = (byte)(b[0] & 1);
        f[40] = (byte)(b[3] & 1);
        f[41] = (byte)((b[4] >> 2) & 1);
        f[42] = (byte)((b[4] >> 1) & 1);
        f[43] = (byte)(b[4] & 1);
        f[44] = (byte)(b[5] & 1);
        f[45] = (byte)(b[6] & 1);
        f[46] = (byte)(b[7] & 1);
        f[47] = (byte)((b[8] >> 1) & 1);
        f[48] = (byte)(b[8] & 1);

        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 8; j++)
                ambe[i] |= (byte)(f[(i * 8) + j] << (7 - j));
        }
    }

    // ------------------------------------------------------------------
    // Analyse und Quantisierung
    // ------------------------------------------------------------------

    private void AnalyzeAndQuantize(ReadOnlySpan<short> pcm, bool dstar)
    {
        _analyzer.Analyze(pcm, _imbe);

        if (EncodeAmbe(dstar, 1.0f))
        {
            Array.Copy(_b, _lastB, 9);
        }
        else
        {
            // Die Quantisierung ist nicht möglich (Tonhöhe außerhalb des Bereichs): letzten Frame wiederholen
            Array.Copy(_lastB, _b, 9);
        }
    }

    /// <summary><c>encode_ambe</c>: quantisiert die Analysewerte zu den neun Parametern b[0..8].</summary>
    private bool EncodeAmbe(bool dstar, float gainAdjust)
    {
        ImbeParameters imbe = _imbe;
        MbeParms prevMp = _prev;
        int[] b = _b;

        int b0Lmax = AmbeEncodeTables.B0Lookup.Length;
        int b0I = (imbe.RefPitch >> 5) - 159;
        if (b0I < 0 || b0I >= b0Lmax)
            return false;

        b[0] = AmbeEncodeTables.B0Lookup[b0I];
        int L = dstar ? (int)AmbeTables.AmbePlusLtable[b[0]] : (int)AmbeTables.AmbeLtable[b[0]];

        for (int guard = 0; L != imbe.NumHarms; guard++)
        {
            if (L < imbe.NumHarms)
                b0I++;
            else
                b0I--;

            if (b0I < 0 || b0I >= b0Lmax || guard > 2000)
                return false;

            b[0] = AmbeEncodeTables.B0Lookup[b0I];
            L = dstar ? (int)AmbeTables.AmbePlusLtable[b[0]] : (int)AmbeTables.AmbeLtable[b[0]];
        }

        Span<float> mFloat2 = stackalloc float[56];
        for (int l = 1; l <= L; l++)
        {
            mFloat2[l - 1] = imbe.Sa[l - 1];
            mFloat2[l - 1] = mFloat2[l - 1] * mFloat2[l - 1];
        }

        // b[1]: Stimmhaft/Stimmlos-Muster mit der kleinsten Energie der abweichenden Bänder
        float enMin = 0;
        b[1] = 0;
        int vuvMax = dstar ? 16 : 17;
        for (int n = 0; n < vuvMax; n++)
        {
            float en = 0;
            for (int l = 1; l <= L; l++)
            {
                int jl = dstar
                    ? (int)((float)l * 16.0f * MakeF0(b[0]))
                    : (int)((float)l * 16.0f * AmbeTables.AmbeW0table[b[0]]);

                int kl = 12;
                if (l <= 36)
                    kl = (l + 2) / 3;

                int pattern = dstar ? AmbeTables.AmbePlusVuv[(n * 8) + jl] : AmbeTables.AmbeVuv[(n * 8) + jl];
                if (imbe.VUvDsn[(kl - 1) * 3] != pattern)
                    en += mFloat2[l - 1];
            }

            if (n == 0)
            {
                enMin = en;
            }
            else if (en < enMin)
            {
                b[1] = n;
                enMin = en;
            }
        }

        float numHarmsF = imbe.NumHarms;
        float logL2 = (float)(0.5 * MathF.Log2(numHarmsF));
        float logLW0;
        if (dstar)
            logLW0 = (float)((0.5 * MathF.Log2((float)(numHarmsF * MakeF0(b[0]) * 2.0 * Math.PI))) + 2.289);
        else
            logLW0 = (float)((0.5 * MathF.Log2((float)(numHarmsF * AmbeTables.AmbeW0table[b[0]] * 2.0 * Math.PI))) + 2.289);

        Span<float> lsa = stackalloc float[56];
        float lsaSum = 0.0f;
        for (int i1 = 0; i1 < imbe.NumHarms; i1++)
        {
            float sa = imbe.Sa[i1];
            if (sa < 1) sa = 1.0f;

            if (imbe.VUvDsn[i1] != 0)
                lsa[i1] = logL2 + MathF.Log2(sa);
            else
                lsa[i1] = logLW0 + MathF.Log2(sa);

            lsaSum += lsa[i1];
        }

        // b[2]: Verstärkung
        float gain = lsaSum / numHarmsF;
        float diffGain = dstar ? gain : (float)(gain - (0.5 * prevMp.Gamma));
        diffGain -= gainAdjust;

        float error = 0;
        int errorIndex = 0;
        int maxDg = dstar ? 64 : 32;
        for (int i1 = 0; i1 < maxDg; i1++)
        {
            float diff = MathF.Abs(diffGain - (dstar ? AmbeTables.AmbePlusDg[i1] : AmbeTables.AmbeDg[i1]));
            if (i1 == 0 || diff < error)
            {
                error = diff;
                errorIndex = i1;
            }
        }

        b[2] = errorIndex;

        // Vorhersage aus dem vorigen Frame
        float lPrevL = (float)prevMp.L / numHarmsF;
        prevMp.Log2Ml[0] = prevMp.Log2Ml[1];

        Span<float> T = stackalloc float[56];
        for (int i1 = 0; i1 < imbe.NumHarms; i1++)
        {
            float kl = lPrevL * (float)(i1 + 1);
            int klFloor = (int)kl;
            float klFrac = kl - klFloor;
            float log2A = prevMp.Log2Ml[Math.Min(klFloor, 56)];
            float log2B = prevMp.Log2Ml[Math.Min(klFloor + 1, 56)];

            T[i1] = (float)(lsa[i1] - (0.65 * (1.0 - klFrac) * log2A) - (0.65 * klFrac * log2B));
        }

        int[] lmprbl = dstar ? AmbeTables.AmbePlusLmprbl : AmbeTables.AmbeLmprbl;
        Span<int> J = stackalloc int[4];
        for (int i = 0; i < 4; i++)
            J[i] = lmprbl[(imbe.NumHarms * 4) + i];

        Span<int> cOffset = stackalloc int[4];
        int acc = 0;
        for (int i = 0; i < 4; i++)
        {
            cOffset[i] = acc;
            acc += J[i];
        }

        Span<float> C = stackalloc float[4 * 17];   // C[i][k] = C[i * 17 + k]
        for (int i = 1; i <= 4; i++)
        {
            for (int k = 1; k <= J[i - 1]; k++)
            {
                float s = 0.0f;
                for (int j = 1; j <= J[i - 1]; j++)
                    s += T[cOffset[i - 1] + j - 1] * MathF.Cos((float)((Math.PI * (((float)k) - 1.0) * (((float)j) - 0.5)) / (float)J[i - 1]));

                C[((i - 1) * 17) + k - 1] = s / (float)J[i - 1];
            }
        }

        Span<float> R = stackalloc float[8];
        R[0] = C[0] + (Sqrt2 * C[1]);
        R[1] = C[0] - (Sqrt2 * C[1]);
        R[2] = C[17] + (Sqrt2 * C[18]);
        R[3] = C[17] - (Sqrt2 * C[18]);
        R[4] = C[34] + (Sqrt2 * C[35]);
        R[5] = C[34] - (Sqrt2 * C[35]);
        R[6] = C[51] + (Sqrt2 * C[52]);
        R[7] = C[51] - (Sqrt2 * C[52]);

        Span<float> G = stackalloc float[8];
        for (int m = 1; m <= 8; m++)
        {
            G[m - 1] = 0.0f;
            for (int i = 1; i <= 8; i++)
                G[m - 1] += R[i - 1] * MathF.Cos((float)((Math.PI * (((float)m) - 1.0) * (((float)i) - 0.5)) / 8.0));

            G[m - 1] = (float)(G[m - 1] / 8.0);
        }

        // b[3], b[4]: PRBA-Vektoren
        float[] prba24 = dstar ? AmbeTables.AmbePlusPRBA24 : AmbeTables.AmbePRBA24;
        for (int i = 0; i < 512; i++)
        {
            float err = 0.0f;
            float diff = G[1] - prba24[(i * 3) + 0];
            err += diff * diff;
            diff = G[2] - prba24[(i * 3) + 1];
            err += diff * diff;
            diff = G[3] - prba24[(i * 3) + 2];
            err += diff * diff;

            if (i == 0 || err < error)
            {
                error = err;
                errorIndex = i;
            }
        }

        b[3] = errorIndex;

        float[] prba58 = dstar ? AmbeTables.AmbePlusPRBA58 : AmbeTables.AmbePRBA58;
        for (int i = 0; i < 128; i++)
        {
            float err = 0.0f;
            float diff = G[4] - prba58[(i * 4) + 0];
            err += diff * diff;
            diff = G[5] - prba58[(i * 4) + 1];
            err += diff * diff;
            diff = G[6] - prba58[(i * 4) + 2];
            err += diff * diff;
            diff = G[7] - prba58[(i * 4) + 3];
            err += diff * diff;

            if (i == 0 || err < error)
            {
                error = err;
                errorIndex = i;
            }
        }

        b[4] = errorIndex;

        // b[5] bis b[8]: höhere Koeffizienten
        b[5] = QuantizeHoc(1, J, C, dstar ? AmbeTables.AmbePlusHOCb5 : AmbeTables.AmbeHOCb5, dstar ? 16 : 32);
        b[6] = QuantizeHoc(2, J, C, dstar ? AmbeTables.AmbePlusHOCb6 : AmbeTables.AmbeHOCb6, 16);
        b[7] = QuantizeHoc(3, J, C, dstar ? AmbeTables.AmbePlusHOCb7 : AmbeTables.AmbeHOCb7, 16);
        b[8] = QuantizeHoc(4, J, C, dstar ? AmbeTables.AmbePlusHOCb8 : AmbeTables.AmbeHOCb8, dstar ? 16 : 8);

        // Zustand für die Vorhersage des nächsten Frames nachführen
        DequantizeParms(dstar, b, _cur, _prev);
        MbeLib.MoveParms(_cur, _prev);
        return true;
    }

    /// <summary>Sucht in einer HOC-Tabelle den Eintrag mit dem kleinsten Abstand (b5 bis b8 des Originals).</summary>
    private static int QuantizeHoc(int ii, ReadOnlySpan<int> J, ReadOnlySpan<float> C, float[] table, int entries)
    {
        if (J[ii - 1] <= 2)
            return 0;

        float error = 0;
        int errorIndex = 0;
        for (int n = 0; n < entries; n++)
        {
            float err = 0.0f;
            for (int j = 1; j <= J[ii - 1] - 2 && j <= 4; j++)
            {
                float diff = table[(n * 4) + j - 1] - C[((ii - 1) * 17) + j + 2 - 1];
                err += diff * diff;
            }

            if (n == 0 || err < error)
            {
                error = err;
                errorIndex = n;
            }
        }

        return errorIndex;
    }

    /// <summary><c>make_f0</c>: Grundfrequenz zu b0 bei D-STAR.</summary>
    private static float MakeF0(int b0) =>
        MathF.Pow(2f, (float)(-4.311767578125 - (2.1336e-2 * ((float)b0 + 0.5))));

    // ------------------------------------------------------------------
    // Rückrechnung der quantisierten Parameter (mbe_dequantizeAmbeParms)
    // ------------------------------------------------------------------

    private static int DequantizeParms(bool dstar, int[] b, MbeParms cur, MbeParms prev)
    {
        Span<int> intkl = stackalloc int[57];
        Span<float> flokl = stackalloc float[57];
        Span<float> deltal = stackalloc float[57];
        Span<float> Tl = stackalloc float[57];
        Span<float> Gm = stackalloc float[9];
        Span<float> Ri = stackalloc float[9];
        Span<float> Cik = stackalloc float[5 * 18];
        Span<int> Ji = stackalloc int[5];

        int b0 = b[0], b1 = b[1], b2 = b[2], b3 = b[3], b4 = b[4], b5 = b[5], b6 = b[6], b7 = b[7], b8 = b[8];
        int silence = 0;
        float f0 = 0;
        int L = 0;

        cur.Repeat = prev.Repeat;

        if (b0 >= 120 && b0 <= 123)
            return 2;
        if (b0 == 124 || b0 == 125)
        {
            silence = 1;
            cur.W0 = (float)((2f * Math.PI) / 32f);
            f0 = 1f / 32f;
            L = 14;
            cur.L = 14;
            for (int l = 1; l <= L; l++)
                cur.Vl[l] = 0;
        }
        else if (b0 == 126 || b0 == 127)
        {
            return 3;
        }

        if (silence == 0)
        {
            f0 = dstar ? MakeF0(b0) : AmbeTables.AmbeW0table[b0];
            cur.W0 = (float)((f0 * 2f) * Math.PI);
        }

        float unvc = (float)0.2046 / MathF.Sqrt(cur.W0);

        if (silence == 0)
        {
            L = dstar ? (int)AmbeTables.AmbePlusLtable[b0] : (int)AmbeTables.AmbeLtable[b0];
            cur.L = L;
        }

        for (int l = 1; l <= L; l++)
        {
            int jl = (int)((float)l * 16.0f * f0);
            if (silence == 0)
                cur.Vl[l] = dstar ? AmbeTables.AmbePlusVuv[(b1 * 8) + jl] : AmbeTables.AmbeVuv[(b1 * 8) + jl];
        }

        float deltaGamma = dstar ? AmbeTables.AmbePlusDg[b2] : AmbeTables.AmbeDg[b2];
        cur.Gamma = deltaGamma + ((float)0.5 * prev.Gamma);

        Gm[1] = 0;
        float[] prba24 = dstar ? AmbeTables.AmbePlusPRBA24 : AmbeTables.AmbePRBA24;
        float[] prba58 = dstar ? AmbeTables.AmbePlusPRBA58 : AmbeTables.AmbePRBA58;
        Gm[2] = prba24[(b3 * 3) + 0];
        Gm[3] = prba24[(b3 * 3) + 1];
        Gm[4] = prba24[(b3 * 3) + 2];
        Gm[5] = prba58[(b4 * 4) + 0];
        Gm[6] = prba58[(b4 * 4) + 1];
        Gm[7] = prba58[(b4 * 4) + 2];
        Gm[8] = prba58[(b4 * 4) + 3];

        for (int i = 1; i <= 8; i++)
        {
            float sum = 0;
            for (int m = 1; m <= 8; m++)
            {
                int am = m == 1 ? 1 : 2;
                sum = sum + ((float)am * Gm[m] * MathF.Cos((float)((Math.PI * (float)(m - 1) * ((float)i - 0.5f)) / 8f)));
            }

            Ri[i] = sum;
        }

        float rconst = (float)(1f / (2f * Math.Sqrt(2.0)));
        Cik[(1 * 18) + 1] = 0.5f * (Ri[1] + Ri[2]);
        Cik[(1 * 18) + 2] = rconst * (Ri[1] - Ri[2]);
        Cik[(2 * 18) + 1] = 0.5f * (Ri[3] + Ri[4]);
        Cik[(2 * 18) + 2] = rconst * (Ri[3] - Ri[4]);
        Cik[(3 * 18) + 1] = 0.5f * (Ri[5] + Ri[6]);
        Cik[(3 * 18) + 2] = rconst * (Ri[5] - Ri[6]);
        Cik[(4 * 18) + 1] = 0.5f * (Ri[7] + Ri[8]);
        Cik[(4 * 18) + 2] = rconst * (Ri[7] - Ri[8]);

        int[] lmprbl = dstar ? AmbeTables.AmbePlusLmprbl : AmbeTables.AmbeLmprbl;
        Ji[1] = lmprbl[(L * 4) + 0];
        Ji[2] = lmprbl[(L * 4) + 1];
        Ji[3] = lmprbl[(L * 4) + 2];
        Ji[4] = lmprbl[(L * 4) + 3];

        float[] hoc5 = dstar ? AmbeTables.AmbePlusHOCb5 : AmbeTables.AmbeHOCb5;
        float[] hoc6 = dstar ? AmbeTables.AmbePlusHOCb6 : AmbeTables.AmbeHOCb6;
        float[] hoc7 = dstar ? AmbeTables.AmbePlusHOCb7 : AmbeTables.AmbeHOCb7;
        float[] hoc8 = dstar ? AmbeTables.AmbePlusHOCb8 : AmbeTables.AmbeHOCb8;

        for (int k = 3; k <= Ji[1]; k++)
            Cik[(1 * 18) + k] = k > 6 ? 0 : hoc5[(b5 * 4) + (k - 3)];
        for (int k = 3; k <= Ji[2]; k++)
            Cik[(2 * 18) + k] = k > 6 ? 0 : hoc6[(b6 * 4) + (k - 3)];
        for (int k = 3; k <= Ji[3]; k++)
            Cik[(3 * 18) + k] = k > 6 ? 0 : hoc7[(b7 * 4) + (k - 3)];
        for (int k = 3; k <= Ji[4]; k++)
            Cik[(4 * 18) + k] = k > 6 ? 0 : hoc8[(b8 * 4) + (k - 3)];

        int ll = 1;
        for (int i = 1; i <= 4; i++)
        {
            int ji = Ji[i];
            for (int j = 1; j <= ji; j++)
            {
                float sum = 0;
                for (int k = 1; k <= ji; k++)
                {
                    int ak = k == 1 ? 1 : 2;
                    sum = sum + ((float)ak * Cik[(i * 18) + k] * MathF.Cos((float)((Math.PI * (float)(k - 1) * ((float)j - 0.5f)) / (float)ji)));
                }

                Tl[ll] = sum;
                ll++;
            }
        }

        if (cur.L > prev.L)
        {
            for (int l = prev.L + 1; l <= cur.L; l++)
            {
                prev.Ml[l] = prev.Ml[prev.L];
                prev.Log2Ml[l] = prev.Log2Ml[prev.L];
            }
        }

        prev.Log2Ml[0] = prev.Log2Ml[1];
        prev.Ml[0] = prev.Ml[1];

        float sum43 = 0;
        for (int l = 1; l <= cur.L; l++)
        {
            flokl[l] = ((float)prev.L / (float)cur.L) * (float)l;
            intkl[l] = (int)flokl[l];
            deltal[l] = flokl[l] - (float)intkl[l];
            sum43 = sum43 + ((((float)1 - deltal[l]) * prev.Log2Ml[intkl[l]]) + (deltal[l] * prev.Log2Ml[Math.Min(intkl[l] + 1, 56)]));
        }

        sum43 = ((float)0.65 / (float)cur.L) * sum43;

        float sum42 = 0;
        for (int l = 1; l <= cur.L; l++)
            sum42 += Tl[l];
        sum42 = sum42 / (float)cur.L;

        float bigGamma = (float)((double)cur.Gamma - ((double)0.5f * (Math.Log((double)cur.L) / Math.Log(2.0))) - (double)sum42);

        for (int l = 1; l <= cur.L; l++)
        {
            float c1 = (float)0.65 * ((float)1 - deltal[l]) * prev.Log2Ml[intkl[l]];
            float c2 = (float)0.65 * deltal[l] * prev.Log2Ml[Math.Min(intkl[l] + 1, 56)];
            cur.Log2Ml[l] = Tl[l] + c1 + c2 - sum43 + bigGamma;

            if (cur.Vl[l] == 1)
                cur.Ml[l] = (float)Math.Exp((double)((float)0.693 * cur.Log2Ml[l]));
            else
                cur.Ml[l] = (float)(unvc * Math.Exp((double)((float)0.693 * cur.Log2Ml[l])));
        }

        return 0;
    }

    // ------------------------------------------------------------------
    // Hilfsfunktionen: Golay, Bits
    // ------------------------------------------------------------------

    private static readonly uint[] GolayEncoding =
    [
        0x800C75, 0x40063B, 0x200F68, 0x1007B4, 0x0803DA, 0x040D99, 0x0206CD, 0x010367, 0x008DC6, 0x004A97, 0x00293E, 0x0018EB,
    ];

    /// <summary><c>golay_24_encode</c>.</summary>
    private static uint Golay24Encode(uint codeWordIn)
    {
        uint codeWordOut = 0;
        for (int i = 0; i < 12; i++)
        {
            uint tempWord = codeWordIn & (1u << (11 - i));
            if (tempWord >= 1)
                codeWordOut ^= GolayEncoding[i];
        }

        return codeWordOut;
    }

    private static bool ReadBit(ReadOnlySpan<byte> p, int i) => (p[i >> 3] & (0x80 >> (i & 7))) != 0;

    private static void WriteBit(Span<byte> p, int i, bool value)
    {
        int mask = 0x80 >> (i & 7);
        p[i >> 3] = value ? (byte)(p[i >> 3] | mask) : (byte)(p[i >> 3] & ~mask);
    }

    private static void StoreReg(int reg, Span<byte> val, int len)
    {
        for (int i = 0; i < len; i++)
            val[i] = (byte)((reg >> (len - 1 - i)) & 1);
    }

    private static int LoadReg(ReadOnlySpan<byte> val, int len)
    {
        int acc = 0;
        for (int i = 0; i < len; i++)
            acc = (acc << 1) + (val[i] & 1);
        return acc;
    }

    private static void CheckArguments(ReadOnlySpan<short> pcm, Span<byte> ambe, int length)
    {
        if (pcm.Length < 160) throw new ArgumentException("A frame has 160 samples.", nameof(pcm));
        if (ambe.Length < length) throw new ArgumentException($"The AMBE buffer needs {length} bytes.", nameof(ambe));
    }
}
