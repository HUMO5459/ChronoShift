using Godot;

namespace ChronoShift;

/// <summary>
/// The "Vazifalar jurnali" from the UI design: a clipboard of the campaign's
/// missions, their briefings, total rewards, and completion stamps. A mission is
/// stamped done once every engineering task on its map has been finished.
/// </summary>
public partial class MissionsPanel : Control
{
    [Signal]
    public delegate void ClosedEventHandler();

    [Export] private NodePath _rowsPath = "Scrim/Center/Board/Rows/Scroll/Missions";
    [Export] private NodePath _backButtonPath = "Scrim/Center/Board/Rows/Header/HeaderRow/Close";
    [Export] private NodePath _footerCountPath = "Scrim/Center/Board/Rows/Footer/Count";
    [Export] private NodePath _footerRulePath = "Scrim/Center/Board/Rows/Footer/Rule";

    [Export] private PackedScene _missionRowScene = null!;

    /// <summary>The campaign's missions, in order — the same list the board offers.</summary>
    [Export] private Godot.Collections.Array<MissionData> _missions = new();

    private VBoxContainer _rows = null!;

    public override void _Ready()
    {
        Visible = false;
        _rows = GetNode<VBoxContainer>(_rowsPath);

        GetNode<Button>(_backButtonPath).Pressed += OnBackPressed;

        BuildRows();
    }

    /// <summary>Refreshes stamps and the tally; the journal can outlive a load.</summary>
    public void Refresh()
    {
        foreach (Node child in _rows.GetChildren())
        {
            child.QueueFree();
        }

        BuildRows();
    }

    /// <summary>True once every task on the mission's map has been finished at least once.</summary>
    public static bool IsMissionComplete(MissionData mission)
    {
        if (mission?.Map == null || mission.Map.Tasks.Length == 0 || MissionManager.Instance == null)
        {
            return false;
        }

        foreach (EngineeringTaskData? task in mission.Map.Tasks)
        {
            if (task != null && !MissionManager.Instance.IsTaskCompleted(task.TaskId))
            {
                return false;
            }
        }

        return true;
    }

    private void BuildRows()
    {
        if (_missionRowScene == null)
        {
            GD.PushWarning("MissionsPanel: no mission row scene assigned.");
            return;
        }

        int completed = 0;
        int total = 0;

        foreach (MissionData mission in _missions)
        {
            if (mission == null)
            {
                continue;
            }

            bool done = IsMissionComplete(mission);
            total++;
            if (done)
            {
                completed++;
            }

            var row = _missionRowScene.Instantiate<MissionRow>();
            _rows.AddChild(row);
            row.Bind(mission, done);
        }

        GetNode<Label>(_footerCountPath).Text = $"BAJARILDI: {completed}/{total}";

        int year = TimeManager.Instance != null ? TimeManager.Instance.CurrentYear : 1900;
        GetNode<Label>(_footerRulePath).Text =
            $"KAMPANIYA YILI: {year} — HAR BAJARILGAN VAZIFA KALENDARNI OLDINGA SURADI";
    }

    private void OnBackPressed()
    {
        Visible = false;
        EmitSignal(SignalName.Closed);
    }
}
