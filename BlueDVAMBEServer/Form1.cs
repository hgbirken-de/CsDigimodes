using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Windows.Forms;
using BlueDVAMBEServer.Properties;

namespace BlueDVAMBEServer
{
	// Token: 0x02000002 RID: 2
	public partial class Form1 : Form
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		private void SetTextAMBESerial(string text)
		{
			if (this.AMBEVersion.InvokeRequired)
			{
				Form1.SetTextCallback setTextCallback = new Form1.SetTextCallback(this.SetTextAMBESerial);
				base.Invoke(setTextCallback, new object[] { text });
				return;
			}
			this.AMBEVersion.Text = text;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002098 File Offset: 0x00000298
		private void SetTextClientIP(string text)
		{
			if (this.ClientIP.InvokeRequired)
			{
				Form1.SetTextCallback setTextCallback = new Form1.SetTextCallback(this.SetTextClientIP);
				base.Invoke(setTextCallback, new object[] { text });
				return;
			}
			this.ClientIP.Text = text;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x000020E0 File Offset: 0x000002E0
		public Form1()
		{
			this.InitializeComponent();
			serial.StatusTextChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetTextAMBESerial(serial.StatusText);
			};
			UDPServerTest.StatusTextChanged += delegate(object sender1, EventArgs e1)
			{
				this.SetTextClientIP(UDPServerTest.StatusText);
			};
			if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "AMBEServerLog.txt")))
			{
				File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "AMBEServerLog.txt"));
			}
			foreach (string text in SerialPort.GetPortNames())
			{
				this.COMportcomboBox.Items.Add(text);
			}
			int num = this.COMportcomboBox.FindString(Settings.Default.comport);
			this.COMportcomboBox.SelectedIndex = num;
			information.myCOMPort = Settings.Default.comport;
			int num2 = this.BaudrateComboBox.FindString(Settings.Default.baudrate);
			this.BaudrateComboBox.SelectedIndex = num2;
			int num3;
			if (int.TryParse(Settings.Default.baudrate, out num3))
			{
				information.mySerialBaudRate = num3;
			}
			else
			{
				information.mySerialBaudRate = 460800;
			}
			int num4;
			if (int.TryParse(Settings.Default.UDPport, out num4))
			{
				information.UDPport = num4;
			}
			else
			{
				information.UDPport = 2460;
			}
			this.UDPportTextBox.Text = information.UDPport.ToString();
			this.autostartcheckbox.Checked = Settings.Default.autostart;
			this.version.Text = "Version : " + Assembly.GetExecutingAssembly().GetName().Version.ToString().Trim();
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002270 File Offset: 0x00000470
		private void button1_Click(object sender, EventArgs e)
		{
			serial.setParityOff();
			serial.getAMBEproductID();
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000227C File Offset: 0x0000047C
		private void button2_Click(object sender, EventArgs e)
		{
			this.COMportcomboBox.Enabled = false;
			this.BaudrateComboBox.Enabled = false;
			this.UDPportTextBox.Enabled = false;
			this.StopButton.Enabled = true;
			this.StartButton.Enabled = false;
			serial.open();
			serial.setParityOff();
			serial.getAMBEproductID();
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000022D5 File Offset: 0x000004D5
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			serial.close();
			UDPServerTest.Stop();
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000022E1 File Offset: 0x000004E1
		private void COMportcomboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			Settings.Default.comport = this.COMportcomboBox.GetItemText(this.COMportcomboBox.SelectedItem);
			Settings.Default.Save();
			information.myCOMPort = Settings.Default.comport;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000231C File Offset: 0x0000051C
		private void BaudrateComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			Settings.Default.baudrate = this.BaudrateComboBox.GetItemText(this.BaudrateComboBox.SelectedItem);
			Settings.Default.Save();
			int num;
			if (int.TryParse(Settings.Default.baudrate, out num))
			{
				information.mySerialBaudRate = num;
				return;
			}
			information.mySerialBaudRate = 460800;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002378 File Offset: 0x00000578
		private void StopButton_Click(object sender, EventArgs e)
		{
			UDPServerTest.Stop();
			serial.close();
			information.AMBE6000R_FOUND = false;
			this.COMportcomboBox.Enabled = true;
			this.BaudrateComboBox.Enabled = true;
			this.UDPportTextBox.Enabled = true;
			this.StopButton.Enabled = false;
			this.StartButton.Enabled = true;
			this.AMBEVersion.Text = "";
			this.ClientIP.Text = "";
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000023F4 File Offset: 0x000005F4
		private void UDPportTextBox_TextChanged(object sender, EventArgs e)
		{
			Settings.Default.UDPport = this.UDPportTextBox.Text;
			Settings.Default.Save();
			int num;
			if (int.TryParse(Settings.Default.UDPport, out num))
			{
				information.UDPport = num;
				return;
			}
			information.UDPport = 2460;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002444 File Offset: 0x00000644
		private void autostartcheckbox_CheckedChanged(object sender, EventArgs e)
		{
			if (this.autostartcheckbox.Checked)
			{
				Settings.Default.autostart = true;
			}
			else
			{
				Settings.Default.autostart = false;
			}
			Settings.Default.Save();
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002475 File Offset: 0x00000675
		private void Form1_Load_1(object sender, EventArgs e)
		{
			if (Settings.Default.autostart)
			{
				this.StartButton.PerformClick();
			}
		}

		// Token: 0x0200000B RID: 11
		// (Invoke) Token: 0x06000046 RID: 70
		private delegate void SetTextCallback(string text);
	}
}
