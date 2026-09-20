using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BlueDV
{
	// Token: 0x0200002E RID: 46
	internal class MMDVM_HS_TIMER
	{
		// Token: 0x06000374 RID: 884 RVA: 0x0002674C File Offset: 0x0002494C
		public static void Start()
		{
			MMDVM_HS_TIMER.running = true;
			ThreadStart threadStart;
			if ((threadStart = MMDVM_HS_TIMER.<>O.<0>__david) == null)
			{
				threadStart = (MMDVM_HS_TIMER.<>O.<0>__david = new ThreadStart(MMDVM_HS_TIMER.david));
			}
			new Thread(threadStart).Start();
		}

		// Token: 0x06000375 RID: 885 RVA: 0x0002677C File Offset: 0x0002497C
		private static void david()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			while (MMDVM_HS_TIMER.running)
			{
				if (stopwatch.ElapsedTicks > (long)MMDVM_HS_TIMER.TIME_INTERVAL_IN_MILLISECONDS)
				{
					DVMEGASerial.getDVMEGAStatus();
					stopwatch.Restart();
				}
				Thread.Sleep(1);
			}
		}

		// Token: 0x06000376 RID: 886 RVA: 0x000267B7 File Offset: 0x000249B7
		private static void timer_Tick(object state)
		{
			if (DVMEGASerial.isOpen())
			{
				DVMEGASerial.getDVMEGAStatus();
			}
			if (MMDVM_HS_TIMER.timer != null)
			{
				MMDVM_HS_TIMER.timer.Change(MMDVM_HS_TIMER.TIME_INTERVAL_IN_MILLISECONDS, -1);
			}
		}

		// Token: 0x06000377 RID: 887 RVA: 0x000267DD File Offset: 0x000249DD
		public static void Stop()
		{
			MMDVM_HS_TIMER.running = false;
		}

		// Token: 0x04000253 RID: 595
		private static Timer timer;

		// Token: 0x04000254 RID: 596
		private static int TIME_INTERVAL_IN_MILLISECONDS = 1500;

		// Token: 0x04000255 RID: 597
		private static bool running = false;

		// Token: 0x0200007B RID: 123
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040004FC RID: 1276
			public static ThreadStart <0>__david;
		}
	}
}
