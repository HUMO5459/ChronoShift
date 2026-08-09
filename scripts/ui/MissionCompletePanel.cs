using Godot;

namespace ChronoShift;

/// <summary>
/// Shown when every task on a mission map is done: names the mission, states the
/// bonus, and returns to the hub. The bonus is only paid once the player leaves,
/// so the numbers on screen are still the ones about to be awarded.
/// </summary>
public partial class MissionCompletePanel : Control
{
    [Export] private NodePath _titlePath = "Scrim/Center/Card/Rows/Title";
    [Export] private NodePath _missionNamePath = "Scrim/Center/Card/Rows/Mission";
    [Export] private NodePath _bonusPath = "Scrim/Center/Card/Rows/Bonus";
    [Export] private NodePath _returnButtonPath = "Scrim/Center/Card/Rows/Return";

    public override void _Ready()
    {
        Visible = false;
        GetNode<Button>(_returnButtonPath).Pressed += OnReturnPressed;

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.MissionObjectivesComplete += _ => Open();
        }
        else
        {
            GD.PushWarning("MissionCompletePanel: MissionManager autoload not found.");
        }
    }

    private void Open()
    {
        MissionData? mission = MissionManager.Instance?.ActiveMission;
        if (mission == null)
        {
            return;
        }

        GetNode<Label>(_missionNamePath).Text = mission.DisplayName;
        GetNode<Label>(_bonusPath).Text = $"MUKOFOT:  +₳ {mission.BonusMoney}   ·   +{mission.BonusXp} XP";

        Visible = true;
        GetTree().Paused = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void OnReturnPressed()
    {
        Visible = false;
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;

        // Pays the bonus and swaps the world back to the hub.
        MissionManager.Instance?.FinishMission();
    }
}
