using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace BlueDVAMBEServer
{
	// Token: 0x02000007 RID: 7
	internal class UDPServerTest
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000028 RID: 40 RVA: 0x00003580 File Offset: 0x00001780
		// (set) Token: 0x06000029 RID: 41 RVA: 0x00003587 File Offset: 0x00001787
		public static string StatusText { get; private set; }

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x0600002A RID: 42 RVA: 0x00003590 File Offset: 0x00001790
		// (remove) Token: 0x0600002B RID: 43 RVA: 0x000035C4 File Offset: 0x000017C4
		public static event EventHandler StatusTextChanged;

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600002D RID: 45 RVA: 0x000035FF File Offset: 0x000017FF
		// (set) Token: 0x0600002C RID: 44 RVA: 0x000035F7 File Offset: 0x000017F7
		public List<Tuple<EndPoint, byte[]>> DataList
		{
			get
			{
				return UDPServerTest.dataList;
			}
			private set
			{
				UDPServerTest.dataList = value;
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00003608 File Offset: 0x00001808
		public static void Start()
		{
			UDPServerTest.serverSocket = null;
			UDPServerTest.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			UDPServerTest.serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			UDPServerTest.serverSocket.Bind(new IPEndPoint(IPAddress.Any, information.UDPport));
			UDPServerTest.newClientEP = new IPEndPoint(IPAddress.Any, 0);
			UDPServerTest.serverSocket.BeginReceiveFrom(UDPServerTest.byteData, 0, UDPServerTest.byteData.Length, SocketFlags.None, ref UDPServerTest.newClientEP, new AsyncCallback(UDPServerTest.DoReceiveFrom), UDPServerTest.newClientEP);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00003694 File Offset: 0x00001894
		private static void DoReceiveFrom(IAsyncResult iar)
		{
			try
			{
				if (UDPServerTest.serverSocket != null)
				{
					int num = UDPServerTest.serverSocket.EndReceiveFrom(iar, ref UDPServerTest.newClientEP);
					byte[] array = new byte[num];
					Array.Copy(UDPServerTest.byteData, array, num);
					serial.write(array);
				}
			}
			catch (Exception)
			{
				UDPServerTest.ChangeClientIPStatusText("");
			}
			finally
			{
				try
				{
					if (UDPServerTest.serverSocket != null)
					{
						EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
						UDPServerTest.serverSocket.BeginReceiveFrom(UDPServerTest.byteData, 0, UDPServerTest.byteData.Length, SocketFlags.None, ref endPoint, new AsyncCallback(UDPServerTest.DoReceiveFrom), endPoint);
					}
				}
				catch (Exception)
				{
					UDPServerTest.ChangeClientIPStatusText("");
				}
			}
			if (!UDPServerTest.newClientEP.Equals(information.clientIP))
			{
				information.clientIP = UDPServerTest.newClientEP.ToString();
				UDPServerTest.ChangeClientIPStatusText(information.clientIP);
			}
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00003784 File Offset: 0x00001984
		public static void SendTo(byte[] data)
		{
			if (UDPServerTest.serverSocket != null)
			{
				try
				{
					UDPServerTest.serverSocket.SendTo(data, UDPServerTest.newClientEP);
				}
				catch (SocketException)
				{
					information.clientIP = "";
					UDPServerTest.ChangeClientIPStatusText("");
				}
			}
		}

		// Token: 0x06000031 RID: 49 RVA: 0x000037D4 File Offset: 0x000019D4
		private static void ChangeClientIPStatusText(string text)
		{
			UDPServerTest.StatusText = text;
			EventHandler statusTextChanged = UDPServerTest.StatusTextChanged;
			if (statusTextChanged != null)
			{
				statusTextChanged(null, EventArgs.Empty);
			}
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000037FC File Offset: 0x000019FC
		public static void Stop()
		{
			if (UDPServerTest.serverSocket != null)
			{
				UDPServerTest.serverSocket.Close();
				UDPServerTest.serverSocket = null;
			}
			UDPServerTest.dataList.Clear();
		}

		// Token: 0x04000021 RID: 33
		private static Socket serverSocket = null;

		// Token: 0x04000022 RID: 34
		private static List<Tuple<EndPoint, byte[]>> dataList = new List<Tuple<EndPoint, byte[]>>();

		// Token: 0x04000023 RID: 35
		private static byte[] byteData = new byte[1024];

		// Token: 0x04000024 RID: 36
		private static EndPoint newClientEP;
	}
}
