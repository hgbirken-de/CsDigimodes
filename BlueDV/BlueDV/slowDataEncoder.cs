using System;

namespace BlueDV
{
	// Token: 0x02000037 RID: 55
	internal class slowDataEncoder
	{
		// Token: 0x06000400 RID: 1024 RVA: 0x00029DCD File Offset: 0x00027FCD
		public static byte[] makeheaderData()
		{
			byte[] array = new byte[45];
			array[0] = 85;
			array[1] = 64;
			array[2] = 0;
			array[3] = 0;
			return array;
		}
	}
}
