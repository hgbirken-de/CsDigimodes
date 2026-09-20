using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BlueDV
{
	// Token: 0x02000029 RID: 41
	internal class delayQueue
	{
		// Token: 0x06000311 RID: 785 RVA: 0x00023703 File Offset: 0x00021903
		public static void addQueue(byte[] voice)
		{
			delayQueue.cq.Enqueue(voice);
		}

		// Token: 0x06000312 RID: 786 RVA: 0x00023714 File Offset: 0x00021914
		public static void start()
		{
			delayQueue.cq = new ConcurrentQueue<byte[]>();
			delayQueue.running = true;
			delayQueue.Clear();
			ThreadStart threadStart;
			if ((threadStart = delayQueue.<>O.<0>__queuerRunner) == null)
			{
				threadStart = (delayQueue.<>O.<0>__queuerRunner = new ThreadStart(delayQueue.queuerRunner));
			}
			delayQueue.q = new Thread(threadStart);
			delayQueue.q.Priority = ThreadPriority.Highest;
			delayQueue.q.Start();
			delayQueue.sw = new Stopwatch();
		}

		// Token: 0x06000313 RID: 787 RVA: 0x0002377C File Offset: 0x0002197C
		public static void Clear()
		{
			byte[] array;
			while (delayQueue.cq.TryDequeue(out array))
			{
			}
		}

		// Token: 0x06000314 RID: 788 RVA: 0x00023799 File Offset: 0x00021999
		public static void stop()
		{
			delayQueue.running = false;
			delayQueue.Clear();
			if (delayQueue.q != null)
			{
				delayQueue.q.Abort();
			}
			delayQueue.sw.Stop();
		}

		// Token: 0x06000315 RID: 789 RVA: 0x000237C1 File Offset: 0x000219C1
		public static void resetTimer(int timer_ms)
		{
			delayQueue.TIMER = (long)timer_ms;
			if (delayQueue.sw != null)
			{
				delayQueue.sw = null;
			}
			delayQueue.sw = new Stopwatch();
			delayQueue.sw.Start();
		}

		// Token: 0x06000316 RID: 790 RVA: 0x000237EC File Offset: 0x000219EC
		private static void queuerRunner()
		{
			while (delayQueue.running)
			{
				while (delayQueue.cq.Count<byte[]>() == 0 && delayQueue.running)
				{
					Thread.Sleep(1);
				}
				if (delayQueue.sw.ElapsedMilliseconds > delayQueue.TIMER && delayQueue.cq.TryDequeue(out delayQueue.outQUEUE))
				{
					information.AMBETYPE ambetype = information.m_ambetype;
					if (ambetype != information.AMBETYPE.AMBE3000)
					{
						if (ambetype == information.AMBETYPE.AMBE3003)
						{
							DVMEGAAMBE.PCMtoAMBEMMDVM3003(delayQueue.outQUEUE);
						}
					}
					else
					{
						DVMEGAAMBE.PCMtoAMBEMMDVM3000(delayQueue.outQUEUE);
					}
				}
			}
		}

		// Token: 0x0400021A RID: 538
		private static volatile ConcurrentQueue<byte[]> cq = new ConcurrentQueue<byte[]>();

		// Token: 0x0400021B RID: 539
		private static bool running = true;

		// Token: 0x0400021C RID: 540
		private static byte[] outQUEUE;

		// Token: 0x0400021D RID: 541
		private static Thread q;

		// Token: 0x0400021E RID: 542
		private static int statusCounter = 0;

		// Token: 0x0400021F RID: 543
		private static Stopwatch sw;

		// Token: 0x04000220 RID: 544
		private static long TIMER = 0L;

		// Token: 0x0200007A RID: 122
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040004FB RID: 1275
			public static ThreadStart <0>__queuerRunner;
		}
	}
}
