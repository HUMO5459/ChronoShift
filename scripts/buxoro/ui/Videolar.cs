using System;
using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// The in-game explanatory videos, videolar.js. Two modes (scenario §9: "No
/// cutscene. No slow-down."):
///   'toxtatib'  — fullscreen, the game PAUSED (an act's start, the endings)
///   'burchakda' — a small window, the game runs on (during a fight)
/// One cue may play SEVERAL videos in a row; each has a SKIP button.
/// </summary>
/// <remarks>
/// The web plays H.264 .mp4; Godot plays only Ogg Theora, so the same files are
/// converted to .ogv (tools/video-ogv.sh). A missing file behaves as the web's
/// 'error' event: the sequence moves on, so the game never waits on a video.
/// </remarks>
public partial class Videolar : CanvasLayer
{
    public static Videolar? Instance { get; private set; }

    private const string Yol = "res://assets/video/buxoro/";

    private sealed record Sahna(string Sabab, string[] Fayllar, string Rejim);

    /// <summary>
    /// The cue table. fayllar — played in order (no extension); sabab — when it
    /// shows; rejim — 'toxtatib' | 'burchakda'.
    /// </summary>
    private static readonly Sahna[] Sahnalar =
    {
        // Opening: 1 -> 2 (the Mongols in the dust) -> 3 (Humo flies away)
        new("boshlanish", new[] { "1", "2", "3" }, "toxtatib"),

        // ACT I over — the stone stage
        new("toshTugadi", new[] { "4" }, "toxtatib"),

        // ACT II over — the bow fights began, Tarobiy took an arrow
        new("tarobiy", new[] { "6" }, "toxtatib"),

        // ACT III — the sword is made, he takes it into the fight
        new("qilichYasaldi", new[] { "5", "7" }, "toxtatib"),

        // Victory: the Mongols withdraw -> Humo returns above the citadel
        new("galaba", new[] { "8", "9" }, "toxtatib"),

        // Defeat: game over -> the commander arrives
        new("maglubiyat", new[] { "game-over", "game-over-1" }, "toxtatib"),
        new("olim", new[] { "game-over", "game-over-1" }, "toxtatib"),
    };

    private static readonly Color Krem2 = new("#e8dfc9");

    // korilgan — each cue plays once per run (the web's page load = one episode here)
    private readonly HashSet<string> _korilgan = new();
    private Sahna? _joriy;
    private readonly Queue<string> _navbat = new();
    private Action? _tugaganda;

    private ColorRect _fon = null!;
    private Panel _burchak = null!;
    private VideoStreamPlayer _video = null!;
    private Label _hisob = null!;
    private Button _tugma = null!;
    private FontVariation _tugmaShrift = null!, _hisobShrift = null!;

    /// <summary>Creates the cutscene layer under the episode root (videolar.js builds it lazily).</summary>
    public static Videolar Ulash(Node ota)
    {
        var v = new Videolar { Name = "Videolar", Layer = 46, ProcessMode = ProcessModeEnum.Always, Visible = false };
        ota.AddChild(v);
        return v;
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

    /// <summary>videolar.js:40-74 — #oyinVideo with its video, .hisob and .otkaz.</summary>
    private void Qur()
    {
        // .toliq — inset 0, black
        _fon = new ColorRect { Color = Colors.Black, MouseFilter = Control.MouseFilterEnum.Ignore };
        _fon.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_fon);

        // .burchak — the window frame (no cue uses it today; kept for the table)
        var ramka = new StyleBoxFlat
        {
            BgColor = new Color("#0a0806"), BorderColor = new Color(Krem2, 0.35f),
            ShadowColor = new Color(0, 0, 0, 0.6f),
        };
        _burchak = new Panel { MouseFilter = Control.MouseFilterEnum.Ignore };
        _burchak.AddThemeStyleboxOverride("panel", ramka);
        AddChild(_burchak);

        _video = new VideoStreamPlayer { Expand = true, MouseFilter = Control.MouseFilterEnum.Ignore };
        _video.Finished += Keyingisi;
        AddChild(_video);

        var shrift = new SystemFont { FontNames = new[] { "Georgia", "serif" } };
        _hisobShrift = new FontVariation { BaseFont = shrift };
        _hisob = new Label { MouseFilter = Control.MouseFilterEnum.Ignore, Modulate = new Color(1, 1, 1, 0.45f) };
        _hisob.AddThemeFontOverride("font", _hisobShrift);
        _hisob.AddThemeColorOverride("font_color", Krem2);
        AddChild(_hisob);

        _tugmaShrift = new FontVariation { BaseFont = shrift };
        _tugma = new Button { FocusMode = Control.FocusModeEnum.None, MouseDefaultCursorShape = Control.CursorShape.PointingHand };
        _tugma.AddThemeFontOverride("font", _tugmaShrift);
        foreach (string k in new[] { "font_color", "font_hover_color", "font_pressed_color", "font_focus_color" })
        {
            _tugma.AddThemeColorOverride(k, Krem2);
        }

        // the button skips the WHOLE sequence, not just the current video
        _tugma.Pressed += Yopish;
        AddChild(_tugma);
    }

    // ================= API (window.VIDEOLAR) =================

    /// <summary>
    /// videolar.js:76-94. True when a video took over; <paramref name="keyin"/> runs
    /// when the sequence ends or is skipped.
    /// </summary>
    public static bool Sabab(string nom, Action? keyin = null)
    {
        Videolar? v = Instance;
        if (v == null || v._joriy != null)
        {
            return false;
        }

        Sahna? s = Array.Find(Sahnalar, x => x.Sabab == nom && !v._korilgan.Contains(x.Sabab));
        if (s == null || s.Fayllar.Length == 0)
        {
            return false;
        }

        v._korilgan.Add(s.Sabab);
        v._joriy = s;
        v._navbat.Clear();
        foreach (string f in s.Fayllar)
        {
            v._navbat.Enqueue(f);
        }

        v._tugaganda = keyin;
        v.Visible = true;
        bool toliq = s.Rejim == "toxtatib";
        v._fon.Visible = toliq;
        v._burchak.Visible = !toliq;
        v._tugma.Visible = true;
        BuxoroHud.VideoOchiq(true); // body.videoOchiq — the HUD fades out

        v.Oynat();
        return true;
    }

    /// <summary>videolar.js:96-121.</summary>
    private void Oynat()
    {
        if (!_navbat.TryDequeue(out string? kalit))
        {
            Yopish();
            return;
        }

        _tugma.Text = BuxoroHud.Til == "en" ? "SKIP ›" : "O‘TKAZIB YUBORISH ›";

        // which one of the sequence
        int jami = _joriy!.Fayllar.Length, n = jami - _navbat.Count;
        _hisob.Text = jami > 1 ? n + " / " + jami : "";
        _hisob.Visible = jami > 1;

        string yol = Yol + kalit + ".ogv";
        VideoStream? oqim = ResourceLoader.Exists(yol) ? ResourceLoader.Load<VideoStream>(yol) : null;
        if (oqim == null)
        {
            // video.onerror → keyingisi, asynchronously as in the browser
            GD.PushWarning($"[buxoro] video {yol} not found — skipped");
            Callable.From(Keyingisi).CallDeferred();
            return;
        }

        _video.Stream = oqim;
        _video.VolumeDb = _joriy.Rejim == "burchakda" ? -80f : 0f; // video.muted
        _video.Play();
    }

    /// <summary>videolar.js:123-127.</summary>
    private void Keyingisi()
    {
        if (_joriy == null)
        {
            return;
        }

        if (_navbat.Count > 0)
        {
            Oynat();
        }
        else
        {
            Yopish();
        }
    }

    /// <summary>videolar.js:129-137 — closes the sequence and runs its callback.</summary>
    public static void Yopish()
    {
        Videolar? v = Instance;
        if (v?._joriy == null)
        {
            return;
        }

        Action? cb = v._tugaganda;
        v._joriy = null;
        v._navbat.Clear();
        v._tugaganda = null;
        v.Visible = false;
        BuxoroHud.VideoOchiq(false);
        v._video.Stop();
        v._video.Stream = null;
        cb?.Invoke();
    }

    /// <summary>videolar.js:139 — a fullscreen video freezes the simulation.</summary>
    public static bool Toxtatadimi() => Instance?._joriy?.Rejim == "toxtatib";

    /// <summary>videolar.js:140.</summary>
    public static bool Ochiqmi() => Instance?._joriy != null;

    /// <summary>For --test-video: "cue n/total", or null when closed.</summary>
    public static string? Holat()
    {
        Videolar? v = Instance;
        if (v?._joriy == null)
        {
            return null;
        }

        int jami = v._joriy.Fayllar.Length;
        return $"{v._joriy.Sabab} {jami - v._navbat.Count}/{jami}" + (v._video.IsPlaying() ? "" : " (to'xtagan)");
    }

    // ================= input (videolar.js:58-73) =================

    /// <summary>
    /// Runs before the game's handlers (the web listens in the capture phase and
    /// stops propagation): Esc skips all, Space/Enter or a click goes to the next video.
    /// </summary>
    public override void _Input(InputEvent e)
    {
        if (_joriy == null)
        {
            return;
        }

        if (e is InputEventKey { Pressed: true, Echo: false } k)
        {
            if (k.PhysicalKeycode == Key.Escape)
            {
                GetViewport().SetInputAsHandled();
                Yopish();
            }
            else if (k.PhysicalKeycode is Key.Space or Key.Enter)
            {
                GetViewport().SetInputAsHandled();
                Keyingisi();
            }

            return;
        }

        // A touch arrives twice, as itself and as the mouse Godot synthesises from it (or the
        // reverse under --sensorli); only the original may step, or one tap skips two videos.
        if (e.Device == InputEvent.DeviceIdEmulation)
        {
            return;
        }

        Vector2? nuqta = e switch
        {
            InputEventMouseButton { Pressed: true } mb => mb.Position,
            InputEventScreenTouch { Pressed: true } st => st.Position,
            _ => null,
        };
        if (nuqta is not Vector2 p)
        {
            return;
        }

        if (_tugma.Visible && _tugma.GetGlobalRect().HasPoint(p))
        {
            return; // the button handles it
        }

        // a press on the video — the fullscreen layer covers everything; the corner window only itself
        if (_joriy.Rejim == "toxtatib" || _burchak.GetGlobalRect().HasPoint(p))
        {
            GetViewport().SetInputAsHandled();
            Keyingisi();
        }
    }

    // ================= layout (styles.css:395-434) =================

    public override void _Process(double delta)
    {
        if (_joriy == null)
        {
            return;
        }

        Vector2 ekran = GetViewport().GetVisibleRect().Size;
        float v = Math.Min(ekran.X, ekran.Y) / 100f;
        bool toliq = _joriy.Rejim == "toxtatib";

        Vector2 ts = _video.GetVideoTexture()?.GetSize() ?? Vector2.Zero;
        if (ts.X <= 0 || ts.Y <= 0)
        {
            ts = new Vector2(16, 9);
        }

        if (toliq)
        {
            // object-fit: contain, centred on black
            float k = Math.Min(ekran.X / ts.X, ekran.Y / ts.Y);
            _video.Size = ts * k;
            _video.Position = (ekran - _video.Size) / 2;
        }
        else
        {
            // right 2.5vmin, top 12vmin, width 32vmin, height by the video's aspect
            var olcham = new Vector2(32 * v, 32 * v * ts.Y / ts.X);
            _burchak.Size = olcham;
            _burchak.Position = new Vector2(ekran.X - (2.5f * v) - olcham.X, 12 * v);
            var ramka = (StyleBoxFlat)_burchak.GetThemeStylebox("panel");
            int b = Math.Max(1, (int)Math.Round(0.22f * v));
            ramka.SetBorderWidthAll(b);
            ramka.SetCornerRadiusAll((int)Math.Round(0.6f * v));
            ramka.ShadowSize = (int)Math.Round(3 * v);
            ramka.ShadowOffset = new Vector2(0, v);
            _video.Size = olcham - (2 * b * Vector2.One);
            _video.Position = _burchak.Position + (b * Vector2.One);
        }

        // .otkaz — right/bottom 3vmin, 2vmin, .12em spacing, padding 1.1 x 2.4vmin (corner: .8 / 1.5 / .6 x 1.2)
        float shriftPx = (toliq ? 2f : 1.5f) * v, chet = (toliq ? 3f : 0.8f) * v;
        float py = (toliq ? 1.1f : 0.6f) * v, px = (toliq ? 2.4f : 1.2f) * v;
        int kalit = (int)Math.Round(shriftPx * 100);
        if (kalit != _tugmaKalit)
        {
            _tugmaKalit = kalit;
            _tugma.AddThemeFontSizeOverride("font_size", Math.Max(6, (int)Math.Round(shriftPx)));
            _tugmaShrift.SpacingGlyph = (int)Math.Round(0.12f * shriftPx);
            foreach ((string holat, bool ustida) in new[] { ("normal", false), ("hover", true), ("pressed", true), ("focus", false) })
            {
                var sb = new StyleBoxFlat
                {
                    BgColor = ustida ? new Color(Krem2, 0.16f) : new Color(16 / 255f, 13 / 255f, 9 / 255f, 0.62f),
                    BorderColor = ustida ? Krem2 : new Color(Krem2, 0.4f),
                    ContentMarginLeft = px, ContentMarginRight = px, ContentMarginTop = py, ContentMarginBottom = py,
                    DrawCenter = holat != "focus",
                };
                sb.SetBorderWidthAll(Math.Max(1, (int)Math.Round(0.2f * v)));
                sb.SetCornerRadiusAll((int)Math.Round(0.4f * v));
                _tugma.AddThemeStyleboxOverride(holat, sb);
            }
        }

        _tugma.Size = _tugma.GetCombinedMinimumSize();
        Rect2 joy = toliq ? new Rect2(Vector2.Zero, ekran) : new Rect2(_burchak.Position, _burchak.Size);
        _tugma.Position = joy.End - new Vector2(chet, chet) - _tugma.Size;

        // .hisob — left 3vmin, bottom 3.4vmin, 1.8vmin, .2em spacing, opacity .45
        _hisob.AddThemeFontSizeOverride("font_size", Math.Max(6, (int)Math.Round(1.8f * v)));
        _hisobShrift.SpacingGlyph = (int)Math.Round(0.2f * 1.8f * v);
        _hisob.Size = _hisob.GetCombinedMinimumSize();
        _hisob.Position = new Vector2(joy.Position.X + (3 * v), joy.End.Y - (3.4f * v) - _hisob.Size.Y);
    }

    private int _tugmaKalit = -1;
}
