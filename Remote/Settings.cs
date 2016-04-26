using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Remote
{
	
	[ProtoBuf.ProtoContract(SkipConstructor=true)]
	public class Settings
	{
		[ProtoBuf.ProtoMember(1)]
		public int remoteDesktopPort = 6546; //Remote port for connections. Every type of packet will be sent through this port.
		[ProtoBuf.ProtoMember(2)]
		public List<string> addresses = new List<string>(10); //Saved addresses
		public static ErrorCode Save()
		{
			try
			{
				using (FileStream fs = new FileStream("settings.set", FileMode.OpenOrCreate, FileAccess.Write))
				{
					ProtoBuf.Serializer.Serialize(fs, Remote.Settings);
				}
			}
			catch
			{
				return new ErrorCode { error = true };
			}
			return new ErrorCode { error = false };
		}
		public static ErrorCode Load()
		{
			if (File.Exists("settings.set"))
			{
				using (FileStream fs = new FileStream("settings.set", FileMode.Open, FileAccess.Read))
				{
					Remote.Settings = ProtoBuf.Serializer.Deserialize<Settings>(fs);
				}
			}
			else //load default
			{
				Remote.Settings = new Settings();
				return new ErrorCode { error = true };
			}
			return new ErrorCode { error = false };
		}
		public class ErrorCode
		{
			public bool error;
		}
	}
}
