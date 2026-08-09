using Godot;
using System.Collections.Generic;

namespace ChronoShift;

/// <summary>Loads the tech catalog, tracks unlocks, and exposes aggregate gameplay multipliers.</summary>
public partial class TechTreeManager : Node
{
    public static TechTreeManager Instance { get; private set; } = null!;

    [Signal]
    public delegate void TechUnlockedEventHandler(string techId);

    [Export] private string _techFolder = "res://resources/data/tech";

    private readonly List<TechNodeData> _catalog = new();
    private readonly HashSet<string> _unlocked = new();

    public System.Collections.Generic.IReadOnlyList<TechNodeData> Catalog => _catalog;

    public System.Collections.Generic.IReadOnlyCollection<string> UnlockedTechIds => _unlocked;

    public float RewardMoneyMultiplier { get; private set; } = 1f;
    public float PlayerSpeedMultiplier { get; private set; } = 1f;

    public override void _Ready()
    {
        Instance = this;
        LoadCatalog();
    }

    private void LoadCatalog()
    {
        foreach (string path in ResourceFolder.Paths(_techFolder))
        {
            var node = ResourceLoader.Load<TechNodeData>(path);
            if (node != null)
            {
                _catalog.Add(node);
            }
        }

        if (_catalog.Count == 0)
        {
            GD.PushWarning("TechTreeManager: no tech nodes loaded from " + _techFolder + ".");
        }
        else
        {
            GD.Print($"TechTreeManager: loaded {_catalog.Count} tech node(s).");
        }
    }

    public bool IsUnlocked(string techId) => _unlocked.Contains(techId);

    public bool CanUnlock(TechNodeData node)
    {
        if (node == null || IsUnlocked(node.TechId))
        {
            return false;
        }

        foreach (string id in node.PrerequisiteIds)
        {
            if (!IsUnlocked(id))
            {
                return false;
            }
        }

        if (ProgressionManager.Instance != null && ProgressionManager.Instance.Level < node.RequiredLevel)
        {
            return false;
        }

        if (EconomyManager.Instance != null && EconomyManager.Instance.Money < node.CostMoney)
        {
            return false;
        }

        return true;
    }

    public bool TryUnlock(TechNodeData node)
    {
        if (!CanUnlock(node))
        {
            return false;
        }

        if (EconomyManager.Instance == null || !EconomyManager.Instance.TrySpendMoney(node.CostMoney))
        {
            return false;
        }

        _unlocked.Add(node.TechId);
        RecomputeEffects();
        EmitSignal(SignalName.TechUnlocked, node.TechId);
        return true;
    }

    public void LoadState(System.Collections.Generic.IEnumerable<string> unlockedIds)
    {
        _unlocked.Clear();
        foreach (string id in unlockedIds)
        {
            _unlocked.Add(id);
        }

        RecomputeEffects();
    }

    private void RecomputeEffects()
    {
        RewardMoneyMultiplier = 1f;
        PlayerSpeedMultiplier = 1f;

        foreach (TechNodeData node in _catalog)
        {
            if (!IsUnlocked(node.TechId))
            {
                continue;
            }

            switch (node.Effect)
            {
                case TechEffectType.RewardMoneyMultiplier:
                    RewardMoneyMultiplier += node.EffectValue;
                    break;
                case TechEffectType.PlayerSpeedMultiplier:
                    PlayerSpeedMultiplier += node.EffectValue;
                    break;
            }
        }
    }
}
