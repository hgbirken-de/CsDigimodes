using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace STA.Settings
{
	// Token: 0x02000007 RID: 7
	internal class INIFile
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000012 RID: 18 RVA: 0x000023AE File Offset: 0x000005AE
		internal string FileName
		{
			get
			{
				return this.m_FileName;
			}
		}

		// Token: 0x06000013 RID: 19 RVA: 0x000023B6 File Offset: 0x000005B6
		public INIFile(string FileName)
		{
			this.Initialize(FileName, false, false);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000023E8 File Offset: 0x000005E8
		public INIFile(string FileName, bool Lazy, bool AutoFlush)
		{
			this.Initialize(FileName, Lazy, AutoFlush);
		}

		// Token: 0x06000015 RID: 21 RVA: 0x0000241A File Offset: 0x0000061A
		private void Initialize(string FileName, bool Lazy, bool AutoFlush)
		{
			this.m_FileName = FileName;
			this.m_Lazy = Lazy;
			this.m_AutoFlush = AutoFlush;
			if (!this.m_Lazy)
			{
				this.Refresh();
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x0000243F File Offset: 0x0000063F
		private string ParseSectionName(string Line)
		{
			if (!Line.StartsWith("["))
			{
				return null;
			}
			if (!Line.EndsWith("]"))
			{
				return null;
			}
			if (Line.Length < 3)
			{
				return null;
			}
			return Line.Substring(1, Line.Length - 2);
		}

		// Token: 0x06000017 RID: 23 RVA: 0x0000247C File Offset: 0x0000067C
		private bool ParseKeyValuePair(string Line, ref string Key, ref string Value)
		{
			int num;
			if ((num = Line.IndexOf('=')) <= 0)
			{
				return false;
			}
			int num2 = Line.Length - num - 1;
			Key = Line.Substring(0, num).Trim();
			if (Key.Length <= 0)
			{
				return false;
			}
			Value = ((num2 > 0) ? Line.Substring(num + 1, num2).Trim() : "");
			return true;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000024DC File Offset: 0x000006DC
		internal void Refresh()
		{
			object @lock = this.m_Lock;
			lock (@lock)
			{
				StreamReader streamReader = null;
				try
				{
					this.m_Sections.Clear();
					this.m_Modified.Clear();
					try
					{
						streamReader = new StreamReader(this.m_FileName);
					}
					catch (FileNotFoundException)
					{
						return;
					}
					Dictionary<string, string> dictionary = null;
					string text = null;
					string text2 = null;
					string text3;
					while ((text3 = streamReader.ReadLine()) != null)
					{
						text3 = text3.Trim();
						string text4 = this.ParseSectionName(text3);
						if (text4 != null)
						{
							if (this.m_Sections.ContainsKey(text4))
							{
								dictionary = null;
							}
							else
							{
								dictionary = new Dictionary<string, string>();
								this.m_Sections.Add(text4, dictionary);
							}
						}
						else if (dictionary != null && this.ParseKeyValuePair(text3, ref text, ref text2) && !dictionary.ContainsKey(text))
						{
							dictionary.Add(text, text2);
						}
					}
				}
				finally
				{
					if (streamReader != null)
					{
						streamReader.Close();
					}
					streamReader = null;
				}
			}
		}

		// Token: 0x06000019 RID: 25 RVA: 0x000025E0 File Offset: 0x000007E0
		internal void Flush()
		{
			object @lock = this.m_Lock;
			lock (@lock)
			{
				this.PerformFlush();
			}
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002620 File Offset: 0x00000820
		private void PerformFlush()
		{
			if (!this.m_CacheModified)
			{
				return;
			}
			this.m_CacheModified = false;
			bool flag = File.Exists(this.m_FileName);
			string text = Path.ChangeExtension(this.m_FileName, "$n$");
			StreamWriter streamWriter = null;
			streamWriter = new StreamWriter(text);
			try
			{
				Dictionary<string, string> dictionary = null;
				if (flag)
				{
					StreamReader streamReader = null;
					try
					{
						streamReader = new StreamReader(this.m_FileName);
						string text2 = null;
						string text3 = null;
						bool flag2 = true;
						while (flag2)
						{
							string text4 = streamReader.ReadLine();
							flag2 = text4 != null;
							bool flag3;
							string text5;
							if (flag2)
							{
								flag3 = true;
								text4 = text4.Trim();
								text5 = this.ParseSectionName(text4);
							}
							else
							{
								flag3 = false;
								text5 = null;
							}
							if (text5 != null || !flag2)
							{
								if (dictionary != null && dictionary.Count > 0)
								{
									foreach (string text6 in dictionary.Keys)
									{
										if (dictionary.TryGetValue(text6, out text3))
										{
											streamWriter.Write(text6);
											streamWriter.Write('=');
											streamWriter.WriteLine(text3);
										}
									}
									streamWriter.WriteLine();
									dictionary.Clear();
								}
								if (flag2 && !this.m_Modified.TryGetValue(text5, out dictionary))
								{
									dictionary = null;
								}
							}
							else if (dictionary != null && this.ParseKeyValuePair(text4, ref text2, ref text3) && dictionary.TryGetValue(text2, out text3))
							{
								flag3 = false;
								dictionary.Remove(text2);
								streamWriter.Write(text2);
								streamWriter.Write('=');
								streamWriter.WriteLine(text3);
							}
							if (flag3)
							{
								streamWriter.WriteLine(text4);
							}
						}
						streamReader.Close();
						streamReader = null;
					}
					finally
					{
						if (streamReader != null)
						{
							streamReader.Close();
						}
						streamReader = null;
					}
				}
				foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePair in this.m_Modified)
				{
					dictionary = keyValuePair.Value;
					if (dictionary.Count > 0)
					{
						streamWriter.WriteLine();
						streamWriter.Write('[');
						streamWriter.Write(keyValuePair.Key);
						streamWriter.WriteLine(']');
						foreach (KeyValuePair<string, string> keyValuePair2 in dictionary)
						{
							streamWriter.Write(keyValuePair2.Key);
							streamWriter.Write('=');
							streamWriter.WriteLine(keyValuePair2.Value);
						}
						dictionary.Clear();
					}
				}
				this.m_Modified.Clear();
				streamWriter.Close();
				streamWriter = null;
				File.Copy(text, this.m_FileName, true);
				File.Delete(text);
			}
			finally
			{
				if (streamWriter != null)
				{
					streamWriter.Close();
				}
				streamWriter = null;
			}
		}

		// Token: 0x0600001B RID: 27 RVA: 0x0000292C File Offset: 0x00000B2C
		internal string GetValue(string SectionName, string Key, string DefaultValue)
		{
			if (this.m_Lazy)
			{
				this.m_Lazy = false;
				this.Refresh();
			}
			object @lock = this.m_Lock;
			string text;
			lock (@lock)
			{
				Dictionary<string, string> dictionary;
				string text2;
				if (!this.m_Sections.TryGetValue(SectionName, out dictionary))
				{
					text = DefaultValue;
				}
				else if (!dictionary.TryGetValue(Key, out text2))
				{
					text = DefaultValue;
				}
				else
				{
					text = text2;
				}
			}
			return text;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x000029A8 File Offset: 0x00000BA8
		internal void SetValue(string SectionName, string Key, string Value)
		{
			if (this.m_Lazy)
			{
				this.m_Lazy = false;
				this.Refresh();
			}
			object @lock = this.m_Lock;
			lock (@lock)
			{
				this.m_CacheModified = true;
				Dictionary<string, string> dictionary;
				if (!this.m_Sections.TryGetValue(SectionName, out dictionary))
				{
					dictionary = new Dictionary<string, string>();
					this.m_Sections.Add(SectionName, dictionary);
				}
				if (dictionary.ContainsKey(Key))
				{
					dictionary.Remove(Key);
				}
				dictionary.Add(Key, Value);
				if (!this.m_Modified.TryGetValue(SectionName, out dictionary))
				{
					dictionary = new Dictionary<string, string>();
					this.m_Modified.Add(SectionName, dictionary);
				}
				if (dictionary.ContainsKey(Key))
				{
					dictionary.Remove(Key);
				}
				dictionary.Add(Key, Value);
				if (this.m_AutoFlush)
				{
					this.PerformFlush();
				}
			}
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002A84 File Offset: 0x00000C84
		private string EncodeByteArray(byte[] Value)
		{
			if (Value == null)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < Value.Length; i++)
			{
				string text = Convert.ToString(Value[i], 16);
				int length = text.Length;
				if (length > 2)
				{
					stringBuilder.Append(text.Substring(length - 2, 2));
				}
				else
				{
					if (length < 2)
					{
						stringBuilder.Append("0");
					}
					stringBuilder.Append(text);
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002AF8 File Offset: 0x00000CF8
		private byte[] DecodeByteArray(string Value)
		{
			if (Value == null)
			{
				return null;
			}
			int num = Value.Length;
			if (num < 2)
			{
				return new byte[0];
			}
			num /= 2;
			byte[] array = new byte[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = Convert.ToByte(Value.Substring(i * 2, 2), 16);
			}
			return array;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002B48 File Offset: 0x00000D48
		internal bool GetValue(string SectionName, string Key, bool DefaultValue)
		{
			int num;
			if (int.TryParse(this.GetValue(SectionName, Key, DefaultValue.ToString(CultureInfo.InvariantCulture)), out num))
			{
				return num != 0;
			}
			return DefaultValue;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002B78 File Offset: 0x00000D78
		internal int GetValue(string SectionName, string Key, int DefaultValue)
		{
			int num;
			if (int.TryParse(this.GetValue(SectionName, Key, DefaultValue.ToString(CultureInfo.InvariantCulture)), NumberStyles.Any, CultureInfo.InvariantCulture, out num))
			{
				return num;
			}
			return DefaultValue;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002BB0 File Offset: 0x00000DB0
		internal long GetValue(string SectionName, string Key, long DefaultValue)
		{
			long num;
			if (long.TryParse(this.GetValue(SectionName, Key, DefaultValue.ToString(CultureInfo.InvariantCulture)), NumberStyles.Any, CultureInfo.InvariantCulture, out num))
			{
				return num;
			}
			return DefaultValue;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002BE8 File Offset: 0x00000DE8
		internal double GetValue(string SectionName, string Key, double DefaultValue)
		{
			double num;
			if (double.TryParse(this.GetValue(SectionName, Key, DefaultValue.ToString(CultureInfo.InvariantCulture)), NumberStyles.Any, CultureInfo.InvariantCulture, out num))
			{
				return num;
			}
			return DefaultValue;
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002C20 File Offset: 0x00000E20
		internal byte[] GetValue(string SectionName, string Key, byte[] DefaultValue)
		{
			string value = this.GetValue(SectionName, Key, this.EncodeByteArray(DefaultValue));
			byte[] array;
			try
			{
				array = this.DecodeByteArray(value);
			}
			catch (FormatException)
			{
				array = DefaultValue;
			}
			return array;
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002C60 File Offset: 0x00000E60
		internal DateTime GetValue(string SectionName, string Key, DateTime DefaultValue)
		{
			DateTime dateTime;
			if (DateTime.TryParse(this.GetValue(SectionName, Key, DefaultValue.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture, DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.AllowInnerWhite | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AssumeLocal, out dateTime))
			{
				return dateTime;
			}
			return DefaultValue;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002C94 File Offset: 0x00000E94
		internal void SetValue(string SectionName, string Key, bool Value)
		{
			this.SetValue(SectionName, Key, Value ? "1" : "0");
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00002CAD File Offset: 0x00000EAD
		internal void SetValue(string SectionName, string Key, int Value)
		{
			this.SetValue(SectionName, Key, Value.ToString(CultureInfo.InvariantCulture));
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002CC3 File Offset: 0x00000EC3
		internal void SetValue(string SectionName, string Key, long Value)
		{
			this.SetValue(SectionName, Key, Value.ToString(CultureInfo.InvariantCulture));
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002CD9 File Offset: 0x00000ED9
		internal void SetValue(string SectionName, string Key, double Value)
		{
			this.SetValue(SectionName, Key, Value.ToString(CultureInfo.InvariantCulture));
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002CEF File Offset: 0x00000EEF
		internal void SetValue(string SectionName, string Key, byte[] Value)
		{
			this.SetValue(SectionName, Key, this.EncodeByteArray(Value));
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002D00 File Offset: 0x00000F00
		internal void SetValue(string SectionName, string Key, DateTime Value)
		{
			this.SetValue(SectionName, Key, Value.ToString(CultureInfo.InvariantCulture));
		}

		// Token: 0x04000009 RID: 9
		private object m_Lock = new object();

		// Token: 0x0400000A RID: 10
		private string m_FileName;

		// Token: 0x0400000B RID: 11
		private bool m_Lazy;

		// Token: 0x0400000C RID: 12
		private bool m_AutoFlush;

		// Token: 0x0400000D RID: 13
		private Dictionary<string, Dictionary<string, string>> m_Sections = new Dictionary<string, Dictionary<string, string>>();

		// Token: 0x0400000E RID: 14
		private Dictionary<string, Dictionary<string, string>> m_Modified = new Dictionary<string, Dictionary<string, string>>();

		// Token: 0x0400000F RID: 15
		private bool m_CacheModified;
	}
}
