using Godot;

namespace ChronoShift;

/// <summary>
/// The "Saqlash daftari" from the UI design: a paper ledger of save slots.
/// The design sketches three slots; the game still keeps a single save file,
/// so slot 1 is live and the other two read as empty until save slots exist.
/// </summary>
public partial class SavesPanel : Control
{
    [Signal] public delegate void ClosedEventHandler();

    /// <summary>Emitted when a save is loaded, so the host can enter gameplay.</summary>
    [Signal] public delegate void SaveLoadedEventHandler();

    [Export] private NodePath _modeLabelPath = "Scrim/Book/Rows/Header/HeaderRow/Titles/Mode";
    [Export] private NodePath _closeButtonPath = "Scrim/Book/Rows/Header/HeaderRow/Close";
    [Export] private NodePath _slot1Path = "Scrim/Book/Rows/Slot1";

    private Label _modeLabel = null!;
    private SaveSlotRow _slot1 = null!;

    /// <summary>Save mode offers a Save button; load mode (from the main menu) does not.</summary>
    private bool _saveMode;

    public override void _Ready()
    {
        _modeLabel = GetNode<Label>(_modeLabelPath);
        _slot1 = GetNode<SaveSlotRow>(_slot1Path);

        GetNode<Button>(_closeButtonPath).Pressed += Close;
        _slot1.SavePressed += OnSavePressed;
        _slot1.LoadPressed += OnLoadPressed;

        Visible = false;
    }

    /// <summary>Opens the ledger. <paramref name="saveMode"/> shows the Save action (pause menu).</summary>
    public void Open(bool saveMode)
    {
        _saveMode = saveMode;
        _modeLabel.Text = saveMode ? "SAQLASH REJIMI" : "YUKLASH REJIMI";
        Refresh();
        Visible = true;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Visible && @event.IsActionPressed("ui_cancel"))
        {
            Close();
            GetViewport().SetInputAsHandled();
        }
    }

    private void Refresh()
    {
        SaveData? data = SaveManager.Instance?.Peek();
        _slot1.Bind(data, _saveMode);
    }

    private void OnSavePressed()
    {
        SaveManager.Instance?.Save();
        Refresh();
    }

    private void OnLoadPressed()
    {
        if (SaveManager.Instance == null || !SaveManager.Instance.HasSave())
        {
            return;
        }

        SaveManager.Instance.Load();
        Visible = false;
        EmitSignal(SignalName.SaveLoaded);
    }

    private void Close()
    {
        Visible = false;
        EmitSignal(SignalName.Closed);
    }
}
