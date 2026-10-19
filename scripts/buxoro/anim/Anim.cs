using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ChronoShift;

/// <summary>One actor's animation controller — the object anim.js:23-32 returns.</summary>
public sealed class AnimCtrl
{
    public AnimationPlayer Ap = null!;
    public string? Joriy;

    /// <summary>A one-shot clip is not interrupted until this runs out (anim.js:26).</summary>
    public double Qulf;

    /// <summary>
    /// Phase offset: six enemies spawned together must not step in lock-step
    /// (anim.js:27-30). Math.random in the source, so GD.Randf here.
    /// </summary>
    public double Siljish = GD.Randf();
    public bool Birinchi = true;
    public double TezlikKoef = 1;
}

/// <summary>Options for <see cref="Anim.Oynat"/> — anim.js's <c>opt</c>.</summary>
public struct AnimOpt
{
    public bool BirMarta;
    public bool Majburiy;
    public bool Qayta;
    public double? Fade;
    public double Tezlik;
}

/// <summary>Procedural-walk state an actor keeps between frames (anim.js:180-183).</summary>
public sealed class ProtseduralHolat
{
    public double Faza;
    public double Aralash;
}

/// <summary>
/// The animation controller, anim.js. GLB clips play through the model's own
/// AnimationPlayer; without a clip the caller's procedural motion takes over and
/// the call returns false, exactly as in the web build.
/// </summary>
/// <remarks>
/// Players are switched to manual processing and advanced by <see cref="Yangila"/>
/// with the game's clamped dt, mirroring main.js:278 <c>ANIM.yangila(dt)</c>.
/// </remarks>
public static class Anim
{
    private static readonly List<AnimCtrl> Mikserlar = new();

    /// <summary>anim.js:96 — beyond these the feet turn into wheels or into treacle.</summary>
    private const double TezlikMin = 0.55, TezlikMax = 2.4;

    /// <summary>anim.js:10-33. Null when the model has no clips.</summary>
    public static AnimCtrl? Ulash(Node? mesh, string nom)
    {
        if (mesh == null)
        {
            return null;
        }

        var ap = mesh.FindChildren("*", nameof(AnimationPlayer), true, false).OfType<AnimationPlayer>().FirstOrDefault();
        if (ap == null || ap.GetAnimationList().Length == 0)
        {
            return null;
        }

        ap.CallbackModeProcess = AnimationMixer.AnimationCallbackModeProcess.Manual;
        ap.Autoplay = "";
        var ctrl = new AnimCtrl { Ap = ap };
        Mikserlar.Add(ctrl);
        return ctrl;
    }

    /// <summary>anim.js:37-74. False when the clip is missing — the caller falls back.</summary>
    public static bool Oynat(AnimCtrl? ctrl, string nom, AnimOpt opt = default)
    {
        if (ctrl == null || !ctrl.Ap.HasAnimation(nom))
        {
            return false;
        }

        if (ctrl.Joriy == nom && !opt.Qayta)
        {
            return true;
        }

        if (ctrl.Qulf > 0 && !opt.Majburiy)
        {
            return false;
        }

        double fade = opt.Fade ?? 0.18;
        double tezlik = opt.Tezlik > 0 ? opt.Tezlik : 1;
        Animation a = ctrl.Ap.GetAnimation(nom);

        if (!opt.BirMarta && ctrl.Birinchi)
        {
            ctrl.Birinchi = false;
            ctrl.TezlikKoef = 0.93 + (ctrl.Siljish * 0.14); // steps must not coincide
        }

        double koef = opt.BirMarta ? tezlik : tezlik * ctrl.TezlikKoef;
        ctrl.Ap.Play(nom, ctrl.Joriy == null ? 0 : fade, 1.0f);
        ctrl.Ap.SpeedScale = (float)koef;

        // a looping clip starts from a random point (anim.js:55-56)
        if (!opt.BirMarta)
        {
            ctrl.Ap.Seek(ctrl.Siljish * a.Length, true);
        }

        ctrl.Joriy = nom;
        if (opt.BirMarta)
        {
            ctrl.Qulf = a.Length / tezlik;
        }

        return true;
    }

    /// <summary>anim.js:77-79.</summary>
    public static bool Bormi(AnimCtrl? ctrl, string nom) => ctrl != null && ctrl.Ap.HasAnimation(nom);

    /// <summary>
    /// The clip's natural speed in m/s, measured at import from the root motion it
    /// removed (BuxoroImport, models.js:121-128). 0 when the clip has no stride.
    /// </summary>
    public static double TabiiyTezlik(AnimCtrl? ctrl, string nom)
    {
        if (ctrl == null || !ctrl.Ap.HasAnimation(nom))
        {
            return 0;
        }

        Animation a = ctrl.Ap.GetAnimation(nom);
        return a.HasMeta("buxoro_tabiiy_tezlik") ? a.GetMeta("buxoro_tabiiy_tezlik").AsDouble() : 0;
    }

    /// <summary>anim.js:98-106 — stride frequency follows the real speed, clamped.</summary>
    public static void MoslaTezlikka(AnimCtrl? ctrl, string nom, double tezlik)
    {
        if (ctrl == null || !ctrl.Ap.HasAnimation(nom))
        {
            return;
        }

        double tabiiy = TabiiyTezlik(ctrl, nom);
        double koef = tabiiy > 0 ? tezlik / tabiiy : 1;
        ctrl.Ap.SpeedScale = (float)(Math.Max(TezlikMin, Math.Min(TezlikMax, koef)) * ctrl.TezlikKoef);
    }

    /// <summary>anim.js:110-116 — the older signature some callers still use.</summary>
    public static void TezlikMosla(AnimCtrl? ctrl, string nom, double asosiy, double joriy)
    {
        if (ctrl == null || !ctrl.Ap.HasAnimation(nom))
        {
            return;
        }

        if (TabiiyTezlik(ctrl, nom) > 0)
        {
            MoslaTezlikka(ctrl, nom, joriy);
            return;
        }

        if (asosiy <= 0)
        {
            return;
        }

        ctrl.Ap.SpeedScale = (float)(Math.Max(0.35, Math.Min(2.2, joriy / asosiy)) * ctrl.TezlikKoef);
    }

    /// <summary>
    /// anim.js:127-145. Ordered (default): the FIRST available clip — Shift shows
    /// run, no Shift shows walk. Unordered: the clip whose natural speed is nearest
    /// in log distance (enemies at 4.5 m/s want 'charge', not 'walk_menace').
    /// </summary>
    public static bool Yurish(AnimCtrl? ctrl, IReadOnlyList<string> nomlar, double tezlik, bool tartib = true)
    {
        if (ctrl == null)
        {
            return false;
        }

        string? tanlangan = null;
        double engYaqin = double.PositiveInfinity;
        for (int i = 0; i < nomlar.Count; i++)
        {
            string nom = nomlar[i];
            if (!ctrl.Ap.HasAnimation(nom))
            {
                continue;
            }

            if (tartib)
            {
                tanlangan = nom;
                break;
            }

            double tabiiy = TabiiyTezlik(ctrl, nom);
            double farq = (tabiiy > 0 && tezlik > 0) ? Math.Abs(Math.Log(tezlik / tabiiy)) : 1e3 + i;
            if (farq < engYaqin)
            {
                engYaqin = farq;
                tanlangan = nom;
            }
        }

        if (tanlangan == null || !Oynat(ctrl, tanlangan))
        {
            return false;
        }

        MoslaTezlikka(ctrl, tanlangan, tezlik);
        return true;
    }

    /// <summary>main.js:278 — every mixer, the game's clamped dt.</summary>
    public static void Yangila(double dt)
    {
        for (int i = Mikserlar.Count - 1; i >= 0; i--)
        {
            AnimationPlayer ap = Mikserlar[i].Ap;
            if (!GodotObject.IsInstanceValid(ap))
            {
                Mikserlar.RemoveAt(i);
                continue;
            }

            ap.Advance(dt);
        }
    }

    /// <summary>anim.js:151-153.</summary>
    public static void QulfYangila(AnimCtrl? ctrl, double dt)
    {
        if (ctrl != null && ctrl.Qulf > 0)
        {
            ctrl.Qulf -= dt;
        }
    }

    /// <summary>anim.js:157-162 — so a missing 'die' clip does not leave idle playing on a corpse.</summary>
    public static void Toxtat(AnimCtrl? ctrl)
    {
        if (ctrl == null)
        {
            return;
        }

        ctrl.Ap.Stop();
        ctrl.Joriy = null;
        ctrl.Qulf = 0;
    }

    private static readonly Dictionary<string, (double Chastota, double Balandlik, double Engashish, double Chayqalish)> Uslub = new()
    {
        ["shogird"] = (9.5, 0.055, 0.13, 0.05),
        ["yurish"] = (6.5, 0.040, 0.06, 0.04),
        ["ogir"] = (4.2, 0.065, 0.04, 0.08),
    };

    /// <summary>
    /// anim.js:180-197 — a clipless model would just glide; this gives it a step
    /// bob, a forward lean and a sway, the minimum that reads as walking.
    /// </summary>
    public static void ProtseduralYurish(ProtseduralHolat holat, Node3D mesh, bool harakatda, double dt, string uslub, double yerY)
    {
        var u = Uslub.TryGetValue(uslub, out var v) ? v : Uslub["yurish"];
        if (holat.Faza == 0)
        {
            holat.Faza = GD.Randf() * 10;
        }

        double maqsad = harakatda ? 1 : 0;
        holat.Aralash += (maqsad - holat.Aralash) * Math.Min(1, dt * 7);
        if (harakatda)
        {
            holat.Faza += dt * u.Chastota;
        }

        double a = holat.Aralash;
        double kotarilish = Math.Abs(Math.Sin(holat.Faza)) * u.Balandlik * a;
        mesh.Position = new Vector3(mesh.Position.X, (float)(yerY + kotarilish), mesh.Position.Z);
        Actors.JsRotation(mesh, -u.Engashish * a, mesh.Rotation.Y, Math.Sin(holat.Faza * 0.5) * u.Chayqalish * a);
    }

    /// <summary>anim.js:199 — a new episode starts with no mixers.</summary>
    public static void Tozala() => Mikserlar.Clear();
}
