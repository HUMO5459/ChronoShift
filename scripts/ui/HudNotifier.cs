using Godot;

namespace ChronoShift;

/// <summary>
/// Lets world objects raise a HUD toast without holding a reference to the HUD.
/// The overlay lives in the shell scene and the world under it is swapped per
/// mission, so the lookup goes through the group every time — same approach as
/// <see cref="Fx"/> and <see cref="Sfx"/>.
/// </summary>
public static class HudNotifier
{
    public static void Notify(Node context, string title, string sub)
    {
        if (!GodotObject.IsInstanceValid(context))
        {
            return;
        }

        if (context.GetTree()?.GetFirstNodeInGroup(HUD.HudGroup) is HUD hud)
        {
            hud.Notify(title, sub);
        }
    }
}
