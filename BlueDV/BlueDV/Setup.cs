using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Windows.Forms;
using BlueDV.Properties;
using STA.Settings;

namespace BlueDV
{
	// Token: 0x02000049 RID: 73
	public partial class Setup : Form
	{
		// Token: 0x0600057F RID: 1407 RVA: 0x0002F134 File Offset: 0x0002D334
		private void HelpNXDNlinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(new ProcessStartInfo("https://www.pa7lim.nl/nxdnhelp"));
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x00002F42 File Offset: 0x00001142
		private void DMRtypeSelectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		// Token: 0x06000581 RID: 1409 RVA: 0x00002F42 File Offset: 0x00001142
		private void DVMEGAPowerTrackBar_Scroll(object sender, EventArgs e)
		{
		}

		// Token: 0x06000582 RID: 1410 RVA: 0x00002F42 File Offset: 0x00001142
		private void AMBEtypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		// Token: 0x06000583 RID: 1411 RVA: 0x00002F42 File Offset: 0x00001142
		private void label35_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x06000584 RID: 1412 RVA: 0x00002F42 File Offset: 0x00001142
		private void cancel_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x06000585 RID: 1413 RVA: 0x00002F42 File Offset: 0x00001142
		private void radioButtonBrandmeister_CheckedChanged(object sender, EventArgs e)
		{
		}

		// Token: 0x06000586 RID: 1414 RVA: 0x0002F148 File Offset: 0x0002D348
		private void setAMBEBoxSettings()
		{
			if (this.dvdonglecheckBox.Checked)
			{
				this.enableAMBEServerCheckBox.Enabled = true;
				this.AMBEServerHostTextBox.Enabled = true;
				this.AMBEServerHostPortTextBox.Enabled = true;
				this.AMBEDMRid.Enabled = true;
				this.AMBENXDNid.Enabled = true;
				this.AMBEspeed.Enabled = true;
				this.comboBox3.Enabled = true;
				this.AMBEBeepcheckBox.Enabled = true;
				this.PTTKeyingcheckBox.Enabled = true;
				this.COMportPTTcomboBox.Enabled = true;
				this.killTimerComboBox.Enabled = true;
				this.DSTARSlowDataTextBox.Enabled = true;
				this.RXRTSradioButton.Enabled = true;
				this.RXDTRradioButton.Enabled = true;
				this.RXHIGHradioButton.Enabled = true;
				this.RXLOWradioButton.Enabled = true;
				this.PTTCTSradioButton.Enabled = true;
				this.PTTDSRradioButton.Enabled = true;
				this.PTTHIGHradioButton.Enabled = true;
				this.PTTLOWradioButton.Enabled = true;
			}
			else
			{
				this.enableAMBEServerCheckBox.Enabled = false;
				this.AMBEServerHostTextBox.Enabled = false;
				this.AMBEServerHostPortTextBox.Enabled = false;
				this.AMBEDMRid.Enabled = false;
				this.AMBENXDNid.Enabled = false;
				this.AMBEspeed.Enabled = false;
				this.comboBox3.Enabled = false;
				this.AMBEBeepcheckBox.Enabled = false;
				this.COMportPTTcomboBox.Enabled = false;
				this.PTTKeyingcheckBox.Enabled = false;
				this.killTimerComboBox.Enabled = false;
				this.DSTARSlowDataTextBox.Enabled = false;
				this.RXRTSradioButton.Enabled = false;
				this.RXDTRradioButton.Enabled = false;
				this.RXHIGHradioButton.Enabled = false;
				this.RXLOWradioButton.Enabled = false;
				this.PTTCTSradioButton.Enabled = false;
				this.PTTDSRradioButton.Enabled = false;
				this.PTTHIGHradioButton.Enabled = false;
				this.PTTLOWradioButton.Enabled = false;
			}
			if (this.PTTKeyingcheckBox.Checked)
			{
				this.COMportPTTcomboBox.Enabled = true;
				this.RXIndicatorEnablecheckBox.Enabled = true;
				this.RXRTSradioButton.Enabled = true;
				this.RXDTRradioButton.Enabled = true;
				this.RXHIGHradioButton.Enabled = true;
				this.RXLOWradioButton.Enabled = true;
				this.PTTCTSradioButton.Enabled = true;
				this.PTTDSRradioButton.Enabled = true;
				this.PTTHIGHradioButton.Enabled = true;
				this.PTTLOWradioButton.Enabled = true;
				return;
			}
			this.COMportPTTcomboBox.Enabled = false;
			this.RXIndicatorEnablecheckBox.Enabled = false;
			this.RXRTSradioButton.Enabled = false;
			this.RXDTRradioButton.Enabled = false;
			this.RXHIGHradioButton.Enabled = false;
			this.RXLOWradioButton.Enabled = false;
			this.PTTCTSradioButton.Enabled = false;
			this.PTTDSRradioButton.Enabled = false;
			this.PTTHIGHradioButton.Enabled = false;
			this.PTTLOWradioButton.Enabled = false;
		}

		// Token: 0x06000587 RID: 1415 RVA: 0x0002F448 File Offset: 0x0002D648
		private void dvdonglecheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this.setAMBEBoxSettings();
		}

		// Token: 0x06000588 RID: 1416 RVA: 0x0002F450 File Offset: 0x0002D650
		private void Setup_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.S && Control.ModifierKeys == Keys.Control)
			{
				e.SuppressKeyPress = true;
				this.save.PerformClick();
			}
			if (e.KeyCode == Keys.X && Control.ModifierKeys == Keys.Control)
			{
				e.SuppressKeyPress = true;
				this.cancel.PerformClick();
			}
		}

		// Token: 0x06000589 RID: 1417 RVA: 0x0002F4AD File Offset: 0x0002D6AD
		private void YSFradioButton_Click(object sender, EventArgs e)
		{
			if (this.YSFradioButton.Checked)
			{
				this._FusionAutoStartprotocol = "YSF";
				return;
			}
			this._FusionAutoStartprotocol = "FCS";
		}

		// Token: 0x0600058A RID: 1418 RVA: 0x0002F4D3 File Offset: 0x0002D6D3
		private void RXradiobutton_Click(object sender, EventArgs e)
		{
			if (this.RXRTSradioButton.Checked)
			{
				this._RXIndicator = "RTS";
				return;
			}
			this._RXIndicator = "DTR";
		}

		// Token: 0x0600058B RID: 1419 RVA: 0x0002F4F9 File Offset: 0x0002D6F9
		private void RXradiobuttonHighLow_Click(object sender, EventArgs e)
		{
			if (this.RXHIGHradioButton.Checked)
			{
				this._RXIndicatorHighLow = "HIGH";
				return;
			}
			this._RXIndicatorHighLow = "LOW";
		}

		// Token: 0x0600058C RID: 1420 RVA: 0x0002F51F File Offset: 0x0002D71F
		private void PTTradiobutton_Click(object sender, EventArgs e)
		{
			if (this.PTTCTSradioButton.Checked)
			{
				this._PTTIndicator = "CTS";
				return;
			}
			this._PTTIndicator = "DSR";
		}

		// Token: 0x0600058D RID: 1421 RVA: 0x0002F545 File Offset: 0x0002D745
		private void PTTradiobuttonHighLow_Click(object sender, EventArgs e)
		{
			if (this.PTTHIGHradioButton.Checked)
			{
				this._PTTIndicatorHighLow = "HIGH";
				return;
			}
			this._PTTIndicatorHighLow = "LOW";
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x0002F56B File Offset: 0x0002D76B
		private void defaultDSTARReflectorTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == ' ')
			{
				MessageBox.Show("Do not use whitespaces");
				e.Handled = true;
			}
		}

		// Token: 0x0600058F RID: 1423 RVA: 0x00002F42 File Offset: 0x00001142
		private void defaultDSTARReflectorTextBox_KeyDown(object sender, KeyEventArgs e)
		{
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x00002F42 File Offset: 0x00001142
		private void BootProtoSelectedIndexChanged(object sender, EventArgs e)
		{
		}

		// Token: 0x06000591 RID: 1425 RVA: 0x00002F42 File Offset: 0x00001142
		private void label14_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x0002F589 File Offset: 0x0002D789
		private void comboBoxDMRPlus_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.dmrplusmaster = DMRPlusHostParser.lookupHostname(this.comboBoxDMRPlus.SelectedIndex);
			this.dmrpluspassword = DMRPlusHostParser.lookupDMRPassword(this.dmrplusmaster);
			this.dmrplusport = DMRPlusHostParser.lookupDMRPort(this.dmrplusmaster);
		}

		// Token: 0x06000593 RID: 1427 RVA: 0x00002F42 File Offset: 0x00001142
		private void label1_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x06000594 RID: 1428 RVA: 0x0002F5C4 File Offset: 0x0002D7C4
		public Setup()
		{
			this.InitializeComponent();
			base.Icon = Resources.web_hi_res_512;
			this.save.DialogResult = DialogResult.OK;
			this.cancel.DialogResult = DialogResult.Cancel;
			switch (utils.RunningPlatform())
			{
			case utils.Platform.Windows:
				this.Text = "BlueDV for Windows";
				break;
			case utils.Platform.Linux:
				this.Text = "BlueDV for Linux";
				this.dvdonglecheckBox.Checked = false;
				this.dvdonglecheckBox.Enabled = false;
				break;
			case utils.Platform.Mac:
				this.Text = "BlueDV for OSX";
				this.dvdonglecheckBox.Checked = false;
				this.dvdonglecheckBox.Enabled = false;
				break;
			}
			INIFile inifile;
			if (utils.RunningPlatform() == utils.Platform.Windows)
			{
				inifile = new INIFile(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\" + information.configFile);
			}
			else
			{
				inifile = new INIFile(information.configFile);
			}
			this._mycall = inifile.GetValue("GENERAL", "myCall", "NOCALL");
			this._comport = inifile.GetValue("GENERAL", "comport", "/dev/ttyUSB0");
			this._invertDTRRadio = inifile.GetValue("GENERAL", "invertDTR", false);
			this._dvdongle = inifile.GetValue("GENERAL", "dvdongle", false);
			this._saveQSOLog = inifile.GetValue("GENERAL", "saveQSOLog", false);
			this._RXTXColors = inifile.GetValue("GENERAL", "RXTXColors", false);
			this._DVMEGAPower = inifile.GetValue("GENERAL", "DVMEGAPower", "254");
			this._startProtocol = inifile.GetValue("GENERAL", "StartProtocol", "None");
			this._latitude = inifile.GetValue("GENERAL", "latitude", "+53.0570");
			this._longitude = inifile.GetValue("GENERAL", "longitude", "+005.0739");
			if (this._longitude == "+004.0739")
			{
				this._longitude = "+003.0739";
			}
			if (this._latitude == "+51.0570")
			{
				this._latitude = "+50.0570";
			}
			this._alwaysOnTop = inifile.GetValue("GENERAL", "alwaysOnTop", false);
			this._invertRXTX = inifile.GetValue("GENERAL", "invertRXTXColors", false);
			this._language = inifile.GetValue("GENERAL", "language", "English");
			this._AMBEtype = inifile.GetValue("AMBE", "AMBEtype", "AMBE3000");
			this._AMBEDMRid = inifile.GetValue("AMBE", "AMBEDMRid", "2043000");
			this._AMBENXDNid = inifile.GetValue("AMBE", "AMBENXDNid", "9999");
			this._AMBEspeed = inifile.GetValue("AMBE", "AMBEspeed", "230400");
			this._enableAMBEServer = inifile.GetValue("AMBE", "enableAMBEServer", false);
			this._AMBEServerHost = inifile.GetValue("AMBE", "AMBEServerHost", "192.168.1.10");
			this._AMBEServerHostPort = inifile.GetValue("AMBE", "AMBEServerHostPort", "2460");
			this._AMBEBeep = inifile.GetValue("AMBE", "enableRoger", true);
			this._comportAMBE = inifile.GetValue("AMBE", "comport", "/dev/ttyUSB0");
			this._PTTKeyingEnable = inifile.GetValue("AMBE", "PTTKeying", false);
			this._COMportPTT = inifile.GetValue("AMBE", "PTTCOMPort", "/dev/ttyUSB0");
			this._enableRXIndicator = inifile.GetValue("AMBE", "enableRXIndicator", true);
			this._RXIndicator = inifile.GetValue("AMBE", "RXIndicator", "RTS");
			this._RXIndicatorHighLow = inifile.GetValue("AMBE", "RXIndicatorHighLow", "HIGH");
			this._PTTIndicator = inifile.GetValue("AMBE", "PTTIndicator", "CTS");
			this._PTTIndicatorHighLow = inifile.GetValue("AMBE", "PTTIndicatorHighLow", "HIGH");
			this._killswitch = inifile.GetValue("AMBE", "killswitch", "5");
			this._dstarslowdata = inifile.GetValue("AMBE", "DSTARSlowDataText", "BlueDV by PA7LIM");
			this._DMRid = inifile.GetValue("DMR", "dmrid", "2040000");
			this._DMRidSimple = inifile.GetValue("DMR", "dmridsimplemode", "2040000");
			this._freqDMR = inifile.GetValue("GENERAL", "frequency", "434300000");
			this._qrg = inifile.GetValue("DMR", "qrg", "0");
			this._dmrmaster = inifile.GetValue("DMR", "DMRmaster", "213.222.29.197");
			this._DMRpassword = inifile.GetValue("DMR", "DMRpassword", "passw0rd");
			this._TGIFpassword = inifile.GetValue("DMR", "TGIFpassword", "passw0rd");
			this._dmrplusmaster = inifile.GetValue("DMR", "DMRplusmaster", "193.253.109.97");
			this._DMRtypeSelection = inifile.GetValue("DMR", "DMRtypeSelection", "BM");
			this._noInbandData = inifile.GetValue("DMR", "noInbandData", false);
			this._QTHlocator = inifile.GetValue("FUSION", "QTHlocation", "JO22MB");
			this._modeTimerNet = inifile.GetValue("GENERAL", "modeTimerNet", "5");
			this._DSTARmodule = inifile.GetValue("DSTAR", "DSTARmodule", "D");
			this._defaultDSTARReflector = inifile.GetValue("DSTAR", "defaultReflector", "");
			this._modeTimerRF = inifile.GetValue("GENERAL", "modeTimerRF", "10");
			this._APRS = inifile.GetValue("DSTAR", "APRS", false);
			this._DMREnabledAtBoot = inifile.GetValue("DMR", "autostart", false);
			this._DSTAREnabledAtBoot = inifile.GetValue("DSTAR", "autostart", false);
			this._FusionEnabledAtBoot = inifile.GetValue("FUSION", "autostart", false);
			this._defaultFCSReflector = inifile.GetValue("FUSION", "defaultFCSReflector", "FCS00401");
			this._FusionAutoStartprotocol = inifile.GetValue("FUSION", "autostartProtocol", "FCS");
			this._defaultYSFReflector = inifile.GetValue("FUSION", "defaultYSFReflector", "NL-CENTRAL");
			this.mycall.Text = this._mycall;
			this.dmrid.Text = this._DMRid;
			this.DMRidSimpleTextBox.Text = this._DMRidSimple;
			this.freqDMR.Text = this._freqDMR;
			this.dmrmaster = this._dmrmaster;
			this.dmrplusmaster = this._dmrplusmaster;
			this.DMRpassword.Text = this._DMRpassword;
			this.TGIFPassword.Text = this._TGIFpassword;
			this.QTHlocatorTextBox.Text = this._QTHlocator;
			this.longitudeMaskedTextBox.Text = this._longitude.Substring(1, this._longitude.Length - 1);
			this.latitudeMaskedTextBox.Text = this._latitude.Substring(1, this._latitude.Length - 1);
			this.PlusMinLoncomboBox.SelectedItem = this._longitude.Substring(0, 1);
			this.PlusMinLatcomboBox.SelectedItem = this._latitude.Substring(0, 1);
			this.modeTimerRF.Text = this._modeTimerRF;
			this.modeTimerNet.Text = this._modeTimerNet;
			this.defaultDSTARReflectorTextBox.Text = this._defaultDSTARReflector;
			this.DSTARSlowDataTextBox.Text = this._dstarslowdata;
			this.DMRtypeSelectionComboBox.DataSource = Enum.GetValues(typeof(information.DMRMODUS));
			this.DMRtypeSelectionComboBox.SelectedIndex = this.DMRtypeSelectionComboBox.FindString(this._DMRtypeSelection);
			this.killTimerComboBox.SelectedIndex = this.killTimerComboBox.FindString(this._killswitch);
			this.AMBEspeed.SelectedItem = this._AMBEspeed;
			this.AMBEDMRid.Text = this._AMBEDMRid;
			this.AMBENXDNid.Text = this._AMBENXDNid;
			this.AMBEtypeComboBox.SelectedItem = this._AMBEtype;
			this.languageComboBox.SelectedIndex = this.languageComboBox.FindString(this._language);
			this.invertDTRRadioCheckBox.Checked = this._invertDTRRadio;
			if (this._enableAMBEServer)
			{
				this.enableAMBEServerCheckBox.Checked = true;
			}
			else
			{
				this.enableAMBEServerCheckBox.Checked = false;
			}
			if (this._AMBEBeep)
			{
				this.AMBEBeepcheckBox.Checked = true;
			}
			else
			{
				this.AMBEBeepcheckBox.Checked = false;
			}
			if (this._enableRXIndicator)
			{
				this.RXIndicatorEnablecheckBox.Checked = true;
			}
			else
			{
				this.RXIndicatorEnablecheckBox.Checked = false;
			}
			if (this._alwaysOnTop)
			{
				this.alwaysOnTopcheckBox.Checked = true;
			}
			else
			{
				this.alwaysOnTopcheckBox.Checked = false;
			}
			if (this._invertRXTX)
			{
				this.invertRXTXcheckBox.Checked = true;
			}
			else
			{
				this.invertRXTXcheckBox.Checked = false;
			}
			if (this._PTTKeyingEnable)
			{
				this.PTTKeyingcheckBox.Checked = true;
			}
			else
			{
				this.PTTKeyingcheckBox.Checked = false;
			}
			this.AMBEServerHostTextBox.Text = this._AMBEServerHost;
			this.AMBEServerHostPortTextBox.Text = this._AMBEServerHostPort;
			try
			{
				int num = int.Parse(this._DVMEGAPower);
				if (num < 10 || num > 254)
				{
					num = 254;
				}
				this.DVMEGAPowerTrackBar.Value = num;
			}
			catch (FormatException ex)
			{
				Console.WriteLine(ex.Message);
			}
			this.comboBox2.DataSource = JSONCallQuery.getMasterList();
			this.comboBoxDMRPlus.DataSource = DMRPlusHostParser.returnTotalList();
			try
			{
				this.comboBoxDMRPlus.SelectedIndex = this.comboBoxDMRPlus.FindStringExact(DMRPlusHostParser.lookupLogicName(this._dmrplusmaster));
			}
			catch (ArgumentOutOfRangeException)
			{
			}
			this.YSFreflectorListcomboBox.DataSource = JSONCallQuery.getYSFMasterList();
			try
			{
				this.YSFreflectorListcomboBox.SelectedIndex = JSONCallQuery.searchYSFhost(this._defaultYSFReflector);
			}
			catch (ArgumentOutOfRangeException)
			{
			}
			this.checkBoxsaveQSOlog.Checked = this._saveQSOLog;
			this.RXTXColorsCheckBox.Checked = this._RXTXColors;
			this.APRScheckBox.Checked = this._APRS;
			this.PTTKeyingcheckBox.Checked = this._PTTKeyingEnable;
			this.dvdonglecheckBox.Checked = this._dvdongle;
			this.checkBoxDMREnabledAtBoot.Checked = this._DMREnabledAtBoot;
			this.checkBoxDSTAREnabledAtBoot.Checked = this._DSTAREnabledAtBoot;
			this.checkBoxFusionEnabledAtBoot.Checked = this._FusionEnabledAtBoot;
			this.noInbandDataCheckBox.Checked = this._noInbandData;
			int num2 = JSONCallQuery.lookupDMRmaster(this._dmrmaster);
			if (num2 > 0)
			{
				this.comboBox2.SelectedIndex = num2;
			}
			this.startProtocolComboBox.SelectedItem = this._startProtocol;
			int num3 = this.listBox1.FindStringExact(this._qrg);
			if (num3 == -1)
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					MessageBox.Show("Item is not available");
					break;
				case information.LANGUAGE.JAPANESE:
					MessageBox.Show("アイテムは利用できません");
					break;
				case information.LANGUAGE.CHINEES:
					MessageBox.Show("Item is not available");
					break;
				case information.LANGUAGE.KOREAN:
					MessageBox.Show("항목이 유효하지 않음");
					break;
				}
			}
			else
			{
				this.listBox1.SetSelected(num3, true);
			}
			if (this._FusionAutoStartprotocol == "YSF")
			{
				this.YSFradioButton.Checked = true;
			}
			else
			{
				this.FCSradioButton.Checked = true;
			}
			if (this._RXIndicator == "RTS")
			{
				this.RXRTSradioButton.Checked = true;
			}
			else
			{
				this.RXDTRradioButton.Checked = true;
			}
			if (this._RXIndicatorHighLow == "HIGH")
			{
				this.RXHIGHradioButton.Checked = true;
			}
			else
			{
				this.RXLOWradioButton.Checked = true;
			}
			if (this._PTTIndicator == "CTS")
			{
				this.PTTCTSradioButton.Checked = true;
			}
			else
			{
				this.PTTDSRradioButton.Checked = true;
			}
			if (this._PTTIndicatorHighLow == "HIGH")
			{
				this.PTTHIGHradioButton.Checked = true;
			}
			else
			{
				this.PTTLOWradioButton.Checked = true;
			}
			string defaultFCSReflector = this._defaultFCSReflector;
			string text = defaultFCSReflector.Substring(6, 2);
			string text2 = defaultFCSReflector.Substring(0, 6);
			string[] array = new string[100];
			for (int i = 0; i <= 99; i++)
			{
				array[i] = i.ToString("D2");
			}
			this.FCSReflectorModulecomboBox.DataSource = array;
			this.FCSReflectorModulecomboBox.SelectedItem = text;
			this.FCSReflectorHostcomboBox.DataSource = new BindingSource(downloadHostsTable.returnFCS(), null);
			this.FCSReflectorHostcomboBox.DisplayMember = "Key";
			this.FCSReflectorHostcomboBox.ValueMember = "Key";
			this.FCSReflectorHostcomboBox.SelectedValue = text2;
			foreach (string text3 in SerialPort.GetPortNames())
			{
				this.comboBox1.Items.Add(text3);
				this.comboBox3.Items.Add(text3);
				this.COMportPTTcomboBox.Items.Add(text3);
			}
			int num4 = this.comboBox1.FindStringExact(this._comport);
			this.comboBox1.SelectedIndex = num4;
			int num5 = this.comboBox3.FindStringExact(this._comportAMBE);
			this.comboBox3.SelectedIndex = num5;
			this.COMportPTTcomboBox.SelectedIndex = this.COMportPTTcomboBox.FindString(this._COMportPTT);
			int num6 = this.DSTARmodule.FindString(this._DSTARmodule);
			this.DSTARmodule.SelectedIndex = num6;
			this.setAMBEBoxSettings();
		}

		// Token: 0x06000595 RID: 1429 RVA: 0x000303A0 File Offset: 0x0002E5A0
		private void saveButton_Click(object sender, EventArgs e)
		{
			if (this.mycall.Text.Length < 2)
			{
				MessageBox.Show("Enter a correct call!");
				return;
			}
			if (this.freqDMR.Text.Length < 9)
			{
				MessageBox.Show("Frequency must be at least 9 characters long.");
				return;
			}
			if (this.modeTimerRF.Text.Length < 1)
			{
				MessageBox.Show("Fill in correct value in mode timer RF");
				return;
			}
			if (this.modeTimerNet.Text.Length < 1)
			{
				MessageBox.Show("Fill in correct value in mode timer NET");
				return;
			}
			INIFile inifile;
			if (utils.RunningPlatform() == utils.Platform.Windows)
			{
				inifile = new INIFile(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\" + information.configFile);
			}
			else
			{
				inifile = new INIFile(information.configFile);
			}
			inifile.SetValue("GENERAL", "myCall", this.mycall.Text.PadRight(8).Substring(0, 8).Trim());
			string text = null;
			try
			{
				text = this.comboBox1.GetItemText(this.comboBox1.SelectedItem);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error : " + ex.StackTrace);
			}
			if (text != null)
			{
				inifile.SetValue("GENERAL", "comport", this.comboBox1.GetItemText(this.comboBox1.SelectedItem));
			}
			else
			{
				inifile.SetValue("GENERAL", "comport", "COM1");
			}
			inifile.SetValue("GENERAL", "comport", this.comboBox1.GetItemText(this.comboBox1.SelectedItem));
			inifile.SetValue("GENERAL", "invertDTR", this.invertDTRRadioCheckBox.Checked);
			inifile.SetValue("GENERAL", "saveQSOLog", this.checkBoxsaveQSOlog.Checked);
			inifile.SetValue("GENERAL", "RXTXColors", this.RXTXColorsCheckBox.Checked);
			inifile.SetValue("GENERAL", "StartProtocol", this.startProtocolComboBox.GetItemText(this.startProtocolComboBox.SelectedItem));
			inifile.SetValue("GENERAL", "dvdongle", this.dvdonglecheckBox.Checked);
			inifile.SetValue("GENERAL", "frequency", this.freqDMR.Text);
			inifile.SetValue("GENERAL", "modeTimerRF", this.modeTimerRF.Text);
			inifile.SetValue("GENERAL", "modeTimerNet", this.modeTimerNet.Text);
			inifile.SetValue("GENERAL", "DVMEGAPower", this.DVMEGAPowerTrackBar.Value.ToString());
			inifile.SetValue("GENERAL", "longitude", string.Format("{0,-9}", this.PlusMinLoncomboBox.SelectedItem.ToString() + this.longitudeMaskedTextBox.Text.ToString()).Replace(" ", "0").Replace(",", "."));
			inifile.SetValue("GENERAL", "latitude", string.Format("{0,-8}", this.PlusMinLatcomboBox.SelectedItem.ToString() + this.latitudeMaskedTextBox.Text.ToString()).Replace(" ", "0").Replace(",", "."));
			inifile.SetValue("GENERAL", "alwaysOnTop", this.alwaysOnTopcheckBox.Checked);
			inifile.SetValue("GENERAL", "invertRXTXColors", this.invertRXTXcheckBox.Checked);
			inifile.SetValue("GENERAL", "language", this.languageComboBox.SelectedItem.ToString());
			inifile.SetValue("AMBE", "AMBEtype", this.AMBEtypeComboBox.GetItemText(this.AMBEtypeComboBox.SelectedItem));
			inifile.SetValue("AMBE", "AMBEspeed", this.AMBEspeed.GetItemText(this.AMBEspeed.SelectedItem));
			inifile.SetValue("AMBE", "AMBEDMRid", this.AMBEDMRid.Text);
			inifile.SetValue("AMBE", "AMBENXDNid", this.AMBENXDNid.Text);
			inifile.SetValue("AMBE", "enableAMBEServer", this.enableAMBEServerCheckBox.Checked);
			if (!this.dvdonglecheckBox.Checked)
			{
				inifile.SetValue("AMBE", "enableAMBEServer", false);
			}
			inifile.SetValue("AMBE", "AMBEServerHost", this.AMBEServerHostTextBox.Text);
			inifile.SetValue("AMBE", "AMBEServerHostPort", this.AMBEServerHostPortTextBox.Text);
			inifile.SetValue("AMBE", "enableRoger", this.AMBEBeepcheckBox.Checked);
			if (!string.IsNullOrEmpty(this.comboBox3.GetItemText(this.comboBox3.SelectedItem)))
			{
				inifile.SetValue("AMBE", "comport", this.comboBox3.GetItemText(this.comboBox3.SelectedItem));
			}
			else
			{
				inifile.SetValue("AMBE", "comport", "COM2");
			}
			inifile.SetValue("AMBE", "PTTKeying", this.PTTKeyingcheckBox.Checked);
			if (!string.IsNullOrEmpty(this.COMportPTTcomboBox.GetItemText(this.COMportPTTcomboBox.SelectedItem)))
			{
				inifile.SetValue("AMBE", "PTTCOMPort", this.COMportPTTcomboBox.GetItemText(this.COMportPTTcomboBox.SelectedItem));
			}
			else
			{
				inifile.SetValue("AMBE", "PTTCOMPort", "COM1");
			}
			inifile.SetValue("AMBE", "enableRXIndicator", this.RXIndicatorEnablecheckBox.Checked);
			inifile.SetValue("AMBE", "RXIndicator", this._RXIndicator);
			inifile.SetValue("AMBE", "RXIndicatorHighLow", this._RXIndicatorHighLow);
			inifile.SetValue("AMBE", "PTTIndicator", this._PTTIndicator);
			inifile.SetValue("AMBE", "PTTIndicatorHighLow", this._PTTIndicatorHighLow);
			if (this.killTimerComboBox.SelectedItem != null)
			{
				inifile.SetValue("AMBE", "killswitch", this.killTimerComboBox.SelectedItem.ToString());
			}
			inifile.SetValue("AMBE", "DSTARSlowDataText", this.DSTARSlowDataTextBox.Text.PadRight(20).Substring(0, 20));
			inifile.SetValue("DMR", "dmrid", this.dmrid.Text);
			inifile.SetValue("DMR", "dmridsimplemode", this.DMRidSimpleTextBox.Text);
			inifile.SetValue("DMR", "DMRmaster", this.dmrmaster);
			inifile.SetValue("DMR", "DMRpassword", this.DMRpassword.Text);
			inifile.SetValue("DMR", "TGIFpassword", this.TGIFPassword.Text);
			inifile.SetValue("DMR", "DMRplusmaster", this.dmrplusmaster);
			inifile.SetValue("DMR", "DMRplusPort", this.dmrplusport);
			inifile.SetValue("DMR", "DMRplusPassword", this.dmrpluspassword);
			inifile.SetValue("DMR", "qrg", this.listBox1.SelectedItem.ToString());
			inifile.SetValue("DMR", "autostart", this.checkBoxDMREnabledAtBoot.Checked);
			inifile.SetValue("DMR", "noInbandData", this.noInbandDataCheckBox.Checked);
			inifile.SetValue("DMR", "DMRtypeSelection", this.DMRtypeSelectionComboBox.SelectedItem.ToString());
			inifile.SetValue("FUSION", "autostart", this.checkBoxFusionEnabledAtBoot.Checked);
			inifile.SetValue("FUSION", "QTHlocation", this.QTHlocatorTextBox.Text);
			string text2;
			try
			{
				text2 = this.YSFreflectorListcomboBox.SelectedItem.ToString();
			}
			catch
			{
				text2 = null;
			}
			if (!string.IsNullOrEmpty(text2))
			{
				inifile.SetValue("FUSION", "defaultYSFReflector", this.YSFreflectorListcomboBox.SelectedItem.ToString());
			}
			else
			{
				inifile.SetValue("FUSION", "defaultYSFReflector", "NL Central");
			}
			inifile.SetValue("FUSION", "defaultFCSReflector", this.FCSReflectorHostcomboBox.SelectedValue.ToString() + this.FCSReflectorModulecomboBox.SelectedItem.ToString());
			inifile.SetValue("FUSION", "autostartProtocol", this._FusionAutoStartprotocol);
			inifile.SetValue("DSTAR", "autostart", this.checkBoxDSTAREnabledAtBoot.Checked);
			inifile.SetValue("DSTAR", "DSTARmodule", this.DSTARmodule.GetItemText(this.DSTARmodule.SelectedItem));
			inifile.SetValue("DSTAR", "APRS", this.APRScheckBox.Checked);
			inifile.SetValue("DSTAR", "defaultReflector", this.defaultDSTARReflectorTextBox.Text);
			inifile.Flush();
			Setup.applyConfigVariables();
			if (information.APRS)
			{
				APRSClient.close();
				APRSClient.connect("euro.aprs2.net", 14580);
			}
			else
			{
				APRSClient.close();
			}
			bool usePTTkeying = information.usePTTkeying;
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x00002F42 File Offset: 0x00001142
		private void label4_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x00002F42 File Offset: 0x00001142
		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		// Token: 0x06000598 RID: 1432 RVA: 0x00030CB4 File Offset: 0x0002EEB4
		public static void applyConfigVariables()
		{
			INIFile inifile;
			if (utils.RunningPlatform() == utils.Platform.Windows)
			{
				inifile = new INIFile(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\" + information.configFile);
			}
			else
			{
				inifile = new INIFile(information.configFile);
			}
			information.myCall = inifile.GetValue("GENERAL", "myCall", "NOCALL");
			information.invertDTRRadio = inifile.GetValue("GENERAL", "invertDTR", false);
			information.myCall = information.myCall.PadRight(8).Substring(0, 8).Trim();
			information.myModeTimerRF = inifile.GetValue("GENERAL", "modeTimerRF", "15");
			information.myModeTimerNet = inifile.GetValue("GENERAL", "modeTimerNet", "5");
			if (inifile.GetValue("GENERAL", "dvdongle", false))
			{
				information.m_device = information.DEVICE.DV3000R;
			}
			else
			{
				information.m_device = information.DEVICE.DVMEGARADIO;
				information.m_deviceconnection = information.DEVICECONNECTION.SERIAL;
			}
			string value = inifile.GetValue("AMBE", "AMBEspeed", "230400");
			if (value.Equals("230400"))
			{
				information.m_ambespeed = information.AMBESPEED.SLOW;
			}
			else if (value.Equals("460800"))
			{
				information.m_ambespeed = information.AMBESPEED.HIGH;
			}
			else
			{
				information.m_ambespeed = information.AMBESPEED.SUPERHIGH;
			}
			information.myFREQ = inifile.GetValue("GENERAL", "frequency", "434300000");
			information.myCOMPort = inifile.GetValue("GENERAL", "comport", "COM1");
			information.saveQSOLog = inifile.GetValue("GENERAL", "saveQSOLog", false);
			information.startProtocol = inifile.GetValue("GENERAL", "StartProtocol", "None");
			information.RXTXColor = inifile.GetValue("GENERAL", "RXTXColors", false);
			information.DVMEGAPower = inifile.GetValue("GENERAL", "DVMEGAPower", "128");
			information.longitude = inifile.GetValue("GENERAL", "longitude", "+005.0739");
			information.latitude = inifile.GetValue("GENERAL", "latitude", "+50.0670");
			information.appinforeground = inifile.GetValue("GENERAL", "alwaysOnTop", false);
			information.invertScreenRXTX = inifile.GetValue("GENERAL", "invertRXTXColors", false);
			information.location = inifile.GetValue("GENERAL", "location", "Somewhere");
			information.description = inifile.GetValue("GENERAL", "description", "BlueDV");
			information.URL = inifile.GetValue("GENERAL", "URL", "www.pa7lim.nl");
			information.height = inifile.GetValue("GENERAL", "height", "1");
			information.m_ambetype = (information.AMBETYPE)Enum.Parse(typeof(information.AMBETYPE), inifile.GetValue("AMBE", "AMBEtype", "AMBE3000"));
			information.AMBEDMRid = inifile.GetValue("AMBE", "AMBEDMRid", "2043000");
			information.AMBENXDNid = inifile.GetValue("AMBE", "AMBENXDNid", "9999");
			information.enableAMBEServer = inifile.GetValue("AMBE", "enableAMBEServer", false);
			information.AMBEServerHost = inifile.GetValue("AMBE", "AMBEServerHost", "192.168.1.10");
			information.AMBEServerHostPort = inifile.GetValue("AMBE", "AMBEServerHostPort", "2460");
			information.AMBEBeep = inifile.GetValue("AMBE", "enableRoger", true);
			information.myCOMPortAMBE = inifile.GetValue("AMBE", "comport", "COM1");
			information.usePTTkeying = inifile.GetValue("AMBE", "PTTKeying", false);
			information.myCOMPortPTT = inifile.GetValue("AMBE", "PTTCOMPort", "COM1");
			information.enableRXIndicator = inifile.GetValue("AMBE", "enableRXIndicator", true);
			information.DSTARslowDataText = inifile.GetValue("AMBE", "DSTARSlowDataText", "BlueDV by PA7LIM");
			information.enableWEB = inifile.GetValue("AMBE", "enableWEB", false);
			information.webServerPort = inifile.GetValue("AMBE", "webServerPort", "8080");
			information.webVoicePort = inifile.GetValue("AMBE", "webVoicePort", "9999");
			string value2 = inifile.GetValue("AMBE", "killswitch", "5");
			if (value2.Equals("3") || value2.Equals("4") || value2.Equals("5") || value2.Equals("15"))
			{
				int num;
				if (int.TryParse(value2, out num))
				{
					information.killswitch = num * 60;
				}
				else
				{
					information.killswitch = 300;
				}
			}
			else
			{
				information.killswitch = 300;
			}
			string text = inifile.GetValue("AMBE", "RXIndicator", "CTS");
			if (!(text == "RTS"))
			{
				if (text == "DTR")
				{
					information.m_rxindicator = information.RXINDICATOR.DTR;
				}
			}
			else
			{
				information.m_rxindicator = information.RXINDICATOR.RTS;
			}
			text = inifile.GetValue("AMBE", "RXIndicatorHighLow", "HIGH");
			if (!(text == "HIGH"))
			{
				if (text == "LOW")
				{
					information.m_RXhighlow = information.HIGHLOW.LOW;
				}
			}
			else
			{
				information.m_RXhighlow = information.HIGHLOW.HIGH;
			}
			text = inifile.GetValue("AMBE", "PTTIndicator", "CTS");
			if (!(text == "CTS"))
			{
				if (text == "DSR")
				{
					information.m_pttindicator = information.PTTINDICATOR.DSR;
				}
			}
			else
			{
				information.m_pttindicator = information.PTTINDICATOR.CTS;
			}
			text = inifile.GetValue("AMBE", "PTTIndicatorHighLow", "HIGH");
			if (!(text == "HIGH"))
			{
				if (text == "LOW")
				{
					information.m_PTThighlow = information.HIGHLOW.LOW;
				}
			}
			else
			{
				information.m_PTThighlow = information.HIGHLOW.HIGH;
			}
			text = inifile.GetValue("GENERAL", "language", "English");
			if (!(text == "English"))
			{
				if (!(text == "Japanese"))
				{
					if (!(text == "Chinese"))
					{
						if (text == "Korean")
						{
							information.m_language = information.LANGUAGE.KOREAN;
						}
					}
					else
					{
						information.m_language = information.LANGUAGE.CHINEES;
					}
				}
				else
				{
					information.m_language = information.LANGUAGE.JAPANESE;
				}
			}
			else
			{
				information.m_language = information.LANGUAGE.ENGLISH;
			}
			information.myDMRID = inifile.GetValue("DMR", "dmrid", "2040000");
			information.myDMRIDsimple = inifile.GetValue("DMR", "dmridsimplemode", "2040000");
			information.myDMRHostname = inifile.GetValue("DMR", "DMRmaster", "213.222.29.197");
			information.myDMRPassword = inifile.GetValue("DMR", "DMRpassword", "passw0rd");
			information.myTGIFPassword = inifile.GetValue("DMR", "TGIFpassword", "passw0rd");
			information.myDMRPlusHostname = inifile.GetValue("DMR", "DMRplusmaster", "109.69.105.88");
			information.myDMRPlusPort = inifile.GetValue("DMR", "DMRplusPort", 55555);
			information.myDMRPlusPassword = inifile.GetValue("DMR", "DMRplusPassword", "passw0rd");
			information.DMRPlusOptions = inifile.GetValue("DMR", "DMRplusOptions", "");
			information.myDMRQRG = inifile.GetValue("DMR", "qrg", "0");
			information.autostartDMR = inifile.GetValue("DMR", "autostart", false);
			information.noInbandData = inifile.GetValue("DMR", "noInbandData", false);
			downloadHostsTable.parseTGIFHostFile();
			text = inifile.GetValue("DMR", "DMRtypeSelection", "BM");
			if (text != null)
			{
				switch (text.Length)
				{
				case 2:
					if (text == "BM")
					{
						information.m_dmrmodus = information.DMRMODUS.BM;
						goto IL_0860;
					}
					break;
				case 4:
					if (text == "TGIF")
					{
						information.m_dmrmodus = information.DMRMODUS.TGIF;
						goto IL_0860;
					}
					break;
				case 6:
					if (text == "XLXDMR")
					{
						information.m_dmrmodus = information.DMRMODUS.XLXDMR;
						goto IL_0860;
					}
					break;
				case 7:
				{
					char c = text[0];
					if (c != 'D')
					{
						if (c != 'F')
						{
							if (c == 'S')
							{
								if (text == "SYSTEMX")
								{
									information.m_dmrmodus = information.DMRMODUS.SYSTEMX;
									goto IL_0860;
								}
							}
						}
						else if (text == "FREEDMR")
						{
							information.m_dmrmodus = information.DMRMODUS.FREEDMR;
							goto IL_0860;
						}
					}
					else if (text == "DMRPLUS")
					{
						information.m_dmrmodus = information.DMRMODUS.DMRPLUS;
						goto IL_0860;
					}
					break;
				}
				case 10:
					if (text == "ADNSYSTEMS")
					{
						information.m_dmrmodus = information.DMRMODUS.ADNSYSTEMS;
						goto IL_0860;
					}
					break;
				}
			}
			information.m_dmrmodus = information.DMRMODUS.BM;
			IL_0860:
			information.myDSTARmodule = inifile.GetValue("DSTAR", "DSTARmodule", "D");
			information.APRS = inifile.GetValue("DSTAR", "APRS", false);
			information.defaultDSTARReflector = inifile.GetValue("DSTAR", "defaultReflector", "DCS600A");
			information.autostartDSTAR = inifile.GetValue("DSTAR", "autostart", false);
			information.autostartFUSION = inifile.GetValue("FUSION", "autostart", false);
			information.defaultYSFReflector = inifile.GetValue("FUSION", "defaultYSFReflector", "SC Scotland");
			information.defaultFCSReflector = inifile.GetValue("FUSION", "defaultFCSReflector", "FCS00401");
			information.myQTHlocation = inifile.GetValue("FUSION", "QTHlocation", "JO22MB");
			information.YSFOoptions = inifile.GetValue("FUSION", "Options", "");
			information.fusionAutoStartProtocol = inifile.GetValue("FUSION", "autostartProtocol", "YSF");
			information.myFusionQRG = inifile.GetValue("FUSION", "qrg", "0");
			if (information.enableAMBEServer)
			{
				information.m_deviceconnection = information.DEVICECONNECTION.AMBESERVER;
				return;
			}
			information.m_deviceconnection = information.DEVICECONNECTION.SERIAL;
		}

		// Token: 0x06000599 RID: 1433 RVA: 0x00031647 File Offset: 0x0002F847
		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.dmrmaster = JSONCallQuery.lookupHostname(this.comboBox2.SelectedIndex);
		}

		// Token: 0x0600059A RID: 1434 RVA: 0x00031660 File Offset: 0x0002F860
		private void freq_TextChanged(object sender, EventArgs e)
		{
			try
			{
				long num = long.Parse(this.freqDMR.Text);
				this.FrequencyWarninglabel.Text = num.ToString();
				if (num >= 145800000L && num <= 146000000L)
				{
					this.FrequencyWarninglabel.Text = "Fequency NOT allowed";
				}
				else if (num >= 435000000L && num <= 438000000L)
				{
					this.FrequencyWarninglabel.Text = "Fequency NOT allowed";
				}
				else
				{
					this.FrequencyWarninglabel.Text = "";
				}
			}
			catch (FormatException)
			{
			}
		}

		// Token: 0x0600059B RID: 1435 RVA: 0x0001D58F File Offset: 0x0001B78F
		private void keyPressedFREQ(object sender, KeyPressEventArgs e)
		{
			e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
		}

		// Token: 0x0600059C RID: 1436 RVA: 0x00002F42 File Offset: 0x00001142
		private void Setup_Load(object sender, EventArgs e)
		{
		}

		// Token: 0x0400038B RID: 907
		private bool _saveQSOLog;

		// Token: 0x0400038C RID: 908
		private bool _RXTXColors;

		// Token: 0x0400038D RID: 909
		private bool _enableAMBEServer;

		// Token: 0x0400038E RID: 910
		private bool _dvdongle;

		// Token: 0x0400038F RID: 911
		private bool _APRS;

		// Token: 0x04000390 RID: 912
		private bool _DMREnabledAtBoot;

		// Token: 0x04000391 RID: 913
		private bool _DSTAREnabledAtBoot;

		// Token: 0x04000392 RID: 914
		private bool _FusionEnabledAtBoot;

		// Token: 0x04000393 RID: 915
		private bool _noInbandData;

		// Token: 0x04000394 RID: 916
		private bool _AMBEBeep;

		// Token: 0x04000395 RID: 917
		private bool _PTTKeyingEnable;

		// Token: 0x04000396 RID: 918
		private bool _enableRXIndicator;

		// Token: 0x04000397 RID: 919
		private bool _alwaysOnTop;

		// Token: 0x04000398 RID: 920
		private bool _invertRXTX;

		// Token: 0x04000399 RID: 921
		private bool _invertDTRRadio;

		// Token: 0x0400039A RID: 922
		private string _mycall;

		// Token: 0x0400039B RID: 923
		private string _DMRid;

		// Token: 0x0400039C RID: 924
		private string _freqDMR;

		// Token: 0x0400039D RID: 925
		private string _dmrmaster;

		// Token: 0x0400039E RID: 926
		private string _dmrplusmaster;

		// Token: 0x0400039F RID: 927
		private string _DMRpassword;

		// Token: 0x040003A0 RID: 928
		private string _TGIFpassword;

		// Token: 0x040003A1 RID: 929
		private string _QTHlocator;

		// Token: 0x040003A2 RID: 930
		private string _modeTimerRF;

		// Token: 0x040003A3 RID: 931
		private string _modeTimerNet;

		// Token: 0x040003A4 RID: 932
		private string _DSTARmodule;

		// Token: 0x040003A5 RID: 933
		private string _qrg;

		// Token: 0x040003A6 RID: 934
		private string _comport;

		// Token: 0x040003A7 RID: 935
		private string _comportAMBE;

		// Token: 0x040003A8 RID: 936
		private string _startProtocol;

		// Token: 0x040003A9 RID: 937
		private string _defaultDSTARReflector;

		// Token: 0x040003AA RID: 938
		private string _defaultYSFReflector;

		// Token: 0x040003AB RID: 939
		private string _DVMEGAPower;

		// Token: 0x040003AC RID: 940
		private string _latitude;

		// Token: 0x040003AD RID: 941
		private string _longitude;

		// Token: 0x040003AE RID: 942
		private string _FusionAutoStartprotocol;

		// Token: 0x040003AF RID: 943
		private string _defaultFCSReflector;

		// Token: 0x040003B0 RID: 944
		private string _AMBEspeed;

		// Token: 0x040003B1 RID: 945
		private string _DMRtypeSelection;

		// Token: 0x040003B2 RID: 946
		private string _AMBEDMRid;

		// Token: 0x040003B3 RID: 947
		private string _AMBEServerHost;

		// Token: 0x040003B4 RID: 948
		private string _AMBEServerHostPort;

		// Token: 0x040003B5 RID: 949
		private string _COMportPTT;

		// Token: 0x040003B6 RID: 950
		private string _RXIndicator;

		// Token: 0x040003B7 RID: 951
		private string _RXIndicatorHighLow;

		// Token: 0x040003B8 RID: 952
		private string _PTTIndicator;

		// Token: 0x040003B9 RID: 953
		private string _PTTIndicatorHighLow;

		// Token: 0x040003BA RID: 954
		private string _DMRidSimple;

		// Token: 0x040003BB RID: 955
		private string _killswitch;

		// Token: 0x040003BC RID: 956
		private string _language;

		// Token: 0x040003BD RID: 957
		private string _dstarslowdata;

		// Token: 0x040003BE RID: 958
		private string _AMBEtype;

		// Token: 0x040003BF RID: 959
		private string _AMBENXDNid;

		// Token: 0x040003C0 RID: 960
		private string dmrmaster;

		// Token: 0x040003C1 RID: 961
		private string dmrplusmaster;

		// Token: 0x040003C2 RID: 962
		private string dmrpluspassword;

		// Token: 0x040003C3 RID: 963
		private string dmrplusport;
	}
}
