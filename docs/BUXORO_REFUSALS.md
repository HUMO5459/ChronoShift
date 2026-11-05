# Buxoro 1238 refusals and deviations

Step 18 of [BUXORO_PORT_PLAN.md](BUXORO_PORT_PLAN.md) leaves two outcomes for any difference from the web build: "fixed or written down as a named, deliberate deviation with a reason. There is no third category." This is where they are written down, along with every approach the plan or the code refused. Read the reason before you "fix" anything here: many of these look like mistakes and are not.

Status of each system is in [BUXORO_PARITY.md](BUXORO_PARITY.md). Line numbers are as of 2026-11-04. Where no reason was recorded, this file says so rather than supplying one.

## Refused approaches the code has today

The plan refuses these three, yet the port does them. Nothing in the code or `PROGRESS.md` gives a reason, so they are not deviations. They are `fail` rows in the parity checklist, waiting to be fixed or accepted.

| Refused | Refused where | What the code does | Parity row |
|---|---|---|---|
| `[b]vaqt.[/b]` in a RichTextLabel. It renders inline, italic, full size and full opacity, which inverts the emphasis. The web's `#gap b` is `display: block`, smaller and dimmer | Correction 4, C.3 | `BuxoroHud.cs:639-643` maps `<b>` to `[b]` inside `[i]` in a RichTextLabel | 28, M1 |
| A random offset into the noise buffer. `audio.js:81` always starts at sample 0, so each noise sound is bit-identical on every trigger | Correction 3, §4 | `BuxoroAudio.cs:217`: `ShovqinIdx = GD.Randi() % ...` | 32, T4 |
| "Fixing" the E hold so that focus loss resets it. This was one of data-first's three rejected fixes | Mapping row "Hiring craftsmen"; "Ideas grafted", last line | `BuxoroInput.cs:144-147` zeroes the timer on the next poll after focus loss | 11, Q7 |

## 1. The five written refusals

Numbered as in the plan. Each names what it would break.

| # | Refused | Reason and what it would change | Recorded at |
|---|---|---|---|
| 1 | `move_and_slide` world collision and Godot depenetration | Neither reproduces the three solver rules: three passes with the `surildi` early-out so a corner does not cancel itself; the annulus interior is solid and ejects outward to r2 + r (49.97 + 0.45 on foot), which is the fix for the gate oscillation; degenerate centres escape along a fixed axis. Both would also give apprentices and enemies a collision they have never had: `toqnashuvHal` has exactly one caller, the player | `BuxoroSolver.cs:55`, `Player.cs:67`. The code goes further than the plan, which kept a `CharacterBody3D` with an empty mask: there is no body at all |
| 2 | `SpringArm3D` | It pulls in on collision, and the web camera never does. Offsets stay fixed: back 6.0, up 4.5 on foot; back 8.0, up 5.5 mounted | Mapping row "Third-person camera". No comment at the site |
| 3 | `NavigationAgent3D` on enemies and apprentices, and any `CollisionShape3D` on them | Travel times are the economy: a stone round trip is 42.3 s at the 1238 seed and a full wall is 12 trips, about 508 apprentice-seconds against 120 s of Act I, which is what makes recruiting mandatory. Enemies are blocked by a coordinate box, not a collider, and pathfinding would route them around the wall line, deleting the wall's meaning and the +25 s economy | `Apprentices.cs:74`; mapping rows "Apprentice AI" and "Enemy AI" |
| 4 | `TimeManager` for the clock | It counts up at 0.02 days/s (`TimeManager.cs:21`). Buxoro's clock is one float that counts down from 600, jumps back 25 s, caps at 720 and drives acts and waves by its value | Mapping row "Clock as the only resource". No comment at the site |
| 5 | `Sfx.PlayAt`, and any `AudioStreamPlayer3D` in the episode | `PlayAt` detunes ±8 % per shot (`Sfx.cs:17-18, 75-76`) and attenuates at `MaxDistance` 60 (`Sfx.cs:25-34`); neither helper sets a bus. The web has one hop and nothing else (`audio.js:72`). `belgi` would stop being the same note twice, the `vaqt` arpeggio would be out of tune with itself, the far half of the battle would go silent, and the bird's cry at 250 m would be inaudible. Passing `pitchSpread: 0.0f` is not a fix: it leaves the attenuation. `Sfx.PlayUi` is exempt from the refusal but still unused, because it cannot set a bus, schedule layer delays or be gated by the focus mute. Do not add `bus` or `maxDistance` to `Sfx.cs`: the main game calls it at nine sites. Adding "a few footstep variations" through `Sfx.Pick` would also be a fidelity break: the web plays the same `qadam` forever | Correction 3, §1. A search finds no `Sfx.Play*` or `AudioStreamPlayer3D` in `scripts/buxoro`. The header comment the plan asked for in `BuxoroAudio.cs` is not there |

The plan asked for each of these as a file-header comment at its site. Only 1 and 3 have one. Searching `scripts/buxoro` for `REFUSED` gives those sites, `Touch.cs:35` (section 3), and `Combat.cs:358`, which is a gameplay rule rather than an approach: a soldier who is already armed cannot be armed again.

## 2. ChronoShift systems refused

Each fails the plan's test: can it produce the same behaviour and the same numbers? None is used in `scripts/buxoro`.

| Refused | Reason |
|---|---|
| `InventoryManager` for the ombor | A string-keyed, save-backed campaign catalog. Wiring it in would let a mid-run save resurrect a dead clock. The ombor is five integers with no capacity |
| `Enemy.cs` | 100 hp, detect 17, attack 11, fire interval 2.4, no wall rule, no mounted variant |
| `WeaponController` | 724 lines of hitscan with magazines, reload, spread and scope, and Nathan-specific bone names in `[Export]` defaults: `FindBone` returns -1 and every consumer silently returns |
| `PlayerHealth.cs` | 7 hp/s regeneration after 4 s, a 2 s spawn grace and a `FindChild("Spawn")` teleport respawn would make a 10-minute run unlosable |
| `PlayerController` | Stamina, `MoveToward` acceleration and the `TechTreeManager` speed multiplier contradict the web's instant start and stop at 5.0 and 7.0 |
| `Vehicle.cs` for the horse | Enter hides the driver, disables its process mode and collision, and swaps to a second camera rig through hardcoded NodePaths. The web does none of that: the rider stays visible and the horse is a transform copy |
| `Workshop.cs`, `WorkshopPanel` | A tree-pausing standing-order queue with a progress bar and an `IsBusy` gate, against a non-pausing proximity hold with a fixed priority and no panel |
| `GenericMissionMap`, `MapData`, `MissionMap`, `MapLoader`, `MissionManager` | `MissionMap` warns and can never finish with zero `EngineeringTask`s, so routing through it forces a fake completion task. `MapData` cannot express a 180x100 field, a 46 m ring, six rotated wall points or analytic terrain, and extending it would change the contract all 10 shipped maps load. Buxoro wins on a kill counter and loses on a clock. Both runner-up designs routed through it and had to invent that fake task |
| `HUD.cs` | 846 lines of money, XP, ammo and vehicle hull in a different art direction |
| `ObjectiveMarker.cs` | Explicitly not instantiated. The bird and the haze are the wave warning, and a marker breaks the design even if nothing looks wrong |
| Reskinning `woe_theme.tres` | Blueprint blue and dust ochre are different art directions for different episodes |
| ChronoShift's `move_*`, `jump`, `run`, `interact`, `weapon_*` actions | `jump` includes the Up arrow and `run` the Down arrow (`project.godot`), while the web's arrows only walk. So Buxoro declares its own 15 `buxoro_*` actions and reads nothing else. Do not edit the shared ones either: see "critic's fixes" below |
| The seven autoloads for gameplay | Buxoro has no money, XP, inventory or tech tree. They stay loaded and idle |
| `DistanceCull` | Listed as reuse, then challenged by critic MAJOR 4: its 90 m default culls the bird, which must read at about 250 m and is the only wave telegraph. Not used |
| `ArmIk` | Listed as reuse, then challenged by the critics: the web never does two-bone IK, so it would be new behaviour rather than fidelity. Not used |

## 3. Godot features and shortcuts refused

| Refused | Reason | Recorded at |
|---|---|---|
| `Sensor.tscn` with `TouchScreenButton`s | They read the touch in `_input` and leave it in the pipeline, so the same finger would also start the joystick. The web's `e.stopPropagation()` is what a `MouseFilter.Stop` control does, so the buttons are Controls built in code | `Touch.cs:35-37`, `PROGRESS.md`. The plan's step 18 asked for this scene |
| `Area3D` for melee | A code cone against body yaw: every enemy in ±70° takes full damage with no falloff and no cap, and the arc trails the camera | Mapping row "Three weapons" |
| `RigidBody3D` for arrows | A ballistic projectile would silently nerf the player's bow and all eight archers. Arrows home at 42 m/s and cannot miss within their 2.2 s life | Same row |
| Godot's exponential fog | `FogExp2` is 1 - exp(-(d·x)²) and Godot's is 1 - exp(-d·x), which would haze the near field twice as much. See deviation V2 | `World.cs:272-275`. The plan's row named this fog mode |
| A `uint` LCG | It wraps into a completely different field. The product peaks at about 7.15e15, under 2^53, so JS doubles and a 64-bit integer agree bit for bit | `Lcg.cs:17` |
| Spawning from a world LCG | The web uses `Math.random` at `combat.js:137, 155, 163`. One extra draw from the shared stream shifts every later node. Wave layouts differ every run in both builds, on purpose | `Combat.cs:58-59` |
| Baked wind | Baking loses the breathing: the band-pass moves live at 0.09 Hz | Mapping row "Procedural WebAudio" |
| Baked music | `PitchScale` on a baked loop would transpose the victory reprise. It must change tempo at 1.35 and not pitch | Same row |
| Baked SFX as the shipping path | It abandons the literal zero-audio-files property and adds a Node toolchain. Baking was kept only as a test oracle, which was never built | "Ideas grafted" |
| A 20 s wind loop | The plan's own error: `audio.js:36` is 2 s, shared with the SFX, and the texture differs | Correction 3, §5. The code has 2 s |
| A voice cap, same-sound cooldown or distance cull | The web creates unbounded overlapping sounds. Each of these is a behaviour change | Correction 3, §3 |
| A per-sound `MaxDistance` table | Critic finding 13's fix, cancelled: with no 3D players it would document behaviour that does not exist | Correction 3 |
| Importing the raw `_xom/` GLBs | 1.86 to 1.99 million triangles each, against an 8k budget | Risks |
| `buxoro_til`, an L key for language | The web toggles on a click (`hud.js:46`). An added binding is as much a deviation as a dropped one | Correction 2 |
| Keypad Enter on videos | The web's `e.code === 'Enter'` does not match the numpad | Correction 2 |
| Erasing Godot's `ui_*` defaults | The menus need them. HUD Controls get `FocusMode.None` instead | Correction 2, `BuxoroHud.cs:17-20` |
| An empty `ru` column | Godot would resolve every key to an empty string and blank the UI. Moot: there is no CSV | Critic MINOR 8 |
| The data-first design's three deliberate fixes | E-hold accumulation, the touch intro skip and live locale retranslation were rejected in favour of reproducing the web. The intro skip and the locale behaviour are kept (Q8, Q9). The E hold is not: see the top of this file | "Ideas grafted", last line |

**The critics' fixes, refused by the corrections that re-checked them:**

| Critic's fix | Why it was refused |
|---|---|
| Arrows added to the shared `move_*` actions | Up arrow would fire `move_forward` and `jump` in ChronoShift's own scenes, and Down would fire `move_back` and `run`. It fixes Buxoro by breaking the host |
| `z = -38f + (i / Mathf.Max(1, soni - 1)) * 76f` | `Mathf.Max(int, int)` makes this integer division: five of six wave-1 footmen stack at z = -38, silently. Its claim that the source yields NaN was also false: `Math.max(1, soni - 1)` is the source's own guard. `Combat.cs:308` casts |
| `Sfx.PlayAt(..., pitchSpread: 0.0f)` | Leaves the 3D attenuation in force. See refusal 5 |
| `[b]` and `\n` in a RichTextLabel for dialogue and cards | See the top of this file |
| A parity dump that draws only x and z per node | The world stream is also consumed inside placeholder geometry. Node 20 would be wrong by 9.7 m and everything after it |

## 4. Deliberate deviations from the web build

These differ from the web on purpose, or because the engine leaves no choice. Each has its reason.

| # | Web | Port | Reason | Where |
|---|---|---|---|---|
| V1 | H.264 `.mp4` | Theora `.ogv`, 36.7 MB, converted from the web's own 1280x720 copies at q:v 6 and q:a 4. The plan asked for q:v 7 from the originals | Godot plays only Theora. The script records "no visible difference from the source", judged by eye on 2026-10-21. No reason is recorded for skipping the originals, and the plan warned about generation loss on dust and smoke | `tools/video-ogv.sh`, `Videolar.cs:15-17` |
| V2 | `FogExp2` | Depth fog: end = 1.65 / density (220 m), curve 1.5 | Godot has no exp2 fog. Fog fraction is 0.11 / 0.31 / 0.56 / 0.80 at 50 / 100 / 150 / 190 m against the web's 0.13 / 0.43 / 0.72 / 0.87. No single power curve has exp2's S-shape, and this keeps the near field and the Ark backdrop within 0.15 | `BuxoroWorld.cs:36-40, 56-65`, `World.cs:272-285` |
| V3 | Sun 0.85, ambient 0.38 | Both scaled by `YoruglikKoef` 0.7 | In `gl_compatibility`, sun alone and ambient alone each match three.js, but together render about 2.3x brighter, and why was never found. Measured ground: 138,120,82 against the web's 138,124,93. `PROGRESS.md` left the remaining yellow cast for step 12 and records no later measurement. Re-measure if the renderer or Godot changes | `BuxoroWorld.cs:46-54` |
| V4 | ACES at exposure 0.86; clear colour and fog blended after tone mapping | Exposure converted, `TonemapWhite` 1000, and sky and fog colours pre-inverted through the curve | Different bias and white handling in the same fitted curve. Within 1.7 %. Sky lands exactly on 216,204,180 | `BuxoroEkran.cs:10-21` |
| V5 | Rescale, foot drop, double-sidedness, clip names and root-motion strip at load | Done once at import by `BuxoroImport` | "The one deliberate structural difference": same result, no per-instance cost | `BuxoroImport.cs:16-24` |
| V6 | `frustumCulled = false` on skinned meshes | Their bounding box is padded by 4.0 | Godot has no such switch | `BuxoroImport.cs:58-64` |
| V7 | Ring segment count, gate angle, ring radii and building footprints measured from `Box3` at load | Frozen in `world_buxoro.tres` from the web's values, with only a 0.05 m assert against the imported model | Godot's importer lands about 3e-5 m off `Box3`, and a point grazing a house corner amplifies that 50x. A changed segment count would shift every later draw. With a GLB missing, they are measured as in the web | `BuxoroWorld.cs:9-15, 83-128`, `World.cs:671` |
| V8 | Second scene in a scissored corner of the canvas; `HemisphereLight` | `SubViewport` with its own `World3D`; hemisphere as ambient plus directional lights | Godot has no hemisphere light. For the viewport: no scissor or CSS-pixel juggling | `Chizma.cs:14-19, 111` |
| V9 | If pointer lock is refused: cursor-delta look at 0.0042 and 0.0032, plus screen-edge turning at 2.4 rad/s | Only the locked path | It exists for iframes that refuse the lock, and a desktop build always gets the capture. The critics required this to be written down rather than dropped silently | `BuxoroInput.cs:34-40`, critic MAJOR 8 |
| V10 | After resume the player clicks to re-lock | Resume recaptures the mouse at once, except on touch | Godot has no `pointerlockchange`, and exports do not release capture on Escape, so the pause path owns it | `GameDirector.cs:464-467`, Correction 2 |
| V11 | Sound mutes when the tab is hidden (`visibilitychange`) | Sound mutes on window focus loss | A desktop window has no hidden tab. The plan named this mapping | `GameDirector.cs:308-319`, Correction 3, §7 |
| V12 | Outer level is its own "CHRONOSHIFT" catalogue page of 5 rows, 2 locked | ChronoShift's `MainMenu` is the outer level, and its "1-BO'LIM — BUXORO 1238" button opens the Buxoro menu. The 5-row data-driven catalogue was not built | `MainMenu` "already is that blueprint sheet" | `Menyu.cs:12-15`, `PROGRESS.md` |
| V13 | QAYTA BOSHLASH, SERIYA MENYUSI and the end card's restart all call `location.reload()`, which lands on the menu | Restart reloads the episode at once, on the same preset. SERIYA MENYUSI goes to `MainMenu` | The plan mapped `location.reload()` to a scene reload. Landing directly in the game, rather than on a menu, is a difference the plan did not discuss | `GameDirector.cs:549, 557`, `BuxoroHud.cs:781` |
| V14 | O'YINDAN CHIQISH shows only under Electron | Always shown; a real quit | "Desktop build". This is the web's Electron branch | `GameDirector.cs:558`, `main.js:237-242` |
| V15 | BOSHLASH waits for GLB downloads | The same wait covers a threaded preload of the same list, ending in "18 model · qolgan 17 tasi protsedural" | Nothing to download | `Menyu.cs:16-17` |
| V16 | `'ontouchstart' in window` or `maxTouchPoints` | Up front on phones and tablets; on a Windows touchscreen, from its first real touch | Godot cannot tell in advance on Windows | `Touch.cs:66-71` |
| V17 | Held touch buttons write `p.tugmalar` once, on the event | `BuxoroInput.SensorE`, `SensorF` and `SensorBlok` join the per-frame poll | `Oqi` rewrites those fields every frame, so a one-time write would be lost. No visible change | `BuxoroInput.cs:49-54` |
| V18 | `?skip`, `?t=`, `?shogird=`, `?uysiz=1` in the URL | `--skip`, `--t`, `--shogird` on the command line. `?uysiz=1` has no flag, only the `BuxoroWorld.Uylar` field. New: `--missiya`, `--glbsiz`, `--sensorli` | No URL. The new flags are test hooks | `GameDirector.cs:13-20` |
| V19 | WebAudio nodes at the context's sample rate | One `AudioStreamGenerator` at 44,100 Hz mixed in C#: the same oscillators, biquads and ramps as maths | Keeps zero sound files, and tempo-without-pitch for the reprise. See also section 5 | `BuxoroAudio.cs:7-22` |
| V20 | Not applicable | The generator is never freed for the life of the process, and the GC is stopped at exit | Engine workarounds. Godot's playback keeps a raw pointer to its generator, a use-after-free measured at 3 of 33 exits. A finalizer running during shutdown crashed 1 of 26 exits. After both: `--test-harakat` 100 of 100, `--test-oyin` 30 of 30 clean, per `PROGRESS.md` | `BuxoroAudio.cs:29-37`, `BuxoroRuntime.cs:97-120` |
| V21 | The opening card hides 2 s after it appears | Entering through the menu, the 2 s timer can run out during loading, because `Kechik` counts the first frame's delta | Left unfixed on purpose: the card sits under the opening videos, so the player cannot see the difference | `GameDirector.cs:226, 330`, `PROGRESS.md` |
| V22 | `DodecahedronGeometry(r, 0)` for coal lumps and for the rubble in ruined ring segments | A 6x3 low-poly sphere of the same radius. These are on the shipping path: coal has no GLB, and the rubble is generated | Recorded only as "Visual only": the draws that size and place it are unchanged, so the stream is safe. The shape does differ on screen | `Actors.cs:196-199`, `World.cs:430, 739, 764` |
| V23 | Ground normals from `computeVertexNormals` | The analytic normal of `YerBalandligi` | "What computeVertexNormals approximates". Not measured against the web | `World.cs:323` |

## 5. Plan approaches the code did not take

These explain why files and names in the plan do not exist. So far as anyone has checked they change no web behaviour, apart from the three at the top of this file and the unbuilt poplar and pause rules (parity rows 7 and 41). Most have no recorded reason.

| Plan | Code | Reason recorded |
|---|---|---|
| `CharacterBody3D` + `MoveAndSlide` | Direct position integration on a plain `Oyinchi` object | `Player.cs:67`: refusal 1 |
| Whole simulation in `_PhysicsProcess` at a fixed 60 Hz with physics interpolation | One tick per rendered frame in `_Process`, dt clamped to 1/20 | `GameDirector.cs:244-247`: "so the web's per-frame idioms behave as in the browser". `Smoothing.cs:19-23` still says 60 ticks are required, and `[physics]` remains in `project.godot` |
| `BuxoroSehrli`, `sehrli_buxoro.tres`, "no episode C# file holds a gameplay literal" | Off-table numbers are literals at their call sites, mostly with a JS line cited | None. `BuxoroBalance.cs:14` still points at the missing class |
| `buxoro.csv`, `.translation` files, `TranslationServer` | C# dictionaries in `Game.cs:84` and `BuxoroHud.cs:27`, own language field | None. Side effects: the second `olim` string does not ship, and the blueprint caption stays Uzbek-only, as in the web |
| `BuxoroMarkup.cs`, a `Gap` `VBoxContainer` of 4 Labels, `LabelSettings`, card Labels, the C.10 assertion | RichTextLabels with BBCode for dialogue and card text | None. See the top of this file |
| `default_bus_layout.tres` with four buses, `WavSynth`, `Chal()`, one player per layer | One generator with gains as maths | `BuxoroAudio.cs:7-18` describes it. No comparison with the plan |
| `buxoro_theme.tres`, a bundled serif (EB Garamond or Gelasio) | `SystemFont` naming Georgia, Times New Roman, serif | `BuxoroHud.cs:18`: the serif the web names. A machine without Georgia gets a fallback |
| `AnimationTree`, `StateMachine`, `BlendSpace1D`, `OneShot`, `ActorClipSet`, one `BoneMap` | `AnimationPlayer` on manual processing, `anim.js` ported as written; no BoneMap | `Anim.cs:44-52` describes it |
| Scene files: `Shomurod`, `Shogird`, `Belgi`, `Dushman`, `Askar`, `Humo`, `BuxoroHud`, `ChizmaKorik`, `Videolar`, `Menyu`, `YakunKadr`, `Sensor` | Built in code; `BuxoroMenyu.tscn` and `BuxoroEpisode.tscn` are the game's scenes | None, except `Sensor` |
| `.tres` rows per cue, mission, series row, caption, sound and palette; `bino_specs.tres` | C# tables at the mirror site | None |
| File names `ApprenticeSystem`, `CombatSystem`, `Oq`, `Models`, `BuxoroMain` | `Apprentices`, `Combat`, `BuxoroModels`, `GameDirector` | None |
| Ring as one `MultiMeshInstance3D` per variant | Ordinary nodes; only grass uses MultiMesh | None |
| `tools/buxoro_models.sh` | `Torobiy/tools/godot-eksport.js` | `PROGRESS.md`: it did the same job and shares the name map and triangle budget |
| `GD.PushWarning` naming a dropped video | Silent, as in the web | None |
| Named switches `_terakUmumiySoladi`, `_gapOlimTuzatilgan`, `_eHoldSurvivesFocusLoss`, `_sensorOchilishniOtkazadi`, `_yordamTarjimaQoshildi`, `_pitchYoqilgan`; constant `TurishChegi` | None exists. The behaviours are plain code as in the web, or fail (Q1, Q7) | None |
| "Pointer-lock-loss-as-pause is replaced by window focus loss" | Not built: focus loss only mutes and clears input | None. Parity row 41 |
| The whole poplar set fading when one occludes | Only the hit mesh fades | None. Parity row 7 |
| `ModelDresser`, `ResourceFolder`, `Fx` reuse | Not used | None |
| Test fixtures | Not written: see the parity checklist | None |

## 6. Web-only behaviour not ported

| Web | Why it is gone |
|---|---|
| `fileOgohlantirish()`, `main.js:349-368`: 10 lines of `<b>`, `<br>`, `<code>` saying a browser cannot open the game over `file://` | It fires only when `location.protocol === 'file:'`, and Godot has no such state. Recorded here so a later string audit does not "find a missing string" and port a browser error into a desktop game (Correction 4, C.9) |
| iframe scroll and context-menu suppression, `preventDefault`, `passive: false` | Browser concerns |
| The muted-autoplay retry chain in `videolar.js:118-121` | Browser policy |
| The user-gesture `AudioContext` gate | Browser policy. Music and wind start with the episode |
| `void el.offsetWidth` to restart a CSS animation | Tweens restart directly |
| The dead sprite `ikonka` (`apprentices.js:122-129`), `p.tezlikVec`, `MUHIT.qishloqFon` | Declared and read by nothing. `belgiMesh` and `band`, also dead, are still declared at `World.cs:18-19` |

## 7. Web quirks kept 1:1

They look like bugs, but they are what the web does, so they stay. Fixing one is a deviation and belongs in section 4 first.

**From the 2026-11-04 notes in `PROGRESS.md`:**

| Quirk | Where | Notes |
|---|---|---|
| `qiyaBlok`'s indices are wound inside out, so from outside you see the far walls | `binolar.js:52-70`, `Binolar.cs:71-73` | Only visible with a GLB missing |
| The procedural city wall's long axis runs along Z while the ring's tangent is X, so a GLB-less ring stands crosswise | `Binolar.cs:344` | Only visible with `shahar_devori.glb` missing |
| The joystick ring sits off the finger on a large screen, because sizes cap in px while margins stay vmin | `Touch.cs:562-563` | |
| A touch player cannot answer the sword choice, because it needs the 1 and 2 keys | `Game.cs:485-499` | Decision below |
| `scale *= 1.62 / 1.35` also applies to `shogird.glb`, so apprentices stand about 1.94 m | `apprentices.js:90`, `Apprentices.cs:193-194` | `PROGRESS.md` says to tell you. This is that |

**From the plan:**

| Quirk | Where | Why it stays |
|---|---|---|
| A built wall (about 0.3 m) is walked over, not an obstacle | `Combat.cs:114-116` | A visual, an enemy trigger and a time bonus. D8 |
| `game.js:25` overwrites `:16`, so Act III's banner reads USTA YIQILDI and "Endi senga hech kim aytmaydi." never appears | `Game.cs:103-104` | Decision below |
| "Yugur!" fires at a walking player: 6 m/s threshold against a 5.0 walk | `Game.cs:699` | Q3 |
| Stone vanishes from the HUD at Act II, while the Act III tip tells you to spend it | `Game.cs:763` | D9 |
| The flee timer does not refresh: a 5 s flee, resume, flee cycle | `Apprentices.cs:319` | Q4. Apprentices take no damage anywhere; the flee is theatre |
| A mark is used up per trip, and a trip takes 2 units, so a stone node needs two marks | `Apprentices.cs:415, 427` | The main hidden cost. D1 |
| The stats card counts the 2 starters as recruits: 2 / 6 | `Game.cs:1048` | Q5 |
| `Ceil(Vaqt % 60)` can print 4:60 | `Game.cs:1040` | Q6 |
| Any touch skips the opening camera, so it never plays on a touch device | `Touch.cs:295` | Q8 |
| A line on screen keeps its language after the toggle | `Game.G` | Q9 |
| A cue fired during another is dropped, so `parda3` loses video 6 | `Videolar.cs:141` | Q11 |
| The camera ignores pitch: `Oyinchi.Pitch` is written and clamped, read by nothing | `Player.cs:35, 650-652` | Q10. Adding pitch would change every combat sightline |
| Marking reaches 30 m by tap, 3 m by key | `Touch.cs:51` | On purpose in `touch.js` |
| Only 4 of 6 craftsmen can be hired, because the cap of 6 includes the 2 starters | `Apprentices.cs:580` | The other two stay scenery |
| The sword outranks everything at the forge, so no more bows after that point | `Game.cs:568` | |
| Each sword is re-offered at 3 iron + 1 coal and changes no ending text | `Game.cs:613-643` | |
| A swordsman has no leash and never comes home | `Combat.cs:621-623` | |
| Victory is the kill counter, so a leaker at the gate blocks the win for good | `Game.cs:220` | |
| Wall coverage stops at ±39.6 and spawns reach ±42, so about 30 % of edge enemies walk past. The ±46 clamp never binds | `Combat.cs:308-309, 458` | |
| Rebuilds are unbounded, each pays +25 s, and the card can read 8 / 6 | `Game.cs:437-438` | |
| The 720 cap binds: 150 s of bonus against 120 s of headroom | `Game.cs:657` | D4 |
| `Buzilgan` index 3 is dead: the east arc skips it first | `BuxoroWorld.cs:102-106` | |
| `BELGILASH_VAQTI` is declared and read by nothing | `BuxoroBalance.cs:53` | Marking is instant |
| Tarobiy's timed lines use wall-clock timers, so they can play behind a video or the pause card | `Game.cs:677-690`, `GameDirector.cs:226` | |
| Dead soldier meshes stay on the field | `Game.cs:938-952` | |
| Feet slide at full run: the clip speed clamp is [0.55, 2.4] | `Anim.cs:57` | An art call recorded at `anim.js:90-96` |
| The controls table is Uzbek-only even in EN mode | `BoshqaruvJadvali.cs:39` | Decision below |
| The blueprint caption "Bu chizmani hech kim bermadi." is Uzbek-only | `Chizma.cs:36` | The plan's step 13 asked for the active language. The port kept the web's text and has no reason on record |
| `olim1` and `olim2` use a straight apostrophe where the rest of the table uses U+2018 | `Game.cs:105-106` | Transcribed, not normalised |

**Comments in the web source that contradict its code. The port follows the code:**

| Comment says | Code does |
|---|---|
| A stone node holds 1 unit, 14 nodes | 4 units, 18 nodes (`world.js:216`) |
| A wall holds attackers 5 s (`combat.js:281`) | 6.0 s per single attacker (`balance.js:71`) |
| The spawn zone is 0 to 25 m (`combat.js:136`) | x in [2, 14) |
| Recruiting takes 15 s (comments when the plan was written) | 11.0 s then, 4.0 s now (`balance.js:44`). `index.html` and the comment above `yollashTekshir` now say 4 s |

## 8. Decisions only you can make

1. **The Act III line.** The sentence the third act is built on, "Endi senga hech kim aytmaydi.", never appears, because of a duplicate key in the web. The port reproduces that and does not carry the string. The plan calls this the one place you should actually decide.
2. **The controls table in English.** No English exists anywhere in the source. Writing 13 rows is authoring, not porting, so the table stays Uzbek-only until you hand them over.
3. **The sword choice on touch.** No touch player can answer it, in either build.
4. **The six failing rows in `BUXORO_PARITY.md`.** Each one is either fixed or moved into section 4 with your reason.
