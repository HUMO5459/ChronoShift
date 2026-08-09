using Godot;

namespace ChronoShift;

/// <summary>Data-driven definition of a timed engineering task.</summary>
[GlobalClass]
public partial class EngineeringTaskData : Resource
{
    [Export] public string TaskId = "";
    [Export] public string DisplayName = "Engineering Task";

    /// <summary>Work category shown on the HUD tracker, e.g. "QURILISH" or "TA'MIRLASH".</summary>
    [Export] public string Kind = "";

    /// <summary>Briefing line shown in the mission journal.</summary>
    [Export(PropertyHint.MultilineText)] public string Description = "";

    [Export] public float DurationSeconds = 3.0f;
    [Export] public int RewardMoney = 0;
    [Export] public int RewardXp = 0;

    /// <summary>
    /// Equipment the workshop must have produced before this task can be started.
    /// Null means no requirement, which is how every task behaved before the
    /// production loop existed — old task resources keep working untouched.
    /// </summary>
    [Export] public ItemData? RequiredItem;

    /// <summary>How many of <see cref="RequiredItem"/> the task needs.</summary>
    [Export] public int RequiredAmount = 1;

    /// <summary>Whether starting the task spends the equipment (tools are kept).</summary>
    [Export] public bool ConsumesRequirement = true;

    /// <summary>
    /// In-game days the calendar jumps forward when this task is finished. The
    /// campaign advances through work, not through wall-clock time.
    /// </summary>
    [Export] public float AdvanceDays = 400.0f;
}
