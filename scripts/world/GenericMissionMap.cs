using Godot;

namespace ChronoShift;

/// <summary>
/// A mission map assembled at runtime from a <see cref="MapData"/> resource: it
/// lays the ground, sets the era's sky/fog/sun, marks the spawn, and drops the
/// tasks, crates and groves. The active mission supplies the data, so all missions
/// share this one scene and differ only by their MapData.
/// </summary>
public partial class GenericMissionMap : MissionMap
{
    /// <summary>The workstation prop each task is built on (root is an EngineeringTask).</summary>
    [Export] private PackedScene _workstationScene = null!;

    /// <summary>Destructible crate prop, placed at each of the map's crate positions.</summary>
    [Export] private PackedScene? _crateScene;

    /// <summary>Tree cluster prop, placed at each of the map's grove positions.</summary>
    [Export] private PackedScene? _groveScene;

    /// <summary>Enemy target, placed at each of the map's enemy positions.</summary>
    [Export] private PackedScene? _enemyScene;

    /// <summary>Harvestable deposit prop, placed at each of the map's resource positions.</summary>
    [Export] private PackedScene? _resourceNodeScene;

    /// <summary>Field workshop where the map's production orders are run.</summary>
    [Export] private PackedScene? _workshopScene;

    /// <summary>Standing NPC (idle rig) — workers at a post, guards, officers.</summary>
    [Export] private PackedScene? _npcScene;

    /// <summary>Walking NPC (walk rig) — used for any NPC whose data patrols.</summary>
    [Export] private PackedScene? _npcWalkerScene;

    /// <summary>Used when the scene is run on its own (no active mission) to preview a map.</summary>
    [Export] private MapData? _previewMap;

    public override void _Ready()
    {
        MapData? map = MissionManager.Instance?.ActiveMission?.Map ?? _previewMap;
        if (map != null)
        {
            BuildWorld(map);
        }
        else
        {
            GD.PushWarning("GenericMissionMap: no MapData (no active mission and no preview); empty map.");
        }

        // MissionMap collects the tasks we just added and wires up completion.
        base._Ready();
    }

    private void BuildWorld(MapData map)
    {
        BuildGround(map);
        BuildEnvironment(map);
        BuildSun(map);
        BuildSpawn(map);
        BuildResourceNodes(map);
        BuildWorkshop(map);
        BuildNpcs(map);
        BuildTasks(map);
        BuildProps(map);
    }

    private void BuildNpcs(MapData map)
    {
        for (int i = 0; i < map.Npcs.Length; i++)
        {
            NpcData? data = map.Npcs[i];
            if (data == null)
            {
                continue;
            }

            // A patrolling NPC needs the walking rig or it slides along in a standing pose.
            PackedScene? scene = data.UsesWalkModel ? _npcWalkerScene : _npcScene;
            if (scene == null)
            {
                continue;
            }

            var npc = scene.Instantiate<Node3D>();
            npc.Position = i < map.NpcPositions.Length ? map.NpcPositions[i] : Vector3.Zero;
            npc.RotationDegrees = new Vector3(0, i < map.NpcRotationsY.Length ? map.NpcRotationsY[i] : 0.0f, 0);

            // Set before the node enters the tree so its _Ready reads the role.
            npc.Set("_data", data);
            AddChild(npc);
        }
    }

    private void BuildResourceNodes(MapData map)
    {
        if (_resourceNodeScene == null || map.ResourceNodes.Length == 0)
        {
            return;
        }

        for (int i = 0; i < map.ResourceNodes.Length; i++)
        {
            ItemData? item = map.ResourceNodes[i];
            if (item == null)
            {
                continue;
            }

            var deposit = _resourceNodeScene.Instantiate<Node3D>();
            deposit.Position = i < map.ResourceNodePositions.Length ? map.ResourceNodePositions[i] : Vector3.Zero;

            // Set before the node enters the tree so its _Ready tints and counts correctly.
            deposit.Set("_item", item);
            deposit.Set("_amount", i < map.ResourceNodeAmounts.Length ? map.ResourceNodeAmounts[i] : 4);
            deposit.Set("_uses", map.ResourceNodeUses);
            AddChild(deposit);
        }
    }

    private void BuildWorkshop(MapData map)
    {
        if (!map.HasWorkshop || _workshopScene == null)
        {
            return;
        }

        var workshop = _workshopScene.Instantiate<Node3D>();
        workshop.Position = map.WorkshopPosition;
        workshop.RotationDegrees = new Vector3(0, map.WorkshopRotationY, 0);
        workshop.Set("_recipes", new Godot.Collections.Array<RecipeData>(map.Recipes));
        AddChild(workshop);
    }

    private void BuildGround(MapData map)
    {
        var ground = new StaticBody3D { Name = "Ground" };
        AddChild(ground);

        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = new Vector3(map.GroundSize, 1, map.GroundSize) },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = map.GroundColor, Roughness = 1.0f },
            Position = new Vector3(0, -0.5f, 0),
        };
        ground.AddChild(mesh);

        var collision = new CollisionShape3D
        {
            Shape = new BoxShape3D { Size = new Vector3(map.GroundSize, 1, map.GroundSize) },
            Position = new Vector3(0, -0.5f, 0),
        };
        ground.AddChild(collision);
    }

    private void BuildEnvironment(MapData map)
    {
        var sky = new ProceduralSkyMaterial
        {
            SkyTopColor = map.SkyTopColor,
            SkyHorizonColor = map.SkyHorizonColor,
            GroundBottomColor = map.SkyGroundColor,
            GroundHorizonColor = map.SkyHorizonColor,
        };

        var environment = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Sky,
            Sky = new Sky { SkyMaterial = sky },
            AmbientLightSource = Godot.Environment.AmbientSource.Sky,
            AmbientLightColor = map.AmbientColor,
            AmbientLightSkyContribution = 0.7f,
            FogEnabled = map.FogDensity > 0.0f,
            FogLightColor = map.FogColor,
            FogDensity = map.FogDensity,
        };

        AddChild(new WorldEnvironment { Environment = environment });
    }

    private void BuildSun(MapData map)
    {
        var sun = new DirectionalLight3D
        {
            Name = "Sun",
            ShadowEnabled = true,
            LightColor = map.SunColor,
            LightEnergy = map.SunEnergy,
            RotationDegrees = map.SunRotationDegrees,
        };
        AddChild(sun);
    }

    private void BuildSpawn(MapData map)
    {
        // Named "Spawn" so MapLoader moves the player here after the swap.
        AddChild(new Node3D { Name = "Spawn", Position = map.SpawnPosition });
    }

    private void BuildTasks(MapData map)
    {
        if (_workstationScene == null)
        {
            GD.PushWarning("GenericMissionMap: no workstation scene; tasks not built.");
            return;
        }

        for (int i = 0; i < map.Tasks.Length; i++)
        {
            EngineeringTaskData? data = map.Tasks[i];
            if (data == null)
            {
                continue;
            }

            var station = _workstationScene.Instantiate<Node3D>();
            station.Position = i < map.TaskPositions.Length ? map.TaskPositions[i] : Vector3.Zero;
            float yaw = i < map.TaskRotationsY.Length ? map.TaskRotationsY[i] : 0.0f;
            station.RotationDegrees = new Vector3(0, yaw, 0);

            // Assign the task data before the node enters the tree so its _Ready sees it.
            station.Set("_taskData", data);
            AddChild(station);
        }
    }

    private void BuildProps(MapData map)
    {
        if (_crateScene != null)
        {
            foreach (Vector3 position in map.CratePositions)
            {
                var crate = _crateScene.Instantiate<Node3D>();
                crate.Position = position;
                AddChild(crate);
            }
        }

        if (_groveScene != null)
        {
            foreach (Vector3 position in map.GrovePositions)
            {
                var grove = _groveScene.Instantiate<Node3D>();
                grove.Position = position;
                AddChild(grove);
            }
        }

        if (_enemyScene != null)
        {
            foreach (Vector3 position in map.EnemyPositions)
            {
                var enemy = _enemyScene.Instantiate<Node3D>();
                enemy.Position = position;
                AddChild(enemy);
            }
        }
    }
}
