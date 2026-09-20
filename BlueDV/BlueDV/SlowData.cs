using System;
using System.Globalization;
using System.Text;

namespace BlueDV
{
	// Token: 0x02000036 RID: 54
	internal class SlowData
	{
		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060003F0 RID: 1008 RVA: 0x00029118 File Offset: 0x00027318
		// (set) Token: 0x060003F1 RID: 1009 RVA: 0x0002911F File Offset: 0x0002731F
		public static string StatusTextSlowData { get; private set; }

		// Token: 0x1400001C RID: 28
		// (add) Token: 0x060003F2 RID: 1010 RVA: 0x00029128 File Offset: 0x00027328
		// (remove) Token: 0x060003F3 RID: 1011 RVA: 0x0002915C File Offset: 0x0002735C
		public static event EventHandler StatusTextChangedSlowData;

		// Token: 0x060003F4 RID: 1012 RVA: 0x00029190 File Offset: 0x00027390
		private static void ChangeStatusTextSlowData(string text)
		{
			SlowData.StatusTextSlowData = text;
			EventHandler statusTextChangedSlowData = SlowData.StatusTextChangedSlowData;
			if (statusTextChangedSlowData != null)
			{
				statusTextChangedSlowData(null, EventArgs.Empty);
			}
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x000291B8 File Offset: 0x000273B8
		public static void slowDavid(byte[] slowData2, bool fromInternet)
		{
			SlowData.fromInternetGlobal = fromInternet;
			if (slowData2[0] == 0x55 && slowData2[1] == 0x55 && slowData2[2] == 0x55)
			{
				SlowData.total_counter = 0;
				SlowData.textDataCounter = 0;
				SlowData.gpsDataCounter = 0;
				SlowData.headerDataCounter = 0;
				SlowData.firstGPS = false;
				SlowData.firstText = false;
			}
			if (slowData2[0] == 0x55 && slowData2[1] == 2D && slowData2[2] == 0x16)
			{	
				// SYNC aquisition
				SlowData.total_counter = 0;
			}
			if (SlowData.total_counter == 1 || SlowData.total_counter == 3 || SlowData.total_counter == 5 || SlowData.total_counter == 7 || SlowData.total_counter == 9 || SlowData.total_counter == 11 || SlowData.total_counter == 13 || SlowData.total_counter == 15 || SlowData.total_counter == 17 || SlowData.total_counter == 19 || SlowData.total_counter == 21 || SlowData.total_counter == 23 || SlowData.total_counter == 25 || SlowData.total_counter == 27)
			{
				byte b = slowData2[0];
				if (b <= 37)
				{
					if (b != 22)
					{
						switch (b)
						{
						case 33:
							SlowData.headerData(slowData2, true);
							SlowData.firstHeader = true;
							break;
						case 34:
							SlowData.headerData(slowData2, true);
							SlowData.firstHeader = true;
							break;
						case 35:
							SlowData.headerData(slowData2, true);
							SlowData.firstHeader = true;
							break;
						case 36:
							SlowData.headerData(slowData2, true);
							SlowData.firstHeader = true;
							break;
						case 37:
							SlowData.headerData(slowData2, true);
							SlowData.firstHeader = true;
							break;
						}
					}
				}
				else
				{
					switch (b)
					{
					case 48:
						SlowData.textDataCounter = 0;
						SlowData.textData(slowData2, true);
						SlowData.firstText = true;
						SlowData.found30 = true;
						break;
					case 49:
						SlowData.textData(slowData2, true);
						SlowData.firstText = true;
						SlowData.found31 = true;
						break;
					case 50:
						SlowData.textData(slowData2, true);
						SlowData.firstText = true;
						SlowData.found32 = true;
						break;
					case 51:
						SlowData.textData(slowData2, true);
						SlowData.firstText = true;
						SlowData.found33 = true;
						break;
					default:
						switch (b)
						{
						case 65:
							SlowData.endGPS = true;
							SlowData.gpsData(slowData2, true);
							SlowData.firstGPS = true;
							break;
						case 66:
							SlowData.endGPS = true;
							SlowData.gpsData(slowData2, true);
							SlowData.firstGPS = true;
							break;
						case 67:
							SlowData.endGPS = true;
							SlowData.gpsData(slowData2, true);
							SlowData.firstGPS = true;
							break;
						case 68:
							SlowData.endGPS = true;
							SlowData.gpsData(slowData2, true);
							SlowData.firstGPS = true;
							break;
						case 69:
							SlowData.gpsData(slowData2, true);
							SlowData.firstGPS = true;
							SlowData.endGPS = false;
							break;
						}
						break;
					}
				}
			}
			else
			{
				if (SlowData.firstText)
				{
					SlowData.textData(slowData2, false);
					SlowData.firstText = false;
				}
				if (SlowData.firstGPS)
				{
					SlowData.gpsData(slowData2, false);
					SlowData.firstGPS = false;
				}
				if (SlowData.firstHeader)
				{
					SlowData.headerData(slowData2, false);
					SlowData.firstHeader = false;
				}
			}
			SlowData.total_counter++;
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x0002947C File Offset: 0x0002767C
		public static void gpsData(byte[] slowData4, bool firstGPSpacket)
		{
			if ((slowData4[0] & 255) == 69 && (slowData4[1] & 255) == 107 && (slowData4[2] & 255) == 183 && SlowData.gpsDataCounter > 50)
			{
				string text = Encoding.ASCII.GetString(SlowData.gpsDataValue).Substring(10, SlowData.gpsDataCounter - 10).Replace("f", "");
				int num = utils.calcCCITTCRC(Encoding.ASCII.GetBytes(text), 0, text.Length);
				string text2 = Encoding.ASCII.GetString(SlowData.gpsDataValue).Substring(5, 4).ToUpper();
				int num2;
				try
				{
					num2 = int.Parse(text2, NumberStyles.HexNumber);
				}
				catch (Exception)
				{
					num2 = 0;
				}
				if (num2 == num)
				{
					APRSClient.sendAPRS(Encoding.Default.GetString(SlowData.gpsDataValue).Substring(10, SlowData.gpsDataCounter) + "\n");
				}
				SlowData.gpsDataValue = new byte[100];
				SlowData.gpsDataCounter = 0;
			}
			if (!firstGPSpacket)
			{
				SlowData.gpsDataValue[SlowData.gpsDataCounter] = (byte.MaxValue & slowData4[0]) ^ 112;
				SlowData.gpsDataCounter++;
			}
			SlowData.gpsDataValue[SlowData.gpsDataCounter] = (byte.MaxValue & slowData4[1]) ^ 79;
			SlowData.gpsDataCounter++;
			SlowData.gpsDataValue[SlowData.gpsDataCounter] = (byte.MaxValue & slowData4[2]) ^ 147;
			SlowData.gpsDataCounter++;
			if (SlowData.gpsDataCounter >= 97)
			{
				SlowData.gpsDataCounter = 0;
				SlowData.gpsDataValue = new byte[100];
			}
		}

		// Token: 0x060003F7 RID: 1015 RVA: 0x0002961C File Offset: 0x0002781C
		public static void textData(byte[] slowData3, bool firstPacket)
		{
			if (slowData3.Length == 0)
			{
				return;
			}
			if (!firstPacket)
			{
				SlowData.freeText2[SlowData.textDataCounter] = (byte.MaxValue & slowData3[0]) ^ 112;
				SlowData.textDataCounter++;
			}
			SlowData.freeText2[SlowData.textDataCounter] = (byte.MaxValue & slowData3[1]) ^ 79;
			SlowData.textDataCounter++;
			SlowData.freeText2[SlowData.textDataCounter] = (byte.MaxValue & slowData3[2]) ^ 147;
			SlowData.textDataCounter++;
			if (SlowData.textDataCounter > 19 && SlowData.found30 && SlowData.found31 && SlowData.found32 && SlowData.found33)
			{
				if (SlowData.isAllASCII(Encoding.ASCII.GetString(SlowData.freeText2)))
				{
					SlowData.ChangeStatusTextSlowData(Encoding.ASCII.GetString(SlowData.freeText2));
				}
				SlowData.found30 = false;
				SlowData.found31 = false;
				SlowData.found32 = false;
				SlowData.found33 = false;
			}
			if (SlowData.textDataCounter >= 20)
			{
				SlowData.textDataCounter = 0;
			}
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x00029718 File Offset: 0x00027918
		private static bool isAllASCII(string input)
		{
			bool flag = true;
			for (int i = 0; i < input.Length; i++)
			{
				if (input[i] > '\u007f')
				{
					flag = false;
					break;
				}
			}
			return flag;
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x00002F42 File Offset: 0x00001142
		public static void headerData(byte[] slowData3, bool firstPacket)
		{
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x00029748 File Offset: 0x00027948
		public static void make_free_text(string textinput)
		{
			string text = textinput.PadRight(20);
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			SlowData.m_text = new byte[24];
			SlowData.m_text[0] = 64;
			SlowData.m_text[1] = bytes[0];
			SlowData.m_text[2] = bytes[1];
			SlowData.m_text[3] = bytes[2];
			SlowData.m_text[4] = bytes[3];
			SlowData.m_text[5] = bytes[4];
			SlowData.m_text[6] = 65;
			SlowData.m_text[7] = bytes[5];
			SlowData.m_text[8] = bytes[6];
			SlowData.m_text[9] = bytes[7];
			SlowData.m_text[10] = bytes[8];
			SlowData.m_text[11] = bytes[9];
			SlowData.m_text[12] = 66;
			SlowData.m_text[13] = bytes[10];
			SlowData.m_text[14] = bytes[11];
			SlowData.m_text[15] = bytes[12];
			SlowData.m_text[16] = bytes[13];
			SlowData.m_text[17] = bytes[14];
			SlowData.m_text[18] = 67;
			SlowData.m_text[19] = bytes[15];
			SlowData.m_text[20] = bytes[16];
			SlowData.m_text[21] = bytes[17];
			SlowData.m_text[22] = bytes[18];
			SlowData.m_text[23] = bytes[19];
			SlowData.m_textPoiner = 0;
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x00029884 File Offset: 0x00027A84
		public static byte[] makeSlowDataFromtext()
		{
			byte[] array = new byte[3];
			if (SlowData.m_textPoiner < 24)
			{
				array[0] = SlowData.m_text[SlowData.m_textPoiner++] ^ 112;
				array[1] = SlowData.m_text[SlowData.m_textPoiner++] ^ 79;
				array[2] = SlowData.m_text[SlowData.m_textPoiner++] ^ 147;
			}
			else
			{
				array[0] = 22;
				array[1] = 41;
				array[2] = 245;
			}
			return array;
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00029908 File Offset: 0x00027B08
		public static void makeSlowDataHeader(string rpt1, string rpt2, string your, string my, string myShort)
		{
			byte[] array = new byte[5];
			byte[] array2 = new byte[5];
			byte[] array3 = new byte[5];
			byte[] array4 = new byte[5];
			byte[] array5 = new byte[5];
			byte[] array6 = new byte[5];
			byte[] array7 = new byte[5];
			byte[] array8 = new byte[5];
			array[0] = 64;
			array[1] = 0;
			array[2] = 0;
			array[3] = (byte)rpt1[0];
			array[4] = (byte)rpt1[1];
			array2[0] = (byte)rpt1[2];
			array2[1] = (byte)rpt1[3];
			array2[2] = (byte)rpt1[4];
			array2[3] = (byte)rpt1[5];
			array2[4] = (byte)rpt1[6];
			array3[0] = (byte)rpt1[7];
			array3[1] = (byte)rpt2[0];
			array3[2] = (byte)rpt2[1];
			array3[3] = (byte)rpt2[2];
			array3[4] = (byte)rpt2[3];
			array4[0] = (byte)rpt2[4];
			array4[1] = (byte)rpt2[5];
			array4[2] = (byte)rpt2[6];
			array4[3] = (byte)rpt2[7];
			array4[4] = (byte)your[0];
			array5[0] = (byte)your[1];
			array5[1] = (byte)your[2];
			array5[2] = (byte)your[3];
			array5[3] = (byte)your[4];
			array5[4] = (byte)your[5];
			array6[0] = (byte)your[6];
			array6[1] = (byte)your[7];
			array6[2] = (byte)my[0];
			array6[3] = (byte)my[1];
			array6[4] = (byte)my[2];
			array7[0] = (byte)my[3];
			array7[1] = (byte)my[4];
			array7[2] = (byte)my[5];
			array7[3] = (byte)my[6];
			array7[4] = (byte)my[7];
			array8[0] = (byte)myShort[0];
			array8[1] = (byte)myShort[1];
			array8[2] = (byte)myShort[2];
			array8[3] = (byte)myShort[3];
			byte[] array9 = new byte[39];
			Buffer.BlockCopy(array, 0, array9, 0, 5);
			Buffer.BlockCopy(array2, 0, array9, 5, 5);
			Buffer.BlockCopy(array3, 0, array9, 10, 5);
			Buffer.BlockCopy(array4, 0, array9, 15, 5);
			Buffer.BlockCopy(array5, 0, array9, 20, 5);
			Buffer.BlockCopy(array6, 0, array9, 25, 5);
			Buffer.BlockCopy(array7, 0, array9, 30, 5);
			Buffer.BlockCopy(array8, 0, array9, 35, 4);
			int num = crc.crcX25(array9);
			Buffer.BlockCopy(array9, 0, SlowData.CheckedHeader, 0, 39);
			SlowData.CheckedHeader[39] = (byte)num;
			SlowData.CheckedHeader[40] = (byte)(num >> 8);
			SlowData.CheckedHeader[41] = 102;
			SlowData.totalNow[0] = 85;
			Buffer.BlockCopy(SlowData.CheckedHeader, 0, SlowData.totalNow, 1, 5);
			SlowData.totalNow[6] = 85;
			Buffer.BlockCopy(SlowData.CheckedHeader, 5, SlowData.totalNow, 7, 5);
			SlowData.totalNow[12] = 85;
			Buffer.BlockCopy(SlowData.CheckedHeader, 10, SlowData.totalNow, 13, 5);
			SlowData.totalNow[18] = 85;
			Buffer.BlockCopy(SlowData.CheckedHeader, 15, SlowData.totalNow, 19, 5);
			SlowData.totalNow[24] = 85;
			Buffer.BlockCopy(SlowData.CheckedHeader, 20, SlowData.totalNow, 25, 5);
			SlowData.totalNow[30] = 85;
			Buffer.BlockCopy(SlowData.CheckedHeader, 25, SlowData.totalNow, 31, 5);
			SlowData.totalNow[36] = 85;
			Buffer.BlockCopy(SlowData.CheckedHeader, 30, SlowData.totalNow, 37, 5);
			SlowData.totalNow[42] = 85;
			Buffer.BlockCopy(SlowData.CheckedHeader, 35, SlowData.totalNow, 43, 5);
			SlowData.totalNow[48] = 81;
			Buffer.BlockCopy(SlowData.CheckedHeader, 40, SlowData.totalNow, 49, 1);
			SlowData.m_textPoinerHeader = 0;
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x00029CB8 File Offset: 0x00027EB8
		public static byte[] makeSlowDataFromHeader()
		{
			byte[] array = new byte[3];
			if (SlowData.m_textPoinerHeader < 41)
			{
				array[0] = SlowData.totalNow[SlowData.m_textPoinerHeader++] ^ 112;
				array[1] = SlowData.totalNow[SlowData.m_textPoinerHeader++] ^ 79;
				array[2] = SlowData.totalNow[SlowData.m_textPoinerHeader++] ^ 147;
			}
			else
			{
				array[0] = 22;
				array[1] = 41;
				array[2] = 245;
			}
			return array;
		}

		// Token: 0x04000291 RID: 657
		private static byte[] gpsDataValue = new byte[100];

		// Token: 0x04000292 RID: 658
		private static byte[] CheckedHeader = new byte[42];

		// Token: 0x04000293 RID: 659
		private static byte[] totalNow = new byte[51];

		// Token: 0x04000294 RID: 660
		private static int m_textPoiner = 0;

		// Token: 0x04000295 RID: 661
		private static int m_textPoinerHeader = 0;

		// Token: 0x04000296 RID: 662
		private static bool firstText = false;

		// Token: 0x04000297 RID: 663
		private static bool firstGPS = false;

		// Token: 0x04000298 RID: 664
		private static bool firstHeader = false;

		// Token: 0x04000299 RID: 665
		private static bool endGPS = false;

		// Token: 0x0400029A RID: 666
		private static bool found30 = false;

		// Token: 0x0400029B RID: 667
		private static bool found31 = false;

		// Token: 0x0400029C RID: 668
		private static bool found32 = false;

		// Token: 0x0400029D RID: 669
		private static bool found33 = false;

		// Token: 0x0400029E RID: 670
		private static int total_counter = 0;

		// Token: 0x0400029F RID: 671
		private static int textDataCounter = 0;

		// Token: 0x040002A0 RID: 672
		private static int gpsDataCounter = 0;

		// Token: 0x040002A1 RID: 673
		private static int headerDataCounter = 0;

		// Token: 0x040002A2 RID: 674
		public static bool fromInternetGlobal;

		// Token: 0x040002A3 RID: 675
		private static byte[] m_text;

		// Token: 0x040002A4 RID: 676
		private static byte[] freeText2 = new byte[20];
	}
}
