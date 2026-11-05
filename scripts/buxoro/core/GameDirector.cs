using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;

namespace ChronoShift;

/// <summary>
/// Root of BuxoroEpisode.tscn — the port of main.js: bootstrap, the per-frame
/// loop, the opening camera, pause, the camera hint. game.js lives in <see cref="Game"/>.
/// </summary>
/// <remarks>
/// Command-line hooks (after <c>--</c>), the URL parameters of the web build:
///   <c>--skip</c>            no opening shot          (?skip)
///   <c>--t 240</c>           start the clock there    (?t=240)
///   <c>--shogird 6</c>       start with N apprentices (?shogird=6)
///   <c>--missiya parda2</c>  a mission preset (menyu.js MISSIYALAR)
///   <c>--sensorli</c>        touch controls, the mouse as one finger
///   <c>--glbsiz a,b</c>      as if these GLBs were missing (<c>hammasi</c> = all)
/// plus the verification hooks in GameDirector.Tekshiruv.cs.
/// </remarks>
public partial class GameDirector : Node3D
{
    /// <summary>menyu.js:43-100 — key, start clock (null = 600), apprentices.</summary>
    private static readonly Dictionary<string, (double? Vaqt, int Shogird)> Missiyalar = new()
    {
        ["toliq"] = (null, 2),
        ["parda1"] = (600, 2),
        ["parda2"] = (480, 4),
        ["parda3"] = (240, 6),
    };

    /// <summary>The preset the Buxoro menu (Menyu.cs) chose; a direct start leaves it at "toliq".</summary>
    public static string Missiya = "toliq";

    /// <summary>Set by Menyu.cs: the menu fades out over the running game (#menyu.yoq, opacity .8 s).</summary>
    public static bool MenyudanKirdi;

    public Dunyo? Dunyo { get; private set; }

    /// <summary>The game state, game.js's O.</summary>
    public OyinHolati? O { get; private set; }

    private Camera3D _kamera = null!;
    private BuxoroInput? _input;
    private Touch? _sensor;
    private bool _kameraQotgan; // --kamera was given: keep that pose (reference renders)

    // main.js:136-140 — the opening camera move
    private sealed class OchilishHolat
    {
        public double T, Davomiylik = 6.0;
        public Vector3 BoshPoz, BoshQara;
    }

    private OchilishHolat? _ochilish;

    // main.js:178 — pause
    private bool _pauza, _musiqaEdi;
    private CanvasLayer? _pauzaQatlam;
    private Control? _pzJadval;
    private Button? _pauzaTugma;
    private StyleBoxFlat? _ptNormal, _ptUstida;
    private float _ptV = -1;
    private bool _ptTor;

    public override void _Ready()
    {
        if (BuxoroRuntime.Instance == null || !BuxoroRuntime.Instance.IsReady)
        {
            GD.PushError("[buxoro] GameDirector: BuxoroRuntime is not ready — is the autoload registered?");
            return;
        }

        var world = new World(BuxoroRuntime.B, BuxoroRuntime.W);
        var dunyo3d = GetNodeOrNull<Node3D>("Dunyo") ?? this;
        Dunyo = world.Qur(dunyo3d);
        bool ok = SanoqTekshir(world);
        GD.Print($"[buxoro] world built — {Dunyo.Tugunlar.Count} nodes, {Dunyo.Teraklar.Count} poplars, " +
                 $"{Dunyo.Tosiqlar.Count} obstacles, draws {world.Lcg.DrawCount}. {(ok ? "PASS" : "FAIL")}");

        // ---- main.js boshla() / oyinniBoshla() ----
        _kamera = GetNode<Camera3D>("Kamera");
        BuxoroAudio.Boshla(this);
        BuxoroAudio.Shamol(0.22, 2.5);
        BuxoroHud.Ulash(this);
        Chizma.Ulash(this);
        Videolar.Ulash(this);
        Anim.Tozala();
        O = Game.Yarat(dunyo3d, _kamera, Dunyo, world, Dunyo.Muhit!.Environment, Kechik);
        Player.OkklyuziyaTayyorla(Dunyo.Teraklar);
        _input = new BuxoroInput { Name = "BuxoroInput", P = O.Oyinchi };
        AddChild(_input);
        _sensor = Touch.Ulash(this, O, _input);
        BuxoroHud.Chizmalar(O.Chizma);
        BuxoroHud.Soat(O.Vaqt);

        // mission preset (chosen in the menu)
        string[] args = OS.GetCmdlineUserArgs();
        string? argMissiya = ArgQiymat(args, "--missiya");
        string missiya = argMissiya ?? Missiya;
        if (Missiyalar.TryGetValue(missiya, out var mi))
        {
            if (mi.Vaqt is double mv)
            {
                O.Vaqt = mv;
            }

            int n = Math.Min(6, mi.Shogird);
            while (O.Shogirdlar.Shogirdlar.Count < n)
            {
                Apprentices.ShogirdQosh(dunyo3d, O.Shogirdlar, Dunyo);
            }

            O.Missiya = missiya;
            GD.Print($"[buxoro] missiya {missiya}: soat {O.Vaqt:F0} s, shogird {O.Shogirdlar.Shogirdlar.Count}");
        }

        // styles.css:303-306 — the menu (#100d09 under its video) fades out over .8 s while the game already runs
        if (MenyudanKirdi)
        {
            MenyudanKirdi = false;
            var qatlam = new CanvasLayer { Layer = 50 };
            var parda = new ColorRect { Color = new Color("#100d09"), MouseFilter = Control.MouseFilterEnum.Ignore };
            parda.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            qatlam.AddChild(parda);
            AddChild(qatlam);
            Tween tw = CreateTween();
            tw.TweenProperty(parda, "color:a", 0f, 0.8f);
            tw.TweenCallback(Callable.From(qatlam.QueueFree));
        }

        // dev parameters outrank the mission (main.js:92-97)
        if (ArgQiymat(args, "--t") is string ts && double.TryParse(ts, NumberStyles.Float, CultureInfo.InvariantCulture, out double tv))
        {
            O.Vaqt = Math.Max(1, tv);
        }

        if (ArgQiymat(args, "--shogird") is string ss && int.TryParse(ss, out int sv))
        {
            while (O.Shogirdlar.Shogirdlar.Count < Math.Min(6, sv))
            {
                Apprentices.ShogirdQosh(dunyo3d, O.Shogirdlar, Dunyo);
            }
        }

        // the opening shot only on a full run — the practice missions play at once
        bool ochilishKerak = Array.IndexOf(args, "--skip") < 0 && missiya == "toliq";
        if (ochilishKerak)
        {
            Videolar.Sabab("boshlanish");
            OchilishBoshla();
        }
        else
        {
            BuxoroAudio.Musiqa(true);
            Player.KameraniJoylashtir(O.Oyinchi, _kamera);
        }

        KameraArg();
        Input.MouseMode = Input.MouseModeEnum.Visible; // captured on the first click (pointer lock)
        PauzaTugmaQur();

        // DEBUG: -- --chizma qilich — opens the blueprint viewer at once (kamon / qilich / kaltak / galvir_qalqon)
        if (ArgQiymat(args, "--chizma") is string cn)
        {
            Chizma.Korsat(cn, 60);
        }

        if (Array.IndexOf(args, "--test-sensor") >= 0)
        {
            SensorTesti();
            return;
        }

        if (Array.IndexOf(args, "--test-video") >= 0)
        {
            VideoTesti();
            return;
        }

        if (Array.IndexOf(args, "--test-qayta") >= 0)
        {
            QaytaTesti();
            return;
        }

        if (Array.IndexOf(args, "--test-harakat") >= 0)
        {
            GetTree().Quit(HarakatTesti() ? 0 : 1);
            return;
        }

        if (Array.IndexOf(args, "--test-oyin") >= 0)
        {
            GetTree().Quit(OyinTesti() ? 0 : 1);
            return;
        }

        int ci = Array.IndexOf(args, "--solver-compare");
        if (ci >= 0 || Array.IndexOf(args, "--solver-sweep") >= 0)
        {
            List<string> sweep = SolverSweep(Dunyo);
            if (ci >= 0 && ci + 1 < args.Length)
            {
                ok &= SolverCompare(sweep, args[ci + 1]);
            }

            GetTree().Quit(ok ? 0 : 1);
            return;
        }

        if (ArgQiymat(args, "--binolar-compare") is string bp)
        {
            GetTree().Quit(BinolarCompare(world, bp) ? 0 : 1);
        }
    }

    private static string? ArgQiymat(string[] args, string nom)
    {
        int i = Array.IndexOf(args, nom);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    /// <summary>setTimeout: wall-clock, runs through pause like the browser's.</summary>
    private void Kechik(double soniya, Action a)
    {
        if (!IsInsideTree())
        {
            return;
        }

        GetTree().CreateTimer(soniya, processAlways: true, processInPhysics: false, ignoreTimeScale: true).Timeout += () =>
        {
            if (IsInstanceValid(this))
            {
                a();
            }
        };
    }

    // ================= LOOP, main.js:267-297 =================

    /// <summary>
    /// One tick per rendered frame, dt clamped to 1/20 s — run per frame (not at a
    /// fixed physics rate) so the web's per-frame idioms behave as in the browser.
    /// </summary>
    public override void _Process(double delta)
    {
        if (O == null)
        {
            return;
        }

        PauzaTugmaJoyla();

        double dt = Math.Min(delta, 1.0 / 20);
        if (dt <= 0)
        {
            return;
        }

        Anim.Yangila(dt);

        if (_ochilish != null)
        {
            OchilishYangila(dt);
            Humo.Yangila(O.Humo, dt);
        }
        else if (Videolar.Toxtatadimi() || _pauza)
        {
            // a fullscreen video or the pause — the game stops, rendering goes on
        }
        else
        {
            _input?.Oqi();
            Game.Yangila(O, dt);
            if (O.Tugadi == null && !_kameraQotgan)
            {
                Player.Kamera(O.Oyinchi, _kamera, dt);
            }

            _sensor?.Yangila(O.SensorHolat);
            Chizma.Yangila(dt);
            KameraMaslahat();
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        // main.js:251-256 — Escape toggles the pause (a cutscene takes Escape first)
        if (!e.IsActionPressed(BuxoroActions.Pauza) || O == null || O.Tugadi != null || Videolar.Ochiqmi())
        {
            return;
        }

        GetViewport().SetInputAsHandled();
        if (_pauza)
        {
            PauzaYop();
        }
        else
        {
            PauzaOch();
        }
    }

    public override void _Notification(int what)
    {
        // main.js:336-338 — a hidden window plays no sound
        if (what == NotificationApplicationFocusOut && BuxoroAudio.Tayyor)
        {
            BuxoroAudio.OvozYoq(false);
        }
        else if (what == NotificationApplicationFocusIn && BuxoroAudio.Tayyor)
        {
            BuxoroAudio.OvozYoq(true);
        }
    }

    // ================= OPENING SHOT, main.js:129-170 =================

    /// <summary>The camera starts at Humo, crosses the field, stops behind Shomurod. 6 s.</summary>
    private void OchilishBoshla()
    {
        BuxoroHud.Kadr(null, Game.G("ochilish"));
        Vector3 tayanch = O!.Dunyo.HumoTayanchi!.Position;
        _ochilish = new OchilishHolat { BoshPoz = tayanch + new Vector3(-26, 8, 6), BoshQara = tayanch };
        _kamera.LookAtFromPosition(_ochilish.BoshPoz, _ochilish.BoshQara, Vector3.Up);
        Kechik(2.0, () =>
        {
            if (_ochilish != null)
            {
                BuxoroHud.KadrYashir();
            }
        });
    }

    private void OchilishYangila(double dt)
    {
        OchilishHolat oc = _ochilish!;
        Oyinchi p = O!.Oyinchi;
        oc.T += dt;
        double n = Math.Min(1, oc.T / oc.Davomiylik);
        double e = n < 0.5 ? 2 * n * n : 1 - (Math.Pow((-2 * n) + 2, 2) / 2); // easeInOutQuad

        var oxirgiPoz = new Vector3((float)(p.X + (Math.Sin(p.Yonalish) * 6)), (float)(p.Y + 4.5), (float)(p.Z + (Math.Cos(p.Yonalish) * 6)));
        var oxirgiQara = new Vector3((float)p.X, (float)(p.Y + 1.5), (float)p.Z);
        _kamera.Position = oc.BoshPoz.Lerp(oxirgiPoz, (float)e);
        _kamera.LookAt(oc.BoshQara.Lerp(oxirgiQara, (float)e), Vector3.Up);

        // Space skips it
        if (p.SpaceBosildi || n >= 1)
        {
            p.SpaceBosildi = false;
            _ochilish = null;
            BuxoroHud.KadrYashir();
            Player.KameraniJoylashtir(p, _kamera);
            BuxoroAudio.Musiqa(true);
        }
    }

    /// <summary>main.js:302-314 — "click to look" until the cursor is captured, only early on.</summary>
    private void KameraMaslahat()
    {
        bool korsat = !Touch.Bormi() && Input.MouseMode != Input.MouseModeEnum.Captured && O!.Otgan < 12 && O.Tugadi == null;
        BuxoroHud.KameraMaslahat(korsat);
    }

    // ================= PAUSE, main.js:178-249 =================

    /// <summary>
    /// #pauzaTugma, index.html:127 + styles.css:591-600. A touch device has no Esc, so this
    /// button is mandatory there; the web shows it on every device (body.oyinda).
    /// </summary>
    private void PauzaTugmaQur()
    {
        var qatlam = new CanvasLayer { Layer = 60 };
        AddChild(qatlam);
        _pauzaTugma = new Button { Text = "❚❚", TooltipText = "Pauza (Esc)", FocusMode = Control.FocusModeEnum.None };
        _ptNormal = new StyleBoxFlat { BgColor = new Color(16 / 255f, 13 / 255f, 9 / 255f, 0.5f), BorderColor = new Color(0xe8dfc94d) };
        _ptNormal.SetBorderWidthAll(1);
        _ptUstida = (StyleBoxFlat)_ptNormal.Duplicate();
        _ptUstida.BgColor = new Color(30 / 255f, 25 / 255f, 17 / 255f, 0.8f);
        _ptUstida.BorderColor = new Color(0xf2c14e99);
        _pauzaTugma.AddThemeStyleboxOverride("normal", _ptNormal);
        _pauzaTugma.AddThemeStyleboxOverride("hover", _ptUstida);
        _pauzaTugma.AddThemeStyleboxOverride("pressed", _ptUstida);
        _pauzaTugma.AddThemeColorOverride("font_color", new Color("#e8dfc9"));
        _pauzaTugma.Pressed += PauzaOch;
        qatlam.AddChild(_pauzaTugma);
        PauzaTugmaJoyla();
    }

    /// <summary>right 1vmin, bottom 5.6vmin; a small screen 8vmin. Gone during a video and the pause.</summary>
    private void PauzaTugmaJoyla()
    {
        if (_pauzaTugma == null)
        {
            return;
        }

        _pauzaTugma.Visible = !_pauza && !Videolar.Ochiqmi();
        if (!_pauzaTugma.Visible)
        {
            return;
        }

        Vector2 ekran = GetViewport().GetVisibleRect().Size;
        float v = Math.Min(ekran.X, ekran.Y) / 100f;
        bool tor = Touch.TorEkran(this);
        if (v != _ptV || tor != _ptTor)
        {
            _ptV = v;
            _ptTor = tor;
            float px = (tor ? 2.6f : 1.5f) * v;
            foreach (StyleBoxFlat sb in new[] { _ptNormal!, _ptUstida! })
            {
                sb.ContentMarginLeft = sb.ContentMarginRight = (tor ? 1.8f : 1.2f) * v;
                sb.ContentMarginTop = sb.ContentMarginBottom = (tor ? 1.2f : 0.7f) * v;
                sb.SetCornerRadiusAll((int)(0.4f * v));
            }

            _pauzaTugma.AddThemeFontOverride("font", MenyuShrift.Georgia(0.1f * px));
            _pauzaTugma.AddThemeFontSizeOverride("font_size", Math.Max(6, (int)Math.Round(px)));
        }

        _pauzaTugma.Size = _pauzaTugma.GetCombinedMinimumSize();
        _pauzaTugma.Position = new Vector2(ekran.X - v - _pauzaTugma.Size.X, ekran.Y - ((tor ? 8 : 5.6f) * v) - _pauzaTugma.Size.Y);
    }

    private void PauzaOch()
    {
        if (_pauza || O == null || O.Tugadi != null || Videolar.Ochiqmi())
        {
            return;
        }

        _pauza = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        _input?.Tozala(); // held keys and the shield do not stick

        // §9: after Tarobiy's death the music is gone for good — resuming must not bring it back
        _musiqaEdi = BuxoroAudio.MusiqaYoniqmi;
        BuxoroAudio.Musiqa(false);
        PauzaQur();
    }

    private void PauzaYop()
    {
        if (!_pauza)
        {
            return;
        }

        _pauza = false;
        _pauzaQatlam?.QueueFree();
        _pauzaQatlam = null;
        if (_musiqaEdi)
        {
            BuxoroAudio.Musiqa(true);
        }

        if (!Touch.Bormi())
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    /// <summary>The pause card, styles.css:602-640; a small screen has its @media sizes (styles.css:635-640).</summary>
    private void PauzaQur()
    {
        bool en = BuxoroHud.Til == "en";
        string Q(string uz, string ing) => en ? ing : uz;
        float v = Math.Min(GetViewport().GetVisibleRect().Size.X, GetViewport().GetVisibleRect().Size.Y) / 100f;
        var shrift = new SystemFont { FontNames = new[] { "Georgia", "Times New Roman", "serif" } };
        var krem = new Color("#e8dfc9");
        var oltin = new Color("#f2c14e");
        bool tor = Touch.TorEkran(this);

        _pauzaQatlam = new CanvasLayer { Layer = 58 };
        var fon = new ColorRect { Color = new Color(10 / 255f, 8 / 255f, 5 / 255f, 0.82f) };
        fon.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        fon.GuiInput += ev =>
        {
            if (ev is InputEventMouseButton { Pressed: true }) // a click on the backdrop closes it
            {
                PauzaYop();
            }
        };
        _pauzaQatlam.AddChild(fon);

        var markaz = new CenterContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        markaz.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _pauzaQatlam.AddChild(markaz);

        var quti = new PanelContainer();
        var sb = new StyleBoxFlat
        {
            BgColor = new Color(20 / 255f, 16 / 255f, 11 / 255f, 0.94f), BorderColor = new Color(krem, 0.28f),
            ContentMarginLeft = 5.5f * v, ContentMarginRight = 5.5f * v, ContentMarginTop = 4.5f * v, ContentMarginBottom = 4.5f * v,
        };
        sb.SetBorderWidthAll(1);
        quti.AddThemeStyleboxOverride("panel", sb);
        markaz.AddChild(quti);

        var ust = new VBoxContainer();
        ust.AddThemeConstantOverride("separation", (int)(1.3f * v));
        quti.AddChild(ust);

        Label Yozuv(string matn, float px, Color rang)
        {
            var l = new Label { Text = matn, HorizontalAlignment = HorizontalAlignment.Center };
            l.AddThemeFontOverride("font", shrift);
            l.AddThemeFontSizeOverride("font_size", (int)px);
            l.AddThemeColorOverride("font_color", rang);
            return l;
        }

        ust.AddChild(Yozuv(Q("P A U Z A", "P A U S E D"), (tor ? 4.4f : 3) * v, oltin));
        ust.AddChild(new Control { CustomMinimumSize = new Vector2(0, 1.9f * v) });

        Button Tugma(string matn, Action bosildi)
        {
            var b = new Button { Text = matn, CustomMinimumSize = new Vector2((tor ? 52 : 30) * v, 0), FocusMode = Control.FocusModeEnum.None };
            var n = new StyleBoxFlat
            {
                BgColor = new Color(30 / 255f, 25 / 255f, 17 / 255f, 0.55f), BorderColor = new Color(krem, 0.3f),
                ContentMarginLeft = (tor ? 3 : 2.4f) * v, ContentMarginRight = (tor ? 3 : 2.4f) * v,
                ContentMarginTop = (tor ? 2 : 1.3f) * v, ContentMarginBottom = (tor ? 2 : 1.3f) * v,
            };
            n.SetBorderWidthAll(1);
            var h = (StyleBoxFlat)n.Duplicate();
            h.BgColor = new Color(38 / 255f, 31 / 255f, 20 / 255f, 0.8f);
            h.BorderColor = new Color(oltin, 0.8f);
            b.AddThemeStyleboxOverride("normal", n);
            b.AddThemeStyleboxOverride("hover", h);
            b.AddThemeStyleboxOverride("pressed", h);
            b.AddThemeFontOverride("font", shrift);
            b.AddThemeFontSizeOverride("font_size", (int)((tor ? 3 : 2) * v));
            b.AddThemeColorOverride("font_color", krem);
            b.AddThemeColorOverride("font_hover_color", oltin);
            b.Pressed += bosildi;
            ust.AddChild(b);
            return b;
        }

        Tugma(Q("DAVOM ETTIRISH", "RESUME"), PauzaYop);
        Tugma(Q("QAYTA BOSHLASH", "RESTART"), () => GetTree().ReloadCurrentScene());
        Tugma(Q("BOSHQARUV", "CONTROLS"), () =>
        {
            if (_pzJadval != null)
            {
                _pzJadval.Visible = !_pzJadval.Visible;
            }
        });
        Tugma(Q("SERIYA MENYUSI", "SERIES MENU"), () => GetTree().ChangeSceneToFile("res://scenes/ui/MainMenu.tscn"));
        Tugma(Q("O‘YINDAN CHIQISH", "QUIT GAME"), () => GetTree().Quit()); // desktop build: a real quit

        // main.js:229-232 — the menu's controls table, cloned (margin-top 3vmin, styles.css:630)
        var jadval = new VBoxContainer { Visible = false };
        jadval.AddThemeConstantOverride("separation", 0);
        jadval.AddChild(new Control { CustomMinimumSize = new Vector2(0, 3 * v - (1.3f * v)) });
        jadval.AddChild(BoshqaruvJadvali.Qur(v));
        _pzJadval = jadval;
        ust.AddChild(jadval);
        ust.AddChild(Yozuv(Q("Esc — davom ettirish", "Esc — resume"), (tor ? 2.2f : 1.5f) * v, new Color(krem, 0.42f)));
        AddChild(_pauzaQatlam);
    }
}
