using System;

namespace BlueDV
{
	// Token: 0x02000041 RID: 65
	internal class CYFSConvolution : IDisposable
	{
		// Token: 0x06000444 RID: 1092 RVA: 0x0002B5D6 File Offset: 0x000297D6
		public bool READ_BIT1(byte[] p, int i)
		{
			return (p[i >> 3] & CYFSConvolution.BIT_MASK_TABLE[i & 7]) > 0;
		}

		// Token: 0x06000445 RID: 1093 RVA: 0x0002B5F0 File Offset: 0x000297F0
		public void CYSFConvolution()
		{
			this.m_metrics1 = new ushort[32];
			this.m_metrics2 = new ushort[32];
			this.m_oldMetrics = new ushort[32];
			this.m_newMetrics = new ushort[32];
			this.m_decisions = new ulong[180];
			this.m_dp = new ulong[180];
		}

		// Token: 0x06000446 RID: 1094 RVA: 0x00002F42 File Offset: 0x00001142
		public void Dispose()
		{
		}

		// Token: 0x06000447 RID: 1095 RVA: 0x0002B654 File Offset: 0x00029854
		public void start()
		{
			this.m_metrics1 = new ushort[32];
			this.m_metrics2 = new ushort[32];
			this.m_oldMetrics = new ushort[32];
			this.m_newMetrics = new ushort[32];
			this.m_decisions = new ulong[180];
			this.m_dp = new ulong[180];
		}

		// Token: 0x06000448 RID: 1096 RVA: 0x0002B6B8 File Offset: 0x000298B8
		public void decode(byte s0, byte s1)
		{
			for (uint num = 0U; num < (uint)CYFSConvolution.NUM_OF_STATES_D2; num += 1U)
			{
				uint num2 = num * 2U;
				ushort num3 = (ushort)((CYFSConvolution.BRANCH_TABLE1[(int)num] ^ s0) + (CYFSConvolution.BRANCH_TABLE2[(int)num] ^ s1));
				ushort num4 = this.m_oldMetrics[(int)num] + num3;
				ushort num5 = (ushort)((uint)this.m_oldMetrics[(int)(num + (uint)CYFSConvolution.NUM_OF_STATES_D2)] + (CYFSConvolution.M - (uint)num3));
				uint num6 = ((num4 >= num5) ? 1U : 0U);
				this.m_newMetrics[(int)num2] = ((num6 != 0U) ? num5 : num4);
				num4 = (ushort)((uint)this.m_oldMetrics[(int)num] + (CYFSConvolution.M - (uint)num3));
				num5 = this.m_oldMetrics[(int)(num + (uint)CYFSConvolution.NUM_OF_STATES_D2)] + num3;
				uint num7 = ((num4 >= num5) ? 1U : 0U);
				this.m_newMetrics[(int)(num2 + 1U)] = ((num7 != 0U) ? num5 : num4);
				this.m_dp[this.m_dp_counter] |= (ulong)((ulong)num7 << (int)(num2 + 1U)) | (ulong)((ulong)num6 << (int)num2);
			}
			this.m_dp_counter++;
			ushort[] oldMetrics = this.m_oldMetrics;
			this.m_oldMetrics = this.m_newMetrics;
			this.m_newMetrics = oldMetrics;
		}

		// Token: 0x06000449 RID: 1097 RVA: 0x0002B7CC File Offset: 0x000299CC
		public void chainback(byte[] @out, byte nBits)
		{
			uint num = 0U;
			for (;;)
			{
				byte b = nBits;
				nBits = b - 1;
				if (b <= 0)
				{
					break;
				}
				this.m_dp_counter--;
				uint num2 = num >> (int)(9 - CYFSConvolution.K);
				byte b2 = (byte)((this.m_dp[this.m_dp_counter] >> (int)num2) & 1UL);
				num = (uint)(((int)b2 << 7) | (int)(num >> 1));
				@out[nBits >> 3] = ((b2 != 0) ? (@out[nBits >> 3] | CYFSConvolution.BIT_MASK_TABLE[(int)(nBits & 7)]) : (@out[nBits >> 3] & ~CYFSConvolution.BIT_MASK_TABLE[(int)(nBits & 7)]));
			}
		}

		// Token: 0x0600044A RID: 1098 RVA: 0x0002B850 File Offset: 0x00029A50
		public byte[] encode(byte[] incoming, int nBits, int outBytes)
		{
			byte[] array = new byte[outBytes];
			uint num = 0U;
			uint num2 = 0U;
			uint num3 = 0U;
			uint num4 = 0U;
			int num5 = 0;
			for (int i = 0; i < nBits; i++)
			{
				uint num6;
				if (this.READ_BIT1(incoming, i))
				{
					num6 = 1U;
				}
				else
				{
					num6 = 0U;
				}
				uint num7 = (num6 + num3 + num4) & 1U;
				uint num8 = (num6 + num + num2 + num4) & 1U;
				num4 = num3;
				num3 = num2;
				num2 = num;
				num = num6;
				array = fusion_extract.bit_writer(array, num5, num7 > 0U);
				num5++;
				array = fusion_extract.bit_writer(array, num5, num8 > 0U);
				num5++;
			}
			return array;
		}

		// Token: 0x040002C8 RID: 712
		public static readonly byte[] BIT_MASK_TABLE = new byte[] { 128, 64, 32, 16, 8, 4, 2, 1 };

		// Token: 0x040002C9 RID: 713
		public static readonly byte[] BRANCH_TABLE1 = new byte[] { 0, 0, 0, 0, 1, 1, 1, 1 };

		// Token: 0x040002CA RID: 714
		public static readonly byte[] BRANCH_TABLE2 = new byte[] { 0, 1, 1, 0, 0, 1, 1, 0 };

		// Token: 0x040002CB RID: 715
		public static readonly byte NUM_OF_STATES_D2 = 8;

		// Token: 0x040002CC RID: 716
		public static readonly byte NUM_OF_STATES = 16;

		// Token: 0x040002CD RID: 717
		public static readonly uint M = 2U;

		// Token: 0x040002CE RID: 718
		public static readonly byte K = 5;

		// Token: 0x040002CF RID: 719
		private ushort[] m_metrics1;

		// Token: 0x040002D0 RID: 720
		private ushort[] m_metrics2;

		// Token: 0x040002D1 RID: 721
		private ushort[] m_oldMetrics;

		// Token: 0x040002D2 RID: 722
		private ushort[] m_newMetrics;

		// Token: 0x040002D3 RID: 723
		private ulong[] m_decisions;

		// Token: 0x040002D4 RID: 724
		private ulong[] m_dp;

		// Token: 0x040002D5 RID: 725
		private int m_dp_counter;
	}
}
