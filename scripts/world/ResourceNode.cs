using Godot;

namespace ChronoShift;

/// <summary>
/// A deposit the player harvests: walk up, press the interact key, take the
/// material. No survival layer, no tool gating — the gathering step of the loop
/// is meant to be one keypress. A deposit is spent after a fixed number of
/// harvests and goes grey.
/// </summary>
public partial class ResourceNode : StaticBody3D, IInteractable
{
    /// <summary>Every deposit registers here so the HUD can point at the nearest one.</summary>
    public const string NodeGroup = "resource_nodes";

    /// <summary>What this deposit yields; also colours the prop.</summary>
    [Export] private ItemData? _item;

    /// <summary>Units handed over per harvest.</summary>
    [Export] private int _amount = 4;

    /// <summary>How many harvests before the deposit is spent.</summary>
    [Export] private int _uses = 3;

    [Export] private NodePath _meshPath = "MeshInstance3D";
    [Export] private float _focusEmissionEnergy = 0.55f;

    /// <summary>Colour a spent deposit fades to.</summary>
    [Export] private Color _spentColor = new(0.32f, 0.31f, 0.29f);

    private StandardMaterial3D _material = null!;
    private int _remainingUses;

    /// <summary>The material this deposit yields; the HUD tracker labels it.</summary>
    public ItemData? Item => _item;

    /// <summary>Harvests left before the deposit is spent.</summary>
    public int RemainingUses => _remainingUses;

    public string InteractionPrompt =>
        _item != null ? $"{_item.DisplayName} yig'ish" : "Yig'ish";

    /// <summary>Line under the prompt, e.g. "+4 · 3 MARTA QOLDI".</summary>
    public string YieldLabel => $"+{_amount} · {_remainingUses} MARTA QOLDI";

    public override void _Ready()
    {
        AddToGroup(NodeGroup);
        _remainingUses = Mathf.Max(1, _uses);

        var mesh = GetNode<MeshInstance3D>(_meshPath);
        Color tint = _item?.Tint ?? new Color(0.7f, 0.6f, 0.45f);
        _material = new StandardMaterial3D
        {
            AlbedoColor = tint,
            Roughness = 0.95f,
            EmissionEnabled = true,
            Emission = tint,
            EmissionEnergyMultiplier = 0f,
        };
        mesh.SetSurfaceOverrideMaterial(0, _material);

        if (_item == null)
        {
            GD.PushWarning($"ResourceNode '{Name}' has no ItemData assigned.");
        }
    }

    public bool CanInteract(Node3D interactor) => _item != null && _remainingUses > 0;

    public void Interact(Node3D interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        InventoryManager.Instance?.Add(_item, _amount);
        _remainingUses--;

        if (_remainingUses <= 0)
        {
            _material.AlbedoColor = _spentColor;
            _material.Emission = _spentColor;
            _material.EmissionEnergyMultiplier = 0f;
            Scale = new Vector3(Scale.X, Scale.Y * 0.55f, Scale.Z);
        }
    }

    public void SetFocused(bool focused)
    {
        _material.EmissionEnergyMultiplier = focused && CanInteract(this) ? _focusEmissionEnergy : 0f;
    }
}
