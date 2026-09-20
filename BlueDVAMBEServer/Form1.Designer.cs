namespace BlueDVAMBEServer
{
	// Token: 0x02000002 RID: 2
	public partial class Form1 : global::System.Windows.Forms.Form
	{
		// Token: 0x0600000D RID: 13 RVA: 0x0000248E File Offset: 0x0000068E
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000024B0 File Offset: 0x000006B0
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BlueDVAMBEServer.Form1));
			this.StartButton = new global::System.Windows.Forms.Button();
			this.groupBox1 = new global::System.Windows.Forms.GroupBox();
			this.ClientIP = new global::System.Windows.Forms.Label();
			this.label2 = new global::System.Windows.Forms.Label();
			this.AMBEVersion = new global::System.Windows.Forms.Label();
			this.label1 = new global::System.Windows.Forms.Label();
			this.groupBox2 = new global::System.Windows.Forms.GroupBox();
			this.UDPportTextBox = new global::System.Windows.Forms.TextBox();
			this.label6 = new global::System.Windows.Forms.Label();
			this.BaudrateComboBox = new global::System.Windows.Forms.ComboBox();
			this.COMportcomboBox = new global::System.Windows.Forms.ComboBox();
			this.label4 = new global::System.Windows.Forms.Label();
			this.label3 = new global::System.Windows.Forms.Label();
			this.label5 = new global::System.Windows.Forms.Label();
			this.StopButton = new global::System.Windows.Forms.Button();
			this.version = new global::System.Windows.Forms.Label();
			this.autostartcheckbox = new global::System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			base.SuspendLayout();
			this.StartButton.Location = new global::System.Drawing.Point(12, 11);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = new global::System.Drawing.Size(75, 23);
			this.StartButton.TabIndex = 1;
			this.StartButton.Text = "Start";
			this.StartButton.UseVisualStyleBackColor = true;
			this.StartButton.Click += new global::System.EventHandler(this.button2_Click);
			this.groupBox1.Controls.Add(this.ClientIP);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.AMBEVersion);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new global::System.Drawing.Point(93, 1);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new global::System.Drawing.Size(204, 62);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Status";
			this.ClientIP.AutoSize = true;
			this.ClientIP.Location = new global::System.Drawing.Point(83, 38);
			this.ClientIP.Name = "ClientIP";
			this.ClientIP.Size = new global::System.Drawing.Size(69, 13);
			this.ClientIP.TabIndex = 3;
			this.ClientIP.Text = "Not detected";
			this.label2.AutoSize = true;
			this.label2.Location = new global::System.Drawing.Point(7, 38);
			this.label2.Name = "label2";
			this.label2.Size = new global::System.Drawing.Size(46, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Client IP";
			this.AMBEVersion.AutoSize = true;
			this.AMBEVersion.Location = new global::System.Drawing.Point(83, 20);
			this.AMBEVersion.Name = "AMBEVersion";
			this.AMBEVersion.Size = new global::System.Drawing.Size(69, 13);
			this.AMBEVersion.TabIndex = 1;
			this.AMBEVersion.Text = "Not detected";
			this.label1.AutoSize = true;
			this.label1.Location = new global::System.Drawing.Point(7, 20);
			this.label1.Name = "label1";
			this.label1.Size = new global::System.Drawing.Size(53, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Hardware";
			this.groupBox2.Controls.Add(this.UDPportTextBox);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.BaudrateComboBox);
			this.groupBox2.Controls.Add(this.COMportcomboBox);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Location = new global::System.Drawing.Point(93, 69);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new global::System.Drawing.Size(204, 92);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Settings";
			this.UDPportTextBox.Location = new global::System.Drawing.Point(86, 64);
			this.UDPportTextBox.Name = "UDPportTextBox";
			this.UDPportTextBox.Size = new global::System.Drawing.Size(108, 20);
			this.UDPportTextBox.TabIndex = 5;
			this.UDPportTextBox.TextChanged += new global::System.EventHandler(this.UDPportTextBox_TextChanged);
			this.label6.AutoSize = true;
			this.label6.Location = new global::System.Drawing.Point(7, 71);
			this.label6.Name = "label6";
			this.label6.Size = new global::System.Drawing.Size(51, 13);
			this.label6.TabIndex = 4;
			this.label6.Text = "UDP port";
			this.BaudrateComboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.BaudrateComboBox.FormattingEnabled = true;
			this.BaudrateComboBox.Items.AddRange(new object[] { "230400", "460800", "921600" });
			this.BaudrateComboBox.Location = new global::System.Drawing.Point(86, 37);
			this.BaudrateComboBox.Name = "BaudrateComboBox";
			this.BaudrateComboBox.Size = new global::System.Drawing.Size(108, 21);
			this.BaudrateComboBox.TabIndex = 3;
			this.BaudrateComboBox.SelectedIndexChanged += new global::System.EventHandler(this.BaudrateComboBox_SelectedIndexChanged);
			this.COMportcomboBox.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.COMportcomboBox.FormattingEnabled = true;
			this.COMportcomboBox.Location = new global::System.Drawing.Point(86, 11);
			this.COMportcomboBox.Name = "COMportcomboBox";
			this.COMportcomboBox.Size = new global::System.Drawing.Size(108, 21);
			this.COMportcomboBox.TabIndex = 2;
			this.COMportcomboBox.SelectedIndexChanged += new global::System.EventHandler(this.COMportcomboBox_SelectedIndexChanged);
			this.label4.AutoSize = true;
			this.label4.Location = new global::System.Drawing.Point(7, 45);
			this.label4.Name = "label4";
			this.label4.Size = new global::System.Drawing.Size(53, 13);
			this.label4.TabIndex = 1;
			this.label4.Text = "Baud rate";
			this.label3.AutoSize = true;
			this.label3.Location = new global::System.Drawing.Point(7, 20);
			this.label3.Name = "label3";
			this.label3.Size = new global::System.Drawing.Size(54, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "Serial port";
			this.label5.AutoSize = true;
			this.label5.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 6.75f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.label5.Location = new global::System.Drawing.Point(10, 106);
			this.label5.Name = "label5";
			this.label5.Size = new global::System.Drawing.Size(80, 12);
			this.label5.TabIndex = 4;
			this.label5.Text = "By David PA7LIM";
			this.StopButton.Enabled = false;
			this.StopButton.Location = new global::System.Drawing.Point(12, 40);
			this.StopButton.Name = "StopButton";
			this.StopButton.Size = new global::System.Drawing.Size(75, 23);
			this.StopButton.TabIndex = 5;
			this.StopButton.Text = "Stop";
			this.StopButton.UseVisualStyleBackColor = true;
			this.StopButton.Click += new global::System.EventHandler(this.StopButton_Click);
			this.version.AutoSize = true;
			this.version.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 6.75f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.version.Location = new global::System.Drawing.Point(10, 118);
			this.version.Name = "version";
			this.version.Size = new global::System.Drawing.Size(37, 12);
			this.version.TabIndex = 6;
			this.version.Text = "Version";
			this.autostartcheckbox.AutoSize = true;
			this.autostartcheckbox.Location = new global::System.Drawing.Point(12, 69);
			this.autostartcheckbox.Name = "autostartcheckbox";
			this.autostartcheckbox.Size = new global::System.Drawing.Size(71, 17);
			this.autostartcheckbox.TabIndex = 8;
			this.autostartcheckbox.Text = "Auto start";
			this.autostartcheckbox.UseVisualStyleBackColor = true;
			this.autostartcheckbox.CheckedChanged += new global::System.EventHandler(this.autostartcheckbox_CheckedChanged);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(307, 165);
			base.Controls.Add(this.autostartcheckbox);
			base.Controls.Add(this.version);
			base.Controls.Add(this.StopButton);
			base.Controls.Add(this.label5);
			base.Controls.Add(this.groupBox2);
			base.Controls.Add(this.groupBox1);
			base.Controls.Add(this.StartButton);
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "Form1";
			this.Text = "BlueDV AMBEServer";
			base.FormClosing += new global::System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			base.Load += new global::System.EventHandler(this.Form1_Load_1);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x04000001 RID: 1
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000002 RID: 2
		private global::System.Windows.Forms.Button StartButton;

		// Token: 0x04000003 RID: 3
		private global::System.Windows.Forms.GroupBox groupBox1;

		// Token: 0x04000004 RID: 4
		private global::System.Windows.Forms.Label AMBEVersion;

		// Token: 0x04000005 RID: 5
		private global::System.Windows.Forms.Label label1;

		// Token: 0x04000006 RID: 6
		private global::System.Windows.Forms.Label ClientIP;

		// Token: 0x04000007 RID: 7
		private global::System.Windows.Forms.Label label2;

		// Token: 0x04000008 RID: 8
		private global::System.Windows.Forms.GroupBox groupBox2;

		// Token: 0x04000009 RID: 9
		private global::System.Windows.Forms.ComboBox BaudrateComboBox;

		// Token: 0x0400000A RID: 10
		private global::System.Windows.Forms.ComboBox COMportcomboBox;

		// Token: 0x0400000B RID: 11
		private global::System.Windows.Forms.Label label4;

		// Token: 0x0400000C RID: 12
		private global::System.Windows.Forms.Label label3;

		// Token: 0x0400000D RID: 13
		private global::System.Windows.Forms.Label label5;

		// Token: 0x0400000E RID: 14
		private global::System.Windows.Forms.Button StopButton;

		// Token: 0x0400000F RID: 15
		private global::System.Windows.Forms.TextBox UDPportTextBox;

		// Token: 0x04000010 RID: 16
		private global::System.Windows.Forms.Label label6;

		// Token: 0x04000011 RID: 17
		private global::System.Windows.Forms.Label version;

		// Token: 0x04000012 RID: 18
		private global::System.Windows.Forms.CheckBox autostartcheckbox;
	}
}
