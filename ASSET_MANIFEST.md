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

## 0. Production specs to lock before mass generation — **[DECIDE FIRST]**
- **Canvas resolution / aspect** (e.g. 16:9 target, internal pixel resolution).
- **Sprite pixel sizes:** player/enemy base height, "big version" scales (miniboss ~1.2×, boss ~2×), bosses.
- **Palette** (shared limited palette for cohesion; the red-pixel gore color; per-area accent ramps).
- **Animation fps & frame budgets** per action (idle 2–4, walk 6–8, attacks 3–6…).
- **Framing:** bottom ~50–60% = scene, top ~40% = themed HUD backdrop (`AREAS.md` §1.1).
- **Atlas / naming convention & file format** for import.
> Until these are set, generate **one test character + one enemy + one backdrop** to validate, then scale up.

---

## 1. Characters (4) — `PLAYER.md`, `CHARACTERS.md`
**Pipeline note:** bespoke animation; **[OPEN]** shared-skeleton reskin (rec) vs. fully bespoke per character.
All 4 share the moveset; each has a distinct **skin** + unique **Special**.

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
- **P1 (all 7 bespoke bosses placed):** Sandwich Bros (big Tier-1, Area 1) · **Burly Macho Guy** (Area 1 dept
  store) · **Colossus** (Area 2 Sacramento, whip) · **Helicopter** (Area 2 airport) · **Monkey Boss** (Area 3
  farm) · big Arm-Ripper (Area 3 Dixon) · **Boomergunner** boss (Area 4 Marin) · **Tank** (Area 4 Vallejo) ·
  **Gatling Gun Guy** (Area 4 Golden Gate, barrage + car cover).
- **P2:** **Phil** (top-hat zombie, pencil-draw, **sharpen animation**, re-summons, rooftop sway).

---

## 5. UI / HUD — `UI.md` (chunky-arcade)
- **P0:** Health bar (pixel, **damage chunk→explosion** anim, green/yellow/red) · **Special meter** (yellow/blue/green + armed pulse) · **combo popup** (`N HIT!`) · weapon-type icon.
- **P1:** Money counter (+ full-dime highlight) · Monkey-merc cluster (icon + timer ring) · boomerang-gun bullets · ball&chain use-pips · **boss HP bar + name card** · **"BARRAGE INCOMING"** warning · Sniper **red-dot** targeting.
- **P2:** Title · pause · **results/grade** (cosmetic) · area cards · game-over · button prompts · fonts.

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

## 8. Audio — see **`AUDIO.md`** (fully specced: 21 music tracks, 64 SFX, VO plan, mix) — VO+SFX creator-produced
- **P1:** **Intro VO** (creator voice, the in-the-beginning-there-was-this script) · core SFX (punch, hit, weapon fires, explosions, zombie, whistle, trolley) .
- **P2:** per-area **music**, ambient beds, boss themes, UI sounds. *(A full audio pass is not yet designed.)*

---

## 9. Cutscenes & vignettes — `STAGES.md` §1a–1c, `AREAS.md`
- **P1:** **Opening cinematic** (~20s voiced picture clips) · **Phil intro** (tower sway foreshadow).
- **P1 (short 3–5s vignettes, reuse enemy/weapon art):** mall (guard→zombie→grab) · airport (head-grenade + bat-a-plane) · Sacramento (whip-pull) · causeway (sniper + dime + monkey combined) · **[ITERATE]** the rest per stage.

---

## 10. Systems still open (won't block art, but pin before final build)
Exact per-stage enemy rosters/counts · checkpoint & continue specifics ·
economy scope beyond monkeys · character animation pipeline choice ·
**player attack frame data** (startup/active/recovery) · **per-stage encounter/wave tables** · boss arena layouts ·
save/settings menus · audio pass. *(Controls/keybinds + gamepad are now LOCKED in `PLAYER.md` §2.)*
