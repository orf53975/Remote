using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remote;
using System.Net;
using System.Net.Sockets;
namespace RemoteConsole
{
	class Program
	{
		public static SOCKET.Server server;
		//public static List<SOCKET.Client> clients;
		public static Dictionary<string, SOCKET.Client> clients = new Dictionary<string,SOCKET.Client>();
		public static Dictionary<string, IPAddress> computers = new Dictionary<string,IPAddress>();
		public static Dictionary<string, MyEnum> enumerators = new Dictionary<string,MyEnum>();
		public static string guiPath = @"C:\Users\tomes11\Documents\Visual Studio 2013\Projects\Remote\RemoteGUI\bin\Debug\RemoteGUI.exe";
		static void Main(string[] args)
		{
			server = new SOCKET.Server("127.0.0.1", 6546);
			server.Listen();
			server.StartReceiveAsync(OnConnectionAcept);
			while (true)
			{
				try
				{
					Remote.NetworkStream ns = new Remote.NetworkStream("127.0.0.1", 6547, Remote.NetworkStream.Type.Server);
					Console.WriteLine("Test server running");
				}
				catch
				{
					Console.Clear();
					Remote.NetworkStream ns = new Remote.NetworkStream("127.0.0.1", 6547, Remote.NetworkStream.Type.Client);
					Console.WriteLine("Test client running");
					byte[] arr = new byte[1016 * 295000 + 50];
					ns.SendSimple(arr);
				}
				string input = Console.ReadLine();
				string[] words = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				ProcessInput(ref words);
				continue;
				
			}
		}
		public static void ProcessInput(ref string[] words)
		{
			switch (words[0].ToLower())
			{
				case "connect":
					{
						if (!ArrayPositionCheck(1, words))
						{
							Console.WriteLine("Error: No IP");
							return;
						}
						SOCKET.Client client = CONNECT.Connect(ref words);
						if (client != null)
						{
							if (clients.ContainsKey(client.ClientAddress.ToString()))
							{
								Console.WriteLine("A connection to the following address is already made! -> {0}", client.ClientAddress.ToString());
								client.socket.Disconnect(false);
							}
							else
							{
								client.StartReceiveAsync(OnReceiveAsync);
								clients.Add(client.ClientAddress.ToString(), client);
								Console.WriteLine("Succesfully connected to {0}!", client.ClientAddress.ToString());
							}
						}
					}
					break;
				case "select":
					if (!ArrayPositionCheck(1, words))
					{
						Console.WriteLine("Can't select nothing!");
						return;
					}
					else
					{
						switch (words[1].ToLower())
						{
							case "clients":
								{
									MyEnum temp;
									enumerators.TryGetValue("clients", out temp);
									if (temp != null)
									{
										Console.WriteLine("An enumerator named \"clients\" already exists!");
									}
									else
									{
										enumerators.Add("clients", new MyEnum() { type = "clients", pos = 0, objects = clients.Keys.ToArray()});
										Console.WriteLine("Enumerator created!");
									}
								}
								break;
							default:
								break;
						}
					}
					break;
				case "next()":
				case "next":
					{
						if (!ArrayPositionCheck(2, words))
						{
							Console.WriteLine("Invalid enumerator command!");
							return;
						}
						else if (words[1] == "->")
						{
							MyEnum temp;
							enumerators.TryGetValue(words[2], out temp);
							if (temp != null)
							{
								temp.Next();
							}
							else
							{
								Console.WriteLine("Invalid enumerator!");
							}
						}
						else
						{
							Console.WriteLine("Invalid enumerator command!");
							return;
						}
					}
					break;
				case "ping":
					if (!ArrayPositionCheck(1, words))
					{
						Console.WriteLine("No IP or local channel, pinging default -> 192.168.1.*");
						PING.ALL("1");
						PING.ADD_NEW(ref computers);
						return;
					}
					else
					{
						int temp;
						if (int.TryParse(words[1], out temp))
						{
							PING.ALL(words[1]);
							PING.ADD_NEW(ref computers);
							return;
						}
						else
						{
							Console.WriteLine("Error: Invalid channel");
						}
					}
					break;
				case "get":
					if (!ArrayPositionCheck(1, words))
					{
						Console.WriteLine("Error: No command!");
						return;
					}
					switch (words[1].ToLower())
					{
						case "ip":
							{
								foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
								{
									if (ip.AddressFamily == AddressFamily.InterNetwork)
									{
										Console.WriteLine(ip.ToString());
									}
								}
							}
							break;
						default:
							break;
					}
					break;
				case "push":
					{
						List<PUSH.Message> m = new List<PUSH.Message>();
					PushMain:
						Console.Write("> ");
						string pushInput = Console.ReadLine();
						string[] pushWords = pushInput.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						switch (pushWords[0].ToLower())
						{
							case "send":
								{
									/*if (!ArrayPositionCheck(1, pushWords))
									{
										Console.WriteLine("Error: No command!");
										continue;
									}*/
									m.Add(new PUSH.Message(Command.SEND, pushWords));
									goto PushMain;
								}
							case "update":
								m.Add(new PUSH.Message(Command.UPDATE, pushWords));
								goto PushMain;
							case "done":
								break;
							default:
								goto PushMain;
							//break;
						}
					}
					break;
				case "remote":
					if (ArrayPositionCheck(2, words))
					{
						/*if (words[1].ToLower() == "desktop" && words[2].ToLower() == "->")
						{
							if (words[3].ToLower() == "open")
							{
								System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
								psi.FileName = guiPath;
								psi.Arguments = "-";
							}
							MyEnum temp;
							enumerators.TryGetValue("clients", out temp);
							if (temp == null) //ip cím
							{

							}
							else
							{

							}
							//RemoteGUI.MainWindow mw = new RemoteGUI.MainWindow();
							//mw.jajj();
						}*/
						if (words[1].ToLower() == "desktop")
						{
							if (words[2].ToLower() == "clients")
							{
								foreach (IPAddress address in computers.Values)
								{
									SOCKET.Client client = new SOCKET.Client();
									client.socket = new System.Net.Sockets.Socket(new System.Net.IPEndPoint(address, 6546).AddressFamily, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
									
									try
									{
											client.socket.Connect(address, 6546);
									}
									catch (System.Net.Sockets.SocketException ee)
									{
											//Console.WriteLine(e);
										Console.WriteLine("{0} -> Offline.", address.ToString());
									}
									client.pmOut.New();
									client.pmOut.Set(Command.COMPUTER_NAME_REQUEST);
									client.pmOut.End();
									client.socket.SendTimeout = 1000;
									int sent;
									try
									{
										sent = client.socket.Send(client.bufferOut, 0, client.pmOut.Size, SocketFlags.None);
									}
									catch
									{
										Console.WriteLine("{0} -> SendTimeout.", address.ToString());
									}
									int received = 0;
									client.socket.ReceiveTimeout = 1000;
									try
									{
										received = client.socket.Receive(client.bufferIn);
									}
									catch
									{
										Console.WriteLine("{0} -> ReceiveTimeout.", address.ToString());
									}
									if (received > 0)
									{
										client.pmIn.New();
										if (client.pmIn.Size == received)
										{
											Command c = client.pmIn.Read();
											if (c == Command.COMPUTER_NAME)
											{
												Console.WriteLine("{0} -> Online -> {1}", address.ToString(), client.pmIn.br.ReadString());
											}
										}
										else
										{
											Console.WriteLine("{0} -> Packet error.", address.ToString());
										}
									}
								}
							}
						}
					}


					Commands.Set(ref words);
					break;
				case "network":
					if (ArrayPositionCheck(2, words))
					{
						if (words[1] == "stream")
						{
							if (words[2] == "host")
							{
								Remote.NetworkStream ns = new Remote.NetworkStream("127.0.0.1", 6547, Remote.NetworkStream.Type.Server);
								Console.WriteLine("Test server running");
							}
							else if (words[2] == "client")
							{
								Remote.NetworkStream ns = new Remote.NetworkStream("127.0.0.1", 6547, Remote.NetworkStream.Type.Client);
								Console.WriteLine("Test client running");
								byte[] arr = new byte[1016 * 1500];
								ns.SendSimple(arr);
							}
						}

					}
					break;
				case "exit":
					foreach (var client in clients)
					{
						client.Value.socket.Shutdown(SocketShutdown.Both);
						client.Value.socket.Close();
					}
					Environment.Exit(0);
					break;
				default:
					break;
			}
		}
		public static void OnConnectionAcept(SocketAsyncEventArgs arg)
		{
			SOCKET.Client client = new SOCKET.Client();
			IPAddress address = arg.ReceiveMessageFromPacketInfo.Address;
			clients.Add(address.ToString(), client);
			client.socket = arg.AcceptSocket;
			client.StartReceiveAsync(OnReceiveAsync);
		}
		public static bool OnReceiveAsync(SocketAsyncEventArgs arg)
		{
			Commands.Get(arg.Buffer);
			return true;
		}
		public static bool ArrayPositionCheck(int pos, Array arr)
		{
			if (pos < arr.Length) return true;
			else return false;
		}
	}
	public class MyEnum
	{
		public string type;
		public int pos;
		public string[] objects;
		public void Next()
		{
			if(pos + 1 == objects.Length)
			{
				Console.WriteLine("No more values!");
			}
			else
			{
				pos++;
			}
		}
		public void Reset()
		{
			pos = 0;
		}
	}
}
