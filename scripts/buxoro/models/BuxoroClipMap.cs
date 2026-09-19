using Godot;

namespace ChronoShift;

/// <summary>
/// Exporter clip name to game clip name, per model.
/// </summary>
/// <remarks>
/// Transcribed from Torobiy/src/assets/models/klip-xaritasi.json, which is itself
/// incomplete: it names 3 of shomurod's 8 clips and says so in its own header.
/// Unmapped clips keep their imported name, exactly as the web build does
/// (models.js:46-55), and the game falls back to procedural motion for anything
/// it cannot find.
/// </remarks>
[GlobalClass]
public partial class BuxoroClipMap : Resource
{
    [Export] public Godot.Collections.Array<BuxoroClipRow> Clips = new();

    /// <summary>Row for this model and imported clip name, or null when unmapped.</summary>
    public BuxoroClipRow? Find(string model, string glbClip)
    {
        foreach (BuxoroClipRow row in Clips)
        {
            if (row != null && row.Model == model && row.GlbClip == glbClip)
            {
                return row;
            }
        }

        return null;
    }
}
