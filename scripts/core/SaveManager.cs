using Godot;
using System.Collections.Generic;
using System.Text.Json;
using FileAccess = Godot.FileAccess;

namespace ChronoShift;

/// <summary>Persists and restores the aggregate game state via a user:// JSON file.</summary>
public partial class SaveManager : Node
{
    public static SaveManager Instance { get; private set; } = null!;

    [Export] private string _savePath = "user://savegame.json";

    /// <summary>
    /// Seconds between background flushes. Gathering and production change the run
    /// several times a second, so those ask for a save rather than forcing one; a
    /// hard crash or a force-quit now costs at most this much progress.
    /// </summary>
    [Export] private double _autosaveSeconds = 20.0;

    /// <summary>Set when something changed that a background flush should pick up.</summary>
    private bool _dirty;

    private double _sinceFlush;

    public override void _Ready()
    {
        Instance = this;
        GetTree().AutoAcceptQuit = false; // so we can save before the window closes

        // The autosave has to keep ticking while the game is paused: the pause menu
        // is exactly where a player stops to think, and where they quit from.
        ProcessMode = ProcessModeEnum.Always;

        Load();

        if (MissionManager.Instance != null)
        {
            // Milestones are written straight away — losing one of these is what
            // "my progress did not save" actually looks like from the player's side.
            MissionManager.Instance.MissionCompleted += (_, _, _) => Save();
            MissionManager.Instance.MissionStarted += _ => Save();
            MissionManager.Instance.MissionFinished += _ => Save();
            MissionManager.Instance.MissionAborted += _ => Save();
            MissionManager.Instance.CampaignCompleted += Save;
        }

        if (TechTreeManager.Instance != null)
        {
            TechTreeManager.Instance.TechUnlocked += _ => Save();
        }

        // The rest change often — bounties, crates, gathering, production — so they
        // only mark the run dirty and let the flush below batch them.
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.InventoryChanged += RequestSave;
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.MoneyChanged += _ => RequestSave();
        }

        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.XpChanged += (_, _) => RequestSave();
        }
    }

    public override void _Process(double delta)
    {
        if (!_dirty)
        {
            return;
        }

        _sinceFlush += delta;
        if (_sinceFlush >= _autosaveSeconds)
        {
            Save();
        }
    }

    /// <summary>
    /// Marks the run as changed. The write happens on the next flush rather than
    /// now, so a run through a deposit field does not rewrite the file per pickup.
    /// </summary>
    public void RequestSave() => _dirty = true;

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            Save();
            GetTree().Quit();
        }
    }

    /// <summary>True when a save file exists on disk (used to gate the Continue button).</summary>
    public bool HasSave() => FileAccess.FileExists(_savePath);

    /// <summary>
    /// Reads the save without applying it, so the save book can label its slot.
    /// Returns null when there is no save or it cannot be parsed.
    /// </summary>
    public SaveData? Peek() => ReadSave();

    private SaveData? ReadSave()
    {
        if (!FileAccess.FileExists(_savePath))
        {
            return null;
        }

        try
        {
            using FileAccess file = FileAccess.Open(_savePath, FileAccess.ModeFlags.Read);
            return file == null ? null : JsonSerializer.Deserialize<SaveData>(file.GetAsText());
        }
        catch (System.Exception e)
        {
            GD.PushWarning("SaveManager: failed to read save file: " + e.Message);
            return null;
        }
    }

    public void Save()
    {
        try
        {
            var data = new SaveData
            {
                Money = EconomyManager.Instance != null ? EconomyManager.Instance.Money : 0,
                Xp = ProgressionManager.Instance != null ? ProgressionManager.Instance.Xp : 0,
                Level = ProgressionManager.Instance != null ? ProgressionManager.Instance.Level : 1,
                CompletedCount = MissionManager.Instance != null ? MissionManager.Instance.CompletedCount : 0,
                CompletedTaskIds = MissionManager.Instance != null
                    ? new List<string>(MissionManager.Instance.CompletedTaskIds)
                    : new List<string>(),
                ElapsedDays = TimeManager.Instance != null ? TimeManager.Instance.ElapsedDays : 0.0,
                UnlockedTech = TechTreeManager.Instance != null
                    ? new List<string>(TechTreeManager.Instance.UnlockedTechIds)
                    : new List<string>(),
                Inventory = InventoryManager.Instance != null
                    ? InventoryManager.Instance.Snapshot()
                    : new Dictionary<string, int>(),
                ActiveMissionId = MissionManager.Instance != null ? MissionManager.Instance.ActiveMissionId : "",
                RunTaskIds = MissionManager.Instance != null
                    ? new List<string>(MissionManager.Instance.RunTaskIds)
                    : new List<string>(),
                SelectedHeroId = CharacterRoster.SelectedId,
                SelectedVehicleId = VehicleRoster.SelectedId,
                Designs = DesignRegistry.Snapshot(),
            };

            string json = JsonSerializer.Serialize(data);
            using FileAccess file = FileAccess.Open(_savePath, FileAccess.ModeFlags.Write);
            if (file != null)
            {
                file.StoreString(json);
            }

            _dirty = false;
            _sinceFlush = 0.0;
        }
        catch (System.Exception e)
        {
            GD.PushWarning("SaveManager: failed to save game state: " + e.Message);
        }
    }

    public void Load()
    {
        SaveData? data = ReadSave();
        if (data == null)
        {
            GD.Print("SaveManager: no save found, starting fresh.");
            return;
        }

        // Restore Time last so it recomputes scale from the restored CompletedCount.
        EconomyManager.Instance?.LoadState(data.Money);
        ProgressionManager.Instance?.LoadState(data.Xp, data.Level);
        MissionManager.Instance?.LoadState(
            data.CompletedCount, data.CompletedTaskIds, data.ActiveMissionId, data.RunTaskIds);
        CharacterRoster.SelectedId = data.SelectedHeroId;

        // Designs before the vehicle choice: the selected id may name a machine the
        // player built, which only exists once its plan has been restored.
        DesignRegistry.LoadState(data.Designs);
        VehicleRoster.SelectedId = data.SelectedVehicleId;
        TechTreeManager.Instance?.LoadState(data.UnlockedTech);
        TimeManager.Instance?.LoadState(data.ElapsedDays);
        InventoryManager.Instance?.LoadState(data.Inventory);

        GD.Print($"SaveManager: loaded save (money={data.Money}, level={data.Level}, tech={data.UnlockedTech.Count}).");
    }
}
