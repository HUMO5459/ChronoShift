using Godot;

namespace ChronoShift;

/// <summary>
/// One entry in the mission journal: status dot, briefing, total reward, and the
/// "BAJARILDI" stamp once every task on the mission has been finished.
/// </summary>
public partial class MissionRow : PanelContainer
{
    [Export] private NodePath _dotPath = "Row/Dot";
    [Export] private NodePath _namePath = "Row/Text/Name";
    [Export] private NodePath _descriptionPath = "Row/Text/Description";
    [Export] private NodePath _moneyPath = "Row/Rewards/Money";
    [Export] private NodePath _xpPath = "Row/Rewards/Xp";
    [Export] private NodePath _stampPath = "Row/Stamp";

    private Panel _dot = null!;
    private Label _stamp = null!;

    /// <summary>Fills the row from a mission and its completion state.</summary>
    public void Bind(MissionData mission, bool completed)
    {
        _dot = GetNode<Panel>(_dotPath);
        _stamp = GetNode<Label>(_stampPath);

        // The mission's full payout is every task reward plus the completion bonus.
        int money = mission.BonusMoney;
        int xp = mission.BonusXp;
        int taskCount = 0;
        int doneCount = 0;
        if (mission.Map != null)
        {
            foreach (EngineeringTaskData? task in mission.Map.Tasks)
            {
                if (task == null)
                {
                    continue;
                }

                money += task.RewardMoney;
                xp += task.RewardXp;
                taskCount++;
                if (MissionManager.Instance != null && MissionManager.Instance.IsTaskCompleted(task.TaskId))
                {
                    doneCount++;
                }
            }
        }

        string progress = taskCount > 0 ? $"  ·  {doneCount}/{taskCount} vazifa" : "";
        GetNode<Label>(_namePath).Text = mission.DisplayName;
        GetNode<Label>(_descriptionPath).Text = mission.Description + progress;
        GetNode<Label>(_moneyPath).Text = $"+₳ {money}";
        GetNode<Label>(_xpPath).Text = $"+{xp} XP";

        _stamp.Visible = completed;

        // A finished mission fills its dot with the stamp's ink; an open one stays hollow.
        var dotStyle = (StyleBoxFlat)_dot.GetThemeStylebox("panel").Duplicate();
        dotStyle.BgColor = completed ? UiPalette.StampDone : Colors.Transparent;
        dotStyle.BorderColor = completed ? UiPalette.StampDone : UiPalette.PaperInkFaint;
        _dot.AddThemeStyleboxOverride("panel", dotStyle);
    }
}
