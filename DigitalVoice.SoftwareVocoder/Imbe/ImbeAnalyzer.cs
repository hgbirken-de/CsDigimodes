// Port of the analysis part of the "Project 25 IMBE Encoder/Decoder Fixed-Point implementation",
// Developed by Pavel Yazev <pyazev@gmail.com>, Version 1.0 (c) Copyright 2009
// (encode.cc, dc_rmv.cc, pe_lpf.cc, pitch_est.cc, pitch_ref.cc, v_uv_det.cc, dsp_sub.cc in the version contained in DroidStar).
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later
// version. It is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program (LICENSE-GPL.txt).

using static DigitalVoice.SoftwareVocoder.Imbe.Fx;

namespace DigitalVoice.SoftwareVocoder.Imbe;

/// <summary>
/// Sprachanalyse (Festkomma, bitgleich zum C-Original <c>imbe_vocoder</c>): Gleichanteil entfernen, Tiefpass, Tonhöhenschätzung,
/// FFT, Tonhöhenverfeinerung, Stimmhaft/Stimmlos-Entscheidung und Spektralamplituden. Für AMBE werden nur Tonhöhe, Zahl
/// der Harmonischen, Spektralamplituden und die Stimmhaft-Flags gebraucht. Die IMBE-Quantisierung (<c>sa_encode</c>,
/// <c>encode_frame_vector</c>) entfällt, weil sie auf diese Werte keinen Einfluss hat.
/// </summary>
public sealed class ImbeAnalyzer : IImbeAnalyzer
{
    private const int Frame = 160;
    private const int NumHarmsMax = 56;
    private const int NumHarmsMin = 9;
    private const int NumBandsMax = 12;
    private const int PitchEstFrame = 301;
    private const int PitchEstBufSize = 621;
    private const int PeLpfOrd = 21;
    private const int FftLength = 256;
    private const int MinIndex = 50;

    // Konstanten (globals.h, v_uv_det.cc, pitch_est.cc)
    private const int Cnst09254Q016 = 60647;
    private const int Cnst033Q016 = 0x5556;
    private const short Cnst1125Q88 = 0x0120;
    private const short Cnst05Q88 = 0x0080;
    private const short Cnst0125Q88 = 0x0020;
    private const short Cnst025Q88 = 0x0040;
    private const short Cnst08717Q115 = 0x6F94;
    private const short Cnst00031Q115 = 0x0064;
    private const short Cnst048Q412 = 0x07AE;
    private const short Cnst100Q412 = 0x1000;
    private const short Cnst085Q412 = 0x0D9B;
    private const short Cnst04Q412 = 0x0666;
    private const short Cnst005Q412 = 0x00CD;
    private const short Cnst05882Q115 = 0x4B4B;
    private const short Cnst02857Q115 = 0x2492;
    private const short Cnst09900Q115 = 0x7EB8;   // 0.99
    private const short Cnst05625Q115 = 0x4800;
    private const short Cnst045Q115 = 0x3999;
    private const short Cnst01741Q115 = 0x164A;
    private const short Cnst01393Q115 = 0x11D5;
    private const short Cnst001Q115 = 0x0148;
    private const short Cnst00025Q115 = 0x0051;
    private const short Cnst025Q115 = 0x2000;
    private const short CnstPi4Q115 = 0x6488;
    private const short Cnst055Q412 = 0x08CD;

    // Zustand (die Mitglieder von imbe_vocoder_impl)
    private short _prevPitch, _prevPrevPitch, _prevEP, _prevPrevEP;
    private int _thMax;
    private readonly short[] _bandVuv = new short[NumBandsMax];          // v_uv_dsn[NUM_BANDS_MAX] des Objekts
    private readonly short[] _wrArray = new short[(FftLength / 2) + 1];
    private readonly short[] _wiArray = new short[(FftLength / 2) + 1];
    private readonly short[] _pitchEstBuf = new short[PitchEstBufSize];
    private readonly short[] _pitchRefBuf = new short[PitchEstBufSize];
    private int _dcRmvMem;
    private readonly short[] _fftBuf = new short[FftLength * 2];          // fft_buf[i].re = [2i], .im = [2i+1]
    private readonly short[] _peLpfMem = new short[PeLpfOrd];

    // IMBE_PARAM
    private short _pitch, _eP, _refPitch, _numHarms, _numBands;
    private int _fundFreq;
    private readonly short[] _sa = new short[NumHarmsMax];
    private readonly short[] _harmVuv = new short[NumHarmsMax];          // imbe_param->v_uv_dsn

    // Arbeitsfelder (damit pro Frame nichts angelegt wird)
    private readonly short[] _epArr0 = new short[203];
    private readonly short[] _epArr1 = new short[203];
    private readonly short[] _epArr2 = new short[203];
    private readonly short[] _epArr2Min = new short[203];
    private readonly short[] _e1p1E2p2Save = new short[203];
    private readonly short[] _sigWndwed = new short[PitchEstFrame];
    private readonly int[] _corr = new int[259];
    private readonly short[] _spRecRe = new short[FftLength];
    private readonly short[] _spRecIm = new short[FftLength];
    private readonly int[] _indexTbl = new int[64];
    private readonly int[] _mNum = new int[NumHarmsMax];
    private readonly short[] _mDen = new short[NumHarmsMax];

    public ImbeAnalyzer() => Reset();

    /// <inheritdoc />
    public void Reset()
    {
        Array.Clear(_pitchEstBuf);
        Array.Clear(_pitchRefBuf);
        Array.Clear(_peLpfMem);
        Array.Clear(_fftBuf);
        Array.Clear(_sa);
        Array.Clear(_harmVuv);
        Array.Clear(_bandVuv);
        _pitch = _eP = _refPitch = _numHarms = _numBands = 0;
        _fundFreq = 0;

        // pitch_est_init
        _prevPitch = _prevPrevPitch = 158;
        _prevEP = _prevPrevEP = 0;

        FftInit();
        _dcRmvMem = 0;

        // pitch_ref_init
        _thMax = 0;
    }

    /// <inheritdoc />
    public void Analyze(ReadOnlySpan<short> pcm, ImbeParameters result)
    {
        if (pcm.Length < Frame) throw new ArgumentException("A frame has 160 samples.", nameof(pcm));
        ArgumentNullException.ThrowIfNull(result);

        // imbe_vocoder_impl::encode()
        Array.Copy(_pitchEstBuf, Frame, _pitchEstBuf, 0, PitchEstBufSize - Frame);
        Array.Copy(_pitchRefBuf, Frame, _pitchRefBuf, 0, PitchEstBufSize - Frame);

        DcRmv(pcm, _pitchRefBuf, PitchEstBufSize - Frame, Frame);
        PeLpf(_pitchRefBuf, PitchEstBufSize - Frame, _pitchEstBuf, PitchEstBufSize - Frame, Frame);

        PitchEst(_pitchEstBuf);

        // Sprachfensterung und FFT
        int wrIndex = 0;
        int sigIndex = 40;
        for (int i = 146; i < 256; i++)
        {
            _fftBuf[2 * i] = Mult(_pitchRefBuf[sigIndex++], ImbeTables.Wr[wrIndex++]);
            _fftBuf[(2 * i) + 1] = 0;
        }

        _fftBuf[0] = _pitchRefBuf[sigIndex++];
        _fftBuf[1] = 0;
        wrIndex--;
        for (int i = 1; i < 111; i++)
        {
            _fftBuf[2 * i] = Mult(_pitchRefBuf[sigIndex++], ImbeTables.Wr[wrIndex--]);
            _fftBuf[(2 * i) + 1] = 0;
        }

        for (int i = 111; i < 146; i++)
            _fftBuf[2 * i] = _fftBuf[(2 * i) + 1] = 0;

        Fft(_fftBuf, FftLength, 1);

        PitchRef();
        VUvDet();

        result.RefPitch = _refPitch;
        result.NumHarms = _numHarms;
        Array.Copy(_sa, result.Sa, NumHarmsMax);
        Array.Copy(_harmVuv, result.VUvDsn, NumHarmsMax);
    }

    // ------------------------------------------------------------------
    // dc_rmv.cc, pe_lpf.cc
    // ------------------------------------------------------------------

    private void DcRmv(ReadOnlySpan<short> sigin, short[] sigout, int outOffset, int len)
    {
        int mem = _dcRmvMem;
        for (int k = 0; k < len; k++)
        {
            int tmp = LDepositH(sigin[k]);
            mem = LAdd(mem, tmp);
            sigout[outOffset + k] = Round(mem);
            mem = LMpyLs(mem, Cnst09900Q115);
            mem = LSub(mem, tmp);
        }

        _dcRmvMem = mem;
    }

    private void PeLpf(short[] sigin, int inOffset, short[] sigout, int outOffset, int len)
    {
        for (int k = 0; k < len; k++)
        {
            for (int i = 0; i < PeLpfOrd - 1; i++)
                _peLpfMem[i] = _peLpfMem[i + 1];
            _peLpfMem[PeLpfOrd - 1] = sigin[inOffset + k];

            int sum = 0;
            for (int i = 0; i < PeLpfOrd; i++)
                sum = LMac(sum, _peLpfMem[i], ImbeTables.LpfCoef[i]);
            sigout[outOffset + k] = Round(sum);
        }
    }

    // ------------------------------------------------------------------
    // dsp_sub.cc: FFT
    // ------------------------------------------------------------------

    private void FftInit()
    {
        short fftLen2 = Shr(FftLength, 1);
        short shift = NormS(fftLen2);
        short step = Shl(2, shift);
        short theta = 0;

        for (short i = 0; i <= fftLen2; i++)
        {
            _wrArray[i] = CosFxp(theta);
            _wiArray[i] = SinFxp(theta);
            if (i >= fftLen2 - 1)
                theta = OneQ15;
            else
                theta = Add(theta, step);
        }
    }

    /// <summary>
    /// FFT in Festkomma. <paramref name="buf"/> ist <c>datam1</c> des Originals (re und im abwechselnd); das Original
    /// rechnet mit <c>data = &amp;datam1[-1]</c>, also mit 1-basierten Indizes. <c>data[k]</c> ist hier <c>buf[k - 1]</c>.
    /// </summary>
    private void Fft(short[] buf, int nn, int isign)
    {
        short n = Shl((short)nn, 1);
        short j = 1;
        short m, mmax, istep, i, index, indexStep, wr, wi;

        for (i = 1; i < n; i += 2)
        {
            if (j > i)
            {
                (buf[j - 1], buf[i - 1]) = (buf[i - 1], buf[j - 1]);
                (buf[j], buf[i]) = (buf[i], buf[j]);
            }

            m = (short)nn;
            while (m >= 2 && j > m)
            {
                j = Sub(j, m);
                m = Shr(m, 1);
            }

            j = Add(j, m);
        }

        mmax = 2;
        indexStep = (short)nn;
        while (n > mmax)
        {
            istep = Shl(mmax, 1);
            index = 0;
            indexStep = Shr(indexStep, 1);
            wr = OneQ15;
            wi = 0;

            for (m = 1; m < mmax; m += 2)
            {
                for (i = m; i <= n; i += istep)
                {
                    j = (short)(i + mmax);

                    int lTemp1 = LShr(LMult(wr, buf[j - 1]), 1);
                    int lTemp2 = LShr(LMult(wi, buf[j]), 1);
                    int lTempr = LSub(lTemp1, lTemp2);

                    lTemp1 = LShr(LMult(wr, buf[j]), 1);
                    lTemp2 = LShr(LMult(wi, buf[j - 1]), 1);
                    int lTempi = LAdd(lTemp1, lTemp2);

                    lTemp1 = LShr(LDepositH(buf[i - 1]), 1);
                    buf[j - 1] = Round(LSub(lTemp1, lTempr));
                    buf[i - 1] = Round(LAdd(lTemp1, lTempr));

                    lTemp1 = LShr(LDepositH(buf[i]), 1);
                    buf[j] = Round(LSub(lTemp1, lTempi));
                    buf[i] = Round(LAdd(lTemp1, lTempi));
                }

                index = Add(index, indexStep);
                wr = _wrArray[index];
                wi = isign < 0 ? Negate(_wiArray[index]) : _wiArray[index];
            }

            mmax = istep;
        }
    }

    // ------------------------------------------------------------------
    // pitch_est.cc
    // ------------------------------------------------------------------

    private int Autocorr(short[] sigin, short shift, short scaleShift)
    {
        int sum = 0;
        for (short i = 0; i < PitchEstFrame - shift; i++)
            sum = LAdd(sum, LShr(LMult(sigin[i], sigin[i + shift]), scaleShift));
        return sum;
    }

    private void EP(short[] sigin, int offset, short[] resBuf)
    {
        short[] wi = ImbeTables.Wi;
        short i, j, tmp, scaleShift;

        for (i = 0; i < PitchEstFrame; i++)
            _sigWndwed[i] = MultR(sigin[offset + i], wi[i]);

        int lSum = 0;
        for (i = 0; i < PitchEstFrame; i++)
            lSum = LAdd(lSum, LMpyLs(LMult(sigin[offset + i], sigin[offset + i]), wi[i]));

        if (lSum == Max32)
        {
            lSum = 0;
            for (i = 0; i < PitchEstFrame; i++)
                lSum = LAdd(lSum, LMpyLs(LShr(LMult(sigin[offset + i], sigin[offset + i]), 5), wi[i]));
            scaleShift = 5;
        }
        else
        {
            scaleShift = 0;
        }

        int lE0 = 0;
        for (i = 0; i < PitchEstFrame; i++)
            lE0 = LAdd(lE0, LShr(LMult(_sigWndwed[i], _sigWndwed[i]), scaleShift));

        for (tmp = 21, i = 0; tmp <= 150; tmp++, i += 2)
            _corr[i] = Autocorr(_sigWndwed, tmp, scaleShift);

        for (i = 1; i < 258; i += 2)
            _corr[i] = LShr(LAdd(_corr[i - 1], _corr[i + 1]), 1);

        short denPartAcc = Cnst08717Q115;
        short indexStep = 42;
        short indexBeg = 0;
        lE0 = LShr(lE0, 7);

        for (i = 0; i < 203; i++)
        {
            int lTmp = 0;
            j = indexBeg;
            while (j <= 258)
            {
                lTmp = LAdd(lTmp, _corr[j]);
                j = (short)(j + indexStep);
            }

            lTmp = LShr(lTmp, 6);
            lTmp = LAdd(lTmp, lE0);
            lTmp = unchecked(lTmp * indexStep);
            int lNum = LSub(lSum, lTmp);

            indexBeg++;
            indexStep++;

            int lDen = LMpyLs(lSum, denPartAcc);

            if (lNum < lDen && lDen != 0)
            {
                if (lNum <= 0)
                {
                    resBuf[i] = 0;
                }
                else
                {
                    tmp = NormL(lDen);
                    tmp = DivS(ExtractH(LShl(lNum, tmp)), ExtractH(LShl(lDen, tmp)));
                    resBuf[i] = Shr(tmp, 3);
                }
            }
            else if (lNum >= lDen)
            {
                resBuf[i] = Cnst100Q412;
            }
            else
            {
                resBuf[i] = Cnst100Q412;
            }

            denPartAcc = Sub(denPartAcc, Cnst00031Q115);
        }
    }

    private void PitchEst(short[] framesBuf)
    {
        ushort[] minMax = ImbeTables.MinMax;

        EP(framesBuf, 0, _epArr0);

        short minIndex = (short)((minMax[_prevPitch] >> 8) & 0xFF);
        short maxIndex = (short)(minMax[_prevPitch] & 0xFF);
        short p, pb;
        p = pb = minIndex;
        short ePCur = _epArr0[minIndex];

        while (++p <= maxIndex)
        {
            if (_epArr0[p] < ePCur)
            {
                ePCur = _epArr0[p];
                pb = p;
            }
        }

        short ceb = Add(ePCur, Add(_prevEP, _prevPrevEP));

        if (ceb <= Cnst048Q412)
        {
            _prevPrevPitch = _prevPitch;
            _prevPitch = pb;
            _prevPrevEP = _prevEP;
            _prevEP = _epArr0[pb];

            _pitch = (short)(pb + 42);
            _eP = _prevEP;
            return;
        }

        EP(framesBuf, Frame, _epArr1);
        EP(framesBuf, 2 * Frame, _epArr2);

        short p0Est, p0, p1, p2, p1MaxIndex, p2MaxIndex, e1p1E2p2Est, cef, cefEst;
        p0Est = p0 = 0;
        cefEst = (short)(_epArr0[p0] + _epArr1[p0] + _epArr2[p0]);

        p1 = 0;
        while (p1 < 203)
        {
            p2 = (short)((minMax[p1] >> 8) & 0xFF);
            p2MaxIndex = (short)(minMax[p1] & 0xFF);
            short sTmp = _epArr2[p1];
            while (p2 <= p2MaxIndex)
            {
                if (_epArr2[p2] < sTmp)
                    sTmp = _epArr2[p2];
                p2++;
            }

            _epArr2Min[p1] = sTmp;
            p1++;
        }

        while (p0 < 203)
        {
            e1p1E2p2Est = (short)(_epArr1[p0] + _epArr2Min[p0]);
            p1 = (short)((minMax[p0] >> 8) & 0xFF);
            p1MaxIndex = (short)(minMax[p0] & 0xFF);
            while (p1 <= p1MaxIndex)
            {
                if (Add(_epArr1[p1], _epArr2Min[p1]) < e1p1E2p2Est)
                    e1p1E2p2Est = Add(_epArr1[p1], _epArr2Min[p1]);
                p1++;
            }

            _e1p1E2p2Save[p0] = e1p1E2p2Est;
            cef = Add(_epArr0[p0], e1p1E2p2Est);
            if (cef < cefEst)
            {
                cefEst = cef;
                p0Est = p0;
            }

            p0++;
        }

        short pf = p0Est;
        if (pf >= 42)
        {
            short i;
            if (pf < 84) i = 1;
            else if (pf < 126) i = 2;
            else if (pf < 168) i = 3;
            else i = 4;

            ushort pFp = (ushort)((pf + 42) << 8);
            ushort tmp = 0;

            while (i-- != 0)
            {
                switch (i)
                {
                    case 0:
                        tmp = (ushort)(pFp >> 1);
                        break;
                    case 1:
                        tmp = (ushort)(((uint)pFp * 0x5555u) >> 16);
                        break;
                    case 2:
                        tmp = (ushort)(pFp >> 2);
                        break;
                    case 3:
                        tmp = (ushort)(((uint)pFp * 0x3333u) >> 16);
                        break;
                }

                short pIndex = (short)(((tmp + 0x0080) >> 8) - 42);
                if (pIndex < 0 || pIndex > 202)
                    continue;   // im C-Original ein Zugriff außerhalb der Tabelle (nicht definiert)

                cef = Add(_epArr0[pIndex], _e1p1E2p2Save[pIndex]);
                if (cef <= Cnst085Q412 && MultR(cef, Cnst05882Q115) <= cefEst)
                {
                    pf = pIndex;
                    break;
                }

                if (cef <= Cnst04Q412 && MultR(cef, Cnst02857Q115) <= cefEst)
                {
                    pf = pIndex;
                    break;
                }

                if (cef <= Cnst005Q412)
                {
                    pf = pIndex;
                    break;
                }
            }
        }

        cef = Add(_epArr0[pf], _e1p1E2p2Save[pf]);
        p = ceb <= cef ? pb : pf;

        _prevPrevPitch = _prevPitch;
        _prevPitch = p;
        _prevPrevEP = _prevEP;
        _prevEP = _epArr0[p];

        _pitch = (short)(p + 42);
        _eP = _prevEP;
    }

    // ------------------------------------------------------------------
    // pitch_ref.cc
    // ------------------------------------------------------------------

    private void PitchRef()
    {
        short[] wrSp = ImbeTables.WrSp;
        short pitchCand = 0;
        int fundFreqCand = 0;

        short pitchEst = Shl(_pitch, 7);
        pitchEst = Sub(pitchEst, Cnst1125Q88);

        int lDiffMin = Max32;

        for (short i = 0; i < 19; i++)
        {
            short shift = NormS(pitchEst);
            short tmp = Shl(pitchEst, shift);
            tmp = DivS(0x4000, tmp);
            int fundFreq = LShl(tmp, shift + 11);
            int fundFreqAcc = fundFreq;
            int fundFreq2 = LShr(fundFreq, 1);
            int fundFreqAccA = LSub(fundFreq, fundFreq2);
            int fundFreqAccB = LAdd(fundFreq, fundFreq2);

            short upLim = ExtractH(LShr(unchecked((int)((uint)Cnst09254Q016 * (uint)pitchEst)), 1));
            upLim = Sub(upLim, Cnst05Q88);
            upLim = (short)(upLim & 0xFF00);
            upLim = Mult(upLim, ExtractH(fundFreq));
            upLim = Shr(upLim, 1);

            short indexB = 0;
            while (indexB <= upLim)
            {
                short ha = ExtractH(fundFreqAccA);
                short hb = ExtractH(fundFreqAccB);
                short indexA = (short)((ha >> 8) + ((ha & 0xFF) != 0 ? 1 : 0));
                indexB = (short)((hb >> 8) + ((hb & 0xFF) != 0 ? 1 : 0));

                if (indexB >= MinIndex)
                {
                    int lTmp = LShl(LDepositH(indexA), 8);
                    lTmp = LSub(lTmp, fundFreqAcc);
                    lTmp = LAdd(lTmp, 0x00020000);
                    lTmp = LShr(lTmp, 2);

                    short indexASave = indexA;
                    int itInd = 0;
                    int ampReAcc = 0, ampImAcc = 0;

                    while (indexA < indexB)
                    {
                        short indexWr = ExtractH(lTmp);
                        if (indexWr < 0 && (lTmp & 0xFFFF) != 0)
                            indexWr = Add(indexWr, 1);
                        indexWr = Add(indexWr, 160);

                        _indexTbl[itInd++] = indexWr;

                        if (indexWr >= 0 && indexWr <= 320)
                        {
                            ampReAcc = LMac(ampReAcc, _fftBuf[2 * indexA], wrSp[indexWr]);
                            ampImAcc = LMac(ampImAcc, _fftBuf[(2 * indexA) + 1], wrSp[indexWr]);
                        }

                        indexA++;
                        lTmp = LAdd(lTmp, 0x400000);
                    }

                    itInd = 0;
                    indexA = indexASave;
                    while (indexA < indexB)
                    {
                        int indexWr = _indexTbl[itInd++];
                        if (indexWr < 0 || indexWr > 320)
                        {
                            _spRecIm[indexA] = _spRecRe[indexA] = 0;
                        }
                        else
                        {
                            _spRecIm[indexA] = Mult(Mult(ExtractH(ampImAcc), wrSp[indexWr]), 0x6666);
                            _spRecRe[indexA] = Mult(Mult(ExtractH(ampReAcc), wrSp[indexWr]), 0x6666);
                        }

                        indexA++;
                    }
                }

                fundFreqAccA = LAdd(fundFreqAccA, fundFreq);
                fundFreqAccB = LAdd(fundFreqAccB, fundFreq);
                fundFreqAcc = LAdd(fundFreqAcc, fundFreq);
            }

            int lSum = 0;
            for (short j = MinIndex; j <= upLim; j++)
            {
                short reTmp = Sub(_fftBuf[2 * j], _spRecRe[j]);
                short imTmp = Sub(_fftBuf[(2 * j) + 1], _spRecIm[j]);
                lSum = LMac(lSum, reTmp, reTmp);
                lSum = LMac(lSum, imTmp, imTmp);
            }

            if (lSum < lDiffMin)
            {
                lDiffMin = lSum;
                pitchCand = pitchEst;
                fundFreqCand = fundFreq;
            }

            pitchEst = Add(pitchEst, Cnst0125Q88);
        }

        _refPitch = pitchCand;
        _fundFreq = fundFreqCand;
    }

    // ------------------------------------------------------------------
    // v_uv_det.cc
    // ------------------------------------------------------------------

    private static short VoicedSaCalc(int num, short den)
    {
        int lTmp = LMpyLs(num, den);
        lTmp = SqrtLExp(lTmp, out short tmp);
        lTmp = LShr(lTmp, tmp - 3);
        return ExtractH(lTmp);
    }

    private static short UnvoicedSaCalc(int num, short den)
    {
        short shift = NormS(den);
        short tmp = DivS(0x4000, Shl(den, shift));
        int lTmp = LShl(LMpyLs(num, tmp), shift + 2);
        lTmp = SqrtLExp(lTmp, out tmp);
        lTmp = LShr(lTmp, tmp - 2 + 6);
        lTmp = LMpyLs(lTmp, 0x4A76);
        return ExtractH(lTmp);
    }

    private void VUvDet()
    {
        short[] wrSp = ImbeTables.WrSp;
        short tmp;
        short dsnThr = 0;

        int fundFreq = _fundFreq;
        tmp = Shr(Add(Shr(_refPitch, 1), Cnst025Q88), 8);

        short numHarms = ExtractH(unchecked((int)((uint)Cnst09254Q016 * (uint)(int)tmp)));
        if (numHarms < NumHarmsMin)
            numHarms = NumHarmsMin;
        else if (numHarms > NumHarmsMax)
            numHarms = NumHarmsMax;

        short numBands;
        if (numHarms <= 36)
            numBands = ExtractH(unchecked((int)((uint)(numHarms + 2) * (uint)Cnst033Q016)));
        else
            numBands = NumBandsMax;

        _numHarms = numHarms;
        _numBands = numBands;

        int thLf = 0;
        for (int j = 0; j < 64; j++)
        {
            thLf = LMac(thLf, _fftBuf[2 * j], _fftBuf[2 * j]);
            thLf = LMac(thLf, _fftBuf[(2 * j) + 1], _fftBuf[(2 * j) + 1]);
        }

        int thHf = 0;
        for (int j = 64; j < 128; j++)
        {
            thHf = LMac(thHf, _fftBuf[2 * j], _fftBuf[2 * j]);
            thHf = LMac(thHf, _fftBuf[(2 * j) + 1], _fftBuf[(2 * j) + 1]);
        }

        int th0 = LAdd(thLf, thHf);
        if (th0 > _thMax)
            _thMax = LShr(LAdd(_thMax, th0), 1);
        else
            _thMax = LAdd(LMpyLs(_thMax, Cnst09900Q115), LMpyLs(th0, Cnst001Q115));

        int mFcnNum = LAdd(th0, LMpyLs(_thMax, Cnst00025Q115));
        int mFcnDen = LAdd(th0, LMpyLs(_thMax, Cnst001Q115));
        short mFcn;

        if (mFcnDen == 0)
        {
            mFcn = Cnst025Q115;
        }
        else
        {
            tmp = NormL(mFcnDen);
            mFcnDen = LShl(mFcnDen, tmp);
            mFcnNum = LShl(mFcnNum, tmp);
            mFcn = DivS(ExtractH(mFcnNum), ExtractH(mFcnDen));

            int lTmp0 = LAdd(LShl(thHf, 2), thHf);
            if (thLf < lTmp0)
            {
                tmp = NormL(lTmp0);
                mFcnDen = LShl(lTmp0, tmp);
                thLf = LShl(thLf, tmp);
                tmp = DivS(ExtractH(thLf), ExtractH(mFcnDen));
                int lTmp1 = SqrtLExp(LDepositH(tmp), out tmp);
                if (tmp != 0)
                    lTmp1 = LShr(lTmp1, tmp);
                mFcn = Mult(mFcn, ExtractH(lTmp1));
            }
        }

        int fundFrStep = LShl(LMpyLs(fundFreq, CnstPi4Q115), 2);

        short uvHarmsCnt = 0;
        short b1Vec = 0;
        short bandCnt = 0;
        short numHarmsCnt = 0;
        int dNum = 0, dDen = 0;
        int fundFrAcc = 0;
        int fundFreqAcc = fundFreq;
        int fundFreq2 = LShr(fundFreq, 1);
        int fundFreqAccA = LSub(fundFreq, fundFreq2);
        int fundFreqAccB = LAdd(fundFreq, fundFreq2);

        short indexB = 0, indexASave = 0;

        for (short j = 0; j < numHarms; j++)
        {
            short ha = ExtractH(fundFreqAccA);
            short hb = ExtractH(fundFreqAccB);
            short indexA = (short)((ha >> 8) + ((ha & 0xFF) != 0 ? 1 : 0));
            indexB = (short)((hb >> 8) + ((hb & 0xFF) != 0 ? 1 : 0));

            int lTmp = LShl(LDepositH(indexA), 8);
            lTmp = LSub(lTmp, fundFreqAcc);
            lTmp = LAdd(lTmp, 0x00020000);
            lTmp = LShr(lTmp, 2);

            indexASave = indexA;
            int itInd = 0;

            if (numHarmsCnt == 0)
            {
                if (_eP > Cnst055Q412 && bandCnt >= 1)
                    dsnThr = 0;
                else if (_bandVuv[bandCnt] == 1)
                    dsnThr = Mult(mFcn, Sub(Cnst05625Q115, Mult(Cnst01741Q115, ExtractH(fundFrAcc))));
                else
                    dsnThr = Mult(mFcn, Sub(Cnst045Q115, Mult(Cnst01393Q115, ExtractH(fundFrAcc))));

                fundFrAcc = LAdd(fundFrAcc, fundFrStep);
            }

            int mDenSum = 0;
            int ampReAcc = 0, ampImAcc = 0;

            while (indexA < indexB)
            {
                short indexWr = ExtractH(lTmp);
                if (indexWr < 0 && (lTmp & 0xFFFF) != 0)
                    indexWr = Add(indexWr, 1);
                indexWr = Add(indexWr, 160);

                _indexTbl[itInd++] = indexWr;

                if (indexWr >= 0 && indexWr <= 320)
                {
                    ampReAcc = LMac(ampReAcc, _fftBuf[2 * indexA], wrSp[indexWr]);
                    ampImAcc = LMac(ampImAcc, _fftBuf[(2 * indexA) + 1], wrSp[indexWr]);
                    mDenSum = LAdd(mDenSum, Mult(wrSp[indexWr], wrSp[indexWr]));
                }

                indexA++;
                lTmp = LAdd(lTmp, 0x400000);
            }

            short scCoef = DivS(0x4000, ExtractL(LShr(mDenSum, 1)));
            short imTmp2 = Mult(ExtractH(ampImAcc), scCoef);
            short reTmp2 = Mult(ExtractH(ampReAcc), scCoef);

            int mNumSum = 0;
            itInd = 0;
            indexA = indexASave;

            while (indexA < indexB)
            {
                int indexWr = _indexTbl[itInd++];
                short spRecRe, spRecIm;

                if (indexWr < 0 || indexWr > 320)
                {
                    spRecRe = spRecIm = 0;
                }
                else
                {
                    spRecIm = Mult(imTmp2, wrSp[indexWr]);
                    spRecRe = Mult(reTmp2, wrSp[indexWr]);
                }

                short reTmp = Sub(_fftBuf[2 * indexA], spRecRe);
                short imTmp = Sub(_fftBuf[(2 * indexA) + 1], spRecIm);

                dNum = LMac(dNum, reTmp, reTmp);
                dNum = LMac(dNum, imTmp, imTmp);

                mNumSum = LMac(mNumSum, _fftBuf[2 * indexA], _fftBuf[2 * indexA]);
                mNumSum = LMac(mNumSum, _fftBuf[(2 * indexA) + 1], _fftBuf[(2 * indexA) + 1]);

                indexA++;
            }

            _mDen[j] = scCoef;
            _mNum[j] = mNumSum;
            dDen = LAdd(dDen, mNumSum);

            if (++numHarmsCnt == 3 && bandCnt < numBands - 1)
            {
                b1Vec = (short)(b1Vec << 1);

                short dk;
                if (dDen > dNum && dDen != 0)
                {
                    tmp = NormL(dDen);
                    dk = DivS(ExtractH(LShl(dNum, tmp)), ExtractH(LShl(dDen, tmp)));
                }
                else
                {
                    dk = Max16;
                }

                if (dk < dsnThr)
                {
                    _bandVuv[bandCnt] = 1;
                    b1Vec |= 1;

                    for (short k = (short)(j - 2); k <= j; k++)
                    {
                        _sa[k] = VoicedSaCalc(_mNum[k], _mDen[k]);
                        _harmVuv[k] = 1;
                    }
                }
                else
                {
                    _bandVuv[bandCnt] = 0;

                    for (short k = (short)(j - 2); k <= j; k++)
                    {
                        _sa[k] = UnvoicedSaCalc(_mNum[k], (short)(indexB - indexASave));
                        _harmVuv[k] = 0;
                        uvHarmsCnt++;
                    }
                }

                dNum = dDen = 0;
                numHarmsCnt = 0;
                bandCnt++;
            }

            fundFreqAccA = LAdd(fundFreqAccA, fundFreq);
            fundFreqAccB = LAdd(fundFreqAccB, fundFreq);
            fundFreqAcc = LAdd(fundFreqAcc, fundFreq);
        }

        if (numHarmsCnt != 0)
        {
            b1Vec = (short)(b1Vec << 1);

            short dk;
            if (dDen > dNum && dDen != 0)
            {
                tmp = NormL(dDen);
                dk = DivS(ExtractH(LShl(dNum, tmp)), ExtractH(LShl(dDen, tmp)));
            }
            else
            {
                dk = Max16;
            }

            if (dk < dsnThr)
            {
                _bandVuv[bandCnt] = 1;
                b1Vec |= 1;

                for (short k = (short)(numHarms - numHarmsCnt); k < numHarms; k++)
                {
                    _sa[k] = VoicedSaCalc(_mNum[k], _mDen[k]);
                    _harmVuv[k] = 1;
                }
            }
            else
            {
                _bandVuv[bandCnt] = 0;

                for (short k = (short)(numHarms - numHarmsCnt); k < numHarms; k++)
                {
                    _sa[k] = UnvoicedSaCalc(_mNum[k], (short)(indexB - indexASave));
                    _harmVuv[k] = 0;
                    uvHarmsCnt++;
                }
            }
        }
    }
}
