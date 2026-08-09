using System;
using Godot;

namespace ChronoShift;

/// <summary>One character on the hero-select panel.</summary>
public partial class HeroSelectRow : PanelContainer
{
    public event Action<CharacterData>? Picked;

    [Export] private NodePath _namePath = "Row/Text/Name";
    [Export] private NodePath _clipsPath = "Row/Text/Clips";
    [Export] private NodePath _selectButtonPath = "Row/Select";
    [Export] private NodePath _currentLabelPath = "Row/Current";

    private CharacterData _data = null!;

    public void Bind(CharacterData data, bool isCurrent)
    {
        _data = data;

        GetNode<Label>(_namePath).Text = data.DisplayName;

        // Show which locomotion clips this character actually ships — a walk-only
        // rig reads differently from one with idle/run/jump.
        var clips = new System.Collections.Generic.List<string>();
        if (!string.IsNullOrEmpty(data.IdleClip)) clips.Add("idle");
        if (!string.IsNullOrEmpty(data.WalkClip)) clips.Add("yurish");
        if (!string.IsNullOrEmpty(data.RunClip)) clips.Add("yugurish");
        if (!string.IsNullOrEmpty(data.JumpClip)) clips.Add("sakrash");
        GetNode<Label>(_clipsPath).Text = clips.Count > 0 ? string.Join(" · ", clips).ToUpperInvariant() : "—";

        GetNode<Label>(_currentLabelPath).Visible = isCurrent;
        GetNode<Button>(_selectButtonPath).Visible = !isCurrent;
        GetNode<Button>(_selectButtonPath).Pressed += () => Picked?.Invoke(_data);
    }
}
