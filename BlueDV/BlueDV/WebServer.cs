using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BlueDV
{
	// Token: 0x02000035 RID: 53
	public class WebServer
	{
		// Token: 0x060003E4 RID: 996 RVA: 0x00028A40 File Offset: 0x00026C40
		public WebServer(string prefixes)
		{
			if (!HttpListener.IsSupported)
			{
				throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
			}
			if (prefixes == null || prefixes.Length == 0)
			{
				throw new ArgumentException("prefixes");
			}
			this._listener.Prefixes.Add(prefixes);
			Func<HttpListenerRequest, string> func;
			if ((func = WebServer.<>O.<0>__SendResponse2) == null)
			{
				func = (WebServer.<>O.<0>__SendResponse2 = new Func<HttpListenerRequest, string>(WebServer.SendResponse2));
			}
			this._responderMethod = func;
			try
			{
				this._listener.Start();
			}
			catch (HttpListenerException ex)
			{
				MessageBox.Show(ex.ToString());
				if (ex.Message.Contains("Access is denied"))
				{
					MessageBox.Show("WebServer permission Denied");
				}
			}
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x00028B00 File Offset: 0x00026D00
		public void Run()
		{
			ThreadPool.QueueUserWorkItem(delegate(object o)
			{
				try
				{
					while (this._listener.IsListening)
					{
						ThreadPool.QueueUserWorkItem(delegate(object c)
						{
							HttpListenerContext httpListenerContext = c as HttpListenerContext;
							try
							{
								string text = this._responderMethod(httpListenerContext.Request);
								byte[] bytes = Encoding.UTF8.GetBytes(text);
								httpListenerContext.Response.ContentLength64 = (long)bytes.Length;
								httpListenerContext.Response.OutputStream.Write(bytes, 0, bytes.Length);
							}
							catch
							{
							}
							finally
							{
								httpListenerContext.Response.OutputStream.Close();
							}
						}, this._listener.GetContext());
					}
				}
				catch
				{
				}
			});
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x00028B14 File Offset: 0x00026D14
		public void Stop()
		{
			this._listener.Stop();
			this._listener.Close();
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x00028B2C File Offset: 0x00026D2C
		public static string SendResponse2(HttpListenerRequest request)
		{
			if (string.Compare(request.HttpMethod, "POST", true) == 0)
			{
				return WebServer.doPost(request);
			}
			string rawUrl = request.RawUrl;
			if (rawUrl != null)
			{
				int length = rawUrl.Length;
				if (length != 1)
				{
					switch (length)
					{
					case 7:
						if (rawUrl == "/mobile")
						{
							if (information.m_language == information.LANGUAGE.JAPANESE)
							{
								return WebServer.ReadTextResourceFromAssembly("BlueDV.HTML.mobile-japanese.html");
							}
							return WebServer.ReadTextResourceFromAssembly("BlueDV.HTML.mobile.html");
						}
						break;
					case 8:
						if (rawUrl == "/mobile/")
						{
							if (information.m_language == information.LANGUAGE.JAPANESE)
							{
								return WebServer.ReadTextResourceFromAssembly("BlueDV.HTML.mobile-japanese.html");
							}
							return WebServer.ReadTextResourceFromAssembly("BlueDV.HTML.mobile.html");
						}
						break;
					case 9:
						if (rawUrl == "/api/motd")
						{
							return "{\"motd\":\"" + WebServer.getFiles2("webmessage.txt") + "\"}";
						}
						break;
					case 10:
					case 11:
					case 13:
						break;
					case 12:
					{
						char c = rawUrl[5];
						if (c != 'g')
						{
							if (c == 'r')
							{
								if (rawUrl == "/api/reccall")
								{
									switch (information.stream_modus)
									{
									case information.MODUS.DMR:
										return string.Concat(new string[]
										{
											"{\"Call\":\"",
											information.hisDMRID,
											"\",  \"destination\":\"",
											information.hisDMRdest,
											"\", \"mode\":\"",
											information.stream_modus.ToString(),
											"\"}"
										});
									case information.MODUS.DSTAR:
										return string.Concat(new string[]
										{
											"{\"Call\":\"",
											information.hisCall,
											"\",  \"destination\":\"",
											information.connectedDSTARReflector,
											"\", \"mode\":\"",
											information.stream_modus.ToString(),
											"\"}"
										});
									case information.MODUS.FUSION:
										return string.Concat(new string[]
										{
											"{\"Call\":\"",
											information.hisCall,
											"\",  \"destination\":\"",
											information.fusionReflector,
											"\", \"mode\":\"",
											information.stream_modus.ToString(),
											"\"}"
										});
									default:
										return "{\"Call\":\"        \",  \"destination\":\"  \", \"mode\":\" \"}";
									}
								}
							}
						}
						else if (rawUrl == "/api/general")
						{
							return string.Concat(new string[]
							{
								"{\"voiceport\":\"",
								information.webVoicePort,
								"\",\"appVersion\":\"",
								information.BlueDVVersion,
								"\", \"radio_version\":\"AMBE3000\"}"
							});
						}
						break;
					}
					case 14:
						if (rawUrl == "/pcm-player.js")
						{
							return WebServer.ReadTextResourceFromAssembly("BlueDV.HTML.pcm-player.js");
						}
						break;
					default:
						if (length == 18)
						{
							if (rawUrl == "/api/networkstatus")
							{
								return string.Concat(new string[]
								{
									"{\"hostname\":\"",
									information.AMBEServerHost,
									"\", \"connectedClients\": \"",
									information.WEBusercount.ToString(),
									"\", \"mode\":\"",
									information.stream_modus.ToString(),
									"\", \"AMBEStatus\":\"",
									information.m_qsoStatus.ToString(),
									"\"}"
								});
							}
						}
						break;
					}
				}
				else if (rawUrl == "/")
				{
					if (information.m_language == information.LANGUAGE.JAPANESE)
					{
						return WebServer.ReadTextResourceFromAssembly("BlueDV.HTML.index-japanese.html");
					}
					return WebServer.ReadTextResourceFromAssembly("BlueDV.HTML.index.html");
				}
			}
			if (information.m_language == information.LANGUAGE.JAPANESE)
			{
				return WebServer.ReadTextResourceFromAssembly("BlueDV.HTML.index-japanese.html");
			}
			return WebServer.ReadTextResourceFromAssembly("BlueDV.HTML.index.html");
		}

		// Token: 0x060003E8 RID: 1000 RVA: 0x00028EB8 File Offset: 0x000270B8
		private static string doPost(HttpListenerRequest request)
		{
			using (Stream inputStream = request.InputStream)
			{
				using (StreamReader streamReader = new StreamReader(inputStream, request.ContentEncoding))
				{
					streamReader.ReadToEnd();
					request.RawUrl.StartsWith("/api/dstar/connect");
				}
			}
			return "Did not authenticate with Duo.";
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x00028F28 File Offset: 0x00027128
		public static string getFiles2(string fileName)
		{
			string text = "";
			try
			{
				text = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\" + fileName);
			}
			catch (Exception)
			{
			}
			return text;
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x00028F68 File Offset: 0x00027168
		private static byte[] readBinary(string fileName)
		{
			return File.ReadAllBytes(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\html\\" + fileName);
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x00028F80 File Offset: 0x00027180
		private static string getFiles(string fileName)
		{
			try
			{
				StreamReader streamReader;
				if (utils.RunningPlatform() == utils.Platform.Windows)
				{
					streamReader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\html\\" + fileName);
				}
				else
				{
					streamReader = null;
				}
				string text = null;
				string text2;
				while ((text2 = streamReader.ReadLine()) != null)
				{
					text = text + text2 + "\n";
				}
				streamReader.Close();
				return text;
			}
			catch (FileNotFoundException)
			{
			}
			return "Page not found";
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x00028FF0 File Offset: 0x000271F0
		public static string ReadTextResourceFromAssembly(string name)
		{
			string text;
			using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
			{
				text = new StreamReader(manifestResourceStream).ReadToEnd();
			}
			return text;
		}

		// Token: 0x0400028E RID: 654
		private readonly HttpListener _listener = new HttpListener();

		// Token: 0x0400028F RID: 655
		private readonly Func<HttpListenerRequest, string> _responderMethod;

		// Token: 0x04000290 RID: 656
		public static string HTMLPAGE = "<HTML><form  method=\"post\" enctype=\"application/json\">  First name:<br>  <input type = \"text\" name=\"firstname\" value=\"Mickey\"><br>Last name:<br> <input type = \"text\" name=\"lastname\" value=\"Mouse\"><br><br>  <input type = \"submit\" value=\"Submit\"></form></HTML>";

		// Token: 0x0200007E RID: 126
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04000506 RID: 1286
			public static Func<HttpListenerRequest, string> <0>__SendResponse2;
		}
	}
}
