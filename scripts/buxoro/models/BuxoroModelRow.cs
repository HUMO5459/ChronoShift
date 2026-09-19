using Godot;

namespace ChronoShift;

/// <summary>
/// One model's import contract for the Buxoro 1238 episode.
/// </summary>
/// <remarks>
/// Transcribed 1:1 from the web build's <c>MOLJAL</c> and <c>IKKI_TOMON</c> tables
/// (Torobiy/src/js/models.js:187-207). The exporters produce roughly unit-tall
/// meshes, so every model is rescaled on import to the height the game world
/// actually assumes. Changing a value here changes how the game reads, not just
/// how it looks: Noyon towering over a footman is a design statement.
/// </remarks>
[GlobalClass]
public partial class BuxoroModelRow : Resource
{
    /// <summary>Game-side model name, matching the .glb basename (e.g. "shomurod").</summary>
    [Export] public string Name = "";

    /// <summary>
    /// Intended height in metres. The importer scales the model so its bounding
    /// box stands this tall. Source: models.js MOLJAL.
    /// </summary>
    [Export] public float TargetHeight = 0.0f;

    /// <summary>
    /// Draw both faces. The source meshes are single-sided, so a wall viewed from
    /// inside the city simply vanishes. Source: models.js IKKI_TOMON.
    /// </summary>
    [Export] public bool TwoSided;
}
