using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Reflection;
using System.IO;

namespace Remote
{
	public static class Language
	{
		private static Dictionary<string, string> dictionary = new Dictionary<string,string>();
		[Startup("Loading language...")]
		private static void Load()
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					foreach (MethodInfo mi in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
					{
						if (mi.GetCustomAttribute(typeof(LanguageLoadAttribute)) != null)
						{
							string o = (string)mi.Invoke(null, null);
							string line;
							string entry;
							string value;
							using (StringReader reader = new StringReader(o))
							{
								while (true)
								{
									line = reader.ReadLine();
									if (line != null && line != "")
									{
										if (line[0] == '#') //comment, ignore
										{
											continue;
										}
										else
										{
											// Read the object name
											int i = line.IndexOf(' ');
											entry = line.Substring(0, i);

											// Read the value
											// Notes: No need to check for "=" as it's only for visual. From the first white space we read it to the end.
											// Todo: Remove spaces on end if any?
											i = line.IndexOf(' ', i + 1);
											value = line.Substring(i, line.Length - i);

											dictionary.Add(entry, value);
											continue;
										}
									}
									else if (line == null) break;
									else continue;
								}
							}
						}
					}
				}
			}
		}
		public static string Find<T>(Expression<Func<T>> memberExpression, object o, string original)
		{
			List<string> list = new List<string>(6); //6 should be enough
			StringBuilder sb = new StringBuilder(100); //should be enough => "qwertzuiopasdfghjklyxcvbnm" thats only 26! even if its not enough, just reallocate


			var memberExp = memberExpression.Body as MemberExpression;
			while (memberExp != null)
			{
				MemberInfo memberInfo = memberExp.Member;
				list.Add(memberInfo.Name);
				memberExp = memberExp.Expression as MemberExpression;
			}
			//sb.Append(t.Name);
			//sb.Append(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());
			sb.Append(o.GetType().Namespace);
			sb.Append('.');
			sb.Append(o.GetType().Name);
			for (int i = list.Count; i --> 0;)
			{
				sb.Append('.');
				sb.Append(list[i]);
			}

			string temp;
			if (dictionary.TryGetValue(sb.ToString(), out temp))
			{
				return temp;
			}
			else
			{
				return original;
			}
		}
		public static string Find(string name, object o)
		{
			StringBuilder sb = new StringBuilder(100);
			sb.Append(o.GetType().Namespace);
			sb.Append('.');
			sb.Append(o.GetType().Name);
			sb.Append('.');
			sb.Append(name);

			//return sb.ToString();

			string temp;
			if (dictionary.TryGetValue(sb.ToString(), out temp))
			{
				return temp;
			}
			else
			{
				return null;
			}
		}
	}
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class LanguageLoadAttribute : Attribute
	{
		public LanguageLoadAttribute()
		{
		}
	}
}
