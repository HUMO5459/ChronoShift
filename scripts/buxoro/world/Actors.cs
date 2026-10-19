using System;
using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>The rig <see cref="Actors.Gumanoid"/> returns (actors.js:121).</summary>
public sealed class Gumanoid
{
    public Node3D Root = null!;
    public Node3D ChapOyoq = null!, OngOyoq = null!, ChapQol = null!, OngQol = null!, Tana = null!, Bosh = null!;
    public double S;

    // character extras
    public MeshInstance3D? Savat;
    public uint Rang;
    public Node3D? ChapQanot, OngQanot;
}

/// <summary>
/// Procedural placeholder characters and the primitive builders — actors.js.
/// A GLB from MODELS.ol always wins; these are the fallback that actually ships
/// for the actors with no GLB today (mogul_otliq, humo, the horse fallback).
/// </summary>
/// <remarks>
/// DETERMINISM: none of these consume randomness. Every rnd() in world.js sits in
/// the CALLER's argument list, hoisted into locals in textual order.
///
/// COLOUR: three.js r128 without colour management treats a hex material colour
/// as LINEAR and sRGB-encodes the output (main.js:17), so 0x685a3e renders
/// brighter than the hex. Godot treats albedo as sRGB. <see cref="Rang"/> converts,
/// so the same hex lands on the same pixel.
/// </remarks>
public static class Actors
{
    // ---- palette, actors.js:6-28 ----
    public const uint Teri = 0xa8825c, Chopon = 0x7a5730, Belbog = 0x7a2f2a, Doppi = 0x2a2420,
        IchkiKoylak = 0xbcb094, Shim = 0x6e6455, Etik = 0x4a3728, TarobiyTun = 0x6d6455,
        Salla = 0xdcd5c2, Galvir = 0x9c7b4a, AskarTun = 0xa19274, CharmZirh = 0x7a5333,
        MogulDeel = 0x3e2f24, MogulZirh = 0x6b3f2e, MogulKal = 0x4a4038, NoyonZirh = 0x232323,
        NoyonMo = 0x1a1a1a, KozQizil = 0xc23a1e;

    /// <summary>actors.js:30-37 — six apprentice robes.</summary>
    public static readonly uint[] ShogirdRanglari = { 0x3a4a6b, 0x6b4a30, 0x8a8578, 0x5f6b40, 0x2e2c2a, 0xd8cdb2 };

    private static readonly Dictionary<(uint, float, float, bool), StandardMaterial3D> Cache = new();

    /// <summary>A three.js hex colour as Godot sees it (linear hex → sRGB value).</summary>
    public static Color Rang(uint hex)
    {
        var c = new Color(((hex >> 16) & 0xff) / 255f, ((hex >> 8) & 0xff) / 255f, (hex & 0xff) / 255f);
        return c.LinearToSrgb();
    }

    /// <summary>MeshStandardMaterial({ color, roughness, metalness }). Shared, like binolar.js's cache.</summary>
    public static StandardMaterial3D Mat(uint hex, float rough = 0.9f, float metal = 0f)
    {
        var key = (hex, rough, metal, false);
        if (!Cache.TryGetValue(key, out var m))
        {
            m = new StandardMaterial3D { AlbedoColor = Rang(hex), Roughness = rough, Metallic = metal };
            Cache[key] = m;
        }

        return m;
    }

    /// <summary>A fresh (unshared) material — for anything a caller will tint or fade later.</summary>
    public static StandardMaterial3D YangiMat(uint hex, float rough = 0.9f, float metal = 0f) =>
        new() { AlbedoColor = Rang(hex), Roughness = rough, Metallic = metal };

    /// <summary>MeshBasicMaterial — unlit.</summary>
    public static StandardMaterial3D Unlit(uint hex)
    {
        var key = (hex, 0f, 0f, true);
        if (!Cache.TryGetValue(key, out var m))
        {
            m = new StandardMaterial3D { AlbedoColor = Rang(hex), ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded };
            Cache[key] = m;
        }

        return m;
    }

    /// <summary>A box mesh at a local position. actors.js:45.</summary>
    public static MeshInstance3D Quti(float w, float h, float d, Material m, float x = 0, float y = 0, float z = 0)
    {
        return Kesim(new BoxMesh { Size = new Vector3(w, h, d), Material = m }, x, y, z);
    }

    /// <summary>CylinderGeometry(rt, rp, h, seg). actors.js:52.</summary>
    public static MeshInstance3D Silindr(float rt, float rp, float h, Material m, float x = 0, float y = 0, float z = 0, int seg = 8, bool ochiq = false)
    {
        return Kesim(new CylinderMesh
        {
            TopRadius = rt, BottomRadius = rp, Height = h, RadialSegments = seg, Rings = 1, Material = m,
            CapTop = !ochiq, CapBottom = !ochiq,
        }, x, y, z);
    }

    /// <summary>ConeGeometry(r, h, seg) — a cylinder with a zero top.</summary>
    public static MeshInstance3D Konus(float r, float h, int seg, Material m) => Silindr(0, r, h, m, 0, 0, 0, seg);

    /// <summary>
    /// TorusGeometry(radius, tube). three.js's ring lies in the XY plane (axis Z);
    /// Godot's TorusMesh lies in XZ (axis Y). The returned node carries the ring
    /// in three.js orientation, so callers apply the source's rotation unchanged.
    /// </summary>
    public static Node3D Torus(float radius, float tube, Material m, int radial = 8, int tubular = 20)
    {
        var tashqi = new Node3D();
        var t = new MeshInstance3D
        {
            Mesh = new TorusMesh
            {
                InnerRadius = radius - tube, OuterRadius = radius + tube, Rings = tubular, RingSegments = radial, Material = m,
            },
            Rotation = new Vector3(Mathf.Pi / 2, 0, 0),
        };
        tashqi.AddChild(t);
        return tashqi;
    }

    /// <summary>CircleGeometry(r, seg) — a disc in the XY plane facing +Z.</summary>
    public static MeshInstance3D Disk(float r, int seg, Material m)
    {
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        for (int i = 0; i < seg; i++)
        {
            float a0 = Mathf.Tau * i / seg, a1 = Mathf.Tau * (i + 1) / seg;
            foreach (Vector3 v in new[] { Vector3.Zero, new Vector3(r * Mathf.Cos(a1), r * Mathf.Sin(a1), 0), new Vector3(r * Mathf.Cos(a0), r * Mathf.Sin(a0), 0) })
            {
                st.SetNormal(Vector3.Back);
                st.AddVertex(v);
            }
        }

        st.SetMaterial(m);
        return new MeshInstance3D { Mesh = st.Commit() };
    }

    /// <summary>
    /// TubeGeometry(CatmullRomCurve3(points), segments, radius, radial). The curves
    /// used here lie in a plane containing X as a normal, so X is a stable frame.
    /// </summary>
    public static MeshInstance3D Tube(Vector3[] nuqtalar, int segments, float radius, int radial, Material m)
    {
        var markaz = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            markaz[i] = CatmullRom(nuqtalar, (float)i / segments);
        }

        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        for (int i = 0; i < segments; i++)
        {
            Vector3 ta = (markaz[Math.Min(i + 1, segments)] - markaz[Math.Max(i - 1, 0)]).Normalized();
            Vector3 tb = (markaz[Math.Min(i + 2, segments)] - markaz[i]).Normalized();
            for (int j = 0; j < radial; j++)
            {
                float a0 = Mathf.Tau * j / radial, a1 = Mathf.Tau * (j + 1) / radial;
                Vector3 Uzuk(Vector3 c, Vector3 t, float a)
                {
                    Vector3 n = Vector3.Right, b = t.Cross(n).Normalized();
                    return c + (((n * Mathf.Cos(a)) + (b * Mathf.Sin(a))) * radius);
                }

                Vector3 p00 = Uzuk(markaz[i], ta, a0), p01 = Uzuk(markaz[i], ta, a1);
                Vector3 p10 = Uzuk(markaz[i + 1], tb, a0), p11 = Uzuk(markaz[i + 1], tb, a1);
                st.AddVertex(p00);
                st.AddVertex(p10);
                st.AddVertex(p01);
                st.AddVertex(p01);
                st.AddVertex(p10);
                st.AddVertex(p11);
            }
        }

        st.GenerateNormals();
        st.SetMaterial(m);
        return new MeshInstance3D { Mesh = st.Commit() };
    }

    private static Vector3 CatmullRom(Vector3[] p, float t)
    {
        float f = t * (p.Length - 1);
        int i = Math.Min((int)f, p.Length - 2);
        float u = f - i;
        Vector3 p0 = p[Math.Max(i - 1, 0)], p1 = p[i], p2 = p[i + 1], p3 = p[Math.Min(i + 2, p.Length - 1)];
        return 0.5f * ((2 * p1) + ((-p0 + p2) * u) + (((2 * p0) - (5 * p1) + (4 * p2) - p3) * u * u) + ((-p0 + (3 * p1) - (3 * p2) + p3) * u * u * u));
    }

    /// <summary>
    /// Stand-in for DodecahedronGeometry(r, 0): a 6x3 low-poly sphere of the same
    /// radius. Visual only — the draws that size and place it are made by the caller.
    /// </summary>
    public static MeshInstance3D Dodekaedr(float r, Material m)
    {
        return Kesim(new SphereMesh { Radius = r, Height = r * 2f, RadialSegments = 6, Rings = 3, Material = m }, 0, 0, 0);
    }

    /// <summary>
    /// three.js composes Euler angles X→Y→Z; Godot's default is YXZ. Every node
    /// that receives a JS rotation with more than one non-zero axis must use this.
    /// </summary>
    public static void JsRotation(Node3D n, double rx, double ry, double rz)
    {
        n.RotationOrder = EulerOrder.Xyz;
        n.Rotation = new Vector3((float)rx, (float)ry, (float)rz);
    }

    /// <summary>A Group at a position — three.js's <c>new THREE.Group()</c> + position.set.</summary>
    public static Node3D Guruh(float x = 0, float y = 0, float z = 0) => new() { Position = new Vector3(x, y, z) };

    private static MeshInstance3D Kesim(Mesh mesh, float x, float y, float z)
    {
        return new MeshInstance3D
        {
            Mesh = mesh,
            Position = new Vector3(x, y, z),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.On,
        };
    }

    // ================= characters =================

    /// <summary>Options for <see cref="Gumanoid"/> — actors.js's <c>opt</c>.</summary>
    public sealed class Opt
    {
        public double Balandlik = 1.75;
        public uint Tana;
        public uint? Teri, Shim, Etik, BelbogRang, Zirh, DubulgaRang, QalpoqRang;
        public bool Belbog = true;
        public string? Bosh;
        public bool Koz;
    }

    /// <summary>The shared humanoid frame, actors.js:61-122. Origin on the ground.</summary>
    public static Gumanoid GumanoidQur(Opt opt)
    {
        float s = (float)(opt.Balandlik / 1.75);
        var root = new Node3D();

        var mTeri = Mat(opt.Teri ?? Teri, 0.85f);
        var mTana = Mat(opt.Tana);
        var mShim = Mat(opt.Shim ?? Shim);
        var mEtik = Mat(opt.Etik ?? Etik, 0.7f);

        // legs — the group pivots at the hip
        var chapOyoq = Guruh(-0.11f * s, 0.85f * s, 0);
        var ongOyoq = Guruh(0.11f * s, 0.85f * s, 0);
        foreach (Node3D oy in new[] { chapOyoq, ongOyoq })
        {
            oy.AddChild(Quti(0.15f * s, 0.52f * s, 0.16f * s, mShim, 0, -0.26f * s, 0));
            oy.AddChild(Quti(0.17f * s, 0.30f * s, 0.17f * s, mEtik, 0, -0.66f * s, 0.01f * s));
            root.AddChild(oy);
        }

        // torso
        var tana = Guruh(0, 0.85f * s, 0);
        tana.AddChild(Quti(0.46f * s, 0.60f * s, 0.26f * s, mTana, 0, 0.30f * s, 0));
        if (opt.Belbog)
        {
            tana.AddChild(Quti(0.49f * s, 0.13f * s, 0.29f * s, Mat(opt.BelbogRang ?? Belbog), 0, 0.09f * s, 0));
        }

        if (opt.Zirh is uint zirh)
        {
            tana.AddChild(Quti(0.50f * s, 0.40f * s, 0.30f * s, Mat(zirh, 0.75f), 0, 0.36f * s, 0));
        }

        root.AddChild(tana);

        // arms
        var chapQol = Guruh(-0.29f * s, 1.42f * s, 0);
        var ongQol = Guruh(0.29f * s, 1.42f * s, 0);
        foreach (Node3D q in new[] { chapQol, ongQol })
        {
            q.AddChild(Quti(0.12f * s, 0.46f * s, 0.13f * s, mTana, 0, -0.23f * s, 0));
            q.AddChild(Quti(0.115f * s, 0.20f * s, 0.125f * s, mTeri, 0, -0.55f * s, 0)); // rolled sleeve
            root.AddChild(q);
        }

        // head
        var bosh = Guruh(0, 1.50f * s, 0);
        bosh.AddChild(Quti(0.21f * s, 0.24f * s, 0.20f * s, mTeri, 0, 0.12f * s, 0));
        switch (opt.Bosh)
        {
            case "doppi":
                bosh.AddChild(Quti(0.23f * s, 0.08f * s, 0.22f * s, Mat(Doppi), 0, 0.27f * s, 0));
                break;
            case "salla":
                bosh.AddChild(Silindr(0.15f * s, 0.15f * s, 0.15f * s, Mat(Salla), 0, 0.30f * s, 0, 10));
                bosh.AddChild(Quti(0.19f * s, 0.16f * s, 0.06f * s, Mat(0xe8e2d2), 0, 0.10f * s, 0.10f * s)); // white beard
                break;
            case "dubulga":
                bosh.AddChild(Silindr(0.03f * s, 0.14f * s, 0.20f * s, Mat(opt.DubulgaRang ?? MogulKal, 0.55f), 0, 0.32f * s, 0, 10));
                break;
            case "qalpoq":
                bosh.AddChild(Silindr(0.11f * s, 0.13f * s, 0.13f * s, Mat(opt.QalpoqRang ?? 0xb8ad92), 0, 0.29f * s, 0, 8));
                break;
        }

        root.AddChild(bosh);

        if (opt.Koz)
        {
            var mKoz = Unlit(KozQizil);
            bosh.AddChild(Quti(0.035f * s, 0.022f * s, 0.02f * s, mKoz, -0.055f * s, 0.15f * s, 0.10f * s));
            bosh.AddChild(Quti(0.035f * s, 0.022f * s, 0.02f * s, mKoz, 0.055f * s, 0.15f * s, 0.10f * s));
        }

        return new Gumanoid { Root = root, ChapOyoq = chapOyoq, OngOyoq = ongOyoq, ChapQol = chapQol, OngQol = ongQol, Tana = tana, Bosh = bosh, S = s };
    }

    public static Gumanoid Shomurod() => GumanoidQur(new Opt { Tana = Chopon, Bosh = "doppi", Balandlik = 1.78 });

    /// <summary>actors.js:131-150 — with the sieve rim in his left hand.</summary>
    public static Gumanoid Tarobiy()
    {
        Gumanoid h = GumanoidQur(new Opt
        {
            Tana = TarobiyTun, Bosh = "salla", BelbogRang = 0xc9bfa4, Shim = TarobiyTun, Etik = 0x6b5a44, Balandlik = 1.70,
        });
        Node3D galvir = Torus(0.22f, 0.035f, Mat(Galvir, 0.85f), 6, 20);
        galvir.Rotation = new Vector3(0, Mathf.Pi / 2, 0);
        galvir.Position = new Vector3(-0.06f, -0.60f, 0.08f);
        var to = new StandardMaterial3D
        {
            AlbedoColor = new Color(Rang(0x7d6544), 0.55f), Roughness = 1, CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        };
        MeshInstance3D mesh = Disk(0.20f, 20, to);
        mesh.Rotation = new Vector3(0, Mathf.Pi / 2, 0);
        mesh.Position = galvir.Position;
        h.ChapQol.AddChild(galvir);
        h.ChapQol.AddChild(mesh);
        return h;
    }

    /// <summary>actors.js:152-168 — a basket held at the waist.</summary>
    public static Gumanoid Shogird(int indeks)
    {
        uint rang = ShogirdRanglari[indeks % ShogirdRanglari.Length];
        Gumanoid h = GumanoidQur(new Opt { Tana = rang, Bosh = "qalpoq", QalpoqRang = rang, BelbogRang = 0x9a8f76, Balandlik = 1.62 });
        var savatMat = new StandardMaterial3D { AlbedoColor = Rang(0xa88b5c), Roughness = 1, CullMode = BaseMaterial3D.CullModeEnum.Disabled };
        MeshInstance3D savat = Silindr(0.19f, 0.15f, 0.14f, savatMat, 0, 1.02f, 0.30f, 10, ochiq: true);
        h.Root.AddChild(savat);
        h.Savat = savat;
        h.Rang = rang;
        return h;
    }

    public static Gumanoid Askar() => GumanoidQur(new Opt { Tana = AskarTun, Bosh = "qalpoq", QalpoqRang = 0xb0a48a, Balandlik = 1.74 });

    public static Gumanoid MogulPiyoda() => GumanoidQur(new Opt
    {
        Tana = MogulDeel, Zirh = MogulZirh, Bosh = "dubulga", BelbogRang = 0x554433, Etik = 0x3a2c20, Koz = true, Balandlik = 1.72,
    });

    /// <summary>actors.js:183-192 — with the sable collar.</summary>
    public static Gumanoid Noyon()
    {
        Gumanoid h = GumanoidQur(new Opt
        {
            Tana = NoyonMo, Zirh = NoyonZirh, Bosh = "dubulga", DubulgaRang = 0x2b2b2b, BelbogRang = 0x2a2a2a, Etik = 0x262626,
            Koz = true, Balandlik = 2.05,
        });
        h.Tana.AddChild(Quti(0.58f, 0.12f, 0.36f, Mat(0x3a3128, 1.0f), 0, 0.58f, 0));
        return h;
    }

    /// <summary>actors.js:194-205.</summary>
    public static Node3D Ot(uint rang = 0x8a7355)
    {
        var g = new Node3D { Name = "ot" };
        var m = Mat(rang, 0.95f);
        g.AddChild(Quti(0.55f, 0.62f, 1.55f, m, 0, 1.05f, 0));      // body
        g.AddChild(Quti(0.34f, 0.52f, 0.40f, m, 0, 1.42f, 0.82f));  // neck
        g.AddChild(Quti(0.28f, 0.26f, 0.52f, m, 0, 1.60f, 1.10f));  // head
        foreach ((float px, float pz) in new[] { (-0.20f, 0.62f), (0.20f, 0.62f), (-0.20f, -0.58f), (0.20f, -0.58f) })
        {
            g.AddChild(Quti(0.15f, 0.78f, 0.16f, m, px, 0.39f, pz)); // legs
        }

        g.AddChild(Quti(0.40f, 0.10f, 0.46f, Mat(0x6b5334, 0.9f), 0, 1.40f, -0.10f)); // saddle
        return g;
    }

    /// <summary>
    /// actors.js:207-225. Emissive and fog-proof: it must read from 250 m — the
    /// bird is UI, the game's only wave telegraph.
    /// </summary>
    public static Gumanoid Humo()
    {
        var g = new Node3D { Name = "humo" };
        var oq = new StandardMaterial3D
        {
            AlbedoColor = Rang(0xf7f2e6), Roughness = 0.6f, EmissionEnabled = true, Emission = Rang(0xf7f2e6),
            EmissionEnergyMultiplier = 0.35f, DisableFog = true,
        };
        g.AddChild(Quti(0.30f, 0.28f, 0.62f, oq, 0, 0, 0));
        g.AddChild(Quti(0.18f, 0.18f, 0.18f, oq, 0, 0.22f, 0.32f));
        MeshInstance3D chap = Quti(0.06f, 0.14f, 0.52f, oq, -0.20f, 0.06f, 0);
        MeshInstance3D ong = Quti(0.06f, 0.14f, 0.52f, oq, 0.20f, 0.06f, 0);
        g.AddChild(chap);
        g.AddChild(ong);
        for (int i = -1; i <= 1; i++)
        {
            g.AddChild(Quti(0.05f, 0.03f, 0.55f, oq, i * 0.09f, -0.02f, -0.55f)); // three tail feathers
        }

        return new Gumanoid { Root = g, ChapQanot = chap, OngQanot = ong };
    }
}
