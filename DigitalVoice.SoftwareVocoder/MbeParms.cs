namespace DigitalVoice.SoftwareVocoder;

/// <summary>
/// Parameter eines MBE-Frames (Grundfrequenz, Anzahl der Harmonischen, Spektralbeträge, Stimmhaft/Stimmlos-Flags ...).
/// Entspricht <c>mbe_parms</c> der mbelib. Die Felder sind absichtlich öffentlich (internal) und wie im C-Original benannt.
/// </summary>
internal sealed class MbeParms
{
    public float W0;
    public int L;
    public int K;
    public readonly int[] Vl = new int[57];
    public readonly float[] Ml = new float[57];
    public readonly float[] Log2Ml = new float[57];
    public readonly float[] PHIl = new float[57];
    public readonly float[] PSIl = new float[57];
    public float Gamma;
    public int Repeat;
}
