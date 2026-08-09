using Godot;

namespace ChronoShift;

/// <summary>
/// The player's health: takes enemy fire, regenerates after a lull, and on death
/// heals and respawns the player at the map's spawn point (arcade — no penalty).
/// The HUD reads it through the "player_health" group.
/// </summary>
public partial class PlayerHealth : Node
{
    [Export] public float MaxHealth = 100.0f;

    /// <summary>Health regained per second once the regen delay has passed.</summary>
    [Export] private float _regenPerSecond = 7.0f;

    /// <summary>Seconds without being hit before health starts regenerating.</summary>
    [Export] private float _regenDelay = 4.0f;

    /// <summary>Grace period after a respawn (and spawn) during which no damage lands.</summary>
    [Export] private float _spawnGrace = 2.0f;

    [Signal] public delegate void HealthChangedEventHandler(float current, float max);
    [Signal] public delegate void DiedEventHandler();

    /// <summary>
    /// A hit landed. Carries where it came from so the HUD can point at the shooter;
    /// <paramref name="hasSource"/> is false for damage with no position (falls, script).
    /// </summary>
    [Signal] public delegate void DamagedEventHandler(float amount, Vector3 origin, bool hasSource);

    public float Health { get; private set; }
    public float Fraction => MaxHealth > 0.0f ? Mathf.Clamp(Health / MaxHealth, 0.0f, 1.0f) : 0.0f;
    public bool IsInvulnerable => _invuln > 0.0f;

    private CharacterBody3D? _body;
    private float _sinceHit;
    private float _invuln;

    public override void _Ready()
    {
        AddToGroup("player_health");
        _body = GetParent() as CharacterBody3D;
        Health = MaxHealth;
        _invuln = _spawnGrace;
        EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (_invuln > 0.0f)
        {
            _invuln = Mathf.MoveToward(_invuln, 0.0f, dt);
        }

        _sinceHit += dt;
        if (_sinceHit >= _regenDelay && Health < MaxHealth)
        {
            Health = Mathf.MoveToward(Health, MaxHealth, _regenPerSecond * dt);
            EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
        }
    }

    /// <summary>
    /// Applies enemy damage; ignored while invulnerable or already at zero.
    /// <paramref name="source"/> is the shooter, when there is one, so the HUD can
    /// show which direction the fire came from.
    /// </summary>
    public void Damage(float amount, Node3D? source = null)
    {
        if (_invuln > 0.0f || Health <= 0.0f || amount <= 0.0f)
        {
            return;
        }

        Health = Mathf.Max(0.0f, Health - amount);
        _sinceHit = 0.0f;
        EmitSignal(SignalName.HealthChanged, Health, MaxHealth);

        bool hasSource = source != null && IsInstanceValid(source);
        EmitSignal(
            SignalName.Damaged,
            amount,
            hasSource ? source!.GlobalPosition : Vector3.Zero,
            hasSource);

        if (Health <= 0.0f)
        {
            Die();
        }
    }

    private void Die()
    {
        EmitSignal(SignalName.Died);

        // Arcade respawn: back to the map spawn, full health, brief grace.
        Node? spawn = GetTree().Root.FindChild("Spawn", true, false);
        if (spawn is Node3D marker && _body != null)
        {
            _body.GlobalPosition = marker.GlobalPosition;
            _body.Velocity = Vector3.Zero;
        }

        Health = MaxHealth;
        _invuln = _spawnGrace;
        EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
    }
}
