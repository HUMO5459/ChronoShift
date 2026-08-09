using Godot;

namespace ChronoShift;

/// <summary>
/// The closing screen of the prototype: it sums up the run and states the point
/// the whole vertical slice exists to make — the campaign moved because of
/// engineering, not because of firepower.
/// </summary>
public partial class CampaignCompletePanel : Control
{
    [Export] private NodePath _yearsPath = "Scrim/Center/Card/Rows/Years";
    [Export] private NodePath _statsPath = "Scrim/Center/Card/Rows/Stats";
    [Export] private NodePath _closeButtonPath = "Scrim/Center/Card/Rows/Close";

    public override void _Ready()
    {
        Visible = false;
        GetNode<Button>(_closeButtonPath).Pressed += Close;

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.CampaignCompleted += Open;
        }
        else
        {
            GD.PushWarning("CampaignCompletePanel: MissionManager autoload not found.");
        }
    }

    private void Open()
    {
        int startYear = TimeManager.Instance?.StartYear ?? 1900;
        int endYear = TimeManager.Instance?.CurrentYear ?? startYear;
        GetNode<Label>(_yearsPath).Text = $"{startYear} — {endYear}";

        int tasks = MissionManager.Instance?.CompletedTaskIds.Count ?? 0;
        int money = EconomyManager.Instance?.Money ?? 0;
        int level = ProgressionManager.Instance?.Level ?? 1;
        int tech = TechTreeManager.Instance?.UnlockedTechIds.Count ?? 0;

        GetNode<Label>(_statsPath).Text =
            $"{tasks} muhandislik vazifasi   ·   {endYear - startYear} yil   ·   "
            + $"{tech} texnologiya   ·   ₳ {money}   ·   {level}-daraja";

        Visible = true;
        GetTree().Paused = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void Close()
    {
        Visible = false;
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }
}
