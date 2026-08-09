using Godot;

namespace ChronoShift;

/// <summary>What a module can be fitted to.</summary>
public enum ModuleKind
{
    Vehicle,
    Weapon,
}

/// <summary>
/// The bay a module occupies. A design holds at most one module per slot, and the
/// first slot of each kind (Chassis, Frame) carries the template everything else
/// modifies.
/// </summary>
public enum ModuleSlot
{
    // Vehicle
    Chassis,
    Engine,
    Armor,
    Armament,

    // Weapon
    Frame,
    Barrel,
    Magazine,
    Sight,
}

/// <summary>
/// One part the workshop can fit to a design: what it costs and what it does to
/// the finished machine's characteristics. New parts — and therefore new vehicles
/// and weapons — are new resources, not new code.
/// </summary>
[GlobalClass]
public partial class ModuleData : Resource
{
    [Export] public string ModuleId = "";
    [Export] public string DisplayName = "Modul";
    [Export(PropertyHint.MultilineText)] public string Description = "";

    [Export] public ModuleKind Kind = ModuleKind.Vehicle;
    [Export] public ModuleSlot Slot = ModuleSlot.Chassis;

    /// <summary>
    /// Chassis modules only: the vehicle this design is built on. Its scene is what
    /// actually spawns, and its characteristics are the starting point.
    /// </summary>
    [Export] public VehicleData? BaseVehicle;

    /// <summary>Frame modules only: the weapon the design is built on.</summary>
    [Export] public WeaponData? BaseWeapon;

    // --- Characteristic deltas, added on top of the base -----------------------

    [Export] public float HealthBonus;
    [Export] public float SpeedBonus;

    /// <summary>Added to the absorbed-damage share; the total is clamped to 0.85.</summary>
    [Export] public float ArmorBonus;

    [Export] public float DamageBonus;
    [Export] public float RangeBonus;

    /// <summary>Seconds added to the reload — negative parts make it faster.</summary>
    [Export] public float ReloadDelta;

    [Export] public int MagazineBonus;

    // --- Price ----------------------------------------------------------------

    [Export] public int CostMoney;

    /// <summary>Materials consumed when the design goes into production.</summary>
    [Export] public ItemData[] Inputs = System.Array.Empty<ItemData>();

    /// <summary>Units of each input; parallel to <see cref="Inputs"/>.</summary>
    [Export] public int[] InputAmounts = System.Array.Empty<int>();

    /// <summary>Tech node that has to be unlocked first; empty means always available.</summary>
    [Export] public string RequiredTechId = "";

    /// <summary>Cost of input <paramref name="index"/>; 1 when the amounts array is short.</summary>
    public int AmountFor(int index) => index < InputAmounts.Length ? InputAmounts[index] : 1;

    /// <summary>True when the tech tree has opened this part up.</summary>
    public bool IsUnlocked =>
        string.IsNullOrEmpty(RequiredTechId)
        || TechTreeManager.Instance == null
        || TechTreeManager.Instance.IsUnlocked(RequiredTechId);
}
