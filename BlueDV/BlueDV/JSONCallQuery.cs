using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;

namespace BlueDV
{
	// Token: 0x02000045 RID: 69
	internal class JSONCallQuery
	{
		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x06000537 RID: 1335 RVA: 0x0002CBD8 File Offset: 0x0002ADD8
		// (set) Token: 0x06000538 RID: 1336 RVA: 0x0002CBDF File Offset: 0x0002ADDF
		public static string StatusText2 { get; private set; }

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x06000539 RID: 1337 RVA: 0x0002CBE7 File Offset: 0x0002ADE7
		// (set) Token: 0x0600053A RID: 1338 RVA: 0x0002CBEE File Offset: 0x0002ADEE
		public static string StatusText3 { get; private set; }

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x0600053B RID: 1339 RVA: 0x0002CBF6 File Offset: 0x0002ADF6
		// (set) Token: 0x0600053C RID: 1340 RVA: 0x0002CBFD File Offset: 0x0002ADFD
		public static string dmrid { get; private set; }

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x0600053D RID: 1341 RVA: 0x0002CC05 File Offset: 0x0002AE05
		// (set) Token: 0x0600053E RID: 1342 RVA: 0x0002CC0C File Offset: 0x0002AE0C
		public static string dmrdest { get; private set; }

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x0600053F RID: 1343 RVA: 0x0002CC14 File Offset: 0x0002AE14
		// (set) Token: 0x06000540 RID: 1344 RVA: 0x0002CC1B File Offset: 0x0002AE1B
		public static string city { get; private set; }

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000541 RID: 1345 RVA: 0x0002CC23 File Offset: 0x0002AE23
		// (set) Token: 0x06000542 RID: 1346 RVA: 0x0002CC2A File Offset: 0x0002AE2A
		public static string country { get; private set; }

		// Token: 0x1400001F RID: 31
		// (add) Token: 0x06000543 RID: 1347 RVA: 0x0002CC34 File Offset: 0x0002AE34
		// (remove) Token: 0x06000544 RID: 1348 RVA: 0x0002CC68 File Offset: 0x0002AE68
		public static event EventHandler StatusHisDMRTextChanged;

		// Token: 0x06000545 RID: 1349 RVA: 0x0002CC9C File Offset: 0x0002AE9C
		public static List<JSONCallQuery.callClass> searchCalls(string search)
		{
			List<JSONCallQuery.callClass> list = new List<JSONCallQuery.callClass>();
			foreach (JSONCallQuery.callClass callClass in JSONCallQuery.callDBlist)
			{
				if (callClass.call.StartsWith(search))
				{
					list.Add(callClass);
				}
			}
			return list;
		}

		// Token: 0x06000546 RID: 1350 RVA: 0x0002CD04 File Offset: 0x0002AF04
		private static void ChangeDMRCallStatusText(string text, string name, string dmrID, string dmrDEST, string City, string Country)
		{
			JSONCallQuery.StatusText2 = text;
			JSONCallQuery.StatusText3 = name;
			JSONCallQuery.city = City;
			JSONCallQuery.country = Country;
			JSONCallQuery.dmrid = dmrID;
			JSONCallQuery.dmrdest = dmrDEST;
			EventHandler statusHisDMRTextChanged = JSONCallQuery.StatusHisDMRTextChanged;
			if (statusHisDMRTextChanged != null)
			{
				statusHisDMRTextChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000547 RID: 1351 RVA: 0x0002CD4C File Offset: 0x0002AF4C
		private static string gett(string url)
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.Timeout = 10000;
			string text;
			try
			{
				using (Stream responseStream = httpWebRequest.GetResponse().GetResponseStream())
				{
					text = new StreamReader(responseStream, Encoding.UTF8).ReadToEnd();
				}
			}
			catch (TimeoutException ex)
			{
				MessageBox.Show(ex.Message, "Error ", MessageBoxButtons.OK);
				text = null;
			}
			catch (WebException)
			{
				text = null;
			}
			return text;
		}

		// Token: 0x06000548 RID: 1352 RVA: 0x0002CDEC File Offset: 0x0002AFEC
		public static List<string> getYSFMasterList()
		{
			if (JSONCallQuery.YSFtermsList != null)
			{
				return JSONCallQuery.YSFtermsList;
			}
			JSONCallQuery.schema2 = null;
			string text = JSONCallQuery.gett("https://hosts.pa7lim.nl/hosts/YSFHosts.json");
			if (string.IsNullOrEmpty(text))
			{
				text = JSONCallQuery.gett("https://hosts.pa7lim.nl/hosts/YSFHosts.json");
			}
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					JSONCallQuery.schema2 = JsonConvert.DeserializeObject(text);
					JSONCallQuery.YSFtermsList = new List<string>();
					if (JSONCallQuery.<>o__40.<>p__2 == null)
					{
						JSONCallQuery.<>o__40.<>p__2 = CallSite<Func<CallSite, object, IEnumerable>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(IEnumerable), typeof(JSONCallQuery)));
					}
					foreach (object obj in JSONCallQuery.<>o__40.<>p__2.Target(JSONCallQuery.<>o__40.<>p__2, JSONCallQuery.schema2))
					{
						List<string> ysftermsList = JSONCallQuery.YSFtermsList;
						if (JSONCallQuery.<>o__40.<>p__1 == null)
						{
							JSONCallQuery.<>o__40.<>p__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(string), typeof(JSONCallQuery)));
						}
						Func<CallSite, object, string> target = JSONCallQuery.<>o__40.<>p__1.Target;
						CallSite <>p__ = JSONCallQuery.<>o__40.<>p__1;
						if (JSONCallQuery.<>o__40.<>p__0 == null)
						{
							JSONCallQuery.<>o__40.<>p__0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Name", typeof(JSONCallQuery), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
						}
						ysftermsList.Add(target(<>p__, JSONCallQuery.<>o__40.<>p__0.Target(JSONCallQuery.<>o__40.<>p__0, obj)));
					}
					return JSONCallQuery.YSFtermsList;
				}
				catch (RuntimeBinderException)
				{
					MessageBox.Show("There is an issue downloading from register.ysfreflector.de");
					JSONCallQuery.YSFtermsList = new List<string>();
					JSONCallQuery.YSFtermsList.Add("NL Central");
					return JSONCallQuery.YSFtermsList;
				}
				catch (Exception)
				{
					MessageBox.Show("There is an issue downloading from register.ysfreflector.de");
					JSONCallQuery.YSFtermsList = new List<string>();
					JSONCallQuery.YSFtermsList.Add("NL Central");
					return JSONCallQuery.YSFtermsList;
				}
			}
			return null;
		}

		// Token: 0x06000549 RID: 1353 RVA: 0x0002D008 File Offset: 0x0002B208
		public static List<string> getMasterList()
		{
			List<string> list3;
			try
			{
				if (!string.IsNullOrEmpty(File.ReadAllText("BMhosts.txt")))
				{
					JSONCallQuery.schema = JsonConvert.DeserializeObject(File.ReadAllText("BMhosts.txt"));
					try
					{
						List<string> list = new List<string>();
						if (JSONCallQuery.<>o__41.<>p__4 == null)
						{
							JSONCallQuery.<>o__41.<>p__4 = CallSite<Func<CallSite, object, IEnumerable>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(IEnumerable), typeof(JSONCallQuery)));
						}
						foreach (object obj in JSONCallQuery.<>o__41.<>p__4.Target(JSONCallQuery.<>o__41.<>p__4, JSONCallQuery.schema))
						{
							List<string> list2 = list;
							if (JSONCallQuery.<>o__41.<>p__1 == null)
							{
								JSONCallQuery.<>o__41.<>p__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(string), typeof(JSONCallQuery)));
							}
							Func<CallSite, object, string> target = JSONCallQuery.<>o__41.<>p__1.Target;
							CallSite <>p__ = JSONCallQuery.<>o__41.<>p__1;
							if (JSONCallQuery.<>o__41.<>p__0 == null)
							{
								JSONCallQuery.<>o__41.<>p__0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "id", typeof(JSONCallQuery), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
							}
							string text = target(<>p__, JSONCallQuery.<>o__41.<>p__0.Target(JSONCallQuery.<>o__41.<>p__0, obj));
							string text2 = "  ";
							if (JSONCallQuery.<>o__41.<>p__3 == null)
							{
								JSONCallQuery.<>o__41.<>p__3 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(string), typeof(JSONCallQuery)));
							}
							Func<CallSite, object, string> target2 = JSONCallQuery.<>o__41.<>p__3.Target;
							CallSite <>p__2 = JSONCallQuery.<>o__41.<>p__3;
							if (JSONCallQuery.<>o__41.<>p__2 == null)
							{
								JSONCallQuery.<>o__41.<>p__2 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "country", typeof(JSONCallQuery), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
							}
							list2.Add(text + text2 + target2(<>p__2, JSONCallQuery.<>o__41.<>p__2.Target(JSONCallQuery.<>o__41.<>p__2, obj)));
						}
						return list;
					}
					catch (RuntimeBinderException)
					{
						return null;
					}
				}
				list3 = null;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error ", MessageBoxButtons.OK);
				list3 = null;
			}
			return list3;
		}

		// Token: 0x0600054A RID: 1354 RVA: 0x0002D254 File Offset: 0x0002B454
		public static List<string> getMasterListDDOS()
		{
			if (!string.IsNullOrEmpty(JSONCallQuery.gett("http://registry.dstar.su/api/node.php")))
			{
				JSONCallQuery.schema = JsonConvert.DeserializeObject(JSONCallQuery.gett("http://registry.dstar.su/api/node.php"));
				try
				{
					List<string> list = new List<string>();
					if (JSONCallQuery.<>o__42.<>p__4 == null)
					{
						JSONCallQuery.<>o__42.<>p__4 = CallSite<Func<CallSite, object, IEnumerable>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(IEnumerable), typeof(JSONCallQuery)));
					}
					foreach (object obj in JSONCallQuery.<>o__42.<>p__4.Target(JSONCallQuery.<>o__42.<>p__4, JSONCallQuery.schema))
					{
						List<string> list2 = list;
						if (JSONCallQuery.<>o__42.<>p__1 == null)
						{
							JSONCallQuery.<>o__42.<>p__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(string), typeof(JSONCallQuery)));
						}
						Func<CallSite, object, string> target = JSONCallQuery.<>o__42.<>p__1.Target;
						CallSite <>p__ = JSONCallQuery.<>o__42.<>p__1;
						if (JSONCallQuery.<>o__42.<>p__0 == null)
						{
							JSONCallQuery.<>o__42.<>p__0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ID", typeof(JSONCallQuery), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
						}
						string text = target(<>p__, JSONCallQuery.<>o__42.<>p__0.Target(JSONCallQuery.<>o__42.<>p__0, obj));
						string text2 = "  ";
						if (JSONCallQuery.<>o__42.<>p__3 == null)
						{
							JSONCallQuery.<>o__42.<>p__3 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(string), typeof(JSONCallQuery)));
						}
						Func<CallSite, object, string> target2 = JSONCallQuery.<>o__42.<>p__3.Target;
						CallSite <>p__2 = JSONCallQuery.<>o__42.<>p__3;
						if (JSONCallQuery.<>o__42.<>p__2 == null)
						{
							JSONCallQuery.<>o__42.<>p__2 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Country", typeof(JSONCallQuery), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
						}
						list2.Add(text + text2 + target2(<>p__2, JSONCallQuery.<>o__42.<>p__2.Target(JSONCallQuery.<>o__42.<>p__2, obj)));
					}
					return list;
				}
				catch (RuntimeBinderException)
				{
					return null;
				}
			}
			return null;
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x0002D46C File Offset: 0x0002B66C
		public static int lookupDMRmaster(string hostName)
		{
			if (JSONCallQuery.<>o__43.<>p__1 == null)
			{
				JSONCallQuery.<>o__43.<>p__1 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(JSONCallQuery), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
			}
			Func<CallSite, object, bool> target = JSONCallQuery.<>o__43.<>p__1.Target;
			CallSite <>p__ = JSONCallQuery.<>o__43.<>p__1;
			if (JSONCallQuery.<>o__43.<>p__0 == null)
			{
				JSONCallQuery.<>o__43.<>p__0 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(JSONCallQuery), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null)
				}));
			}
			if (target(<>p__, JSONCallQuery.<>o__43.<>p__0.Target(JSONCallQuery.<>o__43.<>p__0, JSONCallQuery.schema, null)))
			{
				return -1;
			}
			int num = 0;
			if (JSONCallQuery.<>o__43.<>p__5 == null)
			{
				JSONCallQuery.<>o__43.<>p__5 = CallSite<Func<CallSite, object, IEnumerable>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(IEnumerable), typeof(JSONCallQuery)));
			}
			foreach (object obj in JSONCallQuery.<>o__43.<>p__5.Target(JSONCallQuery.<>o__43.<>p__5, JSONCallQuery.schema))
			{
				if (JSONCallQuery.<>o__43.<>p__4 == null)
				{
					JSONCallQuery.<>o__43.<>p__4 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(JSONCallQuery), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
				}
				Func<CallSite, object, bool> target2 = JSONCallQuery.<>o__43.<>p__4.Target;
				CallSite <>p__2 = JSONCallQuery.<>o__43.<>p__4;
				if (JSONCallQuery.<>o__43.<>p__3 == null)
				{
					JSONCallQuery.<>o__43.<>p__3 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(JSONCallQuery), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
					}));
				}
				Func<CallSite, object, string, object> target3 = JSONCallQuery.<>o__43.<>p__3.Target;
				CallSite <>p__3 = JSONCallQuery.<>o__43.<>p__3;
				if (JSONCallQuery.<>o__43.<>p__2 == null)
				{
					JSONCallQuery.<>o__43.<>p__2 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "address", typeof(JSONCallQuery), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
				}
				if (target2(<>p__2, target3(<>p__3, JSONCallQuery.<>o__43.<>p__2.Target(JSONCallQuery.<>o__43.<>p__2, obj), hostName)))
				{
					return num;
				}
				num++;
			}
			return 0;
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x0002D6A4 File Offset: 0x0002B8A4
		public static string lookupHostname(int number)
		{
			if (JSONCallQuery.<>o__44.<>p__2 == null)
			{
				JSONCallQuery.<>o__44.<>p__2 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(string), typeof(JSONCallQuery)));
			}
			Func<CallSite, object, string> target = JSONCallQuery.<>o__44.<>p__2.Target;
			CallSite <>p__ = JSONCallQuery.<>o__44.<>p__2;
			if (JSONCallQuery.<>o__44.<>p__1 == null)
			{
				JSONCallQuery.<>o__44.<>p__1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "address", typeof(JSONCallQuery), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
			}
			Func<CallSite, object, object> target2 = JSONCallQuery.<>o__44.<>p__1.Target;
			CallSite <>p__2 = JSONCallQuery.<>o__44.<>p__1;
			if (JSONCallQuery.<>o__44.<>p__0 == null)
			{
				JSONCallQuery.<>o__44.<>p__0 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(JSONCallQuery), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
				}));
			}
			return target(<>p__, target2(<>p__2, JSONCallQuery.<>o__44.<>p__0.Target(JSONCallQuery.<>o__44.<>p__0, JSONCallQuery.schema, number)));
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x0002D790 File Offset: 0x0002B990
		public static int searchYSFhost(string reflector)
		{
			int num = 0;
			if (JSONCallQuery.getYSFMasterList() == null)
			{
				return -1;
			}
			using (List<string>.Enumerator enumerator = JSONCallQuery.getYSFMasterList().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Equals(reflector))
					{
						return num;
					}
					num++;
				}
			}
			return -1;
		}

		// Token: 0x0600054E RID: 1358 RVA: 0x0002D7F8 File Offset: 0x0002B9F8
		public static string lookupFusionHostname(int number)
		{
			if (JSONCallQuery.<>o__46.<>p__2 == null)
			{
				JSONCallQuery.<>o__46.<>p__2 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(string), typeof(JSONCallQuery)));
			}
			Func<CallSite, object, string> target = JSONCallQuery.<>o__46.<>p__2.Target;
			CallSite <>p__ = JSONCallQuery.<>o__46.<>p__2;
			if (JSONCallQuery.<>o__46.<>p__1 == null)
			{
				JSONCallQuery.<>o__46.<>p__1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Host", typeof(JSONCallQuery), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
			}
			Func<CallSite, object, object> target2 = JSONCallQuery.<>o__46.<>p__1.Target;
			CallSite <>p__2 = JSONCallQuery.<>o__46.<>p__1;
			if (JSONCallQuery.<>o__46.<>p__0 == null)
			{
				JSONCallQuery.<>o__46.<>p__0 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(JSONCallQuery), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
				}));
			}
			return target(<>p__, target2(<>p__2, JSONCallQuery.<>o__46.<>p__0.Target(JSONCallQuery.<>o__46.<>p__0, JSONCallQuery.schema2, number)));
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x0002D8E4 File Offset: 0x0002BAE4
		public static int lookupFusionPort(int number)
		{
			int num = 42000;
			try
			{
				if (JSONCallQuery.<>o__47.<>p__2 == null)
				{
					JSONCallQuery.<>o__47.<>p__2 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(string), typeof(JSONCallQuery)));
				}
				Func<CallSite, object, string> target = JSONCallQuery.<>o__47.<>p__2.Target;
				CallSite <>p__ = JSONCallQuery.<>o__47.<>p__2;
				if (JSONCallQuery.<>o__47.<>p__1 == null)
				{
					JSONCallQuery.<>o__47.<>p__1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Port", typeof(JSONCallQuery), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
				}
				Func<CallSite, object, object> target2 = JSONCallQuery.<>o__47.<>p__1.Target;
				CallSite <>p__2 = JSONCallQuery.<>o__47.<>p__1;
				if (JSONCallQuery.<>o__47.<>p__0 == null)
				{
					JSONCallQuery.<>o__47.<>p__0 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(JSONCallQuery), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
					}));
				}
				num = int.Parse(target(<>p__, target2(<>p__2, JSONCallQuery.<>o__47.<>p__0.Target(JSONCallQuery.<>o__47.<>p__0, JSONCallQuery.schema2, number))));
			}
			catch (FormatException ex)
			{
				Console.WriteLine(ex.Message);
			}
			return num;
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x0002D9FC File Offset: 0x0002BBFC
		public static void callLoaderNXDN()
		{
			try
			{
				JSONCallQuery.callDBlistNXDN.Clear();
				string text;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					text = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\nxdn.csv";
				}
				else
				{
					text = "nxdn.csv";
				}
				using (FileStream fileStream = File.OpenRead(text))
				{
					using (BufferedStream bufferedStream = new BufferedStream(fileStream))
					{
						using (StreamReader streamReader = new StreamReader(bufferedStream))
						{
							int num = 0;
							string text2;
							while ((text2 = streamReader.ReadLine()) != null)
							{
								if (num != 0)
								{
									JSONCallQuery.callClassNXDN callClassNXDN = new JSONCallQuery.callClassNXDN();
									callClassNXDN.nxdnID = text2.Split(new char[] { ',' })[0];
									callClassNXDN.call = text2.Split(new char[] { ',' })[1];
									callClassNXDN.name = text2.Split(new char[] { ',' })[2].Split(null)[0];
									callClassNXDN.city = text2.Split(new char[] { ',' })[4];
									callClassNXDN.country = text2.Split(new char[] { ',' })[6];
									JSONCallQuery.callDBlistNXDN.Add(callClassNXDN);
								}
								num++;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				string text3 = "ERROR callLoader : ";
				Exception ex2 = ex;
				MessageBox.Show(text3 + ((ex2 != null) ? ex2.ToString() : null));
			}
		}

		// Token: 0x06000551 RID: 1361 RVA: 0x0002DBBC File Offset: 0x0002BDBC
		public static void callLoader()
		{
			try
			{
				JSONCallQuery.callDBlist.Clear();
				string text;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					text = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\user.csv";
				}
				else
				{
					text = "user.csv";
				}
				using (FileStream fileStream = File.OpenRead(text))
				{
					using (BufferedStream bufferedStream = new BufferedStream(fileStream))
					{
						using (StreamReader streamReader = new StreamReader(bufferedStream))
						{
							int num = 0;
							string text2;
							while ((text2 = streamReader.ReadLine()) != null)
							{
								if (num != 0)
								{
									JSONCallQuery.callClass callClass = new JSONCallQuery.callClass();
									callClass.dmrID = text2.Split(new char[] { ',' })[0];
									callClass.call = text2.Split(new char[] { ',' })[1];
									callClass.name = text2.Split(new char[] { ',' })[2].Split(null)[0];
									callClass.city = text2.Split(new char[] { ',' })[4];
									callClass.country = text2.Split(new char[] { ',' })[6];
									JSONCallQuery.callDBlist.Add(callClass);
								}
								num++;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				string text3 = "ERROR callLoader : ";
				Exception ex2 = ex;
				MessageBox.Show(text3 + ((ex2 != null) ? ex2.ToString() : null));
			}
		}

		// Token: 0x06000552 RID: 1362 RVA: 0x0002DD7C File Offset: 0x0002BF7C
		public static string[] callLookupcsvNEW(string lookup, information.MODUS moduss)
		{
			if (lookup == null)
			{
				return new string[] { " ", "", "", "", "" };
			}
			if (moduss != information.MODUS.NXDN)
			{
				if (JSONCallQuery.callDBlist.Count < 1)
				{
					JSONCallQuery.callLoader();
				}
				foreach (JSONCallQuery.callClass callClass in JSONCallQuery.callDBlist)
				{
					if (callClass.call.Equals(lookup.Trim()))
					{
						return new string[] { callClass.call, callClass.name, callClass.dmrID, callClass.city, callClass.country };
					}
					if (callClass.dmrID.Equals(lookup.Trim()))
					{
						return new string[] { callClass.call, callClass.name, callClass.dmrID, callClass.city, callClass.country };
					}
				}
			}
			if (moduss == information.MODUS.NXDN)
			{
				if (JSONCallQuery.callDBlistNXDN.Count < 1)
				{
					JSONCallQuery.callLoaderNXDN();
				}
				foreach (JSONCallQuery.callClassNXDN callClassNXDN in JSONCallQuery.callDBlistNXDN)
				{
					if (callClassNXDN.nxdnID == lookup)
					{
						return new string[] { callClassNXDN.call, callClassNXDN.name, callClassNXDN.nxdnID, callClassNXDN.city, callClassNXDN.country };
					}
				}
			}
			return new string[] { lookup, "Unknown", "Unknown", "Unknown", "Unknown" };
		}

		// Token: 0x06000553 RID: 1363 RVA: 0x0002DF7C File Offset: 0x0002C17C
		public static void callLookupcsv(string searchName, information.MODUS moduss)
		{
			if (searchName != null && searchName.Trim().Length != 0)
			{
				string[] array = JSONCallQuery.callLookupcsvNEW(searchName, moduss);
				JSONCallQuery.ChangeDMRCallStatusText(array[0], array[1], array[2], information.hisDMRdest, array[3], array[4]);
			}
		}

		// Token: 0x0400036D RID: 877
		[Dynamic]
		private static dynamic schema;

		// Token: 0x0400036E RID: 878
		[Dynamic]
		private static dynamic schema2;

		// Token: 0x0400036F RID: 879
		private static string lastSearchname = "";

		// Token: 0x04000370 RID: 880
		private static List<string> YSFtermsList = null;

		// Token: 0x04000371 RID: 881
		private static IEnumerable<string> strLinesNew = null;

		// Token: 0x04000372 RID: 882
		private static IEnumerable<string> strLines = null;

		// Token: 0x04000373 RID: 883
		public static List<JSONCallQuery.callClass> callDBlist = new List<JSONCallQuery.callClass>();

		// Token: 0x04000374 RID: 884
		public static List<JSONCallQuery.callClassNXDN> callDBlistNXDN = new List<JSONCallQuery.callClassNXDN>();

		// Token: 0x02000094 RID: 148
		public class callClass
		{
			// Token: 0x170000E9 RID: 233
			// (get) Token: 0x060006B6 RID: 1718 RVA: 0x000360D3 File Offset: 0x000342D3
			// (set) Token: 0x060006B7 RID: 1719 RVA: 0x000360DB File Offset: 0x000342DB
			public string dmrID { get; set; }

			// Token: 0x170000EA RID: 234
			// (get) Token: 0x060006B8 RID: 1720 RVA: 0x000360E4 File Offset: 0x000342E4
			// (set) Token: 0x060006B9 RID: 1721 RVA: 0x000360EC File Offset: 0x000342EC
			public string call { get; set; }

			// Token: 0x170000EB RID: 235
			// (get) Token: 0x060006BA RID: 1722 RVA: 0x000360F5 File Offset: 0x000342F5
			// (set) Token: 0x060006BB RID: 1723 RVA: 0x000360FD File Offset: 0x000342FD
			public string name { get; set; }

			// Token: 0x170000EC RID: 236
			// (get) Token: 0x060006BC RID: 1724 RVA: 0x00036106 File Offset: 0x00034306
			// (set) Token: 0x060006BD RID: 1725 RVA: 0x0003610E File Offset: 0x0003430E
			public string city { get; set; }

			// Token: 0x170000ED RID: 237
			// (get) Token: 0x060006BE RID: 1726 RVA: 0x00036117 File Offset: 0x00034317
			// (set) Token: 0x060006BF RID: 1727 RVA: 0x0003611F File Offset: 0x0003431F
			public string country { get; set; }
		}

		// Token: 0x02000095 RID: 149
		public class callClassNXDN
		{
			// Token: 0x170000EE RID: 238
			// (get) Token: 0x060006C1 RID: 1729 RVA: 0x00036128 File Offset: 0x00034328
			// (set) Token: 0x060006C2 RID: 1730 RVA: 0x00036130 File Offset: 0x00034330
			public string nxdnID { get; set; }

			// Token: 0x170000EF RID: 239
			// (get) Token: 0x060006C3 RID: 1731 RVA: 0x00036139 File Offset: 0x00034339
			// (set) Token: 0x060006C4 RID: 1732 RVA: 0x00036141 File Offset: 0x00034341
			public string call { get; set; }

			// Token: 0x170000F0 RID: 240
			// (get) Token: 0x060006C5 RID: 1733 RVA: 0x0003614A File Offset: 0x0003434A
			// (set) Token: 0x060006C6 RID: 1734 RVA: 0x00036152 File Offset: 0x00034352
			public string name { get; set; }

			// Token: 0x170000F1 RID: 241
			// (get) Token: 0x060006C7 RID: 1735 RVA: 0x0003615B File Offset: 0x0003435B
			// (set) Token: 0x060006C8 RID: 1736 RVA: 0x00036163 File Offset: 0x00034363
			public string city { get; set; }

			// Token: 0x170000F2 RID: 242
			// (get) Token: 0x060006C9 RID: 1737 RVA: 0x0003616C File Offset: 0x0003436C
			// (set) Token: 0x060006CA RID: 1738 RVA: 0x00036174 File Offset: 0x00034374
			public string country { get; set; }
		}
	}
}
