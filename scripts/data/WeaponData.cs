using Godot;

namespace ChronoShift;

/// <summary>Broad weapon family, used for HUD labels and default handling.</summary>
public enum WeaponCategory { Pistol, Rifle, Sniper, Shotgun }

/// <summary>Data-driven definition of a single weapon: stats plus the hand-held model to spawn.</summary>
[GlobalClass]
public partial class WeaponData : Resource
{
    [Export] public string WeaponId = "";
    [Export] public string DisplayName = "";
    [Export] public WeaponCategory Category = WeaponCategory.Pistol;

    [Export] public float Damage = 25.0f;
    [Export] public float Range = 100.0f;

    /// <summary>Rounds per second. With Automatic on, this is the sustained rate while held.</summary>
    [Export] public float FireRate = 3.0f;

    /// <summary>Hold the fire button to keep shooting (rifles). Off = one shot per press.</summary>
    [Export] public bool Automatic;

    /// <summary>Rounds per magazine before a reload is needed.</summary>
    [Export] public int MagazineSize = 12;

    /// <summary>Seconds to reload a fresh magazine.</summary>
    [Export] public float ReloadSeconds = 1.6f;

    /// <summary>What the armoury charges to produce one, for the stats screen.</summary>
    [Export] public int ProductionCost;

    /// <summary>Rays per shot (shotguns fire several); 1 for everything else.</summary>
    [Export] public int PelletsPerShot = 1;

    /// <summary>Half-angle of the bullet spread cone, in degrees. 0 = pin-point accurate.</summary>
    [Export] public float SpreadDegrees;

    /// <summary>When aiming (right mouse), the camera FOV to zoom to. 0 = no scope/zoom.</summary>
    /// <summary>
    /// Draws a rifle scope over the screen while aiming and hides the held model,
    /// the way a scoped weapon reads. Iron-sight weapons only zoom.
    /// </summary>
    [Export] public bool UsesScope;

    [Export] public float AimFov;

    /// <summary>A self-contained prop scene (root Node3D), NOT a raw .fbx/.obj.</summary>
    /// <summary>Fired once per shot. Data, not code - each weapon carries its own.</summary>
    [Export] public AudioStream? FireSound;

    [Export] public PackedScene? ModelScene;

    [Export] public Vector3 GripPosition = Vector3.Zero;
    [Export] public Vector3 GripRotationDegrees = Vector3.Zero;
    [Export] public float GripScale = 1.0f;
}
