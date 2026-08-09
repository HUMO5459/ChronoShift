using Godot;

namespace ChronoShift;

/// <summary>Interactable workstation that runs a timed engineering task with color feedback.</summary>
public partial class EngineeringTask : StaticBody3D, IInteractable
{
    [Export] private EngineeringTaskData _taskData = null!;
    [Export] private NodePath _meshPath = "MeshInstance3D";
    [Export] private Color _idleColor = new Color(0.85f, 0.5f, 0.2f);
    [Export] private Color _workingColor = new Color(0.95f, 0.9f, 0.25f);
    [Export] private Color _doneColor = new Color(0.3f, 0.75f, 0.35f);
    [Export] private float _focusEmissionEnergy = 0.5f;

    [Signal] public delegate void TaskStartedEventHandler(string taskId);
    [Signal] public delegate void TaskCompletedEventHandler(string taskId);

    private MeshInstance3D _mesh = null!;
    private StandardMaterial3D _material = null!;
    private bool _active;
    private bool _completed;
    private bool _focused;
    private float _elapsed;

    /// <summary>Every task registers here so the HUD can track the nearest one without a direct wire.</summary>
    public const string TaskGroup = "engineering_tasks";

    private float Duration => _taskData != null ? _taskData.DurationSeconds : 3f;
    private string TaskId => _taskData != null ? _taskData.TaskId : "unknown";

    public string InteractionPrompt => _taskData != null ? _taskData.DisplayName : "Work";

    /// <summary>Work category for the HUD tracker, e.g. "TA'MIRLASH".</summary>
    public string Kind => _taskData != null ? _taskData.Kind : "";

    /// <summary>Seconds of work this task takes — the HUD shows it as an ETA.</summary>
    public float DurationSeconds => Duration;

    public bool IsActive => _active;
    public bool IsCompleted => _completed;

    /// <summary>Equipment this task needs the workshop to have produced, or null.</summary>
    public ItemData? RequiredItem => _taskData?.RequiredItem;

    /// <summary>How many of <see cref="RequiredItem"/> the task needs.</summary>
    public int RequiredAmount => _taskData != null ? _taskData.RequiredAmount : 0;

    /// <summary>False only while a required piece of equipment is still missing.</summary>
    public bool RequirementMet =>
        RequiredItem == null
        || InventoryManager.Instance == null
        || InventoryManager.Instance.Has(RequiredItem, RequiredAmount);

    /// <summary>What the HUD prints under the prompt, e.g. "KERAK: Ponton to'plami ×1 (0/1)".</summary>
    public string RequirementLabel
    {
        get
        {
            if (RequiredItem == null)
            {
                return "";
            }

            int held = InventoryManager.Instance?.Count(RequiredItem) ?? 0;
            return $"KERAK: {RequiredItem.DisplayName} ×{RequiredAmount} ({held}/{RequiredAmount})";
        }
    }

    /// <summary>Work done so far, 0..1.</summary>
    public float Progress => Mathf.Clamp(_elapsed / Duration, 0f, 1f);

    public override void _Ready()
    {
        AddToGroup(TaskGroup);

        if (_taskData == null)
        {
            GD.PushWarning($"EngineeringTask '{Name}' has no EngineeringTaskData assigned.");
        }

        _mesh = GetNode<MeshInstance3D>(_meshPath);
        _material = new StandardMaterial3D
        {
            AlbedoColor = _idleColor,
            EmissionEnabled = true,
            Emission = _idleColor,
            EmissionEnergyMultiplier = 0f,
        };
        _mesh.SetSurfaceOverrideMaterial(0, _material);

        // Resuming a saved mission: work already done stays done. It also closes the
        // hole where reloading re-opened finished tasks and paid their reward again.
        if (MissionManager.Instance?.IsTaskDoneThisRun(TaskId) == true)
        {
            _completed = true;
            _material.AlbedoColor = _doneColor;
            _material.Emission = _doneColor;
        }
    }

    public bool CanInteract(Node3D interactor)
    {
        return _taskData != null && !_active && !_completed;
    }

    public void Interact(Node3D interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        // The task stays focusable while the equipment is missing so the HUD can
        // say what to go and produce; it just refuses to start.
        if (!RequirementMet)
        {
            return;
        }

        if (_taskData is { RequiredItem: not null, ConsumesRequirement: true }
            && InventoryManager.Instance?.TryConsume(_taskData.RequiredItem, _taskData.RequiredAmount) == false)
        {
            return;
        }

        _active = true;
        _elapsed = 0f;
        EmitSignal(SignalName.TaskStarted, TaskId);
    }

    public void SetFocused(bool focused)
    {
        _focused = focused;
        _material.EmissionEnergyMultiplier = (_focused && CanInteract(null!)) ? _focusEmissionEnergy : 0f;
    }

    public override void _Process(double delta)
    {
        if (!_active)
        {
            return;
        }

        _elapsed += (float)delta;
        float p = Mathf.Clamp(_elapsed / Duration, 0f, 1f);
        Color albedo = _idleColor.Lerp(_workingColor, p);
        _material.AlbedoColor = albedo;
        _material.Emission = albedo;

        if (p >= 1f)
        {
            _active = false;
            _completed = true;
            _material.AlbedoColor = _doneColor;
            _material.Emission = _doneColor;
            _material.EmissionEnergyMultiplier = 0f;
            EmitSignal(SignalName.TaskCompleted, TaskId);
            MissionManager.Instance?.CompleteTask(_taskData);
        }
    }
}
