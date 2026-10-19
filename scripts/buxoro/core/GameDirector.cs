using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Godot;

namespace ChronoShift;

/// <summary>
/// Root of BuxoroEpisode.tscn. STEP-3 STUB: builds the world and checks it.
/// Step 8 turns this into the ordered tick of game.js:121-181.
/// </summary>
/// <remarks>
/// Command-line hooks (after <c>--</c>):
///   <c>--solver-sweep</c>                 write user://tosiqlar_godot.csv and
///                                         user://solver_godot.csv, then quit
///   <c>--solver-compare &lt;web.csv&gt;</c>  also diff the sweep against the
///                                         browser's tools/parity/solver_web.js
///                                         output; exit code 1 on any point &gt; 1e-4
/// </remarks>
public partial class GameDirector : Node3D
{
    /// <summary>
    /// Cumulative world-stream checkpoints for the 18-GLB set (BUXORO_PORT_PLAN.md
    /// §2.4). They are a property of the ASSET SET: adding a tugun_*.glb or
    /// paxsa_devor.glb legitimately changes them — and re-rolls the field.
    /// </summary>
    private static readonly (string Nom, int Draw)[] KutilganSanoq =
    {
        ("nodes", 1080), ("poplars", 1259), ("houses", 1267), ("gate", 1474), ("edges", 1554),
    };

    /// <summary>binolar.js stream after Chekkalar: 42 PaxsaDevor calls x 12 draws.</summary>
    private const int KutilganBinolarDraw = 504;

    /// <summary>Lattice: 0.5 m over x 0..180, z -50..50, player radius on foot.</summary>
    private const double Qadam = 0.5;

    public Dunyo? Dunyo { get; private set; }

    /// <summary>The game state, game.js's O.</summary>
    public OyinHolati? O { get; private set; }

    private Camera3D _kamera = null!;
    private BuxoroInput? _input;
    private Label? _holat;
    private string _holatBosh = "";
    private bool _kameraQotgan; // --kamera was given: keep that pose (reference renders)

    /// <summary>
    /// main.js tsikl (main.js:267-297): one tick per rendered frame, dt clamped to
    /// 1/20 s. Run per frame rather than at a fixed physics rate so the web's
    /// per-frame idioms behave exactly as they do in the browser on this display.
    /// </summary>
    public override void _Process(double delta)
    {
        if (O == null)
        {
            return;
        }

        double dt = Math.Min(delta, 1.0 / 20);
        if (dt <= 0)
        {
            return;
        }

        Anim.Yangila(dt);
        _input?.Oqi();
        Game.Yangila(O, dt);
        if (O.Tugadi == null && !_kameraQotgan)
        {
            Player.Kamera(O.Oyinchi, _kamera, dt);
        }

        HolatYangila();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        // Temporary until the pause menu (step 16): Escape leaves to the main menu.
        if (e.IsActionPressed(BuxoroActions.Pauza))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GetTree().ChangeSceneToFile("res://scenes/ui/MainMenu.tscn");
        }
    }

    /// <summary>
    /// Step-4 stopwatch (BUXORO_PORT_PLAN.md): x 96 → 156 walking 12.0 s, running
    /// 8.57 s; jump apex 1.079 m, airtime 0.939 s. Simulated at 60 Hz through the
    /// real Player.Yangila, obstacles included.
    /// </summary>
    private bool HarakatTesti()
    {
        Oyinchi p = O!.Oyinchi;
        const double dt = 1.0 / 60;

        double Yugur(bool yugur)
        {
            p.X = 96;
            p.Z = 6;
            p.Yonalish = -Math.PI / 2; // camera behind, looking east
            p.Tugmalar = new Tugmalar { Oldinga = true, Yugur = yugur };
            double t = 0;
            while (p.X < 156 && t < 60)
            {
                Player.Yangila(p, dt, Dunyo);
                t += dt;
            }

            return t;
        }

        double yur = Yugur(false), yug = Yugur(true);

        p.Tugmalar = new Tugmalar { Space = true };
        double cho = 0, havo = 0;
        Player.Yangila(p, dt, Dunyo);
        p.Tugmalar = new Tugmalar();
        havo += dt;
        while (p.Sakramoqda && havo < 5)
        {
            cho = Math.Max(cho, p.Balandlik);
            Player.Yangila(p, dt, Dunyo);
            havo += dt;
        }

        // The plan's 1.079 m / 0.939 s are the analytic v²/2g and 2v/g. player.js
        // integrates per frame (vy -= g·dt; h += vy·dt), so the web build itself
        // peaks at 1.041 m and lands after 0.933 s at 60 fps — those are the targets.
        bool ok = Math.Abs(yur - 12.0) < 0.05 && Math.Abs(yug - 8.571) < 0.05 &&
                  Math.Abs(cho - 1.041) < 0.01 && Math.Abs(havo - 0.933) < 0.02;
        GD.Print(string.Format(CultureInfo.InvariantCulture,
            "[buxoro] harakat: yurish {0:F3} s (12.0), yugurish {1:F3} s (8.571), sakrash cho'qqisi {2:F3} m (1.041 @60Hz), havoda {3:F3} s (0.933 @60Hz) — {4}",
            yur, yug, cho, havo, ok ? "PASS" : "FAIL"));
        return ok;
    }

    /// <summary>DEBUG readout for the step-4 stopwatch checks; replaced by BuxoroHud.</summary>
    private void HolatYangila()
    {
        if (_holat == null || O == null)
        {
            return;
        }

        Oyinchi p = O.Oyinchi;
        _holat.Text = _holatBosh +
            string.Format(CultureInfo.InvariantCulture, "\nx {0:F2}  z {1:F2}  sakrash {2:F3} m  vaqt {3:F1}", p.X, p.Z, p.Balandlik, O.Vaqt);
    }
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
        DevorBelgilari(dunyo3d);

        // ---- main.js boshla() / oyinniBoshla() ----
        _kamera = GetNode<Camera3D>("Kamera");
        BuxoroAudio.Boshla(this);
        BuxoroAudio.Shamol(0.22, 2.5);
        Anim.Tozala();
        O = Game.Yarat(dunyo3d, _kamera, Dunyo);
        Player.OkklyuziyaTayyorla(Dunyo.Teraklar);
        _input = new BuxoroInput { Name = "BuxoroInput", P = O.Oyinchi };
        AddChild(_input);
        BuxoroAudio.Musiqa(true);
        Player.KameraniJoylashtir(O.Oyinchi, _kamera);
        KameraArg();
        Input.MouseMode = Input.MouseModeEnum.Captured;

        GD.Print($"[buxoro] world built — {Dunyo.Tugunlar.Count} nodes, {Dunyo.Teraklar.Count} poplars, " +
                 $"{Dunyo.Tosiqlar.Count} obstacles, draws {world.Lcg.DrawCount}. {(ok ? "PASS" : "FAIL")}");
        HolatYozuvi(world, ok);

        string[] args = OS.GetCmdlineUserArgs();
        if (Array.IndexOf(args, "--test-harakat") >= 0)
        {
            GetTree().Quit(HarakatTesti() ? 0 : 1);
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
        }
    }

    /// <summary>
    /// DEBUG: <c>-- --kamera px,py,pz,tx,ty,tz</c> poses the camera, so --write-movie can
    /// render reference views headlessly-by-script (e.g. the web game's same view).
    /// </summary>
    private void KameraArg()
    {
        string[] args = OS.GetCmdlineUserArgs();
        int i = Array.IndexOf(args, "--kamera");
        if (i < 0 || i + 1 >= args.Length)
        {
            return;
        }

        string[] p = args[i + 1].Split(',');
        if (p.Length != 6)
        {
            GD.PushError("[buxoro] --kamera expects px,py,pz,tx,ty,tz");
            return;
        }

        float F(int k) => float.Parse(p[k], CultureInfo.InvariantCulture);
        GetNodeOrNull<Camera3D>("Kamera")?.LookAtFromPosition(new Vector3(F(0), F(1), F(2)), new Vector3(F(3), F(4), F(5)), Vector3.Up);
        _kameraQotgan = true;
    }

    /// <summary>
    /// DEBUG ONLY: the build status and fly-camera keys on screen, so a tester running
    /// the exported .exe sees the result without a console. Replaced by BuxoroHud (step 11).
    /// </summary>
    private void HolatYozuvi(World world, bool ok)
    {
        var layer = new CanvasLayer { Name = "HolatYozuvi" };
        var label = new Label { Position = new Vector2(24, 20) };
        label.AddThemeColorOverride("font_color", ok ? new Color(1, 1, 1) : new Color(1, 0.35f, 0.3f));
        label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0));
        label.AddThemeConstantOverride("outline_size", 6);
        _holatBosh = "1-BO'LIM — BUXORO 1238 · port, 4-qadam (Shomurod)\n" +
                     $"tugunlar {Dunyo!.Tugunlar.Count} · teraklar {Dunyo.Teraklar.Count} · to'siqlar {Dunyo.Tosiqlar.Count} · " +
                     $"draw {world.Lcg.DrawCount} — {(ok ? "PASS" : "FAIL")}\n" +
                     "WASD — yurish · Shift — yugurish · Space — sakrash · sichqoncha — qarash · Esc — menyu";
        label.Text = _holatBosh;
        _holat = label;
        layer.AddChild(label);
        AddChild(layer);
    }

    private bool SanoqTekshir(World world)
    {
        bool ok = true;
        var sb = new StringBuilder("[buxoro] draw checkpoints:");
        for (int i = 0; i < KutilganSanoq.Length; i++)
        {
            (string nom, int draw) = Dunyo!.DrawSanoq[i];
            bool mos = nom == KutilganSanoq[i].Nom && draw == KutilganSanoq[i].Draw;
            ok &= mos;
            sb.Append($" {nom} {draw}{(mos ? "" : $" (EXPECTED {KutilganSanoq[i].Draw})")}");
        }

        GD.Print(sb.ToString());
        if (!ok)
        {
            GD.PushError("[buxoro] world stream diverged from the web build — every node position after the first " +
                         "mismatch is wrong. Did the GLB set in assets/models/buxoro change?");
        }

        if (world.BinolarLcg.DrawCount != KutilganBinolarDraw)
        {
            GD.PushError($"[buxoro] binolar stream at {world.BinolarLcg.DrawCount}, expected {KutilganBinolarDraw}.");
            ok = false;
        }

        if (Dunyo!.Tugunlar.Count != 66)
        {
            GD.PushError($"[buxoro] {Dunyo.Tugunlar.Count} resource nodes, expected 66.");
            ok = false;
        }

        return ok;
    }

    /// <summary>
    /// DEBUG ONLY: thin posts at the six wall points so step 3 can be checked by eye.
    /// The real markers and walls arrive with GameDirector/CombatSystem (steps 8-9).
    /// </summary>
    private void DevorBelgilari(Node3D ota)
    {
        var m = new StandardMaterial3D { AlbedoColor = new Color(0.9f, 0.5f, 0.1f), ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded };
        foreach (DevorNuqta n in Dunyo!.DevorNuqtalari)
        {
            var post = Actors.Quti(0.3f, 3f, 0.3f, m, (float)n.X, (float)World.YerBalandligi(n.X, n.Z) + 1.5f, (float)n.Z);
            post.Name = "DevorNuqtaDebug";
            ota.AddChild(post);
        }
    }

    /// <summary>Runs ToqnashuvHal over the whole field. Mirrors tools/parity/solver_web.js line for line.</summary>
    private static List<string> SolverSweep(Dunyo d)
    {
        CultureInfo inv = CultureInfo.InvariantCulture;
        double r = BuxoroRuntime.B.ToqnashuvRadiusPiyoda;

        var tos = new StringBuilder("i,shakl,x,z,r,r1,r2,w,d,burilish\n");
        for (int i = 0; i < d.Tosiqlar.Count; i++)
        {
            Tosiq t = d.Tosiqlar[i];
            tos.Append(string.Format(inv, "{0},{1},{2:R},{3:R},{4:R},{5:R},{6:R},{7:R},{8:R},{9:R}\n",
                i, t.Shakl, t.X, t.Z, t.R, t.R1, t.R2, t.W, t.D, t.Burilish));
        }

        Yoz("user://tosiqlar_godot.csv", tos.ToString());

        var rows = new List<string>();
        var sb = new StringBuilder("x,z,ox,oz\n");
        for (int ix = 0; ix <= 360; ix++)
        {
            for (int iz = 0; iz <= 200; iz++)
            {
                double x = ix * Qadam, z = -50 + (iz * Qadam);
                double ox = x, oz = z;
                BuxoroSolver.ToqnashuvHal(d.Tosiqlar, ref ox, ref oz, r);
                string row = string.Format(inv, "{0},{1},{2:F9},{3:F9}", x, z, ox, oz);
                rows.Add(row);
                sb.Append(row).Append('\n');
            }
        }

        Yoz("user://solver_godot.csv", sb.ToString());
        GD.Print($"[buxoro] solver sweep: {rows.Count} points, {d.Tosiqlar.Count} obstacles → " +
                 ProjectSettings.GlobalizePath("user://solver_godot.csv"));
        return rows;
    }

    private static bool SolverCompare(List<string> godot, string webPath)
    {
        if (!System.IO.File.Exists(webPath))
        {
            GD.PushError($"[buxoro] solver compare: {webPath} not found");
            return false;
        }

        string[] web = System.IO.File.ReadAllLines(webPath);
        int n = Math.Min(godot.Count, web.Length - 1), yomon = 0;
        double eng = 0;
        for (int i = 0; i < n; i++)
        {
            double[] a = Sonlar(godot[i]), b = Sonlar(web[i + 1]);
            double farq = Math.Max(Math.Abs(a[2] - b[2]), Math.Abs(a[3] - b[3]));
            eng = Math.Max(eng, farq);
            if (farq > 1e-4 || a[0] != b[0] || a[1] != b[1])
            {
                if (yomon++ < 10)
                {
                    GD.Print($"[buxoro]   diff at {godot[i]}  web {web[i + 1]}");
                }
            }
        }

        bool ok = yomon == 0 && godot.Count == web.Length - 1;
        GD.Print($"[buxoro] solver compare: {n} points, {yomon} over 1e-4, max {eng:E2} — {(ok ? "PASS" : "FAIL")}");
        return ok;
    }

    private static double[] Sonlar(string row)
    {
        string[] p = row.Split(',');
        var v = new double[p.Length];
        for (int i = 0; i < p.Length; i++)
        {
            v[i] = double.Parse(p[i], CultureInfo.InvariantCulture);
        }

        return v;
    }

    private static void Yoz(string path, string text)
    {
        using var f = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
        f.StoreString(text);
    }
}
