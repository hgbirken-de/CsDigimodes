using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace BlueDV
{
	// Token: 0x02000023 RID: 35
	internal class DPLUSconnection
	{
		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060001EC RID: 492 RVA: 0x00011FD1 File Offset: 0x000101D1
		// (set) Token: 0x060001ED RID: 493 RVA: 0x00011FD8 File Offset: 0x000101D8
		public static string StatusTextDPLUS { get; private set; }

		// Token: 0x14000012 RID: 18
		// (add) Token: 0x060001EE RID: 494 RVA: 0x00011FE0 File Offset: 0x000101E0
		// (remove) Token: 0x060001EF RID: 495 RVA: 0x00012014 File Offset: 0x00010214
		public static event EventHandler StatusTextChangedDPLUS;

		// Token: 0x060001F0 RID: 496 RVA: 0x00012048 File Offset: 0x00010248
		public static void open()
		{
			DPLUSconnection.m_status = DPLUSconnection.STATUS.W_LOGIN;
			DPLUSconnection.m_timeoutTimer.start();
			DPLUSconnection.m_retryTimer.start();
			TimerCallback timerCallback;
			if ((timerCallback = DPLUSconnection.<>O.<0>__writePing) == null)
			{
				timerCallback = (DPLUSconnection.<>O.<0>__writePing = new TimerCallback(DPLUSconnection.writePing));
			}
			DPLUSconnection._timer = new Timer(timerCallback, null, DPLUSconnection.TIME_INTERVAL_IN_MILLISECONDS, -1);
			DPLUSconnection.writeLogin();
			DPLUSconnection.currenttime = DateTime.Now.Ticks / 10000L;
			DPLUSconnection.ms = (int)(DPLUSconnection.currenttime - DPLUSconnection.oldtime);
			DPLUSconnection.oldtime = DPLUSconnection.currenttime;
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x000120D4 File Offset: 0x000102D4
		public static bool isStreaming()
		{
			return DPLUSconnection._isStreaming;
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x000120DC File Offset: 0x000102DC
		public static void reconnect()
		{
			DPLUSconnection.m_status = DPLUSconnection.STATUS.W_LOGIN;
			try
			{
				DPLUSconnection.udpClient.Close();
				DPLUSconnection.udpClient = new UdpClient(DPLUSconnection.SRCPORT);
				DPLUSconnection.udpClient.Connect(DPLUSconnection.HOSTNAME, DPLUSconnection.DSTPORT);
				DPLUSconnection.udpClient.BeginReceive(new AsyncCallback(DPLUSconnection.receiveData2), DPLUSconnection.udpClient);
				DPLUSconnection.m_status = DPLUSconnection.STATUS.W_LOGIN;
				DPLUSconnection.m_timeoutTimer.start();
				DPLUSconnection.m_retryTimer.start();
				DPLUSconnection.writeLogin();
				DPLUSconnection.currenttime = DateTime.Now.Ticks / 10000L;
				DPLUSconnection.ms = (int)(DPLUSconnection.currenttime - DPLUSconnection.oldtime);
				DPLUSconnection.oldtime = DPLUSconnection.currenttime;
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x000121A0 File Offset: 0x000103A0
		public static void connect(string hostname)
		{
			DPLUSconnection.HOSTNAME = hostname;
			try
			{
				DPLUSconnection.m_retryTimer = new Timert(1000, 5, 0);
				DPLUSconnection.m_timeoutTimer = new Timert(5000, 5, 0);
				DPLUSconnection.udpClient = new UdpClient(DPLUSconnection.SRCPORT);
			}
			catch (SocketException ex)
			{
				if (ex.ErrorCode == 10048)
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DPLUSconnection.ChangeStatusTextDPLUS("ERR: DPLUS port in use");
						break;
					case information.LANGUAGE.JAPANESE:
						DPLUSconnection.ChangeStatusTextDPLUS("エラー：DPLUSポート使用中");
						break;
					case information.LANGUAGE.CHINEES:
						DPLUSconnection.ChangeStatusTextDPLUS("ERR: DPLUS port in use");
						break;
					case information.LANGUAGE.KOREAN:
						DPLUSconnection.ChangeStatusTextDPLUS("에러: DMR포트 사용중");
						break;
					}
				}
				else if (ex.ErrorCode == 10060)
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DPLUSconnection.ChangeStatusTextDPLUS("ERR: DPLUS Conn timed out");
						break;
					case information.LANGUAGE.JAPANESE:
						DPLUSconnection.ChangeStatusTextDPLUS("エラー：DPLUSタイムアウト");
						break;
					case information.LANGUAGE.CHINEES:
						DPLUSconnection.ChangeStatusTextDPLUS("ERR: DPLUS Conn timed out");
						break;
					case information.LANGUAGE.KOREAN:
						DPLUSconnection.ChangeStatusTextDPLUS("에러: DMR연결 시간초과");
						break;
					}
				}
				else
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DPLUSconnection.ChangeStatusTextDPLUS("ERR: " + ex.ErrorCode.ToString());
						break;
					case information.LANGUAGE.JAPANESE:
						DPLUSconnection.ChangeStatusTextDPLUS("エラー：" + ex.ErrorCode.ToString());
						break;
					case information.LANGUAGE.CHINEES:
						DPLUSconnection.ChangeStatusTextDPLUS("ERR: " + ex.ErrorCode.ToString());
						break;
					case information.LANGUAGE.KOREAN:
						DPLUSconnection.ChangeStatusTextDPLUS("ERR: " + ex.ErrorCode.ToString());
						break;
					}
				}
				if (DPLUSconnection.udpClient != null)
				{
					DPLUSconnection.udpClient.Close();
				}
				DPLUSconnection.m_retryTimer.stop();
				DPLUSconnection.m_timeoutTimer.stop();
				return;
			}
			try
			{
				DPLUSconnection.udpClient.Connect(DPLUSconnection.HOSTNAME, DPLUSconnection.DSTPORT);
				DPLUSconnection.udpClient.BeginReceive(new AsyncCallback(DPLUSconnection.receiveData2), DPLUSconnection.udpClient);
				DPLUSconnection.open();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex2)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					DPLUSconnection.ChangeStatusTextDPLUS("ERR: " + ex2.ToString());
					break;
				case information.LANGUAGE.JAPANESE:
					DPLUSconnection.ChangeStatusTextDPLUS("エラー： " + ex2.ToString());
					break;
				case information.LANGUAGE.CHINEES:
					DPLUSconnection.ChangeStatusTextDPLUS("ERR: " + ex2.ToString());
					break;
				case information.LANGUAGE.KOREAN:
					DPLUSconnection.ChangeStatusTextDPLUS("ERR: " + ex2.ToString());
					break;
				}
			}
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x00012490 File Offset: 0x00010690
		private static void close()
		{
			DPLUSconnection.selectedStream[0] = 0;
			DPLUSconnection.selectedStream[1] = 0;
			if (DPLUSconnection.udpClient != null)
			{
				try
				{
					DPLUSconnection.sendDisconnect();
					DPLUSconnection._timer.Dispose();
					if (DPLUSconnection.m_status != DPLUSconnection.STATUS.BUSY)
					{
						DPLUSconnection.m_status = DPLUSconnection.STATUS.DISCONNECTED;
					}
					DPLUSconnection.udpClient.Client.Shutdown(SocketShutdown.Receive);
					DPLUSconnection.udpClient.Close();
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DPLUSconnection.ChangeStatusTextDPLUS("Not linked");
						break;
					case information.LANGUAGE.JAPANESE:
						DPLUSconnection.ChangeStatusTextDPLUS("未接続");
						break;
					case information.LANGUAGE.CHINEES:
						DPLUSconnection.ChangeStatusTextDPLUS("Not linked");
						break;
					case information.LANGUAGE.KOREAN:
						DPLUSconnection.ChangeStatusTextDPLUS("연결안됨");
						break;
					}
					DSTARhandler.talkWoman("@");
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x0001255C File Offset: 0x0001075C
		public static void unlink()
		{
			DPLUSconnection.close();
			DPLUSconnection.linked = false;
			DPLUSconnection._isStreaming = false;
			DPLUSconnection.selectedStream[0] = 0;
			DPLUSconnection.selectedStream[1] = 0;
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x0001257F File Offset: 0x0001077F
		public static void link(string reflector, string hostname)
		{
			DPLUSconnection.setDestination = reflector;
			DPLUSconnection.connect(hostname);
			DPLUSconnection.linked = true;
			information.connectedDSTARReflector = reflector;
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x00012599 File Offset: 0x00010799
		public static bool isLinked()
		{
			return DPLUSconnection.linked;
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x000125A0 File Offset: 0x000107A0
		public static void sendRAW(byte[] data, int length)
		{
			try
			{
				DPLUSconnection.udpClient.Send(data, length);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x000125D0 File Offset: 0x000107D0
		private static void sendDisconnect()
		{
			byte[] array = new byte[5];
			array[0] = 5;
			array[2] = 24;
			DPLUSconnection.sendRAW(array, array.Length);
		}

		// Token: 0x060001FA RID: 506 RVA: 0x00006FAC File Offset: 0x000051AC
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

		// Token: 0x060001FB RID: 507 RVA: 0x000125EC File Offset: 0x000107EC
		private static void receiveData2(IAsyncResult ar)
		{
			try
			{
				UdpClient udpClient = (UdpClient)ar.AsyncState;
				IPEndPoint ipendPoint = new IPEndPoint(IPAddress.Any, 0);
				byte[] array = udpClient.EndReceive(ar, ref ipendPoint);
				int num = array.Length;
				if (num > 0 && DPLUSconnection.m_status != DPLUSconnection.STATUS.REJECTED)
				{
					if (num <= 8)
					{
						if (num != 3)
						{
							if (num != 5)
							{
								if (num == 8)
								{
									byte[] array2 = new byte[4];
									Buffer.BlockCopy(array, 4, array2, 0, 4);
									if (Encoding.UTF8.GetString(array2).Equals("BUSY"))
									{
										DPLUSconnection.m_status = DPLUSconnection.STATUS.BUSY;
										DPLUSconnection.close();
									}
									else
									{
										DSTARhandler.talkWoman("!" + DPLUSconnection.setDestination.Replace(" ", ""));
										DPLUSconnection.m_status = DPLUSconnection.STATUS.RUNNING;
										DPLUSconnection.ping_back();
									}
								}
							}
							else
							{
								if ((array[4] & 255) == 1)
								{
									DPLUSconnection.writeAuthorisation();
									DPLUSconnection.m_status = DPLUSconnection.STATUS.W_AUTHORISATION;
								}
								if ((array[4] & 255) == 0)
								{
									DPLUSconnection.m_status = DPLUSconnection.STATUS.DISCONNECTED;
									DPLUSconnection.close();
								}
							}
						}
						else
						{
							DPLUSconnection.ping_back();
							DPLUSconnection.m_status = DPLUSconnection.STATUS.RUNNING;
							DPLUSconnection.m_timeoutTimer.start();
						}
					}
					else if (num <= 29)
					{
						if (num != 10)
						{
							if (num == 29)
							{
								if (DPLUSconnection.selectedStream[0] == array[14] && DPLUSconnection.selectedStream[1] == array[15])
								{
									ShowCallTimer.resetTimerDSTAR(information.hisCall);
								}
								if (DPLUSconnection.selectedStream[0] == array[14] && DPLUSconnection.selectedStream[1] == array[15] && DPLUSconnection.canStream())
								{
									if (DPLUSconnection.selectedStream[0] != DPLUSconnection.lastSession[0] && DPLUSconnection.selectedStream[1] != DPLUSconnection.lastSession[1])
									{
										DPLUSconnection.seenEnd = true;
										DPLUSconnection.lastSession[0] = DPLUSconnection.selectedStream[0];
										DPLUSconnection.lastSession[1] = DPLUSconnection.selectedStream[1];
									}
									if (DPLUSconnection.seenEnd)
									{
										DSTARhandler.makeDSTARHeaderMMDVM();
										DPLUSconnection.seenEnd = false;
									}
									byte[] array3 = new byte[12];
									byte[] array4 = new byte[3];
									Buffer.BlockCopy(array, 17, array3, 0, 12);
									Buffer.BlockCopy(array3, 9, array4, 0, 3);
									SlowData.slowDavid(array4, true);
									int num2 = DPLUSconnection.calculate_missing(array[16], DPLUSconnection.lastCounter) & 255;
									if (num2 > 0 && num2 < 20)
									{
										for (int i = 0; i < num2; i++)
										{
											DPLUSconnection.makeDSTARVoiceMMDVM(information.NULL_FRAME_DATA_BYTES);
										}
									}
									DPLUSconnection.lastCounter = array[16];
									DPLUSconnection.lastCounter += 1;
									DPLUSconnection.makeDSTARVoiceMMDVM(array3);
									DPLUSconnection._isStreaming = true;
									TimerRXTX.TX();
									if (array3[9] == 85 && array3[10] == 85 && array3[11] == 85)
									{
										DSTARhandler.makeDSTARStopMMDVM();
										DPLUSconnection.seenEnd = true;
										DPLUSconnection.lastCounter = 0;
										DPLUSconnection._isStreaming = false;
									}
								}
							}
						}
					}
					else if (num != 32)
					{
						if (num == 58)
						{
							byte[] array5 = new byte[36];
							Buffer.BlockCopy(array, 20, array5, 0, 36);
							string @string = Encoding.UTF8.GetString(array5);
							string text = @string.Substring(0, 8);
							string text2 = @string.Substring(8, 8);
							@string.Substring(16, 8);
							@string.Substring(24, 8);
							@string.Substring(32, 4);
							if (text.Equals(DPLUSconnection.setDestination) || text2.Equals(DPLUSconnection.setDestination))
							{
								if (DPLUSconnection.canStream())
								{
									DSTARhandler.destination = @string.Substring(0, 8);
									DSTARhandler.departure = @string.Substring(8, 8);
									DSTARhandler.companion = @string.Substring(16, 8);
									DSTARhandler.own1 = @string.Substring(24, 8);
									DSTARhandler.own2 = @string.Substring(32, 4);
									information.hisCall = DSTARhandler.own1;
									information.hisCallsmall = DSTARhandler.own2;
									Buffer.BlockCopy(array, 14, DPLUSconnection.selectedStream, 0, 2);
								}
								ShowCallTimer.resetTimerDSTAR(@string.Substring(24, 8));
							}
						}
					}
					else if (DPLUSconnection.selectedStream[0] == array[14] && DPLUSconnection.selectedStream[1] == array[15] && DPLUSconnection.canStream())
					{
						DSTARhandler.makeDSTARStopMMDVM();
						DPLUSconnection.seenEnd = true;
						DPLUSconnection.lastCounter = 0;
						DPLUSconnection._isStreaming = false;
					}
				}
				udpClient.BeginReceive(new AsyncCallback(DPLUSconnection.receiveData2), ar.AsyncState);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060001FC RID: 508 RVA: 0x00012A2C File Offset: 0x00010C2C
		public static void writePing(object o)
		{
			DPLUSconnection.currenttime = DateTime.Now.Ticks / 10000L;
			DPLUSconnection.ms = (int)(DPLUSconnection.currenttime - DPLUSconnection.oldtime);
			DPLUSconnection.oldtime = DPLUSconnection.currenttime;
			DPLUSconnection.STATUS status = DPLUSconnection.m_status;
			if (DPLUSconnection.m_timeoutTimer.isRunning() && DPLUSconnection.m_timeoutTimer.hasExpired())
			{
				DPLUSconnection.isReconnected = true;
				DPLUSconnection.reconnect();
			}
			DPLUSconnection.m_retryTimer.clock(DPLUSconnection.ms);
			if ((DPLUSconnection.m_retryTimer.isRunning() && DPLUSconnection.m_retryTimer.hasExpired()) || DPLUSconnection.startLogin)
			{
				switch (DPLUSconnection.m_status)
				{
				case DPLUSconnection.STATUS.DISCONNECTED:
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DPLUSconnection.ChangeStatusTextDPLUS("Not linked");
						break;
					case information.LANGUAGE.JAPANESE:
						DPLUSconnection.ChangeStatusTextDPLUS("未接続");
						break;
					case information.LANGUAGE.CHINEES:
						DPLUSconnection.ChangeStatusTextDPLUS("Not linked");
						break;
					case information.LANGUAGE.KOREAN:
						DPLUSconnection.ChangeStatusTextDPLUS("연결안됨");
						break;
					}
					break;
				case DPLUSconnection.STATUS.W_LOGIN:
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DPLUSconnection.ChangeStatusTextDPLUS("Linking to " + DPLUSconnection.setDestination);
						break;
					case information.LANGUAGE.JAPANESE:
						DPLUSconnection.ChangeStatusTextDPLUS("接続中 " + DPLUSconnection.setDestination);
						break;
					case information.LANGUAGE.CHINEES:
						DPLUSconnection.ChangeStatusTextDPLUS("Linking to " + DPLUSconnection.setDestination);
						break;
					case information.LANGUAGE.KOREAN:
						DPLUSconnection.ChangeStatusTextDPLUS("연결중 " + DPLUSconnection.setDestination);
						break;
					}
					DPLUSconnection.writeLogin();
					break;
				case DPLUSconnection.STATUS.W_AUTHORISATION:
					DPLUSconnection.writeAuthorisation();
					break;
				case DPLUSconnection.STATUS.RUNNING:
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						DPLUSconnection.ChangeStatusTextDPLUS("Linked to " + DPLUSconnection.setDestination);
						break;
					case information.LANGUAGE.JAPANESE:
						DPLUSconnection.ChangeStatusTextDPLUS("接続中 " + DPLUSconnection.setDestination);
						break;
					case information.LANGUAGE.CHINEES:
						DPLUSconnection.ChangeStatusTextDPLUS("Linked to " + DPLUSconnection.setDestination);
						break;
					case information.LANGUAGE.KOREAN:
						DPLUSconnection.ChangeStatusTextDPLUS("연결됨 " + DPLUSconnection.setDestination);
						break;
					}
					break;
				case DPLUSconnection.STATUS.REJECTED:
					DPLUSconnection.ChangeStatusTextDPLUS("Rejected from server. Contact admin");
					DPLUSconnection.sendDisconnect();
					break;
				case DPLUSconnection.STATUS.BUSY:
					DPLUSconnection.ChangeStatusTextDPLUS("Server is BUSY!! Try in 5 minutes!!");
					DPLUSconnection.sendDisconnect();
					break;
				}
			}
			DPLUSconnection.m_timeoutTimer.clock(DPLUSconnection.ms);
			if (DPLUSconnection.m_timeoutTimer.isRunning() && DPLUSconnection.m_timeoutTimer.hasExpired())
			{
				DPLUSconnection.isReconnected = true;
				DPLUSconnection.reconnect();
			}
			DPLUSconnection.currenttime = DateTime.Now.Ticks / 10000L;
			DPLUSconnection.ms = (int)(DPLUSconnection.currenttime - DPLUSconnection.oldtime);
			DPLUSconnection.oldtime = DPLUSconnection.currenttime;
			if (DPLUSconnection._timer != null)
			{
				try
				{
					DPLUSconnection._timer.Change(DPLUSconnection.TIME_INTERVAL_IN_MILLISECONDS, -1);
				}
				catch (ObjectDisposedException)
				{
				}
			}
		}

		// Token: 0x060001FD RID: 509 RVA: 0x00012D08 File Offset: 0x00010F08
		private static void ping_back()
		{
			byte[] array = new byte[3];
			array[0] = 3;
			array[1] = 96;
			byte[] array2 = array;
			if (DPLUSconnection.udpClient.Client == null)
			{
				return;
			}
			DPLUSconnection.sendRAW(array2, array2.Length);
		}

		// Token: 0x060001FE RID: 510 RVA: 0x00012D3C File Offset: 0x00010F3C
		private static void ChangeStatusTextDPLUS(string text)
		{
			DPLUSconnection.StatusTextDPLUS = text;
			EventHandler statusTextChangedDPLUS = DPLUSconnection.StatusTextChangedDPLUS;
			if (statusTextChangedDPLUS != null)
			{
				statusTextChangedDPLUS(null, EventArgs.Empty);
			}
		}

		// Token: 0x060001FF RID: 511 RVA: 0x00012D64 File Offset: 0x00010F64
		private static void writeLogin()
		{
			if (DPLUSconnection.udpClient.Client == null)
			{
				return;
			}
			byte[] array = new byte[] { 5, 0, 24, 0, 1 };
			DPLUSconnection.sendRAW(array, array.Length);
		}

		// Token: 0x06000200 RID: 512 RVA: 0x00012D8C File Offset: 0x00010F8C
		public static void writeAuthorisation()
		{
			byte[] array = new byte[]
			{
				28, 192, 4, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				68, 86, 48, 49, 57, 57, 57, 57
			};
			string text = information.myCall.Trim();
			for (int i = 0; i <= text.Length - 1; i++)
			{
				array[i + 4] = (byte)text[i];
			}
			DPLUSconnection.sendRAW(array, array.Length);
		}

		// Token: 0x06000201 RID: 513 RVA: 0x00012DE0 File Offset: 0x00010FE0
		public static void makeDSTARVoiceMMDVM(byte[] AMBEStream)
		{
			byte[] array = new byte[15];
			array[0] = 224;
			array[1] = 15;
			array[2] = 17;
			Buffer.BlockCopy(AMBEStream, 0, array, 3, 12);
			FusionQueue.addQueue(array);
		}

		// Token: 0x06000202 RID: 514 RVA: 0x00012E18 File Offset: 0x00011018
		private static int calculate_missing(byte first, byte until)
		{
			int num = 0;
			if (first != until)
			{
				int num2 = (int)(first - until);
				if (num2 < 0)
				{
					num = num2 + 20;
				}
				else
				{
					num = num2;
				}
			}
			return num;
		}

		// Token: 0x06000203 RID: 515 RVA: 0x00012E40 File Offset: 0x00011040
		public static void beepBack()
		{
			DSTARhandler.destination = "        ";
			DSTARhandler.departure = "        ";
			DSTARhandler.companion = "        ";
			DSTARhandler.own1 = information.myCall.PadRight(8);
			DSTARhandler.own2 = "BLDV";
			byte[] array = new byte[44];
			array[0] = 224;
			array[1] = 44;
			array[2] = 16;
			Buffer.BlockCopy(DSTARhandler.makeDSTARHeader(), 0, array, 3, 41);
			array[3] = 2;
			productSelector.write(array);
			DPLUSconnection.makeDSTARVoiceMMDVM(DPLUSconnection.NULL_FRAME_DATA_BYTES_SYNC);
			for (int i = 0; i < 20; i++)
			{
				new byte[12];
				Buffer.BlockCopy(SlowData.makeSlowDataFromtext(), 0, DPLUSconnection.NULL_FRAME_DATA_BYTES, 9, 3);
				DPLUSconnection.makeDSTARVoiceMMDVM(DPLUSconnection.NULL_FRAME_DATA_BYTES);
			}
			DSTARhandler.makeDSTARStopMMDVM();
		}

		// Token: 0x06000204 RID: 516 RVA: 0x00012EFC File Offset: 0x000110FC
		public static void makeControlFrame()
		{
			byte[] array = new byte[58];
			array[0] = 58;
			array[1] = 128;
			array[2] = 68;
			array[3] = 83;
			array[4] = 86;
			array[5] = 84;
			array[6] = 16;
			array[7] = 0;
			array[8] = 0;
			array[9] = 0;
			array[10] = 32;
			array[11] = 0;
			array[12] = 2;
			array[13] = 1;
			array[14] = DSTARhandler.session_id_byte()[0];
			array[15] = DSTARhandler.session_id_byte()[1];
			array[16] = 128;
			array[17] = 0;
			array[18] = 0;
			array[19] = 0;
			Buffer.BlockCopy(DSTARhandler.dstarHeaderBytes, 0, array, 20, 36);
			array[56] = 0;
			array[57] = 11;
			for (int i = 0; i <= 7; i++)
			{
				array[20 + i] = (byte)DPLUSconnection.setDestination[i];
			}
			string text = information.myCall.PadRight(7);
			for (int j = 0; j <= 6; j++)
			{
				array[28 + j] = (byte)text[j];
			}
			array[35] = (byte)information.myDSTARmodule[0];
			if (DPLUSconnection.isLinked())
			{
				DPLUSconnection.sendRAW(array, array.Length);
			}
		}

		// Token: 0x06000205 RID: 517 RVA: 0x00013008 File Offset: 0x00011208
		public static void makeVoiceFrame(byte[] AMBE12bytes)
		{
			byte[] array = new byte[29];
			array[0] = 29;
			array[1] = 128;
			array[2] = 68;
			array[3] = 83;
			array[4] = 86;
			array[5] = 84;
			array[6] = 32;
			array[7] = 0;
			array[8] = 0;
			array[9] = 0;
			array[10] = 32;
			array[11] = 0;
			array[12] = 2;
			array[13] = 1;
			array[14] = DSTARhandler.session_id_byte()[0];
			array[15] = DSTARhandler.session_id_byte()[1];
			array[16] = DPLUSconnection.smallCounter;
			Buffer.BlockCopy(AMBE12bytes, 0, array, 17, 12);
			DPLUSconnection.smallCounter += 1;
			if ((DPLUSconnection.smallCounter & 255) == 21)
			{
				DPLUSconnection.makeControlFrame();
				DPLUSconnection.makeControlFrame();
				DPLUSconnection.makeControlFrame();
				DPLUSconnection.makeControlFrame();
			}
			if ((DPLUSconnection.smallCounter & 255) == 21)
			{
				DPLUSconnection.smallCounter = 0;
			}
			if (DPLUSconnection.isLinked())
			{
				DPLUSconnection.sendRAW(array, array.Length);
			}
		}

		// Token: 0x06000206 RID: 518 RVA: 0x000130E8 File Offset: 0x000112E8
		public static void sendEndFrame()
		{
			byte[] array = new byte[]
			{
				32,
				128,
				68,
				83,
				86,
				84,
				32,
				0,
				0,
				0,
				32,
				0,
				2,
				1,
				DSTARhandler.session_id_byte()[0],
				DSTARhandler.session_id_byte()[1],
				DPLUSconnection.smallCounter | 64,
				158,
				141,
				50,
				136,
				38,
				26,
				63,
				97,
				232,
				85,
				85,
				85,
				85,
				200,
				122
			};
			if (DPLUSconnection.isLinked())
			{
				DPLUSconnection.sendRAW(array, array.Length);
			}
			DPLUSconnection.smallCounter = 0;
		}

		// Token: 0x06000207 RID: 519 RVA: 0x000131E8 File Offset: 0x000113E8
		public static int CRCcalc2(byte[] args, int startpos, int length)
		{
			int num = 0;
			int num2 = 4129;
			for (int i = startpos; i < startpos + length; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					bool flag = ((args[i] >> 7 - j) & 1) == 1;
					bool flag2 = ((num >> 15) & 1) == 1;
					num <<= 1;
					if (flag2 ^ flag)
					{
						num ^= num2;
					}
				}
			}
			return num & 65535;
		}

		// Token: 0x06000208 RID: 520 RVA: 0x00013248 File Offset: 0x00011448
		public static ushort GenCrc16(byte[] c, int nByte)
		{
			ushort num = 4129;
			ushort num2 = 0;
			ushort num3 = 0;
			ushort num4 = num2;
			ushort num5 = 0;
			while ((int)num5 < nByte)
			{
				ushort num6 = (ushort)(byte.MaxValue & c[(int)num3]);
				ushort num7 = (ushort)((ushort)((num4 >> 8) ^ (int)num6) << 8);
				for (ushort num8 = 0; num8 < 8; num8 += 1)
				{
					if ((num7 & 32768) != 0)
					{
						num7 = (ushort)(((int)num7 << 1) ^ (int)num);
					}
					else
					{
						num7 = (ushort)(num7 << 1);
					}
				}
				num4 = (ushort)(((int)num4 << 8) ^ (int)num7);
				num3 += 1;
				num5 += 1;
			}
			return num4;
		}

		// Token: 0x040000FF RID: 255
		public static byte[] NULL_FRAME_DATA_BYTES = new byte[]
		{
			158, 141, 50, 136, 38, 26, 63, 97, 232, 22,
			41, 245
		};

		// Token: 0x04000100 RID: 256
		public static byte[] NULL_FRAME_DATA_BYTES_SYNC = new byte[]
		{
			158, 141, 50, 136, 38, 26, 63, 97, 232, 85,
			45, 22
		};

		// Token: 0x04000101 RID: 257
		public static byte[] NULL_FRAME_DATA_BYTES_VOICE = new byte[] { 158, 141, 50, 136, 38, 26, 63, 97, 232 };

		// Token: 0x04000102 RID: 258
		public static byte[] SLOW_DATA_SYNC = new byte[] { 85, 45, 22 };

		// Token: 0x04000103 RID: 259
		public static byte[] SLOW_DATA_NULL = new byte[] { 22, 41, 245 };

		// Token: 0x04000104 RID: 260
		private static byte lastCounter = 0;

		// Token: 0x04000105 RID: 261
		private static bool _isStreaming = false;

		// Token: 0x04000106 RID: 262
		private static DPLUSconnection.STATUS m_status = DPLUSconnection.STATUS.DISCONNECTED;

		// Token: 0x04000107 RID: 263
		private static UdpClient udpClient;

		// Token: 0x04000108 RID: 264
		private static Timer _timer;

		// Token: 0x04000109 RID: 265
		private static Timert m_timeoutTimer;

		// Token: 0x0400010A RID: 266
		private static Timert m_retryTimer;

		// Token: 0x0400010B RID: 267
		private static long currenttime = 0L;

		// Token: 0x0400010C RID: 268
		private static long oldtime = 0L;

		// Token: 0x0400010D RID: 269
		private static int ms = 0;

		// Token: 0x0400010E RID: 270
		private static bool startLogin = true;

		// Token: 0x0400010F RID: 271
		private static bool isReconnected = false;

		// Token: 0x04000110 RID: 272
		private static int TIME_INTERVAL_IN_MILLISECONDS = 1500;

		// Token: 0x04000111 RID: 273
		private static int DSTPORT = 20001;

		// Token: 0x04000112 RID: 274
		private static int SRCPORT = 20002;

		// Token: 0x04000113 RID: 275
		private static bool linked = false;

		// Token: 0x04000114 RID: 276
		private static string HOSTNAME;

		// Token: 0x04000115 RID: 277
		private static byte[] lastSession = new byte[2];

		// Token: 0x04000116 RID: 278
		public static byte smallCounter = 0;

		// Token: 0x04000117 RID: 279
		private static bool seenEnd = true;

		// Token: 0x04000118 RID: 280
		private static string setDestination = "REF001 C";

		// Token: 0x04000119 RID: 281
		private static byte[] selectedStream = new byte[2];

		// Token: 0x0200005A RID: 90
		public enum STATUS
		{
			// Token: 0x040004DA RID: 1242
			DISCONNECTED,
			// Token: 0x040004DB RID: 1243
			W_LOGIN,
			// Token: 0x040004DC RID: 1244
			W_AUTHORISATION,
			// Token: 0x040004DD RID: 1245
			W_CONFIG,
			// Token: 0x040004DE RID: 1246
			RUNNING,
			// Token: 0x040004DF RID: 1247
			REJECTED,
			// Token: 0x040004E0 RID: 1248
			BUSY
		}

		// Token: 0x0200005B RID: 91
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040004E1 RID: 1249
			public static TimerCallback <0>__writePing;
		}
	}
}
