using Godot;

namespace ChronoShift;

/// <summary>
/// The single access point for every Buxoro tuning value. Register in
/// Project Settings → Autoload as <c>BuxoroRuntime</c>.
/// </summary>
/// <remarks>
/// WHY THIS EXISTS. The web build opens balance.js with a promise: every number is
/// in this file, no magic numbers anywhere else. It did not keep that promise —
/// roughly forty gameplay values ended up scattered through world.js, player.js,
/// apprentices.js and combat.js, and nobody noticed because nothing enforced it.
///
/// Routing every read through one autoload makes the rule structural instead of a
/// matter of discipline: a Buxoro script that wants a number has exactly one place
/// to get it, and a number that is not in a .tres has no way to reach the game.
///
/// It fails loudly rather than limping. A null table here means every downstream
/// system silently falls back to zero, which reads as "the game is broken" rather
/// than "one resource path is wrong", and would cost hours to trace.
/// </remarks>
public partial class BuxoroRuntime : Node
{
    private const string BalancePath = "res://resources/data/buxoro/balance_buxoro.tres";
    private const string WorldPath = "res://resources/data/buxoro/world_buxoro.tres";

    public static BuxoroRuntime Instance { get; private set; } = null!;

    /// <summary>Everything transcribed from balance.js.</summary>
    [Export] public BuxoroBalance? Balance;

    /// <summary>world.js literals balance.js does not carry (ground, light, wall ring).</summary>
    [Export] public BuxoroWorld? World;

    /// <summary>True when every table loaded and passed its own checks.</summary>
    public bool IsReady { get; private set; }

    /// <summary>Shorthand for the balance table. Throws if startup failed.</summary>
    public static BuxoroBalance B => Instance.Balance
        ?? throw new System.InvalidOperationException(
            "BuxoroRuntime.Balance is null — see the startup error.");

    /// <summary>Shorthand for the world table. Throws if startup failed.</summary>
    public static BuxoroWorld W => Instance.World
        ?? throw new System.InvalidOperationException(
            "BuxoroRuntime.World is null — see the startup error.");

    public override void _Ready()
    {
        Instance = this;

        // DEBUG: -- --sensorli — plays the touch controls with the mouse as one finger
        if (System.Array.IndexOf(OS.GetCmdlineUserArgs(), "--sensorli") >= 0)
        {
            Input.EmulateTouchFromMouse = true;
        }

        Balance ??= ResourceLoader.Load<BuxoroBalance>(BalancePath);
        World ??= ResourceLoader.Load<BuxoroWorld>(WorldPath);

        if (World == null)
        {
            GD.PushError($"[buxoro] BuxoroRuntime: {WorldPath} did not load as BuxoroWorld.");
            return;
        }

        if (Balance == null)
        {
            GD.PushError(
                $"[buxoro] BuxoroRuntime: {BalancePath} did not load as BuxoroBalance. " +
                "Every Buxoro system reads its numbers from here, so nothing downstream " +
                "will behave correctly until this is fixed.");
            return;
        }

        IsReady = Balance.Tekshir();

        if (IsReady)
        {
            GD.Print(
                $"[buxoro] runtime ready — {Balance.TugunSoni.Count} node types, " +
                $"{Balance.Tolqin.Count} waves, {Balance.JamiDushman} enemies to win, " +
                $"clock {Balance.BoshlangichVaqt:F0}s.");
        }
    }

    /// <summary>A real finger anywhere, in any scene, makes this a touch device (see <see cref="Touch.Bormi"/>).</summary>
    public override void _Input(InputEvent e)
    {
        if (e is InputEventScreenTouch && e.Device != InputEvent.DeviceIdEmulation)
        {
            Touch.Tegildi();
        }
    }

    /// <summary>
    /// The last moment the engine is whole at shutdown: an autoload leaves the tree only
    /// when the app quits, after the current scene. A Godot collection is freed by the
    /// .NET finalizer unless disposed, and one finalized while the native side tears down
    /// crashes the exit (0xC0000005 in godotsharp_array_destroy — measured: 1 of 26
    /// --test-harakat exits). So run the pending finalizers now and allow no further GC:
    /// nothing new can reach the finalizer queue, and Godot's own DisposablesTracker
    /// disposes everything still alive.
    /// </summary>
    public override void _ExitTree()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        try
        {
            GC.TryStartNoGCRegion(64L * 1024 * 1024); // far more than shutdown allocates; past it GC simply resumes
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            GD.PushWarning("[buxoro] no-GC region at exit: " + ex.Message);
        }

        GC.WaitForPendingFinalizers(); // starting the region may itself have collected
    }

    /// <summary>A fresh world generator. Seeded 1238, exactly as world.js:10.</summary>
    public static Lcg NewWorldLcg() => new(Lcg.WorldSeed);

    /// <summary>A fresh procedural-building generator. Seeded 8317, binolar.js:16.</summary>
    public static Lcg NewBuildingLcg() => new(Lcg.BuildingSeed);

    /// <summary>A fresh grass generator. Seeded 4711, otlar.js:13.</summary>
    public static Lcg NewGrassLcg() => new(Lcg.GrassSeed);
}
