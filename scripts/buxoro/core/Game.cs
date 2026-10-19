using Godot;

namespace ChronoShift;

/// <summary>The game state game.js <c>yarat()</c> returns — <c>O</c> (game.js:74-94).</summary>
public sealed class OyinHolati
{
    public Node3D Scene = null!;
    public Camera3D Camera = null!;
    public Dunyo Dunyo = null!;
    public Oyinchi Oyinchi = null!;

    public double Vaqt;
    public double Otgan;
    public int Parda = 1;

    /// <summary>Non-null once an ending fired (game.js:590).</summary>
    public string? Tugadi;
}

/// <summary>
/// The orchestrator, game.js. <see cref="Yangila"/> keeps game.js's call order
/// (game.js:121-180) — the mirror's central claim, checked by reading.
/// Systems land step by step; each call below names its source line.
/// </summary>
public static class Game
{
    private static BuxoroBalance B => BuxoroRuntime.B;

    /// <summary>game.js:66-95.</summary>
    public static OyinHolati Yarat(Node3D scene, Camera3D camera, Dunyo dunyo)
    {
        var o = new OyinHolati
        {
            Scene = scene,
            Camera = camera,
            Dunyo = dunyo,
            Oyinchi = Player.Yarat(scene),
            Vaqt = B.BoshlangichVaqt,
        };
        return o;
    }

    /// <summary>game.js:121-180.</summary>
    public static void Yangila(OyinHolati o, double dt)
    {
        if (o.Tugadi != null)
        {
            return;
        }

        o.Otgan += dt;
        o.Vaqt -= dt;

        Player.Yangila(o.Oyinchi, dt, o.Dunyo);
    }
}
