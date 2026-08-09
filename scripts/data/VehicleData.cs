using Godot;

namespace ChronoShift;

/// <summary>
/// A selectable vehicle: the scene to spawn plus the card shown in the depot
/// menu. The chosen vehicle is dropped next to the player when a mission starts.
/// </summary>
[GlobalClass]
public partial class VehicleData : Resource
{
    [Export] public string VehicleId = "";
    [Export] public string DisplayName = "Texnika";

    /// <summary>Category label for the card, e.g. "ASOSIY TANK" or "YENGIL TRANSPORT".</summary>
    [Export] public string Kind = "";

    [Export(PropertyHint.MultilineText)] public string Description = "";

    /// <summary>The vehicle scene spawned on a mission map; empty means "on foot".</summary>
    [Export] public PackedScene? VehicleScene;

    /// <summary>Optional livery tint applied to the spawned vehicle (white = none).</summary>
    [Export] public Color LiveryTint = Colors.White;

    // --- Characteristics -----------------------------------------------------
    // These drive the spawned vehicle as well as the stats screen, so the numbers
    // the player reads are the numbers the game uses. Zero means "leave the scene's
    // own value alone", which keeps a half-filled resource from nerfing a vehicle.

    /// <summary>Hull points.</summary>
    [Export] public float MaxHealth;

    /// <summary>Top speed, metres per second.</summary>
    [Export] public float MoveSpeed;

    /// <summary>Steering rate, radians per second.</summary>
    [Export] public float TurnSpeed;

    /// <summary>
    /// Fraction of incoming damage the plating absorbs, 0..0.85. Unlike the rest
    /// this has no scene fallback — armour is a property of the vehicle class,
    /// not of the model.
    /// </summary>
    [Export] public float Armor;

    /// <summary>What the depot charges to field one, for the production screen.</summary>
    [Export] public int ProductionCost;

    // --- Armament (leave the damage at 0 for unarmed vehicles) ---------------

    /// <summary>Gun name for the stats screen, e.g. "125 mm 2A46".</summary>
    [Export] public string ArmamentName = "";

    [Export] public float ArmamentDamage;
    [Export] public float ArmamentRange;
    [Export] public float ArmamentReloadSeconds;

    /// <summary>True when this vehicle carries a gun worth listing.</summary>
    public bool IsArmed => ArmamentDamage > 0.0f;
}
