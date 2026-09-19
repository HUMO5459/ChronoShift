using Godot;

namespace ChronoShift;

/// <summary>
/// Every frame-rate-dependent smoothing expression the web build uses, in one
/// place, verbatim.
/// </summary>
/// <remarks>
/// These are the numbers that decide how the game FEELS: how heavily the camera
/// trails the player, how sharply the body snaps to a new heading, how fast the
/// fog thickens before a wave. They were tuned by hand against a browser running
/// at 60 Hz.
///
/// The rule this file exists to enforce: NO OTHER FILE MAY RE-DERIVE A SMOOTHING
/// CONSTANT. Two call sites that drift apart by 0.01 produce a camera that feels
/// wrong in a way nobody can locate afterwards.
///
/// Four of these are NOT frame-rate corrected in the original — <see cref="Fov"/>,
/// <see cref="GaitBlend"/>, <see cref="Fog"/> and <see cref="ChizmaFade"/> multiply
/// delta directly, so their behaviour depends on the tick rate. They are ported as
/// they are, which is why the project must run at 60 physics ticks per second: at
/// any other rate these four stop matching the tuning they were given.
/// </remarks>
public static class Smoothing
{
    /// <summary>
    /// Camera follow. <c>1 - pow(1 - 0.12, dt * 60)</c> (player.js:449).
    /// Frame-rate corrected: 0.12 per 60 Hz frame, so the camera lags slightly and
    /// the movement reads as weight rather than as a rigid mount.
    /// </summary>
    public static float CameraPosition(double delta)
    {
        return 1.0f - Mathf.Pow(1.0f - 0.12f, (float)delta * 60.0f);
    }

    /// <summary>
    /// Field-of-view easing when mounting or dismounting.
    /// <c>min(1, dt * 3)</c> (player.js:437). NOT frame-rate corrected.
    /// </summary>
    public static float Fov(double delta)
    {
        return Mathf.Min(1.0f, (float)delta * 3.0f);
    }

    /// <summary>
    /// Player body turning toward its heading. <c>1 - exp(-14 * dt)</c>
    /// (player.js:340). Frame-rate corrected and deliberately fast: the body must
    /// arrive before the next step lands or the feet cross over.
    /// </summary>
    public static float BodyYaw(double delta)
    {
        return 1.0f - Mathf.Exp(-14.0f * (float)delta);
    }

    /// <summary>
    /// Apprentices and enemies turning. <c>1 - pow(1e-6, dt)</c>
    /// (apprentices.js:367, combat.js:303). Near-instant; the visible softness
    /// comes from their pathing, not from this.
    /// </summary>
    public static float ActorYaw(double delta)
    {
        return 1.0f - Mathf.Pow(1e-6f, (float)delta);
    }

    /// <summary>
    /// Limb return to rest when a procedural actor stops.
    /// <c>pow(0.02, dt)</c> (player.js:269, apprentices.js:351). This is a
    /// MULTIPLIER applied to the current angle, not a lerp weight.
    /// </summary>
    public static float LimbRelax(double delta)
    {
        return Mathf.Pow(0.02f, (float)delta);
    }

    /// <summary>
    /// Ending camera drift. <c>1 - pow(0.02, dt)</c> (game.js:629).
    /// </summary>
    public static float EndingCamera(double delta)
    {
        return 1.0f - Mathf.Pow(0.02f, (float)delta);
    }

    /// <summary>
    /// Walk-to-run blend weight. <c>min(1, dt * 7)</c> (anim.js:187).
    /// NOT frame-rate corrected.
    /// </summary>
    public static float GaitBlend(double delta)
    {
        return Mathf.Min(1.0f, (float)delta * 7.0f);
    }

    /// <summary>
    /// Fog density approach. <c>dt * 0.6</c> (game.js:179). NOT frame-rate
    /// corrected, and not clamped in the original either. The fog thickening as a
    /// wave approaches is the game's only ambient warning, so this rate is content.
    /// </summary>
    public static float Fog(double delta)
    {
        return (float)delta * 0.6f;
    }

    /// <summary>
    /// Blueprint panel fade-in. <c>dt * 2.2</c> (chizma.js:105), accumulated and
    /// clamped to 1 by the caller. NOT frame-rate corrected.
    /// </summary>
    public static float ChizmaFade(double delta)
    {
        return (float)delta * 2.2f;
    }
}
