using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>Boot scene: top-level navigation into gameplay, the read-only panels, and the aircraft demo.</summary>
public partial class MainMenu : Control
{
    [Export] private NodePath _menuPath = "Left";
    [Export] private NodePath _newGamePath = "Left/Menu/NewGame";
    [Export] private NodePath _loadPath = "Left/Menu/Load";
    [Export] private NodePath _loadHintPath = "Left/Menu/Load/Hint";
    [Export] private NodePath _missionsPath = "Left/Menu/Missions";
    [Export] private NodePath _heroPath = "Left/Menu/Hero";
    [Export] private NodePath _vehiclePath = "Left/Menu/Vehicle";
    [Export] private NodePath _controlsPath = "Left/Menu/Controls";
    [Export] private NodePath _aircraftDemoPath = "Left/Menu/AircraftDemo";
    [Export] private NodePath _quitPath = "Left/Menu/Quit";
    [Export] private NodePath _controlsPanelPath = "ControlsPanel";
    [Export] private NodePath _missionsPanelPath = "MissionsPanel";
    [Export] private NodePath _savesPanelPath = "SavesPanel";
    [Export] private NodePath _heroPanelPath = "HeroSelectPanel";
    [Export] private NodePath _vehiclePanelPath = "VehicleSelectPanel";

    private const string GameplayScene = "res://scenes/main/Main.tscn";

    private Control _menu = null!;
    private ControlsPanel _controlsPanel = null!;
    private MissionsPanel _missionsPanel = null!;
    private SavesPanel _savesPanel = null!;
    private HeroSelectPanel _heroPanel = null!;
    private VehicleSelectPanel _vehiclePanel = null!;

    public override void _Ready()
    {
        // This is a menu, not gameplay: keep the cursor free.
        Input.MouseMode = Input.MouseModeEnum.Visible;

        _menu = GetNode<Control>(_menuPath);
        _controlsPanel = GetNode<ControlsPanel>(_controlsPanelPath);
        _missionsPanel = GetNode<MissionsPanel>(_missionsPanelPath);
        _savesPanel = GetNode<SavesPanel>(_savesPanelPath);
        _heroPanel = GetNode<HeroSelectPanel>(_heroPanelPath);
        _vehiclePanel = GetNode<VehicleSelectPanel>(_vehiclePanelPath);

        _controlsPanel.Closed += OnPanelClosed;
        _missionsPanel.Closed += OnPanelClosed;
        _savesPanel.Closed += OnPanelClosed;
        _savesPanel.SaveLoaded += OnSaveLoaded;
        _heroPanel.Closed += OnPanelClosed;
        _vehiclePanel.Closed += OnPanelClosed;

        GetNode<Button>(_newGamePath).Pressed += OnNewGamePressed;
        GetNode<Button>(_missionsPath).Pressed += OnMissionsPressed;
        GetNode<Button>(_heroPath).Pressed += OnHeroPressed;
        GetNode<Button>(_vehiclePath).Pressed += OnVehiclePressed;
        GetNode<Button>(_controlsPath).Pressed += () => ShowPanel(_controlsPanel);
        GetNode<Button>(_quitPath).Pressed += OnQuitPressed;

        SetUpLoadButton();
        SetUpAircraftDemoButton();
    }

    /// <summary>The save book replaces a bare Continue: it shows what is on the slot before loading it.</summary>
    private void SetUpLoadButton()
    {
        bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasSave();

        var loadButton = GetNode<Button>(_loadPath);
        loadButton.Pressed += OnLoadPressed;
        loadButton.Disabled = !hasSave;

        GetNode<Label>(_loadHintPath).Text = hasSave ? "1 SLOT" : "BO'SH";
    }

    private void SetUpAircraftDemoButton()
    {
        var aircraftButton = GetNode<Button>(_aircraftDemoPath);
        if (GameDemoLauncher.IsAvailable())
        {
            aircraftButton.Pressed += OnAircraftDemoPressed;
        }
        else
        {
            aircraftButton.Disabled = true;
            aircraftButton.Text = "AVIA DEMO — O'RNATILMAGAN";
        }
    }

    private void OnNewGamePressed()
    {
        EconomyManager.Instance?.LoadState(0);
        ProgressionManager.Instance?.LoadState(0, 1);
        MissionManager.Instance?.LoadState(0);
        TechTreeManager.Instance?.LoadState(new List<string>());
        TimeManager.Instance?.LoadState(0);
        InventoryManager.Instance?.LoadState(null);

        // A fresh campaign opens with the briefing; a loaded one resumes silently.
        GameIntro.Pending = true;
        GetTree().ChangeSceneToFile(GameplayScene);
    }

    /// <summary>Rebuild first: the stamps reflect whatever campaign state is loaded right now.</summary>
    private void OnMissionsPressed()
    {
        _missionsPanel.Refresh();
        ShowPanel(_missionsPanel);
    }

    private void OnHeroPressed()
    {
        _menu.Visible = false;
        _heroPanel.Open();
    }

    private void OnVehiclePressed()
    {
        _menu.Visible = false;
        _vehiclePanel.Open();
    }

    private void OnLoadPressed()
    {
        _menu.Visible = false;
        _savesPanel.Open(saveMode: false);
    }

    private void OnSaveLoaded()
    {
        GameIntro.Pending = false;
        GetTree().ChangeSceneToFile(GameplayScene);
    }

    private void OnAircraftDemoPressed() => GameDemoLauncher.Launch();

    private void OnQuitPressed() => GetTree().Quit();

    private void ShowPanel(Control panel)
    {
        _menu.Visible = false;
        panel.Visible = true;
    }

    private void OnPanelClosed() => _menu.Visible = true;
}
