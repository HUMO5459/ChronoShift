using Godot;

namespace ChronoShift;

/// <summary>
/// Procedural architecture, binolar.js. Only the generator the current asset set
/// actually reaches is ported here — <see cref="PaxsaDevor"/>, because
/// paxsa_devor.glb does not exist. Every other generator (uy, uyVayrona,
/// bozorRastasi, qalaDarvoza, shaharDevori, arkQalasi) is shadowed by a GLB today
/// and lands in step 5.
/// </summary>
/// <remarks>
/// OWN STREAM. binolar.js:16 seeds its own LCG with 8317. It must never draw from
/// the world stream (1238): Chekkalar calls PaxsaDevor 42 times, and routing those
/// through the world LCG would add 504 draws and re-roll nothing visible but break
/// every parity checkpoint after it. One instance per world, like the JS module.
/// </remarks>
public sealed class Binolar
{
    // binolar.js:21-32 palette (the subset used here).
    private const uint Suvoq = 0xa5926e;
    private const uint XomGisht = 0x93805e;

    public Binolar(Lcg lcg)
    {
        Lcg = lcg;
    }

    /// <summary>The 8317 stream.</summary>
    public Lcg Lcg { get; }

    private double Ora(double a, double b) => a + (Lcg.Next() * (b - a));

    /// <summary>
    /// Battered wall block — wide at the base, narrow at the top. binolar.js:52-70.
    /// Indices are the JS ones with each triangle's winding reversed, because
    /// Godot's front faces are clockwise and three.js's counter-clockwise.
    /// </summary>
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

        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        foreach (Vector3 p in v)
        {
            st.AddVertex(p);
        }

        for (int k = 0; k < i.Length; k += 3)
        {
            st.AddIndex(i[k]);
            st.AddIndex(i[k + 2]);
            st.AddIndex(i[k + 1]);
        }

        st.GenerateNormals();
        st.SetMaterial(m);
        return new MeshInstance3D { Mesh = st.Commit() };
    }

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
}
