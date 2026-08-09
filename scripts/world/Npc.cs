using Godot;

namespace ChronoShift;

/// <summary>
/// A civilian or friendly soldier populating the camp and the mission maps. It
/// works at a post, patrols a small area, or stands guard scanning — and looks up
/// to say something when the player finishes a piece of engineering. Simple by
/// design: extras that make the world look staffed, not agents with goals.
/// </summary>
public partial class Npc : CharacterBody3D, IDamageable
{
    /// <summary>Every NPC registers here, so a map can count its inhabitants.</summary>
    public const string NpcGroup = "npcs";

    [Export] private NpcData? _data;

    /// <summary>
    /// Civilians are soft: they are not combatants, so a couple of rounds put one
    /// down. Before this they simply absorbed fire with no effect at all.
    /// </summary>
    [Export] private float _health = 60.0f;

    [Export] private PackedScene? _deathFxScene;
    [Export] private AudioStream? _deathSound;

    /// <summary>Body flash colour on a hit, so a shot that connects is legible.</summary>
    [Export] private Color _hitFlashColor = new(0.9f, 0.15f, 0.12f);

    /// <summary>Model root; tinted by role and turned to face things.</summary>
    [Export] private NodePath _visualPath = "Visual";

    /// <summary>Billboard plate above the head: the role, or a spoken line.</summary>
    [Export] private NodePath _platePath = "Plate";

    /// <summary>Seconds a reaction line stays up before the role plate returns.</summary>
    [Export] private float _speechSeconds = 4.0f;

    /// <summary>Only NPCs this close to the player answer a finished task.</summary>
    [Export] private float _reactRange = 18.0f;

    /// <summary>
    /// The character models are photogrammetry scans at roughly 50k triangles
    /// each, so a crowd is only drawn while the player is near enough to read it.
    /// </summary>
    [Export] private float _drawRange = 45.0f;

    /// <summary>How close counts as having arrived at a patrol point.</summary>
    [Export] private float _arriveDistance = 0.7f;

    private Node3D? _visual;
    private Label3D? _plate;
    private Node3D? _player;

    private Vector3 _home;
    private float _homeYaw;
    private Vector3 _target;
    private float _pauseRemaining;
    private float _speechRemaining;
    private float _scanTime;
    private float _gravity;

    private StandardMaterial3D? _bodyMaterial;
    private float _current;
    private float _flash;
    private bool _dead;

    private NpcBehavior Behavior => _data?.Behavior ?? NpcBehavior.Work;

    public override void _Ready()
    {
        AddToGroup(NpcGroup);
        _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
        _current = _health;

        _visual = GetNodeOrNull<Node3D>(_visualPath);
        _plate = GetNodeOrNull<Label3D>(_platePath);

        _home = GlobalPosition;
        _homeYaw = Rotation.Y;
        _target = _home;

        ApplyTint();
        DistanceCull.ApplyTo(_visual!, _drawRange, 8.0f);
        ShowRolePlate();
        PickPatrolTarget();

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.MissionCompleted += OnTaskCompleted;
        }
    }

    /// <summary>
    /// One model serves every role, so the tint is what tells them apart. The
    /// duplicate is kept even when there is no tint, because hits flash the same
    /// material and sharing it would flash every NPC on the map at once.
    /// </summary>
    private void ApplyTint()
    {
        // The NPC visual builds the material first (children are ready before the
        // parent), so duplicating it here keeps the tint per-instance.
        MeshInstance3D? mesh = FindDescendant<MeshInstance3D>(_visual);
        if (mesh?.GetActiveMaterial(0) is not StandardMaterial3D source)
        {
            return;
        }

        var tinted = (StandardMaterial3D)source.Duplicate();
        if (_data != null && _data.Tint != Colors.White)
        {
            tinted.AlbedoColor = _data.Tint;
        }

        mesh.SetSurfaceOverrideMaterial(0, tinted);
        _bodyMaterial = tinted;
    }

    /// <summary>Takes fire like anything else in the world; civilians just die faster.</summary>
    public void TakeDamage(float amount, Node3D source)
    {
        if (_dead || amount <= 0.0f)
        {
            return;
        }

        _current -= amount;
        _flash = 1.0f;

        // Being shot at breaks whatever the NPC was doing — they turn and stop.
        _speechRemaining = 0.0f;
        _pauseRemaining = 0.6f;

        if (_current <= 0.0f)
        {
            Die();
        }
    }

    private void Die()
    {
        _dead = true;

        Sfx.PlayAt(this, _deathSound, GlobalPosition, -4.0f);
        Fx.SpawnImpact(_deathFxScene, this, GlobalPosition + Vector3.Up, Vector3.Up);

        if (_plate != null)
        {
            _plate.Visible = false;
        }

        // Stop blocking the player and stop taking hits, then topple and clear out.
        CollisionLayer = 0;
        CollisionMask = 0;

        Tween tween = CreateTween();
        if (_visual != null)
        {
            tween.TweenProperty(_visual, "rotation:x", Mathf.Pi * 0.5f, 0.5f);
        }

        tween.TweenInterval(0.6f);
        tween.TweenCallback(Callable.From(QueueFree));
    }

    private void UpdateFlash(float dt)
    {
        if (_flash <= 0.0f || _bodyMaterial == null)
        {
            return;
        }

        _flash = Mathf.MoveToward(_flash, 0.0f, dt / 0.15f);
        _bodyMaterial.EmissionEnabled = true;
        _bodyMaterial.Emission = _hitFlashColor;
        _bodyMaterial.EmissionEnergyMultiplier = _flash * 3.0f;
    }

    private void ShowRolePlate()
    {
        if (_plate == null)
        {
            return;
        }

        _plate.Text = _data?.RoleLabel ?? "";
        _plate.Modulate = Colors.White.With(0.55f);
        _plate.Visible = !string.IsNullOrEmpty(_plate.Text);
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        UpdateFlash(dt);

        Vector3 velocity = Velocity;
        velocity.Y = IsOnFloor() ? 0.0f : velocity.Y - _gravity * dt;

        Vector3 planar = Vector3.Zero;

        if (_dead)
        {
            velocity.X = 0.0f;
            velocity.Z = 0.0f;
            Velocity = velocity;
            MoveAndSlide();
            return;
        }

        if (_speechRemaining > 0.0f)
        {
            // Mid-reaction: stop whatever you were doing and look at the player.
            _speechRemaining -= dt;
            FaceThePlayer(dt);
            if (_speechRemaining <= 0.0f)
            {
                ShowRolePlate();
            }
        }
        else
        {
            switch (Behavior)
            {
                case NpcBehavior.Patrol:
                    planar = Walk(dt);
                    break;
                case NpcBehavior.Guard:
                    Scan(dt);
                    break;
            }
        }

        velocity.X = planar.X;
        velocity.Z = planar.Z;
        Velocity = velocity;
        MoveAndSlide();
    }

    private Vector3 Walk(float dt)
    {
        if (_pauseRemaining > 0.0f)
        {
            _pauseRemaining -= dt;
            return Vector3.Zero;
        }

        Vector3 toTarget = _target - GlobalPosition;
        toTarget.Y = 0.0f;

        if (toTarget.Length() <= _arriveDistance)
        {
            _pauseRemaining = _data?.PauseSeconds ?? 2.0f;
            PickPatrolTarget();
            return Vector3.Zero;
        }

        Vector3 direction = toTarget.Normalized();
        FaceDirection(direction, dt, 6.0f);
        return direction * (_data?.MoveSpeed ?? 1.8f);
    }

    private void PickPatrolTarget()
    {
        if (Behavior != NpcBehavior.Patrol)
        {
            return;
        }

        float radius = _data?.PatrolRadius ?? 6.0f;
        float angle = GD.Randf() * Mathf.Tau;
        float distance = radius * (0.4f + 0.6f * GD.Randf());
        _target = _home + new Vector3(Mathf.Cos(angle) * distance, 0.0f, Mathf.Sin(angle) * distance);
    }

    private void Scan(float dt)
    {
        _scanTime += dt;
        float sweep = Mathf.DegToRad(_data?.ScanDegrees ?? 45.0f);
        Vector3 rotation = Rotation;
        rotation.Y = _homeYaw + Mathf.Sin(_scanTime * 0.6f) * sweep;
        Rotation = rotation;
    }

    private void FaceThePlayer(float dt)
    {
        if (_player == null || !IsInstanceValid(_player))
        {
            _player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        }

        if (_player == null)
        {
            return;
        }

        Vector3 toPlayer = _player.GlobalPosition - GlobalPosition;
        toPlayer.Y = 0.0f;
        if (toPlayer.LengthSquared() > 0.01f)
        {
            FaceDirection(toPlayer.Normalized(), dt, 5.0f);
        }
    }

    private void FaceDirection(Vector3 direction, float dt, float speed)
    {
        // Godot forward is -Z. Without the signs the NPC walked its patrol backwards,
        // the same way the hostiles did.
        float targetYaw = Mathf.Atan2(-direction.X, -direction.Z);
        Vector3 rotation = Rotation;
        rotation.Y = Mathf.LerpAngle(rotation.Y, targetYaw, Mathf.Min(1.0f, speed * dt));
        Rotation = rotation;
    }

    /// <summary>The camp notices when engineering work lands — TZ's "react to missions".</summary>
    private void OnTaskCompleted(string taskId, int rewardMoney, int rewardXp)
    {
        if (_plate == null || _data == null || string.IsNullOrEmpty(_data.MissionLine))
        {
            return;
        }

        // Only nearby NPCs pipe up; the whole map shouting at once reads as noise.
        Node3D? player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        if (player != null && player.GlobalPosition.DistanceTo(GlobalPosition) > _reactRange)
        {
            return;
        }

        _plate.Text = _data.MissionLine;
        _plate.Modulate = UiPalette.Highlight;
        _plate.Visible = true;  
        _speechRemaining = _speechSeconds;
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
