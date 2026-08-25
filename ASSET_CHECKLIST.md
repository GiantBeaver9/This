# this.l — Asset Production Checklist (Sprites · SFX · Music)

> **Purpose:** a single tickable list for the first-round asset pass, pulled from the design bible.
> Full specs live in the source docs (cited per line). This is the *what-to-make* list, not the *how-it-works*.
>
> **Production specs (LOCKED, `ASSET_MANIFEST.md` §0) — apply to every sprite:**
> - Internal render **640×360**, **1 wu = 24 px**, player/regular height **48 px**, Swarmer **24 px**,
>   miniboss **≈58 px** (×1.2), boss **≈96 px** (×2.0), giant boss **up to 180 px**.
> - **32-color base palette** (hex list in `ASSET_MANIFEST.md` §0), gore red = `#B31E2B`, + a per-area
>   6-color accent ramp. Play band = bottom 60%, HUD/sky = top 40%.
> - Animate at **12 fps**. Where a frame count is a range, **build the upper bound**.
> - PNG atlases, one per actor, Point filter, no compression, ppu = 24, naming `actor_action_dir_frame`.
> - Priority: **P0** = prototype the core loop · **P1** = vertical slice · **P2** = polish.
>
> **Big-version rule:** minibosses (×1.2) and the 3 big-version bosses (×2.0) reuse the base enemy sprite at
> scale — **NO new art**. They are NOT listed as separate sprite work below.

---

## A. SPRITES

### A1. Player characters — 4 FULLY BESPOKE sets (`PLAYER.md` §7, `CHARACTERS.md`) — P0/P1
Each of the 4 (**Tactical**, **Shotgunner**, **Werewolf/Gabe**, **Underdog**) gets a complete, from-scratch set
— no shared skeleton. Per character:
- [ ] Idle (2–4 loop) · Walk/Run (6–8, mirror) — **P0**
- [ ] Dash lunge + recover (3–4) · Air-dash (2–3) · Shield Rush (grab + run, 4–5) · Fall-over + getup (4–6)
- [ ] Jump rise/peak/fall (3) · Land (2)
- [ ] Ground: **P1 jab** (4) · **P2 cross** (4) · **combo sweep** (hit 3, 5) · **combo finisher** (hit 4, 5) ·
      **↑↑ launcher** (4) · **single-tap finish** (3)
- [ ] Ground directional: up-attack (3–4) · down-attack (3–4)
- [ ] Air: side (3) · up (3) · down/spike (3–4)
- [ ] **Gun-execute variants ×4** (Quickdraw / Coup / Skyshot / No-Look, 4 ea, `COMBOS.md` §2)
- [ ] Hurt/hitstun (2) · Death (4–6) · Pick-up (0–2)
- [ ] **Per-character Special** (`PLAYER.md` §7):
  - [ ] Tactical — Sniper: draw 3 · aim 2 · fire 2 · recover 2
  - [ ] Shotgunner — Giant Shotgun: raise 3 · boom 3 · cock/recover 3
  - [ ] Werewolf — wolf form: **transform 5 · wolf idle 3 · wolf run 6 · auto-slash 4 · air-slash 3 · revert 4**
  - [ ] Underdog — Vaporize: wind-up 3 · burst 4 · empower-aura loop 3

### A2. Enemies — 17 sprites (`ENEMIES.md`, `TUNING.md` §4) — P0→P2
Each: idle · walk (mirror) · attack(s) + telegraph · hurt/stagger · **death + gore** · any transform/projectile.
- [ ] **P0:** Zombie (hollow head) · Swarmer (24px) · Regular Melee (punch/jump-kick/slide-kick) · **Pod** (spawner, HP 50)
- [ ] **P1 (Area 2):** Anti-Aircraft (rock throw) · Head-Thrower (self-decapitate + regrow + fire-blink→BOOM) · Snapper (snap-a-T1-to-sword)
- [ ] **P1 (Area 3):** Sniper (beret + rifle, scope up/down, red-dot) · Flying Monkey (swoop) · Monkey Tamer (whistle) · Monkey (economy, drops Merc-claim) · Arm-Ripper (akimbo pistols + headbutt state)
- [ ] **P1/P2 (Area 4):** Ninja (smoke-teleport + shuriken) · Pickpocket (steal + flee) · Boomergunner (throw orbit gun) · Gatling Gunner (contort + stream) · Ground Smasher (club + shockwave) · Heavy ("Bold"/Burly, extended punch + gust)

### A3. Bosses — 7 BESPOKE only (`BOSSES.md` §5, `ASSET_MANIFEST.md` §4) — P1 (Phil P2)
Each: idle/move · **one anim per named §5 attack** (telegraph+active+recovery) · phase transitions · hurt ·
death · sniper-dodge · **HP bar + name card + boss-face icon**.
- [ ] **Burly Macho Guy** — ground-spike, enemy-toss, charge
- [ ] **Colossus** — body-swipe; **6 shed-state silhouettes** (head→R arm→L arm→upper-torso→R leg→L leg/core; each torn piece = a Regular add)
- [ ] **Helicopter** — head-throw, rotor-gust, 2 descend altitudes (sky-band actor, up to 180px)
- [ ] **Monkey Boss** — dime-toss ×1, dime-toss ×2 (throws dimes, no contact)
- [ ] **Tank** — MG-sweep, hatch-open, **tread-roll reposition** (144×72px footprint, hatch + rear mount)
- [ ] **Gatling Gun Guy** — barrage wind-up, chip-stream, melee
- [ ] **Phil (P2)** — top-hat zombie, pencil-draw, sharpen, contact, **pencil-laser kill beam**, rooftop sway
- [ ] *(NO new art: Sandwich Bros, big Arm-Ripper, Boomergunner boss — scaled reuse)*

### A4. Weapons — 16 (`WEAPONS.md`, `ASSET_MANIFEST.md` §2) — P0→P2
Each: in-hand idle/walk/jump · combo-swing kit (or fist-combo-holding-gun) · fire/finisher · wear/break ·
**ground pickup sprite**.
- [ ] **P0:** Sword (wear/chip states) · Shotgun (spine-magazine segments, cock, eject)
- [ ] **P1:** Boomerang (in-flight + stun) · Pistol · Revolver (tracer, pierce, casing-eject, cigarette-flick) ·
      Grenade (bounce marker, lob/fastball trails, big/small blast) · Whip (arc/pull/line + head-rip→grenade) ·
      Bat (reflect) · Staff (ice/fire/lightning cast FX) · Gatling (0.5s barrage) · Boomerang Gun (orbit + auto-fire) ·
      Ball & Chain (4 launch shapes + heavy impact) · **Club** (in-hand + swing + wear + placed-pickup) · Rocket Launcher
- [ ] **P1/P2:** Monkey Merc (summon poof, pistol/shotgun/rocket variants, expire)

### A5. Ground pickups & tokens (`TUNING.md` §6, `UI.md` §3.4) — P1
- [ ] Ground pickup sprite for **every weapon** (above)
- [ ] **Heal pickup** (distinct positive read) · **Coin** (1¢) · **Dime** (10¢, full-dime highlight)
- [ ] **Merc-claim token** (dropped by a killed Monkey) · **Sniper's dropped rifle** (+100 meter, distinct from held rifle)

### A6. VFX (`VFX.md`, `ASSET_MANIFEST.md` §6) — P0→P2
- [ ] **P0:** air-punch gust · dash dust · jump/land puff · hit spark · finisher flash · **red-pixel death burst** · muzzle flash
- [ ] **P1:** air-dash streak · sword wear/break · spine eject · boomerang stun · staff ice/fire/lightning · grenade trails+explosions · head-grenade · whip crack · ball&chain impact · **time-slow overlay** + sniper tracer/ricochet + **red-dot** (2 variants: player-chain & enemy-on-player) · zombie hollow-head · transforms (snap/rip/contort/teleport smoke) · **screen-shake presets** (light/medium/heavy)
- [ ] **P2:** monkey summon/expire · boss phase flashes · tower-sway · barrage eviscerate · heal glint · **Phil pencil-laser beam**
- [ ] **[LOCKED] blob ground-shadow** — 1 per actor, the most-instanced sprite (`TUNING.md` §1)

### A7. UI / HUD (`UI.md`, `ASSET_MANIFEST.md` §5) — P0→P2
- [ ] **P0:** Health bar (chunk→explosion damage anim, green/yellow/red) · Special meter (yellow/blue/green + armed pulse) · combo popup (`N HIT!`) · weapon-type icon
- [ ] **P1:** Money counter (+ full-dime highlight) · Monkey-merc cluster (icon + timer ring + greyed spent) · boomerang-gun bullet pips · ball&chain use-pips · **boss HP bar + name card + boss-face icon** · objective-boss readouts (Helicopter 6-pip · Tank 2-pip · Colossus 6-segment · Monkey Boss HP-200 · Phil HP+4-notches) · **"BARRAGE INCOMING"** warning · execute prompt (`▶ SPECIAL`) · **Endless score + mm:ss timer** readouts
- [ ] **P2 screens:** title/main-menu · difficulty-select (+ in-run badge) · character-select (+ 4 Special icons) · pause · options (+rebinding) · results/grade · area cards · game-over · button-prompt glyphs
- [ ] **Bundled pixel font** (`ui_font.png`): chunky-arcade all-caps, ~8px cap height; glyphs `A–Z 0–9 $ . : ¢ % ! ? ▶ · / + -`

### A8. Environments — 12 stage-theme backdrop sets (`AREAS.md`, `ASSET_MANIFEST.md` §7) — P0→P2
Each theme = **3 parallax layers** (far 0.2× / mid 0.5× / near 0.85×, each a 360px-tall tiling strip) + lane
floor + set dressing + funnels + hazards + ambient (non-hittable) actors.
- [ ] **Area 1:** Lincoln suburbs (houses/trees/sidewalk) · Rocklin/Old Hwy 65 · **Roseville Galleria** mall interior. Ambient: fleeing civilians, mail carrier, kid+bike, jogger, dog/cat/birds, **dancing Zebra** (+ its vignette punch). Hazard: **cars & school buses** (+ horn telegraph).
- [ ] **Area 2:** Sacramento Victorian downtown · Sacramento Airport (tarmac). Hazard: **taxiing plane** (+ engine-whine telegraph). Vignette demo-actors: **airport Bat**, **Sacramento whip**.
- [ ] **Area 3:** Hills · Davis causeway (6×10wu platforms + water + splash) · Farm/Ranch · Dixon deserted town.
- [ ] **Area 4:** Vallejo amusement park (+ **roller-coaster hazard car**) · Marin redwoods (mist) · Golden Gate Bridge (+ **car cover**) · SF streets (+ **trolley/cable-car** hazard) · **Salesforce Tower** exterior/elevator/**swaying rooftop**.
- [ ] Checkpoint marker (world visual) · area-card art.

### A9. Cutscene / story art (`STORY.md`, `STAGES.md`) — P2
- [ ] **5 hand-drawn intro still-clips** (~20s ea) · **outro/credits** stills.

---

## B. SFX — 92 one-shots (`AUDIO.md` §4)
Short arcade mono one-shots, pitch-randomized ±2 semitones. Grouped exactly as the bible counts them (`+` = 2
sounds; a `/` = variant spellings of 1 unless an explicit count is given).
- [ ] **Player melee (8):** punch ×2, sweep, finisher (heavy), air-hit, dash-whoosh, jump, land
- [ ] **Player states (6):** hurt/grunt, death, weapon-pickup, **heal-pickup chime**, weapon-break puff, shield-rush scrape
- [ ] **Impacts/juice (6):** hit-spark, finisher-crunch, enemy stagger, knockdown thud, block/soak, screen-shake boom
- [ ] **Weapon fire (24):** sword swing+break, shotgun blast+cock, boomerang throw+return, pistol, revolver, grenade throw+explode, ball&chain launch+impact, whip crack, whip head-rip pop, staff cast ice/fire/lightning (3), gatling barrage, boomerang-gun spin, boomerang-gun shot-down break, rocket launch+blast, club whack, bat reflect-ping
- [ ] **Enemy signature (19):** zombie moan+grab, swarmer chitter, head-throw, fire-blink→BOOM, snapper snap-to-sword, arm-rip, gatling contort, ninja smoke-teleport, sniper scope-in+shot, ground-smash overhead+shockwave, whistle (tamer), monkey merc chatter, AA rock-throw, ninja shuriken throw, boomergunner gun-throw, Pod spawn-burst
- [ ] **Meter/specials (7):** meter tick-up, armed "ready" chime, sniper time-slow enter, time-resume whoosh, Werewolf transform-howl, Giant Shotgun boom, Underdog Vaporize whomp
- [ ] **Phil/finale (3):** pencil-draw scribble, sharpen scrape, pencil-laser fire
- [ ] **UI (7):** menu move, confirm, cancel, coin pickup, full-dime highlight, combo-popup pips, "BARRAGE INCOMING" alarm
- [ ] **Economy/misc (3):** pickpocket steal, coins-doubled jingle, checkpoint chime
- [ ] **Hazards (9):** car/bus pass-by, car horn, plane jet-blast, cow moo, SF trolley bell+rumble, tower-sway creak, roller-coaster pass, causeway water splash

*(Recording spec: mono, 44.1 kHz. VO + SFX are creator-produced — this is the record-against shot-list.)*

---

## C. MUSIC & AMBIENT (`AUDIO.md` §2–3, §7)
Streamed loops (OGG, 44.1 kHz stereo, ~128 kbps). Creator-composed (CC0 library fallback if time-boxed).

### C1. Stage / area loops — 12 (P2)
- [ ] A1 surf-rock opener (Stages 1–2) · A1 synth-punk mall (Stage 3) · A2 ragtime garage-rock (Stage 4) ·
      A2 industrial electronic (Stage 5) · A3 spaghetti-western (Stage 6) · A3 hoedown bluegrass (Stage 7) ·
      A3 western dread (Stage 8) · A4 circus-rock (Stage 9) · A4 psych-rock (Stage 10) · A4 orchestral-rock (Stage 11) ·
      A4 electro-punk (Stage 12) · Finale rooftop approach (Stage 13 pre-fight)

### C2. Boss cues — 9 (P2)
- [ ] 8 per-boss re-orchestrations of the shared "Phil's Army" 4-bar motif (Burly, Colossus, Helicopter,
      Monkey Boss, big Arm-Ripper, Tank, Gatling Gun Guy, Boomergunner) + **Phil's fully realized version**
      (orchestra + choir). *(Sandwich Bros reuses the A1 opener — no separate cue.)*

### C3. Other music (P2)
- [ ] **Title theme** (1) · **Endless layered track** (1, SF electro-punk + 4 add-in stems) ·
      **5 area-transition stingers** (2s each, cut from the incoming loop — A1/A2/A3/A4/Finale)
- **Total music assets = 23** (12 loops + 1 title + 1 Endless + 9 boss) + 5 stingers.

### C4. Ambient beds — 12 (P2)
- [ ] 1 looping bed per stage-music slot: birds/traffic (Lincoln) · mall murmur (Galleria) · old-town tone (Sacramento) ·
      tarmac+jet (Airport) · marsh/wind (causeway) · barnyard (Farm) · deserted-town wind (Dixon) · carnival (Vallejo) ·
      redwood forest (Marin) · bridge wind (Golden Gate) · city crowd+sirens (SF) · rooftop wind (Finale).

---

## D. VOICE-OVER (`AUDIO.md` §5) — creator voice, mono 44.1 kHz
- [ ] **5 intro clips** (~20s ea, ~1:40 total) · **Phil rooftop monologue** (~30s) · **1 outro line** (~10s).
- No in-gameplay dialogue (enemies/bosses non-verbal).

---

### Quick totals
| Bucket | Count |
|---|---|
| Player character sets (bespoke) | 4 |
| Enemy sprites | 17 |
| Bespoke boss sprite sets | 7 |
| Weapon sprite sets | 16 |
| Backdrop themes (3 layers each) | 12 |
| SFX one-shots | 92 |
| Music assets (+5 stingers) | 23 (+5) |
| Ambient beds | 12 |
| VO clips | 7 |
