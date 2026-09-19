using Godot;

namespace ChronoShift;

/// <summary>What one buildable costs.</summary>
/// <remarks>
/// Transcribed from balance.js RETSEPT (balance.js:59-63). Stored as five explicit
/// fields rather than a dictionary so the .tres stays readable and a missing
/// resource type cannot hide behind a key typo.
/// </remarks>
[GlobalClass]
public partial class BuxoroRecipeRow : Resource
{
    /// <summary>"devor", "kamon" or "qilich".</summary>
    [Export] public string Nomi = "";

    [Export] public int Tosh;
    [Export] public int Yogoch;
    [Export] public int Teri;
    [Export] public int Temir;
    [Export] public int Komir;
}
