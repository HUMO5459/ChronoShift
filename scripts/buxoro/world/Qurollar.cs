using Godot;

namespace ChronoShift;

/// <summary>
/// Weapons — one source, qurollar.js. Player, combat and the blueprint viewer all
/// take from here. A GLB wins if present (none is today), else procedural.
/// </summary>
public static class Qurollar
{
    private static StandardMaterial3D Mat(uint rang, float rough = 0.9f, float metal = 0f) => Actors.Mat(rang, rough, metal);

    private static T At<T>(T n, float x, float y, float z)
        where T : Node3D
    {
        n.Position = new Vector3(x, y, z);
        return n;
    }

    /// <summary>KALTAK (§2.1) — "not a weapon, a stick". qurollar.js:16-45.</summary>
    public static Node3D Kaltak()
    {
        Node3D? g = BuxoroModels.Ol("kaltak");
        if (g != null)
        {
            return g;
        }

        var k = new Node3D { Name = "kaltak" };
        var yog = Mat(0x6f5836, 0.95f);

        // heavy end wide, handle thin
        k.AddChild(Actors.Silindr(0.038f, 0.075f, 0.80f, yog));

        // bark stripped unevenly — small knots
        for (int i = 0; i < 5; i++)
        {
            float y = -0.3f + (i * 0.14f);
            float r = 0.04f + ((0.3f + y) * 0.05f);
            k.AddChild(Actors.Silindr(r, r, 0.05f, yog, 0.006f, y, 0.004f, 6));
        }

        k.AddChild(Actors.Silindr(0.048f, 0.048f, 0.20f, Mat(0x3d2f22, 0.9f), 0, -0.30f, 0));  // leather grip
        k.AddChild(Actors.Silindr(0.076f, 0.072f, 0.10f, Mat(0x554430, 0.95f), 0, 0.36f, 0));  // darkened striking end
        return k;
    }

    /// <summary>
    /// SIEVE-SHIELD (§2.2) — the game's signature object. qurollar.js:48-100.
    /// The wooden rim MUST read, or it is just a shield.
    /// </summary>
    public static Node3D Qalqon()
    {
        Node3D? g = BuxoroModels.Ol("galvir_qalqon");
        if (g != null)
        {
            return g;
        }

        var q = new Node3D { Name = "qalqon" };
        Node3D gardish = Actors.Torus(0.25f, 0.032f, Mat(0x9c7b4a, 0.9f), 8, 26);
        gardish.Rotation = new Vector3(0, Mathf.Pi / 2, 0);
        q.AddChild(gardish);

        // horsehair mesh — still partly visible
        var torMat = new StandardMaterial3D
        {
            AlbedoColor = new Color(Actors.Rang(0x7d6544), 0.85f), Roughness = 1,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled, Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        };
        MeshInstance3D tor = Actors.Disk(0.235f, 24, torMat);
        tor.Rotation = new Vector3(0, Mathf.Pi / 2, 0);
        q.AddChild(tor);

        // iron scraps from different objects — none alike
        (float W, float H, float Y, float Z, uint Rang, float Qiya)[] parchalar =
        {
            (0.145f, 0.17f, 0.055f, 0.06f, 0x6b6058, 0.18f),   // a pot shard
            (0.105f, 0.20f, -0.09f, -0.05f, 0x5a5550, -0.22f), // a flattened hook
            (0.17f, 0.085f, 0.02f, -0.13f, 0x625a52, 0.08f),
        };
        foreach (var p in parchalar)
        {
            MeshInstance3D pl = Actors.Quti(0.014f, p.W, p.H, Mat(p.Rang, 0.55f, 0.35f), 0.024f, p.Y, p.Z);
            pl.Rotation = new Vector3(p.Qiya, 0, 0);
            q.AddChild(pl);
            for (int i = 0; i < 3; i++) // rivets
            {
                q.AddChild(Actors.Quti(0.012f, 0.016f, 0.016f, Mat(0x4a443c, 0.5f, 0.4f),
                    0.034f, p.Y + ((i - 1) * p.W * 0.3f), p.Z + ((i - 1) * p.H * 0.25f)));
            }
        }

        // behind: leather strap and wooden grip
        q.AddChild(Actors.Quti(0.012f, 0.05f, 0.20f, Mat(0x5a4530, 0.95f), -0.03f, 0.06f, 0));
        MeshInstance3D dasta = Actors.Silindr(0.016f, 0.016f, 0.13f, Mat(0x6b5433, 0.9f), -0.05f, -0.06f, 0, 6);
        dasta.Rotation = new Vector3(Mathf.Pi / 2, 0, 0);
        q.AddChild(dasta);
        return q;
    }

    /// <summary>BOW (§2.3) — a learned weapon; a Central Asian recurve. qurollar.js:103-150.</summary>
    public static Node3D Kamon()
    {
        Node3D? g = BuxoroModels.Ol("kamon");
        if (g != null)
        {
            return g;
        }

        var k = new Node3D { Name = "kamon" };
        var yog = Mat(0x6b5433, 0.88f);
        Vector3[] shakl =
        {
            new(0, -0.60f, 0.06f), new(0, -0.42f, -0.03f), new(0, -0.14f, -0.05f), new(0, 0.00f, -0.045f),
            new(0, 0.14f, -0.05f), new(0, 0.42f, -0.03f), new(0, 0.60f, 0.06f),
        };
        k.AddChild(Actors.Tube(shakl, 22, 0.017f, 6, yog));

        // horn and sinew layers — a slightly different colour
        MeshInstance3D shox = Actors.Tube(shakl, 22, 0.011f, 5, Mat(0xbfae8c, 0.7f));
        shox.Position = new Vector3(0, 0, -0.014f);
        k.AddChild(shox);

        k.AddChild(Actors.Silindr(0.004f, 0.004f, 1.18f, Mat(0xc9bfa4, 0.95f), 0, 0, 0.06f, 4));    // string
        k.AddChild(Actors.Silindr(0.024f, 0.024f, 0.13f, Mat(0x4a3a26, 0.92f), 0, 0, -0.045f, 7));  // grip wrap
        foreach (float y in new[] { -0.56f, 0.56f })
        {
            k.AddChild(Actors.Silindr(0.021f, 0.021f, 0.05f, Mat(0x8a7a58, 0.9f), 0, y, 0.035f, 6)); // tip lashings
        }

        return k;
    }

    /// <summary>ARROW. qurollar.js:153-171. Its long axis is +Y.</summary>
    public static Node3D Oq()
    {
        var g = new Node3D { Name = "oq" };
        g.AddChild(Actors.Silindr(0.005f, 0.005f, 0.62f, Mat(0x8a7350, 0.9f), 0, 0, 0, 5));
        g.AddChild(At(Actors.Konus(0.015f, 0.07f, 4, Mat(0x555055, 0.45f, 0.3f)), 0, 0.34f, 0));
        for (int i = 0; i < 3; i++)
        {
            float a = i / 3f * Mathf.Tau;
            MeshInstance3D p = Actors.Quti(0.001f, 0.07f, 0.022f, Mat(0x9c9484, 0.95f), 0, -0.26f, 0);
            p.Rotation = new Vector3(0, a, 0);
            p.Position += new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * 0.012f; // translateZ(0.012)
            g.AddChild(p);
        }

        return g;
    }

    /// <summary>SWORD (§2.4) — "it must not be pretty, it is the first attempt". qurollar.js:174-229.</summary>
    public static Node3D Qilich()
    {
        Node3D? g = BuxoroModels.Ol("qilich");
        if (g != null)
        {
            return g;
        }

        var q = new Node3D { Name = "qilich" };
        q.AddChild(Actors.Quti(0.052f, 0.85f, 0.011f, Mat(0x555055, 0.42f, 0.55f), 0, 0.14f, 0)); // blade, one edge, unpolished
        for (int i = 0; i < 7; i++) // hammer marks — an uneven face
        {
            q.AddChild(Actors.Quti(0.048f, 0.055f, 0.013f, Mat(i % 2 == 1 ? 0x4e4a4eu : 0x5d585du, 0.5f, 0.5f),
                i % 2 == 1 ? 0.002f : -0.002f, -0.22f + (i * 0.11f), 0));
        }

        q.AddChild(Actors.Quti(0.014f, 0.85f, 0.005f, Mat(0x6e696e, 0.35f, 0.6f), 0.026f, 0.14f, 0)); // the edge
        q.AddChild(Actors.Quti(0.053f, 0.09f, 0.012f, Mat(0x3f3a36, 0.85f, 0.2f), 0, -0.25f, 0));     // scorch at the base

        MeshInstance3D panoh = Actors.Quti(0.20f, 0.028f, 0.028f, Mat(0x4a4038, 0.6f, 0.4f), 0, -0.29f, 0); // a bent iron rod
        panoh.Rotation = new Vector3(0, 0, 0.04f); // set on crooked
        q.AddChild(panoh);

        q.AddChild(Actors.Silindr(0.026f, 0.030f, 0.20f, Mat(0x33281e, 0.95f), 0, -0.40f, 0, 7)); // grip
        for (int i = 0; i < 6; i++) // leather cord
        {
            Node3D o = Actors.Torus(0.028f, 0.005f, Mat(0x2b2118, 0.95f), 4, 8);
            o.Rotation = new Vector3(Mathf.Pi / 2, 0, 0);
            o.Position = new Vector3(0, -0.33f - (i * 0.028f), 0);
            q.AddChild(o);
        }

        q.AddChild(Actors.Silindr(0.032f, 0.026f, 0.035f, Mat(0x4a4038, 0.6f, 0.4f), 0, -0.51f, 0, 7)); // pommel
        return q;
    }

    /// <summary>SPEAR (Mongol infantry). qurollar.js:232-247.</summary>
    public static Node3D Nayza()
    {
        var g = new Node3D { Name = "nayza" };
        g.AddChild(Actors.Silindr(0.018f, 0.021f, 1.8f, Mat(0x5f4a30, 0.92f), 0, 0, 0, 6));
        g.AddChild(At(Actors.Konus(0.032f, 0.24f, 4, Mat(0x4a4540, 0.5f, 0.4f)), 0, 0.98f, 0));
        g.AddChild(Actors.Silindr(0.024f, 0.024f, 0.09f, Mat(0x3a2c20, 0.95f), 0, 0.84f, 0, 6));
        return g;
    }
}
