using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace ChronoShift;

/// <summary>One soldier HP bar in screen pixels (hud.js:149).</summary>
public readonly record struct JonBar(float X, float Y, float Nisbat);

/// <summary>
/// The HUD — §3: on screen only the CLOCK and the HANDS; everything else faint or
/// transient. hud.js + styles.css:13-186, built as Controls in code.
/// </summary>
/// <remarks>
/// Every size is in vmin (1 % of the shorter viewport side), as in styles.css, so
/// 1920x1080 and 1280x720 keep identical proportions. The font is the system
/// Georgia / Times serif the web build names. No HUD control takes focus, so the
/// arrows and Space never become UI navigation (correction 2).
/// </remarks>
public partial class BuxoroHud : CanvasLayer
{
    public static BuxoroHud? Instance { get; private set; }

    private static string _til = "uz";

    private static readonly Dictionary<string, Dictionary<string, string>> Matn = new()
    {
        ["uz"] = new()
        {
            ["belgila"] = "E — BELGILA", ["kutma"] = "Kutma. Keyingisiga yugur.", ["qur"] = "F — QUR",
            ["yolla"] = "E — USHLAB TUR (yolla)", ["min"] = "E — OT MIN", ["yasa"] = "F — YASA",
            ["devorTurdi"] = "devor turdi", ["kamonTayyor"] = "kamon tayyor",
            ["qilichTayyor"] = "qilich tayyor", ["shogirdQoshildi"] = "shogird qo‘shildi",
            ["resursYetmadi"] = "resurs yetmadi", ["navbatTola"] = "belgi navbati to‘la",
            ["tosh"] = "tosh", ["yogoch"] = "yog‘och", ["teri"] = "teri", ["temir"] = "temir", ["komir"] = "ko‘mir",
            ["kaltak"] = "KALTAK", ["kamon"] = "KAMON", ["qilich"] = "QILICH",
            ["jon"] = "JON", ["haliYoq"] = "hali yo‘q",
            ["kamonOzimga"] = "kamon sizda — 2 bilan tanlang",
            ["otdasiz"] = "otliqlar keldi — siz ham otdasiz",
        },
        ["en"] = new()
        {
            ["belgila"] = "E — MARK", ["kutma"] = "Don't wait. Run to the next.", ["qur"] = "F — BUILD",
            ["yolla"] = "E — HOLD (recruit)", ["min"] = "E — MOUNT", ["yasa"] = "F — CRAFT",
            ["devorTurdi"] = "wall raised", ["kamonTayyor"] = "bow ready",
            ["qilichTayyor"] = "sword ready", ["shogirdQoshildi"] = "apprentice joined",
            ["resursYetmadi"] = "not enough", ["navbatTola"] = "mark queue full",
            ["tosh"] = "stone", ["yogoch"] = "wood", ["teri"] = "leather", ["temir"] = "iron", ["komir"] = "coal",
            ["kaltak"] = "CLUB", ["kamon"] = "BOW", ["qilich"] = "SWORD",
            ["jon"] = "HP", ["haliYoq"] = "not yet",
            ["kamonOzimga"] = "bow is yours — press 2",
            ["otdasiz"] = "riders are coming — you are mounted too",
        },
    };

    private static readonly Dictionary<string, Color> ResursRangi = new()
    {
        ["tosh"] = new Color("#8d8579"), ["yogoch"] = new Color("#7d6039"), ["teri"] = new Color("#a07a4e"),
        ["temir"] = new Color("#6d4a35"), ["komir"] = new Color("#2b2724"),
    };

    private static readonly Color Krem = new("#f0e6cd");
    private static readonly Color Krem2 = new("#e8dfc9");
    private static readonly Color Qizil = new("#d9573a");

    /// <summary>hud.js:31 — a translated string, or the key itself.</summary>
    public static string T(string k) => Matn[_til].TryGetValue(k, out string? v) ? v : k;

    public static string Til => _til;

    // ---- nodes ----
    private Control _ildiz = null!;
    private Label _soat = null!;
    private Control _qollar = null!;
    private VBoxContainer _resurslar = null!;
    private VBoxContainer _chizmalar = null!;
    private VBoxContainer _toastlar = null!;
    private Label _korsatma = null!;
    private ColorRect _ushla = null!, _ushlaBar = null!;
    private HBoxContainer _jon = null!;
    private Label _jonYorliq = null!, _jonSon = null!;
    private ColorRect _jonFon = null!, _jonBar = null!;
    private Control _jonlar = null!;
    private RichTextLabel _gap = null!;
    private Label _tilEl = null!;
    private Label _maslahat = null!;
    private ColorRect _oqarish = null!;
    private Font _shrift = null!;
    private TextureRect? _qizar;
    private Tween? _qizarTween;
    private CanvasLayer? _kadrQatlam;

    // ---- state ----
    private int _oxirgiSoat = -1;
    private bool _soatXavf;
    private Tween? _shiftTween;
    private float _soatShiftY, _soatShiftS = 1;
    private string _oxirgiResurslar = "";
    private Dictionary<string, string> _chizmaHolat = new() { ["kaltak"] = "ochiq", ["kamon"] = "qulf", ["qilich"] = "qulf" };
    private string _faolQurol = "kaltak";
    private (int Soni, bool[] Band) _qollarHolat = (0, Array.Empty<bool>());
    private int _qollarMaks = 6;
    private int _oxirgiJon = -1;
    private bool _jonXavf;
    private float _korsatmaAlfa, _ushlaAlfa, _gapAlfa, _maslahatAlfa;
    private bool _korsatmaYon, _ushlaYon, _gapYon, _maslahatYon;
    private ulong _gapGacha;
    private readonly List<(Label L, double T)> _toastRoyxat = new();
    private readonly List<JonBar> _jonBarlar = new();

    private float Vmin => Math.Min(GetViewport().GetVisibleRect().Size.X, GetViewport().GetVisibleRect().Size.Y) / 100f;

    /// <summary>hud.js:36-52.</summary>
    public static BuxoroHud Ulash(Node ota)
    {
        var h = new BuxoroHud { Name = "BuxoroHud", Layer = 10, ProcessMode = ProcessModeEnum.Always };
        ota.AddChild(h);
        return h;
    }

    public override void _Ready()
    {
        Instance = this;
        _shrift = new SystemFont { FontNames = new[] { "Georgia", "Times New Roman", "serif" } };

        _ildiz = new Control { MouseFilter = Control.MouseFilterEnum.Ignore, FocusMode = Control.FocusModeEnum.None };
        _ildiz.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_ildiz);

        _soat = YangiLabel(Krem);
        _qollar = new QollarCtrl(this) { MouseFilter = Control.MouseFilterEnum.Ignore };
        _ildiz.AddChild(_qollar);
        _resurslar = new VBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore, Modulate = new Color(1, 1, 1, 0.88f) };
        _ildiz.AddChild(_resurslar);
        _chizmalar = new VBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        _ildiz.AddChild(_chizmalar);
        _toastlar = new VBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore, Alignment = BoxContainer.AlignmentMode.End };
        _ildiz.AddChild(_toastlar);
        _korsatma = YangiLabel(Krem2);
        _ushla = new ColorRect { Color = new Color(Krem, 0.16f), MouseFilter = Control.MouseFilterEnum.Ignore };
        _ushlaBar = new ColorRect { Color = Krem2, MouseFilter = Control.MouseFilterEnum.Ignore };
        _ushla.AddChild(_ushlaBar);
        _ildiz.AddChild(_ushla);

        _jon = new HBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore, Modulate = new Color(1, 1, 1, 0.85f) };
        _jonYorliq = YangiLabel(Krem, _jon);
        _jonFon = new ColorRect { Color = new Color(Krem, 0.16f), MouseFilter = Control.MouseFilterEnum.Ignore, SizeFlagsVertical = Control.SizeFlags.ShrinkCenter };
        _jonBar = new ColorRect { Color = Krem2, MouseFilter = Control.MouseFilterEnum.Ignore };
        _jonFon.AddChild(_jonBar);
        _jon.AddChild(_jonFon);
        _jonSon = YangiLabel(Krem, _jon);
        _ildiz.AddChild(_jon);

        _jonlar = new JonlarCtrl(this) { MouseFilter = Control.MouseFilterEnum.Ignore };
        _jonlar.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _ildiz.AddChild(_jonlar);

        _gap = new RichTextLabel
        {
            BbcodeEnabled = true, FitContent = true, ScrollActive = false, MouseFilter = Control.MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        _ildiz.AddChild(_gap);

        _tilEl = YangiLabel(Krem2);
        _tilEl.MouseFilter = Control.MouseFilterEnum.Stop; // the one clickable HUD element (hud.js:44)
        _tilEl.GuiInput += e =>
        {
            if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            {
                _til = _til == "uz" ? "en" : "uz";
                _tilEl.Text = _til == "uz" ? "UZ / en" : "uz / EN";
                _jonYorliq.Text = T("jon");
                ChizmalarQayta();
                _oxirgiResurslar = "";
            }
        };
        _tilEl.Text = "UZ / en";
        _jonYorliq.Text = T("jon");
        _jonSon.Text = "140";

        _maslahat = YangiLabel(Krem2);
        _maslahat.Text = "Bosing — kursor yashirinadi, kamera erkin buriladi";

        _oqarish = new ColorRect { Color = new Color(1, 1, 1, 0), MouseFilter = Control.MouseFilterEnum.Ignore };
        _oqarish.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        var oqLayer = new CanvasLayer { Layer = 30 };
        oqLayer.AddChild(_oqarish);
        AddChild(oqLayer);

        ChizmalarQayta();
        Joylash();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private Label YangiLabel(Color rang, Node? ota = null)
    {
        var l = new Label { MouseFilter = Control.MouseFilterEnum.Ignore, FocusMode = Control.FocusModeEnum.None };
        l.AddThemeFontOverride("font", _shrift);
        l.AddThemeColorOverride("font_color", rang);
        l.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.85f));
        l.AddThemeConstantOverride("shadow_offset_y", 2);
        l.AddThemeConstantOverride("shadow_outline_size", 6);
        (ota ?? _ildiz).AddChild(l);
        return l;
    }

    private static void Shrift(Control c, float px)
    {
        int s = Math.Max(6, (int)Math.Round(px));
        if (c is RichTextLabel)
        {
            c.AddThemeFontSizeOverride("normal_font_size", s);
            c.AddThemeFontSizeOverride("italics_font_size", s); // #gap's text is all [i]
        }
        else
        {
            c.AddThemeFontSizeOverride("font_size", s);
        }
    }

    // ================= layout (styles.css) =================

    private void Joylash()
    {
        float v = Vmin;
        Vector2 ekran = GetViewport().GetVisibleRect().Size;

        // body.sensorli (styles.css:231-236): out of the touch buttons' way
        bool sensorli = Touch.Bormi();

        // #soat — top 2.5vmin, centred, 6.4vmin
        Shrift(_soat, 6.4f * v);
        _soat.Size = _soat.GetCombinedMinimumSize();
        _soat.PivotOffset = _soat.Size / 2;
        _soat.Scale = Vector2.One * _soatShiftS;
        _soat.Position = new Vector2((ekran.X - _soat.Size.X) / 2, (2.5f * v) + _soatShiftY);

        // #qollar — top 11.5vmin, centred
        _qollar.Size = new Vector2((_qollarMaks * 2.5f * v) + ((_qollarMaks - 1) * 1.1f * v), 2.5f * v);
        _qollar.Position = new Vector2((ekran.X - _qollar.Size.X) / 2, 11.5f * v);

        // #resurslar — right 2.5vmin, top 6.5vmin
        _resurslar.AddThemeConstantOverride("separation", (int)(0.9f * v));
        _resurslar.Size = _resurslar.GetCombinedMinimumSize();
        _resurslar.Position = new Vector2(ekran.X - (2.5f * v) - _resurslar.Size.X, 6.5f * v);

        // #chizmalar — left 2.5vmin, vertically centred
        _chizmalar.AddThemeConstantOverride("separation", (int)(1.1f * v));
        _chizmalar.Size = _chizmalar.GetCombinedMinimumSize();
        _chizmalar.Position = new Vector2(2.5f * v, (ekran.Y - _chizmalar.Size.Y) / 2);
        _chizmalar.PivotOffset = new Vector2(0, _chizmalar.Size.Y / 2); // sensorli: scale(.85) from left centre
        _chizmalar.Scale = Vector2.One * (sensorli ? 0.85f : 1f);

        // #toastlar — bottom 9vmin (sensorli 26), centred
        _toastlar.AddThemeConstantOverride("separation", (int)(0.6f * v));
        _toastlar.Size = _toastlar.GetCombinedMinimumSize();
        _toastlar.Position = new Vector2((ekran.X - _toastlar.Size.X) / 2, ekran.Y - ((sensorli ? 26 : 9) * v) - _toastlar.Size.Y);

        // #korsatma — bottom 15vmin (sensorli 32), 2.5vmin
        Shrift(_korsatma, 2.5f * v);
        _korsatma.Size = _korsatma.GetCombinedMinimumSize();
        _korsatma.Position = new Vector2((ekran.X - _korsatma.Size.X) / 2, ekran.Y - ((sensorli ? 32 : 15) * v) - _korsatma.Size.Y);

        // #ushla — bottom 21vmin (sensorli 38), 22vmin x 0.8vmin
        _ushla.Size = new Vector2(22 * v, 0.8f * v);
        _ushla.Position = new Vector2((ekran.X - _ushla.Size.X) / 2, ekran.Y - ((sensorli ? 38 : 21) * v) - _ushla.Size.Y);
        _ushlaBar.Size = new Vector2(_ushlaBar.Size.X, _ushla.Size.Y);

        // #jon — left 2.5vmin, bottom 2.5vmin (sensorli: top 2.5vmin, clear of the joystick)
        Shrift(_jonYorliq, 1.8f * v);
        Shrift(_jonSon, 1.8f * v);
        _jon.AddThemeConstantOverride("separation", (int)v);
        _jonFon.CustomMinimumSize = new Vector2(22 * v, 1.1f * v);
        _jonSon.CustomMinimumSize = new Vector2(4 * v, 0);
        _jon.Size = _jon.GetCombinedMinimumSize();
        _jon.Position = new Vector2(2.5f * v, sensorli ? 2.5f * v : ekran.Y - (2.5f * v) - _jon.Size.Y);
        _jonBar.Size = new Vector2(_jonBar.Size.X, 1.1f * v);

        // #gap — bottom 5vmin, max 62vmin, 2.3vmin italic (sensorli 3vmin, 78vmin, 2.8vmin)
        float gapEn = (sensorli ? 78 : 62) * v;
        Shrift(_gap, (sensorli ? 2.8f : 2.3f) * v);
        _gap.Size = new Vector2(gapEn, _gap.Size.Y);
        _gap.Position = new Vector2((ekran.X - gapEn) / 2, ekran.Y - ((sensorli ? 3 : 5) * v) - _gap.GetContentHeight());

        // #til — top right 2.5vmin, 1.8vmin
        Shrift(_tilEl, 1.8f * v);
        _tilEl.Size = _tilEl.GetCombinedMinimumSize();
        _tilEl.Position = new Vector2(ekran.X - (2.5f * v) - _tilEl.Size.X, 2.5f * v);

        // #kameraMaslahat — bottom 4.5vmin, 2.1vmin
        Shrift(_maslahat, 2.1f * v);
        _maslahat.Size = _maslahat.GetCombinedMinimumSize();
        _maslahat.Position = new Vector2((ekran.X - _maslahat.Size.X) / 2, ekran.Y - (4.5f * v) - _maslahat.Size.Y);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        // CSS transitions: opacity eases toward its class state
        _korsatmaAlfa = Yaqinlash(_korsatmaAlfa, _korsatmaYon ? 0.92f : 0, dt / 0.35f);
        _ushlaAlfa = Yaqinlash(_ushlaAlfa, _ushlaYon ? 1 : 0, dt / 0.2f);
        if (_gapYon && Time.GetTicksMsec() >= _gapGacha)
        {
            _gapYon = false;
        }

        _gapAlfa = Yaqinlash(_gapAlfa, _gapYon ? 0.95f : 0, dt / 0.5f);
        _maslahatAlfa = Yaqinlash(_maslahatAlfa, _maslahatYon ? 0.9f : 0, dt / 0.4f);
        _korsatma.Modulate = new Color(1, 1, 1, _korsatmaAlfa);
        _ushla.Modulate = new Color(1, 1, 1, _ushlaAlfa);
        _gap.Modulate = new Color(1, 1, 1, _gapAlfa);
        _maslahat.Modulate = new Color(1, 1, 1, _maslahatAlfa);
        _tilEl.Modulate = new Color(1, 1, 1, _tilEl.GetGlobalRect().HasPoint(_tilEl.GetGlobalMousePosition()) ? 0.85f : 0.45f);

        // @keyframes puls — 1.6 s between opacity 1 and 0.45
        float puls = 0.725f + (0.275f * Mathf.Cos((float)(Time.GetTicksMsec() / 1000.0 * Mathf.Tau / 1.6)));
        _soat.Modulate = new Color(1, 1, 1, _soatXavf ? puls : 1);
        _jon.Modulate = new Color(1, 1, 1, _jonXavf ? 0.85f * puls : 0.85f);

        // toasts: in .25 s, hold, out .5 s from 1.5 s, removed at 2 s
        for (int i = _toastRoyxat.Count - 1; i >= 0; i--)
        {
            (Label l, double t) = _toastRoyxat[i];
            t += delta;
            _toastRoyxat[i] = (l, t);
            float a = t < 0.25 ? (float)(t / 0.25) : t < 1.5 ? 1 : (float)Math.Max(0, 1 - ((t - 1.5) / 0.5));
            l.Modulate = new Color(1, 1, 1, a);
            if (t >= 2.0)
            {
                l.QueueFree();
                _toastRoyxat.RemoveAt(i);
            }
        }

        Joylash();
    }

    private static float Yaqinlash(float a, float b, float qadam) => a < b ? Math.Min(b, a + qadam) : Math.Max(b, a - qadam);

    // ================= API (window.HUD) =================

    /// <summary>hud.js:56-63 — whole seconds, red and pulsing at one minute.</summary>
    public static void Soat(double qolgan)
    {
        if (Instance == null)
        {
            return;
        }

        int butun = Math.Max(0, (int)Math.Ceiling(qolgan));
        if (butun == Instance._oxirgiSoat)
        {
            return;
        }

        Instance._oxirgiSoat = butun;
        int d = butun / 60, s = butun % 60;
        Instance._soat.Text = "⏳ " + d + ":" + (s < 10 ? "0" : "") + s;
        Instance._soatXavf = butun <= 60;
        Instance._soat.AddThemeColorOverride("font_color", Instance._soatXavf ? Qizil : Krem);
    }

    /// <summary>hud.js:66-71 — the ChronoShift moment: the clock jumps back.</summary>
    public static void VaqtQoshildi()
    {
        BuxoroHud? h = Instance;
        if (h == null)
        {
            return;
        }

        // @keyframes shift: up 1.6vmin and 1.22x at 35 %, back by 0.55 s
        h._shiftTween?.Kill();
        h._shiftTween = h.CreateTween();
        h._shiftTween.TweenMethod(Callable.From<float>(t =>
        {
            float k = t < 0.35f ? t / 0.35f : 1 - ((t - 0.35f) / 0.65f);
            h._soatShiftY = -1.6f * h.Vmin * k;
            h._soatShiftS = 1 + (0.22f * k);
        }), 0f, 1f, 0.55f).SetEase(Tween.EaseType.Out);

        // @keyframes oqar: white flash 0.82 → 0 in 0.45 s
        h._oqarish.Color = new Color(1, 1, 1, 0.82f);
        h.CreateTween().TweenProperty(h._oqarish, "color:a", 0f, 0.45f).SetEase(Tween.EaseType.Out);
    }

    /// <summary>hud.js:74-81 — one circle per possible apprentice: busy filled, missing faint.</summary>
    public static void Qollar(IReadOnlyList<Shogird> royxat, int maksimum)
    {
        if (Instance == null)
        {
            return;
        }

        var band = new bool[royxat.Count];
        for (int i = 0; i < royxat.Count; i++)
        {
            band[i] = !royxat[i].Bosh;
        }

        Instance._qollarHolat = (royxat.Count, band);
        Instance._qollarMaks = maksimum;
        Instance._qollar.QueueRedraw();
    }

    /// <summary>hud.js:85-93 — only the current act's resources, count + NAME.</summary>
    public static void Resurslar(Dictionary<string, int> ombor, IReadOnlyList<string> korinadigan)
    {
        BuxoroHud? h = Instance;
        if (h == null)
        {
            return;
        }

        string kalit = _til + string.Join(",", korinadigan) + ":" + string.Join(",", Array.ConvertAll(new List<string>(korinadigan).ToArray(), k => ombor.TryGetValue(k, out int n) ? n : 0));
        if (kalit == h._oxirgiResurslar)
        {
            return; // called every frame — rebuild only on change
        }

        h._oxirgiResurslar = kalit;
        foreach (Node c in BuxoroModels.Bolalar(h._resurslar))
        {
            c.QueueFree();
        }

        float v = h.Vmin;
        foreach (string k in korinadigan)
        {
            var qator = new HBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
            qator.AddThemeConstantOverride("separation", (int)(0.8f * v));
            qator.AddChild(new ColorRect
            {
                Color = ResursRangi[k], CustomMinimumSize = new Vector2(1.9f * v, 1.9f * v),
                SizeFlagsVertical = Control.SizeFlags.ShrinkCenter, MouseFilter = Control.MouseFilterEnum.Ignore,
            });
            Label son = h.YangiLabel(Krem, qator);
            son.Text = (ombor.TryGetValue(k, out int n) ? n : 0).ToString();
            son.HorizontalAlignment = HorizontalAlignment.Right;
            son.CustomMinimumSize = new Vector2(2.8f * v, 0);
            Shrift(son, 2.4f * v);
            Label nom = h.YangiLabel(new Color(Krem, 0.8f), qator);
            nom.Text = T(k);
            Shrift(nom, 2.4f * v);
            h._resurslar.AddChild(qator);
        }
    }

    /// <summary>hud.js:97-100.</summary>
    public static void Chizmalar(Dictionary<string, string>? holat)
    {
        if (Instance == null)
        {
            return;
        }

        if (holat != null)
        {
            Instance._chizmaHolat = new Dictionary<string, string>(holat);
        }

        Instance.ChizmalarQayta();
    }

    /// <summary>hud.js:110-114 (the .faol class has no CSS rule — no visual change).</summary>
    public static void Qurol(string nom)
    {
        if (Instance == null || Instance._faolQurol == nom)
        {
            return;
        }

        Instance._faolQurol = nom;
        Instance.ChizmalarQayta();
    }

    /// <summary>hud.js:104-108 + styles.css:68-86 — the blueprint strip.</summary>
    private void ChizmalarQayta()
    {
        foreach (Node c in BuxoroModels.Bolalar(_chizmalar))
        {
            c.QueueFree();
        }

        float v = Vmin;
        string[] nomlar = { "kaltak", "kamon", "qilich" };
        for (int i = 0; i < 3; i++)
        {
            string holat = _chizmaHolat[nomlar[i]];
            bool olik = holat == "olik";
            var sb = new StyleBoxFlat
            {
                BgColor = olik ? new Color("#0a0908") : new Color(20 / 255f, 17 / 255f, 12 / 255f, 0.42f),
                BorderColor = olik ? new Color("#0a0908") : holat == "ochiq" ? new Color(Krem, 0.72f) : new Color(Krem, 0.28f),
                ContentMarginTop = 0.8f * v, ContentMarginBottom = 0.8f * v, ContentMarginLeft = v, ContentMarginRight = v,
            };
            sb.SetBorderWidthAll(Math.Max(1, (int)(0.22f * v)));
            sb.SetCornerRadiusAll((int)(0.5f * v));
            var panel = new PanelContainer { MouseFilter = Control.MouseFilterEnum.Ignore, CustomMinimumSize = new Vector2(9.5f * v, 0) };
            panel.AddThemeStyleboxOverride("panel", sb);
            panel.Modulate = new Color(1, 1, 1, holat == "qulf" ? 0.42f : 1);
            var l = new Label
            {
                Text = (i + 1) + "  " + T(nomlar[i]) + (holat == "qulf" ? " ⌾" : ""),
                HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            l.AddThemeFontOverride("font", _shrift);
            l.AddThemeFontSizeOverride("font_size", Math.Max(6, (int)(1.55f * v)));
            l.AddThemeColorOverride("font_color", olik ? new Color("#0a0908") : holat == "ochiq" ? Krem : Krem2);
            panel.AddChild(l);
            _chizmalar.AddChild(panel);
        }
    }

    /// <summary>hud.js:117-122.</summary>
    public static void Toast(string matn)
    {
        BuxoroHud? h = Instance;
        if (h == null)
        {
            return;
        }

        float v = h.Vmin;
        var sb = new StyleBoxFlat { BgColor = new Color(16 / 255f, 13 / 255f, 9 / 255f, 0.62f), ContentMarginLeft = 1.4f * v, ContentMarginRight = 1.4f * v, ContentMarginTop = 0.5f * v, ContentMarginBottom = 0.5f * v };
        sb.SetCornerRadiusAll((int)(0.4f * v));
        var l = new Label { Text = matn, MouseFilter = Control.MouseFilterEnum.Ignore, HorizontalAlignment = HorizontalAlignment.Center, Modulate = new Color(1, 1, 1, 0) };
        l.AddThemeStyleboxOverride("normal", sb);
        l.AddThemeFontOverride("font", h._shrift);
        l.AddThemeFontSizeOverride("font_size", Math.Max(6, (int)(2.1f * v)));
        l.AddThemeColorOverride("font_color", Krem2);
        l.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        h._toastlar.AddChild(l);
        h._toastRoyxat.Add((l, 0));
    }

    /// <summary>hud.js:125-128.</summary>
    public static void Korsatma(string? matn)
    {
        if (Instance == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(matn))
        {
            Instance._korsatma.Text = matn;
            Instance._korsatmaYon = true;
        }
        else
        {
            Instance._korsatmaYon = false;
        }
    }

    /// <summary>hud.js:131-135 — the hold bar; null hides it.</summary>
    public static void Ushla(double? nisbat)
    {
        if (Instance == null)
        {
            return;
        }

        if (nisbat == null)
        {
            Instance._ushlaYon = false;
            return;
        }

        Instance._ushlaYon = true;
        Instance._ushlaBar.Size = new Vector2((float)(Math.Min(1, nisbat.Value) * Instance._ushla.Size.X), Instance._ushla.Size.Y);
    }

    /// <summary>hud.js:139-146 — red and pulsing at 30 %.</summary>
    public static void Jon(double hp, double max)
    {
        BuxoroHud? h = Instance;
        if (h == null)
        {
            return;
        }

        int butun = Math.Max(0, (int)Math.Ceiling(hp));
        if (butun == h._oxirgiJon)
        {
            return;
        }

        h._oxirgiJon = butun;
        float nisbat = (float)Math.Max(0, Math.Min(1, butun / max));
        h.CreateTween().TweenProperty(h._jonBar, "size:x", nisbat * h._jonFon.CustomMinimumSize.X, 0.25f); // transition: width .25s
        h._jonSon.Text = butun.ToString();
        h._jonXavf = butun <= max * 0.3;
        h._jonBar.Color = h._jonXavf ? Qizil : Krem2;
    }

    /// <summary>hud.js:150-166 — soldier bars, screen px.</summary>
    public static void Jonlar(IReadOnlyList<JonBar> royxat)
    {
        if (Instance == null)
        {
            return;
        }

        Instance._jonBarlar.Clear();
        Instance._jonBarlar.AddRange(royxat);
        Instance._jonlar.QueueRedraw();
    }

    /// <summary>
    /// hud.js:170-176 — a line of dialogue: the speaker small on top, the line
    /// italic. The source strings carry &lt;b&gt; and &lt;br&gt;; both are mapped to BBCode.
    /// </summary>
    public static void Gap(string? kim, string? matn = null, double davomiylik = 5)
    {
        BuxoroHud? h = Instance;
        if (h == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(matn))
        {
            h._gapYon = false;
            return;
        }

        float v = h.Vmin;
        string bb = Regex.Replace(matn, "<br\\s*/?>", "\n").Replace("<b>", "[b]").Replace("</b>", "[/b]");
        string bosh = string.IsNullOrEmpty(kim)
            ? ""
            : $"[font_size={(int)(1.8f * v)}][color=#e8dfc999]{kim}[/color][/font_size]\n";
        h._gap.Text = "[center]" + bosh + "[i]" + bb + "[/i][/center]";
        h._gap.AddThemeFontOverride("normal_font", h._shrift);
        h._gap.AddThemeFontOverride("italics_font", new FontVariation { BaseFont = h._shrift, VariationTransform = new Transform2D(1, 0, 0.2f, 1, 0, 0) });
        h._gap.AddThemeColorOverride("default_color", Krem2);
        h._gap.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.95f));
        h._gap.AddThemeConstantOverride("shadow_outline_size", 6);
        h._gapYon = true;
        h._gapGacha = Time.GetTicksMsec() + (ulong)(davomiylik * 1000);
    }

    /// <summary>main.js:302-314 — the "click to look around" hint.</summary>
    public static void KameraMaslahat(bool korsat)
    {
        if (Instance != null)
        {
            Instance._maslahatYon = korsat;
        }
    }

    /// <summary>
    /// game.js:577-586 — the damage vignette: an inset red glow (box-shadow inset
    /// 18vmin rgba(160,30,20,.75)) shown at once, faded out over 0.45 s after 90 ms.
    /// </summary>
    public static void Qizar()
    {
        BuxoroHud? h = Instance;
        if (h == null)
        {
            return;
        }

        if (h._qizar == null)
        {
            var g = new Gradient();
            g.SetColor(0, new Color(160 / 255f, 30 / 255f, 20 / 255f, 0));
            g.SetOffset(0, 0.62f);
            g.SetColor(1, new Color(160 / 255f, 30 / 255f, 20 / 255f, 0.75f));
            g.SetOffset(1, 1.0f);
            h._qizar = new TextureRect
            {
                Texture = new GradientTexture2D
                {
                    Gradient = g, Fill = GradientTexture2D.FillEnum.Radial,
                    FillFrom = new Vector2(0.5f, 0.5f), FillTo = new Vector2(1.08f, 0.5f), Width = 256, Height = 256,
                },
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.Scale,
                MouseFilter = Control.MouseFilterEnum.Ignore, Modulate = new Color(1, 1, 1, 0),
            };
            h._qizar.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            var qatlam = new CanvasLayer { Layer = 25 };
            qatlam.AddChild(h._qizar);
            h.AddChild(qatlam);
        }

        h._qizarTween?.Kill();
        h._qizar.Modulate = new Color(1, 1, 1, 1);
        h._qizarTween = h.CreateTween();
        h._qizarTween.TweenInterval(0.09);
        h._qizarTween.TweenProperty(h._qizar, "modulate:a", 0f, 0.45f);
    }

    /// <summary>
    /// The #kadr frame (styles.css:159-177): a dark full-screen card for the
    /// opening line and the ending stats, fading in and out over 1.2 s. With a
    /// button label, a RESTART button reloads the episode.
    /// </summary>
    public static void Kadr(string? katta, string matn, string? stat = null, string? tugma = null)
    {
        BuxoroHud? h = Instance;
        if (h == null)
        {
            return;
        }

        h._kadrQatlam?.QueueFree();
        float v = h.Vmin;
        h._kadrQatlam = new CanvasLayer { Layer = 40 };
        var ildiz = new Control { Modulate = new Color(1, 1, 1, 0) };
        ildiz.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        var fon = new ColorRect { Color = new Color("#0a0806") };
        fon.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        ildiz.AddChild(fon);

        var markaz = new CenterContainer();
        markaz.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        var ust = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        markaz.AddChild(ust);
        ildiz.AddChild(markaz);

        if (!string.IsNullOrEmpty(katta))
        {
            Label k = h.YangiLabel(Krem2, ust);
            k.Text = katta;
            k.HorizontalAlignment = HorizontalAlignment.Center;
            Shrift(k, 5.5f * v);
            ust.AddChild(new Control { CustomMinimumSize = new Vector2(0, 2 * v) });
        }

        var m = new RichTextLabel
        {
            BbcodeEnabled = true, FitContent = true, ScrollActive = false, CustomMinimumSize = new Vector2(70 * v, 0),
            Text = "[center][i]" + Regex.Replace(matn, "<br\\s*/?>", "\n").Replace("<b>", "[b]").Replace("</b>", "[/b]") + "[/i][/center]",
            Modulate = new Color(1, 1, 1, 0.92f), MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        m.AddThemeFontOverride("normal_font", h._shrift);
        m.AddThemeFontOverride("italics_font", new FontVariation { BaseFont = h._shrift, VariationTransform = new Transform2D(1, 0, 0.2f, 1, 0, 0) });
        m.AddThemeFontSizeOverride("normal_font_size", (int)(2.8f * v));
        m.AddThemeFontSizeOverride("italics_font_size", (int)(2.8f * v));
        m.AddThemeConstantOverride("line_separation", (int)(2.5f * v));
        m.AddThemeColorOverride("default_color", Krem2);
        ust.AddChild(m);

        if (!string.IsNullOrEmpty(stat))
        {
            ust.AddChild(new Control { CustomMinimumSize = new Vector2(0, 4 * v) });
            Label st = h.YangiLabel(new Color(Krem2, 0.7f), ust);
            st.Text = stat;
            st.HorizontalAlignment = HorizontalAlignment.Center;
            st.AddThemeConstantOverride("line_spacing", (int)(1.2f * v));
            Shrift(st, 2.1f * v);
        }

        if (!string.IsNullOrEmpty(tugma))
        {
            ust.AddChild(new Control { CustomMinimumSize = new Vector2(0, 4 * v) });
            var b = new Button { Text = tugma, SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter, FocusMode = Control.FocusModeEnum.None };
            var sb = new StyleBoxFlat { BgColor = new Color(0, 0, 0, 0), BorderColor = new Color(Krem2, 0.5f), ContentMarginLeft = 4 * v, ContentMarginRight = 4 * v, ContentMarginTop = 1.4f * v, ContentMarginBottom = 1.4f * v };
            sb.SetBorderWidthAll(Math.Max(1, (int)(0.22f * v)));
            sb.SetCornerRadiusAll((int)(0.4f * v));
            var sbH = (StyleBoxFlat)sb.Duplicate();
            sbH.BgColor = new Color(Krem2, 0.12f);
            sbH.BorderColor = Krem2;
            b.AddThemeStyleboxOverride("normal", sb);
            b.AddThemeStyleboxOverride("hover", sbH);
            b.AddThemeStyleboxOverride("pressed", sbH);
            b.AddThemeFontOverride("font", h._shrift);
            b.AddThemeFontSizeOverride("font_size", (int)(2.3f * v));
            b.AddThemeColorOverride("font_color", Krem2);
            b.Pressed += () => h.GetTree().ReloadCurrentScene();
            ust.AddChild(b);
        }

        h._kadrQatlam.AddChild(ildiz);
        h.AddChild(h._kadrQatlam);
        h.CreateTween().TweenProperty(ildiz, "modulate:a", 1f, 1.2f); // transition: opacity 1.2s
    }

    /// <summary>For --test-video: the ending card is on screen.</summary>
    public static bool KadrOchiq => Instance?._kadrQatlam != null;

    /// <summary>Fades the #kadr frame out (class 'yoq', 1.2 s).</summary>
    public static void KadrYashir()
    {
        BuxoroHud? h = Instance;
        if (h?._kadrQatlam == null || h._kadrQatlam.GetChildCount() == 0)
        {
            return;
        }

        CanvasLayer q = h._kadrQatlam;
        h._kadrQatlam = null;

        // #kadr.yoq has pointer-events: none from its first moment, fading or not
        foreach (Control c in BuxoroModels.Avlodlar<Control>(q))
        {
            c.MouseFilter = Control.MouseFilterEnum.Ignore;
        }

        Tween tw = h.CreateTween();
        tw.TweenProperty(q.GetChild(0), "modulate:a", 0f, 1.2f);
        tw.TweenCallback(Callable.From(q.QueueFree));
    }

    /// <summary>Hides every transient widget (game.js:592 on an ending).</summary>
    public static void Yashir(bool yashir)
    {
        if (Instance != null)
        {
            Instance._ildiz.Visible = !yashir;
        }
    }

    /// <summary>
    /// body.videoOchiq (styles.css:445): the HUD fades out over .3 s while a video
    /// plays; the transition lives only on that rule, so it comes back at once.
    /// </summary>
    public static void VideoOchiq(bool ochiq)
    {
        BuxoroHud? h = Instance;
        if (h == null)
        {
            return;
        }

        h._videoTween?.Kill();
        if (ochiq)
        {
            h._videoTween = h.CreateTween();
            h._videoTween.TweenProperty(h._ildiz, "modulate:a", 0f, 0.3f);
        }
        else
        {
            h._ildiz.Modulate = Colors.White;
        }
    }

    private Tween? _videoTween;

    // ================= drawn widgets =================

    /// <summary>#qollar — circles, filled when busy (styles.css:40-50).</summary>
    private sealed partial class QollarCtrl : Control
    {
        private readonly BuxoroHud _h;

        public QollarCtrl(BuxoroHud h)
        {
            _h = h;
        }

        public override void _Draw()
        {
            float v = _h.Vmin, d = 2.5f * v, g = 1.1f * v, w = 0.28f * v;
            (int soni, bool[] band) = _h._qollarHolat;
            for (int i = 0; i < _h._qollarMaks; i++)
            {
                var c = new Vector2((i * (d + g)) + (d / 2), d / 2);
                if (i >= soni)
                {
                    DrawArc(c, (d / 2) - (w / 2), 0, Mathf.Tau, 32, new Color(Krem, 0.55f * 0.18f), w, true);
                }
                else if (band[i])
                {
                    DrawCircle(c, d / 2, Krem);
                }
                else
                {
                    DrawArc(c, (d / 2) - (w / 2), 0, Mathf.Tau, 32, new Color(Krem, 0.55f), w, true);
                }
            }
        }
    }

    /// <summary>#jonlar — 6vmin x 0.7vmin bars above nearby soldiers (styles.css:136-145).</summary>
    private sealed partial class JonlarCtrl : Control
    {
        private readonly BuxoroHud _h;

        public JonlarCtrl(BuxoroHud h)
        {
            _h = h;
        }

        public override void _Draw()
        {
            float v = _h.Vmin, w = 6 * v, hh = 0.7f * v;
            foreach (JonBar r in _h._jonBarlar)
            {
                var rect = new Rect2(r.X - (w / 2), r.Y - hh, w, hh);
                DrawRect(rect.Grow(0.18f * v), new Color(Krem, 0.4f));
                DrawRect(rect, new Color(20 / 255f, 17 / 255f, 12 / 255f, 0.55f));
                DrawRect(new Rect2(rect.Position, new Vector2(w * r.Nisbat, hh)), r.Nisbat <= 0.3f ? Qizil : Krem2);
            }
        }
    }
}
