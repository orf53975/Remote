using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Remote
{
	public static class CONNECT
	{
		public static SOCKET.Client Connect(ref string[] arr)
		{
			IPAddress address;
			if (IPAddress.TryParse(arr[1], out address))
			{
				SOCKET.Client client = new SOCKET.Client();
				client.socket = new Socket(new IPEndPoint(address, 6546).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				try
				{
					client.socket.Connect(address, 6546);
					return client;
				}
				catch (SocketException e)
				{
					Console.WriteLine(e);
					return null;
				}
			}
			return null;
		}
	}
}
