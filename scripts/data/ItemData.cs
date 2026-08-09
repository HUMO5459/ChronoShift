using Godot;

namespace ChronoShift;

/// <summary>Which drawer of the inventory an item lands in.</summary>
public enum ItemCategory
{
    /// <summary>Raw material gathered off a map: wood, iron, coal, stone.</summary>
    Resource,

    /// <summary>Produced by the workshop and consumed by an engineering task.</summary>
    Equipment,

    /// <summary>Reusable engineering tool — produced once, never consumed.</summary>
    Tool,

    /// <summary>Story item carried between missions.</summary>
    Quest,
}

/// <summary>
/// One thing the player can hold. Resources are gathered from the world,
/// equipment comes out of the workshop; both live in the same inventory and
/// differ only by <see cref="Category"/>.
/// </summary>
[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public string ItemId = "";
    [Export] public string DisplayName = "Predmet";
    [Export(PropertyHint.MultilineText)] public string Description = "";

    /// <summary>Inventory drawer this belongs to.</summary>
    [Export] public ItemCategory Category = ItemCategory.Resource;

    /// <summary>Short tag for the HUD resource strip, e.g. "YOG'" — no icon art yet.</summary>
    [Export] public string ShortLabel = "";

    /// <summary>Colour of the item's chip in the HUD, the inventory and the world prop.</summary>
    [Export] public Color Tint = new(0.85f, 0.8f, 0.7f);

    /// <summary>
    /// Listing order in the HUD strip and the inventory. Without it the catalog
    /// would come out in filename order, which is not the order a player thinks
    /// about materials in.
    /// </summary>
    [Export] public int SortOrder;
}
