using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Remote;

namespace RemoteGUI
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			Remote.Startup.Load();
			if (e.Args.Length == 0) //nincs parancs, normális indítás
			{
				//Settings.Load(); //No longer needed
				MainWindow mw = new MainWindow();
				mw.Show();
			}
			else if (e.Args.Length == 2)
			{
				switch (e.Args[0].ToLower())
				{
					case "-remotedesktop":
						{
							Options o = new Options(OptionsTypes.RemoteDesktop);
							o.ShowDialog();
							RemoteDesktop rd = new RemoteDesktop();

							System.Net.IPAddress address;
							if (System.Net.IPAddress.TryParse(e.Args[1], out address))
							{
								rd.client = new SOCKET.Client();
								rd.client.socket = new System.Net.Sockets.Socket(new System.Net.IPEndPoint(address, 6546).AddressFamily, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
								try
								{
									rd.client.socket.Connect(address, 6546);
								}
								catch (System.Net.Sockets.SocketException ee)
								{
									Environment.Exit(1);
									//Console.WriteLine(e);
								}
							}
							PacketManager pmOut = rd.client.pmOut;
							pmOut.New();
							pmOut.Set(Command.REMOTE_DESKTOP);
							pmOut.bw.Write(UserControls.RemoteDesktopOptions.screenCaptureMethods[o.rd.ScreenCaptureMethodComboBox.SelectedIndex]);
							pmOut.bw.Write(UserControls.RemoteDesktopOptions.frameBufferings[o.rd.BufferingComboBox.SelectedIndex]);
							pmOut.bw.Write(UserControls.RemoteDesktopOptions.desktopCompositions[o.rd.CompositionComboBox.SelectedIndex]);
							pmOut.bw.Write(UserControls.RemoteDesktopOptions.pixelFormats[o.rd.FormatComboBox.SelectedIndex]);
							pmOut.bw.Write(UserControls.RemoteDesktopOptions.framesPerSeconds[o.rd.FPSComboBox.SelectedIndex]);
							pmOut.bw.Write(UserControls.RemoteDesktopOptions.compressions[o.rd.CompressionComboBox.SelectedIndex]);
							pmOut.bw.Write(((System.Windows.Controls.ComboBoxItem)o.rd.CodecComboBox.SelectedItem).Content.ToString());
							pmOut.bw.Write(UserControls.RemoteDesktopOptions.LZ4BlockSizes[o.rd.LZ4BlockSizeComboBox.SelectedIndex]);


							bool? b = rd.ShowDialog();
						}
						break;
					default:
						break;
				}
			}
		}
	}
}
