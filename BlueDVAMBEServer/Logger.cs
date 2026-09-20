using System;
using System.IO;

namespace BlueDVAMBEServer
{
	// Token: 0x02000004 RID: 4
	internal class Logger
	{
		// Token: 0x06000013 RID: 19 RVA: 0x00002F54 File Offset: 0x00001154
		public static void Log(string logMessage)
		{
			bool logging = information.logging;
			try
			{
				string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "AMBEServerLog.txt");
				Logger.WriteToLog(logMessage, text);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002F98 File Offset: 0x00001198
		private static void WriteToLog(string logMessage, string logFilePath)
		{
			if (information.logging)
			{
				object locker = Logger._locker;
				lock (locker)
				{
					DateTime now = DateTime.Now;
					File.AppendAllText(logFilePath, string.Format("Logged on: {1} at: {2}{0}Message: {3}{0}--------------------{0}", new object[]
					{
						Environment.NewLine,
						now.ToString("MM/dd/yyyy hh:mm:ss.fff"),
						DateTime.Now.ToLongTimeString(),
						logMessage
					}));
				}
			}
		}

		// Token: 0x04000019 RID: 25
		private static readonly object _locker = new object();
	}
}
