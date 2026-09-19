using Godot;

namespace ChronoShift;

/// <summary>
/// Renames one exporter-generated animation clip to the name the game asks for.
/// </summary>
/// <remarks>
/// Tripo and Meshy export clips as "NlaTrack", "NlaTrack.001" and so on, which
/// carry no meaning. The web build mapped them at load time through
/// klip-xaritasi.json; the same mapping is applied here at import time.
///
/// IMPORTANT: Godot's glTF importer replaces the dot in a clip name with an
/// underscore, so "NlaTrack.001" arrives as "NlaTrack_001". Measured on Godot
/// 4.7.1. <see cref="GlbClip"/> therefore holds the SANITISED name.
/// </remarks>
[GlobalClass]
public partial class BuxoroClipRow : Resource
{
    /// <summary>Model this rename applies to, matching the .glb basename.</summary>
    [Export] public string Model = "";

    /// <summary>Clip name as Godot imports it, with underscores (e.g. "NlaTrack_003").</summary>
    [Export] public string GlbClip = "";

    /// <summary>Name the game code uses (e.g. "walk").</summary>
    [Export] public string GameClip = "";

    /// <summary>
    /// Loop this clip. Locomotion and idles loop; one-shots such as an attack or a
    /// death do not. The web build decided this per play call
    /// (anim.js:49); baking it into the imported Animation gives the same result
    /// for every looping clip and leaves one-shots to the runtime.
    /// </summary>
    [Export] public bool Loop = true;
}
