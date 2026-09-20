using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace BlueDV
{
	// Token: 0x02000014 RID: 20
	internal class DMRPlus
	{
		// Token: 0x1400000C RID: 12
		// (add) Token: 0x060000E9 RID: 233 RVA: 0x00008C90 File Offset: 0x00006E90
		// (remove) Token: 0x060000EA RID: 234 RVA: 0x00008CC4 File Offset: 0x00006EC4
		public static event EventHandler ThresholdReached;

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060000EB RID: 235 RVA: 0x00008CF7 File Offset: 0x00006EF7
		// (set) Token: 0x060000EC RID: 236 RVA: 0x00008CFE File Offset: 0x00006EFE
		public static string StatusText { get; private set; }

		// Token: 0x1400000D RID: 13
		// (add) Token: 0x060000ED RID: 237 RVA: 0x00008D08 File Offset: 0x00006F08
		// (remove) Token: 0x060000EE RID: 238 RVA: 0x00008D3C File Offset: 0x00006F3C
		public static event EventHandler StatusTextChanged;

		// Token: 0x060000EF RID: 239 RVA: 0x00008D70 File Offset: 0x00006F70
		protected virtual void OnThresholdReached(EventArgs e)
		{
			EventHandler thresholdReached = DMRPlus.ThresholdReached;
			if (thresholdReached != null)
			{
				thresholdReached(this, e);
			}
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x00008D90 File Offset: 0x00006F90
		public static void open()
		{
			DMRPlus.m_status = DMRPlus.STATUS.W_LOGIN;
			DMRPlus.m_timeoutTimer.start();
			DMRPlus.m_retryTimer.start();
			TimerCallback timerCallback;
			if ((timerCallback = DMRPlus.<>O.<0>__writePing) == null)
			{
				timerCallback = (DMRPlus.<>O.<0>__writePing = new TimerCallback(DMRPlus.writePing));
			}
			DMRPlus._timer = new Timer(timerCallback, null, DMRPlus.TIME_INTERVAL_IN_MILLISECONDS, -1);
			DMRPlus.writeLogin();
			DMRPlus.currenttime = DateTime.Now.Ticks / 10000L;
			DMRPlus.ms = (int)(DMRPlus.currenttime - DMRPlus.oldtime);
			DMRPlus.oldtime = DMRPlus.currenttime;
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00008E1C File Offset: 0x0000701C
		public static void reconnect()
		{
			try
			{
				DMRPlus.udpClient.Close();
				DMRPlus.udpClient = new UdpClient();
				DMRPlus.udpClient.Connect(DMRPlus.HOSTNAME, DMRPlus.PORT);
				DMRPlus.udpClient.BeginReceive(new AsyncCallback(DMRPlus.receiveData), DMRPlus.udpClient);
				DMRPlus.m_status = DMRPlus.STATUS.W_LOGIN;
				DMRPlus.m_timeoutTimer.start();
				DMRPlus.m_retryTimer.start();
				DMRPlus.writeLogin();
				DMRPlus.currenttime = DateTime.Now.Ticks / 10000L;
				DMRPlus.ms = (int)(DMRPlus.currenttime - DMRPlus.oldtime);
				DMRPlus.oldtime = DMRPlus.currenttime;
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x00008ED8 File Offset: 0x000070D8
		public static void connect(string hostname, int port, string password)
		{
			DMRPlus.PASSWORD = password;
			DMRPlus.HOSTNAME = hostname;
			DMRPlus.PORT = port;
			try
			{
				DMRPlus.m_retryTimer = new Timert(1000, 5, 0);
				DMRPlus.m_timeoutTimer = new Timert(5000, 5, 0);
				DMRPlus.udpClient = new UdpClient();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException ex)
			{
				if (ex.ErrorCode == 10048)
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DMRPlus.ChangeStatusText("ERR: DMR port in use");
						break;
					case information.LANGUAGE.JAPANESE:
						DMRPlus.ChangeStatusText("エラー：DMRポート使用中");
						break;
					case information.LANGUAGE.CHINEES:
						DMRPlus.ChangeStatusText("ERR: DMR port in use");
						break;
					case information.LANGUAGE.KOREAN:
						DMRPlus.ChangeStatusText("에러: DMR포트 사용중");
						break;
					}
				}
				else if (ex.ErrorCode == 10060)
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DMRPlus.ChangeStatusText("ERR: Conn timed out");
						break;
					case information.LANGUAGE.JAPANESE:
						DMRPlus.ChangeStatusText("エラー：DMRタイムアウト");
						break;
					case information.LANGUAGE.CHINEES:
						DMRPlus.ChangeStatusText("ERR: Conn timed out");
						break;
					case information.LANGUAGE.KOREAN:
						DMRPlus.ChangeStatusText("에러: DMR연결 시간초과");
						break;
					}
				}
				else if (ex.ErrorCode == 10013)
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DMRPlus.ChangeStatusText("ERR: DMR port in use");
						break;
					case information.LANGUAGE.JAPANESE:
						DMRPlus.ChangeStatusText("エラー：DMRポート使用中");
						break;
					case information.LANGUAGE.CHINEES:
						DMRPlus.ChangeStatusText("ERR: DMR port in use");
						break;
					case information.LANGUAGE.KOREAN:
						DMRPlus.ChangeStatusText("에러: DMR포트 사용중");
						break;
					}
				}
				else
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DMRPlus.ChangeStatusText("ERR: " + ex.ErrorCode.ToString());
						break;
					case information.LANGUAGE.JAPANESE:
						DMRPlus.ChangeStatusText("エラー： " + ex.ErrorCode.ToString());
						break;
					case information.LANGUAGE.CHINEES:
						DMRPlus.ChangeStatusText("ERR: " + ex.ErrorCode.ToString());
						break;
					case information.LANGUAGE.KOREAN:
						DMRPlus.ChangeStatusText("ERR: " + ex.ErrorCode.ToString());
						break;
					}
				}
				if (DMRPlus.udpClient != null)
				{
					DMRPlus.udpClient.Close();
				}
				DMRPlus.m_retryTimer.stop();
				DMRPlus.m_timeoutTimer.stop();
				return;
			}
			try
			{
				DMRPlus.udpClient.Connect(DMRPlus.HOSTNAME, DMRPlus.PORT);
				DMRPlus.udpClient.BeginReceive(new AsyncCallback(DMRPlus.receiveData), DMRPlus.udpClient);
				DMRPlus.open();
			}
			catch (Exception ex2)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					DMRPlus.ChangeStatusText("ERR :" + ex2.ToString());
					break;
				case information.LANGUAGE.JAPANESE:
					DMRPlus.ChangeStatusText("エラー：" + ex2.ToString());
					break;
				case information.LANGUAGE.CHINEES:
					DMRPlus.ChangeStatusText("ERR :" + ex2.ToString());
					break;
				case information.LANGUAGE.KOREAN:
					DMRPlus.ChangeStatusText("ERR :" + ex2.ToString());
					break;
				}
			}
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x00009238 File Offset: 0x00007438
		private static void ChangeStatusText(string text)
		{
			DMRPlus.StatusText = text;
			EventHandler statusTextChanged = DMRPlus.StatusTextChanged;
			if (statusTextChanged != null)
			{
				statusTextChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x00009260 File Offset: 0x00007460
		public static void sendRAW(byte[] data, int length)
		{
			try
			{
				DMRPlus.udpClient.Send(data, length);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x00009290 File Offset: 0x00007490
		public static void changeReflector1()
		{
			byte[] array = new byte[55];
			array[0] = 68;
			array[1] = 77;
			array[2] = 82;
			array[3] = 68;
			array[4] = 0;
			long num = 9L;
			long.TryParse(information.myDMRID, out num);
			array[5] = (byte)(num >> 16);
			array[6] = (byte)(num >> 8);
			array[7] = (byte)num;
			long num2 = 9L;
			long.TryParse(information.myDMRPlusDefaultReflector, out num2);
			array[8] = (byte)(num2 >> 16);
			array[9] = (byte)(num2 >> 8);
			array[10] = (byte)num2;
			byte[] array2 = utils.stringto4byte(information.myDMRID);
			array[11] = array2[3];
			array[12] = array2[2];
			array[13] = array2[1];
			array[14] = array2[0];
			array[15] = 161;
			array[16] = 1;
			array[17] = 0;
			array[18] = 0;
			array[19] = 0;
			Buffer.BlockCopy(DVMEGAAMBE.generateVoiceHeaderParameters(65, (int)num2, information.GROUPPRIVATE.PRIVATE), 0, array, 20, 33);
			DMRPlus.sendRAW(array, 55);
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x0000936C File Offset: 0x0000756C
		public static void changeReflector2()
		{
			byte[] array = new byte[55];
			array[0] = 68;
			array[1] = 77;
			array[2] = 82;
			array[3] = 68;
			array[4] = 0;
			long num = 9L;
			long.TryParse(information.myDMRID, out num);
			array[5] = (byte)(num >> 16);
			array[6] = (byte)(num >> 8);
			array[7] = (byte)num;
			long num2 = 9L;
			long.TryParse(information.myDMRPlusDefaultReflector, out num2);
			array[8] = (byte)(num2 >> 16);
			array[9] = (byte)(num2 >> 8);
			array[10] = (byte)num2;
			byte[] array2 = utils.stringto4byte(information.myDMRID);
			array[11] = array2[3];
			array[12] = array2[2];
			array[13] = array2[1];
			array[14] = array2[0];
			array[15] = 162;
			array[16] = 1;
			array[17] = 0;
			array[18] = 0;
			array[19] = 0;
			Buffer.BlockCopy(DVMEGAAMBE.generateVoiceHeaderParameters(66, (int)num2, information.GROUPPRIVATE.PRIVATE), 0, array, 20, 33);
			DMRPlus.sendRAW(array, 55);
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x00009448 File Offset: 0x00007648
		public static void writeLogin()
		{
			switch (information.m_language)
			{
			case information.LANGUAGE.ENGLISH:
				DMRPlus.ChangeStatusText("Sending DMR Login");
				break;
			case information.LANGUAGE.JAPANESE:
				DMRPlus.ChangeStatusText("DMRへログイン開始");
				break;
			case information.LANGUAGE.CHINEES:
				DMRPlus.ChangeStatusText("Sending DMR Login");
				break;
			case information.LANGUAGE.KOREAN:
				DMRPlus.ChangeStatusText("DMR 로그인 전송");
				break;
			}
			string text = "";
			text += "RPTL";
			text += "    ";
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			Buffer.BlockCopy(DMRPlus.dmrid(), 0, bytes, 4, 4);
			if (DMRPlus.udpClient.Client == null)
			{
				return;
			}
			DMRPlus.sendRAW(bytes, bytes.Length);
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x000094F4 File Offset: 0x000076F4
		public static string getHashSha256(string text)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			byte[] array = new SHA256Managed().ComputeHash(bytes);
			string text2 = string.Empty;
			foreach (byte b in array)
			{
				text2 += string.Format("{0:X2}", b);
			}
			return text2;
		}

		// Token: 0x060000FA RID: 250 RVA: 0x0000954B File Offset: 0x0000774B
		public static byte[] getHashSha256Bytes(byte[] data)
		{
			return new SHA256Managed().ComputeHash(data);
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00009558 File Offset: 0x00007758
		private static string bytetostring(byte[] data)
		{
			string text = string.Empty;
			foreach (byte b in data)
			{
				text += string.Format("{0:X2}", b);
			}
			return text;
		}

		// Token: 0x060000FC RID: 252 RVA: 0x00009598 File Offset: 0x00007798
		private static string StringtoHEXstring(string data)
		{
			string text = string.Empty;
			foreach (byte b in data)
			{
				text += string.Format("{0:X2}", b);
			}
			return text;
		}

		// Token: 0x060000FD RID: 253 RVA: 0x000095E0 File Offset: 0x000077E0
		public static void writeAuthorisation()
		{
			switch (information.m_language)
			{
			case information.LANGUAGE.ENGLISH:
				DMRPlus.ChangeStatusText("Sending DMR authorisation");
				break;
			case information.LANGUAGE.JAPANESE:
				DMRPlus.ChangeStatusText("DMRの認証開始");
				break;
			case information.LANGUAGE.CHINEES:
				DMRPlus.ChangeStatusText("Sending DMR authorisation");
				break;
			case information.LANGUAGE.KOREAN:
				DMRPlus.ChangeStatusText("DMR 승인 전송");
				break;
			}
			"" + "RPTK" + "    ";
			Array bytes = Encoding.ASCII.GetBytes("RPTK");
			byte[] array = new byte[40];
			Buffer.BlockCopy(bytes, 0, array, 0, 4);
			Buffer.BlockCopy(DMRPlus.dmrid(), 0, array, 4, 4);
			byte[] bytes2 = Encoding.ASCII.GetBytes(DMRPlus.PASSWORD);
			byte[] array2 = new byte[bytes2.Length + DMRPlus.saltBytes.Length];
			Buffer.BlockCopy(DMRPlus.saltBytes, 0, array2, 0, DMRPlus.saltBytes.Length);
			Buffer.BlockCopy(bytes2, 0, array2, DMRPlus.saltBytes.Length, bytes2.Length);
			byte[] hashSha256Bytes = DMRPlus.getHashSha256Bytes(array2);
			Buffer.BlockCopy(hashSha256Bytes, 0, array, 8, hashSha256Bytes.Length);
			DMRPlus.sendRAW(array, 40);
		}

		// Token: 0x060000FE RID: 254 RVA: 0x000096E8 File Offset: 0x000078E8
		private static void writeConfig()
		{
			string text = "RPTC";
			text += "dmri";
			text += string.Format("{0,-8}", information.myCall);
			text += string.Format("{0,-9}", information.myFREQ);
			text += string.Format("{0,-9}", information.myFREQ);
			text += "01";
			text += "01";
			text += string.Format("{0,-8}", information.latitude);
			text += string.Format("{0,-9}", information.longitude);
			text += "001";
			text += string.Format("{0,-20}", information.location);
			text += string.Format("{0,-19}", information.description);
			text += string.Format("{0,-1}", "4");
			text += string.Format("{0,-124}", information.URL);
			text += string.Format("{0,-40}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
			switch (utils.RunningPlatform())
			{
			case utils.Platform.Windows:
				text += string.Format("{0,-40}", "Windows:BlueDV");
				break;
			case utils.Platform.Linux:
				text += string.Format("{0,-40}", "Linux:BlueDV");
				break;
			case utils.Platform.Mac:
				text += string.Format("{0,-40}", "MAC:BlueDV");
				break;
			}
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			long num = (long)int.Parse(information.myDMRID);
			bytes[4] = (byte)(num >> 24);
			bytes[5] = (byte)(num >> 16);
			bytes[6] = (byte)(num >> 8);
			bytes[7] = (byte)num;
			DMRPlus.sendRAW(bytes, bytes.Length);
		}

		// Token: 0x060000FF RID: 255 RVA: 0x000098B8 File Offset: 0x00007AB8
		private static byte[] dmrid()
		{
			long num = (long)int.Parse(information.myDMRID);
			return new byte[]
			{
				(byte)(num >> 24),
				(byte)(num >> 16),
				(byte)(num >> 8),
				(byte)num
			};
		}

		// Token: 0x06000100 RID: 256 RVA: 0x000098F4 File Offset: 0x00007AF4
		public static void writeOptions()
		{
			byte[] array = new byte[200];
			long num = (long)int.Parse(information.myDMRID);
			array[0] = 82;
			array[1] = 80;
			array[2] = 84;
			array[3] = 79;
			array[4] = (byte)(num >> 24);
			array[5] = (byte)(num >> 16);
			array[6] = (byte)(num >> 8);
			array[7] = (byte)num;
			byte[] bytes = Encoding.UTF8.GetBytes(information.DMRPlusOptions.Trim(new char[] { '"' }));
			Buffer.BlockCopy(bytes, 0, array, 8, bytes.Length);
			DMRPlus.sendRAW(array, array.Length);
		}

		// Token: 0x06000101 RID: 257 RVA: 0x00009980 File Offset: 0x00007B80
		public static void writePing(object o)
		{
			DMRPlus.currenttime = DateTime.Now.Ticks / 10000L;
			DMRPlus.ms = (int)(DMRPlus.currenttime - DMRPlus.oldtime);
			DMRPlus.oldtime = DMRPlus.currenttime;
			if (DMRPlus.m_status == DMRPlus.STATUS.RUNNING)
			{
				string text = "RPTPING    ";
				byte[] bytes = Encoding.ASCII.GetBytes(text);
				Buffer.BlockCopy(DMRPlus.dmrid(), 0, bytes, 7, 4);
				DMRPlus.sendRAW(bytes, bytes.Length);
			}
			if (DMRPlus.m_timeoutTimer.isRunning() && DMRPlus.m_timeoutTimer.hasExpired())
			{
				DMRPlus.isReconnected = true;
				DMRPlus.open();
			}
			DMRPlus.m_retryTimer.clock(DMRPlus.ms);
			if ((DMRPlus.m_retryTimer.isRunning() && DMRPlus.m_retryTimer.hasExpired()) || DMRPlus.startLogin)
			{
				switch (DMRPlus.m_status)
				{
				case DMRPlus.STATUS.W_LOGIN:
					DMRPlus.writeLogin();
					break;
				case DMRPlus.STATUS.W_AUTHORISATION:
					DMRPlus.writeAuthorisation();
					break;
				case DMRPlus.STATUS.W_CONFIG:
					if (!Form1.dmrplus)
					{
						DMRPlus.writeConfig();
					}
					break;
				case DMRPlus.STATUS.RUNNING:
					DMRPlus.startLogin = false;
					break;
				}
			}
			DMRPlus.m_timeoutTimer.clock(DMRPlus.ms);
			if (DMRPlus.m_timeoutTimer.isRunning() && DMRPlus.m_timeoutTimer.hasExpired())
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					DMRPlus.ChangeStatusText("Connection timed out!");
					break;
				case information.LANGUAGE.JAPANESE:
					DMRPlus.ChangeStatusText("DMRマスターの接続がタイムアウト！");
					break;
				case information.LANGUAGE.CHINEES:
					DMRPlus.ChangeStatusText("Connection timed out!");
					break;
				case information.LANGUAGE.KOREAN:
					DMRPlus.ChangeStatusText("에러: DMR연결 시간초과");
					break;
				}
				DMRPlus.isReconnected = true;
				DMRPlus.reconnect();
			}
			DMRPlus.currenttime = DateTime.Now.Ticks / 10000L;
			DMRPlus.ms = (int)(DMRPlus.currenttime - DMRPlus.oldtime);
			DMRPlus.oldtime = DMRPlus.currenttime;
			DMRPlus._timer.Change(DMRPlus.TIME_INTERVAL_IN_MILLISECONDS, -1);
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00009B4C File Offset: 0x00007D4C
		public static void writeping2()
		{
			string text = "RPTPING    ";
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			Buffer.BlockCopy(DMRPlus.dmrid(), 0, bytes, 7, 4);
			DMRPlus.sendRAW(bytes, bytes.Length);
		}

		// Token: 0x06000103 RID: 259 RVA: 0x00009B84 File Offset: 0x00007D84
		public static void logout()
		{
			DMRPlus.m_timeoutTimer.start();
			string text = "RPTCL    ";
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			Buffer.BlockCopy(DMRPlus.dmrid(), 0, bytes, 5, 4);
			DMRPlus.sendRAW(bytes, bytes.Length);
		}

		// Token: 0x06000104 RID: 260 RVA: 0x00009BC4 File Offset: 0x00007DC4
		private static byte[] rewriteToColorCode1(byte[] voice, byte type)
		{
			bool[] array = new bool[] { false, false, false, true, false, false, true, true };
			bool[] array2 = new bool[] { true, false, false, true, false, false, false, true };
			bool[] array3 = new bool[] { false, false, false, true, false, true, true, true };
			bool[] array4 = new bool[] { false, true, true, true, false, true, false, false };
			bool[] array5 = new bool[] { false, false, false, true, false, true, true, true };
			bool[] array6 = new bool[] { false, true, true, true, false, true, false, false };
			bool[] array7 = new bool[] { false, false, false, true, false, true, false, true };
			bool[] array8 = new bool[] { false, false, false, false, false, true, true, true };
			bool[] array9 = new bool[]
			{
				default(bool),
				default(bool),
				default(bool),
				true,
				default(bool),
				default(bool),
				default(bool),
				true
			};
			bool[] array10 = new bool[] { true, true, true, false, false, false, true, false };
			bool[] array11 = DVMEGAAMBE.byteToBitConverter(voice);
			switch (type)
			{
			case 129:
				Buffer.BlockCopy(array, 0, array11, 108, 8);
				Buffer.BlockCopy(array2, 0, array11, 148, 8);
				return DVMEGAAMBE.bitToByteConvevrt(array11);
			case 130:
				Buffer.BlockCopy(array3, 0, array11, 108, 8);
				Buffer.BlockCopy(array4, 0, array11, 148, 8);
				return DVMEGAAMBE.bitToByteConvevrt(array11);
			case 131:
				Buffer.BlockCopy(array5, 0, array11, 108, 8);
				Buffer.BlockCopy(array6, 0, array11, 148, 8);
				return DVMEGAAMBE.bitToByteConvevrt(array11);
			case 132:
				Buffer.BlockCopy(array7, 0, array11, 108, 8);
				Buffer.BlockCopy(array8, 0, array11, 148, 8);
				return DVMEGAAMBE.bitToByteConvevrt(array11);
			case 133:
				Buffer.BlockCopy(array9, 0, array11, 108, 8);
				Buffer.BlockCopy(array10, 0, array11, 148, 8);
				return DVMEGAAMBE.bitToByteConvevrt(array11);
			default:
				return voice;
			}
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00009D6C File Offset: 0x00007F6C
		private static void fromDMR(byte[] voice, byte type)
		{
			byte[] array = new byte[37];
			array[0] = 224;
			array[1] = 37;
			array[2] = 26;
			array[3] = type;
			if (type == 24)
			{
				array[3] = 65;
			}
			if (type == 4)
			{
				array[3] = 32;
			}
			if (type == 16)
			{
				array[3] = 1;
			}
			if (type == 32)
			{
				array[3] = 2;
			}
			if (type == 48)
			{
				array[3] = 3;
			}
			if (type == 64)
			{
				array[3] = 4;
			}
			if (type == 80)
			{
				array[3] = 5;
			}
			if (type == 40)
			{
				array[3] = 66;
			}
			if (type == 161)
			{
				array[3] = 65;
			}
			if (type == 144)
			{
				array[3] = 32;
			}
			if (type == 129)
			{
				array[3] = 1;
			}
			if (type == 130)
			{
				array[3] = 2;
			}
			if (type == 131)
			{
				array[3] = 3;
			}
			if (type == 132)
			{
				array[3] = 4;
			}
			if (type == 133)
			{
				array[3] = 5;
			}
			if (type == 162)
			{
				array[3] = 66;
			}
			if (type == 225)
			{
				array[3] = 65;
			}
			if (type == 208)
			{
				array[3] = 32;
			}
			if (type == 193)
			{
				array[3] = 1;
			}
			if (type == 194)
			{
				array[3] = 2;
			}
			if (type == 195)
			{
				array[3] = 3;
			}
			if (type == 196)
			{
				array[3] = 4;
			}
			if (type == 197)
			{
				array[3] = 5;
			}
			if (type == 226)
			{
				array[3] = 66;
			}
			Buffer.BlockCopy(voice, 0, array, 4, 33);
			productSelector.fromInternetDMR(array);
		}

		// Token: 0x06000106 RID: 262 RVA: 0x00009EB8 File Offset: 0x000080B8
		private static bool canStream()
		{
			if (information.stream_modus == information.MODUS.IDLE)
			{
				DVMEGASerial.setmode2DMR();
				information.stream_modus = information.MODUS.DMR;
			}
			if (information.stream_modus == information.MODUS.DMR)
			{
				modeTimer.resetTimer();
				return true;
			}
			return false;
		}

		// Token: 0x06000107 RID: 263 RVA: 0x00009EDC File Offset: 0x000080DC
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
					if (@string.Substring(0, 4).Equals("DMRD") && num == 55)
					{
						long num2 = (long)((long)array[8] << 16);
						num2 += (long)((long)array[9] << 8);
						num2 += (long)((ulong)array[10]);
						long num3 = (long)((long)array[5] << 16);
						num3 += (long)((long)array[6] << 8);
						num3 += (long)((ulong)array[7]);
						int num4 = (int)(array[16] & byte.MaxValue & byte.MaxValue) | ((int)(array[17] & byte.MaxValue & byte.MaxValue) << 8) | ((int)(array[18] & byte.MaxValue) << 16) | ((int)(array[19] & byte.MaxValue) << 24);
						if (DMRPlus.canStream())
						{
							information.hisDMRID = num3.ToString();
							information.hisDMRdest = num2.ToString();
							if (array[15] != 161 || array[15] != 162)
							{
								TimerRXTX.TX();
							}
							byte[] array2 = new byte[33];
							Buffer.BlockCopy(array, 20, array2, 0, 33);
							DMRPlus.fromDMR(DMRPlus.rewriteToColorCode1(array2, array[15]), array[15]);
						}
						ShowCallTimer.resetTimerDMR(num3.ToString(), num4);
					}
					else if (@string.Substring(0, 6).Equals("MSTNAK"))
					{
						if (DMRPlus.m_status == DMRPlus.STATUS.RUNNING)
						{
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRPlus.ChangeStatusText("DMR master is restarting");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRPlus.ChangeStatusText("DMRマスターは再起動中");
								break;
							case information.LANGUAGE.CHINEES:
								DMRPlus.ChangeStatusText("DMR master is restarting");
								break;
							case information.LANGUAGE.KOREAN:
								DMRPlus.ChangeStatusText("DMR 마스터 재가동중");
								break;
							}
							DMRPlus.m_status = DMRPlus.STATUS.W_LOGIN;
							DMRPlus.m_timeoutTimer.start();
							DMRPlus.m_retryTimer.start();
						}
						else
						{
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRPlus.ChangeStatusText("Login to master failed");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRPlus.ChangeStatusText("DMRへのログイン失敗");
								break;
							case information.LANGUAGE.CHINEES:
								DMRPlus.ChangeStatusText("Login to master failed");
								break;
							case information.LANGUAGE.KOREAN:
								DMRPlus.ChangeStatusText("DMR 마스터로그인실패");
								break;
							}
							DMRPlus.close();
							DMRPlus.m_status = DMRPlus.STATUS.DISCONNECTED;
							DMRPlus.m_timeoutTimer.stop();
							DMRPlus.m_retryTimer.stop();
						}
					}
					else if (@string.Substring(0, 6).Equals("MSTACK"))
					{
						switch (DMRPlus.m_status)
						{
						case DMRPlus.STATUS.W_LOGIN:
						{
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRPlus.ChangeStatusText("Waiting for login");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRPlus.ChangeStatusText("DMRへのログイン待ち");
								break;
							case information.LANGUAGE.CHINEES:
								DMRPlus.ChangeStatusText("Waiting for login");
								break;
							case information.LANGUAGE.KOREAN:
								DMRPlus.ChangeStatusText("DMR 로그인 대기");
								break;
							}
							byte[] array3 = new byte[array.Length - 10];
							Buffer.BlockCopy(array, 10, array3, 0, array.Length - 10);
							DMRPlus.writeAuthorisation();
							DMRPlus.m_status = DMRPlus.STATUS.W_AUTHORISATION;
							DMRPlus.m_timeoutTimer.start();
							DMRPlus.m_retryTimer.start();
							break;
						}
						case DMRPlus.STATUS.W_AUTHORISATION:
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRPlus.ChangeStatusText("Waiting for authorisation");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRPlus.ChangeStatusText("DMRへの認証待ち");
								break;
							case information.LANGUAGE.CHINEES:
								DMRPlus.ChangeStatusText("Waiting for authorisation");
								break;
							case information.LANGUAGE.KOREAN:
								DMRPlus.ChangeStatusText("DMR 승인 대기");
								break;
							}
							DMRPlus.writeConfig();
							DMRPlus.m_status = DMRPlus.STATUS.W_CONFIG;
							DMRPlus.m_timeoutTimer.start();
							DMRPlus.m_retryTimer.start();
							break;
						case DMRPlus.STATUS.W_CONFIG:
							DMRPlus.m_status = DMRPlus.STATUS.RUNNING;
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRPlus.ChangeStatusText("Logged in to DMR");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRPlus.ChangeStatusText("DMRへログイン完了");
								break;
							case information.LANGUAGE.CHINEES:
								DMRPlus.ChangeStatusText("Logged in to DMR");
								break;
							case information.LANGUAGE.KOREAN:
								DMRPlus.ChangeStatusText("로그인 완료");
								break;
							}
							DMRPlus.m_timeoutTimer.start();
							DMRPlus.m_retryTimer.start();
							if (DMRPlus.isReconnected)
							{
								DMRPlus.isReconnected = false;
							}
							break;
						}
					}
					else if (@string.Substring(0, 6).Equals("RPTACK"))
					{
						switch (DMRPlus.m_status)
						{
						case DMRPlus.STATUS.W_LOGIN:
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRPlus.ChangeStatusText("Logging in..");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRPlus.ChangeStatusText("DMRへログイン完了");
								break;
							case information.LANGUAGE.CHINEES:
								DMRPlus.ChangeStatusText("Logging in..");
								break;
							case information.LANGUAGE.KOREAN:
								DMRPlus.ChangeStatusText("DMR 마스터 로그인중");
								break;
							}
							DMRPlus.saltBytes = new byte[4];
							Buffer.BlockCopy(array, 6, DMRPlus.saltBytes, 0, 4);
							DMRPlus.writeAuthorisation();
							DMRPlus.m_status = DMRPlus.STATUS.W_AUTHORISATION;
							DMRPlus.m_timeoutTimer.start();
							DMRPlus.m_retryTimer.start();
							break;
						case DMRPlus.STATUS.W_AUTHORISATION:
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRPlus.ChangeStatusText("Waiting for authorisation");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRPlus.ChangeStatusText("DMRへの認証待ち");
								break;
							case information.LANGUAGE.CHINEES:
								DMRPlus.ChangeStatusText("Waiting for authorisation");
								break;
							case information.LANGUAGE.KOREAN:
								DMRPlus.ChangeStatusText("DMR 승인 대기");
								break;
							}
							DMRPlus.writeConfig();
							DMRPlus.m_status = DMRPlus.STATUS.W_CONFIG;
							DMRPlus.m_timeoutTimer.start();
							DMRPlus.m_retryTimer.start();
							break;
						case DMRPlus.STATUS.W_CONFIG:
							DMRPlus.m_status = DMRPlus.STATUS.RUNNING;
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRPlus.ChangeStatusText("Logged in...");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRPlus.ChangeStatusText("DMRへログイン完了");
								break;
							case information.LANGUAGE.CHINEES:
								DMRPlus.ChangeStatusText("Logged in...");
								break;
							case information.LANGUAGE.KOREAN:
								DMRPlus.ChangeStatusText("DMR 로그인 완료");
								break;
							}
							DMRPlus.m_timeoutTimer.start();
							DMRPlus.m_retryTimer.start();
							DMRPlus.writeOptions();
							if (DMRPlus.isReconnected)
							{
								DMRPlus.isReconnected = false;
							}
							break;
						}
					}
					else if (@string.Substring(0, 5).Equals("MSTCL"))
					{
						switch (information.m_language)
						{
						case information.LANGUAGE.ENGLISH:
							DMRPlus.ChangeStatusText("Master went down");
							break;
						case information.LANGUAGE.JAPANESE:
							DMRPlus.ChangeStatusText("DMRマスターとの切断");
							break;
						case information.LANGUAGE.CHINEES:
							DMRPlus.ChangeStatusText("Master went down");
							break;
						case information.LANGUAGE.KOREAN:
							DMRPlus.ChangeStatusText("DMR 마스터서버 다운");
							break;
						}
						DMRPlus.reconnect();
					}
					else if (@string.Substring(0, 7).Equals("MSTPONG"))
					{
						DMRPlus.m_timeoutTimer.start();
					}
					else if (@string.Substring(0, 5).Equals("MSTCO"))
					{
						switch (information.m_language)
						{
						case information.LANGUAGE.ENGLISH:
							DMRPlus.ChangeStatusText("Master went down");
							break;
						case information.LANGUAGE.JAPANESE:
							DMRPlus.ChangeStatusText("DMRマスターとの切断");
							break;
						case information.LANGUAGE.CHINEES:
							DMRPlus.ChangeStatusText("Master went down");
							break;
						case information.LANGUAGE.KOREAN:
							DMRPlus.ChangeStatusText("DMR 마스터서버 다운");
							break;
						}
					}
				}
				udpClient.BeginReceive(new AsyncCallback(DMRPlus.receiveData), ar.AsyncState);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000108 RID: 264 RVA: 0x0000A5E0 File Offset: 0x000087E0
		public static void close()
		{
			if (DMRPlus.udpClient != null)
			{
				try
				{
					DMRPlus.m_status = DMRPlus.STATUS.DISCONNECTED;
					DMRPlus.logout();
					DMRPlus._timer.Dispose();
					DMRPlus.udpClient.Client.Shutdown(SocketShutdown.Receive);
					DMRPlus.udpClient.Close();
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DMRPlus.ChangeStatusText("Logged Out");
						break;
					case information.LANGUAGE.JAPANESE:
						DMRPlus.ChangeStatusText("DMRマスターログアウト");
						break;
					case information.LANGUAGE.CHINEES:
						DMRPlus.ChangeStatusText("Logged Out");
						break;
					case information.LANGUAGE.KOREAN:
						DMRPlus.ChangeStatusText("로그아웃");
						break;
					}
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x04000075 RID: 117
		private static bool isReconnected = false;

		// Token: 0x04000076 RID: 118
		private static bool startLogin = true;

		// Token: 0x04000077 RID: 119
		private static Timert m_timeoutTimer;

		// Token: 0x04000078 RID: 120
		private static Timert m_retryTimer;

		// Token: 0x04000079 RID: 121
		private static long currenttime = 0L;

		// Token: 0x0400007A RID: 122
		private static long oldtime = 0L;

		// Token: 0x0400007B RID: 123
		private static int ms = 0;

		// Token: 0x0400007C RID: 124
		private static int PORT = 55555;

		// Token: 0x0400007D RID: 125
		private static string HOSTNAME;

		// Token: 0x0400007E RID: 126
		private static string PASSWORD;

		// Token: 0x0400007F RID: 127
		private static byte[] saltBytes;

		// Token: 0x04000080 RID: 128
		private static DMRPlus.STATUS m_status = DMRPlus.STATUS.DISCONNECTED;

		// Token: 0x04000081 RID: 129
		private static UdpClient udpClient;

		// Token: 0x04000082 RID: 130
		private static string salt;

		// Token: 0x04000083 RID: 131
		private static Timer _timer;

		// Token: 0x04000084 RID: 132
		private static int TIME_INTERVAL_IN_MILLISECONDS = 5000;

		// Token: 0x02000055 RID: 85
		public enum STATUS
		{
			// Token: 0x040004CA RID: 1226
			DISCONNECTED,
			// Token: 0x040004CB RID: 1227
			W_LOGIN,
			// Token: 0x040004CC RID: 1228
			W_AUTHORISATION,
			// Token: 0x040004CD RID: 1229
			W_CONFIG,
			// Token: 0x040004CE RID: 1230
			RUNNING
		}

		// Token: 0x02000056 RID: 86
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040004CF RID: 1231
			public static TimerCallback <0>__writePing;
		}
	}
}
