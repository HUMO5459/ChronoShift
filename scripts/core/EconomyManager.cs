using Godot;

namespace ChronoShift;

/// <summary>Global bank: tracks the player's money and announces changes.</summary>
public partial class EconomyManager : Node
{
    public static EconomyManager Instance { get; private set; } = null!;

    [Signal]
    public delegate void MoneyChangedEventHandler(int money);

    public int Money { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Money += amount;
        EmitSignal(SignalName.MoneyChanged, Money);
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount < 0 || amount > Money)
        {
            return false;
        }

        Money -= amount;
        EmitSignal(SignalName.MoneyChanged, Money);
        return true;
    }

    public void LoadState(int money)
    {
        Money = money;
        EmitSignal(SignalName.MoneyChanged, Money);
    }
}
