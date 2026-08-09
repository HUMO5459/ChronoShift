using Godot;

namespace ChronoShift;

/// <summary>
/// Hero select, opened from the main menu. Lists the roster and remembers the
/// choice in CharacterRoster, which the player scene reads when gameplay starts.
/// </summary>
public partial class HeroSelectPanel : Control
{
    [Signal] public delegate void ClosedEventHandler();

    [Export] private NodePath _rowsPath = "Scrim/Center/Board/Rows/Body/Heroes";
    [Export] private NodePath _closeButtonPath = "Scrim/Center/Board/Rows/Header/HeaderRow/Close";
    [Export] private NodePath _previewPath = "Scrim/Center/Board/Rows/Body/PreviewCol/ViewportBox/SubViewport/Pivot";

    [Export] private PackedScene _rowScene = null!;
    [Export] private CharacterRosterData? _roster;

    private VBoxContainer _rows = null!;
    private HeroPreview? _preview;

    public override void _Ready()
    {
        _rows = GetNode<VBoxContainer>(_rowsPath);
        _preview = GetNodeOrNull<HeroPreview>(_previewPath);
        GetNode<Button>(_closeButtonPath).Pressed += Close;
        Visible = false;
    }

    public void Open()
    {
        BuildRows();
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

    private void BuildRows()
    {
        foreach (Node child in _rows.GetChildren())
        {
            _rows.RemoveChild(child);
            child.QueueFree();
        }

        if (_rowScene == null || _roster == null)
        {
            GD.PushWarning("HeroSelectPanel: no row scene or roster assigned.");
            return;
        }

        CharacterData? current = CharacterRoster.Resolve(_roster);

        foreach (CharacterData character in _roster.Characters)
        {
            if (character == null)
            {
                continue;
            }

            var row = _rowScene.Instantiate<HeroSelectRow>();
            _rows.AddChild(row);
            row.Bind(character, ReferenceEquals(character, current));
            row.Picked += OnPicked;
        }

        _preview?.Show(current);
    }

    private void OnPicked(CharacterData character)
    {
        CharacterRoster.SelectedId = character.CharacterId;
        BuildRows(); // refresh the "current" marker and preview
    }

    private void Close()
    {
        Visible = false;
        EmitSignal(SignalName.Closed);
    }
}
