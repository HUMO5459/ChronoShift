using System;
using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// Buxoro 1238's own menu, menyu.js + index.html:83-120 + styles.css:300-392 —
/// BOSHLASH / MISSIYALAR / BOSHQARUV over the blurred kirish video.
/// </summary>
/// <remarks>
/// The web's OUTER level, the CHRONOSHIFT series catalogue (#seriyaMenyu), is
/// ChronoShift's own MainMenu here: it already is that blueprint sheet, and its
/// "1-BO'LIM — BUXORO 1238" row opens this scene. So "← SERIYA" and Escape on
/// the first page return there, where the web showed #seriyaMenyu.
/// The web greys BOSHLASH out as "YUKLANMOQDA…" while the GLBs download; here the
/// same wait covers a threaded preload of the same model list (BuxoroModels.Kutilgan).
/// </remarks>
public partial class Menyu : Control
{
    public const string Sahna = "res://scenes/buxoro/BuxoroMenyu.tscn";
    private const string Epizod = "res://scenes/buxoro/BuxoroEpisode.tscn";
    private const string Seriya = "res://scenes/ui/MainMenu.tscn";

    private static readonly Color Krem = new("#e8dfc9"), Oltin = new("#f2c14e"), Fon = new("#100d09");

    /// <summary>#menyuQoplama — a radial dark centre over a vertical fade (styles.css:312-318).</summary>
    private const string QoplamaShader = @"
shader_type canvas_item;
void fragment() {
    // linear-gradient(to bottom, .70, .35 45%, .80)
    float y = UV.y;
    float al = y < 0.45 ? mix(0.70, 0.35, y / 0.45) : mix(0.35, 0.80, (y - 0.45) / 0.55);
    // radial-gradient(ellipse 62% 55% at 50% 46%, .86 0%, .62 55%, .42 100%) — drawn on top
    float t = length((UV - vec2(0.5, 0.46)) / vec2(0.62, 0.55));
    float ar = t < 0.55 ? mix(0.86, 0.62, t / 0.55) : (t < 1.0 ? mix(0.62, 0.42, (t - 0.55) / 0.45) : 0.42);
    COLOR = vec4(16.0 / 255.0, 13.0 / 255.0, 9.0 / 255.0, 1.0 - (1.0 - al) * (1.0 - ar));
}";

    public sealed record MissiyaMalumot(string Kalit, string NomUz, string NomEn, string TavsifUz, string TavsifEn, string[] MaqsadUz, string[] MaqsadEn);

    /// <summary>
    /// menyu.js:46-110. The scenario is one continuous run (§7), so a "mission" is a
    /// stage to start from. Their clocks and apprentice counts are GameDirector.Missiyalar.
    /// </summary>
    public static readonly MissiyaMalumot[] Missiyalar =
    {
        new("toliq", "TO‘LIQ O‘TISH", "FULL RUN",
            "O‘n daqiqa. Uch parda. Yagona haqiqiy o‘yin.", "Ten minutes. Three acts. The real game.",
            new[] { "Resurslarni BELGILA — o‘zing yig‘ma", "Devor qur — soat orqaga sakraydi", "Bozordan shogird yolla", "24 mo‘g‘ulni to‘xtat" },
            new[] { "MARK resources — do not gather them yourself", "Build walls — the clock jumps backward", "Recruit apprentices at the market", "Stop all 24 Mongols" }),
        new("parda1", "PARDA I — TOSH", "ACT I — STONE",
            "Belgilashni o‘rgan. Tarobiy bir marta aytadi.", "Learn to mark. Tarobiy says it once.",
            new[] { "Tosh tugunlarini belgila", "Birinchi devorni qur", "8:00 da birinchi to‘lqin keladi" },
            new[] { "Mark the stone nodes", "Build the first wall", "The first wave arrives at 8:00" }),
        new("parda2", "PARDA II — KAMON", "ACT II — BOW",
            "Yog‘och ataylab yetmaydi. Tanlashga to‘g‘ri keladi.", "There is deliberately not enough wood. You must choose.",
            new[] { "Yog‘och va teri yig‘", "Askarlarga kamon yasa", "4:00 da Tarobiy o‘ladi" },
            new[] { "Gather wood and leather", "Craft bows for the soldiers", "At 4:00 Tarobiy dies" }),
        new("parda3", "PARDA III — QILICH", "ACT III — SWORD",
            "Endi senga hech kim aytmaydi. Otga min va o‘zing top.", "Now no one will tell you. Mount the horse and work it out.",
            new[] { "Tarobiyning otiga min", "Ustaxonaga bor — qilich chizmasi ochiladi", "Temir 3 qilichga yetadi: o‘zingga yoki askargami?", "2:30 da Eldiz No‘yon keladi" },
            new[] { "Mount Tarobiy’s horse", "Go to the forge — the sword unlocks itself", "Iron makes 3 swords: for you or for a soldier?", "Eldiz Noyon arrives at 2:30" }),
    };

    private readonly Dictionary<string, Control> _sahifalar = new();
    private string _joriy = "bosh", _oldingi = "bosh";
    private Button _mBoshlash = null!, _mMissiya = null!, _mBoshqaruv = null!;
    private Label _mHolat = null!;
    private bool _tayyor, _ketdi;
    private float _v;
    private string? _avtoBoshla;

    private static bool En => BuxoroHud.Til == "en";

    private static string M(string uz, string en) => En ? en : uz;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        SetAnchorsPreset(LayoutPreset.FullRect);
        Vector2 ekran = GetViewport().GetVisibleRect().Size;
        _v = Math.Min(ekran.X, ekran.Y) / 100f;

        // #menyu background, the video, #menyuQoplama
        var fon = new ColorRect { Color = Fon, MouseFilter = MouseFilterEnum.Ignore };
        fon.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(fon);
        MenyuFon.Qosh(this, 1, seriya: false);
        var qoplama = new ColorRect
        {
            MouseFilter = MouseFilterEnum.Ignore,
            Material = new ShaderMaterial { Shader = new Shader { Code = QoplamaShader } },
        };
        qoplama.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(qoplama);

        _sahifalar["bosh"] = BoshQur();
        _sahifalar["missiya"] = MissiyaQur();
        _sahifalar["boshqaruv"] = BoshqaruvQur();

        BuxoroModels.YuklashniBoshla();

        string[] args = OS.GetCmdlineUserArgs();
        _avtoBoshla = Arg(args, "--menyu-boshla"); // DEBUG: start this mission as soon as the menu is ready
        Korsat(Arg(args, "--sahifa") ?? "bosh");
    }

    private static string? Arg(string[] args, string nom)
    {
        int i = Array.IndexOf(args, nom);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    // ================= pages =================

    /// <summary>menyu.js korsat — one page visible.</summary>
    private void Korsat(string nom)
    {
        if (!_sahifalar.ContainsKey(nom))
        {
            nom = "bosh";
        }

        _joriy = nom;
        foreach ((string k, Control s) in _sahifalar)
        {
            s.Visible = k == nom;
        }
    }

    /// <summary>.sahifa — a centred column with 5vmin padding.</summary>
    private VBoxContainer Sahifa()
    {
        var m = new MarginContainer { MouseFilter = MouseFilterEnum.Ignore };
        m.SetAnchorsPreset(LayoutPreset.FullRect);
        foreach (string t in new[] { "left", "right", "top", "bottom" })
        {
            m.AddThemeConstantOverride("margin_" + t, (int)Math.Round(5 * _v));
        }

        var c = new CenterContainer { MouseFilter = MouseFilterEnum.Ignore };
        m.AddChild(c);
        var ust = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore, Alignment = BoxContainer.AlignmentMode.Center };
        ust.AddThemeConstantOverride("separation", 0);
        c.AddChild(ust);
        AddChild(m);
        _sahifaIldiz[ust] = m;
        return ust;
    }

    private readonly Dictionary<VBoxContainer, Control> _sahifaIldiz = new();

    private Control Bosluq(float vmin) => new() { CustomMinimumSize = new Vector2(0, vmin * _v), MouseFilter = MouseFilterEnum.Ignore };

    private static T Markaz<T>(T c)
        where T : Control
    {
        c.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        return c;
    }

    /// <summary>.sahifa nav button / .orqaga — outlined, transparent, hover lifts the fill.</summary>
    private Button Tugma(string matn, float shriftVmin, float minVmin, bool oltin = false)
    {
        float px = shriftVmin * _v;
        var b = new Button
        {
            Text = matn, FocusMode = FocusModeEnum.None, MouseDefaultCursorShape = CursorShape.PointingHand,
            CustomMinimumSize = new Vector2(minVmin * _v, 0),
        };
        b.AddThemeFontOverride("font", MenyuShrift.Georgia(0.16f * px));
        b.AddThemeFontSizeOverride("font_size", (int)Math.Round(px));
        Color rang = oltin ? Oltin : Krem;
        foreach (string k in new[] { "font_color", "font_hover_color", "font_pressed_color", "font_focus_color", "font_disabled_color" })
        {
            b.AddThemeColorOverride(k, rang);
        }

        StyleBoxFlat Uslub(bool ustida)
        {
            var sb = new StyleBoxFlat
            {
                BgColor = ustida ? new Color(Krem, 0.14f) : Colors.Transparent,
                // #mBoshlash's own border colour outranks the :hover rule (id selector)
                BorderColor = oltin ? new Color(Oltin, 0.6f) : ustida ? Krem : new Color(Krem, 0.45f),
                ContentMarginLeft = 5 * _v, ContentMarginRight = 5 * _v, ContentMarginTop = 1.4f * _v, ContentMarginBottom = 1.4f * _v,
            };
            sb.SetBorderWidthAll(Math.Max(1, (int)Math.Round(0.22f * _v)));
            sb.SetCornerRadiusAll((int)Math.Round(0.4f * _v));
            return sb;
        }

        b.AddThemeStyleboxOverride("normal", Uslub(false));
        b.AddThemeStyleboxOverride("hover", Uslub(true));
        b.AddThemeStyleboxOverride("pressed", Uslub(true));
        b.AddThemeStyleboxOverride("disabled", Uslub(false));
        b.AddThemeStyleboxOverride("focus", new StyleBoxEmpty());
        return b;
    }

    /// <summary>:disabled { opacity: .28 }.</summary>
    private static void Ochiq(Button b, bool ochiq)
    {
        b.Disabled = !ochiq;
        b.Modulate = new Color(1, 1, 1, ochiq ? 1 : 0.28f);
    }

    /// <summary>#boshMenyu — index.html:83-98.</summary>
    private Control BoshQur()
    {
        VBoxContainer s = Sahifa();
        s.AddChild(Markaz(MenyuShrift.Yozuv("BUXORO 1238", 7 * _v, Krem, oraliqEm: 0.18f)));
        s.AddChild(Bosluq(1));
        s.AddChild(Markaz(MenyuShrift.Yozuv("CHRONOSHIFT · 001 — TOROBIY", 2.2f * _v, Krem, 0.55f, 0.2f)));
        s.AddChild(Bosluq(4));

        // .shior — italic, line-height 2.1, max 60vmin
        float px = 2.1f * _v;
        Label shior = MenyuShrift.Yozuv("Sen jang qilmaysan — jangni tayyorlaysan.\nYagona resurs — vaqt.", px, Krem, 0.62f, kursiv: true);
        shior.HorizontalAlignment = HorizontalAlignment.Center;
        shior.VerticalAlignment = VerticalAlignment.Center;
        shior.AddThemeConstantOverride("line_spacing", (int)Math.Round((2.1f - 1.14f) * px));
        shior.CustomMinimumSize = new Vector2(0, 2 * 2.1f * px);
        s.AddChild(Markaz(shior));
        s.AddChild(Bosluq(5));

        var nav = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        nav.AddThemeConstantOverride("separation", (int)Math.Round(1.6f * _v));
        _mBoshlash = Tugma(En ? "LOADING…" : "YUKLANMOQDA…", 2.3f, 34, oltin: true);
        _mMissiya = Tugma("MISSIYALAR", 2.3f, 34);
        _mBoshqaruv = Tugma("BOSHQARUV", 2.3f, 34);
        foreach (Button b in new[] { _mBoshlash, _mMissiya, _mBoshqaruv })
        {
            Ochiq(b, false);
            nav.AddChild(b);
        }

        _mBoshlash.Pressed += () => Boshla(Missiyalar[0]);
        _mMissiya.Pressed += () =>
        {
            _oldingi = "bosh";
            Korsat("missiya");
        };
        _mBoshqaruv.Pressed += () =>
        {
            _oldingi = "bosh";
            Korsat("boshqaruv");
        };
        s.AddChild(Markaz(nav));

        s.AddChild(Bosluq(2.5f));
        _mHolat = MenyuShrift.Yozuv("", 1.7f * _v, Krem, 0.38f);
        _mHolat.HorizontalAlignment = HorizontalAlignment.Center;
        s.AddChild(Markaz(_mHolat));

        // data-ga="seriya" — the series level is ChronoShift's MainMenu
        s.AddChild(Bosluq(3));
        Button orqaga = Tugma("← SERIYA", 1.9f, 22);
        orqaga.Pressed += SeriyagaQayt;
        s.AddChild(Markaz(orqaga));
        return _sahifaIldiz[s];
    }

    /// <summary>#missiyaMenyu — the four stage cards in two columns (menyu.js:145-157).</summary>
    private Control MissiyaQur()
    {
        VBoxContainer s = Sahifa();
        s.AddChild(Markaz(MenyuShrift.Yozuv("MISSIYALAR", 3.4f * _v, Oltin, oraliqEm: 0.22f)));
        s.AddChild(Bosluq(3));

        var grid = new GridContainer { Columns = 2, MouseFilter = MouseFilterEnum.Ignore };
        grid.AddThemeConstantOverride("h_separation", (int)Math.Round(2 * _v));
        grid.AddThemeConstantOverride("v_separation", (int)Math.Round(2 * _v));
        float kenglik = (100 - 2) / 2f * _v; // max-width 100vmin, gap 2vmin
        foreach (MissiyaMalumot mi in Missiyalar)
        {
            grid.AddChild(Kartochka(mi, kenglik));
        }

        s.AddChild(Markaz(grid));
        s.AddChild(Bosluq(3));
        Button orqaga = Tugma("← ORQAGA", 1.9f, 22);
        orqaga.Pressed += () => Korsat(_oldingi);
        s.AddChild(Markaz(orqaga));
        return _sahifaIldiz[s];
    }

    /// <summary>.missiya — a clickable card: name, description, objectives (styles.css:368-382).</summary>
    private Control Kartochka(MissiyaMalumot mi, float kenglik)
    {
        StyleBoxFlat Uslub(bool ustida)
        {
            var sb = new StyleBoxFlat
            {
                BgColor = ustida ? new Color(30 / 255f, 25 / 255f, 17 / 255f, 0.75f) : new Color(20 / 255f, 17 / 255f, 12 / 255f, 0.55f),
                BorderColor = ustida ? new Color(Oltin, 0.7f) : new Color(Krem, 0.22f),
                ContentMarginLeft = 2.2f * _v, ContentMarginRight = 2.2f * _v, ContentMarginTop = 2 * _v, ContentMarginBottom = 2 * _v,
            };
            sb.SetBorderWidthAll(Math.Max(1, (int)Math.Round(0.2f * _v)));
            sb.SetCornerRadiusAll((int)Math.Round(0.5f * _v));
            return sb;
        }

        var k = new PanelContainer
        {
            CustomMinimumSize = new Vector2(kenglik, 0), MouseDefaultCursorShape = CursorShape.PointingHand,
            MouseFilter = MouseFilterEnum.Stop,
        };
        StyleBoxFlat oddiy = Uslub(false), ustida = Uslub(true);
        k.AddThemeStyleboxOverride("panel", oddiy);
        k.MouseEntered += () => k.AddThemeStyleboxOverride("panel", ustida);
        k.MouseExited += () => k.AddThemeStyleboxOverride("panel", oddiy);
        k.GuiInput += e =>
        {
            if (e is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false })
            {
                Boshla(mi);
            }
        };

        var ust = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        ust.AddThemeConstantOverride("separation", 0);
        k.AddChild(ust);

        // b — 2.2vmin, .12em, gold, bold
        ust.AddChild(MenyuShrift.Yozuv(M(mi.NomUz, mi.NomEn), 2.2f * _v, Oltin, oraliqEm: 0.12f, vazn: 700));
        ust.AddChild(Bosluq(0.9f));

        // i — 1.75vmin, opacity .7
        Label tavsif = MenyuShrift.Yozuv(M(mi.TavsifUz, mi.TavsifEn), 1.75f * _v, Krem, 0.7f, kursiv: true);
        tavsif.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        ust.AddChild(tavsif);
        ust.AddChild(Bosluq(1.2f));

        // ul li — 1.6vmin, opacity .58, line-height 1.9, a gold '·' at the left
        float px = 1.6f * _v;
        foreach (string m in En ? mi.MaqsadEn : mi.MaqsadUz)
        {
            var li = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            li.AddThemeConstantOverride("separation", 0);
            Label nuqta = MenyuShrift.Yozuv("·", px, Oltin, 0.8f);
            nuqta.CustomMinimumSize = new Vector2(1.6f * _v, 1.9f * px);
            nuqta.VerticalAlignment = VerticalAlignment.Center;
            li.AddChild(nuqta);
            Label matn = MenyuShrift.Yozuv(m, px, Krem, 0.58f);
            matn.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            matn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            matn.CustomMinimumSize = new Vector2(0, 1.9f * px);
            matn.VerticalAlignment = VerticalAlignment.Center;
            matn.AddThemeConstantOverride("line_spacing", (int)Math.Round((1.9f - 1.14f) * px));
            li.AddChild(matn);
            ust.AddChild(li);
        }

        return k;
    }

    /// <summary>#boshqaruvMenyu — index.html:101-120.</summary>
    private Control BoshqaruvQur()
    {
        VBoxContainer s = Sahifa();
        s.AddChild(Markaz(MenyuShrift.Yozuv("BOSHQARUV", 3.4f * _v, Oltin, oraliqEm: 0.22f)));
        s.AddChild(Bosluq(3));
        s.AddChild(Markaz(BoshqaruvJadvali.Qur(_v)));
        s.AddChild(Bosluq(3));
        Button orqaga = Tugma("← ORQAGA", 1.9f, 22);
        orqaga.Pressed += () => Korsat(_oldingi);
        s.AddChild(Markaz(orqaga));
        return _sahifaIldiz[s];
    }

    // ================= flow =================

    public override void _Process(double delta)
    {
        if (_tayyor)
        {
            return;
        }

        double n = BuxoroModels.Yuklanish();
        _mHolat.Text = Math.Round(n * 100) + "%";
        if (n >= 1)
        {
            (int glb, int jami) = BuxoroModels.Hisobot();
            Tayyor(glb == 0 ? "protsedural rejim"
                : glb == jami ? "barcha " + jami + " model yuklandi"
                : glb + " model · qolgan " + (jami - glb) + " tasi protsedural");
        }
    }

    /// <summary>menyu.js:186-198.</summary>
    private void Tayyor(string matn)
    {
        _tayyor = true;
        _mBoshlash.Text = En ? "START" : "BOSHLASH";
        _mHolat.Text = matn;
        foreach (Button b in new[] { _mBoshlash, _mMissiya, _mBoshqaruv })
        {
            Ochiq(b, true);
        }

        GD.Print("[buxoro] menyu tayyor — " + matn);
        if (_avtoBoshla != null)
        {
            Boshla(Array.Find(Missiyalar, x => x.Kalit == _avtoBoshla) ?? Missiyalar[0]);
        }
    }

    /// <summary>menyu.js:159-165 — the chosen stage goes to the game, which fades the menu out.</summary>
    private void Boshla(MissiyaMalumot mi)
    {
        if (_ketdi || !_tayyor)
        {
            return;
        }

        _ketdi = true;
        GameDirector.Missiya = mi.Kalit;
        GameDirector.MenyudanKirdi = true;
        GetTree().ChangeSceneToFile(Epizod);
    }

    private void SeriyagaQayt() => GetTree().ChangeSceneToFile(Seriya);

    /// <summary>menyu.js:176-180 — Escape goes back one level.</summary>
    public override void _UnhandledInput(InputEvent e)
    {
        if (e is not InputEventKey { Pressed: true, Echo: false, PhysicalKeycode: Key.Escape })
        {
            return;
        }

        GetViewport().SetInputAsHandled();
        if (_joriy == "bosh")
        {
            SeriyagaQayt();
        }
        else
        {
            Korsat(_oldingi);
        }
    }
}
