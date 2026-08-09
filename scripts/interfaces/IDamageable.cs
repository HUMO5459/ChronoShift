using Godot;

namespace ChronoShift;

/// <summary>Contract for world objects that can receive damage from a source.</summary>
public interface IDamageable
{
    void TakeDamage(float amount, Node3D source);
}
