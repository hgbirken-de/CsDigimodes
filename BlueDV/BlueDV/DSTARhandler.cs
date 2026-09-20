using System;
using System.Text;
using System.Threading;

namespace BlueDV
{
	// Token: 0x02000024 RID: 36
	internal class DSTARhandler
	{
		// Token: 0x0600020B RID: 523 RVA: 0x000133C0 File Offset: 0x000115C0
		public static bool connectProcessing(string reflector)
		{
			if (reflector[7] == 'L')
			{
				string text = reflector.Substring(0, 6);
				char c = reflector[6];
				if (downloadHostsTable.searchJPN(text) != null)
				{
					information.m_dstarmodus = information.DSTARMODUS.JPN;
				}
				else if (downloadHostsTable.searchREF(text) != null)
				{
					information.m_dstarmodus = information.DSTARMODUS.REF;
				}
				else if (downloadHostsTable.searchDCS(text) != null)
				{
					information.m_dstarmodus = information.DSTARMODUS.DCS;
				}
				else if (downloadHostsTable.searchXRF(text) != null)
				{
					information.m_dstarmodus = information.DSTARMODUS.XRF;
				}
				else if (downloadHostsTable.searchXLX(text) != null)
				{
					information.m_dstarmodus = information.DSTARMODUS.XLX;
				}
				if (DPLUSconnection.isLinked())
				{
					DPLUSconnection.unlink();
				}
				if (DCSconnection.isLinked())
				{
					DCSconnection.unlink();
				}
				switch (information.m_dstarmodus)
				{
				case information.DSTARMODUS.REF:
				{
					string text2 = downloadHostsTable.searchREF(text);
					if (text2 == null)
					{
						DPLUSconnection.unlink();
						return false;
					}
					DPLUSconnection.link(text + " " + c.ToString(), text2);
					return true;
				}
				case information.DSTARMODUS.DCS:
				{
					string text2 = downloadHostsTable.searchDCS(text);
					if (text2 == null)
					{
						DCSconnection.unlink();
						return false;
					}
					DCSconnection.link(text + " " + c.ToString(), text2);
					return true;
				}
				case information.DSTARMODUS.XRF:
				{
					string text2 = downloadHostsTable.searchXRF(text);
					if (text2 == null)
					{
						DPLUSconnection.unlink();
						return false;
					}
					DPLUSconnection.link(text + " " + c.ToString(), text2);
					return true;
				}
				case information.DSTARMODUS.XLX:
				{
					string text2 = downloadHostsTable.searchXLX(text);
					if (text2 == null)
					{
						DCSconnection.unlink();
						return false;
					}
					text = text.Replace("XLX", "DCS");
					DCSconnection.link(text + " " + c.ToString(), text2);
					return true;
				}
				case information.DSTARMODUS.JPN:
				{
					string text2 = downloadHostsTable.searchJPN(text);
					if (text2 == null)
					{
						DCSconnection.unlink();
						return false;
					}
					DCSconnection.link(text + " " + c.ToString(), text2);
					return true;
				}
				}
			}
			if (reflector[7] == 'U')
			{
				switch (information.m_dstarmodus)
				{
				case information.DSTARMODUS.REF:
					DPLUSconnection.unlink();
					break;
				case information.DSTARMODUS.DCS:
					DCSconnection.unlink();
					break;
				case information.DSTARMODUS.XRF:
					DPLUSconnection.unlink();
					break;
				case information.DSTARMODUS.XLX:
					DCSconnection.unlink();
					break;
				case information.DSTARMODUS.JPN:
					DCSconnection.unlink();
					break;
				}
			}
			return true;
		}

		// Token: 0x0600020C RID: 524 RVA: 0x000135B4 File Offset: 0x000117B4
		public static void talkWoman(string reflector)
		{
			new Thread(delegate
			{
				playAMBE.playString(reflector);
			})
			{
				Name = "PlayAMBE"
			}.Start();
		}

		// Token: 0x0600020D RID: 525 RVA: 0x000135E4 File Offset: 0x000117E4
		public static string[] processDSTARHeader(byte[] header)
		{
			DSTARhandler.increment_session_id();
			Buffer.BlockCopy(header, 6, DSTARhandler.dstarHeaderBytes, 0, 36);
			byte[] array = new byte[8];
			for (int i = 0; i <= 7; i++)
			{
				array[i] = header[i + 6];
			}
			string @string = Encoding.UTF8.GetString(array);
			byte[] array2 = new byte[8];
			for (int j = 0; j <= 7; j++)
			{
				array2[j] = header[j + 14];
			}
			string string2 = Encoding.UTF8.GetString(array2);
			byte[] array3 = new byte[8];
			for (int k = 0; k <= 7; k++)
			{
				array3[k] = header[k + 22];
			}
			string string3 = Encoding.UTF8.GetString(array3);
			byte[] array4 = new byte[8];
			for (int l = 0; l <= 7; l++)
			{
				array4[l] = header[l + 30];
			}
			string string4 = Encoding.UTF8.GetString(array4);
			byte[] array5 = new byte[4];
			for (int m = 0; m <= 3; m++)
			{
				array5[m] = header[m + 38];
			}
			string string5 = Encoding.UTF8.GetString(array5);
			string[] array6 = new string[] { @string, string2, string3, string4, string5 };
			DPLUSconnection.makeControlFrame();
			return array6;
		}

		// Token: 0x0600020E RID: 526 RVA: 0x00013718 File Offset: 0x00011918
		public static void processDSTARVoice(byte[] voice)
		{
			byte[] array = new byte[12];
			Buffer.BlockCopy(voice, 3, array, 0, 12);
			if (information.APRS)
			{
				byte[] array2 = new byte[3];
				Buffer.BlockCopy(array, 9, array2, 0, 3);
				SlowData.slowDavid(array2, false);
			}
			if (!DTMF.testme(array, false))
			{
				switch (information.m_dstarmodus)
				{
				case information.DSTARMODUS.REF:
					DPLUSconnection.makeVoiceFrame(array);
					return;
				case information.DSTARMODUS.DCS:
					DCSconnection.makeFrame(array, false);
					return;
				case information.DSTARMODUS.XRF:
					DPLUSconnection.makeVoiceFrame(array);
					return;
				case information.DSTARMODUS.XLX:
					DCSconnection.makeFrame(array, false);
					return;
				case information.DSTARMODUS.JPN:
					DCSconnection.makeFrame(array, false);
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x0600020F RID: 527 RVA: 0x000137AC File Offset: 0x000119AC
		public static void processDSTAREot(byte[] EOT)
		{
			switch (information.m_dstarmodus)
			{
			case information.DSTARMODUS.REF:
				DPLUSconnection.sendEndFrame();
				return;
			case information.DSTARMODUS.DCS:
				DCSconnection.makeFrame(new byte[]
				{
					85, 85, 85, 85, 85, 85, 85, 85, 85, 85,
					85, 85
				}, true);
				return;
			case information.DSTARMODUS.XRF:
				DPLUSconnection.sendEndFrame();
				return;
			case information.DSTARMODUS.XLX:
				DCSconnection.makeFrame(new byte[]
				{
					85, 85, 85, 85, 85, 85, 85, 85, 85, 85,
					85, 85
				}, true);
				return;
			case information.DSTARMODUS.JPN:
				DCSconnection.makeFrame(new byte[]
				{
					85, 85, 85, 85, 85, 85, 85, 85, 85, 85,
					85, 85
				}, true);
				return;
			default:
				return;
			}
		}

		// Token: 0x06000210 RID: 528 RVA: 0x00013832 File Offset: 0x00011A32
		public static void increment_session_id()
		{
			DSTARhandler.session_id_int = new Random().Next(0, 40000);
		}

		// Token: 0x06000211 RID: 529 RVA: 0x00013849 File Offset: 0x00011A49
		public static byte[] session_id_byte()
		{
			return new byte[]
			{
				(byte)(DSTARhandler.session_id_int & 255),
				(byte)((DSTARhandler.session_id_int >> 8) & 255)
			};
		}

		// Token: 0x06000212 RID: 530 RVA: 0x00013874 File Offset: 0x00011A74
		public static void makeDSTARVoiceMMDVM(byte[] AMBEStream)
		{
			byte[] array = new byte[15];
			array[0] = 224;
			array[1] = 15;
			array[2] = 17;
			Buffer.BlockCopy(AMBEStream, 0, array, 3, 12);
			FusionQueue.addQueue(array);
		}

		// Token: 0x06000213 RID: 531 RVA: 0x000138AC File Offset: 0x00011AAC
		public static void makeDSTARStopMMDVM()
		{
			information.hisCall = "        ";
			FusionQueue.addQueue(new byte[] { 224, 3, 19 });
		}

		// Token: 0x06000214 RID: 532 RVA: 0x000138D4 File Offset: 0x00011AD4
		public static void makeDSTARStopMMDVMWomen()
		{
			information.hisCall = "        ";
			WomenQueue.addQueue(new byte[] { 224, 3, 19 });
		}

		// Token: 0x06000215 RID: 533 RVA: 0x000138FC File Offset: 0x00011AFC
		public static void clearHeader()
		{
			DSTARhandler.destination = "        ";
			DSTARhandler.departure = "        ";
			DSTARhandler.companion = "        ";
			DSTARhandler.own1 = "        ";
			DSTARhandler.own2 = "    ";
		}

		// Token: 0x06000216 RID: 534 RVA: 0x00013930 File Offset: 0x00011B30
		public static void makeWomenHeader()
		{
			DSTARhandler.destination = information.myCall.PadRight(8).Substring(0, 6) + " G";
			DSTARhandler.departure = information.myCall.PadRight(8).Substring(0, 6) + " " + information.myDSTARmodule.Substring(0, 1);
			DSTARhandler.companion = "CQCQCQ  ";
			DSTARhandler.own1 = information.myCall.PadRight(8);
			DSTARhandler.own2 = "Blue";
		}

		// Token: 0x06000217 RID: 535 RVA: 0x000139B0 File Offset: 0x00011BB0
		public static byte[] makeDSTARHeader()
		{
			byte[] array = new byte[41];
			array[0] = 0;
			array[1] = 0;
			array[2] = 0;
			for (int i = 0; i <= 7; i++)
			{
				array[i + 3] = (byte)DSTARhandler.departure[i];
			}
			for (int j = 0; j <= 7; j++)
			{
				array[j + 11] = (byte)DSTARhandler.destination[j];
			}
			for (int k = 0; k <= 7; k++)
			{
				array[k + 19] = (byte)DSTARhandler.companion[k];
			}
			for (int l = 0; l <= 7; l++)
			{
				array[l + 27] = (byte)DSTARhandler.own1[l];
			}
			for (int m = 0; m <= 3; m++)
			{
				array[m + 35] = (byte)DSTARhandler.own2[m];
			}
			int num = utils.crc16DSTAR(array, array.Length - 2);
			array[39] = (byte)num;
			array[40] = (byte)(num >> 8);
			return array;
		}

		// Token: 0x06000218 RID: 536 RVA: 0x00013A94 File Offset: 0x00011C94
		public static void makeDSTARHeaderMMDVM()
		{
			byte[] array = new byte[44];
			array[0] = 224;
			array[1] = 44;
			array[2] = 16;
			Buffer.BlockCopy(DSTARhandler.makeDSTARHeader(), 0, array, 3, 41);
			FusionQueue.addQueue(array);
		}

		// Token: 0x06000219 RID: 537 RVA: 0x00013AD0 File Offset: 0x00011CD0
		public static void makeDSTARHeaderMMDVMWomen()
		{
			byte[] array = new byte[44];
			array[0] = 224;
			array[1] = 44;
			array[2] = 16;
			Buffer.BlockCopy(DSTARhandler.makeDSTARHeader(), 0, array, 3, 41);
			WomenQueue.addQueue(array);
		}

		// Token: 0x0400011A RID: 282
		public static string destination = "        ";

		// Token: 0x0400011B RID: 283
		public static string departure = "        ";

		// Token: 0x0400011C RID: 284
		public static string companion = "        ";

		// Token: 0x0400011D RID: 285
		public static string own1 = "        ";

		// Token: 0x0400011E RID: 286
		public static string own2 = "    ";

		// Token: 0x0400011F RID: 287
		public static byte[] dstarHeaderBytes = new byte[36];

		// Token: 0x04000120 RID: 288
		private static int session_id_int = 12345;
	}
}
