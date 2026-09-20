using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BlueDV
{
	// Token: 0x02000026 RID: 38
	internal class DVMEGASerial
	{
		// Token: 0x17000030 RID: 48
		// (get) Token: 0x06000221 RID: 545 RVA: 0x000143EE File Offset: 0x000125EE
		// (set) Token: 0x06000222 RID: 546 RVA: 0x000143F5 File Offset: 0x000125F5
		public static string StatusText { get; private set; }

		// Token: 0x14000013 RID: 19
		// (add) Token: 0x06000223 RID: 547 RVA: 0x00014400 File Offset: 0x00012600
		// (remove) Token: 0x06000224 RID: 548 RVA: 0x00014434 File Offset: 0x00012634
		public static event EventHandler StatusTextChanged;

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x06000225 RID: 549 RVA: 0x00014467 File Offset: 0x00012667
		// (set) Token: 0x06000226 RID: 550 RVA: 0x0001446E File Offset: 0x0001266E
		public static string StatusErrorText { get; private set; }

		// Token: 0x14000014 RID: 20
		// (add) Token: 0x06000227 RID: 551 RVA: 0x00014478 File Offset: 0x00012678
		// (remove) Token: 0x06000228 RID: 552 RVA: 0x000144AC File Offset: 0x000126AC
		public static event EventHandler StatusErrorTextChanged;

		// Token: 0x06000229 RID: 553 RVA: 0x000144DF File Offset: 0x000126DF
		public static bool isOpen()
		{
			return DVMEGASerial.serialPort1 != null && DVMEGASerial.serialPort1.IsOpen;
		}

		// Token: 0x0600022A RID: 554 RVA: 0x000144F4 File Offset: 0x000126F4
		public static void ping()
		{
			byte[] array = new byte[3];
			array[0] = 3;
			array[1] = 96;
			DVMEGASerial.write2(array);
		}

		// Token: 0x0600022B RID: 555 RVA: 0x0001450C File Offset: 0x0001270C
		private static void port_PinChanged(object sender, SerialPinChangedEventArgs e)
		{
			if (e.EventType != SerialPinChange.Break && e.EventType != SerialPinChange.CDChanged)
			{
				if (e.EventType == SerialPinChange.CtsChanged)
				{
					bool ctsHolding = DVMEGASerial.serialPort1.CtsHolding;
					return;
				}
				if (e.EventType != SerialPinChange.DsrChanged)
				{
					SerialPinChange eventType = e.EventType;
				}
			}
		}

		// Token: 0x0600022C RID: 556 RVA: 0x0001455C File Offset: 0x0001275C
		public static bool open()
		{
			if (DVMEGASerial.serialPort1 == null)
			{
				if (information.myCOMPort.Length == 0)
				{
					information.myCOMPort = "COM1";
				}
				DVMEGASerial.serialPort1 = new SerialPort();
			}
			switch (information.m_device)
			{
			case information.DEVICE.DVMEGARADIO:
				if (string.IsNullOrEmpty(information.myCOMPort))
				{
					DVMEGASerial.serialPort1.Close();
					return false;
				}
				break;
			case information.DEVICE.DV3000R:
				if (string.IsNullOrEmpty(information.myCOMPortAMBE))
				{
					DVMEGASerial.serialPort1.Close();
					return false;
				}
				break;
			case information.DEVICE.DVAP:
				if (string.IsNullOrEmpty(information.myCOMPort))
				{
					DVMEGASerial.serialPort1.Close();
					return false;
				}
				break;
			}
			bool flag;
			try
			{
				if (!DVMEGASerial.serialPort1.IsOpen)
				{
					switch (information.m_device)
					{
					case information.DEVICE.DVMEGARADIO:
						DVMEGASerial.serialPort1.PortName = information.myCOMPort;
						DVMEGASerial.serialPort1.BaudRate = 115200;
						if (information.invertDTRRadio)
						{
							DVMEGASerial.serialPort1.DtrEnable = true;
						}
						else
						{
							DVMEGASerial.serialPort1.DtrEnable = false;
						}
						break;
					case information.DEVICE.DV3000R:
					{
						DVMEGASerial.serialPort1.PortName = information.myCOMPortAMBE;
						if (!information.ZUMAMBE)
						{
							DVMEGASerial.serialPort1.DtrEnable = true;
						}
						if (information.ZUMAMBE)
						{
							DVMEGASerial.serialPort1.DtrEnable = false;
							DVMEGASerial.serialPort1.RtsEnable = false;
							DVMEGASerial.serialPort1.RtsEnable = true;
							DVMEGASerial.serialPort1.RtsEnable = false;
						}
						information.AMBESPEED ambespeed = information.m_ambespeed;
						if (ambespeed != information.AMBESPEED.SLOW)
						{
							if (ambespeed != information.AMBESPEED.HIGH)
							{
								if (ambespeed == information.AMBESPEED.SUPERHIGH)
								{
									DVMEGASerial.serialPort1.BaudRate = 921600;
								}
							}
							else
							{
								DVMEGASerial.serialPort1.BaudRate = 460800;
							}
						}
						else
						{
							DVMEGASerial.serialPort1.BaudRate = 230400;
						}
						break;
					}
					case information.DEVICE.DVAP:
						DVMEGASerial.serialPort1.PortName = information.myCOMPort;
						DVMEGASerial.serialPort1.BaudRate = 230400;
						DVMEGASerial.serialPort1.DtrEnable = true;
						break;
					}
					DVMEGASerial.serialPort1.DataBits = 8;
					DVMEGASerial.serialPort1.Parity = Parity.None;
					DVMEGASerial.serialPort1.StopBits = StopBits.One;
					DVMEGASerial.serialPort1.Handshake = Handshake.None;
					if (!information.ZUMAMBE)
					{
						DVMEGASerial.serialPort1.RtsEnable = false;
					}
					DVMEGASerial.serialPort1.ReadTimeout = 5000;
					DVMEGASerial.serialPort1.WriteTimeout = -1;
					DVMEGASerial.serialPort1.WriteBufferSize = 2048;
					DVMEGASerial.serialPort1.PinChanged += DVMEGASerial.port_PinChanged;
					DVMEGASerial.serialPort1.Open();
					DVMEGASerial.running = true;
					new Thread(new ThreadStart(DVMEGASerial.dataReceived)).Start();
				}
				flag = true;
			}
			catch (IOException)
			{
				try
				{
					DVMEGASerial.serialPort1.Close();
				}
				catch (Exception)
				{
				}
				flag = false;
			}
			catch (IndexOutOfRangeException)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					DVMEGASerial.ChangeStatusText("Serial port issue");
					break;
				case information.LANGUAGE.JAPANESE:
					DVMEGASerial.ChangeStatusText("シリアルポート故障");
					break;
				case information.LANGUAGE.CHINEES:
					DVMEGASerial.ChangeStatusText("Serial port issue");
					break;
				case information.LANGUAGE.KOREAN:
					DVMEGASerial.ChangeStatusText("시리얼포트 문제");
					break;
				}
				flag = false;
			}
			catch (UnauthorizedAccessException)
			{
				flag = false;
			}
			catch (Exception)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					DVMEGASerial.ChangeStatusText("Serial port issue");
					break;
				case information.LANGUAGE.JAPANESE:
					DVMEGASerial.ChangeStatusText("シリアルポート故障");
					break;
				case information.LANGUAGE.CHINEES:
					DVMEGASerial.ChangeStatusText("Serial port issue");
					break;
				case information.LANGUAGE.KOREAN:
					DVMEGASerial.ChangeStatusText("시리얼포트 문제");
					break;
				}
				flag = false;
			}
			return flag;
		}

		// Token: 0x0600022D RID: 557 RVA: 0x0001492C File Offset: 0x00012B2C
		public static void close()
		{
			if (DVMEGASerial.serialPort1 != null && DVMEGASerial.serialPort1.IsOpen)
			{
				DVMEGASerial.running = false;
				DVMEGASerial.serialPort1.Close();
				MMDVM_HS_TIMER.Stop();
			}
		}

		// Token: 0x0600022E RID: 558 RVA: 0x00002F42 File Offset: 0x00001142
		private static void SP_ErrorRecieved(object sender, SerialErrorReceivedEventArgs e)
		{
		}

		// Token: 0x0600022F RID: 559 RVA: 0x00014958 File Offset: 0x00012B58
		private static void dataReceived()
		{
			while (DVMEGASerial.running)
			{
				if (DVMEGASerial.serialPort1.IsOpen)
				{
					try
					{
						int bytesToRead = DVMEGASerial.serialPort1.BytesToRead;
						if (bytesToRead > 0)
						{
							byte[] array = new byte[bytesToRead];
							try
							{
								DVMEGASerial.serialPort1.Read(array, 0, bytesToRead);
								syslog.WriteToFile("R : " + BitConverter.ToString(array));
								foreach (byte b in array)
								{
									switch (information.m_device)
									{
									case information.DEVICE.DVMEGARADIO:
										DVMEGASerial.procesMMDVM(b);
										break;
									case information.DEVICE.DV3000R:
										DVMEGAAMBE.decodeAMBE(b);
										break;
									case information.DEVICE.DV4MINI:
										DV4MINI.decode(b);
										break;
									}
								}
							}
							catch (TimeoutException)
							{
							}
						}
					}
					catch (Exception)
					{
					}
				}
				Thread.Sleep(2);
			}
		}

		// Token: 0x06000230 RID: 560 RVA: 0x00014A34 File Offset: 0x00012C34
		private static void ChangeStatusText(string text)
		{
			DVMEGASerial.StatusText = text;
			EventHandler statusTextChanged = DVMEGASerial.StatusTextChanged;
			if (statusTextChanged != null)
			{
				statusTextChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000231 RID: 561 RVA: 0x00014A5C File Offset: 0x00012C5C
		private static void ChangeErrorStatusText(string text)
		{
			DVMEGASerial.StatusErrorText = text;
			EventHandler statusErrorTextChanged = DVMEGASerial.StatusErrorTextChanged;
			if (statusErrorTextChanged != null)
			{
				statusErrorTextChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000232 RID: 562 RVA: 0x00014A84 File Offset: 0x00012C84
		public static void createNewDMRSessionID()
		{
			DVMEGASerial.uniqSessionID = new Random().Next(254) + 1;
			DVMEGASerial.DMRsequencecounter = 0;
		}

		// Token: 0x06000233 RID: 563 RVA: 0x00014AA4 File Offset: 0x00012CA4
		private static void ReceiveFromBlueToothDVMEGA(byte[] voice, byte controlByte)
		{
			try
			{
				byte[] array = DMRsrcdstConverter.fromDVMEGA(voice, controlByte);
				if (controlByte != 65 || berCounter.getFullLC(array, controlByte))
				{
					if (controlByte != 65)
					{
						berCounter.resetCounters();
					}
					if (controlByte == 65 && !DVMEGASerial.seenStart)
					{
						DVMEGASerial.seenStart = true;
						DVMEGASerial.uniqSessionID = new Random().Next(254) + 1;
						DVMEGASerial.DMRsequencecounter = 0;
					}
					if (controlByte == 1)
					{
						TimerRXTX.RX();
					}
					if (controlByte == 66)
					{
						DVMEGASerial.seenStart = false;
					}
					byte[] array2 = new byte[55];
					array2[0] = 68;
					array2[1] = 77;
					array2[2] = 82;
					array2[3] = 68;
					array2[4] = (byte)DVMEGASerial.DMRsequencecounter;
					byte[] array3 = utils.stringto4byte(information.myDMRID);
					long num = 9L;
					long.TryParse(information.myResolvedSrcDMRID, out num);
					array2[5] = (byte)(num >> 16);
					array2[6] = (byte)(num >> 8);
					array2[7] = (byte)num;
					long num2 = 9L;
					long.TryParse(information.myResolvedDstDMRID, out num2);
					array2[8] = (byte)(num2 >> 16);
					array2[9] = (byte)(num2 >> 8);
					array2[10] = (byte)num2;
					array2[11] = array3[3];
					array2[12] = array3[2];
					array2[13] = array3[1];
					array2[14] = array3[0];
					if (controlByte == 65)
					{
						array2[15] = 24;
					}
					if (controlByte == 32)
					{
						array2[15] = 4;
					}
					if (controlByte == 1)
					{
						array2[15] = 16;
					}
					if (controlByte == 2)
					{
						array2[15] = 32;
					}
					if (controlByte == 3)
					{
						array2[15] = 48;
					}
					if (controlByte == 4)
					{
						array2[15] = 64;
					}
					if (controlByte == 5)
					{
						array2[15] = 80;
					}
					if (controlByte == 66)
					{
						array2[15] = 40;
					}
					if (controlByte == 64)
					{
						array2[15] = 8;
					}
					if (controlByte == 67)
					{
						array2[15] = 56;
					}
					if (controlByte == 68)
					{
						array2[15] = 72;
					}
					if (controlByte == 69)
					{
						array2[15] = 88;
					}
					if (controlByte == 70)
					{
						array2[15] = 104;
					}
					if (controlByte == 71)
					{
						array2[15] = 120;
					}
					if (controlByte == 72)
					{
						array2[15] = 136;
					}
					if (controlByte == 73)
					{
						array2[15] = 152;
					}
					if (controlByte == 74)
					{
						array2[15] = 168;
					}
					if (controlByte == 75)
					{
						array2[15] = 184;
					}
					if (controlByte == 76)
					{
						array2[15] = 200;
					}
					if (controlByte == 77)
					{
						array2[15] = 216;
					}
					if (controlByte == 78)
					{
						array2[15] = 232;
					}
					if (controlByte == 79)
					{
						array2[15] = 248;
					}
					array2[16] = (byte)(DVMEGASerial.uniqSessionID >> 24);
					array2[17] = (byte)(DVMEGASerial.uniqSessionID >> 16);
					array2[18] = (byte)(DVMEGASerial.uniqSessionID >> 8);
					array2[19] = (byte)DVMEGASerial.uniqSessionID;
					Buffer.BlockCopy(array, 0, array2, 20, 33);
					try
					{
						DMRconnection.sendRAW(array2, 53);
					}
					catch (Exception)
					{
					}
					DVMEGASerial.DMRsequencecounter++;
					if (DVMEGASerial.DMRsequencecounter == 256)
					{
						DVMEGASerial.DMRsequencecounter = 0;
					}
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000234 RID: 564 RVA: 0x00014D58 File Offset: 0x00012F58
		private static void ReceiveFromBlueToothDVMEGADMRPLUS(byte[] voice, byte controlByte)
		{
			byte[] array = DMRsrcdstConverter.fromDVMEGA(voice, controlByte);
			if (controlByte == 65)
			{
				berCounter.getFullLC(array, controlByte);
			}
			if (controlByte == 65 && !DVMEGASerial.seenStart)
			{
				DVMEGASerial.seenStart = true;
				DVMEGASerial.uniqSessionID = new Random().Next(254) + 1;
				DVMEGASerial.DMRsequencecounter = 0;
			}
			if (controlByte == 1)
			{
				TimerRXTX.RX();
				DVMEGASerial.seenStart = false;
			}
			if (controlByte == 66)
			{
				DVMEGASerial.seenStart = false;
				DVMEGASerial.DMRsequencecounter = 0;
			}
			byte[] array2 = new byte[55];
			array2[0] = 68;
			array2[1] = 77;
			array2[2] = 82;
			array2[3] = 68;
			array2[4] = (byte)DVMEGASerial.DMRsequencecounter;
			long num = 9L;
			if (information.DMRsimpleMode)
			{
				long.TryParse(information.myDMRIDsimple, out num);
			}
			else
			{
				long.TryParse(information.myResolvedSrcDMRID, out num);
			}
			array2[5] = (byte)(num >> 16);
			array2[6] = (byte)(num >> 8);
			array2[7] = (byte)num;
			long num2 = 9L;
			if (information.DMRsimpleMode)
			{
				if (!long.TryParse(information.myAMBEDstDMRIDinput, out num2))
				{
					num2 = 9L;
				}
			}
			else if (!long.TryParse(information.myResolvedDstDMRID, out num2))
			{
				num2 = 9L;
			}
			array2[8] = (byte)(num2 >> 16);
			array2[9] = (byte)(num2 >> 8);
			array2[10] = (byte)num2;
			byte[] array3 = utils.stringto4byte(information.myDMRID);
			array2[11] = array3[3];
			array2[12] = array3[2];
			array2[13] = array3[1];
			array2[14] = array3[0];
			if (controlByte == 65)
			{
				array2[15] = 161;
			}
			if (controlByte == 32)
			{
				array2[15] = 144;
			}
			if (controlByte == 1)
			{
				array2[15] = 129;
			}
			if (controlByte == 2)
			{
				array2[15] = 130;
			}
			if (controlByte == 3)
			{
				array2[15] = 131;
			}
			if (controlByte == 4)
			{
				array2[15] = 132;
			}
			if (controlByte == 5)
			{
				array2[15] = 133;
			}
			if (controlByte == 66)
			{
				array2[15] = 162;
			}
			if (controlByte == 64)
			{
				array2[15] = 8;
			}
			if (controlByte == 67)
			{
				array2[15] = 56;
			}
			if (controlByte == 68)
			{
				array2[15] = 72;
			}
			if (controlByte == 69)
			{
				array2[15] = 88;
			}
			if (controlByte == 70)
			{
				array2[15] = 104;
			}
			if (controlByte == 71)
			{
				array2[15] = 120;
			}
			if (controlByte == 72)
			{
				array2[15] = 136;
			}
			if (controlByte == 73)
			{
				array2[15] = 152;
			}
			if (controlByte == 74)
			{
				array2[15] = 168;
			}
			if (controlByte == 75)
			{
				array2[15] = 184;
			}
			if (controlByte == 76)
			{
				array2[15] = 200;
			}
			if (controlByte == 77)
			{
				array2[15] = 216;
			}
			if (controlByte == 78)
			{
				array2[15] = 232;
			}
			if (controlByte == 79)
			{
				array2[15] = 248;
			}
			if (information.m_device == information.DEVICE.DV3000R)
			{
				if (information.m_groupprivate != information.GROUPPRIVATE.GROUP)
				{
					array2[15] = array2[15] | 64;
				}
				else
				{
					array2[15] = (byte)((int)array2[15] & -65);
				}
			}
			array2[16] = (byte)DVMEGASerial.uniqSessionID;
			array2[17] = (byte)DVMEGASerial.uniqSessionID;
			array2[18] = (byte)DVMEGASerial.uniqSessionID;
			array2[19] = (byte)DVMEGASerial.uniqSessionID;
			Buffer.BlockCopy(array, 0, array2, 20, 33);
			try
			{
				DMRPlus.sendRAW(array2, 55);
			}
			catch (Exception)
			{
			}
			DVMEGASerial.DMRsequencecounter++;
			if (DVMEGASerial.DMRsequencecounter == 256)
			{
				DVMEGASerial.DMRsequencecounter = 0;
			}
		}

		// Token: 0x06000235 RID: 565 RVA: 0x00015050 File Offset: 0x00013250
		public static void procesMMDVM(byte mmdvmData)
		{
			DVMEGASerial.serial3_bufferM[DVMEGASerial.serial3_buffer_input_pointerM] = mmdvmData;
			DVMEGASerial.serial3_buffer_input_pointerM++;
			if (DVMEGASerial.serial3_buffer_input_pointerM > DVMEGASerial.ser3_in_buf_lenM)
			{
				DVMEGASerial.serial3_buffer_input_pointerM = 0;
			}
			if (DVMEGASerial.serial3_bufferM[0] != 224)
			{
				DVMEGASerial.serial3_buffer_input_pointerM = 0;
			}
			if (DVMEGASerial.serial3_bufferM[0] == 224 && DVMEGASerial.serial3_buffer_input_pointerM >= 2 && DVMEGASerial.serial3_buffer_input_pointerM >= (int)(DVMEGASerial.serial3_bufferM[1] & 255) && DVMEGASerial.serial3_buffer_input_pointerM >= 3)
			{
				byte[] array = new byte[(int)(DVMEGASerial.serial3_bufferM[1] & byte.MaxValue)];
				Buffer.BlockCopy(DVMEGASerial.serial3_bufferM, 0, array, 0, (int)(DVMEGASerial.serial3_bufferM[1] & byte.MaxValue));
				byte b = array[2];
				if (b <= 26)
				{
					if (b <= 1)
					{
						if (b != 0)
						{
							if (b == 1)
							{
								information.DVMEGAbufferDSTAR = array[6];
								information.DVMEGAbufferDMRslot1 = array[7];
								information.DVMEGAbufferDMRslot2 = array[8];
								information.DVMEGAbufferFUSION = array[9];
							}
						}
						else
						{
							information.foundDVMEGA = true;
							byte[] array2 = new byte[(int)(DVMEGASerial.serial3_bufferM[1] - 4)];
							Buffer.BlockCopy(DVMEGASerial.serial3_bufferM, 4, array2, 0, (int)(DVMEGASerial.serial3_bufferM[1] - 4));
							string text = Encoding.ASCII.GetString(array2).Replace(" ", "_");
							information.myDVMEGAVersion = text.Substring(0, (int)(DVMEGASerial.serial3_bufferM[1] - 4)).PadRight(16).Substring(0, 16);
							DVMEGASerial.ChangeStatusText(text.Substring(0, (int)(DVMEGASerial.serial3_bufferM[1] - 4)).PadRight(16).Substring(0, 16));
							information.DVMEGAdetected = true;
							DVMEGASerial.setDMRFrequency();
							DVMEGASerial.setmode2Idle();
							if (text.StartsWith("DVMEGA_HR2") && !DVMEGASerial.seenoldVersion)
							{
								DVMEGASerial.seenoldVersion = true;
								MessageBox.Show("Fusion and DSTAR are only working on DVMEGA firmware version 3 and higher. \n\nPlease download new version at http://www.dvmega.auria.nl/Downloads.html ");
							}
						}
					}
					else
					{
						switch (b)
						{
						case 16:
							DVMEGASerial.setmode2DSTAR();
							modeTimer.setMode(information.MODUS.DSTAR);
							DCSconnection.resetCounters();
							DVMEGASerial.headerArray = DSTARhandler.processDSTARHeader(array);
							DSTARhandler.connectProcessing(DVMEGASerial.headerArray[2]);
							information.hisCall = DVMEGASerial.headerArray[3];
							information.hisCallsmall = DVMEGASerial.headerArray[4];
							if (information.m_device == information.DEVICE.DV3000R)
							{
								soundcard.beep(750.0);
							}
							break;
						case 17:
							modeTimer.setMode(information.MODUS.DSTAR);
							if (DVMEGASerial.headerArray[2].Equals("CQCQCQ  "))
							{
								DSTARhandler.processDSTARVoice(array);
							}
							TimerRXTX.RX();
							break;
						case 18:
							break;
						case 19:
							DTMF.testme(new byte[2], true);
							if (DVMEGASerial.headerArray[2].Equals("CQCQCQ  "))
							{
								DSTARhandler.processDSTAREot(array);
							}
							if (information.m_device == information.DEVICE.DV3000R)
							{
								soundcard.beep(550.0);
							}
							else
							{
								SlowData.make_free_text("BlueDV by PA7LIM");
								DPLUSconnection.beepBack();
							}
							break;
						default:
							if (b == 26)
							{
								DateTimeOffset now = DateTimeOffset.Now;
								if (now.ToUniversalTime().Ticks / 10000L - DVMEGASerial.oldTimerDMR > 1000L)
								{
									DVMEGASerial.seenStart = true;
									DVMEGASerial.uniqSessionID = new Random().Next(254) + 1;
									DVMEGASerial.DMRsequencecounter = 0;
									DVMEGASerial.oldTimerDMR = now.ToUniversalTime().Ticks;
								}
								modeTimer.setMode(information.MODUS.DMR);
								byte[] array3 = new byte[33];
								TimerRXTX.RX();
								Buffer.BlockCopy(array, 4, array3, 0, 33);
								berCounter.ber(array3, array[3]);
								DecodeDMR.decode(array3, array[3]);
								DecodeDMR.kanweg(array3, array[3]);
								berCounter.test(array3, array[3]);
								switch (information.m_dmrmodus)
								{
								case information.DMRMODUS.BM:
									DVMEGASerial.ReceiveFromBlueToothDVMEGA(array3, array[3]);
									break;
								case information.DMRMODUS.DMRPLUS:
									DVMEGASerial.ReceiveFromBlueToothDVMEGADMRPLUS(array3, array[3]);
									break;
								case information.DMRMODUS.XLXDMR:
									DVMEGASerial.ReceiveFromBlueToothDVMEGADMRPLUS(array3, array[3]);
									break;
								case information.DMRMODUS.FREEDMR:
									DVMEGASerial.ReceiveFromBlueToothDVMEGADMRPLUS(array3, array[3]);
									break;
								case information.DMRMODUS.SYSTEMX:
									DVMEGASerial.ReceiveFromBlueToothDVMEGADMRPLUS(array3, array[3]);
									break;
								case information.DMRMODUS.TGIF:
									DVMEGASerial.ReceiveFromBlueToothDVMEGADMRPLUS(array3, array[3]);
									break;
								case information.DMRMODUS.ADNSYSTEMS:
									DVMEGASerial.ReceiveFromBlueToothDVMEGADMRPLUS(array3, array[3]);
									break;
								}
							}
							break;
						}
					}
				}
				else if (b <= 64)
				{
					if (b != 32)
					{
						if (b == 64)
						{
							modeTimer.setMode(information.MODUS.NXDN);
							TimerRXTX.RX();
							byte[] array4 = new byte[7];
							Buffer.BlockCopy(array, 3, array4, 0, 7);
							NXDNLogic.makeNXDNvoiceFrameFromAMBE(array4);
						}
					}
					else
					{
						TimerRXTX.RX();
						modeTimer.setMode(information.MODUS.FUSION);
						DateTimeOffset now2 = DateTimeOffset.Now;
						if (now2.ToUniversalTime().Ticks / 10000L - DVMEGASerial.oldTimerFusion > 1000L)
						{
							DVMEGASerial.setmode2Fusion();
							DVMEGASerial.oldTimerFusion = now2.ToUniversalTime().Ticks;
						}
						switch (information.m_fusionmodus)
						{
						case information.FUSIONMODUS.YSF:
						{
							byte[] array5 = new byte[121];
							Buffer.BlockCopy(array, 3, array5, 0, 121);
							YSFFusion.makeFrame(array5);
							break;
						}
						case information.FUSIONMODUS.FCS:
						{
							byte[] array6 = new byte[120];
							Buffer.BlockCopy(array, 4, array6, 0, 120);
							FCSFusion.makeFrame(array6);
							break;
						}
						case information.FUSIONMODUS.XLXYSF:
						{
							byte[] array7 = new byte[121];
							Buffer.BlockCopy(array, 3, array7, 0, 121);
							YSFFusion.makeFrame(array7);
							break;
						}
						}
					}
				}
				else if (b != 112)
				{
					if (b == 127)
					{
						syslog.WriteToFile("ERR1 : " + BitConverter.ToString(array));
					}
				}
				DVMEGASerial.serial3_buffer_input_pointerM = 0;
				DVMEGASerial.serial3_bufferM[0] = 0;
			}
		}

		// Token: 0x06000236 RID: 566 RVA: 0x000155A4 File Offset: 0x000137A4
		public static void write2(byte[] data)
		{
			if (DVMEGASerial.serialPort1 == null)
			{
				return;
			}
			if (!DVMEGASerial.serialPort1.IsOpen)
			{
				return;
			}
			try
			{
				syslog.WriteToFile("W : " + BitConverter.ToString(data));
				DVMEGASerial.serialPort1.Write(data, 0, data.Length);
			}
			catch (TimeoutException)
			{
				try
				{
					MessageBox.Show("COM port timeout");
					DVMEGASerial.close();
				}
				catch (Exception)
				{
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000237 RID: 567 RVA: 0x00015630 File Offset: 0x00013830
		public static void PTTon()
		{
			productSelector.write(new byte[] { 224, 3, 28 });
		}

		// Token: 0x06000238 RID: 568 RVA: 0x00015648 File Offset: 0x00013848
		public static void getDVMEGAStatus()
		{
			productSelector.write(new byte[] { 224, 3, 1 });
		}

		// Token: 0x06000239 RID: 569 RVA: 0x00015660 File Offset: 0x00013860
		public static void getDVMEGAVersion()
		{
			byte[] array = new byte[3];
			array[0] = 224;
			array[1] = 3;
			productSelector.write(array);
			Thread.Sleep(20);
			DVMEGASerial.getDVMEGAStatus();
		}

		// Token: 0x0600023A RID: 570 RVA: 0x00015688 File Offset: 0x00013888
		public static void setDVMEGAPower()
		{
			int num = int.Parse(information.DVMEGAPower) & 127;
			byte[] array = new byte[]
			{
				224, 12, 4, 1, 0, 0, 0, 0, 0, 0,
				0, 0
			};
			array[4] = (byte)num;
			productSelector.write(array);
		}

		// Token: 0x0600023B RID: 571 RVA: 0x000156BF File Offset: 0x000138BF
		public static void setmode2Idle()
		{
			productSelector.write(new byte[] { 224, 4, 3, 0 });
			modeTimer.setMode(information.MODUS.IDLE);
		}

		// Token: 0x0600023C RID: 572 RVA: 0x000156DD File Offset: 0x000138DD
		public static void setmode2DMR()
		{
			productSelector.write(new byte[] { 224, 4, 3, 2 });
			modeTimer.setMode(information.MODUS.DMR);
		}

		// Token: 0x0600023D RID: 573 RVA: 0x000156FB File Offset: 0x000138FB
		public static void setmode2DSTAR()
		{
			productSelector.write(new byte[] { 224, 4, 3, 1 });
			modeTimer.setMode(information.MODUS.DSTAR);
		}

		// Token: 0x0600023E RID: 574 RVA: 0x00015719 File Offset: 0x00013919
		public static void setmode2Fusion()
		{
			productSelector.write(new byte[] { 224, 4, 3, 3 });
			modeTimer.setMode(information.MODUS.FUSION);
		}

		// Token: 0x0600023F RID: 575 RVA: 0x00015737 File Offset: 0x00013937
		public static void setmode2NXDN()
		{
			productSelector.write(new byte[] { 224, 4, 3, 16 });
			modeTimer.setMode(information.MODUS.NXDN);
		}

		// Token: 0x06000240 RID: 576 RVA: 0x00015755 File Offset: 0x00013955
		public static void setMMDVMtransmit()
		{
			productSelector.write(new byte[] { 224, 3, 28 });
			modeTimer.setMode(information.MODUS.FUSION);
		}

		// Token: 0x06000241 RID: 577 RVA: 0x00015774 File Offset: 0x00013974
		public static void setDMRFrequency()
		{
			try
			{
				int num = int.Parse(information.myFREQ);
				int num2 = int.Parse(information.myDMRQRG);
				byte[] array = utils.stringto4byte((num + num2).ToString());
				byte[] array2 = utils.stringto4byte(information.myFREQ);
				productSelector.write(new byte[]
				{
					224,
					12,
					4,
					0,
					array[0],
					array[1],
					array[2],
					array[3],
					array2[0],
					array2[1],
					array2[2],
					array2[3]
				});
			}
			catch (FormatException)
			{
			}
		}

		// Token: 0x06000242 RID: 578 RVA: 0x0001581C File Offset: 0x00013A1C
		public static void setDVMEGAmode()
		{
			uint num = uint.Parse(information.DVMEGAPower) & 127U;
			productSelector.write(new byte[]
			{
				224,
				21,
				2,
				130,
				information.DVMEGAmodus,
				10,
				0,
				128,
				128,
				1,
				0,
				128,
				(byte)num,
				(byte)num,
				(byte)num,
				(byte)num,
				128,
				128,
				128,
				4,
				128
			});
			DVMEGASerial.getDVMEGAStatus();
		}

		// Token: 0x04000139 RID: 313
		private static SerialPort serialPort1;

		// Token: 0x0400013E RID: 318
		private static long oldTimerDMR = 0L;

		// Token: 0x0400013F RID: 319
		private static long oldTimerFusion = 0L;

		// Token: 0x04000140 RID: 320
		private static int ser3_in_buf_lenM = 126;

		// Token: 0x04000141 RID: 321
		private static byte[] serial3_bufferM = new byte[DVMEGASerial.ser3_in_buf_lenM + 1];

		// Token: 0x04000142 RID: 322
		private static int serial3_buffer_input_pointerM = 0;

		// Token: 0x04000143 RID: 323
		private static int DMRsequencecounter = 0;

		// Token: 0x04000144 RID: 324
		private static int uniqSessionID = 0;

		// Token: 0x04000145 RID: 325
		private static bool seenStart = false;

		// Token: 0x04000146 RID: 326
		private static bool seenoldVersion = false;

		// Token: 0x04000147 RID: 327
		private static string[] headerArray;

		// Token: 0x04000148 RID: 328
		private static bool running = true;
	}
}
