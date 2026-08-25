# this.l — Master Asset Manifest

> **One checklist for everything to make** — sprites, animations, VFX, UI, backdrops, audio, and cutscenes —
> aggregated from all design docs. Use it to plan asset creation and to feed an automated build.
>
> **Priority:** **P0** = needed to prototype the core loop · **P1** = full vertical slice / first playable ·
> **P2** = polish. **Source** points at the doc with the full spec.
>
> **⚠ Lock these production specs FIRST** (they keep every asset consistent — see §0). Everything below
> assumes lightweight **pixel-art 2.5D** that runs on low-end GPUs.

---

## 0. Production specs — **[LOCKED defaults]** (concrete; override only deliberately)
> These are pinned so mass asset-generation and the Unity import can proceed with **zero further decisions**.
> They are chosen lightweight-first (runs on a GTX 1650 / RTX 3050 up to an RTX 5090). Treat as authoritative defaults.

- **Canvas resolution / aspect:** **16:9**, internal render **640 × 360** (integer-scaled to the window —
  crisp pixels at 720p/1080p/1440p/4K via ×2/×3/×4/×6). Camera is orthographic 2.5D.
- **Sprite pixel sizes:** **player/regular-enemy base height = 48 px** (= 2.0 wu, so **1 wu = 24 px**);
  **Swarmer = 24 px** (half); **miniboss = ×1.2 (≈58 px)**; **boss = ×2.0 (≈96 px)**; **giant bosses**
  (Colossus, Helicopter) drawn at **up to 180 px** reaching into the band. World-unit ↔ pixel is fixed here so
  `TUNING.md` distances convert directly (e.g. a 4 wu dash = 96 px).
- **[LOCKED] Vehicle bosses (non-humanoid, so scaled by footprint not the ×2 humanoid rule):**
  - **Tank:** body footprint **6.0 wu wide × 3.0 wu tall** (**144 × 72 px**), a low bulk filling the left of its
    34-wu arena. **Hatch** = a 1.0 wu circular target centered on the top of the turret at **~2.4 wu height**
    (the grenade-drop point). **Rear-tread mount point** = a 1.0 wu climb-on zone at the **back-left**, ground
    height, where the player mounts to climb (the "direct hit while mounting 22.5" risk zone, `TUNING.md` §7).
    Turret **MG muzzle** sits front-center at ~1.8 wu height. The Tank mainly **holds position and rotates the
    turret**, but **reverses and repositions once at Phase 2** (`BOSSES.md` §5.3) — so it needs a **tread-roll
    / reposition** anim, not just a static body.
  - **Helicopter:** drawn at up to 180 px, **hovers in the sky band** (top 40%) and descends 2 altitude steps
    over the fight (`BOSSES.md` §5.5); its **rotor + cockpit** are the visible mass, no ground footprint.
- **Palette:** a **shared 32-color base palette** (limited, cohesive) + **per-area 6-color accent ramp**
  swapped per theme. **Gore red is one fixed hue** (`VFX.md`) across all areas. Total on-screen ≤ ~48 colors.
- **[LOCKED] The 32-color base palette (concrete hex — the single source of truth all sprites draw from):**
  - **Ink/mono ramp (6)** — stick figures are ink on paper: `#0D0B0E` `#2A2A33` `#4A4A57` `#7A7A88` `#B8B8C4` `#F4F2EC`
  - **Reds (4)** — **gore red = `#B31E2B` (the one fixed gore hue, LOCKED)**, `#E8433F` `#F5794F` `#7A1420`
  - **Oranges/yellows (4):** `#F2A03D` `#FFD24A` `#C77A2A` `#FFF2B0`
  - **Greens (4):** `#234D2C` `#3A7D44` `#6CBF5A` `#A8D98B`
  - **Blues/cyans (5)** — sky & water: `#1B3A5C` `#2E6FB0` `#4AA3D8` `#9FD6EF` `#CFEAF7`
  - **Purples/pinks (2):** `#6A3D8A` `#C86FA8`
  - **Browns/earth (4):** `#3F2A17` `#6B4423` `#9C6B3F` `#C99A6A`
  - **Warm skin/accent (3):** `#8A5A3A` `#D99A6C` `#E6B88A`
  - **Per-area 6-color accent ramp** = 6 additional hues chosen *per area* on top of these 32 (e.g. Vallejo
    carnival adds saturated circus magentas/teals; Marin adds filtered greens) — the base 32 stay constant so
    characters read consistently across every area; only the environment accent ramp swaps.
- **[LOCKED] Font:** a **single bundled bitmap pixel font** — **"chunky-arcade" all-caps display face, ~8px cap
  height** at the 640×360 internal res, uniform-width heavy strokes (readable at 1×). One face used everywhere
  (HUD numerals, screen text, combo popups, boss name cards); glyph set = **A–Z, 0–9, and `$ . : ¢ % ! ? ▶ · / + -`**
  (the **`:`** is required for the Endless `mm:ss` timer, `TUNING.md` §8.3).
  Bundled as a PNG glyph atlas (`ui_font.png`, same import settings as sprites). No secondary font in v1.
- **Animation fps & frame budgets:** play back at **12 fps** (anime-on-2s feel, cheap). Frame budgets:
  **idle 2–4 · walk 6–8 · attack 3–6 · hurt 2 · death 4–6 · dash 3** (matches `PLAYER.md` §5/§7). **Where a
  budget is a range, the build target is its upper bound** (the lower bound is the time-boxed fallback) — the
  same convention `PLAYER.md` §7 pins for its per-move table; treat the two as one rule. The **12 fps
  playback is independent of the 60 fps sim** — frame data (`TUNING.md` §2.5) is in sim-frames; art just
  needs enough drawn frames to read.
- **Framing:** **bottom 60% = scene**, **top 40% = themed HUD/sky backdrop** (`AREAS.md` §1.1, `TUNING.md` §1).
- **Atlas / naming / format:** **PNG** sprite sheets, one atlas per actor, **power-of-two** pages (≤ 2048²);
  naming **`actor_action_dir_frame`** (e.g. `player_attack_side_03.png`); Unity **Sprite (2D) import, Point
  filter, no compression, pixels-per-unit = 24**.
> Validation gate still applies: generate **one test character + one enemy + one backdrop** at these specs,
> confirm they read at 640×360, then scale up.

---

## 1. Characters (4) — `PLAYER.md`, `CHARACTERS.md`
**Pipeline note — [LOCKED]: FULLY BESPOKE per character** (`CHARACTERS.md` §3) — **4 complete animation sets**,
no shared skeleton/reskin. All 4 share the *moveset design* but are **each animated from scratch**; each has a
unique **Special**. Budget ~4× a single-character pile.

### 1a. Moveset (bespoke per character — 4 sets, §1) — mostly **P0**
Idle · walk/run (mirror) · dash + recover · **dash attacks per direction** (side/up/down + air) · air-dash ·
**Shield Rush** (grab + run) · fall-over + getup · jump (rise/peak/fall) · land · ground attacks
(side/up/down) · air attacks (side/up/down) · **air-punch reach gust** (per direction) · hurt · death ·
pick-up · special draw/aim/fire/recover.

### 1b. Per-character skins & Specials — **P1**
- **Tactical (you):** skin + **Sniper time-slow** special (draw/aim/fire/recover, ricochet tracer, boss dodge).
- **Shotgunner (redhead/bulky):** skin + **Giant Shotgun** special (blast + knockback).
- **Werewolf (Gabe):** skin + **transformation** (5s, i-frames, slash-all one-hit-kill).
- **Underdog (short/hard mode):** skin + **Vaporize + 30s +20% buff** special.
- **Character-select screen** (UI).

---

## 2. Weapons — `WEAPONS.md`
Each melee = in-hand idle/walk/jump + directional swing kit + wear/decay states + break VFX. Each ranged =
in-hand + fist-combo-holding-weapon + unique **finisher/fire** + muzzle/projectile VFX + ammo readout.
- **P0:** Fists (in moveset) · **Sword** (wear states) · **Shotgun** (+ **spine magazine** segments, cock, eject).
- **P1:** Boomerang (in-flight + stun) · **Pistol & Revolver** (straight tracer, pierce, cigarette-flick finisher) ·
  **Grenade/Bomb** (bounce marker, lob/fastball trails, big/small blast) · **Whip** (arc/pull/line + head-rip→grenade) ·
  **Bat** (reflect) · **Staff** (ice/fire/lightning cast FX + **crystal/orb decay states — dims one notch per cast, 6→0**, `WEAPONS.md` §3.5) · **Gatling** (0.5s barrage + **barrel heat-glow states — cool→cherry-red toward the 5-barrage/20-s overheat**, `WEAPONS.md` §3.6) · **Boomerang Gun**
  (orbit + auto-fire) · **Ball & Chain** (launch, heavy impact) · **Rocket Launcher** (world pickup) ·
  **Club** (in-hand idle/walk + short-swing + big-knockback swing kit + wear states + **placed-pickup sprite** —
  the airport/Stage-5-on placed heavy-melee option, `WEAPONS.md` §3.7c / `TUNING.md` §6.1).
- **P1/P2:** **Monkey Merc** (summon poof, pistol/shotgun/rocket variants, expire) · **Merc-claim token** ground
  sprite (the pickup a killed Monkey drops, `WEAPONS.md` §3.7) · ground **pickup sprites** for every weapon.
- **[LOCKED] Non-weapon ground pickups (were missing — now itemized):** **heal pickup** sprite (a distinct
  positive-read item, e.g. a small first-aid/health mote; drop per `TUNING.md` §2.2, chime per `AUDIO.md` §4) ·
  **coin** (1¢) and **dime** (10¢) sprites (`UI.md` §3.4) · the **Sniper's dropped rifle** — the +100-meter
  ground pickup (a rifle icon, distinct from the enemy Sniper's held rifle; `ENEMIES.md` §2.14). All use the
  standard 12 s ground-lifetime (`TUNING.md` §4.1).

---

## 3. Enemies (17) — `ENEMIES.md`
Each: idle · walk (mirror) · attack(s) · hurt/stagger · **death + part/gore** · projectile/telegraph VFX ·
**Big versions** = same art scaled ~1.2× (miniboss) / ~2× (boss). *(No rank/wristband marker — the rank system is cut for v1, `ENEMIES.md` §4.)*
- **P0 (Area 1):** Regular Melee · Swarmer · Zombie (+ hollow-head state) · **Pod** (destroyable HP-50 spawner — idle/pulse/spit/destroyed, `TUNING.md` §4).
- **Vignette-only actors (P1, reuse where possible):** the **mall security guard** (fires once, gets grabbed — Stage 3 vignette), **fleeing Marin civilian** (Boomergunner vignette), the **airport Bat demo-actor** (an enemy holding a bat), the **Sacramento whip demo-actor** (the whip-wielding figure of the Stage-4 whip-pull vignette — its airport-Bat counterpart is already listed, `ENCOUNTERS.md` Stage 4 / `VIGNETTES.md`). These are scripted bit-players, not roster enemies (`VIGNETTES.md`, `ENEMIES.md` §6).
- **P1 (Area 2):** Anti-Aircraft · Head-Thrower (+ blink-explode) · Snapper (+ snap-to-sword). *(The "Bat
  enemy" in the airport vignette is a **demo actor, not one of the 17** — no roster asset needed.)*
- **P1 (Area 3):** Sniper (beret+rifle, scope up/down) · Flying Monkey · Monkey Tamer (+ enemy monkeys) · Monkey · Arm-Ripper (+ Headbutt state).
- **P1/P2 (Area 4):** Ninja (teleport smoke, shuriken) · Pickpocket · Boomergunner · Gatling Gunner (contort) · Ground Smasher (club+shockwave) · Heavy (Bold/Burly).

---

## 4. Bosses — `BOSSES.md`
Each **bespoke** boss: idle/move · attacks + telegraphs · phase transitions · hurt · death · **sniper-dodge** ·
adds/hazards · **boss HP bar + name card**. "Big version" bosses/minibosses need **no new art**.
- **[LOCKED] Per-boss attack-animation enumeration = the attack list in that boss's `BOSSES.md` §5 entry** (not
  the generic "attack 3–6" budget) — author one telegraph+active+recovery anim per named attack. E.g. **Burly**:
  ground-spike, enemy-toss, charge (§5.2); **Tank**: MG-sweep, hatch-open, reposition (§5.3); **Helicopter**:
  head-throw, rotor-gust, descend (§5.5); **Gatling Gun Guy**: barrage wind-up, chip-stream, melee (§5.6);
  **Monkey Boss**: dime-toss ×1, dime-toss ×2 (§5.7); **Phil**: pencil-draw, sharpen, contact, pencil-laser
  (§5.1). Frame budget per attack = the §0 upper-bound (6) unless the §5 entry pins otherwise.
- **[LOCKED] Colossus piece-shed breakdown (6 pieces, `BOSSES.md` §5.4):** the giant is a stack of stick-figure
  parts; whip-pull removes them **top-down in a fixed order: (6) head → (5) right arm → (4) left arm → (3)
  torso-upper → (2) right leg → (1) left leg/core**. Author the **6 silhouette states** (one per remaining-piece
  count, each ~20 s on screen) + each torn piece as a **T1 add** that drops in (reuses the Regular sprite). It
  speeds up at 4 and 2 pieces (§5.4).
- **10 boss encounters placed = 7 bespoke + 3 big-version** (the 3 big-versions need **NO new art** — they
  reuse the enemy sprite at ~2× scale):
- **P1 — 7 bespoke bosses (need bespoke art):** **Burly Macho Guy** (Area 1 dept store) · **Colossus** (Area 2
  Sacramento, whip) · **Helicopter** (Area 2 airport) · **Monkey Boss** (Area 3 farm) · **Tank** (Area 4
  Vallejo) · **Gatling Gun Guy** (Area 4 Golden Gate) · **Phil** (finale — top-hat zombie, pencil-draw, sharpen
  anim, rooftop sway, **and the scripted pencil-laser kill VFX** — the beam the player fires from the pencil in
  the execute window, `BOSSES.md` §5.1; the game's climax needs its own drawn asset).
- **3 big-version bosses (NO new art):** **Sandwich Bros** (big Tier-1, Area 1) · **big Arm-Ripper** (Area 3
  Dixon) · **Boomergunner boss** (Area 4 Marin — the Boomergunner enemy at boss scale, `ENEMIES.md` §2.17).
- **Phil's bespoke art is P2** (built last), the other 6 bespoke bosses are P1.

---

## 5. UI / HUD — `UI.md` (chunky-arcade)
- **P0:** Health bar (pixel, **damage chunk→explosion** anim, green/yellow/red) · **Special meter** (yellow/blue/green + armed pulse) · **combo popup** (`N HIT!`) · weapon-type icon.
- **P1:** Money counter (+ full-dime highlight) · Monkey-merc cluster (icon + timer ring) · boomerang-gun bullets · ball&chain use-pips · **boss HP bar + name card** · **"BARRAGE INCOMING"** warning · Sniper **red-dot** targeting · **Endless HUD: live score readout + `mm:ss` elapsed timer** (top-center, independent of the top-left health/meter cluster; Endless-only, `TUNING.md` §8.3 / `UI.md` §5).
- **P2 (all specced, `UI.md` §5):** Title/main-menu · **difficulty-select** (+ in-run HUD difficulty badge) ·
  **character-select** · pause · options (+rebinding) · **results/grade** (cosmetic) · area cards · game-over ·
  **execute-prompt** (`▶ SPECIAL` over a ≤10% boss) · **boss HP bar + name card** (+ objective-boss pip
  readouts) · button-prompt glyphs · fonts.

---

## 6. VFX — `VFX.md` (comedic transient gore; scaled shake/hitstop; bullets always readable)
- **P0:** air-punch gust · dash dust · jump/land puff · hit spark · finisher flash · **red-pixel death burst** (clears) · muzzle flash · **blob ground-shadow / Z-marker (1 per actor** — the most-instanced sprite, reads each actor's exact Z, `TUNING.md` §1).
- **P1:** air-dash streak · sword wear/break · **spine eject** · boomerang stun · staff ice/fire/lightning · grenade trails+explosions · **head-grenade** · whip crack · ball&chain impact · **time-slow overlay** + sniper tracer/ricochet · zombie hollow-head · enemy transformations (snap/rip/contort/teleport smoke) · **screen-shake presets** · hitstop (code).
- **P2:** monkey summon/expire · boss phase flashes · tower-sway ambient · barrage eviscerate · **heal-pickup
  glint** · **pencil-laser beam** (Phil's scripted kill, `BOSSES.md` §5.1).

---

## 7. Environments (per area) — `AREAS.md`, `STAGES.md`
Each theme: parallax backdrop layers + lane floor + set dressing + ambient actors + hazards + funnels.
- **[LOCKED] Parallax layer spec (every area, same structure):** **3 scrolling layers behind the play lane**,
  with fixed horizontal scroll factors relative to camera motion: **far = 0.2×** (sky/horizon band — fills the
  top-40% HUD-sky region, `TUNING.md` §1), **mid = 0.5×** (buildings/treeline/hills), **near = 0.85×**
  (roadside props just behind the lane); the **play lane itself = 1.0×** (locked to world, where actors live).
  Each layer is a **horizontally-tiling strip** (seamless loop), authored at **360 px tall** (the full internal
  render height — the canvas is 640×**360**) so it covers the whole frame. **Far layer never scrolls
  vertically**; mid/near don't either (the camera is Z-locked, §1). **One 3-layer set per stage-theme = 12 sets**
  (one per §2 music/backdrop theme, so the two Area-1 suburb stages share a set, matching the loop/bed sharing,
  `AUDIO.md` §2), swapped at each theme boundary — **not** 5 (that's the area count; backdrops swap by
  stage-theme, not by area).
- **Area 1 (P0/P1):** suburb sky+wispy clouds, houses, **mulberry/tall trees**, sidewalk/road; ambient (fleeing civilians, mail carrier, kid+bike, jogger; dog/cat/birds; **dancing Zebra** (+ its **vignette PUNCH anim** — the Stage-1 Zebra-punch vignette, `VIGNETTES.md`)); props (parked car, trash can, hydrant, mailbox, hedge, fence, porch, lawn sign); **hazard: cars & school buses**; **Lincoln High**, **Sandwich Bros**; **Galleria mall** interior (storefronts + cowering shoppers, atrium, skylight, kiosks/planters/benches/escalators).
- **Area 2 (P1):** Sacramento **Victorian houses**, streetcar, lamp posts; **airport terminal + tarmac**, taxiing **planes**, ground crew, luggage carts, small planes.
- **Area 3 (P1):** rolling **hills/farmland**, **Yolo causeway + platforms**, marsh; **Dixon** (mid-2000s: main street, water tower, feed store, storefronts); animals (**cows**, goats, chickens, crows), tractors, hay bales, fences; **hazards: ponds/puddles + cow blocking path**.
- **Area 4 (P1/P2):** **Vallejo** amusement park (roller coasters, ferris wheel, midway) + the **roller-coaster
  HAZARD car** (an on-rail hazard vehicle, 50 dmg every 7 s, Stage 9 — a drawn moving car, distinct from the
  backdrop coasters, like Area 1's hazard vehicles); **redwood forest** (redwoods, ferns, mist); bay
  **causeway/bridge** + **water-splash** on a fall; **Golden Gate Bridge** (+ **car cover**); **San Francisco**
  cityscape + **trolley/cable-car** (hazard) + **Salesforce Tower** exterior/elevator/**swaying rooftop**.
- **[LOCKED] Checkpoint markers (P1):** the **visible world marker** (flag/beacon) planted at each checkpoint,
  **themed per area** (`ENCOUNTERS.md` §0) — **~12 themed instances** (one per stage/theme). Pairs with the
  checkpoint chime (`AUDIO.md` §4) so a checkpoint reads both visually and audibly.
- **[LOCKED] Boss-arena-specific props (P1):** the bespoke set dressing each boss arena needs (`ENCOUNTERS.md`
  boss-arena table / `AREAS.md`), not covered by the per-area lists above — **Burly dept-store display islands**
  (2, soft cover) · **Colossus Sacramento Victorian-plaza set** · **Helicopter tarmac luggage carts** (2) ·
  **Monkey Boss ranch hay-bale covers** (2) · **Tank Six-Flags ride-support pillars** · **big Arm-Ripper
  water-tower base pillar** · **Boomergunner redwood-clearing trunks** (3) · **Gatling Gun Guy bridge parked
  cars** (4, hard cover) · **Phil rooftop HVAC-block cover**.

---

## 8. Audio — see **`AUDIO.md`** (fully specced: 23 music tracks, 95 SFX, VO plan, mix) — VO+SFX creator-produced
- **P1:** **Intro VO** (creator voice, the in-the-beginning-there-was-this script) · core SFX (punch, hit, weapon fires, explosions, zombie, whistle, trolley) .
- **P2:** per-area **music**, ambient beds, boss themes, UI sounds. *(Full audio pass is **specced** — see `AUDIO.md`: 23 tracks, 95 SFX, VO, mix.)*

---

## 9. Cutscenes & vignettes — `STAGES.md` §1a–1c, `AREAS.md`
- **P1:** **Opening cinematic** (~20s voiced picture clips) · **Phil intro** (tower sway foreshadow).
- **P1 (short 3–5s vignettes, reuse enemy/weapon art):** **all 12 locked** in `VIGNETTES.md` (one per stage
  except **Stage 2 / Sandwich Bros**, which introduces no new mechanic and skips per the VIGNETTES rule) —
  Zebra-punch · mall guard→zombie→grab · Sacramento whip-pull · airport head-grenade+bat-a-plane · causeway
  sniper+dime+monkey · farm merc-shoots-boss · Dixon arm-rip · Vallejo pickpocket→ninja→2× · Marin boomergun ·
  Golden Gate stun+barrage+car-cover · SF trolley-vs-Heavy · Phil rooftop monologue.
- **P2:** **Outro epilogue** (~15s hand-drawn voiced still-clips, `STORY.md` §3) + **scrolling credits** over
  the finale music.

---

## 10. Systems status — **all major systems now LOCKED**
Nothing here blocks art. Everything previously open is resolved:
**Now LOCKED (were open):** **animation pipeline** = fully bespoke, 4 sets (`CHARACTERS.md` §3) · **save +
settings/options menus** (`UI.md` §5) · player attack **frame data** + universal **reaction-state durations**
(`TUNING.md` §2.5–2.6) · **per-stage encounter/wave tables** + **boss arena layouts** (`ENCOUNTERS.md`) ·
**special meter-tier scaling** for all 4 characters (`TUNING.md` §3.1) · checkpoint & continue specifics
(`TUNING.md` §8.1) · Controls/keybinds + gamepad (`PLAYER.md` §2) · **hazard damage** + **area-gated loot
pools** (`TUNING.md` §6.1–6.2) · audio pass (`AUDIO.md`) · story spine (`STORY.md`).
*(Residual polish only: exact icon placement, minor per-enemy flavor — none block the overnight build.)*
