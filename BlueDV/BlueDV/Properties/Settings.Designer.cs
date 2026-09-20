using System;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace BlueDV.Properties
{
	// Token: 0x02000050 RID: 80
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.9.0.0")]
	internal sealed partial class Settings : ApplicationSettingsBase
	{
		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x060005F1 RID: 1521 RVA: 0x00035A3F File Offset: 0x00033C3F
		public static Settings Default
		{
			get
			{
				return Settings.defaultInstance;
			}
		}

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x060005F2 RID: 1522 RVA: 0x00035A46 File Offset: 0x00033C46
		// (set) Token: 0x060005F3 RID: 1523 RVA: 0x00035A58 File Offset: 0x00033C58
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool disableFonts
		{
			get
			{
				return (bool)this["disableFonts"];
			}
			set
			{
				this["disableFonts"] = value;
			}
		}

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x060005F4 RID: 1524 RVA: 0x00035A6B File Offset: 0x00033C6B
		// (set) Token: 0x060005F5 RID: 1525 RVA: 0x00035A7D File Offset: 0x00033C7D
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("REF001")]
		public string savedREFreflector
		{
			get
			{
				return (string)this["savedREFreflector"];
			}
			set
			{
				this["savedREFreflector"] = value;
			}
		}

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x060005F6 RID: 1526 RVA: 0x00035A8B File Offset: 0x00033C8B
		// (set) Token: 0x060005F7 RID: 1527 RVA: 0x00035A9D File Offset: 0x00033C9D
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("A")]
		public string savedREFreflectorModule
		{
			get
			{
				return (string)this["savedREFreflectorModule"];
			}
			set
			{
				this["savedREFreflectorModule"] = value;
			}
		}

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x060005F8 RID: 1528 RVA: 0x00035AAB File Offset: 0x00033CAB
		// (set) Token: 0x060005F9 RID: 1529 RVA: 0x00035ABD File Offset: 0x00033CBD
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("DCS001")]
		public string savedDCSreflector
		{
			get
			{
				return (string)this["savedDCSreflector"];
			}
			set
			{
				this["savedDCSreflector"] = value;
			}
		}

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x060005FA RID: 1530 RVA: 0x00035ACB File Offset: 0x00033CCB
		// (set) Token: 0x060005FB RID: 1531 RVA: 0x00035ADD File Offset: 0x00033CDD
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("B")]
		public string savedDCSreflectorModule
		{
			get
			{
				return (string)this["savedDCSreflectorModule"];
			}
			set
			{
				this["savedDCSreflectorModule"] = value;
			}
		}

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x060005FC RID: 1532 RVA: 0x00035AEB File Offset: 0x00033CEB
		// (set) Token: 0x060005FD RID: 1533 RVA: 0x00035AFD File Offset: 0x00033CFD
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("XRF001")]
		public string savedXRFreflector
		{
			get
			{
				return (string)this["savedXRFreflector"];
			}
			set
			{
				this["savedXRFreflector"] = value;
			}
		}

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x060005FE RID: 1534 RVA: 0x00035B0B File Offset: 0x00033D0B
		// (set) Token: 0x060005FF RID: 1535 RVA: 0x00035B1D File Offset: 0x00033D1D
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("C")]
		public string savedXRFreflectorModule
		{
			get
			{
				return (string)this["savedXRFreflectorModule"];
			}
			set
			{
				this["savedXRFreflectorModule"] = value;
			}
		}

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x06000600 RID: 1536 RVA: 0x00035B2B File Offset: 0x00033D2B
		// (set) Token: 0x06000601 RID: 1537 RVA: 0x00035B3D File Offset: 0x00033D3D
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool savedDSTARorDMRswitch
		{
			get
			{
				return (bool)this["savedDSTARorDMRswitch"];
			}
			set
			{
				this["savedDSTARorDMRswitch"] = value;
			}
		}

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x06000602 RID: 1538 RVA: 0x00035B50 File Offset: 0x00033D50
		// (set) Token: 0x06000603 RID: 1539 RVA: 0x00035B62 File Offset: 0x00033D62
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool savedprivateorGroupswitch
		{
			get
			{
				return (bool)this["savedprivateorGroupswitch"];
			}
			set
			{
				this["savedprivateorGroupswitch"] = value;
			}
		}

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x06000604 RID: 1540 RVA: 0x00035B75 File Offset: 0x00033D75
		// (set) Token: 0x06000605 RID: 1541 RVA: 0x00035B87 File Offset: 0x00033D87
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("204")]
		public string savedDMRmanualDial
		{
			get
			{
				return (string)this["savedDMRmanualDial"];
			}
			set
			{
				this["savedDMRmanualDial"] = value;
			}
		}

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x06000606 RID: 1542 RVA: 0x00035B95 File Offset: 0x00033D95
		// (set) Token: 0x06000607 RID: 1543 RVA: 0x00035BA7 File Offset: 0x00033DA7
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("50")]
		public int recordingVolumeDMR
		{
			get
			{
				return (int)this["recordingVolumeDMR"];
			}
			set
			{
				this["recordingVolumeDMR"] = value;
			}
		}

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x06000608 RID: 1544 RVA: 0x00035BBA File Offset: 0x00033DBA
		// (set) Token: 0x06000609 RID: 1545 RVA: 0x00035BCC File Offset: 0x00033DCC
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("1000")]
		public int savedVOXDetectionLevel
		{
			get
			{
				return (int)this["savedVOXDetectionLevel"];
			}
			set
			{
				this["savedVOXDetectionLevel"] = value;
			}
		}

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x0600060A RID: 1546 RVA: 0x00035BDF File Offset: 0x00033DDF
		// (set) Token: 0x0600060B RID: 1547 RVA: 0x00035BF1 File Offset: 0x00033DF1
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("5")]
		public int savedDSTARoutGain
		{
			get
			{
				return (int)this["savedDSTARoutGain"];
			}
			set
			{
				this["savedDSTARoutGain"] = value;
			}
		}

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x0600060C RID: 1548 RVA: 0x00035C04 File Offset: 0x00033E04
		// (set) Token: 0x0600060D RID: 1549 RVA: 0x00035C16 File Offset: 0x00033E16
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("5")]
		public int savedDSTARinGain
		{
			get
			{
				return (int)this["savedDSTARinGain"];
			}
			set
			{
				this["savedDSTARinGain"] = value;
			}
		}

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x0600060E RID: 1550 RVA: 0x00035C29 File Offset: 0x00033E29
		// (set) Token: 0x0600060F RID: 1551 RVA: 0x00035C3B File Offset: 0x00033E3B
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("3")]
		public int savedDMRoutGain
		{
			get
			{
				return (int)this["savedDMRoutGain"];
			}
			set
			{
				this["savedDMRoutGain"] = value;
			}
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x06000610 RID: 1552 RVA: 0x00035C4E File Offset: 0x00033E4E
		// (set) Token: 0x06000611 RID: 1553 RVA: 0x00035C60 File Offset: 0x00033E60
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("-5")]
		public int savedDMRinGain
		{
			get
			{
				return (int)this["savedDMRinGain"];
			}
			set
			{
				this["savedDMRinGain"] = value;
			}
		}

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x06000612 RID: 1554 RVA: 0x00035C73 File Offset: 0x00033E73
		// (set) Token: 0x06000613 RID: 1555 RVA: 0x00035C85 File Offset: 0x00033E85
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool savedDMRsimpleMode
		{
			get
			{
				return (bool)this["savedDMRsimpleMode"];
			}
			set
			{
				this["savedDMRsimpleMode"] = value;
			}
		}

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x06000614 RID: 1556 RVA: 0x00035C98 File Offset: 0x00033E98
		// (set) Token: 0x06000615 RID: 1557 RVA: 0x00035CAA File Offset: 0x00033EAA
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("XLX001")]
		public string savedXLXreflector
		{
			get
			{
				return (string)this["savedXLXreflector"];
			}
			set
			{
				this["savedXLXreflector"] = value;
			}
		}

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x06000616 RID: 1558 RVA: 0x00035CB8 File Offset: 0x00033EB8
		// (set) Token: 0x06000617 RID: 1559 RVA: 0x00035CCA File Offset: 0x00033ECA
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("B")]
		public string savedXLXreflectorModule
		{
			get
			{
				return (string)this["savedXLXreflectorModule"];
			}
			set
			{
				this["savedXLXreflectorModule"] = value;
			}
		}

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x06000618 RID: 1560 RVA: 0x00035CD8 File Offset: 0x00033ED8
		// (set) Token: 0x06000619 RID: 1561 RVA: 0x00035CEA File Offset: 0x00033EEA
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("5")]
		public int savedFUSIONoutGain
		{
			get
			{
				return (int)this["savedFUSIONoutGain"];
			}
			set
			{
				this["savedFUSIONoutGain"] = value;
			}
		}

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x0600061A RID: 1562 RVA: 0x00035CFD File Offset: 0x00033EFD
		// (set) Token: 0x0600061B RID: 1563 RVA: 0x00035D0F File Offset: 0x00033F0F
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("-5")]
		public int savedFUSIONinGain
		{
			get
			{
				return (int)this["savedFUSIONinGain"];
			}
			set
			{
				this["savedFUSIONinGain"] = value;
			}
		}

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x0600061C RID: 1564 RVA: 0x00035D22 File Offset: 0x00033F22
		// (set) Token: 0x0600061D RID: 1565 RVA: 0x00035D34 File Offset: 0x00033F34
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("JP3YIX")]
		public string savedJPNreflector
		{
			get
			{
				return (string)this["savedJPNreflector"];
			}
			set
			{
				this["savedJPNreflector"] = value;
			}
		}

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x0600061E RID: 1566 RVA: 0x00035D42 File Offset: 0x00033F42
		// (set) Token: 0x0600061F RID: 1567 RVA: 0x00035D54 File Offset: 0x00033F54
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("B")]
		public string savedJPNreflectorModule
		{
			get
			{
				return (string)this["savedJPNreflectorModule"];
			}
			set
			{
				this["savedJPNreflectorModule"] = value;
			}
		}

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x06000620 RID: 1568 RVA: 0x00035D62 File Offset: 0x00033F62
		// (set) Token: 0x06000621 RID: 1569 RVA: 0x00035D74 File Offset: 0x00033F74
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("JPN")]
		public string savedReflectorType
		{
			get
			{
				return (string)this["savedReflectorType"];
			}
			set
			{
				this["savedReflectorType"] = value;
			}
		}

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x06000622 RID: 1570 RVA: 0x00035D82 File Offset: 0x00033F82
		// (set) Token: 0x06000623 RID: 1571 RVA: 0x00035D94 File Offset: 0x00033F94
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("0, 0")]
		public Point F1Location
		{
			get
			{
				return (Point)this["F1Location"];
			}
			set
			{
				this["F1Location"] = value;
			}
		}

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x06000624 RID: 1572 RVA: 0x00035DA7 File Offset: 0x00033FA7
		// (set) Token: 0x06000625 RID: 1573 RVA: 0x00035DB9 File Offset: 0x00033FB9
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("3000")]
		public int savedVOXHangTime
		{
			get
			{
				return (int)this["savedVOXHangTime"];
			}
			set
			{
				this["savedVOXHangTime"] = value;
			}
		}

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x06000626 RID: 1574 RVA: 0x00035DCC File Offset: 0x00033FCC
		// (set) Token: 0x06000627 RID: 1575 RVA: 0x00035DDE File Offset: 0x00033FDE
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("EUROPELINK")]
		public string savedYSFreflector
		{
			get
			{
				return (string)this["savedYSFreflector"];
			}
			set
			{
				this["savedYSFreflector"] = value;
			}
		}

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x06000628 RID: 1576 RVA: 0x00035DEC File Offset: 0x00033FEC
		// (set) Token: 0x06000629 RID: 1577 RVA: 0x00035DFE File Offset: 0x00033FFE
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("0")]
		public int savedDGID
		{
			get
			{
				return (int)this["savedDGID"];
			}
			set
			{
				this["savedDGID"] = value;
			}
		}

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x0600062A RID: 1578 RVA: 0x00035E11 File Offset: 0x00034011
		// (set) Token: 0x0600062B RID: 1579 RVA: 0x00035E23 File Offset: 0x00034023
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("EU")]
		public string savedFREEDMRreflector
		{
			get
			{
				return (string)this["savedFREEDMRreflector"];
			}
			set
			{
				this["savedFREEDMRreflector"] = value;
			}
		}

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x0600062C RID: 1580 RVA: 0x00035E31 File Offset: 0x00034031
		// (set) Token: 0x0600062D RID: 1581 RVA: 0x00035E43 File Offset: 0x00034043
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("Europe")]
		public string savedSYSTEMXreflector
		{
			get
			{
				return (string)this["savedSYSTEMXreflector"];
			}
			set
			{
				this["savedSYSTEMXreflector"] = value;
			}
		}

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x0600062E RID: 1582 RVA: 0x00035E51 File Offset: 0x00034051
		// (set) Token: 0x0600062F RID: 1583 RVA: 0x00035E63 File Offset: 0x00034063
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n  <string>4000</string>\r\n</ArrayOfString>")]
		public StringCollection savedDMRManualDialCollection
		{
			get
			{
				return (StringCollection)this["savedDMRManualDialCollection"];
			}
			set
			{
				this["savedDMRManualDialCollection"] = value;
			}
		}

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x06000630 RID: 1584 RVA: 0x00035E71 File Offset: 0x00034071
		// (set) Token: 0x06000631 RID: 1585 RVA: 0x00035E83 File Offset: 0x00034083
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n  <string>PA7LIM</string>\r\n</ArrayOfString>")]
		public StringCollection savedAPRSChatCallsCollection
		{
			get
			{
				return (StringCollection)this["savedAPRSChatCallsCollection"];
			}
			set
			{
				this["savedAPRSChatCallsCollection"] = value;
			}
		}

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x06000632 RID: 1586 RVA: 0x00035E91 File Offset: 0x00034091
		// (set) Token: 0x06000633 RID: 1587 RVA: 0x00035EA3 File Offset: 0x000340A3
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("TGIF")]
		public string savedTGIFreflector
		{
			get
			{
				return (string)this["savedTGIFreflector"];
			}
			set
			{
				this["savedTGIFreflector"] = value;
			}
		}

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x06000634 RID: 1588 RVA: 0x00035EB1 File Offset: 0x000340B1
		// (set) Token: 0x06000635 RID: 1589 RVA: 0x00035EC3 File Offset: 0x000340C3
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("710")]
		public string savedNXDNreflector
		{
			get
			{
				return (string)this["savedNXDNreflector"];
			}
			set
			{
				this["savedNXDNreflector"] = value;
			}
		}

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x06000636 RID: 1590 RVA: 0x00035ED1 File Offset: 0x000340D1
		// (set) Token: 0x06000637 RID: 1591 RVA: 0x00035EE3 File Offset: 0x000340E3
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("0")]
		public int savedNXDNoutGain
		{
			get
			{
				return (int)this["savedNXDNoutGain"];
			}
			set
			{
				this["savedNXDNoutGain"] = value;
			}
		}

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x06000638 RID: 1592 RVA: 0x00035EF6 File Offset: 0x000340F6
		// (set) Token: 0x06000639 RID: 1593 RVA: 0x00035F08 File Offset: 0x00034108
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("-1")]
		public int savedNXDNinGain
		{
			get
			{
				return (int)this["savedNXDNinGain"];
			}
			set
			{
				this["savedNXDNinGain"] = value;
			}
		}

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x0600063A RID: 1594 RVA: 0x00035F1B File Offset: 0x0003411B
		// (set) Token: 0x0600063B RID: 1595 RVA: 0x00035F2D File Offset: 0x0003412D
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("ADN_2141_Spain")]
		public string savedADNSYSTEMSreflector
		{
			get
			{
				return (string)this["savedADNSYSTEMSreflector"];
			}
			set
			{
				this["savedADNSYSTEMSreflector"] = value;
			}
		}

		// Token: 0x0400045B RID: 1115
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
	}
}
