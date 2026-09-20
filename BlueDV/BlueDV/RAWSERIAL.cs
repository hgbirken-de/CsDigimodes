using System;
using System.IO.Ports;

namespace BlueDV
{
	// Token: 0x02000033 RID: 51
	internal class RAWSERIAL
	{
		// Token: 0x060003CA RID: 970 RVA: 0x00028308 File Offset: 0x00026508
		public static void Start()
		{
			RAWSERIAL.serialPort1 = new SerialPort();
			RAWSERIAL.serialPort1.PortName = "COM15";
			RAWSERIAL.serialPort1.BaudRate = 230400;
			RAWSERIAL.serialPort1.DataBits = 8;
			RAWSERIAL.serialPort1.Parity = Parity.None;
			RAWSERIAL.serialPort1.StopBits = StopBits.One;
			RAWSERIAL.serialPort1.Handshake = Handshake.None;
			RAWSERIAL.serialPort1.WriteTimeout = -1;
			RAWSERIAL.serialPort1.WriteBufferSize = 8000;
			RAWSERIAL.serialPort1.DtrEnable = true;
			RAWSERIAL.serialPort1.RtsEnable = true;
			RAWSERIAL.serialPort1.DataReceived += RAWSERIAL.comPort_DataReceived;
			RAWSERIAL.serialPort1.Open();
		}

		// Token: 0x060003CB RID: 971 RVA: 0x000283BC File Offset: 0x000265BC
		private static void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			SerialPort serialPort = (SerialPort)sender;
			byte[] array = new byte[serialPort.BytesToRead];
			serialPort.Read(array, 0, array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				RAWSERIAL.processVoice(array[i]);
			}
		}

		// Token: 0x060003CC RID: 972 RVA: 0x000283FC File Offset: 0x000265FC
		public static void Stop()
		{
			if (RAWSERIAL.serialPort1 != null)
			{
				RAWSERIAL.serialPort1.Close();
				RAWSERIAL.serialPort1 = null;
			}
		}

		// Token: 0x060003CD RID: 973 RVA: 0x00028418 File Offset: 0x00026618
		public static void write(byte[] data)
		{
			if (RAWSERIAL.serialPort1 != null)
			{
				byte[] array = new byte[324];
				array[0] = 224;
				array[1] = 1;
				array[2] = 68;
				array[3] = 96;
				Buffer.BlockCopy(data, 0, array, 4, 320);
				RAWSERIAL.serialPort1.Write(array, 0, 324);
			}
		}

		// Token: 0x060003CE RID: 974 RVA: 0x0002846C File Offset: 0x0002666C
		private static void processVoice(byte ambeData)
		{
			RAWSERIAL.serial3_bufferP[RAWSERIAL.serial3_buffer_input_pointerP] = ambeData;
			RAWSERIAL.serial3_buffer_input_pointerP++;
			if (RAWSERIAL.serial3_buffer_input_pointerP > RAWSERIAL.ser3_in_buf_lenP)
			{
				RAWSERIAL.serial3_buffer_input_pointerP = 0;
			}
			if (RAWSERIAL.serial3_bufferP[0] != 224)
			{
				RAWSERIAL.serial3_buffer_input_pointerP = 0;
			}
			if (RAWSERIAL.serial3_bufferP[0] == 224 && RAWSERIAL.serial3_buffer_input_pointerP >= 3)
			{
				int num = ((int)RAWSERIAL.serial3_bufferP[1] << 8) | (int)RAWSERIAL.serial3_bufferP[2];
				if (RAWSERIAL.serial3_buffer_input_pointerP >= num && RAWSERIAL.serial3_buffer_input_pointerP >= 2)
				{
					int num2 = ((int)RAWSERIAL.serial3_bufferP[1] << 8) | (int)RAWSERIAL.serial3_bufferP[2];
					byte[] array = new byte[num2];
					Buffer.BlockCopy(RAWSERIAL.serial3_bufferP, 0, array, 0, num2);
					if (array[3] == 96)
					{
						byte[] array2 = new byte[320];
						Buffer.BlockCopy(RAWSERIAL.serial3_bufferP, 4, array2, 0, 320);
						byte[] array3 = new byte[320];
						for (int i = 0; i < 320; i += 2)
						{
							array3[i] = array2[i + 1];
							array3[i + 1] = array2[i];
						}
						soundcard.play(array3);
					}
					RAWSERIAL.serial3_buffer_input_pointerP = 0;
					RAWSERIAL.serial3_bufferP[0] = 0;
				}
			}
		}

		// Token: 0x0400027D RID: 637
		private static SerialPort serialPort1 = null;

		// Token: 0x0400027E RID: 638
		private static int ser3_in_buf_lenP = 324;

		// Token: 0x0400027F RID: 639
		private static byte[] serial3_bufferP = new byte[RAWSERIAL.ser3_in_buf_lenP + 1];

		// Token: 0x04000280 RID: 640
		private static int serial3_buffer_input_pointerP = 0;
	}
}
