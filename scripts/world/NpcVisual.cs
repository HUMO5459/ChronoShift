using Godot;

namespace ChronoShift;

/// <summary>Plays a stationary NPC's idle clip forever and applies its albedo/normal material.</summary>
public partial class NpcVisual : Node3D
{
    [Export] private string _animation = "Take 001";
    [Export] private Texture2D? _albedo;
    [Export] private Texture2D? _normal;

    // The idle clip may translate the root bone; routing it through the root motion
    // track makes Godot keep the pose in place so the NPC never drifts.
    [Export] private string _rootMotionTrack =
        "rp_sophia_animated_003_idling/rp_sophia_animated_003_idling_CTRL/Skeleton3D:rp_sophia_animated_003_idling_root";

    /// <summary>
    /// Opt-in for NPCs that walk: the clip runs only while the body is moving and
    /// freezes on a standing frame otherwise. Off by default, so the stationary
    /// hub NPC and the enemies keep looping exactly as before.
    /// </summary>
    [Export] private bool _driveFromMotion;

    /// <summary>Time in a walk-only clip whose pose reads as "standing".</summary>
    [Export] private float _idleFrameSeconds = 0.1f;

    /// <summary>Ground speed above which the walk clip runs.</summary>
    [Export] private float _moveThreshold = 0.25f;

    private AnimationPlayer? _anim;
    private CharacterBody3D? _body;
    private bool _walking;

    public override void _Ready()
    {
        _anim = FindDescendant<AnimationPlayer>(this);

        if (_anim != null && _anim.HasAnimation(_animation))
        {
            _anim.GetAnimation(_animation).LoopMode = Animation.LoopModeEnum.Linear;
        }

        if (_anim != null && !string.IsNullOrEmpty(_rootMotionTrack))
        {
            _anim.RootMotionTrack = _rootMotionTrack;
        }

        if (_anim != null && _anim.HasAnimation(_animation))
        {
            _anim.Play(_animation);
        }

        if (_driveFromMotion)
        {
            _body = FindAncestor<CharacterBody3D>(this);
            FreezeStanding();
        }

        MeshInstance3D? mesh = FindDescendant<MeshInstance3D>(this);
        if (mesh != null && _albedo != null)
        {
            var material = new StandardMaterial3D { AlbedoTexture = _albedo };
            if (_normal != null)
            {
                material.NormalEnabled = true;
                material.NormalTexture = _normal;
            }
            mesh.SetSurfaceOverrideMaterial(0, material);
        }
    }

    public override void _Process(double delta)
    {
        if (!_driveFromMotion || _anim == null || _body == null)
        {
            return;
        }

        var horizontal = new Vector3(_body.Velocity.X, 0.0f, _body.Velocity.Z);
        bool moving = horizontal.Length() > _moveThreshold;
        if (moving == _walking)
        {
            return;
        }

        _walking = moving;
        if (moving)
        {
            _anim.Play(_animation);
        }
        else
        {
            FreezeStanding();
        }
    }

    /// <summary>Holds the walk clip on a still frame, so standing is never the bind pose.</summary>
    private void FreezeStanding()
    {
        if (_anim == null || !_anim.HasAnimation(_animation))
        {
            return;
        }

        _anim.Play(_animation);
        _anim.Seek(_idleFrameSeconds, update: true);
        _anim.Pause();
        _walking = false;
    }

    private static T? FindAncestor<T>(Node node) where T : class
    {
        for (Node? current = node.GetParent(); current != null; current = current.GetParent())
        {
            if (current is T match)
            {
                return match;
            }
        }

        return null;
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
