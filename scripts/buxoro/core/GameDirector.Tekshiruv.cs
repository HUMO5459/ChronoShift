using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Godot;

namespace ChronoShift;

/// <summary>GameDirector's verification hooks: draw checkpoints, the solver sweep, the stopwatch, --kamera.</summary>
public partial class GameDirector
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
        if (BuxoroModels.GlbsizMi)
        {
            GD.Print($"[buxoro] --glbsiz: another asset set, its own checkpoints (binolar stream {world.BinolarLcg.DrawCount}).");
            return true;
        }

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
    /// <c>-- --test-oyin</c>: a whole run, simulated at 30 Hz through Game.Yangila. The
    /// player is made invulnerable and marks every node within reach of the spawn so
    /// the economy runs; the test logs the beats (waves, mounting, acts, ombor) and
    /// fails on any exception or a missing beat.
    /// </summary>
    private bool OyinTesti()
    {
        OyinHolati o = O!;
        Oyinchi p = o.Oyinchi;
        const double dt = 1.0 / 30;
        _ochilish = null;
        p.Hp = 1e9;

        // mark the nearest stone nodes, the way a player would in Act I
        foreach (Tugun t in o.Dunyo.Tugunlar.FindAll(t => t.Tur == "tosh").GetRange(0, 4))
        {
            Apprentices.BelgilaTugun(o.Scene, o.Shogirdlar, t);
        }

        int tolqin = -1, parda = 1;
        bool otda = false, ok = true;
        double t0 = o.Vaqt;
        var izlar = new List<string>();
        try
        {
            for (int k = 0; k < 30 * 900 && o.Tugadi == null; k++)
            {
                Anim.Yangila(dt);
                Game.Yangila(o, dt);
                p.Hp = 1e9;
                if (o.TolqinIndeks != tolqin)
                {
                    tolqin = o.TolqinIndeks;
                    izlar.Add($"{o.Vaqt:F0}s tolqin {tolqin} dushman {o.Dushmanlar.Count}");
                }

                if (p.Otda && !otda)
                {
                    otda = true;
                    izlar.Add($"{o.Vaqt:F0}s otda (chavandozlar: {o.Dushmanlar.FindAll(d => d.Chavandoz).Count})");
                }

                if (o.Parda != parda)
                {
                    parda = o.Parda;
                    izlar.Add($"{o.Vaqt:F0}s parda {parda}");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PushError("[buxoro] oyin testi: " + ex);
            ok = false;
        }

        izlar.Add($"yakun: {o.Tugadi?.Tur ?? "yo'q"} — ombor tosh {o.Shogirdlar.Ombor["tosh"]}, jami yig'ilgan {o.Shogirdlar.JamiYigilgan}, olgan dushman {o.OlganDushman}/{BuxoroRuntime.B.JamiDushman}");
        foreach (string iz in izlar)
        {
            GD.Print("[buxoro]   " + iz);
        }

        ok &= otda && parda == 3 && tolqin == 3 && o.Shogirdlar.JamiYigilgan > 0 && o.Tugadi != null;
        GD.Print($"[buxoro] oyin testi ({t0:F0}s dan): {(ok ? "PASS" : "FAIL")}");
        return ok;
    }

    /// <summary>
    /// <c>-- --test-video</c> (real time: Theora paces itself by the audio clock, so
    /// --fixed-fps would starve it): the cutscenes through the
    /// real Videolar layer, frame loop and input pipeline — the opening sequence
    /// (Space → next, a video's natural end → next, the game frozen meanwhile and
    /// running after), then a defeat (its video first, Esc skips all, the stats
    /// card after).
    /// </summary>
    private async void VideoTesti()
    {
        bool ok = true;
        OyinHolati o = O!;
        SceneTree tree = GetTree();

        void Tekshir(bool shart, string nima)
        {
            GD.Print($"[buxoro]   {(shart ? "ok  " : "XATO")} {nima}");
            ok &= shart;
        }

        async System.Threading.Tasks.Task<bool> Kut(Func<bool> shart, double soniya)
        {
            for (double t = 0; t < soniya; t += GetProcessDeltaTime())
            {
                if (shart())
                {
                    return true;
                }

                await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            }

            return shart();
        }

        void Bos(Key k)
        {
            Input.ParseInputEvent(new InputEventKey { Keycode = k, PhysicalKeycode = k, Pressed = true });
            Input.ParseInputEvent(new InputEventKey { Keycode = k, PhysicalKeycode = k, Pressed = false });
        }

        await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        double vaqt0 = o.Vaqt;
        Tekshir(Videolar.Holat() == "boshlanish 1/3" && Videolar.Toxtatadimi(), $"ochilishda video: {Videolar.Holat()}");

        Bos(Key.Space);
        await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        Tekshir(Videolar.Holat() == "boshlanish 2/3", $"Space → keyingi video: {Videolar.Holat()}");
        Tekshir(!o.Oyinchi.SpaceBosildi && _ochilish != null, "Space ochilish parvoziga yetmadi (video yutdi)");

        Tekshir(await Kut(() => Videolar.Holat() == "boshlanish 3/3", 12), $"2-video o'zi tugadi → 3/3 ({Videolar.Holat()})");
        Tekshir(await Kut(() => !Videolar.Ochiqmi(), 12), "3-video tugadi → yopildi");
        Tekshir(Math.Abs(o.Vaqt - vaqt0) < 1e-9, $"video paytida soat to'xtagan ({o.Vaqt:F2})");
        await Kut(() => false, 1);
        Tekshir(o.Vaqt < vaqt0 - 0.5, $"videodan keyin o'yin yuradi ({o.Vaqt:F2})");

        // defeat: the clock runs out → 3.5 s of the ending camera → its video. The jump
        // crosses the act boundaries too, so their cues (toshTugadi, …) come first: Esc them.
        o.Vaqt = 0.2;
        var oraliq = new List<string>();
        while (await Kut(() => Videolar.Ochiqmi(), 6) && !Videolar.Holat()!.StartsWith("maglubiyat") && oraliq.Count < 5)
        {
            oraliq.Add(Videolar.Holat()!);
            Bos(Key.Escape);
            await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        }

        Tekshir(Videolar.Holat() == "maglubiyat 1/2", $"mag'lubiyat videosi (oldin: {string.Join(", ", oraliq)})");
        Tekshir(!BuxoroHud.KadrOchiq, "kadr video tugaguncha kutadi");
        Bos(Key.Escape);
        await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        Tekshir(!Videolar.Ochiqmi() && BuxoroHud.KadrOchiq && !_pauza, "Esc hammasini o'tkazdi → yakun kadri, pauza ochilmadi");

        GD.Print($"[buxoro] video testi: {(ok ? "PASS" : "FAIL")}");
        tree.Quit(ok ? 0 : 1);
    }

    private static int _qaytaSanoq;
    private static bool _qaytaOk = true;

    /// <summary>
    /// <c>-- --test-qayta</c> (real time): QAYTA BOSHLASH five times, then the main menu,
    /// then out. Every new episode must get sound through the one shared generator
    /// (BuxoroAudio._generator): more samples written than its ring buffer holds (8191)
    /// holds means the mixer really consumed them.
    /// </summary>
    private async void QaytaTesti()
    {
        SceneTree tree = GetTree();
        for (double t = 0; BuxoroAudio.Yozilgan <= 12000 && t < 5; t += GetProcessDeltaTime())
        {
            await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        }

        bool ovoz = BuxoroAudio.Yozilgan > 12000;
        _qaytaOk &= ovoz;
        _qaytaSanoq++;
        GD.Print($"[buxoro]   {(ovoz ? "ok  " : "XATO")} {_qaytaSanoq}-yuklash: ovoz {BuxoroAudio.Yozilgan} namuna");
        if (_qaytaSanoq < 5)
        {
            tree.ReloadCurrentScene();
            return;
        }

        bool ok = _qaytaOk;
        tree.ChangeSceneToFile("res://scenes/ui/MainMenu.tscn");
        tree.CreateTimer(2).Timeout += () =>
        {
            GD.Print($"[buxoro] qayta testi: {(ok ? "PASS" : "FAIL")}");
            tree.Quit(ok ? 0 : 1);
        };
    }

    /// <summary>
    /// <c>-- --test-sensor</c> (real time): the touch layer through the real pipeline. Every
    /// finger is an event given to Input, so it goes _Input → GUI → _UnhandledInput and brings
    /// Godot's synthesised mouse along, exactly as a phone's does. The checks read the fields
    /// right after the event, before a frame can consume them.
    /// </summary>
    private async void SensorTesti()
    {
        bool ok = true;
        OyinHolati o = O!;
        Oyinchi p = o.Oyinchi;
        Touch s = _sensor!;
        BuxoroInput kirish = _input!;
        SceneTree tree = GetTree();

        void Tekshir(bool shart, string nima)
        {
            GD.Print($"[buxoro]   {(shart ? "ok  " : "XATO")} {nima}");
            ok &= shart;
        }

        Vector2 Oyna(Vector2 k) => GetViewport().GetFinalTransform() * k;

        void Berib(InputEvent e)
        {
            Input.ParseInputEvent(e);
            Input.FlushBufferedEvents();
        }

        void Teg(int b, Vector2 k) => Berib(new InputEventScreenTouch { Index = b, Position = Oyna(k), Pressed = true });
        void Qoy(int b, Vector2 k) => Berib(new InputEventScreenTouch { Index = b, Position = Oyna(k), Pressed = false });
        void Sur(int b, Vector2 dan, Vector2 ga) => Berib(new InputEventScreenDrag { Index = b, Position = Oyna(ga), Relative = Oyna(ga) - Oyna(dan) });

        async System.Threading.Tasks.Task Kadrlar(int n)
        {
            for (int i = 0; i < n; i++)
            {
                await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            }
        }

        static bool Yaqin(double a, double b, double tol = 1e-4) => Math.Abs(a - b) < tol;

        await Kadrlar(1);
        Vector2 ekran = GetViewport().GetVisibleRect().Size;
        GD.Print($"[buxoro] sensor testi: kanvas {ekran}, oyna {tree.Root.Size}, qurilgan {s.Qurilgan}");

        // ---- a fullscreen video takes the tap, and one tap is one step ----
        Vector2 orta = ekran / 2;
        Teg(0, orta);
        Qoy(0, orta);
        Tekshir(Videolar.Holat() == "boshlanish 2/3", $"videoda bir tegish = bir qadam: {Videolar.Holat()}");
        Tekshir(!p.SpaceBosildi && !p.Urmoqchi && s.JoyBarmoq < 0, "video tegishni yutdi: o'yinga yetmadi");
        Berib(new InputEventKey { Keycode = Key.Escape, PhysicalKeycode = Key.Escape, Pressed = true });
        Berib(new InputEventKey { Keycode = Key.Escape, PhysicalKeycode = Key.Escape, Pressed = false });
        Tekshir(!Videolar.Ochiqmi() && _ochilish != null, "video yopildi, ochilish parvozi davom etadi");

        // ---- the opening: #kadr covers the canvas; once it fades, a tap skips ----
        // Its 2 s timer also counts the scene's load time: through the menu (main.js has no
        // such step) the first frame's delta can use it all up, so show it again if so.
        if (!BuxoroHud.KadrOchiq)
        {
            GD.Print("[buxoro]   kadr taymeri yuklanish vaqtida tugagan: qayta ko'rsatildi");
            BuxoroHud.Kadr(null, Game.G("ochilish"));
            await Kadrlar(1);
        }

        Teg(0, orta);
        Qoy(0, orta);
        Tekshir(BuxoroHud.KadrOchiq && !p.SpaceBosildi, "kadr ochiq: tegish o'tmaydi");
        BuxoroHud.KadrYashir();
        await Kadrlar(1); // the first real touch above built the overlay in _Process, or this one does
        Tekshir(s.Qurilgan && p.Analog != null, "birinchi tegishdan keyin sensor qatlami qurildi");
        Teg(0, orta);
        Tekshir(p.SpaceBosildi, "kadr so'nganda tegish parvozni o'tkazadi");
        Qoy(0, orta);
        await Kadrlar(2);
        Tekshir(_ochilish == null, "parvoz o'tkazildi");
        Tekshir(!p.Urmoqchi && Input.MouseMode != Input.MouseModeEnum.Captured, "sintetik sichqoncha urmadi, kursorni ushlamadi");

        float px = s.Px;
        var sol = new Vector2(ekran.X * 0.2f, ekran.Y * 0.6f);
        var ong = new Vector2(ekran.X * 0.7f, ekran.Y * 0.4f);

        // ---- joystick ----
        p.X = 40;
        p.Z = 0;
        p.Yonalish = -Math.PI / 2;
        Teg(0, sol);
        Tekshir(s.JoyBarmoq == 0 && p.Analog!.Kuch == 0, "chapda tegish = joystik, siljishsiz kuch 0");
        Sur(0, sol, sol + new Vector2(0, 26 * px));
        Tekshir(Yaqin(p.Analog!.Z, -0.5) && Yaqin(p.Analog.X, 0) && Yaqin(p.Analog.Kuch, 0.5), $"26 px pastga: z {p.Analog.Z:F3}, kuch {p.Analog.Kuch:F3}");
        Sur(0, sol, sol + new Vector2(300 * px, 0));
        Tekshir(Yaqin(p.Analog.X, 1) && Yaqin(p.Analog.Kuch, 1), $"radiusdan tashqari: x {p.Analog.X:F3}, kuch {p.Analog.Kuch:F3}");

        async System.Threading.Tasks.Task<double> Tezlik()
        {
            double x0 = p.X, z0 = p.Z, t0 = o.Otgan;
            await Kadrlar(12);
            return Math.Sqrt(((p.X - x0) * (p.X - x0)) + ((p.Z - z0) * (p.Z - z0))) / Math.Max(1e-6, o.Otgan - t0);
        }

        Sur(0, sol, sol + new Vector2(0, -100 * px));
        double tez = await Tezlik();
        Tekshir(Math.Abs(tez - 7) < 0.5, $"to'liq bosilgan = yugurish: {tez:F2} m/s (7.0)");
        p.X = 40;
        p.Z = 0;
        Sur(0, sol, sol + new Vector2(0, -26 * px));
        tez = await Tezlik();
        Tekshir(Math.Abs(tez - 2.5) < 0.3, $"yarim bosilgan: {tez:F2} m/s (5.0 x 0.5)");
        Qoy(0, sol);
        Tekshir(s.JoyBarmoq < 0 && p.Analog.Kuch == 0 && p.Analog.X == 0 && p.Analog.Z == 0, "qo'yib yuborilganda hammasi nol");

        // ---- looking ----
        double yaw0 = p.Yonalish;
        p.Pitch = -0.38;
        Teg(1, ong);
        Tekshir(s.QaraBarmoq == 1 && s.JoyBarmoq < 0, "o'ngda tegish = qarash");
        Sur(1, ong, ong + new Vector2(100 * px, 20 * px));
        Tekshir(Yaqin(p.Yonalish, yaw0 - 0.6, 1e-3) && Yaqin(p.Pitch, -0.46, 1e-3), $"surish: yonalish {p.Yonalish - yaw0:F3} (-0.6), pitch {p.Pitch:F3} (-0.46)");
        Sur(1, ong + new Vector2(100 * px, 20 * px), ong + new Vector2(100 * px, 2000 * px));
        Tekshir(Yaqin(p.Pitch, -0.85), $"pitch chegarada: {p.Pitch:F3}");
        Qoy(1, ong);
        Tekshir(s.QaraBarmoq < 0 && !p.EBosildi, "ko'p surilgan: tap emas");

        Teg(1, ong);
        await ToSignal(tree.CreateTimer(0.32), SceneTreeTimer.SignalName.Timeout);
        Qoy(1, ong);
        Tekshir(!p.EBosildi, "uzoq bosilgan (280 ms dan ko'p): tap emas");

        // ---- two fingers, and a second one on the left while the joystick is busy ----
        Teg(0, sol);
        Teg(1, ong);
        yaw0 = p.Yonalish;
        Sur(1, ong, ong + new Vector2(50 * px, 0));
        Sur(0, sol, sol + new Vector2(52 * px, 0));
        Tekshir(Yaqin(p.Yonalish, yaw0 - 0.3, 1e-3) && Yaqin(p.Analog.X, 1), "ikki barmoq bir vaqtda");
        Qoy(1, ong);
        Teg(2, sol + new Vector2(30, 30));
        Tekshir(s.JoyBarmoq == 0 && s.QaraBarmoq == 2, "joystik band: chapdagi ikkinchi barmoq qaraydi");
        Qoy(2, sol + new Vector2(30, 30));
        p.EBosildi = false;
        Qoy(0, sol);

        // ---- tap to mark ----
        void Oldida(double x, double z, double masofa)
        {
            p.X = x + (masofa * Math.Sin(p.Yonalish));
            p.Z = z + (masofa * Math.Cos(p.Yonalish));
            p.Y = 0;
            Player.KameraniJoylashtir(p, _kamera);
        }

        Vector2 Ekranda(Node3D ob)
        {
            var mi = BuxoroModels.Avlodlar<MeshInstance3D>(ob)[0];
            return _kamera.UnprojectPosition(mi.GlobalTransform * mi.GetAabb().GetCenter());
        }

        p.Yonalish = -Math.PI / 2;
        var yaqinda = new Vector2(ekran.X * 0.6f, ekran.Y * 0.2f);
        Oldida(70, 0, 6);
        Teg(0, yaqinda);
        Qoy(0, yaqinda);
        Tekshir(p.EBosildi && !p.Urmoqchi, "bo'sh joyga tap = oddiy E");
        p.EBosildi = false;

        Tugun t25 = o.Dunyo.Tugunlar[0], t35 = o.Dunyo.Tugunlar[1];
        int belgilar = o.Shogirdlar.Belgilar.Count;
        Oldida(t25.X, t25.Z, 25);
        Vector2 n25 = Ekranda(t25.Mesh);
        Teg(1, n25);
        Qoy(1, n25);
        Tekshir(t25.Belgilangan && o.Shogirdlar.Belgilar.Count == belgilar + 1 && !p.EBosildi, $"25 m dagi tugunga tap belgilaydi ({n25})");
        Teg(1, n25);
        Qoy(1, n25);
        Tekshir(p.EBosildi && o.Shogirdlar.Belgilar.Count == belgilar + 1, "belgilangan tugun yana belgilanmaydi: oddiy E");
        p.EBosildi = false;

        Oldida(t35.X, t35.Z, 35);
        Vector2 n35 = Ekranda(t35.Mesh);
        Teg(1, n35);
        Qoy(1, n35);
        Tekshir(!t35.Belgilangan && p.EBosildi, $"35 m: belgilanmaydi, oddiy E ({n35})");
        p.EBosildi = false;

        // ---- tap an enemy: skinned, so the triangles are the posed ones ----
        p.X = 40;
        p.Z = 0;
        Player.KameraniJoylashtir(p, _kamera);
        Dushman d = Combat.DushmanYarat(o.Scene, "piyoda", 0);
        o.Dushmanlar.Add(d);
        d.X = 50;
        d.Z = 0;
        d.Mesh.Position = new Vector3(50, (float)World.YerBalandligi(50, 0), 0);
        MeshInstance3D? tana = null;
        foreach (MeshInstance3D m in BuxoroModels.Avlodlar<MeshInstance3D>(d.Mesh))
        {
            if (tana == null || m.GetAabb().Volume > tana.GetAabb().Volume)
            {
                tana = m;
            }
        }

        Vector2 dn = _kamera.UnprojectPosition(tana!.GlobalTransform * tana.GetAabb().GetCenter());
        Teg(1, dn);
        Qoy(1, dn);
        Tekshir(p.Urmoqchi && !p.EBosildi, $"dushmanga tap = urish (skin {tana.Skin != null}, {dn})");
        p.Urmoqchi = false;
        Vector2 yonida = dn + new Vector2(ekran.X * 0.1f, 0);
        Teg(1, yonida);
        Qoy(1, yonida);
        Tekshir(!p.Urmoqchi && p.EBosildi, "dushman yonidagi bo'sh joy: urish yo'q, oddiy E");
        p.EBosildi = false;
        p.Urmoqchi = false;
        await Kadrlar(2);
        Tekshir(o.SensorHolat.Jang, "dushman 14 m ichida: jang tugmalari");
        Combat.ZararBer(d, d.Hp + 1, _ => { });
        await Kadrlar(1);

        // ---- the conditions the buttons come from, in the running game ----
        DevorNuqta nuqta = o.Dunyo.DevorNuqtalari[0];
        p.X = nuqta.X + 2;
        p.Z = nuqta.Z;
        o.Shogirdlar.Ombor["tosh"] = 0;
        await Kadrlar(2);
        Tekshir(!o.SensorHolat.FKorinadi, "devor nuqtasida tosh yo'q: F chiqmaydi");
        o.Shogirdlar.Ombor["tosh"] = BuxoroRuntime.B.Recipe("devor")!.Tosh;
        await Kadrlar(2);
        Tekshir(o.SensorHolat.FKorinadi && s.TugmaFaol(1), "tosh yetarli: F chiqadi");
        o.Shogirdlar.Ombor["tosh"] = 0;
        Tugun yaqinTugun = o.Dunyo.Tugunlar.Find(t => !t.Belgilangan && t.Qoldi > 0)!;
        p.X = yaqinTugun.X + 1;
        p.Z = yaqinTugun.Z;
        await Kadrlar(2);
        Tekshir(o.SensorHolat.EKorinadi && o.SensorHolat.EYorliq == "◈" && s.TugmaYozuv(0) == "◈", "tugun 3 m ichida: E tugmasi ◈");
        p.EBosildi = false;

        // ---- the buttons ----
        var hammasi = new SensorHolat { EKorinadi = true, FKorinadi = true, Jang = true, EYorliq = "✋", Qurol = "kaltak", QurolSoni = 2 };
        s.Yangila(hammasi);
        Tekshir(s.TugmaYozuv(0) == "✋" && s.TugmaYozuv(4) == "🏏" && s.TugmaFaol(0) && s.TugmaFaol(3), "yorliqlar va ko'rinish holatdan");
        Vector2 mE = s.TugmaMarkaz(0), mF = s.TugmaMarkaz(1), mUr = s.TugmaMarkaz(2), mBl = s.TugmaMarkaz(3), mQr = s.TugmaMarkaz(4);
        GD.Print($"[buxoro]   tugmalar: E {mE} F {mF} Ur {mUr} Bl {mBl} Qr {mQr}");

        p.EUshlandi = 0;
        Teg(0, mE);
        Tekshir(p.EBosildi && kirish.SensorE && s.QaraBarmoq < 0 && s.JoyBarmoq < 0, "E: bosildi, qatlamga o'tmadi");
        await Kadrlar(4);
        Tekshir(p.Tugmalar.E && p.EUshlandi > 0, $"E ushlab turilibdi: {p.EUshlandi:F3} s");
        s.Yangila(new SensorHolat());
        Qoy(0, mE);
        Tekshir(!kirish.SensorE && p.EUshlandi == 0, "E: yashirilgan bo'lsa ham qo'yish eshitiladi, taymer nol");
        kirish.Oqi();
        Tekshir(!p.Tugmalar.E, "E: poll ham qo'yib yubordi");

        s.Yangila(hammasi);
        Teg(0, mF);
        await Kadrlar(4);
        Tekshir(kirish.SensorF && p.Tugmalar.F && p.FUshlandi > 0, $"F ushlab turilibdi: {p.FUshlandi:F3} s");
        Qoy(0, mF);
        Tekshir(!kirish.SensorF && p.FUshlandi == 0, "F qo'yildi");

        s.Yangila(hammasi);
        p.Urmoqchi = false;
        Teg(0, mUr);
        Tekshir(p.Urmoqchi, "Ur: urish");
        p.Urmoqchi = false;
        Qoy(0, mUr);

        s.Yangila(hammasi);
        Teg(0, mBl);
        kirish.Oqi();
        Tekshir(p.Bloklaymi, "Bl: ushlab turilganda blok");
        Qoy(0, mBl);
        kirish.Oqi();
        Tekshir(!p.Bloklaymi, "Bl: qo'yilganda blok yo'q");

        Player.KamonBer(p);
        hammasi.Qurol = "kamon";
        s.Yangila(hammasi);
        Tekshir(!s.TugmaFaol(3) && s.TugmaYozuv(4) == "🏹", "kamon bilan blok yo'q, Qr belgisi kamon");
        Teg(0, mQr);
        Tekshir(p.Qurol == "kaltak", $"Qr: keyingi qurol {p.Qurol}");
        Qoy(0, mQr);
        Teg(0, mQr);
        Tekshir(p.Qurol == "kamon", $"Qr: yana keyingisi {p.Qurol}");
        Qoy(0, mQr);

        // a hidden button is air (pointer-events: none): the finger lands on the canvas
        s.Yangila(new SensorHolat());
        Tekshir(!s.TugmaFaol(0) && !s.TugmaFaol(1) && !s.TugmaFaol(2) && !s.TugmaFaol(4), "holat bo'sh: hammasi yashirin");
        p.EBosildi = false;
        Teg(0, mE);
        Tekshir(s.QaraBarmoq == 0 && !kirish.SensorE, "yashirin E ustidan: qarash barmog'i");

        // a look drag crossing a shown button stays a look drag
        s.Yangila(hammasi);
        yaw0 = p.Yonalish;
        Sur(0, mE, mUr + new Vector2(20, 0));
        Tekshir(p.Yonalish != yaw0 && !p.Urmoqchi, "qarash surishi tugma ustidan o'tadi");
        Qoy(0, mUr);
        p.EBosildi = false;

        // joystick and a button together
        s.Yangila(hammasi);
        Teg(0, sol);
        Teg(1, mE);
        Sur(0, sol, sol + new Vector2(52 * px, 0));
        Tekshir(s.JoyBarmoq == 0 && kirish.SensorE && Yaqin(p.Analog.Kuch, 1), "joystik + E bir vaqtda");
        Qoy(0, sol);
        Qoy(1, mE);
        Tekshir(s.JoyBarmoq < 0 && !kirish.SensorE, "ikkalasi qo'yildi");

        // ---- #yordamSensor ----
        Control jadval = BoshqaruvJadvali.Qur(10);
        AddChild(jadval);
        bool klav = jadval.GetChild<Control>(0).Visible, sens = jadval.GetChild<Control>(1).Visible;
        int satr = jadval.GetChild(1).GetChildCount();
        jadval.QueueFree();
        Tekshir(!klav && sens && satr == 4, $"boshqaruv jadvali: sensor, {satr} satr");

        // ---- pause by the button (a touch device has no Esc) ----
        Vector2 pt = _pauzaTugma!.GetGlobalRect().GetCenter();
        Teg(0, pt);
        Qoy(0, pt);
        Tekshir(_pauza && _pauzaQatlam!.IsInsideTree(), $"❚❚ tegilganda pauza kartasi ({pt})");
        await Kadrlar(2);
        List<Button> kartaTugmalari = BuxoroModels.Avlodlar<Button>(_pauzaQatlam!);
        Tekshir(kartaTugmalari.Count == 5 && kartaTugmalari[2].GetGlobalRect().Size.X > 0, $"kartada {kartaTugmalari.Count} tugma");
        Vector2 yordam = kartaTugmalari[2].GetGlobalRect().GetCenter();
        Teg(0, yordam);
        Qoy(0, yordam);
        await Kadrlar(2);
        Control klon = (Control)_pzJadval!.GetChild(1);
        Tekshir(_pzJadval.Visible && !klon.GetChild<Control>(0).Visible && klon.GetChild<Control>(1).Visible, "BOSHQARUV: sensor jadvali ko'rinadi");
        Vector2 davom = kartaTugmalari[0].GetGlobalRect().GetCenter();
        Teg(0, davom);
        Qoy(0, davom);
        Tekshir(!_pauza, "DAVOM ETTIRISH tegilganda davom etadi");
        await Kadrlar(2);
        Teg(0, _pauzaTugma.GetGlobalRect().GetCenter());
        Qoy(0, _pauzaTugma.GetGlobalRect().GetCenter());
        await Kadrlar(2);
        var fon = new Vector2(10, 10);
        Teg(0, fon);
        Qoy(0, fon);
        Tekshir(!_pauza && Input.MouseMode != Input.MouseModeEnum.Captured, "fonga tegilganda davom etadi, kursor ushlanmaydi");

        // ---- the mouse ----
        void Sichqon(Vector2 k, bool bosilgan) => Berib(new InputEventMouseButton
        {
            ButtonIndex = MouseButton.Left, Position = Oyna(k), GlobalPosition = Oyna(k), Pressed = bosilgan,
            ButtonMask = bosilgan ? MouseButtonMask.Left : 0,
        });

        p.Urmoqchi = false;
        await Kadrlar(2);
        if (Input.EmulateTouchFromMouse)
        {
            // --sensorli: the mouse is one finger, and only that
            Vector2 sur = sol + new Vector2(52 * px, 0);
            Sichqon(sol, true);
            Tekshir(s.JoyBarmoq == 0 && !p.Urmoqchi, "--sensorli: bosish = joystik, urish emas");
            Berib(new InputEventMouseMotion { Position = Oyna(sur), GlobalPosition = Oyna(sur), Relative = Oyna(sur) - Oyna(sol), ButtonMask = MouseButtonMask.Left });
            Tekshir(Yaqin(p.Analog.X, 1), $"--sensorli: surish = joystik ({p.Analog.X:F3})");
            Sichqon(sur, false);
            Tekshir(s.JoyBarmoq < 0 && !p.Urmoqchi && Input.MouseMode != Input.MouseModeEnum.Captured, "--sensorli: qo'yildi, kursor erkin");
        }
        else
        {
            // a real mouse keeps working beside the fingers, as in the web
            Sichqon(orta, true);
            bool ushladi = Input.MouseMode == Input.MouseModeEnum.Captured || DisplayServer.GetName() == "headless"; // a headless run has no cursor to capture
            Tekshir(p.Urmoqchi && ushladi, "haqiqiy sichqoncha: uradi va kursorni ushlaydi");
            Sichqon(orta, false);
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        GD.Print($"[buxoro] sensor testi: {(ok ? "PASS" : "FAIL")}");
        tree.Quit(ok ? 0 : 1);
    }

    /// <summary>
    /// <c>-- --glbsiz hammasi --binolar-compare tools/parity/binolar_web.csv</c>: the file
    /// tools/parity/binolar_web.js wrote by running the real web sources. Every mesh of every
    /// generator (bounding box, colour, roughness), the footprints and the 8317 stream after
    /// each; and with no GLB at all, the whole field: the world checkpoints and every obstacle.
    /// Without --glbsiz only the generators are compared.
    /// </summary>
    private bool BinolarCompare(World world, string yol)
    {
        if (!System.IO.File.Exists(yol))
        {
            GD.PushError($"[buxoro] binolar compare: {yol} not found");
            return false;
        }

        CultureInfo inv = CultureInfo.InvariantCulture;
        var satrlar = new List<string[]>();

        void Tosiqlar(string bosh, IReadOnlyList<Tosiq> royxat)
        {
            for (int i = 0; i < royxat.Count; i++)
            {
                Tosiq t = royxat[i];
                var q = new List<string>(bosh.Split(',')) { i.ToString(inv), t.Shakl.ToString() };
                foreach (double son in new[] { t.X, t.Z, t.R, t.R1, t.R2, t.W, t.D, t.Burilish })
                {
                    q.Add(son.ToString("R", inv));
                }

                satrlar.Add(q.ToArray());
            }
        }

        // the same calls in the same order, on one fresh stream
        var b = new Binolar(BuxoroRuntime.NewBuildingLcg());
        var generatorlar = new (string Nom, Func<(Node3D, List<Tosiq>?)> Yasa)[]
        {
            ("uy_kichik", () => (b.Uy("kichik", out List<Tosiq> iz), iz)),
            ("uy_katta", () => (b.Uy("katta", out List<Tosiq> iz), iz)),
            ("uy_vayrona", () => (b.UyVayrona(out List<Tosiq> iz), iz)),
            ("bozor_rastasi", () => (b.BozorRastasi(), null)),
            ("qala_darvoza", () => (Binolar.QalaDarvoza(out List<Tosiq> iz), iz)),
            ("shahar_devori", () => (Binolar.ShaharDevori(), null)),
            ("ark_qalasi", () => (Binolar.ArkQalasi(), null)),
            ("paxsa_devor", () => (b.PaxsaDevor(), null)),
        };

        foreach ((string nom, Func<(Node3D, List<Tosiq>?)> yasa) in generatorlar)
        {
            (Node3D g, List<Tosiq>? iz) = yasa();
            int i = 0;
            void Yur(Node n, Transform3D t)
            {
                if (n is MeshInstance3D mi)
                {
                    Aabb q = t * mi.Mesh.GetAabb();
                    var mat = (StandardMaterial3D)mi.Mesh.SurfaceGetMaterial(0);
                    Color c = mat.AlbedoColor.SrgbToLinear();
                    int hex = ((int)Math.Round(c.R * 255) << 16) | ((int)Math.Round(c.G * 255) << 8) | (int)Math.Round(c.B * 255);
                    satrlar.Add(new[]
                    {
                        "M", nom, (i++).ToString(inv),
                        q.Position.X.ToString("R", inv), q.Position.Y.ToString("R", inv), q.Position.Z.ToString("R", inv),
                        q.End.X.ToString("R", inv), q.End.Y.ToString("R", inv), q.End.Z.ToString("R", inv),
                        hex.ToString("x", inv), mat.Roughness.ToString("R", inv),
                    });
                }

                foreach (Node bola in BuxoroModels.Bolalar(n))
                {
                    Yur(bola, bola is Node3D b3 ? t * b3.Transform : t);
                }
            }

            Yur(g, g.Transform);
            g.Free();
            if (iz != null)
            {
                Tosiqlar($"T,{nom}", iz);
            }

            satrlar.Add(new[] { "D", nom, b.Lcg.DrawCount.ToString(inv) });
        }

        if (BuxoroModels.GlbsizMi)
        {
            foreach ((string nom, int draw) in Dunyo!.DrawSanoq)
            {
                satrlar.Add(new[] { "C", nom, draw.ToString(inv) });
            }

            satrlar.Add(new[] { "B", world.BinolarLcg.DrawCount.ToString(inv) });
            Tosiqlar("W", Dunyo.Tosiqlar);
        }

        var web = new List<string[]>();
        foreach (string s in System.IO.File.ReadAllLines(yol))
        {
            if (s.Length > 0 && (BuxoroModels.GlbsizMi || s[0] is 'M' or 'T' or 'D'))
            {
                web.Add(s.Split(','));
            }
        }

        int yomon = 0;
        double eng = 0;
        for (int i = 0; i < Math.Min(web.Count, satrlar.Count); i++)
        {
            string[] a = satrlar[i], w = web[i];
            bool mos = a.Length == w.Length;
            for (int k = 0; mos && k < a.Length; k++)
            {
                if (a[k] == w[k])
                {
                    continue;
                }

                if (double.TryParse(a[k], NumberStyles.Float, inv, out double x) && double.TryParse(w[k], NumberStyles.Float, inv, out double y))
                {
                    eng = Math.Max(eng, Math.Abs(x - y));
                    mos = Math.Abs(x - y) <= 1e-4;
                }
                else
                {
                    mos = false;
                }
            }

            if (!mos && yomon++ < 12)
            {
                GD.Print($"[buxoro]   diff  godot {string.Join(',', a)}");
                GD.Print($"[buxoro]         web   {string.Join(',', w)}");
            }
        }

        bool ok = yomon == 0 && web.Count == satrlar.Count;
        GD.Print($"[buxoro] binolar compare: {satrlar.Count} satr (web {web.Count}), {yomon} farq, max {eng:E2} — {(ok ? "PASS" : "FAIL")}");
        return ok;
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
