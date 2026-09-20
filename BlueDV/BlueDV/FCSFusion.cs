using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace BlueDV
{
	// Token: 0x02000027 RID: 39
	internal class FCSFusion
	{
		// Token: 0x17000032 RID: 50
		// (get) Token: 0x06000245 RID: 581 RVA: 0x0001592F File Offset: 0x00013B2F
		// (set) Token: 0x06000246 RID: 582 RVA: 0x00015936 File Offset: 0x00013B36
		public static string StatusTextFUSION { get; private set; }

		// Token: 0x14000015 RID: 21
		// (add) Token: 0x06000247 RID: 583 RVA: 0x00015940 File Offset: 0x00013B40
		// (remove) Token: 0x06000248 RID: 584 RVA: 0x00015974 File Offset: 0x00013B74
		public static event EventHandler StatusTextChangedFUSION;

		// Token: 0x06000249 RID: 585 RVA: 0x000159A7 File Offset: 0x00013BA7
		public static void open()
		{
			FCSFusion.m_status = FCSFusion.STATUS.W_LOGIN;
			TimerCallback timerCallback;
			if ((timerCallback = FCSFusion.<>O.<0>__writePing) == null)
			{
				timerCallback = (FCSFusion.<>O.<0>__writePing = new TimerCallback(FCSFusion.writePing));
			}
			FCSFusion._timer = new Timer(timerCallback, null, FCSFusion.TIME_INTERVAL_IN_MILLISECONDS, -1);
			FCSFusion.ping();
			FCSFusion.login();
		}

		// Token: 0x0600024A RID: 586 RVA: 0x000159E8 File Offset: 0x00013BE8
		public static void reconnect()
		{
			FCSFusion.m_status = FCSFusion.STATUS.W_LOGIN;
			try
			{
				FCSFusion.udpClient.Close();
				FCSFusion.udpClient = new UdpClient();
				FCSFusion.udpClient.Connect(FCSFusion.HOSTNAME, FCSFusion.PORT);
				FCSFusion.udpClient.BeginReceive(new AsyncCallback(FCSFusion.receiveData), FCSFusion.udpClient);
				FCSFusion.m_status = FCSFusion.STATUS.W_LOGIN;
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x0600024B RID: 587 RVA: 0x00015A5C File Offset: 0x00013C5C
		public static void connect(string hostname, int port)
		{
			FCSFusion.HOSTNAME = hostname;
			FCSFusion.PORT = port;
			DSTARhandler.increment_session_id();
			try
			{
				FCSFusion.udpClient = new UdpClient();
			}
			catch (SocketException)
			{
				return;
			}
			try
			{
				FCSFusion.udpClient.Connect(FCSFusion.HOSTNAME, FCSFusion.PORT);
				UdpClient udpClient = FCSFusion.udpClient;
				AsyncCallback asyncCallback;
				if ((asyncCallback = FCSFusion.<>O.<1>__receiveData) == null)
				{
					asyncCallback = (FCSFusion.<>O.<1>__receiveData = new AsyncCallback(FCSFusion.receiveData));
				}
				udpClient.BeginReceive(asyncCallback, FCSFusion.udpClient);
				FCSFusion.open();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					FCSFusion.ChangeStatusTextFUSION("ERR: " + ex.Message);
					break;
				case information.LANGUAGE.JAPANESE:
					FCSFusion.ChangeStatusTextFUSION("エラー： " + ex.Message);
					break;
				case information.LANGUAGE.CHINEES:
					FCSFusion.ChangeStatusTextFUSION("ERR: " + ex.Message);
					break;
				case information.LANGUAGE.KOREAN:
					FCSFusion.ChangeStatusTextFUSION("ERR: " + ex.Message);
					break;
				}
			}
		}

		// Token: 0x0600024C RID: 588 RVA: 0x00015B80 File Offset: 0x00013D80
		private static void ChangeStatusTextFUSION(string text)
		{
			FCSFusion.StatusTextFUSION = text;
			EventHandler statusTextChangedFUSION = FCSFusion.StatusTextChangedFUSION;
			if (statusTextChangedFUSION != null)
			{
				statusTextChangedFUSION(null, EventArgs.Empty);
			}
		}

		// Token: 0x0600024D RID: 589 RVA: 0x00015BA8 File Offset: 0x00013DA8
		public static void makeFrame(byte[] voice)
		{
			byte[] array = new byte[130];
			if (information.fusionReflector == null)
			{
				return;
			}
			string text = information.fusionReflector.PadRight(8);
			Array bytes = Encoding.ASCII.GetBytes(text);
			Buffer.BlockCopy(voice, 0, array, 0, 120);
			Buffer.BlockCopy(bytes, 0, array, 121, 8);
			array[120] = (byte)(FCSFusion.fichCounter << 1);
			FCSFusion.fichCounter += 1;
			array[129] = 0;
			FCSFusion.sendRAW(array, 130);
		}

		// Token: 0x0600024E RID: 590 RVA: 0x00015C20 File Offset: 0x00013E20
		public static void close()
		{
			if (FCSFusion.udpClient != null)
			{
				try
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						FCSFusion.ChangeStatusTextFUSION("Not linked");
						break;
					case information.LANGUAGE.JAPANESE:
						FCSFusion.ChangeStatusTextFUSION("未接続");
						break;
					case information.LANGUAGE.CHINEES:
						FCSFusion.ChangeStatusTextFUSION("Not linked");
						break;
					case information.LANGUAGE.KOREAN:
						FCSFusion.ChangeStatusTextFUSION("연결안됨");
						break;
					}
					FCSFusion.logout();
					FCSFusion._timer.Dispose();
					FCSFusion.udpClient.Client.Shutdown(SocketShutdown.Receive);
					FCSFusion.udpClient.Close();
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x0600024F RID: 591 RVA: 0x00015CC0 File Offset: 0x00013EC0
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
					if (@string.StartsWith("ONLINE"))
					{
						switch (information.m_language)
						{
						case information.LANGUAGE.ENGLISH:
							FCSFusion.ChangeStatusTextFUSION("Linked to " + information.fusionReflector.Substring(0, 6) + " " + information.fusionReflector.Substring(6, 2));
							break;
						case information.LANGUAGE.JAPANESE:
							FCSFusion.ChangeStatusTextFUSION("接続中 " + information.fusionReflector.Substring(0, 6) + " " + information.fusionReflector.Substring(6, 2));
							break;
						case information.LANGUAGE.CHINEES:
							FCSFusion.ChangeStatusTextFUSION("Linked to " + information.fusionReflector.Substring(0, 6) + " " + information.fusionReflector.Substring(6, 2));
							break;
						case information.LANGUAGE.KOREAN:
							FCSFusion.ChangeStatusTextFUSION("연결됨 " + information.fusionReflector.Substring(0, 6) + " " + information.fusionReflector.Substring(6, 2));
							break;
						}
					}
					if (array.Length == 130)
					{
						if (FCSFusion.canStream())
						{
							information.hisCall = @string.Substring(121, 6).PadRight(8);
							if (information.hisCall.StartsWith("q?"))
							{
								information.hisCall = "ECHO";
							}
							TimerRXTX.TX();
							byte[] array2 = new byte[120];
							Buffer.BlockCopy(array, 0, array2, 0, 120);
							array2[0] = 212;
							array2[1] = 113;
							array2[2] = 201;
							array2[3] = 99;
							array2[4] = 77;
							FCSFusion.fromFUSION(array2);
						}
						ShowCallTimer.resetTimerFUSION(@string.Substring(121, 6).PadRight(8));
					}
				}
				udpClient.BeginReceive(new AsyncCallback(FCSFusion.receiveData), ar.AsyncState);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000250 RID: 592 RVA: 0x00015ECC File Offset: 0x000140CC
		private static void writeFCSI()
		{
			try
			{
				string text = string.Concat(new string[]
				{
					"FCSI",
					information.fusionReflector.PadRight(8),
					information.myFREQ.PadRight(9),
					information.myFREQ.PadRight(9),
					information.myQTHlocation.PadRight(6),
					"Hamshack".PadRight(20),
					"BlueDV".PadRight(12),
					information.myDMRID.PadRight(7),
					" ".PadRight(5)
				});
				FCSFusion.sendRAW(Encoding.ASCII.GetBytes(text), 80);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000251 RID: 593 RVA: 0x00015F8C File Offset: 0x0001418C
		private static void fromFUSION(byte[] voice)
		{
			byte[] array = new byte[124];
			array[0] = 224;
			array[1] = 124;
			array[2] = 32;
			array[3] = 0;
			Buffer.BlockCopy(voice, 0, array, 4, 120);
			FusionQueue.addQueue(array);
		}

		// Token: 0x06000252 RID: 594 RVA: 0x00015FC8 File Offset: 0x000141C8
		private static void writePing(object o)
		{
			FCSFusion.ping();
			FCSFusion._timer.Change(FCSFusion.TIME_INTERVAL_IN_MILLISECONDS, -1);
		}

		// Token: 0x06000253 RID: 595 RVA: 0x00015FE0 File Offset: 0x000141E0
		private static void ping()
		{
			string text = "PING" + information.myCall.PadRight(6) + information.fusionReflector.PadRight(8) + " ".PadRight(7);
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			bytes[18] = 0;
			bytes[19] = DSTARhandler.session_id_byte()[0];
			bytes[20] = DSTARhandler.session_id_byte()[1];
			bytes[21] = DSTARhandler.session_id_byte()[0];
			bytes[22] = DSTARhandler.session_id_byte()[1];
			bytes[23] = 116;
			bytes[24] = 156;
			FCSFusion.sendRAW(bytes, 25);
		}

		// Token: 0x06000254 RID: 596 RVA: 0x0001606C File Offset: 0x0001426C
		private static byte[] GetBytes(string str)
		{
			byte[] array = new byte[str.Length * 2];
			Buffer.BlockCopy(str.ToCharArray(), 0, array, 0, array.Length);
			return array;
		}

		// Token: 0x06000255 RID: 597 RVA: 0x0001609C File Offset: 0x0001429C
		private static void logout()
		{
			for (int i = 0; i < 5; i++)
			{
				FCSFusion.sendRAW(Encoding.ASCII.GetBytes("CLOSE      "), 11);
			}
		}

		// Token: 0x06000256 RID: 598 RVA: 0x000160CC File Offset: 0x000142CC
		private static void writeYSFO()
		{
			try
			{
				string text = "FCSO" + information.fusionReflector.PadRight(8) + information.YSFOoptions.PadRight(38);
				FCSFusion.sendRAW(Encoding.ASCII.GetBytes(text), 50);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000257 RID: 599 RVA: 0x00016124 File Offset: 0x00014324
		private static void login()
		{
			string text = string.Concat(new string[]
			{
				information.myFREQ,
				information.myFREQ,
				information.myQTHlocation.PadRight(6),
				"BlueDV WIN".PadRight(12),
				information.myDMRID.PadRight(7)
			}).PadRight(100);
			switch (utils.RunningPlatform())
			{
			case utils.Platform.Windows:
				text = string.Concat(new string[]
				{
					information.myFREQ,
					information.myFREQ,
					information.myQTHlocation.PadRight(6),
					"BlueDV WIN".PadRight(12),
					information.myDMRID.PadRight(7)
				}).PadRight(100);
				break;
			case utils.Platform.Linux:
				text = string.Concat(new string[]
				{
					information.myFREQ,
					information.myFREQ,
					information.myQTHlocation.PadRight(6),
					"BlueDV LNX".PadRight(12),
					information.myDMRID.PadRight(7)
				}).PadRight(100);
				break;
			case utils.Platform.Mac:
				text = string.Concat(new string[]
				{
					information.myFREQ,
					information.myFREQ,
					information.myQTHlocation.PadRight(6),
					"BlueDV MAC".PadRight(12),
					information.myDMRID.PadRight(7)
				}).PadRight(100);
				break;
			}
			FCSFusion.sendRAW(Encoding.ASCII.GetBytes(text), 100);
			FCSFusion.writeFCSI();
			FCSFusion.writeYSFO();
		}

		// Token: 0x06000258 RID: 600 RVA: 0x000162AC File Offset: 0x000144AC
		public static void sendRAW(byte[] data, int length)
		{
			try
			{
				FCSFusion.udpClient.Send(data, length);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000259 RID: 601 RVA: 0x000162DC File Offset: 0x000144DC
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

		// Token: 0x0600025A RID: 602 RVA: 0x00016300 File Offset: 0x00014500
		private static string GetString(byte[] bytes)
		{
			char[] array = new char[bytes.Length / 2];
			Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
			return new string(array);
		}

		// Token: 0x04000149 RID: 329
		private static UdpClient udpClient;

		// Token: 0x0400014A RID: 330
		private static Timer _timer;

		// Token: 0x0400014B RID: 331
		private static int TIME_INTERVAL_IN_MILLISECONDS = 5000;

		// Token: 0x0400014C RID: 332
		private static byte fichCounter = 0;

		// Token: 0x0400014D RID: 333
		private static int PORT = 43200;

		// Token: 0x0400014E RID: 334
		private static string HOSTNAME = "";

		// Token: 0x0400014F RID: 335
		private static string oldCall = "";

		// Token: 0x04000152 RID: 338
		private static FCSFusion.STATUS m_status = FCSFusion.STATUS.DISCONNECTED;

		// Token: 0x0200005D RID: 93
		public enum STATUS
		{
			// Token: 0x040004E4 RID: 1252
			DISCONNECTED,
			// Token: 0x040004E5 RID: 1253
			W_LOGIN,
			// Token: 0x040004E6 RID: 1254
			W_AUTHORISATION,
			// Token: 0x040004E7 RID: 1255
			W_CONFIG,
			// Token: 0x040004E8 RID: 1256
			RUNNING
		}

		// Token: 0x0200005E RID: 94
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040004E9 RID: 1257
			public static TimerCallback <0>__writePing;

			// Token: 0x040004EA RID: 1258
			public static AsyncCallback <1>__receiveData;
		}
	}
}
