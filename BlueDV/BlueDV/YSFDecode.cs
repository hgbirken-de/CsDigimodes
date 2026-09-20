using System;

namespace BlueDV
{
	// Token: 0x02000042 RID: 66
	internal class YSFDecode
	{
		// Token: 0x0600044D RID: 1101 RVA: 0x0002B948 File Offset: 0x00029B48
		public static int READ_BIT1(byte[] p, int i)
		{
			return (int)(p[i >> 3] & YSFDecode.BIT_MASK_TABLE[i & 7]);
		}

		// Token: 0x0600044E RID: 1102 RVA: 0x0002B95C File Offset: 0x00029B5C
		public static void decode(byte[] voice)
		{
			information.m_modus = information.MODUS.DMR;
			information.stream_modus = information.MODUS.DMR;
			int num = 0;
			for (int i = 0; i < 5; i++)
			{
				byte[] array = new byte[9];
				Buffer.BlockCopy(voice, 38 + num, array, 0, 9);
				num += 18;
				DVMEGAAMBE.makeDSTARVoiceMMDVM3000(array);
			}
		}

		// Token: 0x040002D6 RID: 726
		private static byte[] INTERLEAVE_TABLE = new byte[]
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

		// Token: 0x040002D7 RID: 727
		private static byte[] BIT_MASK_TABLE = new byte[] { 128, 64, 32, 16, 8, 4, 2, 1 };
	}
}
