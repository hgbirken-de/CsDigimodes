using System;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Windows.Forms;

namespace BlueDV
{
	// Token: 0x0200000A RID: 10
	internal class AMBESERVER
	{
		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000051 RID: 81 RVA: 0x000037F4 File Offset: 0x000019F4
		// (set) Token: 0x06000052 RID: 82 RVA: 0x000037FB File Offset: 0x000019FB
		public static string StatusTextAMBEServer { get; private set; }

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x06000053 RID: 83 RVA: 0x00003804 File Offset: 0x00001A04
		// (remove) Token: 0x06000054 RID: 84 RVA: 0x00003838 File Offset: 0x00001A38
		public static event EventHandler StatusTextChangedAMBEServer;

		// Token: 0x06000055 RID: 85 RVA: 0x0000386B File Offset: 0x00001A6B
		public static bool startServer(string hostname, int UDPPort)
		{
			AMBESERVER.HOSTNAME = hostname;
			AMBESERVER.DSTPORT = UDPPort;
			return AMBESERVER.start();
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00003880 File Offset: 0x00001A80
		private static void startTimer()
		{
			DVMEGAAMBE.getAMBERDVMEGA_PIN1();
			switch (information.m_language)
			{
			case information.LANGUAGE.ENGLISH:
				AMBESERVER.ChangeStatusTextAMBEServer("Connecting to AMBEServer ");
				break;
			case information.LANGUAGE.JAPANESE:
				AMBESERVER.ChangeStatusTextAMBEServer("AMBEサーバーに接続開始");
				break;
			case information.LANGUAGE.CHINEES:
				AMBESERVER.ChangeStatusTextAMBEServer("Connecting to AMBEServer ");
				break;
			case information.LANGUAGE.KOREAN:
				AMBESERVER.ChangeStatusTextAMBEServer("AMBE서버에 연결중");
				break;
			}
			AMBESERVER.aTimer = new global::System.Timers.Timer();
			AMBESERVER.aTimer.Elapsed += AMBESERVER.ping;
			AMBESERVER.aTimer.Interval = 5000.0;
			AMBESERVER.aTimer.Enabled = true;
		}

		// Token: 0x06000057 RID: 87 RVA: 0x0000391E File Offset: 0x00001B1E
		private static void stopTimer()
		{
			if (AMBESERVER.aTimer != null)
			{
				AMBESERVER.aTimer.Stop();
				AMBESERVER.aTimer.Close();
				AMBESERVER.aTimer.Dispose();
			}
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00003948 File Offset: 0x00001B48
		private static void ping(object sender, ElapsedEventArgs e)
		{
			DVMEGAAMBE.getAMBERDVMEGA_PIN1();
			if (DateTime.Now.Ticks / 10000L - AMBESERVER.lastPongInMiliseconds > 10000L)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					AMBESERVER.ChangeStatusTextAMBEServer("Reconnecting AMBEServer ");
					return;
				case information.LANGUAGE.JAPANESE:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEサーバーに再接続中");
					return;
				case information.LANGUAGE.CHINEES:
					AMBESERVER.ChangeStatusTextAMBEServer("Reconnecting AMBEServer ");
					return;
				case information.LANGUAGE.KOREAN:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBE서버에 다시 연결중 ");
					return;
				default:
					return;
				}
			}
			else
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					AMBESERVER.ChangeStatusTextAMBEServer("Connected to AMBEServer ");
					return;
				case information.LANGUAGE.JAPANESE:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEサーバーに接続");
					return;
				case information.LANGUAGE.CHINEES:
					AMBESERVER.ChangeStatusTextAMBEServer("Connected to AMBEServer ");
					return;
				case information.LANGUAGE.KOREAN:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBE서버에 연결됨");
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00003A14 File Offset: 0x00001C14
		public static void pong()
		{
			AMBESERVER.lastPongInMiliseconds = DateTime.Now.Ticks / 10000L;
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00003A3C File Offset: 0x00001C3C
		private static bool start()
		{
			try
			{
				AMBESERVER.udpClient = new UdpClient();
			}
			catch (SocketException ex)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEServer: " + ex.Message.ToString());
					break;
				case information.LANGUAGE.JAPANESE:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEサーバー: " + ex.Message.ToString());
					break;
				case information.LANGUAGE.CHINEES:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEServer: " + ex.Message.ToString());
					break;
				case information.LANGUAGE.KOREAN:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEServer: " + ex.Message.ToString());
					break;
				}
				return false;
			}
			try
			{
				AMBESERVER.udpClient.Connect(AMBESERVER.HOSTNAME, AMBESERVER.DSTPORT);
				AMBESERVER.udpClient.BeginReceive(new AsyncCallback(AMBESERVER.receiveData2), AMBESERVER.udpClient);
				AMBESERVER.startTimer();
			}
			catch (ObjectDisposedException ex2)
			{
				AMBESERVER.m_status = AMBESERVER.STATUS.DISCONNECTED;
				string text = "AMBEServer ERROR : ";
				ObjectDisposedException ex3 = ex2;
				MessageBox.Show(text + ((ex3 != null) ? ex3.ToString() : null));
				return false;
			}
			catch (SocketException ex4)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEServer: " + ex4.Message.ToString());
					break;
				case information.LANGUAGE.JAPANESE:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEサーバー: " + ex4.Message.ToString());
					break;
				case information.LANGUAGE.CHINEES:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEServer: " + ex4.Message.ToString());
					break;
				case information.LANGUAGE.KOREAN:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEServer: " + ex4.Message.ToString());
					break;
				}
			}
			catch (Exception ex5)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEServer: " + ex5.Message.ToString());
					break;
				case information.LANGUAGE.JAPANESE:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEサーバー: " + ex5.Message.ToString());
					break;
				case information.LANGUAGE.CHINEES:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEServer: " + ex5.Message.ToString());
					break;
				case information.LANGUAGE.KOREAN:
					AMBESERVER.ChangeStatusTextAMBEServer("AMBEServer: " + ex5.Message.ToString());
					break;
				}
				AMBESERVER.m_status = AMBESERVER.STATUS.DISCONNECTED;
				return false;
			}
			return true;
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00003CC4 File Offset: 0x00001EC4
		public static void stopServer()
		{
			AMBESERVER.stopTimer();
			AMBESERVER.m_status = AMBESERVER.STATUS.STOPPED;
			if (AMBESERVER.udpClient != null)
			{
				AMBESERVER.udpClient.Client.Shutdown(SocketShutdown.Receive);
				AMBESERVER.udpClient.Close();
				AMBESERVER.udpClient = null;
			}
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00003CF8 File Offset: 0x00001EF8
		private static void receiveData2(IAsyncResult ar)
		{
			try
			{
				IPEndPoint ipendPoint = new IPEndPoint(IPAddress.Any, 0);
				byte[] array = AMBESERVER.udpClient.EndReceive(ar, ref ipendPoint);
				int num = array.Length;
				if (num > 0)
				{
					byte[] array2 = new byte[num];
					Buffer.BlockCopy(array, 0, array2, 0, num);
					foreach (byte b in array2)
					{
						information.DEVICE device = information.m_device;
						if (device != information.DEVICE.DVMEGARADIO)
						{
							if (device == information.DEVICE.DV3000R)
							{
								DVMEGAAMBE.decodeAMBE(b);
							}
						}
						else
						{
							DVMEGASerial.procesMMDVM(b);
						}
					}
				}
				AMBESERVER.udpClient.BeginReceive(new AsyncCallback(AMBESERVER.receiveData2), ar.AsyncState);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (NullReferenceException)
			{
			}
			catch (SocketException ex)
			{
				MessageBox.Show("AMBEServer connection ERROR : " + ex.Message);
			}
			catch (Exception ex2)
			{
				string text = "AMBEServer ERROR : ";
				Exception ex3 = ex2;
				MessageBox.Show(text + ((ex3 != null) ? ex3.ToString() : null));
				AMBESERVER.m_status = AMBESERVER.STATUS.DISCONNECTED;
			}
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00003E14 File Offset: 0x00002014
		public static bool write2(byte[] data)
		{
			try
			{
				AMBESERVER.udpClient.Send(data, data.Length);
			}
			catch (ObjectDisposedException)
			{
				return false;
			}
			catch (Exception)
			{
				AMBESERVER.m_status = AMBESERVER.STATUS.DISCONNECTED;
				return false;
			}
			return true;
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00003E64 File Offset: 0x00002064
		private static void ChangeStatusTextAMBEServer(string text)
		{
			AMBESERVER.StatusTextAMBEServer = text;
			EventHandler statusTextChangedAMBEServer = AMBESERVER.StatusTextChangedAMBEServer;
			if (statusTextChangedAMBEServer != null)
			{
				statusTextChangedAMBEServer(null, EventArgs.Empty);
			}
		}

		// Token: 0x0400001F RID: 31
		private static UdpClient udpClient;

		// Token: 0x04000020 RID: 32
		private static string HOSTNAME;

		// Token: 0x04000021 RID: 33
		private static int DSTPORT = 2460;

		// Token: 0x04000022 RID: 34
		private static global::System.Timers.Timer aTimer;

		// Token: 0x04000023 RID: 35
		private static long lastPongInMiliseconds = 0L;

		// Token: 0x04000024 RID: 36
		private const long TIMEOUT = 10000L;

		// Token: 0x04000027 RID: 39
		private static AMBESERVER.STATUS m_status = AMBESERVER.STATUS.DISCONNECTED;

		// Token: 0x02000052 RID: 82
		public enum STATUS
		{
			// Token: 0x040004BE RID: 1214
			DISCONNECTED,
			// Token: 0x040004BF RID: 1215
			CONNECTED,
			// Token: 0x040004C0 RID: 1216
			CONNECTING,
			// Token: 0x040004C1 RID: 1217
			STOPPED
		}
	}
}
