namespace BlueDV
{
	// Token: 0x02000008 RID: 8
	internal partial class AboutBox1 : global::System.Windows.Forms.Form
	{
		// Token: 0x06000036 RID: 54 RVA: 0x00002F44 File Offset: 0x00001144
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00002F64 File Offset: 0x00001164
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BlueDV.AboutBox1));
			this.tableLayoutPanel = new global::System.Windows.Forms.TableLayoutPanel();
			this.labelProductName = new global::System.Windows.Forms.Label();
			this.labelVersion = new global::System.Windows.Forms.Label();
			this.labelCopyright = new global::System.Windows.Forms.Label();
			this.labelCompanyName = new global::System.Windows.Forms.Label();
			this.textBoxDescription = new global::System.Windows.Forms.TextBox();
			this.okButton = new global::System.Windows.Forms.Button();
			this.tableLayoutPanel.SuspendLayout();
			base.SuspendLayout();
			componentResourceManager.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
			this.tableLayoutPanel.Controls.Add(this.labelProductName, 6, 0);
			this.tableLayoutPanel.Controls.Add(this.labelVersion, 1, 1);
			this.tableLayoutPanel.Controls.Add(this.labelCopyright, 1, 2);
			this.tableLayoutPanel.Controls.Add(this.labelCompanyName, 1, 3);
			this.tableLayoutPanel.Controls.Add(this.textBoxDescription, 1, 4);
			this.tableLayoutPanel.Controls.Add(this.okButton, 0, 5);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			componentResourceManager.ApplyResources(this.labelProductName, "labelProductName");
			this.labelProductName.Name = "labelProductName";
			this.labelProductName.Click += new global::System.EventHandler(this.labelProductName_Click);
			componentResourceManager.ApplyResources(this.labelVersion, "labelVersion");
			this.labelVersion.Name = "labelVersion";
			componentResourceManager.ApplyResources(this.labelCopyright, "labelCopyright");
			this.labelCopyright.Name = "labelCopyright";
			componentResourceManager.ApplyResources(this.labelCompanyName, "labelCompanyName");
			this.labelCompanyName.Name = "labelCompanyName";
			this.textBoxDescription.AcceptsReturn = true;
			componentResourceManager.ApplyResources(this.textBoxDescription, "textBoxDescription");
			this.textBoxDescription.Name = "textBoxDescription";
			this.textBoxDescription.ReadOnly = true;
			this.textBoxDescription.TabStop = false;
			this.textBoxDescription.TextChanged += new global::System.EventHandler(this.textBoxDescription_TextChanged);
			componentResourceManager.ApplyResources(this.okButton, "okButton");
			this.okButton.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
			this.okButton.Name = "okButton";
			this.okButton.Click += new global::System.EventHandler(this.okButton_Click);
			base.AcceptButton = this.okButton;
			componentResourceManager.ApplyResources(this, "$this");
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(this.tableLayoutPanel);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "AboutBox1";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.Load += new global::System.EventHandler(this.AboutBox1_Load);
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			base.ResumeLayout(false);
		}

		// Token: 0x04000010 RID: 16
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000011 RID: 17
		private global::System.Windows.Forms.TableLayoutPanel tableLayoutPanel;

		// Token: 0x04000012 RID: 18
		private global::System.Windows.Forms.Label labelProductName;

		// Token: 0x04000013 RID: 19
		private global::System.Windows.Forms.Label labelVersion;

		// Token: 0x04000014 RID: 20
		private global::System.Windows.Forms.Label labelCopyright;

		// Token: 0x04000015 RID: 21
		private global::System.Windows.Forms.Label labelCompanyName;

		// Token: 0x04000016 RID: 22
		private global::System.Windows.Forms.TextBox textBoxDescription;

		// Token: 0x04000017 RID: 23
		private global::System.Windows.Forms.Button okButton;
	}
}
