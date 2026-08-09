using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// The satchel, in four drawers: resources, produced equipment, engineering
/// tools and quest items. Deliberately a list — no slot grid, no weight, no
/// drag and drop.
/// </summary>
public partial class InventoryPanel : Control
{
    /// <summary>Group the pause menu and hub props use to find the panel.</summary>
    public const string PanelGroup = "inventory_panel";

    [Export] private NodePath _columnsPath = "Scrim/Center/Board/Rows/Columns";
    [Export] private NodePath _closeButtonPath = "Scrim/Center/Board/Rows/Header/HeaderRow/Close";
    [Export] private NodePath _footerPath = "Scrim/Center/Board/Rows/Footer";

    private HBoxContainer _columns = null!;
    private Label _footer = null!;

    private static readonly (ItemCategory Category, string Title)[] Drawers =
    {
        (ItemCategory.Resource, "RESURSLAR"),
        (ItemCategory.Equipment, "USKUNA"),
        (ItemCategory.Tool, "MUHANDISLIK ASBOBLARI"),
        (ItemCategory.Quest, "KVEST PREDMETLARI"),
    };

    public override void _Ready()
    {
        AddToGroup(PanelGroup);
        _columns = GetNode<HBoxContainer>(_columnsPath);
        _footer = GetNode<Label>(_footerPath);
        GetNode<Button>(_closeButtonPath).Pressed += Close;
        Visible = false;
    }

    public void Toggle()
    {
        if (Visible)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        Rebuild();
        Visible = true;
        GetTree().Paused = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Opening is refused while another panel holds the pause, so the satchel
        // never stacks on top of the workshop, tech tree or pause menu.
        if (@event.IsActionPressed("inventory") && (Visible || !GetTree().Paused))
        {
            Toggle();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (Visible && @event.IsActionPressed("ui_cancel"))
        {
            Close();
            GetViewport().SetInputAsHandled();
        }
    }

    private void Rebuild()
    {
        foreach (Node child in _columns.GetChildren())
        {
            _columns.RemoveChild(child);
            child.QueueFree();
        }

        InventoryManager? inventory = InventoryManager.Instance;
        int total = 0;

        foreach ((ItemCategory category, string title) in Drawers)
        {
            List<(ItemData Item, int Count)> owned = inventory?.OwnedIn(category) ?? new();
            total += owned.Count;
            _columns.AddChild(BuildDrawer(title, owned));
        }

        _footer.Text = total == 0
            ? "SUMKA BO'SH — MATERIAL YIG'ING VA MASTSERSKAYADA ISHLAB CHIQARING"
            : $"JAMI: {total} TUR";
    }

    private static VBoxContainer BuildDrawer(string title, List<(ItemData Item, int Count)> owned)
    {
        var column = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        column.AddThemeConstantOverride("separation", 8);

        var heading = new Label { Text = title };
        heading.ThemeTypeVariation = "MonoPaper";
        heading.AddThemeColorOverride("font_color", UiPalette.StampBlue);
        heading.AddThemeFontSizeOverride("font_size", 12);
        column.AddChild(heading);

        var rule = new ColorRect
        {
            Color = UiPalette.PaperInk.With(0.25f),
            CustomMinimumSize = new Vector2(0, 2),
        };
        column.AddChild(rule);

        if (owned.Count == 0)
        {
            var empty = new Label { Text = "—" };
            empty.ThemeTypeVariation = "BodyPaper";
            empty.AddThemeColorOverride("font_color", UiPalette.PaperInkFaint);
            column.AddChild(empty);
            return column;
        }

        foreach ((ItemData item, int count) in owned)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 10);

            var chip = new ColorRect
            {
                Color = item.Tint,
                CustomMinimumSize = new Vector2(12, 12),
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
            };
            row.AddChild(chip);

            var name = new Label { Text = item.DisplayName, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            name.ThemeTypeVariation = "BodyPaper";
            name.AddThemeFontSizeOverride("font_size", 15);
            row.AddChild(name);

            var amount = new Label { Text = $"×{count}" };
            amount.ThemeTypeVariation = "MonoPaper";
            amount.AddThemeColorOverride("font_color", UiPalette.PaperInk);
            amount.AddThemeFontSizeOverride("font_size", 14);
            row.AddChild(amount);

            column.AddChild(row);
        }

        return column;
    }

    private void Close()
    {
        Visible = false;
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }
}
