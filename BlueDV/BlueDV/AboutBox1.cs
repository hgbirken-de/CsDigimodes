using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace BlueDV
{
	// Token: 0x02000008 RID: 8
	internal partial class AboutBox1 : Form
	{
		// Token: 0x0600002B RID: 43 RVA: 0x00002D18 File Offset: 0x00000F18
		public AboutBox1()
		{
			this.InitializeComponent();
			switch (utils.RunningPlatform())
			{
			case utils.Platform.Windows:
				this.Text = "BlueDV for Windows";
				this.Text = string.Format("About {0}", this.AssemblyTitle);
				this.labelProductName.Text = this.AssemblyProduct;
				break;
			case utils.Platform.Linux:
				this.Text = "BlueDV for Linux";
				this.labelProductName.Text = "BlueDVLinux";
				break;
			case utils.Platform.Mac:
				this.Text = "BlueDV for OSX";
				break;
			}
			this.labelVersion.Text = string.Format("Version {0}", this.AssemblyVersion);
			this.labelCopyright.Text = this.AssemblyCopyright;
			this.labelCompanyName.Text = this.AssemblyCompany;
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600002C RID: 44 RVA: 0x00002DE4 File Offset: 0x00000FE4
		public string AssemblyTitle
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if (customAttributes.Length != 0)
				{
					AssemblyTitleAttribute assemblyTitleAttribute = (AssemblyTitleAttribute)customAttributes[0];
					if (assemblyTitleAttribute.Title != "")
					{
						return assemblyTitleAttribute.Title;
					}
				}
				return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600002D RID: 45 RVA: 0x00002E3C File Offset: 0x0000103C
		public string AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600002E RID: 46 RVA: 0x00002E54 File Offset: 0x00001054
		public string AssemblyDescription
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				if (customAttributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyDescriptionAttribute)customAttributes[0]).Description;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600002F RID: 47 RVA: 0x00002E90 File Offset: 0x00001090
		public string AssemblyProduct
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				if (customAttributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyProductAttribute)customAttributes[0]).Product;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000030 RID: 48 RVA: 0x00002ECC File Offset: 0x000010CC
		public string AssemblyCopyright
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				if (customAttributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCopyrightAttribute)customAttributes[0]).Copyright;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000031 RID: 49 RVA: 0x00002F08 File Offset: 0x00001108
		public string AssemblyCompany
		{
			get
			{
				object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				if (customAttributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCompanyAttribute)customAttributes[0]).Company;
			}
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00002F42 File Offset: 0x00001142
		private void AboutBox1_Load(object sender, EventArgs e)
		{
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00002F42 File Offset: 0x00001142
		private void okButton_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00002F42 File Offset: 0x00001142
		private void labelProductName_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002F42 File Offset: 0x00001142
		private void textBoxDescription_TextChanged(object sender, EventArgs e)
		{
		}
	}
}
