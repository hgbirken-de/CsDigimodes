using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BlueDV
{
	// Token: 0x02000015 RID: 21
	internal class DMRconnection
	{
		// Token: 0x1400000E RID: 14
		// (add) Token: 0x0600010A RID: 266 RVA: 0x0000A6C8 File Offset: 0x000088C8
		// (remove) Token: 0x0600010B RID: 267 RVA: 0x0000A6FC File Offset: 0x000088FC
		public static event EventHandler ThresholdReached;

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600010C RID: 268 RVA: 0x0000A72F File Offset: 0x0000892F
		// (set) Token: 0x0600010D RID: 269 RVA: 0x0000A736 File Offset: 0x00008936
		public static string StatusText { get; private set; }

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x0600010E RID: 270 RVA: 0x0000A740 File Offset: 0x00008940
		// (remove) Token: 0x0600010F RID: 271 RVA: 0x0000A774 File Offset: 0x00008974
		public static event EventHandler StatusTextChanged;

		// Token: 0x06000110 RID: 272 RVA: 0x0000A7A8 File Offset: 0x000089A8
		protected virtual void OnThresholdReached(EventArgs e)
		{
			EventHandler thresholdReached = DMRconnection.ThresholdReached;
			if (thresholdReached != null)
			{
				thresholdReached(this, e);
			}
		}

		// Token: 0x06000112 RID: 274 RVA: 0x0000A7C8 File Offset: 0x000089C8
		public static void open()
		{
			DMRconnection.m_status = DMRconnection.STATUS.W_LOGIN;
			DMRconnection.m_timeoutTimer.start();
			DMRconnection.m_retryTimer.start();
			TimerCallback timerCallback;
			if ((timerCallback = DMRconnection.<>O.<0>__writePing) == null)
			{
				timerCallback = (DMRconnection.<>O.<0>__writePing = new TimerCallback(DMRconnection.writePing));
			}
			DMRconnection._timer = new Timer(timerCallback, null, DMRconnection.TIME_INTERVAL_IN_MILLISECONDS, -1);
			DMRconnection.writeLogin();
			DMRconnection.currenttime = DateTime.Now.Ticks / 10000L;
			DMRconnection.ms = (int)(DMRconnection.currenttime - DMRconnection.oldtime);
			DMRconnection.oldtime = DMRconnection.currenttime;
		}

		// Token: 0x06000113 RID: 275 RVA: 0x0000A854 File Offset: 0x00008A54
		public static void reconnect()
		{
			try
			{
				DMRconnection.udpClient.Close();
				DMRconnection.udpClient = new UdpClient();
				DMRconnection.udpClient.Connect(DMRconnection.HOSTNAME, DMRconnection.PORT);
				DMRconnection.udpClient.BeginReceive(new AsyncCallback(DMRconnection.receiveData), DMRconnection.udpClient);
				DMRconnection.m_status = DMRconnection.STATUS.W_LOGIN;
				DMRconnection.m_timeoutTimer.start();
				DMRconnection.m_retryTimer.start();
				DMRconnection.writeLogin();
				DMRconnection.currenttime = DateTime.Now.Ticks / 10000L;
				DMRconnection.ms = (int)(DMRconnection.currenttime - DMRconnection.oldtime);
				DMRconnection.oldtime = DMRconnection.currenttime;
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000114 RID: 276 RVA: 0x0000A910 File Offset: 0x00008B10
		public static void connect(string hostname)
		{
			DMRconnection.HOSTNAME = hostname;
			try
			{
				DMRconnection.m_retryTimer = new Timert(1000, 5, 0);
				DMRconnection.m_timeoutTimer = new Timert(5000, 5, 0);
				DMRconnection.udpClient = new UdpClient();
				DMRconnection.udpClient.Connect(DMRconnection.HOSTNAME, DMRconnection.PORT);
			}
			catch (SocketException ex)
			{
				if (ex.ErrorCode == 10048)
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DMRconnection.ChangeStatusText("ERR: DMR port in use");
						break;
					case information.LANGUAGE.JAPANESE:
						DMRconnection.ChangeStatusText("エラー：DMRポート使用中");
						break;
					case information.LANGUAGE.CHINEES:
						DMRconnection.ChangeStatusText("ERR: DMR port in use");
						break;
					case information.LANGUAGE.KOREAN:
						DMRconnection.ChangeStatusText("에러: DMR포트 사용중");
						break;
					}
				}
				else if (ex.ErrorCode == 10060)
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DMRconnection.ChangeStatusText("ERR: Conn timed out");
						break;
					case information.LANGUAGE.JAPANESE:
						DMRconnection.ChangeStatusText("エラー：DMRタイムアウト");
						break;
					case information.LANGUAGE.CHINEES:
						DMRconnection.ChangeStatusText("ERR: Conn timed out");
						break;
					case information.LANGUAGE.KOREAN:
						DMRconnection.ChangeStatusText("ERR: DMR 마스터연결 시간초과");
						break;
					}
				}
				else if (ex.ErrorCode == 10013)
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DMRconnection.ChangeStatusText("ERR: DMR port PD");
						break;
					case information.LANGUAGE.JAPANESE:
						DMRconnection.ChangeStatusText("エラー：DMRマスターポート使用中");
						break;
					case information.LANGUAGE.CHINEES:
						DMRconnection.ChangeStatusText("ERR: DMR port PD");
						break;
					case information.LANGUAGE.KOREAN:
						DMRconnection.ChangeStatusText("ERR: DMR port PD");
						break;
					}
				}
				else
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DMRconnection.ChangeStatusText("ERR: " + ex.ErrorCode.ToString());
						break;
					case information.LANGUAGE.JAPANESE:
						DMRconnection.ChangeStatusText("エラー：" + ex.ErrorCode.ToString());
						break;
					case information.LANGUAGE.CHINEES:
						DMRconnection.ChangeStatusText("ERR: " + ex.ErrorCode.ToString());
						break;
					case information.LANGUAGE.KOREAN:
						DMRconnection.ChangeStatusText("ERR: " + ex.ErrorCode.ToString());
						break;
					}
				}
				if (DMRconnection.udpClient != null)
				{
					DMRconnection.udpClient.Close();
				}
				DMRconnection.m_retryTimer.stop();
				DMRconnection.m_timeoutTimer.stop();
				return;
			}
			try
			{
				DMRconnection.udpClient.Connect(DMRconnection.HOSTNAME, DMRconnection.PORT);
				DMRconnection.udpClient.BeginReceive(new AsyncCallback(DMRconnection.receiveData), DMRconnection.udpClient);
				DMRconnection.open();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex2)
			{
				DMRconnection.ChangeStatusText("ERR :" + ex2.ToString());
				switch (information.m_language)
				{
				}
			}
		}

		// Token: 0x06000115 RID: 277 RVA: 0x0000AC2C File Offset: 0x00008E2C
		private static void ChangeStatusText(string text)
		{
			DMRconnection.StatusText = text;
			EventHandler statusTextChanged = DMRconnection.StatusTextChanged;
			if (statusTextChanged != null)
			{
				statusTextChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000116 RID: 278 RVA: 0x0000AC54 File Offset: 0x00008E54
		public static void sendRAW(byte[] data, int length)
		{
			try
			{
				DMRconnection.udpClient.Send(data, length);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000117 RID: 279 RVA: 0x0000AC84 File Offset: 0x00008E84
		public static void writeLogin()
		{
			switch (information.m_language)
			{
			case information.LANGUAGE.ENGLISH:
				DMRconnection.ChangeStatusText("Sending DMR Login");
				break;
			case information.LANGUAGE.JAPANESE:
				DMRconnection.ChangeStatusText("DMRへログイン開始");
				break;
			case information.LANGUAGE.CHINEES:
				DMRconnection.ChangeStatusText("Sending DMR Login");
				break;
			case information.LANGUAGE.KOREAN:
				DMRconnection.ChangeStatusText("DMR 로그인 전송");
				break;
			}
			byte[] bytes = Encoding.ASCII.GetBytes("RPTL" + utils.dmrid2hexstring4(information.myDMRID));
			if (DMRconnection.udpClient.Client == null)
			{
				return;
			}
			DMRconnection.sendRAW(bytes, bytes.Length);
		}

		// Token: 0x06000118 RID: 280 RVA: 0x0000AD14 File Offset: 0x00008F14
		public static void cancelStream()
		{
			byte[] bytes = Encoding.ASCII.GetBytes("RPTINTR" + utils.dmrid2hexstring4(information.myDMRID) + ":0");
			if (DMRconnection.udpClient.Client == null)
			{
				return;
			}
			DMRconnection.sendRAW(bytes, bytes.Length);
		}

		// Token: 0x06000119 RID: 281 RVA: 0x0000AD5C File Offset: 0x00008F5C
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

		// Token: 0x0600011A RID: 282 RVA: 0x0000ADB4 File Offset: 0x00008FB4
		public static void writeAuthorisation()
		{
			switch (information.m_language)
			{
			case information.LANGUAGE.ENGLISH:
				DMRconnection.ChangeStatusText("Sending authorisation");
				break;
			case information.LANGUAGE.JAPANESE:
				DMRconnection.ChangeStatusText("DMRの認証開始");
				break;
			case information.LANGUAGE.CHINEES:
				DMRconnection.ChangeStatusText("Sending authorisation");
				break;
			case information.LANGUAGE.KOREAN:
				DMRconnection.ChangeStatusText("DMR 승인 전송");
				break;
			}
			string text = "RPTK" + utils.dmrid2hexstring4(information.myDMRID) + DMRconnection.getHashSha256(DMRconnection.salt + information.myDMRPassword);
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			DMRconnection.sendRAW(bytes, bytes.Length);
		}

		// Token: 0x0600011B RID: 283 RVA: 0x0000AE4C File Offset: 0x0000904C
		private static void writeConfig()
		{
			string text = "RPTC";
			text += string.Format("{0,-8}", information.myCall);
			text += utils.dmrid2hexstring4(information.myDMRID);
			text += string.Format("{0,-9}", information.myFREQ);
			text += string.Format("{0,-9}", information.myFREQ);
			text += "01";
			text += "01";
			text += string.Format("{0,-8}", information.latitude);
			text += string.Format("{0,-9}", information.longitude);
			text += information.height.PadLeft(3, '0');
			text += string.Format("{0,-20}", information.location);
			text += string.Format("{0,-20}", information.description);
			text += string.Format("{0,-124}", information.URL);
			string text2 = Regex.Replace(Assembly.GetExecutingAssembly().GetName().Version.ToString() + "-" + information.myDVMEGAVersion, "[^A-Za-z0-9 ._-]", "");
			text += string.Format("{0,-40}", text2);
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
			int length = text.Length;
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			DMRconnection.sendRAW(bytes, bytes.Length);
		}

		// Token: 0x0600011C RID: 284 RVA: 0x0000B014 File Offset: 0x00009214
		public static void writePing(object o)
		{
			DMRconnection.currenttime = DateTime.Now.Ticks / 10000L;
			DMRconnection.ms = (int)(DMRconnection.currenttime - DMRconnection.oldtime);
			DMRconnection.oldtime = DMRconnection.currenttime;
			if (DMRconnection.m_status == DMRconnection.STATUS.RUNNING)
			{
				string text = "MSTPING" + utils.dmrid2hexstring4(information.myDMRID);
				byte[] bytes = Encoding.ASCII.GetBytes(text);
				DMRconnection.sendRAW(bytes, bytes.Length);
			}
			if (DMRconnection.m_timeoutTimer.isRunning() && DMRconnection.m_timeoutTimer.hasExpired())
			{
				DMRconnection.isReconnected = true;
				DMRconnection.reconnect();
			}
			DMRconnection.m_retryTimer.clock(DMRconnection.ms);
			if ((DMRconnection.m_retryTimer.isRunning() && DMRconnection.m_retryTimer.hasExpired()) || DMRconnection.startLogin)
			{
				switch (DMRconnection.m_status)
				{
				case DMRconnection.STATUS.W_LOGIN:
					DMRconnection.writeLogin();
					break;
				case DMRconnection.STATUS.W_AUTHORISATION:
					DMRconnection.writeAuthorisation();
					break;
				case DMRconnection.STATUS.W_CONFIG:
					DMRconnection.writeConfig();
					break;
				case DMRconnection.STATUS.RUNNING:
					DMRconnection.startLogin = false;
					break;
				}
			}
			DMRconnection.m_timeoutTimer.clock(DMRconnection.ms);
			if (DMRconnection.m_timeoutTimer.isRunning() && DMRconnection.m_timeoutTimer.hasExpired())
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					DMRconnection.ChangeStatusText("Connection timed out!");
					break;
				case information.LANGUAGE.JAPANESE:
					DMRconnection.ChangeStatusText("DMRマスターの接続がタイムアウト！");
					break;
				case information.LANGUAGE.CHINEES:
					DMRconnection.ChangeStatusText("Connection timed out!");
					break;
				case information.LANGUAGE.KOREAN:
					DMRconnection.ChangeStatusText("DMR 마스터연결 시간초과");
					break;
				}
				DMRconnection.isReconnected = true;
				DMRconnection.reconnect();
			}
			DMRconnection.currenttime = DateTime.Now.Ticks / 10000L;
			DMRconnection.ms = (int)(DMRconnection.currenttime - DMRconnection.oldtime);
			DMRconnection.oldtime = DMRconnection.currenttime;
			DMRconnection._timer.Change(DMRconnection.TIME_INTERVAL_IN_MILLISECONDS, -1);
		}

		// Token: 0x0600011D RID: 285 RVA: 0x0000B1D8 File Offset: 0x000093D8
		public static void writeping2()
		{
			string text = "MSTPING" + utils.dmrid2hexstring4(information.myDMRID);
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			DMRconnection.sendRAW(bytes, bytes.Length);
		}

		// Token: 0x0600011E RID: 286 RVA: 0x0000B210 File Offset: 0x00009410
		public static void logout()
		{
			DMRconnection.m_timeoutTimer.start();
			string text = "RPTCL" + utils.dmrid2hexstring4(information.myDMRID);
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			DMRconnection.sendRAW(bytes, bytes.Length);
		}

		// Token: 0x0600011F RID: 287 RVA: 0x0000B24F File Offset: 0x0000944F
		private static bool IsBitSet(byte b, byte nPos)
		{
			return new BitArray(new byte[] { b })[(int)nPos];
		}

		// Token: 0x06000120 RID: 288 RVA: 0x0000B268 File Offset: 0x00009468
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
			if (type <= 32)
			{
				if (type == 16)
				{
					Buffer.BlockCopy(array, 0, array11, 108, 8);
					Buffer.BlockCopy(array2, 0, array11, 148, 8);
					return DVMEGAAMBE.bitToByteConvevrt(array11);
				}
				if (type == 32)
				{
					Buffer.BlockCopy(array3, 0, array11, 108, 8);
					Buffer.BlockCopy(array4, 0, array11, 148, 8);
					return DVMEGAAMBE.bitToByteConvevrt(array11);
				}
			}
			else
			{
				if (type == 48)
				{
					Buffer.BlockCopy(array5, 0, array11, 108, 8);
					Buffer.BlockCopy(array6, 0, array11, 148, 8);
					return DVMEGAAMBE.bitToByteConvevrt(array11);
				}
				if (type == 64)
				{
					Buffer.BlockCopy(array7, 0, array11, 108, 8);
					Buffer.BlockCopy(array8, 0, array11, 148, 8);
					return DVMEGAAMBE.bitToByteConvevrt(array11);
				}
				if (type == 80)
				{
					Buffer.BlockCopy(array9, 0, array11, 108, 8);
					Buffer.BlockCopy(array10, 0, array11, 148, 8);
					return DVMEGAAMBE.bitToByteConvevrt(array11);
				}
			}
			return voice;
		}

		// Token: 0x06000121 RID: 289 RVA: 0x0000B418 File Offset: 0x00009618
		private static void fromDMR(byte[] voice, byte sequence, byte type)
		{
			DecodeDMR.GetEMBfromVoice(voice);
			for (int i = 0; i < 48; i++)
			{
			}
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
			if (type == 26)
			{
				array[3] = 65;
			}
			if (type == 6)
			{
				array[3] = 32;
			}
			if (type == 18)
			{
				array[3] = 1;
			}
			if (type == 34)
			{
				array[3] = 2;
			}
			if (type == 50)
			{
				array[3] = 3;
			}
			if (type == 66)
			{
				array[3] = 4;
			}
			if (type == 82)
			{
				array[3] = 5;
			}
			if (type == 42)
			{
				array[3] = 66;
			}
			byte[] array2 = new byte[7];
			Buffer.BlockCopy(voice, 13, array2, 0, 7);
			if (type <= 32)
			{
				if (type <= 16)
				{
					if (type != 4)
					{
						if (type == 16)
						{
							if (!DMRconnection.stopRecord)
							{
								DMRconnection.fragment1 = array2;
							}
						}
					}
					else if (!DMRconnection.stopRecord)
					{
						DMRconnection.syncfragment = array2;
					}
				}
				else if (type != 24)
				{
					if (type == 32)
					{
						if (!DMRconnection.stopRecord)
						{
							DMRconnection.fragment2 = array2;
						}
						if (DMRconnection.IsBitSet(array2[1], 3))
						{
							DMRconnection.foundHC = true;
						}
						if (DMRconnection.IsBitSet(array2[2], 3))
						{
							DMRconnection.foundHC = true;
						}
					}
				}
				else
				{
					DMRconnection.stopRecord = false;
				}
			}
			else if (type <= 48)
			{
				if (type != 40)
				{
					if (type == 48)
					{
						if (!DMRconnection.stopRecord)
						{
							DMRconnection.fragment3 = array2;
						}
					}
				}
				else
				{
					DMRconnection.stopRecord = false;
					DMRconnection.foundHC = false;
				}
			}
			else if (type != 64)
			{
				if (type == 80)
				{
					if (!DMRconnection.stopRecord)
					{
						DMRconnection.nullfragment = array2;
					}
					DMRconnection.stopRecord = true;
				}
			}
			else if (!DMRconnection.stopRecord)
			{
				DMRconnection.fragment4 = array2;
			}
			if (DMRconnection.foundHC && information.noInbandData)
			{
				if (type <= 32)
				{
					if (type != 4)
					{
						if (type != 16)
						{
							if (type == 32)
							{
								voice[13] = (byte)((voice[13] >> 4 << 4) ^ (int)(DMRconnection.fragment2[0] & 15));
								voice[14] = DMRconnection.fragment2[1];
								voice[15] = DMRconnection.fragment2[2];
								voice[16] = DMRconnection.fragment2[3];
								voice[17] = DMRconnection.fragment2[4];
								voice[18] = DMRconnection.fragment2[5];
								voice[19] = (voice[19] & 15) ^ (DMRconnection.fragment2[6] & 240);
							}
						}
						else
						{
							voice[13] = (byte)((voice[13] >> 4 << 4) ^ (int)(DMRconnection.fragment1[0] & 15));
							voice[14] = DMRconnection.fragment1[1];
							voice[15] = DMRconnection.fragment1[2];
							voice[16] = DMRconnection.fragment1[3];
							voice[17] = DMRconnection.fragment1[4];
							voice[18] = DMRconnection.fragment1[5];
							voice[19] = (voice[19] & 15) ^ (DMRconnection.fragment1[6] & 240);
						}
					}
					else
					{
						voice[13] = (byte)((voice[13] >> 4 << 4) ^ (int)(DMRconnection.syncfragment[0] & 15));
						voice[14] = DMRconnection.syncfragment[1];
						voice[15] = DMRconnection.syncfragment[2];
						voice[16] = DMRconnection.syncfragment[3];
						voice[17] = DMRconnection.syncfragment[4];
						voice[18] = DMRconnection.syncfragment[5];
						voice[19] = (voice[19] & 15) ^ (DMRconnection.syncfragment[6] & 240);
					}
				}
				else if (type != 48)
				{
					if (type != 64)
					{
						if (type == 80)
						{
							voice[13] = (byte)((voice[13] >> 4 << 4) ^ (int)(DMRconnection.nullfragment[0] & 15));
							voice[14] = DMRconnection.nullfragment[1];
							voice[15] = DMRconnection.nullfragment[2];
							voice[16] = DMRconnection.nullfragment[3];
							voice[17] = DMRconnection.nullfragment[4];
							voice[18] = DMRconnection.nullfragment[5];
							voice[19] = (voice[19] & 15) ^ (DMRconnection.nullfragment[6] & 240);
						}
					}
					else
					{
						voice[13] = (byte)((voice[13] >> 4 << 4) ^ (int)(DMRconnection.fragment4[0] & 15));
						voice[14] = DMRconnection.fragment4[1];
						voice[15] = DMRconnection.fragment4[2];
						voice[16] = DMRconnection.fragment4[3];
						voice[17] = DMRconnection.fragment4[4];
						voice[18] = DMRconnection.fragment4[5];
						voice[19] = (voice[19] & 15) ^ (DMRconnection.fragment4[6] & 240);
					}
				}
				else
				{
					voice[13] = (byte)((voice[13] >> 4 << 4) ^ (int)(DMRconnection.fragment3[0] & 15));
					voice[14] = DMRconnection.fragment3[1];
					voice[15] = DMRconnection.fragment3[2];
					voice[16] = DMRconnection.fragment3[3];
					voice[17] = DMRconnection.fragment3[4];
					voice[18] = DMRconnection.fragment3[5];
					voice[19] = (voice[19] & 15) ^ (DMRconnection.fragment3[6] & 240);
				}
			}
			Buffer.BlockCopy(voice, 0, array, 4, 33);
			productSelector.fromInternetDMR(array);
		}

		// Token: 0x06000122 RID: 290 RVA: 0x0000B8D8 File Offset: 0x00009AD8
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
					if (@string.Substring(0, 4).Equals("DMRD") && num == 53)
					{
						long num2 = (long)((long)array[8] << 16);
						num2 += (long)((long)array[9] << 8);
						num2 += (long)((ulong)array[10]);
						long num3 = (long)((long)array[5] << 16);
						num3 += (long)((long)array[6] << 8);
						num3 += (long)((ulong)array[7]);
						int num4 = (int)(array[16] & byte.MaxValue & byte.MaxValue) | ((int)(array[17] & byte.MaxValue & byte.MaxValue) << 8) | ((int)(array[18] & byte.MaxValue) << 16) | ((int)(array[19] & byte.MaxValue) << 24);
						if (DMRconnection.canStream())
						{
							information.hisDMRID = num3.ToString();
							information.hisDMRdest = num2.ToString();
							byte[] array2 = new byte[33];
							Buffer.BlockCopy(array, 20, array2, 0, 33);
							byte[] array3 = DMRconnection.rewriteToColorCode1(array2, array[15]);
							DMRconnection.fromDMR(array3, array[4], array[15]);
							DMRNETincoming.netIN(array3, array[4], array[15]);
							if (array[15] != 65 || array[15] != 66)
							{
								TimerRXTX.TX();
							}
							if (array[15] != 18 || array[15] != 18)
							{
								TimerRXTX.TX();
							}
						}
						ShowCallTimer.resetTimerDMR(num3.ToString(), num4);
					}
					else if (@string.Substring(0, 6).Equals("MSTNAK"))
					{
						if (DMRconnection.m_status == DMRconnection.STATUS.RUNNING)
						{
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRconnection.ChangeStatusText("DMR master is reconnecting");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRconnection.ChangeStatusText("DMRマスターが再接続中");
								break;
							case information.LANGUAGE.CHINEES:
								DMRconnection.ChangeStatusText("DMR master is reconnecting");
								break;
							case information.LANGUAGE.KOREAN:
								DMRconnection.ChangeStatusText("DMR 마스터 재연결");
								break;
							}
							DMRconnection.m_status = DMRconnection.STATUS.W_LOGIN;
							DMRconnection.m_timeoutTimer.start();
							DMRconnection.m_retryTimer.start();
						}
						else
						{
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRconnection.ChangeStatusText("Login to master failed");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRconnection.ChangeStatusText("DMRへのログイン失敗");
								break;
							case information.LANGUAGE.CHINEES:
								DMRconnection.ChangeStatusText("Login to master failed");
								break;
							case information.LANGUAGE.KOREAN:
								DMRconnection.ChangeStatusText("DMR 마스터로그인실패");
								break;
							}
							DMRconnection.close();
							DMRconnection.m_status = DMRconnection.STATUS.DISCONNECTED;
							DMRconnection.m_timeoutTimer.stop();
							DMRconnection.m_retryTimer.stop();
						}
					}
					else if (@string.Substring(0, 6).Equals("MSTACK"))
					{
						switch (DMRconnection.m_status)
						{
						case DMRconnection.STATUS.W_LOGIN:
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRconnection.ChangeStatusText("Waiting for login");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRconnection.ChangeStatusText("DMRへのログイン待ち");
								break;
							case information.LANGUAGE.CHINEES:
								DMRconnection.ChangeStatusText("Waiting for login");
								break;
							case information.LANGUAGE.KOREAN:
								DMRconnection.ChangeStatusText("DMR 로그인 대기");
								break;
							}
							DMRconnection.salt = @string.Substring(14, 8);
							DMRconnection.writeAuthorisation();
							DMRconnection.m_status = DMRconnection.STATUS.W_AUTHORISATION;
							DMRconnection.m_timeoutTimer.start();
							DMRconnection.m_retryTimer.start();
							break;
						case DMRconnection.STATUS.W_AUTHORISATION:
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRconnection.ChangeStatusText("Waiting for authorisation");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRconnection.ChangeStatusText("DMRへの認証待ち");
								break;
							case information.LANGUAGE.CHINEES:
								DMRconnection.ChangeStatusText("Waiting for authorisation");
								break;
							case information.LANGUAGE.KOREAN:
								DMRconnection.ChangeStatusText("DMR 승인 대기");
								break;
							}
							DMRconnection.writeConfig();
							DMRconnection.m_status = DMRconnection.STATUS.W_CONFIG;
							DMRconnection.m_timeoutTimer.start();
							DMRconnection.m_retryTimer.start();
							break;
						case DMRconnection.STATUS.W_CONFIG:
							DMRconnection.m_status = DMRconnection.STATUS.RUNNING;
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRconnection.ChangeStatusText("Logged in to DMR");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRconnection.ChangeStatusText("DMRへログイン完了");
								break;
							case information.LANGUAGE.CHINEES:
								DMRconnection.ChangeStatusText("Logged in to DMR");
								break;
							case information.LANGUAGE.KOREAN:
								DMRconnection.ChangeStatusText("로그인 완료");
								break;
							}
							DMRconnection.m_timeoutTimer.start();
							DMRconnection.m_retryTimer.start();
							if (DMRconnection.isReconnected)
							{
								DMRconnection.isReconnected = false;
							}
							break;
						}
					}
					else if (@string.Substring(0, 6).Equals("RPTACK"))
					{
						switch (DMRconnection.m_status)
						{
						case DMRconnection.STATUS.W_LOGIN:
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRconnection.ChangeStatusText("Logging in..");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRconnection.ChangeStatusText("DMRマスターへログイン");
								break;
							case information.LANGUAGE.CHINEES:
								DMRconnection.ChangeStatusText("Logging in..");
								break;
							case information.LANGUAGE.KOREAN:
								DMRconnection.ChangeStatusText("DMR 마스터 로그인중");
								break;
							}
							if (@string.Length > 13)
							{
								DMRconnection.salt = @string.Substring(6, 8);
							}
							else
							{
								DMRconnection.salt = "BOGO";
							}
							DMRconnection.writeAuthorisation();
							DMRconnection.m_status = DMRconnection.STATUS.W_AUTHORISATION;
							DMRconnection.m_timeoutTimer.start();
							DMRconnection.m_retryTimer.start();
							break;
						case DMRconnection.STATUS.W_AUTHORISATION:
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRconnection.ChangeStatusText("Waiting for authorisation");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRconnection.ChangeStatusText("DMRへの認証待ち");
								break;
							case information.LANGUAGE.CHINEES:
								DMRconnection.ChangeStatusText("Waiting for authorisation");
								break;
							case information.LANGUAGE.KOREAN:
								DMRconnection.ChangeStatusText("DMR 승인 대기");
								break;
							}
							DMRconnection.writeConfig();
							DMRconnection.m_status = DMRconnection.STATUS.W_CONFIG;
							DMRconnection.m_timeoutTimer.start();
							DMRconnection.m_retryTimer.start();
							break;
						case DMRconnection.STATUS.W_CONFIG:
							DMRconnection.m_status = DMRconnection.STATUS.RUNNING;
							switch (information.m_language)
							{
							case information.LANGUAGE.ENGLISH:
								DMRconnection.ChangeStatusText("Logged in...");
								break;
							case information.LANGUAGE.JAPANESE:
								DMRconnection.ChangeStatusText("DMRへログイン完了");
								break;
							case information.LANGUAGE.CHINEES:
								DMRconnection.ChangeStatusText("Logged in...");
								break;
							case information.LANGUAGE.KOREAN:
								DMRconnection.ChangeStatusText("DMR 로그인 완료");
								break;
							}
							DMRconnection.m_timeoutTimer.start();
							DMRconnection.m_retryTimer.start();
							if (DMRconnection.isReconnected)
							{
								DMRconnection.isReconnected = false;
							}
							break;
						}
					}
					else if (@string.Substring(0, 5).Equals("MSTCL"))
					{
						switch (information.m_language)
						{
						case information.LANGUAGE.ENGLISH:
							DMRconnection.ChangeStatusText("Master went down");
							break;
						case information.LANGUAGE.JAPANESE:
							DMRconnection.ChangeStatusText("DMRマスターとの切断");
							break;
						case information.LANGUAGE.CHINEES:
							DMRconnection.ChangeStatusText("Master went down");
							break;
						case information.LANGUAGE.KOREAN:
							DMRconnection.ChangeStatusText("DMR 마스터서버 다운");
							break;
						}
						DMRconnection.reconnect();
					}
					else if (@string.Substring(0, 7).Equals("RPTPONG"))
					{
						DMRconnection.m_timeoutTimer.start();
					}
				}
				udpClient.BeginReceive(new AsyncCallback(DMRconnection.receiveData), ar.AsyncState);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex)
			{
				DMRconnection.ChangeStatusText("ERR:" + ex.ToString());
				switch (information.m_language)
				{
				}
			}
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00009EB8 File Offset: 0x000080B8
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

		// Token: 0x06000124 RID: 292 RVA: 0x0000BFD8 File Offset: 0x0000A1D8
		public static void close()
		{
			if (DMRconnection.udpClient != null)
			{
				try
				{
					DMRconnection.logout();
					DMRconnection._timer.Dispose();
					DMRconnection.udpClient.Client.Shutdown(SocketShutdown.Receive);
					DMRconnection.udpClient.Close();
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DMRconnection.ChangeStatusText("Logged Out");
						break;
					case information.LANGUAGE.JAPANESE:
						DMRconnection.ChangeStatusText("DMRマスターログアウト");
						break;
					case information.LANGUAGE.CHINEES:
						DMRconnection.ChangeStatusText("Logged Out");
						break;
					case information.LANGUAGE.KOREAN:
						DMRconnection.ChangeStatusText("로그아웃");
						break;
					}
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x04000088 RID: 136
		private static bool isReconnected = false;

		// Token: 0x04000089 RID: 137
		private static bool startLogin = true;

		// Token: 0x0400008A RID: 138
		private static Timert m_timeoutTimer;

		// Token: 0x0400008B RID: 139
		private static Timert m_retryTimer;

		// Token: 0x0400008C RID: 140
		private static long currenttime = 0L;

		// Token: 0x0400008D RID: 141
		private static long oldtime = 0L;

		// Token: 0x0400008E RID: 142
		private static int ms = 0;

		// Token: 0x0400008F RID: 143
		private static int PORT = 62030;

		// Token: 0x04000090 RID: 144
		private static string HOSTNAME;

		// Token: 0x04000091 RID: 145
		private static byte[] syncfragment;

		// Token: 0x04000092 RID: 146
		private static byte[] fragment1;

		// Token: 0x04000093 RID: 147
		private static byte[] fragment2;

		// Token: 0x04000094 RID: 148
		private static byte[] fragment3;

		// Token: 0x04000095 RID: 149
		private static byte[] fragment4;

		// Token: 0x04000096 RID: 150
		private static byte[] nullfragment;

		// Token: 0x04000097 RID: 151
		private static bool foundHC;

		// Token: 0x04000098 RID: 152
		private static bool stopRecord;

		// Token: 0x04000099 RID: 153
		private static DMRconnection.STATUS m_status = DMRconnection.STATUS.DISCONNECTED;

		// Token: 0x0400009A RID: 154
		private static UdpClient udpClient;

		// Token: 0x0400009B RID: 155
		private static string salt;

		// Token: 0x0400009C RID: 156
		private static Timer _timer;

		// Token: 0x0400009D RID: 157
		private static int TIME_INTERVAL_IN_MILLISECONDS = 5000;

		// Token: 0x02000057 RID: 87
		public enum STATUS
		{
			// Token: 0x040004D1 RID: 1233
			DISCONNECTED,
			// Token: 0x040004D2 RID: 1234
			W_LOGIN,
			// Token: 0x040004D3 RID: 1235
			W_AUTHORISATION,
			// Token: 0x040004D4 RID: 1236
			W_CONFIG,
			// Token: 0x040004D5 RID: 1237
			RUNNING
		}

		// Token: 0x02000058 RID: 88
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040004D6 RID: 1238
			public static TimerCallback <0>__writePing;
		}
	}
}
