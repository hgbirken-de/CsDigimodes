using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Timers;

namespace BlueDV
{
	// Token: 0x0200001A RID: 26
	internal class DVAP
	{
		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600013E RID: 318 RVA: 0x0000D8A5 File Offset: 0x0000BAA5
		// (set) Token: 0x0600013F RID: 319 RVA: 0x0000D8AC File Offset: 0x0000BAAC
		public static string StatusText { get; private set; }

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x06000140 RID: 320 RVA: 0x0000D8B4 File Offset: 0x0000BAB4
		// (remove) Token: 0x06000141 RID: 321 RVA: 0x0000D8E8 File Offset: 0x0000BAE8
		public static event EventHandler StatusTextChanged;

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000142 RID: 322 RVA: 0x0000D91B File Offset: 0x0000BB1B
		// (set) Token: 0x06000143 RID: 323 RVA: 0x0000D922 File Offset: 0x0000BB22
		public static bool StatusBoolAMBE_VOX { get; private set; }

		// Token: 0x14000011 RID: 17
		// (add) Token: 0x06000144 RID: 324 RVA: 0x0000D92C File Offset: 0x0000BB2C
		// (remove) Token: 0x06000145 RID: 325 RVA: 0x0000D960 File Offset: 0x0000BB60
		public static event EventHandler StatusBoolChangedAMBE_VOX;

		// Token: 0x06000146 RID: 326 RVA: 0x0000D993 File Offset: 0x0000BB93
		public static bool isOpen()
		{
			return DVAP.serialPort1 != null && DVAP.serialPort1.IsOpen;
		}

		// Token: 0x06000147 RID: 327 RVA: 0x0000D9A8 File Offset: 0x0000BBA8
		public static bool open()
		{
			if (DVAP.serialPort1 == null)
			{
				DVAP.serialPort1 = new SerialPort();
			}
			if (string.IsNullOrEmpty("COM19"))
			{
				DVAP.serialPort1.Close();
				return false;
			}
			try
			{
				DVAP.running = false;
				if (!DVAP.serialPort1.IsOpen)
				{
					DVAP.serialPort1.PortName = information.myCOMPort;
					DVAP.serialPort1.BaudRate = 230400;
					DVAP.serialPort1.DataBits = 8;
					DVAP.serialPort1.Parity = Parity.None;
					DVAP.serialPort1.StopBits = StopBits.One;
					DVAP.serialPort1.Handshake = Handshake.None;
					DVAP.serialPort1.RtsEnable = true;
					DVAP.serialPort1.DtrEnable = true;
					DVAP.serialPort1.ReadTimeout = 5000;
					DVAP.serialPort1.WriteTimeout = -1;
					DVAP.serialPort1.WriteBufferSize = 512;
					DVAP.serialPort1.ReadBufferSize = 512;
					DVAP.serialPort1.Open();
					DVAP.running = true;
					new Thread(new ThreadStart(DVAP.dataReceived)).Start();
					DVAP.aTimer = new global::System.Timers.Timer();
					DVAP.aTimer.Elapsed += DVAP.OnTimedEvent;
					DVAP.aTimer.Interval = 2000.0;
					DVAP.aTimer.Enabled = true;
					DVAP.setDVAPFrequency();
					DVAP.setQuelch();
					DVAP.setRunMode();
				}
				return true;
			}
			catch (Exception)
			{
			}
			return true;
		}

		// Token: 0x06000148 RID: 328 RVA: 0x0000DB28 File Offset: 0x0000BD28
		private static void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
			DVAP.ping();
			DVAP.setRunMode();
		}

		// Token: 0x06000149 RID: 329 RVA: 0x0000DB34 File Offset: 0x0000BD34
		private static void dataReceived()
		{
			while (DVAP.running)
			{
				if (DVAP.serialPort1.IsOpen)
				{
					try
					{
						int bytesToRead = DVAP.serialPort1.BytesToRead;
						if (bytesToRead > 0)
						{
							byte[] array = new byte[bytesToRead];
							try
							{
								DVAP.serialPort1.Read(array, 0, bytesToRead);
								foreach (byte b in array)
								{
									DVAP.decodeHeader(b);
									DVAP.decodeAMBE(b);
								}
							}
							catch (TimeoutException)
							{
							}
						}
						goto IL_0060;
					}
					catch (Exception)
					{
						goto IL_0060;
					}
					goto IL_005A;
				}
				goto IL_005A;
				IL_0060:
				Thread.Sleep(1);
				continue;
				IL_005A:
				DVAP.running = false;
				goto IL_0060;
			}
		}

		// Token: 0x0600014A RID: 330 RVA: 0x0000DBCC File Offset: 0x0000BDCC
		public static void write(byte[] data)
		{
			DVAP.serialPort1.Write(data, 0, data.Length);
		}

		// Token: 0x0600014B RID: 331 RVA: 0x0000DBE0 File Offset: 0x0000BDE0
		public static void unknownSet()
		{
			DVAP.write(new byte[] { 4, 32, 48, 2 });
			DVAP.write(new byte[] { 4, 32, 32, 0, 4, 32, 32, 0 });
			DVAP.write(new byte[] { 4, 32, 32, 1 });
			DVAP.write(new byte[] { 4, 32, 32, 1, 4, 32, 0, 4 });
			DVAP.write(new byte[] { 4, 32, 0, 4 });
		}

		// Token: 0x0600014C RID: 332 RVA: 0x0000DC5C File Offset: 0x0000BE5C
		public static void close()
		{
			if (DVAP.serialPort1 != null && DVAP.serialPort1.IsOpen)
			{
				DVAP.running = false;
				DVAP.serialPort1.Close();
			}
			if (DVAP.aTimer != null)
			{
				DVAP.aTimer.Enabled = false;
				DVAP.aTimer.Stop();
				DVAP.aTimer = null;
			}
		}

		// Token: 0x0600014D RID: 333 RVA: 0x0000DCB0 File Offset: 0x0000BEB0
		private static void ChangeAMBEStatusText(string text)
		{
			DVAP.StatusText = text;
			EventHandler statusTextChanged = DVAP.StatusTextChanged;
			if (statusTextChanged != null)
			{
				statusTextChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x0600014E RID: 334 RVA: 0x0000DCD8 File Offset: 0x0000BED8
		public static void makePCM_Analog(byte[] pcm)
		{
			byte[] array = new byte[322];
			array[0] = 66;
			array[1] = 129;
			Buffer.BlockCopy(pcm, 0, array, 2, 320);
			DVAP.write(array);
		}

		// Token: 0x0600014F RID: 335 RVA: 0x0000DD11 File Offset: 0x0000BF11
		public static void getDVAPproductID()
		{
			DVAP.write(new byte[] { 97, 0, 1, 0, 48 });
		}

		// Token: 0x06000150 RID: 336 RVA: 0x0000DD2C File Offset: 0x0000BF2C
		public static void setDVAPFrequency()
		{
			byte[] array = utils.stringto4byte(information.myFREQ);
			DVAP.write(new byte[]
			{
				8,
				0,
				32,
				2,
				array[0],
				array[1],
				array[2],
				array[3]
			});
		}

		// Token: 0x06000151 RID: 337 RVA: 0x0000DD78 File Offset: 0x0000BF78
		public static void pttOnManual(bool jn)
		{
			if (jn)
			{
				DVAP.write(new byte[] { 5, 32, 24, 1, 1 });
				return;
			}
			DVAP.write(new byte[] { 5, 32, 24, 1, 0 });
		}

		// Token: 0x06000152 RID: 338 RVA: 0x0000DDAA File Offset: 0x0000BFAA
		public static void setRunMode()
		{
			DVAP.write(new byte[] { 5, 0, 24, 0, 1 });
		}

		// Token: 0x06000153 RID: 339 RVA: 0x0000DDC4 File Offset: 0x0000BFC4
		public static void dstardata(byte[] data)
		{
			if (data.Length > 7)
			{
				byte[] array = new byte[18];
				array[0] = 18;
				array[1] = 192;
				array[2] = 0;
				array[3] = 0;
				array[4] = 0;
				array[5] = 0;
				Buffer.BlockCopy(data, 3, array, 6, 12);
				DVAP.write(array);
			}
		}

		// Token: 0x06000154 RID: 340 RVA: 0x0000DE10 File Offset: 0x0000C010
		public static void incommingMMDVM(byte[] data)
		{
			for (int i = 0; i < data.Length; i++)
			{
				DVAP.procesMMDVM(data[i]);
			}
		}

		// Token: 0x06000155 RID: 341 RVA: 0x0000DE38 File Offset: 0x0000C038
		public static void dstarHeaderMMDVM(byte[] data)
		{
			byte[] array = new byte[47];
			array[0] = 47;
			array[1] = 160;
			array[2] = 0;
			array[3] = 2;
			array[4] = 128;
			array[5] = 0;
			array[6] = 0;
			array[7] = 0;
			array[8] = 0;
			Buffer.BlockCopy(data, 0, array, 6, 41);
			DVAP.write(array);
		}

		// Token: 0x06000156 RID: 342 RVA: 0x0000DE8C File Offset: 0x0000C08C
		public static void dstardataMMDVM(byte[] data)
		{
			DVAP.pttOnManual(true);
			if (DVAP.frameCounter == 21)
			{
				DVAP.frameCounter = 0;
			}
			if (data.Length > 7)
			{
				byte[] array = new byte[18];
				array[0] = 18;
				array[1] = 192;
				array[2] = 0;
				array[3] = 2;
				array[4] = DVAP.frameCounter;
				array[5] = DVAP.frameSequence;
				Buffer.BlockCopy(data, 0, array, 6, 12);
				DVAP.write(array);
				DVAP.frameCounter += 1;
				DVAP.frameSequence += 1;
			}
		}

		// Token: 0x06000157 RID: 343 RVA: 0x0000DF0C File Offset: 0x0000C10C
		public static void dstardata2(byte[] data)
		{
			byte[] array = new byte[18];
			array[0] = 18;
			array[1] = 192;
			array[2] = 0;
			array[3] = 0;
			array[4] = 0;
			array[5] = 0;
			Buffer.BlockCopy(array, 6, data, 0, 12);
			DVAP.write(array);
		}

		// Token: 0x06000158 RID: 344 RVA: 0x00002F42 File Offset: 0x00001142
		public static void decodeFromDVAP(byte[] data)
		{
		}

		// Token: 0x06000159 RID: 345 RVA: 0x0000DF42 File Offset: 0x0000C142
		public static void ping()
		{
			byte[] array = new byte[3];
			array[0] = 3;
			array[1] = 96;
			DVAP.write(array);
		}

		// Token: 0x0600015A RID: 346 RVA: 0x0000DF58 File Offset: 0x0000C158
		public static void setQuelch()
		{
			DVAP.write(new byte[] { 5, 0, 128, 0, 156 });
		}

		// Token: 0x0600015B RID: 347 RVA: 0x0000DF70 File Offset: 0x0000C170
		public static void setDSTARmode()
		{
			DVAP.write(new byte[] { 5, 0, 40, 0, 1 });
		}

		// Token: 0x0600015C RID: 348 RVA: 0x0000DF88 File Offset: 0x0000C188
		public static void PTT(byte ambeData)
		{
			DVAP.serial3_bufferM[DVAP.serial3_buffer_input_pointerM] = ambeData;
			DVAP.serial3_buffer_input_pointerM++;
			if (DVAP.serial3_buffer_input_pointerM > DVAP.ser3_in_buf_lenM)
			{
				DVAP.serial3_buffer_input_pointerM = 0;
			}
			if (DVAP.serial3_bufferM[0] != 7)
			{
				DVAP.serial3_buffer_input_pointerM = 0;
			}
			if (DVAP.serial3_bufferM[0] == 7 && DVAP.serial3_bufferM[1] == 32 && DVAP.serial3_bufferM[2] == 144 && DVAP.serial3_bufferM[3] == 0 && DVAP.serial3_buffer_input_pointerP >= 4)
			{
				if (DVAP.serial3_bufferM[5] == 0)
				{
					DVAP.ChangeStatusBoolAMBE_VOX(false);
					DVAP.pttOn = false;
				}
				if (DVAP.serial3_bufferM[5] == 1)
				{
					DVAP.ChangeStatusBoolAMBE_VOX(true);
					DVAP.pttOn = true;
				}
			}
		}

		// Token: 0x0600015D RID: 349 RVA: 0x0000E030 File Offset: 0x0000C230
		public static void decodeAMBE(byte ambeData)
		{
			DVAP.serial3_bufferP[DVAP.serial3_buffer_input_pointerP] = ambeData;
			DVAP.serial3_buffer_input_pointerP++;
			if (DVAP.serial3_buffer_input_pointerP > DVAP.ser3_in_buf_lenP)
			{
				DVAP.serial3_buffer_input_pointerP = 0;
			}
			if (DVAP.serial3_bufferP[0] != 18)
			{
				DVAP.serial3_buffer_input_pointerP = 0;
			}
			if (DVAP.serial3_bufferP[0] == 18 && DVAP.serial3_bufferP[1] == 192 && DVAP.serial3_buffer_input_pointerP >= 3)
			{
				if (DVAP.serial3_buffer_input_pointerP == 18)
				{
					byte[] array = new byte[18];
					Buffer.BlockCopy(DVAP.serial3_bufferP, 0, array, 0, 18);
					byte[] array2 = new byte[15];
					Buffer.BlockCopy(array, 6, array2, 3, 12);
					DSTARhandler.processDSTARVoice(array2);
					TimerRXTX.RX();
					DVAP.serial3_buffer_input_pointerP = 0;
					DVAP.serial3_bufferP[0] = 0;
				}
				if (DVAP.serial3_buffer_input_pointerP >= 46)
				{
					byte[] array3 = new byte[47];
					Buffer.BlockCopy(DVAP.serial3_bufferP, 0, array3, 0, 47);
				}
			}
		}

		// Token: 0x0600015E RID: 350 RVA: 0x0000E108 File Offset: 0x0000C308
		public static void decodeHeader(byte headerData)
		{
			DVAP.serial3_bufferH[DVAP.serial3_buffer_input_pointerH] = headerData;
			DVAP.serial3_buffer_input_pointerH++;
			if (DVAP.serial3_buffer_input_pointerH > DVAP.ser3_in_buf_lenH)
			{
				DVAP.serial3_buffer_input_pointerH = 0;
			}
			if (DVAP.serial3_bufferH[0] != 47)
			{
				DVAP.serial3_buffer_input_pointerH = 0;
			}
			if (DVAP.serial3_bufferH[0] == 47 && DVAP.serial3_bufferH[1] == 96 && DVAP.serial3_buffer_input_pointerH >= 3 && DVAP.serial3_buffer_input_pointerH >= 47)
			{
				byte[] array = new byte[47];
				Buffer.BlockCopy(DVAP.serial3_bufferH, 0, array, 0, 47);
				TimerRXTX.RX();
				DVAP.serial3_buffer_input_pointerH = 0;
				DVAP.serial3_bufferH[0] = 0;
			}
		}

		// Token: 0x0600015F RID: 351 RVA: 0x0000E1A4 File Offset: 0x0000C3A4
		private static void ChangeStatusBoolAMBE_VOX(bool vox)
		{
			DVAP.StatusBoolAMBE_VOX = vox;
			EventHandler statusBoolChangedAMBE_VOX = DVAP.StatusBoolChangedAMBE_VOX;
			if (statusBoolChangedAMBE_VOX != null)
			{
				statusBoolChangedAMBE_VOX(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000160 RID: 352 RVA: 0x0000E1CC File Offset: 0x0000C3CC
		private static void procesMMDVM(byte mmdvmData)
		{
			DVAP.serial3_bufferD[DVAP.serial3_buffer_input_pointerD] = mmdvmData;
			DVAP.serial3_buffer_input_pointerD++;
			if (DVAP.serial3_buffer_input_pointerD > DVAP.ser3_in_buf_lenD)
			{
				DVAP.serial3_buffer_input_pointerD = 0;
			}
			if (DVAP.serial3_bufferD[0] != 224)
			{
				DVAP.serial3_buffer_input_pointerD = 0;
			}
			if (DVAP.serial3_bufferD[0] == 224 && DVAP.serial3_buffer_input_pointerD >= 3 && DVAP.serial3_buffer_input_pointerD >= (int)(DVAP.serial3_bufferD[1] & 255) && DVAP.serial3_buffer_input_pointerD >= 3)
			{
				byte[] array = new byte[(int)(DVAP.serial3_bufferD[1] & byte.MaxValue)];
				Buffer.BlockCopy(DVAP.serial3_bufferD, 0, array, 0, (int)(DVAP.serial3_bufferD[1] & byte.MaxValue));
				int num = (int)(DVAP.serial3_bufferD[1] & byte.MaxValue);
				byte b = array[2];
				if (b <= 26)
				{
					switch (b)
					{
					case 0:
						DVAP.setDSTARmode();
						if (information.myDVMEGAVersion != null)
						{
							byte[] bytes = Encoding.ASCII.GetBytes(information.myDVMEGAVersion);
							byte[] array2 = new byte[bytes.Length + 4];
							array2[0] = 224;
							array2[1] = (byte)array2.Length;
							array2[2] = 0;
							array2[3] = 1;
							Buffer.BlockCopy(bytes, 0, array2, 4, array2.Length - 4);
							if (array2.Length > 4)
							{
							}
						}
						break;
					case 1:
					case 2:
						break;
					case 3:
						switch (array[3])
						{
						case 1:
							DVAP.setDSTARmode();
							break;
						}
						break;
					default:
						switch (b)
						{
						case 16:
						{
							byte[] array3 = new byte[45];
							Buffer.BlockCopy(array, 3, array3, 0, num - 3);
							DVAP.dstarHeaderMMDVM(array3);
							break;
						}
						case 17:
						{
							byte[] array4 = new byte[12];
							Buffer.BlockCopy(array, 3, array4, 0, 12);
							DVAP.dstardataMMDVM(array4);
							break;
						}
						case 18:
						case 19:
							break;
						default:
							if (b != 26)
							{
							}
							break;
						}
						break;
					}
				}
				else if (b != 32 && b != 112 && b != 127)
				{
				}
				DVAP.serial3_buffer_input_pointerD = 0;
				DVAP.serial3_bufferD[0] = 0;
			}
		}

		// Token: 0x040000AA RID: 170
		private static SerialPort serialPort1;

		// Token: 0x040000AB RID: 171
		private static bool running = false;

		// Token: 0x040000B0 RID: 176
		private static int ser3_in_buf_lenP = 326;

		// Token: 0x040000B1 RID: 177
		private static byte[] serial3_bufferP = new byte[DVAP.ser3_in_buf_lenP + 1];

		// Token: 0x040000B2 RID: 178
		private static int serial3_buffer_input_pointerP = 0;

		// Token: 0x040000B3 RID: 179
		private static int ser3_in_buf_lenM = 10;

		// Token: 0x040000B4 RID: 180
		private static byte[] serial3_bufferM = new byte[DVAP.ser3_in_buf_lenM + 1];

		// Token: 0x040000B5 RID: 181
		private static int serial3_buffer_input_pointerM = 0;

		// Token: 0x040000B6 RID: 182
		private static int ser3_in_buf_lenD = 126;

		// Token: 0x040000B7 RID: 183
		private static byte[] serial3_bufferD = new byte[DVAP.ser3_in_buf_lenD + 1];

		// Token: 0x040000B8 RID: 184
		private static int serial3_buffer_input_pointerD = 0;

		// Token: 0x040000B9 RID: 185
		private static int ser3_in_buf_lenH = 47;

		// Token: 0x040000BA RID: 186
		private static byte[] serial3_bufferH = new byte[DVAP.ser3_in_buf_lenD + 1];

		// Token: 0x040000BB RID: 187
		private static int serial3_buffer_input_pointerH = 0;

		// Token: 0x040000BC RID: 188
		private static global::System.Timers.Timer aTimer = null;

		// Token: 0x040000BD RID: 189
		private static byte frameCounter;

		// Token: 0x040000BE RID: 190
		private static byte frameSequence = 0;

		// Token: 0x040000BF RID: 191
		private static bool pttOn = false;
	}
}
