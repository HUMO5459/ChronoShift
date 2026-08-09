using Godot;

namespace ChronoShift;

/// <summary>
/// One production order the workshop can run: what it costs in raw resources and
/// what drops into the inventory when the workers are done. Industrial production,
/// not part-by-part assembly — the player picks an order and waits.
/// </summary>
[GlobalClass]
public partial class RecipeData : Resource
{
    [Export] public string RecipeId = "";
    [Export] public string DisplayName = "Ishlab chiqarish";
    [Export(PropertyHint.MultilineText)] public string Description = "";

    /// <summary>Raw materials consumed; parallel to <see cref="InputAmounts"/>.</summary>
    [Export] public ItemData[] Inputs = System.Array.Empty<ItemData>();

    /// <summary>How many of each input the order costs; parallel to <see cref="Inputs"/>.</summary>
    [Export] public int[] InputAmounts = System.Array.Empty<int>();

    /// <summary>What the order produces.</summary>
    [Export] public ItemData? Output;

    [Export] public int OutputAmount = 1;

    /// <summary>Seconds the workers take — the panel runs a progress bar for this long.</summary>
    [Export] public float CraftSeconds = 3.0f;

    /// <summary>Cost of input <paramref name="index"/>; 1 when the amounts array is short.</summary>
    public int AmountFor(int index) => index < InputAmounts.Length ? InputAmounts[index] : 1;
}
