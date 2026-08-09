using Godot;

namespace ChronoShift;

/// <summary>Contract for world objects the player can focus and activate.</summary>
public interface IInteractable
{
    string InteractionPrompt { get; }
    bool CanInteract(Node3D interactor);
    void Interact(Node3D interactor);
    void SetFocused(bool focused);
}
