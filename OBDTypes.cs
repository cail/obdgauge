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

namespace OBDGauge
{

	public enum eUnits { UNITS_SI, UNITS_US, UNITS_UK };
	public enum eInterface { INTERFACE_ELM, INTERFACE_MULTIPLEX };
	public enum eBaud { BAUD_19200, BAUD_9600 };
	public enum eProtocol 
	{
		PROTOCOL_DISABLE,
		PROTOCOL_ISO,
		PROTOCOL_VPW,
		PROTOCOL_PWM,
		PROTOCOL_KWP
	};

	public class Prefs_s
	{
		public eUnits Units;
		public byte Query;
		public eInterface Interface;
		public eBaud Baud;
		public byte Address;
		public eProtocol Protocol;
		public byte Language;
		public byte Timeout;
		public byte GraphType;
		public string Port;
	};

	enum eMode 
	{
		MODE_NULL,
		MODE_INIT,
		MODE_ECHO_OFF,
		MODE_LINEFEED_OFF,
		MODE_SET_TIMEOUT,
		MODE_READ_PIDS,
		MODE_READ_SENSORS,
		MODE_READ_MIL,
		MODE_READ_ACTIVE_CODES,
		MODE_READ_PENDING_CODES,
		MODE_CLEAR_CODES
	};
}
