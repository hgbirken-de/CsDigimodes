using System;
using System.Timers;

namespace BlueDV
{
	// Token: 0x0200000B RID: 11
	internal class AMBE_VOX
	{
		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000061 RID: 97 RVA: 0x00003EA5 File Offset: 0x000020A5
		// (set) Token: 0x06000062 RID: 98 RVA: 0x00003EAC File Offset: 0x000020AC
		public static bool StatusBoolAMBE_VOX { get; private set; }

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x06000063 RID: 99 RVA: 0x00003EB4 File Offset: 0x000020B4
		// (remove) Token: 0x06000064 RID: 100 RVA: 0x00003EE8 File Offset: 0x000020E8
		public static event EventHandler StatusBoolChangedAMBE_VOX;

		// Token: 0x06000065 RID: 101 RVA: 0x00003F1C File Offset: 0x0000211C
		public static void start()
		{
			AMBE_VOX.lastPongInMiliseconds = 0L;
			AMBE_VOX.aTimer = new Timer();
			AMBE_VOX.aTimer.Elapsed += AMBE_VOX.ping;
			AMBE_VOX.aTimer.Interval = 100.0;
			AMBE_VOX.aTimer.Enabled = true;
			AMBE_VOX.runnning = true;
			AMBE_VOX.detectedVoice = false;
			AMBE_VOX.lastPongInMiliseconds = 0L;
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00003F84 File Offset: 0x00002184
		private static void ping(object sender, ElapsedEventArgs e)
		{
			if (DateTime.Now.Ticks / 10000L - AMBE_VOX.lastPongInMiliseconds > information.VOXHangTime)
			{
				if (AMBE_VOX.detectedVoice)
				{
					AMBE_VOX.ChangeStatusBoolAMBE_VOX(false);
				}
				AMBE_VOX.detectedVoice = false;
			}
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00003FC5 File Offset: 0x000021C5
		public static void stop()
		{
			if (AMBE_VOX.aTimer != null)
			{
				AMBE_VOX.aTimer.Stop();
				AMBE_VOX.aTimer.Close();
				AMBE_VOX.aTimer.Dispose();
			}
			AMBE_VOX.runnning = false;
			AMBE_VOX.ChangeStatusBoolAMBE_VOX(false);
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00003FF8 File Offset: 0x000021F8
		public static void detectVoice(short voice)
		{
			if (AMBE_VOX.runnning && (int)voice > information.VOXDetectionLevel)
			{
				if (!AMBE_VOX.detectedVoice)
				{
					AMBE_VOX.ChangeStatusBoolAMBE_VOX(true);
				}
				AMBE_VOX.detectedVoice = true;
				AMBE_VOX.lastPongInMiliseconds = DateTime.Now.Ticks / 10000L;
			}
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00004040 File Offset: 0x00002240
		public static void setDetectionLevel(int level)
		{
			information.VOXDetectionLevel = level;
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00004048 File Offset: 0x00002248
		private static void ChangeStatusBoolAMBE_VOX(bool vox)
		{
			AMBE_VOX.StatusBoolAMBE_VOX = vox;
			EventHandler statusBoolChangedAMBE_VOX = AMBE_VOX.StatusBoolChangedAMBE_VOX;
			if (statusBoolChangedAMBE_VOX != null)
			{
				statusBoolChangedAMBE_VOX(null, EventArgs.Empty);
			}
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00004070 File Offset: 0x00002270
		private static void setVOXTimeout(long timeOutMilliseconds)
		{
			information.VOXHangTime = timeOutMilliseconds;
		}

		// Token: 0x04000028 RID: 40
		private static Timer aTimer;

		// Token: 0x04000029 RID: 41
		public static bool runnning;

		// Token: 0x0400002A RID: 42
		public static bool detectedVoice;

		// Token: 0x0400002B RID: 43
		private static long lastPongInMiliseconds;
	}
}
