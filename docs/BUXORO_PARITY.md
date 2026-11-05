# Buxoro 1238 parity checklist

Step 18 of [BUXORO_PORT_PLAN.md](BUXORO_PORT_PLAN.md). One row per mapped system, each marked pass, fail or unverified. There is no "close enough". Approaches that were refused, and differences kept on purpose, are in [BUXORO_REFUSALS.md](BUXORO_REFUSALS.md).

**Where it stands on 2026-11-04.** Of the 47 mapping rows, 3 pass, 6 fail and 38 are unverified. Nobody has played the two builds side by side yet, so every row that needs that stays unverified. The last section lists what you have to play.

The six fails come from reading both sources, not from a run. Each one needs a fix, or your decision to accept it as a deviation:

| Row | Web build | Port |
|---|---|---|
| 7 | All poplars fade together, because they share one material | Only the poplar the camera ray hits fades |
| 11 | Losing window focus keeps the E hold timer | Losing focus zeroes it |
| 28 | `vaqt.` in dialogue t1b is its own smaller, dimmer line, and the clock's hourglass is smaller and faint | `vaqt.` is inline bold, and the hourglass is full size and full opacity |
| 32 | Noise sounds always start at sample 0, so each sounds identical every time | Each starts at a random offset |
| 35 | With no `hunarmand.glb`, a seated box figure stands in | An empty node, so the craftsman is invisible (the shipped GLB hides this) |
| 41 | Losing pointer lock, alt-tab included, opens the pause card | Alt-tab only mutes, and the clock keeps running |

## What the statuses mean

- **pass**: a named check below exercises this system and would exit non-zero if what the row promises stopped matching. Reading the code never earns a pass. A pass makes no claim about how something looks or sounds. Where that still matters, it has its own line in the sign-off list.
- **fail**: reading the web source and the port shows different behaviour that is neither fixed nor named as a deviation. No run produced these. A row can fail on one clause while the rest of it is unverified.
- **unverified**: everything else, including rows a check covers only in part. Those rows say what is proven and what is still open.

## The reference

The reference is the web source in `D:\PROJECTS\Torobiy\.claude\worktrees\torobiy-chronoshift-integration-6f62fe\src`, not the plan's numbers. The web changed after the plan was written, and the port followed it. Where the plan and this table disagree, this table is right:

| What | Plan says | Web and port now | Rows |
|---|---|---|---|
| Recruit hold | 11.0 s | 4.0 s (`balance.js:44`) | 11 |
| Grass instances | 800 / 600 / 120 / 150, total 1670 | 2600 / 1600 / 160 / 320, total 4680 (`otlar.js:20-23`) | 37 |
| Ground colour | `0x685a3e` | `0x4f6a2c` (`world.js:80`) | 3 |
| Horse | Appears at Act III, mounted by holding E | Also: the hero mounts automatically when the first cavalry wave spawns, and no second horse is made | 19 |
| Riders | Box figure or `mogul_otliq` | `mogul_piyoda` seated on the horse with a spear when `mogul_otliq.glb` is missing | 14 |
| GLB set | 17 files | 18 files: `shogird.glb` joined on 2026-09-16 | 34, 44 |
| Jump | 1.079 m, 0.939 s (analytic) | 1.041 m, 0.933 s: what `player.js` integrates per frame at 60 Hz | 6 |

## Named checks

Only these seven count as evidence for a pass. All seven were re-run on 2026-11-04 from the uncommitted working tree, headless, on Godot 4.7.1 .NET, and all exited 0. These results are from the source, not an exe. `PROGRESS.md` records the checks run inside `build/ChronoShift-windows-2026-11-04-2/`, where the copied `--test-sensor` still fails one check because of a test bug fixed afterwards.

Run them from the project root, swapping in the flags from the first column:

```bash
"D:/Tools/Godot_v4.7.1-stable_mono_win64/Godot_v4.7.1-stable_mono_win64_console.exe" --headless --path . scenes/buxoro/BuxoroEpisode.tscn -- --test-harakat
```

| Check | Result today | What it asserts | What it cannot see |
|---|---|---|---|
| `--test-harakat` | Walk 12.000 s, run 8.583 s, jump 1.041 m, airtime 0.933 s | Through the real `Player.Yangila` at 60 Hz: x 96 to 156 walking in 12.0 ±0.05 s and running in 8.571 ±0.05 s, jump peak 1.041 ±0.01 m, airtime 0.933 ±0.02 s | Riding, the arena clamp, body turning, footsteps |
| `--test-oyin` | Pass. The log is below this table | A whole run at 30 Hz with an invulnerable player who only marks four stone nodes. Asserts no exception, all 3 waves spawned, Act III reached, player mounted, at least one delivery, an ending reached | Anything that takes player input: walls, crafting, fighting, the win and death endings. The clock values in its log are printed, not asserted |
| `--test-video` | 10 of 10 checks | In real time: the opening starts at video 1 of 3 and freezes the simulation; Space steps to 2 of 3 and does not reach the opening camera; a video's natural end steps on; the sequence closes; the clock did not move during it and runs after it; on defeat the end card waits for its video; Esc skips all and does not open the pause card | The `tarobiy`, `qilichYasaldi`, `galaba` and `olim` cues, Enter, the SKIP button, corner mode, picture quality |
| `--test-qayta` | 5 of 5 reloads, 12,287 samples each | Five scene reloads, each pushing more samples through the one shared audio generator than its ring buffer holds, then the main menu and a clean exit | What any of it sounds like |
| `--test-sensor` | 56 of 56 checks | Real input pipeline, synthetic fingers. Joystick numbers and run threshold, measured speed at full and half stick, look drag, tap rules (25 m marks, 35 m does not, enemy attacks, empty ground is a plain E), the five buttons and their conditions, hidden buttons pass touches through, two fingers at once, the pause card by touch | A real finger on real hardware. Headless also means no posed skeleton and no cursor capture: `PROGRESS.md` records a windowed run of this test on 2026-11-04, which was not repeated here. The extra `--sensorli` checks were not run today |
| `--solver-compare tools/parity/solver_web.csv` | 72,561 points, 0 over 1e-4, max 3.30e-8 | The obstacle solver over a 0.5 m lattice, using the 75 obstacles World really builds, against the web's own output. Its exit code also requires the draw checkpoints 1080 / 1259 / 1267 / 1474 / 1554, the building stream at 504 and 66 nodes | Node coordinates, heights, anything outside the solver |
| `--glbsiz hammasi --binolar-compare tools/parity/binolar_web.csv` | 319 lines, 0 differences, max 2.38e-6 | Against the real web sources run in Node with no GLB: every mesh of every `binolar.js` generator (bounding box, colour, roughness), their footprints, the 8317 stream after each, the world checkpoints 1404 / 1527 / 1535 / 1697 / 1777, the 8317 stream at 1019, all 93 obstacles | The GLB path, where ring segments end up in the world, anything that is not a mesh box |

`--test-oyin` log (Godot only, nothing here is compared with the web):

| Clock | Beat |
|---|---|
| 480 s | Wave 1, 6 enemies alive; Act II |
| 330 s | Wave 2, 14 alive; player mounted, 3 riders |
| 240 s | Act III |
| 150 s | Wave 3, 24 alive |
| End | Defeat at clock 0, 0 of 24 killed, 4 deliveries, 8 stone in the ombor |

It also passes with `--glbsiz hammasi`: same clocks, 0 riders because the box-figure path runs instead.

Every run prints the known `Anim.cs:74` autoplay warning and an ObjectDB leak warning at exit. Neither changes the exit code.

Not counted, because they are not on the list: `scenes/buxoro/BuxoroImportCheck.tscn` (run today: PASS, 18 models) and `ParityDump` (66 of 66 nodes to 6 decimals against the web on 2026-09-15 per `PROGRESS.md`, not re-run).

## Mapping rows

All 47, in plan order.

| # | System | Status | Evidence | Not covered |
|---|---|---|---|---|
| 1 | Tuning tables — balance.js plus every number hardcoded outside it | unverified | No named check compares the tables with `balance.js`. Every run executes `BuxoroBalance.Tekshir()`: waves sum to 24 | A value-by-value read of `balance_buxoro.tres` and `world_buxoro.tres` beside `balance.js`. `BuxoroSehrli` was never built, so the off-table numbers are literals at their call sites |
| 2 | Deterministic LCG + analytic terrain (world.js:9-27) | unverified | `--solver-compare tools/parity/solver_web.csv` and `--glbsiz hammasi --binolar-compare tools/parity/binolar_web.csv` prove the 1238 and 8317 streams: draw counts match and what they place lands within tolerance | `YerBalandligi` and the 4711 grass stream are in neither check. Nothing asserts that spawns stay off the LCG |
| 3 | Terrain mesh, world generation, lighting, fog (world.js:37-164, 349-491, 533-556) | unverified | The same two checks prove the draw order and the resulting obstacles, with the GLBs (1080 to 1554) and without (1404 to 1777) | Ground mesh, light and fog values, and where poplars, houses, workshop, bazaar, ring segments, Ark and wall points really stand. Fog and light are named deviations with a measured residual: ground 138,120,82 against the web's 138,124,93 |
| 4 | Resource nodes — 5 types, deterministic zones, finite stock (world.js:85-101, 166-218) | unverified | `--solver-compare tools/parity/solver_web.csv`: 66 nodes, stream at 1080 after the node loop. `--test-oyin` log: 4 stone marks gave 4 deliveries and 8 stone | Node coordinates are in neither CSV. Yields of 4 and 2 and hiding a depleted node are not asserted. Coal lumps are a named stand-in shape (REFUSALS V22) |
| 5 | Obstacle registry + toqnashuvHal 3-pass solver (world.js:558-659) | **pass** | `--solver-compare tools/parity/solver_web.csv`: 72,561 points through the 75 real obstacles, 0 over 1e-4, max 3.30e-8. `--glbsiz hammasi --binolar-compare tools/parity/binolar_web.csv`: all 93 obstacles of the GLB-less world match | Adding and removing an obstacle when a wall is built or falls: that is D8. Walking into things is in the sign-off list |
| 6 | Player movement, jump, arena clamp (player.js:206-345) | unverified | `--test-harakat`: walk 12.000 s, run 8.583 s, jump 1.041 m and 0.933 s, so no acceleration. `--test-sensor`: full stick 7.00 m/s, half stick 2.50 m/s | Mounted 10.0 m/s, the arena clamp, solver radius 0.6 when mounted, no jump while mounted, body turn rate, footstep cadence |
| 7 | Third-person camera + occlusion fade (player.js:176-194, 426-467) | **fail** | No named check. By reading: `world.js:222` clones poplars without `materialniKlonla` and `player.js:472-473` writes that shared material, so all fade together. `Player.cs:713` fades only the mesh the ray hit, and the plan's `_terakUmumiySoladi` was not built | Offsets, FOV and lag. Also open: critic MAJOR 12 asks whether `GeometryInstance3D.Transparency` works at all under the project's `gl_compatibility` renderer, and nobody has checked |
| 8 | MARKING mechanic — belgilash, the single verb (apprentices.js:10-41, 151-197) | unverified | `--test-sensor`: tap marks at 25 m, not at 35 m, a marked node does not re-mark, the E button shows ◈ at a node. `--test-oyin`: marks lead to deliveries | The keyboard's 3.0 m radius, the queue cap (D2), the pillar's material and readability at 100 m |
| 9 | Apprentice AI — 5-state FSM, claim-on-idle, flee (apprentices.js:57-369) | unverified | `--test-oyin` asserts at least one delivery; its log shows 4 | The plan's golden trace was never built, so no trip time is measured. Flee oscillation, claim-on-idle, arrival tolerances |
| 10 | Shared storage (ombor) + conversion (apprentices.js:47, 281-291; game.js:253-386) | unverified | `--test-sensor`: the F button follows `Ombor["tosh"]` against the wall recipe. `--test-oyin` log: 8 stone | Spending, the three radii, the fixed sword-first priority, the anvil tick |
| 11 | Hiring craftsmen — hold E, per-target baseline (apprentices.js:371-414; world.js:312-347) | **fail** | No named check hires anyone. By reading: `player.js:128,202,360` leave `eUshlandi` untouched on focus loss, and `BuxoroInput.cs:144-147` zeroes it on the next poll. The plan's `_eHoldSurvivesFocusLoss` was not built. Also `World.cs:597-598` starts the craftsmen's idle by Autoplay, so all six idle in step, while the web's `ANIM.ulash` gives each a random phase | The rest of the row: 4.0 s hold, 3.2 m, per-target baseline, the 3x2 grid, cap of 6 |
| 12 | Defensive wall — 6 sections, HP, build, destroy, unbounded rebuild (world.js:133-141; combat.js:7-74; game.js:253-283) | unverified | `--test-sensor`: F shows at a wall point only with enough stone. Nothing builds a wall | Build, +25 s, the height gate (D8), destruction, rubble, unbounded rebuild, the ±39.6 coverage hole |
| 13 | Clock as the only resource — countdown, +25 s, 720 cap, shift feel (game.js:121-146, 398-403; hud.js:54-71) | unverified | `--test-oyin`: runs from 600 to defeat at 0. `--test-video`: frozen during a video, runs after | +25 s (D3), the 720 cap (D4), the single caller of `VaqtQosh`, the clock jump, flash and arpeggio |
| 14 | Wave table + spawn scheduling + enemy types (balance.js:74-81; game.js:513-539; combat.js:77-152) | unverified | `--test-oyin` asserts all three waves spawned; its log has them at 480, 330 and 150 with 6, 14 and 24 alive and 3 riders. `Tekshir()` checks the cohort sum | The spawn X and Z formula (Correction 1's assertions were not written), per-enemy materials, corpses, the kill-counter victory, a leaker blocking the win |
| 15 | Enemy AI — wall priority, target selection, attack (combat.js:218-310) | unverified | `--test-oyin` runs it for 480 s without an exception. Nothing asserts behaviour | The wall box, cavalry flanking, targeting, attack ranges |
| 16 | Three weapons — club, bow, sword: damage, cooldown, cone, homing arrows (player.js:480-520; game.js:148-160; combat.js:406-423) | unverified | `--test-sensor`: tapping an enemy sets the attack flag, and the weapon button cycles | Damage, cooldowns, the cone (D5), homing arrows (D6) |
| 17 | Shield blocking and player HP (game.js:548-586; player.js:168, 318) | unverified | `--test-sensor`: the block button holds and releases the shield and is gone with the bow | The 0.30 multiplier, 140 HP, no regeneration, the damage vignette |
| 18 | Allied soldiers — spawn, arming, three behaviour modes (combat.js:154-200, 313-400) | unverified | `--test-oyin` spawns and ticks them. Nothing asserts behaviour | All three modes, arming, the no-leash chase, bars |
| 19 | Horse mounting and riding (game.js:502-509, 210-222; player.js:306-312, 570) | unverified | `--test-oyin` asserts the player is mounted; its log has it at 330 s with 3 riders | 10 m/s, the E-hold mount, FOV 78, no dismount, the tip |
| 20 | Three-act state machine — parda I/II/III (game.js:406-510) | unverified | `--test-oyin` asserts Act III; its log has Act II at 480 s and Act III at 240 s. `--test-video`: the `toshTugadi` cue fires when the clock jumps past 480 | The sword unlock, the HUD resource swap, both acts firing on one frame in the `parda3` preset |
| 21 | Tutorial step ladder (game.js:410-431) | unverified | No named check | All of it |
| 22 | Tarobiy — dialogue table, light pillar, death at 4:00 (game.js:7-64, 97-118, 473-510) | unverified | `--test-oyin` passes through his death without an exception. Nothing asserts the beats | The six death beats, line timing, the pillar |
| 23 | Humo bird — the wave siren that replaces a HUD banner (humo.js:1-119; game.js:522-538) | unverified | `--test-oyin` runs every state change without an exception. Nothing asserts positions | Flight paths, timers, readability at 250 m, the fog coupling |
| 24 | Blueprint panel + non-modal 3D corner viewer (chizma.js:1-146; hud.js:95-114) | unverified | No named check. `PROGRESS.md` records one side-by-side render on 2026-10-21 where pose, size, place and colour matched; not counted | The non-modal contract, 7 s and 9 s durations, the strip's states |
| 25 | Crafting flow at the workshop (game.js:285-302, 336-373) | unverified | `--test-sensor`: the F hold timer runs and resets through the button | Recipes, priority, first bow to the player, routing to soldiers |
| 26 | Sword choice — self vs soldier (game.js:304-317, 367-386) | unverified | No named check | All of it. A touch player cannot answer the choice, and the web has the same gap |
| 27 | Tip system (otMaslahat) + contextual prompt labels (game.js:216-222, 242-245, 388-395) | unverified | No named check | All of it |
| 28 | HUD DOM overlay — clock, hands, blueprints, resources, HP, prompt, hold bar, dialogue, toasts (hud.js:36-182) | **fail** | No named check. By reading: `BuxoroHud.cs:639-643` maps `<b>vaqt.</b>` to inline `[b]` inside `[i]` in a RichTextLabel, the approach Correction 4 rejected, while `styles.css:155-156` makes it its own smaller, dimmer block. `BuxoroHud.cs:368` draws the hourglass in the digits' label, while `styles.css:22` gives it 4.2vmin and opacity .75 | Everything else on the HUD, including its look |
| 29 | Floating ally HP bars — 3D→screen projection (game.js:183-198; hud.js:150-168) | unverified | No named check | All of it |
| 30 | UZ/EN runtime language toggle — four scattered dictionaries (hud.js, game.js, menyu.js, main.js, chizma.js) | unverified | No named check. The tables are C# dictionaries, not the plan's CSV | The toggle, a visible line keeping its language, full coverage |
| 31 | Diegetic (in-world) feedback layer (apprentices.js:194; humo.js; game.js:105-179, 575; hud.js:66) | unverified | No named check | All of it (D11) |
| 32 | Procedural WebAudio — 15 SFX, wind bed, 13-note melody, zero sound files (audio.js:1-175) | **fail** | `--test-qayta`: audio survives five reloads and reaches the mixer. By reading: `BuxoroAudio.cs:217` starts each noise SFX at a random offset, while `audio.js:81` always plays from sample 0. Correction 3 names this exact trap | Everything audible. T1 to T6 below |
| 33 | Animation dispatch layer — soft-fail oynat, clip lock, speed match, procedural fallback (anim.js:1-203) | unverified | Every run ticks it; `--test-oyin` also with no GLB. Nothing asserts clip choice | Clip selection, locks, the speed clamp, phase jitter |
| 34 | GLB loader / model registry with silent procedural degradation (models.js:1-428) | unverified | `--glbsiz hammasi --test-oyin`: a whole run with every GLB absent and no exception. `--solver-compare tools/parity/solver_web.csv`: the stream the 18-GLB set produces | Hand attachment, tints, per-instance materials |
| 35 | Procedural placeholder actors (actors.js:1-231) | **fail** | `--glbsiz hammasi --test-oyin` builds the procedural cast without an exception. By reading: `World.cs:592` falls back to an empty `Node3D`, while `world.js:325-331` builds a seated `gumanoid` | Every actor's look. The shipped build never takes the failing branch |
| 36 | Procedural architecture — 7 generators + battered-wall geometry (binolar.js:1-465; world.js:246-491) | **pass** | `--glbsiz hammasi --binolar-compare tools/parity/binolar_web.csv`: 319 lines against the real web sources, 0 differences, max 2.38e-6. Every mesh of `Uy` (both sizes), `UyVayrona`, `BozorRastasi`, `QalaDarvoza`, `ShaharDevori`, `ArkQalasi` and `PaxsaDevor`, their footprints and the 8317 stream after each | Where ring segments stand in the world (row 3), the gate swing (row 40), and the plan's four-camera screenshot compare (sign-off) |
| 37 | Instanced grass / ground cover (otlar.js:1-126) | unverified | No named check. The 4711 stream is not compared | Counts, rejection zones, whether running reads as speed |
| 38 | Procedural weapons — club, sieve-shield, bow, sword, spear, arrow (qurollar.js:1-250) | unverified | No named check | All of it |
| 39 | Cutscene video system — 7 cues, 2 modes, skip semantics (videolar.js:1-147) | unverified | `--test-video`: see its row above. `--test-sensor`: one tap is one step, and the video swallows the tap | Four of the seven cues, Enter, the SKIP button, the once-per-run rule, corner mode (no shipped cue uses it), the counter's text |
| 40 | Endings, death sequence, camera epilogue, stats card (game.js:548-671) | unverified | `--test-oyin` asserts an ending; its log has defeat at clock 0. `--test-video`: the defeat video, then the card, no pause | Win and death, the gate swing, camera moves, wind levels, combat running on, the stats text |
| 41 | Series catalog + two-level menu + mission presets + pause (menyu.js:1-203; main.js:58-249) | **fail** | `--test-sensor`: pause by ❚❚, 5 buttons, BOSHQARUV shows the table, DAVOM ETTIRISH and a backdrop tap resume. `--test-qayta`: reload and return to the main menu. By reading: `main.js:260-264` pauses when pointer lock drops, while `GameDirector.cs:308-319` only mutes, so the clock runs on behind alt-tab | The four presets, the menu pages, the music carve-out (D10) |
| 42 | Touch controls — input synthesizer + adaptive button HUD (touch.js:1-201) | **pass** | `--test-sensor`: all 56 checks. A search of `scripts/buxoro` today finds no platform check, only the capability test in `Touch.Bormi`, which is the web's `ontouchstart` test | A real finger on real hardware and the HUD's touch layout (sign-off). The plan's `Sensor.tscn` was refused on purpose |
| 43 | Frame loop, dt clamp, pause, opening flythrough (main.js:1-405) | unverified | `--test-video`: the simulation freezes in a video and the opening camera carries on under it. `--test-sensor`: a tap skips the opening camera | The dt clamp, the animation-first and camera-last order, the 6 s camera path. Pause on focus loss fails under row 41 |
| 44 | Asset intake — 17 Draco GLBs Godot 4.7 hard-rejects, plus scale, facing, double-sidedness, clip names, loop modes, root motion | unverified | `--solver-compare tools/parity/solver_web.csv` exits non-zero if the draw checkpoints move, and they hold for the imported 18-GLB set. `BuxoroImportCheck` passes with 18 models; not counted | Facing and looping in play. No BoneMap was authored, so there is no Mixamo retargeting |
| 45 | Cutscene media — 12 h264 mp4 vs Godot's Theora-only VideoStreamPlayer | unverified | `--test-video` plays videos 2 and 3 to their natural end and opens 1, 4 and game-over. The 12 `.ogv` files total 36.7 MB | Picture and sound quality. They come from the web's compressed copies at q:v 6, not the originals the plan asked for |
| 46 | Fonts and UI styling — Georgia/serif HUD, vmin sizing, dust-ochre palette | unverified | No named check. The code uses the system font, not the plan's bundled serif | Glyph coverage, proportions at two resolutions, the palette |
| 47 | ChronoShift shell integration — menu entry, autoloads, save | unverified | `--test-qayta`: reload, return to the main menu, clean exit. The click path through the menu was tested once with temporary code on 2026-11-04 and that test is gone; not counted | The menu buttons, autoloads left untouched, no save state |

## Subtle-drift rows

The rows step 18 lists by name, because they catch drift a casual run would miss. All are unverified: no named check exercises any of them. The code column says where to look. It is not evidence.

| ID | Row | Status | Port code | Web code |
|---|---|---|---|---|
| D1 | Stone needs 2 marks | unverified | `Apprentices.cs:415` | `world.js:216`, `balance.js:43`, `apprentices.js:265` |
| D2 | Queue caps at shogird+2 | unverified | `Apprentices.cs:243, 276` | `apprentices.js:151, 169` |
| D3 | Walls push the wave schedule later | unverified | `Game.cs:438, 877` | `game.js:398, 532` |
| D4 | The clock caps at 720 | unverified | `Game.cs:657` | `game.js:398-403` |
| D5 | The melee arc trails the camera | unverified | `Player.cs:209` | `player.js:509-515` |
| D6 | Arrows never miss | unverified | `Combat.cs:696-709` | `combat.js:428-445` |
| D7 | Apprentices clip through the city wall | unverified | `Apprentices.cs:538-553` has no solver call | `apprentices.js:358-369` |
| D8 | The player walks over his own wall | unverified | `Combat.cs:116` | `combat.js:47`, `world.js:603` |
| D9 | Stone disappears from the HUD at Act II | unverified | `Game.cs:763` | `game.js:467` |
| D10 | Music never returns after 240 s | unverified | `Game.cs:804`, `GameDirector.cs:444, 459` | `game.js:497`, `main.js:195, 204` |
| D11 | The bird is the only wave telegraph | unverified | `Game.cs:871-875` | `game.js:544` |

## Final parity

Step 18's closing check. All need both builds played, so all stay unverified. The Godot side of F1 and F3 appears in the `--test-oyin` log above, but only one build has been measured.

| ID | Row | Status |
|---|---|---|
| F1 | Same wave timings | unverified |
| F2 | Same ombor totals at the same clock values | unverified |
| F3 | Same enemy count at 2:30 | unverified |
| F4 | Same ending at the same second | unverified |

## Other rows the plan asks this file to carry

**Positive assertions (Correction 5).**

| ID | Row | Status | Evidence |
|---|---|---|---|
| P1 | "Exactly one tugun_*.glb exists (tugun_tosh). terak.glb exists. If either changes, all 66 node positions change and every number in balance.js must be re-tuned." | **pass** | `--solver-compare tools/parity/solver_web.csv` exits non-zero if the checkpoints move. A listing today shows 18 `.glb` files, one `tugun_*` and `terak`. `shogird.glb` is the newcomer and is not a stream input |
| P2 | Placeholder box counts are determinism: one extra box moves all 66 nodes | **pass** | `--glbsiz hammasi --binolar-compare tools/parity/binolar_web.csv`: the world checkpoints with every placeholder branch running match the web (nodes 1404, poplars 1527, houses 1535, gate 1697, edges 1777) |
| P3 | Gate ring and edge wall draw budget | **pass** | `--solver-compare tools/parity/solver_web.csv`: 1474 and 1554. The plan's conditional "unverified" row for this is closed |

**Quirks, phrased so that a later "fix" fails the checklist (Risks section).** Others are D8 and D9. `BUXORO_REFUSALS.md` lists every kept quirk.

| ID | Row | Status | Notes |
|---|---|---|---|
| Q1 | The whole poplar set fades when one occludes | **fail** | Row 7 |
| Q2 | The Act III banner reads USTA YIQILDI, because `game.js:25` overwrites `:16`, and "Endi senga hech kim aytmaydi." never appears | unverified | `Game.cs:104, 807`. A decision the plan leaves to you: see REFUSALS |
| Q3 | "Yugur!" fires at a walking player, because the threshold is 6 m/s | unverified | `Game.cs:699`, `game.js:424` |
| Q4 | The flee timer never refreshes, giving a 5 s flee, resume, flee cycle | unverified | `Apprentices.cs:319` |
| Q5 | A no-recruit run's stats card reads 2 / 6 | unverified | `Game.cs:1048` |
| Q6 | `Ceil(Vaqt % 60)` can print 4:60 on the win card | unverified | `Game.cs:1040`, `game.js:673` |
| Q7 | The E hold survives focus loss | **fail** | Row 11 |
| Q8 | Any touch skips the opening camera | **pass** | `--test-sensor`: "kadr so'nganda tegish parvozni o'tkazadi" |
| Q9 | A line already on screen keeps its language after the toggle | unverified | `Game.G` resolves when the line is shown |
| Q10 | Pitch input changes the camera not at all | unverified | `--test-sensor` proves pitch is written and clamped, not that the camera ignores it. `Player.cs:653-670` |
| Q11 | A second cue is dropped while one plays, so the `parda3` preset loses video 6 | unverified | `Videolar.cs:141`. Silent, as in the web; the plan's warning was not added |

**Audio (Correction 3).** The planned fixture `BuxoroAudioParity` does not exist, and the design it tests, one `AudioStreamPlayer` per layer on four buses, was not built. As written, T1 and T2 cannot run against one mixed generator.

| ID | Row | Status | Notes |
|---|---|---|---|
| T1 | No 3D player, no jitter, no wrong bus, no double gain | unverified | A search today finds no `Sfx.Play*` and no `AudioStreamPlayer3D` in `scripts/buxoro`. Oscillators carry no random pitch by reading. Not a test |
| T2 | Bus layout numbers | unverified | No bus layout exists. Master 0.55, music 0.30 and wind are maths in `BuxoroAudio.cs:43-45` |
| T3 | Distance invariance | unverified | The mix has no distance term by reading. Not measured |
| T4 | Determinism: identical waveform on every trigger | **fail** | Row 32 |
| T5 | Wind cycle of 11.1 s and a 2.000 s loop | unverified | `BuxoroAudio.cs:442` (2 s buffer), `:387-398` (LFO). Not recorded |
| T6 | Focus mute | unverified | `GameDirector.cs:308-319`, `BuxoroAudio.cs:117-126`. Not measured |

**Markup (Correction 4).**

| ID | Row | Status | Notes |
|---|---|---|---|
| M1 | t1b is four stacked blocks, with `vaqt.` alone on its line, smaller and dimmer | **fail** | Row 28. The word sits inside the sentence, the picture this check exists to catch |
| M2 | Cards: opening 2 lines; victory 5 stat lines, defeat 4; RESTART at the same offset in both | unverified | `Game.cs:1045-1050` builds the stats with no trailing newline. Not measured |
| M3 | No `<b>`, `</b>` or `<br>` ever visible, in either language | unverified | The C.10 startup assertion was not built |
| M4 | The quotes in `"Yugur!"` survive | unverified | The plan's CSV round trip cannot run: there is no CSV. The literal keeps them at `Game.cs:91` |

**Others.**

| ID | Row | Status | Notes |
|---|---|---|---|
| W1 | Pressing 2 before the first bow toasts "KAMON — hali yo'q" and does not switch (critic MAJOR 5) | unverified | `Game.cs:523-530` |
| I1 | Input Map: 15 `buxoro_*` actions, 20 events, no keypad Enter, `jump` and `run` untouched (Correction 2) | unverified | A search of `project.godot` today counts 15 actions. `InputMapTests` was not written |
| S1 | Spawn Z: six distinct values for six piyoda, the lone noyon at -38, a range of ±42, no LCG draws (Correction 1) | unverified | `Combat.cs:308`. The plan says only an automated test catches this; it was not written |

## Tests the plan asked for that were never written

These are why so many rows stay unverified. Writing them would close rows without playing, and the numbers they would need are in the plan.

- `BuxoroTraceHarness` and `tools/parity/trace_capture.js`: the 120 s apprentice golden trace (rows 9, 10)
- `BuxoroAudioParity` and `tools/parity/bake_reference.js`: T1 to T6 (row 32)
- `InputMapTests`: I1
- Correction 1's spawn assertions and their `_tasodif` seam: S1 (row 14)
- The camera offset test, including "pitch changes nothing": Q10 (row 7)
- A terrain and node-table compare in the named set; `ParityDump` still exists and did this once (rows 2, 4)

## Rows you must sign off by playing

The plan's step 18: play one complete run of both builds back to back on all four presets (`toliq`, `parda1`, `parda2`, `parda3`) and walk this list. Tick a row only when the two builds match. If they differ, fix the port or add a named deviation with its reason to `BUXORO_REFUSALS.md`.

Before you start:

- Play the exe in `build/ChronoShift-windows-2026-11-04-2/`. The older `build/ChronoShift-windows-2026-11-04/` has no working pause card and no touch controls. Any fix to a failing row needs another build.
- Start the web build with the `buxoro-win` launch config in the Torobiy worktree.
- To reach a preset fast in Godot, use MISSIYALAR in the Buxoro menu, as you would in the web build.

**Decide these first.** They fail today, so they cannot be ticked until they are fixed or you accept them as deviations.

- [ ] Row 7, Q1: stand so one poplar blocks the camera. In the web every poplar fades. Also confirm any fade shows at all in the Godot build
- [ ] Row 11, Q7: hold E on a craftsman, alt-tab, let go, come back, hold again. In the web the bar continues. Also watch the six craftsmen idle: in the web they are out of step
- [ ] Row 28, M1: read t1b, the second thing Tarobiy says, then look at the clock's hourglass. Compare with the web
- [ ] Row 32, T4: listen to repeated footsteps and anvil hits. In the web each is the identical sound
- [ ] Row 35: start with `-- --buxoro --glbsiz hunarmand` and look at the bazaar. The web shows seated figures in that case
- [ ] Row 41: alt-tab mid-run. In the web the pause card opens and the clock stops

**Final parity.**

- [ ] F1: note the clock at each wave
- [ ] F2: note the ombor at the same clock values, with the same marking routine in both builds
- [ ] F3: count enemies at 2:30
- [ ] F4: note the ending and the second it comes

**Drift rows.**

- [ ] D1: a stone node takes two marks and two round trips; every other type takes one
- [ ] D2: with 2 apprentices the 5th mark is refused with the `rad` sound and a toast, and replaces nothing
- [ ] D3: build a wall between waves; the next wave comes later by the clock
- [ ] D4: build all six early; the clock stops at 12:00 and the last sections are worth less
- [ ] D5: spin the camera fast and swing; the hit arc lags behind
- [ ] D6: shoot a running rider; the arrow curves and hits
- [ ] D7: watch an apprentice's straight line cross walls and houses
- [ ] D8: walk straight over a built section
- [ ] D9: at Act II the readout swaps to wood and leather, and your stone is no longer shown
- [ ] D10: after 4:00, pause and resume; the music stays gone until a win
- [ ] D11: before a wave, the bird and the haze are the only warning, with no counter, marker or banner anywhere

**Mapping rows.**

- [ ] Row 1: no single play check closes this. Read the two `.tres` files beside `balance.js` in one sitting, as plan step 2 asks
- [ ] Row 2: walk the field; feet stay on the ground, including at x=60 and x=152
- [ ] Row 3: compare screenshots from the gate (172,0), the Ark (250,0,10), the bazaar (148,20) and the forge (152,-22): same layout, same ruined segments and hole, same light and haze
- [ ] Row 4: spend a node to zero; it disappears and stays gone
- [ ] Row 5: walk into a house, a poplar, a stall and the gate, then the ring from both sides; you eject smoothly and never stick
- [ ] Row 6: ride, walk into the field's edges, press Space while mounted, turn on the spot, listen to the footstep rhythm
- [ ] Row 8: mark with E just under 3 m; the beam reads at 100 m against the sky
- [ ] Row 9: time round trips: stone about 42.3 s, wood 12.8, leather 25.5, iron 26.8, coal 34.0. Park an enemy on an apprentice and watch the 5 s cycle. Watch a hand dot flip busy and back
- [ ] Row 10: spend each resource; the radii are 4, 5 and 6 m; once the sword blueprint is open the forge offers the sword first
- [ ] Row 12: 4 stone and a 3 s hold give +25 s; one piyoda flattens a section in 6 s and six in about 1 s; rubble can be walked over; rebuilding pays again; a spawn past ±39.6 walks by
- [ ] Row 13: the clock turns red and pulses at 1:00; a wall makes it jump, flash white and play four rising notes
- [ ] Row 14: six footmen spread down the west edge; bodies fall and sink; a leaker at the gate blocks the win
- [ ] Row 15: footmen stop at a wall, riders slide around it, a wave stacks into a column
- [ ] Row 16: one club swing hits everything in the cone for 8; a piyoda takes 7 swings, a noyon 8 sword hits; a bow shot with no target still spends the cooldown
- [ ] Row 17: blocking turns 7 damage into 2.1 from any side at no cost; no regeneration; red vignette; the bar goes red at 42
- [ ] Row 18: unarmed soldiers never advance, archers stay put, a swordsman chases across the field and never returns, arming twice is refused
- [ ] Row 19: mounted at 5:30 with wave 2; FOV widens; no dismount; in `parda3` the horse stands by Tarobiy and takes a 0.7 s hold of E
- [ ] Row 20: the bow blueprint at 8:00, the resource swap, Tarobiy at 4:00, the sword unlock within 6 m of the forge once mounted; in `parda3` both changes on the first frame
- [ ] Row 21: t1a, t1b and t1c timing; standing still 3 s after your first mark brings "Yugur!"; the ladder moves on stone and on the first wall
- [ ] Row 22: the pillar pulses; at 4:00 all six beats happen and the body stays on the field
- [ ] Row 23: at 8:15 the bird circles the dome and the fog thickens; at 8:00 the wide circle; readable from the west edge; on defeat it flies off for good
- [ ] Row 24: bow 7 s, sword 9 s, spin and wobble; keep running while it shows; it freezes during a pause or video
- [ ] Row 25: hold F at the forge, anvil at 6 per second; the first bow is yours with the toast; later bows go to the first unarmed soldier
- [ ] Row 26: 1 is for you, 2 for a soldier, 3 does nothing, weapon keys do not switch while it is up; it comes back with every 3 iron + 1 coal
- [ ] Row 27: the mount tip branches on unbuilt walls; the fallen-wall tip shows once; prompts read like "E — BELGILA — TOSH"
- [ ] Row 29: bars only within 12 m, red at 30 %, none on enemies
- [ ] Row 30: click the corner toggle; a visible line keeps its language until it expires; check the whole HUD in both languages
- [ ] Row 31: the beam, the bird, the pillar and the haze carry the game with no wave counter or marker added
- [ ] Row 33: Shift visibly switches to the run clip; six enemies step out of phase; feet slide at full run, which is correct
- [ ] Row 34: weapons sit in the hands; models stand at the right height; a run with `--glbsiz` degrades as the web does
- [ ] Row 37: running past near tufts reads as speed; no grass on the wall line, forge, bazaar or gate road
- [ ] Row 38: compare club, shield, bow, sword, spear and arrow
- [ ] Row 39: click steps one clip, SKIP abandons the sequence, Enter steps, each cue plays once, `parda3` loses video 6
- [ ] Row 40: all three endings: headline, gate swing on defeat, camera, wind, music at 1.35 on a win, combat running over your body on death, video at 3.5 s, then the card
- [ ] Row 42: a real finger on a touchscreen (`-- --buxoro --sensorli` makes the mouse one finger if you have none), and the HUD's touch layout
- [ ] Row 43: the 6 s opening camera runs under videos 1 to 3; at a very low frame rate the game goes slow rather than jumping
- [ ] Row 44: every model faces and loops correctly; no hip drift
- [ ] Row 45: watch clip 7, the dust and smoke, fullscreen for ringing
- [ ] Row 46: every glyph draws, including U+2018, ⌾ and ⏳; proportions match at 1920x1080 and 1280x720
- [ ] Row 47: main menu to the Buxoro menu to the game, SERIYA MENYUSI back, and no money or XP moved

The remaining rows, to be checked in the same runs.

- [ ] Row 36: the four-camera compare from Row 3 also covers these buildings
- [ ] Q2 to Q6, Q9 to Q11, W1: as worded in their rows
- [ ] T1 to T3, T5, T6: by ear, until `BuxoroAudioParity` exists. The wind breathes on an 11 s cycle, every sound is at full level anywhere on the field, and the game goes silent when the window loses focus
- [ ] M2 to M4: the opening card, a win card and a defeat card, in each language
- [ ] I1: Correction 2's key matrix: arrows move and do nothing else, Space and Enter step a video, Esc closes a video and does not pause
- [ ] S1: look down the west edge at wave 1; six footmen about 15 m apart, not bunched
