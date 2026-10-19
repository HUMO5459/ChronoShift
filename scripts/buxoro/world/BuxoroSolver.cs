using System;
using System.Collections.Generic;

namespace ChronoShift;

/// <summary>Footprint kind of one ground obstacle. world.js:564-570.</summary>
public enum TosiqShakli
{
    /// <summary>Circle <c>{ x, z, r }</c> — poplar trunks, workshop posts.</summary>
    Aylana,

    /// <summary>Rotated rectangle <c>{ x, z, w, d, burilish }</c> — houses, stalls, the gate.</summary>
    Tortburchak,

    /// <summary>Annulus <c>{ x, z, r1, r2 }</c> — the city wall ring, interior solid.</summary>
    Halqa,
}

/// <summary>One ground footprint. Height is never considered (world.js:566).</summary>
public sealed class Tosiq
{
    public TosiqShakli Shakl;
    public double X;
    public double Z;

    /// <summary>Circle radius.</summary>
    public double R;

    /// <summary>Annulus inner and outer radius.</summary>
    public double R1;
    public double R2;

    /// <summary>Rectangle size along its local X and Z, and its yaw in radians.</summary>
    public double W;
    public double D;
    public double Burilish;

    public static Tosiq Aylana(double x, double z, double r) =>
        new() { Shakl = TosiqShakli.Aylana, X = x, Z = z, R = r };

    public static Tosiq Tortburchak(double x, double z, double w, double d, double burilish) =>
        new() { Shakl = TosiqShakli.Tortburchak, X = x, Z = z, W = w, D = d, Burilish = burilish };

    public static Tosiq Halqa(double x, double z, double r1, double r2) =>
        new() { Shakl = TosiqShakli.Halqa, X = x, Z = z, R1 = r1, R2 = r2 };
}

/// <summary>
/// The obstacle solver. A literal port of <c>toqnashuvHal</c>, world.js:612-665.
/// </summary>
/// <remarks>
/// Deliberately free of Godot types so the parity sweep can run it outside the
/// engine and diff it against the browser to 1e-4.
///
/// REFUSED: move_and_slide world collision and Godot depenetration. Neither
/// reproduces the three behaviours below, and both would give apprentices and
/// enemies a collision they have never had (toqnashuvHal has exactly one caller,
/// player.js:298).
///   1. Three passes with the <c>surildi</c> early-out, so two obstacles meeting
///      at a corner do not cancel each other.
///   2. The annulus INTERIOR is solid: anything inside r2 + r is ejected OUTWARD
///      to r2 + r. world.js:621-625 records the gate oscillation this fixes.
///   3. Degenerate centres escape along a fixed axis (-X for the ring, +X for a
///      circle), and a point inside a rectangle leaves through the nearest edge.
/// Doubles throughout, matching JS numbers.
/// </remarks>
public static class BuxoroSolver
{
    /// <summary>Pushes the circle (<paramref name="x"/>, <paramref name="z"/>, <paramref name="r"/>) out of every obstacle.</summary>
    public static void ToqnashuvHal(IReadOnlyList<Tosiq> T, ref double x, ref double z, double r)
    {
        if (T == null || T.Count == 0)
        {
            return;
        }

        for (int pass = 0; pass < 3; pass++)
        {
            bool surildi = false;
            for (int i = 0; i < T.Count; i++)
            {
                Tosiq t = T[i];
                double dx = x - t.X, dz = z - t.Z;

                if (t.Shakl == TosiqShakli.Halqa)
                {
                    // City wall ring. The interior (the city) is not playable: the belt
                    // AND everything inside it is solid, always ejected to the outer edge.
                    // Otherwise the large gate footprint pushed the player inward and the
                    // belt then let them go as "inside the city" — an oscillation.
                    double m = Hypot(dx, dz);
                    double tashqi = t.R2 + r;
                    if (m >= tashqi)
                    {
                        continue;
                    }

                    if (m < 1e-4)
                    {
                        x = t.X - tashqi; // dead centre — west
                    }
                    else
                    {
                        x = t.X + (dx / m * tashqi);
                        z = t.Z + (dz / m * tashqi);
                    }

                    surildi = true;
                    continue;
                }

                if (t.Shakl == TosiqShakli.Aylana)
                {
                    double R = t.R + r, m2 = (dx * dx) + (dz * dz);
                    if (m2 >= R * R)
                    {
                        continue;
                    }

                    double m = Math.Sqrt(m2);
                    if (m < 1e-4)
                    {
                        x = t.X + R; // dead centre — no direction
                    }
                    else
                    {
                        x = t.X + (dx / m * R);
                        z = t.Z + (dz / m * R);
                    }

                    surildi = true;
                    continue;
                }

                // Rotated rectangle — move the point into its local axes.
                double cos = Math.Cos(t.Burilish), sin = Math.Sin(t.Burilish);
                double lx = (cos * dx) - (sin * dz), lz = (sin * dx) + (cos * dz);
                double hw = t.W / 2, hd = t.D / 2;
                double cx = Math.Max(-hw, Math.Min(hw, lx)), cz = Math.Max(-hd, Math.Min(hd, lz));
                double ox, oz;
                if (cx == lx && cz == lz)
                {
                    // Inside: leave through the nearest edge.
                    if (hw - Math.Abs(lx) < hd - Math.Abs(lz))
                    {
                        ox = lx >= 0 ? hw + r : -hw - r;
                        oz = lz;
                    }
                    else
                    {
                        ox = lx;
                        oz = lz >= 0 ? hd + r : -hd - r;
                    }
                }
                else
                {
                    double ex = lx - cx, ez = lz - cz, m2 = (ex * ex) + (ez * ez);
                    if (m2 >= r * r)
                    {
                        continue;
                    }

                    double m = Math.Sqrt(m2);
                    if (m == 0)
                    {
                        m = 1e-6; // JS: Math.sqrt(m2) || 1e-6
                    }

                    ox = cx + (ex / m * r);
                    oz = cz + (ez / m * r);
                }

                x = t.X + (cos * ox) + (sin * oz);
                z = t.Z - (sin * ox) + (cos * oz);
                surildi = true;
            }

            if (!surildi)
            {
                break;
            }
        }
    }

    /// <summary>
    /// JS <c>Math.hypot</c>. Plain sqrt(dx²+dz²) is used rather than a scaled hypot:
    /// for metre-scale field coordinates the two agree far inside the 1e-4 gate.
    /// </summary>
    private static double Hypot(double a, double b) => Math.Sqrt((a * a) + (b * b));
}
