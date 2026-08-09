using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>Turns completed engineering tasks into money and XP rewards.</summary>
public partial class MissionManager : Node
{
    public static MissionManager Instance { get; private set; } = null!;

    [Signal]
    public delegate void MissionCompletedEventHandler(string taskId, int rewardMoney, int rewardXp);

    /// <summary>A mission's map has been loaded and the player is on it.</summary>
    [Signal]
    public delegate void MissionStartedEventHandler(string missionId);

    /// <summary>
    /// Every task on the mission map is done. The bonus is not paid yet — the
    /// completion screen shows first and calls FinishMission when the player leaves.
    /// </summary>
    [Signal]
    public delegate void MissionObjectivesCompleteEventHandler(string missionId);

    /// <summary>The bonus has been paid and the hub is coming back.</summary>
    [Signal]
    public delegate void MissionFinishedEventHandler(string missionId);

    /// <summary>The campaign's final mission is done — the prototype has been played through.</summary>
    [Signal]
    public delegate void CampaignCompletedEventHandler();

    /// <summary>The player walked away from a mission; the hub is coming back, unpaid.</summary>
    [Signal]
    public delegate void MissionAbortedEventHandler(string missionId);

    [Export] private string _missionFolder = "res://resources/data/missions";

    private readonly Dictionary<string, MissionData> _catalog = new();

    /// <summary>Tasks finished during the mission currently being played.</summary>
    private readonly HashSet<string> _runTaskIds = new();

    /// <summary>Id of the mission in progress, or empty in the hub — persisted in the save.</summary>
    public string ActiveMissionId => ActiveMission?.MissionId ?? "";

    /// <summary>Task ids already done on the map being played; a resumed save restores these.</summary>
    public IReadOnlyCollection<string> RunTaskIds => _runTaskIds;

    /// <summary>
    /// True when this task is already finished on the map currently being played.
    /// Distinct from <see cref="IsTaskCompleted"/>, which never forgets: replaying a
    /// mission has to hand the player fresh work rather than a map of green boxes.
    /// </summary>
    public bool IsTaskDoneThisRun(string taskId) => _runTaskIds.Contains(taskId);

    /// <summary>True once the final mission has been finished this campaign.</summary>
    public bool CampaignComplete { get; private set; }

    public int CompletedCount { get; private set; }

    /// <summary>The mission being played, or null while in the hub.</summary>
    public MissionData? ActiveMission { get; private set; }

    private readonly HashSet<string> _completedTaskIds = new();

    /// <summary>Ids of tasks finished at least once — the mission journal stamps these.</summary>
    public IReadOnlyCollection<string> CompletedTaskIds => _completedTaskIds;

    /// <summary>True when <paramref name="taskId"/> has been finished at least once this campaign.</summary>
    public bool IsTaskCompleted(string taskId) => _completedTaskIds.Contains(taskId);

    public override void _Ready()
    {
        Instance = this;
        LoadCatalog();
    }

    /// <summary>Missions are looked up by id so a save can name the one it was playing.</summary>
    private void LoadCatalog()
    {
        foreach (string path in ResourceFolder.Paths(_missionFolder))
        {
            var mission = ResourceLoader.Load<MissionData>(path);
            if (mission != null && !string.IsNullOrEmpty(mission.MissionId))
            {
                _catalog[mission.MissionId] = mission;
            }
        }
    }

    public MissionData? Resolve(string missionId) =>
        _catalog.TryGetValue(missionId, out MissionData? mission) ? mission : null;

    /// <summary>
    /// Puts the player back on the map they were on when they saved. Called by the
    /// shell once the world exists; without it a save made mid-mission reopened in
    /// the hub with the mission still marked active.
    /// </summary>
    public void ResumeActiveMission()
    {
        if (ActiveMission?.MapScene == null)
        {
            return;
        }

        FindLoader()?.LoadMap(ActiveMission.MapScene);
        EmitSignal(SignalName.MissionStarted, ActiveMission.MissionId);
    }

    public void CompleteTask(EngineeringTaskData data)
    {
        if (data == null)
        {
            return;
        }

        int money = data.RewardMoney;
        if (TechTreeManager.Instance != null)
        {
            money = Mathf.RoundToInt(money * TechTreeManager.Instance.RewardMoneyMultiplier);
        }

        EconomyManager.Instance?.AddMoney(money);
        ProgressionManager.Instance?.AddXp(data.RewardXp);
        CompletedCount++;
        _completedTaskIds.Add(data.TaskId);
        _runTaskIds.Add(data.TaskId);

        // The campaign calendar moves because work got done, not because time passed.
        TimeManager.Instance?.AdvanceDays(data.AdvanceDays);

        EmitSignal(SignalName.MissionCompleted, data.TaskId, data.RewardMoney, data.RewardXp);
    }

    /// <summary>Loads the mission's map and puts the player on it.</summary>
    public void StartMission(MissionData mission)
    {
        if (mission == null)
        {
            return;
        }

        MapLoader? loader = FindLoader();
        if (loader == null)
        {
            GD.PushWarning("MissionManager: no MapLoader in the scene; cannot start a mission.");
            return;
        }

        ActiveMission = mission;

        // A fresh run starts with all of the map's work to do, even on a replay.
        _runTaskIds.Clear();

        // Entering an era carries the calendar there; it never rewinds, so replaying
        // an early mission does not drag the campaign back in time.
        if (mission.EraYear > 0)
        {
            TimeManager.Instance?.AdvanceToYear(mission.EraYear);
        }

        loader.LoadMap(mission.MapScene);
        EmitSignal(SignalName.MissionStarted, mission.MissionId);
    }

    /// <summary>Called by the map once its last task is done; opens the completion screen.</summary>
    public void ReportObjectivesComplete()
    {
        if (ActiveMission != null)
        {
            EmitSignal(SignalName.MissionObjectivesComplete, ActiveMission.MissionId);
        }
    }

    /// <summary>Pays the completion bonus and returns the player to the hub.</summary>
    public void FinishMission()
    {
        if (ActiveMission == null)
        {
            return;
        }

        MissionData finished = ActiveMission;
        ActiveMission = null;
        _runTaskIds.Clear();

        if (finished.BonusMoney > 0)
        {
            EconomyManager.Instance?.AddMoney(finished.BonusMoney);
        }

        if (finished.BonusXp > 0)
        {
            ProgressionManager.Instance?.AddXp(finished.BonusXp);
        }

        FindLoader()?.LoadHub();

        if (finished.IsFinal)
        {
            // The closing screen takes over, so no MissionFinished — nothing should
            // slide the next briefing in underneath it.
            CampaignComplete = true;
            EmitSignal(SignalName.CampaignCompleted);
            return;
        }

        EmitSignal(SignalName.MissionFinished, finished.MissionId);
    }

    /// <summary>Abandons the mission and returns to the hub without paying the bonus.</summary>
    public void AbortMission()
    {
        if (ActiveMission == null)
        {
            return;
        }

        MissionData abandoned = ActiveMission;
        ActiveMission = null;
        _runTaskIds.Clear();

        FindLoader()?.LoadHub();
        EmitSignal(SignalName.MissionAborted, abandoned.MissionId);
    }

    private MapLoader? FindLoader() =>
        Instance?.GetTree().GetFirstNodeInGroup(MapLoader.LoaderGroup) as MapLoader;

    /// <summary>
    /// Restores campaign progress. <paramref name="completedTaskIds"/> is null for a
    /// fresh campaign or a pre-journal save; <paramref name="activeMissionId"/> is
    /// empty when the save was made in the hub.
    /// </summary>
    public void LoadState(
        int completedCount,
        IEnumerable<string>? completedTaskIds = null,
        string activeMissionId = "",
        IEnumerable<string>? runTaskIds = null)
    {
        CompletedCount = completedCount;

        _completedTaskIds.Clear();
        if (completedTaskIds != null)
        {
            foreach (string taskId in completedTaskIds)
            {
                _completedTaskIds.Add(taskId);
            }
        }

        _runTaskIds.Clear();
        if (runTaskIds != null)
        {
            foreach (string taskId in runTaskIds)
            {
                _runTaskIds.Add(taskId);
            }
        }

        // Only remember the mission; the shell reloads its map once the world is up.
        ActiveMission = string.IsNullOrEmpty(activeMissionId) ? null : Resolve(activeMissionId);
        if (ActiveMission == null)
        {
            _runTaskIds.Clear();
        }
    }
}
