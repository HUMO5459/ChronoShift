using Godot;

namespace ChronoShift;

/// <summary>The movement state the animator plays a clip for.</summary>
public enum LocomotionState { Idle, Walk, Run, Jump }

/// <summary>Camera-relative third-person locomotion for the player character.</summary>
public partial class PlayerController : CharacterBody3D
{
    [Export] private float _walkSpeed = 4.0f;
    [Export] private float _runSpeed = 7.0f;
    [Export] private float _jumpVelocity = 4.8f;
    [Export] private float _acceleration = 10.0f;
    [Export] private float _visualRotationSpeed = 10.0f;
    [Export] private NodePath _cameraRigPath = "CameraRig";
    [Export] private NodePath _visualPath = "Visual";

    private Node3D _cameraRig = null!;
    private PlayerCamera? _camera;
    private Node3D _visual = null!;
    private float _gravity;

    /// <summary>What the character is doing this frame; the animator reads it.</summary>
    public LocomotionState Locomotion { get; private set; } = LocomotionState.Idle;

    /// <summary>Ground speed threshold below which the character reads as idle.</summary>
    [Export] private float _idleThreshold = 0.3f;

    // --- Stamina -------------------------------------------------------------
    [Export] private float _maxStamina = 100.0f;

    /// <summary>Stamina spent per second while actually running.</summary>
    [Export] private float _staminaDrain = 17.0f;

    /// <summary>Stamina recovered per second once the player stops sprinting.</summary>
    [Export] private float _staminaRegen = 13.0f;

    /// <summary>Seconds after a sprint before recovery starts.</summary>
    [Export] private float _staminaRegenDelay = 0.7f;

    /// <summary>Stamina the player must recover to before sprinting is allowed again.</summary>
    [Export] private float _staminaResume = 18.0f;

    private float _stamina;
    private float _regenCooldown;
    private bool _winded;
    private bool _sprinting;

    public float Stamina => _stamina;
    public float MaxStamina => _maxStamina;

    /// <summary>Stamina left, 0..1 — the HUD bar reads this.</summary>
    public float StaminaFraction => _maxStamina <= 0f ? 1f : Mathf.Clamp(_stamina / _maxStamina, 0f, 1f);

    /// <summary>True while the player is out of breath and cannot sprint.</summary>
    public bool IsWinded => _winded;

    public override void _Ready()
    {
        _stamina = _maxStamina;
        _cameraRig = GetNode<Node3D>(_cameraRigPath);
        _camera = _cameraRig as PlayerCamera;
        _visual = GetNode<Node3D>(_visualPath);
        _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        Vector3 velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity.Y -= _gravity * dt;
        }

        if (IsOnFloor() && Input.IsActionJustPressed("jump"))
        {
            velocity.Y = _jumpVelocity;
        }

        Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");

        Basis rigBasis = _cameraRig.GlobalBasis;
        Vector3 forward = rigBasis.Z;
        forward.Y = 0.0f;
        Vector3 right = rigBasis.X;
        right.Y = 0.0f;

        Vector3 direction = (right * input.X + forward * input.Y);
        if (direction.LengthSquared() > 0.0f)
        {
            direction = direction.Normalized();
        }

        bool sprinting = UpdateStamina(dt, direction.LengthSquared() > 0.0f);

        float speed = sprinting ? _runSpeed : _walkSpeed;
        if (TechTreeManager.Instance != null)
        {
            speed *= TechTreeManager.Instance.PlayerSpeedMultiplier;
        }

        Vector3 targetHorizontal = direction * speed;

        float maxDelta = _acceleration * speed * dt;
        velocity.X = Mathf.MoveToward(velocity.X, targetHorizontal.X, maxDelta);
        velocity.Z = Mathf.MoveToward(velocity.Z, targetHorizontal.Z, maxDelta);

        Velocity = velocity;
        MoveAndSlide();

        Vector3 horizontal = new Vector3(Velocity.X, 0.0f, Velocity.Z);
        Locomotion = ResolveLocomotion(horizontal.Length());

        // While moving, the body faces the movement direction so A/D turn it (and W
        // aligns with the camera anyway). While standing still, it faces where the
        // camera looks, so the mouse still aims the body. The two agree when walking
        // forward, so there is no snap crossing between them.
        //
        // In first person the body must always follow the camera: the arms and the
        // weapon are on screen, so a body turned away from the view reads as the gun
        // pointing off to one side while the crosshair looks straight ahead.
        bool firstPerson = _camera?.IsFirstPerson == true;
        float targetYaw = !firstPerson && horizontal.Length() > _idleThreshold
            ? Mathf.Atan2(-horizontal.X, -horizontal.Z)
            : _cameraRig.Rotation.Y;
        Vector3 rotation = _visual.Rotation;
        rotation.Y = Mathf.LerpAngle(rotation.Y, targetYaw, _visualRotationSpeed * dt);
        _visual.Rotation = rotation;
    }

    /// <summary>
    /// Burns stamina while sprinting and recovers it otherwise. Once spent, the
    /// player is winded and walks until they have caught enough breath to matter,
    /// so the bar cannot be flickered on and off at zero.
    /// </summary>
    private bool UpdateStamina(float dt, bool moving)
    {
        bool wantsToRun = Input.IsActionPressed("run");
        _sprinting = wantsToRun && moving && !_winded && _stamina > 0f;

        if (_sprinting)
        {
            _stamina = Mathf.Max(0f, _stamina - _staminaDrain * dt);
            _regenCooldown = _staminaRegenDelay;

            if (_stamina <= 0f)
            {
                _winded = true;
                _sprinting = false;
            }

            return _sprinting;
        }

        if (_regenCooldown > 0f)
        {
            _regenCooldown -= dt;
            return false;
        }

        _stamina = Mathf.Min(_maxStamina, _stamina + _staminaRegen * dt);
        if (_winded && _stamina >= _staminaResume)
        {
            _winded = false;
        }

        return false;
    }

    private LocomotionState ResolveLocomotion(float groundSpeed)
    {
        if (!IsOnFloor())
        {
            return LocomotionState.Jump;
        }

        if (groundSpeed < _idleThreshold)
        {
            return LocomotionState.Idle;
        }

        return _sprinting ? LocomotionState.Run : LocomotionState.Walk;
    }
}
