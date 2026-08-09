using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// Detects nearby interactables, focuses the nearest, and triggers it on input.
///
/// It sweeps a sphere against the physics world every frame rather than listening
/// to Area3D's body_entered/body_exited. Those signals never fired for the static
/// props — the board, workstations, deposits, the workshop, parked vehicles — so
/// nothing in the world could be used. A direct shape query sees them all, and it
/// also cannot go stale when a map swap frees whatever was focused.
/// </summary>
public partial class PlayerInteraction : Area3D
{
    /// <summary>Lets the HUD read the current focus without being wired to the player scene.</summary>
    public const string InteractionGroup = "player_interaction";

    /// <summary>Fallback reach if the shape child is missing or is not a sphere.</summary>
    [Export] private float _fallbackRadius = 2.5f;

    /// <summary>Most colliders considered in one sweep; well above what a crowded corner holds.</summary>
    [Export] private int _maxResults = 24;

    private Node3D _player = null!;
    private PhysicsShapeQueryParameters3D _query = null!;
    private Node3D? _focused;
    private readonly List<Node3D> _candidates = new();

    /// <summary>The interactable the player would activate right now, or null.</summary>
    public Node3D? Focused => _focused;

    public override void _Ready()
    {
        AddToGroup(InteractionGroup);
        _player = GetParent<Node3D>();

        _query = new PhysicsShapeQueryParameters3D
        {
            Shape = new SphereShape3D { Radius = ResolveRadius() },
            CollisionMask = CollisionMask,
            CollideWithBodies = true,
            CollideWithAreas = false,
        };
    }

    /// <summary>Reach comes from the scene's own collision shape, so the editor stays authoritative.</summary>
    private float ResolveRadius()
    {
        foreach (Node child in GetChildren())
        {
            if (child is CollisionShape3D { Shape: SphereShape3D sphere })
            {
                return sphere.Radius;
            }
        }

        return _fallbackRadius;
    }

    public override void _Process(double delta)
    {
        Sweep();
        UpdateFocus();

        if (Input.IsActionJustPressed("interact")
            && _focused != null
            && ((IInteractable)_focused).CanInteract(_player))
        {
            ((IInteractable)_focused).Interact(_player);
        }
    }

    private void Sweep()
    {
        _candidates.Clear();
        _query.Transform = new Transform3D(Basis.Identity, GlobalPosition);

        Godot.Collections.Array<Godot.Collections.Dictionary> hits =
            GetWorld3D().DirectSpaceState.IntersectShape(_query, _maxResults);

        foreach (Godot.Collections.Dictionary hit in hits)
        {
            if (hit["collider"].As<GodotObject>() is Node3D { } node && node is IInteractable)
            {
                _candidates.Add(node);
            }
        }
    }

    private void UpdateFocus()
    {
        Node3D? nearest = null;
        float nearestDistSq = float.MaxValue;
        Vector3 playerPos = _player.GlobalPosition;

        foreach (Node3D candidate in _candidates)
        {
            if (!((IInteractable)candidate).CanInteract(_player))
            {
                continue;
            }

            float distSq = candidate.GlobalPosition.DistanceSquaredTo(playerPos);
            if (distSq < nearestDistSq)
            {
                nearestDistSq = distSq;
                nearest = candidate;
            }
        }

        if (nearest == _focused)
        {
            return;
        }

        // The old focus may have been freed by a map swap between frames.
        if (_focused != null && IsInstanceValid(_focused))
        {
            ((IInteractable)_focused).SetFocused(false);
        }

        if (nearest != null)
        {
            ((IInteractable)nearest).SetFocused(true);
        }

        _focused = nearest;
    }
}
