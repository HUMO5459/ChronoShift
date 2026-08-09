using System.Collections.Generic;

namespace ChronoShift;

/// <summary>
/// A construction the player drew up in the workshop, stored by module id rather
/// than by object so it survives to disk and back. Kept a plain class — the save
/// file serialises it directly.
/// </summary>
public class DesignRecord
{
    public string Name { get; set; } = "";

    /// <summary>"Vehicle" or "Weapon"; stored as text so the save stays readable.</summary>
    public string Kind { get; set; } = nameof(ModuleKind.Vehicle);

    /// <summary>Fitted modules, in slot order. Resolved against the catalog on load.</summary>
    public List<string> ModuleIds { get; set; } = new();

    /// <summary>True once the design has been produced and is available to use.</summary>
    public bool Built { get; set; }

    /// <summary>Id the produced vehicle/weapon is registered under.</summary>
    public string ProductId => $"design_{Name.ToLowerInvariant().Replace(' ', '_')}";
}
