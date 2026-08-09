using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// The workshop's drawing office. The player picks a kind, fits a module into each
/// bay, watches the characteristics move as they do, names the result and either
/// files it as a drawing or sends it to production.
///
/// Rows are built in code because both the bay list and the parts available to it
/// come from the module catalog — a new part is a new resource, not a new control.
/// </summary>
public partial class DesignPanel : Control
{
    [Export] private NodePath _vehicleTabPath = "Scrim/Center/Card/Rows/Header/Kinds/Vehicle";
    [Export] private NodePath _weaponTabPath = "Scrim/Center/Card/Rows/Header/Kinds/Weapon";
    [Export] private NodePath _closeButtonPath = "Scrim/Center/Card/Rows/Header/Close";
    [Export] private NodePath _slotsPath = "Scrim/Center/Card/Rows/Columns/Slots/List";
    [Export] private NodePath _statsPath = "Scrim/Center/Card/Rows/Columns/Stats/List";
    [Export] private NodePath _materialsPath = "Scrim/Center/Card/Rows/Columns/Stats/Materials";
    [Export] private NodePath _savedPath = "Scrim/Center/Card/Rows/Columns/Saved/List";
    [Export] private NodePath _namePath = "Scrim/Center/Card/Rows/Footer/Name";
    [Export] private NodePath _saveButtonPath = "Scrim/Center/Card/Rows/Footer/Save";
    [Export] private NodePath _produceButtonPath = "Scrim/Center/Card/Rows/Footer/Produce";
    [Export] private NodePath _statusPath = "Scrim/Center/Card/Rows/Status";

    [Export] private string _designerAction = "designer";

    private Button _vehicleTab = null!;
    private Button _weaponTab = null!;
    private VBoxContainer _slots = null!;
    private VBoxContainer _statsList = null!;
    private Label _materials = null!;
    private VBoxContainer _saved = null!;
    private LineEdit _name = null!;
    private Button _produce = null!;
    private Label _status = null!;

    private ModuleKind _kind = ModuleKind.Vehicle;

    /// <summary>The module fitted in each bay, indexed the same as SlotsFor(kind).</summary>
    private readonly List<ModuleData?> _fitted = new();

    public override void _Ready()
    {
        _vehicleTab = GetNode<Button>(_vehicleTabPath);
        _weaponTab = GetNode<Button>(_weaponTabPath);
        _slots = GetNode<VBoxContainer>(_slotsPath);
        _statsList = GetNode<VBoxContainer>(_statsPath);
        _materials = GetNode<Label>(_materialsPath);
        _saved = GetNode<VBoxContainer>(_savedPath);
        _name = GetNode<LineEdit>(_namePath);
        _produce = GetNode<Button>(_produceButtonPath);
        _status = GetNode<Label>(_statusPath);

        _vehicleTab.Pressed += () => SetKind(ModuleKind.Vehicle);
        _weaponTab.Pressed += () => SetKind(ModuleKind.Weapon);
        GetNode<Button>(_saveButtonPath).Pressed += OnSavePressed;
        _produce.Pressed += OnProducePressed;
        GetNode<Button>(_closeButtonPath).Pressed += Close;

        Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        bool pressed = InputMap.HasAction(_designerAction) && @event.IsActionPressed(_designerAction);

        if (Visible)
        {
            // The name field swallows the designer key while it has focus, so a
            // player typing "B" into a name does not slam the window shut.
            if (@event.IsActionPressed("ui_cancel") || (pressed && !_name.HasFocus()))
            {
                Close();
                GetViewport().SetInputAsHandled();
            }

            return;
        }

        if (pressed)
        {
            Open();
            GetViewport().SetInputAsHandled();
        }
    }

    public void Open()
    {
        SetKind(_kind);
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

    private void SetKind(ModuleKind kind)
    {
        _kind = kind;
        _vehicleTab.Disabled = kind == ModuleKind.Vehicle;
        _weaponTab.Disabled = kind == ModuleKind.Weapon;

        // Start each kind on its first available part per bay, so the sheet opens
        // on a buildable machine rather than a column of dashes.
        _fitted.Clear();
        foreach (ModuleSlot slot in DesignRegistry.SlotsFor(kind))
        {
            List<ModuleData> options = DesignRegistry.Available(kind, slot);
            _fitted.Add(options.Count > 0 ? options[0] : null);
        }

        Say("");
        RebuildAll();
    }

    private void RebuildAll()
    {
        BuildSlots();
        BuildStats();
        BuildSaved();
    }

    private void BuildSlots()
    {
        Clear(_slots);

        ModuleSlot[] slots = DesignRegistry.SlotsFor(_kind);
        for (int i = 0; i < slots.Length; i++)
        {
            AddSlotRow(i, slots[i]);
        }
    }

    /// <summary>
    /// One bay: its name, the fitted part, and arrows that step through the parts
    /// that fit it. The base bay cannot be emptied — a design needs something to
    /// be built on — so only the rest offer a "none" entry.
    /// </summary>
    private void AddSlotRow(int index, ModuleSlot slot)
    {
        List<ModuleData> options = DesignRegistry.Available(_kind, slot);
        bool isBase = index == 0;

        var panel = new PanelContainer();
        var style = new StyleBoxFlat
        {
            BgColor = Colors.White.With(0.05f),
            ContentMarginLeft = 14,
            ContentMarginRight = 14,
            ContentMarginTop = 9,
            ContentMarginBottom = 9,
        };
        panel.AddThemeStyleboxOverride("panel", style);

        var rows = new VBoxContainer();
        rows.AddThemeConstantOverride("separation", 3);
        panel.AddChild(rows);

        var head = new HBoxContainer();
        head.AddThemeConstantOverride("separation", 10);
        rows.AddChild(head);

        var caption = new Label { Text = SlotLabel(slot), SizeFlagsHorizontal = SizeFlags.ExpandFill };
        caption.ThemeTypeVariation = "MonoLabel";
        caption.AddThemeFontSizeOverride("font_size", 12);
        caption.AddThemeColorOverride("font_color", Colors.White.With(0.45f));
        head.AddChild(caption);

        var previous = new Button { Text = "<", CustomMinimumSize = new Vector2(38, 0) };
        var next = new Button { Text = ">", CustomMinimumSize = new Vector2(38, 0) };
        previous.Pressed += () => Step(index, slot, -1);
        next.Pressed += () => Step(index, slot, +1);
        previous.Disabled = options.Count == 0;
        next.Disabled = options.Count == 0;
        head.AddChild(previous);
        head.AddChild(next);

        ModuleData? fitted = _fitted[index];
        var name = new Label
        {
            Text = fitted?.DisplayName ?? (isBase ? "— mavjud emas —" : "— o'rnatilmagan —"),
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        name.ThemeTypeVariation = "Body";
        name.AddThemeFontSizeOverride("font_size", 16);
        name.AddThemeColorOverride("font_color", fitted != null ? Colors.White : UiPalette.Warning);
        rows.AddChild(name);

        if (fitted != null && !string.IsNullOrEmpty(fitted.Description))
        {
            var note = new Label { Text = fitted.Description, AutowrapMode = TextServer.AutowrapMode.WordSmart };
            note.ThemeTypeVariation = "MonoLabel";
            note.AddThemeFontSizeOverride("font_size", 11);
            note.AddThemeColorOverride("font_color", Colors.White.With(0.4f));
            rows.AddChild(note);
        }

        _slots.AddChild(panel);
    }

    /// <summary>
    /// Cycles the bay. Non-base bays get a null entry in the ring, which is how a
    /// part is taken off again.
    /// </summary>
    private void Step(int index, ModuleSlot slot, int direction)
    {
        List<ModuleData> options = DesignRegistry.Available(_kind, slot);
        if (options.Count == 0)
        {
            return;
        }

        var ring = new List<ModuleData?>(options);
        if (index != 0)
        {
            ring.Add(null);
        }

        int current = ring.IndexOf(_fitted[index]);
        int wanted = ((current + direction) % ring.Count + ring.Count) % ring.Count;
        _fitted[index] = ring[wanted];

        Say("");
        BuildSlots();
        BuildStats();
    }

    private void BuildStats()
    {
        Clear(_statsList);

        DesignStats stats = DesignRegistry.Compute(_kind, _fitted);

        if (_kind == ModuleKind.Vehicle)
        {
            AddStat("KORPUS", $"{Mathf.RoundToInt(stats.Health)}");
            AddStat("TEZLIK", $"{stats.Speed:0.0} m/s");
            AddStat("BRONYA", $"{stats.Armor * 100.0f:0}%");
            AddStat("URON", stats.Damage > 0.0f ? $"{stats.Damage:0}" : "qurolsiz");
            AddStat("OTISH MASOFASI", stats.Range > 0.0f ? $"{stats.Range:0} m" : "—");
            AddStat("QAYTA O'QLASH", stats.Damage > 0.0f ? $"{stats.ReloadSeconds:0.0} s" : "—");
        }
        else
        {
            AddStat("URON", $"{stats.Damage:0}");
            AddStat("OTISH MASOFASI", $"{stats.Range:0} m");
            AddStat("MAGAZIN", $"{stats.Magazine}");
            AddStat("QAYTA O'QLASH", $"{stats.ReloadSeconds:0.0} s");
        }

        int money = EconomyManager.Instance?.Money ?? 0;
        AddStat("NARX", $"₳{stats.CostMoney}", stats.CostMoney > money ? UiPalette.Warning : UiPalette.HudText);

        BuildMaterials();
    }

    /// <summary>The bill of materials, marked up when the satchel is short.</summary>
    private void BuildMaterials()
    {
        Dictionary<ItemData, int> materials = DesignRegistry.MaterialsFor(_fitted);
        if (materials.Count == 0)
        {
            _materials.Text = "MATERIAL TALAB QILINMAYDI";
            _materials.AddThemeColorOverride("font_color", Colors.White.With(0.4f));
            return;
        }

        var parts = new List<string>();
        bool short_ = false;

        foreach ((ItemData item, int amount) in materials)
        {
            int held = InventoryManager.Instance?.Count(item) ?? 0;
            short_ |= held < amount;
            parts.Add($"{item.DisplayName} {held}/{amount}");
        }

        _materials.Text = "MATERIAL:  " + string.Join("   ·   ", parts);
        _materials.AddThemeColorOverride("font_color", short_ ? UiPalette.Warning : Colors.White.With(0.55f));
    }

    private void BuildSaved()
    {
        Clear(_saved);

        int shown = 0;
        foreach (DesignRecord record in DesignRegistry.Designs)
        {
            AddSavedRow(record);
            shown++;
        }

        if (shown == 0)
        {
            var empty = new Label
            {
                Text = "Hali chizma saqlanmagan.",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            empty.ThemeTypeVariation = "Body";
            empty.AddThemeFontSizeOverride("font_size", 14);
            empty.AddThemeColorOverride("font_color", Colors.White.With(0.4f));
            _saved.AddChild(empty);
        }
    }

    private void AddSavedRow(DesignRecord record)
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat
        {
            BgColor = record.Built ? UiPalette.Accent.With(0.14f) : Colors.White.With(0.05f),
            ContentMarginLeft = 12,
            ContentMarginRight = 12,
            ContentMarginTop = 8,
            ContentMarginBottom = 8,
        };
        panel.AddThemeStyleboxOverride("panel", style);

        var rows = new VBoxContainer();
        rows.AddThemeConstantOverride("separation", 4);
        panel.AddChild(rows);

        var head = new HBoxContainer();
        head.AddThemeConstantOverride("separation", 8);
        rows.AddChild(head);

        var name = new Label
        {
            Text = record.Name,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        name.ThemeTypeVariation = "Body";
        name.AddThemeFontSizeOverride("font_size", 15);
        head.AddChild(name);

        var state = new Label
        {
            Text = record.Built ? "ISHLAB CHIQARILGAN" : DesignRegistry.KindOf(record) == ModuleKind.Vehicle ? "TEXNIKA" : "QUROL",
        };
        state.ThemeTypeVariation = "MonoLabel";
        state.AddThemeFontSizeOverride("font_size", 11);
        state.AddThemeColorOverride("font_color", record.Built ? UiPalette.AccentLight : Colors.White.With(0.4f));
        head.AddChild(state);

        var buttons = new HBoxContainer();
        buttons.AddThemeConstantOverride("separation", 6);
        rows.AddChild(buttons);

        var load = new Button { Text = "OCHISH", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        load.Pressed += () => LoadDesign(record);
        buttons.AddChild(load);

        var remove = new Button { Text = "O'CHIRISH" };
        remove.Pressed += () =>
        {
            DesignRegistry.Delete(record);
            SaveManager.Instance?.Save();
            BuildSaved();
            Say($"\"{record.Name}\" o'chirildi.");
        };
        buttons.AddChild(remove);

        _saved.AddChild(panel);
    }

    private void LoadDesign(DesignRecord record)
    {
        _kind = DesignRegistry.KindOf(record);
        _vehicleTab.Disabled = _kind == ModuleKind.Vehicle;
        _weaponTab.Disabled = _kind == ModuleKind.Weapon;

        _fitted.Clear();
        _fitted.AddRange(DesignRegistry.ModulesOf(record));

        // A plan saved before a bay existed comes back short; pad it out so the
        // slot list and the fitted list stay the same length.
        while (_fitted.Count < DesignRegistry.SlotsFor(_kind).Length)
        {
            _fitted.Add(null);
        }

        _name.Text = record.Name;
        RebuildAll();
        Say($"\"{record.Name}\" ochildi.");
    }

    private void OnSavePressed()
    {
        string name = _name.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            Say("Konstruksiyaga nom bering.", UiPalette.Warning);
            return;
        }

        if (!DesignRegistry.SaveDesign(name, _kind, _fitted))
        {
            Say("Asos moduli o'rnatilmagan — saqlab bo'lmadi.", UiPalette.Warning);
            return;
        }

        SaveManager.Instance?.Save();
        BuildSaved();
        Say($"\"{name}\" chizma sifatida saqlandi.");
    }

    private void OnProducePressed()
    {
        string name = _name.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            Say("Konstruksiyaga nom bering.", UiPalette.Warning);
            return;
        }

        // Production always works from a filed drawing, so what was built and what
        // the plan says can never disagree.
        if (!DesignRegistry.SaveDesign(name, _kind, _fitted))
        {
            Say("Asos moduli o'rnatilmagan — ishlab chiqarib bo'lmadi.", UiPalette.Warning);
            return;
        }

        DesignRecord record = DesignRegistry.Find(name)!;
        string failure = DesignRegistry.TryProduce(record);
        if (!string.IsNullOrEmpty(failure))
        {
            Say(failure, UiPalette.Warning);
            BuildSaved();
            return;
        }

        string note = DesignRegistry.KindOf(record) == ModuleKind.Vehicle
            ? "Depoda tanlash mumkin."
            : Equip(record);

        SaveManager.Instance?.Save();
        RebuildAll();
        Say($"\"{name}\" ishlab chiqarildi. {note}", UiPalette.AccentLight);
    }

    /// <summary>Hands a produced weapon straight to the player, when they are on the map.</summary>
    private string Equip(DesignRecord record)
    {
        WeaponData? weapon = DesignRegistry.BuiltWeapon(record.ProductId);
        if (weapon == null)
        {
            return "";
        }

        if (GetTree().GetFirstNodeInGroup("player_weapon") is WeaponController controller)
        {
            controller.AddWeapon(weapon);
            return "Qo'lingizga berildi.";
        }

        return "Qurol omborda.";
    }

    private void AddStat(string label, string value, Color? valueColor = null)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 12);

        var caption = new Label { Text = label, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        caption.ThemeTypeVariation = "MonoLabel";
        caption.AddThemeFontSizeOverride("font_size", 12);
        caption.AddThemeColorOverride("font_color", Colors.White.With(0.5f));
        row.AddChild(caption);

        var amount = new Label { Text = value, HorizontalAlignment = HorizontalAlignment.Right };
        amount.ThemeTypeVariation = "MonoValue";
        amount.AddThemeFontSizeOverride("font_size", 16);
        amount.AddThemeColorOverride("font_color", valueColor ?? UiPalette.HudText);
        row.AddChild(amount);

        _statsList.AddChild(row);
    }

    private void Say(string message, Color? color = null)
    {
        _status.Text = message;
        _status.Visible = !string.IsNullOrEmpty(message);
        _status.AddThemeColorOverride("font_color", color ?? Colors.White.With(0.6f));
    }

    private static string SlotLabel(ModuleSlot slot) => slot switch
    {
        ModuleSlot.Chassis => "SHASSI",
        ModuleSlot.Engine => "DVIGATEL",
        ModuleSlot.Armor => "ZIRH",
        ModuleSlot.Armament => "QUROLLANISH",
        ModuleSlot.Frame => "RAMKA",
        ModuleSlot.Barrel => "STVOL",
        ModuleSlot.Magazine => "MAGAZIN",
        ModuleSlot.Sight => "NISHON",
        _ => slot.ToString().ToUpperInvariant(),
    };

    private static void Clear(Node list)
    {
        foreach (Node child in list.GetChildren())
        {
            list.RemoveChild(child);
            child.QueueFree();
        }
    }
}
