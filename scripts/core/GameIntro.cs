namespace ChronoShift;

/// <summary>
/// Carries "this run just started fresh" across the scene change from the main
/// menu into gameplay, so the briefing plays for a new campaign but not when a
/// save is loaded. Static rather than an autoload: one bool, no lifecycle.
/// </summary>
public static class GameIntro
{
    /// <summary>True while the opening briefing still owes the player a showing.</summary>
    public static bool Pending { get; set; }
}
