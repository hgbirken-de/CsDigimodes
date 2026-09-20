using System;
using System.Text;

namespace BlueDV
{
	// Token: 0x0200002A RID: 42
	internal class fusion_extract
	{
		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000319 RID: 793 RVA: 0x00023888 File Offset: 0x00021A88
		// (set) Token: 0x0600031A RID: 794 RVA: 0x0002388F File Offset: 0x00021A8F
		public static string StatusRadioText { get; private set; }

		// Token: 0x14000016 RID: 22
		// (add) Token: 0x0600031B RID: 795 RVA: 0x00023898 File Offset: 0x00021A98
		// (remove) Token: 0x0600031C RID: 796 RVA: 0x000238CC File Offset: 0x00021ACC
		public static event EventHandler StatusRadioTextChanged;

		// Token: 0x0600031D RID: 797 RVA: 0x000238FF File Offset: 0x00021AFF
		public static bool bit_reader(byte[] p, int i)
		{
			return (p[i >> 3] & fusion_extract.BIT_MASK_TABLE[i & 7]) > 0;
		}

		// Token: 0x0600031E RID: 798 RVA: 0x00023918 File Offset: 0x00021B18
		internal static byte gmult(byte a, byte b)
		{
			if (a == 0 || b == 0)
			{
				return 0;
			}
			uint num = (uint)fusion_extract.LOG_TABLE[(int)a];
			uint num2 = (uint)fusion_extract.LOG_TABLE[(int)b];
			return fusion_extract.EXP_TABLE[(int)(num + num2)];
		}

		// Token: 0x0600031F RID: 799 RVA: 0x00023948 File Offset: 0x00021B48
		public static byte[] writeDataFRMode1(string data, byte[] voice)
		{
			byte[] array = new byte[23];
			byte[] bytes = Encoding.ASCII.GetBytes(data);
			for (int i = 0; i < 20; i++)
			{
				byte[] array2 = bytes;
				int num = i;
				array2[num] ^= fusion_extract.WHITENING_DATA[i];
			}
			Buffer.BlockCopy(bytes, 0, array, 0, 20);
			Buffer.BlockCopy(utils.addCCITT162(array, 22), 0, array, 0, 22);
			array[22] = 0;
			byte[] array3 = new CYFSConvolution().encode(array, 180, 45);
			int num2 = 0;
			byte[] array4 = new byte[45];
			for (int j = 0; j < 180; j++)
			{
				int num3 = fusion_extract.INTERLEAVE_TABLE_9_20[j];
				bool flag = utils.bit_reader(array3, num2);
				num2++;
				bool flag2 = utils.bit_reader(array3, num2);
				num2++;
				array4 = utils.bit_writer(array4, num3, flag);
				num3++;
				array4 = utils.bit_writer(array4, num3, flag2);
			}
			int num4 = 0;
			int num5 = 0;
			for (int k = 0; k < 5; k++)
			{
				Buffer.BlockCopy(array4, num4, voice, 30 + num5, 9);
				num4 += 9;
				num5 += 18;
			}
			return voice;
		}

		// Token: 0x06000320 RID: 800 RVA: 0x00023A60 File Offset: 0x00021C60
		public static byte[] writeDataFRMode1(byte[] output1, byte[] voice)
		{
			byte[] array = new byte[23];
			for (int i = 0; i < 20; i++)
			{
				int num = i;
				output1[num] ^= fusion_extract.WHITENING_DATA[i];
			}
			Buffer.BlockCopy(output1, 0, array, 0, 20);
			Buffer.BlockCopy(utils.addCCITT162(array, 22), 0, array, 0, 22);
			array[22] = 0;
			byte[] array2 = new CYFSConvolution().encode(array, 180, 45);
			int num2 = 0;
			byte[] array3 = new byte[45];
			for (int j = 0; j < 180; j++)
			{
				int num3 = fusion_extract.INTERLEAVE_TABLE_9_20[j];
				bool flag = utils.bit_reader(array2, num2);
				num2++;
				bool flag2 = utils.bit_reader(array2, num2);
				num2++;
				array3 = utils.bit_writer(array3, num3, flag);
				num3++;
				array3 = utils.bit_writer(array3, num3, flag2);
			}
			int num4 = 0;
			int num5 = 0;
			for (int k = 0; k < 5; k++)
			{
				Buffer.BlockCopy(array3, num4, voice, 30 + num5, 9);
				num4 += 9;
				num5 += 18;
			}
			return voice;
		}

		// Token: 0x06000321 RID: 801 RVA: 0x00023B64 File Offset: 0x00021D64
		public static byte[] writeDataFRMode2(string data, byte[] voice)
		{
			byte[] array = new byte[23];
			byte[] bytes = Encoding.ASCII.GetBytes(data);
			for (int i = 0; i < 20; i++)
			{
				byte[] array2 = bytes;
				int num = i;
				array2[num] ^= fusion_extract.WHITENING_DATA[i];
			}
			Buffer.BlockCopy(bytes, 0, array, 0, 20);
			Buffer.BlockCopy(utils.addCCITT162(array, 22), 0, array, 0, 22);
			array[22] = 0;
			byte[] array3 = new CYFSConvolution().encode(array, 180, 45);
			int num2 = 0;
			byte[] array4 = new byte[45];
			for (int j = 0; j < 180; j++)
			{
				int num3 = fusion_extract.INTERLEAVE_TABLE_9_20[j];
				bool flag = utils.bit_reader(array3, num2);
				num2++;
				bool flag2 = utils.bit_reader(array3, num2);
				num2++;
				array4 = utils.bit_writer(array4, num3, flag);
				num3++;
				array4 = utils.bit_writer(array4, num3, flag2);
			}
			int num4 = 0;
			int num5 = 9;
			for (int k = 0; k < 5; k++)
			{
				Buffer.BlockCopy(array4, num4, voice, 30 + num5, 9);
				num4 += 9;
				num5 += 18;
			}
			return voice;
		}

		// Token: 0x06000322 RID: 802 RVA: 0x00023C7C File Offset: 0x00021E7C
		public static byte[] writeDataFRMode2(byte[] output1, byte[] voice)
		{
			byte[] array = new byte[23];
			for (int i = 0; i < 20; i++)
			{
				int num = i;
				output1[num] ^= fusion_extract.WHITENING_DATA[i];
			}
			Buffer.BlockCopy(output1, 0, array, 0, 20);
			Buffer.BlockCopy(utils.addCCITT162(array, 22), 0, array, 0, 22);
			array[22] = 0;
			byte[] array2 = new CYFSConvolution().encode(array, 180, 45);
			int num2 = 0;
			byte[] array3 = new byte[45];
			for (int j = 0; j < 180; j++)
			{
				int num3 = fusion_extract.INTERLEAVE_TABLE_9_20[j];
				bool flag = utils.bit_reader(array2, num2);
				num2++;
				bool flag2 = utils.bit_reader(array2, num2);
				num2++;
				array3 = utils.bit_writer(array3, num3, flag);
				num3++;
				array3 = utils.bit_writer(array3, num3, flag2);
			}
			int num4 = 0;
			int num5 = 9;
			for (int k = 0; k < 5; k++)
			{
				Buffer.BlockCopy(array3, num4, voice, 30 + num5, 9);
				num4 += 9;
				num5 += 18;
			}
			return voice;
		}

		// Token: 0x06000323 RID: 803 RVA: 0x00023D80 File Offset: 0x00021F80
		public static bool readFR1Mode(byte[] data)
		{
			byte[] array = new byte[90];
			byte[] array2 = new byte[90];
			int num = 0;
			Buffer.BlockCopy(data, 30, array2, 0, 90);
			int num2 = 0;
			for (int i = 0; i < 5; i++)
			{
				Buffer.BlockCopy(array2, num2, array, num, 9);
				num2 += 18;
				num += 9;
			}
			CYFSConvolution cyfsconvolution = new CYFSConvolution();
			cyfsconvolution.start();
			for (int j = 0; j < 180; j++)
			{
				int num3 = fusion_extract.INTERLEAVE_TABLE_9_20[j];
				byte b = ((utils.bit_reader(array, num3) > false) ? 1 : 0);
				num3++;
				byte b2 = ((utils.bit_reader(array, num3) > false) ? 1 : 0);
				cyfsconvolution.decode(b, b2);
			}
			byte[] array3 = new byte[23];
			cyfsconvolution.chainback(array3, 176);
			bool flag = utils.checkCCITT162(array3, 22U);
			if (flag)
			{
				for (int k = 0; k < 20; k++)
				{
					byte[] array4 = array3;
					int num4 = k;
					array4[num4] ^= fusion_extract.WHITENING_DATA[k];
				}
			}
			return flag;
		}

		// Token: 0x06000324 RID: 804 RVA: 0x00023E7C File Offset: 0x0002207C
		public static bool readFR2Mode(byte[] data)
		{
			byte[] array = new byte[90];
			byte[] array2 = new byte[90];
			int num = 9;
			int num2 = 0;
			Buffer.BlockCopy(data, 30, array2, 0, 90);
			for (int i = 0; i < 5; i++)
			{
				Buffer.BlockCopy(array2, num, array, num2, 9);
				num += 18;
				num2 += 9;
			}
			CYFSConvolution cyfsconvolution = new CYFSConvolution();
			cyfsconvolution.start();
			for (int j = 0; j < 180; j++)
			{
				int num3 = fusion_extract.INTERLEAVE_TABLE_9_20[j];
				byte b = ((utils.bit_reader(array, num3) > false) ? 1 : 0);
				num3++;
				byte b2 = ((utils.bit_reader(array, num3) > false) ? 1 : 0);
				cyfsconvolution.decode(b, b2);
			}
			byte[] array3 = new byte[23];
			cyfsconvolution.chainback(array3, 176);
			bool flag = utils.checkCCITT162(array3, 22U);
			if (flag)
			{
				for (int k = 0; k < 20; k++)
				{
					byte[] array4 = array3;
					int num4 = k;
					array4[num4] ^= fusion_extract.WHITENING_DATA[k];
				}
			}
			return flag;
		}

		// Token: 0x06000325 RID: 805 RVA: 0x00023F78 File Offset: 0x00022178
		public static byte[] make_fusion_header_edit_call(string call)
		{
			string text = call.PadRight(10, ' ');
			string text2 = "**********".PadRight(10, ' ') + text;
			string text3 = information.myCall.PadRight(10, ' ');
			string text4 = information.myCall.PadRight(10, ' ') + text3;
			byte[] array = new byte[23];
			Buffer.BlockCopy(Encoding.ASCII.GetBytes(text2), 0, array, 0, 20);
			for (int i = 0; i < 20; i++)
			{
				byte[] array2 = array;
				int num = i;
				array2[num] ^= fusion_extract.WHITENING_DATA[i];
			}
			byte[] array3 = utils.addCCITT162(array, 22);
			byte[] array4 = new CYFSConvolution().encode(array3, 180, 45);
			int num2 = 0;
			byte[] array5 = new byte[45];
			for (int j = 0; j < 180; j++)
			{
				int num3 = fusion_extract.INTERLEAVE_TABLE_9_20[j];
				bool flag = fusion_extract.bit_reader(array4, num2);
				num2++;
				bool flag2 = fusion_extract.bit_reader(array4, num2);
				num2++;
				array5 = fusion_extract.bit_writer(array5, num3, flag);
				num3++;
				array5 = fusion_extract.bit_writer(array5, num3, flag2);
			}
			int num4 = 0;
			int num5 = 0;
			for (int k = 0; k < 5; k++)
			{
				Buffer.BlockCopy(array5, num4, fusion_extract.C4fM_FRAME_HEADER, 30 + num5, 9);
				Buffer.BlockCopy(array5, num4, fusion_extract.C4fM_FRAME_END, 30 + num5, 9);
				num4 += 9;
				num5 += 18;
			}
			array = new byte[23];
			Buffer.BlockCopy(Encoding.ASCII.GetBytes(text4), 0, array, 0, 20);
			for (int l = 0; l < 20; l++)
			{
				byte[] array6 = array;
				int num6 = l;
				array6[num6] ^= fusion_extract.WHITENING_DATA[l];
			}
			array3 = utils.addCCITT162(array, 22);
			array4 = new CYFSConvolution().encode(array3, 180, 45);
			num2 = 0;
			array5 = new byte[45];
			for (int m = 0; m < 180; m++)
			{
				int num7 = fusion_extract.INTERLEAVE_TABLE_9_20[m];
				bool flag3 = fusion_extract.bit_reader(array4, num2);
				num2++;
				bool flag4 = fusion_extract.bit_reader(array4, num2);
				num2++;
				array5 = fusion_extract.bit_writer(array5, num7, flag3);
				num7++;
				array5 = fusion_extract.bit_writer(array5, num7, flag4);
			}
			num4 = 0;
			num5 = 9;
			for (int n = 0; n < 5; n++)
			{
				Buffer.BlockCopy(array5, num4, fusion_extract.C4fM_FRAME_HEADER, 30 + num5, 9);
				Buffer.BlockCopy(array5, num4, fusion_extract.C4fM_FRAME_END, 30 + num5, 9);
				num4 += 9;
				num5 += 18;
			}
			return fusion_extract.C4fM_FRAME_HEADER;
		}

		// Token: 0x06000326 RID: 806 RVA: 0x00024210 File Offset: 0x00022410
		public static void make_fusion_header()
		{
			string text = information.myCall.PadRight(10, ' ');
			string text2 = "**********".PadRight(10, ' ') + text;
			string text3 = information.myCall.PadRight(10, ' ');
			string text4 = information.myCall.PadRight(10, ' ') + text3;
			byte[] array = new byte[23];
			Buffer.BlockCopy(Encoding.ASCII.GetBytes(text2), 0, array, 0, 20);
			for (int i = 0; i < 20; i++)
			{
				byte[] array2 = array;
				int num = i;
				array2[num] ^= fusion_extract.WHITENING_DATA[i];
			}
			byte[] array3 = utils.addCCITT162(array, 22);
			byte[] array4 = new CYFSConvolution().encode(array3, 180, 45);
			int num2 = 0;
			byte[] array5 = new byte[45];
			for (int j = 0; j < 180; j++)
			{
				int num3 = fusion_extract.INTERLEAVE_TABLE_9_20[j];
				bool flag = fusion_extract.bit_reader(array4, num2);
				num2++;
				bool flag2 = fusion_extract.bit_reader(array4, num2);
				num2++;
				array5 = fusion_extract.bit_writer(array5, num3, flag);
				num3++;
				array5 = fusion_extract.bit_writer(array5, num3, flag2);
			}
			int num4 = 0;
			int num5 = 0;
			for (int k = 0; k < 5; k++)
			{
				Buffer.BlockCopy(array5, num4, fusion_extract.C4fM_FRAME_HEADER, 30 + num5, 9);
				Buffer.BlockCopy(array5, num4, fusion_extract.C4fM_FRAME_END, 30 + num5, 9);
				num4 += 9;
				num5 += 18;
			}
			array = new byte[23];
			Buffer.BlockCopy(Encoding.ASCII.GetBytes(text4), 0, array, 0, 20);
			for (int l = 0; l < 20; l++)
			{
				byte[] array6 = array;
				int num6 = l;
				array6[num6] ^= fusion_extract.WHITENING_DATA[l];
			}
			array3 = utils.addCCITT162(array, 22);
			array4 = new CYFSConvolution().encode(array3, 180, 45);
			num2 = 0;
			array5 = new byte[45];
			for (int m = 0; m < 180; m++)
			{
				int num7 = fusion_extract.INTERLEAVE_TABLE_9_20[m];
				bool flag3 = fusion_extract.bit_reader(array4, num2);
				num2++;
				bool flag4 = fusion_extract.bit_reader(array4, num2);
				num2++;
				array5 = fusion_extract.bit_writer(array5, num7, flag3);
				num7++;
				array5 = fusion_extract.bit_writer(array5, num7, flag4);
			}
			num4 = 0;
			num5 = 9;
			for (int n = 0; n < 5; n++)
			{
				Buffer.BlockCopy(array5, num4, fusion_extract.C4fM_FRAME_HEADER, 30 + num5, 9);
				Buffer.BlockCopy(array5, num4, fusion_extract.C4fM_FRAME_END, 30 + num5, 9);
				num4 += 9;
				num5 += 18;
			}
		}

		// Token: 0x06000327 RID: 807 RVA: 0x000244A8 File Offset: 0x000226A8
		public static bool get_fusion_headerFirstBlock(byte[] allData)
		{
			byte[] array = new byte[90];
			byte[] array2 = new byte[45];
			Buffer.BlockCopy(allData, 30, array, 0, 90);
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < 5; i++)
			{
				Buffer.BlockCopy(array, num, array2, num2, 9);
				num += 18;
				num2 += 9;
			}
			CYFSConvolution cyfsconvolution = new CYFSConvolution();
			cyfsconvolution.start();
			for (int j = 0; j < 180; j++)
			{
				int num3 = fusion_extract.INTERLEAVE_TABLE_9_20[j];
				byte b = ((fusion_extract.bit_reader(array2, num3) > false) ? 1 : 0);
				num3++;
				byte b2 = ((fusion_extract.bit_reader(array2, num3) > false) ? 1 : 0);
				cyfsconvolution.decode(b, b2);
			}
			byte[] array3 = new byte[23];
			cyfsconvolution.chainback(array3, 176);
			bool flag = utils.checkCCITT162(array3, 22U);
			if (flag)
			{
				for (int k = 0; k < 20; k++)
				{
					byte[] array4 = array3;
					int num4 = k;
					array4[num4] ^= fusion_extract.WHITENING_DATA[k];
				}
				byte[] array5 = new byte[10];
				Buffer.BlockCopy(array3, 0, array5, 0, 10);
				Encoding.UTF8.GetString(array3);
				Buffer.BlockCopy(array3, 10, array5, 0, 10);
			}
			return flag;
		}

		// Token: 0x06000328 RID: 808 RVA: 0x000245D4 File Offset: 0x000227D4
		public static bool get_fusion_headerSecondBlock(byte[] allData)
		{
			byte[] array = new byte[90];
			byte[] array2 = new byte[45];
			Buffer.BlockCopy(allData, 30, array, 0, 90);
			int num = 9;
			int num2 = 0;
			for (int i = 0; i < 5; i++)
			{
				Buffer.BlockCopy(array, num, array2, num2, 9);
				num += 18;
				num2 += 9;
			}
			CYFSConvolution cyfsconvolution = new CYFSConvolution();
			cyfsconvolution.start();
			for (int j = 0; j < 180; j++)
			{
				int num3 = fusion_extract.INTERLEAVE_TABLE_9_20[j];
				byte b = ((fusion_extract.bit_reader(array2, num3) > false) ? 1 : 0);
				num3++;
				byte b2 = ((fusion_extract.bit_reader(array2, num3) > false) ? 1 : 0);
				cyfsconvolution.decode(b, b2);
			}
			byte[] array3 = new byte[23];
			cyfsconvolution.chainback(array3, 176);
			bool flag = utils.checkCCITT162(array3, 22U);
			if (flag)
			{
				for (int k = 0; k < 20; k++)
				{
					byte[] array4 = array3;
					int num4 = k;
					array4[num4] ^= fusion_extract.WHITENING_DATA[k];
				}
				byte[] array5 = new byte[10];
				Buffer.BlockCopy(array3, 0, array5, 0, 10);
				Encoding.UTF8.GetString(array3);
				Buffer.BlockCopy(array3, 10, array5, 0, 10);
			}
			return flag;
		}

		// Token: 0x06000329 RID: 809 RVA: 0x00024700 File Offset: 0x00022900
		public static byte[] fill_DCH_VD2(byte[] voice, string data)
		{
			byte[] tenBytesFromFn = fusion_extract.getTenBytesFromFn(data);
			Buffer.BlockCopy(tenBytesFromFn, 0, voice, 30, 5);
			Buffer.BlockCopy(tenBytesFromFn, 5, voice, 48, 5);
			Buffer.BlockCopy(tenBytesFromFn, 10, voice, 66, 5);
			Buffer.BlockCopy(tenBytesFromFn, 15, voice, 84, 5);
			Buffer.BlockCopy(tenBytesFromFn, 20, voice, 102, 5);
			return voice;
		}

		// Token: 0x0600032A RID: 810 RVA: 0x00024750 File Offset: 0x00022950
		public static void fill_DCH_VD2(int fnMode)
		{
			new byte[25];
			new byte[45];
			switch (fnMode)
			{
			case 0:
			{
				byte[] tenBytesFromFn = fusion_extract.getTenBytesFromFn(information.myCall);
				Buffer.BlockCopy(tenBytesFromFn, 0, fusion_extract.C4fM_FRAME_FN0, 30, 5);
				Buffer.BlockCopy(tenBytesFromFn, 5, fusion_extract.C4fM_FRAME_FN0, 48, 5);
				Buffer.BlockCopy(tenBytesFromFn, 10, fusion_extract.C4fM_FRAME_FN0, 66, 5);
				Buffer.BlockCopy(tenBytesFromFn, 15, fusion_extract.C4fM_FRAME_FN0, 84, 5);
				Buffer.BlockCopy(tenBytesFromFn, 20, fusion_extract.C4fM_FRAME_FN0, 102, 5);
				return;
			}
			case 1:
			{
				byte[] tenBytesFromFn2 = fusion_extract.getTenBytesFromFn("*****F5ZFW");
				Buffer.BlockCopy(tenBytesFromFn2, 0, fusion_extract.C4fM_FRAME_FN1, 30, 5);
				Buffer.BlockCopy(tenBytesFromFn2, 5, fusion_extract.C4fM_FRAME_FN1, 48, 5);
				Buffer.BlockCopy(tenBytesFromFn2, 10, fusion_extract.C4fM_FRAME_FN1, 66, 5);
				Buffer.BlockCopy(tenBytesFromFn2, 15, fusion_extract.C4fM_FRAME_FN1, 84, 5);
				Buffer.BlockCopy(tenBytesFromFn2, 20, fusion_extract.C4fM_FRAME_FN1, 102, 5);
				return;
			}
			case 2:
			{
				byte[] tenBytesFromFn3 = fusion_extract.getTenBytesFromFn("BLUEDV    ");
				Buffer.BlockCopy(tenBytesFromFn3, 0, fusion_extract.C4fM_FRAME_FN2, 30, 5);
				Buffer.BlockCopy(tenBytesFromFn3, 5, fusion_extract.C4fM_FRAME_FN2, 48, 5);
				Buffer.BlockCopy(tenBytesFromFn3, 10, fusion_extract.C4fM_FRAME_FN2, 66, 5);
				Buffer.BlockCopy(tenBytesFromFn3, 15, fusion_extract.C4fM_FRAME_FN2, 84, 5);
				Buffer.BlockCopy(tenBytesFromFn3, 20, fusion_extract.C4fM_FRAME_FN2, 102, 5);
				return;
			}
			case 3:
			{
				byte[] tenBytesFromFn4 = fusion_extract.getTenBytesFromFn("BLUEDV    ");
				Buffer.BlockCopy(tenBytesFromFn4, 0, fusion_extract.C4fM_FRAME_FN3, 30, 5);
				Buffer.BlockCopy(tenBytesFromFn4, 5, fusion_extract.C4fM_FRAME_FN3, 48, 5);
				Buffer.BlockCopy(tenBytesFromFn4, 10, fusion_extract.C4fM_FRAME_FN3, 66, 5);
				Buffer.BlockCopy(tenBytesFromFn4, 15, fusion_extract.C4fM_FRAME_FN3, 84, 5);
				Buffer.BlockCopy(tenBytesFromFn4, 20, fusion_extract.C4fM_FRAME_FN3, 102, 5);
				return;
			}
			case 4:
			{
				byte[] tenBytesFromFn5 = fusion_extract.getTenBytesFromFn("     F5ZFW");
				Buffer.BlockCopy(tenBytesFromFn5, 0, fusion_extract.C4fM_FRAME_FN4, 30, 5);
				Buffer.BlockCopy(tenBytesFromFn5, 5, fusion_extract.C4fM_FRAME_FN4, 48, 5);
				Buffer.BlockCopy(tenBytesFromFn5, 10, fusion_extract.C4fM_FRAME_FN4, 66, 5);
				Buffer.BlockCopy(tenBytesFromFn5, 15, fusion_extract.C4fM_FRAME_FN4, 84, 5);
				Buffer.BlockCopy(tenBytesFromFn5, 20, fusion_extract.C4fM_FRAME_FN4, 102, 5);
				return;
			}
			case 5:
			{
				byte[] tenBytesFromFn6 = fusion_extract.getTenBytesFromFn("          ");
				Buffer.BlockCopy(tenBytesFromFn6, 0, fusion_extract.C4fM_FRAME_FN5, 30, 5);
				Buffer.BlockCopy(tenBytesFromFn6, 5, fusion_extract.C4fM_FRAME_FN5, 48, 5);
				Buffer.BlockCopy(tenBytesFromFn6, 10, fusion_extract.C4fM_FRAME_FN5, 66, 5);
				Buffer.BlockCopy(tenBytesFromFn6, 15, fusion_extract.C4fM_FRAME_FN5, 84, 5);
				Buffer.BlockCopy(tenBytesFromFn6, 20, fusion_extract.C4fM_FRAME_FN5, 102, 5);
				return;
			}
			case 6:
			{
				byte[] tenBytesFromFnBytes = fusion_extract.getTenBytesFromFnBytes(new byte[] { 22, 34, 97, 95, 16, 3, 11, 0, 0, 0 });
				Buffer.BlockCopy(tenBytesFromFnBytes, 0, fusion_extract.C4fM_FRAME_FN6, 30, 5);
				Buffer.BlockCopy(tenBytesFromFnBytes, 5, fusion_extract.C4fM_FRAME_FN6, 48, 5);
				Buffer.BlockCopy(tenBytesFromFnBytes, 10, fusion_extract.C4fM_FRAME_FN6, 66, 5);
				Buffer.BlockCopy(tenBytesFromFnBytes, 15, fusion_extract.C4fM_FRAME_FN6, 84, 5);
				Buffer.BlockCopy(tenBytesFromFnBytes, 20, fusion_extract.C4fM_FRAME_FN6, 102, 5);
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x0600032B RID: 811 RVA: 0x00024A04 File Offset: 0x00022C04
		public static byte[] getTenBytesFromFn(string line)
		{
			string text = line.PadRight(10);
			byte[] array = new byte[13];
			Buffer.BlockCopy(Encoding.ASCII.GetBytes(text), 0, array, 0, 10);
			for (int i = 0; i < 10; i++)
			{
				byte[] array2 = array;
				int num = i;
				array2[num] ^= fusion_extract.WHITENING_DATA[i];
			}
			Buffer.BlockCopy(utils.addCCITT162(array, 12), 0, array, 0, 12);
			array[12] = 0;
			byte[] array3 = new CYFSConvolution().encode(array, 100, 25);
			int num2 = 0;
			byte[] array4 = new byte[25];
			for (int j = 0; j < 100; j++)
			{
				int num3 = fusion_extract.INTERLEAVE_TABLE_5_20[j];
				bool flag = fusion_extract.bit_reader(array3, num2);
				num2++;
				bool flag2 = fusion_extract.bit_reader(array3, num2);
				num2++;
				array4 = fusion_extract.bit_writer(array4, num3, flag);
				num3++;
				array4 = fusion_extract.bit_writer(array4, num3, flag2);
			}
			return array4;
		}

		// Token: 0x0600032C RID: 812 RVA: 0x00024AE8 File Offset: 0x00022CE8
		public static byte[] getTenBytesFromFnBytes(byte[] line)
		{
			byte[] array = new byte[13];
			Buffer.BlockCopy(line, 0, array, 0, 10);
			for (int i = 0; i < 10; i++)
			{
				byte[] array2 = array;
				int num = i;
				array2[num] ^= fusion_extract.WHITENING_DATA[i];
			}
			Buffer.BlockCopy(utils.addCCITT162(array, 12), 0, array, 0, 12);
			array[12] = 0;
			byte[] array3 = new CYFSConvolution().encode(array, 100, 25);
			int num2 = 0;
			byte[] array4 = new byte[25];
			for (int j = 0; j < 100; j++)
			{
				int num3 = fusion_extract.INTERLEAVE_TABLE_5_20[j];
				bool flag = fusion_extract.bit_reader(array3, num2);
				num2++;
				bool flag2 = fusion_extract.bit_reader(array3, num2);
				num2++;
				array4 = fusion_extract.bit_writer(array4, num3, flag);
				num3++;
				array4 = fusion_extract.bit_writer(array4, num3, flag2);
			}
			return array4;
		}

		// Token: 0x0600032D RID: 813 RVA: 0x00024BB4 File Offset: 0x00022DB4
		public static void get_Fusion_VD2_data(byte[] allData, int fnMode)
		{
			byte[] array = new byte[25];
			Buffer.BlockCopy(allData, 30, array, 0, 5);
			Buffer.BlockCopy(allData, 48, array, 5, 5);
			Buffer.BlockCopy(allData, 66, array, 10, 5);
			Buffer.BlockCopy(allData, 84, array, 15, 5);
			Buffer.BlockCopy(allData, 102, array, 20, 5);
			CYFSConvolution cyfsconvolution = new CYFSConvolution();
			cyfsconvolution.start();
			int num = 0;
			while ((long)num < 100L)
			{
				int num2 = fusion_extract.INTERLEAVE_TABLE_5_20[num];
				byte b = ((fusion_extract.bit_reader(array, num2) > false) ? 1 : 0);
				num2++;
				byte b2 = ((fusion_extract.bit_reader(array, num2) > false) ? 1 : 0);
				cyfsconvolution.decode(b, b2);
				num++;
			}
			byte[] array2 = new byte[13];
			cyfsconvolution.chainback(array2, 96);
			if (utils.checkCCITT162(array2, 12U))
			{
				for (int i = 0; i < 10; i++)
				{
					byte[] array3 = array2;
					int num3 = i;
					array3[num3] ^= fusion_extract.WHITENING_DATA[i];
				}
				byte[] array4 = new byte[10];
				Buffer.BlockCopy(array2, 0, array4, 0, 10);
				if ((fusion_extract.getFN() == 6 || fusion_extract.getFN() == 7) && (array4[5] == 85 || array4[5] == 84 || array4[5] == 83 || array4[5] == 3))
				{
					fusion_extract.setradioID(array4[4]);
				}
				switch (fnMode)
				{
				}
			}
		}

		// Token: 0x0600032E RID: 814 RVA: 0x00024D0C File Offset: 0x00022F0C
		private static void setradioID(byte data)
		{
			if (data != 9)
			{
				switch (data)
				{
				case 16:
					fusion_extract.radio = "BlueDV";
					goto IL_01D8;
				case 17:
					fusion_extract.radio = "Peanut";
					goto IL_01D8;
				case 21:
					fusion_extract.radio = "DMR2YSF";
					goto IL_01D8;
				case 22:
					fusion_extract.radio = "IPSC2";
					goto IL_01D8;
				case 27:
					fusion_extract.radio = "FTM-100D";
					goto IL_01D8;
				case 32:
					fusion_extract.radio = "DR-2X";
					goto IL_01D8;
				case 36:
					fusion_extract.radio = "FT-1D";
					goto IL_01D8;
				case 37:
					fusion_extract.radio = "FTM-400D";
					goto IL_01D8;
				case 38:
					fusion_extract.radio = "DR-1X";
					goto IL_01D8;
				case 39:
					fusion_extract.radio = "FT-991";
					goto IL_01D8;
				case 40:
					fusion_extract.radio = "FT-2D";
					goto IL_01D8;
				case 41:
					fusion_extract.radio = "FTM-100D";
					goto IL_01D8;
				case 43:
					fusion_extract.radio = "FT-70D";
					goto IL_01D8;
				case 44:
					fusion_extract.radio = "FTM-3207D";
					goto IL_01D8;
				case 46:
					fusion_extract.radio = "FTM-7250D";
					goto IL_01D8;
				case 48:
					fusion_extract.radio = "FT-3D";
					goto IL_01D8;
				case 49:
					fusion_extract.radio = "FTM-300D";
					goto IL_01D8;
				case 50:
					fusion_extract.radio = "FTM-200D";
					goto IL_01D8;
				case 51:
					fusion_extract.radio = "FT-5D";
					goto IL_01D8;
				case 52:
					fusion_extract.radio = "FTM-500D";
					goto IL_01D8;
				}
				fusion_extract.radio = data.ToString("X2");
			}
			else
			{
				fusion_extract.radio = "DVMEGA-Cast";
			}
			IL_01D8:
			fusion_extract.ChangeRadioText(fusion_extract.radio);
		}

		// Token: 0x0600032F RID: 815 RVA: 0x00024EFC File Offset: 0x000230FC
		public static void fakeVWVoice()
		{
			information.AMBETYPE ambetype = information.m_ambetype;
			if (ambetype == information.AMBETYPE.AMBE3000)
			{
				DVMEGAAMBE.makeC4FMVoiceMMDVM3000(fusion_extract.YSF_SILENCE);
				return;
			}
			if (ambetype != information.AMBETYPE.AMBE3003)
			{
				return;
			}
			DVMEGAAMBE.makeC4FMVoiceMMDVM3003(fusion_extract.YSF_SILENCE);
		}

		// Token: 0x06000330 RID: 816 RVA: 0x00024F2D File Offset: 0x0002312D
		public static string getRadioID()
		{
			return fusion_extract.radio;
		}

		// Token: 0x06000331 RID: 817 RVA: 0x00024F34 File Offset: 0x00023134
		public static void makeVD2VoiceBlocks(byte[] allData)
		{
			byte[] array = new byte[13];
			Buffer.BlockCopy(allData, 35, array, 0, 13);
			fusion_extract.makeAMBE(array);
			byte[] array2 = new byte[13];
			Buffer.BlockCopy(allData, 53, array2, 0, 13);
			fusion_extract.makeAMBE(array2);
			byte[] array3 = new byte[13];
			Buffer.BlockCopy(allData, 71, array3, 0, 13);
			fusion_extract.makeAMBE(array3);
			byte[] array4 = new byte[13];
			Buffer.BlockCopy(allData, 89, array4, 0, 13);
			fusion_extract.makeAMBE(array4);
			byte[] array5 = new byte[13];
			Buffer.BlockCopy(allData, 107, array5, 0, 13);
			fusion_extract.makeAMBE(array5);
		}

		// Token: 0x06000332 RID: 818 RVA: 0x00024FC8 File Offset: 0x000231C8
		public static void makeAMBE(byte[] data)
		{
			bool[] array = utils.bytesToBits(data);
			bool[] array2 = new bool[104];
			for (int i = 0; i < 104; i++)
			{
				int num = fusion_extract.INTERLEAVE_TABLE_26_4[i];
				array2[i] = array[num];
			}
			byte[] array3 = new byte[13];
			array3 = utils.ToByteArray(array2);
			for (int j = 0; j < 13; j++)
			{
				byte[] array4 = array3;
				int num2 = j;
				array4[num2] ^= fusion_extract.WHITENING_DATA[j];
			}
			int num3 = 0;
			bool[] array5 = new bool[104];
			array5 = utils.bytesToBits(array3);
			bool[] array6 = new bool[49];
			int num4 = 0;
			for (int k = 0; k < 27; k++)
			{
				int num5 = 0;
				if (array5[num4])
				{
					num5++;
				}
				if (array5[num4 + 1])
				{
					num5++;
				}
				if (array5[num4 + 2])
				{
					num5++;
				}
				if (num5 == 1)
				{
					array5[num4] = false;
					array5[num4 + 1] = false;
					array5[num4 + 2] = false;
					num3++;
				}
				if (num5 == 2)
				{
					array5[num4] = true;
					array5[num4 + 1] = true;
					array5[num4 + 2] = true;
					num3++;
				}
				array6[k] = array5[num4];
				num4 += 3;
			}
			bool[] array7 = new bool[22];
			Buffer.BlockCopy(array5, 81, array7, 0, 22);
			bool[] array8 = new bool[49];
			Buffer.BlockCopy(array6, 0, array8, 0, 27);
			Buffer.BlockCopy(array7, 0, array8, 27, 22);
			bool[] array9 = new bool[56];
			for (int l = 0; l < 49; l++)
			{
				int num6 = fusion_extract.INTER_TABBIE_VCH49[l];
				array9[l] = array8[num6];
			}
			byte[] array10 = new byte[13];
			array10 = utils.ToByteArray(array9);
			information.AMBETYPE ambetype = information.m_ambetype;
			if (ambetype == information.AMBETYPE.AMBE3000)
			{
				DVMEGAAMBE.makeC4FMVoiceMMDVM3000(array10);
				return;
			}
			if (ambetype != information.AMBETYPE.AMBE3003)
			{
				return;
			}
			DVMEGAAMBE.makeC4FMVoiceMMDVM3003(array10);
		}

		// Token: 0x06000333 RID: 819 RVA: 0x0002518C File Offset: 0x0002338C
		public static void ambe_to_vch_vech(byte[] ambe)
		{
			bool[] array = utils.bytesToBits(ambe);
			for (int i = 0; i < 56; i++)
			{
				bool flag = array[i];
			}
			bool[] array2 = new bool[49];
			for (int j = 0; j < 49; j++)
			{
				int num = fusion_extract.INTER_TABBIE_VCH49[j];
				array2[num] = array[j];
			}
			for (int k = 0; k < 49; k++)
			{
				bool flag2 = array2[k];
			}
			bool[] array3 = new bool[104];
			int num2 = 0;
			for (int l = 0; l < 27; l++)
			{
				array3[num2] = array2[l];
				num2++;
				array3[num2] = array2[l];
				num2++;
				array3[num2] = array2[l];
				num2++;
			}
			Buffer.BlockCopy(array2, 27, array3, 81, 22);
			for (int m = 0; m < 104; m++)
			{
				bool flag3 = array3[m];
			}
			byte[] array4 = new byte[13];
			array4 = utils.ToByteArray(array3);
			for (int n = 0; n < 13; n++)
			{
				byte[] array5 = array4;
				int num3 = n;
				array5[num3] ^= fusion_extract.WHITENING_DATA[n];
			}
			bool[] array6 = new bool[104];
			array6 = utils.bytesToBits(array4);
			bool[] array7 = new bool[104];
			for (int num4 = 0; num4 < 104; num4++)
			{
				int num5 = fusion_extract.INTERLEAVE_TABLE_26_4[num4];
				array7[num5] = array6[num4];
			}
			fusion_extract.make_full_frame_insert_voice(utils.bitsToBytes(array7), false);
		}

		// Token: 0x06000334 RID: 820 RVA: 0x000252DC File Offset: 0x000234DC
		public static void make_full_frame_insert_voice(byte[] voice, bool start)
		{
			if (start)
			{
				fusion_extract.fn = 0;
			}
			switch (fusion_extract.fn)
			{
			case 0:
				switch (fusion_extract.voiceCounter)
				{
				case 0:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN1, 35, 13);
					fusion_extract.voiceCounter++;
					return;
				case 1:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN1, 53, 13);
					fusion_extract.voiceCounter++;
					return;
				case 2:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN1, 71, 13);
					fusion_extract.voiceCounter++;
					return;
				case 3:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN1, 89, 13);
					fusion_extract.voiceCounter++;
					return;
				case 4:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN1, 107, 13);
					fusion_extract.fn = 1;
					fusion_extract.voiceCounter = 0;
					DVMEGAAMBE.makeC4FMVoiceFrameFromAMBE(fusion_extract.C4fM_FRAME_FN1, false);
					return;
				default:
					return;
				}
				break;
			case 1:
				switch (fusion_extract.voiceCounter)
				{
				case 0:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN0, 35, 13);
					fusion_extract.voiceCounter++;
					return;
				case 1:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN0, 53, 13);
					fusion_extract.voiceCounter++;
					return;
				case 2:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN0, 71, 13);
					fusion_extract.voiceCounter++;
					return;
				case 3:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN0, 89, 13);
					fusion_extract.voiceCounter++;
					return;
				case 4:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN0, 107, 13);
					fusion_extract.fn = 2;
					fusion_extract.voiceCounter = 0;
					DVMEGAAMBE.makeC4FMVoiceFrameFromAMBE(fusion_extract.C4fM_FRAME_FN0, false);
					return;
				default:
					return;
				}
				break;
			case 2:
				switch (fusion_extract.voiceCounter)
				{
				case 0:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN2, 35, 13);
					fusion_extract.voiceCounter++;
					return;
				case 1:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN2, 53, 13);
					fusion_extract.voiceCounter++;
					return;
				case 2:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN2, 71, 13);
					fusion_extract.voiceCounter++;
					return;
				case 3:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN2, 89, 13);
					fusion_extract.voiceCounter++;
					return;
				case 4:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN2, 107, 13);
					fusion_extract.fn = 3;
					fusion_extract.voiceCounter = 0;
					DVMEGAAMBE.makeC4FMVoiceFrameFromAMBE(fusion_extract.C4fM_FRAME_FN2, false);
					return;
				default:
					return;
				}
				break;
			case 3:
				switch (fusion_extract.voiceCounter)
				{
				case 0:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN3, 35, 13);
					fusion_extract.voiceCounter++;
					return;
				case 1:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN3, 53, 13);
					fusion_extract.voiceCounter++;
					return;
				case 2:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN3, 71, 13);
					fusion_extract.voiceCounter++;
					return;
				case 3:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN3, 89, 13);
					fusion_extract.voiceCounter++;
					return;
				case 4:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN3, 107, 13);
					fusion_extract.fn = 4;
					fusion_extract.voiceCounter = 0;
					DVMEGAAMBE.makeC4FMVoiceFrameFromAMBE(fusion_extract.C4fM_FRAME_FN3, false);
					return;
				default:
					return;
				}
				break;
			case 4:
				switch (fusion_extract.voiceCounter)
				{
				case 0:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN5, 35, 13);
					fusion_extract.voiceCounter++;
					return;
				case 1:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN5, 53, 13);
					fusion_extract.voiceCounter++;
					return;
				case 2:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN5, 71, 13);
					fusion_extract.voiceCounter++;
					return;
				case 3:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN5, 89, 13);
					fusion_extract.voiceCounter++;
					return;
				case 4:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN5, 107, 13);
					fusion_extract.fn = 5;
					fusion_extract.voiceCounter = 0;
					DVMEGAAMBE.makeC4FMVoiceFrameFromAMBE(fusion_extract.C4fM_FRAME_FN5, false);
					return;
				default:
					return;
				}
				break;
			case 5:
				switch (fusion_extract.voiceCounter)
				{
				case 0:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN4, 35, 13);
					fusion_extract.voiceCounter++;
					return;
				case 1:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN4, 53, 13);
					fusion_extract.voiceCounter++;
					return;
				case 2:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN4, 71, 13);
					fusion_extract.voiceCounter++;
					return;
				case 3:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN4, 89, 13);
					fusion_extract.voiceCounter++;
					return;
				case 4:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN4, 107, 13);
					fusion_extract.fn = 6;
					fusion_extract.voiceCounter = 0;
					DVMEGAAMBE.makeC4FMVoiceFrameFromAMBE(fusion_extract.C4fM_FRAME_FN4, false);
					return;
				default:
					return;
				}
				break;
			case 6:
				switch (fusion_extract.voiceCounter)
				{
				case 0:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN6, 35, 13);
					fusion_extract.voiceCounter++;
					return;
				case 1:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN6, 53, 13);
					fusion_extract.voiceCounter++;
					return;
				case 2:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN6, 71, 13);
					fusion_extract.voiceCounter++;
					return;
				case 3:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN6, 89, 13);
					fusion_extract.voiceCounter++;
					return;
				case 4:
					Buffer.BlockCopy(voice, 0, fusion_extract.C4fM_FRAME_FN6, 107, 13);
					fusion_extract.fn = 0;
					fusion_extract.voiceCounter = 0;
					DVMEGAAMBE.makeC4FMVoiceFrameFromAMBE(fusion_extract.C4fM_FRAME_FN6, false);
					return;
				default:
					return;
				}
				break;
			default:
				return;
			}
		}

		// Token: 0x06000335 RID: 821 RVA: 0x00025848 File Offset: 0x00023A48
		public static void processFusion(byte[] data)
		{
			if (fusion_extract.decode_FICH(data))
			{
				switch (fusion_extract.getFI())
				{
				case 0:
				{
					bool flag = fusion_extract.get_fusion_headerFirstBlock(data);
					bool flag2 = fusion_extract.get_fusion_headerSecondBlock(data);
					return;
				}
				case 1:
					fusion_extract.process_vd2(data);
					break;
				case 2:
				case 3:
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x06000336 RID: 822 RVA: 0x00025890 File Offset: 0x00023A90
		private static void process_vd2(byte[] data)
		{
			switch (fusion_extract.getDT())
			{
			case 0:
			case 3:
				break;
			case 1:
				fusion_extract.makeVD2VoiceBlocks(data);
				fusion_extract.get_Fusion_VD2_data(data, fusion_extract.getFN());
				return;
			case 2:
				fusion_extract.makeVD2VoiceBlocks(data);
				fusion_extract.get_Fusion_VD2_data(data, fusion_extract.getFN());
				break;
			default:
				return;
			}
		}

		// Token: 0x06000337 RID: 823 RVA: 0x000258E0 File Offset: 0x00023AE0
		private static void ChangeRadioText(string text)
		{
			fusion_extract.StatusRadioText = text;
			EventHandler statusRadioTextChanged = fusion_extract.StatusRadioTextChanged;
			if (statusRadioTextChanged != null)
			{
				statusRadioTextChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000338 RID: 824 RVA: 0x00025908 File Offset: 0x00023B08
		public static bool decode_FICH(byte[] data)
		{
			byte[] array = new byte[25];
			Buffer.BlockCopy(data, 5, array, 0, 25);
			CYFSConvolution cyfsconvolution = new CYFSConvolution();
			cyfsconvolution.start();
			for (int i = 0; i < 100; i++)
			{
				int num = fusion_extract.INTERLEAVE_TABLE2[i];
				byte b = ((fusion_extract.bit_reader(array, num) > false) ? 1 : 0);
				num++;
				byte b2 = ((fusion_extract.bit_reader(array, num) > false) ? 1 : 0);
				cyfsconvolution.decode(b, b2);
			}
			byte[] array2 = new byte[13];
			cyfsconvolution.chainback(array2, 96);
			byte[] array3 = new byte[13];
			byte[] array4 = new byte[10];
			byte[] array5 = new byte[7];
			byte[] array6 = new byte[4];
			Buffer.BlockCopy(array2, 0, array3, 0, 13);
			Buffer.BlockCopy(array2, 3, array4, 0, 10);
			Buffer.BlockCopy(array2, 6, array5, 0, 7);
			Buffer.BlockCopy(array2, 9, array6, 0, 4);
			uint num2 = Golay24128.decode24128(array3);
			uint num3 = Golay24128.decode24128(array4);
			uint num4 = Golay24128.decode24128(array5);
			uint num5 = Golay24128.decode24128(array6);
			fusion_extract.m_fich[0] = (byte)((num2 >> 4) & 255U);
			fusion_extract.m_fich[1] = (byte)(((num2 << 4) & 240U) | ((num3 >> 8) & 15U));
			fusion_extract.m_fich[2] = (byte)(num3 & 255U);
			fusion_extract.m_fich[3] = (byte)((num4 >> 4) & 255U);
			fusion_extract.m_fich[4] = (byte)(((num4 << 4) & 240U) | ((num5 >> 8) & 15U));
			fusion_extract.m_fich[5] = (byte)(num5 & 255U);
			return utils.checkCCITT162(fusion_extract.m_fich, 6U);
		}

		// Token: 0x06000339 RID: 825 RVA: 0x00025A84 File Offset: 0x00023C84
		public static byte[] encode_FICH()
		{
			byte[] array = utils.addCCITT162(fusion_extract.m_fich, 6);
			utils.checkCCITT162(array, 6U);
			int num = (((int)array[0] << 4) & 4080) | ((array[1] >> 4) & 15);
			int num2 = (((int)array[1] << 8) & 3840) | (int)(array[2] & byte.MaxValue);
			int num3 = (((int)array[3] << 4) & 4080) | ((array[4] >> 4) & 15);
			int num4 = (((int)array[4] << 8) & 3840) | (int)(array[5] & byte.MaxValue);
			int num5 = Golay24128.encode24128(num & 65535);
			int num6 = Golay24128.encode24128(num2 & 65535);
			int num7 = Golay24128.encode24128(num3 & 65535);
			int num8 = Golay24128.encode24128(num4 & 65535);
			byte[] array2 = new byte[]
			{
				(byte)((num5 >> 16) & 255),
				(byte)((num5 >> 8) & 255),
				(byte)(num5 & 255),
				(byte)((num6 >> 16) & 255),
				(byte)((num6 >> 8) & 255),
				(byte)(num6 & 255),
				(byte)((num7 >> 16) & 255),
				(byte)((num7 >> 8) & 255),
				(byte)(num7 & 255),
				(byte)((num8 >> 16) & 255),
				(byte)((num8 >> 8) & 255),
				(byte)(num8 & 255),
				0
			};
			CYFSConvolution cyfsconvolution = new CYFSConvolution();
			byte[] array3 = new byte[25];
			byte[] array4 = cyfsconvolution.encode(array2, 100, 25);
			int num9 = 0;
			for (int i = 0; i < 100; i++)
			{
				int num10 = fusion_extract.INTERLEAVE_TABLE22[i];
				bool flag = utils.bit_reader(array4, num9);
				num9++;
				bool flag2 = utils.bit_reader(array4, num9);
				num9++;
				array3 = utils.bit_writer(array3, num10, flag);
				num10++;
				array3 = utils.bit_writer(array3, num10, flag2);
			}
			return array3;
		}

		// Token: 0x0600033A RID: 826 RVA: 0x00025C70 File Offset: 0x00023E70
		public static void changeDGID(int dgid)
		{
			fusion_extract.decode_FICH(fusion_extract.C4fM_FRAME_HEADER);
			fusion_extract.setDGId((byte)dgid);
			Buffer.BlockCopy(fusion_extract.encode_FICH(), 0, fusion_extract.C4fM_FRAME_HEADER, 5, 25);
			fusion_extract.decode_FICH(fusion_extract.C4fM_FRAME_FN0);
			fusion_extract.setDGId((byte)dgid);
			Buffer.BlockCopy(fusion_extract.encode_FICH(), 0, fusion_extract.C4fM_FRAME_FN0, 5, 25);
			fusion_extract.decode_FICH(fusion_extract.C4fM_FRAME_FN1);
			fusion_extract.setDGId((byte)dgid);
			Buffer.BlockCopy(fusion_extract.encode_FICH(), 0, fusion_extract.C4fM_FRAME_FN1, 5, 25);
			fusion_extract.decode_FICH(fusion_extract.C4fM_FRAME_FN2);
			fusion_extract.setDGId((byte)dgid);
			Buffer.BlockCopy(fusion_extract.encode_FICH(), 0, fusion_extract.C4fM_FRAME_FN2, 5, 25);
			fusion_extract.decode_FICH(fusion_extract.C4fM_FRAME_FN3);
			fusion_extract.setDGId((byte)dgid);
			Buffer.BlockCopy(fusion_extract.encode_FICH(), 0, fusion_extract.C4fM_FRAME_FN3, 5, 25);
			fusion_extract.decode_FICH(fusion_extract.C4fM_FRAME_FN4);
			fusion_extract.setDGId((byte)dgid);
			Buffer.BlockCopy(fusion_extract.encode_FICH(), 0, fusion_extract.C4fM_FRAME_FN4, 5, 25);
			fusion_extract.decode_FICH(fusion_extract.C4fM_FRAME_FN5);
			fusion_extract.setDGId((byte)dgid);
			Buffer.BlockCopy(fusion_extract.encode_FICH(), 0, fusion_extract.C4fM_FRAME_FN5, 5, 25);
			fusion_extract.decode_FICH(fusion_extract.C4fM_FRAME_FN6);
			fusion_extract.setDGId((byte)dgid);
			Buffer.BlockCopy(fusion_extract.encode_FICH(), 0, fusion_extract.C4fM_FRAME_FN6, 5, 25);
		}

		// Token: 0x0600033B RID: 827 RVA: 0x00025DA5 File Offset: 0x00023FA5
		public static byte[] bit_writer(byte[] p, int i, bool b)
		{
			bool[] array = utils.bytesToBits(p);
			array[i] = b;
			return utils.bitsToBytes(array);
		}

		// Token: 0x0600033C RID: 828 RVA: 0x00025DB6 File Offset: 0x00023FB6
		public static int getFI()
		{
			return (fusion_extract.m_fich[0] >> 6) & 3;
		}

		// Token: 0x0600033D RID: 829 RVA: 0x00025DC3 File Offset: 0x00023FC3
		public static int getCS()
		{
			return (fusion_extract.m_fich[0] >> 4) & 3;
		}

		// Token: 0x0600033E RID: 830 RVA: 0x00025DD0 File Offset: 0x00023FD0
		public static int getCM()
		{
			return (fusion_extract.m_fich[0] >> 2) & 3;
		}

		// Token: 0x0600033F RID: 831 RVA: 0x00025DDD File Offset: 0x00023FDD
		public static int getBN()
		{
			return (int)(fusion_extract.m_fich[0] & 3);
		}

		// Token: 0x06000340 RID: 832 RVA: 0x00025DE8 File Offset: 0x00023FE8
		public static int getBT()
		{
			return (fusion_extract.m_fich[1] >> 6) & 3;
		}

		// Token: 0x06000341 RID: 833 RVA: 0x00025DF5 File Offset: 0x00023FF5
		public static int getFN()
		{
			return (fusion_extract.m_fich[1] >> 3) & 7;
		}

		// Token: 0x06000342 RID: 834 RVA: 0x00025E02 File Offset: 0x00024002
		public static int getFT()
		{
			return (int)(fusion_extract.m_fich[1] & 7);
		}

		// Token: 0x06000343 RID: 835 RVA: 0x00025E0D File Offset: 0x0002400D
		public static int getDT()
		{
			return (int)(fusion_extract.m_fich[2] & 3);
		}

		// Token: 0x06000344 RID: 836 RVA: 0x00025E18 File Offset: 0x00024018
		public static int getMR()
		{
			return (fusion_extract.m_fich[2] >> 3) & 3;
		}

		// Token: 0x06000345 RID: 837 RVA: 0x00025E25 File Offset: 0x00024025
		public static bool getVOIP()
		{
			return (fusion_extract.m_fich[2] & 4) == 4;
		}

		// Token: 0x06000346 RID: 838 RVA: 0x00025E33 File Offset: 0x00024033
		public static bool getDev()
		{
			return (fusion_extract.m_fich[2] & 64) == 64;
		}

		// Token: 0x06000347 RID: 839 RVA: 0x00025E43 File Offset: 0x00024043
		public static bool getSQLType()
		{
			return (fusion_extract.m_fich[3] & 128) == 128;
		}

		// Token: 0x06000348 RID: 840 RVA: 0x00025E59 File Offset: 0x00024059
		public static int getDGId()
		{
			return (int)(fusion_extract.m_fich[3] & 127);
		}

		// Token: 0x06000349 RID: 841 RVA: 0x00025E65 File Offset: 0x00024065
		public static void setFI(byte fi)
		{
			byte[] fich = fusion_extract.m_fich;
			int num = 0;
			fich[num] &= 63;
			byte[] fich2 = fusion_extract.m_fich;
			int num2 = 0;
			fich2[num2] |= (byte)(((int)fi << 6) & 192);
		}

		// Token: 0x0600034A RID: 842 RVA: 0x00025E93 File Offset: 0x00024093
		public static void setCS(byte cs)
		{
			byte[] fich = fusion_extract.m_fich;
			int num = 0;
			fich[num] &= 207;
			byte[] fich2 = fusion_extract.m_fich;
			int num2 = 0;
			fich2[num2] |= (byte)(((int)cs << 4) & 48);
		}

		// Token: 0x0600034B RID: 843 RVA: 0x00025EC1 File Offset: 0x000240C1
		public static void setCM(byte cm)
		{
			byte[] fich = fusion_extract.m_fich;
			int num = 0;
			fich[num] &= 243;
			byte[] fich2 = fusion_extract.m_fich;
			int num2 = 0;
			fich2[num2] |= (byte)(((int)cm << 2) & 12);
		}

		// Token: 0x0600034C RID: 844 RVA: 0x00025EEF File Offset: 0x000240EF
		public static void setBN(byte bn)
		{
			byte[] fich = fusion_extract.m_fich;
			int num = 0;
			fich[num] &= 252;
			byte[] fich2 = fusion_extract.m_fich;
			int num2 = 0;
			fich2[num2] |= bn & 3;
		}

		// Token: 0x0600034D RID: 845 RVA: 0x00025F1A File Offset: 0x0002411A
		public static void setFN(byte fn)
		{
			byte[] fich = fusion_extract.m_fich;
			int num = 1;
			fich[num] &= 199;
			byte[] fich2 = fusion_extract.m_fich;
			int num2 = 1;
			fich2[num2] |= (byte)(((int)fn << 3) & 56);
		}

		// Token: 0x0600034E RID: 846 RVA: 0x00025F48 File Offset: 0x00024148
		public static void setFT(byte ft)
		{
			byte[] fich = fusion_extract.m_fich;
			int num = 1;
			fich[num] &= 248;
			byte[] fich2 = fusion_extract.m_fich;
			int num2 = 1;
			fich2[num2] |= ft & 7;
		}

		// Token: 0x0600034F RID: 847 RVA: 0x00025F73 File Offset: 0x00024173
		public static void setMR(byte mr)
		{
			byte[] fich = fusion_extract.m_fich;
			int num = 2;
			fich[num] &= 199;
			byte[] fich2 = fusion_extract.m_fich;
			int num2 = 2;
			fich2[num2] |= (byte)(((int)mr << 3) & 56);
		}

		// Token: 0x06000350 RID: 848 RVA: 0x00025FA1 File Offset: 0x000241A1
		public static void setVoIP(bool on)
		{
			if (on)
			{
				byte[] fich = fusion_extract.m_fich;
				int num = 2;
				fich[num] |= 4;
				return;
			}
			byte[] fich2 = fusion_extract.m_fich;
			int num2 = 2;
			fich2[num2] &= 251;
		}

		// Token: 0x06000351 RID: 849 RVA: 0x00025FCD File Offset: 0x000241CD
		public static void setDev(bool on)
		{
			if (on)
			{
				byte[] fich = fusion_extract.m_fich;
				int num = 2;
				fich[num] |= 64;
				return;
			}
			byte[] fich2 = fusion_extract.m_fich;
			int num2 = 2;
			fich2[num2] &= 191;
		}

		// Token: 0x06000352 RID: 850 RVA: 0x00025FFA File Offset: 0x000241FA
		public static void setDT(byte dt)
		{
			byte[] fich = fusion_extract.m_fich;
			int num = 2;
			fich[num] &= 252;
			byte[] fich2 = fusion_extract.m_fich;
			int num2 = 2;
			fich2[num2] |= dt & 3;
		}

		// Token: 0x06000353 RID: 851 RVA: 0x00026025 File Offset: 0x00024225
		public static void setDGId(byte id)
		{
			byte[] fich = fusion_extract.m_fich;
			int num = 3;
			fich[num] &= 128;
			byte[] fich2 = fusion_extract.m_fich;
			int num2 = 3;
			fich2[num2] |= id & 127;
		}

		// Token: 0x06000354 RID: 852 RVA: 0x00026051 File Offset: 0x00024251
		public static void setBT(byte bt)
		{
			byte[] fich = fusion_extract.m_fich;
			int num = 1;
			fich[num] &= 63;
			byte[] fich2 = fusion_extract.m_fich;
			int num2 = 1;
			fich2[num2] |= (byte)(((int)bt << 6) & 192);
		}

		// Token: 0x04000221 RID: 545
		public static int fn = 0;

		// Token: 0x04000222 RID: 546
		public static int voiceCounter = 0;

		// Token: 0x04000223 RID: 547
		private static string radio = "";

		// Token: 0x04000226 RID: 550
		public static int[] INTERLEAVE_TABLE22 = new int[]
		{
			0, 40, 80, 120, 160, 2, 42, 82, 122, 162,
			4, 44, 84, 124, 164, 6, 46, 86, 126, 166,
			8, 48, 88, 128, 168, 10, 50, 90, 130, 170,
			12, 52, 92, 132, 172, 14, 54, 94, 134, 174,
			16, 56, 96, 136, 176, 18, 58, 98, 138, 178,
			20, 60, 100, 140, 180, 22, 62, 102, 142, 182,
			24, 64, 104, 144, 184, 26, 66, 106, 146, 186,
			28, 68, 108, 148, 188, 30, 70, 110, 150, 190,
			32, 72, 112, 152, 192, 34, 74, 114, 154, 194,
			36, 76, 116, 156, 196, 38, 78, 118, 158, 198
		};

		// Token: 0x04000227 RID: 551
		public static int[] INTERLEAVE_TABLE_5_20 = new int[]
		{
			0, 40, 80, 120, 160, 2, 42, 82, 122, 162,
			4, 44, 84, 124, 164, 6, 46, 86, 126, 166,
			8, 48, 88, 128, 168, 10, 50, 90, 130, 170,
			12, 52, 92, 132, 172, 14, 54, 94, 134, 174,
			16, 56, 96, 136, 176, 18, 58, 98, 138, 178,
			20, 60, 100, 140, 180, 22, 62, 102, 142, 182,
			24, 64, 104, 144, 184, 26, 66, 106, 146, 186,
			28, 68, 108, 148, 188, 30, 70, 110, 150, 190,
			32, 72, 112, 152, 192, 34, 74, 114, 154, 194,
			36, 76, 116, 156, 196, 38, 78, 118, 158, 198
		};

		// Token: 0x04000228 RID: 552
		public static uint[] INTERLEAVE_TABLE = new uint[]
		{
			0U, 40U, 80U, 120U, 160U, 2U, 42U, 82U, 122U, 162U,
			4U, 44U, 84U, 124U, 164U, 6U, 46U, 86U, 126U, 166U,
			8U, 48U, 88U, 128U, 168U, 10U, 50U, 90U, 130U, 170U,
			12U, 52U, 92U, 132U, 172U, 14U, 54U, 94U, 134U, 174U,
			16U, 56U, 96U, 136U, 176U, 18U, 58U, 98U, 138U, 178U,
			20U, 60U, 100U, 140U, 180U, 22U, 62U, 102U, 142U, 182U,
			24U, 64U, 104U, 144U, 184U, 26U, 66U, 106U, 146U, 186U,
			28U, 68U, 108U, 148U, 188U, 30U, 70U, 110U, 150U, 190U,
			32U, 72U, 112U, 152U, 192U, 34U, 74U, 114U, 154U, 194U,
			36U, 76U, 116U, 156U, 196U, 38U, 78U, 118U, 158U, 198U
		};

		// Token: 0x04000229 RID: 553
		public static int[] INTERLEAVE_TABLE2 = new int[]
		{
			0, 40, 80, 120, 160, 2, 42, 82, 122, 162,
			4, 44, 84, 124, 164, 6, 46, 86, 126, 166,
			8, 48, 88, 128, 168, 10, 50, 90, 130, 170,
			12, 52, 92, 132, 172, 14, 54, 94, 134, 174,
			16, 56, 96, 136, 176, 18, 58, 98, 138, 178,
			20, 60, 100, 140, 180, 22, 62, 102, 142, 182,
			24, 64, 104, 144, 184, 26, 66, 106, 146, 186,
			28, 68, 108, 148, 188, 30, 70, 110, 150, 190,
			32, 72, 112, 152, 192, 34, 74, 114, 154, 194,
			36, 76, 116, 156, 196, 38, 78, 118, 158, 198
		};

		// Token: 0x0400022A RID: 554
		public static int[] INTERLEAVE_TABLE_9_20 = new int[]
		{
			0, 40, 80, 120, 160, 200, 240, 280, 320, 2,
			42, 82, 122, 162, 202, 242, 282, 322, 4, 44,
			84, 124, 164, 204, 244, 284, 324, 6, 46, 86,
			126, 166, 206, 246, 286, 326, 8, 48, 88, 128,
			168, 208, 248, 288, 328, 10, 50, 90, 130, 170,
			210, 250, 290, 330, 12, 52, 92, 132, 172, 212,
			252, 292, 332, 14, 54, 94, 134, 174, 214, 254,
			294, 334, 16, 56, 96, 136, 176, 216, 256, 296,
			336, 18, 58, 98, 138, 178, 218, 258, 298, 338,
			20, 60, 100, 140, 180, 220, 260, 300, 340, 22,
			62, 102, 142, 182, 222, 262, 302, 342, 24, 64,
			104, 144, 184, 224, 264, 304, 344, 26, 66, 106,
			146, 186, 226, 266, 306, 346, 28, 68, 108, 148,
			188, 228, 268, 308, 348, 30, 70, 110, 150, 190,
			230, 270, 310, 350, 32, 72, 112, 152, 192, 232,
			272, 312, 352, 34, 74, 114, 154, 194, 234, 274,
			314, 354, 36, 76, 116, 156, 196, 236, 276, 316,
			356, 38, 78, 118, 158, 198, 238, 278, 318, 358
		};

		// Token: 0x0400022B RID: 555
		public static int[] INTERLEAVE_TABLE_26_4 = new int[]
		{
			0, 4, 8, 12, 16, 20, 24, 28, 32, 36,
			40, 44, 48, 52, 56, 60, 64, 68, 72, 76,
			80, 84, 88, 92, 96, 100, 1, 5, 9, 13,
			17, 21, 25, 29, 33, 37, 41, 45, 49, 53,
			57, 61, 65, 69, 73, 77, 81, 85, 89, 93,
			97, 101, 2, 6, 10, 14, 18, 22, 26, 30,
			34, 38, 42, 46, 50, 54, 58, 62, 66, 70,
			74, 78, 82, 86, 90, 94, 98, 102, 3, 7,
			11, 15, 19, 23, 27, 31, 35, 39, 43, 47,
			51, 55, 59, 63, 67, 71, 75, 79, 83, 87,
			91, 95, 99, 103
		};

		// Token: 0x0400022C RID: 556
		public static int[] INTERLEAVE_TABLE_26_4_2 = new int[]
		{
			0, 26, 52, 78, 1, 27, 53, 79, 2, 28,
			54, 80, 3, 29, 55, 81, 4, 30, 56, 82,
			5, 31, 57, 83, 6, 32, 58, 84, 7, 33,
			59, 85, 8, 34, 60, 86, 9, 35, 61, 87,
			10, 36, 62, 88, 11, 37, 63, 89, 12, 38,
			64, 90, 13, 39, 65, 91, 14, 40, 66, 92,
			15, 41, 67, 93, 16, 42, 68, 94, 17, 43,
			69, 95, 18, 44, 70, 96, 19, 45, 71, 97,
			20, 46, 72, 98, 21, 47, 73, 99, 22, 48,
			74, 100, 23, 49, 75, 101, 24, 50, 76, 102,
			25, 51, 77, 103
		};

		// Token: 0x0400022D RID: 557
		private static byte[] WHITENING_DATA = new byte[]
		{
			147, 215, 81, 33, 156, 47, 108, 208, 239, 15,
			248, 61, 241, 115, 32, 148, 237, 30, 124, 216
		};

		// Token: 0x0400022E RID: 558
		private static int[] INTERLEAVE_TABLE_VCH49 = new int[]
		{
			0, 3, 6, 9, 12, 15, 18, 21, 24, 27,
			30, 33, 36, 39, 41, 43, 45, 47, 1, 4,
			7, 10, 13, 16, 19, 22, 25, 28, 31, 34,
			37, 40, 42, 44, 46, 48, 2, 5, 8, 11,
			14, 17, 20, 23, 26, 29, 32, 35, 38
		};

		// Token: 0x0400022F RID: 559
		private static int[] INTER_TAB_VCH49 = new int[]
		{
			11, 17, 47, 10, 16, 46, 9, 15, 45, 8,
			14, 44, 7, 13, 43, 6, 12, 42, 5, 34,
			41, 4, 33, 40, 3, 32, 39, 2, 31, 38,
			1, 30, 37, 0, 29, 36, 23, 28, 35, 22,
			27, 21, 26, 20, 25, 19, 24, 18, 48
		};

		// Token: 0x04000230 RID: 560
		private static int[] INTER_TABBIE_VCH49 = new int[]
		{
			0, 18, 36, 1, 19, 37, 2, 20, 38, 3,
			21, 39, 4, 22, 40, 5, 23, 41, 6, 24,
			42, 7, 25, 43, 8, 26, 44, 9, 27, 45,
			10, 28, 46, 11, 29, 47, 12, 30, 48, 13,
			31, 14, 32, 15, 33, 16, 34, 17, 35
		};

		// Token: 0x04000231 RID: 561
		public static byte[] YSF_SILENCE = new byte[]
		{
			123, 178, 142, 67, 54, 228, 162, 57, 120, 73,
			51, 104, 51
		};

		// Token: 0x04000232 RID: 562
		public static byte[] C4fM_FRAME_HEADER = new byte[]
		{
			212, 113, 201, 99, 77, 17, 45, 56, 220, 236,
			34, 1, byte.MaxValue, 48, 14, 208, 114, 130, 120, 236,
			96, 51, 0, 134, 112, 125, 28, 32, 166, 111,
			243, 206, 251, 204, 22, 83, 27, 93, 40, 223,
			35, 99, 192, 23, 96, 127, 28, 136, 180, 165,
			246, 52, 150, 148, 38, 224, 218, 185, 168, 90,
			186, 70, 146, 16, 20, 190, 157, 172, 44, 222,
			244, 121, 25, 156, 166, 117, 168, 247, 127, 184,
			17, 16, 242, 198, 99, 227, 3, 62, 110, 41,
			249, 150, 143, 61, 239, 14, 132, 37, 30, 73,
			148, 100, 3, 116, 246, 14, 87, 61, 178, 49,
			159, 19, 115, 246, 13, 251, 213, 137, 1, 147
		};

		// Token: 0x04000233 RID: 563
		public static byte[] C4fM_FRAME_FN1 = new byte[]
		{
			212, 113, 201, 99, 77, 33, 157, 56, 53, 147,
			225, 145, byte.MaxValue, 71, 240, 145, 130, 129, 124, 118,
			44, 243, 2, 24, 90, 231, 156, 32, 83, 39,
			231, 123, 202, 239, 156, 127, 246, 200, 37, 114,
			244, 166, 125, 120, 77, 65, 30, 81, 72, 83,
			153, 7, 211, 127, 213, 232, 37, 114, 176, 226,
			56, 120, 78, 103, 46, 81, 169, 240, 112, 237,
			175, 127, 247, 200, 37, 114, 244, 166, 125, 120,
			76, 103, 46, 81, 24, 251, 47, 164, 85, 127,
			247, 200, 37, 114, 244, 166, 125, 120, 76, 103,
			46, 81, 48, 223, 29, 225, 171, 127, 247, 201,
			53, 114, 244, 166, 125, 123, 127, 103, 46, 81
		};

		// Token: 0x04000234 RID: 564
		public static byte[] C4fM_FRAME_FN0 = new byte[]
		{
			212, 113, 201, 99, 77, 33, 141, 64, 164, 48,
			225, 130, 240, 50, 120, 145, 160, 16, 236, 136,
			44, 195, 240, 124, 172, 247, 95, 131, 7, 112,
			243, 239, 219, 110, 158, 127, 247, 200, 37, 80,
			212, 166, 125, 107, 127, 103, 46, 81, 187, 144,
			70, 6, 107, 127, 247, 136, 37, 80, 212, 166,
			125, 107, 127, 103, 46, 81, 42, 77, 21, 116,
			4, 127, 247, 200, 36, 65, 212, 166, 125, 106,
			127, 103, 46, 81, 249, 3, 215, 224, 152, 127,
			247, 200, 37, 80, 212, 166, 124, 106, 127, 103,
			46, 81, 96, 249, 209, 217, 164, 127, 247, 200,
			36, 65, 196, 166, 125, 106, 127, 103, 47, 81
		};

		// Token: 0x04000235 RID: 565
		public static byte[] C4fM_FRAME_FN2 = new byte[]
		{
			212, 113, 201, 99, 77, 32, 109, 56, 68, 104,
			237, 129, byte.MaxValue, 231, 152, 155, 242, 130, 228, 84,
			47, 243, 3, 251, 200, 249, 92, 33, 56, 60,
			248, 147, 99, 110, 78, 127, 247, 200, 36, 65,
			196, 182, 125, 107, 127, 103, 46, 81, 106, 44,
			250, 6, 232, 127, 230, 136, 36, 65, 196, 182,
			121, 107, 127, 69, 14, 81, 151, 121, 21, 116,
			213, 127, 247, 200, 37, 80, 196, 182, 125, 107,
			127, 103, 46, 81, 63, 1, 23, 224, 231, 127,
			247, 200, 36, 65, 196, 182, 125, 107, 127, 103,
			46, 81, 220, 64, 241, 217, 44, 127, 246, 200,
			37, 80, 196, 166, 125, 107, 127, 69, 14, 81
		};

		// Token: 0x04000236 RID: 566
		public static byte[] C4fM_FRAME_FN3 = new byte[]
		{
			212, 113, 201, 99, 77, 32, 125, 64, 209, 203,
			237, 146, 240, 146, 16, 155, 208, 3, 116, 170,
			47, 195, 241, 159, 62, 249, 159, 130, 108, 107,
			248, 147, 99, 110, 78, 127, 246, 200, 37, 80,
			212, 166, 121, 107, 127, 69, 14, 81, 106, 44,
			250, 6, 232, 127, 246, 200, 37, 80, 212, 166,
			125, 107, 127, 69, 14, 81, 151, 121, 21, 116,
			213, 127, 246, 200, 37, 80, 196, 166, 125, 107,
			127, 69, 14, 81, 63, 1, 7, 224, 231, 127,
			247, 200, 37, 80, 196, 166, 125, 106, 127, 103,
			46, 81, 220, 0, 241, 217, 44, 127, 247, 200,
			37, 80, 212, 166, 125, 106, 127, 103, 46, 81
		};

		// Token: 0x04000237 RID: 567
		public static byte[] C4fM_FRAME_FN5 = new byte[]
		{
			212, 113, 201, 99, 77, 44, 77, 56, 61, 249,
			231, 225, 252, 231, 210, 152, 2, 128, 115, 231,
			33, 179, 3, 3, 64, 254, 156, 34, 148, 196,
			248, 131, 99, 110, 78, 127, 247, 200, 37, 80,
			196, 166, 125, 107, 127, 103, 46, 81, 106, 44,
			250, 6, 232, 127, 246, 200, 37, 80, 196, 166,
			125, 107, 127, 69, 14, 81, 151, 121, 21, 116,
			213, 127, 246, 200, 37, 80, 212, 166, 125, 107,
			127, 69, 14, 81, 63, 1, 7, 224, 231, 127,
			246, 200, 37, 80, 212, 166, 125, 104, 77, 69,
			14, 81, 220, 0, 241, 217, 44, 127, 246, 200,
			37, 80, 212, 166, 124, 107, 127, 69, 14, 17
		};

		// Token: 0x04000238 RID: 568
		public static byte[] C4fM_FRAME_FN4 = new byte[]
		{
			212, 113, 201, 99, 77, 44, 29, 64, 172, 26,
			231, 242, 243, 146, 26, 152, 32, 17, 227, 25,
			33, 131, 241, 103, 182, 254, 95, 129, 192, 147,
			248, 131, 10, 239, 96, 127, 246, 200, 37, 80,
			212, 166, 125, 106, 127, 69, 14, 16, 106, 44,
			25, 7, 141, 127, 246, 200, 37, 80, 212, 166,
			124, 107, 127, 69, 14, 17, 151, 121, 176, 237,
			206, 127, 246, 200, 37, 80, 196, 166, 125, 107,
			127, 69, 14, 81, 63, 1, 47, 164, 39, 127,
			246, 200, 37, 80, 212, 166, 124, 107, 127, 69,
			14, 17, 220, 64, 29, 225, 208, 127, 246, 200,
			37, 80, 212, 166, 124, 107, 127, 69, 14, 17
		};

		// Token: 0x04000239 RID: 569
		public static byte[] C4fM_FRAME_FN6 = new byte[]
		{
			212, 113, 201, 99, 77, 45, 189, 56, 76, 2,
			235, 241, 252, 71, 186, 130, 114, 131, 235, 197,
			34, 179, 2, 224, 210, 240, 92, 35, byte.MaxValue, 223,
			237, 56, 143, 35, 96, 127, 246, 200, 37, 80,
			212, 166, 125, 107, 127, 69, 14, 80, 119, 231,
			246, 183, 9, 127, 246, 200, 37, 80, 196, 166,
			125, 107, 127, 69, 14, 81, 134, 124, 249, 166,
			164, 127, 246, 200, 37, 80, 196, 166, 125, 107,
			127, 69, 14, 17, 253, 208, 80, 243, 133, 110,
			231, 200, 37, 80, 196, 166, 125, 107, 127, 85,
			12, 115, 161, 38, 188, 245, 79, 110, 231, 200,
			37, 80, 196, 166, 124, 107, 127, 85, 12, 115
		};

		// Token: 0x0400023A RID: 570
		public static byte[] C4fM_FRAME_END = new byte[]
		{
			212, 113, 201, 99, 77, 210, 197, 120, 60, 3,
			99, 237, 188, 230, 125, 156, 113, 96, 114, 131,
			234, 96, 227, 1, 204, 190, 143, 210, 154, 28,
			243, 207, 155, 204, 23, 83, 41, 173, 104, 223,
			35, 99, 192, 23, 96, 127, 28, 136, 187, 165,
			215, 244, 150, 156, 38, 225, 90, 185, 168, 90,
			186, 70, 146, 16, 20, 190, 157, 161, 44, 239,
			52, 121, 30, 156, 181, 117, 168, 247, 127, 184,
			17, 16, 242, 198, 227, 227, 9, 62, 83, 233,
			249, 156, 143, 61, 239, 14, 132, 37, 30, 9,
			148, 100, 6, 116, 246, 5, 87, 57, 50, 49,
			151, 19, 115, 246, 13, 251, 213, 137, 3, 57
		};

		// Token: 0x0400023B RID: 571
		public static byte[] m_fich = new byte[6];

		// Token: 0x0400023C RID: 572
		public const uint NPAR = 3U;

		// Token: 0x0400023D RID: 573
		public static uint MAXDEG = 6U;

		// Token: 0x0400023E RID: 574
		public static readonly byte[] POLY = new byte[]
		{
			64, 56, 14, 1, 0, 0, 0, 0, 0, 0,
			0, 0
		};

		// Token: 0x0400023F RID: 575
		public static readonly byte[] EXP_TABLE = new byte[]
		{
			1, 2, 4, 8, 16, 32, 64, 128, 29, 58,
			116, 232, 205, 135, 19, 38, 76, 152, 45, 90,
			180, 117, 234, 201, 143, 3, 6, 12, 24, 48,
			96, 192, 157, 39, 78, 156, 37, 74, 148, 53,
			106, 212, 181, 119, 238, 193, 159, 35, 70, 140,
			5, 10, 20, 40, 80, 160, 93, 186, 105, 210,
			185, 111, 222, 161, 95, 190, 97, 194, 153, 47,
			94, 188, 101, 202, 137, 15, 30, 60, 120, 240,
			253, 231, 211, 187, 107, 214, 177, 127, 254, 225,
			223, 163, 91, 182, 113, 226, 217, 175, 67, 134,
			17, 34, 68, 136, 13, 26, 52, 104, 208, 189,
			103, 206, 129, 31, 62, 124, 248, 237, 199, 147,
			59, 118, 236, 197, 151, 51, 102, 204, 133, 23,
			46, 92, 184, 109, 218, 169, 79, 158, 33, 66,
			132, 21, 42, 84, 168, 77, 154, 41, 82, 164,
			85, 170, 73, 146, 57, 114, 228, 213, 183, 115,
			230, 209, 191, 99, 198, 145, 63, 126, 252, 229,
			215, 179, 123, 246, 241, byte.MaxValue, 227, 219, 171, 75,
			150, 49, 98, 196, 149, 55, 110, 220, 165, 87,
			174, 65, 130, 25, 50, 100, 200, 141, 7, 14,
			28, 56, 112, 224, 221, 167, 83, 166, 81, 162,
			89, 178, 121, 242, 249, 239, 195, 155, 43, 86,
			172, 69, 138, 9, 18, 36, 72, 144, 61, 122,
			244, 245, 247, 243, 251, 235, 203, 139, 11, 22,
			44, 88, 176, 125, 250, 233, 207, 131, 27, 54,
			108, 216, 173, 71, 142, 1, 2, 4, 8, 16,
			32, 64, 128, 29, 58, 116, 232, 205, 135, 19,
			38, 76, 152, 45, 90, 180, 117, 234, 201, 143,
			3, 6, 12, 24, 48, 96, 192, 157, 39, 78,
			156, 37, 74, 148, 53, 106, 212, 181, 119, 238,
			193, 159, 35, 70, 140, 5, 10, 20, 40, 80,
			160, 93, 186, 105, 210, 185, 111, 222, 161, 95,
			190, 97, 194, 153, 47, 94, 188, 101, 202, 137,
			15, 30, 60, 120, 240, 253, 231, 211, 187, 107,
			214, 177, 127, 254, 225, 223, 163, 91, 182, 113,
			226, 217, 175, 67, 134, 17, 34, 68, 136, 13,
			26, 52, 104, 208, 189, 103, 206, 129, 31, 62,
			124, 248, 237, 199, 147, 59, 118, 236, 197, 151,
			51, 102, 204, 133, 23, 46, 92, 184, 109, 218,
			169, 79, 158, 33, 66, 132, 21, 42, 84, 168,
			77, 154, 41, 82, 164, 85, 170, 73, 146, 57,
			114, 228, 213, 183, 115, 230, 209, 191, 99, 198,
			145, 63, 126, 252, 229, 215, 179, 123, 246, 241,
			byte.MaxValue, 227, 219, 171, 75, 150, 49, 98, 196, 149,
			55, 110, 220, 165, 87, 174, 65, 130, 25, 50,
			100, 200, 141, 7, 14, 28, 56, 112, 224, 221,
			167, 83, 166, 81, 162, 89, 178, 121, 242, 249,
			239, 195, 155, 43, 86, 172, 69, 138, 9, 18,
			36, 72, 144, 61, 122, 244, 245, 247, 243, 251,
			235, 203, 139, 11, 22, 44, 88, 176, 125, 250,
			233, 207, 131, 27, 54, 108, 216, 173, 71, 142,
			1, 0
		};

		// Token: 0x04000240 RID: 576
		public static readonly byte[] LOG_TABLE = new byte[]
		{
			0, 0, 1, 25, 2, 50, 26, 198, 3, 223,
			51, 238, 27, 104, 199, 75, 4, 100, 224, 14,
			52, 141, 239, 129, 28, 193, 105, 248, 200, 8,
			76, 113, 5, 138, 101, 47, 225, 36, 15, 33,
			53, 147, 142, 218, 240, 18, 130, 69, 29, 181,
			194, 125, 106, 39, 249, 185, 201, 154, 9, 120,
			77, 228, 114, 166, 6, 191, 139, 98, 102, 221,
			48, 253, 226, 152, 37, 179, 16, 145, 34, 136,
			54, 208, 148, 206, 143, 150, 219, 189, 241, 210,
			19, 92, 131, 56, 70, 64, 30, 66, 182, 163,
			195, 72, 126, 110, 107, 58, 40, 84, 250, 133,
			186, 61, 202, 94, 155, 159, 10, 21, 121, 43,
			78, 212, 229, 172, 115, 243, 167, 87, 7, 112,
			192, 247, 140, 128, 99, 13, 103, 74, 222, 237,
			49, 197, 254, 24, 227, 165, 153, 119, 38, 184,
			180, 124, 17, 68, 146, 217, 35, 32, 137, 46,
			55, 63, 209, 91, 149, 188, 207, 205, 144, 135,
			151, 178, 220, 252, 190, 97, 242, 86, 211, 171,
			20, 42, 93, 158, 132, 60, 57, 83, 71, 109,
			65, 162, 31, 45, 67, 216, 183, 123, 164, 118,
			196, 23, 73, 236, 127, 12, 111, 246, 108, 161,
			59, 82, 41, 157, 85, 170, 251, 96, 134, 177,
			187, 204, 62, 90, 203, 89, 95, 176, 156, 169,
			160, 81, 11, 245, 22, 235, 122, 117, 44, 215,
			79, 174, 213, 233, 230, 231, 173, 232, 116, 214,
			244, 234, 168, 80, 88, 175
		};

		// Token: 0x04000241 RID: 577
		public static readonly byte[] BIT_MASK_TABLE = new byte[] { 128, 64, 32, 16, 8, 4, 2, 1 };
	}
}
