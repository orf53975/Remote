using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Threading;
using System.Net;


namespace Remote
{
	public static class PING
	{
		private static List<Ping> pingers = new List<Ping>();
		private static int instances = 0;

		private static object @lock = new object();

		private static int result = 0;
		private static int timeOut = 250;

		private static int ttl = 5;

		private static List<IPAddress> addresses = new List<IPAddress>();

		public static void ALL(string channel)
		{
			string baseIP = "192.168." + channel + ".";

			Console.WriteLine("Pinging 255 destinations of D-class in {0}*", baseIP);

			CreatePingers(255);

			PingOptions po = new PingOptions(ttl, true);
			System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
			byte[] data = enc.GetBytes("abababababababababababababababab");

			SpinWait wait = new SpinWait();
			int cnt = 1;

			//Stopwatch watch = Stopwatch.StartNew();

			foreach (Ping p in pingers)
			{
				lock (@lock)
				{
					instances += 1;
				}

				p.SendAsync(string.Concat(baseIP, cnt.ToString()), timeOut, data, po);
				cnt += 1;
			}

			while (instances > 0)
			{
				wait.SpinOnce();
			}

			//watch.Stop();

			DestroyPingers();

			Console.WriteLine("Found {0} active IP-addresses.", result);
			result = 0;
		}
		public static void ADD_NEW(ref Dictionary<string, IPAddress> arr)
		{
			for (int i = 0; i < addresses.Count; i++)
			{
				if (!arr.ContainsKey(addresses[i].ToString()))
				{
					arr.Add(addresses[i].ToString(), addresses[i]);
				}
			}
			/*System.DirectoryServices.DirectoryEntry root = new System.DirectoryServices.DirectoryEntry("WinNT:");
			foreach (System.DirectoryServices.DirectoryEntry computers in root.Children)
			{
				foreach (System.DirectoryServices.DirectoryEntry computer in computers.Children)
				{
					if (computer.Name != "Schema")
					{
						Console.WriteLine(computer.Name);
					}
				}
			}*/
		}
		public static void REFRESH()
		{

		}
		private static void Ping_completed(object s, PingCompletedEventArgs e)
		{
			lock (@lock)
			{
				instances -= 1;
			}

			if (e.Reply.Status == IPStatus.Success)
			{
				Console.WriteLine(string.Concat("Active IP: ", e.Reply.Address.ToString()));
				result += 1;
				lock (@lock)
				{
					addresses.Add(e.Reply.Address);
				}
			}
			else
			{
				//Console.WriteLine(String.Concat("Non-active IP: ", e.Reply.Address.ToString()))
			}
		}
		private static void CreatePingers(int cnt)
		{
			for (int i = 1; i <= cnt; i++)
			{
				Ping p = new Ping();
				p.PingCompleted += Ping_completed;
				pingers.Add(p);
			}
		}
		private static void DestroyPingers()
		{
			foreach (Ping p in pingers)
			{
				p.PingCompleted -= Ping_completed;
				p.Dispose();
			}

			pingers.Clear();

		}
	}
}
