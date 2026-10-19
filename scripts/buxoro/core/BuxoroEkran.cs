using System;
using Godot;

namespace ChronoShift;

/// <summary>
/// Display-space maths for matching the web build's frame (main.js:17-19:
/// sRGB output, ACESFilmic, exposure 0.86).
/// </summary>
/// <remarks>
/// three.js r128 and Godot 4.7 use the same fitted ACES curve (Stephen Hill's
/// RRT+ODT fit, identical matrices and A..E constants). Two things differ:
///   1. the pre-curve bias — three.js multiplies by exposure / 0.6, Godot by
///      exposure · 1.8. <see cref="Ekspozitsiya"/> converts.
///   2. Godot divides by the curve at <c>tonemap_white</c>; three.js does not.
///      A white of 1000 makes that divisor 1.0165, i.e. within 1.7 % of three.js.
/// And two things three.js draws AFTER tone mapping, straight in display space:
/// the clear colour (scene.background) and the fog blend (fog_fragment follows
/// tonemapping_fragment and encodings_fragment in r128). Godot tone-maps both, so
/// their colours are pre-inverted through the curve by <see cref="Teskari"/>.
/// </remarks>
public static class BuxoroEkran
{
    /// <summary>Godot's tonemap_white for a three.js-like (unnormalised) curve.</summary>
    public const float Oq = 1000f;

    private const double A = 0.0245786, B = 0.000090537, C = 0.983729, D = 0.432951, E = 0.238081;
    private const double GodotBias = 1.8;

    /// <summary>The Godot exposure that reproduces a three.js toneMappingExposure.</summary>
    public static float Ekspozitsiya(float threeExposure) => (float)(threeExposure / 0.6 / GodotBias);

    /// <summary>
    /// The colour to hand Godot so that, after its ACES at <paramref name="godotExposure"/>,
    /// the screen shows <paramref name="ekranHex"/> exactly (as three.js shows it).
    /// Returned as an sRGB Color, ready for BackgroundColor / FogLightColor.
    /// </summary>
    public static Color Teskari(Color ekranHex, float godotExposure)
    {
        double[] target = { Lin(ekranHex.R), Lin(ekranHex.G), Lin(ekranHex.B) };
        double[] l = (double[])target.Clone();
        for (int k = 0; k < 60; k++)
        {
            double[] f = Aces(l, godotExposure);
            for (int i = 0; i < 3; i++)
            {
                l[i] = Math.Max(1e-6, l[i] * (target[i] / Math.Max(1e-6, f[i])));
            }
        }

        return new Color((float)l[0], (float)l[1], (float)l[2]).LinearToSrgb();
    }

    /// <summary>Godot 4.7 tonemap_aces with white = <see cref="Oq"/>, linear in, linear out.</summary>
    private static double[] Aces(double[] lin, float exposure)
    {
        double r = lin[0] * exposure * GodotBias, g = lin[1] * exposure * GodotBias, b = lin[2] * exposure * GodotBias;
        double[] v =
        {
            (0.59719 * r) + (0.35458 * g) + (0.04823 * b),
            (0.07600 * r) + (0.90834 * g) + (0.01566 * b),
            (0.02840 * r) + (0.13383 * g) + (0.83777 * b),
        };
        for (int i = 0; i < 3; i++)
        {
            v[i] = ((v[i] * (v[i] + A)) - B) / ((v[i] * ((C * v[i]) + D)) + E);
        }

        double w = Oq * GodotBias;
        double wt = ((w * (w + A)) - B) / ((w * ((C * w) + D)) + E);
        return new[]
        {
            Math.Clamp(((1.60475 * v[0]) - (0.53108 * v[1]) - (0.07367 * v[2])) / wt, 0, 1),
            Math.Clamp(((-0.10208 * v[0]) + (1.10813 * v[1]) - (0.00605 * v[2])) / wt, 0, 1),
            Math.Clamp(((-0.00327 * v[0]) - (0.07276 * v[1]) + (1.07602 * v[2])) / wt, 0, 1),
        };
    }

    private static double Lin(float s) => s <= 0.04045 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
}
