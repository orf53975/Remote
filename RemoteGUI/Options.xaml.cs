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

namespace RemoteGUI
{
	/// <summary>
	/// Interaction logic for Options.xaml
	/// </summary>
	public partial class Options : Window
	{
		/*public Options()
		{
			InitializeComponent();
		}*/
		public UserControls.RemoteDesktopOptions rd;
		private bool single;
		public Options(OptionsTypes t)
		{
			switch (t)
			{
				case OptionsTypes.Normal:
					break;
				case OptionsTypes.RemoteDesktop:
					rd = new UserControls.RemoteDesktopOptions(true, this);
					uc = rd;
					single = true;
					break;
				default:
					break;
			}
		}
	}
	public enum OptionsTypes
	{
		Normal, RemoteDesktop
	}
}
