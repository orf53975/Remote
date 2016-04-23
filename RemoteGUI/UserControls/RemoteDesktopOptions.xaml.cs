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

namespace RemoteGUI.UserControls
{
	/// <summary>
	/// Interaction logic for RemoteDesktopOptions.xaml
	/// </summary>
	public partial class RemoteDesktopOptions : UserControl
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
		public int fps;
		public bool result;
		private Window parent;
		public RemoteDesktopOptions()
		{
			InitializeComponent();
		}
		public RemoteDesktopOptions(bool onItsOwn, Window parent)
		{
			InitializeComponent();
			this.parent = parent;
			if (onItsOwn)
			{
				//System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
			}
			else
			{

			}
			ScreenCaptureMethodComboBox.ItemsSource = screenCaptureMethods;
			BufferingComboBox.ItemsSource = frameBufferings;
			CompositionComboBox.ItemsSource = desktopCompositions;
			FormatComboBox.ItemsSource = pixelFormats;
			FPSComboBox.ItemsSource = framesPerSeconds;
			CompressionComboBox.ItemsSource = compressions;
			CodecComboBox.ItemsSource = losslessCodec;
			LZ4BlockSizeComboBox.ItemsSource = LZ4BlockSizes;
		}

		private void ScreenCaptureMethodComboBox_Loaded(object sender, RoutedEventArgs e)
		{

		}

		private void ScreenCaptureMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			result = true;
			parent.Close();
		}
	}
}
