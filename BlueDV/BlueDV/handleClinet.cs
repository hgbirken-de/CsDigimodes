using System;
using System.Collections;
using System.Net.Sockets;
using System.Threading;

namespace BlueDV
{
	// Token: 0x0200003B RID: 59
	public class handleClinet
	{
		// Token: 0x06000428 RID: 1064 RVA: 0x0002AE28 File Offset: 0x00029028
		public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList)
		{
			this.clientSocket = inClientSocket;
			this.clNo = clineNo;
			this.clientsList = cList;
			new Thread(new ThreadStart(this.doChat)).Start();
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x0002AE58 File Offset: 0x00029058
		private void doChat()
		{
			int num = 0;
			num = 0;
			for (;;)
			{
				try
				{
					byte[] array = new byte[this.clientSocket.ReceiveBufferSize];
					num++;
					this.clientSocket.GetStream().Read(array, 0, array.Length);
					Convert.ToString(num);
					StreamServer.broadcast(array);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}

		// Token: 0x040002B8 RID: 696
		private TcpClient clientSocket;

		// Token: 0x040002B9 RID: 697
		private string clNo;

		// Token: 0x040002BA RID: 698
		private Hashtable clientsList;
	}
}
