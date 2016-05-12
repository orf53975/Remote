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
using System.Windows.Shapes;
using Remote;
using System.Net.Sockets;

namespace RemoteGUI
{
	/// <summary>
	/// Interaction logic for RemoteDesktop.xaml
	/// </summary>
	public partial class RemoteDesktop : System.Windows.Window
	{
		public SOCKET.Client clientStream;
		public SOCKET.Client clientInput;

		public int x;
		public int y;
		
		
		public RemoteDesktop()
		{
			InitializeComponent();
		}

		public bool OnReceiveAsyncStream(SocketAsyncEventArgs arg)
		{
			if (arg.BytesTransferred > 4 && clientStream.pmIn.NewRead() == arg.BytesTransferred)
			{
				switch (clientStream.pmIn.ReadCommand())
				{
					case Command.REMOTE_DESKTOP_CONNECT_STREAM_SUCCESS:
						{
							MainWindow.loader.ChangeText("Creating input stream...");
							System.Net.IPAddress address = clientStream.ClientAddress;

							clientInput = new SOCKET.Client();
							clientInput.socket = new System.Net.Sockets.Socket(new System.Net.IPEndPoint(address, Settings.s.remoteDesktopPort).AddressFamily, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
							try
							{
								clientInput.socket.Connect(address, Settings.s.remoteDesktopPort);
								clientInput.StartReceiveAsync(OnReceiveAsyncInput);

								clientInput.socket.SendTimeout = 5000;
								clientInput.socket.ReceiveTimeout = 5000;

								clientInput.pmOut.Write(Command.REMOTE_DESKTOP_CONNECT_INPUT);

								clientInput.pmOut.End();

								if (clientInput.SendSafe())
								{
									//cw.WriteLine(Remote.Language.Find("ClientConnecting", this, "Connecting to {0}"), Address.Text);
									return true;
								}
								else
								{
									//cw.WriteLine(clientInput.lastSendException.Message);
									clientInput.Close();
									clientInput = null;
									return false;
								}

								//cw.WriteLine(Remote.Language.Find("ClientConnected", this, "Succesfully conencted to {0}"), Address.Text);

							}
							catch (System.Net.Sockets.SocketException ee)
							{
								//cw.WriteLine(ee.Message);
								clientInput.Close();
								clientInput = null;
							}
						}
						break;
					default:
						break;
				}
			}
			return false;
		}

		private bool OnReceiveAsyncInput(SocketAsyncEventArgs arg)
		{
			if (arg.BytesTransferred > 4 && clientInput.pmIn.NewRead() == arg.BytesTransferred)
			{
				switch (clientInput.pmIn.ReadCommand())
				{
					case Command.REMOTE_DESKTOP_CONNECT_INPUT_SUCCESS:
						{
							Dispatcher.Invoke(new Action(() =>
							{
								Show();
							}), System.Windows.Threading.DispatcherPriority.Render);
						}
						break;
					default:
						break;
				}
			}

			return false;
		}

	}
}
