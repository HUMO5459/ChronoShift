using Godot;

namespace ChronoShift;

/// <summary>
/// Ground height for the Buxoro field. Port of world.js:25-27.
/// </summary>
/// <remarks>
/// <code>
/// function yerBalandligi(x, z) {
///   return Math.sin(x * 0.09) * 0.16 + Math.cos(z * 0.11) * 0.14;
/// }
/// </code>
///
/// A PURE FUNCTION, and deliberately so. An earlier web build displaced the ground
/// mesh with random offsets that could not be recomputed, so actors stood at y = 0
/// and sank into every rise. Everything that needs a height now asks for it.
///
/// The ground MESH must be displaced with this identical expression, in world
/// coordinates. The web build had a real bug here, fixed 2026-09-10: the plane is
/// centred at x = 120, and the displacement loop fed it LOCAL x, which shifted the
/// phase by 10.8 radians and left the visible surface up to 0.30 m away from what
/// this function reported. Feed world x and z, not vertex-local ones.
/// </remarks>
public static class BuxoroTerrain
{
    private const double XFrequency = 0.09;
    private const double XAmplitude = 0.16;
    private const double ZFrequency = 0.11;
    private const double ZAmplitude = 0.14;

    /// <summary>Ground height in metres at a world position.</summary>
    public static double HeightAt(double x, double z)
    {
        return (Mathf.Sin(x * XFrequency) * XAmplitude) + (Mathf.Cos(z * ZFrequency) * ZAmplitude);
    }

    /// <summary>Convenience overload for float call sites.</summary>
    public static float HeightAt(float x, float z)
    {
        return (float)HeightAt((double)x, z);
    }

    /// <summary>The position with its Y replaced by the ground height beneath it.</summary>
    public static Vector3 OnGround(Vector3 position)
    {
        return new Vector3(position.X, HeightAt(position.X, position.Z), position.Z);
    }
}
