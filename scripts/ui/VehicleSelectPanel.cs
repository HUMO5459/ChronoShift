using Godot;

namespace ChronoShift;

/// <summary>
/// Vehicle depot, opened from the main menu. Lists the roster and remembers the
/// choice in VehicleRoster; the map loader spawns it beside the player on a mission.
/// </summary>
public partial class VehicleSelectPanel : Control
{
    [Signal] public delegate void ClosedEventHandler();

    [Export] private NodePath _rowsPath = "Scrim/Center/Board/Rows/Vehicles";
    [Export] private NodePath _closeButtonPath = "Scrim/Center/Board/Rows/Header/HeaderRow/Close";

    [Export] private PackedScene _rowScene = null!;
    [Export] private VehicleRosterData? _roster;

    private VBoxContainer _rows = null!;

    public override void _Ready()
    {
        _rows = GetNode<VBoxContainer>(_rowsPath);
        GetNode<Button>(_closeButtonPath).Pressed += Close;
        Visible = false;
    }

    public void Open()
    {
        BuildRows();
        Visible = true;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Visible && @event.IsActionPressed("ui_cancel"))
        {
            Close();
            GetViewport().SetInputAsHandled();
        }
    }

    private void BuildRows()
    {
        foreach (Node child in _rows.GetChildren())
        {
            _rows.RemoveChild(child);
            child.QueueFree();
        }

        if (_rowScene == null || _roster == null)
        {
            GD.PushWarning("VehicleSelectPanel: no row scene or roster assigned.");
            return;
        }

        // Resolve returns null for on-foot; match that against the empty selection.
        VehicleData? current = VehicleRoster.Resolve(_roster);

        foreach (VehicleData vehicle in _roster.Vehicles)
        {
            AddRow(vehicle, current);
        }

        // Machines the player designed and produced sit under the stock roster,
        // so the depot lists everything they can actually field in one place.
        foreach (VehicleData built in DesignRegistry.AllBuiltVehicles)
        {
            AddRow(built, current);
        }
    }

    private void AddRow(VehicleData? vehicle, VehicleData? current)
    {
        if (vehicle == null)
        {
            return;
        }

        bool isCurrent = current == null
            ? vehicle.VehicleScene == null
            : ReferenceEquals(vehicle, current);

        var row = _rowScene.Instantiate<VehicleSelectRow>();
        _rows.AddChild(row);
        row.Bind(vehicle, isCurrent);
        row.Picked += OnPicked;
    }

    private void OnPicked(VehicleData vehicle)
    {
        // On-foot entries have no scene; store an empty id so nothing spawns.
        VehicleRoster.SelectedId = vehicle.VehicleScene != null ? vehicle.VehicleId : "";
        BuildRows();
    }

    private void Close()
    {
        Visible = false;
        EmitSignal(SignalName.Closed);
    }
}
