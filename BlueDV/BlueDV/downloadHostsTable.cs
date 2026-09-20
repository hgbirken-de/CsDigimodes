using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BlueDV
{
	// Token: 0x02000022 RID: 34
	internal class downloadHostsTable
	{
		// Token: 0x060001C2 RID: 450 RVA: 0x00010FD4 File Offset: 0x0000F1D4
		private static void parseREFHostFile()
		{
			try
			{
				StreamReader streamReader;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					streamReader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\DPlus_Hosts.txt");
				}
				else
				{
					streamReader = File.OpenText("DPlus_Hosts.txt");
				}
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					if (!text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2)
					{
						string[] array = text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
						string text2 = array[0];
						string text3 = array[1];
						try
						{
							downloadHostsTable.REF.Add(text2, text3);
						}
						catch (ArgumentException)
						{
						}
					}
				}
				streamReader.Close();
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("HOSTFILE MISSING!!");
			}
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x00011094 File Offset: 0x0000F294
		public static string searchREF(string reflector)
		{
			if (downloadHostsTable.REF.Count < 1)
			{
				downloadHostsTable.parseREFHostFile();
			}
			string text;
			if (downloadHostsTable.REF.TryGetValue(reflector.Trim(), out text))
			{
				return text;
			}
			return null;
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x000110CA File Offset: 0x0000F2CA
		public static Dictionary<string, string> returnREF()
		{
			if (downloadHostsTable.REF.Count < 1)
			{
				downloadHostsTable.parseREFHostFile();
			}
			return downloadHostsTable.REF;
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x000110E4 File Offset: 0x0000F2E4
		private static void parseDCSHostFile()
		{
			try
			{
				StreamReader streamReader;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					streamReader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\DCS_Hosts.txt");
				}
				else
				{
					streamReader = File.OpenText("DCS_Hosts.txt");
				}
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					if (!text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2)
					{
						string[] array = text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
						string text2 = array[0];
						string text3 = array[1];
						try
						{
							downloadHostsTable.DCS.Add(text2, text3);
						}
						catch (ArgumentException)
						{
						}
					}
				}
				streamReader.Close();
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("HOSTFILE MISSING!!");
			}
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x000111A4 File Offset: 0x0000F3A4
		public static string searchDCS(string reflector)
		{
			if (downloadHostsTable.DCS.Count < 1)
			{
				downloadHostsTable.parseDCSHostFile();
			}
			string text;
			if (downloadHostsTable.DCS.TryGetValue(reflector.Trim(), out text))
			{
				return text;
			}
			return null;
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x000111DA File Offset: 0x0000F3DA
		public static Dictionary<string, string> returnDCS()
		{
			if (downloadHostsTable.DCS.Count < 1)
			{
				downloadHostsTable.parseDCSHostFile();
			}
			return downloadHostsTable.DCS;
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x000111F4 File Offset: 0x0000F3F4
		private static void parseXRFHostFile()
		{
			try
			{
				StreamReader streamReader;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					streamReader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\DExtra_Hosts.txt");
				}
				else
				{
					streamReader = File.OpenText("DExtra_Hosts.txt");
				}
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					if (!text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2)
					{
						string[] array = text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
						string text2 = array[0];
						string text3 = array[1];
						try
						{
							downloadHostsTable.XRF.Add(text2, text3);
						}
						catch (ArgumentException)
						{
						}
					}
				}
				streamReader.Close();
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("XRF HOSTFILE MISSING!!");
			}
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x000112B4 File Offset: 0x0000F4B4
		private static void parseJPNHostFile()
		{
			try
			{
				StreamReader streamReader;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					streamReader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\JPN_Hosts.txt");
				}
				else
				{
					streamReader = File.OpenText("JPN_Hosts.txt");
				}
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					if (!text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2)
					{
						string[] array = text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
						string text2 = array[0];
						string text3 = array[1];
						try
						{
							downloadHostsTable.JPN.Add(text2, text3);
						}
						catch (ArgumentException)
						{
						}
					}
				}
				streamReader.Close();
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("JPN HOSTFILE MISSING!!");
			}
		}

		// Token: 0x060001CA RID: 458 RVA: 0x00011374 File Offset: 0x0000F574
		public static string searchJPN(string reflector)
		{
			if (downloadHostsTable.JPN.Count < 1)
			{
				downloadHostsTable.parseJPNHostFile();
			}
			string text;
			if (downloadHostsTable.JPN.TryGetValue(reflector.Trim(), out text))
			{
				return text;
			}
			return null;
		}

		// Token: 0x060001CB RID: 459 RVA: 0x000113AA File Offset: 0x0000F5AA
		public static Dictionary<string, string> returnJPN()
		{
			if (downloadHostsTable.JPN.Count < 1)
			{
				downloadHostsTable.parseJPNHostFile();
			}
			return downloadHostsTable.JPN;
		}

		// Token: 0x060001CC RID: 460 RVA: 0x000113C4 File Offset: 0x0000F5C4
		public static void parseXLXDSTARHostFile()
		{
			try
			{
				StreamReader streamReader;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					streamReader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\XLX_Hosts.txt");
				}
				else
				{
					streamReader = File.OpenText("XLX_Hosts.txt");
				}
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					if (!text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2)
					{
						string[] array = text.Split(new char[] { '@' });
						string text2 = array[0];
						string text3 = array[1];
						string text4 = array[2];
						try
						{
							downloadHostsTable.XLXDSTAR.Add(text2, text3);
							downloadHostsTable.XLXDSTARURL.Add(text2, text4);
						}
						catch (ArgumentException)
						{
						}
					}
				}
				streamReader.Close();
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("XRF HOSTFILE MISSING!!");
			}
		}

		// Token: 0x060001CD RID: 461 RVA: 0x00011498 File Offset: 0x0000F698
		public static string searchXLX(string reflector)
		{
			if (downloadHostsTable.XLXDSTAR.Count < 1)
			{
				downloadHostsTable.parseXLXDSTARHostFile();
			}
			string text;
			if (downloadHostsTable.XLXDSTAR.TryGetValue(reflector.Trim(), out text))
			{
				return text;
			}
			return null;
		}

		// Token: 0x060001CE RID: 462 RVA: 0x000114D0 File Offset: 0x0000F6D0
		public static string searchXRF(string reflector)
		{
			if (downloadHostsTable.XRF.Count < 1)
			{
				downloadHostsTable.parseXRFHostFile();
			}
			string text;
			if (downloadHostsTable.XRF.TryGetValue(reflector.Trim(), out text))
			{
				return text;
			}
			return null;
		}

		// Token: 0x060001CF RID: 463 RVA: 0x00011506 File Offset: 0x0000F706
		public static Dictionary<string, string> returnXRF()
		{
			if (downloadHostsTable.XRF.Count < 1)
			{
				downloadHostsTable.parseXRFHostFile();
			}
			return downloadHostsTable.XRF;
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x0001151F File Offset: 0x0000F71F
		public static Dictionary<string, string> returnXLXDSTAR()
		{
			if (downloadHostsTable.XLXDSTAR.Count < 1)
			{
				downloadHostsTable.parseXLXDSTARHostFile();
			}
			return downloadHostsTable.XLXDSTAR;
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x00011538 File Offset: 0x0000F738
		public static Dictionary<string, string> returnXLXDSTARURL()
		{
			if (downloadHostsTable.XLXDSTARURL.Count < 1)
			{
				downloadHostsTable.parseXLXDSTARHostFile();
			}
			return downloadHostsTable.XLXDSTARURL;
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x00011554 File Offset: 0x0000F754
		private static void parseXLXDMRHostFile()
		{
			try
			{
				StreamReader streamReader;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					streamReader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\xlx_DMRMaster.txt");
				}
				else
				{
					streamReader = File.OpenText("xlx_DMRMaster.txt");
				}
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					if (!text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2)
					{
						string[] array = text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
						string text2 = array[0];
						string text3 = array[1];
						try
						{
							downloadHostsTable.XLXDMR.Add(text2, text3);
						}
						catch (ArgumentException)
						{
						}
					}
				}
				streamReader.Close();
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("HOSTFILE MISSING!!");
			}
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x00011614 File Offset: 0x0000F814
		public static string searchXLXDMR(string reflector)
		{
			if (downloadHostsTable.XLXDMR.Count < 1)
			{
				downloadHostsTable.parseXLXDMRHostFile();
			}
			string text;
			if (downloadHostsTable.XLXDMR.TryGetValue(reflector.Trim(), out text))
			{
				return text;
			}
			return null;
		}

		// Token: 0x060001D4 RID: 468 RVA: 0x0001164A File Offset: 0x0000F84A
		public static Dictionary<string, string> returnXLXDMR()
		{
			if (downloadHostsTable.XLXDMR.Count < 1)
			{
				downloadHostsTable.parseXLXDMRHostFile();
			}
			return downloadHostsTable.XLXDMR;
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x00011664 File Offset: 0x0000F864
		private static void parseFreeDMRHostFile()
		{
			try
			{
				StreamReader streamReader;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					streamReader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\FreeDMR_Hosts.txt");
				}
				else
				{
					streamReader = File.OpenText("FreeDMR_Hosts.txt");
				}
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					if (!text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2 && !text.Contains("DUP") && !text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2 && !text.Contains("DUP"))
					{
						string[] array = text.Split(new char[] { ',' });
						string text2 = array[0].Replace("_DMO", "");
						string text3 = array[2];
						string text4 = array[3];
						int num = int.Parse(array[4]);
						try
						{
							downloadHostsTable.FREEDMR.Add(text2, new FreeDMRStruct
							{
								country = text2,
								hostname = text3,
								password = text4,
								port = num
							});
						}
						catch (ArgumentException)
						{
						}
					}
				}
				streamReader.Close();
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("HOSTFILE MISSING!!");
			}
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x000117D4 File Offset: 0x0000F9D4
		public static FreeDMRStruct searchFreeDMR(string reflector)
		{
			if (downloadHostsTable.FREEDMR.Count < 1)
			{
				downloadHostsTable.parseFreeDMRHostFile();
			}
			FreeDMRStruct freeDMRStruct;
			if (downloadHostsTable.FREEDMR.TryGetValue(reflector.Trim(), out freeDMRStruct))
			{
				return freeDMRStruct;
			}
			return null;
		}

		// Token: 0x060001D7 RID: 471 RVA: 0x0001180A File Offset: 0x0000FA0A
		public static Dictionary<string, FreeDMRStruct> returnFreeDMR()
		{
			if (downloadHostsTable.FREEDMR.Count < 1)
			{
				downloadHostsTable.parseFreeDMRHostFile();
			}
			return downloadHostsTable.FREEDMR;
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x00011824 File Offset: 0x0000FA24
		private static void parseSystemXHostFile()
		{
			try
			{
				StreamReader streamReader;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					streamReader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\SystemX_Hosts.txt");
				}
				else
				{
					streamReader = File.OpenText("SystemX_Hosts.txt");
				}
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					if (!text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2 && !text.Contains("DUP"))
					{
						string[] array = text.Split(new char[] { ',' });
						string text2 = array[0].Replace("_DMO", "");
						string text3 = array[1];
						string text4 = array[2];
						int num = int.Parse(array[3]);
						try
						{
							downloadHostsTable.SYSTEMX.Add(text2, new SystemXStruct
							{
								country = text2,
								hostname = text3,
								password = text4,
								port = num
							});
						}
						catch (ArgumentException)
						{
						}
					}
				}
				streamReader.Close();
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("HOSTFILE MISSING!!");
			}
		}

		// Token: 0x060001D9 RID: 473 RVA: 0x00011940 File Offset: 0x0000FB40
		public static SystemXStruct searchSystemX(string reflector)
		{
			if (downloadHostsTable.SYSTEMX.Count < 1)
			{
				downloadHostsTable.parseSystemXHostFile();
			}
			SystemXStruct systemXStruct;
			if (downloadHostsTable.SYSTEMX.TryGetValue(reflector.Trim(), out systemXStruct))
			{
				return systemXStruct;
			}
			return null;
		}

		// Token: 0x060001DA RID: 474 RVA: 0x00011976 File Offset: 0x0000FB76
		public static Dictionary<string, SystemXStruct> returnSystemX()
		{
			if (downloadHostsTable.SYSTEMX.Count < 1)
			{
				downloadHostsTable.parseSystemXHostFile();
			}
			return downloadHostsTable.SYSTEMX;
		}

		// Token: 0x060001DB RID: 475 RVA: 0x00011990 File Offset: 0x0000FB90
		public static void parseTGIFHostFile()
		{
			downloadHostsTable.TGIF.Clear();
			downloadHostsTable.TGIF.Add("TGIF", new TGIFStruct
			{
				country = "TGIF",
				hostname = "tgif.network",
				password = information.myTGIFPassword,
				port = 62031
			});
		}

		// Token: 0x060001DC RID: 476 RVA: 0x000119E8 File Offset: 0x0000FBE8
		public static TGIFStruct searchTGIF(string reflector)
		{
			if (downloadHostsTable.TGIF.Count == 0)
			{
				downloadHostsTable.parseTGIFHostFile();
			}
			TGIFStruct tgifstruct;
			if (downloadHostsTable.TGIF.TryGetValue(reflector.Trim(), out tgifstruct))
			{
				return tgifstruct;
			}
			return null;
		}

		// Token: 0x060001DD RID: 477 RVA: 0x00011A1D File Offset: 0x0000FC1D
		public static Dictionary<string, TGIFStruct> returnTGIF()
		{
			if (downloadHostsTable.TGIF.Count == 0)
			{
				downloadHostsTable.parseTGIFHostFile();
			}
			return downloadHostsTable.TGIF;
		}

		// Token: 0x060001DE RID: 478 RVA: 0x00011A38 File Offset: 0x0000FC38
		private static void parseADNSYSTEMSHostFile()
		{
			try
			{
				StreamReader streamReader;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					streamReader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\ADNSYSTEMS_Hosts.txt");
				}
				else
				{
					streamReader = File.OpenText("ADNSYSTEMS_Hosts.txt");
				}
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					if (!text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2 && !text.Contains("DUP"))
					{
						string[] array = text.Split(new char[] { ',' });
						string text2 = array[0].Replace("_DMO", "");
						string text3 = array[2];
						string text4 = array[3];
						int num = int.Parse(array[4]);
						try
						{
							downloadHostsTable.ADNSYSTEMS.Add(text2, new ADNSYSTEMSStruct
							{
								country = text2,
								hostname = text3,
								password = text4,
								port = num
							});
						}
						catch (ArgumentException)
						{
						}
					}
				}
				streamReader.Close();
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("ADNSYSTEMS HOSTFILE MISSING!!");
			}
		}

		// Token: 0x060001DF RID: 479 RVA: 0x00011B54 File Offset: 0x0000FD54
		public static ADNSYSTEMSStruct searchADNSYSTEMS(string reflector)
		{
			if (downloadHostsTable.ADNSYSTEMS.Count < 1)
			{
				downloadHostsTable.parseADNSYSTEMSHostFile();
			}
			ADNSYSTEMSStruct adnsystemsstruct;
			if (downloadHostsTable.ADNSYSTEMS.TryGetValue(reflector.Trim(), out adnsystemsstruct))
			{
				return adnsystemsstruct;
			}
			return null;
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x00011B8A File Offset: 0x0000FD8A
		public static Dictionary<string, ADNSYSTEMSStruct> returnADNSYSTEMS()
		{
			if (downloadHostsTable.ADNSYSTEMS.Count < 1)
			{
				downloadHostsTable.parseADNSYSTEMSHostFile();
			}
			return downloadHostsTable.ADNSYSTEMS;
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x00011BA4 File Offset: 0x0000FDA4
		private static void parseFCSHostFile()
		{
			try
			{
				StreamReader streamReader;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					streamReader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\fcs_masters.txt");
				}
				else
				{
					streamReader = File.OpenText("fcs_masters.txt");
				}
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					if (!text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2)
					{
						string[] array = text.Split(new char[] { ',' });
						string text2 = array[0];
						string text3 = array[1];
						try
						{
							downloadHostsTable.FCS.Add(text2, text3);
						}
						catch (ArgumentException)
						{
						}
					}
				}
				streamReader.Close();
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("fcs.txt HOSTFILE MISSING!!");
			}
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x00011C68 File Offset: 0x0000FE68
		public static string searchFCS(string reflector)
		{
			if (downloadHostsTable.FCS.Count < 1)
			{
				downloadHostsTable.parseFCSHostFile();
			}
			string text;
			if (downloadHostsTable.FCS.TryGetValue(reflector.Trim(), out text))
			{
				return text;
			}
			return null;
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x00011C9E File Offset: 0x0000FE9E
		public static Dictionary<string, string> returnFCS()
		{
			if (downloadHostsTable.FCS.Count < 1)
			{
				downloadHostsTable.parseFCSHostFile();
			}
			return downloadHostsTable.FCS;
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x00011CB8 File Offset: 0x0000FEB8
		private static void parseXLXYSFHostFile()
		{
			try
			{
				StreamReader streamReader;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					streamReader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\XLXYSF_Hosts.txt");
				}
				else
				{
					streamReader = File.OpenText("XLXYSF_Hosts.txt");
				}
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					if (!text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2)
					{
						string[] array = text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
						string text2 = array[0];
						string text3 = array[1];
						try
						{
							downloadHostsTable.XLXYSF.Add(text2, text3);
						}
						catch (ArgumentException)
						{
						}
					}
				}
				streamReader.Close();
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("HOSTFILE MISSING!!");
			}
		}

		// Token: 0x060001E5 RID: 485 RVA: 0x00011D78 File Offset: 0x0000FF78
		public static string searchXLXYSF(string reflector)
		{
			if (downloadHostsTable.XLXYSF.Count < 1)
			{
				downloadHostsTable.parseXLXYSFHostFile();
			}
			string text;
			if (downloadHostsTable.XLXYSF.TryGetValue(reflector.Trim(), out text))
			{
				return text;
			}
			return null;
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x00011DAE File Offset: 0x0000FFAE
		public static List<string> returnXLXYSF()
		{
			if (downloadHostsTable.XLXYSF.Count < 1)
			{
				downloadHostsTable.parseXLXYSFHostFile();
			}
			return downloadHostsTable.XLXYSF.Keys.ToList<string>();
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x00011DD4 File Offset: 0x0000FFD4
		private static void parseNXDNHostFile()
		{
			try
			{
				StreamReader streamReader;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					streamReader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\NXDNHosts.txt");
				}
				else
				{
					streamReader = File.OpenText("NXDNHosts.txt");
				}
				downloadHostsTable.NXDN.Clear();
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					if (!text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2 && !text.StartsWith(" ") && !text.StartsWith("#") && text.Length > 2)
					{
						string[] array = text.Split(null, StringSplitOptions.RemoveEmptyEntries);
						string text2 = array[0];
						string text3 = array[1];
						int num = int.Parse(array[2]);
						try
						{
							downloadHostsTable.NXDN.Add(Convert.ToString(text2), new NXDNStruct
							{
								ID = text2,
								hostname = text3,
								port = num
							});
						}
						catch (ArgumentException)
						{
						}
					}
				}
				streamReader.Close();
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("NXDN HOSTFILE MISSING!!");
			}
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x00011EEC File Offset: 0x000100EC
		public static NXDNStruct searchNXDN(string reflector)
		{
			if (downloadHostsTable.NXDN.Count < 1)
			{
				downloadHostsTable.parseNXDNHostFile();
			}
			NXDNStruct nxdnstruct;
			if (downloadHostsTable.NXDN.TryGetValue(reflector, out nxdnstruct))
			{
				return nxdnstruct;
			}
			return null;
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x00011F1D File Offset: 0x0001011D
		public static Dictionary<string, NXDNStruct> returnNXDN()
		{
			if (downloadHostsTable.NXDN.Count < 1)
			{
				downloadHostsTable.parseNXDNHostFile();
			}
			return downloadHostsTable.NXDN;
		}

		// Token: 0x040000EF RID: 239
		private static Dictionary<string, string> REF = new Dictionary<string, string>();

		// Token: 0x040000F0 RID: 240
		private static Dictionary<string, string> DCS = new Dictionary<string, string>();

		// Token: 0x040000F1 RID: 241
		private static Dictionary<string, string> XRF = new Dictionary<string, string>();

		// Token: 0x040000F2 RID: 242
		private static Dictionary<string, string> JPN = new Dictionary<string, string>();

		// Token: 0x040000F3 RID: 243
		private static Dictionary<string, string> XLXDSTAR = new Dictionary<string, string>();

		// Token: 0x040000F4 RID: 244
		private static Dictionary<string, string> XLXDSTARURL = new Dictionary<string, string>();

		// Token: 0x040000F5 RID: 245
		private static Dictionary<string, string> XLXDMR = new Dictionary<string, string>();

		// Token: 0x040000F6 RID: 246
		private static Dictionary<string, FreeDMRStruct> FREEDMR = new Dictionary<string, FreeDMRStruct>();

		// Token: 0x040000F7 RID: 247
		private static Dictionary<string, SystemXStruct> SYSTEMX = new Dictionary<string, SystemXStruct>();

		// Token: 0x040000F8 RID: 248
		private static Dictionary<string, TGIFStruct> TGIF = new Dictionary<string, TGIFStruct>();

		// Token: 0x040000F9 RID: 249
		private static Dictionary<string, ADNSYSTEMSStruct> ADNSYSTEMS = new Dictionary<string, ADNSYSTEMSStruct>();

		// Token: 0x040000FA RID: 250
		private static Dictionary<string, NXDNStruct> NXDN = new Dictionary<string, NXDNStruct>();

		// Token: 0x040000FB RID: 251
		private static Dictionary<string, string> FCS = new Dictionary<string, string>();

		// Token: 0x040000FC RID: 252
		private static Dictionary<string, string> XLXYSF = new Dictionary<string, string>();
	}
}
