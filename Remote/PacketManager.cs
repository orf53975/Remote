using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Remote
{
	public class PacketManager
	{
		public MemoryStream ms;
		public BinaryWriter bw;
		public BinaryReader br;
		//public int size;
		
		public PacketManager(byte[] data)
		{
			ms = new MemoryStream(data);
			bw = new BinaryWriter(ms);
			br = new BinaryReader(ms);
			ms.Position = 4L;
		}
		public PacketManager(MemoryStream ms)
		{
			this.ms = ms;
			bw = new BinaryWriter(ms);
			br = new BinaryReader(ms);
			ms.Position = 4L;
		}
		public void New()
		{
			ms.Position = 4L;
		}
		public int NewRead()
		{
			ms.Position = 0L;
			return br.ReadInt32();
		}
		public void Set(Command c)
		{
			bw.Write((int)c);
		}
		public Command Read()
		{
			return (Command)br.ReadInt32();
		}
		public void End()
		{
			long pos = ms.Position;
			ms.Position = 0L;
			bw.Write((int)pos);
			ms.Position = pos;
		}
		public int Size
		{
			get
			{
				long pos = ms.Position;
				ms.Position = 0L;
				int i = br.ReadInt32();
				ms.Position = pos;
				return i;
			}
		}
	}

}
