using System;

namespace BlueDV
{
	// Token: 0x02000012 RID: 18
	internal class DMREmbeddedData
	{
		// Token: 0x060000E4 RID: 228 RVA: 0x00008A3C File Offset: 0x00006C3C
		public static void decodeEmbeddedData(bool[] m_raw)
		{
			bool[] array = new bool[128];
			int num = 0;
			int num2 = 0;
			while ((long)num2 < 128L)
			{
				array[num] = m_raw[num2];
				num += 16;
				if (num > 127)
				{
					num -= 127;
				}
				num2++;
			}
			for (int i = 0; i < 112; i += 16)
			{
				bool[] array2 = new bool[16];
				Buffer.BlockCopy(array, i, array2, 0, 16);
				if (!DecodeDMR.repairHamming16114(array2))
				{
					return;
				}
				Buffer.BlockCopy(array2, 0, array, i, 16);
			}
			for (int j = 0; j < 16; j++)
			{
				if (array[j] ^ array[j + 16] ^ array[j + 32] ^ array[j + 48] ^ array[j + 64] ^ array[j + 80] ^ array[j + 96] ^ array[j + 112])
				{
					return;
				}
			}
			num = 0;
			int k = 0;
			while (k < 11)
			{
				DMREmbeddedData.m_data[num] = array[k];
				k++;
				num++;
			}
			int l = 16;
			while (l < 27)
			{
				DMREmbeddedData.m_data[num] = array[l];
				l++;
				num++;
			}
			int m = 32;
			while (m < 42)
			{
				DMREmbeddedData.m_data[num] = array[m];
				m++;
				num++;
			}
			int n = 48;
			while (n < 58)
			{
				DMREmbeddedData.m_data[num] = array[n];
				n++;
				num++;
			}
			int num3 = 64;
			while ((long)num3 < 74L)
			{
				DMREmbeddedData.m_data[num] = array[num3];
				num3++;
				num++;
			}
			int num4 = 80;
			while ((long)num4 < 90L)
			{
				DMREmbeddedData.m_data[num] = array[num4];
				num4++;
				num++;
			}
			int num5 = 96;
			while ((long)num5 < 106L)
			{
				DMREmbeddedData.m_data[num] = array[num5];
				num5++;
				num++;
			}
			int num6 = 0;
			if (array[42])
			{
				num6 += 16;
			}
			if (array[58])
			{
				num6 += 8;
			}
			if (array[74])
			{
				num6 += 4;
			}
			if (array[90])
			{
				num6 += 2;
			}
			if (array[106])
			{
				num6++;
			}
		}

		// Token: 0x04000071 RID: 113
		private static bool[] m_data = new bool[72];
	}
}
