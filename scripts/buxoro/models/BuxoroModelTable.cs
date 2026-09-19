using Godot;

namespace ChronoShift;

/// <summary>
/// The Buxoro 1238 model import table: target heights and two-sided flags.
/// </summary>
/// <remarks>
/// Data-driven on purpose. The web build kept these numbers in two JavaScript
/// literals; here they live in moljal.tres so the importer holds no gameplay
/// constant of its own.
/// </remarks>
[GlobalClass]
public partial class BuxoroModelTable : Resource
{
    [Export] public Godot.Collections.Array<BuxoroModelRow> Models = new();

    /// <summary>Row for this model name, or null when the model is not in the table.</summary>
    public BuxoroModelRow? Find(string name)
    {
        foreach (BuxoroModelRow row in Models)
        {
            if (row != null && row.Name == name)
            {
                return row;
            }
        }

        return null;
    }
}
