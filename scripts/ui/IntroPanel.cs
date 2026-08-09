using Godot;

namespace ChronoShift;

/// <summary>
/// The opening briefing of a new campaign: the army is losing, production is
/// wrecked, and the player is handed the engineer's commission. It hands over to
/// the mission board when dismissed, so a new game starts with the story and then
/// the list of work.
/// </summary>
public partial class IntroPanel : Control
{
    [Export] private NodePath _acceptButtonPath = "Scrim/Center/Card/Rows/Accept";

    public override void _Ready()
    {
        Visible = false;
        GetNode<Button>(_acceptButtonPath).Pressed += OnAccepted;

        if (GameIntro.Pending)
        {
            GameIntro.Pending = false;
            CallDeferred(MethodName.Open);
        }
    }

    private void Open()
    {
        Visible = true;
        GetTree().Paused = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void OnAccepted()
    {
        Visible = false;

        // Straight into the briefing board — the commission is only useful with orders.
        if (GetTree().GetFirstNodeInGroup(MissionBoard.PanelGroup) is MissionBoardPanel board)
        {
            board.Open();
            return;
        }

        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }
}
