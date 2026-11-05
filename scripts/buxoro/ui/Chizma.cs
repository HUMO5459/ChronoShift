using System;
using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// The blueprint viewer, chizma.js — when a weapon's drawing opens it spins up
/// close in a corner window. Scenario §9: "the game does not stop for a second",
/// so this is NOT modal: the game runs on behind it. §10's "No one gave you this
/// drawing" moment is shown here, or it would be just panel text.
/// </summary>
/// <remarks>
/// The web draws a second scene into a scissored corner of the same canvas after
/// the main render (chizma.js:118-140). Here that scene lives in its own World3D
/// inside a transparent SubViewport, shown by a TextureRect under the HUD (the
/// canvas sits below every DOM layer); the #chizmaKorik captions (z-index 15) are
/// a separate layer above the HUD.
/// </remarks>
public partial class Chizma : CanvasLayer
{
    public static Chizma? Instance { get; private set; }

    private const float Kenglik = 0.30f; // share of the screen width
    private const float Nisbat = 0.75f;  // height / width

    private static readonly Color Oltin = new("#f2c14e");
    private static readonly Color Krem2 = new("#e8dfc9");

    private sealed record Malumot(string Nom, string Retsept, string Izoh);

    private static readonly Dictionary<string, Malumot> Malumotlar = new()
    {
        ["kaltak"] = new("KALTAK", "", ""),
        ["kamon"] = new("KAMON", "YOG‘OCH ×2 + TERI ×1", ""),
        ["qilich"] = new("QILICH", "TEMIR ×3 + KO‘MIR ×1", "Bu chizmani hech kim bermadi."),
        ["galvir_qalqon"] = new("G‘ALVIR-QALQON", "", ""),
    };

    /// <summary>Starting angle: the object begins from its most recognisable side (three.js XYZ Euler).</summary>
    private static readonly Dictionary<string, (double X, double Y, double Z)> Burilish = new()
    {
        ["kamon"] = (0.10, Math.PI / 2, 0),  // the limb's profile, not its edge
        ["qilich"] = (0.14, 0.55, 0.18),
        ["kaltak"] = (0.10, 0.6, 0.35),
        ["galvir_qalqon"] = (0.05, (Math.PI / 2) - 0.5, 0),
    };

    private SubViewport _vp = null!;
    private TextureRect _oyna = null!;
    private Node3D _tayanch = null!;
    private Label _nom = null!, _retsept = null!, _izoh = null!;
    private FontVariation _nomShrift = null!, _retseptShrift = null!;

    private string? _joriy;
    private double _taymer, _kirish, _tayanchY;

    // CSS transitions: #chizmaKorik opacity .45s; em opacity .6s after .8s
    private bool _yon, _izohli;
    private float _alfa, _emAlfa;
    private double _emOzgardi;

    /// <summary>Creates the viewer under the episode root (chizma.js builds it lazily on first korsat).</summary>
    public static Chizma Ulash(Node ota)
    {
        var c = new Chizma { Name = "Chizma", Layer = 15, ProcessMode = ProcessModeEnum.Always };
        ota.AddChild(c);
        return c;
    }

    public override void _Ready()
    {
        Instance = this;
        Qur();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>chizma.js:16-41 — the little scene, its camera and lights, and the caption layer.</summary>
    private void Qur()
    {
        BuxoroWorld w = BuxoroRuntime.W;
        float ekspozitsiya = BuxoroEkran.Ekspozitsiya(w.Ekspozitsiya);
        float k = w.YoruglikKoef; // the same gl_compatibility light scale as the field

        _vp = new SubViewport
        {
            Name = "ChizmaSahna", OwnWorld3D = true, TransparentBg = true,
            Msaa3D = Viewport.Msaa.Msaa4X, RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled,
            Size = new Vector2I(64, 48),
        };
        AddChild(_vp);

        // The shared renderer tone-maps this pass too (main.js:17-19); no fog in this scene.
        var env = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.ClearColor,
            ReflectedLightSource = Godot.Environment.ReflectionSource.Disabled,
            TonemapMode = Godot.Environment.ToneMapper.Aces,
            TonemapWhite = BuxoroEkran.Oq,
            TonemapExposure = ekspozitsiya,
        };

        // HemisphereLight(0xf0e6d0, 0x3a3226, 1.0): irradiance = mix(ground, sky, ½ + ½·n.y).
        // Godot has none, so: ambient = the mean, plus ½(sky − ground)·n.y from a light
        // straight down and a NEGATIVE one straight up — exact for diffuse, no specular.
        Color osmon = Lin(0xf0e6d0), yer = Lin(0x3a3226);
        Color orta = (osmon + yer) * 0.5f, yarim = (osmon - yer) * 0.5f;
        env.AmbientLightSource = Godot.Environment.AmbientSource.Color;
        env.AmbientLightColor = orta.LinearToSrgb();
        env.AmbientLightEnergy = 1.0f * k;
        _vp.AddChild(new WorldEnvironment { Environment = env });
        foreach ((float burchak, bool manfiy) in new[] { (-90f, false), (90f, true) })
        {
            _vp.AddChild(new DirectionalLight3D
            {
                RotationDegrees = new Vector3(burchak, 0, 0), LightColor = yarim.LinearToSrgb(),
                LightEnergy = k, LightNegative = manfiy, LightSpecular = 0,
            });
        }

        _vp.AddChild(Nur(0xfff2dc, 1.6f * k, new Vector3(2.2f, 3, 3)));
        _vp.AddChild(Nur(0x8ea0c0, 0.5f * k, new Vector3(-2, 1, -2.5f))); // back light — so the edges read

        // PerspectiveCamera(38, 1/NISBAT, 0.05, 20) at (0, 0, 2.4)
        _vp.AddChild(new Camera3D { Fov = 38, Near = 0.05f, Far = 20, Position = new Vector3(0, 0, 2.4f), Current = true });

        _tayanch = new Node3D { Name = "tayanch", RotationOrder = EulerOrder.Xyz };
        _vp.AddChild(_tayanch);

        // The 3D window is part of the canvas — under the HUD (z-index 10).
        var ost = new CanvasLayer { Layer = 9 };
        AddChild(ost);
        _oyna = new TextureRect
        {
            Texture = _vp.GetTexture(), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale, MouseFilter = Control.MouseFilterEnum.Ignore,
            Visible = false,
            Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.PremultAlpha },
        };
        ost.AddChild(_oyna);

        // DOM layer: <div id="chizmaKorik"><b></b><i></i><em></em></div>
        var shrift = new SystemFont { FontNames = new[] { "Georgia", "Times New Roman", "serif" } };
        _nomShrift = new FontVariation { BaseFont = shrift };
        _retseptShrift = new FontVariation { BaseFont = shrift };
        var kursiv = new SystemFont { FontNames = shrift.FontNames, FontItalic = true };
        _nom = Yorliq(_nomShrift, Oltin, 0.9f);
        _retsept = Yorliq(_retseptShrift, Krem2, 0.9f);
        _izoh = Yorliq(kursiv, Krem2, 0.95f);
    }

    private static Color Lin(uint hex) => new((hex >> 16 & 0xff) / 255f, (hex >> 8 & 0xff) / 255f, (hex & 0xff) / 255f);

    /// <summary>A three.js DirectionalLight at <paramref name="joy"/>, aimed at the origin.</summary>
    private static DirectionalLight3D Nur(uint hex, float kuch, Vector3 joy) => new()
    {
        LightColor = Lin(hex).LinearToSrgb(), LightEnergy = kuch,
        Transform = new Transform3D(Basis.Identity, joy).LookingAt(Vector3.Zero, Vector3.Up),
    };

    private Label Yorliq(Font f, Color rang, float soya)
    {
        var l = new Label
        {
            MouseFilter = Control.MouseFilterEnum.Ignore, FocusMode = Control.FocusModeEnum.None,
            HorizontalAlignment = HorizontalAlignment.Center, Modulate = new Color(1, 1, 1, 0),
        };
        l.AddThemeFontOverride("font", f);
        l.AddThemeColorOverride("font_color", rang);
        l.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, soya));
        AddChild(l);
        return l;
    }

    // ================= API (window.CHIZMA) =================

    /// <summary>chizma.js:52-102. nom: 'kamon' | 'qilich' | 'kaltak' | 'galvir_qalqon'.</summary>
    public static void Korsat(string nom, double soniya)
    {
        Chizma? c = Instance;
        if (c == null)
        {
            return;
        }

        foreach (Node n in BuxoroModels.Bolalar(c._tayanch))
        {
            c._tayanch.RemoveChild(n);
            n.QueueFree();
        }

        Node3D? m = null;
        if (nom == "kamon")
        {
            m = new Node3D();
            m.AddChild(Qurollar.Kamon());
            Node3D o = Qurollar.Oq();
            o.Rotation = new Vector3(0, 0, -0.12f);
            o.Position = new Vector3(0.17f, -0.02f, 0.03f);
            m.AddChild(o);
        }
        else if (nom == "qilich")
        {
            m = Qurollar.Qilich();
        }
        else if (nom == "kaltak")
        {
            m = Qurollar.Kaltak();
        }
        else if (nom == "galvir_qalqon")
        {
            m = Qurollar.Qalqon();
        }

        if (m == null)
        {
            return;
        }

        // Centre and fit the window. Mirrors the web exactly, including that the
        // centre offset is applied UNSCALED while the scale is 1.35/size.
        if (BuxoroModels.Olcham(m, Transform3D.Identity, out Aabb q))
        {
            m.Position -= q.GetCenter();
            float eng = Math.Max(q.Size.X, Math.Max(q.Size.Y, q.Size.Z));
            m.Scale = Vector3.One * (1.35f / (eng > 0 ? eng : 1));
        }

        c._tayanch.AddChild(m);
        c._tayanchY = 0;
        Actors.JsRotation(c._tayanch, 0, 0, 0);

        (double X, double Y, double Z) br = Burilish.TryGetValue(nom, out var b) ? b : (0, 0, 0);
        Actors.JsRotation(m, br.X, br.Y, br.Z);

        c._joriy = nom;
        c._taymer = soniya > 0 ? soniya : 7;
        c._kirish = 0;

        Malumot d = Malumotlar.TryGetValue(nom, out Malumot? md) ? md : new Malumot(nom, "", "");
        c._nom.Text = d.Nom;
        c._retsept.Text = d.Retsept;
        c._izoh.Text = d.Izoh;
        c.Yon(true, d.Izoh.Length > 0);
    }

    /// <summary>chizma.js:104-111 — runs with the game (main.js:291), so it freezes in pause.</summary>
    public static void Yangila(double dt)
    {
        Chizma? c = Instance;
        if (c?._joriy == null)
        {
            return;
        }

        c._taymer -= dt;
        c._kirish = Math.Min(1, c._kirish + (dt * 2.2));

        // turns slowly and sways a little — as if held in a hand
        c._tayanchY += dt * 0.75;
        double x = (Math.Sin(Time.GetTicksMsec() * 0.0006) * 0.13) - 0.06;
        Actors.JsRotation(c._tayanch, x, c._tayanchY, 0);
        if (c._taymer <= 0)
        {
            Yashir();
        }
    }

    /// <summary>chizma.js:113-116.</summary>
    public static void Yashir()
    {
        Chizma? c = Instance;
        if (c == null)
        {
            return;
        }

        c._joriy = null;
        c.Yon(false, c._izohli);
    }

    /// <summary>chizma.js:142.</summary>
    public static bool Ochiqmi() => Instance?._joriy != null;

    // ================= frame =================

    private void Yon(bool yon, bool izohli)
    {
        bool emOldin = _yon && _izohli;
        _yon = yon;
        _izohli = izohli;
        if ((_yon && _izohli) != emOldin)
        {
            _emOzgardi = 0; // the em transition restarts, with its .8 s delay
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        // chizma.js:121 — the scene is drawn only while open and past the first frames
        bool chiz = _joriy != null && _kirish > 0.02;
        _oyna.Visible = chiz;
        _vp.RenderTargetUpdateMode = chiz ? SubViewport.UpdateMode.Always : SubViewport.UpdateMode.Disabled;

        _alfa = Yaqinlash(_alfa, _yon ? 1 : 0, dt / 0.45f);
        _emOzgardi += delta;
        if (_emOzgardi >= 0.8)
        {
            _emAlfa = Yaqinlash(_emAlfa, _yon && _izohli ? 0.85f : 0, dt / 0.6f);
        }

        _nom.Modulate = new Color(1, 1, 1, _alfa);
        _retsept.Modulate = new Color(1, 1, 1, _alfa * 0.8f);
        _izoh.Modulate = new Color(1, 1, 1, _alfa * _emAlfa);

        Joylash();
    }

    private static float Yaqinlash(float a, float b, float qadam) => a < b ? Math.Min(b, a + qadam) : Math.Max(b, a - qadam);

    /// <summary>chizma.js:123-131 for the window, styles.css:240-263 for the captions.</summary>
    private void Joylash()
    {
        Vector2 ekran = GetViewport().GetVisibleRect().Size;
        float v = Math.Min(ekran.X, ekran.Y) / 100f;

        float w = Mathf.Round(ekran.X * Kenglik), h = Mathf.Round(w * Nisbat);
        _oyna.Position = new Vector2(Mathf.Round(ekran.X * 0.085f), Mathf.Round((ekran.Y * 0.5f) - (h * 0.5f)));
        _oyna.Size = new Vector2(w, h);

        // render at the window's real pixel size, not the 1920x1080 canvas size
        Vector2 pencere = GetTree().Root.Size;
        float olchov = Math.Max(0.25f, Math.Min(pencere.X / ekran.X, pencere.Y / ekran.Y));
        var px = new Vector2I(Math.Max(8, (int)Math.Round(w * olchov)), Math.Max(6, (int)Math.Round(h * olchov)));
        if (_vp.Size != px)
        {
            _vp.Size = px;
        }

        // #chizmaKorik: left 8.5 %, width 30 % (body.sensorli: 6 %, 38 % — the window itself stays), centred on the screen's middle;
        // b 2.6vmin (.22em spacing, margin 11.5vmin), i 1.9vmin (.1em, margin .5vmin),
        // em 1.9vmin italic (margin 1.4vmin). An empty block has no height.
        Olcham(_nom, 2.6f * v, v);
        Olcham(_retsept, 1.9f * v, v);
        Olcham(_izoh, 1.9f * v, v);
        _nomShrift.SpacingGlyph = (int)Math.Round(0.22f * 2.6f * v);
        _retseptShrift.SpacingGlyph = (int)Math.Round(0.1f * 1.9f * v);

        float hb = Bal(_nom), hi = Bal(_retsept), he = Bal(_izoh);
        float jami = (11.5f * v) + hb + (0.5f * v) + hi + (1.4f * v) + he;
        bool sensorli = Touch.Bormi();
        float x = ekran.X * (sensorli ? 0.06f : 0.085f), kenglik = ekran.X * (sensorli ? 0.38f : Kenglik);
        float y = (ekran.Y * 0.5f) - (jami * 0.5f) + (11.5f * v);
        _nom.Position = new Vector2(x, y);
        y += hb + (0.5f * v);
        _retsept.Position = new Vector2(x, y);
        y += hi + (1.4f * v);
        _izoh.Position = new Vector2(x, y);
        foreach (Label l in new[] { _nom, _retsept, _izoh })
        {
            l.Size = new Vector2(kenglik, l.GetCombinedMinimumSize().Y);
        }
    }

    private static void Olcham(Label l, float px, float v)
    {
        l.AddThemeFontSizeOverride("font_size", Math.Max(6, (int)Math.Round(px)));
        l.AddThemeConstantOverride("shadow_offset_y", Math.Max(1, (int)Math.Round(0.2f * v)));
        l.AddThemeConstantOverride("shadow_outline_size", Math.Max(1, (int)Math.Round(v)));
    }

    private static float Bal(Label l) => l.Text.Length == 0 ? 0 : l.GetCombinedMinimumSize().Y;
}
