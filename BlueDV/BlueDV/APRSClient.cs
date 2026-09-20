using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BlueDV
{
	// Token: 0x0200000C RID: 12
	internal class APRSClient
	{
		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600006D RID: 109 RVA: 0x00004078 File Offset: 0x00002278
		// (set) Token: 0x0600006E RID: 110 RVA: 0x0000407F File Offset: 0x0000227F
		public static string StatusChat { get; private set; }

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x0600006F RID: 111 RVA: 0x00004088 File Offset: 0x00002288
		// (remove) Token: 0x06000070 RID: 112 RVA: 0x000040BC File Offset: 0x000022BC
		public static event EventHandler StatusChatChanged;

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000071 RID: 113 RVA: 0x000040EF File Offset: 0x000022EF
		// (set) Token: 0x06000072 RID: 114 RVA: 0x000040F6 File Offset: 0x000022F6
		public static string StatusPicture { get; private set; }

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x06000073 RID: 115 RVA: 0x00004100 File Offset: 0x00002300
		// (remove) Token: 0x06000074 RID: 116 RVA: 0x00004134 File Offset: 0x00002334
		public static event EventHandler StatusPictureChanged;

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000075 RID: 117 RVA: 0x00004167 File Offset: 0x00002367
		// (set) Token: 0x06000076 RID: 118 RVA: 0x0000416E File Offset: 0x0000236E
		public static string StatusTextAPRS { get; private set; }

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x06000077 RID: 119 RVA: 0x00004178 File Offset: 0x00002378
		// (remove) Token: 0x06000078 RID: 120 RVA: 0x000041AC File Offset: 0x000023AC
		public static event EventHandler StatusTextChangedAPRS;

		// Token: 0x06000079 RID: 121 RVA: 0x000041E0 File Offset: 0x000023E0
		public static void connect(string hostname, int port)
		{
			APRSClient.nullCounter = 0;
			try
			{
				try
				{
					IPEndPoint ipendPoint = new IPEndPoint(Dns.GetHostEntry(hostname).AddressList[0], port);
					Socket socket;
					if (ipendPoint.AddressFamily == AddressFamily.InterNetwork)
					{
						socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					}
					else
					{
						socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
					}
					socket.BeginConnect(ipendPoint, new AsyncCallback(APRSClient.Connected), socket);
				}
				catch (ObjectDisposedException)
				{
				}
				catch (Exception ex)
				{
					APRSClient.ChangeStatusTextAPRS("APRS: " + ex.Message);
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00004284 File Offset: 0x00002484
		public static void login()
		{
			string text = string.Concat(new string[]
			{
				"user ",
				information.myCall.Trim(),
				"-G pass ",
				APRSClient.doHash(information.myCall.Trim()).ToString(),
				" vers BlueDV Windows\n"
			});
			switch (utils.RunningPlatform())
			{
			case utils.Platform.Windows:
				text = string.Concat(new string[]
				{
					"user ",
					information.myCall.Trim(),
					"-G pass ",
					APRSClient.doHash(information.myCall.Trim()).ToString(),
					" vers BlueDV Windows\n"
				});
				break;
			case utils.Platform.Linux:
				text = string.Concat(new string[]
				{
					"user ",
					information.myCall.Trim(),
					"-G pass ",
					APRSClient.doHash(information.myCall.Trim()).ToString(),
					" vers BlueDV Linux\n"
				});
				break;
			case utils.Platform.Mac:
				text = string.Concat(new string[]
				{
					"user ",
					information.myCall.Trim(),
					"-G pass ",
					APRSClient.doHash(information.myCall.Trim()).ToString(),
					" vers BlueDV OSX\n"
				});
				break;
			}
			APRSClient.send(text);
		}

		// Token: 0x0600007B RID: 123 RVA: 0x000043E8 File Offset: 0x000025E8
		public static void send(string sendingString)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(sendingString);
			APRSClient.clientSocket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(APRSClient.SendData), APRSClient.clientSocket);
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00004424 File Offset: 0x00002624
		public static void sendAPRS(string sendingString)
		{
			try
			{
				if (DateTime.Now.Ticks / 10000L - APRSClient.lastTime > APRSClient.TIMEOUT)
				{
					byte[] bytes = Encoding.ASCII.GetBytes(sendingString);
					if (APRSClient.clientSocket != null)
					{
						APRSClient.clientSocket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(APRSClient.SendData), APRSClient.clientSocket);
					}
					APRSClient.lastTime = DateTime.Now.Ticks / 10000L;
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x0600007D RID: 125 RVA: 0x000044B8 File Offset: 0x000026B8
		private static void Connected(IAsyncResult iar)
		{
			APRSClient.nullCounter = 0;
			APRSClient.clientSocket = (Socket)iar.AsyncState;
			try
			{
				APRSClient.clientSocket.EndConnect(iar);
				APRSClient.ChangeStatusTextAPRS("");
				APRSClient.clientSocket.BeginReceive(APRSClient.data, 0, APRSClient.size, SocketFlags.None, new AsyncCallback(APRSClient.onReceive), APRSClient.clientSocket);
			}
			catch (ArgumentException)
			{
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException ex)
			{
				APRSClient.ChangeStatusTextAPRS("APRS: " + ex.Message);
				APRSClient.close();
				Thread.Sleep(10000);
				APRSClient.connect("euro.aprs2.net", 14580);
			}
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00004580 File Offset: 0x00002780
		public static void onReceive(IAsyncResult iar)
		{
			try
			{
				Socket socket = (Socket)iar.AsyncState;
				int num = socket.EndReceive(iar);
				string @string = Encoding.ASCII.GetString(APRSClient.data, 0, num);
				if (num == 0)
				{
					APRSClient.nullCounter++;
				}
				if (APRSClient.nullCounter > 20)
				{
					APRSClient.close();
					Thread.Sleep(10000);
					APRSClient.connect("euro.aprs2.net", 14580);
				}
				if (@string.Contains(">AP"))
				{
					string text = @string.Split(new char[] { ':' })[3].Split(new char[] { '{' })[0];
					APRSClient.ChangeStatusChat(string.Concat(new string[]
					{
						"[ ",
						DateTime.Now.ToString("h:mm tt"),
						" ",
						@string.Split(new char[] { '>' })[0],
						" -> ",
						information.myCall,
						"-G ] \r\n"
					}));
					APRSClient.ChangeStatusChat(text);
					APRSClient.ChangeStatusPicture("NEW");
				}
				if (@string.StartsWith("# aprsc") && !APRSClient.loggedin)
				{
					APRSClient.login();
				}
				if (@string.Contains("verified"))
				{
					APRSClient.loggedin = true;
				}
				if (@string.Contains("{"))
				{
					string text2 = @string.Split(new char[] { '>' })[0];
					string[] array = @string.Split(new char[] { '{' });
					APRSClient.send(string.Concat(new string[]
					{
						information.myCall,
						"-G>APRS,TCPIP*,qAC,SIXTH::",
						text2.PadRight(9),
						":ack",
						array[1].Replace("}", ""),
						"\n"
					}));
				}
				socket.BeginReceive(APRSClient.data, 0, APRSClient.size, SocketFlags.None, new AsyncCallback(APRSClient.onReceive), socket);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x0600007F RID: 127 RVA: 0x000047A4 File Offset: 0x000029A4
		private static void SendData(IAsyncResult iar)
		{
			try
			{
				if (iar != null)
				{
					Socket socket = (Socket)iar.AsyncState;
					socket.EndSend(iar);
					socket.BeginReceive(APRSClient.data, 0, APRSClient.size, SocketFlags.None, new AsyncCallback(APRSClient.onReceive), socket);
				}
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00004810 File Offset: 0x00002A10
		public static int doHash(string callSign)
		{
			int num = 29666;
			if (callSign.IndexOf('-') > 0)
			{
				callSign = callSign.Substring(0, callSign.IndexOf('-'));
			}
			callSign = callSign.ToUpper();
			short num2 = 0;
			int num3 = num;
			int length = callSign.Length;
			while ((int)num2 < length)
			{
				num3 ^= (int)((int)callSign[(int)num2] << 8);
				if ((int)(num2 + 1) < length)
				{
					num3 ^= (int)callSign[(int)(num2 + 1)];
				}
				num2 += 2;
			}
			return num3 & 32767;
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00004880 File Offset: 0x00002A80
		private static void ChangeStatusChat(string text)
		{
			APRSClient.StatusChat = text;
			information.APRSChatText += text;
			EventHandler statusChatChanged = APRSClient.StatusChatChanged;
			if (statusChatChanged != null)
			{
				statusChatChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000082 RID: 130 RVA: 0x000048B8 File Offset: 0x00002AB8
		private static void ChangeStatusPicture(string text2)
		{
			APRSClient.StatusPicture = text2;
			EventHandler statusPictureChanged = APRSClient.StatusPictureChanged;
			if (statusPictureChanged != null)
			{
				statusPictureChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000083 RID: 131 RVA: 0x000048E0 File Offset: 0x00002AE0
		private static void ChangeStatusTextAPRS(string text)
		{
			APRSClient.StatusTextAPRS = text;
			EventHandler statusTextChangedAPRS = APRSClient.StatusTextChangedAPRS;
			if (statusTextChangedAPRS != null)
			{
				statusTextChangedAPRS(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00004908 File Offset: 0x00002B08
		public static void close()
		{
			APRSClient.loggedin = false;
			APRSClient.nullCounter = 0;
			if (APRSClient.clientSocket != null)
			{
				try
				{
					APRSClient.clientSocket.Close();
					APRSClient.clientSocket.Dispose();
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x0400002E RID: 46
		public static Socket clientSocket;

		// Token: 0x0400002F RID: 47
		private static byte[] data = new byte[1024];

		// Token: 0x04000030 RID: 48
		private static int size = 1024;

		// Token: 0x04000031 RID: 49
		private static long lastTime = 0L;

		// Token: 0x04000032 RID: 50
		private static long TIMEOUT = 15000L;

		// Token: 0x04000033 RID: 51
		private static bool loggedin = false;

		// Token: 0x04000034 RID: 52
		private static int nullCounter = 0;
	}
}
