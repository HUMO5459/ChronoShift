using Godot;

namespace ChronoShift;

/// <summary>
/// The characteristics sheet: what the current vehicle and the equipped weapon
/// actually do. Every figure is read from the live node when one exists and from
/// the resource otherwise, so the screen can never quote a number the game is not
/// using. Opened with the stats key, and from the depot card in the menus.
/// </summary>
public partial class StatsPanel : Control
{
    [Export] private NodePath _vehicleNamePath = "Scrim/Center/Card/Rows/Columns/Vehicle/Name";
    [Export] private NodePath _vehicleKindPath = "Scrim/Center/Card/Rows/Columns/Vehicle/Kind";
    [Export] private NodePath _vehicleListPath = "Scrim/Center/Card/Rows/Columns/Vehicle/List";
    [Export] private NodePath _weaponNamePath = "Scrim/Center/Card/Rows/Columns/Weapon/Name";
    [Export] private NodePath _weaponKindPath = "Scrim/Center/Card/Rows/Columns/Weapon/Kind";
    [Export] private NodePath _weaponListPath = "Scrim/Center/Card/Rows/Columns/Weapon/List";
    [Export] private NodePath _closeButtonPath = "Scrim/Center/Card/Rows/Close";

    /// <summary>Roster used to name the chosen vehicle while the player is on foot.</summary>
    [Export] private VehicleRosterData? _roster;

    [Export] private string _statsAction = "stats";

    private Label _vehicleName = null!;
    private Label _vehicleKind = null!;
    private VBoxContainer _vehicleList = null!;
    private Label _weaponName = null!;
    private Label _weaponKind = null!;
    private VBoxContainer _weaponList = null!;

    public override void _Ready()
    {
        _vehicleName = GetNode<Label>(_vehicleNamePath);
        _vehicleKind = GetNode<Label>(_vehicleKindPath);
        _vehicleList = GetNode<VBoxContainer>(_vehicleListPath);
        _weaponName = GetNode<Label>(_weaponNamePath);
        _weaponKind = GetNode<Label>(_weaponKindPath);
        _weaponList = GetNode<VBoxContainer>(_weaponListPath);

        GetNode<Button>(_closeButtonPath).Pressed += Close;
        Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        bool statsPressed = InputMap.HasAction(_statsAction) && @event.IsActionPressed(_statsAction);

        if (Visible)
        {
            if (statsPressed || @event.IsActionPressed("ui_cancel"))
            {
                Close();
                GetViewport().SetInputAsHandled();
            }

            return;
        }

        if (statsPressed)
        {
            Open();
            GetViewport().SetInputAsHandled();
        }
    }

    public void Open()
    {
        Build();
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

    private void Build()
    {
        BuildVehicle();
        BuildWeapon();
    }

    /// <summary>
    /// Prefers the vehicle being driven — its hull reads live, and a hand-placed
    /// vehicle has no roster entry at all. On foot it falls back to the depot choice.
    /// </summary>
    private void BuildVehicle()
    {
        Clear(_vehicleList);

        Vehicle? driven = null;
        foreach (Node node in GetTree().GetNodesInGroup(Vehicle.VehicleGroup))
        {
            if (node is Vehicle { IsDriven: true } vehicle)
            {
                driven = vehicle;
                break;
            }
        }

        VehicleData? data = driven?.Data ?? VehicleRoster.Resolve(_roster);

        if (driven == null && data == null)
        {
            _vehicleName.Text = "Piyoda";
            _vehicleKind.Text = "TEXNIKASIZ";
            AddNote(_vehicleList, "Depoda texnika tanlanmagan.");
            return;
        }

        _vehicleName.Text = data?.DisplayName ?? driven!.Name.ToString();
        _vehicleKind.Text = string.IsNullOrEmpty(data?.Kind) ? "TEXNIKA" : data!.Kind;

        // Hull reads as remaining/total while aboard, and as the rating otherwise.
        AddStat(
            _vehicleList,
            "KORPUS",
            driven != null
                ? $"{Mathf.CeilToInt(driven.Health)} / {Mathf.RoundToInt(driven.MaxHealth)}"
                : $"{Mathf.RoundToInt(data?.MaxHealth ?? 0.0f)}");

        AddStat(_vehicleList, "TEZLIK", $"{(driven?.MoveSpeed ?? data?.MoveSpeed ?? 0.0f):0.0} m/s");
        AddStat(_vehicleList, "BRONYA", $"{(driven?.Armor ?? data?.Armor ?? 0.0f) * 100.0f:0}%");

        AddArmament(driven, data);

        int cost = data?.ProductionCost ?? 0;
        AddStat(_vehicleList, "ISHLAB CHIQARISH", cost > 0 ? $"₳{cost}" : "—");
    }

    /// <summary>
    /// The mounted gun, taken from the live cannon when the tank is on the map so
    /// the sheet and the gun cannot drift apart.
    /// </summary>
    private void AddArmament(Vehicle? driven, VehicleData? data)
    {
        TankCannon? cannon = driven != null ? FindCannon(driven) : null;

        if (cannon == null && data?.IsArmed != true)
        {
            AddStat(_vehicleList, "QUROLLANISH", "Qurolsiz");
            return;
        }

        AddStat(_vehicleList, "QUROLLANISH", string.IsNullOrEmpty(data?.ArmamentName) ? "To'p" : data!.ArmamentName);
        AddStat(_vehicleList, "URON", $"{cannon?.Damage ?? data?.ArmamentDamage ?? 0.0f:0}");
        AddStat(_vehicleList, "OTISH MASOFASI", $"{cannon?.Range ?? data?.ArmamentRange ?? 0.0f:0} m");
        AddStat(_vehicleList, "QAYTA O'QLASH", $"{cannon?.ReloadSeconds ?? data?.ArmamentReloadSeconds ?? 0.0f:0.0} s");
    }

    private static TankCannon? FindCannon(Vehicle vehicle)
    {
        foreach (Node node in vehicle.GetTree().GetNodesInGroup(TankCannon.CannonGroup))
        {
            if (node is TankCannon cannon && vehicle.IsAncestorOf(cannon))
            {
                return cannon;
            }
        }

        return null;
    }

    private void BuildWeapon()
    {
        Clear(_weaponList);

        var controller = GetTree().GetFirstNodeInGroup("player_weapon") as WeaponController;
        WeaponData? weapon = controller?.Current;

        if (weapon == null)
        {
            _weaponName.Text = "Qurolsiz";
            _weaponKind.Text = "—";
            AddNote(_weaponList, "Javondan qurol oling.");
            return;
        }

        _weaponName.Text = weapon.DisplayName;
        _weaponKind.Text = weapon.Category.ToString().ToUpperInvariant();

        AddStat(_weaponList, "URON", $"{weapon.Damage:0}");
        AddStat(_weaponList, "OTISH MASOFASI", $"{weapon.Range:0} m");
        AddStat(_weaponList, "O'T OCHISH", $"{weapon.FireRate:0.0} o'q/s{(weapon.Automatic ? " · avtomat" : "")}");
        AddStat(_weaponList, "MAGAZIN", $"{controller!.Ammo} / {weapon.MagazineSize}");
        AddStat(_weaponList, "QAYTA O'QLASH", $"{weapon.ReloadSeconds:0.0} s");
        AddStat(_weaponList, "ISHLAB CHIQARISH", weapon.ProductionCost > 0 ? $"₳{weapon.ProductionCost}" : "—");
    }

    private static void Clear(Node list)
    {
        foreach (Node child in list.GetChildren())
        {
            list.RemoveChild(child);
            child.QueueFree();
        }
    }

    /// <summary>One "LABEL ......... value" line, built in code so the rows stay data-driven.</summary>
    private static void AddStat(Node list, string label, string value)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 12);

        var caption = new Label { Text = label, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        caption.ThemeTypeVariation = "MonoLabel";
        caption.AddThemeFontSizeOverride("font_size", 13);
        caption.AddThemeColorOverride("font_color", Colors.White.With(0.5f));
        row.AddChild(caption);

        var amount = new Label { Text = value, HorizontalAlignment = HorizontalAlignment.Right };
        amount.ThemeTypeVariation = "MonoValue";
        amount.AddThemeFontSizeOverride("font_size", 16);
        amount.AddThemeColorOverride("font_color", UiPalette.HudText);
        row.AddChild(amount);

        list.AddChild(row);
    }

    private static void AddNote(Node list, string text)
    {
        var note = new Label { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        note.ThemeTypeVariation = "Body";
        note.AddThemeFontSizeOverride("font_size", 14);
        note.AddThemeColorOverride("font_color", Colors.White.With(0.45f));
        list.AddChild(note);
    }
}
