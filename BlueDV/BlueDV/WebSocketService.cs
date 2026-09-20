using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace BlueDV
{
	// Token: 0x0200003E RID: 62
	public class WebSocketService : WebSocketBehavior
	{
		// Token: 0x06000432 RID: 1074 RVA: 0x0002B060 File Offset: 0x00029260
		protected override void OnMessage(MessageEventArgs e)
		{
			WebSocketService.connected = true;
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x0002B068 File Offset: 0x00029268
		protected override void OnOpen()
		{
			information.WEBusercount++;
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x0002B076 File Offset: 0x00029276
		protected override void OnClose(CloseEventArgs e)
		{
			information.WEBusercount--;
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x0002B084 File Offset: 0x00029284
		public void send(byte[] data)
		{
			if (WebSocketService.connected)
			{
				try
				{
					base.Send(data);
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x040002BC RID: 700
		private static bool connected;
	}
}
