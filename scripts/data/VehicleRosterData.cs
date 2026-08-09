using Godot;

namespace ChronoShift;

/// <summary>The set of selectable vehicles, shared by the depot menu and the map loader.</summary>
[GlobalClass]
public partial class VehicleRosterData : Resource
{
    [Export] public Godot.Collections.Array<VehicleData> Vehicles = new();
}
