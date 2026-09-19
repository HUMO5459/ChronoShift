# BUXORO 1238 — Godot C# port plan

> Generated 2026-09-15 by a 14-agent workflow: 8 codebase readers, 3 independent architectures, 1 judge, 2 adversarial critics. 88 systems mapped, 0 agent errors.

**Mandate.** Literal 1:1 transfer of the Three.js game. Nothing cut, simplified, or replaced with a ChronoShift near-equivalent that behaves differently. ChronoShift CLAUDE.md rule 1 (MVP scope discipline) is explicitly overridden for this port by the user.

**Estimate.** 20-26 weeks.

> **READ THIS FIRST.** Five blocker corrections have been applied and are authoritative. All five blockers were real, and in all five the critic's own proposed fix was wrong or incomplete: one diagnosis was factually wrong, four were incomplete. See the corrections section immediately below.

---

## Blocker corrections — APPLIED 2026-09-15

The two adversarial critics raised 5 blockers. Each was then re-verified against source by its own dedicated agent, across 86 source reads. **All five blockers were real, and in all five the critic's own proposed fix was wrong or incomplete** — one diagnosis was factually wrong (Correction 1), four were incomplete. Every critic fix is superseded by the text here. This section is AUTHORITATIVE and overrides anything later in this document that contradicts it.

| # | Blocker | Verdict on the critic | Status |
|---|---|---|---|
| 1 | Enemy spawn Z — the guard exists in source; the plan dropped it, and the obvious C# fix is an integer-division trap | defect real, critic diagnosis wrong | corrected below |
| 2 | Input Map — ChronoShift binds the arrow keys to jump and run; Buxoro needs them as movement | real, but the critic fix was incomplete or wrong | corrected below |
| 3 | Audio — Sfx.PlayAt adds pitch jitter and 3D attenuation the web game does not have | real, but the critic fix was incomplete or wrong | corrected below |
| 4 | Markup — dialogue and card strings carry HTML the plan never accounted for | real, but the critic fix was incomplete or wrong | corrected below |
| 5 | Determinism gate — the parity harness replays the wrong random stream and cannot pass | real, but the critic fix was incomplete or wrong | corrected below |

The five verifications surfaced **49 further findings** the critics missed. They are listed under each correction.

---

### Correction 1. Enemy spawn Z — the guard exists in source; the plan dropped it, and the obvious C# fix is an integer-division trap

**Verdict on the original finding:** `partly_wrong`

The defect is real but the critic's diagnosis and its proposed fix are both wrong.

WHAT IS TRUE: the plan's mapping row (BUXORO_PORT_PLAN.md:281) transcribes the spawn Z as "z = -38 + (i/(n-1))*76 ±4", which silently DROPS the divide-by-zero guard that exists in the source. That transcription must be corrected, so the blocker earns its place.

WHAT IS FALSE: the critic claims "The source is `-38 + (i / Math.max(1, soni - 1)) * 76 ...` ... a literal port yields 0/0 = NaN." That is self-contradictory. `Math.max(1, soni - 1)` IS the guard, and it is in the shipped source at combat.js:143. For wave 3's lone noyon, soni = 1, so soni - 1 = 0, Math.max(1, 0) = 1, and i/1 = 0/1 = 0. The web game produces z = -38 + jitter. There is no NaN in JS, there never was, and a genuinely literal port of the SOURCE also produces no NaN. The whole downstream consequence chain the critic asserts ("the noyon never reaches a target, is never killed, OlganDushman can never reach 24, victory unreachable") describes a bug that does not exist in the web build. Only a port of the PLAN'S PARAPHRASE would break.

WHAT IS WORSE: the critic's proposed replacement line is itself a new, silent bug. `z = -38f + (i / Mathf.Max(1, soni - 1)) * 76f + ...` — Godot 4.7.1's GodotSharp exposes `Int32 Max(Int32, Int32)`. With `i` and `soni` both int, `Mathf.Max(1, soni - 1)` binds the int overload and `i / <int>` is INTEGER division. I compiled and ran this against GodotSharp 4.7.1 (net8.0). Result for wave 1 (piyoda soni = 6): base z = -38, -38, -38, -38, -38, +38. Five of the six footmen stack on a single point at the south-west corner instead of spreading across the 76 m front. That is a fidelity break far more visible than the imaginary NaN, and it would ship silently because nothing crashes. `(float)i` is mandatory.

ALSO WRONG IN THE FIX: the critic's line drops the `Math.max(-46, Math.min(46, z))` clamp that combat.js:143-144 applies before the value reaches dushmanYarat. The plan's prose mentions the clamp; the critic's code does not.

COLLATERAL CHECK (the "does the same trap exist elsewhere" mandate): NO. combat.js:143 is the only division in the entire web source whose denominator derives from a count, and it is guarded. Every other variable-denominator division is independently guarded: combat.js:298 and 357 and apprentices.js:300 use `Math.hypot(dx,dz) || 1`; apprentices.js:360-364 early-returns on `m < tolerans` before dividing; combat.js:414-418 `continue`s on `u < 1.0` before dividing; player.js:226 is fenced by `if (uz > 0)`; binolar.js:87 and 370 use `Math.max(2, ...)`; models.js:103 uses `|| 1`; anim.js:102 uses a truthiness ternary. There is no second instance of this trap to fix.

NET: keep the blocker, rewrite both its statement and its fix.

<details><summary>Evidence checked — 16 source reads</summary>

- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/combat.js:143` : `        const z = -38 + (i / Math.max(1, soni - 1)) * 76 + (Math.random() - 0.5) * 8;`
  The guard Math.max(1, soni-1) is PRESENT in the shipped source. The critic quoted this exact line and then asserted it produces 0/0. It does not: for soni=1 the denominator is 1, not 0. No NaN is possible in JS for any non-negative soni.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/combat.js:139-147` : `  function tolqinChiqar(scene, tolqin) {     const yangi = [];     const qoy = (tur, soni) => {       for (let i = 0; i < soni; i++) {         const z = -38 + (i / Math.max(1, soni - 1)) * 76 + (Math.random() - 0.5) * 8;         yangi.push(dushmanYarat(scene, tur, Math.max(-46, Math.min(46, z))));       }     };     qoy('piyoda', tolqin.piyoda);`
  Full spawn loop. Two things the critic's fix omits: (a) the clamp Math.max(-46, Math.min(46, z)) is applied to the ARGUMENT of dushmanYarat, not stored in z; (b) when soni === 0 the loop body never runs, so the denominator is unreachable for empty cohorts. Spawn order is strictly piyoda, then otliq, then noyon.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/balance.js:74-80` : `  TOLQIN: [     { vaqt: 480, piyoda: 6, otliq: 0, noyon: 0 },   // 8:00     { vaqt: 330, piyoda: 5, otliq: 3, noyon: 0 },   // 5:30     { vaqt: 150, piyoda: 6, otliq: 3, noyon: 1 }    // 2:30   ],   JAMI_DUSHMAN: 24,`
  The critic is right that wave 3 spawns exactly ONE noyon. Cohorts sum 17 piyoda + 6 otliq + 1 noyon = 24 = JAMI_DUSHMAN. The soni=1 case is real; the NaN it supposedly causes is not. Note also soni=0 appears four times (otliq/noyon in waves 1 and 2), which the loop handles by never iterating.
- `GodotSharp 4.7.1 /Users/humoyunochilov/.nuget/packages/godotsharp/4.7.1/lib/net8.0/GodotSharp.dll — reflection over Godot.Mathf, executed` : `Int32 Max(Int32, Int32) Single Max(Single, Single) Double Max(Double, Double)`
  Mathf.Max has an int overload. With int i and int soni, the critic's `i / Mathf.Max(1, soni - 1)` binds Int32 Max and performs INTEGER division. This is the real bug the fix would introduce.
- `scratchpad/mxchk — compiled and run against GodotSharp 4.7.1, net8.0, arm64` : `Mathf.Max(1, soni-1) runtime type = System.Int32 value=1 CRITIC EXPR wave1 piyoda soni=6 i=0 -> z_base=-38 CRITIC EXPR wave1 piyoda soni=6 i=1 -> z_base=-38 CRITIC EXPR wave1 piyoda soni=6 i=2 -> z_base=-38 CRITIC EXPR wave1 piyoda soni=6 i=3 -> z_base=-38 CRITIC EXPR wave1 piyoda soni=6 i=4 -> z_base=-38 CRITIC EXPR wave1 piyoda soni=6 i=5 -> z_base=38`
  Empirical proof, not reasoning: the critic's exact proposed expression collapses five of six wave-1 footmen onto z=-38. The 76 m spawn front becomes two points.
- `scratchpad/mxchk — same run, corrected expression with (float) cast` : `FIXED    wave1 piyoda soni=6 i=0 -> z_base=-38 FIXED    wave1 piyoda soni=6 i=1 -> z_base=-22.8 FIXED    wave1 piyoda soni=6 i=2 -> z_base=-7.6000004 FIXED    wave1 piyoda soni=6 i=3 -> z_base=7.6000023 FIXED    wave1 piyoda soni=6 i=4 -> z_base=22.8 FIXED    wave1 piyoda soni=6 i=5 -> z_base=38 CRITIC EXPR wave3 noyon soni=1 i=0 -> -38`
  The (float) cast restores the JS values exactly. Note float rounding: -7.6000004 and 7.6000023, so any parity assertion needs a tolerance of 1e-3, not exact equality. Also confirms the lone noyon lands at -38 under BOTH expressions — the soni=1 case was never the broken one.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/combat.js:120-121` : `    // g'arb chekkasidan — spawn zonasi (0..25 m)     const sx = 2 + Math.random() * B.SPAWN_ZONASI_X;`
  Spawn X. SPAWN_ZONASI_X = 12 (balance.js:108), so the real range is [2, 14), NOT the 0..25 m the comment claims. This is a fourth comment-contradicts-code case belonging beside the plan's YollashVaqti 11.0-not-15, DevorUshlabTurish 6.0-not-5 and TugunQoldiTosh 4-not-1 list at plan line 100.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/combat.js:135` : `      olimTaymer: 0, faza: Math.random() * 6`
  Third unseeded draw per enemy. Per-enemy Math.random() order is: z jitter (line 143, drawn in tolqinChiqar BEFORE dushmanYarat is called), then any procedural-fallback draws inside dushmanYarat, then sx (line 121), then faza (line 135). All unseeded, so order is not parity-critical — but the plan's citation 'combat.js:139-142 uses Math.random' (plan lines 269 and 565) points at lines containing no Math.random call at all. The real lines are 121, 135 and 143.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/docs/BUXORO_PORT_PLAN.md:281` : `Spawn x = 2 + GD.Randf()*12 (deliberately unseeded, matching Math.random), z = -38 + (i/(n-1))*76 ±4 clamped to ±46.`
  The actual defective plan text. The guard is missing from '(i/(n-1))'. The x formula and the ±46 clamp are correct. This is the row that must be rewritten.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/docs/BUXORO_PORT_PLAN.md:269` : `Enemy spawn uses GD.Randf, NOT the LCG, because combat.js:139-142 uses Math.random — the split is reproduced deliberately.`
  The task brief asked me to note 'the plan uses a deterministic LCG rather than GD.Randf'. That premise is wrong: the plan ALREADY specifies GD.Randf for enemy spawn and explicitly excludes the LCG, at both line 269 and line 565. The critic's use of GD.Randf is therefore correct and consistent with the plan. Only the line citation needs fixing.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/CLAUDE.md:62` : `- Kod va comment — **English**. O'yin ichidagi matn keyin lokalizatsiya qilinadi (uz/ru/en).`
  House rule requires English code and comments, while the plan uses Uzbek identifiers (TolqinChiqar, DushmanYarat, Soni). The corrected spec below is given in the plan's Uzbek-identifier convention so it drops straight in; the formula is unaffected by whichever naming convention wins that separate dispute.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/combat.js:247` : `          if (Math.abs(n.z - p.z) < 6.6 && p.x > n.x - 4 && p.x < n.x + 2.5) devor = n;`
  Wall attack coverage half-width is 6.6 m around each wall point. With world.js:137 placing points at z = -33 + i*13.2 for i in 0..5 (z = -33, -19.8, -6.6, 6.6, 19.8, 33), coverage is the open interval (-39.6, 39.6). Relevant because it bounds what the spawn formula can actually leak.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/world.js:133-139` : `    // ---- Devor nuqtalari: x=60 chizig'ida 6 seksiya ----     for (let i = 0; i < B.DEVOR_SEKSIYA_SONI; i++) {       dunyo.devorNuqtalari.push({         x: B.DEVOR_CHIZIGI_X,         z: -33 + i * 13.2,`
  Confirms the six wall-point Z values used above. Combined with the corrected spawn formula this shows the plan's step-9 check 'An enemy spawning at z=44' is unreachable: max attainable spawn z is 38 + 4 = 42.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/apprentices.js:300 and combat.js:298 and combat.js:357` : `          const u = Math.hypot(dx, dz) || 1;`
  Collateral scan: every magnitude division in the web source is guarded by `|| 1`. No second divide-by-zero trap of any kind.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/player.js:225-232` : `    const uz = Math.abs(ix) + Math.abs(iz);     if (uz > 0) {       const n = Math.sqrt(ix * ix + iz * iz);`
  The one unguarded-looking normalise (n has no `|| 1`) is fenced by `if (uz > 0)`, so n is never 0. Safe. Confirms the collateral scan found nothing.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/binolar.js:87-88` : `    const soni = Math.max(2, Math.round(uzunlik / 1.9));     const kengligi = uzunlik / (soni * 2 - 1);`
  The only other count-derived denominator in the source. Guarded by Math.max(2,...), so soni>=2 and the denominator is >=3. Not a trap. binolar.js:370 uses the same Math.max(2,...) idiom.

</details>

#### Corrected specification

##### BLOCKER 1 (rewritten) — Enemy spawn Z: the guard is in the source; the plan dropped it, and the obvious C# fix is an integer-division trap

###### Statement of the defect

The plan's mapping row at BUXORO_PORT_PLAN.md:281 transcribes the spawn Z as `z = -38 + (i/(n-1))*76 ±4`, dropping the `Math.max(1, ...)` guard that the source actually has. Implemented literally, wave 3's lone noyon computes `0/0` — `NaN` with float operands, or a `DivideByZeroException` with int operands.

The source itself is NOT broken. combat.js:143 reads, verbatim:

```js
const z = -38 + (i / Math.max(1, soni - 1)) * 76 + (Math.random() - 0.5) * 8;
```

For wave 3's noyon (`soni = 1`): `Math.max(1, 0) = 1`, `0 / 1 = 0`, `z = -38 + jitter`. The web game has never produced NaN here and the victory condition has never been unreachable for this reason. The fix is a transcription fix, not a behaviour change.

###### The C# trap that must not be walked into

`Mathf.Max(1, soni - 1)` with `int soni` binds Godot 4.7.1's `Int32 Max(Int32, Int32)` overload. `i / <int>` is then **integer division**. Measured against GodotSharp 4.7.1, wave 1 (`soni = 6`) yields base Z values `-38, -38, -38, -38, -38, +38` — five of six footmen stacked on one point. Nothing crashes; the 76 m spawn front silently becomes two points. **The numerator must be cast to float.**

###### Replacement text for the mapping row (BUXORO_PORT_PLAN.md:281)

Replace:

> Spawn x = 2 + GD.Randf()*12 (deliberately unseeded, matching Math.random), z = -38 + (i/(n-1))*76 ±4 clamped to ±46.

with:

> Spawn x = `2f + GD.Randf() * SpawnZonasiX` where SpawnZonasiX = 12, giving x ∈ [2, 14) — deliberately unseeded, matching Math.random, and note that combat.js:120's comment claiming "spawn zonasi (0..25 m)" contradicts its own code. Spawn z = `-38f + ((float)i / Mathf.Max(1, soni - 1)) * 76f + (GD.Randf() - 0.5f) * 8f`, then `Mathf.Clamp(z, -46f, 46f)`. Two hazards, both load-bearing: (1) `Mathf.Max(1, soni - 1)` is the source's own guard (combat.js:143) and exists because wave 3 spawns exactly ONE noyon, where `soni - 1 == 0`; (2) `Mathf.Max(int, int)` returns `int`, so the numerator MUST be cast — `(float)i` — or integer division collapses five of wave 1's six piyoda onto z = -38. Cohorts spawn strictly piyoda → otliq → noyon; `soni == 0` never enters the loop, so empty cohorts never reach the denominator.

###### Authoritative C# (step 9, scripts/buxoro/combat/CombatSystem.cs)

```csharp
// Port of combat.js:139-147 (tolqinChiqar).
// Source line 143, verbatim:
//   const z = -38 + (i / Math.max(1, soni - 1)) * 76 + (Math.random() - 0.5) * 8;
//
// TWO separate hazards live in this one expression. Do not "simplify" either.
//
// 1) Math.max(1, soni - 1) is the SOURCE's own divide-by-zero guard. Wave 3
//    (balance.js:78) spawns exactly one noyon, so soni - 1 == 0. The web game
//    is correct as shipped; it is the port plan's paraphrase that dropped this.
//
// 2) Godot's Mathf.Max(int, int) returns int. Without the (float) cast on the
//    numerator this is INTEGER division: wave 1 (soni == 6) would produce base
//    Z values -38,-38,-38,-38,-38,+38 instead of -38,-22.8,-7.6,7.6,22.8,38.
//    Verified empirically against GodotSharp 4.7.1. Nothing throws; the whole
//    76 m spawn front silently collapses to two points.
//
// Randomness: GD.Randf() and NOT Lcg. The web uses unseeded Math.random here
// (combat.js:121, 135, 143) while world/building/grass generation uses the
// seeded LCG (1238 / 8317 / 4711). That split is deliberate and reproduced.
// Never draw from a world Lcg instance in this file: the LCG stream is shared
// world state and one extra draw shifts every downstream node position,
// breaking the step-2 parity gate.

public List<Dushman> TolqinChiqar(TolqinData tolqin)
{
    var yangi = new List<Dushman>();
    // Order is fixed by combat.js:145-147 and is observable in the arrival pattern.
    Qoy(yangi, DushmanTuri.Piyoda, tolqin.Piyoda);
    Qoy(yangi, DushmanTuri.Otliq,  tolqin.Otliq);
    Qoy(yangi, DushmanTuri.Noyon,  tolqin.Noyon);
    return yangi;
}

private void Qoy(List<Dushman> yangi, DushmanTuri tur, int soni)
{
    // soni == 0 (wave 1 otliq/noyon, wave 2 noyon): loop never runs.
    int bolgich = Mathf.Max(1, soni - 1);   // JS: Math.max(1, soni - 1)

    for (int i = 0; i < soni; i++)
    {
        // (float)i is MANDATORY — bolgich is int. See hazard 2 above.
        float z = -38f + ((float)i / bolgich) * 76f + (GD.Randf() - 0.5f) * 8f;

        // JS: Math.max(-46, Math.min(46, z)), applied to the ARGUMENT.
        // Dead for the shipped wave table (|z| never exceeds 42) but ported
        // verbatim: it only binds if the TOLQIN rows are ever retuned.
        z = Mathf.Clamp(z, -46f, 46f);

        yangi.Add(DushmanYarat(tur, z));
    }
}
```

And inside `DushmanYarat`, matching combat.js:121 and 135:

```csharp
float sx = 2f + GD.Randf() * Balans.SpawnZonasiX;   // SpawnZonasiX = 12 -> x in [2, 14)
float syer = BuxoroTerrain.YerBalandligi(sx, z);
// otliq with a horse mesh rides at +1.42; the horse itself sits at syer.
// ...
Faza = GD.Randf() * 6f;
```

###### Ground truth: every Z this formula can produce

Base values before jitter, from the shipped table (balance.js:74-79):

| Wave | clock | cohort | soni | bolgich | base Z values |
|---|---|---|---|---|---|
| 1 | 480 | piyoda | 6 | 5 | -38, -22.8, -7.6, 7.6, 22.8, 38 |
| 1 | 480 | otliq / noyon | 0 | — | loop never runs |
| 2 | 330 | piyoda | 5 | 4 | -38, -19, 0, 19, 38 |
| 2 | 330 | otliq | 3 | 2 | -38, 0, 38 |
| 2 | 330 | noyon | 0 | — | loop never runs |
| 3 | 150 | piyoda | 6 | 5 | -38, -22.8, -7.6, 7.6, 22.8, 38 |
| 3 | 150 | otliq | 3 | 2 | -38, 0, 38 |
| 3 | 150 | **noyon** | **1** | **1** | **-38** ← the case the critic thought was NaN |

Jitter is ±4 exactly. Attainable Z range across the whole game is therefore **[-42, +42]**. The ±46 clamp never binds. Float rounding gives -7.6000004 and 7.6000023 rather than exact ±7.6, so parity assertions need a 1e-3 tolerance.

###### Two knock-on corrections this forces elsewhere in the plan

**(a) Step 9's "Verified by" text is factually impossible as written.** It says: *"An enemy spawning at z=44 walks past the wall line untouched, because coverage stops at ±39.6."* Max attainable Z is 42, so z=44 can never occur. The hole is real but its bounds are different. Wall points sit at z = -33, -19.8, -6.6, 6.6, 19.8, 33 (world.js:137) and combat.js:247 gates the attack on `Math.abs(n.z - p.z) < 6.6`, so coverage is the open interval (-39.6, 39.6). Leakage happens when an edge enemy (base Z ±38) draws jitter beyond ±1.6, i.e. roughly 30% of the time per edge enemy. Replace that sentence with: *"An enemy spawning at |z| > 39.6 — reachable only from the ±38 edge slots with jitter beyond ±1.6, about 30% of the time — walks past the wall line untouched, because coverage stops at ±39.6. The maximum attainable spawn Z is 42, so the ±46 clamp is dead for the shipped table; port it anyway."*

**(b) Fix the Math.random citation at plan lines 269 and 565.** Both cite "combat.js:139-142". No Math.random call exists on those lines (139 is the `function tolqinChiqar(` declaration). The actual unseeded draws are **combat.js:121** (spawn x), **combat.js:135** (faza) and **combat.js:143** (z jitter). Per-enemy draw order is: z jitter first (in TolqinChiqar, before DushmanYarat is entered), then any procedural-fallback draws inside DushmanYarat, then sx, then faza — order is not parity-critical precisely because these are unseeded, and that should be stated so no future reader treats it as a determinism obligation.

###### Collateral scan result — record this so nobody re-runs it

combat.js:143 is the **only** division in the entire web source whose denominator derives from a count, and the source guards it. Every other variable-denominator division is independently safe: `Math.hypot(dx,dz) || 1` (combat.js:298, 357; apprentices.js:300); early return on `m < tolerans` before dividing (apprentices.js:360-364); `continue` on `u < 1.0` before dividing (combat.js:414-418); fenced by `if (uz > 0)` (player.js:225-232); `Math.max(2, ...)` (binolar.js:87-88, 370-374); `|| 1` (models.js:103); truthiness ternary (anim.js:102). **There is no second instance of this trap.** Do not spend a step looking for one.

#### Code

// scripts/buxoro/combat/CombatSystem.cs — step 9
// Port of combat.js:139-147 (tolqinChiqar).
// Source line 143, verbatim:
//   const z = -38 + (i / Math.max(1, soni - 1)) * 76 + (Math.random() - 0.5) * 8;
//
// TWO separate hazards live in this one expression. Do not "simplify" either.
//
// 1) Math.max(1, soni - 1) is the SOURCE's own divide-by-zero guard. Wave 3
//    (balance.js:78) spawns exactly one noyon, so soni - 1 == 0. The web game
//    is correct as shipped; the port plan's paraphrase "(i/(n-1))" dropped it.
//
// 2) Godot's Mathf.Max(int, int) returns int. Without the (float) cast on the
//    numerator this is INTEGER division: wave 1 (soni == 6) produces base Z
//    values -38,-38,-38,-38,-38,+38 instead of -38,-22.8,-7.6,7.6,22.8,38.
//    Verified against GodotSharp 4.7.1. Nothing throws; the 76 m spawn front
//    silently collapses to two points.
//
// Randomness: GD.Randf(), NOT Lcg. combat.js:121/135/143 use unseeded
// Math.random while world/building/grass generation uses the seeded LCG
// (1238 / 8317 / 4711). The split is deliberate. Never draw from a world Lcg
// here: one extra draw shifts every downstream node and breaks step 2.

public List<Dushman> TolqinChiqar(TolqinData tolqin)
{
    var yangi = new List<Dushman>();
    // Order fixed by combat.js:145-147; observable in the arrival pattern.
    Qoy(yangi, DushmanTuri.Piyoda, tolqin.Piyoda);
    Qoy(yangi, DushmanTuri.Otliq,  tolqin.Otliq);
    Qoy(yangi, DushmanTuri.Noyon,  tolqin.Noyon);
    return yangi;
}

private void Qoy(List<Dushman> yangi, DushmanTuri tur, int soni)
{
    // soni == 0 (wave 1 otliq/noyon, wave 2 noyon): loop never runs.
    int bolgich = Mathf.Max(1, soni - 1);   // JS: Math.max(1, soni - 1)

    for (int i = 0; i < soni; i++)
    {
        // (float)i is MANDATORY - bolgich is int. See hazard 2 above.
        float z = -38f + ((float)i / bolgich) * 76f + (GD.Randf() - 0.5f) * 8f;

        // JS: Math.max(-46, Math.min(46, z)), applied to the ARGUMENT.
        // Dead for the shipped wave table (|z| never exceeds 42) but ported
        // verbatim: it only binds if the TOLQIN rows are ever retuned.
        z = Mathf.Clamp(z, -46f, 46f);

        yangi.Add(DushmanYarat(tur, z));
    }
}

// Inside DushmanYarat, matching combat.js:121 and 135:
//   float sx   = 2f + GD.Randf() * Balans.SpawnZonasiX;   // = 12 -> x in [2, 14)
//   float syer = BuxoroTerrain.YerBalandligi(sx, z);
//   Faza       = GD.Randf() * 6f;

#### Test that proves it

Add to step 9's "Verified by". It must be an automated assertion, not a playtest observation, because the integer-division failure is silent — nothing throws and the game remains winnable, so eyeballing a wave will not catch it.

**Setup.** Make the jitter injectable so the test can zero it: give CombatSystem an internal `Func<float> _tasodif = GD.Randf` and have `Qoy` call `_tasodif()`. The test substitutes `() => 0.5f`, which makes `(0.5f - 0.5f) * 8f == 0f` and exposes the pure base values. Do not test through GD.Randf directly — a ±4 jitter band is wide enough to hide the very defect being tested.

**Assertion 1 — the integer-division regression (the one that actually matters).**
`Qoy(DushmanTuri.Piyoda, 6)` with jitter zeroed must yield exactly six distinct Z values:
`-38.0, -22.8, -7.6, 7.6, 22.8, 38.0`, each within 1e-3 (float rounding produces -7.6000004 and 7.6000023, so exact equality fails).
Explicit guard clause: `Assert(zValues.Distinct().Count() == 6)`. The critic's un-cast expression yields `{-38, 38}` — two distinct values — and this single line catches it.

**Assertion 2 — the lone noyon, which is what the blocker was nominally about.**
`Qoy(DushmanTuri.Noyon, 1)` with jitter zeroed must yield exactly one enemy at Z = -38.0 ± 1e-3.
Also assert `!float.IsNaN(z) && !float.IsInfinity(z)` so a future refactor that removes `Mathf.Max` fails here rather than in a playtest. Note in the test comment that the web source never produced NaN here — this assertion is a guard against a port regression, not a reproduction of a source bug.

**Assertion 3 — the remaining table rows.**
`Qoy(Piyoda, 5)` → `-38, -19, 0, 19, 38`. `Qoy(Otliq, 3)` → `-38, 0, 38`. `Qoy(Otliq, 0)` and `Qoy(Noyon, 0)` → empty list, zero draws from `_tasodif` (assert the call count is 0; this proves the empty-cohort path never reaches the denominator).

**Assertion 4 — spawn order and totals.**
`TolqinChiqar` on wave 3 `{Vaqt=150, Piyoda=6, Otliq=3, Noyon=1}` returns 10 enemies whose `Tur` sequence is exactly six Piyoda, then three Otliq, then one Noyon. Summed over all three rows: 17 Piyoda + 6 Otliq + 1 Noyon = 24 = `Balans.JamiDushman`. Since victory is a kill counter, a mismatch makes the win unreachable or premature — this belongs beside the plan's existing `_Ready` cohort-sum assertion.

**Assertion 5 — attainable range, with jitter live.**
Restore `_tasodif = GD.Randf`. Run all three wave rows 10 000 times. Assert every Z satisfies `-42.0001f <= z <= 42.0001f`, and assert `Mathf.Clamp(z, -46f, 46f)` never changed a value (count clamp hits, expect 0). This documents in executable form that the ±46 clamp is dead for the shipped table while keeping it ported. Separately assert that at least one sample per run-set falls outside (-39.6, 39.6) — the wall-coverage hole is real at roughly 30% per edge enemy, and it is source behaviour that must survive, not a bug to close.

**Assertion 6 — LCG isolation.**
Snapshot the world `Lcg` internal state, run `TolqinChiqar` for all three waves, and assert the state is unchanged. Enemy spawning must consume zero LCG draws; if it ever does, every node position after that point shifts and the step-2 parity gate silently starts failing at runtime rather than at load.

#### What the user does in the Godot editor

None. This fix is pure C# inside scripts/buxoro/combat/CombatSystem.cs plus text edits to docs/BUXORO_PORT_PLAN.md. No node tree, no .tscn, no .tres, and no Input Map entry is involved — spawn Z is computed, never authored, and the wave table already lives in the TolqinData rows the plan defines.

One thing to be aware of rather than to do: when the user playtests step 9, the integer-division defect is invisible by eye. Five footmen stacked at z = -38 still walk, still attack the wall, still die, and the game is still winnable — it just plays as one clump instead of a 76 m line. So the step-9 playtest ("Wave 1 spawns exactly 6 piyoda along the west edge at clock 480") must not be treated as covering this; only the automated assertion does. If the user wants a visual check anyway, have them use the parda2 preset to reach wave 1 quickly and look down the west edge: the six footmen must be visibly spread across the full field depth, roughly 15 m apart, not bunched at the south-west corner.

#### Sections of this plan that this correction overrides

- BLOCKER 1 itself (lines 377-379): replace both the finding text and the *Fix:* line. The finding's NaN claim about the source is false, and the proposed `(i / Mathf.Max(1, soni - 1))` fix introduces an integer-division bug that stacks five of six wave-1 piyoda on one point.
- Mapping row 'Wave table + spawn scheduling + enemy types (balance.js:74-81; game.js:513-539; combat.js:77-152)' (line 281): replace 'z = -38 + (i/(n-1))*76 ±4 clamped to ±46' with the guarded, float-cast expression; add the x ∈ [2,14) note.
- Mapping row 'Deterministic LCG + analytic terrain (world.js:9-27)' (line 269): correct the citation 'combat.js:139-142 uses Math.random' to combat.js:121, 135, 143.
- Step 2 deliverable, FILE 3 — Lcg.cs (line 565): same citation correction, 'combat.js:139-142' -> 'combat.js:121/135/143'.
- Step 2 'Verified by' (line 100): add SPAWN_ZONASI_X to the list of constants whose comments contradict the code — combat.js:120 says '0..25 m', the code gives [2,14) — alongside YollashVaqti 11.0-not-15, DevorUshlabTurish 6.0-not-5 and TugunQoldiTosh 4-not-1.
- Step 9 'Verified by' (line 170): the sentence 'An enemy spawning at z=44 walks past the wall line untouched' is impossible — max attainable spawn Z is 42. Rewrite to the |z| > 39.6 / ~30%-per-edge-enemy formulation and add the six new assertions.
- Step 9 'Files' (line 166): if the jitter is made injectable for the test, note the `Func<float> _tasodif` seam on scripts/buxoro/combat/CombatSystem.cs so the test project is not an afterthought.
- Traps/warnings section (near line 347): add a standing entry — 'Mathf.Max(int,int) returns int; any ported JS expression of the form a / Math.max(1, n-1) needs an explicit (float) cast on the numerator or it becomes integer division.' This is a general C#-port hazard, not a one-site fix.
- No change needed to the LCG-vs-GD.Randf decision: the plan already specifies GD.Randf for enemy spawn at lines 269 and 565 and explicitly excludes the LCG. That part is correct as written.

#### Further findings the critics missed — 9

- THE CRITIC'S OWN FIX IS BROKEN, AND SILENTLY SO. `z = -38f + (i / Mathf.Max(1, soni - 1)) * 76f + ...` uses Godot's `Int32 Max(Int32, Int32)` overload, making this integer division. I compiled and ran it against GodotSharp 4.7.1: wave 1 base Z values come out -38, -38, -38, -38, -38, +38. Five of six footmen stack on one point. This is a worse and far more likely fidelity break than the NaN the critic imagined, and nothing throws to reveal it. `(float)i` is mandatory.
- THE CRITIC'S FIX ALSO DROPS THE CLAMP. combat.js:143-144 wraps the value in `Math.max(-46, Math.min(46, z))` before passing it to dushmanYarat. The critic's proposed line has no clamp. The plan's prose mentions '±46' but the replacement code the critic offers would delete it.
- PLAN STEP 9 CONTAINS AN IMPOSSIBLE ACCEPTANCE CRITERION. Line 170 says 'An enemy spawning at z=44 walks past the wall line untouched, because coverage stops at ±39.6.' Max attainable spawn Z is 38 + 4 = 42. z=44 cannot occur. The wall hole is real — coverage is the open interval (-39.6, 39.6) per combat.js:247's `Math.abs(n.z - p.z) < 6.6` against wall points at z = -33, -19.8, -6.6, 6.6, 19.8, 33 (world.js:137) — but it is reached only by the ±38 edge slots drawing jitter beyond ±1.6, roughly 30% of the time per edge enemy. Whoever wrote that criterion will test something unreachable and either mark it passed without testing or spend hours trying to force a spawn that cannot happen.
- THE ±46 CLAMP IS DEAD CODE FOR THE SHIPPED WAVE TABLE. Attainable Z is [-42, 42] across all three waves. Port the clamp anyway — it is source behaviour and becomes live the moment the TOLQIN rows are retuned — but record that it never fires today, so nobody 'verifies' it by trying to observe a clamped spawn.
- THE PLAN CITES A LINE RANGE CONTAINING NO Math.random CALL. Both line 269 and line 565 say 'combat.js:139-142 uses Math.random'. Line 139 is `function tolqinChiqar(scene, tolqin) {`; lines 140-142 are the array declaration, the arrow-function header and the for-loop header. The actual unseeded draws are at combat.js:121 (spawn x), 135 (faza) and 143 (z jitter). The plan's conclusion is right and its evidence pointer is wrong, which is exactly the kind of citation that erodes trust in a document whose whole value is that its citations hold.
- A FOURTH COMMENT-CONTRADICTS-CODE CASE, sitting directly adjacent to the blocker. combat.js:120 comments the spawn zone as '(0..25 m)' while combat.js:121 computes `2 + Math.random() * B.SPAWN_ZONASI_X` with SPAWN_ZONASI_X = 12 (balance.js:108), i.e. x ∈ [2, 14). The plan's step-2 verification already asks the user to read ~110 constants beside balance.js watching for three such contradictions (YollashVaqti 11.0 not 15, DevorUshlabTurish 6.0 not 5, TugunQoldiTosh 4 not 1). This is the fourth and belongs on that list.
- A GENERAL PORT HAZARD WORTH PROMOTING TO THE TRAPS SECTION. The JS idiom `a / Math.max(1, n - 1)` is common in this codebase's geometry and layout code. In JS every number is a double, so the idiom is safe everywhere. In C# the same text becomes integer division wherever both operands are int. This is not a one-site fix — it is a class of defect that will recur through binolar.js and world.js ports. binolar.js:88 (`uzunlik / (soni * 2 - 1)`) and binolar.js:373 (`uzun / n`) are the next two sites where it will bite, both currently safe in JS via `Math.max(2, ...)`.
- COLLATERAL SCAN CAME BACK CLEAN — RECORD THIS SO IT IS NOT REDONE. combat.js:143 is the only count-derived denominator in the web source. Every other variable-denominator division is independently guarded: `|| 1` at combat.js:298, 357 and apprentices.js:300 and models.js:103; early return on `m < tolerans` at apprentices.js:360-364; `continue` on `u < 1.0` at combat.js:414-418; `if (uz > 0)` fence at player.js:225-232; `Math.max(2, ...)` at binolar.js:87-88 and 370-374; truthiness ternary at anim.js:102. There is no second instance of this trap anywhere in the port surface.
- WORTH ADDING TO THE TEST: an LCG-isolation assertion. Nothing in the plan currently prevents a future implementer from reaching for the world Lcg instance for spawn jitter — it is the 'obvious' choice in a document that spends pages on determinism. If they do, every node position after the first wave shifts and the step-2 parity gate starts failing at runtime rather than at load, which is a miserable thing to debug. Snapshot the Lcg state across TolqinChiqar and assert it is unchanged.

---

### Correction 2. Input Map — ChronoShift binds the arrow keys to jump and run; Buxoro needs them as movement

**Verdict on the original finding:** `confirmed_but_critic_incomplete`

The blocker is real and the decoded numbers are exactly right. ChronoShift's `jump` binds physical_keycode 32 (Space) AND 4194320 (KEY_UP), and `run` binds 4194325 (KEY_SHIFT) AND 4194322 (KEY_DOWN). The web game reads ArrowUp/ArrowDown/ArrowLeft/ArrowRight as pure movement aliases for W/S/A/D. So "reuse move_*, jump and run" (plan line 118) ships a game where Up arrow jumps instead of walking forward and Down arrow sprints instead of walking back. Confirmed.

But the critic's PROPOSED FIX is wrong in one important way and incomplete in four others.

(1) Wrong: "add ArrowUp/Down/Left/Right as second events on the four move actions" mutates ChronoShift's OWN shared actions. `move_forward` and `jump` are polled by ChronoShift's existing PlayerController. After that edit, in ChronoShift's own scenes Up arrow would fire move_forward AND jump in the same frame, and Down arrow would fire move_back AND run. The critic fixes Buxoro by breaking the host project. The correct move is the one the critic already applied to sakra/yugur, applied to ALL of it: Buxoro declares a complete, self-contained `buxoro_*` action set including movement, and touches none of ChronoShift's actions. Godot actions are global names, not exclusive key claims — two actions may share a key with zero conflict as long as no single system polls both, so `buxoro_oldinga` (W + Up) can coexist with `jump` (Space + Up) forever, because no Buxoro script ever reads `jump`.

(2) Incomplete: `buxoro_yugur` must be ONE event, physical_keycode 4194325 with "location":0. Godot's KEY_SHIFT with location 0 already matches both physical shift keys — that is exactly what ChronoShift's existing `run` does. "ShiftLeft/ShiftRight only" as two separate events is not how Godot 4 spells it (locations 1 and 2 would be needed, and that is the fragile spelling).

(3) Incomplete — missing keys. The critic's key census stops at the movement block. The web game also reads Digit1/Digit2/Digit3 (game.js:307-311, weapon switch and the sword-choice branch), Escape (main.js:252 pause; videolar.js:74 close cutscene sequence), and Enter (videolar.js:75 advance cutscene). Digit1-3 work even though they are absent from the KUZATILADI preventDefault list, because the keydown handler records EVERY e.code into p.tugmalar — only preventDefault is filtered. The plan covers 1/2/3 as buxoro_qurol_1/2/3 but has no Escape and no Enter action anywhere.

(4) Incomplete — the real remaining "arrow does something else" hazard is Godot's BUILT-IN ui_* actions, which the critic never mentions. ChronoShift's project.godot contains no `ui_` overrides at all, so Godot's defaults are live: ui_up/ui_down/ui_left/ui_right = the four arrows, ui_accept = Space + Enter + KP Enter, ui_cancel = Escape. Those fire on any focused Control. The critic's step-4 check "every arrow key moves and nothing else" will silently FAIL the moment the HUD has a focusable Button, and no Input Map edit fixes it — it is fixed with FocusMode.

(5) Separate fidelity bug the critic missed, in the same sentence of the plan: `buxoro_til (L)` is an invention. The web game has NO language hotkey. The UZ/EN toggle is a CLICK on a HUD element (hud.js:44 `el.til.addEventListener('click', ...)`). Under the 1:1 mandate an added keybinding is as much a deviation as a removed one. Drop buxoro_til; port the clickable toggle.

Everything else in the plan's line-118 list checks out: move_forward/back/left/right really are W/S/A/D only (87/83/65/68) with identical semantics, E=69 and F=70 match KeyE/KeyF, LMB/RMB match player.js:169-173, and 49/50/51 match Digit1/2/3.

<details><summary>Evidence checked — 18 source reads</summary>

- `/Users/humoyunochilov/PROJECTS/ChronoShift/project.godot:59-63` : `jump={ "deadzone": 0.2, "events": [Object(InputEventKey,...,"physical_keycode":32,...), Object(InputEventKey,...,"physical_keycode":4194320,...) ] }`
  CONFIRMED. 4194320 = KEY_SPECIAL(0x400000=4194304) + 0x10 = KEY_UP. ChronoShift's jump is Space OR Up arrow. Reused verbatim, the web game's forward-walk key jumps.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/project.godot:64-68` : `run={ "deadzone": 0.2, "events": [Object(InputEventKey,...,"physical_keycode":4194325,...), Object(InputEventKey,...,"physical_keycode":4194322,...) ] }`
  CONFIRMED. 4194325 = 4194304+0x15 = KEY_SHIFT (location 0, so both shift keys). 4194322 = 4194304+0x12 = KEY_DOWN. ChronoShift's run is Shift OR Down arrow. Reused verbatim, the web game's back-walk key sprints.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/project.godot:39-58` : `move_forward={..."physical_keycode":87...}  move_back={..."physical_keycode":83...}  move_left={..."physical_keycode":65...}  move_right={..."physical_keycode":68...}`
  The four move actions are W/S/A/D ONLY — one event each, no arrow aliases. They match the web's WASD half exactly but carry none of the arrow half. This is why the critic wanted to append arrows to them; doing so is what would break ChronoShift's own jump/run.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/player.js:211-214` : `if (T.KeyW || T.ArrowUp)    iz += 1;     if (T.KeyS || T.ArrowDown)  iz -= 1;     if (T.KeyA || T.ArrowLeft)  ix -= 1;     if (T.KeyD || T.ArrowRight) ix += 1;`
  CONFIRMED verbatim. The arrows are pure movement aliases for WASD — identical weight, no modifier, no separate code path. Four move actions with two events each reproduce this exactly.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/player.js:113-114` : `const KUZATILADI = ['KeyW','KeyA','KeyS','KeyD','KeyE','KeyF','Space',                         'ShiftLeft','ShiftRight','ArrowUp','ArrowDown','ArrowLeft','ArrowRight'];`
  The 13 codes the web preventDefaults so the itch.io iframe does not scroll. This is NOT the full input census — see the next entry.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/player.js:116-122` : `window.addEventListener('keydown', e => {       // itch.io iframe: Space va strelkalar sahifani skroll qilmasin       if (KUZATILADI.indexOf(e.code) !== -1) e.preventDefault();       if (e.repeat) return;       p.tugmalar[e.code] = true;       if (e.code === 'KeyE') p.eBosildi = true;       if (e.code === 'Space') p.spaceBosildi = true;`
  Two things. (a) p.tugmalar[e.code] = true runs for EVERY key, unfiltered — which is why Digit1/2/3 work despite being absent from KUZATILADI. Anyone who derives the key list from KUZATILADI alone drops the weapon keys. (b) `if (e.repeat) return` means OS key-repeat never re-fires a press — the exact semantics of Godot's IsActionJustPressed.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/player.js:219-220` : `const yuguradi = T.ShiftLeft || T.ShiftRight ||                      (p.analog && p.analog.kuch > 0.75);`
  Run is a HOLD on either shift key, OR touch-joystick magnitude above 0.75. Godot: Input.IsActionPressed("buxoro_yugur") || Analog.Kuch > 0.75. One KEY_SHIFT event with location 0 covers both shifts.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/player.js:274-278` : `// ---- SAKRASH: Space (faqat piyoda) ----     if (p.tugmalar.Space && !p.otda && !p.sakramoqda) {       p.sakramoqda = true;       p.vy = B.SAKRASH_TEZLIGI;       p.tugmalar.Space = false;        // ushlab turilsa qayta-qayta sakramasin`
  Jump is Space, on foot only (!p.otda = not mounted), consumed on use so holding does not re-jump. Maps to IsActionJustPressed("buxoro_sakra") && !Otda && !Sakramoqda. No arrow key involved anywhere.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/game.js:307-313` : `if (p.tugmalar.Digit1) { p.tugmalar.Digit1 = false; qilichBer(O, 'ozim'); }       if (p.tugmalar.Digit2) { p.tugmalar.Digit2 = false; qilichBer(O, 'askar'); }     } else {       // --- Qurol almashtirish: 1 kaltak · 2 kamon · 3 qilich ---       [['Digit1', 'kaltak'], ['Digit2', 'kamon'], ['Digit3', 'qilich']].forEach(([k, q]) => {`
  Digit1/2/3 are real gameplay keys with a mode-dependent meaning (sword choice while O.qilichTanlov, otherwise weapon switch), and each is consumed on read — just-pressed semantics. The critic's key census omitted them entirely.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/main.js:251-256` : `window.addEventListener('keydown', e => {     if (e.code !== 'Escape' || !ishlaydi || !O || O.tugadi) return;     if (window.VIDEOLAR.ochiqmi()) return;      // Esc videoni yopadi (videolar.js)     e.preventDefault();     pauza ? pauzaYop() : pauzaOch();   });`
  Escape = pause toggle, explicitly suppressed while a cutscene is open. A fifteenth key the plan's Input Map list does not mention. 4194305 = KEY_ESCAPE.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/videolar.js:72-78` : `window.addEventListener('keydown', e => {       if (!joriy) return;       if (e.code === 'Escape') { e.preventDefault(); e.stopPropagation(); yopish(); }       else if (e.code === 'Space' || e.code === 'Enter') {         e.preventDefault(); e.stopPropagation(); keyingisi();       }     }, true);`
  Enter is a sixteenth key (advance cutscene), and this listener is registered in the CAPTURE phase with stopPropagation — so while a cutscene is playing, Escape/Space/Enter never reach player.js or main.js. That capture-phase priority is behaviour that must be ported, not just the keybinding. e.code === 'Enter' is the main Enter only (numpad is 'NumpadEnter'), so KEY_ENTER 4194309 and NOT KEY_KP_ENTER 4194310.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/main.js:163-164` : `if (O.oyinchi.spaceBosildi || n >= 1) {       O.oyinchi.spaceBosildi = false;`
  Space has a third job: skipping the opening camera fly-in. Same physical key, third consumer, resolved by game state. buxoro_sakra must be readable by the intro controller too.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/player.js:168-173` : `dom.addEventListener('mousedown', e => {       if (e.button === 0) { lockSora(); p.urmoqchi = true; }       if (e.button === 2) p.bloklaymi = true;     });     document.addEventListener('mouseup', e => {       if (e.button === 2) p.bloklaymi = false;     });`
  LMB = attack (edge-triggered, plus a pointer-lock request), RMB = block (held, released on mouseup). The plan's buxoro_urish (LMB) / buxoro_blok (RMB) are correct as written. Note the release is bound on `document`, not `dom`, so a release outside the canvas still drops the shield.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/player.js:202` : `window.addEventListener('blur', () => { p.tugmalar = {}; p.bloklaymi = false; });`
  Focus loss clears every held key and drops the shield, so keys never stick. Godot needs the explicit equivalent on NOTIFICATION_APPLICATION_FOCUS_OUT — Godot does not guarantee synthetic key-ups on focus loss.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/main.js:192` : `if (O.oyinchi) { O.oyinchi.tugmalar = {}; O.oyinchi.bloklaymi = false; }`
  Opening the pause menu also clears held keys and the shield — the same reset as blur. Must be ported into the pause path, otherwise the player resumes mid-sprint or mid-block.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/hud.js:44-46` : `el.til.addEventListener('click', () => {       til = til === 'uz' ? 'en' : 'uz';       el.til.textContent = til === 'uz' ? 'UZ / en' : 'uz / EN';`
  REFUTES the plan's buxoro_til (L). The language toggle is a CLICK on a HUD element. There is no L key and no language hotkey anywhere in the web game — grep for KeyL across src/js returns nothing. An added hotkey violates the 1:1 mandate in the same direction as a dropped one.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/project.godot:37-180` : `[input]  move_forward={ ... designer={ ... }  [display]`
  The complete [input] section holds 21 actions and contains NO ui_* override of any kind. Godot 4.7's built-in defaults are therefore live and unmodified: ui_up/ui_down/ui_left/ui_right = the four arrow keys, ui_accept = Space + Enter + KP Enter, ui_cancel = Escape. Any focusable Control in the Buxoro HUD will consume arrows and Space. The critic never mentions this, yet it is what will actually break their proposed step-4 check.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/docs/BUXORO_PORT_PLAN.md:118` : `In Project Settings → Input Map add, all by physical_keycode: buxoro_belgila (E), buxoro_yasa (F), buxoro_urish (LMB), buxoro_blok (RMB), buxoro_qurol_1/2/3, buxoro_til (L); reuse move_*, jump and run.`
  The exact plan text under review. Confirms the critic's quotation. Three defects in one sentence: the jump/run reuse, the missing arrow/Escape/Enter coverage, and the invented buxoro_til (L).

</details>

#### Corrected specification

##### Step 4 — Input Map (AUTHORITATIVE, replaces the Input Map sentence on plan line 118)

**Principle.** Buxoro declares a COMPLETE, SELF-CONTAINED `buxoro_*` action set and reads nothing else. ChronoShift's existing actions (`move_forward`, `move_back`, `move_left`, `move_right`, `jump`, `run`, `interact`, `weapon_1..5`, …) are NOT reused, NOT edited, and NOT deleted. Rationale: ChronoShift binds `jump` to Space AND Up arrow (project.godot:59-63) and `run` to Shift AND Down arrow (project.godot:64-68), while the web game uses the arrows as plain movement aliases (player.js:211-214). Appending arrows to `move_*` — the obvious-looking fix — would make Up arrow fire `move_forward` AND `jump` inside ChronoShift's own scenes. Godot action names are global labels, not exclusive key claims: `buxoro_oldinga` (W + Up) and `jump` (Space + Up) coexist harmlessly because no Buxoro script ever polls `jump`.

###### The 15 actions

| Action | Events (physical) | Read as | Web source |
|---|---|---|---|
| `buxoro_oldinga` | W `87`, Up `4194320` | Pressed | player.js:211 |
| `buxoro_orqaga` | S `83`, Down `4194322` | Pressed | player.js:212 |
| `buxoro_chapga` | A `65`, Left `4194319` | Pressed | player.js:213 |
| `buxoro_ongga` | D `68`, Right `4194321` | Pressed | player.js:214 |
| `buxoro_sakra` | Space `32` | JustPressed | player.js:275, main.js:163 |
| `buxoro_yugur` | Shift `4194325`, location 0 | Pressed | player.js:219 |
| `buxoro_belgila` | E `69` | JustPressed + Pressed (hold timer) | player.js:121, 353 |
| `buxoro_yasa` | F `70` | Pressed (hold timer only) | player.js:354 |
| `buxoro_urish` | MouseButton `1` (left) | JustPressed | player.js:169 |
| `buxoro_blok` | MouseButton `2` (right) | Pressed | player.js:170,173 |
| `buxoro_qurol_1` | `49` | JustPressed | game.js:307,311 |
| `buxoro_qurol_2` | `50` | JustPressed | game.js:308,311 |
| `buxoro_qurol_3` | `51` | JustPressed | game.js:311 |
| `buxoro_pauza` | Escape `4194305` | JustPressed | main.js:252, videolar.js:74 |
| `buxoro_video_keyingi` | Space `32`, Enter `4194309` | JustPressed | videolar.js:75 |

Totals for the assertion test: **15 actions, 18 key events, 2 mouse events, 20 events.**

Deliberate key sharing, all resolved by game state, never by the Input Map:
- Space is in `buxoro_sakra` and `buxoro_video_keyingi`. Cutscene open wins (videolar.js registers in the capture phase with stopPropagation).
- Escape (`buxoro_pauza`) closes an open cutscene instead of pausing (main.js:253 `if (window.VIDEOLAR.ochiqmi()) return;`).
- `buxoro_qurol_1/2/3` double as the sword-choice keys while `QilichTanlov` is true — the JS is an else-branch (game.js:306-316), so weapon switching is genuinely unavailable during the decision.
- `buxoro_belgila` (E=69) shares a key with ChronoShift's `interact`, and `buxoro_qurol_1/2/3` with `weapon_1/2/3`. Harmless and intentional: no Buxoro script reads the ChronoShift names, no ChronoShift script reads the Buxoro names.

###### `buxoro_til` is DELETED from the plan
There is no language hotkey in the web game. hud.js:44 toggles UZ/EN on a **click** of the `til` HUD element. `grep -rn "KeyL" src/js/` returns nothing. Port the clickable toggle in the HUD (Step for hud), not an Input Map action. Adding an L binding violates the 1:1 mandate exactly as much as dropping a key would.

###### project.godot [input] text to append (verbatim, deadzone 0.2 to match the file's convention)

```
buxoro_oldinga={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":87,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null), Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194320,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
buxoro_orqaga={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":83,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null), Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194322,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
buxoro_chapga={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":65,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null), Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194319,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
buxoro_ongga={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":68,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null), Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194321,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
buxoro_sakra={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":32,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
buxoro_yugur={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194325,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
buxoro_belgila={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":69,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
buxoro_yasa={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":70,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
buxoro_urish={
"deadzone": 0.2,
"events": [Object(InputEventMouseButton,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"button_mask":0,"position":Vector2(0, 0),"global_position":Vector2(0, 0),"factor":1.0,"button_index":1,"canceled":false,"pressed":false,"double_click":false,"script":null)
]
}
buxoro_blok={
"deadzone": 0.2,
"events": [Object(InputEventMouseButton,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"button_mask":0,"position":Vector2(0, 0),"global_position":Vector2(0, 0),"factor":1.0,"button_index":2,"canceled":false,"pressed":false,"double_click":false,"script":null)
]
}
buxoro_qurol_1={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":49,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
buxoro_qurol_2={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":50,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
buxoro_qurol_3={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":51,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
buxoro_pauza={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194305,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
buxoro_video_keyingi={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":32,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null), Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194309,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
```

Keycode decode table for the reviewer (Godot 4 `Key`, KEY_SPECIAL = 0x400000 = 4194304): Left 4194319, Up 4194320, Right 4194321, Down 4194322, Shift 4194325, Escape 4194305, Enter 4194309 (KP_ENTER 4194310 is deliberately EXCLUDED — the web's `e.code === 'Enter'` does not match the numpad key).

###### The built-in ui_* hazard (not fixable in the Input Map)
ChronoShift's project.godot has no `ui_` override, so Godot's defaults are live: `ui_up/ui_down/ui_left/ui_right` = the four arrows, `ui_accept` = Space + Enter + KP Enter, `ui_cancel` = Escape. These fire on any **focused Control**. Mandatory countermeasures, both cheap:
1. Every Control in the Buxoro gameplay HUD gets `focus_mode = 0` (FocusModeEnum.None) and, unless it is a touch button, `mouse_filter = 2` (Ignore). A HUD with a focusable Button turns the arrow keys back into UI navigation.
2. Only the pause menu takes focus, and it takes focus only while paused — and while paused the player is not simulated, mirroring main.js:192 which clears `tugmalar` and `bloklaymi` on pause.
Do NOT "solve" this by erasing Godot's ui_* defaults; the pause menu and the main menu need them.

###### Focus-loss reset (player.js:202, main.js:192)
`BuxoroInput` must handle `NOTIFICATION_APPLICATION_FOCUS_OUT` / `NOTIFICATION_WM_WINDOW_FOCUS_OUT` by clearing all held state and setting `Bloklaymi = false`. Godot does not guarantee synthetic key-ups on focus loss, so without this a key held at alt-tab stays held. The pause path calls the same reset.

###### Escape and mouse capture
The web relies on the browser releasing pointer lock on Escape and treats `pointerlockchange` as a pause signal (main.js:260-264). Godot has no such event: the `buxoro_pauza` handler must itself set `Input.MouseMode = Input.MouseModeEnum.Visible` when opening the pause menu and back to `Captured` on resume. Exported builds do not release capture on Escape by themselves, so omitting this leaves the cursor trapped.

###### Read-order priority (mirrors the capture-phase listener at videolar.js:78)
Per frame, in this order, first consumer wins: (1) Videolar open → `buxoro_pauza` closes the whole sequence, `buxoro_video_keyingi` advances, nothing else is read; (2) paused → only `buxoro_pauza`; (3) opening fly-in active → `buxoro_sakra` skips it (main.js:163); (4) normal gameplay → everything else. `buxoro_sakra` is consumed for the jump (`Tugmalar` flag cleared, player.js:278) so a held Space cannot re-jump; Godot's `IsActionJustPressed` gives this for free because Input does not report echo events as just-pressed.

###### Touch (Step 13, touch.js)
Touch writes the same fields keyboard/mouse write and declares no Input Map actions at all: `Analog{X,Z,Kuch}` (touch.js:57-59), `Tugmalar[KeyE/KeyF]` (touch.js:164-167), `Urmoqchi`, `Bloklaymi`, plus `SpaceBosildi = true` on any touchstart (touch.js:83). Analog input overrides the keyboard vector (player.js:217) and `Kuch > 0.75` substitutes for `buxoro_yugur`. There is no touch jump — correct and 1:1: the web has none.

#### Code

// scripts/buxoro/input/BuxoroActions.cs
// Action NAMES only. No keycodes here: bindings live in project.godot's [input],
// authored in the editor. Nothing in scripts/buxoro may name a ChronoShift action.
namespace ChronoShift.Buxoro.Input;

public static class BuxoroActions
{
    public const string Oldinga      = "buxoro_oldinga";       // W  + Up arrow
    public const string Orqaga       = "buxoro_orqaga";        // S  + Down arrow
    public const string Chapga       = "buxoro_chapga";        // A  + Left arrow
    public const string Ongga        = "buxoro_ongga";         // D  + Right arrow
    public const string Sakra        = "buxoro_sakra";         // Space
    public const string Yugur        = "buxoro_yugur";         // Shift (either)
    public const string Belgila      = "buxoro_belgila";       // E
    public const string Yasa         = "buxoro_yasa";          // F
    public const string Urish        = "buxoro_urish";         // LMB
    public const string Blok         = "buxoro_blok";          // RMB
    public const string Qurol1       = "buxoro_qurol_1";       // 1
    public const string Qurol2       = "buxoro_qurol_2";       // 2
    public const string Qurol3       = "buxoro_qurol_3";       // 3
    public const string Pauza        = "buxoro_pauza";         // Escape
    public const string VideoKeyingi = "buxoro_video_keyingi"; // Space or Enter
}

// scripts/buxoro/input/BuxoroInput.cs
using Godot;

namespace ChronoShift.Buxoro.Input;

public partial class BuxoroInput : Node
{
    // Touch writes these same fields, so no gameplay code branches on platform.
    public Vector2 Analog;          // X = right, Z in Analog.Y = forward (touch.js:57-59)
    public float   AnalogKuch;      // touch.js:59
    public bool    Bloklaymi;       // player.js:170,173
    public bool    Urmoqchi;        // player.js:169
    public bool    EBosildi;        // player.js:121
    public bool    SpaceBosildi;    // player.js:122, consumed by the intro (main.js:163)
    public float   EUshlandi;       // player.js:353
    public float   FUshlandi;       // player.js:354

    // player.js:211-214 — arrows are plain aliases for WASD, identical weight.
    public Vector2 HarakatVektori()
    {
        float ix = 0f, iz = 0f;
        if (Godot.Input.IsActionPressed(BuxoroActions.Oldinga)) iz += 1f;
        if (Godot.Input.IsActionPressed(BuxoroActions.Orqaga))  iz -= 1f;
        if (Godot.Input.IsActionPressed(BuxoroActions.Chapga))  ix -= 1f;
        if (Godot.Input.IsActionPressed(BuxoroActions.Ongga))   ix += 1f;
        // player.js:217 — analog input OUTRANKS the keyboard, it does not sum with it.
        if (AnalogKuch > 0f && (Analog.X != 0f || Analog.Y != 0f)) return Analog;
        return new Vector2(ix, iz);
    }

    // player.js:219-220
    public bool Yuguradi() =>
        Godot.Input.IsActionPressed(BuxoroActions.Yugur) || AnalogKuch > 0.75f;

    // player.js:275-278 — Godot's IsActionJustPressed already ignores echo events,
    // which is what `if (e.repeat) return` + `tugmalar.Space = false` buy in JS.
    public bool SakraBosildi() => Godot.Input.IsActionJustPressed(BuxoroActions.Sakra);

    // player.js:202 and main.js:192 — Godot does not guarantee key-ups on focus loss.
    public override void _Notification(int what)
    {
        if (what == (int)NotificationApplicationFocusOut ||
            what == (int)NotificationWMWindowFocusOut)
            HammaTugmalarniTozala();
    }

    public void HammaTugmalarniTozala()
    {
        Analog = Vector2.Zero;
        AnalogKuch = 0f;
        Bloklaymi = false;
        Urmoqchi = false;
        EUshlandi = 0f;
        FUshlandi = 0f;
        // Drops every held action so nothing sticks across a focus change or pause.
        Godot.Input.FlushBufferedEvents();
    }
}

#### Test that proves it

Two mechanical tests plus one manual matrix.

**Test 1 — `InputMapTests.BuxoroActionsAreExactAndSelfContained` (C#, runs headless).**
Assert exactly, against `InputMap.GetActions()` filtered to the `buxoro_` prefix:
- Count of `buxoro_*` actions == **15**.
- Per action, the multiset of `InputEventKey.PhysicalKeycode` / `InputEventMouseButton.ButtonIndex` equals: oldinga {87, 4194320}; orqaga {83, 4194322}; chapga {65, 4194319}; ongga {68, 4194321}; sakra {32}; yugur {4194325}; belgila {69}; yasa {70}; urish {MouseButton.Left}; blok {MouseButton.Right}; qurol_1 {49}; qurol_2 {50}; qurol_3 {51}; pauza {4194305}; video_keyingi {32, 4194309}.
- Totals across all 15: **18 InputEventKey events + 2 InputEventMouseButton events = 20**.
- Every `InputEventKey` has `Keycode == 0` (physical-only, per CLAUDE.md) and `Location == 0`.
- No `buxoro_*` action contains keycode 4194310 (KP_ENTER).
- Regression guard on the host project: `jump` still has exactly {32, 4194320}, `run` still has exactly {4194325, 4194322}, and `move_forward/back/left/right` still have exactly one event each ({87}/{83}/{65}/{68}). This is the assertion that fails loudly if someone later "helpfully" implements the critic's original arrow-on-move_* suggestion.

**Test 2 — `BuxoroDoesNotPollChronoShiftActions` (source grep test).**
Over `scripts/buxoro/**/*.cs`, assert zero matches for the string literals `"move_forward"`, `"move_back"`, `"move_left"`, `"move_right"`, `"jump"`, `"run"`, `"interact"`, `"weapon_1"`…`"weapon_5"`, `"ui_accept"`, `"ui_cancel"`, `"ui_up"`, `"ui_down"`, `"ui_left"`, `"ui_right"`. Expected: **0 matches**. Also assert every `Input.IsAction*` string literal in that tree is one of the 15 names above (set equality, so a typo'd action name fails too).

**Test 3 — manual matrix, replaces the critic's "every arrow key moves and nothing else".**
On foot, standing still, camera facing +Z, cutscenes finished, with the HUD visible:
| Key held 1 s | Expected | Would-be failure |
|---|---|---|
| Up arrow | walks forward, identical distance to W (5.0 m/s walk), Y stays 0.00 | plays `jump` clip / Y rises |
| Down arrow | walks backward, identical to S, speed 5.0 m/s | speed 7.0 m/s (run) |
| Left / Right arrow | strafes, identical to A / D | UI focus moves |
| Shift + W | 7.0 m/s, `run` clip | 5.0 m/s |
| Space (on foot) | one jump, one only while held | repeats, or nothing |
| Space (mounted, `Otda`) | nothing at all (player.js:275) | jumps while mounted |
| Escape | pause opens, cursor becomes visible | cursor stays captured, or UI focus ring appears |
| Escape during a cutscene | closes the cutscene sequence, does NOT pause | pause opens behind the video |
| Space / Enter during a cutscene | advances to the next clip, player does NOT jump | jump audio under the video |
| 1 / 2 / 3 during sword choice | picks self/soldier; 3 does nothing | weapon switches instead |
| Alt-tab while holding W, then return | player is stopped | keeps walking |
Measure the two forward distances with a 1-second scripted hold and assert equality to **0.01 m** (they must be byte-identical code paths, not merely similar).

#### What the user does in the Godot editor

Three things in the Godot editor, all in the ChronoShift project.

**1. Add the 15 Buxoro actions (Project Settings → Input Map).**
Two ways; the second is faster and less error-prone.
- (a) By hand: untick "Show Built-in Actions", type each action name, then **+ Add Event → Listen for Input**, press the key, and in the event's context menu tick **"Physical Key"** (mandatory per CLAUDE.md — Godot records a Keycode by default, which breaks on AZERTY/Dvorak). For `buxoro_urish` / `buxoro_blok` use **+ → Mouse Button → Left / Right**. Leave every Deadzone at 0.2.
- (b) Faster: close the Godot editor, paste the `[input]` block from correctedSpec at the END of the existing `[input]` section in `/Users/humoyunochilov/PROJECTS/ChronoShift/project.godot` (after the `designer=` entry, before `[display]`), reopen the editor, and confirm all 15 appear under Input Map. Godot rewrites this file on close, so the editor must not be running while you edit it.

**2. Do NOT touch the existing actions.** `jump`, `run`, `move_forward`, `move_back`, `move_left`, `move_right`, `interact`, `weapon_1`…`weapon_5` stay exactly as they are, arrow-key events included. ChronoShift's own PlayerController polls them. If anyone suggests removing Up from `jump` or Down from `run`, or appending arrows to `move_*`, refuse — Buxoro no longer reads any of these names.

**3. When building the Buxoro HUD scene (Step 10):** select every Control on the HUD CanvasLayer and set **Focus → Mode = None** and **Mouse → Filter = Ignore** (leave Filter on Stop for the touch buttons in Sensor.tscn, which must receive presses). Godot's built-in `ui_up`/`ui_down`/`ui_left`/`ui_right`/`ui_accept` are bound to the arrows, Space and Enter by engine default and are not visible in project.godot — a single focusable HUD Button will silently steal the arrow keys back from movement, and no Input Map change can prevent it.

**Verification after step 1:** in Input Map, tick "Show Built-in Actions" and confirm `ui_up` still shows the Up arrow and `jump` still shows Space + Up. Both SHOULD still be bound — that overlap is intentional and harmless, and seeing it confirms you edited nothing you should not have.

#### Sections of this plan that this correction overrides

- Line 118, Step 4 'User does (Godot editor)' — the sentence 'In Project Settings → Input Map add, all by physical_keycode: buxoro_belgila (E), buxoro_yasa (F), buxoro_urish (LMB), buxoro_blok (RMB), buxoro_qurol_1/2/3, buxoro_til (L); reuse move_*, jump and run.' is DELETED and replaced by the 15-action table above. 'reuse move_*, jump and run' is removed outright; 'buxoro_til (L)' is removed outright.
- Line 114, Step 4 Deliverable — 'BuxoroInput reading only Input Map actions' must be tightened to 'reading only the 15 buxoro_* actions; referencing any ChronoShift action name is a build failure (InputMapTests Test 2)'. Add the focus-loss reset and the Input.MouseMode handoff to the deliverable.
- Line 116, Step 4 Files — add scripts/buxoro/input/BuxoroActions.cs (the name constants) and tests/buxoro/InputMapTests.cs to the file list alongside Player.cs and BuxoroInput.cs.
- Step 4 acceptance criteria — add the three tests from testToAdd, replacing the critic's proposed 'every arrow key moves and nothing else' with the full matrix (that check as worded cannot pass while any HUD Control is focusable).
- New subsection under Step 4: 'Godot built-in ui_* actions' — the arrows, Space, Enter and Escape are bound by engine default and ChronoShift overrides none of them; every Buxoro gameplay HUD Control gets focus_mode = 0 and mouse_filter = 2.
- Mapping row line 284 (shield blocking) — 'Right mouse (new Input Map action buxoro_blok)' is correct and unchanged; add the note that the release is bound on document, not the canvas (player.js:172-174), so a mouse-up outside the viewport still drops the shield.
- Mapping row line 293 (sword choice) — restate that the rebound keys are buxoro_qurol_1 and buxoro_qurol_2, and that buxoro_qurol_3 is inert while QilichTanlov is true (game.js is an else-branch).
- Mapping row line 306 (cutscene video system) — add the concrete skip bindings buxoro_pauza (Escape, closes the whole sequence) and buxoro_video_keyingi (Space or Enter, advances), and the capture-phase priority rule from videolar.js:78 that suppresses jump/pause while a cutscene is open.
- The HUD step — the UZ/EN toggle is a CLICK on the til element (hud.js:44), not a hotkey. Any mention of an L keybinding is removed from the plan.
- Pause/main-loop step — Escape must set Input.MouseMode = Visible on open and Captured on resume (Godot has no pointerlockchange), and the pause path clears held input and Bloklaymi, mirroring main.js:192.
- Step 13 touch (lines 254-256) — add the explicit statement that Touch.cs declares NO Input Map actions and writes the same fields keyboard/mouse write, including SpaceBosildi on touchstart (touch.js:83) and the Kuch > 0.75 run substitute (player.js:219-220).
- Blocker list item 2 (lines 381-383) — mark resolved, and record that the critic's proposed fix was amended: arrows go on new buxoro_* movement actions, NOT on ChronoShift's shared move_* actions.

#### Further findings the critics missed — 11

- The critic's own fix would break ChronoShift. Appending ArrowUp to `move_forward` while `jump` keeps ArrowUp (project.godot:59-63) makes Up arrow fire move_forward AND jump in the same frame inside ChronoShift's existing scenes, and Down arrow fire move_back AND run. Right diagnosis, fix aimed at the wrong actions. Buxoro must declare its own movement actions and leave the host project's Input Map untouched.
- `buxoro_til (L)` on plan line 118 is invented. The web game has no language hotkey — hud.js:44 toggles UZ/EN on a CLICK of the `til` HUD element, and `grep -rn "KeyL" src/js/` returns nothing. Under the 1:1 mandate, adding a binding is as much a deviation as dropping one. Delete the action; port the clickable toggle.
- Escape is missing from the plan's Input Map entirely, and it has two jobs: pause toggle (main.js:251-256) and closing an open cutscene sequence (videolar.js:74), with an explicit precedence rule — main.js:253 `if (window.VIDEOLAR.ochiqmi()) return;` means the cutscene wins. Physical keycode 4194305.
- Enter is missing from the plan's Input Map: videolar.js:75 advances the cutscene on Space OR Enter. Note `e.code === 'Enter'` does NOT match the numpad key ('NumpadEnter'), so KEY_ENTER 4194309 is bound and KEY_KP_ENTER 4194310 is deliberately not — Godot's built-in ui_accept includes KP Enter, which is another reason not to lean on ui_accept here.
- Godot's built-in ui_* actions are the real residual hazard and neither critic nor plan mentions them. ChronoShift's project.godot contains no `ui_` override, so ui_up/ui_down/ui_left/ui_right = the four arrows, ui_accept = Space + Enter + KP Enter, ui_cancel = Escape are all live. They fire on any focused Control, so the critic's step-4 check 'every arrow key moves and nothing else' cannot pass as worded once the HUD has a focusable Button. Fixed with focus_mode = None on HUD Controls, not in the Input Map.
- Digit1/2/3 are real gameplay keys even though they are absent from player.js:113-114's KUZATILADI list — the keydown handler at player.js:120 records EVERY e.code into p.tugmalar and only filters preventDefault. Anyone auditing the web's key set from KUZATILADI alone (as the critic implicitly did) drops the weapon-switch and sword-choice keys. The plan happens to cover them via buxoro_qurol_1/2/3, but for the wrong reason.
- Escape + mouse capture has no Godot equivalent of the web mechanism. main.js:260-264 treats losing pointer lock as a pause signal because the browser swallows Escape itself. Godot exports do not release mouse capture on Escape, so the buxoro_pauza handler must set Input.MouseMode = Visible on open and Captured on resume, or the cursor stays trapped behind the pause menu.
- Focus-loss key reset (player.js:202 `window.addEventListener('blur', ...)`) and the identical reset on pause (main.js:192) have no automatic Godot counterpart — Godot does not guarantee synthetic key-ups when the window loses focus. Without an explicit NOTIFICATION_APPLICATION_FOCUS_OUT handler, alt-tabbing mid-sprint leaves the player running, and mid-block leaves the shield up with its damage reduction applied.
- `buxoro_yugur` must be a single KEY_SHIFT event with "location":0, not two events for left and right shift. Godot's location 0 means 'either side', which is exactly how ChronoShift's existing `run` already spells it (physical_keycode 4194325, location 0) and exactly matches `T.ShiftLeft || T.ShiftRight` at player.js:219. The critic's 'ShiftLeft/ShiftRight only' phrasing invites the fragile two-event spelling with locations 1 and 2.
- Space carries three distinct jobs in the web build — jump (player.js:275), opening fly-in skip (main.js:163), cutscene advance (videolar.js:75) — arbitrated purely by game state, with videolar.js winning via a capture-phase listener that calls stopPropagation (videolar.js:78). That arbitration order is portable behaviour, not an incidental DOM detail, and must be written into the plan as an explicit read-order rule, since Godot's _UnhandledInput chain will not reproduce it by accident.
- player.js:172 binds mouseup on `document`, not on the canvas `dom`, while mousedown is on `dom`. Consequence: releasing the right button outside the viewport still drops the shield. A Godot port that reads buxoro_blok only while the viewport has focus would leave the shield stuck up — IsActionPressed polling gets this right for free, but a signal-based implementation would not.

---

### Correction 3. Audio — Sfx.PlayAt adds pitch jitter and 3D attenuation the web game does not have

**Verdict on the original finding:** `confirmed_but_critic_incomplete`

The core of the blocker is real and I confirmed every load-bearing number by reading the source. Sfx.PlayAt's signature really does default pitchSpread to 0.08f, really does feed that into Jitter() to produce a per-shot PitchScale of 1.0 ± 8%, really does build an AudioStreamPlayer3D with MaxDistance = 60.0f and UnitSize = 6.0f, and really does assign no Bus (so it lands on Master, bypassing the Music/Sfx/Ambient layout the plan makes a prerequisite — a layout that does not exist yet: the project has no AudioBusLayout resource and project.godot has no [audio] section at all). And audio.js really has no PannerNode, no StereoPanner, no listener, no distance term and no per-trigger randomness of any kind: every one-shot's gain node connects straight to `master` (audio.js:72), and the file's only Math.random call (audio.js:39) fills a white-noise buffer that is created ONCE and cached (audio.js:35), then played from offset 0 on every trigger (audio.js:81), so every noise-based SFX is bit-identical run to run within a session. So yes: as the plan is written today, the anvil, footsteps, mark chime and bird cry all detune ±8% and fade with distance, and none of them reach the Sfx bus.

Where the critic is wrong or incomplete, and it matters:

1. WRONG ON PlayUi. The critic writes "PlayAt/PlayUi ... are NOT behaviour-neutral" and attributes pitch jitter to both. PlayUi's default is `float pitchSpread = 0.0f` (Sfx.cs:44), and Jitter returns exactly 1.0f when spread <= 0 (Sfx.cs:76). PlayUi is a 2D AudioStreamPlayer with no jitter, no attenuation and ProcessMode.Always. It is very nearly the correct primitive for Buxoro already. Blaming it obscures the actual defect, which is exclusively PlayAt's two defaults plus the missing Bus on BOTH helpers.

2. DANGEROUSLY INCOMPLETE FIRST FIX. The critic's leading proposal, `Sfx.PlayAt(ctx, stream, pos, db, pitchSpread: 0.0f)`, fixes only half the break. It leaves the AudioStreamPlayer3D, MaxDistance 60 and UnitSize 6 fully in force. An implementer months from now who reads the fix line and stops at the first clause will ship distance-attenuated audio and believe the blocker is closed. The second clause of the critic's own fix is the right one and must be the only one stated.

3. INCOMPLETE ON THE BUS. "Routed to the Sfx bus" is necessary but not sufficient. The web's gain structure is three-deep and every level has a literal number: master 0.55, musiqaGain target 0.30, shamolGain driven 0.0 → 0.22/0.10/0.28/0.30, and per-sound Hajm 0.09–0.30 inside zarb(). Since WavSynth already bakes Hajm into the sample (plan line 299: "multiplied by the gain envelope Hajm → 0.0001"), the playback VolumeDb must be exactly 0.0 or the per-sound level is applied twice. The critic does not say this and PlayAt/PlayUi's own call sites in ChronoShift all pass a negative dB (Vehicle.cs:246 -6.0f, Enemy.cs:230 -6.0f, HUD.cs:825 -10.0f), so the house habit is to pass one.

4. MISSED: THIS FIX DISSOLVES BLOCKER #13. Plan line 447 raises MaxDistance 60 killing the Humo bird at ~250 m as a separate blocker. Once every Buxoro SFX is a 2D AudioStreamPlayer, MaxDistance has no meaning and #13's audio half evaporates — no per-sound MaxDistance table is needed, which is what its proposed fix asks for. Those two blockers must be closed with one edit, not two conflicting ones.

5. MISSED: THE MUTE PATH. main.js:337 calls `window.AUDIO.ovoz(!document.hidden)` on visibilitychange, and ovoz() (audio.js:168-171) ramps MASTER — not SFX — between 0.55 and 0.0 over 0.2 s while ALSO gating zarb() via ovozYoniq (audio.js:67). The plan has no mapping for this at all. It is a Master-bus behaviour, so it belongs in the same spec.

6. MISSED, AND IT IS A SECOND REAL FIDELITY BREAK IN THE SAME STEP. Plan line 299 and line 345 both specify "one 20 s noise loop" for the wind. audio.js:36 is `const n = ctx.sampleRate * 2;` — the buffer is TWO seconds, and it is the same single buffer the SFX slice from. A 2 s band-passed noise loop at 420 Hz Q 0.6 has audible periodicity that a 20 s loop does not. The plan's own "the wind must audibly BREATHE" acceptance test would pass while the texture is wrong.

So: blocker confirmed, its stated fix must not be used as written, and one adjacent plan number is wrong.

<details><summary>Evidence checked — 19 source reads</summary>

- `/Users/humoyunochilov/PROJECTS/ChronoShift/scripts/fx/Sfx.cs:17-18` : `public static void PlayAt(Node context, AudioStream? stream, Vector3 position,                               float volumeDb = 0.0f, float pitchSpread = 0.08f)`
  CONFIRMED. The pitch-jitter default is 0.08f and is opt-out, not opt-in. Every existing caller omits it (Vehicle.cs:246,276; Npc.cs:144; Enemy.cs:230,341; Target.cs:49; WeaponController.cs:647,654,655), so the house pattern is to take the jitter.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/scripts/fx/Sfx.cs:25-34` : `var player = new AudioStreamPlayer3D         {             Stream = stream,             VolumeDb = volumeDb,             PitchScale = Jitter(pitchSpread),             // Effects outlive the node that fired them - a bullet impact must not             // cut off because the muzzle flash was freed.             MaxDistance = 60.0f,             UnitSize = 6.0f,         };`
  CONFIRMED on all three counts: AudioStreamPlayer3D (positional), MaxDistance 60.0f, UnitSize 6.0f. Also note what is ABSENT: there is no `Bus =` initializer, so the player uses Godot's default bus, "Master". The plan's Music/Sfx/Ambient buses are unreachable through this helper.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/scripts/fx/Sfx.cs:75-76` : `private static float Jitter(float spread) =>         spread <= 0.0f ? 1.0f : 1.0f + (float)GD.RandRange(-spread, spread);`
  CONFIRMED: PitchScale becomes a uniform random value in [0.92, 1.08] per shot. Also proves the escape hatch: any spread <= 0 returns exactly 1.0f, so passing 0.0f is a true no-op, not an approximation.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/scripts/fx/Sfx.cs:43-44` : `public static void PlayUi(Node context, AudioStream? stream, float volumeDb = 0.0f,                               float pitchSpread = 0.0f)`
  CRITIC PARTLY WRONG. PlayUi defaults pitchSpread to 0.0f, so it applies NO jitter. It is a plain 2D AudioStreamPlayer (Sfx.cs:51) with ProcessMode.Always (Sfx.cs:57), parented to GetTree().Root (Sfx.cs:60). Its only defect for Buxoro is the missing Bus assignment.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/audio.js:66-74` : `function zarb(o) {     if (!ctx || !ovozYoniq) return;     const t = ctx.currentTime;     const g = ctx.createGain();     g.gain.setValueAtTime(o.hajm, t);     g.gain.exponentialRampToValueAtTime(0.0001, t + o.uzunlik);     g.connect(master);`
  CONFIRMED. Every one-shot's gain node connects directly to `master`. There is no intermediate node, no panner, no distance term. Also note `!ovozYoniq` short-circuits BEFORE any node is created — the mute flag suppresses the sound entirely rather than attenuating it.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/audio.js:35-40` : `if (shovqinBuf) return shovqinBuf;     const n = ctx.sampleRate * 2;     shovqinBuf = ctx.createBuffer(1, n, ctx.sampleRate);     const d = shovqinBuf.getChannelData(0);     for (let i = 0; i < n; i++) d[i] = Math.random() * 2 - 1;`
  TWO findings. (a) The noise buffer is TWO seconds, not the 20 s the plan specifies at lines 299 and 345. (b) It is memoised, so the file's only Math.random call runs once per session; it is not per-trigger variation.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/audio.js:81` : `s.start(t); s.stop(t + o.uzunlik);`
  CONFIRMED zero randomness. `start(when)` with no offset argument plays the cached buffer from sample 0 every single time. So bolga, qadam, tuyoq, qilich, kamon and blok all slice the identical opening 0.07-0.13 s of the same waveform on every trigger — bit-identical, not merely statistically similar.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/audio.js (whole file, grep)` : `grep -niE 'panner|spatial|listener|position|distance|detune|playbackRate' audio.js  ->  audio.js:133:    // ozgina detune — jonli hissi`
  CONFIRMED: the sole hit in all 175 lines is an Uzbek comment on the MUSIC oscillator's fixed 1.005 ratio (audio.js:135). There is no PannerNode, no AudioListener, no playbackRate, no positional API anywhere. The Buxoro sound field is unambiguously 2D.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/audio.js:17-27` : `master = ctx.createGain();     master.gain.value = 0.55;     master.connect(ctx.destination);      musiqaGain = ctx.createGain();     musiqaGain.gain.value = 0.0;     musiqaGain.connect(master);      shamolGain = ctx.createGain();     shamolGain.gain.value = 0.0;     shamolGain.connect(master);`
  The web's real bus topology, and it is exactly three buses under a master: Music and Ambient each have their own gain, SFX has none and hits master directly. Master 0.55 = -5.1927 dB. This validates the plan's Master/Music/Sfx/Ambient layout, and fixes Sfx at 0.0 dB.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/audio.js:92-113` : `bolga: () => { zarb({ chastota: 2100, uzunlik: 0.13, hajm: 0.28, shovqin: true, q: 5 });                        zarb({ chastota: 780, oxirgi: 300, uzunlik: 0.22, hajm: 0.16, tur: 'square' }); },`
  The S table holds exactly 15 keys (bolga, qadam, tuyoq, kaltak, qilich, kamon, blok, olim, zarba, belgi, yigildi, rad, shogird, vaqt, qushUchdi), confirming the plan's count. Multi-layer sounds are SEPARATE zarb() calls, i.e. separate gain nodes summing at master — so in Godot they are separate players, not one pre-mixed sample.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/audio.js:107-112` : `shogird:   () => { zarb({ chastota: 440, oxirgi: 660, uzunlik: 0.16, hajm: 0.14, tur: 'sine' });                        setTimeout(() => zarb({ chastota: 660, oxirgi: 880, uzunlik: 0.20, hajm: 0.12, tur: 'sine' }), 110); },     vaqt:      () => { [523, 659, 784, 1046].forEach((f, i) =>                         setTimeout(() => zarb({ chastota: f, uzunlik: 0.55, hajm: 0.16, tur: 'sine' }), i * 85)); },`
  The two delayed sounds use setTimeout, which is WALL-CLOCK and unaffected by game pause. In Godot these must be real-time SceneTreeTimers with ProcessMode.Always, not game-clock timers, or they silently drop when the pause menu or a cutscene opens.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/audio.js:168-171` : `function ovoz(yoq) {     ovozYoniq = yoq;     if (master) master.gain.linearRampToValueAtTime(yoq ? 0.55 : 0.0, ctx.currentTime + 0.2);   }`
  MISSED BY THE CRITIC AND BY THE PLAN. The mute is a 0.2 s linear ramp on MASTER (everything, including music and wind), combined with a hard gate on new SFX. It is not an SFX-bus mute.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/main.js:336-340` : `document.addEventListener('visibilitychange', () => {       if (window.AUDIO.tayyor()) window.AUDIO.ovoz(!document.hidden);     });`
  The ONLY caller of ovoz() in the entire codebase (verified by grep across src/js). There is no user-facing sound toggle. The trigger is window focus, which maps to NotificationApplicationFocusOut/In in Godot.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/player.js:258-263` : `p.qadamTimer -= dt;       if (p.qadamTimer <= 0) {         p.qadamTimer = p.otda ? 0.26 : (yuguradi ? 0.33 : 0.46);         window.AUDIO.S[p.otda ? 'tuyoq' : 'qadam']();       }`
  ANSWERS THE 'is anything deliberately randomized' question: NO. Even the footstep cadence — the one place a game normally jitters — is three fixed constants (0.26 mounted / 0.33 running / 0.46 walking). Nothing in the web's audio layer, synthesis or triggering, is randomized per event.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/combat.js:341,377,473` : `window.AUDIO.S.kamon();  /  window.AUDIO.S.qilich();  /  window.AUDIO.S.olim();`
  Decisive on 2D. These fire from ALLY ARCHERS, ALLY SWORDSMEN and DYING ENEMIES scattered across a 180x100 m field, yet all play at full, unattenuated volume through `master`. Under PlayAt's MaxDistance 60 the far half of the battle would go silent — a change any player would hear immediately.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/project.godot + find` : `find . -name '*bus*' -o -name '*.tres' | grep -i bus   ->   (no output);   grep -n -i '\[audio\]|bus_layout' project.godot   ->   (no output)`
  CONFIRMED the plan's claim at line 325 that the project has no bus layout. There is no AudioBusLayout resource on disk and project.godot declares no [audio] section, so Godot uses its implicit Master-only default. default_bus_layout.tres plus the project.godot key are both genuinely new work.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/scripts/ (grep)` : `grep -rn 'Bus\b|AudioServer|AudioStreamPlayer' --include=*.cs scripts/  ->  scripts/fx/Sfx.cs:25, scripts/fx/Sfx.cs:51  (only)`
  Nothing anywhere in ChronoShift's C# sets a Bus or touches AudioServer. Every sound in the project currently plays on Master. Buxoro's bus routing therefore cannot be inherited and must be written explicitly.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/docs/BUXORO_PORT_PLAN.md:299` : `Played through the existing Sfx.PlayAt/PlayUi helpers. WIND is live, never baked: one 20 s noise loop on the Ambient bus`
  The exact sentence that must be replaced, and in the same breath the 20 s wind error (source is 2 s, audio.js:36). Both defects live in the same mapping row.
- `/Users/humoyunochilov/PROJECTS/ChronoShift/docs/BUXORO_PORT_PLAN.md:76` : `Sfx.PlayAt/PlayUi is the right one-shot pattern;`
  The reuse-list justification that accepted Sfx on its name rather than its defaults — precisely the failure mode the plan's own critic summary names at line 515. This sentence must be qualified, not deleted: the create-play-QueueFree PATTERN is right, PlayAt's defaults are not.

</details>

#### Corrected specification

##### REPLACES the clause "Played through the existing Sfx.PlayAt/PlayUi helpers" in the audio mapping row (plan line 299), and amends Step 14, the reuse list (line 76), the data-resource row (line 325) and the rationale (line 345).

###### 0. The rule, stated once

Buxoro's sound field is 2D and deterministic. Every one of the 15 SFX, the wind bed and the 13-note melody plays at full, unattenuated, un-detuned volume regardless of where the source is on the 180 x 100 m field and regardless of where the camera is looking. No Buxoro sound is ever positional. No Buxoro sound is ever pitch-randomised. Verified: audio.js contains no PannerNode, no AudioListener, no playbackRate and no per-trigger Math.random (its one Math.random, audio.js:39, fills a memoised buffer once per session, and audio.js:81 replays it from offset 0 every time).

###### 1. REFUSAL — add to docs/BUXORO_REFUSALS.md as the fifth entry, and repeat as a file-header comment in BuxoroAudio.cs

**REFUSAL 5 — no Sfx.PlayAt for Buxoro; no AudioStreamPlayer3D anywhere in the episode.**

`scripts/fx/Sfx.cs:17-18` defaults `pitchSpread` to `0.08f`, and `Sfx.cs:75-76` turns that into `PitchScale = 1.0f + RandRange(-0.08, 0.08)` — a uniform ±8% detune on every single shot. `Sfx.cs:25-34` builds an `AudioStreamPlayer3D` with `MaxDistance = 60.0f` and `UnitSize = 6.0f`. Neither helper assigns `Bus`, so both play on Master and the Music/Sfx/Ambient layout is bypassed.

The web has none of this. `audio.js:72` is `g.connect(master)` — one hop, no panner, no distance term, no jitter. Concretely, adopting PlayAt would change these numbers:

- Pitch: `belgi` is a 880 Hz → 1320 Hz chirp (a fixed musical fifth-and-a-bit, the game's "a node is marked" signature). Under ±8% it lands anywhere in 809-950 Hz → 1214-1426 Hz and stops being the same note twice. `vaqt` is a literal [523, 659, 784, 1046] Hz arpeggio (C-E-G-C) — under ±8% it is out of tune with itself.
- Distance: the battle spans x ∈ [2, 176]. `olim` (enemy death, combat.js:473), `qilich` (ally swordsman, combat.js:377) and `kamon` (ally archer, combat.js:341) fire from anywhere on that field. At `MaxDistance = 60` the far half of the battle is silent. The Humo bird (`qushUchdi`, humo.js:48) reads at ~250 m by design and would be inaudible.

Do NOT "fix" this by passing `pitchSpread: 0.0f` to PlayAt. That kills the jitter and leaves the 3D attenuation fully in force. PlayAt must not be called from any Buxoro file at all.

`Sfx.PlayUi` is NOT part of this refusal — it already defaults `pitchSpread` to `0.0f` (`Sfx.cs:44`), is a 2D `AudioStreamPlayer`, and sets `ProcessMode.Always`. Its only gap is the missing `Bus`. Buxoro nevertheless uses its own `BuxoroAudio.Chal()` rather than PlayUi, because PlayUi cannot set a bus, cannot schedule the +110 ms and +85 ms layer delays that `shogird` and `vaqt` require, and cannot be gated by the focus-mute flag.

This refusal also CLOSES the audio half of critic finding #13 (plan line 447-449). No per-sound MaxDistance table is needed and none must be added: with every player 2D, MaxDistance has no meaning. Do not add `bus`/`maxDistance` parameters to `Sfx.cs` — it is shared with the main ChronoShift game and must not be touched by this port.

###### 2. resources/default_bus_layout.tres — four buses, exact dB

Derived from audio.js:17-27. WebAudio gains are LINEAR; Godot is dB; every literal goes through `Mathf.LinearToDb`.

| Bus | Index | Send | volume_db | Source |
|---|---|---|---|---|
| Master | 0 | — | **-5.193** | `master.gain.value = 0.55` (audio.js:18); 20·log10(0.55) = -5.19275 |
| Music | 1 | Master | **-10.458** | `musiqaGain` ramp target 0.30 (audio.js:150); 20·log10(0.30) = -10.45757 |
| Sfx | 2 | Master | **0.0** | zarb() connects straight to master (audio.js:72) — there is no SFX gain stage |
| Ambient | 3 | Master | **0.0** | `shamolGain` is driven per-event, so its level lives on the player, not the bus |

Ambient carries one effect, `AudioEffectBandPassFilter`, `enabled = true`, initial `cutoff_hz = 420.0`, `resonance = 0.6` (audio.js:49). BuxoroAudio writes its `CutoffHz` every `_Process` frame — see §5.

Bus names are spelled exactly `Master`, `Music`, `Sfx`, `Ambient`. Code addresses them via `AudioServer.GetBusIndex("Sfx")`, never by a hardcoded index other than Master = 0.

`project.godot` gains, under a new `[audio]` section:
```
[audio]

buses/default_bus_layout="res://resources/default_bus_layout.tres"
```

###### 3. BuxoroAudio.Chal() — the single SFX entry point, replacing Sfx.PlayAt/PlayUi

All 15 SFX go through one method. There is no PlayUi/PlayAt split in Buxoro because there is no 2D/3D split in the web.

```csharp
// REFUSAL 5: never Sfx.PlayAt here - it detunes +/-8% (Sfx.cs:17-18, 75-76)
// and attenuates over 60 m (Sfx.cs:32-33). audio.js:72 does neither.
public void Chal(string kalit)
{
    // audio.js:67 - the mute GATES the sound, it does not attenuate it.
    if (!_ovozYoniq) { return; }
    if (!_retseptlar.TryGetValue(kalit, out SfxRecipeData? retsept)) { return; }

    foreach (SfxLayerData qatlam in retsept.Qatlamlar)
    {
        if (qatlam.KechikishMs <= 0)
        {
            ChalQatlam(qatlam);
        }
        else
        {
            // audio.js:108,111 use setTimeout - WALL CLOCK, unaffected by pause.
            SceneTreeTimer timer = GetTree().CreateTimer(
                qatlam.KechikishMs / 1000.0,
                processAlways: true, processInPhysics: false, ignoreTimeScale: true);
            timer.Timeout += () => ChalQatlam(qatlam);
        }
    }
}

private void ChalQatlam(SfxLayerData qatlam)
{
    var player = new AudioStreamPlayer
    {
        Stream = qatlam.Namuna,       // AudioStreamWAV baked by WavSynth at _Ready
        Bus = "Sfx",
        VolumeDb = 0.0f,              // Hajm is ALREADY inside the sample - see below
        PitchScale = 1.0f,            // audio.js has zero pitch variation
        ProcessMode = ProcessModeEnum.Always,
    };

    AddChild(player);                 // parented to BuxoroAudio, not CurrentScene
    player.Finished += player.QueueFree;
    player.Play();
}
```

Four properties of this method are load-bearing and must not be "improved":

- **`VolumeDb = 0.0f`, always.** WavSynth bakes the envelope `Hajm → 0.0001` into the sample (plan line 299), so the per-sound level is already in the waveform. Passing a negative dB the way every existing ChronoShift call site does (Vehicle.cs:246 `-6.0f`, Enemy.cs:230 `-6.0f`, HUD.cs:825 `-10.0f`) would apply the level twice.
- **`PitchScale = 1.0f`, always.** Written explicitly rather than left to the default so the refusal is visible at the site.
- **No voice cap, no distance cull, no cooldown.** Thirty enemies dying in the same second play thirty overlapping `olim` at full volume, exactly as the web does. Do not add polyphony limiting — it is a behaviour the source does not have.
- **`ProcessMode.Always` and `ignoreTimeScale: true`.** WebAudio runs on `ctx.currentTime`, which no game pause and no `Engine.TimeScale` affects. A sound already playing when the pause menu opens must finish; a `shogird` second layer scheduled 110 ms out must still fire.

###### 4. The 15 SFX — layers, delays, and where each is triggered

Layer parameters are the zarb() table, already correct in the plan's data row (line 325). What this spec adds is the layer/delay structure and the trigger sites, so nothing is left to inference.

| Key | Layers (KechikishMs) | Peak Hajm sum | Trigger sites |
|---|---|---|---|
| `bolga` | 2 (0, 0) | 0.44 | game.js:268, 296, 366, 371, 454 — anvil at 6 Hz while F is held, weapon forged, sword blueprint reveal |
| `qadam` | 1 (0) | 0.09 | player.js:262 (every 0.33 s running / 0.46 s walking), 280 (jump), 287 (land) |
| `tuyoq` | 1 (0) | 0.20 | player.js:262 mounted, every 0.26 s |
| `kaltak` | 1 (0) | 0.26 | player.js:488 (club swing), 517 (bow release), combat.js:400 (unarmed ally) |
| `qilich` | 2 (0, 0) | 0.42 | player.js:488, combat.js:377 (ally swordsman) |
| `kamon` | 1 (0) | 0.15 | combat.js:338 (ally archer) |
| `blok` | 1 (0) | 0.24 | game.js:552 (shield block) |
| `olim` | 1 (0) | 0.20 | combat.js:473 (enemy death), game.js:489, 597 |
| `zarba` | 1 (0) | 0.30 | game.js:553, 562 (player hit), combat.js:265 (enemy hits the wall) |
| `belgi` | 1 (0) | 0.10 | apprentices.js:176 (node marked) |
| `yigildi` | 1 (0) | 0.09 | apprentices.js:276 (resource picked up) |
| `rad` | 1 (0) | 0.14 | apprentices.js:170 (queue full — refusal) |
| `shogird` | 2 (**0, 110**) | 0.14 + 0.12 | apprentices.js:407 (apprentice recruited) |
| `vaqt` | 4 (**0, 85, 170, 255**) | 0.16 each | game.js:402 (time added — the ChronoShift rewind cue) |
| `qushUchdi` | 1 (0) | 0.13 | humo.js:48 (bird takes off / leaves) |

Layers are separate `zarb()` calls with separate gain nodes summing at `master` (audio.js:93-94, 98-99, 107-108), so they are separate `AudioStreamPlayer` nodes, NOT one pre-mixed sample. Peak sums stay below 1.0, so no clipping guard is needed and none must be added.

Every noise-based layer (`bolga` L1, `qadam`, `tuyoq`, `qilich` L1, `kamon`, `blok`) slices the SAME cached buffer from offset 0 (audio.js:35, 81), so WavSynth must generate ONE 2-second noise buffer at `_Ready` and take every SFX slice from sample 0 of it. Do not reseed per recipe and do not use a random offset — the web's noise SFX are bit-identical on every trigger.

###### 5. Wind — Ambient bus

CORRECTION TO THE PLAN: the noise loop is **2.0 seconds**, not 20. `audio.js:36` is `const n = ctx.sampleRate * 2;`, and it is the same buffer the SFX slice from. A 20 s loop has a materially different texture and would pass the plan's "must audibly breathe" test while being wrong.

One `AudioStreamPlayer`, `Bus = "Ambient"`, `ProcessMode = ProcessModeEnum.Always`, stream = a looping `AudioStreamWAV` of the 2 s noise buffer (`LoopMode = AudioStreamWav.LoopModeEnum.Forward`, `LoopBegin = 0`, `LoopEnd = sample count`). Started once at `_Ready` and never stopped.

The band-pass lives on the Ambient bus, not on the player, and BuxoroAudio drives it every frame:

```csharp
_wind_t += (float)delta;                                  // real seconds, unscaled
_bandPass.CutoffHz = 420.0f + Mathf.Sin(_wind_t * 0.09f * Mathf.Tau) * 190.0f;
```

420 Hz centre, ±190 Hz, 0.09 Hz LFO (audio.js:49, 51-53) — an 11.1 s breathing cycle. `Resonance` stays 0.6 and is never modulated.

Level is on the PLAYER, mirroring `shamolGain`, and ramps LINEARLY in gain then converts:
```csharp
// audio.js:62 - linearRampToValueAtTime is linear in GAIN, not in dB.
float g = Mathf.Lerp(_shamolBoshi, _shamolMaqsad, _shamolT / _shamolDavri);
_windPlayer.VolumeDb = g <= 0.0001f ? -80.0f : Mathf.LinearToDb(g);
```
Ramping `VolumeDb` directly instead is WRONG — it produces a different curve. Targets: 0.22 over 2.5 s at game start (main.js:66), 0.10 over 3 s on the Act transition (game.js:604), 0.28 over 2 s (game.js:607), 0.30 over 2 s at Tarobiy's death (game.js:599). Initial value 0.0 (audio.js:26).

###### 6. Music — Music bus

Note-by-note as the plan already specifies, each note an `AudioStreamPlayer` with `Bus = "Music"`, `VolumeDb = 0.0f` (the 0.20 peak of audio.js:131 is baked into the note sample), `PitchScale = 1.0f`, `ProcessMode = ProcessModeEnum.Always`. The Music BUS sits at -10.458 dB and BuxoroAudio ramps that bus (not the players) for the fades:

- Fade in: linear gain 0 → 0.30 over 2.5 s (audio.js:150), again ramped in linear gain then converted.
- Fade out at Tarobiy's death: current → 0.0001 over 1.2 s (audio.js:164), then PERMANENT.

`ProcessMode.Always` on the note players is required: the pause menu calls `Musiqa(false)` (main.js:196) and the 1.2 s fade must complete while the tree is paused.

###### 7. Focus mute — NEW, no mapping exists in the plan today

audio.js:168-171 + main.js:337. When the browser tab is hidden the web ramps **Master** 0.55 → 0.0 over 0.2 s AND sets `ovozYoniq = false`, which makes `zarb()` early-return (audio.js:67) so no new SFX are even created. Music and wind keep being scheduled, silently; on unmute the melody resumes mid-phrase rather than from the top.

```csharp
public override void _Notification(int what)
{
    if (what == NotificationApplicationFocusOut) { Ovoz(false); }
    else if (what == NotificationApplicationFocusIn) { Ovoz(true); }
}

private void Ovoz(bool yoq)
{
    _ovozYoniq = yoq;                                  // gates Chal(), audio.js:67
    _masterMaqsad = yoq ? 0.55f : 0.0f;                // linear, ramped over 0.2 s
    _masterDavri = 0.2f;
}
```
The ramp writes `AudioServer.SetBusVolumeDb(0, ...)` from a linear lerp, clamped to -80 dB at zero. This is a window-focus notification, not input — no Input Map action is involved, so the house rule does not apply. There is no user-facing sound toggle in the web (grep confirms main.js:337 is the only caller of `ovoz`), so do not add one.

###### 8. Files

Unchanged from Step 14, plus: `resources/default_bus_layout.tres` gets the four buses and the Ambient band-pass above; `project.godot` gets the `[audio]` section; `docs/BUXORO_REFUSALS.md` gets REFUSAL 5. `scripts/fx/Sfx.cs` is NOT modified — it stays as-is for the main ChronoShift game.

#### Code

// ===== resources/default_bus_layout.tres =====
// Master 0.55 -> -5.193 dB (audio.js:18); Music 0.30 -> -10.458 dB (audio.js:150);
// Sfx 0.0 dB because zarb() connects straight to master (audio.js:72);
// Ambient 0.0 dB because shamolGain is driven per-event on the player.
[gd_resource type="AudioBusLayout" load_steps=2 format=3]

[sub_resource type="AudioEffectBandPassFilter" id="AudioEffectBandPassFilter_shamol"]
cutoff_hz = 420.0
resonance = 0.6

[resource]
bus/0/volume_db = -5.193
bus/1/name = &"Music"
bus/1/solo = false
bus/1/mute = false
bus/1/bypass_fx = false
bus/1/volume_db = -10.458
bus/1/send = &"Master"
bus/2/name = &"Sfx"
bus/2/solo = false
bus/2/mute = false
bus/2/bypass_fx = false
bus/2/volume_db = 0.0
bus/2/send = &"Master"
bus/3/name = &"Ambient"
bus/3/solo = false
bus/3/mute = false
bus/3/bypass_fx = false
bus/3/volume_db = 0.0
bus/3/send = &"Master"
bus/3/effect/0/effect = SubResource("AudioEffectBandPassFilter_shamol")
bus/3/effect/0/enabled = true


// ===== project.godot (new section) =====
// [audio]
//
// buses/default_bus_layout="res://resources/default_bus_layout.tres"


// ===== scripts/buxoro/audio/BuxoroAudio.cs (the parts this blocker governs) =====
using Godot;
using System.Collections.Generic;

namespace ChronoShift.Buxoro;

/// <summary>
/// The whole Buxoro sound field: 15 live-synthesised one-shots, the wind bed and
/// the D-Phrygian melody.
/// </summary>
/// <remarks>
/// REFUSAL 5 - no Sfx.PlayAt and no AudioStreamPlayer3D anywhere in this episode.
/// Sfx.cs:17-18 defaults pitchSpread to 0.08f and Sfx.cs:75-76 turns that into a
/// per-shot PitchScale of 1.0 +/- 8%; Sfx.cs:32-33 attenuates over MaxDistance 60
/// with UnitSize 6. The web does neither: audio.js:72 is a bare g.connect(master),
/// there is no PannerNode in the file, and its only Math.random (audio.js:39) fills
/// a buffer that is cached (audio.js:35) and replayed from offset 0 every time
/// (audio.js:81). Passing pitchSpread: 0.0f is NOT the fix - it leaves the 3D
/// attenuation in force, which alone silences the far half of a 180 m battlefield.
/// Sfx.PlayUi is jitter-free already, but it cannot set a Bus and cannot schedule
/// the +110 ms / +85 ms layer delays shogird and vaqt need, so Buxoro uses Chal().
/// </remarks>
public partial class BuxoroAudio : Node
{
    private readonly Dictionary<string, SfxRecipeData> _retseptlar = new();
    private AudioStreamPlayer _shamolPlayer = null!;
    private AudioEffectBandPassFilter _bandPass = null!;

    private bool _ovozYoniq = true;          // audio.js:9
    private float _shamolT;                  // real seconds, never time-scaled

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        int ambient = AudioServer.GetBusIndex("Ambient");
        _bandPass = (AudioEffectBandPassFilter)AudioServer.GetBusEffect(ambient, 0);
    }

    /// <summary>Fires one of the 15 SFX. 2D, full volume, no pitch variation, ever.</summary>
    public void Chal(string kalit)
    {
        // audio.js:67 - the mute GATES the sound; it is not an attenuation.
        if (!_ovozYoniq || !_retseptlar.TryGetValue(kalit, out SfxRecipeData? retsept))
        {
            return;
        }

        foreach (SfxLayerData qatlam in retsept.Qatlamlar)
        {
            if (qatlam.KechikishMs <= 0)
            {
                ChalQatlam(qatlam);
            }
            else
            {
                // audio.js:108 (shogird +110 ms) and audio.js:111 (vaqt +85 ms steps)
                // use setTimeout: wall clock, unaffected by pause or Engine.TimeScale.
                SceneTreeTimer timer = GetTree().CreateTimer(
                    qatlam.KechikishMs / 1000.0,
                    processAlways: true, processInPhysics: false, ignoreTimeScale: true);
                timer.Timeout += () => ChalQatlam(qatlam);
            }
        }
    }

    private void ChalQatlam(SfxLayerData qatlam)
    {
        var player = new AudioStreamPlayer
        {
            Stream = qatlam.Namuna,
            Bus = "Sfx",
            // Hajm is already inside the baked sample (WavSynth applies the
            // Hajm -> 0.0001 envelope), so a second gain here would square it.
            VolumeDb = 0.0f,
            // Written explicitly so the refusal is visible at the call site.
            PitchScale = 1.0f,
            // WebAudio runs on ctx.currentTime; no game pause silences it.
            ProcessMode = ProcessModeEnum.Always,
        };

        player.AddToGroup("buxoro_sfx");   // parity harness T1 reads this group
        AddChild(player);
        player.Finished += player.QueueFree;
        player.Play();
    }

    public override void _Process(double delta)
    {
        // audio.js:51-54 - 0.09 Hz LFO, +/-190 Hz around a 420 Hz centre.
        // An 11.1 s cycle. Resonance stays 0.6 and is never modulated.
        _shamolT += (float)delta;
        _bandPass.CutoffHz = 420.0f + (Mathf.Sin(_shamolT * 0.09f * Mathf.Tau) * 190.0f);

        ShamolRampYangila(delta);
        MusiqaRampYangila(delta);
        MasterRampYangila(delta);
    }

    /// <summary>Wind level. audio.js:62 ramps LINEARLY IN GAIN, so convert last.</summary>
    private void ShamolRampYangila(double delta)
    {
        if (_shamolDavri <= 0.0f) { return; }

        _shamolO += (float)delta;
        float n = Mathf.Min(1.0f, _shamolO / _shamolDavri);
        float g = Mathf.Lerp(_shamolBoshi, _shamolMaqsad, n);

        // Ramping VolumeDb directly would trace a different curve than WebAudio.
        _shamolPlayer.VolumeDb = g <= 0.0001f ? -80.0f : Mathf.LinearToDb(g);

        if (n >= 1.0f) { _shamolDavri = 0.0f; }
    }

    // main.js:337 - visibilitychange. audio.js:168-171 ramps MASTER (everything)
    // over 0.2 s AND gates new one-shots. It is not an SFX-only mute.
    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut) { Ovoz(false); }
        else if (what == NotificationApplicationFocusIn) { Ovoz(true); }
    }

    private void Ovoz(bool yoq)
    {
        _ovozYoniq = yoq;
        _masterBoshi = _masterJoriy;
        _masterMaqsad = yoq ? 0.55f : 0.0f;   // audio.js:170
        _masterDavri = 0.2f;
        _masterO = 0.0f;
    }

    private void MasterRampYangila(double delta)
    {
        if (_masterDavri <= 0.0f) { return; }

        _masterO += (float)delta;
        float n = Mathf.Min(1.0f, _masterO / _masterDavri);
        _masterJoriy = Mathf.Lerp(_masterBoshi, _masterMaqsad, n);

        AudioServer.SetBusVolumeDb(0,
            _masterJoriy <= 0.0001f ? -80.0f : Mathf.LinearToDb(_masterJoriy));

        if (n >= 1.0f) { _masterDavri = 0.0f; }
    }
}


// ===== scripts/buxoro/audio/SfxRecipeData.cs =====
using Godot;

namespace ChronoShift.Buxoro;

/// <summary>One of audio.js's 15 S-table entries, as data.</summary>
[GlobalClass]
public partial class SfxRecipeData : Resource
{
    /// <summary>
    /// Separate zarb() calls, each its own gain node summing at master
    /// (audio.js:93-94, 98-99, 107-108, 110-111). Never pre-mix these into one
    /// sample: shogird's second layer is +110 ms and vaqt's are +85 ms apart.
    /// </summary>
    [Export] public SfxLayerData[] Qatlamlar { get; set; } = System.Array.Empty<SfxLayerData>();
}

[GlobalClass]
public partial class SfxLayerData : Resource
{
    [Export] public float Chastota { get; set; }        // o.chastota
    [Export] public float Oxirgi { get; set; }          // o.oxirgi, 0 = no ramp
    [Export] public float Uzunlik { get; set; }         // o.uzunlik, seconds
    [Export] public float Hajm { get; set; }            // o.hajm, baked into Namuna
    [Export] public string Tur { get; set; } = "triangle";
    [Export] public bool Shovqin { get; set; }
    [Export] public string Filtr { get; set; } = "bandpass";
    [Export] public float Q { get; set; } = 1.2f;
    [Export] public int KechikishMs { get; set; }       // setTimeout offset, wall clock

    /// <summary>Baked once by WavSynth at _Ready. Never ships as a file.</summary>
    public AudioStreamWav? Namuna { get; set; }
}

#### Test that proves it

Add to docs/BUXORO_PARITY.md as five rows, and as a GdUnit/xUnit fixture `BuxoroAudioParity`. All five are mechanical pass/fail with no "close enough" option.

**T1 — No 3D player, no jitter, no wrong bus, no double gain (the refusal itself).**
Add `player.AddToGroup("buxoro_sfx")` in `ChalQatlam`. Fire all 15 keys 50 times each (750 calls, 900 layers) with the tree paused so nothing frees, then assert over `GetTree().GetNodesInGroup("buxoro_sfx")`:
- Count == 900 exactly (bolga/qilich/shogird contribute 2 layers each, vaqt 4 → 50·(11·1 + 3·2 + 1·4) = 50·21 = 1050; assert 1050).
- `node is AudioStreamPlayer && node is not AudioStreamPlayer3D` for every node. Zero AudioStreamPlayer3D.
- `PitchScale == 1.0f` exactly for every node (exact float equality, not a tolerance — any jitter fails).
- `Bus == "Sfx"` for every node.
- `VolumeDb == 0.0f` exactly for every node.
Also assert statically that `grep -rn "Sfx.PlayAt\|Sfx.PlayUi" scripts/buxoro/` returns zero lines. That single grep is the cheapest permanent guard on REFUSAL 5.

**T2 — Bus layout numbers.**
```
AudioServer.GetBusCount() == 4
GetBusVolumeDb(GetBusIndex("Master"))  == -5.193f  +/- 0.002
GetBusVolumeDb(GetBusIndex("Music"))   == -10.458f +/- 0.002
GetBusVolumeDb(GetBusIndex("Sfx"))     ==   0.0f   +/- 0.001
GetBusVolumeDb(GetBusIndex("Ambient")) ==   0.0f   +/- 0.001
GetBusSend(GetBusIndex("Music")) == "Master"   (same for Sfx, Ambient)
GetBusEffectCount(GetBusIndex("Ambient")) == 1 and effect 0 is AudioEffectBandPassFilter
```

**T3 — Distance invariance (the behaviour the blocker is actually about).**
Put an `AudioEffectSpectrumAnalyzer` on the Sfx bus. Move the Camera3D / listener to (0, 1.7, 0), fire `qushUchdi`, capture peak magnitude over its 0.55 s. Teleport the listener to (250, 1.7, 0) — beyond PlayAt's 60 m MaxDistance — and fire it again. The two peaks must be **identical to within 0.05 dB**. Repeat for `olim` at 0 m vs 176 m (the field's x extent). Under the current plan this test fails by roughly -20 dB and -14 dB respectively; under the corrected spec the delta is zero because there is no distance term at all.

**T4 — Determinism (the "zero randomness" property).**
Record the Sfx bus to WAV across 20 consecutive `Chal("qadam")` calls, 1 s apart, and 20 consecutive `Chal("belgi")` calls. Split into 20 clips and assert every clip is **sample-identical** to clip 0 (max abs sample difference == 0.0, not a tolerance). `qadam` proves the noise slice is taken from offset 0 of one cached buffer; `belgi` proves the 880 → 1320 Hz chirp is not detuned. Then assert the spectral centroid of `vaqt`'s four layers lands within 1 Hz of 523 / 659 / 784 / 1046 Hz. Under ±8% jitter the 20 clips differ and the arpeggio centroids scatter over ±8% (480-565 / 606-712 / 721-847 / 962-1130 Hz), so this test fails loudly today.

**T5 — Wind cycle and loop length (catches the 20 s error).**
Record the Ambient bus for 30 s at idle. Assert (a) the band-pass cutoff telemetry traces 420 ± 190 Hz with a period of **11.1 s ± 0.2 s** (0.09 Hz), reaching 610 Hz and 230 Hz; and (b) the autocorrelation of the raw noise source has a peak at a lag of **2.000 s ± 1 sample**, not 20 s. Then chain into the existing RMS/spectral-centroid oracle from tools/parity/bake_reference.js for all 15 clips: RMS envelope within 5%, spectral centroid within 3%.

**T6 — Focus mute.**
Send `NotificationApplicationFocusOut`. Assert that 0.25 s later `GetBusVolumeDb(0) <= -79.0f`, and that `Chal("bolga")` during the muted window adds **zero** nodes to the `buxoro_sfx` group (it is gated, not attenuated). Send `NotificationApplicationFocusIn`; assert `GetBusVolumeDb(0)` is back to -5.193 ± 0.002 within 0.25 s and that the music player was never stopped or restarted (its `GetPlaybackPosition()` advanced monotonically across the whole mute window).

#### What the user does in the Godot editor

Three things in the Godot editor, in this order.

**1. Register the bus layout (required, and the plan understates it).**
After Claude writes `res://resources/default_bus_layout.tres` and the `[audio]` section in `project.godot`, open Project → Project Settings → Audio → Buses, click **Load** in the bus panel toolbar and pick `res://resources/default_bus_layout.tres`, then click **Save As** and overwrite that same path. The reopen-and-resave round trip is what makes the editor's own bus list show Master / Music / Sfx / Ambient; without it `AudioServer.GetBusIndex("Sfx")` returns -1 at runtime and every Buxoro sound silently falls back to Master, which is the exact failure this blocker is about. Confirm you see four buses named exactly `Master`, `Music`, `Sfx`, `Ambient`, that Music/Sfx/Ambient all send to Master, and that Ambient has one **BandPassFilter** effect showing Cutoff 420 Hz and Resonance 0.6.

**2. Add BuxoroAudio to the episode scene.**
In `scenes/buxoro/Buxoro.tscn`, add a single child Node of the scene root, name it `BuxoroAudio`, and attach `scripts/buxoro/audio/BuxoroAudio.cs`. In the Inspector set its **Process Mode** to **Always** (the script also sets this in `_Ready`, but having it wrong in the scene file is confusing to read later). It needs no other children — the wind player, the note players and every one-shot are created in code. Do NOT add any AudioStreamPlayer3D or an AudioListener3D to the Buxoro scene; if one is already there from a template, delete it.

**3. Confirm no audio files ship.**
`res://assets/audio/buxoro/` must not exist, and no `.wav` or `.ogg` may be imported under it. The synthesis is the asset. (`tools/parity/bake_reference.js` writes its 15 reference WAVs outside `res://`, to `tools/parity/out/` — they are the test oracle and must never enter the project tree, or Godot will import and export them.)

**Nothing for the Input Map.** The focus mute is driven by `NotificationApplicationFocusOut`/`In`, a window notification rather than input, so no action needs adding. The web has no user-facing sound toggle either — `ovoz()` is called from exactly one place, `main.js:337` — so do not add a mute key or a settings row.

#### Sections of this plan that this correction overrides

- Line 76 (reuse-candidate justification) — 'Sfx.PlayAt/PlayUi is the right one-shot pattern' must become 'Sfx's create-play-QueueFree PATTERN is right and BuxoroAudio copies it, but PlayAt's defaults are not: REFUSED, see REFUSAL 5. PlayUi is jitter-free (Sfx.cs:44) but cannot set a bus or schedule layer delays, so Buxoro uses its own Chal().'
- Line 299 (audio mapping row) — delete 'Played through the existing Sfx.PlayAt/PlayUi helpers.' and substitute §0/§3 of the corrected spec. In the same row change 'one 20 s noise loop' to 'one 2.0 s noise loop (audio.js:36 is ctx.sampleRate * 2), shared with the noise SFX'.
- Line 325 (data-resource row) — extend 'Buses Master=-5.19dB, Music=-10.46dB, Sfx, Ambient' to the full four-row table of §2 including Sfx=0.0 dB, Ambient=0.0 dB, the AudioEffectBandPassFilter on Ambient (420 Hz / Q 0.6) and the project.godot buses/default_bus_layout key. Add the layer/delay column from §4 (shogird +110 ms, vaqt +85/170/255 ms) since the current row shows the parameters but not the scheduling.
- Line 345 (audio rationale paragraph) — same 20 s → 2.0 s correction, and add the sentence that every Buxoro player is a 2D AudioStreamPlayer on a named bus, never AudioStreamPlayer3D.
- Step 14 'Deliverable' (line 214) — add docs/BUXORO_REFUSALS.md REFUSAL 5 to the list of things Claude writes, and add the project.godot [audio] section.
- Step 14 'Files' (line 216) — add `docs/BUXORO_REFUSALS.md` and `project.godot`; state explicitly that `scripts/fx/Sfx.cs` is NOT modified.
- Step 14 'User does (Godot editor)' (line 218) — replace with the §userAction text; the current wording ('Load default_bus_layout.tres in Project Settings') understates it, since the layout must also be re-saved from the editor for the bus names to register, and BuxoroAudio must be added to the episode scene root.
- Step 14 'Verified by' (line 220) — prepend tests T1-T4 and T6 from testToAdd; the current acceptance is entirely subjective/oracle-based and would pass with ±8% jitter and 60 m attenuation intact.
- Step 16 'Files' (line 256) — docs/BUXORO_REFUSALS.md is finalised here and now carries five refusals, not four.
- Line 41 (grafts summary) and line 19 (executive summary) — both say 'the four written REFUSALS'; update to five and name the new one.
- Line 385-387 (BLOCKER 3) — mark resolved, and strike the first clause of its proposed fix ('Call Sfx.PlayAt(..., pitchSpread: 0.0f)') as insufficient so no future reader implements it.
- Line 447-449 (BLOCKER 13) — its audio half is DISSOLVED by REFUSAL 5, not fixed separately. Explicitly cancel its proposed fix: do NOT add bus/maxDistance parameters to Sfx.cs, and do NOT build a per-sound MaxDistance table. Keep only its separate point about the Humo bird's VISUAL cull range (DistanceCull's 90 m), which is a different subsystem.
- Line 515 (critic summary 'Actually') — the Sfx half is now answered by REFUSAL 5; the DistanceCull half remains open.

#### Further findings the critics missed — 7

- WIND LOOP LENGTH IS WRONG IN THE PLAN, in the same step. Plan lines 299 and 345 both specify 'one 20 s noise loop' for the wind bed. audio.js:36 is `const n = ctx.sampleRate * 2;` — the buffer is 2.0 seconds, and it is the SAME cached buffer every noise-based SFX slices from (audio.js:34-41). A 2 s band-passed noise loop at 420 Hz / Q 0.6 has audible periodicity that a 20 s loop does not, so this is a real texture change and the plan's own acceptance test ('the wind must audibly BREATHE on an ~11 s cycle') would pass while the sound is wrong, because the 11 s breathing comes from the LFO and is independent of the loop length. Fix: 2.0 s, generated once, shared with the SFX.
- THE NOISE SLICE OFFSET IS UNSPECIFIED AND THE OBVIOUS IMPLEMENTATION IS WRONG. audio.js:81 is `s.start(t); s.stop(t + o.uzunlik);` — `start(when)` with no offset argument always plays from sample 0. So bolga's first layer, qadam, tuyoq, qilich's first layer, kamon and blok all take the identical opening 0.07–0.13 s of one buffer, and are bit-identical on every trigger. A reasonable implementer told only 'a slice of one 2 s white-noise buffer' (plan line 299) would naturally pick a random or rotating offset to avoid an obviously repetitive footstep — which would add exactly the run-to-run variation this blocker exists to remove, through a different door. The spec must say 'from sample 0, every time'.
- THE PLAN HAS NO MAPPING AT ALL FOR THE FOCUS MUTE. audio.js:168-171 (`ovoz`) plus main.js:336-340 (visibilitychange) is a real shipped behaviour: alt-tabbing ramps Master 0.55 → 0.0 over 0.2 s and suppresses new one-shot creation. Grepping the plan for 'ovoz', 'visibility' or 'focus' finds nothing in the audio row. Under the mandate this is a cut feature, not a simplification. It also belongs to this blocker rather than a separate one, because it is a Master-bus behaviour that only becomes expressible once the bus layout exists.
- BLOCKER 13's PROPOSED FIX MUST BE ACTIVELY CANCELLED, NOT JUST SUPERSEDED. Plan line 449 proposes 'either add optional bus and maxDistance parameters to Sfx.PlayAt ... or give Buxoro its own thin BuxoroSfx wrapper. Name in the audio mapping which sounds are 3D and what each one's MaxDistance is — the bird, the anvil and the wall impacts all exceed 60 m.' That instruction is incompatible with this blocker's resolution: NO Buxoro sound is 3D, so there is no per-sound MaxDistance table to write, and modifying scripts/fx/Sfx.cs would touch a file the main ChronoShift game depends on at nine call sites (Vehicle.cs:246,276; Npc.cs:144; Enemy.cs:230,341; Target.cs:49; WeaponController.cs:647,654,655) for no benefit. Two blockers, one edit. If both fixes are applied as written the port ends up with a MaxDistance table that the code never reads, which is worse than either alone because it documents a behaviour that does not exist.
- A SIXTH REUSE-BY-NAME IS LURKING IN THE SAME HELPER. Sfx.Pick (Sfx.cs:66-69) selects a random stream via GD.RandRange. It is harmless for Buxoro only because no Buxoro sound has a multi-sample set — all 15 are single deterministic recipes. Worth one sentence in REFUSAL 5 so that a later contributor adding 'a few footstep variations' recognises it as a fidelity break rather than a natural extension: the web plays the identical qadam waveform every 0.46 s forever, deliberately.
- PAUSE SEMANTICS DIFFER BETWEEN THE THREE LAYERS AND THE PLAN ONLY COVERS MUSIC. Plan line 220 correctly demands that music stay dead after pause/resume (main.js:193-204's musiqaEdi carve-out). It says nothing about the other two. In the web, WebAudio nodes run on ctx.currentTime, so during pause: the wind loop KEEPS PLAYING at its current level (shamolGain is untouched by pauzaOch), and any in-flight one-shot FINISHES. Godot's default ProcessMode.Inherit would stop both dead the moment get_tree().paused goes true — a silence the web never has. Hence ProcessMode.Always on the wind player, on every note player and on every one-shot, plus ignoreTimeScale: true on the shogird/vaqt layer timers (audio.js:108,111 use setTimeout, which no pause affects).
- SIMULTANEITY IS A FEATURE, NOT A BUG TO GUARD. combat.js:473 fires olim per enemy death and the wave spawner can kill many in one frame; game.js:268 fires bolga at 6 Hz for as long as F is held. The web creates an unbounded number of overlapping gain nodes and never limits them. A performance-minded implementer will be tempted to add a voice cap, a same-sound cooldown or a distance cull to BuxoroAudio.Chal — all three are behaviour changes. The spec says so explicitly; keep that sentence when the row is pasted in.

---

### Correction 4. Markup — dialogue and card strings carry HTML the plan never accounted for

**Verdict on the original finding:** `confirmed_but_critic_incomplete`

The blocker is real: markup-bearing strings exist, the plan says nothing about them, and a plain Label would print "<b>" and "<br>" literally on screen. But the critic's proposed fix is wrong on the one detail it makes load-bearing, and the inventory it gives is a small fraction of the real one.

WRONG: "the speaker name renders bold" and "'vaqt' is emphasised". The CSS rule is `#gap b { display: block; font-style: normal; font-size: 1.8vmin; letter-spacing: .16em; opacity: .6; margin-bottom: .7vmin; }` (styles.css:155-156). `display:block` means the `<b>` is NOT inline — CSS generates anonymous block boxes around the sibling text, so each `<b>` gets its own full-width centred line. And the same rule hits BOTH `<b>`s in the line, because it is a descendant selector on `#gap`, not a speaker-only class. I rendered the real rule in a browser to confirm: at 1280x720 the speaker "TAROBIY" and the word "vaqt." come back with byte-identical computed style — display block, font-style NORMAL (the surrounding body is italic), font-size 12.96px against the body's 16.56px, font-weight 700, opacity 0.6, letter-spacing 2.0736px — and t1b lays out as four stacked centred blocks, with "vaqt." alone on line 4 of 5, SMALLER and DIMMER than the sentence around it. The thesis word is emphasised by isolation, tracking and weight while being de-emphasised in size and opacity. `[b]vaqt.[/b]` in a RichTextLabel produces the opposite: inline, italic-inherited, full size, full opacity, no line break. That is a visible fidelity break, and it is exactly the kind of "ChronoShift near-equivalent that behaves differently" the mandate forbids.

ALSO WRONG (harmless but misleading): "the ending card [is a] RichTextLabel with BBCode on". No card string contains inline markup — I grepped every string literal in all 20 JS files; `<b>` appears in exactly two data values in the whole game, `GAP.uz.t1b` and `GAP.en.t1b`. Cards carry only `<br>`. Three plain Labels (they need three different font sizes, styles and opacities anyway) plus a Button is both sufficient and more faithful than one RichTextLabel.

INCOMPLETE: the critic found 4 of the markup-bearing surfaces. There are 10, plus two silent data-loss hazards in the CSV pipeline that nothing in the plan covers: 30 of the 50 GAP values begin with a literal ASCII double-quote character, which RFC4180/Godot's CSV importer will eat unless each field is quoted and the inner quotes doubled; and the pause-menu controls table is 26 markup-structured Uzbek-only strings living in index.html, outside all four JS dictionaries the plan's CSV inventory enumerates.

<details><summary>Evidence checked — 14 source reads</summary>

- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/hud.js:173` : `el.gap.innerHTML = (kim ? '<b>' + kim + '</b>' : '') + matn;`
  The dialogue banner is assembled by innerHTML. The speaker is wrapped in <b>, and `matn` is injected as raw HTML, not text — so any markup inside the string is live. The critic cited this as hud.js:171; the actual line is 173.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/styles.css:155-156` : `#gap b { display: block; font-style: normal; font-size: 1.8vmin;          letter-spacing: .16em; opacity: .6; margin-bottom: .7vmin; }`
  THE KEY LINE THE CRITIC DID NOT READ. Descendant selector, so it applies to EVERY <b> under #gap — speaker label and the word 'vaqt.' alike. display:block forces its own line; font-style:normal cancels the italic that #gap sets; 1.8vmin is SMALLER than the body's 2.3vmin; opacity .6 is DIMMER. font-weight is not set, so the UA default bold (700) also applies. Six properties, only one of which is bold. `[b]…[/b]` reproduces one sixth of it.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/styles.css:148-153` : `#gap {   position: absolute; bottom: 5vmin; left: 50%; transform: translateX(-50%);   max-width: 62vmin; text-align: center; font-size: 2.3vmin; line-height: 1.55;   font-style: italic; opacity: 0; transition: opacity .5s;   text-shadow: 0 .2vmin 1vmin rgba(0,0,0,.95); }`
  Banner body: italic 2.3vmin, unitless line-height 1.55 (inherited as a NUMBER, so each block computes its own leading from its own font-size), max-width 62vmin, centred, 5vmin from the bottom. #gap.yon sets opacity .95 (styles.css:154), which MULTIPLIES the .6 on the b — effective alpha 0.57, not 0.6.
- `browser measurement of styles.css:148-156 at 1280x720 (vmin=720)` : `{"gap":{"h":127,"w":446,"fontStyle":"italic","fontSize":"16.56px","lineHeight":"25.668px"},"bs":[{"text":"TAROBIY","top":557,"h":20,"display":"block","fontStyle":"normal","fontWeight":"700","fontSize":"12.96px","opacity":"0.6","ls":"2.0736px","mb":"5.04px"},{"text":"vaqt.","top":633,"h":20,"display":"block","fontStyle":"normal","fontWeight":"700","fontSize":"12.96px","opacity":"0.6","ls":"2.0736px","mb":"5.04px"}]}`
  Ground truth, not inference. I reproduced the exact rules and ran t1b with speaker TAROBIY through a real browser. The speaker and 'vaqt.' come back with IDENTICAL computed style. Block heights confirm the unitless-line-height maths: 1.55x12.96 = 20.09 for the b blocks, 1.55x16.56 = 25.67 for body lines; total 20.09+5.04+25.67+25.67+20.09+5.04+25.67 = 127.27 = the measured 127. The screenshot shows five centred lines: TAROBIY / "Bizda qurol yo'q. Odam ham kam. Bizda faqat bitta narsa / bor — / v a q t . / Va uni ham yer beradi." — with 'vaqt.' visibly smaller and dimmer than the text above it.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/game.js:10` : `t1b: '"Bizda qurol yo‘q. Odam ham kam. Bizda faqat bitta narsa bor — <b>vaqt.</b> Va uni ham yer beradi."',`
  One of only TWO data values in the entire game carrying inline markup (the other is its EN twin at game.js:38, '— <b>time.</b>'). Note also: the value begins and ends with a literal ASCII double-quote character — a CSV hazard, see below.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/game.js:23` : `ochilish: '"Ular kelganida men sakkiz yoshda edim. Endi yigirma to‘qqizdaman.<br>Shahar hali ham kul hidi anqiydi."',`
  The opening narration card. One <br>. EN twin at game.js:51. This card is rendered by main.js:133, not by game.js — a surface the plan never assigns to any step.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/main.js:133` : `kadr.innerHTML = '<div class="matn">' + window.GAME.g('ochilish') + '</div>';`
  The opening narration card reuses the SAME #kadr element as the ending card, with only the .matn div. Hidden 2000 ms later (main.js:144) with the 1.2 s opacity transition from styles.css:162. Critic 1's own summary already noted 'the opening narration card (absent)' from the plan — this is where it lives.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/game.js:654-667` : `kadr.innerHTML =   '<div class="katta">' + g(galaba ? 'galaba' : olim ? 'olim' : 'maglubiyat') + '</div>' +   '<div class="matn">' +     (olim ? g('olim1') + '<br>' + g('olim2')           : g(galaba ? 'gal1' : 'mag1') + '<br>' + g(galaba ? 'gal2' : 'mag2')) +   '</div>' +   '<div class="stat">' +     g('sDevor') + ': ' + O.qurilganDevor + ' / ' + B.DEVOR_SEKSIYA_SONI + '<br>' + ... +     (galaba ? g('sVaqt') + ': ' + d + ':' + (s < 10 ? '0' : '') + s : '') +   '</div>' +   '<button id="qaytaTugma">' + g('qayta') + '</button>';`
  Ending card: four surfaces with four different type styles. The <br>s are CODE-GENERATED joins, not data — the strings themselves (gal1/gal2/mag1/mag2/olim1/olim2/sDevor/...) are clean. So the cards need line-break handling, not inline markup. I measured the trailing-<br> question: on a non-victory ending the last emitted '<br>' is followed by an empty string and Chrome produces NO phantom line box (4 lines = 120.94px vs victory's 5 lines = 151.17px at line-height 2 / 2.1vmin), so the port must TrimEnd the newline rather than leave it.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/chizma.js:35,95-97` : `d.innerHTML = '<b></b><i></i><em></em>'; ... elNom.textContent = d.nom; elRetsept.textContent = d.retsept; elIzoh.textContent = d.izoh;`
  The blueprint viewer's innerHTML is EMPTY SCAFFOLDING — three tags used as style hooks (styles.css:248-263 gives each a different size, colour and a delayed fade). All three text values go in via textContent. So chizma content carries NO markup: three plain Labels, not one RichTextLabel. Same pattern in videolar.js:46-50 and menyu.js:131-153 — structural innerHTML, clean interpolated strings.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/hud.js:61` : `el.soat.innerHTML = '<span class="qum">⏳</span>' + d + ':' + (s < 10 ? '0' : '') + s;`
  The clock is a MIXED-STYLE line, not one string with a glyph prefix. styles.css:17-21 gives the digits 6.4vmin/weight 700/letter-spacing .06em; styles.css:21 gives .qum 4.2vmin/opacity .75/margin-right .8vmin. The plan's MINOR-2 fix ('keep the hourglass glyph in the clock label') would put a 45px glyph at 69px and full opacity. It needs two sibling Labels.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/index.html:103-119` : `<div class="boshqaruvJadval" id="yordamKlav">   <div><b>W A S D</b><span>yurish</span></div>   ... 9 rows ... <div class="boshqaruvJadval" id="yordamSensor" hidden>   <div><b>Chap yarim</b><span>yurish (to‘liq bursangiz yuguradi)</span></div>   ... 4 rows ...`
  MISSED BY THE CRITIC AND BY THE PLAN. 13 rows x 2 fields = 26 user-facing strings, markup-structured (<b> key at flex 0 0 16vmin in #f2c14e, <span> description at opacity .7 — styles.css:391-392), UZBEK-ONLY with no English anywhere in the repo, and living in index.html rather than in any of the four JS dictionaries the plan's CSV row enumerates ('HUD MATN 22 keys · GAP ~30 · menyu SERIYA+MISSIYALAR · main pauzaMatn · chizma MALUMOT'). main.js:229-232 clones this innerHTML into the pause overlay, so it is reachable from two places.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/game.js:8-67 (measured)` : `30 of the 50 GAP values match /^'"/ — keys t1a t1b t1c yugur bozor t2a t2b olim gal1 gal2 mag1 mag2 ochilish olim1 olim2, in both uz and en`
  CSV HAZARD THE PLAN DOES NOT COVER. 30 values begin AND end with a literal ASCII double-quote character (the in-fiction quotation marks). Under RFC4180, which Godot's CSV translation importer follows, a field starting with `"` must be quoted and every inner quote doubled — `"""Yugur!"""`. Author it naively and the leading and trailing quotes vanish or the row splits. 53 of the 62 dictionary lines also contain commas. Separately, olim1/olim2 uz use the STRAIGHT ASCII apostrophe (Bolg'a, ko'taradigan) while the rest of the file uses U+2018 (yo‘q) — preserve verbatim, do not normalise.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/main.js:345-362` : `function fileProtokoli() { return location.protocol === 'file:'; } ... yordam.innerHTML =   '<b style="color:#d9573a">Bu o‘yinni fayl sifatida ochib bo‘lmaydi.</b><br><br>' + ... 8 more lines of <b>/<br>/<code> ...`
  The heaviest markup block in the codebase, and it must NOT be ported. It fires only when location.protocol === 'file:' — a browser CORS condition with no Godot equivalent. Write it into BUXORO_REFUSALS.md explicitly, or a future reader auditing string coverage will 'find a missing string' and port a browser error message into a desktop game.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/hud.js:9-17 (counted)` : `belgila kutma qur yolla min yasa devorTurdi kamonTayyor qilichTayyor shogirdQoshildi resursYetmadi navbatTola tosh yogoch teri temir komir kaltak kamon qilich jon haliYoq kamonOzimga`
  MATN has exactly 23 keys per language. The plan's Data-resources row says 22; the critic's MINOR-2 says 26. BOTH ARE WRONG. GAP has 25 literal entries per language resolving to 24 distinct keys (the `olim` collision at game.js:16/:25 and :44/:53, already correctly caught in the plan's line 326). Grand total for the dialogue/card half of buxoro.csv: 23 + 24 = 47 keys.

</details>

#### Corrected specification

##### Markup contract (new subsection, insert after "System mapping", before "Data resources")

Two of the game's 47 dialogue/card strings carry inline HTML and two carry line breaks. Everything else that looks like markup in the JS is empty structural scaffolding for CSS hooks, not content. This section is the single authority on what reaches a Control's text property.

###### C.1 — The inventory. Every markup-bearing string in the source, complete.

DATA-BORNE MARKUP — lives inside a translatable value, must survive the CSV:

| key | file:line | markup | what it must render as |
|---|---|---|---|
| `gap_t1b` (uz) | game.js:10 | one `<b>vaqt.</b>` | see C.3 — its own line, smaller and dimmer, NOT inline bold |
| `gap_t1b` (en) | game.js:38 | one `<b>time.</b>` | same |
| `gap_ochilish` (uz) | game.js:23 | one `<br>` | a hard line break, two lines |
| `gap_ochilish` (en) | game.js:51 | one `<br>` | same |

That is the whole list. I grepped every string literal in all 20 files of src/js: no other translatable value contains a `<`, and no value anywhere contains a `[` or `]`, so no pre-existing text needs BBCode escaping today (guard for it anyway — C.5).

CODE-BORNE MARKUP — generated at display time, never in the CSV:

| site | file:line | markup | port |
|---|---|---|---|
| speaker label | hud.js:173 | `'<b>' + kim + '</b>'` | a separate block Label, C.3 |
| ending card body | game.js:657-658 | two lines joined by `<br>` | `\n` |
| ending card stats | game.js:661-665 | 4 or 5 lines joined by `<br>` | `\n`, TrimEnd |
| clock | hud.js:61 | `<span class="qum">⏳</span>` + digits | two sibling Labels, C.6 |
| resource row | hud.js:87-88 | `<i>` swatch + `<b>` count + `<span>` name | ColorRect + 2 Labels, C.6 |
| blueprint cell | hud.js:105-107 | `<b>` index + name | 2 Labels, C.6 |
| mission bullets | menyu.js:153 | `<ul><li>` over a string ARRAY | C.7 |
| controls table | index.html:103-119 | 13 rows of `<b>`+`<span>` | C.8 |
| file:// warning | main.js:354-362 | 10 lines of `<b>/<br>/<code>` | REFUSED, C.9 |

STRUCTURAL-ONLY innerHTML, carries no text — do not chase these: chizma.js:35 (`'<b></b><i></i><em></em>'`, three empty style hooks; all three values arrive via `textContent` at chizma.js:95-97), videolar.js:46-50, hud.js:77-78, hud.js:153, menyu.js:131-133/149-154. In every one of these the interpolated strings are clean.

###### C.2 — The pipeline. One expander, asserted.

buxoro.csv stores the web spelling VERBATIM — `<b>`, `</b>`, `<br>` — so the CSV diffs line-for-line against game.js and the plan's "fidelity checked mechanically" property extends to the string table. Godot never treats `<` as special, so an unexpanded string would print its tags on screen; the expansion is therefore mandatory and lives in exactly one file.

`scripts/buxoro/ui/BuxoroMarkup.cs` — static, no state, the only code in the episode that knows what `<b>` means:

- `public static List<Blok> GapBloklar(string xom)` — splits on `<b>`/`</b>` into an ordered list of `(string Matn, bool Bosh)`. `gap_t1b` yields 3 blocks (body, bold-block, body); every other GAP key yields 1.
- `public static string Qatorlar(string xom)` — `xom.Replace("<br>", "\n")`, for card surfaces.
- `public static string Qochir(string xom)` — `xom.Replace("[", "[lb]")`, applied FIRST and unconditionally, before any BBCode is built. No current string needs it; a future translator's `[` would otherwise be swallowed by a RichTextLabel.
- `public static bool Tozami(string s)` — `!s.Contains('<')`. Called by the C.10 assertion.

Nothing else may assign to a dialogue or card Control's `Text`.

###### C.3 — The dialogue banner. THE FIDELITY POINT.

`#gap b` (styles.css:155-156) is `display: block`. It is a descendant selector, so it hits the speaker label AND the `<b>vaqt.</b>` inside t1b with the same six properties. Verified by rendering the real rule: both come back `display:block, font-style:normal, font-weight:700, font-size:12.96px, opacity:0.6, letter-spacing:2.0736px` at vmin 720, against a body of italic 16.56px. t1b lays out as FIVE centred lines, with `vaqt.` alone on line 4, **smaller and dimmer than the sentence around it**.

REJECTED: `[b]vaqt.[/b]` in a RichTextLabel. It renders inline, italic-inherited, at 100% size and 100% opacity, with no line break — a different image, and it inverts the emphasis the author built (isolation + tracking + weight, against a REDUCTION in size and alpha).

**Node: `Gap`, a `VBoxContainer`** (NOT a RichTextLabel, NOT a Label), anchored bottom-centre, `custom_minimum_size.x = 669.6` (62vmin), `offset_bottom = -54` (5vmin), `alignment = Center`, theme constant `separation = 0`. It holds a fixed pool of 4 reusable `Label` children (`Blok0..Blok3`), each `autowrap_mode = Word`, `horizontal_alignment = Center`, hidden by default. Four is the maximum: 1 speaker + 3 body runs, reached only by t1b.

`Gap(string kim, string matnKey, float davomiylik)`:
1. resolve `matn = Tr(matnKey)` ONCE and cache it — the plan's existing fidelity decision at mapping line 297 (a line on screen keeps its old language until it expires) applies unchanged.
2. blocks = (kim non-empty ? [(kim, Bosh:true)] : []) ++ `BuxoroMarkup.GapBloklar(matn)`.
3. assign block i to `Blok{i}`, `LabelSettings = blok.Bosh ? LsGapBosh : LsGapMatn`, show it; hide the rest.
4. `davomiylik` defaults to 5 s (hud.js:175), fade via a 0.5 s Tween on `modulate.a` (styles.css:151).

Two `LabelSettings` .tres under `res://resources/theme/buxoro/` (content in .tres, per CLAUDE.md). All px at 1920x1080 canvas_items stretch, where 1 vmin = 10.8 px:

`ls_gap_matn.tres` — serif ITALIC face, `font_size = 24.84` (2.3vmin), `line_spacing = 13.66` (so the line box is 1.55 x 24.84 = 38.50), `font_color = Color(0.910, 0.875, 0.788, 0.95)` (#e8dfc9 at the .95 of `#gap.yon`), shadow offset (0, 2.16) blur 10.8 alpha .95.

`ls_gap_bosh.tres` — serif BOLD UPRIGHT face wrapped in a `FontVariation` with `spacing_glyph = 3` (.16em of 19.44 px = 3.11, rounded — Godot takes integer glyph spacing), `font_size = 19.44` (1.8vmin), `line_spacing = 10.69 + 7.56 = 18.25` (30.13 px line box plus the `margin-bottom: .7vmin`), `font_color = Color(0.910, 0.875, 0.788, 0.57)`. **0.57, not 0.6** — CSS `opacity` on `#gap.yon` (.95) multiplies the `.6` on the `<b>`. A naive .6 is 5% too bright.

ACCEPTABLE FALLBACK, if someone insists on one node: a single `RichTextLabel` with `bbcode_enabled`, text `[center]…[font=res://…/fv_gap_bosh.tres][font_size=19]…[color=#e8dfc991][b]vaqt.[/b][/color][/font_size][/font]…[/center]` with explicit `\n` before and after. It is off by 8.37 px of leading on the two b-blocks, because RichTextLabel applies one `line_separation` to every line while CSS's unitless `line-height: 1.55` scales per-block. Write that number down if you take this route, so it is a decision and not an accident.

###### C.4 — The cards. Labels. No BBCode anywhere.

`#kadr` serves TWO screens through the same element. Build one `Kadr` scene, a full-rect `ColorRect` at `#0a0806` over a `VBoxContainer`, `padding 64.8` (6vmin), fade 1.2 s.

Children: `Katta` (Label), `Matn` (Label), `Stat` (Label), `Qayta` (Button). No child needs mixed styling, so no child is a RichTextLabel.

- `ls_kadr_katta.tres` — upright, 59.4 px (5.5vmin), `FontVariation.spacing_glyph = 6` (.1em), alpha 1.0, bottom margin 21.6 px (2vmin).
- `ls_kadr_matn.tres` — ITALIC, 30.24 px (2.8vmin), `line_spacing` to reach a 57.46 px box (line-height 1.9), alpha 0.92, `Matn.custom_minimum_size.x = 756` (70vmin), autowrap Word, centred.
- `ls_kadr_stat.tres` — UPRIGHT, 22.68 px (2.1vmin), `line_spacing` to reach a 45.36 px box (line-height 2), alpha 0.70, top margin 43.2 px (4vmin).
- `Qayta` — top margin 43.2, padding 15.12 / 43.2, font 24.84, `spacing_glyph = 3` (.12em), transparent StyleBoxFlat, border 2.38 px rgba(232,223,201,.5), radius 4.32; hover fill rgba(232,223,201,.12), border #e8dfc9.

OPENING NARRATION (main.js:131-145 — a surface no step currently owns; give it to Step 17 alongside the endings, since it is the same scene): show `Kadr` with only `Matn` visible, `Matn.Text = BuxoroMarkup.Qatorlar(Tr("gap_ochilish"))` → two lines. Hide after 2000 ms with the 1.2 s fade, while the 6.0 s opening camera move continues underneath.

ENDING CARD (game.js:647-671), fired 3.5 s after `yakunla` and after any ending video:
- `Katta.Text = Tr(galaba ? "gap_galaba" : olim ? "gap_olim_sarlavha" : "gap_maglubiyat")` — note this consumes `gap_olim_sarlavha`, the plan's already-correct split of the game.js:16/:25 duplicate-key collision (Data-resources row, line 326).
- `Matn.Text = olim ? Tr("gap_olim1") + "\n" + Tr("gap_olim2") : Tr(galaba?"gap_gal1":"gap_mag1") + "\n" + Tr(galaba?"gap_gal2":"gap_mag2")`.
- `Stat.Text` = the four `sDevor/sQurol/sShogird/sDushman` rows joined by `\n`, plus `sVaqt` only on victory. **TrimEnd the newline.** I measured the original: a trailing `<br>` with an empty string after it produces NO phantom line box in Chrome — victory is 5 line boxes (151.17 px), defeat is 4 (120.94 px) at 1280x720. Leaving the `\n` in would push the RESTART button 45.36 px down on every defeat.

###### C.5 — CSV authoring rules. Get these wrong and strings silently break.

1. **30 of the 50 GAP values begin AND end with a literal ASCII `"`** — keys t1a t1b t1c yugur bozor t2a t2b olim gal1 gal2 mag1 mag2 ochilish olim1 olim2, in both languages. Godot's CSV translation importer is RFC4180: a field starting with `"` MUST be wrapped in quotes with every inner quote doubled. `gap_yugur,"""Yugur!""","""Run!"""`. Author the CSV with a real CSV writer, never by hand-joining with commas.
2. 53 of the 62 dictionary lines contain a comma. Same rule.
3. Preserve every character verbatim, including the inconsistency: `gap_olim1`/`gap_olim2` uz use a STRAIGHT apostrophe (`Bolg'a`, `ko'taradigan`) where the rest of the file uses U+2018 (`yo‘q`). Do not normalise. Also preserve U+23F3 ⏳, U+232F ⌾, U+203A ›, U+00B7 ·, U+2014 —, U+2018/U+2019.
4. `gap_otMaslahat` contains the placeholder `{n}`, replaced with `BALANCE.DEVOR_BONUS` at game.js:394. It must survive the CSV untouched and be substituted after `Tr()`, not before.
5. Key counts, corrected against source: MATN = **23** keys per language (hud.js:9-17, not the 22 in the plan's Data-resources row and not the 26 in critic MINOR-2). GAP = 25 literal entries per language resolving to **24** distinct keys after the `olim` collision. 47 keys for the dialogue/card half of the table.

###### C.6 — HUD surfaces that are mixed-style lines, not markup

- **Clock** (hud.js:61): `Soat` is an `HBoxContainer` of two Labels. `Qum` — the ⏳ glyph, 45.36 px (4.2vmin), alpha .75, right margin 8.64 px (.8vmin). `Raqam` — the digits, 69.12 px (6.4vmin), bold 700, `spacing_glyph = 4` (.06em), `#f0e6cd`, tabular figures. One Label with a glyph prefix would draw the hourglass at 69 px and full opacity. This supersedes the fix on critic MINOR-2, which said only "keep the hourglass glyph in the clock label".
- **Resource row** (hud.js:87-88): per item an `HBoxContainer` with `separation = 8.64`, holding a `ColorRect` 20.52x20.52 (1.9vmin) radius 3.24 with a 2.16 px rim rgba(240,230,205,.35); a count Label bold 700, tabular, `custom_minimum_size.x = 30.24` (2.8vmin), right-aligned; a name Label alpha .8, `spacing_glyph = 2` (.06em).
- **Blueprint cell** (hud.js:105-107): a `PanelContainer` 102.6 px wide (9.5vmin) holding an `HBoxContainer` of an index Label (bold, "1"/"2"/"3") and a name Label, both 16.74 px (1.55vmin), `spacing_glyph = 2` (.09em). The `qulf` state appends `" ⌾"` at alpha .7 (`styles.css:81`); the `olik` state clears it (`styles.css:86`) and paints panel, border and text all `#0a0908`.

###### C.7 — Mission bullets are an ARRAY, not a string

`menyu.js:53-60, 72-76, 87-91, 102-108`: `MISSIYALAR[i].maqsad` is a string array rendered as `<li>`s with a `·` pseudo-element bullet in `#f2c14e` at alpha .8 (styles.css:380-382). **14 bullets per language** (toliq 4, parda1 3, parda2 3, parda3 4). One CSV key each: `menyu_maqsad_<kalit>_<n>` — `menyu_maqsad_toliq_1` … `menyu_maqsad_parda3_4`, 28 rows total. Render as a `VBoxContainer` of `HBoxContainer(Label("·", #f2c14e alpha .8), Label(text))`, bullet column 17.28 px (1.6vmin), text 17.28 px (1.6vmin) alpha .58, line box 1.9. A single Label with `"· "` prefixes loses the separate bullet colour.

###### C.8 — The controls table is 26 untranslated strings outside every dictionary

`index.html:103-119` holds 13 rows (`yordamKlav` 9, `yordamSensor` 4) of `<b>key</b><span>description</span>`. They are **Uzbek-only — there is no English version anywhere in the repo** — and they live in HTML, so the plan's CSV inventory ("four scattered JS dictionaries plus MALUMOT") does not reach them. Add 26 keys: `yordam_klav_<n>_kalit` / `yordam_klav_<n>_izoh` (n = 1..9) and `yordam_sensor_<n>_kalit` / `_izoh` (n = 1..4). Layout per row: `HBoxContainer`, separation 21.6 px (2vmin), bottom border 1 px rgba(232,223,201,.10), padding-block 9.18 px (.85vmin), font 19.98 px (1.85vmin); key Label fixed width 172.8 px (16vmin) in `#f2c14e` weight 600; description Label alpha .7. Table `max_width = 756` (70vmin).

**USER DECISION REQUIRED, one row in the plan:** the English column for these 26 keys does not exist in the source. Literal 1:1 means the controls table shows Uzbek even in EN mode. Ship that as the default (`_yordamTarjimaQoshildi = false`, the uz string copied into both columns) and flag it beside the `gap_olim` decision as the second place the user should actually choose, because unlike `gap_olim` this one is not a bug — it is simply untranslated content, and writing 13 English rows is authoring, not porting.

###### C.9 — Refused, in writing

`main.js:345-368` `fileOgohlantirish()` — 10 lines of `<b>/<br>/<code>` explaining that a browser cannot open the game over `file://`. It fires only on `location.protocol === 'file:'` (main.js:346). Godot has no such state. NOT PORTED. Record it in BUXORO_REFUSALS.md with this reason, or a later string-coverage audit will "find a missing string" and port a browser CORS error into a desktop game.

###### C.10 — The assertion that makes this self-enforcing

In `BuxoroHud._Ready()`, DEBUG only: iterate every key in buxoro.csv in both locales, run it through `BuxoroMarkup`, and `PushError` unless (a) `Tozami()` holds for every expanded result — no `<` reaches a Control, (b) exactly 2 keys produce more than one Gap block, and they are `gap_t1b` uz and en with 3 blocks each, (c) exactly 2 keys produce more than one card line, and they are `gap_ochilish` uz and en with 2 lines each. If a translator later adds markup, the game says so on launch instead of printing tags at the player.

#### Code

// scripts/buxoro/ui/BuxoroMarkup.cs
// The only code in the episode that knows what <b> and <br> mean.
// buxoro.csv stores the web spelling verbatim so it diffs against game.js;
// Godot does not treat '<' as special, so expansion here is mandatory.
using System.Collections.Generic;

namespace ChronoShift.Buxoro.Ui;

public readonly struct GapBlok
{
    public readonly string Matn;
    public readonly bool Bosh;   // true => styled by #gap b: own line, 1.8vmin, upright, tracked, alpha .57
    public GapBlok(string matn, bool bosh) { Matn = matn; Bosh = bosh; }
}

public static class BuxoroMarkup
{
    // Escape a literal '[' so a future translator's bracket is not eaten by BBCode.
    // No string in the source needs this today (verified: zero '[' in any value).
    public static string Qochir(string xom) => xom.Replace("[", "[lb]");

    // <br> -> newline. Card surfaces only. hud.js never emits <br>.
    public static string Qatorlar(string xom) => xom.Replace("<br>", "\n");

    // Split on <b>...</b> into ordered blocks.
    // gap_t1b yields 3; every other GAP key yields 1.
    public static List<GapBlok> GapBloklar(string xom)
    {
        var chiqish = new List<GapBlok>();
        int i = 0;
        while (i < xom.Length)
        {
            int ochil = xom.IndexOf("<b>", i, System.StringComparison.Ordinal);
            if (ochil < 0) { Qosh(chiqish, xom[i..], false); break; }
            Qosh(chiqish, xom[i..ochil], false);
            int yopil = xom.IndexOf("</b>", ochil + 3, System.StringComparison.Ordinal);
            if (yopil < 0) { Qosh(chiqish, xom[(ochil + 3)..], true); break; }
            Qosh(chiqish, xom[(ochil + 3)..yopil], true);
            i = yopil + 4;
        }
        return chiqish;
    }

    private static void Qosh(List<GapBlok> royxat, string s, bool bosh)
    {
        // Trim only the join whitespace CSS would have collapsed at a block boundary.
        s = s.Trim(' ');
        if (s.Length > 0) royxat.Add(new GapBlok(s, bosh));
    }

    public static bool Tozami(string s) => !s.Contains('<');
}

// --- BuxoroHud.Gap: NOT a RichTextLabel. #gap b is display:block (styles.css:155). ---
// _gapBloklar is a fixed pool of 4 Labels under the Gap VBoxContainer.
// Max blocks = 1 speaker + 3 body runs, reached only by gap_t1b.
public void Gap(string kim, string matnKalit, float davomiylik = 5.0f)
{
    if (string.IsNullOrEmpty(matnKalit)) { _gap.Hide(); return; }

    // Resolve ONCE and cache: a line already on screen keeps its language
    // until it expires, matching GAP[H.til()][k] resolving at call time (game.js:69).
    string matn = BuxoroMarkup.Qochir(Tr(matnKalit));

    var bloklar = new List<GapBlok>();
    if (!string.IsNullOrEmpty(kim)) bloklar.Add(new GapBlok(kim, true));   // hud.js:173
    bloklar.AddRange(BuxoroMarkup.GapBloklar(matn));

    for (int i = 0; i < _gapBloklar.Length; i++)
    {
        if (i >= bloklar.Count) { _gapBloklar[i].Hide(); continue; }
        _gapBloklar[i].Text = bloklar[i].Matn;
        // alpha .57 on Bosh = #gap.yon opacity .95 x #gap b opacity .6. NOT .6.
        _gapBloklar[i].LabelSettings = bloklar[i].Bosh ? _lsGapBosh : _lsGapMatn;
        _gapBloklar[i].Show();
    }
    _gap.Show();
    _gapTaymer = davomiylik;   // hud.js:175 default 5 s, 0.5 s fade out
}

// --- Cards: plain Labels, no BBCode. game.js:654-667 ---
public void KadrYakun(bool galaba, bool olim, BuxoroStat st)
{
    _kadrKatta.Text = Tr(galaba ? "gap_galaba" : olim ? "gap_olim_sarlavha" : "gap_maglubiyat");

    _kadrMatn.Text = olim
        ? Tr("gap_olim1") + "\n" + Tr("gap_olim2")
        : Tr(galaba ? "gap_gal1" : "gap_mag1") + "\n" + Tr(galaba ? "gap_gal2" : "gap_mag2");

    var sb = new System.Text.StringBuilder();
    sb.Append(Tr("gap_sDevor")).Append(": ").Append(st.QurilganDevor).Append(" / ").Append(_bal.DevorSeksiyaSoni).Append('\n');
    sb.Append(Tr("gap_sQurol")).Append(": ").Append(st.YasalganQurol).Append('\n');
    sb.Append(Tr("gap_sShogird")).Append(": ").Append(st.ShogirdSoni).Append(" / ").Append(_bal.ShogirdMaksimum).Append('\n');
    sb.Append(Tr("gap_sDushman")).Append(": ").Append(st.OlganDushman).Append(" / ").Append(_bal.JamiDushman).Append('\n');
    if (galaba) sb.Append(Tr("gap_sVaqt")).Append(": ").Append(st.VaqtMinSek);
    // TrimEnd: measured, Chrome emits NO line box for the trailing <br> on a loss.
    // Victory 5 lines / 151.17 px, defeat 4 lines / 120.94 px at 1280x720.
    _kadrStat.Text = sb.ToString().TrimEnd('\n');

    _kadrQayta.Text = Tr("gap_qayta");
}

// --- Opening narration, main.js:131-145. Same Kadr scene, only Matn visible. ---
public void KadrOchilish()
{
    _kadrKatta.Hide(); _kadrStat.Hide(); _kadrQayta.Hide();
    _kadrMatn.Text = BuxoroMarkup.Qatorlar(Tr("gap_ochilish"));   // -> 2 lines
    _kadrMatn.Show(); _kadr.Show();
    GetTree().CreateTimer(2.0).Timeout += KadrYashir;             // 2000 ms, then the 1.2 s fade
}

// --- DEBUG-only guard so a later translator cannot print tags at the player. ---
private void MarkupTekshir()
{
    foreach (string locale in new[] { "uz", "en" })
    {
        int kopBlok = 0, kopQator = 0;
        foreach (string kalit in BuxoroCsvKalitlar.Hammasi)
        {
            string xom = TranslationServer.GetTranslationObject(locale).Get(kalit);
            var bl = BuxoroMarkup.GapBloklar(xom);
            if (bl.Count > 1) kopBlok++;
            string q = BuxoroMarkup.Qatorlar(xom);
            if (q.Contains('\n')) kopQator++;
            foreach (var b in bl)
                if (!BuxoroMarkup.Tozami(b.Matn))
                    GD.PushError($"buxoro.csv[{locale}].{kalit} still contains '<' after expansion: {b.Matn}");
        }
        if (kopBlok != 1) GD.PushError($"{locale}: expected exactly 1 multi-block key (gap_t1b), got {kopBlok}");
        if (kopQator != 1) GD.PushError($"{locale}: expected exactly 1 multi-line key (gap_ochilish), got {kopQator}");
    }
}

#### Test that proves it

**Step 11 check, replacing the critic's "speaker name renders bold / 'vaqt' is emphasised".**

Start a run, let the tutorial reach step 0→1, and wait for t1b (fires 4200 ms after t1a, holds 5 s — game.js:416). At 1920x1080 the banner must be FOUR centred blocks stacked upward from 54 px above the bottom edge, total height 190.9 px, wrap width 669.6 px:

1. `TAROBIY` — 19.44 px, bold 700, UPRIGHT, glyph spacing 3 px, alpha 0.57, own line, 45.2 px tall including its 7.56 px bottom margin.
2. `"Bizda qurol yo'q. Odam ham kam. Bizda faqat bitta narsa bor —` — italic 24.84 px, alpha 0.95, wrapping to 2 lines at 669.6 px, 77.0 px tall.
3. `vaqt.` — alone on its own line. 19.44 px, bold, UPRIGHT, glyph spacing 3 px, alpha 0.57. **It must be SMALLER AND DIMMER than the line above it, and must NOT sit inside that sentence.** 45.2 px tall.
4. `Va uni ham yer beradi."` — back to italic 24.84 px alpha 0.95, 38.5 px tall.

Then the same line in EN (`— time.`), identical geometry.

FAILURE THIS CHECK EXISTS TO CATCH: t1b reading as one flowing italic paragraph with a bold word in the middle. That is what `[b]…[/b]` produces and it is the wrong picture.

Cross-check against the original rather than by eye: open the web build at exactly 1280x720, trigger t1b, and run in the console —
`var g=document.getElementById('gap'); JSON.stringify({h:g.getBoundingClientRect().height, bs:[...g.querySelectorAll('b')].map(b=>({t:b.textContent, top:Math.round(b.getBoundingClientRect().top), h:Math.round(b.getBoundingClientRect().height)}))})`
It returns `{"h":127,"bs":[{"t":"TAROBIY","top":557,"h":20},{"t":"vaqt.","top":633,"h":20}]}` (measured). Screenshot the Godot build at 1280x720 and overlay: the two 20 px blocks must land at y 557 and y 633, ±2 px.

THREE MORE ROWS, all cheap:

- **Cards.** Reach the opening card: 2 lines, italic 30.24 px, 57.46 px leading. Reach a victory and a defeat ending: victory stats = 5 lines / 151.17 px tall at 1280x720 scaling; defeat = 4 lines / 120.94 px. The RESTART button must sit at the same offset below the last stat line in both — if it drops 45 px on defeat, the trailing `\n` was not trimmed.
- **No tags on screen.** The C.10 `_Ready` assertion must print nothing. Then play one full run in each locale and confirm no `<b>`, `</b>` or `<br>` is ever visible.
- **CSV round-trip.** Import buxoro.csv, then in a scratch scene print `Tr("gap_yugur")` in uz. It must be exactly `"Yugur!"` — 8 characters, leading and trailing ASCII double-quote present. If it comes back as `Yugur!` the CSV quoting was wrong, and 30 lines of dialogue lost their quotation marks.

#### What the user does in the Godot editor

Four things in the Godot editor, plus one content decision.

**1. Import three serif font faces — this is the real prerequisite, and nothing else in C.3 works without it.** Godot's default font is a sans-serif; the game is Georgia/Times (styles.css:7) and the contract needs three distinct faces simultaneously: upright regular, ITALIC (the entire dialogue banner and the card body are italic), and BOLD upright (the speaker label and `vaqt.`). Georgia is not redistributable — use **Gelasio** (SIL OFL, metric-compatible with Georgia): Gelasio-Regular.ttf, Gelasio-Italic.ttf, Gelasio-Bold.ttf into `res://assets/fonts/buxoro/`. In each .import set Antialiasing = Gray, Hinting = Light, Subpixel Positioning = Auto, and Multichannel SDF ON (the HUD scales text across resolutions). If you substitute a different serif, say so in PROGRESS.md — the px figures in C.3/C.4 are metric-derived and a non-metric-compatible face will change the wrap points.

**2. Create one FontVariation for letter-spacing.** Godot has no BBCode or theme property for CSS `letter-spacing`; it lives on FontVariation. Create `res://resources/theme/buxoro/fv_gap_bosh.tres` — base font Gelasio-Bold, `spacing_glyph = 3`. Create `fv_soat.tres` (Gelasio-Bold, `spacing_glyph = 4`) and `fv_kadr_katta.tres` (Gelasio-Regular, `spacing_glyph = 6`). Claude writes the LabelSettings .tres that reference these; you only need to confirm each one's `font` slot resolved and is not showing the fallback.

**3. Build the Gap node as a VBoxContainer, not a Label.** Under BuxoroHud: `Gap` = VBoxContainer, anchors bottom-centre, `offset_bottom = -54`, `custom_minimum_size = (669.6, 0)`, `alignment = Center`, theme constant `separation = 0`. Add exactly four Label children named `Blok0` `Blok1` `Blok2` `Blok3`, each with `autowrap_mode = Word`, `horizontal_alignment = Center`, `visible = false`. Four, not three — t1b with a speaker needs all four. If you build `Gap` as a single Label or RichTextLabel the code will not find the pool and the banner stays blank.

**4. Build the Kadr scene once and reuse it for both cards.** `Kadr` = Control full-rect → ColorRect `#0a0806` → MarginContainer (all margins 64.8) → VBoxContainer `alignment = Center` → Labels `Katta`, `Matn`, `Stat` and Button `Qayta`, in that order. `Matn.custom_minimum_size.x = 756`, `autowrap_mode = Word`, `horizontal_alignment = Center`. Both the opening narration and the ending stats use this one scene.

**5. Import the CSV with the quoting intact, then verify one string by hand.** After Project Settings → Localization → import buxoro.csv and adding buxoro.uz.translation and buxoro.en.translation: open a scratch scene, print `TranslationServer.GetTranslationObject("uz").Get("gap_yugur")`, and confirm it comes back as `"Yugur!"` **with** the leading and trailing ASCII double-quote characters, 8 characters long. If it prints `Yugur!` the CSV lost its RFC4180 quoting and 30 lines of dialogue have silently shed their in-fiction quotation marks. Do not proceed past Step 11 until that check passes.

**6. One content decision, yours not Claude's (C.8).** The 13-row controls table in index.html:103-119 exists only in Uzbek — there is no English original anywhere in the repo. The literal port therefore shows Uzbek controls text even with the language toggle on EN. Claude will ship exactly that (the uz string duplicated into the en column, behind `_yordamTarjimaQoshildi = false`). If you want English there, write the 13 English rows yourself and hand them over — that is authoring new content, not porting, and Claude will not invent it under a 1:1 mandate. This is the second of two places in the whole port where you have to decide rather than verify; the first is the `gap_olim` duplicate-key casualty already recorded at plan line 326.

#### Sections of this plan that this correction overrides

- NEW subsection 'Markup contract' (C.1-C.10), inserted between '## System mapping' (line 262) and '## Data resources' (line 316)
- Step 11 (lines 182-190) — add BuxoroMarkup.cs and the two ls_gap_*.tres / three ls_kadr_*.tres to **Files**; add the four-block t1b geometry check and the CSV round-trip check to **Verified by**; add the font import and the Gap VBoxContainer pool to **User does**
- Step 17 'Endings, death camera epilogue, stats card' (line 247) — currently owns no opening narration card; add main.js:131-145 and the shared Kadr scene (C.4)
- Step 13 'Chizma.cs blueprint viewer' (line 202) — record that chizma.js:35's innerHTML is empty scaffolding and all three values arrive via textContent, so the panel is three plain Labels, not a RichTextLabel
- Step 16 'Menyu.cs' (line 232) — add the 28 menyu_maqsad_* bullet keys (C.7) and the 26 yordam_* controls-table keys (C.8), neither of which any step's files list currently reaches
- System mapping row 'HUD DOM overlay — clock, hands, blueprints, resources, HP, prompt, hold bar, dialogue, toasts' (line 295) — the row names 13 Controls but does not say which are Labels and which are containers of Labels; add the C.6 mixed-style-line rule and the Gap VBoxContainer
- System mapping row 'UZ/EN runtime language toggle' (line 297) — add the pointer to the Markup contract and correct the dictionary inventory to include index.html's controls table
- Data resources row 'res://resources/translations/buxoro.csv' (line 326) — correct 'HUD MATN 22 keys' to 23 (verified hud.js:9-17); add GAP = 24 distinct keys; add the 28 bullet keys, the 26 controls keys, and the C.5 RFC4180 quoting rules
- Critic findings → BLOCKER 4 (lines 389-391) — replace the stated fix: the emphasis is display:block, not inline bold, so the banner is a VBoxContainer of Labels and the cards need no BBCode at all
- Critic findings → MINOR 2 (lines 465-467) — the fix is incomplete (the hourglass is a different size and opacity, so it is two Labels) and the key count is wrong in both directions: 23, not 22 and not 26
- NEW row in BUXORO_REFUSALS.md — main.js:345-368 fileOgohlantirish() is browser-only and is not ported (C.9)
- 'Wrong assumptions caught in the plan' (line 501) — add the MATN key count, and the assumption that all user-facing strings live in the four JS dictionaries

#### Further findings the critics missed — 12

- THE CRITIC'S EMPHASIS CLAIM IS BACKWARDS. `#gap b` is `display: block`, and the selector is a descendant selector so it applies to the `<b>` inside t1b exactly as it does to the speaker label. Rendered and measured: both come back `display:block, font-style:normal (against an italic body), font-weight:700, font-size:12.96px (against a 16.56px body), opacity:0.6, letter-spacing:2.0736px`. 'vaqt.' therefore renders on its own centred line, SMALLER and DIMMER than the sentence it belongs to. `[b]vaqt.[/b]` in a RichTextLabel renders it inline, italic, full size, full opacity — the opposite image on the word the critic itself calls the thesis of the game.
- THE CARDS NEED NO BBCODE AT ALL. `<b>` appears in exactly two data values in the entire codebase — GAP.uz.t1b (game.js:10) and GAP.en.t1b (game.js:38). Every card string (galaba, maglubiyat, olim, olim1, olim2, gal1, gal2, mag1, mag2, ochilish, sDevor..sDushman, qayta) is clean; the `<br>`s in kadrKorsat are code-generated joins, not data. Three plain Labels plus a Button is both sufficient and more faithful than one RichTextLabel, since the three surfaces need three different sizes, two different slants and three different alphas anyway.
- 30 OF THE 50 GAP VALUES BEGIN AND END WITH A LITERAL ASCII DOUBLE-QUOTE — the in-fiction quotation marks (t1a t1b t1c yugur bozor t2a t2b olim gal1 gal2 mag1 mag2 ochilish olim1 olim2, both languages). Godot's CSV translation importer is RFC4180: such a field must be quoted with inner quotes doubled (`"""Yugur!"""`). 53 of the 62 dictionary lines also contain commas. Nothing in the plan's CSV row mentions quoting. Author the file with a real CSV writer and verify one string round-trips, or 30 dialogue lines silently lose their quotation marks.
- THE PAUSE-MENU CONTROLS TABLE IS 26 STRINGS NO ONE HAS COUNTED. index.html:103-119, 13 rows of `<b>key</b><span>desc</span>`, Uzbek-only with no English anywhere in the repo, living in HTML rather than in any of the four JS dictionaries the plan's CSV inventory enumerates. main.js:229-232 clones this innerHTML into the pause overlay, so it is reachable from two screens. Needs 26 new keys, the C.8 two-column layout, and a user decision about the missing English.
- MISSION OBJECTIVES ARE A STRING ARRAY RENDERED AS A BULLET LIST. menyu.js:53-60/72-76/87-91/102-108 — `maqsad` is `string[]`, rendered by menyu.js:153 as `<li>`s with a `·` pseudo-element bullet in #f2c14e at alpha .8 while the text itself is #e8dfc9 at alpha .58 (styles.css:376-382). 14 bullets per language, 28 CSV rows. A single Label with '· ' prefixes loses the separate bullet colour. The plan's CSV row says only 'menyu SERIYA+MISSIYALAR'.
- THE CLOCK FIX IN CRITIC MINOR-2 IS INCOMPLETE. It says 'keep the hourglass glyph in the clock label'. But hud.js:61 is a mixed-style line: the ⏳ is 4.2vmin at opacity .75 with an 8.64 px right margin (styles.css:21) while the digits are 6.4vmin, weight 700, letter-spacing .06em (styles.css:17-18). One string in one Label draws the hourglass 52% too large at full opacity. It is two sibling Labels in an HBox.
- BOTH THE PLAN AND THE CRITIC HAVE THE MATN KEY COUNT WRONG. The plan's Data-resources row says 22; critic MINOR-2 says 26. Counted off hud.js:9-17: belgila kutma qur yolla min yasa devorTurdi kamonTayyor qilichTayyor shogirdQoshildi resursYetmadi navbatTola tosh yogoch teri temir komir kaltak kamon qilich jon haliYoq kamonOzimga = 23. GAP is 25 literal entries per language resolving to 24 distinct keys after the `olim` collision the plan already caught.
- THE OPACITY ON THE SPEAKER LABEL IS 0.57, NOT 0.6. CSS `opacity` compounds: `#gap.yon` is .95 (styles.css:154) and `#gap b` is .6 (styles.css:156), so the effective alpha of the speaker and of 'vaqt.' is .95 x .6 = 0.57. Setting the LabelSettings font_color alpha to .6 makes them 5% too bright, in both the banner's two styled blocks.
- THE ENDING STATS BLOCK NEEDS A TrimEnd AND THE REASON IS MEASURABLE. game.js:664 always emits a trailing '<br>' after sDushman, and on a non-victory ending it is followed by an empty string. I measured the original: Chrome emits NO line box for it — victory 5 lines / 151.17 px, defeat 4 lines / 120.94 px at line-height 2 / 2.1vmin. Port it as a `\n` and the RESTART button sits 45.36 px lower on every defeat than it does in the web build.
- main.js:345-368 MUST NOT BE PORTED AND THE PLAN SHOULD SAY SO. It is the heaviest markup block in the codebase — 10 lines of <b>/<br>/<code> — but it fires only when `location.protocol === 'file:'` (main.js:346), a browser CORS condition with no Godot equivalent. Add it to BUXORO_REFUSALS.md, or a later string-coverage audit finds 10 'missing' strings and ports a browser error message into a desktop game.
- NO USER-FACING STRING CONTAINS A LITERAL '[' OR ']' TODAY — I grepped every string literal in all 20 files. So no existing text needs BBCode escaping. Add BuxoroMarkup.Qochir() anyway and apply it first, unconditionally: the moment a translator writes a bracket it would be eaten silently by any RichTextLabel path, and the fallback route in C.3 uses one.
- THE SOURCE IS INTERNALLY INCONSISTENT ABOUT APOSTROPHES AND THE PORT MUST PRESERVE IT. game.js:26-27 uz uses the STRAIGHT ASCII apostrophe (`Bolg'a yerda qoldi`, `ko'taradigan odam`) where the rest of the same table uses U+2018 (`yo‘q`, `qo‘l`). Under a 1:1 mandate this is transcribed, not normalised. Same for U+23F3 ⏳, U+232F ⌾ (the locked-blueprint ::after glyph, styles.css:81), U+203A › and U+00B7 ·, none of which render in Godot's default font — another reason the font import in the user action is a hard prerequisite rather than a polish step.

---

### Correction 5. Determinism gate — the parity harness replays the wrong random stream and cannot pass

**Verdict on the original finding:** `confirmed_but_critic_incomplete`

The blocker is real and it does stop step 2. world.js consumes the world LCG inside tugunYarat's procedural branch, that branch is gated on MODELS.ol('tugun_'+tur) being null, tugun_tosh.glb is the only resource-node GLB that exists in either repo, and WORLD.qur is only ever called after MODELS.yukla has resolved (main.js:42-52 -> MENYU.tayyor -> GAME.yarat -> world.js qur at game.js:67), so the GLB set really is decided before the first draw. A dump that draws only x,z per node produces node 19 correctly and every node from 20 onward wrong. Three of the critic's specifics are wrong, and one of them is wrong by a lot.

(1) The per-type procedural draw counts are undercounted. Real counts, arguments evaluated left to right: tosh 6 draws x 3 rocks = 18 (critic right, path not taken); yogoch 4 x 4 = 16 (right); teri 1 (right); temir 7 x 5 = 35, NOT "~5 x 5" = 25 — A.quti's width, depth, x, y and z arguments are five separate draws before p.rotation.y and p.rotation.z; komir 4 x 6 = 24, NOT "2 x 6" = 12 — the dodecahedron radius plus all three components of c.position.set. Anyone who implemented the critic's numbers would get a stream that is wrong in a different way.

(2) "every node position after the 19th" is off by one. Node 19 is the first yogoch node and it uses draws #37 and #38 in both the idealised and the real stream, so node 19 is correct. The first wrong node is #20 (yogoch index 1): real (135.075675, -28.064980) vs the plan's idealised (131.074573, -19.213526) — 9.7 m apart. Nodes 1-19 match, 20-66 do not.

(3) "the LCG stream ... and every node position after the 19th depends on which GLBs loaded" overstates the coupling for the node table specifically. The tugun loop runs FIRST in qur(), before teraks, houses, the gate ring and the edge walls, so the 66 node positions depend only on which of the five tugun_*.glb files exist. terakYarat's GLB-vs-procedural split (2 draws vs 1) is real and does condition everything after draw #1080, but it cannot move a node. That matters because it makes the step-2 gate achievable without porting the gate ring or the edge wall.

Two things the critic missed entirely, both worse than what it found: the city-wall ring segment count is derived at runtime from the imported shahar_devori AABB (soni = max(8, round(2*PI / (sO.x/46)))), so a Godot importer that measures the mesh even slightly differently changes the segment count, which changes the buzilgan draw budget, which changes every draw after it; and the wall's collision annulus radii come from the same AABB (world.js:487, r1 = 46 - sO.z/2, r2 = 46 + sO.z/2), which is a gameplay-critical collider that world_buxoro.tres does not list at all.

The corrected spec below carries the real draw table, the real 66-node coordinates, the measured AABB numbers, and a defined cut point for the parity gate.

<details><summary>Evidence checked — 19 source reads</summary>

- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/world.js:10-12` : `let urugh = 1238;   function rnd() { urugh = (urugh * 1664525 + 1013904223) % 4294967296; return urugh / 4294967296; }   function orasida(a, b) { return a + rnd() * (b - a); }`
  One LCG instance per module load, seed 1238, never reseeded inside qur(). rnd is NOT on window.WORLD (world.js:661), so the stream is private to qur() and ends when qur() returns.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/world.js:96-101` : `Object.keys(B.TUGUN_SONI).forEach(tur => {       for (let i = 0; i < B.TUGUN_SONI[tur]; i++) {         const [x, z] = joylar[tur]();         dunyo.tugunlar.push(tugunYarat(scene, tur, x, z));       }     });`
  Per node: joylar[tur]() draws x then z (array literal, left to right), THEN tugunYarat draws 0..35 more. The node loop is the first rnd consumer in qur(), so nothing before it can shift node positions.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/balance.js:56` : `TUGUN_SONI: { tosh: 18, yogoch: 16, teri: 10, temir: 14, komir: 8 },`
  Object.keys order is insertion order for these non-numeric keys: tosh, yogoch, teri, temir, komir. 66 nodes total.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/world.js:167-169` : `let mesh = window.MODELS.ol('tugun_' + tur);     if (!mesh) {       mesh = new THREE.Group();`
  The entire procedural block, and every rnd() inside it, is skipped when the GLB loaded. This is the gate the whole determinism contract hangs on.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/world.js:192-194` : `const p = A.quti(0.10 + rnd() * 0.24, 0.07, 0.09 + rnd() * 0.14, m,             (rnd() - 0.5) * 0.7, 0.05 + rnd() * 0.12, (rnd() - 0.5) * 0.7);           p.rotation.y = rnd() * 3; p.rotation.z = rnd() * 0.5;`
  SEVEN draws per temir piece (w, d, x, y, z, rot.y, rot.z), x5 pieces = 35. The critic said ~5 x 5 = 25.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/world.js:199-200` : `const c = new THREE.Mesh(new THREE.DodecahedronGeometry(0.13 + rnd() * 0.10, 0), m);           c.position.set((rnd() - 0.5) * 0.75, 0.09 + rnd() * 0.14, (rnd() - 0.5) * 0.75);`
  FOUR draws per komir lump (radius, x, y, z), x6 = 24. The critic said 2 x 6 = 12.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/actors.js:45-50` : `function quti(w, h, d, m, x, y, z) {     const g = new THREE.Mesh(new THREE.BoxGeometry(w, h, d), m);     g.position.set(x || 0, y || 0, z || 0);`
  A.quti and A.silindr (actors.js:52) consume no randomness themselves; every draw is in the caller's argument list, evaluated left to right. So the draw ORDER is exactly the textual order of rnd() in world.js.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/world.js:221-231` : `const glb = window.MODELS.ol('terak');     if (glb) {       glb.position.set(x, yerBalandligi(x, z), z);       glb.rotation.y = rnd() * Math.PI * 2;       glb.scale.multiplyScalar(0.82 + rnd() * 0.40);`
  terak.glb exists, so 2 draws per poplar. The procedural fallback (world.js:231, h = 9 + rnd() * 3.5) is 1 draw. 53 poplars are created, so this single branch is worth 53 draws of shift.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/models.js:290-293` : `function ol(nom, opts) {     opts = opts || {};     const manba = keshi[nom];     if (!manba) return null;`
  ol() returns null exactly when keshi[nom] is null (loader error handler, models.js:170-172) or undefined (yukla not finished). Not a per-call state; the answer is fixed once loading completes.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/main.js:42-52` : `window.MODELS.yukla(nisbat => {       ...     }).then(() => {       const h = window.MODELS.hisobot();       ...       window.MENYU.tayyor(matn);     });`
  The menu only becomes ready after every GLB has resolved, and WORLD.qur is reached from GAME.yarat (game.js:67) only after the user starts from that menu. The GLB set is therefore fully known before the first draw — there is no load race.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/assets/models/ (ls)` : `ark_qalasi.glb askar.glb bozor_rastasi.glb devor_seksiya.glb hunarmand.glb mogul_piyoda.glb noyon.glb ot.glb qala_darvoza.glb shahar_devori.glb shomurod.glb tarobiy.glb terak.glb tugun_tosh.glb uy_katta.glb uy_kichik.glb uy_vayrona.glb`
  17 of the 35 names in models.js KUTILGAN exist. /Users/humoyunochilov/PROJECTS/ChronoShift/assets/models/buxoro/ holds the identical 17 basenames. tugun_yogoch, tugun_teri, tugun_temir, tugun_komir, ustaxona, paxsa_devor, shogird, mogul_otliq, humo and the four weapons are absent from both.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/world.js:255` : `if (!MUHIT.uylar) return;`
  ?uysiz=1 makes uylarQoy return before its 8 rotation draws (world.js:265), shifting every subsequent draw by 8. The port must reproduce MUHIT.uylar as a bool defaulting true. MUHIT.qishloqFon (world.js:18) is declared and never read anywhere in the codebase — dead.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/world.js:377-378` : `const segUzunlik = Math.max(4, sO.x);     const R = 46;                          // halqa radiusi`
  sO is the Box3 of the SCALED shahar_devori GLB. Measured from the file: raw AABB 0.9792 x 0.6864 x 0.6411, MOLJAL 8.5/0.6864 = 12.38342x applied (models.js:236 threshold |koef-1|>0.15 passes), giving sO = 12.1253 x 8.5000 x 7.9396. So segUzunlik = 12.1253 and soni = max(8, round(2*PI/(12.1253/46))) = 24. An asset-derived loop bound inside the determinism stream.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/world.js:487` : `tosiqQosh(dunyo, { x: markazX, z: z, r1: R - sO.z / 2, r2: R + sO.z / 2 });`
  The city-wall collision annulus is 42.0302 .. 49.9698 with the current asset (sO.z = 7.9396). This is the belt that ejects the player to r2 + r (world.js:620-624) and it is nowhere in world_buxoro.tres.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/world.js:412-419` : `for (let i = 0; i < soni; i++) {       const t = i * qadam;       // darvoza yoyiga tushsa — o'tkazamiz       let farq = Math.abs(((t - Math.PI + Math.PI) % (Math.PI * 2)) - Math.PI);       if (farq < darvozaBurchagi) continue;       // sharqiy ko'rinmas yoy       const sharqFarq = Math.abs(((t + Math.PI) % (Math.PI * 2)) - Math.PI);       if (sharqFarq < KORINMAS_YOY) continue;`
  With soni=24, qadam=0.2617994, darvozaBurchagi=(16.3299/46)*0.62=0.2200985 and KORINMAS_YOY=0.9424778, the kept indices are exactly 4,5,6,7,8,9,10,11,13,14,15,16,17,18,19,20 (16 segments). Index 3 is skipped by the east arc, so of buzilgan={3,4,11,17} only 4, 11 and 17 ever execute. world_buxoro.tres's Buzilgan=[3,4,11,17] is a verbatim-correct transcription of a set whose first member is dead code.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/world.js:544-549` : `for (let x = -4; x < 192; x += uz - 0.15) {         const s = window.MODELS.ol('paxsa_devor') || window.BINOLAR.paxsaDevor();         // uzun o'q X bo'lsa devor chizig'i ham X bo'ylab — burish shart emas         if (!uzunX) s.rotation.y = Math.PI / 2;         s.rotation.y += (rnd() - 0.5) * 0.03;         s.position.set(x, 0, z + (rnd() - 0.5) * 0.25);`
  paxsa_devor.glb is absent, so BINOLAR.paxsaDevor() runs — its UZ = 10 along Z with max X extent 1.3 (binolar.js:415, 435), so uzunX is false, uz = 10, step = 9.85, and the loop runs 20 times per line. 2 world draws per segment x 20 x 2 lines = 80 draws. The placeholder's own randomness comes from binolar.js's separate seed 8317 (binolar.js:16-18) and does not touch this stream.
- `/Users/humoyunochilov/PROJECTS/Torobiy/src/js/world.js:273-300` : `let g = window.MODELS.ol('ustaxona');     if (!g) {       g = new THREE.Group();       // bosqon (anvil) + kunda       g.add(A.silindr(0.42, 0.50, 0.62, A.mat(0x5c4a35, 1.0), 0, 0.31, 0, 9));`
  ustaxona.glb is absent so the procedural branch runs, but every literal in it is fixed — ustaxonaYarat consumes ZERO rnd draws on either branch. It is the one missing-GLB generator that is determinism-neutral.
- `simulation of world.js stream, seed 1238 (scratchpad/sim.js)` : `tosh: 18 nodes x (2 pos + 0 proc) = 36 draws / yogoch: 16 x (2 + 16) = 288 / teri: 10 x (2 + 1) = 30 / temir: 14 x (2 + 35) = 518 / komir: 8 x (2 + 24) = 208 / TUGUN TOTAL DRAWS = 1080`
  The node table consumes 1080 draws, not 132. Running the plan's idealised 132-draw replay and the real one side by side: node 19 agrees exactly (126.962996, -22.937801); node 20 is (135.075675, -28.064980) real vs (131.074573, -19.213526) idealised.
- `simulation of world.js stream, seed 1238 (scratchpad/sim.js)` : `row1 draws 48 row2 30 groups 65 group n values = 2,4,4,2,3 gate-road 36 / terak count total = 53 / draws after teraklar = 1259 / uylarQoy draws 8 cum 1267`
  The five poplar clusters draw n = 2,4,4,2,3 (15 trees), so 53 poplars total: 16 + 10 + 15 + 12. Cumulative draw checkpoints: 1080 after nodes, 1259 after poplars, 1267 after houses, 1474 after the gate ring (3 buzilgan x 45 + 12 tuynuk stones x 6 = 207), 1554 at the end of qur().

</details>

#### Corrected specification

##### Step 2 — corrected determinism contract (replaces the ParityDump paragraph in FILE 7 and the step-2 "step is done when" clause)

###### 2.0 The contract in one sentence
The world LCG stream is consumed by the PROCEDURAL PLACEHOLDER GEOMETRY inside `tugunYarat` and `terakYarat`, and which branch runs is decided by whether a GLB loaded. Placeholder box counts are load-bearing determinism, not cosmetics. Changing the number of rocks in a stone node, or shipping `tugun_yogoch.glb`, moves all 66 resource nodes and therefore the entire Act I economy.

###### 2.1 The GLB set the stream is conditioned on (FROZEN)
Both `/Users/humoyunochilov/PROJECTS/Torobiy/src/assets/models/` and `/Users/humoyunochilov/PROJECTS/ChronoShift/assets/models/buxoro/` contain exactly these 17 basenames and no others:

    ark_qalasi, askar, bozor_rastasi, devor_seksiya, hunarmand, mogul_piyoda,
    noyon, ot, qala_darvoza, shahar_devori, shomurod, tarobiy, terak,
    tugun_tosh, uy_katta, uy_kichik, uy_vayrona

models.js `KUTILGAN` lists 35 names; the 18 absent ones are: shogird, mogul_otliq, humo, kaltak, galvir_qalqon, kamon, qilich, devor_buzilgan, paxsa_devor, ustaxona, ot_tutam, ot_past, gul_sariq, butta, tugun_yogoch, tugun_teri, tugun_temir, tugun_komir.

Only four of these matter to the stream: **tugun_tosh present** (stone takes the GLB path, 0 extra draws), **tugun_yogoch / tugun_teri / tugun_temir / tugun_komir absent** (procedural path), **terak present** (2 draws per poplar). `ustaxona` and `paxsa_devor` are absent but their procedural generators consume zero world draws.

`Models.Ol(name)` in the port MUST return null for a name with no imported scene, exactly as models.js:290-293 does, and `TugunYarat` / `TerakYarat` MUST branch on that null. Do not hardcode "stone is GLB" — hardcode the null test, so that adding `tugun_yogoch.glb` later fails the parity checklist loudly instead of silently re-rolling the field.

Add to BUXORO_PARITY.md as a positive assertion: *"Exactly one tugun_*.glb exists (tugun_tosh). terak.glb exists. If either changes, all 66 node positions change and every number in balance.js must be re-tuned."*

###### 2.2 Exact draw table for `TugunYarat` (world.js:166-205)
Arguments are evaluated left to right; `A.quti` / `A.silindr` consume nothing themselves (actors.js:45-58), so the draw order is the textual order of `rnd()` in the source.

| tur | branch taken today | draws per node |
|---|---|---|
| tosh | **GLB** (tugun_tosh.glb) | 2 (x, z) + 0 = **2** |
| yogoch | procedural | 2 + 16 = **18** |
| teri | procedural | 2 + 1 = **3** |
| temir | procedural | 2 + 35 = **37** |
| komir | procedural | 2 + 24 = **26** |

Per-branch draw order, to be reproduced literally:

- **tosh procedural (NOT taken today; port it anyway, behind the null test)** — 3 iterations x 6 draws = 18:
  `r`, `pos.x`, `pos.z`, `rot.x`, `rot.y`, `rot.z`
- **yogoch** — 4 iterations x 4 draws = 16:
  `x-offset`, `z-offset`, `rot.z`, `rot.y`
- **teri** — 1 draw total: `rot.y` of the first hide. The second `A.quti` is all literals.
- **temir** — 5 iterations x 7 draws = 35:
  `width`, `depth`, `pos.x`, `pos.y`, `pos.z`, `rot.y`, `rot.z`
  (The critic's "~5 draws x 5 pieces" is wrong: width and depth are two draws inside the `A.quti` argument list, before the three position draws.)
- **komir** (the `else` branch) — 6 iterations x 4 draws = 24:
  `radius`, `pos.x`, `pos.y`, `pos.z`
  (The critic's "2 draws x 6" is wrong by a factor of two.)

Node loop total: 18x2 + 16x18 + 10x3 + 14x37 + 8x26 = **1080 draws**, not 132.

###### 2.3 Exact draw table for `TerakYarat` (world.js:219-242) and the poplar call sites
- GLB path (taken today): **2** draws — `rotation.y = rnd()*2PI`, then `scale *= 0.82 + rnd()*0.40`.
- Procedural path: **1** draw — `h = 9 + rnd()*3.5`.

Call sites, in order (world.js:107-127):
1. Main row, 16 iterations: 1 draw for `x = 129 + rnd()*4`, then TerakYarat. **48 draws.**
2. Second row, 10 iterations: 1 draw for `x = 137 + rnd()*3`, then TerakYarat. **30 draws.**
3. Five clusters at (112,-46), (104,40), (120,34), (96,-44), (88,42): per cluster 1 draw for `n = 2 + floor(rnd()*3)`, then n x (x-jitter, z-jitter, TerakYarat). With seed 1238 the drawn n values are **2, 4, 4, 2, 3** (15 trees). **65 draws.**
4. Gate road, 6 iterations x 2 trees: each tree is 1 draw for z-jitter + TerakYarat. **36 draws.**

Poplar total **179 draws**, **53 trees**. `TerakYol=12` and `TerakKlaster=5` in world_buxoro.tres are correct; the cluster sizes are drawn, not stored.

###### 2.4 Full ordered consumer list for `qur()`, with cumulative draw index
| # | consumer | world.js | draws | cumulative |
|---|---|---|---|---|
| 1 | Resource nodes, tosh→yogoch→teri→temir→komir | 96-101 + 166-205 | 1080 | 1080 |
| 2 | Poplars (4 call-site groups above) | 107-127 + 219-242 | 179 | 1259 |
| 3 | `uylarQoy` — 8 houses, 1 yaw draw each | 247-271 | 8 | 1267 |
| 4 | `ustaxonaYarat` | 273-310 | 0 | 1267 |
| 5 | `bozorYarat` | 312-347 | 0 | 1267 |
| 6 | `darvozaYarat` — 3 ruined segments x 45 + 12 rubble stones x 6 | 357-491 | 207 | 1474 |
| 7 | `arkYarat` | 493-531 | 0 | 1474 |
| 8 | `OTLAR.qur` — **separate LCG, seed 4711** | otlar.js:13 | 0 | 1474 |
| 9 | `chekkalar` — 2 lines x 20 segments x 2 draws | 533-556 | 80 | 1554 |

`rnd` is not exported on `window.WORLD` (world.js:661), so the stream ends at 1554 and nothing after `qur()` can touch it.

`binolar.js` (seed 8317) and `otlar.js` (seed 4711) are independent streams. `BINOLAR.paxsaDevor()` is called 42 times inside `chekkalar` and consumes the 8317 stream; it must never be routed through the 1238 LCG.

###### 2.5 The ruined-wall sub-budget (world.js:426-469)
Per `buzilgan` segment = **45 draws**: `k` (scale), `rot.z`, `rot.x`, then 7 rubble stones x 6 draws each (`r`, `pos.x`, `pos.z`, `rot.x`, `rot.y`, `rot.z`).
The `tuynuk` rubble heap = 12 stones x 6 = **72 draws**, and it runs from the standalone `tuynuk.forEach` (world.js:455-469) whether or not segment 12 was reachable.

With the current assets only **3** of the four `buzilgan` indices execute (see 2.6), so 3x45 + 72 = 207.

###### 2.6 Asset-derived loop bounds — the second, unreported hazard
`darvozaYarat` measures the SCALED GLB bounding boxes and derives loop bounds from them:

    segUzunlik = max(4, sO.x)               // sO = Box3 of scaled shahar_devori
    soni       = max(8, round(2PI / (segUzunlik / 46)))
    qadam      = 2PI / soni
    darvozaBurchagi = (dO.x / 46) * 0.62    // dO = Box3 of scaled qala_darvoza

Measured from the actual .glb files, with models.js MOLJAL scaling applied (models.js:226-240):
- `shahar_devori`: raw AABB 0.9792 x 0.6864 x 0.6411, koef = 8.5/0.6864 = **12.38342**, scaled **sO = 12.1253 x 8.5000 x 7.9396**
- `qala_darvoza`: raw AABB 0.9798 x 0.6600 x 0.7919, koef = 11.0/0.6600 = **16.66657**, scaled **dO = 16.3299 x 11.0000 x 13.1988**
- `terak`: raw 0.1520 x 0.9776 x 0.1491, koef = 11.0/0.9776 = **11.25204**, scaled 1.7101 x 11.0 x 1.6774

Therefore: **segUzunlik = 12.1253, soni = 24, qadam = 0.26179939, darvozaBurchagi = 0.22009850, KORINMAS_YOY = 0.30*PI = 0.94247780.**

Kept segment indices are exactly **4,5,6,7,8,9,10,11,13,14,15,16,17,18,19,20** (16 segments). Index 3 is eliminated by the east invisible arc *before* the `buzilgan` test, so **`Buzilgan=[3,4,11,17]` reaches only 4, 11 and 17** — index 3 is dead. Keep the array verbatim (it is the source), and add the comment `// index 3 is skipped by KorinmasYoy before it is tested — dead, kept verbatim`.

**These four derived numbers (soni=24, qadam, darvozaBurchagi, segUzunlik) MUST be stored in world_buxoro.tres as constants, not recomputed from Godot's imported AABB.** Godot's glTF importer and Three.js `Box3.setFromObject` need not agree to the last bit, and a difference of 0.01 m in `sO.x` flips `round(23.835)` only at 12.44 m — but the derived `darvozaBurchagi` feeds a `<` comparison and `soni` feeds the loop bound, so a divergence silently changes the ruined-segment count and shifts every draw after index 1267. Add: `DevorSegUzunlik=12.1253, DevorSoni=24, DarvozaBurchagi=0.2200985` with the comment `// measured from shahar_devori.glb/qala_darvoza.glb AABB after MOLJAL scaling; frozen so Godot's importer cannot re-derive a different field`. `World.cs` asserts the imported AABB is within 0.05 m of these and `PushError`s otherwise.

###### 2.7 Missing gameplay value the AABB also produces
world.js:487 builds the city-wall collision annulus as `{ x: markazX, z, r1: R - sO.z/2, r2: R + sO.z/2 }` = **r1 = 42.0302, r2 = 49.9698**. This is the belt that ejects the player outward to `r2 + r` (world.js:615-625) and it is the single largest collider in the level. It is absent from `sehrli_buxoro.tres` and `world_buxoro.tres`. Add `ShaharDevoriR1=42.0302, ShaharDevoriR2=49.9698` alongside the existing `ShaharDevoriR=46`, sourced `// world.js:487, sO.z = 7.9396 measured from shahar_devori.glb`.

###### 2.8 The `?uysiz=1` flag
`MUHIT.uylar` (world.js:17, 255) gates `uylarQoy`'s 8 yaw draws. Port it as `BuxoroWorld.Uylar` (bool, default **true**). Setting it false removes all 8 houses AND shifts draws 1268..1554. `MUHIT.qishloqFon` (world.js:18) is declared and read nowhere in the entire codebase — port it as a commented dead field or omit it; do not invent behaviour for it.

###### 2.9 Rewritten ParityDump spec (replaces FILE 7)
`ParityDump.cs` writes `user://parity_godot.csv` in five blocks. `tools/parity/dump_web.js` emits byte-identical blocks from an unmodified `world.js` under `?parity=1`, by shadowing the module's `rnd` with a counting wrapper — it must NOT reimplement world.js.

- **BLOCK A — RAW**: the first 200 raw `Rnd()` values from a fresh `Lcg(1238)`, one per line, `index,value` at 17 significant digits ("R" format). Byte-identical is the bar.
- **BLOCK B — NODES**: replay `Qur()`'s node loop exactly as specified in 2.2, emitting `idx,tur,i,x,z,firstDrawIndex` for all 66 nodes, x/z to 6 decimals. Only the node loop runs; nothing before it consumes draws, so this block is self-contained.
- **BLOCK C — DRAWCOUNTS**: the cumulative draw index after each consumer in table 2.4: `nodes,1080 / poplars,1259 / houses,1267 / gate,1474 / edges,1554`, plus the five cluster sizes `2,4,4,2,3`.
- **BLOCK D — TERRAIN**: `YerBalandligi` at (96,6), (108,1), (152,-22), (148,20), (60,0), (172,0), 6 decimals.
- **BLOCK E — RESOURCES**: every `[Export]` of `BuxoroBalance` and `BuxoroSehrli` by reflection in declaration order.

The 66 node positions the port MUST reproduce (seed 1238, the 17-GLB set above), `idx,tur,i,x,z`:

    1,tosh,0,92.427209,5.865567        34,yogoch,15,138.933420,-37.679532
    2,tosh,1,71.953742,-0.493257       35,teri,0,135.359269,10.162522
    3,tosh,2,70.237404,-17.819681      36,teri,1,134.040234,33.985564
    4,tosh,3,115.081470,28.341793      37,teri,2,136.559647,16.735443
    5,tosh,4,54.740194,17.054735       38,teri,3,140.005352,10.354370
    6,tosh,5,73.817033,2.942667        39,teri,4,137.497985,27.383042
    7,tosh,6,115.123720,-22.232039     40,teri,5,138.739994,29.074444
    8,tosh,7,88.766055,3.413623        41,teri,6,133.270405,13.306815
    9,tosh,8,56.138292,-31.108485      42,teri,7,132.137891,22.259889
    10,tosh,9,30.814654,-1.745561      43,teri,8,135.912957,22.194003
    11,tosh,10,36.148762,-14.800310    44,teri,9,139.635250,18.786763
    12,tosh,11,41.354663,-40.031916    45,temir,0,98.637014,-21.015357
    13,tosh,12,59.571655,-22.884889    46,temir,1,96.160085,-9.001795
    14,tosh,13,113.534240,-20.890210   47,temir,2,89.832815,-16.381232
    15,tosh,14,32.603320,-11.248056    48,temir,3,121.266613,-24.671655
    16,tosh,15,99.690118,-36.659976    49,temir,4,84.588046,-21.617459
    17,tosh,16,113.645344,-34.380731   50,temir,5,99.065831,-33.493357
    18,tosh,17,112.617748,41.043884    51,temir,6,102.579544,-27.191868
    19,yogoch,0,126.962996,-22.937801  52,temir,7,106.534204,-24.441736
    20,yogoch,1,135.075675,-28.064980  53,temir,8,96.056861,-27.461960
    21,yogoch,2,136.161166,-18.756223  54,temir,9,99.468228,-23.253526
    22,yogoch,3,133.457745,-33.008458  55,temir,10,98.391733,-24.105266
    23,yogoch,4,137.704647,-40.111251  56,temir,11,99.288754,-9.442915
    24,yogoch,5,138.919599,-43.881869  57,temir,12,119.412332,-12.840234
    25,yogoch,6,132.615136,-38.775545  58,temir,13,96.105277,-33.773659
    26,yogoch,7,136.032315,-38.217405  59,komir,0,107.206288,14.574348
    27,yogoch,8,145.458715,-41.750516  60,komir,1,101.710262,15.171527
    28,yogoch,9,144.467149,-23.389475  61,komir,2,113.894897,37.484979
    29,yogoch,10,136.212309,-32.785735 62,komir,3,122.638539,14.822053
    30,yogoch,11,132.004561,-35.324541 63,komir,4,111.248432,32.097452
    31,yogoch,12,144.493425,-27.958893 64,komir,5,121.608653,16.130540
    32,yogoch,13,138.187784,-22.318384 65,komir,6,107.245963,29.828905
    33,yogoch,14,143.965663,-18.285049 66,komir,7,98.531825,25.410781

Sanity check against the bug this blocker is about: node 19 is the value the plan's idealised harness would also produce, and node 20 is where it breaks — idealised gives (131.074573, -19.213526), 9.7 m from the true (135.075675, -28.064980). **The first wrong node is 20, not 19.**

###### 2.10 Revised step-2 gate (replaces the "step is done when" clause)
The step is done when ALL of:
1. Block A is byte-identical between `parity_godot.csv` and `parity_web.csv` on all 200 draws.
2. Block B agrees on all 66 nodes to 6 decimals, **and** `firstDrawIndex` matches on every row (this is what catches a wrong placeholder box count; the coordinates alone can coincide).
3. Block C matches exactly: 1080 / 1259 / 1267 / 1474 / 1554 and cluster sizes 2,4,4,2,3.
4. Block D agrees to 6 decimals.
5. The user has read all ~110 constants beside balance.js.

Blocks C items 4 and 5 (`gate`, `edges`) may be deferred to step 3 **only** if `Ustaxona`, `Bozor`, `DarvozaYarat` and `Chekkalar` are not yet written; in that case the gate is blocks A, B, D plus the first three checkpoints of C (1080 / 1259 / 1267), and BUXORO_PARITY.md carries an open row "gate ring + edge wall draw budget unverified". Nothing else about the stop-rule changes: **do not write World.Qur's poplar/house/ring code, do not write Player.cs, until blocks A, B and D pass.**

###### 2.11 Mapping-table edits
Add to the **"Resource nodes"** row (plan line 271): *"TugunYarat MUST port all five placeholder branches with rnd() consumed in the exact order and count of world.js:171-204 (tosh 6x3, yogoch 4x4, teri 1, temir 7x5, komir 4x6), gated on Models.Ol('tugun_'+tur) == null. Today only tugun_tosh.glb exists, so stone costs 2 draws and the other four cost 18/3/37/26. This is the determinism contract: one extra box moves all 66 nodes."*

Add to the **"Procedural placeholder actors (actors.js)"** row (plan line 302): *"The placeholder geometry inside world.js's TugunYarat and TerakYarat is DETERMINISM, not scaffolding. Actors.Quti and Actors.Silindr must consume no randomness of their own — every draw stays in the caller's argument list, left to right, so the draw order is the textual order of rnd() in the source."*

Add to the **"Procedural architecture (binolar.js)"** row (plan line 303): *"soni=24 / qadam / darvozaBurchagi are derived from the scaled shahar_devori and qala_darvoza AABBs in world.js:373-400; they are frozen as constants in world_buxoro.tres (12.1253 / 24 / 0.2200985) rather than re-measured, because Godot's importer need not reproduce Three.js Box3 to the bit. Of Buzilgan=[3,4,11,17], index 3 is unreachable — KorinmasYoy skips it first."*

Add to **world_buxoro.tres** (plan line 322): `Uylar=true (world.js:17, ?uysiz=1 disables and shifts 8 LCG draws)`, `UyJoylari=[uy_katta(140,-40), uy_kichik(150,-30), uy_kichik(134,-46), uy_katta(146,36), uy_kichik(156,26), uy_vayrona(98,-30), uy_vayrona(108,-22), uy_vayrona(116,-34)]`, `DevorSegUzunlik=12.1253, DevorSoni=24, DarvozaBurchagi=0.2200985`, `ShaharDevoriR1=42.0302, ShaharDevoriR2=49.9698`, `ChekkaUz=10.0, ChekkaQadam=9.85, ChekkaX0=-4, ChekkaXMax=192 (20 segments per line)`.

Blocker #6's fix (the UyJoylari array) is subsumed here and is consistent with it: houses are consumer #3 at cumulative draw 1267.

#### Code

// scripts/buxoro/world/World.cs — the two determinism-critical branches.
// Draw order is the TEXTUAL order of rnd() in world.js. C# evaluates method
// arguments left to right (ECMA-334 §12.6.2), matching JS, so the argument-list
// draws below are legal to write inline — but they are hoisted into named locals
// anyway so a future reader cannot reorder them by accident.

private Node3D TugunYarat(string tur, float x, float z)
{
    // world.js:167 — the null test IS the determinism gate. Do not replace it
    // with a hardcoded "tosh is GLB": if tugun_yogoch.glb is ever added, this
    // must re-roll the field loudly, not silently.
    Node3D mesh = _models.Ol("tugun_" + tur);
    if (mesh == null)
    {
        mesh = new Node3D();
        var m = Actors.Mat(BuxoroWorld.ResursRangi[tur], 1.0f);
        switch (tur)
        {
            case "tosh":            // world.js:171-179 — 3 x 6 = 18 draws
                for (int i = 0; i < 3; i++)
                {
                    float r  = 0.26f + _lcg.Rnd() * 0.20f;   // 1
                    float px = (_lcg.Rnd() - 0.5f) * 0.8f;   // 2
                    float pz = (_lcg.Rnd() - 0.5f) * 0.8f;   // 3
                    float rx = _lcg.Rnd() * 3f;              // 4
                    float ry = _lcg.Rnd() * 3f;              // 5
                    float rz = _lcg.Rnd() * 3f;              // 6
                    mesh.AddChild(Actors.Dodekaedr(r, m, px, r * 0.7f, pz, rx, ry, rz));
                }
                break;

            case "yogoch":          // world.js:180-185 — 4 x 4 = 16 draws
                for (int i = 0; i < 4; i++)
                {
                    float ox = (_lcg.Rnd() - 0.5f) * 0.3f;   // 1
                    float oz = (_lcg.Rnd() - 0.5f) * 0.3f;   // 2
                    float rz = 0.35f + _lcg.Rnd() * 0.3f;    // 3
                    float ry = _lcg.Rnd() * 3f;              // 4
                    var b = Actors.Silindr(0.07f, 0.08f, 1.5f, m, ox, 0.30f, oz, 6);
                    b.RotateObjectLocal(Vector3.Back, rz);
                    b.RotateObjectLocal(Vector3.Up, ry);
                    mesh.AddChild(b);
                }
                break;

            case "teri":            // world.js:186-189 — 1 draw
            {
                float ry = _lcg.Rnd() * 3f;                  // 1
                var t = Actors.Quti(0.78f, 0.14f, 0.62f, m, 0f, 0.09f, 0f);
                t.Rotation = new Vector3(0f, ry, 0f);
                mesh.AddChild(t);
                mesh.AddChild(Actors.Quti(0.62f, 0.10f, 0.50f, m, 0.06f, 0.22f, 0.05f));
                break;
            }

            case "temir":           // world.js:190-196 — 5 x 7 = 35 draws
                for (int i = 0; i < 5; i++)
                {
                    float w  = 0.10f + _lcg.Rnd() * 0.24f;   // 1
                    float d  = 0.09f + _lcg.Rnd() * 0.14f;   // 2
                    float px = (_lcg.Rnd() - 0.5f) * 0.7f;   // 3
                    float py = 0.05f + _lcg.Rnd() * 0.12f;   // 4
                    float pz = (_lcg.Rnd() - 0.5f) * 0.7f;   // 5
                    float ry = _lcg.Rnd() * 3f;              // 6
                    float rz = _lcg.Rnd() * 0.5f;            // 7
                    var p = Actors.Quti(w, 0.07f, d, m, px, py, pz);
                    p.Rotation = new Vector3(0f, ry, rz);
                    mesh.AddChild(p);
                }
                break;

            default:                // "komir" — world.js:197-204 — 6 x 4 = 24 draws
                for (int i = 0; i < 6; i++)
                {
                    float r  = 0.13f + _lcg.Rnd() * 0.10f;   // 1
                    float px = (_lcg.Rnd() - 0.5f) * 0.75f;  // 2
                    float py = 0.09f + _lcg.Rnd() * 0.14f;   // 3
                    float pz = (_lcg.Rnd() - 0.5f) * 0.75f;  // 4
                    mesh.AddChild(Actors.Dodekaedr(r, m, px, py, pz, 0f, 0f, 0f));
                }
                break;
        }
    }
    mesh.Position = new Vector3(x, BuxoroTerrain.YerBalandligi(x, z), z);
    _tugunlar3d.AddChild(mesh);
    return mesh;
}

private Node3D TerakYarat(float x, float z)
{
    // world.js:220-229 — GLB path is TWO draws; procedural path is ONE.
    // 53 poplars are created, so this branch is worth 53 draws of shift.
    Node3D glb = _models.Ol("terak");
    if (glb != null)
    {
        glb.Position = new Vector3(x, BuxoroTerrain.YerBalandligi(x, z), z);
        float ry = _lcg.Rnd() * Mathf.Pi * 2f;               // 1
        float sc = 0.82f + _lcg.Rnd() * 0.40f;               // 2
        glb.Rotation = new Vector3(0f, ry, 0f);
        glb.Scale *= sc;                                     // MULTIPLY, never assign
        _teraklar3d.AddChild(glb);
        return glb;
    }
    float h = 9f + _lcg.Rnd() * 3.5f;                        // 1 — fallback path
    var g = Binolar.TerakProtsedural(h);
    g.Position = new Vector3(x, BuxoroTerrain.YerBalandligi(x, z), z);
    _teraklar3d.AddChild(g);
    return g;
}

#### Test that proves it

**T1 — Node table + draw index (the gate itself).** `ParityDump` block B must emit 66 rows and match, to 6 decimals on x/z and exactly on `firstDrawIndex`:

    1,tosh,0,92.427209,5.865567,1
    18,tosh,17,112.617748,41.043884,35
    19,yogoch,0,126.962996,-22.937801,37
    20,yogoch,1,135.075675,-28.064980,55       <- the bug's first casualty
    34,yogoch,15,138.933420,-37.679532,307
    35,teri,0,135.359269,10.162522,325
    44,teri,9,139.635250,18.786763,352
    45,temir,0,98.637014,-21.015357,355
    58,temir,13,96.105277,-33.773659,836
    59,komir,0,107.206288,14.574348,873
    66,komir,7,98.531825,25.410781,1055

`firstDrawIndex` is the load-bearing column: the coordinates alone can coincide, but the draw index cannot. A harness implementing the plan as written emits node 20 as (131.074573, -19.213526) at draw 39 — both columns fail, which is the point.

**T2 — Draw budget checkpoints (block C).** After `Qur()` completes with `Uylar = true` and the 17-GLB set, the cumulative draw counter must read exactly:

    after nodes    1080
    after poplars  1259   (delta 179; cluster sizes drawn = 2,4,4,2,3; 53 poplars)
    after houses   1267   (delta 8)
    after gate     1474   (delta 207 = 3 ruined segments x 45 + 12 rubble stones x 6)
    after edges    1554   (delta 80 = 2 lines x 20 segments x 2)

**T3 — Branch-flip regression (a unit test, not a diff).** Run the node loop twice against a stubbed `Models.Ol`: once with `{tugun_tosh}` present, once with `{tugun_tosh, tugun_yogoch}` present. Assert the first run's node 20 is `(135.075675, -28.064980)`, the second run's is NOT, and that the second run's total is 1080 - 16*16 = 824 draws. This is the test that fails the day someone adds a GLB and re-rolls the whole economy.

**T4 — Wall-ring derivation.** Assert `Mathf.Abs(importedShaharDevoriAabb.Size.X - 12.1253f) < 0.05f` and `Mathf.Abs(importedQalaDarvozaAabb.Size.X - 16.3299f) < 0.05f` after MOLJAL scaling; assert the kept segment indices are exactly `{4,5,6,7,8,9,10,11,13,14,15,16,17,18,19,20}` (16 of 24) and that the executed `buzilgan` set is `{4,11,17}` (3, not 4). Assert `ShaharDevoriR1 == 42.0302f` and `ShaharDevoriR2 == 49.9698f`.

**T5 — Stream isolation.** Assert `Binolar`'s LCG (seed 8317) and `Otlar`'s (4711) are distinct instances from the world LCG (1238), and that a full `Qur()` with `ustaxona`/`paxsa_devor` absent leaves the world counter at 1554 — i.e. the 42 `Binolar.PaxsaDevor()` calls inside `Chekkalar` moved the 8317 stream and not the 1238 one.

#### What the user does in the Godot editor

Nothing in the Godot editor is required for this fix beyond what step 2 already asks. Three additions to the existing step-2 editor checklist:

1. **Do not import any further GLB into `res://assets/models/buxoro/`** before the step-2 gate passes. Specifically: do not add `tugun_yogoch.glb`, `tugun_teri.glb`, `tugun_temir.glb`, `tugun_komir.glb`, or remove/replace `tugun_tosh.glb` or `terak.glb`. The LCG stream, and therefore all 66 node positions and the whole Act I economy, is conditioned on exactly the 17 files present today. If one of these five is ever added later, the parity CSV must be regenerated and every number in balance.js re-validated — it is not a drop-in.

2. **Do not change the import scale of `shahar_devori.glb` or `qala_darvoza.glb`** (Import dock → Advanced → root scale) before the gate passes. Their post-MOLJAL bounding boxes (12.1253 and 16.3299 on X) determine the wall-ring segment count and the ruined-segment draw budget. If the importer's measured AABB differs from those numbers by more than 0.05 m, `World.cs` will `PushError` at startup — report that number rather than "fixing" the constant.

3. When running the two halves of the parity diff: open the web build with `?parity=1` and **no other query parameters** — in particular not `?uysiz=1`, which removes eight LCG draws and shifts the second half of the CSV while leaving all 66 node rows correct, which is the most confusing possible failure mode.

Everything else for this fix is C# and .tres text that Claude writes.

#### Sections of this plan that this correction overrides

- Step 2 (line 92-100) — the whole 'Deliverable' clause for FILE 7 (ParityDump.cs + tools/parity/dump_web.js), line 573
- Step 2 'the step is done when' stop-rule, line 577 — replace with the five-part gate in §2.10
- Step 2 'User does (Godot editor)', line 98 — add the Autoload registration caveat and the ParityDump scratch-scene note
- Step 3 (line 102-110) — World.cs deliverable: TugunYarat/TerakYarat must carry all five placeholder branches, not just the GLB path
- Mapping row 'Resource nodes — 5 types, deterministic zones, finite stock (world.js:85-101, 166-218)', line 271
- Mapping row 'Procedural placeholder actors (actors.js:1-231)', line 302
- Mapping row 'Procedural architecture — 7 generators + battered-wall geometry (binolar.js:1-465; world.js:246-491)', line 303
- Resource row 'res://resources/data/buxoro/world_buxoro.tres', line 322 — add Uylar, UyJoylari, DevorSegUzunlik/DevorSoni/DarvozaBurchagi, ShaharDevoriR1/R2, ChekkaUz/ChekkaQadam
- Resource row 'res://resources/data/buxoro/sehrli_buxoro.tres', line 321 — ShaharDevoriRadius=46.0 is incomplete; the annulus r1/r2 are the actual colliders
- Resource row 'res://resources/data/buxoro/tugun_{tosh,yogoch,teri,temir,komir}.tres', line 323 — the note 'The LCG consumption order tosh→yogoch→teri→temir→komir must match world.js or all 66 node positions shift' is true but insufficient; add the per-type draw counts
- Blocker #6 and its fix (lines 419-421) — subsumed by §2.4/§2.11; houses are consumer #3 at cumulative draw 1267
- Blocker #3 and its fix (lines 407-409, the missing Ustaxona generator) — annotate that ustaxonaYarat is determinism-neutral (0 draws on both branches), so adding it cannot shift the stream
- BUXORO_PARITY.md — new positive assertions for the frozen GLB set and for the placeholder box counts

#### Further findings the critics missed — 10

- The critic's per-type draw counts are wrong for two of the five types. temir is 7 draws x 5 pieces = 35, not '~5 x 5' = 25 (A.quti's width and depth arguments are draws too, world.js:192). komir is 4 draws x 6 lumps = 24, not '2 x 6' = 12 (the dodecahedron radius plus all three components of position.set, world.js:199-200). Implementing the critic's numbers verbatim produces a stream that is wrong in a new way — off by 10 draws per iron node and 12 per coal node, i.e. 236 draws over the node loop.
- The first divergent node is #20, not #19. Node 19 (yogoch index 0) reads draws #37 and #38 in both the idealised and the real stream and is therefore correct in both. Real node 20 = (135.075675, -28.064980) at draw #55; idealised node 20 = (131.074573, -19.213526) at draw #39.
- The node table is insulated from terakYarat/uylarQoy/darvozaYarat, because the tugun loop is the FIRST rnd consumer in qur(). This makes the step-2 gate achievable with a smaller port than the critic implies: blocks A, B and D only need Lcg, BuxoroTerrain and TugunYarat's five branches. Worth saying explicitly so the stop-rule does not accidentally block on the gate ring.
- UNREPORTED, worse than the reported blocker: the city-wall segment count is derived at runtime from the imported mesh AABB (world.js:377-399, soni = max(8, round(2PI/(sO.x/46)))). With the shipped shahar_devori.glb this is 24, but the value comes from Three.js Box3 after models.js MOLJAL scaling and Godot's glTF importer is not guaranteed to reproduce it. If soni shifts by one, the ruined-segment set changes and every draw after index 1267 shifts. Freeze soni=24, segUzunlik=12.1253 and darvozaBurchagi=0.2200985 in world_buxoro.tres with an assert against the imported AABB.
- UNREPORTED: of Buzilgan=[3,4,11,17], index 3 is never reached — the east KORINMAS_YOY test (world.js:418-419) skips i=0..3 before the buzilgan branch is evaluated. Only 4, 11 and 17 execute, so the ruined-wall budget is 3x45 + 72 = 207 draws, not 4x45 + 72 = 252. A port that reaches index 3 because its soni differs will consume 45 extra draws and shift everything after.
- UNREPORTED: the city-wall COLLISION annulus radii are AABB-derived and absent from every .tres in the plan. world.js:487 uses r1 = 46 - sO.z/2 = 42.0302 and r2 = 46 + sO.z/2 = 49.9698 (sO.z = 7.9396 measured). This is the belt the player is ejected from (world.js:615-625) and the plan stores only ShaharDevoriR=46.
- The ?uysiz=1 URL flag (MUHIT.uylar, world.js:17 and 255) removes uylarQoy's 8 yaw draws along with the houses, shifting draws 1268..1554. It must be a named bool defaulting true, in the same family as the plan's other reproduce-then-offer flags. Its sibling MUHIT.qishloqFon (world.js:18) is declared and read nowhere in the entire src/js tree — dead, do not invent behaviour for it.
- ustaxonaYarat (world.js:273-310) is the one missing-GLB generator that consumes ZERO world draws on either branch — every literal is fixed. This is relevant to blocker #3: adding Binolar.Ustaxona() cannot shift the stream, so that fix is determinism-safe and can land any time.
- Chekkalar (world.js:533-556) consumes 80 world draws (2 per segment, 20 segments per line, 2 lines) and the segment length uz=10 comes from BINOLAR.paxsaDevor()'s own geometry (UZ=10 along Z, max X extent 1.3, so uzunX is false and every segment is rotated PI/2). Crucially, paxsaDevor's internal randomness comes from binolar.js's separate seed 8317, not the world stream — routing it through the world LCG would add 42 x N draws and destroy the field.
- The five poplar cluster sizes are DRAWN, not stored: n = 2 + floor(rnd()*3) gives 2,4,4,2,3 with seed 1238, for 53 poplars total (16 + 10 + 15 + 12). The plan's world_buxoro.tres correctly stores TerakKlaster=5 and TerakYol=12, but a reader could mistake the cluster count for the tree count; the row should say '5 clusters, sizes drawn: 2,4,4,2,3'.

---

---

## Chosen architecture

| Approach | Score |
|---|---|
| `design:mirror` **WINNER** | 91 |
| `design:godot-native` | 87 |
| `design:data-first` | 83 |

The mirror plan wins, and it wins on the criterion weighted heaviest: it is the only one of the three whose fidelity can be checked mechanically rather than argued about. If World.cs has TugunYarat, TerakYarat, ShaharDevori, BinoTosiq and ToqnashuvHal in the same order as world.js, a reviewer verifies the port in an afternoon without launching it — and that property is worth more than the elegance the other two buy by dissolving the module boundaries. But mirroring is right only for the simulation. Mirroring hud.js into a C# class, or chizma.js's scissor/autoClear/clearDepth dance, or videolar.js's muted-autoplay retry chain preserves a shape that no longer has a job, so the final plan keeps the mirror for world/player/apprentices/combat/game/humo/anim/actors/binolar/otlar/qurollar/touch/main and goes deliberately Godot-native for the HUD (CanvasLayer of Controls), the blueprint viewer (SubViewport), video (VideoStreamPlayer), grass (MultiMesh), translation (TranslationServer) and ally bars (UnprojectPosition). Underneath both layers sits data-first's discipline: two .tres files hold every number, one transcribing balance.js verbatim and one collecting the ~40 values hardcoded elsewhere despite the README's promise, each with a file:line citation, read through a single autoload so no episode C# file can hold a gameplay literal. Three grafts are load-bearing rather than decorative: godot-native's four written REFUSALS (no move_and_slide world collision, no SpringArm3D, no NavigationAgent3D, no TimeManager), each naming the exact number it would change and each surviving as a permanent doc so a future reader cannot helpfully undo them; its max_physics_steps_per_frame = 3, which turns the web's 1/20 s dt clamp from dead insurance into a live slow-motion floor; and data-first's five proof mechanisms — the golden apprentice trace diffed to 0.01 m, the 20,000-point solver lattice diffed to 1e-4, the RMS-and-spectral-centroid audio oracle, the four-camera screenshot diff, and the camera-offset unit test — which convert the five systems that genuinely resist being data from judgement calls into diffs. I rejected data-first's three deliberate deviations: the E-hold accumulation, the touch intro-skip and live locale retranslation are reproduced behind named booleans, because the mandate is a literal transfer and the correct default is reproduce-then-offer, not fix-then-ask. One genuine divergence from both runners-up: they each routed the episode through MissionMap and were each forced to invent a fake EngineeringTask, because MissionMap warns and can never finish with zero tasks — this plan refuses the framework outright and ships Buxoro as a standalone scene reached from the catalog, which costs one menu row and removes a fake objective from a game whose win condition is a kill counter. The step order is rebuilt so the user sees a correctly-scaled Shomurod on day one, a walkable deterministic field by step 3 and real 5.0 m/s movement by step 4, because the realistic failure mode of a 6,210-line literal port is not any single system — it is fidelity checks quietly getting skipped when there is nothing on screen to check them against.

### `design:mirror` — 91

**Strengths.** Highest fidelity and the cheapest proof of it: one C# file per JS module, same public names, same internal call order, so verification is a side-by-side read rather than a judgement call. GameDirector._PhysicsProcess is a literal transcription of game.js:121-181 — I verified that tick order in source (Otgan/Vaqt, clock decrement, loss check, win check, player, acts, waves, Humo, apprentices, combat, attack resolution, OzaroTasir, four HUD calls, Tarobiy pulse, fog lerp) and the plan reproduces it exactly. Zero CUT entries, zero deferrals. Every shipped bug is reproduced behind a named boolean with a JS file:line citation (_terakUmumiySoladi, _gapOlimTuzatilgan, SehrliTurishChegi, TosiqBalandlikChegi) so it reads as a decision, not an accident. The CharacterBody3D argument satisfies CLAUDE.md in letter (Velocity + MoveAndSlide in _PhysicsProcess) while CollisionMask=0 guarantees the engine cannot deviate from the web's integration. The frame-timing argument is correct and load-bearing: pinning the whole sim to 60 Hz makes the four UNcorrected smoothers (dt*3 FOV, dt*7 gait, dt*0.6 fog, dt*2.2 chizma) land on exactly the values the author tuned at. Risk section is the most specific — Draco verified as a hard ERR_PARSE_ERROR, ffmpeg verified decode-only, the clip map admitted as nearly empty.

**Weaknesses.** Mirroring JS module boundaries is right for simulation and wrong for presentation: hud.js, chizma.js, videolar.js and audio.js have no natural 1:1 C# shape and the mirror forces one anyway. Step ordering front-loads a parity harness that prints CSV — correct engineering, but nothing is on screen until step 3 and nothing playable until step 5. Its audio answer (bake 15 WAVs via Node's OfflineAudioContext) adds a toolchain dependency and quietly abandons the literal zero-audio-files property. Models.cs and Qurollar.cs mirror files whose content largely dissolves into Godot import settings, preserving a shape with no job.

### `design:godot-native` — 87

**Strengths.** The four written REFUSALS are the most valuable durable artifact in any of the three, and each is correct: move_and_slide cannot reproduce the annulus-interior-is-solid rule (world.js:615-625 records the oscillation bug it fixes); SpringArm3D pulls in on collision and the web camera never does; NavigationAgent3D deletes the wall's meaning because 'blocked' is a coordinate box (x<62.5, |n.z-p.z|<6.6, n.x-4<p.x<n.x+2.5), not a collision; TimeManager counts UP at 0.02 days/s while Buxoro's clock counts down and runs backward. The Smoothing static class holding all four web idioms verbatim so no later file re-derives a constant is a clean structural guarantee. max_physics_steps_per_frame=3 turns the web's 1/20 s clamp into a live, faithful slow-motion floor instead of dead insurance. Live C# WavSynth rendering AudioStreamWAV in memory preserves the zero-sound-files property exactly with no external toolchain. Asset unblock is step 1, where it belongs.

**Weaknesses.** Dissolving the module boundaries buys elegance and pays in verifiability, which is weighted second: with O exploded into eleven sibling nodes there is no file-for-file diff, so proving fidelity falls back on two .tres files plus playtests. Routing through MapLoader/MissionMap forces inventing a BuxoroCompletionTask purely because MissionMap warns and can never finish with zero EngineeringTasks — a fake objective added to satisfy a framework the episode otherwise refuses. Its resolution of the duplicate olim key ships the unreachable line as an unused key with a switch, which is right, but decides for the user rather than surfacing the decision.

### `design:data-first` — 83

**Strengths.** The best verification thinking anywhere, and concrete rather than aspirational: a golden trace for the apprentice FSM (the web dumps state/x/z/carry/ombor every 0.5 s for a scripted 120 s run, C# replays the identical mark sequence and diffs positions to 0.01 m); a 20,000-point lattice sweep through toqnashuvHal in both languages diffed to 1e-4; offline-render RMS envelope plus spectral centroid per audio clip; screenshot diff against the web with models disabled. Its honest 'five things genuinely resist being data' framing is exactly right. BuxoroRuntime as an autoload makes the no-literals rule structural instead of a matter of discipline. Per-field // source: file:line provenance is the cheapest defence against the README's false single-source claim. Landing both .tres files before any behaviour gives the user one surface to sign off in a sitting.

**Weaknesses.** It deliberately deviates from shipped behaviour in three places — fixing the E-hold accumulation, fixing the touch intro-skip, and making the locale toggle retranslate live, 'the only place the port is deliberately better'. All three are flagged, so nothing is silent, but the mandate is a literal transfer and the correct default is reproduce-behind-a-flag. Worst front-loading of the three: steps 1 and 2 produce a Node that prints constants and nothing else, so there is no runnable world for weeks — precisely the condition under which fidelity checks start getting skipped. It inherits the MissionMap adapter problem with the same one-fake-task workaround.

## Ideas grafted from the runner-up approaches

- From godot-native — the four written REFUSALS, promoted to a permanent docs/BUXORO_REFUSALS.md and repeated as a file-header comment at each site: no move_and_slide world collision, no SpringArm3D, no NavigationAgent3D on enemies or apprentices, no TimeManager for the clock. Each carries the JS file:line and the exact number it would change.
- From godot-native — Smoothing.cs holding all the web idioms as named expressions (PosLerp 1-Pow(1-0.12,dt*60), FovLerp Min(1,dt*3), BodyYaw 1-Exp(-14*dt), ActorYaw 1-Pow(1e-6,dt), ShogirdYaw 1-Pow(1e-6,dt), GaitBlend Min(1,dt*7), FogLerp dt*0.6, ChizmaFade dt*2.2). No other file may re-derive a smoothing constant.
- From godot-native — max_physics_steps_per_frame = 3 alongside physics_ticks_per_second = 60, making the web's dt clamp at 1/20 s a live, faithful slow-motion floor rather than the dead one-line insurance mirror proposed.
- From godot-native — WavSynth: render the 15 SFX as AudioStreamWAV in memory at _Ready from SfxRecipeData resources, preserving the literal zero-audio-files property and removing the Node/OfflineAudioContext dependency from the shipping path. Mirror's bake is kept but demoted to the verification oracle.
- From godot-native — ActorClipSet as a .tres carrying clip availability as a build-time capability set with keys already sanitized (NlaTrack.003 to NlaTrack_003, tripo::Root to tripo__Root), rather than probing at runtime. The soft-fail boolean contract gameplay branches on survives; the guesswork does not.
- From godot-native — one BoneMap authored against SkeletonProfileHumanoid, exploiting the byte-identical 41-bone Tripo rig shared by all six characters. The only affordable route to the ~20 clips the code requests and klip-xaritasi.json does not name.
- From data-first — the five named proof mechanisms: golden trace for the apprentice FSM, 20,000-point lattice diff for toqnashuvHal, offline-render RMS plus spectral-centroid comparison for audio, four-camera screenshot diff for procedural architecture, and a unit test asserting camera offsets and that pitch input changes nothing.
- From data-first — BuxoroRuntime autoload exposing Balance/Sehrli/World statically, so the no-literals rule is enforced by structure. Mirror's static Balance.B accessor is replaced by this.
- From data-first — instrument the ORIGINAL rather than comparing by eye: a ?parity=1 hook on a throwaway copy of the web build emits the same CSV blocks the Godot ParityDump emits (LCG draws, node table, terrain samples, apprentice trace, solver lattice). Every fidelity gate becomes a file diff.
- From data-first — a // source: file:line provenance doc comment on every field of the second tuning resource, since README:197's 'only balance.js' claim is verifiably false.
- From data-first — land both tuning .tres files before any behaviour exists, as one surface the user signs off in a single sitting. Kept, but moved behind the asset unblock so the user sees a model at correct scale on day one.
- REJECTED from data-first, stated so the choice is visible: its three deliberate fixes (E-hold accumulation, touch intro-skip, live locale retranslation) are reverted to faithful reproduction behind named booleans, per the mandate.

## Approach

MIRROR THE SIMULATION, GO NATIVE FOR PRESENTATION, PUT EVERY NUMBER IN .TRES. A self-contained episode at res://scenes/buxoro/BuxoroEpisode.tscn that does not route through MapData/GenericMissionMap/MissionMap/MapLoader at all.

LAYER 1 — SIMULATION IS MIRRORED, FILE FOR FILE. world.js→World.cs, player.js→Player.cs, apprentices.js→ApprenticeSystem.cs, combat.js→CombatSystem.cs, game.js→GameDirector.cs, humo.js→Humo.cs, anim.js→Anim.cs, actors.js→Actors.cs, binolar.js→Binolar.cs, otlar.js→Otlar.cs, qurollar.js→Qurollar.cs, touch.js→Touch.cs, main.js→BuxoroMain.cs. Same public method names transliterated, same internal call order, same numbers. Only GameDirector, BuxoroHud, Chizma and Videolar are Godot Nodes; the rest are plain C# classes owned by GameDirector and driven by explicit calls. GameDirector._PhysicsProcess is a literal transcription of game.js:121-181, verified in source. A reviewer opens World.cs beside world.js and checks it function by function.

LAYER 2 — PRESENTATION IS GODOT-NATIVE, because mirroring a DOM overlay or a scissor viewport preserves a shape with no job. BuxoroHud is a CanvasLayer of Controls; Chizma is a SubViewport, deleting the scissor/autoClear/clearDepth dance and the CSS-px-vs-device-px trap at chizma.js:122-124; Videolar is a VideoStreamPlayer; Otlar is four MultiMeshInstance3D; translation is TranslationServer over one CSV; ally HP bars are Camera3D.UnprojectPosition. Each produces the same observable result with less machinery, and each mapping says so.

LAYER 3 — EVERY NUMBER IS DATA, IN TWO RESOURCES. balance_buxoro.tres transcribes balance.js verbatim; sehrli_buxoro.tres collects every tuning value hardcoded outside it, each field carrying its JS file:line. No C# file in the episode holds a gameplay literal; all read through the BuxoroRuntime autoload. A parity check is then a diff of two .tres files against balance.js, not a code read.

TICK MODEL. Engine.PhysicsTicksPerSecond = 60, max_physics_steps_per_frame = 3, physics_interpolation = true. The ENTIRE simulation including camera and occlusion fade runs in GameDirector._PhysicsProcess. dt is then the constant 1/60, exactly what the author tuned on, so the frame-rate-UNcorrected smoothers land on their authored curves and the corrected ones are unchanged. The 3-step catch-up reproduces the web's 1/20 s clamp as a real slow-motion floor.

MOVEMENT. Shomurod is a CharacterBody3D moved with Velocity + MoveAndSlide() in _PhysicsProcess per CLAUDE.md, with CollisionMask = 0 so MoveAndSlide is a pure integrator; then World.ToqnashuvHal — a literal port of the 3-pass solver — runs on GlobalPosition, in the web's exact order move, clamp, solve, clamp, snap Y. Apprentices, enemies and soldiers are Node3D with no CollisionShape3D at all, because toqnashuvHal has exactly one caller (player.js:298) and their travel times ARE the resource economy.

INTEGRATION. Reachable from ChronoShift's MainMenu as catalog row '001 — BUXORO 1238' via ChangeSceneToFile. The seven autoloads stay loaded and idle; Buxoro has no money, XP, inventory or tech tree. Genuine reuse is limited to ArmIk, Sfx/Fx, ResourceFolder, DistanceCull, ModelDresser and the CharacterData clip-name-table pattern.

### Rationale

Fidelity is the success criterion and the cheapest proof of fidelity is a mechanical one. For the simulation that proof is a file-for-file diff, which is why the mirror wins the simulation layer outright. For the numbers that proof is a .tres-versus-balance.js diff, which is why data-first's transcription discipline is adopted wholesale rather than letting constants be retyped inline as each system needs them. For presentation there is no mechanical proof either way, so the tie breaks on which produces the same pixels with fewer moving parts, and that is consistently Godot-native.

Reuse candidates were evaluated one by one against 'can it produce the SAME observable behaviour and the SAME numbers', and almost all fail. GenericMissionMap/MapData cannot express a 180x100 field (GroundSize is one centred square, default 70), a 46 m wall ring, six rotated wall points, an analytic terrain function, or a win condition that is a kill counter — its only completion rule is counting EngineeringTasks, which is why both runners-up ended up inventing a fake task. This plan refuses the framework instead. PlayerController's stamina, MoveToward acceleration and TechTreeManager speed multiplier contradict the web's instant-start/instant-stop 5.0/7.0. PlayerHealth regenerates 7 hp/s after 4 s and teleport-respawns, which would make a 10-minute run unlosable. WeaponController is 724 lines of hitscan with magazines and Nathan-specific bone names baked into [Export] defaults where FindBone returns -1 and every consumer silently early-returns. Enemy.cs has 100 hp and no wall-priority rule. TimeManager counts up at 0.02 days/s. Each refusal is written down with the number it would change.

What survives is real: ArmIk is pure Skeleton3D math with no era assumptions and is exactly what the bow's two-hand anchor and the sieve-shield's forearm pin need; Sfx.PlayAt/PlayUi is the right one-shot pattern; ResourceFolder is mandatory or the exported build loads zero .tres; DistanceCull and ModelDresser already solve the triangle-budget and FBX-drops-textures problems; and CharacterData's shape — the resource carries the clip NAMES so different rigs survive — is precisely what klip-xaritasi.json is.

Step order is chosen so the user sees something in the editor on day one and plays real movement in week two, because the realistic failure mode of a 6,210-line literal port is not any single system, it is fidelity checks quietly getting skipped when there is nothing on screen to check them against. The asset unblock is step 1 because Draco is a verified hard blocker.

## Implementation steps

### Step 1. Asset unblock — strip Draco, import 17 models at correct scale, facing and loops

**Deliverable.** CLAUDE WRITES: tools/buxoro_models.sh (gltf-transform decode of all 17 GLBs), BuxoroImport.cs (EditorScenePostImport applying the moljal height table, the bbox foot-drop, CullMode.Disabled for the 12 two-sided models, LoopMode.Linear on locomotion clips, and the sanitised clip-name map), moljal.tres, klip_xaritasi.tres, and the BoneMap against SkeletonProfileHumanoid. This is step 1 because Draco is a verified hard blocker — ERR_PARSE_ERROR at gltf_document.cpp:7377, no partial import, no fallback — and nothing visual is evaluable until it clears.

**Files.** `tools/buxoro_models.sh`, `scripts/buxoro/models/BuxoroImport.cs`, `resources/data/buxoro/moljal.tres`, `resources/data/buxoro/klip_xaritasi.tres`

**User does (Godot editor).** Run the shell script over all 17 files in Torobiy's src/assets/models/ and copy the output into res://assets/models/buxoro/. In Godot, set BuxoroImport.cs as the post-import script on each .glb's Import tab and click Reimport. Drag shomurod.glb into an empty 3D scene next to a 1.78 m BoxMesh reference. Separately install a theora-enabled ffmpeg NOW (brew tap homebrew-ffmpeg/ffmpeg && brew install homebrew-ffmpeg/ffmpeg/ffmpeg --with-theora) so it is not a surprise at step 15 — the stock one on this Mac is decode-only.

**Verified by.** Console is free of 'KHR_draco_mesh_compression is not supported' — before this step every single file fails with it. Shomurod's head reaches the top of the 1.78 m box, NOT 0.975. Noyon measures 2.05 and visibly towers over mogul_piyoda at 1.72; hunarmand at 1.35 reads as a child. Walk the editor camera INSIDE shahar_devori.glb: no face may be invisible. In the AnimationPlayer dropdown NlaTrack_003 exists with an underscore, not a dot, and loops instead of freezing after one cycle; play it and confirm the Hip does not drift forward. The user sees a correctly-scaled Shomurod on day one.

### Step 2. Determinism kernel — the two tuning resources, the LCG, terrain, Smoothing, and the parity harness

**Deliverable.** CLAUDE WRITES: BuxoroBalance.cs, BuxoroSehrli.cs, BuxoroWorld.cs and the four row types, every .tres hand-written in text format 3 following repo convention (Script ext_resource with NO uid, load_steps = 1 + ext + sub, [resource] opening with script = ExtResource("1") — all three confirmed against item_res_wood.tres); Lcg.cs (ulong, seeded 1238/8317/4711, with the doc comment explaining that the intermediate product peaks at ~7.15e15 < 2^53 so JS doubles and ulong agree bit for bit, while a uint implementation wraps into a completely different field); BuxoroTerrain.cs; Smoothing.cs holding all the web idioms as named expressions; BuxoroRuntime.cs autoload with a loud PushError on a null export; ParityDump.cs; and tools/parity/dump_web.js, a throwaway ?parity=1 hook for the web build emitting the identical CSV.

**Files.** `scripts/buxoro/data/BuxoroBalance.cs`, `scripts/buxoro/data/BuxoroSehrli.cs`, `scripts/buxoro/core/Lcg.cs`, `scripts/buxoro/core/Smoothing.cs`, `scripts/buxoro/core/BuxoroRuntime.cs`, `scripts/buxoro/world/BuxoroTerrain.cs`, `scripts/buxoro/debug/ParityDump.cs`, `resources/data/buxoro/balance_buxoro.tres`, `tools/parity/dump_web.js`

**User does (Godot editor).** Register BuxoroRuntime in Project Settings → Autoload. Set physics_ticks_per_second = 60 and max_physics_steps_per_frame = 3 (verified: project.godot has no [physics] section today). Open both .tres in the Inspector and confirm every [Export] field appears — if one is missing, [GlobalClass] or the field's accessibility is wrong. Attach ParityDump to a scratch scene, press F6, then open the web build with ?parity=1 and diff the two CSVs.

**Verified by.** THE GATE FOR EVERYTHING DOWNSTREAM. The first 200 raw LCG draws must be byte-identical between the browser and Godot; if the LCG diverges, nothing built on top of it is worth building. All 66 node positions agree to six decimals INCLUDING their order (a correct LCG with the wrong key-iteration order produces a matching set in a permuted sequence — the diff catches that too). YerBalandligi agrees at (96,6), (108,1), (152,-22), (148,20), (60,0), (172,0). Separately the user reads all ~110 printed constants beside balance.js in one sitting, paying attention to the three the comments contradict: YollashVaqti 11.0 not 15, DevorUshlabTurish 6.0 not 5, TugunQoldiTosh 4 not 1. A _Ready assertion fails loudly if the wave rows do not sum to JamiDushman.

### Step 3. World.cs — the walkable field appears: terrain, 66 nodes, poplars, wall markers, obstacle registry, solver

**Deliverable.** CLAUDE WRITES World.cs complete — Qur(), the ground ArrayMesh displaced with WORLD x, TugunYarat, TerakYarat, DevorNuqtalari, TosiqQosh/TosiqOchir/BinoTosiq, and ToqnashuvHal as a literal port of the 3-pass solver — plus a SolverSweep debug command sampling a 0.5 m lattice over the whole 180x100 field into user://solver_godot.csv, with the matching web-side sweep in tools/parity/. This is where the project stops being text and becomes a place.

**Files.** `scripts/buxoro/world/World.cs`, `scenes/buxoro/BuxoroEpisode.tscn`, `tools/parity/solver_web.js`

**User does (Godot editor).** Create BuxoroEpisode.tscn: Node3D root 'BuxoroEpisode' with GameDirector.cs attached (a stub for now), children Dunyo (Node3D) → Tugunlar, Teraklar, Binolar, Shogirdlar, Dushmanlar, Askarlar, Oqlar, all Node3D; plus WorldEnvironment (FogMode Exponential, density 0.0075, colour d4c8b0), Quyosh (DirectionalLight3D, energy 0.85, shadows 2048), and a Camera3D. Set GameDirector's balance exports. F6 and fly the editor camera over the field.

**Verified by.** Two proofs. (a) VISUAL: 66 node props stand in exactly the five zones, six wall markers at x=60 z=-33/-19.8/-6.6/6.6/19.8/33, and a screenshot from (96,40,6) looking west overlays the web game's same view. The ground must visibly undulate with ±0.30 m total amplitude and the wave phase must line up at x=60 and x=152 — if a character would sink knee-deep, the ground was displaced with LOCAL x instead of world x, which is the world.js:70-76 shipped bug and the easiest way to make the port look broken on day one. (b) MECHANICAL, grafted from data-first: the 0.5 m lattice sweep through ToqnashuvHal at radius 0.45 is run in both languages and diffed — every output position within 1e-4. That 20,000-point diff is far stronger evidence than any amount of walking into walls.

### Step 4. Player.cs — PLAYABLE. Movement, jump, the solver in the loop, camera, occlusion fade

**Deliverable.** CLAUDE WRITES Player.cs with Yangila (move → clamp → ToqnashuvHal → clamp → snap Y), the cosmetic jump, Kamera with the six-hit occlusion ray, and BuxoroInput reading only Input Map actions. Plus a unit test asserting the camera offset table (6.0/8.0 back, 4.5/5.5 up, 70/78 FOV) and that pitch input changes the camera not at all.

**Files.** `scripts/buxoro/player/Player.cs`, `scripts/buxoro/input/BuxoroInput.cs`, `scenes/buxoro/Shomurod.tscn`

**User does (Godot editor).** Build Shomurod.tscn: CharacterBody3D 'Shomurod' with CollisionLayer 2 and CollisionMask 0 — the empty mask is ON PURPOSE so MoveAndSlide integrates without world collision and World.ToqnashuvHal does the resolving → CollisionShape3D (capsule r 0.45 h 1.78) → BodyOrient (Node3D, rotation Y 180°) → the imported shomurod model + AnimationPlayer → KameraPivot → Camera3D (fov 70). Add it under BuxoroEpisode. In Project Settings → Input Map add, all by physical_keycode: buxoro_belgila (E), buxoro_yasa (F), buxoro_urish (LMB), buxoro_blok (RMB), buxoro_qurol_1/2/3, buxoro_til (L); reuse move_*, jump and run. Playtest with the web build open beside it.

**Verified by.** STOPWATCH, not vibes. Run x=96 to x=156 walking: exactly 12.0 s at 5.0 m/s; running: 8.57 s at 7.0. Jump apex 1.079 m, airtime 0.939 s against a debug readout. Walk into a house, a poplar, a bazaar stall and the gate: eject smoothly with zero sticking — the 'never stuck' guarantee the 3-pass solver exists for. Approach the city wall ring from OUTSIDE and from INSIDE: both must eject outward to r2+0.45 with no oscillation, which is the annulus rule and the easiest thing here to get subtly wrong. Walk straight over a wall-point marker and pass through it. Confirm you can still get within 3.2 m of a seated craftsman — if not, the stall footprint came from the canopy bbox instead of the 2.4x2.0 platform. Move the mouse up and down: NOTHING happens, and that is correct.

### Step 5. Binolar.cs + Otlar.cs — the field becomes Buxoro

**Deliverable.** CLAUDE WRITES all seven building generators, QiyaBlok battered-wall geometry, the 46 m wall ring with its 4 ruins / 1 hole / skipped east arc as MultiMeshInstance3D per variant, the gate with named leaves, the Ark with Humo's dome, and 1670 grass instances across four MultiMesh groups.

**Files.** `scripts/buxoro/world/Binolar.cs`, `scripts/buxoro/world/Otlar.cs`, `resources/data/buxoro/bino_specs.tres`

**User does (Godot editor).** Nothing new in the tree — World.Qur calls both. F6 and walk the field on foot along the gate road and around the wall ring.

**Verified by.** Side-by-side screenshots against the web build from four fixed points: the gate at (172,0), the Ark at (250,0,10), the bazaar at (148,20), the forge at (152,-22) — this is data-first's screenshot-diff proof applied where it belongs. Wall segments 3, 4, 11 and 17 must be ruined and 12 missing, in the same places. Grass absent from the wall line ±3.5 m, the forge r²<60, the bazaar r²<90 and the gate road — and running past it must READ AS SPEED, which is its stated purpose; if the field feels static, the density was trimmed.

### Step 6. Anim.cs + Models.cs + Actors.cs — the animation spine and the procedural fallback that actually ships

**Deliverable.** CLAUDE WRITES Anim.cs (Oynat returning the availability boolean from a cached ActorClipSet, the two-mode Yurish selector, the Qulf one-shot lock, the [0.55,2.4] clamp), Models.cs with the null-returns-procedural contract and a loud PushError on a missing bone or clip rather than a silent early-return, Actors.cs box-figure cast, and ProtseduralYurish.cs with its three style tables.

**Files.** `scripts/buxoro/anim/Anim.cs`, `scripts/buxoro/anim/ProtseduralYurish.cs`, `scripts/buxoro/models/Models.cs`, `scripts/buxoro/actors/Actors.cs`

**User does (Godot editor).** Attach an AnimationTree to Shomurod.tscn: StateMachine with Idle/Walk/Run/Attack/Die at xfade 0.18; a BlendSpace1D on speed with points 0 / 1.21 / 4.33; a TimeScale node; a OneShot in Add mode for attacks. Set RootMotionTrack = 'Armature/Skeleton3D:Hip' and ProcessCallback = Manual (GameDirector advances it). Playtest walk→run→idle.

**Verified by.** Shift must VISIBLY switch to the run clip (ordered mode), not blend halfway. Feet slide at full run — correct and deliberate, an art call recorded at anim.js:90-96, not a bug to fix. Temporarily rename a clip in the AnimationPlayer: the actor falls back to the procedural sine bob, never a T-pose or a freeze, and the console NAMES the missing clip. Spawn six enemies at once: gaits visibly out of phase. With RootMotionTrack set, a looping walk keeps the mesh centred and the Hip does not drift.

### Step 7. ApprenticeSystem.cs — the core verb: marking, the 5-state FSM, the ombor, recruiting

**Deliverable.** CLAUDE WRITES Belgila with its three-value return and queue cap, the mark pillar (additive, fog-disabled, depth-write off, pulsing), YangilaShogird's five cases, Yur, the ombor, ShogirdQosh, YollashTekshir with the per-craftsman baseline, and BuxoroTraceHarness.cs plus the web-side trace_capture hook.

**Files.** `scripts/buxoro/apprentices/ApprenticeSystem.cs`, `scripts/buxoro/debug/BuxoroTraceHarness.cs`, `scenes/buxoro/Shogird.tscn`, `scenes/buxoro/Belgi.tscn`, `tools/parity/trace_capture.js`

**User does (Godot editor).** Build Shogird.tscn: Node3D → BodyOrient → model + AnimationPlayer + Savat (basket at (0,1.02,0.30) or bone-attached to the left hand). NO CollisionShape3D — apprentices collide with nothing by design. Build Belgi.tscn and verify the pillar material by eye in the viewport: Unshaded, Transparency Alpha, Blend Add, Depth Draw Never, Disable Fog ON, Cull Disabled, albedo ffb524 alpha 0.85. Add two apprentices under Dunyo/Shogirdlar. Run the scripted 120 s trace in both builds and diff.

**Verified by.** THE ECONOMY GATE, and the hardest fidelity check in the plan — proved by golden trace, grafted from data-first. The web hook dumps (t, apprenticeIndex, state, x, z, carry, ombor) every 0.5 s for a scripted 120 s run with a fixed mark sequence; the C# harness replays the identical sequence and diffs: positions within 0.01 m, ombor counts exact. Plus four targeted asserts: a stone node needs exactly TWO Belgila calls and two round trips to exhaust while every other type needs one; a stone round trip measures ≈42.3 s at the 1238 seed (wood 12.8, leather 25.5, iron 26.8, coal 34.0 — sample ten of each and compare means); with 2 apprentices the 5th mark is REFUSED with the rad sound and a navbatTola toast, never replacing; an enemy parked on a working apprentice produces the 5-second flee/resume/re-flee oscillation, not sustained flight. Holding E down the stall row restarts the bar at each craftsman, never chain-recruiting, and a recruit completes at 11.0 s. Finally: the beam must be readable at 100 m against the bright sky — if it washes out, the additive/fog-off/depth-write-off triple is wrong and the only signal the mechanic has is dead.

### Step 8. GameDirector.cs — the ordered tick, the clock, acts, tutorial, OzaroTasir, wall building, crafting

**Deliverable.** CLAUDE WRITES the transcription of game.js:121-181 and main.js:266-297 as one _PhysicsProcess, plus Yarat, PardaYangila, TutorialYangila, OzaroTasir, NimaYasaladi, Yasa, VaqtQosh, Hodisa, and BuxoroMain.cs with the freeze branch and the opening flythrough.

**Files.** `scripts/buxoro/GameDirector.cs`, `scripts/buxoro/BuxoroMain.cs`, `scripts/buxoro/combat/CombatSystem.cs`

**User does (Godot editor).** Set GameDirector's exports (balance, sehrli, world, mission preset). Playtest a full Act I: mark stone, bank 4, build a wall section, watch the clock jump backward.

**Verified by.** Put GameDirector._PhysicsProcess beside game.js:124-181 and confirm the calls are in the same order — clock decrement and BOTH terminal checks first, then player, acts, waves, Humo, apprentices, combat, attack resolution, OzaroTasir, the four HUD calls, Tarobiy pulse, fog. This is the mirror's central claim and it is checked by reading, not running. Then: building one wall adds exactly +25 s and caps at 720; build all six and the cap demonstrably binds (150 s of bonus against 120 s of headroom, so the last two are worth less). Walk over a built section and pass through it — if you cannot, the 1.0 m height gate was accidentally 'fixed'. Start the parda2 preset at 480 and confirm the bow blueprint opens on frame one alongside wave 1.

### Step 9. CombatSystem.cs — enemies, waves, wall HP, soldiers, homing arrows

**Deliverable.** CLAUDE WRITES TolqinChiqar, DushmanYangila's three-priority decision with the coordinate-box wall test and the cavalry flank, DevorQur/DevorBuzildi, the eight soldiers and their three modes, OqOt/OqYangila, and the unified ZararBer.

**Files.** `scripts/buxoro/combat/CombatSystem.cs`, `scripts/buxoro/combat/Oq.cs`, `scenes/buxoro/Dushman.tscn`, `scenes/buxoro/Askar.tscn`

**User does (Godot editor).** Build Dushman.tscn and Askar.tscn as Node3D → BodyOrient → model + AnimationPlayer, with NO CollisionShape3D (enemies collide with nothing). Use the parda2 preset to reach wave 1 in seconds. Playtest all three waves.

**Verified by.** Wave 1 spawns exactly 6 piyoda along the west edge at clock 480. One piyoda takes exactly 6.0 s to flatten a 140 hp section; six take ~1 s. A mounted otliq slides AROUND a built section and never attacks it. An enemy spawning at z=44 walks past the wall line untouched, because coverage stops at ±39.6 — that hole is in the source. Build a wall between waves and confirm the next wave arrives genuinely LATER, because the clock moved: this coupling is the game's main strategic decision and an elapsed-time timer would silently delete it. The swordsman soldier chases the last enemy across the whole 180 m and never comes home. An unarmed soldier never advances. Arm a soldier twice: refused.

### Step 10. Player weapons, shield, HP, the single damage seam

**Deliverable.** CLAUDE WRITES Urish as a pure query returning the hit list, ZarbaPozasi as an additive AnimationNodeOneShot layer, the shield, HP, and the one dushmanUrdi damage seam in GameDirector.

**Files.** `scripts/buxoro/player/Player.cs`, `scripts/buxoro/world/Qurollar.cs`, `scripts/buxoro/GameDirector.cs`

**User does (Godot editor).** Add the club mesh to Shomurod's R_Hand BoneAttachment3D. Playtest melee against a live wave and try blocking.

**Verified by.** One club swing into three enemies inside the ±70° cone damages ALL THREE for 8 each — no falloff, no per-target cap. Damage registers on the frame the button goes down while the 0.34 s swing plays after. Spin the camera fast and swing: the hit arc visibly TRAILS the crosshair, because the cone tests body yaw which lags at 14/s — shipped behaviour. Club a piyoda: 50/8 = 7 swings at 0.60 s. Sword a noyon: 200/25 = 8 at 0.45. Fire the bow with no target in cone: the 0.90 s cooldown and animation still fire with no arrow. Fire at a running otliq: the arrow curves and cannot miss. Block a piyoda's 7 damage and take exactly 2.1, from any direction, with no stamina cost.

### Step 11. BuxoroHud.cs + the translation table + theme

**Deliverable.** CLAUDE WRITES the 13 HUD widgets, the clock danger pulse, the hold bar, the blueprint strip's four states, the toast queue, the ally HP bars via UnprojectPosition, buxoro.csv, and buxoro_theme.tres.

**Files.** `scripts/buxoro/ui/BuxoroHud.cs`, `scenes/buxoro/BuxoroHud.tscn`, `resources/translations/buxoro.csv`, `resources/theme/buxoro_theme.tres`

**User does (Godot editor).** Build BuxoroHud.tscn as a CanvasLayer with the 13 Control children named to match the exported NodePath defaults, and apply buxoro_theme.tres to its root. Add it under BuxoroEpisode. In Project Settings → Localization import buxoro.csv and add both .translation files. Playtest and click the UZ/EN toggle mid-dialogue.

**Verified by.** Only the clock and six hand dots are on screen at rest — nothing else, ever. The clock turns red and pulses at 60 s. HP bar turns red below 42. At Act II the resource readout REPLACES stone with wood+leather, making your stone invisible — correct, matches source, and worth the user seeing so it is not later reported as a bug. Toggling language mid-line leaves the visible line in its old language until it expires — reproduced deliberately, not fixed. Screenshot at 1920x1080 and 1280x720: the vmin proportions are identical. Ally bars appear within 12 m and turn red at 30%; enemies never get bars.

### Step 12. Humo.cs + the diegetic feedback layer

**Deliverable.** CLAUDE WRITES the four-state bird with its procedural wing fallback, the fog coupling, Tarobiy's pulsing pillar, the damage vignette, and the time-gain flash with the clock keyframe.

**Files.** `scripts/buxoro/Humo.cs`, `scripts/buxoro/ui/BuxoroHud.cs`, `scenes/buxoro/Humo.tscn`

**User does (Godot editor).** Add Humo.tscn under Dunyo (Node3D → model with named wing groups, AnimationPlayer, unshaded emissive DisableFog material, scale 3.2). Playtest to 8:15 with the HUD toggled off and watch the sky.

**Verified by.** At clock 495 the bird spreads its wings and circles the dome at r=16 climbing to 14 m while the fog visibly thickens 0.0075 → 0.0115; at 480 it takes off to the wide circle at r=62/34 and 46 m. Readable from the far west end at ~250 m. On defeat it flies east at 26 m/s and never returns. THE ACCEPTANCE QUESTION: with the HUD hidden, can the user tell a wave is coming from the bird and the haze alone? If not, the only pre-wave telegraph in the game is broken. And the negative check: confirm there is no wave counter, no objective marker and no 'WAVE 2' text anywhere.

### Step 13. Chizma.cs blueprint viewer

**Deliverable.** CLAUDE WRITES the SubViewport corner viewer with its own camera and lights, the fit-and-spin, the caption overlay, and the non-modal contract.

**Files.** `scripts/buxoro/ui/Chizma.cs`, `scenes/buxoro/ChizmaKorik.tscn`, `resources/data/buxoro/malumot_*.tres`

**User does (Godot editor).** Build ChizmaKorik.tscn: SubViewportContainer (30% width, 0.75 aspect, left 8.5%, centred) → SubViewport → Camera3D (FOV 38, z 2.4, near 0.05, far 20) + two DirectionalLight3Ds + a Pivot. Add under BuxoroHud. Playtest the Act II transition.

**Verified by.** At Act II the bow appears in the corner, fit to 1.35 units, spinning at 0.75 rad/s with the sine wobble, for exactly 7 s — AND THE GAME DOES NOT PAUSE: keep running while it spins. If the tree pauses, the beat is dead and the whole reason chizma.js exists is gone. The sword unlock (9 s) shows 'Bu chizmani hech kim bermadi.' in the ACTIVE language, not Uzbek-only.

### Step 14. BuxoroAudio.cs — live-synthesised SFX, breathing wind, the melody, and the Act III silence

**Deliverable.** CLAUDE WRITES WavSynth.cs (oscillator/noise/biquad/envelope renderer producing AudioStreamWAV in memory), the 15 sfx_*.tres, kuy_dphrygian.tres, BuxoroAudio.cs, default_bus_layout.tres, and tools/parity/bake_reference.js — which runs audio.js's own zarb() table under OfflineAudioContext to produce 15 reference WAVs used ONLY as the verification oracle, never shipped.

**Files.** `scripts/buxoro/audio/WavSynth.cs`, `scripts/buxoro/audio/BuxoroAudio.cs`, `resources/default_bus_layout.tres`, `resources/data/buxoro/sfx_*.tres`, `tools/parity/bake_reference.js`

**User does (Godot editor).** Load default_bus_layout.tres in Project Settings → Audio (verified: the project has none today, Master-only). Playtest with headphones through Tarobiy's death at 4:00, then open the pause menu and resume.

**Verified by.** Objective first, grafted from data-first: render both the reference bake and the C# output to WAV and compare RMS envelope and spectral centroid per clip — the synthesis parameters are the spec, so the two should track closely, and a divergent clip names its own bug. Then subjective A/B at the same trigger for the four that carry the most meaning: the anvil, the mark chime, the four-note time arpeggio, the bird cry. The wind must audibly BREATHE on an ~11 s cycle (0.09 Hz), not loop flat. The victory reprise at tezlik 1.35 must change TEMPO without transposing. At 4:00 the music fades over 1.2 s and never comes back for the last four minutes, leaving wind, hammer, steps and iron — then pause and resume and the music must STILL be gone. If pause restores it, the act's tonal turn is destroyed. Finally: confirm no .wav or .ogg ships under res://assets/audio/buxoro/ — the synthesis IS the asset.

### Step 15. Videolar.cs — Theora cutscenes, cue table, skip semantics, freeze contract

**Deliverable.** CLAUDE WRITES tools/buxoro_video.sh, the cue table as sahna_*.tres, Videolar.cs with both modes, the one-shot dedupe, the n/total counter and the three-way skip model.

**Files.** `tools/buxoro_video.sh`, `scripts/buxoro/ui/Videolar.cs`, `scenes/buxoro/Videolar.tscn`, `resources/data/buxoro/sahna_*.tres`

**User does (Godot editor).** Run the transcode script against assets/video/_xom/ (the 145 MB originals, never the shipped copies). Import the .ogv files and add Videolar.tscn as a CanvasLayer under BuxoroEpisode. Check the total size (expect 30-37 MB) and decide whether to drop to 960x540. Playtest a full run from the menu.

**Verified by.** The opening plays 1→2→3 fullscreen with the sim frozen and rendering continuing, showing '1 / 3'. Clicking advances one clip; SKIP abandons the whole remaining sequence; Escape is a full skip; gameplay keys are swallowed. Each cue fires at most once per run. Watch clip 7 (dust and smoke) fullscreen: no visible Theora ringing — if there is, the transcode came from the compressed web copy or q:v is below 6. Start the parda3 preset and watch the console: the warning must NAME video 6 as dropped, because parda2 and parda3 both fire on frame one. That drop is in the source; now it is visible instead of mysterious.

### Step 16. Menyu.cs — series catalog, Buxoro menu, mission presets, controls, pause, flythrough

**Deliverable.** CLAUDE WRITES the two-level menu, the data-driven catalog, the four presets, the controls screen with the recruit time corrected to 11 s, the pause overlay with its music carve-out, and the opening flythrough.

**Files.** `scripts/buxoro/ui/Menyu.cs`, `scenes/buxoro/Menyu.tscn`, `resources/data/buxoro/missiya_*.tres`, `resources/data/buxoro/seriya_*.tres`

**User does (Godot editor).** Build Menyu.tscn with the blueprint-sheet styling and wire the '001 — BUXORO 1238' row from ChronoShift's MainMenu.tscn to it. Set kirish.ogv as the muted looping background. Playtest each of the four missions.

**Verified by.** The catalog shows 5 rows with 2 locked. Each preset starts at its clock and apprentice count: toliq 600/2 with the opening videos, parda1 600/2, parda2 480/4, parda3 240/6 without. The 6 s flythrough runs CONCURRENTLY behind videos 1-3 on a full run — shipped behaviour, not a bug. Escape navigates back one level. The pause menu does not restart the music after Tarobiy's death.

### Step 17. Endings, death camera epilogue, stats card

**Deliverable.** CLAUDE WRITES Yakunla's three branches with their distinct camera, audio, bird state and world change; YakunYangila's epilogue; KadrKorsat.

**Files.** `scripts/buxoro/GameDirector.cs`, `scenes/buxoro/YakunKadr.tscn`

**User does (Godot editor).** Build YakunKadr.tscn (Control with the headline, two body lines and the five stat rows). Playtest all three endings — win via the parda3 preset, lose by idling to 0:00, die by standing in a wave.

**Verified by.** Win: 'QUSH QAYTDI', bird perched, music back at rate 1.35, wind 0.10, time remaining shown. Timeout: 'QUSH QAYTMADI', bird gone, wind 0.28, and the city gate SWINGS OPEN to ±1.1 rad. Death: 'USTA YIQILDI', camera hangs low on the body, and COMBAT KEEPS RUNNING over your corpse — that cruelty is the point and must not be tidied away. All three fire the ending video at exactly 3.5 s. A no-recruit run honestly reports 'shogird: 2 / 6'.

### Step 18. Touch.cs + the full-run parity pass

**Deliverable.** CLAUDE WRITES the touch input synthesiser, the five contextual buttons, tap-to-mark, and docs/BUXORO_PARITY.md — a checklist with one row per mapped system, a pass/fail column and no 'close enough' option. Also finalises docs/BUXORO_REFUSALS.md.

**Files.** `scripts/buxoro/input/Touch.cs`, `scenes/buxoro/Sensor.tscn`, `docs/BUXORO_PARITY.md`, `docs/BUXORO_REFUSALS.md`

**User does (Godot editor).** Build Sensor.tscn with the five TouchScreenButtons under BuxoroHud. Export an Android build (or enable Emulate Touch From Mouse) and playtest on a phone. Then play one complete 10-minute run of BOTH builds back to back, on all four presets, and walk the checklist row by row. Sign off per row.

**Verified by.** Touch: no gameplay code branches on platform — verified by grepping the Buxoro scripts for a platform check and finding none. Tap a node 25 m away and it marks (30 m generosity) while the keyboard still requires 3 m. The F button appears only when the recipe is affordable; block never appears with the bow; attack/block only within 14 m. The opening camera move is still skipped by the first tap, behind its named flag. FINAL PARITY: same wave timings, same ombor totals at the same clock values, same enemy count at 2:30, same ending at the same second. The rows that catch the subtle drift are listed explicitly — stone needs 2 marks; queue caps at shogird+2; walls push the wave schedule later; the clock caps at 720; the melee arc trails the camera; arrows never miss; apprentices clip through the city wall; the player walks over his own wall; stone disappears from the HUD at Act II; music never returns after 240 s; the bird is the only wave telegraph. Anything that differs is fixed or written down as a named, deliberate deviation with a reason. There is no third category.

## System mapping

47 systems. No `CUT:` entries.

| Web system | Godot approach | Kind | Effort |
|---|---|---|---|
| Tuning tables — balance.js plus every number hardcoded outside it | BuxoroBalance + BuxoroSehrli + BuxoroWorld, [GlobalClass] Resource subclasses with public [Export] fields in PascalCase (the .tres key is the field name verbatim — confirmed against ItemData.cs/item_res_wood.tres), read through the BuxoroRuntime autoload. No episode C# file holds a gameplay literal. | new | S |
| Deterministic LCG + analytic terrain (world.js:9-27) | Lcg struct with ulong state: _u = (_u*1664525 + 1013904223) % 4294967296. Max intermediate ~7.15e15 < 2^53 so JS double arithmetic and ulong agree bit for bit; a uint implementation wraps and produces a completely different field. BuxoroTerrain.YerBalandligi(x,z) = Sin(x*0.09)*0.16 + Cos(z*0.11)*0.14 in double, cast to float — a pure static, no heightmap, no raycast, sampled for every actor's Y every tick. Three named seeds as consts. Enemy spawn uses GD.Randf, NOT the LCG, because combat.js:139-142 uses Math.random — the split is reproduced deliberately. | new | S |
| Terrain mesh, world generation, lighting, fog (world.js:37-164, 349-491, 533-556) | World.cs plain class. Ground is an ArrayMesh via SurfaceTool, 680x520 at 110x80, centred x=120, each vertex displaced with WORLD x (local x + 120) — the world.js:70-76 shipped-bug comment copied verbatim into the C# so nobody 'fixes' it back to local x, which reintroduces a ~10.8 rad phase offset and sinks characters knee-deep. Environment FogMode.Exponential density 0.0075 colour #d4c8b0; ambient from sky #c6bda6 / ground #3b3427 at 0.38 replaces the three.js HemisphereLight; DirectionalLight3D energy 0.85 colour #efdcbb at (-96,40,26), 2048 shadow, ortho L-90/R90/T90/B-60, near 1 far 260, bias -0.0008. Poplars, houses, workshop, bazaar, wall ring, Ark and the 6 wall points placed in the SAME LCG consumption order as world.js or the seed diverges. | new | L |
| Resource nodes — 5 types, deterministic zones, finite stock (world.js:85-101, 166-218) | World.Tugun class {Tur, X, Z, Mesh, Qoldi, Belgilangan}. The dead fields belgiMesh and band are dropped. Yield is Tur==Tosh ? 4 : 2 from tugun_*.tres, sourced from world.js:215 which I verified in source — NOT the obsolete comment directly above it describing 14 nodes at 1 unit, which would break the entire Act I economy. Depletion sets Mesh.Visible=false and the Tugun STAYS in the list forever; every query keeps the Qoldi<=0 filter, so indices never shift and yaqinTugun and the touch raycast behave identically. | new | S |
| Obstacle registry + toqnashuvHal 3-pass solver (world.js:558-659) | World.Tosiq struct with TosiqShakli {Aylana, Tortburchak, Halqa}, a flat List, no physics bodies. ToqnashuvHal(ref Vector2, float r) is a literal port: 3 passes with the surildi early-out (so two obstacles at a corner don't cancel), the annulus rule that the INTERIOR is solid and always ejects outward to r2+r (world.js:615-625 records the gate-oscillation bug this fixes), degenerate-centre escapes (m<1e-4 pushes -X for the ring, +X for a circle), and the rect inside-escape through the nearest edge. BinoTosiq prefers the generator's hand-authored footprint via Node3D.SetMeta('tosiqlar') — NOT a measured bbox, because those are playtest fixes (the bazaar's 2.4x2.0 platform instead of the canopy bbox is what keeps craftsmen reachable) — falling back to an axis-aligned AABB with Rotation.Y temporarily zeroed. The 1.0 m height gate is preserved and commented. REFUSED IN WRITING: move_and_slide world collision and Godot depenetration, which reproduce none of the above and would give apprentices and enemies collision they have never had. | new | M |
| Player movement, jump, arena clamp (player.js:206-345) | Player : CharacterBody3D. _PhysicsProcess is empty; GameDirector calls Player.Yangila(dt, dunyo) so tick order is explicit. Camera-relative direction using the web's exact basis; run at 7.0 when the run action is held OR analog kuch > 0.75, walk 5.0, mounted 10.0; analog magnitude clamped [0.35,1.0]. No acceleration, no friction, no velocity state — p.tezlikVec is declared and never read in the web, and nothing is added. Velocity = dir*tezlik; MoveAndSlide() with CollisionMask=0 so the engine cannot deviate; then clamp x[2,176] z[±47], then ToqnashuvHal at 0.45/0.6, then clamp again — the exact order of player.js:298-302. Y = YerBalandligi + Balandlik, the cosmetic jump: 4.6 against 9.8 gives apex 1.079 m, airtime 0.939 s, and affects nothing else (no collision change, no speed change, blocked while mounted). Body yaw lerps at 1-Exp(-14*dt). Footsteps 0.26/0.33/0.46. | new | M |
| Third-person camera + occlusion fade (player.js:176-194, 426-467) | Player.Kamera(cam, dt, teraklar) called LAST in GameDirector._PhysicsProcess, mirroring main.js:289. REFUSED: SpringArm3D — it pulls in on collision and the web camera never does. Fixed yaw-orbit offset in code: back 6.0/up 4.5/fov 70 on foot, 8.0/5.5/78 mounted. Position lerp 1-Pow(1-0.12,dt*60), FOV lerp Min(1,dt*3) applied only when \|Δ\|>0.1 — both idioms kept distinct in Smoothing.cs. LookAt player XZ at y+1.5. THE CAMERA HAS NO PITCH: p.pitch is written in four places and read nowhere. The field is kept and clamped so the mouse and touch code is identical, documented as inert, with _pitchYoqilgan=false as the one-line switch — adding pitch would change every combat sightline and the whole game is balanced on the fixed framing. Occlusion: IntersectRay from camera toward player with To = player - dir*1.2, up to 6 hits, against a layer containing ONLY poplar StaticBody3Ds (main.js:288 passes only dunyo.teraklar). Fade via MeshInstance3D.Transparency = 0.78, per-instance, never touching a shared material. FIDELITY NOTE: because terakYarat clones without materialniKlonla the web fades EVERY poplar at once whenever one occludes, so when the ray hits at least one tree the port fades all of dunyo.teraklar, behind _terakUmumiySoladi=true. | new | M |
| MARKING mechanic — belgilash, the single verb (apprentices.js:10-41, 151-197) | ApprenticeSystem.Belgila returns enum BelgiNatija {Yoq, NavbatTola, Ok} replacing the JS string return. Nearest unmarked non-depleted node within 3.0 m, squared compare. Queue cap = Shogirdlar.Count + 2 (4 at start, 8 at six); overflow plays Rad() plus a navbatTola toast and REFUSES — never replaces. Pillar is three MeshInstance3D: CylinderMesh 0.22/0.42 h8 rings10 with ShadingMode.Unshaded + BlendMode.Add + DisableFog + DepthDrawMode.Never (the exact equivalent of MeshBasicMaterial{AdditiveBlending, depthWrite:false, fog:false} — this triple is load-bearing or the pillar washes out at distance and the only signal the mechanic has is dead), albedo #ffb524 alpha 0.85; inner core 0.07/0.14 #fff0c0 alpha 0.95; ground TorusMesh 0.55-0.95 at y=0.09. Pulse alpha = 0.72 + Sin(Otgan*3)*0.22. BOTH input paths survive: keyboard 3.0 m, touch raycast marks anything within 30 m — the marking radius is genuinely input-dependent. | new | M |
| Apprentice AI — 5-state FSM, claim-on-idle, flee (apprentices.js:57-369) | ApprenticeSystem.Shogird on a plain Node3D — never CharacterBody3D, never a CollisionShape3D, never NavigationAgent3D (REFUSED in writing: travel times ARE the economy; a stone round trip is 42.3 s at the 1238 seed and a full wall is 24 stone = 12 trips ≈ 508 apprentice-seconds against 120 s of Act I, which is what makes recruiting mathematically mandatory). enum Holat {Bosh=0, Bormoqda, Yigmoqda, Tashimoqda, Qochmoqda} — Bosh MUST be 0 because the resume path (sh.oldingiHolat \|\| HOLAT.BOSH) at apprentices.js:307 works only by falsiness; pinned with a comment and a static assert. Yur() normalises XZ, steps 4.0*coef*dt, snaps Y, slerps yaw at 1-Pow(1e-6,dt). Three arrival tolerances kept per-state: 1.1 node, 2.8 store, 0.5 flee. Claim-on-idle writes Egallagan so two never race. Flee at 4.0 m runs exactly 5.0 s to a point 10 m away at 5.4 m/s, and the guard if(Holat != Qochmoqda) is preserved so the timer does NOT refresh — the 5-second oscillation is reproduced, not fixed. Yigmoqda downgrades to Bormoqda on resume so the 2.0 s gather restarts. Marks consumed PER TRIP. Apprentices take no damage anywhere — the flee is theatre by design. The byte-identical if/else at apprentices.js:268-273 collapses to one branch. The dead ikonka sprite (opacity 0, never referenced) is not ported; its absence is noted in the file header. | new | L |
| Shared storage (ombor) + conversion (apprentices.js:47, 281-291; game.js:253-386) | A single int[5] indexed by ResursTuri on the system object — no building entity, no capacity, no per-apprentice limit beyond the fixed 2 units. InventoryManager is REFUSED: a string-keyed, save-backed campaign catalog, and wiring it in would let a mid-run save resurrect a dead clock. Deposit is instant on arrival at (152,-22) within 2.8 m with a one-shot deliver clip. Three consumers, all squared-radius gated in GameDirector.OzaroTasir: wall 4 m, workshop 5 m, sword unlock 6 m. Crafting is hold-F with the anvil tick every 1/6 s via the same Floor(f*6) crossing test. NimaYasaladi keeps the fixed priority — sword absolutely first once the blueprint is open and iron>=3/coal>=1, then bow ONLY if the player lacks one or a soldier is unarmed, so the bow count self-caps at 1+8. The sword's absolute priority (game.js:340) means the forge will never offer a bow again after that point; reproduced and flagged in a comment as intentional-per-source. | new | M |
| Hiring craftsmen — hold E, per-target baseline (apprentices.js:371-414; world.js:312-347) | YollashTekshir returns a readonly record struct covering the JS 4-way union. YollashVaqti 11.0 from balance.js — the two code comments and index.html:108 saying 15 s are NOT ported and the controls screen is corrected to 11. Hire proximity 3.2 and the 0.15 s minimum hold from sehrli. The _yollanmoqda = {hunarmand, boshi: eUshlandi} snapshot is reproduced literally including the release case if(eUshlandi < boshi) boshi = 0, because it is the shipped fix for the chain-recruit bug. Six craftsmen in the 3x2 grid around (148,20): ax=(i%3)*4.2-4.2, az=floor(i/3)*4.6-2.3, seated y=0.28, facing -PI/2, each registering the 2.4x2.0 STALL footprint not the GLB canopy bbox. Cap 6 means only 4 of 6 are ever hireable; the other two stay scenery. FIDELITY DECISION, reversing data-first: player.js:353 gives E no else-reset while F has one, so a swallowed keyup leaves the hold accumulating. Godot has no lost-keyup failure mode, so the port reproduces the OBSERVABLE behaviour deliberately — NotificationApplicationFocusOut does NOT zero EUshlandi — behind _eHoldSurvivesFocusLoss = true. | new | M |
| Defensive wall — 6 sections, HP, build, destroy, unbounded rebuild (world.js:133-141; combat.js:7-74; game.js:253-283) | World.DevorNuqta at x=60, z=-33,-19.8,-6.6,6.6,19.8,33, each holding Qurilgan/Buzilgan/Hp/Mesh/Tosiq. Build: within 4 m of an unbuilt node with >=4 stone, hold F 3.0 s with the 6/s anvil tick, spend 4 tosh, +25 s via VaqtQosh, toast, tutorial 3→4. Obstacle registration keeps the measured-height gate s.Y < 1.0f ? null : TosiqQosh — with devor_seksiya.glb at ~0.3 m the built wall is NOT a player obstacle, only a visual plus an enemy-AI trigger plus a time bonus; the gate and its reason are commented so nobody 'fixes' it into a wall the player can no longer cross. Destroy at 0 hp swaps in rubble (GLB else 14 random boxes), removes the obstacle, emits devorBuzildi. Rebuild is UNBOUNDED — every rebuild pays another +25 s, so QurilganDevor can exceed 6 and the stats card can honestly read '8 / 6'. Coverage holes preserved: sections cover z[-39.6,+39.6] while enemies spawn to ±46, so edge spawns walk past untouched. | new | M |
| Clock as the only resource — countdown, +25 s, 720 cap, shift feel (game.js:121-146, 398-403; hud.js:54-71) | A single float O.Vaqt on GameDirector, decremented at the very TOP of _PhysicsProcess with both terminal checks immediately after, before any subsystem — the order of game.js:124-132 which I verified in source, so a wave can never spawn on the frame the run ends. Starts 600, loss at <=0, win at OlganDushman>=24. VaqtQosh does Vaqt = Min(720, Vaqt + s) and has EXACTLY ONE caller (wall completion), asserted by a test. TimeManager is REFUSED in writing: it counts UP at 0.02 days/s as a campaign calendar. The cap genuinely binds — 6x25=150 s of bonus against 120 s of headroom — so the last two sections are worth less if built late. Acts and waves key off the clock VALUE, so the clock jumping backward literally rewinds the schedule; no second time source exists anywhere. The shift beat: clock AnimationPlayer keyframe 0.55 s peaking scale 1.22 at 35%, a white full-screen ColorRect flash, and the rising arpeggio [523,659,784,1046] at 85 ms. | new | S |
| Wave table + spawn scheduling + enemy types (balance.js:74-81; game.js:513-539; combat.js:77-152) | Three TolqinData rows keyed by clock VALUE with a _Ready assertion that the cohorts sum to JamiDushman, because victory is a counter and a mismatch makes it unreachable or premature. TolqinYangila walks TolqinIndeks forward on Vaqt <= t.Vaqt, clears the warning flag, puts Humo into Uchdi, calls TolqinChiqar which spawns piyoda then otliq then noyon. Spawn x = 2 + GD.Randf()*12 (deliberately unseeded, matching Math.random), z = -38 + (i/(n-1))*76 ±4 clamped to ±46. Materials duplicated per enemy so a hit tint affects one body. Mounted enemies with no GLB get a separate horse Node3D transform-copied like the player's, and the rider skips terrain snapping. Corpses: olimTaymer 2.5 s, procedural fall rotating X at 4 rad/s and sinking at 0.6 m/s to y=0.2. Humo raises wings 15.0 s before each wave (clock 495/345/165). Victory is the kill COUNTER, not 'no enemies remain' — a leaker that reaches the gate is never despawned or counted and permanently blocks the win; reproduced as shipped. | new | M |
| Enemy AI — wall priority, target selection, attack (combat.js:218-310) | A flat three-priority inline decision per enemy per tick in DushmanYangila: no state machine, no steering, no separation, so a wave stacks into a column exactly as shipped. (1) Wall block is a pure COORDINATE-BOX test — p.X<62.5 && \|n.Z-p.Z\|<6.6 && n.X-4<p.X<n.X+2.5 — which is precisely why NavigationAgent3D is REFUSED in writing: pathfinding routes enemies around the wall line and deletes both the wall's tactical meaning and the +25 s economy. Wall damage per swing = (140/6.0)*hujum = 28.0 piyoda, 35.0 noyon, per single attacker, so six flatten a section in one second. Mounted enemies instead FLANK: sideways at 0.85x, forward at 0.25x, clamped \|z\|<=44, unconditionally bypassing the wall. (2) Else attack a live player or soldier within 2.8 m (noyon) / 2.2 m on the hujum interval, raising a C# event — the enemy never writes HP. (3) Else walk the straight normalised vector toward the nearest target within 40 m, else toward (172,0). Yaw lerp 1-Pow(1e-6,dt). Uncached rescan of 8 soldiers + player every tick. Enemy.cs is REFUSED: 100 hp, detect 17/attack 11/fireInterval 2.4, no wall rule, no mounted variant. | new | L |
| Three weapons — club, bow, sword: damage, cooldown, cone, homing arrows (player.js:480-520; game.js:148-160; combat.js:406-423) | Player.Urish stays a PURE QUERY returning the hit list — it starts the cooldown and swing pose and applies NO damage; GameDirector applies it, exactly as the web splits ownership. WeaponController is REFUSED: 724 lines of hitscan with magazines, reload, spread, scope and Nathan-specific bone names in [Export] defaults where FindBone returns -1 and every consumer silently early-returns. Melee is a code cone, not an Area3D: squared distance against 2.0/2.5, then \|AngleDifference(Atan2(dx,dz), mesh.Rotation.Y)\| < 1.22 — every enemy in the ±70° cone takes FULL damage, no falloff, no per-target cap, and the cone is tested against BODY yaw which lags the camera through the 14/s lerp, so the arc trails the crosshair. Damage lands on the input frame; the visible 0.34 s swing plays afterwards. Kaltak 8/0.60, Qilich 25/0.45. Bow 18/0.90, auto-targets the nearest live enemy within 32 m and ±0.40 rad, and CONSUMES the shot with no target (cooldown + animation, no arrow). Arrows are HOMING and cannot miss: step toward the target's CURRENT position at 42 m/s in XZ only, Y frozen at spawn (player y+1.45, soldier 1.5), damage at planar distance < 1.0, lifetime 2.2 s. No RigidBody3D — a ballistic projectile would silently nerf both the player's bow and all eight archers. The swing pose is an AnimationNodeOneShot in Add mode layered on locomotion, which is the branch the web takes whenever a mixer exists. | new | L |
| Shield blocking and player HP (game.js:548-586; player.js:168, 318) | Right mouse (new Input Map action buxoro_blok) or the touch Bl button. Blocking is omnidirectional and free — no stamina, no cooldown, no facing check, no guard break — and applies ONLY inside the maqsad==oyinchi branch of the dushmanUrdi handler, multiplying incoming melee by 0.30. Melee-only by construction, since no enemy fires projectiles at the player. ShomurodHp 140, no regeneration, no respawn. PlayerHealth.cs is REFUSED in writing: 7 hp/s regen after 4 s, 2 s spawn grace and a FindChild('Spawn') teleport-respawn would make a 10-minute run unlosable. HP lives as a float on Player and changes in exactly one place, preserving the web's single damage seam. Hit feedback is a 90 ms full-screen inset vignette rgba(160,30,20,.75) with a 0.45 s fade — a ColorRect with an inset shader driven by a Tween. Equipping the bow force-clears the shield (two-handed). | new | S |
| Allied soldiers — spawn, arming, three behaviour modes (combat.js:154-200, 313-400) | 8 soldiers, 60 hp, at x=158, z=-21+i*6, facing west. Behaviour branches entirely off enum AskarQurol {Yoq, Kamon, Qilich}. Qurolsiz {3, 1.6, 2.2} resists but never moves. Kamonchi {12, 1.2, 25} rooted, fires the same homing arrows. Qilichboz {20, 0.8} charges at 5.2 m/s to a 2.0 m gap with yaw lerp 1-Pow(0.001,dt) and NO LEASH (engM starts at Infinity), chasing the last living enemy across all 180 m and never returning home — reproduced with a comment. AskarniQurollantir hard-refuses anyone not currently Yoq, so loadouts are permanent and one-way; arming also visually adds leather armour and lifts the unarmed slump. Damage TO soldiers currently duplicates the ragdoll logic inline in game.js:561-571; the port routes both enemy and soldier damage through one ZararBer, which changes no number and removes the duplication. Dead soldier meshes are never removed, as today. | new | M |
| Horse mounting and riding (game.js:502-509, 210-222; player.js:306-312, 570) | Vehicle.cs is REFUSED in writing: Enter hides the driver, disables its ProcessMode and CollisionShape and swaps to a second camera rig via hardcoded NodePaths into the player scene — none of which the web does. Player.OtMin sets two fields; the riding branch writes the horse Node3D's GlobalPosition from the player's XZ at ground level, the rider at ground+1.42, and copies the yaw. A transform copy, nothing parented. Mount at EUshlandi>0.7 within squared distance 9. The horse spawns at Act III at TarobiyJoyi+(2.5,1.5)=(110.5,2.5) running horse_idle, swapped to horse_mounted_idle; the rider plays ride_idle/ride_run. Hoof sound every 0.26 s. There is NO dismount anywhere in the source, so there is none here: Otda stays true for the run, permanently blocking the jump, holding the 0.6 m radius and the 78° FOV. The horse is never collision-registered. The one-shot otMaslahat toast fires on mount and branches on whether any wall section is still unbuilt. | new | S |
| Three-act state machine — parda I/II/III (game.js:406-510) | GameDirector.PardaYangila is a flat if-chain over int O.Parda driven purely by clock value, with Parda2() and Parda3() as explicit one-way methods guarded by the act int — NOT signals, because both can legitimately fire on the SAME first frame when the parda3 preset starts at 240. Act I→II at v<=480: cue toshTugadi, Chizma.Kamon=Ochiq, viewer 7 s, KorinadiganResurs swapped to [yogoch,teri], t2a then t2b 4.2 s later. Act II→III at v<=240: the Tarobiy death beat. Act III has no fourth act, only the self-service sword unlock: when Chizma.Qilich==Olik && OtMinilgan and the player is within 6 m of the forge, the dead slot flips to Ochiq by itself, the viewer pops 9 s, HUD swaps to [temir,komir]. The act-480 / wave-480 coincidence is deliberate — the bow unlocks the instant the first Mongols appear — and is preserved by keeping both on the same clock. | new | M |
| Tutorial step ladder (game.js:410-431) | O.Tutorial struct {Qadam, TurganVaqt, OxirgiPoz} — no UI layer, every step is a Tarobiy line or a nudge. 0→1 at Otgan>1.5 fires t1a/t1b/t1c at 0/4200/9400 ms for 4/5/4 s. 1→2 on the first Belgila returning Ok. Step 2 is the idle-nag: diff position each tick, at >3 s shout 'Yugur!' plus the pinned hint, then set TurganVaqt=-6 as a 6 s cooldown. 2→3 at Ombor[Tosh]>=4. 3→4 on the first completed wall. Step 4 is terminal and read by nothing, reproduced as such. FIDELITY DECISION: game.js:424 compares per-frame distance against 0.1*60*dt — a 6 m/s threshold against a 5.0 walk speed, so the game shouts 'Run!' at a walking player. Reproduced, written as an explicit speed comparison against the named constant TurishChegi=6.0 so the intent is visible and one number changes it. | new | S |
| Tarobiy — dialogue table, light pillar, death at 4:00 (game.js:7-64, 97-118, 473-510) | Stands at (108,1) facing -PI/2 with a translucent white pillar (cylinder 0.22/0.38 x 9) pulsing 0.16 + Sin(Otgan*1.6)*0.09 while he lives. Lines in order t1a/t1b/t1c, the one-shot bozor line gated on !BozorGapi && v<560, then t2a/t2b. Death at v<=240 is the literal six beats: (1) cue tarobiy; (2) death_kneel once-forced with the procedural fallback — in Godot the guard is AnimationPlayer.HasAnimation, else stop the animator and lay the mesh at Rotation.X=PI/2.1, y=0.28 so the corpse stays on the field; (3) pillar off, death sting; (4) BOTH blueprint slots blacken, kamon Qulf→Olik and qilich→Olik; (5) BuxoroAudio.Musiqa(false) over 1.2 s, PERMANENT, with the pause menu carrying the same explicit carve-out main.js:193-196 has, or the act's tonal turn is undone; (6) spawn the riderless horse at +2.5x/+1.5z. NARRATIVE TIMING: the web fires t1b/t1c/t2b on wall-clock setTimeout, so lines play behind a fullscreen cutscene and over the pause overlay; the port reproduces that by scheduling them on a real-time timer, not the game clock, guarded only by if(!O.Tugadi). | new | M |
| Humo bird — the wave siren that replaces a HUD banner (humo.js:1-119; game.js:522-538) | Humo.cs on one Node3D, enum HumoHolat {Qongan, Ogoh, Uchdi, Ketdi} with a clip per state. Qongan lerps home to the Ark dome facing PI. Ogoh circles the dome at r=16 climbing toward 14 m, angular 0.55, flap 0.75 @ 3.2 Hz. Uchdi is the wide circle centred x=95, radii 62/34, climbing toward 46 m, angular 0.32, flap 0.5 @ 2.1 Hz. Ketdi flies east and up at 26/7 m/s, hidden past x>460, never returns. HolatBer owns takeoffTaymer 1.2 s → fly_loop and landTaymer 1.6 s → perched_idle and fires the bird cry on takeoff and departure. Scaled 3.2x so it reads from 250 m; unshaded emissive with DisableFog — an explicit 'this is UI, not scenery' decision. Procedural sin-driven wing fallback kept, because no humo.glb ships. Re-landing gated on TolqinIndeks<3, including the early-return path that still re-lands once all waves are exhausted. The 15 s warning also drives the fog: 0.0075→0.0115 lerped at dt*0.6. NO wave counter, NO objective marker, NO banner is added anywhere — the bird and the haze ARE the warning UI, and ObjectiveMarker.cs is explicitly not instantiated in this episode. | new | M |
| Blueprint panel + non-modal 3D corner viewer (chizma.js:1-146; hud.js:95-114) | Two pieces. HUD strip: three enum ChizmaHolat {Ochiq, Qulf, Olik} slots as numbered cells with the held weapon getting .faol — Olik is the narrative beat fired at Tarobiy's death, not styling. Viewer: SubViewport + SubViewportContainer at 30% screen width, 0.75 aspect, left edge 8.5%, vertically centred, with its own Camera3D (FOV 38, near 0.05, far 20, z=2.4) and two DirectionalLight3Ds — Godot-native, replacing the scissor/autoClear/clearDepth juggling and the CSS-px-vs-device-px trap entirely. Object auto-centred and fit with Scale = 1.35/longestAxis, given the hand-tuned per-item BURILISH rotation, spinning 0.75 rad/s with the sine wobble ±0.13. Fade-in dt*2.2, skip render below 0.02. THE CONTRACT THAT MUST SURVIVE: GetTree().Paused stays FALSE — the player keeps running while the sword rotates in the corner, which is how 'No one gave you this drawing' lands. Durations bow 7 s, sword 9 s. MALUMOT, currently Uzbek-only with no en branch, folds into the same translation table. | new | M |
| Crafting flow at the workshop (game.js:285-302, 336-373) | Proximity 5 m of (152,-22) and not standing on a wall node, plus hold-F. Workshop.cs/WorkshopPanel are REFUSED: a tree-pausing standing-order queue with a progress bar and an IsBusy gate, versus a non-pausing proximity hold with a fixed priority and no panel at all. NimaYasaladi returns {Yoq, Devor, Kamon, Qilich}. Prompt composed as Tr('yasa') + ' — ' + Tr(nima) → 'F — YASA — KAMON'. The hold fills the HUD bar at FUshlandi/kerak with the anvil every 1/6 s so it has an audible rhythm. YasashVaqti 3.0 wall and bow, 4.0 sword. Routing is the progression fantasy: the FIRST bow always to Shomurod with the toast 'kamon sizda — 2 bilan tanlang', every later bow to the first unarmed living soldier. A sword does NOT auto-resolve. | new | M |
| Sword choice — self vs soldier (game.js:304-317, 367-386) | A mode FLAG, not a dialog. Finishing a sword sets QilichTanlov=true and deducts immediately. While up, OzaroTasir REBINDS the number keys — the JS is an else-branch so weapon switching is genuinely unavailable during the decision; in Godot that is an input-context swap inside OzaroTasir, not an added handler. Prompt '1 — o'zimga · 2 — askarga'. Self force-equips; soldier arms the first unarmed living one (25 dmg in your hands vs a 20-dmg ally who can die). Either path increments YasalganQurol, fires cue qilichYasaldi (clips 5 then 7) and toasts. Re-offered every time 3 iron + 1 coal accumulates — the field holds 12 iron and 4 coal, so up to 4 swords — and produces no different ending text, exactly as today. | new | S |
| Tip system (otMaslahat) + contextual prompt labels (game.js:216-222, 242-245, 388-395) | Three Act III tips through Hud.Gap('', text, seconds) with an EMPTY speaker so they read as system voice — that convention is ported explicitly, because the same banner carries both character lines (bold name) and system tips (no name), and the distinction carries the Act III theme that nobody tells you any more. Mount tip 8 s branching on whether any wall node is unbuilt; first devorBuzildi tip 6 s, one-shot flagged. The {n} token is substituted at runtime from DevorBonus so the tip stays truthful if retuned — the token stays in the translation string, never a baked '25'. Resource prompt built as Tr('belgila') + ' — ' + Tr(tur).ToUpper() → 'E — BELGILA — TOSH', deliberately overridden by any higher-priority prompt. | new | S |
| HUD DOM overlay — clock, hands, blueprints, resources, HP, prompt, hold bar, dialogue, toasts (hud.js:36-182) | BuxoroHud : CanvasLayer with 13 Control children, written in ChronoShift's proven five-band layout (exported NodePaths, cached typed fields, _Ready resolve + group join + subscribe, a tick of one-line Update calls, handlers at the bottom) but with NONE of HUD.cs reused — it is 846 lines of money/XP/ammo/vehicle-hull on a different art direction. Bars are plain Controls with AnchorRight set to a 0..1 fraction, matching repo convention, not ProgressBars. The three DOM dirty-check caches (oxirgiSoat, oxirgiResurslar, oxirgiJon) are deleted — web-only optimisations, not design. THE HUD PHILOSOPHY IS THE THING TO KEEP: only the clock and the six apprentice-hand dots are permanently on screen; everything else appears only when actionable and fades. Clock redraws on integer-second change, .xavf pulse at <=60 s. Hand dots = 6. Toast 2000 ms + 1.5 s fade, max 4. Dialogue default 5 s. HP red at 42. Resource readout shows ONLY KorinadiganResurs, REPLACED wholesale per act, so stone becomes invisible from Act II — reads as a counter bug, is reproduced, and the tension is noted because the Act III tip tells the player to spend a resource the HUD no longer shows. All vmin sizing maps onto the existing canvas_items/keep stretch at 1920x1080. | new | L |
| Floating ally HP bars — 3D→screen projection (game.js:183-198; hud.js:150-168) | Camera3D.UnprojectPosition on a point 2.2 m above each soldier's mesh, replacing the manual Vector3.project + NDC→px math and the innerWidth-vs-renderer.getSize mismatch. Same design, same numbers: ONLY allies get bars (enemy HP is deliberately invisible and stays invisible — this is the playtest fix that made 'which soldier still needs a weapon' legible), only within 12 m, red at ratio <=0.3, bar 6vmin x 0.7vmin, culled behind the camera or beyond \|ndc\|>1.05. A pooled list of Controls grown on demand, mirroring HUD.jonlar's div pool. | new | S |
| UZ/EN runtime language toggle — four scattered dictionaries (hud.js, game.js, menyu.js, main.js, chizma.js) | All four dictionaries plus MALUMOT collapse into ONE buxoro.csv (keys,uz,en,ru-empty) imported to .translation files and registered in project.godot. The corner toggle calls TranslationServer.SetLocale and flips its label between 'UZ / en' and 'uz / EN', re-rendering the blueprint panel and HP label. FIDELITY DECISION, reversing data-first: the web resolves language at call time via GAP[H.til()][k], so a dialogue line already on screen keeps its old language until it expires. The port reproduces that — HudDialogue caches the resolved string at show time and deliberately ignores NOTIFICATION_TRANSLATION_CHANGED — rather than 'improving' it to live retranslation. This also fixes the two coverage holes by construction: chizma MALUMOT and the pause menu's inline ternaries become normal keys. | new | M |
| Diegetic (in-world) feedback layer (apprentices.js:194; humo.js; game.js:105-179, 575; hud.js:66) | Every piece survives; only delivery changes. Mark beam and Humo have their own mappings. Tarobiy's pillar pulses 0.16 + Sin(Otgan*1.6)*0.09 and switches off the instant he dies. Fog density lerps 0.0075→0.0115 at dt*0.6 as a wave approaches — the only 'something is coming' cue besides the bird. Damage vignette: ColorRect with an inset shader, 18vmin rgba(160,30,20,.75), 90 ms hold then 450 ms fade, driven by a Tween. Time-gain: white full-screen ColorRect flash plus an AnimationPlayer on the clock label reproducing the CSS keyframe (0.55 s, peak scale 1.22 at 35%) — the void el.offsetWidth reflow hack becomes anim.Stop(); anim.Play(). THE RULE WRITTEN INTO THE PORT DOC: the screen shows only a clock and six dots, so the bird, the beam, the light column and the haze ARE the UI. A port that quietly adds a wave counter or an objective marker has broken the design even though nothing looks wrong. | new | M |
| Procedural WebAudio — 15 SFX, wind bed, 13-note melody, zero sound files (audio.js:1-175) | THE PRIMARY PATH IS LIVE SYNTHESIS, preserving the literal zero-audio-files property. WavSynth renders each SfxRecipeData ONCE at _Ready into an in-memory AudioStreamWAV: an oscillator (tri/square/sine/saw) with optional exponential ramp to Oxirgi, or a slice of one 2 s white-noise buffer generated once, through a biquad (bandpass/lowpass at Chastota, given Q), multiplied by the gain envelope Hajm → 0.0001 exponential over Uzunlik. The web's zarb() parameters ARE the spec, so this is ~60 lines. Played through the existing Sfx.PlayAt/PlayUi helpers. WIND is live, never baked: one 20 s noise loop on the Ambient bus through AudioEffectBandPassFilter whose CutoffHz is written every tick as 420 + Sin(t*0.09*Tau)*190, because baking loses the breathing. MUSIC is scheduled note-by-note, not a baked loop, because the victory reprise plays at tezlik 1.35 (slower) and PitchScale on a baked loop would transpose it — wrong for a time-rewind game. Fade-in 2.5 s to 0.30, death fade 1.2 s then PERMANENTLY off. default_bus_layout.tres with Master/Music/SFX/Ambient is a prerequisite — verified, the project has none. Every gain through Mathf.LinearToDb. The user-gesture AudioContext gate disappears; the Act III music-death beat becomes an explicit Musiqa(false) call, which is where it should have been. | extend | L |
| Animation dispatch layer — soft-fail oynat, clip lock, speed match, procedural fallback (anim.js:1-203) | Anim.cs keeps the SAME public shape because gameplay branches on it: combat.js:470 picks ragdoll vs die-clip from the boolean and harakatAnim checks bormi('charge'). In Godot capability is known at load, so AnimKontrol caches a per-actor HashSet from the ActorClipSet resource at spawn and Oynat returns membership — the boolean contract survives, the guesswork dies, and a missing clip pushes a loud error instead of degrading silently. AnimationTree + StateMachine, xfade 0.18 / 0.06 attacks / 0.05 jump. Qulf one-shot lock becomes AnimationNodeOneShot. Yurish keeps BOTH modes: ordered (first available wins — the player, so Shift visibly means run) and nearest-natural-speed by log-distance (enemies, so a 4.5 m/s Mongol picks charge over walk_menace), the latter becoming a BlendSpace1D keyed at 0 / 1.21 / 4.33 m/s. AnimationNodeTimeScale carries the [0.55, 2.4] clamp — KEEP IT: walk is authored for 1.2 m/s while the game walks at 5.0, a true 4x match 'turns the legs into a wheel', and the foot slide is an art call recorded at anim.js:90-96, not a bug. Per-instance Advance(randf) plus 0.93+rand*0.14 timescale jitter de-syncs the 6 enemies of a wave. ProtseduralYurish (styles shogird 9.5/0.055/0.13/0.05, yurish 6.5/0.040/0.06/0.04, ogir 4.2/0.065/0.04/0.08, blend dt*7) is hand-ported and KEPT as first-class shipping content — klip-xaritasi.json names only 14 clips across 6 models, so this is what players actually saw. | extend | L |
| GLB loader / model registry with silent procedural degradation (models.js:1-428) | Models.cs keeps the registry and the MODELS.ol(x) ?? procedural(x) contract — every call site keeps that shape, because it is why the game shipped at all and 18 of the 35 expected names have no GLB. What moves OUT of runtime and INTO import: the MOLJAL rescale, the foot-drop to bbox.min.y=0, DoubleSide for the 12 IKKI_TOMON models, and sRGB. These become one EditorScenePostImport script driven by moljal.tres. Hand attachment: the six-way regex and the synthesized __qol_R anchor both collapse to BoneAttachment3D with BoneName R_Hand/L_Hand — which is what all six Tripo characters actually have, contradicting MANIFEST.md's claim of HAND_R/HAND_L empties that exist in no shipped file. Models._Ready hard-fails loudly (PushError plus an on-screen debug warning) on a -1 bone or missing clip rather than early-returning — the WeaponController failure mode. DistanceCull and ModelDresser are reused verbatim. | extend | M |
| Procedural placeholder actors (actors.js:1-231) | Actors.cs ports Gumanoid(h) — the parameterised box figure scaled off a 1.75 m reference, returning the named joints {ChapOyoq, OngOyoq, ChapQol, OngQol, Tana, Bosh} that gameplay code rotates directly for walk cycles, gather wobble and block poses. Seven specialisations layer headgear (doppi/salla/dubulga/qalpoq), armour slabs, red eye quads, Tarobiy's torus + translucent-circle sieve shield, the apprentice basket, the 4-legged box horse, and the Humo bird with named wing groups. Scheduled as CONTENT, not scaffolding: shogird, mogul_otliq, humo and all four weapons have no GLB, so this is what players see for those actors. The RANG palette and the 6-colour apprentice roster (0x3a4a6b, 0x6b4a30, 0x8a8578, 0x5f6b40, 0x2e2c2a, 0xd8cdb2) move into palette_buxoro.tres because the art doc's muted-earth rule lives there and the GLB tinting MULTIPLIES against that same dusty ramp. | new | M |
| Procedural architecture — 7 generators + battered-wall geometry (binolar.js:1-465; world.js:246-491) | Binolar.cs ports all seven generators (gate, wall segment, Ark citadel, house small/large, burned ruin, rammed-earth wall, bazaar stall) plus QiyaBlok() — the battered-wall geometry tapering a box from wide base to narrow top, the signature silhouette of every fortification here — as an ArrayMesh via SurfaceTool. LCG seed 8317, separate from world's 1238 and grass's 4711. Materials cached by colour+roughness. Palette gisht #9a8562 (explicitly dust-grey, NOT red), suvoq #a5926e, kuyik #3a332a. Gate: two 11 m battered towers at z=±4.6, 8.6 m centre block, 2.0 m arch, leaves as named nodes so the timeout ending can swing them ±1.1 rad. Ark: 30 m battered platform 74x62, 12 buttresses, dome at (-2, P+0.9, 7) where Humo perches. Each generator writes its hand-authored tosiqlar footprint as node metadata, NOT the bbox. City wall: ring R=46, tangent rotation Atan2(-Cos t, -Sin t), segments 3/4/11/17 pre-ruined, 12 a hole, side coefficient 1.30, and the ±0.30π east arc never built. The ring becomes a MultiMeshInstance3D per segment variant, which removes the draw-call reason for that skip while KEEPING the skip itself, since the missing arc is now visible design (18-year-old war damage), not an optimisation. | new | L |
| Instanced grass / ground cover (otlar.js:1-126) | Four MultiMeshInstance3D groups totalling 1670: ot_past 800 @0.8-1.5 #6b6a56, ot_tutam 600 @0.7-1.4 #77704f, butta 120 @0.8-1.3 #5f5e4c, gul_sariq 150 @0.9-1.2 #8f7f3e. LCG seed 4711, attempts capped at 6x count, rejection-sampled away from the wall line ±3.5, the forge r²<60, the bazaar r²<90 and the gate road. Y from YerBalandligi, yaw and non-uniform scale randomised. Shadow casting off, receive on, frustum culling off. NOT decoration and the density must not be trimmed: the file header states the purpose is speed perception — on an empty field you cannot feel running, and near tufts streaming past against far ones crawling is the motion cue for 'the player should always be running'. | new | S |
| Procedural weapons — club, sieve-shield, bow, sword, spear, arrow (qurollar.js:1-250) | Qurollar.cs ports all six as ArrayMesh builders, including the bow's CatmullRom reflex curve swept as a tube with a separate horn/sinew layer (a Curve3D sample in Godot). Contains ZERO gameplay numbers — pure geometry used only when Models.Ol returns null, and no balance value is read out of it. On the shipping path rather than a fallback, because all four weapon GLBs are among the 18 missing and the blueprint viewer needs a mesh to rotate on the frame the bow unlocks. Attachment via BoneAttachment3D on R_Hand/L_Hand with the per-weapon grip offsets from the call sites. | new | S |
| Cutscene video system — 7 cues, 2 modes, skip semantics (videolar.js:1-147) | Videolar.cs on a CanvasLayer with a VideoStreamPlayer and the cue table as SahnaData rows. Both modes implemented, including burchakda (corner, muted, Toxtatadimi() returns false, game keeps running) even though zero shipped cues use it — it is fully implemented in the source and the mandate forbids dropping it. One-shot dedupe by cue name via a Korilgan HashSet. Sabab() still returns false and DROPS a second cue while one is playing — reproduced, but with an explicit GD.PushWarning naming the dropped cue, because the parda3 preset silently loses video 6 and that should be visible rather than discovered. Skip model preserved exactly: the skip button abandons the ENTIRE remaining sequence, click/Space/Enter advance one clip, Escape is a full skip, handled in _Input with SetInputAsHandled so gameplay keys are swallowed. The n/total counter is kept. Freeze contract: Toxtatadimi() \|\| Pauza short-circuits GameDirector's simulation while rendering continues. The muted-autoplay retry chain is browser policy and does not exist in Godot. | new | M |
| Endings, death sequence, camera epilogue, stats card (game.js:548-671) | Three terminal states funnelled through Yakunla(TugashTuri) — each a small coroutine touching four subsystems, not an if-chain. Galaba: Humo back to Qongan, music restarts at rate 1.35, wind 0.10. Olim: play the die clip, death sting, music off, wind 0.30, Humo leaves forever. Maglubiyat: Humo leaves, wind 0.28, and the city gate SWINGS OPEN (±1.1 rad) — the Mongols are inside. YakunYangila runs the camera epilogue: on death the camera hangs low on the body (offset -3.5x/+3z, rising at most 3 m); otherwise it climbs toward +26 m and looks at the Ark dome. COMBAT KEEPS RUNNING with the player marked dead — 'Jang davom etadi — dunyo unga qaramaydi' — and that deliberate cruelty is preserved. At t>3.5 s the ending video plays, then the stats card: walls x/6, weapons made, apprentices x/6, enemies x/24, and time remaining on a win. Headlines QUSH QAYTDI / QUSH QAYTMADI / USTA YIQILDI with their two body lines each. Two source quirks reproduced and left switchable: the apprentice count prints Shogirdlar.Count including the 2 starters (a no-recruit run honestly reports '2 / 6'), and Ceil(Vaqt % 60) can yield 60 and print '4:60'. Restart is a reload of BuxoroEpisode.tscn (the web does location.reload()). | new | M |
| Series catalog + two-level menu + mission presets + pause (menyu.js:1-203; main.js:58-249) | Menyu.cs. OUTER level is the CHRONOSHIFT catalog styled as a blueprint sheet (blue paper, grid, monospace, compass rose, title block LOYIHA/QISM/DAVR/VARAQ, 'CHIZMA № 001 — SERIYA KATALOGI') built data-driven from SeriyaQatorData rows so episode 002 is a row insert — which is the whole reason this menu matters to the wider ChronoShift plan, since the web menu already declares Buxoro as 001 and reserves two slots. Five rows, two locked. INNER level is Buxoro's own menu under the tagline. Four MissiyaData presets applied at run start — toliq 600/2, parda1 600/2, parda2 480/4, parda3 240/6, clamped to 6 — kept because they are the only fast path to Acts II/III and therefore the QA path for every later step. Background is kirish.ogv muted and looped. Pause menu (DAVOM ETTIRISH / QAYTA BOSHLASH / BOSHQARUV / SERIYA MENYUSI / CHIQISH) on ui_cancel and the ❚❚ button, carrying the music carve-out. Pointer-lock-loss-as-pause is replaced by window focus loss. The 6.0 s opening flythrough (easeInOutQuad from humoTayanchi + (-26,8,6) to 6 m behind and 4.5 m above the player) runs CONCURRENTLY with videos 1-3 exactly as main.js:280 does. | new | M |
| Touch controls — input synthesizer + adaptive button HUD (touch.js:1-201) | Touch.cs writes the IDENTICAL fields keyboard/mouse writes on Player (Tugmalar, EBosildi, Urmoqchi, Bloklaymi, Yonalish, Analog{X,Z,Kuch}) so NO gameplay code branches on platform — that architecture is why one build ships to web, Electron and Android, and it is preserved; a grep for a platform check in the Buxoro scripts must find none. InputEventScreenTouch/Drag replace the raw listeners; the passive:false/preventDefault noise disappears. Left 42% spawns a floating joystick wherever you touch (radius 52 px, magnitude clamped [0.35,1.0], run at kuch>0.75); right side is a look-drag at 0.006 rad/px yaw. A release under 280 ms with under 14 px travel is a TAP that raycasts: enemy → attack, resource node within 30 m → mark it DIRECTLY bypassing the 3 m rule, else a plain E press. Five TouchScreenButtons shown per frame from SensorHolat: E with a contextual glyph (🐎/✋/◈), F only when the recipe is affordable, attack/block only within 14 m of an enemy, block never with the bow, weapon-cycle only at 2+ weapons — the HUD philosophy applied to input. body.sensorli reflowing the HUD upward becomes a MarginContainer offset. FIDELITY DECISION, reversing data-first: touch.js:83 sets spaceBosildi on EVERY touchstart so the 6 s opening camera move can never play on a touch device; reproduced behind _sensorOchilishniOtkazadi = true rather than fixed. | new | M |
| Frame loop, dt clamp, pause, opening flythrough (main.js:1-405) | BuxoroMain.cs is thin because GameDirector owns the order. Sets Engine.PhysicsTicksPerSecond = 60, max_physics_steps_per_frame = 3 and physics_interpolation = true; owns the 6.0 s flythrough, the pause overlay, and the freeze branch if(Videolar.Toxtatadimi() \|\| Pauza) { sim halted, rendering continues }. The dt clamp Min(dt, 1/20) is carried and, thanks to the 3-step cap, is a LIVE slow-motion floor reproducing the web's backgrounded-tab behaviour rather than dead insurance. Anim.Yangila runs FIRST each tick exactly as main.js:277 does, BEFORE the sim — because ZarbaPozasi writes shoulder/elbow rotations AFTER the animator has written the frame and relies on that ordering (player.js:83-86). In Godot that ordering is enforced by AnimationTree.ProcessCallback = Manual with the advance called from GameDirector, so it cannot break silently the way a scene-child reorder breaks WeaponController today. Web-only concerns deleted with a note: iframe scroll/contextmenu suppression, the file:// CORS warning path, the muted-autoplay chain. | new | M |
| Asset intake — 17 Draco GLBs Godot 4.7 hard-rejects, plus scale, facing, double-sidedness, clip names, loop modes, root motion | A prerequisite build step, not runtime code. npx @gltf-transform/cli dedup decodes Draco in place (3.66 MB → ~8 MB, verified to import with zero errors and webp textures intact, since EXT_texture_webp IS supported). Preferred: re-run Torobiy's own tools/optimize-models.js from _xom/ with --compress false, keeping the per-model simplify ratios. The 991 MB raw _xom/ set is NEVER imported directly (1.9M tris each against an 8k budget). Then ONE EditorScenePostImport does the rest from data. SCALE: MOLJAL is the real spec — every Tripo model is unit-normalised (shomurod measures 0.975, not the 1.75 m MANIFEST.md claims), so shomurod is 1.826x; without this everything spawns at ~55%. FACING: the web yaws with atan2(dx,dz) (+Z forward), Godot is -Z, so every character gets 180° on a BodyOrient CHILD node, never on the model, and never on hunarmand.glb or tarobiy.glb whose extra roots already carry a -90° X quaternion that would double-rotate. DOUBLE-SIDED: CullMode.Disabled on the 12 IKKI_TOMON models or walls vanish from inside. CLIP NAMES: Godot sanitises NlaTrack.001 → NlaTrack_001 and tripo::Root → tripo__Root, so klip_xaritasi.tres stores SANITISED keys — a direct port of the dotted JSON matches nothing and fails silently. LOOP MODE: every clip imports LOOP_NONE, so walk/run play once and freeze; the post-import sets LoopMode.Linear on locomotion and leaves die/fall/land/takeoff alone. ROOT MOTION: baked on the Hip bone's LOCAL Y (Tripo bones are Z-up) — walk -1.5700 / 2.375 s and run -3.0654 / 1.2917 s, which at 1.826x is 1.21 and 4.33 m/s. RootMotionTrack = 'Armature/Skeleton3D:Hip' removes the drift for free, replacing all 60 lines of ildizSiljishi; the motion is IGNORED because game code owns position, exactly as the web intends. Those two speeds are the BlendSpace1D points. ONE BoneMap against SkeletonProfileHumanoid authored once for the shared 41-bone rig (watch tarobiy's 42nd neutral_bone) opens Mixamo retargeting for the ~20 missing clips. | new | M |
| Cutscene media — 12 h264 mp4 vs Godot's Theora-only VideoStreamPlayer | Transcode from assets/video/_xom/ (the 145 MB originals), never the shipped 17.3 MB web copies — those are already squeezed to ~1 Mbps and Theora over them stacks generation loss on exactly the dust-and-smoke content where Theora rings worst. libtheora q:v 7 + libvorbis q:a 4 for clips 1-9 and both game-over files; kirish.mp4 gets -an q:v 6 (it has no audio stream and is played muted). Expect 17.3 MB → 30-37 MB at 720p; do not go below q:v 6, drop to 960x540 q:v 7 if size matters. BLOCKER, verified: this Mac's ffmpeg is Theora DECODE-only, so a theora-enabled build is a hard gate, moved into step 1's user actions. One piece of luck: all 7 shipped cues are fullscreen-and-pause, so the port only ever decodes Theora while the sim is frozen and Godot's seek and audio-sync weaknesses never come up. | new | M |
| Fonts and UI styling — Georgia/serif HUD, vmin sizing, dust-ochre palette | A SECOND Theme resource, buxoro_theme.tres, swapped into the episode's Control root rather than reskinning woe_theme.tres — blueprint-blue 1850-1900 and Bukhara-1238 dust-ochre are different art directions for different episodes, and two themes exercise the data-driven claim. Georgia is a Microsoft font and cannot ship in a Godot export, so the serif is substituted with EB Garamond (or Source Serif 4 / Noto Serif); numerics keep IBM Plex Mono, which the repo already licenses. Coverage check is mandatory before committing: the Uzbek Latin UI uses U+02BB and U+2018 literally (the skip button reads 'O‘TKAZIB YUBORISH'), so the serif must cover Latin Extended. Palette as theme constants: bg #1a1712, text #e8dfc9 / #f0e6cd, danger #d9573a, plus the five resource chips tosh #8d8579 / yogoch #7d6039 / teri #a07a4e / temir #6d4a35 / komir #2b2724. All vmin sizing maps onto the existing canvas_items / keep stretch at 1920x1080, so the HUD survives windowed, fullscreen and any embed identically — exactly what vmin bought on itch.io. | new | S |
| ChronoShift shell integration — menu entry, autoloads, save | The episode is a standalone scene reached from ChronoShift's MainMenu catalog row via ChangeSceneToFile. GenericMissionMap/MapData/MissionMap/MapLoader are ALL REFUSED in writing, and this is where this plan diverges from both runners-up: MissionMap warns and can never finish with zero EngineeringTasks, so routing through it forces the invention of a fake completion task purely to satisfy a framework the episode otherwise rejects. Buxoro's win condition is a kill counter and its loss condition is a clock; MapData cannot express a 180x100 field, a 46 m ring, six rotated wall points or an analytic terrain, and extending it would change the shared contract all 10 shipped maps load. Buxoro touches none of the seven autoloads — no money, XP, inventory or tech tree — and they stay loaded and idle. Buxoro keeps no save state: it restarts from zero every run, matching location.reload(), so only an episode-unlocked flag would ever persist. | extend | S |

## Data resources

| Path | Purpose | Fields |
|---|---|---|
| `res://resources/data/buxoro/balance_buxoro.tres` | Verbatim transcription of balance.js — the single reviewable diff surface, landed before any behaviour exists. Verified field-for-field against source. | BuxoroBalance: BoshlangichVaqt=600, DevorBonus=25, DevorSeksiyaSoni=6, MaksimumVaqt=720 \| YurishTezligi=5.0, YugurishTezligi=7.0, OtTezligi=10.0, SakrashTezligi=4.6, Gravitatsiya=9.8, BelgilashRadius=3.0, BelgilashVaqti=0.2 (balance.js:20 — referenced nowhere, marking is instantaneous), YasashVaqti=3.0, QilichYasashVaqti=4.0 \| KaltakZarar=8, KaltakCooldown=0.60, KaltakRadius=2.0, QilichZarar=25, QilichCooldown=0.45, QilichRadius=2.5, KamonZarar=18, KamonCooldown=0.90, KamonMasofa=32, KamonKonus=0.40, QalqonBlok=0.70 \| ShogirdBoshlangich=2, ShogirdMaksimum=6, ShogirdTezligi=4.0, YigishVaqti=2.0, BirSafarOladi=2, YollashVaqti=11.0 (three comments and index.html:108 say 15 s and are wrong), QochishRadiusi=4.0, QochishDavomiyligi=5.0, BelgiNavbatiQoshimcha=2 \| TugunSoni tosh=18 yogoch=16 teri=10 temir=14 komir=8 \| DevorUshlabTurish=6.0 (per SINGLE attacker; combat.js:263 says 5 s and is wrong), DevorSeksiyaHp=140 \| JamiDushman=24, HumoOgohlantirish=15.0 \| AskarSoni=8, AskarHp=60, AskarJonMasofa=12, KamonchiZarar=12/Otish=1.2/Masofa=25, QilichbozZarar=20/Hujum=0.8, QurolsizZarar=3/Hujum=1.6/Masofa=2.2 \| TarobiyOlimi=240, ShomurodHp=140, ToqnashuvRadiusPiyoda=0.45, ToqnashuvRadiusOtda=0.6 \| ShomurodBoshi=Vector2(96,6), TarobiyJoyi=Vector2(108,1), MaydonKengligi=180, MaydonBalandligi=100, DevorChizigiX=60, SpawnZonasiX=12, Ustaxona=Vector2(152,-22), Bozor=Vector2(148,20), DarvozaX=172 \| typed Array refs to TugunTuri[5], TolqinData[3], DushmanTuri[3], RetseptData[3] \| Validate() asserts (6+0+0)+(5+3+0)+(6+3+1)=24=JamiDushman |
| `res://resources/data/buxoro/sehrli_buxoro.tres` | Every gameplay number hardcoded OUTSIDE balance.js despite README:197 promising there are none. Without it the port cannot be faithful — these shape the feel. Each field carries a // source: file:line comment. All radii stored UNSQUARED; the JS writes squared literals (16, 25, 36, 196) and porting those as-is is how a radius silently becomes wrong. | TugunQoldiTosh=4, TugunQoldiBoshqa=2 (world.js:215 — verified in source, NOT the obsolete 14-node comment directly above it) \| YollashYaqinlik=3.2, YollashMinUshlash=0.15 \| ShogirdTolerantsiyaTugun=1.1, ShogirdTolerantsiyaOmbor=2.8 (deliberately wider so six apprentices do not pile on one point), ShogirdTolerantsiyaQochish=0.5, QochishMasofa=10.0, QochishKoef=1.35 \| RadiusOt=3.0, RadiusDevor=4.0, RadiusUstaxona=5.0, RadiusQilichOchish=6.0, RadiusJangUi=14.0 \| SensorBelgiMasofa=30.0 (touch.js:145 — 10x the keyboard radius on purpose), SensorTapMs=280, SensorTapPx=14 \| DushmanAggro=40.0, DushmanYaqinlik=2.2, NoyonYaqinlik=2.8 \| DevorBlokX=62.5, DevorBlokZ=6.6, DevorBlokOldi=4.0, DevorBlokOrqa=2.5 \| ShaharDevoriRadius=46.0 \| TosiqBalandlikChegi=1.0 (world.js:596 + combat.js:46 — devor_seksiya.glb is ~0.3 m so built walls are NOT player obstacles; deliberate, do not fix) \| MeleeKonusYarim=1.22, ZarbaPozaMelee=0.34, ZarbaPozaKamon=0.45, KamonTortish=0.55 \| OqTezligi=42.0, OqUmri=2.2, OqRadius=1.0, OqSpawnYOyinchi=1.45, OqSpawnYAskar=1.5 \| OtMinishUshlash=0.7, RiderYOffset=1.42 \| TurishChegi=6.0 (game.js:424 compares against 6 m/s while walk is 5.0 — a walking player is nagged to run; shipped), TurishNagSoniya=3.0, TurishNagCooldown=-6.0 \| QilichbozTezlik=5.2, QilichbozOraliq=2.0, OtliqYonKoef=0.85, OtliqOldinKoef=0.25, OtliqZClamp=44.0, OlimTaymer=2.5 \| KameraOrqaPiyoda=6.0/Otda=8.0, KameraTepaPiyoda=4.5/Otda=5.5, KameraFovPiyoda=70/Otda=78, KameraLookY=1.5, TerakSoldirish=0.22, OkklyuziyaOrqaga=1.2, OkklyuziyaHits=6 \| GavdaYawLerp=14.0, QadamOtda=0.26/Yugurish=0.33/Yurish=0.46 \| AnimClampMin=0.55, AnimClampMax=2.4, AnimXfade=0.18/Hujum=0.06/Sakrash=0.05, AnimJitterBase=0.93, AnimJitterRange=0.14 |
| `res://resources/data/buxoro/world_buxoro.tres` | Deterministic world-generation inputs. Three seeds reproduced exactly because balance.js assumes a byte-identical field every run. | UrughDunyo=1238 (world.js:10, verified), UrughBino=8317, UrughOt=4711 \| YerXMarkaz=120, YerKengligi=680, YerChuqurligi=520, YerSegX=110, YerSegZ=80 \| TumanOddiy=0.0075, TumanOgoh=0.0115, TumanRang=#d4c8b0, TumanLerp=0.6, FonRang=#d8ccb4 \| QuyoshRang=#efdcbb, QuyoshEnergiya=0.85, QuyoshPoz=Vector3(-96,40,26), SoyaOlcham=2048, SoyaBias=-0.0008, AmbientSky=#c6bda6, AmbientGround=#3b3427, AmbientEnergy=0.38 \| ClampX=Vector2(2,176), ClampZ=47 \| TerakRow1=16 @x129 step6.4, TerakRow2=10 @x137 step9.2, TerakKlaster=5, TerakYol=12, TerakTosiqR=0.4 \| DevorNuqtaX=60, DevorNuqtaZ0=-33, DevorNuqtaOraliq=13.2 \| ShaharDevoriR=46, BalandlikKoef=1.30, KorinmasYoy=0.30*PI, Buzilgan=[3,4,11,17], Yoq=[12] \| ArkPoz=Vector3(250,0,10) \| BozorDx=4.2, BozorDz=4.6, BozorSeatY=0.28, BozorRastaTosiq=Vector2(2.4,2.0) \| OtPast=800, OtTutam=600, Butta=120, GulSariq=150, UrinishKoef=6, ChetDevor=3.5, ChetForgeR2=60, ChetBozorR2=90, ChetYolX=160, ChetYolZ=12 |
| `res://resources/data/buxoro/tugun_{tosh,yogoch,teri,temir,komir}.tres` | Five resource kinds. The LCG consumption order tosh→yogoch→teri→temir→komir must match world.js or all 66 node positions shift. | TugunTuri {Tur, Soni, Qoldi, Rang3d, RangHud, Zona, ModelNomi}: tosh 18/4/#7d7568/x[28,118] z[-42,42] · yogoch 16/2/#7d6039/x[126,146] z[-44,-18] · teri 10/2/#a07a4e/x[132,150] z[10,34] · temir 14/2/#6d4a35/x[84,122] z[-40,-8] · komir 8/2/#2b2724/x[96,124] z[14,40]. Field totals 72/32/20/28/16 against demand 24/16+8/12+4. Because BirSafarOladi=2 a stone node needs exactly TWO marks and every other type one — the main hidden cost driver. |
| `res://resources/data/buxoro/tolqin_{1,2,3}.tres · dushman_{piyoda,otliq,noyon}.tres · retsept_{devor,kamon,qilich}.tres` | Waves keyed by CLOCK VALUE not elapsed time — this is what makes building a wall genuinely delay the next wave. Enemy archetypes and recipes. | TolqinData {Vaqt,Piyoda,Otliq,Noyon}: 480/6/0/0 · 330/5/3/0 · 150/6/3/1, sum 24 asserted at _Ready. DushmanTuri {Hp,Zarar,Tezlik,Hujum,YaqinlikRadius,ModelNomi,Uslub,Otli}: piyoda 50/7/4.5/1.2/2.2 · otliq 72/12/8.4/1.7/2.2/otli · noyon 200/18/5.0/1.5/2.8. Wall damage is derived not stored: (DevorSeksiyaHp/DevorUshlabTurish)*Hujum = 28.0 piyoda, 35.0 noyon. RetseptData: devor 4 tosh/3.0 s · kamon 2 yogoch+1 teri/3.0 s · qilich 3 temir+1 komir/4.0 s |
| `res://resources/data/buxoro/sfx_*.tres (15) + kuy_dphrygian.tres + res://default_bus_layout.tres` | The WebAudio zarb() parameter table as data so synthesis stays data-driven and zero audio files ship. Verified: the project has NO bus layout file today, and the Act III beat is a 1.2 s fade on the Music bus alone, so the layout is a prerequisite. | SfxRecipe {Hajm,Uzunlik,Chastota,Oxirgi,Tur,Shovqin,Filtr,Q,KechikishMs,Keyingi}: bolga {2100,0.13,0.28,noise,Q5}+{780→300,0.22,0.16,square} · qadam {160,0.07,lowpass} · tuyoq {110,0.10,lowpass} · kaltak {210→80,square} · qilich {3000,noise,Q8}+{620→190,saw} · kamon {1500,noise,Q3} · blok {1100,noise,Q6} · olim {220→55,0.45,saw} · zarba {90→40,sine} · belgi {880→1320,sine} · yigildi {520→700} · rad {200→120,square} · shogird {440→660}+{660→880 @+110ms} · vaqt [523,659,784,1046] @85ms · qushUchdi {300→1200,0.55}. Wind: noise→bandpass 420 Hz Q0.6, LFO 0.09 Hz ±190 Hz, driven live. Kuy: 13 D-Phrygian (hz,sec) pairs, triangle + 1.005-detuned sine at 0.35, gaps 0.92x, fade-in 2.5 s to 0.30, fade-out 1.2 s, victory reprise 1.35. Buses Master=-5.19dB, Music=-10.46dB, Sfx, Ambient. |
| `res://resources/translations/buxoro.csv → buxoro.uz.translation / buxoro.en.translation` | All four scattered JS dictionaries plus the Uzbek-only MALUMOT merged into one table. Key names preserved verbatim because the code indexes by key everywhere. | Columns keys,uz,en,ru(empty — CLAUDE.md asks for uz/ru/en). HUD MATN 22 keys · GAP ~30 · menyu SERIYA+MISSIYALAR · main pauzaMatn · chizma MALUMOT (first English column). THE DUPLICATE-KEY CASUALTY, verified at game.js:16/:25 and en :44/:53: both strings ship as distinct keys — gap_olim_tarobiy = '"Endi senga hech kim aytmaydi."' and gap_olim_sarlavha = 'USTA YIQILDI' — with the faithful behaviour wired by default (_gapOlimTuzatilgan=false, both display the headline) and flagged as the ONE place the user should actually decide, because reproducing the bug means Act III's thesis line never appears in the game. |
| `res://resources/data/buxoro/moljal.tres · klip_xaritasi.tres · sahna_*.tres · missiya_*.tres · seriya_*.tres · malumot_*.tres · palette_buxoro.tres · bino_specs.tres` | Import table, clip map, cutscene cues, act-entry presets, series catalog, blueprint captions, actor palette, architecture dimensions. | moljal: shomurod 1.78 (measured GLB is 0.975 units so factor 1.826x — MANIFEST.md's ≈1.75 m claim is false for every file), tarobiy 1.70, shogird 1.62, askar 1.74, mogul 1.72, noyon 2.05, hunarmand 1.35, ot 1.65, humo 0.70, qala_darvoza 11.0, shahar_devori 8.5, ark_qalasi 46.0, terak 11.0, uy_kichik 3.4, uy_katta 4.8, uy_vayrona 2.4, paxsa_devor 2.1, ustaxona 3.1, bozor_rastasi 2.4, plus IkkiTomon list of 12. klip_xaritasi: SANITIZED keys (NlaTrack_003 not NlaTrack.003), TabiiyTezlik shomurod walk 1.21 m/s run 4.33 m/s, LoopKliplar, SiljishQolsin=die/death/olim/yiqil/fall/land/takeoff. sahna: boshlanish[1,2,3] toshTugadi[4] tarobiy[6] qilichYasaldi[5,7] galaba[8,9] maglubiyat[game-over,game-over-1] olim[same two, separate one-shot key]. missiya: toliq 600/2/opening, parda1 600/2, parda2 480/4, parda3 240/6. seriya: 5 rows, 2 locked. |

## Risks

- **Draco is a verified hard blocker that makes the entire port unevaluable. All 17 GLBs are rejected outright by the installed Godot 4.7.1 — 'KHR_draco_mesh_compression is not supported', ERR_PARSE_ERROR, no partial import, no fallback, and Godot has no decoder planned. A team that hits this in week three has built everything against capsules.**

  It is step 1, before any gameplay code, and it is cheap: npx @gltf-transform/cli dedup decodes in place in seconds (3.66 MB → ~8 MB) and the result was verified to import with zero errors and webp textures intact, since EXT_texture_webp IS supported in 4.7 — so no texture work is needed. The cleaner path is re-running Torobiy's own tools/optimize-models.js from _xom/ with --compress false, keeping the per-model simplify ratios. NEVER import _xom/ raw: 1.86-1.99M tris each against an 8k-per-asset budget and a hub already measured at 662k tris on gl_compatibility.

- **Animation retargeting is where the port quietly becomes a different game. klip-xaritasi.json names only 14 clips across 6 models and its own _holat field admits only idle/walk/run are trusted; the ~22 other names the code requests resolve to nothing. Gameplay BRANCHES on that miss — combat.js:470 picks ragdoll vs die-clip from the boolean. Worse, Godot silently renames on import (NlaTrack.001 → NlaTrack_001, tripo::Root → tripo__Root), so a direct port of the JSON matches nothing and fails without an error, and every clip arrives LOOP_NONE so walk and run play once and freeze.**

  Three defences. (1) klip_xaritasi.tres stores SANITISED keys and the post-import sets LoopMode.Linear on locomotion. (2) The soft-fail boolean contract survives as a per-actor capability set built at load, and Models hard-fails LOUDLY (PushError plus an on-screen warning in debug builds) on a missing clip or a -1 bone rather than early-returning — the web's silent degradation is what hid this for so long, and it is the same class of bug that makes WeaponController's Nathan-specific bone names produce a character that walks perfectly and never raises its arms. (3) ProtseduralYurish is scheduled as first-class shipping content with its three style tables ported verbatim, because it is what players actually saw. The leverage: all six characters share a byte-identical 41-bone Tripo rig, so ONE BoneMap against SkeletonProfileHumanoid both shares clips across the cast and opens Mixamo retargeting — by far the cheapest route to the missing ~20 animations. Watch tarobiy's 42nd neutral_bone, which breaks any 'assert 41 bones' shortcut, and hunarmand/tarobiy's extra roots carrying a -90° X quaternion that double-rotates under an import rotation.

- **mp4 → Theora is blocked on this machine today and roughly doubles the media budget. The installed ffmpeg is Theora DECODE-only — the documented command fails immediately with 'Unknown encoder libtheora'. Theora needs ~2.0-2.5 Mbps to perceptually match the current ~1.0 Mbps h264, taking the set from 17.3 MB to 30-37 MB, and these are AI-generated dust-and-smoke clips, exactly where its ringing is worst.**

  Installing a theora-enabled ffmpeg is moved into step 1's user actions rather than step 15's, so it is never a surprise on the critical path. Transcode from _xom/ originals, never the shipped copies. Do not go below q:v 6 at 720p; drop to 960x540 q:v 7 (~1.5 MB/clip) if size matters more than sharpness. One piece of luck materially reduces this risk: all 7 shipped cues are fullscreen-and-pause, so the port only ever decodes Theora while the sim is frozen, and Godot's known seek and audio-sync weaknesses never come up.

- **Procedural WebAudio has no Godot equivalent, and a naive answer loses the two things that matter most — the wind's continuous breathing and the music's tempo-without-pitch change on the victory reprise. Get it wrong and the Act III silence beat, which the whole third act rests on, lands as a bug instead of a statement. There is also no bus layout in the project at all, so a music-only fade is impossible out of the box.**

  Live C# synthesis is the shipping path, preserving the literal zero-audio-files property and removing any build-tool dependency; the web's zarb() parameters are the spec so it is ~60 lines. Wind is NEVER baked — a 20 s noise loop on a dedicated Ambient bus with a live AudioEffectBandPassFilter whose cutoff is written per tick. Music is scheduled note by note so rate 1.35 changes tempo, not pitch. default_bus_layout.tres with Master/Music/SFX/Ambient is created first, as a prerequisite rather than polish. Every gain literal goes through Mathf.LinearToDb (WebAudio is linear, Godot is dB). And the verification is objective rather than a listening test: the reference bake produced by running audio.js itself under OfflineAudioContext gives an RMS-envelope and spectral-centroid oracle per clip.

- **The apprentice AI IS the economy, and any 'improvement' silently rebalances the whole game. They have no pathfinding and no collision — they walk through trees, houses, the bazaar and the city wall. A stone round trip measures 42.3 s at the 1238 seed and a full wall is 12 trips ≈ 508 apprentice-seconds against 120 s of Act I, which is what makes recruiting mathematically mandatory. Give them a NavigationAgent3D or a CollisionShape3D and every one of those numbers moves. Three subtler traps: Holat.Bosh must be 0 or the flee-resume fallback silently changes; the flee timer deliberately does not refresh; and marks are consumed per TRIP.**

  Shogird is a Node3D that never calls ToqnashuvHal, with the reason in the file header and in BUXORO_REFUSALS.md. Bosh = 0 is pinned with a comment and a static assert. All the off-balance.js numbers live in sehrli_buxoro.tres with file:line provenance so they are visible and diffable rather than buried. Step 7's check is explicitly economic rather than functional — a golden trace diffed to 0.01 m plus measured round-trip means against the web's figures — because an FSM that looks correct can still be wrong by 20% on travel time, and no amount of playtesting catches that.

- **Frame-timing drift between rAF and Godot's split _Process/_PhysicsProcess. Several smoothers are frame-rate corrected and several are NOT (FOV Min(1,dt*3), gait blend Min(1,dt*7), fog dt*0.6, chizma dt*2.2). Split them across two rates and the uncorrected ones drift away from the values the game was tuned at — which is 'feel', the hardest thing to notice going wrong and the hardest to argue about afterwards.**

  Run the ENTIRE simulation, camera and occlusion fade included, in GameDirector._PhysicsProcess at a fixed 60 Hz with physics interpolation on for high-refresh monitors. dt is then always 1/60, exactly what the author tuned on, so the uncorrected smoothers become correct by construction and the corrected ones are unchanged; all idioms live as literal expressions in one Smoothing class so no later file re-derives a constant. max_physics_steps_per_frame = 3 makes the web's 1/20 s clamp a live slow-motion floor. Advance the AnimationTree manually from the same tick so the ZarbaPozasi-after-animator ordering is enforced by code, not by scene-child order — the way it silently breaks in WeaponController today. Step 4's checks are stopwatch-based precisely because integration drift is invisible to the eye.

- **The clock-as-resource design is easy to dissolve into ChronoShift's existing systems and impossible to get back. One descending float is simultaneously the timer, the act sequencer, the wave trigger and the loss condition, and it can move BACKWARDS — building a wall adds +25 s to the same clock that schedules waves, which is the game's central strategic decision. TimeManager is an active temptation and is exactly wrong.**

  One float, decremented at the very top of the tick with both terminal checks immediately after, and exactly ONE caller of VaqtQosh, asserted by a test. Acts and waves read the clock by VALUE, never elapsed time. TimeManager, MissionManager, MissionMap and MapData are refused in writing with the numbers each would change. The wave table sums to JamiDushman under a _Ready assertion, because victory is a counter and a mismatch makes it unreachable or premature. Step 9's explicit test is: build a wall between waves and confirm the next wave arrives later.

- **Faithfully reproduced bugs get 'fixed' by the next person, or get reported as port defects. The source ships at least eight behaviours that look like mistakes: built walls are walk-through, the whole poplar row fades at once, the duplicate olim key makes Act III's thesis line unreachable, 'Yugur!' fires at a walking player, stone goes invisible from Act II while the Act III tip tells you to spend it, the flee timer never refreshes, the stats card counts the two free apprentices as recruits, and Ceil can print '4:60'.**

  Every one is reproduced with a named boolean or constant beside it and a comment citing the JS file:line and the symptom, so it reads as a decision: _terakUmumiySoladi, _gapOlimTuzatilgan, TurishChegi, TosiqBalandlikChegi, _eHoldSurvivesFocusLoss, _sensorOchilishniOtkazadi. Each also gets a row in BUXORO_PARITY.md phrased as a positive assertion, so a future 'fix' FAILS the checklist rather than passing silently. The duplicate-key case is the ONE I recommend the user actually decide, because the faithful behaviour means the sentence the entire third act is built on never appears in the game — both strings ship as distinct keys so either choice is one flag away.

- **Sheer size. ~6,210 lines of dense JS across 20 modules and ~44 systems, plus 17 models, 12 videos, 15 synthesised sounds and two languages, going into a codebase with no melee, no projectiles, no mount-that-keeps-the-rider-visible, no cutscenes, no localisation layer and no audio buses — by a workflow where Claude writes C# but the user wires every node tree and performs every playtest. The realistic failure is not one system; it is momentum dying around step 9 with a half-ported game, or fidelity checks quietly getting skipped under fatigue.**

  The mirror structure IS the schedule control — each step maps to one or two JS modules and is independently runnable, so progress is legible and a stall is local. The ordering is chosen so there is always something on screen to check against: step 1 puts a correctly-scaled character in the editor on day one, step 3 a walkable field, step 4 real movement, steps 5-8 the complete Act I loop (mark, gather, build, clock), which is the whole game's thesis. Steps 9-11 add the siege; 12-18 are additive and each independently postponable to a later pass WITHOUT cutting anything permanently — they are ordered last because they are additive, not because they are optional. The mission presets land early as data and are the QA path that makes Acts II and III reachable in seconds, which is what keeps later steps testable at all. PROGRESS.md is updated every session as CLAUDE.md requires, and each step is gated 'DONE' on the user's playtest, never on compilation.

- **Silent-failure patterns already present in ChronoShift will bite the new code the same way. GenericMissionMap hands data to nodes by STRING field name with no compile-time link; WeaponController bakes Nathan-specific bone names into [Export] defaults and every consumer early-returns on FindBone's -1, so a new model walks perfectly and never raises its arms with no error and no log line; five module .tres files already ship a uid/path mismatch that loads coal where the path says wood.**

  Buxoro uses typed [Export] resource references throughout, never Set-by-string. Bone names live in klip_xaritasi.tres next to the clip names, and Models._Ready hard-fails loudly on any -1 bone or missing clip. Every new .tres gets a fresh unique uid, load_steps computed as 1 + ext + sub, and the Script ext_resource carries no uid — all three confirmed against the repo's existing files. All folder scans go through ResourceFolder.Paths, never a raw .tres filter, or the exported build loads nothing (an export repacks text resources as .res/.remap).

## Critic findings

Two adversarial critics reviewed the final plan: completeness and Godot feasibility. 30 findings — 5 blocker, 15 major, 10 minor. **Fix the blockers before writing code.**

### BLOCKER (5) — ALL RESOLVED

> Superseded by the Blocker corrections section near the top, which re-verified each finding against source. Kept here as the audit trail. Where a fix below contradicts the corrections section, the corrections section wins.

1. **Enemy spawn Z formula is transcribed in the plan as "z = -38 + (i/(n-1))*76 ±4". The source is `-38 + (i / Math.max(1, soni - 1)) * 76 + (Math.random()-0.5)*8` (combat.js:143). Wave 3 spawns exactly ONE noyon, so n-1 = 0 and a literal port yields 0/0 = NaN in both JS and C# float.**

   *Fix:* Write the guard into the mapping text and into the tolqin spawn code: `z = -38f + (i / Mathf.Max(1, soni - 1)) * 76f + (GD.Randf()-0.5f)*8f`. Add a step-9 test asserting the lone noyon spawns at z≈-38, not NaN — with NaN the noyon never reaches a target, is never killed, and OlganDushman can never reach 24, which makes victory unreachable.

2. **The plan's Input Map says "reuse move_*, jump and run" from ChronoShift. ChronoShift binds jump to Space AND Up arrow, and run to Shift AND Down arrow (project.godot:36-147). The web game uses the arrow keys as MOVEMENT: `if (T.KeyW || T.ArrowUp) iz += 1; if (T.KeyS || T.ArrowDown) iz -= 1; ArrowLeft/ArrowRight` (player.js:211-214). Reused verbatim, Up arrow jumps instead of walking forward and Down arrow sprints instead of walking back.**

   *Fix:* Do not reuse ChronoShift's jump/run actions. Declare buxoro_sakra (Space only) and buxoro_yugur (ShiftLeft/ShiftRight only), and add ArrowUp/Down/Left/Right as second events on the four move actions — all by physical_keycode. Add a step-4 check: every arrow key moves and nothing else.

3. **All 15 SFX are routed through ChronoShift's `Sfx.PlayAt/PlayUi`, which are NOT behaviour-neutral. `PlayAt(..., float pitchSpread = 0.08f)` applies ±8% random PitchScale to every one-shot (Sfx.cs:17,74-77) and plays through an AudioStreamPlayer3D with MaxDistance 60 / UnitSize 6 distance attenuation. The web game has zero pitch variation and zero positional audio — every `zarb()` gain node connects straight to `master` (audio.js:66-74); there is no PannerNode anywhere in the file. So the anvil, footsteps, mark chime and hoofbeats all change pitch run-to-run and fade with distance in the port.**

   *Fix:* Call `Sfx.PlayAt(ctx, stream, pos, db, pitchSpread: 0.0f)` — or, correctly, do not use PlayAt at all for Buxoro: the web sound field is 2D, so every SFX belongs on a plain AudioStreamPlayer routed to the Sfx bus (Sfx.cs assigns no bus either, so the plan's Master/Music/SFX/Ambient layout is bypassed). Add an explicit REFUSAL row for Sfx.PlayAt's 3D/jitter defaults alongside the WeaponController/PlayerHealth refusals.

4. **Dialogue and card strings carry HTML markup that the plan's CSV + plain-Control HUD cannot render. `HUD.gap` writes `el.gap.innerHTML = '<b>'+kim+'</b>' + matn` (hud.js:171); t1b contains `— <b>vaqt.</b>`; the `ochilish` narration contains `<br>`; `kadrKorsat` joins the two body lines with `<br>` (game.js:657-659). The plan's translation mapping and BuxoroHud mapping say nothing about markup.**

   *Fix:* State the markup contract: the dialogue banner and the ending card are RichTextLabels with BBCode on, and the CSV holds `[b]…[/b]` / `\n` in place of `<b>` / `<br>`. Add a step-11 check that the speaker name renders bold and that 'vaqt' is emphasised inside t1b, because that emphasis is the thesis word of the game.

5. **The step-2 determinism gate cannot pass as specified. ParityDump is spec'd to replay "tosh 18 → yogoch 16 → teri 10 → temir 14 → komir 8, each drawing Orasida(minX,maxX) then Orasida(minZ,maxZ)" — but world.js draws far more than 2 values per node. tugunYarat (world.js:160-205) consumes rnd() INSIDE the procedural branch: yogoch 4 draws x4 bushes, temir ~5 draws x5 pieces, komir 2 draws x6, teri 1, tosh 6 draws x3 — and that branch only runs when MODELS.ol('tugun_'+tur) returns null. Only tugun_tosh.glb exists, so stone takes the GLB path (0 extra draws) while the other four take the procedural path. terakYarat (world.js:219-226) likewise draws 2 (rotation.y, scale) on the GLB path and a different count on the procedural path. Therefore the LCG stream — and every node position after the 19th — depends on which GLBs loaded, and a dump that draws x,z only diverges from the first yogoch node onward. The plan's own stop-rule ("Until that holds: do not write World.Qur, do not write Player.cs") makes this a hard stop.**

   *Fix:* Rewrite step 2's parity harness to replay world.js's ACTUAL generation, not an idealised x/z sequence: port tugunYarat/terakYarat's placeholder geometry with rnd() consumed in the same order and count, gated on the same MODELS.ol() null test. Add an explicit line to the Actors.cs/World.cs mappings stating that procedural placeholder geometry is part of the determinism contract, not cosmetics — changing a box count changes all 66 node positions. Also state which GLBs are assumed present, since the stream is conditioned on that set.

### MAJOR (15)

1. **The opening narration card. `ochilishBoshla` fills `#kadr` with GAP key `ochilish` ('"Ular kelganida men sakkiz yoshda edim…"' / '"I was eight when they came…"') and auto-hides it after 2000 ms while the 6 s flythrough runs (main.js:131-143). The plan's only `#kadr` equivalent is YakunKadr.tscn, which is ending-only; no mapping, step or scene shows this card.**

   *Fix:* Add the opening card to the Menyu/BuxoroMain mapping: same Control as the ending card, shown on flythrough start with Tr('ochilish'), hidden by a 2.0 s real-time timer (not the game clock), and cleared by the same Space/skip path that ends the flythrough.

2. **The apprentice hand dots are a THREE-state widget, not a count. `qollar(royxat, maksimum)` emits `<div class="q yoq">` for an unrecruited slot, `<div class="q">` for an apprentice in HOLAT.BOSH, and `<div class="q band">` for a working one (hud.js:73-81). The plan says only "the six apprentice-hand dots" and "Hand dots = 6". The busy/idle distinction is the only readout telling the player an apprentice is standing around waiting for a mark — with the HUD philosophy of clock-plus-dots-only, dropping it removes half the permanent UI's information.**

   *Fix:* Specify three dot states (yoq / bosh / band) with their three styles in buxoro_theme.tres, fed from `Shogirdlar[i].Holat == Holat.Bosh`, and add a step-7 check: mark a node and watch exactly one dot flip to busy and back.

3. **The workshop is missing from the Binolar generator list. `ustaxonaYarat` (world.js:271-308) is a full procedural generator — anvil cylinder + block, a fire cylinder with an unshaded ember disc (MeshBasicMaterial 0xd4551e), four posts, a 6.0x0.16x5.2 reed roof, SIX hand-authored `tosiqlar` (anvil r0.6, fire r0.8, four posts r0.16 so you can walk under the roof) — plus a 4.2 m ash circle added unconditionally, GLB path included. The plan lists seven generators and the workshop is not among them; World.cs's mapping names it only as a placement.**

   *Fix:* Add Binolar.Ustaxona() to the generator list with its six footprints and the ash disc, and put its dimensions in bino_specs.tres. Note in the same mapping that the six footprints exist only on the procedural branch — with ustaxona.glb present, binoTosiq falls back to the measured AABB, which seals the walk-under-the-roof gap the footprints were authored to preserve.

4. **DistanceCull is listed as "genuine reuse" and "reused verbatim" with no range named. Its defaults are range 90 / margin 12, and the web game does the opposite: models.js disables frustumCulled on skinned meshes and the Humo bird is scaled 3.2x specifically so it reads at ~250 m, while the Ark is 46 m tall and read from across the 180 m field.**

   *Fix:* Either drop DistanceCull from the Buxoro reuse list, or name a per-actor range in sehrli_buxoro.tres (Humo and architecture uncapped; enemies/apprentices ≥ 180 m) and add a step-12 check that the bird is still visible from the west edge. A silent 90 m cull would read as pop-in, and the bird is the only wave telegraph.

5. **The weapon-switch toasts — one of the playtest items named in the brief. game.js:311-317: `if (PLAYER.qurolTanla(p,q)) H.toast(H.t(q)); else if (!p.qurollar[q]) H.toast(H.t(q) + ' — ' + H.t('haliYoq'))`, i.e. 'KALTAK' on a successful switch and 'KAMON — hali yo'q' on a weapon you have not crafted. Neither appears in the plan's weapon, HUD or sword-choice mappings.**

   *Fix:* Add both toasts to the weapons mapping and to the OzaroTasir step-8 deliverable, and pin the `haliYoq` key in buxoro.csv. Add a parity row: pressing 2 before the first bow toasts 'KAMON — hali yo'q' and does NOT switch.

6. **House placement. `uylarQoy` (world.js:249-253) hardcodes 8 buildings at fixed coordinates — uy_katta (140,-40), uy_kichik (150,-30), uy_kichik (134,-46), uy_katta (146,36), uy_kichik (156,26), uy_vayrona (98,-30), (108,-22), (116,-34) — each given a random yaw drawn from the world LCG, which means they also consume LCG draws in a fixed position in the sequence. world_buxoro.tres has slots for poplars, wall points, the ring, the Ark, the bazaar and grass, but none for houses.**

   *Fix:* Add a UyJoylari array (model name + x + z) to world_buxoro.tres and place it at the exact point in the LCG consumption order that world.js uses, or step 2's 66-node parity check will pass while every later draw is shifted.

7. **Baseline wind level. `AUDIO.shamol(0.22, 2.5)` at game start (main.js:66). The plan carries only the three ending levels (0.10 / 0.28 / 0.30) and the 0.09 Hz LFO; the level the game actually spends nine of its ten minutes at is nowhere in sfx/kuy .tres.**

   *Fix:* Add ShamolBoshlangich = 0.22 with a 2.5 s ramp, and a ShamolRamp field, to the audio data, with the file:line citation.

8. **Mouse look sensitivity and the unlocked-mouse fallback have no home. Locked yaw is 0.0022 rad/px (player.js:178), the unlocked fallback is 0.0042 yaw / 0.0032 pitch (player.js:186-187), and there is a screen-edge auto-turn at 2.4 rad/s with a dead zone of max(40 px, 6% of width) fed through `p.chetBurilish()` (player.js:191-196, applied at player.js:344-347). The plan keeps p.pitch as an inert field but never mentions yaw sensitivity at all — so the single most-felt number in the game has no field in sehrli_buxoro.tres, contradicting the plan's own "no episode C# file holds a gameplay literal" guarantee.**

   *Fix:* Add SichqonchaYaw = 0.0022 (and the touch 0.006) to sehrli_buxoro.tres with citations. Then either port the edge-turn as a windowed-mode camera aid (ChetBurilishTezlik 2.4, ChetDeadzonePx 40, ChetDeadzoneNisbat 0.06) or write it into BUXORO_REFUSALS.md as a pointer-lock-only mechanic being dropped — silence is the one option that fails the mandate.

9. **The mark pillar's material and geometry are described imprecisely enough to be wrong. In source only the OUTER cylinder is AdditiveBlending; the inner core (0.07/0.14, #fff0c0, α0.95) and the ground ring (#ffb524, α0.95) are ordinary transparent, fog:false, depthWrite:false, DoubleSide (apprentices.js:13-35). Both cylinders are open-ended (no caps). The ground piece is a flat RingGeometry(0.55, 0.95, 24), not a torus; the plan says "ground TorusMesh 0.55-0.95", and Godot's CylinderMesh is always capped and single-sided by default.**

   *Fix:* Correct the mapping: outer tube additive α0.85, core and ring alpha-blended α0.95, all three unshaded / fog off / depth-write off / cull disabled; build the two tubes and the flat annulus as ArrayMeshes (or a capless cylinder + a quad with a ring shader), because CylinderMesh caps and TorusMesh thickness both change the silhouette of the game's single most important signal.

10. **Nothing in the plan creates the poplar collision bodies the occlusion ray needs. Step 4's mapping says "IntersectRay from camera toward player ... against a layer containing ONLY poplar StaticBody3Ds", but Godot's glTF import creates no physics bodies: assets/models/buxoro/terak.glb.import has no physics option set, and the -col/-colonly suffix route (nodes/use_name_suffixes=true) would require renaming nodes inside the GLB. World.TerakYarat as described instantiates the model only.**

   *Fix:* Add to the step-3/step-4 deliverable: TerakYarat wraps each poplar in a StaticBody3D + CylinderShape3D (r≈0.4, matching the world.js:130 obstacle radius) on a dedicated collision layer, created in code. Note this is presentation-only — it must NOT be added to World.Tosiqlar, which already has the trunk as a circle obstacle.

11. **Two Godot 4.7 API calls in the plan do not exist as written. (1) "AnimationTree.ProcessCallback = Manual" — verified against the installed 4.7.1 class reference: AnimationTree.xml exposes only the deprecated methods set_process_callback/get_process_callback (enum AnimationTree.AnimationProcessCallback); the live member is AnimationMixer.callback_mode_process (enum AnimationMixer.AnimationCallbackModeProcess, ANIMATION_CALLBACK_MODE_PROCESS_MANUAL = 2). `tree.ProcessCallback = ...` will not compile, and the step-6 instruction telling the user to set "ProcessCallback" in the Inspector names a property that is labelled "Callback Mode Process". (2) PhysicsDirectSpaceState3D.intersect_ray returns a single Dictionary, not a hit list — "up to 6 hits" is not a parameter of anything.**

   *Fix:* Use AnimationTree.CallbackModeProcess = AnimationMixer.AnimationCallbackModeProcess.Manual (and tell the user the Inspector label). Replace the occlusion query with an explicit loop of up to 6 IntersectRay calls, each adding the previous hit's collider RID to PhysicsRayQueryParameters3D.Exclude, or use a ShapeCast3D. Also: BaseMaterial3D.DepthDrawMode has no "Never" — the constant is DEPTH_DRAW_DISABLED.

12. **The per-instance poplar fade (MeshInstance3D.Transparency = 0.78) is asserted without checking the renderer. project.godot pins renderer/rendering_method="gl_compatibility" (and .mobile likewise); GeometryInstance3D.transparency is a Forward+/Mobile feature and is not honoured by the Compatibility renderer. The plan makes this the ONLY mechanism for the occlusion fade and calls it out as a fidelity-critical behaviour.**

   *Fix:* Verify on the real project before step 4 (one poplar, Transparency=0.78, F6 under gl_compatibility). If it is ignored, fall back to duplicating the poplar material once and animating AlbedoColor.A / Transparency=Alpha on that duplicate — which the plan's own _terakUmumiySoladi=true note (all poplars fade together) makes cheap, since one shared duplicate is enough. Either way write the chosen mechanism into the mapping.

13. **"Played through the existing Sfx.PlayAt/PlayUi helpers" does not reach the Master/Music/SFX/Ambient bus layout the plan makes a prerequisite. scripts/fx/Sfx.cs:29-37 constructs AudioStreamPlayer3D with no Bus assignment (so everything lands on Master) and hardcodes MaxDistance = 60.0f / UnitSize = 6.0f. The Humo cry is specified to read from ~250 m (step 12) and the bird is scaled 3.2x precisely so it carries that far — at MaxDistance 60 it is silent.**

   *Fix:* Either add optional bus and maxDistance parameters to Sfx.PlayAt (an additive, compile-safe change to a file already in use by WeaponController/Fx), or give Buxoro its own thin BuxoroSfx wrapper. Name in the audio mapping which sounds are 3D and what each one's MaxDistance is — the bird, the anvil and the wall impacts all exceed 60 m.

14. **Several steps depend on files a later step writes, so as ordered they are not independently runnable — which is the plan's stated defence against momentum loss. Step 3 tells the user to attach GameDirector.cs (written in step 8) and its testable ("66 node props stand in the five zones", four-camera screenshot overlay) needs Models.cs/Actors.cs (step 6). Step 7's testable requires the scripted tick loop and OzaroTasir (step 8) plus the rad sound and navbatTola toast (audio step 14, HUD step 11). Step 8 calls H.qollar/H.resurslar/H.jon and Hud.Gap, which arrive in step 11.**

   *Fix:* Either state explicitly in steps 3, 7 and 8 which stub files Claude writes early (a GameDirector that only calls World.Qur; a BuxoroHud with no-op Update methods; a BuxoroAudio with no-op cues), and list them in those steps' `files`, or move a minimal HUD + audio ahead of step 7. As written, three steps' "testable" sections cannot be executed at that point.

15. **The plan systematically violates CLAUDE.md's golden rule "Kod va comment — English" (CLAUDE.md, Godot 4 C# konventsiyalari section) — every class, method, field, enum and .tres key is transliterated Uzbek (Yangila, ToqnashuvHal, BelgiNatija, BoshlangichVaqt). The mirror premise depends on it, and the .tres field names must match the C# field names verbatim, so this is not cosmetic. The plan never acknowledges the conflict.**

   *Fix:* Surface it as an explicit decision for the user before file 1: either amend the CLAUDE.md rule to carve out res://scripts/buxoro (recommended — the file-for-file diff against world.js is the whole fidelity argument), or keep English identifiers and carry the JS name in an XML doc comment on every member. Do not leave it implicit; whichever way it goes, file 1 (BuxoroBalance.cs) locks in ~110 .tres keys that cannot be renamed later without rewriting every .tres.

### MINOR (10)

1. **The stats card's restart button. `kadrKorsat` appends `<button id="qaytaTugma">` with the localized GAP key `qayta` ('QAYTA BOSHLASH' / 'RESTART') wired to location.reload() (game.js:668-670). The plan's YakunKadr.tscn is specified as "Control with the headline, two body lines and the five stat rows" — no button.**

   *Fix:* Add the button to the YakunKadr.tscn deliverable, labelled Tr('qayta'), calling ReloadCurrentScene.

2. **Small HUD details: the clock renders `<span class="qum">⏳</span>` before the digits (hud.js:61); MATN has 26 keys, not the 22 the plan's CSV mapping states (hud.js:7-30).**

   *Fix:* Count the keys off the source when authoring buxoro.csv and keep the hourglass glyph in the clock label.

3. **Dev entry points: the URL overrides ?skip, ?t=, ?shogird= (main.js) and the ?uysiz=1 MUHIT flag that suppresses all houses (world.js:16). These are the QA levers that sit beside the four mission presets the plan keeps precisely because they are the QA path.**

   *Fix:* Name a Godot equivalent — command-line `--t=240 --shogird=6 --skip` parsed in BuxoroMain, or debug fields on the mission preset resource — in step 16.

4. **Tab-visibility ducking: `AUDIO.ovoz(!document.hidden)` mutes all one-shots while the tab is hidden (main.js:334-336, audio.js:168). The plan lists several web-only concerns it deletes with a note but not this one.**

   *Fix:* Either mirror it on NOTIFICATION_APPLICATION_FOCUS_OUT or add it to the deleted-web-concerns list with a reason.

5. **Step 16 says "wire the '001 — BUXORO 1238' row from ChronoShift's MainMenu.tscn to it", but MainMenu is not data-driven: scripts/ui/MainMenu.cs:9-23 resolves each button through an individual [Export] NodePath (_newGamePath, _missionsPath, _heroPath, ...) and hardcodes handlers that call GetTree().ChangeSceneToFile(GameplayScene) (line 101, 132). Adding a row means editing MainMenu.cs and MainMenu.tscn — neither appears in any step's `files` list, and MainMenu.cs is a shared file the episode otherwise never touches.**

   *Fix:* Add scripts/ui/MainMenu.cs and scenes/ui/MainMenu.tscn to step 16's files, with the concrete edit: one new [Export] NodePath _buxoroPath, one button in the .tscn, one handler calling ChangeSceneToFile("res://scenes/buxoro/BuxoroEpisode.tscn").

6. **Step 15's instruction "Run the transcode script against assets/video/_xom/ (the 145 MB originals, never the shipped copies)" is not executable for the menu background. src/assets/video/_xom/ holds 11 files totalling 120 MB (1-9, game-over, game-over-1) — kirish.mp4 exists only as the 1.5 MB shipped copy in src/assets/video/. The 145 MB figure is also wrong.**

   *Fix:* Correct the figure to ~120 MB and state that kirish.ogv must be transcoded from the already-compressed 1.5 MB src/assets/video/kirish.mp4, accepting one generation of loss on a muted, looping, heavily-blurred menu background — or drop it for a still frame.

7. **No step acquires the serif font. The plan substitutes EB Garamond / Source Serif 4 / Noto Serif for Georgia and requires Latin Extended coverage for U+02BB / U+2018, but assets/fonts/ contains only IBMPlexMono (4 weights), Inter-Variable, Oswald-Variable and Archivo-Variable. buxoro_theme.tres cannot reference a font that is not in the repo.**

   *Fix:* Add to step 11's user actions: download EB Garamond (OFL) into res://assets/fonts/, and verify U+02BB MODIFIER LETTER TURNED COMMA and U+2018 render before committing the theme — Torobiy's src/styles.css:7 and index.html:147 use Georgia and the skip button text carries both characters.

8. **buxoro.csv is spec'd with a ru column left empty. Godot's CSV translation importer generates a ru .translation whose every entry is an empty string, which is worse than no ru file at all: TranslationServer will resolve ru keys to "" and the UI goes blank rather than falling back.**

   *Fix:* Ship only keys,uz,en. Add ru as a column when the strings exist; CLAUDE.md's uz/ru/en note is about eventual localisation, not an empty placeholder column.

9. **physics_interpolation = true interacts with the plan's "write GlobalPosition every tick" model in ways the plan does not cover. Interpolated nodes smear across the teleport when an actor is spawned or repositioned (wave spawn, horse mount transform-copy, corpse placement, the mission-preset restart), and MultiMeshInstance3D grass and code-built ArrayMesh nodes need the same treatment. ProjectSettings confirms physics/common/physics_interpolation exists (default false) and physics/3d/physics_interpolation/scene_traversal is a separate knob.**

   *Fix:* Add a line to the TICK MODEL section: every spawn/teleport calls ResetPhysicsInterpolation() on the node in the same tick it is placed — enemies at TolqinChiqar, the horse at mount, the camera at the flythrough→gameplay handoff, and the Humo on state change.

10. **CHIZMA.yangila is inside main.js's else-branch (main.js:291), i.e. the blueprint viewer's spin and fade freeze during a fullscreen video or pause, exactly like the sim. The plan's chizma mapping stresses only that GetTree().Paused stays false during gameplay and does not say the viewer must freeze with the sim under the Videolar/Pauza short-circuit.**

   *Fix:* One sentence in the Chizma mapping: the viewer advances from GameDirector's freeze-gated branch, not from _Process, so it stops with the sim during a cutscene or pause while continuing to render.

## Wrong assumptions caught in the plan

Claims the plan made that the critics checked against source and found incorrect. Correct each before the affected step.

- **Claimed:** "Toast 2000 ms + 1.5 s fade, max 4."

  **Actually:** hud.js:116-121 appends a div and removes it at exactly 2000 ms; styles.css:96 runs toastIn 0.25 s then toastOut 0.5 s starting at 1.5 s — so the fade is inside the 2 s life, not after it. And there is no cap of any kind on concurrent toasts. "max 4" is an invented deviation, and the plan's own rule is that every deviation must be named and flagged.

- **Claimed:** "Each generator writes its hand-authored tosiqlar footprint as node metadata, NOT the bbox."

  **Actually:** True only on the procedural branch. `binoTosiq` prefers userData.tosiqlar (world.js:575-600), but a model returned by MODELS.ol() carries none, so every building that HAS a GLB falls back to the rotation-zeroed AABB. The bazaar is the exception only because world.js:340-345 registers its 2.4x2.0 footprints externally, outside the generator. The plan's whole "playtest footprints survive" argument therefore holds for the bazaar and for missing-GLB buildings only.

- **Claimed:** "Genuine reuse is limited to ArmIk, Sfx/Fx, ResourceFolder, DistanceCull, ModelDresser…" with Sfx and DistanceCull "reused verbatim".

  **Actually:** Sfx.PlayAt defaults to ±8% pitch jitter and 3D distance attenuation with no bus assignment; the web has neither. DistanceCull defaults to a 90 m visibility range; the web explicitly disables frustum culling on skinned meshes and needs the Humo bird legible at ~250 m. Two of the six "safe" reuses are fidelity breaks at their defaults — exactly the failure mode the plan writes four REFUSALS to avoid elsewhere.

- **Claimed:** "Spawn ... z = -38 + (i/(n-1))*76 ±4 clamped to ±46."

  **Actually:** combat.js:143 divides by `Math.max(1, soni - 1)`. The guard is load-bearing for the single noyon in wave 3.

- **Claimed:** "ground TorusMesh 0.55-0.95 at y=0.09" and the pillar triple described as one additive/fog-off/depth-off material set.

  **Actually:** apprentices.js:30-33 uses a flat RingGeometry(0.55, 0.95, 24) at opacity 0.95, and only the outer cylinder uses AdditiveBlending — the core and ring are NormalBlending. Both cylinders are also openEnded + DoubleSide, which Godot's CylinderMesh is not.

- **Claimed:** "HUD MATN 22 keys."

  **Actually:** 26 keys in each language block (hud.js:8-29), including `haliYoq` and `kamonOzimga`, which are the two the plan's mappings never reference.

- **Claimed:** The plan's HUD mapping treats `#kadr` as the ending card only ("the stats card", YakunKadr.tscn).

  **Actually:** `#kadr` is used twice: the 2-second opening narration at main.js:131-134 and the ending stats card at game.js:650. Treating it as ending-only is what loses the opening line.

- **Claimed:** "ArmIk … is exactly what the bow's two-hand anchor and the sieve-shield's forearm pin need."

  **Actually:** Plausible but unverified against the actual need: the web never does two-bone IK at all — weapons are parented to a hand bone (models.js:341) or to a synthesized anchor, and the shield is a child mesh. ArmIk is therefore net-new behaviour, not a reproduction. Harmless if it only replaces the synthesized-anchor fallback, but it should be listed as an addition rather than as fidelity-preserving reuse, and it must not be applied to a rig whose bones resolve to -1 (the WeaponController silent-failure pattern the plan otherwise calls out).

- **Claimed:** "USER, IMMEDIATELY — unblock the assets. Run npx @gltf-transform/cli dedup over all 17 files in src/assets/models/ and copy the results into res://assets/models/buxoro/." Step 1 is framed as the day-one blocker gating everything visual.

  **Actually:** Already done, today. /Users/humoyunochilov/PROJECTS/ChronoShift/assets/models/buxoro/ contains all 17 GLBs with Draco stripped — extensionsRequired is now ['EXT_texture_webp'] only on every file — each with a .import carrying a real uid (e.g. shomurod.glb.import uid://61micrdcvg7q), textures already extracted as .webp siblings, and the imported scenes present in .godot/imported/. Step 1's remaining work is the post-import script, the moljal table and the BoneMap, not the decode.

- **Claimed:** "Transcode from assets/video/_xom/ (the 145 MB originals)", implying every cue has an uncompressed source.

  **Actually:** src/assets/video/_xom/ is 120 MB across 11 files and does not contain kirish.mp4. The menu background exists only as the 1.5 MB already-compressed src/assets/video/kirish.mp4.

- **Claimed:** "five module .tres files already ship a uid/path mismatch that loads coal where the path says wood" (direction stated as coal-for-wood).

  **Actually:** The mismatch is real but inverted: the modules reference uid://woeitm01 (which is item_res_wood.tres) paired with path item_res_coal.tres, and uid://woeitm03 (item_res_coal.tres) paired with path item_res_wood.tres. Godot resolves by uid, so those recipes load WOOD where the path says coal. The lesson the plan draws from it stands; the example is backwards.

## Critic verdicts

**Critic 1.** The plan is unusually complete for a literal port — every system in the web map has a named mapping, there are no deferrals, no "phase 2", and no "handled by existing systems" hand-waves on the big subsystems. All 16 items the brief asked me to verify by name are present and detailed, with the exception of the weapon-not-yet toast (absent) and the opening narration card (absent). But it is not yet a 1:1 plan, and four of the gaps will produce a game that is provably different rather than subtly different: the enemy-spawn formula is transcribed with a divide-by-zero the source guards against, and it silently makes victory unreachable; reusing ChronoShift's jump/run actions breaks arrow-key movement the web supports; routing all 15 synthesised sounds through Sfx.PlayAt adds per-shot pitch jitter and 3D distance falloff to a sound design that has neither; and the dialogue strings contain HTML the plan's CSV-plus-plain-Controls pipeline has no story for, which will either print literal tags or silently drop the emphasis on the word the game is about. The pattern behind most of the rest is the same one the plan itself warns about: three of the six ChronoShift classes it accepts as "genuine reuse" (Sfx, DistanceCull, and arguably ArmIk) were accepted on their name rather than on their defaults, which is precisely the test the plan applies so ruthlessly to WeaponController, PlayerHealth and MissionMap. Apply that same read-the-source test to the reuse list, fix the four blockers, and give homes to the twelve orphaned numbers and behaviours (mouse sensitivity, baseline wind, house placements, the workshop generator, the three-state hand dots), and the plan clears the bar it sets for itself.

**Critic 2.** The plan's source reading is unusually accurate — I spot-checked about thirty of its citations and nearly all hold. Verified correct: balance.js transcription field-for-field including YOLLASH_VAQTI 11.0 vs index.html:108's \"15 s\" and DEVOR_USHLAB_TURISH 6.0; world.js:10 seed 1238 and the LCG formula (max intermediate ~7.15e15 < 2^53, so ulong and JS doubles agree); world.js:215 stone-yields-4; the Object.keys consumption order; the annulus eject-to-r2+r rule and the -X degenerate escape (world.js:615-625); binoTosiq's temporary rotation.y=0 and the s.y<1.0 gate (world.js:588-599); player.js:298-302's move/clamp/solve/clamp order with x clamped to [2,176]; game.js:121-181's tick with the clock decremented first and both terminal checks before any subsystem; apprentices.js:307's `sh.oldingiHolat || HOLAT.BOSH` falsiness (so Holat.Bosh MUST be 0); combat.js's Math.random spawn split; MOLJAL's 19 entries and IKKI_TOMON's 12 verbatim; the empty CHOKISH/TUZATISH/TEKISLA tables; videolar's burchakda mode; main.js's ANIM-first, camera-last, dt-clamp-1/20 loop. Every ChronoShift refusal checks out: WeaponController is 724 lines, PlayerHealth regenerates 7.0/s after 4.0 s and FindChild(\"Spawn\") respawns (PlayerHealth.cs:15,18,100), Enemy.cs:12 is 100 hp, TimeManager.cs:21 drifts 0.02 days/s, MapData.cs:18 GroundSize defaults to 70, MissionMap.cs:26-28 warns and can never finish with zero tasks. The .tres conventions are right (Script ext_resource with no uid, load_steps = 1 + ext, item_res_wood.tres:1-6), and I confirmed empirically what the plan asserts about import naming by decompressing .godot/imported/shomurod.glb-*.scn: animations really do become NlaTrack_001..NlaTrack_007 and bones really do become tripo__Root / tripo__Head_0, with R_Hand/L_Hand present and Armature/Skeleton3D:Hip a valid root-motion path — MANIFEST.md's HAND_R claim is indeed false, and shomurod really measures 0.975 units, not 1.75 m.\n\nWhat the plan gets wrong is concentrated in two places. First, the determinism gate it declares as the stop-the-world precondition is itself under-specified: the LCG stream is consumed by the procedural placeholder geometry inside tugunYarat and terakYarat, and conditionally on which GLB loaded, so the parity dump as written will diverge at the 19th node and block step 2 indefinitely until the harness is rewritten. That is the one finding that should be fixed before file 1 is written, because it changes what Actors.cs and World.cs are FOR — placeholder boxes are load-bearing determinism, not scaffolding. Second, a handful of Godot-surface details were reasoned about rather than checked: AnimationTree.ProcessCallback and the six-hit IntersectRay do not exist in 4.7.1, DepthDrawMode.Never is DEPTH_DRAW_DISABLED, the existing Sfx helper cannot reach the bus layout the plan makes a prerequisite and caps audio at 60 m, nothing creates the poplar StaticBody3Ds the occlusion ray queries, and MeshInstance3D.Transparency is asserted under a project pinned to gl_compatibility without testing it. None of those are structural — each is a localized fix — but four of them land inside step 4, which is the plan's \"PLAYABLE\" milestone and therefore the step least able to absorb surprises. Add the forward-dependency stubs for steps 3/7/8, settle the CLAUDE.md English-identifiers conflict before the ~110 .tres keys are frozen, and tell the user step 1's decode is already done. The architecture — mirror the sim, go native for presentation, every number in .tres — survives all of this intact.

## First step, in detail

Two things happen in parallel on day one. The user starts the asset unblock (mostly waiting on a CLI); Claude writes the determinism kernel, because if these files are wrong every number downstream is wrong and no amount of later work recovers it.

USER, IMMEDIATELY — unblock the assets. In the Torobiy repo run `npx @gltf-transform/cli dedup <in>.glb <out>.glb` over all 17 files in src/assets/models/ and copy the results into res://assets/models/buxoro/ (the set goes 3.66 MB → ~8 MB and imports with zero errors; webp textures need no conversion). While that runs, install a theora-enabled ffmpeg — `brew tap homebrew-ffmpeg/ffmpeg && brew install homebrew-ffmpeg/ffmpeg/ffmpeg --with-theora` — because the stock one on this Mac is decode-only and it is a hard gate ten steps from now.

CLAUDE, FILE 1 — scripts/buxoro/data/BuxoroBalance.cs. `[GlobalClass] public partial class BuxoroBalance : Resource` in namespace ChronoShift.Buxoro. Public [Export] FIELDS in PascalCase, not properties — that is this repo's data-class house style (confirmed against ItemData.cs and item_res_wood.tres) and the .tres key is the field name verbatim, so a property silently breaks every file that sets it. Grouped with [ExportGroup] in balance.js's own section order so a reviewer scrolls the two side by side: VAQT (BoshlangichVaqt 600f, DevorBonus 25f, DevorSeksiyaSoni 6, MaksimumVaqt 720f), SHOMUROD, QUROL, SHOGIRD, TUGUN_SONI flattened to five ints, DUSHMAN/DEVOR, ITTIFOQCHI, VOQEA, MAYDON, then typed `Godot.Collections.Array<T>` refs for the four row types. One XML doc comment per group citing the balance.js line range. Three fields get an explicit honesty comment: BelgilashVaqti (`// balance.js:20 — declared, referenced nowhere. Marking is instantaneous. Transcribed so the table matches.`), YollashVaqti (`// balance.js:44 = 11.0. apprentices.js:371, game.js:225 and index.html:108 all say 15 s and are wrong. The constant is what the code divides by.`) and DevorUshlabTurish (`// 6.0, per SINGLE attacker. combat.js:263's comment says 5 s and is wrong. Six attackers flatten a section in one second.`). Add `public bool Validate(out string error)` asserting the wave rows sum to JamiDushman — 6 + 8 + 10 = 24 — because victory is a kill counter and a silent mismatch makes it unreachable or premature.

CLAUDE, FILE 2 — scripts/buxoro/data/BuxoroSehrli.cs. Same shape. This is the file that repairs README:197's false claim that balance.js is the only source of tuning, and without it the port cannot be faithful, because these are the values that actually shape the feel. Every field carries `/// <summary>world.js:215 — stone nodes yield 4, everything else 2. NOT the obsolete comment above it, which describes 14 nodes at 1 unit and would break the entire Act I economy.</summary>`-style provenance. Full field list per the dataResources entry. Two constraints in the header: every radius is stored UNSQUARED and the comparison sites square it, because the JS writes squared literals (16, 25, 36, 196) and porting those as-is is how a radius silently becomes wrong; and TosiqBalandlikChegi = 1.0 carries `// world.js:596 + combat.js:46. devor_seksiya.glb is ~0.3 m, so built walls are NOT player obstacles — only a visual, an enemy-AI trigger and a time bonus. Deliberate. Do not 'fix'.`

CLAUDE, FILE 3 — scripts/buxoro/core/Lcg.cs. `public struct Lcg` with `private ulong _u;`, `public Lcg(ulong urugh) => _u = urugh;`, `public float Rnd() { _u = (_u * 1664525UL + 1013904223UL) % 4294967296UL; return (float)(_u / 4294967296.0); }` and `public float Orasida(float a, float b) => a + Rnd() * (b - a);`. The doc comment must state WHY ulong and not uint: the intermediate product peaks at ~7.15e15, under 2^53, so JavaScript's double arithmetic and this ulong arithmetic agree bit for bit, while a uint implementation wraps and produces a completely different field — and the entire balance depends on the field matching. Three named seeds as consts: DunyoUrugh = 1238 (world.js:10, verified in source), BinoUrugh = 8317 (binolar.js:16), OtlarUrugh = 4711 (otlar.js:13). A comment records that enemy spawn x/z and node depletion use UNSEEDED randomness in the source (combat.js:139-142 uses Math.random) and must therefore use GD.Randf, not this — the split is deliberate and reproducing it matters.

CLAUDE, FILE 4 — scripts/buxoro/world/BuxoroTerrain.cs. `public static float YerBalandligi(double x, double z) => (float)(Math.Sin(x * 0.09) * 0.16 + Math.Cos(z * 0.11) * 0.14);` — a pure function, no heightmap, no physics floor, no raycast anywhere in this game; every character's Y is sampled from here every tick. Computed in double and returned as float so it matches the JS at the precision the game observes. The file header carries the shipped-bug warning verbatim in English where it cannot be missed: the ground mesh is 680x520 at 110x80 centred at x=120 and each vertex MUST be displaced with its WORLD x (local x + 120), never local x, which reintroduces a ~10.8 rad phase offset and sinks every character knee-deep — world.js:70-76 records this as a real playtest bug and it is the single easiest way to make the port look broken on day one.

CLAUDE, FILE 5 — scripts/buxoro/core/Smoothing.cs. The web idioms as named static methods evaluated at the pinned dt, so no later file ever re-derives a smoothing constant: PosLerp `1 - Pow(1 - 0.12f, dt * 60f)`, FovLerp `Min(1, dt * 3f)`, BodyYaw `1 - Exp(-14f * dt)`, ActorYaw `1 - Pow(1e-6f, dt)`, GaitBlend `Min(1, dt * 7f)`, FogLerp `dt * 0.6f`, ChizmaFade `dt * 2.2f`. The header states which are frame-rate corrected and which are not, and that pinning the sim to 60 Hz is what makes the uncorrected ones land on their authored curves.

CLAUDE, FILE 6 — scripts/buxoro/core/BuxoroRuntime.cs, a Node autoload exposing Balance/Sehrli/World statically with a loud GD.PushError on a null export, because a null balance would otherwise surface as a silently zeroed game. Every Buxoro system reads constants through this and holds none of its own — that is the mechanism that makes the data-first guarantee structural rather than aspirational.

CLAUDE, FILE 7 — scripts/buxoro/debug/ParityDump.cs plus tools/parity/dump_web.js. ParityDump prints, in one CSV block to the console and to user://parity_godot.csv: the first 200 raw Rnd() draws from a fresh Lcg(1238); then, replaying the exact consumption order of world.js (tosh 18 → yogoch 16 → teri 10 → temir 14 → komir 8, each drawing Orasida(minX,maxX) then Orasida(minZ,maxZ)), all 66 node positions as tur,index,x,z; then YerBalandligi at (96,6), (108,1), (152,-22), (148,20), (60,0), (172,0); then every field of both tuning resources by reflection in declaration order. dump_web.js emits the identical three blocks from the browser console against an unmodified world.js, so the user diffs two files rather than comparing screenshots.

Then the .tres files: resources/data/buxoro/balance_buxoro.tres and sehrli_buxoro.tres, hand-written in text format 3 following the repo's exact conventions, which I verified against item_res_wood.tres — `[gd_resource type="Resource" script_class="BuxoroBalance" load_steps=2 format=3 uid="uid://woebux01"]`, blank line, the Script ext_resource carrying NO uid (every other ext_resource type does), blank line, `[resource]` opening with `script = ExtResource("1")`, then every field as `FieldName = value` with `Vector2(96, 6)` syntax, bare floats, enums as bare ints, and `;` comments carrying the balance.js line numbers. load_steps = 1 + ext_resource count.

The step is done when the user runs ParityDump with F6 and the Godot CSV and the browser CSV are byte-identical on the 200 draws and agree to six decimals on all 66 node positions, AND the user has read all ~110 constants beside balance.js. Until that holds: do not write World.Qur, do not write Player.cs, do not touch a scene file — because every balance number in the game assumes that exact field, and a divergence found later has a search space of 6,210 lines instead of five files.
