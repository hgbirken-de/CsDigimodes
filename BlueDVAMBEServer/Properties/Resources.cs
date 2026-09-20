using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace BlueDVAMBEServer.Properties
{
	// Token: 0x02000008 RID: 8
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x06000035 RID: 53 RVA: 0x00002F13 File Offset: 0x00001113
		internal Resources()
		{
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000036 RID: 54 RVA: 0x00003840 File Offset: 0x00001A40
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new ResourceManager("BlueDVAMBEServer.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000037 RID: 55 RVA: 0x0000386C File Offset: 0x00001A6C
		// (set) Token: 0x06000038 RID: 56 RVA: 0x00003873 File Offset: 0x00001A73
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000039 RID: 57 RVA: 0x0000387B File Offset: 0x00001A7B
		internal static Icon web_hi_res_512
		{
			get
			{
				return (Icon)Resources.ResourceManager.GetObject("web_hi_res_512", Resources.resourceCulture);
			}
		}

		// Token: 0x04000027 RID: 39
		private static ResourceManager resourceMan;

		// Token: 0x04000028 RID: 40
		private static CultureInfo resourceCulture;
	}
}
