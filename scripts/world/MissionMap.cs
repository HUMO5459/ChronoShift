using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// Root of a mission map. Watches its own engineering tasks and reports the
/// mission finished once every one of them is done.
///
/// It deliberately does not name its own mission: MissionData already points at
/// this map, and pointing back would make the two resources depend on each other.
/// MissionManager knows which mission is running.
/// </summary>
public partial class MissionMap : Node3D
{
    /// <summary>Beat between the last task finishing and the completion screen, so its reward toast lands first.</summary>
    [Export] private float _completionDelay = 1.2f;

    private readonly List<EngineeringTask> _tasks = new();
    private bool _finished;

    public override void _Ready()
    {
        AddToGroup(MapGroup);
        CollectTasks(this);
        if (_tasks.Count == 0)
        {
            GD.PushWarning($"MissionMap '{Name}' has no EngineeringTask nodes; it can never finish.");
            return;
        }

        foreach (EngineeringTask task in _tasks)
        {
            task.TaskCompleted += _ => CheckFinished();
        }

        // A resumed save can arrive with every task already done; nothing would emit
        // TaskCompleted then, so the map would sit finished but never report it.
        CallDeferred(MethodName.CheckFinished);
    }

    /// <summary>How many of this map's tasks are done, for the HUD's mission counter.</summary>
    public int CompletedCount
    {
        get
        {
            int done = 0;
            foreach (EngineeringTask task in _tasks)
            {
                if (task.IsCompleted)
                {
                    done++;
                }
            }

            return done;
        }
    }

    /// <summary>Total tasks on this map.</summary>
    public int TaskCount => _tasks.Count;

    /// <summary>This map's objectives in the order the mission data lays them out.</summary>
    public IReadOnlyList<EngineeringTask> Tasks => _tasks;

    /// <summary>
    /// What the player should be doing right now: whatever is under way, or else the
    /// first objective still outstanding. Null once the map is finished. The HUD and
    /// the journal both point at this rather than at whichever task happens to be
    /// nearest, which was misleading whenever the next objective was across the map.
    /// </summary>
    public EngineeringTask? CurrentObjective
    {
        get
        {
            foreach (EngineeringTask task in _tasks)
            {
                if (task.IsActive)
                {
                    return task;
                }
            }

            foreach (EngineeringTask task in _tasks)
            {
                if (!task.IsCompleted)
                {
                    return task;
                }
            }

            return null;
        }
    }

    /// <summary>1-based position of <paramref name="task"/> in map order; 0 when it is not ours.</summary>
    public int NumberOf(EngineeringTask task) => _tasks.IndexOf(task) + 1;

    private void CollectTasks(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is EngineeringTask task)
            {
                _tasks.Add(task);
            }

            CollectTasks(child);
        }
    }

    /// <summary>Group the HUD uses to find the map currently being played.</summary>
    public const string MapGroup = "mission_map";

    private void CheckFinished()
    {
        if (_finished)
        {
            return;
        }

        foreach (EngineeringTask task in _tasks)
        {
            if (!task.IsCompleted)
            {
                return;
            }
        }

        _finished = true;

        // The map only reports; the completion screen decides when to leave.
        GetTree().CreateTimer(_completionDelay).Timeout += () => MissionManager.Instance?.ReportObjectivesComplete();
    }
}
