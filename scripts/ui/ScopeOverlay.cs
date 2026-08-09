using Godot;

namespace ChronoShift;

/// <summary>
/// The sniper's view: everything outside the scope circle is blacked out, with a
/// reticle drawn in the middle. Drawn rather than textured so it fits any
/// resolution without an asset.
/// </summary>
public partial class ScopeOverlay : Control
{
    /// <summary>Scope circle radius as a share of the screen height.</summary>
    [Export] private float _radiusFraction = 0.42f;

    [Export] private Color _mask = new(0.0f, 0.0f, 0.0f, 0.96f);
    [Export] private Color _reticle = new(0.05f, 0.06f, 0.05f, 0.9f);

    /// <summary>Gap around the centre so the target is never covered by the cross.</summary>
    [Export] private float _centreGap = 26.0f;

    public override void _Ready()
    {
        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
        Resized += QueueRedraw;
    }

    public override void _Draw()
    {
        Vector2 size = Size;
        Vector2 centre = size * 0.5f;
        float radius = size.Y * _radiusFraction;

        // Everything outside the circle is one very thick ring — thick enough to
        // reach past the corners — which keeps the edge exactly round.
        int segments = 128;
        float thickness = size.Length();
        DrawArc(centre, radius + thickness * 0.5f, 0.0f, Mathf.Tau, segments, _mask, thickness, false);

        DrawArc(centre, radius, 0.0f, Mathf.Tau, segments, _reticle, 3.0f, true);

        // Crosshair: full-width hairlines with a gap at the centre.
        DrawLine(new Vector2(centre.X - radius, centre.Y), new Vector2(centre.X - _centreGap, centre.Y), _reticle, 1.5f, true);
        DrawLine(new Vector2(centre.X + _centreGap, centre.Y), new Vector2(centre.X + radius, centre.Y), _reticle, 1.5f, true);
        DrawLine(new Vector2(centre.X, centre.Y - radius), new Vector2(centre.X, centre.Y - _centreGap), _reticle, 1.5f, true);
        DrawLine(new Vector2(centre.X, centre.Y + _centreGap), new Vector2(centre.X, centre.Y + radius), _reticle, 1.5f, true);

        // Range ticks down the lower hairline, as a rifle scope carries.
        for (int i = 1; i <= 4; i++)
        {
            float y = centre.Y + _centreGap + i * (radius - _centreGap) / 5.0f;
            float half = 7.0f - i;
            DrawLine(new Vector2(centre.X - half, y), new Vector2(centre.X + half, y), _reticle, 1.5f, true);
        }
    }
}
