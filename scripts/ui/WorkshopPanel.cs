using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// The production window. The player picks a standing order, the workers spend a
/// few seconds on it, and the finished equipment lands in the inventory — the
/// factory floor, not a parts-assembly minigame.
/// </summary>
public partial class WorkshopPanel : Control
{
    [Export] private NodePath _rowsPath = "Scrim/Center/Board/Rows/Scroll/Recipes";
    [Export] private NodePath _closeButtonPath = "Scrim/Center/Board/Rows/Header/HeaderRow/Close";
    [Export] private NodePath _statusPath = "Scrim/Center/Board/Rows/Status";
    [Export] private NodePath _statusTextPath = "Scrim/Center/Board/Rows/Status/Rows/Text";
    [Export] private NodePath _statusBarPath = "Scrim/Center/Board/Rows/Status/Rows/Bar/Fill";
    [Export] private NodePath _emptyPath = "Scrim/Center/Board/Rows/Empty";
    [Export] private NodePath _designerButtonPath = "Scrim/Center/Board/Rows/Header/HeaderRow/Designer";

    [Export] private PackedScene _rowScene = null!;

    private VBoxContainer _rows = null!;
    private Control _status = null!;
    private Label _statusText = null!;
    private Control _statusBar = null!;
    private Label _empty = null!;

    private readonly List<RecipeRow> _boundRows = new();

    private RecipeData? _order;
    private float _orderRemaining;
    private float _orderTotal;
    private float _doneMessageRemaining;

    private bool IsBusy => _order != null;

    public override void _Ready()
    {
        AddToGroup(Workshop.PanelGroup);
        _rows = GetNode<VBoxContainer>(_rowsPath);
        _status = GetNode<Control>(_statusPath);
        _statusText = GetNode<Label>(_statusTextPath);
        _statusBar = GetNode<Control>(_statusBarPath);
        _empty = GetNode<Label>(_emptyPath);

        GetNode<Button>(_closeButtonPath).Pressed += Close;
        GetNode<Button>(_designerButtonPath).Pressed += OnDesignerPressed;

        _status.Visible = false;
        Visible = false;
    }

    /// <summary>
    /// Hands over to the drawing office. Standing orders and one-off constructions
    /// are the same workshop as far as the player is concerned, so the two screens
    /// are reachable from each other rather than only from a key.
    /// </summary>
    private void OnDesignerPressed()
    {
        Visible = false;

        // The designer manages the pause and the cursor itself, so this hands over
        // rather than closing — closing first would unpause the world underneath it.
        if (GetTree().Root.FindChild("DesignPanel", true, false) is DesignPanel designer)
        {
            designer.Open();
            return;
        }

        Close();
    }

    public void Open(Godot.Collections.Array<RecipeData> recipes)
    {
        BuildRows(recipes);
        Visible = true;
        GetTree().Paused = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Visible && @event.IsActionPressed("ui_cancel"))
        {
            Close();
            GetViewport().SetInputAsHandled();
        }
    }

    /// <summary>
    /// The tree is paused while the window is open, so this node runs Always and
    /// drives the production timer itself.
    /// </summary>
    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (_doneMessageRemaining > 0f)
        {
            _doneMessageRemaining -= dt;
            if (_doneMessageRemaining <= 0f && !IsBusy)
            {
                _status.Visible = false;
            }
        }

        if (_order == null)
        {
            return;
        }

        _orderRemaining -= dt;
        float progress = _orderTotal <= 0f ? 1f : 1f - Mathf.Clamp(_orderRemaining / _orderTotal, 0f, 1f);
        _statusBar.AnchorRight = progress;
        _statusText.Text = $"ISHLAB CHIQARILMOQDA — {_order.DisplayName} · {progress * 100f:0}%";

        if (_orderRemaining <= 0f)
        {
            CompleteOrder();
        }
    }

    private void BuildRows(Godot.Collections.Array<RecipeData> recipes)
    {
        foreach (Node child in _rows.GetChildren())
        {
            _rows.RemoveChild(child);
            child.QueueFree();
        }

        _boundRows.Clear();

        if (_rowScene == null)
        {
            GD.PushWarning("WorkshopPanel: no row scene assigned.");
            return;
        }

        foreach (RecipeData recipe in recipes)
        {
            if (recipe == null)
            {
                continue;
            }

            var row = _rowScene.Instantiate<RecipeRow>();
            _rows.AddChild(row);
            row.Bind(recipe);
            row.ProducePressed += OnProducePressed;
            _boundRows.Add(row);
        }

        _empty.Visible = _boundRows.Count == 0;
        RefreshRows();
    }

    private void RefreshRows()
    {
        foreach (RecipeRow row in _boundRows)
        {
            row.Refresh(IsBusy);
        }
    }

    private void OnProducePressed(RecipeData recipe)
    {
        if (IsBusy || InventoryManager.Instance == null || !InventoryManager.Instance.TryTakeInputs(recipe))
        {
            return;
        }

        _order = recipe;
        _orderTotal = Mathf.Max(0.2f, recipe.CraftSeconds);
        _orderRemaining = _orderTotal;
        _doneMessageRemaining = 0f;

        _status.Visible = true;
        _statusBar.AnchorRight = 0f;
        _statusText.AddThemeColorOverride("font_color", UiPalette.PaperInkSoft);
        RefreshRows();
    }

    private void CompleteOrder()
    {
        RecipeData finished = _order!;
        _order = null;

        InventoryManager.Instance?.Add(finished.Output, finished.OutputAmount);

        string produced = finished.Output != null
            ? $"{finished.Output.DisplayName} ×{finished.OutputAmount}"
            : finished.DisplayName;
        _statusText.Text = $"ISHLAB CHIQARISH TUGADI — {produced} inventarga qo'shildi";
        _statusText.AddThemeColorOverride("font_color", UiPalette.StampBlue);
        _statusBar.AnchorRight = 1f;
        _doneMessageRemaining = 3.5f;

        RefreshRows();
    }

    private void Close()
    {
        Visible = false;
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }
}
