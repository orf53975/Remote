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

namespace RemoteGuiLoader
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		Action empty = delegate { };
		public MainWindow()
		{
			InitializeComponent();
		}
		public void ChangeText(string text)
		{
			ProgressText.Content = text;
			Dispatcher.Invoke(empty , System.Windows.Threading.DispatcherPriority.Render);
		}
	}
}
