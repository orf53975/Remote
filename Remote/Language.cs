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
		private static Dictionary<string, LanguageEntry> dictionary = new Dictionary<string,LanguageEntry>();
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

											i = line.IndexOf('=');

											// Check if it's an array
											if (i + 4 < line.Length && line[i + 1] == '>' && line[i + 2] == ' ' && line[i + 3] == '(' && line[i + 4] == ')')
											{
												LanguageEntry le = new LanguageEntry();
												List<string> list = new List<string>();
												int iLast;
												while (true)
												{
													line = reader.ReadLine();
													if (line.IndexOf("() => end") > 0)
													{
														le.text = list.ToArray();
														dictionary.Add(entry, le);
														goto End;
													}
													i = 0;
													while (line[i] == '\t')
													{
														i++;
													}
													while (true)
													{
														iLast = line.IndexOf('=', i);
														if (iLast > 0 && line[iLast + 1] == '>')
														{
															list.Add(line.Substring(i, iLast - i - 1));
														}
														else // Last one, read till the end
														{
															list.Add(line.Substring(i));
															break;
														}
														i = iLast + 3; // We skip "=> "
													}
												}
											}

											// Check if it's multi conditional
											if (i + 1 < line.Length && line[i + 1] == '>')
											{
												LanguageEntry le = new LanguageEntry();
												string condition;
												while (true)
												{
													line = reader.ReadLine();
													// Should never happen
													if (line == null)
													{
														goto End;
													}
													if (line.IndexOf("() => end") > 0)
													{
														dictionary.Add(entry, le);
														goto End;
													}
													i = line.IndexOf('(');
													condition = line.Substring(i + 1, line.IndexOf(')', i) - i - 1);
													i = line.IndexOf('=');
												Loop:
													if (i + 1 < line.Length && line[i + 1] == ' ')
													{
														i++;
														goto Loop;
													}
													if (i + 1 < line.Length)
													{
														value = line.Substring(i + 1);
														le[condition] = value;
													}
													else le[condition] = "";
												}

											}

											// Read the value
											// Notes: No need to check for "=" here. From the first white space we read it to the end.
											// Todo: Remove spaces on end if any?
											i = line.IndexOf(' ', i + 1);
											value = line.Substring(i + 1);

											dictionary.Add(entry, new LanguageEntry(value));
										End:
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
		public static string[] Find<T>(Expression<Func<T>> memberExpression, object o, string[] original)
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
			for (int i = list.Count; i-- > 0; )
			{
				sb.Append('.');
				sb.Append(list[i]);
			}

			LanguageEntry temp;
			if (dictionary.TryGetValue(sb.ToString(), out temp))
			{
				if (temp.text.Length == original.Length)
					return temp.text;
				else
					return original;
			}
			else
			{
				return original;
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

			LanguageEntry temp;
			if (dictionary.TryGetValue(sb.ToString(), out temp))
			{
				return temp[null];
			}
			else
			{
				return original;
			}
		}
		public static string Find<T>(Expression<Func<string, T>> memberExpression, object o, string original)
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
			for (int i = list.Count; i-- > 0; )
			{
				sb.Append('.');
				sb.Append(list[i]);
			}

			LanguageEntry temp;
			if (dictionary.TryGetValue(sb.ToString(), out temp))
			{
				return temp[memberExpression.Parameters[0].Name];
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

			LanguageEntry temp;
			if (dictionary.TryGetValue(sb.ToString(), out temp))
			{
				return temp[null];
			}
			else
			{
				return null;
			}
		}
		public static string Find(string name, object o, string original, [CallerMemberName]string method = "")
		{
			StringBuilder sb = new StringBuilder(100);
			sb.Append(o.GetType().Namespace);
			sb.Append('.');
			sb.Append(o.GetType().Name);
			sb.Append('.');
			sb.Append(method);
			sb.Append('.');
			sb.Append(name);

			//return sb.ToString();

			LanguageEntry temp;
			if (dictionary.TryGetValue(sb.ToString(), out temp))
			{
				return temp[null];
			}
			else
			{
				return original;
			}
		}
		private class LanguageEntry
		{
			public Dictionary<string, string> dictionary;
			private bool multi;
			public string[] text;
			public LanguageEntry(string s)
			{
				text = new string[1];
				text[0] = s;
				multi = false;
			}
			public LanguageEntry()
			{
				dictionary = new Dictionary<string, string>();
				multi = true;
			}
			public string this[string s]
			{
				get
				{
					if (s != null)
					{
						string temp;
						if (dictionary.TryGetValue(s, out temp))
						{
							return temp;
						}
						else { return null; }
					}
					else
					{
						//if(text.Length == 1)
						return text[0];
					}
				}
				set
				{
					if (s == null) return;
					dictionary[s] = value;
				}
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
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class LanguageNameAttribute : Attribute
	{
		readonly string name;

		public LanguageNameAttribute(string name)
		{
			this.name = name;
		}

		public string Name
		{
			get { return name; }
		}
	}
}
