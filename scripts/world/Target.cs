using Godot;

namespace ChronoShift;

/// <summary>Destructible target: flashes on hit, pays out money and frees itself when destroyed.</summary>
public partial class Target : StaticBody3D, IDamageable
{
    [Export] private float _health = 50.0f;

    [Export] private AudioStream? _destroySound;
    [Export] private int _rewardMoney = 15;
    [Export] private NodePath _meshPath = "MeshInstance3D";
    [Export] private Color _normalColor = new Color(0.8f, 0.2f, 0.2f);
    [Export] private Color _hitColor = new Color(1f, 1f, 1f);

    private MeshInstance3D _mesh = null!;
    private StandardMaterial3D _material = null!;
    private float _current;

    public override void _Ready()
    {
        _current = _health;
        _mesh = GetNode<MeshInstance3D>(_meshPath);
        _material = new StandardMaterial3D
        {
            AlbedoColor = _normalColor,
        };
        _mesh.SetSurfaceOverrideMaterial(0, _material);
    }

    public void TakeDamage(float amount, Node3D source)
    {
        _current -= amount;

        _material.AlbedoColor = _hitColor;
        GetTree().CreateTimer(0.1).Timeout += () =>
        {
            if (IsInstanceValid(_material))
            {
                _material.AlbedoColor = _normalColor;
            }
        };

        if (_current <= 0)
        {
            EconomyManager.Instance?.AddMoney(_rewardMoney);
            // Played before the node goes: Sfx parents the player to the scene,
            // so the sound outlives the target that made it.
            Sfx.PlayAt(this, _destroySound, GlobalPosition);
            QueueFree();
        }
    }
}
