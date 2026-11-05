using System;
using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>What the contextual buttons show this frame: game.js <c>O.sensorHolat</c> (game.js:84, 330-333).</summary>
public sealed class SensorHolat
{
    public bool EKorinadi, FKorinadi, Jang;
    public string EYorliq = "E";
    public string? Qurol;
    public int QurolSoni;
}

/// <summary>
/// Touch controls, touch.js: a joystick wherever the left of the screen is touched,
/// drag-to-look on the right, a tap marks a resource (scenario §15.2: marking works
/// very well by tap, so the main verb is a TAP on the resource, not a button), and
/// five buttons that show only when needed (HUD philosophy, §3).
/// </summary>
/// <remarks>
/// It writes the same Oyinchi fields BuxoroInput writes, so no game code knows which
/// device played. Held buttons go through <see cref="BuxoroInput.SensorE"/> and its
/// siblings, because BuxoroInput.Oqi polls those fields anew every frame.
///
/// THE WEB'S EVENT ROUTES, kept: touch.js listens on the canvas, so a press reaches it
/// only if nothing above took it — here _UnhandledInput, after the GUI (buttons, pause
/// card, video, end card). A touch then stays with its first target wherever the finger
/// slides, so moves and releases of the fingers this layer owns are read in _Input,
/// ahead of the GUI: a drag crossing a button must not be swallowed by it, and a button
/// hidden while held (pointer-events: none) must still hear its release, which Godot
/// would not deliver to a control set to MouseFilter.Ignore.
///
/// REFUSED, from the plan: Sensor.tscn with TouchScreenButtons. They read the touch in
/// _input and leave it in the pipeline, so the same finger would also start the
/// joystick; the web's <c>e.stopPropagation()</c> is what a MouseFilter.Stop control does.
///
/// CSS PIXELS. touch.js and styles.css count CSS pixels, which on a phone are not device
/// pixels. <see cref="Px"/> converts: the device scale (devicePixelRatio) over the
/// stretch from window pixels to this project's 1920x1080 canvas.
/// </remarks>
public partial class Touch : CanvasLayer
{
    private const float JoyRadius = 52;          // touch.js:43, CSS px
    private const float ChapUlush = 0.42f;       // touch.js:72 — the joystick's share of the width
    private const double YawKoef = 0.006, PitchKoef = 0.004; // touch.js:93-94, per CSS px
    private const double PitchMin = -0.85, PitchMax = 0.25;
    private const ulong TapVaqtMs = 280;         // touch.js:106
    private const double TapSurish = 14;         // touch.js:106, CSS px
    private const double TapMasofaKv = 900;      // touch.js:142 — 30 m by tap (the keyboard's is 3 m)

    private static readonly Dictionary<string, string> QurolBelgi = new()
    {
        ["kaltak"] = "🏏", ["kamon"] = "🏹", ["qilich"] = "⚔",
    };

    private static readonly string[] QurolTartib = { "kaltak", "kamon", "qilich" };

    private static readonly Color Krem = new("#e8dfc9");
    private static readonly Color Oltin = new("#f2c14e");

    private static bool _tegildi;
    private static bool? _ekran;

    /// <summary>
    /// touch.js:9 <c>('ontouchstart' in window) || navigator.maxTouchPoints > 0</c>. Godot can
    /// tell that up front on phones and tablets but not on a Windows touchscreen, which
    /// announces itself only with its first real touch (<see cref="Tegildi"/>).
    /// </summary>
    public static bool Bormi() => _tegildi || (_ekran ??= DisplayServer.IsTouchscreenAvailable());

    /// <summary>A real finger was seen: from here on this is a touch device. Called by the BuxoroRuntime autoload.</summary>
    public static void Tegildi() => _tegildi = true;

    /// <summary>@media (max-width: 820px), (max-height: 520px) — the window, in CSS pixels.</summary>
    public static bool TorEkran(Node n)
    {
        Vector2I oyna = n.GetTree().Root.Size;
        float dpr = Math.Max(1f, DisplayServer.ScreenGetScale());
        return oyna.X / dpr <= 820 || oyna.Y / dpr <= 520;
    }

    private OyinHolati _o = null!;
    private Oyinchi _p = null!;
    private BuxoroInput _kirish = null!;
    private bool _qurilgan;

    private Control _ildiz = null!;
    private JoyCtrl _joy = null!;
    private Tugma _e = null!, _f = null!, _ur = null!, _bl = null!, _qr = null!;
    private Tugma[] _tugmalar = Array.Empty<Tugma>();
    private float _videoAlfa = 1;

    // touch.js:42 — the joystick's finger; -1 is its null
    private int _joyId = -1;
    private Vector2 _joyBosh;

    // touch.js:69 — the looking finger
    private int _qaraId = -1;
    private Vector2 _qara;
    private double _qaraSurildi;
    private ulong _qaraBosh;

    /// <summary>touch.js ulash() (main.js:72). Stays dormant until this is a touch device.</summary>
    public static Touch Ulash(Node ota, OyinHolati o, BuxoroInput kirish)
    {
        var t = new Touch { Name = "Touch", Layer = 20, ProcessMode = ProcessModeEnum.Always, _o = o, _p = o.Oyinchi, _kirish = kirish };
        ota.AddChild(t);
        return t;
    }

    private float Vmin
    {
        get
        {
            Vector2 s = GetViewport().GetVisibleRect().Size;
            return Math.Min(s.X, s.Y) / 100f;
        }
    }

    /// <summary>One CSS pixel in canvas units.</summary>
    public float Px
    {
        get
        {
            Vector2 ekran = GetViewport().GetVisibleRect().Size;
            Vector2 oyna = GetTree().Root.Size;
            float olchov = Math.Min(oyna.X / ekran.X, oyna.Y / ekran.Y);
            return Math.Max(1f, DisplayServer.ScreenGetScale()) / (olchov > 0 ? olchov : 1f);
        }
    }

    public override void _Ready() => Qur();

    /// <summary>touch.js:13-39 — the overlay; also the moment p.analog starts to exist.</summary>
    private void Qur()
    {
        if (_qurilgan || !Bormi())
        {
            return;
        }

        _qurilgan = true;
        _p.Analog = new Analog();

        _ildiz = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        _ildiz.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_ildiz);

        _joy = new JoyCtrl(this) { MouseFilter = Control.MouseFilterEnum.Ignore, Modulate = new Color(1, 1, 1, 0) };
        _joy.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _ildiz.AddChild(_joy);

        // touch.js:164-179. #tugmalar is flex column-reverse: E at the bottom, then upward.
        _e = new Tugma(this, 3.4f, kichik: false, asosiy: true, () =>
        {
            _kirish.SensorE = true;
            _p.EBosildi = true;
        }, () =>
        {
            _kirish.SensorE = false;
            _p.EUshlandi = 0;
        });
        _f = new Tugma(this, 3.4f, kichik: false, asosiy: false, () => _kirish.SensorF = true, () =>
        {
            _kirish.SensorF = false;
            _p.FUshlandi = 0;
        });
        _ur = new Tugma(this, 5f, kichik: false, asosiy: false, () => _p.Urmoqchi = true, null);
        _bl = new Tugma(this, 3.6f, kichik: true, asosiy: false, () => _kirish.SensorBlok = true, () => _kirish.SensorBlok = false);
        _qr = new Tugma(this, 2.6f, kichik: true, asosiy: false, QurolAlmashtir, null);
        _f.Yozuv.Text = "F";        // #tF::after
        _ur.Yozuv.Text = "⚔";       // #tUr::after
        _bl.Yozuv.Text = "🛡";      // #tBl::after
        _tugmalar = new[] { _e, _f, _ur, _bl, _qr };
        foreach (Tugma t in _tugmalar)
        {
            _ildiz.AddChild(t);
        }

        Joylash();
    }

    /// <summary>touch.js:171-179 — no 1/2/3 on a touch device: the button steps to the next weapon.</summary>
    private void QurolAlmashtir()
    {
        var bor = new List<string>();
        foreach (string q in QurolTartib)
        {
            if (_p.Qurollar[q])
            {
                bor.Add(q);
            }
        }

        if (bor.Count < 2)
        {
            return;
        }

        Player.QurolTanla(_p, bor[(bor.IndexOf(_p.Qurol) + 1) % bor.Count]);
    }

    // ================= per frame =================

    /// <summary>touch.js:188-198, main.js:290 — the buttons show only when needed.</summary>
    public void Yangila(SensorHolat h)
    {
        if (!_qurilgan)
        {
            return;
        }

        _e.Yozuv.Text = string.IsNullOrEmpty(h.EYorliq) ? "E" : h.EYorliq;
        _e.Yashir(!h.EKorinadi);
        _f.Yashir(!h.FKorinadi);
        _ur.Yashir(!h.Jang);
        _bl.Yashir(!h.Jang || h.Qurol == "kamon"); // the bow takes both hands — no shield
        _qr.Yozuv.Text = h.Qurol != null && QurolBelgi.TryGetValue(h.Qurol, out string? belgi) ? belgi : "🏏";
        _qr.Yashir(!(h.QurolSoni > 1));
    }

    public override void _Process(double delta)
    {
        Qur(); // a Windows touchscreen can announce itself at any moment
        if (!_qurilgan)
        {
            return;
        }

        float dt = (float)delta;

        // body.videoOchiq #sensor (styles.css:445): out over .3 s; the transition lives on that rule only, so back at once
        _videoAlfa = Videolar.Ochiqmi() ? Math.Max(0, _videoAlfa - (dt / 0.3f)) : 1;
        _ildiz.Modulate = new Color(1, 1, 1, _videoAlfa);

        // #joy: opacity .8 while on, transition .15 s
        float joyA = _joy.Modulate.A, joyMaqsad = _joyId >= 0 ? 0.8f : 0;
        joyA = joyA < joyMaqsad ? Math.Min(joyMaqsad, joyA + (dt / 0.15f)) : Math.Max(joyMaqsad, joyA - (dt / 0.15f));
        _joy.Modulate = new Color(1, 1, 1, joyA);

        foreach (Tugma t in _tugmalar)
        {
            t.Yangila(dt);
        }

        Joylash();
    }

    /// <summary>#tugmalar: right 3vmin, bottom 4vmin, gap 2vmin; .st 17vmin (max 96px); .kichik 12.5vmin (max 70px).</summary>
    private void Joylash()
    {
        Vector2 ekran = GetViewport().GetVisibleRect().Size;
        float v = Vmin, px = Px;
        float y = ekran.Y - (4 * v);
        foreach (Tugma t in _tugmalar)
        {
            float s = t.Kichik ? Math.Min(12.5f * v, 70 * px) : Math.Min(17 * v, 96 * px);
            y -= s;
            t.Joyla(new Vector2(ekran.X - (3 * v) - s, y), s, v);
            y -= 2 * v;
        }
    }

    // ================= the canvas =================

    /// <summary>touch.js:74-84 — only what no control above took reaches here.</summary>
    public override void _UnhandledInput(InputEvent e)
    {
        if (e is not InputEventScreenTouch { Pressed: true } t)
        {
            return;
        }

        Qur();
        if (!_qurilgan)
        {
            return;
        }

        GetViewport().SetInputAsHandled();
        if (t.Position.X < GetViewport().GetVisibleRect().Size.X * ChapUlush && _joyId < 0)
        {
            JoyBoshla(t);
        }
        else if (_qaraId < 0)
        {
            _qaraId = t.Index;
            _qara = t.Position;
            _qaraSurildi = 0;
            _qaraBosh = Time.GetTicksMsec();
        }

        _p.SpaceBosildi = true; // skips the opening shot
    }

    /// <summary>touch.js:86-113 — the fingers this layer already owns, and every button's release.</summary>
    public override void _Input(InputEvent e)
    {
        if (!_qurilgan)
        {
            return;
        }

        if (e is InputEventScreenDrag d)
        {
            if (d.Index == _joyId)
            {
                JoyHarakat(d.Position);
                GetViewport().SetInputAsHandled();
            }
            else if (d.Index == _qaraId)
            {
                float px = Px;
                double dx = (d.Position.X - _qara.X) / px, dy = (d.Position.Y - _qara.Y) / px;
                _qaraSurildi += Math.Sqrt((dx * dx) + (dy * dy));
                _p.Yonalish -= dx * YawKoef;
                _p.Pitch = Math.Max(PitchMin, Math.Min(PitchMax, _p.Pitch - (dy * PitchKoef)));
                _qara = d.Position;
                GetViewport().SetInputAsHandled();
            }
        }
        else if (e is InputEventScreenTouch { Pressed: false } t)
        {
            if (t.Index == _joyId)
            {
                JoyTugat();
            }
            else if (t.Index == _qaraId)
            {
                // a short touch with little travel = a TAP → mark
                if (Time.GetTicksMsec() - _qaraBosh < TapVaqtMs && _qaraSurildi < TapSurish)
                {
                    TapBelgila(t.Position);
                }

                _qaraId = -1;
            }

            foreach (Tugma b in _tugmalar)
            {
                b.Qoy(t.Index);
            }
        }
    }

    // ================= joystick =================

    private void JoyBoshla(InputEventScreenTouch t)
    {
        _joyId = t.Index;
        _joyBosh = t.Position;
        _joy.Bosh = t.Position;
        _joy.Surish = Vector2.Zero;
        _joy.QueueRedraw();
    }

    private void JoyHarakat(Vector2 joy)
    {
        float px = Px;
        double dx = (joy.X - _joyBosh.X) / px, dy = (joy.Y - _joyBosh.Y) / px;
        double m = Math.Sqrt((dx * dx) + (dy * dy));
        double chek = Math.Min(m, JoyRadius);
        if (m > 0)
        {
            dx = dx / m * chek;
            dy = dy / m * chek;
        }

        _joy.Surish = new Vector2((float)dx * px, (float)dy * px);
        _joy.QueueRedraw();
        Analog a = _p.Analog!;
        a.X = dx / JoyRadius;
        a.Z = -dy / JoyRadius; // down the screen = backward
        a.Kuch = chek / JoyRadius;
    }

    private void JoyTugat()
    {
        _joyId = -1;
        _joy.Surish = Vector2.Zero;
        _joy.QueueRedraw();
        Analog a = _p.Analog!;
        a.X = a.Z = a.Kuch = 0;
    }

    // ================= tap to mark =================

    /// <summary>
    /// touch.js:118-149. Like the web's Raycaster this tests the given objects' triangles
    /// and nothing else — the ground and buildings do not block a tap, and neither does
    /// mobile imprecision: anything hit within 30 m is marked.
    /// </summary>
    private void TapBelgila(Vector2 nuqta)
    {
        Camera3D kamera = _o.Camera;
        Vector3 bosh = kamera.ProjectRayOrigin(nuqta), yon = kamera.ProjectRayNormal(nuqta);

        // an enemy → strike
        foreach (Dushman d in _o.Dushmanlar)
        {
            if (!d.Olgan && NurUrildi(d.Mesh, bosh, yon, out _))
            {
                _p.Urmoqchi = true;
                return;
            }
        }

        // a node → mark it, wherever the player stands within reach
        Tugun? eng = null;
        double engM = double.MaxValue;
        foreach (Tugun t in _o.Dunyo.Tugunlar)
        {
            if (t.Qoldi > 0 && !t.Belgilangan && NurUrildi(t.Mesh, bosh, yon, out double m) && m < engM)
            {
                engM = m;
                eng = t;
            }
        }

        if (eng != null)
        {
            double dx = eng.X - _p.X, dz = eng.Z - _p.Z;
            if ((dx * dx) + (dz * dz) < TapMasofaKv)
            {
                Apprentices.BelgilaTugun(_o.Scene, _o.Shogirdlar, eng);
                return;
            }
        }

        _p.EBosildi = true; // otherwise a plain E
    }

    /// <summary>
    /// <c>Raycaster.intersectObject(ob, true)</c>: the nearest hit on any mesh under
    /// <paramref name="ob"/>. A bounding-box early-out in the mesh's rest pose, then its
    /// triangles — posed, for a skinned mesh, as three.js's SkinnedMesh.boneTransform poses them.
    /// The pose comes from the RenderingServer's copy of the skeleton, which a headless run
    /// does not have; there the rest pose stands in.
    /// </summary>
    private static bool NurUrildi(Node3D ob, Vector3 bosh, Vector3 yon, out double masofa)
    {
        masofa = double.MaxValue;
        List<MeshInstance3D> meshlar = BuxoroModels.Avlodlar<MeshInstance3D>(ob);
        if (ob is MeshInstance3D ildiz)
        {
            meshlar.Add(ildiz);
        }

        foreach (MeshInstance3D mi in meshlar)
        {
            Mesh? mesh = mi.Mesh;
            if (mesh == null)
            {
                continue;
            }

            Transform3D teskari = mi.GlobalTransform.AffineInverse();
            Vector3 lb = teskari * bosh, ly = teskari.Basis * yon; // keeps t in world metres
            if (!QutiNur(mi.GetAabb(), lb, ly))
            {
                continue;
            }

            using SkinReference? skin = mi.GetSkinReference();
            ArrayMesh? pozada = skin != null && RenderingServer.SkeletonGetBoneCount(skin.GetSkeleton()) > 0
                ? mi.BakeMeshFromCurrentSkeletonPose()
                : null;
            Vector3[] yuzlar = (pozada ?? mesh).GetFaces();
            pozada?.Dispose();
            for (int i = 0; i + 2 < yuzlar.Length; i += 3)
            {
                double t = UchburchakNur(lb, ly, yuzlar[i], yuzlar[i + 1], yuzlar[i + 2]);
                if (t >= 0 && t < masofa)
                {
                    masofa = t;
                }
            }
        }

        return masofa < double.MaxValue;
    }

    /// <summary>The slab test: does the ray meet the box?</summary>
    private static bool QutiNur(Aabb q, Vector3 o, Vector3 d)
    {
        float tMin = 0, tMax = float.MaxValue;
        for (int i = 0; i < 3; i++)
        {
            float a = q.Position[i], b = q.End[i];
            if (Math.Abs(d[i]) < 1e-12f)
            {
                if (o[i] < a || o[i] > b)
                {
                    return false;
                }

                continue;
            }

            float t1 = (a - o[i]) / d[i], t2 = (b - o[i]) / d[i];
            tMin = Math.Max(tMin, Math.Min(t1, t2));
            tMax = Math.Min(tMax, Math.Max(t1, t2));
            if (tMin > tMax)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Möller–Trumbore, double-sided: the ray parameter, or -1.</summary>
    private static double UchburchakNur(Vector3 o, Vector3 d, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 e1 = b - a, e2 = c - a, p = d.Cross(e2);
        float det = e1.Dot(p);
        if (Math.Abs(det) < 1e-12f)
        {
            return -1;
        }

        float f = 1f / det;
        Vector3 s = o - a;
        float u = f * s.Dot(p);
        if (u is < 0f or > 1f)
        {
            return -1;
        }

        Vector3 q = s.Cross(e1);
        float v = f * d.Dot(q);
        if (v < 0f || u + v > 1f)
        {
            return -1;
        }

        float t = f * e2.Dot(q);
        return t >= 0f ? t : -1;
    }

    // ================= for --test-sensor =================

    public bool Qurilgan => _qurilgan;

    public int JoyBarmoq => _joyId;

    public int QaraBarmoq => _qaraId;

    /// <summary>A button's centre on the canvas: E, F, Ur, Bl, Qr.</summary>
    public Vector2 TugmaMarkaz(int i) => _tugmalar[i].Position + (_tugmalar[i].Size / 2);

    public string TugmaYozuv(int i) => _tugmalar[i].Yozuv.Text;

    /// <summary>Whether a button takes touches now (.yashirin means pointer-events: none).</summary>
    public bool TugmaFaol(int i) => _tugmalar[i].MouseFilter != Control.MouseFilterEnum.Ignore;

    // ================= drawn widgets =================

    /// <summary>
    /// #joy and its knob, styles.css:193-205. The sizes cap at 150/64 CSS px but the
    /// margins stay vmin, so past the cap the ring sits off the finger — as in the web.
    /// </summary>
    private sealed partial class JoyCtrl : Control
    {
        private readonly Touch _t;

        public JoyCtrl(Touch t)
        {
            _t = t;
        }

        public Vector2 Bosh;
        public Vector2 Surish;

        public override void _Draw()
        {
            float v = _t.Vmin, px = _t.Px;
            float w = Math.Min(26 * v, 150 * px), b = 0.4f * v;
            Vector2 c = Bosh - new Vector2(13 * v, 13 * v) + new Vector2(w / 2, w / 2);
            DrawArc(c, (w / 2) - (b / 2), 0, Mathf.Tau, 64, new Color(Krem, 0.28f), b, true);

            float k = Math.Min(11 * v, 64 * px);
            Vector2 kc = c - new Vector2(5.5f * v, 5.5f * v) + new Vector2(k / 2, k / 2) + Surish;
            DrawCircle(kc, k / 2, new Color(Krem, 0.42f), true, -1, true);
        }
    }

    /// <summary>One .st button, styles.css:211-224. Filter Stop = pointer-events: auto + stopPropagation.</summary>
    private sealed partial class Tugma : Control
    {
        private static readonly Color Fon = new(20 / 255f, 17 / 255f, 12 / 255f, 0.42f);
        private static readonly Color FonBosilgan = new(Krem, 0.30f);

        private readonly Touch _t;
        private readonly Action _bosildi;
        private readonly Action? _qoyildi;
        private readonly bool _asosiy;
        private readonly float _shriftVmin;
        private readonly List<int> _barmoqlar = new();
        private bool _bosilgan;
        private float _bosish;
        private float _alfa = 1;
        private bool _yashirin;
        private float _oxirgiV = -1;

        public Tugma(Touch t, float shriftVmin, bool kichik, bool asosiy, Action bosildi, Action? qoyildi)
        {
            _t = t;
            _shriftVmin = shriftVmin;
            _asosiy = asosiy;
            Kichik = kichik;
            _bosildi = bosildi;
            _qoyildi = qoyildi;
            MouseFilter = MouseFilterEnum.Stop;
            FocusMode = FocusModeEnum.None;
            Yozuv = MenyuShrift.Yozuv("", 10, asosiy ? Oltin : Krem);
            Yozuv.HorizontalAlignment = HorizontalAlignment.Center;
            Yozuv.VerticalAlignment = VerticalAlignment.Center;
            Yozuv.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(Yozuv);
        }

        public bool Kichik { get; }

        public Label Yozuv { get; }

        public void Yashir(bool yashir)
        {
            _yashirin = yashir;
            MouseFilter = yashir ? MouseFilterEnum.Ignore : MouseFilterEnum.Stop;
        }

        public override void _GuiInput(InputEvent e)
        {
            if (e is InputEventScreenTouch { Pressed: true } t)
            {
                _barmoqlar.Add(t.Index);
                _bosilgan = true;
                _bosildi();
                AcceptEvent();
            }
        }

        /// <summary>touchend / touchcancel of a finger that pressed this button.</summary>
        public void Qoy(int barmoq)
        {
            if (_barmoqlar.Remove(barmoq))
            {
                _bosilgan = false;
                _qoyildi?.Invoke();
            }
        }

        /// <summary>transition: opacity .2s, transform .08s, background .08s.</summary>
        public void Yangila(float dt)
        {
            float bMaqsad = _bosilgan ? 1 : 0, aMaqsad = _yashirin ? 0 : 1;
            float b = _bosish < bMaqsad ? Math.Min(bMaqsad, _bosish + (dt / 0.08f)) : Math.Max(bMaqsad, _bosish - (dt / 0.08f));
            if (b != _bosish)
            {
                _bosish = b;
                QueueRedraw();
            }

            _alfa = _alfa < aMaqsad ? Math.Min(aMaqsad, _alfa + (dt / 0.2f)) : Math.Max(aMaqsad, _alfa - (dt / 0.2f));
            Modulate = new Color(1, 1, 1, _alfa);
            Scale = Vector2.One * (1 - (0.07f * _bosish)); // .bosilgan: scale(.93)
        }

        public void Joyla(Vector2 joy, float s, float v)
        {
            Position = joy;
            Size = new Vector2(s, s);
            PivotOffset = Size / 2;
            if (v != _oxirgiV)
            {
                _oxirgiV = v;
                Yozuv.AddThemeFontSizeOverride("font_size", Math.Max(6, (int)Math.Round(_shriftVmin * v)));
                QueueRedraw();
            }
        }

        public override void _Draw()
        {
            float b = 0.4f * _t.Vmin, r = Size.X / 2;
            var c = new Vector2(r, r);
            DrawCircle(c, r, Fon.Lerp(FonBosilgan, _bosish), true, -1, true);
            DrawArc(c, r - (b / 2), 0, Mathf.Tau, 64, _asosiy ? new Color(Oltin, 0.75f) : new Color(Krem, 0.42f), b, true);
        }
    }
}
