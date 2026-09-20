using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BlueDV
{
	// Token: 0x02000010 RID: 16
	internal class DCSconnection
	{
		// Token: 0x17000017 RID: 23
		// (get) Token: 0x060000B5 RID: 181 RVA: 0x000063BD File Offset: 0x000045BD
		// (set) Token: 0x060000B6 RID: 182 RVA: 0x000063C4 File Offset: 0x000045C4
		public static string StatusTextDCS { get; private set; }

		// Token: 0x1400000B RID: 11
		// (add) Token: 0x060000B7 RID: 183 RVA: 0x000063CC File Offset: 0x000045CC
		// (remove) Token: 0x060000B8 RID: 184 RVA: 0x00006400 File Offset: 0x00004600
		public static event EventHandler StatusTextChangedDCS;

		// Token: 0x060000B9 RID: 185 RVA: 0x00006434 File Offset: 0x00004634
		private static string getDcsLoginString()
		{
			DCSconnection.ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			string text = string.Concat(new string[]
			{
				information.myCall.PadRight(8),
				information.myDSTARmodule,
				DCSconnection.setDestination.Substring(7),
				"\0",
				DCSconnection.setDestination.Substring(0, 7).PadRight(8),
				"<table border=\"0\" width=\"95%\"><tr><td width=\"4%\"><a href=\"http://www.pa7lim.nl/\"><img border=\"0\" src=\"http://www.pa7lim.nl/bluedvsmall.png\"></td><td width=\"96%\"><font size=\"2\"><b>BLUEDV</b> version ",
				DCSconnection.ApplicationVersion,
				" by PA7LIM running on Windows with DVMEGA by Guus PE1PLM </font></td></td></table>"
			});
			switch (utils.RunningPlatform())
			{
			case utils.Platform.Windows:
				text = string.Concat(new string[]
				{
					information.myCall.PadRight(8),
					information.myDSTARmodule,
					DCSconnection.setDestination.Substring(7),
					"\0",
					DCSconnection.setDestination.Substring(0, 7).PadRight(8),
					"<table border=\"0\" width=\"95%\"><tr><td width=\"4%\"><a href=\"http://www.pa7lim.nl/\"><img border=\"0\" src=\"http://www.pa7lim.nl/bluedvsmall.png\"></td><td width=\"96%\"><font size=\"2\"><b>BLUEDV</b> version ",
					DCSconnection.ApplicationVersion,
					" by PA7LIM running on Windows with DVMEGA by Guus PE1PLM </font></td></td></table>"
				});
				break;
			case utils.Platform.Linux:
				text = string.Concat(new string[]
				{
					information.myCall.PadRight(8),
					information.myDSTARmodule,
					DCSconnection.setDestination.Substring(7),
					"\0",
					DCSconnection.setDestination.Substring(0, 7).PadRight(8),
					"<table border=\"0\" width=\"95%\"><tr><td width=\"4%\"><a href=\"http://www.pa7lim.nl/\"><img border=\"0\" src=\"http://www.pa7lim.nl/bluedvsmall.png\"></td><td width=\"96%\"><font size=\"2\"><b>BLUEDV</b> version ",
					DCSconnection.ApplicationVersion,
					" by PA7LIM running on Linux with DVMEGA by Guus PE1PLM </font></td></td></table>"
				});
				break;
			case utils.Platform.Mac:
				text = string.Concat(new string[]
				{
					information.myCall.PadRight(8),
					information.myDSTARmodule,
					DCSconnection.setDestination.Substring(7),
					"\0",
					DCSconnection.setDestination.Substring(0, 7).PadRight(8),
					"<table border=\"0\" width=\"95%\"><tr><td width=\"4%\"><a href=\"http://www.pa7lim.nl/\"><img border=\"0\" src=\"http://www.pa7lim.nl/bluedvsmall.png\"></td><td width=\"96%\"><font size=\"2\"><b>BLUEDV</b> version ",
					DCSconnection.ApplicationVersion,
					" by PA7LIM running on MAC with DVMEGA by Guus PE1PLM </font></td></td></table>"
				});
				break;
			}
			return text.PadRight(519);
		}

		// Token: 0x060000BA RID: 186 RVA: 0x0000661D File Offset: 0x0000481D
		public static void resetCounters()
		{
			DCSconnection.sessionCounter = 0;
			DCSconnection.smallCounter = 0;
		}

		// Token: 0x060000BB RID: 187 RVA: 0x0000662C File Offset: 0x0000482C
		public static void open()
		{
			DCSconnection.m_status = DCSconnection.STATUS.W_LOGIN;
			DCSconnection.m_timeoutTimer.start();
			DCSconnection.m_retryTimer.start();
			TimerCallback timerCallback;
			if ((timerCallback = DCSconnection.<>O.<0>__writePing) == null)
			{
				timerCallback = (DCSconnection.<>O.<0>__writePing = new TimerCallback(DCSconnection.writePing));
			}
			DCSconnection._timer = new Timer(timerCallback, null, DCSconnection.TIME_INTERVAL_IN_MILLISECONDS, -1);
			DCSconnection.writeLogin();
			DCSconnection.currenttime = DateTime.Now.Ticks / 10000L;
			DCSconnection.ms = (int)(DCSconnection.currenttime - DCSconnection.oldtime);
			DCSconnection.oldtime = DCSconnection.currenttime;
		}

		// Token: 0x060000BC RID: 188 RVA: 0x000066B8 File Offset: 0x000048B8
		public static void makeFrame(byte[] frame, bool EOT)
		{
			byte[] array = new byte[100];
			array[0] = 48;
			array[1] = 48;
			array[2] = 48;
			array[3] = 49;
			array[4] = 0;
			array[5] = 0;
			array[6] = 0;
			Buffer.BlockCopy(DSTARhandler.dstarHeaderBytes, 0, array, 7, 36);
			for (int i = 0; i <= 7; i++)
			{
				array[i + 7] = (byte)DCSconnection.setDestination[i];
			}
			array[43] = DSTARhandler.session_id_byte()[0];
			array[44] = DSTARhandler.session_id_byte()[1];
			Buffer.BlockCopy(frame, 0, array, 46, 12);
			if (EOT)
			{
				array[45] = (byte)DCSconnection.smallCounter | 64;
				array[46] = 85;
				array[47] = 85;
				array[48] = 85;
				array[49] = 85;
				array[50] = 200;
				array[51] = 122;
				array[52] = 0;
				array[53] = 0;
				array[54] = 0;
			}
			else
			{
				array[45] = (byte)DCSconnection.smallCounter;
			}
			array[58] = (byte)(DCSconnection.sessionCounter & 255);
			array[59] = (byte)((DCSconnection.sessionCounter >> 8) & 255);
			array[60] = (byte)((DCSconnection.sessionCounter >> 16) & 255);
			array[61] = 1;
			array[62] = 0;
			array[63] = 33;
			string text = "BlueDV for Windows  ";
			for (int j = 0; j <= 19; j++)
			{
				array[j + 64] = (byte)text[j];
			}
			DCSconnection.sendRAW(array, 100);
			DCSconnection.sessionCounter++;
			DCSconnection.smallCounter++;
			if (DCSconnection.smallCounter == 21)
			{
				DCSconnection.smallCounter = 0;
			}
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00006824 File Offset: 0x00004A24
		private static byte[] GetBytes(string str)
		{
			byte[] array = new byte[str.Length * 2];
			Buffer.BlockCopy(str.ToCharArray(), 0, array, 0, array.Length);
			return array;
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00006854 File Offset: 0x00004A54
		private static string GetString(byte[] bytes)
		{
			char[] array = new char[bytes.Length / 2];
			Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
			return new string(array);
		}

		// Token: 0x060000BF RID: 191 RVA: 0x0000687E File Offset: 0x00004A7E
		public static void unlink()
		{
			DCSconnection.close();
			DCSconnection.linked = false;
			DCSconnection._isStreaming = false;
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00006891 File Offset: 0x00004A91
		public static void link(string reflector, string hostname)
		{
			DCSconnection.HOSTNAME = hostname;
			DCSconnection.setDestination = reflector;
			DCSconnection.connect(DCSconnection.HOSTNAME);
			DCSconnection.linked = true;
			information.connectedDSTARReflector = reflector;
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x000068B8 File Offset: 0x00004AB8
		public static void sendRAW(byte[] data, int length)
		{
			try
			{
				DCSconnection.udpClient.Send(data, length);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x000068E8 File Offset: 0x00004AE8
		private static void writeLogin()
		{
			DCSconnection.sendRAW(Encoding.ASCII.GetBytes(DCSconnection.getDcsLoginString()), 519);
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x00006904 File Offset: 0x00004B04
		private static void ChangeStatusTextDCS(string text)
		{
			DCSconnection.StatusTextDCS = text;
			EventHandler statusTextChangedDCS = DCSconnection.StatusTextChangedDCS;
			if (statusTextChangedDCS != null)
			{
				statusTextChangedDCS(null, EventArgs.Empty);
			}
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x0000692C File Offset: 0x00004B2C
		private static void sendDisconnect()
		{
			try
			{
				string text = information.myCall.PadRight(8) + information.myDSTARmodule.Trim() + " \0" + DCSconnection.setDestination.Substring(0, 7).PadRight(8);
				DCSconnection.sendRAW(Encoding.ASCII.GetBytes(text), 19);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00006994 File Offset: 0x00004B94
		private static void close()
		{
			DCSconnection.resetCounters();
			if (DCSconnection.udpClient != null)
			{
				try
				{
					DCSconnection.sendDisconnect();
					DCSconnection.m_status = DCSconnection.STATUS.DISCONNECTED;
					DCSconnection.udpClient.Client.Shutdown(SocketShutdown.Receive);
					DCSconnection.udpClient.Close();
					DCSconnection._timer.Dispose();
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DCSconnection.ChangeStatusTextDCS("Not linked");
						break;
					case information.LANGUAGE.JAPANESE:
						DCSconnection.ChangeStatusTextDCS("未接続");
						break;
					case information.LANGUAGE.CHINEES:
						DCSconnection.ChangeStatusTextDCS("Not linked");
						break;
					case information.LANGUAGE.KOREAN:
						DCSconnection.ChangeStatusTextDCS("연결안됨");
						break;
					}
					DSTARhandler.talkWoman("@");
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00006A4C File Offset: 0x00004C4C
		public static void reconnect()
		{
			DCSconnection.m_status = DCSconnection.STATUS.W_LOGIN;
			try
			{
				DCSconnection.udpClient.Close();
				DCSconnection.udpClient = new UdpClient(DCSconnection.SRCPORT);
				DCSconnection.udpClient.Connect(DCSconnection.HOSTNAME, DCSconnection.DSTPORT);
				DCSconnection.udpClient.BeginReceive(new AsyncCallback(DCSconnection.receiveData2), DCSconnection.udpClient);
				DCSconnection.m_status = DCSconnection.STATUS.W_LOGIN;
				DCSconnection.m_timeoutTimer.start();
				DCSconnection.m_retryTimer.start();
				DCSconnection.writeLogin();
				DCSconnection.currenttime = DateTime.Now.Ticks / 10000L;
				DCSconnection.ms = (int)(DCSconnection.currenttime - DCSconnection.oldtime);
				DCSconnection.oldtime = DCSconnection.currenttime;
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x00006B20 File Offset: 0x00004D20
		public static void connect(string hostname)
		{
			DCSconnection.HOSTNAME = hostname;
			try
			{
				DCSconnection.m_retryTimer = new Timert(1000, 5, 0);
				DCSconnection.m_timeoutTimer = new Timert(5000, 5, 0);
				DCSconnection.udpClient = new UdpClient(DCSconnection.SRCPORT);
			}
			catch (SocketException)
			{
				if (DCSconnection.udpClient != null)
				{
					DCSconnection.udpClient.Close();
				}
				DCSconnection.m_retryTimer.stop();
				DCSconnection.m_timeoutTimer.stop();
				return;
			}
			try
			{
				DCSconnection.udpClient.Connect(DCSconnection.HOSTNAME, DCSconnection.DSTPORT);
				DCSconnection.udpClient.BeginReceive(new AsyncCallback(DCSconnection.receiveData2), DCSconnection.udpClient);
				DCSconnection.open();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					DCSconnection.ChangeStatusTextDCS("ERR: " + ex.ToString());
					break;
				case information.LANGUAGE.JAPANESE:
					DCSconnection.ChangeStatusTextDCS("エラー： " + ex.ToString());
					break;
				case information.LANGUAGE.CHINEES:
					DCSconnection.ChangeStatusTextDCS("ERR: " + ex.ToString());
					break;
				case information.LANGUAGE.KOREAN:
					DCSconnection.ChangeStatusTextDCS("ERR: " + ex.ToString());
					break;
				}
			}
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x00006C74 File Offset: 0x00004E74
		private static void receiveData2(IAsyncResult ar)
		{
			try
			{
				UdpClient udpClient = (UdpClient)ar.AsyncState;
				IPEndPoint ipendPoint = new IPEndPoint(IPAddress.Any, 0);
				byte[] array = udpClient.EndReceive(ar, ref ipendPoint);
				int num = array.Length;
				if (num > 0)
				{
					if (num != 14)
					{
						if (num != 22)
						{
							if (num == 100)
							{
								if (DCSconnection.canStream())
								{
									int num2 = (int)(array[60] & byte.MaxValue) << 16;
									num2 |= (int)(array[59] & byte.MaxValue) << 8;
									num2 |= (int)(array[58] & byte.MaxValue);
									string @string = Encoding.Default.GetString(array);
									information.hisCall = @string.Substring(31, 8);
									information.hisCallsmall = @string.Substring(39, 4);
									DSTARhandler.destination = @string.Substring(7, 8);
									DSTARhandler.departure = @string.Substring(15, 8);
									DSTARhandler.companion = @string.Substring(23, 8);
									DSTARhandler.own1 = @string.Substring(31, 8);
									DSTARhandler.own2 = @string.Substring(39, 4);
									if (num2 == 0)
									{
										DSTARhandler.makeDSTARHeaderMMDVM();
									}
									if (num2 == 0)
									{
										DCSconnection.nextSequenceCounter = 0;
									}
									if (num2 - DCSconnection.nextSequenceCounter > 11)
									{
										DCSconnection.nextSequenceCounter = num2;
									}
									if (num2 - DCSconnection.nextSequenceCounter < 0)
									{
										DCSconnection.nextSequenceCounter = num2;
									}
									if (DCSconnection.nextSequenceCounter != num2)
									{
										DCSconnection.misssingSequenceCounter = num2 - DCSconnection.nextSequenceCounter;
										DCSconnection.nextSequenceCounter = num2;
										DCSconnection.missing = true;
									}
									DCSconnection.nextSequenceCounter++;
									if (DCSconnection.misssingSequenceCounter < 10 && DCSconnection.misssingSequenceCounter > 0)
									{
										for (int i = DCSconnection.misssingSequenceCounter; i >= 1; i--)
										{
											DSTARhandler.makeDSTARVoiceMMDVM(information.NULL_FRAME_DATA_BYTES);
											DCSconnection.missing = false;
											DCSconnection.misssingSequenceCounter = 0;
										}
									}
									byte[] array2 = new byte[12];
									byte[] array3 = new byte[3];
									TimerRXTX.TX();
									Buffer.BlockCopy(array, 46, array2, 0, 12);
									Buffer.BlockCopy(array2, 9, array3, 0, 3);
									SlowData.slowDavid(array3, true);
									DSTARhandler.makeDSTARVoiceMMDVM(array2);
									if (DCSconnection.lastByteCounter + 1 == (int)(array[45] & 255))
									{
										DSTARhandler.makeDSTARStopMMDVM();
										DCSconnection._isStreaming = false;
									}
									else if (array2[9] == 85 && array2[10] == 85)
									{
										DSTARhandler.makeDSTARStopMMDVM();
										DCSconnection._isStreaming = false;
									}
									DCSconnection.lastByteCounter = (int)(array[45] | 64);
									DCSconnection._isStreaming = true;
								}
								ShowCallTimer.resetTimerDSTAR(Encoding.Default.GetString(array).Substring(31, 8));
							}
						}
						else
						{
							Encoding.Default.GetString(array);
							string text = information.myCall.PadRight(8).Substring(0, 7) + information.myDSTARmodule.Trim() + "\0" + DCSconnection.setDestination;
							byte[] bytes = Encoding.ASCII.GetBytes(text);
							DCSconnection.sendRAW(bytes, bytes.Length);
							DCSconnection.m_timeoutTimer.start();
						}
					}
					else
					{
						string string2 = Encoding.Default.GetString(array);
						if (string2.Substring(10, 3).Equals("ACK"))
						{
							DCSconnection.m_status = DCSconnection.STATUS.RUNNING;
							DSTARhandler.talkWoman("!" + Regex.Replace(DCSconnection.setDestination, "\\s+", ""));
						}
						if (string2.Substring(10, 3).Equals("NAK"))
						{
						}
					}
				}
				udpClient.BeginReceive(new AsyncCallback(DCSconnection.receiveData2), ar.AsyncState);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x00006FAC File Offset: 0x000051AC
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

		// Token: 0x060000CA RID: 202 RVA: 0x00006FD0 File Offset: 0x000051D0
		public static bool isLinked()
		{
			return DCSconnection.linked;
		}

		// Token: 0x060000CB RID: 203 RVA: 0x00006FD7 File Offset: 0x000051D7
		public static bool isStreaming()
		{
			return DCSconnection._isStreaming;
		}

		// Token: 0x060000CC RID: 204 RVA: 0x00006FE0 File Offset: 0x000051E0
		public static void writePing(object o)
		{
			DCSconnection.currenttime = DateTime.Now.Ticks / 10000L;
			DCSconnection.ms = (int)(DCSconnection.currenttime - DCSconnection.oldtime);
			DCSconnection.oldtime = DCSconnection.currenttime;
			DCSconnection.STATUS status = DCSconnection.m_status;
			if (DCSconnection.m_timeoutTimer.isRunning() && DCSconnection.m_timeoutTimer.hasExpired())
			{
				DCSconnection.reconnect();
			}
			DCSconnection.m_retryTimer.clock(DCSconnection.ms);
			if (DCSconnection.m_retryTimer.isRunning() && DCSconnection.m_retryTimer.hasExpired())
			{
				switch (DCSconnection.m_status)
				{
				case DCSconnection.STATUS.DISCONNECTED:
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DCSconnection.ChangeStatusTextDCS("Not linked");
						break;
					case information.LANGUAGE.JAPANESE:
						DCSconnection.ChangeStatusTextDCS("未接続");
						break;
					case information.LANGUAGE.CHINEES:
						DCSconnection.ChangeStatusTextDCS("Not linked");
						break;
					case information.LANGUAGE.KOREAN:
						DCSconnection.ChangeStatusTextDCS("연결안됨");
						break;
					}
					break;
				case DCSconnection.STATUS.W_LOGIN:
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DCSconnection.ChangeStatusTextDCS("Linking to " + DCSconnection.setDestination);
						break;
					case information.LANGUAGE.JAPANESE:
						DCSconnection.ChangeStatusTextDCS("接続中 " + DCSconnection.setDestination);
						break;
					case information.LANGUAGE.CHINEES:
						DCSconnection.ChangeStatusTextDCS("Linking to " + DCSconnection.setDestination);
						break;
					case information.LANGUAGE.KOREAN:
						DCSconnection.ChangeStatusTextDCS("연결중 " + DCSconnection.setDestination);
						break;
					}
					DCSconnection.writeLogin();
					break;
				case DCSconnection.STATUS.RUNNING:
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DCSconnection.ChangeStatusTextDCS("Linked to " + DCSconnection.setDestination);
						break;
					case information.LANGUAGE.JAPANESE:
						DCSconnection.ChangeStatusTextDCS("接続中 " + DCSconnection.setDestination);
						break;
					case information.LANGUAGE.CHINEES:
						DCSconnection.ChangeStatusTextDCS("Linked to " + DCSconnection.setDestination);
						break;
					case information.LANGUAGE.KOREAN:
						DCSconnection.ChangeStatusTextDCS("연결됨 " + DCSconnection.setDestination);
						break;
					}
					break;
				}
			}
			DCSconnection.m_timeoutTimer.clock(DCSconnection.ms);
			if (DCSconnection.m_timeoutTimer.isRunning() && DCSconnection.m_timeoutTimer.hasExpired())
			{
				DCSconnection.reconnect();
			}
			DCSconnection.currenttime = DateTime.Now.Ticks / 10000L;
			DCSconnection.ms = (int)(DCSconnection.currenttime - DCSconnection.oldtime);
			DCSconnection.oldtime = DCSconnection.currenttime;
			DCSconnection._timer.Change(DCSconnection.TIME_INTERVAL_IN_MILLISECONDS, -1);
		}

		// Token: 0x04000050 RID: 80
		private static int POLYNOMIAL = 33800;

		// Token: 0x04000051 RID: 81
		private static int PRESET_VALUE = 65535;

		// Token: 0x04000054 RID: 84
		private static bool _isStreaming = false;

		// Token: 0x04000055 RID: 85
		private static DCSconnection.STATUS m_status = DCSconnection.STATUS.DISCONNECTED;

		// Token: 0x04000056 RID: 86
		private static UdpClient udpClient;

		// Token: 0x04000057 RID: 87
		private static Timer _timer;

		// Token: 0x04000058 RID: 88
		private static Timert m_timeoutTimer;

		// Token: 0x04000059 RID: 89
		private static Timert m_retryTimer;

		// Token: 0x0400005A RID: 90
		private static long currenttime = 0L;

		// Token: 0x0400005B RID: 91
		private static long oldtime = 0L;

		// Token: 0x0400005C RID: 92
		private static int ms = 0;

		// Token: 0x0400005D RID: 93
		private static string ApplicationVersion;

		// Token: 0x0400005E RID: 94
		private static string setDestination = "";

		// Token: 0x0400005F RID: 95
		private static int TIME_INTERVAL_IN_MILLISECONDS = 1500;

		// Token: 0x04000060 RID: 96
		private static int DSTPORT = 30051;

		// Token: 0x04000061 RID: 97
		private static int SRCPORT = 30052;

		// Token: 0x04000062 RID: 98
		private static bool linked = false;

		// Token: 0x04000063 RID: 99
		private static int sessionCounter = 0;

		// Token: 0x04000064 RID: 100
		private static int smallCounter = 0;

		// Token: 0x04000065 RID: 101
		private static int nextSequenceCounter = 0;

		// Token: 0x04000066 RID: 102
		private static int misssingSequenceCounter = 0;

		// Token: 0x04000067 RID: 103
		private static bool missing = false;

		// Token: 0x04000068 RID: 104
		private static string HOSTNAME;

		// Token: 0x04000069 RID: 105
		private static int lastByteCounter;

		// Token: 0x02000053 RID: 83
		public enum STATUS
		{
			// Token: 0x040004C3 RID: 1219
			DISCONNECTED,
			// Token: 0x040004C4 RID: 1220
			W_LOGIN,
			// Token: 0x040004C5 RID: 1221
			W_AUTHORISATION,
			// Token: 0x040004C6 RID: 1222
			W_CONFIG,
			// Token: 0x040004C7 RID: 1223
			RUNNING
		}

		// Token: 0x02000054 RID: 84
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040004C8 RID: 1224
			public static TimerCallback <0>__writePing;
		}
	}
}
