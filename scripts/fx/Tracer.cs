using Godot;

namespace ChronoShift;

/// <summary>
/// The visible streak of a shot: a thin beam stretched from the muzzle to the
/// impact point that fades out and frees itself.
/// </summary>
public partial class Tracer : Node3D
{
    [Export] private NodePath _beamPath = "Beam";
    [Export] private float _fadeSeconds = 0.08f;

    private MeshInstance3D _beam = null!;
    private float _elapsed;

    /// <summary>Positions the beam between two world points. Call right after instancing.</summary>
    public void Stretch(Vector3 from, Vector3 to)
    {
        _beam = GetNode<MeshInstance3D>(_beamPath);

        Vector3 delta = to - from;
        float length = delta.Length();
        if (length < 0.01f)
        {
            QueueFree();
            return;
        }

        GlobalPosition = from + delta * 0.5f;

        // LookAt fails when the shot runs parallel to the up vector — pick another.
        Vector3 up = Mathf.Abs(delta.Normalized().Dot(Vector3.Up)) > 0.99f ? Vector3.Right : Vector3.Up;
        LookAt(to, up);

        // The beam mesh is a unit cylinder along Z; scale it to span the shot.
        _beam.Scale = new Vector3(1, 1, length);
    }

    public override void _Process(double delta)
    {
        _elapsed += (float)delta;
        float remaining = 1.0f - Mathf.Clamp(_elapsed / _fadeSeconds, 0.0f, 1.0f);

        Modulate3D(remaining);

        if (remaining <= 0.0f)
        {
            QueueFree();
        }
    }

    private void Modulate3D(float alpha)
    {
        if (_beam?.GetActiveMaterial(0) is StandardMaterial3D material)
        {
            Color color = material.AlbedoColor;
            color.A = alpha;
            material.AlbedoColor = color;
        }
    }
}
