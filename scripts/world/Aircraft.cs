using Godot;

namespace ChronoShift;

/// <summary>
/// Arcade flight for planes and helicopters. Enter like a vehicle, then fly with
/// the movement keys: no stall model, no realistic aerodynamics — the aircraft
/// simply goes where its nose points. Reuses the existing Input Map, so W/S, A/D,
/// jump (climb) and run (descend) all drive it without new bindings.
/// </summary>
public partial class Aircraft : CharacterBody3D, IInteractable
{
    public enum FlightMode { Plane, Helicopter }

    [Export] private FlightMode _mode = FlightMode.Plane;

    [Export] private NodePath _meshPath = "Body";
    [Export] private NodePath _cameraRigPath = "CameraRig";
    [Export] private NodePath _vehicleCameraPath = "CameraRig/SpringArm3D/Camera3D";
    [Export] private NodePath _playerCollisionPath = "CollisionShape3D";
    [Export] private NodePath _playerCameraPath = "CameraRig/SpringArm3D/Camera3D";

    [Export] private Vector3 _exitOffset = new(3.5f, 0.5f, 0);

    // --- Plane tuning --------------------------------------------------------
    [Export] private float _planeSpeed = 22.0f;
    [Export] private float _planeThrottleBoost = 1.6f;
    [Export] private float _pitchRate = 1.1f;      // radians/sec
    [Export] private float _rollRate = 1.8f;
    /// <summary>How hard a bank turns the nose — arcade coordinated turn.</summary>
    [Export] private float _bankTurn = 0.9f;
    /// <summary>Roll eases back toward level when the stick is centred.</summary>
    [Export] private float _rollRecovery = 1.5f;

    // --- Helicopter tuning ---------------------------------------------------
    [Export] private float _heliMoveSpeed = 12.0f;
    [Export] private float _heliClimbSpeed = 6.0f;
    [Export] private float _heliYawRate = 1.4f;
    /// <summary>Visual nose-down/roll lean in the direction of travel.</summary>
    [Export] private float _heliLean = 0.35f;

    private Node3D _cameraRig = null!;
    private Camera3D _camera = null!;
    private Node3D _driver = null!;
    private bool _active;
    private float _roll;

    public string InteractionPrompt => _mode == FlightMode.Plane ? "Uch" : "Havoga ko'taril";

    public bool IsFlown => _active;

    public override void _Ready()
    {
        _cameraRig = GetNode<Node3D>(_cameraRigPath);
        _camera = GetNode<Camera3D>(_vehicleCameraPath);
        _camera.GetParent<SpringArm3D>().AddExcludedObject(GetRid());
    }

    public bool CanInteract(Node3D interactor) => !_active;

    public void Interact(Node3D interactor)
    {
        if (!_active)
        {
            Enter(interactor);
        }
    }

    public void SetFocused(bool focused) { }

    private void Enter(Node3D driver)
    {
        _driver = driver;
        _active = true;
        _driver.Visible = false;
        _driver.ProcessMode = ProcessModeEnum.Disabled;
        _driver.GetNode<CollisionShape3D>(_playerCollisionPath).Disabled = true;
        _cameraRig.Rotation = Vector3.Zero;
        _camera.Current = true;
    }

    private void Exit()
    {
        if (_driver != null)
        {
            _driver.GlobalPosition = GlobalPosition + GlobalTransform.Basis * _exitOffset;
            _driver.ProcessMode = ProcessModeEnum.Inherit;
            _driver.Visible = true;
            _driver.GetNode<CollisionShape3D>(_playerCollisionPath).Disabled = false;
            _driver.GetNode<Camera3D>(_playerCameraPath).Current = true;
        }

        _active = false;
        _driver = null!;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_active)
        {
            return;
        }

        if (Input.IsActionJustPressed("exit_vehicle"))
        {
            Exit();
            return;
        }

        float dt = (float)delta;
        if (_mode == FlightMode.Plane)
        {
            FlyPlane(dt);
        }
        else
        {
            FlyHelicopter(dt);
        }

        MoveAndSlide();
    }

    private void FlyPlane(float dt)
    {
        float pitchInput = Input.GetActionStrength("move_back") - Input.GetActionStrength("move_forward");
        float rollInput = Input.GetActionStrength("move_left") - Input.GetActionStrength("move_right");

        // Pitch and yaw turn the whole aircraft; the nose is where it flies.
        RotateObjectLocal(Vector3.Right, pitchInput * _pitchRate * dt);

        // Bank is a visual roll on the mesh, tracked as a target angle that eases to level.
        float target = rollInput * Mathf.Pi * 0.5f;
        _roll = Mathf.Abs(rollInput) > 0.01f
            ? Mathf.MoveToward(_roll, target, _rollRate * dt)
            : Mathf.MoveToward(_roll, 0.0f, _rollRecovery * dt);
        ApplyMeshRoll(_roll);

        // The bank angle turns the nose into the roll — the arcade coordinated turn.
        RotateY(_roll * _bankTurn * dt);

        float throttle = Input.IsActionPressed("run") ? _planeThrottleBoost : 1.0f;
        Velocity = -GlobalTransform.Basis.Z * _planeSpeed * throttle;
    }

    /// <summary>Rolls only the mesh wrapper, leaving the body's heading untouched.</summary>
    private void ApplyMeshRoll(float roll)
    {
        var mesh = GetNodeOrNull<Node3D>(_meshPath);
        if (mesh != null)
        {
            Vector3 rotation = mesh.Rotation;
            rotation.Z = roll;
            mesh.Rotation = rotation;
        }
    }

    private void FlyHelicopter(float dt)
    {
        float forwardInput = Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_back");
        float yawInput = Input.GetActionStrength("move_left") - Input.GetActionStrength("move_right");
        float climbInput = Input.GetActionStrength("jump") - Input.GetActionStrength("run");

        RotateY(yawInput * _heliYawRate * dt);

        Vector3 forward = -GlobalTransform.Basis.Z;
        Vector3 horizontal = forward * forwardInput * _heliMoveSpeed;

        Velocity = new Vector3(horizontal.X, climbInput * _heliClimbSpeed, horizontal.Z);

        // Lean the body into its motion for readability, without steering it.
        var mesh = GetNodeOrNull<Node3D>(_meshPath);
        if (mesh != null)
        {
            Vector3 rotation = mesh.Rotation;
            rotation.X = Mathf.LerpAngle(rotation.X, -forwardInput * _heliLean, 6.0f * dt);
            rotation.Z = Mathf.LerpAngle(rotation.Z, yawInput * _heliLean, 6.0f * dt);
            mesh.Rotation = rotation;
        }
    }
}
