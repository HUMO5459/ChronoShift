using Godot;

namespace ChronoShift;

/// <summary>
/// A weapon lying in the world that the player can take with the interact key,
/// the same way they board a vehicle. It shows the weapon's own model, so the
/// rack reads as the guns that are actually on offer.
/// </summary>
public partial class WeaponPickup : StaticBody3D, IInteractable
{
    /// <summary>The weapon this hands over.</summary>
    [Export] private WeaponData? _weapon;

    /// <summary>Node the model is parented to, so the collision box stays put.</summary>
    [Export] private NodePath _modelRootPath = "Model";

    /// <summary>Rotation/scale for the display model; the in-hand grip has its own.</summary>
    [Export] private Vector3 _displayRotationDegrees = Vector3.Zero;
    [Export] private float _displayScale = 1.0f;

    /// <summary>Whether the pickup disappears once taken.</summary>
    [Export] private bool _consumeOnTake = true;

    [Export] private float _spinDegreesPerSecond = 35.0f;

    /// <summary>Weapon models are dense; they are only worth drawing up close.</summary>
    [Export] private float _drawRange = 55.0f;

    private Node3D _modelRoot = null!;
    private bool _taken;

    public string InteractionPrompt => _weapon != null ? _weapon.DisplayName : "Qurol";

    /// <summary>Line under the prompt, e.g. "AVTOMAT · 32 ZARAR".</summary>
    public string DetailLabel =>
        _weapon == null ? "" : $"{_weapon.Category.ToString().ToUpperInvariant()} · {_weapon.Damage:0} ZARAR";

    public override void _Ready()
    {
        _modelRoot = GetNode<Node3D>(_modelRootPath);

        if (_weapon?.ModelScene == null)
        {
            GD.PushWarning($"WeaponPickup '{Name}' has no weapon or model assigned.");
            return;
        }

        var model = _weapon.ModelScene.Instantiate<Node3D>();
        model.RotationDegrees = _displayRotationDegrees;
        model.Scale = new Vector3(_displayScale, _displayScale, _displayScale);
        _modelRoot.AddChild(model);
        DistanceCull.ApplyTo(model, _drawRange, 8.0f);
    }

    public override void _Process(double delta)
    {
        if (!_taken && _spinDegreesPerSecond != 0.0f)
        {
            _modelRoot.RotateY(Mathf.DegToRad(_spinDegreesPerSecond) * (float)delta);
        }
    }

    public bool CanInteract(Node3D interactor) => !_taken && _weapon != null;

    public void Interact(Node3D interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        if (GetTree().GetFirstNodeInGroup("player_weapon") is not WeaponController controller)
        {
            GD.PushWarning("WeaponPickup: no WeaponController in the scene.");
            return;
        }

        controller.AddWeapon(_weapon);
        _taken = true;

        if (_consumeOnTake)
        {
            QueueFree();
        }
    }

    public void SetFocused(bool focused)
    {
        // The prompt carries the feedback; a rack of glowing guns reads as noise.
    }
}
