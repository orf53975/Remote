﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Remote
{
	public class Commands
	{
		
		public static void Get(byte[] array)
		{
			//PROCESS.Get(ref array);
		}
		public static void Set(ref string[] words)
		{
			
		}
	}
	public enum Command
	{
		COMPUTER_NAME,
		COMPUTER_NAME_REQUEST,
		CONNECT,
		CONNECT_SUCCESS,
		FOR,
		FORWARD,
		GET,
		GET_NAME,
		MESSENGER,
		NETWORK,
		NETWORK_DISCOVERY,
		NETWORK_STREAM,
		NETWORK_STREAM_ADD_COMMAND,
		NETWORK_STREAM_CONNECT,
		NETWORK_STREAM_CONNECT_SUCCESS,
		NETWORK_STREAM_LAST,
		NETWORK_STREAM_NULL,
		NETWORK_STREAM_READY,
		NETWORK_STREAM_SEND,
		NETWORK_STREAM_SEND_CORRUPTED,
		NETWORK_STREAM_SEND_END,//
		NETWORK_STREAM_SEND_INIT,
		NETWORK_STREAM_SEND_LAST,
		NETWORK_STREAM_SEND_SAFE,
		NETWORK_STREAM_SEND_SIMPLE,
		NETWORK_STREAM_SEND_START,//
		NETWORK_STREAM_SEND_SUCCESS,
		NETWORK_STREAM_SEND_WAS_LAST,
		PING,
		PROCESS,
		PROCESS_START,
		PUSH = 987564,
		REMOTE_DESKTOP,
		REMOTE_DESKTOP_FRAME,
		REMOTE_DESKTOP_MOUSE_MOVE,
		REMOTE_DESKTOP_REQUEST,
		SEND,
		UPDATE = 867584, //static, minden verzióban ugyan az
		USER_NAME
	}
}
