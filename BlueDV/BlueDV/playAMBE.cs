using System;
using System.IO;
using System.Threading;

namespace BlueDV
{
	// Token: 0x02000047 RID: 71
	internal class playAMBE
	{
		// Token: 0x06000559 RID: 1369 RVA: 0x0002E1D4 File Offset: 0x0002C3D4
		public static bool loadAMBEfile()
		{
			playAMBE.music = playAMBE.ReadFully(File.OpenRead("en_us.ambe"));
			return true;
		}

		// Token: 0x0600055A RID: 1370 RVA: 0x00006FAC File Offset: 0x000051AC
		private static bool canStream()
		{
			if (information.stream_modus == information.MODUS.IDLE)
			{
				DVMEGASerial.setmode2DSTAR();
				information.stream_modus = information.MODUS.DSTAR;
			}
			if (information.stream_modus == information.MODUS.DSTAR)
			{
				modeTimer.resetTimer();
				return true;
			}
			return false;
		}

		// Token: 0x0600055B RID: 1371 RVA: 0x0002E1EC File Offset: 0x0002C3EC
		public static byte[] ReadFully(Stream input)
		{
			byte[] array = new byte[16384];
			byte[] array2;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				int num;
				while ((num = input.Read(array, 0, array.Length)) > 0)
				{
					memoryStream.Write(array, 0, num);
				}
				array2 = memoryStream.ToArray();
			}
			return array2;
		}

		// Token: 0x0600055C RID: 1372 RVA: 0x0002E24C File Offset: 0x0002C44C
		[STAThread]
		public static void playString(string text)
		{
			if (playAMBE.canStream())
			{
				if (playAMBE.isStreaming)
				{
					WomenQueue.Clear();
				}
				playAMBE.isStreaming = true;
				playAMBE.cancelVoice = true;
				Thread.Sleep(3000);
				playAMBE.cancelVoice = false;
				playAMBE.session_counter = 0;
				playAMBE.small_counter = 0;
				DSTARhandler.clearHeader();
				DSTARhandler.makeWomenHeader();
				DSTARhandler.makeDSTARHeaderMMDVMWomen();
				for (int i = 0; i <= text.Length - 1; i++)
				{
					if (i != 8 && text[i] != 'L')
					{
						playAMBE.lookupAMBE(text[i]);
					}
				}
				DSTARhandler.makeDSTARStopMMDVMWomen();
				playAMBE.isStreaming = false;
			}
		}

		// Token: 0x0600055D RID: 1373 RVA: 0x0002E2E8 File Offset: 0x0002C4E8
		public static void lookupAMBE(char hups)
		{
			switch (hups)
			{
			case '!':
				playAMBE.play(2637, 14);
				return;
			case '"':
			case '%':
			case '&':
			case '\'':
			case '(':
			case ')':
			case '*':
			case '+':
			case ',':
			case '-':
			case '.':
			case '/':
			case ':':
			case ';':
			case '<':
			case '=':
			case '>':
			case '?':
				break;
			case '#':
				playAMBE.play(2779, 18);
				return;
			case '$':
				playAMBE.play(2872, 30);
				break;
			case '0':
				playAMBE.play(89, 20);
				return;
			case '1':
				playAMBE.play(146, 20);
				return;
			case '2':
				playAMBE.play(202, 20);
				return;
			case '3':
				playAMBE.play(246, 20);
				return;
			case '4':
				playAMBE.play(294, 20);
				return;
			case '5':
				playAMBE.play(345, 20);
				return;
			case '6':
				playAMBE.play(409, 20);
				return;
			case '7':
				playAMBE.play(466, 20);
				return;
			case '8':
				playAMBE.play(521, 20);
				return;
			case '9':
				playAMBE.play(568, 20);
				return;
			case '@':
				playAMBE.play(2710, 25);
				return;
			case 'A':
				playAMBE.play(833, 15);
				return;
			case 'B':
				playAMBE.play(870, 17);
				return;
			case 'C':
				playAMBE.play(938, 17);
				return;
			case 'D':
				playAMBE.play(1009, 17);
				return;
			case 'E':
				playAMBE.play(1076, 17);
				return;
			case 'F':
				playAMBE.play(1148, 17);
				return;
			case 'G':
				playAMBE.play(1221, 17);
				return;
			case 'H':
				playAMBE.play(1290, 17);
				return;
			case 'I':
				playAMBE.play(1363, 17);
				return;
			case 'J':
				playAMBE.play(1432, 17);
				return;
			case 'K':
				playAMBE.play(1510, 17);
				return;
			case 'L':
				playAMBE.play(1578, 17);
				return;
			case 'M':
				playAMBE.play(1644, 17);
				return;
			case 'N':
				playAMBE.play(1715, 17);
				return;
			case 'O':
				playAMBE.play(1785, 17);
				return;
			case 'P':
				playAMBE.play(1858, 17);
				return;
			case 'Q':
				playAMBE.play(1927, 17);
				return;
			case 'R':
				playAMBE.play(1993, 17);
				return;
			case 'S':
				playAMBE.play(2063, 17);
				return;
			case 'T':
				playAMBE.play(2139, 18);
				return;
			case 'U':
				playAMBE.play(2208, 17);
				return;
			case 'V':
				playAMBE.play(2273, 17);
				return;
			case 'W':
				playAMBE.play(2344, 17);
				return;
			case 'X':
				playAMBE.play(2421, 17);
				return;
			case 'Y':
				playAMBE.play(2489, 17);
				return;
			case 'Z':
				playAMBE.play(2561, 17);
				return;
			default:
				return;
			}
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x0002E5EC File Offset: 0x0002C7EC
		public static void makeDSTARVoiceMMDVM(byte[] AMBEStream)
		{
			byte[] array = new byte[15];
			array[0] = 224;
			array[1] = 15;
			array[2] = 17;
			Buffer.BlockCopy(AMBEStream, 0, array, 3, 12);
			WomenQueue.addQueue(array);
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x0002E624 File Offset: 0x0002C824
		public static void play(int startPos2, int length1)
		{
			if (playAMBE.music == null)
			{
				playAMBE.loadAMBEfile();
			}
			for (int i = 0; i <= length1 * 2; i++)
			{
				switch (information.m_dstarmodus)
				{
				case information.DSTARMODUS.REF:
					if (DPLUSconnection.isStreaming())
					{
						return;
					}
					break;
				case information.DSTARMODUS.DCS:
					if (DCSconnection.isStreaming())
					{
						return;
					}
					break;
				case information.DSTARMODUS.XRF:
					if (DPLUSconnection.isStreaming())
					{
						return;
					}
					break;
				case information.DSTARMODUS.XLX:
					if (DCSconnection.isStreaming())
					{
						return;
					}
					break;
				case information.DSTARMODUS.JPN:
					if (DCSconnection.isStreaming())
					{
						return;
					}
					break;
				}
				if (playAMBE.cancelVoice)
				{
					WomenQueue.Clear();
					return;
				}
				byte[] array = new byte[12];
				int num = 4 + startPos2 * 9 + i * 9;
				array[0] = playAMBE.music[num];
				array[1] = playAMBE.music[num + 1];
				array[2] = playAMBE.music[num + 2];
				array[3] = playAMBE.music[num + 3];
				array[4] = playAMBE.music[num + 4];
				array[5] = playAMBE.music[num + 5];
				array[6] = playAMBE.music[num + 6];
				array[7] = playAMBE.music[num + 7];
				array[8] = playAMBE.music[num + 8];
				if (playAMBE.small_counter == 0)
				{
					array[9] = DPLUSconnection.SLOW_DATA_SYNC[0];
					array[10] = DPLUSconnection.SLOW_DATA_SYNC[1];
					array[11] = DPLUSconnection.SLOW_DATA_SYNC[2];
				}
				else
				{
					array[9] = DPLUSconnection.SLOW_DATA_NULL[0];
					array[10] = DPLUSconnection.SLOW_DATA_NULL[1];
					array[11] = DPLUSconnection.SLOW_DATA_NULL[2];
				}
				playAMBE.small_counter++;
				playAMBE.makeDSTARVoiceMMDVM(array);
				if (playAMBE.small_counter == 21)
				{
					playAMBE.small_counter = 0;
				}
			}
		}

		// Token: 0x04000376 RID: 886
		private static byte[] music;

		// Token: 0x04000377 RID: 887
		private static volatile bool cancelVoice;

		// Token: 0x04000378 RID: 888
		private static volatile int session_counter;

		// Token: 0x04000379 RID: 889
		private static volatile int small_counter;

		// Token: 0x0400037A RID: 890
		public static bool isStreaming;
	}
}
