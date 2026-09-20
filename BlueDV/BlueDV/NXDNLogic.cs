using System;

namespace BlueDV
{
	// Token: 0x02000031 RID: 49
	internal class NXDNLogic
	{
		// Token: 0x0600039F RID: 927 RVA: 0x00027B7C File Offset: 0x00025D7C
		public static byte[] deinterleave_ambe(byte[] d)
		{
			byte[] array = new byte[49];
			byte[] array2 = new byte[7];
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					array[j + 8 * i] = (byte)(1 & (d[i] >> 7 - j));
				}
			}
			array[48] = (byte)(1 & (d[6] >> 7));
			for (int k = 0; k < 49; k++)
			{
				int num = NXDNLogic.dvsi_interleave[k];
				byte[] array3 = array2;
				int num2 = k / 8;
				array3[num2] += (byte)(array[num] << 7 - k % 8);
			}
			return array2;
		}

		// Token: 0x060003A0 RID: 928 RVA: 0x00027C0A File Offset: 0x00025E0A
		public static byte get_lich_fct(byte lich)
		{
			return (byte)((lich >> 4) & 3);
		}

		// Token: 0x060003A1 RID: 929 RVA: 0x00027C12 File Offset: 0x00025E12
		public static void setGroupCall(bool stat)
		{
			NXDNLogic.group = stat;
		}

		// Token: 0x060003A2 RID: 930 RVA: 0x00027C1C File Offset: 0x00025E1C
		public static void makeNXDNvoiceFrameFromAMBE(byte[] voice)
		{
			NXDNLogic.counter = ++NXDNLogic.counter % 4;
			Buffer.BlockCopy(voice, 0, NXDNLogic.AMBEFour, NXDNLogic.counter * 7, 7);
			if (NXDNLogic.counter == 3)
			{
				NXDNConnect.getFrame(NXDNLogic.AMBEFour, false);
				NXDNLogic.AMBEFour = new byte[28];
			}
		}

		// Token: 0x060003A3 RID: 931 RVA: 0x00027C71 File Offset: 0x00025E71
		public static void set_lich_rfct(byte rfct)
		{
			NXDNLogic.m_lich &= 63;
			NXDNLogic.m_lich |= (byte)(((int)rfct << 6) & 192);
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x00027C97 File Offset: 0x00025E97
		public static int get_lich_rfct()
		{
			return (NXDNLogic.m_lich >> 6) & 3;
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x00027CA2 File Offset: 0x00025EA2
		public static void set_lich_fct(int fct)
		{
			NXDNLogic.m_lich &= 207;
			NXDNLogic.m_lich |= (byte)((fct << 4) & 48);
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x00027CC8 File Offset: 0x00025EC8
		public static int get_lich_fct()
		{
			return (NXDNLogic.m_lich >> 4) & 3;
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x00027CD3 File Offset: 0x00025ED3
		public static void set_lich_option(byte opt)
		{
			NXDNLogic.m_lich &= 243;
			NXDNLogic.m_lich |= (byte)(((int)opt << 2) & 12);
		}

		// Token: 0x060003A8 RID: 936 RVA: 0x00027CF9 File Offset: 0x00025EF9
		public static int get_lich_option()
		{
			return (NXDNLogic.m_lich >> 2) & 3;
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x00027D04 File Offset: 0x00025F04
		public static void set_lich_direction(byte dir)
		{
			NXDNLogic.m_lich &= 253;
			NXDNLogic.m_lich |= (byte)(((int)dir << 1) & 2);
		}

		// Token: 0x060003AA RID: 938 RVA: 0x00027D29 File Offset: 0x00025F29
		public static int get_lich_direction()
		{
			return (NXDNLogic.m_lich >> 1) & 1;
		}

		// Token: 0x060003AB RID: 939 RVA: 0x00027D34 File Offset: 0x00025F34
		public static void set_lich(byte data)
		{
			NXDNLogic.m_lich = data;
		}

		// Token: 0x060003AC RID: 940 RVA: 0x00027D3C File Offset: 0x00025F3C
		public static byte get_lich()
		{
			bool flag = true;
			int num = (int)(NXDNLogic.m_lich & 240);
			if (num != 128)
			{
				flag = num == 176;
			}
			if (flag)
			{
				NXDNLogic.m_lich |= 1;
			}
			else
			{
				NXDNLogic.m_lich &= 254;
			}
			return NXDNLogic.m_lich;
		}

		// Token: 0x060003AD RID: 941 RVA: 0x00027D95 File Offset: 0x00025F95
		public static void set_sacch_ran(int ran)
		{
			byte[] sacch = NXDNLogic.m_sacch;
			int num = 0;
			sacch[num] &= 192;
			byte[] sacch2 = NXDNLogic.m_sacch;
			int num2 = 0;
			sacch2[num2] |= (byte)ran;
		}

		// Token: 0x060003AE RID: 942 RVA: 0x00027DBE File Offset: 0x00025FBE
		public static int get_sacch_ran()
		{
			return (int)(NXDNLogic.m_sacch[0] & 63);
		}

		// Token: 0x060003AF RID: 943 RVA: 0x00027DCA File Offset: 0x00025FCA
		public static void set_sacch_struct(int s)
		{
			byte[] sacch = NXDNLogic.m_sacch;
			int num = 0;
			sacch[num] &= 63;
			byte[] sacch2 = NXDNLogic.m_sacch;
			int num2 = 0;
			sacch2[num2] |= (byte)((s << 6) & 192);
		}

		// Token: 0x060003B0 RID: 944 RVA: 0x00027DF8 File Offset: 0x00025FF8
		public static int get_sacch_struct()
		{
			return (NXDNLogic.m_sacch[0] >> 6) & 3;
		}

		// Token: 0x060003B1 RID: 945 RVA: 0x00027E08 File Offset: 0x00026008
		public static void set_sacch_data(byte[] d)
		{
			int num = 8;
			int i = 0;
			while (i < 18)
			{
				bool flag = utils.bit_reader(d, i);
				NXDNLogic.m_sacch = utils.bit_writer(NXDNLogic.m_sacch, num, flag);
				i++;
				num++;
			}
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x00027E44 File Offset: 0x00026044
		public static byte[] get_sacch_data()
		{
			byte[] array = new byte[4];
			int num = 8;
			int i = 0;
			while (i < 18)
			{
				bool flag = utils.bit_reader(NXDNLogic.m_sacch, num);
				array = utils.bit_writer(array, i, flag);
				i++;
				num++;
			}
			return array;
		}

		// Token: 0x060003B3 RID: 947 RVA: 0x00027E84 File Offset: 0x00026084
		public static byte[] get_sacch()
		{
			byte[] array = new byte[4];
			Buffer.BlockCopy(NXDNLogic.m_sacch, 0, array, 0, 4);
			return NXDNLogic.encode_crc6(array, 26);
		}

		// Token: 0x060003B4 RID: 948 RVA: 0x00027EAE File Offset: 0x000260AE
		public static bool set_sacch(byte[] data)
		{
			Buffer.BlockCopy(data, 0, NXDNLogic.m_sacch, 0, 4);
			return NXDNLogic.check_crc6(data, 26);
		}

		// Token: 0x060003B5 RID: 949 RVA: 0x00027EC6 File Offset: 0x000260C6
		public static void set_layer3_msgtype(byte t)
		{
			byte[] layer = NXDNLogic.m_layer3;
			int num = 0;
			layer[num] &= 192;
			byte[] layer2 = NXDNLogic.m_layer3;
			int num2 = 0;
			layer2[num2] |= t & 63;
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x00027EF2 File Offset: 0x000260F2
		public static byte get_layer3_msgtype()
		{
			return NXDNLogic.m_layer3[0] & 63;
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x00027EFF File Offset: 0x000260FF
		public static void set_layer3_srcid(int src)
		{
			NXDNLogic.m_layer3[3] = (byte)((src >> 8) & 255);
			NXDNLogic.m_layer3[4] = (byte)(src & 255);
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x00027F21 File Offset: 0x00026121
		public static int get_layer3_srcid()
		{
			return ((int)(byte.MaxValue & NXDNLogic.m_layer3[3]) << 8) | (int)(byte.MaxValue & NXDNLogic.m_layer3[4]);
		}

		// Token: 0x060003B9 RID: 953 RVA: 0x00027F40 File Offset: 0x00026140
		public static void set_layer3_dstid(int dst)
		{
			NXDNLogic.m_layer3[5] = (byte)((dst >> 8) & 255);
			NXDNLogic.m_layer3[6] = (byte)(dst & 255);
		}

		// Token: 0x060003BA RID: 954 RVA: 0x00027F64 File Offset: 0x00026164
		public static void get_message_type(int i)
		{
			switch (i)
			{
			default:
				return;
			}
		}

		// Token: 0x060003BB RID: 955 RVA: 0x00027FBC File Offset: 0x000261BC
		public static int get_layer3_dstid()
		{
			return ((int)(byte.MaxValue & NXDNLogic.m_layer3[5]) << 8) | (int)(byte.MaxValue & NXDNLogic.m_layer3[6]);
		}

		// Token: 0x060003BC RID: 956 RVA: 0x00027FDB File Offset: 0x000261DB
		public static void set_layer3_grp(bool grp)
		{
			byte[] layer = NXDNLogic.m_layer3;
			int num = 2;
			layer[num] |= (grp ? 32 : 32);
		}

		// Token: 0x060003BD RID: 957 RVA: 0x00027FF7 File Offset: 0x000261F7
		public static bool get_layer3_grp()
		{
			return (byte.MaxValue & NXDNLogic.m_layer3[2] & 128) != 128;
		}

		// Token: 0x060003BE RID: 958 RVA: 0x00028016 File Offset: 0x00026216
		public static void set_layer3_blks(int b)
		{
			byte[] layer = NXDNLogic.m_layer3;
			int num = 8;
			layer[num] &= 240;
			byte[] layer2 = NXDNLogic.m_layer3;
			int num2 = 8;
			layer2[num2] |= (byte)(b & 15);
		}

		// Token: 0x060003BF RID: 959 RVA: 0x00028042 File Offset: 0x00026242
		public static int get_layer3_blks()
		{
			return (int)((NXDNLogic.m_layer3[8] & 15) + 1);
		}

		// Token: 0x060003C0 RID: 960 RVA: 0x00028050 File Offset: 0x00026250
		public static byte[] layer3_encode(byte[] d, int len, int offset)
		{
			int i = 0;
			while (i < len)
			{
				bool flag = utils.bit_reader(NXDNLogic.m_layer3, offset);
				d = utils.bit_writer(d, i, flag);
				i++;
				offset++;
			}
			return d;
		}

		// Token: 0x060003C1 RID: 961 RVA: 0x00028088 File Offset: 0x00026288
		public static void layer3_decode(byte[] d, int len, int offset)
		{
			int i = 0;
			while (i < len)
			{
				bool flag = utils.bit_reader(d, i);
				NXDNLogic.m_layer3 = utils.bit_writer(NXDNLogic.m_layer3, offset, flag);
				i++;
				offset++;
			}
		}

		// Token: 0x060003C2 RID: 962 RVA: 0x000280C0 File Offset: 0x000262C0
		public static byte[] encode_crc6(byte[] d, int len)
		{
			byte b = 63;
			for (int i = 0; i < len; i++)
			{
				bool flag = utils.bit_reader(d, i);
				bool flag2 = (b & 32) == 32;
				b = (byte)(b << 1);
				if (flag ^ flag2)
				{
					b ^= 39;
				}
			}
			b &= 63;
			byte[] array = new byte[2];
			array[0] = b;
			int num = len;
			int j = 2;
			while (j < 8)
			{
				bool flag3 = utils.bit_reader(array, j);
				d = utils.bit_writer(d, num, flag3);
				j++;
				num++;
			}
			return d;
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x0002813C File Offset: 0x0002633C
		public static bool check_crc6(byte[] d, int len)
		{
			byte b = 63;
			for (int i = 0; i < len; i++)
			{
				bool flag = utils.bit_reader(d, i);
				bool flag2 = (b & 32) == 32;
				b = (byte)(b << 1);
				if (flag ^ flag2)
				{
					b ^= 39;
				}
			}
			b &= 63;
			byte[] array = new byte[] { 0, 0 };
			int num = len;
			int j = 2;
			while (j < 8)
			{
				bool flag3 = utils.bit_reader(d, num);
				array = utils.bit_writer(array, j, flag3);
				j++;
				num++;
			}
			return b == array[0];
		}

		// Token: 0x060003C5 RID: 965 RVA: 0x000281C0 File Offset: 0x000263C0
		// Note: this type is marked as 'beforefieldinit'.
		static NXDNLogic()
		{
			byte[] array = new byte[3];
			array[0] = 16;
			NXDNLogic.idle = array;
			NXDNLogic.AMBEFour = new byte[28];
			NXDNLogic.m_lich = 0;
			NXDNLogic.m_layer3 = new byte[22];
			NXDNLogic.m_sacch = new byte[5];
		}

		// Token: 0x04000275 RID: 629
		public static int[] dvsi_interleave = new int[]
		{
			0, 3, 6, 9, 12, 15, 18, 21, 24, 27,
			30, 33, 36, 39, 41, 43, 45, 47, 1, 4,
			7, 10, 13, 16, 19, 22, 25, 28, 31, 34,
			37, 40, 42, 44, 46, 48, 2, 5, 8, 11,
			14, 17, 20, 23, 26, 29, 32, 35, 38
		};

		// Token: 0x04000276 RID: 630
		public static bool group = true;

		// Token: 0x04000277 RID: 631
		public static int counter = 0;

		// Token: 0x04000278 RID: 632
		public static byte[] idle;

		// Token: 0x04000279 RID: 633
		public static byte[] AMBEFour;

		// Token: 0x0400027A RID: 634
		public static byte m_lich;

		// Token: 0x0400027B RID: 635
		public static byte[] m_layer3;

		// Token: 0x0400027C RID: 636
		public static byte[] m_sacch;
	}
}
