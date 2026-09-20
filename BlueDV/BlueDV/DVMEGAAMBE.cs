using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using WebSocketSharp.Server;

namespace BlueDV
{
	// Token: 0x0200001B RID: 27
	internal class DVMEGAAMBE
	{
		// Token: 0x06000163 RID: 355 RVA: 0x0000E458 File Offset: 0x0000C658
		public static void PCMtoAMBEMMDVM3000(byte[] voiceReverted)
		{
			byte[] array = new byte[326];
			array[0] = 97;
			array[1] = 1;
			array[2] = 66;
			array[3] = 2;
			array[4] = 0;
			array[5] = 160;
			Buffer.BlockCopy(voiceReverted, 0, array, 6, 320);
			productSelector.deviceWrite(array);
		}

		// Token: 0x06000164 RID: 356 RVA: 0x0000E4A4 File Offset: 0x0000C6A4
		public static void PCMtoAMBEMMDVM3003(byte[] voiceReverted)
		{
			byte[] array = new byte[327];
			array[0] = 97;
			array[1] = 1;
			array[2] = 67;
			array[3] = 2;
			array[4] = information.AMBEchannel;
			array[5] = 0;
			array[6] = 160;
			Buffer.BlockCopy(voiceReverted, 0, array, 7, 320);
			productSelector.deviceWrite(array);
		}

		// Token: 0x06000165 RID: 357 RVA: 0x0000E4F8 File Offset: 0x0000C6F8
		public static void decodeAMBE(byte ambeData)
		{
			DVMEGAAMBE.serial3_bufferP[DVMEGAAMBE.serial3_buffer_input_pointerP] = ambeData;
			DVMEGAAMBE.serial3_buffer_input_pointerP++;
			if (DVMEGAAMBE.serial3_buffer_input_pointerP > DVMEGAAMBE.ser3_in_buf_lenP)
			{
				DVMEGAAMBE.serial3_buffer_input_pointerP = 0;
			}
			if (DVMEGAAMBE.serial3_bufferP[0] != 97)
			{
				DVMEGAAMBE.serial3_buffer_input_pointerP = 0;
			}
			if (DVMEGAAMBE.serial3_bufferP[0] == 97 && DVMEGAAMBE.serial3_buffer_input_pointerP >= 3)
			{
				int num = ((int)DVMEGAAMBE.serial3_bufferP[1] << 8) | (int)DVMEGAAMBE.serial3_bufferP[2];
				if (DVMEGAAMBE.serial3_buffer_input_pointerP >= num + 4 && DVMEGAAMBE.serial3_buffer_input_pointerP >= 2)
				{
					int num2 = ((int)DVMEGAAMBE.serial3_bufferP[1] << 8) | (int)DVMEGAAMBE.serial3_bufferP[2];
					byte[] array = new byte[num2 + 4];
					Buffer.BlockCopy(DVMEGAAMBE.serial3_bufferP, 0, array, 0, num2 + 4);
					switch (array[3])
					{
					case 0:
					{
						byte b = DVMEGAAMBE.serial3_bufferP[4];
						if (b <= 49)
						{
							if (b != 48)
							{
								if (b == 49)
								{
									byte[] array2 = new byte[num2 - 2];
									Buffer.BlockCopy(DVMEGAAMBE.serial3_bufferP, 5, array2, 0, num2 - 2);
									if (Encoding.UTF8.GetString(array2).Contains("R007") && information.myDVMEGAVersion.Contains("AMBE3000F"))
									{
										information.i = true;
									}
									else
									{
										information.i = false;
									}
								}
							}
							else
							{
								byte[] array3 = new byte[num2 - 2];
								Buffer.BlockCopy(DVMEGAAMBE.serial3_bufferP, 5, array3, 0, num2 - 2);
								information.myDVMEGAVersion = Encoding.UTF8.GetString(array3);
								if (num2 > 5)
								{
									byte[] bytes = Encoding.ASCII.GetBytes(information.myDVMEGAVersion);
									byte[] array4 = new byte[bytes.Length + 4];
									array4[0] = 224;
									array4[1] = (byte)(bytes.Length + 4);
									array4[2] = 0;
									array4[3] = 1;
									Buffer.BlockCopy(bytes, 0, array4, 4, bytes.Length);
									DVMEGAAMBE.drainToDVMEGAHandler(array4);
								}
							}
						}
						else if (b != 54)
						{
							if (b != 55)
							{
								if (b != 64)
								{
								}
							}
							else
							{
								information.MODUS modus = information.setAMBEMode;
								if (modus != information.MODUS.DMR)
								{
									if (modus == information.MODUS.NXDN)
									{
										NXDNConnect.makeEndFrame(new byte[28], true);
										NXDNConnect.increment_session_id();
									}
								}
								else
								{
									DVMEGAAMBE.sendDMRHeader(66);
								}
							}
						}
						else
						{
							AMBESERVER.pong();
						}
						break;
					}
					case 1:
						if (num2 == 11)
						{
							byte[] array5 = new byte[9];
							Buffer.BlockCopy(array, 6, array5, 0, 9);
							information.MODUS modus = information.setAMBEMode;
							if (modus != information.MODUS.DMR)
							{
								if (modus == information.MODUS.DSTAR)
								{
									DVMEGAAMBE.makeDSTARvoiceFrameFromAMBE(array5);
								}
							}
							else
							{
								long num3 = DateTime.Now.Ticks / 10000L;
								if (num3 - DVMEGAAMBE.oldMilliseconds > 1000L)
								{
									DVMEGAAMBE.sendDMRHeader(65);
								}
								DVMEGAAMBE.oldMilliseconds = num3;
								DVMEGAAMBE.makeDMRvoiceFrameFromAMBE(array5);
							}
						}
						if (num2 == 9)
						{
							if (information.setAMBEMode == information.MODUS.FUSION)
							{
								long num4 = DateTime.Now.Ticks / 10000L;
								if (num4 - DVMEGAAMBE.oldMilliseconds > 1000L)
								{
									DVMEGAAMBE.setAMBEC4FM();
									fusion_extract.fn = 0;
									fusion_extract.voiceCounter = 0;
									DVMEGAAMBE.makeC4FMVoiceFrameFromAMBE(fusion_extract.C4fM_FRAME_HEADER, false);
								}
								DVMEGAAMBE.oldMilliseconds = num4;
								byte[] array6 = new byte[7];
								Buffer.BlockCopy(array, 6, array6, 0, 7);
								fusion_extract.ambe_to_vch_vech(array6);
							}
							else if (information.setAMBEMode == information.MODUS.NXDN)
							{
								byte[] array7 = new byte[7];
								Buffer.BlockCopy(array, 6, array7, 0, 7);
								DVMEGAAMBE.makeNXDNvoiceFrameFromAMBE(array7);
							}
						}
						if (num2 == 12)
						{
							byte[] array8 = new byte[9];
							Buffer.BlockCopy(array, 7, array8, 0, 9);
							information.MODUS modus = information.setAMBEMode;
							if (modus != information.MODUS.DMR)
							{
								if (modus == information.MODUS.DSTAR)
								{
									DVMEGAAMBE.makeDSTARvoiceFrameFromAMBE(array8);
								}
							}
							else
							{
								long num5 = DateTime.Now.Ticks / 10000L;
								if (num5 - DVMEGAAMBE.oldMilliseconds > 1000L)
								{
									DVMEGAAMBE.sendDMRHeader(65);
								}
								DVMEGAAMBE.oldMilliseconds = num5;
								DVMEGAAMBE.makeDMRvoiceFrameFromAMBE(array8);
							}
						}
						if (num2 == 10 && information.setAMBEMode == information.MODUS.FUSION)
						{
							long num6 = DateTime.Now.Ticks / 10000L;
							if (num6 - DVMEGAAMBE.oldMilliseconds > 1000L)
							{
								fusion_extract.fn = 0;
								fusion_extract.voiceCounter = 0;
								DVMEGAAMBE.makeC4FMVoiceFrameFromAMBE(fusion_extract.C4fM_FRAME_HEADER, false);
							}
							DVMEGAAMBE.oldMilliseconds = num6;
							byte[] array9 = new byte[7];
							Buffer.BlockCopy(array, 7, array9, 0, 7);
							fusion_extract.ambe_to_vch_vech(array9);
						}
						if (num2 == 10 && DVMEGAAMBE.serial3_bufferP[4] == 1 && information.m_ambetype == information.AMBETYPE.AMBE3000)
						{
							DVMEGAAMBE.setAMBEpacketmode();
						}
						break;
					case 2:
					{
						byte[] array10 = new byte[320];
						information.AMBETYPE ambetype = information.m_ambetype;
						if (ambetype != information.AMBETYPE.AMBE3000)
						{
							if (ambetype == information.AMBETYPE.AMBE3003)
							{
								Buffer.BlockCopy(array, 7, array10, 0, 320);
							}
						}
						else
						{
							Buffer.BlockCopy(array, 6, array10, 0, 320);
						}
						soundcard.play(array10);
						break;
					}
					}
					DVMEGAAMBE.serial3_buffer_input_pointerP = 0;
					DVMEGAAMBE.serial3_bufferP[0] = 0;
				}
			}
		}

		// Token: 0x06000166 RID: 358 RVA: 0x0000E974 File Offset: 0x0000CB74
		public static void makeDSTARHeaderFrameFromAMBE()
		{
			DVMEGAAMBE.PCMcounter = 0;
			byte[] array = new byte[44];
			array[0] = 224;
			array[1] = 44;
			array[2] = 16;
			array[3] = 0;
			array[4] = 0;
			array[5] = 0;
			array[6] = 0;
			string text = string.Concat(new string[]
			{
				"REF088 B",
				information.myCall.PadRight(8).Substring(0, 7),
				information.myDSTARmodule[0].ToString(),
				"CQCQCQ  ",
				information.myCall.PadRight(8).Substring(0, 8),
				"AMBE"
			});
			Buffer.BlockCopy(Encoding.ASCII.GetBytes(text), 0, array, 6, 36);
			DVMEGAAMBE.drainToDVMEGAHandler(array);
		}

		// Token: 0x06000167 RID: 359 RVA: 0x0000EA30 File Offset: 0x0000CC30
		private static void makeDMRvoiceFrameFromAMBE(byte[] ambe9Bytes)
		{
			byte[] array = DVMEGAAMBE.maketotalDMRframe(ambe9Bytes);
			if (array != null)
			{
				byte[] array2 = new byte[37];
				array2[0] = 224;
				array2[1] = 37;
				array2[2] = 26;
				try
				{
					Buffer.BlockCopy(array, 0, array2, 3, 34);
					DVMEGAAMBE.drainToDVMEGAHandler(array2);
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x06000168 RID: 360 RVA: 0x0000EA88 File Offset: 0x0000CC88
		private static void makeC4FMVoiceFrameFromAMBENext(byte[] ambe7voice)
		{
			fusion_extract.ambe_to_vch_vech(ambe7voice);
		}

		// Token: 0x06000169 RID: 361 RVA: 0x0000EA90 File Offset: 0x0000CC90
		public static void makeC4FMVoiceFrameFromAMBE(byte[] fullFrameFusion, bool endFrame)
		{
			byte[] array = new byte[124];
			array[0] = 224;
			array[1] = 124;
			array[2] = 32;
			array[3] = ((endFrame > false) ? 1 : 0);
			byte[] array2 = array;
			int num = 3;
			array2[num] |= (byte)((DVMEGAAMBE.fichCounter & 127U) << 1);
			DVMEGAAMBE.fichCounter += 1U;
			try
			{
				Buffer.BlockCopy(fullFrameFusion, 0, array, 4, 120);
				DVMEGAAMBE.drainToDVMEGAHandler(array);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x0600016A RID: 362 RVA: 0x0000EB08 File Offset: 0x0000CD08
		private static void makeDSTARvoiceFrameFromAMBE(byte[] ambe9Bytes)
		{
			byte[] array = new byte[15];
			array[0] = 224;
			array[1] = 15;
			array[2] = 17;
			Buffer.BlockCopy(ambe9Bytes, 0, array, 3, 9);
			array[12] = 22;
			array[13] = 41;
			array[14] = 85;
			if (DVMEGAAMBE.PCMcounter == 0)
			{
				array[12] = 85;
				array[13] = 45;
				array[14] = 22;
			}
			else
			{
				Buffer.BlockCopy(SlowData.makeSlowDataFromtext(), 0, array, 12, 3);
			}
			DVMEGAAMBE.PCMcounter++;
			if (DVMEGAAMBE.PCMcounter == 21)
			{
				DVMEGAAMBE.PCMcounter = 0;
			}
			DVMEGAAMBE.drainToDVMEGAHandler(array);
		}

		// Token: 0x0600016B RID: 363 RVA: 0x0000EB98 File Offset: 0x0000CD98
		private static void makeNXDNvoiceFrameFromAMBE(byte[] ambe7Bytes)
		{
			byte[] array = new byte[10];
			array[0] = 224;
			array[1] = 10;
			array[2] = 64;
			Buffer.BlockCopy(ambe7Bytes, 0, array, 3, 7);
			DVMEGAAMBE.drainToDVMEGAHandler(array);
		}

		// Token: 0x0600016C RID: 364 RVA: 0x0000EBCF File Offset: 0x0000CDCF
		public static void setAMBEDSTAR()
		{
			productSelector.deviceWrite(new byte[]
			{
				97, 0, 13, 0, 10, 1, 48, 7, 99, 64,
				0, 0, 0, 0, 0, 0, 72
			});
		}

		// Token: 0x0600016D RID: 365 RVA: 0x0000EBE8 File Offset: 0x0000CDE8
		public static void setAMBEDMR()
		{
			productSelector.deviceWrite(new byte[]
			{
				97, 0, 13, 0, 10, 4, 49, 7, 84, 36,
				0, 0, 0, 0, 0, 111, 72
			});
		}

		// Token: 0x0600016E RID: 366 RVA: 0x0000EC01 File Offset: 0x0000CE01
		public static void setAMBEC4FM()
		{
			productSelector.deviceWrite(new byte[]
			{
				97, 0, 13, 0, 10, 4, 49, 7, 84, 0,
				0, 0, 0, 0, 0, 112, 49
			});
		}

		// Token: 0x0600016F RID: 367 RVA: 0x0000EC1A File Offset: 0x0000CE1A
		public static void setAMBENXDN()
		{
			RuntimeHelpers.InitializeArray(new byte[17], fieldof(<PrivateImplementationDetails>.0E523C80E38F4412342D8BAD6BC08E26BFBF8482C88F01D5283AA1EEA3C074DE).FieldHandle);
			productSelector.deviceWrite(new byte[]
			{
				97, 0, 13, 0, 10, 4, 49, 7, 84, 0,
				0, 0, 0, 0, 0, 112, 49
			});
		}

		// Token: 0x06000170 RID: 368 RVA: 0x0000EC46 File Offset: 0x0000CE46
		public static void getAMBEproductID()
		{
			productSelector.deviceWrite(new byte[] { 97, 0, 1, 0, 48 });
		}

		// Token: 0x06000171 RID: 369 RVA: 0x0000EC5E File Offset: 0x0000CE5E
		public static void endFrameTrick()
		{
			productSelector.deviceWrite(new byte[] { 97, 0, 1, 0, 55 });
		}

		// Token: 0x06000172 RID: 370 RVA: 0x0000EC76 File Offset: 0x0000CE76
		public static byte[] setAMBEnoisecancel()
		{
			byte[] array = new byte[] { 97, 0, 3, 0, 5, 16, 64 };
			productSelector.deviceWrite(array);
			return array;
		}

		// Token: 0x06000173 RID: 371 RVA: 0x0000ECA3 File Offset: 0x0000CEA3
		public static byte[] setAMBEnoisecancelOff()
		{
			byte[] array = new byte[] { 97, 0, 3, 0, 5, 0, 0 };
			productSelector.deviceWrite(array);
			return array;
		}

		// Token: 0x06000174 RID: 372 RVA: 0x0000ECCE File Offset: 0x0000CECE
		public static void setVolume(byte inputGain, byte outputGain)
		{
			byte[] array = new byte[] { 97, 0, 3, 0, 75, 0, 0 };
			array[5] = inputGain;
			array[6] = outputGain;
			productSelector.deviceWrite(array);
		}

		// Token: 0x06000175 RID: 373 RVA: 0x0000ECEE File Offset: 0x0000CEEE
		public static void setParityOff()
		{
			productSelector.deviceWrite(new byte[] { 97, 0, 4, 0, 63, 0, 47, 20 });
		}

		// Token: 0x06000176 RID: 374 RVA: 0x0000ED06 File Offset: 0x0000CF06
		public static void getAMBEVerString()
		{
			productSelector.deviceWrite(new byte[] { 97, 0, 1, 0, 49 });
		}

		// Token: 0x06000177 RID: 375 RVA: 0x0000ED1E File Offset: 0x0000CF1E
		public static void setAMBESoftReset()
		{
			productSelector.deviceWrite(new byte[] { 97, 0, 1, 0, 51 });
		}

		// Token: 0x06000178 RID: 376 RVA: 0x0000ED36 File Offset: 0x0000CF36
		public static void getAMBERDVMEGA_PIN1()
		{
			productSelector.deviceWrite(new byte[] { 97, 0, 1, 0, 54 });
		}

		// Token: 0x06000179 RID: 377 RVA: 0x0000ED4E File Offset: 0x0000CF4E
		public static void setAMBEpacketmode()
		{
			productSelector.deviceWrite(new byte[]
			{
				97, 0, 7, 0, 52, 5, 0, 0, 7, 0,
				16
			});
		}

		// Token: 0x0600017A RID: 378 RVA: 0x0000ED68 File Offset: 0x0000CF68
		public static void makeDSTARVoiceMMDVM3000(byte[] AMBEStream)
		{
			byte[] array = new byte[15];
			array[0] = 97;
			array[1] = 0;
			array[2] = 11;
			array[3] = 1;
			array[4] = 1;
			array[5] = 72;
			Buffer.BlockCopy(AMBEStream, 0, array, 6, 9);
			productSelector.deviceWrite(array);
		}

		// Token: 0x0600017B RID: 379 RVA: 0x0000EDAC File Offset: 0x0000CFAC
		public static void makeDSTARVoiceMMDVM3003(byte[] AMBEStream)
		{
			byte[] array = new byte[16];
			array[0] = 97;
			array[1] = 0;
			array[2] = 12;
			array[3] = 1;
			array[4] = information.AMBEchannel;
			array[5] = 1;
			array[6] = 72;
			Buffer.BlockCopy(AMBEStream, 0, array, 7, 9);
			productSelector.deviceWrite(array);
		}

		// Token: 0x0600017C RID: 380 RVA: 0x0000EDF8 File Offset: 0x0000CFF8
		public static void makeC4FMVoiceMMDVM3000(byte[] AMBEStream)
		{
			byte[] array = new byte[13];
			array[0] = 97;
			array[1] = 0;
			array[2] = 9;
			array[3] = 1;
			array[4] = 1;
			array[5] = 49;
			Buffer.BlockCopy(AMBEStream, 0, array, 6, 7);
			productSelector.deviceWrite(array);
		}

		// Token: 0x0600017D RID: 381 RVA: 0x0000EE38 File Offset: 0x0000D038
		public static void makeC4FMVoiceMMDVM3003(byte[] AMBEStream)
		{
			byte[] array = new byte[14];
			array[0] = 97;
			array[1] = 0;
			array[2] = 10;
			array[3] = 1;
			array[4] = information.AMBEchannel;
			array[5] = 1;
			array[6] = 49;
			Buffer.BlockCopy(AMBEStream, 0, array, 7, 7);
			productSelector.deviceWrite(array);
		}

		// Token: 0x0600017E RID: 382 RVA: 0x0000EE80 File Offset: 0x0000D080
		public static void makeNXDNVoiceMMDVM3000(byte[] AMBEStream)
		{
			byte[] array = new byte[13];
			array[0] = 97;
			array[1] = 0;
			array[2] = 9;
			array[3] = 1;
			array[4] = 1;
			array[5] = 49;
			Buffer.BlockCopy(AMBEStream, 0, array, 6, 7);
			productSelector.deviceWrite(array);
		}

		// Token: 0x0600017F RID: 383 RVA: 0x0000EEC0 File Offset: 0x0000D0C0
		public static void makeNXDNVoiceMMDVM3003(byte[] AMBEStream)
		{
			byte[] array = new byte[14];
			array[0] = 97;
			array[1] = 0;
			array[2] = 10;
			array[3] = 1;
			array[4] = information.AMBEchannel;
			array[5] = 1;
			array[6] = 49;
			Buffer.BlockCopy(AMBEStream, 0, array, 7, 7);
			productSelector.deviceWrite(array);
		}

		// Token: 0x06000180 RID: 384 RVA: 0x0000EF08 File Offset: 0x0000D108
		public static void makeDstarEndMMDVM()
		{
			DVMEGAAMBE.drainToDVMEGAHandler(new byte[] { 224, 3, 19 });
		}

		// Token: 0x06000181 RID: 385 RVA: 0x0000EF28 File Offset: 0x0000D128
		private static void drainToDVMEGAHandler(byte[] mmdvmdata)
		{
			for (int i = 0; i < mmdvmdata.Length; i++)
			{
				DVMEGASerial.procesMMDVM(mmdvmdata[i]);
			}
		}

		// Token: 0x06000182 RID: 386 RVA: 0x0000EF50 File Offset: 0x0000D150
		public static bool write(byte[] data)
		{
			for (int i = 0; i < data.Length; i++)
			{
				DVMEGAAMBE.procesMMDVM(data[i]);
			}
			return true;
		}

		// Token: 0x06000183 RID: 387 RVA: 0x0000EF78 File Offset: 0x0000D178
		private static void procesMMDVM(byte mmdvmData)
		{
			DVMEGAAMBE.serial3_bufferM[DVMEGAAMBE.serial3_buffer_input_pointerM] = mmdvmData;
			DVMEGAAMBE.serial3_buffer_input_pointerM++;
			if (DVMEGAAMBE.serial3_buffer_input_pointerM > DVMEGAAMBE.ser3_in_buf_lenM)
			{
				DVMEGAAMBE.serial3_buffer_input_pointerM = 0;
			}
			if (DVMEGAAMBE.serial3_bufferM[0] != 224)
			{
				DVMEGAAMBE.serial3_buffer_input_pointerM = 0;
			}
			if (DVMEGAAMBE.serial3_bufferM[0] == 224 && DVMEGAAMBE.serial3_buffer_input_pointerM >= 3 && DVMEGAAMBE.serial3_buffer_input_pointerM >= (int)(DVMEGAAMBE.serial3_bufferM[1] & 255) && DVMEGAAMBE.serial3_buffer_input_pointerM >= 3)
			{
				byte[] array = new byte[(int)(DVMEGAAMBE.serial3_bufferM[1] & byte.MaxValue)];
				Buffer.BlockCopy(DVMEGAAMBE.serial3_bufferM, 0, array, 0, (int)(DVMEGAAMBE.serial3_bufferM[1] & byte.MaxValue));
				byte b = array[2];
				if (b <= 26)
				{
					switch (b)
					{
					case 0:
						if (information.m_ambetype == information.AMBETYPE.AMBE3003)
						{
							DVMEGAAMBE.setParityOff();
						}
						Thread.Sleep(10);
						if (information.m_ambetype == information.AMBETYPE.AMBE3000)
						{
							DVMEGAAMBE.setAMBEpacketmode();
						}
						Thread.Sleep(10);
						DVMEGAAMBE.setAMBEnoisecancel();
						Thread.Sleep(10);
						DVMEGAAMBE.setAMBEDSTAR();
						Thread.Sleep(10);
						DVMEGAAMBE.getAMBEproductID();
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
								DVMEGAAMBE.drainToDVMEGAHandler(array2);
							}
						}
						break;
					case 1:
					case 2:
						break;
					case 3:
					{
						byte b2 = array[3];
						switch (b2)
						{
						case 1:
							DVMEGAAMBE.setAMBEDSTAR();
							DVMEGAAMBE.setVolume((byte)information.DSTARinGain, (byte)information.DSTARoutGain);
							break;
						case 2:
							DVMEGAAMBE.setAMBEDMR();
							DVMEGAAMBE.setVolume((byte)information.DMRinGain, (byte)information.DMRoutGain);
							break;
						case 3:
							DVMEGAAMBE.setAMBEC4FM();
							DVMEGAAMBE.setVolume((byte)information.FUSIONinGain, (byte)information.FUSIONoutGain);
							break;
						default:
							if (b2 == 16)
							{
								DVMEGAAMBE.setAMBENXDN();
								DVMEGAAMBE.setVolume((byte)information.NXDNinGain, (byte)information.NXDNoutGain);
							}
							break;
						}
						break;
					}
					default:
						switch (b)
						{
						case 16:
							soundcard.beep(750.0);
							DVMEGAAMBE.beeped = false;
							break;
						case 17:
						{
							byte[] array3 = new byte[9];
							Buffer.BlockCopy(array, 3, array3, 0, 9);
							information.AMBETYPE ambetype = information.m_ambetype;
							if (ambetype != information.AMBETYPE.AMBE3000)
							{
								if (ambetype == information.AMBETYPE.AMBE3003)
								{
									DVMEGAAMBE.makeDSTARVoiceMMDVM3003(array3);
								}
							}
							else
							{
								DVMEGAAMBE.makeDSTARVoiceMMDVM3000(array3);
							}
							break;
						}
						case 18:
							break;
						case 19:
							if (!DVMEGAAMBE.beeped)
							{
								ThreadPool.QueueUserWorkItem(delegate
								{
									Thread.Sleep(1000);
									soundcard.beep(550.0);
								});
								DVMEGAAMBE.beeped = true;
							}
							break;
						default:
							if (b == 26)
							{
								if (array[3] == 65)
								{
									soundcard.beep(750.0);
									DVMEGAAMBE.beeped = false;
								}
								if (array[3] == 66 && !DVMEGAAMBE.beeped)
								{
									soundcard.beep(550.0);
								}
								byte[] array4 = new byte[33];
								if (array[3] != 65 && array[3] != 66)
								{
									DVMEGAAMBE.beeped = false;
								}
								Buffer.BlockCopy(array, 4, array4, 0, 33);
								DVMEGAAMBE.makeDMRtoAMBE(array4, array[3]);
							}
							break;
						}
						break;
					}
				}
				else if (b <= 64)
				{
					if (b != 32)
					{
						if (b == 64)
						{
							byte[] array5 = new byte[7];
							Buffer.BlockCopy(array, 3, array5, 0, 7);
							DVMEGAAMBE.makeNXDNVoiceMMDVM3000(array5);
						}
					}
					else
					{
						information.DVMEGAbufferFUSION = 10;
						byte[] array6 = new byte[120];
						Buffer.BlockCopy(array, 4, array6, 0, 120);
						fusion_extract.processFusion(array6);
					}
				}
				else if (b != 112 && b != 127)
				{
				}
				DVMEGAAMBE.serial3_buffer_input_pointerM = 0;
				DVMEGAAMBE.serial3_bufferM[0] = 0;
			}
		}

		// Token: 0x06000184 RID: 388 RVA: 0x0000F32C File Offset: 0x0000D52C
		private static void makeDMRtoAMBE(byte[] ambeDMR, byte v)
		{
			if (v == 161)
			{
				return;
			}
			if (v == 162)
			{
				return;
			}
			if (v == 65)
			{
				return;
			}
			if (v == 66)
			{
				return;
			}
			if (v == 24)
			{
				return;
			}
			if (v == 40)
			{
				return;
			}
			new BitArray(ambeDMR);
			Array array = utils.byteToBitsBE(ambeDMR[13]);
			bool[] array2 = utils.byteToBitsBE(ambeDMR[19]);
			bool[] array3 = new bool[8];
			Buffer.BlockCopy(array, 0, array3, 0, 4);
			Buffer.BlockCopy(array2, 4, array3, 4, 4);
			byte[] array4 = new byte[9];
			byte[] array5 = new byte[9];
			byte[] array6 = new byte[9];
			Buffer.BlockCopy(ambeDMR, 0, array4, 0, 9);
			Buffer.BlockCopy(ambeDMR, 9, array5, 0, 4);
			array5[4] = utils.bitsToByteBE(array3);
			Buffer.BlockCopy(ambeDMR, 20, array5, 5, 4);
			Buffer.BlockCopy(ambeDMR, 24, array6, 0, 9);
			information.AMBETYPE ambetype = information.m_ambetype;
			if (ambetype == information.AMBETYPE.AMBE3000)
			{
				DVMEGAAMBE.makeDSTARVoiceMMDVM3000(array4);
				Thread.Sleep(10);
				DVMEGAAMBE.makeDSTARVoiceMMDVM3000(array5);
				Thread.Sleep(10);
				DVMEGAAMBE.makeDSTARVoiceMMDVM3000(array6);
				Thread.Sleep(10);
				return;
			}
			if (ambetype != information.AMBETYPE.AMBE3003)
			{
				return;
			}
			DVMEGAAMBE.makeDSTARVoiceMMDVM3003(array4);
			Thread.Sleep(10);
			DVMEGAAMBE.makeDSTARVoiceMMDVM3003(array5);
			Thread.Sleep(10);
			DVMEGAAMBE.makeDSTARVoiceMMDVM3003(array6);
			Thread.Sleep(10);
		}

		// Token: 0x06000185 RID: 389 RVA: 0x0000F44C File Offset: 0x0000D64C
		private static void DisplayBitArray(BitArray bitArray)
		{
			for (int i = 0; i < bitArray.Count; i++)
			{
				bitArray.Get(i);
			}
		}

		// Token: 0x06000186 RID: 390 RVA: 0x0000F474 File Offset: 0x0000D674
		public static byte[] BitArrayT11oByteArray(BitArray bits)
		{
			byte[] array = new byte[(bits.Length - 1) / 8 + 1];
			bits.CopyTo(array, 0);
			return array;
		}

		// Token: 0x06000187 RID: 391 RVA: 0x0000F49C File Offset: 0x0000D69C
		public static byte[] maketotalDMRframe(byte[] nineBytevoice)
		{
			bool[] array = DVMEGAAMBE.byteToBitConverter(nineBytevoice);
			switch (DVMEGAAMBE.voiceNumber)
			{
			case 1:
			{
				for (int i = 0; i < 72; i++)
				{
					DVMEGAAMBE.totalVoiceBool[i] = array[i];
				}
				DVMEGAAMBE.voiceNumber++;
				break;
			}
			case 2:
			{
				for (int j = 0; j < 36; j++)
				{
					DVMEGAAMBE.totalVoiceBool[j + 72] = array[j];
				}
				for (int k = 36; k < 72; k++)
				{
					DVMEGAAMBE.totalVoiceBool[k + 120] = array[k];
				}
				DVMEGAAMBE.voiceNumber++;
				break;
			}
			case 3:
			{
				for (int l = 0; l < 72; l++)
				{
					DVMEGAAMBE.totalVoiceBool[l + 192] = array[l];
				}
				switch (DVMEGAAMBE.fragmentCounter)
				{
				case 1:
				{
					DVMEGAAMBE.typeDMRByte = 32;
					for (int m = 0; m < 48; m++)
					{
						DVMEGAAMBE.totalVoiceBool[m + 108] = DVMEGAAMBE.nullfragment[m];
					}
					DVMEGAAMBE.fragmentCounter++;
					break;
				}
				case 2:
				{
					DVMEGAAMBE.typeDMRByte = 1;
					for (int n = 0; n < 48; n++)
					{
						DVMEGAAMBE.totalVoiceBool[n + 108] = DVMEGAAMBE.fragment1[n];
					}
					DVMEGAAMBE.fragmentCounter++;
					break;
				}
				case 3:
				{
					DVMEGAAMBE.typeDMRByte = 2;
					for (int num = 0; num < 48; num++)
					{
						DVMEGAAMBE.totalVoiceBool[num + 108] = DVMEGAAMBE.fragment2[num];
					}
					DVMEGAAMBE.fragmentCounter++;
					break;
				}
				case 4:
				{
					DVMEGAAMBE.typeDMRByte = 3;
					for (int num2 = 0; num2 < 48; num2++)
					{
						DVMEGAAMBE.totalVoiceBool[num2 + 108] = DVMEGAAMBE.fragment3[num2];
					}
					DVMEGAAMBE.fragmentCounter++;
					break;
				}
				case 5:
				{
					DVMEGAAMBE.typeDMRByte = 4;
					for (int num3 = 0; num3 < 48; num3++)
					{
						DVMEGAAMBE.totalVoiceBool[num3 + 108] = DVMEGAAMBE.fragment4[num3];
					}
					DVMEGAAMBE.fragmentCounter++;
					break;
				}
				case 6:
				{
					DVMEGAAMBE.typeDMRByte = 5;
					for (int num4 = 0; num4 < 48; num4++)
					{
						DVMEGAAMBE.totalVoiceBool[num4 + 108] = DVMEGAAMBE.fragment5[num4];
					}
					DVMEGAAMBE.fragmentCounter = 1;
					break;
				}
				}
				DVMEGAAMBE.voiceNumber = 1;
				byte[] array2 = new byte[34];
				array2[0] = DVMEGAAMBE.typeDMRByte;
				Buffer.BlockCopy(DVMEGAAMBE.bitToByteConvevrt(DVMEGAAMBE.totalVoiceBool), 0, array2, 1, 33);
				return array2;
			}
			}
			return null;
		}

		// Token: 0x06000188 RID: 392 RVA: 0x0000F718 File Offset: 0x0000D918
		public static byte[] bitToByteConvevrt(bool[] bits)
		{
			byte[] array = new byte[33];
			int num = 0;
			for (int i = 0; i < 33; i++)
			{
				bool[] array2 = new bool[8];
				for (int j = 0; j < 8; j++)
				{
					array2[j] = bits[num];
					num++;
				}
				array[i] = utils.bitsToByteBE(array2);
			}
			return array;
		}

		// Token: 0x06000189 RID: 393 RVA: 0x0000F768 File Offset: 0x0000D968
		public static bool[] byteToBitConverter(byte[] data)
		{
			bool[] array = new bool[data.Length * 8];
			for (int i = 0; i < data.Length; i++)
			{
				new bool[8];
				Buffer.BlockCopy(utils.byteToBitsBE(data[i]), 0, array, i * 8, 8);
			}
			return array;
		}

		// Token: 0x0600018A RID: 394 RVA: 0x0000F7AC File Offset: 0x0000D9AC
		public static void generateBPTC()
		{
			int num = int.Parse(information.AMBEDMRid);
			int num2 = int.Parse(information.myAMBEDstDMRID);
			bool[] array = new bool[8];
			information.GROUPPRIVATE groupprivate = information.m_groupprivate;
			if (groupprivate != information.GROUPPRIVATE.GROUP)
			{
				if (groupprivate == information.GROUPPRIVATE.PRIVATE)
				{
					array = new bool[]
					{
						default(bool),
						default(bool),
						default(bool),
						default(bool),
						default(bool),
						default(bool),
						true,
						true
					};
				}
			}
			else
			{
				array = new bool[8];
			}
			bool[] array2 = new bool[8];
			bool[] array3 = new bool[8];
			bool[] array4 = utils.byteToBitsBE((byte)(num2 >> 16));
			bool[] array5 = utils.byteToBitsBE((byte)(num2 >> 8));
			bool[] array6 = utils.byteToBitsBE((byte)num2);
			bool[] array7 = utils.byteToBitsBE((byte)(num >> 16));
			bool[] array8 = utils.byteToBitsBE((byte)(num >> 8));
			bool[] array9 = utils.byteToBitsBE((byte)num);
			foreach (bool flag in array7)
			{
			}
			foreach (bool flag2 in array8)
			{
			}
			foreach (bool flag3 in array9)
			{
			}
			bool[] array11 = new bool[16];
			bool[] array12 = new bool[16];
			bool[] array13 = new bool[16];
			bool[] array14 = new bool[16];
			bool[] array15 = new bool[16];
			bool[] array16 = new bool[16];
			bool[] array17 = new bool[16];
			bool[] array18 = new bool[16];
			Buffer.BlockCopy(array, 0, array11, 0, 8);
			Buffer.BlockCopy(array2, 0, array11, 8, 3);
			Buffer.BlockCopy(array2, 3, array12, 0, 5);
			Buffer.BlockCopy(array3, 0, array12, 5, 6);
			Buffer.BlockCopy(array3, 6, array13, 0, 2);
			Buffer.BlockCopy(array4, 0, array13, 2, 8);
			Buffer.BlockCopy(array5, 0, array14, 0, 8);
			Buffer.BlockCopy(array6, 0, array14, 8, 2);
			Buffer.BlockCopy(array6, 2, array15, 0, 6);
			Buffer.BlockCopy(array7, 0, array15, 6, 4);
			Buffer.BlockCopy(array7, 4, array16, 0, 4);
			Buffer.BlockCopy(array8, 0, array16, 4, 6);
			Buffer.BlockCopy(array8, 6, array17, 0, 2);
			Buffer.BlockCopy(array9, 0, array17, 2, 8);
			bool[] array19 = new bool[72];
			Buffer.BlockCopy(array, 0, array19, 0, 8);
			Buffer.BlockCopy(array2, 0, array19, 8, 8);
			Buffer.BlockCopy(array3, 0, array19, 16, 8);
			Buffer.BlockCopy(array4, 0, array19, 24, 8);
			Buffer.BlockCopy(array5, 0, array19, 32, 8);
			Buffer.BlockCopy(array6, 0, array19, 40, 8);
			Buffer.BlockCopy(array7, 0, array19, 48, 8);
			Buffer.BlockCopy(array8, 0, array19, 56, 8);
			Buffer.BlockCopy(array9, 0, array19, 64, 8);
			char c = new crc().crcFiveBitNewNEW(array19);
			array13[10] = (c & '\u0010') == '\u0010';
			array14[10] = (c & '\b') == '\b';
			array15[10] = (c & '\u0004') == '\u0004';
			array16[10] = (c & '\u0002') == '\u0002';
			array17[10] = (c & '\u0001') == '\u0001';
			array11 = DecodeDMR.encode16114(array11);
			array12 = DecodeDMR.encode16114(array12);
			array13 = DecodeDMR.encode16114(array13);
			array14 = DecodeDMR.encode16114(array14);
			array15 = DecodeDMR.encode16114(array15);
			array16 = DecodeDMR.encode16114(array16);
			array17 = DecodeDMR.encode16114(array17);
			for (int j = 0; j < 16; j++)
			{
				array18[j] = array11[j] ^ array12[j] ^ array13[j] ^ array14[j] ^ array15[j] ^ array16[j] ^ array17[j];
			}
			foreach (bool flag4 in array18)
			{
			}
			bool[] array20 = new bool[128];
			Buffer.BlockCopy(array11, 0, array20, 0, 16);
			Buffer.BlockCopy(array12, 0, array20, 16, 16);
			Buffer.BlockCopy(array13, 0, array20, 32, 16);
			Buffer.BlockCopy(array14, 0, array20, 48, 16);
			Buffer.BlockCopy(array15, 0, array20, 64, 16);
			Buffer.BlockCopy(array16, 0, array20, 80, 16);
			Buffer.BlockCopy(array17, 0, array20, 96, 16);
			Buffer.BlockCopy(array18, 0, array20, 112, 16);
			int num3 = 0;
			bool[] array21 = new bool[128];
			for (int k = 0; k < 128; k++)
			{
				array21[k] = array20[num3];
				num3 += 16;
				if (num3 > 127)
				{
					num3 -= 127;
				}
			}
			bool[] array22 = new bool[32];
			bool[] array23 = new bool[32];
			bool[] array24 = new bool[32];
			bool[] array25 = new bool[32];
			Buffer.BlockCopy(array21, 0, array22, 0, 32);
			Buffer.BlockCopy(array21, 32, array23, 0, 32);
			Buffer.BlockCopy(array21, 64, array24, 0, 32);
			Buffer.BlockCopy(array21, 96, array25, 0, 32);
			Buffer.BlockCopy(array22, 0, DVMEGAAMBE.fragment1, 8, 32);
			Buffer.BlockCopy(array23, 0, DVMEGAAMBE.fragment2, 8, 32);
			Buffer.BlockCopy(array24, 0, DVMEGAAMBE.fragment3, 8, 32);
			Buffer.BlockCopy(array25, 0, DVMEGAAMBE.fragment4, 8, 32);
		}

		// Token: 0x0600018B RID: 395 RVA: 0x0000FC4A File Offset: 0x0000DE4A
		private static bool GetBit(ushort bits, int offset)
		{
			return ((int)bits & (1 << offset)) != 0;
		}

		// Token: 0x0600018C RID: 396 RVA: 0x0000FC58 File Offset: 0x0000DE58
		public static byte[] generateVoiceHeader(byte typeDMRByte1)
		{
			int num = int.Parse(information.AMBEDMRid);
			int num2 = int.Parse(information.myAMBEDstDMRID);
			bool[] array = new bool[]
			{
				default(bool),
				default(bool),
				default(bool),
				default(bool),
				default(bool),
				default(bool),
				true,
				true
			};
			information.GROUPPRIVATE groupprivate = information.m_groupprivate;
			if (groupprivate != information.GROUPPRIVATE.GROUP)
			{
				if (groupprivate == information.GROUPPRIVATE.PRIVATE)
				{
					array = new bool[]
					{
						default(bool),
						default(bool),
						default(bool),
						default(bool),
						default(bool),
						default(bool),
						true,
						true
					};
				}
			}
			else
			{
				array = new bool[8];
			}
			bool[] array2 = new bool[8];
			bool[] array3 = new bool[8];
			bool[] array4 = utils.byteToBitsBE((byte)(num2 >> 16));
			bool[] array5 = utils.byteToBitsBE((byte)(num2 >> 8));
			bool[] array6 = utils.byteToBitsBE((byte)num2);
			bool[] array7 = utils.byteToBitsBE((byte)(num >> 16));
			bool[] array8 = utils.byteToBitsBE((byte)(num >> 8));
			bool[] array9 = utils.byteToBitsBE((byte)num);
			bool[] array10 = new bool[8];
			bool[] array11 = new bool[8];
			bool[] array12 = new bool[8];
			byte[] array13 = CRS129.Encode_RS129(new byte[]
			{
				utils.bitsToByteBE(array),
				utils.bitsToByteBE(array2),
				utils.bitsToByteBE(array3),
				utils.bitsToByteBE(array4),
				utils.bitsToByteBE(array5),
				utils.bitsToByteBE(array6),
				utils.bitsToByteBE(array7),
				utils.bitsToByteBE(array8),
				utils.bitsToByteBE(array9)
			});
			if (typeDMRByte1 == 65)
			{
				byte[] array14 = array13;
				int num3 = 0;
				array12 = utils.byteToBitsBE(array14[num3] ^= 150);
				byte[] array15 = array13;
				int num4 = 1;
				array11 = utils.byteToBitsBE(array15[num4] ^= 150);
				byte[] array16 = array13;
				int num5 = 2;
				array10 = utils.byteToBitsBE(array16[num5] ^= 150);
			}
			else
			{
				byte[] array17 = array13;
				int num6 = 0;
				array12 = utils.byteToBitsBE(array17[num6] ^= 153);
				byte[] array18 = array13;
				int num7 = 1;
				array11 = utils.byteToBitsBE(array18[num7] ^= 153);
				byte[] array19 = array13;
				int num8 = 2;
				array10 = utils.byteToBitsBE(array19[num8] ^= 153);
			}
			foreach (bool flag in array12)
			{
			}
			foreach (bool flag2 in array11)
			{
			}
			foreach (bool flag3 in array10)
			{
			}
			foreach (bool flag4 in array9)
			{
			}
			foreach (bool flag5 in array8)
			{
			}
			foreach (bool flag6 in array7)
			{
			}
			foreach (bool flag7 in array6)
			{
			}
			foreach (bool flag8 in array5)
			{
			}
			foreach (bool flag9 in array4)
			{
			}
			foreach (bool flag10 in array3)
			{
			}
			foreach (bool flag11 in array2)
			{
			}
			foreach (bool flag12 in array)
			{
			}
			bool[] array21 = new bool[196];
			bool[] array22 = new bool[196];
			new bool[96];
			bool[] array23 = new bool[15];
			bool[] array24 = new bool[15];
			bool[] array25 = new bool[15];
			bool[] array26 = new bool[15];
			bool[] array27 = new bool[15];
			bool[] array28 = new bool[15];
			bool[] array29 = new bool[15];
			bool[] array30 = new bool[15];
			bool[] array31 = new bool[15];
			bool[] array32 = new bool[15];
			bool[] array33 = new bool[15];
			bool[] array34 = new bool[15];
			bool[] array35 = new bool[15];
			array23[0] = false;
			array23[1] = false;
			array23[2] = false;
			Buffer.BlockCopy(array, 0, array23, 3, 8);
			Buffer.BlockCopy(array2, 0, array24, 0, 8);
			Buffer.BlockCopy(array3, 0, array24, 8, 3);
			Buffer.BlockCopy(array3, 3, array25, 0, 5);
			Buffer.BlockCopy(array4, 0, array25, 5, 6);
			Buffer.BlockCopy(array4, 6, array26, 0, 2);
			Buffer.BlockCopy(array5, 0, array26, 2, 8);
			Buffer.BlockCopy(array6, 0, array26, 10, 1);
			Buffer.BlockCopy(array6, 1, array27, 0, 7);
			Buffer.BlockCopy(array7, 0, array27, 7, 4);
			Buffer.BlockCopy(array7, 4, array28, 0, 4);
			Buffer.BlockCopy(array8, 0, array28, 4, 7);
			Buffer.BlockCopy(array8, 7, array29, 0, 1);
			Buffer.BlockCopy(array9, 0, array29, 1, 8);
			Buffer.BlockCopy(array10, 0, array29, 9, 2);
			Buffer.BlockCopy(array10, 2, array30, 0, 6);
			Buffer.BlockCopy(array11, 0, array30, 6, 5);
			Buffer.BlockCopy(array11, 5, array31, 0, 3);
			Buffer.BlockCopy(array12, 0, array31, 3, 8);
			array23 = DecodeDMR.encodehamming15113(array23);
			array24 = DecodeDMR.encodehamming15113(array24);
			array25 = DecodeDMR.encodehamming15113(array25);
			array26 = DecodeDMR.encodehamming15113(array26);
			array27 = DecodeDMR.encodehamming15113(array27);
			array28 = DecodeDMR.encodehamming15113(array28);
			array29 = DecodeDMR.encodehamming15113(array29);
			array30 = DecodeDMR.encodehamming15113(array30);
			array31 = DecodeDMR.encodehamming15113(array31);
			for (int j = 0; j < 15; j++)
			{
				bool[] array36 = new bool[13];
				array36[0] = array23[j];
				array36[1] = array24[j];
				array36[2] = array25[j];
				array36[3] = array26[j];
				array36[4] = array27[j];
				array36[5] = array28[j];
				array36[6] = array29[j];
				array36[7] = array30[j];
				array36[8] = array31[j];
				bool[] array37 = DecodeDMR.hamming1393(array36);
				array32[j] = array37[0];
				array33[j] = array37[1];
				array34[j] = array37[2];
				array35[j] = array37[3];
				foreach (bool flag13 in array37)
				{
				}
			}
			array22[0] = false;
			Buffer.BlockCopy(array23, 0, array22, 1, 15);
			Buffer.BlockCopy(array24, 0, array22, 16, 15);
			Buffer.BlockCopy(array25, 0, array22, 31, 15);
			Buffer.BlockCopy(array26, 0, array22, 46, 15);
			Buffer.BlockCopy(array27, 0, array22, 61, 15);
			Buffer.BlockCopy(array28, 0, array22, 76, 15);
			Buffer.BlockCopy(array29, 0, array22, 91, 15);
			Buffer.BlockCopy(array30, 0, array22, 106, 15);
			Buffer.BlockCopy(array31, 0, array22, 121, 15);
			Buffer.BlockCopy(array32, 0, array22, 136, 15);
			Buffer.BlockCopy(array33, 0, array22, 151, 15);
			Buffer.BlockCopy(array34, 0, array22, 166, 15);
			Buffer.BlockCopy(array35, 0, array22, 181, 15);
			for (int k = 0; k < 196; k++)
			{
				int num9 = k * 181 % 196;
				array21[num9] = array22[k];
			}
			foreach (bool flag14 in array21)
			{
			}
			Buffer.BlockCopy(array21, 0, DVMEGAAMBE.voiceHeader, 0, 98);
			Buffer.BlockCopy(array21, 98, DVMEGAAMBE.voiceHeader, 166, 98);
			DVMEGAAMBE.voiceHeader[0] = false;
			int num10 = 0;
			byte[] array38 = new byte[33];
			for (int l = 0; l < 33; l++)
			{
				bool[] array39 = new bool[8];
				Buffer.BlockCopy(DVMEGAAMBE.voiceHeader, num10, array39, 0, 8);
				array38[l] = utils.bitsToByteBE(array39);
				num10 += 8;
			}
			return array38;
		}

		// Token: 0x0600018D RID: 397 RVA: 0x000103A4 File Offset: 0x0000E5A4
		public static void sendDMRHeader(byte typeStopStart)
		{
			byte[] array = new byte[37];
			array[0] = 224;
			array[1] = 37;
			array[2] = 26;
			array[3] = typeStopStart;
			try
			{
				Buffer.BlockCopy(DVMEGAAMBE.generateVoiceHeader(typeStopStart), 0, array, 4, 33);
				DVMEGAAMBE.drainToDVMEGAHandler(array);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x0600018E RID: 398 RVA: 0x000103FC File Offset: 0x0000E5FC
		public static byte[] getDMRVoiceHeaderMMDVM(byte typeStopStart)
		{
			byte[] array = new byte[37];
			array[0] = 224;
			array[1] = 37;
			array[2] = 26;
			array[3] = typeStopStart;
			try
			{
				Buffer.BlockCopy(DVMEGAAMBE.generateVoiceHeader(typeStopStart), 0, array, 4, 33);
			}
			catch (Exception)
			{
			}
			return array;
		}

		// Token: 0x0600018F RID: 399 RVA: 0x00010450 File Offset: 0x0000E650
		public static byte[] generateVoiceHeaderParameters(byte typeDMRByte1, int dstDMRid, information.GROUPPRIVATE m_groupprivate)
		{
			int num = int.Parse(information.AMBEDMRid);
			bool[] array = new bool[]
			{
				default(bool),
				default(bool),
				default(bool),
				default(bool),
				default(bool),
				default(bool),
				true,
				true
			};
			if (m_groupprivate != information.GROUPPRIVATE.GROUP)
			{
				if (m_groupprivate == information.GROUPPRIVATE.PRIVATE)
				{
					array = new bool[]
					{
						default(bool),
						default(bool),
						default(bool),
						default(bool),
						default(bool),
						default(bool),
						true,
						true
					};
				}
			}
			else
			{
				array = new bool[8];
			}
			bool[] array2 = new bool[8];
			bool[] array3 = new bool[8];
			bool[] array4 = utils.byteToBitsBE((byte)(dstDMRid >> 16));
			bool[] array5 = utils.byteToBitsBE((byte)(dstDMRid >> 8));
			bool[] array6 = utils.byteToBitsBE((byte)dstDMRid);
			bool[] array7 = utils.byteToBitsBE((byte)(num >> 16));
			bool[] array8 = utils.byteToBitsBE((byte)(num >> 8));
			bool[] array9 = utils.byteToBitsBE((byte)num);
			bool[] array10 = new bool[8];
			bool[] array11 = new bool[8];
			bool[] array12 = new bool[8];
			byte[] array13 = CRS129.Encode_RS129(new byte[]
			{
				utils.bitsToByteBE(array),
				utils.bitsToByteBE(array2),
				utils.bitsToByteBE(array3),
				utils.bitsToByteBE(array4),
				utils.bitsToByteBE(array5),
				utils.bitsToByteBE(array6),
				utils.bitsToByteBE(array7),
				utils.bitsToByteBE(array8),
				utils.bitsToByteBE(array9)
			});
			if (typeDMRByte1 == 65)
			{
				byte[] array14 = array13;
				int num2 = 0;
				array12 = utils.byteToBitsBE(array14[num2] ^= 150);
				byte[] array15 = array13;
				int num3 = 1;
				array11 = utils.byteToBitsBE(array15[num3] ^= 150);
				byte[] array16 = array13;
				int num4 = 2;
				array10 = utils.byteToBitsBE(array16[num4] ^= 150);
			}
			else
			{
				byte[] array17 = array13;
				int num5 = 0;
				array12 = utils.byteToBitsBE(array17[num5] ^= 153);
				byte[] array18 = array13;
				int num6 = 1;
				array11 = utils.byteToBitsBE(array18[num6] ^= 153);
				byte[] array19 = array13;
				int num7 = 2;
				array10 = utils.byteToBitsBE(array19[num7] ^= 153);
			}
			foreach (bool flag in array12)
			{
			}
			foreach (bool flag2 in array11)
			{
			}
			foreach (bool flag3 in array10)
			{
			}
			foreach (bool flag4 in array9)
			{
			}
			foreach (bool flag5 in array8)
			{
			}
			foreach (bool flag6 in array7)
			{
			}
			foreach (bool flag7 in array6)
			{
			}
			foreach (bool flag8 in array5)
			{
			}
			foreach (bool flag9 in array4)
			{
			}
			foreach (bool flag10 in array3)
			{
			}
			foreach (bool flag11 in array2)
			{
			}
			foreach (bool flag12 in array)
			{
			}
			bool[] array21 = new bool[196];
			bool[] array22 = new bool[196];
			new bool[96];
			bool[] array23 = new bool[15];
			bool[] array24 = new bool[15];
			bool[] array25 = new bool[15];
			bool[] array26 = new bool[15];
			bool[] array27 = new bool[15];
			bool[] array28 = new bool[15];
			bool[] array29 = new bool[15];
			bool[] array30 = new bool[15];
			bool[] array31 = new bool[15];
			bool[] array32 = new bool[15];
			bool[] array33 = new bool[15];
			bool[] array34 = new bool[15];
			bool[] array35 = new bool[15];
			array23[0] = false;
			array23[1] = false;
			array23[2] = false;
			Buffer.BlockCopy(array, 0, array23, 3, 8);
			Buffer.BlockCopy(array2, 0, array24, 0, 8);
			Buffer.BlockCopy(array3, 0, array24, 8, 3);
			Buffer.BlockCopy(array3, 3, array25, 0, 5);
			Buffer.BlockCopy(array4, 0, array25, 5, 6);
			Buffer.BlockCopy(array4, 6, array26, 0, 2);
			Buffer.BlockCopy(array5, 0, array26, 2, 8);
			Buffer.BlockCopy(array6, 0, array26, 10, 1);
			Buffer.BlockCopy(array6, 1, array27, 0, 7);
			Buffer.BlockCopy(array7, 0, array27, 7, 4);
			Buffer.BlockCopy(array7, 4, array28, 0, 4);
			Buffer.BlockCopy(array8, 0, array28, 4, 7);
			Buffer.BlockCopy(array8, 7, array29, 0, 1);
			Buffer.BlockCopy(array9, 0, array29, 1, 8);
			Buffer.BlockCopy(array10, 0, array29, 9, 2);
			Buffer.BlockCopy(array10, 2, array30, 0, 6);
			Buffer.BlockCopy(array11, 0, array30, 6, 5);
			Buffer.BlockCopy(array11, 5, array31, 0, 3);
			Buffer.BlockCopy(array12, 0, array31, 3, 8);
			array23 = DecodeDMR.encodehamming15113(array23);
			array24 = DecodeDMR.encodehamming15113(array24);
			array25 = DecodeDMR.encodehamming15113(array25);
			array26 = DecodeDMR.encodehamming15113(array26);
			array27 = DecodeDMR.encodehamming15113(array27);
			array28 = DecodeDMR.encodehamming15113(array28);
			array29 = DecodeDMR.encodehamming15113(array29);
			array30 = DecodeDMR.encodehamming15113(array30);
			array31 = DecodeDMR.encodehamming15113(array31);
			for (int j = 0; j < 15; j++)
			{
				bool[] array36 = new bool[13];
				array36[0] = array23[j];
				array36[1] = array24[j];
				array36[2] = array25[j];
				array36[3] = array26[j];
				array36[4] = array27[j];
				array36[5] = array28[j];
				array36[6] = array29[j];
				array36[7] = array30[j];
				array36[8] = array31[j];
				bool[] array37 = DecodeDMR.hamming1393(array36);
				array32[j] = array37[0];
				array33[j] = array37[1];
				array34[j] = array37[2];
				array35[j] = array37[3];
				foreach (bool flag13 in array37)
				{
				}
			}
			Buffer.BlockCopy(array23, 0, array22, 1, 15);
			Buffer.BlockCopy(array24, 0, array22, 16, 15);
			Buffer.BlockCopy(array25, 0, array22, 31, 15);
			Buffer.BlockCopy(array26, 0, array22, 46, 15);
			Buffer.BlockCopy(array27, 0, array22, 61, 15);
			Buffer.BlockCopy(array28, 0, array22, 76, 15);
			Buffer.BlockCopy(array29, 0, array22, 91, 15);
			Buffer.BlockCopy(array30, 0, array22, 106, 15);
			Buffer.BlockCopy(array31, 0, array22, 121, 15);
			Buffer.BlockCopy(array32, 0, array22, 136, 15);
			Buffer.BlockCopy(array33, 0, array22, 151, 15);
			Buffer.BlockCopy(array34, 0, array22, 166, 15);
			Buffer.BlockCopy(array35, 0, array22, 181, 15);
			int num8 = 0;
			while ((long)num8 < 196L)
			{
				int num9 = num8 * 181 % 196;
				array21[num9] = array22[num8];
				num8++;
			}
			foreach (bool flag14 in array21)
			{
			}
			Buffer.BlockCopy(array21, 0, DVMEGAAMBE.voiceHeader, 0, 98);
			Buffer.BlockCopy(array21, 98, DVMEGAAMBE.voiceHeader, 166, 98);
			int num10 = 0;
			byte[] array38 = new byte[33];
			for (int k = 0; k < 33; k++)
			{
				bool[] array39 = new bool[8];
				Buffer.BlockCopy(DVMEGAAMBE.voiceHeader, num10, array39, 0, 8);
				array38[k] = utils.bitsToByteBE(array39);
				num10 += 8;
			}
			byte[] array40 = new byte[34];
			array40[0] = typeDMRByte1;
			Buffer.BlockCopy(array38, 0, array40, 1, 33);
			return array40;
		}

		// Token: 0x040000C0 RID: 192
		private static int ser3_in_buf_lenP = 330;

		// Token: 0x040000C1 RID: 193
		private static byte[] serial3_bufferP = new byte[DVMEGAAMBE.ser3_in_buf_lenP + 1];

		// Token: 0x040000C2 RID: 194
		private static int serial3_buffer_input_pointerP = 0;

		// Token: 0x040000C3 RID: 195
		private static int ser3_in_buf_lenM = 330;

		// Token: 0x040000C4 RID: 196
		private static byte[] serial3_bufferM = new byte[DVMEGAAMBE.ser3_in_buf_lenM + 1];

		// Token: 0x040000C5 RID: 197
		private static int serial3_buffer_input_pointerM = 0;

		// Token: 0x040000C6 RID: 198
		private static int voiceNumber = 1;

		// Token: 0x040000C7 RID: 199
		private static int PCMcounter = 0;

		// Token: 0x040000C8 RID: 200
		private static byte typeDMRByte = 1;

		// Token: 0x040000C9 RID: 201
		private static long oldMilliseconds = 0L;

		// Token: 0x040000CA RID: 202
		private static uint fichCounter = 0U;

		// Token: 0x040000CB RID: 203
		public static bool[] fragment1 = new bool[]
		{
			false, false, false, true, false, false, true, true, false, false,
			false, false, false, true, false, true, false, false, false, false,
			false, true, false, true, false, false, false, true, false, true,
			true, true, false, false, false, false, true, true, false, false,
			true, false, false, true, false, false, false, true
		};

		// Token: 0x040000CC RID: 204
		public static bool[] fragment2 = new bool[]
		{
			false, false, false, true, false, true, true, true, false, false,
			false, false, true, false, true, false, false, false, false, true,
			false, false, false, true, true, false, false, true, false, true,
			false, true, true, false, false, true, false, false, false, false,
			false, true, true, true, false, true, false, false
		};

		// Token: 0x040000CD RID: 205
		public static bool[] fragment3 = new bool[]
		{
			false, false, false, true, false, true, true, true, false, false,
			false, false, false, true, true, false, false, false, false, false,
			true, true, false, false, false, false, false, false, true, true,
			false, false, true, false, false, true, true, true, false, false,
			false, true, true, true, false, true, false, false
		};

		// Token: 0x040000CE RID: 206
		public static bool[] fragment4 = new bool[]
		{
			false, false, false, true, false, true, false, true, true, false,
			false, false, true, false, true, true, true, false, false, true,
			false, false, false, false, false, false, false, true, false, true,
			false, false, true, false, false, true, false, false, false, false,
			false, false, false, false, false, true, true, true
		};

		// Token: 0x040000CF RID: 207
		public static bool[] fragment5 = new bool[]
		{
			false, false, false, true, false, false, false, true, false, false,
			false, false, false, false, false, false, false, false, false, false,
			false, false, false, false, false, false, false, false, false, false,
			false, false, false, false, false, false, false, false, false, false,
			true, true, true, false, false, false, true, false
		};

		// Token: 0x040000D0 RID: 208
		public static bool[] nullfragment = new bool[]
		{
			false, true, true, true, true, true, true, true, false, true,
			true, true, true, true, false, true, false, true, false, true,
			true, true, false, true, true, true, false, true, false, true,
			false, true, false, true, true, true, true, true, false, true,
			true, true, true, true, true, true, false, true
		};

		// Token: 0x040000D1 RID: 209
		private static bool[] voiceHeader = new bool[]
		{
			false, true, false, false, false, true, true, false, true, false,
			false, false, true, false, true, false, false, false, false, false,
			false, true, false, true, true, false, true, true, false, false,
			false, true, false, false, true, false, true, true, true, false,
			false, false, false, false, true, true, true, false, false, false,
			false, false, false, false, true, false, true, true, false, true,
			true, false, false, false, false, false, true, true, true, false,
			false, false, false, false, true, true, false, false, false, false,
			false, true, false, false, false, true, true, false, true, false,
			false, false, false, false, false, true, true, true, false, false,
			false, true, false, false, false, true, true, true, true, true,
			false, true, false, true, false, true, true, true, false, true,
			false, true, true, true, true, true, true, true, false, true,
			true, true, false, true, true, true, true, true, true, true,
			true, true, false, true, false, true, true, true, false, true,
			false, true, false, true, true, true, true, true, true, false,
			false, false, true, true, false, false, true, false, false, false,
			true, true, true, true, false, false, false, false, true, true,
			true, false, false, false, true, true, false, true, false, false,
			false, false, false, false, false, false, false, true, false, true,
			true, true, true, true, false, false, false, false, false, false,
			true, false, true, true, false, false, false, true, true, false,
			false, false, false, true, false, false, true, true, true, true,
			true, true, true, false, false, true, false, false, true, false,
			false, false, false, true, true, false, true, false, true, false,
			false, false, false, true, true, true, true, false, false, true,
			false, false, false, true
		};

		// Token: 0x040000D2 RID: 210
		private static byte[] totalVoice33 = new byte[33];

		// Token: 0x040000D3 RID: 211
		private static bool[] totalVoiceBool = new bool[264];

		// Token: 0x040000D4 RID: 212
		private static int fragmentCounter = 1;

		// Token: 0x040000D5 RID: 213
		private static bool beeped = false;

		// Token: 0x040000D6 RID: 214
		private static WebSocketService davidjoo;

		// Token: 0x040000D7 RID: 215
		private static WebSocketServer wssv;
	}
}
