using Godot;

namespace ChronoShift;

/// <summary>
/// Mission select, opened from the hub's briefing board. Same paper language as
/// the journal, but each row can be started — which loads that mission's map.
/// </summary>
public partial class MissionBoardPanel : Control
{
    [Export] private NodePath _rowsPath = "Scrim/Center/Board/Rows/Scroll/Missions";
    [Export] private NodePath _closeButtonPath = "Scrim/Center/Board/Rows/Header/HeaderRow/Close";

    [Export] private PackedScene _rowScene = null!;

    /// <summary>The missions offered on the board, in order.</summary>
    [Export] private Godot.Collections.Array<MissionData> _missions = new();

    /// <summary>
    /// Greet the player with the mission list instead of dropping them into an
    /// empty hub: the campaign is picked from a list, played on its own map, and
    /// the list comes back when the mission is done.
    /// </summary>
    [Export] private bool _openOnStart = true;

    private VBoxContainer _rows = null!;

    public override void _Ready()
    {
        AddToGroup(MissionBoard.PanelGroup);
        _rows = GetNode<VBoxContainer>(_rowsPath);
        GetNode<Button>(_closeButtonPath).Pressed += Close;
        Visible = false;

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.MissionFinished += OnMissionFinished;
            MissionManager.Instance.MissionAborted += OnMissionFinished;
        }

        // On a new campaign the briefing goes first and opens the board itself.
        // A save resumed mid-mission goes straight back to its map instead.
        if (_openOnStart && !GameIntro.Pending && MissionManager.Instance?.ActiveMission == null)
        {
            // Deferred so the rest of the shell (player, HUD, hub) is ready first.
            CallDeferred(MethodName.Open);
        }
    }

    /// <summary>The player is back at base — offer the next mission straight away.</summary>
    private void OnMissionFinished(string missionId) => CallDeferred(MethodName.Open);

    public void Open()
    {
        BuildRows();
        Visible = true;
        GetTree().Paused = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
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

        if (_rowScene == null)
        {
            GD.PushWarning("MissionBoardPanel: no row scene assigned.");
            return;
        }

        bool nextMarked = false;
        foreach (MissionData mission in _missions)
        {
            if (mission == null)
            {
                continue;
            }

            bool done = MissionsPanel.IsMissionComplete(mission);
            bool next = !done && !nextMarked;
            nextMarked |= next;

            var row = _rowScene.Instantiate<MissionBoardRow>();
            _rows.AddChild(row);
            row.Bind(mission, done, next);
            row.StartPressed += OnStartPressed;
        }
    }

    private void OnStartPressed(MissionData mission)
    {
        Close();
        MissionManager.Instance?.StartMission(mission);
    }

    private void Close()
    {
        Visible = false;
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }
}
