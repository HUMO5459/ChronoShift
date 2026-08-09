using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>Characteristics of a design, worked out from its base plus every fitted module.</summary>
public readonly struct DesignStats
{
    public float Health { get; init; }
    public float Speed { get; init; }
    public float Armor { get; init; }
    public float Damage { get; init; }
    public float Range { get; init; }
    public float ReloadSeconds { get; init; }
    public int Magazine { get; init; }
    public int CostMoney { get; init; }
}

/// <summary>
/// The workshop's drawing office: the catalog of parts, the constructions the
/// player has drawn up, and the act of putting one into production.
///
/// A plain static rather than an autoload, matching <see cref="VehicleRoster"/> —
/// it has to outlive the menu-to-gameplay scene change, and adding an autoload
/// would mean a manual Project Settings step for no gain.
/// </summary>
public static class DesignRegistry
{
    private const string ModuleFolder = "res://resources/data/modules";

    private static readonly List<ModuleData> Catalog = new();
    private static readonly List<DesignRecord> Saved = new();

    /// <summary>Products of built designs, by id — what the depot and armoury hand out.</summary>
    private static readonly Dictionary<string, VehicleData> BuiltVehicles = new();
    private static readonly Dictionary<string, WeaponData> BuiltWeapons = new();

    private static bool _catalogLoaded;

    /// <summary>Every construction the player has drawn up, saved and unsaved alike.</summary>
    public static IReadOnlyList<DesignRecord> Designs
    {
        get
        {
            EnsureCatalog();
            return Saved;
        }
    }

    /// <summary>Parts that fit <paramref name="slot"/> and are open to the player.</summary>
    public static List<ModuleData> Available(ModuleKind kind, ModuleSlot slot)
    {
        EnsureCatalog();

        var matches = new List<ModuleData>();
        foreach (ModuleData module in Catalog)
        {
            if (module.Kind == kind && module.Slot == slot && module.IsUnlocked)
            {
                matches.Add(module);
            }
        }

        return matches;
    }

    /// <summary>The slots a design of this kind is assembled from, in fitting order.</summary>
    public static ModuleSlot[] SlotsFor(ModuleKind kind) => kind == ModuleKind.Vehicle
        ? new[] { ModuleSlot.Chassis, ModuleSlot.Engine, ModuleSlot.Armor, ModuleSlot.Armament }
        : new[] { ModuleSlot.Frame, ModuleSlot.Barrel, ModuleSlot.Magazine, ModuleSlot.Sight };

    public static ModuleData? Resolve(string moduleId)
    {
        EnsureCatalog();

        foreach (ModuleData module in Catalog)
        {
            if (module.ModuleId == moduleId)
            {
                return module;
            }
        }

        return null;
    }

    /// <summary>
    /// Adds up a design. The chassis (or frame) supplies the starting figures and
    /// every other module shifts them, so an empty design is simply its base.
    /// </summary>
    public static DesignStats Compute(ModuleKind kind, IEnumerable<ModuleData?> modules)
    {
        float health = 0.0f, speed = 0.0f, armor = 0.0f, damage = 0.0f, range = 0.0f, reload = 0.0f;
        int magazine = 0, cost = 0;

        foreach (ModuleData? module in modules)
        {
            if (module == null)
            {
                continue;
            }

            if (kind == ModuleKind.Vehicle && module.BaseVehicle != null)
            {
                VehicleData baseVehicle = module.BaseVehicle;
                health += baseVehicle.MaxHealth;
                speed += baseVehicle.MoveSpeed;
                armor += baseVehicle.Armor;
                damage += baseVehicle.ArmamentDamage;
                range += baseVehicle.ArmamentRange;
                reload += baseVehicle.ArmamentReloadSeconds;
            }
            else if (kind == ModuleKind.Weapon && module.BaseWeapon != null)
            {
                WeaponData baseWeapon = module.BaseWeapon;
                damage += baseWeapon.Damage;
                range += baseWeapon.Range;
                reload += baseWeapon.ReloadSeconds;
                magazine += baseWeapon.MagazineSize;
            }

            health += module.HealthBonus;
            speed += module.SpeedBonus;
            armor += module.ArmorBonus;
            damage += module.DamageBonus;
            range += module.RangeBonus;
            reload += module.ReloadDelta;
            magazine += module.MagazineBonus;
            cost += module.CostMoney;
        }

        return new DesignStats
        {
            Health = Mathf.Max(0.0f, health),
            Speed = Mathf.Max(0.0f, speed),
            Armor = Mathf.Clamp(armor, 0.0f, 0.85f),
            Damage = Mathf.Max(0.0f, damage),
            Range = Mathf.Max(0.0f, range),
            // A reload can be shortened by parts but never to nothing.
            ReloadSeconds = Mathf.Max(0.3f, reload),
            Magazine = Mathf.Max(1, magazine),
            CostMoney = cost,
        };
    }

    public static DesignStats Compute(DesignRecord record) =>
        Compute(KindOf(record), ModulesOf(record));

    /// <summary>Materials a design consumes, summed over its modules.</summary>
    public static Dictionary<ItemData, int> MaterialsFor(IEnumerable<ModuleData?> modules)
    {
        var totals = new Dictionary<ItemData, int>();

        foreach (ModuleData? module in modules)
        {
            if (module == null)
            {
                continue;
            }

            for (int i = 0; i < module.Inputs.Length; i++)
            {
                ItemData? item = module.Inputs[i];
                if (item == null)
                {
                    continue;
                }

                totals[item] = totals.GetValueOrDefault(item) + module.AmountFor(i);
            }
        }

        return totals;
    }

    public static ModuleKind KindOf(DesignRecord record) =>
        record.Kind == nameof(ModuleKind.Weapon) ? ModuleKind.Weapon : ModuleKind.Vehicle;

    public static List<ModuleData?> ModulesOf(DesignRecord record)
    {
        EnsureCatalog();

        var modules = new List<ModuleData?>();
        foreach (string id in record.ModuleIds)
        {
            modules.Add(string.IsNullOrEmpty(id) ? null : Resolve(id));
        }

        return modules;
    }

    /// <summary>
    /// Files a construction under <paramref name="name"/>, replacing an earlier
    /// drawing of the same name. A design has to have its base module fitted —
    /// without a chassis or a frame there is nothing to build on.
    /// </summary>
    public static bool SaveDesign(string name, ModuleKind kind, IReadOnlyList<ModuleData?> modules)
    {
        EnsureCatalog();

        if (string.IsNullOrWhiteSpace(name) || modules.Count == 0 || modules[0] == null)
        {
            return false;
        }

        var ids = new List<string>();
        foreach (ModuleData? module in modules)
        {
            ids.Add(module?.ModuleId ?? "");
        }

        DesignRecord? existing = Find(name);
        if (existing != null)
        {
            existing.Kind = kind.ToString();
            existing.ModuleIds = ids;

            // Redrawing a produced design makes it a drawing again: the machine on
            // the shelf was built to the old plan, so the new one is not paid for.
            existing.Built = false;
            BuiltVehicles.Remove(existing.ProductId);
            BuiltWeapons.Remove(existing.ProductId);
            return true;
        }

        Saved.Add(new DesignRecord { Name = name.Trim(), Kind = kind.ToString(), ModuleIds = ids });
        return true;
    }

    public static DesignRecord? Find(string name)
    {
        EnsureCatalog();

        foreach (DesignRecord record in Saved)
        {
            if (string.Equals(record.Name, name.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return record;
            }
        }

        return null;
    }

    public static void Delete(DesignRecord record)
    {
        BuiltVehicles.Remove(record.ProductId);
        BuiltWeapons.Remove(record.ProductId);
        Saved.Remove(record);
    }

    /// <summary>
    /// Puts a design into production: money and materials are spent, and the
    /// finished machine is registered so the depot (vehicles) or the player's
    /// hands (weapons) can take it. Returns why it failed, or empty on success.
    /// </summary>
    public static string TryProduce(DesignRecord record)
    {
        EnsureCatalog();

        List<ModuleData?> modules = ModulesOf(record);
        if (modules.Count == 0 || modules[0] == null)
        {
            return "Asos moduli o'rnatilmagan.";
        }

        DesignStats stats = Compute(KindOf(record), modules);

        if (EconomyManager.Instance == null || EconomyManager.Instance.Money < stats.CostMoney)
        {
            return $"Pul yetarli emas — ₳{stats.CostMoney} kerak.";
        }

        Dictionary<ItemData, int> materials = MaterialsFor(modules);
        foreach ((ItemData item, int amount) in materials)
        {
            if (InventoryManager.Instance?.Has(item, amount) != true)
            {
                int held = InventoryManager.Instance?.Count(item) ?? 0;
                return $"Material yetarli emas — {item.DisplayName} ×{amount} ({held}/{amount}).";
            }
        }

        // Everything checked before anything is spent, so a failed order never
        // leaves the player short of materials with nothing to show for it.
        if (!EconomyManager.Instance.TrySpendMoney(stats.CostMoney))
        {
            return "To'lov amalga oshmadi.";
        }

        foreach ((ItemData item, int amount) in materials)
        {
            InventoryManager.Instance?.TryConsume(item, amount);
        }

        Register(record, modules, stats);
        record.Built = true;
        return "";
    }

    /// <summary>Builds the finished product and files it where the game looks for it.</summary>
    private static void Register(DesignRecord record, List<ModuleData?> modules, DesignStats stats)
    {
        if (KindOf(record) == ModuleKind.Vehicle)
        {
            VehicleData? baseVehicle = modules[0]?.BaseVehicle;
            if (baseVehicle == null)
            {
                return;
            }

            BuiltVehicles[record.ProductId] = new VehicleData
            {
                VehicleId = record.ProductId,
                DisplayName = record.Name,
                Kind = "O'Z KONSTRUKSIYASI",
                Description = Summary(modules),
                VehicleScene = baseVehicle.VehicleScene,
                LiveryTint = baseVehicle.LiveryTint,
                MaxHealth = stats.Health,
                MoveSpeed = stats.Speed,
                TurnSpeed = baseVehicle.TurnSpeed,
                Armor = stats.Armor,
                ProductionCost = stats.CostMoney,
                ArmamentName = ArmamentName(modules),
                ArmamentDamage = stats.Damage,
                ArmamentRange = stats.Range,
                ArmamentReloadSeconds = stats.ReloadSeconds,
            };

            return;
        }

        WeaponData? baseWeapon = modules[0]?.BaseWeapon;
        if (baseWeapon == null)
        {
            return;
        }

        BuiltWeapons[record.ProductId] = new WeaponData
        {
            WeaponId = record.ProductId,
            DisplayName = record.Name,
            Category = baseWeapon.Category,
            Damage = stats.Damage,
            Range = stats.Range,
            FireRate = baseWeapon.FireRate,
            Automatic = baseWeapon.Automatic,
            MagazineSize = stats.Magazine,
            ReloadSeconds = stats.ReloadSeconds,
            PelletsPerShot = baseWeapon.PelletsPerShot,
            SpreadDegrees = baseWeapon.SpreadDegrees,
            UsesScope = baseWeapon.UsesScope || HasSight(modules),
            AimFov = baseWeapon.AimFov,
            FireSound = baseWeapon.FireSound,
            ModelScene = baseWeapon.ModelScene,
            GripPosition = baseWeapon.GripPosition,
            GripRotationDegrees = baseWeapon.GripRotationDegrees,
            GripScale = baseWeapon.GripScale,
            ProductionCost = stats.CostMoney,
        };
    }

    private static bool HasSight(IEnumerable<ModuleData?> modules)
    {
        foreach (ModuleData? module in modules)
        {
            if (module is { Slot: ModuleSlot.Sight })
            {
                return true;
            }
        }

        return false;
    }

    private static string ArmamentName(IEnumerable<ModuleData?> modules)
    {
        foreach (ModuleData? module in modules)
        {
            if (module is { Slot: ModuleSlot.Armament })
            {
                return module.DisplayName;
            }
        }

        return "";
    }

    private static string Summary(IEnumerable<ModuleData?> modules)
    {
        var names = new List<string>();
        foreach (ModuleData? module in modules)
        {
            if (module != null)
            {
                names.Add(module.DisplayName);
            }
        }

        return string.Join(" · ", names);
    }

    /// <summary>A produced vehicle, for the depot and the map loader.</summary>
    public static VehicleData? BuiltVehicle(string productId) =>
        BuiltVehicles.TryGetValue(productId, out VehicleData? vehicle) ? vehicle : null;

    /// <summary>Every produced vehicle, so the depot can list them beside the stock ones.</summary>
    public static IEnumerable<VehicleData> AllBuiltVehicles => BuiltVehicles.Values;

    public static WeaponData? BuiltWeapon(string productId) =>
        BuiltWeapons.TryGetValue(productId, out WeaponData? weapon) ? weapon : null;

    // --- Persistence ---------------------------------------------------------

    public static List<DesignRecord> Snapshot() => new(Designs);

    /// <summary>
    /// Restores the drawings and rebuilds the products of the ones already paid
    /// for — the machines themselves are derived, so only the plans are stored.
    /// </summary>
    public static void LoadState(IEnumerable<DesignRecord>? records)
    {
        EnsureCatalog();

        Saved.Clear();
        BuiltVehicles.Clear();
        BuiltWeapons.Clear();

        if (records == null)
        {
            return;
        }

        foreach (DesignRecord record in records)
        {
            Saved.Add(record);
            if (!record.Built)
            {
                continue;
            }

            List<ModuleData?> modules = ModulesOf(record);
            if (modules.Count > 0 && modules[0] != null)
            {
                Register(record, modules, Compute(KindOf(record), modules));
            }
            else
            {
                // The plan references a part this build no longer ships; keep the
                // drawing but do not pretend the machine exists.
                record.Built = false;
            }
        }
    }

    private static void EnsureCatalog()
    {
        if (_catalogLoaded)
        {
            return;
        }

        _catalogLoaded = true;

        foreach (string path in ResourceFolder.Paths(ModuleFolder))
        {
            var module = ResourceLoader.Load<ModuleData>(path);
            if (module != null && !string.IsNullOrEmpty(module.ModuleId))
            {
                Catalog.Add(module);
            }
        }

        GD.Print($"DesignRegistry: loaded {Catalog.Count} module(s).");
    }
}
