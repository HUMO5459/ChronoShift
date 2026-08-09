using Godot;

namespace ChronoShift;

/// <summary>
/// Fire-and-forget sound playback, mirroring <see cref="Fx"/> for visuals.
/// </summary>
/// <remarks>
/// Every sound here is a one-shot: the player node is created, played and frees
/// itself when finished. Nothing pools or reuses players, because the game never
/// has more than a handful of overlapping effects and a pool would only add state
/// to get wrong.
/// </remarks>
public static class Sfx
{
    /// <summary>Plays <paramref name="stream"/> at a point in the world.</summary>
    public static void PlayAt(Node context, AudioStream? stream, Vector3 position,
                              float volumeDb = 0.0f, float pitchSpread = 0.08f)
    {
        if (stream == null || !GodotObject.IsInstanceValid(context))
        {
            return;
        }

        var player = new AudioStreamPlayer3D
        {
            Stream = stream,
            VolumeDb = volumeDb,
            PitchScale = Jitter(pitchSpread),
            // Effects outlive the node that fired them - a bullet impact must not
            // cut off because the muzzle flash was freed.
            MaxDistance = 60.0f,
            UnitSize = 6.0f,
        };

        context.GetTree().CurrentScene?.AddChild(player);
        player.GlobalPosition = position;
        player.Finished += player.QueueFree;
        player.Play();
    }

    /// <summary>Plays <paramref name="stream"/> without a position, for interface sounds.</summary>
    public static void PlayUi(Node context, AudioStream? stream, float volumeDb = 0.0f,
                              float pitchSpread = 0.0f)
    {
        if (stream == null || !GodotObject.IsInstanceValid(context))
        {
            return;
        }

        var player = new AudioStreamPlayer
        {
            Stream = stream,
            VolumeDb = volumeDb,
            PitchScale = Jitter(pitchSpread),
            // Interface sounds must survive the pause that opens the menu playing them.
            ProcessMode = Node.ProcessModeEnum.Always,
        };

        context.GetTree().Root.AddChild(player);
        player.Finished += player.QueueFree;
        player.Play();
    }

    /// <summary>One of <paramref name="streams"/>, or null when the set is empty.</summary>
    public static AudioStream? Pick(AudioStream[]? streams) =>
        streams == null || streams.Length == 0
            ? null
            : streams[GD.RandRange(0, streams.Length - 1)];

    /// <summary>
    /// Slight per-shot pitch variation, so a repeated sound does not read as a
    /// looping sample.
    /// </summary>
    private static float Jitter(float spread) =>
        spread <= 0.0f ? 1.0f : 1.0f + (float)GD.RandRange(-spread, spread);
}
