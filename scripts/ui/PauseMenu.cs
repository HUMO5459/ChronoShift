using Godot;

namespace ChronoShift;

/// <summary>In-game pause overlay: freezes the tree and offers resume/save/load/controls/main-menu/quit.</summary>
public partial class PauseMenu : CanvasLayer
{
    [Export] private NodePath _menuPanelPath = "MenuPanel";
    [Export] private NodePath _controlsPanelPath = "ControlsPanel";
    [Export] private NodePath _savesPanelPath = "SavesPanel";
    [Export] private NodePath _resumeButtonPath = "MenuPanel/Center/Panel/VBox/Resume";
    [Export] private NodePath _saveButtonPath = "MenuPanel/Center/Panel/VBox/Save";
    [Export] private NodePath _loadButtonPath = "MenuPanel/Center/Panel/VBox/Load";
    [Export] private NodePath _controlsButtonPath = "MenuPanel/Center/Panel/VBox/Controls";
    [Export] private NodePath _abortButtonPath = "MenuPanel/Center/Panel/VBox/Abort";
    [Export] private NodePath _mainMenuButtonPath = "MenuPanel/Center/Panel/VBox/MainMenu";
    [Export] private NodePath _quitButtonPath = "MenuPanel/Center/Panel/VBox/Quit";
    [Export] private NodePath _statsPath = "MenuPanel/Center/Panel/VBox/Stats";

    private Control _menuPanel = null!;
    private ControlsPanel _controlsPanel = null!;
    private SavesPanel _savesPanel = null!;
    private Label _stats = null!;
    private Button _abortButton = null!;

    public override void _Ready()
    {
        Visible = false;

        _menuPanel = GetNode<Control>(_menuPanelPath);
        _controlsPanel = GetNode<ControlsPanel>(_controlsPanelPath);
        _savesPanel = GetNode<SavesPanel>(_savesPanelPath);
        _stats = GetNode<Label>(_statsPath);

        _controlsPanel.Closed += OnSubPanelClosed;
        _savesPanel.Closed += OnSubPanelClosed;
        _savesPanel.SaveLoaded += OnSaveLoaded;

        GetNode<Button>(_resumeButtonPath).Pressed += OnResumePressed;
        GetNode<Button>(_saveButtonPath).Pressed += () => OpenSaves(saveMode: true);
        GetNode<Button>(_loadButtonPath).Pressed += () => OpenSaves(saveMode: false);
        GetNode<Button>(_controlsButtonPath).Pressed += OnControlsPressed;

        _abortButton = GetNode<Button>(_abortButtonPath);
        _abortButton.Pressed += OnAbortPressed;
        GetNode<Button>(_mainMenuButtonPath).Pressed += OnMainMenuPressed;
        GetNode<Button>(_quitButtonPath).Pressed += OnQuitPressed;
    }

    /// <summary>
    /// Escape is handled here rather than polled: the save book consumes its own
    /// Escape first, so one press never closes both it and the pause menu.
    /// </summary>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!@event.IsActionPressed("ui_cancel"))
        {
            return;
        }

        if (_controlsPanel.Visible)
        {
            _controlsPanel.Visible = false;
            _menuPanel.Visible = true;
        }
        else
        {
            Toggle();
        }

        GetViewport().SetInputAsHandled();
    }

    private void Toggle()
    {
        bool open = !Visible;
        Visible = open;
        GetTree().Paused = open;
        Input.MouseMode = open ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
        if (open)
        {
            _menuPanel.Visible = true;
            _controlsPanel.Visible = false;
            _savesPanel.Visible = false;

            // Abandoning only means something while a mission map is loaded.
            _abortButton.Visible = MissionManager.Instance?.ActiveMission != null;

            WriteStats();
        }
    }

    /// <summary>The footer restates the run at a glance, as the design's pause card does.</summary>
    private void WriteStats()
    {
        int money = EconomyManager.Instance != null ? EconomyManager.Instance.Money : 0;
        int level = ProgressionManager.Instance != null ? ProgressionManager.Instance.Level : 1;

        string date = "—";
        string era = "—";
        if (TimeManager.Instance != null)
        {
            date = GameCalendar.FormatDate(TimeManager.Instance.CurrentYear, TimeManager.Instance.DayOfYear);
            era = $"{TimeManager.Instance.CurrentYear}-YIL";
        }

        _stats.Text = $"₳ {money}     DARAJA {level}     {date}     {era}";
    }

    private void OnResumePressed() => Toggle();

    private void OpenSaves(bool saveMode)
    {
        _menuPanel.Visible = false;
        _savesPanel.Open(saveMode);
    }

    private void OnSaveLoaded()
    {
        // State is already restored; drop back into the world with the loaded run.
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }

    private void OnControlsPressed()
    {
        _menuPanel.Visible = false;
        _controlsPanel.Visible = true;
    }

    /// <summary>Leaves the mission map for the hub. No bonus — the work was not finished.</summary>
    private void OnAbortPressed()
    {
        Toggle(); // close the pause menu first, so the hub comes back unpaused
        MissionManager.Instance?.AbortMission();
    }

    private void OnSubPanelClosed() => _menuPanel.Visible = true;

    /// <summary>
    /// Leaving for the menu is a quit as far as the run is concerned, so it writes
    /// first. Without this the whole session since the last milestone was dropped,
    /// and the save book then offered the stale file back.
    /// </summary>
    private void OnMainMenuPressed()
    {
        SaveManager.Instance?.Save();
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://scenes/ui/MainMenu.tscn");
    }

    private void OnQuitPressed()
    {
        SaveManager.Instance?.Save();
        GetTree().Quit();
    }
}
