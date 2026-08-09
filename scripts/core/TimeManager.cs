using Godot;

namespace ChronoShift;

/// <summary>
/// The campaign calendar. It drifts slowly on its own — just enough for the HUD
/// clock to move — and makes its real progress in jumps: every finished
/// engineering task pushes the date forward, and finishing a mission carries the
/// campaign into the next era's year. Time is earned by work, it does not run
/// away on its own.
/// </summary>
public partial class TimeManager : Node
{
    public static TimeManager Instance { get; private set; } = null!;

    /// <summary>The calendar jumped: from year, to year, and by how many days.</summary>
    [Signal]
    public delegate void TimeJumpedEventHandler(int fromYear, int toYear, int days);

    /// <summary>Ambient drift so the clock ticks; deliberately tiny, never accelerated.</summary>
    [Export] private float _driftDaysPerSecond = 0.02f;

    [Export] private int _startYear = 1900;

    public int CurrentYear => GameCalendar.YearOf(_elapsedDays, _startYear);
    public int DayOfYear => GameCalendar.DayOfYear(_elapsedDays); // 1-based

    public double ElapsedDays => _elapsedDays;

    /// <summary>The era the campaign opens in; the save book needs it to date a save.</summary>
    public int StartYear => _startYear;

    private double _elapsedDays;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        _elapsedDays += delta * _driftDaysPerSecond;
    }

    /// <summary>Pushes the calendar forward; a finished task is what pays for this.</summary>
    public void AdvanceDays(double days)
    {
        if (days <= 0.0)
        {
            return;
        }

        int fromYear = CurrentYear;
        _elapsedDays += days;
        EmitSignal(SignalName.TimeJumped, fromYear, CurrentYear, Mathf.RoundToInt(days));
    }

    /// <summary>
    /// Carries the calendar to the start of <paramref name="year"/>. Never moves
    /// backwards, so a mission played out of order cannot rewind the campaign.
    /// </summary>
    public void AdvanceToYear(int year)
    {
        double target = (year - _startYear) * GameCalendar.DaysPerYear;
        if (target <= _elapsedDays)
        {
            return;
        }

        AdvanceDays(target - _elapsedDays);
    }

    public void LoadState(double elapsedDays)
    {
        _elapsedDays = elapsedDays;
    }
}
