using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remote
{
	public class NetworkStream
	{
		private SOCKET.Server server;
		private SOCKET.Client client;
		private System.IO.MemoryStream ms = new System.IO.MemoryStream(1024 * 1024 * 10); //10 MB //bufferre lecserélni!
		private PacketManager pm;
		private PacketManager pmIn;
		private PacketManager pmOut;
		private Type type;
		private int count;
		private int totalBytes;
		public int size;
		private bool[] segments = new bool[300000];
		private bool[] segmentsDefault = new bool[10000];
		private int NETWORK_STREAM_SEND_START;
		private int NETWORK_STREAM_SEND;
		private int NETWORK_STREAM_SEND_END;
		private int NETWORK_STREAM_NULL;
		private int NETWORK_STREAM_SEND_CORRUPTED;
		private int NETWORK_STREAM_SEND_CORRUPTED_MAYBE;
		private int NETWORK_STREAM_RECEIVE_MORE;
		private int NETWORK_STREAM_RECEIVE_STILL_LEFT;
		private int NETWORK_STREAM_RECEIVE_MAYBE_FULL;

		private int NETWORK_STREAM_SEND_FULL;
		public NetworkStream(string ip, int port, Type type)
		{
			switch (type)
			{
				case Type.Server:
					server = new SOCKET.Server(ip, port);
					server.Listen();
					server.StartReceiveAsync(OnConnectionAccept);
					type = Type.Server;
					pm = new PacketManager(ms);
					break;
				case Type.Client:
					{
						System.Net.IPAddress address;
						if (System.Net.IPAddress.TryParse(ip, out address))
						{
							client = new SOCKET.Client();
							client.socket = new System.Net.Sockets.Socket(new System.Net.IPEndPoint(address, port).AddressFamily, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
							try
							{
								client.socket.Connect(address, port);
								//Console.WriteLine("yes");
								type = Type.Client;
								pm = new PacketManager(ms);
								pmIn = new PacketManager(client.bufferIn);
								pmOut = new PacketManager(client.bufferOut);
								//client.StartReceiveAsync(OnReceiveAsync);
							}
							catch (System.Net.Sockets.SocketException e)
							{
								Console.WriteLine(e);
							}
						}
					}
					break;
				default:
					break;
			}
		}
		public bool OnConnectionAccept(System.Net.Sockets.SocketAsyncEventArgs arg)
		{
			client = new SOCKET.Client();
			//System.Net.IPAddress address = arg.ReceiveMessageFromPacketInfo.Address;
			Console.WriteLine("Incoming connection");
			client.socket = arg.AcceptSocket;
			pmIn = new PacketManager(client.bufferIn);
			pmOut = new PacketManager(client.bufferOut);
			client.StartReceiveAsync(OnReceiveAsync);
			return false; //todo: check
		}
		public bool OnReceiveAsync(System.Net.Sockets.SocketAsyncEventArgs arg)
		{
			pmIn.New();
			Console.WriteLine("OnReceiveAsync -> {0} bytes", arg.BytesTransferred);
			switch (pmIn.Read())
			{
				case Command.NETWORK_STREAM:
					break;
				case Command.NETWORK_STREAM_ADD_COMMAND:
					break;
				case Command.NETWORK_STREAM_CONNECT:
					break;
				case Command.NETWORK_STREAM_CONNECT_SUCCESS:
					break;
				case Command.NETWORK_STREAM_LAST:
					break;
				case Command.NETWORK_STREAM_NULL:
					break;
				case Command.NETWORK_STREAM_READY:
					break;
				case Command.NETWORK_STREAM_SEND:
					break;
				case Command.NETWORK_STREAM_SEND_CORRUPTED:
					break;
				case Command.NETWORK_STREAM_SEND_INIT:
					Console.WriteLine("NETWORK_STREAM_SEND_INIT");
					Command c = pmIn.Read();
					Console.WriteLine(c);
					if (c == Command.NETWORK_STREAM_SEND_SIMPLE)
					{
						ReceiveSimple();
					}
					else Receive();
					break;
				case Command.NETWORK_STREAM_SEND_LAST:
					break;
				case Command.NETWORK_STREAM_SEND_SUCCESS:
					break;
				case Command.NETWORK_STREAM_SEND_WAS_LAST:
					break;
				default:
					pmIn.New();
					Console.WriteLine(pmIn.Read());
					break;
			}
			return true;
		}
		public bool Send(byte[] arr, Command onDone)
		{
			int i = 0;
			int prevI = 0;
			int segment = 0;
			int buffer = /*1024;//*/4096;
			int sendResult;
			int receiveResult;
			bool done = false;
			pm.New();
			pm.Set(Command.NETWORK_STREAM_SEND_INIT);
			//pm.Set(Command.NETWORK_STREAM_SEND_SAFE);
			pm.bw.Write(arr.Length);
			pm.Set(Command.NETWORK_STREAM_ADD_COMMAND);
			pm.Set(onDone);
			pm.End();
			sendResult = client.socket.Send(ms.GetBuffer(), 0, (int)ms.Position, System.Net.Sockets.SocketFlags.None);
			receiveResult = client.socket.Receive(client.bufferIn);
			Console.WriteLine("Received: {0} -> {1}", receiveResult, pmIn.Size);
			pmIn.New();
			Console.WriteLine(pmIn.Read());
			pmIn.New();
			if (receiveResult != 0 && pmIn.Size == receiveResult && pmIn.Read() == Command.NETWORK_STREAM_READY)
			{
				while (true)
				{
					count++;
					if (count == 1000)
					{
						Console.WriteLine(totalBytes / 1024 / 1024);
						count = 0;
					}
					totalBytes += buffer;
					pm.New();
					if (i + buffer - 1 < arr.Length)//<-----
					{
						pm.Set(Command.NETWORK_STREAM_SEND);
						//pm.bw.Write(segment);
						pm.bw.Write(buffer);
						pm.bw.Write(i);
						pm.Set(Command.NETWORK_STREAM_SEND_START);
						pm.bw.Write(arr, i, buffer);
						pm.Set(Command.NETWORK_STREAM_SEND_END);
						prevI = i;
						//i += buffer; //<---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
						pm.Set(Command.NETWORK_STREAM_NULL);
						pm.End();
					}
					else if (i >= arr.Length)
					{
						pm.Set(Command.NETWORK_STREAM_SEND_WAS_LAST);
						pm.End();
						done = true;
					}
					else
					{
						pm.Set(Command.NETWORK_STREAM_SEND);
						//pm.bw.Write(segment);
						pm.bw.Write(arr.Length - buffer);
						pm.Set(Command.NETWORK_STREAM_SEND_START);
						pm.bw.Write(arr, i, arr.Length - buffer);
						pm.Set(Command.NETWORK_STREAM_SEND_END);
						prevI = i;
						i = arr.Length;
						pm.Set(Command.NETWORK_STREAM_LAST);
						pm.Set(Command.NETWORK_STREAM_NULL);
						pm.End();
						done = true;
					}
					try
					{
						sendResult = client.socket.Send(ms.GetBuffer(), 0, (int)ms.Position, System.Net.Sockets.SocketFlags.None);
						if (sendResult != buffer + 24)
						{
							Console.WriteLine("ERROR");
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
					try
					{
						receiveResult = client.socket.Receive(client.bufferIn);
						pmIn.ms.Position = 0;
						if (receiveResult != 0 && pmIn.Size == receiveResult)
						{
							/*if (pmIn.Read() == Command.NETWORK_STREAM_READY) continue;
							else if (pmIn.Read() == Command.NETWORK_STREAM_SEND_CORRUPTED)
							{
								//segment--;
								done = false;
								i = prevI;
								continue;
							}*/
							switch (pmIn.Read())
							{
								case Command.NETWORK_STREAM_READY:
									
									continue;
								case Command.NETWORK_STREAM_SEND_CORRUPTED:
									done = false;
									i = prevI;
									continue;
							}
						}
					}
					catch (Exception ex)
					{

					}
					if (done) break;
					segment++;
				}
				return true;
			}
			else
			{
				if (receiveResult == 0)
				{

				}
			}
			return false;
		}
		public void Receive()
		{
			int i;
			int buffer = 0;
			bool loop = true;
			//int segment;

			//int length = pmIn.br.ReadInt32();
			//pmIn.ms.Position += 4;
			pmIn.New();
			Command c = pmIn.Read();
			int sendResult;
			int receiveResult;
			pm.New();
			pm.Set(Command.NETWORK_STREAM_READY);
			pm.End();
			Console.WriteLine(pm.Size);
			sendResult = client.socket.Send(ms.GetBuffer(), 0, (int)ms.Position, System.Net.Sockets.SocketFlags.None);
			pm.New();
			while (true)
			{
				pmIn.New();
				pmOut.New();
				if (client.socket.Available - 4120 > 0)
				{
					NETWORK_STREAM_RECEIVE_MORE++;
				}
				receiveResult = client.socket.Receive(client.bufferIn);
				if (client.socket.Available > 0)
				{
					if (client.socket.Available + receiveResult == 4120) NETWORK_STREAM_RECEIVE_MAYBE_FULL++;
					else NETWORK_STREAM_RECEIVE_STILL_LEFT++;
				}
			Check:
				if (receiveResult != 0 && pmIn.Size == receiveResult)
				{
					while (loop)
					{
						switch (pmIn.Read())
						{				
							case Command.NETWORK_STREAM_SEND:
								//segment = pmIn.br.ReadInt32();
								buffer = pmIn.br.ReadInt32();
								pm.ms.Position = pmIn.br.ReadInt32() + 4;////////////////
								NETWORK_STREAM_SEND++;
								break;
							case Command.NETWORK_STREAM_SEND_START:
								pm.bw.Write(pmIn.br.ReadBytes(buffer));
								//pm.ms.Position = 4L;
								count++;
								if (count == 5000)
								{
									Console.Clear();
									Console.WriteLine("NETWORK_STREAM_SEND: {0}\nNETWORK_STREAM_SEND_START: {1}\nNETWORK_STREAM_SEND_END: {2}\nNETWORK_STREAM_NULL: {3}\nNETWORK_STREAM_SEND_CORRUPTED: {4}\nNETWORK_STREAM_SEND_CORRUPTED_MAYBE: {5}\nNETWORK_STREAM_RECEIVE_MORE: {6}\nNETWORK_STREAM_RECEIVE_MAYBE_FULL: {7}\nNETWORK_STREAM_RECEIVE_STILL_LEFT: {8}", NETWORK_STREAM_SEND, NETWORK_STREAM_SEND_START, NETWORK_STREAM_SEND_END, NETWORK_STREAM_NULL, NETWORK_STREAM_SEND_CORRUPTED, NETWORK_STREAM_SEND_CORRUPTED_MAYBE, NETWORK_STREAM_RECEIVE_MORE, NETWORK_STREAM_RECEIVE_MAYBE_FULL, NETWORK_STREAM_RECEIVE_STILL_LEFT);//Console.WriteLine(totalBytes / 1024 / 1024);
									count = 0;
								}
								totalBytes += buffer;
								NETWORK_STREAM_SEND_START++;
								break;
							case Command.NETWORK_STREAM_SEND_END:
								NETWORK_STREAM_SEND_END++;
								break;
							case Command.NETWORK_STREAM_NULL:
								pmOut.Set(Command.NETWORK_STREAM_READY);
								pmOut.End();

								try
								{
									sendResult = client.socket.Send(client.bufferOut, 0, pmOut.Size, System.Net.Sockets.SocketFlags.None);
								}
								catch
								{

								}
								loop = false;
								NETWORK_STREAM_NULL++;
								break;
							case Command.NETWORK_STREAM_SEND_LAST:
							case Command.NETWORK_STREAM_SEND_WAS_LAST:
								goto Break;
							default:
								break;
						}
					}
					loop = true;
				}
				else if (receiveResult != 0)
				{
					if (receiveResult > pmIn.Size)
					{
						//Console.WriteLine("error");
					}
					if (client.socket.Available > 0)
					{
						int receiveResult1 = client.socket.Receive(client.bufferIn, receiveResult, client.bufferIn.Length - receiveResult, System.Net.Sockets.SocketFlags.None);
						if (receiveResult1 == 0)
						{
							//Console.WriteLine("error");
							count++;
							if (count == 5000)
							{
								Console.Clear();
								Console.WriteLine("NETWORK_STREAM_SEND: {0}\nNETWORK_STREAM_SEND_START: {1}\nNETWORK_STREAM_SEND_END: {2}\nNETWORK_STREAM_NULL: {3}\nNETWORK_STREAM_SEND_CORRUPTED: {4}\nNETWORK_STREAM_SEND_CORRUPTED_MAYBE: {5}\nNETWORK_STREAM_RECEIVE_MORE: {6}\nNETWORK_STREAM_RECEIVE_MAYBE_FULL: {7}\nNETWORK_STREAM_RECEIVE_STILL_LEFT: {8}", NETWORK_STREAM_SEND, NETWORK_STREAM_SEND_START, NETWORK_STREAM_SEND_END, NETWORK_STREAM_NULL, NETWORK_STREAM_SEND_CORRUPTED, NETWORK_STREAM_SEND_CORRUPTED_MAYBE, NETWORK_STREAM_RECEIVE_MORE, NETWORK_STREAM_RECEIVE_MAYBE_FULL, NETWORK_STREAM_RECEIVE_STILL_LEFT);
								count = 0;
							}
						}
						NETWORK_STREAM_SEND_CORRUPTED_MAYBE++;
						goto Check;
					}

					pmOut.Set(Command.NETWORK_STREAM_SEND_CORRUPTED);
					pmOut.End();
					try
					{
						sendResult = client.socket.Send(client.bufferOut, 0, pmOut.Size, System.Net.Sockets.SocketFlags.None);
					}
					catch
					{

					}
					NETWORK_STREAM_SEND_CORRUPTED++;
				}
			}
		Break: ;
			

		}
		public void SendSimple(byte[] arr)
		{
			int i = 0;
			int prevI = 0;
			int segment = 0;
			int buffer = /*1024;//*//*4092*//*8188*//*arr.Length*/1016;
			int sendResult;
			int receiveResult;
			bool done = false;
			int pos = 0;

			int maxPos = arr.Length / buffer * buffer;
			bool lastOne;
			int lastData = 0;
			if (arr.Length - maxPos != 0)
			{
				lastOne = false;
				lastData = arr.Length - maxPos;
			}
			else lastOne = true;
			int maxSegments;
			if (!lastOne)
			{
				maxSegments = maxPos / buffer + 1;
			}
			else maxSegments = maxPos / buffer;
			Console.WriteLine("arr.Length: " + arr.Length + ", maxPos: " + maxPos + ", lastOne: " + lastOne + ", lastData: " + lastData + ", maxSegments: " + maxSegments);

			pm.New();
			pm.Set(Command.NETWORK_STREAM_SEND_INIT);
			pm.Set(Command.NETWORK_STREAM_SEND_SIMPLE);
			pm.bw.Write(arr.Length);
			pm.bw.Write(buffer/* + 4 + 4*/);
			//pm.Set(Command.NETWORK_STREAM_ADD_COMMAND);
			//pm.Set(onDone);
			pm.bw.Write(maxPos);
			pm.bw.Write(lastOne);
			pm.bw.Write(lastData);
			pm.bw.Write(maxSegments);
			pm.End();
			sendResult = client.socket.Send(ms.GetBuffer(), 0, (int)ms.Position, System.Net.Sockets.SocketFlags.None);
			receiveResult = client.socket.Receive(client.bufferIn);
			Console.WriteLine("Received: {0} -> {1}", receiveResult, pmIn.Size);
			pmIn.New();
			Console.WriteLine(pmIn.Read());
			pmIn.New();
			if (receiveResult != 0 && pmIn.Size == receiveResult && pmIn.Read() == Command.NETWORK_STREAM_READY)
			{
				while (pos < maxPos)
				{
					pm.New();
					//pm.
					//pm.ms.Position = 4L;
					pm.bw.Write(segment);
					pm.bw.Write(arr, pos, buffer);
					pm.End();
					//pos = (int)pm.ms.Position;
					//pm.ms.Position = 0L;
					//pm.bw.Write(pos);
					sendResult = client.socket.Send(ms.GetBuffer(), 0, (int)ms.Position/*pos*/, System.Net.Sockets.SocketFlags.None);
					pos += buffer;
					segment++;
					//Console.WriteLine(segment);
					//System.Threading.Thread.Sleep(1);
					/*if (sendResult == 0)
					{
						NETWORK_STREAM_SEND_FULL++;
					}
					else NETWORK_STREAM_SEND++;
					count++;
					if (count == 5000)
					{
						Console.Clear();
						Console.WriteLine("NETWORK_STREAM_SEND: {0}\nNETWORK_STREAM_SEND_FULL: {1}", NETWORK_STREAM_SEND, NETWORK_STREAM_SEND_FULL);
						count = 0;
					}*/
					//NETWORK_STREAM_SEND++;
				}
				if (!lastOne)
				{
					pm.New();
					pm.bw.Write(segment);
					pm.bw.Write(arr, pos, lastData);
					int temp = buffer - lastData;
					byte[] tempArr = new byte[temp];
					pm.bw.Write(tempArr, 0, temp);
					pm.End();
					Console.WriteLine(pm.Size);
					sendResult = client.socket.Send(ms.GetBuffer(), 0, (int)ms.Position, System.Net.Sockets.SocketFlags.None);
				}
				Console.WriteLine("Done");
			}
			/*else
			{
				if (receiveResult == 0)
				{

				}
			}*/
			//return false;
		}
		public void ReceiveSimple()
		{
			//int i;
			int buffer = 0;
			bool loop = true;
			int receivePos = 0;
			int size = 0;
			int fileSize;
			int max = 1024;//8192;
			int totalBytes = 0;
			int gotBytes = 0;
			int bytesSentBetweenTicks = 0;
			int firstTick;
			int oldTick;
			int newTick;
			int segment = 0;
			int maxPos;
			bool lastOne;
			int lastData;
			int maxSegments;
			int maxSegmentsFast;
			int segmentCounter = 0;
			int segmentDuplicate = 0;
			int countFast = 10000 / (max / 1024);
			ProgressBar progressBar;
			int temp = 0;
			int number = 0;
			//int pmInPos = 0;
			//int segment;

			//int length = pmIn.br.ReadInt32();
			//pmIn.ms.Position += 4;
			//pmIn.New();
			//Command c = pmIn.Read();
			fileSize = pmIn.br.ReadInt32();
			buffer = pmIn.br.ReadInt32();
			maxPos = pmIn.br.ReadInt32();
			lastOne = pmIn.br.ReadBoolean();
			lastData = pmIn.br.ReadInt32();
			maxSegments = pmIn.br.ReadInt32();
			if (!lastOne) maxSegmentsFast = maxSegments - 1;
			else maxSegmentsFast = maxSegments;
			int sendResult;
			int receiveResult = 0;
			int prevResult = 0;
			pm.New();
			pm.Set(Command.NETWORK_STREAM_READY);
			pm.End();
			//Console.WriteLine(pm.Size);
			sendResult = client.socket.Send(ms.GetBuffer(), 0, (int)ms.Position, System.Net.Sockets.SocketFlags.None);
			pm.New();
			pmIn.ms.Position = 0L;
			oldTick = Environment.TickCount;
			firstTick = oldTick;
			Console.Clear();
			while (true)
			{
				if (client.socket.Available >= max)
					receiveResult = client.socket.Receive(client.bufferIn, prevResult, max - prevResult, System.Net.Sockets.SocketFlags.None);
				else continue;
				/*if (receiveResult == 588)
				{
					Console.WriteLine();
				}*/
				//todo: 0 byte ellenőrzés!!!!!!! <---
				if (receiveResult != 0)
				{
					size = pmIn.br.ReadInt32();
					if (size == 0) Console.WriteLine("pmIn.ms.Position: {0}\nprevResult: {1}\nreceivePos: {2}", pmIn.ms.Position, prevResult, receivePos);
				}
				if (receiveResult != 0 && receiveResult >= size)
				{
					prevResult += receiveResult;
					while (receivePos + /*buffer*/size <= prevResult)
					{
						segmentCounter++;
						segment = pmIn.br.ReadInt32();
						//Console.WriteLine(segment);
						if (segments[segment])
						{
							//Console.WriteLine("{0} is already present", segment);
							segmentDuplicate++;
						}
						segments[segment] = true;
						ms.Position = segment * buffer;
						pm.bw.Write(client.bufferIn, receivePos + 8, size - 8);
						//pm.ms.Position = 0L; //Test fix -> később kiszedni!
						receivePos += size;
						pmIn.ms.Position = receivePos;
						totalBytes += size;
						gotBytes += 1016;
						bytesSentBetweenTicks++;
						NETWORK_STREAM_SEND++;
						if(receivePos != max)
						{
							//Console.WriteLine("error?");
							size = pmIn.br.ReadInt32(); //check hogy nem e a stream vége!
						}
						if (size == 0) break;
					}
					if (receivePos == prevResult)
					{
						receivePos = 0;
						prevResult = 0;

						pmIn.ms.Position = 0L;
					}
					else
					{
						NETWORK_STREAM_RECEIVE_STILL_LEFT++;
					}
				}
				if (segmentCounter == maxSegments)//todo: last block!
				{
					//Console.Clear();
					Console.WriteLine("*************************************************\nStarting error checking...");
					temp = 0;
					for (int i = 0; i < maxSegments; i++)
					{
						if (segments[i] == false)
						{
							Console.WriteLine(i);
							temp++;
						}
					}
					if (!lastOne)
					{
						gotBytes -= 1016 - lastData;
						//gotBytes += lastData;
					}
					break;
				}
				count++;
				if (count == countFast)
				{
					newTick = Environment.TickCount;
					number = (int)(gotBytes / (float)fileSize * 50);
					Console.Clear();
					Console.WriteLine("NETWORK_STREAM_SEND: {0}\nNETWORK_STREAM_RECEIVE_STILL_LEFT: {1}\nTotal megabytes sent: {2} mb ({3} mb/s)\n{4}", NETWORK_STREAM_SEND, NETWORK_STREAM_RECEIVE_STILL_LEFT, totalBytes / 1048576, (bytesSentBetweenTicks * buffer / 1048576) / ((newTick - oldTick) / 1000f), new string('*', number));//Console.WriteLine(totalBytes / 1024 / 1024);
					count = 0;
					oldTick = newTick;
					bytesSentBetweenTicks = 0;
				}
			}
			Console.Clear();
			newTick = Environment.TickCount;
			number = (int)(gotBytes / (float)fileSize * 50);
			Console.Clear();
			Console.WriteLine("NETWORK_STREAM_SEND: {0}\nNETWORK_STREAM_RECEIVE_STILL_LEFT: {1}\nTotal megabytes sent: {2} mb ({3} mb/s)\n{4}", NETWORK_STREAM_SEND, NETWORK_STREAM_RECEIVE_STILL_LEFT, totalBytes / 1048576, (bytesSentBetweenTicks * buffer / 1048576) / ((newTick - oldTick) / 1000f), new string('*', number));//Console.WriteLine(totalBytes / 1024 / 1024);
			count = 0;
			oldTick = newTick;
			bytesSentBetweenTicks = 0;
			Console.WriteLine("{0} duplicates found.", segmentDuplicate);
			if (temp == 0 && fileSize == gotBytes) Console.WriteLine("End of error check, no error found.");
			else if (temp == 1) Console.WriteLine("End of error check, 1 error found.");
			else if (temp == 0 && fileSize != gotBytes) Console.WriteLine("End of error check. Warning! File size does not match!\nExpected: {0}, got {1}", fileSize, gotBytes);
			else Console.WriteLine("End of error check, {0} errors found.", temp);
			Console.WriteLine("Took {0} seconds, with an average of {1} mb/s.", (Environment.TickCount - firstTick) / 1000f, (totalBytes / 1048576) / ((Environment.TickCount - firstTick) / 1000f));
		}
		private System.Net.Sockets.Socket GetSocket(Type type)
		{
			if(type == Type.Client) return client.socket;
			else return server.socket;
		}
		public enum Type
		{
			Server, Client
		}
	}
}
