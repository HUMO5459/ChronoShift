using Godot;

namespace ChronoShift;

/// <summary>
/// A playable character: the model to instance plus the animation clip names for
/// each locomotion state. Different models ship with different rigs and clip
/// names, so those live here as data rather than being hardcoded in the animator.
/// </summary>
[GlobalClass]
public partial class CharacterData : Resource
{
    [Export] public string CharacterId = "";
    [Export] public string DisplayName = "Geroy";

    /// <summary>
    /// The model scene instanced under the player's Visual node. Leave empty to
    /// animate a model already baked into the player scene (the default hero).
    /// </summary>
    [Export] public PackedScene? ModelScene;

    /// <summary>Model shown in the hero-select preview; falls back to ModelScene when empty.</summary>
    [Export] public PackedScene? PreviewModel;

    /// <summary>Local transform for the instanced model — models vary in scale, facing and origin.</summary>
    [Export] public Vector3 ModelScale = Vector3.One;
    [Export] public Vector3 ModelRotationDegrees = Vector3.Zero;
    [Export] public Vector3 ModelOffset = Vector3.Zero;

    // --- Animation clip names, as they appear in the model's AnimationPlayer -----
    [Export] public string IdleClip = "";
    [Export] public string WalkClip = "";
    [Export] public string RunClip = "";
    [Export] public string JumpClip = "";

    /// <summary>
    /// Track that carries the walk clip's forward translation, if any. Routing it
    /// through root motion keeps the model in place while the body does the moving.
    /// Empty when the clips are already authored in place (most rigs).
    /// </summary>
    [Export] public string RootMotionTrack = "";

    // --- Optional material override (Nathan ships textures separately) ------------
    [Export] public Texture2D? Albedo;
    [Export] public Texture2D? Normal;

    /// <summary>Returns the clip name for a state, falling back to walk then idle.</summary>
    public string ClipFor(LocomotionState state)
    {
        string clip = state switch
        {
            LocomotionState.Idle => IdleClip,
            LocomotionState.Walk => WalkClip,
            LocomotionState.Run => RunClip,
            LocomotionState.Jump => JumpClip,
            _ => "",
        };

        if (!string.IsNullOrEmpty(clip))
        {
            return clip;
        }

        // A model missing a run/jump clip still moves — reuse walk, then idle.
        return !string.IsNullOrEmpty(WalkClip) ? WalkClip : IdleClip;
    }
}
