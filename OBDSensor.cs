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
using System.Drawing;
using System.IO;
using System.Text;

namespace OBDGauge
{
	/// <summary>
	/// class to accumulate sensor data and draw graphs
	/// </summary>
	public class OBDSensor
	{
		const int ROWMAX  = 8;
		const int PAGEMAX = 4;
		const int STATUSWIDTH = 176;

		static OBDSensor mOBDSensor;

		static public void CreateSingleton()
		{
			mOBDSensor = new OBDSensor();
		}

		static public OBDSensor GetSingleton()
		{
			return mOBDSensor;
		}

		static public void DestroySingleton()
		{
			mOBDSensor.CloseRecordDatabase();
			mOBDSensor.Deinit();
			mOBDSensor = null;
		}

		private OBDSensor()
		{
			mGraphics = OBDGauge.GetSingleton().CreateGraphics();
			mRectangle = OBDGauge.GetSingleton().ClientRectangle;
			SensorLoadPrefs();
            SensorLoadReadings();
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

		private Font titleFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular);
		private Font valueFont = new Font(FontFamily.GenericSansSerif, 20, FontStyle.Regular);
		private Pen blackPen = new Pen(Color.Black);
		private Pen bluePen = new Pen(Color.Blue);
		private Pen whitePen = new Pen(Color.White);
		private SolidBrush whiteBrush = new SolidBrush(Color.FromArgb(0xff, 0xff, 0xff));
		private SolidBrush yellowBrush = new SolidBrush(Color.FromArgb(0xff, 0xff, 0xcc));
		private SolidBrush blackBrush = new SolidBrush(Color.Black);
		private SolidBrush greenBrush = new SolidBrush(Color.FromArgb(0, 0xff, 0));
		private SolidBrush darkGreenBrush = new SolidBrush(Color.FromArgb(0, 0xcc, 0));
		private SolidBrush grayBrush = new SolidBrush(Color.FromArgb(0x88, 0x88, 0x88));
		private BinaryWriter mLogWriter = null;

		struct s_sensor
		{
			public String name;
			public String metric;
			public String imperial;
			public s_sensor(String newName, String newMetric, String newImperial)
			{
				name = newName;
				metric = newMetric;
				imperial = newImperial;
			}
		};

		struct s_reading
		{
			public Int16 value;
			public Int16 datacount;
			public Int16[] data;
		};  

		enum eSensor
		{
			x11, x0C, x0D, x04, x0E, x0B, x10, x06, x07, x08, x09,
			x0F, x05, x0A, x14, x15, x16, x17, x18, x19, x1A, x1B,
			SENSORMAX
		};

		enum eLanguage
		{
			ENGLISH, FRENCH, GERMAN, DUTCH, SPANISH, LANGMAX
		};

		struct SensorPrefs_s {
			public byte Page;
			public UInt32 SupportedPids;
			public bool Imperial;
			public byte[,] Sensor;
			public byte[] SensorCount;
		};

		private SensorPrefs_s mSensorPrefs = new SensorPrefs_s();
		private int mSelectedRow = -1;
		private int mLastReadTicks;
		private int mUpdateCount = 0;
		private Int16[] mUpdateRate = new Int16[(int)eSensor.SENSORMAX];
		private Prefs_s mPrefs;
		private Graphics mGraphics;
		private Rectangle mRectangle;

		eSensor[] activeSensor =
		{
			eSensor.x11, eSensor.x0C, eSensor.x0D, eSensor.x04, eSensor.x0E, eSensor.x0B,
			eSensor.x10, eSensor.x06, eSensor.x07, eSensor.x08, eSensor.x09,
			eSensor.x0F, eSensor.x05, eSensor.x0A, eSensor.x14, eSensor.x15, eSensor.x16,
			eSensor.x17, eSensor.x18, eSensor.x19, eSensor.x1A, eSensor.x1B, eSensor.SENSORMAX
		};

		byte[] pidMap =
		{
			0x11, 0x0c, 0x0d, 0x04, 0x0e, 0x0b, 0x10, 0x06, 0x07, 0x08, 0x09,
			0x0f, 0x05, 0x0a, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b
		};

		eSensor[] sensorMap =
		{
			eSensor.SENSORMAX, eSensor.SENSORMAX, eSensor.SENSORMAX, eSensor.SENSORMAX,
			eSensor.x04, eSensor.x05, eSensor.x06, eSensor.x07, eSensor.x08, eSensor.x09,
			eSensor.x0A, eSensor.x0B, eSensor.x0C, eSensor.x0D, eSensor.x0E, eSensor.x0F,
			eSensor.x10, eSensor.x11, eSensor.SENSORMAX, eSensor.SENSORMAX, eSensor.x14,
			eSensor.x15, eSensor.x16, eSensor.x17, eSensor.x18, eSensor.x19, eSensor.x1A,
			eSensor.x1B, eSensor.SENSORMAX, eSensor.SENSORMAX, eSensor.SENSORMAX, eSensor.SENSORMAX
		};

		s_sensor[,] sensorTitle =
		{
				// English
			{
				new s_sensor("Throttle position", null, null ),
				new s_sensor("Engine RPM", null, null),
				new s_sensor("Vehicle speed", "(km/h)", "(mph)"),
				new s_sensor("Calculated load value %", null, null),
				new s_sensor("Ignition timing advance °", null, null),
				new s_sensor("Intake air pressure", "(kPa)", "(inMg)"),
				new s_sensor("Intake air flow rate", "(g/s)", "(lb/min)"),
				new s_sensor("Short term fuel trim", null, null),
				new s_sensor("Long term fuel trim", null, null),
				new s_sensor("Short term fuel trim 2", null, null),
				new s_sensor("Long term fuel trim 2", null, null),
				new s_sensor("Air temperature", "(°C)", "(°F)"),
				new s_sensor("Coolant temperature", "(°C)", "(°F)"),
				new s_sensor("Fuel pressure", "(kPa)", "(inMg)"),
				new s_sensor("Oxygen Sensor 1", "(mV)", "(mV)"),
				new s_sensor("Oxygen Sensor 2", "(mV)", "(mV)"),
				new s_sensor("Oxygen Sensor 3", "(mV)", "(mV)"),
				new s_sensor("Oxygen Sensor 4", "(mV)", "(mV)"),
				new s_sensor("Oxygen Sensor 2-1", "(mV)", "(mV)"),
				new s_sensor("Oxygen Sensor 2-2", "(mV)", "(mV)"),
				new s_sensor("Oxygen Sensor 2-3", "(mV)", "(mV)"),
				new s_sensor("Oxygen Sensor 2-4", "(mV)", "(mV)")
			},
					// French
			{
				new s_sensor("Position du papillon", null, null),
				new s_sensor("Régime moteur", null, null),
				new s_sensor("Vitesse du véhicule", "(km/h)", "(mph)"),
				new s_sensor("Charge calculée moteur %", null, null),
				new s_sensor("Avance à l'allumage °", null, null),
				new s_sensor("Pression d'air", "(kPa)", "(inMg)"),
				new s_sensor("Débit d'air (admission)", "(g/s)", "(lb/min)"),
				new s_sensor("CT correction carburant", null, null),
				new s_sensor("LT correction carburant", null, null),
				new s_sensor("CT correction carburant 2", null, null),
				new s_sensor("LT correction carburant 2", null, null),
				new s_sensor("Température d'air", "(°C)", "(°F)"),
				new s_sensor("Température d'eau", "(°C)", "(°F)"),
				new s_sensor("Pression carburant", "(kPa)", "(inMg)"),
				new s_sensor("Sonde à oxygène 1", "(mV)", "(mV)"),
				new s_sensor("Sonde à oxygène 2", "(mV)", "(mV)"),
				new s_sensor("Sonde à oxygène 3", "(mV)", "(mV)"),
				new s_sensor("Sonde à oxygène 4", "(mV)", "(mV)"),
				new s_sensor("Sonde à oxygène 2-1", "(mV)", "(mV)"),
				new s_sensor("Sonde à oxygène 2-2", "(mV)", "(mV)"),
				new s_sensor("Sonde à oxygène 2-3", "(mV)", "(mV)"),
				new s_sensor("Sonde à oxygène 2-4", "(mV)", "(mV)")
			},
					// German
			{
				new s_sensor("Drosselkl. Stellung", null, null),
				new s_sensor("Motor U/min", null, null),
				new s_sensor("Geschwindigkeit", "(km/h)", "(mph)"),
				new s_sensor("Berechn. Lastwert %", null, null),
				new s_sensor("Zündvoreilung °", null, null),
				new s_sensor("Ansaug-Luftdruck", "(kPa)", "(inMg)"),
				new s_sensor("Ansaug-Luftmenge", "(g/s)", "(lb/min)"),
				new s_sensor("Kurzz. Kraftst. Trim", null, null),
				new s_sensor("Langz. Kraftst. Trim", null, null),
				new s_sensor("Kurzz. Kraftst. Trim 2", null, null),
				new s_sensor("Langz. Kraftst. Trim 2", null, null),
				new s_sensor("Lufttemperatur", "(°C)", "(°F)"),
				new s_sensor("Kühlmitteltemp.", "(°C)", "(°F)"),
				new s_sensor("Kraftstoffdruck", "(kPa)", "(inMg)"),
				new s_sensor("Lambdasonde 1", "(mV)", "(mV)"),
				new s_sensor("Lambdasonde 2", "(mV)", "(mV)"),
				new s_sensor("Lambdasonde 3", "(mV)", "(mV)"),
				new s_sensor("Lambdasonde 4", "(mV)", "(mV)"),
				new s_sensor("Lambdasonde 2-1", "(mV)", "(mV)"),
				new s_sensor("Lambdasonde 2-2", "(mV)", "(mV)"),
				new s_sensor("Lambdasonde 2-3", "(mV)", "(mV)"),
				new s_sensor("Lambdasonde 2-4", "(mV)", "(mV)")
			},
					// Dutch
			{
				new s_sensor("Gaspedaal Positie", null, null),
				new s_sensor("Toerental RPM", null, null),
				new s_sensor("Snelheid", "(km/h)", "(mph)"),
				new s_sensor("Berekende Belasting %", null, null),
				new s_sensor("Ontstekingstijdstip °", null, null),
				new s_sensor("Inlaatlucht Druk", "(kPa)", "(inMg)"),
				new s_sensor("Inlaatlucht Snelheid", "(g/s)", "(lb/min)"),
				new s_sensor("Korte Term Br.st. Inst.", null, null),
				new s_sensor("Lange Term Br.st. Inst.", null, null),
				new s_sensor("Korte Term Br.st. Inst. 2", null, null),
				new s_sensor("Lange Term Br.st. Inst. 2", null, null),
				new s_sensor("Lucht Temperatuur", "(°C)", "(°F)"),
				new s_sensor("Koelwater Temperatuur", "(°C)", "(°F)"),
				new s_sensor("Brandstof Druk", "(kPa)", "(inMg)"),
				new s_sensor("Zuurstof Opnemer 1", "(mV)", "(mV)"),
				new s_sensor("Zuurstof Opnemer 2", "(mV)", "(mV)"),
				new s_sensor("Zuurstof Opnemer 3", "(mV)", "(mV)"),
				new s_sensor("Zuurstof Opnemer 4", "(mV)", "(mV)"),
				new s_sensor("Zuurstof Opnemer 2-1", "(mV)", "(mV)"),
				new s_sensor("Zuurstof Opnemer 2-2", "(mV)", "(mV)"),
				new s_sensor("Zuurstof Opnemer 2-3", "(mV)", "(mV)"),
				new s_sensor("Zuurstof Opnemer 2-4", "(mV)", "(mV)")
			},
					// Spanish
			{
				new s_sensor("Posicion del acelerador", null, null),
				new s_sensor("RPM del motor", null, null),
				new s_sensor("Velocidad del vehiculo", "(km/h)", "(mph)"),
				new s_sensor("% de carga calculada", null, null),
				new s_sensor("Avance del tiempo de enc.", null, null),
				new s_sensor("Pres.de flujo de aire ad.", "(kPa)", "(inMg)"),
				new s_sensor("Prop.de flujo de aire ad.", "(g/s)", "(lb/min)"),
				new s_sensor("Regulac. corta de nafta", null, null),
				new s_sensor("Regulac. larga de nafta", null, null),
				new s_sensor("Regulac. corta de nafta 2", null, null),
				new s_sensor("Regulac. larga de nafta 2", null, null),
				new s_sensor("Temperatura del aire", "(°C)", "(°F)"),
				new s_sensor("Temp. del refrigerante", "(°C)", "(°F)"),
				new s_sensor("Presión de combustible", "(kPa)", "(inMg)"),
				new s_sensor("Sensor de oxigeno 1", "(mV)", "(mV)"),
				new s_sensor("Sensor de oxigeno 2", "(mV)", "(mV)"),
				new s_sensor("Sensor de oxigeno 3", "(mV)", "(mV)"),
				new s_sensor("Sensor de oxigeno 4", "(mV)", "(mV)"),
				new s_sensor("Sensor de oxigeno 2-1", "(mV)", "(mV)"),
				new s_sensor("Sensor de oxigeno 2-2", "(mV)", "(mV)"),
				new s_sensor("Sensor de oxigeno 2-3", "(mV)", "(mV)"),
				new s_sensor("Sensor de oxigeno 2-4", "(mV)", "(mV)")
			}
		};

		private s_reading[] reading = new s_reading[(int)eSensor.SENSORMAX];

		enum eColor
		{
			white, lightGray, darkGray, black
		};

		public UInt32 SensorGetAllActivePids()
		{
			int i;
			UInt32 activePids = 0;
			for (i = 0; i < (int)eSensor.SENSORMAX; i++)
				activePids |= (UInt32)(1L << (32 - pidMap[i]));
			return activePids;
		}  

		public void Init()
		{
			SensorPage(mSensorPrefs.Page);
			if (mSensorPrefs.SupportedPids == 0)
				SensorInit(0xffffffff);
		}

		private void Deinit()
		{
			SensorSavePrefs();
			SensorSaveReadings();
		}

		private  void SensorSavePrefs()
		{
			String exePath = System.IO.Path.GetDirectoryName( 
				System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
			String filePath = Path.Combine(exePath, "Layout.dat");
			BinaryWriter writer = new BinaryWriter(new FileStream(filePath, FileMode.Create));
			writer.Write(mSensorPrefs.Page);
			writer.Write(mSensorPrefs.SupportedPids);
			writer.Write(mSensorPrefs.Imperial);
			int page, row;
			for (page = 0; page < PAGEMAX; page++)
				for (row = 0; row < ROWMAX; row++)
					writer.Write(mSensorPrefs.Sensor[page, row]);
			writer.Write(mSensorPrefs.SensorCount);
			writer.Close();
		}

		private  void SensorLoadPrefs()
		{
			mSensorPrefs = new SensorPrefs_s();
			try
			{
				String exePath = System.IO.Path.GetDirectoryName( 
					System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
				String filePath = Path.Combine(exePath, "Layout.dat");
				BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open));
				mSensorPrefs.Page = reader.ReadByte();
				mSensorPrefs.SupportedPids = reader.ReadUInt32();;
				mSensorPrefs.Imperial = reader.ReadBoolean();
				mSensorPrefs.Sensor = new byte[PAGEMAX,ROWMAX];
				mSensorPrefs.SensorCount = new byte[PAGEMAX];
				int page, row;
				for (page = 0; page < PAGEMAX; page++)
					for (row = 0; row < ROWMAX; row++)
						mSensorPrefs.Sensor[page, row] = reader.ReadByte();
				mSensorPrefs.SensorCount = reader.ReadBytes(PAGEMAX);
				reader.Close();
			}
			catch (System.IO.FileNotFoundException)
			{
				// initialize mSensorPrefs
				mSensorPrefs.Page = 0;
				mSensorPrefs.SupportedPids = 0;
				mSensorPrefs.Imperial = false;
				mSensorPrefs.Sensor = new byte[PAGEMAX,ROWMAX];
				mSensorPrefs.SensorCount = new byte[PAGEMAX];
			}
		}
		
		public void SensorSaveReadings()
		{
			String exePath = System.IO.Path.GetDirectoryName( 
				System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
			String filePath = Path.Combine(exePath, "Sensor.dat");
			BinaryWriter writer = new BinaryWriter(new FileStream(filePath, FileMode.Create));
			int i;
			for (i = 0; i < (int)eSensor.SENSORMAX; i++) 
			{
				writer.Write(reading[i].value);
				writer.Write(reading[i].datacount);
				int n;
				for (n = 0; n < STATUSWIDTH; n++)
					writer.Write(reading[i].data[n]);
			}
			writer.Close();
		}

		public void SensorLoadReadings()
		{
			SensorClear();
			try
			{
				String exePath = System.IO.Path.GetDirectoryName( 
					System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
				String filePath = Path.Combine(exePath, "Sensor.dat");
				BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open));
				int i;
				for (i = 0; i < (int)eSensor.SENSORMAX; i++) 
				{
					reading[i].value = reader.ReadInt16();
					reading[i].datacount = reader.ReadInt16();
					int n;
					for (n = 0; n < STATUSWIDTH; n++)
						reading[i].data[n] = reader.ReadInt16();
				}
				reader.Close();
			}
			catch (System.IO.FileNotFoundException)
			{
				// initialize mSensorPrefs
				mSensorPrefs.Page = 0;
				mSensorPrefs.SupportedPids = 0;
				mSensorPrefs.Imperial = false;
				mSensorPrefs.Sensor = new byte[PAGEMAX,ROWMAX];
				mSensorPrefs.SensorCount = new byte[PAGEMAX];
			}
		}

		private String GetLogFilename()
		{
			return "\\My Documents\\OBDGaugeLog.dat";
		}

		public void OpenRecordDatabase()
		{
			if (mLogWriter == null)
				mLogWriter = new BinaryWriter(new FileStream(GetLogFilename(), FileMode.Append));
		}

		public void CloseRecordDatabase()
		{
			if (mLogWriter != null)
				mLogWriter.Close();
			mLogWriter = null;
		}

		public void ClearRecordDatabase()
		{
			bool wasOpen = (mLogWriter != null);
			CloseRecordDatabase();
			try
			{
				System.IO.File.Delete(GetLogFilename());
			}
			catch (System.IO.FileNotFoundException)
			{
				// do nothing
			}
			if (wasOpen)
				OpenRecordDatabase();
		}

		public void SensorWriteLogRecord()
		{
			if (mLogWriter == null)
				return;
			// Palm uses Jan 1, 1904 as date times are measured from
			UInt32 seconds = Convert.ToUInt32(DateTime.Now.Subtract(new DateTime(1904, 1, 1)).TotalSeconds);
			UInt32 activePids = OBDRead.GetSingleton().ReadGetActivePids();
			byte length = 8;
			int pid;
			eSensor sensorNum;
			for (pid = 1; pid < 32; pid++) 
			{
				if ((activePids & (1L << (32 - pid))) != 0) 
				{
					sensorNum = sensorMap[pid];
					if (sensorNum >= 0)
						length += 2;
				}
			}
			mLogWriter.Write(length);
			mLogWriter.Write(seconds);
			mLogWriter.Write(activePids);
			for (pid = 1; pid < 32; pid++) 
			{
				if ((activePids & (1L << (32 - pid))) != 0) 
				{
					sensorNum = sensorMap[pid];
					if (sensorNum >= 0) 
					{
						mLogWriter.Write(reading[(int)sensorNum].value);
						mUpdateCount = 0;
					}
				}
			}
		}

		private void SelectRow(bool select)
		{
			Rectangle bounds;
			if (mSelectedRow >= 0)
			{
				bounds = GetRowRectangle(mSelectedRow);
				if (select)
					mGraphics.DrawRectangle(bluePen, bounds);
				else
					mGraphics.DrawRectangle(whitePen, bounds);
				OBDGauge.GetSingleton().EnableGaugeMenu(false);
			}
			OBDGauge.GetSingleton().EnableGaugeMenu(select);
		}

		public void SensorUpdateStatus(String message)
		{
			OBDGauge.GetSingleton().Text = message;
		}

		private void UpdateRate(Int16 rate)
		{
			int i;
			StringBuilder s = new StringBuilder();
			UInt32 activePids;
			Int16 sensorCount;
			int pid;
			activePids = OBDRead.GetSingleton().ReadGetActivePids();
			sensorCount = 0;
			if (mLogWriter != null)
			{
				s.Append("Record ");
				long fileLength = mLogWriter.BaseStream.Length;
				s.Append(fileLength / 1024);
				s.Append('.');
				s.Append((fileLength % 1024) * 100 / 1024);
				s.Append('K');
			} 
			else 
			{
				s.Append("Scan ");
				for (pid = 1; pid < 32; pid++)
					if ((activePids & (1L << (32 - pid))) != 0)
						if (sensorMap[pid] >= 0)
							sensorCount++;
				for (i = 0; i < sensorCount; i++)
					s.Append((i == mUpdateCount) ? 'o' : '.');
				mUpdateRate[mUpdateCount++] = rate;
				if (mUpdateCount >= sensorCount)
					mUpdateCount = 0;
				rate = 0;
				for (i = 0; i < sensorCount; i++)
					rate += mUpdateRate[i];
				if (sensorCount != 0)
					rate /= sensorCount;
				s.Append(' ');
				if (rate < 0)
					rate = 0;
				if (rate > 99)
					rate = 99;
				s.Append(rate / 10);
				s.Append('.');
				s.Append(rate - (rate / 10) * 10);
				s.Append("/s");
			}
			SensorUpdateStatus(s.ToString());
		}

		private void UpdateGraph(Graphics g, Rectangle bounds, Int16 row)
		{
			const int widthData = 1;
			const int widthCurrent = 5;
			const int widthNumber = 64;
			String title;
			Rectangle rect;
			int sensorNum = mSensorPrefs.Sensor[mSensorPrefs.Page,row];
			int value = reading[sensorNum].value;
			int datalen = reading[sensorNum].datacount;
			int i, minvalue, maxvalue, scale, lastY;
			int y = 0;
			eUnits units;
			SolidBrush drawBrush = blackBrush;
			title = sensorTitle[mPrefs.Language,sensorNum].name + " ";
			// use mph for UK
			units = mPrefs.Units;
			if (pidMap[sensorNum] == 0x0D && mPrefs.Units == eUnits.UNITS_UK)
				units = eUnits.UNITS_US;
			switch (units)
			{
				case eUnits.UNITS_SI:
				case eUnits.UNITS_UK:
					if (sensorTitle[mPrefs.Language,sensorNum].metric != null)
						title += sensorTitle[mPrefs.Language,sensorNum].metric;
					break;
				case eUnits.UNITS_US:
					if (sensorTitle[mPrefs.Language,sensorNum].imperial != null)
						title += sensorTitle[mPrefs.Language,sensorNum].imperial;
					break;
			}
			rect = new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2);
			g.FillRectangle(yellowBrush, rect);
			if (mPrefs.GraphType == 0)
				g.DrawString(title, titleFont, blackBrush,
					bounds.X + 5, bounds.Y + (bounds.Height - 16) / 2);
			if (value != 0x7fff)
				g.DrawString(value.ToString(), valueFont, blackBrush,
					bounds.X + bounds.Width - widthNumber, bounds.Y + (bounds.Height - 30) / 2);
			g.DrawRectangle(blackPen, rect);
			bounds.Width -= widthNumber + 7;
			rect.X = bounds.X + bounds.Width - widthData;
			minvalue = 0x7fff;
			maxvalue = -0x7fff;
			lastY = -1;
			for (i = 0; i < datalen; i++)
			{
				if (reading[sensorNum].data[i] < minvalue)
					minvalue = reading[sensorNum].data[i];
				if (reading[sensorNum].data[i] > maxvalue && reading[sensorNum].data[i] != 0x7fff)
					maxvalue = reading[sensorNum].data[i];
			}
			if (maxvalue == minvalue)
			{
				maxvalue += 1;
				minvalue -= 1;
				scale = 3;
			} else
				scale = maxvalue - minvalue;
			
			for (i = 0; i < datalen; i++)
			{
				if (i == 0)
				{
					drawBrush = greenBrush; 
					rect.Width = widthCurrent;
				}
				if (i == 1)
				{
					drawBrush = darkGreenBrush;
					rect.Width = widthData;
				}
				if (mPrefs.GraphType == 1)
				{
					if (reading[sensorNum].data[i] == 0x7fff)
					{
						drawBrush = grayBrush;
						rect.Height = bounds.Height - 3;
					} else
						rect.Height = (reading[sensorNum].data[i] - minvalue)*(bounds.Height - 4) / scale + 1;
					rect.Y = bounds.Y + bounds.Height - rect.Height - 1;
				} else {
					if (reading[sensorNum].data[i] == 0x7fff) {
						drawBrush = grayBrush;
						lastY = -1;
						y = bounds.Height - 3;
					} else {
						if (lastY != -1)
							lastY = y;
						y = (reading[sensorNum].data[i] - minvalue)*(bounds.Height - 4) / scale + 1;
						if (lastY == -1)
							lastY = y;
					}
					if (reading[sensorNum].data[i] != 0x7fff)
					{
						if (lastY == -1) {
							rect.Height = 1;
							rect.Y = bounds.Y + bounds.Height - y - 1;
						} else {
							rect.Height = y > lastY ? y - lastY : lastY - y;
							if (rect.Height == 0)
								rect.Height = 1;
							rect.Y = bounds.Y + bounds.Height - (y > lastY ? y : lastY) - 1;
						}
					} else {
						rect.Height = y;
						rect.Y = bounds.Y + bounds.Height - y - 1;
					}
				}
				if (i != 0 || reading[sensorNum].data[0] != 0x7fff)
					g.FillRectangle(drawBrush, rect);
				if (reading[sensorNum].data[i] == 0x7fff)
					drawBrush = darkGreenBrush;
				rect.X -= widthData;
				if (rect.X <= bounds.X + 1)
					break;
			}
			if (mPrefs.GraphType == 1)
				g.DrawString(title, titleFont, blackBrush,
					bounds.X + 5, bounds.Y + (bounds.Height - 16) / 2);
			if (bounds.Height >= 12 * 3)
			{
				if (maxvalue != -0x7fff)
					g.DrawString(maxvalue.ToString(), titleFont, blackBrush, bounds.X + 5, bounds.Y);
				if (minvalue != 0x7fff)
					g.DrawString(minvalue.ToString(), titleFont, blackBrush, bounds.X + 5,
						bounds.Y + bounds.Height - 16);
			}
		}

		private Rectangle GetRowRectangle(int row)
		{
			int graphHeight = (mRectangle.Height - 1) / mSensorPrefs.SensorCount[mSensorPrefs.Page];
			Rectangle bounds = new Rectangle(mRectangle.X, mRectangle.Y + row * graphHeight,
				mRectangle.Width - 1, graphHeight);
			return bounds;
		}

		private void UpdateRow(int row)
		{
			UpdateGraph(mGraphics, GetRowRectangle(row), (Int16)row);
		}  

		public void SensorClear()
		{
			int i;
			for (i = 0; i < (int)eSensor.SENSORMAX; i++) {
				reading[i].value = 0;
				reading[i].datacount = 0;
				reading[i].data = new Int16[STATUSWIDTH];
			}
		}

		private void InitSensorTable()
		{
		}

		public void SensorRedraw()
		{
			RedrawSensorTable();
		}

		private void RedrawSensorTable()
		{
			int row;
			mGraphics.FillRectangle(whiteBrush, mRectangle);
			for (row = 0; row < mSensorPrefs.SensorCount[mSensorPrefs.Page]; row++)
				UpdateRow(row);
			SelectRow(true);
		}

		private void InitSensorMap()
		{
			int i, page, row;
			int count = 0;
			UInt32 activePid;
			for (i = 0; i < (int)eSensor.SENSORMAX; i++)
			{
				activePid = (UInt32)(1L << (32 - pidMap[(int)activeSensor[i]]));
				if ((mSensorPrefs.SupportedPids & activePid) != 0)
				count++;
			}
			for (page = 0; page < PAGEMAX; page++)
				mSensorPrefs.SensorCount[page] = 0;
			row = 0;
			page = 0;
			for (i = 0; i < (int)eSensor.SENSORMAX; i++)
			{
				activePid = (UInt32)(1L << (32 - pidMap[(int)activeSensor[i]]));
				if ((mSensorPrefs.SupportedPids & activePid) != 0) {
					mSensorPrefs.Sensor[page,row] = (byte)i;
					mSensorPrefs.SensorCount[page] = (byte)++row;
					if (row >= (count + PAGEMAX - 1) / PAGEMAX)
					{
						page++;
						row = 0;
					}
				}
			}
		}

		private void UpdateStatus(eSensor sensorNum, int value)
		{
			int row;
			Array.Copy(reading[(int)sensorNum].data, 0, reading[(int)sensorNum].data, 1,
				reading[(int)sensorNum].datacount);
			if (reading[(int)sensorNum].datacount < STATUSWIDTH-1)
				reading[(int)sensorNum].datacount++;
			reading[(int)sensorNum].value = (Int16)value;
			reading[(int)sensorNum].data[0] = (Int16)value;
			for (row = 0; row < mSensorPrefs.SensorCount[mSensorPrefs.Page]; row++)
				if (mSensorPrefs.Sensor[mSensorPrefs.Page,row] == (int)sensorNum)
					UpdateRow(row);
		}

		private void InsertBreakPage(int page)
		{
			int row;
			for (row = 0; row < mSensorPrefs.SensorCount[page]; row++)
				UpdateStatus((eSensor)mSensorPrefs.Sensor[page,row], 0x7fff);
		}

		public void SensorInsertBreak()
		{
			int page;
			for (page = 0; page < PAGEMAX; page++)
				InsertBreakPage(page);
		}

		private void CalculateRate()
		{
			int ticks = Environment.TickCount;
			int msecs = ticks - mLastReadTicks;
			if (msecs > 0)
				UpdateRate((Int16)(10 * 1000 / msecs));
			mLastReadTicks = ticks;
		}

		public void SensorReceiveElmObd(byte[] message, int start, int length)
		{
			int pid;
			if (length == 0)
				return;
			switch (message[start])
			{
				case 0x41:
					if (length < 3)
						return;
					pid = message[start + 1];
					switch (pid)
					{
						case 0x04: // calculated load value
							UpdateStatus(eSensor.x04, (message[start + 2] * 100) / 255);
							break;
						case 0x05: // engine coolent temperature
							switch (mPrefs.Units)
							{
								case eUnits.UNITS_SI:
								case eUnits.UNITS_UK:
									UpdateStatus(eSensor.x05, message[start + 2] - 40);
									break;
								case eUnits.UNITS_US:
									UpdateStatus(eSensor.x05, (message[start + 2] - 40) * 9 / 5 + 32);
									break;
							}
							break;
						case 0x06: // short term fuel trim
						case 0x07: // long term fuel trim
						case 0x08: // short term fuel trim bank 2
						case 0x09: // long term fuel trim bank 2
							UpdateStatus(pid - 0x06 + eSensor.x06, (message[start + 2] - 128) * 100 / 128);
							break;
						case 0x0a: // fuel pressure
							switch (mPrefs.Units)
							{
								case eUnits.UNITS_SI:
								case eUnits.UNITS_UK:
									UpdateStatus(eSensor.x0A, message[start + 2] * 3);
									break;
								case eUnits.UNITS_US:
									UpdateStatus(eSensor.x0A, (message[start + 2] * 3) * 29 / 100);
									break;
							}
							break;
						case 0x0b: // intake manifold pressure
							switch (mPrefs.Units)
							{
								case eUnits.UNITS_SI:
								case eUnits.UNITS_UK:
									UpdateStatus(eSensor.x0B, message[start + 2]);
									break;
								case eUnits.UNITS_US:
									UpdateStatus(eSensor.x0B, message[start + 2] * 29 / 100);
									break;
							}
							break;
						case 0x0c: // engine RPM
							if (length < 4)
								return;
							UpdateStatus(eSensor.x0C, (message[start + 2] * 0x100 + message[start + 3]) / 4);
							break;
						case 0x0d: // vehicle speed
							switch (mPrefs.Units)
							{
								case eUnits.UNITS_SI:
								case eUnits.UNITS_UK:
									UpdateStatus(eSensor.x0D, message[start + 2]);
									break;
								case eUnits.UNITS_US:
									UpdateStatus(eSensor.x0D, message[start + 2] * 62 / 100);
									break;
							}
							break;
						case 0x0e: // timing advance for #1 cylinder
							UpdateStatus(eSensor.x0E, (message[start + 2] - 128) / 2);
							break;
						case 0x0f: // air intake temperature
							switch (mPrefs.Units)
							{
								case eUnits.UNITS_SI:
								case eUnits.UNITS_UK:
									UpdateStatus(eSensor.x0F, message[start + 2] - 40);
									break;
								case eUnits.UNITS_US:
									UpdateStatus(eSensor.x0F, (message[start + 2] - 40) * 9 / 5 + 32);
									break;
							}
							break;
						case 0x10: // air flow rate
							switch (mPrefs.Units)
							{
								case eUnits.UNITS_SI:
								case eUnits.UNITS_UK:
									UpdateStatus(eSensor.x10, (message[start + 2] * 0x100 + message[start + 3]) / 100);
									break;
								case eUnits.UNITS_US:
									UpdateStatus(eSensor.x10, (message[start + 2] * 0x100 + message[start + 3]) * 132 / 100000);
									break;
							}
							break;
						case 0x11: // absolute throttle position
							UpdateStatus(eSensor.x11, message[start + 2] * 100 / 255);
							break;
						case 0x14: // O2 bank 1 sensor 1
						case 0x15: // O2 bank 1 sensor 2
						case 0x16: // O2 bank 1 sensor 3
						case 0x17: // O2 bank 1 sensor 4
						case 0x18: // O2 bank 2 sensor 1
						case 0x19: // O2 bank 2 sensor 2
						case 0x1a: // O2 bank 2 sensor 3
						case 0x1b: // O2 bank 2 sensor 4
							UpdateStatus(pid - 0x14 + eSensor.x14, message[start + 2] * 5);
							break;
					}
					break;
			}
			CalculateRate();
		}

		private void CalcActivePids()
		{
			int page, row;
			UInt32 activePids = 0;
			for (page = 0; page < PAGEMAX; page++)
			{
				if (mPrefs.Query == 0 || page == mSensorPrefs.Page)
					for (row = 0; row < mSensorPrefs.SensorCount[page]; row++)
						activePids |= (UInt32)(1L << (32 - pidMap[(int)activeSensor[mSensorPrefs.Sensor[page,row]]]));
			}
			OBDRead.GetSingleton().ReadSetActivePids(activePids);
		}

		private void InitPage()
		{
			InitSensorTable();
			RedrawSensorTable();
			CalcActivePids();
		}

		public void SensorInit(UInt32 supportedPids)
		{
			if (mSensorPrefs.SupportedPids != supportedPids)
			{
				mSensorPrefs.SupportedPids = supportedPids;
				InitSensorMap();
			}
			InitPage();
			SensorInsertBreak();
		}

		public void SensorSelectRow(int sx, int sy)
		{
			if (mSensorPrefs.SensorCount[mSensorPrefs.Page] > 0)
			{
				int graphHeight = mRectangle.Height / mSensorPrefs.SensorCount[mSensorPrefs.Page];
				SelectRow(false);
				mSelectedRow = sy / graphHeight;
				SelectRow(true);
			}
		}

		public void SensorPage(byte page)
		{
			mSelectedRow = -1;
			SelectRow(false);
			mSensorPrefs.Page = page;
			OBDGauge.GetSingleton().SetPage(page);
			if (mPrefs.Query != 0)
				InsertBreakPage(page);
			InitPage();
		}

		public void GaugeMoveUp()
		{
			if (mSelectedRow >= 1) 
			{
				int sensor = mSensorPrefs.Sensor[mSensorPrefs.Page,mSelectedRow];
				mSensorPrefs.Sensor[mSensorPrefs.Page,mSelectedRow] =
					mSensorPrefs.Sensor[mSensorPrefs.Page,mSelectedRow-1];
				mSensorPrefs.Sensor[mSensorPrefs.Page,mSelectedRow-1] = (byte)sensor;
				SelectRow(false);
				UpdateRow(mSelectedRow);
				mSelectedRow -= 1;
				UpdateRow(mSelectedRow);
			} 
		}

		public void GaugeMoveDown()
		{
			if (mSelectedRow >= 0
				&& mSelectedRow < mSensorPrefs.SensorCount[mSensorPrefs.Page]-1) 
			{
				int sensor = mSensorPrefs.Sensor[mSensorPrefs.Page,mSelectedRow];
				mSensorPrefs.Sensor[mSensorPrefs.Page,mSelectedRow] =
					mSensorPrefs.Sensor[mSensorPrefs.Page,mSelectedRow+1];
				mSensorPrefs.Sensor[mSensorPrefs.Page,mSelectedRow+1] = (byte)sensor;
				SelectRow(false);
				UpdateRow(mSelectedRow);
				mSelectedRow += 1;
				UpdateRow(mSelectedRow);
			}		
		}

		public void GaugeMoveToPage(byte page)
		{
			if (mSelectedRow >= 0) 
			{
				int row;
				if (mSensorPrefs.Page != page
					&& mSensorPrefs.SensorCount[page] < ROWMAX) 
				{
					mSensorPrefs.Sensor[page,mSensorPrefs.SensorCount[page]++] =
						mSensorPrefs.Sensor[mSensorPrefs.Page,mSelectedRow];
					for (row = mSelectedRow;
						row < mSensorPrefs.SensorCount[mSensorPrefs.Page]-1; row++)
						mSensorPrefs.Sensor[mSensorPrefs.Page,row] =
							mSensorPrefs.Sensor[mSensorPrefs.Page,row + 1];
					mSensorPrefs.SensorCount[mSensorPrefs.Page] -= 1;
					SensorPage(page);
					mSelectedRow = mSensorPrefs.SensorCount[page]-1;
				} 
			}
		}

		public void GaugeRemove()
		{
			if (mSelectedRow >= 0) 
			{
				int row;
				for (row = mSelectedRow;
					row < mSensorPrefs.SensorCount[mSensorPrefs.Page]-1; row++)
					mSensorPrefs.Sensor[mSensorPrefs.Page,row] =
						mSensorPrefs.Sensor[mSensorPrefs.Page,row+1];
				mSensorPrefs.SensorCount[mSensorPrefs.Page] -= 1;
				mSelectedRow -= 1;
				InitPage();
			}
		}

		public void GaugeDefaultLayout()
		{
			InitSensorMap();
			InitPage();
		}
	}
}
