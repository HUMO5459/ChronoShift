using Godot;

namespace ChronoShift;

/// <summary>
/// The world.js literals that balance.js does not carry: ground, light, fog, the
/// wall ring geometry and the edge walls. Every value cites its source line.
/// </summary>
/// <remarks>
/// The ring numbers (DevorSegUzunlik, DevorSoni, DarvozaBurchagi, R1, R2) are
/// FROZEN measurements, not inputs. world.js derives them from the scaled GLB
/// bounding boxes (world.js:373-406, 493) and they bound loops that draw from the
/// world stream. Godot's importer need not reproduce three.js Box3 to the bit, so
/// World.cs uses these and only ASSERTS that the imported AABB is within 0.05 m.
/// </remarks>
[GlobalClass]
public partial class BuxoroWorld : Resource
{
    // ---- Ground (world.js:65-83) ----
    /// <summary>Plane centre x. Displacement uses WORLD x = local x + this.</summary>
    [Export] public float YerX = 120f;
    [Export] public Vector2 YerOlcham = new(680f, 520f);
    [Export] public Vector2I YerBolak = new(110, 80);
    /// <summary>Green meadow (web 2026-10-19, world.js:80); was 0x685a3e dry earth.</summary>
    [Export] public Color YerRangi = new(0x4f6a2cff);

    // ---- Light, fog, sky (world.js:46-62, main.js:17-19) ----
    [Export] public Color OsmonRangi = new(0xc6bda6ff);
    [Export] public Color ZaminRangi = new(0x3b3427ff);
    [Export] public float AmbientKuchi = 0.38f;
    [Export] public Color QuyoshRangi = new(0xefdcbbff);
    [Export] public float QuyoshKuchi = 0.85f;
    [Export] public Vector3 QuyoshJoyi = new(-96f, 40f, 26f);
    [Export] public Color TumanRangi = new(0xd4c8b0ff);

    /// <summary>
    /// three.js FogExp2 density, verbatim (world.js:61). World.TumanQoy maps it onto
    /// Godot depth fog, because Godot has no exp2 fog.
    /// </summary>
    [Export] public float TumanZichligi = 0.0075f;
    [Export] public Color FonRangi = new(0xd8ccb4ff);

    /// <summary>three.js toneMappingExposure (main.js:19). BuxoroEkran converts it for Godot.</summary>
    [Export] public float Ekspozitsiya = 0.86f;

    /// <summary>
    /// CALIBRATION, not a world.js value. Both lights are scaled by this so the
    /// Compatibility renderer lands on the web frame: measured 2026-10-19, ground at
    /// (130,18,40)→(150,0,10) reads 138,120,82 in Godot vs 138,124,93 in the browser.
    /// Measured cause: in gl_compatibility the sun alone and the ambient alone each
    /// match three.js exactly, but together they render ~2.3x brighter in linear
    /// terms than their sum. Re-measure if the renderer or Godot version changes.
    /// </summary>
    [Export] public float YoruglikKoef = 0.7f;

    /// <summary>
    /// Depth-fog end = this / FogExp2 density (220 m at 0.0075). With <see cref="TumanEgri"/> 1.5
    /// the fog fraction is 0.11 / 0.31 / 0.56 / 0.80 at 50 / 100 / 150 / 190 m against FogExp2's
    /// 0.13 / 0.43 / 0.72 / 0.87 — no single power curve has exp2's S-shape; this keeps the
    /// near field and the Ark backdrop both within 0.15 (chosen from web-frame comparisons).
    /// </summary>
    [Export] public float TumanOxiriKoef = 1.65f;

    /// <summary>Depth-fog curve exponent for the FogExp2 match.</summary>
    [Export] public float TumanEgri = 1.5f;

    // ---- Houses (world.js:17, 247-271) ----
    /// <summary>?uysiz=1 sets this false: removes 8 houses AND shifts 8 draws.</summary>
    [Export] public bool Uylar = true;

    // ---- Poplars (world.js:130) ----
    [Export] public double TerakTanaR = 0.4;

    // ---- Wall points (world.js:134-139) ----
    [Export] public float DevorNuqtaZ0 = -33f;
    [Export] public float DevorNuqtaQadam = 13.2f;

    // ---- City wall ring (world.js:363-497) ----
    [Export] public float DevorR = 46f;
    [Export] public float DevorBalandlikKoef = 1.30f;
    [Export] public float DarvozaYOffset = -0.15f;

    /// <summary>Measured from shahar_devori.glb AABB after MOLJAL scaling; frozen so Godot's importer cannot re-derive a different field.</summary>
    [Export] public float DevorSegUzunlik = 12.1253f;
    [Export] public int DevorSoni = 24;

    /// <summary>(dO.x / R) · 0.62 with dO.x = 16.3299 (qala_darvoza.glb). Frozen.</summary>
    [Export] public float DarvozaBurchagi = 0.2200985f;
    [Export] public float DarvozaEni = 16.3299f;

    /// <summary>
    /// R ∓ sO.z / 2, world.js:493 — the largest collider in the level. Frozen at full
    /// double precision from the web build (sO.z = 7.93955559…), because the solver
    /// is diffed against it to 1e-4.
    /// </summary>
    [Export] public double ShaharDevoriR1 = 42.030222201381484;
    [Export] public double ShaharDevoriR2 = 49.969777798618516;

    /// <summary>The east arc is never built: ±0.30π around θ = 0 (world.js:416).</summary>
    [Export] public float KorinmasYoyPi = 0.30f;

    /// <summary>
    /// Ruined segment indices, verbatim. Index 3 is skipped by KorinmasYoy before it
    /// is tested — dead, kept verbatim.
    /// </summary>
    [Export] public int[] Buzilgan = { 3, 4, 11, 17 };
    [Export] public int[] Tuynuk = { 12 };

    // ---- Building footprints (world.js:597-605, the binoTosiq bbox path) ----
    /// <summary>Models whose obstacle footprint is frozen, in the order of <see cref="IzQiymatlar"/>.</summary>
    [Export] public string[] IzModellar = { "uy_katta", "uy_kichik", "uy_vayrona", "qala_darvoza" };

    /// <summary>
    /// Four doubles per model: footprint width (local X), depth (local Z), and the bbox
    /// centre's offset from the object origin (local X, Z, yaw zeroed). Measured from the
    /// web build's <c>O.dunyo.tosiqlar</c> (tools/parity/tosiqlar_web.csv), identical to
    /// 1e-14 across every instance of a model. FROZEN for the same reason as the ring:
    /// Godot's importer lands ~3e-5 m off three.js Box3, and a point grazing a house
    /// corner amplifies that 50x (measured: (144,-36) moved 1.4 mm). World.cs asserts the
    /// imported AABB is within 0.05 m of these.
    /// </summary>
    [Export] public double[] IzQiymatlar =
    {
        7.91592247084697, 8.05580362365716, -0.000125117310191285, -0.00237722889391494,
        7.34840470516281, 7.35230684262696, -0.00275350092794367, -0.000458916821339574,
        3.4416008614501, 3.4477389037823, -0.000376884422109585, 0.00226130653266456,
        16.3298661833419, 13.1987808930415, 0, 0,
    };

    /// <summary>The frozen footprint of <paramref name="model"/>, or null when it is measured.</summary>
    public (double W, double D, double Ox, double Oz)? Iz(string model)
    {
        int i = System.Array.IndexOf(IzModellar, model);
        if (i < 0 || (i * 4) + 3 >= IzQiymatlar.Length)
        {
            return null;
        }

        return (IzQiymatlar[i * 4], IzQiymatlar[(i * 4) + 1], IzQiymatlar[(i * 4) + 2], IzQiymatlar[(i * 4) + 3]);
    }

    // ---- Ark (world.js:499-537) ----
    [Export] public Vector3 ArkJoyi = new(250f, 0f, 10f);

    // ---- Edge walls (world.js:539-558) ----
    [Export] public float ChekkaX0 = -4f;
    [Export] public float ChekkaXMax = 192f;
    [Export] public float ChekkaUstma = 0.15f;
    [Export] public float ChekkaZ = 50f;
}
