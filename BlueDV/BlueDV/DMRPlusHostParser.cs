using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BlueDV
{
	// Token: 0x02000016 RID: 22
	internal class DMRPlusHostParser
	{
		// Token: 0x06000126 RID: 294 RVA: 0x0000C0B4 File Offset: 0x0000A2B4
		public static List<string> returnTotalList()
		{
			List<string> list = DMRPlusHostParser.downloadHosts2();
			DMRPlusHostParser.downloadHostsIPCS();
			DMRPlusHostParser.getHBLinkList();
			foreach (string text in list)
			{
			}
			return DMRPlusHostParser.termsList;
		}

		// Token: 0x06000127 RID: 295 RVA: 0x0000C110 File Offset: 0x0000A310
		public static List<string> downloadHosts2()
		{
			List<string> list = new List<string>();
			DMRPlusHostParser.totalList.Clear();
			DMRPlusHostParser.termsList.Clear();
			List<string> list2;
			try
			{
				ServicePointManager.Expect100Continue = true;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(DMRPlusHostParser.urlIPCS);
				httpWebRequest.Timeout = 7000;
				httpWebRequest.Method = "GET";
				DMRPlusHostParser.response = httpWebRequest.GetResponse();
				DMRPlusHostParser.reader = new StreamReader(DMRPlusHostParser.response.GetResponseStream(), Encoding.UTF8);
				DMRPlusHostParser.result = DMRPlusHostParser.reader.ReadToEnd();
				foreach (string text in Regex.Split(DMRPlusHostParser.result, "END\n"))
				{
					if (text.Trim().Length > 10)
					{
						string[] array2 = text.Split("@".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						if (array2.Length > 1)
						{
							DMRPlusHostParser.termsList.Add(array2[1]);
							DMRPlusHostParser.totalList.Add(new string[]
							{
								array2[0],
								array2[1],
								"passw0rd",
								"55555"
							});
							list = DMRPlusHostParser.termsList;
						}
					}
				}
				list2 = list;
			}
			catch (Exception ex)
			{
				MessageBox.Show("DMR+ hosts issue : " + ex.Message);
				list2 = null;
			}
			finally
			{
				if (DMRPlusHostParser.reader != null)
				{
					DMRPlusHostParser.reader.Close();
				}
				if (DMRPlusHostParser.response != null)
				{
					DMRPlusHostParser.response.Close();
				}
			}
			return list2;
		}

		// Token: 0x06000128 RID: 296 RVA: 0x0000C2A8 File Offset: 0x0000A4A8
		public static List<string> downloadHostsIPCS()
		{
			List<string> list = new List<string>();
			List<string> list2;
			try
			{
				ServicePointManager.Expect100Continue = true;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(DMRPlusHostParser.urlIPCS);
				httpWebRequest.Timeout = 7000;
				httpWebRequest.Method = "GET";
				DMRPlusHostParser.responseIPCS = httpWebRequest.GetResponse();
				DMRPlusHostParser.readerIPCS = new StreamReader(DMRPlusHostParser.responseIPCS.GetResponseStream(), Encoding.UTF8);
				DMRPlusHostParser.resultIPCS = DMRPlusHostParser.readerIPCS.ReadToEnd();
				foreach (string text in Regex.Split(DMRPlusHostParser.resultIPCS, "\n"))
				{
					if (text.Trim().Length > 10)
					{
						string[] array2 = text.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						if (array2.Length > 1)
						{
							DMRPlusHostParser.termsList.Add(array2[0]);
							DMRPlusHostParser.totalList.Add(new string[]
							{
								array2[2],
								array2[0],
								"passw0rd",
								"55555"
							});
							list = DMRPlusHostParser.termsList;
						}
					}
				}
				list2 = list;
			}
			catch (Exception ex)
			{
				MessageBox.Show("DMR+ IPCS hosts issue, contact DMR plus team : " + ex.Message);
				list2 = null;
			}
			finally
			{
				if (DMRPlusHostParser.readerIPCS != null)
				{
					DMRPlusHostParser.readerIPCS.Close();
				}
				if (DMRPlusHostParser.responseIPCS != null)
				{
					DMRPlusHostParser.responseIPCS.Close();
				}
			}
			return list2;
		}

		// Token: 0x06000129 RID: 297 RVA: 0x0000C42C File Offset: 0x0000A62C
		public static List<string> getHBLinkList()
		{
			List<string> list = new List<string>();
			try
			{
				string text;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					text = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\HBLink.txt";
				}
				else
				{
					text = "HBLink.txt";
				}
				using (FileStream fileStream = File.Open(text, FileMode.Open, FileAccess.Read))
				{
					using (BufferedStream bufferedStream = new BufferedStream(fileStream))
					{
						using (StreamReader streamReader = new StreamReader(bufferedStream))
						{
							string text2;
							while ((text2 = streamReader.ReadLine()) != null)
							{
								if (!text2.StartsWith("#"))
								{
									string[] array = Regex.Split(text2, ",");
									DMRPlusHostParser.termsList.Add(array[0].Trim());
									DMRPlusHostParser.totalList.Add(new string[]
									{
										array[1].Trim(),
										array[0].Trim(),
										array[2].Trim(),
										array[3].Trim()
									});
									list = DMRPlusHostParser.termsList;
								}
							}
						}
					}
				}
			}
			catch (IndexOutOfRangeException)
			{
			}
			catch (Exception)
			{
			}
			return list;
		}

		// Token: 0x0600012A RID: 298 RVA: 0x0000C56C File Offset: 0x0000A76C
		public static string lookupHostname(int selectedIndex)
		{
			return DMRPlusHostParser.totalList[selectedIndex][0];
		}

		// Token: 0x0600012B RID: 299 RVA: 0x0000C57C File Offset: 0x0000A77C
		public static string lookupLogicName(string ipAddress)
		{
			foreach (string[] array in DMRPlusHostParser.totalList)
			{
				if (array[0].Equals(ipAddress))
				{
					return array[1];
				}
			}
			return " ";
		}

		// Token: 0x0600012C RID: 300 RVA: 0x0000C5E0 File Offset: 0x0000A7E0
		public static string lookupDMRPort(string ipAddress)
		{
			foreach (string[] array in DMRPlusHostParser.totalList)
			{
				if (array[0].Equals(ipAddress))
				{
					return array[3];
				}
			}
			return "55555";
		}

		// Token: 0x0600012D RID: 301 RVA: 0x0000C644 File Offset: 0x0000A844
		public static string lookupDMRPassword(string ipAddress)
		{
			foreach (string[] array in DMRPlusHostParser.totalList)
			{
				if (array[0].Equals(ipAddress))
				{
					return array[2];
				}
			}
			return "passw0rd";
		}

		// Token: 0x0600012E RID: 302 RVA: 0x0000C6A8 File Offset: 0x0000A8A8
		public static int searchHost(string ipAddress)
		{
			int num = 0;
			using (List<string[]>.Enumerator enumerator = DMRPlusHostParser.totalList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current[0].Equals(ipAddress))
					{
						return num;
					}
					num++;
				}
			}
			return num;
		}

		// Token: 0x0400009E RID: 158
		private static string result = null;

		// Token: 0x0400009F RID: 159
		private static string resultIPCS = null;

		// Token: 0x040000A0 RID: 160
		private static string urlIPCS = "http://bmaster-eu.xreflector.net/bmaster+/ipsc_masters.txt";

		// Token: 0x040000A1 RID: 161
		private static WebResponse response = null;

		// Token: 0x040000A2 RID: 162
		private static WebResponse responseIPCS = null;

		// Token: 0x040000A3 RID: 163
		private static StreamReader reader = null;

		// Token: 0x040000A4 RID: 164
		private static StreamReader readerIPCS = null;

		// Token: 0x040000A5 RID: 165
		private static List<string[]> totalList = new List<string[]>();

		// Token: 0x040000A6 RID: 166
		private static List<string> termsList = new List<string>();
	}
}
