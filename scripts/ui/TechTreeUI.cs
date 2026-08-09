using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// The tech sheet from the UI design: a drafting-paper overlay where nodes sit
/// in columns by prerequisite depth, linked by their dependency lines.
/// Only the MVP-era catalog is drawn — later eras are backlog, not built.
/// </summary>
public partial class TechTreeUI : CanvasLayer
{
    [Export] private NodePath _cardsPath = "Paper/Body/Cards";
    [Export] private NodePath _edgesPath = "Paper/Body/Edges";
    [Export] private NodePath _moneyPath = "Paper/Header/Row/Money";
    [Export] private NodePath _levelPath = "Paper/Header/Row/Level";
    [Export] private NodePath _yearPath = "Paper/Header/Row/Year";
    [Export] private NodePath _closeButtonPath = "Paper/Header/Row/Close";

    [Export] private PackedScene _cardScene = null!;

    /// <summary>Grid from the design canvas: columns 310 px apart, rows 170 px.</summary>
    [Export] private Vector2 _gridOrigin = new(52, 64);
    [Export] private Vector2 _gridStep = new(310, 170);

    private Control _cards = null!;
    private TechEdges _edges = null!;
    private readonly Dictionary<string, TechNodeCard> _cardsById = new();
    private readonly Dictionary<string, string> _namesById = new();

    public override void _Ready()
    {
        Visible = false;
        _cards = GetNode<Control>(_cardsPath);
        _edges = GetNode<TechEdges>(_edgesPath);

        GetNode<Button>(_closeButtonPath).Pressed += Close;

        if (TechTreeManager.Instance == null)
        {
            GD.PushWarning("TechTreeUI: TechTreeManager autoload not found.");
            return;
        }

        BuildCards();

        TechTreeManager.Instance.TechUnlocked += _ => Refresh();

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.MoneyChanged += _ => Refresh();
        }

        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.XpChanged += (_, _) => Refresh();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("tech_tree"))
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

    private void BuildCards()
    {
        if (_cardScene == null)
        {
            GD.PushWarning("TechTreeUI: no tech card scene assigned.");
            return;
        }

        IReadOnlyList<TechNodeData> catalog = TechTreeManager.Instance.Catalog;
        foreach (TechNodeData node in catalog)
        {
            _namesById[node.TechId] = node.DisplayName;
        }

        var rowsPerColumn = new Dictionary<int, int>();
        foreach (TechNodeData node in catalog)
        {
            int column = DepthOf(node, catalog);
            int row = rowsPerColumn.TryGetValue(column, out int used) ? used : 0;
            rowsPerColumn[column] = row + 1;

            var card = _cardScene.Instantiate<TechNodeCard>();
            _cards.AddChild(card);
            card.Position = _gridOrigin + new Vector2(column * _gridStep.X, row * _gridStep.Y);
            card.UnlockPressed += OnUnlockPressed;
            _cardsById[node.TechId] = card;
        }

        Refresh();
    }

    /// <summary>Column index: how many unlocks deep this node sits.</summary>
    private static int DepthOf(TechNodeData node, IReadOnlyList<TechNodeData> catalog)
    {
        int depth = 0;
        foreach (string prerequisiteId in node.PrerequisiteIds)
        {
            foreach (TechNodeData candidate in catalog)
            {
                if (candidate.TechId == prerequisiteId)
                {
                    depth = Mathf.Max(depth, DepthOf(candidate, catalog) + 1);
                }
            }
        }

        return depth;
    }

    private void Refresh()
    {
        if (TechTreeManager.Instance == null)
        {
            return;
        }

        foreach (TechNodeData node in TechTreeManager.Instance.Catalog)
        {
            if (_cardsById.TryGetValue(node.TechId, out TechNodeCard? card))
            {
                card.Bind(node, _namesById);
            }
        }

        RefreshHeader();
        RefreshEdges();
    }

    private void RefreshHeader()
    {
        int money = EconomyManager.Instance != null ? EconomyManager.Instance.Money : 0;
        int level = ProgressionManager.Instance != null ? ProgressionManager.Instance.Level : 1;
        int year = TimeManager.Instance != null ? TimeManager.Instance.CurrentYear : 1900;

        GetNode<Label>(_moneyPath).Text = $"₳ {money}";
        GetNode<Label>(_levelPath).Text = $"DARAJA {level}";
        GetNode<Label>(_yearPath).Text = $"YIL {year}";
    }

    private void RefreshEdges()
    {
        var edges = new List<(Control, Control, bool)>();
        foreach (TechNodeData node in TechTreeManager.Instance.Catalog)
        {
            foreach (string prerequisiteId in node.PrerequisiteIds)
            {
                if (_cardsById.TryGetValue(prerequisiteId, out TechNodeCard? from)
                    && _cardsById.TryGetValue(node.TechId, out TechNodeCard? to))
                {
                    edges.Add((from, to, TechTreeManager.Instance.IsUnlocked(prerequisiteId)));
                }
            }
        }

        _edges.SetEdges(edges);
    }

    private void Toggle()
    {
        bool open = !Visible;
        Visible = open;
        GetTree().Paused = open;
        Input.MouseMode = open ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
        if (open)
        {
            Refresh();
        }
    }

    private void Close()
    {
        if (Visible)
        {
            Toggle();
        }
    }

    private void OnUnlockPressed(string techId)
    {
        foreach (TechNodeData node in TechTreeManager.Instance.Catalog)
        {
            if (node.TechId == techId)
            {
                // Refresh runs from the TechUnlocked signal on success.
                TechTreeManager.Instance.TryUnlock(node);
                return;
            }
        }
    }
}
