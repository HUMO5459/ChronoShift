using Godot;

namespace ChronoShift;

/// <summary>
/// The mission's objective sheet, in two modes off one screen.
///
/// • <b>Briefing</b> — opens by itself the moment a mission starts, so taking a job
///   is an event the player reads rather than a map that silently swaps underneath
///   them. It pauses until they accept.
/// • <b>Journal</b> — the same sheet reopened at any time with the journal key, to
///   check what is left. The objective in progress is highlighted in both.
/// </summary>
public partial class MissionBriefingPanel : Control
{
    [Export] private NodePath _kickerPath = "Scrim/Center/Card/Rows/Kicker";
    [Export] private NodePath _titlePath = "Scrim/Center/Card/Rows/Title";
    [Export] private NodePath _metaPath = "Scrim/Center/Card/Rows/Meta";
    [Export] private NodePath _descriptionPath = "Scrim/Center/Card/Rows/Description";
    [Export] private NodePath _objectivesPath = "Scrim/Center/Card/Rows/Objectives";
    [Export] private NodePath _rewardPath = "Scrim/Center/Card/Rows/Reward";
    [Export] private NodePath _acceptPath = "Scrim/Center/Card/Rows/Accept";

    /// <summary>Input action that opens the journal; read from the Input Map, never hardcoded.</summary>
    [Export] private string _journalAction = "journal";

    private Label _kicker = null!;
    private Label _title = null!;
    private Label _meta = null!;
    private Label _description = null!;
    private VBoxContainer _objectives = null!;
    private Label _reward = null!;
    private Button _accept = null!;

    /// <summary>True while showing the opening briefing, false when reopened as a journal.</summary>
    private bool _briefing;

    public override void _Ready()
    {
        _kicker = GetNode<Label>(_kickerPath);
        _title = GetNode<Label>(_titlePath);
        _meta = GetNode<Label>(_metaPath);
        _description = GetNode<Label>(_descriptionPath);
        _objectives = GetNode<VBoxContainer>(_objectivesPath);
        _reward = GetNode<Label>(_rewardPath);
        _accept = GetNode<Button>(_acceptPath);

        _accept.Pressed += Close;
        Visible = false;

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.MissionStarted += OnMissionStarted;
        }
        else
        {
            GD.PushWarning("MissionBriefingPanel: MissionManager autoload not found.");
        }
    }

    /// <summary>
    /// Deferred: the map is instanced in the same call that raises this signal, and
    /// the sheet lists that map's objectives.
    /// </summary>
    private void OnMissionStarted(string missionId) => CallDeferred(MethodName.OpenBriefing);

    public void OpenBriefing()
    {
        _briefing = true;
        Open();
    }

    public void OpenJournal()
    {
        _briefing = false;
        Open();
    }

    private void Open()
    {
        if (MissionManager.Instance?.ActiveMission == null)
        {
            return; // In the hub there is nothing to brief; the board covers that.
        }

        Build();
        Visible = true;
        GetTree().Paused = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void Close()
    {
        Visible = false;
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Visible)
        {
            // Escape and the journal key both back out of the journal. The briefing
            // insists on its button instead — but still swallows Escape, or the pause
            // menu would stack itself on top of a screen that is already paused.
            if (!_briefing && (@event.IsActionPressed("ui_cancel") || IsJournalPressed(@event)))
            {
                Close();
                GetViewport().SetInputAsHandled();
            }
            else if (_briefing && @event.IsActionPressed("ui_cancel"))
            {
                GetViewport().SetInputAsHandled();
            }

            return;
        }

        if (IsJournalPressed(@event) && MissionManager.Instance?.ActiveMission != null)
        {
            OpenJournal();
            GetViewport().SetInputAsHandled();
        }
    }

    private bool IsJournalPressed(InputEvent @event) =>
        InputMap.HasAction(_journalAction) && @event.IsActionPressed(_journalAction);

    private void Build()
    {
        MissionData? mission = MissionManager.Instance?.ActiveMission;
        if (mission == null)
        {
            return;
        }

        _kicker.Text = _briefing ? "YANGI VAZIFA" : "VAZIFALAR RO'YXATI";
        _title.Text = mission.DisplayName;
        _accept.Text = _briefing ? "VAZIFANI BOSHLASH" : "YOPISH";

        string era = mission.EraYear > 0 ? mission.EraYear.ToString() : "";
        string kind = string.IsNullOrEmpty(mission.Kind) ? "" : mission.Kind;
        _meta.Text = string.Join("  ·  ", System.Array.FindAll(new[] { era, kind }, s => !string.IsNullOrEmpty(s)));

        _description.Text = mission.Description;
        _description.Visible = !string.IsNullOrEmpty(mission.Description);

        _reward.Text = $"MISSIYA BONUSI:  +₳{mission.BonusMoney}   ·   +{mission.BonusXp} XP";
        _reward.Visible = mission.BonusMoney > 0 || mission.BonusXp > 0;

        BuildObjectives(mission);
    }

    /// <summary>
    /// Lists the map's objectives in order. Live task nodes are preferred because
    /// they carry progress; the mission's map data is the fallback for the briefing,
    /// which can run a frame before the world finishes building itself.
    /// </summary>
    private void BuildObjectives(MissionData mission)
    {
        foreach (Node child in _objectives.GetChildren())
        {
            _objectives.RemoveChild(child);
            child.QueueFree();
        }

        var map = GetTree().GetFirstNodeInGroup(MissionMap.MapGroup) as MissionMap;
        EngineeringTask? current = map?.CurrentObjective;

        if (map != null && map.TaskCount > 0)
        {
            for (int i = 0; i < map.Tasks.Count; i++)
            {
                EngineeringTask task = map.Tasks[i];
                AddRow(
                    i + 1,
                    task.InteractionPrompt,
                    task.Kind,
                    task.IsCompleted ? Status.Done : task == current ? Status.Current : Status.Pending,
                    task.RequirementMet ? "" : task.RequirementLabel);
            }

            return;
        }

        EngineeringTaskData[] tasks = mission.Map?.Tasks ?? System.Array.Empty<EngineeringTaskData>();
        for (int i = 0; i < tasks.Length; i++)
        {
            EngineeringTaskData? data = tasks[i];
            if (data == null)
            {
                continue;
            }

            AddRow(i + 1, data.DisplayName, data.Kind, i == 0 ? Status.Current : Status.Pending, "");
        }
    }

    private enum Status
    {
        Done,
        Current,
        Pending,
    }

    /// <summary>
    /// One objective line: number, name, and where it stands. The current objective
    /// is the only one drawn at full strength, so the eye lands on it first.
    /// </summary>
    private void AddRow(int index, string name, string kind, Status status, string requirement)
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat
        {
            BgColor = status == Status.Current ? UiPalette.Accent.With(0.16f) : Colors.White.With(0.04f),
            ContentMarginLeft = 14,
            ContentMarginRight = 14,
            ContentMarginTop = 10,
            ContentMarginBottom = 10,
        };

        if (status == Status.Current)
        {
            style.BorderWidthLeft = 3;
            style.BorderColor = UiPalette.Highlight;
        }

        panel.AddThemeStyleboxOverride("panel", style);

        var rows = new VBoxContainer();
        rows.AddThemeConstantOverride("separation", 3);
        panel.AddChild(rows);

        var head = new HBoxContainer();
        head.AddThemeConstantOverride("separation", 12);
        rows.AddChild(head);

        var number = new Label { Text = index.ToString("00") };
        number.ThemeTypeVariation = "MonoValue";
        number.AddThemeFontSizeOverride("font_size", 14);
        number.AddThemeColorOverride(
            "font_color",
            status == Status.Current ? UiPalette.Highlight : Colors.White.With(0.4f));
        head.AddChild(number);

        var label = new Label
        {
            Text = name,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        label.ThemeTypeVariation = "Body";
        label.AddThemeFontSizeOverride("font_size", 16);
        label.AddThemeColorOverride(
            "font_color",
            status == Status.Done ? Colors.White.With(0.38f) : Colors.White.With(status == Status.Current ? 1.0f : 0.72f));
        head.AddChild(label);

        var state = new Label
        {
            Text = status switch
            {
                Status.Done => "BAJARILDI",
                Status.Current => "JORIY",
                _ => string.IsNullOrEmpty(kind) ? "KUTILMOQDA" : kind,
            },
        };
        state.ThemeTypeVariation = "MonoLabel";
        state.AddThemeFontSizeOverride("font_size", 12);
        state.AddThemeColorOverride(
            "font_color",
            status switch
            {
                Status.Done => UiPalette.AccentLight,
                Status.Current => UiPalette.Highlight,
                _ => Colors.White.With(0.4f),
            });
        head.AddChild(state);

        // Only shown when the objective is blocked on equipment the workshop owes it.
        if (!string.IsNullOrEmpty(requirement))
        {
            var note = new Label { Text = requirement };
            note.ThemeTypeVariation = "MonoLabel";
            note.AddThemeFontSizeOverride("font_size", 12);
            note.AddThemeColorOverride("font_color", UiPalette.Warning);
            rows.AddChild(note);
        }

        _objectives.AddChild(panel);
    }
}
