using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Remote
{
	public static class PUSH
	{
		public static void SEND(Message[] message, string[] addresses)
		{
			for (int i = 0; i < addresses.Length; i++)
			{
				string[] temp = new string[] { null, addresses[i] };
				SOCKET.Client client = CONNECT.Connect(ref temp);

				/*if (client != null)
				{
					client.StartReceiveAsync(ReceiveAsync);
				}*/

				client.p.New();
				client.p.Set(Command.PUSH);
				//client.p.bw.Write(message);
			}
		}
		private static void ReceiveAsync(SocketAsyncEventArgs arg)
		{

		}
		public class Message
		{
			public Command c;
			public string[] message;
			public Message(Command c, string[] message)
			{
				this.c = c;
				this.message = message;
			}
		}
	}
}
