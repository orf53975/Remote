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
		public static Settings s;

		[ProtoBuf.ProtoMember(1)]
		public int remoteDesktopPort = 6546; //Remote port for connections. Every type of packet will be sent through this port.
		[ProtoBuf.ProtoMember(2)]
		public List<string> addresses = new List<string>(10); //Saved addresses
		[ProtoBuf.ProtoMember(3)]
		public RemoteDesktopSettings remoteDesktopSettings = new RemoteDesktopSettings(); //
		[ProtoBuf.ProtoMember(4)]
		public bool startServer;
		[ProtoBuf.ProtoMember(5)]
		public string language = "LanguageEN";
		[ProtoBuf.ProtoMember(6)]
		public string name = Environment.UserName;

		// Do not save it, it will be generated at startup ... hopefully
		public string[] languages;

		[ProtoBuf.ProtoContract(SkipConstructor = true)]
		public class RemoteDesktopSettings
		{
			[ProtoBuf.ProtoMember(1)]
			public int screenCaptureMethod;
			[ProtoBuf.ProtoMember(2)]
			public int frameBuffering;
			[ProtoBuf.ProtoMember(3)]
			public int desktopComposition;
			[ProtoBuf.ProtoMember(4)]
			public int pixelFormat;
			[ProtoBuf.ProtoMember(5)]
			public int framesPerSecond;
			[ProtoBuf.ProtoMember(6)]
			public int compression;
			[ProtoBuf.ProtoMember(7)]
			public int losslessCodec;
			[ProtoBuf.ProtoMember(8)]
			public int lossyCodec;
			[ProtoBuf.ProtoMember(9)]
			public int LZ4BlockSize;
		}
		public static ErrorCode Save()
		{
			try
			{
				using (FileStream fs = new FileStream("settings.set", FileMode.OpenOrCreate, FileAccess.Write))
				{
					ProtoBuf.Serializer.Serialize(fs, Settings.s);
				}
			}
			catch //needs work!
			{
				return new ErrorCode { error = true };
			}
			return new ErrorCode { error = false };
		}
		[Startup("Loading settings...", Priority=0)]
		public static ErrorCode Load()
		{
			if (File.Exists("settings.set"))
			{
				using (FileStream fs = new FileStream("settings.set", FileMode.Open, FileAccess.Read))
				{
					Settings.s = ProtoBuf.Serializer.Deserialize<Settings>(fs);
				}
			}
			else //load default
			{
				Settings.s = new Settings();
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
