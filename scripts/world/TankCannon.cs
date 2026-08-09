using Godot;

namespace ChronoShift;

/// <summary>
/// The tank's main gun. Fires along the barrel — not along the camera — so the
/// turret has to finish traversing before the shot lands where you are looking.
/// Only the driver can fire it.
/// </summary>
public partial class TankCannon : Node3D
{
    [Signal] public delegate void ReloadStateChangedEventHandler(bool ready);

    /// <summary>The HUD finds the gun through this rather than being wired to the tank.</summary>
    public const string CannonGroup = "vehicle_cannon";

    [Export] private NodePath _vehiclePath = "..";

    /// <summary>Empty marker at the barrel tip; the shot starts and flashes here.</summary>
    [Export] private NodePath _muzzlePath = "Muzzle";

    [Export] private float _damage = 120.0f;
    [Export] private float _range = 400.0f;
    [Export] private float _reloadSeconds = 3.0f;

    /// <summary>Radius around the hit that also takes damage — a shell is not a bullet.</summary>
    [Export] private float _splashRadius = 3.5f;

    [Export] private PackedScene? _muzzleFxScene;
    [Export] private PackedScene? _impactFxScene;
    [Export] private PackedScene? _tracerScene;

    private Vehicle _vehicle = null!;
    private Node3D _muzzle = null!;
    private float _reloadRemaining;

    /// <summary>True when the gun is loaded; the HUD shows this.</summary>
    public bool IsLoaded => _reloadRemaining <= 0.0f;

    /// <summary>0..1 reload progress, for the HUD bar.</summary>
    public float ReloadProgress => _reloadSeconds <= 0.0f
        ? 1.0f
        : 1.0f - Mathf.Clamp(_reloadRemaining / _reloadSeconds, 0.0f, 1.0f);

    /// <summary>True while someone is driving the tank this gun belongs to.</summary>
    public bool IsDriverAboard => _vehicle.IsDriven;

    public override void _Ready()
    {
        AddToGroup(CannonGroup);
        _vehicle = GetNode<Vehicle>(_vehiclePath);
        _muzzle = GetNode<Node3D>(_muzzlePath);

        // The roster entry, when there is one, is the single source for the gun's
        // numbers — otherwise the stats screen could quote figures the gun ignores.
        VehicleData? data = _vehicle.Data;
        if (data is { IsArmed: true })
        {
            _damage = data.ArmamentDamage;

            if (data.ArmamentRange > 0.0f)
            {
                _range = data.ArmamentRange;
            }

            if (data.ArmamentReloadSeconds > 0.0f)
            {
                _reloadSeconds = data.ArmamentReloadSeconds;
            }
        }
    }

    /// <summary>Live figures for the stats screen, so it never has to guess.</summary>
    public float Damage => _damage;

    public float Range => _range;

    public float ReloadSeconds => _reloadSeconds;

    public override void _Process(double delta)
    {
        if (_reloadRemaining > 0.0f)
        {
            _reloadRemaining -= (float)delta;
            if (_reloadRemaining <= 0.0f)
            {
                EmitSignal(SignalName.ReloadStateChanged, true);
            }
        }

        if (_vehicle.IsDriven && IsLoaded && Input.IsActionJustPressed("fire"))
        {
            Fire();
        }
    }

    private void Fire()
    {
        _reloadRemaining = _reloadSeconds;
        EmitSignal(SignalName.ReloadStateChanged, false);

        Vector3 from = _muzzle.GlobalPosition;
        Vector3 direction = -_muzzle.GlobalTransform.Basis.Z;
        Vector3 to = from + direction * _range;

        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.Exclude = new Godot.Collections.Array<Rid> { _vehicle.GetRid() };

        var hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
        Vector3 end = to;

        if (hit.Count > 0 && hit.ContainsKey("collider"))
        {
            end = hit["position"].AsVector3();
            Fx.SpawnImpact(_impactFxScene, this, end, hit["normal"].AsVector3());
            DamageAround(end, hit["collider"].As<GodotObject>());
        }

        Fx.SpawnMuzzle(_muzzleFxScene, this, from);
        Fx.SpawnTracer(_tracerScene, this, from, end);
    }

    /// <summary>Damages the object hit plus anything within the blast radius.</summary>
    private void DamageAround(Vector3 centre, GodotObject direct)
    {
        if (direct is IDamageable hit)
        {
            hit.TakeDamage(_damage, this);
        }

        if (_splashRadius <= 0.0f)
        {
            return;
        }

        var shape = new SphereShape3D { Radius = _splashRadius };
        var query = new PhysicsShapeQueryParameters3D
        {
            Shape = shape,
            Transform = new Transform3D(Basis.Identity, centre),
            CollideWithBodies = true,
            Exclude = new Godot.Collections.Array<Rid> { _vehicle.GetRid() },
        };

        foreach (Godot.Collections.Dictionary result in GetWorld3D().DirectSpaceState.IntersectShape(query, 32))
        {
            if (!result.ContainsKey("collider"))
            {
                continue;
            }

            GodotObject collider = result["collider"].As<GodotObject>();
            if (collider is IDamageable splashed && !ReferenceEquals(collider, direct))
            {
                // Falls off with distance so the blast edge only chips things.
                float distance = collider is Node3D node ? node.GlobalPosition.DistanceTo(centre) : _splashRadius;
                float falloff = 1.0f - Mathf.Clamp(distance / _splashRadius, 0.0f, 1.0f);
                splashed.TakeDamage(_damage * 0.5f * falloff, this);
            }
        }
    }
}
