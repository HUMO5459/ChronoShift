using Godot;

namespace ChronoShift;

/// <summary>
/// Every tuning number in Buxoro 1238, transcribed field for field from
/// Torobiy/src/js/balance.js.
/// </summary>
/// <remarks>
/// The web file opens with "O'yinning BARCHA raqamlari shu yerda. Boshqa faylda
/// sehrli raqam bo'lmasligi kerak" — every number is here, no magic numbers
/// elsewhere. That promise is not actually kept by the web build: roughly forty
/// gameplay values live in world.js, player.js, apprentices.js and combat.js
/// instead. Those are collected in <see cref="BuxoroSehrli"/> rather than being
/// folded in here, so this resource stays a one-to-one mirror of balance.js and
/// can be diffed against it.
///
/// THREE COMMENTS IN balance.js CONTRADICT ITS OWN CODE. The values here follow the
/// CODE, which is what the game actually runs:
///   - <see cref="YollashVaqti"/> is 11.0 s. The controls screen, the itch.io page
///     text and several code comments all say 15 s.
///   - <see cref="DevorUshlabTurish"/> is 6.0 s, not the 5 s the scenario says.
///   - Node stock is 4 for stone and 2 for everything else (world.js:205), not the
///     1 implied by the balance.js comment block.
/// </remarks>
[GlobalClass]
public partial class BuxoroBalance : Resource
{
    // ------------------------------------------------------------------- VAQT

    /// <summary>Starting clock in seconds — 10:00. The only resource in the game.</summary>
    [Export] public float BoshlangichVaqt = 600.0f;

    /// <summary>Seconds the clock jumps BACKWARD per completed wall section.</summary>
    [Export] public float DevorBonus = 25.0f;

    [Export] public int DevorSeksiyaSoni = 6;

    /// <summary>Ceiling the clock cannot exceed — 12:00.</summary>
    [Export] public float MaksimumVaqt = 720.0f;

    // --------------------------------------------------------------- SHOMUROD

    [Export] public float YurishTezligi = 5.0f;
    [Export] public float YugurishTezligi = 7.0f;
    [Export] public float OtTezligi = 10.0f;

    /// <summary>Jump launch speed, giving about 1.1 m of height. On foot only.</summary>
    [Export] public float SakrashTezligi = 4.6f;

    [Export] public float Gravitatsiya = 9.8f;
    [Export] public float BelgilashRadius = 3.0f;
    [Export] public float BelgilashVaqti = 0.2f;
    [Export] public float YasashVaqti = 3.0f;
    [Export] public float QilichYasashVaqti = 4.0f;

    [Export] public float KaltakZarar = 8.0f;
    [Export] public float KaltakCooldown = 0.60f;
    [Export] public float KaltakRadius = 2.0f;

    [Export] public float QilichZarar = 25.0f;
    [Export] public float QilichCooldown = 0.45f;
    [Export] public float QilichRadius = 2.5f;

    /// <summary>
    /// The bow reaches, but slowly. Club and sword stay better in a close fight, so
    /// each weapon keeps a job.
    /// </summary>
    [Export] public float KamonZarar = 18.0f;

    [Export] public float KamonCooldown = 0.90f;
    [Export] public float KamonMasofa = 32.0f;

    /// <summary>Aim cone half-angle in radians, about 23 degrees.</summary>
    [Export] public float KamonKonus = 0.40f;

    /// <summary>Damage multiplier removed by a successful block — 70%.</summary>
    [Export] public float QalqonBlok = 0.70f;

    // ---------------------------------------------------------------- SHOGIRD

    [Export] public int ShogirdBoshlangich = 2;
    [Export] public int ShogirdMaksimum = 6;
    [Export] public float ShogirdTezligi = 4.0f;
    [Export] public float YigishVaqti = 2.0f;

    /// <summary>Units carried per trip. Two, so gathering stays quick.</summary>
    [Export] public int BirSafarOladi = 2;

    /// <summary>
    /// Seconds of hold-E to recruit one craftsman. ELEVEN, not the 15 the controls
    /// screen and the store page claim. This is the hardest decision in the game,
    /// so the real number matters.
    /// </summary>
    [Export] public float YollashVaqti = 11.0f;

    [Export] public float QochishRadiusi = 4.0f;
    [Export] public float QochishDavomiyligi = 5.0f;

    /// <summary>Mark queue length is apprentice count plus this.</summary>
    [Export] public int BelgiNavbatiQoshimcha = 2;

    // ------------------------------------------------------- RESURS TUGUNLARI

    /// <summary>
    /// Node counts per type, IN GENERATION ORDER. Stock is deliberately two to
    /// three times what a run needs, so a first-time player still reaches every
    /// stage. Needed: 24 stone for 6 wall sections, 16 wood plus 8 hide for 8 bows,
    /// 12 iron plus 4 coal for 4 swords. Available: 72 / 32 / 20 / 28 / 16.
    /// </summary>
    [Export] public Godot.Collections.Array<BuxoroNodeCountRow> TugunSoni = new();

    // -------------------------------------------------------------- RETSEPTLAR

    [Export] public Godot.Collections.Array<BuxoroRecipeRow> Retsept = new();

    // --------------------------------------------------------------- DUSHMANLAR

    [Export] public Godot.Collections.Array<BuxoroEnemySpec> Dushman = new();

    /// <summary>Seconds one enemy needs to flatten a full-health wall section.</summary>
    [Export] public float DevorUshlabTurish = 6.0f;

    [Export] public float DevorSeksiyaHp = 140.0f;

    // ----------------------------------------------------------------- TO'LQIN

    [Export] public Godot.Collections.Array<BuxoroWaveRow> Tolqin = new();

    /// <summary>
    /// Enemies that must fall for victory. Must equal the sum of the wave rows;
    /// <see cref="Tekshir"/> asserts it, because a mismatch makes victory either
    /// unreachable or premature and nothing else would report it.
    /// </summary>
    [Export] public int JamiDushman = 24;

    /// <summary>Seconds before a wave that the Humo bird spreads its wings.</summary>
    [Export] public float HumoOgohlantirish = 15.0f;

    // ------------------------------------------------------------- ITTIFOQCHILAR

    [Export] public int AskarSoni = 8;
    [Export] public float AskarHp = 60.0f;

    /// <summary>Range at which an ally's health bar appears (playtest item 5).</summary>
    [Export] public float AskarJonMasofa = 12.0f;

    [Export] public float KamonchiZarar = 12.0f;
    [Export] public float KamonchiOtish = 1.2f;
    [Export] public float KamonchiMasofa = 25.0f;

    [Export] public float QilichbozZarar = 20.0f;
    [Export] public float QilichbozHujum = 0.8f;

    /// <summary>
    /// An unarmed soldier. He resists rather than standing like a statue, but his
    /// damage is tiny and he never charges: arming him is what makes him useful.
    /// </summary>
    [Export] public float QurolsizZarar = 3.0f;

    [Export] public float QurolsizHujum = 1.6f;
    [Export] public float QurolsizMasofa = 2.2f;

    // ---------------------------------------------------------------- VOQEALAR

    /// <summary>Clock value at which Tarobiy is killed — 4:00.</summary>
    [Export] public float TarobiyOlimi = 240.0f;

    [Export] public float ShomurodHp = 140.0f;

    /// <summary>Collision radius on foot (playtest item 7).</summary>
    [Export] public float ToqnashuvRadiusPiyoda = 0.45f;

    /// <summary>Collision radius while mounted.</summary>
    [Export] public float ToqnashuvRadiusOtda = 0.6f;

    // ------------------------------------------------------- BOSHLANG'ICH JOYLAR

    [Export] public Vector2 ShomurodBoshi = new(96.0f, 6.0f);

    /// <summary>12 m east, so he is in frame when the camera faces east.</summary>
    [Export] public Vector2 TarobiyJoyi = new(108.0f, 1.0f);

    // ------------------------------------------------------------------ MAYDON

    [Export] public float MaydonKengligi = 180.0f;
    [Export] public float MaydonBalandligi = 100.0f;
    [Export] public float DevorChizigiX = 60.0f;

    /// <summary>
    /// Spawn band width. Enemies appear at x in [2, 2 + this), i.e. [2, 14).
    /// combat.js:120's comment claiming "0..25 m" contradicts its own code.
    /// </summary>
    [Export] public float SpawnZonasiX = 12.0f;

    [Export] public Vector2 Ustaxona = new(152.0f, -22.0f);
    [Export] public Vector2 Bozor = new(148.0f, 20.0f);
    [Export] public float DarvozaX = 172.0f;

    // ------------------------------------------------------------------ lookup

    public BuxoroEnemySpec? Enemy(string tur)
    {
        foreach (BuxoroEnemySpec spec in Dushman)
        {
            if (spec != null && spec.Tur == tur)
            {
                return spec;
            }
        }

        return null;
    }

    public BuxoroRecipeRow? Recipe(string nomi)
    {
        foreach (BuxoroRecipeRow row in Retsept)
        {
            if (row != null && row.Nomi == nomi)
            {
                return row;
            }
        }

        return null;
    }

    public BuxoroNodeCountRow? NodeType(string tur)
    {
        foreach (BuxoroNodeCountRow row in TugunSoni)
        {
            if (row != null && row.Tur == tur)
            {
                return row;
            }
        }

        return null;
    }

    /// <summary>
    /// Fails loudly on the internal contradictions that would otherwise surface as
    /// an unwinnable run. Called from <see cref="BuxoroRuntime"/> at startup.
    /// </summary>
    public bool Tekshir()
    {
        bool ok = true;

        int waveTotal = 0;
        foreach (BuxoroWaveRow wave in Tolqin)
        {
            if (wave != null)
            {
                waveTotal += wave.Jami;
            }
        }

        if (waveTotal != JamiDushman)
        {
            GD.PushError(
                $"[buxoro] balance: waves sum to {waveTotal} but JamiDushman is {JamiDushman}. " +
                "Victory needs the kill counter to reach JamiDushman, so this makes the run " +
                "unwinnable or endable early.");
            ok = false;
        }

        if (TugunSoni.Count != 5)
        {
            GD.PushError($"[buxoro] balance: expected 5 node types, found {TugunSoni.Count}. " +
                         "Order and count are part of the determinism contract.");
            ok = false;
        }

        if (Retsept.Count != 3 || Dushman.Count != 3)
        {
            GD.PushError("[buxoro] balance: expected 3 recipes and 3 enemy types.");
            ok = false;
        }

        return ok;
    }
}
