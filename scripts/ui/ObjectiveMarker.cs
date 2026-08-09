using Godot;

namespace ChronoShift;

/// <summary>
/// Keeps the current objective on screen at all times: a diamond over it when it is
/// in view, and an arrow pinned to the edge pointing the way when it is not. Before
/// this the only pointer was a panel that hid once the player walked away, so a
/// mission's next objective was invisible from anywhere but on top of it.
/// </summary>
public partial class ObjectiveMarker : Control
{
    /// <summary>How far from the screen edge the off-screen arrow sits.</summary>
    [Export] private float _edgePadding = 90.0f;

    /// <summary>Height above the objective the diamond floats at, in metres.</summary>
    [Export] private float _worldHeight = 2.2f;

    [Export] private float _diamondRadius = 11.0f;
    [Export] private float _arrowRadius = 15.0f;

    private Label _label = null!;
    private Vector2 _point;
    private float _arrowAngle;
    private bool _onScreen;
    private bool _visible;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsPreset(LayoutPreset.FullRect);

        _label = new Label { MouseFilter = MouseFilterEnum.Ignore };
        _label.ThemeTypeVariation = "MonoLabel";
        _label.AddThemeFontSizeOverride("font_size", 13);
        _label.AddThemeColorOverride("font_color", UiPalette.Highlight);

        // The marker floats over whatever the world happens to be — pale sky, bright
        // ground — so it carries its own dark outline rather than trusting contrast.
        _label.AddThemeColorOverride("font_outline_color", new Color(0.02f, 0.04f, 0.07f, 0.85f));
        _label.AddThemeConstantOverride("outline_size", 5);
        AddChild(_label);
    }

    public override void _Process(double delta)
    {
        _visible = false;
        _label.Visible = false;

        EngineeringTask? objective = FindObjective();
        Camera3D? camera = GetViewport()?.GetCamera3D();
        if (objective == null || camera == null)
        {
            QueueRedraw();
            return;
        }

        Vector2 size = GetViewportRect().Size;
        Vector3 world = objective.GlobalPosition + Vector3.Up * _worldHeight;
        Vector2 centre = size * 0.5f;
        Vector2 screen = camera.UnprojectPosition(world);

        // Behind the camera, the projection folds back onto the screen and points the
        // wrong way; mirroring through the centre puts the arrow on the correct side.
        Vector3 local = camera.GlobalTransform.AffineInverse() * world;
        bool behind = local.Z > 0.0f;
        if (behind)
        {
            screen = centre + (centre - screen);
        }

        // Padding is capped so a small window can never invert the rectangle.
        float pad = Mathf.Min(_edgePadding, Mathf.Min(size.X, size.Y) * 0.25f);
        var bounds = new Rect2(
            new Vector2(pad, pad),
            new Vector2(Mathf.Max(1.0f, size.X - pad * 2.0f), Mathf.Max(1.0f, size.Y - pad * 2.0f)));

        _onScreen = !behind && bounds.HasPoint(screen);
        _point = _onScreen
            ? screen
            : new Vector2(
                Mathf.Clamp(screen.X, bounds.Position.X, bounds.End.X),
                Mathf.Clamp(screen.Y, bounds.Position.Y, bounds.End.Y));

        Vector2 away = _point - centre;
        _arrowAngle = away.LengthSquared() > 0.01f ? away.Angle() : 0.0f;

        float distance = camera.GlobalPosition.DistanceTo(objective.GlobalPosition);
        _label.Text = $"{objective.InteractionPrompt.ToUpperInvariant()} · {distance:0} M";
        _label.Size = _label.GetMinimumSize();
        _label.Position = new Vector2(
            Mathf.Clamp(_point.X - _label.Size.X * 0.5f, 8.0f, Mathf.Max(8.0f, size.X - _label.Size.X - 8.0f)),
            _point.Y + (_onScreen ? -_diamondRadius - _label.Size.Y - 8.0f : _arrowRadius + 8.0f));
        _label.Visible = true;

        _visible = true;
        QueueRedraw();
    }

    /// <summary>
    /// The mission map's current objective. Off-mission (the hub, or a map with no
    /// MissionMap root) it falls back to the nearest unfinished task, so the hub's
    /// own work still gets a pointer.
    /// </summary>
    private EngineeringTask? FindObjective()
    {
        if (GetTree().GetFirstNodeInGroup(MissionMap.MapGroup) is MissionMap map)
        {
            return map.CurrentObjective;
        }

        Camera3D? camera = GetViewport()?.GetCamera3D();
        if (camera == null)
        {
            return null;
        }

        EngineeringTask? nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Node node in GetTree().GetNodesInGroup(EngineeringTask.TaskGroup))
        {
            if (node is not EngineeringTask { IsCompleted: false } task)
            {
                continue;
            }

            float distance = task.GlobalPosition.DistanceTo(camera.GlobalPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = task;
            }
        }

        return nearest;
    }

    public override void _Draw()
    {
        if (!_visible)
        {
            return;
        }

        Color fill = UiPalette.Highlight;
        Color outline = new(0.02f, 0.04f, 0.07f, 0.85f);

        if (_onScreen)
        {
            Vector2[] diamond =
            {
                _point + new Vector2(0, -_diamondRadius),
                _point + new Vector2(_diamondRadius, 0),
                _point + new Vector2(0, _diamondRadius),
                _point + new Vector2(-_diamondRadius, 0),
            };

            DrawColoredPolygon(diamond, fill);
            DrawPolyline(new[] { diamond[0], diamond[1], diamond[2], diamond[3], diamond[0] }, outline, 2.0f);
            return;
        }

        // Off screen: a triangle at the clamped point, nose out toward the objective.
        Vector2 nose = _point + Vector2.FromAngle(_arrowAngle) * _arrowRadius;
        Vector2 left = _point + Vector2.FromAngle(_arrowAngle + 2.4f) * _arrowRadius;
        Vector2 right = _point + Vector2.FromAngle(_arrowAngle - 2.4f) * _arrowRadius;

        DrawColoredPolygon(new[] { nose, left, right }, fill);
        DrawPolyline(new[] { nose, left, right, nose }, outline, 2.0f);
    }
}
