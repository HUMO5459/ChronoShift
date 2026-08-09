using Godot;

namespace ChronoShift;

/// <summary>
/// Root for a fire-and-forget effect: fades its flash light, then frees itself.
/// Spawners can drop one in the world and forget about it.
/// </summary>
public partial class OneShotFx : Node3D
{
    [Export] private float _lifetime = 0.8f;

    /// <summary>Optional flash that decays over <see cref="_flashSeconds"/>.</summary>
    [Export] private NodePath _lightPath = new();
    [Export] private float _flashSeconds = 0.08f;

    private OmniLight3D? _light;
    private float _lightEnergy;
    private float _elapsed;

    public override void _Ready()
    {
        _light = _lightPath.IsEmpty ? null : GetNodeOrNull<OmniLight3D>(_lightPath);
        if (_light != null)
        {
            _lightEnergy = _light.LightEnergy;
        }

        GetTree().CreateTimer(_lifetime).Timeout += () =>
        {
            if (IsInstanceValid(this))
            {
                QueueFree();
            }
        };
    }

    public override void _Process(double delta)
    {
        if (_light == null)
        {
            SetProcess(false);
            return;
        }

        _elapsed += (float)delta;
        float remaining = 1.0f - Mathf.Clamp(_elapsed / _flashSeconds, 0.0f, 1.0f);
        _light.LightEnergy = _lightEnergy * remaining;

        if (remaining <= 0.0f)
        {
            _light.Visible = false;
            SetProcess(false);
        }
    }
}
