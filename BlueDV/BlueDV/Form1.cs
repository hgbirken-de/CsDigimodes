using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using BlueDV.Properties;
using JCS;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using ProgressBarEx;

namespace BlueDV
{
	// Token: 0x02000028 RID: 40
	public partial class Form1 : Form
	{
		// Token: 0x17000033 RID: 51
		// (get) Token: 0x0600025D RID: 605 RVA: 0x00016360 File Offset: 0x00014560
		// (set) Token: 0x0600025E RID: 606 RVA: 0x00016368 File Offset: 0x00014568
		public string[] SendResponse { get; private set; }

		// Token: 0x0600025F RID: 607 RVA: 0x00016374 File Offset: 0x00014574
		public void makeScreen()
		{
			switch (utils.RunningPlatform())
			{
			case utils.Platform.Windows:
				this.Text = "BlueDV for Windows";
				break;
			case utils.Platform.Linux:
				this.Text = "BlueDV for Linux";
				break;
			case utils.Platform.Mac:
				this.Text = "BlueDV for OSX";
				break;
			}
			this.dmrmaster.Text = "";
			this.comboBoxReflectorList.Visible = false;
			this.comboBoxReflectorModule.Visible = false;
			this.linkButton.Enabled = false;
			this.unlinkButton.Enabled = false;
			this.linkButton.Hide();
			this.unlinkButton.Hide();
			this.radioButtonREF.Hide();
			this.radioButtonDCS.Hide();
			this.radioButtonXRF.Hide();
			this.radioButtonJPN.Hide();
			this.radioButtonXLX.Hide();
			this.killTimerLabel.Text = "";
			this.timer1.Enabled = false;
			string text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			this.version.Text = "Version " + text;
			information.BlueDVVersion = text;
			CultureInfo installedUICulture = CultureInfo.InstalledUICulture;
			if (installedUICulture.Name.StartsWith("JAPAN"))
			{
				this.thanksTo.Visible = true;
			}
			if (installedUICulture.Name.StartsWith("KOREAN"))
			{
				this.thanksTo.Visible = false;
			}
			information.LANGUAGE language = information.m_language;
			if (language == information.LANGUAGE.JAPANESE)
			{
				this.thanksTo.Visible = true;
				return;
			}
			if (language != information.LANGUAGE.KOREAN)
			{
				this.thanksTo.Visible = false;
				return;
			}
			this.thanksTo.Visible = false;
		}

		// Token: 0x06000260 RID: 608 RVA: 0x00016510 File Offset: 0x00014710
		public Form1()
		{
			information.DVMEGAdetected = false;
			RuntimeHelpers.InitializeArray(new byte[87], fieldof(<PrivateImplementationDetails>.3B12F916FE00E0926B1345455F618258F66ED800709295B195343F6152568EA0).FieldHandle);
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			if (commandLineArgs.Length == 2)
			{
				information.configFile = commandLineArgs[1];
			}
			else
			{
				information.configFile = "BlueDVconfig.ini";
			}
			Setup.applyConfigVariables();
			switch (information.m_language)
			{
			case information.LANGUAGE.ENGLISH:
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
				break;
			case information.LANGUAGE.JAPANESE:
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("ja");
				break;
			case information.LANGUAGE.CHINEES:
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("zh-CHT");
				break;
			case information.LANGUAGE.KOREAN:
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("ko");
				break;
			}
			this.InitializeComponent();
			this.APRSchatTab.Enabled = information.APRS;
			JSONCallQuery.callLoader();
			JSONCallQuery.callLoaderNXDN();
			information.foundDVMEGA = false;
			information.m_dstarmodus = information.DSTARMODUS.REF;
			information.m_fusionmodus = information.FUSIONMODUS.YSF;
			base.Icon = Resources.web_hi_res_512;
			information.m_modus = information.MODUS.DMR;
			information.DMRconnected = false;
			if (Form1.IsNetworkAvailable())
			{
				this.checkVersion();
			}
			SystemEvents.PowerModeChanged += this.onPowerChange;
			this.makeScreen();
			base.TopMost = information.appinforeground;
			if (information.m_device == information.DEVICE.DV3000R)
			{
				this.killTimerLabel.Visible = true;
				this.aMBEToolStripMenuItem.Visible = true;
			}
			else
			{
				this.killTimerLabel.Visible = false;
				this.aMBEToolStripMenuItem.Visible = false;
			}
			TGLookup.loadAll2();
			DVMEGASerial.StatusTextChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetTextSerial(DVMEGASerial.StatusText);
			};
			DVMEGASerial.StatusErrorTextChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetTextErrorSerial(DVMEGASerial.StatusErrorText);
			};
			modeTimer.StatusModeTextChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetTextModeSerial(modeTimer.StatusModeText);
			};
			fusion_extract.StatusRadioTextChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetTextRadioText(fusion_extract.StatusRadioText);
			};
			ShowCallTimer.StatusModeModeTextChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetTextModeMode(ShowCallTimer.StatusModeMode, ShowCallTimer.StatusModeText);
			};
			DMRconnection.StatusTextChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetDMRText(DMRconnection.StatusText);
			};
			DMRPlus.StatusTextChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetDMRText(DMRPlus.StatusText);
			};
			YSFFusion.StatusTextChangedFUSION += delegate(object sender1, EventArgs e1)
			{
				this.SetFusionText(YSFFusion.StatusTextFUSION);
			};
			FCSFusion.StatusTextChangedFUSION += delegate(object sender1, EventArgs e1)
			{
				this.SetFusionText(FCSFusion.StatusTextFUSION);
			};
			NXDNConnect.StatusTextChangedNXDN += delegate(object sender1, EventArgs e1)
			{
				this.SetNXDNText(NXDNConnect.StatusTextNXDN);
			};
			DPLUSconnection.StatusTextChangedDPLUS += delegate(object sender1, EventArgs e1)
			{
				this.SetDSTARText(DPLUSconnection.StatusTextDPLUS);
			};
			DCSconnection.StatusTextChangedDCS += delegate(object sender1, EventArgs e1)
			{
				this.SetDSTARText(DCSconnection.StatusTextDCS);
			};
			SlowData.StatusTextChangedSlowData += delegate(object sender1, EventArgs e1)
			{
				this.SetDSTARTextSlowData(SlowData.StatusTextSlowData);
			};
			AMBESERVER.StatusTextChangedAMBEServer += delegate(object sender1, EventArgs e1)
			{
				this.SetAMBESERVERText(AMBESERVER.StatusTextAMBEServer);
			};
			AMBE_VOX.StatusBoolChangedAMBE_VOX += delegate(object sender1, EventArgs e1)
			{
				this.SetAMBE_VOXText(AMBE_VOX.StatusBoolAMBE_VOX);
			};
			JSONCallQuery.StatusHisDMRTextChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetHisText(JSONCallQuery.StatusText2, JSONCallQuery.StatusText3, JSONCallQuery.dmrid, JSONCallQuery.dmrdest, JSONCallQuery.city, JSONCallQuery.country);
			};
			TimerRXTX.StatusRXTXChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetRXTXText(TimerRXTX.StatusRXTX);
			};
			berCounter.StatusBERChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetBERText(berCounter.StatusBER);
			};
			berCounter.StatusSRCDSTChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetSRCDSTText(berCounter.srcID, berCounter.dstID, berCounter.TextID);
			};
			APRSClient.StatusPictureChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetPictureText(APRSClient.StatusPicture);
			};
			APRSClient.StatusTextChangedAPRS += delegate(object sender1, EventArgs e1)
			{
				this.SetAPRSText(APRSClient.StatusTextAPRS);
			};
			soundcard.StatusVUChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetVUMeter(soundcard.StatusVU);
			};
			killTimer.StatusPTTChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetPTTswitch(killTimer.StatusBoolean);
			};
			killTimer.StatusPTTTimerChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetPTTTimer(killTimer.StatusTimer, killTimer.totalSeconds);
			};
			AMBEPTTControl.StatusBoolChangedAMBE_PTT += delegate(object sender1, EventArgs e1)
			{
				this.SetAMBE_VOXText(AMBEPTTControl.StatusBoolAMBE_PTT);
			};
			AMBEPTTControl.StatusTextChangedControl += delegate(object sender1, EventArgs e1)
			{
				this.SetTextErrorSerial(AMBEPTTControl.StatusTextControl);
			};
			APRSClient.StatusChatChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetChatText(APRSClient.StatusChat);
			};
			TimerRXTX.startTimer();
			WomenQueue.start();
			FusionQueue.start();
			modeTimer.startTimer();
			ShowCallTimer.startTimer();
			if (information.APRS)
			{
				APRSClient.close();
				APRSClient.connect("euro.aprs2.net", 14580);
			}
			else
			{
				APRSClient.close();
			}
			if (information.enableWEB && information.m_device == information.DEVICE.DV3000R)
			{
				if (Form1.ws != null)
				{
					Form1.ws.Stop();
					soundcard.stopWebSocket();
				}
				Form1.ws = new WebServer("http://+:" + information.webServerPort + "/");
				Form1.ws.Run();
				soundcard.startWebSocket();
			}
			if (information.autostartDMR || information.autostartDSTAR || information.autostartFUSION)
			{
				this.toggleSwitchSerialConnect.Checked = true;
			}
			this.DMRManualDialComboBox1.Items.Clear();
			foreach (object obj in Settings.Default.savedDMRManualDialCollection)
			{
				this.DMRManualDialComboBox1.Items.Add(obj);
			}
			foreach (object obj2 in Settings.Default.savedAPRSChatCallsCollection)
			{
				this.APRSChatCallComboBox.Items.Add(obj2);
			}
			this.TGlistView.Items.Clear();
			if (TGLookup.schema != null)
			{
				foreach (KeyValuePair<string, string> keyValuePair in TGLookup.schema)
				{
					ListViewItem listViewItem = new ListViewItem(new string[] { keyValuePair.Key, keyValuePair.Value });
					this.TGlistView.Items.Add(listViewItem);
				}
			}
			this.DSTARouttrackBar.Value = Settings.Default.savedDSTARoutGain;
			information.DSTARoutGain = this.DSTARouttrackBar.Value;
			this.DSTARoutlabel.Text = Settings.Default.savedDSTARoutGain.ToString();
			this.DSTARintrackBar.Value = Settings.Default.savedDSTARinGain;
			information.DSTARinGain = this.DSTARintrackBar.Value;
			this.DSTARinlabel.Text = Settings.Default.savedDSTARinGain.ToString();
			this.DMRouttrackBar.Value = Settings.Default.savedDMRoutGain;
			information.DMRoutGain = this.DMRouttrackBar.Value;
			this.DMRoutlabel.Text = Settings.Default.savedDMRoutGain.ToString();
			this.DMRintrackBar.Value = Settings.Default.savedDMRinGain;
			information.DMRinGain = this.DMRintrackBar.Value;
			this.DMRinlabel.Text = Settings.Default.savedDMRinGain.ToString();
			this.FUSIONouttrackBar.Value = Settings.Default.savedFUSIONoutGain;
			information.FUSIONoutGain = this.FUSIONouttrackBar.Value;
			this.FUSIONoutlabel.Text = Settings.Default.savedFUSIONoutGain.ToString();
			this.FUSIONintrackBar.Value = Settings.Default.savedFUSIONinGain;
			information.FUSIONinGain = this.FUSIONintrackBar.Value;
			this.FUSIONinlabel.Text = Settings.Default.savedFUSIONinGain.ToString();
			this.NXDNouttrackBar.Value = Settings.Default.savedNXDNoutGain;
			information.NXDNoutGain = this.NXDNouttrackBar.Value;
			this.NXDNoutlabel.Text = Settings.Default.savedNXDNoutGain.ToString();
			this.NXDNintrackBar.Value = Settings.Default.savedNXDNinGain;
			information.NXDNinGain = this.NXDNintrackBar.Value;
			this.NXDNinlabel.Text = Settings.Default.savedNXDNinGain.ToString();
			int num = this.DGIDComboBox.FindString(Settings.Default.savedDGID.ToString());
			this.DGIDComboBox.SelectedIndex = num;
			fusion_extract.changeDGID(num);
			this.hangTimeTrackBar.Value = Settings.Default.savedVOXHangTime;
			information.VOXHangTime = (long)this.hangTimeTrackBar.Value;
			this.hangLabel.Text = (Settings.Default.savedVOXHangTime / 1000).ToString();
			this.VOXLevelTrackBar.Value = Settings.Default.savedVOXDetectionLevel;
			information.VOXDetectionLevel = Settings.Default.savedVOXDetectionLevel;
			AMBE_VOX.setDetectionLevel(Settings.Default.savedVOXDetectionLevel);
			this.VOXlabel.Text = (Settings.Default.savedVOXDetectionLevel / 1000).ToString();
			this.simpleModeCheckBox.Checked = Settings.Default.savedDMRsimpleMode;
			information.myAMBEDstDMRID = this.DMRManualDialComboBox1.Text;
			information.myAMBEDstDMRIDinput = this.DMRManualDialComboBox1.Text;
			if (this.toggleSwitchGroupPrivate.Checked)
			{
				information.m_groupprivate = information.GROUPPRIVATE.PRIVATE;
			}
			else
			{
				information.m_groupprivate = information.GROUPPRIVATE.GROUP;
			}
			if (utils.RunningPlatform() != utils.Platform.Linux)
			{
				soundcard.selectRecordingDevice(soundcard.selectedRecordingDevice());
				soundcard.selectPlayDevice(soundcard.selectedPlayDevice());
			}
		}

		// Token: 0x06000261 RID: 609 RVA: 0x00016DB4 File Offset: 0x00014FB4
		private void onPowerChange(object sender, PowerModeChangedEventArgs e)
		{
			switch (e.Mode)
			{
			case PowerModes.Resume:
			case PowerModes.StatusChange:
				break;
			case PowerModes.Suspend:
				this.disconnect();
				break;
			default:
				return;
			}
		}

		// Token: 0x06000262 RID: 610 RVA: 0x00016DE3 File Offset: 0x00014FE3
		public void _priceChanger_PriceUpdate(string value)
		{
			this.hisDMRid.Text = value;
		}

		// Token: 0x06000263 RID: 611 RVA: 0x00016DF4 File Offset: 0x00014FF4
		private void SetAMBESERVERText(string text)
		{
			if (this.dynamicStatus.InvokeRequired)
			{
				Form1.SetAMBESERVERTextCallback setAMBESERVERTextCallback = new Form1.SetAMBESERVERTextCallback(this.SetAMBESERVERText);
				base.Invoke(setAMBESERVERTextCallback, new object[] { text });
				return;
			}
			this.dynamicStatus.Text = text;
		}

		// Token: 0x06000264 RID: 612 RVA: 0x00016E3C File Offset: 0x0001503C
		private void SetAMBE_VOXText(bool vox)
		{
			if (this.onAIRswitch.InvokeRequired)
			{
				Form1.SetAMBE_VOXTextCallback setAMBE_VOXTextCallback = new Form1.SetAMBE_VOXTextCallback(this.SetAMBE_VOXText);
				base.Invoke(setAMBE_VOXTextCallback, new object[] { vox });
				return;
			}
			this.onAIRswitch.Checked = vox;
		}

		// Token: 0x06000265 RID: 613 RVA: 0x00016E88 File Offset: 0x00015088
		private void SetDMRText(string text)
		{
			if (this.dynamicDMRstatus.InvokeRequired)
			{
				Form1.SetDMRTextCallback setDMRTextCallback = new Form1.SetDMRTextCallback(this.SetDMRText);
				base.Invoke(setDMRTextCallback, new object[] { text });
				return;
			}
			this.dynamicDMRstatus.Text = text;
		}

		// Token: 0x06000266 RID: 614 RVA: 0x00016ECE File Offset: 0x000150CE
		private string setCallInSmallBoxDMR(string searchName)
		{
			return JSONCallQuery.callLookupcsvNEW(searchName, information.MODUS.DMR)[0];
		}

		// Token: 0x06000267 RID: 615 RVA: 0x00016ED9 File Offset: 0x000150D9
		private string setCallInSmallBoxNXDN(string searchName)
		{
			return JSONCallQuery.callLookupcsvNEW(searchName, information.MODUS.NXDN)[0];
		}

		// Token: 0x06000268 RID: 616 RVA: 0x00016EE4 File Offset: 0x000150E4
		private void SetTextModeMode(information.MODUS mode, string text)
		{
			switch (mode)
			{
			case information.MODUS.DMR:
			{
				if (this.dynamicHisDMRCallBox.InvokeRequired)
				{
					Form1.SetModeModeTextCallback setModeModeTextCallback = new Form1.SetModeModeTextCallback(this.SetTextModeMode);
					base.Invoke(setModeModeTextCallback, new object[] { mode, text });
					return;
				}
				string name2 = null;
				this.JSONCallQueryThread = new Thread(delegate
				{
					name2 = this.setCallInSmallBoxDMR(text);
				});
				this.JSONCallQueryThread.Start();
				this.JSONCallQueryThread.Join();
				if (name2 == null)
				{
					name2 = text;
				}
				this.dynamicHisDMRCallBox.Text = name2;
				if (information.invertScreenRXTX)
				{
					if (text == " ")
					{
						this.DMRPanel.BackgroundImage = Resources.bluedvwinbluebgrnd;
						return;
					}
					this.DMRPanel.BackgroundImage = Resources.bluedvwingreenbgrnd;
					return;
				}
				else
				{
					if (text == " ")
					{
						this.DMRPanel.BackgroundImage = Resources.bluedvwinbluebgrnd;
						return;
					}
					this.DMRPanel.BackgroundImage = Resources.bluedvwinredbgrnd;
					return;
				}
				break;
			}
			case information.MODUS.DSTAR:
				if (this.dynamicHisDSTARCallBox.InvokeRequired)
				{
					Form1.SetModeModeTextCallback setModeModeTextCallback2 = new Form1.SetModeModeTextCallback(this.SetTextModeMode);
					base.Invoke(setModeModeTextCallback2, new object[] { mode, text });
					return;
				}
				this.dynamicHisDSTARCallBox.Text = text;
				if (information.invertScreenRXTX)
				{
					if (text == " ")
					{
						this.DSTARPanel.BackgroundImage = Resources.bluedvwinbluebgrnd;
						return;
					}
					this.DSTARPanel.BackgroundImage = Resources.bluedvwingreenbgrnd;
					return;
				}
				else
				{
					if (text == " ")
					{
						this.DSTARPanel.BackgroundImage = Resources.bluedvwinbluebgrnd;
						return;
					}
					this.DSTARPanel.BackgroundImage = Resources.bluedvwinredbgrnd;
					return;
				}
				break;
			case information.MODUS.FUSION:
				if (this.dynamicHisFUSIONCallBox.InvokeRequired)
				{
					Form1.SetModeModeTextCallback setModeModeTextCallback3 = new Form1.SetModeModeTextCallback(this.SetTextModeMode);
					base.Invoke(setModeModeTextCallback3, new object[] { mode, text });
					return;
				}
				this.dynamicHisFUSIONCallBox.Text = text;
				if (information.invertScreenRXTX)
				{
					if (text == " ")
					{
						this.FusionPanel.BackgroundImage = Resources.bluedvwinbluebgrnd;
						return;
					}
					this.FusionPanel.BackgroundImage = Resources.bluedvwingreenbgrnd;
					return;
				}
				else
				{
					if (text == " ")
					{
						this.FusionPanel.BackgroundImage = Resources.bluedvwinbluebgrnd;
						return;
					}
					this.FusionPanel.BackgroundImage = Resources.bluedvwinredbgrnd;
					return;
				}
				break;
			case information.MODUS.NXDN:
			{
				if (this.dynamicHisNXDNCallBox.InvokeRequired)
				{
					Form1.SetModeModeTextCallback setModeModeTextCallback4 = new Form1.SetModeModeTextCallback(this.SetTextModeMode);
					base.Invoke(setModeModeTextCallback4, new object[] { mode, text });
					return;
				}
				string name = null;
				this.JSONCallQueryThread = new Thread(delegate
				{
					name = this.setCallInSmallBoxNXDN(text);
				});
				this.JSONCallQueryThread.Start();
				this.JSONCallQueryThread.Join();
				if (name == null)
				{
					name = text;
				}
				this.dynamicHisNXDNCallBox.Text = name;
				if (information.invertScreenRXTX)
				{
					if (text == " ")
					{
						this.NXDNPanel.BackgroundImage = Resources.bluedvwinbluebgrnd;
						return;
					}
					this.NXDNPanel.BackgroundImage = Resources.bluedvwingreenbgrnd;
					return;
				}
				else
				{
					if (text == " ")
					{
						this.NXDNPanel.BackgroundImage = Resources.bluedvwinbluebgrnd;
						return;
					}
					this.NXDNPanel.BackgroundImage = Resources.bluedvwinredbgrnd;
					return;
				}
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x06000269 RID: 617 RVA: 0x000172E0 File Offset: 0x000154E0
		private void SetDSTARText(string text)
		{
			if (this.dynamicDSTARstatus.InvokeRequired)
			{
				Form1.SetDSTARTextCallback setDSTARTextCallback = new Form1.SetDSTARTextCallback(this.SetDSTARText);
				base.Invoke(setDSTARTextCallback, new object[] { text });
				return;
			}
			this.dynamicDSTARstatus.Text = text;
		}

		// Token: 0x0600026A RID: 618 RVA: 0x00017328 File Offset: 0x00015528
		private void SetDSTARTextSlowData(string text)
		{
			if (this.dynamicDSTARstatus.InvokeRequired)
			{
				Form1.SetTextCallBackSlowData setTextCallBackSlowData = new Form1.SetTextCallBackSlowData(this.SetDSTARTextSlowData);
				base.Invoke(setTextCallBackSlowData, new object[] { text });
				return;
			}
			this.hisDMRid.Text = text;
		}

		// Token: 0x0600026B RID: 619 RVA: 0x00017370 File Offset: 0x00015570
		private void SetFusionText(string text)
		{
			if (this.dynamicFusionstatus.InvokeRequired)
			{
				Form1.SetFusionTextCallback setFusionTextCallback = new Form1.SetFusionTextCallback(this.SetFusionText);
				base.Invoke(setFusionTextCallback, new object[] { text });
				return;
			}
			this.dynamicFusionstatus.Text = text;
		}

		// Token: 0x0600026C RID: 620 RVA: 0x000173B8 File Offset: 0x000155B8
		private void SetNXDNText(string text)
		{
			if (this.dynamicNXDNstatus.InvokeRequired)
			{
				Form1.SetNXDNTextCallback setNXDNTextCallback = new Form1.SetNXDNTextCallback(this.SetNXDNText);
				base.Invoke(setNXDNTextCallback, new object[] { text });
				return;
			}
			this.dynamicNXDNstatus.Text = text;
		}

		// Token: 0x0600026D RID: 621 RVA: 0x00017400 File Offset: 0x00015600
		private void SetTextErrorSerial(string text)
		{
			if (this.dynamicStatus.InvokeRequired)
			{
				Form1.SetTextCallback setTextCallback = new Form1.SetTextCallback(this.SetTextErrorSerial);
				base.Invoke(setTextCallback, new object[] { text });
				return;
			}
			this.dynamicStatus.Text = text;
		}

		// Token: 0x0600026E RID: 622 RVA: 0x00017448 File Offset: 0x00015648
		private void SetTextRadioText(string text)
		{
			if (this.hisDSTARidsmall.InvokeRequired)
			{
				Form1.SetTextRadioCallback setTextRadioCallback = new Form1.SetTextRadioCallback(this.SetTextRadioText);
				base.Invoke(setTextRadioCallback, new object[] { text });
				return;
			}
			this.hisDSTARidsmall.Font = new Font("ErbosDraco Nova Open NBP", 12f, FontStyle.Regular);
			this.hisDSTARidsmall.Text = text;
		}

		// Token: 0x0600026F RID: 623 RVA: 0x000174AC File Offset: 0x000156AC
		private void SetTextModeSerial(string text)
		{
			if (this.dynamic_mode_text.InvokeRequired)
			{
				Form1.SetModeTextCallback setModeTextCallback = new Form1.SetModeTextCallback(this.SetTextModeSerial);
				base.Invoke(setModeTextCallback, new object[] { text });
				return;
			}
			switch (information.stream_modus)
			{
			case information.MODUS.IDLE:
				this.dynamic_mode_text.ForeColor = Color.Green;
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					this.dynamic_mode_text.Text = "IDLE";
					return;
				case information.LANGUAGE.JAPANESE:
					this.dynamic_mode_text.Text = "待機中";
					return;
				case information.LANGUAGE.CHINEES:
					this.dynamic_mode_text.Text = "IDLE";
					return;
				case information.LANGUAGE.KOREAN:
					this.dynamic_mode_text.Text = "모드대기";
					return;
				default:
					return;
				}
				break;
			case information.MODUS.DMR:
				this.dynamic_mode_text.ForeColor = Color.Red;
				this.dynamic_mode_text.Text = "DMR";
				return;
			case information.MODUS.DSTAR:
				this.dynamic_mode_text.ForeColor = Color.Red;
				this.dynamic_mode_text.Text = "DSTAR";
				return;
			case information.MODUS.FUSION:
				this.dynamic_mode_text.ForeColor = Color.Red;
				this.dynamic_mode_text.Text = "C4FM";
				return;
			case information.MODUS.NXDN:
				this.dynamic_mode_text.ForeColor = Color.Red;
				this.dynamic_mode_text.Text = "NXDN";
				return;
			default:
				return;
			}
		}

		// Token: 0x06000270 RID: 624 RVA: 0x00017600 File Offset: 0x00015800
		private void SetTextSerial(string text)
		{
			if (this.Firmware.InvokeRequired)
			{
				Form1.SetTextSerialCallback setTextSerialCallback = new Form1.SetTextSerialCallback(this.SetTextSerial);
				base.Invoke(setTextSerialCallback, new object[] { text });
				return;
			}
			this.Firmware.Text = text;
		}

		// Token: 0x06000271 RID: 625 RVA: 0x00017648 File Offset: 0x00015848
		private void SetPTTswitch(bool swit)
		{
			if (this.onAIRswitch.InvokeRequired)
			{
				Form1.SetPTTCallback setPTTCallback = new Form1.SetPTTCallback(this.SetPTTswitch);
				base.Invoke(setPTTCallback, new object[] { swit });
				return;
			}
			this.onAIRswitch.Checked = false;
		}

		// Token: 0x06000272 RID: 626 RVA: 0x00017694 File Offset: 0x00015894
		private void SetPTTTimer(string timert, long timeLeft)
		{
			if (this.killTimerLabel.InvokeRequired)
			{
				Form1.SetPTTTimerCallback setPTTTimerCallback = new Form1.SetPTTTimerCallback(this.SetPTTTimer);
				base.Invoke(setPTTTimerCallback, new object[] { timert, timeLeft });
				return;
			}
			if ((long)information.killswitch - timeLeft < 10L)
			{
				this.killTimerLabel.ForeColor = Color.Red;
				soundcard.beep(750.0);
			}
			else
			{
				this.killTimerLabel.ForeColor = Color.Black;
			}
			this.killTimerLabel.Text = timert;
		}

		// Token: 0x06000273 RID: 627 RVA: 0x00017720 File Offset: 0x00015920
		private void SetRXTXText(information.QSOSTATUS text1)
		{
			if (this.dynamicRXTX.InvokeRequired && this.Panel1.InvokeRequired && this.dynamicRXTX.InvokeRequired)
			{
				Form1.SetTextCallbackRXTX setTextCallbackRXTX = new Form1.SetTextCallbackRXTX(this.SetRXTXText);
				base.Invoke(setTextCallbackRXTX, new object[] { text1 });
				return;
			}
			int num = 100;
			if (information.m_device == information.DEVICE.DV3000R && information.m_modus == information.MODUS.FUSION)
			{
				this.showFusionInfoOnscreen();
			}
			switch (text1)
			{
			case information.QSOSTATUS.RX:
				if (information.invertScreenRXTX)
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						this.dynamicRXTX.Text = "TX";
						break;
					case information.LANGUAGE.JAPANESE:
						this.dynamicRXTX.Text = "送信";
						break;
					case information.LANGUAGE.CHINEES:
						this.dynamicRXTX.Text = "TX";
						break;
					case information.LANGUAGE.KOREAN:
						this.dynamicRXTX.Text = "TX";
						break;
					}
				}
				else
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						this.dynamicRXTX.Text = "RX";
						break;
					case information.LANGUAGE.JAPANESE:
						this.dynamicRXTX.Text = "受信";
						break;
					case information.LANGUAGE.CHINEES:
						this.dynamicRXTX.Text = "RX";
						break;
					case information.LANGUAGE.KOREAN:
						this.dynamicRXTX.Text = "RX";
						break;
					}
				}
				if (information.invertScreenRXTX)
				{
					this.dynamicRXTX.ForeColor = Color.Red;
					this.RXprogressBarEx.Value = 0;
					this.TXprogressBarEx.Value = 100;
				}
				else
				{
					this.dynamicRXTX.ForeColor = Color.Green;
					this.RXprogressBarEx.Value = 100;
					this.TXprogressBarEx.Value = 0;
				}
				if (information.invertScreenRXTX)
				{
					if (information.RXTXColor)
					{
						this.Panel1.BackgroundImage = Resources.bluedvwinredbgrnd;
					}
				}
				else if (information.RXTXColor)
				{
					this.Panel1.BackgroundImage = Resources.bluedvwingreenbgrnd;
				}
				if (information.stream_modus.Equals(information.MODUS.DSTAR))
				{
					this.hisCall.Text = information.hisCall;
					this.hisDSTARidsmall.Text = information.hisCallsmall;
					this.JSONCallQueryThread = new Thread(delegate
					{
						JSONCallQuery.callLookupcsv(information.hisCall.Trim(), information.MODUS.DSTAR);
					});
					this.JSONCallQueryThread.Start();
				}
				if (information.stream_modus.Equals(information.MODUS.DMR))
				{
					this.hisDMRid.Text = information.myResolvedSrcDMRID;
					this.dynamichisDMRdest.Text = information.hisDMRdest;
					if (information.m_device == information.DEVICE.DV3000R)
					{
						this.JSONCallQueryThread = new Thread(delegate
						{
							JSONCallQuery.callLookupcsv(information.myCall.Trim(), information.MODUS.DMR);
						});
						this.JSONCallQueryThread.Start();
					}
				}
				if (information.stream_modus.Equals(information.MODUS.FUSION))
				{
					this.JSONCallQueryThread = new Thread(delegate
					{
						JSONCallQuery.callLookupcsv(information.myCall.Trim(), information.MODUS.FUSION);
					});
					this.JSONCallQueryThread.Start();
				}
				if (information.stream_modus.Equals(information.MODUS.NXDN))
				{
					this.JSONCallQueryThread = new Thread(delegate
					{
						JSONCallQuery.callLookupcsv(information.myCall.Trim(), information.MODUS.FUSION);
					});
					this.JSONCallQueryThread.Start();
					return;
				}
				break;
			case information.QSOSTATUS.TX:
				if (information.invertScreenRXTX)
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						this.dynamicRXTX.Text = "RX";
						break;
					case information.LANGUAGE.JAPANESE:
						this.dynamicRXTX.Text = "受信";
						break;
					case information.LANGUAGE.CHINEES:
						this.dynamicRXTX.Text = "RX";
						break;
					case information.LANGUAGE.KOREAN:
						this.dynamicRXTX.Text = "RX";
						break;
					}
				}
				else
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						this.dynamicRXTX.Text = "TX";
						break;
					case information.LANGUAGE.JAPANESE:
						this.dynamicRXTX.Text = "送信";
						break;
					case information.LANGUAGE.CHINEES:
						this.dynamicRXTX.Text = "TX";
						break;
					case information.LANGUAGE.KOREAN:
						this.dynamicRXTX.Text = "TX";
						break;
					}
				}
				AMBEPTTControl.setRXstatus(true);
				try
				{
					num = int.Parse(information.DVMEGAPower) & 127;
				}
				catch (FormatException)
				{
				}
				if (information.invertScreenRXTX)
				{
					this.dynamicRXTX.ForeColor = Color.Green;
				}
				else
				{
					this.dynamicRXTX.ForeColor = Color.Red;
				}
				if (information.invertScreenRXTX)
				{
					this.TXprogressBarEx.Value = 0;
					this.RXprogressBarEx.Value = 100;
				}
				else
				{
					this.TXprogressBarEx.Value = num;
					this.RXprogressBarEx.Value = 0;
				}
				if (information.invertScreenRXTX)
				{
					if (information.RXTXColor)
					{
						this.Panel1.BackgroundImage = Resources.bluedvwingreenbgrnd;
					}
				}
				else if (information.RXTXColor)
				{
					this.Panel1.BackgroundImage = Resources.bluedvwinredbgrnd;
				}
				if (information.stream_modus.Equals(information.MODUS.DMR))
				{
					this.JSONCallQueryThread = new Thread(delegate
					{
						JSONCallQuery.callLookupcsv(information.hisDMRID, information.MODUS.DMR);
					});
					this.JSONCallQueryThread.Start();
				}
				if (information.stream_modus.Equals(information.MODUS.FUSION))
				{
					this.hisCall.Text = information.hisCall;
					this.JSONCallQueryThread = new Thread(delegate
					{
						JSONCallQuery.callLookupcsv(information.hisCall, information.MODUS.FUSION);
					});
					this.JSONCallQueryThread.Start();
				}
				if (information.stream_modus.Equals(information.MODUS.DSTAR))
				{
					this.hisCall.Text = information.hisCall;
					this.hisDSTARidsmall.Text = information.hisCallsmall;
					this.JSONCallQueryThread = new Thread(delegate
					{
						JSONCallQuery.callLookupcsv(information.hisCall, information.MODUS.DSTAR);
					});
					this.JSONCallQueryThread.Start();
				}
				if (information.stream_modus.Equals(information.MODUS.NXDN))
				{
					this.JSONCallQueryThread = new Thread(delegate
					{
						JSONCallQuery.callLookupcsv(information.hisNXDNID, information.MODUS.NXDN);
					});
					this.JSONCallQueryThread.Start();
					return;
				}
				break;
			case information.QSOSTATUS.LISTENING:
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					this.dynamicRXTX.Text = "LISTENING";
					this.BER.Text = "BER";
					break;
				case information.LANGUAGE.JAPANESE:
					this.dynamicRXTX.Text = "待機中";
					this.BER.Text = "符号誤り";
					break;
				case information.LANGUAGE.CHINEES:
					this.dynamicRXTX.Text = "LISTENING";
					this.BER.Text = "BER";
					break;
				case information.LANGUAGE.KOREAN:
					this.dynamicRXTX.Text = "대기상태";
					this.BER.Text = "BER";
					break;
				}
				if (information.RXTXColor)
				{
					this.Panel1.BackgroundImage = Resources.bluedvwinbluebgrnd;
				}
				this.dynamicRXTX.ForeColor = Color.Black;
				this.fusionModeLabel.Text = "";
				this.RXprogressBarEx.Value = 0;
				this.TXprogressBarEx.Value = 0;
				this.VUMeter.Value = 0f;
				this.hisName.Text = " ";
				this.hisCall.Text = " ";
				this.hisCity.Text = " ";
				this.hisCountry.Text = " ";
				this.hisDMRid.Text = " ";
				this.hisDSTARidsmall.Text = " ";
				this.dynamichisDMRdest.Text = " ";
				information.myResolvedSrcDMRID = "";
				information.hisCallsmall = " ";
				DVMEGASerial.createNewDMRSessionID();
				AMBEPTTControl.setRXstatus(false);
				break;
			default:
				return;
			}
		}

		// Token: 0x06000274 RID: 628 RVA: 0x00017F38 File Offset: 0x00016138
		private void showFusionInfoOnscreen()
		{
			string text = "";
			switch (fusion_extract.getDT())
			{
			case 0:
				text = "DN1";
				break;
			case 1:
				text = "FD";
				break;
			case 2:
				text = "DN";
				break;
			case 3:
				text = "WV";
				break;
			}
			this.fusionModeLabel.Text = text + " DGID:" + fusion_extract.getDGId().ToString();
		}

		// Token: 0x06000275 RID: 629 RVA: 0x00017FAC File Offset: 0x000161AC
		private void SetBERText(string text)
		{
			if (this.BER.InvokeRequired)
			{
				Form1.SetTextCallback setTextCallback = new Form1.SetTextCallback(this.SetBERText);
				base.Invoke(setTextCallback, new object[] { text });
				return;
			}
			this.BER.Text = text;
			if (information.invertScreenRXTX)
			{
				this.TXprogressBarEx.Value = 100 - information.myBER;
				return;
			}
			this.RXprogressBarEx.Value = 100 - information.myBER;
		}

		// Token: 0x06000276 RID: 630 RVA: 0x00018020 File Offset: 0x00016220
		private void SetAPRSText(string text)
		{
			if (this.dynamicStatus.InvokeRequired)
			{
				Form1.SetTextCallBackAPRS setTextCallBackAPRS = new Form1.SetTextCallBackAPRS(this.SetAPRSText);
				base.Invoke(setTextCallBackAPRS, new object[] { text });
				return;
			}
			this.dynamicStatus.Text = text;
		}

		// Token: 0x06000277 RID: 631 RVA: 0x00018068 File Offset: 0x00016268
		private void SetPictureText(string text)
		{
			if (this.chatImage.InvokeRequired)
			{
				Form1.SetTextCallbackPicture setTextCallbackPicture = new Form1.SetTextCallbackPicture(this.SetPictureText);
				base.Invoke(setTextCallbackPicture, new object[] { text });
				return;
			}
			if (text.Contains("NEW") && this.tabControl1.SelectedTab != this.APRSchatTab)
			{
				this.chatImage.Visible = true;
			}
		}

		// Token: 0x06000278 RID: 632 RVA: 0x000180D0 File Offset: 0x000162D0
		private void SetChatText(string text)
		{
			try
			{
				if (base.IsHandleCreated)
				{
					if (this.ChattextBox.InvokeRequired)
					{
						Form1.SetTextCallbackAPRS setTextCallbackAPRS = new Form1.SetTextCallbackAPRS(this.SetChatText);
						base.Invoke(setTextCallbackAPRS, new object[] { text });
					}
					else
					{
						TextBox chattextBox = this.ChattextBox;
						chattextBox.Text += text;
					}
				}
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000279 RID: 633 RVA: 0x00018150 File Offset: 0x00016350
		private void SetVUMeter(int vu)
		{
			if (this.VUMeter.InvokeRequired)
			{
				Form1.SetVUMeterCallback setVUMeterCallback = new Form1.SetVUMeterCallback(this.SetVUMeter);
				base.Invoke(setVUMeterCallback, new object[] { vu });
				return;
			}
			if (information.m_qsoStatus != information.QSOSTATUS.LISTENING)
			{
				if (!this.onAIRswitch.Checked)
				{
					this.VUMeter.MaxValue = (float)Math.Max((int)this.VUMeter.MaxValue, vu);
				}
				else
				{
					this.VUMeter.MaxValue = (float)Math.Max((int)this.VUMeter.MaxValue, vu);
				}
				this.VUMeter.Value = (float)vu;
			}
		}

		// Token: 0x0600027A RID: 634 RVA: 0x000181F0 File Offset: 0x000163F0
		private void SetSRCDSTText(string srcid, string dstid, string text)
		{
			if (this.hisDMRid.InvokeRequired)
			{
				Form1.SetSRCDSTCallback setSRCDSTCallback = new Form1.SetSRCDSTCallback(this.SetSRCDSTText);
				base.Invoke(setSRCDSTCallback, new object[] { srcid, dstid, text });
				return;
			}
			this.hisDMRid.Text = srcid;
			this.dynLastReflector.Text = text;
			information.hisDMRdest = " ";
		}

		// Token: 0x0600027B RID: 635 RVA: 0x00018254 File Offset: 0x00016454
		private void SetHisText(string call, string name, string dmrid, string dmrdest, string city, string country)
		{
			if (call == null)
			{
				call = " ";
			}
			if (this.hisName.InvokeRequired && this.hisCall.InvokeRequired && this.hisDMRid.InvokeRequired && this.dynamichisDMRdest.InvokeRequired && this.hisCity.InvokeRequired && this.hisCountry.InvokeRequired)
			{
				Form1.SetDMRIDTextCallback setDMRIDTextCallback = new Form1.SetDMRIDTextCallback(this.SetHisText);
				base.Invoke(setDMRIDTextCallback, new object[] { call, name, dmrid, dmrdest, city, country });
				return;
			}
			if (TimerRXTX.status == TimerRXTX.STATUS.TX || TimerRXTX.status == TimerRXTX.STATUS.RX)
			{
				if (string.IsNullOrEmpty(information.hisDMRdest))
				{
					information.hisDMRdest = "0";
				}
				if (!string.IsNullOrEmpty(name))
				{
					this.hisName.Text = name;
				}
				if (!string.IsNullOrEmpty(call))
				{
					this.hisCall.Text = call;
				}
				if (!string.IsNullOrEmpty(city))
				{
					this.hisCity.Text = city;
				}
				if (!string.IsNullOrEmpty(country))
				{
					this.hisCountry.Text = country;
				}
				else
				{
					this.hisCall.Text = information.hisCall;
				}
				if (information.stream_modus == information.MODUS.DMR)
				{
					if (!string.IsNullOrEmpty(dmrid))
					{
						this.hisDMRid.Text = dmrid;
					}
					if (!string.IsNullOrEmpty(dmrdest))
					{
						this.dynamichisDMRdest.Text = dmrdest;
					}
				}
				else if (information.stream_modus != information.MODUS.NXDN && !string.IsNullOrEmpty(dmrid))
				{
					this.hisDMRid.Text = dmrid;
				}
			}
			if (call.Trim().Equals("5057"))
			{
				return;
			}
			if (call.Trim().Equals("4000"))
			{
				return;
			}
			if (call.Trim().Equals("ANSAGE"))
			{
				return;
			}
			if (call.Equals("        "))
			{
				return;
			}
			if (information.hisCall != null || !string.IsNullOrEmpty(call) || !call.Equals(this.cached_text) || !call.Equals("        "))
			{
				ListViewItem listViewItem = new ListViewItem(new string[]
				{
					DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture),
					call,
					name,
					information.stream_modus.ToString()
				});
				listViewItem.ToolTipText = this.dynamichisDMRdest.Text;
				this.cached_text = call;
				ListViewItem listViewItem2 = new ListViewItem();
				listViewItem2.ToolTipText = this.dynamichisDMRdest.Text;
				listViewItem2.SubItems.Add(string.Format("{0:hh:mm tt}", DateTime.Now));
				if (!call.Equals(""))
				{
					listViewItem2.SubItems.Add(call);
				}
				else
				{
					listViewItem2.SubItems.Add(information.hisCall);
				}
				listViewItem2.SubItems.Add(name);
				listViewItem2.SubItems.Add(information.stream_modus.ToString());
				if (this.listView1.Items.Count > 12)
				{
					listViewItem2.SubItems.RemoveAt(0);
					this.listView1.Items.RemoveAt(0);
				}
				this.listView1.Items.Add(listViewItem);
				this.listView1.Items[this.listView1.Items.Count - 1].EnsureVisible();
				if (information.saveQSOLog)
				{
					string text;
					if (information.stream_modus == information.MODUS.DMR)
					{
						text = string.Concat(new string[]
						{
							string.Format("{0:d/M/yyyy HH:mm:sst}", DateTime.Now),
							";",
							call,
							";",
							name,
							";",
							information.stream_modus.ToString(),
							";",
							information.hisDMRdest.ToString()
						});
					}
					else
					{
						text = string.Concat(new string[]
						{
							string.Format("{0:d/M/yyyy HH:mm:sst}", DateTime.Now),
							";",
							call,
							";",
							name,
							";",
							information.stream_modus.ToString()
						});
					}
					LastHeardRecord.WriteToFile(text);
				}
			}
		}

		// Token: 0x0600027C RID: 636 RVA: 0x00018671 File Offset: 0x00016871
		private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			AboutBox1 aboutBox = new AboutBox1();
			aboutBox.ShowDialog(this);
			aboutBox.Dispose();
		}

		// Token: 0x0600027D RID: 637 RVA: 0x00018688 File Offset: 0x00016888
		private void setupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (Form1.IsNetworkAvailable())
			{
				this.dynamicStatus.Text = "";
				Setup setup = new Setup();
				setup.ShowDialog(this);
				setup.Dispose();
				if (information.m_device == information.DEVICE.DV3000R)
				{
					this.killTimerLabel.Visible = true;
					this.aMBEToolStripMenuItem.Visible = true;
				}
				else
				{
					this.killTimerLabel.Visible = false;
					this.aMBEToolStripMenuItem.Visible = false;
				}
				base.TopMost = information.appinforeground;
				this.APRSchatTab.Enabled = information.APRS;
				return;
			}
			this.dynamicStatus.Text = "No Network connection.";
		}

		// Token: 0x0600027E RID: 638 RVA: 0x00018728 File Offset: 0x00016928
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (Form1.ws != null)
			{
				Form1.ws.Stop();
				soundcard.stopWebSocket();
			}
			FusionQueue.stop();
			APRSClient.close();
			this.disconnect();
			Application.Exit();
			Environment.Exit(0);
		}

		// Token: 0x0600027F RID: 639 RVA: 0x0001875B File Offset: 0x0001695B
		private void showFreq(bool show)
		{
			if (show)
			{
				this.frequency.Text = information.myFREQ.Insert(3, ".").Insert(7, ".");
				return;
			}
			this.frequency.Text = "";
		}

		// Token: 0x06000280 RID: 640 RVA: 0x00018798 File Offset: 0x00016998
		private bool openSerialpoort()
		{
			if (information.enableAMBEServer)
			{
				return true;
			}
			if (!DVMEGASerial.open())
			{
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					this.dynamicStatus.Text = "Can not open COM port";
					break;
				case information.LANGUAGE.JAPANESE:
					this.dynamicStatus.Text = "COMポート利用不可";
					break;
				case information.LANGUAGE.CHINEES:
					this.dynamicStatus.Text = "Can not open COM port";
					break;
				case information.LANGUAGE.KOREAN:
					this.dynamicStatus.Text = "COM포트를 열수없음";
					break;
				}
				return false;
			}
			return true;
		}

		// Token: 0x06000281 RID: 641 RVA: 0x0001881E File Offset: 0x00016A1E
		private void Form1_Load(object sender, EventArgs e)
		{
			if (base.WindowState == FormWindowState.Minimized)
			{
				base.WindowState = FormWindowState.Normal;
			}
			base.Location = Settings.Default.F1Location;
		}

		// Token: 0x06000282 RID: 642 RVA: 0x00018840 File Offset: 0x00016A40
		private void checkVersion()
		{
			switch (utils.RunningPlatform())
			{
			case utils.Platform.Windows:
				try
				{
					WebClient webClient = new WebClient();
					string text;
					if (this.BETA)
					{
						text = webClient.DownloadString("http://software.pa7lim.nl/BlueDV/BETA/PREVERSION");
					}
					else
					{
						text = webClient.DownloadString("http://software.pa7lim.nl/BlueDV/BETA/VERSION");
					}
					double num;
					double num2;
					if (double.TryParse(text, out num) && double.TryParse(Assembly.GetExecutingAssembly().GetName().Version.ToString().Trim(), out num2) && num2 < num)
					{
						this.newVersionLinkLabel.Visible = true;
					}
					return;
				}
				catch (WebException)
				{
					return;
				}
				break;
			case utils.Platform.Linux:
				break;
			case utils.Platform.Mac:
				return;
			default:
				return;
			}
			try
			{
				double num3;
				double num4;
				if (double.TryParse(new WebClient().DownloadString("http://software.pa7lim.nl/BlueDV/BETA/Linux/VERSION-LINUX"), out num3) && double.TryParse(Assembly.GetExecutingAssembly().GetName().Version.ToString().Trim(), out num4) && num4 < num3)
				{
					this.newVersionLinkLabel.Visible = true;
				}
			}
			catch (WebException)
			{
			}
		}

		// Token: 0x06000283 RID: 643 RVA: 0x00018948 File Offset: 0x00016B48
		private static bool searchDVMEGA()
		{
			int num = 0;
			while (!information.foundDVMEGA && num < 20)
			{
				DVMEGASerial.getDVMEGAVersion();
				Thread.Sleep(200);
				num++;
			}
			return information.foundDVMEGA;
		}

		// Token: 0x06000284 RID: 644 RVA: 0x00018984 File Offset: 0x00016B84
		private void connectDMR()
		{
			information.DMRMODUS dmrmodus = information.m_dmrmodus;
			if (dmrmodus != information.DMRMODUS.BM)
			{
				if (dmrmodus != information.DMRMODUS.DMRPLUS)
				{
					return;
				}
			}
			else
			{
				DMRconnection.connect(information.myDMRHostname);
				try
				{
					this.dmrmaster.Text = JSONCallQuery.getMasterList()[JSONCallQuery.lookupDMRmaster(information.myDMRHostname)];
					return;
				}
				catch (Exception)
				{
					return;
				}
			}
			DMRPlusHostParser.downloadHosts2();
			DMRPlusHostParser.downloadHostsIPCS();
			DMRPlusHostParser.getHBLinkList();
			this.dmrmaster.Text = DMRPlusHostParser.lookupLogicName(information.myDMRPlusHostname);
			DMRPlus.connect(information.myDMRPlusHostname, information.myDMRPlusPort, information.myDMRPlusPassword);
		}

		// Token: 0x06000285 RID: 645 RVA: 0x00018A1C File Offset: 0x00016C1C
		private void connect()
		{
			if (!information.DMRconnected)
			{
				Setup.applyConfigVariables();
				if (Form1.IsNetworkAvailable())
				{
					this.dynamicStatus.Text = "";
					if (!this.openSerialpoort())
					{
						return;
					}
					this.setupToolStripMenuItem.Enabled = false;
					int num = 0;
					while (string.IsNullOrEmpty(information.myDVMEGAVersion) && num < 15)
					{
						if (string.IsNullOrEmpty(information.myDVMEGAVersion))
						{
							DVMEGASerial.getDVMEGAVersion();
							if (string.IsNullOrEmpty(information.myDVMEGAVersion))
							{
								Thread.Sleep(200);
								DVMEGASerial.getDVMEGAVersion();
							}
						}
						else
						{
							DVMEGASerial.getDVMEGAVersion();
							this.Firmware.Text = information.myDVMEGAVersion;
						}
						num++;
					}
					if (!string.IsNullOrEmpty(information.myDVMEGAVersion))
					{
						DVMEGASerial.getDVMEGAVersion();
						this.Firmware.Text = information.myDVMEGAVersion;
					}
					this.showFreq(true);
					switch (information.m_modus)
					{
					case information.MODUS.DMR:
						switch (information.m_dmrmodus)
						{
						case information.DMRMODUS.BM:
							DMRconnection.connect(information.myDMRHostname);
							try
							{
								this.dmrmaster.Text = JSONCallQuery.getMasterList()[JSONCallQuery.lookupDMRmaster(information.myDMRHostname)];
								return;
							}
							catch (Exception)
							{
								return;
							}
							break;
						case information.DMRMODUS.DMRPLUS:
							break;
						case information.DMRMODUS.XLXDMR:
						case information.DMRMODUS.FREEDMR:
						case information.DMRMODUS.SYSTEMX:
						case information.DMRMODUS.TGIF:
						case information.DMRMODUS.ADNSYSTEMS:
							return;
						default:
							return;
						}
						DMRPlusHostParser.downloadHosts2();
						DMRPlusHostParser.downloadHostsIPCS();
						DMRPlusHostParser.getHBLinkList();
						this.dmrmaster.Text = DMRPlusHostParser.lookupLogicName(information.myDMRPlusHostname);
						return;
					case information.MODUS.DSTAR:
					case information.MODUS.FUSION:
						break;
					default:
						return;
					}
				}
				else
				{
					switch (information.m_language)
					{
					case information.LANGUAGE.ENGLISH:
						this.dynamicStatus.Text = "No Network connection.";
						return;
					case information.LANGUAGE.JAPANESE:
						this.dynamicStatus.Text = "ネットワーク接続不可";
						return;
					case information.LANGUAGE.CHINEES:
						this.dynamicStatus.Text = "No Network connection.";
						return;
					case information.LANGUAGE.KOREAN:
						this.dynamicStatus.Text = "네트워크 연결안됨";
						return;
					default:
						return;
					}
				}
			}
		}

		// Token: 0x06000286 RID: 646 RVA: 0x00018BFC File Offset: 0x00016DFC
		private void disconnect()
		{
			if (information.DMRconnected)
			{
				this.showFreq(false);
				switch (information.m_modus)
				{
				case information.MODUS.DMR:
					DMRconnection.StatusTextChanged -= delegate(object sender1, EventArgs e1)
					{
						this.SetDMRText(DMRconnection.StatusText);
					};
					DMRconnection.close();
					DMRPlus.StatusTextChanged -= delegate(object sender1, EventArgs e1)
					{
						this.SetDMRText(DMRPlus.StatusText);
					};
					DMRPlus.close();
					break;
				case information.MODUS.DSTAR:
					switch (information.m_dstarmodus)
					{
					}
					break;
				case information.MODUS.FUSION:
					YSFFusion.StatusTextChangedFUSION -= delegate(object sender1, EventArgs e1)
					{
						this.SetFusionText(YSFFusion.StatusTextFUSION);
					};
					break;
				}
				DMRconnection.close();
				DMRPlus.close();
				TimerRXTX.stop();
				DPLUSconnection.unlink();
				DCSconnection.unlink();
				YSFFusion.close();
				FCSFusion.close();
				NXDNConnect.close();
				ShowCallTimer.stopTimers();
				this.hisName.Text = " ";
				this.hisCall.Text = " ";
				this.hisDMRid.Text = " ";
				this.hisDSTARidsmall.Text = " ";
				this.dynamicRXTX.Text = "";
				this.dmrmaster.Text = "";
				switch (information.m_language)
				{
				case information.LANGUAGE.ENGLISH:
					this.Firmware.Text = "Not detected";
					this.dynamicStatus.Text = "Disconnected";
					this.dynamicRXTX.Text = "LISTENING";
					break;
				case information.LANGUAGE.JAPANESE:
					this.Firmware.Text = "デバイス認識不可";
					this.dynamicStatus.Text = "未接続";
					this.dynamicRXTX.Text = "待機中";
					break;
				case information.LANGUAGE.CHINEES:
					this.Firmware.Text = "Not detected";
					this.dynamicStatus.Text = "Disconnected";
					this.dynamicRXTX.Text = "LISTENING";
					break;
				case information.LANGUAGE.KOREAN:
					this.Firmware.Text = "감지되지않음";
					this.dynamicStatus.Text = "연결끊김";
					this.dynamicRXTX.Text = "대기상태";
					break;
				}
				this.dynLastReflector.Text = " ";
				information.hisDMRdest = " ";
				this.setupToolStripMenuItem.Enabled = true;
				this.updateToolStripMenuItem.Enabled = true;
				information.DMRconnected = false;
				this.VUMeter.MaxValue = 80f;
			}
		}

		// Token: 0x06000287 RID: 647 RVA: 0x00002F42 File Offset: 0x00001142
		private void Connect_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x06000288 RID: 648 RVA: 0x00002F42 File Offset: 0x00001142
		private void roundButton2_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x06000289 RID: 649 RVA: 0x00018E5A File Offset: 0x0001705A
		public static bool IsNetworkAvailable()
		{
			return true;
		}

		// Token: 0x0600028A RID: 650 RVA: 0x00018E60 File Offset: 0x00017060
		public static bool IsNetworkAvailable(long minimumSpeed)
		{
			if (!NetworkInterface.GetIsNetworkAvailable())
			{
				return false;
			}
			foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel && networkInterface.Speed >= minimumSpeed && networkInterface.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) < 0 && networkInterface.Name.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) < 0 && !networkInterface.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600028B RID: 651 RVA: 0x00018EF8 File Offset: 0x000170F8
		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F2)
			{
				DVMEGAAMBE.setAMBEnoisecancel();
			}
			if (e.KeyCode == Keys.F3)
			{
				DVMEGAAMBE.setAMBEnoisecancelOff();
			}
			Keys keyCode = e.KeyCode;
			Keys keyCode2 = e.KeyCode;
			if (e.KeyCode == Keys.R && Control.ModifierKeys == Keys.Control)
			{
				e.SuppressKeyPress = true;
				this.comboBoxReflectorList.Focus();
			}
			if (e.KeyCode == Keys.M && Control.ModifierKeys == Keys.Control)
			{
				e.SuppressKeyPress = true;
				this.comboBoxModeSelector.Focus();
			}
			if (e.KeyCode == Keys.C && Control.ModifierKeys == Keys.Control && this.toggleSwitchDMRconnect.Enabled)
			{
				utils.BeepBeep(1000, 1000, 200);
				e.SuppressKeyPress = true;
				this.toggleSwitchDMRconnect.Checked = true;
			}
			if (e.KeyCode == Keys.D && Control.ModifierKeys == Keys.Control && this.toggleSwitchDMRconnect.Enabled)
			{
				e.SuppressKeyPress = true;
				this.toggleSwitchDMRconnect.Checked = false;
				utils.BeepBeep(1000, 1500, 200);
			}
			if (e.KeyCode == Keys.F && Control.ModifierKeys == (Keys.Shift | Keys.Control | Keys.Alt) && this.toggleSwitchFusionconnect.Enabled)
			{
				e.SuppressKeyPress = true;
				this.toggleSwitchFusionconnect.Checked = false;
				utils.BeepBeep(1000, 1500, 200);
			}
			if (e.KeyCode == Keys.F && Control.ModifierKeys == Keys.Control && this.toggleSwitchFusionconnect.Enabled)
			{
				e.SuppressKeyPress = true;
				this.toggleSwitchFusionconnect.Checked = true;
				utils.BeepBeep(1000, 1500, 200);
			}
			if (e.KeyCode == Keys.N && Control.ModifierKeys == (Keys.Shift | Keys.Control | Keys.Alt) && this.toggleSwitchNXDNconnect.Enabled)
			{
				e.SuppressKeyPress = true;
				this.toggleSwitchNXDNconnect.Checked = false;
				utils.BeepBeep(1000, 1500, 200);
			}
			if (e.KeyCode == Keys.N && Control.ModifierKeys == Keys.Control && this.toggleSwitchNXDNconnect.Enabled)
			{
				e.SuppressKeyPress = true;
				this.toggleSwitchNXDNconnect.Checked = true;
				utils.BeepBeep(1000, 1500, 200);
			}
			if (e.KeyCode == Keys.D && Control.ModifierKeys == (Keys.Shift | Keys.Control | Keys.Alt) && this.toggleSwitchDSTARconnect.Enabled)
			{
				utils.BeepBeep(1000, 1000, 200);
				e.SuppressKeyPress = true;
				this.toggleSwitchDSTARconnect.Checked = true;
			}
			if (e.KeyCode == Keys.X && Control.ModifierKeys == (Keys.Shift | Keys.Control | Keys.Alt) && this.toggleSwitchDSTARconnect.Enabled)
			{
				utils.BeepBeep(1000, 1500, 200);
				e.SuppressKeyPress = true;
				this.toggleSwitchDSTARconnect.Checked = false;
			}
			if (e.KeyCode == Keys.Z && Control.ModifierKeys == Keys.Control && this.toggleSwitchDSTARconnect.Checked)
			{
				e.SuppressKeyPress = true;
				this.radioButtonREF.Checked = true;
			}
			if (e.KeyCode == Keys.X && Control.ModifierKeys == Keys.Control && this.toggleSwitchDSTARconnect.Checked)
			{
				e.SuppressKeyPress = true;
				this.radioButtonDCS.Checked = true;
			}
			if (e.KeyCode == Keys.J && Control.ModifierKeys == Keys.Control && this.toggleSwitchDSTARconnect.Checked)
			{
				e.SuppressKeyPress = true;
				this.radioButtonXRF.Checked = true;
			}
			if (e.KeyCode == Keys.W && Control.ModifierKeys == Keys.Control && this.toggleSwitchDSTARconnect.Checked)
			{
				e.SuppressKeyPress = true;
				this.radioButtonXLX.Checked = true;
			}
			if (e.KeyCode == Keys.Y && Control.ModifierKeys == Keys.Control && this.toggleSwitchFusionconnect.Checked)
			{
				e.SuppressKeyPress = true;
				this.radioButtonREF.Checked = true;
			}
			if (e.KeyCode == Keys.F && Control.ModifierKeys == (Keys.Control | Keys.Alt) && this.toggleSwitchFusionconnect.Checked)
			{
				e.SuppressKeyPress = true;
				this.radioButtonDCS.Checked = true;
			}
			if (e.KeyCode == Keys.S && Control.ModifierKeys == Keys.Control && this.setupToolStripMenuItem.Enabled)
			{
				e.SuppressKeyPress = true;
				this.setupToolStripMenuItem.PerformClick();
			}
			if (e.KeyCode == Keys.H && Control.ModifierKeys == (Keys.Control | Keys.Alt))
			{
				e.SuppressKeyPress = true;
			}
			if (e.KeyCode == Keys.L && Control.ModifierKeys == Keys.Control && (this.toggleSwitchDSTARconnect.Checked || this.toggleSwitchFusionconnect.Checked))
			{
				e.SuppressKeyPress = true;
				this.linkButton.PerformClick();
			}
			if (e.KeyCode == Keys.U && Control.ModifierKeys == Keys.Control && (this.toggleSwitchDSTARconnect.Checked || this.toggleSwitchFusionconnect.Checked))
			{
				e.SuppressKeyPress = true;
				this.unlinkButton.PerformClick();
			}
			if (e.KeyCode == Keys.P && Control.ModifierKeys == Keys.Control)
			{
				e.SuppressKeyPress = true;
				this.toggleSwitchSerialConnect.Checked = true;
			}
			if (e.KeyCode == Keys.P && Control.ModifierKeys == (Keys.Shift | Keys.Control))
			{
				e.SuppressKeyPress = true;
				this.toggleSwitchSerialConnect.Checked = false;
			}
			if (e.KeyCode == Keys.G && Control.ModifierKeys == Keys.Control)
			{
				e.SuppressKeyPress = true;
				if (this.toggleSwitchGroupPrivate.Enabled)
				{
					this.toggleSwitchGroupPrivate.Checked = false;
				}
			}
			if (e.KeyCode == Keys.G && Control.ModifierKeys == (Keys.Shift | Keys.Control))
			{
				e.SuppressKeyPress = true;
				if (this.toggleSwitchGroupPrivate.Enabled)
				{
					this.toggleSwitchGroupPrivate.Checked = true;
				}
			}
			if (e.KeyCode == Keys.T && Control.ModifierKeys == Keys.Control)
			{
				bool enabled = this.toggleSwitchGroupPrivate.Enabled;
			}
			if (e.KeyCode == Keys.T && Control.ModifierKeys == (Keys.Shift | Keys.Control))
			{
				bool enabled2 = this.toggleSwitchGroupPrivate.Enabled;
			}
			if (e.KeyCode == Keys.N && Control.ModifierKeys == (Keys.Shift | Keys.Control))
			{
				bool enabled3 = this.toggleSwitchGroupPrivate.Enabled;
			}
			if (e.KeyCode == Keys.Space && !this.MessagetextBox.Focused)
			{
				e.SuppressKeyPress = true;
				if (this.ptt)
				{
					this.ptt = false;
				}
				else
				{
					this.ptt = true;
				}
				if (this.ptt && this.onAIRswitch.Enabled)
				{
					this.onAIRswitch.Checked = true;
				}
				else
				{
					this.onAIRswitch.Checked = false;
				}
			}
			if (e.KeyCode == Keys.Q && Control.ModifierKeys == Keys.Control)
			{
				this.toggleSwitchDMRconnect.Enabled = true;
			}
			if (e.KeyCode == Keys.D1)
			{
				Keys modifierKeys = Control.ModifierKeys;
			}
		}

		// Token: 0x0600028C RID: 652 RVA: 0x000195B8 File Offset: 0x000177B8
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			soundcard.stop();
			FusionQueue.stop();
			this.disconnect();
			DPLUSconnection.unlink();
			DCSconnection.unlink();
			DMRconnection.close();
			DMRPlus.close();
			FCSFusion.close();
			YSFFusion.close();
			NXDNConnect.close();
			DVMEGASerial.close();
			APRSClient.close();
			this.timer1.Stop();
			this.timer1.Dispose();
			modeTimer.stopTimer();
			TimerRXTX.stop();
			Application.ExitThread();
			if (Form1.ws != null)
			{
				Form1.ws.Stop();
				soundcard.stopWebSocket();
			}
			if (base.WindowState == FormWindowState.Normal)
			{
				Settings.Default.F1Location = base.Location;
			}
			else
			{
				Settings.Default.F1Location = base.RestoreBounds.Location;
			}
			Settings.Default.Save();
		}

		// Token: 0x0600028D RID: 653 RVA: 0x00019679 File Offset: 0x00017879
		private void qrzLookup(object sender, EventArgs e)
		{
			Process.Start(new ProcessStartInfo("https://www.qrz.com/db/" + this.hisCall.Text));
		}

		// Token: 0x0600028E RID: 654 RVA: 0x0001969C File Offset: 0x0001789C
		private void qrzLookupItem(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection selectedItems = this.listView1.SelectedItems;
			string text = "";
			foreach (object obj in selectedItems)
			{
				text = ((ListViewItem)obj).SubItems[1].Text;
			}
			Process.Start(new ProcessStartInfo("https://www.qrz.com/db/" + text));
		}

		// Token: 0x0600028F RID: 655 RVA: 0x00019720 File Offset: 0x00017920
		private void timer1_Tick(object sender, EventArgs e)
		{
			try
			{
				MMDeviceEnumerator mmdeviceEnumerator = new MMDeviceEnumerator();
				MMDevice mmdevice;
				if (!this.onAIRswitch.Checked)
				{
					mmdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
					mmdevice = mmdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
				}
				else
				{
					mmdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
					mmdevice = mmdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
				}
				this.VUMeter.Value = (float)((int)Math.Round((double)(mmdevice.AudioMeterInformation.MasterPeakValue * 100f)));
				Form1.maxValueVU = Math.Max(Form1.maxValueVU, (int)Math.Round((double)(mmdevice.AudioMeterInformation.MasterPeakValue * 100f)));
				this.VUMeter.MaxValue = (float)Form1.maxValueVU;
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000290 RID: 656 RVA: 0x000197D8 File Offset: 0x000179D8
		private void XLXDMR_screen(bool show)
		{
			if (show)
			{
				this.radioButtonREF.Text = "TST";
				this.radioButtonDCS.Text = "TS1";
				this.linkButton.Enabled = true;
				this.unlinkButton.Enabled = true;
				this.comboBoxReflectorList.Visible = true;
				this.comboBoxReflectorModule.Visible = true;
				this.linkButton.Visible = true;
				this.unlinkButton.Visible = true;
				this.radioButtonREF.Visible = false;
				this.radioButtonDCS.Visible = false;
				this.radioButtonXRF.Visible = false;
				this.radioButtonJPN.Visible = false;
				this.radioButtonXLX.Visible = false;
				this.selectXLXDMR();
				this.radioButtonREF.Checked = true;
				return;
			}
			this.comboBoxReflectorList.Visible = false;
			this.comboBoxReflectorModule.Visible = false;
			this.linkButton.Hide();
			this.unlinkButton.Hide();
			this.radioButtonREF.Hide();
			this.radioButtonDCS.Hide();
			this.radioButtonXRF.Hide();
			this.radioButtonJPN.Hide();
			this.radioButtonXLX.Hide();
		}

		// Token: 0x06000291 RID: 657 RVA: 0x00019908 File Offset: 0x00017B08
		private void FreeDMR_screen(bool show)
		{
			if (show)
			{
				this.radioButtonREF.Text = "TST";
				this.radioButtonDCS.Text = "TS1";
				this.linkButton.Enabled = true;
				this.unlinkButton.Enabled = true;
				this.comboBoxReflectorList.Visible = true;
				this.comboBoxReflectorModule.Visible = true;
				this.linkButton.Visible = true;
				this.unlinkButton.Visible = true;
				this.radioButtonREF.Visible = false;
				this.radioButtonDCS.Visible = false;
				this.radioButtonXRF.Visible = false;
				this.radioButtonJPN.Visible = false;
				this.radioButtonXLX.Visible = false;
				this.selectFreeDMR();
				this.radioButtonREF.Checked = true;
				return;
			}
			this.comboBoxReflectorList.Visible = false;
			this.comboBoxReflectorModule.Visible = false;
			this.linkButton.Hide();
			this.unlinkButton.Hide();
			this.radioButtonREF.Hide();
			this.radioButtonDCS.Hide();
			this.radioButtonXRF.Hide();
			this.radioButtonJPN.Hide();
			this.radioButtonXLX.Hide();
		}

		// Token: 0x06000292 RID: 658 RVA: 0x00019A38 File Offset: 0x00017C38
		private void SystemX_screen(bool show)
		{
			if (show)
			{
				this.radioButtonREF.Text = "TST";
				this.radioButtonDCS.Text = "TS1";
				this.linkButton.Enabled = true;
				this.unlinkButton.Enabled = true;
				this.comboBoxReflectorList.Visible = true;
				this.comboBoxReflectorModule.Visible = true;
				this.linkButton.Visible = true;
				this.unlinkButton.Visible = true;
				this.radioButtonREF.Visible = false;
				this.radioButtonDCS.Visible = false;
				this.radioButtonXRF.Visible = false;
				this.radioButtonJPN.Visible = false;
				this.radioButtonXLX.Visible = false;
				this.selectSystemX();
				this.radioButtonREF.Checked = true;
				return;
			}
			this.comboBoxReflectorList.Visible = false;
			this.comboBoxReflectorModule.Visible = false;
			this.linkButton.Hide();
			this.unlinkButton.Hide();
			this.radioButtonREF.Hide();
			this.radioButtonDCS.Hide();
			this.radioButtonXRF.Hide();
			this.radioButtonJPN.Hide();
			this.radioButtonXLX.Hide();
		}

		// Token: 0x06000293 RID: 659 RVA: 0x00019B68 File Offset: 0x00017D68
		private void TGIF_screen(bool show)
		{
			if (show)
			{
				this.radioButtonREF.Text = "TST";
				this.radioButtonDCS.Text = "TS1";
				this.linkButton.Enabled = true;
				this.unlinkButton.Enabled = true;
				this.comboBoxReflectorList.Visible = true;
				this.comboBoxReflectorModule.Visible = true;
				this.linkButton.Visible = true;
				this.unlinkButton.Visible = true;
				this.radioButtonREF.Visible = false;
				this.radioButtonDCS.Visible = false;
				this.radioButtonXRF.Visible = false;
				this.radioButtonJPN.Visible = false;
				this.radioButtonXLX.Visible = false;
				this.selectTGIF();
				this.radioButtonREF.Checked = true;
				return;
			}
			this.comboBoxReflectorList.Visible = false;
			this.comboBoxReflectorModule.Visible = false;
			this.linkButton.Hide();
			this.unlinkButton.Hide();
			this.radioButtonREF.Hide();
			this.radioButtonDCS.Hide();
			this.radioButtonXRF.Hide();
			this.radioButtonJPN.Hide();
			this.radioButtonXLX.Hide();
		}

		// Token: 0x06000294 RID: 660 RVA: 0x00019C98 File Offset: 0x00017E98
		private void ADNSYSTEMS_screen(bool show)
		{
			if (show)
			{
				this.radioButtonREF.Text = "TST";
				this.radioButtonDCS.Text = "TS1";
				this.linkButton.Enabled = true;
				this.unlinkButton.Enabled = true;
				this.comboBoxReflectorList.Visible = true;
				this.comboBoxReflectorModule.Visible = true;
				this.linkButton.Visible = true;
				this.unlinkButton.Visible = true;
				this.radioButtonREF.Visible = false;
				this.radioButtonDCS.Visible = false;
				this.radioButtonXRF.Visible = false;
				this.radioButtonJPN.Visible = false;
				this.radioButtonXLX.Visible = false;
				this.selectADNSYSTEMS();
				this.radioButtonREF.Checked = true;
				return;
			}
			this.comboBoxReflectorList.Visible = false;
			this.comboBoxReflectorModule.Visible = false;
			this.linkButton.Hide();
			this.unlinkButton.Hide();
			this.radioButtonREF.Hide();
			this.radioButtonDCS.Hide();
			this.radioButtonXRF.Hide();
			this.radioButtonJPN.Hide();
			this.radioButtonXLX.Hide();
		}

		// Token: 0x06000295 RID: 661 RVA: 0x00019DC8 File Offset: 0x00017FC8
		private void DSTAR_screen(bool show)
		{
			if (!show)
			{
				this.XLXURLpictureBox.Visible = false;
				this.comboBoxReflectorList.Visible = false;
				this.comboBoxReflectorModule.Visible = false;
				this.linkButton.Hide();
				this.unlinkButton.Hide();
				this.radioButtonREF.Hide();
				this.radioButtonDCS.Hide();
				this.radioButtonXRF.Hide();
				this.radioButtonJPN.Hide();
				this.radioButtonXLX.Hide();
				return;
			}
			this.radioButtonREF.Text = "REF";
			this.radioButtonDCS.Text = "DCS";
			this.radioButtonXRF.Text = "XRF";
			this.radioButtonXLX.Text = "XLX";
			this.linkButton.Enabled = true;
			this.unlinkButton.Enabled = true;
			this.comboBoxReflectorList.Visible = true;
			this.comboBoxReflectorModule.Visible = true;
			this.linkButton.Visible = true;
			this.unlinkButton.Visible = true;
			this.radioButtonREF.Visible = true;
			this.radioButtonDCS.Visible = true;
			this.radioButtonXRF.Visible = true;
			if (information.m_language == information.LANGUAGE.JAPANESE)
			{
				this.radioButtonJPN.Visible = true;
			}
			this.radioButtonXLX.Visible = true;
			this.selectREF();
			string savedReflectorType = Settings.Default.savedReflectorType;
			if (savedReflectorType == "REF")
			{
				this.radioButtonREF.PerformClick();
				return;
			}
			if (savedReflectorType == "XRF")
			{
				this.radioButtonXRF.PerformClick();
				return;
			}
			if (savedReflectorType == "DCS")
			{
				this.radioButtonDCS.PerformClick();
				return;
			}
			if (savedReflectorType == "XLX")
			{
				this.radioButtonXLX.PerformClick();
				return;
			}
			if (!(savedReflectorType == "JPN"))
			{
				return;
			}
			this.radioButtonJPN.PerformClick();
		}

		// Token: 0x06000296 RID: 662 RVA: 0x00019FA8 File Offset: 0x000181A8
		private void Fusion_screen(bool show)
		{
			if (show)
			{
				this.radioButtonREF.Text = "YSF";
				this.radioButtonDCS.Text = "FCS";
				this.radioButtonXRF.Text = "XLX";
				this.comboBoxReflectorList.Visible = true;
				this.comboBoxReflectorModule.Visible = false;
				this.linkButton.Enabled = true;
				this.unlinkButton.Enabled = true;
				this.linkButton.Visible = true;
				this.unlinkButton.Visible = true;
				this.radioButtonREF.Visible = true;
				this.radioButtonDCS.Visible = true;
				this.radioButtonXRF.Visible = true;
				this.radioButtonJPN.Visible = false;
				this.radioButtonREF.Checked = true;
				return;
			}
			this.comboBoxReflectorList.Visible = false;
			this.comboBoxReflectorModule.Visible = false;
			this.linkButton.Enabled = false;
			this.unlinkButton.Enabled = false;
			this.linkButton.Hide();
			this.unlinkButton.Hide();
			this.radioButtonREF.Hide();
			this.radioButtonDCS.Hide();
			this.radioButtonXRF.Hide();
			this.radioButtonJPN.Hide();
			this.radioButtonXLX.Hide();
		}

		// Token: 0x06000297 RID: 663 RVA: 0x0001A0F0 File Offset: 0x000182F0
		private void NXDN_screen(bool show)
		{
			if (show)
			{
				this.comboBoxReflectorList.Visible = true;
				this.comboBoxReflectorModule.Visible = false;
				this.linkButton.Enabled = true;
				this.unlinkButton.Enabled = true;
				this.linkButton.Visible = true;
				this.unlinkButton.Visible = true;
				this.radioButtonREF.Visible = false;
				this.radioButtonDCS.Visible = false;
				this.radioButtonXRF.Visible = false;
				this.radioButtonJPN.Visible = false;
				this.radioButtonREF.Checked = false;
				return;
			}
			this.comboBoxReflectorList.Visible = false;
			this.comboBoxReflectorModule.Visible = false;
			this.linkButton.Enabled = false;
			this.unlinkButton.Enabled = false;
			this.linkButton.Hide();
			this.unlinkButton.Hide();
			this.radioButtonREF.Hide();
			this.radioButtonDCS.Hide();
			this.radioButtonXRF.Hide();
			this.radioButtonJPN.Hide();
			this.radioButtonXLX.Hide();
		}

		// Token: 0x06000298 RID: 664 RVA: 0x0001A208 File Offset: 0x00018408
		private void connectFromSwitch()
		{
			if (Form1.searchDVMEGA())
			{
				if (information.m_device == information.DEVICE.DV3000R)
				{
					soundcard.start();
					if (information.usePTTkeying)
					{
						AMBEPTTControl.open();
					}
					this.showFreq(false);
					information.DVMEGAbufferDSTAR = 48;
					this.toggleSwitchDMRconnect.Enabled = true;
					this.toggleSwitchDSTARconnect.Enabled = true;
					this.toggleSwitchFusionconnect.Enabled = true;
					this.toggleSwitchNXDNconnect.Enabled = true;
					this.comboBoxModeSelector.Enabled = false;
					this.onAIRswitch.Enabled = true;
					this.VOXcheckBox.Enabled = true;
					this.VOXLevelTrackBar.Enabled = true;
					this.hangTimeTrackBar.Enabled = true;
					this.DSTARouttrackBar.Enabled = true;
					this.DSTARintrackBar.Enabled = true;
					this.DMRouttrackBar.Enabled = true;
					this.DMRintrackBar.Enabled = true;
					this.FUSIONouttrackBar.Enabled = true;
					this.FUSIONintrackBar.Enabled = true;
					this.NXDNouttrackBar.Enabled = true;
					this.NXDNintrackBar.Enabled = true;
					this.VUMeter.Visible = true;
					this.DMRManualDialComboBox1.Enabled = true;
					this.toggleSwitchGroupPrivate.Enabled = true;
					this.DMRManualDialComboBox1.Text = Settings.Default.savedDMRmanualDial;
					this.toggleSwitchGroupPrivate.Checked = Settings.Default.savedprivateorGroupswitch;
					if (this.timer1 != null && information.m_device == information.DEVICE.DV3000R)
					{
						this.timer1.Enabled = true;
					}
					if (information.autostartDMR)
					{
						this.toggleSwitchDMRconnect.Checked = true;
					}
					if (information.autostartDSTAR)
					{
						this.toggleSwitchDSTARconnect.Checked = true;
					}
					if (information.autostartFUSION)
					{
						this.toggleSwitchFusionconnect.Checked = true;
						return;
					}
				}
				else
				{
					this.dynamicStatus.Text = "";
					this.showFreq(true);
					this.toggleSwitchDMRconnect.Enabled = true;
					this.toggleSwitchDSTARconnect.Enabled = true;
					this.toggleSwitchFusionconnect.Enabled = true;
					this.toggleSwitchNXDNconnect.Enabled = false;
					this.comboBoxModeSelector.Enabled = true;
					this.onAIRswitch.Enabled = false;
					if (information.autostartDMR)
					{
						this.toggleSwitchDMRconnect.Checked = true;
					}
					if (information.autostartDSTAR)
					{
						this.toggleSwitchDSTARconnect.Checked = true;
					}
					if (information.autostartFUSION)
					{
						this.toggleSwitchFusionconnect.Checked = true;
					}
					if (this.timer1 != null && information.m_device == information.DEVICE.DV3000R)
					{
						this.timer1.Enabled = false;
					}
				}
			}
		}

		// Token: 0x06000299 RID: 665 RVA: 0x0001A474 File Offset: 0x00018674
		private void toggleSwitchSerialConnect_CheckedChanged(object sender, EventArgs e)
		{
			this.setupToolStripMenuItem.Enabled = false;
			this.updateToolStripMenuItem.Enabled = false;
			if (information.m_device == information.DEVICE.DV3000R)
			{
				this.simpleModeCheckBox.Checked = false;
				this.simpleModeCheckBox.Visible = false;
				information.DMRsimpleMode = false;
			}
			else
			{
				this.simpleModeCheckBox.Visible = true;
			}
			if (this.toggleSwitchSerialConnect.Checked)
			{
				if (information.m_device == information.DEVICE.DV3000R)
				{
					this.aMBEToolStripMenuItem.Enabled = false;
					if (this.VOXcheckBox.Enabled)
					{
						soundcard.record();
					}
				}
				Setup.applyConfigVariables();
				if (information.foundDVMEGA)
				{
					this.showFreq(true);
					return;
				}
				information.DEVICECONNECTION deviceconnection = information.m_deviceconnection;
				if (deviceconnection != information.DEVICECONNECTION.SERIAL)
				{
					if (deviceconnection != information.DEVICECONNECTION.AMBESERVER)
					{
						return;
					}
					AMBESERVER.startServer(information.AMBEServerHost, int.Parse(information.AMBEServerHostPort));
					this.connectFromSwitch();
					return;
				}
				else if (this.openSerialpoort())
				{
					this.connectFromSwitch();
					return;
				}
			}
			else
			{
				this.setupToolStripMenuItem.Enabled = true;
				if (information.m_device == information.DEVICE.DV3000R)
				{
					this.aMBEToolStripMenuItem.Enabled = true;
				}
				this.updateToolStripMenuItem.Enabled = true;
				DVMEGASerial.close();
				AMBESERVER.stopServer();
				this.showFreq(false);
				this.toggleSwitchDMRconnect.Enabled = false;
				this.toggleSwitchDSTARconnect.Enabled = false;
				this.toggleSwitchFusionconnect.Enabled = false;
				this.toggleSwitchNXDNconnect.Enabled = false;
				information.foundDVMEGA = false;
				this.Firmware.Text = " ";
				this.dynamicStatus.Text = "";
				information.myDVMEGAVersion = "";
				this.toggleSwitchDMRconnect.Checked = false;
				this.toggleSwitchDSTARconnect.Checked = false;
				this.toggleSwitchFusionconnect.Checked = false;
				this.toggleSwitchNXDNconnect.Checked = false;
				this.comboBoxModeSelector.Enabled = false;
				modeTimer.setMode(information.MODUS.IDLE);
				soundcard.stop();
				AMBEPTTControl.close();
				this.onAIRswitch.Checked = false;
				this.onAIRswitch.Enabled = false;
				this.DMRAMBEButton.Enabled = false;
				this.DSTARAMBEButton.Enabled = false;
				this.FUSIONAMBEButton.Enabled = false;
				this.NXDNAMBEButton.Enabled = false;
				this.VOXcheckBox.Enabled = false;
				this.VOXLevelTrackBar.Enabled = false;
				this.hangTimeTrackBar.Enabled = false;
				this.DSTARouttrackBar.Enabled = false;
				this.DSTARintrackBar.Enabled = false;
				this.DMRouttrackBar.Enabled = false;
				this.DMRintrackBar.Enabled = false;
				this.FUSIONouttrackBar.Enabled = false;
				this.FUSIONintrackBar.Enabled = false;
				soundcard.stoprecording();
				if (this.timer1 != null)
				{
					this.timer1.Enabled = false;
				}
				this.DSTAR_screen(false);
				MMDVM_HS_TIMER.Stop();
				this.VOXcheckBox.Checked = false;
				this.DMRAMBEButton.Enabled = false;
			}
		}

		// Token: 0x0600029A RID: 666 RVA: 0x0001A72C File Offset: 0x0001892C
		private void toggleSwitchDMRconnect_CheckedChanged(object sender, EventArgs e)
		{
			information.m_modus = information.MODUS.DMR;
			if (this.toggleSwitchDMRconnect.Checked)
			{
				information.DVMEGAmodus |= 2;
				DVMEGASerial.setDVMEGAmode();
				if (information.m_dmrmodus == information.DMRMODUS.XLXDMR)
				{
					this.comboBoxModeSelector.Enabled = true;
					this.comboBoxModeSelector.Items.Add("DMR");
					int num = this.comboBoxModeSelector.FindString("DMR");
					this.comboBoxModeSelector.SelectedIndex = num;
				}
				else if (information.m_dmrmodus == information.DMRMODUS.FREEDMR)
				{
					this.comboBoxModeSelector.Enabled = true;
					this.comboBoxModeSelector.Items.Add("DMR");
					int num2 = this.comboBoxModeSelector.FindString("DMR");
					this.comboBoxModeSelector.SelectedIndex = num2;
				}
				else if (information.m_dmrmodus == information.DMRMODUS.SYSTEMX)
				{
					this.comboBoxModeSelector.Enabled = true;
					this.comboBoxModeSelector.Items.Add("DMR");
					int num3 = this.comboBoxModeSelector.FindString("DMR");
					this.comboBoxModeSelector.SelectedIndex = num3;
				}
				else if (information.m_dmrmodus == information.DMRMODUS.TGIF)
				{
					this.comboBoxModeSelector.Enabled = true;
					this.comboBoxModeSelector.Items.Add("DMR");
					int num4 = this.comboBoxModeSelector.FindString("DMR");
					this.comboBoxModeSelector.SelectedIndex = num4;
				}
				else if (information.m_dmrmodus == information.DMRMODUS.ADNSYSTEMS)
				{
					this.comboBoxModeSelector.Enabled = true;
					this.comboBoxModeSelector.Items.Add("DMR");
					int num5 = this.comboBoxModeSelector.FindString("DMR");
					this.comboBoxModeSelector.SelectedIndex = num5;
				}
				else
				{
					this.connectDMR();
				}
				if (information.m_device == information.DEVICE.DV3000R)
				{
					this.DMRAMBEButton.Enabled = true;
					this.DMRAMBEButton.PerformClick();
					return;
				}
			}
			else
			{
				if (information.m_dmrmodus == information.DMRMODUS.XLXDMR)
				{
					if (this.comboBoxModeSelector.SelectedItem.Equals("DMR"))
					{
						this.DSTAR_screen(false);
					}
					this.comboBoxModeSelector.Items.Remove("DMR");
					if (this.comboBoxModeSelector.Items.Count > 0)
					{
						this.comboBoxModeSelector.SelectedIndex = 0;
					}
					if (!this.toggleSwitchFusionconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
					if (!this.toggleSwitchDSTARconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
					if (!this.toggleSwitchNXDNconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
				}
				else if (information.m_dmrmodus == information.DMRMODUS.FREEDMR)
				{
					if (this.comboBoxModeSelector.SelectedItem.Equals("DMR"))
					{
						this.DSTAR_screen(false);
					}
					this.comboBoxModeSelector.Items.Remove("DMR");
					if (this.comboBoxModeSelector.Items.Count > 0)
					{
						this.comboBoxModeSelector.SelectedIndex = 0;
					}
					if (!this.toggleSwitchFusionconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
					if (!this.toggleSwitchDSTARconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
					if (!this.toggleSwitchNXDNconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
				}
				else if (information.m_dmrmodus == information.DMRMODUS.SYSTEMX)
				{
					if (this.comboBoxModeSelector.SelectedItem.Equals("DMR"))
					{
						this.DSTAR_screen(false);
					}
					this.comboBoxModeSelector.Items.Remove("DMR");
					if (this.comboBoxModeSelector.Items.Count > 0)
					{
						this.comboBoxModeSelector.SelectedIndex = 0;
					}
					if (!this.toggleSwitchFusionconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
					if (!this.toggleSwitchDSTARconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
					if (!this.toggleSwitchNXDNconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
				}
				else if (information.m_dmrmodus == information.DMRMODUS.TGIF)
				{
					if (this.comboBoxModeSelector.SelectedItem.Equals("DMR"))
					{
						this.DSTAR_screen(false);
					}
					this.comboBoxModeSelector.Items.Remove("DMR");
					if (this.comboBoxModeSelector.Items.Count > 0)
					{
						this.comboBoxModeSelector.SelectedIndex = 0;
					}
					if (!this.toggleSwitchFusionconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
					if (!this.toggleSwitchDSTARconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
					if (!this.toggleSwitchNXDNconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
				}
				else if (information.m_dmrmodus == information.DMRMODUS.ADNSYSTEMS)
				{
					if (this.comboBoxModeSelector.SelectedItem.Equals("DMR"))
					{
						this.DSTAR_screen(false);
					}
					this.comboBoxModeSelector.Items.Remove("DMR");
					if (this.comboBoxModeSelector.Items.Count > 0)
					{
						this.comboBoxModeSelector.SelectedIndex = 0;
					}
					if (!this.toggleSwitchFusionconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
					if (!this.toggleSwitchDSTARconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
					if (!this.toggleSwitchNXDNconnect.Checked)
					{
						this.comboBoxModeSelector.Enabled = false;
					}
				}
				this.SetTextModeMode(information.MODUS.DMR, " ");
				information.DVMEGAmodus ^= 2;
				DVMEGASerial.setDVMEGAmode();
				this.dmrmaster.Text = "";
				DMRconnection.close();
				DMRPlus.close();
				this.DMRAMBEButton.Enabled = false;
				if (information.setAMBEMode == information.MODUS.DMR && information.m_device == information.DEVICE.DV3000R)
				{
					if (this.toggleSwitchDSTARconnect.Checked)
					{
						this.DSTARAMBEButton.PerformClick();
					}
					else if (this.toggleSwitchFusionconnect.Checked)
					{
						this.FUSIONAMBEButton.PerformClick();
					}
					else if (this.toggleSwitchNXDNconnect.Checked)
					{
						this.NXDNAMBEButton.PerformClick();
					}
					if (!this.toggleSwitchDSTARconnect.Checked && !this.toggleSwitchDMRconnect.Checked && !this.toggleSwitchFusionconnect.Checked && !this.toggleSwitchNXDNconnect.Checked)
					{
						this.DSTARAMBEpictureBox.Image = Resources.LED_OFF;
						this.DMRAMBEpictureBox.Image = Resources.LED_OFF;
						this.FUSIONAMBEpictureBox.Image = Resources.LED_OFF;
						this.NXDNAMBEpictureBox.Image = Resources.LED_OFF;
					}
				}
			}
		}

		// Token: 0x0600029B RID: 667 RVA: 0x0001AD84 File Offset: 0x00018F84
		private void toggleSwitchDSTARconnect_CheckedChanged(object sender, EventArgs e)
		{
			if (this.toggleSwitchDSTARconnect.Checked)
			{
				information.DVMEGAmodus |= 1;
				DVMEGASerial.setDVMEGAmode();
				this.comboBoxModeSelector.Enabled = true;
				this.comboBoxModeSelector.Items.Add("DSTAR");
				int num = this.comboBoxModeSelector.FindString("DSTAR");
				this.comboBoxModeSelector.SelectedIndex = num;
				this.radioButtonREF.PerformClick();
				if (information.m_device == information.DEVICE.DV3000R)
				{
					this.DSTARAMBEButton.Enabled = true;
					this.DSTARAMBEButton.PerformClick();
				}
				if (information.defaultDSTARReflector.Trim().Length > 0)
				{
					DSTARhandler.connectProcessing(information.defaultDSTARReflector.Trim().PadRight(7) + "L");
					return;
				}
			}
			else
			{
				information.DVMEGAmodus ^= 1;
				DVMEGASerial.setDVMEGAmode();
				DPLUSconnection.unlink();
				DCSconnection.unlink();
				this.SetTextModeMode(information.MODUS.DSTAR, " ");
				if (this.comboBoxModeSelector.SelectedItem.Equals("DSTAR"))
				{
					this.DSTAR_screen(false);
				}
				this.comboBoxModeSelector.Items.Remove("DSTAR");
				if (this.comboBoxModeSelector.Items.Count > 0)
				{
					this.comboBoxModeSelector.SelectedIndex = 0;
				}
				if (!this.toggleSwitchFusionconnect.Checked)
				{
					this.comboBoxModeSelector.Enabled = false;
				}
				if (information.setAMBEMode == information.MODUS.DSTAR && information.m_device == information.DEVICE.DV3000R)
				{
					if (this.toggleSwitchDMRconnect.Checked)
					{
						this.DMRAMBEButton.PerformClick();
					}
					else if (this.toggleSwitchFusionconnect.Checked)
					{
						this.FUSIONAMBEButton.PerformClick();
					}
					else if (this.toggleSwitchNXDNconnect.Checked)
					{
						this.NXDNAMBEButton.PerformClick();
					}
					if (!this.toggleSwitchDSTARconnect.Checked && !this.toggleSwitchDMRconnect.Checked && !this.toggleSwitchFusionconnect.Checked && !this.toggleSwitchNXDNconnect.Checked)
					{
						this.DSTARAMBEpictureBox.Image = Resources.LED_OFF;
						this.DMRAMBEpictureBox.Image = Resources.LED_OFF;
						this.FUSIONAMBEpictureBox.Image = Resources.LED_OFF;
						this.NXDNAMBEpictureBox.Image = Resources.LED_OFF;
					}
				}
				this.DSTARAMBEButton.Enabled = false;
			}
		}

		// Token: 0x0600029C RID: 668 RVA: 0x0001AFC8 File Offset: 0x000191C8
		private void toggleSwitchFusionconnect_CheckedChanged(object sender, EventArgs e)
		{
			if (this.toggleSwitchFusionconnect.Checked)
			{
				information.DVMEGAmodus |= 4;
				DVMEGASerial.setDVMEGAmode();
				this.comboBoxModeSelector.Enabled = true;
				this.comboBoxModeSelector.Items.Add("FUSION");
				int num = this.comboBoxModeSelector.FindString("FUSION");
				this.comboBoxModeSelector.SelectedIndex = num;
				FCSFusion.close();
				YSFFusion.close();
				if (information.autostartFUSION)
				{
					if (information.fusionAutoStartProtocol.Equals("YSF"))
					{
						information.m_fusionmodus = information.FUSIONMODUS.YSF;
						int num2 = JSONCallQuery.searchYSFhost(information.defaultYSFReflector);
						if (num2 > 0)
						{
							YSFFusion.connect(JSONCallQuery.lookupFusionHostname(num2), JSONCallQuery.lookupFusionPort(num2));
						}
					}
					else
					{
						information.m_fusionmodus = information.FUSIONMODUS.FCS;
						information.fusionReflector = information.defaultFCSReflector;
						FCSFusion.connect(information.defaultFCSReflector.Substring(0, 6) + ".xreflector.net", 62500);
					}
				}
				if (information.m_device == information.DEVICE.DV3000R)
				{
					this.FUSIONAMBEButton.Enabled = true;
					this.FUSIONAMBEButton.PerformClick();
					return;
				}
			}
			else
			{
				information.DVMEGAmodus ^= 4;
				DVMEGASerial.setDVMEGAmode();
				YSFFusion.close();
				FCSFusion.close();
				this.SetTextModeMode(information.MODUS.FUSION, " ");
				if (this.comboBoxModeSelector.SelectedItem.Equals("FUSION"))
				{
					this.DSTAR_screen(false);
				}
				this.comboBoxModeSelector.Items.Remove("FUSION");
				if (this.comboBoxModeSelector.Items.Count > 0)
				{
					this.comboBoxModeSelector.SelectedIndex = 0;
				}
				if (!this.toggleSwitchDSTARconnect.Checked)
				{
					this.comboBoxModeSelector.Enabled = false;
				}
				if (information.setAMBEMode == information.MODUS.FUSION && information.m_device == information.DEVICE.DV3000R)
				{
					if (this.toggleSwitchDSTARconnect.Checked)
					{
						this.DSTARAMBEButton.PerformClick();
					}
					else if (this.toggleSwitchDMRconnect.Checked)
					{
						this.DMRAMBEButton.PerformClick();
					}
					else if (this.toggleSwitchNXDNconnect.Checked)
					{
						this.NXDNAMBEButton.PerformClick();
					}
					if (!this.toggleSwitchDSTARconnect.Checked && !this.toggleSwitchDMRconnect.Checked && !this.toggleSwitchFusionconnect.Checked && !this.toggleSwitchNXDNconnect.Checked)
					{
						this.DSTARAMBEpictureBox.Image = Resources.LED_OFF;
						this.DMRAMBEpictureBox.Image = Resources.LED_OFF;
						this.FUSIONAMBEpictureBox.Image = Resources.LED_OFF;
						this.NXDNAMBEpictureBox.Image = Resources.LED_OFF;
					}
				}
				this.FUSIONAMBEButton.Enabled = false;
			}
		}

		// Token: 0x0600029D RID: 669 RVA: 0x0001B248 File Offset: 0x00019448
		private void toggleSwitchNXDNconnect_CheckedChanged(object sender, EventArgs e)
		{
			if (this.toggleSwitchNXDNconnect.Checked)
			{
				information.DVMEGAmodus |= 16;
				DVMEGASerial.setDVMEGAmode();
				DVMEGASerial.setmode2NXDN();
				this.comboBoxModeSelector.Enabled = true;
				this.comboBoxModeSelector.Items.Add("NXDN");
				int num = this.comboBoxModeSelector.FindString("NXDN");
				this.comboBoxModeSelector.SelectedIndex = num;
				NXDNConnect.close();
				if (information.m_device == information.DEVICE.DV3000R)
				{
					this.NXDNAMBEButton.Enabled = true;
					this.NXDNAMBEButton.PerformClick();
					return;
				}
			}
			else
			{
				information.DVMEGAmodus ^= 16;
				DVMEGASerial.setDVMEGAmode();
				NXDNConnect.close();
				this.SetTextModeMode(information.MODUS.NXDN, " ");
				if (this.comboBoxModeSelector.SelectedItem.Equals("NXDN"))
				{
					this.DSTAR_screen(false);
				}
				this.comboBoxModeSelector.Items.Remove("NXDN");
				if (this.comboBoxModeSelector.Items.Count > 0)
				{
					this.comboBoxModeSelector.SelectedIndex = 0;
				}
				if (!this.toggleSwitchDSTARconnect.Checked)
				{
					this.comboBoxModeSelector.Enabled = false;
				}
				if (information.setAMBEMode == information.MODUS.NXDN && information.m_device == information.DEVICE.DV3000R)
				{
					if (this.toggleSwitchDSTARconnect.Checked)
					{
						this.DSTARAMBEButton.PerformClick();
					}
					else if (this.toggleSwitchDMRconnect.Checked)
					{
						this.DMRAMBEButton.PerformClick();
					}
					else if (this.toggleSwitchFusionconnect.Checked)
					{
						this.FUSIONAMBEButton.PerformClick();
					}
					if (!this.toggleSwitchDSTARconnect.Checked && !this.toggleSwitchDMRconnect.Checked && !this.toggleSwitchFusionconnect.Checked && !this.toggleSwitchNXDNconnect.Checked)
					{
						this.DSTARAMBEpictureBox.Image = Resources.LED_OFF;
						this.DMRAMBEpictureBox.Image = Resources.LED_OFF;
						this.FUSIONAMBEpictureBox.Image = Resources.LED_OFF;
						this.NXDNAMBEpictureBox.Image = Resources.LED_OFF;
					}
				}
				this.NXDNAMBEButton.Enabled = false;
			}
		}

		// Token: 0x0600029E RID: 670 RVA: 0x0001B454 File Offset: 0x00019654
		private void linkButton_Click(object sender, EventArgs e)
		{
			if (this.comboBoxModeSelector.SelectedItem.Equals("DSTAR"))
			{
				information.m_modus = information.MODUS.DSTAR;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("FUSION"))
			{
				information.m_modus = information.MODUS.FUSION;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("DMR"))
			{
				information.m_modus = information.MODUS.DMR;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("NXDN"))
			{
				information.m_modus = information.MODUS.NXDN;
			}
			switch (information.m_modus)
			{
			case information.MODUS.DMR:
				switch (information.m_dmrmodus)
				{
				case information.DMRMODUS.XLXDMR:
					DMRPlus.connect(downloadHostsTable.searchXLXDMR(this.comboBoxReflectorList.GetItemText(this.comboBoxReflectorList.SelectedItem)), 62030, information.myDMRPlusPassword);
					return;
				case information.DMRMODUS.FREEDMR:
				{
					FreeDMRStruct freeDMRStruct = downloadHostsTable.searchFreeDMR(this.comboBoxReflectorList.GetItemText(this.comboBoxReflectorList.SelectedItem));
					DMRPlus.connect(freeDMRStruct.hostname, freeDMRStruct.port, freeDMRStruct.password);
					return;
				}
				case information.DMRMODUS.SYSTEMX:
				{
					SystemXStruct systemXStruct = downloadHostsTable.searchSystemX(this.comboBoxReflectorList.GetItemText(this.comboBoxReflectorList.SelectedItem));
					DMRPlus.connect(systemXStruct.hostname, systemXStruct.port, systemXStruct.password);
					return;
				}
				case information.DMRMODUS.TGIF:
				{
					TGIFStruct tgifstruct = downloadHostsTable.searchTGIF(this.comboBoxReflectorList.GetItemText(this.comboBoxReflectorList.SelectedItem));
					DMRPlus.connect(tgifstruct.hostname, tgifstruct.port, tgifstruct.password);
					return;
				}
				case information.DMRMODUS.ADNSYSTEMS:
				{
					ADNSYSTEMSStruct adnsystemsstruct = downloadHostsTable.searchADNSYSTEMS(this.comboBoxReflectorList.GetItemText(this.comboBoxReflectorList.SelectedItem));
					DMRPlus.connect(adnsystemsstruct.hostname, adnsystemsstruct.port, adnsystemsstruct.password);
					return;
				}
				default:
					return;
				}
				break;
			case information.MODUS.DSTAR:
			{
				string itemText = this.comboBoxReflectorList.GetItemText(this.comboBoxReflectorList.SelectedItem);
				string itemText2 = this.comboBoxReflectorModule.GetItemText(this.comboBoxReflectorModule.SelectedItem);
				DSTARhandler.connectProcessing(itemText.PadRight(6) + itemText2 + "L");
				return;
			}
			case information.MODUS.FUSION:
				FCSFusion.close();
				YSFFusion.close();
				switch (information.m_fusionmodus)
				{
				case information.FUSIONMODUS.YSF:
					information.fusionHost = JSONCallQuery.lookupFusionHostname(this.comboBoxReflectorList.SelectedIndex);
					information.fusionPort = JSONCallQuery.lookupFusionPort(this.comboBoxReflectorList.SelectedIndex);
					YSFFusion.connect(information.fusionHost, information.fusionPort);
					information.DMRconnected = true;
					return;
				case information.FUSIONMODUS.FCS:
					information.fusionReflector = this.comboBoxReflectorList.SelectedValue.ToString() + this.comboBoxReflectorModule.SelectedValue.ToString();
					FCSFusion.connect(this.comboBoxReflectorList.SelectedValue.ToString() + ".xreflector.net", 62500);
					return;
				case information.FUSIONMODUS.XLXYSF:
					information.fusionReflector = this.comboBoxReflectorList.SelectedValue.ToString();
					information.fusionPort = 42000;
					YSFFusion.connect(downloadHostsTable.searchXLXYSF(this.comboBoxReflectorList.SelectedValue.ToString()), information.fusionPort);
					information.DMRconnected = true;
					return;
				default:
					return;
				}
				break;
			case information.MODUS.NXDN:
			{
				NXDNConnect.close();
				NXDNStruct nxdnstruct = downloadHostsTable.searchNXDN(this.comboBoxReflectorList.GetItemText(this.comboBoxReflectorList.SelectedItem));
				NXDNConnect.connect(nxdnstruct.hostname, nxdnstruct.port, int.Parse(nxdnstruct.ID));
				information.DMRconnected = true;
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x0600029F RID: 671 RVA: 0x0001B7A8 File Offset: 0x000199A8
		private void unlinkButton_Click(object sender, EventArgs e)
		{
			if (this.comboBoxModeSelector.SelectedItem.Equals("DSTAR"))
			{
				information.m_modus = information.MODUS.DSTAR;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("FUSION"))
			{
				information.m_modus = information.MODUS.FUSION;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("DMR"))
			{
				information.m_modus = information.MODUS.DMR;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("NXDN"))
			{
				information.m_modus = information.MODUS.NXDN;
			}
			this.TXprogressBarEx.Value = 0;
			switch (information.m_modus)
			{
			case information.MODUS.DMR:
				DMRPlus.close();
				return;
			case information.MODUS.DSTAR:
				DSTARhandler.connectProcessing("       U");
				return;
			case information.MODUS.FUSION:
				FCSFusion.close();
				YSFFusion.close();
				return;
			case information.MODUS.NXDN:
				NXDNConnect.close();
				return;
			default:
				return;
			}
		}

		// Token: 0x060002A0 RID: 672 RVA: 0x0001B878 File Offset: 0x00019A78
		private void radioButtonREF_CheckedChanged(object sender, EventArgs e)
		{
			if (this.comboBoxModeSelector.SelectedItem.Equals("DSTAR"))
			{
				information.m_modus = information.MODUS.DSTAR;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("FUSION"))
			{
				information.m_modus = information.MODUS.FUSION;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("DMR"))
			{
				information.m_modus = information.MODUS.DMR;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("NXDN"))
			{
				information.m_modus = information.MODUS.NXDN;
			}
			information.MODUS modus = information.m_modus;
			if (modus == information.MODUS.DSTAR)
			{
				Settings.Default.savedReflectorType = "REF";
				Settings.Default.Save();
				this.selectREF();
				return;
			}
			if (modus != information.MODUS.FUSION)
			{
				return;
			}
			this.selectYSF();
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x0001B930 File Offset: 0x00019B30
		private void radioButtonDCS_CheckedChanged(object sender, EventArgs e)
		{
			if (this.comboBoxModeSelector.SelectedItem.Equals("DSTAR"))
			{
				information.m_modus = information.MODUS.DSTAR;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("FUSION"))
			{
				information.m_modus = information.MODUS.FUSION;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("DMR"))
			{
				information.m_modus = information.MODUS.DMR;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("NXDN"))
			{
				information.m_modus = information.MODUS.NXDN;
			}
			information.MODUS modus = information.m_modus;
			if (modus == information.MODUS.DSTAR)
			{
				Settings.Default.savedReflectorType = "DCS";
				Settings.Default.Save();
				this.selectDCS();
				return;
			}
			if (modus != information.MODUS.FUSION)
			{
				return;
			}
			this.selectFCS();
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x0001B9E8 File Offset: 0x00019BE8
		private void radioButtonXRF_CheckedChanged(object sender, EventArgs e)
		{
			Settings.Default.savedReflectorType = "XRF";
			Settings.Default.Save();
			if (this.comboBoxModeSelector.SelectedItem.Equals("DSTAR"))
			{
				information.m_modus = information.MODUS.DSTAR;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("FUSION"))
			{
				information.m_modus = information.MODUS.FUSION;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("DMR"))
			{
				information.m_modus = information.MODUS.DMR;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("NXDN"))
			{
				information.m_modus = information.MODUS.NXDN;
			}
			information.MODUS modus = information.m_modus;
			if (modus == information.MODUS.DSTAR)
			{
				Settings.Default.savedReflectorType = "DCS";
				Settings.Default.Save();
				this.selectXRF();
				return;
			}
			if (modus != information.MODUS.FUSION)
			{
				return;
			}
			this.selectXLXYSF();
		}

		// Token: 0x060002A3 RID: 675 RVA: 0x0001BAB8 File Offset: 0x00019CB8
		private void radioButtonJPN_CheckedChanged(object sender, EventArgs e)
		{
			Settings.Default.savedReflectorType = "JPN";
			Settings.Default.Save();
			if (this.comboBoxModeSelector.SelectedItem.Equals("DSTAR"))
			{
				information.m_modus = information.MODUS.DSTAR;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("FUSION"))
			{
				information.m_modus = information.MODUS.FUSION;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("DMR"))
			{
				information.m_modus = information.MODUS.DMR;
			}
			this.selectJPN();
			if (this.radioButtonJPN.Checked)
			{
				this.XLXURLpictureBox.Visible = true;
				return;
			}
			this.XLXURLpictureBox.Visible = false;
		}

		// Token: 0x060002A4 RID: 676 RVA: 0x0001BB64 File Offset: 0x00019D64
		private void selectXLXDMR()
		{
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorList.DataSource = new BindingSource(downloadHostsTable.returnXLXDMR(), null);
			this.comboBoxReflectorModule.Visible = false;
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x0001BBB4 File Offset: 0x00019DB4
		private void selectFreeDMR()
		{
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorList.DataSource = new BindingSource(downloadHostsTable.returnFreeDMR(), null);
			this.comboBoxReflectorModule.Visible = false;
			this.comboBoxReflectorList.SelectedValue = Settings.Default.savedFREEDMRreflector;
		}

		// Token: 0x060002A6 RID: 678 RVA: 0x0001BC18 File Offset: 0x00019E18
		private void selectSystemX()
		{
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorList.DataSource = new BindingSource(downloadHostsTable.returnSystemX(), null);
			this.comboBoxReflectorModule.Visible = false;
			this.comboBoxReflectorList.SelectedValue = Settings.Default.savedSYSTEMXreflector;
		}

		// Token: 0x060002A7 RID: 679 RVA: 0x0001BC7C File Offset: 0x00019E7C
		private void selectADNSYSTEMS()
		{
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorList.DataSource = new BindingSource(downloadHostsTable.returnADNSYSTEMS(), null);
			this.comboBoxReflectorModule.Visible = false;
			this.comboBoxReflectorList.SelectedValue = Settings.Default.savedADNSYSTEMSreflector;
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x0001BCE0 File Offset: 0x00019EE0
		private void selectTGIF()
		{
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorList.DataSource = new BindingSource(downloadHostsTable.returnTGIF(), null);
			this.comboBoxReflectorModule.Visible = false;
			this.comboBoxReflectorList.SelectedValue = Settings.Default.savedTGIFreflector;
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x0001BD44 File Offset: 0x00019F44
		private void selectREF()
		{
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorList.DataSource = new BindingSource(downloadHostsTable.returnREF(), null);
			this.comboBoxReflectorList.SelectedValue = Settings.Default.savedREFreflector;
			this.comboBoxReflectorModule.SelectedIndex = 1;
			this.comboBoxReflectorModule.DataSource = new string[]
			{
				"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
				"K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
				"U", "V", "W", "X", "Y", "Z"
			};
			this.comboBoxReflectorModule.SelectedItem = Settings.Default.savedREFreflectorModule;
		}

		// Token: 0x060002AA RID: 682 RVA: 0x0001BEB0 File Offset: 0x0001A0B0
		private void selectDCS()
		{
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorList.DataSource = new BindingSource(downloadHostsTable.returnDCS(), null);
			this.comboBoxReflectorList.SelectedValue = Settings.Default.savedDCSreflector;
			this.comboBoxReflectorModule.DataSource = new string[]
			{
				"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
				"K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
				"U", "V", "W", "X", "Y", "Z"
			};
			this.comboBoxReflectorModule.SelectedItem = Settings.Default.savedDCSreflectorModule;
		}

		// Token: 0x060002AB RID: 683 RVA: 0x0001C010 File Offset: 0x0001A210
		private void selectXRF()
		{
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorList.DataSource = new BindingSource(downloadHostsTable.returnXRF(), null);
			this.comboBoxReflectorList.SelectedValue = Settings.Default.savedXRFreflector;
			this.comboBoxReflectorModule.DataSource = new string[]
			{
				"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
				"K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
				"U", "V", "W", "X", "Y", "Z"
			};
			this.comboBoxReflectorModule.SelectedItem = Settings.Default.savedXRFreflectorModule;
		}

		// Token: 0x060002AC RID: 684 RVA: 0x0001C170 File Offset: 0x0001A370
		private void selectJPN()
		{
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorList.DataSource = new BindingSource(downloadHostsTable.returnJPN(), null);
			this.comboBoxReflectorList.SelectedValue = Settings.Default.savedJPNreflector;
			if (this.comboBoxReflectorList.SelectedIndex == -1 && downloadHostsTable.returnJPN().Count > 1)
			{
				this.comboBoxReflectorList.SelectedIndex = 0;
			}
			this.comboBoxReflectorModule.DataSource = new string[]
			{
				"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
				"K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
				"U", "V", "W", "X", "Y", "Z"
			};
			this.comboBoxReflectorModule.SelectedItem = Settings.Default.savedJPNreflectorModule;
		}

		// Token: 0x060002AD RID: 685 RVA: 0x0001C2F8 File Offset: 0x0001A4F8
		private void selectXLXDSTAR()
		{
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorList.DataSource = new BindingSource(downloadHostsTable.returnXLXDSTAR(), null);
			this.comboBoxReflectorList.SelectedValue = Settings.Default.savedXLXreflector;
			this.comboBoxReflectorModule.DataSource = new string[]
			{
				"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
				"K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
				"U", "V", "W", "X", "Y", "Z"
			};
			this.comboBoxReflectorModule.SelectedItem = Settings.Default.savedXLXreflectorModule;
		}

		// Token: 0x060002AE RID: 686 RVA: 0x0001C458 File Offset: 0x0001A658
		private void selectFCS()
		{
			information.m_fusionmodus = information.FUSIONMODUS.FCS;
			this.comboBoxReflectorModule.Visible = true;
			this.comboBoxReflectorList.DataSource = new BindingSource(downloadHostsTable.returnFCS(), null);
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorModule.SelectedIndex = 1;
			string[] array = new string[100];
			for (int i = 0; i <= 99; i++)
			{
				array[i] = i.ToString("D2");
			}
			this.comboBoxReflectorModule.DataSource = array;
		}

		// Token: 0x060002AF RID: 687 RVA: 0x0001C4EC File Offset: 0x0001A6EC
		private void selectYSF()
		{
			information.m_fusionmodus = information.FUSIONMODUS.YSF;
			this.comboBoxReflectorModule.Hide();
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorList.DataSource = JSONCallQuery.getYSFMasterList();
			this.comboBoxReflectorList.SelectedItem = Settings.Default.savedYSFreflector;
		}

		// Token: 0x060002B0 RID: 688 RVA: 0x0001C550 File Offset: 0x0001A750
		private void selectXLXYSF()
		{
			information.m_fusionmodus = information.FUSIONMODUS.XLXYSF;
			this.comboBoxReflectorModule.Hide();
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorList.DataSource = downloadHostsTable.returnXLXYSF();
			this.comboBoxReflectorModule.SelectedIndex = 1;
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x0001C5AC File Offset: 0x0001A7AC
		private void selectNXDN()
		{
			this.comboBoxReflectorModule.Hide();
			this.comboBoxReflectorList.DataSource = null;
			this.comboBoxReflectorList.DisplayMember = "Key";
			this.comboBoxReflectorList.ValueMember = "Key";
			this.comboBoxReflectorList.DataSource = new BindingSource(downloadHostsTable.returnNXDN(), null);
			this.comboBoxReflectorList.SelectedValue = Settings.Default.savedNXDNreflector;
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x0001C61B File Offset: 0x0001A81B
		private void reflectorListKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
			{
				e.KeyChar -= ' ';
			}
		}

		// Token: 0x060002B3 RID: 691 RVA: 0x00002F42 File Offset: 0x00001142
		private void downloadDSTARHostsToolStripMenuItem_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x0001C641 File Offset: 0x0001A841
		private void Click_Chat_Picture(object sender, EventArgs e)
		{
			this.tabControl1.SelectedTab = this.APRSchatTab;
			this.chatImage.Visible = false;
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x0001C660 File Offset: 0x0001A860
		private void comboBoxModeSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.comboBoxModeSelector.SelectedItem == null)
			{
				return;
			}
			string text = this.comboBoxModeSelector.SelectedItem.ToString();
			if (!(text == "DMR"))
			{
				if (text == "DSTAR")
				{
					information.m_modus = information.MODUS.DSTAR;
					this.selectREF();
					this.NXDN_screen(false);
					this.DSTAR_screen(true);
					return;
				}
				if (text == "FUSION")
				{
					information.m_modus = information.MODUS.FUSION;
					this.selectYSF();
					this.DSTAR_screen(false);
					this.NXDN_screen(false);
					this.Fusion_screen(true);
					return;
				}
				if (!(text == "NXDN"))
				{
					return;
				}
				information.m_modus = information.MODUS.NXDN;
				this.selectNXDN();
				this.DSTAR_screen(false);
				this.Fusion_screen(false);
				this.NXDN_screen(true);
			}
			else
			{
				information.m_modus = information.MODUS.DMR;
				this.DSTAR_screen(false);
				if (information.m_dmrmodus == information.DMRMODUS.XLXDMR)
				{
					this.selectXLXDMR();
					this.XLXDMR_screen(true);
					return;
				}
				if (information.m_dmrmodus == information.DMRMODUS.FREEDMR)
				{
					this.selectFreeDMR();
					this.FreeDMR_screen(true);
					return;
				}
				if (information.m_dmrmodus == information.DMRMODUS.SYSTEMX)
				{
					this.selectSystemX();
					this.SystemX_screen(true);
					return;
				}
				if (information.m_dmrmodus == information.DMRMODUS.TGIF)
				{
					this.selectTGIF();
					this.TGIF_screen(true);
					return;
				}
				if (information.m_dmrmodus == information.DMRMODUS.ADNSYSTEMS)
				{
					this.selectADNSYSTEMS();
					this.ADNSYSTEMS_screen(true);
					return;
				}
			}
		}

		// Token: 0x060002B6 RID: 694 RVA: 0x0001C7A3 File Offset: 0x0001A9A3
		private void DMRPanel_DoubleClick(object sender, EventArgs e)
		{
			information.stream_modus = information.MODUS.DMR;
			modeTimer.setMode(information.MODUS.DMR);
			modeTimer.resetTimer();
			DVMEGASerial.setmode2DMR();
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x0001C7BB File Offset: 0x0001A9BB
		private void DSTARPanel_DoubleClick(object sender, EventArgs e)
		{
			information.stream_modus = information.MODUS.DSTAR;
			modeTimer.setMode(information.MODUS.DSTAR);
			modeTimer.resetTimer();
			DVMEGASerial.setmode2DSTAR();
		}

		// Token: 0x060002B8 RID: 696 RVA: 0x0001C7D3 File Offset: 0x0001A9D3
		private void FusionPanel_DoubleClick(object sender, EventArgs e)
		{
			information.stream_modus = information.MODUS.FUSION;
			modeTimer.setMode(information.MODUS.FUSION);
			modeTimer.resetTimer();
			DVMEGASerial.setmode2Fusion();
		}

		// Token: 0x060002B9 RID: 697 RVA: 0x0001C7EB File Offset: 0x0001A9EB
		private void NXDNPanel_DoubleClick(object sender, EventArgs e)
		{
			information.stream_modus = information.MODUS.NXDN;
			modeTimer.setMode(information.MODUS.NXDN);
			modeTimer.resetTimer();
			DVMEGASerial.setmode2NXDN();
		}

		// Token: 0x060002BA RID: 698 RVA: 0x0001C804 File Offset: 0x0001AA04
		private void saveLastSavedKANWEG()
		{
			bool flag = false;
			using (IEnumerator enumerator = this.DMRManualDialComboBox1.Items.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (((string)enumerator.Current).Trim().Equals(this.DMRManualDialComboBox1.Text.Trim()))
					{
						flag = true;
					}
				}
			}
			if (!flag && this.DMRManualDialComboBox1.Text.Trim().Length > 0)
			{
				this.DMRManualDialComboBox1.Items.Add(this.DMRManualDialComboBox1.Text);
			}
			if (this.DMRManualDialComboBox1.Items.Count > 8)
			{
				this.DMRManualDialComboBox1.Items.RemoveAt(1);
			}
			if (!this.DMRManualDialComboBox1.Items.Contains("4000"))
			{
				this.DMRManualDialComboBox1.Items.Insert(0, "4000");
			}
			Settings.Default.savedDMRManualDialCollection.Clear();
			foreach (object obj in this.DMRManualDialComboBox1.Items)
			{
				Settings.Default.savedDMRManualDialCollection.Add(obj.ToString());
			}
			Settings.Default.Save();
		}

		// Token: 0x060002BB RID: 699 RVA: 0x0001C974 File Offset: 0x0001AB74
		private bool OnAirFunction()
		{
			if ((TimerRXTX.status == TimerRXTX.STATUS.LISTENING && information.m_device == information.DEVICE.DV3000R) || (TimerRXTX.status == TimerRXTX.STATUS.RX && information.m_device == information.DEVICE.DV3000R))
			{
				this.VUMeter.MaxValue = 80f;
				if (this.onAIRswitch.Checked)
				{
					if (information.AMBEBeep && this.VOXcheckBox.Checked)
					{
						soundcard.beep(750.0);
					}
					if (information.setAMBEMode == information.MODUS.DSTAR)
					{
						DVMEGAAMBE.setVolume((byte)information.DSTARinGain, (byte)information.DSTARoutGain);
						DVMEGAAMBE.setAMBEDSTAR();
						SlowData.make_free_text(information.DSTARslowDataText);
						DVMEGAAMBE.makeDSTARHeaderFrameFromAMBE();
					}
					else if (information.setAMBEMode == information.MODUS.DMR)
					{
						DVMEGAAMBE.setVolume((byte)information.DMRinGain, (byte)information.DMRoutGain);
						DVMEGAAMBE.setAMBEDMR();
						if (this.DMRManualDialComboBox1.Text.Length > 0)
						{
							information.myAMBEDstDMRID = this.DMRManualDialComboBox1.Text;
						}
						else
						{
							information.myAMBEDstDMRID = "9";
						}
						int num;
						if (!int.TryParse(information.myAMBEDstDMRID, out num))
						{
							MessageBox.Show("ERROR: Use only numbers as Destination ID!");
							information.myAMBEDstDMRID = "9";
						}
						if (this.toggleSwitchGroupPrivate.Checked)
						{
							information.m_groupprivate = information.GROUPPRIVATE.PRIVATE;
						}
						else
						{
							information.m_groupprivate = information.GROUPPRIVATE.GROUP;
						}
						DVMEGASerial.createNewDMRSessionID();
						DVMEGAAMBE.generateBPTC();
					}
					else if (information.setAMBEMode == information.MODUS.FUSION)
					{
						this.DGIDComboBox.Enabled = false;
						DVMEGAAMBE.setVolume((byte)information.FUSIONinGain, (byte)information.FUSIONoutGain);
						DVMEGAAMBE.setAMBEC4FM();
						fusion_extract.make_fusion_header();
						for (int i = 0; i < 6; i++)
						{
							fusion_extract.fill_DCH_VD2(i);
							fusion_extract.fill_DCH_VD2(6);
						}
					}
					else if (information.setAMBEMode == information.MODUS.NXDN)
					{
						this.DGIDComboBox.Enabled = false;
						DVMEGAAMBE.setVolume((byte)information.NXDNinGain, (byte)information.NXDNoutGain);
						DVMEGAAMBE.setAMBENXDN();
					}
					killTimer.start();
					this.saveLastSavedKANWEG();
					soundcard.recordNow(true);
				}
				else
				{
					killTimer.stop();
					soundcard.recordNow(false);
					this.VUMeter.Value = 0f;
					switch (information.setAMBEMode)
					{
					case information.MODUS.DSTAR:
						DVMEGAAMBE.makeDstarEndMMDVM();
						break;
					case information.MODUS.FUSION:
						this.DGIDComboBox.Enabled = true;
						DVMEGAAMBE.makeC4FMVoiceFrameFromAMBE(fusion_extract.C4fM_FRAME_END, true);
						break;
					}
					if (information.AMBEBeep && this.VOXcheckBox.Checked)
					{
						soundcard.beep(550.0);
					}
				}
			}
			else
			{
				this.onAIRswitch.Checked = false;
			}
			return true;
		}

		// Token: 0x060002BC RID: 700 RVA: 0x0001CBC3 File Offset: 0x0001ADC3
		private void onAIR(object sender, EventArgs e)
		{
			this.OnAirFunction();
		}

		// Token: 0x060002BD RID: 701 RVA: 0x00002F42 File Offset: 0x00001142
		private void downloadCallInfoToolStripMenuItem_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x060002BE RID: 702 RVA: 0x0001CBCC File Offset: 0x0001ADCC
		private void updateDSTARHostsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult dialogResult;
			switch (information.m_language)
			{
			case information.LANGUAGE.ENGLISH:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your DSTAR/Fusion hosts files.", "Download DSTAR hosts files", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.JAPANESE:
				dialogResult = MessageBox.Show("DSTAR hostsファイルをダウンロードして上書きしますか？", "DSTAR/Fusion hostsファイルをダウンロードする", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.CHINEES:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your DSTAR/Fusion hosts files.", "Download DSTAR hosts files", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.KOREAN:
				dialogResult = MessageBox.Show("DSTAR 호스트파일 다운로드를 하겠습니까", "DSTAR/Fusion 호스트파일 다운로드", MessageBoxButtons.YesNo);
				break;
			default:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your DSTAR/Fusion hosts files.", "Download DSTAR hosts files", MessageBoxButtons.YesNo);
				break;
			}
			if (dialogResult != DialogResult.Yes)
			{
				return;
			}
			if (utils.RunningPlatform() == utils.Platform.Windows)
			{
				FileDownloader.DownloadFile("http://www.arrg.us/HF/DExtra_Hosts.txt", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\DExtra_Hosts.txt", 10000, "DExtra_Hosts");
				FileDownloader.DownloadFile("http://www.arrg.us/HF/DCS_Hosts.txt", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\DCS_Hosts.txt", 10000, "DCS_Hosts");
				FileDownloader.DownloadFile("http://www.arrg.us/HF/DPlus_Hosts.txt", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\DPlus_Hosts.txt", 10000, "DPlus_Hosts");
				FileDownloader.DownloadFile("http://jq1ztn.mydns.jp/repeater/NExcon_hosts.txt", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\JPN_Hosts.txt", 10000, "JPN_Hosts");
				FileDownloader.DownloadFile("http://xlxapi.rlx.lu/api.php?do=GetReflectors", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\XLX_Hosts.txt", 10000, "XLX_Hosts");
				FileDownloader.DownloadFile("http://xlxapi.rlx.lu/api.php?do=GetXLXYSFMaster", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\XLXYSF_Hosts.txt", 10000, "XLXYSF_Hosts");
				return;
			}
			FileDownloader.DownloadFile("http://www.arrg.us/HF/DExtra_Hosts.txt", "DExtra_Hosts.txt", 10000, "DExtra_Hosts");
			FileDownloader.DownloadFile("http://www.arrg.us/HF/DCS_Hosts.txt", "DCS_Hosts.txt", 10000, "DCS_Hosts");
			FileDownloader.DownloadFile("http://www.arrg.us/HF/DPlus_Hosts.txt", "DPlus_Hosts.txt", 10000, "DPlus_Hosts");
			FileDownloader.DownloadFile("http://jq1ztn.mydns.jp/repeater/NExcon_hosts.txt", "JPN_Hosts.txt", 10000, "JPN_Hosts");
			FileDownloader.DownloadFile("http://xlxapi.rlx.lu/api.php?do=GetReflectors", "XLX_Hosts.txt", 10000, "XLX_Hosts");
			FileDownloader.DownloadFile("http://xlxapi.rlx.lu/api.php?do=GetXLXYSFMaster", "XLXYSF_Hosts.txt", 10000, "XLXYSF_Hosts");
		}

		// Token: 0x060002BF RID: 703 RVA: 0x0001CDE8 File Offset: 0x0001AFE8
		private void updateCallDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult dialogResult;
			switch (information.m_language)
			{
			case information.LANGUAGE.ENGLISH:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your User database.", "Download User database", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.JAPANESE:
				dialogResult = MessageBox.Show("ユーザーデータベースがダウンロードされました", "ユーザーデータベースをダウンロード", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.CHINEES:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your User database.", "Download User database", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.KOREAN:
				dialogResult = MessageBox.Show("사용자정보를 다운로드해서 갱신하겠습니까", "사용자정보 다운로드", MessageBoxButtons.YesNo);
				break;
			default:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your User database.", "Download User database", MessageBoxButtons.YesNo);
				break;
			}
			if (dialogResult != DialogResult.Yes)
			{
				return;
			}
			if (utils.RunningPlatform() == utils.Platform.Windows)
			{
				FileDownloader.DownloadFile("https://radioid.net/static/user.csv", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\user.csv", 10000, "DMR User database downloaded");
				FileDownloader.DownloadFile("https://radioid.net/static/nxdn.csv", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\nxdn.csv", 10000, "NXDN User database downloaded");
				return;
			}
			FileDownloader.DownloadFile("https://radioid.net/static/user.csv", "user.csv", 10000, "DMR User database downloaded");
			FileDownloader.DownloadFile("https://radioid.net/static/nxdn.csv", "nxdn.csv", 10000, "NXDN User database downloaded");
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0001CF04 File Offset: 0x0001B104
		private void updateFusionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult dialogResult;
			switch (information.m_language)
			{
			case information.LANGUAGE.ENGLISH:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your Fusion database.", "Download Fusion master database", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.JAPANESE:
				dialogResult = MessageBox.Show("Fusionマスターをダウンロードして上書きしますか？", "Fusionマスターデータベースをダウンロード", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.CHINEES:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your Fusion database.", "Download Fusion database", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.KOREAN:
				dialogResult = MessageBox.Show("Fusion의 자료를 다운로드해서 갱신하겠습니까", "Fusion 자료 다운로드", MessageBoxButtons.YesNo);
				break;
			default:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your Fusion database.", "Download Fusion database", MessageBoxButtons.YesNo);
				break;
			}
			if (dialogResult != DialogResult.Yes)
			{
				return;
			}
			if (utils.RunningPlatform() == utils.Platform.Windows)
			{
				FileDownloader.DownloadFile("http://fcsdata.xreflector.es/database/fcs_masters.txt", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\fcs_masters.txt", 10000, "Fusion master database");
				return;
			}
			FileDownloader.DownloadFile("http://fcsdata.xreflector.es/database/fcs_masters.txt", "fcs_masters.txt", 10000, "Fusion master database");
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0001CFE0 File Offset: 0x0001B1E0
		private void updateBMMastersToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult dialogResult;
			switch (information.m_language)
			{
			case information.LANGUAGE.ENGLISH:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your Brandmeister master, XLX, FreeDMR database.", "Download Brandmeister, XLX, FreeDMR master database", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.JAPANESE:
				dialogResult = MessageBox.Show("BrandmeisterマスターとXLX DMRデータベースをダウンロードして上書きしますか？", "Brandmeister＆XLX DMRマスターデータベースをダウンロード", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.CHINEES:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your Brandmeister master AND XLX DMR database.", "Download Brandmeister & XLX DMR master database", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.KOREAN:
				dialogResult = MessageBox.Show("BM과 XLX DMR의 자료를 다운로드해서 갱신하겠습니까", "BM과 XLX DMR 자료 다운로드", MessageBoxButtons.YesNo);
				break;
			default:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your Brandmeister master, XLX, FreeDMR database.", "Download Brandmeister, XLX, FreeDMR master database", MessageBoxButtons.YesNo);
				break;
			}
			if (dialogResult != DialogResult.Yes)
			{
				return;
			}
			FileDownloader.DownloadFile("https://api.brandmeister.network/v2/master", "BMhosts.txt", 10000, "Brandmeister master database");
			if (utils.RunningPlatform() == utils.Platform.Windows)
			{
				FileDownloader.DownloadFile("http://xlxapi.rlx.lu/api.php?do=GetXLXDMRMaster", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\xlx_DMRMaster.txt", 10000, "XLX DMR master database");
			}
			else
			{
				FileDownloader.DownloadFile("http://xlxapi.rlx.lu/api.php?do=GetXLXDMRMaster", "xlx_DMRMaster.txt", 10000, "XLX DMR master database");
			}
			if (utils.RunningPlatform() == utils.Platform.Windows)
			{
				FileDownloader.DownloadFile("http://downloads.freedmr.uk/downloads/FreeDMR_Hosts.txt", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\FreeDMR_Hosts.txt", 10000, "FreeDMR master database");
			}
			else
			{
				FileDownloader.DownloadFile("http://downloads.freedmr.uk/downloads/FreeDMR_Hosts.txt", "FreeDMR_Hosts.txt", 10000, "FreeDMR master database");
			}
			if (utils.RunningPlatform() == utils.Platform.Windows)
			{
				FileDownloader.DownloadFile("https://freestar.network/downloads/SystemX_Hosts.txt", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\SystemX_Hosts.txt", 10000, "SystemX master database");
			}
			else
			{
				FileDownloader.DownloadFile("https://freestar.network/downloads/SystemX_Hosts.txt", "SystemX_Hosts.txt", 10000, "SystemX master database");
			}
			if (utils.RunningPlatform() == utils.Platform.Windows)
			{
				FileDownloader.DownloadFile("https://adn.systems/servers/adn-servers.csv", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\ADNSYSTEMS_Hosts.txt", 10000, "ADNSYSTEMS master database");
				return;
			}
			FileDownloader.DownloadFile("https://adn.systems/servers/adn-servers.csv", "ADNSYSTEMS_Hosts.txt", 10000, "adnsystems master database");
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x0001D1AF File Offset: 0x0001B3AF
		private void soundInputToolStripMenuItem_SelectedIndexChanged(object sender, EventArgs e)
		{
			soundcard.selectRecordingDevice(this.soundInputToolStripMenuItem.SelectedIndex);
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x0001D1C1 File Offset: 0x0001B3C1
		private void soundOutputToolStripMenuItem_SelectedIndexChanged(object sender, EventArgs e)
		{
			soundcard.selectPlayDevice(this.soundOutputToolStripMenuItem.SelectedIndex);
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x0001D1D4 File Offset: 0x0001B3D4
		private void aMBEToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.soundOutputToolStripMenuItem.Items.Clear();
			foreach (string text in soundcard.listPlayDevices())
			{
				this.soundOutputToolStripMenuItem.Items.Add(text);
			}
			this.soundInputToolStripMenuItem.Items.Clear();
			foreach (string text2 in soundcard.listRecordingDevices())
			{
				this.soundInputToolStripMenuItem.Items.Add(text2);
			}
			try
			{
				this.soundInputToolStripMenuItem.SelectedIndex = soundcard.selectedRecordingDevice();
				this.soundOutputToolStripMenuItem.SelectedIndex = soundcard.selectedPlayDevice();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x0001D2DC File Offset: 0x0001B4DC
		private void comboBoxReflectorList_SelectionChangeCommitted(object sender, EventArgs e)
		{
			switch (information.m_modus)
			{
			case information.MODUS.DMR:
				switch (information.m_dmrmodus)
				{
				case information.DMRMODUS.FREEDMR:
					Settings.Default.savedFREEDMRreflector = this.comboBoxReflectorList.SelectedValue.ToString();
					Settings.Default.Save();
					return;
				case information.DMRMODUS.SYSTEMX:
					Settings.Default.savedSYSTEMXreflector = this.comboBoxReflectorList.SelectedValue.ToString();
					Settings.Default.Save();
					return;
				case information.DMRMODUS.TGIF:
					Settings.Default.savedTGIFreflector = this.comboBoxReflectorList.SelectedValue.ToString();
					Settings.Default.Save();
					return;
				case information.DMRMODUS.ADNSYSTEMS:
					Settings.Default.savedADNSYSTEMSreflector = this.comboBoxReflectorList.SelectedValue.ToString();
					Settings.Default.Save();
					break;
				default:
					return;
				}
				break;
			case information.MODUS.DSTAR:
				if (this.radioButtonREF.Checked)
				{
					Settings.Default.savedREFreflector = this.comboBoxReflectorList.SelectedValue.ToString();
					Settings.Default.savedREFreflectorModule = this.comboBoxReflectorModule.SelectedValue.ToString();
					Settings.Default.Save();
				}
				if (this.radioButtonDCS.Checked)
				{
					Settings.Default.savedDCSreflector = this.comboBoxReflectorList.SelectedValue.ToString();
					Settings.Default.savedDCSreflectorModule = this.comboBoxReflectorModule.SelectedValue.ToString();
					Settings.Default.Save();
				}
				if (this.radioButtonXRF.Checked)
				{
					Settings.Default.savedXRFreflector = this.comboBoxReflectorList.SelectedValue.ToString();
					Settings.Default.savedXRFreflectorModule = this.comboBoxReflectorModule.SelectedValue.ToString();
					Settings.Default.Save();
				}
				if (this.radioButtonJPN.Checked)
				{
					Settings.Default.savedJPNreflector = this.comboBoxReflectorList.SelectedValue.ToString();
					Settings.Default.savedJPNreflectorModule = this.comboBoxReflectorModule.SelectedValue.ToString();
					Settings.Default.Save();
				}
				if (this.radioButtonXLX.Checked)
				{
					Settings.Default.savedXLXreflector = this.comboBoxReflectorList.SelectedValue.ToString();
					Settings.Default.savedXLXreflectorModule = this.comboBoxReflectorModule.SelectedValue.ToString();
					Settings.Default.Save();
					return;
				}
				break;
			case information.MODUS.FUSION:
				if (this.radioButtonREF.Checked)
				{
					Settings.Default.savedYSFreflector = this.comboBoxReflectorList.SelectedValue.ToString();
					Settings.Default.Save();
					return;
				}
				break;
			case information.MODUS.NXDN:
				Settings.Default.savedNXDNreflector = this.comboBoxReflectorList.SelectedValue.ToString();
				Settings.Default.Save();
				return;
			default:
				return;
			}
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x0001D58F File Offset: 0x0001B78F
		private void DMRmanualDial_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x0001D5B8 File Offset: 0x0001B7B8
		private void toggleSwitchGroupPrivate_CheckedChanged(object sender, EventArgs e)
		{
			if (this.toggleSwitchGroupPrivate.Checked)
			{
				information.m_groupprivate = information.GROUPPRIVATE.PRIVATE;
				information.m_groupprivateSelected = information.GROUPPRIVATE.PRIVATE;
			}
			else
			{
				information.m_groupprivate = information.GROUPPRIVATE.GROUP;
				information.m_groupprivateSelected = information.GROUPPRIVATE.GROUP;
			}
			Settings.Default.savedprivateorGroupswitch = this.toggleSwitchGroupPrivate.Checked;
			Settings.Default.Save();
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x0001D60C File Offset: 0x0001B80C
		private void DMRmanualDial_TextChanged(object sender, EventArgs e)
		{
			information.myAMBEDstDMRIDinput = this.DMRManualDialComboBox1.Text;
			Settings.Default.savedDMRmanualDial = this.DMRManualDialComboBox1.Text;
			Settings.Default.Save();
			string text = TGLookup.lookup(this.DMRManualDialComboBox1.Text);
			if (text != null && information.m_dmrmodus == information.DMRMODUS.BM)
			{
				this.TGLookupLabel.Text = text;
				return;
			}
			this.TGLookupLabel.Text = "";
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x0001D684 File Offset: 0x0001B884
		private void VOXcheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (this.VOXcheckBox.Checked)
			{
				AMBE_VOX.start();
				information.onAIRswitchBool = true;
				if (this.VOXcheckBox.Enabled)
				{
					soundcard.record();
					return;
				}
			}
			else
			{
				AMBE_VOX.stop();
				information.onAIRswitchBool = false;
				if (this.VOXcheckBox.Enabled)
				{
					soundcard.stoprecording();
				}
			}
		}

		// Token: 0x060002CA RID: 714 RVA: 0x0001D6DC File Offset: 0x0001B8DC
		private void trackBar1_Scroll_1(object sender, EventArgs e)
		{
			AMBE_VOX.setDetectionLevel(this.VOXLevelTrackBar.Value);
			information.VOXDetectionLevel = this.VOXLevelTrackBar.Value;
			this.VOXlabel.Text = (this.VOXLevelTrackBar.Value / 100).ToString();
			Settings.Default.savedVOXDetectionLevel = this.VOXLevelTrackBar.Value;
			Settings.Default.Save();
		}

		// Token: 0x060002CB RID: 715 RVA: 0x0001D74C File Offset: 0x0001B94C
		private void DSTARouttrackBar_Scroll(object sender, EventArgs e)
		{
			this.DSTARoutlabel.Text = this.DSTARouttrackBar.Value.ToString();
			information.DSTARoutGain = this.DSTARouttrackBar.Value;
			if (information.stream_modus == information.MODUS.DSTAR)
			{
				DVMEGAAMBE.setVolume((byte)this.DSTARintrackBar.Value, (byte)this.DSTARouttrackBar.Value);
			}
			Settings.Default.savedDSTARoutGain = this.DSTARouttrackBar.Value;
			Settings.Default.Save();
		}

		// Token: 0x060002CC RID: 716 RVA: 0x0001D7CC File Offset: 0x0001B9CC
		private void DSTARintrackBar_Scroll(object sender, EventArgs e)
		{
			this.DSTARinlabel.Text = this.DSTARintrackBar.Value.ToString();
			information.DSTARinGain = this.DSTARintrackBar.Value;
			if (information.stream_modus == information.MODUS.DSTAR)
			{
				DVMEGAAMBE.setVolume((byte)this.DSTARintrackBar.Value, (byte)this.DSTARouttrackBar.Value);
			}
			Settings.Default.savedDSTARinGain = this.DSTARintrackBar.Value;
			Settings.Default.Save();
		}

		// Token: 0x060002CD RID: 717 RVA: 0x0001D84C File Offset: 0x0001BA4C
		private void DMRouttrackBar_Scroll(object sender, EventArgs e)
		{
			this.DMRoutlabel.Text = this.DMRouttrackBar.Value.ToString();
			information.DMRoutGain = this.DMRouttrackBar.Value;
			if (information.stream_modus == information.MODUS.DMR)
			{
				DVMEGAAMBE.setVolume((byte)this.DMRintrackBar.Value, (byte)this.DMRouttrackBar.Value);
			}
			Settings.Default.savedDMRoutGain = this.DMRouttrackBar.Value;
			Settings.Default.Save();
		}

		// Token: 0x060002CE RID: 718 RVA: 0x0001D8CC File Offset: 0x0001BACC
		private void DMRintrackBar_Scroll(object sender, EventArgs e)
		{
			this.DMRinlabel.Text = this.DMRintrackBar.Value.ToString();
			information.DMRinGain = this.DMRintrackBar.Value;
			if (information.stream_modus == information.MODUS.DMR)
			{
				DVMEGAAMBE.setVolume((byte)this.DMRintrackBar.Value, (byte)this.DMRouttrackBar.Value);
			}
			Settings.Default.savedDMRinGain = this.DMRintrackBar.Value;
			Settings.Default.Save();
		}

		// Token: 0x060002CF RID: 719 RVA: 0x0001D94C File Offset: 0x0001BB4C
		private void NXDNouttrackBar_Scroll(object sender, EventArgs e)
		{
			this.NXDNoutlabel.Text = this.NXDNouttrackBar.Value.ToString();
			information.NXDNoutGain = this.NXDNouttrackBar.Value;
			if (information.stream_modus == information.MODUS.NXDN)
			{
				DVMEGAAMBE.setVolume((byte)this.NXDNintrackBar.Value, (byte)this.NXDNouttrackBar.Value);
			}
			Settings.Default.savedNXDNoutGain = this.NXDNouttrackBar.Value;
			Settings.Default.Save();
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x0001D9CC File Offset: 0x0001BBCC
		private void NXDNintrackBar_Scroll(object sender, EventArgs e)
		{
			this.NXDNinlabel.Text = this.NXDNintrackBar.Value.ToString();
			information.NXDNinGain = this.NXDNintrackBar.Value;
			if (information.stream_modus == information.MODUS.NXDN)
			{
				DVMEGAAMBE.setVolume((byte)this.NXDNintrackBar.Value, (byte)this.NXDNouttrackBar.Value);
			}
			Settings.Default.savedNXDNinGain = this.NXDNintrackBar.Value;
			Settings.Default.Save();
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x0001DA4C File Offset: 0x0001BC4C
		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			MMDeviceEnumerator mmdeviceEnumerator = new MMDeviceEnumerator();
			mmdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
			MMDevice defaultAudioEndpoint = mmdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
			if (this.checkBox1.Checked)
			{
				defaultAudioEndpoint.AudioEndpointVolume.Mute = true;
				return;
			}
			defaultAudioEndpoint.AudioEndpointVolume.Mute = false;
		}

		// Token: 0x060002D2 RID: 722 RVA: 0x0001DA98 File Offset: 0x0001BC98
		private void helplinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			switch (information.m_language)
			{
			case information.LANGUAGE.ENGLISH:
				Process.Start("http://www.pa7lim.nl/ambetabhelp/");
				return;
			case information.LANGUAGE.JAPANESE:
				Process.Start("http://www.pa7lim.nl/ambetabhelp-ja/");
				return;
			case information.LANGUAGE.CHINEES:
				Process.Start("http://www.pa7lim.nl/ambetabhelp/");
				return;
			case information.LANGUAGE.KOREAN:
				Process.Start("http://www.pa7lim.nl/ambetabhelp/");
				return;
			default:
				Process.Start("http://www.pa7lim.nl/ambetabhelp/");
				return;
			}
		}

		// Token: 0x060002D3 RID: 723 RVA: 0x0001DB00 File Offset: 0x0001BD00
		private void newVersionLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://software.pa7lim.nl/BlueDV/BETA/Windows/");
		}

		// Token: 0x060002D4 RID: 724 RVA: 0x0001DB10 File Offset: 0x0001BD10
		private void TGlistView_Click(object sender, EventArgs e)
		{
			this.DMRManualDialComboBox1.Text = this.TGlistView.SelectedItems[0].Text;
			this.DMRAMBEButton.PerformClick();
			this.toggleSwitchGroupPrivate.Checked = false;
			this.tabControl1.SelectTab(1);
		}

		// Token: 0x060002D5 RID: 725 RVA: 0x0001DB64 File Offset: 0x0001BD64
		private void SearchTextBox_TextChanged(object sender, EventArgs e)
		{
			this.TGlistView.Items.Clear();
			if (this.SearchTextBox.Text.Length == 0)
			{
				if (TGLookup.schema == null)
				{
					return;
				}
				using (Dictionary<string, string>.Enumerator enumerator = TGLookup.schema.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, string> keyValuePair = enumerator.Current;
						ListViewItem listViewItem = new ListViewItem(new string[] { keyValuePair.Key, keyValuePair.Value });
						this.TGlistView.Items.Add(listViewItem);
					}
					return;
				}
			}
			if (TGLookup.schema != null)
			{
				foreach (KeyValuePair<string, string> keyValuePair2 in TGLookup.schema)
				{
					if (keyValuePair2.Key.ToString().ToUpper().Contains(this.SearchTextBox.Text.ToUpper()) || keyValuePair2.Value.ToString().ToUpper().Contains(this.SearchTextBox.Text.ToUpper()))
					{
						ListViewItem listViewItem2 = new ListViewItem(new string[] { keyValuePair2.Key, keyValuePair2.Value });
						this.TGlistView.Items.Add(listViewItem2);
					}
				}
			}
		}

		// Token: 0x060002D6 RID: 726 RVA: 0x0001DCE0 File Offset: 0x0001BEE0
		private void CallLookupTextBox_TextChanged(object sender, EventArgs e)
		{
			if (this.CallLookupTextBox.Text.Length > 2)
			{
				this.CallookupListView.Items.Clear();
				foreach (JSONCallQuery.callClass callClass in JSONCallQuery.searchCalls(this.CallLookupTextBox.Text))
				{
					ListViewItem listViewItem = new ListViewItem(new string[] { callClass.call, callClass.name, callClass.dmrID });
					this.CallookupListView.Items.Add(listViewItem);
				}
			}
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x0001DD98 File Offset: 0x0001BF98
		private void CallLookupResult_Click(object sender, EventArgs e)
		{
			if (!this.jsonlookupCall[1].Equals("Unknown"))
			{
				this.DMRManualDialComboBox1.Text = this.jsonlookupCall[2];
				this.DMRAMBEButton.PerformClick();
				this.toggleSwitchGroupPrivate.Checked = true;
				this.tabControl1.SelectTab(1);
			}
		}

		// Token: 0x060002D8 RID: 728 RVA: 0x0001DDF0 File Offset: 0x0001BFF0
		private void CallookupListView_Click(object sender, EventArgs e)
		{
			this.DMRManualDialComboBox1.Text = this.CallookupListView.SelectedItems[0].SubItems[2].Text;
			this.DMRAMBEButton.PerformClick();
			this.toggleSwitchGroupPrivate.Checked = true;
			this.tabControl1.SelectTab(1);
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x0001DE4C File Offset: 0x0001C04C
		private void listView1_DoubleClick(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection selectedItems = this.listView1.SelectedItems;
			string text = "";
			foreach (object obj in selectedItems)
			{
				text = ((ListViewItem)obj).SubItems[1].Text;
			}
			this.CallLookupTextBox.Text = text;
			this.tabControl1.SelectTab(2);
		}

		// Token: 0x060002DA RID: 730 RVA: 0x0001DED4 File Offset: 0x0001C0D4
		private void simpleModeCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Settings.Default.savedDMRsimpleMode = this.simpleModeCheckBox.Checked;
			Settings.Default.Save();
			if (this.simpleModeCheckBox.Checked)
			{
				this.DMRManualDialComboBox1.Enabled = true;
				this.toggleSwitchGroupPrivate.Enabled = true;
				information.DMRsimpleMode = true;
				return;
			}
			information.DMRsimpleMode = false;
			this.DMRManualDialComboBox1.Enabled = false;
			this.toggleSwitchGroupPrivate.Enabled = false;
		}

		// Token: 0x060002DB RID: 731 RVA: 0x0001DF4A File Offset: 0x0001C14A
		public void addLine(string line)
		{
			TextBox chattextBox = this.ChattextBox;
			chattextBox.Text = chattextBox.Text + line + Environment.NewLine;
		}

		// Token: 0x060002DC RID: 732 RVA: 0x0001DF68 File Offset: 0x0001C168
		private void sendAPRSMessage()
		{
			APRSClient.send(string.Concat(new string[]
			{
				information.myCall,
				"-G>APRS,TCPIP*,qAC,SIXTH::",
				this.APRSChatCallComboBox.Text.PadRight(8),
				" :",
				this.MessagetextBox.Text,
				"\n"
			}));
			this.ChattextBox.AppendText(string.Concat(new string[]
			{
				"[ ",
				DateTime.Now.ToString("h:mm tt"),
				" ",
				information.myCall,
				"-G -> ",
				this.APRSChatCallComboBox.Text,
				" ] \r\n"
			}));
			this.ChattextBox.AppendText(this.MessagetextBox.Text + "\r\n");
			this.MessagetextBox.Text = "";
		}

		// Token: 0x060002DD RID: 733 RVA: 0x0001E058 File Offset: 0x0001C258
		private void messageKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return && this.MessagetextBox.Text.Length >= 1)
			{
				this.sendAPRSMessage();
				this.saveLastCallAPRSChat();
			}
		}

		// Token: 0x060002DE RID: 734 RVA: 0x0001E084 File Offset: 0x0001C284
		private void saveLastCallAPRSChat()
		{
			bool flag = false;
			using (IEnumerator enumerator = this.APRSChatCallComboBox.Items.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (((string)enumerator.Current).Trim().Equals(this.APRSChatCallComboBox.Text.Trim()))
					{
						flag = true;
					}
				}
			}
			if (!flag && this.APRSChatCallComboBox.Text.Trim().Length > 0)
			{
				this.APRSChatCallComboBox.Items.Add(this.APRSChatCallComboBox.Text);
			}
			if (this.APRSChatCallComboBox.Items.Count > 8)
			{
				this.APRSChatCallComboBox.Items.RemoveAt(1);
			}
			if (Settings.Default.savedAPRSChatCallsCollection != null)
			{
				Settings.Default.savedAPRSChatCallsCollection.Clear();
			}
			foreach (object obj in this.APRSChatCallComboBox.Items)
			{
				Settings.Default.savedAPRSChatCallsCollection.Add(obj.ToString());
			}
			Settings.Default.Save();
		}

		// Token: 0x060002DF RID: 735 RVA: 0x0001E1D4 File Offset: 0x0001C3D4
		private void messageKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '|')
			{
				e.Handled = true;
			}
			if (e.KeyChar == '~')
			{
				e.Handled = true;
			}
			if (e.KeyChar == '{')
			{
				e.Handled = true;
			}
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x0001E209 File Offset: 0x0001C409
		private void HelplinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://www.pa7lim.nl/bluedv-aprs-chat");
		}

		// Token: 0x060002E1 RID: 737 RVA: 0x0001E216 File Offset: 0x0001C416
		private void ChattextBox_TextChanged(object sender, EventArgs e)
		{
			this.ChattextBox.SelectionStart = this.ChattextBox.Text.Length;
			this.ChattextBox.ScrollToCaret();
			this.ChattextBox.Refresh();
		}

		// Token: 0x060002E2 RID: 738 RVA: 0x00002F42 File Offset: 0x00001142
		private void ChattextBox_MouseDown(object sender, MouseEventArgs e)
		{
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x0001E249 File Offset: 0x0001C449
		private void APRSChatCallComboBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (char.IsLetter(e.KeyChar))
			{
				e.KeyChar = char.ToUpper(e.KeyChar);
			}
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x0001E269 File Offset: 0x0001C469
		private void DMRManualDialComboBox1_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
			{
				e.Handled = true;
			}
		}

		// Token: 0x060002E5 RID: 741 RVA: 0x0001E28C File Offset: 0x0001C48C
		private void radioButtonXLX_CheckedChanged(object sender, EventArgs e)
		{
			if (this.comboBoxModeSelector.SelectedItem.Equals("DSTAR"))
			{
				information.m_modus = information.MODUS.DSTAR;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("FUSION"))
			{
				information.m_modus = information.MODUS.FUSION;
			}
			if (this.comboBoxModeSelector.SelectedItem.Equals("DMR"))
			{
				information.m_modus = information.MODUS.DMR;
			}
			this.selectXLXDSTAR();
			if (this.radioButtonXLX.Checked)
			{
				this.XLXURLpictureBox.Visible = true;
			}
			else
			{
				this.XLXURLpictureBox.Visible = false;
			}
			Settings.Default.savedReflectorType = "XLX";
			Settings.Default.Save();
		}

		// Token: 0x060002E6 RID: 742 RVA: 0x0001E338 File Offset: 0x0001C538
		private void XLXURLpictureBox_Click(object sender, EventArgs e)
		{
			if (this.radioButtonJPN.Checked)
			{
				Process.Start("http://jq1ztn.mydns.jp/repeater/jpn_repeaters.html");
				return;
			}
			if (information.m_dstarmodus != information.DSTARMODUS.JPN && downloadHostsTable.returnXLXDSTARURL().ContainsKey(this.comboBoxReflectorList.SelectedValue.ToString()))
			{
				Process.Start(downloadHostsTable.returnXLXDSTARURL()[this.comboBoxReflectorList.SelectedValue.ToString()]);
			}
		}

		// Token: 0x060002E7 RID: 743 RVA: 0x0001E3A4 File Offset: 0x0001C5A4
		private void FUSIONouttrackBar_Scroll(object sender, EventArgs e)
		{
			this.FUSIONoutlabel.Text = this.FUSIONouttrackBar.Value.ToString();
			information.FUSIONoutGain = this.FUSIONouttrackBar.Value;
			if (information.stream_modus == information.MODUS.FUSION)
			{
				DVMEGAAMBE.setVolume((byte)this.FUSIONintrackBar.Value, (byte)this.FUSIONouttrackBar.Value);
			}
			Settings.Default.savedFUSIONoutGain = this.FUSIONouttrackBar.Value;
			Settings.Default.Save();
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x0001E424 File Offset: 0x0001C624
		private void FUSIONintrackBar_Scroll(object sender, EventArgs e)
		{
			this.FUSIONinlabel.Text = this.FUSIONintrackBar.Value.ToString();
			information.FUSIONinGain = this.FUSIONintrackBar.Value;
			if (information.stream_modus == information.MODUS.FUSION)
			{
				DVMEGAAMBE.setVolume((byte)this.FUSIONintrackBar.Value, (byte)this.FUSIONouttrackBar.Value);
			}
			Settings.Default.savedFUSIONinGain = this.FUSIONintrackBar.Value;
			Settings.Default.Save();
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x0001E4A4 File Offset: 0x0001C6A4
		private void hangTimeTrackBar_Scroll(object sender, EventArgs e)
		{
			this.hangLabel.Text = (this.hangTimeTrackBar.Value / 1000).ToString();
			AMBE_VOX.setDetectionLevel(this.hangTimeTrackBar.Value);
			Settings.Default.savedVOXHangTime = this.hangTimeTrackBar.Value;
			Settings.Default.Save();
		}

		// Token: 0x060002EA RID: 746 RVA: 0x0001E504 File Offset: 0x0001C704
		private void DMRAMBEButton_Click(object sender, EventArgs e)
		{
			information.setAMBEMode = information.MODUS.DMR;
			this.DMRManualDialComboBox1.Enabled = true;
			this.toggleSwitchGroupPrivate.Enabled = true;
			this.DGIDComboBox.Enabled = false;
			this.DMRAMBEpictureBox.Image = Resources.LED_ON;
			this.DSTARAMBEpictureBox.Image = Resources.LED_OFF;
			this.FUSIONAMBEpictureBox.Image = Resources.LED_OFF;
			this.NXDNAMBEpictureBox.Image = Resources.LED_OFF;
		}

		// Token: 0x060002EB RID: 747 RVA: 0x0001E57C File Offset: 0x0001C77C
		private void DSTARAMBEButton_Click(object sender, EventArgs e)
		{
			information.setAMBEMode = information.MODUS.DSTAR;
			this.DMRManualDialComboBox1.Enabled = false;
			this.toggleSwitchGroupPrivate.Enabled = false;
			this.DGIDComboBox.Enabled = false;
			this.DMRAMBEpictureBox.Image = Resources.LED_OFF;
			this.DSTARAMBEpictureBox.Image = Resources.LED_ON;
			this.FUSIONAMBEpictureBox.Image = Resources.LED_OFF;
			this.NXDNAMBEpictureBox.Image = Resources.LED_OFF;
		}

		// Token: 0x060002EC RID: 748 RVA: 0x0001E5F4 File Offset: 0x0001C7F4
		private void FUSIONAMBEButton_Click(object sender, EventArgs e)
		{
			information.setAMBEMode = information.MODUS.FUSION;
			this.DGIDComboBox.Enabled = true;
			this.DMRManualDialComboBox1.Enabled = false;
			this.toggleSwitchGroupPrivate.Enabled = false;
			this.DMRAMBEpictureBox.Image = Resources.LED_OFF;
			this.DSTARAMBEpictureBox.Image = Resources.LED_OFF;
			this.FUSIONAMBEpictureBox.Image = Resources.LED_ON;
			this.NXDNAMBEpictureBox.Image = Resources.LED_OFF;
		}

		// Token: 0x060002ED RID: 749 RVA: 0x0001E66C File Offset: 0x0001C86C
		private void NXDNAMBEButton_Click(object sender, EventArgs e)
		{
			information.setAMBEMode = information.MODUS.NXDN;
			this.DGIDComboBox.Enabled = false;
			this.DMRManualDialComboBox1.Enabled = false;
			this.toggleSwitchGroupPrivate.Enabled = false;
			this.DMRAMBEpictureBox.Image = Resources.LED_OFF;
			this.DSTARAMBEpictureBox.Image = Resources.LED_OFF;
			this.FUSIONAMBEpictureBox.Image = Resources.LED_OFF;
			this.NXDNAMBEpictureBox.Image = Resources.LED_ON;
		}

		// Token: 0x060002EE RID: 750 RVA: 0x0001E6E3 File Offset: 0x0001C8E3
		private void donatePictureBox_Click(object sender, EventArgs e)
		{
			Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=VUMQBXU6KHNJE&source=url");
		}

		// Token: 0x060002EF RID: 751 RVA: 0x0001E6F0 File Offset: 0x0001C8F0
		private void DGIDComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			fusion_extract.changeDGID(this.DGIDComboBox.SelectedIndex);
			Settings.Default.savedDGID = this.DGIDComboBox.SelectedIndex;
			Settings.Default.Save();
		}

		// Token: 0x060002F0 RID: 752 RVA: 0x0001E724 File Offset: 0x0001C924
		private void updateNXDNHostsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult dialogResult;
			switch (information.m_language)
			{
			case information.LANGUAGE.ENGLISH:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your NXDN database.", "Download NXDN master database", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.JAPANESE:
				dialogResult = MessageBox.Show("NXDNマスターをダウンロードして上書きしますか？", "NXDNマスターデータベースをダウンロード", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.CHINEES:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your NXDN database.", "Download NXDN database", MessageBoxButtons.YesNo);
				break;
			case information.LANGUAGE.KOREAN:
				dialogResult = MessageBox.Show("NXDN의 자료를 다운로드해서 갱신하겠습니까", "NXDN 자료 다운로드", MessageBoxButtons.YesNo);
				break;
			default:
				dialogResult = MessageBox.Show("Do you want to download and overwrite your NXDN database.", "Download NXDN database", MessageBoxButtons.YesNo);
				break;
			}
			if (dialogResult != DialogResult.Yes)
			{
				return;
			}
			if (utils.RunningPlatform() == utils.Platform.Windows)
			{
				FileDownloader.DownloadFile("http://hosts.pa7lim.nl/hosts/NXDNHosts.txt", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\BlueDV\\NXDNHosts.txt", 10000, "NXDN master database");
				return;
			}
			FileDownloader.DownloadFile("http://hosts.pa7lim.nl/hosts/NXDNHosts.txt", "NXDNHosts.txt", 10000, "NXDN master database");
		}

		// Token: 0x04000153 RID: 339
		private string[] jsonlookupCall;

		// Token: 0x04000154 RID: 340
		private bool BETA;

		// Token: 0x04000155 RID: 341
		public static bool record;

		// Token: 0x04000156 RID: 342
		private bool ptt;

		// Token: 0x04000157 RID: 343
		public static bool dmrplus;

		// Token: 0x04000158 RID: 344
		private static int maxValueVU;

		// Token: 0x04000159 RID: 345
		private static int VUCounter;

		// Token: 0x0400015A RID: 346
		private string cached_text;

		// Token: 0x0400015B RID: 347
		private Thread JSONCallQueryThread;

		// Token: 0x0400015C RID: 348
		private string oldText = "";

		// Token: 0x0400015D RID: 349
		private string[] chatStringArray;

		// Token: 0x0400015E RID: 350
		private int chatCounter;

		// Token: 0x0400015F RID: 351
		public static WebServer ws;

		// Token: 0x04000160 RID: 352
		public static string tekst;

		// Token: 0x0200005F RID: 95
		// (Invoke) Token: 0x06000643 RID: 1603
		private delegate void SetTextCallback(string text);

		// Token: 0x02000060 RID: 96
		// (Invoke) Token: 0x06000647 RID: 1607
		private delegate void SetDMRTextCallback(string text);

		// Token: 0x02000061 RID: 97
		// (Invoke) Token: 0x0600064B RID: 1611
		private delegate void SetDSTARTextCallback(string text);

		// Token: 0x02000062 RID: 98
		// (Invoke) Token: 0x0600064F RID: 1615
		private delegate void SetFusionTextCallback(string text);

		// Token: 0x02000063 RID: 99
		// (Invoke) Token: 0x06000653 RID: 1619
		private delegate void SetNXDNTextCallback(string text);

		// Token: 0x02000064 RID: 100
		// (Invoke) Token: 0x06000657 RID: 1623
		private delegate void SetAMBESERVERTextCallback(string text);

		// Token: 0x02000065 RID: 101
		// (Invoke) Token: 0x0600065B RID: 1627
		private delegate void SetAMBE_VOXTextCallback(bool vox);

		// Token: 0x02000066 RID: 102
		// (Invoke) Token: 0x0600065F RID: 1631
		private delegate void SetModeTextCallback(string text);

		// Token: 0x02000067 RID: 103
		// (Invoke) Token: 0x06000663 RID: 1635
		private delegate void SetTextCallbackRXTX(information.QSOSTATUS text);

		// Token: 0x02000068 RID: 104
		// (Invoke) Token: 0x06000667 RID: 1639
		private delegate void SetTextCallbackPicture(string text);

		// Token: 0x02000069 RID: 105
		// (Invoke) Token: 0x0600066B RID: 1643
		private delegate void SetTextCallBackAPRS(string text);

		// Token: 0x0200006A RID: 106
		// (Invoke) Token: 0x0600066F RID: 1647
		private delegate void SetTextCallBackPTT(string text);

		// Token: 0x0200006B RID: 107
		// (Invoke) Token: 0x06000673 RID: 1651
		private delegate void SetTextCallBackSlowData(string text);

		// Token: 0x0200006C RID: 108
		// (Invoke) Token: 0x06000677 RID: 1655
		private delegate void SetTextRadioCallback(string radio);

		// Token: 0x0200006D RID: 109
		// (Invoke) Token: 0x0600067B RID: 1659
		private delegate void SetPTTCallback(bool swit);

		// Token: 0x0200006E RID: 110
		// (Invoke) Token: 0x0600067F RID: 1663
		private delegate void SetPTTTimerCallback(string timer, long timeLeft);

		// Token: 0x0200006F RID: 111
		// (Invoke) Token: 0x06000683 RID: 1667
		private delegate void SetVUMeterCallback(int vu);

		// Token: 0x02000070 RID: 112
		// (Invoke) Token: 0x06000687 RID: 1671
		private delegate void SetDMRIDTextCallback(string text, string name, string dmrid, string dmrdest, string city, string country);

		// Token: 0x02000071 RID: 113
		// (Invoke) Token: 0x0600068B RID: 1675
		private delegate void SetModeModeTextCallback(information.MODUS mode, string text);

		// Token: 0x02000072 RID: 114
		// (Invoke) Token: 0x0600068F RID: 1679
		private delegate void SetTextSerialCallback(string text);

		// Token: 0x02000073 RID: 115
		// (Invoke) Token: 0x06000693 RID: 1683
		private delegate void SetSRCDSTCallback(string srcID, string dstID, string text);

		// Token: 0x02000074 RID: 116
		// (Invoke) Token: 0x06000697 RID: 1687
		private delegate void SetTextCallbackAPRS(string text);

		// Token: 0x02000075 RID: 117
		public class connectReflector
		{
			// Token: 0x170000E8 RID: 232
			// (get) Token: 0x0600069A RID: 1690 RVA: 0x00035F84 File Offset: 0x00034184
			// (set) Token: 0x0600069B RID: 1691 RVA: 0x00035F8C File Offset: 0x0003418C
			public string reflector { get; set; }
		}
	}
}
