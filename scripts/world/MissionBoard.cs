using Godot;

namespace ChronoShift;

/// <summary>
/// The briefing board in the hub: interact with it to pick a mission and load
/// its map. The panel itself lives in the game shell, so it survives the swap.
/// </summary>
public partial class MissionBoard : StaticBody3D, IInteractable
{
    /// <summary>Group the shell's mission panel registers under.</summary>
    public const string PanelGroup = "mission_board_panel";

    [Export] private NodePath _meshPath = "MeshInstance3D";
    [Export] private float _focusEmissionEnergy = 0.5f;

    private StandardMaterial3D? _material;

    public string InteractionPrompt => "Missiyalar";

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
        if (GetTree().GetFirstNodeInGroup(PanelGroup) is MissionBoardPanel panel)
        {
            panel.Open();
        }
        else
        {
            GD.PushWarning("MissionBoard: no MissionBoardPanel in the scene.");
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
