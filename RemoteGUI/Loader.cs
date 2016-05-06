using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remote;
using System.Reflection;
using System.IO;

namespace RemoteGUI
{
	public static class Loader
	{
		public const string LanguageHU = "RemoteGUI.HU.txt";
		[LanguageLoad]
		public static string LanguageLoad()
		{
			Assembly a = Assembly.GetExecutingAssembly();
			using (Stream resFilestream = a.GetManifestResourceStream(LanguageHU))
			{
				if (resFilestream == null) return null;
				using (StreamReader reader = new StreamReader(resFilestream))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
