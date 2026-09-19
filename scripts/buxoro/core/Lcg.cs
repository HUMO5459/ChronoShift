namespace ChronoShift;

/// <summary>
/// The deterministic generator the Buxoro world is built from. A literal port of
/// world.js:10-12.
/// </summary>
/// <remarks>
/// <code>
/// let urugh = 1238;
/// function rnd() { urugh = (urugh * 1664525 + 1013904223) % 4294967296;
///                  return urugh / 4294967296; }
/// </code>
///
/// WHY ulong AND NOT uint. The JavaScript original keeps the seed in a double and
/// takes the modulus explicitly. The intermediate product peaks at
/// 4294967295 x 1664525 which is about 7.15e15, comfortably under 2^53, so every
/// step is exact in a double and a 64-bit integer reproduces it bit for bit. A
/// uint implementation would instead wrap on the multiply, silently landing in a
/// completely different sequence that still looks random. The whole world is
/// downstream of this, so that failure would be invisible and total.
///
/// Three independent streams exist in the web build and MUST NOT be mixed:
/// the world (seed 1238, world.js:10), the procedural buildings (seed 8317,
/// binolar.js:16) and the grass (seed 4711, otlar.js:13).
/// </remarks>
public sealed class Lcg
{
    /// <summary>World stream. Builds the 66 resource nodes, the poplars and the gate.</summary>
    public const ulong WorldSeed = 1238;

    /// <summary>Procedural building stream (binolar.js:16).</summary>
    public const ulong BuildingSeed = 8317;

    /// <summary>Grass and scatter stream (otlar.js:13).</summary>
    public const ulong GrassSeed = 4711;

    private const ulong Multiplier = 1664525;
    private const ulong Increment = 1013904223;
    private const ulong Modulus = 4294967296; // 2^32

    private ulong _state;

    public Lcg(ulong seed)
    {
        _state = seed;
        Seed = seed;
    }

    /// <summary>The seed this generator started from, for diagnostics.</summary>
    public ulong Seed { get; }

    /// <summary>How many values have been drawn. The parity harness compares this.</summary>
    public int DrawCount { get; private set; }

    /// <summary>Next value in [0, 1).</summary>
    public double Next()
    {
        _state = ((_state * Multiplier) + Increment) % Modulus;
        DrawCount++;
        return (double)_state / Modulus;
    }

    /// <summary>Next value in [a, b). Port of world.js:12 <c>orasida</c>.</summary>
    public double Between(double a, double b)
    {
        return a + (Next() * (b - a));
    }

    /// <summary>Restarts the stream. Only the parity harness should need this.</summary>
    public void Reset()
    {
        _state = Seed;
        DrawCount = 0;
    }
}
