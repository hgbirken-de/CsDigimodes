namespace DigitalVoice.SoftwareVocoder;

/// <summary>
/// Dekodierung eines AMBE-Frames in MBE-Parameter und Sprache. Port von <c>ambe3600x2450.c</c> (DMR, YSF, NXDN: AMBE+2) und
/// <c>ambe3600x2400.c</c> (D-STAR) der mbelib (Copyright (C) 2010 mbelib Author, ISC-Lizenz). Beide Varianten sind bis auf
/// die Bitzuordnung und die Tabellen gleich. Gleitkomma-Ausdrücke wie im C-Original (float/double an denselben Stellen).
/// </summary>
internal static class AmbeFrameDecoder
{
    /// <summary>Ein Frame aus 4 Codewörtern (C0 bis C3) zu je 24 Bit: <c>ambe_fr[4][24]</c> des C-Originals, abgeflacht.</summary>
    public const int FrameBits = 4 * 24;

    // ------------------------------------------------------------------
    // Fehlerkorrektur und Demodulation (für beide Varianten identisch)
    // ------------------------------------------------------------------

    /// <summary>Golay-Korrektur von C0 (<c>mbe_eccAmbe3600x2450C0</c>).</summary>
    public static int EccC0(Span<int> frame)
    {
        Span<int> input = stackalloc int[23];
        Span<int> output = stackalloc int[23];

        for (int j = 0; j < 23; j++)
            input[j] = frame[0 * 24 + j + 1];

        int errs = Ecc.Golay2312(input, output);

        for (int j = 0; j < 23; j++)
            frame[0 * 24 + j + 1] = output[j];

        return errs;
    }

    /// <summary>Entnimmt die 49 Datenbits und korrigiert dabei C1 (<c>mbe_eccAmbe3600x2450Data</c>).</summary>
    public static int EccData(ReadOnlySpan<int> frame, Span<int> ambeD)
    {
        Span<int> gin = stackalloc int[24];
        Span<int> gout = stackalloc int[24];
        int a = 0;

        // C0 unverändert
        for (int j = 23; j > 11; j--)
            ambeD[a++] = frame[0 * 24 + j];

        // C1 korrigieren
        for (int j = 0; j < 23; j++)
            gin[j] = frame[1 * 24 + j];
        int errs = Ecc.Golay2312(gin, gout);
        for (int j = 22; j > 10; j--)
            ambeD[a++] = gout[j];

        // C2 unverändert
        for (int j = 10; j >= 0; j--)
            ambeD[a++] = frame[2 * 24 + j];

        // C3 unverändert
        for (int j = 13; j >= 0; j--)
            ambeD[a++] = frame[3 * 24 + j];

        return errs;
    }

    /// <summary>Hebt die Pseudo-Zufallsmodulation von C1 auf (<c>mbe_demodulateAmbe3600x2450Data</c>).</summary>
    public static void Demodulate(Span<int> frame)
    {
        Span<ushort> pr = stackalloc ushort[115];
        ushort foo = 0;

        for (int i = 23; i >= 12; i--)
        {
            foo <<= 1;
            foo |= (ushort)frame[0 * 24 + i];
        }

        pr[0] = (ushort)(16 * foo);
        for (int i = 1; i < 24; i++)
            pr[i] = (ushort)((173 * pr[i - 1]) + 13849 - (65536 * (((173 * pr[i - 1]) + 13849) / 65536)));
        for (int i = 1; i < 24; i++)
            pr[i] = (ushort)(pr[i] / 32768);

        int k = 1;
        for (int j = 22; j >= 0; j--)
        {
            frame[1 * 24 + j] = frame[1 * 24 + j] ^ pr[k];
            k++;
        }
    }

    // ------------------------------------------------------------------
    // Parameter aus den 49 Datenbits
    // ------------------------------------------------------------------

    private enum Variant { Ambe2450, Ambe2400 }

    /// <summary>AMBE+2 3600x2450 (DMR, YSF, NXDN): <c>mbe_decodeAmbe2450Parms</c>.</summary>
    public static int DecodeParms2450(ReadOnlySpan<int> ambeD, MbeParms cur, MbeParms prev) =>
        DecodeParms(Variant.Ambe2450, ambeD, cur, prev);

    /// <summary>AMBE 3600x2400 (D-STAR): <c>mbe_decodeAmbe2400Parms</c>.</summary>
    public static int DecodeParms2400(ReadOnlySpan<int> ambeD, MbeParms cur, MbeParms prev) =>
        DecodeParms(Variant.Ambe2400, ambeD, cur, prev);

    /// <returns>0 = Sprache, 2 = Erasure-Frame, 3 = Tonframe.</returns>
    private static int DecodeParms(Variant variant, ReadOnlySpan<int> d, MbeParms cur, MbeParms prev)
    {
        bool v2450 = variant == Variant.Ambe2450;

        Span<int> intkl = stackalloc int[57];
        Span<float> flokl = stackalloc float[57];
        Span<float> deltal = stackalloc float[57];
        Span<float> Tl = stackalloc float[57];
        Span<float> Gm = stackalloc float[9];
        Span<float> Ri = stackalloc float[9];
        Span<float> Cik = stackalloc float[5 * 18];   // Cik[i][k] = Cik[i * 18 + k]
        Span<int> Ji = stackalloc int[5];

        int silence = 0;
        float f0;
        int L;

        cur.Repeat = prev.Repeat;

        // --- b0: Grundfrequenz -----------------------------------------------------------------
        int b0;
        if (v2450)
        {
            b0 = (d[0] << 6) | (d[1] << 5) | (d[2] << 4) | (d[3] << 3) | (d[37] << 2) | (d[38] << 1) | d[39];

            if (b0 >= 120 && b0 <= 123)        // Erasure
                return 2;

            if (b0 == 124 || b0 == 125)        // Stille
            {
                silence = 1;
                cur.W0 = (float)((2f * Math.PI) / 32f);
                f0 = 1f / 32f;
                L = 14;
                cur.L = 14;
                for (int l = 1; l <= L; l++)
                    cur.Vl[l] = 0;
            }
            else if (b0 == 126 || b0 == 127)   // Ton
            {
                return 3;
            }
            else
            {
                f0 = AmbeTables.AmbeW0table[b0];
                cur.W0 = (float)((f0 * 2f) * Math.PI);
                L = 0;
            }
        }
        else
        {
            b0 = (d[0] << 6) | (d[1] << 5) | (d[2] << 4) | (d[3] << 3) | (d[4] << 2) | (d[5] << 1) | d[48];

            if ((b0 & 0x7E) == 0x7E)           // Tonframe (die Tonfrequenz wird nicht ausgewertet)
                return 3;

            f0 = MathF.Pow(2f, (float)(-4.311767578125 - (2.1336e-2 * ((float)b0 + 0.5))));
            cur.W0 = (float)((f0 * 2f) * Math.PI);
            L = 0;
        }

        float unvc = (float)0.2046 / MathF.Sqrt(cur.W0);

        // --- L aus der Tabelle (die Tabellen sind float, L ist int: Abschneiden wie in C) ---------
        if (silence == 0)
        {
            L = v2450 ? (int)AmbeTables.AmbeLtable[b0] : (int)AmbeTables.AmbePlusLtable[b0];
            cur.L = L;
        }

        // --- V/UV --------------------------------------------------------------------------------
        int b1;
        if (v2450)
            b1 = (d[4] << 4) | (d[5] << 3) | (d[6] << 2) | (d[7] << 1) | d[35];
        else
            b1 = (d[38] << 3) | (d[39] << 2) | (d[40] << 1) | d[41];

        for (int l = 1; l <= L; l++)
        {
            int jl = (int)((float)l * 16.0f * f0);
            if (silence == 0)
                cur.Vl[l] = v2450 ? AmbeTables.AmbeVuv[b1 * 8 + jl] : AmbeTables.AmbePlusVuv[b1 * 8 + jl];
        }

        // --- Verstärkung -------------------------------------------------------------------------
        int b2;
        float deltaGamma;
        if (v2450)
        {
            b2 = (d[8] << 4) | (d[9] << 3) | (d[10] << 2) | (d[11] << 1) | d[36];
            deltaGamma = AmbeTables.AmbeDg[b2];
        }
        else
        {
            b2 = (d[6] << 5) | (d[7] << 4) | (d[8] << 3) | (d[9] << 2) | (d[42] << 1) | d[43];
            deltaGamma = AmbeTables.AmbePlusDg[b2];
        }
        cur.Gamma = deltaGamma + ((float)0.5 * prev.Gamma);

        // --- PRBA-Vektoren -----------------------------------------------------------------------
        Gm[1] = 0;

        int b3;
        int b4;
        if (v2450)
        {
            b3 = (d[12] << 8) | (d[13] << 7) | (d[14] << 6) | (d[15] << 5) | (d[16] << 4) | (d[17] << 3) | (d[18] << 2) | (d[19] << 1) | d[40];
            Gm[2] = AmbeTables.AmbePRBA24[b3 * 3 + 0];
            Gm[3] = AmbeTables.AmbePRBA24[b3 * 3 + 1];
            Gm[4] = AmbeTables.AmbePRBA24[b3 * 3 + 2];

            b4 = (d[20] << 6) | (d[21] << 5) | (d[22] << 4) | (d[23] << 3) | (d[41] << 2) | (d[42] << 1) | d[43];
            Gm[5] = AmbeTables.AmbePRBA58[b4 * 4 + 0];
            Gm[6] = AmbeTables.AmbePRBA58[b4 * 4 + 1];
            Gm[7] = AmbeTables.AmbePRBA58[b4 * 4 + 2];
            Gm[8] = AmbeTables.AmbePRBA58[b4 * 4 + 3];
        }
        else
        {
            b3 = (d[10] << 8) | (d[11] << 7) | (d[12] << 6) | (d[13] << 5) | (d[14] << 4) | (d[15] << 3) | (d[16] << 2) | (d[44] << 1) | d[45];
            Gm[2] = AmbeTables.AmbePlusPRBA24[b3 * 3 + 0];
            Gm[3] = AmbeTables.AmbePlusPRBA24[b3 * 3 + 1];
            Gm[4] = AmbeTables.AmbePlusPRBA24[b3 * 3 + 2];

            b4 = (d[17] << 6) | (d[18] << 5) | (d[19] << 4) | (d[20] << 3) | (d[21] << 2) | (d[46] << 1) | d[47];
            Gm[5] = AmbeTables.AmbePlusPRBA58[b4 * 4 + 0];
            Gm[6] = AmbeTables.AmbePlusPRBA58[b4 * 4 + 1];
            Gm[7] = AmbeTables.AmbePlusPRBA58[b4 * 4 + 2];
            Gm[8] = AmbeTables.AmbePlusPRBA58[b4 * 4 + 3];
        }

        // Ri berechnen
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

        // Die ersten beiden Elemente jedes Ci,k-Blocks aus dem PRBA-Vektor
        float rconst = (float)(1f / (2f * Math.Sqrt(2.0)));
        Cik[1 * 18 + 1] = 0.5f * (Ri[1] + Ri[2]);
        Cik[1 * 18 + 2] = rconst * (Ri[1] - Ri[2]);
        Cik[2 * 18 + 1] = 0.5f * (Ri[3] + Ri[4]);
        Cik[2 * 18 + 2] = rconst * (Ri[3] - Ri[4]);
        Cik[3 * 18 + 1] = 0.5f * (Ri[5] + Ri[6]);
        Cik[3 * 18 + 2] = rconst * (Ri[5] - Ri[6]);
        Cik[4 * 18 + 1] = 0.5f * (Ri[7] + Ri[8]);
        Cik[4 * 18 + 2] = rconst * (Ri[7] - Ri[8]);

        // --- HOC ---------------------------------------------------------------------------------
        int b5, b6, b7, b8;
        if (v2450)
        {
            b5 = (d[24] << 4) | (d[25] << 3) | (d[26] << 2) | (d[27] << 1) | d[44];
            b6 = (d[28] << 3) | (d[29] << 2) | (d[30] << 1) | d[45];
            b7 = (d[31] << 3) | (d[32] << 2) | (d[33] << 1) | d[46];
            b8 = (d[34] << 2) | (d[47] << 1) | d[48];
        }
        else
        {
            b5 = (d[22] << 3) | (d[23] << 2) | (d[25] << 1) | d[26];
            b6 = (d[27] << 3) | (d[28] << 2) | (d[29] << 1) | d[30];
            b7 = (d[31] << 3) | (d[32] << 2) | (d[33] << 1) | d[34];
            b8 = (d[35] << 3) | (d[36] << 2) | (d[37] << 1);   // das niederwertigste Bit ist hier immer 0
        }

        int[] lmprbl = v2450 ? AmbeTables.AmbeLmprbl : AmbeTables.AmbePlusLmprbl;
        Ji[1] = lmprbl[L * 4 + 0];
        Ji[2] = lmprbl[L * 4 + 1];
        Ji[3] = lmprbl[L * 4 + 2];
        Ji[4] = lmprbl[L * 4 + 3];

        float[] hoc5 = v2450 ? AmbeTables.AmbeHOCb5 : AmbeTables.AmbePlusHOCb5;
        float[] hoc6 = v2450 ? AmbeTables.AmbeHOCb6 : AmbeTables.AmbePlusHOCb6;
        float[] hoc7 = v2450 ? AmbeTables.AmbeHOCb7 : AmbeTables.AmbePlusHOCb7;
        float[] hoc8 = v2450 ? AmbeTables.AmbeHOCb8 : AmbeTables.AmbePlusHOCb8;

        // Ci,k aus den HOC-Tabellen laden (3 <= k <= Ji und k <= 6)
        for (int k = 3; k <= Ji[1]; k++)
            Cik[1 * 18 + k] = k > 6 ? 0 : hoc5[b5 * 4 + (k - 3)];
        for (int k = 3; k <= Ji[2]; k++)
            Cik[2 * 18 + k] = k > 6 ? 0 : hoc6[b6 * 4 + (k - 3)];
        for (int k = 3; k <= Ji[3]; k++)
            Cik[3 * 18 + k] = k > 6 ? 0 : hoc7[b7 * 4 + (k - 3)];
        for (int k = 3; k <= Ji[4]; k++)
            Cik[4 * 18 + k] = k > 6 ? 0 : hoc8[b8 * 4 + (k - 3)];

        // inverse DCT jedes Ci,k ergibt ci,j (Tl)
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
                    sum = sum + ((float)ak * Cik[i * 18 + k] * MathF.Cos((float)((Math.PI * (float)(k - 1) * ((float)j - 0.5f)) / (float)ji)));
                }
                Tl[ll] = sum;
                ll++;
            }
        }

        // log2Ml aus dem vorigen log2Ml und ci,j bestimmen. Wenn L > L(-1): vorherige Werte auffüllen
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

        // Teil 1
        float Sum43 = 0;
        for (int l = 1; l <= cur.L; l++)
        {
            // Gleichung 40
            flokl[l] = ((float)prev.L / (float)cur.L) * (float)l;
            intkl[l] = (int)flokl[l];
            // Gleichung 41
            deltal[l] = flokl[l] - (float)intkl[l];
            // Gleichung 43 (der Index intkl+1 kann 57 erreichen; dann ist deltal 0, das C-Original liest dort ein Nachbarfeld)
            Sum43 = Sum43 + ((((float)1 - deltal[l]) * prev.Log2Ml[intkl[l]]) + (deltal[l] * prev.Log2Ml[Math.Min(intkl[l] + 1, 56)]));
        }
        Sum43 = ((float)0.65 / (float)cur.L) * Sum43;

        // Teil 2
        float Sum42 = 0;
        for (int l = 1; l <= cur.L; l++)
            Sum42 += Tl[l];
        Sum42 = Sum42 / (float)cur.L;
        float BigGamma = (float)((double)cur.Gamma - ((double)0.5f * (Math.Log((double)cur.L) / Math.Log(2.0))) - (double)Sum42);

        // Teil 3
        for (int l = 1; l <= cur.L; l++)
        {
            float c1 = (float)0.65 * ((float)1 - deltal[l]) * prev.Log2Ml[intkl[l]];
            float c2 = (float)0.65 * deltal[l] * prev.Log2Ml[Math.Min(intkl[l] + 1, 56)];
            cur.Log2Ml[l] = Tl[l] + c1 + c2 - Sum43 + BigGamma;

            // inverser Logarithmus ergibt die Spektralbeträge
            if (cur.Vl[l] == 1)
                cur.Ml[l] = (float)Math.Exp((double)((float)0.693 * cur.Log2Ml[l]));
            else
                cur.Ml[l] = (float)(unvc * Math.Exp((double)((float)0.693 * cur.Log2Ml[l])));
        }

        return 0;
    }

    // ------------------------------------------------------------------
    // Ein Frame verarbeiten
    // ------------------------------------------------------------------

    /// <summary>
    /// Wertet die 49 Datenbits aus und erzeugt 160 Samples (<c>mbe_processAmbe2450Dataf</c> bzw.
    /// <c>mbe_processAmbe2400Dataf</c>).
    /// </summary>
    /// <param name="errs2">Anzahl der in den Codewörtern korrigierten Fehler (Eingabe, wird nicht verändert).</param>
    public static void ProcessData(bool is2450, Span<float> output, int errs2, ReadOnlySpan<int> ambeD,
        MbeParms cur, MbeParms prev, MbeParms prevEnhanced, int uvquality, NoiseSource noise)
    {
        int bad = is2450 ? DecodeParms2450(ambeD, cur, prev) : DecodeParms2400(ambeD, cur, prev);

        if (bad == 2)
        {
            cur.Repeat = 0;                       // Erasure-Frame
        }
        else if (bad == 3)
        {
            cur.Repeat = 0;                       // Tonframe
        }
        else if (errs2 > 3)
        {
            MbeLib.UseLastParms(cur, prev);
            cur.Repeat++;
        }
        else
        {
            cur.Repeat = 0;
        }

        if (bad == 0)
        {
            if (cur.Repeat <= 3)
            {
                MbeLib.MoveParms(cur, prev);
                MbeLib.SpectralAmpEnhance(cur);
                MbeLib.SynthesizeSpeech(output, cur, prevEnhanced, uvquality, noise);
                MbeLib.MoveParms(cur, prevEnhanced);
            }
            else
            {
                MbeLib.SynthesizeSilence(output);
                MbeLib.InitParms(cur, prev, prevEnhanced);
            }
        }
        else
        {
            MbeLib.SynthesizeSilence(output);
            MbeLib.InitParms(cur, prev, prevEnhanced);
        }
    }
}
