using Godot;

namespace ChronoShift;

/// <summary>
/// Single source of truth for the UI colour language, ported from the
/// "ChronoShift UI" design canvas. Scenes and themes reference these
/// instead of restating hex values, so a palette change lands in one place.
/// </summary>
public static class UiPalette
{
    // --- Blueprint (main menu) — the drafting-table surface -------------------
    public static readonly Color BlueprintTop = new("#17457a");
    public static readonly Color BlueprintMid = new("#123663");
    public static readonly Color BlueprintDeep = new("#0d2a4f");

    /// <summary>Drafting ink on blueprint: borders, grid, rules.</summary>
    public static readonly Color Ink = new("#dce9f7");
    public static readonly Color InkStrong = new("#f2f6fb");
    public static readonly Color InkSoft = new("#b9d2ec");
    public static readonly Color InkMuted = new("#8fb4dc");
    public static readonly Color InkLabel = new("#9fc3e8");
    public static readonly Color InkTable = new("#c6daf0");

    // --- HUD (in-game overlay) ----------------------------------------------
    /// <summary>Translucent slate behind HUD widgets — rgba(22,24,38,.78).</summary>
    public static readonly Color HudPanel = new(0.0706f, 0.2118f, 0.3882f, 0.88f);
    /// <summary>Hairline around HUD widgets — rgba(233,233,237,.14).</summary>
    public static readonly Color HudBorder = new(0.6235f, 0.7647f, 0.9098f, 0.24f);
    public static readonly Color HudText = new("#dce9f7");

    // --- Accents -------------------------------------------------------------
    /// <summary>Time/XP accent — the signature "acceleration" purple.</summary>
    public static readonly Color Accent = new("#9fc3e8");
    public static readonly Color AccentLight = new("#c6daf0");
    public static readonly Color AccentDeep = new("#6f9fd0");
    /// <summary>Currency mark (₳).</summary>
    /// Attention/warning state - deliberately outside the blueprint blues so it
    /// still reads as "something needs you" on a blue HUD.
    public static readonly Color Warning = new("#e8a94e");

    /// Decorative highlight (money, selected slot, name plates).
    public static readonly Color Highlight = new("#dce9f7");

    // --- Panels (tech tree, mission journal, save book, workshop, satchel) ------
    // These were a paper-stock surface; the panels now use the main menu's
    // drafting-table language, so the names describe the role and the values are
    // blueprint tones. Code that asks for "panel ink" keeps working unchanged.
    public static readonly Color Paper = new("#123663");
    public static readonly Color PaperLight = new("#17457a");
    public static readonly Color PaperJournal = new("#0d2a4f");
    public static readonly Color PaperInk = new("#dce9f7");
    public static readonly Color PaperInkSoft = new("#b9d2ec");
    public static readonly Color PaperInkMuted = new("#9fc3e8");
    public static readonly Color PaperInkFaint = new("#8fb4dc");

    /// <summary>"OCHILDI" stamp + unlock buttons — lifted to read on the blue surface.</summary>
    public static readonly Color StampBlue = new("#9fc3e8");
    public static readonly Color StampBlueHover = new("#c6daf0");
    /// <summary>"BAJARILDI" stamp; warm enough to stay legible on blueprint.</summary>
    public static readonly Color StampRed = new("#e2705c");

    /// Positive "completed / confirmed" state. Kept apart from StampRed so a
    /// finished mission never looks like a warning.
    public static readonly Color StampDone = new("#78c8a4");

    // --- Modal (pause, dialogs) ----------------------------------------------
    public static readonly Color ModalBg = new("#1b1e2e");
    /// <summary>Screen dim behind modals — rgba(4,6,10,.74).</summary>
    public static readonly Color ModalScrim = new(0.016f, 0.024f, 0.039f, 0.74f);

    /// <summary>Returns <paramref name="color"/> at <paramref name="alpha"/> — mirrors the design's rgba() ramps.</summary>
    public static Color With(this Color color, float alpha) => new(color.R, color.G, color.B, alpha);
}
