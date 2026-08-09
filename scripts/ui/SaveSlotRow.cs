using Godot;

namespace ChronoShift;

/// <summary>
/// One row of the save ledger: slot label, save summary (or "— BO'SH —"),
/// and the Save/Load actions. Which actions show follows the design: Save only
/// in save mode, Load only when the slot holds something.
/// </summary>
public partial class SaveSlotRow : PanelContainer
{
    [Signal] public delegate void SavePressedEventHandler();
    [Signal] public delegate void LoadPressedEventHandler();

    [Export] private NodePath _slotLabelPath = "Row/SlotLabel";
    [Export] private NodePath _infoMainPath = "Row/Info/Main";
    [Export] private NodePath _infoSubPath = "Row/Info/Sub";
    [Export] private NodePath _saveButtonPath = "Row/Save";
    [Export] private NodePath _loadButtonPath = "Row/Load";

    /// <summary>Ledger label for this row, e.g. "SLOT 2".</summary>
    [Export] private string _slotName = "SLOT 1";

    /// <summary>Slots the single-save build cannot fill yet; they stay empty and inert.</summary>
    [Export] private bool _placeholderSlot;

    private Label _infoMain = null!;
    private Label _infoSub = null!;
    private Button _saveButton = null!;
    private Button _loadButton = null!;

    public override void _Ready()
    {
        GetNode<Label>(_slotLabelPath).Text = _slotName;
        _infoMain = GetNode<Label>(_infoMainPath);
        _infoSub = GetNode<Label>(_infoSubPath);
        _saveButton = GetNode<Button>(_saveButtonPath);
        _loadButton = GetNode<Button>(_loadButtonPath);

        _saveButton.Pressed += () => EmitSignal(SignalName.SavePressed);
        _loadButton.Pressed += () => EmitSignal(SignalName.LoadPressed);

        if (_placeholderSlot)
        {
            Bind(null, saveMode: false);
        }
    }

    /// <summary>Fills the row from <paramref name="data"/>, or shows it empty when null.</summary>
    public void Bind(SaveData? data, bool saveMode)
    {
        bool hasSave = data != null && !_placeholderSlot;

        if (hasSave)
        {
            // The menu's live clock is irrelevant here — date the save from its own day counter.
            int startYear = TimeManager.Instance?.StartYear ?? 1900;
            string date = GameCalendar.FormatDate(data!.ElapsedDays, startYear);

            _infoMain.Text = $"{date} · ₳{data.Money} · DARAJA {data.Level}";
            _infoSub.Text = $"{data.CompletedCount} VAZIFA · {data.UnlockedTech.Count} TEX · {data.Xp} XP";
        }
        else
        {
            _infoMain.Text = "— BO'SH —";
            _infoSub.Text = "";
        }

        // An empty slot reads as faint pencil; a filled one is proper ink.
        _infoMain.AddThemeColorOverride("font_color", hasSave ? UiPalette.PaperInk : UiPalette.PaperInkFaint);

        _infoSub.Visible = hasSave;
        _saveButton.Visible = saveMode && !_placeholderSlot;
        _loadButton.Visible = hasSave;
    }
}
