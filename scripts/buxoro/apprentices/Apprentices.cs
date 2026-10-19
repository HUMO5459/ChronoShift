using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ChronoShift;

/// <summary>apprentices.js:7. BOSH must be 0 — <c>sh.oldingiHolat || HOLAT.BOSH</c> relies on it.</summary>
public enum ShogirdHolat
{
    Bosh = 0,
    Bormoqda = 1,
    Yigmoqda = 2,
    Tashimoqda = 3,
    Qochmoqda = 4,
}

/// <summary>One apprentice (apprentices.js:134-145). Position kept in double; the mesh follows.</summary>
public sealed class Shogird
{
    public Node3D Mesh = null!;
    public Gumanoid? Proc;
    public MeshInstance3D? Savat;
    public StandardMaterial3D? SavatMat;
    public int Indeks;
    public AnimCtrl? Anim;
    public ShogirdHolat Holat = ShogirdHolat.Bosh;
    public bool Bosh = true;
    public Belgi? Belgi;
    public double Taymer;
    public string? Yuk;
    public double QochishTaymer;
    public ShogirdHolat? OldingiHolat;
    public Dushman? QochishManba;
    public double Faza = GD.Randf() * 6;
    public readonly ProtseduralHolat Yurish = new();

    public double X, Z, Yaw;

    public void Sync()
    {
        Mesh.Position = new Vector3((float)X, (float)World.YerBalandligi(X, Z), (float)Z);
        Mesh.Rotation = new Vector3(Mesh.Rotation.X, (float)Yaw, Mesh.Rotation.Z);
    }
}

/// <summary>A mark on a resource node (apprentices.js:46).</summary>
public sealed class Belgi
{
    public Tugun Tugun = null!;
    public Node3D Mesh = null!;
    public StandardMaterial3D UstunMat = null!;
    public Shogird? Egallagan;
}

/// <summary>The apprentice system state, apprentices.js:44-50.</summary>
public sealed class ShogirdTizim
{
    public readonly List<Shogird> Shogirdlar = new();
    public readonly List<Belgi> Belgilar = new();
    public readonly Dictionary<string, int> Ombor = new() { ["tosh"] = 0, ["yogoch"] = 0, ["teri"] = 0, ["temir"] = 0, ["komir"] = 0 };
    public (Hunarmand Hunarmand, double Boshi)? Yollanmoqda;
    public int JamiYigilgan;
}

/// <summary>What <see cref="Apprentices.YollashTekshir"/> reports (apprentices.js:378-414).</summary>
public sealed record YollashNatija(bool Tugadi, double? Nisbat, Hunarmand Hunarmand, bool Yaqin);

/// <summary>
/// MARKING + apprentice AI — apprentices.js. The heart of the game: Shomurod's
/// time is for marking, the apprentices' for carrying.
/// </summary>
/// <remarks>
/// REFUSED: NavigationAgent3D and any collision. Apprentices walk straight lines
/// and pass through walls and houses — their travel times ARE the economy, and the
/// web build never resolves their collisions (toqnashuvHal has one caller).
/// </remarks>
public static class Apprentices
{
    private static BuxoroBalance B => BuxoroRuntime.B;

    /// <summary>world.js:29-35, also the basket tint while carrying.</summary>
    public static readonly Dictionary<string, uint> ResursRangi = new()
    {
        ["tosh"] = 0x7d7568, ["yogoch"] = 0x7d6039, ["teri"] = 0xa07a4e, ["temir"] = 0x6d4a35, ["komir"] = 0x2b2724,
    };

    // Dust tints for the GLB robes, apprentices.js:76-83
    private static readonly uint[] Chang = { 0x5f5c52, 0x6b5f47, 0x555c4e, 0x685550, 0x474540, 0x6e6656 };

    /// <summary>
    /// The mark pillar — a yellow beam, 8 m, readable from far away (apprentices.js:10-41).
    /// It MUST read against the bright sky: it is the mechanic's only signal, so it is
    /// additive, fog-proof and writes no depth.
    /// </summary>
    private static (Node3D, StandardMaterial3D) BelgiYarat(Node3D scene, double x, double z)
    {
        var g = new Node3D { Name = "belgi" };
        StandardMaterial3D Nur(uint rang, float alfa, bool additive) => new()
        {
            AlbedoColor = new Color(Actors.Rang(rang), alfa),
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            BlendMode = additive ? BaseMaterial3D.BlendModeEnum.Add : BaseMaterial3D.BlendModeEnum.Mix,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Disabled,
            DisableFog = true,
        };

        StandardMaterial3D ustunMat = Nur(0xffb524, 0.85f, true);
        MeshInstance3D ustun = Actors.Silindr(0.22f, 0.42f, 8, ustunMat, 0, 4, 0, 10, ochiq: true);
        ustun.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        g.AddChild(ustun);

        // inner core — denser, so the silhouette is crisp
        MeshInstance3D yadro = Actors.Silindr(0.07f, 0.14f, 8, Nur(0xfff0c0, 0.95f, false), 0, 4, 0, 6, ochiq: true);
        yadro.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        g.AddChild(yadro);

        // RingGeometry(0.55, 0.95, 24) lying flat
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        for (int i = 0; i < 24; i++)
        {
            float a0 = Mathf.Tau * i / 24, a1 = Mathf.Tau * (i + 1) / 24;
            Vector3 P(float r, float a) => new(r * Mathf.Cos(a), 0, r * Mathf.Sin(a));
            st.AddVertex(P(0.55f, a0));
            st.AddVertex(P(0.95f, a0));
            st.AddVertex(P(0.95f, a1));
            st.AddVertex(P(0.55f, a0));
            st.AddVertex(P(0.95f, a1));
            st.AddVertex(P(0.55f, a1));
        }

        st.SetMaterial(Nur(0xffb524, 0.95f, false));
        g.AddChild(new MeshInstance3D { Mesh = st.Commit(), Position = new Vector3(0, 0.09f, 0), CastShadow = GeometryInstance3D.ShadowCastingSetting.Off });

        g.Position = new Vector3((float)x, 0, (float)z);
        scene.AddChild(g);
        return (g, ustunMat);
    }

    /// <summary>apprentices.js:43-55.</summary>
    public static ShogirdTizim Tizim(Node3D scene, Dunyo dunyo)
    {
        var s = new ShogirdTizim();
        for (int i = 0; i < B.ShogirdBoshlangich; i++)
        {
            ShogirdQosh(scene, s, dunyo);
        }

        return s;
    }

    /// <summary>apprentices.js:57-148.</summary>
    public static Shogird? ShogirdQosh(Node3D scene, ShogirdTizim s, Dunyo dunyo)
    {
        if (s.Shogirdlar.Count >= B.ShogirdMaksimum)
        {
            return null;
        }

        int i = s.Shogirdlar.Count;
        Node3D? mesh = BuxoroModels.Ol("shogird") ?? BuxoroModels.Ol("hunarmand");
        Gumanoid? proc = null;
        MeshInstance3D? savat;
        StandardMaterial3D? savatMat;

        double yaw = 0;
        if (mesh != null)
        {
            // Each apprentice a different dust tone, and the ikat robe desaturated
            // (art doc §0 bans "saturated candy colours"). Multiplies the texture.
            Color tus = Actors.Rang(Chang[i % 6]);
            foreach (Node n in mesh.FindChildren("*", nameof(MeshInstance3D), true, false))
            {
                if (n is not MeshInstance3D mi || mi.Mesh == null)
                {
                    continue;
                }

                for (int k = 0; k < mi.Mesh.GetSurfaceCount(); k++)
                {
                    if (mi.GetActiveMaterial(k) is BaseMaterial3D bm)
                    {
                        var kl = (BaseMaterial3D)bm.Duplicate(); // materialniKlonla
                        kl.AlbedoColor *= tus;
                        mi.SetSurfaceOverrideMaterial(k, kl);
                    }
                }
            }

            // apprentices.js:90 — applied to whichever model loaded, shogird.glb included
            mesh.Scale *= 1.62f / 1.35f;

            savatMat = new StandardMaterial3D { AlbedoColor = Actors.Rang(0xa88b5c), Roughness = 1, CullMode = BaseMaterial3D.CullModeEnum.Disabled };
            savat = Actors.Silindr(0.19f, 0.15f, 0.14f, savatMat, 0, 0, 0, 10, ochiq: true);

            // the basket is attached once the mesh is in the tree (bone transforms)
        }
        else
        {
            proc = Actors.Shogird(i);
            mesh = proc.Root;
            savat = proc.Savat;
            savatMat = (savat?.Mesh as PrimitiveMesh)?.Material as StandardMaterial3D;
        }

        var sh = new Shogird { Mesh = mesh, Proc = proc, Savat = savat, SavatMat = savatMat, Indeks = i };

        // The first two stand BESIDE THE PLAYER (§8); recruits appear by the forge.
        if (i < B.ShogirdBoshlangich)
        {
            double bur = (-Math.PI / 2) + ((i - 0.5) * 0.9); // in front, either side
            sh.X = B.ShomurodBoshi.X + (Math.Cos(bur) * 2.6);
            sh.Z = B.ShomurodBoshi.Y + (Math.Sin(bur) * 2.6);
            yaw = -Math.PI / 2;
        }
        else
        {
            double bur = (double)i / B.ShogirdMaksimum * Math.PI * 2;
            sh.X = B.Ustaxona.X + (Math.Cos(bur) * 4.5);
            sh.Z = B.Ustaxona.Y + (Math.Sin(bur) * 4.5);
        }

        sh.Yaw = yaw;
        mesh.Position = new Vector3((float)sh.X, 0, (float)sh.Z); // y = 0 until the first step
        mesh.Rotation = new Vector3(0, (float)yaw, 0);
        scene.AddChild(mesh);

        if (proc == null && savat != null && !BuxoroModels.SuyakkaUla(BuxoroModels.Qol(mesh, 'L'), savat, new Vector3(0, -0.04f, 0.13f), Vector3.Zero))
        {
            savat.Position = new Vector3(0, 1.02f, 0.30f);
            mesh.AddChild(savat);
        }

        sh.Anim = Anim.Ulash(mesh, "shogird");
        s.Shogirdlar.Add(sh);
        return sh;
    }

    /// <summary>apprentices.js:151-153 — the queue holds apprentices + 2.</summary>
    public static int NavbatLimiti(ShogirdTizim s) => s.Shogirdlar.Count + B.BelgiNavbatiQoshimcha;

    /// <summary>apprentices.js:155-164 — the nearest live, unmarked node within 3 m.</summary>
    public static Tugun? YaqinTugun(Dunyo dunyo, double x, double z)
    {
        Tugun? eng = null;
        double engM = B.BelgilashRadius * B.BelgilashRadius;
        foreach (Tugun t in dunyo.Tugunlar)
        {
            if (t.Qoldi <= 0 || t.Belgilangan)
            {
                continue;
            }

            double dx = t.X - x, dz = t.Z - z, m = (dx * dx) + (dz * dz);
            if (m < engM)
            {
                engM = m;
                eng = t;
            }
        }

        return eng;
    }

    /// <summary>apprentices.js:167-178. Touch calls this directly with the tapped node.</summary>
    public static string BelgilaTugun(Node3D scene, ShogirdTizim s, Tugun? t)
    {
        if (t == null || t.Qoldi <= 0 || t.Belgilangan)
        {
            return "yoq";
        }

        if (s.Belgilar.Count >= NavbatLimiti(s))
        {
            BuxoroAudio.S("rad");
            return "navbatTola";
        }

        t.Belgilangan = true;
        (Node3D mesh, StandardMaterial3D mat) = BelgiYarat(scene, t.X, t.Z);
        s.Belgilar.Add(new Belgi { Tugun = t, Mesh = mesh, UstunMat = mat });
        BuxoroAudio.S("belgi");
        return "ok";
    }

    /// <summary>apprentices.js:180-182.</summary>
    public static string Belgila(Node3D scene, ShogirdTizim s, Dunyo dunyo, double x, double z) =>
        BelgilaTugun(scene, s, YaqinTugun(dunyo, x, z));

    private static void BelginiOchir(ShogirdTizim s, Belgi belgi)
    {
        belgi.Mesh.QueueFree();
        belgi.Tugun.Belgilangan = false;
        s.Belgilar.Remove(belgi);
    }

    /// <summary>apprentices.js:193-200.</summary>
    public static void Yangila(Node3D scene, ShogirdTizim s, Dunyo dunyo, double dt, List<Dushman> dushmanlar, double vaqt)
    {
        foreach (Belgi b in s.Belgilar)
        {
            Color c = b.UstunMat.AlbedoColor;
            b.UstunMat.AlbedoColor = new Color(c.R, c.G, c.B, (float)(0.72 + (Math.Sin(vaqt * 3) * 0.22))); // the beam pulses
        }

        foreach (Shogird sh in s.Shogirdlar)
        {
            YangilaShogird(s, sh, dt, dushmanlar);
        }
    }

    /// <summary>apprentices.js:202-355.</summary>
    private static void YangilaShogird(ShogirdTizim s, Shogird sh, double dt, List<Dushman> dushmanlar)
    {
        // --- FLEE: overrides every state (§4) ---
        if (sh.Holat != ShogirdHolat.Qochmoqda)
        {
            Dushman? yaqin = null;
            double engM = B.QochishRadiusi * B.QochishRadiusi;
            foreach (Dushman d in dushmanlar)
            {
                if (d.Olgan)
                {
                    continue;
                }

                double dx = d.X - sh.X, dz = d.Z - sh.Z, m = (dx * dx) + (dz * dz);
                if (m < engM)
                {
                    engM = m;
                    yaqin = d;
                }
            }

            if (yaqin != null)
            {
                sh.OldingiHolat = sh.Holat;
                sh.Holat = ShogirdHolat.Qochmoqda;
                sh.QochishTaymer = B.QochishDavomiyligi;
                sh.QochishManba = yaqin;
            }
        }

        switch (sh.Holat)
        {
            case ShogirdHolat.Bosh:
            {
                sh.Bosh = true;

                // takes the nearest unclaimed mark
                Belgi? eng = null;
                double engM = double.PositiveInfinity;
                foreach (Belgi b in s.Belgilar)
                {
                    if (b.Egallagan != null)
                    {
                        continue;
                    }

                    double dx = b.Tugun.X - sh.X, dz = b.Tugun.Z - sh.Z, m = (dx * dx) + (dz * dz);
                    if (m < engM)
                    {
                        engM = m;
                        eng = b;
                    }
                }

                if (eng != null)
                {
                    eng.Egallagan = sh;
                    sh.Belgi = eng;
                    sh.Holat = ShogirdHolat.Bormoqda;
                    sh.Bosh = false;
                }

                break;
            }

            case ShogirdHolat.Bormoqda:
                if (sh.Belgi == null || sh.Belgi.Tugun.Qoldi <= 0)
                {
                    if (sh.Belgi != null)
                    {
                        BelginiOchir(s, sh.Belgi);
                    }

                    sh.Belgi = null;
                    sh.Holat = ShogirdHolat.Bosh;
                    break;
                }

                if (Yur(sh, sh.Belgi.Tugun.X, sh.Belgi.Tugun.Z, dt, 1.1))
                {
                    sh.Holat = ShogirdHolat.Yigmoqda;
                    sh.Taymer = B.YigishVaqti;
                }

                break;

            case ShogirdHolat.Yigmoqda:
            {
                sh.Taymer -= dt;
                if (sh.Proc != null) // gathering sway
                {
                    sh.Proc.ChapQol.Rotation = new Vector3((float)(-0.9 + (Math.Sin(sh.Taymer * 9) * 0.35)), 0, 0);
                    sh.Proc.OngQol.Rotation = new Vector3((float)(-0.9 + (Math.Sin((sh.Taymer * 9) + 1) * 0.35)), 0, 0);
                }

                if (sh.Taymer <= 0)
                {
                    Tugun t = sh.Belgi!.Tugun;
                    t.Qoldi -= B.BirSafarOladi;
                    sh.Yuk = t.Tur;
                    if (sh.SavatMat != null)
                    {
                        sh.SavatMat.AlbedoColor = Actors.Rang(ResursRangi[t.Tur]);
                    }

                    if (t.Qoldi <= 0)
                    {
                        t.Mesh.Visible = false;
                    }

                    BelginiOchir(s, sh.Belgi);
                    sh.Belgi = null;
                    sh.Holat = ShogirdHolat.Tashimoqda;
                    BuxoroAudio.S("yigildi");
                }

                break;
            }

            case ShogirdHolat.Tashimoqda:
                if (Yur(sh, B.Ustaxona.X, B.Ustaxona.Y, dt, 2.8))
                {
                    Anim.Oynat(sh.Anim, "deliver", new AnimOpt { BirMarta = true, Majburiy = true });
                    s.Ombor[sh.Yuk!] += B.BirSafarOladi;
                    s.JamiYigilgan++;
                    sh.Yuk = null;
                    if (sh.SavatMat != null)
                    {
                        sh.SavatMat.AlbedoColor = Actors.Rang(0xa88b5c);
                    }

                    sh.Holat = ShogirdHolat.Bosh;
                }

                break;

            case ShogirdHolat.Qochmoqda:
            {
                sh.QochishTaymer -= dt;
                Dushman? d = sh.QochishManba;
                if (d != null && !d.Olgan)
                {
                    // directly away from the enemy
                    double dx = sh.X - d.X, dz = sh.Z - d.Z;
                    double u = Math.Sqrt((dx * dx) + (dz * dz));
                    if (u == 0)
                    {
                        u = 1;
                    }

                    Yur(sh, sh.X + (dx / u * 10), sh.Z + (dz / u * 10), dt, 0.5, 1.35);
                }

                if (sh.QochishTaymer <= 0)
                {
                    // back to the previous job (§4: apprentices do not die)
                    sh.Holat = sh.OldingiHolat == ShogirdHolat.Yigmoqda ? ShogirdHolat.Bormoqda : (sh.OldingiHolat ?? ShogirdHolat.Bosh);
                    if (sh.Holat == ShogirdHolat.Bormoqda && sh.Belgi == null)
                    {
                        sh.Holat = ShogirdHolat.Bosh;
                    }
                }

                break;
            }
        }

        // --- animation per state ---
        Anim.QulfYangila(sh.Anim, dt);
        if (sh.Anim != null)
        {
            switch (sh.Holat)
            {
                case ShogirdHolat.Bosh:
                    Anim.Oynat(sh.Anim, "idle_waiting");
                    break;
                case ShogirdHolat.Bormoqda:
                    Anim.Yurish(sh.Anim, new[] { "run", "carry_run", "walk" }, B.ShogirdTezligi);
                    break;
                case ShogirdHolat.Yigmoqda:
                    Anim.Oynat(sh.Anim, "gather_stone");
                    break;
                case ShogirdHolat.Tashimoqda:
                    Anim.Yurish(sh.Anim, new[] { "carry_run", "run", "walk" }, B.ShogirdTezligi);
                    break;
                case ShogirdHolat.Qochmoqda:
                    Anim.Yurish(sh.Anim, new[] { "flee", "run", "walk" }, B.ShogirdTezligi * 1.35);
                    break;
            }
        }

        bool harakatda = sh.Holat is ShogirdHolat.Bormoqda or ShogirdHolat.Tashimoqda or ShogirdHolat.Qochmoqda;

        // A clipless GLB just glides — give it the procedural step bob.
        if (sh.Proc == null && !Anim.Bormi(sh.Anim, "carry_run"))
        {
            Anim.ProtseduralYurish(sh.Yurish, sh.Mesh, harakatda, dt, "shogird", World.YerBalandligi(sh.X, sh.Z));
        }

        // procedural limbs for the box placeholder
        if (sh.Proc != null && sh.Holat != ShogirdHolat.Yigmoqda)
        {
            if (harakatda)
            {
                sh.Faza += dt * 9;
                float a = (float)(Math.Sin(sh.Faza) * 0.55);
                sh.Proc.ChapOyoq.Rotation = new Vector3(a, 0, 0);
                sh.Proc.OngOyoq.Rotation = new Vector3(-a, 0, 0);
                sh.Proc.ChapQol.Rotation = new Vector3(-1.15f, 0, 0); // basket in both hands
                sh.Proc.OngQol.Rotation = new Vector3(-1.15f, 0, 0);
            }
            else
            {
                float k = Smoothing.LimbRelax(dt);
                sh.Proc.ChapOyoq.Rotation *= k;
                sh.Proc.OngOyoq.Rotation *= k;
            }
        }
    }

    /// <summary>apprentices.js:358-369 — walks toward a target; true on arrival.</summary>
    private static bool Yur(Shogird sh, double x, double z, double dt, double tolerans = 1.0, double tezlikKoef = 1)
    {
        double dx = x - sh.X, dz = z - sh.Z;
        double m = Math.Sqrt((dx * dx) + (dz * dz));
        if (m < tolerans)
        {
            return true;
        }

        double v = B.ShogirdTezligi * tezlikKoef * dt;
        sh.X += dx / m * v;
        sh.Z += dz / m * v;
        sh.Yaw = Player.BurchakLerp(sh.Yaw, Math.Atan2(dx, dz), Smoothing.ActorYaw(dt));
        sh.Sync();
        return false;
    }

    /// <summary>
    /// RECRUITING: hold E (apprentices.js:378-414). Each craftsman's bar fills
    /// SEPARATELY: the E timer keeps rising until release, so progress counts from
    /// the value it had when this craftsman was reached — otherwise walking on to the
    /// next one without releasing E recruits instantly (playtest, band 6).
    /// </summary>
    public static YollashNatija? YollashTekshir(Node3D scene, ShogirdTizim s, Dunyo dunyo, double px, double pz, double eUshlandi)
    {
        Hunarmand? yaqin = null;
        double engM = 3.2 * 3.2;
        foreach (Hunarmand h in dunyo.Hunarmandlar)
        {
            if (h.Yollandi)
            {
                continue;
            }

            double dx = h.X - px, dz = h.Z - pz, m = (dx * dx) + (dz * dz);
            if (m < engM)
            {
                engM = m;
                yaqin = h;
            }
        }

        if (yaqin == null || s.Shogirdlar.Count >= B.ShogirdMaksimum)
        {
            s.Yollanmoqda = null;
            return null;
        }

        if (s.Yollanmoqda == null || s.Yollanmoqda.Value.Hunarmand != yaqin)
        {
            s.Yollanmoqda = (yaqin, eUshlandi); // a new target starts from zero
        }

        if (eUshlandi < s.Yollanmoqda.Value.Boshi)
        {
            s.Yollanmoqda = (yaqin, 0); // E was released
        }

        double ushlandi = eUshlandi - s.Yollanmoqda.Value.Boshi;
        if (ushlandi > 0.15)
        {
            double nisbat = Math.Min(1, ushlandi / B.YollashVaqti);
            if (nisbat >= 1)
            {
                yaqin.Yollandi = true;
                yaqin.Mesh.Visible = false;
                Shogird? sh = ShogirdQosh(scene, s, dunyo);
                if (sh != null)
                {
                    sh.X = yaqin.X;
                    sh.Z = yaqin.Z;
                    sh.Mesh.Position = new Vector3((float)yaqin.X, 0, (float)yaqin.Z);
                }

                BuxoroAudio.S("shogird");
                s.Yollanmoqda = null;
                return new YollashNatija(true, null, yaqin, false);
            }

            return new YollashNatija(false, nisbat, yaqin, false);
        }

        return new YollashNatija(false, null, yaqin, true);
    }

    /// <summary>apprentices.js:416.</summary>
    public static int BoshSoni(ShogirdTizim s) => s.Shogirdlar.Count(x => x.Bosh);
}
