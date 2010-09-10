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
using System.Text;
using System.IO;

namespace OBDGauge
{
	/// <summary>
	/// form to display and clear Diagnostic Trouble Codes (DTCs)
	/// </summary>
	public class OBDCodes : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListBox activeListBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListBox pendingListBox;
		private System.Windows.Forms.RadioButton engineOnRadioButton;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.RadioButton engineOffRadioButton;
		private System.Windows.Forms.Button clearButton;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox descriptionTextBox;
	
		static OBDCodes mOBDCodes;

		static public void CreateSingleton()
		{
			mOBDCodes = new OBDCodes();
		}

		static public OBDCodes GetSingleton()
		{
			return mOBDCodes;
		}

		static public void DestroySingleton()
		{
			mOBDCodes = null;
		}

		public OBDCodes()
		{
			InitializeComponent();
			DisplayWait();
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
			this.label1 = new System.Windows.Forms.Label();
			this.activeListBox = new System.Windows.Forms.ListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.pendingListBox = new System.Windows.Forms.ListBox();
			this.engineOnRadioButton = new System.Windows.Forms.RadioButton();
			this.label3 = new System.Windows.Forms.Label();
			this.engineOffRadioButton = new System.Windows.Forms.RadioButton();
			this.clearButton = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.descriptionTextBox = new System.Windows.Forms.TextBox();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Size = new System.Drawing.Size(56, 20);
			this.label1.Text = "Active:";
			// 
			// activeListBox
			// 
			this.activeListBox.Location = new System.Drawing.Point(8, 32);
			this.activeListBox.Size = new System.Drawing.Size(56, 100);
			this.activeListBox.SelectedIndexChanged += new System.EventHandler(this.activeListBox_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular);
			this.label2.Location = new System.Drawing.Point(80, 8);
			this.label2.Size = new System.Drawing.Size(56, 20);
			this.label2.Text = "Pending:";
			// 
			// pendingListBox
			// 
			this.pendingListBox.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular);
			this.pendingListBox.Location = new System.Drawing.Point(80, 32);
			this.pendingListBox.Size = new System.Drawing.Size(56, 100);
			this.pendingListBox.SelectedIndexChanged += new System.EventHandler(this.pendingListBox_SelectedIndexChanged);
			// 
			// engineOnRadioButton
			// 
			this.engineOnRadioButton.Enabled = false;
			this.engineOnRadioButton.Location = new System.Drawing.Point(168, 32);
			this.engineOnRadioButton.Size = new System.Drawing.Size(48, 20);
			this.engineOnRadioButton.Text = "On";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(152, 8);
			this.label3.Size = new System.Drawing.Size(80, 20);
			this.label3.Text = "Engine Lamp:";
			// 
			// engineOffRadioButton
			// 
			this.engineOffRadioButton.Enabled = false;
			this.engineOffRadioButton.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular);
			this.engineOffRadioButton.Location = new System.Drawing.Point(168, 56);
			this.engineOffRadioButton.Size = new System.Drawing.Size(48, 20);
			this.engineOffRadioButton.Text = "Off";
			// 
			// clearButton
			// 
			this.clearButton.Location = new System.Drawing.Point(152, 112);
			this.clearButton.Text = "Clear";
			this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 136);
			this.label4.Text = "Description:";
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.Location = new System.Drawing.Point(8, 160);
			this.descriptionTextBox.Multiline = true;
			this.descriptionTextBox.ReadOnly = true;
			this.descriptionTextBox.Size = new System.Drawing.Size(224, 104);
			this.descriptionTextBox.Text = "";
			// 
			// FOBDCodes
			// 
			this.Controls.Add(this.descriptionTextBox);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.clearButton);
			this.Controls.Add(this.engineOffRadioButton);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.engineOnRadioButton);
			this.Controls.Add(this.pendingListBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.activeListBox);
			this.Controls.Add(this.label1);
			this.Text = "Trouble Codes";

		}
		#endregion

		private const int DTCINDEXMAX = 15;
		private const int DTCTYPEACTIVE = 0;
		private const int DTCTYPEPENDING = 1;
		private const int DTCTYPEMAX = 2;

		private UInt16[,] mDtcCode = new UInt16[DTCTYPEMAX,DTCINDEXMAX];
		private String[,] mDtcText = new String[DTCTYPEMAX,DTCINDEXMAX];
		private int[] mDtcCount = new int[DTCTYPEMAX];
		private bool mMIL = false;

		private void DisplayWait()
		{
			descriptionTextBox.Text = "Please wait...";
		}

		private void ClearWait()
		{
			descriptionTextBox.Text = String.Empty;
		}

		private void GetDescription(String dtc)
		{
			DisplayWait();
			String exePath = System.IO.Path.GetDirectoryName( 
				System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
			String filePath = Path.Combine(exePath, "DTC-Generic.txt");
			StreamReader reader = new StreamReader(filePath);
			String line;
			bool found = false;
			String dtcString = dtc + "\t";
			while ((line = reader.ReadLine()) != null)
				if (line.StartsWith(dtcString))
				{
					found = true;
					break;
				}
			reader.Close();
			if (found)
				descriptionTextBox.Text = line.Substring(dtcString.Length);
			else
				descriptionTextBox.Text = "Please see appropriate service manual" +
					" for manufacturer defined trouble codes.";
		}

		private void ClearLists()
		{
			Array.Clear(mDtcCount, 0, mDtcCount.Length);
			mMIL = false;
			CodesReadComplete();
		}

		public void CodesSetMIL(bool MIL)
		{
			mMIL = MIL;
		}

		public void CodesSetClearCodes()
		{

		}

		public void CodesReceiveElmObd(byte[] message, int length)
		{
			char[] prefix = { 'P', 'C', 'B', 'U' };
			char[] code =
			{
				'0', '1', '2', '3', '4', '5', '6', '7', '8','9', 'A', 'B', 'C', 'D', 'E', 'F'
			};
			int dtcType, i;
			UInt16 dtcCode;
			if (length < 7)
				return;
			if (message[0] == 0x43)
				dtcType = DTCTYPEACTIVE;
			else if (message[0] == 0x47)
				dtcType = DTCTYPEPENDING;
			else
				return;
			for (i = 0; i < 3; i++)
			{
				if (mDtcCount[dtcType] < DTCINDEXMAX)
				{
					dtcCode = (UInt16)((message[i * 2 + 1] << 8) | message[i * 2 + 2]);
					if (dtcCode != 0)
					{
						StringBuilder dtcText = new StringBuilder();
						mDtcCode[dtcType,mDtcCount[dtcType]] = dtcCode;
						dtcText.Append(prefix[(dtcCode >> 14) & 0x03]);
						dtcText.Append(code[(dtcCode >> 12) & 0x03]);
						dtcText.Append(code[(dtcCode >> 8) & 0x0f]);
						dtcText.Append(code[(dtcCode >> 4) & 0x0f]);
						dtcText.Append(code[dtcCode & 0x0f]);
						mDtcText[dtcType,mDtcCount[dtcType]] = dtcText.ToString();
						mDtcCount[dtcType]++;
					}
				}
			}
		}

		public void CodesReadComplete()
		{
			int i;
			ClearWait();
			if (mMIL)
				engineOnRadioButton.Checked = true;
			else
				engineOffRadioButton.Checked = true;
			activeListBox.Items.Clear();
			for (i = 0; i < mDtcCount[DTCTYPEACTIVE]; i++)
				activeListBox.Items.Add(mDtcText[DTCTYPEACTIVE,i]);
			if (mDtcCount[DTCTYPEACTIVE] > 0)
				activeListBox.SelectedIndex = 0;
			pendingListBox.Items.Clear();
			for (i = 0; i < mDtcCount[DTCTYPEPENDING]; i++)
				pendingListBox.Items.Add(mDtcText[DTCTYPEPENDING,i]);
		}

		private void clearButton_Click(object sender, System.EventArgs e)
		{
			if (MessageBox.Show(
				"Turn off the engine lamp and erase all diagnostic data?\nPlease shut off engine but leave ignition on before proceeding.",
				"Clear Trouble Codes", MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1) == DialogResult.OK)
			{
				ClearLists();
				DisplayWait();
				OBDRead.GetSingleton().ReadClearCodes();
			}
		}

		private void activeListBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (activeListBox.SelectedIndex >= 0)
			{
				pendingListBox.SelectedIndex = -1;
				GetDescription(activeListBox.Items[activeListBox.SelectedIndex].ToString());
			}
		}

		private void pendingListBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (pendingListBox.SelectedIndex >= 0)
			{
				activeListBox.SelectedIndex = -1;
				GetDescription(pendingListBox.Items[pendingListBox.SelectedIndex].ToString());
			}
		}

	}
}
