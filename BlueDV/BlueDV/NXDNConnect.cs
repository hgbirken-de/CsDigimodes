using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BlueDV
{
	// Token: 0x02000030 RID: 48
	internal class NXDNConnect
	{
		// Token: 0x17000039 RID: 57
		// (get) Token: 0x06000387 RID: 903 RVA: 0x00026AA0 File Offset: 0x00024CA0
		// (set) Token: 0x06000388 RID: 904 RVA: 0x00026AA7 File Offset: 0x00024CA7
		public static string StatusTextNXDN { get; private set; }

		// Token: 0x1400001A RID: 26
		// (add) Token: 0x06000389 RID: 905 RVA: 0x00026AB0 File Offset: 0x00024CB0
		// (remove) Token: 0x0600038A RID: 906 RVA: 0x00026AE4 File Offset: 0x00024CE4
		public static event EventHandler StatusTextChangedNXDN;

		// Token: 0x0600038B RID: 907 RVA: 0x00026B17 File Offset: 0x00024D17
		private static bool canStream()
		{
			if (information.stream_modus == information.MODUS.IDLE)
			{
				DVMEGASerial.setmode2NXDN();
				information.stream_modus = information.MODUS.NXDN;
			}
			if (information.stream_modus == information.MODUS.NXDN)
			{
				modeTimer.resetTimer();
				return true;
			}
			return false;
		}

		// Token: 0x0600038C RID: 908 RVA: 0x00026B3C File Offset: 0x00024D3C
		private static void ping()
		{
			string text = "NXDNP" + information.myCall.PadRight(10);
			byte[] array = new byte[17];
			Buffer.BlockCopy(Encoding.ASCII.GetBytes(text), 0, array, 0, 15);
			array[15] = (byte)((NXDNConnect.dstID >> 8) & 255);
			array[16] = (byte)(NXDNConnect.dstID & 255);
			NXDNConnect.sendRAW(array, 17);
		}

		// Token: 0x0600038D RID: 909 RVA: 0x00026BA8 File Offset: 0x00024DA8
		private static void disconnect()
		{
			string text = "NXDNU" + information.myCall.PadRight(10);
			byte[] array = new byte[17];
			Buffer.BlockCopy(Encoding.ASCII.GetBytes(text), 0, array, 0, 15);
			array[15] = (byte)((NXDNConnect.dstID >> 8) & 255);
			array[16] = (byte)(NXDNConnect.dstID & 255);
			NXDNConnect.sendRAW(array, 17);
		}

		// Token: 0x0600038E RID: 910 RVA: 0x00026C14 File Offset: 0x00024E14
		public static void open()
		{
			NXDNConnect.m_status = NXDNConnect.STATUS.W_LOGIN;
			NXDNConnect.ping();
			TimerCallback timerCallback;
			if ((timerCallback = NXDNConnect.<>O.<0>__writePing) == null)
			{
				timerCallback = (NXDNConnect.<>O.<0>__writePing = new TimerCallback(NXDNConnect.writePing));
			}
			NXDNConnect._timer = new global::System.Threading.Timer(timerCallback, null, NXDNConnect.TIME_INTERVAL_IN_MILLISECONDS, -1);
			TimerCallback timerCallback2;
			if ((timerCallback2 = NXDNConnect.<>O.<1>__pongCheck) == null)
			{
				timerCallback2 = (NXDNConnect.<>O.<1>__pongCheck = new TimerCallback(NXDNConnect.pongCheck));
			}
			NXDNConnect._timer_pongcheck = new global::System.Threading.Timer(timerCallback2, null, NXDNConnect.TIME_INTERVAL_IN_MILLISECONDS_PONG, -1);
		}

		// Token: 0x0600038F RID: 911 RVA: 0x00026C84 File Offset: 0x00024E84
		private static void pongCheck(object state)
		{
			if (DateTime.Now.Ticks / 10000L - NXDNConnect.last_pong > (long)NXDNConnect.TIME_INTERVAL_IN_MILLISECONDS_PONG)
			{
				NXDNConnect.close();
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					NXDNConnect.ChangeStatusTextNXDN("Reconnecting ");
					break;
				case information.LANGUAGE.JAPANESE:
					NXDNConnect.ChangeStatusTextNXDN("再接続する");
					break;
				case information.LANGUAGE.CHINEES:
					NXDNConnect.ChangeStatusTextNXDN("Reconnecting ");
					break;
				case information.LANGUAGE.KOREAN:
					NXDNConnect.ChangeStatusTextNXDN("재연결 ");
					break;
				}
			}
			try
			{
				if (NXDNConnect._timer_pongcheck != null)
				{
					NXDNConnect._timer_pongcheck.Change(NXDNConnect.TIME_INTERVAL_IN_MILLISECONDS_PONG, -1);
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000390 RID: 912 RVA: 0x00026D34 File Offset: 0x00024F34
		public static void connect(string hostname, int port, int dest)
		{
			NXDNConnect.HOSTNAME = hostname;
			NXDNConnect.PORT = port;
			NXDNConnect.dstID = dest;
			try
			{
				NXDNConnect.srcID = int.Parse(information.AMBENXDNid);
			}
			catch (Exception)
			{
				MessageBox.Show("You have issues with your NXDN id! Fix in the BlueDV setup");
			}
			try
			{
				NXDNConnect.udpClient = new UdpClient();
			}
			catch (SocketException)
			{
				return;
			}
			try
			{
				NXDNConnect.udpClient.Connect(NXDNConnect.HOSTNAME, NXDNConnect.PORT);
				UdpClient udpClient = NXDNConnect.udpClient;
				AsyncCallback asyncCallback;
				if ((asyncCallback = NXDNConnect.<>O.<2>__receiveDataFromInternet) == null)
				{
					asyncCallback = (NXDNConnect.<>O.<2>__receiveDataFromInternet = new AsyncCallback(NXDNConnect.receiveDataFromInternet));
				}
				udpClient.BeginReceive(asyncCallback, NXDNConnect.udpClient);
				NXDNConnect.open();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					NXDNConnect.ChangeStatusTextNXDN("ERR: " + ex.Message);
					break;
				case information.LANGUAGE.JAPANESE:
					NXDNConnect.ChangeStatusTextNXDN("エラー： " + ex.Message);
					break;
				case information.LANGUAGE.CHINEES:
					NXDNConnect.ChangeStatusTextNXDN("ERR: " + ex.Message);
					break;
				case information.LANGUAGE.KOREAN:
					NXDNConnect.ChangeStatusTextNXDN("ERR: " + ex.Message);
					break;
				}
			}
		}

		// Token: 0x06000391 RID: 913 RVA: 0x00026E84 File Offset: 0x00025084
		private static void ChangeStatusTextNXDN(string text)
		{
			NXDNConnect.StatusTextNXDN = text;
			EventHandler statusTextChangedNXDN = NXDNConnect.StatusTextChangedNXDN;
			if (statusTextChangedNXDN != null)
			{
				statusTextChangedNXDN(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000392 RID: 914 RVA: 0x00026EAC File Offset: 0x000250AC
		public static void close()
		{
			if (NXDNConnect.udpClient != null)
			{
				try
				{
					NXDNConnect.disconnect();
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						NXDNConnect.ChangeStatusTextNXDN("Not linked");
						break;
					case information.LANGUAGE.JAPANESE:
						NXDNConnect.ChangeStatusTextNXDN("未接続");
						break;
					case information.LANGUAGE.CHINEES:
						NXDNConnect.ChangeStatusTextNXDN("Not linked");
						break;
					case information.LANGUAGE.KOREAN:
						NXDNConnect.ChangeStatusTextNXDN("연결안됨");
						break;
					}
					NXDNConnect._timer.Dispose();
					NXDNConnect._timer_pongcheck.Dispose();
					NXDNConnect.udpClient.Client.Shutdown(SocketShutdown.Both);
					NXDNConnect.udpClient.Close();
					UdpClient udpClient = NXDNConnect.udpClient;
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x06000393 RID: 915 RVA: 0x00026F60 File Offset: 0x00025160
		private static void receiveDataFromInternet(IAsyncResult ar)
		{
			try
			{
				UdpClient udpClient = (UdpClient)ar.AsyncState;
				IPEndPoint ipendPoint = new IPEndPoint(IPAddress.Any, 0);
				byte[] array = udpClient.EndReceive(ar, ref ipendPoint);
				int num = array.Length;
				if (num > 3)
				{
					string @string = Encoding.ASCII.GetString(array);
					if (@string.StartsWith("NXDND") && num == 43)
					{
						int num2 = ((int)(byte.MaxValue & array[5]) << 8) | (int)(byte.MaxValue & array[6]);
						information.hisNXDNID = num2.ToString();
						if (NXDNConnect.canStream())
						{
							byte b = array[7];
							byte b2 = array[8];
							NXDNConnect.currentVoiceTime = DateTime.Now.Ticks / 10000L;
							NXDNLogic.set_lich(array[10]);
							if (NXDNLogic.get_lich_fct(array[10]) == NXDNConnect.NXDN_LICH_USC_SACCH_NS)
							{
								if ((array[9] & 8) == 8)
								{
									goto IL_034B;
								}
								try
								{
									byte[] array2 = new byte[4];
									Buffer.BlockCopy(array, 11, array2, 0, 4);
									NXDNLogic.set_sacch(array2);
									goto IL_034B;
								}
								catch (Exception)
								{
									goto IL_034B;
								}
							}
							NXDNLogic.set_lich(array[10]);
							byte[] array3 = new byte[4];
							bool flag = false;
							Buffer.BlockCopy(array, 11, array3, 0, 4);
							NXDNLogic.set_sacch(array3);
							if (NXDNLogic.get_lich_rfct() == 2 && NXDNLogic.get_lich_fct() == 2)
							{
								int lich_option = NXDNLogic.get_lich_option();
							}
							switch (NXDNLogic.get_sacch_struct())
							{
							case 0:
								NXDNLogic.layer3_decode(NXDNLogic.get_sacch_data(), 18, 54);
								if (flag)
								{
									NXDNLogic.get_message_type((int)NXDNLogic.get_layer3_msgtype());
								}
								if (NXDNLogic.get_layer3_msgtype() != 63 && NXDNLogic.get_layer3_msgtype() == 0)
								{
									if (array[14] == 0 && array[15] == 2)
									{
										for (int i = 15; i < 39; i++)
										{
										}
									}
									if (NXDNLogic.get_lich_fct() == 1 && NXDNLogic.get_lich_option() == 3)
									{
										int lich_rfct = NXDNLogic.get_lich_rfct();
									}
								}
								NXDNLogic.m_layer3 = new byte[22];
								break;
							case 1:
								NXDNLogic.layer3_decode(NXDNLogic.get_sacch_data(), 18, 36);
								break;
							case 2:
								NXDNLogic.layer3_decode(NXDNLogic.get_sacch_data(), 18, 18);
								break;
							case 3:
								NXDNLogic.layer3_decode(NXDNLogic.get_sacch_data(), 18, 0);
								break;
							}
							if (NXDNLogic.get_lich_fct() == 2 && NXDNLogic.get_lich_option() == 3 && NXDNLogic.get_lich_rfct() == 2)
							{
								byte[] array4 = new byte[7];
								Buffer.BlockCopy(array, 15, array4, 0, 7);
								NXDNConnect.fromNXDN(NXDNConnect.interleave(array4), 7);
								byte[] array5 = new byte[7];
								byte[] array6 = new byte[7];
								Buffer.BlockCopy(array, 21, array5, 0, 7);
								for (int j = 0; j < 6; j++)
								{
									array6[j] = (byte)((byte.MaxValue & array5[j]) << 1);
									byte[] array7 = array6;
									int num3 = j;
									array7[num3] |= (byte)(1 & ((byte.MaxValue & array5[j + 1]) >> 7));
								}
								array6[6] = (byte)((byte.MaxValue & array5[6]) << 1);
								NXDNConnect.fromNXDN(NXDNConnect.interleave(array6), 7);
								byte[] array8 = new byte[7];
								Buffer.BlockCopy(array, 29, array8, 0, 7);
								NXDNConnect.fromNXDN(NXDNConnect.interleave(array8), 7);
								byte[] array9 = new byte[7];
								byte[] array10 = new byte[7];
								Buffer.BlockCopy(array, 35, array9, 0, 7);
								for (int k = 0; k < 6; k++)
								{
									array10[k] = (byte)((byte.MaxValue & array9[k]) << 1);
									byte[] array11 = array10;
									int num4 = k;
									array11[num4] |= (byte)(1 & (array9[k + 1] >> 7));
								}
								array10[6] = (byte)((byte.MaxValue & array9[6]) << 1);
								NXDNConnect.fromNXDN(NXDNConnect.interleave(array10), 7);
							}
							IL_034B:
							NXDNConnect.voiceTimer = DateTime.Now.Ticks / 10000L;
							TimerRXTX.TX();
						}
						ShowCallTimer.resetTimerNXDN(num2.ToString());
					}
					if (@string.StartsWith("NXDNP") && num > 6)
					{
						int num5 = ((int)(byte.MaxValue & array[15]) << 8) | (int)(byte.MaxValue & array[16]);
						switch (information.m_language)
						{
						case information.LANGUAGE.ENGLISH:
							NXDNConnect.ChangeStatusTextNXDN("Linked to " + num5.ToString());
							break;
						case information.LANGUAGE.JAPANESE:
							NXDNConnect.ChangeStatusTextNXDN("接続中 " + num5.ToString());
							break;
						case information.LANGUAGE.CHINEES:
							NXDNConnect.ChangeStatusTextNXDN("Linked to " + num5.ToString());
							break;
						case information.LANGUAGE.KOREAN:
							NXDNConnect.ChangeStatusTextNXDN("연결됨 " + num5.ToString());
							break;
						}
						NXDNConnect.last_pong = DateTime.Now.Ticks / 10000L;
					}
				}
				udpClient.BeginReceive(new AsyncCallback(NXDNConnect.receiveDataFromInternet), ar.AsyncState);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception)
			{
				NXDNConnect.ChangeStatusTextNXDN("Connection error");
			}
		}

		// Token: 0x06000394 RID: 916 RVA: 0x00027428 File Offset: 0x00025628
		public static void makeEndFrame(byte[] david, bool end)
		{
			NXDNConnect.getFrame(david, end);
		}

		// Token: 0x06000395 RID: 917 RVA: 0x00027432 File Offset: 0x00025632
		public static void increment_session_id()
		{
			NXDNConnect.m_txcnt = 0;
		}

		// Token: 0x06000396 RID: 918 RVA: 0x0002743C File Offset: 0x0002563C
		private static void fromNXDN(byte[] voice, int length)
		{
			byte[] array = new byte[124];
			array[0] = 224;
			array[1] = (byte)(length + 4);
			array[2] = 64;
			Buffer.BlockCopy(voice, 0, array, 3, length);
			productSelector.write(array);
		}

		// Token: 0x06000397 RID: 919 RVA: 0x00027475 File Offset: 0x00025675
		private static void writePing(object o)
		{
			NXDNConnect.ping();
			if (NXDNConnect._timer != null)
			{
				NXDNConnect._timer.Change(NXDNConnect.TIME_INTERVAL_IN_MILLISECONDS, -1);
			}
		}

		// Token: 0x06000398 RID: 920 RVA: 0x00027494 File Offset: 0x00025694
		public static void sendRAW(byte[] data, int length)
		{
			try
			{
				NXDNConnect.udpClient.Send(data, length);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000399 RID: 921 RVA: 0x000274C4 File Offset: 0x000256C4
		private static byte[] interleave(byte[] ambe)
		{
			byte[] array = new byte[49];
			byte[] array2 = new byte[7];
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					array[j + 8 * i] = (byte)(1 & (ambe[i] >> 7 - j));
				}
			}
			array[48] = (byte)(1 & (ambe[6] >> 7));
			for (int k = 0; k < 49; k++)
			{
				int num = NXDNLogic.dvsi_interleave[k];
				byte[] array3 = array2;
				int num2 = num / 8;
				array3[num2] += (byte)((byte.MaxValue & array[k]) << 7 - num % 8);
			}
			return array2;
		}

		// Token: 0x0600039A RID: 922 RVA: 0x00027558 File Offset: 0x00025758
		public static byte[] getFrame(byte[] data, bool m_eot)
		{
			NXDNConnect.m_nxdnframe = new byte[55];
			NXDNConnect.m_nxdnframe[0] = 78;
			NXDNConnect.m_nxdnframe[1] = 88;
			NXDNConnect.m_nxdnframe[2] = 68;
			NXDNConnect.m_nxdnframe[3] = 78;
			NXDNConnect.m_nxdnframe[4] = 68;
			NXDNConnect.m_nxdnframe[5] = (byte)((NXDNConnect.srcID >> 8) & 255);
			NXDNConnect.m_nxdnframe[6] = (byte)(NXDNConnect.srcID & 255);
			NXDNConnect.m_nxdnframe[7] = (byte)((NXDNConnect.dstID >> 8) & 255);
			NXDNConnect.m_nxdnframe[8] = (byte)(NXDNConnect.dstID & 255);
			NXDNConnect.m_nxdnframe[9] = 0;
			byte[] nxdnframe = NXDNConnect.m_nxdnframe;
			int num = 9;
			nxdnframe[num] |= ((NXDNLogic.group > false) ? 1 : 0);
			for (int i = 0; i < 4; i++)
			{
				byte[] array = new byte[7];
				Buffer.BlockCopy(data, 7 * i, array, 0, 7);
				Buffer.BlockCopy(NXDNLogic.deinterleave_ambe(array), 0, data, 7 * i, 7);
			}
			if (NXDNConnect.m_txcnt < 1 || m_eot)
			{
				NXDNConnect.encode_header(m_eot);
			}
			else
			{
				NXDNConnect.encode_data(data);
			}
			if (NXDNConnect.m_nxdnframe[10] == 129 || NXDNConnect.m_nxdnframe[10] == 131)
			{
				byte[] nxdnframe2 = NXDNConnect.m_nxdnframe;
				int num2 = 9;
				nxdnframe2[num2] |= ((NXDNConnect.m_nxdnframe[15] == 1) ? 4 : 0);
				byte[] nxdnframe3 = NXDNConnect.m_nxdnframe;
				int num3 = 9;
				nxdnframe3[num3] |= ((NXDNConnect.m_nxdnframe[15] == 8) ? 8 : 0);
			}
			else if ((NXDNConnect.m_nxdnframe[10] & 240) == 144)
			{
				byte[] nxdnframe4 = NXDNConnect.m_nxdnframe;
				int num4 = 9;
				nxdnframe4[num4] |= 2;
				if (NXDNConnect.m_nxdnframe[10] == 144 || NXDNConnect.m_nxdnframe[10] == 146 || NXDNConnect.m_nxdnframe[10] == 156 || NXDNConnect.m_nxdnframe[10] == 158)
				{
					byte[] nxdnframe5 = NXDNConnect.m_nxdnframe;
					int num5 = 9;
					nxdnframe5[num5] |= ((NXDNConnect.m_nxdnframe[12] == 9) ? 4 : 0);
					byte[] nxdnframe6 = NXDNConnect.m_nxdnframe;
					int num6 = 9;
					nxdnframe6[num6] |= ((NXDNConnect.m_nxdnframe[12] == 8) ? 8 : 0);
				}
			}
			if (m_eot)
			{
				NXDNConnect.m_txcnt = 0;
				m_eot = false;
			}
			else
			{
				NXDNConnect.m_txcnt++;
			}
			NXDNConnect.sendRAW(NXDNConnect.m_nxdnframe, 43);
			return NXDNConnect.m_nxdnframe;
		}

		// Token: 0x0600039B RID: 923 RVA: 0x00027790 File Offset: 0x00025990
		private static void encode_header(bool m_eot)
		{
			NXDNLogic.m_lich = 0;
			NXDNLogic.m_sacch = new byte[5];
			NXDNLogic.m_layer3 = new byte[22];
			NXDNLogic.set_lich_rfct(NXDNConnect.NXDN_LICH_RFCT_RDCH);
			NXDNLogic.set_lich_fct((int)NXDNConnect.NXDN_LICH_USC_SACCH_NS);
			NXDNLogic.set_lich_option(NXDNConnect.NXDN_LICH_STEAL_FACCH);
			NXDNLogic.set_lich_direction(NXDNConnect.NXDN_LICH_DIRECTION_INBOUND);
			NXDNConnect.m_nxdnframe[10] = NXDNLogic.get_lich();
			NXDNLogic.set_sacch_ran(1);
			NXDNLogic.set_sacch_struct(0);
			NXDNLogic.set_sacch_data(NXDNLogic.idle);
			NXDNConnect.m_nxdnframe[11] = NXDNLogic.get_sacch()[0];
			NXDNConnect.m_nxdnframe[12] = NXDNLogic.get_sacch()[1];
			NXDNConnect.m_nxdnframe[13] = NXDNLogic.get_sacch()[2];
			NXDNConnect.m_nxdnframe[14] = NXDNLogic.get_sacch()[3];
			if (m_eot)
			{
				NXDNLogic.set_layer3_msgtype(NXDNConnect.NXDN_MESSAGE_TYPE_TX_REL);
			}
			else
			{
				NXDNLogic.set_layer3_msgtype(NXDNConnect.NXDN_MESSAGE_TYPE_VCALL);
			}
			NXDNLogic.set_layer3_srcid(NXDNConnect.srcID);
			NXDNLogic.set_layer3_dstid(NXDNConnect.dstID);
			NXDNLogic.set_layer3_grp(NXDNLogic.group);
			NXDNLogic.set_layer3_blks(0);
			Buffer.BlockCopy(NXDNLogic.m_layer3, 0, NXDNConnect.m_nxdnframe, 15, 14);
			Buffer.BlockCopy(NXDNLogic.m_layer3, 0, NXDNConnect.m_nxdnframe, 29, 14);
		}

		// Token: 0x0600039C RID: 924 RVA: 0x000278A8 File Offset: 0x00025AA8
		private static void encode_data(byte[] data)
		{
			byte[] array = new byte[3];
			NXDNLogic.m_lich = 0;
			NXDNLogic.m_sacch = new byte[5];
			NXDNLogic.m_layer3 = new byte[22];
			NXDNLogic.set_lich_rfct(NXDNConnect.NXDN_LICH_RFCT_RDCH);
			NXDNLogic.set_lich_fct((int)NXDNConnect.NXDN_LICH_USC_SACCH_SS);
			NXDNLogic.set_lich_option(NXDNConnect.NXDN_LICH_STEAL_NONE);
			NXDNLogic.set_lich_direction(NXDNConnect.NXDN_LICH_DIRECTION_INBOUND);
			NXDNConnect.m_nxdnframe[10] = NXDNLogic.get_lich();
			NXDNLogic.set_sacch_ran(1);
			NXDNLogic.set_layer3_msgtype(NXDNConnect.NXDN_MESSAGE_TYPE_VCALL);
			NXDNLogic.set_layer3_srcid(NXDNConnect.srcID);
			NXDNLogic.set_layer3_dstid(NXDNConnect.dstID);
			NXDNLogic.set_layer3_grp(NXDNLogic.group);
			NXDNLogic.set_layer3_blks(0);
			switch (NXDNConnect.m_txcnt % 4)
			{
			case 0:
				NXDNLogic.set_sacch_struct(3);
				NXDNLogic.set_sacch_data(NXDNLogic.layer3_encode(array, 18, 0));
				break;
			case 1:
				NXDNLogic.set_sacch_struct(2);
				NXDNLogic.set_sacch_data(NXDNLogic.layer3_encode(array, 18, 18));
				break;
			case 2:
				NXDNLogic.set_sacch_struct(1);
				NXDNLogic.set_sacch_data(NXDNLogic.layer3_encode(array, 18, 36));
				break;
			case 3:
				NXDNLogic.set_sacch_struct(0);
				NXDNLogic.set_sacch_data(NXDNLogic.layer3_encode(array, 18, 54));
				break;
			}
			NXDNConnect.m_nxdnframe[11] = NXDNLogic.get_sacch()[0];
			NXDNConnect.m_nxdnframe[12] = NXDNLogic.get_sacch()[1];
			NXDNConnect.m_nxdnframe[13] = NXDNLogic.get_sacch()[2];
			NXDNConnect.m_nxdnframe[14] = NXDNLogic.get_sacch()[3];
			Buffer.BlockCopy(data, 0, NXDNConnect.m_nxdnframe, 15, 7);
			for (int i = 0; i < 7; i++)
			{
				byte[] nxdnframe = NXDNConnect.m_nxdnframe;
				int num = 21 + i;
				nxdnframe[num] |= (byte)(data[7 + i] >> 1);
				NXDNConnect.m_nxdnframe[22 + i] = (byte)((byte.MaxValue & data[7 + i] & 1) << 7);
			}
			byte[] nxdnframe2 = NXDNConnect.m_nxdnframe;
			int num2 = 28;
			nxdnframe2[num2] |= (byte)(data[13] >> 2);
			Buffer.BlockCopy(data, 14, NXDNConnect.m_nxdnframe, 29, 7);
			for (int j = 0; j < 7; j++)
			{
				byte[] nxdnframe3 = NXDNConnect.m_nxdnframe;
				int num3 = 35 + j;
				nxdnframe3[num3] |= (byte)(data[21 + j] >> 1);
				NXDNConnect.m_nxdnframe[36 + j] = (byte)((byte.MaxValue & data[21 + j] & 1) << 7);
			}
			byte[] nxdnframe4 = NXDNConnect.m_nxdnframe;
			int num4 = 41;
			nxdnframe4[num4] |= (byte)(data[27] >> 2);
		}

		// Token: 0x0400025A RID: 602
		private static byte NXDN_LICH_RFCT_RDCH = 2;

		// Token: 0x0400025B RID: 603
		private static byte NXDN_LICH_USC_SACCH_NS = 0;

		// Token: 0x0400025C RID: 604
		private static byte NXDN_LICH_USC_SACCH_SS = 2;

		// Token: 0x0400025D RID: 605
		private static byte NXDN_LICH_STEAL_FACCH = 0;

		// Token: 0x0400025E RID: 606
		private static byte NXDN_LICH_STEAL_NONE = 3;

		// Token: 0x0400025F RID: 607
		private static byte NXDN_LICH_DIRECTION_INBOUND = 0;

		// Token: 0x04000260 RID: 608
		private static byte NXDN_LICH_DIRECTION_OUTBOUND = 1;

		// Token: 0x04000261 RID: 609
		private static byte NXDN_MESSAGE_TYPE_VCALL = 1;

		// Token: 0x04000262 RID: 610
		private static byte NXDN_MESSAGE_TYPE_TX_REL = 8;

		// Token: 0x04000263 RID: 611
		private static UdpClient udpClient;

		// Token: 0x04000264 RID: 612
		private static global::System.Threading.Timer _timer;

		// Token: 0x04000265 RID: 613
		private static global::System.Threading.Timer _timer_pongcheck;

		// Token: 0x04000266 RID: 614
		private static int TIME_INTERVAL_IN_MILLISECONDS = 3000;

		// Token: 0x04000267 RID: 615
		private static int TIME_INTERVAL_IN_MILLISECONDS_PONG = 20000;

		// Token: 0x04000268 RID: 616
		private static int PORT = 41400;

		// Token: 0x04000269 RID: 617
		private static string HOSTNAME = "";

		// Token: 0x0400026A RID: 618
		private static byte[] m_nxdnframe = new byte[55];

		// Token: 0x0400026B RID: 619
		private static byte[] m_ambe = new byte[36];

		// Token: 0x0400026C RID: 620
		private static int m_txcnt = 0;

		// Token: 0x0400026D RID: 621
		private static long currentVoiceTime = 0L;

		// Token: 0x0400026E RID: 622
		public static long voiceTimer = 0L;

		// Token: 0x0400026F RID: 623
		private static int srcID = 0;

		// Token: 0x04000270 RID: 624
		private static int dstID = 0;

		// Token: 0x04000273 RID: 627
		private static long last_pong = 0L;

		// Token: 0x04000274 RID: 628
		private static NXDNConnect.STATUS m_status = NXDNConnect.STATUS.DISCONNECTED;

		// Token: 0x0200007C RID: 124
		public enum STATUS
		{
			// Token: 0x040004FE RID: 1278
			DISCONNECTED,
			// Token: 0x040004FF RID: 1279
			W_LOGIN,
			// Token: 0x04000500 RID: 1280
			W_AUTHORISATION,
			// Token: 0x04000501 RID: 1281
			W_CONFIG,
			// Token: 0x04000502 RID: 1282
			RUNNING
		}

		// Token: 0x0200007D RID: 125
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04000503 RID: 1283
			public static TimerCallback <0>__writePing;

			// Token: 0x04000504 RID: 1284
			public static TimerCallback <1>__pongCheck;

			// Token: 0x04000505 RID: 1285
			public static AsyncCallback <2>__receiveDataFromInternet;
		}
	}
}
