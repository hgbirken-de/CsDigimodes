namespace DigitalVoice.SoftwareVocoder;

/// <summary>
/// Synthese der Sprache aus MBE-Parametern: Port von <c>mbelib.c</c> (Copyright (C) 2010 mbelib Author, ISC-Lizenz).
/// Die Gleitkomma-Ausdrücke sind so übernommen, wie sie im C-Original stehen (float bzw. double an denselben Stellen),
/// damit das Ergebnis dem Original entspricht.
/// </summary>
internal static class MbeLib
{
    private const int N = 160;   // Samples je Frame

    /// <summary>Kopiert <paramref name="cur"/> nach <paramref name="prev"/> (<c>mbe_moveMbeParms</c>).</summary>
    public static void MoveParms(MbeParms cur, MbeParms prev)
    {
        prev.W0 = cur.W0;
        prev.L = cur.L;
        prev.K = cur.K;
        prev.Ml[0] = 0f;
        prev.Gamma = cur.Gamma;
        prev.Repeat = cur.Repeat;
        for (int l = 0; l <= 56; l++)
        {
            prev.Ml[l] = cur.Ml[l];
            prev.Vl[l] = cur.Vl[l];
            prev.Log2Ml[l] = cur.Log2Ml[l];
            prev.PHIl[l] = cur.PHIl[l];
            prev.PSIl[l] = cur.PSIl[l];
        }
    }

    /// <summary>Übernimmt die letzten Parameter erneut (<c>mbe_useLastMbeParms</c>): kopiert <paramref name="prev"/> nach <paramref name="cur"/>.</summary>
    public static void UseLastParms(MbeParms cur, MbeParms prev)
    {
        cur.W0 = prev.W0;
        cur.L = prev.L;
        cur.K = prev.K;
        cur.Ml[0] = 0f;
        cur.Gamma = prev.Gamma;
        cur.Repeat = prev.Repeat;
        for (int l = 0; l <= 56; l++)
        {
            cur.Ml[l] = prev.Ml[l];
            cur.Vl[l] = prev.Vl[l];
            cur.Log2Ml[l] = prev.Log2Ml[l];
            cur.PHIl[l] = prev.PHIl[l];
            cur.PSIl[l] = prev.PSIl[l];
        }
    }

    /// <summary>Setzt alle Parameter auf den Anfangszustand (<c>mbe_initMbeParms</c>).</summary>
    public static void InitParms(MbeParms cur, MbeParms prev, MbeParms prevEnhanced)
    {
        prev.W0 = (float)0.09378;
        prev.L = 30;
        prev.K = 10;
        prev.Gamma = 0f;
        for (int l = 0; l <= 56; l++)
        {
            prev.Ml[l] = 0f;
            prev.Vl[l] = 0;
            prev.Log2Ml[l] = 0f;   // log2 von 1 ist 0
            prev.PHIl[l] = 0f;
            prev.PSIl[l] = (float)(Math.PI / 2.0f);
        }
        prev.Repeat = 0;
        MoveParms(prev, cur);
        MoveParms(prev, prevEnhanced);
    }

    /// <summary>Hebt die Spektralbeträge an (<c>mbe_spectralAmpEnhance</c>).</summary>
    public static void SpectralAmpEnhance(MbeParms cur)
    {
        Span<float> Wl = stackalloc float[57];

        float Rm0 = 0;
        float Rm1 = 0;
        for (int l = 1; l <= cur.L; l++)
        {
            Rm0 = Rm0 + (cur.Ml[l] * cur.Ml[l]);
            Rm1 = Rm1 + ((cur.Ml[l] * cur.Ml[l]) * MathF.Cos(cur.W0 * (float)l));
        }

        float R2m0 = Rm0 * Rm0;
        float R2m1 = Rm1 * Rm1;

        for (int l = 1; l <= cur.L; l++)
        {
            if (cur.Ml[l] != 0)
            {
                // Im C-Original: (float)0.96 * M_PI ist double, der ganze Bruch wird in double gerechnet und erst
                // für powf wieder zu float.
                double quotient = ((double)(float)0.96 * Math.PI * ((R2m0 + R2m1) - (2f * Rm0 * Rm1 * MathF.Cos(cur.W0 * (float)l))))
                                  / (cur.W0 * Rm0 * (R2m0 - R2m1));
                Wl[l] = MathF.Sqrt(cur.Ml[l]) * MathF.Pow((float)quotient, 0.25f);

                if ((8 * l) <= cur.L)
                {
                }
                else if (Wl[l] > 1.2)
                {
                    cur.Ml[l] = (float)(1.2 * cur.Ml[l]);
                }
                else if (Wl[l] < 0.5)
                {
                    cur.Ml[l] = (float)(0.5 * cur.Ml[l]);
                }
                else
                {
                    cur.Ml[l] = Wl[l] * cur.Ml[l];
                }
            }
        }

        // Skalierungsfaktor
        float sum = 0;
        for (int l = 1; l <= cur.L; l++)
        {
            float M = cur.Ml[l];
            if (M < 0)
                M = -M;
            sum += M * M;
        }

        float gamma = sum == 0 ? 1.0f : MathF.Sqrt(Rm0 / sum);

        for (int l = 1; l <= cur.L; l++)
            cur.Ml[l] = gamma * cur.Ml[l];
    }

    /// <summary>Stille (160 Nullen) erzeugen (<c>mbe_synthesizeSilencef</c>).</summary>
    public static void SynthesizeSilence(Span<float> output)
    {
        for (int n = 0; n < N; n++)
            output[n] = 0f;
    }

    /// <summary>
    /// Erzeugt 160 Samples Sprache aus den aktuellen und den vorigen Parametern (<c>mbe_synthesizeSpeechf</c>).
    /// </summary>
    public static void SynthesizeSpeech(Span<float> output, MbeParms cur, MbeParms prev, int uvquality, NoiseSource noise)
    {
        float[] Ws = AmbeTables.Ws;
        Span<float> rphase = stackalloc float[64];
        Span<float> rphase2 = stackalloc float[64];

        float uvthresholdf = 2700f;
        float uvthreshold = (float)((uvthresholdf * Math.PI) / 4000f);

        // Gewichte für stimmhaft/stimmlos
        float uvsine = (float)((float)1.3591409 * Math.E);
        float uvrand = 2.0f;

        if (uvquality < 1 || uvquality > 64)
            uvquality = 3;

        float loguvquality = uvquality == 1
            ? (float)(1f / Math.E)
            : (float)(Math.Log((double)uvquality) / (double)uvquality);

        float uvstep = 1.0f / (float)uvquality;
        float qfactor = loguvquality;
        float uvoffset = (uvstep * (float)(uvquality - 1)) / 2f;

        // Anzahl der stimmlosen Bänder
        int numUv = 0;
        for (int l = 1; l <= cur.L; l++)
        {
            if (cur.Vl[l] == 0)
                numUv++;
        }

        float cw0 = cur.W0;
        float pw0 = prev.W0;

        for (int n = 0; n < N; n++)
            output[n] = 0f;

        // Gleichungen 128 und 129
        int maxl;
        if (cur.L > prev.L)
        {
            maxl = cur.L;
            for (int l = prev.L + 1; l <= maxl; l++)
            {
                prev.Ml[l] = 0f;
                prev.Vl[l] = 1;
            }
        }
        else
        {
            maxl = prev.L;
            for (int l = cur.L + 1; l <= maxl; l++)
            {
                cur.Ml[l] = 0f;
                cur.Vl[l] = 1;
            }
        }

        // Phasen aus Gleichungen 139 und 140
        for (int l = 1; l <= 56; l++)
        {
            cur.PSIl[l] = prev.PSIl[l] + ((pw0 + cw0) * ((float)(l * N) / 2f));
            if (l <= (int)(cur.L / 4))
                cur.PHIl[l] = cur.PSIl[l];
            else
                cur.PHIl[l] = cur.PSIl[l] + ((numUv * noise.RandPhase()) / cur.L);
        }

        for (int l = 1; l <= maxl; l++)
        {
            float cw0l = cw0 * (float)l;
            float pw0l = pw0 * (float)l;

            if (cur.Vl[l] == 0 && prev.Vl[l] == 1)
            {
                for (int i = 0; i < uvquality; i++)
                    rphase[i] = noise.RandPhase();

                for (int n = 0; n < N; n++)
                {
                    // Gleichung 131
                    float C1 = Ws[n + N] * prev.Ml[l] * MathF.Cos((pw0l * (float)n) + prev.PHIl[l]);
                    float C3 = 0;
                    for (int i = 0; i < uvquality; i++)
                    {
                        C3 = C3 + MathF.Cos((cw0 * (float)n * ((float)l + ((float)i * uvstep) - uvoffset)) + rphase[i]);
                        if (cw0l > uvthreshold)
                            C3 = C3 + ((cw0l - uvthreshold) * uvrand * noise.Rand());
                    }
                    C3 = C3 * uvsine * Ws[n] * cur.Ml[l] * qfactor;
                    output[n] = output[n] + C1 + C3;
                }
            }
            else if (cur.Vl[l] == 1 && prev.Vl[l] == 0)
            {
                for (int i = 0; i < uvquality; i++)
                    rphase[i] = noise.RandPhase();

                for (int n = 0; n < N; n++)
                {
                    // Gleichung 132
                    float C1 = Ws[n] * cur.Ml[l] * MathF.Cos((cw0l * (float)(n - N)) + cur.PHIl[l]);
                    float C3 = 0;
                    for (int i = 0; i < uvquality; i++)
                    {
                        C3 = C3 + MathF.Cos((pw0 * (float)n * ((float)l + ((float)i * uvstep) - uvoffset)) + rphase[i]);
                        if (pw0l > uvthreshold)
                            C3 = C3 + ((pw0l - uvthreshold) * uvrand * noise.Rand());
                    }
                    C3 = C3 * uvsine * Ws[n + N] * prev.Ml[l] * qfactor;
                    output[n] = output[n] + C1 + C3;
                }
            }
            else if (cur.Vl[l] == 1 || prev.Vl[l] == 1)
            {
                for (int n = 0; n < N; n++)
                {
                    // Gleichung 133-1
                    float C1 = Ws[n + N] * prev.Ml[l] * MathF.Cos((pw0l * (float)n) + prev.PHIl[l]);
                    // Gleichung 133-2
                    float C2 = Ws[n] * cur.Ml[l] * MathF.Cos((cw0l * (float)(n - N)) + cur.PHIl[l]);
                    output[n] = output[n] + C1 + C2;
                }
            }
            else
            {
                for (int i = 0; i < uvquality; i++)
                    rphase[i] = noise.RandPhase();
                for (int i = 0; i < uvquality; i++)
                    rphase2[i] = noise.RandPhase();

                for (int n = 0; n < N; n++)
                {
                    float C3 = 0;
                    for (int i = 0; i < uvquality; i++)
                    {
                        C3 = C3 + MathF.Cos((pw0 * (float)n * ((float)l + ((float)i * uvstep) - uvoffset)) + rphase[i]);
                        if (pw0l > uvthreshold)
                            C3 = C3 + ((pw0l - uvthreshold) * uvrand * noise.Rand());
                    }
                    C3 = C3 * uvsine * Ws[n + N] * prev.Ml[l] * qfactor;

                    float C4 = 0;
                    for (int i = 0; i < uvquality; i++)
                    {
                        C4 = C4 + MathF.Cos((cw0 * (float)n * ((float)l + ((float)i * uvstep) - uvoffset)) + rphase2[i]);
                        if (cw0l > uvthreshold)
                            C4 = C4 + ((cw0l - uvthreshold) * uvrand * noise.Rand());
                    }
                    C4 = C4 * uvsine * Ws[n] * cur.Ml[l] * qfactor;

                    output[n] = output[n] + C3 + C4;
                }
            }
        }
    }
}
