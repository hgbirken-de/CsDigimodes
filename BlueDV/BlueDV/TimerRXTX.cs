using System;
using System.Threading;

namespace BlueDV
{
	// Token: 0x0200004C RID: 76
	internal class TimerRXTX
	{
		// Token: 0x170000AD RID: 173
		// (get) Token: 0x060005A4 RID: 1444 RVA: 0x00034A72 File Offset: 0x00032C72
		// (set) Token: 0x060005A5 RID: 1445 RVA: 0x00034A79 File Offset: 0x00032C79
		public static information.QSOSTATUS StatusRXTX { get; private set; }

		// Token: 0x14000020 RID: 32
		// (add) Token: 0x060005A6 RID: 1446 RVA: 0x00034A84 File Offset: 0x00032C84
		// (remove) Token: 0x060005A7 RID: 1447 RVA: 0x00034AB8 File Offset: 0x00032CB8
		public static event EventHandler StatusRXTXChanged;

		// Token: 0x060005A8 RID: 1448 RVA: 0x00034AEC File Offset: 0x00032CEC
		public static void startTimer()
		{
			TimerRXTX.isRunningVar = true;
			TimerRXTX.thread = new Thread(new ThreadStart(TimerRXTX.WorkThreadFunction));
			TimerRXTX.thread.Name = "TimerRXTX";
			TimerRXTX.thread.IsBackground = true;
			TimerRXTX.thread.Start();
		}

		// Token: 0x060005A9 RID: 1449 RVA: 0x00034B3C File Offset: 0x00032D3C
		private static void WorkThreadFunction()
		{
			try
			{
				while (TimerRXTX.isRunningVar)
				{
					long num = DateTime.Now.Ticks / 10000L;
					if (num - TimerRXTX.rxtime < 600L)
					{
						TimerRXTX.status = TimerRXTX.STATUS.RX;
						TimerRXTX.txtime = 0L;
					}
					else if (num - TimerRXTX.txtime < 600L)
					{
						TimerRXTX.status = TimerRXTX.STATUS.TX;
						TimerRXTX.rxtime = 0L;
					}
					else
					{
						TimerRXTX.status = TimerRXTX.STATUS.LISTENING;
						TimerRXTX.rxtime = 0L;
						TimerRXTX.txtime = 0L;
					}
					if (TimerRXTX.status != TimerRXTX.oldstatus)
					{
						switch (TimerRXTX.status)
						{
						case TimerRXTX.STATUS.RX:
							TimerRXTX.ChangeStatusRXTX(information.QSOSTATUS.RX);
							information.m_qsoStatus = information.QSOSTATUS.RX;
							break;
						case TimerRXTX.STATUS.TX:
							TimerRXTX.ChangeStatusRXTX(information.QSOSTATUS.TX);
							information.m_qsoStatus = information.QSOSTATUS.TX;
							break;
						case TimerRXTX.STATUS.LISTENING:
							TimerRXTX.rxtime = 0L;
							TimerRXTX.txtime = 0L;
							TimerRXTX.ChangeStatusRXTX(information.QSOSTATUS.LISTENING);
							information.m_qsoStatus = information.QSOSTATUS.LISTENING;
							break;
						}
						TimerRXTX.oldstatus = TimerRXTX.status;
					}
					try
					{
						Thread.Sleep(20);
					}
					catch (ThreadAbortException)
					{
					}
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060005AA RID: 1450 RVA: 0x00034C54 File Offset: 0x00032E54
		public static bool isRunning()
		{
			return TimerRXTX.isRunningVar;
		}

		// Token: 0x060005AB RID: 1451 RVA: 0x00034C60 File Offset: 0x00032E60
		public static void RX()
		{
			TimerRXTX.rxtime = DateTime.Now.Ticks / 10000L;
		}

		// Token: 0x060005AC RID: 1452 RVA: 0x00034C88 File Offset: 0x00032E88
		public static void TX()
		{
			TimerRXTX.txtime = DateTime.Now.Ticks / 10000L;
		}

		// Token: 0x060005AD RID: 1453 RVA: 0x00034CAE File Offset: 0x00032EAE
		public static void stop()
		{
			TimerRXTX.isRunningVar = false;
			if (TimerRXTX.thread != null)
			{
				TimerRXTX.thread.Abort();
				TimerRXTX.thread = null;
			}
		}

		// Token: 0x060005AE RID: 1454 RVA: 0x00034CD0 File Offset: 0x00032ED0
		private static void ChangeStatusRXTX(information.QSOSTATUS text1)
		{
			TimerRXTX.StatusRXTX = text1;
			EventHandler statusRXTXChanged = TimerRXTX.StatusRXTXChanged;
			if (statusRXTXChanged != null)
			{
				statusRXTXChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x0400044A RID: 1098
		public static TimerRXTX.STATUS status;

		// Token: 0x0400044B RID: 1099
		private static volatile bool isRunningVar = true;

		// Token: 0x0400044C RID: 1100
		private static long rxtime;

		// Token: 0x0400044D RID: 1101
		private static long txtime;

		// Token: 0x0400044E RID: 1102
		private const long TIME_OUT = 600L;

		// Token: 0x0400044F RID: 1103
		private static TimerRXTX.STATUS oldstatus;

		// Token: 0x04000450 RID: 1104
		private static Thread thread;

		// Token: 0x0200009D RID: 157
		public enum STATUS
		{
			// Token: 0x0400057C RID: 1404
			RX,
			// Token: 0x0400057D RID: 1405
			TX,
			// Token: 0x0400057E RID: 1406
			LISTENING
		}
	}
}
