using Godot;

namespace ChronoShift;

/// <summary>
/// Screen feedback for taking fire: a red wash that fades, plus an arc around the
/// crosshair for each recent hit pointing back at whoever fired it. Drawn rather
/// than built from nodes, because the arcs are transient and vary in number.
/// </summary>
public partial class DamageIndicator : Control
{
    /// <summary>Seconds a hit's direction arc stays on screen.</summary>
    [Export] private float _arcSeconds = 1.6f;

    /// <summary>Seconds the red wash takes to clear.</summary>
    [Export] private float _flashSeconds = 0.55f;

    /// <summary>Alpha of the wash for a hit worth a full health bar; small hits scale down.</summary>
    [Export] private float _maxFlashAlpha = 0.42f;

    /// <summary>Distance from the crosshair to the arcs, in pixels at the design resolution.</summary>
    [Export] private float _arcRadius = 190.0f;

    [Export] private float _arcHalfWidthDegrees = 22.0f;
    [Export] private float _arcThickness = 7.0f;
    [Export] private Color _damageColor = new(0.86f, 0.16f, 0.14f);

    /// <summary>Live direction arcs: screen-space angle in radians, and remaining life.</summary>
    private readonly System.Collections.Generic.List<(float Angle, float Life)> _arcs = new();

    private float _flash;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsPreset(LayoutPreset.FullRect);
    }

    /// <summary>
    /// Records a hit. <paramref name="origin"/> is where it came from; when
    /// <paramref name="hasSource"/> is false only the wash plays, with no arc.
    /// </summary>
    public void Register(float amount, float maxHealth, Vector3 origin, bool hasSource)
    {
        float share = maxHealth > 0.0f ? Mathf.Clamp(amount / maxHealth, 0.0f, 1.0f) : 0.5f;

        // A single rifle round is a small share of the bar, so the wash is floored:
        // otherwise being shot barely registered.
        _flash = Mathf.Max(_flash, Mathf.Lerp(0.45f, 1.0f, share));

        Camera3D? camera = GetViewport()?.GetCamera3D();
        if (hasSource && camera != null)
        {
            // Angle in the camera's own frame: 0 is dead ahead, positive to the right.
            Vector3 local = camera.GlobalTransform.AffineInverse() * origin;
            float bearing = Mathf.Atan2(local.X, -local.Z);

            // Screen space measures from +X, so rotate a quarter turn to put 0 at the top.
            _arcs.Add((bearing - Mathf.Pi * 0.5f, _arcSeconds));
        }

        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_flash <= 0.0f && _arcs.Count == 0)
        {
            return;
        }

        float dt = (float)delta;

        if (_flash > 0.0f)
        {
            _flash = Mathf.MoveToward(_flash, 0.0f, dt / Mathf.Max(0.01f, _flashSeconds));
        }

        for (int i = _arcs.Count - 1; i >= 0; i--)
        {
            float life = _arcs[i].Life - dt;
            if (life <= 0.0f)
            {
                _arcs.RemoveAt(i);
            }
            else
            {
                _arcs[i] = (_arcs[i].Angle, life);
            }
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        // The viewport, not this control's own rect — under a CanvasLayer a Control
        // built in code reports Size (0, 0), and the wash would draw nothing at all.
        Vector2 size = GetViewportRect().Size;

        if (_flash > 0.0f)
        {
            DrawRect(new Rect2(Vector2.Zero, size), _damageColor.With(_flash * _maxFlashAlpha));
        }

        if (_arcs.Count == 0)
        {
            return;
        }

        Vector2 centre = size * 0.5f;
        float half = Mathf.DegToRad(_arcHalfWidthDegrees);

        foreach ((float angle, float life) in _arcs)
        {
            float fade = Mathf.Clamp(life / _arcSeconds, 0.0f, 1.0f);
            DrawArc(
                centre,
                _arcRadius,
                angle - half,
                angle + half,
                24,
                _damageColor.With(fade),
                _arcThickness,
                true);
        }
    }
}
