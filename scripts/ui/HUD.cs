using Godot;

namespace ChronoShift;

/// <summary>
/// In-game overlay from the UI design: money, level/XP, the accelerating clock,
/// the nearest-task tracker, the interact prompt, task progress, and reward toasts.
/// </summary>
public partial class HUD : CanvasLayer
{
    /// <summary>World objects raise toasts through this group — see <see cref="HudNotifier"/>.</summary>
    public const string HudGroup = "hud";

    [Export] private NodePath _moneyLabelPath = "TopLeft/Money/Row/Amount";

    /// Sounded whenever a toast appears - task done, material gathered.
    [Export] private AudioStream? _toastSound;
    [Export] private NodePath _levelLabelPath = "TopLeft/Xp/Rows/Head/Level";
    [Export] private NodePath _xpLabelPath = "TopLeft/Xp/Rows/Head/Xp";
    [Export] private NodePath _xpBarPath = "TopLeft/Xp/Rows/Bar/Fill";
    [Export] private NodePath _dateLabelPath = "TopRight/Date/Row/Date";
    [Export] private NodePath _clockLabelPath = "TopRight/Date/Row/Clock";
    [Export] private NodePath _timeScaleLabelPath = "TopRight/Time/Row/Scale";
    [Export] private NodePath _timeChevronPath = "TopRight/Time/Row/Chevron";
    [Export] private NodePath _trackerPath = "Tracker";
    [Export] private NodePath _trackerNamePath = "Tracker/Rows/Name";
    [Export] private NodePath _trackerMetaPath = "Tracker/Rows/Meta";
    [Export] private NodePath _promptPath = "Prompt";
    [Export] private NodePath _promptTitlePath = "Prompt/Row/Text/Title";
    [Export] private NodePath _promptSubPath = "Prompt/Row/Text/Sub";
    [Export] private NodePath _taskPanelPath = "TaskProgress";
    [Export] private NodePath _taskNamePath = "TaskProgress/Rows/Head/Name";
    [Export] private NodePath _taskPercentPath = "TaskProgress/Rows/Head/Percent";
    [Export] private NodePath _taskBarPath = "TaskProgress/Rows/Bar/Fill";
    [Export] private NodePath _reloadPanelPath = "Reload";
    [Export] private NodePath _reloadStatePath = "Reload/Rows/Head/State";
    [Export] private NodePath _reloadBarPath = "Reload/Rows/Bar/Fill";
    [Export] private NodePath _healthBarPath = "TopLeft/Health/Rows/Bar/Fill";
    [Export] private NodePath _healthValuePath = "TopLeft/Health/Rows/Head/Value";
    [Export] private NodePath _staminaBarPath = "TopLeft/Stamina/Rows/Bar/Fill";
    [Export] private NodePath _staminaValuePath = "TopLeft/Stamina/Rows/Head/Value";
    [Export] private NodePath _resourcesPath = "Resources";
    [Export] private NodePath _resourceListPath = "Resources/Rows/List";
    [Export] private NodePath _quickSlotsPath = "QuickSlots";
    [Export] private NodePath _scopePath = "Scope";
    [Export] private NodePath _crosshairPath = "Crosshair";
    [Export] private NodePath _vehiclePanelPath = "Vehicle";
    [Export] private NodePath _vehicleNamePath = "Vehicle/Rows/Head/Name";
    [Export] private NodePath _vehicleValuePath = "Vehicle/Rows/Head/Value";
    [Export] private NodePath _vehicleBarPath = "Vehicle/Rows/Bar/Fill";
    [Export] private NodePath _missionPanelPath = "TopRight/Mission";
    [Export] private NodePath _missionNamePath = "TopRight/Mission/Row/Name";
    [Export] private NodePath _missionCountPath = "TopRight/Mission/Row/Count";
    [Export] private NodePath _toastsPath = "Toasts";
    [Export] private NodePath _surgePath = "Surge";
    [Export] private NodePath _surgeScalePath = "Surge/Rows/Scale";
    [Export] private NodePath _movementHintPath = "BottomLeft";
    [Export] private NodePath _techKeyPath = "Hints/Tech/Key/Label";
    [Export] private NodePath _pauseKeyPath = "Hints/Pause/Key/Label";

    [Export] private PackedScene _toastScene = null!;

    [Export] private float _surgeSeconds = 1.5f;

    /// <summary>How many reward/pickup toasts may stack before the oldest is dropped.</summary>
    [Export] private int _maxToasts = 4;

    private Label _moneyLabel = null!;
    private Label _levelLabel = null!;
    private Label _xpLabel = null!;
    private Control _xpBar = null!;
    private Label _dateLabel = null!;
    private Label _clockLabel = null!;
    private Label _timeScaleLabel = null!;
    private Label _timeChevron = null!;
    private Control _tracker = null!;
    private Label _trackerName = null!;
    private Label _trackerMeta = null!;
    private Control _prompt = null!;
    private Label _promptTitle = null!;
    private Label _promptSub = null!;
    private Control _taskPanel = null!;
    private Label _taskName = null!;
    private Label _taskPercent = null!;
    private Control _taskBar = null!;
    private Control _reloadPanel = null!;
    private Label _reloadState = null!;
    private Control _reloadBar = null!;
    private Control _healthBar = null!;
    private Label _healthValue = null!;
    private Control _staminaBar = null!;
    private Label _staminaValue = null!;
    private Control _resourcesPanel = null!;
    private BoxContainer _resourceList = null!;
    private BoxContainer _quickSlots = null!;
    private Control _scope = null!;
    private Control _crosshair = null!;
    private Control _missionPanel = null!;
    private Label _missionName = null!;
    private Label _missionCount = null!;
    private Control _vehiclePanel = null!;
    private Label _vehicleName = null!;
    private Label _vehicleValue = null!;
    private Control _vehicleBar = null!;
    private DamageIndicator _damageIndicator = null!;
    private ObjectiveMarker _objectiveMarker = null!;
    private WeaponController? _weapon;
    private readonly System.Collections.Generic.List<(PanelContainer Chip, Label Name, Label Ammo)> _slots = new();
    private PlayerHealth? _playerHealth;
    private PlayerController? _player;
    private BoxContainer _toasts = null!;
    private Control _surge = null!;
    private Label _surgeScale = null!;

    private float _surgeRemaining;

    public override void _Ready()
    {
        _moneyLabel = GetNode<Label>(_moneyLabelPath);
        _levelLabel = GetNode<Label>(_levelLabelPath);
        _xpLabel = GetNode<Label>(_xpLabelPath);
        _xpBar = GetNode<Control>(_xpBarPath);
        _dateLabel = GetNode<Label>(_dateLabelPath);
        _clockLabel = GetNode<Label>(_clockLabelPath);
        _timeScaleLabel = GetNode<Label>(_timeScaleLabelPath);
        _timeChevron = GetNode<Label>(_timeChevronPath);
        _tracker = GetNode<Control>(_trackerPath);
        _trackerName = GetNode<Label>(_trackerNamePath);
        _trackerMeta = GetNode<Label>(_trackerMetaPath);
        _prompt = GetNode<Control>(_promptPath);
        _promptTitle = GetNode<Label>(_promptTitlePath);
        _promptSub = GetNode<Label>(_promptSubPath);
        _taskPanel = GetNode<Control>(_taskPanelPath);
        _taskName = GetNode<Label>(_taskNamePath);
        _taskPercent = GetNode<Label>(_taskPercentPath);
        _taskBar = GetNode<Control>(_taskBarPath);
        _reloadPanel = GetNode<Control>(_reloadPanelPath);
        _reloadState = GetNode<Label>(_reloadStatePath);
        _reloadBar = GetNode<Control>(_reloadBarPath);
        _healthBar = GetNode<Control>(_healthBarPath);
        _healthValue = GetNode<Label>(_healthValuePath);
        _staminaBar = GetNode<Control>(_staminaBarPath);
        _staminaValue = GetNode<Label>(_staminaValuePath);
        _resourcesPanel = GetNode<Control>(_resourcesPath);
        _resourceList = GetNode<BoxContainer>(_resourceListPath);
        _quickSlots = GetNode<BoxContainer>(_quickSlotsPath);
        _scope = GetNode<Control>(_scopePath);
        _crosshair = GetNode<Control>(_crosshairPath);
        _missionPanel = GetNode<Control>(_missionPanelPath);
        _missionName = GetNode<Label>(_missionNamePath);
        _missionCount = GetNode<Label>(_missionCountPath);
        _vehiclePanel = GetNode<Control>(_vehiclePanelPath);
        _vehicleName = GetNode<Label>(_vehicleNamePath);
        _vehicleValue = GetNode<Label>(_vehicleValuePath);
        _vehicleBar = GetNode<Control>(_vehicleBarPath);
        _toasts = GetNode<BoxContainer>(_toastsPath);
        _surge = GetNode<Control>(_surgePath);
        _surgeScale = GetNode<Label>(_surgeScalePath);

        _tracker.Visible = false;
        _prompt.Visible = false;
        _taskPanel.Visible = false;
        _reloadPanel.Visible = false;
        _vehiclePanel.Visible = false;
        _surge.Visible = false;

        AddToGroup(HudGroup);

        // Added here rather than in the scene: both draw themselves, and their order
        // on this layer is what decides what covers what — the objective pointer sits
        // under the panels' information, the damage wash above everything.
        _objectiveMarker = new ObjectiveMarker { Name = "ObjectiveMarker" };
        AddChild(_objectiveMarker);

        _damageIndicator = new DamageIndicator { Name = "DamageIndicator" };
        AddChild(_damageIndicator);

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.MoneyChanged += OnMoneyChanged;
            OnMoneyChanged(EconomyManager.Instance.Money);
        }
        else
        {
            GD.PushWarning("HUD: EconomyManager autoload not found.");
        }

        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.XpChanged += OnXpChanged;
            OnXpChanged(ProgressionManager.Instance.Xp, ProgressionManager.Instance.Level);
        }
        else
        {
            GD.PushWarning("HUD: ProgressionManager autoload not found.");
        }

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.MissionCompleted += OnMissionCompleted;
        }
        else
        {
            GD.PushWarning("HUD: MissionManager autoload not found.");
        }

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.TimeJumped += OnTimeJumped;
        }
        else
        {
            GD.PushWarning("HUD: TimeManager autoload not found.");
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.InventoryChanged += RebuildResourceStrip;
            InventoryManager.Instance.ItemGained += OnItemGained;
            RebuildResourceStrip();
        }
        else
        {
            GD.PushWarning("HUD: InventoryManager autoload not found.");
            _resourcesPanel.Visible = false;
        }

        // The chevron belonged to the old acceleration badge; the era plate has no use for it.
        _timeChevron.Visible = false;

        WriteKeyHints();
    }

    /// <summary>The left-hand material strip: every resource the campaign defines, with its count.</summary>
    private void RebuildResourceStrip()
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }

        foreach (Node child in _resourceList.GetChildren())
        {
            _resourceList.RemoveChild(child);
            child.QueueFree();
        }

        System.Collections.Generic.List<(ItemData Item, int Count)> resources =
            InventoryManager.Instance.AllResources();

        _resourcesPanel.Visible = resources.Count > 0;

        foreach ((ItemData item, int count) in resources)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 9);

            var chip = new ColorRect
            {
                Color = item.Tint,
                CustomMinimumSize = new Vector2(10, 10),
                SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            };
            row.AddChild(chip);

            var name = new Label
            {
                Text = string.IsNullOrEmpty(item.ShortLabel) ? item.DisplayName : item.ShortLabel,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            };
            name.ThemeTypeVariation = "HudCaption";
            name.AddThemeFontSizeOverride("font_size", 12);
            row.AddChild(name);

            var amount = new Label { Text = count.ToString() };
            amount.ThemeTypeVariation = "MonoValue";
            amount.AddThemeFontSizeOverride("font_size", 14);
            amount.AddThemeColorOverride("font_color", count > 0 ? UiPalette.HudText : UiPalette.HudText.With(0.35f));
            row.AddChild(amount);

            _resourceList.AddChild(row);
        }
    }

    /// <summary>Key hints are read from the Input Map, so a rebind never leaves the HUD lying.</summary>
    private void WriteKeyHints()
    {
        GetNode<Label>(_movementHintPath).Text =
            $"[{KeyLabel("move_forward")} {KeyLabel("move_left")} {KeyLabel("move_back")} {KeyLabel("move_right")}]"
            + $" yurish · [{KeyLabel("run")}] yugurish · [{KeyLabel("interact")}] ishlash"
            + $" · [{KeyLabel("inventory")}] sumka"
            + $" · [{KeyLabel("journal")}] vazifalar"
            + $" · [{KeyLabel("stats")}] xarakteristika"
            + $" · [{KeyLabel("designer")}] konstruktor"
            + $" · [{KeyLabel("camera_toggle")}] ko'rinish";

        GetNode<Label>(_techKeyPath).Text = KeyLabel("tech_tree");
        GetNode<Label>(_pauseKeyPath).Text = KeyLabel("ui_cancel");
    }

    /// <summary>First keyboard key bound to <paramref name="action"/>, as the player's layout labels it.</summary>
    private static string KeyLabel(string action)
    {
        if (!InputMap.HasAction(action))
        {
            return "—";
        }

        foreach (InputEvent inputEvent in InputMap.ActionGetEvents(action))
        {
            if (inputEvent is InputEventKey key)
            {
                Key keycode = key.PhysicalKeycode != Key.None
                    ? DisplayServer.KeyboardGetKeycodeFromPhysical(key.PhysicalKeycode)
                    : key.Keycode;
                return OS.GetKeycodeString(keycode);
            }
        }

        return "—";
    }

    public override void _Process(double delta)
    {
        UpdateClock();
        UpdateTaskWidgets();
        UpdateReload();
        UpdateHealth();
        UpdateStamina();
        UpdateQuickSlots();
        UpdateVehicle();
        UpdateMission();
        UpdateSurge((float)delta);
    }

    /// <summary>Hull integrity of the vehicle being driven; hidden on foot.</summary>
    private void UpdateVehicle()
    {
        Vehicle? driven = null;
        foreach (Node node in GetTree().GetNodesInGroup(Vehicle.VehicleGroup))
        {
            if (node is Vehicle { IsDriven: true } vehicle)
            {
                driven = vehicle;
                break;
            }
        }

        _vehiclePanel.Visible = driven != null;
        if (driven == null)
        {
            return;
        }

        float fraction = driven.HealthFraction;
        _vehicleName.Text = driven.Name.ToString().ToUpperInvariant();
        _vehicleValue.Text = $"{Mathf.CeilToInt(driven.Health)}/{Mathf.RoundToInt(driven.MaxHealth)}";
        _vehicleBar.AnchorRight = fraction;
        _vehicleValue.AddThemeColorOverride(
            "font_color",
            fraction <= 0.3f ? UiPalette.Warning : Colors.White.With(0.7f));
    }

    /// <summary>Player took a hit: wash the screen red and point back at the shooter.</summary>
    private void OnPlayerDamaged(float amount, Vector3 origin, bool hasSource)
    {
        _damageIndicator.Register(amount, _playerHealth?.MaxHealth ?? 100.0f, origin, hasSource);
    }

    /// <summary>Raises a toast on behalf of a world object — see <see cref="HudNotifier"/>.</summary>
    public void Notify(string title, string sub) => ShowToast(title, sub);

    /// <summary>Names the mission in progress and how much of its work is left.</summary>
    private void UpdateMission()
    {
        MissionData? mission = MissionManager.Instance?.ActiveMission;
        _missionPanel.Visible = mission != null;
        if (mission == null)
        {
            return;
        }

        _missionName.Text = mission.DisplayName;

        if (GetTree().GetFirstNodeInGroup(MissionMap.MapGroup) is MissionMap map && map.TaskCount > 0)
        {
            _missionCount.Text = $"{map.CompletedCount}/{map.TaskCount}";
            bool done = map.CompletedCount >= map.TaskCount;
            _missionCount.AddThemeColorOverride("font_color", done ? UiPalette.AccentLight : UiPalette.Accent);
        }
        else
        {
            _missionCount.Text = "—";
        }
    }

    /// <summary>
    /// The bottom strip of carried weapons. Chips are built once from the loadout
    /// and then only their ammo and highlight change, so this is cheap per frame.
    /// </summary>
    private void UpdateQuickSlots()
    {
        if (_weapon == null || !IsInstanceValid(_weapon))
        {
            _weapon = GetTree().GetFirstNodeInGroup("player_weapon") as WeaponController;
            _slots.Clear();
        }

        if (_weapon == null)
        {
            _quickSlots.Visible = false;
            return;
        }

        if (_slots.Count != _weapon.Loadout.Count)
        {
            BuildQuickSlots(_weapon);
        }

        // The scope replaces both the crosshair and the quick slots: a rifle optic
        // fills the screen and any HUD inside it reads as a bug.
        bool scoped = _weapon.IsScoped;
        _scope.Visible = scoped;
        _crosshair.Visible = !scoped;
        _quickSlots.Visible = _slots.Count > 0 && !scoped;
        if (scoped)
        {
            return;
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            bool selected = i == _weapon.SelectedIndex;
            (PanelContainer chip, Label name, Label ammo) = _slots[i];

            chip.Modulate = Colors.White.With(selected ? 1.0f : 0.45f);
            name.AddThemeColorOverride("font_color", selected ? UiPalette.Highlight : UiPalette.HudText.With(0.6f));
            ammo.Text = $"{_weapon.AmmoIn(i)}/{_weapon.Loadout[i]?.MagazineSize ?? 0}";
        }
    }

    private void BuildQuickSlots(WeaponController weapon)
    {
        foreach (Node child in _quickSlots.GetChildren())
        {
            _quickSlots.RemoveChild(child);
            child.QueueFree();
        }

        _slots.Clear();

        for (int i = 0; i < weapon.Loadout.Count; i++)
        {
            WeaponData? data = weapon.Loadout[i];
            var chip = new PanelContainer { ThemeTypeVariation = "HudPanel" };
            chip.CustomMinimumSize = new Vector2(158, 0);

            var rows = new VBoxContainer();
            rows.AddThemeConstantOverride("separation", 2);
            chip.AddChild(rows);

            var head = new HBoxContainer();
            head.AddThemeConstantOverride("separation", 8);
            rows.AddChild(head);

            var key = new Label { Text = (i + 1).ToString() };
            key.ThemeTypeVariation = "MonoValue";
            key.AddThemeFontSizeOverride("font_size", 13);
            key.AddThemeColorOverride("font_color", UiPalette.Accent);
            head.AddChild(key);

            var name = new Label
            {
                Text = data?.DisplayName ?? "—",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                // Longer weapon names were being cut mid-glyph by the fixed slot
                // width; an ellipsis reads as deliberate.
                TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
                ClipText = true,
            };
            name.ThemeTypeVariation = "HudCaption";
            name.AddThemeFontSizeOverride("font_size", 12);
            head.AddChild(name);

            var ammo = new Label { Text = "0/0" };
            ammo.ThemeTypeVariation = "Mono";
            ammo.AddThemeFontSizeOverride("font_size", 12);
            ammo.AddThemeColorOverride("font_color", Colors.White.With(0.6f));
            ammo.HorizontalAlignment = HorizontalAlignment.Right;
            rows.AddChild(ammo);

            _quickSlots.AddChild(chip);
            _slots.Add((chip, name, ammo));
        }
    }

    /// <summary>Polls the player's stamina (found via group) and drives the HUD bar.</summary>
    private void UpdateStamina()
    {
        if (_player == null || !IsInstanceValid(_player))
        {
            _player = GetTree().GetFirstNodeInGroup("player") as PlayerController;
        }

        if (_player == null)
        {
            return;
        }

        _staminaBar.AnchorRight = _player.StaminaFraction;
        _staminaValue.Text = $"{Mathf.CeilToInt(_player.Stamina)}/{Mathf.RoundToInt(_player.MaxStamina)}";
        _staminaValue.AddThemeColorOverride(
            "font_color",
            _player.IsWinded ? UiPalette.Warning : Colors.White.With(0.55f));
    }

    /// <summary>Polls the player's health (found via group) and drives the HUD bar.</summary>
    private void UpdateHealth()
    {
        if (_playerHealth == null || !IsInstanceValid(_playerHealth))
        {
            _playerHealth = GetTree().GetFirstNodeInGroup("player_health") as PlayerHealth;

            // Safe to connect unconditionally: this branch only runs when the old
            // reference was null or freed, so whatever it resolves to is new to us.
            if (_playerHealth != null)
            {
                _playerHealth.Damaged += OnPlayerDamaged;
            }
        }

        if (_playerHealth == null)
        {
            return;
        }

        _healthBar.AnchorRight = _playerHealth.Fraction;
        _healthValue.Text = $"{Mathf.CeilToInt(_playerHealth.Health)}/{Mathf.RoundToInt(_playerHealth.MaxHealth)}";
    }

    /// <summary>The clock has no change signal — it drifts every frame, so poll it.</summary>
    private void UpdateClock()
    {
        if (TimeManager.Instance == null)
        {
            return;
        }

        TimeManager time = TimeManager.Instance;
        _dateLabel.Text = GameCalendar.FormatDate(time.CurrentYear, time.DayOfYear);
        _clockLabel.Text = GameCalendar.FormatClock(time.ElapsedDays);
        _timeScaleLabel.Text = time.CurrentYear.ToString();
    }

    private void UpdateTaskWidgets()
    {
        EngineeringTask? active = FindActiveTask();
        _taskPanel.Visible = active != null;
        if (active != null)
        {
            _taskName.Text = active.InteractionPrompt;
            _taskPercent.Text = $"{active.Progress * 100.0f:0}%";
            _taskBar.AnchorRight = active.Progress;
        }

        Node3D? focused = FocusedInteractable();
        _prompt.Visible = focused != null && active == null;
        if (focused != null)
        {
            WritePrompt(focused);
        }

        UpdateTracker(active != null || focused is EngineeringTask);
    }

    /// <summary>
    /// One prompt for every interactable: a task states its work or the equipment
    /// it is still waiting on, a deposit states its yield, the workshop invites an
    /// order, and anything else just names itself.
    /// </summary>
    private void WritePrompt(Node3D focused)
    {
        Color subColor = Colors.White.With(0.6f);
        string title;
        string sub;

        switch (focused)
        {
            case EngineeringTask task when !task.RequirementMet:
                title = $"Uskuna kerak — {task.InteractionPrompt}";
                sub = task.RequirementLabel;
                subColor = UiPalette.Warning;
                break;

            case EngineeringTask task:
                title = $"Vazifani boshlash — {task.InteractionPrompt}";
                sub = $"~{task.DurationSeconds:0} S · JOYINGIZDA TURING";
                break;

            case ResourceNode deposit:
                title = deposit.InteractionPrompt;
                sub = deposit.YieldLabel;
                break;

            case Workshop workshop:
                title = $"{workshop.InteractionPrompt} — ishlab chiqarish";
                sub = "BUYURTMA TANLANG";
                break;

            case WeaponPickup pickup:
                title = $"Qurolni olish — {pickup.InteractionPrompt}";
                sub = pickup.DetailLabel;
                subColor = UiPalette.AccentLight;
                break;

            default:
                title = focused is IInteractable interactable ? interactable.InteractionPrompt : "";
                sub = "";
                break;
        }

        _promptTitle.Text = title;
        _promptSub.Text = sub;
        _promptSub.AddThemeColorOverride("font_color", subColor);
    }

    /// <summary>
    /// Names the objective the mission expects next — not merely the closest task,
    /// which pointed at the wrong one whenever the next objective was across the map.
    /// It yields only to the interact prompt and the progress panel, which say the
    /// same thing at closer range.
    /// </summary>
    private void UpdateTracker(bool suppressed)
    {
        if (suppressed)
        {
            _tracker.Visible = false;
            return;
        }

        Node3D? player = GetPlayer();
        EngineeringTask? objective = CurrentObjective();

        _tracker.Visible = objective != null && player != null;
        if (objective == null || player == null)
        {
            return;
        }

        float distance = objective.GlobalPosition.DistanceTo(player.GlobalPosition);
        string position = ObjectiveNumberLabel(objective);

        _trackerName.Text = objective.InteractionPrompt;
        _trackerMeta.Text = objective.RequirementMet
            ? $"{position}{distance:0} M · {objective.Kind}"
            : $"{position}{distance:0} M · USKUNA KERAK";
        _trackerMeta.AddThemeColorOverride(
            "font_color",
            objective.RequirementMet ? Colors.White.With(0.6f) : UiPalette.Warning);
    }

    /// <summary>"2/3 · " when the map knows its running order, empty otherwise.</summary>
    private string ObjectiveNumberLabel(EngineeringTask objective)
    {
        if (GetTree().GetFirstNodeInGroup(MissionMap.MapGroup) is not MissionMap map)
        {
            return "";
        }

        int number = map.NumberOf(objective);
        return number > 0 ? $"{number}/{map.TaskCount} · " : "";
    }

    /// <summary>
    /// The objective the mission is waiting on. Off-mission there is no running
    /// order, so the nearest outstanding task stands in.
    /// </summary>
    private EngineeringTask? CurrentObjective()
    {
        if (GetTree().GetFirstNodeInGroup(MissionMap.MapGroup) is MissionMap map)
        {
            return map.CurrentObjective;
        }

        Node3D? player = GetPlayer();
        if (player == null)
        {
            return null;
        }

        EngineeringTask? nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Node node in GetTree().GetNodesInGroup(EngineeringTask.TaskGroup))
        {
            if (node is not EngineeringTask { IsCompleted: false } task)
            {
                continue;
            }

            float distance = task.GlobalPosition.DistanceTo(player.GlobalPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = task;
            }
        }

        return nearest;
    }

    /// <summary>Shows the gun's reload only while the player is actually in a tank.</summary>
    private void UpdateReload()
    {
        TankCannon? cannon = null;
        foreach (Node node in GetTree().GetNodesInGroup(TankCannon.CannonGroup))
        {
            if (node is TankCannon { IsDriverAboard: true } aboard)
            {
                cannon = aboard;
                break;
            }
        }

        _reloadPanel.Visible = cannon != null;
        if (cannon == null)
        {
            return;
        }

        _reloadBar.AnchorRight = cannon.ReloadProgress;
        _reloadState.Text = cannon.IsLoaded ? "TAYYOR" : "O'QLANMOQDA";
        _reloadState.AddThemeColorOverride("font_color", cannon.IsLoaded ? UiPalette.AccentLight : UiPalette.Warning);
    }

    private EngineeringTask? FindActiveTask()
    {
        foreach (Node node in GetTree().GetNodesInGroup(EngineeringTask.TaskGroup))
        {
            if (node is EngineeringTask { IsActive: true } task)
            {
                return task;
            }
        }

        return null;
    }

    private Node3D? FocusedInteractable()
    {
        var interaction = GetTree().GetFirstNodeInGroup(PlayerInteraction.InteractionGroup) as PlayerInteraction;
        return interaction?.Focused;
    }

    private Node3D? GetPlayer()
    {
        var interaction = GetTree().GetFirstNodeInGroup(PlayerInteraction.InteractionGroup) as PlayerInteraction;
        return interaction?.GetParent<Node3D>();
    }

    private void UpdateSurge(float delta)
    {
        if (_surgeRemaining <= 0f)
        {
            return;
        }

        _surgeRemaining -= delta;
        if (_surgeRemaining <= 0f)
        {
            _surge.Visible = false;
            return;
        }

        // Fade the flash out over its lifetime.
        _surge.Modulate = Colors.White.With(Mathf.Clamp(_surgeRemaining / _surgeSeconds, 0f, 1f));
    }

    private void OnMoneyChanged(int money) => _moneyLabel.Text = money.ToString();

    private void OnXpChanged(int xp, int level)
    {
        _levelLabel.Text = $"DARAJA {level}";

        if (ProgressionManager.Instance != null)
        {
            _xpLabel.Text = $"{ProgressionManager.Instance.XpIntoLevel}/{ProgressionManager.Instance.XpPerLevel} XP";
            _xpBar.AnchorRight = ProgressionManager.Instance.LevelProgress;
        }
    }

    private void OnMissionCompleted(string taskId, int rewardMoney, int rewardXp)
    {
        ShowToast("VAZIFA BAJARILDI", $"+₳{rewardMoney} · +{rewardXp} XP");
    }

    /// <summary>A finished task or a new era moved the calendar — announce the new year.</summary>
    private void OnTimeJumped(int fromYear, int toYear, int days)
    {
        if (toYear == fromYear)
        {
            return; // A jump inside one year is not worth a full-screen flash.
        }

        _surgeScale.Text = toYear.ToString();
        _surge.Visible = true;
        _surge.Modulate = Colors.White;
        _surgeRemaining = _surgeSeconds;
    }

    private void OnItemGained(string itemId, int amount)
    {
        ItemData? item = InventoryManager.Instance?.Resolve(itemId);
        if (item == null)
        {
            return;
        }

        string heading = item.Category == ItemCategory.Resource ? "MATERIAL OLINDI" : "USKUNA TAYYOR";
        ShowToast(heading, $"{item.DisplayName} ×{amount}");
    }

    private void ShowToast(string text, string sub)
    {
        Sfx.PlayUi(this, _toastSound, -10.0f);

        if (_toastScene == null)
        {
            GD.PushWarning("HUD: no toast scene assigned.");
            return;
        }

        var toast = _toastScene.Instantiate<HudToast>();
        _toasts.AddChild(toast);
        toast.Display(text, sub);

        // Gathering fires one of these per harvest; without a cap a run through a
        // deposit field would paper the whole right edge of the screen.
        while (_toasts.GetChildCount() > _maxToasts)
        {
            Node oldest = _toasts.GetChild(0);
            _toasts.RemoveChild(oldest);
            oldest.QueueFree();
        }
    }
}
