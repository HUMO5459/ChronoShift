namespace ChronoShift;

/// <summary>
/// Remembers the vehicle the player picked in the depot. A plain static so the
/// choice survives the menu-to-gameplay scene change without an autoload.
/// The default (empty id) means "on foot" — no vehicle spawns.
/// </summary>
public static class VehicleRoster
{
    public static string SelectedId { get; set; } = "";

    /// <summary>The chosen vehicle, or null for on-foot / unresolved.</summary>
    public static VehicleData? Resolve(VehicleRosterData? roster)
    {
        if (string.IsNullOrEmpty(SelectedId))
        {
            return null;
        }

        // A vehicle the player designed and produced is not in the stock roster,
        // so the workshop's output is checked first.
        VehicleData? built = DesignRegistry.BuiltVehicle(SelectedId);
        if (built != null)
        {
            return built;
        }

        if (roster == null)
        {
            return null;
        }

        foreach (VehicleData vehicle in roster.Vehicles)
        {
            if (vehicle != null && vehicle.VehicleId == SelectedId)
            {
                return vehicle;
            }
        }

        return null;
    }
}
