using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Remote
{
	public static class Startup
	{
		/// <summary>
		///	Invokes every method that has the StartupAttribute
		/// </summary>
		public static void Load()
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					foreach (MethodInfo mi in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
					{
						if (mi.GetCustomAttribute(typeof(StartupAttribute)) != null)
						{
							mi.Invoke(null, null);
						}
					}
				}
			}
		}
		/// <summary>
		///	Invokes every method that has the StartupAttribute. It will call the parameter method each time to update text
		/// </summary>
		public static void Load(Action<string> action)
		{
			SortedDictionary<int, Queue<MethodInfo>> dic = new SortedDictionary<int, Queue<MethodInfo>>();
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					foreach (MethodInfo mi in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
					{
						StartupAttribute sa = (StartupAttribute)mi.GetCustomAttribute(typeof(StartupAttribute));
						if (sa != null)
						{
							if (dic.ContainsKey(sa.Priority))
							{
								dic[sa.Priority].Enqueue(mi);
							}
							else
							{
								dic[sa.Priority] = new Queue<MethodInfo>();
								dic[sa.Priority].Enqueue(mi);
							}
						}
					}
				}
			}
			foreach (Queue<MethodInfo> item in dic.Values)
			{
				foreach (MethodInfo mi in item)
				{
					StartupAttribute sa = (StartupAttribute)mi.GetCustomAttribute(typeof(StartupAttribute));
					// Should always be true, as we dont add an item if this was null before
					if (sa != null)
					{
						if(sa.Text != null)
						{
							action(sa.Text);
						}
						mi.Invoke(null, null);
					}

				}
			}
		}
	}
	/// <summary>
	/// Registers the method for invocation for the Startup.Load() method
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class StartupAttribute : Attribute
	{
		private string text;
		private int priority = int.MaxValue;
		public StartupAttribute(string text)
		{
			this.text = text;
		}
		public string Text
		{
			get { return text; }
		}
		public int Priority
		{
			get { return priority; }
			set { priority = value; }
		}
	}
}
