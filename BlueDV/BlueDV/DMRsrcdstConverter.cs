using System;

namespace BlueDV
{
	// Token: 0x02000018 RID: 24
	internal class DMRsrcdstConverter
	{
		// Token: 0x06000133 RID: 307 RVA: 0x0000D18C File Offset: 0x0000B38C
		public static byte[] fromDVMEGA(byte[] voice, byte type)
		{
			if (!information.DMRsimpleMode)
			{
				return voice;
			}
			information.m_groupprivate = information.m_groupprivateSelected;
			information.myAMBEDstDMRID = information.myAMBEDstDMRIDinput;
			information.AMBEDMRid = information.myDMRIDsimple;
			DVMEGAAMBE.generateBPTC();
			bool[] array = utils.bytesToBits(voice);
			if (type <= 32)
			{
				switch (type)
				{
				case 1:
				{
					for (int i = 0; i < 48; i++)
					{
						array[i + 108] = DVMEGAAMBE.fragment1[i];
					}
					byte[] array2 = new byte[33];
					Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array2, 0, 33);
					return array2;
				}
				case 2:
				{
					for (int j = 0; j < 48; j++)
					{
						array[j + 108] = DVMEGAAMBE.fragment2[j];
					}
					byte[] array3 = new byte[33];
					Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array3, 0, 33);
					return array3;
				}
				case 3:
				{
					for (int k = 0; k < 48; k++)
					{
						array[k + 108] = DVMEGAAMBE.fragment3[k];
					}
					byte[] array4 = new byte[33];
					Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array4, 0, 33);
					return array4;
				}
				case 4:
				{
					for (int l = 0; l < 48; l++)
					{
						array[l + 108] = DVMEGAAMBE.fragment4[l];
					}
					byte[] array5 = new byte[33];
					Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array5, 0, 33);
					return array5;
				}
				case 5:
				{
					for (int m = 0; m < 48; m++)
					{
						array[m + 108] = DVMEGAAMBE.fragment5[m];
					}
					byte[] array6 = new byte[33];
					Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array6, 0, 33);
					return array6;
				}
				default:
					if (type == 32)
					{
						for (int n = 0; n < 48; n++)
						{
							array[n + 108] = DVMEGAAMBE.nullfragment[n];
						}
						byte[] array7 = new byte[33];
						Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array7, 0, 33);
						return array7;
					}
					break;
				}
			}
			else
			{
				if (type == 65)
				{
					return DVMEGAAMBE.generateVoiceHeader(65);
				}
				if (type == 66)
				{
					return DVMEGAAMBE.generateVoiceHeader(66);
				}
			}
			return voice;
		}

		// Token: 0x06000134 RID: 308 RVA: 0x0000D380 File Offset: 0x0000B580
		public static byte[] fromInternetConverter2(byte[] voice, byte type)
		{
			if (!information.DMRsimpleMode)
			{
				return voice;
			}
			information.myAMBEDstDMRID = "9";
			information.AMBEDMRid = information.hisDMRID;
			information.m_groupprivate = information.GROUPPRIVATE.GROUP;
			bool[] array = utils.bytesToBits(voice);
			DVMEGAAMBE.generateBPTC();
			if (type <= 32)
			{
				switch (type)
				{
				case 1:
				{
					for (int i = 0; i < 48; i++)
					{
						array[i + 108] = DVMEGAAMBE.fragment1[i];
					}
					byte[] array2 = new byte[33];
					Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array2, 0, 33);
					return array2;
				}
				case 2:
				{
					for (int j = 0; j < 48; j++)
					{
						array[j + 108] = DVMEGAAMBE.fragment2[j];
					}
					byte[] array3 = new byte[33];
					Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array3, 0, 33);
					return array3;
				}
				case 3:
				{
					for (int k = 0; k < 48; k++)
					{
						array[k + 108] = DVMEGAAMBE.fragment3[k];
					}
					byte[] array4 = new byte[33];
					Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array4, 0, 33);
					return array4;
				}
				case 4:
				{
					for (int l = 0; l < 48; l++)
					{
						array[l + 108] = DVMEGAAMBE.fragment4[l];
					}
					byte[] array5 = new byte[33];
					Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array5, 0, 33);
					return array5;
				}
				case 5:
				{
					for (int m = 0; m < 48; m++)
					{
						array[m + 108] = DVMEGAAMBE.fragment5[m];
					}
					byte[] array6 = new byte[33];
					Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array6, 0, 33);
					return array6;
				}
				default:
					if (type == 32)
					{
						for (int n = 0; n < 48; n++)
						{
							array[n + 108] = DVMEGAAMBE.nullfragment[n];
						}
						byte[] array7 = new byte[33];
						Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array7, 0, 33);
						return array7;
					}
					break;
				}
			}
			else
			{
				if (type == 65)
				{
					return DVMEGAAMBE.generateVoiceHeader(65);
				}
				if (type == 66)
				{
					return DVMEGAAMBE.generateVoiceHeader(66);
				}
			}
			return voice;
		}

		// Token: 0x06000135 RID: 309 RVA: 0x0000D570 File Offset: 0x0000B770
		public static byte[] fromInternetConverter(byte[] voice, byte type)
		{
			if (!information.DMRsimpleMode)
			{
				return voice;
			}
			information.myAMBEDstDMRID = "9";
			information.AMBEDMRid = information.hisDMRID;
			information.m_groupprivate = information.GROUPPRIVATE.GROUP;
			bool[] array = utils.bytesToBits(voice);
			DVMEGAAMBE.generateBPTC();
			if (type == 65)
			{
				return DVMEGAAMBE.generateVoiceHeader(65);
			}
			if (type == 66)
			{
				return DVMEGAAMBE.generateVoiceHeader(66);
			}
			if (type == 1)
			{
				for (int i = 0; i < 48; i++)
				{
					array[i + 108] = DVMEGAAMBE.fragment1[i];
				}
				byte[] array2 = new byte[33];
				Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array2, 0, 33);
				return array2;
			}
			if (type == 2)
			{
				for (int j = 0; j < 48; j++)
				{
					array[j + 108] = DVMEGAAMBE.fragment2[j];
				}
				byte[] array3 = new byte[33];
				Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array3, 0, 33);
				return array3;
			}
			if (type == 3)
			{
				for (int k = 0; k < 48; k++)
				{
					array[k + 108] = DVMEGAAMBE.fragment3[k];
				}
				byte[] array4 = new byte[33];
				Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array4, 0, 33);
				return array4;
			}
			if (type == 4)
			{
				for (int l = 0; l < 48; l++)
				{
					array[l + 108] = DVMEGAAMBE.fragment4[l];
				}
				byte[] array5 = new byte[33];
				Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array5, 0, 33);
				return array5;
			}
			if (type == 5)
			{
				for (int m = 0; m < 48; m++)
				{
					array[m + 108] = DVMEGAAMBE.fragment5[m];
				}
				byte[] array6 = new byte[33];
				Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array6, 0, 33);
				return array6;
			}
			if (type == 32)
			{
				for (int n = 0; n < 48; n++)
				{
					array[n + 108] = DVMEGAAMBE.nullfragment[n];
				}
				byte[] array7 = new byte[33];
				Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(array), 0, array7, 0, 33);
				return array7;
			}
			return null;
		}
	}
}
