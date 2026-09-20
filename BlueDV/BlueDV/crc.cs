using System;

namespace BlueDV
{
	// Token: 0x0200000E RID: 14
	internal class crc
	{
		// Token: 0x060000A1 RID: 161 RVA: 0x00005EE4 File Offset: 0x000040E4
		public void setCrc8Value(int crc8Value)
		{
			this.crc8Value = crc8Value;
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00005EED File Offset: 0x000040ED
		public int getCrc8Value()
		{
			return this.crc8Value;
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x00005EF8 File Offset: 0x000040F8
		public void crc8(bool bit)
		{
			bool flag = (this.crc8Value & 1) > 0;
			this.crc8Value >>= 1;
			if (bit ^ flag)
			{
				this.crc8Value ^= 224;
			}
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x00005F3C File Offset: 0x0000413C
		private void ccitt_crc16(int innn)
		{
			byte b = (byte)innn;
			for (int i = 0; i < 8; i++)
			{
				bool flag = ((this.crc16Value >> 15) & 1) == 1;
				bool flag2 = ((b >> 7 - i) & 1) == 1;
				this.crc16Value <<= 1;
				if (flag ^ flag2)
				{
					this.crc16Value ^= 4129;
				}
			}
			this.crc16Value &= 65535;
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x00005FAC File Offset: 0x000041AC
		public bool crcCSBK(bool[] innn)
		{
			this.crc16Value = 0;
			for (int i = 0; i < 96; i += 8)
			{
				int num = 0;
				for (int j = 0; j < 8; j++)
				{
					if (innn[i + j])
					{
						num += (int)Math.Pow(2.0, 7.0 - (double)j);
					}
				}
				if (i >= 80)
				{
					num ^= 165;
				}
				this.ccitt_crc16(num);
			}
			return this.crc16Value == 7439;
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00006024 File Offset: 0x00004224
		public bool crcDataHeader(bool[] innn)
		{
			this.crc16Value = 0;
			for (int i = 0; i < 96; i += 8)
			{
				int num = 0;
				for (int j = 0; j < 8; j++)
				{
					if (innn[i + j])
					{
						num += (int)Math.Pow(2.0, 7.0 - (double)j);
					}
				}
				if (i >= 80)
				{
					num ^= 204;
				}
				this.ccitt_crc16(num);
			}
			return this.crc16Value == 7439;
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x0000609C File Offset: 0x0000429C
		public bool RS129(bool[] innn)
		{
			int num = 0;
			int[] array = new int[12];
			for (int i = 0; i < 96; i += 8)
			{
				array[num] = 0;
				for (int j = 0; j < 8; j++)
				{
					int num2 = (int)Math.Pow(2.0, (double)(8 - j - 1));
					if (innn[i + j])
					{
						array[num] += num2;
					}
				}
				num++;
			}
			return false;
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00006100 File Offset: 0x00004300
		public bool crcFiveBitNew(bool[] inn, uint tcrc)
		{
			ushort num = 0;
			bool[] array = new bool[8];
			for (int i = 0; i < 72; i += 8)
			{
				for (int j = 0; j < 8; j++)
				{
					array[j] = inn[i + j];
				}
				byte b = utils.bitsToByteBE(array);
				num += (ushort)b;
			}
			num %= 31;
			return (uint)num == tcrc;
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00006154 File Offset: 0x00004354
		public char crcFiveBitNewNEW(bool[] inn)
		{
			char c = '\0';
			bool[] array = new bool[8];
			for (int i = 0; i < 72; i += 8)
			{
				for (int j = 0; j < 8; j++)
				{
					array[j] = inn[i + j];
				}
				char c2 = utils.bitsToByteBE2(array);
				c += c2;
			}
			return c % '\u001f';
		}

		// Token: 0x060000AA RID: 170 RVA: 0x000061A4 File Offset: 0x000043A4
		public static int crcFiveBitNEW(bool[] innie)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < 72; i++)
			{
				if (innie[i])
				{
					num2 += (int)Math.Pow(2.0, (double)num);
				}
				num++;
				if (num == 8)
				{
					num = 0;
					num3 += num2;
					num2 = 0;
				}
			}
			return num3 % 31;
		}

		// Token: 0x060000AB RID: 171 RVA: 0x000061F4 File Offset: 0x000043F4
		public static ushort crcFiveBitEncode(bool[] inn)
		{
			ushort num = 0;
			bool[] array = new bool[8];
			for (int i = 0; i < 72; i += 8)
			{
				for (int j = 0; j < 8; j++)
				{
					array[j] = inn[i + j];
				}
				byte b = utils.bitsToByteBE(array);
				num += (ushort)b;
			}
			return num % 31;
		}

		// Token: 0x060000AC RID: 172 RVA: 0x00006244 File Offset: 0x00004444
		public bool crcFiveBit(bool[] innn, int tcrc)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < 72; i++)
			{
				if (innn[i])
				{
					num2 += (int)Math.Pow(2.0, (double)num);
				}
				num++;
				if (num == 8)
				{
					num = 0;
					num3 += num2;
					num2 = 0;
				}
			}
			num3 %= 31;
			return num3 == tcrc;
		}

		// Token: 0x060000AD RID: 173 RVA: 0x0000629C File Offset: 0x0000449C
		public static ushort CalcCRC16(byte[] data)
		{
			ushort num = 0;
			for (int i = 0; i < data.Length; i++)
			{
				num ^= (ushort)(data[i] << 8);
				for (int j = 0; j < 8; j++)
				{
					if ((num & 32768) > 0)
					{
						num = (ushort)(((int)num << 1) ^ 32773);
					}
					else
					{
						num = (ushort)(num << 1);
					}
				}
			}
			return num;
		}

		// Token: 0x060000AE RID: 174 RVA: 0x000062EC File Offset: 0x000044EC
		public static int crcX25(byte[] bytes)
		{
			int[] array = new int[]
			{
				0, 4489, 8978, 12955, 17956, 22445, 25910, 29887, 35912, 40385,
				44890, 48851, 51820, 56293, 59774, 63735, 4225, 264, 13203, 8730,
				22181, 18220, 30135, 25662, 40137, 36160, 49115, 44626, 56045, 52068,
				63999, 59510, 8450, 12427, 528, 5017, 26406, 30383, 17460, 21949,
				44362, 48323, 36440, 40913, 60270, 64231, 51324, 55797, 12675, 8202,
				4753, 792, 30631, 26158, 21685, 17724, 48587, 44098, 40665, 36688,
				64495, 60006, 55549, 51572, 16900, 21389, 24854, 28831, 1056, 5545,
				10034, 14011, 52812, 57285, 60766, 64727, 34920, 39393, 43898, 47859,
				21125, 17164, 29079, 24606, 5281, 1320, 14259, 9786, 57037, 53060,
				64991, 60502, 39145, 35168, 48123, 43634, 25350, 29327, 16404, 20893,
				9506, 13483, 1584, 6073, 61262, 65223, 52316, 56789, 43370, 47331,
				35448, 39921, 29575, 25102, 20629, 16668, 13731, 9258, 5809, 1848,
				65487, 60998, 56541, 52564, 47595, 43106, 39673, 35696, 33800, 38273,
				42778, 46739, 49708, 54181, 57662, 61623, 2112, 6601, 11090, 15067,
				20068, 24557, 28022, 31999, 38025, 34048, 47003, 42514, 53933, 49956,
				61887, 57398, 6337, 2376, 15315, 10842, 24293, 20332, 32247, 27774,
				42250, 46211, 34328, 38801, 58158, 62119, 49212, 53685, 10562, 14539,
				2640, 7129, 28518, 32495, 19572, 24061, 46475, 41986, 38553, 34576,
				62383, 57894, 53437, 49460, 14787, 10314, 6865, 2904, 32743, 28270,
				23797, 19836, 50700, 55173, 58654, 62615, 32808, 37281, 41786, 45747,
				19012, 23501, 26966, 30943, 3168, 7657, 12146, 16123, 54925, 50948,
				62879, 58390, 37033, 33056, 46011, 41522, 23237, 19276, 31191, 26718,
				7393, 3432, 16371, 11898, 59150, 63111, 50204, 54677, 41258, 45219,
				33336, 37809, 27462, 31439, 18516, 23005, 11618, 15595, 3696, 8185,
				63375, 58886, 54429, 50452, 45483, 40994, 37561, 33584, 31687, 27214,
				22741, 18780, 15843, 11370, 7921, 3960
			};
			int num = 65535;
			for (int i = 0; i < bytes.Length; i++)
			{
				byte b = (byte)(num ^ (int)bytes[i]);
				num = (int)((ushort)((num >> 8) ^ array[(int)b]));
			}
			return num ^ 65535;
		}

		// Token: 0x0400004B RID: 75
		private int crc8Value;

		// Token: 0x0400004C RID: 76
		private int crc16Value;
	}
}
