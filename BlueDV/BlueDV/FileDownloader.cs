using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace BlueDV
{
	// Token: 0x0200001C RID: 28
	internal class FileDownloader
	{
		// Token: 0x06000192 RID: 402 RVA: 0x00010CD0 File Offset: 0x0000EED0
		public FileDownloader(string url, string fullPathWhereToSave, string popupmessage)
		{
			if (string.IsNullOrEmpty(url))
			{
				throw new ArgumentNullException("url");
			}
			if (string.IsNullOrEmpty(fullPathWhereToSave))
			{
				throw new ArgumentNullException("fullPathWhereToSave");
			}
			this.popupMessage = popupmessage;
			this._url = url;
			this._fullPathWhereToSave = fullPathWhereToSave;
		}

		// Token: 0x06000193 RID: 403 RVA: 0x00010D2C File Offset: 0x0000EF2C
		public bool StartDownload(int timeout)
		{
			bool flag;
			try
			{
				using (WebClient webClient = new WebClient())
				{
					Uri uri = new Uri(this._url);
					webClient.DownloadProgressChanged += this.WebClientDownloadProgressChanged;
					webClient.DownloadFileCompleted += this.WebClientDownloadCompleted;
					webClient.DownloadFileAsync(uri, this._fullPathWhereToSave);
					flag = this._result && File.Exists(this._fullPathWhereToSave);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000194 RID: 404 RVA: 0x00002F42 File Offset: 0x00001142
		private void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
		}

		// Token: 0x06000195 RID: 405 RVA: 0x00010DCC File Offset: 0x0000EFCC
		private void WebClientDownloadCompleted(object sender, AsyncCompletedEventArgs args)
		{
			if (args.Error != null)
			{
				MessageBox.Show(args.ToString());
			}
			this._result = !args.Cancelled;
			if (this._result)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					MessageBox.Show("Downloaded : " + this.popupMessage);
					break;
				case information.LANGUAGE.JAPANESE:
					MessageBox.Show("ダウンロード : " + this.popupMessage);
					return;
				case information.LANGUAGE.CHINEES:
					MessageBox.Show("Downloaded : " + this.popupMessage);
					return;
				case information.LANGUAGE.KOREAN:
					MessageBox.Show("다운로드됨 : " + this.popupMessage);
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x06000196 RID: 406 RVA: 0x00010E7E File Offset: 0x0000F07E
		public static bool DownloadFile(string url, string fullPathWhereToSave, int timeoutInMilliSec, string popupmessage)
		{
			return new FileDownloader(url, fullPathWhereToSave, popupmessage).StartDownload(timeoutInMilliSec);
		}

		// Token: 0x040000D8 RID: 216
		private readonly string _url;

		// Token: 0x040000D9 RID: 217
		private readonly string _fullPathWhereToSave;

		// Token: 0x040000DA RID: 218
		private bool _result;

		// Token: 0x040000DB RID: 219
		private readonly string popupMessage = "";
	}
}
