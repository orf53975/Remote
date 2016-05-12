using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;
using System.Diagnostics;
using Remote;
using MahApps.Metro.Controls;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
namespace RemoteGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		Dictionary<string, BindingEntry> bindings = new Dictionary<string, BindingEntry>();

		public static string[] screenCaptureMethods = new string[] { "GDI", "BitBlt", "DirectX" };
		public static string[] frameBufferings = new string[] { "Disabled", "Enabled" };
		public static string[] desktopCompositions = new string[] { "Enabled", "Disabled" };
		public static string[] pixelFormats = new string[] { "32bppARGB", "24bppRGB", "16bppRGB565" };
		public static string[] framesPerSeconds = new string[] { "5", "10", "15", "20", "25", "30", "Max" };
		public static string[] compressions = new string[] { "Lossless", "Lossy" };
		public static string[] losslessCodec = new string[] { "LZ4" };
		public static string[] lossyCodec = new string[] { "ffmpeg" };
		public static string[] LZ4BlockSizes = new string[] { "32kb", "64kb", "128kb", "256kb", "512kb", "Max" };
		public static string[] autoStartServer = new string[] { "Disabled", "Enabled" };

		public List<string> serverConnectedComputerNames;
		public List<SOCKET.Client> serverConnectedComputers;
		public bool serverEmpty = true;

		SOCKET.Server server;
		//List<SOCKET.Client> client;

		RemoteDesktopHost host;

		List<StackPanel> connectedComputersItems = new List<StackPanel>();
		//Dictionary<StackPanel, ClientData> connectedComputersBinding = new Dictionary<StackPanel, ClientData>();
		Clients clients = new Clients();
		public static RemoteGuiLoader.MainWindow loader = new RemoteGuiLoader.MainWindow();
		Command lastCommand;

		ConsoleWriter cw = new ConsoleWriter(1000);
		string tempText;

		public DispatcherTimer dt;
		public int fps = 1;
		public long oldTick;
		public long newTick;
		public string sajt;
		BitmapImage bmi;
		Stopwatch sw;
		int kecske;
		int x = 1920;
		int y = 1080;
		System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
		//[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		//public static extern bool DeleteObject(IntPtr hObject);
		public MainWindow()
		{

			InitializeComponent();
			Loaded += MainWindow_Loaded;
			Closing += MainWindow_Closing;

			FlyoutsControlAll.Visibility = System.Windows.Visibility.Visible;
			ConnectedInfo.Visibility = System.Windows.Visibility.Hidden;
			
			ScreenCaptureMethodComboBox.ItemsSource = screenCaptureMethods;
			BufferingComboBox.ItemsSource = Remote.Language.Find(() => BufferingComboBox.ItemsSource, this, frameBufferings);//frameBufferings;
			CompositionComboBox.ItemsSource = Remote.Language.Find(() => CompositionComboBox.ItemsSource, this, desktopCompositions);//desktopCompositions;
			FormatComboBox.ItemsSource = pixelFormats;
			FPSComboBox.ItemsSource = framesPerSeconds;
			CompressionComboBox.ItemsSource = Remote.Language.Find(() => CompressionComboBox.ItemsSource, this, compressions);//compressions;
			CodecComboBox.ItemsSource = losslessCodec;
			LZ4BlockSizeComboBox.ItemsSource = LZ4BlockSizes;
			AutoStartServerComboBox.ItemsSource = Remote.Language.Find(() => AutoStartServerComboBox.ItemsSource, this, autoStartServer); ;

			Bind(ScreenCaptureMethodComboBox.Name, () => Settings.s.remoteDesktopSettings.screenCaptureMethod, true);
			Bind(BufferingComboBox.Name, () => Settings.s.remoteDesktopSettings.frameBuffering, true);
			Bind(CompositionComboBox.Name, () => Settings.s.remoteDesktopSettings.desktopComposition, true);
			Bind(FormatComboBox.Name, () => Settings.s.remoteDesktopSettings.pixelFormat, true);
			Bind(FPSComboBox.Name, () => Settings.s.remoteDesktopSettings.framesPerSecond, true);
			Bind(CompressionComboBox.Name, () => Settings.s.remoteDesktopSettings.compression, true);
			Bind(LZ4BlockSizeComboBox.Name, () => Settings.s.remoteDesktopSettings.LZ4BlockSize, true);
			Bind(AutoStartServerComboBox.Name, () => Settings.s.startServer, false);

			ScreenCaptureMethodComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.screenCaptureMethod;
			BufferingComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.frameBuffering;
			CompositionComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.desktopComposition;
			FormatComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.pixelFormat;
			FPSComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.framesPerSecond;
			CompressionComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.compression;
			if(CompressionComboBox.SelectedIndex == 0)
			{
				//Bind(CodecComboBox.Name, () => Settings.s.remoteDesktopSettings.losslessCodec);
				// Notes: No need to bind here, as this will be changed in custom method and not in generic one!
				CodecComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.losslessCodec;
			}
			else
			{
				//Bind(CodecComboBox.Name, () => Settings.s.remoteDesktopSettings.lossyCodec);
				// Notes: No need to bind here, as this will be changed in custom method and not in generic one!
				CodecComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.lossyCodec;
			}
			LZ4BlockSizeComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.LZ4BlockSize;


			Address.ItemsSource = Settings.s.addresses;

			if (Settings.s.startServer == 1)
			{
				server = new SOCKET.Server(Settings.s.remoteDesktopPort);
				server.Listen();
				server.StartReceiveAsync(OnConnectionAccept);
				ServerStartButton.Content = Remote.Language.Find(on => ServerStartButton.Content, this, "");
				//ServerStartButton.Content = "Stop Server";
			}
			else
			{
				server = new SOCKET.Server(Settings.s.remoteDesktopPort);
				server.Close();
				ServerStartButton.Content = Remote.Language.Find(off => ServerStartButton.Content, this, "");
				//ServerStartButton.Content = "Start Server";
			}

			UserNameTextBox.Text = Settings.s.name;
			PortTextBox.Text = Settings.s.remoteDesktopPort.ToString();

			//lServer.Content = Remote.Language.Find(() => lServer.Content, this, "Server");
			//cw.WriteLine(lServer.Name);

			//Screen.EnableComposition(false);
			sw = new Stopwatch();
			sw.Start();
			dt = new DispatcherTimer(DispatcherPriority.Send);
			dt.Tick += dt_Tick;
			dt.Interval = new TimeSpan(0, 0, 0, 0, /*1000 / fps*/ 40);
			oldTick = Environment.TickCount;
			dt.Start();
			/*Thread thread = new Thread(dt_Tick1);
			thread.Priority = ThreadPriority.Highest;
			thread.Start();*/

			/*Thread thread1 = new Thread(dt_Tick1);
			thread1.Priority = ThreadPriority.Highest;
			thread1.Start();*/

			/*Thread thread2 = new Thread(dt_Tick1);
			thread2.Priority = ThreadPriority.Highest;
			thread2.Start();*/

		}

		void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			foreach (var item in clients)
			{
				item.client.Close();
			}
			loader.Close();
		}

		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{

			// Load non english language
			foreach (var item in FindVisualChildren<Label>(this))
			{
				// If it's empty, just ignore
				if (item.Content != null)
				{
					if (item.Name != "")
					{
						item.Content = Remote.Language.Find(item.Name + ".Content", this) ?? item.Content; // Find text for the given element
						//cw.WriteLine(Remote.Language.Find(item.Name + ".Content", this));
					}
				}
				//else cw.WriteLine("error <-----------");
			}
			foreach (var item in FindVisualChildren<Button>(this))
			{
				if (item.Content != null)
				{
					// Not really important to exclude nav and PART_XXX, as we just reassign the contents.
					// This is because they dont have language files.
					// Note: ServerStartButton uses conditions, so this call would throw an exception.
					if (item.Name != "" && item.Name != "nav" && !item.Name.Contains("PART") && item.Name != "ServerStartButton")
					{
						item.Content = Remote.Language.Find(item.Name + ".Content", this) ?? item.Content;
						//cw.WriteLine(item.Name);
					}
				}
			}
			bSettings.Content = Remote.Language.Find(bSettings.Name + ".Content", this) ?? bSettings.Content;
		}

		private bool OnConnectionAccept(System.Net.Sockets.SocketAsyncEventArgs arg)
		{
			Dispatcher.Invoke(new Action(() =>
			{
				ClientData cd = new ClientData();
				cd.client = new SOCKET.Client();
				cd.client.socket = arg.AcceptSocket;

				StackPanel sp = new StackPanel();
				Label l = new Label();
				l.Content = "No name yet";
				cd.lName = l;
				sp.Children.Add(l);
				clients[sp] = cd;
				cd.sp = sp;

				cd.client.StartReceiveAsync(OnReceiveAsync);
			}), DispatcherPriority.Render);

			return true;
		}
		private bool OnReceiveAsync(System.Net.Sockets.SocketAsyncEventArgs arg)
		{
			// Get the client object from arg
			SOCKET.Client client = (SOCKET.Client)arg.UserToken;

			// Notes: First 4 bytes are the size of the message.
			// Notes: It's impossible to send only 4 bytes, as that would mean we only send the message size, which doesn't make sense , so this must be always bigger than 4 bytes
			if (arg.BytesTransferred > 4 && client.pmIn.NewRead() == arg.BytesTransferred)
			{
				lastCommand = client.pmIn.ReadCommand();
				switch (lastCommand)
				{
						// Connection attempt
					case Command.CONNECT:
						#region CONNECT
						{
							cw.WriteLine(Remote.Language.Find("IncomingConnection", this, "Incoming connection attempt from: {0}"), client.ClientAddress);
							Command c = client.pmIn.ReadCommand();
							if (c == Command.USER_NAME)
							{
								Dispatcher.Invoke(new Action(() =>
								{
									foreach (var cd in clients)
									{
										if (cd.client == client)
										{
											cd.name = client.pmIn.ReadString();
											cd.lName.Content = cd.name;

											connectedComputersItems.Add(cd.sp);
											ConnectedComputers.ItemsSource = connectedComputersItems;
											break;
										}
									}
								}),
								DispatcherPriority.Render);
								client.pmOut.New();
								client.pmOut.Write(Command.CONNECT_SUCCESS);
								client.pmOut.Write(Command.USER_NAME);
								client.pmOut.Write(Settings.s.name);
								client.pmOut.End();
								if (client.SendSafe())
								{
									cw.WriteLine(Remote.Language.Find("ClientConnectSuccess", this, "Client from {0} successfully connected!"), client.ClientAddress);
									return true;
								}
								else
								{
									cw.WriteLine(client.lastSendException.Message);
									client.Close();
									client = null;
								}
							}
						}
						break;
						#endregion
					// Success!
					case Command.CONNECT_SUCCESS:
						#region CONNECT_SUCCES
						{
							cw.WriteLine(Remote.Language.Find("ClientConnected", this, "Succesfully connected to {0}"), client.ClientAddress);
							Command c = client.pmIn.ReadCommand();
							if (c == Command.USER_NAME)
							{
								Dispatcher.Invoke(new Action(() =>
								{
									ClientData cd = new ClientData();
									cd.client = client;
									Label l = new Label();
									cd.name = client.pmIn.ReadString();
									cd.lName = l;
									l.Content = cd.name;

									StackPanel sp = new StackPanel();
									sp.Children.Add(l);
									//connectedComputersBinding.Add(sp, cd);
									clients[sp] = cd;
									connectedComputersItems.Add(sp);
									ConnectedComputers.ItemsSource = connectedComputersItems;
								}),
								DispatcherPriority.Render);
							}
							return true;
						}
						#endregion
					case Command.REMOTE_DESKTOP_REQUEST:
						#region REMOTE_DESKTOP_REQUEST
						{
							ClientData cd = clients[(SOCKET.Client)arg.UserToken];
							cd.remoteDesktopRequest = true;
							Dispatcher.Invoke(new Action(() =>
							{
								ConnectedComputers_SelectionChanged(null, null);
							}), DispatcherPriority.Render);
							cw.WriteLine(Remote.Language.Find("RemoteDesktopRequestIncoming", this, "A Remote Desktop request was received from {0} / {1}"), cd.name, cd.client.ClientAddress);
							return true;
						}
						#endregion
					case Command.REMOTE_DESKTOP_CAPTURE_INFO_REQUEST:
						#region REMOTE_DESKTOP_CAPTURE_INFO_REQUEST
						{
							Dispatcher.Invoke(new Action(() =>
								{
									this.WindowState = System.Windows.WindowState.Minimized;
									loader.Show();
								}), DispatcherPriority.Render);
							loader.ChangeText("Sending streaming settings...");

							ClientData cd = clients[(SOCKET.Client)arg.UserToken];

							cd.client.pmOut.New();
							cd.client.pmOut.Write(Command.REMOTE_DESKTOP_CAPTURE_INFO);

							ProtoBuf.Serializer.SerializeWithLengthPrefix(cd.client.pmOut.Stream, Settings.s.remoteDesktopSettings, ProtoBuf.PrefixStyle.Base128);

							cd.client.pmOut.End();

							if (cd.client.SendSafe())
							{
								return true;
							}
							else
							{

							}

						}
						break;
						#endregion
					case Command.REMOTE_DESKTOP_CAPTURE_INFO:
						#region REMOTE_DESKTOP_CAPTURE_INFO
						{
							loader.ChangeText("Creating host...");

							ClientData cd = clients[(SOCKET.Client)arg.UserToken];

							Settings.RemoteDesktopSettings s = ProtoBuf.Serializer.DeserializeWithLengthPrefix<Settings.RemoteDesktopSettings>(cd.client.pmIn.Stream, ProtoBuf.PrefixStyle.Base128);
							host = new RemoteDesktopHost(s);

							cd.client.pmOut.New();
							cd.client.pmOut.Write(Command.REMOTE_DESKTOP_HOST_READY);

							cd.client.pmOut.Write(SystemParameters.PrimaryScreenWidth);
							cd.client.pmOut.Write(SystemParameters.PrimaryScreenHeight);

							cd.client.pmOut.End();

							if (cd.client.SendSafe())
							{
								return true;
							}
							else
							{
								
							}
						}
						break;
						#endregion
					case Command.REMOTE_DESKTOP_HOST_READY:
						#region REMOTE_DESKTOP_HOST_READY
						{
							loader.ChangeText("Creating streaming connection...");
							bool whatToReturn = false;
							RemoteDesktop rd;
							Dispatcher.Invoke(new Action(() =>
							{
								rd = new RemoteDesktop();
								rd.x = (int)client.pmIn.ReadDouble();
								rd.y = (int)client.pmIn.ReadDouble();

								System.Net.IPAddress address = client.ClientAddress;

								rd.clientStream = new SOCKET.Client();
								rd.clientStream.socket = new System.Net.Sockets.Socket(new System.Net.IPEndPoint(address, Settings.s.remoteDesktopPort).AddressFamily, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
								try
								{
									rd.clientStream.socket.Connect(address, Settings.s.remoteDesktopPort);
									rd.clientStream.StartReceiveAsync(rd.OnReceiveAsyncStream);

									rd.clientStream.socket.SendTimeout = 5000;
									rd.clientStream.socket.ReceiveTimeout = 5000;

									rd.clientStream.pmOut.Write(Command.REMOTE_DESKTOP_CONNECT_STREAM);

									rd.clientStream.pmOut.End();

									if (rd.clientStream.SendSafe())
									{
										//cw.WriteLine(Remote.Language.Find("ClientConnecting", this, "Connecting to {0}"), Address.Text);
										whatToReturn = true;
									}
									else
									{
										cw.WriteLine(rd.clientStream.lastSendException.Message);
										rd.clientStream.Close();
										rd.clientStream = null;
										whatToReturn = false;
									}

									//cw.WriteLine(Remote.Language.Find("ClientConnected", this, "Succesfully conencted to {0}"), Address.Text);

								}
								catch (System.Net.Sockets.SocketException ee)
								{
									cw.WriteLine(ee.Message);
									rd.clientStream.Close();
									rd.clientStream = null;
								}
							}), DispatcherPriority.Render);
							if (whatToReturn) return true;
							else return false;
						}
						#endregion
					case Command.REMOTE_DESKTOP_CONNECT_STREAM:
						#region REMOTE_DESKTOP_CONNECT_STREAM
						{
							loader.ChangeText("Streaming connection created...");
							ClientData cd = clients[(SOCKET.Client)arg.UserToken];
							Dispatcher.Invoke(new Action(() =>
							{
								cd.sp.Children.Remove(cd.lName);
								clients.Remove(cd);
								host.clientStream = cd.client;
								host.clientStream.onReceiveAsync = host.OnReceiveAsync;
							}), DispatcherPriority.Render);

							host.clientStream.pmOut.New();
							host.clientStream.pmOut.Write(Command.REMOTE_DESKTOP_CONNECT_STREAM_SUCCESS);
							host.clientStream.pmOut.End();

							if (host.clientStream.SendSafe())
							{
								return true;
							}
							else
							{

							}
							// send success
						}
						break;
						#endregion
					case Command.REMOTE_DESKTOP_CONNECT_INPUT:
						#region REMOTE_DESKTOP_CONNECT_INPUT
						{
							loader.ChangeText("Input stream created...");
							ClientData cd = clients[(SOCKET.Client)arg.UserToken];
							Dispatcher.Invoke(new Action(() =>
							{
								cd.sp.Children.Remove(cd.lName);
								clients.Remove(cd);
								host.clientInput = cd.client;
								host.clientInput.onReceiveAsync = host.OnReceiveAsyncInput;
							}), DispatcherPriority.Render);

							host.clientInput.pmOut.New();
							host.clientInput.pmOut.Write(Command.REMOTE_DESKTOP_CONNECT_INPUT_SUCCESS);
							host.clientInput.pmOut.End();

							if (host.clientInput.SendSafe())
							{
								return true;
							}
							else
							{

							}
							// send success
						}
						break;
						#endregion
					default:
						break;
				}
			}
			//Client disconnected, clean up.
			else if (arg.BytesTransferred == 0)
			{
				switch (lastCommand)
				{
						// Error? EMPTY_COMMAND = 0
					case Command.EMPTY_COMMAND:
						break;
						// Restore MainWindow upon client fail
					case Command.REMOTE_DESKTOP_CAPTURE_INFO_REQUEST:
						{
							Dispatcher.Invoke(new Action(() =>
							{
								loader.Close();
								loader = new RemoteGuiLoader.MainWindow();
								WindowState = System.Windows.WindowState.Normal;
							}), DispatcherPriority.Render);
							cw.WriteLine(Remote.Language.Find("RemoteDekstopCaptureInfoRequestClientDisconnected", this, "The client disconnected during Remote Desktop initialization"));
						}
						break;
					default:
						break;
				}

				foreach (var item in connectedComputersItems)
				{
					
				}
				//cw.WriteLine("End");
			}
			return false;
		}
		/*protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
				bool[] b = new bool[4];
				b[0] = Keyboard.Modifiers == ModifierKeys.Control;
				b[1] = Keyboard.Modifiers == ModifierKeys.Alt;
				b[2] = e.SystemKey == Key.Delete;
				b[3] = e.SystemKey == Key.Escape;
				sajt = string.Format("Control: {0}, Alt: {1}, Delete: {2}, Escape: {3}, Modifier: {4}, SystemKey: {5}, Key: {6}", b[0], b[1], b[2], b[3], Keyboard.Modifiers, e.SystemKey, e.Key);
				e.Handled = true;
				return;
			if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.F4 ||
			   Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Escape ||
				Keyboard.Modifiers == ModifierKeys.Control && Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.Delete)
			{
				e.Handled = true;
			}
			else
			{
				base.OnPreviewKeyDown(e);
			}
		}*/
		void dt_Tick(object sender, EventArgs e)
		{
			//FPS.Content = sajt;
			//cw.WriteLine(Environment.TickCount.ToString());
			
			/*cw.WriteLine(Environment.TickCount.ToString());
			ConsoleQuickText.Text = cw.GetLastLine();
			ConsoleText.Text = cw.Update();
			ConsoleText.ScrollToEnd();*/
			//dt_Tick11();

			if (ConsoleFlyout.IsOpen)
			{
				tempText = cw.Update();
				if (tempText != null)
				{
					ConsoleText.Text = tempText;
					ConsoleText.CaretIndex = tempText.Length;
					ConsoleText.ScrollToEnd();
				}
			}
			else
			{
				ConsoleQuickText.Text = cw.GetLastLine();
			}
		}
		async void dt_Tick1()
		{
			int kecske = 0;
			int csiga = 0;
			int boci = 0;
			int count = 0;
			//int totalDiff = 0;
			Thread.Sleep(100);
			Bitmap bmpScreenCapture = new Bitmap(/*(int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight,*/x, y, format);
			BitmapImage bmi;
			byte[][] arr = new byte[2][];// = await task;
			byte[] diff = new byte[0];

			Screen.Capture(x, y, ref bmpScreenCapture, format);//1
			Task<byte[]> task = Task.Run(() => Screen.CompressImageLZ4(bmpScreenCapture));//1
			
			Screen.Capture(x, y, ref bmpScreenCapture, format);//0
			arr[1] = await task;
			//Screen.arr11 = new int[arr[1].Length / 4];
			//Task<BitmapImage> task = Screen.CompressImageAsync(bmpScreenCapture);
			task = Task.Run(() => Screen.CompressImageLZ4(bmpScreenCapture));//0


			Screen.Capture(x, y, ref bmpScreenCapture, format);//1
			//bmi = await task;

			//Screen.CompressImageAsync(bmpScreenCapture);
			arr[0] = await task;
			//Screen.arr22 = new int[arr[0].Length / 4];
			//Screen.arr = new int[Screen.arr11.Length];
			//Screen.arr0 = new byte[Screen.arr.Length * 4];
			Screen.InitReturnDifference(arr[1].Length);
			Task<byte[]> taskDiff = Task.Run(() => Screen.ReturnDifference(arr[1], arr[0], ref count));
			//task = Task.Run(() => Screen.CompressImageLZ4(bmpScreenCapture));
			//arr[0] = await task;
			//arr[2] = await taskDiff;
			//taskDiff = Task.Run(() => Screen.ReturnDifference(arr[1], arr[0]));
			task = Task.Run(() => Screen.CompressImageLZ4(bmpScreenCapture)); //1
			//arr[2] = await taskDiff;


			while (true)
			{
				newTick = Environment.TickCount;
				kecske++;
				Screen.Capture(x, y, ref bmpScreenCapture, format);
				if (!task.IsCompleted) csiga++;
				//bmi = await task;
				//task = Screen.CompressImageAsync(bmpScreenCapture);

				arr[1] = await task;
				if (!taskDiff.IsCompleted) boci++;
				diff = await taskDiff;
				//totalDiff += diff.Length;
				sajt = kecske / sw.Elapsed.TotalSeconds + ", "/* + 1000 / (newTick - oldTick) */+ ", " + (newTick - oldTick) + ", " + fps + ", " + kecske + " - " + csiga + " - " + boci/* + ", Total uncompressed/compressed diff (MB): " + count * 4 / 1048576 + "/" + *//*totalDiff * 4 / 1048576*//* + ", compressed image size: " + arr.Length*/ + ", Delta size: " + diff.Length + ", Diff count: " + count;//TimeSpan.FromTicks(newTick - oldTick).TotalSeconds;
				oldTick = newTick;
				count = 0;
				taskDiff = Task.Run(() => Screen.ReturnDifference(arr[0], arr[1], ref count));
				task = Task.Run(() => Screen.CompressImageLZ4(bmpScreenCapture));


				newTick = Environment.TickCount;
				kecske++;
				Screen.Capture(x, y, ref bmpScreenCapture, format);
				if (!task.IsCompleted) csiga++;
				arr[0] = await task;
				if (!taskDiff.IsCompleted) boci++;
				diff = await taskDiff;
				//totalDiff += diff.Length;
				sajt = kecske / sw.Elapsed.TotalSeconds + ", "/* + 1000 / (newTick - oldTick) */+ ", " + (newTick - oldTick) + ", " + fps + ", " + kecske + " - " + csiga + " - " + boci/* + ", Total uncompressed/compressed diff (MB): " + count * 4 / 1048576 + "/" + *//*totalDiff * 4 / 1048576*//* + ", compressed image size: " + arr.Length*/ + ", Delta size: " + diff.Length + ", Diff count: " + count;
				oldTick = newTick;
				count = 0;
				taskDiff = Task.Run(() => Screen.ReturnDifference(arr[1], arr[0], ref count));
				task = Task.Run(() => Screen.CompressImageLZ4(bmpScreenCapture));

			}
		}
		void dt_Tick11()
		{
			//Thread.Sleep(100);
			//IntPtr ptr;
			//while (true)
			//{
			sw.Restart();
			jololo:
				oldTick = Environment.TickCount;
				using (Bitmap
				bmpScreenCapture = new Bitmap(/*(int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight,*/1280, 720, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
				{
					using (Graphics g = Graphics.FromImage(bmpScreenCapture))
					{
						g.CopyFromScreen(0,
										 0,
										 0, 0,
										 bmpScreenCapture.Size,
										// new System.Drawing.Size(1280, 720),
										 CopyPixelOperation.SourceCopy);
					}
					//Compress(bmpScreenCapture);
					//using (FileStream ms = new FileStream(@"H:\" + DateTime.Now.Ticks + ".png", FileMode.Create))
					/*using (MemoryStream ms = new MemoryStream())
					{
						bmpScreenCapture.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
						BitmapImage bmi = new BitmapImage();
						bmi.BeginInit();
						bmi.CacheOption = BitmapCacheOption.OnLoad;
						ms.Position = 0;
						bmi.StreamSource = ms;
						bmi.EndInit();					
						Image_Test.Source = bmi;
					}*/
					//ptr = bmpScreenCapture.GetHbitmap();
					//Image_Test.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
						//ptr,
						//IntPtr.Zero,
						//Int32Rect.Empty,
						//BitmapSizeOptions.FromEmptyOptions());
					//bmpScreenCapture.Save(@"H:\" + DateTime.Now.Ticks + ".png", ImageFormat.Png);
					//DeleteObject(ptr);
				}
				newTick = Environment.TickCount;
				sajt = kecske / sw.Elapsed.TotalSeconds + ", " + ", " + (newTick - oldTick) + ", " + kecske;//TimeSpan.FromTicks(newTick - oldTick).TotalSeconds;
				kecske++;
				goto jololo;
			//}
		}
		/*public async void Compress(Bitmap picture)
		{

		}*/
		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			/*fps = (int)e.NewValue;
			if(dt != null)
			dt.Interval = new TimeSpan(0, 0, 0, 0, 1000 / fps);*/
		}
		public string Path
		{
			get
			{
				return Environment.CurrentDirectory;
			}
		}
		// Todo: Blocking call if failed to connect for about 10-15 sec
		private void Connect_Click(object sender, RoutedEventArgs e)
		{
			/*if (client != null)
			{
				cw.WriteLine(Remote.Language.Find("ClientNotNull", this, "A connection is already made!"));
				return;
			}*/

			System.Net.IPAddress address;
			if (System.Net.IPAddress.TryParse(Address.Text, out address))
			{
				SOCKET.Client client = new SOCKET.Client();
				client.socket = new System.Net.Sockets.Socket(new System.Net.IPEndPoint(address, Settings.s.remoteDesktopPort).AddressFamily, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
				try
				{
					client.socket.Connect(address, Settings.s.remoteDesktopPort);
					client.StartReceiveAsync(OnReceiveAsync);

					client.socket.SendTimeout = 5000;
					client.socket.ReceiveTimeout = 5000;

					client.pmOut.Write(Command.CONNECT);
					client.pmOut.Write(Command.USER_NAME);
					client.pmOut.Write(Settings.s.name);
					client.pmOut.End();

					if (client.SendSafe())
					{
						cw.WriteLine(Remote.Language.Find("ClientConnecting", this, "Connecting to {0}"), Address.Text);
						return;
					}
					else
					{
						cw.WriteLine(client.lastSendException.Message);
						client.Close();
						client = null;
					}
					
					//cw.WriteLine(Remote.Language.Find("ClientConnected", this, "Succesfully conencted to {0}"), Address.Text);
					
				}
				catch (System.Net.Sockets.SocketException ee)
				{
					cw.WriteLine(ee.Message);
					client.Close();
					client = null;
				}
			}
		}
		private void RemoveAddress_Click(object sender, RoutedEventArgs e)
		{
			if (Settings.s.addresses.Contains(Address.Text))
			{
				Settings.s.addresses.Remove(Address.Text);
			}
			Address.ItemsSource = null; //Without this the dropdown isn't updated, although autocompletion works, strange, maybe a bug in current Metro nuget
			Address.ItemsSource = Settings.s.addresses;
		}
		private void SaveAddress_Click(object sender, RoutedEventArgs e)
		{
			if (Settings.s.addresses.Contains(Address.Text))
			{
				
			}
			else
			{
				Settings.s.addresses.Add(Address.Text);
			}
			Address.ItemsSource = null; //Without this the dropdown isn't updated, although autocompletion works, strange, maybe a bug in current Metro nuget
			Address.ItemsSource = Settings.s.addresses;
		}
		private void ConsoleInputText_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true; //Not so important, as TextBox doesn't makes new lines.
				//Todo: merge console project
			}
			else if (e.Key == Key.Escape)
			{
				//Keyboard.ClearFocus();
				e.Handled = true;
				ConsoleFlyout.IsOpen = false;
			}
		}

		private void StatusBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ConsoleFlyout.IsOpen = true;
		}

		private void CodecComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (compressions[CompressionComboBox.SelectedIndex] == "Lossless")
			{
				if (CodecComboBox.SelectedIndex < 0)
				{
					CodecComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.losslessCodec;
				}
				else
				{
					Settings.s.remoteDesktopSettings.losslessCodec = CodecComboBox.SelectedIndex;
				}

				if (losslessCodec[Settings.s.remoteDesktopSettings.losslessCodec] == "LZ4")
				{
					CodecInfo.SelectedIndex = 0;
				}
			}
			else
			{
				if (CodecComboBox.SelectedIndex < 0)
				{
					CodecComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.lossyCodec;
				}
				else
				{
					Settings.s.remoteDesktopSettings.lossyCodec = CodecComboBox.SelectedIndex;
				}

				if (lossyCodec[Settings.s.remoteDesktopSettings.lossyCodec] == "ffmpeg")
				{
					CodecInfo.SelectedIndex = 1;
				}
			}
		}

		private void CompressionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (CompressionComboBox.SelectedIndex == 0)
			{
				CodecComboBox.ItemsSource = losslessCodec;
				// No longer needed
				//CodecComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.losslessCodec;
			}
			else
			{
				CodecComboBox.ItemsSource = lossyCodec;
				// No longer needed
				//CodecComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.lossyCodec;
			}
			//CodecComboBox_SelectionChanged(null, null); //useless here, changes made to the ItemsSource will trigger SelectionChanged method
		}

		private void ServerDisconnectButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void ServerStartButton_Click(object sender, RoutedEventArgs e)
		{
			if (server.socket != null)
			{
				server.Close();
				ServerStartButton.Content = Remote.Language.Find(off => ServerStartButton.Content, this, "Start Server");
				//ServerStartButton.Content = "Start Server";
			}
			else
			{
				server = new SOCKET.Server(Settings.s.remoteDesktopPort);
				if (server.Listen())
				{
					server.StartReceiveAsync(OnConnectionAccept);
					ServerStartButton.Content = Remote.Language.Find(on => ServerStartButton.Content, this, "Stop Server");
					//ServerStartButton.Content = "Stop Server";

				}
			}
			
			//cw.WriteLine(Remote.Language.Find(() => ServerStartButton.Content, this, ""));
		}
		//Based on: http://stackoverflow.com/questions/974598/find-all-controls-in-wpf-window-by-type
		public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						yield return (T)child;
					}

					foreach (T childOfChild in FindVisualChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}

		private void ConnectedComputers_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Only show panel if we have something selected
			if (ConnectedComputers.SelectedIndex >= 0)
			{
				StackPanel sp = connectedComputersItems[ConnectedComputers.SelectedIndex];
				//ClientData cd = connectedComputersBinding[sp];
				ClientData cd = clients[sp];

				if (ConnectedInfo.Visibility != System.Windows.Visibility.Visible)
				{
					ConnectedInfo.Visibility = System.Windows.Visibility.Visible;
				}

				AddressConnected.Content = cd.client.ClientAddress;

				if (cd.remoteDesktopRequest)
				{
					lRemoteDesktopRequest.Visibility = System.Windows.Visibility.Visible;
					RemoteDesktopRequestAcceptButton.Visibility = System.Windows.Visibility.Visible;
				}
				else
				{
					lRemoteDesktopRequest.Visibility = System.Windows.Visibility.Hidden;
					RemoteDesktopRequestAcceptButton.Visibility = System.Windows.Visibility.Hidden;
				}
			}
		}

		private void bSettings_Click(object sender, RoutedEventArgs e)
		{
			if (bSettings.IsChecked == true)
			{
				SettingsFlyout.IsOpen = true;
			}
			else
			{
				SettingsFlyout.IsOpen = false;
			}
		}

		private void GenericComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox cb = sender as ComboBox;
			// Should never be false
			if (cb != null)
			{
				//bindings[cb.Name].SetValue(bindings[cb.Name].)
				BindingEntry be = bindings[cb.Name];
				if (be.parent != null)
				{
					be.fi.SetValue(be.parent.GetValue(Settings.s), cb.SelectedIndex);
				}
				else
				{
					be.fi.SetValue(Settings.s, cb.SelectedIndex);
				}
			}
		}

		private void UserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			Settings.s.name = UserNameTextBox.Text;
		}

		private void PortTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			int temp;
			if (!Int32.TryParse(PortTextBox.Text, out temp))
			{
				// What should we do? Nothing right now :(
				return;
			}
			else
			{
				Settings.s.remoteDesktopPort = temp;
			}
		}
		private void Bind<T>(string name, Expression<Func<T>> memberExpression, bool parent)
		{
			var memberExp = memberExpression.Body as MemberExpression;
			MemberInfo memberInfo = memberExp.Member;
			switch (memberInfo.MemberType)
			{
				case MemberTypes.All:
					break;
				case MemberTypes.Constructor:
					break;
				case MemberTypes.Custom:
					break;
				case MemberTypes.Event:
					break;
				case MemberTypes.Field:
					// Save it
					if (parent)
					{
						memberExp = memberExp.Expression as MemberExpression;
						bindings[name] = new BindingEntry() { fi = memberInfo as FieldInfo, parent = memberExp.Member as FieldInfo };
					}
					else
					{
						bindings[name] = new BindingEntry() { fi = memberInfo as FieldInfo, parent = null };
					}
					break;
				case MemberTypes.Method:
					break;
				case MemberTypes.NestedType:
					break;
				case MemberTypes.Property:
					break;
				case MemberTypes.TypeInfo:
					break;
				default:
					break;
			}
		}
		private class BindingEntry
		{
			public FieldInfo fi;
			public FieldInfo parent;
		}
		public class ClientData
		{
			public string name;
			public SOCKET.Client client;

			public Label lName;
			public StackPanel sp;

			public bool remoteDesktopRequest;
		}

		private void RemoteDesktopRequestAcceptButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = System.Windows.WindowState.Minimized;
			loader.Show();
			// Do we need it?
			loader.Activate();

			loader.ChangeText("Wating for Data...");
			StackPanel sp = connectedComputersItems[ConnectedComputers.SelectedIndex];
			ClientData cd = clients[sp];

			cd.client.pmOut.New();
			cd.client.pmOut.Write(Command.REMOTE_DESKTOP_CAPTURE_INFO_REQUEST);
			cd.client.pmOut.End();

			if (cd.client.SendSafe())
			{
				// We need the set lastCommand to this, else if the connection fails midway there is no way knowing what to do
				lastCommand = Command.REMOTE_DESKTOP_CAPTURE_INFO_REQUEST;
			}
			else
			{

			}
		}
		private void RemoteDesktopConnectButton_Click(object sender, RoutedEventArgs e)
		{
			StackPanel sp = connectedComputersItems[ConnectedComputers.SelectedIndex];
			//ClientData cd = connectedComputersBinding[sp];
			ClientData cd = clients[sp];

			cd.client.pmOut.New();
			cd.client.pmOut.Write(Command.REMOTE_DESKTOP_REQUEST);
			cd.client.pmOut.End();

			if (cd.client.SendSafe())
			{
				cw.WriteLine(Remote.Language.Find("RemoteDesktopRequest", this, "Remote Desktop Request was sent to {0} at {1}"), cd.name, cd.client.ClientAddress);
				return;
			}
			else
			{
				// Should we do this here?
				/*cw.WriteLine(client.lastSendException.Message);
				client.Close();
				client = null;*/
			}
		}
		public class Clients : IEnumerable<ClientData>
		{
			private Dictionary<StackPanel, ClientData> spDic = new Dictionary<StackPanel,ClientData>();
			private Dictionary<SOCKET.Client, ClientData> scDic = new Dictionary<SOCKET.Client, ClientData>();
			public ClientData this[StackPanel sp]
			{
				get
				{
					return spDic[sp];
				}
				set
				{
					spDic[sp] = value;
					scDic[value.client] = value;
				}
			}
			public ClientData this[SOCKET.Client sc]
			{
				get
				{
					return scDic[sc];
				}
				/*set
				{
					scDic[sc] = value;
				}*/
			}
			public void Remove(ClientData cd)
			{
				spDic.Remove(cd.sp);
				scDic.Remove(cd.client);
			}
			public IEnumerator<ClientData> GetEnumerator()
			{
				foreach (ClientData item in spDic.Values)
				{
					yield return item;
				}
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
