using System;
using System.Timers;

namespace BlueDV
{
	// Token: 0x0200002F RID: 47
	internal class modeTimer
	{
		// Token: 0x17000038 RID: 56
		// (get) Token: 0x0600037A RID: 890 RVA: 0x000267F7 File Offset: 0x000249F7
		// (set) Token: 0x0600037B RID: 891 RVA: 0x000267FE File Offset: 0x000249FE
		public static string StatusModeText { get; private set; }

		// Token: 0x14000019 RID: 25
		// (add) Token: 0x0600037C RID: 892 RVA: 0x00026808 File Offset: 0x00024A08
		// (remove) Token: 0x0600037D RID: 893 RVA: 0x0002683C File Offset: 0x00024A3C
		public static event EventHandler StatusModeTextChanged;

		// Token: 0x0600037E RID: 894 RVA: 0x00026870 File Offset: 0x00024A70
		public static void setMode(information.MODUS mode)
		{
			if (mode != modeTimer.lastmode)
			{
				switch (mode)
				{
				case information.MODUS.IDLE:
					information.stream_modus = information.MODUS.IDLE;
					modeTimer.ChangeModeText("IDLE");
					break;
				case information.MODUS.DMR:
					information.stream_modus = information.MODUS.DMR;
					modeTimer.ChangeModeText("DMR");
					break;
				case information.MODUS.DSTAR:
					information.stream_modus = information.MODUS.DSTAR;
					modeTimer.ChangeModeText("DSTAR");
					break;
				case information.MODUS.FUSION:
					information.stream_modus = information.MODUS.FUSION;
					modeTimer.ChangeModeText("C4FM");
					break;
				case information.MODUS.NXDN:
					information.stream_modus = information.MODUS.NXDN;
					modeTimer.ChangeModeText("NXDN");
					break;
				}
			}
			modeTimer.lastmode = mode;
			if (modeTimer.atimer != null)
			{
				modeTimer.resetTimer();
			}
		}

		// Token: 0x0600037F RID: 895 RVA: 0x0002690C File Offset: 0x00024B0C
		private static int hangTimerRF()
		{
			try
			{
				return int.Parse(information.myModeTimerRF) * 1000;
			}
			catch (FormatException ex)
			{
				Console.WriteLine(ex.Message);
			}
			return 15000;
		}

		// Token: 0x06000380 RID: 896 RVA: 0x00026950 File Offset: 0x00024B50
		private static int hangTimerNet()
		{
			try
			{
				return int.Parse(information.myModeTimerNet) * 1000;
			}
			catch (FormatException ex)
			{
				Console.WriteLine(ex.Message);
			}
			return 15000;
		}

		// Token: 0x06000381 RID: 897 RVA: 0x00026994 File Offset: 0x00024B94
		public static void startTimer()
		{
			modeTimer.atimer = new Timer((double)modeTimer.hangTimerRF());
			modeTimer.atimer.Elapsed += modeTimer.OnTimeEvent;
			modeTimer.atimer.AutoReset = true;
			modeTimer.atimer.Enabled = true;
			modeTimer.atimer.Interval = 1.0;
			modeTimer.atimer.Stop();
			modeTimer.atimer.Start();
		}

		// Token: 0x06000382 RID: 898 RVA: 0x00026A04 File Offset: 0x00024C04
		public static void stopTimer()
		{
			if (modeTimer.atimer != null)
			{
				modeTimer.atimer.Close();
				modeTimer.atimer.Dispose();
				modeTimer.atimer = null;
			}
		}

		// Token: 0x06000383 RID: 899 RVA: 0x00026A27 File Offset: 0x00024C27
		public static void resetTimer()
		{
			modeTimer.atimer.Stop();
			modeTimer.atimer.Interval = (double)modeTimer.hangTimerRF();
			modeTimer.atimer.Start();
		}

		// Token: 0x06000384 RID: 900 RVA: 0x00026A4D File Offset: 0x00024C4D
		private static void OnTimeEvent(object sender, ElapsedEventArgs e)
		{
			if (information.foundDVMEGA)
			{
				DVMEGASerial.setmode2Idle();
				information.stream_modus = information.MODUS.IDLE;
				modeTimer.ChangeModeText("IDLE");
				modeTimer.atimer.Stop();
			}
		}

		// Token: 0x06000385 RID: 901 RVA: 0x00026A78 File Offset: 0x00024C78
		private static void ChangeModeText(string text)
		{
			modeTimer.StatusModeText = text;
			EventHandler statusModeTextChanged = modeTimer.StatusModeTextChanged;
			if (statusModeTextChanged != null)
			{
				statusModeTextChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x04000256 RID: 598
		private static Timer atimer;

		// Token: 0x04000257 RID: 599
		private static information.MODUS lastmode;
	}
}
