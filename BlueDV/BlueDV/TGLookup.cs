using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;

namespace BlueDV
{
	// Token: 0x0200003D RID: 61
	internal class TGLookup
	{
		// Token: 0x0600042C RID: 1068 RVA: 0x0002AEC0 File Offset: 0x000290C0
		private static string getList(string url)
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
			catch (WebException ex)
			{
				MessageBox.Show("Can not make connection to Brandmeister network. Create a support ticket at Brandmeister! : " + ex.ToString());
				text = null;
			}
			catch (TimeoutException)
			{
				MessageBox.Show("TIMEOUT: Can not make connection to Brandmeister network. Create a support ticket at Brandmeister!");
				text = null;
			}
			return text;
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x0002AF74 File Offset: 0x00029174
		public static string lookup(string zoek)
		{
			if (TGLookup.schema == null)
			{
				return "Error 9123";
			}
			if (TGLookup.schema.ContainsKey(zoek))
			{
				return TGLookup.schema[zoek];
			}
			return null;
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x0002AFAC File Offset: 0x000291AC
		public static string lookup_name(string zoek)
		{
			return TGLookup.schema.FirstOrDefault((KeyValuePair<string, string> x) => x.Value == zoek).Key;
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x0002AFE4 File Offset: 0x000291E4
		public static void loadAll2()
		{
			string list = TGLookup.getList("https://api.brandmeister.network/v2/talkgroup");
			try
			{
				TGLookup.schema = JsonConvert.DeserializeObject<Dictionary<string, string>>(list);
			}
			catch (ArgumentNullException)
			{
				TGLookup.schema = null;
			}
			catch (Exception)
			{
				TGLookup.schema = null;
			}
			try
			{
			}
			catch (RuntimeBinderException)
			{
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x040002BB RID: 699
		public static Dictionary<string, string> schema;
	}
}
