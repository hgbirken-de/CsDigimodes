using System;

namespace BlueDV
{
	// Token: 0x02000013 RID: 19
	internal class DMRNETincoming
	{
		// Token: 0x060000E7 RID: 231 RVA: 0x00008C40 File Offset: 0x00006E40
		public static void netIN(byte[] voice, byte sequence, byte type)
		{
			DecodeDMR.GetEMBfromVoice(voice);
			if (type <= 32)
			{
				if (type <= 16)
				{
					if (type != 4)
					{
						return;
					}
				}
				else if (type != 24)
				{
					return;
				}
			}
			else if (type <= 48)
			{
				if (type != 40)
				{
					return;
				}
			}
			else if (type != 64)
			{
			}
		}
	}
}
