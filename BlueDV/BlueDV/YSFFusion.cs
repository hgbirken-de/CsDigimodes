using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace BlueDV
{
	// Token: 0x02000043 RID: 67
	internal class YSFFusion
	{
		// Token: 0x1700003E RID: 62
		// (get) Token: 0x06000451 RID: 1105 RVA: 0x0002B9D3 File Offset: 0x00029BD3
		// (set) Token: 0x06000452 RID: 1106 RVA: 0x0002B9DA File Offset: 0x00029BDA
		public static string StatusTextFUSION { get; private set; }

		// Token: 0x1400001E RID: 30
		// (add) Token: 0x06000453 RID: 1107 RVA: 0x0002B9E4 File Offset: 0x00029BE4
		// (remove) Token: 0x06000454 RID: 1108 RVA: 0x0002BA18 File Offset: 0x00029C18
		public static event EventHandler StatusTextChangedFUSION;

		// Token: 0x06000456 RID: 1110 RVA: 0x000162DC File Offset: 0x000144DC
		private static bool canStream()
		{
			if (information.stream_modus == information.MODUS.IDLE)
			{
				DVMEGASerial.setmode2Fusion();
				information.stream_modus = information.MODUS.FUSION;
			}
			if (information.stream_modus == information.MODUS.FUSION)
			{
				modeTimer.resetTimer();
				return true;
			}
			return false;
		}

		// Token: 0x06000457 RID: 1111 RVA: 0x0002BA4C File Offset: 0x00029C4C
		public static void open()
		{
			YSFFusion.m_status = YSFFusion.STATUS.W_LOGIN;
			YSFFusion.writeGetInfo();
			TimerCallback timerCallback;
			if ((timerCallback = YSFFusion.<>O.<0>__writePing) == null)
			{
				timerCallback = (YSFFusion.<>O.<0>__writePing = new TimerCallback(YSFFusion.writePing));
			}
			YSFFusion._timer = new Timer(timerCallback, null, YSFFusion.TIME_INTERVAL_IN_MILLISECONDS, -1);
			TimerCallback timerCallback2;
			if ((timerCallback2 = YSFFusion.<>O.<1>__pongCheck) == null)
			{
				timerCallback2 = (YSFFusion.<>O.<1>__pongCheck = new TimerCallback(YSFFusion.pongCheck));
			}
			YSFFusion._timer_pongcheck = new Timer(timerCallback2, null, YSFFusion.TIME_INTERVAL_IN_MILLISECONDS_PONG, -1);
		}

		// Token: 0x06000458 RID: 1112 RVA: 0x0002BABC File Offset: 0x00029CBC
		private static void pongCheck(object state)
		{
			if (DateTime.Now.Ticks / 10000L - YSFFusion.last_pong > (long)YSFFusion.TIME_INTERVAL_IN_MILLISECONDS_PONG)
			{
				YSFFusion.close();
				YSFFusion.reconnect();
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					YSFFusion.ChangeStatusTextFUSION("Reconnecting ");
					break;
				case information.LANGUAGE.JAPANESE:
					YSFFusion.ChangeStatusTextFUSION("再接続する");
					break;
				case information.LANGUAGE.CHINEES:
					YSFFusion.ChangeStatusTextFUSION("Reconnecting ");
					break;
				case information.LANGUAGE.KOREAN:
					YSFFusion.ChangeStatusTextFUSION("재연결 ");
					break;
				}
			}
			else
			{
				YSFFusion.writeGetInfo();
			}
			try
			{
				if (YSFFusion._timer_pongcheck != null)
				{
					YSFFusion._timer_pongcheck.Change(YSFFusion.TIME_INTERVAL_IN_MILLISECONDS_PONG, -1);
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000459 RID: 1113 RVA: 0x0002BB78 File Offset: 0x00029D78
		public static void connect(string hostname, int port)
		{
			YSFFusion.HOSTNAME = hostname;
			YSFFusion.PORT = port;
			try
			{
				YSFFusion.udpClient = new UdpClient();
			}
			catch (SocketException)
			{
				return;
			}
			try
			{
				YSFFusion.udpClient.Connect(YSFFusion.HOSTNAME, YSFFusion.PORT);
				UdpClient udpClient = YSFFusion.udpClient;
				AsyncCallback asyncCallback;
				if ((asyncCallback = YSFFusion.<>O.<2>__receiveData) == null)
				{
					asyncCallback = (YSFFusion.<>O.<2>__receiveData = new AsyncCallback(YSFFusion.receiveData));
				}
				udpClient.BeginReceive(asyncCallback, YSFFusion.udpClient);
				YSFFusion.open();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					YSFFusion.ChangeStatusTextFUSION("ERR: " + ex.Message);
					break;
				case information.LANGUAGE.JAPANESE:
					YSFFusion.ChangeStatusTextFUSION("エラー： " + ex.Message);
					break;
				case information.LANGUAGE.CHINEES:
					YSFFusion.ChangeStatusTextFUSION("ERR: " + ex.Message);
					break;
				case information.LANGUAGE.KOREAN:
					YSFFusion.ChangeStatusTextFUSION("ERR: " + ex.Message);
					break;
				}
			}
		}

		// Token: 0x0600045A RID: 1114 RVA: 0x0002BC98 File Offset: 0x00029E98
		public static void reconnect()
		{
			try
			{
				YSFFusion.udpClient = new UdpClient();
			}
			catch (SocketException)
			{
				return;
			}
			try
			{
				YSFFusion.udpClient.Connect(YSFFusion.HOSTNAME, YSFFusion.PORT);
				UdpClient udpClient = YSFFusion.udpClient;
				AsyncCallback asyncCallback;
				if ((asyncCallback = YSFFusion.<>O.<2>__receiveData) == null)
				{
					asyncCallback = (YSFFusion.<>O.<2>__receiveData = new AsyncCallback(YSFFusion.receiveData));
				}
				udpClient.BeginReceive(asyncCallback, YSFFusion.udpClient);
				YSFFusion.writeLogout();
				YSFFusion.open();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					YSFFusion.ChangeStatusTextFUSION("ERR: " + ex.Message);
					break;
				case information.LANGUAGE.JAPANESE:
					YSFFusion.ChangeStatusTextFUSION("エラー： " + ex.Message);
					break;
				case information.LANGUAGE.CHINEES:
					YSFFusion.ChangeStatusTextFUSION("ERR: " + ex.Message);
					break;
				case information.LANGUAGE.KOREAN:
					YSFFusion.ChangeStatusTextFUSION("ERR: " + ex.Message);
					break;
				}
			}
		}

		// Token: 0x0600045B RID: 1115 RVA: 0x0002BDB0 File Offset: 0x00029FB0
		private static void ChangeStatusTextFUSION(string text)
		{
			YSFFusion.StatusTextFUSION = text;
			EventHandler statusTextChangedFUSION = YSFFusion.StatusTextChangedFUSION;
			if (statusTextChangedFUSION != null)
			{
				statusTextChangedFUSION(null, EventArgs.Empty);
			}
		}

		// Token: 0x0600045C RID: 1116 RVA: 0x0002BDD8 File Offset: 0x00029FD8
		public static void makeFrame(byte[] voice)
		{
			byte[] array = new byte[155];
			string text = "YSFD";
			text += information.myCall.PadRight(10);
			text += information.myCall.PadRight(10);
			text += "ALL".PadRight(10);
			Buffer.BlockCopy(Encoding.ASCII.GetBytes(text), 0, array, 0, 34);
			voice[0] = (byte)((YSFFusion.fichCounter & 127) << 1);
			YSFFusion.fichCounter += 1;
			Buffer.BlockCopy(voice, 0, array, 34, 121);
			YSFFusion.sendRAW(array, 155);
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x0002BE78 File Offset: 0x0002A078
		public static void close()
		{
			if (YSFFusion.udpClient != null)
			{
				try
				{
					YSFFusion.writeLogout();
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						YSFFusion.ChangeStatusTextFUSION("Not linked");
						break;
					case information.LANGUAGE.JAPANESE:
						YSFFusion.ChangeStatusTextFUSION("未接続");
						break;
					case information.LANGUAGE.CHINEES:
						YSFFusion.ChangeStatusTextFUSION("Not linked");
						break;
					case information.LANGUAGE.KOREAN:
						YSFFusion.ChangeStatusTextFUSION("연결안됨");
						break;
					}
					YSFFusion._timer.Dispose();
					YSFFusion._timer_pongcheck.Dispose();
					YSFFusion.udpClient.Client.Shutdown(SocketShutdown.Both);
					YSFFusion.udpClient.Close();
					UdpClient udpClient = YSFFusion.udpClient;
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x0002BF2C File Offset: 0x0002A12C
		private static void receiveData(IAsyncResult ar)
		{
			try
			{
				UdpClient udpClient = (UdpClient)ar.AsyncState;
				IPEndPoint ipendPoint = new IPEndPoint(IPAddress.Any, 0);
				byte[] array = udpClient.EndReceive(ar, ref ipendPoint);
				if (array.Length != 0)
				{
					string @string = Encoding.ASCII.GetString(array);
					int num = array.Length;
					@string.StartsWith("YSFQ");
					if (@string.StartsWith("YSFP"))
					{
						YSFFusion.last_pong = DateTime.Now.Ticks / 10000L;
						YSFFusion.writeYSFI();
						YSFFusion.writeYSFO();
					}
					if (@string.StartsWith("YSFD"))
					{
						if (YSFFusion.canStream())
						{
							information.hisCallsmall = " ";
							information.hisCall = @string.Substring(14, 8).Split(new char[] { '-' })[0].Split(new char[] { '/' })[0].Split(new char[] { ' ' })[0];
							byte[] array2 = new byte[120];
							Buffer.BlockCopy(array, 35, array2, 0, 120);
							array2[0] = 212;
							array2[1] = 113;
							array2[2] = 201;
							array2[3] = 99;
							array2[4] = 77;
							YSFFusion.fromFUSION(array2, array[34]);
							TimerRXTX.TX();
						}
						ShowCallTimer.resetTimerFUSION(@string.Substring(14, 8));
					}
					if (@string.StartsWith("YSFS"))
					{
						switch (information.m_language)
						{
						case information.LANGUAGE.ENGLISH:
							YSFFusion.ChangeStatusTextFUSION("Linked to " + @string.Substring(9, 15));
							break;
						case information.LANGUAGE.JAPANESE:
							YSFFusion.ChangeStatusTextFUSION("接続中 " + @string.Substring(9, 15));
							break;
						case information.LANGUAGE.CHINEES:
							YSFFusion.ChangeStatusTextFUSION("Linked to " + @string.Substring(9, 15));
							break;
						case information.LANGUAGE.KOREAN:
							YSFFusion.ChangeStatusTextFUSION("연결됨 " + @string.Substring(9, 15));
							break;
						}
					}
				}
				udpClient.BeginReceive(new AsyncCallback(YSFFusion.receiveData), ar.AsyncState);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x0600045F RID: 1119 RVA: 0x0002C160 File Offset: 0x0002A360
		private static void writeGetInfo()
		{
			YSFFusion.sendRAW(Encoding.ASCII.GetBytes("YSFS"), 4);
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x0002C177 File Offset: 0x0002A377
		public static void getRoom()
		{
			YSFFusion.sendRAW(Encoding.ASCII.GetBytes("YSFQ"), 4);
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x0002C190 File Offset: 0x0002A390
		private static void fromFUSION(byte[] voice, byte counter)
		{
			byte[] array = new byte[124];
			array[0] = 224;
			array[1] = 124;
			array[2] = 32;
			array[3] = counter;
			Buffer.BlockCopy(voice, 0, array, 4, 120);
			FusionQueue.addQueue(array);
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x0002C1CC File Offset: 0x0002A3CC
		private static void writePing(object o)
		{
			string text = "YSFP" + information.myCall.PadRight(10);
			YSFFusion.sendRAW(Encoding.ASCII.GetBytes(text), 14);
			if (YSFFusion._timer != null)
			{
				YSFFusion._timer.Change(YSFFusion.TIME_INTERVAL_IN_MILLISECONDS, -1);
			}
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x0002C21C File Offset: 0x0002A41C
		private static void writeYSFO()
		{
			try
			{
				string text = "YSFO" + information.myCall.PadRight(10) + information.YSFOoptions.PadRight(36);
				YSFFusion.sendRAW(Encoding.ASCII.GetBytes(text), text.Length);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x0002C278 File Offset: 0x0002A478
		private static void writeYSFI()
		{
			try
			{
				string text = string.Concat(new string[]
				{
					"YSFI",
					information.myCall.PadRight(10),
					information.myFREQ.PadRight(9),
					information.myFREQ.PadRight(9),
					information.myQTHlocation.PadRight(6),
					information.DSTARslowDataText.PadRight(20).Substring(0, 20),
					"BlueDV".PadRight(12),
					information.myDMRID.PadRight(10)
				});
				YSFFusion.sendRAW(Encoding.ASCII.GetBytes(text), 80);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x0002C330 File Offset: 0x0002A530
		private static void writeLogout()
		{
			string text = "YSFU";
			YSFFusion.sendRAW(Encoding.ASCII.GetBytes(text), 4);
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x0002C354 File Offset: 0x0002A554
		public static void sendRAW(byte[] data, int length)
		{
			try
			{
				YSFFusion.udpClient.Send(data, length);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x0002C384 File Offset: 0x0002A584
		private static byte[] GetBytes(string str)
		{
			byte[] array = new byte[str.Length * 2];
			Buffer.BlockCopy(str.ToCharArray(), 0, array, 0, array.Length);
			return array;
		}

		// Token: 0x06000468 RID: 1128 RVA: 0x0002C3B4 File Offset: 0x0002A5B4
		private static string GetString(byte[] bytes)
		{
			char[] array = new char[bytes.Length / 2];
			Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
			return new string(array);
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x0002C3E0 File Offset: 0x0002A5E0
		public static void decode_call(byte[] voice)
		{
			byte[] array = new byte[120];
			Buffer.BlockCopy(voice, 1, array, 0, 120);
			if (fusion_extract.decode_FICH(array))
			{
				switch (fusion_extract.getFI())
				{
				case 0:
				{
					bool flag = fusion_extract.get_fusion_headerFirstBlock(array);
					YSFFusion.fichCounter = 1;
					YSFFusion.makeFrame(voice);
					YSFFusion.seenFusionHeader = true;
					return;
				}
				case 1:
					if (fusion_extract.getDT() == 2)
					{
						fusion_extract.get_Fusion_VD2_data(array, fusion_extract.getFN());
					}
					if (fusion_extract.getFN() == 1 && fusion_extract.getDT() == 2)
					{
						fusion_extract.get_Fusion_VD2_data(array, fusion_extract.getFN());
					}
					YSFFusion.makeFrame(voice);
					return;
				case 2:
					YSFFusion.makeFrame(voice);
					YSFFusion.seenFusionHeader = true;
					break;
				case 3:
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x040002D8 RID: 728
		private static UdpClient udpClient;

		// Token: 0x040002D9 RID: 729
		private static Timer _timer;

		// Token: 0x040002DA RID: 730
		private static Timer _timer_pongcheck;

		// Token: 0x040002DB RID: 731
		private static int TIME_INTERVAL_IN_MILLISECONDS = 5000;

		// Token: 0x040002DC RID: 732
		private static int TIME_INTERVAL_IN_MILLISECONDS_PONG = 20000;

		// Token: 0x040002DD RID: 733
		private static int PORT = 42000;

		// Token: 0x040002DE RID: 734
		private static string HOSTNAME = "";

		// Token: 0x040002DF RID: 735
		private static byte fichCounter = 0;

		// Token: 0x040002E0 RID: 736
		private static bool seenFusionHeader = false;

		// Token: 0x040002E3 RID: 739
		private static long last_pong = 0L;

		// Token: 0x040002E4 RID: 740
		private static long lastVoiceheader = 0L;

		// Token: 0x040002E5 RID: 741
		private static YSFFusion.STATUS m_status = YSFFusion.STATUS.DISCONNECTED;

		// Token: 0x02000084 RID: 132
		public enum STATUS
		{
			// Token: 0x04000511 RID: 1297
			DISCONNECTED,
			// Token: 0x04000512 RID: 1298
			W_LOGIN,
			// Token: 0x04000513 RID: 1299
			W_AUTHORISATION,
			// Token: 0x04000514 RID: 1300
			W_CONFIG,
			// Token: 0x04000515 RID: 1301
			RUNNING
		}

		// Token: 0x02000085 RID: 133
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04000516 RID: 1302
			public static TimerCallback <0>__writePing;

			// Token: 0x04000517 RID: 1303
			public static TimerCallback <1>__pongCheck;

			// Token: 0x04000518 RID: 1304
			public static AsyncCallback <2>__receiveData;
		}
	}
}
