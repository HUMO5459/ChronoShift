using Godot;

namespace ChronoShift;

/// <summary>Stats for one enemy type.</summary>
/// <remarks>Transcribed from balance.js DUSHMAN (balance.js:64-68).</remarks>
[GlobalClass]
public partial class BuxoroEnemySpec : Resource
{
    /// <summary>"piyoda", "otliq" or "noyon".</summary>
    [Export] public string Tur = "";

    [Export] public float Hp;
    [Export] public float Zarar;
    [Export] public float Tezlik;

    /// <summary>Seconds between attacks.</summary>
    [Export] public float Hujum;
}
