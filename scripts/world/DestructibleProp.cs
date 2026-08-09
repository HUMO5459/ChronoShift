using Godot;

namespace ChronoShift;

/// <summary>
/// World scenery that can be shot apart: takes damage, darkens as it is worn
/// down, then blows up and removes itself. Attach to any StaticBody3D prop.
/// </summary>
public partial class DestructibleProp : StaticBody3D, IDamageable
{
    [Signal] public delegate void DestroyedEventHandler();

    [Export] private float _health = 100.0f;

    /// <summary>Meshes tinted as damage accumulates; leave empty to skip the tint.</summary>
    [Export] private Godot.Collections.Array<NodePath> _meshPaths = new();

    /// <summary>How dark the prop goes at the point of collapse.</summary>
    [Export] private float _damageDarkening = 0.55f;

    [Export] private PackedScene? _destroyFxScene;

    /// <summary>Paid out when the prop is destroyed; 0 for pure scenery.</summary>
    [Export] private int _rewardMoney;

    private readonly System.Collections.Generic.List<(MeshInstance3D Mesh, StandardMaterial3D Material, Color Base)>
        _tinted = new();

    private float _current;

    public override void _Ready()
    {
        _current = _health;

        foreach (NodePath path in _meshPaths)
        {
            var mesh = GetNodeOrNull<MeshInstance3D>(path);
            if (mesh == null)
            {
                continue;
            }

            // Duplicate so one damaged prop never tints every other prop sharing the material.
            if (mesh.GetActiveMaterial(0) is not StandardMaterial3D source)
            {
                continue;
            }

            var material = (StandardMaterial3D)source.Duplicate();
            mesh.SetSurfaceOverrideMaterial(0, material);
            _tinted.Add((mesh, material, material.AlbedoColor));
        }
    }

    public void TakeDamage(float amount, Node3D source)
    {
        if (_current <= 0.0f)
        {
            return;
        }

        _current -= amount;

        float wear = 1.0f - Mathf.Clamp(_current / _health, 0.0f, 1.0f);
        foreach ((MeshInstance3D _, StandardMaterial3D material, Color baseColor) in _tinted)
        {
            material.AlbedoColor = baseColor.Darkened(_damageDarkening * wear);
        }

        if (_current <= 0.0f)
        {
            Destroy();
        }
    }

    private void Destroy()
    {
        Fx.SpawnImpact(_destroyFxScene, this, GlobalPosition + Vector3.Up, Vector3.Up);

        if (_rewardMoney > 0)
        {
            EconomyManager.Instance?.AddMoney(_rewardMoney);
        }

        EmitSignal(SignalName.Destroyed);
        QueueFree();
    }
}
