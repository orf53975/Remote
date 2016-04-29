using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remote
{
	//Based on: http://stackoverflow.com/questions/18988170/making-a-net-textbox-work-fifo-style
	public class ConsoleWriter
	{
		private int DisplaySize = 5000;
		private StringBuilder fifo;
		private bool changed;
		private string lastLine;

		public ConsoleWriter()
		{
			fifo = new StringBuilder(2 * DisplaySize);
		}
		public ConsoleWriter(int size)
		{
			DisplaySize = size;
			fifo = new StringBuilder(2 * DisplaySize);
		}

		private void Append(string s)
		{
			if (s.Length >= DisplaySize)
			{
				// FACT: the display will only include data from s
				// therefore, toss the entire buffer, and only keep the tail of s
				fifo.Clear();
				fifo.Append(s, s.Length - DisplaySize, DisplaySize);
				return;
				//return fifo.ToString();
			}
			if (fifo.Length + s.Length > fifo.Capacity)
			{
				// FACT: we will overflow the fifo
				// therefore, keep only data in the fifo that remains on the display
				fifo.Remove(0, fifo.Length + s.Length - DisplaySize);
			}
			fifo.Append(s);
			/*if (fifo.Length <= DisplaySize)
			{
				// FACT: the entire fifo content fits on the display
				// therefore, send it all
				//return fifo.ToString();
				return;
			}*/
			// FACT: the fifo content exceed the display size
			// therefore, extract just the tail
			//return fifo.ToString(fifo.Length - DisplaySize, DisplaySize);
		}
		public void WriteLine(string s)
		{
			changed = true;
			lastLine = s;
			Append(s);
			Append("\n");
		}
		public void WriteLine(string s, params object[] o)
		{
			changed = true;
			lastLine = String.Format(s, o);
			Append(s);
			Append("\n");
		}
		public string Update()
		{
			if (changed)
			{
				changed = false;
				if (fifo.Length <= DisplaySize)
				{
					return fifo.ToString();
				}
				else
				{
					return fifo.ToString(fifo.Length - DisplaySize, DisplaySize);
				}
			}
			else return null;
		}
		public string GetLastLine()
		{
			return lastLine;
		}
	}
}
