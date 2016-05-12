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
		public void Dispose()
		{
			ms.Dispose();
			bw.Dispose();
			br.Dispose();
		}
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
		public MemoryStream Stream
		{
			get
			{
				return ms;
			}
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
		[Obsolete]
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
		#region Write
		public void Write(Command c)
		{
			bw.Write((int)c);
		}
		public void Write(bool value)
		{
			bw.Write(value);
		}
		public void Write(byte value)
		{
			bw.Write(value);
		}
		public void Write(byte[] buffer)
		{
			bw.Write(buffer);
		}
		public void Write(char ch)
		{
			bw.Write(ch);
		}
		public void Write(char[] chars)
		{
			bw.Write(chars);
		}
		public void Write(decimal value)
		{
			bw.Write(value);
		}
		public void Write(double value)
		{
			bw.Write(value);
		}
		public void Write(float value)
		{
			bw.Write(value);
		}
		public void Write(int value)
		{
			bw.Write(value);
		}
		public void Write(long value)
		{
			bw.Write(value);
		}
		public void Write(sbyte value)
		{
			bw.Write(value);
		}
		public void Write(short value)
		{
			bw.Write(value);
		}
		public void Write(string value)
		{
			bw.Write(value);
		}
		public void Write(uint value)
		{
			bw.Write(value);
		}
		public void Write(ulong value)
		{
			bw.Write(value);
		}
		public void Write(ushort value)
		{
			bw.Write(value);
		}
		public void Write(byte[] buffer, int index, int count)
		{
			bw.Write(buffer, index, count);
		}
		public void Write(char[] chars, int index, int count)
		{
			bw.Write(chars, index, count);
		}
		#endregion
		#region Read
		public Command ReadCommand()
		{
			return (Command)br.ReadInt32();
		}
		public bool ReadBoolean()
		{
			return br.ReadBoolean();
		}
		public byte ReadByte()
		{
			return br.ReadByte();
		}
		public byte[] ReadBytes(int count)
		{
			return br.ReadBytes(count);
		}
		public char ReadChar()
		{
			return br.ReadChar();
		}
		public char[] ReadChars(int count)
		{
			return br.ReadChars(count);
		}
		public decimal ReadDecimal()
		{
			return br.ReadDecimal();
		}
		public double ReadDouble()
		{
			return br.ReadDouble();
		}
		public short ReadInt16()
		{
			return br.ReadInt16();
		}
		public int ReadInt32()
		{
			return br.ReadInt32();
		}
		public long ReadInt64()
		{
			return br.ReadInt64();
		}
		public sbyte ReadSByte()
		{
			return br.ReadSByte();
		}
		public float ReadSingle()
		{
			return br.ReadSingle();
		}
		public string ReadString()
		{
			return br.ReadString();
		}
		public ushort ReadUInt16()
		{
			return br.ReadUInt16();
		}
		public uint ReadUInt32()
		{
			return br.ReadUInt32();
		}
		public ulong ReadUInt64()
		{
			return br.ReadUInt64();
		}
		#endregion
	}
}
