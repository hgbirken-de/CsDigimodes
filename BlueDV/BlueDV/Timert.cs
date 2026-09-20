using System;

namespace BlueDV
{
	// Token: 0x0200004D RID: 77
	internal class Timert
	{
		// Token: 0x060005B0 RID: 1456 RVA: 0x00034D04 File Offset: 0x00032F04
		public Timert(int ticksPerSec, int secs, int msecs)
		{
			this.m_ticksPerSec = ticksPerSec;
			this.m_timer = 0;
			this.m_timeout = 0;
			if (secs > 0 || msecs > 0)
			{
				long num = ((long)secs * 1000L + (long)msecs) * (long)this.m_ticksPerSec;
				this.m_timeout = (int)(num / 1000L + 1L);
			}
		}

		// Token: 0x060005B1 RID: 1457 RVA: 0x00034D5B File Offset: 0x00032F5B
		public bool isRunning()
		{
			return this.m_timer > 0;
		}

		// Token: 0x060005B2 RID: 1458 RVA: 0x00034D66 File Offset: 0x00032F66
		public void start(int secs, int msecs)
		{
			this.setTimeout(secs, msecs);
			this.start();
		}

		// Token: 0x060005B3 RID: 1459 RVA: 0x00034D76 File Offset: 0x00032F76
		public void start()
		{
			if (this.m_timeout > 0)
			{
				this.m_timer = 1;
			}
		}

		// Token: 0x060005B4 RID: 1460 RVA: 0x00034D88 File Offset: 0x00032F88
		private void setTimeout(int secs, int msecs)
		{
			if (secs > 0 || msecs > 0)
			{
				long num = (long)((secs * 1000 + msecs) * this.m_ticksPerSec);
				this.m_timeout = (int)(num / 1000L + 1L);
				return;
			}
			this.m_timeout = 0;
			this.m_timer = 0;
		}

		// Token: 0x060005B5 RID: 1461 RVA: 0x00034DD0 File Offset: 0x00032FD0
		public void stop()
		{
			this.m_timer = 0;
		}

		// Token: 0x060005B6 RID: 1462 RVA: 0x00034DD9 File Offset: 0x00032FD9
		public bool hasExpired()
		{
			return this.m_timeout != 0 && this.m_timer != 0 && this.m_timer >= this.m_timeout;
		}

		// Token: 0x060005B7 RID: 1463 RVA: 0x00034DFE File Offset: 0x00032FFE
		public void clock(int ticks)
		{
			if (this.m_timer > 0 && this.m_timeout > 0)
			{
				this.m_timer += ticks;
			}
		}

		// Token: 0x04000453 RID: 1107
		private int m_timer;

		// Token: 0x04000454 RID: 1108
		private int m_timeout;

		// Token: 0x04000455 RID: 1109
		private int m_ticksPerSec;
	}
}
