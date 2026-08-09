namespace ChronoShift;

/// <summary>Serializable snapshot of the aggregate game state persisted between runs.</summary>
public class SaveData
{
    public int Version { get; set; } = 1;
    public int Money { get; set; }
    public int Xp { get; set; }
    public int Level { get; set; } = 1;
    public int CompletedCount { get; set; }
    public double ElapsedDays { get; set; }
    public System.Collections.Generic.List<string> UnlockedTech { get; set; } = new();

    /// <summary>
    /// Ids of tasks finished at least once, so the mission journal can stamp them.
    /// Absent in saves written before the journal existed; those load as an empty list.
    /// </summary>
    public System.Collections.Generic.List<string> CompletedTaskIds { get; set; } = new();

    /// <summary>
    /// Item id → count for everything the player is carrying: gathered materials
    /// and produced equipment. Absent in saves written before the production loop
    /// existed; those load as an empty satchel.
    /// </summary>
    public System.Collections.Generic.Dictionary<string, int> Inventory { get; set; } = new();

    /// <summary>
    /// The mission being played when the game was saved, empty when in the hub.
    /// Without it a save made on a mission map reopened in the hub with the mission
    /// still counted as active.
    /// </summary>
    public string ActiveMissionId { get; set; } = "";

    /// <summary>Tasks already finished on that map, so a resumed mission is not restarted.</summary>
    public System.Collections.Generic.List<string> RunTaskIds { get; set; } = new();

    /// <summary>Hero and vehicle picked in the menus; previously reset on every launch.</summary>
    public string SelectedHeroId { get; set; } = "";
    public string SelectedVehicleId { get; set; } = "";

    /// <summary>
    /// Constructions drawn up in the workshop, plans only — the finished vehicles
    /// and weapons are rebuilt from their modules on load. Absent in saves written
    /// before the drawing office existed; those load with no designs.
    /// </summary>
    public System.Collections.Generic.List<DesignRecord> Designs { get; set; } = new();
}
