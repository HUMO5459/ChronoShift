using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;

namespace ChronoShift;

/// <summary>One resource node. world.js:209-217.</summary>
public sealed class Tugun
{
    public string Tur = "";
    public double X;
    public double Z;
    public Node3D Mesh = null!;
    public int Qoldi;
    public bool Belgilangan;
    public Node3D? BelgiMesh;
    public bool Band;
}

/// <summary>One defensive wall point on x = 60. world.js:135-139.</summary>
public sealed class DevorNuqta
{
    public double X;
    public double Z;
    public bool Qurilgan;
    public bool Buzilgan;
    public double Hp;
    public Node3D? Mesh;
    public Node3D? Belgi;
    public Tosiq? Tosiq;
}

/// <summary>One bazaar craftsman. world.js:340-343.</summary>
public sealed class Hunarmand
{
    public double X;
    public double Z;
    public Node3D Mesh = null!;
    public bool Yollandi;
    public float Progress;
}

/// <summary>A placed building and its anchor. world.js:309, 352.</summary>
public sealed record Bino(Node3D Root, double X, double Z);

/// <summary>The gate and its two door leaves. world.js:495-496.</summary>
public sealed record Darvoza(Node3D Root, Node3D ChapTavaqa, Node3D OngTavaqa, double X, double Z);

/// <summary>Everything <c>WORLD.qur</c> returns. world.js:38-43.</summary>
public sealed class Dunyo
{
    public readonly List<Tugun> Tugunlar = new();
    public readonly List<Node3D> Teraklar = new();
    public readonly List<Hunarmand> Hunarmandlar = new();
    public readonly List<DevorNuqta> DevorNuqtalari = new();
    public readonly List<Tosiq> Tosiqlar = new();
    public Bino? Ustaxona;
    public Bino? Bozor;
    public Darvoza? Darvoza;
    public Node3D? HumoTayanchi;
    public MeshInstance3D? Yer;
    public DirectionalLight3D? Quyosh;
    public WorldEnvironment? Muhit;
    public List<MultiMeshInstance3D> Otlar = new();

    /// <summary>Cumulative world-stream draw index after each consumer (parity block C).</summary>
    public readonly List<(string Nom, int Draw)> DrawSanoq = new();
}

/// <summary>
/// The Buxoro field. A function-for-function mirror of world.js — open the two
/// side by side; every method below names its source lines.
/// </summary>
/// <remarks>
/// THE STREAM. One world LCG (seed 1238) is consumed in the textual order of rnd()
/// in world.js, by positions AND by procedural placeholder geometry. Draws are
/// hoisted into named locals in that order so nobody reorders them by accident.
/// With today's 18-GLB set the checkpoints are nodes 1080, poplars 1259,
/// houses 1267, gate 1474, edges 1554 (BUXORO_PORT_PLAN.md §2.4).
///
/// Grass (otlar.js, seed 4711) is step 5 and draws from its own stream, so leaving
/// it out shifts nothing here.
/// </remarks>
public sealed class World
{
    private static readonly Dictionary<string, uint> ResursRangi = new()
    {
        ["tosh"] = 0x7d7568,
        ["yogoch"] = 0x7d6039,
        ["teri"] = 0xa07a4e,
        ["temir"] = 0x6d4a35,
        ["komir"] = 0x2b2724,
    };

    private readonly BuxoroBalance _b;
    private readonly BuxoroWorld _w;
    private readonly Lcg _lcg;
    private readonly Binolar _binolar;

    /// <summary>Poplar positions in double, in creation order — the obstacle source.</summary>
    private readonly List<(double X, double Z)> _terakJoylari = new();

    public World(BuxoroBalance balance, BuxoroWorld world)
    {
        _b = balance;
        _w = world;
        _lcg = BuxoroRuntime.NewWorldLcg();
        _binolar = new Binolar(BuxoroRuntime.NewBuildingLcg());
    }

    /// <summary>The world stream, exposed for the parity checks only.</summary>
    public Lcg Lcg => _lcg;

    /// <summary>The building stream (8317), exposed for the isolation check only.</summary>
    public Lcg BinolarLcg => _binolar.Lcg;

    private double Rnd() => _lcg.Next();

    private double Orasida(double a, double b) => a + (Rnd() * (b - a));

    /// <summary>world.js:25 — pure, so the mesh and every actor agree.</summary>
    public static double YerBalandligi(double x, double z) => BuxoroTerrain.HeightAt(x, z);

    private static float Yb(double x, double z) => (float)YerBalandligi(x, z);

    /// <summary>Builds the whole field under <paramref name="scene"/>. world.js:37-164.</summary>
    public Dunyo Qur(Node3D scene)
    {
        var dunyo = new Dunyo();

        // ---- Light: overcast late afternoon (world.js:45-58) ----
        float ekspozitsiya = BuxoroEkran.Ekspozitsiya(_w.Ekspozitsiya);
        var env = new Godot.Environment
        {
            // Calibrated against the web build's own frames — see BuxoroEkran.
            BackgroundMode = Godot.Environment.BGMode.Color,
            BackgroundColor = BuxoroEkran.Teskari(_w.FonRangi, ekspozitsiya),
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = _w.OsmonRangi.LinearToSrgb(),
            AmbientLightEnergy = _w.AmbientKuchi * _w.YoruglikKoef,
            ReflectedLightSource = Godot.Environment.ReflectionSource.Disabled, // three.js has no env map
            TonemapMode = Godot.Environment.ToneMapper.Aces,
            TonemapWhite = BuxoroEkran.Oq,
            TonemapExposure = ekspozitsiya,
            FogEnabled = true,
            FogLightColor = BuxoroEkran.Teskari(_w.TumanRangi, ekspozitsiya),
            FogSkyAffect = 0f,
        };
        TumanQoy(env, _w.TumanZichligi);
        dunyo.Muhit = new WorldEnvironment { Name = "Muhit", Environment = env };
        scene.AddChild(dunyo.Muhit);

        var quyosh = new DirectionalLight3D
        {
            Name = "Quyosh",
            LightColor = _w.QuyoshRangi.LinearToSrgb(),
            LightEnergy = _w.QuyoshKuchi * _w.YoruglikKoef,
            ShadowEnabled = true,
            DirectionalShadowMode = DirectionalLight3D.ShadowMode.Orthogonal,
            DirectionalShadowMaxDistance = 260f,
        };
        // The three.js target stays at the origin, so the light shines from its
        // position toward (0,0,0).
        quyosh.Transform = new Transform3D(Basis.Identity, _w.QuyoshJoyi).LookingAt(Vector3.Zero, Vector3.Up);
        scene.AddChild(quyosh);
        dunyo.Quyosh = quyosh;

        // ---- Ground (world.js:64-83) ----
        dunyo.Yer = YerQur();
        scene.AddChild(dunyo.Yer);

        var tugunlar3d = Bola(scene, "Tugunlar");
        var teraklar3d = Bola(scene, "Teraklar");
        var binolar3d = Bola(scene, "Binolar");

        // ---- Resource nodes (world.js:85-101). Order tosh→yogoch→teri→temir→komir
        //      comes from balance_buxoro.tres and is load-bearing. ----
        foreach (BuxoroNodeCountRow row in _b.TugunSoni)
        {
            for (int i = 0; i < row.Soni; i++)
            {
                double x = Orasida(row.Zona.X, row.Zona.Y); // joylar[tur]() — X then Z
                double z = Orasida(row.Zona.Z, row.Zona.W);
                dunyo.Tugunlar.Add(TugunYarat(tugunlar3d, row, x, z));
            }
        }

        Sanoq(dunyo, "nodes");

        // ---- Poplars (world.js:103-127) ----
        for (int i = 0; i < 16; i++) // main row, north to south
        {
            double x = 129 + (Rnd() * 4);
            dunyo.Teraklar.Add(TerakYarat(teraklar3d, x, -50 + (i * 6.4)));
        }

        for (int i = 0; i < 10; i++) // second row, slightly behind — depth
        {
            double x = 137 + (Rnd() * 3);
            dunyo.Teraklar.Add(TerakYarat(teraklar3d, x, -44 + (i * 9.2)));
        }

        foreach ((double gx, double gz) in BuxoroWorldStream.Clusters) // by the channel
        {
            int n = 2 + (int)Math.Floor(Rnd() * 3);
            for (int i = 0; i < n; i++)
            {
                double x = gx + ((Rnd() - 0.5) * 7);
                double z = gz + ((Rnd() - 0.5) * 7);
                dunyo.Teraklar.Add(TerakYarat(teraklar3d, x, z));
            }
        }

        for (int i = 0; i < 6; i++) // gate road
        {
            double zs = -14 - (Rnd() * 2);
            dunyo.Teraklar.Add(TerakYarat(teraklar3d, 148 + (i * 4.5), zs));
            double zn = 14 + (Rnd() * 2);
            dunyo.Teraklar.Add(TerakYarat(teraklar3d, 148 + (i * 4.5), zn));
        }

        Sanoq(dunyo, "poplars");

        // Poplar trunks are obstacles (playtest, band 7). Taken from the generator's
        // doubles, not the float node position: ~1e-6 of float error flips the solver's
        // nearest-edge choice at tie points (measured by the solver sweep).
        foreach ((double tx, double tz) in _terakJoylari)
        {
            TosiqQosh(dunyo, Tosiq.Aylana(tx, tz, _w.TerakTanaR));
        }

        // ---- Wall points: 6 sections on x = 60 (world.js:133-140) ----
        for (int i = 0; i < _b.DevorSeksiyaSoni; i++)
        {
            dunyo.DevorNuqtalari.Add(new DevorNuqta
            {
                X = _b.DevorChizigiX,
                Z = _w.DevorNuqtaZ0 + (i * _w.DevorNuqtaQadam),
            });
        }

        // ---- Houses (world.js:142-143) ----
        UylarQoy(binolar3d, dunyo);
        Sanoq(dunyo, "houses");

        // ---- Workshop, bazaar, gate, Ark (world.js:145-155) ----
        dunyo.Ustaxona = UstaxonaYarat(binolar3d, _b.Ustaxona.X, _b.Ustaxona.Y, dunyo);
        dunyo.Bozor = BozorYarat(binolar3d, _b.Bozor.X, _b.Bozor.Y, dunyo.Hunarmandlar, dunyo);
        dunyo.Darvoza = DarvozaYarat(binolar3d, _b.DarvozaX, 0, dunyo);
        Sanoq(dunyo, "gate");
        dunyo.HumoTayanchi = ArkYarat(binolar3d);

        // ---- Grass (world.js:157-158) — InstancedMesh, its own stream 4711 ----
        dunyo.Otlar = Otlar.Qur(binolar3d);

        // ---- Edges (world.js:160-161) ----
        Chekkalar(binolar3d);
        Sanoq(dunyo, "edges");

        return dunyo;
    }

    private void Sanoq(Dunyo d, string nom) => d.DrawSanoq.Add((nom, _lcg.DrawCount));

    /// <summary>
    /// Sets the fog for a three.js FogExp2 density. Public because game.js thickens
    /// the dust as a wave approaches; GameDirector (step 8) calls this every change.
    /// </summary>
    /// <remarks>
    /// FogExp2 is 1 - exp(-(d·x)²); Godot's exponential fog is 1 - exp(-d·x) and would
    /// haze the near field twice as much. Godot's depth fog with an end of
    /// <see cref="BuxoroWorld.TumanOxiriKoef"/> / density follows the exp2 curve
    /// closely over the playable range (calibrated against web frames).
    /// </remarks>
    public void TumanQoy(Godot.Environment env, float zichlik)
    {
        env.FogMode = Godot.Environment.FogModeEnum.Depth;
        env.FogDensity = 1f;
        env.FogDepthBegin = 0f;
        env.FogDepthEnd = _w.TumanOxiriKoef / Mathf.Max(zichlik, 1e-5f);
        env.FogDepthCurve = _w.TumanEgri;
    }

    private static Node3D Bola(Node3D ota, string nom)
    {
        var n = ota.GetNodeOrNull<Node3D>(nom);
        if (n == null)
        {
            n = new Node3D { Name = nom };
            ota.AddChild(n);
        }

        return n;
    }

    /// <summary>
    /// The ground plane. world.js:64-83.
    /// </summary>
    /// <remarks>
    /// Same as yerBalandligi() — no randomness, or measured heights would not match
    /// the surface. IMPORTANT: the plane is moved to x = YER_X, so height is taken in
    /// WORLD coordinates (local x + YER_X). Local x was used once — the phase
    /// slipped ~10.8 rad and the hero sank knee-deep in places and walked on air in
    /// others (playtest). Do not "simplify" this back to local x.
    /// </remarks>
    private MeshInstance3D YerQur()
    {
        int nx = _w.YerBolak.X, nz = _w.YerBolak.Y;
        float sx = _w.YerOlcham.X, sz = _w.YerOlcham.Y;
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        for (int j = 0; j <= nz; j++)
        {
            for (int i = 0; i <= nx; i++)
            {
                float lx = (-sx / 2) + (sx * i / nx);
                float lz = (-sz / 2) + (sz * j / nz);
                double wx = lx + _w.YerX;

                // analytic normal of yerBalandligi — what computeVertexNormals approximates
                double dhdx = Math.Cos(wx * 0.09) * 0.09 * 0.16;
                double dhdz = -Math.Sin(lz * 0.11) * 0.11 * 0.14;
                st.SetNormal(new Vector3((float)-dhdx, 1f, (float)-dhdz).Normalized());
                st.SetUV(new Vector2((float)i / nx, (float)j / nz));
                st.AddVertex(new Vector3(lx, Yb(wx, lz), lz));
            }
        }

        int row = nx + 1;
        for (int j = 0; j < nz; j++)
        {
            for (int i = 0; i < nx; i++)
            {
                int a = (j * row) + i, b = a + 1, c = a + row, d = c + 1;

                // clockwise seen from above = Godot front face
                st.AddIndex(a); st.AddIndex(b); st.AddIndex(c);
                st.AddIndex(b); st.AddIndex(d); st.AddIndex(c);
            }
        }

        st.SetMaterial(Actors.Mat(ToHex(_w.YerRangi), 1.0f));
        return new MeshInstance3D
        {
            Name = "Yer",
            Mesh = st.Commit(),
            Position = new Vector3(_w.YerX, 0, 0),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
        };
    }

    private static uint ToHex(Color c) => (uint)c.ToRgba32() >> 8;

    /// <summary>world.js:166-218.</summary>
    private Tugun TugunYarat(Node3D ota, BuxoroNodeCountRow row, double x, double z)
    {
        string tur = row.Tur;
        Node3D? mesh = BuxoroModels.Ol("tugun_" + tur);
        if (mesh == null)
        {
            mesh = new Node3D { Name = "tugun_" + tur };
            var m = Actors.Mat(ResursRangi[tur], 1.0f);
            switch (tur)
            {
                case "tosh": // 3 x 6 draws
                    for (int i = 0; i < 3; i++)
                    {
                        double r = 0.26 + (Rnd() * 0.20);
                        double px = (Rnd() - 0.5) * 0.8;
                        double pz = (Rnd() - 0.5) * 0.8;
                        double rx = Rnd() * 3, ry = Rnd() * 3, rz = Rnd() * 3;
                        var s = Actors.Dodekaedr((float)r, m);
                        s.Position = new Vector3((float)px, (float)(r * 0.7), (float)pz);
                        Actors.JsRotation(s, rx, ry, rz);
                        mesh.AddChild(s);
                    }

                    break;

                case "yogoch": // 4 x 4 draws
                    for (int i = 0; i < 4; i++)
                    {
                        double ox = (Rnd() - 0.5) * 0.3;
                        double oz = (Rnd() - 0.5) * 0.3;
                        var b = Actors.Silindr(0.07f, 0.08f, 1.5f, m, (float)ox, 0.30f, (float)oz, 6);
                        double rz = 0.35 + (Rnd() * 0.3);
                        double ry = Rnd() * 3;
                        Actors.JsRotation(b, 0, ry, rz);
                        mesh.AddChild(b);
                    }

                    break;

                case "teri": // 1 draw
                {
                    var t = Actors.Quti(0.78f, 0.14f, 0.62f, m, 0, 0.09f, 0);
                    t.Rotation = new Vector3(0, (float)(Rnd() * 3), 0);
                    mesh.AddChild(t);
                    mesh.AddChild(Actors.Quti(0.62f, 0.10f, 0.50f, m, 0.06f, 0.22f, 0.05f));
                    break;
                }

                case "temir": // 5 x 7 draws
                    for (int i = 0; i < 5; i++)
                    {
                        double w = 0.10 + (Rnd() * 0.24);
                        double d = 0.09 + (Rnd() * 0.14);
                        double px = (Rnd() - 0.5) * 0.7;
                        double py = 0.05 + (Rnd() * 0.12);
                        double pz = (Rnd() - 0.5) * 0.7;
                        var p = Actors.Quti((float)w, 0.07f, (float)d, m, (float)px, (float)py, (float)pz);
                        double ry = Rnd() * 3;
                        double rz = Rnd() * 0.5;
                        Actors.JsRotation(p, 0, ry, rz);
                        mesh.AddChild(p);
                    }

                    break;

                default: // komir — 6 x 4 draws
                    for (int i = 0; i < 6; i++)
                    {
                        double r = 0.13 + (Rnd() * 0.10);
                        double px = (Rnd() - 0.5) * 0.75;
                        double py = 0.09 + (Rnd() * 0.14);
                        double pz = (Rnd() - 0.5) * 0.75;
                        var c = Actors.Dodekaedr((float)r, m);
                        c.Position = new Vector3((float)px, (float)py, (float)pz);
                        mesh.AddChild(c);
                    }

                    break;
            }
        }

        mesh.Position = new Vector3((float)x, Yb(x, z), (float)z);
        ota.AddChild(mesh);

        return new Tugun
        {
            Tur = tur, X = x, Z = z, Mesh = mesh,
            // §12 mismatch: 14 stone nodes x 1 unit < 24 needed, so stone holds 4.
            // Source world.js:215 — balance.js's comment saying 1 is wrong.
            Qoldi = row.Qoldi,
        };
    }

    /// <summary>world.js:220-242. GLB path: 2 draws. Procedural: 1.</summary>
    private Node3D TerakYarat(Node3D ota, double x, double z)
    {
        _terakJoylari.Add((x, z));
        Node3D? glb = BuxoroModels.Ol("terak");
        if (glb != null)
        {
            glb.Position = new Vector3((float)x, Yb(x, z), (float)z);
            double ry = Rnd() * Math.PI * 2;
            double sc = 0.82 + (Rnd() * 0.40);
            glb.Rotation = new Vector3(glb.Rotation.X, (float)ry, glb.Rotation.Z);
            glb.Scale *= (float)sc; // MULTIPLY — the import scale must survive
            ota.AddChild(glb);
            return glb;
        }

        double h = 9 + (Rnd() * 3.5);
        var g = new Node3D { Name = "terak" };
        g.AddChild(Actors.Silindr(0.16f, 0.28f, (float)h, Actors.Mat(0x6b5a42, 1.0f), 0, (float)(h / 2), 0, 7));
        var barg = new MeshInstance3D
        {
            Mesh = new CylinderMesh
            {
                TopRadius = 0f, BottomRadius = 1.5f, Height = (float)(h * 0.72), RadialSegments = 8, Rings = 1,
                Material = Actors.Mat(0x6e7444, 1.0f),
            },
            Position = new Vector3(0, (float)(h * 0.68), 0),
        };
        g.AddChild(barg);
        g.Position = new Vector3((float)x, Yb(x, z), (float)z);
        ota.AddChild(g);
        return g;
    }

    /// <summary>
    /// Mud-plastered houses on the east edge, burnt ruins around the iron nodes.
    /// world.js:247-271. One yaw draw per house, in table order.
    /// </summary>
    private void UylarQoy(Node3D ota, Dunyo dunyo)
    {
        if (!_w.Uylar)
        {
            return;
        }

        foreach ((string nom, double x, double z) in BuxoroWorldStream.Houses)
        {
            Node3D? m = BuxoroModels.Ol(nom);
            List<Tosiq>? lokal = null;
            if (m == null)
            {
                // binolar.js — these draw from the 8317 stream only, never from this one.
                m = nom == "uy_vayrona" ? _binolar.UyVayrona(out lokal) : _binolar.Uy(nom == "uy_katta" ? "katta" : "kichik", out lokal);
                m.Position = new Vector3((float)x, 0, (float)z);
            }
            else
            {
                m.Position = new Vector3((float)x, m.Position.Y + Yb(x, z), (float)z); // y += — keeps the foot drop
            }

            double yaw = Rnd() * Math.PI * 2;
            m.Rotation = new Vector3(m.Rotation.X, (float)yaw, m.Rotation.Z);
            ota.AddChild(m);
            BinoTosiq(dunyo, m, x, z, yaw, lokal, _w.Iz(nom));
        }
    }

    /// <summary>The workshop. world.js:273-310. No world draws on either branch.</summary>
    private Bino UstaxonaYarat(Node3D ota, double x, double z, Dunyo dunyo)
    {
        Node3D? g = BuxoroModels.Ol("ustaxona");
        List<Tosiq>? lokal = null;
        if (g == null)
        {
            g = new Node3D { Name = "ustaxona" };

            // anvil + block
            g.AddChild(Actors.Silindr(0.42f, 0.50f, 0.62f, Actors.Mat(0x5c4a35, 1.0f), 0, 0.31f, 0, 9));
            g.AddChild(Actors.Quti(0.30f, 0.26f, 0.86f, Actors.Mat(0x3d3a38, 0.45f), 0, 0.75f, 0));

            // fire pit + glowing ember disc
            g.AddChild(Actors.Silindr(0.55f, 0.75f, 0.75f, Actors.Mat(0x7a6247, 1.0f), -1.9f, 0.37f, 0.4f, 10));
            var chogh = Actors.Silindr(0.36f, 0.36f, 0.06f, Actors.Unlit(0xd4551e), -1.9f, 0.76f, 0.4f, 10);
            chogh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
            g.AddChild(chogh);

            // roof: 4 posts + reeds
            float[,] ustun = { { -2.6f, -2.2f }, { 2.6f, -2.2f }, { -2.6f, 2.2f }, { 2.6f, 2.2f } };
            for (int i = 0; i < 4; i++)
            {
                g.AddChild(Actors.Silindr(0.11f, 0.12f, 2.9f, Actors.Mat(0x6b5940, 1.0f), ustun[i, 0], 1.45f, ustun[i, 1], 6));
            }

            g.AddChild(Actors.Quti(6.0f, 0.16f, 5.2f, Actors.Mat(0x8a7a55, 1.0f), 0, 2.95f, 0));

            // ground footprint: anvil, fire, four posts (you can walk under the roof)
            lokal = new List<Tosiq>
            {
                Tosiq.Aylana(0, 0, 0.6), Tosiq.Aylana(-1.9, 0.4, 0.8),
                Tosiq.Aylana(-2.6, -2.2, 0.16), Tosiq.Aylana(2.6, -2.2, 0.16),
                Tosiq.Aylana(-2.6, 2.2, 0.16), Tosiq.Aylana(2.6, 2.2, 0.16),
            };
        }

        g.Position = new Vector3((float)x, Yb(x, z), (float)z);
        ota.AddChild(g);
        BinoTosiq(dunyo, g, x, z, g.Rotation.Y, lokal);

        // black charcoal ash — a CircleGeometry(4.2, 20) lying flat at y = 0.04
        var kul = new MeshInstance3D
        {
            Name = "kul",
            Mesh = new CylinderMesh
            {
                TopRadius = 4.2f, BottomRadius = 4.2f, Height = 0.001f, RadialSegments = 20, Rings = 1,
                CapBottom = false, Material = Actors.Mat(0x4e463c, 1.0f),
            },
            Position = new Vector3((float)x, 0.04f, (float)z),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
        };
        ota.AddChild(kul);
        return new Bino(g, x, z);
    }

    /// <summary>The bazaar: 6 stalls + 6 seated craftsmen. world.js:312-353.</summary>
    private Bino BozorYarat(Node3D ota, double x, double z, List<Hunarmand> royxat, Dunyo dunyo)
    {
        var g = new Node3D { Name = "bozor" };
        var rastaJoy = new List<(double, double)>(); // doubles, as the JS children positions are
        for (int i = 0; i < 6; i++)
        {
            double ax = ((i % 3) * 4.2) - 4.2;
            double az = (Math.Floor(i / 3.0) * 4.6) - 2.3;

            // stall
            Node3D rasta = BuxoroModels.Ol("bozor_rastasi") ?? _binolar.BozorRastasi();
            rasta.Position = new Vector3((float)ax, 0, (float)az);
            rastaJoy.Add((ax, az));
            g.AddChild(rasta);

            // craftsman — the procedural seated gumanoid fallback is step 6
            Node3D h = BuxoroModels.Ol("hunarmand") ?? new Node3D { Name = "hunarmand" };
            h.Position = new Vector3((float)(x + ax), 0.28f, (float)(z + az));
            h.Rotation = new Vector3(0, -Mathf.Pi / 2, 0);

            // Bind pose is a T-pose, so the idle clip is required (2026-09-16).
            // Full ANIM.ulash is step 6; until then play the clip directly.
            OynatIdle(h, "idle_waiting");
            ota.AddChild(h);
            royxat.Add(new Hunarmand { X = x + ax, Z = z + az, Mesh = h });
        }

        g.Position = new Vector3((float)x, 0, (float)z);
        ota.AddChild(g);

        // Stalls are obstacles — NOT the GLB bbox, the exact 2.4 x 2.0 platform:
        // the canopy is wider and closed the gaps between neighbouring stalls, so a
        // craftsman could no longer be approached within 3.2 m.
        foreach ((double ax, double az) in rastaJoy)
        {
            TosiqQosh(dunyo, Tosiq.Tortburchak(x + ax, z + az, 2.4, 2.0, 0));
        }

        return new Bino(g, x, z);
    }

    private static void OynatIdle(Node n, string klip)
    {
        var ap = BuxoroModels.Avlodlar<AnimationPlayer>(n).OfType<AnimationPlayer>().FirstOrDefault();
        if (ap != null && ap.HasAnimation(klip))
        {
            ap.Autoplay = klip;
        }
    }

    /// <summary>
    /// The CITY WALL — a RING, with the gate at its west point. world.js:355-497.
    /// </summary>
    /// <remarks>
    /// The model's length runs along X. Each ring segment must turn to the tangent,
    /// or they stand crosswise (that was the earlier bug; the gate was not turned
    /// either). Draws: 3 ruined segments x 45 + the breach heap 12 x 6 = 207.
    /// </remarks>
    private Darvoza DarvozaYarat(Node3D ota, double x, double z, Dunyo dunyo)
    {
        var g = new Node3D { Name = "darvoza_halqa" };

        // GLB, else binolar.js (neither generator draws).
        List<Tosiq>? darvozaIz = null;
        Node3D DModel() => BuxoroModels.Ol("qala_darvoza") ?? Binolar.QalaDarvoza(out darvozaIz);
        Node3D SModel() => BuxoroModels.Ol("shahar_devori") ?? Binolar.ShaharDevori();

        OlchamTekshir("qala_darvoza", _w.DarvozaEni);
        OlchamTekshir("shahar_devori", _w.DevorSegUzunlik);

        double R = _w.DevorR;
        double markazX = x + R; // the gate sits on the ring's west point

        // Tangent yaw: local X along the tangent. cos φ = -sin θ, sin φ = -cos θ.
        static double Urinma(double t) => Math.Atan2(-Math.Cos(t), -Math.Sin(t));
        void Joy(double t, Node3D ob)
        {
            double wx = markazX + (Math.Cos(t) * R);
            double wz = (Math.Sin(t) * R) + z;
            ob.Position = new Vector3((float)(wx - x), (float)(YerBalandligi(wx, wz) + _w.DarvozaYOffset), (float)(wz - z));
        }

        // --- Gate: west point (θ = π) ---
        Node3D darvoza = DModel();
        darvoza.Rotation = new Vector3(darvoza.Rotation.X, (float)Urinma(Math.PI), darvoza.Rotation.Z);
        Joy(Math.PI, darvoza);
        g.AddChild(darvoza);

        // --- Segments around the ring (frozen derivation, see BuxoroWorld) ---
        int soni = _w.DevorSoni;
        double darvozaBurchagi = _w.DarvozaBurchagi;
        double r1 = _w.ShaharDevoriR1, r2 = _w.ShaharDevoriR2;

        // world.js:380-407, 494 measures a sample of each model. For the GLBs those results
        // are frozen in BuxoroWorld; a procedural stand-in has no frozen value, so it is
        // measured as the web does — the ring follows it, and the draw total with it.
        if (!BuxoroModels.Bor("qala_darvoza"))
        {
            darvozaBurchagi = OrnakOlcham(Binolar.QalaDarvoza(out _)).X / R * 0.62;
        }

        if (!BuxoroModels.Bor("shahar_devori"))
        {
            Vector3 sO = OrnakOlcham(Binolar.ShaharDevori());
            double burchakQadam = Math.Max(4, sO.X) / R;
            soni = Math.Max(8, (int)Math.Floor((Math.PI * 2 / burchakQadam) + 0.5)); // JS Math.round
            r1 = R - (sO.Z / 2);
            r2 = R + (sO.Z / 2);
        }

        double qadam = Math.PI * 2 / soni;

        // 18 years ago the Mongols razed the city; parts of the wall are still down.
        var buzilgan = new HashSet<int>(_w.Buzilgan);
        var tuynuk = new HashSet<int>(_w.Tuynuk);

        // The ring's EAST arc is never seen: the player is always west of the wall
        // and the Ark (46 m tall, 74 m wide) hides it. Not built — saves ~150k tris.
        double korinmasYoy = Math.PI * _w.KorinmasYoyPi;

        var tosh = Actors.Mat(0x8a7f6a, 1.0f);
        for (int i = 0; i < soni; i++)
        {
            double t = i * qadam;
            double farq = Math.Abs(((t - Math.PI + Math.PI) % (Math.PI * 2)) - Math.PI);
            if (farq < darvozaBurchagi)
            {
                continue; // inside the gate's arc
            }

            double sharqFarq = Math.Abs(((t + Math.PI) % (Math.PI * 2)) - Math.PI);
            if (sharqFarq < korinmasYoy)
            {
                continue; // invisible east arc
            }

            if (tuynuk.Contains(i))
            {
                continue;
            }

            Node3D seg = SModel();
            double yaw = Urinma(t);
            Joy(t, seg);

            if (buzilgan.Contains(i))
            {
                // Collapsed: lower, tilted, sunk. MULTIPLY scale, never assign —
                // the model's own metre-scale must survive.
                double k = 0.30 + (Rnd() * 0.22);
                seg.Scale = new Vector3(seg.Scale.X, (float)(seg.Scale.Y * k), seg.Scale.Z);
                double rz = (Rnd() - 0.5) * 0.10;
                double rx = (Rnd() - 0.5) * 0.06;
                Actors.JsRotation(seg, rx, yaw, rz);
                seg.Position = new Vector3(seg.Position.X, -0.25f, seg.Position.Z);

                // rubble spilled around it
                for (int k2 = 0; k2 < 7; k2++)
                {
                    double r = 0.35 + (Rnd() * 0.55);
                    double px = seg.Position.X + ((Rnd() - 0.5) * 7);
                    double pz = seg.Position.Z + ((Rnd() - 0.5) * 7);
                    double ax = Rnd() * 3, ay = Rnd() * 3, az = Rnd() * 3;
                    var s = Actors.Dodekaedr((float)r, tosh);
                    s.Position = new Vector3((float)px, (float)(r * 0.55), (float)pz);
                    Actors.JsRotation(s, ax, ay, az);
                    g.AddChild(s);
                }
            }
            else
            {
                seg.Rotation = new Vector3(seg.Rotation.X, (float)yaw, seg.Rotation.Z);
                seg.Scale = new Vector3(seg.Scale.X, seg.Scale.Y * _w.DevorBalandlikKoef, seg.Scale.Z);
            }

            g.AddChild(seg);
        }

        // A rubble heap where the breach is. Runs whether or not the index was reachable.
        foreach (int i in _w.Tuynuk)
        {
            double t = i * qadam;
            for (int k = 0; k < 12; k++)
            {
                double r = 0.4 + (Rnd() * 0.7);
                double px = markazX + (Math.Cos(t) * R) - x + ((Rnd() - 0.5) * 10);
                double pz = (Math.Sin(t) * R) + ((Rnd() - 0.5) * 10);
                double ax = Rnd() * 3, ay = Rnd() * 3, az = Rnd() * 3;
                var s = Actors.Dodekaedr((float)r, tosh);
                s.Position = new Vector3((float)px, (float)(r * 0.5), (float)pz);
                Actors.JsRotation(s, ax, ay, az);
                g.AddChild(s);
            }
        }

        // Door leaves — they swing open on defeat.
        Node3D? chap = null, ong = null;
        foreach (Node n in BuxoroModels.Avlodlar<Node>(darvoza).Prepend(darvoza))
        {
            string nom = n.Name.ToString().ToLowerInvariant();
            if (n is Node3D n3 && Regex.IsMatch(nom, "tavaqa_l|door_l|gate_l"))
            {
                chap = n3;
            }

            if (n is Node3D n4 && Regex.IsMatch(nom, "tavaqa_r|door_r|gate_r"))
            {
                ong = n4;
            }
        }

        g.Position = new Vector3((float)x, 0, (float)z);
        ota.AddChild(g);

        // Obstacles (band 7): the gate footprint + the whole ring as one belt, so the
        // gaps between segments cannot be slipped through. The belt's radial
        // thickness is one segment's Z size.
        BinoTosiq(dunyo, darvoza, markazX + (Math.Cos(Math.PI) * R), (Math.Sin(Math.PI) * R) + z, Urinma(Math.PI), darvozaIz, _w.Iz("qala_darvoza"));
        TosiqQosh(dunyo, Tosiq.Halqa(markazX, z, r1, r2));

        return new Darvoza(g, chap ?? new Node3D(), ong ?? new Node3D(), x, z);
    }

    /// <summary><c>olcham(m)</c>, world.js:374-378 — a sample's size; the sample is freed.</summary>
    private static Vector3 OrnakOlcham(Node3D ornak)
    {
        BuxoroModels.Olcham(ornak, Transform3D.Identity, out Aabb q);
        ornak.Free();
        return q.Size;
    }

    /// <summary>
    /// Asserts the imported GLB still measures what the frozen ring constants were
    /// derived from. Report the number rather than "fixing" the constant.
    /// </summary>
    private static void OlchamTekshir(string nom, float kutilganX)
    {
        Node3D? m = BuxoroModels.Ol(nom);
        if (m == null)
        {
            return;
        }

        if (BuxoroModels.Olcham(m, Transform3D.Identity, out Aabb box) && Mathf.Abs(box.Size.X - kutilganX) > 0.05f)
        {
            GD.PushError($"[buxoro] {nom}.glb AABB x = {box.Size.X:F4}, frozen value {kutilganX:F4} (tolerance 0.05). " +
                         "The ring layout is frozen and will NOT follow the model — check the import scale.");
        }

        m.Free();
    }

    /// <summary>The Ark at ~250 m, and the perch Humo sits on. world.js:499-537.</summary>
    private Node3D ArkYarat(Node3D ota)
    {
        var tayanch = new Node3D { Name = "HumoTayanchi" };
        Node3D? glb = BuxoroModels.Ol("ark_qalasi");
        if (glb != null)
        {
            glb.Position = _w.ArkJoyi;
            ota.AddChild(glb);

            // A HUMO_TAYANCH empty in the GLB if there is one, else above the bbox.
            Node3D? nomli = null;
            foreach (Node n in BuxoroModels.Avlodlar<Node>(glb))
            {
                if (n is Node3D n3 && Regex.IsMatch(n.Name.ToString(), "humo|dome|gumbaz", RegexOptions.IgnoreCase))
                {
                    nomli = n3;
                }
            }

            if (nomli != null)
            {
                tayanch.Position = YigmaJoy(nomli, glb) + new Vector3(0, 1.5f, 0);
            }
            else if (BuxoroModels.Olcham(glb, Transform3D.Identity, out Aabb q))
            {
                tayanch.Position = new Vector3(_w.ArkJoyi.X, q.End.Y + 1.2f, _w.ArkJoyi.Z);
            }

            ota.AddChild(tayanch);
            return tayanch;
        }

        // binolar.js arkQalasi(): Humo sits on the GUMBAZ.
        Node3D g = Binolar.ArkQalasi();
        g.Position = _w.ArkJoyi;
        ota.AddChild(g);
        Node3D? gum = g.FindChild("GUMBAZ", true, false) as Node3D;
        tayanch.Position = gum != null
            ? YigmaJoy(gum, g) + new Vector3(0, 5.6f, 0)
            : new Vector3(_w.ArkJoyi.X, 52, _w.ArkJoyi.Z);
        ota.AddChild(tayanch);
        return tayanch;
    }

    /// <summary>World position of a descendant of a not-yet-ticked root.</summary>
    private static Vector3 YigmaJoy(Node3D n, Node3D root)
    {
        Transform3D t = n.Transform;
        for (Node? p = n.GetParent(); p != null && p != root.GetParent(); p = p.GetParent())
        {
            if (p is Node3D p3)
            {
                t = p3.Transform * t;
            }
        }

        return t.Origin;
    }

    /// <summary>
    /// Rammed-earth boundary on the north and south edges. world.js:539-562.
    /// 2 world draws per segment, 20 segments per line = 80. paxsa_devor.glb does
    /// not exist, so every segment — and the one built just to measure — comes from
    /// Binolar on the 8317 stream: 42 calls that never touch this stream.
    /// </summary>
    private void Chekkalar(Node3D ota)
    {
        foreach (double z in new[] { -(double)_w.ChekkaZ, _w.ChekkaZ })
        {
            // measure one segment to chain them
            Node3D nam = BuxoroModels.Ol("paxsa_devor") ?? _binolar.PaxsaDevor();
            BuxoroModels.Olcham(nam, Transform3D.Identity, out Aabb nq);
            nam.Free();
            Vector3 no = nq.Size;
            bool uzunX = no.X >= no.Z;
            double uz = Math.Max(2, uzunX ? no.X : no.Z);

            for (double x = _w.ChekkaX0; x < _w.ChekkaXMax; x += uz - _w.ChekkaUstma)
            {
                Node3D s = BuxoroModels.Ol("paxsa_devor") ?? _binolar.PaxsaDevor();
                double ry = uzunX ? 0 : Math.PI / 2; // long axis already along X → no turn
                ry += (Rnd() - 0.5) * 0.03;
                s.Rotation = new Vector3(0, (float)ry, 0);
                s.Position = new Vector3((float)x, 0, (float)(z + ((Rnd() - 0.5) * 0.25)));
                ota.AddChild(s);
            }
        }

        // West dust horizon — NO geometry. Any shell or plane gives an edge in frame
        // (the camera is always inside it). Dust is fog only; game.js thickens it as
        // a wave approaches.
    }

    // ==================== OBSTACLES (playtest, band 7) ====================
    // Ground shapes only (y ignored):
    //   circle { x, z, r } · rectangle { x, z, w, d, burilish } · ring { x, z, r1, r2 }

    /// <summary>world.js:571.</summary>
    public static Tosiq TosiqQosh(Dunyo dunyo, Tosiq t)
    {
        dunyo.Tosiqlar.Add(t);
        return t;
    }

    /// <summary>world.js:572-575.</summary>
    public static void TosiqOchir(Dunyo dunyo, Tosiq t)
    {
        dunyo.Tosiqlar.Remove(t);
    }

    /// <summary>
    /// Registers a placed object's ground footprint. world.js:581-606.
    /// </summary>
    /// <remarks>
    /// If the generator supplied a hand-authored footprint (<paramref name="lokal"/>,
    /// local coordinates, turned with the object) that wins — those are playtest
    /// fixes. Otherwise (a GLB) the bbox is measured with yaw zeroed.
    /// </remarks>
    /// <param name="wx">World x of the object, as the generator computed it.</param>
    /// <param name="wz">World z of the object.</param>
    /// <param name="bur">Its yaw in radians.</param>
    /// <remarks>
    /// Position and yaw come in as the generator's DOUBLES, never read back from the
    /// float node: ~1e-6 of float error (π/2 → 1.5707964) flips the solver's
    /// nearest-edge choice at tie points — measured by the solver sweep.
    /// </remarks>
    /// <param name="muzlatilgan">A frozen GLB footprint (w, d, centre offset), see BuxoroWorld.Iz.</param>
    public static void BinoTosiq(Dunyo dunyo, Node3D ob, double wx, double wz, double bur,
        IReadOnlyList<Tosiq>? lokal = null, (double W, double D, double Ox, double Oz)? muzlatilgan = null)
    {
        double cos = Math.Cos(bur), sin = Math.Sin(bur);
        (double, double) Dunyoga(double lx, double lz) => (wx + (lx * cos) + (lz * sin), wz - (lx * sin) + (lz * cos));

        if (lokal != null && lokal.Count > 0)
        {
            foreach (Tosiq t in lokal)
            {
                (double px, double pz) = Dunyoga(t.X, t.Z);
                if (t.Shakl == TosiqShakli.Aylana)
                {
                    TosiqQosh(dunyo, Tosiq.Aylana(px, pz, t.R));
                }
                else
                {
                    TosiqQosh(dunyo, Tosiq.Tortburchak(px, pz, t.W, t.D, bur + t.Burilish));
                }
            }

            return;
        }

        // rotation.y = 0 → measure → restore. Done on a copy of the transform.
        Node3D probe = ob;
        Vector3 rot = probe.Rotation;
        var frame = new Transform3D(
            Basis.FromEuler(new Vector3(rot.X, 0, rot.Z), probe.RotationOrder) * Basis.FromScale(probe.Scale),
            Vector3.Zero);
        if (!BuxoroModels.Olcham(probe, frame * probe.Transform.AffineInverse(), out Aabb q))
        {
            return;
        }

        Vector3 s = q.Size;
        if (s.Y < 1.0f)
        {
            // Low things (a platform, a heap) are walked over. NOTE: devor_seksiya.glb
            // is ~0.3 m, so a BUILT defensive wall is not a player obstacle — keep it so.
            return;
        }

        Vector3 c = q.GetCenter();
        if (muzlatilgan is { } iz)
        {
            // Frozen footprint (BuxoroWorld.IzQiymatlar): the measurement only guards it.
            if (Math.Abs(s.X - iz.W) > 0.05 || Math.Abs(s.Z - iz.D) > 0.05)
            {
                GD.PushError($"[buxoro] {ob.Name} footprint measures {s.X:F4} x {s.Z:F4}, frozen {iz.W:F4} x {iz.D:F4} " +
                             "(tolerance 0.05). The obstacle stays frozen — check the import scale.");
            }

            (double fx, double fz) = Dunyoga(iz.Ox, iz.Oz);
            TosiqQosh(dunyo, Tosiq.Tortburchak(fx, fz, iz.W, iz.D, bur));
            return;
        }

        (double cx, double cz) = Dunyoga(c.X, c.Z);
        TosiqQosh(dunyo, Tosiq.Tortburchak(cx, cz, s.X, s.Z, bur));
    }

    /// <summary>world.js:612-665 — see <see cref="BuxoroSolver"/>.</summary>
    public static void ToqnashuvHal(Dunyo dunyo, ref double x, ref double z, double r)
    {
        BuxoroSolver.ToqnashuvHal(dunyo.Tosiqlar, ref x, ref z, r);
    }
}
