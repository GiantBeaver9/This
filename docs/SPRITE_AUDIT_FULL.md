# Sprite / Art Gap Audit — FULL (this.l) — 2026-08-25

Exhaustive accounting of **every** sprite/animation the design calls for, cross-referenced against
(a) what actually EXISTS on disk and (b) what the CODE references. Supersedes the quick pass in
`docs/SPRITE_AUDIT.md` (which under-reports — it lists heroes/enemies as "done" without noting the
missing clips the code plays, the weapon-hold/projectile gap, the 11 missing backdrop sets, or the
per-boss/per-enemy special states).

Legend: **DONE** = art on disk and used · **PARTIAL** = folder/atlas exists but missing clips/states
(named) · **MISSING** = nothing on disk. **⚠ RUNTIME GAP** = the CODE plays this clip but no atlas
frame exists → `SpriteAnimator` silently falls back (to the previous clip / idle / a tinted stick
figure), so the action reads wrong in-game today.

Method: parsed every `assets/sprites/**/*.json` atlas (clip = frame-name minus `_NN.png`), inventoried
`assets/backdrops`, `assets/portraits`, `assets/ui`, `assets/fonts`; grepped `unity/Assets/Scripts`
for `Anim.Play(...)` (sprite clips), `Sfx.Play(...)` (audio, excluded), `WeaponKind`, `EnemyArchetype`,
and the boss registry in `Bosses.cs`.

---

## 0. What EXISTS on disk today (clip-level inventory)

**Heroes (4/4 folders)** — `player_tactical`, `player_shotgunner`, `player_werewolf`, `player_underdog`.
Every hero atlas has the **same 9 clips**: `idle`(4) `walk`(4) `attack_side`(3) `attack_up`(7)
`attack_down`(7) `sweep`(7) `dash`(6) `hurt`(6) `death`(7). Portraits exist for all 4.

**Enemies (19/19 archetypes)** — one stick-figure atlas each, **5 clips**: `idle` `walk` `attack_side`
`hurt` `death` (except `enemy_pod` = `idle`/`spit`/`death`). Matches the `EnemyArchetype` enum exactly.

**Bosses (1/10 arted)** — only `phil` has a bespoke atlas: `idle` `walk` `attack_side` `hurt` `death`
`sharpen`(6). All 9 other boss ids fall back to a tinted `enemy_regular`.

**Weapons** — 11 ground-pickup PNGs (sword, shotgun, pistol, revolver, club, bat, staff, whip,
boomerang, grenade, ballchain) + sword wear states (`fresh`/`worn`/`chipped`) + shotgun extras
(`shotgun.png` held special, `shotgun_spine_segment.png`). **No in-hand/held, swing, fire, or
projectile sprites for any other weapon.**

**Pickups (5/5)** — coin, dime, heal, merc_token, sniper_rifle.

**VFX** — `vfx` atlas: `hit_spark`(4) `dash_dust`(4) `death_burst`(5) `finisher_flash`(5) `gust`(4)
`jump_puff`(3) `land_puff`(4) `muzzle_flash`(3) + `blob_shadow` (placeholder JSON).

**UI** — health (green/yellow/red), special (yellow/blue/green/armed), combo (3/8/15 hit),
weapon icons (fist/gun/sword), `prompt_execute`, `warn_barrage`, `ui_font.png` + palette. Boss bars,
menus, money, endless HUD: none.

**Backdrops** — only **Area 1 suburb** has a real layered set (`far`/`mid`/`near`/`lane` + preview).
Areas 2/3/4 have only `house.png` + `tree.png` prop pairs. (Note: `World/Backdrop.cs` currently draws
procedural code bands; the tileable sprite strips the creator wants are Area-1-only.)

---

## 1. HEROES — PARTIAL (4/4 exist, but each is missing most of the specced moveset)

Design source: `SPRITE_BRIEF.md`, `PLAYER.md §7`, `ASSET_CHECKLIST.md §A1`, `COMBOS.md`.
Each hero has 9 clips; the fully-bespoke spec calls for ~30+. Gaps are **identical across all 4** unless noted.

| Clip the design/code wants | On disk? | Status |
|---|---|---|
| `idle` `walk` `attack_side` `attack_up` `attack_down` `dash` `hurt` `death` | yes | DONE |
| `sweep` (combo hit 3) | yes (7 frames) | **art exists but UNWIRED** — `PlayerController.StartSweep()` plays `attack_side`, not `sweep`. Same for `StartFinisher()` (no `finisher` clip) and `StartDashAttack()` (reuses cardinals). |
| `jump` | **no** | ⚠ RUNTIME GAP — `PlayerController.cs:1129` plays `"jump"` every airborne frame → falls back to idle/dash. |
| `air_side` / `air_up` / `air_down` | **no** | ⚠ RUNTIME GAP — `PlayerController.cs:644` plays these on every air attack → fallback. |
| `special` (sniper spin / giant shotgun / vaporize) | **no** | ⚠ RUNTIME GAP — `Characters.cs:78,93,135` plays `"special"` → fallback (only the code glow/ring shows). |
| `land` | **no** | MISSING (code plays only the `land` SFX, no clip). |
| `finisher` (unarmed stomp) | **no** | MISSING (design wants it; code reuses `attack_side`). |
| `execute_<weapon>` cinematic finisher ×N weapons | **no** | MISSING (design wants one per weapon; code reuses `attack_side`). |
| Shield-Rush (grab+run), fall-over+getup, launcher, single-tap finish, pick-up, jump rise/peak/fall, air-dash | **no** | MISSING (specced in `ASSET_CHECKLIST §A1`; code reuses `dash`/`attack_*`). |
| Gun-execute variants ×4 (Quickdraw/Coup/Skyshot/No-Look) | **no** | MISSING (`COMBOS.md §2`). |

**Werewolf (Gabe) extra — MISSING entirely:** `transform` (⚠ code `Characters.cs:118` plays `"transform"` → fallback) plus the whole wolf sub-set `wolf_idle` `wolf_run` `wolf_slash` `wolf_air_slash` `revert` (`SPRITE_BRIEF.md` "wolf sub-set"). 6 clips, none on disk, none wired.

**Tutorial "zebra" demonstrator — MISSING:** no `assets/sprites/characters/zebra/` atlas; tutorial/vignette code falls back to a stick figure. Wants at least `idle`/`walk`/`attack_side` (+ a punch for the Stage-1 zebra vignette).

**Approx hero clips missing: ~20–25 per hero shared moveset + 7 werewolf-specific + 3 zebra ≈ 90+ clips** (many are "reuse-acceptable" per design, but 4 per-hero clips — `jump`, `air_side/up/down`, `special`(+`transform`) — are live RUNTIME GAPS seen every session).

---

## 2. ENEMIES — PARTIAL (19/19 base atlases, but every special/telegraph state is MISSING)

Design source: `ENEMIES.md`, `ASSET_CHECKLIST.md §A2`. All 19 exist as single 5-clip stick figures
(idle/walk/attack_side/hurt/death). Their special controllers (`NinjaController`, `SnapperController`,
`AntiAircraftController`, `RangedEnemyController`, `Pod`) only ever `Anim.Play` those 5 base clips —
so the distinctive states the design specifies are **not drawn** (they're conveyed by code motion/VFX only):

| Enemy | Base 5 clips | Missing specced states (design) |
|---|---|---|
| Zombie | DONE | hollow-head state / zombify turn |
| Swarmer | DONE | (24px scale ok) |
| Pod | idle/spit/death | pulse/telegraph state |
| Head-Thrower | DONE | self-decapitate, head regrow, fire-blink→BOOM |
| Snapper | DONE | snap-a-Regular-to-sword state |
| Anti-Aircraft | DONE | rock wind-up telegraph |
| Sniper | DONE | scope up/down, red-dot, beret+rifle read |
| Arm-Ripper | DONE | akimbo-pistols, headbutt state, disarmed state |
| Ninja | DONE | teleport-smoke, shuriken throw |
| Pickpocket | DONE | steal + flee states |
| Monkey / Tamer / Flying Monkey | DONE | whistle, swoop, merc-claim |
| Boomergunner | DONE | orbit-gun throw |
| Gatling Gunner | DONE | contort + stream |
| Ground Smasher | DONE | overhead club + shockwave |
| Heavy | DONE | extended punch + gust |

Also: enemy **projectiles** (rocks, shuriken, thrown heads, dimes, boomer-guns) have no bespoke sprites.
**Approx enemy special-state clips missing: ~30–40.**

---

## 3. BOSSES — mostly MISSING (1 arted, 6 bespoke un-arted, 3 big-version need no art)

Design source: `BOSSES.md §5`, `ASSET_CHECKLIST.md §A3`, `ASSET_MANIFEST.md §4`. Registry in
`Bosses.cs` has **10 encounters**. `BossController` loads a bespoke atlas only if `sprites/bosses/<id>`
exists — else tinted `enemy_regular`. Every boss controller `Anim.Play`s only `idle`/`walk`/
`attack_side`/`attack_up`/`death`/`hurt` (their named attacks — ground_smash, enemy_toss, boss_windup,
whoosh_heavy, sniper_dodge, boss_phase_change — are **SFX only**, no sprite states).

| Boss id | Kind | On disk | Status |
|---|---|---|---|
| `phil` | bespoke | idle/walk/attack_side/hurt/death/sharpen | **PARTIAL** — missing `attack_up` (played by bosses → ⚠ fallback), pencil-draw, contact, rooftop-sway, phase states; **pencil-laser kill-beam VFX MISSING**. |
| `burly` | bespoke | — | **MISSING** (barrel-chested wrestler; ground-spike/enemy-toss/charge). |
| `colossus` | bespoke | — | **MISSING** — needs 6 shed-state silhouettes (head→R arm→L arm→torso→R leg→L leg/core) + body-swipe. |
| `helicopter` | bespoke | — | **MISSING** — sky-band actor up to 180px; head-throw, rotor-gust, 2 descend altitudes. |
| `monkey_boss` | bespoke | — | **MISSING** (ringmaster/organ-grinder; dime-toss ×1/×2). |
| `tank` | bespoke | — | **MISSING** — 144×72px vehicle; MG-sweep, hatch-open, tread-roll reposition. |
| `gatlinggunguy` | bespoke | — | **MISSING** — barrage wind-up, chip-stream, melee. |
| `sandwich_bros` | big-version | (reuse) | **NO ART NEEDED** (scaled Tier-1). |
| `big_armripper` | big-version | (reuse) | **NO ART NEEDED** (scaled Arm-Ripper). |
| `boomergunner` | big-version | (reuse) | **NO ART NEEDED** (scaled Boomergunner enemy). |

**Also missing per boss:** HP bar + name card + boss-face icon (see §7 UI). **Approx bespoke boss clips
missing: ~55–65** (6 boss sets × ~8–12 attack/phase/telegraph clips) + Phil top-ups.

---

## 4. WEAPONS — mostly MISSING (pickup PNGs only; no held / swing / fire / projectile art)

Design source: `WEAPONS.md`, `ASSET_CHECKLIST.md §A4`. `WeaponKind` enum (12 real weapons):
Fists, Sword, Shotgun, Boomerang, Pistol, Revolver, Whip, Staff, Bat, Club, Grenade, BallChain, Gatling.
The player attacks with `attack_side` regardless of weapon — **no weapon-in-hand sprites at all**.

| Weapon | Ground pickup | Held / swing / fire | Projectile / effect | Wear/break |
|---|---|---|---|---|
| Sword | yes | MISSING | — | **fresh/worn/chipped DONE** |
| Shotgun | yes (+ held `shotgun.png`, spine segment) | PARTIAL (held art only, no swing) | muzzle only | cock/eject MISSING |
| Pistol | yes | MISSING | tracer MISSING | — |
| Revolver | yes | MISSING | pierce/tracer/casing/cigarette-flick MISSING | — |
| Club | yes | MISSING | — | wear MISSING |
| Bat | yes | MISSING | reflect FX MISSING | — |
| Staff | yes | MISSING | ice/fire/lightning cast FX MISSING | orb decay 6→0 MISSING |
| Whip | yes | MISSING | arc/pull/line + head-rip→grenade MISSING | — |
| Boomerang | yes | MISSING | in-flight + stun MISSING | — |
| Grenade | yes | MISSING | bounce-marker, lob/fastball trails, big/small blast MISSING | — |
| BallChain | yes | MISSING | 4 launch shapes + heavy impact MISSING | — |
| **Gatling** | **MISSING (no folder)** | MISSING | 0.5s barrage + barrel heat-glow MISSING | — |
| Boomerang Gun | **MISSING** | MISSING | orbit + auto-fire MISSING | — |
| Rocket Launcher | **MISSING** | MISSING | launch + blast MISSING | — |
| Monkey Merc | **MISSING** | MISSING | summon-poof / pistol-shotgun-rocket variants / expire MISSING | — |

**Approx weapon sprites missing: ~70+** (in-hand idle/walk/jump + swing kit + fire/finisher +
projectile per weapon), plus the Gatling ground-pickup, plus 3 entirely-missing weapon kinds.
(Note: the "weapons ARE dead stick figures" premise means weapon-hold can reuse — but nothing is drawn yet.)

---

## 5. ENVIRONMENTS — mostly MISSING (1 of 12 backdrop themes; hazards/ambient/checkpoints absent)

Design source: `AREAS.md`, `STAGES.md`, `ASSET_MANIFEST.md §7`, `ENCOUNTERS.md`. Creator's explicit ask:
a **seamless TILEABLE background per theme (no floating props)**.

- **Parallax backdrop sets: 1/12 done.** Only `area1_suburb` (far/mid/near/lane). The other **11
  stage-theme sets are MISSING** (Rocklin/Hwy65, Roseville Galleria, Sacramento Victorian, Airport,
  Hills, Davis causeway, Farm/Ranch, Dixon, Vallejo park, Marin redwoods, Golden Gate, SF streets,
  Salesforce Tower) — each = 3 tiling 360px strips = **~33 layer strips**.
- **Hazards:** `car.png` DONE. MISSING: school bus, taxiing plane, SF trolley/cable-car, roller-coaster
  hazard car (~4 drawn hazard vehicles + telegraphs).
- **Ambient actors:** MISSING — fleeing civilians, mail carrier, kid+bike, jogger, dog/cat/birds,
  dancing zebra, cows/goats/chickens, ground crew, etc. (~10+).
- **Boss-arena prop sets (9):** MISSING — dept-store islands, Victorian plaza, luggage carts, hay-bale
  covers, ride-support pillars, water-tower base, redwood trunks, bridge parked cars, rooftop HVAC.
- **Checkpoint markers (~12 themed):** MISSING.
- **Area props:** Areas 2/3/4 have only generic `house.png`+`tree.png`; full set dressing MISSING.

**Approx environment assets missing: ~33 backdrop strips + ~25 props/hazards/ambient + 12 checkpoints ≈ 70+.**

---

## 6. VFX — PARTIAL (8 core effects done; most P1/P2 effects missing)

Design source: `VFX.md`, `ASSET_MANIFEST.md §6`. DONE: hit_spark, dash_dust, death_burst,
finisher_flash, gust, jump_puff, land_puff, muzzle_flash, blob_shadow.
**MISSING (~20):** air-dash streak, sword wear/break, shotgun spine-eject, boomerang stun, staff
ice/fire/lightning, grenade trails+explosions, head-grenade, whip crack, ball&chain impact,
**time-slow overlay**, sniper tracer/ricochet, **red-dot (2 variants)**, zombie hollow-head,
enemy transforms (snap/rip/contort/teleport-smoke), monkey summon/expire, boss phase flashes,
tower-sway, barrage eviscerate, heal glint, **Phil pencil-laser beam**.

---

## 7. UI / HUD — PARTIAL (core HUD done; screens + boss bars + economy missing)

Design source: `UI.md`, `ASSET_MANIFEST.md §5`. DONE: health bar, special meter, combo popups,
weapon-type icons (fist/gun/sword only), execute prompt, barrage warning, pixel font.
**MISSING (~20):** boss HP bar + name card + **boss-face icon**, objective-boss pip readouts
(Helicopter 6-pip / Tank 2-pip / Colossus 6-segment / Monkey-Boss / Phil notches), money counter
(+full-dime highlight), Monkey-merc cluster (icon+timer ring), boomerang-gun bullet pips, ball&chain
use-pips, **Endless score + mm:ss timer**, per-weapon icons beyond fist/gun/sword; and all **screens**:
title/main-menu, character-select (+4 special icons), difficulty-select (+in-run badge), pause, options
(+rebinding), results/grade, area cards, game-over, button-prompt glyphs.

---

## 8. PICKUPS — DONE (5/5)

coin, dime, heal, merc_token, sniper_rifle all on disk (`assets/sprites/pickups`). Only gap is the
per-weapon ground pickups tracked under §4 (Gatling pickup missing).

---

## 9. VIGNETTES / CUTSCENES — MISSING (bespoke stills)

Design source: `VIGNETTES.md`, `STORY.md`. The 12 in-engine vignettes reuse enemy/weapon art (their
gaps are the §1–4 ones). Bespoke still art is **all MISSING**: **5 hand-drawn intro still-clips** +
**outro/credits stills** + Phil rooftop monologue framing.

---

## 10. Prioritized biggest gaps (most-seen / most-impactful first)

1. **Hero live runtime gaps — `jump`, `air_side/up/down`, `special`(+werewolf `transform`).** Seen
   every single session; the code plays them and they silently fall back to idle/dash, so jumping,
   air attacks, and all 4 signature specials look wrong. Also **wire the existing `sweep` art**
   (drawn but `StartSweep` still plays `attack_side`). Cheapest high-impact win. (~4–6 clips/hero.)
2. **Bosses — 6 bespoke boss art sets (burly, colossus, helicopter, monkey_boss, tank, gatlinggunguy).**
   Every boss fight currently shows a tinted `enemy_regular`; `attack_up` also falls back. The
   climaxes of each area. (~55–65 clips + boss HP bar/name card/face icon.)
3. **Weapon-in-hand + swing + projectile art.** 11 weapons are ground-pickup-only; combat shows no
   weapon in the hand and no bespoke projectiles/effects. Gatling has no pickup at all; Boomerang-Gun,
   Rocket Launcher, Monkey Merc are entirely absent. (~70+ sprites.)
4. **Tileable backdrops — 11 of 12 stage themes missing** (only Area 1 suburb exists), plus hazards
   (bus/plane/trolley/coaster), ambient actors, boss-arena props, and 12 checkpoint markers. The
   creator's explicit "seamless, no floating props" ask. (~70+ assets.)
5. **UI screens + boss/economy HUD.** No boss HP bar/name card/face icon, no money counter, no Endless
   score/timer, and no menu/character-select/difficulty/pause/results screens. (~20+ elements.)

Runner-up: **per-enemy special-state art** (~30–40 clips) and the **~20 missing VFX** (time-slow
overlay, red-dot, staff elements, pencil-laser) — high polish value, lower "first-glance" impact.

---

## 11. Approximate scale of the gap

| Category | Exists | Missing (approx) |
|---|---|---|
| Hero moveset clips (incl. werewolf sub-set, zebra) | 36 (9×4) + 4 portraits | **~90 clips** (of which ~24 are live runtime gaps) |
| Enemy special-state clips + projectiles | 95 base (5×19) | **~30–40** |
| Boss bespoke clips | 6 (Phil) | **~55–65** (6 boss sets) — 3 big-version need none |
| Weapon held/swing/fire/projectile/wear + missing pickups | ~16 pickup/wear PNGs | **~70+** |
| Environment (backdrop strips, hazards, ambient, arena props, checkpoints) | ~7 | **~70+** |
| VFX | 9 | **~20** |
| UI (screens + boss/economy HUD) | ~14 | **~20+** |
| Vignette/cutscene stills | 0 | **~7+** |
| **TOTAL missing sprites/clips/art assets** | | **≈ 350–400+** |

*(Ranges because the design marks many hero/enemy states as "reuse-acceptable"; the low end counts only
distinct required art, the high end counts every specced clip. Audio/music/VO are out of scope.)*
