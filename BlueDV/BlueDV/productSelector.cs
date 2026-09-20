using System;

namespace BlueDV
{
	// Token: 0x02000032 RID: 50
	internal class productSelector
	{
		// Token: 0x060003C6 RID: 966 RVA: 0x0002822C File Offset: 0x0002642C
		public static void fromInternetDMR(byte[] data)
		{
			byte[] array = new byte[33];
			byte[] array2 = new byte[37];
			Buffer.BlockCopy(data, 4, array, 0, 33);
			byte[] array3 = DMRsrcdstConverter.fromInternetConverter(array, data[3]);
			if (array3 == null)
			{
				return;
			}
			array2[0] = 224;
			array2[1] = 37;
			array2[2] = 26;
			array2[3] = data[3];
			Buffer.BlockCopy(array3, 0, array2, 4, 33);
			productSelector.write(array2);
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x0002828C File Offset: 0x0002648C
		public static void write(byte[] data)
		{
			switch (information.m_device)
			{
			case information.DEVICE.DVMEGARADIO:
				DVMEGASerial.write2(data);
				return;
			case information.DEVICE.DV3000R:
				if (!information.i)
				{
					DVMEGAAMBE.write(data);
				}
				break;
			case information.DEVICE.DV4MINI:
				break;
			case information.DEVICE.DVAP:
				DVAP.incommingMMDVM(data);
				return;
			default:
				return;
			}
		}

		// Token: 0x060003C8 RID: 968 RVA: 0x000282D4 File Offset: 0x000264D4
		public static void deviceWrite(byte[] data)
		{
			information.DEVICECONNECTION deviceconnection = information.m_deviceconnection;
			if (deviceconnection == information.DEVICECONNECTION.SERIAL)
			{
				DVMEGASerial.write2(data);
				return;
			}
			if (deviceconnection != information.DEVICECONNECTION.AMBESERVER)
			{
				return;
			}
			if (!information.i)
			{
				AMBESERVER.write2(data);
			}
		}
	}
}
