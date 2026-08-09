using Godot;

namespace ChronoShift;

public enum TechEffectType { None = 0, RewardMoneyMultiplier = 1, PlayerSpeedMultiplier = 2 }

/// <summary>Data-driven definition of a single unlockable tech tree node.</summary>
[GlobalClass]
public partial class TechNodeData : Resource
{
    [Export] public string TechId = "";
    [Export] public string DisplayName = "";
    [Export(PropertyHint.MultilineText)] public string Description = "";
    [Export] public int CostMoney = 0;
    [Export] public int RequiredLevel = 0;
    [Export] public string[] PrerequisiteIds = System.Array.Empty<string>();
    [Export] public TechEffectType Effect = TechEffectType.None;
    [Export] public float EffectValue = 0.0f;
}
