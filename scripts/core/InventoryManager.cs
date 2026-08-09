using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// The player's satchel: raw resources gathered off the maps and the equipment
/// the workshop produced. Counts are keyed by item id so a save restores without
/// holding resource references; the catalog under <c>_itemFolder</c> turns those
/// ids back into <see cref="ItemData"/> for the UI.
/// </summary>
public partial class InventoryManager : Node
{
    public static InventoryManager Instance { get; private set; } = null!;

    /// <summary>Anything the player holds has changed — the HUD and panels repaint.</summary>
    [Signal]
    public delegate void InventoryChangedEventHandler();

    /// <summary>Something was just picked up or produced; the HUD toasts it.</summary>
    [Signal]
    public delegate void ItemGainedEventHandler(string itemId, int amount);

    [Export] private string _itemFolder = "res://resources/data/items";

    private readonly Dictionary<string, int> _counts = new();
    private readonly Dictionary<string, ItemData> _catalog = new();

    /// <summary>Every item the campaign defines, in load order.</summary>
    public IReadOnlyDictionary<string, ItemData> Catalog => _catalog;

    public override void _Ready()
    {
        Instance = this;
        LoadCatalog();
    }

    private void LoadCatalog()
    {
        foreach (string path in ResourceFolder.Paths(_itemFolder))
        {
            var item = ResourceLoader.Load<ItemData>(path);
            if (item != null && !string.IsNullOrEmpty(item.ItemId))
            {
                _catalog[item.ItemId] = item;
            }
        }

        GD.Print($"InventoryManager: loaded {_catalog.Count} item definition(s).");
    }

    /// <summary>The <see cref="ItemData"/> behind an id, or null when the campaign has no such item.</summary>
    public ItemData? Resolve(string itemId) =>
        _catalog.TryGetValue(itemId, out ItemData? item) ? item : null;

    public int Count(string itemId) => _counts.TryGetValue(itemId, out int count) ? count : 0;

    public int Count(ItemData? item) => item == null ? 0 : Count(item.ItemId);

    public bool Has(ItemData? item, int amount) => item == null || Count(item.ItemId) >= amount;

    public void Add(ItemData? item, int amount)
    {
        if (item == null || amount <= 0 || string.IsNullOrEmpty(item.ItemId))
        {
            return;
        }

        // Remember items handed out by a map even if they are not in the folder.
        _catalog.TryAdd(item.ItemId, item);

        _counts[item.ItemId] = Count(item.ItemId) + amount;
        EmitSignal(SignalName.ItemGained, item.ItemId, amount);
        EmitSignal(SignalName.InventoryChanged);
    }

    /// <summary>Spends <paramref name="amount"/> of an item; false (and no change) when short.</summary>
    public bool TryConsume(ItemData? item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return true;
        }

        int held = Count(item.ItemId);
        if (held < amount)
        {
            return false;
        }

        _counts[item.ItemId] = held - amount;
        EmitSignal(SignalName.InventoryChanged);
        return true;
    }

    /// <summary>True when every input of <paramref name="recipe"/> is covered.</summary>
    public bool CanCraft(RecipeData? recipe)
    {
        if (recipe?.Output == null)
        {
            return false;
        }

        for (int i = 0; i < recipe.Inputs.Length; i++)
        {
            if (!Has(recipe.Inputs[i], recipe.AmountFor(i)))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Takes the inputs off the shelf; call before running the production timer.</summary>
    public bool TryTakeInputs(RecipeData? recipe)
    {
        if (!CanCraft(recipe) || recipe == null)
        {
            return false;
        }

        for (int i = 0; i < recipe.Inputs.Length; i++)
        {
            TryConsume(recipe.Inputs[i], recipe.AmountFor(i));
        }

        return true;
    }

    /// <summary>Owned items of one category, with their counts, in listing order.</summary>
    public List<(ItemData Item, int Count)> OwnedIn(ItemCategory category)
    {
        var owned = new List<(ItemData Item, int Count)>();
        foreach (KeyValuePair<string, ItemData> entry in _catalog)
        {
            int count = Count(entry.Key);
            if (count > 0 && entry.Value.Category == category)
            {
                owned.Add((entry.Value, count));
            }
        }

        owned.Sort((a, b) => a.Item.SortOrder.CompareTo(b.Item.SortOrder));
        return owned;
    }

    /// <summary>Every resource the campaign defines, held or not — the HUD strip shows zeroes too.</summary>
    public List<(ItemData Item, int Count)> AllResources()
    {
        var all = new List<(ItemData Item, int Count)>();
        foreach (ItemData item in _catalog.Values)
        {
            if (item.Category == ItemCategory.Resource)
            {
                all.Add((item, Count(item.ItemId)));
            }
        }

        all.Sort((a, b) => a.Item.SortOrder.CompareTo(b.Item.SortOrder));
        return all;
    }

    /// <summary>Flat id → count map for the save file.</summary>
    public Dictionary<string, int> Snapshot() => new(_counts);

    public void LoadState(Dictionary<string, int>? counts)
    {
        _counts.Clear();
        if (counts != null)
        {
            foreach (KeyValuePair<string, int> entry in counts)
            {
                if (entry.Value > 0)
                {
                    _counts[entry.Key] = entry.Value;
                }
            }
        }

        EmitSignal(SignalName.InventoryChanged);
    }
}
