using Godot;

namespace ChronoShift;

/// <summary>
/// Draws the main menu's drafting-sheet furniture: the 60 px grid, the double
/// sheet border, and the compass rose (concentric circles + crosshair).
/// Coordinates come from the UI design canvas and assume a 1920x1080 stage,
/// which the project's canvas_items stretch scales to the window.
/// </summary>
public partial class BlueprintBackground : Control
{
    [Export] private float _gridSpacing = 60.0f;
    [Export] private float _outerInset = 30.0f;
    [Export] private float _innerInset = 40.0f;

    /// <summary>Shared centre of the compass circles and the crosshair.</summary>
    [Export] private Vector2 _roseCentre = new(1620, 280);
    [Export] private float _roseOuterRadius = 150.0f;
    [Export] private float _roseInnerRadius = 100.0f;
    [Export] private float _crosshairHalfLength = 170.0f;

    /// <summary>Dash length for the inner circle, in pixels along the arc.</summary>
    [Export] private float _dashLength = 9.0f;

    public override void _Draw()
    {
        Vector2 size = Size;

        DrawGrid(size);

        // Double border: 2 px outer, 1 px inner — the sheet's trim marks.
        DrawRect(new Rect2(_outerInset, _outerInset, size.X - _outerInset * 2, size.Y - _outerInset * 2),
            UiPalette.Ink.With(0.4f), filled: false, width: 2.0f);
        DrawRect(new Rect2(_innerInset, _innerInset, size.X - _innerInset * 2, size.Y - _innerInset * 2),
            UiPalette.Ink.With(0.2f), filled: false, width: 1.0f);

        DrawCompassRose();
    }

    private void DrawGrid(Vector2 size)
    {
        Color gridColor = UiPalette.Ink.With(0.06f);

        for (float x = _gridSpacing; x < size.X; x += _gridSpacing)
        {
            DrawLine(new Vector2(x, 0), new Vector2(x, size.Y), gridColor, 1.0f);
        }

        for (float y = _gridSpacing; y < size.Y; y += _gridSpacing)
        {
            DrawLine(new Vector2(0, y), new Vector2(size.X, y), gridColor, 1.0f);
        }
    }

    private void DrawCompassRose()
    {
        DrawArc(_roseCentre, _roseOuterRadius, 0, Mathf.Tau, 96, UiPalette.Ink.With(0.22f), 1.0f, antialiased: true);
        DrawDashedCircle(_roseCentre, _roseInnerRadius, UiPalette.Ink.With(0.3f));

        Color crosshair = UiPalette.Ink.With(0.25f);
        DrawLine(_roseCentre + new Vector2(0, -_crosshairHalfLength),
            _roseCentre + new Vector2(0, _crosshairHalfLength), crosshair, 1.0f);
        DrawLine(_roseCentre + new Vector2(-_crosshairHalfLength, 0),
            _roseCentre + new Vector2(_crosshairHalfLength, 0), crosshair, 1.0f);
    }

    /// <summary>DrawArc has no dash support, so step the circle in on/off arc segments.</summary>
    private void DrawDashedCircle(Vector2 centre, float radius, Color color)
    {
        float dashAngle = _dashLength / radius;
        for (float angle = 0; angle < Mathf.Tau; angle += dashAngle * 2.0f)
        {
            DrawArc(centre, radius, angle, angle + dashAngle, 4, color, 1.0f, antialiased: true);
        }
    }
}
