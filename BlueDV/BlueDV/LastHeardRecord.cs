using System;
using System.IO;
using System.Text;

namespace BlueDV
{
	// Token: 0x02000046 RID: 70
	internal class LastHeardRecord
	{
		// Token: 0x06000556 RID: 1366 RVA: 0x0002DFEC File Offset: 0x0002C1EC
		public static void WriteToFile(string text)
		{
			object obj = LastHeardRecord.locker;
			lock (obj)
			{
				switch (utils.RunningPlatform())
				{
				case utils.Platform.Windows:
				{
					Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "BlueDV"));
					using (FileStream fileStream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\CallLogging.txt", FileMode.Append, FileAccess.Write, FileShare.Read))
					{
						using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.Unicode))
						{
							streamWriter.WriteLine(text.ToString());
							return;
						}
					}
					break;
				}
				case utils.Platform.Linux:
					break;
				case utils.Platform.Mac:
					goto IL_00D5;
				default:
					return;
				}
				using (FileStream fileStream2 = new FileStream("CallLogging.txt", FileMode.Append, FileAccess.Write, FileShare.Read))
				{
					using (StreamWriter streamWriter2 = new StreamWriter(fileStream2, Encoding.Unicode))
					{
						streamWriter2.WriteLine(text.ToString());
						return;
					}
				}
				IL_00D5:
				using (FileStream fileStream3 = new FileStream("CallLogging.txt", FileMode.Append, FileAccess.Write, FileShare.Read))
				{
					using (StreamWriter streamWriter3 = new StreamWriter(fileStream3, Encoding.Unicode))
					{
						streamWriter3.WriteLine(text.ToString());
					}
				}
			}
		}

		// Token: 0x04000375 RID: 885
		private static object locker = new object();
	}
}
