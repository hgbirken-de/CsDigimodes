using System;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;

namespace BlueDV
{
	// Token: 0x0200002D RID: 45
	internal class masterVolume
	{
		// Token: 0x0600036D RID: 877
		[DllImport("winmm.dll")]
		public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

		// Token: 0x0600036E RID: 878
		[DllImport("winmm.dll")]
		public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

		// Token: 0x0600036F RID: 879 RVA: 0x000266B0 File Offset: 0x000248B0
		public masterVolume()
		{
			this.de = new MMDeviceEnumerator();
			this.device = this.de.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
		}

		// Token: 0x06000370 RID: 880 RVA: 0x000266D6 File Offset: 0x000248D6
		public int maxVolume()
		{
			return (int)this.device.AudioEndpointVolume.VolumeRange.MaxDecibels;
		}

		// Token: 0x06000371 RID: 881 RVA: 0x000266EE File Offset: 0x000248EE
		public int minVolume()
		{
			return (int)this.device.AudioEndpointVolume.VolumeRange.MinDecibels;
		}

		// Token: 0x06000372 RID: 882 RVA: 0x00026708 File Offset: 0x00024908
		public int GetVolume()
		{
			uint num = 0U;
			masterVolume.waveOutGetVolume(IntPtr.Zero, out num);
			return (int)this.device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
		}

		// Token: 0x06000373 RID: 883 RVA: 0x00026738 File Offset: 0x00024938
		public void SetVolume(int volumeLevel)
		{
			this.device.AudioEndpointVolume.MasterVolumeLevel = (float)volumeLevel;
		}

		// Token: 0x04000251 RID: 593
		private MMDeviceEnumerator de;

		// Token: 0x04000252 RID: 594
		private MMDevice device;
	}
}
