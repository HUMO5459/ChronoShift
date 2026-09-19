using Godot;

namespace ChronoShift;

/// <summary>One Mongol wave: when it arrives and what is in it.</summary>
/// <remarks>
/// Transcribed from balance.js TOLQIN (balance.js:74-78). The trigger is a CLOCK
/// value, not elapsed time, because the clock runs backwards and a built wall
/// pushes it up again — a player who builds well genuinely delays the wave.
/// </remarks>
[GlobalClass]
public partial class BuxoroWaveRow : Resource
{
    /// <summary>Clock value at which the wave spawns (480 = 8:00).</summary>
    [Export] public float Vaqt;

    [Export] public int Piyoda;
    [Export] public int Otliq;
    [Export] public int Noyon;

    /// <summary>Total enemies in this wave.</summary>
    public int Jami => Piyoda + Otliq + Noyon;
}
