using System;

// Token: 0x02000006 RID: 6
internal static class Arrays
{
	// Token: 0x06000010 RID: 16 RVA: 0x00002344 File Offset: 0x00000544
	public static T[] InitializeWithDefaultInstances<T>(int length) where T : new()
	{
		T[] array = new T[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = new T();
		}
		return array;
	}

	// Token: 0x06000011 RID: 17 RVA: 0x00002374 File Offset: 0x00000574
	public static void DeleteArray<T>(T[] array) where T : IDisposable
	{
		foreach (T t in array)
		{
			if (t != null)
			{
				t.Dispose();
			}
		}
	}
}
