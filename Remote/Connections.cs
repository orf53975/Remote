using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Remote
{
	/*class Connections
	{*/
	public class SOCKET
	{
		public class Server
		{
			public Socket socket;
			public IPAddress ipAddress;
			public IPEndPoint ipEndPoint;
			public Func<SocketAsyncEventArgs, bool> onConnectionAccept;
			public Server(string address, int port)
			{
				ipAddress = IPAddress.Parse(address);
				ipEndPoint = new IPEndPoint(ipAddress, port);
				socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			}
			public Server(int port)
			{
				ipAddress = IPAddress.Any;
				ipEndPoint = new IPEndPoint(ipAddress, port);
				socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			}
			public bool Listen()
			{
				try
				{
					socket.Bind(ipEndPoint);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Could not bind to Address {0}: {1}", ipEndPoint, ex);
					return false;
				}
				socket.Listen(100);
				socket.NoDelay = true;
				return true;
			}
			public void StartReceiveAsync(Func<SocketAsyncEventArgs, bool> action)
			{
				onConnectionAccept = action;
				Init();
				//Start(arg);
			}
			private void Init()
			{
				SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
				arg.Completed += Received;
				Start(arg);
			}
			private void Start(SocketAsyncEventArgs arg)
			{
				if (arg != null)
				{
					arg.AcceptSocket = null;
				}
				bool notReceived = socket.AcceptAsync(arg);
				//Console.WriteLine(notReceived);
				if (!notReceived) onConnectionAccept(arg);
			}
			public void Received(object sender, SocketAsyncEventArgs e)
			{
				//Console.WriteLine("Got a package");
				//Process(e);
				if(onConnectionAccept(e)) Start(e);
			}
		}
		public class Client
		{
			public Socket socket;
			public byte[] bufferIn;
			public byte[] bufferOut;
			public Func<SocketAsyncEventArgs, bool> onReceiveAsync;
			public PacketManager p;
			public PacketManager pmIn;
			public PacketManager pmOut;
			public IPAddress ClientAddress
			{
				get
				{
					return (socket != null && socket.RemoteEndPoint != null) ?
						((IPEndPoint)socket.RemoteEndPoint).Address : null;
				}
			}
			public Client()
			{
				bufferIn = new byte[8192];
				bufferOut = new byte[8192];
				p = new PacketManager(bufferOut);
				pmIn = new PacketManager(bufferIn);
				pmOut = new PacketManager(bufferOut);
			}
			public Client(int inSize, int outSize)
			{
				bufferIn = new byte[inSize];
				bufferOut = new byte[outSize];
				p = new PacketManager(bufferOut);
				pmIn = new PacketManager(bufferIn);
				pmOut = new PacketManager(bufferOut);
			}
			public void StartReceiveAsync(Func<SocketAsyncEventArgs, bool> arg)
			{
				onReceiveAsync = arg;
				Receive();
			}
			public void Receive()
			{
				if (socket != null && socket.Connected)
				{
					SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
					arg.Completed += Received;
					arg.SetBuffer(bufferIn, 0, 8192);
					arg.UserToken = this;
					bool notReceived = socket.ReceiveAsync(arg);
					if (!notReceived) onReceiveAsync(arg);
				}
			}
			private void Resume(SocketAsyncEventArgs arg)
			{
				if (socket != null && socket.Connected)
				{
					bool notReceived = socket.ReceiveAsync(arg);
					if (!notReceived) onReceiveAsync(arg);
				}
			}

			private void Received(object sender, SocketAsyncEventArgs e)
			{
				if(onReceiveAsync(e)) Resume(e);
			}
			public void Send(int count)
			{
				if (socket != null && socket.Connected)
				{
					SocketAsyncEventArgs args = new SocketAsyncEventArgs();
					if (args != null)
					{

						//args.Completed += SendAsyncComplete;
						//args.SetBuffer(bufferOut, 0, count);
						//args.UserToken = this;
						//tcpSocket.SendAsync(args);
						socket.Send(bufferOut, count, SocketFlags.None);
						//Console.WriteLine("S -> C: {0} {1} bájt", packet.PacketId, packet.BaseStream.Position);
						//Console.WriteLine(pkt.Length);

					}
					else
					{
						Console.WriteLine("Client {0}'s SocketArgs are null", this);
					}
				}
			}
			public void Send(byte[] data, int count)
			{
				if (socket != null && socket.Connected)
				{
					SocketAsyncEventArgs args = new SocketAsyncEventArgs();
					if (args != null)
					{
						//args.Completed += SendAsyncComplete;
						//args.SetBuffer(bufferOut, 0, (int)count);
						//args.UserToken = this;
						//tcpSocket.SendAsync(args);
						socket.Send(data, count, SocketFlags.None);
						//Console.WriteLine("S -> C: {0} {1} bájt", packet.PacketId, packet.BaseStream.Position);
						//Console.WriteLine(pkt.Length);

					}
					else
					{
						Console.WriteLine("Client {0}'s SocketArgs are null", this);
					}
				}
			}
		}
	}
	//}	
}
