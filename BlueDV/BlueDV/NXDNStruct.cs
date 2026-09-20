using System;

namespace BlueDV
{
	// Token: 0x02000021 RID: 33
	public class NXDNStruct
	{
		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060001BB RID: 443 RVA: 0x00010F9E File Offset: 0x0000F19E
		// (set) Token: 0x060001BC RID: 444 RVA: 0x00010FA6 File Offset: 0x0000F1A6
		public string ID { get; set; }

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060001BD RID: 445 RVA: 0x00010FAF File Offset: 0x0000F1AF
		// (set) Token: 0x060001BE RID: 446 RVA: 0x00010FB7 File Offset: 0x0000F1B7
		public string hostname { get; set; }

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060001BF RID: 447 RVA: 0x00010FC0 File Offset: 0x0000F1C0
		// (set) Token: 0x060001C0 RID: 448 RVA: 0x00010FC8 File Offset: 0x0000F1C8
		public int port { get; set; }
	}
}
