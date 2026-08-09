using Godot;

namespace ChronoShift;

/// <summary>
/// Stops a prop's geometry drawing past a given distance. The heavy props here
/// are detailed models used as scenery — parked aircraft, weapons on a rack —
/// and on the low-end hardware this project targets their triangle count costs
/// far more than their silhouette is worth from across the map.
///
/// Applied to every GeometryInstance3D below this node, including meshes that
/// were instanced at runtime, so it works on props built by code as well.
/// </summary>
public partial class DistanceCull : Node3D
{
    /// <summary>Metres past which the geometry stops being drawn.</summary>
    [Export] private float _range = 90.0f;

    /// <summary>Fade band before the cut, so the pop is softened.</summary>
    [Export] private float _margin = 12.0f;

    public override void _Ready() => CallDeferred(MethodName.Apply);

    /// <summary>Public so props that build their model in code can re-apply after adding it.</summary>
    public void Apply() => ApplyTo(this, _range, _margin);

    /// <summary>Sets the range on every drawable below <paramref name="root"/>.</summary>
    public static void ApplyTo(Node root, float range, float margin)
    {
        if (root is GeometryInstance3D geometry)
        {
            geometry.VisibilityRangeEnd = range;
            geometry.VisibilityRangeEndMargin = margin;
        }

        foreach (Node child in root.GetChildren())
        {
            ApplyTo(child, range, margin);
        }
    }
}
