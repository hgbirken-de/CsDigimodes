using System;
using System.Collections;

namespace BlueDV
{
	// Token: 0x02000011 RID: 17
	internal class DecodeDMR
	{
		// Token: 0x060000CF RID: 207 RVA: 0x000072E8 File Offset: 0x000054E8
		public static void PrintValues(IEnumerable myList, int myWidth)
		{
			int num = myWidth;
			foreach (object obj in myList)
			{
				if (num <= 0)
				{
					num = myWidth;
					Console.WriteLine();
				}
				num--;
				Console.Write("{0,8}", obj);
			}
			Console.WriteLine();
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x00007354 File Offset: 0x00005554
		public static void kanweg(byte[] voice, byte type)
		{
			try
			{
				bool[] array = new bool[48];
				bool[] array2 = utils.byteToBitsBE(voice[13]);
				for (int i = 4; i < 8; i++)
				{
					array[i - 4] = array2[i];
				}
				bool[] array3 = utils.byteToBitsBE(voice[14]);
				for (int j = 0; j < 8; j++)
				{
					array[j + 4] = array3[j];
				}
				bool[] array4 = utils.byteToBitsBE(voice[15]);
				for (int k = 0; k < 8; k++)
				{
					array[k + 12] = array4[k];
				}
				bool[] array5 = utils.byteToBitsBE(voice[16]);
				for (int l = 0; l < 8; l++)
				{
					array[l + 20] = array5[l];
				}
				bool[] array6 = utils.byteToBitsBE(voice[17]);
				for (int m = 0; m < 8; m++)
				{
					array[m + 28] = array6[m];
				}
				bool[] array7 = utils.byteToBitsBE(voice[18]);
				for (int n = 0; n < 8; n++)
				{
					array[n + 36] = array7[n];
				}
				bool[] array8 = utils.byteToBitsBE(voice[19]);
				for (int num = 0; num < 4; num++)
				{
					array[num + 44] = array8[num];
				}
				for (int num2 = 0; num2 < 48; num2++)
				{
				}
				bool[] array9 = new bool[16];
				Buffer.BlockCopy(array, 0, array9, 0, 8);
				Buffer.BlockCopy(array, 40, array9, 8, 8);
				int num3;
				if (array9[0])
				{
					num3 = 8;
				}
				else
				{
					num3 = 0;
				}
				if (array9[1])
				{
					num3 += 4;
				}
				if (array9[2])
				{
					num3 += 2;
				}
				if (array9[3])
				{
					num3++;
				}
				int num4;
				if (array9[5])
				{
					num4 = 2;
				}
				else
				{
					num4 = 0;
				}
				if (array9[6])
				{
					num4++;
				}
				if (num4 == 0 || num4 == 1 || num4 != 2)
				{
				}
				foreach (bool flag in array9)
				{
				}
				bool[] array11 = new bool[7];
				Buffer.BlockCopy(array9, 0, array11, 0, 7);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x0000755C File Offset: 0x0000575C
		public static bool[] GetEMBfromVoice(byte[] voice)
		{
			bool[] array = new bool[48];
			bool[] array2 = utils.byteToBitsBE(voice[13]);
			for (int i = 4; i < 8; i++)
			{
				array[i - 4] = array2[i];
			}
			bool[] array3 = utils.byteToBitsBE(voice[14]);
			for (int j = 0; j < 8; j++)
			{
				array[j + 4] = array3[j];
			}
			bool[] array4 = utils.byteToBitsBE(voice[15]);
			for (int k = 0; k < 8; k++)
			{
				array[k + 12] = array4[k];
			}
			bool[] array5 = utils.byteToBitsBE(voice[16]);
			for (int l = 0; l < 8; l++)
			{
				array[l + 20] = array5[l];
			}
			bool[] array6 = utils.byteToBitsBE(voice[17]);
			for (int m = 0; m < 8; m++)
			{
				array[m + 28] = array6[m];
			}
			bool[] array7 = utils.byteToBitsBE(voice[18]);
			for (int n = 0; n < 8; n++)
			{
				array[n + 36] = array7[n];
			}
			bool[] array8 = utils.byteToBitsBE(voice[19]);
			for (int num = 0; num < 4; num++)
			{
				array[num + 44] = array8[num];
			}
			return array;
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x0000767C File Offset: 0x0000587C
		private static bool QuadResidue1676(bool[] word)
		{
			int[] array = new int[]
			{
				0, 627, 1253, 1686, 2505, 3002, 3372, 3935, 4578, 5009,
				5383, 6004, 6187, 6744, 7374, 7869, 8631, 9156, 9554, 10017,
				10366, 10765, 11419, 12008, 12373, 12838, 13488, 14019, 14748, 15343,
				15737, 16138, 16670, 17261, 17915, 18312, 18647, 19108, 19506, 20033,
				20732, 21135, 21529, 22122, 22837, 23366, 24016, 24483, 24745, 25306,
				25676, 26175, 26976, 27411, 28037, 28662, 29003, 29496, 30126, 30685,
				30850, 31473, 31847, 32276, 32847, 33340, 33962, 34521, 35206, 35829,
				36195, 36624, 37293, 37854, 38216, 38715, 39012, 39447, 40065, 40690,
				41464, 41867, 42269, 42862, 43057, 43586, 44244, 44711, 45082, 45673,
				46335, 46732, 47571, 48032, 48438, 48965, 49489, 49954, 50612, 51143,
				51352, 51947, 52349, 52750, 53427, 53952, 54358, 54821, 55674, 56073,
				56735, 57324, 57574, 58005, 58371, 58992, 59695, 60252, 60874, 61369,
				61700, 62327, 62945, 63378, 63693, 64190, 64552, 65115
			};
			if (word[15])
			{
				DecodeDMR.residueValue = 1;
			}
			else
			{
				DecodeDMR.residueValue = 0;
			}
			if (word[14])
			{
				DecodeDMR.residueValue += 2;
			}
			if (word[13])
			{
				DecodeDMR.residueValue += 4;
			}
			if (word[12])
			{
				DecodeDMR.residueValue += 8;
			}
			if (word[11])
			{
				DecodeDMR.residueValue += 16;
			}
			if (word[10])
			{
				DecodeDMR.residueValue += 32;
			}
			if (word[9])
			{
				DecodeDMR.residueValue += 64;
			}
			if (word[8])
			{
				DecodeDMR.residueValue += 128;
			}
			if (word[7])
			{
				DecodeDMR.residueValue += 256;
			}
			if (word[6])
			{
				DecodeDMR.residueValue += 512;
			}
			if (word[5])
			{
				DecodeDMR.residueValue += 1024;
			}
			if (word[4])
			{
				DecodeDMR.residueValue += 2048;
			}
			if (word[3])
			{
				DecodeDMR.residueValue += 4096;
			}
			if (word[2])
			{
				DecodeDMR.residueValue += 8192;
			}
			if (word[1])
			{
				DecodeDMR.residueValue += 16384;
			}
			if (word[0])
			{
				DecodeDMR.residueValue += 32768;
			}
			for (int i = 0; i < 128; i++)
			{
				if (DecodeDMR.residueValue == array[i])
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x000077FC File Offset: 0x000059FC
		public static bool calcQuadResidue1676(bool[] d)
		{
			bool[] array = new bool[9];
			int[] array2 = new int[128];
			for (int i = 0; i < 128; i++)
			{
				if ((i & 64) > 0)
				{
					d[0] = true;
				}
				else
				{
					d[0] = false;
				}
				if ((i & 32) > 0)
				{
					d[1] = true;
				}
				else
				{
					d[1] = false;
				}
				if ((i & 16) > 0)
				{
					d[2] = true;
				}
				else
				{
					d[2] = false;
				}
				if ((i & 8) > 0)
				{
					d[3] = true;
				}
				else
				{
					d[3] = false;
				}
				if ((i & 4) > 0)
				{
					d[4] = true;
				}
				else
				{
					d[4] = false;
				}
				if ((i & 2) > 0)
				{
					d[5] = true;
				}
				else
				{
					d[5] = false;
				}
				if ((i & 1) > 0)
				{
					d[6] = true;
				}
				else
				{
					d[6] = false;
				}
				array2[i] = i << 9;
				array[0] = d[1] ^ d[2] ^ d[3] ^ d[4];
				array[1] = d[2] ^ d[3] ^ d[4] ^ d[5];
				array[2] = d[0] ^ d[3] ^ d[4] ^ d[5] ^ d[6];
				array[3] = d[2] ^ d[3] ^ d[5] ^ d[6];
				array[4] = d[1] ^ d[2] ^ d[6];
				array[5] = d[0] ^ d[1] ^ d[4];
				array[6] = d[0] ^ d[1] ^ d[2] ^ d[5];
				array[7] = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[6];
				array[8] = d[0] ^ d[2] ^ d[4] ^ d[5] ^ d[6];
				if (array[0])
				{
					array2[i] += 256;
				}
				if (array[1])
				{
					array2[i] += 128;
				}
				if (array[2])
				{
					array2[i] += 64;
				}
				if (array[3])
				{
					array2[i] += 32;
				}
				if (array[4])
				{
					array2[i] += 16;
				}
				if (array[5])
				{
					array2[i] += 8;
				}
				if (array[6])
				{
					array2[i] += 4;
				}
				if (array[7])
				{
					array2[i] += 2;
				}
				if (array[8])
				{
					array2[i]++;
				}
			}
			for (int j = 0; j < 9; j++)
			{
			}
			return true;
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x000079E0 File Offset: 0x00005BE0
		public static byte[] ChangeColorcode(byte[] voice)
		{
			bool[] embfromVoice = DecodeDMR.GetEMBfromVoice(voice);
			embfromVoice[0] = false;
			embfromVoice[1] = false;
			embfromVoice[2] = false;
			embfromVoice[3] = true;
			return null;
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x000079F8 File Offset: 0x00005BF8
		public static bool[] calculateQR(bool[] emb_in)
		{
			bool[] array = new bool[7];
			Buffer.BlockCopy(emb_in, 0, array, 0, 7);
			int num = utils.ToNumeral(array, 7);
			int num2 = DecodeDMR.ENCODING_TABLE_1676[num];
			bool[] array2 = new bool[32];
			utils.ToBinary(num2).CopyTo(array2, 0);
			byte[] array3 = new byte[]
			{
				(byte)(num2 >> 8),
				(byte)num2
			};
			bool[] array4 = utils.byteToBitsBE(array3[0]);
			Array array5 = utils.byteToBitsBE(array3[1]);
			bool[] array6 = new bool[16];
			Buffer.BlockCopy(array4, 0, array6, 0, 8);
			Buffer.BlockCopy(array5, 0, array6, 8, 8);
			return array6;
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x00007A80 File Offset: 0x00005C80
		public static void decode(byte[] voice, byte type)
		{
			try
			{
				bool[] array = new bool[40];
				bool[] array2 = utils.byteToBitsBE(voice[14]);
				for (int i = 4; i < 8; i++)
				{
					array[i - 4] = array2[i];
				}
				bool[] array3 = utils.byteToBitsBE(voice[15]);
				for (int j = 0; j < 8; j++)
				{
					array[j + 4] = array3[j];
				}
				bool[] array4 = utils.byteToBitsBE(voice[16]);
				for (int k = 0; k < 8; k++)
				{
					array[k + 12] = array4[k];
				}
				bool[] array5 = utils.byteToBitsBE(voice[17]);
				for (int l = 0; l < 8; l++)
				{
					array[l + 20] = array5[l];
				}
				bool[] array6 = utils.byteToBitsBE(voice[18]);
				for (int m = 0; m < 4; m++)
				{
					array[m + 28] = array6[m];
				}
				new bool[32];
				new bool[32];
				new bool[32];
				new bool[32];
				switch (type)
				{
				case 1:
				{
					for (int n = 0; n < 32; n++)
					{
						DecodeDMR.rawLC[n] = array[n];
					}
					break;
				}
				case 2:
				{
					for (int num = 0; num < 32; num++)
					{
						DecodeDMR.rawLC[num + 32] = array[num];
					}
					break;
				}
				case 3:
				{
					for (int num2 = 0; num2 < 32; num2++)
					{
						DecodeDMR.rawLC[num2 + 64] = array[num2];
					}
					break;
				}
				case 4:
				{
					for (int num3 = 0; num3 < 32; num3++)
					{
						DecodeDMR.rawLC[num3 + 96] = array[num3];
					}
					break;
				}
				case 5:
				{
					for (int num4 = 0; num4 < 128; num4++)
					{
					}
					break;
				}
				default:
					if (type != 32)
					{
					}
					break;
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x00007C54 File Offset: 0x00005E54
		private static bool processMultiBlockEmbeddedLC()
		{
			int i = 0;
			bool[] array = new bool[128];
			bool[] array2 = new bool[16];
			for (int j = 0; j < 128; j++)
			{
				array[i] = DecodeDMR.rawLC[j];
				i += 16;
				if (i > 127)
				{
					i -= 127;
				}
			}
			for (int j = 0; j <= 96; j += 16)
			{
				for (i = 0; i < 16; i++)
				{
					array2[i] = array[j + i];
				}
				foreach (bool flag in array2)
				{
				}
				DecodeDMR.repairHamming16114(array2);
				for (i = 0; i < 16; i++)
				{
					array[j + i] = array2[i];
				}
			}
			i = 0;
			for (int j = 0; j < 11; j++)
			{
				DecodeDMR.lcData[i] = array[j];
				i++;
			}
			for (int j = 16; j < 27; j++)
			{
				DecodeDMR.lcData[i] = array[j];
				i++;
			}
			for (int j = 32; j < 42; j++)
			{
				DecodeDMR.lcData[i] = array[j];
				i++;
			}
			for (int j = 48; j < 58; j++)
			{
				DecodeDMR.lcData[i] = array[j];
				i++;
			}
			for (int j = 64; j < 74; j++)
			{
				DecodeDMR.lcData[i] = array[j];
				i++;
			}
			for (int j = 80; j < 90; j++)
			{
				DecodeDMR.lcData[i] = array[j];
				i++;
			}
			for (int j = 96; j < 106; j++)
			{
				DecodeDMR.lcData[i] = array[j];
				i++;
			}
			try
			{
				new bool[72];
				int num = 0;
				bool[] array4 = new bool[8];
				for (int l = 0; l < 8; l++)
				{
					array4[l] = DecodeDMR.lcData[num];
					num++;
				}
				utils.bitsToByteBE(array4);
				for (int m = 0; m < 8; m++)
				{
					array4[m] = DecodeDMR.lcData[num];
					num++;
				}
				utils.bitsToByteBE(array4);
				for (int n = 0; n < 8; n++)
				{
					array4[n] = DecodeDMR.lcData[num];
					num++;
				}
				utils.bitsToByteBE(array4);
				for (int num2 = 0; num2 < 8; num2++)
				{
					array4[num2] = DecodeDMR.lcData[num];
					num++;
				}
				byte b = utils.bitsToByteBE(array4);
				for (int num3 = 0; num3 < 8; num3++)
				{
					array4[num3] = DecodeDMR.lcData[num];
					num++;
				}
				byte b2 = utils.bitsToByteBE(array4);
				for (int num4 = 0; num4 < 8; num4++)
				{
					array4[num4] = DecodeDMR.lcData[num];
					num++;
				}
				byte b3 = utils.bitsToByteBE(array4);
				for (int num5 = 0; num5 < 8; num5++)
				{
					array4[num5] = DecodeDMR.lcData[num];
					num++;
				}
				byte b4 = utils.bitsToByteBE(array4);
				for (int num6 = 0; num6 < 8; num6++)
				{
					array4[num6] = DecodeDMR.lcData[num];
					num++;
				}
				byte b5 = utils.bitsToByteBE(array4);
				for (int num7 = 0; num7 < 8; num7++)
				{
					array4[num7] = DecodeDMR.lcData[num];
					num++;
				}
				byte b6 = utils.bitsToByteBE(array4);
				long num8 = (long)((long)b << 16);
				num8 += (long)((long)b2 << 8);
				information.myResolvedDstDMRID = (num8 + (long)((ulong)b3)).ToString();
				long num9 = (long)((long)b4 << 16) + (long)((long)b5 << 8);
			}
			catch (Exception)
			{
			}
			uint num10;
			if (array[42])
			{
				num10 = 16U;
			}
			else
			{
				num10 = 0U;
			}
			if (array[58])
			{
				num10 += 8U;
			}
			if (array[74])
			{
				num10 += 4U;
			}
			if (array[90])
			{
				num10 += 2U;
			}
			if (array[106])
			{
				num10 += 1U;
			}
			if (!new crc().crcFiveBitNew(DecodeDMR.lcData, num10))
			{
				return false;
			}
			for (int j = 0; j < 128; j++)
			{
			}
			for (int j = 0; j < 72; j++)
			{
			}
			DecodeDMR.dataReady = true;
			return true;
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x00008034 File Offset: 0x00006234
		public static bool hamming16114(bool[] d)
		{
			bool[] array = new bool[]
			{
				d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[5] ^ d[7] ^ d[8],
				d[1] ^ d[2] ^ d[3] ^ d[4] ^ d[6] ^ d[8] ^ d[9],
				d[2] ^ d[3] ^ d[4] ^ d[5] ^ d[7] ^ d[9] ^ d[10],
				d[0] ^ d[1] ^ d[2] ^ d[4] ^ d[6] ^ d[7] ^ d[10],
				d[0] ^ d[2] ^ d[5] ^ d[6] ^ d[8] ^ d[9] ^ d[10]
			};
			byte b = 0;
			b |= ((array[0] != d[11]) ? 1 : 0);
			b |= ((array[1] != d[12]) ? 2 : 0);
			b |= ((array[2] != d[13]) ? 4 : 0);
			b |= ((array[3] != d[14]) ? 8 : 0);
			switch (b | ((array[4] != d[15]) ? 16 : 0))
			{
			case 0:
				return true;
			case 1:
				d[11] = !d[11];
				return true;
			case 2:
				d[12] = !d[12];
				return true;
			case 4:
				d[13] = !d[13];
				return true;
			case 7:
				d[3] = !d[3];
				return true;
			case 8:
				d[14] = !d[14];
				return true;
			case 11:
				d[1] = !d[1];
				return true;
			case 13:
				d[7] = !d[7];
				return true;
			case 14:
				d[4] = !d[4];
				return true;
			case 16:
				d[15] = !d[15];
				return true;
			case 19:
				d[8] = !d[8];
				return true;
			case 21:
				d[5] = !d[5];
				return true;
			case 22:
				d[9] = !d[9];
				return true;
			case 25:
				d[0] = !d[0];
				return true;
			case 26:
				d[6] = !d[6];
				return true;
			case 28:
				d[10] = !d[10];
				return true;
			case 31:
				d[2] = !d[2];
				return true;
			}
			return false;
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x00008288 File Offset: 0x00006488
		public static bool[] encode16114(bool[] d)
		{
			d[11] = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[5] ^ d[7] ^ d[8];
			d[12] = d[1] ^ d[2] ^ d[3] ^ d[4] ^ d[6] ^ d[8] ^ d[9];
			d[13] = d[2] ^ d[3] ^ d[4] ^ d[5] ^ d[7] ^ d[9] ^ d[10];
			d[14] = d[0] ^ d[1] ^ d[2] ^ d[4] ^ d[6] ^ d[7] ^ d[10];
			d[15] = d[0] ^ d[2] ^ d[5] ^ d[6] ^ d[8] ^ d[9] ^ d[10];
			return d;
		}

		// Token: 0x060000DA RID: 218 RVA: 0x00008338 File Offset: 0x00006538
		public static bool[] encodehamming15113(bool[] d)
		{
			d[11] = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[5] ^ d[7] ^ d[8];
			d[12] = d[1] ^ d[2] ^ d[3] ^ d[4] ^ d[6] ^ d[8] ^ d[9];
			d[13] = d[2] ^ d[3] ^ d[4] ^ d[5] ^ d[7] ^ d[9] ^ d[10];
			d[14] = d[0] ^ d[1] ^ d[2] ^ d[4] ^ d[6] ^ d[7] ^ d[10];
			return d;
		}

		// Token: 0x060000DB RID: 219 RVA: 0x000083C8 File Offset: 0x000065C8
		public static bool[] hamming1393(bool[] d)
		{
			return new bool[]
			{
				d[0] ^ d[1] ^ d[3] ^ d[5] ^ d[6],
				d[0] ^ d[1] ^ d[2] ^ d[4] ^ d[6] ^ d[7],
				d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[5] ^ d[7] ^ d[8],
				d[0] ^ d[2] ^ d[4] ^ d[5] ^ d[8]
			};
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00008440 File Offset: 0x00006640
		private static bool[] getEMBbits(bool[] bits)
		{
			bool[] array = new bool[32];
			for (int i = 0; i < 32; i++)
			{
				array[i] = bits[i + 116];
			}
			return array;
		}

		// Token: 0x060000DD RID: 221 RVA: 0x0000846C File Offset: 0x0000666C
		private bool[] deInterleaveShortLC(bool[] raw)
		{
			int[] array = new int[]
			{
				0, 4, 8, 12, 16, 20, 24, 28, 32, 36,
				40, 44, 1, 5, 9, 13, 17, 21, 25, 29,
				33, 37, 41, 45, 2, 6, 10, 14, 18, 22,
				26, 30, 34, 38, 42, 46
			};
			bool[] array2 = new bool[36];
			for (int i = 0; i < 36; i++)
			{
				int num = array[i];
				array2[i] = raw[num];
			}
			return array2;
		}

		// Token: 0x060000DE RID: 222 RVA: 0x000084AC File Offset: 0x000066AC
		private bool shortLCcrc(bool[] dataBits)
		{
			crc crc = new crc();
			crc.setCrc8Value(0);
			for (int i = 0; i < dataBits.Length; i++)
			{
				crc.crc8(dataBits[i]);
			}
			return crc.getCrc8Value() == 0;
		}

		// Token: 0x060000DF RID: 223 RVA: 0x000084E8 File Offset: 0x000066E8
		private static bool[] interleave(bool[] emb)
		{
			bool[] array = new bool[196];
			BitArray bitArray = new BitArray(196);
			int num = 0;
			while ((long)num < 196L)
			{
				array[num] = false;
				num++;
			}
			int num2 = 0;
			while ((long)num2 < 196L)
			{
				long num3 = (long)num2 * 181L % 196L;
				array[num2] = bitArray[(int)num3];
				num2++;
			}
			return array;
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x00008554 File Offset: 0x00006754
		public static bool repairHamming16114(bool[] Bit_int)
		{
			int num = 0;
			bool flag = Bit_int[num] ^ Bit_int[1 + num] ^ Bit_int[2 + num] ^ Bit_int[3 + num] ^ Bit_int[5 + num] ^ Bit_int[7 + num] ^ Bit_int[8 + num];
			bool flag2 = Bit_int[1 + num] ^ Bit_int[2 + num] ^ Bit_int[3 + num] ^ Bit_int[4 + num] ^ Bit_int[6 + num] ^ Bit_int[8 + num] ^ Bit_int[9 + num];
			bool flag3 = Bit_int[2 + num] ^ Bit_int[3 + num] ^ Bit_int[4 + num] ^ Bit_int[5 + num] ^ Bit_int[7 + num] ^ Bit_int[9 + num] ^ Bit_int[10 + num];
			bool flag4 = Bit_int[num] ^ Bit_int[1 + num] ^ Bit_int[2 + num] ^ Bit_int[4 + num] ^ Bit_int[6 + num] ^ Bit_int[7 + num] ^ Bit_int[10 + num];
			bool flag5 = Bit_int[num] ^ Bit_int[2 + num] ^ Bit_int[5 + num] ^ Bit_int[6 + num] ^ Bit_int[8 + num] ^ Bit_int[9 + num] ^ Bit_int[10 + num];
			byte b = 0;
			b |= ((flag != Bit_int[11 + num]) ? 1 : 0);
			b |= ((flag2 != Bit_int[12 + num]) ? 2 : 0);
			b |= ((flag3 != Bit_int[13 + num]) ? 4 : 0);
			b |= ((flag4 != Bit_int[14 + num]) ? 8 : 0);
			switch (b | ((flag5 != Bit_int[15 + num]) ? 16 : 0))
			{
			case 0:
				return true;
			case 1:
				Bit_int[11] = !Bit_int[11];
				return true;
			case 2:
				Bit_int[12] = !Bit_int[12];
				return true;
			case 4:
				Bit_int[13] = !Bit_int[13];
				return true;
			case 7:
				Bit_int[3] = !Bit_int[3];
				return true;
			case 8:
				Bit_int[14] = !Bit_int[14];
				return true;
			case 11:
				Bit_int[1] = !Bit_int[1];
				return true;
			case 13:
				Bit_int[7] = !Bit_int[7];
				return true;
			case 14:
				Bit_int[4] = !Bit_int[4];
				return true;
			case 16:
				Bit_int[15] = !Bit_int[15];
				return true;
			case 19:
				Bit_int[8] = !Bit_int[8];
				return true;
			case 21:
				Bit_int[5] = !Bit_int[5];
				return true;
			case 22:
				Bit_int[9] = !Bit_int[9];
				return true;
			case 25:
				Bit_int[0] = !Bit_int[0];
				return true;
			case 26:
				Bit_int[6] = !Bit_int[6];
				return true;
			case 28:
				Bit_int[10] = !Bit_int[10];
				return true;
			case 31:
				Bit_int[2] = !Bit_int[2];
				return true;
			}
			return false;
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x000087EC File Offset: 0x000069EC
		public static bool repairHamming15113(bool[] Bit_int)
		{
			int num = 0;
			bool flag = Bit_int[1 + num] ^ Bit_int[16 + num] ^ Bit_int[46 + num] ^ Bit_int[76 + num] ^ Bit_int[91 + num];
			bool flag2 = Bit_int[1 + num] ^ Bit_int[16 + num] ^ Bit_int[31 + num] ^ Bit_int[61 + num] ^ Bit_int[91 + num] ^ Bit_int[106 + num];
			bool flag3 = Bit_int[1 + num] ^ Bit_int[16 + num] ^ Bit_int[31 + num] ^ Bit_int[46 + num] ^ Bit_int[76 + num] ^ Bit_int[106 + num] ^ Bit_int[121 + num];
			bool flag4 = Bit_int[1 + num] ^ Bit_int[31 + num] ^ Bit_int[61 + num] ^ Bit_int[76 + num] ^ Bit_int[121 + num];
			byte b = 0;
			b |= ((flag != Bit_int[136 + num]) ? 1 : 0);
			b |= ((flag2 != Bit_int[151 + num]) ? 2 : 0);
			b |= ((flag3 != Bit_int[166 + num]) ? 4 : 0);
			switch (b | ((flag4 != Bit_int[181 + num]) ? 8 : 0))
			{
			case 1:
				Bit_int[9] = !Bit_int[9];
				return true;
			case 2:
				Bit_int[10] = !Bit_int[10];
				return true;
			case 3:
				Bit_int[6] = !Bit_int[6];
				return true;
			case 4:
				Bit_int[11] = !Bit_int[11];
				return true;
			case 5:
				Bit_int[3] = !Bit_int[3];
				return true;
			case 6:
				Bit_int[7] = !Bit_int[7];
				return true;
			case 7:
				Bit_int[1] = !Bit_int[1];
				return true;
			case 8:
				Bit_int[12] = !Bit_int[12];
				return true;
			case 10:
				Bit_int[4] = !Bit_int[4];
				return true;
			case 12:
				Bit_int[8] = !Bit_int[8];
				return true;
			case 13:
				Bit_int[5] = !Bit_int[5];
				return true;
			case 14:
				Bit_int[2] = !Bit_int[2];
				return true;
			case 15:
				Bit_int[0] = !Bit_int[0];
				return true;
			}
			return false;
		}

		// Token: 0x0400006A RID: 106
		private static BitArray m_rawDataNotInterfeaved = new BitArray(196);

		// Token: 0x0400006B RID: 107
		private static bool[] rawLC = new bool[128];

		// Token: 0x0400006C RID: 108
		private static bool[] lcData = new bool[72];

		// Token: 0x0400006D RID: 109
		private static bool dataReady = false;

		// Token: 0x0400006E RID: 110
		private static string[] lines = new string[3];

		// Token: 0x0400006F RID: 111
		private static int residueValue;

		// Token: 0x04000070 RID: 112
		private static int[] ENCODING_TABLE_1676 = new int[]
		{
			0, 627, 1253, 1686, 2505, 3002, 3372, 3935, 4578, 5009,
			5383, 6004, 6187, 6744, 7374, 7869, 8631, 9156, 9554, 10017,
			10366, 10765, 11419, 12008, 12373, 12838, 13488, 14019, 14748, 15343,
			15737, 16138, 16670, 17261, 17915, 18312, 18647, 19108, 19506, 20033,
			20732, 21135, 21529, 22122, 22837, 23366, 24016, 24483, 24745, 25306,
			25676, 26175, 26976, 27411, 28037, 28662, 29003, 29496, 30126, 30685,
			30850, 31473, 31847, 32276, 32847, 33340, 33962, 34521, 35206, 35829,
			36195, 36624, 37293, 37854, 38216, 38715, 39012, 39447, 40065, 40690,
			41464, 41867, 42269, 42862, 43057, 43586, 44244, 44711, 45082, 45673,
			46335, 46732, 47571, 48032, 48438, 48965, 49489, 49954, 50612, 51143,
			51352, 51947, 52349, 52750, 53427, 53952, 54358, 54821, 55674, 56073,
			56735, 57324, 57574, 58005, 58371, 58992, 59695, 60252, 60874, 61369,
			61700, 62327, 62945, 63378, 63693, 64190, 64552, 65115
		};
	}
}
