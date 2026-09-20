using System;
using System.Windows.Forms;

namespace BlueDV
{
	// Token: 0x0200004A RID: 74
	internal static class Program
	{
		// Token: 0x0600059F RID: 1439 RVA: 0x00034972 File Offset: 0x00032B72
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}
