using System;
using System.IO;
using System.IO.Ports;

namespace BlueDV
{
	// Token: 0x02000009 RID: 9
	internal class AMBEPTTControl
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000038 RID: 56 RVA: 0x00003257 File Offset: 0x00001457
		// (set) Token: 0x06000039 RID: 57 RVA: 0x0000325E File Offset: 0x0000145E
		public static string StatusTextControl { get; private set; }

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x0600003A RID: 58 RVA: 0x00003268 File Offset: 0x00001468
		// (remove) Token: 0x0600003B RID: 59 RVA: 0x0000329C File Offset: 0x0000149C
		public static event EventHandler StatusTextChangedControl;

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600003C RID: 60 RVA: 0x000032CF File Offset: 0x000014CF
		// (set) Token: 0x0600003D RID: 61 RVA: 0x000032D6 File Offset: 0x000014D6
		public static string StatusErrorTextControl { get; private set; }

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x0600003E RID: 62 RVA: 0x000032E0 File Offset: 0x000014E0
		// (remove) Token: 0x0600003F RID: 63 RVA: 0x00003314 File Offset: 0x00001514
		public static event EventHandler StatusErrorTextChangedControl;

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000040 RID: 64 RVA: 0x00003347 File Offset: 0x00001547
		// (set) Token: 0x06000041 RID: 65 RVA: 0x0000334E File Offset: 0x0000154E
		public static bool StatusBoolAMBE_PTT { get; private set; }

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000042 RID: 66 RVA: 0x00003358 File Offset: 0x00001558
		// (remove) Token: 0x06000043 RID: 67 RVA: 0x0000338C File Offset: 0x0000158C
		public static event EventHandler StatusBoolChangedAMBE_PTT;

		// Token: 0x06000044 RID: 68 RVA: 0x000033C0 File Offset: 0x000015C0
		public static bool open()
		{
			if (AMBEPTTControl.serialPort1 == null)
			{
				AMBEPTTControl.serialPort1 = new SerialPort();
			}
			if (string.IsNullOrEmpty(information.myCOMPortPTT))
			{
				AMBEPTTControl.ChangeStatusTextControl("PTT: Serial port not found");
				AMBEPTTControl.serialPort1.Close();
				return false;
			}
			if (string.IsNullOrEmpty(information.myCOMPortAMBE))
			{
				AMBEPTTControl.ChangeStatusTextControl("PTT: Serial port not found");
				AMBEPTTControl.serialPort1.Close();
				return false;
			}
			bool flag;
			try
			{
				if (!AMBEPTTControl.serialPort1.IsOpen)
				{
					AMBEPTTControl.serialPort1.PortName = information.myCOMPortPTT;
					AMBEPTTControl.serialPort1.BaudRate = 115200;
					AMBEPTTControl.serialPort1.DataBits = 8;
					AMBEPTTControl.serialPort1.Parity = Parity.None;
					AMBEPTTControl.serialPort1.StopBits = StopBits.One;
					AMBEPTTControl.serialPort1.Handshake = Handshake.None;
					AMBEPTTControl.serialPort1.PinChanged -= AMBEPTTControl.port_PinChanged;
					AMBEPTTControl.serialPort1.PinChanged += AMBEPTTControl.port_PinChanged;
					AMBEPTTControl.serialPort1.Open();
				}
				AMBEPTTControl.ctsTrigger();
				AMBEPTTControl.dsrTrigger();
				AMBEPTTControl.setInitialRXState();
				flag = true;
			}
			catch (IOException)
			{
				AMBEPTTControl.ChangeStatusTextControl("PTT: Serial port IO error");
				try
				{
					AMBEPTTControl.serialPort1.Close();
				}
				catch (Exception)
				{
				}
				flag = false;
			}
			catch (IndexOutOfRangeException)
			{
				AMBEPTTControl.ChangeStatusTextControl("PTT: Serial port issue");
				flag = false;
			}
			catch (UnauthorizedAccessException)
			{
				flag = false;
			}
			catch (Exception)
			{
				AMBEPTTControl.ChangeStatusTextControl("PTT: Serial port issue");
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00003548 File Offset: 0x00001748
		private static bool setInitialRXState()
		{
			if (information.enableRXIndicator)
			{
				information.RXINDICATOR rxindicator = information.m_rxindicator;
				if (rxindicator != information.RXINDICATOR.RTS)
				{
					if (rxindicator == information.RXINDICATOR.DTR)
					{
						if (information.m_RXhighlow == information.HIGHLOW.LOW)
						{
							AMBEPTTControl.serialPort1.DtrEnable = false;
						}
						else
						{
							AMBEPTTControl.serialPort1.DtrEnable = true;
						}
					}
				}
				else if (information.m_RXhighlow == information.HIGHLOW.LOW)
				{
					AMBEPTTControl.serialPort1.RtsEnable = false;
				}
				else
				{
					AMBEPTTControl.serialPort1.RtsEnable = true;
				}
			}
			else
			{
				AMBEPTTControl.serialPort1.RtsEnable = false;
				AMBEPTTControl.serialPort1.DtrEnable = false;
			}
			return true;
		}

		// Token: 0x06000046 RID: 70 RVA: 0x000035C8 File Offset: 0x000017C8
		public static void setRXstatus(bool state)
		{
			if (information.usePTTkeying && information.enableRXIndicator)
			{
				information.RXINDICATOR rxindicator = information.m_rxindicator;
				if (rxindicator == information.RXINDICATOR.RTS)
				{
					AMBEPTTControl.setRTS(state);
					return;
				}
				if (rxindicator != information.RXINDICATOR.DTR)
				{
					return;
				}
				AMBEPTTControl.setDTR(state);
			}
		}

		// Token: 0x06000047 RID: 71 RVA: 0x000035FF File Offset: 0x000017FF
		private static void setRTS(bool rtsState)
		{
			if (information.enableRXIndicator && AMBEPTTControl.serialPort1 != null)
			{
				if (information.m_RXhighlow == information.HIGHLOW.LOW)
				{
					AMBEPTTControl.serialPort1.RtsEnable = rtsState;
					return;
				}
				AMBEPTTControl.serialPort1.RtsEnable = !rtsState;
			}
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00003631 File Offset: 0x00001831
		private static void setDTR(bool dtrState)
		{
			if (AMBEPTTControl.serialPort1 != null && information.enableRXIndicator)
			{
				if (information.m_RXhighlow == information.HIGHLOW.LOW)
				{
					AMBEPTTControl.serialPort1.DtrEnable = dtrState;
					return;
				}
				AMBEPTTControl.serialPort1.DtrEnable = !dtrState;
			}
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00003664 File Offset: 0x00001864
		private static void port_PinChanged(object sender, SerialPinChangedEventArgs e)
		{
			if (e.EventType != SerialPinChange.Break && e.EventType != SerialPinChange.CDChanged)
			{
				if (e.EventType == SerialPinChange.CtsChanged && information.m_pttindicator == information.PTTINDICATOR.CTS)
				{
					AMBEPTTControl.ctsTrigger();
					return;
				}
				if (e.EventType == SerialPinChange.DsrChanged && information.m_pttindicator == information.PTTINDICATOR.DSR)
				{
					AMBEPTTControl.dsrTrigger();
					return;
				}
				SerialPinChange eventType = e.EventType;
			}
		}

		// Token: 0x0600004A RID: 74 RVA: 0x000036C4 File Offset: 0x000018C4
		public static void ctsTrigger()
		{
			if (information.m_pttindicator == information.PTTINDICATOR.CTS)
			{
				if (AMBEPTTControl.serialPort1.CtsHolding)
				{
					if (information.m_PTThighlow == information.HIGHLOW.LOW)
					{
						AMBEPTTControl.ChangeStatusBoolAMBE_PTT(true);
						return;
					}
					AMBEPTTControl.ChangeStatusBoolAMBE_PTT(false);
					return;
				}
				else
				{
					if (information.m_PTThighlow == information.HIGHLOW.LOW)
					{
						AMBEPTTControl.ChangeStatusBoolAMBE_PTT(false);
						return;
					}
					AMBEPTTControl.ChangeStatusBoolAMBE_PTT(true);
				}
			}
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00003710 File Offset: 0x00001910
		public static void dsrTrigger()
		{
			if (information.m_pttindicator == information.PTTINDICATOR.DSR)
			{
				if (AMBEPTTControl.serialPort1.DsrHolding)
				{
					if (information.m_PTThighlow == information.HIGHLOW.LOW)
					{
						AMBEPTTControl.ChangeStatusBoolAMBE_PTT(true);
						return;
					}
					AMBEPTTControl.ChangeStatusBoolAMBE_PTT(false);
					return;
				}
				else
				{
					if (information.m_PTThighlow == information.HIGHLOW.LOW)
					{
						AMBEPTTControl.ChangeStatusBoolAMBE_PTT(false);
						return;
					}
					AMBEPTTControl.ChangeStatusBoolAMBE_PTT(true);
				}
			}
		}

		// Token: 0x0600004C RID: 76 RVA: 0x0000375C File Offset: 0x0000195C
		public static void close()
		{
			if (AMBEPTTControl.serialPort1 != null && AMBEPTTControl.serialPort1.IsOpen)
			{
				AMBEPTTControl.serialPort1.Close();
			}
		}

		// Token: 0x0600004D RID: 77 RVA: 0x0000377C File Offset: 0x0000197C
		private static void ChangeStatusTextControl(string text)
		{
			AMBEPTTControl.StatusTextControl = text;
			EventHandler statusTextChangedControl = AMBEPTTControl.StatusTextChangedControl;
			if (statusTextChangedControl != null)
			{
				statusTextChangedControl(null, EventArgs.Empty);
			}
		}

		// Token: 0x0600004E RID: 78 RVA: 0x000037A4 File Offset: 0x000019A4
		private static void ChangeErrorStatusTextControl(string text)
		{
			AMBEPTTControl.StatusErrorTextControl = text;
			EventHandler statusErrorTextChangedControl = AMBEPTTControl.StatusErrorTextChangedControl;
			if (statusErrorTextChangedControl != null)
			{
				statusErrorTextChangedControl(null, EventArgs.Empty);
			}
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000037CC File Offset: 0x000019CC
		private static void ChangeStatusBoolAMBE_PTT(bool vox)
		{
			AMBEPTTControl.StatusBoolAMBE_PTT = vox;
			EventHandler statusBoolChangedAMBE_PTT = AMBEPTTControl.StatusBoolChangedAMBE_PTT;
			if (statusBoolChangedAMBE_PTT != null)
			{
				statusBoolChangedAMBE_PTT(null, EventArgs.Empty);
			}
		}

		// Token: 0x0400001E RID: 30
		private static SerialPort serialPort1;
	}
}
