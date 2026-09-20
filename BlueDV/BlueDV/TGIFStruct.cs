using System;

namespace BlueDV
{
	// Token: 0x0200001F RID: 31
	public class TGIFStruct
	{
		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060001A9 RID: 425 RVA: 0x00010F16 File Offset: 0x0000F116
		// (set) Token: 0x060001AA RID: 426 RVA: 0x00010F1E File Offset: 0x0000F11E
		public string country { get; set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060001AB RID: 427 RVA: 0x00010F27 File Offset: 0x0000F127
		// (set) Token: 0x060001AC RID: 428 RVA: 0x00010F2F File Offset: 0x0000F12F
		public string hostname { get; set; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060001AD RID: 429 RVA: 0x00010F38 File Offset: 0x0000F138
		// (set) Token: 0x060001AE RID: 430 RVA: 0x00010F40 File Offset: 0x0000F140
		public string password { get; set; }

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060001AF RID: 431 RVA: 0x00010F49 File Offset: 0x0000F149
		// (set) Token: 0x060001B0 RID: 432 RVA: 0x00010F51 File Offset: 0x0000F151
		public int port { get; set; }
	}
}
