namespace BlueDV
{
	// Token: 0x02000028 RID: 40
	public partial class Form1 : global::System.Windows.Forms.Form
	{
		// Token: 0x060002F1 RID: 753 RVA: 0x0001E7FE File Offset: 0x0001C9FE
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x060002F2 RID: 754 RVA: 0x0001E820 File Offset: 0x0001CA20
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BlueDV.Form1));
			this.menuStrip1 = new global::System.Windows.Forms.MenuStrip();
			this.toolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.setupToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.updateToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.updateDSTARHostsToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.updateCallDatabaseToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.updateBMMastersToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.updateFusionMastersToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.updateNXDNHostsToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.aMBEToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.soundInputToolStripMenuItem = new global::System.Windows.Forms.ToolStripComboBox();
			this.soundOutputToolStripMenuItem = new global::System.Windows.Forms.ToolStripComboBox();
			this.aboutToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.hisDMRid = new global::System.Windows.Forms.Label();
			this.hisName = new global::System.Windows.Forms.Label();
			this.nameStaticLabel = new global::System.Windows.Forms.Label();
			this.hisCall = new global::System.Windows.Forms.Label();
			this.dmridStaticLabel = new global::System.Windows.Forms.Label();
			this.callStaticLabel = new global::System.Windows.Forms.Label();
			this.dynamicRXTX = new global::System.Windows.Forms.Label();
			this.listView1 = new global::System.Windows.Forms.ListView();
			this.Date = new global::System.Windows.Forms.ColumnHeader();
			this.Call = new global::System.Windows.Forms.ColumnHeader();
			this.NiceName = new global::System.Windows.Forms.ColumnHeader();
			this.Mode = new global::System.Windows.Forms.ColumnHeader();
			this.Panel1 = new global::System.Windows.Forms.Panel();
			this.hisCountry = new global::System.Windows.Forms.Label();
			this.hisCity = new global::System.Windows.Forms.Label();
			this.label25 = new global::System.Windows.Forms.Label();
			this.label24 = new global::System.Windows.Forms.Label();
			this.fusionModeLabel = new global::System.Windows.Forms.Label();
			this.dynamic_mode_text = new global::System.Windows.Forms.Label();
			this.chatImage = new global::System.Windows.Forms.PictureBox();
			this.hisDSTARidsmall = new global::System.Windows.Forms.Label();
			this.lastConnectedReflector = new global::System.Windows.Forms.Label();
			this.RX = new global::System.Windows.Forms.Label();
			this.TX = new global::System.Windows.Forms.Label();
			this.TXprogressBarEx = new global::ProgressBarEx.ProgressBarEx();
			this.BER = new global::System.Windows.Forms.Label();
			this.RXprogressBarEx = new global::ProgressBarEx.ProgressBarEx();
			this.dynamicStatus = new global::System.Windows.Forms.Label();
			this.staticStatus = new global::System.Windows.Forms.Label();
			this.Firmware = new global::System.Windows.Forms.Label();
			this.firmwareLabel = new global::System.Windows.Forms.Label();
			this.dmrmaster = new global::System.Windows.Forms.Label();
			this.DMRmasterStatic = new global::System.Windows.Forms.Label();
			this.FrequencyLabel = new global::System.Windows.Forms.Label();
			this.frequency = new global::System.Windows.Forms.Label();
			this.dynamichisDMRdest = new global::System.Windows.Forms.Label();
			this.dynLastReflector = new global::System.Windows.Forms.Label();
			this.killTimerLabel = new global::System.Windows.Forms.Label();
			this.bypa7lim = new global::System.Windows.Forms.Label();
			this.version = new global::System.Windows.Forms.Label();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			this.cpuinfo = new global::System.Windows.Forms.Label();
			this.toggleSwitchDMRconnect = new global::JCS.ToggleSwitch();
			this.staticDMR = new global::System.Windows.Forms.Label();
			this.toggleSwitchDSTARconnect = new global::JCS.ToggleSwitch();
			this.toggleSwitchFusionconnect = new global::JCS.ToggleSwitch();
			this.label1 = new global::System.Windows.Forms.Label();
			this.label2 = new global::System.Windows.Forms.Label();
			this.comboBoxReflectorList = new global::System.Windows.Forms.ComboBox();
			this.comboBoxReflectorModule = new global::System.Windows.Forms.ComboBox();
			this.radioButtonREF = new global::System.Windows.Forms.RadioButton();
			this.radioButtonDCS = new global::System.Windows.Forms.RadioButton();
			this.radioButtonXRF = new global::System.Windows.Forms.RadioButton();
			this.DMRPanel = new global::System.Windows.Forms.Panel();
			this.label9 = new global::System.Windows.Forms.Label();
			this.dynamicHisDMRCallBox = new global::System.Windows.Forms.Label();
			this.dynamicDMRstatus = new global::System.Windows.Forms.Label();
			this.staticDMRstatus = new global::System.Windows.Forms.Label();
			this.label4 = new global::System.Windows.Forms.Label();
			this.DSTARPanel = new global::System.Windows.Forms.Panel();
			this.label8 = new global::System.Windows.Forms.Label();
			this.dynamicHisDSTARCallBox = new global::System.Windows.Forms.Label();
			this.dynamicDSTARstatus = new global::System.Windows.Forms.Label();
			this.staticDSTARstatus = new global::System.Windows.Forms.Label();
			this.label5 = new global::System.Windows.Forms.Label();
			this.FusionPanel = new global::System.Windows.Forms.Panel();
			this.label7 = new global::System.Windows.Forms.Label();
			this.dynamicHisFUSIONCallBox = new global::System.Windows.Forms.Label();
			this.dynamicFusionstatus = new global::System.Windows.Forms.Label();
			this.staticFusionstatus = new global::System.Windows.Forms.Label();
			this.label6 = new global::System.Windows.Forms.Label();
			this.comboBoxModeSelector = new global::System.Windows.Forms.ComboBox();
			this.toggleSwitchSerialConnect = new global::JCS.ToggleSwitch();
			this.staticSerial = new global::System.Windows.Forms.Label();
			this.onAIRswitch = new global::JCS.ToggleSwitch();
			this.VUMeter = new global::System.Windows.Forms.AGauge();
			this.toggleSwitchGroupPrivate = new global::JCS.ToggleSwitch();
			this.VOXcheckBox = new global::System.Windows.Forms.CheckBox();
			this.VOXLevelTrackBar = new global::System.Windows.Forms.TrackBar();
			this.DMRManualDialComboBox1 = new global::System.Windows.Forms.ComboBox();
			this.tabControl1 = new global::System.Windows.Forms.TabControl();
			this.tabPage1 = new global::System.Windows.Forms.TabPage();
			this.AMBE = new global::System.Windows.Forms.TabPage();
			this.label31 = new global::System.Windows.Forms.Label();
			this.label32 = new global::System.Windows.Forms.Label();
			this.NXDNinlabel = new global::System.Windows.Forms.Label();
			this.NXDNoutlabel = new global::System.Windows.Forms.Label();
			this.NXDNintrackBar = new global::System.Windows.Forms.TrackBar();
			this.NXDNouttrackBar = new global::System.Windows.Forms.TrackBar();
			this.label35 = new global::System.Windows.Forms.Label();
			this.FusionGoupBox = new global::System.Windows.Forms.GroupBox();
			this.label21 = new global::System.Windows.Forms.Label();
			this.DGIDComboBox = new global::System.Windows.Forms.ComboBox();
			this.groupBox1 = new global::System.Windows.Forms.GroupBox();
			this.TGLookupLabel = new global::System.Windows.Forms.Label();
			this.VOXlabel = new global::System.Windows.Forms.Label();
			this.hangLabel = new global::System.Windows.Forms.Label();
			this.label20 = new global::System.Windows.Forms.Label();
			this.HangTimeLabel = new global::System.Windows.Forms.Label();
			this.label23 = new global::System.Windows.Forms.Label();
			this.label22 = new global::System.Windows.Forms.Label();
			this.FUSIONinlabel = new global::System.Windows.Forms.Label();
			this.FUSIONoutlabel = new global::System.Windows.Forms.Label();
			this.FUSIONintrackBar = new global::System.Windows.Forms.TrackBar();
			this.FUSIONouttrackBar = new global::System.Windows.Forms.TrackBar();
			this.label19 = new global::System.Windows.Forms.Label();
			this.simpleModeCheckBox = new global::System.Windows.Forms.CheckBox();
			this.helplinkLabel = new global::System.Windows.Forms.LinkLabel();
			this.hangTimeTrackBar = new global::System.Windows.Forms.TrackBar();
			this.label14 = new global::System.Windows.Forms.Label();
			this.DMRGainlabel = new global::System.Windows.Forms.Label();
			this.label12 = new global::System.Windows.Forms.Label();
			this.label11 = new global::System.Windows.Forms.Label();
			this.label10 = new global::System.Windows.Forms.Label();
			this.label3 = new global::System.Windows.Forms.Label();
			this.DMRinlabel = new global::System.Windows.Forms.Label();
			this.DMRoutlabel = new global::System.Windows.Forms.Label();
			this.DSTARoutlabel = new global::System.Windows.Forms.Label();
			this.DSTARinlabel = new global::System.Windows.Forms.Label();
			this.DSTARintrackBar = new global::System.Windows.Forms.TrackBar();
			this.DSTARouttrackBar = new global::System.Windows.Forms.TrackBar();
			this.DMRintrackBar = new global::System.Windows.Forms.TrackBar();
			this.DMRouttrackBar = new global::System.Windows.Forms.TrackBar();
			this.tabPage3 = new global::System.Windows.Forms.TabPage();
			this.CallookupListView = new global::System.Windows.Forms.ListView();
			this.columnHeader3 = new global::System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new global::System.Windows.Forms.ColumnHeader();
			this.DMRid = new global::System.Windows.Forms.ColumnHeader();
			this.label16 = new global::System.Windows.Forms.Label();
			this.CallLookupTextBox = new global::System.Windows.Forms.TextBox();
			this.label15 = new global::System.Windows.Forms.Label();
			this.label13 = new global::System.Windows.Forms.Label();
			this.SearchTextBox = new global::System.Windows.Forms.TextBox();
			this.TGlistView = new global::System.Windows.Forms.ListView();
			this.columnHeader1 = new global::System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new global::System.Windows.Forms.ColumnHeader();
			this.APRSchatTab = new global::System.Windows.Forms.TabPage();
			this.APRSChatCallComboBox = new global::System.Windows.Forms.ComboBox();
			this.HelplinkLabel1 = new global::System.Windows.Forms.LinkLabel();
			this.label18 = new global::System.Windows.Forms.Label();
			this.label17 = new global::System.Windows.Forms.Label();
			this.MessagetextBox = new global::System.Windows.Forms.TextBox();
			this.ChattextBox = new global::System.Windows.Forms.TextBox();
			this.checkBox1 = new global::System.Windows.Forms.CheckBox();
			this.newVersionLinkLabel = new global::System.Windows.Forms.LinkLabel();
			this.thanksTo = new global::System.Windows.Forms.Label();
			this.radioButtonXLX = new global::System.Windows.Forms.RadioButton();
			this.XLXURLpictureBox = new global::System.Windows.Forms.PictureBox();
			this.radioButtonJPN = new global::System.Windows.Forms.RadioButton();
			this.DMRAMBEpictureBox = new global::System.Windows.Forms.PictureBox();
			this.DSTARAMBEpictureBox = new global::System.Windows.Forms.PictureBox();
			this.FUSIONAMBEpictureBox = new global::System.Windows.Forms.PictureBox();
			this.donatePictureBox = new global::System.Windows.Forms.PictureBox();
			this.NXDNPanel = new global::System.Windows.Forms.Panel();
			this.label26 = new global::System.Windows.Forms.Label();
			this.dynamicHisNXDNCallBox = new global::System.Windows.Forms.Label();
			this.dynamicNXDNstatus = new global::System.Windows.Forms.Label();
			this.label29 = new global::System.Windows.Forms.Label();
			this.label30 = new global::System.Windows.Forms.Label();
			this.toggleSwitchNXDNconnect = new global::JCS.ToggleSwitch();
			this.label36 = new global::System.Windows.Forms.Label();
			this.NXDNAMBEpictureBox = new global::System.Windows.Forms.PictureBox();
			this.NXDNAMBEButton = new global::BlueDV.RoundButton();
			this.FUSIONAMBEButton = new global::BlueDV.RoundButton();
			this.DSTARAMBEButton = new global::BlueDV.RoundButton();
			this.DMRAMBEButton = new global::BlueDV.RoundButton();
			this.unlinkButton = new global::BlueDV.RoundButton();
			this.linkButton = new global::BlueDV.RoundButton();
			this.menuStrip1.SuspendLayout();
			this.Panel1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.chatImage).BeginInit();
			this.DMRPanel.SuspendLayout();
			this.DSTARPanel.SuspendLayout();
			this.FusionPanel.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.VOXLevelTrackBar).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.AMBE.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.NXDNintrackBar).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.NXDNouttrackBar).BeginInit();
			this.FusionGoupBox.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.FUSIONintrackBar).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.FUSIONouttrackBar).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.hangTimeTrackBar).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.DSTARintrackBar).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.DSTARouttrackBar).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.DMRintrackBar).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.DMRouttrackBar).BeginInit();
			this.tabPage3.SuspendLayout();
			this.APRSchatTab.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.XLXURLpictureBox).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.DMRAMBEpictureBox).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.DSTARAMBEpictureBox).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.FUSIONAMBEpictureBox).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.donatePictureBox).BeginInit();
			this.NXDNPanel.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.NXDNAMBEpictureBox).BeginInit();
			base.SuspendLayout();
			this.menuStrip1.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[] { this.toolStripMenuItem1, this.updateToolStripMenuItem, this.aMBEToolStripMenuItem, this.aboutToolStripMenuItem });
			componentResourceManager.ApplyResources(this.menuStrip1, "menuStrip1");
			this.menuStrip1.Name = "menuStrip1";
			componentResourceManager.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
			this.toolStripMenuItem1.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[] { this.setupToolStripMenuItem, this.exitToolStripMenuItem });
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			componentResourceManager.ApplyResources(this.setupToolStripMenuItem, "setupToolStripMenuItem");
			this.setupToolStripMenuItem.Name = "setupToolStripMenuItem";
			this.setupToolStripMenuItem.Click += new global::System.EventHandler(this.setupToolStripMenuItem_Click);
			componentResourceManager.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Click += new global::System.EventHandler(this.exitToolStripMenuItem_Click);
			componentResourceManager.ApplyResources(this.updateToolStripMenuItem, "updateToolStripMenuItem");
			this.updateToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[] { this.updateDSTARHostsToolStripMenuItem, this.updateCallDatabaseToolStripMenuItem, this.updateBMMastersToolStripMenuItem, this.updateFusionMastersToolStripMenuItem, this.updateNXDNHostsToolStripMenuItem });
			this.updateToolStripMenuItem.Name = "updateToolStripMenuItem";
			componentResourceManager.ApplyResources(this.updateDSTARHostsToolStripMenuItem, "updateDSTARHostsToolStripMenuItem");
			this.updateDSTARHostsToolStripMenuItem.Name = "updateDSTARHostsToolStripMenuItem";
			this.updateDSTARHostsToolStripMenuItem.Click += new global::System.EventHandler(this.updateDSTARHostsToolStripMenuItem_Click);
			componentResourceManager.ApplyResources(this.updateCallDatabaseToolStripMenuItem, "updateCallDatabaseToolStripMenuItem");
			this.updateCallDatabaseToolStripMenuItem.Name = "updateCallDatabaseToolStripMenuItem";
			this.updateCallDatabaseToolStripMenuItem.Click += new global::System.EventHandler(this.updateCallDatabaseToolStripMenuItem_Click);
			componentResourceManager.ApplyResources(this.updateBMMastersToolStripMenuItem, "updateBMMastersToolStripMenuItem");
			this.updateBMMastersToolStripMenuItem.Name = "updateBMMastersToolStripMenuItem";
			this.updateBMMastersToolStripMenuItem.Click += new global::System.EventHandler(this.updateBMMastersToolStripMenuItem_Click);
			componentResourceManager.ApplyResources(this.updateFusionMastersToolStripMenuItem, "updateFusionMastersToolStripMenuItem");
			this.updateFusionMastersToolStripMenuItem.Name = "updateFusionMastersToolStripMenuItem";
			this.updateFusionMastersToolStripMenuItem.Click += new global::System.EventHandler(this.updateFusionToolStripMenuItem_Click);
			componentResourceManager.ApplyResources(this.updateNXDNHostsToolStripMenuItem, "updateNXDNHostsToolStripMenuItem");
			this.updateNXDNHostsToolStripMenuItem.Name = "updateNXDNHostsToolStripMenuItem";
			this.updateNXDNHostsToolStripMenuItem.Click += new global::System.EventHandler(this.updateNXDNHostsToolStripMenuItem_Click);
			componentResourceManager.ApplyResources(this.aMBEToolStripMenuItem, "aMBEToolStripMenuItem");
			this.aMBEToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[] { this.soundInputToolStripMenuItem, this.soundOutputToolStripMenuItem });
			this.aMBEToolStripMenuItem.Name = "aMBEToolStripMenuItem";
			this.aMBEToolStripMenuItem.Click += new global::System.EventHandler(this.aMBEToolStripMenuItem_Click);
			this.soundInputToolStripMenuItem.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.soundInputToolStripMenuItem.DropDownWidth = 300;
			this.soundInputToolStripMenuItem.Name = "soundInputToolStripMenuItem";
			componentResourceManager.ApplyResources(this.soundInputToolStripMenuItem, "soundInputToolStripMenuItem");
			this.soundInputToolStripMenuItem.SelectedIndexChanged += new global::System.EventHandler(this.soundInputToolStripMenuItem_SelectedIndexChanged);
			this.soundOutputToolStripMenuItem.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.soundOutputToolStripMenuItem.DropDownWidth = 300;
			this.soundOutputToolStripMenuItem.Name = "soundOutputToolStripMenuItem";
			componentResourceManager.ApplyResources(this.soundOutputToolStripMenuItem, "soundOutputToolStripMenuItem");
			this.soundOutputToolStripMenuItem.SelectedIndexChanged += new global::System.EventHandler(this.soundOutputToolStripMenuItem_SelectedIndexChanged);
			this.aboutToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[] { this.aboutToolStripMenuItem1 });
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			componentResourceManager.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
			componentResourceManager.ApplyResources(this.aboutToolStripMenuItem1, "aboutToolStripMenuItem1");
			this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
			this.aboutToolStripMenuItem1.Click += new global::System.EventHandler(this.aboutToolStripMenuItem1_Click);
			componentResourceManager.ApplyResources(this.hisDMRid, "hisDMRid");
			this.hisDMRid.BackColor = global::System.Drawing.Color.Transparent;
			this.hisDMRid.Name = "hisDMRid";
			componentResourceManager.ApplyResources(this.hisName, "hisName");
			this.hisName.BackColor = global::System.Drawing.Color.Transparent;
			this.hisName.Name = "hisName";
			componentResourceManager.ApplyResources(this.nameStaticLabel, "nameStaticLabel");
			this.nameStaticLabel.BackColor = global::System.Drawing.Color.Transparent;
			this.nameStaticLabel.Name = "nameStaticLabel";
			componentResourceManager.ApplyResources(this.hisCall, "hisCall");
			this.hisCall.BackColor = global::System.Drawing.Color.Transparent;
			this.hisCall.Name = "hisCall";
			this.hisCall.Click += new global::System.EventHandler(this.qrzLookup);
			componentResourceManager.ApplyResources(this.dmridStaticLabel, "dmridStaticLabel");
			this.dmridStaticLabel.BackColor = global::System.Drawing.Color.Transparent;
			this.dmridStaticLabel.Name = "dmridStaticLabel";
			componentResourceManager.ApplyResources(this.callStaticLabel, "callStaticLabel");
			this.callStaticLabel.BackColor = global::System.Drawing.Color.Transparent;
			this.callStaticLabel.Name = "callStaticLabel";
			componentResourceManager.ApplyResources(this.dynamicRXTX, "dynamicRXTX");
			this.dynamicRXTX.Name = "dynamicRXTX";
			componentResourceManager.ApplyResources(this.listView1, "listView1");
			this.listView1.BackColor = global::System.Drawing.SystemColors.ActiveBorder;
			this.listView1.BackgroundImage = global::BlueDV.Properties.Resources.image1_nr3;
			this.listView1.BackgroundImageTiled = true;
			this.listView1.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.listView1.Columns.AddRange(new global::System.Windows.Forms.ColumnHeader[] { this.Date, this.Call, this.NiceName, this.Mode });
			this.listView1.ForeColor = global::System.Drawing.SystemColors.WindowText;
			this.listView1.FullRowSelect = true;
			this.listView1.HeaderStyle = global::System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView1.HideSelection = false;
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.ShowItemToolTips = true;
			this.listView1.TileSize = new global::System.Drawing.Size(168, 30);
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = global::System.Windows.Forms.View.Details;
			this.listView1.Click += new global::System.EventHandler(this.qrzLookupItem);
			this.listView1.DoubleClick += new global::System.EventHandler(this.listView1_DoubleClick);
			componentResourceManager.ApplyResources(this.Date, "Date");
			componentResourceManager.ApplyResources(this.Call, "Call");
			componentResourceManager.ApplyResources(this.NiceName, "NiceName");
			componentResourceManager.ApplyResources(this.Mode, "Mode");
			this.Panel1.BackColor = global::System.Drawing.Color.Transparent;
			this.Panel1.BackgroundImage = global::BlueDV.Properties.Resources.bluedvwinbluebgrnd;
			componentResourceManager.ApplyResources(this.Panel1, "Panel1");
			this.Panel1.Controls.Add(this.hisCountry);
			this.Panel1.Controls.Add(this.hisCity);
			this.Panel1.Controls.Add(this.label25);
			this.Panel1.Controls.Add(this.label24);
			this.Panel1.Controls.Add(this.fusionModeLabel);
			this.Panel1.Controls.Add(this.hisDMRid);
			this.Panel1.Controls.Add(this.dynamic_mode_text);
			this.Panel1.Controls.Add(this.chatImage);
			this.Panel1.Controls.Add(this.hisDSTARidsmall);
			this.Panel1.Controls.Add(this.lastConnectedReflector);
			this.Panel1.Controls.Add(this.RX);
			this.Panel1.Controls.Add(this.TX);
			this.Panel1.Controls.Add(this.TXprogressBarEx);
			this.Panel1.Controls.Add(this.BER);
			this.Panel1.Controls.Add(this.RXprogressBarEx);
			this.Panel1.Controls.Add(this.dynamicStatus);
			this.Panel1.Controls.Add(this.staticStatus);
			this.Panel1.Controls.Add(this.Firmware);
			this.Panel1.Controls.Add(this.firmwareLabel);
			this.Panel1.Controls.Add(this.dmrmaster);
			this.Panel1.Controls.Add(this.DMRmasterStatic);
			this.Panel1.Controls.Add(this.FrequencyLabel);
			this.Panel1.Controls.Add(this.frequency);
			this.Panel1.Controls.Add(this.hisName);
			this.Panel1.Controls.Add(this.dynamicRXTX);
			this.Panel1.Controls.Add(this.callStaticLabel);
			this.Panel1.Controls.Add(this.hisCall);
			this.Panel1.Controls.Add(this.nameStaticLabel);
			this.Panel1.Controls.Add(this.dmridStaticLabel);
			this.Panel1.Controls.Add(this.dynamichisDMRdest);
			this.Panel1.Name = "Panel1";
			componentResourceManager.ApplyResources(this.hisCountry, "hisCountry");
			this.hisCountry.BackColor = global::System.Drawing.Color.Transparent;
			this.hisCountry.Name = "hisCountry";
			componentResourceManager.ApplyResources(this.hisCity, "hisCity");
			this.hisCity.BackColor = global::System.Drawing.Color.Transparent;
			this.hisCity.Name = "hisCity";
			componentResourceManager.ApplyResources(this.label25, "label25");
			this.label25.BackColor = global::System.Drawing.Color.Transparent;
			this.label25.Name = "label25";
			componentResourceManager.ApplyResources(this.label24, "label24");
			this.label24.BackColor = global::System.Drawing.Color.Transparent;
			this.label24.Name = "label24";
			componentResourceManager.ApplyResources(this.fusionModeLabel, "fusionModeLabel");
			this.fusionModeLabel.Name = "fusionModeLabel";
			componentResourceManager.ApplyResources(this.dynamic_mode_text, "dynamic_mode_text");
			this.dynamic_mode_text.Name = "dynamic_mode_text";
			componentResourceManager.ApplyResources(this.chatImage, "chatImage");
			this.chatImage.Name = "chatImage";
			this.chatImage.TabStop = false;
			this.chatImage.Click += new global::System.EventHandler(this.Click_Chat_Picture);
			componentResourceManager.ApplyResources(this.hisDSTARidsmall, "hisDSTARidsmall");
			this.hisDSTARidsmall.Name = "hisDSTARidsmall";
			componentResourceManager.ApplyResources(this.lastConnectedReflector, "lastConnectedReflector");
			this.lastConnectedReflector.Name = "lastConnectedReflector";
			componentResourceManager.ApplyResources(this.RX, "RX");
			this.RX.Name = "RX";
			this.RX.Tag = "JsonConvert.DeserializeObject(gett(\"http://registry.dstar.su/api/node.php\")";
			componentResourceManager.ApplyResources(this.TX, "TX");
			this.TX.Name = "TX";
			this.TX.Tag = "JsonConvert.DeserializeObject(gett(\"http://registry.dstar.su/api/node.php\")";
			this.TXprogressBarEx.BackColor = global::System.Drawing.Color.Transparent;
			this.TXprogressBarEx.BackgroundColor = global::System.Drawing.Color.DodgerBlue;
			this.TXprogressBarEx.Image = null;
			componentResourceManager.ApplyResources(this.TXprogressBarEx, "TXprogressBarEx");
			this.TXprogressBarEx.Name = "TXprogressBarEx";
			this.TXprogressBarEx.ProgressColor = global::System.Drawing.Color.Red;
			this.TXprogressBarEx.ProgressDirection = global::ProgressBarEx.ProgressBarEx.ProgressDir.Vertical;
			componentResourceManager.ApplyResources(this.BER, "BER");
			this.BER.Name = "BER";
			this.RXprogressBarEx.BackColor = global::System.Drawing.Color.Transparent;
			this.RXprogressBarEx.BackgroundColor = global::System.Drawing.Color.DodgerBlue;
			this.RXprogressBarEx.Image = null;
			componentResourceManager.ApplyResources(this.RXprogressBarEx, "RXprogressBarEx");
			this.RXprogressBarEx.Name = "RXprogressBarEx";
			this.RXprogressBarEx.ProgressColor = global::System.Drawing.Color.Green;
			this.RXprogressBarEx.ProgressDirection = global::ProgressBarEx.ProgressBarEx.ProgressDir.Vertical;
			componentResourceManager.ApplyResources(this.dynamicStatus, "dynamicStatus");
			this.dynamicStatus.Name = "dynamicStatus";
			componentResourceManager.ApplyResources(this.staticStatus, "staticStatus");
			this.staticStatus.Name = "staticStatus";
			componentResourceManager.ApplyResources(this.Firmware, "Firmware");
			this.Firmware.Name = "Firmware";
			componentResourceManager.ApplyResources(this.firmwareLabel, "firmwareLabel");
			this.firmwareLabel.Name = "firmwareLabel";
			componentResourceManager.ApplyResources(this.dmrmaster, "dmrmaster");
			this.dmrmaster.Name = "dmrmaster";
			componentResourceManager.ApplyResources(this.DMRmasterStatic, "DMRmasterStatic");
			this.DMRmasterStatic.Name = "DMRmasterStatic";
			componentResourceManager.ApplyResources(this.FrequencyLabel, "FrequencyLabel");
			this.FrequencyLabel.Name = "FrequencyLabel";
			componentResourceManager.ApplyResources(this.frequency, "frequency");
			this.frequency.Name = "frequency";
			componentResourceManager.ApplyResources(this.dynamichisDMRdest, "dynamichisDMRdest");
			this.dynamichisDMRdest.BackColor = global::System.Drawing.Color.Transparent;
			this.dynamichisDMRdest.Name = "dynamichisDMRdest";
			componentResourceManager.ApplyResources(this.dynLastReflector, "dynLastReflector");
			this.dynLastReflector.Name = "dynLastReflector";
			componentResourceManager.ApplyResources(this.killTimerLabel, "killTimerLabel");
			this.killTimerLabel.BackColor = global::System.Drawing.Color.Transparent;
			this.killTimerLabel.Name = "killTimerLabel";
			componentResourceManager.ApplyResources(this.bypa7lim, "bypa7lim");
			this.bypa7lim.BackColor = global::System.Drawing.Color.Transparent;
			this.bypa7lim.Name = "bypa7lim";
			componentResourceManager.ApplyResources(this.version, "version");
			this.version.BackColor = global::System.Drawing.Color.Transparent;
			this.version.Name = "version";
			this.timer1.Enabled = true;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			componentResourceManager.ApplyResources(this.cpuinfo, "cpuinfo");
			this.cpuinfo.BackColor = global::System.Drawing.Color.Transparent;
			this.cpuinfo.Name = "cpuinfo";
			componentResourceManager.ApplyResources(this.toggleSwitchDMRconnect, "toggleSwitchDMRconnect");
			this.toggleSwitchDMRconnect.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.toggleSwitchDMRconnect.BackColor = global::System.Drawing.Color.Transparent;
			this.toggleSwitchDMRconnect.Name = "toggleSwitchDMRconnect";
			this.toggleSwitchDMRconnect.OffFont = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.toggleSwitchDMRconnect.OnFont = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.toggleSwitchDMRconnect.Style = global::JCS.ToggleSwitch.ToggleSwitchStyle.Iphone;
			this.toggleSwitchDMRconnect.CheckedChanged += new global::JCS.ToggleSwitch.CheckedChangedDelegate(this.toggleSwitchDMRconnect_CheckedChanged);
			componentResourceManager.ApplyResources(this.staticDMR, "staticDMR");
			this.staticDMR.BackColor = global::System.Drawing.Color.Transparent;
			this.staticDMR.Name = "staticDMR";
			componentResourceManager.ApplyResources(this.toggleSwitchDSTARconnect, "toggleSwitchDSTARconnect");
			this.toggleSwitchDSTARconnect.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.toggleSwitchDSTARconnect.BackColor = global::System.Drawing.Color.Transparent;
			this.toggleSwitchDSTARconnect.Name = "toggleSwitchDSTARconnect";
			this.toggleSwitchDSTARconnect.OffFont = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.toggleSwitchDSTARconnect.OnFont = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.toggleSwitchDSTARconnect.Style = global::JCS.ToggleSwitch.ToggleSwitchStyle.Iphone;
			this.toggleSwitchDSTARconnect.CheckedChanged += new global::JCS.ToggleSwitch.CheckedChangedDelegate(this.toggleSwitchDSTARconnect_CheckedChanged);
			componentResourceManager.ApplyResources(this.toggleSwitchFusionconnect, "toggleSwitchFusionconnect");
			this.toggleSwitchFusionconnect.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.toggleSwitchFusionconnect.BackColor = global::System.Drawing.Color.Transparent;
			this.toggleSwitchFusionconnect.Name = "toggleSwitchFusionconnect";
			this.toggleSwitchFusionconnect.OffFont = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.toggleSwitchFusionconnect.OnFont = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.toggleSwitchFusionconnect.Style = global::JCS.ToggleSwitch.ToggleSwitchStyle.Iphone;
			this.toggleSwitchFusionconnect.CheckedChanged += new global::JCS.ToggleSwitch.CheckedChangedDelegate(this.toggleSwitchFusionconnect_CheckedChanged);
			componentResourceManager.ApplyResources(this.label1, "label1");
			this.label1.BackColor = global::System.Drawing.Color.Transparent;
			this.label1.Name = "label1";
			componentResourceManager.ApplyResources(this.label2, "label2");
			this.label2.BackColor = global::System.Drawing.Color.Transparent;
			this.label2.Name = "label2";
			this.comboBoxReflectorList.AutoCompleteMode = global::System.Windows.Forms.AutoCompleteMode.Suggest;
			this.comboBoxReflectorList.AutoCompleteSource = global::System.Windows.Forms.AutoCompleteSource.ListItems;
			this.comboBoxReflectorList.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxReflectorList.FormattingEnabled = true;
			componentResourceManager.ApplyResources(this.comboBoxReflectorList, "comboBoxReflectorList");
			this.comboBoxReflectorList.Name = "comboBoxReflectorList";
			this.comboBoxReflectorList.SelectionChangeCommitted += new global::System.EventHandler(this.comboBoxReflectorList_SelectionChangeCommitted);
			this.comboBoxReflectorList.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.reflectorListKeyPress);
			this.comboBoxReflectorModule.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxReflectorModule.FormattingEnabled = true;
			this.comboBoxReflectorModule.Items.AddRange(new object[]
			{
				componentResourceManager.GetString("comboBoxReflectorModule.Items"),
				componentResourceManager.GetString("comboBoxReflectorModule.Items1"),
				componentResourceManager.GetString("comboBoxReflectorModule.Items2"),
				componentResourceManager.GetString("comboBoxReflectorModule.Items3"),
				componentResourceManager.GetString("comboBoxReflectorModule.Items4")
			});
			componentResourceManager.ApplyResources(this.comboBoxReflectorModule, "comboBoxReflectorModule");
			this.comboBoxReflectorModule.Name = "comboBoxReflectorModule";
			this.comboBoxReflectorModule.SelectionChangeCommitted += new global::System.EventHandler(this.comboBoxReflectorList_SelectionChangeCommitted);
			componentResourceManager.ApplyResources(this.radioButtonREF, "radioButtonREF");
			this.radioButtonREF.BackColor = global::System.Drawing.Color.Transparent;
			this.radioButtonREF.Name = "radioButtonREF";
			this.radioButtonREF.UseVisualStyleBackColor = false;
			this.radioButtonREF.CheckedChanged += new global::System.EventHandler(this.radioButtonREF_CheckedChanged);
			componentResourceManager.ApplyResources(this.radioButtonDCS, "radioButtonDCS");
			this.radioButtonDCS.BackColor = global::System.Drawing.Color.Transparent;
			this.radioButtonDCS.Name = "radioButtonDCS";
			this.radioButtonDCS.UseVisualStyleBackColor = false;
			this.radioButtonDCS.CheckedChanged += new global::System.EventHandler(this.radioButtonDCS_CheckedChanged);
			componentResourceManager.ApplyResources(this.radioButtonXRF, "radioButtonXRF");
			this.radioButtonXRF.BackColor = global::System.Drawing.Color.Transparent;
			this.radioButtonXRF.Name = "radioButtonXRF";
			this.radioButtonXRF.UseVisualStyleBackColor = false;
			this.radioButtonXRF.CheckedChanged += new global::System.EventHandler(this.radioButtonXRF_CheckedChanged);
			componentResourceManager.ApplyResources(this.DMRPanel, "DMRPanel");
			this.DMRPanel.BackColor = global::System.Drawing.Color.Transparent;
			this.DMRPanel.BackgroundImage = global::BlueDV.Properties.Resources.bluedvwinbluebgrnd;
			this.DMRPanel.Controls.Add(this.label9);
			this.DMRPanel.Controls.Add(this.dynamicHisDMRCallBox);
			this.DMRPanel.Controls.Add(this.dynamicDMRstatus);
			this.DMRPanel.Controls.Add(this.staticDMRstatus);
			this.DMRPanel.Controls.Add(this.label4);
			this.DMRPanel.Name = "DMRPanel";
			this.DMRPanel.DoubleClick += new global::System.EventHandler(this.DMRPanel_DoubleClick);
			componentResourceManager.ApplyResources(this.label9, "label9");
			this.label9.Name = "label9";
			componentResourceManager.ApplyResources(this.dynamicHisDMRCallBox, "dynamicHisDMRCallBox");
			this.dynamicHisDMRCallBox.Name = "dynamicHisDMRCallBox";
			componentResourceManager.ApplyResources(this.dynamicDMRstatus, "dynamicDMRstatus");
			this.dynamicDMRstatus.Name = "dynamicDMRstatus";
			componentResourceManager.ApplyResources(this.staticDMRstatus, "staticDMRstatus");
			this.staticDMRstatus.Name = "staticDMRstatus";
			componentResourceManager.ApplyResources(this.label4, "label4");
			this.label4.Name = "label4";
			componentResourceManager.ApplyResources(this.DSTARPanel, "DSTARPanel");
			this.DSTARPanel.BackColor = global::System.Drawing.Color.Transparent;
			this.DSTARPanel.BackgroundImage = global::BlueDV.Properties.Resources.bluedvwinbluebgrnd;
			this.DSTARPanel.Controls.Add(this.label8);
			this.DSTARPanel.Controls.Add(this.dynamicHisDSTARCallBox);
			this.DSTARPanel.Controls.Add(this.dynamicDSTARstatus);
			this.DSTARPanel.Controls.Add(this.staticDSTARstatus);
			this.DSTARPanel.Controls.Add(this.label5);
			this.DSTARPanel.Name = "DSTARPanel";
			this.DSTARPanel.DoubleClick += new global::System.EventHandler(this.DSTARPanel_DoubleClick);
			componentResourceManager.ApplyResources(this.label8, "label8");
			this.label8.Name = "label8";
			componentResourceManager.ApplyResources(this.dynamicHisDSTARCallBox, "dynamicHisDSTARCallBox");
			this.dynamicHisDSTARCallBox.Name = "dynamicHisDSTARCallBox";
			componentResourceManager.ApplyResources(this.dynamicDSTARstatus, "dynamicDSTARstatus");
			this.dynamicDSTARstatus.Name = "dynamicDSTARstatus";
			componentResourceManager.ApplyResources(this.staticDSTARstatus, "staticDSTARstatus");
			this.staticDSTARstatus.Name = "staticDSTARstatus";
			componentResourceManager.ApplyResources(this.label5, "label5");
			this.label5.Name = "label5";
			componentResourceManager.ApplyResources(this.FusionPanel, "FusionPanel");
			this.FusionPanel.BackColor = global::System.Drawing.Color.Transparent;
			this.FusionPanel.BackgroundImage = global::BlueDV.Properties.Resources.bluedvwinbluebgrnd;
			this.FusionPanel.Controls.Add(this.label7);
			this.FusionPanel.Controls.Add(this.dynamicHisFUSIONCallBox);
			this.FusionPanel.Controls.Add(this.dynamicFusionstatus);
			this.FusionPanel.Controls.Add(this.staticFusionstatus);
			this.FusionPanel.Controls.Add(this.label6);
			this.FusionPanel.Name = "FusionPanel";
			this.FusionPanel.DoubleClick += new global::System.EventHandler(this.FusionPanel_DoubleClick);
			componentResourceManager.ApplyResources(this.label7, "label7");
			this.label7.Name = "label7";
			componentResourceManager.ApplyResources(this.dynamicHisFUSIONCallBox, "dynamicHisFUSIONCallBox");
			this.dynamicHisFUSIONCallBox.Name = "dynamicHisFUSIONCallBox";
			componentResourceManager.ApplyResources(this.dynamicFusionstatus, "dynamicFusionstatus");
			this.dynamicFusionstatus.Name = "dynamicFusionstatus";
			componentResourceManager.ApplyResources(this.staticFusionstatus, "staticFusionstatus");
			this.staticFusionstatus.Name = "staticFusionstatus";
			componentResourceManager.ApplyResources(this.label6, "label6");
			this.label6.Name = "label6";
			componentResourceManager.ApplyResources(this.comboBoxModeSelector, "comboBoxModeSelector");
			this.comboBoxModeSelector.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxModeSelector.FormattingEnabled = true;
			this.comboBoxModeSelector.Name = "comboBoxModeSelector";
			this.comboBoxModeSelector.SelectedIndexChanged += new global::System.EventHandler(this.comboBoxModeSelector_SelectedIndexChanged);
			componentResourceManager.ApplyResources(this.toggleSwitchSerialConnect, "toggleSwitchSerialConnect");
			this.toggleSwitchSerialConnect.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.toggleSwitchSerialConnect.BackColor = global::System.Drawing.Color.Transparent;
			this.toggleSwitchSerialConnect.Name = "toggleSwitchSerialConnect";
			this.toggleSwitchSerialConnect.OffFont = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.toggleSwitchSerialConnect.OnFont = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.toggleSwitchSerialConnect.Style = global::JCS.ToggleSwitch.ToggleSwitchStyle.Iphone;
			this.toggleSwitchSerialConnect.CheckedChanged += new global::JCS.ToggleSwitch.CheckedChangedDelegate(this.toggleSwitchSerialConnect_CheckedChanged);
			componentResourceManager.ApplyResources(this.staticSerial, "staticSerial");
			this.staticSerial.BackColor = global::System.Drawing.Color.Transparent;
			this.staticSerial.Name = "staticSerial";
			componentResourceManager.ApplyResources(this.onAIRswitch, "onAIRswitch");
			this.onAIRswitch.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.onAIRswitch.BackColor = global::System.Drawing.Color.Transparent;
			this.onAIRswitch.Name = "onAIRswitch";
			this.onAIRswitch.OffFont = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.onAIRswitch.OffForeColor = global::System.Drawing.Color.White;
			this.onAIRswitch.OffText = "AMBE3000";
			this.onAIRswitch.OnFont = new global::System.Drawing.Font("Microsoft Sans Serif", 14.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.onAIRswitch.OnText = "On AIR";
			this.onAIRswitch.Style = global::JCS.ToggleSwitch.ToggleSwitchStyle.Carbon;
			this.onAIRswitch.CheckedChanged += new global::JCS.ToggleSwitch.CheckedChangedDelegate(this.onAIR);
			componentResourceManager.ApplyResources(this.VUMeter, "VUMeter");
			this.VUMeter.BackgroundImage = global::BlueDV.Properties.Resources.Signal_Meter2;
			this.VUMeter.BaseArcColor = global::System.Drawing.Color.Gray;
			this.VUMeter.BaseArcRadius = 0;
			this.VUMeter.BaseArcStart = 205;
			this.VUMeter.BaseArcSweep = 130;
			this.VUMeter.BaseArcWidth = 0;
			this.VUMeter.Center = new global::System.Drawing.Point(50, 40);
			this.VUMeter.MaxValue = 1f;
			this.VUMeter.MinValue = 0f;
			this.VUMeter.Name = "VUMeter";
			this.VUMeter.NeedleColor1 = global::System.Windows.Forms.AGaugeNeedleColor.Red;
			this.VUMeter.NeedleColor2 = global::System.Drawing.Color.DeepSkyBlue;
			this.VUMeter.NeedleRadius = 35;
			this.VUMeter.NeedleType = global::System.Windows.Forms.NeedleType.Advance;
			this.VUMeter.NeedleWidth = 1;
			this.VUMeter.ScaleLinesInterColor = global::System.Drawing.Color.Black;
			this.VUMeter.ScaleLinesInterInnerRadius = 0;
			this.VUMeter.ScaleLinesInterOuterRadius = 0;
			this.VUMeter.ScaleLinesInterWidth = 1;
			this.VUMeter.ScaleLinesMajorColor = global::System.Drawing.Color.Black;
			this.VUMeter.ScaleLinesMajorInnerRadius = 0;
			this.VUMeter.ScaleLinesMajorOuterRadius = 0;
			this.VUMeter.ScaleLinesMajorStepValue = 1f;
			this.VUMeter.ScaleLinesMajorWidth = 0;
			this.VUMeter.ScaleLinesMinorColor = global::System.Drawing.Color.Gray;
			this.VUMeter.ScaleLinesMinorInnerRadius = 0;
			this.VUMeter.ScaleLinesMinorOuterRadius = 0;
			this.VUMeter.ScaleLinesMinorTicks = 0;
			this.VUMeter.ScaleLinesMinorWidth = 1;
			this.VUMeter.ScaleNumbersColor = global::System.Drawing.Color.Black;
			this.VUMeter.ScaleNumbersFormat = null;
			this.VUMeter.ScaleNumbersRadius = 0;
			this.VUMeter.ScaleNumbersRotation = 0;
			this.VUMeter.ScaleNumbersStartScaleLine = 0;
			this.VUMeter.ScaleNumbersStepScaleLines = 1;
			this.VUMeter.Value = 0f;
			componentResourceManager.ApplyResources(this.toggleSwitchGroupPrivate, "toggleSwitchGroupPrivate");
			this.toggleSwitchGroupPrivate.BackColor = global::System.Drawing.Color.Transparent;
			this.toggleSwitchGroupPrivate.Name = "toggleSwitchGroupPrivate";
			this.toggleSwitchGroupPrivate.OffFont = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.toggleSwitchGroupPrivate.OffText = "G";
			this.toggleSwitchGroupPrivate.OnFont = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.toggleSwitchGroupPrivate.OnText = "P";
			this.toggleSwitchGroupPrivate.Style = global::JCS.ToggleSwitch.ToggleSwitchStyle.Iphone;
			this.toggleSwitchGroupPrivate.CheckedChanged += new global::JCS.ToggleSwitch.CheckedChangedDelegate(this.toggleSwitchGroupPrivate_CheckedChanged);
			componentResourceManager.ApplyResources(this.VOXcheckBox, "VOXcheckBox");
			this.VOXcheckBox.Name = "VOXcheckBox";
			this.VOXcheckBox.UseVisualStyleBackColor = true;
			this.VOXcheckBox.CheckedChanged += new global::System.EventHandler(this.VOXcheckBox_CheckedChanged);
			componentResourceManager.ApplyResources(this.VOXLevelTrackBar, "VOXLevelTrackBar");
			this.VOXLevelTrackBar.LargeChange = 100;
			this.VOXLevelTrackBar.Maximum = 30000;
			this.VOXLevelTrackBar.Minimum = 10;
			this.VOXLevelTrackBar.Name = "VOXLevelTrackBar";
			this.VOXLevelTrackBar.SmallChange = 10;
			this.VOXLevelTrackBar.TickFrequency = 10;
			this.VOXLevelTrackBar.TickStyle = global::System.Windows.Forms.TickStyle.None;
			this.VOXLevelTrackBar.Value = 1000;
			this.VOXLevelTrackBar.Scroll += new global::System.EventHandler(this.trackBar1_Scroll_1);
			componentResourceManager.ApplyResources(this.DMRManualDialComboBox1, "DMRManualDialComboBox1");
			this.DMRManualDialComboBox1.FormattingEnabled = true;
			this.DMRManualDialComboBox1.Name = "DMRManualDialComboBox1";
			this.DMRManualDialComboBox1.TextChanged += new global::System.EventHandler(this.DMRmanualDial_TextChanged);
			this.DMRManualDialComboBox1.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.DMRManualDialComboBox1_KeyPress);
			componentResourceManager.ApplyResources(this.tabControl1, "tabControl1");
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.AMBE);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.APRSchatTab);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			componentResourceManager.ApplyResources(this.tabPage1, "tabPage1");
			this.tabPage1.Controls.Add(this.listView1);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.AMBE, "AMBE");
			this.AMBE.BackgroundImage = global::BlueDV.Properties.Resources.image1nr2;
			this.AMBE.Controls.Add(this.label31);
			this.AMBE.Controls.Add(this.label32);
			this.AMBE.Controls.Add(this.NXDNinlabel);
			this.AMBE.Controls.Add(this.NXDNoutlabel);
			this.AMBE.Controls.Add(this.NXDNintrackBar);
			this.AMBE.Controls.Add(this.NXDNouttrackBar);
			this.AMBE.Controls.Add(this.label35);
			this.AMBE.Controls.Add(this.FusionGoupBox);
			this.AMBE.Controls.Add(this.groupBox1);
			this.AMBE.Controls.Add(this.VOXlabel);
			this.AMBE.Controls.Add(this.hangLabel);
			this.AMBE.Controls.Add(this.label20);
			this.AMBE.Controls.Add(this.HangTimeLabel);
			this.AMBE.Controls.Add(this.label23);
			this.AMBE.Controls.Add(this.label22);
			this.AMBE.Controls.Add(this.FUSIONinlabel);
			this.AMBE.Controls.Add(this.FUSIONoutlabel);
			this.AMBE.Controls.Add(this.FUSIONintrackBar);
			this.AMBE.Controls.Add(this.FUSIONouttrackBar);
			this.AMBE.Controls.Add(this.label19);
			this.AMBE.Controls.Add(this.simpleModeCheckBox);
			this.AMBE.Controls.Add(this.helplinkLabel);
			this.AMBE.Controls.Add(this.hangTimeTrackBar);
			this.AMBE.Controls.Add(this.label14);
			this.AMBE.Controls.Add(this.DMRGainlabel);
			this.AMBE.Controls.Add(this.label12);
			this.AMBE.Controls.Add(this.label11);
			this.AMBE.Controls.Add(this.label10);
			this.AMBE.Controls.Add(this.label3);
			this.AMBE.Controls.Add(this.DMRinlabel);
			this.AMBE.Controls.Add(this.DMRoutlabel);
			this.AMBE.Controls.Add(this.DSTARoutlabel);
			this.AMBE.Controls.Add(this.DSTARinlabel);
			this.AMBE.Controls.Add(this.DSTARintrackBar);
			this.AMBE.Controls.Add(this.DSTARouttrackBar);
			this.AMBE.Controls.Add(this.DMRintrackBar);
			this.AMBE.Controls.Add(this.DMRouttrackBar);
			this.AMBE.Controls.Add(this.VUMeter);
			this.AMBE.Controls.Add(this.VOXcheckBox);
			this.AMBE.Controls.Add(this.VOXLevelTrackBar);
			this.AMBE.Name = "AMBE";
			this.AMBE.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.label31, "label31");
			this.label31.Name = "label31";
			componentResourceManager.ApplyResources(this.label32, "label32");
			this.label32.Name = "label32";
			componentResourceManager.ApplyResources(this.NXDNinlabel, "NXDNinlabel");
			this.NXDNinlabel.Name = "NXDNinlabel";
			componentResourceManager.ApplyResources(this.NXDNoutlabel, "NXDNoutlabel");
			this.NXDNoutlabel.Name = "NXDNoutlabel";
			componentResourceManager.ApplyResources(this.NXDNintrackBar, "NXDNintrackBar");
			this.NXDNintrackBar.Maximum = 30;
			this.NXDNintrackBar.Minimum = -30;
			this.NXDNintrackBar.Name = "NXDNintrackBar";
			this.NXDNintrackBar.TickStyle = global::System.Windows.Forms.TickStyle.None;
			this.NXDNintrackBar.Scroll += new global::System.EventHandler(this.NXDNintrackBar_Scroll);
			componentResourceManager.ApplyResources(this.NXDNouttrackBar, "NXDNouttrackBar");
			this.NXDNouttrackBar.Maximum = 30;
			this.NXDNouttrackBar.Minimum = -30;
			this.NXDNouttrackBar.Name = "NXDNouttrackBar";
			this.NXDNouttrackBar.TickStyle = global::System.Windows.Forms.TickStyle.None;
			this.NXDNouttrackBar.Scroll += new global::System.EventHandler(this.NXDNouttrackBar_Scroll);
			componentResourceManager.ApplyResources(this.label35, "label35");
			this.label35.Name = "label35";
			this.FusionGoupBox.Controls.Add(this.label21);
			this.FusionGoupBox.Controls.Add(this.DGIDComboBox);
			componentResourceManager.ApplyResources(this.FusionGoupBox, "FusionGoupBox");
			this.FusionGoupBox.Name = "FusionGoupBox";
			this.FusionGoupBox.TabStop = false;
			componentResourceManager.ApplyResources(this.label21, "label21");
			this.label21.Name = "label21";
			componentResourceManager.ApplyResources(this.DGIDComboBox, "DGIDComboBox");
			this.DGIDComboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.DGIDComboBox.FormattingEnabled = true;
			this.DGIDComboBox.Items.AddRange(new object[]
			{
				componentResourceManager.GetString("DGIDComboBox.Items"),
				componentResourceManager.GetString("DGIDComboBox.Items1"),
				componentResourceManager.GetString("DGIDComboBox.Items2"),
				componentResourceManager.GetString("DGIDComboBox.Items3"),
				componentResourceManager.GetString("DGIDComboBox.Items4"),
				componentResourceManager.GetString("DGIDComboBox.Items5"),
				componentResourceManager.GetString("DGIDComboBox.Items6"),
				componentResourceManager.GetString("DGIDComboBox.Items7"),
				componentResourceManager.GetString("DGIDComboBox.Items8"),
				componentResourceManager.GetString("DGIDComboBox.Items9"),
				componentResourceManager.GetString("DGIDComboBox.Items10"),
				componentResourceManager.GetString("DGIDComboBox.Items11"),
				componentResourceManager.GetString("DGIDComboBox.Items12"),
				componentResourceManager.GetString("DGIDComboBox.Items13"),
				componentResourceManager.GetString("DGIDComboBox.Items14"),
				componentResourceManager.GetString("DGIDComboBox.Items15"),
				componentResourceManager.GetString("DGIDComboBox.Items16"),
				componentResourceManager.GetString("DGIDComboBox.Items17"),
				componentResourceManager.GetString("DGIDComboBox.Items18"),
				componentResourceManager.GetString("DGIDComboBox.Items19"),
				componentResourceManager.GetString("DGIDComboBox.Items20"),
				componentResourceManager.GetString("DGIDComboBox.Items21"),
				componentResourceManager.GetString("DGIDComboBox.Items22"),
				componentResourceManager.GetString("DGIDComboBox.Items23"),
				componentResourceManager.GetString("DGIDComboBox.Items24"),
				componentResourceManager.GetString("DGIDComboBox.Items25"),
				componentResourceManager.GetString("DGIDComboBox.Items26"),
				componentResourceManager.GetString("DGIDComboBox.Items27"),
				componentResourceManager.GetString("DGIDComboBox.Items28"),
				componentResourceManager.GetString("DGIDComboBox.Items29"),
				componentResourceManager.GetString("DGIDComboBox.Items30"),
				componentResourceManager.GetString("DGIDComboBox.Items31"),
				componentResourceManager.GetString("DGIDComboBox.Items32"),
				componentResourceManager.GetString("DGIDComboBox.Items33"),
				componentResourceManager.GetString("DGIDComboBox.Items34"),
				componentResourceManager.GetString("DGIDComboBox.Items35"),
				componentResourceManager.GetString("DGIDComboBox.Items36"),
				componentResourceManager.GetString("DGIDComboBox.Items37"),
				componentResourceManager.GetString("DGIDComboBox.Items38"),
				componentResourceManager.GetString("DGIDComboBox.Items39"),
				componentResourceManager.GetString("DGIDComboBox.Items40"),
				componentResourceManager.GetString("DGIDComboBox.Items41"),
				componentResourceManager.GetString("DGIDComboBox.Items42"),
				componentResourceManager.GetString("DGIDComboBox.Items43"),
				componentResourceManager.GetString("DGIDComboBox.Items44"),
				componentResourceManager.GetString("DGIDComboBox.Items45"),
				componentResourceManager.GetString("DGIDComboBox.Items46"),
				componentResourceManager.GetString("DGIDComboBox.Items47"),
				componentResourceManager.GetString("DGIDComboBox.Items48"),
				componentResourceManager.GetString("DGIDComboBox.Items49"),
				componentResourceManager.GetString("DGIDComboBox.Items50"),
				componentResourceManager.GetString("DGIDComboBox.Items51"),
				componentResourceManager.GetString("DGIDComboBox.Items52"),
				componentResourceManager.GetString("DGIDComboBox.Items53"),
				componentResourceManager.GetString("DGIDComboBox.Items54"),
				componentResourceManager.GetString("DGIDComboBox.Items55"),
				componentResourceManager.GetString("DGIDComboBox.Items56"),
				componentResourceManager.GetString("DGIDComboBox.Items57"),
				componentResourceManager.GetString("DGIDComboBox.Items58"),
				componentResourceManager.GetString("DGIDComboBox.Items59"),
				componentResourceManager.GetString("DGIDComboBox.Items60"),
				componentResourceManager.GetString("DGIDComboBox.Items61"),
				componentResourceManager.GetString("DGIDComboBox.Items62"),
				componentResourceManager.GetString("DGIDComboBox.Items63"),
				componentResourceManager.GetString("DGIDComboBox.Items64"),
				componentResourceManager.GetString("DGIDComboBox.Items65"),
				componentResourceManager.GetString("DGIDComboBox.Items66"),
				componentResourceManager.GetString("DGIDComboBox.Items67"),
				componentResourceManager.GetString("DGIDComboBox.Items68"),
				componentResourceManager.GetString("DGIDComboBox.Items69"),
				componentResourceManager.GetString("DGIDComboBox.Items70"),
				componentResourceManager.GetString("DGIDComboBox.Items71"),
				componentResourceManager.GetString("DGIDComboBox.Items72"),
				componentResourceManager.GetString("DGIDComboBox.Items73"),
				componentResourceManager.GetString("DGIDComboBox.Items74"),
				componentResourceManager.GetString("DGIDComboBox.Items75"),
				componentResourceManager.GetString("DGIDComboBox.Items76"),
				componentResourceManager.GetString("DGIDComboBox.Items77"),
				componentResourceManager.GetString("DGIDComboBox.Items78"),
				componentResourceManager.GetString("DGIDComboBox.Items79"),
				componentResourceManager.GetString("DGIDComboBox.Items80"),
				componentResourceManager.GetString("DGIDComboBox.Items81"),
				componentResourceManager.GetString("DGIDComboBox.Items82"),
				componentResourceManager.GetString("DGIDComboBox.Items83"),
				componentResourceManager.GetString("DGIDComboBox.Items84"),
				componentResourceManager.GetString("DGIDComboBox.Items85"),
				componentResourceManager.GetString("DGIDComboBox.Items86"),
				componentResourceManager.GetString("DGIDComboBox.Items87"),
				componentResourceManager.GetString("DGIDComboBox.Items88"),
				componentResourceManager.GetString("DGIDComboBox.Items89"),
				componentResourceManager.GetString("DGIDComboBox.Items90"),
				componentResourceManager.GetString("DGIDComboBox.Items91"),
				componentResourceManager.GetString("DGIDComboBox.Items92"),
				componentResourceManager.GetString("DGIDComboBox.Items93"),
				componentResourceManager.GetString("DGIDComboBox.Items94"),
				componentResourceManager.GetString("DGIDComboBox.Items95"),
				componentResourceManager.GetString("DGIDComboBox.Items96"),
				componentResourceManager.GetString("DGIDComboBox.Items97"),
				componentResourceManager.GetString("DGIDComboBox.Items98"),
				componentResourceManager.GetString("DGIDComboBox.Items99")
			});
			this.DGIDComboBox.Name = "DGIDComboBox";
			this.DGIDComboBox.SelectedIndexChanged += new global::System.EventHandler(this.DGIDComboBox_SelectedIndexChanged);
			this.groupBox1.Controls.Add(this.TGLookupLabel);
			this.groupBox1.Controls.Add(this.DMRManualDialComboBox1);
			this.groupBox1.Controls.Add(this.toggleSwitchGroupPrivate);
			componentResourceManager.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			componentResourceManager.ApplyResources(this.TGLookupLabel, "TGLookupLabel");
			this.TGLookupLabel.Name = "TGLookupLabel";
			componentResourceManager.ApplyResources(this.VOXlabel, "VOXlabel");
			this.VOXlabel.Name = "VOXlabel";
			componentResourceManager.ApplyResources(this.hangLabel, "hangLabel");
			this.hangLabel.Name = "hangLabel";
			componentResourceManager.ApplyResources(this.label20, "label20");
			this.label20.Name = "label20";
			componentResourceManager.ApplyResources(this.HangTimeLabel, "HangTimeLabel");
			this.HangTimeLabel.Name = "HangTimeLabel";
			componentResourceManager.ApplyResources(this.label23, "label23");
			this.label23.Name = "label23";
			componentResourceManager.ApplyResources(this.label22, "label22");
			this.label22.Name = "label22";
			componentResourceManager.ApplyResources(this.FUSIONinlabel, "FUSIONinlabel");
			this.FUSIONinlabel.Name = "FUSIONinlabel";
			componentResourceManager.ApplyResources(this.FUSIONoutlabel, "FUSIONoutlabel");
			this.FUSIONoutlabel.Name = "FUSIONoutlabel";
			componentResourceManager.ApplyResources(this.FUSIONintrackBar, "FUSIONintrackBar");
			this.FUSIONintrackBar.Maximum = 30;
			this.FUSIONintrackBar.Minimum = -30;
			this.FUSIONintrackBar.Name = "FUSIONintrackBar";
			this.FUSIONintrackBar.TickStyle = global::System.Windows.Forms.TickStyle.None;
			this.FUSIONintrackBar.Scroll += new global::System.EventHandler(this.FUSIONintrackBar_Scroll);
			componentResourceManager.ApplyResources(this.FUSIONouttrackBar, "FUSIONouttrackBar");
			this.FUSIONouttrackBar.Maximum = 30;
			this.FUSIONouttrackBar.Minimum = -30;
			this.FUSIONouttrackBar.Name = "FUSIONouttrackBar";
			this.FUSIONouttrackBar.TickStyle = global::System.Windows.Forms.TickStyle.None;
			this.FUSIONouttrackBar.Scroll += new global::System.EventHandler(this.FUSIONouttrackBar_Scroll);
			componentResourceManager.ApplyResources(this.label19, "label19");
			this.label19.Name = "label19";
			componentResourceManager.ApplyResources(this.simpleModeCheckBox, "simpleModeCheckBox");
			this.simpleModeCheckBox.Name = "simpleModeCheckBox";
			this.simpleModeCheckBox.UseVisualStyleBackColor = true;
			this.simpleModeCheckBox.CheckedChanged += new global::System.EventHandler(this.simpleModeCheckBox_CheckedChanged);
			componentResourceManager.ApplyResources(this.helplinkLabel, "helplinkLabel");
			this.helplinkLabel.Name = "helplinkLabel";
			this.helplinkLabel.TabStop = true;
			this.helplinkLabel.LinkClicked += new global::System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.helplinkLabel_LinkClicked);
			componentResourceManager.ApplyResources(this.hangTimeTrackBar, "hangTimeTrackBar");
			this.hangTimeTrackBar.LargeChange = 500;
			this.hangTimeTrackBar.Maximum = 4000;
			this.hangTimeTrackBar.Minimum = 500;
			this.hangTimeTrackBar.Name = "hangTimeTrackBar";
			this.hangTimeTrackBar.SmallChange = 1000;
			this.hangTimeTrackBar.TickFrequency = 500;
			this.hangTimeTrackBar.TickStyle = global::System.Windows.Forms.TickStyle.None;
			this.hangTimeTrackBar.Value = 3000;
			this.hangTimeTrackBar.Scroll += new global::System.EventHandler(this.hangTimeTrackBar_Scroll);
			componentResourceManager.ApplyResources(this.label14, "label14");
			this.label14.Name = "label14";
			componentResourceManager.ApplyResources(this.DMRGainlabel, "DMRGainlabel");
			this.DMRGainlabel.Name = "DMRGainlabel";
			componentResourceManager.ApplyResources(this.label12, "label12");
			this.label12.Name = "label12";
			componentResourceManager.ApplyResources(this.label11, "label11");
			this.label11.Name = "label11";
			componentResourceManager.ApplyResources(this.label10, "label10");
			this.label10.Name = "label10";
			componentResourceManager.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			componentResourceManager.ApplyResources(this.DMRinlabel, "DMRinlabel");
			this.DMRinlabel.Name = "DMRinlabel";
			componentResourceManager.ApplyResources(this.DMRoutlabel, "DMRoutlabel");
			this.DMRoutlabel.Name = "DMRoutlabel";
			componentResourceManager.ApplyResources(this.DSTARoutlabel, "DSTARoutlabel");
			this.DSTARoutlabel.Name = "DSTARoutlabel";
			componentResourceManager.ApplyResources(this.DSTARinlabel, "DSTARinlabel");
			this.DSTARinlabel.Name = "DSTARinlabel";
			componentResourceManager.ApplyResources(this.DSTARintrackBar, "DSTARintrackBar");
			this.DSTARintrackBar.Maximum = 30;
			this.DSTARintrackBar.Minimum = -30;
			this.DSTARintrackBar.Name = "DSTARintrackBar";
			this.DSTARintrackBar.TickStyle = global::System.Windows.Forms.TickStyle.None;
			this.DSTARintrackBar.Scroll += new global::System.EventHandler(this.DSTARintrackBar_Scroll);
			componentResourceManager.ApplyResources(this.DSTARouttrackBar, "DSTARouttrackBar");
			this.DSTARouttrackBar.Maximum = 30;
			this.DSTARouttrackBar.Minimum = -30;
			this.DSTARouttrackBar.Name = "DSTARouttrackBar";
			this.DSTARouttrackBar.TickStyle = global::System.Windows.Forms.TickStyle.None;
			this.DSTARouttrackBar.Scroll += new global::System.EventHandler(this.DSTARouttrackBar_Scroll);
			componentResourceManager.ApplyResources(this.DMRintrackBar, "DMRintrackBar");
			this.DMRintrackBar.Maximum = 30;
			this.DMRintrackBar.Minimum = -30;
			this.DMRintrackBar.Name = "DMRintrackBar";
			this.DMRintrackBar.TickStyle = global::System.Windows.Forms.TickStyle.None;
			this.DMRintrackBar.Scroll += new global::System.EventHandler(this.DMRintrackBar_Scroll);
			componentResourceManager.ApplyResources(this.DMRouttrackBar, "DMRouttrackBar");
			this.DMRouttrackBar.Maximum = 30;
			this.DMRouttrackBar.Minimum = -30;
			this.DMRouttrackBar.Name = "DMRouttrackBar";
			this.DMRouttrackBar.TickStyle = global::System.Windows.Forms.TickStyle.None;
			this.DMRouttrackBar.Scroll += new global::System.EventHandler(this.DMRouttrackBar_Scroll);
			componentResourceManager.ApplyResources(this.tabPage3, "tabPage3");
			this.tabPage3.BackgroundImage = global::BlueDV.Properties.Resources.image1_nr3;
			this.tabPage3.Controls.Add(this.CallookupListView);
			this.tabPage3.Controls.Add(this.label16);
			this.tabPage3.Controls.Add(this.CallLookupTextBox);
			this.tabPage3.Controls.Add(this.label15);
			this.tabPage3.Controls.Add(this.label13);
			this.tabPage3.Controls.Add(this.SearchTextBox);
			this.tabPage3.Controls.Add(this.TGlistView);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.UseVisualStyleBackColor = true;
			this.CallookupListView.AutoArrange = false;
			this.CallookupListView.BackColor = global::System.Drawing.SystemColors.GradientInactiveCaption;
			this.CallookupListView.Columns.AddRange(new global::System.Windows.Forms.ColumnHeader[] { this.columnHeader3, this.columnHeader4, this.DMRid });
			this.CallookupListView.FullRowSelect = true;
			this.CallookupListView.GridLines = true;
			this.CallookupListView.HeaderStyle = global::System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.CallookupListView.HideSelection = false;
			componentResourceManager.ApplyResources(this.CallookupListView, "CallookupListView");
			this.CallookupListView.MultiSelect = false;
			this.CallookupListView.Name = "CallookupListView";
			this.CallookupListView.UseCompatibleStateImageBehavior = false;
			this.CallookupListView.View = global::System.Windows.Forms.View.Details;
			this.CallookupListView.Click += new global::System.EventHandler(this.CallookupListView_Click);
			componentResourceManager.ApplyResources(this.columnHeader3, "columnHeader3");
			componentResourceManager.ApplyResources(this.columnHeader4, "columnHeader4");
			componentResourceManager.ApplyResources(this.DMRid, "DMRid");
			componentResourceManager.ApplyResources(this.label16, "label16");
			this.label16.Name = "label16";
			componentResourceManager.ApplyResources(this.CallLookupTextBox, "CallLookupTextBox");
			this.CallLookupTextBox.CharacterCasing = global::System.Windows.Forms.CharacterCasing.Upper;
			this.CallLookupTextBox.Name = "CallLookupTextBox";
			this.CallLookupTextBox.TextChanged += new global::System.EventHandler(this.CallLookupTextBox_TextChanged);
			componentResourceManager.ApplyResources(this.label15, "label15");
			this.label15.Name = "label15";
			componentResourceManager.ApplyResources(this.label13, "label13");
			this.label13.Name = "label13";
			componentResourceManager.ApplyResources(this.SearchTextBox, "SearchTextBox");
			this.SearchTextBox.Name = "SearchTextBox";
			this.SearchTextBox.TextChanged += new global::System.EventHandler(this.SearchTextBox_TextChanged);
			this.TGlistView.AutoArrange = false;
			this.TGlistView.BackColor = global::System.Drawing.SystemColors.GradientInactiveCaption;
			this.TGlistView.Columns.AddRange(new global::System.Windows.Forms.ColumnHeader[] { this.columnHeader1, this.columnHeader2 });
			this.TGlistView.FullRowSelect = true;
			this.TGlistView.GridLines = true;
			this.TGlistView.HeaderStyle = global::System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.TGlistView.HideSelection = false;
			componentResourceManager.ApplyResources(this.TGlistView, "TGlistView");
			this.TGlistView.MultiSelect = false;
			this.TGlistView.Name = "TGlistView";
			this.TGlistView.UseCompatibleStateImageBehavior = false;
			this.TGlistView.View = global::System.Windows.Forms.View.Details;
			this.TGlistView.Click += new global::System.EventHandler(this.TGlistView_Click);
			componentResourceManager.ApplyResources(this.columnHeader1, "columnHeader1");
			componentResourceManager.ApplyResources(this.columnHeader2, "columnHeader2");
			componentResourceManager.ApplyResources(this.APRSchatTab, "APRSchatTab");
			this.APRSchatTab.BackgroundImage = global::BlueDV.Properties.Resources.image1_nr3;
			this.APRSchatTab.Controls.Add(this.APRSChatCallComboBox);
			this.APRSchatTab.Controls.Add(this.HelplinkLabel1);
			this.APRSchatTab.Controls.Add(this.label18);
			this.APRSchatTab.Controls.Add(this.label17);
			this.APRSchatTab.Controls.Add(this.MessagetextBox);
			this.APRSchatTab.Controls.Add(this.ChattextBox);
			this.APRSchatTab.Name = "APRSchatTab";
			this.APRSchatTab.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.APRSChatCallComboBox, "APRSChatCallComboBox");
			this.APRSChatCallComboBox.FormattingEnabled = true;
			this.APRSChatCallComboBox.Name = "APRSChatCallComboBox";
			this.APRSChatCallComboBox.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.APRSChatCallComboBox_KeyPress);
			componentResourceManager.ApplyResources(this.HelplinkLabel1, "HelplinkLabel1");
			this.HelplinkLabel1.BackColor = global::System.Drawing.Color.Transparent;
			this.HelplinkLabel1.Name = "HelplinkLabel1";
			this.HelplinkLabel1.TabStop = true;
			this.HelplinkLabel1.LinkClicked += new global::System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HelplinkLabel1_LinkClicked);
			componentResourceManager.ApplyResources(this.label18, "label18");
			this.label18.BackColor = global::System.Drawing.Color.Transparent;
			this.label18.Name = "label18";
			componentResourceManager.ApplyResources(this.label17, "label17");
			this.label17.BackColor = global::System.Drawing.Color.Transparent;
			this.label17.Name = "label17";
			componentResourceManager.ApplyResources(this.MessagetextBox, "MessagetextBox");
			this.MessagetextBox.Name = "MessagetextBox";
			this.MessagetextBox.KeyDown += new global::System.Windows.Forms.KeyEventHandler(this.messageKeyDown);
			this.MessagetextBox.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.messageKeyPress);
			componentResourceManager.ApplyResources(this.ChattextBox, "ChattextBox");
			this.ChattextBox.Name = "ChattextBox";
			this.ChattextBox.ReadOnly = true;
			this.ChattextBox.TextChanged += new global::System.EventHandler(this.ChattextBox_TextChanged);
			this.ChattextBox.MouseDown += new global::System.Windows.Forms.MouseEventHandler(this.ChattextBox_MouseDown);
			componentResourceManager.ApplyResources(this.checkBox1, "checkBox1");
			this.checkBox1.BackColor = global::System.Drawing.Color.Transparent;
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.UseVisualStyleBackColor = false;
			this.checkBox1.CheckedChanged += new global::System.EventHandler(this.checkBox1_CheckedChanged);
			componentResourceManager.ApplyResources(this.newVersionLinkLabel, "newVersionLinkLabel");
			this.newVersionLinkLabel.BackColor = global::System.Drawing.Color.Transparent;
			this.newVersionLinkLabel.Name = "newVersionLinkLabel";
			this.newVersionLinkLabel.TabStop = true;
			this.newVersionLinkLabel.LinkClicked += new global::System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.newVersionLinkLabel_LinkClicked);
			componentResourceManager.ApplyResources(this.thanksTo, "thanksTo");
			this.thanksTo.BackColor = global::System.Drawing.Color.Transparent;
			this.thanksTo.Name = "thanksTo";
			componentResourceManager.ApplyResources(this.radioButtonXLX, "radioButtonXLX");
			this.radioButtonXLX.BackColor = global::System.Drawing.Color.Transparent;
			this.radioButtonXLX.Name = "radioButtonXLX";
			this.radioButtonXLX.UseVisualStyleBackColor = false;
			this.radioButtonXLX.CheckedChanged += new global::System.EventHandler(this.radioButtonXLX_CheckedChanged);
			this.XLXURLpictureBox.BackColor = global::System.Drawing.Color.Transparent;
			this.XLXURLpictureBox.BackgroundImage = global::BlueDV.Properties.Resources.Very_Basic_Link_icon;
			componentResourceManager.ApplyResources(this.XLXURLpictureBox, "XLXURLpictureBox");
			this.XLXURLpictureBox.Image = global::BlueDV.Properties.Resources.Very_Basic_Link_icon;
			this.XLXURLpictureBox.Name = "XLXURLpictureBox";
			this.XLXURLpictureBox.TabStop = false;
			this.XLXURLpictureBox.Click += new global::System.EventHandler(this.XLXURLpictureBox_Click);
			componentResourceManager.ApplyResources(this.radioButtonJPN, "radioButtonJPN");
			this.radioButtonJPN.BackColor = global::System.Drawing.Color.Transparent;
			this.radioButtonJPN.Name = "radioButtonJPN";
			this.radioButtonJPN.UseVisualStyleBackColor = false;
			this.radioButtonJPN.CheckedChanged += new global::System.EventHandler(this.radioButtonJPN_CheckedChanged);
			this.DMRAMBEpictureBox.BackColor = global::System.Drawing.Color.Transparent;
			componentResourceManager.ApplyResources(this.DMRAMBEpictureBox, "DMRAMBEpictureBox");
			this.DMRAMBEpictureBox.Image = global::BlueDV.Properties.Resources.LED_OFF;
			this.DMRAMBEpictureBox.Name = "DMRAMBEpictureBox";
			this.DMRAMBEpictureBox.TabStop = false;
			this.DSTARAMBEpictureBox.BackColor = global::System.Drawing.Color.Transparent;
			componentResourceManager.ApplyResources(this.DSTARAMBEpictureBox, "DSTARAMBEpictureBox");
			this.DSTARAMBEpictureBox.Image = global::BlueDV.Properties.Resources.LED_OFF;
			this.DSTARAMBEpictureBox.Name = "DSTARAMBEpictureBox";
			this.DSTARAMBEpictureBox.TabStop = false;
			this.FUSIONAMBEpictureBox.BackColor = global::System.Drawing.Color.Transparent;
			componentResourceManager.ApplyResources(this.FUSIONAMBEpictureBox, "FUSIONAMBEpictureBox");
			this.FUSIONAMBEpictureBox.Image = global::BlueDV.Properties.Resources.LED_OFF;
			this.FUSIONAMBEpictureBox.Name = "FUSIONAMBEpictureBox";
			this.FUSIONAMBEpictureBox.TabStop = false;
			componentResourceManager.ApplyResources(this.donatePictureBox, "donatePictureBox");
			this.donatePictureBox.BackColor = global::System.Drawing.Color.Transparent;
			this.donatePictureBox.Name = "donatePictureBox";
			this.donatePictureBox.TabStop = false;
			this.donatePictureBox.Click += new global::System.EventHandler(this.donatePictureBox_Click);
			componentResourceManager.ApplyResources(this.NXDNPanel, "NXDNPanel");
			this.NXDNPanel.BackColor = global::System.Drawing.Color.Transparent;
			this.NXDNPanel.BackgroundImage = global::BlueDV.Properties.Resources.bluedvwinbluebgrnd;
			this.NXDNPanel.Controls.Add(this.label26);
			this.NXDNPanel.Controls.Add(this.dynamicHisNXDNCallBox);
			this.NXDNPanel.Controls.Add(this.dynamicNXDNstatus);
			this.NXDNPanel.Controls.Add(this.label29);
			this.NXDNPanel.Controls.Add(this.label30);
			this.NXDNPanel.Name = "NXDNPanel";
			this.NXDNPanel.DoubleClick += new global::System.EventHandler(this.NXDNPanel_DoubleClick);
			componentResourceManager.ApplyResources(this.label26, "label26");
			this.label26.Name = "label26";
			componentResourceManager.ApplyResources(this.dynamicHisNXDNCallBox, "dynamicHisNXDNCallBox");
			this.dynamicHisNXDNCallBox.Name = "dynamicHisNXDNCallBox";
			componentResourceManager.ApplyResources(this.dynamicNXDNstatus, "dynamicNXDNstatus");
			this.dynamicNXDNstatus.Name = "dynamicNXDNstatus";
			componentResourceManager.ApplyResources(this.label29, "label29");
			this.label29.Name = "label29";
			componentResourceManager.ApplyResources(this.label30, "label30");
			this.label30.Name = "label30";
			componentResourceManager.ApplyResources(this.toggleSwitchNXDNconnect, "toggleSwitchNXDNconnect");
			this.toggleSwitchNXDNconnect.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.toggleSwitchNXDNconnect.BackColor = global::System.Drawing.Color.Transparent;
			this.toggleSwitchNXDNconnect.Name = "toggleSwitchNXDNconnect";
			this.toggleSwitchNXDNconnect.OffFont = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.toggleSwitchNXDNconnect.OnFont = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.toggleSwitchNXDNconnect.Style = global::JCS.ToggleSwitch.ToggleSwitchStyle.Iphone;
			this.toggleSwitchNXDNconnect.CheckedChanged += new global::JCS.ToggleSwitch.CheckedChangedDelegate(this.toggleSwitchNXDNconnect_CheckedChanged);
			componentResourceManager.ApplyResources(this.label36, "label36");
			this.label36.BackColor = global::System.Drawing.Color.Transparent;
			this.label36.Name = "label36";
			this.NXDNAMBEpictureBox.BackColor = global::System.Drawing.Color.Transparent;
			componentResourceManager.ApplyResources(this.NXDNAMBEpictureBox, "NXDNAMBEpictureBox");
			this.NXDNAMBEpictureBox.Image = global::BlueDV.Properties.Resources.LED_OFF;
			this.NXDNAMBEpictureBox.Name = "NXDNAMBEpictureBox";
			this.NXDNAMBEpictureBox.TabStop = false;
			componentResourceManager.ApplyResources(this.NXDNAMBEButton, "NXDNAMBEButton");
			this.NXDNAMBEButton.GradientBottom = global::System.Drawing.Color.FromArgb(224, 224, 224);
			this.NXDNAMBEButton.GradientTop = global::System.Drawing.Color.Silver;
			this.NXDNAMBEButton.Name = "NXDNAMBEButton";
			this.NXDNAMBEButton.UseVisualStyleBackColor = true;
			this.NXDNAMBEButton.Click += new global::System.EventHandler(this.NXDNAMBEButton_Click);
			componentResourceManager.ApplyResources(this.FUSIONAMBEButton, "FUSIONAMBEButton");
			this.FUSIONAMBEButton.GradientBottom = global::System.Drawing.Color.FromArgb(224, 224, 224);
			this.FUSIONAMBEButton.GradientTop = global::System.Drawing.Color.Silver;
			this.FUSIONAMBEButton.Name = "FUSIONAMBEButton";
			this.FUSIONAMBEButton.UseVisualStyleBackColor = true;
			this.FUSIONAMBEButton.Click += new global::System.EventHandler(this.FUSIONAMBEButton_Click);
			componentResourceManager.ApplyResources(this.DSTARAMBEButton, "DSTARAMBEButton");
			this.DSTARAMBEButton.GradientBottom = global::System.Drawing.Color.FromArgb(224, 224, 224);
			this.DSTARAMBEButton.GradientTop = global::System.Drawing.Color.Silver;
			this.DSTARAMBEButton.Name = "DSTARAMBEButton";
			this.DSTARAMBEButton.UseVisualStyleBackColor = true;
			this.DSTARAMBEButton.Click += new global::System.EventHandler(this.DSTARAMBEButton_Click);
			componentResourceManager.ApplyResources(this.DMRAMBEButton, "DMRAMBEButton");
			this.DMRAMBEButton.GradientBottom = global::System.Drawing.Color.FromArgb(224, 224, 224);
			this.DMRAMBEButton.GradientTop = global::System.Drawing.Color.Silver;
			this.DMRAMBEButton.Name = "DMRAMBEButton";
			this.DMRAMBEButton.UseVisualStyleBackColor = true;
			this.DMRAMBEButton.Click += new global::System.EventHandler(this.DMRAMBEButton_Click);
			componentResourceManager.ApplyResources(this.unlinkButton, "unlinkButton");
			this.unlinkButton.Name = "unlinkButton";
			this.unlinkButton.UseVisualStyleBackColor = true;
			this.unlinkButton.Click += new global::System.EventHandler(this.unlinkButton_Click);
			componentResourceManager.ApplyResources(this.linkButton, "linkButton");
			this.linkButton.Name = "linkButton";
			this.linkButton.UseVisualStyleBackColor = true;
			this.linkButton.Click += new global::System.EventHandler(this.linkButton_Click);
			componentResourceManager.ApplyResources(this, "$this");
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackgroundImage = global::BlueDV.Properties.Resources.bluedv_background;
			base.Controls.Add(this.NXDNAMBEpictureBox);
			base.Controls.Add(this.NXDNAMBEButton);
			base.Controls.Add(this.label36);
			base.Controls.Add(this.toggleSwitchNXDNconnect);
			base.Controls.Add(this.NXDNPanel);
			base.Controls.Add(this.donatePictureBox);
			base.Controls.Add(this.FUSIONAMBEpictureBox);
			base.Controls.Add(this.DSTARAMBEpictureBox);
			base.Controls.Add(this.DMRAMBEpictureBox);
			base.Controls.Add(this.FUSIONAMBEButton);
			base.Controls.Add(this.DSTARAMBEButton);
			base.Controls.Add(this.DMRAMBEButton);
			base.Controls.Add(this.radioButtonJPN);
			base.Controls.Add(this.XLXURLpictureBox);
			base.Controls.Add(this.radioButtonXLX);
			base.Controls.Add(this.thanksTo);
			base.Controls.Add(this.newVersionLinkLabel);
			base.Controls.Add(this.checkBox1);
			base.Controls.Add(this.dynLastReflector);
			base.Controls.Add(this.killTimerLabel);
			base.Controls.Add(this.tabControl1);
			base.Controls.Add(this.staticSerial);
			base.Controls.Add(this.toggleSwitchSerialConnect);
			base.Controls.Add(this.DMRPanel);
			base.Controls.Add(this.onAIRswitch);
			base.Controls.Add(this.comboBoxModeSelector);
			base.Controls.Add(this.DSTARPanel);
			base.Controls.Add(this.FusionPanel);
			base.Controls.Add(this.Panel1);
			base.Controls.Add(this.radioButtonXRF);
			base.Controls.Add(this.radioButtonDCS);
			base.Controls.Add(this.radioButtonREF);
			base.Controls.Add(this.unlinkButton);
			base.Controls.Add(this.comboBoxReflectorModule);
			base.Controls.Add(this.linkButton);
			base.Controls.Add(this.comboBoxReflectorList);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.toggleSwitchFusionconnect);
			base.Controls.Add(this.toggleSwitchDSTARconnect);
			base.Controls.Add(this.staticDMR);
			base.Controls.Add(this.toggleSwitchDMRconnect);
			base.Controls.Add(this.cpuinfo);
			base.Controls.Add(this.version);
			base.Controls.Add(this.bypa7lim);
			base.Controls.Add(this.menuStrip1);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.Fixed3D;
			base.KeyPreview = true;
			base.MainMenuStrip = this.menuStrip1;
			base.MaximizeBox = false;
			base.Name = "Form1";
			base.FormClosing += new global::System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			base.Load += new global::System.EventHandler(this.Form1_Load);
			base.KeyDown += new global::System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.Panel1.ResumeLayout(false);
			this.Panel1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.chatImage).EndInit();
			this.DMRPanel.ResumeLayout(false);
			this.DMRPanel.PerformLayout();
			this.DSTARPanel.ResumeLayout(false);
			this.DSTARPanel.PerformLayout();
			this.FusionPanel.ResumeLayout(false);
			this.FusionPanel.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.VOXLevelTrackBar).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.AMBE.ResumeLayout(false);
			this.AMBE.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.NXDNintrackBar).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.NXDNouttrackBar).EndInit();
			this.FusionGoupBox.ResumeLayout(false);
			this.FusionGoupBox.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.FUSIONintrackBar).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.FUSIONouttrackBar).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.hangTimeTrackBar).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.DSTARintrackBar).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.DSTARouttrackBar).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.DMRintrackBar).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.DMRouttrackBar).EndInit();
			this.tabPage3.ResumeLayout(false);
			this.tabPage3.PerformLayout();
			this.APRSchatTab.ResumeLayout(false);
			this.APRSchatTab.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.XLXURLpictureBox).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.DMRAMBEpictureBox).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.DSTARAMBEpictureBox).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.FUSIONAMBEpictureBox).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.donatePictureBox).EndInit();
			this.NXDNPanel.ResumeLayout(false);
			this.NXDNPanel.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.NXDNAMBEpictureBox).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x04000162 RID: 354
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000163 RID: 355
		private global::System.Windows.Forms.MenuStrip menuStrip1;

		// Token: 0x04000164 RID: 356
		private global::System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;

		// Token: 0x04000165 RID: 357
		private global::System.Windows.Forms.ToolStripMenuItem setupToolStripMenuItem;

		// Token: 0x04000166 RID: 358
		private global::System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;

		// Token: 0x04000167 RID: 359
		private global::System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;

		// Token: 0x04000168 RID: 360
		private global::System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;

		// Token: 0x04000169 RID: 361
		private global::System.Windows.Forms.Label hisDMRid;

		// Token: 0x0400016A RID: 362
		private global::System.Windows.Forms.Label callStaticLabel;

		// Token: 0x0400016B RID: 363
		private global::System.Windows.Forms.Label hisName;

		// Token: 0x0400016C RID: 364
		private global::System.Windows.Forms.Label nameStaticLabel;

		// Token: 0x0400016D RID: 365
		private global::System.Windows.Forms.Label hisCall;

		// Token: 0x0400016E RID: 366
		private global::System.Windows.Forms.Label dmridStaticLabel;

		// Token: 0x0400016F RID: 367
		private global::System.Windows.Forms.Label dynamicRXTX;

		// Token: 0x04000170 RID: 368
		private global::System.Windows.Forms.ListView listView1;

		// Token: 0x04000171 RID: 369
		private global::System.Windows.Forms.ColumnHeader Date;

		// Token: 0x04000172 RID: 370
		private global::System.Windows.Forms.ColumnHeader NiceName;

		// Token: 0x04000173 RID: 371
		private global::System.Windows.Forms.Panel Panel1;

		// Token: 0x04000174 RID: 372
		private global::System.Windows.Forms.Label frequency;

		// Token: 0x04000175 RID: 373
		private global::System.Windows.Forms.Label FrequencyLabel;

		// Token: 0x04000176 RID: 374
		private global::System.Windows.Forms.Label dmrmaster;

		// Token: 0x04000177 RID: 375
		private global::System.Windows.Forms.Label DMRmasterStatic;

		// Token: 0x04000178 RID: 376
		private global::System.Windows.Forms.Label Firmware;

		// Token: 0x04000179 RID: 377
		private global::System.Windows.Forms.Label firmwareLabel;

		// Token: 0x0400017A RID: 378
		private global::System.Windows.Forms.Label dynamicStatus;

		// Token: 0x0400017B RID: 379
		private global::System.Windows.Forms.Label staticStatus;

		// Token: 0x0400017C RID: 380
		private global::System.Windows.Forms.Label bypa7lim;

		// Token: 0x0400017D RID: 381
		private global::System.Windows.Forms.Label BER;

		// Token: 0x0400017E RID: 382
		private global::ProgressBarEx.ProgressBarEx RXprogressBarEx;

		// Token: 0x0400017F RID: 383
		private global::ProgressBarEx.ProgressBarEx TXprogressBarEx;

		// Token: 0x04000180 RID: 384
		private global::System.Windows.Forms.ColumnHeader Call;

		// Token: 0x04000181 RID: 385
		private global::System.Windows.Forms.Label RX;

		// Token: 0x04000182 RID: 386
		private global::System.Windows.Forms.Label TX;

		// Token: 0x04000183 RID: 387
		private global::System.Windows.Forms.Label version;

		// Token: 0x04000184 RID: 388
		private global::System.Windows.Forms.Label dynLastReflector;

		// Token: 0x04000185 RID: 389
		private global::System.Windows.Forms.Label lastConnectedReflector;

		// Token: 0x04000186 RID: 390
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x04000187 RID: 391
		private global::System.Windows.Forms.Label cpuinfo;

		// Token: 0x04000188 RID: 392
		private global::JCS.ToggleSwitch toggleSwitchDMRconnect;

		// Token: 0x04000189 RID: 393
		private global::System.Windows.Forms.Label staticDMR;

		// Token: 0x0400018A RID: 394
		private global::JCS.ToggleSwitch toggleSwitchDSTARconnect;

		// Token: 0x0400018B RID: 395
		private global::JCS.ToggleSwitch toggleSwitchFusionconnect;

		// Token: 0x0400018C RID: 396
		private global::System.Windows.Forms.Label label1;

		// Token: 0x0400018D RID: 397
		private global::System.Windows.Forms.Label label2;

		// Token: 0x0400018E RID: 398
		private global::System.Windows.Forms.ComboBox comboBoxReflectorList;

		// Token: 0x0400018F RID: 399
		private global::BlueDV.RoundButton linkButton;

		// Token: 0x04000190 RID: 400
		private global::System.Windows.Forms.ComboBox comboBoxReflectorModule;

		// Token: 0x04000191 RID: 401
		private global::BlueDV.RoundButton unlinkButton;

		// Token: 0x04000192 RID: 402
		private global::System.Windows.Forms.RadioButton radioButtonREF;

		// Token: 0x04000193 RID: 403
		private global::System.Windows.Forms.RadioButton radioButtonDCS;

		// Token: 0x04000194 RID: 404
		private global::System.Windows.Forms.RadioButton radioButtonXRF;

		// Token: 0x04000195 RID: 405
		private global::System.Windows.Forms.Label hisDSTARidsmall;

		// Token: 0x04000196 RID: 406
		private global::System.Windows.Forms.PictureBox chatImage;

		// Token: 0x04000197 RID: 407
		private global::System.Windows.Forms.Label dynamic_mode_text;

		// Token: 0x04000198 RID: 408
		private global::System.Windows.Forms.Panel DMRPanel;

		// Token: 0x04000199 RID: 409
		private global::System.Windows.Forms.Panel DSTARPanel;

		// Token: 0x0400019A RID: 410
		private global::System.Windows.Forms.Panel FusionPanel;

		// Token: 0x0400019B RID: 411
		private global::System.Windows.Forms.Label label4;

		// Token: 0x0400019C RID: 412
		private global::System.Windows.Forms.Label label5;

		// Token: 0x0400019D RID: 413
		private global::System.Windows.Forms.Label label6;

		// Token: 0x0400019E RID: 414
		private global::System.Windows.Forms.ComboBox comboBoxModeSelector;

		// Token: 0x0400019F RID: 415
		private global::System.Windows.Forms.Label dynamicDMRstatus;

		// Token: 0x040001A0 RID: 416
		private global::System.Windows.Forms.Label staticDMRstatus;

		// Token: 0x040001A1 RID: 417
		private global::System.Windows.Forms.Label dynamicDSTARstatus;

		// Token: 0x040001A2 RID: 418
		private global::System.Windows.Forms.Label staticDSTARstatus;

		// Token: 0x040001A3 RID: 419
		private global::System.Windows.Forms.Label dynamicFusionstatus;

		// Token: 0x040001A4 RID: 420
		private global::System.Windows.Forms.Label staticFusionstatus;

		// Token: 0x040001A5 RID: 421
		private global::JCS.ToggleSwitch toggleSwitchSerialConnect;

		// Token: 0x040001A6 RID: 422
		private global::System.Windows.Forms.Label staticSerial;

		// Token: 0x040001A7 RID: 423
		private global::System.Windows.Forms.Label dynamicHisDMRCallBox;

		// Token: 0x040001A8 RID: 424
		private global::System.Windows.Forms.Label dynamicHisDSTARCallBox;

		// Token: 0x040001A9 RID: 425
		private global::System.Windows.Forms.Label dynamicHisFUSIONCallBox;

		// Token: 0x040001AA RID: 426
		private global::System.Windows.Forms.Label label9;

		// Token: 0x040001AB RID: 427
		private global::System.Windows.Forms.Label label8;

		// Token: 0x040001AC RID: 428
		private global::System.Windows.Forms.Label label7;

		// Token: 0x040001AD RID: 429
		private global::System.Windows.Forms.ColumnHeader Mode;

		// Token: 0x040001AE RID: 430
		private global::System.Windows.Forms.Label dynamichisDMRdest;

		// Token: 0x040001AF RID: 431
		private global::System.Windows.Forms.Label killTimerLabel;

		// Token: 0x040001B0 RID: 432
		private global::JCS.ToggleSwitch onAIRswitch;

		// Token: 0x040001B1 RID: 433
		private global::System.Windows.Forms.ToolStripMenuItem updateToolStripMenuItem;

		// Token: 0x040001B2 RID: 434
		private global::System.Windows.Forms.ToolStripMenuItem updateDSTARHostsToolStripMenuItem;

		// Token: 0x040001B3 RID: 435
		private global::System.Windows.Forms.ToolStripMenuItem updateCallDatabaseToolStripMenuItem;

		// Token: 0x040001B4 RID: 436
		private global::System.Windows.Forms.ToolStripMenuItem updateBMMastersToolStripMenuItem;

		// Token: 0x040001B5 RID: 437
		private global::System.Windows.Forms.ToolStripMenuItem aMBEToolStripMenuItem;

		// Token: 0x040001B6 RID: 438
		private global::System.Windows.Forms.ToolStripComboBox soundInputToolStripMenuItem;

		// Token: 0x040001B7 RID: 439
		private global::System.Windows.Forms.ToolStripComboBox soundOutputToolStripMenuItem;

		// Token: 0x040001B8 RID: 440
		private global::System.Windows.Forms.AGauge VUMeter;

		// Token: 0x040001B9 RID: 441
		private global::JCS.ToggleSwitch toggleSwitchGroupPrivate;

		// Token: 0x040001BA RID: 442
		private global::System.Windows.Forms.ComboBox DMRManualDialComboBox1;

		// Token: 0x040001BB RID: 443
		private global::System.Windows.Forms.CheckBox VOXcheckBox;

		// Token: 0x040001BC RID: 444
		private global::System.Windows.Forms.TrackBar VOXLevelTrackBar;

		// Token: 0x040001BD RID: 445
		private global::System.Windows.Forms.TabControl tabControl1;

		// Token: 0x040001BE RID: 446
		private global::System.Windows.Forms.TabPage tabPage1;

		// Token: 0x040001BF RID: 447
		private global::System.Windows.Forms.TabPage AMBE;

		// Token: 0x040001C0 RID: 448
		private global::System.Windows.Forms.Label DMRinlabel;

		// Token: 0x040001C1 RID: 449
		private global::System.Windows.Forms.Label DMRoutlabel;

		// Token: 0x040001C2 RID: 450
		private global::System.Windows.Forms.Label DSTARoutlabel;

		// Token: 0x040001C3 RID: 451
		private global::System.Windows.Forms.Label DSTARinlabel;

		// Token: 0x040001C4 RID: 452
		private global::System.Windows.Forms.TrackBar DSTARintrackBar;

		// Token: 0x040001C5 RID: 453
		private global::System.Windows.Forms.TrackBar DSTARouttrackBar;

		// Token: 0x040001C6 RID: 454
		private global::System.Windows.Forms.TrackBar DMRintrackBar;

		// Token: 0x040001C7 RID: 455
		private global::System.Windows.Forms.TrackBar DMRouttrackBar;

		// Token: 0x040001C8 RID: 456
		private global::System.Windows.Forms.Label label14;

		// Token: 0x040001C9 RID: 457
		private global::System.Windows.Forms.Label DMRGainlabel;

		// Token: 0x040001CA RID: 458
		private global::System.Windows.Forms.Label label12;

		// Token: 0x040001CB RID: 459
		private global::System.Windows.Forms.Label label11;

		// Token: 0x040001CC RID: 460
		private global::System.Windows.Forms.Label label10;

		// Token: 0x040001CD RID: 461
		private global::System.Windows.Forms.Label label3;

		// Token: 0x040001CE RID: 462
		private global::System.Windows.Forms.TrackBar hangTimeTrackBar;

		// Token: 0x040001CF RID: 463
		private global::System.Windows.Forms.CheckBox checkBox1;

		// Token: 0x040001D0 RID: 464
		private global::System.Windows.Forms.LinkLabel helplinkLabel;

		// Token: 0x040001D1 RID: 465
		private global::System.Windows.Forms.LinkLabel newVersionLinkLabel;

		// Token: 0x040001D2 RID: 466
		private global::System.Windows.Forms.Label TGLookupLabel;

		// Token: 0x040001D3 RID: 467
		private global::System.Windows.Forms.TabPage tabPage3;

		// Token: 0x040001D4 RID: 468
		private global::System.Windows.Forms.ListView TGlistView;

		// Token: 0x040001D5 RID: 469
		private global::System.Windows.Forms.ColumnHeader columnHeader1;

		// Token: 0x040001D6 RID: 470
		private global::System.Windows.Forms.ColumnHeader columnHeader2;

		// Token: 0x040001D7 RID: 471
		private global::System.Windows.Forms.TextBox SearchTextBox;

		// Token: 0x040001D8 RID: 472
		private global::System.Windows.Forms.Label label15;

		// Token: 0x040001D9 RID: 473
		private global::System.Windows.Forms.Label label13;

		// Token: 0x040001DA RID: 474
		private global::System.Windows.Forms.Label label16;

		// Token: 0x040001DB RID: 475
		private global::System.Windows.Forms.TextBox CallLookupTextBox;

		// Token: 0x040001DC RID: 476
		private global::System.Windows.Forms.ListView CallookupListView;

		// Token: 0x040001DD RID: 477
		private global::System.Windows.Forms.ColumnHeader columnHeader3;

		// Token: 0x040001DE RID: 478
		private global::System.Windows.Forms.ColumnHeader columnHeader4;

		// Token: 0x040001DF RID: 479
		private global::System.Windows.Forms.ColumnHeader DMRid;

		// Token: 0x040001E0 RID: 480
		private global::System.Windows.Forms.CheckBox simpleModeCheckBox;

		// Token: 0x040001E1 RID: 481
		private global::System.Windows.Forms.Label thanksTo;

		// Token: 0x040001E2 RID: 482
		private global::System.Windows.Forms.TabPage APRSchatTab;

		// Token: 0x040001E3 RID: 483
		private global::System.Windows.Forms.ComboBox APRSChatCallComboBox;

		// Token: 0x040001E4 RID: 484
		private global::System.Windows.Forms.LinkLabel HelplinkLabel1;

		// Token: 0x040001E5 RID: 485
		private global::System.Windows.Forms.Label label18;

		// Token: 0x040001E6 RID: 486
		private global::System.Windows.Forms.Label label17;

		// Token: 0x040001E7 RID: 487
		private global::System.Windows.Forms.TextBox MessagetextBox;

		// Token: 0x040001E8 RID: 488
		private global::System.Windows.Forms.TextBox ChattextBox;

		// Token: 0x040001E9 RID: 489
		private global::System.Windows.Forms.RadioButton radioButtonXLX;

		// Token: 0x040001EA RID: 490
		private global::System.Windows.Forms.PictureBox XLXURLpictureBox;

		// Token: 0x040001EB RID: 491
		private global::System.Windows.Forms.Label label23;

		// Token: 0x040001EC RID: 492
		private global::System.Windows.Forms.Label label22;

		// Token: 0x040001ED RID: 493
		private global::System.Windows.Forms.Label FUSIONinlabel;

		// Token: 0x040001EE RID: 494
		private global::System.Windows.Forms.Label FUSIONoutlabel;

		// Token: 0x040001EF RID: 495
		private global::System.Windows.Forms.TrackBar FUSIONintrackBar;

		// Token: 0x040001F0 RID: 496
		private global::System.Windows.Forms.TrackBar FUSIONouttrackBar;

		// Token: 0x040001F1 RID: 497
		private global::System.Windows.Forms.Label label19;

		// Token: 0x040001F2 RID: 498
		private global::System.Windows.Forms.RadioButton radioButtonJPN;

		// Token: 0x040001F3 RID: 499
		private global::System.Windows.Forms.Label HangTimeLabel;

		// Token: 0x040001F4 RID: 500
		private global::System.Windows.Forms.Label label20;

		// Token: 0x040001F5 RID: 501
		private global::System.Windows.Forms.Label VOXlabel;

		// Token: 0x040001F6 RID: 502
		private global::System.Windows.Forms.Label hangLabel;

		// Token: 0x040001F7 RID: 503
		private global::BlueDV.RoundButton DMRAMBEButton;

		// Token: 0x040001F8 RID: 504
		private global::BlueDV.RoundButton DSTARAMBEButton;

		// Token: 0x040001F9 RID: 505
		private global::BlueDV.RoundButton FUSIONAMBEButton;

		// Token: 0x040001FA RID: 506
		private global::System.Windows.Forms.PictureBox DMRAMBEpictureBox;

		// Token: 0x040001FB RID: 507
		private global::System.Windows.Forms.PictureBox DSTARAMBEpictureBox;

		// Token: 0x040001FC RID: 508
		private global::System.Windows.Forms.PictureBox FUSIONAMBEpictureBox;

		// Token: 0x040001FD RID: 509
		private global::System.Windows.Forms.PictureBox donatePictureBox;

		// Token: 0x040001FE RID: 510
		private global::System.Windows.Forms.Label fusionModeLabel;

		// Token: 0x040001FF RID: 511
		private global::System.Windows.Forms.GroupBox groupBox1;

		// Token: 0x04000200 RID: 512
		private global::System.Windows.Forms.GroupBox FusionGoupBox;

		// Token: 0x04000201 RID: 513
		private global::System.Windows.Forms.ComboBox DGIDComboBox;

		// Token: 0x04000202 RID: 514
		private global::System.Windows.Forms.Label label21;

		// Token: 0x04000203 RID: 515
		private global::System.Windows.Forms.ToolStripMenuItem updateFusionMastersToolStripMenuItem;

		// Token: 0x04000204 RID: 516
		private global::System.Windows.Forms.Label label24;

		// Token: 0x04000205 RID: 517
		private global::System.Windows.Forms.Label label25;

		// Token: 0x04000206 RID: 518
		private global::System.Windows.Forms.Label label31;

		// Token: 0x04000207 RID: 519
		private global::System.Windows.Forms.Label label32;

		// Token: 0x04000208 RID: 520
		private global::System.Windows.Forms.Label NXDNinlabel;

		// Token: 0x04000209 RID: 521
		private global::System.Windows.Forms.Label NXDNoutlabel;

		// Token: 0x0400020A RID: 522
		private global::System.Windows.Forms.TrackBar NXDNintrackBar;

		// Token: 0x0400020B RID: 523
		private global::System.Windows.Forms.TrackBar NXDNouttrackBar;

		// Token: 0x0400020C RID: 524
		private global::System.Windows.Forms.Label label35;

		// Token: 0x0400020D RID: 525
		private global::System.Windows.Forms.Panel NXDNPanel;

		// Token: 0x0400020E RID: 526
		private global::System.Windows.Forms.Label label26;

		// Token: 0x0400020F RID: 527
		private global::System.Windows.Forms.Label dynamicHisNXDNCallBox;

		// Token: 0x04000210 RID: 528
		private global::System.Windows.Forms.Label dynamicNXDNstatus;

		// Token: 0x04000211 RID: 529
		private global::System.Windows.Forms.Label label29;

		// Token: 0x04000212 RID: 530
		private global::System.Windows.Forms.Label label30;

		// Token: 0x04000213 RID: 531
		private global::JCS.ToggleSwitch toggleSwitchNXDNconnect;

		// Token: 0x04000214 RID: 532
		private global::System.Windows.Forms.Label label36;

		// Token: 0x04000215 RID: 533
		private global::System.Windows.Forms.Label hisCountry;

		// Token: 0x04000216 RID: 534
		private global::System.Windows.Forms.Label hisCity;

		// Token: 0x04000217 RID: 535
		private global::System.Windows.Forms.PictureBox NXDNAMBEpictureBox;

		// Token: 0x04000218 RID: 536
		private global::BlueDV.RoundButton NXDNAMBEButton;

		// Token: 0x04000219 RID: 537
		private global::System.Windows.Forms.ToolStripMenuItem updateNXDNHostsToolStripMenuItem;
	}
}
