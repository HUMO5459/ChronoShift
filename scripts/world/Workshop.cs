using Godot;

namespace ChronoShift;

/// <summary>
/// The production point. Interacting opens the order window, which lives in the
/// game shell so it survives a map swap. The workshop itself only knows which
/// orders this era can run — the map hands them over when it is built.
/// </summary>
public partial class Workshop : StaticBody3D, IInteractable
{
    /// <summary>Group the shell's production panel registers under.</summary>
    public const string PanelGroup = "workshop_panel";

    /// <summary>Production orders offered here; the map fills this in from its MapData.</summary>
    [Export] private Godot.Collections.Array<RecipeData> _recipes = new();

    [Export] private NodePath _meshPath = "MeshInstance3D";
    [Export] private float _focusEmissionEnergy = 0.5f;

    private StandardMaterial3D? _material;

    public string InteractionPrompt => "Mastserskaya";

    /// <summary>Orders this workshop can run.</summary>
    public Godot.Collections.Array<RecipeData> Recipes => _recipes;

    public override void _Ready()
    {
        var mesh = GetNodeOrNull<MeshInstance3D>(_meshPath);
        if (mesh?.GetActiveMaterial(0) is StandardMaterial3D source)
        {
            _material = (StandardMaterial3D)source.Duplicate();
            _material.EmissionEnabled = true;
            _material.Emission = _material.AlbedoColor;
            _material.EmissionEnergyMultiplier = 0f;
            mesh.SetSurfaceOverrideMaterial(0, _material);
        }
    }

    public bool CanInteract(Node3D interactor) => true;

    public void Interact(Node3D interactor)
    {
        if (GetTree().GetFirstNodeInGroup(PanelGroup) is WorkshopPanel panel)
        {
            panel.Open(_recipes);
        }
        else
        {
            GD.PushWarning("Workshop: no WorkshopPanel in the scene.");
        }
    }

    public void SetFocused(bool focused)
    {
        if (_material != null)
        {
            _material.EmissionEnergyMultiplier = focused ? _focusEmissionEnergy : 0f;
        }
    }
}
