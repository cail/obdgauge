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
using System.Windows.Forms;
using System.Data;
using System.Reflection;

namespace OBDGauge
{
	/// <summary>
	/// main application form for OBD Gauge
	/// </summary>
	public class OBDGauge : System.Windows.Forms.Form
	{

		private System.Windows.Forms.Timer tickTimer;
		private System.Windows.Forms.ToolBar pageToolBar;
		private System.Windows.Forms.ImageList pageImageList;
		private System.Windows.Forms.ToolBarButton page1ToolBarButton;
		private System.Windows.Forms.ToolBarButton page2ToolBarButton;
		private System.Windows.Forms.ToolBarButton page3ToolBarButton;
		private System.Windows.Forms.ToolBarButton page4ToolBarButton;
		private System.Windows.Forms.MenuItem aboutMenuItem;
		private System.Windows.Forms.MenuItem preferencesMenuItem;
		private System.Windows.Forms.ToolBarButton toolBarButton1;
		private System.Windows.Forms.ToolBarButton exitToolBarButton;
		private System.Windows.Forms.MenuItem defaultLayoutMenuItem;
		private System.Windows.Forms.MenuItem clearHistoryMenuItem;
		private System.Windows.Forms.ToolBarButton toolBarButton2;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuMenuItem;
		private System.Windows.Forms.ToolBarButton toolBarButton3;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem exitMenuItem;
		private System.Windows.Forms.MenuItem DtcMenuItem;
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.MenuItem moveUpMenuItem;
		private System.Windows.Forms.MenuItem moveDownMenuItem;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem moveToPage1MenuItem;
		private System.Windows.Forms.MenuItem moveToPage2MenuItem;
		private System.Windows.Forms.MenuItem moveToPage3MenuItem;
		private System.Windows.Forms.MenuItem moveToPage4MenuItem;
		private System.Windows.Forms.MenuItem menuItem12;
		private System.Windows.Forms.MenuItem removeMenuItem;
		private System.Windows.Forms.MenuItem selectedGaugeMenuItem;

		private Prefs_s mPrefs = new Prefs_s();
		private System.Windows.Forms.MenuItem recordingMenuItem;
		private System.Windows.Forms.MenuItem recordingStartMenuItem;
		private System.Windows.Forms.MenuItem recordingStopMenuItem;
		private System.Windows.Forms.MenuItem recordingClearMenuItem;

		static OBDGauge mOBDGauge;

		static public OBDGauge GetSingleton()
		{
			return mOBDGauge;
		}

		public OBDGauge()
		{
			InitializeComponent();
			mOBDGauge = this;
			// explicitly load imagelist to avoid transparency problem
			int i;
			String[] iconName = {"Page1", "Page2", "Page3", "Page4", "Exit"};
			int[] iconIndex = { 1, 2, 3, 4, 6 };
			for (i = 0; i < iconName.Length; i++)
			{
				String name = "OBDGauge." + iconName[i] + ".ico";
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				Icon icon = new Icon(executingAssembly.GetManifestResourceStream(name));
				this.pageImageList.Images.Add(icon);
			}
			this.pageToolBar.ImageList = pageImageList;
			for (i = 0; i < iconName.Length; i++)
				this.pageToolBar.Buttons[iconIndex[i]].ImageIndex = i;

			tickTimer.Tick += new EventHandler(tickTimer_Tick);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(OBDGauge));
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.menuMenuItem = new System.Windows.Forms.MenuItem();
			this.selectedGaugeMenuItem = new System.Windows.Forms.MenuItem();
			this.moveUpMenuItem = new System.Windows.Forms.MenuItem();
			this.moveDownMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.moveToPage1MenuItem = new System.Windows.Forms.MenuItem();
			this.moveToPage2MenuItem = new System.Windows.Forms.MenuItem();
			this.moveToPage3MenuItem = new System.Windows.Forms.MenuItem();
			this.moveToPage4MenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem12 = new System.Windows.Forms.MenuItem();
			this.removeMenuItem = new System.Windows.Forms.MenuItem();
			this.clearHistoryMenuItem = new System.Windows.Forms.MenuItem();
			this.defaultLayoutMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.DtcMenuItem = new System.Windows.Forms.MenuItem();
			this.preferencesMenuItem = new System.Windows.Forms.MenuItem();
			this.aboutMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.exitMenuItem = new System.Windows.Forms.MenuItem();
			this.tickTimer = new System.Windows.Forms.Timer();
			this.pageToolBar = new System.Windows.Forms.ToolBar();
			this.toolBarButton3 = new System.Windows.Forms.ToolBarButton();
			this.page1ToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.page2ToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.page3ToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.page4ToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
			this.exitToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
			this.pageImageList = new System.Windows.Forms.ImageList();
			this.recordingMenuItem = new System.Windows.Forms.MenuItem();
			this.recordingStartMenuItem = new System.Windows.Forms.MenuItem();
			this.recordingStopMenuItem = new System.Windows.Forms.MenuItem();
			this.recordingClearMenuItem = new System.Windows.Forms.MenuItem();
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.Add(this.menuMenuItem);
			// 
			// menuMenuItem
			// 
			this.menuMenuItem.MenuItems.Add(this.selectedGaugeMenuItem);
			this.menuMenuItem.MenuItems.Add(this.clearHistoryMenuItem);
			this.menuMenuItem.MenuItems.Add(this.defaultLayoutMenuItem);
			this.menuMenuItem.MenuItems.Add(this.menuItem1);
			this.menuMenuItem.MenuItems.Add(this.recordingMenuItem);
			this.menuMenuItem.MenuItems.Add(this.DtcMenuItem);
			this.menuMenuItem.MenuItems.Add(this.preferencesMenuItem);
			this.menuMenuItem.MenuItems.Add(this.aboutMenuItem);
			this.menuMenuItem.MenuItems.Add(this.menuItem2);
			this.menuMenuItem.MenuItems.Add(this.exitMenuItem);
			this.menuMenuItem.Text = "&Menu";
			// 
			// selectedGaugeMenuItem
			// 
			this.selectedGaugeMenuItem.MenuItems.Add(this.moveUpMenuItem);
			this.selectedGaugeMenuItem.MenuItems.Add(this.moveDownMenuItem);
			this.selectedGaugeMenuItem.MenuItems.Add(this.menuItem7);
			this.selectedGaugeMenuItem.MenuItems.Add(this.moveToPage1MenuItem);
			this.selectedGaugeMenuItem.MenuItems.Add(this.moveToPage2MenuItem);
			this.selectedGaugeMenuItem.MenuItems.Add(this.moveToPage3MenuItem);
			this.selectedGaugeMenuItem.MenuItems.Add(this.moveToPage4MenuItem);
			this.selectedGaugeMenuItem.MenuItems.Add(this.menuItem12);
			this.selectedGaugeMenuItem.MenuItems.Add(this.removeMenuItem);
			this.selectedGaugeMenuItem.Text = "&Selected Gauge";
			// 
			// moveUpMenuItem
			// 
			this.moveUpMenuItem.Text = "Move &Up";
			this.moveUpMenuItem.Click += new System.EventHandler(this.moveUpMenuItem_Click);
			// 
			// moveDownMenuItem
			// 
			this.moveDownMenuItem.Text = "Move &Down";
			this.moveDownMenuItem.Click += new System.EventHandler(this.moveDownMenuItem_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Text = "-";
			// 
			// moveToPage1MenuItem
			// 
			this.moveToPage1MenuItem.Text = "Move to Page &1";
			this.moveToPage1MenuItem.Click += new System.EventHandler(this.moveToPage1MenuItem_Click);
			// 
			// moveToPage2MenuItem
			// 
			this.moveToPage2MenuItem.Text = "Move to Page &2";
			this.moveToPage2MenuItem.Click += new System.EventHandler(this.moveToPage2MenuItem_Click);
			// 
			// moveToPage3MenuItem
			// 
			this.moveToPage3MenuItem.Text = "Move to Page &3";
			this.moveToPage3MenuItem.Click += new System.EventHandler(this.moveToPage3MenuItem_Click);
			// 
			// moveToPage4MenuItem
			// 
			this.moveToPage4MenuItem.Text = "Move to Page &4";
			this.moveToPage4MenuItem.Click += new System.EventHandler(this.moveToPage4MenuItem_Click);
			// 
			// menuItem12
			// 
			this.menuItem12.Text = "-";
			// 
			// removeMenuItem
			// 
			this.removeMenuItem.Text = "&Remove";
			this.removeMenuItem.Click += new System.EventHandler(this.removeMenuItem_Click);
			// 
			// clearHistoryMenuItem
			// 
			this.clearHistoryMenuItem.Text = "&Clear History";
			this.clearHistoryMenuItem.Click += new System.EventHandler(this.clearHistoryMenuItem_Click);
			// 
			// defaultLayoutMenuItem
			// 
			this.defaultLayoutMenuItem.Text = "Default &Layout";
			this.defaultLayoutMenuItem.Click += new System.EventHandler(this.defaultLayoutMenuItem_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Text = "-";
			// 
			// DtcMenuItem
			// 
			this.DtcMenuItem.Text = "&Diagnostic Trouble Codes";
			this.DtcMenuItem.Click += new System.EventHandler(this.DtcMenuItem_Click);
			// 
			// preferencesMenuItem
			// 
			this.preferencesMenuItem.Text = "&Preferences";
			this.preferencesMenuItem.Click += new System.EventHandler(this.preferencesMenuItem_Click);
			// 
			// aboutMenuItem
			// 
			this.aboutMenuItem.Text = "&About OBD Gauge";
			this.aboutMenuItem.Click += new System.EventHandler(this.aboutMenuItem_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Text = "-";
			// 
			// exitMenuItem
			// 
			this.exitMenuItem.Text = "&Exit";
			this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
			// 
			// tickTimer
			// 
			this.tickTimer.Enabled = true;
			this.tickTimer.Interval = 1;
			this.tickTimer.Tick += new System.EventHandler(this.tickTimer_Tick);
			// 
			// pageToolBar
			// 
			this.pageToolBar.Buttons.Add(this.toolBarButton3);
			this.pageToolBar.Buttons.Add(this.page1ToolBarButton);
			this.pageToolBar.Buttons.Add(this.page2ToolBarButton);
			this.pageToolBar.Buttons.Add(this.page3ToolBarButton);
			this.pageToolBar.Buttons.Add(this.page4ToolBarButton);
			this.pageToolBar.Buttons.Add(this.toolBarButton1);
			this.pageToolBar.Buttons.Add(this.exitToolBarButton);
			this.pageToolBar.Buttons.Add(this.toolBarButton2);
			this.pageToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.pageToolBar_ButtonClick);
			// 
			// toolBarButton3
			// 
			this.toolBarButton3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// page1ToolBarButton
			// 
			this.page1ToolBarButton.ImageIndex = 0;
			this.page1ToolBarButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// page2ToolBarButton
			// 
			this.page2ToolBarButton.ImageIndex = 1;
			this.page2ToolBarButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// page3ToolBarButton
			// 
			this.page3ToolBarButton.ImageIndex = 2;
			this.page3ToolBarButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// page4ToolBarButton
			// 
			this.page4ToolBarButton.ImageIndex = 3;
			this.page4ToolBarButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// toolBarButton1
			// 
			this.toolBarButton1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// exitToolBarButton
			// 
			this.exitToolBarButton.ImageIndex = 4;
			// 
			// toolBarButton2
			// 
			this.toolBarButton2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// pageImageList
			// 
			this.pageImageList.ImageSize = new System.Drawing.Size(16, 16);
			// 
			// recordingMenuItem
			// 
			this.recordingMenuItem.MenuItems.Add(this.recordingStartMenuItem);
			this.recordingMenuItem.MenuItems.Add(this.recordingStopMenuItem);
			this.recordingMenuItem.MenuItems.Add(this.recordingClearMenuItem);
			this.recordingMenuItem.Text = "&Recording";
			// 
			// recordingStartMenuItem
			// 
			this.recordingStartMenuItem.Text = "Start";
			this.recordingStartMenuItem.Click += new System.EventHandler(this.recordingStartMenuItem_Click);
			// 
			// recordingStopMenuItem
			// 
			this.recordingStopMenuItem.Text = "Stop";
			this.recordingStopMenuItem.Click += new System.EventHandler(this.recordingStopMenuItem_Click);
			// 
			// recordingClearMenuItem
			// 
			this.recordingClearMenuItem.Text = "Clear";
			this.recordingClearMenuItem.Click += new System.EventHandler(this.recordingClearMenuItem_Click);
			// 
			// OBDGauge
			// 
			this.Controls.Add(this.pageToolBar);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu;
			this.Text = "OBD Gauge";
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OBDGauge_MouseDown);
			this.Load += new System.EventHandler(this.OBDGauge_Load);
			this.Closed += new System.EventHandler(this.OBDGauge_Closed);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.OBDGauge_Paint);

		}
		#endregion

		static void Main() 
		{
			Application.Run(new OBDGauge());
		}

		private void OBDGauge_Load(object sender, System.EventArgs e)
		{
			OBDSerial.CreateSingleton();
			OBDRead.CreateSingleton();
			OBDSensor.CreateSingleton();
			OBDPrefs.LoadPrefs(mPrefs);
			OBDRead.GetSingleton().Prefs = mPrefs;
			OBDSensor.GetSingleton().Prefs = mPrefs;
			OBDSensor.GetSingleton().Init();
		}

		private void OBDGauge_Closed(object sender, System.EventArgs e)
		{
			OBDSensor.DestroySingleton();
			OBDRead.DestroySingleton();
			OBDSerial.DestroySingleton();
		}

		private void OBDGauge_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			OBDSensor.GetSingleton().SensorRedraw();
		}

		private void OBDGauge_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			OBDSensor.GetSingleton().SensorSelectRow(e.X, e.Y);
		}

		bool mFirstTick = true;

		private void tickTimer_Tick(object sender, System.EventArgs e)
		{
			if (mFirstTick)
			{
				mFirstTick = false;
				OBDSensor.GetSingleton().SensorPage(0);
			}
			OBDRead.GetSingleton().ReadHandleTick();
		}

		private void pageToolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			byte page;
			if (e.Button == page1ToolBarButton)
				page = 0;
			else if (e.Button == page2ToolBarButton)
				page = 1;
			else if (e.Button == page3ToolBarButton)
				page = 2;
			else if (e.Button == page4ToolBarButton)
				page = 3;
			else if (e.Button == exitToolBarButton)
			{
				this.Close();
				return;
			}
			else
				return;
			OBDSensor.GetSingleton().SensorPage(page);
		}

		public void SetPage(int page)
		{
			ToolBarButton[] button = {page1ToolBarButton, page2ToolBarButton, page3ToolBarButton, page4ToolBarButton};
			int i;
			for (i = 0; i < 4; i++)
				button[i].Pushed = (i == page);
		}

		public void EnableGaugeMenu(bool enable)
		{
			selectedGaugeMenuItem.Enabled = enable;
		}

		private void aboutMenuItem_Click(object sender, System.EventArgs e)
		{
			OBDAbout OBDAbout = new OBDAbout();
			OBDAbout.ShowDialog();
		}

		private void preferencesMenuItem_Click(object sender, System.EventArgs e)
		{
			OBDRead.GetSingleton().ReadClose();
			OBDPrefs OBDPrefs = new OBDPrefs(mPrefs);
			OBDPrefs.ShowDialog();
			OBDPrefs.SavePrefs(mPrefs);
			OBDRead.GetSingleton().Prefs = mPrefs;
			OBDSensor.GetSingleton().Prefs = mPrefs;
			try
			{
				OBDRead.GetSingleton().ReadOpen();
			}
			catch
			{
			}
		}

		private void moveUpMenuItem_Click(object sender, System.EventArgs e)
		{
			OBDSensor.GetSingleton().GaugeMoveUp();
		}

		private void moveDownMenuItem_Click(object sender, System.EventArgs e)
		{
			OBDSensor.GetSingleton().GaugeMoveDown();
		}

		private void moveToPage1MenuItem_Click(object sender, System.EventArgs e)
		{
			OBDSensor.GetSingleton().GaugeMoveToPage(0);
		}

		private void moveToPage2MenuItem_Click(object sender, System.EventArgs e)
		{
			OBDSensor.GetSingleton().GaugeMoveToPage(1);
		}

		private void moveToPage3MenuItem_Click(object sender, System.EventArgs e)
		{
			OBDSensor.GetSingleton().GaugeMoveToPage(2);
		}

		private void moveToPage4MenuItem_Click(object sender, System.EventArgs e)
		{
			OBDSensor.GetSingleton().GaugeMoveToPage(3);
		}

		private void removeMenuItem_Click(object sender, System.EventArgs e)
		{
			OBDSensor.GetSingleton().GaugeRemove();		
		}

		private void clearHistoryMenuItem_Click(object sender, System.EventArgs e)
		{
			OBDSensor.GetSingleton().SensorClear();
		}

		private void defaultLayoutMenuItem_Click(object sender, System.EventArgs e)
		{
			OBDSensor.GetSingleton().GaugeDefaultLayout();
		}

		private void exitMenuItem_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void DtcMenuItem_Click(object sender, System.EventArgs e)
		{
			OBDCodes.CreateSingleton();
			OBDRead.GetSingleton().ReadReadCodes();
			OBDCodes.GetSingleton().ShowDialog();
			OBDCodes.DestroySingleton();
			OBDRead.GetSingleton().ReadRestart();
		}

		private void recordingStartMenuItem_Click(object sender, System.EventArgs e)
		{
			OBDSensor.GetSingleton().OpenRecordDatabase();
		}

		private void recordingStopMenuItem_Click(object sender, System.EventArgs e)
		{
			OBDSensor.GetSingleton().CloseRecordDatabase();
		}

		private void recordingClearMenuItem_Click(object sender, System.EventArgs e)
		{
			OBDSensor.GetSingleton().ClearRecordDatabase();
		}

	}
}
