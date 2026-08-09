using System;
using Godot;

namespace ChronoShift;

/// <summary>One selectable mission on the briefing board.</summary>
public partial class MissionBoardRow : PanelContainer
{
    /// <summary>C# event rather than a Godot signal: MissionData is passed straight through.</summary>
    public event Action<MissionData>? StartPressed;

    [Export] private NodePath _namePath = "Row/Text/Name";
    [Export] private NodePath _kindPath = "Row/Text/Kind";
    [Export] private NodePath _descriptionPath = "Row/Text/Description";
    [Export] private NodePath _bonusPath = "Row/Bonus";
    [Export] private NodePath _startPath = "Row/Start";

    private MissionData _mission = null!;

    /// <summary>
    /// <paramref name="done"/> stamps a mission the campaign has already cleared;
    /// <paramref name="next"/> marks the first one it has not, so the board reads
    /// as a campaign in progress rather than a flat menu.
    /// </summary>
    public void Bind(MissionData mission, bool done = false, bool next = false)
    {
        _mission = mission;

        GetNode<Label>(_namePath).Text = mission.DisplayName;
        GetNode<Label>(_descriptionPath).Text = mission.Description;
        GetNode<Label>(_bonusPath).Text = $"+₳ {mission.BonusMoney}\n+{mission.BonusXp} XP";

        var kind = GetNode<Label>(_kindPath);
        var start = GetNode<Button>(_startPath);

        if (done)
        {
            kind.Text = $"{mission.Kind} · BAJARILDI";
            kind.AddThemeColorOverride("font_color", UiPalette.StampDone);
            start.Text = "QAYTA O'YNASH";
        }
        else if (next)
        {
            kind.Text = $"{mission.Kind} · KEYINGI";
            kind.AddThemeColorOverride("font_color", UiPalette.StampBlue);
            start.Text = "BOSHLASH";
        }
        else
        {
            kind.Text = mission.Kind;
            start.Text = "BOSHLASH";
        }

        start.Pressed += () => StartPressed?.Invoke(_mission);
    }
}
