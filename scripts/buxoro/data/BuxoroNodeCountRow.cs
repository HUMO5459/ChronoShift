using Godot;

namespace ChronoShift;

/// <summary>How many nodes of one resource type the field holds.</summary>
/// <remarks>
/// Transcribed from balance.js TUGUN_SONI (balance.js:56).
///
/// ORDER IS LOAD-BEARING. The world generator walks these in array order and draws
/// from the deterministic stream as it goes, so tosh, yogoch, teri, temir, komir is
/// not a presentation choice: reordering this array moves all 66 nodes.
/// </remarks>
[GlobalClass]
public partial class BuxoroNodeCountRow : Resource
{
    /// <summary>"tosh", "yogoch", "teri", "temir" or "komir".</summary>
    [Export] public string Tur = "";

    [Export] public int Soni;

    /// <summary>
    /// Units left in one node of this type. world.js:205 gives stone 4 and
    /// everything else 2, contradicting the balance.js comment that says 1.
    /// </summary>
    [Export] public int Qoldi = 2;

    /// <summary>
    /// Draws this type consumes inside TugunYarat's procedural branch, beyond the
    /// two position draws. Zero when the type's GLB exists and the branch is
    /// skipped. This is determinism data, not cosmetics.
    /// </summary>
    [Export] public int ProtseduralDraw;

    /// <summary>
    /// Zone this type scatters in, as (minX, maxX, minZ, maxZ). Source:
    /// world.js:82-88. Each node draws X first, then Z, from this box.
    /// </summary>
    [Export] public Vector4 Zona;

    /// <summary>Name of the GLB whose presence skips the procedural branch.</summary>
    [Export] public string GlbNomi = "";
}
