using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// One node on the tech sheet: name, price, effect, and either an unlock
/// action, the reason it is locked, or the "OCHILDI" stamp once bought.
/// </summary>
public partial class TechNodeCard : PanelContainer
{
    [Signal] public delegate void UnlockPressedEventHandler(string techId);

    [Export] private NodePath _namePath = "Rows/Head/Name";
    [Export] private NodePath _costPath = "Rows/Head/Cost";
    [Export] private NodePath _effectPath = "Rows/Effect";
    [Export] private NodePath _unlockPath = "Rows/Unlock";
    [Export] private NodePath _lockPath = "Rows/Lock";
    [Export] private NodePath _stampPath = "Rows/Stamp";

    private TechNodeData _data = null!;

    public override void _Ready()
    {
        GetNode<Button>(_unlockPath).Pressed += () => EmitSignal(SignalName.UnlockPressed, _data.TechId);
    }

    /// <summary>Fills the card and switches it between owned / buyable / locked.</summary>
    public void Bind(TechNodeData data, IReadOnlyDictionary<string, string> nameById)
    {
        _data = data;

        GetNode<Label>(_namePath).Text = data.DisplayName;
        GetNode<Label>(_costPath).Text = $"₳ {data.CostMoney}";
        GetNode<Label>(_effectPath).Text = data.Description;

        TechTreeManager? tech = TechTreeManager.Instance;
        bool owned = tech != null && tech.IsUnlocked(data.TechId);
        bool canBuy = tech != null && tech.CanUnlock(data);

        var unlock = GetNode<Button>(_unlockPath);
        unlock.Visible = canBuy;
        unlock.Text = $"OCHISH — ₳ {data.CostMoney}";

        GetNode<Label>(_stampPath).Visible = owned;

        string lockReason = owned || canBuy ? "" : LockReason(data, tech, nameById);
        var lockLabel = GetNode<Label>(_lockPath);
        lockLabel.Text = lockReason;
        lockLabel.Visible = lockReason.Length > 0;

        Modulate = owned || canBuy ? Colors.White : Colors.White.With(0.75f);

        // Nothing lays the card out (its parent is a plain Control), and swapping the
        // unlock button for a lock note changes how tall it needs to be.
        ResetSize();
    }

    /// <summary>Mirrors TechTreeManager.CanUnlock's order: prerequisites, then level, then money.</summary>
    private static string LockReason(
        TechNodeData data, TechTreeManager? tech, IReadOnlyDictionary<string, string> nameById)
    {
        if (tech == null)
        {
            return "";
        }

        foreach (string id in data.PrerequisiteIds)
        {
            if (!tech.IsUnlocked(id))
            {
                string name = nameById.TryGetValue(id, out string? display) ? display : id;
                return $"AVVAL: {name.ToUpperInvariant()}";
            }
        }

        if (ProgressionManager.Instance != null && ProgressionManager.Instance.Level < data.RequiredLevel)
        {
            return $"DARAJA {data.RequiredLevel} KERAK";
        }

        if (EconomyManager.Instance != null && EconomyManager.Instance.Money < data.CostMoney)
        {
            return $"₳ {data.CostMoney - EconomyManager.Instance.Money} YETMAYDI";
        }

        return "";
    }
}
