using System;
using System.Windows.Forms;

namespace BlueDVAMBEServer
{
	// Token: 0x02000005 RID: 5
	internal static class Program
	{
		// Token: 0x06000017 RID: 23 RVA: 0x0000302C File Offset: 0x0000122C
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}
