using Godot;

namespace ChronoShift;

/// <summary>Tracks experience and derives the player's level from it.</summary>
public partial class ProgressionManager : Node
{
    public static ProgressionManager Instance { get; private set; } = null!;

    [Signal]
    public delegate void XpChangedEventHandler(int xp, int level);

    [Signal]
    public delegate void LeveledUpEventHandler(int level);

    [Export] private int _xpPerLevel = 100;

    public int Xp { get; private set; }
    public int Level { get; private set; } = 1;

    /// <summary>XP a single level costs; the HUD bar needs it to show progress within the level.</summary>
    public int XpPerLevel => _xpPerLevel;

    /// <summary>XP earned inside the current level, 0.._xpPerLevel.</summary>
    public int XpIntoLevel => _xpPerLevel > 0 ? Xp % _xpPerLevel : 0;

    /// <summary>How far through the current level the player is, 0..1.</summary>
    public float LevelProgress => _xpPerLevel > 0 ? (float)XpIntoLevel / _xpPerLevel : 0f;

    public override void _Ready()
    {
        Instance = this;
    }

    public void AddXp(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Xp += amount;
        int newLevel = 1 + Xp / _xpPerLevel;
        bool leveledUp = newLevel > Level;
        Level = newLevel;

        EmitSignal(SignalName.XpChanged, Xp, Level);
        if (leveledUp)
        {
            EmitSignal(SignalName.LeveledUp, Level);
        }
    }

    public void LoadState(int xp, int level)
    {
        Xp = xp;
        Level = level;
        EmitSignal(SignalName.XpChanged, Xp, Level);
    }
}
