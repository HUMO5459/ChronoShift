using Godot;

namespace ChronoShift;

/// <summary>
/// Hitscan weapons on the player. Holds a small loadout, switches between them with
/// the number keys, fires from screen centre, and damages IDamageable hits. All the
/// per-weapon behaviour (rate, automatic, magazine, reload, spread, scope) comes from
/// the <see cref="WeaponData"/> resources, so new guns are new data, not new code.
/// </summary>
public partial class WeaponController : Node3D
{
    [Export] private NodePath _cameraPath = "../CameraRig/SpringArm3D/Camera3D";

    /// <summary>The guns the player carries, in slot order (keys 1..N).</summary>
    [Export] private WeaponData[] _loadout = System.Array.Empty<WeaponData>();

    /// <summary>Skeleton bone the weapon model is parented to (right hand, verified index 51).</summary>
    [Export] private string _gripBoneName = "rp_nathan_animated_003_walking_hand_r";

    [Export] private PackedScene? _muzzleFxScene;
    [Export] private PackedScene? _impactFxScene;
    [Export] private PackedScene? _tracerScene;

    /// <summary>Distance from the hand to the barrel end, where the flash and tracer start.</summary>
    [Export] private float _muzzleReach = 0.45f;

    /// Picked at random per hit so repeated fire does not sound identical.
    [Export] private AudioStream[] _impactSounds = System.Array.Empty<AudioStream>();

    [Export] private AudioStream[] _shellSounds = System.Array.Empty<AudioStream>();

    // --- Aim pose ------------------------------------------------------------
    // The character rig has no aiming clip, so the shooting arm is posed in code:
    // the upper arm swings up and the forearm folds in while aiming or firing,
    // and a shot kicks it back briefly.
    [Export] private string _upperArmBone = "rp_nathan_animated_003_walking_upperarm_r";
    [Export] private string _foreArmBone = "rp_nathan_animated_003_walking_lowerarm_r";

    /// <summary>Upper-arm swing (degrees) at full aim, around the shoulder's pitch axis.</summary>
    [Export] private Vector3 _upperArmAimDegrees = new(0.0f, -52.0f, -8.0f);

    /// <summary>Forearm fold (degrees) at full aim.</summary>
    [Export] private Vector3 _foreArmAimDegrees = new(0.0f, -58.0f, 0.0f);

    /// Extra arm rotation applied only in first person. The third-person aim pose
    /// holds the gun out to the shooter's right, which from eye height sits well
    /// outside the frustum - measured at 0.55 m right, 0.07 m forward.
    [Export] private Vector3 _upperArmFirstPersonDegrees = new(0.0f, -30.0f, -20.0f);

    [Export] private Vector3 _foreArmFirstPersonDegrees = Vector3.Zero;

    /// Where the gun hand sits in camera space in first person. Rotating the
    /// shoulder cannot reach here - measured across two sweeps - so the arm is
    /// placed with the same two-bone IK the support hand already uses.
    [Export] private Vector3 _firstPersonHandOffset = new(0.20f, -0.16f, -0.52f);

    [Export] private Vector3 _firstPersonElbowPole = new(0.35f, -0.55f, 0.10f);

    /// Collapsed while in first person. The eye sits inside the skull, so without
    /// this the character's own mouth and nose fill the top of the frame.
    [Export] private string _headBone = "rp_nathan_animated_003_walking_head";

    /// <summary>How fast the arm rises and settles.</summary>
    [Export] private float _aimPoseSpeed = 9.0f;

    /// <summary>Extra upward kick (degrees) on the frame a shot goes off.</summary>
    [Export] private float _recoilDegrees = 12.0f;

    /// <summary>How long a shot keeps the arm up before it can drop again.</summary>
    [Export] private float _fireHoldSeconds = 0.9f;

    // --- Support hand --------------------------------------------------------
    // The left arm has no clip either, so while aiming it is bent onto the
    // weapon's foregrip with two-bone IK instead of hanging by the character's side.
    [Export] private string _leftUpperArmBone = "rp_nathan_animated_003_walking_upperarm_l";
    [Export] private string _leftForeArmBone = "rp_nathan_animated_003_walking_lowerarm_l";
    [Export] private string _leftHandBone = "rp_nathan_animated_003_walking_hand_l";

    /// <summary>How far along the weapon, from its rear, the support hand grabs.</summary>
    [Export] private float _supportGripAlongLength = 0.52f;

    /// <summary>
    /// The IK chain ends at the wrist, but the palm is what should sit on the
    /// handguard — so the target is pulled back down the forearm by a hand's length.
    /// </summary>
    [Export] private float _supportWristPullback = 0.085f;

    /// <summary>Where the elbow is pushed; down and out from the body.</summary>
    [Export] private Vector3 _supportElbowPole = new(0.35f, -0.9f, 0.15f);

    /// <summary>
    /// Fine rotation of the support hand once it is in place (degrees). The IK
    /// only moves the arm; without this the palm stays wherever the walk clip
    /// left it and the hand reads as open rather than gripping.
    /// </summary>
    [Export] private Vector3 _supportHandTweakDegrees = new(0.0f, 0.0f, -25.0f);

    private Camera3D _camera = null!;
    private PlayerCamera? _cameraRig;
    private CharacterBody3D _playerBody = null!;
    private Skeleton3D? _skeleton;
    private BoneAttachment3D? _attachment;

    /// <summary>The hand-held model, used as the muzzle origin for flash and tracer.</summary>
    private Node3D? _weaponModel;
    private Node3D? _playerVisual;
    private int _headIdx = -1;
    private int _gripHandIdx = -1;
    private bool _headHidden;

    private int _index = -1;
    private int[] _ammo = System.Array.Empty<int>();
    private float _cooldown;
    private float _reloadTimer = -1.0f;
    private float _defaultFov = 75.0f;

    private int _upperArmIdx = -1;
    private int _foreArmIdx = -1;
    private Quaternion _upperArmRest = Quaternion.Identity;
    private Quaternion _foreArmRest = Quaternion.Identity;
    private float _aimBlend;
    private float _recoil;
    private float _fireHold;

    private int _leftUpperIdx = -1;
    private int _leftForeIdx = -1;
    private int _leftHandIdx = -1;

    /// <summary>Foregrip point in the weapon model's own space, measured when it is attached.</summary>
    private Vector3 _supportPointLocal;
    private bool _hasSupportPoint;

    /// <summary>The equipped weapon, or null when the player is unarmed.</summary>
    public WeaponData? Current => _index >= 0 && _index < _loadout.Length ? _loadout[_index] : null;

    /// <summary>Everything the player is carrying, in slot order — the HUD's quick slots.</summary>
    public System.Collections.Generic.IReadOnlyList<WeaponData> Loadout => _loadout;

    /// <summary>Index of the equipped slot, or -1 when unarmed.</summary>
    public int SelectedIndex => _index;

    /// <summary>Rounds left in slot <paramref name="slot"/>, for the quick-slot readout.</summary>
    public int AmmoIn(int slot) => slot >= 0 && slot < _ammo.Length ? _ammo[slot] : 0;

    /// <summary>True while looking through a scoped weapon's optic.</summary>
    public bool IsScoped { get; private set; }

    /// <summary>Rounds left in the current magazine (for the HUD).</summary>
    public int Ammo => _index >= 0 && _index < _ammo.Length ? _ammo[_index] : 0;

    public int MagazineSize => Current?.MagazineSize ?? 0;
    public bool IsReloading => _reloadTimer >= 0.0f;

    /// <summary>0..1 reload progress while reloading; 0 otherwise (for the HUD bar).</summary>
    public float ReloadProgress =>
        IsReloading && Current != null && Current.ReloadSeconds > 0.0f
            ? Mathf.Clamp(_reloadTimer / Current.ReloadSeconds, 0.0f, 1.0f)
            : 0.0f;

    public override void _Ready()
    {
        AddToGroup("player_weapon");

        _camera = GetNode<Camera3D>(_cameraPath);
        _cameraRig = _camera.GetParent()?.GetParent() as PlayerCamera;
        _defaultFov = _camera.Fov;
        _playerBody = GetParent<CharacterBody3D>();
        _playerVisual = _playerBody.GetNodeOrNull<Node3D>("Visual");
        _skeleton = FindDescendant<Skeleton3D>(_playerBody);

        _ammo = new int[_loadout.Length];
        for (int i = 0; i < _loadout.Length; i++)
        {
            _ammo[i] = _loadout[i]?.MagazineSize ?? 0;
        }

        if (_skeleton != null)
        {
            _headIdx = _skeleton.FindBone(_headBone);
            _gripHandIdx = _skeleton.FindBone(_gripBoneName);
            _upperArmIdx = _skeleton.FindBone(_upperArmBone);
            _foreArmIdx = _skeleton.FindBone(_foreArmBone);

            if (_upperArmIdx >= 0)
            {
                _upperArmRest = _skeleton.GetBoneRest(_upperArmIdx).Basis.GetRotationQuaternion();
            }

            if (_foreArmIdx >= 0)
            {
                _foreArmRest = _skeleton.GetBoneRest(_foreArmIdx).Basis.GetRotationQuaternion();
            }

            _leftUpperIdx = _skeleton.FindBone(_leftUpperArmBone);
            _leftForeIdx = _skeleton.FindBone(_leftForeArmBone);
            _leftHandIdx = _skeleton.FindBone(_leftHandBone);
        }

        if (_loadout.Length > 0)
        {
            EquipWeapon(0);
        }
    }

    /// <summary>
    /// Adds a weapon found in the world to the loadout and equips it. Returns false
    /// when the player already carries it, so a rack cannot be farmed.
    /// </summary>
    public bool AddWeapon(WeaponData? weapon)
    {
        if (weapon == null)
        {
            return false;
        }

        for (int i = 0; i < _loadout.Length; i++)
        {
            if (_loadout[i] == weapon || _loadout[i]?.WeaponId == weapon.WeaponId)
            {
                EquipWeapon(i);
                return false;
            }
        }

        System.Array.Resize(ref _loadout, _loadout.Length + 1);
        System.Array.Resize(ref _ammo, _ammo.Length + 1);
        _loadout[^1] = weapon;
        _ammo[^1] = weapon.MagazineSize;

        EquipWeapon(_loadout.Length - 1);
        return true;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        HandleSwitching();

        if (_cooldown > 0.0f)
        {
            _cooldown -= dt;
        }

        UpdateReload(dt);
        UpdateAim();
        UpdateAimPose(dt);

        WeaponData? weapon = Current;
        if (weapon == null)
        {
            return;
        }

        if (Input.IsActionJustPressed("reload"))
        {
            BeginReload();
        }

        bool wantsFire = weapon.Automatic
            ? Input.IsActionPressed("fire")
            : Input.IsActionJustPressed("fire");

        if (wantsFire && _cooldown <= 0.0f && !IsReloading)
        {
            if (_ammo[_index] > 0)
            {
                Fire(weapon);
                _ammo[_index]--;
                _cooldown = weapon.FireRate > 0.0f ? 1.0f / weapon.FireRate : 0.2f;
            }
            else
            {
                BeginReload();
            }
        }
    }

    private void HandleSwitching()
    {
        for (int slot = 0; slot < _loadout.Length && slot < 9; slot++)
        {
            if (Input.IsActionJustPressed($"weapon_{slot + 1}"))
            {
                EquipWeapon(slot);
            }
        }
    }

    private void EquipWeapon(int index)
    {
        if (index == _index || index < 0 || index >= _loadout.Length)
        {
            return;
        }

        _index = index;
        _cooldown = 0.0f;
        _reloadTimer = -1.0f;
        _camera.Fov = _defaultFov;

        AttachModel(_loadout[index]);
    }

    private void AttachModel(WeaponData? weapon)
    {
        if (_weaponModel != null)
        {
            _weaponModel.QueueFree();
            _weaponModel = null;
        }

        if (weapon?.ModelScene == null || _skeleton == null)
        {
            if (weapon?.ModelScene != null && _skeleton == null)
            {
                GD.PushWarning("WeaponController: no Skeleton3D found under player; weapon model not attached.");
            }

            return;
        }

        if (_attachment == null)
        {
            _attachment = new BoneAttachment3D { BoneName = _gripBoneName };
            _skeleton.AddChild(_attachment);
        }

        var model = weapon.ModelScene.Instantiate<Node3D>();
        _attachment.AddChild(model);
        _weaponModel = model;

        var basis = Basis.FromEuler(new Vector3(
                Mathf.DegToRad(weapon.GripRotationDegrees.X),
                Mathf.DegToRad(weapon.GripRotationDegrees.Y),
                Mathf.DegToRad(weapon.GripRotationDegrees.Z)))
            .Scaled(new Vector3(weapon.GripScale, weapon.GripScale, weapon.GripScale));
        model.Transform = new Transform3D(basis, weapon.GripPosition);

        MeasureSupportPoint(model);
    }

    /// <summary>
    /// Finds the foregrip in the model's own space: along its longest axis, a bit
    /// forward of the trigger hand. Measured from the meshes only — the FBX files
    /// also carry lights and cameras, whose bounds would swamp the weapon's.
    /// </summary>
    private void MeasureSupportPoint(Node3D model)
    {
        _hasSupportPoint = false;

        Aabb box = default;
        bool any = false;
        MergeMeshBounds(model, model, ref box, ref any);
        if (!any)
        {
            return;
        }

        Vector3 size = box.Size;
        int longIdx = size.X >= size.Y && size.X >= size.Z ? 0 : size.Y >= size.Z ? 1 : 2;

        Vector3 point = box.GetCenter();
        point[longIdx] = box.Position[longIdx] + size[longIdx] * _supportGripAlongLength;
        _supportPointLocal = point;
        _hasSupportPoint = true;
    }

    private static void MergeMeshBounds(Node node, Node3D root, ref Aabb box, ref bool any)
    {
        if (node is MeshInstance3D mesh)
        {
            Aabb local = root.GlobalTransform.AffineInverse() * (mesh.GlobalTransform * mesh.GetAabb());
            box = any ? box.Merge(local) : local;
            any = true;
        }

        foreach (Node child in node.GetChildren())
        {
            MergeMeshBounds(child, root, ref box, ref any);
        }
    }

    private void BeginReload()
    {
        WeaponData? weapon = Current;
        if (weapon == null || IsReloading || _ammo[_index] >= weapon.MagazineSize)
        {
            return;
        }

        _reloadTimer = 0.0f;
    }

    private void UpdateReload(float dt)
    {
        WeaponData? weapon = Current;
        if (!IsReloading || weapon == null)
        {
            return;
        }

        _reloadTimer += dt;
        if (_reloadTimer >= weapon.ReloadSeconds)
        {
            _ammo[_index] = weapon.MagazineSize;
            _reloadTimer = -1.0f;
        }
    }

    /// <summary>
    /// A scope shows the lens and nothing else: the camera drops to eye level and
    /// the shooter's own body leaves the view, or the arm sits in the middle of
    /// the circle at scope FOV.
    /// </summary>
    private void SetScope(bool on)
    {
        IsScoped = on;

        if (_weaponModel != null)
        {
            _weaponModel.Visible = !on;
        }
        if (_cameraRig != null)
        {
            _cameraRig.ScopeView = on;
        }
        if (_playerVisual != null)
        {
            _playerVisual.Visible = !on;
        }
    }

    /// <summary>
    /// Shrinks the head bone away rather than hiding a mesh: the body is one
    /// skinned mesh, so the head cannot be hidden on its own any other way.
    /// </summary>
    private void SetHeadHidden(bool hidden)
    {
        if (_skeleton == null || _headIdx < 0 || hidden == _headHidden)
        {
            return;
        }

        _headHidden = hidden;
        _skeleton.SetBonePoseScale(_headIdx, hidden ? new Vector3(0.001f, 0.001f, 0.001f) : Vector3.One);
    }

    /// <summary>
    /// Puts the gun hand where a first-person view expects it. The third-person
    /// aim pose holds the gun out at the shooter's side, which from eye height is
    /// either invisible or filling half the screen; shoulder angles alone cannot
    /// bring it forward, so the hand is driven to a camera-relative point.
    /// </summary>
    private void PlaceFirstPersonHand(bool firstPerson)
    {
        if (_skeleton == null || !firstPerson || IsScoped
            || _upperArmIdx < 0 || _foreArmIdx < 0 || _gripHandIdx < 0)
        {
            return;
        }

        Vector3 target = _camera.GlobalTransform * _firstPersonHandOffset;
        Vector3 pole = target + _camera.GlobalTransform.Basis * _firstPersonElbowPole;
        ArmIk.Solve(_skeleton, _upperArmIdx, _foreArmIdx, _gripHandIdx, target, pole, _aimBlend);
    }

    private void ClearScope()
    {
        if (IsScoped)
        {
            SetScope(false);
        }
    }

    private void UpdateAim()
    {
        WeaponData? weapon = Current;
        if (weapon == null)
        {
            // Losing the weapon while scoped must not strand the scope state:
            // the body stays hidden and the optic stays on screen forever.
            ClearScope();
            _camera.Fov = Mathf.MoveToward(_camera.Fov, _defaultFov, 240.0f * (float)GetProcessDeltaTime());
            return;
        }

        bool aiming = Input.IsActionPressed("aim");
        float target = weapon.AimFov > 0.0f && aiming ? weapon.AimFov : _defaultFov;
        _camera.Fov = Mathf.MoveToward(_camera.Fov, target, 240.0f * (float)GetProcessDeltaTime());

        // Scoped only once the optic has actually zoomed in, so the overlay does
        // not snap on before the view has caught up.
        bool wantsScope = weapon.UsesScope && aiming && _camera.Fov < (_defaultFov + weapon.AimFov) * 0.5f;
        if (wantsScope != IsScoped)
        {
            SetScope(wantsScope);
        }
    }

    /// <summary>
    /// Shoots in two stages, the way a third-person shooter has to. The camera
    /// decides *what* the crosshair is on; the barrel decides where the bullet
    /// actually travels from. Firing straight down the camera ray made the flash
    /// and tracer start at the character in the middle of the screen and let shots
    /// pass through cover the player was standing behind.
    /// </summary>
    /// <summary>
    /// Poses the shooting arm by hand. The rig only ships a walk clip, so without
    /// this the character fires with the gun hanging at the hip. Bone poses are
    /// written every frame after the animation has run (this node sits below the
    /// visual in the player scene), so the animation does not fight the pose.
    /// </summary>
    private void UpdateAimPose(float dt)
    {
        if (_skeleton == null || _upperArmIdx < 0)
        {
            return;
        }

        _fireHold = Mathf.Max(0.0f, _fireHold - dt);
        _recoil = Mathf.Max(0.0f, _recoil - dt / 0.12f);

        // In first person the weapon is on screen the whole time, so the arm stays
        // up: a gun hanging at the hip is simply out of frame from eye height.
        bool wantsPose = Current != null
                         && (_cameraRig?.IsFirstPerson == true
                             || Input.IsActionPressed("aim")
                             || Input.IsActionPressed("fire")
                             || _fireHold > 0.0f);

        // The rest pose of this rig is a T-pose, so blending from the rest would
        // fling the arm out sideways. The animated pose at the moment aiming starts
        // is the honest starting point; capture it once and blend away from that.
        if (wantsPose && _aimBlend <= 0.001f)
        {
            _upperArmRest = _skeleton.GetBonePoseRotation(_upperArmIdx);
            if (_foreArmIdx >= 0)
            {
                _foreArmRest = _skeleton.GetBonePoseRotation(_foreArmIdx);
            }
        }

        _aimBlend = Mathf.MoveToward(_aimBlend, wantsPose ? 1.0f : 0.0f, _aimPoseSpeed * dt);

        bool firstPerson = _cameraRig?.IsFirstPerson == true;
        SetHeadHidden(firstPerson && !IsScoped);

        if (_aimBlend <= 0.001f)
        {
            return;
        }

        Vector3 upper = _upperArmAimDegrees + (firstPerson ? _upperArmFirstPersonDegrees : Vector3.Zero);
        upper.Y -= _recoilDegrees * _recoil;

        _skeleton.SetBonePoseRotation(_upperArmIdx, _upperArmRest * EulerQuat(upper * _aimBlend));

        if (_foreArmIdx >= 0)
        {
            Vector3 fore = _foreArmAimDegrees + (firstPerson ? _foreArmFirstPersonDegrees : Vector3.Zero);
            _skeleton.SetBonePoseRotation(_foreArmIdx, _foreArmRest * EulerQuat(fore * _aimBlend));
        }

        PlaceFirstPersonHand(firstPerson);

        SolveSupportHand();
    }

    /// <summary>Bends the left arm onto the weapon's foregrip once the gun is up.</summary>
    private void SolveSupportHand()
    {
        if (!_hasSupportPoint || _weaponModel == null || _skeleton == null)
        {
            return;
        }

        Vector3 grip = _weaponModel.GlobalTransform * _supportPointLocal;

        // Aim the wrist a hand's length short of the grip, along the line from the
        // shoulder, so the palm rather than the wrist lands on the weapon.
        Vector3 shoulder = _skeleton.GlobalTransform * _skeleton.GetBoneGlobalPose(_leftUpperIdx).Origin;
        Vector3 approach = grip - shoulder;
        Vector3 target = approach.LengthSquared() > 0.0001f
            ? grip - approach.Normalized() * _supportWristPullback
            : grip;

        // Push the elbow down and away from the chest so the arm does not fold
        // through the body on its way to the grip.
        Vector3 pole = target + _playerBody.GlobalTransform.Basis * _supportElbowPole;

        ArmIk.Solve(_skeleton, _leftUpperIdx, _leftForeIdx, _leftHandIdx, target, pole, _aimBlend);

        // The measured rig has the left fingers running along the hand's local +X
        // and the thumb toward -Z, so the palm is the -Z face: point the fingers
        // across the weapon and turn the palm up into it.
        Basis body = _playerBody.GlobalTransform.Basis;
        Vector3 fingers = body.X.Normalized();
        Vector3 z = -Vector3.Up;
        Vector3 y = z.Cross(fingers).Normalized();
        var wanted = new Basis(fingers, y, z);
        wanted *= Basis.FromEuler(new Vector3(
            Mathf.DegToRad(_supportHandTweakDegrees.X),
            Mathf.DegToRad(_supportHandTweakDegrees.Y),
            Mathf.DegToRad(_supportHandTweakDegrees.Z)));

        ArmIk.OrientBone(_skeleton, _leftHandIdx, wanted, _aimBlend);
    }

    private static Quaternion EulerQuat(Vector3 degrees) => Quaternion.FromEuler(new Vector3(
        Mathf.DegToRad(degrees.X),
        Mathf.DegToRad(degrees.Y),
        Mathf.DegToRad(degrees.Z)));

    private void Fire(WeaponData weapon)
    {
        Vector3 eye = _camera.GlobalPosition;
        Vector3 forward = -_camera.GlobalTransform.Basis.Z;
        Vector3 muzzle = MuzzlePoint(forward);

        Vector3 aimPoint = ResolveAimPoint(eye, forward, weapon.Range);

        int pellets = Mathf.Max(1, weapon.PelletsPerShot);
        for (int i = 0; i < pellets; i++)
        {
            Vector3 toAim = (aimPoint - muzzle).Normalized();
            Vector3 dir = ApplySpread(toAim, weapon.SpreadDegrees);
            Vector3 to = muzzle + dir * weapon.Range;

            var query = PhysicsRayQueryParameters3D.Create(muzzle, to);
            query.Exclude = new Godot.Collections.Array<Rid> { _playerBody.GetRid() };

            var hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
            Vector3 end = to;

            if (hit.Count > 0 && hit.ContainsKey("collider"))
            {
                end = hit["position"].AsVector3();
                GodotObject collider = hit["collider"].As<GodotObject>();
                if (collider is IDamageable d)
                {
                    d.TakeDamage(weapon.Damage, this);
                }

                Fx.SpawnImpact(_impactFxScene, this, end, hit["normal"].AsVector3());
                Sfx.PlayAt(this, Sfx.Pick(_impactSounds), end, -4.0f);
            }

            Fx.SpawnTracer(_tracerScene, this, muzzle, end);
        }

        Fx.SpawnMuzzle(_muzzleFxScene, this, muzzle);
        Sfx.PlayAt(this, weapon.FireSound, muzzle, -2.0f);
        Sfx.PlayAt(this, Sfx.Pick(_shellSounds), muzzle, -12.0f);
        _recoil = 1.0f;
        _fireHold = _fireHoldSeconds;
    }

    /// <summary>What the crosshair is pointing at, or a far point when it is pointing at sky.</summary>
    private Vector3 ResolveAimPoint(Vector3 eye, Vector3 forward, float range)
    {
        Vector3 far = eye + forward * range;
        var query = PhysicsRayQueryParameters3D.Create(eye, far);
        query.Exclude = new Godot.Collections.Array<Rid> { _playerBody.GetRid() };

        var hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
        return hit.Count > 0 && hit.ContainsKey("position") ? hit["position"].AsVector3() : far;
    }

    /// <summary>
    /// The barrel end. The models have no muzzle marker, so it is taken as a short
    /// step forward from the hand — enough that the flash reads as coming out of
    /// the gun rather than out of the character's chest.
    /// </summary>
    private Vector3 MuzzlePoint(Vector3 forward)
    {
        Vector3 hand = _weaponModel != null ? _weaponModel.GlobalPosition : _camera.GlobalPosition;
        return hand + forward * _muzzleReach;
    }

    private static Vector3 ApplySpread(Vector3 forward, float spreadDegrees)
    {
        if (spreadDegrees <= 0.0f)
        {
            return forward;
        }

        float maxRad = Mathf.DegToRad(spreadDegrees);
        // Random point in a small cone around the aim direction.
        Vector3 basisX = forward.Cross(Vector3.Up);
        if (basisX.LengthSquared() < 0.0001f)
        {
            basisX = forward.Cross(Vector3.Right);
        }

        basisX = basisX.Normalized();
        Vector3 basisY = forward.Cross(basisX).Normalized();

        float angle = GD.Randf() * Mathf.Tau;
        float radius = Mathf.Sqrt(GD.Randf()) * maxRad;
        Vector3 offset = (basisX * Mathf.Cos(angle) + basisY * Mathf.Sin(angle)) * Mathf.Tan(radius);
        return (forward + offset).Normalized();
    }

    private static T? FindDescendant<T>(Node root) where T : class
    {
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
