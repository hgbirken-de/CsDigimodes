using static FusionCodec.BitOperations;

namespace FusionCodec;

public class Convolution
{

    static readonly int[] BRANCH_TABLE1 = { 0, 0, 0, 0, 1, 1, 1, 1 };

    static readonly int[] BRANCH_TABLE2 = { 0, 1, 1, 0, 0, 1, 1, 0 };

    const int NUM_OF_STATES_D2 = 8;
    const int NUM_OF_STATES = 16;
    const int MM = 2;
    const int KK = 5;

    private int[] _metrics1 = [];
    private int[] _metrics2 = [];
    private int[] _old_metrics = [];
    private int[] _new_metrics = [];
    private int _dp_i = 0;

    public uint[] _m_dp = [];

    public Convolution() {}

    public void Start()
    {
        _metrics1 = new int[NUM_OF_STATES];
        _metrics2 = new int[NUM_OF_STATES];
        _m_dp = new uint[180];
        _dp_i = 0;
        _old_metrics = (int[])_metrics1.Clone();
        _new_metrics = (int[])_metrics2.Clone();
    }

    public void Chainback(byte[] output, int nbits)
    {
        int state = 0;
        while (nbits > 0)
        {
            _dp_i--;
            nbits--;
            int i = state >> 9 - KK;
            int bit = (int)(_m_dp[_dp_i] >> i & 1) & 0xFF;
            state = bit << 7 | state >> 1;

            WriteBit1(output, nbits, bit != 0);
        }
    }

    public void Decode(int s0, int s1)
    {
        for (int i = 0; i < NUM_OF_STATES_D2; i++)
        {
            int j = i * 2;
            int metric = (BRANCH_TABLE1[i] ^ s0) + (BRANCH_TABLE2[i] ^ s1) & 0xFFFF;
            int m0 = _old_metrics[i] + metric & 0xFFFF;
            int m1 = _old_metrics[i + NUM_OF_STATES_D2] + (MM - metric) & 0xFFFF;

            int decision0 = m0 >= m1 ? 1 : 0;
            _new_metrics[j + 0] = m0 >= m1 ? m1 : m0;

            m0 = _old_metrics[i] + (MM - metric) & 0xFFFF;
            m1 = _old_metrics[i + NUM_OF_STATES_D2] + metric & 0xFFFF;

            int decision1 = m0 >= m1 ? 1 : 0;
            _new_metrics[j + 1] = decision1 != 0 ? m1 : m0;

            _m_dp[_dp_i] |= (uint)decision1 << j + 1 | (uint)decision0 << j;
            
        }

        _dp_i++;

        //var tmp = _old_metrics;
        //_old_metrics = _new_metrics;
        //_new_metrics = tmp;
        (_new_metrics, _old_metrics) = (_old_metrics, _new_metrics);
    }

    //public static void Encode(byte[] input, byte[] output, int nbits)
    //{
    //    int d1 = 0, d2 = 0, d3 = 0, d4 = 0, k = 0;
    //    for (int i = 0; i < nbits; i++)
    //    {
    //        int d = ReadBit1(input, i) > 0 ? 1 : 0;
    //        int g1 = (d + d3 + d4) & 1;
    //        int g2 = (d + d1 + d2 + d4) & 1;

    //        d4 = d3;
    //        d3 = d2;
    //        d2 = d1;
    //        d1 = d;

    //        WriteBit1(output, k, g1 != 0);
    //        k++;

    //        WriteBit1(output, k, g2 != 0);
    //        k++;
    //    }
    //}

    /// <summary>
    /// Convolutionally encodes a binary input sequence into a rate 1/2 encoded output
    /// using a constraint length of 5 and generator polynomials (13, 17) in octal.
    /// </summary>
    /// <param name="input">The input bit sequence, packed into bytes (MSB-first within each byte).</param>
    /// <param name="output">The output buffer where encoded bits will be written, also packed into bytes.
    /// Each input bit produces two output bits, so the caller must ensure that
    /// <paramref name="output"/> is at least <paramref name="nBits"/> * 2 bits long.</param>
    /// <param name="nBits">The number of input bits to encode from <paramref name="input"/>.</param>
    /// <remarks>This encoder resets its shift registers at the start of each call, so each invocation is independent.</remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> or <paramref name="output"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="nBits"/> is not positive.</exception>
    public static void Encode(byte[] input, byte[] output, int nBits)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nBits);

        // Reset shift registers at the beginning of each encode
        byte d1 = 0, d2 = 0, d3 = 0, d4 = 0;

        int k = 0;
        for (int i = 0; i < nBits; i++)
        {
            byte d = (byte)ReadBit1(input, i);

            byte g1 = (byte)(d + d3 + d4 & 1);
            byte g2 = (byte)(d + d1 + d2 + d4 & 1);

            d4 = d3;
            d3 = d2;
            d2 = d1;
            d1 = d;

            WriteBit1(output, k++, g1 != 0);
            WriteBit1(output, k++, g2 != 0);
        }
    }

}

