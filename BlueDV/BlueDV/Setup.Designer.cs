namespace BlueDV
{
	// Token: 0x02000049 RID: 73
	public partial class Setup : global::System.Windows.Forms.Form
	{
		// Token: 0x0600059D RID: 1437 RVA: 0x00031700 File Offset: 0x0002F900
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600059E RID: 1438 RVA: 0x00031720 File Offset: 0x0002F920
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BlueDV.Setup));
			this.mycall = new global::System.Windows.Forms.TextBox();
			this.label1 = new global::System.Windows.Forms.Label();
			this.save = new global::System.Windows.Forms.Button();
			this.cancel = new global::System.Windows.Forms.Button();
			this.label2 = new global::System.Windows.Forms.Label();
			this.dmrid = new global::System.Windows.Forms.TextBox();
			this.label3 = new global::System.Windows.Forms.Label();
			this.freqDMR = new global::System.Windows.Forms.TextBox();
			this.label4 = new global::System.Windows.Forms.Label();
			this.listBox1 = new global::System.Windows.Forms.ListBox();
			this.label5 = new global::System.Windows.Forms.Label();
			this.label6 = new global::System.Windows.Forms.Label();
			this.DMRpassword = new global::System.Windows.Forms.TextBox();
			this.label7 = new global::System.Windows.Forms.Label();
			this.comboBox1 = new global::System.Windows.Forms.ComboBox();
			this.comboBox2 = new global::System.Windows.Forms.ComboBox();
			this.DSTARmodule = new global::System.Windows.Forms.ComboBox();
			this.label9 = new global::System.Windows.Forms.Label();
			this.label10 = new global::System.Windows.Forms.Label();
			this.checkBoxsaveQSOlog = new global::System.Windows.Forms.CheckBox();
			this.label13 = new global::System.Windows.Forms.Label();
			this.QTHlocatorTextBox = new global::System.Windows.Forms.TextBox();
			this.groupBox1 = new global::System.Windows.Forms.GroupBox();
			this.groupBox13 = new global::System.Windows.Forms.GroupBox();
			this.TGIFPassword = new global::System.Windows.Forms.TextBox();
			this.label16 = new global::System.Windows.Forms.Label();
			this.label43 = new global::System.Windows.Forms.Label();
			this.DMRidSimpleTextBox = new global::System.Windows.Forms.TextBox();
			this.label32 = new global::System.Windows.Forms.Label();
			this.DMRtypeSelectionComboBox = new global::System.Windows.Forms.ComboBox();
			this.groupBox6 = new global::System.Windows.Forms.GroupBox();
			this.groupBox5 = new global::System.Windows.Forms.GroupBox();
			this.label15 = new global::System.Windows.Forms.Label();
			this.comboBoxDMRPlus = new global::System.Windows.Forms.ComboBox();
			this.label24 = new global::System.Windows.Forms.Label();
			this.noInbandDataCheckBox = new global::System.Windows.Forms.CheckBox();
			this.checkBoxDMREnabledAtBoot = new global::System.Windows.Forms.CheckBox();
			this.label19 = new global::System.Windows.Forms.Label();
			this.groupBox2 = new global::System.Windows.Forms.GroupBox();
			this.label20 = new global::System.Windows.Forms.Label();
			this.defaultDSTARReflectorTextBox = new global::System.Windows.Forms.TextBox();
			this.label17 = new global::System.Windows.Forms.Label();
			this.checkBoxDSTAREnabledAtBoot = new global::System.Windows.Forms.CheckBox();
			this.label11 = new global::System.Windows.Forms.Label();
			this.label18 = new global::System.Windows.Forms.Label();
			this.APRScheckBox = new global::System.Windows.Forms.CheckBox();
			this.groupBox3 = new global::System.Windows.Forms.GroupBox();
			this.invertDTRRadioCheckBox = new global::System.Windows.Forms.CheckBox();
			this.FrequencyWarninglabel = new global::System.Windows.Forms.Label();
			this.invertRXTXcheckBox = new global::System.Windows.Forms.CheckBox();
			this.invertRXTXLabel = new global::System.Windows.Forms.Label();
			this.label47 = new global::System.Windows.Forms.Label();
			this.languageComboBox = new global::System.Windows.Forms.ComboBox();
			this.label46 = new global::System.Windows.Forms.Label();
			this.alwaysOnTopcheckBox = new global::System.Windows.Forms.CheckBox();
			this.label45 = new global::System.Windows.Forms.Label();
			this.PlusMinLoncomboBox = new global::System.Windows.Forms.ComboBox();
			this.PlusMinLatcomboBox = new global::System.Windows.Forms.ComboBox();
			this.label28 = new global::System.Windows.Forms.Label();
			this.label27 = new global::System.Windows.Forms.Label();
			this.longitudeMaskedTextBox = new global::System.Windows.Forms.MaskedTextBox();
			this.latitudeMaskedTextBox = new global::System.Windows.Forms.MaskedTextBox();
			this.label26 = new global::System.Windows.Forms.Label();
			this.label25 = new global::System.Windows.Forms.Label();
			this.label23 = new global::System.Windows.Forms.Label();
			this.DVMEGAPowerTrackBar = new global::System.Windows.Forms.TrackBar();
			this.label21 = new global::System.Windows.Forms.Label();
			this.modeTimerRF = new global::System.Windows.Forms.TextBox();
			this.label8 = new global::System.Windows.Forms.Label();
			this.label14 = new global::System.Windows.Forms.Label();
			this.RXTXColorsCheckBox = new global::System.Windows.Forms.CheckBox();
			this.AMBEspeed = new global::System.Windows.Forms.ComboBox();
			this.label29 = new global::System.Windows.Forms.Label();
			this.dvdonglecheckBox = new global::System.Windows.Forms.CheckBox();
			this.modeTimerNet = new global::System.Windows.Forms.TextBox();
			this.startProtocolComboBox = new global::System.Windows.Forms.ComboBox();
			this.groupBox4 = new global::System.Windows.Forms.GroupBox();
			this.YSFradioButton = new global::System.Windows.Forms.RadioButton();
			this.FCSradioButton = new global::System.Windows.Forms.RadioButton();
			this.FCSReflectorModulecomboBox = new global::System.Windows.Forms.ComboBox();
			this.FCSReflectorHostcomboBox = new global::System.Windows.Forms.ComboBox();
			this.label31 = new global::System.Windows.Forms.Label();
			this.label30 = new global::System.Windows.Forms.Label();
			this.YSFreflectorListcomboBox = new global::System.Windows.Forms.ComboBox();
			this.label22 = new global::System.Windows.Forms.Label();
			this.checkBoxFusionEnabledAtBoot = new global::System.Windows.Forms.CheckBox();
			this.label12 = new global::System.Windows.Forms.Label();
			this.toolTip1 = new global::System.Windows.Forms.ToolTip(this.components);
			this.AMBEDMRid = new global::System.Windows.Forms.TextBox();
			this.comboBox3 = new global::System.Windows.Forms.ComboBox();
			this.COMportPTTcomboBox = new global::System.Windows.Forms.ComboBox();
			this.AMBEtypeComboBox = new global::System.Windows.Forms.ComboBox();
			this.AMBEBeepcheckBox = new global::System.Windows.Forms.CheckBox();
			this.groupBox7 = new global::System.Windows.Forms.GroupBox();
			this.label49 = new global::System.Windows.Forms.Label();
			this.label48 = new global::System.Windows.Forms.Label();
			this.DSTARSlowDataTextBox = new global::System.Windows.Forms.TextBox();
			this.killTimerComboBox = new global::System.Windows.Forms.ComboBox();
			this.label44 = new global::System.Windows.Forms.Label();
			this.groupBox8 = new global::System.Windows.Forms.GroupBox();
			this.groupBox12 = new global::System.Windows.Forms.GroupBox();
			this.PTTLOWradioButton = new global::System.Windows.Forms.RadioButton();
			this.PTTHIGHradioButton = new global::System.Windows.Forms.RadioButton();
			this.groupBox11 = new global::System.Windows.Forms.GroupBox();
			this.PTTDSRradioButton = new global::System.Windows.Forms.RadioButton();
			this.PTTCTSradioButton = new global::System.Windows.Forms.RadioButton();
			this.groupBox10 = new global::System.Windows.Forms.GroupBox();
			this.RXLOWradioButton = new global::System.Windows.Forms.RadioButton();
			this.RXHIGHradioButton = new global::System.Windows.Forms.RadioButton();
			this.groupBox9 = new global::System.Windows.Forms.GroupBox();
			this.RXIndicatorEnablecheckBox = new global::System.Windows.Forms.CheckBox();
			this.RXDTRradioButton = new global::System.Windows.Forms.RadioButton();
			this.RXRTSradioButton = new global::System.Windows.Forms.RadioButton();
			this.label42 = new global::System.Windows.Forms.Label();
			this.label41 = new global::System.Windows.Forms.Label();
			this.PTTKeyingcheckBox = new global::System.Windows.Forms.CheckBox();
			this.label40 = new global::System.Windows.Forms.Label();
			this.label39 = new global::System.Windows.Forms.Label();
			this.label38 = new global::System.Windows.Forms.Label();
			this.enableAMBEServerCheckBox = new global::System.Windows.Forms.CheckBox();
			this.AMBEServerHostPortTextBox = new global::System.Windows.Forms.TextBox();
			this.AMBEServerHostTextBox = new global::System.Windows.Forms.TextBox();
			this.label37 = new global::System.Windows.Forms.Label();
			this.label36 = new global::System.Windows.Forms.Label();
			this.label35 = new global::System.Windows.Forms.Label();
			this.label34 = new global::System.Windows.Forms.Label();
			this.label33 = new global::System.Windows.Forms.Label();
			this.groupBox14 = new global::System.Windows.Forms.GroupBox();
			this.HelpNXDNlinkLabel = new global::System.Windows.Forms.LinkLabel();
			this.AMBENXDNid = new global::System.Windows.Forms.TextBox();
			this.label50 = new global::System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox13.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.DVMEGAPowerTrackBar).BeginInit();
			this.groupBox4.SuspendLayout();
			this.groupBox7.SuspendLayout();
			this.groupBox8.SuspendLayout();
			this.groupBox12.SuspendLayout();
			this.groupBox11.SuspendLayout();
			this.groupBox10.SuspendLayout();
			this.groupBox9.SuspendLayout();
			this.groupBox14.SuspendLayout();
			base.SuspendLayout();
			componentResourceManager.ApplyResources(this.mycall, "mycall");
			this.mycall.CharacterCasing = global::System.Windows.Forms.CharacterCasing.Upper;
			this.mycall.Name = "mycall";
			this.toolTip1.SetToolTip(this.mycall, componentResourceManager.GetString("mycall.ToolTip"));
			componentResourceManager.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			this.label1.Click += new global::System.EventHandler(this.label1_Click);
			componentResourceManager.ApplyResources(this.save, "save");
			this.save.Name = "save";
			this.save.UseVisualStyleBackColor = true;
			this.save.Click += new global::System.EventHandler(this.saveButton_Click);
			componentResourceManager.ApplyResources(this.cancel, "cancel");
			this.cancel.Name = "cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new global::System.EventHandler(this.cancel_Click);
			componentResourceManager.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			componentResourceManager.ApplyResources(this.dmrid, "dmrid");
			this.dmrid.Name = "dmrid";
			this.toolTip1.SetToolTip(this.dmrid, componentResourceManager.GetString("dmrid.ToolTip"));
			componentResourceManager.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			componentResourceManager.ApplyResources(this.freqDMR, "freqDMR");
			this.freqDMR.Name = "freqDMR";
			this.toolTip1.SetToolTip(this.freqDMR, componentResourceManager.GetString("freqDMR.ToolTip"));
			this.freqDMR.TextChanged += new global::System.EventHandler(this.freq_TextChanged);
			this.freqDMR.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.keyPressedFREQ);
			componentResourceManager.ApplyResources(this.label4, "label4");
			this.label4.Name = "label4";
			this.label4.Click += new global::System.EventHandler(this.label4_Click);
			componentResourceManager.ApplyResources(this.listBox1, "listBox1");
			this.listBox1.FormattingEnabled = true;
			this.listBox1.Items.AddRange(new object[]
			{
				componentResourceManager.GetString("listBox1.Items"),
				componentResourceManager.GetString("listBox1.Items1"),
				componentResourceManager.GetString("listBox1.Items2"),
				componentResourceManager.GetString("listBox1.Items3"),
				componentResourceManager.GetString("listBox1.Items4"),
				componentResourceManager.GetString("listBox1.Items5"),
				componentResourceManager.GetString("listBox1.Items6"),
				componentResourceManager.GetString("listBox1.Items7"),
				componentResourceManager.GetString("listBox1.Items8"),
				componentResourceManager.GetString("listBox1.Items9"),
				componentResourceManager.GetString("listBox1.Items10"),
				componentResourceManager.GetString("listBox1.Items11"),
				componentResourceManager.GetString("listBox1.Items12"),
				componentResourceManager.GetString("listBox1.Items13"),
				componentResourceManager.GetString("listBox1.Items14"),
				componentResourceManager.GetString("listBox1.Items15"),
				componentResourceManager.GetString("listBox1.Items16"),
				componentResourceManager.GetString("listBox1.Items17"),
				componentResourceManager.GetString("listBox1.Items18"),
				componentResourceManager.GetString("listBox1.Items19"),
				componentResourceManager.GetString("listBox1.Items20"),
				componentResourceManager.GetString("listBox1.Items21"),
				componentResourceManager.GetString("listBox1.Items22"),
				componentResourceManager.GetString("listBox1.Items23"),
				componentResourceManager.GetString("listBox1.Items24"),
				componentResourceManager.GetString("listBox1.Items25"),
				componentResourceManager.GetString("listBox1.Items26"),
				componentResourceManager.GetString("listBox1.Items27"),
				componentResourceManager.GetString("listBox1.Items28"),
				componentResourceManager.GetString("listBox1.Items29"),
				componentResourceManager.GetString("listBox1.Items30"),
				componentResourceManager.GetString("listBox1.Items31"),
				componentResourceManager.GetString("listBox1.Items32"),
				componentResourceManager.GetString("listBox1.Items33"),
				componentResourceManager.GetString("listBox1.Items34"),
				componentResourceManager.GetString("listBox1.Items35"),
				componentResourceManager.GetString("listBox1.Items36"),
				componentResourceManager.GetString("listBox1.Items37"),
				componentResourceManager.GetString("listBox1.Items38")
			});
			this.listBox1.Name = "listBox1";
			this.toolTip1.SetToolTip(this.listBox1, componentResourceManager.GetString("listBox1.ToolTip"));
			this.listBox1.SelectedIndexChanged += new global::System.EventHandler(this.listBox1_SelectedIndexChanged);
			componentResourceManager.ApplyResources(this.label5, "label5");
			this.label5.Name = "label5";
			componentResourceManager.ApplyResources(this.label6, "label6");
			this.label6.Name = "label6";
			componentResourceManager.ApplyResources(this.DMRpassword, "DMRpassword");
			this.DMRpassword.Name = "DMRpassword";
			this.toolTip1.SetToolTip(this.DMRpassword, componentResourceManager.GetString("DMRpassword.ToolTip"));
			componentResourceManager.ApplyResources(this.label7, "label7");
			this.label7.Name = "label7";
			componentResourceManager.ApplyResources(this.comboBox1, "comboBox1");
			this.comboBox1.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Name = "comboBox1";
			this.toolTip1.SetToolTip(this.comboBox1, componentResourceManager.GetString("comboBox1.ToolTip"));
			componentResourceManager.ApplyResources(this.comboBox2, "comboBox2");
			this.comboBox2.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox2.FormattingEnabled = true;
			this.comboBox2.Name = "comboBox2";
			this.toolTip1.SetToolTip(this.comboBox2, componentResourceManager.GetString("comboBox2.ToolTip"));
			this.comboBox2.SelectedIndexChanged += new global::System.EventHandler(this.comboBox2_SelectedIndexChanged);
			componentResourceManager.ApplyResources(this.DSTARmodule, "DSTARmodule");
			this.DSTARmodule.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.DSTARmodule.FormattingEnabled = true;
			this.DSTARmodule.Items.AddRange(new object[]
			{
				componentResourceManager.GetString("DSTARmodule.Items"),
				componentResourceManager.GetString("DSTARmodule.Items1"),
				componentResourceManager.GetString("DSTARmodule.Items2"),
				componentResourceManager.GetString("DSTARmodule.Items3"),
				componentResourceManager.GetString("DSTARmodule.Items4")
			});
			this.DSTARmodule.Name = "DSTARmodule";
			this.toolTip1.SetToolTip(this.DSTARmodule, componentResourceManager.GetString("DSTARmodule.ToolTip"));
			componentResourceManager.ApplyResources(this.label9, "label9");
			this.label9.Name = "label9";
			componentResourceManager.ApplyResources(this.label10, "label10");
			this.label10.Name = "label10";
			componentResourceManager.ApplyResources(this.checkBoxsaveQSOlog, "checkBoxsaveQSOlog");
			this.checkBoxsaveQSOlog.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.checkBoxsaveQSOlog.Name = "checkBoxsaveQSOlog";
			this.toolTip1.SetToolTip(this.checkBoxsaveQSOlog, componentResourceManager.GetString("checkBoxsaveQSOlog.ToolTip"));
			this.checkBoxsaveQSOlog.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.label13, "label13");
			this.label13.Name = "label13";
			componentResourceManager.ApplyResources(this.QTHlocatorTextBox, "QTHlocatorTextBox");
			this.QTHlocatorTextBox.CharacterCasing = global::System.Windows.Forms.CharacterCasing.Upper;
			this.QTHlocatorTextBox.Name = "QTHlocatorTextBox";
			this.toolTip1.SetToolTip(this.QTHlocatorTextBox, componentResourceManager.GetString("QTHlocatorTextBox.ToolTip"));
			this.groupBox1.Controls.Add(this.groupBox13);
			this.groupBox1.Controls.Add(this.label43);
			this.groupBox1.Controls.Add(this.DMRidSimpleTextBox);
			this.groupBox1.Controls.Add(this.label32);
			this.groupBox1.Controls.Add(this.DMRtypeSelectionComboBox);
			this.groupBox1.Controls.Add(this.groupBox6);
			this.groupBox1.Controls.Add(this.groupBox5);
			this.groupBox1.Controls.Add(this.label24);
			this.groupBox1.Controls.Add(this.noInbandDataCheckBox);
			this.groupBox1.Controls.Add(this.checkBoxDMREnabledAtBoot);
			this.groupBox1.Controls.Add(this.label19);
			this.groupBox1.Controls.Add(this.listBox1);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.dmrid);
			this.groupBox1.Controls.Add(this.label2);
			componentResourceManager.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			this.groupBox13.Controls.Add(this.TGIFPassword);
			this.groupBox13.Controls.Add(this.label16);
			componentResourceManager.ApplyResources(this.groupBox13, "groupBox13");
			this.groupBox13.Name = "groupBox13";
			this.groupBox13.TabStop = false;
			componentResourceManager.ApplyResources(this.TGIFPassword, "TGIFPassword");
			this.TGIFPassword.Name = "TGIFPassword";
			componentResourceManager.ApplyResources(this.label16, "label16");
			this.label16.Name = "label16";
			componentResourceManager.ApplyResources(this.label43, "label43");
			this.label43.Name = "label43";
			componentResourceManager.ApplyResources(this.DMRidSimpleTextBox, "DMRidSimpleTextBox");
			this.DMRidSimpleTextBox.Name = "DMRidSimpleTextBox";
			this.toolTip1.SetToolTip(this.DMRidSimpleTextBox, componentResourceManager.GetString("DMRidSimpleTextBox.ToolTip"));
			componentResourceManager.ApplyResources(this.label32, "label32");
			this.label32.Name = "label32";
			componentResourceManager.ApplyResources(this.DMRtypeSelectionComboBox, "DMRtypeSelectionComboBox");
			this.DMRtypeSelectionComboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.DMRtypeSelectionComboBox.FormattingEnabled = true;
			this.DMRtypeSelectionComboBox.Name = "DMRtypeSelectionComboBox";
			this.DMRtypeSelectionComboBox.SelectedIndexChanged += new global::System.EventHandler(this.DMRtypeSelectionComboBox_SelectedIndexChanged);
			this.groupBox6.Controls.Add(this.label7);
			this.groupBox6.Controls.Add(this.DMRpassword);
			this.groupBox6.Controls.Add(this.label6);
			this.groupBox6.Controls.Add(this.comboBox2);
			componentResourceManager.ApplyResources(this.groupBox6, "groupBox6");
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.TabStop = false;
			this.groupBox5.Controls.Add(this.label15);
			this.groupBox5.Controls.Add(this.comboBoxDMRPlus);
			componentResourceManager.ApplyResources(this.groupBox5, "groupBox5");
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.TabStop = false;
			componentResourceManager.ApplyResources(this.label15, "label15");
			this.label15.Name = "label15";
			componentResourceManager.ApplyResources(this.comboBoxDMRPlus, "comboBoxDMRPlus");
			this.comboBoxDMRPlus.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxDMRPlus.FormattingEnabled = true;
			this.comboBoxDMRPlus.Name = "comboBoxDMRPlus";
			this.comboBoxDMRPlus.SelectedIndexChanged += new global::System.EventHandler(this.comboBoxDMRPlus_SelectedIndexChanged);
			componentResourceManager.ApplyResources(this.label24, "label24");
			this.label24.Name = "label24";
			componentResourceManager.ApplyResources(this.noInbandDataCheckBox, "noInbandDataCheckBox");
			this.noInbandDataCheckBox.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.noInbandDataCheckBox.Name = "noInbandDataCheckBox";
			this.noInbandDataCheckBox.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.checkBoxDMREnabledAtBoot, "checkBoxDMREnabledAtBoot");
			this.checkBoxDMREnabledAtBoot.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.checkBoxDMREnabledAtBoot.Name = "checkBoxDMREnabledAtBoot";
			this.checkBoxDMREnabledAtBoot.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.label19, "label19");
			this.label19.Name = "label19";
			this.groupBox2.Controls.Add(this.label20);
			this.groupBox2.Controls.Add(this.defaultDSTARReflectorTextBox);
			this.groupBox2.Controls.Add(this.label17);
			this.groupBox2.Controls.Add(this.checkBoxDSTAREnabledAtBoot);
			this.groupBox2.Controls.Add(this.label11);
			this.groupBox2.Controls.Add(this.label18);
			this.groupBox2.Controls.Add(this.APRScheckBox);
			this.groupBox2.Controls.Add(this.DSTARmodule);
			this.groupBox2.Controls.Add(this.label9);
			componentResourceManager.ApplyResources(this.groupBox2, "groupBox2");
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.TabStop = false;
			componentResourceManager.ApplyResources(this.label20, "label20");
			this.label20.Name = "label20";
			componentResourceManager.ApplyResources(this.defaultDSTARReflectorTextBox, "defaultDSTARReflectorTextBox");
			this.defaultDSTARReflectorTextBox.CharacterCasing = global::System.Windows.Forms.CharacterCasing.Upper;
			this.defaultDSTARReflectorTextBox.Name = "defaultDSTARReflectorTextBox";
			this.defaultDSTARReflectorTextBox.KeyDown += new global::System.Windows.Forms.KeyEventHandler(this.defaultDSTARReflectorTextBox_KeyDown);
			this.defaultDSTARReflectorTextBox.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.defaultDSTARReflectorTextBox_KeyPress);
			componentResourceManager.ApplyResources(this.label17, "label17");
			this.label17.Name = "label17";
			componentResourceManager.ApplyResources(this.checkBoxDSTAREnabledAtBoot, "checkBoxDSTAREnabledAtBoot");
			this.checkBoxDSTAREnabledAtBoot.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.checkBoxDSTAREnabledAtBoot.Name = "checkBoxDSTAREnabledAtBoot";
			this.checkBoxDSTAREnabledAtBoot.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.label11, "label11");
			this.label11.Name = "label11";
			componentResourceManager.ApplyResources(this.label18, "label18");
			this.label18.Name = "label18";
			componentResourceManager.ApplyResources(this.APRScheckBox, "APRScheckBox");
			this.APRScheckBox.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.APRScheckBox.Name = "APRScheckBox";
			this.APRScheckBox.UseVisualStyleBackColor = true;
			this.groupBox3.Controls.Add(this.invertDTRRadioCheckBox);
			this.groupBox3.Controls.Add(this.FrequencyWarninglabel);
			this.groupBox3.Controls.Add(this.invertRXTXcheckBox);
			this.groupBox3.Controls.Add(this.invertRXTXLabel);
			this.groupBox3.Controls.Add(this.label47);
			this.groupBox3.Controls.Add(this.languageComboBox);
			this.groupBox3.Controls.Add(this.label46);
			this.groupBox3.Controls.Add(this.alwaysOnTopcheckBox);
			this.groupBox3.Controls.Add(this.label45);
			this.groupBox3.Controls.Add(this.PlusMinLoncomboBox);
			this.groupBox3.Controls.Add(this.PlusMinLatcomboBox);
			this.groupBox3.Controls.Add(this.label28);
			this.groupBox3.Controls.Add(this.label27);
			this.groupBox3.Controls.Add(this.longitudeMaskedTextBox);
			this.groupBox3.Controls.Add(this.latitudeMaskedTextBox);
			this.groupBox3.Controls.Add(this.label26);
			this.groupBox3.Controls.Add(this.label25);
			this.groupBox3.Controls.Add(this.label23);
			this.groupBox3.Controls.Add(this.DVMEGAPowerTrackBar);
			this.groupBox3.Controls.Add(this.label21);
			this.groupBox3.Controls.Add(this.modeTimerRF);
			this.groupBox3.Controls.Add(this.label8);
			this.groupBox3.Controls.Add(this.label14);
			this.groupBox3.Controls.Add(this.RXTXColorsCheckBox);
			this.groupBox3.Controls.Add(this.freqDMR);
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.mycall);
			this.groupBox3.Controls.Add(this.label5);
			this.groupBox3.Controls.Add(this.comboBox1);
			this.groupBox3.Controls.Add(this.label10);
			this.groupBox3.Controls.Add(this.checkBoxsaveQSOlog);
			componentResourceManager.ApplyResources(this.groupBox3, "groupBox3");
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.TabStop = false;
			componentResourceManager.ApplyResources(this.invertDTRRadioCheckBox, "invertDTRRadioCheckBox");
			this.invertDTRRadioCheckBox.Name = "invertDTRRadioCheckBox";
			this.invertDTRRadioCheckBox.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.FrequencyWarninglabel, "FrequencyWarninglabel");
			this.FrequencyWarninglabel.Name = "FrequencyWarninglabel";
			componentResourceManager.ApplyResources(this.invertRXTXcheckBox, "invertRXTXcheckBox");
			this.invertRXTXcheckBox.Name = "invertRXTXcheckBox";
			this.invertRXTXcheckBox.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.invertRXTXLabel, "invertRXTXLabel");
			this.invertRXTXLabel.Name = "invertRXTXLabel";
			componentResourceManager.ApplyResources(this.label47, "label47");
			this.label47.Name = "label47";
			componentResourceManager.ApplyResources(this.languageComboBox, "languageComboBox");
			this.languageComboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.languageComboBox.FormattingEnabled = true;
			this.languageComboBox.Items.AddRange(new object[]
			{
				componentResourceManager.GetString("languageComboBox.Items"),
				componentResourceManager.GetString("languageComboBox.Items1"),
				componentResourceManager.GetString("languageComboBox.Items2"),
				componentResourceManager.GetString("languageComboBox.Items3")
			});
			this.languageComboBox.Name = "languageComboBox";
			componentResourceManager.ApplyResources(this.label46, "label46");
			this.label46.Name = "label46";
			componentResourceManager.ApplyResources(this.alwaysOnTopcheckBox, "alwaysOnTopcheckBox");
			this.alwaysOnTopcheckBox.Name = "alwaysOnTopcheckBox";
			this.alwaysOnTopcheckBox.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.label45, "label45");
			this.label45.Name = "label45";
			componentResourceManager.ApplyResources(this.PlusMinLoncomboBox, "PlusMinLoncomboBox");
			this.PlusMinLoncomboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.PlusMinLoncomboBox.FormattingEnabled = true;
			this.PlusMinLoncomboBox.Items.AddRange(new object[]
			{
				componentResourceManager.GetString("PlusMinLoncomboBox.Items"),
				componentResourceManager.GetString("PlusMinLoncomboBox.Items1")
			});
			this.PlusMinLoncomboBox.Name = "PlusMinLoncomboBox";
			componentResourceManager.ApplyResources(this.PlusMinLatcomboBox, "PlusMinLatcomboBox");
			this.PlusMinLatcomboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.PlusMinLatcomboBox.FormattingEnabled = true;
			this.PlusMinLatcomboBox.Items.AddRange(new object[]
			{
				componentResourceManager.GetString("PlusMinLatcomboBox.Items"),
				componentResourceManager.GetString("PlusMinLatcomboBox.Items1")
			});
			this.PlusMinLatcomboBox.Name = "PlusMinLatcomboBox";
			componentResourceManager.ApplyResources(this.label28, "label28");
			this.label28.Name = "label28";
			componentResourceManager.ApplyResources(this.label27, "label27");
			this.label27.Name = "label27";
			componentResourceManager.ApplyResources(this.longitudeMaskedTextBox, "longitudeMaskedTextBox");
			this.longitudeMaskedTextBox.Culture = new global::System.Globalization.CultureInfo("en-US");
			this.longitudeMaskedTextBox.Name = "longitudeMaskedTextBox";
			componentResourceManager.ApplyResources(this.latitudeMaskedTextBox, "latitudeMaskedTextBox");
			this.latitudeMaskedTextBox.Culture = new global::System.Globalization.CultureInfo("en-US");
			this.latitudeMaskedTextBox.Name = "latitudeMaskedTextBox";
			this.latitudeMaskedTextBox.ResetOnSpace = false;
			componentResourceManager.ApplyResources(this.label26, "label26");
			this.label26.Name = "label26";
			componentResourceManager.ApplyResources(this.label25, "label25");
			this.label25.Name = "label25";
			componentResourceManager.ApplyResources(this.label23, "label23");
			this.label23.Name = "label23";
			componentResourceManager.ApplyResources(this.DVMEGAPowerTrackBar, "DVMEGAPowerTrackBar");
			this.DVMEGAPowerTrackBar.AccessibleRole = global::System.Windows.Forms.AccessibleRole.Slider;
			this.DVMEGAPowerTrackBar.Maximum = 255;
			this.DVMEGAPowerTrackBar.Minimum = 10;
			this.DVMEGAPowerTrackBar.Name = "DVMEGAPowerTrackBar";
			this.DVMEGAPowerTrackBar.TickStyle = global::System.Windows.Forms.TickStyle.None;
			this.DVMEGAPowerTrackBar.Value = 100;
			this.DVMEGAPowerTrackBar.Scroll += new global::System.EventHandler(this.DVMEGAPowerTrackBar_Scroll);
			componentResourceManager.ApplyResources(this.label21, "label21");
			this.label21.Name = "label21";
			componentResourceManager.ApplyResources(this.modeTimerRF, "modeTimerRF");
			this.modeTimerRF.Name = "modeTimerRF";
			componentResourceManager.ApplyResources(this.label8, "label8");
			this.label8.Name = "label8";
			componentResourceManager.ApplyResources(this.label14, "label14");
			this.label14.Name = "label14";
			this.label14.Click += new global::System.EventHandler(this.label14_Click);
			componentResourceManager.ApplyResources(this.RXTXColorsCheckBox, "RXTXColorsCheckBox");
			this.RXTXColorsCheckBox.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.RXTXColorsCheckBox.Name = "RXTXColorsCheckBox";
			this.toolTip1.SetToolTip(this.RXTXColorsCheckBox, componentResourceManager.GetString("RXTXColorsCheckBox.ToolTip"));
			this.RXTXColorsCheckBox.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.AMBEspeed, "AMBEspeed");
			this.AMBEspeed.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.AMBEspeed.FormattingEnabled = true;
			this.AMBEspeed.Items.AddRange(new object[]
			{
				componentResourceManager.GetString("AMBEspeed.Items"),
				componentResourceManager.GetString("AMBEspeed.Items1"),
				componentResourceManager.GetString("AMBEspeed.Items2")
			});
			this.AMBEspeed.Name = "AMBEspeed";
			this.toolTip1.SetToolTip(this.AMBEspeed, componentResourceManager.GetString("AMBEspeed.ToolTip"));
			componentResourceManager.ApplyResources(this.label29, "label29");
			this.label29.Name = "label29";
			componentResourceManager.ApplyResources(this.dvdonglecheckBox, "dvdonglecheckBox");
			this.dvdonglecheckBox.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.dvdonglecheckBox.Name = "dvdonglecheckBox";
			this.dvdonglecheckBox.UseVisualStyleBackColor = true;
			this.dvdonglecheckBox.CheckedChanged += new global::System.EventHandler(this.dvdonglecheckBox_CheckedChanged);
			componentResourceManager.ApplyResources(this.modeTimerNet, "modeTimerNet");
			this.modeTimerNet.Name = "modeTimerNet";
			this.startProtocolComboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.startProtocolComboBox.FormattingEnabled = true;
			this.startProtocolComboBox.Items.AddRange(new object[]
			{
				componentResourceManager.GetString("startProtocolComboBox.Items"),
				componentResourceManager.GetString("startProtocolComboBox.Items1"),
				componentResourceManager.GetString("startProtocolComboBox.Items2")
			});
			componentResourceManager.ApplyResources(this.startProtocolComboBox, "startProtocolComboBox");
			this.startProtocolComboBox.Name = "startProtocolComboBox";
			this.startProtocolComboBox.SelectedIndexChanged += new global::System.EventHandler(this.BootProtoSelectedIndexChanged);
			this.groupBox4.Controls.Add(this.YSFradioButton);
			this.groupBox4.Controls.Add(this.FCSradioButton);
			this.groupBox4.Controls.Add(this.FCSReflectorModulecomboBox);
			this.groupBox4.Controls.Add(this.FCSReflectorHostcomboBox);
			this.groupBox4.Controls.Add(this.label31);
			this.groupBox4.Controls.Add(this.label30);
			this.groupBox4.Controls.Add(this.YSFreflectorListcomboBox);
			this.groupBox4.Controls.Add(this.label22);
			this.groupBox4.Controls.Add(this.checkBoxFusionEnabledAtBoot);
			this.groupBox4.Controls.Add(this.label12);
			this.groupBox4.Controls.Add(this.label13);
			this.groupBox4.Controls.Add(this.QTHlocatorTextBox);
			componentResourceManager.ApplyResources(this.groupBox4, "groupBox4");
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.TabStop = false;
			componentResourceManager.ApplyResources(this.YSFradioButton, "YSFradioButton");
			this.YSFradioButton.Checked = true;
			this.YSFradioButton.Name = "YSFradioButton";
			this.YSFradioButton.TabStop = true;
			this.YSFradioButton.UseVisualStyleBackColor = true;
			this.YSFradioButton.Click += new global::System.EventHandler(this.YSFradioButton_Click);
			componentResourceManager.ApplyResources(this.FCSradioButton, "FCSradioButton");
			this.FCSradioButton.Name = "FCSradioButton";
			this.FCSradioButton.UseVisualStyleBackColor = true;
			this.FCSradioButton.Click += new global::System.EventHandler(this.YSFradioButton_Click);
			componentResourceManager.ApplyResources(this.FCSReflectorModulecomboBox, "FCSReflectorModulecomboBox");
			this.FCSReflectorModulecomboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.FCSReflectorModulecomboBox.FormattingEnabled = true;
			this.FCSReflectorModulecomboBox.Name = "FCSReflectorModulecomboBox";
			componentResourceManager.ApplyResources(this.FCSReflectorHostcomboBox, "FCSReflectorHostcomboBox");
			this.FCSReflectorHostcomboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.FCSReflectorHostcomboBox.FormattingEnabled = true;
			this.FCSReflectorHostcomboBox.Items.AddRange(new object[]
			{
				componentResourceManager.GetString("FCSReflectorHostcomboBox.Items"),
				componentResourceManager.GetString("FCSReflectorHostcomboBox.Items1"),
				componentResourceManager.GetString("FCSReflectorHostcomboBox.Items2"),
				componentResourceManager.GetString("FCSReflectorHostcomboBox.Items3")
			});
			this.FCSReflectorHostcomboBox.Name = "FCSReflectorHostcomboBox";
			componentResourceManager.ApplyResources(this.label31, "label31");
			this.label31.Name = "label31";
			componentResourceManager.ApplyResources(this.label30, "label30");
			this.label30.Name = "label30";
			componentResourceManager.ApplyResources(this.YSFreflectorListcomboBox, "YSFreflectorListcomboBox");
			this.YSFreflectorListcomboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.YSFreflectorListcomboBox.FormattingEnabled = true;
			this.YSFreflectorListcomboBox.Name = "YSFreflectorListcomboBox";
			componentResourceManager.ApplyResources(this.label22, "label22");
			this.label22.Name = "label22";
			componentResourceManager.ApplyResources(this.checkBoxFusionEnabledAtBoot, "checkBoxFusionEnabledAtBoot");
			this.checkBoxFusionEnabledAtBoot.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.checkBoxFusionEnabledAtBoot.Name = "checkBoxFusionEnabledAtBoot";
			this.checkBoxFusionEnabledAtBoot.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.label12, "label12");
			this.label12.Name = "label12";
			componentResourceManager.ApplyResources(this.AMBEDMRid, "AMBEDMRid");
			this.AMBEDMRid.Name = "AMBEDMRid";
			this.toolTip1.SetToolTip(this.AMBEDMRid, componentResourceManager.GetString("AMBEDMRid.ToolTip"));
			componentResourceManager.ApplyResources(this.comboBox3, "comboBox3");
			this.comboBox3.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox3.FormattingEnabled = true;
			this.comboBox3.Name = "comboBox3";
			this.toolTip1.SetToolTip(this.comboBox3, componentResourceManager.GetString("comboBox3.ToolTip"));
			componentResourceManager.ApplyResources(this.COMportPTTcomboBox, "COMportPTTcomboBox");
			this.COMportPTTcomboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.COMportPTTcomboBox.FormattingEnabled = true;
			this.COMportPTTcomboBox.Name = "COMportPTTcomboBox";
			this.toolTip1.SetToolTip(this.COMportPTTcomboBox, componentResourceManager.GetString("COMportPTTcomboBox.ToolTip"));
			componentResourceManager.ApplyResources(this.AMBEtypeComboBox, "AMBEtypeComboBox");
			this.AMBEtypeComboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.AMBEtypeComboBox.FormattingEnabled = true;
			this.AMBEtypeComboBox.Items.AddRange(new object[]
			{
				componentResourceManager.GetString("AMBEtypeComboBox.Items"),
				componentResourceManager.GetString("AMBEtypeComboBox.Items1")
			});
			this.AMBEtypeComboBox.Name = "AMBEtypeComboBox";
			this.toolTip1.SetToolTip(this.AMBEtypeComboBox, componentResourceManager.GetString("AMBEtypeComboBox.ToolTip"));
			this.AMBEtypeComboBox.SelectedIndexChanged += new global::System.EventHandler(this.AMBEtypeComboBox_SelectedIndexChanged);
			componentResourceManager.ApplyResources(this.AMBEBeepcheckBox, "AMBEBeepcheckBox");
			this.AMBEBeepcheckBox.AccessibleRole = global::System.Windows.Forms.AccessibleRole.CheckButton;
			this.AMBEBeepcheckBox.Name = "AMBEBeepcheckBox";
			this.AMBEBeepcheckBox.UseVisualStyleBackColor = true;
			this.groupBox7.Controls.Add(this.label49);
			this.groupBox7.Controls.Add(this.AMBEtypeComboBox);
			this.groupBox7.Controls.Add(this.label48);
			this.groupBox7.Controls.Add(this.DSTARSlowDataTextBox);
			this.groupBox7.Controls.Add(this.killTimerComboBox);
			this.groupBox7.Controls.Add(this.label44);
			this.groupBox7.Controls.Add(this.groupBox8);
			this.groupBox7.Controls.Add(this.label40);
			this.groupBox7.Controls.Add(this.comboBox3);
			this.groupBox7.Controls.Add(this.label39);
			this.groupBox7.Controls.Add(this.AMBEBeepcheckBox);
			this.groupBox7.Controls.Add(this.label38);
			this.groupBox7.Controls.Add(this.enableAMBEServerCheckBox);
			this.groupBox7.Controls.Add(this.AMBEServerHostPortTextBox);
			this.groupBox7.Controls.Add(this.AMBEServerHostTextBox);
			this.groupBox7.Controls.Add(this.label37);
			this.groupBox7.Controls.Add(this.label36);
			this.groupBox7.Controls.Add(this.label29);
			this.groupBox7.Controls.Add(this.label35);
			this.groupBox7.Controls.Add(this.label34);
			this.groupBox7.Controls.Add(this.AMBEspeed);
			this.groupBox7.Controls.Add(this.dvdonglecheckBox);
			this.groupBox7.Controls.Add(this.AMBEDMRid);
			this.groupBox7.Controls.Add(this.label33);
			componentResourceManager.ApplyResources(this.groupBox7, "groupBox7");
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.TabStop = false;
			componentResourceManager.ApplyResources(this.label49, "label49");
			this.label49.Name = "label49";
			componentResourceManager.ApplyResources(this.label48, "label48");
			this.label48.Name = "label48";
			componentResourceManager.ApplyResources(this.DSTARSlowDataTextBox, "DSTARSlowDataTextBox");
			this.DSTARSlowDataTextBox.Name = "DSTARSlowDataTextBox";
			componentResourceManager.ApplyResources(this.killTimerComboBox, "killTimerComboBox");
			this.killTimerComboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.killTimerComboBox.FormattingEnabled = true;
			this.killTimerComboBox.Items.AddRange(new object[]
			{
				componentResourceManager.GetString("killTimerComboBox.Items"),
				componentResourceManager.GetString("killTimerComboBox.Items1"),
				componentResourceManager.GetString("killTimerComboBox.Items2")
			});
			this.killTimerComboBox.Name = "killTimerComboBox";
			componentResourceManager.ApplyResources(this.label44, "label44");
			this.label44.Name = "label44";
			this.groupBox8.Controls.Add(this.groupBox12);
			this.groupBox8.Controls.Add(this.groupBox11);
			this.groupBox8.Controls.Add(this.groupBox10);
			this.groupBox8.Controls.Add(this.groupBox9);
			this.groupBox8.Controls.Add(this.label42);
			this.groupBox8.Controls.Add(this.label41);
			this.groupBox8.Controls.Add(this.PTTKeyingcheckBox);
			this.groupBox8.Controls.Add(this.COMportPTTcomboBox);
			componentResourceManager.ApplyResources(this.groupBox8, "groupBox8");
			this.groupBox8.Name = "groupBox8";
			this.groupBox8.TabStop = false;
			this.groupBox12.Controls.Add(this.PTTLOWradioButton);
			this.groupBox12.Controls.Add(this.PTTHIGHradioButton);
			componentResourceManager.ApplyResources(this.groupBox12, "groupBox12");
			this.groupBox12.Name = "groupBox12";
			this.groupBox12.TabStop = false;
			componentResourceManager.ApplyResources(this.PTTLOWradioButton, "PTTLOWradioButton");
			this.PTTLOWradioButton.Name = "PTTLOWradioButton";
			this.PTTLOWradioButton.TabStop = true;
			this.PTTLOWradioButton.UseVisualStyleBackColor = true;
			this.PTTLOWradioButton.Click += new global::System.EventHandler(this.PTTradiobuttonHighLow_Click);
			componentResourceManager.ApplyResources(this.PTTHIGHradioButton, "PTTHIGHradioButton");
			this.PTTHIGHradioButton.Name = "PTTHIGHradioButton";
			this.PTTHIGHradioButton.TabStop = true;
			this.PTTHIGHradioButton.UseVisualStyleBackColor = true;
			this.PTTHIGHradioButton.Click += new global::System.EventHandler(this.PTTradiobuttonHighLow_Click);
			this.groupBox11.Controls.Add(this.PTTDSRradioButton);
			this.groupBox11.Controls.Add(this.PTTCTSradioButton);
			componentResourceManager.ApplyResources(this.groupBox11, "groupBox11");
			this.groupBox11.Name = "groupBox11";
			this.groupBox11.TabStop = false;
			componentResourceManager.ApplyResources(this.PTTDSRradioButton, "PTTDSRradioButton");
			this.PTTDSRradioButton.Name = "PTTDSRradioButton";
			this.PTTDSRradioButton.TabStop = true;
			this.PTTDSRradioButton.UseVisualStyleBackColor = true;
			this.PTTDSRradioButton.Click += new global::System.EventHandler(this.PTTradiobutton_Click);
			componentResourceManager.ApplyResources(this.PTTCTSradioButton, "PTTCTSradioButton");
			this.PTTCTSradioButton.Name = "PTTCTSradioButton";
			this.PTTCTSradioButton.TabStop = true;
			this.PTTCTSradioButton.UseVisualStyleBackColor = true;
			this.PTTCTSradioButton.Click += new global::System.EventHandler(this.PTTradiobutton_Click);
			this.groupBox10.Controls.Add(this.RXLOWradioButton);
			this.groupBox10.Controls.Add(this.RXHIGHradioButton);
			componentResourceManager.ApplyResources(this.groupBox10, "groupBox10");
			this.groupBox10.Name = "groupBox10";
			this.groupBox10.TabStop = false;
			componentResourceManager.ApplyResources(this.RXLOWradioButton, "RXLOWradioButton");
			this.RXLOWradioButton.Name = "RXLOWradioButton";
			this.RXLOWradioButton.TabStop = true;
			this.RXLOWradioButton.UseVisualStyleBackColor = true;
			this.RXLOWradioButton.Click += new global::System.EventHandler(this.RXradiobuttonHighLow_Click);
			componentResourceManager.ApplyResources(this.RXHIGHradioButton, "RXHIGHradioButton");
			this.RXHIGHradioButton.Name = "RXHIGHradioButton";
			this.RXHIGHradioButton.TabStop = true;
			this.RXHIGHradioButton.UseVisualStyleBackColor = true;
			this.RXHIGHradioButton.Click += new global::System.EventHandler(this.RXradiobuttonHighLow_Click);
			this.groupBox9.Controls.Add(this.RXIndicatorEnablecheckBox);
			this.groupBox9.Controls.Add(this.RXDTRradioButton);
			this.groupBox9.Controls.Add(this.RXRTSradioButton);
			componentResourceManager.ApplyResources(this.groupBox9, "groupBox9");
			this.groupBox9.Name = "groupBox9";
			this.groupBox9.TabStop = false;
			componentResourceManager.ApplyResources(this.RXIndicatorEnablecheckBox, "RXIndicatorEnablecheckBox");
			this.RXIndicatorEnablecheckBox.Name = "RXIndicatorEnablecheckBox";
			this.RXIndicatorEnablecheckBox.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.RXDTRradioButton, "RXDTRradioButton");
			this.RXDTRradioButton.Name = "RXDTRradioButton";
			this.RXDTRradioButton.TabStop = true;
			this.RXDTRradioButton.UseVisualStyleBackColor = true;
			this.RXDTRradioButton.Click += new global::System.EventHandler(this.RXradiobutton_Click);
			componentResourceManager.ApplyResources(this.RXRTSradioButton, "RXRTSradioButton");
			this.RXRTSradioButton.Name = "RXRTSradioButton";
			this.RXRTSradioButton.TabStop = true;
			this.RXRTSradioButton.UseVisualStyleBackColor = true;
			this.RXRTSradioButton.Click += new global::System.EventHandler(this.RXradiobutton_Click);
			componentResourceManager.ApplyResources(this.label42, "label42");
			this.label42.Name = "label42";
			componentResourceManager.ApplyResources(this.label41, "label41");
			this.label41.Name = "label41";
			componentResourceManager.ApplyResources(this.PTTKeyingcheckBox, "PTTKeyingcheckBox");
			this.PTTKeyingcheckBox.Name = "PTTKeyingcheckBox";
			this.PTTKeyingcheckBox.UseVisualStyleBackColor = true;
			this.PTTKeyingcheckBox.CheckedChanged += new global::System.EventHandler(this.dvdonglecheckBox_CheckedChanged);
			componentResourceManager.ApplyResources(this.label40, "label40");
			this.label40.Name = "label40";
			componentResourceManager.ApplyResources(this.label39, "label39");
			this.label39.Name = "label39";
			componentResourceManager.ApplyResources(this.label38, "label38");
			this.label38.Name = "label38";
			componentResourceManager.ApplyResources(this.enableAMBEServerCheckBox, "enableAMBEServerCheckBox");
			this.enableAMBEServerCheckBox.Name = "enableAMBEServerCheckBox";
			this.enableAMBEServerCheckBox.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.AMBEServerHostPortTextBox, "AMBEServerHostPortTextBox");
			this.AMBEServerHostPortTextBox.Name = "AMBEServerHostPortTextBox";
			componentResourceManager.ApplyResources(this.AMBEServerHostTextBox, "AMBEServerHostTextBox");
			this.AMBEServerHostTextBox.Name = "AMBEServerHostTextBox";
			componentResourceManager.ApplyResources(this.label37, "label37");
			this.label37.Name = "label37";
			componentResourceManager.ApplyResources(this.label36, "label36");
			this.label36.Name = "label36";
			componentResourceManager.ApplyResources(this.label35, "label35");
			this.label35.Name = "label35";
			this.label35.Click += new global::System.EventHandler(this.label35_Click);
			componentResourceManager.ApplyResources(this.label34, "label34");
			this.label34.Name = "label34";
			componentResourceManager.ApplyResources(this.label33, "label33");
			this.label33.Name = "label33";
			this.groupBox14.Controls.Add(this.HelpNXDNlinkLabel);
			this.groupBox14.Controls.Add(this.AMBENXDNid);
			this.groupBox14.Controls.Add(this.label50);
			componentResourceManager.ApplyResources(this.groupBox14, "groupBox14");
			this.groupBox14.Name = "groupBox14";
			this.groupBox14.TabStop = false;
			componentResourceManager.ApplyResources(this.HelpNXDNlinkLabel, "HelpNXDNlinkLabel");
			this.HelpNXDNlinkLabel.Name = "HelpNXDNlinkLabel";
			this.HelpNXDNlinkLabel.TabStop = true;
			this.HelpNXDNlinkLabel.LinkClicked += new global::System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HelpNXDNlinkLabel_LinkClicked);
			componentResourceManager.ApplyResources(this.AMBENXDNid, "AMBENXDNid");
			this.AMBENXDNid.Name = "AMBENXDNid";
			componentResourceManager.ApplyResources(this.label50, "label50");
			this.label50.Name = "label50";
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Inherit;
			componentResourceManager.ApplyResources(this, "$this");
			base.Controls.Add(this.groupBox14);
			base.Controls.Add(this.groupBox7);
			base.Controls.Add(this.modeTimerNet);
			base.Controls.Add(this.groupBox4);
			base.Controls.Add(this.groupBox3);
			base.Controls.Add(this.groupBox2);
			base.Controls.Add(this.groupBox1);
			base.Controls.Add(this.startProtocolComboBox);
			base.Controls.Add(this.cancel);
			base.Controls.Add(this.save);
			this.Cursor = global::System.Windows.Forms.Cursors.Default;
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.KeyPreview = true;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "Setup";
			base.Load += new global::System.EventHandler(this.Setup_Load);
			base.KeyDown += new global::System.Windows.Forms.KeyEventHandler(this.Setup_KeyDown);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox13.ResumeLayout(false);
			this.groupBox13.PerformLayout();
			this.groupBox6.ResumeLayout(false);
			this.groupBox6.PerformLayout();
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.DVMEGAPowerTrackBar).EndInit();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.groupBox7.ResumeLayout(false);
			this.groupBox7.PerformLayout();
			this.groupBox8.ResumeLayout(false);
			this.groupBox8.PerformLayout();
			this.groupBox12.ResumeLayout(false);
			this.groupBox12.PerformLayout();
			this.groupBox11.ResumeLayout(false);
			this.groupBox11.PerformLayout();
			this.groupBox10.ResumeLayout(false);
			this.groupBox10.PerformLayout();
			this.groupBox9.ResumeLayout(false);
			this.groupBox9.PerformLayout();
			this.groupBox14.ResumeLayout(false);
			this.groupBox14.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x040003C4 RID: 964
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040003C5 RID: 965
		private global::System.Windows.Forms.TextBox mycall;

		// Token: 0x040003C6 RID: 966
		private global::System.Windows.Forms.Label label1;

		// Token: 0x040003C7 RID: 967
		private global::System.Windows.Forms.Button save;

		// Token: 0x040003C8 RID: 968
		private global::System.Windows.Forms.Button cancel;

		// Token: 0x040003C9 RID: 969
		private global::System.Windows.Forms.Label label2;

		// Token: 0x040003CA RID: 970
		private global::System.Windows.Forms.TextBox dmrid;

		// Token: 0x040003CB RID: 971
		private global::System.Windows.Forms.Label label3;

		// Token: 0x040003CC RID: 972
		private global::System.Windows.Forms.TextBox freqDMR;

		// Token: 0x040003CD RID: 973
		private global::System.Windows.Forms.Label label4;

		// Token: 0x040003CE RID: 974
		private global::System.Windows.Forms.ListBox listBox1;

		// Token: 0x040003CF RID: 975
		private global::System.Windows.Forms.Label label5;

		// Token: 0x040003D0 RID: 976
		private global::System.Windows.Forms.Label label6;

		// Token: 0x040003D1 RID: 977
		private global::System.Windows.Forms.TextBox DMRpassword;

		// Token: 0x040003D2 RID: 978
		private global::System.Windows.Forms.Label label7;

		// Token: 0x040003D3 RID: 979
		private global::System.Windows.Forms.ComboBox comboBox1;

		// Token: 0x040003D4 RID: 980
		private global::System.Windows.Forms.ComboBox comboBox2;

		// Token: 0x040003D5 RID: 981
		private global::System.Windows.Forms.ComboBox DSTARmodule;

		// Token: 0x040003D6 RID: 982
		private global::System.Windows.Forms.Label label9;

		// Token: 0x040003D7 RID: 983
		private global::System.Windows.Forms.Label label10;

		// Token: 0x040003D8 RID: 984
		private global::System.Windows.Forms.CheckBox checkBoxsaveQSOlog;

		// Token: 0x040003D9 RID: 985
		private global::System.Windows.Forms.Label label13;

		// Token: 0x040003DA RID: 986
		private global::System.Windows.Forms.TextBox QTHlocatorTextBox;

		// Token: 0x040003DB RID: 987
		private global::System.Windows.Forms.GroupBox groupBox1;

		// Token: 0x040003DC RID: 988
		private global::System.Windows.Forms.GroupBox groupBox2;

		// Token: 0x040003DD RID: 989
		private global::System.Windows.Forms.GroupBox groupBox3;

		// Token: 0x040003DE RID: 990
		private global::System.Windows.Forms.GroupBox groupBox4;

		// Token: 0x040003DF RID: 991
		private global::System.Windows.Forms.Label label14;

		// Token: 0x040003E0 RID: 992
		private global::System.Windows.Forms.CheckBox RXTXColorsCheckBox;

		// Token: 0x040003E1 RID: 993
		private global::System.Windows.Forms.ToolTip toolTip1;

		// Token: 0x040003E2 RID: 994
		private global::System.Windows.Forms.GroupBox groupBox5;

		// Token: 0x040003E3 RID: 995
		private global::System.Windows.Forms.Label label15;

		// Token: 0x040003E4 RID: 996
		private global::System.Windows.Forms.ComboBox comboBoxDMRPlus;

		// Token: 0x040003E5 RID: 997
		private global::System.Windows.Forms.GroupBox groupBox6;

		// Token: 0x040003E6 RID: 998
		private global::System.Windows.Forms.ComboBox startProtocolComboBox;

		// Token: 0x040003E7 RID: 999
		private global::System.Windows.Forms.Label label18;

		// Token: 0x040003E8 RID: 1000
		private global::System.Windows.Forms.CheckBox APRScheckBox;

		// Token: 0x040003E9 RID: 1001
		private global::System.Windows.Forms.CheckBox checkBoxDMREnabledAtBoot;

		// Token: 0x040003EA RID: 1002
		private global::System.Windows.Forms.Label label19;

		// Token: 0x040003EB RID: 1003
		private global::System.Windows.Forms.CheckBox checkBoxDSTAREnabledAtBoot;

		// Token: 0x040003EC RID: 1004
		private global::System.Windows.Forms.Label label11;

		// Token: 0x040003ED RID: 1005
		private global::System.Windows.Forms.TextBox modeTimerRF;

		// Token: 0x040003EE RID: 1006
		private global::System.Windows.Forms.Label label8;

		// Token: 0x040003EF RID: 1007
		private global::System.Windows.Forms.CheckBox checkBoxFusionEnabledAtBoot;

		// Token: 0x040003F0 RID: 1008
		private global::System.Windows.Forms.Label label12;

		// Token: 0x040003F1 RID: 1009
		private global::System.Windows.Forms.TextBox modeTimerNet;

		// Token: 0x040003F2 RID: 1010
		private global::System.Windows.Forms.Label label21;

		// Token: 0x040003F3 RID: 1011
		private global::System.Windows.Forms.Label label20;

		// Token: 0x040003F4 RID: 1012
		private global::System.Windows.Forms.TextBox defaultDSTARReflectorTextBox;

		// Token: 0x040003F5 RID: 1013
		private global::System.Windows.Forms.Label label17;

		// Token: 0x040003F6 RID: 1014
		private global::System.Windows.Forms.Label label22;

		// Token: 0x040003F7 RID: 1015
		private global::System.Windows.Forms.Label label23;

		// Token: 0x040003F8 RID: 1016
		private global::System.Windows.Forms.TrackBar DVMEGAPowerTrackBar;

		// Token: 0x040003F9 RID: 1017
		private global::System.Windows.Forms.CheckBox noInbandDataCheckBox;

		// Token: 0x040003FA RID: 1018
		private global::System.Windows.Forms.Label label24;

		// Token: 0x040003FB RID: 1019
		private global::System.Windows.Forms.Label label26;

		// Token: 0x040003FC RID: 1020
		private global::System.Windows.Forms.Label label25;

		// Token: 0x040003FD RID: 1021
		private global::System.Windows.Forms.MaskedTextBox longitudeMaskedTextBox;

		// Token: 0x040003FE RID: 1022
		private global::System.Windows.Forms.MaskedTextBox latitudeMaskedTextBox;

		// Token: 0x040003FF RID: 1023
		private global::System.Windows.Forms.Label label28;

		// Token: 0x04000400 RID: 1024
		private global::System.Windows.Forms.Label label27;

		// Token: 0x04000401 RID: 1025
		private global::System.Windows.Forms.Label label29;

		// Token: 0x04000402 RID: 1026
		private global::System.Windows.Forms.CheckBox dvdonglecheckBox;

		// Token: 0x04000403 RID: 1027
		private global::System.Windows.Forms.ComboBox FCSReflectorModulecomboBox;

		// Token: 0x04000404 RID: 1028
		private global::System.Windows.Forms.ComboBox FCSReflectorHostcomboBox;

		// Token: 0x04000405 RID: 1029
		private global::System.Windows.Forms.Label label31;

		// Token: 0x04000406 RID: 1030
		private global::System.Windows.Forms.Label label30;

		// Token: 0x04000407 RID: 1031
		private global::System.Windows.Forms.ComboBox YSFreflectorListcomboBox;

		// Token: 0x04000408 RID: 1032
		private global::System.Windows.Forms.RadioButton YSFradioButton;

		// Token: 0x04000409 RID: 1033
		private global::System.Windows.Forms.RadioButton FCSradioButton;

		// Token: 0x0400040A RID: 1034
		private global::System.Windows.Forms.ComboBox AMBEspeed;

		// Token: 0x0400040B RID: 1035
		private global::System.Windows.Forms.ComboBox DMRtypeSelectionComboBox;

		// Token: 0x0400040C RID: 1036
		private global::System.Windows.Forms.Label label32;

		// Token: 0x0400040D RID: 1037
		private global::System.Windows.Forms.GroupBox groupBox7;

		// Token: 0x0400040E RID: 1038
		private global::System.Windows.Forms.TextBox AMBEDMRid;

		// Token: 0x0400040F RID: 1039
		private global::System.Windows.Forms.Label label33;

		// Token: 0x04000410 RID: 1040
		private global::System.Windows.Forms.Label label35;

		// Token: 0x04000411 RID: 1041
		private global::System.Windows.Forms.Label label34;

		// Token: 0x04000412 RID: 1042
		private global::System.Windows.Forms.Label label38;

		// Token: 0x04000413 RID: 1043
		private global::System.Windows.Forms.CheckBox enableAMBEServerCheckBox;

		// Token: 0x04000414 RID: 1044
		private global::System.Windows.Forms.TextBox AMBEServerHostPortTextBox;

		// Token: 0x04000415 RID: 1045
		private global::System.Windows.Forms.TextBox AMBEServerHostTextBox;

		// Token: 0x04000416 RID: 1046
		private global::System.Windows.Forms.Label label37;

		// Token: 0x04000417 RID: 1047
		private global::System.Windows.Forms.Label label36;

		// Token: 0x04000418 RID: 1048
		private global::System.Windows.Forms.Label label39;

		// Token: 0x04000419 RID: 1049
		private global::System.Windows.Forms.CheckBox AMBEBeepcheckBox;

		// Token: 0x0400041A RID: 1050
		private global::System.Windows.Forms.Label label40;

		// Token: 0x0400041B RID: 1051
		private global::System.Windows.Forms.ComboBox comboBox3;

		// Token: 0x0400041C RID: 1052
		private global::System.Windows.Forms.GroupBox groupBox8;

		// Token: 0x0400041D RID: 1053
		private global::System.Windows.Forms.GroupBox groupBox9;

		// Token: 0x0400041E RID: 1054
		private global::System.Windows.Forms.RadioButton RXDTRradioButton;

		// Token: 0x0400041F RID: 1055
		private global::System.Windows.Forms.RadioButton RXRTSradioButton;

		// Token: 0x04000420 RID: 1056
		private global::System.Windows.Forms.Label label42;

		// Token: 0x04000421 RID: 1057
		private global::System.Windows.Forms.Label label41;

		// Token: 0x04000422 RID: 1058
		private global::System.Windows.Forms.CheckBox PTTKeyingcheckBox;

		// Token: 0x04000423 RID: 1059
		private global::System.Windows.Forms.ComboBox COMportPTTcomboBox;

		// Token: 0x04000424 RID: 1060
		private global::System.Windows.Forms.GroupBox groupBox12;

		// Token: 0x04000425 RID: 1061
		private global::System.Windows.Forms.RadioButton PTTLOWradioButton;

		// Token: 0x04000426 RID: 1062
		private global::System.Windows.Forms.RadioButton PTTHIGHradioButton;

		// Token: 0x04000427 RID: 1063
		private global::System.Windows.Forms.GroupBox groupBox11;

		// Token: 0x04000428 RID: 1064
		private global::System.Windows.Forms.RadioButton PTTDSRradioButton;

		// Token: 0x04000429 RID: 1065
		private global::System.Windows.Forms.RadioButton PTTCTSradioButton;

		// Token: 0x0400042A RID: 1066
		private global::System.Windows.Forms.GroupBox groupBox10;

		// Token: 0x0400042B RID: 1067
		private global::System.Windows.Forms.RadioButton RXLOWradioButton;

		// Token: 0x0400042C RID: 1068
		private global::System.Windows.Forms.RadioButton RXHIGHradioButton;

		// Token: 0x0400042D RID: 1069
		private global::System.Windows.Forms.CheckBox RXIndicatorEnablecheckBox;

		// Token: 0x0400042E RID: 1070
		private global::System.Windows.Forms.ComboBox PlusMinLatcomboBox;

		// Token: 0x0400042F RID: 1071
		private global::System.Windows.Forms.ComboBox PlusMinLoncomboBox;

		// Token: 0x04000430 RID: 1072
		private global::System.Windows.Forms.Label label43;

		// Token: 0x04000431 RID: 1073
		private global::System.Windows.Forms.TextBox DMRidSimpleTextBox;

		// Token: 0x04000432 RID: 1074
		private global::System.Windows.Forms.ComboBox killTimerComboBox;

		// Token: 0x04000433 RID: 1075
		private global::System.Windows.Forms.Label label44;

		// Token: 0x04000434 RID: 1076
		private global::System.Windows.Forms.CheckBox alwaysOnTopcheckBox;

		// Token: 0x04000435 RID: 1077
		private global::System.Windows.Forms.Label label45;

		// Token: 0x04000436 RID: 1078
		private global::System.Windows.Forms.ComboBox languageComboBox;

		// Token: 0x04000437 RID: 1079
		private global::System.Windows.Forms.Label label46;

		// Token: 0x04000438 RID: 1080
		private global::System.Windows.Forms.Label label47;

		// Token: 0x04000439 RID: 1081
		private global::System.Windows.Forms.Label label48;

		// Token: 0x0400043A RID: 1082
		private global::System.Windows.Forms.TextBox DSTARSlowDataTextBox;

		// Token: 0x0400043B RID: 1083
		private global::System.Windows.Forms.CheckBox invertRXTXcheckBox;

		// Token: 0x0400043C RID: 1084
		private global::System.Windows.Forms.Label invertRXTXLabel;

		// Token: 0x0400043D RID: 1085
		private global::System.Windows.Forms.Label FrequencyWarninglabel;

		// Token: 0x0400043E RID: 1086
		private global::System.Windows.Forms.Label label49;

		// Token: 0x0400043F RID: 1087
		private global::System.Windows.Forms.ComboBox AMBEtypeComboBox;

		// Token: 0x04000440 RID: 1088
		private global::System.Windows.Forms.CheckBox invertDTRRadioCheckBox;

		// Token: 0x04000441 RID: 1089
		private global::System.Windows.Forms.GroupBox groupBox13;

		// Token: 0x04000442 RID: 1090
		private global::System.Windows.Forms.TextBox TGIFPassword;

		// Token: 0x04000443 RID: 1091
		private global::System.Windows.Forms.Label label16;

		// Token: 0x04000444 RID: 1092
		private global::System.Windows.Forms.GroupBox groupBox14;

		// Token: 0x04000445 RID: 1093
		private global::System.Windows.Forms.TextBox AMBENXDNid;

		// Token: 0x04000446 RID: 1094
		private global::System.Windows.Forms.Label label50;

		// Token: 0x04000447 RID: 1095
		private global::System.Windows.Forms.LinkLabel HelpNXDNlinkLabel;
	}
}
