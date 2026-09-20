using System;
using System.Collections;
using System.IO;
using System.Media;

namespace BlueDV
{
	// Token: 0x0200004E RID: 78
	internal class utils
	{
		// Token: 0x060005B8 RID: 1464 RVA: 0x00034E20 File Offset: 0x00033020
		public static int READ_BIT1(byte[] p, int i)
		{
			return (int)(p[i >> 3] & utils.BIT_MASK_TABLE[i & 7]);
		}

		// Token: 0x060005B9 RID: 1465 RVA: 0x00034E34 File Offset: 0x00033034
		public static int ToNumeral(bool[] binary, int length)
		{
			int num = 0;
			for (int i = 0; i < length; i++)
			{
				if (binary[i])
				{
					num |= 1 << length - 1 - i;
				}
			}
			return num;
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x00034E64 File Offset: 0x00033064
		public static BitArray ToBinary(int numeral)
		{
			BitArray bitArray = new BitArray(new int[] { numeral });
			bool[] array = new bool[bitArray.Count];
			bitArray.CopyTo(array, 0);
			return bitArray;
		}

		// Token: 0x060005BB RID: 1467 RVA: 0x00034E94 File Offset: 0x00033094
		public static bool[] byteToBitsBE(byte byte1)
		{
			return new bool[]
			{
				(byte1 & 128) == 128,
				(byte1 & 64) == 64,
				(byte1 & 32) == 32,
				(byte1 & 16) == 16,
				(byte1 & 8) == 8,
				(byte1 & 4) == 4,
				(byte1 & 2) == 2,
				(byte1 & 1) == 1
			};
		}

		// Token: 0x060005BC RID: 1468 RVA: 0x00034F00 File Offset: 0x00033100
		public static bool[] byteToBitsLE(byte byte1)
		{
			return new bool[]
			{
				(byte1 & 1) == 1,
				(byte1 & 2) == 2,
				(byte1 & 4) == 4,
				(byte1 & 8) == 8,
				(byte1 & 16) == 16,
				(byte1 & 32) == 32,
				(byte1 & 64) == 64,
				(byte1 & 128) == 128
			};
		}

		// Token: 0x060005BD RID: 1469 RVA: 0x00034F6C File Offset: 0x0003316C
		public static byte bitsToByteBE(bool[] bits)
		{
			return (byte)((bits[0] ? 128 : 0) | (bits[1] ? 64 : 0)) | (bits[2] ? 32 : 0) | (bits[3] ? 16 : 0) | (bits[4] ? 8 : 0) | (bits[5] ? 4 : 0) | (bits[6] ? 2 : 0) | ((bits[7] > false) ? 1 : 0);
		}

		// Token: 0x060005BE RID: 1470 RVA: 0x00034FD4 File Offset: 0x000331D4
		public static char bitsToByteBE2(bool[] bits)
		{
			return (char)((ushort)((bits[0] ? 128 : 0) | (bits[1] ? 64 : 0)) | (bits[2] ? 32 : 0) | (bits[3] ? 16 : 0) | (bits[4] ? 8 : 0) | (bits[5] ? 4 : 0) | (bits[6] ? 2 : 0) | ((bits[7] > false) ? 1 : 0));
		}

		// Token: 0x060005BF RID: 1471 RVA: 0x0003503C File Offset: 0x0003323C
		public static byte bitsToByteLE(bool[] bits)
		{
			return ((bits[0] > false) ? 1 : 0) | (bits[1] ? 2 : 0) | (bits[2] ? 4 : 0) | (bits[3] ? 8 : 0) | (bits[4] ? 16 : 0) | (bits[5] ? 32 : 0) | (bits[6] ? 64 : 0) | (bits[7] ? 128 : 0);
		}

		// Token: 0x060005C0 RID: 1472 RVA: 0x000350A4 File Offset: 0x000332A4
		public static bool[] bytesToBits(byte[] data)
		{
			int num = 0;
			bool[] array = new bool[data.Length * 8];
			for (int i = 0; i < data.Length; i++)
			{
				Buffer.BlockCopy(utils.byteToBitsBE(data[i]), 0, array, num, 8);
				num += 8;
			}
			return array;
		}

		// Token: 0x060005C1 RID: 1473 RVA: 0x000350E4 File Offset: 0x000332E4
		public static bool[] bytesToBitsLE(byte[] data)
		{
			int num = 0;
			bool[] array = new bool[data.Length * 8];
			for (int i = 0; i < data.Length; i++)
			{
				Buffer.BlockCopy(utils.byteToBitsLE(data[i]), 0, array, num, 8);
				num += 8;
			}
			return array;
		}

		// Token: 0x060005C2 RID: 1474 RVA: 0x00035124 File Offset: 0x00033324
		public static byte[] bitToByteConverter(bool[] bits, int len)
		{
			int num = len / 8;
			byte[] array = new byte[num];
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				bool[] array2 = new bool[8];
				for (int j = 0; j < 8; j++)
				{
					array2[j] = bits[num2];
					num2++;
				}
				array[i] = utils.bitsToByteBE(array2);
			}
			return array;
		}

		// Token: 0x060005C3 RID: 1475 RVA: 0x0003517C File Offset: 0x0003337C
		public static byte[] ToByteArray(bool[] bits)
		{
			int num = bits.Length / 8;
			if (bits.Length % 8 != 0)
			{
				num++;
			}
			byte[] array = new byte[num];
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < bits.Length; i++)
			{
				if (bits[i])
				{
					byte[] array2 = array;
					int num4 = num2;
					array2[num4] |= (byte)(1 << 7 - num3);
				}
				num3++;
				if (num3 == 8)
				{
					num3 = 0;
					num2++;
				}
			}
			return array;
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x000351E4 File Offset: 0x000333E4
		public static bool[] byteToBitsLEtotal(byte[] data)
		{
			int num = 0;
			bool[] array = new bool[data.Length * 8];
			for (int i = 0; i < data.Length; i++)
			{
				Buffer.BlockCopy(utils.byteToBitsLE(data[i]), 0, array, num, 8);
				num += 8;
			}
			return array;
		}

		// Token: 0x060005C5 RID: 1477 RVA: 0x00035224 File Offset: 0x00033424
		public static byte[] bitsToBytes(bool[] data)
		{
			int num = 0;
			int num2 = data.Length / 8;
			byte[] array = new byte[num2];
			for (int i = 0; i < num2; i++)
			{
				bool[] array2 = new bool[8];
				Buffer.BlockCopy(data, num, array2, 0, 8);
				array[i] = utils.bitsToByteBE(array2);
				num += 8;
			}
			return array;
		}

		// Token: 0x060005C6 RID: 1478 RVA: 0x00035270 File Offset: 0x00033470
		public static byte[] bitsToBytesLE(bool[] data)
		{
			int num = data.Length - 8;
			int num2 = data.Length / 8;
			byte[] array = new byte[num2];
			for (int i = num2; i > 0; i--)
			{
				bool[] array2 = new bool[8];
				Buffer.BlockCopy(data, num, array2, 0, 8);
				array[i] = utils.bitsToByteLE(array2);
				num -= 8;
			}
			return array;
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x000352B9 File Offset: 0x000334B9
		public static byte[] stringto4byte(string call)
		{
			return BitConverter.GetBytes((long)int.Parse(call));
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x000352C8 File Offset: 0x000334C8
		public static string dmrid2hexstring4(string dmrid)
		{
			long num = (long)int.Parse(information.myDMRID);
			BitConverter.GetBytes(num);
			return num.ToString("X8");
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x000352F4 File Offset: 0x000334F4
		public static long byte2longMS(byte[] a)
		{
			long num = 0L;
			for (int i = 0; i < a.Length; i++)
			{
				num = (num << 8) + (long)(a[i] & byte.MaxValue);
			}
			return num;
		}

		// Token: 0x060005CA RID: 1482 RVA: 0x00035324 File Offset: 0x00033524
		public static void BeepBeep(int Amplitude, int Frequency, int Duration)
		{
			double num = (double)Amplitude * Math.Pow(2.0, 15.0) / 1000.0 - 1.0;
			double num2 = 6.283185307179586 * (double)Frequency / 44100.0;
			int num3 = 441 * Duration / 10;
			int num4 = num3 * 4;
			int[] array = new int[]
			{
				1179011410, 0, 1163280727, 544501094, 16, 131073, 44100, 176400, 1048580, 1635017060,
				0
			};
			array[1] = 36 + num4;
			array[10] = num4;
			int[] array2 = array;
			using (MemoryStream memoryStream = new MemoryStream(44 + num4))
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
				{
					for (int i = 0; i < array2.Length; i++)
					{
						binaryWriter.Write(array2[i]);
					}
					for (int j = 0; j < num3; j++)
					{
						short num5 = Convert.ToInt16(num * Math.Sin(num2 * (double)j));
						binaryWriter.Write(num5);
						binaryWriter.Write(num5);
					}
					binaryWriter.Flush();
					memoryStream.Seek(0L, SeekOrigin.Begin);
					using (SoundPlayer soundPlayer = new SoundPlayer(memoryStream))
					{
						soundPlayer.PlaySync();
					}
				}
			}
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x00035480 File Offset: 0x00033680
		public static int crc16DSTAR(byte[] data, int leng)
		{
			int num = utils.PRESET_VALUE;
			for (int i = 0; i < leng; i++)
			{
				num ^= (int)(data[i] & byte.MaxValue);
				for (int j = 0; j < 8; j++)
				{
					if ((num & 1) != 0)
					{
						num = (num >> 1) ^ utils.POLYNOMIAL;
					}
					else
					{
						num >>= 1;
					}
				}
			}
			num = ~num;
			return num & 65535;
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x000354D8 File Offset: 0x000336D8
		public static bool checkCCITT162(byte[] buf, uint length)
		{
			ushort num = 0;
			int num2 = 0;
			while ((long)num2 < (long)((ulong)(length - 2U)))
			{
				num = (ushort)((int)((byte)(num >> 8)) | ((int)num << 8));
				num ^= (ushort)buf[num2];
				num ^= (ushort)((byte)((num & 255) >> 4));
				num ^= (ushort)(num << 8 << 4);
				num ^= (ushort)((num & 255) << 4 << 1);
				num2++;
			}
			int num3 = (int)(~(int)num);
			byte[] array = new byte[]
			{
				(byte)(num3 >> 8),
				(byte)num3
			};
			return array[1] == buf[(int)(length - 1U)] && array[0] == buf[(int)(length - 2U)];
		}

		// Token: 0x060005CD RID: 1485 RVA: 0x0003555C File Offset: 0x0003375C
		public static byte[] addCCITT162(byte[] buf, int length)
		{
			ushort num = 0;
			for (int i = 0; i < length - 2; i++)
			{
				num = (ushort)((int)((byte)(num >> 8)) | ((int)num << 8));
				num ^= (ushort)buf[i];
				num ^= (ushort)((byte)((num & 255) >> 4));
				num ^= (ushort)(num << 8 << 4);
				num ^= (ushort)((num & 255) << 4 << 1);
			}
			int num2 = (int)(~(int)num);
			buf[length - 2] = (byte)(num2 >> 8);
			buf[length - 1] = (byte)num2;
			return buf;
		}

		// Token: 0x060005CE RID: 1486 RVA: 0x000355C8 File Offset: 0x000337C8
		public static int calcCCITTCRC(byte[] buffer, int startpos, int length)
		{
			int num = 65535;
			for (int i = startpos; i < startpos + length; i++)
			{
				int num2 = (int)(buffer[i] & byte.MaxValue);
				for (int j = 0; j < 8; j++)
				{
					bool flag = ((num ^ num2) & 1) == 1;
					num = (int)((uint)num >> 1);
					if (flag)
					{
						num ^= 33800;
					}
					num2 = (int)((uint)num2 >> 1);
				}
			}
			return ~num & 65535;
		}

		// Token: 0x060005CF RID: 1487 RVA: 0x00035624 File Offset: 0x00033824
		public static string Generate2CRC(byte[] message)
		{
			ushort num = ushort.MaxValue;
			foreach (byte b in message)
			{
				num ^= (ushort)b;
				for (int j = 0; j < 8; j++)
				{
					bool flag = (num & 32768) != 0;
					num = (ushort)(num << 1);
					if (flag)
					{
						num ^= 33796;
					}
				}
			}
			return (~num).ToString("x");
		}

		// Token: 0x060005D0 RID: 1488 RVA: 0x00035686 File Offset: 0x00033886
		public static bool bit_reader(byte[] p, int i)
		{
			return (p[i >> 3] & utils.BIT_MASK_TABLE[i & 7]) > 0;
		}

		// Token: 0x060005D1 RID: 1489 RVA: 0x00025DA5 File Offset: 0x00023FA5
		public static byte[] bit_writer(byte[] p, int i, bool b)
		{
			bool[] array = utils.bytesToBits(p);
			array[i] = b;
			return utils.bitsToBytes(array);
		}

		// Token: 0x060005D2 RID: 1490 RVA: 0x000356A0 File Offset: 0x000338A0
		public static ushort Crc16Ccitt(byte[] bytes)
		{
			ushort[] array = new ushort[256];
			ushort num = ushort.MaxValue;
			for (int i = 0; i < array.Length; i++)
			{
				ushort num2 = 0;
				ushort num3 = (ushort)(i << 8);
				for (int j = 0; j < 8; j++)
				{
					if (((num2 ^ num3) & 32768) != 0)
					{
						num2 = (ushort)(((int)num2 << 1) ^ 4129);
					}
					else
					{
						num2 = (ushort)(num2 << 1);
					}
					num3 = (ushort)(num3 << 1);
				}
				array[i] = num2;
			}
			for (int k = 0; k < bytes.Length; k++)
			{
				num = (ushort)(((int)num << 8) ^ (int)array[(num >> 8) ^ (int)(byte.MaxValue & bytes[k])]);
			}
			return num;
		}

		// Token: 0x060005D3 RID: 1491 RVA: 0x00035738 File Offset: 0x00033938
		public static byte[] GetBytes(string str)
		{
			byte[] array = new byte[str.Length * 2];
			Buffer.BlockCopy(str.ToCharArray(), 0, array, 0, array.Length);
			return array;
		}

		// Token: 0x060005D4 RID: 1492 RVA: 0x00035768 File Offset: 0x00033968
		public static string GetString(byte[] bytes)
		{
			char[] array = new char[bytes.Length / 2];
			Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
			return new string(array);
		}

		// Token: 0x060005D5 RID: 1493 RVA: 0x00035794 File Offset: 0x00033994
		public static utils.Platform RunningPlatform()
		{
			PlatformID platform = Environment.OSVersion.Platform;
			if (platform != PlatformID.Unix)
			{
				if (platform != PlatformID.MacOSX)
				{
					return utils.Platform.Windows;
				}
				return utils.Platform.Mac;
			}
			else
			{
				if (Directory.Exists("/Applications") & Directory.Exists("/System") & Directory.Exists("/Users") & Directory.Exists("/Volumes"))
				{
					return utils.Platform.Mac;
				}
				return utils.Platform.Linux;
			}
		}

		// Token: 0x04000456 RID: 1110
		private static int POLYNOMIAL = 33800;

		// Token: 0x04000457 RID: 1111
		private static int PRESET_VALUE = 65535;

		// Token: 0x04000458 RID: 1112
		public static readonly byte[] BIT_MASK_TABLE = new byte[] { 128, 64, 32, 16, 8, 4, 2, 1 };

		// Token: 0x0200009E RID: 158
		public enum Platform
		{
			// Token: 0x04000580 RID: 1408
			Windows,
			// Token: 0x04000581 RID: 1409
			Linux,
			// Token: 0x04000582 RID: 1410
			Mac
		}
	}
}
