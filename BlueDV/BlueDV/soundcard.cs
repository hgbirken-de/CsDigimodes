using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAudio.Dsp;
using NAudio.Wave;
using STA.Settings;
using WebSocketSharp.Server;

namespace BlueDV
{
	// Token: 0x02000039 RID: 57
	internal class soundcard
	{
		// Token: 0x1700003D RID: 61
		// (get) Token: 0x06000404 RID: 1028 RVA: 0x00029F28 File Offset: 0x00028128
		// (set) Token: 0x06000405 RID: 1029 RVA: 0x00029F2F File Offset: 0x0002812F
		public static int StatusVU { get; private set; }

		// Token: 0x1400001D RID: 29
		// (add) Token: 0x06000406 RID: 1030 RVA: 0x00029F38 File Offset: 0x00028138
		// (remove) Token: 0x06000407 RID: 1031 RVA: 0x00029F6C File Offset: 0x0002816C
		public static event EventHandler StatusVUChanged;

		// Token: 0x06000408 RID: 1032 RVA: 0x00029F9F File Offset: 0x0002819F
		public static void startWebSocket()
		{
			soundcard.wssv = new WebSocketServer(int.Parse(information.webVoicePort), false);
			soundcard.wssv.AddWebSocketService<WebSocketService>("/");
			soundcard.wssv.Start();
		}

		// Token: 0x06000409 RID: 1033 RVA: 0x00029FCF File Offset: 0x000281CF
		public static void stopWebSocket()
		{
			if (soundcard.wssv != null)
			{
				soundcard.wssv.Stop();
				soundcard.wssv = null;
			}
		}

		// Token: 0x0600040A RID: 1034 RVA: 0x00029FE8 File Offset: 0x000281E8
		public static void start()
		{
			soundcard.waveFormat = new WaveFormat(8000, 16, 1);
			soundcard.waveProvider = new BufferedWaveProvider(soundcard.waveFormat);
			soundcard.waveOut = new WaveOut();
			try
			{
				soundcard.waveOut.DeviceNumber = soundcard.playDevice;
				soundcard.waveOut.Init(soundcard.waveProvider);
				soundcard.waveOut.Play();
			}
			catch (Exception ex)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					MessageBox.Show(ex.Message, "Error on soundcard", MessageBoxButtons.OK);
					break;
				case information.LANGUAGE.JAPANESE:
					MessageBox.Show(ex.Message, "サウンドカードのエラー", MessageBoxButtons.OK);
					break;
				case information.LANGUAGE.CHINEES:
					MessageBox.Show(ex.Message, "Error on soundcard", MessageBoxButtons.OK);
					break;
				case information.LANGUAGE.KOREAN:
					MessageBox.Show(ex.Message, "사운드카드 에러", MessageBoxButtons.OK);
					break;
				}
			}
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x0002A0CC File Offset: 0x000282CC
		public static void resetBuffer()
		{
			soundcard.waveProvider.ClearBuffer();
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x0002A0D8 File Offset: 0x000282D8
		public static void stop()
		{
			if (soundcard.waveOut != null)
			{
				soundcard.waveOut.Stop();
				soundcard.waveOut.Dispose();
				soundcard.waveOut = null;
			}
			if (soundcard.sourceStream != null)
			{
				soundcard.sourceStream.StopRecording();
				soundcard.sourceStream.Dispose();
				soundcard.sourceStream = null;
			}
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x0002A128 File Offset: 0x00028328
		public static void play(byte[] voice)
		{
			byte[] array = new byte[320];
			for (int i = 0; i < 320; i += 2)
			{
				array[i] = voice[i + 1];
				array[i + 1] = voice[i];
			}
			try
			{
				soundcard.waveProvider.AddSamples(array, 0, array.Length);
			}
			catch (Exception)
			{
			}
			if (soundcard.waveOut.PlaybackState == PlaybackState.Stopped)
			{
				soundcard.waveOut.Play();
			}
			if (information.enableWEB && information.m_device == information.DEVICE.DV3000R)
			{
				soundcard.wssv.WebSocketServices.Broadcast(array);
			}
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x0002A1BC File Offset: 0x000283BC
		public static void volumeOut(float volume)
		{
			soundcard.waveOut.Volume = volume / 100f;
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x0002A1CF File Offset: 0x000283CF
		public static int getVolume()
		{
			return (int)soundcard.waveOut.Volume * 100;
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x0002A1DF File Offset: 0x000283DF
		public static void force()
		{
			soundcard.waveOut.Play();
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x0002A1EB File Offset: 0x000283EB
		public static void recordNow(bool recording)
		{
			soundcard.RECORD = recording;
			if (!information.onAIRswitchBool)
			{
				if (recording)
				{
					soundcard.record();
					return;
				}
				soundcard.stoprecording();
			}
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x0002A208 File Offset: 0x00028408
		public static void record()
		{
			if (soundcard.sourceStream != null)
			{
				return;
			}
			try
			{
				soundcard.sourceStream = new WaveIn();
				soundcard.sourceStream.DeviceNumber = soundcard.recordDevice;
				soundcard.sourceStream.BufferMilliseconds = 100;
				soundcard.sourceStream.NumberOfBuffers = 3;
				soundcard.sourceStream.WaveFormat = new WaveFormat(8000, 16, 1);
				soundcard.sourceStream.DataAvailable += soundcard.sourceStream_DataAvailable;
				soundcard.sourceStream.StartRecording();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error on soundcard", MessageBoxButtons.OK);
			}
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x0002A2AC File Offset: 0x000284AC
		public static void stoprecording()
		{
			if (soundcard.sourceStream != null)
			{
				soundcard.sourceStream.StopRecording();
				soundcard.sourceStream.Dispose();
				soundcard.sourceStream = null;
				DVMEGAAMBE.endFrameTrick();
			}
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x0002A2D4 File Offset: 0x000284D4
		private static void sourceStream_DataAvailable(object sender, WaveInEventArgs e)
		{
			for (int i = 0; i < 1600; i += 320)
			{
				byte[] array = new byte[320];
				new short[160];
				byte[] array2 = new byte[320];
				Buffer.BlockCopy(e.Buffer, i, array, 0, 320);
				for (int j = 0; j < 320; j += 2)
				{
					AMBE_VOX.detectVoice((short)(((int)array[j + 1] << 8) | (int)array[j]));
					array2[j + 1] = array[j];
					array2[j] = array[j + 1];
				}
				if (soundcard.RECORD)
				{
					information.AMBETYPE ambetype = information.m_ambetype;
					if (ambetype != information.AMBETYPE.AMBE3000)
					{
						if (ambetype == information.AMBETYPE.AMBE3003)
						{
							DVMEGAAMBE.PCMtoAMBEMMDVM3003(array2);
						}
					}
					else
					{
						DVMEGAAMBE.PCMtoAMBEMMDVM3000(array2);
					}
				}
			}
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x0002A388 File Offset: 0x00028588
		public static int recVolume()
		{
			return (int)soundcard.RECVOLUME;
		}

		// Token: 0x06000416 RID: 1046 RVA: 0x0002A38F File Offset: 0x0002858F
		public static int playVolume()
		{
			return (int)soundcard.PLAYVOLUME;
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x0002A398 File Offset: 0x00028598
		public static double CalculateGoertzel(short[] samples, double frequency, int sampleRate)
		{
			double num = Math.Round(frequency * (double)samples.Length / (double)sampleRate);
			double num2 = 6.283185307179586 / (double)samples.Length * num;
			double num3 = Math.Cos(num2);
			Math.Sin(num2);
			double num4 = 2.0 * num3;
			double num5 = 0.0;
			double num6 = 0.0;
			foreach (short num7 in samples)
			{
				double num8 = num4 * num5 - num6 + (double)num7;
				num6 = num5;
				num5 = num8;
			}
			return Math.Sqrt(num5 * num5 + num6 * num6 - num5 * num6 * num4);
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x0002A434 File Offset: 0x00028634
		private static void DTMF(short[] buffer)
		{
			var list = new double[] { 697.0, 770.0, 852.0, 941.0, 1209.0, 1336.0, 1477.0 }.Select((double f) => new
			{
				Frequency = f,
				Magnitude = soundcard.CalculateGoertzel(buffer, f, 8000)
			}).ToList();
			var list2 = list.OrderByDescending(result => result.Magnitude).Take(2).ToList();
			if (list2.All(result => result.Magnitude > 10.0))
			{
				string text = soundcard.phoneKeyOf[(int)list2[0].Frequency][(int)list2[1].Frequency];
				string[] array = new string[5];
				array[0] = "s: [";
				array[1] = text;
				array[2] = "] key; mags = { ";
				array[3] = list.Select(result => result.Magnitude.ToString("00.000")).Aggregate((string result1, string result2) => result1 + " ; " + result2);
				array[4] = " }";
				Console.WriteLine(string.Concat(array));
			}
		}

		// Token: 0x06000419 RID: 1049 RVA: 0x0002A574 File Offset: 0x00028774
		private static byte[] lowPassie(byte[] data)
		{
			byte[] array = new byte[data.Length];
			for (int i = 0; i < data.Length; i += 2)
			{
				float num = (float)((short)((int)data[i] | ((int)data[i + 1] << 8))) / 32767f;
				short num2 = (short)(BiQuadFilter.LowPassFilter(8000f, 3000f, 1f).Transform(num) * 32767f);
				byte[] array2 = new byte[2];
				array2 = BitConverter.GetBytes(num2);
				array[i] = array2[0];
				array[i + 1] = array2[1];
			}
			return array;
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x0002A5EC File Offset: 0x000287EC
		public static List<string> listPlayDevices()
		{
			int deviceCount = WaveOut.DeviceCount;
			List<string> list = new List<string>();
			for (int i = 0; i < deviceCount; i++)
			{
				list.Add(WaveOut.GetCapabilities(i).ProductName);
			}
			return list;
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x0002A628 File Offset: 0x00028828
		public static List<string> listRecordingDevices()
		{
			int deviceCount = WaveIn.DeviceCount;
			List<string> list = new List<string>();
			for (int i = 0; i < deviceCount; i++)
			{
				list.Add(WaveIn.GetCapabilities(i).ProductName);
			}
			return list;
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x0002A664 File Offset: 0x00028864
		public static void selectRecordingDevice(int card)
		{
			if (soundcard.sourceStream == null)
			{
				soundcard.recordDevice = card;
			}
			else
			{
				soundcard.recordDevice = card;
				soundcard.sourceStream.DeviceNumber = card;
			}
			soundcard.configFile.SetValue("GENERAL", "SoundCardIn", card.ToString());
			soundcard.configFile.Flush();
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x0002A6B8 File Offset: 0x000288B8
		public static void selectPlayDevice(int card)
		{
			if (soundcard.waveOut == null)
			{
				soundcard.playDevice = card;
			}
			else
			{
				soundcard.playDevice = card;
				soundcard.waveOut.DeviceNumber = card;
			}
			soundcard.configFile.SetValue("GENERAL", "SoundCardOut", card.ToString());
			soundcard.configFile.Flush();
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x0002A70A File Offset: 0x0002890A
		public static int selectedPlayDevice()
		{
			return int.Parse(soundcard.configFile.GetValue("GENERAL", "SoundCardOut", "-1"));
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x0002A72A File Offset: 0x0002892A
		public static int selectedRecordingDevice()
		{
			return int.Parse(soundcard.configFile.GetValue("GENERAL", "SoundCardIn", "-1"));
		}

		// Token: 0x06000420 RID: 1056 RVA: 0x0002A74C File Offset: 0x0002894C
		public static void beep(double frequency)
		{
			if (information.AMBEBeep)
			{
				int num = 8000;
				short[] array = new short[600];
				double num2 = 8191.75;
				int num3 = 0;
				byte[] array2 = new byte[array.Length * 2];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = (short)(num2 * Math.Sin(6.283185307179586 * (double)i * frequency / (double)num));
					byte[] bytes = BitConverter.GetBytes(array[i]);
					array2[num3] = bytes[0];
					array2[num3 + 1] = bytes[1];
					num3 += 2;
				}
				try
				{
					soundcard.waveProvider.AddSamples(array2, 0, array2.Length);
					if (soundcard.waveOut != null && soundcard.waveOut.PlaybackState == PlaybackState.Stopped)
					{
						soundcard.waveOut.Play();
					}
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x06000421 RID: 1057 RVA: 0x0002A824 File Offset: 0x00028A24
		private static void ChangeVUMeter(int vu)
		{
			soundcard.StatusVU = vu;
			EventHandler statusVUChanged = soundcard.StatusVUChanged;
			if (statusVUChanged != null)
			{
				statusVUChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x040002A7 RID: 679
		private static WaveIn sourceStream = null;

		// Token: 0x040002A8 RID: 680
		private static WaveOut waveOut = null;

		// Token: 0x040002A9 RID: 681
		private static WaveFormat waveFormat = null;

		// Token: 0x040002AA RID: 682
		private static BufferedWaveProvider waveProvider = null;

		// Token: 0x040002AB RID: 683
		private static int recordDevice = -1;

		// Token: 0x040002AC RID: 684
		private static int playDevice = -1;

		// Token: 0x040002AD RID: 685
		private const float UPSAMPLE = 1f;

		// Token: 0x040002AE RID: 686
		private static short RECVOLUME = 0;

		// Token: 0x040002AF RID: 687
		private static short PLAYVOLUME = 0;

		// Token: 0x040002B0 RID: 688
		private static bool RECORD = false;

		// Token: 0x040002B3 RID: 691
		public static double vol = 0.0;

		// Token: 0x040002B4 RID: 692
		private static INIFile configFile = new INIFile(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\BlueDVconfig.ini");

		// Token: 0x040002B5 RID: 693
		private static WebSocketServer wssv;

		// Token: 0x040002B6 RID: 694
		private static readonly Dictionary<int, Dictionary<int, string>> phoneKeyOf = new Dictionary<int, Dictionary<int, string>>
		{
			{
				1209,
				new Dictionary<int, string>
				{
					{ 1477, "?" },
					{ 1336, "?" },
					{ 1209, "?" },
					{ 941, "*" },
					{ 852, "7" },
					{ 770, "4" },
					{ 697, "1" }
				}
			},
			{
				1336,
				new Dictionary<int, string>
				{
					{ 1477, "?" },
					{ 1336, "?" },
					{ 1209, "?" },
					{ 941, "0" },
					{ 852, "8" },
					{ 770, "5" },
					{ 697, "2" }
				}
			},
			{
				1477,
				new Dictionary<int, string>
				{
					{ 1477, "?" },
					{ 1336, "?" },
					{ 1209, "?" },
					{ 941, "#" },
					{ 852, "9" },
					{ 770, "6" },
					{ 697, "3" }
				}
			},
			{
				941,
				new Dictionary<int, string>
				{
					{ 1477, "#" },
					{ 1336, "0" },
					{ 1209, "*" },
					{ 941, "?" },
					{ 852, "?" },
					{ 770, "?" },
					{ 697, "?" }
				}
			},
			{
				852,
				new Dictionary<int, string>
				{
					{ 1477, "9" },
					{ 1336, "8" },
					{ 1209, "7" },
					{ 941, "?" },
					{ 852, "?" },
					{ 770, "?" },
					{ 697, "?" }
				}
			},
			{
				770,
				new Dictionary<int, string>
				{
					{ 1477, "6" },
					{ 1336, "5" },
					{ 1209, "4" },
					{ 941, "?" },
					{ 852, "?" },
					{ 770, "?" },
					{ 697, "?" }
				}
			},
			{
				697,
				new Dictionary<int, string>
				{
					{ 1477, "3" },
					{ 1336, "2" },
					{ 1209, "1" },
					{ 941, "?" },
					{ 852, "?" },
					{ 770, "?" },
					{ 697, "?" }
				}
			}
		};
	}
}
