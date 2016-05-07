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
		public class Languages
		{
			[LanguageName("Hungarian")]
			public const string LanguageHU = "RemoteGUI.HU.txt";
			[LanguageName("English")]
			public const string LanguageEN = null;
		}
		[LanguageLoad]
		public static string LanguageLoad()
		{
			Assembly a = Assembly.GetExecutingAssembly();
			using (Stream resFilestream = a.GetManifestResourceStream(Languages.LanguageHU))
			{
				if (resFilestream == null) return null;
				using (StreamReader reader = new StreamReader(resFilestream))
				{
					return reader.ReadToEnd();
				}
			}
		}
		[Startup("Caching languages...", Priority=1)]
		public static void RegisterLanguages()
		{
			List<string> list = new List<string>();
			FieldInfo[] fi = typeof(Languages).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			foreach (FieldInfo item in fi)
			{
				LanguageNameAttribute ln = (LanguageNameAttribute)item.GetCustomAttribute(typeof(LanguageNameAttribute));
				list.Add(ln.Name);
			}
			Settings.s.languages = list.ToArray();
		}
	}
}
