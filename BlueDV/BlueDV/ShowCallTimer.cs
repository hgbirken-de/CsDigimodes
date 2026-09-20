using System;
using System.Timers;

namespace BlueDV
{
	// Token: 0x02000034 RID: 52
	internal class ShowCallTimer
	{
		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060003D1 RID: 977 RVA: 0x000285BA File Offset: 0x000267BA
		// (set) Token: 0x060003D2 RID: 978 RVA: 0x000285C1 File Offset: 0x000267C1
		public static string StatusModeText { get; private set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060003D3 RID: 979 RVA: 0x000285C9 File Offset: 0x000267C9
		// (set) Token: 0x060003D4 RID: 980 RVA: 0x000285D0 File Offset: 0x000267D0
		public static information.MODUS StatusModeMode { get; private set; }

		// Token: 0x1400001B RID: 27
		// (add) Token: 0x060003D5 RID: 981 RVA: 0x000285D8 File Offset: 0x000267D8
		// (remove) Token: 0x060003D6 RID: 982 RVA: 0x0002860C File Offset: 0x0002680C
		public static event EventHandler StatusModeModeTextChanged;

		// Token: 0x060003D7 RID: 983 RVA: 0x00028640 File Offset: 0x00026840
		public static void startTimer()
		{
			ShowCallTimer.DMRtimer = new Timer((double)ShowCallTimer.hangTimer);
			ShowCallTimer.DMRtimer.Elapsed += ShowCallTimer.OnTimeEventDMR;
			ShowCallTimer.DMRtimer.AutoReset = true;
			ShowCallTimer.DMRtimer.Enabled = true;
			ShowCallTimer.DMRtimer.Interval = (double)ShowCallTimer.hangTimer;
			ShowCallTimer.DMRtimer.Stop();
			ShowCallTimer.DMRtimer.Start();
			ShowCallTimer.DSTARtimer = new Timer((double)(ShowCallTimer.hangTimer + 1));
			ShowCallTimer.DSTARtimer.Elapsed += ShowCallTimer.OnTimeEventDSTAR;
			ShowCallTimer.DSTARtimer.AutoReset = true;
			ShowCallTimer.DSTARtimer.Enabled = true;
			ShowCallTimer.DSTARtimer.Interval = (double)ShowCallTimer.hangTimer;
			ShowCallTimer.DSTARtimer.Stop();
			ShowCallTimer.DSTARtimer.Start();
			ShowCallTimer.FUSIONtimer = new Timer((double)(ShowCallTimer.hangTimer + 2));
			ShowCallTimer.FUSIONtimer.Elapsed += ShowCallTimer.OnTimeEventFUSION;
			ShowCallTimer.FUSIONtimer.AutoReset = true;
			ShowCallTimer.FUSIONtimer.Enabled = true;
			ShowCallTimer.FUSIONtimer.Interval = (double)ShowCallTimer.hangTimer;
			ShowCallTimer.FUSIONtimer.Stop();
			ShowCallTimer.FUSIONtimer.Start();
			ShowCallTimer.NXDNtimer = new Timer((double)(ShowCallTimer.hangTimer + 3));
			ShowCallTimer.NXDNtimer.Elapsed += ShowCallTimer.OnTimeEventNXDN;
			ShowCallTimer.NXDNtimer.AutoReset = true;
			ShowCallTimer.NXDNtimer.Enabled = true;
			ShowCallTimer.NXDNtimer.Interval = (double)ShowCallTimer.hangTimer;
			ShowCallTimer.NXDNtimer.Stop();
			ShowCallTimer.NXDNtimer.Start();
		}

		// Token: 0x060003D8 RID: 984 RVA: 0x000287D3 File Offset: 0x000269D3
		private static void OnTimeEventDMR(object sender, ElapsedEventArgs e)
		{
			if (information.foundDVMEGA)
			{
				ShowCallTimer.ChangeModeText(information.MODUS.DMR, " ");
				ShowCallTimer.lastdmrHisCallName = "";
				ShowCallTimer.DMRtimer.Stop();
			}
		}

		// Token: 0x060003D9 RID: 985 RVA: 0x000287FB File Offset: 0x000269FB
		private static void OnTimeEventDSTAR(object sender, ElapsedEventArgs e)
		{
			if (information.foundDVMEGA)
			{
				ShowCallTimer.ChangeModeText(information.MODUS.DSTAR, " ");
				ShowCallTimer.lastdstarHisCallName = "";
				ShowCallTimer.DSTARtimer.Stop();
			}
		}

		// Token: 0x060003DA RID: 986 RVA: 0x00028823 File Offset: 0x00026A23
		private static void OnTimeEventFUSION(object sender, ElapsedEventArgs e)
		{
			if (information.foundDVMEGA)
			{
				ShowCallTimer.ChangeModeText(information.MODUS.FUSION, " ");
				ShowCallTimer.lastfusionHisCallName = "";
				ShowCallTimer.FUSIONtimer.Stop();
			}
		}

		// Token: 0x060003DB RID: 987 RVA: 0x0002884B File Offset: 0x00026A4B
		private static void OnTimeEventNXDN(object sender, ElapsedEventArgs e)
		{
			if (information.foundDVMEGA)
			{
				ShowCallTimer.ChangeModeText(information.MODUS.NXDN, " ");
				ShowCallTimer.lastnxdnHisCallName = "";
				ShowCallTimer.NXDNtimer.Stop();
			}
		}

		// Token: 0x060003DC RID: 988 RVA: 0x00028874 File Offset: 0x00026A74
		public static void stopTimers()
		{
			if (ShowCallTimer.DMRtimer != null)
			{
				ShowCallTimer.DMRtimer.Close();
				ShowCallTimer.DMRtimer.Dispose();
				ShowCallTimer.DMRtimer = null;
			}
			if (ShowCallTimer.DSTARtimer != null)
			{
				ShowCallTimer.DSTARtimer.Close();
				ShowCallTimer.DSTARtimer.Dispose();
				ShowCallTimer.DSTARtimer = null;
			}
			if (ShowCallTimer.FUSIONtimer != null)
			{
				ShowCallTimer.FUSIONtimer.Close();
				ShowCallTimer.FUSIONtimer.Dispose();
				ShowCallTimer.FUSIONtimer = null;
			}
			if (ShowCallTimer.NXDNtimer != null)
			{
				ShowCallTimer.NXDNtimer.Close();
				ShowCallTimer.NXDNtimer.Dispose();
				ShowCallTimer.NXDNtimer = null;
			}
		}

		// Token: 0x060003DD RID: 989 RVA: 0x00028905 File Offset: 0x00026B05
		public static void resetTimerDMR(string dmrHisCallName, int sessionID)
		{
			ShowCallTimer.DMRtimer.Stop();
			ShowCallTimer.DMRtimer.Start();
			if (!dmrHisCallName.Equals(ShowCallTimer.lastdmrHisCallName))
			{
				if (sessionID != ShowCallTimer.lastSessionsID)
				{
					ShowCallTimer.ChangeModeText(information.MODUS.DMR, dmrHisCallName);
				}
				ShowCallTimer.lastSessionsID = sessionID;
			}
			ShowCallTimer.lastdmrHisCallName = dmrHisCallName;
		}

		// Token: 0x060003DE RID: 990 RVA: 0x00028943 File Offset: 0x00026B43
		public static void resetTimerDSTAR(string dstarHisCallName)
		{
			ShowCallTimer.DSTARtimer.Stop();
			ShowCallTimer.DSTARtimer.Start();
			if (!dstarHisCallName.Equals(ShowCallTimer.lastdstarHisCallName))
			{
				ShowCallTimer.ChangeModeText(information.MODUS.DSTAR, dstarHisCallName);
			}
			ShowCallTimer.lastdstarHisCallName = dstarHisCallName;
		}

		// Token: 0x060003DF RID: 991 RVA: 0x00028973 File Offset: 0x00026B73
		public static void resetTimerFUSION(string fusionHisCallName)
		{
			ShowCallTimer.FUSIONtimer.Stop();
			ShowCallTimer.FUSIONtimer.Start();
			if (!fusionHisCallName.Equals(ShowCallTimer.lastfusionHisCallName))
			{
				ShowCallTimer.ChangeModeText(information.MODUS.FUSION, fusionHisCallName);
			}
			ShowCallTimer.lastfusionHisCallName = fusionHisCallName;
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x000289A3 File Offset: 0x00026BA3
		public static void resetTimerNXDN(string nxdnHisCallName)
		{
			ShowCallTimer.NXDNtimer.Stop();
			ShowCallTimer.NXDNtimer.Start();
			if (!nxdnHisCallName.Equals(ShowCallTimer.lastnxdnHisCallName))
			{
				ShowCallTimer.ChangeModeText(information.MODUS.NXDN, nxdnHisCallName);
			}
			ShowCallTimer.lastnxdnHisCallName = nxdnHisCallName;
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x000289D4 File Offset: 0x00026BD4
		private static void ChangeModeText(information.MODUS mode, string text)
		{
			ShowCallTimer.StatusModeMode = mode;
			ShowCallTimer.StatusModeText = text;
			EventHandler statusModeModeTextChanged = ShowCallTimer.StatusModeModeTextChanged;
			if (statusModeModeTextChanged != null)
			{
				statusModeModeTextChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x04000281 RID: 641
		private static int lastSessionsID = 123;

		// Token: 0x04000282 RID: 642
		private static Timer DMRtimer;

		// Token: 0x04000283 RID: 643
		private static Timer DSTARtimer;

		// Token: 0x04000284 RID: 644
		private static Timer FUSIONtimer;

		// Token: 0x04000285 RID: 645
		private static Timer NXDNtimer;

		// Token: 0x04000286 RID: 646
		private static string lastdmrHisCallName = "";

		// Token: 0x04000287 RID: 647
		private static string lastdstarHisCallName = "";

		// Token: 0x04000288 RID: 648
		private static string lastfusionHisCallName = "";

		// Token: 0x04000289 RID: 649
		private static string lastnxdnHisCallName = "";

		// Token: 0x0400028D RID: 653
		public static int hangTimer = 1000;
	}
}
