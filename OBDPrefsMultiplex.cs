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

namespace OBDGauge
{
	/// <summary>
	/// form to edit Multiplex Engineering specific preferences
	/// </summary>
	public class OBDPrefsMultiplex : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox baudComboBox;
		private System.Windows.Forms.ComboBox addressComboBox;
		private System.Windows.Forms.ComboBox protocolComboBox;
		private System.Windows.Forms.Label label3;

		private Prefs_s mPrefs;

		public OBDPrefsMultiplex(Prefs_s newPrefs)
		{
			InitializeComponent();
			int i;
			for (i = 1; i < 256; i++)
				addressComboBox.Items.Add(i.ToString("X2"));
			mPrefs = newPrefs;
			baudComboBox.SelectedIndex = (int)mPrefs.Baud;
			if (mPrefs.Address == 0)
				addressComboBox.SelectedIndex = 0x25 - 1;
			else
				addressComboBox.SelectedIndex = mPrefs.Address - 1;
			protocolComboBox.SelectedIndex = (int)mPrefs.Protocol;
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
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.baudComboBox = new System.Windows.Forms.ComboBox();
			this.addressComboBox = new System.Windows.Forms.ComboBox();
			this.protocolComboBox = new System.Windows.Forms.ComboBox();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 26);
			this.label1.Size = new System.Drawing.Size(64, 20);
			this.label1.Text = "Baud:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 58);
			this.label2.Size = new System.Drawing.Size(64, 20);
			this.label2.Text = "Address:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 90);
			this.label3.Size = new System.Drawing.Size(64, 20);
			this.label3.Text = "Protocol:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// baudComboBox
			// 
			this.baudComboBox.Items.Add("19200");
			this.baudComboBox.Items.Add("9600");
			this.baudComboBox.Location = new System.Drawing.Point(80, 24);
			this.baudComboBox.Size = new System.Drawing.Size(88, 22);
			// 
			// addressComboBox
			// 
			this.addressComboBox.Location = new System.Drawing.Point(80, 56);
			this.addressComboBox.Size = new System.Drawing.Size(88, 22);
			// 
			// protocolComboBox
			// 
			this.protocolComboBox.Items.Add("Disabled");
			this.protocolComboBox.Items.Add("ISO");
			this.protocolComboBox.Items.Add("VPW");
			this.protocolComboBox.Items.Add("PWM");
			this.protocolComboBox.Items.Add("KWP");
			this.protocolComboBox.Location = new System.Drawing.Point(80, 88);
			this.protocolComboBox.Size = new System.Drawing.Size(88, 22);
			// 
			// FOBDPrefsMultiplex
			// 
			this.Controls.Add(this.protocolComboBox);
			this.Controls.Add(this.addressComboBox);
			this.Controls.Add(this.baudComboBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Text = "Multiplex Setup";
			this.Closed += new System.EventHandler(this.OBDPrefsMultiplex_Closed);

		}
		#endregion

		private void OBDPrefsMultiplex_Closed(object sender, System.EventArgs e)
		{
			mPrefs.Baud = (eBaud)baudComboBox.SelectedIndex;
			mPrefs.Address = (byte)(addressComboBox.SelectedIndex + 1);
			mPrefs.Protocol = (eProtocol)protocolComboBox.SelectedIndex;
		}
	}
}
