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
	}
	/// <summary>
	/// Registers the method for invocation for the Startup.Load() method
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	sealed class StartupAttribute : Attribute
	{
		public StartupAttribute()
		{
			
		}
	}
}
