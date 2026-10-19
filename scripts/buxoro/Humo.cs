using System;
using Godot;

namespace ChronoShift;

/// <summary>humo.js:5 — perched · wings spread (15 s warning) · flown · gone.</summary>
public enum HumoHolat
{
    Qongan,
    Ogoh,
    Uchdi,
    Ketdi,
}

/// <summary>The bird's state (humo.js:18-22).</summary>
public sealed class HumoQush
{
    public Node3D Mesh = null!;
    public Gumanoid? Proc;
    public AnimCtrl? Anim;
    public HumoHolat Holat = HumoHolat.Qongan;
    public Vector3 Gumbaz;
    public double Taymer, Burchak, Balandlik;
    public double TakeoffTaymer, LandTaymer;
}

/// <summary>
/// The Humo bird — §2. Instead of "WAVE 2 INCOMING" on screen, the bird flies.
/// humo.js. There is no wave counter, no marker, no text: this is the telegraph.
/// </summary>
public static class Humo
{
    /// <summary>Each state maps to exactly one clip (animation doc §9).</summary>
    private static string Klip(HumoHolat h) => h switch
    {
        HumoHolat.Qongan => "perched_idle",
        HumoHolat.Ogoh => "wings_spread",
        HumoHolat.Uchdi => "takeoff",
        _ => "fly_away",
    };

    /// <summary>humo.js:7-23.</summary>
    public static HumoQush Yarat(Node3D scene, Node3D tayanch)
    {
        Node3D? mesh = BuxoroModels.Ol("humo");
        Gumanoid? proc = null;
        if (mesh == null)
        {
            proc = Actors.Humo();
            mesh = proc.Root;
        }

        mesh.Scale = Vector3.One * 3.2f; // readable from 250 m
        mesh.Position = tayanch.Position;
        scene.AddChild(mesh);

        var h = new HumoQush { Mesh = mesh, Proc = proc, Anim = Anim.Ulash(mesh, "humo"), Gumbaz = tayanch.Position };
        Anim.Oynat(h.Anim, "perched_idle");
        return h;
    }

    /// <summary>humo.js:33-49.</summary>
    public static void HolatBer(HumoQush h, HumoHolat yangi)
    {
        if (h.Holat == yangi)
        {
            return;
        }

        HumoHolat oldingi = h.Holat;
        h.Holat = yangi;

        if (yangi == HumoHolat.Qongan && oldingi == HumoHolat.Uchdi)
        {
            Anim.Oynat(h.Anim, "land", new AnimOpt { BirMarta = true, Majburiy = true });
            h.LandTaymer = 1.6;
        }
        else
        {
            Anim.Oynat(h.Anim, Klip(yangi), new AnimOpt { BirMarta = yangi is HumoHolat.Uchdi or HumoHolat.Ogoh, Majburiy = true });
            if (yangi == HumoHolat.Uchdi)
            {
                h.TakeoffTaymer = 1.2;
            }
        }

        if (yangi is HumoHolat.Uchdi or HumoHolat.Ketdi)
        {
            BuxoroAudio.S("qushUchdi");
        }
    }

    /// <summary>humo.js:51-109.</summary>
    public static void Yangila(HumoQush h, double dt)
    {
        Vector3 p = h.Mesh.Position;

        Anim.QulfYangila(h.Anim, dt);
        if (h.TakeoffTaymer > 0) // after takeoff, the circling clip
        {
            h.TakeoffTaymer -= dt;
            if (h.TakeoffTaymer <= 0)
            {
                Anim.Oynat(h.Anim, "fly_loop");
            }
        }

        if (h.LandTaymer > 0)
        {
            h.LandTaymer -= dt;
            if (h.LandTaymer <= 0)
            {
                Anim.Oynat(h.Anim, "perched_idle");
            }
        }

        float ry = h.Mesh.Rotation.Y;
        switch (h.Holat)
        {
            case HumoHolat.Qongan:
                p = p.Lerp(h.Gumbaz, (float)(1 - Math.Pow(0.01, dt)));
                h.Balandlik = 0;
                if (h.Proc != null)
                {
                    h.Proc.ChapQanot!.Rotation = Vector3.Zero;
                    h.Proc.OngQanot!.Rotation = Vector3.Zero;
                }

                ry += (float)((Math.PI - ry) * dt * 0.5);
                break;

            case HumoHolat.Ogoh:
                // wings spread, circling over the dome
                h.Burchak += dt * 0.55;
                h.Balandlik += (14 - h.Balandlik) * dt * 0.8;
                p = new Vector3(
                    (float)(h.Gumbaz.X + (Math.Cos(h.Burchak) * 16)),
                    (float)(h.Gumbaz.Y + h.Balandlik),
                    (float)(h.Gumbaz.Z + (Math.Sin(h.Burchak) * 16)));
                Qanot(h, 0.75, 3.2);
                ry = (float)(-h.Burchak + (Math.PI / 2));
                break;

            case HumoHolat.Uchdi:
                // the wave is on the field — west, circling high over it
                h.Burchak += dt * 0.32;
                h.Balandlik += (46 - h.Balandlik) * dt * 0.5;
                p = new Vector3((float)(95 + (Math.Cos(h.Burchak) * 62)), (float)h.Balandlik, (float)(Math.Sin(h.Burchak) * 34));
                Qanot(h, 0.5, 2.1);
                ry = (float)(-h.Burchak + (Math.PI / 2));
                break;

            case HumoHolat.Ketdi:
                // it does not return — east, to the horizon
                p += new Vector3((float)(dt * 26), (float)(dt * 7), 0);
                Qanot(h, 0.4, 1.6);
                if (p.X > 460)
                {
                    h.Mesh.Visible = false;
                }

                break;
        }

        h.Mesh.Position = p;
        h.Mesh.Rotation = new Vector3(h.Mesh.Rotation.X, ry, h.Mesh.Rotation.Z);
    }

    /// <summary>humo.js:111-116 — wall-clock flapping, like performance.now().</summary>
    private static void Qanot(HumoQush h, double amp, double tez)
    {
        if (h.Proc == null)
        {
            return;
        }

        double t = Time.GetTicksMsec() * 0.001 * tez;
        h.Proc.ChapQanot!.Rotation = new Vector3(0, 0, (float)(Math.Sin(t) * amp));
        h.Proc.OngQanot!.Rotation = new Vector3(0, 0, (float)(-Math.Sin(t) * amp));
    }
}
