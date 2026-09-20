using System;
using System.Windows.Forms;

namespace BlueDV
{
	// Token: 0x0200002C RID: 44
	internal class killTimer
	{
		// Token: 0x17000035 RID: 53
		// (get) Token: 0x0600035D RID: 861 RVA: 0x0002640B File Offset: 0x0002460B
		// (set) Token: 0x0600035E RID: 862 RVA: 0x00026412 File Offset: 0x00024612
		public static bool StatusBoolean { get; private set; }

		// Token: 0x14000017 RID: 23
		// (add) Token: 0x0600035F RID: 863 RVA: 0x0002641C File Offset: 0x0002461C
		// (remove) Token: 0x06000360 RID: 864 RVA: 0x00026450 File Offset: 0x00024650
		public static event EventHandler StatusPTTChanged;

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000361 RID: 865 RVA: 0x00026483 File Offset: 0x00024683
		// (set) Token: 0x06000362 RID: 866 RVA: 0x0002648A File Offset: 0x0002468A
		public static string StatusTimer { get; private set; }

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000363 RID: 867 RVA: 0x00026492 File Offset: 0x00024692
		// (set) Token: 0x06000364 RID: 868 RVA: 0x00026499 File Offset: 0x00024699
		public static long totalSeconds { get; private set; }

		// Token: 0x14000018 RID: 24
		// (add) Token: 0x06000365 RID: 869 RVA: 0x000264A4 File Offset: 0x000246A4
		// (remove) Token: 0x06000366 RID: 870 RVA: 0x000264D8 File Offset: 0x000246D8
		public static event EventHandler StatusPTTTimerChanged;

		// Token: 0x06000367 RID: 871 RVA: 0x0002650C File Offset: 0x0002470C
		public static void start()
		{
			if (killTimer.myTimer == null)
			{
				killTimer.timeLeft = 0L;
				killTimer.myTimer = new Timer();
				killTimer.myTimer.Tick += killTimer.TimerEventProcessor;
				killTimer.myTimer.Interval = 1000;
				killTimer.myTimer.Start();
			}
		}

		// Token: 0x06000368 RID: 872 RVA: 0x00026560 File Offset: 0x00024760
		private static void ChangePTTStatus(bool swit)
		{
			killTimer.StatusBoolean = swit;
			EventHandler statusPTTChanged = killTimer.StatusPTTChanged;
			if (statusPTTChanged != null)
			{
				statusPTTChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000369 RID: 873 RVA: 0x00026588 File Offset: 0x00024788
		private static void ChangePTTTimerStatus(string timert, long t)
		{
			killTimer.StatusTimer = timert;
			killTimer.totalSeconds = t;
			EventHandler statusPTTTimerChanged = killTimer.StatusPTTTimerChanged;
			if (statusPTTTimerChanged != null)
			{
				statusPTTTimerChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x0600036A RID: 874 RVA: 0x000265B8 File Offset: 0x000247B8
		private static void TimerEventProcessor(object myObject, EventArgs myEventArgs)
		{
			if (killTimer.timeLeft < (long)information.killswitch)
			{
				killTimer.timeLeft += 1L;
				killTimer.running = true;
				TimeSpan timeSpan = TimeSpan.FromSeconds((double)killTimer.timeLeft);
				long num = (long)timeSpan.TotalSeconds;
				killTimer.ChangePTTTimerStatus(string.Format("{0:D2}:{1:D2}:{2:D2}", new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds }), num);
				return;
			}
			killTimer.myTimer.Stop();
			killTimer.running = false;
			killTimer.ChangePTTStatus(false);
			killTimer.myTimer = null;
			killTimer.ChangePTTTimerStatus("", 0L);
		}

		// Token: 0x0600036B RID: 875 RVA: 0x00026679 File Offset: 0x00024879
		public static void stop()
		{
			if (killTimer.myTimer != null)
			{
				killTimer.myTimer.Stop();
				killTimer.myTimer.Dispose();
				killTimer.running = false;
				killTimer.myTimer = null;
				killTimer.ChangePTTTimerStatus("", 0L);
			}
		}

		// Token: 0x04000249 RID: 585
		private static long timeLeft;

		// Token: 0x0400024A RID: 586
		private static Timer myTimer;

		// Token: 0x0400024B RID: 587
		public static volatile bool running;
	}
}
