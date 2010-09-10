// OBD Gauge
// Copyright (C) 2005 Dana Peters

// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// the License, or (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

using System;
using System.Text;
using OpenNETCF.IO.Serial;

namespace OBDGauge
{
	/// <summary>
	/// class to send queries and receive information from the OBD interface
	/// </summary>
	public class OBDRead
	{
		static OBDRead mOBDRead;

		static public void CreateSingleton()
		{
			mOBDRead = new OBDRead();
		}

		static public OBDRead GetSingleton()
		{
			return mOBDRead;
		}

		static public void DestroySingleton()
		{
			mOBDRead.ReadClose();
			mOBDRead = null;
		}

		public Prefs_s Prefs
		{
			get
			{
				return mPrefs;
			}
			set
			{
				mPrefs = value;
			}
		}

		const int RXBUFSIZE = 32;

		private Prefs_s mPrefs;

		private UInt32 mSupportedPids;
		private UInt32 mActivePids;
		private bool mReadOpen;
		private DateTime mTicks;
		private eMode mMode = eMode.MODE_NULL;
		private bool mReadCodes = false;
		private bool mNeedInit = false;

		private void SendObdMultiplex(byte[] message, int length, bool multi)
		{
			byte[] command = new byte[15];
			int i, cs;
			command[0] = mPrefs.Address;
			switch (mPrefs.Protocol) {
			case eProtocol.PROTOCOL_ISO:
				command[1] = (byte)(mNeedInit ? 0x01 : (multi ? 0x10 : 0x81));
				command[3] = 0x68;
				command[4] = 0x6a;
				break;
			case eProtocol.PROTOCOL_VPW:
				command[1] = (byte)(multi ? 0x02 : 0x82);
				command[3] = 0x68;
				command[4] = 0x6a;
				break;
			case eProtocol.PROTOCOL_PWM:
				command[1] = (byte)(multi ? 0x04 : 0x84);
				command[3] = 0x61;
				command[4] = 0x6a;
				break;
			case eProtocol.PROTOCOL_KWP:
				command[1] = (byte)(mNeedInit? 0x85 : (multi ? 0x88 : 0x89));
				command[3] = 0xc7;
				command[4] = 0x33;
				break;
			default:
				return;
			}
			command[2] = (byte)(length + 3);
			command[5] = 0xf1;
			for (i = 0; i < 8; i++)
				command[i + 6] = i < length ? message[i] : (byte)0;
			cs = 0;
			for (i = 1; i < 14; i++)
				cs += command[i];
			command[14] = (byte)(cs & 0xff);
			OBDSerial.GetSingleton().SerialWrite(command, 15);
		}

		private void SendAsciiElm(String message)
		{
			OBDSerial.GetSingleton().SerialWrite(message);
		}

		private  void SendObdElm(byte[] message, int length)
		{
			StringBuilder sb = new StringBuilder();
			int i;
			for (i = 0; i < length; i++)
				sb.Append(message[i].ToString("X2"));
			sb.Append('\r');
			OBDSerial.GetSingleton().SerialWrite(sb.ToString());
		}

		private void SendObd(byte[] message, int length)
		{
			if (mPrefs.Interface == eInterface.INTERFACE_MULTIPLEX)
				SendObdMultiplex(message, length, false);
			else
				SendObdElm(message, length);
		}

		private void SendObdMulti(byte[] message, int length)
		{
			if (mPrefs.Interface == eInterface.INTERFACE_MULTIPLEX)
				SendObdMultiplex(message, length, true);
			else
				SendObdElm(message, length);
		}

		private void SendPidQuery()
		{
			byte[] msg = new byte[2];
			msg[0] = 1;
			msg[1] = 0;
			SendObd(msg, 2);
		}

		private int bitnum = 0;

		private void SendSensorQuery()
		{
			byte[] msg = new byte[2];
			if (mActivePids != 0)
			{
				for (;; bitnum++)
				{
					if (bitnum > 31)
					{
						OBDSensor.GetSingleton().SensorWriteLogRecord();
						bitnum = 0;
					}
					if ((mActivePids & (1L << (31 - bitnum))) != 0)
					{
						msg[0] = 1;
						msg[1] = (byte)++bitnum;
						SendObd(msg, 2);
						break;
					}
				}
			}
		}

		private void SendMILQuery()
		{
			byte[] msg = new byte[2];
			msg[0] = 1;
			msg[1] = 1;
			SendObd(msg, 2);
		}

		private void SendActiveCodeQuery()
		{
			byte[] msg = new byte[1];
			msg[0] = 3;
			SendObdMulti(msg, 1);
		}

		private void SendPendingCodeQuery()
		{
			byte[] msg = new byte[1];
			msg[0] = 7;
			SendObdMulti(msg, 1);
		}

		private void SendClearCodesCommand()
		{
			byte[] msg = new byte[1];
			msg[0] = 4;
			SendObd(msg, 1);
		}

		private void SendSetTimeout()
		{
			String[] timeout = new String[] { "32", "2B", "25", "1F", "19", "13", "0D", "06" };
			SendAsciiElm(String.Format("ATST {0:s}\r", timeout[mPrefs.Timeout]));
		}

		private void SwitchMode(eMode mode)
		{
			mTicks = DateTime.Now;
			mMode = mode;
			switch (mMode)
			{
				case eMode.MODE_NULL:
					// do nothing
					break;
				case eMode.MODE_INIT:
					mSupportedPids = 0;
					mNeedInit = true;
					if (mPrefs.Interface == eInterface.INTERFACE_MULTIPLEX)
						SwitchMode(eMode.MODE_READ_PIDS);
					else
					SendAsciiElm("ATZ\r");
					OBDSensor.GetSingleton().SensorUpdateStatus("Initializing...");
					break;
				case eMode.MODE_SET_TIMEOUT:
					SendSetTimeout();
					break;
				case eMode.MODE_ECHO_OFF:
					SendAsciiElm("ATE0\r");
					break;
				case eMode.MODE_LINEFEED_OFF:
					SendAsciiElm("ATL0\r");
					break;
				case eMode.MODE_READ_PIDS:
					SendPidQuery();
					break;
				case eMode.MODE_READ_SENSORS:
					break;
				case eMode.MODE_READ_MIL:
					break;
				case eMode.MODE_READ_ACTIVE_CODES:
					SendActiveCodeQuery();
					break;
				case eMode.MODE_READ_PENDING_CODES:
					SendPendingCodeQuery();
					break;
				case eMode.MODE_CLEAR_CODES:
					SendClearCodesCommand();
					break;
			}
		}

		private bool SecsHaveElapsed(int secs)
		{
			if (DateTime.Now.Subtract(mTicks).TotalSeconds > secs) {
				mTicks = DateTime.Now;
				return true;
			}
			return false;
		}

		public void ReadSetActivePids(UInt32 activePids)
		{
			mActivePids = activePids;
		}

		public UInt32 ReadGetActivePids()
		{
			return mActivePids;
		}

		private void ReadElmAscii(String message)
		{
		}

		private void ReadObd(byte[] message, int start, int length)
		{
			if (length == 0)
				return;
			switch (message[start]) {
			case 0x41: // sensor report
				if (length < 3)
					return;
				switch (message[start + 1]) {
				case 0x00: // supported PIDs
					if (length < 6)
						return;
					mSupportedPids = BytesToUInt32(message, start + 2);
					mActivePids = 0;
					OBDSensor.GetSingleton().SensorInit(mSupportedPids);
					SwitchMode(eMode.MODE_READ_SENSORS);
					break;
				case 0x01: // trouble codes count
					if (length < 3)
						return;
					if (OBDCodes.GetSingleton() != null)
						OBDCodes.GetSingleton().CodesSetMIL((message[start + 2] & 0x80) != 0);
					break;
				default:
					mTicks = DateTime.Now;
					OBDSensor.GetSingleton().SensorReceiveElmObd(message, start, length);
					break;
				}
				break;
			case 0x43: // active trouble codes
				if (OBDCodes.GetSingleton() != null)
					OBDCodes.GetSingleton().CodesReceiveElmObd(message, length);
				break;
			case 0x47: // pending trouble codes
				if (OBDCodes.GetSingleton() != null)
					OBDCodes.GetSingleton().CodesReceiveElmObd(message, length);
				break;
			case 0x44: // clear trouble codes
				if (OBDCodes.GetSingleton() != null)
					OBDCodes.GetSingleton().CodesSetClearCodes();
				break;
			}
		}

		private UInt32 BytesToUInt32(byte[] Data, int Start)
		{
			if (BitConverter.IsLittleEndian) 
			{
				byte[] bytes = new byte[4];
				bytes[3] = Data[Start];
				bytes[2] = Data[Start + 1];
				bytes[1] = Data[Start + 2];
				bytes[0] = Data[Start + 3];
				return BitConverter.ToUInt32(bytes, 0);
			} 
			else
				return BitConverter.ToUInt32(Data, Start);
		}

		private void ObdReplyComplete()
		{
			switch (mMode) {
			case eMode.MODE_NULL:
				// do nothing
				break;
			case eMode.MODE_INIT:
				if (mPrefs.Interface == eInterface.INTERFACE_ELM)
				SwitchMode(eMode.MODE_ECHO_OFF);
				break;
			case eMode.MODE_ECHO_OFF:
				SwitchMode(eMode.MODE_LINEFEED_OFF);
				break;
			case eMode.MODE_LINEFEED_OFF:
				SwitchMode(eMode.MODE_SET_TIMEOUT);
				break;
			case eMode.MODE_SET_TIMEOUT:
				SwitchMode(eMode.MODE_READ_PIDS);
				break;
			case eMode.MODE_READ_PIDS:
				break;
			case eMode.MODE_READ_SENSORS:
				mNeedInit = false;
				if (mReadCodes) {
					mReadCodes = false;
					SendMILQuery();
					SwitchMode(eMode.MODE_READ_MIL);
				} else
					SendSensorQuery();
				break;
			case eMode.MODE_READ_MIL:
				SwitchMode(eMode.MODE_READ_ACTIVE_CODES);
				break;
			case eMode.MODE_READ_ACTIVE_CODES:
				SwitchMode(eMode.MODE_READ_PENDING_CODES);
				break;
			case eMode.MODE_READ_PENDING_CODES:
				SwitchMode(eMode.MODE_NULL);
				if (OBDCodes.GetSingleton() != null)
					OBDCodes.GetSingleton().CodesReadComplete();
				break;
			case eMode.MODE_CLEAR_CODES:
				SwitchMode(eMode.MODE_INIT);
				ReadReadCodes();
				break;
			}
		}

		private void ReceiveElm(byte[] message, int start, int end)
		{
			int i;
			int n = 0;
			byte c;
			bool ascii = false; 
			bool odd = false;
			byte b;
			for (i = start; i < end && !ascii; i++)
			{
				c = message[i];
				if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F'))
					odd = !odd;
				else
					ascii = (c != ' ' && c != '>');
			}
			if (ascii || odd)
				ReadElmAscii(Encoding.ASCII.GetString(message, start, end - start));
			else {
				n = 0;
				b = 0;
				odd = false;
				for (i = 0; message[i] != 0; i++)
				{
					c = message[i];
					if (c >= '0' && c <= '9')
						b = (byte)((b << 4) + c - '0');
					else if (c >= 'A' && c <= 'F')
						b = (byte)((b << 4) + c - 'A' + 10);
					else
						continue;
					odd = !odd;
					if (!odd)
					{
						message[n++] = b;
						b = 0;
					}
				}
				ReadObd(message, 0, n);
			}
		}

		private byte[] rxbuf = new byte[RXBUFSIZE];
		private int rxindex = 0;

		public bool ReadSerialMultiplex(byte[] data)
		{
			int i;
			int n = data.Length;
			if (n + rxindex > RXBUFSIZE)
				n = RXBUFSIZE - rxindex;
			Array.Copy(data, 0, rxbuf, rxindex, n);
			i = rxindex;
			rxindex += n;
			if (rxindex >= 14)
			{
				if (rxbuf[0] == 0x40
					&& (rxbuf[1] == 0x81 || rxbuf[1] == 0x82 || rxbuf[1] == 0x84)
					&& (rxbuf[2] == 0x48 || rxbuf[2] == 0x41)
					&& rxbuf[3] == 0x6b)
				ReadObd(rxbuf, 5, 8);
				ObdReplyComplete();
				rxindex -= 14;
				Array.Copy(rxbuf, 14, rxbuf, 0, rxindex);
			}
			return true;
		}

		private int start = 0;
		private DateTime lastTicks;

		public bool ReadSerialElm(byte[] data)
		{
			DateTime ticks;
			int i;
			int n = data.Length;

			ticks = DateTime.Now;
			if (ticks.Subtract(lastTicks).TotalSeconds > 2)
				rxindex = 0;
			lastTicks = ticks;
			if (n + rxindex > RXBUFSIZE)
				n = RXBUFSIZE - rxindex;
			Array.Copy(data, 0, rxbuf, rxindex, n);
			i = rxindex;
			rxindex += n;
			for (; i < rxindex; i++)
			{
				switch (rxbuf[i])
				{
					case 0x0d:
						if (i > start)
							ReceiveElm(rxbuf, start, i);
						start = i + 1;
					break;
					case (byte)'>':
						ObdReplyComplete();
						start = i + 1;
						break;
					default:
						break;
				}
			}
			if (start > 0)
			{
				rxindex -= start;
				Array.Copy(rxbuf, start, rxbuf, 0, rxindex);
				start = 0;
			}
			return true;
		}

		public void ReadSerial()
		{
			while (OBDSerial.GetSingleton().SerialCheck() > 0)
			{
				byte[] data = OBDSerial.GetSingleton().SerialRead();
				switch (mPrefs.Interface)
				{
					case eInterface.INTERFACE_ELM:
						ReadSerialElm(data);
						break;
					case eInterface.INTERFACE_MULTIPLEX:
						ReadSerialMultiplex(data);
						break;
				}
			}
		}

		private String GetPort()
		{
			return "COM" + (mPrefs.Port + 1).ToString() + ":";
		}

		private BaudRates GetBaud()
		{
			switch (mPrefs.Interface)
			{
				case eInterface.INTERFACE_ELM:
					return BaudRates.CBR_9600;
				case eInterface.INTERFACE_MULTIPLEX:
					switch (mPrefs.Baud)
					{
						case eBaud.BAUD_9600:
							return BaudRates.CBR_9600;
						case eBaud.BAUD_19200:
							return BaudRates.CBR_19200;
					}
					break;
			}
			return 0;
		}

		public void ReadHandleTick()
		{
			ReadSerial();
			switch (mMode)
			{
				case eMode.MODE_NULL:
					// do nothing
					break;
				case eMode.MODE_INIT:
					if (mPrefs.Interface == eInterface.INTERFACE_ELM)
					if (SecsHaveElapsed(4))
						SendAsciiElm("ATZ\r");
					break;
				case eMode.MODE_ECHO_OFF:
				case eMode.MODE_LINEFEED_OFF:
				case eMode.MODE_READ_PIDS:
				case eMode.MODE_READ_SENSORS:
					if (SecsHaveElapsed(4))
						SwitchMode(eMode.MODE_INIT);
					break;
				case eMode.MODE_READ_MIL:
				case eMode.MODE_READ_ACTIVE_CODES:
				case eMode.MODE_READ_PENDING_CODES:
				case eMode.MODE_CLEAR_CODES:
					break;
			}
		}

		public void ReadOpen()
		{
			if (mReadOpen)
				ReadClose();
			mReadOpen = true;
			OBDSerial.GetSingleton().SerialOpen(GetPort(), GetBaud());
			SwitchMode(eMode.MODE_INIT);
		}

		public void ReadClose()
		{
			mReadOpen = false;
			SwitchMode(eMode.MODE_NULL);
			OBDSerial.GetSingleton().SerialClose();
		}

		public void ReadRestart()
		{
			if (mMode != eMode.MODE_READ_SENSORS)
				SwitchMode(eMode.MODE_INIT);
		}

		public void ReadPowerUp()
		{
			if (mReadOpen)
			{
				OBDSerial.GetSingleton().SerialOpen(GetPort(), GetBaud());
				SwitchMode(eMode.MODE_INIT);
			}
		}

		public void ReadPowerDown()
		{
			if (mReadOpen)
				OBDSerial.GetSingleton().SerialClose();
		}

		public void ReadReadCodes()
		{
			mReadCodes = true;
		}

		public void ReadClearCodes()
		{
			SwitchMode(eMode.MODE_CLEAR_CODES);
		}

	}
}
