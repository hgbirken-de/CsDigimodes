using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BlueDVAMBEServer
{
	// Token: 0x02000006 RID: 6
	internal class serial
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000018 RID: 24 RVA: 0x00003043 File Offset: 0x00001243
		// (set) Token: 0x06000019 RID: 25 RVA: 0x0000304A File Offset: 0x0000124A
		public static string StatusText { get; private set; }

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x0600001A RID: 26 RVA: 0x00003054 File Offset: 0x00001254
		// (remove) Token: 0x0600001B RID: 27 RVA: 0x00003088 File Offset: 0x00001288
		public static event EventHandler StatusTextChanged;

		// Token: 0x0600001C RID: 28 RVA: 0x000030BB File Offset: 0x000012BB
		public static bool isOpen()
		{
			return serial.serialPort1 != null && serial.serialPort1.IsOpen;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000030D0 File Offset: 0x000012D0
		public static bool open()
		{
			if (serial.serialPort1 == null)
			{
				serial.serialPort1 = new SerialPort();
			}
			if (string.IsNullOrEmpty(information.myCOMPort))
			{
				serial.serialPort1.Close();
				return false;
			}
			try
			{
				if (!serial.serialPort1.IsOpen)
				{
					serial.serialPort1.PortName = information.myCOMPort;
					serial.serialPort1.BaudRate = information.mySerialBaudRate;
					serial.serialPort1.DataBits = 8;
					serial.serialPort1.Parity = Parity.None;
					serial.serialPort1.StopBits = StopBits.One;
					serial.serialPort1.Handshake = Handshake.None;
					serial.serialPort1.RtsEnable = false;
					serial.serialPort1.DtrEnable = true;
					serial.serialPort1.ReadTimeout = 500;
					serial.serialPort1.WriteTimeout = -1;
					serial.serialPort1.WriteBufferSize = 512;
					serial.serialPort1.ReadBufferSize = 512;
					serial.serialPort1.Open();
					serial.running = true;
					new Thread(new ThreadStart(serial.dataReceived)).Start();
				}
				return true;
			}
			catch (Exception)
			{
			}
			return true;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000031F0 File Offset: 0x000013F0
		private static void dataReceived()
		{
			while (serial.running)
			{
				if (serial.serialPort1.IsOpen)
				{
					try
					{
						int bytesToRead = serial.serialPort1.BytesToRead;
						if (bytesToRead > 0)
						{
							byte[] array = new byte[bytesToRead];
							try
							{
								serial.serialPort1.Read(array, 0, bytesToRead);
								byte[] array2 = array;
								for (int i = 0; i < array2.Length; i++)
								{
									serial.decodeAMBE(array2[i]);
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
				Thread.Sleep(1);
			}
		}

		// Token: 0x0600001F RID: 31 RVA: 0x0000327C File Offset: 0x0000147C
		public static void write(byte[] data)
		{
			if (serial.serialPort1 == null)
			{
				return;
			}
			if (!serial.serialPort1.IsOpen)
			{
				return;
			}
			try
			{
				Logger.Log("To AMBE: " + BitConverter.ToString(data));
				serial.serialPort1.Write(data, 0, data.Length);
			}
			catch (TimeoutException)
			{
				try
				{
					MessageBox.Show("COM port timeout");
					serial.close();
				}
				catch (Exception)
				{
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00003308 File Offset: 0x00001508
		public static void close()
		{
			if (serial.serialPort1 != null && serial.serialPort1.IsOpen)
			{
				serial.running = false;
				serial.serialPort1.Close();
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00003330 File Offset: 0x00001530
		private static void ChangeAMBEStatusText(string text)
		{
			serial.StatusText = text;
			EventHandler statusTextChanged = serial.StatusTextChanged;
			if (statusTextChanged != null)
			{
				statusTextChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00003358 File Offset: 0x00001558
		public static void getAMBEproductID()
		{
			serial.write(new byte[] { 97, 0, 1, 0, 48 });
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003370 File Offset: 0x00001570
		public static void setParityOff()
		{
			serial.write(new byte[] { 97, 0, 4, 0, 63, 0, 47, 20 });
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00003388 File Offset: 0x00001588
		public static void setAMBEpacketmode()
		{
			serial.write(new byte[]
			{
				97, 0, 7, 0, 52, 5, 0, 0, 7, 0,
				0
			});
		}

		// Token: 0x06000025 RID: 37 RVA: 0x000033A4 File Offset: 0x000015A4
		public static void decodeAMBE(byte ambeData)
		{
			serial.serial3_bufferP[serial.serial3_buffer_input_pointerP] = ambeData;
			serial.serial3_buffer_input_pointerP++;
			if (serial.serial3_buffer_input_pointerP > serial.ser3_in_buf_lenP)
			{
				serial.serial3_buffer_input_pointerP = 0;
			}
			if ((serial.serial3_bufferP[0] != 97) & (serial.serial3_bufferP[0] != 0 || serial.serial3_bufferP[0] != 1))
			{
				serial.serial3_buffer_input_pointerP = 0;
			}
			if (serial.serial3_bufferP[0] == 97 && serial.serial3_buffer_input_pointerP >= 3)
			{
				int num = ((int)(byte.MaxValue & serial.serial3_bufferP[1]) << 8) | (int)(byte.MaxValue & serial.serial3_bufferP[2]);
				if (serial.serial3_buffer_input_pointerP >= num + 4 && serial.serial3_buffer_input_pointerP >= 2)
				{
					int num2 = ((int)(byte.MaxValue & serial.serial3_bufferP[1]) << 8) | (int)(byte.MaxValue & serial.serial3_bufferP[2]);
					byte[] array = new byte[num2 + 4];
					Buffer.BlockCopy(serial.serial3_bufferP, 0, array, 0, num2 + 4);
					Logger.Log("From AMBE: " + BitConverter.ToString(array));
					switch (array[3])
					{
					case 0:
					{
						byte b = serial.serial3_bufferP[4];
						if (b != 48)
						{
							if (b != 54)
							{
							}
						}
						else
						{
							byte[] array2 = new byte[num2 - 2];
							Buffer.BlockCopy(serial.serial3_bufferP, 5, array2, 0, num2 - 2);
							if (!information.AMBE6000R_FOUND)
							{
								serial.setAMBEpacketmode();
								Thread.Sleep(150);
								UDPServerTest.Start();
								information.AMBE6000R_FOUND = true;
								serial.ChangeAMBEStatusText(Encoding.UTF8.GetString(array2));
							}
						}
						UDPServerTest.SendTo(array);
						break;
					}
					case 1:
						if (num2 != 8 || serial.serial3_bufferP[4] != 1)
						{
							UDPServerTest.SendTo(array);
						}
						break;
					case 2:
						UDPServerTest.SendTo(array);
						break;
					}
					serial.serial3_buffer_input_pointerP = 0;
					serial.serial3_bufferP[0] = 0;
				}
			}
		}

		// Token: 0x0400001A RID: 26
		private static SerialPort serialPort1;

		// Token: 0x0400001B RID: 27
		private static bool running = false;

		// Token: 0x0400001E RID: 30
		private static int ser3_in_buf_lenP = 330;

		// Token: 0x0400001F RID: 31
		private static byte[] serial3_bufferP = new byte[serial.ser3_in_buf_lenP + 1];

		// Token: 0x04000020 RID: 32
		private static int serial3_buffer_input_pointerP = 0;
	}
}
