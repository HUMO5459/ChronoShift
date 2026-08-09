using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// Draws the prerequisite links between tech cards. Reads the cards' live
/// rects each redraw, so it needs no duplicate copy of the layout.
/// </summary>
public partial class TechEdges : Control
{
    /// <summary>An unlocked prerequisite is inked in; a pending one stays faint.</summary>
    private readonly List<(Control From, Control To, bool Satisfied)> _edges = new();

    public void SetEdges(IEnumerable<(Control From, Control To, bool Satisfied)> edges)
    {
        _edges.Clear();
        _edges.AddRange(edges);
        QueueRedraw();
    }

    public override void _Draw()
    {
        foreach ((Control from, Control to, bool satisfied) in _edges)
        {
            // Leave the cards' right and left edges, at their vertical middles.
            var start = new Vector2(from.Position.X + from.Size.X, from.Position.Y + from.Size.Y * 0.5f);
            var end = new Vector2(to.Position.X, to.Position.Y + to.Size.Y * 0.5f);

            Color color = satisfied ? UiPalette.StampBlue : UiPalette.PaperInkFaint;
            DrawLine(start, end, color, 2.5f, antialiased: true);
        }
    }
}
