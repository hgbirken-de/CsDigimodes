using System;
using System.IO;
using System.Text;

namespace BlueDV
{
	// Token: 0x0200004B RID: 75
	internal class syslog
	{
		// Token: 0x060005A0 RID: 1440 RVA: 0x0003498C File Offset: 0x00032B8C
		public static void WriteToFile(string text)
		{
			if (syslog.enableSyslog)
			{
				object obj = syslog.locker;
				lock (obj)
				{
					Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "BlueDV"));
					using (FileStream fileStream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\syslog.txt", FileMode.Append, FileAccess.Write, FileShare.Read))
					{
						using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.Unicode))
						{
							streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " | " + text.ToString());
						}
					}
				}
			}
		}

		// Token: 0x04000448 RID: 1096
		private static object locker = new object();

		// Token: 0x04000449 RID: 1097
		public static bool enableSyslog = false;
	}
}
