using Godot;

namespace ChronoShift;

/// <summary>
/// A hostile the player can shoot. It hunts the player: closes the distance, then
/// fires on a timer for damage. It takes hits (flashing and showing a health bar),
/// and on death pays a bounty, topples over and removes itself.
/// </summary>
public partial class Enemy : CharacterBody3D, IDamageable
{
    [Export] private float _health = 100.0f;

    [Export] private AudioStream? _deathSound;
    [Export] private int _rewardMoney = 15;
    [Export] private int _rewardXp = 8;

    // --- Combat AI ---
    /// <summary>Move speed while closing on the player.</summary>
    [Export] private float _moveSpeed = 2.6f;

    /// <summary>
    /// The enemy notices and reacts to the player within this range. Kept well
    /// short of the map so hostiles are something the player walks into, not an
    /// ambient drain on anyone standing still doing engineering work.
    /// </summary>
    [Export] private float _detectRange = 17.0f;

    /// <summary>It stops closing and starts firing once within this range.</summary>
    [Export] private float _attackRange = 11.0f;

    /// <summary>Seconds between shots.</summary>
    [Export] private float _fireInterval = 2.4f;

    /// <summary>Damage dealt to the player per shot that connects.</summary>
    [Export] private float _attackDamage = 5.0f;

    /// <summary>Height the shot leaves from and aims at, so it clears the ground.</summary>
    [Export] private float _eyeHeight = 1.4f;

    [Export] private PackedScene? _muzzleFxScene;
    [Export] private PackedScene? _tracerScene;

    /// <summary>Played on every shot; without it enemy fire was silent and easy to miss.</summary>
    [Export] private AudioStream? _fireSound;

    /// <summary>
    /// Body tint once the hostile has noticed the player. Together with the health
    /// bar coming up it is how "it has seen you" reads on screen.
    /// </summary>
    [Export] private Color _alertTint = new(1.0f, 0.3f, 0.25f);

    /// <summary>Node rotated to lie down on death; usually the visual model root.</summary>
    [Export] private NodePath _visualPath = "Visual";

    /// <summary>The health bar's coloured fill, scaled and tinted as health drops.</summary>
    [Export] private NodePath _healthFillPath = "HealthBar/Fill";
    [Export] private NodePath _healthBarPath = "HealthBar";

    [Export] private PackedScene? _deathFxScene;

    /// <summary>Full-health and empty-health bar colours; the fill lerps between them.</summary>
    [Export] private Color _healthFullColor = new(0.3f, 0.8f, 0.35f);
    [Export] private Color _healthLowColor = new(0.85f, 0.25f, 0.2f);

    /// <summary>Half the bar's width, so the fill can be pinned to its left edge as it drains.</summary>
    [Export] private float _barHalfWidth = 0.4f;

    /// <summary>Body tint, so a hostile reads apart from the friendly hub NPC (same model).</summary>
    [Export] private Color _bodyTint = new(1.0f, 0.55f, 0.5f);

    private Node3D? _visual;
    private Node3D? _healthBar;
    private MeshInstance3D? _fill;
    private StandardMaterial3D? _fillMaterial;
    private StandardMaterial3D? _bodyMaterial;
    private float _fillY;
    private float _fillZ;
    private float _current;
    private float _flash;
    private float _gravity;
    private bool _dead;

    private Node3D? _player;
    private PlayerHealth? _playerHealth;
    private float _fireTimer;
    private bool _alerted;

    public override void _Ready()
    {
        AddToGroup("enemies");
        _current = _health;
        _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

        _visual = GetNodeOrNull<Node3D>(_visualPath);
        _healthBar = GetNodeOrNull<Node3D>(_healthBarPath);
        _fill = GetNodeOrNull<MeshInstance3D>(_healthFillPath);

        if (_fill != null)
        {
            _fillY = _fill.Position.Y;
            _fillZ = _fill.Position.Z;

            if (_fill.GetActiveMaterial(0) is StandardMaterial3D fillMat)
            {
                _fillMaterial = (StandardMaterial3D)fillMat.Duplicate();
                _fill.SetSurfaceOverrideMaterial(0, _fillMaterial);
                _fillMaterial.AlbedoColor = _healthFullColor;
            }
        }

        // Grab the body material so hits can flash it; the NPC visual sets it up first
        // (children ready before parent), so it exists by now.
        MeshInstance3D? bodyMesh = FindDescendant<MeshInstance3D>(_visual);
        if (bodyMesh?.GetActiveMaterial(0) is StandardMaterial3D bodyMat)
        {
            _bodyMaterial = (StandardMaterial3D)bodyMat.Duplicate();
            _bodyMaterial.AlbedoColor = _bodyTint;
            bodyMesh.SetSurfaceOverrideMaterial(0, _bodyMaterial);
        }

        // The bar starts hidden until the enemy is first hit, to keep the map calm.
        if (_healthBar != null)
        {
            _healthBar.Visible = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        UpdateFlash(dt);

        Vector3 velocity = Velocity;
        velocity.Y = IsOnFloor() ? 0.0f : velocity.Y - _gravity * dt;

        if (_dead)
        {
            velocity.X = 0.0f;
            velocity.Z = 0.0f;
            Velocity = velocity;
            MoveAndSlide();
            return;
        }

        Node3D? target = ResolveTarget();
        Vector3 planar = Vector3.Zero;

        if (target != null)
        {
            Vector3 toTarget = target.GlobalPosition - GlobalPosition;
            toTarget.Y = 0.0f;
            float distance = toTarget.Length();

            if (distance <= _detectRange && distance > 0.01f)
            {
                SetAlerted(true);

                Vector3 direction = toTarget / distance;
                FacePlayer(direction);

                if (distance > _attackRange)
                {
                    planar = direction * _moveSpeed;
                }
                else
                {
                    TryFire(dt, target);
                }
            }
        }

        velocity.X = planar.X;
        velocity.Z = planar.Z;
        Velocity = velocity;
        MoveAndSlide();
    }

    private void UpdateFlash(float dt)
    {
        if (_flash > 0.0f && _bodyMaterial != null)
        {
            _flash = Mathf.MoveToward(_flash, 0.0f, dt / 0.15f);
            _bodyMaterial.EmissionEnabled = true;
            _bodyMaterial.Emission = Colors.Red;
            _bodyMaterial.EmissionEnergyMultiplier = _flash * 3.0f;
        }
    }

    private void FacePlayer(Vector3 direction)
    {
        // Godot forward is -Z, so the yaw that points the body at `direction` is
        // atan2(-x, -z). Without the signs the enemy faced (and walked) backwards.
        float targetYaw = Mathf.Atan2(-direction.X, -direction.Z);
        Vector3 rotation = Rotation;
        rotation.Y = Mathf.LerpAngle(rotation.Y, targetYaw, 0.15f);
        Rotation = rotation;
    }

    private void TryFire(float dt, Node3D victim)
    {
        _fireTimer -= dt;
        if (_fireTimer > 0.0f)
        {
            return;
        }

        _fireTimer = _fireInterval;

        Vector3 from = GlobalPosition + Vector3.Up * _eyeHeight;
        Vector3 aim = victim.GlobalPosition + Vector3.Up * (_eyeHeight * 0.7f);

        // Only hit the target if nothing solid stands between us.
        var query = PhysicsRayQueryParameters3D.Create(from, aim);
        query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
        var hit = GetWorld3D().DirectSpaceState.IntersectRay(query);

        bool clear = hit.Count == 0;
        Vector3 end = aim;
        if (hit.Count > 0)
        {
            end = hit["position"].AsVector3();
            GodotObject collider = hit["collider"].As<GodotObject>();
            clear = collider is Node node && (node == victim || node.IsInGroup("player"));
        }

        Fx.SpawnMuzzle(_muzzleFxScene, this, from);
        Fx.SpawnTracer(_tracerScene, this, from, end);
        Sfx.PlayAt(this, _fireSound, from, -6.0f);

        if (!clear)
        {
            return;
        }

        // A driven vehicle soaks the round instead of the player inside it.
        if (victim is IDamageable armour)
        {
            armour.TakeDamage(_attackDamage, this);
            return;
        }

        (_playerHealth ??= GetTree().GetFirstNodeInGroup("player_health") as PlayerHealth)
            ?.Damage(_attackDamage, this);
    }

    /// <summary>
    /// What to shoot at: the vehicle the player is driving, when they are aboard one,
    /// otherwise the player on foot. Without this a hostile kept aiming at a player
    /// node that had been hidden and disabled inside the hull.
    /// </summary>
    private Node3D? ResolveTarget()
    {
        Node3D? player = ResolvePlayer();

        foreach (Node node in GetTree().GetNodesInGroup(Vehicle.VehicleGroup))
        {
            if (node is Vehicle { IsWrecked: false } vehicle
                && vehicle.Driver != null
                && (player == null || vehicle.Driver == player))
            {
                return vehicle;
            }
        }

        return player;
    }

    private Node3D? ResolvePlayer()
    {
        if (_player == null || !IsInstanceValid(_player))
        {
            _player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        }

        return _player;
    }

    /// <summary>
    /// Shows on the hostile itself that it has spotted the player: the health bar
    /// comes up and the body goes hostile-red. Previously nothing changed, so being
    /// noticed was invisible until the shots landed.
    /// </summary>
    private void SetAlerted(bool alerted)
    {
        if (_alerted == alerted)
        {
            return;
        }

        _alerted = alerted;

        if (_healthBar != null)
        {
            _healthBar.Visible = alerted;
        }

        if (_bodyMaterial != null)
        {
            _bodyMaterial.AlbedoColor = alerted ? _alertTint : _bodyTint;
        }
    }

    public void TakeDamage(float amount, Node3D source)
    {
        if (_dead)
        {
            return;
        }

        _current -= amount;
        _flash = 1.0f;

        if (_healthBar != null)
        {
            _healthBar.Visible = true;
        }

        float fraction = Mathf.Clamp(_current / _health, 0.0f, 1.0f);
        if (_fill != null)
        {
            // Scale from the centre but shift left so the bar drains from the right edge.
            _fill.Scale = new Vector3(fraction, 1.0f, 1.0f);
            _fill.Position = new Vector3(-_barHalfWidth * (1.0f - fraction), _fillY, _fillZ);
        }

        if (_fillMaterial != null)
        {
            _fillMaterial.AlbedoColor = _healthLowColor.Lerp(_healthFullColor, fraction);
        }

        if (_current <= 0.0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Sfx.PlayAt(this, _deathSound, GlobalPosition, -3.0f);

        _dead = true;

        EconomyManager.Instance?.AddMoney(_rewardMoney);
        ProgressionManager.Instance?.AddXp(_rewardXp);
        Fx.SpawnImpact(_deathFxScene, this, GlobalPosition + Vector3.Up, Vector3.Up);
        HudNotifier.Notify(this, "DUSHMAN YO'Q QILINDI", $"+₳{_rewardMoney} · +{_rewardXp} XP");

        if (_healthBar != null)
        {
            _healthBar.Visible = false;
        }

        // Stop taking hits or blocking the player, then topple and remove.
        CollisionLayer = 0;
        CollisionMask = 0;

        Tween tween = CreateTween();
        if (_visual != null)
        {
            tween.TweenProperty(_visual, "rotation:x", Mathf.Pi * 0.5f, 0.5f);
        }

        tween.TweenInterval(0.4f);
        tween.TweenCallback(Callable.From(QueueFree));
    }

    private static T? FindDescendant<T>(Node? root) where T : class
    {
        if (root == null)
        {
            return null;
        }

        foreach (Node child in root.GetChildren())
        {
            if (child is T match)
            {
                return match;
            }

            T? deeper = FindDescendant<T>(child);
            if (deeper != null)
            {
                return deeper;
            }
        }

        return null;
    }
}
