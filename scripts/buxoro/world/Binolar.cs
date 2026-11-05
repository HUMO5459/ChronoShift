using System;
using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// Procedural architecture, binolar.js — all seven generators, each the fallback
/// World.cs uses when that building's GLB is missing. Children are added in the
/// source's order (tools/parity/binolar_web.js diffs them mesh by mesh).
/// </summary>
/// <remarks>
/// With today's 18 GLBs only <see cref="PaxsaDevor"/> runs; the others stand behind
/// the same null test as the web's <c>MODELS.ol(...) || BINOLAR.x()</c>, so they do
/// not exist as far as the stream is concerned.
///
/// OWN STREAM. binolar.js:16 seeds its own LCG with 8317. It must never draw from
/// the world stream (1238): Chekkalar calls PaxsaDevor 42 times, and routing those
/// through the world LCG would add 504 draws and re-roll nothing visible but break
/// every parity checkpoint after it. One instance per world, like the JS module.
/// Draws per call: kichik uy 17, katta uy 31, vayrona 124, rasta 5, paxsa 12;
/// qalaDarvoza, shaharDevori and arkQalasi draw nothing, so those three are static.
/// Draws are hoisted into named locals in the source's textual order.
///
/// The web keeps a footprint in <c>g.userData.tosiqlar</c>; here the generator hands
/// it back through <c>out</c> for World.BinoTosiq. bozorRastasi has one too, but
/// nothing reads it (bozorYarat registers the platform itself, world.js:351).
/// </remarks>
public sealed class Binolar
{
    // binolar.js:21-32 palette.
    private const uint Gisht = 0x9a8562;      // fired brick — dusty grey, NOT red
    private const uint GishtOch = 0xa89272;   // for the course difference
    private const uint GishtQuyuq = 0x8a7654;
    private const uint Suvoq = 0xa5926e;      // mud plaster
    private const uint SuvoqOch = 0xb2a07c;
    private const uint XomGisht = 0x93805e;   // where the plaster fell off
    private const uint Yogoch = 0x6b5940;
    private const uint YogochQuyuq = 0x53442f;
    private const uint Kuyik = 0x3a332a;
    private const uint Qamish = 0x8f7f5a;

    private const double Pi = Math.PI;

    public Binolar(Lcg lcg)
    {
        Lcg = lcg;
    }

    /// <summary>The 8317 stream.</summary>
    public Lcg Lcg { get; }

    private double Ora(double a, double b) => a + (Lcg.Next() * (b - a));

    /// <summary>binolar.js:35-41 — roughness defaults to 1.0; metalness 0.</summary>
    private static StandardMaterial3D Mat(uint rang, float rough = 1.0f) => Actors.Mat(rang, rough);

    /// <summary>JS <c>Math.round</c>: halves go up, not to even.</summary>
    private static int Yaxlit(double x) => (int)Math.Floor(x + 0.5);

    /// <summary>binolar.js:43-48. Sizes stay double until here, as in the JS.</summary>
    private static MeshInstance3D Quti(double w, double h, double d, Material m, double x, double y, double z) =>
        Actors.Quti((float)w, (float)h, (float)d, m, (float)x, (float)y, (float)z);

    /// <summary>
    /// Battered wall block — wide at the base, narrow at the top. binolar.js:52-70.
    /// Indices are the JS ones with each triangle's winding reversed, because
    /// Godot's front faces are clockwise and three.js's counter-clockwise.
    /// </summary>
    /// <remarks>
    /// The JS index list winds this block INSIDE OUT: its front faces and normals point
    /// inward, so from outside you see the far walls. That is what the web renders, so
    /// it is kept. Normals are computeVertexNormals', weighted by triangle area: Godot's
    /// GenerateNormals weighs every triangle alike, which lights a long thin block differently.
    /// </remarks>
    public static MeshInstance3D QiyaBlok(float pastW, float tepaW, float h, float pastD, float tepaD, Material m)
    {
        float pw = pastW / 2, tw = tepaW / 2, pd = pastD / 2, td = tepaD / 2;
        Vector3[] v =
        {
            new(-pw, 0, -pd), new(pw, 0, -pd), new(pw, 0, pd), new(-pw, 0, pd),
            new(-tw, h, -td), new(tw, h, -td), new(tw, h, td), new(-tw, h, td),
        };
        int[] i =
        {
            0, 1, 5, 0, 5, 4, 1, 2, 6, 1, 6, 5, 2, 3, 7, 2, 7, 6, 3, 0, 4, 3, 4, 7,
            4, 5, 6, 4, 6, 7, 3, 2, 1, 3, 1, 0,
        };

        var n = new Vector3[v.Length];
        for (int k = 0; k < i.Length; k += 3)
        {
            Vector3 yuz = (v[i[k + 2]] - v[i[k + 1]]).Cross(v[i[k]] - v[i[k + 1]]);
            n[i[k]] += yuz;
            n[i[k + 1]] += yuz;
            n[i[k + 2]] += yuz;
        }

        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        for (int k = 0; k < v.Length; k++)
        {
            st.SetNormal(n[k].Normalized());
            st.AddVertex(v[k]);
        }

        for (int k = 0; k < i.Length; k += 3)
        {
            st.AddIndex(i[k]);
            st.AddIndex(i[k + 2]);
            st.AddIndex(i[k + 1]);
        }

        st.SetMaterial(m);
        return new MeshInstance3D { Mesh = st.Commit() };
    }

    /// <summary><see cref="QiyaBlok(float,float,float,float,float,Material)"/> with the JS's double arguments.</summary>
    private static MeshInstance3D QiyaBlok(double pastW, double tepaW, double h, double pastD, double tepaD, Material m) =>
        QiyaBlok((float)pastW, (float)tepaW, (float)h, (float)pastD, (float)tepaD, m);

    /// <summary>
    /// <c>new CylinderGeometry(rt, rp, h, seg, 1, false, boshBurchak, yoy)</c> — a slice
    /// of a cylinder, which Godot's CylinderMesh cannot make. Same vertices as
    /// three.js (x = r·sinθ, z = r·cosθ, capped but open along the cut), winding reversed.
    /// </summary>
    public static MeshInstance3D KesikSilindr(float rt, float rp, float h, int seg, float boshBurchak, float yoy, Material m)
    {
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        float yarim = h / 2, qiyalik = (rp - rt) / h;

        Vector3 Halqa(float r, float y, int x)
        {
            float th = (yoy * x / seg) + boshBurchak;
            return new Vector3(r * Mathf.Sin(th), y, r * Mathf.Cos(th));
        }

        // the wall: two rows of seg + 1 vertices
        int qator = seg + 1;
        foreach ((float r, float y) in new[] { (rt, yarim), (rp, -yarim) })
        {
            for (int x = 0; x <= seg; x++)
            {
                float th = (yoy * x / seg) + boshBurchak;
                st.SetNormal(new Vector3(Mathf.Sin(th), qiyalik, Mathf.Cos(th)).Normalized());
                st.AddVertex(Halqa(r, y, x));
            }
        }

        void Uchburchak(int a, int b, int c)
        {
            st.AddIndex(a);
            st.AddIndex(b);
            st.AddIndex(c);
        }

        for (int x = 0; x < seg; x++)
        {
            int a = x, b = qator + x, c = qator + x + 1, d = x + 1;
            Uchburchak(a, d, b); // JS: a, b, d
            Uchburchak(b, d, c); // JS: b, c, d
        }

        // the caps: a fan around the axis
        int indeks = 2 * qator;
        foreach ((float r, float belgi) in new[] { (rt, 1f), (rp, -1f) })
        {
            st.SetNormal(new Vector3(0, belgi, 0));
            st.AddVertex(new Vector3(0, yarim * belgi, 0));
            int markaz = indeks++;
            int boshi = indeks;
            for (int x = 0; x <= seg; x++)
            {
                st.AddVertex(Halqa(r, yarim * belgi, x));
                indeks++;
            }

            for (int x = 0; x < seg; x++)
            {
                if (belgi > 0)
                {
                    Uchburchak(boshi + x + 1, boshi + x, markaz); // JS: i, i + 1, c
                }
                else
                {
                    Uchburchak(boshi + x, boshi + x + 1, markaz); // JS: i + 1, i, c
                }
            }
        }

        st.SetMaterial(m);
        return new MeshInstance3D { Mesh = st.Commit() };
    }

    /// <summary>
    /// <c>new SphereGeometry(r, wSeg, hSeg, 0, 2π, 0, yoy)</c> — the cap of a sphere,
    /// open at its rim. Same vertices as three.js, winding reversed.
    /// </summary>
    public static MeshInstance3D KesikShar(float r, int wSeg, int hSeg, float yoy, Material m)
    {
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        int qator = wSeg + 1;
        for (int iy = 0; iy <= hSeg; iy++)
        {
            float th = yoy * iy / hSeg;
            for (int ix = 0; ix <= wSeg; ix++)
            {
                float phi = Mathf.Tau * ix / wSeg;
                var p = new Vector3(-r * Mathf.Cos(phi) * Mathf.Sin(th), r * Mathf.Cos(th), r * Mathf.Sin(phi) * Mathf.Sin(th));
                st.SetNormal(p.Normalized());
                st.AddVertex(p);
            }
        }

        for (int iy = 0; iy < hSeg; iy++)
        {
            for (int ix = 0; ix < wSeg; ix++)
            {
                int a = (iy * qator) + ix + 1, b = (iy * qator) + ix, c = ((iy + 1) * qator) + ix, d = ((iy + 1) * qator) + ix + 1;
                if (iy != 0)
                {
                    st.AddIndex(a); // JS: a, b, d
                    st.AddIndex(d);
                    st.AddIndex(b);
                }

                st.AddIndex(b); // JS: b, c, d
                st.AddIndex(d);
                st.AddIndex(c);
            }
        }

        st.SetMaterial(m);
        return new MeshInstance3D { Mesh = st.Commit() };
    }

    /// <summary>
    /// binolar.js:77-83 — one structural course. Only ONE: a 25 cm brick course is
    /// nearly invisible at play distance, and a proud one makes the building read as a
    /// modern shelved facade. This band is historical and helps the silhouette.
    /// </summary>
    private static void GishtQatorlari(Node3D ota, double kenglik, double balandlik, double chuqurlik)
    {
        double y = balandlik * 0.64;
        double n = y / balandlik;
        ota.AddChild(Quti((kenglik * (1 - (n * 0.12))) + 0.02, 0.26, (chuqurlik * (1 - (n * 0.12))) + 0.02, Mat(GishtOch), 0, y, 0));
    }

    /// <summary>binolar.js:86-93 — crenellation: flat, square, plain.</summary>
    private static void Kungura(Node3D ota, double uzunlik, double y, double chuqurlik, Material m)
    {
        int soni = Math.Max(2, Yaxlit(uzunlik / 1.9));
        double kengligi = uzunlik / ((soni * 2) - 1);
        for (int i = 0; i < soni; i++)
        {
            double x = (-uzunlik / 2) + (kengligi / 2) + (i * kengligi * 2);
            ota.AddChild(Quti(kengligi, 0.95, chuqurlik, m, x, y + 0.475, 0));
        }
    }

    // ================= THE CITY GATE =================

    /// <summary>binolar.js:96-160. The door leaves are named: World.DarvozaYarat opens them on defeat.</summary>
    public static Node3D QalaDarvoza(out List<Tosiq> iz)
    {
        var g = new Node3D { Name = "qala_darvoza" };
        StandardMaterial3D m = Mat(Gisht);

        // two battered towers
        foreach (double z in new[] { -4.6, 4.6 })
        {
            var min = new Node3D();
            min.AddChild(QiyaBlok(6.2, 5.0, 11.0, 6.2, 5.0, m));
            GishtQatorlari(min, 6.2, 11, 6.2);

            // parapet
            var p = new Node3D();
            Kungura(p, 5.0, 0, 5.0, m);
            p.Position = new Vector3(0, 11, 0);
            min.AddChild(p);

            // arrow slits
            for (int i = 0; i < 3; i++)
            {
                min.AddChild(Quti(0.26, 1.1, 0.2, Mat(Kuyik), -2.7, 5 + (i * 2), -1.4 + (i * 1.4)));
            }

            min.Position = new Vector3(0, 0, (float)z);
            g.AddChild(min);
        }

        // the block between the towers
        var orta = new Node3D();
        orta.AddChild(QiyaBlok(5.0, 4.4, 8.6, 6.6, 6.2, m));
        GishtQatorlari(orta, 5.0, 8.6, 6.6);
        g.AddChild(orta);

        // the fighting platform and its parapet
        var may = new Node3D();
        Kungura(may, 6.6, 0, 4.4, m);
        may.Rotation = new Vector3(0, (float)(Pi / 2), 0);
        may.Position = new Vector3(0, 8.6f, 0);
        g.AddChild(may);

        // the arch recess
        var oyiq = new Node3D { Position = new Vector3(-1.2f, 0, 0) };
        MeshInstance3D rav = KesikSilindr(2.0f, 2.0f, 2.2f, 12, 0, Mathf.Pi, Mat(GishtQuyuq));
        rav.Rotation = new Vector3(0, 0, (float)(-Pi / 2));
        rav.Position = new Vector3(0, 4.2f, 0);
        oyiq.AddChild(rav);
        oyiq.AddChild(Quti(2.2, 4.2, 4.0, Mat(GishtQuyuq), 0, 2.1, 0));
        g.AddChild(oyiq);

        // door leaves — named, the code opens them
        StandardMaterial3D mY = Mat(YogochQuyuq, 0.95f);
        foreach ((string nom, double z) in new[] { ("TAVAQA_L", -1.85), ("TAVAQA_R", 1.85) })
        {
            var t = new Node3D { Name = nom };
            for (int i = 0; i < 6; i++)
            {
                t.AddChild(Quti(0.22, 5.6, 0.58, mY, 0, 2.8, -1.55 + (i * 0.62)));
            }

            // iron straps
            foreach (double y in new[] { 1.4, 3.4 })
            {
                t.AddChild(Quti(0.26, 0.18, 3.5, Mat(0x4a443c, 0.6f), 0.02, y, 0));
            }

            t.Position = new Vector3(-0.4f, 0, (float)z);
            g.AddChild(t);
        }

        // ground footprint: two towers + the middle block
        iz = new List<Tosiq> { Tosiq.Tortburchak(0, 0, 6.6, 15.4, 0) };
        return g;
    }

    // ================= CITY WALL SEGMENT =================

    /// <summary>binolar.js:163-198.</summary>
    public static Node3D ShaharDevori()
    {
        var g = new Node3D { Name = "shahar_devori" };
        StandardMaterial3D m = Mat(Gisht);

        const double UZ = 11, BAL = 8.5;
        g.AddChild(QiyaBlok(2.9, 2.2, BAL, UZ, UZ, m));

        // decorative band — flush with the surface, only the colour differs
        double ly = BAL * 0.66, ln = ly / BAL;
        g.AddChild(Quti((2.9 * (1 - (ln * 0.24))) + 0.02, 0.26, UZ + 0.01, Mat(GishtOch), 0, ly, 0));

        // half-round bastion — plain in the reference picture
        MeshInstance3D b = KesikSilindr(1.35f, 1.75f, (float)BAL, 10, (float)(-Pi / 2), Mathf.Pi, m);
        b.Position = new Vector3(-1.4f, (float)(BAL / 2), 0);
        b.Rotation = new Vector3(0, Mathf.Pi, 0);
        g.AddChild(b);

        // parapet
        var p = new Node3D();
        Kungura(p, UZ, 0, 2.2, m);
        p.Rotation = new Vector3(0, (float)(Pi / 2), 0);
        p.Position = new Vector3(0, (float)BAL, 0);
        g.AddChild(p);

        // arrow slits
        foreach (double z in new[] { -3.0, 3.0 })
        {
            g.AddChild(Quti(0.24, 1.0, 0.18, Mat(Kuyik), -1.35, 5.4, z));
        }

        // spilled earth at the footing
        g.AddChild(Quti(4.2, 0.5, UZ, Mat(XomGisht), 0, 0.25, 0));

        return g;
    }

    // ================= THE ARK =================

    /// <summary>
    /// binolar.js:201-265. The reference picture: the Ark is not a towered building but
    /// a raised MOUND, so the platform is low and very wide and the buttresses are
    /// shallow half-cylinders sunk into the face. Humo lands on the GUMBAZ node.
    /// </summary>
    public static Node3D ArkQalasi()
    {
        var g = new Node3D { Name = "ark_qalasi" };
        StandardMaterial3D m = Mat(Gisht);

        const double P = 30, PW = 74, PD = 62;
        g.AddChild(QiyaBlok(PW, PW * 0.72, P, PD, PD * 0.72, m));

        // few, WIDE and LOW buttresses: many thin ones read as a modern ribbed building
        StandardMaterial3D bMat = Mat(GishtQuyuq);
        void Kontrfors(double x, double z, double burilish)
        {
            MeshInstance3D b = KesikSilindr(3.4f, 5.0f, (float)(P * 0.72), 10, (float)(-Pi / 2), Mathf.Pi, bMat);
            b.Position = new Vector3((float)x, (float)(P * 0.36), (float)z);
            b.Rotation = new Vector3(0, (float)burilish, 0);
            g.AddChild(b);
        }

        for (int i = 0; i < 3; i++)
        {
            double t = (-PD / 2) + (PD * (i + 0.5) / 3);
            Kontrfors(-PW / 2 * 0.90, t * 0.90, Pi);
            Kontrfors(PW / 2 * 0.90, t * 0.90, 0);
        }

        for (int i = 0; i < 3; i++)
        {
            double t = (-PW / 2) + (PW * (i + 0.5) / 3);
            Kontrfors(t * 0.90, -PD / 2 * 0.90, -Pi / 2);
            Kontrfors(t * 0.90, PD / 2 * 0.90, Pi / 2);
        }

        // the top edge finishes the platform
        g.AddChild(Quti(PW * 0.74, 0.9, PD * 0.74, Mat(GishtOch), 0, P, 0));

        // the cluster of blocks on top
        StandardMaterial3D uMat = Mat(Suvoq);
        double[,] bloklar = { { 18, 6, 15, -12, 5 }, { 13, 4.5, 12, 9, -9 }, { 10, 7.5, 10, 7, 11 }, { 14, 3.5, 9, -16, -13 }, { 9, 5, 11, 18, 7 } };
        for (int i = 0; i < bloklar.GetLength(0); i++)
        {
            double h = bloklar[i, 1];
            g.AddChild(Quti(bloklar[i, 0], h, bloklar[i, 2], uMat, bloklar[i, 3], P + (h / 2), bloklar[i, 4]));
        }

        // the low dome — Humo lands here
        var gum = new Node3D { Name = "GUMBAZ" };
        gum.AddChild(Quti(9, 3.2, 9, uMat, 0, 1.6, 0));
        MeshInstance3D q = KesikShar(5.2f, 16, 8, (float)(Pi * 0.42), Mat(SuvoqOch));
        q.Position = new Vector3(0, 3.2f, 0);
        gum.AddChild(q);
        gum.Position = new Vector3(-2, (float)(P + 0.9), 7);
        g.AddChild(gum);

        // the ramp up
        MeshInstance3D r = Quti(8, 2.4, 34, m, PW / 2 * 0.55, P / 2, (PD / 2) + 10);
        r.Rotation = new Vector3((float)-Math.Atan2(P, 34), 0, 0);
        g.AddChild(r);

        return g;
    }

    // ================= MUD-PLASTERED HOUSE =================

    /// <summary>binolar.js:269-353. <paramref name="tur"/>: "kichik" (17 draws) or "katta" (31).</summary>
    public Node3D Uy(string tur, out List<Tosiq> iz)
    {
        bool katta = tur == "katta";
        var g = new Node3D { Name = katta ? "uy_katta" : "uy_kichik" };
        StandardMaterial3D mS = Mat(Lcg.Next() > 0.5 ? Suvoq : SuvoqOch);
        StandardMaterial3D mY = Mat(Yogoch, 0.95f);

        Node3D Blok(double w, double h, double d, double x, double z)
        {
            var b = new Node3D();

            // walls leaning slightly inward
            b.AddChild(QiyaBlok(w, w * 0.96, h, d, d * 0.96, mS));

            // beam ends out of the roof — the reference picture's main detail
            int soni = Math.Max(3, Yaxlit(d / 0.8));
            for (int i = 0; i < soni; i++)
            {
                double tz = (-d / 2) + (d * (i + 0.5) / soni);
                MeshInstance3D t = Actors.Silindr(0.09f, 0.09f, 0.42f, mY, (float)((w / 2 * 0.96) + 0.18), (float)(h - 0.28), (float)tz, 6);
                t.Rotation = new Vector3(0, 0, (float)(Pi / 2));
                b.AddChild(t);
            }

            // roof lip
            b.AddChild(Quti(w * 0.99, 0.28, d * 0.99, mS, 0, h + 0.14, 0));

            // where the plaster fell off: 4 draws each
            for (int i = 0; i < 3; i++)
            {
                double s = Ora(0.5, 1.3);
                double sd = s * Ora(0.6, 1.2);
                double sy = Ora(0.5, h - 0.8);
                double sz = Ora((-d / 2) + 0.6, (d / 2) - 0.6);
                b.AddChild(Quti(0.07, s, sd, Mat(XomGisht), w / 2 * 0.96, sy, sz));
            }

            // the dark damp line at the footing
            b.AddChild(Quti(w * 1.01, 0.4, d * 1.01, Mat(XomGisht), 0, 0.2, 0));
            b.Position = new Vector3((float)x, 0, (float)z);
            return b;
        }

        double h1 = katta ? 4.6 : 3.2;
        double w1 = katta ? 6.5 : 5.0, d1 = katta ? 6.0 : 5.0;
        g.AddChild(Blok(w1, h1, d1, 0, 0));

        // door
        g.AddChild(Quti(0.24, 1.7, 0.95, Mat(Kuyik), w1 / 2 * 0.96, 0.85, 0.4));
        g.AddChild(Quti(0.34, 0.22, 1.25, mY, w1 / 2 * 0.96, 1.78, 0.4)); // lintel

        // windows: 2 draws each
        for (int i = 0; i < (katta ? 3 : 2); i++)
        {
            double wy = h1 - Ora(0.7, 1.2);
            double wz = Ora((-d1 / 2) + 0.8, (d1 / 2) - 0.8);
            g.AddChild(Quti(0.2, 0.42, 0.42, Mat(Kuyik), w1 / 2 * 0.96, wy, wz));
        }

        // the stair to the roof
        var zina = new Node3D();
        for (int i = 0; i < 5; i++)
        {
            zina.AddChild(Quti(1.0, 0.26, 0.42, mS, 0, 0.13 + (i * (h1 / 5)), (-d1 / 2) + 0.3 + (i * 0.42)));
        }

        zina.Position = new Vector3((float)((-w1 / 2) + 0.5), 0, 0);
        g.AddChild(zina);

        if (katta)
        {
            // a second low block — the L shape
            g.AddChild(Blok(5.0, 3.0, 4.4, (-w1 / 2) - 2.5, (d1 / 2) - 2.2));

            // the yard wall
            MeshInstance3D hovli = QiyaBlok(0.42, 0.36, 1.8, 6.0, 5.8, mS);
            hovli.Position = new Vector3((float)((-w1 / 2) - 5.2), 0, -1.5f);
            g.AddChild(hovli);

            // the porch: two posts + a roof
            foreach (double z in new[] { -1.4, 1.4 })
            {
                g.AddChild(Actors.Silindr(0.13f, 0.15f, 2.5f, mY, (float)((w1 / 2) + 1.6), 1.25f, (float)z, 6));
            }

            g.AddChild(Quti(3.6, 0.22, 3.6, Mat(Qamish), (w1 / 2) + 0.9, 2.6, 0));
        }

        // ground footprint
        iz = new List<Tosiq>
        {
            Tosiq.Tortburchak(0, 0, w1, d1, 0),                               // main block
            Tosiq.Tortburchak((-w1 / 2) + 0.5, (-d1 / 2) + 1.15, 1.0, 2.1, 0), // stair
        };
        if (katta)
        {
            iz.Add(Tosiq.Tortburchak((-w1 / 2) - 2.5, (d1 / 2) - 2.2, 5.0, 4.4, 0)); // the L block
            iz.Add(Tosiq.Tortburchak((-w1 / 2) - 5.2, -1.5, 0.42, 6.0, 0));          // yard wall
            iz.Add(Tosiq.Aylana((w1 / 2) + 1.6, -1.4, 0.2));                         // porch posts
            iz.Add(Tosiq.Aylana((w1 / 2) + 1.6, 1.4, 0.2));
        }

        return g;
    }

    // ================= BURNT HOUSE RUIN =================

    /// <summary>binolar.js:356-409. 124 draws.</summary>
    public Node3D UyVayrona(out List<Tosiq> iz)
    {
        var g = new Node3D { Name = "uy_vayrona" };
        StandardMaterial3D mS = Mat(XomGisht);
        const double W = 5.2, D = 4.6;

        // three standing wall stumps, each its own height
        double[] balandliklar = { 2.4, 1.3, 0.8 };
        (double X, double Z)[] joylar = { (0, -D / 2), (W / 2, 0), (0, D / 2) };
        for (int i = 0; i < joylar.Length; i++)
        {
            double h = balandliklar[i];
            double uzun = i == 1 ? D : W;
            var b = new Node3D();
            b.AddChild(QiyaBlok(0.5, 0.42, h, uzun, uzun, mS));

            // the eroded uneven top
            int n = Math.Max(2, Yaxlit(uzun / 0.9));
            for (int k = 0; k < n; k++)
            {
                double kh = Ora(0.05, 0.32);
                b.AddChild(Quti(0.46, kh, uzun / n * 0.92, mS, 0, h + (kh / 2), (-uzun / 2) + (uzun * (k + 0.5) / n)));
            }

            b.Position = new Vector3((float)joylar[i].X, 0, (float)joylar[i].Z);
            if (i == 1)
            {
                b.Rotation = new Vector3(0, (float)(Pi / 2), 0);
            }

            g.AddChild(b);
        }

        // the fallen fourth wall — a spilled heap: 7 draws each
        for (int i = 0; i < 14; i++)
        {
            double s = Ora(0.22, 0.55);
            double sd = s * Ora(0.7, 1.3);
            double sx = (-W / 2) - Ora(0, 1.4);
            double sz = Ora(-D / 2, D / 2);
            MeshInstance3D b = Quti(s, s * 0.45, sd, mS, sx, s * 0.22, sz);
            double rx = Ora(0, 0.4), ry = Ora(0, 3), rz = Ora(0, 0.3);
            Actors.JsRotation(b, rx, ry, rz);
            g.AddChild(b);
        }

        // soot marks
        g.AddChild(Quti(0.53, 1.2, 1.6, Mat(Kuyik), (W / 2) + 0.02, 0.7, 0.4));

        // charred beam stumps: 3 draws each
        for (int i = 0; i < 3; i++)
        {
            double uz = Ora(0.7, 1.3);
            double rz = (Pi / 2) - Ora(0.2, 0.6);
            double ty = Ora(1.3, 2.1);
            MeshInstance3D t = Actors.Silindr(0.08f, 0.1f, (float)uz, Mat(Kuyik, 0.95f), (float)((W / 2) - 0.3), (float)ty, (float)(-1.2 + (i * 1.2)), 5);
            t.Rotation = new Vector3(0, 0, (float)rz);
            g.AddChild(t);
        }

        // ground footprint: the three standing stumps (the heap is walked over)
        iz = new List<Tosiq>
        {
            Tosiq.Tortburchak(0, -D / 2, 0.5, W, 0),
            Tosiq.Tortburchak(W / 2, 0, 0.5, D, Pi / 2),
            Tosiq.Tortburchak(0, D / 2, 0.5, W, 0),
        };
        return g;
    }

    // ================= RAMMED-EARTH WALL SEGMENT =================

    /// <summary>
    /// One rammed-earth boundary segment, 10 m long along local Z, 2.1 m tall.
    /// binolar.js:412-438. Consumes 12 draws of the 8317 stream per call.
    /// </summary>
    public Node3D PaxsaDevor()
    {
        var g = new Node3D { Name = "PaxsaDevor" };
        var mS = Actors.Mat(Suvoq, 1.0f);
        var mX = Actors.Mat(XomGisht, 1.0f);
        const float UZ = 10f, BAL = 2.1f;

        // three layers, each slightly narrower
        float[,] qatlam = { { 0.52f, 0.70f, 0.00f }, { 0.46f, 0.70f, 0.70f }, { 0.40f, 0.70f, 1.40f } };
        for (int q = 0; q < 3; q++)
        {
            float qal = qatlam[q, 0], h = qatlam[q, 1], y = qatlam[q, 2];
            var blok = QiyaBlok(qal, qal * 0.94f, h, UZ, UZ, mS);
            blok.Position = new Vector3(0, y, 0);
            g.AddChild(blok);
            if (y > 0)
            {
                g.AddChild(Actors.Quti(qal + 0.04f, 0.05f, UZ, mX, 0, y, 0)); // layer seam
            }
        }

        // eroded crest
        const int n = 11;
        for (int i = 0; i < n; i++)
        {
            float h = (float)Ora(0.04, 0.22);
            g.AddChild(Actors.Quti(0.40f, h, UZ / n * 0.94f, mS, 0, BAL + (h / 2), (-UZ / 2) + (UZ * (i + 0.5f) / n)));
        }

        // breach in the middle
        float bz = (float)Ora(-1.5, 1.5);
        g.AddChild(Actors.Quti(0.44f, 0.34f, 1.1f, mX, 0, BAL - 0.17f, bz));

        // spilled earth at the foot
        g.AddChild(Actors.Quti(1.3f, 0.28f, UZ, mX, 0, 0.14f, 0));

        return g;
    }

    /// <summary>Size of <see cref="PaxsaDevor"/>'s bounding box: X 1.3 (footing), Z 10.</summary>
    public static Vector3 PaxsaDevorOlcham => new(1.3f, 2.1f, 10f);

    // ================= BAZAAR STALL =================

    /// <summary>binolar.js:441-459. 5 draws.</summary>
    public Node3D BozorRastasi()
    {
        var g = new Node3D { Name = "bozor_rastasi" };
        StandardMaterial3D mS = Mat(Suvoq);
        StandardMaterial3D mY = Mat(Yogoch, 0.95f);

        g.AddChild(Quti(2.4, 0.3, 2.0, mS, 0, 0.15, 0));

        // four posts + a reed awning
        foreach ((float x, float z) in new[] { (-1.0f, -0.8f), (1.0f, -0.8f), (-1.0f, 0.8f), (1.0f, 0.8f) })
        {
            g.AddChild(Actors.Silindr(0.06f, 0.07f, 2.0f, mY, x, 1.0f, z, 5));
        }

        for (int i = 0; i < 5; i++)
        {
            double y = 2.02 + Ora(0, 0.04);
            g.AddChild(Quti(2.5, 0.05, 0.3, Mat(Qamish), 0, y, -0.9 + (i * 0.45)));
        }

        return g;
    }
}
