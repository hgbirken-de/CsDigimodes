namespace DigitalVoice.SoftwareVocoder;

/// <summary>
/// Einfache Zufallsquelle für die Rauschanteile der stimmlosen Bereiche (in der mbelib <c>rand()</c>). Der Generator ist
/// deterministisch und schnell; für den Vergleich mit dem C-Original gibt es die gleiche Folge dort ebenfalls.
/// </summary>
public sealed class NoiseSource
{
    private uint _state;

    public NoiseSource(uint seed = 12345u) => _state = seed;

    /// <summary>31-Bit-Zufallszahl (0 .. 2147483647), wie <c>rand()</c> mit RAND_MAX = 2147483647.</summary>
    public int NextInt()
    {
        unchecked
        {
            _state = _state * 1103515245u + 12345u;
            return (int)((_state >> 1) & 0x7fffffffu);
        }
    }

    /// <summary>Zufallszahl zwischen 0.0 und 1.0 (<c>mbe_rand()</c>).</summary>
    internal float Rand() => (float)NextInt() / 2147483648f;   // (float)RAND_MAX ist 2147483648f

    /// <summary>Zufallsphase zwischen -pi und +pi (<c>mbe_rand_phase()</c>).</summary>
    internal float RandPhase() => Rand() * ((float)Math.PI * 2.0f) - (float)Math.PI;
}
