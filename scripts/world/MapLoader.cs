using Godot;

namespace ChronoShift;

/// <summary>
/// Swaps the world under the game shell: the hub, or a mission map. Only the
/// world changes — the player, HUD and menus stay alive across the switch, so
/// no scene reload and only one map's objects are ever in memory.
/// </summary>
public partial class MapLoader : Node3D
{
    /// <summary>MissionManager finds the loader through this instead of holding a reference.</summary>
    public const string LoaderGroup = "map_loader";

    /// <summary>A map may mark where the player arrives with a node of this name.</summary>
    private const string SpawnNodeName = "Spawn";

    [Signal] public delegate void WorldChangedEventHandler();

    [Export] private NodePath _worldPath = "World";
    [Export] private NodePath _playerPath = "Player";

    /// <summary>The base the player returns to between missions.</summary>
    [Export] private PackedScene _hubScene = null!;

    /// <summary>Vehicle roster; the chosen one is dropped beside the player on a mission map.</summary>
    [Export] private VehicleRosterData? _vehicleRoster;

    /// <summary>Where the chosen vehicle appears relative to the player's spawn.</summary>
    [Export] private Vector3 _vehicleSpawnOffset = new(5, 0.5f, 0);

    private Node3D _world = null!;
    private Node3D _player = null!;

    public override void _Ready()
    {
        AddToGroup(LoaderGroup);
        _world = GetNode<Node3D>(_worldPath);
        _player = GetNode<Node3D>(_playerPath);

        // A save written on a mission map comes back onto that map instead of the
        // hub. Deferred so the shell's panels are listening by the time it fires.
        if (MissionManager.Instance?.ActiveMission != null)
        {
            CallDeferred(MethodName.ResumeMission);
        }
    }

    private void ResumeMission() => MissionManager.Instance?.ResumeActiveMission();

    public void LoadHub() => Swap(_hubScene, withVehicle: false);

    public void LoadMap(PackedScene? scene)
    {
        if (scene == null)
        {
            GD.PushWarning("MapLoader: mission has no map scene; staying put.");
            return;
        }

        Swap(scene, withVehicle: true);
    }

    private void Swap(PackedScene scene, bool withVehicle)
    {
        // Remove before adding so the old map's nodes never overlap the new one's.
        foreach (Node child in _world.GetChildren())
        {
            _world.RemoveChild(child);
            child.QueueFree();
        }

        var instance = scene.Instantiate<Node3D>();
        _world.AddChild(instance);

        Vector3 spawn = MovePlayerToSpawn(instance);
        if (withVehicle)
        {
            SpawnChosenVehicle(spawn);
        }

        EmitSignal(SignalName.WorldChanged);
    }

    private Vector3 MovePlayerToSpawn(Node3D world)
    {
        var spawn = world.GetNodeOrNull<Node3D>(SpawnNodeName);
        Vector3 position = spawn != null ? spawn.GlobalPosition : new Vector3(0, 1, 0);

        _player.GlobalPosition = position;

        // A CharacterBody3D keeps its velocity across the swap and would fall oddly.
        if (_player is CharacterBody3D body)
        {
            body.Velocity = Vector3.Zero;
        }

        return position;
    }

    /// <summary>Drops the player's chosen vehicle beside the spawn; nothing on foot.</summary>
    private void SpawnChosenVehicle(Vector3 playerSpawn)
    {
        VehicleData? choice = VehicleRoster.Resolve(_vehicleRoster);
        if (choice?.VehicleScene == null)
        {
            return;
        }

        var vehicle = choice.VehicleScene.Instantiate<Node3D>();

        // Hand over the roster entry before the vehicle enters the tree: its _Ready
        // reads the characteristics (hull, speed, armour, livery) straight off it.
        vehicle.Set("_data", choice);

        _world.AddChild(vehicle);
        vehicle.GlobalPosition = playerSpawn + _vehicleSpawnOffset;
    }
}
