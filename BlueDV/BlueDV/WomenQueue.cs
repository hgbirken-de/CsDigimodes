using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BlueDV
{
	// Token: 0x0200003F RID: 63
	internal class WomenQueue
	{
		// Token: 0x06000436 RID: 1078 RVA: 0x0002B0B4 File Offset: 0x000292B4
		public static void addQueue(byte[] voice)
		{
			BlockingCollection<byte[]> blockingCollection = WomenQueue.cq;
			lock (blockingCollection)
			{
				WomenQueue.cq.TryAdd(voice);
				Monitor.Pulse(WomenQueue.cq);
			}
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x0002B104 File Offset: 0x00029304
		public static void start()
		{
			WomenQueue.running = true;
			WomenQueue.Clear();
			ThreadStart threadStart;
			if ((threadStart = WomenQueue.<>O.<0>__queuerRunner) == null)
			{
				threadStart = (WomenQueue.<>O.<0>__queuerRunner = new ThreadStart(WomenQueue.queuerRunner));
			}
			WomenQueue.q = new Thread(threadStart);
			WomenQueue.q.Name = "WomenQueue";
			WomenQueue.q.Priority = ThreadPriority.Highest;
			WomenQueue.q.IsBackground = true;
			WomenQueue.q.Start();
		}

		// Token: 0x06000438 RID: 1080 RVA: 0x0002B170 File Offset: 0x00029370
		public static void Clear()
		{
			byte[] array;
			while (WomenQueue.cq.TryTake(out array))
			{
			}
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x0002B18C File Offset: 0x0002938C
		public static void stop()
		{
			WomenQueue.running = false;
			WomenQueue.Clear();
			BlockingCollection<byte[]> blockingCollection = WomenQueue.cq;
			lock (blockingCollection)
			{
				Monitor.Pulse(WomenQueue.cq);
			}
			if (WomenQueue.q != null)
			{
				WomenQueue.q.Abort();
			}
		}

		// Token: 0x0600043A RID: 1082 RVA: 0x0002B1EC File Offset: 0x000293EC
		private static void queuerRunner()
		{
			while (WomenQueue.running)
			{
				BlockingCollection<byte[]> blockingCollection = WomenQueue.cq;
				lock (blockingCollection)
				{
					while (WomenQueue.cq.Count == 0)
					{
						if (!WomenQueue.running)
						{
							break;
						}
						Monitor.Wait(WomenQueue.cq);
					}
					while (WomenQueue.cq.TryTake(out WomenQueue.outQUEUE))
					{
						long num = (long)Environment.TickCount;
						if (WomenQueue.statusCounter > 10)
						{
							DVMEGASerial.getDVMEGAStatus();
							WomenQueue.statusCounter = 0;
						}
						if (information.m_device == information.DEVICE.DV3000R)
						{
							Stopwatch stopwatch = Stopwatch.StartNew();
							while (stopwatch.ElapsedMilliseconds < 18L)
							{
							}
							TimerRXTX.TX();
							productSelector.write(WomenQueue.outQUEUE);
						}
						else if (information.DVMEGAbufferDSTAR <= 16)
						{
							WomenQueue.last = num;
							Stopwatch stopwatch2 = Stopwatch.StartNew();
							while (stopwatch2.ElapsedMilliseconds < 40L)
							{
								Thread.Sleep(1);
							}
							productSelector.write(WomenQueue.outQUEUE);
						}
						else
						{
							Stopwatch stopwatch3 = Stopwatch.StartNew();
							while (stopwatch3.ElapsedMilliseconds < 5L)
							{
							}
							productSelector.write(WomenQueue.outQUEUE);
							WomenQueue.last = num;
						}
						WomenQueue.statusCounter++;
					}
					if (DCSconnection.isStreaming())
					{
						WomenQueue.Clear();
					}
					if (DPLUSconnection.isStreaming())
					{
						WomenQueue.Clear();
					}
					Task.Delay(10).Wait();
				}
			}
		}

		// Token: 0x040002BD RID: 701
		private static BlockingCollection<byte[]> cq = new BlockingCollection<byte[]>();

		// Token: 0x040002BE RID: 702
		private static bool running = true;

		// Token: 0x040002BF RID: 703
		private static byte[] outQUEUE;

		// Token: 0x040002C0 RID: 704
		private static Thread q;

		// Token: 0x040002C1 RID: 705
		private static long last = 100L;

		// Token: 0x040002C2 RID: 706
		private static int statusCounter = 0;

		// Token: 0x02000082 RID: 130
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400050E RID: 1294
			public static ThreadStart <0>__queuerRunner;
		}
	}
}
