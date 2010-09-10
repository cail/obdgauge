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
	/// form to edit ELM specific preferences
	/// </summary>
	public class OBDPrefsElm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ComboBox timeoutComboBox;
		private System.Windows.Forms.Label label1;

		private Prefs_s mPrefs;

		public OBDPrefsElm(Prefs_s newPrefs)
		{
			InitializeComponent();
			mPrefs = newPrefs;
			timeoutComboBox.SelectedIndex = mPrefs.Timeout;
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
			this.timeoutComboBox = new System.Windows.Forms.ComboBox();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 26);
			this.label1.Size = new System.Drawing.Size(64, 20);
			this.label1.Text = "Timeout:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// timeoutComboBox
			// 
			this.timeoutComboBox.Items.Add("200 ms");
			this.timeoutComboBox.Items.Add("175 ms");
			this.timeoutComboBox.Items.Add("150 ms");
			this.timeoutComboBox.Items.Add("125 ms");
			this.timeoutComboBox.Items.Add("100 ms");
			this.timeoutComboBox.Items.Add("75 ms");
			this.timeoutComboBox.Items.Add("50 ms");
			this.timeoutComboBox.Items.Add("25 ms");
			this.timeoutComboBox.Location = new System.Drawing.Point(80, 24);
			this.timeoutComboBox.Size = new System.Drawing.Size(88, 22);
			// 
			// FOBDPrefsElm
			// 
			this.Controls.Add(this.timeoutComboBox);
			this.Controls.Add(this.label1);
			this.Text = "ELM Setup";
			this.Closed += new System.EventHandler(this.OBDPrefsElm_Closed);

		}
		#endregion

		private void OBDPrefsElm_Closed(object sender, System.EventArgs e)
		{
			mPrefs.Timeout = (byte)timeoutComboBox.SelectedIndex;
		}
	}
}
