using System;
using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>One enemy (combat.js:130-136). Position in double; the mesh follows.</summary>
public sealed class Dushman
{
    public string Tur = "";
    public BuxoroEnemySpec Spec = null!;
    public Node3D Mesh = null!;
    public Gumanoid? Proc;
    public Node3D? OtMesh;
    public AnimCtrl? Anim;
    public double Hp;
    public bool Olgan;
    public double HujumTaymer, OlimTaymer;
    public double Faza = GD.Randf() * 6;
    public bool OlimKlipi;
    public bool Chavandoz; // a GLB rider seated on the horse (web 2026-10-19)
    public readonly ProtseduralHolat Yurish = new();
    public double X, Y, Z, Yaw;
}

/// <summary>One allied soldier (combat.js:165-170).</summary>
public sealed class Askar
{
    public Node3D Mesh = null!;
    public Gumanoid? Proc;
    public AnimCtrl? Anim;
    public string Qurol = "yoq";
    public double Hp;
    public bool Olgan;
    public double OtishTaymer, HujumTaymer;
    public double UyX, UyZ;
    public Node3D? QurolMesh;
    public double Faza = GD.Randf() * 6;
    public readonly ProtseduralHolat Yurish = new();
    public double X, Y, Z, Yaw;
}

/// <summary>An arrow in flight (combat.js:214). It homes and cannot miss.</summary>
public sealed class Oq
{
    public Node3D Mesh = null!;
    public Dushman? Maqsad;
    public double Umr;
    public double Zarar;
    public double X, Y, Z;
}

/// <summary>What game.js's <c>hodisa(tur, m)</c> receives.</summary>
public sealed record Hodisa(string Tur, Dushman? Dushman = null, object? Maqsad = null, DevorNuqta? Devor = null);

/// <summary>Combat — §5, §10, §12: waves, the wall, soldiers. combat.js.</summary>
/// <remarks>
/// Enemy spawning uses GD.Randf, not the LCG: combat.js:121/143 use Math.random,
/// so wave layouts differ every run in both builds. That split is deliberate.
/// </remarks>
public static class Combat
{
    private static BuxoroBalance B => BuxoroRuntime.B;
    private static double R() => GD.Randf();

    // ================= WALL =================

    /// <summary>combat.js:7-51.</summary>
    public static Node3D DevorQur(Node3D scene, DevorNuqta nuqta, Dunyo dunyo)
    {
        Node3D? mesh = BuxoroModels.Ol("devor_seksiya");
        if (mesh == null)
        {
            mesh = new Node3D { Name = "devor_seksiya" };
            var m = Actors.Mat(0x8a8073, 1.0f);

            // dry-stacked stone, an uneven top (§2.6)
            for (int r = 0; r < 4; r++)
            {
                for (int i = 0; i < 9; i++)
                {
                    double w = 0.28 + (R() * 0.16);
                    double h = 0.26 + (R() * 0.08);
                    double bx = (R() - 0.5) * 0.12;
                    MeshInstance3D b = Actors.Quti(0.75f, (float)h, (float)w, m, (float)bx, 0.14f + (r * 0.30f), -1.4f + (i * 0.33f) + (r % 2 * 0.14f));
                    b.Rotation = new Vector3(0, (float)((R() - 0.5) * 0.16), 0);
                    mesh.AddChild(b);
                }
            }

            for (int i = 0; i < 4; i++) // stakes leaning forward
            {
                MeshInstance3D q = Actors.Silindr(0.05f, 0.07f, 1.5f, Actors.Mat(0x5f4a30, 1.0f), -0.55f, 0.55f, -1.1f + (i * 0.75f), 6);
                q.Rotation = new Vector3(0.42f, 0, 0);
                mesh.AddChild(q);
            }
        }

        mesh.Position = new Vector3((float)nuqta.X, 0, (float)nuqta.Z);
        scene.AddChild(mesh);
        nuqta.Mesh = mesh;
        nuqta.Qurilgan = true;
        nuqta.Hp = B.DevorSeksiyaHp;

        // Footprint so the player cannot walk through his own wall (playtest, band 7).
        if (nuqta.Tosiq != null)
        {
            World.TosiqOchir(dunyo, nuqta.Tosiq);
        }

        BuxoroModels.Olcham(mesh, Transform3D.Identity, out Aabb q2);
        Vector3 s = q2.Size, c = q2.GetCenter();

        // Under 1 m (the GLB is ~0.3 m) the section is stepped over, not an obstacle.
        // Kept on purpose: the built wall is a visual, an enemy trigger and a time bonus.
        nuqta.Tosiq = s.Y < 1.0f ? null : World.TosiqQosh(dunyo, Tosiq.Tortburchak(c.X, c.Z, Math.Max(0.8, s.X), Math.Max(1.0, s.Z), 0));
        return mesh;
    }

    /// <summary>combat.js:53-74.</summary>
    public static void DevorBuzildi(Node3D scene, DevorNuqta nuqta, Dunyo dunyo)
    {
        nuqta.Mesh?.QueueFree();
        if (nuqta.Tosiq != null)
        {
            World.TosiqOchir(dunyo, nuqta.Tosiq); // the rubble can be walked over
            nuqta.Tosiq = null;
        }

        Node3D? mesh = BuxoroModels.Ol("devor_buzilgan");
        if (mesh == null)
        {
            mesh = new Node3D { Name = "devor_buzilgan" };
            var m = Actors.Mat(0x8a8073, 1.0f);
            for (int i = 0; i < 14; i++)
            {
                double w = 0.5 + (R() * 0.3);
                double x = (R() - 0.5) * 2.2, y = 0.12 + (R() * 0.2), z = -1.5 + (R() * 3);
                MeshInstance3D b = Actors.Quti((float)w, 0.24f, 0.3f, m, (float)x, (float)y, (float)z);
                Actors.JsRotation(b, R(), R() * 3, R() * 0.4);
                mesh.AddChild(b);
            }
        }

        mesh.Position = new Vector3((float)nuqta.X, 0, (float)nuqta.Z);
        scene.AddChild(mesh);
        nuqta.Mesh = mesh;
        nuqta.Qurilgan = false;
        nuqta.Buzilgan = true;
    }

    // ================= ENEMY =================

    /// <summary>combat.js:77-137.</summary>
    public static Dushman DushmanYarat(Node3D scene, string tur, double z)
    {
        BuxoroEnemySpec spec = B.Enemy(tur)!;
        Node3D? mesh;
        Gumanoid? proc = null;
        Node3D? otMesh = null;
        bool chavandoz = false;

        if (tur == "piyoda")
        {
            mesh = KlonMat(BuxoroModels.Ol("mogul_piyoda"));
            if (mesh == null)
            {
                proc = Actors.MogulPiyoda();
                mesh = proc.Root;
            }

            scene.AddChild(mesh);
            Node3D g = Qurollar.Nayza();
            if (!BuxoroModels.SuyakkaUla(BuxoroModels.Qol(mesh, 'R'), g, new Vector3(0, 0.30f, 0.02f), new Vector3(Mathf.Pi / 2, 0, 0)))
            {
                Node3D q = proc != null ? proc.OngQol : BuxoroModels.QolTayanch(mesh, 'R');
                if (proc != null)
                {
                    g.Position = new Vector3(0, -0.55f, 0.12f);
                }

                g.Rotation = new Vector3(0.25f, 0, 0);
                q.AddChild(g);
            }
        }
        else if (tur == "otliq")
        {
            mesh = KlonMat(BuxoroModels.Ol("mogul_otliq"));

            if (mesh == null)
            {
                /* The rider — the Mongol infantry model seated on the horse (web
                   2026-10-19: "riders on the horses too"); the box figure only
                   when that model is missing. */
                mesh = KlonMat(BuxoroModels.Ol("mogul_piyoda"));
                if (mesh != null)
                {
                    chavandoz = true;
                }
                else
                {
                    proc = Actors.MogulPiyoda();
                    mesh = proc.Root;
                    proc.ChapOyoq.Rotation = new Vector3(-1.1f, 0, 0);
                    proc.OngOyoq.Rotation = new Vector3(-1.1f, 0, 0);
                }

                otMesh = BuxoroModels.Ol("ot") ?? Actors.Ot(0x7d6a4c);
                scene.AddChild(otMesh);
            }

            scene.AddChild(mesh);
            if (chavandoz)
            {
                Node3D nayza = Qurollar.Nayza();
                if (!BuxoroModels.SuyakkaUla(BuxoroModels.Qol(mesh, 'R'), nayza, new Vector3(0, 0.30f, 0.02f), new Vector3(Mathf.Pi / 2, 0, 0)))
                {
                    nayza.Rotation = new Vector3(0.25f, 0, 0);
                    BuxoroModels.QolTayanch(mesh, 'R').AddChild(nayza);
                }
            }

            // red banner (§5) — ~3.5 m above the ground; a GLB root is scaled, so cancel it
            float s = mesh.Scale.Y == 0 ? 1 : mesh.Scale.Y;
            float bayroqY = chavandoz ? (float)(2.05 + 1.42 - BuxoroModels.EgarY) : 2.05f;
            MeshInstance3D bayroq = Actors.Quti(0.02f, 0.42f, 0.42f, Actors.Unlit(0xa8342a), 0, bayroqY / s, -0.5f / s);
            bayroq.Scale = Vector3.One / s;
            mesh.AddChild(bayroq);
        }
        else
        {
            mesh = KlonMat(BuxoroModels.Ol("noyon"));
            if (mesh == null)
            {
                proc = Actors.Noyon();
                mesh = proc.Root;
            }

            scene.AddChild(mesh);
            Node3D qilich = Qurollar.Qilich();
            qilich.Scale = Vector3.One * 1.2f; // overridden by the bone attach, as in combat.js
            if (!BuxoroModels.SuyakkaUla(BuxoroModels.Qol(mesh, 'R'), qilich, new Vector3(0, 0.16f, 0.02f), new Vector3(Mathf.Pi / 2, 0, 0)))
            {
                Node3D q = proc != null ? proc.OngQol : BuxoroModels.QolTayanch(mesh, 'R');
                if (proc != null)
                {
                    qilich.Position = new Vector3(0, -0.72f, 0.12f);
                }

                qilich.Rotation = new Vector3(0.5f, 0, 0);
                q.AddChild(qilich);
            }
        }

        // from the west edge — the spawn zone
        double sx = 2 + (R() * B.SpawnZonasiX);
        double syer = World.YerBalandligi(sx, z);
        var d = new Dushman
        {
            Tur = tur, Spec = spec, Mesh = mesh, Proc = proc, OtMesh = otMesh, Hp = spec.Hp, Chavandoz = chavandoz,
            X = sx, Y = syer + (tur == "otliq" && otMesh != null ? (chavandoz ? BuxoroModels.EgarY : 1.42) : 0), Z = z,
        };
        mesh.Position = new Vector3((float)d.X, (float)d.Y, (float)d.Z);
        if (otMesh != null)
        {
            // the horse a little ahead (saddle under the rider), head toward the march
            otMesh.Position = new Vector3((float)sx, (float)syer, (float)(z + (chavandoz ? BuxoroModels.EgarOrqa : 0)));
            otMesh.Rotation = new Vector3(0, (float)BuxoroModels.OtBurilish(), 0);
        }

        d.Anim = Anim.Ulash(mesh, tur == "piyoda" ? "mogul_piyoda" : tur == "otliq" ? (chavandoz ? "mogul_piyoda" : "mogul_otliq") : "noyon");
        return d;
    }

    /// <summary>MODELS.ol(nom, { materialniKlonla: true }) — per-instance materials.</summary>
    private static Node3D? KlonMat(Node3D? mesh)
    {
        if (mesh == null)
        {
            return null;
        }

        foreach (Node n in BuxoroModels.Avlodlar<MeshInstance3D>(mesh))
        {
            if (n is MeshInstance3D mi && mi.Mesh != null)
            {
                for (int k = 0; k < mi.Mesh.GetSurfaceCount(); k++)
                {
                    if (mi.GetActiveMaterial(k) is Material m)
                    {
                        mi.SetSurfaceOverrideMaterial(k, (Material)m.Duplicate());
                    }
                }
            }
        }

        return mesh;
    }

    /// <summary>combat.js:139-151.</summary>
    public static List<Dushman> TolqinChiqar(Node3D scene, BuxoroWaveRow tolqin)
    {
        var yangi = new List<Dushman>();
        void Qoy(string tur, int soni)
        {
            for (int i = 0; i < soni; i++)
            {
                double z = -38 + ((double)i / Math.Max(1, soni - 1) * 76) + ((R() - 0.5) * 8);
                yangi.Add(DushmanYarat(scene, tur, Math.Max(-46, Math.Min(46, z))));
            }
        }

        Qoy("piyoda", tolqin.Piyoda);
        Qoy("otliq", tolqin.Otliq);
        Qoy("noyon", tolqin.Noyon);
        return yangi;
    }

    // ================= SOLDIERS =================

    /// <summary>combat.js:154-175.</summary>
    public static List<Askar> AskarlarYarat(Node3D scene)
    {
        var royxat = new List<Askar>();
        for (int i = 0; i < B.AskarSoni; i++)
        {
            Node3D? mesh = KlonMat(BuxoroModels.Ol("askar"));
            Gumanoid? proc = null;
            if (mesh == null)
            {
                proc = Actors.Askar();
                mesh = proc.Root;
            }

            double z = -21 + (i * 6);
            double x = B.DarvozaX - 14;
            var a = new Askar
            {
                Mesh = mesh, Proc = proc, Hp = B.AskarHp, UyX = x, UyZ = z,
                X = x, Y = World.YerBalandligi(x, z), Z = z, Yaw = -Math.PI / 2, // faces west
            };
            mesh.Position = new Vector3((float)a.X, (float)a.Y, (float)a.Z);
            mesh.Rotation = new Vector3(0, (float)a.Yaw, 0);
            scene.AddChild(mesh);
            a.Anim = Anim.Ulash(mesh, "askar");
            royxat.Add(a);

            if (proc != null) // unarmed: shoulders slumped
            {
                proc.ChapQol.Rotation = new Vector3(0.12f, 0, 0);
                proc.OngQol.Rotation = new Vector3(0.12f, 0, 0);
            }
        }

        return royxat;
    }

    /// <summary>combat.js:177-207. Refused for an already-armed soldier.</summary>
    public static bool AskarniQurollantir(Askar askar, string qurol)
    {
        if (askar.Qurol != "yoq")
        {
            return false;
        }

        askar.Qurol = qurol;
        char tomon = qurol == "kamon" ? 'L' : 'R';
        BuxoroModels.Suyak? suyak = BuxoroModels.Qol(askar.Mesh, tomon);
        Node3D Tayanch() => askar.Proc != null ? (qurol == "kamon" ? askar.Proc.ChapQol : askar.Proc.OngQol) : BuxoroModels.QolTayanch(askar.Mesh, tomon);

        Node3D m;
        if (qurol == "kamon")
        {
            m = Qurollar.Kamon(); // upright on soldiers too, as on the player
            if (!BuxoroModels.SuyakkaUla(suyak, m, new Vector3(0, 0.08f, 0.06f), Vector3.Zero))
            {
                if (askar.Proc != null)
                {
                    m.Position = new Vector3(0, -0.55f, 0.10f);
                }

                Tayanch().AddChild(m);
            }
        }
        else
        {
            m = Qurollar.Qilich();
            if (!BuxoroModels.SuyakkaUla(suyak, m, new Vector3(0, 0.12f, 0.02f), new Vector3(Mathf.Pi / 2, 0, 0)))
            {
                if (askar.Proc != null)
                {
                    m.Position = new Vector3(0, -0.58f, 0.10f);
                }

                Tayanch().AddChild(m);
            }
        }

        askar.QurolMesh = m;
        if (askar.Proc != null) // leather armour goes on
        {
            askar.Proc.Tana.AddChild(Actors.Quti(0.50f, 0.40f, 0.30f, Actors.Mat(Actors.CharmZirh, 0.85f), 0, 0.36f, 0));
            askar.Proc.ChapQol.Rotation = Vector3.Zero;
            askar.Proc.OngQol.Rotation = Vector3.Zero;
        }

        return true;
    }

    /// <summary>combat.js:210-215 — an arrow from a bowman or from Shomurod.</summary>
    public static Oq OqOt(Node3D scene, double x, double z, Dushman maqsad, double zarar = 0, double y = 1.5)
    {
        MeshInstance3D o = Actors.Quti(0.03f, 0.03f, 0.7f, Actors.Unlit(0x4a4038), 0, 0, 0);
        o.Position = new Vector3((float)x, (float)y, (float)z);
        scene.AddChild(o);
        return new Oq { Mesh = o, Maqsad = maqsad, Umr = 2.2, Zarar = zarar > 0 ? zarar : B.KamonchiZarar, X = x, Y = y, Z = z };
    }

    // ================= UPDATE =================

    /// <summary>combat.js:218-424. <paramref name="oyinchiOlgan"/> is the corpse case of game.js:636.</summary>
    public static void Yangila(Node3D scene, double dt, List<Dushman> dushmanlar, List<Askar> askarlar, Dunyo dunyo,
        Oyinchi oyinchi, bool oyinchiOlgan, List<Oq> oqlar, Action<Hodisa> hodisa)
    {
        // ---- Enemies ----
        for (int i = dushmanlar.Count - 1; i >= 0; i--)
        {
            Dushman d = dushmanlar[i];

            if (d.Olgan)
            {
                d.OlimTaymer -= dt;
                if (!d.OlimKlipi) // no 'die' clip -> procedural fall
                {
                    d.Mesh.Rotation = new Vector3(Math.Min(Mathf.Pi / 2, d.Mesh.Rotation.X + (float)(dt * 4)), d.Mesh.Rotation.Y, d.Mesh.Rotation.Z);
                    d.Y = Math.Max(0.2, d.Y - (dt * 0.6));
                    d.Mesh.Position = new Vector3((float)d.X, (float)d.Y, (float)d.Z);
                }

                if (d.OlimTaymer <= 0)
                {
                    d.Mesh.QueueFree();
                    d.OtMesh?.QueueFree();
                    dushmanlar.RemoveAt(i);
                }

                continue;
            }

            d.HujumTaymer -= dt;

            // 1) Is a wall in the way? (a coordinate box, not a raycast)
            DevorNuqta? devor = null;
            if (d.X < B.DevorChizigiX + 2.5)
            {
                foreach (DevorNuqta n in dunyo.DevorNuqtalari)
                {
                    if (n.Qurilgan && Math.Abs(n.Z - d.Z) < 6.6 && d.X > n.X - 4 && d.X < n.X + 2.5)
                    {
                        devor = n;
                    }
                }
            }

            // Riders go round the wall (§9)
            if (devor != null && d.Tur == "otliq")
            {
                int chetga = d.Z > 0 ? 1 : -1;
                d.Z += chetga * d.Spec.Tezlik * dt * 0.85;
                d.X += d.Spec.Tezlik * dt * 0.25;
                if (Math.Abs(d.Z) > 44)
                {
                    d.Z = 44 * chetga;
                }

                devor = null;
            }

            if (devor != null)
            {
                // strike the wall — each living section holds them 6 s
                if (d.HujumTaymer <= 0)
                {
                    d.HujumTaymer = d.Spec.Hujum;
                    devor.Hp -= B.DevorSeksiyaHp / B.DevorUshlabTurish * d.Spec.Hujum;
                    BuxoroAudio.S("zarba");
                    if (devor.Hp <= 0)
                    {
                        DevorBuzildi(scene, devor, dunyo);
                        hodisa(new Hodisa("devorBuzildi", Devor: devor));
                    }
                }

                SyncD(d);
                HarakatAnim(d, dt, false);
                continue;
            }

            // 2) The nearest target: the player or a soldier
            object? maqsad = null;
            double mx0 = 0, mz0 = 0, engM = double.PositiveInfinity;
            void Tekshir(object obj, double px, double pz)
            {
                double dx = px - d.X, dz = pz - d.Z, m = (dx * dx) + (dz * dz);
                if (m < engM)
                {
                    engM = m;
                    maqsad = obj;
                    mx0 = px;
                    mz0 = pz;
                }
            }

            if (!oyinchiOlgan)
            {
                Tekshir(oyinchi, oyinchi.X, oyinchi.Z);
            }

            foreach (Askar a in askarlar)
            {
                if (!a.Olgan)
                {
                    Tekshir(a, a.X, a.Z);
                }
            }

            double hujumMasofasi = d.Tur == "noyon" ? 2.8 : 2.2;
            if (maqsad != null && engM < hujumMasofasi * hujumMasofasi)
            {
                if (d.HujumTaymer <= 0)
                {
                    d.HujumTaymer = d.Spec.Hujum;
                    hodisa(new Hodisa("dushmanUrdi", Dushman: d, Maqsad: maqsad));
                }

                SyncD(d);
                HarakatAnim(d, dt, false);
                continue;
            }

            // 3) Toward the target, or the gate
            double mx, mz;
            if (maqsad != null && engM < 40 * 40)
            {
                mx = mx0;
                mz = mz0;
            }
            else
            {
                mx = B.DarvozaX;
                mz = 0;
            }

            double ddx = mx - d.X, ddz = mz - d.Z;
            double u = Math.Sqrt((ddx * ddx) + (ddz * ddz));
            if (u == 0)
            {
                u = 1;
            }

            d.X += ddx / u * d.Spec.Tezlik * dt;
            d.Z += ddz / u * d.Spec.Tezlik * dt;
            if (d.OtMesh == null)
            {
                d.Y = World.YerBalandligi(d.X, d.Z);
            }

            d.Yaw = Player.BurchakLerp(d.Yaw, Math.Atan2(ddx, ddz), Smoothing.ActorYaw(dt));
            if (d.OtMesh != null)
            {
                double yer = World.YerBalandligi(d.X, d.Z);
                double old = d.Chavandoz ? BuxoroModels.EgarOrqa : 0;
                d.OtMesh.Position = new Vector3((float)(d.X + (Math.Sin(d.Yaw) * old)), (float)yer, (float)(d.Z + (Math.Cos(d.Yaw) * old)));
                d.OtMesh.Rotation = new Vector3(0, (float)(d.Yaw + BuxoroModels.OtBurilish()), 0);
                d.Y = yer + (d.Chavandoz ? BuxoroModels.EgarY : 1.42);
            }

            SyncD(d);
            HarakatAnim(d, dt, true);
        }

        // ---- Soldiers ----
        foreach (Askar a in askarlar)
        {
            if (a.Olgan)
            {
                continue;
            }

            a.OtishTaymer -= dt;
            a.HujumTaymer -= dt;
            Anim.QulfYangila(a.Anim, dt);
            if (a.Anim != null && a.Qurol == "yoq")
            {
                Anim.Oynat(a.Anim, "idle_unarmed");
            }

            if (a.Qurol == "kamon")
            {
                // stands its ground, shoots from 25 m
                Dushman? eng = EngYaqin(dushmanlar, a, B.KamonchiMasofa * B.KamonchiMasofa);
                if (eng != null)
                {
                    a.Yaw = Math.Atan2(eng.X - a.X, eng.Z - a.Z);
                    if (a.OtishTaymer <= 0)
                    {
                        a.OtishTaymer = B.KamonchiOtish;
                        Anim.Oynat(a.Anim, "bow_shoot", new AnimOpt { BirMarta = true, Qayta = true });
                        oqlar.Add(OqOt(scene, a.X, a.Z, eng));
                        BuxoroAudio.S("kamon");
                    }
                }
                else
                {
                    Anim.Oynat(a.Anim, "idle_armed");
                }
            }
            else if (a.Qurol == "qilich")
            {
                // runs at the enemy — across the whole field, and never comes home
                Dushman? eng = EngYaqin(dushmanlar, a, double.PositiveInfinity);
                double mx = eng != null ? eng.X : a.UyX;
                double mz = eng != null ? eng.Z : a.UyZ;
                double dx = mx - a.X, dz = mz - a.Z;
                double u = Math.Sqrt((dx * dx) + (dz * dz));
                if (u == 0)
                {
                    u = 1;
                }

                if (u > 2.0)
                {
                    a.X += dx / u * 5.2 * dt;
                    a.Z += dz / u * 5.2 * dt;
                    a.Y = World.YerBalandligi(a.X, a.Z);

                    // soldiers have no 'charge_run' — 'run'/'walk', matched to speed
                    if (!Anim.Yurish(a.Anim, new[] { "charge_run", "run", "walk" }, 5.2, tartib: false) && a.Proc == null)
                    {
                        Anim.ProtseduralYurish(a.Yurish, a.Mesh, true, dt, "yurish", a.Y);
                    }

                    a.Faza += dt * 10;
                    if (a.Proc != null)
                    {
                        float s = (float)(Math.Sin(a.Faza) * 0.5);
                        a.Proc.ChapOyoq.Rotation = new Vector3(s, 0, 0);
                        a.Proc.OngOyoq.Rotation = new Vector3(-s, 0, 0);
                    }
                }
                else if (eng != null && a.HujumTaymer <= 0)
                {
                    a.HujumTaymer = B.QilichbozHujum;
                    Anim.Oynat(a.Anim, "sword_attack", new AnimOpt { BirMarta = true, Qayta = true });
                    ZararBer(eng, B.QilichbozZarar, hodisa);
                    BuxoroAudio.S("qilich");
                }

                a.Yaw = Player.BurchakLerp(a.Yaw, Math.Atan2(dx, dz), 1 - Math.Pow(0.001, dt));
            }
            else
            {
                // Unarmed: does not move, but resists an enemy beside it. Damage 3 —
                // almost nothing, deliberately; it never runs AT an enemy.
                Dushman? yaqin = EngYaqin(dushmanlar, a, B.QurolsizMasofa * B.QurolsizMasofa);
                if (yaqin != null)
                {
                    a.Yaw = Math.Atan2(yaqin.X - a.X, yaqin.Z - a.Z);
                    if (a.HujumTaymer <= 0)
                    {
                        a.HujumTaymer = B.QurolsizHujum;
                        Anim.Oynat(a.Anim, "attack", new AnimOpt { BirMarta = true, Qayta = true });
                        ZararBer(yaqin, B.QurolsizZarar, hodisa);
                        BuxoroAudio.S("kaltak");
                    }
                }
            }

            a.Mesh.Position = new Vector3((float)a.X, (float)a.Y, (float)a.Z);
            a.Mesh.Rotation = new Vector3(a.Mesh.Rotation.X, (float)a.Yaw, a.Mesh.Rotation.Z);
        }

        // ---- Arrows ----
        for (int i = oqlar.Count - 1; i >= 0; i--)
        {
            Oq oq = oqlar[i];
            oq.Umr -= dt;
            Dushman? m = oq.Maqsad;
            if (oq.Umr <= 0 || m == null || m.Olgan)
            {
                oq.Mesh.QueueFree();
                oqlar.RemoveAt(i);
                continue;
            }

            double dx = m.X - oq.X, dz = m.Z - oq.Z;
            double u = Math.Sqrt((dx * dx) + (dz * dz));
            if (u < 1.0)
            {
                ZararBer(m, oq.Zarar > 0 ? oq.Zarar : B.KamonchiZarar, hodisa);
                oq.Mesh.QueueFree();
                oqlar.RemoveAt(i);
                continue;
            }

            double v = 42 * dt;
            oq.X += dx / u * v;
            oq.Z += dz / u * v;
            oq.Mesh.Position = new Vector3((float)oq.X, (float)oq.Y, (float)oq.Z);
            oq.Mesh.Rotation = new Vector3(0, (float)Math.Atan2(dx, dz), 0);
        }
    }

    private static void SyncD(Dushman d)
    {
        d.Mesh.Position = new Vector3((float)d.X, (float)d.Y, (float)d.Z);
        d.Mesh.Rotation = new Vector3(d.Mesh.Rotation.X, (float)d.Yaw, d.Mesh.Rotation.Z);
    }

    private static Dushman? EngYaqin(List<Dushman> dushmanlar, Askar a, double engM)
    {
        Dushman? eng = null;
        foreach (Dushman d in dushmanlar)
        {
            if (d.Olgan)
            {
                continue;
            }

            double dx = d.X - a.X, dz = d.Z - a.Z, m = (dx * dx) + (dz * dz);
            if (m < engM)
            {
                engM = m;
                eng = d;
            }
        }

        return eng;
    }

    /// <summary>combat.js:426-462.</summary>
    private static void HarakatAnim(Dushman d, double dt, bool harakatda)
    {
        Anim.QulfYangila(d.Anim, dt);

        // Procedural step for a clipless enemy; the noyon walks heavier (§5).
        if (d.Proc == null && d.OtMesh == null && !Anim.Bormi(d.Anim, "charge") && !Anim.Bormi(d.Anim, "walk_menace"))
        {
            Anim.ProtseduralYurish(d.Yurish, d.Mesh, harakatda, dt, d.Tur == "noyon" ? "ogir" : "yurish", World.YerBalandligi(d.X, d.Z));
        }

        if (d.Anim != null && d.Chavandoz)
        {
            // A rider: the run clip would kick the legs — a slow walk_menace (arms and
            // torso sway), legs bent by Otir (web 2026-10-19).
            if (!Anim.Oynat(d.Anim, "horse_ride"))
            {
                Anim.Oynat(d.Anim, "walk_menace", new AnimOpt { Tezlik = 0.45 });
            }

            if (d.Anim.Joriy != null)
            {
                BuxoroModels.Otir(d.Mesh);
            }
        }
        else if (d.Anim != null)
        {
            if (harakatda)
            {
                // the clip is chosen by speed: a Mongol at 4.5 m/s wants 'charge'
                Anim.Yurish(d.Anim,
                    d.Tur == "noyon" ? new[] { "walk_heavy", "charge", "walk" }
                    : d.Tur == "otliq" ? new[] { "horse_ride", "charge", "run" }
                    : new[] { "charge", "walk_menace", "run", "walk" },
                    d.Spec.Tezlik > 0 ? d.Spec.Tezlik : 4.5, tartib: false);
            }
            else
            {
                Anim.Oynat(d.Anim, d.Tur == "noyon" ? "saber_heavy" : d.Tur == "otliq" ? "horse_bow_shoot" : "spear_thrust",
                    new AnimOpt { BirMarta = true, Qayta = true });
            }
        }

        if (d.Proc == null || d.OtMesh != null)
        {
            return;
        }

        if (harakatda)
        {
            d.Faza += dt * 9;
            float s = (float)(Math.Sin(d.Faza) * 0.5);
            d.Proc.ChapOyoq.Rotation = new Vector3(s, 0, 0);
            d.Proc.OngOyoq.Rotation = new Vector3(-s, 0, 0);
            d.Proc.ChapQol.Rotation = new Vector3(-s * 0.5f, 0, 0);
        }
        else
        {
            d.Proc.OngQol.Rotation = new Vector3(-1.4f, 0, 0);
        }
    }

    /// <summary>combat.js:464-476 — the one damage function for enemies.</summary>
    public static void ZararBer(Dushman d, double zarar, Action<Hodisa>? hodisa)
    {
        if (d.Olgan)
        {
            return;
        }

        d.Hp -= zarar;
        if (d.Hp <= 0)
        {
            d.Olgan = true;
            d.OlimTaymer = 2.5;
            d.OlimKlipi = Anim.Oynat(d.Anim, "die", new AnimOpt { BirMarta = true, Majburiy = true });
            if (!d.OlimKlipi)
            {
                Anim.Toxtat(d.Anim);
            }

            BuxoroAudio.S("olim");
            hodisa?.Invoke(new Hodisa("dushmanOldi", Dushman: d));
        }
    }
}
