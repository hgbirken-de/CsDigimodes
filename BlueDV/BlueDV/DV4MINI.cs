using System;
using System.Threading;

namespace BlueDV
{
	// Token: 0x02000019 RID: 25
	internal class DV4MINI
	{
		// Token: 0x06000137 RID: 311 RVA: 0x0000D744 File Offset: 0x0000B944
		public static void decode(byte data)
		{
			DV4MINI.serial3_bufferP[DV4MINI.serial3_buffer_input_pointerP] = data;
			DV4MINI.serial3_buffer_input_pointerP++;
			if (DV4MINI.serial3_buffer_input_pointerP > DV4MINI.ser3_in_buf_lenP)
			{
				DV4MINI.serial3_buffer_input_pointerP = 0;
			}
			if (DV4MINI.serial3_bufferP[0] != 113)
			{
				DV4MINI.serial3_buffer_input_pointerP = 0;
			}
			if (DV4MINI.serial3_bufferP[0] == 113 && DV4MINI.serial3_buffer_input_pointerP >= 3)
			{
				int num = ((int)DV4MINI.serial3_bufferP[1] << 8) | (int)DV4MINI.serial3_bufferP[2];
				if (DV4MINI.serial3_buffer_input_pointerP >= num + 4 && DV4MINI.serial3_buffer_input_pointerP >= 2)
				{
					int num2 = ((int)DV4MINI.serial3_bufferP[1] << 8) | (int)DV4MINI.serial3_bufferP[2];
					byte[] array = new byte[num2 + 4];
					Buffer.BlockCopy(DV4MINI.serial3_bufferP, 0, array, 0, num2 + 4);
					byte b = array[3];
				}
			}
		}

		// Token: 0x06000138 RID: 312 RVA: 0x0000D7F4 File Offset: 0x0000B9F4
		public static void dv4miniconfigFusion()
		{
			byte[] array = new byte[]
			{
				113, 254, 57, 29, 1, 8, 25, 252, 210, 0,
				25, 252, 210, 0
			};
			byte[] array2 = new byte[] { 113, 254, 57, 29, 2, 1, 70 };
			DVMEGASerial.write2(array);
			Thread.Sleep(150);
			DVMEGASerial.write2(array2);
		}

		// Token: 0x06000139 RID: 313 RVA: 0x0000D83A File Offset: 0x0000BA3A
		public static void dv4miniGetData()
		{
			DVMEGASerial.write2(new byte[] { 113, 254, 57, 29, 7, 0 });
		}

		// Token: 0x0600013A RID: 314 RVA: 0x0000D852 File Offset: 0x0000BA52
		public static void dv4miniGetVersion()
		{
			DVMEGASerial.write2(new byte[] { 113, 254, 57, 29, 24, 0 });
		}

		// Token: 0x0600013B RID: 315 RVA: 0x0000D86A File Offset: 0x0000BA6A
		public static void dv4miniWatchDog()
		{
			DVMEGASerial.write2(new byte[] { 113, 254, 57, 29, 5, 0 });
		}

		// Token: 0x040000A7 RID: 167
		private static int ser3_in_buf_lenP = 330;

		// Token: 0x040000A8 RID: 168
		private static byte[] serial3_bufferP = new byte[DV4MINI.ser3_in_buf_lenP + 1];

		// Token: 0x040000A9 RID: 169
		private static int serial3_buffer_input_pointerP = 0;
	}
}
