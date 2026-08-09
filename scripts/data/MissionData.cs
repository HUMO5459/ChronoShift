using Godot;

namespace ChronoShift;

/// <summary>
/// A mission: the briefing shown in the journal and the map it takes place on.
/// The tasks themselves live in the map scene, not here.
/// </summary>
[GlobalClass]
public partial class MissionData : Resource
{
    [Export] public string MissionId = "";
    [Export] public string DisplayName = "Missiya";
    [Export(PropertyHint.MultilineText)] public string Description = "";

    /// <summary>Era/kind label for the board, e.g. "TA'MIRLASH".</summary>
    [Export] public string Kind = "";

    /// <summary>The world loaded when this mission starts (the shared generic map scene).</summary>
    [Export] public PackedScene? MapScene;

    /// <summary>Data the generic map reads to build this mission's world and objectives.</summary>
    [Export] public MapData? Map;

    /// <summary>Paid on top of the task rewards once every task on the map is done.</summary>
    [Export] public int BonusMoney;
    [Export] public int BonusXp;

    /// <summary>
    /// The year this mission's era opens in. Finishing the mission carries the
    /// campaign calendar to the next mission's era year, so the decades pass as
    /// engineering work gets done rather than while the player stands around.
    /// </summary>
    [Export] public int EraYear;

    /// <summary>
    /// The campaign's closing mission. Finishing it ends the prototype with the
    /// summary screen instead of handing the player another briefing.
    /// </summary>
    [Export] public bool IsFinal;
}
