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
namespace RemoteGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public static string[] screenCaptureMethods = new string[] { "GDI", "BitBlt", "DirectX" };
		public static string[] frameBufferings = new string[] { "Disabled", "Enabled" };
		public static string[] desktopCompositions = new string[] { "Enabled", "Disabled" };
		public static string[] pixelFormats = new string[] { "32bppARGB", "24bppRGB", "16bppRGB565" };
		public static string[] framesPerSeconds = new string[] { "5", "10", "15", "20", "25", "30", "Max" };
		public static string[] compressions = new string[] { "Lossless", "Lossy" };
		public static string[] losslessCodec = new string[] { "LZ4" };
		public static string[] lossyCodec = new string[] { "ffmpeg" };
		public static string[] LZ4BlockSizes = new string[] { "32kb", "64kb", "128kb", "256kb", "512kb", "Max" };

		public List<string> serverConnectedComputerNames;
		public List<SOCKET.Client> serverConnectedComputers;
		public bool serverEmpty = true;

		SOCKET.Server server;
		SOCKET.Client client;
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

			ScreenCaptureMethodComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.screenCaptureMethod;
			BufferingComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.frameBuffering;
			CompositionComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.desktopComposition;
			FormatComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.pixelFormat;
			FPSComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.framesPerSecond;
			CompressionComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.compression;
			if(CompressionComboBox.SelectedIndex == 0)
			{
				CodecComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.losslessCodec;
			}
			else CodecComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.lossyCodec;
			LZ4BlockSizeComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.LZ4BlockSize;

			Address.ItemsSource = Settings.s.addresses;

			if (Settings.s.startServer)
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
					// Note: ServerStartButton uses conditions, so this call would throw an exception
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
			return false;
		}
		private bool OnReceiveAsync(System.Net.Sockets.SocketAsyncEventArgs arg)
		{
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
			if (client != null)
			{
				cw.WriteLine(Remote.Language.Find("ClientNotNull", this, "A connection is already made!"));
				return;
			}

			System.Net.IPAddress address;
			if (System.Net.IPAddress.TryParse(Address.Text, out address))
			{
				client = new SOCKET.Client();
				client.socket = new System.Net.Sockets.Socket(new System.Net.IPEndPoint(address, Settings.s.remoteDesktopPort).AddressFamily, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
				try
				{
					client.socket.Connect(address, Settings.s.remoteDesktopPort);
					client.StartReceiveAsync(OnReceiveAsync);

					client.socket.SendTimeout = 2000;
					client.socket.ReceiveTimeout = 2000;

					
					
					
					
					cw.WriteLine(Remote.Language.Find("ClientConnected", this, "Succesfully conencted to {0}"), Address.Text);
					
				}
				catch (System.Net.Sockets.SocketException ee)
				{
					cw.WriteLine(ee.Message);
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
				if (losslessCodec[Settings.s.remoteDesktopSettings.losslessCodec] == "LZ4")
				{
					CodecInfo.SelectedIndex = 0;
				}
			}
			else
			{
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
				CodecComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.losslessCodec;
			}
			else
			{
				CodecComboBox.ItemsSource = lossyCodec;
				CodecComboBox.SelectedIndex = Settings.s.remoteDesktopSettings.lossyCodec;
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

		private void RemoteDesktopConnectButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void ConnectedComputers_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

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

		private void AutoStartServerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void UserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void PortTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{

		}
	}
}
