using Godot;

namespace ChronoShift;

/// <summary>
/// A single reward notification. Slides in from the right like the design's
/// toastIn keyframe, holds, then fades out and frees itself.
/// </summary>
public partial class HudToast : PanelContainer
{
    [Export] private NodePath _textLabelPath = "Rows/Text";
    [Export] private NodePath _subLabelPath = "Rows/Sub";

    [Export] private float _holdSeconds = 2.6f;
    [Export] private float _slideInSeconds = 0.25f;
    [Export] private float _fadeOutSeconds = 0.35f;

    /// <summary>How far right the toast starts before sliding into place.</summary>
    [Export] private float _slideDistance = 24.0f;

    /// <summary>Fills the toast and runs its entrance; the node frees itself when done.</summary>
    public void Display(string text, string sub)
    {
        GetNode<Label>(_textLabelPath).Text = text;

        var subLabel = GetNode<Label>(_subLabelPath);
        subLabel.Text = sub;
        subLabel.Visible = !string.IsNullOrEmpty(sub);

        Modulate = Colors.White.With(0f);
        Position += new Vector2(_slideDistance, 0);

        Tween tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(this, "modulate:a", 1.0f, _slideInSeconds);
        tween.TweenProperty(this, "position:x", Position.X - _slideDistance, _slideInSeconds)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        tween.SetParallel(false);
        tween.TweenInterval(_holdSeconds);
        tween.TweenProperty(this, "modulate:a", 0.0f, _fadeOutSeconds);
        tween.TweenCallback(Callable.From(QueueFree));
    }
}
