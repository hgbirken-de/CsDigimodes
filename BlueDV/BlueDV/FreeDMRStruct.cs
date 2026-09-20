using System;

namespace BlueDV
{
	// Token: 0x0200001D RID: 29
	public class FreeDMRStruct
	{
		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000197 RID: 407 RVA: 0x00010E8E File Offset: 0x0000F08E
		// (set) Token: 0x06000198 RID: 408 RVA: 0x00010E96 File Offset: 0x0000F096
		public string country { get; set; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000199 RID: 409 RVA: 0x00010E9F File Offset: 0x0000F09F
		// (set) Token: 0x0600019A RID: 410 RVA: 0x00010EA7 File Offset: 0x0000F0A7
		public string hostname { get; set; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x0600019B RID: 411 RVA: 0x00010EB0 File Offset: 0x0000F0B0
		// (set) Token: 0x0600019C RID: 412 RVA: 0x00010EB8 File Offset: 0x0000F0B8
		public string password { get; set; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x0600019D RID: 413 RVA: 0x00010EC1 File Offset: 0x0000F0C1
		// (set) Token: 0x0600019E RID: 414 RVA: 0x00010EC9 File Offset: 0x0000F0C9
		public int port { get; set; }
	}
}
