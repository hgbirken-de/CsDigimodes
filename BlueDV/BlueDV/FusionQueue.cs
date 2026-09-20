using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BlueDV
{
	// Token: 0x02000040 RID: 64
	internal class FusionQueue
	{
		// Token: 0x0600043D RID: 1085 RVA: 0x0002B36C File Offset: 0x0002956C
		public static void addQueue(byte[] voice)
		{
			ConcurrentQueue<byte[]> concurrentQueue = FusionQueue.cq;
			lock (concurrentQueue)
			{
				FusionQueue.cq.Enqueue(voice);
				Monitor.Pulse(FusionQueue.cq);
			}
		}

		// Token: 0x0600043E RID: 1086 RVA: 0x0002B3C0 File Offset: 0x000295C0
		public static void start()
		{
			FusionQueue.running = true;
			FusionQueue.Clear();
			ThreadStart threadStart;
			if ((threadStart = FusionQueue.<>O.<0>__queuerRunner) == null)
			{
				threadStart = (FusionQueue.<>O.<0>__queuerRunner = new ThreadStart(FusionQueue.queuerRunner));
			}
			FusionQueue.q = new Thread(threadStart);
			FusionQueue.q.Priority = ThreadPriority.Highest;
			FusionQueue.q.Start();
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x0002B414 File Offset: 0x00029614
		public static void Clear()
		{
			byte[] array;
			while (FusionQueue.cq.TryDequeue(out array))
			{
			}
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x0002B434 File Offset: 0x00029634
		public static void stop()
		{
			FusionQueue.running = false;
			FusionQueue.Clear();
			ConcurrentQueue<byte[]> concurrentQueue = FusionQueue.cq;
			lock (concurrentQueue)
			{
				Monitor.Pulse(FusionQueue.cq);
			}
			if (FusionQueue.q != null)
			{
				FusionQueue.q.Abort();
			}
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x0002B498 File Offset: 0x00029698
		private static void queuerRunner()
		{
			while (FusionQueue.running)
			{
				ConcurrentQueue<byte[]> concurrentQueue = FusionQueue.cq;
				lock (concurrentQueue)
				{
					while (FusionQueue.cq.Count == 0 && FusionQueue.running)
					{
						Monitor.Wait(FusionQueue.cq);
					}
					if (FusionQueue.cq.TryDequeue(out FusionQueue.outQUEUE))
					{
						productSelector.write(FusionQueue.outQUEUE);
						switch (information.stream_modus)
						{
						case information.MODUS.DMR:
							Thread.Sleep(10);
							break;
						case information.MODUS.DSTAR:
							if (FusionQueue.statusCounter > 10)
							{
								DVMEGASerial.getDVMEGAStatus();
								FusionQueue.statusCounter = 0;
							}
							if (information.DVMEGAbufferDSTAR == 0)
							{
								FusionQueue.Clear();
							}
							if (information.DVMEGAbufferDSTAR <= 16)
							{
								Thread.Sleep(22);
							}
							break;
						case information.MODUS.FUSION:
							if (FusionQueue.statusCounter > 2)
							{
								DVMEGASerial.getDVMEGAStatus();
								FusionQueue.statusCounter = 0;
							}
							if (information.DVMEGAbufferFUSION <= 2)
							{
								Thread.Sleep(130);
								TimerRXTX.TX();
							}
							else
							{
								TimerRXTX.TX();
							}
							break;
						}
						FusionQueue.statusCounter++;
					}
				}
			}
		}

		// Token: 0x040002C3 RID: 707
		private static volatile ConcurrentQueue<byte[]> cq = new ConcurrentQueue<byte[]>();

		// Token: 0x040002C4 RID: 708
		private static bool running = true;

		// Token: 0x040002C5 RID: 709
		private static byte[] outQUEUE;

		// Token: 0x040002C6 RID: 710
		private static Thread q;

		// Token: 0x040002C7 RID: 711
		private static int statusCounter = 0;

		// Token: 0x02000083 RID: 131
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400050F RID: 1295
			public static ThreadStart <0>__queuerRunner;
		}
	}
}
