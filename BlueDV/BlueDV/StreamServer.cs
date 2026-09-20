using System;
using System.Collections;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BlueDV
{
	// Token: 0x0200003A RID: 58
	internal class StreamServer
	{
		// Token: 0x06000424 RID: 1060 RVA: 0x0002AC44 File Offset: 0x00028E44
		public static void startme()
		{
			TcpListener tcpListener = new TcpListener(8080);
			tcpListener.Start();
			Console.WriteLine("Chat Server Started ....");
			int num = 0;
			for (;;)
			{
				num++;
				TcpClient tcpClient = tcpListener.AcceptTcpClient();
				byte[] array = new byte[tcpClient.ReceiveBufferSize];
				string text = null;
				tcpClient.GetStream().Read(array, 0, array.Length);
				StreamServer.clientsList.Add(array, tcpClient);
				string @string = Encoding.UTF8.GetString(array);
				if (new Regex("^GET").IsMatch(@string))
				{
					Console.WriteLine("recieved GET");
					StreamServer.broadcast(Encoding.UTF8.GetBytes(string.Concat(new string[]
					{
						"HTTP/1.1 101 Switching Protocols",
						Environment.NewLine,
						"Connection: Upgrade",
						Environment.NewLine,
						"Upgrade: websocket",
						Environment.NewLine,
						"Sec-WebSocket-Accept: ",
						Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(new Regex("Sec-WebSocket-Key: (.*)").Match(@string).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))),
						Environment.NewLine,
						Environment.NewLine
					})));
				}
				Console.WriteLine("New user added : ");
				new handleClinet().startClient(tcpClient, text, StreamServer.clientsList);
			}
		}

		// Token: 0x06000425 RID: 1061 RVA: 0x0002ADA8 File Offset: 0x00028FA8
		public static void broadcast(byte[] broadcastBytes)
		{
			foreach (object obj in StreamServer.clientsList)
			{
				NetworkStream stream = ((TcpClient)((DictionaryEntry)obj).Value).GetStream();
				stream.Write(broadcastBytes, 0, broadcastBytes.Length);
				stream.Flush();
			}
		}

		// Token: 0x040002B7 RID: 695
		public static Hashtable clientsList = new Hashtable();
	}
}
