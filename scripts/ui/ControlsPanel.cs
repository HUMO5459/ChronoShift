using Godot;

namespace ChronoShift;

/// <summary>Read-only overlay listing each game input action and its bound key.</summary>
public partial class ControlsPanel : Control
{
    [Signal]
    public delegate void ClosedEventHandler();

    [Export] private NodePath _rowsPath = "Scrim/Center/Board/Rows/Scroll/Pad/Actions";
    [Export] private NodePath _backButtonPath = "Scrim/Center/Board/Rows/Header/HeaderRow/Back";

    /// The game's own input actions in the order a player meets them, with the
    /// label they see in game (deliberately excludes Godot's built-in ui_* actions).
    private static readonly (string Group, string Action, string Label)[] Bindings =
    {
        ("HARAKAT", "move_forward", "Oldinga"),
        ("HARAKAT", "move_back", "Orqaga"),
        ("HARAKAT", "move_left", "Chapga"),
        ("HARAKAT", "move_right", "O'ngga"),
        ("HARAKAT", "run", "Yugurish"),
        ("HARAKAT", "jump", "Sakrash"),

        ("ISH", "interact", "Ishlash / yig'ish / olish"),
        ("ISH", "inventory", "Sumka"),
        ("ISH", "tech_tree", "Texnologiya daraxti"),

        ("JANG", "fire", "Otish"),
        ("JANG", "aim", "Nishonga olish (optika)"),
        ("JANG", "reload", "O'q qayta joylash"),
        ("JANG", "weapon_1", "1-qurol"),
        ("JANG", "weapon_2", "2-qurol"),
        ("JANG", "weapon_3", "3-qurol"),
        ("JANG", "weapon_4", "4-qurol"),
        ("JANG", "weapon_5", "5-qurol"),

        ("KAMERA VA TEXNIKA", "camera_toggle", "Birinchi / uchinchi shaxs"),
        ("KAMERA VA TEXNIKA", "exit_vehicle", "Texnikadan chiqish"),
    };

    private VBoxContainer _rows = null!;

    public override void _Ready()
    {
        Visible = false;
        _rows = GetNode<VBoxContainer>(_rowsPath);

        string group = string.Empty;
        foreach ((string Group, string Action, string Label) binding in Bindings)
        {
            if (binding.Group != group)
            {
                group = binding.Group;
                _rows.AddChild(BuildGroupHeading(group, _rows.GetChildCount() > 0));
            }

            _rows.AddChild(BuildRow(binding.Label, DescribeBinding(binding.Action)));
        }

        var back = GetNode<Button>(_backButtonPath);
        back.Pressed += OnBackPressed;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Visible && @event.IsActionPressed("ui_cancel"))
        {
            OnBackPressed();
            GetViewport().SetInputAsHandled();
        }
    }

    private static Control BuildGroupHeading(string text, bool spaced)
    {
        var label = new Label
        {
            Text = text,
            ThemeTypeVariation = "MonoLabel",
        };
        label.AddThemeColorOverride("font_color", new Color(0.6235f, 0.7647f, 0.9098f));
        label.AddThemeFontSizeOverride("font_size", 12);
        label.AddThemeConstantOverride("line_spacing", 0);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_top", spaced ? 22 : 8);
        margin.AddThemeConstantOverride("margin_bottom", 6);
        margin.AddChild(label);
        return margin;
    }

    private static Control BuildRow(string label, string key)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 20);

        var name = new Label
        {
            Text = label,
            ThemeTypeVariation = "Body",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };

        var binding = new Label
        {
            Text = key,
            ThemeTypeVariation = "MonoLabel",
            HorizontalAlignment = HorizontalAlignment.Right,
            CustomMinimumSize = new Vector2(180.0f, 0.0f),
        };
        binding.AddThemeColorOverride("font_color", new Color(0.8627f, 0.9137f, 0.9686f));

        row.AddChild(name);
        row.AddChild(binding);
        return row;
    }

    private void OnBackPressed()
    {
        Visible = false;
        EmitSignal(SignalName.Closed);
    }

    private static string DescribeBinding(string action)
    {
        if (!InputMap.HasAction(action))
        {
            return "—";
        }

        Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
        return events.Count == 0 ? "—" : DescribeEvent(events[0]);
    }

    private static string DescribeEvent(InputEvent ev)
    {
        switch (ev)
        {
            case InputEventKey key:
                if (key.PhysicalKeycode != Key.None)
                {
                    return OS.GetKeycodeString(key.PhysicalKeycode);
                }
                if (key.Keycode != Key.None)
                {
                    return OS.GetKeycodeString(key.Keycode);
                }
                return key.AsText();
            case InputEventMouseButton mouse:
                return (MouseButton)mouse.ButtonIndex switch
                {
                    MouseButton.Left => "Sichqoncha chap",
                    MouseButton.Right => "Sichqoncha o'ng",
                    MouseButton.Middle => "Sichqoncha o'rta",
                    _ => "Sichqoncha " + (int)mouse.ButtonIndex,
                };
            default:
                return ev.AsText();
        }
    }
}
