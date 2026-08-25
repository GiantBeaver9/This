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
- **Palette:** a **shared 32-color base palette** (limited, cohesive) + **per-area 6-color accent ramp**
  swapped per theme. **Gore red is one fixed hue** (`VFX.md`) across all areas. Total on-screen ≤ ~48 colors.
- **Animation fps & frame budgets:** play back at **12 fps** (anime-on-2s feel, cheap). Frame budgets:
  **idle 2–4 · walk 6–8 · attack 3–6 · hurt 2 · death 4–6 · dash 3** (matches `PLAYER.md` §5). The **12 fps
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

### 1a. Shared moveset (per character, or shared skeleton) — mostly **P0**
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
  **Bat** (reflect) · **Staff** (ice/fire/lightning cast FX) · **Gatling** (0.5s barrage) · **Boomerang Gun**
  (orbit + auto-fire) · **Ball & Chain** (launch, heavy impact) · **Rocket Launcher** (world pickup).
- **P1/P2:** **Monkey Merc** (summon poof, pistol/shotgun/rocket variants, expire) · ground **pickup sprites** for every weapon.

---

## 3. Enemies (17) — `ENEMIES.md`
Each: idle · walk (mirror) · attack(s) · hurt/stagger · **death + part/gore** · projectile/telegraph VFX ·
**subtle rank marker** (wristband). **Big versions** = same art scaled ~1.2× (miniboss) / ~2× (boss).
- **P0 (Area 1):** Regular Melee · Swarmer · Zombie (+ hollow-head state).
- **P1 (Area 2):** Anti-Aircraft · Head-Thrower (+ blink-explode) · Snapper (+ snap-to-sword). *(The "Bat
  enemy" in the airport vignette is a **demo actor, not one of the 17** — no roster asset needed.)*
- **P1 (Area 3):** Sniper (beret+rifle, scope up/down) · Flying Monkey · Monkey Tamer (+ enemy monkeys) · Monkey · Arm-Ripper (+ Headbutt state).
- **P1/P2 (Area 4):** Ninja (teleport smoke, shuriken) · Pickpocket · Boomergunner · Gatling Gunner (contort) · Ground Smasher (club+shockwave) · Heavy (Bold/Burly).

---

## 4. Bosses — `BOSSES.md`
Each **bespoke** boss: idle/move · attacks + telegraphs · phase transitions · hurt · death · **sniper-dodge** ·
adds/hazards · **boss HP bar + name card**. "Big version" bosses/minibosses need **no new art**.
- **10 boss encounters placed = 7 bespoke + 3 big-version** (the 3 big-versions need **NO new art** — they
  reuse the enemy sprite at ~2× scale):
- **P1 — 7 bespoke bosses (need bespoke art):** **Burly Macho Guy** (Area 1 dept store) · **Colossus** (Area 2
  Sacramento, whip) · **Helicopter** (Area 2 airport) · **Monkey Boss** (Area 3 farm) · **Tank** (Area 4
  Vallejo) · **Gatling Gun Guy** (Area 4 Golden Gate) · **Phil** (finale — top-hat zombie, pencil-draw, sharpen
  anim, rooftop sway).
- **3 big-version bosses (NO new art):** **Sandwich Bros** (big Tier-1, Area 1) · **big Arm-Ripper** (Area 3
  Dixon) · **Boomergunner boss** (Area 4 Marin — the Boomergunner enemy at boss scale, `ENEMIES.md` §2.17).
- **Phil's bespoke art is P2** (built last), the other 6 bespoke bosses are P1.

---

## 5. UI / HUD — `UI.md` (chunky-arcade)
- **P0:** Health bar (pixel, **damage chunk→explosion** anim, green/yellow/red) · **Special meter** (yellow/blue/green + armed pulse) · **combo popup** (`N HIT!`) · weapon-type icon.
- **P1:** Money counter (+ full-dime highlight) · Monkey-merc cluster (icon + timer ring) · boomerang-gun bullets · ball&chain use-pips · **boss HP bar + name card** · **"BARRAGE INCOMING"** warning · Sniper **red-dot** targeting.
- **P2 (all specced, `UI.md` §5):** Title/main-menu · **character-select** · pause · options (+rebinding) ·
  **results/grade** (cosmetic) · area cards · game-over · button-prompt glyphs · fonts.

---

## 6. VFX — `VFX.md` (comedic transient gore; scaled shake/hitstop; bullets always readable)
- **P0:** air-punch gust · dash dust · jump/land puff · hit spark · finisher flash · **red-pixel death burst** (clears) · muzzle flash.
- **P1:** air-dash streak · sword wear/break · **spine eject** · boomerang stun · staff ice/fire/lightning · grenade trails+explosions · **head-grenade** · whip crack · ball&chain impact · **time-slow overlay** + sniper tracer/ricochet · zombie hollow-head · enemy transformations (snap/rip/contort/teleport smoke) · **screen-shake presets** · hitstop (code).
- **P2:** monkey summon/expire · boss phase flashes · tower-sway ambient · barrage eviscerate.

---

## 7. Environments (per area) — `AREAS.md`, `STAGES.md`
Each theme: parallax backdrop layers + lane floor + set dressing + ambient actors + hazards + funnels.
- **Area 1 (P0/P1):** suburb sky+wispy clouds, houses, **mulberry/tall trees**, sidewalk/road; ambient (fleeing civilians, mail carrier, kid+bike, jogger; dog/cat/birds; **dancing Zebra**); props (parked car, trash can, hydrant, mailbox, hedge, fence, porch, lawn sign); **hazard: cars & school buses**; **Lincoln High**, **Sandwich Bros**; **Galleria mall** interior (storefronts + cowering shoppers, atrium, skylight, kiosks/planters/benches/escalators).
- **Area 2 (P1):** Sacramento **Victorian houses**, streetcar, lamp posts; **airport terminal + tarmac**, taxiing **planes**, ground crew, luggage carts, small planes.
- **Area 3 (P1):** rolling **hills/farmland**, **Yolo causeway + platforms**, marsh; **Dixon** (mid-2000s: main street, water tower, feed store, storefronts); animals (**cows**, goats, chickens, crows), tractors, hay bales, fences; **hazards: ponds/puddles + cow blocking path**.
- **Area 4 (P1/P2):** **Vallejo** amusement park (roller coasters, ferris wheel, midway); **redwood forest** (redwoods, ferns, mist); bay **causeway/bridge**; **Golden Gate Bridge** (+ **car cover**); **San Francisco** cityscape + **trolley/cable-car** (hazard) + **Salesforce Tower** exterior/elevator/**swaying rooftop**.

---

## 8. Audio — see **`AUDIO.md`** (fully specced: 23 music tracks, 76 SFX, VO plan, mix) — VO+SFX creator-produced
- **P1:** **Intro VO** (creator voice, the in-the-beginning-there-was-this script) · core SFX (punch, hit, weapon fires, explosions, zombie, whistle, trolley) .
- **P2:** per-area **music**, ambient beds, boss themes, UI sounds. *(Full audio pass is **specced** — see `AUDIO.md`: 23 tracks, 76 SFX, VO, mix.)*

---

## 9. Cutscenes & vignettes — `STAGES.md` §1a–1c, `AREAS.md`
- **P1:** **Opening cinematic** (~20s voiced picture clips) · **Phil intro** (tower sway foreshadow).
- **P1 (short 3–5s vignettes, reuse enemy/weapon art):** **all 12 locked** in `VIGNETTES.md` (one per stage
  except **Stage 2 / Sandwich Bros**, which introduces no new mechanic and skips per the VIGNETTES rule) —
  Zebra-punch · mall guard→zombie→grab · Sacramento whip-pull · airport head-grenade+bat-a-plane · causeway
  sniper+dime+monkey · farm merc-shoots-boss · Dixon arm-rip · Vallejo pickpocket→ninja→2× · Marin boomergun ·
  Golden Gate stun+barrage+car-cover · SF trolley-vs-Heavy · Phil rooftop monologue.

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
