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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using OpenNETCF.IO.Serial;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Ports;

namespace OBDGauge
{
	/// <summary>
	/// form to edit application preferences
	/// </summary>
	public class OBDPrefs : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ComboBox languageComboBox;
		private System.Windows.Forms.ComboBox unitsComboBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox scanComboBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox graphComboBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox interfaceComboBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label1;
	
		private Prefs_s mPrefs;
		private System.Windows.Forms.ComboBox portComboBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button setupButton;
		private System.Windows.Forms.Button btButton;
		//const String REGKEY = "Software\\OBD Gauge";
		private static BluetoothDeviceInfo[] bluetoothDeviceInfo = { };

		public OBDPrefs(Prefs_s newPrefs)
		{
			InitializeComponent();
			mPrefs = newPrefs;
			languageComboBox.SelectedIndex = mPrefs.Language;
			unitsComboBox.SelectedIndex = (int)mPrefs.Units;
			scanComboBox.SelectedIndex = mPrefs.Query;
			graphComboBox.SelectedIndex = mPrefs.GraphType;
			interfaceComboBox.SelectedIndex = (int)mPrefs.Interface;

			if (mPrefs.Port != null && mPrefs.Port.StartsWith("COM"))
			{
				portComboBox.SelectedItem = mPrefs.Port;
			}else{
				foreach(BluetoothDeviceInfo di in bluetoothDeviceInfo)
				{
					if (di.DeviceAddress.ToString() == mPrefs.Port)
					{
						portComboBox.SelectedItem = (string)di.DeviceName.ToString();
						break;
					}
				}
			}
		}

		protected override void Dispose( bool disposing )
		{
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.languageComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.unitsComboBox = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.scanComboBox = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.graphComboBox = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.interfaceComboBox = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.setupButton = new System.Windows.Forms.Button();
			this.portComboBox = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.btButton = new System.Windows.Forms.Button();

			// 
			// languageComboBox
			// 
			this.languageComboBox.Items.Add("English");
			this.languageComboBox.Items.Add("French");
			this.languageComboBox.Items.Add("German");
			this.languageComboBox.Items.Add("Dutch");
			this.languageComboBox.Items.Add("Spanish");
			this.languageComboBox.Location = new System.Drawing.Point(80, 24);
			this.languageComboBox.Size = new System.Drawing.Size(80, 22);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 26);
			this.label1.Size = new System.Drawing.Size(64, 20);
			this.label1.Text = "Language:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// unitsComboBox
			// 
			this.unitsComboBox.Items.Add("Metric");
			this.unitsComboBox.Items.Add("U.S.");
			this.unitsComboBox.Items.Add("U.K.");
			this.unitsComboBox.Location = new System.Drawing.Point(80, 56);
			this.unitsComboBox.Size = new System.Drawing.Size(80, 22);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 58);
			this.label2.Size = new System.Drawing.Size(64, 20);
			this.label2.Text = "Units:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// scanComboBox
			// 
			this.scanComboBox.Items.Add("All sensors");
			this.scanComboBox.Items.Add("Displayed sensors");
			this.scanComboBox.Location = new System.Drawing.Point(80, 88);
			this.scanComboBox.Size = new System.Drawing.Size(112, 22);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 90);
			this.label3.Size = new System.Drawing.Size(64, 20);
			this.label3.Text = "Scan:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// graphComboBox
			// 
			this.graphComboBox.Items.Add("Line");
			this.graphComboBox.Items.Add("Bar");
			this.graphComboBox.Location = new System.Drawing.Point(80, 120);
			this.graphComboBox.Size = new System.Drawing.Size(80, 22);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 122);
			this.label4.Size = new System.Drawing.Size(64, 20);
			this.label4.Text = "Graph:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// interfaceComboBox
			// 
			this.interfaceComboBox.Items.Add("ELM");
			this.interfaceComboBox.Items.Add("Multiplex");
			this.interfaceComboBox.Location = new System.Drawing.Point(80, 152);
			this.interfaceComboBox.Size = new System.Drawing.Size(80, 22);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 154);
			this.label5.Size = new System.Drawing.Size(64, 20);
			this.label5.Text = "Interface:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// setupButton
			// 
			this.setupButton.Location = new System.Drawing.Point(168, 152);
			this.setupButton.Size = new System.Drawing.Size(56, 20);
			this.setupButton.Text = "Setup";
			this.setupButton.Click += new System.EventHandler(this.setupButton_Click);
			// 
			// portComboBox
			// 

			this.portComboBox.Items.Add("COM1");
			this.portComboBox.Items.Add("COM2");
			this.portComboBox.Items.Add("COM3");
			this.portComboBox.Items.Add("COM4");
			this.portComboBox.Items.Add("COM5");
			this.portComboBox.Items.Add("COM6");
			this.portComboBox.Items.Add("COM7");
			this.portComboBox.Items.Add("COM8");

			foreach(BluetoothDeviceInfo di in bluetoothDeviceInfo){
			  this.portComboBox.Items.Add( di.DeviceName );
			}

			this.portComboBox.Location = new System.Drawing.Point(80, 184);
			this.portComboBox.Size = new System.Drawing.Size(80, 22);
			this.portComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 186);
			this.label6.Size = new System.Drawing.Size(64, 20);
			this.label6.Text = "Port:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;

			// btbutton
			this.btButton.Location = new System.Drawing.Point(180, 184);
			this.btButton.Size = new System.Drawing.Size(120, 20);
			this.btButton.Text = "BlueTooth Scan";
			this.btButton.Click += new System.EventHandler(this.btButton_Click);

			// 
			// FOBDPrefs
			// 
			this.Controls.Add(this.label6);
			this.Controls.Add(this.portComboBox);
			this.Controls.Add(this.setupButton);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.interfaceComboBox);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.graphComboBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.scanComboBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.unitsComboBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.languageComboBox);
			this.Controls.Add(this.btButton);
			this.Text = "Preferences";
			this.Closed += new System.EventHandler(this.OBDPrefs_Closed);

		}
		#endregion

		private void OBDPrefs_Closed(object sender, System.EventArgs e)
		{
			mPrefs.Language = (byte)languageComboBox.SelectedIndex;
			mPrefs.Units = (eUnits)unitsComboBox.SelectedIndex;
			mPrefs.Query = (byte)scanComboBox.SelectedIndex;
			mPrefs.GraphType = (byte)graphComboBox.SelectedIndex;
			mPrefs.Interface = (eInterface)interfaceComboBox.SelectedIndex;

			if (portComboBox.SelectedItem != null && ((string)portComboBox.SelectedItem).StartsWith("COM"))
			{
				mPrefs.Port = (string)portComboBox.SelectedItem;
			}else{
				foreach(BluetoothDeviceInfo di in bluetoothDeviceInfo)
				{
					if (di.DeviceName == portComboBox.SelectedItem)
					{
						mPrefs.Port = (string)di.DeviceAddress.ToString();
						break;
					}
				}
			}
			
		}

		public static void SavePrefs(Prefs_s Prefs)
		{
			String exePath = System.IO.Path.GetDirectoryName( 
				System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
			String filePath = Path.Combine(exePath, "Preference.dat");
			if (filePath.StartsWith("file:\\")) filePath = filePath.Substring(6);
			BinaryWriter writer = new BinaryWriter(new FileStream(filePath, FileMode.Create));
			writer.Write(Prefs.Language);
			writer.Write((byte)Prefs.Units);
			writer.Write(Prefs.Query);
			writer.Write(Prefs.GraphType);
			writer.Write((byte)Prefs.Interface);
			writer.Write(Prefs.Timeout);
			writer.Write((byte)Prefs.Baud);
			writer.Write(Prefs.Address);
			writer.Write((byte)Prefs.Protocol);
			writer.Write(Prefs.Port);
			writer.Close();
		}

		public static void LoadPrefs(Prefs_s Prefs)
		{
			BinaryReader reader = null;
			try
			{
				String exePath = System.IO.Path.GetDirectoryName( 
					System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
				String filePath = Path.Combine(exePath, "Preference.dat");
				if (filePath.StartsWith("file:\\")) filePath = filePath.Substring(6);
				reader = new BinaryReader(new FileStream(filePath, FileMode.Open));
				Prefs.Language = reader.ReadByte();
				Prefs.Units = (eUnits)reader.ReadByte();
				Prefs.Query = reader.ReadByte();
				Prefs.GraphType = reader.ReadByte();
				Prefs.Interface = (eInterface)reader.ReadByte();
				Prefs.Timeout = reader.ReadByte();
				Prefs.Baud = (eBaud)reader.ReadByte();
				Prefs.Address = reader.ReadByte();
				Prefs.Protocol = (eProtocol)reader.ReadByte();
				Prefs.Port = reader.ReadString();
				reader.Close();
			}
			catch (System.IO.FileNotFoundException)
			{
				Prefs.Language = 0;
				Prefs.Units = eUnits.UNITS_SI;
				Prefs.Query = 0;
				Prefs.GraphType = 0;
				Prefs.Interface = eInterface.INTERFACE_ELM;
				Prefs.Timeout = 0;
				Prefs.Baud = eBaud.BAUD_19200;
				Prefs.Address = 0x25;
				Prefs.Protocol = eProtocol.PROTOCOL_DISABLE;
				Prefs.Port = "COM1";
			}
			catch (System.IO.EndOfStreamException)
			{
				if (reader != null)
					reader.Close();
			}
		}
		
		private void setupButton_Click(object sender, System.EventArgs e)
		{
			if (interfaceComboBox.SelectedIndex == 0)
			{
				OBDPrefsElm prefsElm = new OBDPrefsElm(mPrefs);
				prefsElm.ShowDialog();
			}
			else
			{
				OBDPrefsMultiplex prefsMultiplex = new OBDPrefsMultiplex(mPrefs);
				prefsMultiplex.ShowDialog();
			}
		}

		private void btButton_Click(object sender, System.EventArgs e)
		{
			BluetoothRadio.PrimaryRadio.Mode = RadioMode.Discoverable;
			BluetoothClient bluetoothClient = new BluetoothClient();
			Cursor.Current = Cursors.WaitCursor;
			
			bluetoothDeviceInfo = bluetoothClient.DiscoverDevices(10, true, true, true);

			foreach(BluetoothDeviceInfo di in bluetoothDeviceInfo){
			  this.portComboBox.Items.Add( di.DeviceName );
			}
			//comboBox1.DataSource = bluetoothDeviceInfo;
			//comboBox1.DisplayMember = "DeviceName";
			//comboBox1.ValueMember = "DeviceAddress";
			this.portComboBox.Focus();
			Cursor.Current = Cursors.Default;   
		}

	}
}
