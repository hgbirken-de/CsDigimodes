using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BlueDV
{
	// Token: 0x02000038 RID: 56
	internal class Server
	{
		// Token: 0x06000402 RID: 1026 RVA: 0x00029DE8 File Offset: 0x00027FE8
		public static void Main()
		{
			TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 80);
			tcpListener.Start();
			Console.WriteLine("Server has started on 127.0.0.1:80.{0}Waiting for a connection...", Environment.NewLine);
			TcpClient tcpClient = tcpListener.AcceptTcpClient();
			Console.WriteLine("A client connected.");
			NetworkStream stream = tcpClient.GetStream();
			for (;;)
			{
				if (stream.DataAvailable)
				{
					byte[] array = new byte[tcpClient.Available];
					stream.Read(array, 0, array.Length);
					string @string = Encoding.UTF8.GetString(array);
					if (Regex.IsMatch(@string, "^GET"))
					{
						byte[] bytes = Encoding.UTF8.GetBytes(string.Concat(new string[]
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
						}));
						stream.Write(bytes, 0, bytes.Length);
					}
				}
			}
		}
	}
}
