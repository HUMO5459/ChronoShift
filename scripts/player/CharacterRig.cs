using Godot;

namespace ChronoShift;

/// <summary>
/// Instances the selected character model and animates it from the player's
/// locomotion state. Any character in the roster works, driven by its own clip
/// names from CharacterData.
/// </summary>
public partial class CharacterRig : Node3D
{
    [Export] private CharacterRosterData? _roster;
    [Export] private NodePath _controllerPath = "../..";

    /// <summary>Crossfade between locomotion clips, seconds.</summary>
    [Export] private float _blendTime = 0.15f;

    /// <summary>
    /// For a walk-only rig (no idle clip), the time in the walk clip whose pose
    /// reads as "standing" — the rig freezes here when idle instead of playing the
    /// walk (moving legs) or falling back to the T-pose bind pose.
    /// </summary>
    [Export] private float _idleFrameSeconds = 0.1f;

    private PlayerController? _controller;
    private AnimationPlayer? _anim;
    private CharacterData? _data;
    private LocomotionState _current = LocomotionState.Idle;
    private bool _started;

    public override void _Ready()
    {
        _controller = GetNodeOrNull<PlayerController>(_controllerPath);
        _data = CharacterRoster.Resolve(_roster);

        if (_data?.ModelScene != null)
        {
            SpawnModel(_data);
        }
        else if (_data != null)
        {
            // No model scene: this character is the one already baked into the player.
            _anim = FindDescendant<AnimationPlayer>(this);
            ApplyMaterial(this, _data);
        }
        else
        {
            _anim = FindDescendant<AnimationPlayer>(this);
        }

        ConfigureClips();
    }

    private void SpawnModel(CharacterData data)
    {
        var model = data.ModelScene!.Instantiate<Node3D>();

        var basis = Basis.FromEuler(new Vector3(
            Mathf.DegToRad(data.ModelRotationDegrees.X),
            Mathf.DegToRad(data.ModelRotationDegrees.Y),
            Mathf.DegToRad(data.ModelRotationDegrees.Z))).Scaled(data.ModelScale);
        model.Transform = new Transform3D(basis, data.ModelOffset);
        AddChild(model);

        _anim = FindDescendant<AnimationPlayer>(model);
        ApplyMaterial(model, data);
    }

    private static void ApplyMaterial(Node model, CharacterData data)
    {
        if (data.Albedo == null)
        {
            return;
        }

        MeshInstance3D? mesh = FindDescendant<MeshInstance3D>(model);
        if (mesh == null)
        {
            return;
        }

        var material = new StandardMaterial3D { AlbedoTexture = data.Albedo };
        if (data.Normal != null)
        {
            material.NormalEnabled = true;
            material.NormalTexture = data.Normal;
        }

        mesh.SetSurfaceOverrideMaterial(0, material);
    }

    private void ConfigureClips()
    {
        if (_anim == null || _data == null)
        {
            return;
        }

        // Locomotion clips loop; the jump clip plays once.
        SetLoop(_data.IdleClip, true);
        SetLoop(_data.WalkClip, true);
        SetLoop(_data.RunClip, true);
        SetLoop(_data.JumpClip, false);

        // Keep a root-motion walk clip in place; the body handles real movement.
        if (!string.IsNullOrEmpty(_data.RootMotionTrack))
        {
            _anim.RootMotionTrack = _data.RootMotionTrack;
        }

        // Start on the idle pose so the bind-pose T never shows for a frame at spawn.
        PlayState(LocomotionState.Idle);
        _current = LocomotionState.Idle;
        _started = true;
    }

    private void SetLoop(string clip, bool loop)
    {
        if (!string.IsNullOrEmpty(clip) && _anim!.HasAnimation(clip))
        {
            _anim.GetAnimation(clip).LoopMode = loop ? Animation.LoopModeEnum.Linear : Animation.LoopModeEnum.None;
        }
    }

    public override void _Process(double delta)
    {
        if (_anim == null || _controller == null || _data == null)
        {
            return;
        }

        LocomotionState state = _controller.Locomotion;
        if (state != _current || !_started)
        {
            _current = state;
            _started = true;
            PlayState(state);
        }
    }

    private void PlayState(LocomotionState state)
    {
        if (_anim == null || _data == null)
        {
            return;
        }

        // Idle on a rig without an idle clip: hold a still standing pose rather than
        // playing the walk (moving legs) or falling back to the T-pose bind pose.
        if (state == LocomotionState.Idle && string.IsNullOrEmpty(_data.IdleClip))
        {
            FreezeStanding();
            return;
        }

        string clip = _data.ClipFor(state);
        if (string.IsNullOrEmpty(clip) || !_anim.HasAnimation(clip))
        {
            FreezeStanding();
            return;
        }

        if (_anim.CurrentAnimation != clip || !_anim.IsPlaying())
        {
            _anim.Play(clip, _blendTime);
        }
    }

    /// <summary>Freezes the walk clip at its standing frame so idle is still but never the bind pose.</summary>
    private void FreezeStanding()
    {
        string clip = _data!.WalkClip;
        if (string.IsNullOrEmpty(clip) || !_anim!.HasAnimation(clip))
        {
            return;
        }

        _anim.Play(clip);
        _anim.Seek(_idleFrameSeconds, update: true);
        _anim.Pause();
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
