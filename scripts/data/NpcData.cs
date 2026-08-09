using Godot;

namespace ChronoShift;

/// <summary>What an NPC does with itself. Deliberately small — these are extras, not agents.</summary>
public enum NpcBehavior
{
    /// <summary>Stands at a post working; faces the way it was placed.</summary>
    Work,

    /// <summary>Walks between points around its post, pausing at each.</summary>
    Patrol,

    /// <summary>Stands guard, slowly scanning left and right.</summary>
    Guard,
}

/// <summary>
/// One inhabitant of the world: a worker, engineer, soldier, officer or villager.
/// Only the role label, the tint and the behaviour differ — the model is shared,
/// so a new kind of extra is a new resource rather than new art.
/// </summary>
[GlobalClass]
public partial class NpcData : Resource
{
    [Export] public string NpcId = "";
    [Export] public string DisplayName = "";

    /// <summary>Plate above the head, e.g. "ISHCHI".</summary>
    [Export] public string RoleLabel = "";

    [Export] public NpcBehavior Behavior = NpcBehavior.Work;

    /// <summary>Body tint, so roles read apart at a glance (one model for everyone).</summary>
    [Export] public Color Tint = new(1.0f, 1.0f, 1.0f);

    /// <summary>Walking speed for <see cref="NpcBehavior.Patrol"/>.</summary>
    [Export] public float MoveSpeed = 1.8f;

    /// <summary>How far from its post a patrolling NPC wanders.</summary>
    [Export] public float PatrolRadius = 6.0f;

    /// <summary>Seconds spent standing at each end of a patrol leg.</summary>
    [Export] public float PauseSeconds = 2.0f;

    /// <summary>Half-angle of a guard's scan, in degrees.</summary>
    [Export] public float ScanDegrees = 45.0f;

    /// <summary>What this NPC says when the player finishes an engineering task.</summary>
    [Export] public string MissionLine = "";

    /// <summary>
    /// True for NPCs that move: they use the walking rig instead of the standing
    /// one, so a patrol does not slide along the ground in an idle pose.
    /// </summary>
    [Export] public bool UsesWalkModel;
}
