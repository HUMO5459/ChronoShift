using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace ChronoShift;

/// <summary>Touch joystick input (touch.js:57-59). Null on keyboard.</summary>
public sealed class Analog
{
    public double X, Z, Kuch;
}

/// <summary>
/// Shomurod — the object player.js <c>yarat()</c> returns (player.js:36-59).
/// Field names mirror the source so player.js and Player.cs read side by side.
/// </summary>
public sealed class Oyinchi
{
    public Node3D Mesh = null!;
    public Gumanoid? Proc;
    public BuxoroModels.Suyak? OngElka, OngTirsak, ChapElka;
    public bool Olgan;
    public AnimCtrl? Anim;

    // Weapons and shield — attached by Qurollar (step 10).
    public Node3D? Kaltak, Qalqon, QilichMesh, KamonMesh;

    /// <summary>p.poz — doubles; the mesh follows. Y is ground + jump (player.js:315).</summary>
    public double X, Y, Z;

    public double Hp;
    public double Yonalish = -Math.PI / 2;        // camera yaw — east, toward the Ark
    public double HarakatYonalishi = Math.PI / 2; // body heading while moving
    public double Pitch = -0.38;                  // read by nothing: the camera ignores it

    public string Qurol = "kaltak";
    public readonly Dictionary<string, bool> Qurollar = new() { ["kaltak"] = true, ["kamon"] = false, ["qilich"] = false };
    public double UrishFaza;
    public string UrishTuri = "yaqin";
    public bool QilichBor;
    public bool Otda;
    public Node3D? Ot;
    public double UrishCooldown;
    public bool Bloklaymi;
    public double YurishFaza;
    public double QadamTimer;
    public double Balandlik, Vy;
    public bool Sakramoqda;
    public Analog? Analog;

    // player.js kirish()
    public Tugmalar Tugmalar = new();
    public bool EBosildi;
    public double EUshlandi;
    public double FUshlandi;
    public bool SpaceBosildi;
    public bool Urmoqchi;
    public bool Harakatda;
}

/// <summary>
/// Shomurod's movement, jump and camera — player.js. Stamina none, hunger none:
/// the only thing on screen is time (player.js:1-2).
/// </summary>
/// <remarks>
/// REFUSED: CharacterBody3D.MoveAndSlide and any physics collision. Movement is
/// the web's direct position integration; obstacles are World.ToqnashuvHal, in
/// the exact order move → clamp → solve → clamp → snap Y (player.js:233-316).
/// </remarks>
public static class Player
{
    private static BuxoroBalance B => BuxoroRuntime.B;

    /// <summary>player.js:7-93.</summary>
    public static Oyinchi Yarat(Node3D scene)
    {
        Node3D? mesh = BuxoroModels.Ol("shomurod");
        Gumanoid? proc = null;
        if (mesh == null)
        {
            proc = Actors.Shomurod();
            mesh = proc.Root;
        }

        var p = new Oyinchi
        {
            Mesh = mesh,
            Proc = proc,
            Anim = Anim.Ulash(mesh, "shomurod"),
            X = B.ShomurodBoshi.X,
            Z = B.ShomurodBoshi.Y,
            Hp = B.ShomurodHp,
        };
        mesh.Position = new Vector3((float)p.X, 0, (float)p.Z);
        scene.AddChild(mesh);

        /* Weapon attachment. To the bone if there is one (its scale cancelled), else
           to an estimated hand. On the bone: grip in the hand, blade forward. */
        BuxoroModels.Suyak? suyakR = BuxoroModels.Qol(mesh, 'R');
        BuxoroModels.Suyak? suyakL = BuxoroModels.Qol(mesh, 'L');
        if (proc == null && (suyakR == null || suyakL == null))
        {
            GD.PushError("[buxoro] shomurod: hand bone not found — weapons fall back to the estimated hand anchor");
        }

        Node3D Tayanch(char tomon) => proc != null ? (tomon == 'R' ? proc.OngQol : proc.ChapQol) : BuxoroModels.QolTayanch(mesh, tomon);

        p.Kaltak = Qurollar.Kaltak();
        if (!BuxoroModels.SuyakkaUla(suyakR, p.Kaltak, new Vector3(0, 0.10f, 0.02f), new Vector3(Mathf.Pi / 2, 0, 0)))
        {
            if (proc != null)
            {
                p.Kaltak.Position = new Vector3(0, -0.62f, 0.10f);
            }

            Tayanch('R').AddChild(p.Kaltak);
        }

        p.Qalqon = Qurollar.Qalqon();
        p.Qalqon.Visible = false;
        if (!BuxoroModels.SuyakkaUla(suyakL, p.Qalqon, new Vector3(0, 0.06f, 0.10f), Vector3.Zero))
        {
            if (proc != null)
            {
                p.Qalqon.Position = new Vector3(-0.05f, -0.55f, 0.16f);
            }

            Tayanch('L').AddChild(p.Qalqon);
        }

        // the sword, ready (it shows in Act III)
        p.QilichMesh = Qurollar.Qilich();
        p.QilichMesh.Visible = false;
        if (!BuxoroModels.SuyakkaUla(suyakR, p.QilichMesh, new Vector3(0, 0.12f, 0.02f), new Vector3(Mathf.Pi / 2, 0, 0)))
        {
            if (proc != null)
            {
                p.QilichMesh.Position = new Vector3(0, -0.60f, 0.10f);
            }

            Tayanch('R').AddChild(p.QilichMesh);
        }

        // the bow — left hand (the right draws the string), UPRIGHT; given in Act II
        p.KamonMesh = Qurollar.Kamon();
        p.KamonMesh.Visible = false;
        if (!BuxoroModels.SuyakkaUla(suyakL, p.KamonMesh, new Vector3(0, 0.08f, 0.06f), Vector3.Zero))
        {
            if (proc != null)
            {
                p.KamonMesh.Position = new Vector3(0, -0.55f, 0.10f);
            }

            Tayanch('L').AddChild(p.KamonMesh);
        }

        /* Strike bones. The GLB has no attack clip — an angle is ADDED on top of the
           pose the mixer wrote (the player updates after the animation advance). */
        p.OngElka = BuxoroModels.SuyakTop(mesh, new Regex("(^|[:_.])(r_?upperarm|rightarm|upper_?arm_?r|arm_?r)$", RegexOptions.IgnoreCase));
        p.OngTirsak = BuxoroModels.SuyakTop(mesh, new Regex("(^|[:_.])(r_?forearm|rightforearm|lower_?arm_?r)$", RegexOptions.IgnoreCase));
        p.ChapElka = BuxoroModels.SuyakTop(mesh, new Regex("(^|[:_.])(l_?upperarm|leftarm|upper_?arm_?l|arm_?l)$", RegexOptions.IgnoreCase));
        return p;
    }

    // ---- STRIKE — player.js:472-538 ----

    /// <summary>
    /// player.js:480-511. Three weapons, three fights: club near/weak/quick, sword
    /// near/strong, bow far (32 m) and slow. Only enemies IN FRONT are hit — a wide
    /// ±70° cone, no falloff, no per-target cap.
    /// </summary>
    public static (string Tur, double Zarar, List<Dushman> Urilgan, Dushman? Maqsad)? Urish(Oyinchi p, List<Dushman> dushmanlar)
    {
        if (p.UrishCooldown > 0)
        {
            return null;
        }

        if (p.Qurol == "kamon")
        {
            return Oting(p, dushmanlar);
        }

        bool qilich = p.Qurol == "qilich";
        p.UrishCooldown = qilich ? B.QilichCooldown : B.KaltakCooldown;
        double radius = qilich ? B.QilichRadius : B.KaltakRadius;
        double zarar = qilich ? B.QilichZarar : B.KaltakZarar;
        BuxoroAudio.S(qilich ? "qilich" : "kaltak");

        p.UrishFaza = 1;
        p.UrishTuri = "yaqin";
        Anim.Oynat(p.Anim, qilich ? "attack_sword" : "attack_club", new AnimOpt { BirMarta = true, Majburiy = true, Qayta = true, Fade = 0.06 });

        if (p.Proc != null)
        {
            p.Proc.OngQol.Rotation = new Vector3(-2.1f, 0, 0);
            Gumanoid proc = p.Proc;
            p.Mesh.GetTree().CreateTimer(0.16).Timeout += () =>
            {
                if (GodotObject.IsInstanceValid(proc.OngQol))
                {
                    proc.OngQol.Rotation = Vector3.Zero;
                }
            };
        }

        var urilgan = new List<Dushman>();
        double yon = p.Mesh.Rotation.Y; // the BODY yaw — it lags the camera at 14/s
        foreach (Dushman d in dushmanlar)
        {
            if (d.Olgan)
            {
                continue;
            }

            double dx = d.X - p.X, dz = d.Z - p.Z;
            if ((dx * dx) + (dz * dz) >= radius * radius)
            {
                continue;
            }

            if (Math.Abs(BurchakFarq(Math.Atan2(dx, dz), yon)) < 1.22)
            {
                urilgan.Add(d);
            }
        }

        return ("yaqin", zarar, urilgan, null);
    }

    /// <summary>player.js:514-532 — the nearest enemy in the facing cone; may be none.</summary>
    private static (string, double, List<Dushman>, Dushman?) Oting(Oyinchi p, List<Dushman> dushmanlar)
    {
        p.UrishCooldown = B.KamonCooldown;
        p.UrishFaza = 1;
        p.UrishTuri = "kamon";
        BuxoroAudio.S("kaltak");
        Anim.Oynat(p.Anim, "bow_shoot", new AnimOpt { BirMarta = true, Majburiy = true, Qayta = true, Fade = 0.06 });

        double yon = p.Mesh.Rotation.Y;
        Dushman? maqsad = null;
        double engM = B.KamonMasofa * B.KamonMasofa;
        foreach (Dushman d in dushmanlar)
        {
            if (d.Olgan)
            {
                continue;
            }

            double dx = d.X - p.X, dz = d.Z - p.Z, m = (dx * dx) + (dz * dz);
            if (m >= engM || Math.Abs(BurchakFarq(Math.Atan2(dx, dz), yon)) > B.KamonKonus)
            {
                continue;
            }

            engM = m;
            maqsad = d;
        }

        return ("oq", B.KamonZarar, new List<Dushman>(), maqsad);
    }

    /// <summary>
    /// player.js:365-417 — STRIKE POSE. The GLB has no attack clip, so an angle is
    /// added on top of the pose the mixer wrote this frame: three.js's
    /// <c>rotation.x += a</c> on an XYZ Euler is a pre-multiplied Rx(a) on the bone.
    /// club/sword: arm raised back (0 → 0.3), then struck forward.
    /// bow: left arm forward, right arm draws the string.
    /// </summary>
    private static void ZarbaPozasi(Oyinchi p, double dt)
    {
        if (p.OngElka == null && p.ChapElka == null)
        {
            return;
        }

        if (p.UrishFaza <= 0)
        {
            return;
        }

        double uzunlik = p.UrishTuri == "kamon" ? 0.45 : 0.34;
        p.UrishFaza -= dt / uzunlik;
        if (p.UrishFaza < 0)
        {
            p.UrishFaza = 0;
        }

        double t = 1 - p.UrishFaza; // 0 → 1

        void Qoy(BuxoroModels.Suyak? s, double burchak)
        {
            if (s is not { } b)
            {
                return;
            }

            Quaternion q = b.Skelet.GetBonePoseRotation(b.Idx);
            b.Skelet.SetBonePoseRotation(b.Idx, new Quaternion(Vector3.Right, (float)burchak) * q);
        }

        if (p.UrishTuri == "kamon")
        {
            double tort = t < 0.55 ? t / 0.55 : 1 - ((t - 0.55) / 0.45); // draw, then release
            Qoy(p.ChapElka, -1.15 * Math.Min(1, t * 4));
            Qoy(p.OngElka, -0.85 * tort);
            Qoy(p.OngTirsak, -1.05 * tort);
        }
        else
        {
            double burchak = t < 0.3 ? -2.0 * (t / 0.3) : -2.0 + (2.7 * ((t - 0.3) / 0.7)); // back, then forward
            Qoy(p.OngElka, burchak);
            Qoy(p.OngTirsak, burchak * 0.35);
        }
    }

    // ---- weapons: give and switch, player.js:541-573 ----

    private static void QurolKorsat(Oyinchi p)
    {
        if (p.Kaltak != null)
        {
            p.Kaltak.Visible = p.Qurol == "kaltak";
        }

        if (p.QilichMesh != null)
        {
            p.QilichMesh.Visible = p.Qurol == "qilich";
        }

        if (p.KamonMesh != null)
        {
            p.KamonMesh.Visible = p.Qurol == "kamon";
        }

        if (p.Qurol == "kamon") // the bow needs both hands — no shield with it
        {
            p.Bloklaymi = false;
            if (p.Qalqon != null)
            {
                p.Qalqon.Visible = false;
            }
        }

        BuxoroHud.Qurol(p.Qurol);
    }

    public static bool QurolTanla(Oyinchi p, string nom)
    {
        if (!p.Qurollar[nom] || p.Qurol == nom)
        {
            return false;
        }

        p.Qurol = nom;
        QurolKorsat(p);
        return true;
    }

    public static void QilichBer(Oyinchi p)
    {
        p.QilichBor = true;
        p.Qurollar["qilich"] = true;
        p.Qurol = "qilich";
        QurolKorsat(p);
    }

    public static void KamonBer(Oyinchi p)
    {
        p.Qurollar["kamon"] = true;
        p.Qurol = "kamon";
        QurolKorsat(p);
    }

    public static void OtMin(Oyinchi p, Node3D otMesh)
    {
        p.Otda = true;
        p.Ot = otMesh;
    }

    /// <summary>player.js:206-355.</summary>
    public static void Yangila(Oyinchi p, double dt, Dunyo? dunyo)
    {
        Tugmalar t = p.Tugmalar;

        // movement vector, relative to the camera
        double ix = 0, iz = 0;
        if (t.Oldinga)
        {
            iz += 1;
        }

        if (t.Orqaga)
        {
            iz -= 1;
        }

        if (t.Chapga)
        {
            ix -= 1;
        }

        if (t.Ongga)
        {
            ix += 1;
        }

        // touch joystick outranks the keyboard
        if (p.Analog != null && (p.Analog.X != 0 || p.Analog.Z != 0))
        {
            ix = p.Analog.X;
            iz = p.Analog.Z;
        }

        bool yuguradi = t.Yugur || (p.Analog != null && p.Analog.Kuch > 0.75);
        double tezlik = p.Otda ? B.OtTezligi : (yuguradi ? B.YugurishTezligi : B.YurishTezligi);

        double uz = Math.Abs(ix) + Math.Abs(iz);
        if (uz > 0)
        {
            double n = Math.Sqrt((ix * ix) + (iz * iz));
            double analogKoef = (p.Analog != null && p.Analog.Kuch > 0) ? Math.Max(0.35, Math.Min(1, p.Analog.Kuch)) : 1;
            double sin = Math.Sin(p.Yonalish), cos = Math.Cos(p.Yonalish);

            // forward = where the camera looks
            double dx = (ix / n * cos) - (iz / n * sin);
            double dz = (ix / n * -sin) - (iz / n * cos);
            p.X += dx * tezlik * analogKoef * dt;
            p.Z += dz * tezlik * analogKoef * dt;

            p.Harakatda = true;
            p.HarakatYonalishi = Math.Atan2(dx, dz); // S turns him round to face it

            // The key decides the clip: Shift shows run, otherwise walk (anim.js).
            if (p.Otda)
            {
                Anim.Oynat(p.Anim, "ride_run");
            }
            else
            {
                Anim.Yurish(p.Anim, yuguradi ? new[] { "run", "walk" } : new[] { "walk", "run" }, tezlik);
            }

            // procedural gait (a model with no clip)
            p.YurishFaza += dt * tezlik * 1.5;
            if (p.Proc != null && !p.Otda)
            {
                float a = (float)(Math.Sin(p.YurishFaza) * 0.5);
                p.Proc.ChapOyoq.Rotation = new Vector3(a, 0, 0);
                p.Proc.OngOyoq.Rotation = new Vector3(-a, 0, 0);
                p.Proc.ChapQol.Rotation = new Vector3(-a * 0.6f, 0, 0);
                if (!p.Bloklaymi)
                {
                    p.Proc.OngQol.Rotation = new Vector3(a * 0.6f, 0, 0);
                }
            }

            // footstep sound
            p.QadamTimer -= dt;
            if (p.QadamTimer <= 0)
            {
                p.QadamTimer = p.Otda ? 0.26 : (yuguradi ? 0.33 : 0.46);
                BuxoroAudio.S(p.Otda ? "tuyoq" : "qadam");
            }
        }
        else
        {
            Anim.Oynat(p.Anim, p.Otda ? "ride_idle" : (p.Bloklaymi ? "block_idle" : "idle"));
            if (p.Proc != null && !p.Otda)
            {
                float k = Smoothing.LimbRelax(dt);
                p.Proc.ChapOyoq.Rotation *= k;
                p.Proc.OngOyoq.Rotation *= k;
                p.Proc.ChapQol.Rotation *= k;
            }
        }

        // ---- JUMP: Space, on foot only ----
        if (t.Space && !p.Otda && !p.Sakramoqda)
        {
            p.Sakramoqda = true;
            p.Vy = B.SakrashTezligi;
            t.Space = false; // holding it must not re-jump
            Anim.Oynat(p.Anim, "jump", new AnimOpt { BirMarta = true, Majburiy = true, Fade = 0.05 });
            BuxoroAudio.S("qadam");
        }

        if (p.Sakramoqda)
        {
            p.Vy -= B.Gravitatsiya * dt;
            p.Balandlik += p.Vy * dt;
            if (p.Balandlik <= 0)
            {
                p.Balandlik = 0;
                p.Vy = 0;
                p.Sakramoqda = false;
                BuxoroAudio.S("qadam");
            }
        }

        // field bounds
        Chegara(p);

        // Obstacles: buildings, the city wall, poplars. Even from inside, the
        // nearest edge ejects (playtest, band 7).
        if (dunyo != null)
        {
            World.ToqnashuvHal(dunyo, ref p.X, ref p.Z, p.Otda ? B.ToqnashuvRadiusOtda : B.ToqnashuvRadiusPiyoda);
            Chegara(p); // still inside after the push
        }

        // follow the horse mesh
        double yer = World.YerBalandligi(p.X, p.Z);
        if (p.Otda && p.Ot != null)
        {
            p.Ot.Position = new Vector3((float)p.X, (float)yer, (float)p.Z);
            p.Ot.Rotation = new Vector3(p.Ot.Rotation.X, p.Mesh.Rotation.Y, p.Ot.Rotation.Z);
            p.Y = yer + 1.42;
            if (p.Proc != null)
            {
                p.Proc.ChapOyoq.Rotation = new Vector3(-1.1f, 0, 0);
                p.Proc.OngOyoq.Rotation = new Vector3(-1.1f, 0, 0);
            }
        }
        else
        {
            p.Y = yer + p.Balandlik; // ground + jump height
        }

        p.Mesh.Position = new Vector3((float)p.X, (float)p.Y, (float)p.Z);

        // shield — the bow needs both hands
        if (p.Qurol == "kamon")
        {
            p.Bloklaymi = false;
        }

        if (p.Qalqon != null)
        {
            p.Qalqon.Visible = p.Bloklaymi;
        }

        if (p.Proc != null && p.Bloklaymi)
        {
            p.Proc.ChapQol.Rotation = new Vector3(-1.15f, 0, 0);
        }

        if (p.UrishCooldown > 0)
        {
            p.UrishCooldown -= dt;
        }

        /* TURNING — on the spot, no arc.
           Standing: toward where the camera looks (mouse turn = body turn).
           Moving:   toward the movement (S turns him round).
           Swing/block: toward the camera, so the blow lands on screen. */
        double kameraga = p.Yonalish + Math.PI;
        double maqsad = (p.Harakatda && !p.Bloklaymi && p.UrishCooldown <= 0) ? p.HarakatYonalishi : kameraga;
        double yaw = BurchakLerp(p.Mesh.Rotation.Y, maqsad, Smoothing.BodyYaw(dt));
        p.Mesh.Rotation = new Vector3(p.Mesh.Rotation.X, (float)yaw, p.Mesh.Rotation.Z);
        p.Harakatda = false;

        Anim.QulfYangila(p.Anim, dt);
        ZarbaPozasi(p, dt);

        // E / F hold timers
        if (t.E)
        {
            p.EUshlandi += dt;
        }

        if (t.F)
        {
            p.FUshlandi += dt;
        }
        else
        {
            p.FUshlandi = 0;
        }
    }

    private static void Chegara(Oyinchi p)
    {
        p.X = Math.Max(2, Math.Min(B.MaydonKengligi - 4, p.X));
        p.Z = Math.Max(-47, Math.Min(47, p.Z));
    }

    /// <summary>player.js:419-423.</summary>
    public static double BurchakLerp(double a, double b, double t)
    {
        double d = ((b - a + Math.PI) % (Math.PI * 2)) - Math.PI;
        if (d < -Math.PI)
        {
            d += Math.PI * 2;
        }

        return a + (d * t);
    }

    /// <summary>player.js:534-538.</summary>
    public static double BurchakFarq(double a, double b)
    {
        double d = ((a - b + Math.PI) % (Math.PI * 2)) - Math.PI;
        if (d < -Math.PI)
        {
            d += Math.PI * 2;
        }

        return d;
    }

    // ---- Camera (§1), player.js:425-470 ----

    /// <summary>Collision layer the occlusion ray hits — poplar trunks only.</summary>
    private const uint OkklyuziyaQatlami = 1u << 19;

    private static readonly List<GeometryInstance3D> Shaffoflar = new();

    /// <summary>The camera offset table (player.js:432-434): back, up, FOV.</summary>
    public static (double Orqa, double Tepa, double Fov) KameraJadvali(bool otda) =>
        otda ? (8.0, 5.5, 78) : (6.0, 4.5, 70);

    /// <summary>main.js:120-127 — snap behind the player with no lerp.</summary>
    public static void KameraniJoylashtir(Oyinchi p, Camera3D camera)
    {
        camera.LookAtFromPosition(
            new Vector3((float)(p.X + (Math.Sin(p.Yonalish) * 6)), (float)(p.Y + 4.5), (float)(p.Z + (Math.Cos(p.Yonalish) * 6))),
            new Vector3((float)p.X, (float)(p.Y + 1.5), (float)p.Z),
            Vector3.Up);
    }

    /// <summary>
    /// player.js:431-470. Note the pitch is never read: the mouse moves the
    /// camera only sideways. That is the shipped behaviour.
    /// </summary>
    public static void Kamera(Oyinchi p, Camera3D camera, double dt)
    {
        var (orqa, tepa, fov) = KameraJadvali(p.Otda);
        if (Math.Abs(camera.Fov - fov) > 0.1)
        {
            camera.Fov += (float)((fov - camera.Fov) * Smoothing.Fov(dt));
        }

        double sin = Math.Sin(p.Yonalish), cos = Math.Cos(p.Yonalish);
        var kamMaqsad = new Vector3((float)(p.X + (sin * orqa)), (float)(p.Y + tepa), (float)(p.Z + (cos * orqa)));

        // lerp 0.12 — not rigid, it trails a little (a sense of weight)
        camera.Position = camera.Position.Lerp(kamMaqsad, Smoothing.CameraPosition(dt));
        var kamPoz = new Vector3((float)p.X, (float)(p.Y + 1.5), (float)p.Z);
        if (!camera.Position.IsEqualApprox(kamPoz))
        {
            camera.LookAt(kamPoz, Vector3.Up);
        }

        // Occlusion: whatever blocks the view turns into a see-through silhouette.
        foreach (GeometryInstance3D o in Shaffoflar)
        {
            if (GodotObject.IsInstanceValid(o))
            {
                o.Transparency = 0f;
            }
        }

        Shaffoflar.Clear();

        PhysicsDirectSpaceState3D? fazo = camera.GetWorld3D()?.DirectSpaceState;
        if (fazo == null)
        {
            return;
        }

        Vector3 yon = kamPoz - camera.Position;
        float masofa = yon.Length();
        if (masofa <= 1.2f)
        {
            return;
        }

        Vector3 oxir = camera.Position + (yon.Normalized() * (masofa - 1.2f)); // rc.far = masofa - 1.2
        var istisno = new Godot.Collections.Array<Rid>();
        for (int i = 0; i < 6; i++) // intersectObjects(...).slice(0, 6)
        {
            var q = PhysicsRayQueryParameters3D.Create(camera.Position, oxir, OkklyuziyaQatlami, istisno);
            var h = fazo.IntersectRay(q);
            if (h.Count == 0)
            {
                break;
            }

            istisno.Add(h["rid"].AsRid());
            if (h["collider"].AsGodotObject() is Node body && body.GetParent() is GeometryInstance3D gi)
            {
                gi.Transparency = 0.78f; // opacity 0.22
                Shaffoflar.Add(gi);
            }
        }
    }

    /// <summary>
    /// Gives each poplar mesh a trimesh body on the occlusion layer, so the camera
    /// ray hits the actual tree like three.js's Raycaster does. Nothing else uses
    /// this layer: the player, apprentices and enemies have no physics at all.
    /// </summary>
    public static void OkklyuziyaTayyorla(IEnumerable<Node3D> teraklar)
    {
        foreach (Node3D t in teraklar)
        {
            foreach (Node n in t.FindChildren("*", nameof(MeshInstance3D), true, false))
            {
                if (n is not MeshInstance3D mi || mi.Mesh == null)
                {
                    continue;
                }

                var body = new StaticBody3D { CollisionLayer = OkklyuziyaQatlami, CollisionMask = 0 };
                body.AddChild(new CollisionShape3D { Shape = mi.Mesh.CreateTrimeshShape() });
                mi.AddChild(body);
            }
        }
    }
}
