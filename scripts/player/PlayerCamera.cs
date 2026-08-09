using Godot;

namespace ChronoShift;

/// <summary>
/// Mouse-driven camera that yaws the rig and pitches the spring arm, in either
/// third or first person. The same script sits on the player, both ground
/// vehicles and all four aircraft, so one toggle key switches the view wherever
/// the player happens to be — on foot, in a cab, or in a cockpit.
///
/// First person is done by collapsing the spring arm and moving the camera to an
/// eye/seat point given per scene, rather than by hiding the character: the arms
/// and the weapon stay on screen, which is the point of the view.
/// </summary>
public partial class PlayerCamera : Node3D
{
    [Export] private float _mouseSensitivity = 0.003f;
    [Export] private float _pitchMinDegrees = -70.0f;
    [Export] private float _pitchMaxDegrees = 30.0f;
    [Export] private NodePath _springArmPath = "SpringArm3D";

    /// <summary>
    /// Where the eye sits in first person, relative to this rig. The default suits
    /// a standing character; vehicles override it with their driver's seat.
    /// </summary>
    [Export] private Vector3 _firstPersonOffset = new(0.14f, 0.18f, -0.28f);

    /// <summary>Pitch limits are wider in first person — you can look up at the sky.</summary>
    [Export] private float _firstPersonPitchMinDegrees = -80.0f;
    [Export] private float _firstPersonPitchMaxDegrees = 75.0f;

    /// <summary>Starts in first person; cockpits may prefer to.</summary>
    [Export] private bool _startInFirstPerson;

    /// <summary>
    /// A standing offset kept even while not aiming. A chase camera centred on the
    /// spine puts the character directly under the crosshair, which is where the
    /// player is looking — so the rig sits off the shoulder the whole time.
    /// </summary>
    [Export] private Vector3 _shoulderOffset = new(0.5f, 0.15f, 0.0f);

    /// <summary>
    /// Extra step taken while aiming, on top of <see cref="_shoulderOffset"/>: further
    /// out and higher, so the shooter's raised arm and weapon stay clear of the
    /// centre of the screen instead of covering the thing being aimed at.
    /// </summary>
    [Export] private Vector3 _aimOffset = new(0.5f, 0.3f, 0.0f);

    /// <summary>
    /// Arm length while aiming. It used to pull in to 1.8 m, which filled the frame
    /// with the character's own back; aiming now stays far enough out to see past him.
    /// </summary>
    [Export] private float _aimSpringLength = 2.8f;

    /// <summary>How fast the camera slides between chase and aim.</summary>
    [Export] private float _aimBlendSpeed = 8.0f;

    /// <summary>How fast the camera slides between third and first person.</summary>
    [Export] private float _viewBlendSpeed = 10.0f;

    private SpringArm3D _springArm = null!;
    private Camera3D? _camera;
    private Vector3 _restOffset;
    private float _restSpringLength;
    private float _aimBlend;
    private float _viewBlend;
    private bool _firstPerson;

    /// <summary>True once the camera has settled into first person.</summary>
    public bool IsFirstPerson => _firstPerson;

    /// <summary>
    /// Forces the eye-level view while a scope is up. Looking down an optic from
    /// over the shoulder puts the shooter's own body between the lens and the
    /// target, which at scope FOV fills a large part of the circle.
    /// </summary>
    public bool ScopeView { get; set; }

    public override void _Ready()
    {
        _springArm = GetNode<SpringArm3D>(_springArmPath);
        _camera = FindCamera(_springArm);
        _restOffset = _springArm.Position;
        _restSpringLength = _springArm.SpringLength;
        _firstPerson = _startInFirstPerson;
        _viewBlend = _firstPerson ? 1.0f : 0.0f;
        Input.MouseMode = Input.MouseModeEnum.Captured;

        if (GetParent() is CollisionObject3D parent)
        {
            _springArm.AddExcludedObject(parent.GetRid());
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        bool aiming = InputMap.HasAction("aim") && Input.IsActionPressed("aim");
        _aimBlend = Mathf.MoveToward(_aimBlend, aiming ? 1.0f : 0.0f, _aimBlendSpeed * dt);
        bool eyeLevel = _firstPerson || ScopeView;
        _viewBlend = Mathf.MoveToward(_viewBlend, eyeLevel ? 1.0f : 0.0f, _viewBlendSpeed * dt);

        // Third-person placement: the standing shoulder offset, plus the extra step
        // taken while aiming. First person collapses both away.
        Vector3 chaseOffset = _restOffset + _shoulderOffset;
        Vector3 thirdOffset = chaseOffset.Lerp(chaseOffset + _aimOffset, _aimBlend);
        float thirdLength = Mathf.Lerp(_restSpringLength, _aimSpringLength, _aimBlend);

        _springArm.Position = thirdOffset.Lerp(_firstPersonOffset, _viewBlend);
        _springArm.SpringLength = Mathf.Lerp(thirdLength, 0.0f, _viewBlend);

        ClampPitch();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion
            && Input.MouseMode == Input.MouseModeEnum.Captured
            && _camera?.Current != false)
        {
            RotateY(-motion.Relative.X * _mouseSensitivity);

            Vector3 rotation = _springArm.Rotation;
            rotation.X -= motion.Relative.Y * _mouseSensitivity;
            _springArm.Rotation = rotation;
            ClampPitch();
            return;
        }

        // Every rig — the player's and each vehicle's — runs this script, so only
        // the one actually rendering may answer the key. Without the check the
        // player's hidden camera swallowed the toggle while driving.
        if (InputMap.HasAction("camera_toggle") && @event.IsActionPressed("camera_toggle")
            && Input.MouseMode == Input.MouseModeEnum.Captured
            && _camera?.Current == true)
        {
            _firstPerson = !_firstPerson;
            GetViewport().SetInputAsHandled();
            return;
        }

        if (@event is InputEventMouseButton button && button.Pressed &&
            Input.MouseMode == Input.MouseModeEnum.Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            GetViewport().SetInputAsHandled();
        }
    }

    private static Camera3D? FindCamera(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is Camera3D camera)
            {
                return camera;
            }

            Camera3D? deeper = FindCamera(child);
            if (deeper != null)
            {
                return deeper;
            }
        }

        return null;
    }

    /// <summary>First person can look further up and down than a chase camera can.</summary>
    private void ClampPitch()
    {
        float min = Mathf.Lerp(_pitchMinDegrees, _firstPersonPitchMinDegrees, _viewBlend);
        float max = Mathf.Lerp(_pitchMaxDegrees, _firstPersonPitchMaxDegrees, _viewBlend);

        Vector3 rotation = _springArm.Rotation;
        rotation.X = Mathf.Clamp(rotation.X, Mathf.DegToRad(min), Mathf.DegToRad(max));
        _springArm.Rotation = rotation;
    }
}
