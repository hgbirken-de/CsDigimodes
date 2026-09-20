using System;
using System.Text;

namespace BlueDV
{
	// Token: 0x02000025 RID: 37
	internal class DTMF
	{
		// Token: 0x0600021C RID: 540 RVA: 0x00013B64 File Offset: 0x00011D64
		public static bool testme(byte[] ambe, bool end)
		{
			if (!end && (ambe[0] & DTMF.DTMF_MASK[0]) == DTMF.DTMF_SIG[0] && (ambe[1] & DTMF.DTMF_MASK[1]) == DTMF.DTMF_SIG[1] && (ambe[2] & DTMF.DTMF_MASK[2]) == DTMF.DTMF_SIG[2] && (ambe[3] & DTMF.DTMF_MASK[3]) == DTMF.DTMF_SIG[3] && (ambe[4] & DTMF.DTMF_MASK[4]) == DTMF.DTMF_SIG[4] && (ambe[5] & DTMF.DTMF_MASK[5]) == DTMF.DTMF_SIG[5] && (ambe[6] & DTMF.DTMF_MASK[6]) == DTMF.DTMF_SIG[6] && (ambe[7] & DTMF.DTMF_MASK[7]) == DTMF.DTMF_SIG[7] && (ambe[8] & DTMF.DTMF_MASK[8]) == DTMF.DTMF_SIG[8])
			{
				int num = (int)(ambe[4] & DTMF.DTMF_SYM_MASK[0]);
				int num2 = (int)(ambe[5] & DTMF.DTMF_SYM_MASK[1]);
				int num3 = (int)(ambe[7] & DTMF.DTMF_SYM_MASK[2]);
				int num4 = (int)(ambe[8] & DTMF.DTMF_SYM_MASK[3]);
				char c = ' ';
				if (num == (int)DTMF.DTMF_SYM0[0] && num2 == (int)DTMF.DTMF_SYM0[1] && num3 == (int)DTMF.DTMF_SYM0[2] && num4 == (int)DTMF.DTMF_SYM0[3])
				{
					c = '0';
				}
				else if (num == (int)DTMF.DTMF_SYM1[0] && num2 == (int)DTMF.DTMF_SYM1[1] && num3 == (int)DTMF.DTMF_SYM1[2] && num4 == (int)DTMF.DTMF_SYM1[3])
				{
					c = '1';
				}
				else if (num == (int)DTMF.DTMF_SYM2[0] && num2 == (int)DTMF.DTMF_SYM2[1] && num3 == (int)DTMF.DTMF_SYM2[2] && num4 == (int)DTMF.DTMF_SYM2[3])
				{
					c = '2';
				}
				else if (num == (int)DTMF.DTMF_SYM3[0] && num2 == (int)DTMF.DTMF_SYM3[1] && num3 == (int)DTMF.DTMF_SYM3[2] && num4 == (int)DTMF.DTMF_SYM3[3])
				{
					c = '3';
				}
				else if (num == (int)DTMF.DTMF_SYM4[0] && num2 == (int)DTMF.DTMF_SYM4[1] && num3 == (int)DTMF.DTMF_SYM4[2] && num4 == (int)DTMF.DTMF_SYM4[3])
				{
					c = '4';
				}
				else if (num == (int)DTMF.DTMF_SYM5[0] && num2 == (int)DTMF.DTMF_SYM5[1] && num3 == (int)DTMF.DTMF_SYM5[2] && num4 == (int)DTMF.DTMF_SYM5[3])
				{
					c = '5';
				}
				else if (num == (int)DTMF.DTMF_SYM6[0] && num2 == (int)DTMF.DTMF_SYM6[1] && num3 == (int)DTMF.DTMF_SYM6[2] && num4 == (int)DTMF.DTMF_SYM6[3])
				{
					c = '6';
				}
				else if (num == (int)DTMF.DTMF_SYM7[0] && num2 == (int)DTMF.DTMF_SYM7[1] && num3 == (int)DTMF.DTMF_SYM7[2] && num4 == (int)DTMF.DTMF_SYM7[3])
				{
					c = '7';
				}
				else if (num == (int)DTMF.DTMF_SYM8[0] && num2 == (int)DTMF.DTMF_SYM8[1] && num3 == (int)DTMF.DTMF_SYM8[2] && num4 == (int)DTMF.DTMF_SYM8[3])
				{
					c = '8';
				}
				else if (num == (int)DTMF.DTMF_SYM9[0] && num2 == (int)DTMF.DTMF_SYM9[1] && num3 == (int)DTMF.DTMF_SYM9[2] && num4 == (int)DTMF.DTMF_SYM9[3])
				{
					c = '9';
				}
				else if (num == (int)DTMF.DTMF_SYMA[0] && num2 == (int)DTMF.DTMF_SYMA[1] && num3 == (int)DTMF.DTMF_SYMA[2] && num4 == (int)DTMF.DTMF_SYMA[3])
				{
					c = 'A';
				}
				else if (num == (int)DTMF.DTMF_SYMB[0] && num2 == (int)DTMF.DTMF_SYMB[1] && num3 == (int)DTMF.DTMF_SYMB[2] && num4 == (int)DTMF.DTMF_SYMB[3])
				{
					c = 'B';
				}
				else if (num == (int)DTMF.DTMF_SYMC[0] && num2 == (int)DTMF.DTMF_SYMC[1] && num3 == (int)DTMF.DTMF_SYMC[2] && num4 == (int)DTMF.DTMF_SYMC[3])
				{
					c = 'C';
				}
				else if (num == (int)DTMF.DTMF_SYMD[0] && num2 == (int)DTMF.DTMF_SYMD[1] && num3 == (int)DTMF.DTMF_SYMD[2] && num4 == (int)DTMF.DTMF_SYMD[3])
				{
					c = 'D';
				}
				else if (num == (int)DTMF.DTMF_SYMS[0] && num2 == (int)DTMF.DTMF_SYMS[1] && num3 == (int)DTMF.DTMF_SYMS[2] && num4 == (int)DTMF.DTMF_SYMS[3])
				{
					c = '*';
				}
				else if (num == (int)DTMF.DTMF_SYMH[0] && num2 == (int)DTMF.DTMF_SYMH[1] && num3 == (int)DTMF.DTMF_SYMH[2] && num4 == (int)DTMF.DTMF_SYMH[3])
				{
					c = '#';
				}
				if ((int)c == DTMF.m_lastChar)
				{
					DTMF.m_pressCount++;
				}
				else
				{
					DTMF.m_lastChar = (int)c;
					DTMF.m_pressCount = 0;
				}
				if (c != ' ' && !DTMF.m_pressed && DTMF.m_pressCount >= 3)
				{
					DTMF.m_data.Append(c);
					DTMF.m_releaseCount = 0;
					DTMF.m_pressed = true;
				}
				return c != ' ';
			}
			if ((end || DTMF.m_releaseCount >= 100) && DTMF.m_data.Length > 0)
			{
				DTMF.process_DTMF(DTMF.m_data.ToString());
				DTMF.m_data.Clear();
				DTMF.m_releaseCount = 0;
			}
			DTMF.m_pressed = false;
			DTMF.m_releaseCount++;
			DTMF.m_pressCount = 0;
			DTMF.m_lastChar = 32;
			return false;
		}

		// Token: 0x0600021D RID: 541 RVA: 0x00014038 File Offset: 0x00012238
		private static string process_DTMF(string dtmfCode)
		{
			int length = dtmfCode.Trim().Length;
			char c = dtmfCode[length - 1];
			if (length == 1 && dtmfCode[0] == '#')
			{
				DSTARhandler.connectProcessing("       U");
				return "       U";
			}
			if (length < 2 || length > 6)
			{
				return "       U";
			}
			try
			{
				uint num;
				uint.TryParse(dtmfCode.Substring(1, length - 2), out num);
			}
			catch (FormatException)
			{
				return "       U";
			}
			if (dtmfCode[0] == 'D')
			{
				DSTARhandler.connectProcessing("       U");
			}
			if (dtmfCode[0] == 'D')
			{
				int num2 = 0;
				try
				{
					int.TryParse(dtmfCode.Substring(dtmfCode.Length - 2, 2), out num2);
				}
				catch
				{
					return "        ";
				}
				string text = "DCS" + dtmfCode.Substring(1, length - 3).PadLeft(3, '0') + DTMF.getCharForNumber(num2).PadRight(1) + "L";
				DSTARhandler.connectProcessing(text);
				return text;
			}
			if (dtmfCode[0] == '*')
			{
				int num3 = 0;
				try
				{
					int.TryParse(dtmfCode.Substring(dtmfCode.Length - 2, 2), out num3);
				}
				catch
				{
					return "       U";
				}
				string text2 = "REF" + dtmfCode.Substring(1, length - 3).PadLeft(3, '0') + DTMF.getCharForNumber(num3).PadRight(1) + "L";
				DSTARhandler.connectProcessing(text2);
				return text2;
			}
			if (dtmfCode[0] == 'B')
			{
				int num4 = 0;
				try
				{
					int.TryParse(dtmfCode.Substring(dtmfCode.Length - 2, 2), out num4);
				}
				catch
				{
					return "       U";
				}
				string text3 = "XRF" + dtmfCode.Substring(1, length - 3).PadLeft(3, '0') + DTMF.getCharForNumber(num4).PadRight(1) + "L";
				DSTARhandler.connectProcessing(text3);
				return text3;
			}
			return "       U";
		}

		// Token: 0x0600021E RID: 542 RVA: 0x00014234 File Offset: 0x00012434
		private static string getCharForNumber(int i)
		{
			char[] array = "XABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
			if (i > 25)
			{
				return " ";
			}
			return char.ToString(array[i]);
		}

		// Token: 0x06000220 RID: 544 RVA: 0x00014260 File Offset: 0x00012460
		// Note: this type is marked as 'beforefieldinit'.
		static DTMF()
		{
			byte[] array = new byte[4];
			array[1] = 64;
			DTMF.DTMF_SYM2 = array;
			byte[] array2 = new byte[4];
			array2[0] = 16;
			DTMF.DTMF_SYM3 = array2;
			DTMF.DTMF_SYM4 = new byte[] { 0, 0, 0, 32 };
			DTMF.DTMF_SYM5 = new byte[] { 0, 64, 0, 32 };
			DTMF.DTMF_SYM6 = new byte[] { 16, 0, 0, 32 };
			byte[] array3 = new byte[4];
			array3[2] = 8;
			DTMF.DTMF_SYM7 = array3;
			byte[] array4 = new byte[4];
			array4[1] = 64;
			array4[2] = 8;
			DTMF.DTMF_SYM8 = array4;
			byte[] array5 = new byte[4];
			array5[0] = 16;
			array5[2] = 8;
			DTMF.DTMF_SYM9 = array5;
			byte[] array6 = new byte[4];
			array6[0] = 16;
			array6[1] = 64;
			DTMF.DTMF_SYMA = array6;
			DTMF.DTMF_SYMB = new byte[] { 16, 64, 0, 32 };
			DTMF.DTMF_SYMC = new byte[] { 16, 64, 8, 0 };
			DTMF.DTMF_SYMD = new byte[] { 16, 64, 8, 32 };
			DTMF.DTMF_SYMS = new byte[] { 0, 0, 8, 32 };
			DTMF.DTMF_SYMH = new byte[] { 16, 0, 8, 32 };
			DTMF.m_data = new StringBuilder();
		}

		// Token: 0x04000121 RID: 289
		private static byte[] DTMF_MASK = new byte[] { 130, 8, 32, 130, 0, 0, 130, 0, 0 };

		// Token: 0x04000122 RID: 290
		private static byte[] DTMF_SIG = new byte[] { 130, 8, 32, 130, 0, 0, 0, 0, 0 };

		// Token: 0x04000123 RID: 291
		private static byte[] DTMF_SYM_MASK = new byte[] { 16, 64, 8, 32 };

		// Token: 0x04000124 RID: 292
		private static byte[] DTMF_SYM0 = new byte[] { 0, 64, 8, 32 };

		// Token: 0x04000125 RID: 293
		private static byte[] DTMF_SYM1 = new byte[4];

		// Token: 0x04000126 RID: 294
		private static byte[] DTMF_SYM2;

		// Token: 0x04000127 RID: 295
		private static byte[] DTMF_SYM3;

		// Token: 0x04000128 RID: 296
		private static byte[] DTMF_SYM4;

		// Token: 0x04000129 RID: 297
		private static byte[] DTMF_SYM5;

		// Token: 0x0400012A RID: 298
		private static byte[] DTMF_SYM6;

		// Token: 0x0400012B RID: 299
		private static byte[] DTMF_SYM7;

		// Token: 0x0400012C RID: 300
		private static byte[] DTMF_SYM8;

		// Token: 0x0400012D RID: 301
		private static byte[] DTMF_SYM9;

		// Token: 0x0400012E RID: 302
		private static byte[] DTMF_SYMA;

		// Token: 0x0400012F RID: 303
		private static byte[] DTMF_SYMB;

		// Token: 0x04000130 RID: 304
		private static byte[] DTMF_SYMC;

		// Token: 0x04000131 RID: 305
		private static byte[] DTMF_SYMD;

		// Token: 0x04000132 RID: 306
		private static byte[] DTMF_SYMS;

		// Token: 0x04000133 RID: 307
		private static byte[] DTMF_SYMH;

		// Token: 0x04000134 RID: 308
		private static int m_lastChar;

		// Token: 0x04000135 RID: 309
		private static int m_pressCount;

		// Token: 0x04000136 RID: 310
		private static bool m_pressed;

		// Token: 0x04000137 RID: 311
		private static int m_releaseCount;

		// Token: 0x04000138 RID: 312
		private static StringBuilder m_data;
	}
}
