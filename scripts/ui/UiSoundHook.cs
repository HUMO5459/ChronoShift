using Godot;

namespace ChronoShift;

/// <summary>
/// Gives every button in the scene a click sound without touching the screens.
/// </summary>
/// <remarks>
/// The interface is built from a dozen separate scenes and most rows are created
/// at runtime, so wiring a sound per button by hand would both miss the generated
/// ones and have to be repeated in every screen. Watching the tree catches all of
/// them, including buttons that appear later.
/// </remarks>
public partial class UiSoundHook : Node
{
    [Export] private AudioStream? _click;

    [Export] private float _volumeDb = -8.0f;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        GetTree().NodeAdded += OnNodeAdded;

        // Buttons that already exist when this node starts are not announced.
        Hook(GetTree().Root);
    }

    public override void _ExitTree()
    {
        if (IsInstanceValid(GetTree()))
        {
            GetTree().NodeAdded -= OnNodeAdded;
        }
    }

    private void OnNodeAdded(Node node) => Hook(node);

    private void Hook(Node node)
    {
        if (node is BaseButton button)
        {
            button.Pressed += () => Sfx.PlayUi(this, _click, _volumeDb);
        }

        foreach (Node child in node.GetChildren())
        {
            Hook(child);
        }
    }
}
