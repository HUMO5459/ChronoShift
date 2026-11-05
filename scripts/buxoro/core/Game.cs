using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ChronoShift;

/// <summary>Tarobiy — the old master (game.js:97-118).</summary>
public sealed class Tarobiy
{
    public Node3D Mesh = null!;
    public Gumanoid? Proc;
    public MeshInstance3D Nur = null!;
    public StandardMaterial3D NurMat = null!;
    public AnimCtrl? Anim;
    public bool Olgan;
    public double X, Z;
}

/// <summary>An ending in progress (game.js:590).</summary>
public sealed class Tugash
{
    public string Tur = "";
    public double Taymer;
    public bool Korsatildi;
}

/// <summary>The game state game.js <c>yarat()</c> returns — <c>O</c> (game.js:74-94).</summary>
public sealed class OyinHolati
{
    public Node3D Scene = null!;
    public Camera3D Camera = null!;
    public Dunyo Dunyo = null!;
    public World World = null!;
    public Godot.Environment Env = null!;
    public Oyinchi Oyinchi = null!;
    public ShogirdTizim Shogirdlar = null!;
    public List<Askar> Askarlar = null!;
    public HumoQush Humo = null!;
    public Tarobiy Tarobiy = null!;

    /// <summary>setTimeout — wall-clock, keeps running through pause, like the browser's.</summary>
    public Action<double, Action> Kechik = (_, _) => { };

    public double Vaqt;
    public double Otgan;
    public int Parda = 1;
    public readonly List<Dushman> Dushmanlar = new();
    public readonly List<Oq> Oqlar = new();
    public int TolqinIndeks;
    public bool TolqinOgoh;
    public int OlganDushman;
    public int QurilganDevor, YasalganQurol;
    public Dictionary<string, string> Chizma = new() { ["kaltak"] = "ochiq", ["kamon"] = "qulf", ["qilich"] = "qulf" };
    public Tugash? Tugadi;
    public int TutorialQadam;
    public double TurganVaqt;
    public Vector3 OxirgiPoz;
    public bool QilichTanlov;
    public Node3D? Ot;
    public AnimCtrl? OtAnim;
    public bool OtMinilgan;
    public bool BozorGapi;
    public bool DevorMaslahati;
    public string[] KorinadiganResurs = { "tosh" };
    public double TumanZichligi;
    public string? Missiya;

    /// <summary>game.js:84 — what the touch buttons show; written by OzaroTasir.</summary>
    public readonly SensorHolat SensorHolat = new();
}

/// <summary>
/// The orchestrator, game.js — §7 (time), §8-10 (acts), §11 (endings).
/// <see cref="Yangila"/> keeps game.js's call order (game.js:121-180): clock and
/// BOTH terminal checks first, then player, acts, waves, Humo, apprentices,
/// combat, the strike, interaction, HUD, Tarobiy's pulse, fog. Checked by reading.
/// </summary>
public static class Game
{
    private static BuxoroBalance B => BuxoroRuntime.B;

    // ---- dialogue, game.js:7-63 ----
    private static readonly Dictionary<string, Dictionary<string, string>> Gap = new()
    {
        ["uz"] = new()
        {
            ["t1a"] = "\"Ko‘rdingmi? Chang. Uch kunlik yo‘ldan kelishyapti.\"",
            ["t1b"] = "\"Bizda qurol yo‘q. Odam ham kam. Bizda faqat bitta narsa bor — <b>vaqt.</b> Va uni ham yer beradi.\"",
            ["t1c"] = "\"Toshni ko‘rsat. Yig‘ishni ularga qo‘y.\"",
            ["yugur"] = "\"Yugur!\"",
            ["bozor"] = "\"Bir kishining qo‘lida turgan hunar — hunar emas, omad.\"",
            ["t2a"] = "\"Kaltak bilan otliqqa yetolmaysan. Ular uzoqdan otadi.\"",
            ["t2b"] = "\"Biz ham otamiz.\"",
            ["qilich"] = "Bu chizmani hech kim bermadi.",
            ["tanla"] = "1 — o‘zimga · 2 — askarga",
            ["gal1"] = "\"Ular bir kishini o‘ldirdilar.\"",
            ["gal2"] = "\"Hunar sakkiz qo‘lda edi.\"",
            ["mag1"] = "\"Juvayniy yozadi: ular zirhsiz chiqdilar.\"",
            ["mag2"] = "\"U g‘olib tomonda edi. Yozgani shu.\"",
            ["ochilish"] = "\"Ular kelganida men sakkiz yoshda edim. Endi yigirma to‘qqizdaman.<br>Shahar hali ham kul hidi anqiydi.\"",
            ["galaba"] = "QUSH QAYTDI", ["maglubiyat"] = "QUSH QAYTMADI",
            // game.js:16 and :25 both define 'olim'; the later one wins in JS
            ["olim"] = "USTA YIQILDI",
            ["olim1"] = "\"Temirchi yiqildi. Bolg'a yerda qoldi.\"",
            ["olim2"] = "\"Uni ko'taradigan odam qolmadi.\"",
            ["qayta"] = "QAYTA BOSHLASH", ["tarobiy"] = "TAROBIY",
            ["otMaslahat"] = "Maslahat: qo‘shin yaqin. Har devor seksiyasi dushmanni ushlab turadi va +{n} soniya beradi. Tosh bilan devor chizig‘ida F ni ushlab tur — QUR.",
            ["devorButun"] = "Maslahat: devor butun. Seksiya buzilsa, o‘sha joyda tosh bilan qayta qurish mumkin — bu devorni kuchaytiradi.",
            ["devorBuzildi"] = "Devor seksiyasi buzildi. O‘sha joyda tosh bilan qayta qur — dushman yana to‘xtaydi.",
            ["sDevor"] = "qurilgan devor", ["sQurol"] = "yasalgan qurol",
            ["sShogird"] = "yollangan shogird", ["sVaqt"] = "qolgan vaqt", ["sDushman"] = "yiqilgan dushman",
        },
        ["en"] = new()
        {
            ["t1a"] = "\"You see it? Dust. Three days' ride away.\"",
            ["t1b"] = "\"We have no weapons. Few men. We have only one thing — <b>time.</b> And the earth gives that too.\"",
            ["t1c"] = "\"Point at the stone. Let them gather it.\"",
            ["yugur"] = "\"Run!\"",
            ["bozor"] = "\"A craft in one man's hands is not a craft — it is luck.\"",
            ["t2a"] = "\"A club will not reach a rider. They shoot from afar.\"",
            ["t2b"] = "\"So will we.\"",
            ["qilich"] = "No one gave you this drawing.",
            ["tanla"] = "1 — for myself · 2 — for a soldier",
            ["gal1"] = "\"They killed one man.\"",
            ["gal2"] = "\"The craft was in eight hands.\"",
            ["mag1"] = "\"Juvayni writes: they came out without armor.\"",
            ["mag2"] = "\"He was on the winning side. That is what he wrote.\"",
            ["ochilish"] = "\"I was eight when they came. Now I am twenty-nine.<br>The city still smells of ash.\"",
            ["galaba"] = "THE BIRD RETURNED", ["maglubiyat"] = "THE BIRD DID NOT RETURN",
            ["olim"] = "THE SMITH FELL",
            ["olim1"] = "\"The blacksmith fell. The hammer stayed on the ground.\"",
            ["olim2"] = "\"No one was left to pick it up.\"",
            ["qayta"] = "RESTART", ["tarobiy"] = "TAROBIY",
            ["otMaslahat"] = "Tip: the army is close. Each wall section holds them back and gives +{n} seconds. With stone, at the wall line, hold F — BUILD.",
            ["devorButun"] = "Tip: the wall is complete. If a section breaks, rebuild it there with stone — that strengthens the wall.",
            ["devorBuzildi"] = "A wall section fell. Rebuild it there with stone — the enemy will stop again.",
            ["sDevor"] = "walls raised", ["sQurol"] = "weapons made",
            ["sShogird"] = "apprentices recruited", ["sVaqt"] = "time remaining", ["sDushman"] = "enemies felled",
        },
    };

    /// <summary>game.js:64.</summary>
    public static string G(string k) => Gap[BuxoroHud.Til].TryGetValue(k, out string? v) ? v : k;

    private static string T(string k) => BuxoroHud.T(k);

    /// <summary>game.js:66-95.</summary>
    public static OyinHolati Yarat(Node3D scene, Camera3D camera, Dunyo dunyo, World world, Godot.Environment env, Action<double, Action> kechik)
    {
        var o = new OyinHolati
        {
            Scene = scene, Camera = camera, Dunyo = dunyo, World = world, Env = env, Kechik = kechik,
            Oyinchi = Player.Yarat(scene),
            Shogirdlar = Apprentices.Tizim(scene, dunyo),
            Askarlar = Combat.AskarlarYarat(scene),
            Humo = Humo.Yarat(scene, dunyo.HumoTayanchi!),
            Tarobiy = TarobiyYarat(scene),
            Vaqt = B.BoshlangichVaqt,
            TumanZichligi = BuxoroRuntime.W.TumanZichligi,
        };
        return o;
    }

    /// <summary>game.js:97-118.</summary>
    private static Tarobiy TarobiyYarat(Node3D scene)
    {
        Node3D? mesh = BuxoroModels.Ol("tarobiy");
        Gumanoid? proc = null;
        if (mesh == null)
        {
            proc = Actors.Tarobiy();
            mesh = proc.Root;
        }

        mesh.Position = new Vector3(B.TarobiyJoyi.X, 0, B.TarobiyJoyi.Y);
        mesh.Rotation = new Vector3(0, -Mathf.Pi / 2, 0); // west — facing the dust
        scene.AddChild(mesh);

        // a white pillar of light — unlike the mark, a slow pulse (§5)
        var nurMat = new StandardMaterial3D
        {
            AlbedoColor = new Color(Actors.Rang(0xf5f0e2), 0.22f), ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha, CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Disabled,
        };
        MeshInstance3D nur = Actors.Silindr(0.22f, 0.38f, 9, nurMat, B.TarobiyJoyi.X, 4.5f, B.TarobiyJoyi.Y, 8, ochiq: true);
        nur.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        scene.AddChild(nur);

        var t = new Tarobiy { Mesh = mesh, Proc = proc, Nur = nur, NurMat = nurMat, X = B.TarobiyJoyi.X, Z = B.TarobiyJoyi.Y };
        t.Anim = Anim.Ulash(mesh, "tarobiy");
        Anim.Oynat(t.Anim, "idle_talk");
        return t;
    }

    // ================= UPDATE =================

    /// <summary>game.js:121-180.</summary>
    public static void Yangila(OyinHolati o, double dt)
    {
        if (o.Tugadi != null)
        {
            YakunYangila(o, dt);
            return;
        }

        o.Otgan += dt;
        o.Vaqt -= dt;
        BuxoroHud.Soat(o.Vaqt);

        // ---- defeat: the clock reads 0:00 ----
        if (o.Vaqt <= 0)
        {
            Yakunla(o, "maglubiyat");
            return;
        }

        // ---- victory: all 24 enemies down ----
        if (o.OlganDushman >= B.JamiDushman)
        {
            Yakunla(o, "galaba");
            return;
        }

        Player.Yangila(o.Oyinchi, dt, o.Dunyo);
        PardaYangila(o, dt);
        TolqinYangila(o);
        Humo.Yangila(o.Humo, dt);

        Apprentices.Yangila(o.Scene, o.Shogirdlar, o.Dunyo, dt, o.Dushmanlar, o.Otgan);

        Combat.Yangila(o.Scene, dt, o.Dushmanlar, o.Askarlar, o.Dunyo, o.Oyinchi, o.Oyinchi.Olgan, o.Oqlar, h => Hodisa(o, h));

        // ---- strike ----
        if (o.Oyinchi.Urmoqchi)
        {
            o.Oyinchi.Urmoqchi = false;
            var r = Player.Urish(o.Oyinchi, o.Dushmanlar);
            if (r is { Tur: "oq" } oq)
            {
                // the bow: an arrow flies if there is a target, else it is loosed for nothing
                if (oq.Maqsad != null)
                {
                    o.Oqlar.Add(Combat.OqOt(o.Scene, o.Oyinchi.X, o.Oyinchi.Z, oq.Maqsad, oq.Zarar, o.Oyinchi.Y + 1.45));
                }
            }
            else if (r is { } yaqin)
            {
                foreach (Dushman d in yaqin.Urilgan)
                {
                    Combat.ZararBer(d, yaqin.Zarar, h => Hodisa(o, h));
                }
            }
        }

        // ---- E: mark / recruit / mount ----
        OzaroTasir(o, dt);

        // ---- HUD ----
        BuxoroHud.Qollar(o.Shogirdlar.Shogirdlar, B.ShogirdMaksimum);
        BuxoroHud.Resurslar(o.Shogirdlar.Ombor, o.KorinadiganResurs);
        BuxoroHud.Jon(o.Oyinchi.Hp, B.ShomurodHp);
        AskarJonlari(o);

        // Tarobiy's beam pulses
        if (!o.Tarobiy.Olgan)
        {
            Color c = o.Tarobiy.NurMat.AlbedoColor;
            o.Tarobiy.NurMat.AlbedoColor = new Color(c.R, c.G, c.B, (float)(0.16 + (Math.Sin(o.Otgan * 1.6) * 0.09)));
        }

        // the dust thickens as a wave approaches (§2)
        double maqsadTuman = o.TolqinOgoh ? 0.0115 : 0.0075;
        o.TumanZichligi += (maqsadTuman - o.TumanZichligi) * Smoothing.Fog(dt);
        o.World.TumanQoy(o.Env, (float)o.TumanZichligi);
    }

    /// <summary>game.js:185-199 — small bars over nearby soldiers (playtest, band 5).</summary>
    private static void AskarJonlari(OyinHolati o)
    {
        var r = new List<JonBar>();
        double m2 = B.AskarJonMasofa * B.AskarJonMasofa;
        Vector2 ekran = o.Camera.GetViewport().GetVisibleRect().Size;
        foreach (Askar a in o.Askarlar)
        {
            if (a.Olgan)
            {
                continue;
            }

            double dx = a.X - o.Oyinchi.X, dz = a.Z - o.Oyinchi.Z;
            if ((dx * dx) + (dz * dz) > m2)
            {
                continue;
            }

            var w = new Vector3((float)a.X, (float)(a.Y + 2.2), (float)a.Z);
            if (o.Camera.IsPositionBehind(w))
            {
                continue;
            }

            Vector2 s = o.Camera.UnprojectPosition(w);
            if (s.X < -0.025f * ekran.X || s.X > 1.025f * ekran.X || s.Y < -0.025f * ekran.Y || s.Y > 1.025f * ekran.Y)
            {
                continue;
            }

            r.Add(new JonBar(s.X, s.Y, (float)Math.Max(0, a.Hp / B.AskarHp)));
        }

        BuxoroHud.Jonlar(r);
    }

    // ================= INTERACTION =================

    /// <summary>game.js:202-334.</summary>
    private static void OzaroTasir(OyinHolati o, double dt)
    {
        Oyinchi p = o.Oyinchi;
        string? korsatma = null;
        double? ushla = null;

        // state for the touch buttons
        bool eKor = false, fKor = false;
        string eYorliq = "E";

        // --- mounting (Act III, or the horse given at the cavalry wave) ---
        if (o.Ot != null && !p.Otda)
        {
            double dx = o.Ot.Position.X - p.X, dz = o.Ot.Position.Z - p.Z;
            if ((dx * dx) + (dz * dz) < 9)
            {
                korsatma = T("min");
                eKor = true;
                eYorliq = "🐎";
                if (p.EUshlandi > 0.7)
                {
                    Player.OtMin(p, o.Ot);
                    o.OtMinilgan = true;
                    p.EUshlandi = 0;
                    Anim.Oynat(o.OtAnim, "horse_mounted_idle");
                    OtMaslahat(o);
                }
            }
        }

        // --- recruit a craftsman: hold E ---
        YollashNatija? y = Apprentices.YollashTekshir(o.Scene, o.Shogirdlar, o.Dunyo, p.X, p.Z, p.EUshlandi);
        if (y != null)
        {
            if (y.Tugadi)
            {
                BuxoroHud.Toast(T("shogirdQoshildi"));
            }
            else if (y.Nisbat != null)
            {
                ushla = y.Nisbat;
                korsatma = T("yolla");
                eKor = true;
                eYorliq = "✋";
            }
            else if (y.Yaqin)
            {
                korsatma = T("yolla");
                eKor = true;
                eYorliq = "✋";
            }
        }

        // --- mark: press E ---
        Tugun? tugun = Apprentices.YaqinTugun(o.Dunyo, p.X, p.Z);
        if (tugun != null)
        {
            eKor = true;
            if (eYorliq == "E")
            {
                eYorliq = "◈";
            }
        }

        if (tugun != null && korsatma == null)
        {
            korsatma = T("belgila") + " — " + T(tugun.Tur).ToUpperInvariant(); // the resource NAME too (playtest, band 2)
        }

        if (p.EBosildi)
        {
            p.EBosildi = false;
            string r = Apprentices.Belgila(o.Scene, o.Shogirdlar, o.Dunyo, p.X, p.Z);
            if (r == "navbatTola")
            {
                BuxoroHud.Toast(T("navbatTola"));
            }
            else if (r == "ok" && o.TutorialQadam == 1)
            {
                o.TutorialQadam = 2;
            }
        }

        // --- build the wall: F at a wall point ---
        DevorNuqta? devorNuqta = null;
        foreach (DevorNuqta n in o.Dunyo.DevorNuqtalari)
        {
            if (n.Qurilgan)
            {
                continue;
            }

            double dx = n.X - p.X, dz = n.Z - p.Z;
            if ((dx * dx) + (dz * dz) < 16)
            {
                devorNuqta = n;
            }
        }

        BuxoroRecipeRow devorR = B.Recipe("devor")!;
        if (devorNuqta != null)
        {
            bool yetarli = o.Shogirdlar.Ombor["tosh"] >= devorR.Tosh;
            fKor = yetarli;
            korsatma = yetarli ? T("qur") : T("resursYetmadi");
            if (yetarli && p.FUshlandi > 0.1)
            {
                ushla = Math.Min(1, p.FUshlandi / B.YasashVaqti);
                if (Math.Floor(p.FUshlandi * 6) != Math.Floor((p.FUshlandi - dt) * 6))
                {
                    BuxoroAudio.S("bolga");
                }

                if (p.FUshlandi >= B.YasashVaqti)
                {
                    p.FUshlandi = 0;
                    o.Shogirdlar.Ombor["tosh"] -= devorR.Tosh;
                    Combat.DevorQur(o.Scene, devorNuqta, o.Dunyo);
                    o.QurilganDevor++;
                    VaqtQosh(o, B.DevorBonus);
                    BuxoroHud.Toast(T("devorTurdi"));
                    if (o.QurilganDevor >= B.DevorSeksiyaSoni)
                    {
                        Videolar.Sabab("toshTugadi"); // the stone stage is over
                    }

                    if (o.TutorialQadam == 3)
                    {
                        o.TutorialQadam = 4;
                    }
                }
            }
        }

        // --- the forge: F crafts a bow / sword ---
        double ux = B.Ustaxona.X - p.X, uz = B.Ustaxona.Y - p.Z;
        if ((ux * ux) + (uz * uz) < 25 && devorNuqta == null)
        {
            string? nima = NimaYasaladi(o);
            if (nima != null)
            {
                fKor = true;
                korsatma = T("yasa") + " — " + T(nima);
                double kerak = nima == "qilich" ? B.QilichYasashVaqti : B.YasashVaqti;
                if (p.FUshlandi > 0.1)
                {
                    ushla = Math.Min(1, p.FUshlandi / kerak);
                    if (Math.Floor(p.FUshlandi * 6) != Math.Floor((p.FUshlandi - dt) * 6))
                    {
                        BuxoroAudio.S("bolga");
                    }

                    if (p.FUshlandi >= kerak)
                    {
                        p.FUshlandi = 0;
                        Yasa(o, nima);
                    }
                }
            }
            else if (o.Parda >= 2)
            {
                korsatma = T("resursYetmadi");
            }
        }

        // --- the sword choice: 1 / 2 ---
        if (o.QilichTanlov)
        {
            korsatma = G("tanla");
            if (p.Tugmalar.Digit1)
            {
                p.Tugmalar.Digit1 = false;
                QilichBer(o, "ozim");
            }

            if (p.Tugmalar.Digit2)
            {
                p.Tugmalar.Digit2 = false;
                QilichBer(o, "askar");
            }
        }
        else
        {
            // --- weapon switch: 1 club · 2 bow · 3 sword ---
            foreach ((bool bosildi, string q, int i) in new[] { (p.Tugmalar.Digit1, "kaltak", 1), (p.Tugmalar.Digit2, "kamon", 2), (p.Tugmalar.Digit3, "qilich", 3) })
            {
                if (!bosildi)
                {
                    continue;
                }

                if (i == 1)
                {
                    p.Tugmalar.Digit1 = false;
                }
                else if (i == 2)
                {
                    p.Tugmalar.Digit2 = false;
                }
                else
                {
                    p.Tugmalar.Digit3 = false;
                }

                if (Player.QurolTanla(p, q))
                {
                    BuxoroHud.Toast(T(q));
                }
                else if (!p.Qurollar[q])
                {
                    BuxoroHud.Toast(T(q) + " — " + T("haliYoq")); // not made yet
                }
            }
        }

        BuxoroHud.Korsatma(korsatma);
        BuxoroHud.Ushla(ushla);

        // game.js:322-333 — the fight buttons: an enemy within 14 m
        bool jang = false;
        foreach (Dushman d in o.Dushmanlar)
        {
            if (d.Olgan)
            {
                continue;
            }

            double dx = d.Mesh.Position.X - p.X, dz = d.Mesh.Position.Z - p.Z;
            if ((dx * dx) + (dz * dz) < 196)
            {
                jang = true;
                break;
            }
        }

        SensorHolat h = o.SensorHolat;
        h.EKorinadi = eKor;
        h.FKorinadi = fKor;
        h.Jang = jang;
        h.EYorliq = eYorliq;
        h.Qurol = p.Qurol;
        h.QurolSoni = (p.Qurollar["kaltak"] ? 1 : 0) + (p.Qurollar["kamon"] ? 1 : 0) + (p.Qurollar["qilich"] ? 1 : 0);
    }

    /// <summary>game.js:336-349 — the sword outranks (Act III); the first bow is the player's.</summary>
    private static string? NimaYasaladi(OyinHolati o)
    {
        Dictionary<string, int> om = o.Shogirdlar.Ombor;
        BuxoroRecipeRow kamon = B.Recipe("kamon")!, qilich = B.Recipe("qilich")!;
        if (o.Chizma["qilich"] == "ochiq" && om["temir"] >= qilich.Temir && om["komir"] >= qilich.Komir)
        {
            return "qilich";
        }

        if (o.Chizma["kamon"] == "ochiq" && om["yogoch"] >= kamon.Yogoch && om["teri"] >= kamon.Teri)
        {
            if (!o.Oyinchi.Qurollar["kamon"] || o.Askarlar.Any(a => a.Qurol == "yoq"))
            {
                return "kamon";
            }
        }

        return null;
    }

    /// <summary>game.js:351-373.</summary>
    private static void Yasa(OyinHolati o, string nima)
    {
        Dictionary<string, int> om = o.Shogirdlar.Ombor;
        if (nima == "kamon")
        {
            BuxoroRecipeRow r = B.Recipe("kamon")!;
            om["yogoch"] -= r.Yogoch;
            om["teri"] -= r.Teri;
            if (!o.Oyinchi.Qurollar["kamon"])
            {
                // §5: the first bow is yours — now you can strike from afar
                Player.KamonBer(o.Oyinchi);
                BuxoroHud.Toast(T("kamonOzimga"));
            }
            else
            {
                Askar? a = o.Askarlar.FirstOrDefault(s => s.Qurol == "yoq" && !s.Olgan);
                if (a != null)
                {
                    Combat.AskarniQurollantir(a, "kamon");
                }

                BuxoroHud.Toast(T("kamonTayyor"));
            }

            o.YasalganQurol++;
            BuxoroAudio.S("bolga");
        }
        else if (nima == "qilich")
        {
            BuxoroRecipeRow r = B.Recipe("qilich")!;
            om["temir"] -= r.Temir;
            om["komir"] -= r.Komir;
            o.QilichTanlov = true; // §10: for myself, or for a soldier?
            BuxoroAudio.S("bolga");
        }
    }

    /// <summary>game.js:375-386.</summary>
    private static void QilichBer(OyinHolati o, string kimga)
    {
        o.QilichTanlov = false;
        o.YasalganQurol++;
        Videolar.Sabab("qilichYasaldi");
        if (kimga == "ozim")
        {
            Player.QilichBer(o.Oyinchi);
        }
        else
        {
            Askar? a = o.Askarlar.FirstOrDefault(s => s.Qurol == "yoq" && !s.Olgan);
            if (a != null)
            {
                Combat.AskarniQurollantir(a, "qilich");
            }
        }

        BuxoroHud.Toast(T("qilichTayyor"));
    }

    /// <summary>game.js:392-395 — a system-voice tip after mounting (playtest, band 10).</summary>
    private static void OtMaslahat(OyinHolati o)
    {
        bool bosh = o.Dunyo.DevorNuqtalari.Any(n => !n.Qurilgan);
        BuxoroHud.Gap("", bosh ? G("otMaslahat").Replace("{n}", B.DevorBonus.ToString()) : G("devorButun"), 8);
    }

    // ================= TIME =================

    /// <summary>game.js:398-403 — the clock jumps back, capped at 12:00.</summary>
    private static void VaqtQosh(OyinHolati o, double soniya)
    {
        o.Vaqt = Math.Min(B.MaksimumVaqt, o.Vaqt + soniya);
        BuxoroHud.Soat(o.Vaqt);
        BuxoroHud.VaqtQoshildi();
        BuxoroAudio.S("vaqt");
    }

    // ================= ACTS =================

    /// <summary>game.js:406-457.</summary>
    private static void PardaYangila(OyinHolati o, double dt)
    {
        double v = o.Vaqt;

        // ---- ACT I: the tutorial ----
        if (o.Parda == 1)
        {
            if (o.TutorialQadam == 0 && o.Otgan > 1.5)
            {
                o.TutorialQadam = 1;
                BuxoroHud.Gap(G("tarobiy"), G("t1a"), 4);
                o.Kechik(4.2, () =>
                {
                    if (o.Tugadi == null)
                    {
                        BuxoroHud.Gap(G("tarobiy"), G("t1b"), 5);
                    }
                });
                o.Kechik(9.4, () =>
                {
                    if (o.Tugadi == null)
                    {
                        BuxoroHud.Gap(G("tarobiy"), G("t1c"), 4);
                    }
                });
            }

            // step 2: standing still, Tarobiy shouts
            if (o.TutorialQadam == 2)
            {
                var poz = new Vector3((float)o.Oyinchi.X, (float)o.Oyinchi.Y, (float)o.Oyinchi.Z);
                double q = poz.DistanceTo(o.OxirgiPoz);
                o.OxirgiPoz = poz;
                if (q < 0.1 * 60 * dt)
                {
                    o.TurganVaqt += dt;
                }
                else
                {
                    o.TurganVaqt = 0;
                }

                if (o.TurganVaqt > 3)
                {
                    o.TurganVaqt = -6;
                    BuxoroHud.Gap(G("tarobiy"), G("yugur"), 2);
                    BuxoroHud.Korsatma(T("kutma"));
                }

                if (o.Shogirdlar.Ombor["tosh"] >= B.Recipe("devor")!.Tosh)
                {
                    o.TutorialQadam = 3;
                }
            }

            // the one line about the bazaar
            if (!o.BozorGapi && v < 560)
            {
                o.BozorGapi = true;
                BuxoroHud.Gap(G("tarobiy"), G("bozor"), 5);
            }

            if (v <= 480)
            {
                Parda2(o);
            }
        }

        // ---- ACT II ----
        if (o.Parda == 2 && v <= B.TarobiyOlimi)
        {
            Parda3(o);
        }

        // ---- ACT III: the sword unlocks by itself ----
        if (o.Parda == 3 && o.Chizma["qilich"] == "olik" && o.OtMinilgan)
        {
            double ux = B.Ustaxona.X - o.Oyinchi.X, uz = B.Ustaxona.Y - o.Oyinchi.Z;
            if ((ux * ux) + (uz * uz) < 36)
            {
                o.Chizma["qilich"] = "ochiq";
                BuxoroHud.Chizmalar(o.Chizma);
                o.KorinadiganResurs = new[] { "temir", "komir" };
                Chizma.Korsat("qilich", 9); // §10's peak — "No one gave you this drawing"
                BuxoroAudio.S("bolga");
            }
        }
    }

    /// <summary>game.js:459-470.</summary>
    private static void Parda2(OyinHolati o)
    {
        o.Parda = 2;
        Videolar.Sabab("toshTugadi"); // the stone stage ends here too, wall finished or not
        o.Chizma["kamon"] = "ochiq";
        BuxoroHud.Chizmalar(o.Chizma);
        Chizma.Korsat("kamon", 7); // the blueprint opens — the weapon up close (the game does not stop)
        o.KorinadiganResurs = new[] { "yogoch", "teri" };
        BuxoroHud.Gap(G("tarobiy"), G("t2a"), 4);
        o.Kechik(4.2, () =>
        {
            if (o.Tugadi == null && o.Parda == 2)
            {
                BuxoroHud.Gap(G("tarobiy"), G("t2b"), 3);
            }
        });
    }

    /// <summary>game.js:473-511 — §9: no cutscene, no slow-motion, the camera does not move.</summary>
    private static void Parda3(OyinHolati o)
    {
        o.Parda = 3;
        o.Tarobiy.Olgan = true;
        Videolar.Sabab("tarobiy"); // the end of Act II — Tarobiy takes an arrow (video 6)

        // kneels and falls — the body stays on the field
        bool olimKlipi = Anim.Oynat(o.Tarobiy.Anim, "death_kneel", new AnimOpt { BirMarta = true, Majburiy = true });
        if (!olimKlipi)
        {
            Anim.Toxtat(o.Tarobiy.Anim); // he must not keep talking lying down
            Vector3 r = o.Tarobiy.Mesh.Rotation;
            Actors.JsRotation(o.Tarobiy.Mesh, Math.PI / 2.1, r.Y, r.Z);
            o.Tarobiy.Mesh.Position = new Vector3(o.Tarobiy.Mesh.Position.X, 0.28f, o.Tarobiy.Mesh.Position.Z);
        }

        o.Tarobiy.Nur.Visible = false;
        BuxoroAudio.S("olim");

        // 1. the locked slot goes black
        if (o.Chizma["kamon"] == "qulf")
        {
            o.Chizma["kamon"] = "olik";
        }

        o.Chizma["qilich"] = "olik";
        BuxoroHud.Chizmalar(o.Chizma);

        // 2. the music stops — for the last four minutes only wind, hammer, steps, iron
        BuxoroAudio.Musiqa(false);

        // 3. one line on screen, 3 seconds
        BuxoroHud.Gap("", G("olim"), 3);

        // the horse: where Tarobiy fell, riderless — unless the hero already rides one
        if (o.Ot == null)
        {
            OtYarat(o, o.Tarobiy.X + 2.5, o.Tarobiy.Z + 1.5);
        }
    }

    /// <summary>game.js otYarat (web 2026-10-19).</summary>
    private static Node3D OtYarat(OyinHolati o, double x, double z)
    {
        Node3D ot = BuxoroModels.Ol("ot") ?? Actors.Ot(0x8a7355);
        ot.Position = new Vector3((float)x, (float)World.YerBalandligi(x, z), (float)z);
        o.Scene.AddChild(ot);
        o.Ot = ot;
        o.OtAnim = Anim.Ulash(ot, "ot");
        Anim.Oynat(o.OtAnim, "horse_idle");
        return ot;
    }

    /// <summary>
    /// game.js otgaMindir (web 2026-10-19): when the cavalry wave comes the hero
    /// rides too — the horse appears beside him and he mounts at once. It replaces
    /// the Act III horse (no second one is made).
    /// </summary>
    private static void OtgaMindir(OyinHolati o)
    {
        Oyinchi p = o.Oyinchi;
        if (p.Otda || o.Tugadi != null)
        {
            return;
        }

        if (o.Ot == null)
        {
            OtYarat(o, p.X, p.Z);
        }

        Player.OtMin(p, o.Ot!);
        o.OtMinilgan = true;
        p.EUshlandi = 0;
        Anim.Oynat(o.OtAnim, "horse_mounted_idle");
        BuxoroHud.Toast(T("otdasiz"));
    }

    // ================= WAVES =================

    /// <summary>game.js:513-539.</summary>
    private static void TolqinYangila(OyinHolati o)
    {
        if (o.TolqinIndeks >= B.Tolqin.Count)
        {
            if (o.Dushmanlar.Count == 0 && o.Humo.Holat == HumoHolat.Uchdi)
            {
                Humo.HolatBer(o.Humo, HumoHolat.Qongan);
            }

            return;
        }

        BuxoroWaveRow t = B.Tolqin[o.TolqinIndeks];

        // Humo spreads its wings — a 15 s warning
        if (!o.TolqinOgoh && o.Vaqt <= t.Vaqt + B.HumoOgohlantirish)
        {
            o.TolqinOgoh = true;
            Humo.HolatBer(o.Humo, HumoHolat.Ogoh);
        }

        if (o.Vaqt <= t.Vaqt)
        {
            o.TolqinIndeks++;
            o.TolqinOgoh = false;
            Humo.HolatBer(o.Humo, HumoHolat.Uchdi);
            o.Dushmanlar.AddRange(Combat.TolqinChiqar(o.Scene, t));

            // riders are coming — the hero rides too (web 2026-10-19)
            if (t.Otliq > 0)
            {
                OtgaMindir(o);
            }

            // in the 3rd wave Humo does not land (§10)
        }
        else if (o.Dushmanlar.Count == 0 && o.TolqinIndeks > 0 && o.Humo.Holat == HumoHolat.Uchdi && o.TolqinIndeks < 3)
        {
            Humo.HolatBer(o.Humo, HumoHolat.Qongan);
        }
    }

    // ================= EVENTS =================

    /// <summary>game.js:542-574 — the single damage seam.</summary>
    private static void Hodisa(OyinHolati o, Hodisa h)
    {
        if (h.Tur == "dushmanOldi")
        {
            o.OlganDushman++;
        }
        else if (h.Tur == "devorBuzildi")
        {
            if (!o.DevorMaslahati) // the first time — a tip about rebuilding (band 10)
            {
                o.DevorMaslahati = true;
                BuxoroHud.Gap("", G("devorBuzildi"), 6);
            }
        }
        else if (h.Tur == "dushmanUrdi")
        {
            double zarar = h.Dushman!.Spec.Zarar;
            if (h.Maqsad == o.Oyinchi)
            {
                if (o.Oyinchi.Bloklaymi)
                {
                    zarar *= 1 - B.QalqonBlok;
                    BuxoroAudio.S("blok");
                }
                else
                {
                    BuxoroAudio.S("zarba");
                }

                o.Oyinchi.Hp -= zarar;
                BuxoroHud.Qizar();
                if (o.Oyinchi.Hp <= 0 && o.Tugadi == null)
                {
                    o.Oyinchi.Olgan = true;
                    Yakunla(o, "olim");
                }
            }
            else if (h.Maqsad is Askar a)
            {
                a.Hp -= zarar;
                BuxoroAudio.S("zarba");
                if (a.Hp <= 0 && !a.Olgan)
                {
                    a.Olgan = true;
                    if (!Anim.Oynat(a.Anim, "die", new AnimOpt { BirMarta = true, Majburiy = true }))
                    {
                        Anim.Toxtat(a.Anim);
                        Vector3 r = a.Mesh.Rotation;
                        Actors.JsRotation(a.Mesh, Math.PI / 2, r.Y, r.Z);
                        a.Mesh.Position = new Vector3(a.Mesh.Position.X, 0.25f, a.Mesh.Position.Z);
                    }
                }
            }
        }
    }

    // ================= ENDINGS (§11) =================

    /// <summary>game.js:589-612.</summary>
    public static void Yakunla(OyinHolati o, string tur)
    {
        o.Tugadi = new Tugash { Tur = tur };
        Input.MouseMode = Input.MouseModeEnum.Visible;
        BuxoroHud.Korsatma(null);
        BuxoroHud.Ushla(null);
        BuxoroHud.Gap(null);
        BuxoroHud.Jonlar(Array.Empty<JonBar>());

        if (tur == "olim")
        {
            // Shomurod falls. The fight goes on — the world does not look at him.
            Anim.Oynat(o.Oyinchi.Anim, "die", new AnimOpt { BirMarta = true, Majburiy = true });
            BuxoroAudio.S("olim");
            BuxoroAudio.Musiqa(false);
            BuxoroAudio.Shamol(0.30, 2);
            Humo.HolatBer(o.Humo, HumoHolat.Ketdi);
        }
        else if (tur == "galaba")
        {
            Humo.HolatBer(o.Humo, HumoHolat.Qongan);
            BuxoroAudio.Musiqa(true, 1.35); // the Act I melody, slower
            BuxoroAudio.Shamol(0.10, 3);
        }
        else
        {
            Humo.HolatBer(o.Humo, HumoHolat.Ketdi);
            BuxoroAudio.Shamol(0.28, 2);

            // the gate swings open
            Darvoza d = o.Dunyo.Darvoza!;
            d.ChapTavaqa.Rotation = new Vector3(d.ChapTavaqa.Rotation.X, -1.1f, d.ChapTavaqa.Rotation.Z);
            d.OngTavaqa.Rotation = new Vector3(d.OngTavaqa.Rotation.X, 1.1f, d.OngTavaqa.Rotation.Z);
        }
    }

    /// <summary>game.js:614-645.</summary>
    private static void YakunYangila(OyinHolati o, double dt)
    {
        Tugash tg = o.Tugadi!;
        tg.Taymer += dt;
        double t = tg.Taymer;

        Camera3D kam = o.Camera;
        Oyinchi p = o.Oyinchi;
        if (tg.Tur == "olim")
        {
            // looks at the fallen hero — it does not slowly rise
            var m = new Vector3((float)(p.X - 3.5), (float)(2.2 + Math.Min(3, t * 0.5)), (float)(p.Z + 3));
            kam.Position = kam.Position.Lerp(m, (float)(1 - Math.Pow(0.05, dt)));
            kam.LookAt(new Vector3((float)p.X, 0.5f, (float)p.Z), Vector3.Up);
        }
        else
        {
            var maqsad = new Vector3((float)(p.X - 4), (float)(6 + Math.Min(26, t * 3.2)), (float)p.Z);
            kam.Position = kam.Position.Lerp(maqsad, Smoothing.EndingCamera(dt));
            kam.LookAt(o.Dunyo.HumoTayanchi!.Position, Vector3.Up);
        }

        Humo.Yangila(o.Humo, dt);
        Combat.Yangila(o.Scene, dt, o.Dushmanlar, o.Askarlar, o.Dunyo, o.Oyinchi, true, o.Oqlar, _ => { });

        if (t > 3.5 && !tg.Korsatildi)
        {
            tg.Korsatildi = true;

            // the ending video first if there is one, then the stats
            if (!Videolar.Sabab(tg.Tur, () => KadrKorsat(o)))
            {
                KadrKorsat(o);
            }
        }
    }

    /// <summary>game.js:647-671 — the stats card.</summary>
    private static void KadrKorsat(OyinHolati o)
    {
        bool galaba = o.Tugadi!.Tur == "galaba";
        bool olim = o.Tugadi.Tur == "olim";
        int d = Math.Max(0, (int)Math.Floor(o.Vaqt / 60));
        int s = Math.Max(0, (int)Math.Ceiling(o.Vaqt % 60));

        string katta = G(galaba ? "galaba" : olim ? "olim" : "maglubiyat");
        string matn = olim ? G("olim1") + "<br>" + G("olim2")
                           : G(galaba ? "gal1" : "mag1") + "<br>" + G(galaba ? "gal2" : "mag2");
        string stat =
            G("sDevor") + ": " + o.QurilganDevor + " / " + B.DevorSeksiyaSoni + "\n" +
            G("sQurol") + ": " + o.YasalganQurol + "\n" +
            G("sShogird") + ": " + o.Shogirdlar.Shogirdlar.Count + " / " + B.ShogirdMaksimum + "\n" +
            G("sDushman") + ": " + o.OlganDushman + " / " + B.JamiDushman +
            (galaba ? "\n" + G("sVaqt") + ": " + d + ":" + (s < 10 ? "0" : "") + s : "");
        BuxoroHud.Kadr(katta, matn, stat, G("qayta"));
    }
}
