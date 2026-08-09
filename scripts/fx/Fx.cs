using Godot;

namespace ChronoShift;

/// <summary>
/// Spawns one-shot combat effects into the world. Effects are parented to the
/// scene root rather than the shooter, so they stay put once the gun moves on.
/// Every scene argument is optional — an unassigned effect simply does nothing.
/// </summary>
public static class Fx
{
    /// <summary>Sparks and dust at an impact point, facing back along the surface normal.</summary>
    public static void SpawnImpact(PackedScene? scene, Node context, Vector3 position, Vector3 normal)
    {
        var fx = Spawn(scene, context, position);
        if (fx == null)
        {
            return;
        }

        // Particles emit along the node's +Z, so aim it out of the surface.
        if (normal.LengthSquared() > 0.001f)
        {
            Vector3 up = Mathf.Abs(normal.Normalized().Dot(Vector3.Up)) > 0.99f ? Vector3.Right : Vector3.Up;
            fx.LookAt(position + normal, up);
        }
    }

    /// <summary>Muzzle flash at the barrel.</summary>
    public static void SpawnMuzzle(PackedScene? scene, Node context, Vector3 position) =>
        Spawn(scene, context, position);

    /// <summary>Visible streak from muzzle to impact.</summary>
    public static void SpawnTracer(PackedScene? scene, Node context, Vector3 from, Vector3 to)
    {
        if (scene == null)
        {
            return;
        }

        var tracer = scene.Instantiate<Tracer>();
        Root(context).AddChild(tracer);
        tracer.Stretch(from, to);
    }

    private static Node3D? Spawn(PackedScene? scene, Node context, Vector3 position)
    {
        if (scene == null)
        {
            return null;
        }

        var fx = scene.Instantiate<Node3D>();
        Root(context).AddChild(fx);
        fx.GlobalPosition = position;
        return fx;
    }

    private static Node Root(Node context) => context.GetTree().CurrentScene ?? context.GetTree().Root;
}
