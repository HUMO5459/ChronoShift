using Godot;

namespace ChronoShift;

/// <summary>
/// Everything needed to build a mission map from data: the era's look (ground,
/// sky, fog, sun), where the player arrives, and where each engineering task and
/// prop sits. One generic map scene reads this and assembles the world, so a new
/// mission is a new resource — not a new hand-built scene.
/// </summary>
[GlobalClass]
public partial class MapData : Resource
{
    /// <summary>Era label for flavour, e.g. "1900 — Imperiya chegarasi".</summary>
    [Export] public string EraName = "";

    // --- Ground ---
    [Export] public float GroundSize = 70.0f;
    [Export] public Color GroundColor = new(0.42f, 0.44f, 0.36f);

    // --- Sky ---
    [Export] public Color SkyTopColor = new(0.38f, 0.5f, 0.68f);
    [Export] public Color SkyHorizonColor = new(0.68f, 0.72f, 0.76f);
    [Export] public Color SkyGroundColor = new(0.31f, 0.33f, 0.3f);

    // --- Atmosphere ---
    [Export] public Color AmbientColor = new(0.68f, 0.72f, 0.78f);
    [Export] public Color FogColor = new(0.72f, 0.75f, 0.78f);
    [Export] public float FogDensity = 0.012f;

    // --- Sun ---
    [Export] public Color SunColor = new(1.0f, 0.98f, 0.92f);
    [Export] public float SunEnergy = 1.0f;
    [Export] public Vector3 SunRotationDegrees = new(-50.0f, 35.0f, 0.0f);

    /// <summary>Where the player (and, on a mission, the chosen vehicle) arrives.</summary>
    [Export] public Vector3 SpawnPosition = new(0, 1, 6);

    // --- Objectives ---
    /// <summary>The engineering tasks on this map; every one must be done to finish.</summary>
    [Export] public EngineeringTaskData[] Tasks = System.Array.Empty<EngineeringTaskData>();

    /// <summary>World position for each task, parallel to <see cref="Tasks"/>.</summary>
    [Export] public Vector3[] TaskPositions = System.Array.Empty<Vector3>();

    /// <summary>Y rotation (degrees) for each task, parallel to <see cref="Tasks"/>. May be shorter.</summary>
    [Export] public float[] TaskRotationsY = System.Array.Empty<float>();

    // --- Decoration (optional) ---
    /// <summary>Destructible crates, doubling as target practice for the weapons.</summary>
    [Export] public Vector3[] CratePositions = System.Array.Empty<Vector3>();

    /// <summary>Tree clusters scattered for cover and scenery.</summary>
    [Export] public Vector3[] GrovePositions = System.Array.Empty<Vector3>();

    /// <summary>Hostile targets the player can shoot for a bounty; optional, not required to finish.</summary>
    [Export] public Vector3[] EnemyPositions = System.Array.Empty<Vector3>();

    // --- Gathering ---
    /// <summary>What each resource deposit on this map yields; parallel to the two arrays below.</summary>
    [Export] public ItemData[] ResourceNodes = System.Array.Empty<ItemData>();

    /// <summary>World position of each deposit, parallel to <see cref="ResourceNodes"/>.</summary>
    [Export] public Vector3[] ResourceNodePositions = System.Array.Empty<Vector3>();

    /// <summary>Units each deposit hands over per harvest, parallel to <see cref="ResourceNodes"/>.</summary>
    [Export] public int[] ResourceNodeAmounts = System.Array.Empty<int>();

    /// <summary>How many times a deposit can be harvested before it is spent.</summary>
    [Export] public int ResourceNodeUses = 3;

    // --- Production ---
    /// <summary>A field workshop lets the player close the gather → produce → build loop on the map.</summary>
    [Export] public bool HasWorkshop = true;

    [Export] public Vector3 WorkshopPosition = new(-8, 0, 2);
    [Export] public float WorkshopRotationY;

    /// <summary>Production orders this era's workshop offers.</summary>
    [Export] public RecipeData[] Recipes = System.Array.Empty<RecipeData>();

    // --- Inhabitants ---
    /// <summary>Friendly extras staffing this map; parallel to the two arrays below.</summary>
    [Export] public NpcData[] Npcs = System.Array.Empty<NpcData>();

    /// <summary>World position of each NPC, parallel to <see cref="Npcs"/>.</summary>
    [Export] public Vector3[] NpcPositions = System.Array.Empty<Vector3>();

    /// <summary>Facing (degrees) of each NPC, parallel to <see cref="Npcs"/>. May be shorter.</summary>
    [Export] public float[] NpcRotationsY = System.Array.Empty<float>();
}
