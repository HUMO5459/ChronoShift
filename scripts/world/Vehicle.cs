using Godot;

namespace ChronoShift;

/// <summary>
/// Repairable, drivable arcade vehicle. Repair on first interact, drive on the next;
/// exit returns control to the driver. It also takes fire: hits wear the hull down
/// and darken it, and at zero the vehicle throws the driver clear and becomes a wreck.
/// </summary>
public partial class Vehicle : CharacterBody3D, IInteractable, IDamageable
{
    /// <summary>Every vehicle joins this, so hostiles can find the one being driven.</summary>
    public const string VehicleGroup = "vehicles";

    /// <summary>
    /// The roster entry this vehicle was spawned from. The map loader assigns it
    /// before the node enters the tree; its characteristics then override the
    /// scene's, so the depot card and the driven vehicle can never disagree.
    /// </summary>
    [Export] private VehicleData? _data;

    [Export] private float _moveSpeed = 8.0f;
    [Export] private float _turnSpeed = 2.0f;              // radians/sec
    [Export] private NodePath _meshPath = "Body";                          // real vehicle model; keep its own materials
    [Export] private NodePath _lampPath = "StatusLamp";                    // roof lamp shows repair/focus state
    [Export] private NodePath _cameraRigPath = "CameraRig";
    [Export] private NodePath _vehicleCameraPath = "CameraRig/SpringArm3D/Camera3D";

    /// <summary>Optional rotating turret; leave empty on vehicles that have none.</summary>
    [Export] private NodePath _turretPath = new();

    /// <summary>Turret traverse, radians/sec — slow enough to feel like a tank.</summary>
    [Export] private float _turretTurnSpeed = 0.9f;

    /// <summary>Nodes rolled around X as the vehicle moves; leave empty on tracked vehicles.</summary>
    [Export] private Godot.Collections.Array<NodePath> _rollingWheels = new();

    /// <summary>Nodes yawed by the steering input — the front pair.</summary>
    [Export] private Godot.Collections.Array<NodePath> _steeringWheels = new();

    /// <summary>Metres; sets how fast the wheels spin for a given speed.</summary>
    [Export] private float _wheelRadius = 0.445f;

    /// <summary>Tints the livery meshes over their texture; white leaves them unchanged.</summary>
    [Export] private Color _liveryTint = Colors.White;

    /// <summary>Meshes the livery tint is multiplied onto — the hull and turret.</summary>
    [Export] private Godot.Collections.Array<NodePath> _liveryMeshes = new();

    [Export] private float _maxSteerDegrees = 28.0f;

    /// <summary>How quickly the front wheels swing to the steering input.</summary>
    [Export] private float _steerResponse = 8.0f;
    [Export] private NodePath _playerCollisionPath = "CollisionShape3D";   // relative to the driver (Player)
    [Export] private NodePath _playerCameraPath = "CameraRig/SpringArm3D/Camera3D"; // relative to the driver
    [Export] private Vector3 _exitOffset = new Vector3(2.9f, 1.0f, 0f);
    [Export] private Color _damagedColor = new Color(0.5f, 0.25f, 0.2f);
    [Export] private Color _repairedColor = new Color(0.3f, 0.55f, 0.7f);
    [Export] private float _focusEmissionEnergy = 0.4f;

    // --- Battle damage --------------------------------------------------------

    /// <summary>Hull points. A tank shell is 120, so an armoured hull survives a few.</summary>
    [Export] private float _maxHealth = 400.0f;

    /// <summary>How dark the hull goes at the point of destruction.</summary>
    [Export] private float _wreckDarkening = 0.7f;

    [Export] private PackedScene? _destroyFxScene;
    [Export] private AudioStream? _destroySound;
    [Export] private AudioStream? _hitSound;

    /// <summary>Paid out to whoever destroys it; the player only earns this on enemy vehicles.</summary>
    [Export] private int _wreckRewardMoney;

    [Signal] public delegate void DestroyedEventHandler();

    private MeshInstance3D _lamp = null!;
    private StandardMaterial3D _material = null!;
    private Camera3D _vehicleCamera = null!;
    private Node3D _cameraRig = null!;
    private Node3D? _turret;
    private Node3D _driver = null!;

    private readonly System.Collections.Generic.List<Node3D> _rollNodes = new();
    private readonly System.Collections.Generic.List<Node3D> _steerNodes = new();

    /// <summary>Livery meshes with their own duplicated material, so battle damage can darken them.</summary>
    private readonly System.Collections.Generic.List<(StandardMaterial3D Material, Color Base)> _hullMaterials = new();

    private float _wheelSpin;
    private float _steerAngle;
    private bool _repaired;
    private bool _active;
    private bool _focused;
    private float _gravity;
    private float _health;

    /// <summary>Fraction of incoming damage absorbed, 0..0.85; set from the roster entry.</summary>
    private float _armor;

    public string InteractionPrompt => IsWrecked ? "Yaroqsiz" : _repaired ? "Drive" : "Repair";

    /// <summary>True while a driver is aboard — mounted weapons only fire then.</summary>
    public bool IsDriven => _active;

    /// <summary>Whoever is aboard, or null when parked — hostiles shoot the hull instead of the driver.</summary>
    public Node3D? Driver => _active ? _driver : null;

    /// <summary>Burnt out: it can no longer be repaired, entered or driven.</summary>
    public bool IsWrecked { get; private set; }

    /// <summary>0..1 hull integrity, for the HUD readout while driving.</summary>
    public float HealthFraction => _maxHealth > 0.0f ? Mathf.Clamp(_health / _maxHealth, 0.0f, 1.0f) : 0.0f;

    public float Health => _health;
    public float MaxHealth => _maxHealth;

    /// <summary>Roster entry this was spawned from, or null for a hand-placed vehicle.</summary>
    public VehicleData? Data => _data;

    /// <summary>Top speed in m/s and absorbed damage share — read by the stats screen.</summary>
    public float MoveSpeed => _moveSpeed;

    public float Armor => _armor;

    public override void _Ready()
    {
        AddToGroup(VehicleGroup);
        ApplyData();
        _health = _maxHealth;

        _lamp = GetNode<MeshInstance3D>(_lampPath);
        _cameraRig = GetNode<Node3D>(_cameraRigPath);
        _vehicleCamera = GetNode<Camera3D>(_vehicleCameraPath);
        _vehicleCamera.GetParent<SpringArm3D>().AddExcludedObject(GetRid());

        // Turret is optional — the Humvee has none.
        _turret = _turretPath.IsEmpty ? null : GetNodeOrNull<Node3D>(_turretPath);

        foreach (NodePath path in _rollingWheels)
        {
            Node3D? wheel = GetNodeOrNull<Node3D>(path);
            if (wheel != null)
            {
                _rollNodes.Add(wheel);
            }
        }

        foreach (NodePath path in _steeringWheels)
        {
            Node3D? wheel = GetNodeOrNull<Node3D>(path);
            if (wheel != null)
            {
                _steerNodes.Add(wheel);
            }
        }

        // State/emission is shown on the lamp only, so the real vehicle model keeps its own materials.
        _material = new StandardMaterial3D
        {
            AlbedoColor = _damagedColor,
            EmissionEnabled = true,
            Emission = _damagedColor,
            EmissionEnergyMultiplier = 0f,
        };
        _lamp.MaterialOverride = _material;

        _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

        ApplyLivery();
    }

    /// <summary>
    /// Copies the roster entry's characteristics over the scene's. A zero in the
    /// resource means "not specified" and leaves the scene value standing, so a
    /// partly filled entry cannot quietly turn a tank into a slow, paper-thin one.
    /// </summary>
    private void ApplyData()
    {
        if (_data == null)
        {
            return;
        }

        if (_data.MaxHealth > 0.0f)
        {
            _maxHealth = _data.MaxHealth;
        }

        if (_data.MoveSpeed > 0.0f)
        {
            _moveSpeed = _data.MoveSpeed;
        }

        if (_data.TurnSpeed > 0.0f)
        {
            _turnSpeed = _data.TurnSpeed;
        }

        _armor = Mathf.Clamp(_data.Armor, 0.0f, 0.85f);

        if (_data.LiveryTint != Colors.White)
        {
            _liveryTint = _data.LiveryTint;
        }
    }

    /// <summary>
    /// Multiplies the tint onto each livery mesh's material, keeping its texture.
    /// The material is duplicated first so one tinted tank never recolours another
    /// — and the duplicates are kept, because battle damage darkens the same ones.
    /// </summary>
    private void ApplyLivery()
    {
        foreach (NodePath path in _liveryMeshes)
        {
            var mesh = GetNodeOrNull<MeshInstance3D>(path);
            if (mesh?.GetActiveMaterial(0) is not StandardMaterial3D source)
            {
                continue;
            }

            var material = (StandardMaterial3D)source.Duplicate();
            if (_liveryTint != Colors.White)
            {
                material.AlbedoColor = _liveryTint;
            }

            mesh.SetSurfaceOverrideMaterial(0, material);
            _hullMaterials.Add((material, material.AlbedoColor));
        }
    }

    /// <summary>Takes a hit from any weapon: wears the hull down, darkens it, and can wreck it.</summary>
    public void TakeDamage(float amount, Node3D source)
    {
        if (IsWrecked || amount <= 0.0f)
        {
            return;
        }

        // Armour is a flat share soaked off every hit, so a tank shrugs off rifle
        // fire while a shell still gets through.
        _health = Mathf.Max(0.0f, _health - amount * (1.0f - _armor));
        Sfx.PlayAt(this, _hitSound, GlobalPosition, -6.0f);

        // Visible wear, so a hull under fire reads as damaged before it blows.
        float wear = 1.0f - HealthFraction;
        foreach ((StandardMaterial3D material, Color baseColor) in _hullMaterials)
        {
            material.AlbedoColor = baseColor.Darkened(_wreckDarkening * wear);
        }

        if (_health <= 0.0f)
        {
            Destroy();
        }
    }

    /// <summary>
    /// Hull loss. The driver is thrown clear first — leaving them inside would strip
    /// their control and camera with no way to get them back.
    /// </summary>
    private void Destroy()
    {
        IsWrecked = true;
        _repaired = false;

        if (_active)
        {
            Exit();
        }

        Fx.SpawnImpact(_destroyFxScene, this, GlobalPosition + Vector3.Up * 1.5f, Vector3.Up);
        Sfx.PlayAt(this, _destroySound, GlobalPosition, -2.0f);

        _material.AlbedoColor = _damagedColor.Darkened(0.6f);
        _material.Emission = _damagedColor.Darkened(0.6f);
        _material.EmissionEnergyMultiplier = 0.0f;

        if (_wreckRewardMoney > 0)
        {
            EconomyManager.Instance?.AddMoney(_wreckRewardMoney);
        }

        HudNotifier.Notify(this, "TEXNIKA YO'Q QILINDI", Name.ToString().ToUpperInvariant());
        EmitSignal(SignalName.Destroyed);
    }

    public bool CanInteract(Node3D interactor)
    {
        return !_active && !IsWrecked;
    }

    public void Interact(Node3D interactor)
    {
        if (_active || IsWrecked)
        {
            return;
        }

        if (!_repaired)
        {
            Repair();
            return;
        }

        Enter(interactor);
    }

    public void SetFocused(bool focused)
    {
        _focused = focused;
        _material.EmissionEnergyMultiplier = (_focused && !_active && !IsWrecked) ? _focusEmissionEnergy : 0f;
    }

    private void Repair()
    {
        _repaired = true;
        _material.AlbedoColor = _repairedColor;
        _material.Emission = _repairedColor;
    }

    private void Enter(Node3D driver)
    {
        _driver = driver;
        _active = true;

        // The rig keeps whatever yaw the mouse left it at, so start every drive looking ahead.
        _cameraRig.Rotation = Vector3.Zero;
        _driver.Visible = false;
        _driver.ProcessMode = Node.ProcessModeEnum.Disabled;
        _driver.GetNode<CollisionShape3D>(_playerCollisionPath).Disabled = true;
        _vehicleCamera.Current = true;
        SetFocused(false);
    }

    private void Exit()
    {
        if (_driver != null)
        {
            _driver.GlobalPosition = GlobalPosition + GlobalTransform.Basis * _exitOffset;
            _driver.ProcessMode = Node.ProcessModeEnum.Inherit;
            _driver.Visible = true;
            _driver.GetNode<CollisionShape3D>(_playerCollisionPath).Disabled = false;
            _driver.GetNode<Camera3D>(_playerCameraPath).Current = true;
        }

        _active = false;
        _driver = null!;
    }

    /// <summary>
    /// Swings the turret toward where the driver is looking. The camera rig's own
    /// yaw is already relative to the hull, so it doubles as the turret's target
    /// angle — steering the hull drags the turret with it, as on a real tank.
    /// </summary>
    private void TraverseTurret(float dt)
    {
        if (_turret == null)
        {
            return;
        }

        float current = _turret.Rotation.Y;
        float difference = Mathf.AngleDifference(current, _cameraRig.Rotation.Y);
        float step = _turretTurnSpeed * dt;

        Vector3 rotation = _turret.Rotation;
        rotation.Y = current + Mathf.Clamp(difference, -step, step);
        _turret.Rotation = rotation;
    }

    /// <summary>Spins the wheels at whatever speed the vehicle is actually travelling.</summary>
    private void RollWheels(float dt)
    {
        if (_rollNodes.Count == 0 || _wheelRadius <= 0.0f)
        {
            return;
        }

        // Signed speed along the vehicle's nose, so reversing rolls them backwards.
        float forwardSpeed = Velocity.Dot(-GlobalTransform.Basis.Z);
        _wheelSpin += forwardSpeed / _wheelRadius * dt;

        foreach (Node3D wheel in _rollNodes)
        {
            wheel.Rotation = new Vector3(_wheelSpin, 0, 0);
        }
    }

    /// <summary>Turns the front wheels toward the steering input, easing rather than snapping.</summary>
    private void SteerWheels(float input, float dt)
    {
        if (_steerNodes.Count == 0)
        {
            return;
        }

        float target = input * Mathf.DegToRad(_maxSteerDegrees);
        _steerAngle = Mathf.Lerp(_steerAngle, target, Mathf.Clamp(_steerResponse * dt, 0.0f, 1.0f));

        foreach (Node3D wheel in _steerNodes)
        {
            Vector3 rotation = wheel.Rotation;
            rotation.Y = _steerAngle;
            wheel.Rotation = rotation;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        Vector3 v = Velocity;

        if (_active)
        {
            if (Input.IsActionJustPressed("exit_vehicle"))
            {
                Exit();
                return;
            }

            float steer = Input.GetActionStrength("move_left") - Input.GetActionStrength("move_right");
            RotateY(steer * _turnSpeed * dt);
            SteerWheels(steer, dt);

            float throttle = Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_back");
            Vector3 forward = -GlobalTransform.Basis.Z;
            Vector3 horiz = forward * throttle * _moveSpeed;
            v.X = horiz.X;
            v.Z = horiz.Z;

            TraverseTurret(dt);
        }
        else
        {
            v.X = 0;
            v.Z = 0;
            SteerWheels(0.0f, dt); // parked: let the wheels straighten out
        }

        RollWheels(dt);

        if (!IsOnFloor())
        {
            v.Y -= _gravity * dt;
        }
        else if (v.Y < 0)
        {
            v.Y = 0;
        }

        Velocity = v;
        MoveAndSlide();
    }
}
