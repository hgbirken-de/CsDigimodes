using System;
using BlueDV.Properties;

namespace BlueDV
{
	// Token: 0x0200000D RID: 13
	internal class berCounter
	{
		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000087 RID: 135 RVA: 0x0000498D File Offset: 0x00002B8D
		// (set) Token: 0x06000088 RID: 136 RVA: 0x00004994 File Offset: 0x00002B94
		public static string StatusBER { get; private set; }

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x06000089 RID: 137 RVA: 0x0000499C File Offset: 0x00002B9C
		// (remove) Token: 0x0600008A RID: 138 RVA: 0x000049D0 File Offset: 0x00002BD0
		public static event EventHandler StatusBERChanged;

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600008B RID: 139 RVA: 0x00004A03 File Offset: 0x00002C03
		// (set) Token: 0x0600008C RID: 140 RVA: 0x00004A0A File Offset: 0x00002C0A
		public static string srcID { get; private set; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600008D RID: 141 RVA: 0x00004A12 File Offset: 0x00002C12
		// (set) Token: 0x0600008E RID: 142 RVA: 0x00004A19 File Offset: 0x00002C19
		public static string dstID { get; private set; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600008F RID: 143 RVA: 0x00004A21 File Offset: 0x00002C21
		// (set) Token: 0x06000090 RID: 144 RVA: 0x00004A28 File Offset: 0x00002C28
		public static string TextID { get; private set; }

		// Token: 0x1400000A RID: 10
		// (add) Token: 0x06000091 RID: 145 RVA: 0x00004A30 File Offset: 0x00002C30
		// (remove) Token: 0x06000092 RID: 146 RVA: 0x00004A64 File Offset: 0x00002C64
		public static event EventHandler StatusSRCDSTChanged;

		// Token: 0x06000093 RID: 147 RVA: 0x00004A98 File Offset: 0x00002C98
		private static int SparseBitcount(long n)
		{
			int num = 0;
			while (n != 0L)
			{
				num++;
				n &= n - 1L;
			}
			return num;
		}

		// Token: 0x06000094 RID: 148 RVA: 0x00004ABC File Offset: 0x00002CBC
		private static long NumberOfSetBits(long i)
		{
			i -= (i >> 1) & 6148914691236517205L;
			i = (i & 3689348814741910323L) + ((i >> 2) & 3689348814741910323L);
			return ((i + (i >> 4)) & 1085102592571150095L) * 72340172838076673L >> 56;
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00004B14 File Offset: 0x00002D14
		private static void ChangeStatusBER(string text)
		{
			berCounter.StatusBER = text;
			EventHandler statusBERChanged = berCounter.StatusBERChanged;
			if (statusBERChanged != null)
			{
				statusBERChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00004B3C File Offset: 0x00002D3C
		private static void ChangeStatusSRCDST(string srcID2, string dstID2, string text2)
		{
			berCounter.srcID = srcID2;
			berCounter.dstID = dstID2;
			berCounter.TextID = text2;
			EventHandler statusSRCDSTChanged = berCounter.StatusSRCDSTChanged;
			if (statusSRCDSTChanged != null)
			{
				statusSRCDSTChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00002F42 File Offset: 0x00001142
		public static void destinationID(byte[] DMRvoice, byte type)
		{
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00004B70 File Offset: 0x00002D70
		private void byteToBitsBE(char byte1, bool[] bits1)
		{
			bits1[0] = (byte1 & '\u0080') == '\u0080';
			bits1[1] = (byte1 & '@') == '@';
			bits1[2] = (byte1 & ' ') == ' ';
			bits1[3] = (byte1 & '\u0010') == '\u0010';
			bits1[4] = (byte1 & '\b') == '\b';
			bits1[5] = (byte1 & '\u0004') == '\u0004';
			bits1[6] = (byte1 & '\u0002') == '\u0002';
			bits1[7] = (byte1 & '\u0001') == '\u0001';
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00004BD4 File Offset: 0x00002DD4
		public static bool test(byte[] voice, byte type)
		{
			int num = 0;
			bool[] array = new bool[64];
			for (int i = 13; i < 20; i++)
			{
				Buffer.BlockCopy(utils.byteToBitsBE(voice[i]), 0, array, num, 8);
				num += 8;
			}
			for (int j = 0; j < 64; j++)
			{
			}
			bool[] array2 = new bool[32];
			Buffer.BlockCopy(array, 12, array2, 0, 32);
			for (int k = 0; k < 32; k++)
			{
			}
			switch (type)
			{
			case 1:
				Buffer.BlockCopy(array2, 0, berCounter.totalMiddel, 0, 32);
				break;
			case 2:
				Buffer.BlockCopy(array2, 0, berCounter.totalMiddel, 32, 32);
				break;
			case 3:
				Buffer.BlockCopy(array2, 0, berCounter.totalMiddel, 64, 32);
				break;
			case 4:
				Buffer.BlockCopy(array2, 0, berCounter.totalMiddel, 96, 32);
				break;
			case 5:
			{
				for (int l = 0; l < 128; l++)
				{
				}
				int num2 = 0;
				bool[] array3 = new bool[128];
				for (int m = 0; m < 128; m++)
				{
					array3[m] = berCounter.totalMiddel[num2];
					num2 += 16;
					if (num2 > 127)
					{
						num2 -= 127;
					}
				}
				for (int n = 0; n < 128; n++)
				{
				}
				bool[] array4 = new bool[16];
				bool[] array5 = new bool[16];
				bool[] array6 = new bool[16];
				bool[] array7 = new bool[16];
				bool[] array8 = new bool[16];
				bool[] array9 = new bool[16];
				bool[] array10 = new bool[16];
				bool[] array11 = new bool[16];
				Buffer.BlockCopy(array3, 0, array4, 0, 16);
				Buffer.BlockCopy(array3, 16, array5, 0, 16);
				Buffer.BlockCopy(array3, 32, array6, 0, 16);
				Buffer.BlockCopy(array3, 48, array7, 0, 16);
				Buffer.BlockCopy(array3, 64, array8, 0, 16);
				Buffer.BlockCopy(array3, 80, array9, 0, 16);
				Buffer.BlockCopy(array3, 96, array10, 0, 16);
				Buffer.BlockCopy(array3, 112, array11, 0, 16);
				for (int num3 = 0; num3 < 16; num3++)
				{
				}
				for (int num4 = 0; num4 < 16; num4++)
				{
				}
				for (int num5 = 0; num5 < 16; num5++)
				{
				}
				for (int num6 = 0; num6 < 16; num6++)
				{
				}
				bool[] array12 = new bool[8];
				bool[] array13 = new bool[8];
				bool[] array14 = new bool[8];
				bool[] array15 = new bool[8];
				bool[] array16 = new bool[8];
				bool[] array17 = new bool[8];
				bool[] array18 = new bool[8];
				bool[] array19 = new bool[8];
				bool[] array20 = new bool[8];
				Buffer.BlockCopy(array4, 0, array12, 0, 8);
				Buffer.BlockCopy(array4, 8, array13, 0, 3);
				Buffer.BlockCopy(array5, 0, array13, 3, 5);
				Buffer.BlockCopy(array5, 5, array14, 0, 6);
				Buffer.BlockCopy(array6, 0, array14, 6, 2);
				Buffer.BlockCopy(array6, 2, array15, 0, 8);
				Buffer.BlockCopy(array7, 0, array16, 0, 8);
				Buffer.BlockCopy(array7, 8, array17, 0, 2);
				Buffer.BlockCopy(array8, 0, array17, 2, 6);
				Buffer.BlockCopy(array8, 6, array18, 0, 4);
				Buffer.BlockCopy(array9, 0, array18, 4, 4);
				Buffer.BlockCopy(array9, 4, array19, 0, 6);
				Buffer.BlockCopy(array10, 0, array19, 6, 2);
				Buffer.BlockCopy(array10, 2, array20, 0, 8);
				long num7 = (long)utils.bitsToByteBE(array15);
				byte b = utils.bitsToByteBE(array16);
				byte b2 = utils.bitsToByteBE(array17);
				long num8 = (num7 << 16) + (long)((long)b << 8);
				break;
			}
			}
			return false;
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00004F34 File Offset: 0x00003134
		private static void decodeDeInterleave()
		{
			int num = 0;
			while ((long)num < 196L)
			{
				berCounter.m_deInterData[num] = false;
				num++;
			}
			int num2 = 0;
			while ((long)num2 < 196L)
			{
				long num3 = (long)num2 * 181L % 196L;
				berCounter.m_deInterData[num2] = berCounter.m_rawData[(int)(checked((IntPtr)num3))];
				num2++;
			}
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00004F90 File Offset: 0x00003190
		public static bool getFullLC(byte[] DMRvoice, byte type)
		{
			bool flag15;
			try
			{
				bool flag = false;
				if ((type & 255) == 65)
				{
					bool[] array = new bool[264];
					int num = 0;
					for (int i = 0; i < 33; i++)
					{
						Buffer.BlockCopy(utils.byteToBitsBE(DMRvoice[i]), 0, array, num, 8);
						num += 8;
					}
					foreach (bool flag2 in array)
					{
					}
					byte[] array3 = new byte[8];
					byte[] array4 = new byte[8];
					byte[] array5 = new byte[8];
					byte[] array6 = new byte[8];
					byte[] array7 = new byte[1];
					Buffer.BlockCopy(DMRvoice, 0, array3, 0, 8);
					Buffer.BlockCopy(DMRvoice, 8, array4, 0, 8);
					Buffer.BlockCopy(DMRvoice, 16, array5, 0, 8);
					Buffer.BlockCopy(DMRvoice, 24, array6, 0, 8);
					Buffer.BlockCopy(DMRvoice, 32, array7, 0, 1);
					long num2 = utils.byte2longMS(array3);
					long num3 = utils.byte2longMS(array4);
					long num4 = utils.byte2longMS(array5);
					long num5 = utils.byte2longMS(array6);
					long num6 = utils.byte2longMS(array7);
					long num7 = 0L;
					if (((num3 >> 59) & 1L) != 0L)
					{
						num7 |= 128L;
					}
					if (((num2 >> 10) & 1L) != 0L)
					{
						num7 |= 64L;
					}
					if (((num5 >> 13) & 1L) != 0L)
					{
						num7 |= 32L;
					}
					if (((num5 >> 28) & 1L) != 0L)
					{
						num7 |= 16L;
					}
					if (((num5 >> 43) & 1L) != 0L)
					{
						num7 |= 8L;
					}
					if (((num5 >> 58) & 1L) != 0L)
					{
						num7 |= 4L;
					}
					if (((num4 >> 9) & 1L) != 0L)
					{
						num7 |= 2L;
					}
					if (((num4 >> 24) & 1L) != 0L)
					{
						num7 |= 1L;
					}
					long num8 = 0L;
					if (((num3 >> 43) & 1L) != 0L)
					{
						num8 |= 128L;
					}
					if (((num3 >> 58) & 1L) != 0L)
					{
						num8 |= 64L;
					}
					if (((num2 >> 9) & 1L) != 0L)
					{
						num8 |= 32L;
					}
					if (((num2 >> 24) & 1L) != 0L)
					{
						num8 |= 16L;
					}
					if (((num2 >> 39) & 1L) != 0L)
					{
						num8 |= 8L;
					}
					if (((num5 >> 42) & 1L) != 0L)
					{
						num8 |= 4L;
					}
					if (((num5 >> 57) & 1L) != 0L)
					{
						num8 |= 2L;
					}
					if (((num4 >> 8) & 1L) != 0L)
					{
						num8 |= 1L;
					}
					long num9 = 0L;
					if (((num4 >> 23) & 1L) != 0L)
					{
						num9 |= 128L;
					}
					if (((num3 >> 42) & 1L) != 0L)
					{
						num9 |= 64L;
					}
					if (((num3 >> 57) & 1L) != 0L)
					{
						num9 |= 32L;
					}
					if (((num2 >> 8) & 1L) != 0L)
					{
						num9 |= 16L;
					}
					if (((num2 >> 23) & 1L) != 0L)
					{
						num9 |= 8L;
					}
					if (((num2 >> 38) & 1L) != 0L)
					{
						num9 |= 4L;
					}
					if (((num2 >> 53) & 1L) != 0L)
					{
						num9 |= 2L;
					}
					if (((num6 >> 4) & 1L) != 0L)
					{
						num9 |= 1L;
					}
					long num10 = 0L;
					if (((num5 >> 31) & 1L) != 0L)
					{
						num10 |= 128L;
					}
					if (((num5 >> 46) & 1L) != 0L)
					{
						num10 |= 64L;
					}
					if (((num5 >> 61) & 1L) != 0L)
					{
						num10 |= 32L;
					}
					if (((num4 >> 12) & 1L) != 0L)
					{
						num10 |= 16L;
					}
					if (((num2 >> 27) & 1L) != 0L)
					{
						num10 |= 8L;
					}
					if (((num2 >> 42) & 1L) != 0L)
					{
						num10 |= 4L;
					}
					if (((num2 >> 57) & 1L) != 0L)
					{
						num10 |= 2L;
					}
					if ((num5 & 1L) != 0L)
					{
						num10 |= 1L;
					}
					long num11 = 0L;
					if (((num5 >> 15) & 1L) != 0L)
					{
						num11 |= 128L;
					}
					if (((num5 >> 30) & 1L) != 0L)
					{
						num11 |= 64L;
					}
					if (((num5 >> 45) & 1L) != 0L)
					{
						num11 |= 32L;
					}
					if (((num5 >> 60) & 1L) != 0L)
					{
						num11 |= 16L;
					}
					if (((num4 >> 11) & 1L) != 0L)
					{
						num11 |= 8L;
					}
					if (((num3 >> 30) & 1L) != 0L)
					{
						num11 |= 4L;
					}
					if (((num3 >> 45) & 1L) != 0L)
					{
						num11 |= 2L;
					}
					if (((num2 >> 56) & 1L) != 0L)
					{
						num11 |= 1L;
					}
					long num12 = 0L;
					if (((num6 >> 7) & 1L) != 0L)
					{
						num12 |= 128L;
					}
					if (((num5 >> 14) & 1L) != 0L)
					{
						num12 |= 64L;
					}
					if (((num5 >> 29) & 1L) != 0L)
					{
						num12 |= 32L;
					}
					if (((num5 >> 44) & 1L) != 0L)
					{
						num12 |= 16L;
					}
					if (((num5 >> 59) & 1L) != 0L)
					{
						num12 |= 8L;
					}
					if (((num4 >> 10) & 1L) != 0L)
					{
						num12 |= 4L;
					}
					if (((num4 >> 25) & 1L) != 0L)
					{
						num12 |= 2L;
					}
					if (((num3 >> 44) & 1L) != 0L)
					{
						num12 |= 1L;
					}
					long num13 = num10 << 16;
					num13 += num11 << 8;
					num13 += num12;
					long num14 = 0L;
					if (((num2 >> 31) & 1L) != 0L)
					{
						num14 |= 128L;
					}
					if (((num2 >> 46) & 1L) != 0L)
					{
						num14 |= 64L;
					}
					if (((num2 >> 61) & 1L) != 0L)
					{
						num14 |= 32L;
					}
					if ((num4 & 1L) != 0L)
					{
						num14 |= 16L;
					}
					if (((num4 >> 15) & 1L) != 0L)
					{
						num14 |= 8L;
					}
					if (((num3 >> 34) & 1L) != 0L)
					{
						num14 |= 4L;
					}
					if (((num3 >> 49) & 1L) != 0L)
					{
						num14 |= 2L;
					}
					if ((num2 & 1L) != 0L)
					{
						num14 |= 1L;
					}
					long num15 = 0L;
					if (((num5 >> 35) & 1L) != 0L)
					{
						num15 |= 128L;
					}
					if (((num5 >> 50) & 1L) != 0L)
					{
						num15 |= 64L;
					}
					if (((num4 >> 1) & 1L) != 0L)
					{
						num15 |= 32L;
					}
					if (((num4 >> 16) & 1L) != 0L)
					{
						num15 |= 16L;
					}
					if (((num3 >> 35) & 1L) != 0L)
					{
						num15 |= 8L;
					}
					if (((num3 >> 50) & 1L) != 0L)
					{
						num15 |= 4L;
					}
					if (((num2 >> 1) & 1L) != 0L)
					{
						num15 |= 2L;
					}
					if (((num2 >> 16) & 1L) != 0L)
					{
						num15 |= 1L;
					}
					long num16 = 0L;
					if (((num5 >> 51) & 1L) != 0L)
					{
						num16 |= 128L;
					}
					if (((num4 >> 2) & 1L) != 0L)
					{
						num16 |= 64L;
					}
					if (((num4 >> 17) & 1L) != 0L)
					{
						num16 |= 32L;
					}
					if (((num3 >> 36) & 1L) != 0L)
					{
						num16 |= 16L;
					}
					if (((num3 >> 51) & 1L) != 0L)
					{
						num16 |= 8L;
					}
					if (((num2 >> 2) & 1L) != 0L)
					{
						num16 |= 4L;
					}
					if (((num2 >> 17) & 1L) != 0L)
					{
						num16 |= 2L;
					}
					if (((num2 >> 32) & 1L) != 0L)
					{
						num16 |= 1L;
					}
					long num17 = 0L;
					if (((num2 >> 15) & 1L) != 0L)
					{
						num17 |= 128L;
					}
					if (((num2 >> 30) & 1L) != 0L)
					{
						num17 |= 64L;
					}
					if (((num2 >> 45) & 1L) != 0L)
					{
						num17 |= 32L;
					}
					if (((num2 >> 60) & 1L) != 0L)
					{
						num17 |= 16L;
					}
					if (((num5 >> 3) & 1L) != 0L)
					{
						num17 |= 8L;
					}
					if (((num5 >> 18) & 1L) != 0L)
					{
						num17 |= 4L;
					}
					if (((num3 >> 33) & 1L) != 0L)
					{
						num17 |= 2L;
					}
					if (((num3 >> 48) & 1L) != 0L)
					{
						num17 |= 1L;
					}
					long num18 = 0L;
					if (((num3 >> 63) & 1L) != 0L)
					{
						num18 |= 128L;
					}
					if (((num2 >> 14) & 1L) != 0L)
					{
						num18 |= 64L;
					}
					if (((num2 >> 29) & 1L) != 0L)
					{
						num18 |= 32L;
					}
					if (((num2 >> 44) & 1L) != 0L)
					{
						num18 |= 16L;
					}
					if (((num2 >> 59) & 1L) != 0L)
					{
						num18 |= 8L;
					}
					if (((num5 >> 2) & 1L) != 0L)
					{
						num18 |= 4L;
					}
					if (((num5 >> 17) & 1L) != 0L)
					{
						num18 |= 2L;
					}
					if (((num5 >> 32) & 1L) != 0L)
					{
						num18 |= 1L;
					}
					long num19 = 0L;
					if (((num5 >> 47) & 1L) != 0L)
					{
						num19 |= 128L;
					}
					if (((num3 >> 62) & 1L) != 0L)
					{
						num19 |= 64L;
					}
					if (((num2 >> 13) & 1L) != 0L)
					{
						num19 |= 32L;
					}
					if (((num2 >> 28) & 1L) != 0L)
					{
						num19 |= 16L;
					}
					if (((num2 >> 43) & 1L) != 0L)
					{
						num19 |= 8L;
					}
					if (((num2 >> 58) & 1L) != 0L)
					{
						num19 |= 4L;
					}
					if (((num5 >> 1) & 1L) != 0L)
					{
						num19 |= 2L;
					}
					if (((num5 >> 16) & 1L) != 0L)
					{
						num19 |= 1L;
					}
					byte[] array8 = new byte[]
					{
						(byte)num16,
						(byte)num15,
						(byte)num14,
						(byte)num17,
						(byte)num18,
						(byte)num19,
						(byte)num10,
						(byte)num11,
						(byte)num12,
						(byte)num7,
						(byte)num8,
						(byte)num9
					};
					int num20 = 9;
					array8[num20] ^= 150;
					int num21 = 10;
					array8[num21] ^= 150;
					int num22 = 11;
					array8[num22] ^= 150;
					flag = CRS129.check(array8);
					long num23 = num17 << 16;
					num23 += num18 << 8;
					num23 += num19;
					foreach (bool flag3 in utils.byteToBitsBE((byte)num9))
					{
					}
					foreach (bool flag4 in utils.byteToBitsBE((byte)num8))
					{
					}
					foreach (bool flag5 in utils.byteToBitsBE((byte)num7))
					{
					}
					foreach (bool flag6 in utils.byteToBitsBE((byte)num12))
					{
					}
					foreach (bool flag7 in utils.byteToBitsBE((byte)num11))
					{
					}
					foreach (bool flag8 in utils.byteToBitsBE((byte)num10))
					{
					}
					foreach (bool flag9 in utils.byteToBitsBE((byte)num19))
					{
					}
					foreach (bool flag10 in utils.byteToBitsBE((byte)num18))
					{
					}
					foreach (bool flag11 in utils.byteToBitsBE((byte)num17))
					{
					}
					foreach (bool flag12 in utils.byteToBitsBE((byte)num14))
					{
					}
					foreach (bool flag13 in utils.byteToBitsBE((byte)num15))
					{
					}
					foreach (bool flag14 in utils.byteToBitsBE((byte)num16))
					{
					}
					if (flag)
					{
						information.myResolvedDstDMRID = num23.ToString();
						information.myResolvedSrcDMRID = num13.ToString();
					}
					if (((num2 >> 32) & 1L) == 0L)
					{
						long num24 = (num2 >> 17) & 1L;
					}
					if (((num2 >> 32) & 1L) == 1L && ((num2 >> 17) & 1L) == 1L)
					{
						information.myResLastReflector = num23.ToString();
					}
					if (flag && berCounter.StartFrameNumber == 0)
					{
						berCounter.ChangeStatusSRCDST(information.myResolvedSrcDMRID, information.myResolvedDstDMRID, information.myResLastReflector);
					}
					berCounter.StartFrameNumber++;
				}
				flag15 = flag;
			}
			catch (Exception)
			{
				flag15 = false;
			}
			return flag15;
		}

		// Token: 0x0600009C RID: 156 RVA: 0x00005A60 File Offset: 0x00003C60
		internal static void resetCounters()
		{
			berCounter.StartFrameNumber = 0;
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00005A68 File Offset: 0x00003C68
		public static void recording(byte[] DMRvoice, byte controlByte)
		{
			if (Form1.record)
			{
				if (controlByte == 65)
				{
					Convert.ToBase64String(DMRvoice);
					Settings.Default.Save();
				}
				if (controlByte == 66)
				{
					Convert.ToBase64String(DMRvoice);
					Settings.Default.Save();
				}
			}
			Form1.record = false;
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00005AA4 File Offset: 0x00003CA4
		public static void ber(byte[] DMRvoice, byte type)
		{
			if ((type & 255) == 65)
			{
				berCounter.golayTotalPercentCounter = 0L;
				berCounter.golayTotalPacketCounter = 0L;
				berCounter.totalBERUitkomst = 0L;
				berCounter.totalBERBits = 0L;
			}
			byte b = type & byte.MaxValue;
			if ((type & 255) != 65 && (type & 255) != 66)
			{
				byte[] array = new byte[4];
				Buffer.BlockCopy(DMRvoice, 0, array, 0, 4);
				byte[] array2 = new byte[4];
				Buffer.BlockCopy(DMRvoice, 4, array2, 0, 4);
				byte[] array3 = new byte[4];
				Buffer.BlockCopy(DMRvoice, 8, array3, 0, 1);
				long num = utils.byte2longMS(array);
				long num2 = utils.byte2longMS(array2);
				long num3 = utils.byte2longMS(array3);
				long num4 = 0L;
				long num5 = 0L;
				if (((num >> 31) & 1L) != 0L)
				{
					num4 |= 2048L;
				}
				if (((num >> 27) & 1L) != 0L)
				{
					num4 |= 1024L;
				}
				if (((num >> 23) & 1L) != 0L)
				{
					num4 |= 512L;
				}
				if (((num >> 19) & 1L) != 0L)
				{
					num4 |= 256L;
				}
				if (((num >> 15) & 1L) != 0L)
				{
					num4 |= 128L;
				}
				if (((num >> 11) & 1L) != 0L)
				{
					num4 |= 64L;
				}
				if (((num >> 7) & 1L) != 0L)
				{
					num4 |= 32L;
				}
				if (((num >> 3) & 1L) != 0L)
				{
					num4 |= 16L;
				}
				if (((num2 >> 31) & 1L) != 0L)
				{
					num4 |= 8L;
				}
				if (((num2 >> 27) & 1L) != 0L)
				{
					num4 |= 4L;
				}
				if (((num2 >> 23) & 1L) != 0L)
				{
					num4 |= 2L;
				}
				if (((num2 >> 19) & 1L) != 0L)
				{
					num4 |= 1L;
				}
				if (((num2 >> 15) & 1L) != 0L)
				{
					num5 |= 2048L;
				}
				if (((num2 >> 11) & 1L) != 0L)
				{
					num5 |= 1024L;
				}
				if (((num2 >> 7) & 1L) != 0L)
				{
					num5 |= 512L;
				}
				if (((num2 >> 3) & 1L) != 0L)
				{
					num5 |= 256L;
				}
				if (((num3 >> 31) & 1L) != 0L)
				{
					num5 |= 128L;
				}
				if (((num3 >> 27) & 1L) != 0L)
				{
					num5 |= 64L;
				}
				if (((num >> 30) & 1L) != 0L)
				{
					num5 |= 32L;
				}
				if (((num >> 26) & 1L) != 0L)
				{
					num5 |= 16L;
				}
				if (((num >> 22) & 1L) != 0L)
				{
					num5 |= 8L;
				}
				if (((num >> 18) & 1L) != 0L)
				{
					num5 |= 4L;
				}
				if (((num >> 14) & 1L) != 0L)
				{
					num5 |= 2L;
				}
				if (((num >> 10) & 1L) != 0L)
				{
					num5 |= 1L;
				}
				long num6 = 3189L;
				for (int i = 1; i <= 12; i++)
				{
					if (((num4 >> 11) & 1L) != 1L)
					{
						num4 <<= 1;
						num4 |= 0L;
					}
					else
					{
						num4 ^= num6;
						i--;
					}
				}
				long num7 = num4 >> 1;
				long num8 = num5 >> 1;
				long num9 = num7 ^ num8;
				berCounter.totalBERUitkomst += (long)berCounter.SparseBitcount(num9);
				berCounter.totalBERBits += berCounter.NumberOfSetBits(num7);
				berCounter.golayTotalPercentCounter += (long)(berCounter.SparseBitcount(num9) * 100 / 7);
				berCounter.golayTotalPacketCounter += 1L;
				double num10 = (double)berCounter.totalBERUitkomst / (double)berCounter.totalBERBits;
				if ((type & 255) == 1 && (num10 != berCounter.golaylastPercentCounter || berCounter.golayTotalPacketCounter == 2L))
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						berCounter.ChangeStatusBER("BER " + string.Format("{0:0.00}", num10) + "%");
						break;
					case information.LANGUAGE.JAPANESE:
						berCounter.ChangeStatusBER("符号誤り " + string.Format("{0:0.00}", num10) + "%");
						break;
					case information.LANGUAGE.CHINEES:
						berCounter.ChangeStatusBER("BER " + string.Format("{0:0.00}", num10) + "%");
						break;
					case information.LANGUAGE.KOREAN:
						berCounter.ChangeStatusBER("BER " + string.Format("{0:0.00}", num10) + "%");
						break;
					}
					information.myBER = (int)num10;
					berCounter.golaylastPercentCounter = num10;
				}
			}
		}

		// Token: 0x0400003B RID: 59
		private static long golayTotalPercentCounter;

		// Token: 0x0400003C RID: 60
		private static long golayTotalPacketCounter;

		// Token: 0x0400003D RID: 61
		private static double golaylastPercentCounter;

		// Token: 0x0400003E RID: 62
		private static long totalBERUitkomst;

		// Token: 0x0400003F RID: 63
		private static long totalBERBits;

		// Token: 0x04000046 RID: 70
		private static bool[] m_deInterData = new bool[196];

		// Token: 0x04000047 RID: 71
		private static bool[] m_rawData = new bool[196];

		// Token: 0x04000048 RID: 72
		private static int StartFrameNumber = 0;

		// Token: 0x04000049 RID: 73
		private static byte[] CRCding = new byte[9];

		// Token: 0x0400004A RID: 74
		private static bool[] totalMiddel = new bool[128];
	}
}
