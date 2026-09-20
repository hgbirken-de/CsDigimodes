using System;

namespace BlueDV
{
	// Token: 0x0200000F RID: 15
	internal class Crc24
	{
		// Token: 0x060000B1 RID: 177 RVA: 0x00006350 File Offset: 0x00004550
		public void Update(int b)
		{
			this.crc ^= b << 16;
			for (int i = 0; i < 8; i++)
			{
				this.crc <<= 1;
				if ((this.crc & 16777216) != 0)
				{
					this.crc ^= 25578747;
				}
			}
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x000063A8 File Offset: 0x000045A8
		[Obsolete("Use 'Value' property instead")]
		public int GetValue()
		{
			return this.crc;
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x000063A8 File Offset: 0x000045A8
		public int Value
		{
			get
			{
				return this.crc;
			}
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x000063B0 File Offset: 0x000045B0
		public void Reset()
		{
			this.crc = 11994318;
		}

		// Token: 0x0400004D RID: 77
		private const int Crc24Init = 11994318;

		// Token: 0x0400004E RID: 78
		private const int Crc24Poly = 25578747;

		// Token: 0x0400004F RID: 79
		private int crc = 11994318;
	}
}
