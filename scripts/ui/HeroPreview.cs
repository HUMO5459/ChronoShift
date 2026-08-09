using Godot;

namespace ChronoShift;

/// <summary>
/// Live 3D preview of a character for the hero-select menu. Lives on a pivot node
/// inside a SubViewport; instances the character's model and turns it slowly so the
/// player can see who they picked.
/// </summary>
public partial class HeroPreview : Node3D
{
    [Export] private float _turnSpeed = 0.6f; // radians/sec

    private Node3D? _model;

    public override void _Process(double delta)
    {
        RotateY(_turnSpeed * (float)delta);
    }

    /// <summary>Swaps the preview to <paramref name="character"/>'s model.</summary>
    public void Show(CharacterData? character)
    {
        _model?.QueueFree();
        _model = null;
        Rotation = Vector3.Zero;

        PackedScene? scene = character?.PreviewModel ?? character?.ModelScene;
        if (scene == null)
        {
            return;
        }

        var instance = scene.Instantiate<Node3D>();

        // Preview always faces the camera the same way, regardless of the in-world facing.
        var basis = Basis.FromEuler(new Vector3(
            Mathf.DegToRad(character!.ModelRotationDegrees.X),
            Mathf.DegToRad(character.ModelRotationDegrees.Y),
            Mathf.DegToRad(character.ModelRotationDegrees.Z))).Scaled(character.ModelScale);
        instance.Transform = new Transform3D(basis, character.ModelOffset);

        AddChild(instance);
        _model = instance;

        ApplyMaterial(instance, character);
        PoseStanding(instance, character);
    }

    /// <summary>Shows the character's standing pose, not the T-pose bind, in the preview.</summary>
    private static void PoseStanding(Node model, CharacterData data)
    {
        AnimationPlayer? anim = FindAnim(model);
        if (anim == null)
        {
            return;
        }

        // A real idle clip loops; a walk-only rig freezes at its standing frame.
        if (!string.IsNullOrEmpty(data.IdleClip) && anim.HasAnimation(data.IdleClip))
        {
            anim.GetAnimation(data.IdleClip).LoopMode = Animation.LoopModeEnum.Linear;
            anim.Play(data.IdleClip);
        }
        else if (!string.IsNullOrEmpty(data.WalkClip) && anim.HasAnimation(data.WalkClip))
        {
            anim.Play(data.WalkClip);
            anim.Seek(0.1, update: true);
            anim.Pause();
        }
    }

    private static AnimationPlayer? FindAnim(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is AnimationPlayer anim)
            {
                return anim;
            }

            AnimationPlayer? deeper = FindAnim(child);
            if (deeper != null)
            {
                return deeper;
            }
        }

        return null;
    }

    private static void ApplyMaterial(Node model, CharacterData data)
    {
        if (data.Albedo == null)
        {
            return;
        }

        MeshInstance3D? mesh = FindMesh(model);
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

    private static MeshInstance3D? FindMesh(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                return mesh;
            }

            MeshInstance3D? deeper = FindMesh(child);
            if (deeper != null)
            {
                return deeper;
            }
        }

        return null;
    }
}
