# this.l — TUNING (authoritative numbers sheet)

> **Scope:** the single source of truth for every gameplay number. Every value here is a **concrete
> first-pass value** chosen to be internally consistent with the LOCKED rules across the design bible.
> Where the source docs gave a range, ONE value is picked and flagged **(tunable)**.
>
> **LOCKED anchors this sheet obeys:** Player HP = 100 · Enemy damage = tier × 7.5 (T1 7.5 · T2 15 · T3 22.5 ·
> T4 30 cap) · Swarm 1–2 · Zombie no hit damage · Gatling 1/hit · Coin 12% · dime = 10 coins · heal drop 5%
> (→20% low-HP; others double low-HP) · meter +10% dmg/fill · sniper special 15 kills (1 fill) → 30 (2 fills) ·
> combo punch/punch/sweep/finisher · telegraphs fist 100ms / sword 150–200ms / ground-smash 1000ms.
>
> **Units:** damage is out of 100 HP. Distance/size in **world units (wu)**; **1 wu = the player's half-height**
> (player sprite = 2.0 wu tall). Time in seconds. All timings assume the un-slowed (1×) clock.

---

## 1. World & camera (Z-band)

| Field | Value | Notes |
|---|---|---|
| Player sprite height | 2.0 wu | reference scale for all sprites |
| Visible screen width | 24 wu | ~12 player-heights across |
| Playfield band (vertical screen share) | **bottom 60%** | **HUD/sky top 40%** (`AREAS.md` §1.1 LOCKED — matched; not tunable) |
| **Z-band depth (near→far)** | **6.0 wu** | continuous, analog; near edge Z=0.0, far edge Z=6.0 |
| Player X-speed on band | see §3 | Z-movement uses same speed value |
| **Sprite depth-scaling** | **100% at Z=0 → 80% at Z=6** | linear −3.33%/wu; floor 80% (`GAMEPLAY_LOOP.md` §3) |
| Ground shadow / Z-marker | ON, 1 blob shadow per actor | reads exact Z (resolves the §3 [LATER]) |
| Bullet/hitbox Z-tolerance | ±0.4 wu | a shot connects only within 0.4 wu depth of target |
| Boss "fixed room" width | 24 wu (scroll stops) | play-band bosses; giant bosses reach down into band |

---

## 2. Player base kit (before character modifiers)

### 2.1 Attack damage (fists)

| Attack | Damage | Notes |
|---|---|---|
| Punch (hit 1 & 2 of string) | **10** | immediate, no wind-up (fist telegraph 100ms is the *enemy's*) |
| Sweep (hit 3) | **12** | knocks enemy DOWN (combo requirement) |
| **Finisher (hit 4)** | **35** | free melee, only lands on a downed enemy |
| Air side | **8** | |
| Air up | **8** | launcher, +knock-up |
| Air down / spike | **12** | spikes; combo starter on landing |
| Up strike (ground) | **10** | anti-air / launcher |
| Down strike (ground) | **10** | low sweep |
| Dash attack (any dir) | **0** | **stagger only** by weight (`PLAYER.md` §3, LOCKED) |
| Air-punch gust reach bonus | +**0.6 wu** hitbox extension | the reach-extender; not a projectile |

> Full fist string on a fresh T1 (40 HP): 10+10+12 = 32, then finisher 35 → **kills in one string**. Intended.

### 2.2 Movement, dash, jump

| Field | Value | Notes |
|---|---|---|
| Base run speed | **7.0 wu/s** | on X and Z |
| Walk speed | 4.5 wu/s | analog stick low-tilt |
| **Dash distance** | **4.0 wu** | grounded burst, **no i-frames** |
| **Dash duration** | **0.20 s** | ~20 wu/s peak during the burst |
| **Dash cooldown** | **0.50 s** | double-tap a WASD dir |
| **Air-dash distance** | **3.5 wu** | once per airtime |
| Air-dash duration | 0.18 s | |
| **Jump height** | **3.0 wu** | apex clears a normal enemy |
| **Jump duration** | **0.80 s** | rise 0.35 / hang 0.10 / fall 0.35 |
| Landing recovery | 0.08 s | |
| Weapon warm-up (baseline) | **0.25 s** | per weapon in §6 (`GAMEPLAY_LOOP.md` §4.1) |
| Low-HP rubber-band threshold | **≤25 HP (25%)** | heal drop 5%→20%, other drops ×2; matches Sniper "<25" kill line |
| Hitstun (taking a hit) | 0.25 s | no i-frames |

### 2.3 Shield Rush (forward double-tap into an enemy)

| Field | Value | Notes |
|---|---|---|
| Grab target | **nearest enemy directly ahead within 2.0 wu** | resolves `PLAYER.md` §3 [ITERATE] |
| Consumes the enemy? | **No — shoves & releases staggered** | unless soak damage kills them (below) |
| Damage it soaks | **up to 40 dmg** absorbed by the human-shield before it drops | e.g. eats gatling stream to close in |
| Shielded enemy takes | 100% of soaked damage | dies if its own HP is exceeded, then rush ends |
| Tier limit | **cannot grab Heavy or a boss** | grabbing them = you bounce & fall (weight rule) |
| Rush speed | 9.0 wu/s | faster than run, to close gaps |
| Cooldown | **1.5 s** | |

### 2.4 Special meter

| Field | Value | Notes |
|---|---|---|
| One full fill (yellow) | **100 meter-points** | |
| **Fist hit value** | **+3.34 pts** | → ~30 punch-hits per fill (LOCKED) |
| Weapon-hit value | **+1.67 pts / hit** | **half the fist rate** = double effort (per HIT, same event as fists — resolves the hit-vs-kill mismatch) |
| **Combo multiplier curve** | 1–4 hits ×1.0 · 5–9 ×1.25 · 10–14 ×1.5 · **15+ ×2.0** | "~15 quick hits surges it" (LOCKED) |
| Combo-drop timeout | 2.0 s without a hit resets the counter | |
| Killed-Sniper rifle pickup | **instant +100 pts (fills one tier)** | free special (`ENEMIES.md` §2.14) |
| Taking damage | breaks the combo counter; does **NOT** drain the meter | resolves §4.3 [LATER] |
| **Tier yellow (1 fill)** | +10% dmg · sniper wipes **15** | |
| **Tier blue (2 fills)** | +20% dmg · sniper wipes **30** | LOCKED 15→30 |
| **Tier green (3 fills / max)** | +30% dmg · sniper wipes **45 (whole screen)** | green = one stronger shot, not banked extras (resolves §4.3 [LATER]) |

### 2.5 Player attack frame data — **[LOCKED]** (at **60 fps**; 1 frame = ~16.7 ms)

> Every attack = **startup** (wind-up before the hitbox is live) → **active** (hitbox live) → **recovery**
> (cannot act). "Cancel window" = the frame from which the next combo hit or a dash/jump may be buffered. These
> are the authoritative timings the build reads; animation frame *counts* (`PLAYER.md` §5) are drawn to fit.

| Attack | Startup | Active | Recovery | Cancel-into | Notes |
|---|---|---|---|---|---|
| **Punch 1** (jab, hit 1) | **4f** | 3f | 8f | next hit @ active+2f | fastest poke |
| **Punch 2** (cross, hit 2) | 5f | 3f | 9f | next hit @ active+2f | |
| **Sweep** (hit 3, knockdown) | 8f | 4f | 14f | finisher only, on a downed target | wider arc; the knockdown setter |
| **Finisher** (hit 4) | 6f | 4f | 16f | — (string ends) | free melee; **only connects on a downed enemy** |
| Side air | 5f | 4f | 10f (or until land) | air-dash / land | |
| Up air (launcher) | 6f | 5f | 12f | — | knock-up |
| Air down / spike | 7f | 5f | landing-lag **8f** | combo starter on landing | spikes airborne foes |
| Up strike (ground) | 6f | 4f | 12f | — | anti-air |
| Down strike (ground) | 6f | 4f | 12f | — | low |
| **Dash attack** (any dir) | 5f | 6f (the lunge) | 10f | jump-cancel on hit | **0 dmg, weight-stagger only** (§2.1) |
| Weapon swing (in-hand, per combo hit) | **+2f** on the matching fist hit above | = fist | +2f | same as fist | ranged weapons bludgeon at fist frames (`PLAYER.md` §5) |

- **Combo cadence:** P1→P2→Sweep chains if the next input lands inside the cancel window; drop it and the
  string resets (matches the **2.0 s** combo-counter timeout, §2.4 — that governs the *meter* counter, this
  governs the *animation* string).
- **Input buffer:** **9f (~0.15 s)** — a press up to 9 frames before an action is actionable still fires
  (matches `COMBOS.md` §1).
- **Whiff vs. hit:** recovery is the same on whiff or hit; there is **no hitstop on normal hits**, only on
  **finishers/kills** (§2.6, `VFX.md` §4).
- **[LOCKED] Which attacks advance the combo string:** the `punch→punch→sweep→finisher` counter advances **only
  on consecutive *side/forward* ground attacks** (the P1/P2/Sweep rows). **Up-strike, down-strike, and all air
  attacks are standalone normals** — they deal their own damage but **do NOT advance or reset** the string;
  they're spacing/juggle tools, not combo steps. The **finisher (hit 4)** additionally requires the
  **same-direction double-tap toward a downed enemy** (`PLAYER.md` §3) — you cannot reach it except after a
  Sweep knockdown. Dropping the string (letting the cancel window lapse) returns you to P1.
- **[LOCKED] Warm-up vs. frame data (no conflict):** the **~0.25 s weapon warm-up** (§2.2, `GAMEPLAY_LOOP.md`
  §4.1) is the aim/ready delay before an **`E`-fire/throw/cast discharges** — it applies to the **E action
  only**. The **+2f on the fist frames** (table above) is the **melee swing** when you bludgeon *through the
  combo* with a weapon in hand. Two different actions: `E` = warm-up then fire; arrow = fist-frames+2 melee.
  They never both apply to the same input.

### 2.6 Universal reaction states — **[LOCKED]** (who freezes, how long)

> One table so every hit reaction reads the same across all 17 enemies + player. Durations in seconds
> (frames in parens @ 60 fps). "Weight" (L/M/H) is the per-enemy class in §4.

| State | Duration | Applies to | Notes |
|---|---|---|---|
| **Enemy hitstun** (normal hit) | **0.18 s (11f)** | L/M enemies | brief flinch + white flash (`VFX.md` hit-flash); H-weight enemies **do not flinch** (super-armor on normals) |
| **L-stagger** (dash-hit, light) | **0.40 s (24f)** | L-weight | stumbles back **1.0 wu**, upright; actionable after |
| **M-stagger** (dash-hit, medium) | **0.55 s (33f)** | M-weight | stumbles back **1.5 wu**; longer opening |
| **H-floors-the-PLAYER** | player down **0.70 s (42f)** | player, on dashing a H-weight/boss | the "wasted getup, **not** invincible" risk (`PLAYER.md` §3) |
| **Knockdown** (sweep, hit 3) | enemy down **1.2 s (72f)** | all non-boss | the **finisher window**; enemy is a valid finisher target this whole time, then auto-gets-up with **0.3 s** getup |
| **Getup** (after any knockdown) | **0.30 s (18f)** | player & enemy | **no i-frames** on either (LOCKED — no-iframe rule) |
| **Launch / juggle hang** (up-air, up-strike, Wrecking Uppercut) | **0.50 s (30f)** airborne | L/M enemies | juggle window; H-weight can't be launched |
| **Hitstop (freeze-frame)** | **3f** on finishers · **5f** on any kill · **0f** on normals | both actors | `VFX.md` §4; scales screen-shake |
| **Player hitstun** (taking a hit) | **0.25 s (15f)** | player | from §2.2; **no i-frames after** |

- **Chip/interrupt rule:** a **normal hit** (hitstun 0.18 s) can be interrupted by the player's next combo hit,
  so juggles/strings work; a **knockdown** (1.2 s) cannot be re-hit for damage until the finisher or getup —
  only the finisher connects on a downed target (§2.5).
- **H-weight super-armor:** Gatling Gunner, Ground Smasher, and Heavy **shrug off normal-hit flinch** but still
  take damage and still **knock down to a sweep** (they are floored like anyone else by hit 3) — this is what
  makes the sweep the answer to armored units.

---

## 3. Character stat modifiers

> Multipliers apply to the §2 base. All four share the moveset; they differ in stats + Special (`CHARACTERS.md`).

| Character | Move speed | Punch dmg | Meter-fill rate | Weapon dmg | Special |
|---|---|---|---|---|---|
| **Tactical (you)** | **×1.12** | **×0.85** | **×1.25** | **×1.15** | Sniper time-slow (no drops) |
| **Shotgunner** | **×0.92** *(tunable — bulk)* | **×1.20** | ×1.00 | **×1.20** (shotgun ×1.35) | Giant Shotgun: wipe ≤T3, keep drops |
| **Werewolf (Gabe)** | ×1.00 | ×1.00 | ×1.00 | ×1.00 | 5 s i-frame slash-all 1HKO, keep drops |
| **Underdog** | **×1.00** (LOCKED: no bump) | **×0.80** | ×1.00 | ×0.80 | Vaporize radius + 30 s +20% buff |

### 3.1 Special payload numbers

| Special | Value | Notes |
|---|---|---|
| Tactical — Sniper | wipes 15/30/45 by tier (§2.4); **drops nothing**; boss dodges >10% HP | LOCKED |
| Shotgunner — Giant Shotgun | **RULE: instakills every T3-and-below on screen** (ignores HP — not a damage number) + **8 wu knockback**; untiered Heavy/Tamer & all bosses survive; **drops stay** | LOCKED ≤T3 |
| Werewolf | **5.0 s** transform, **full i-frames**, every slash = 1HKO, **drops stay**; slash dmg vs boss = 0 above 10% | cooldown = the meter |
| **Werewolf vs. Heavy/untiered** | the 1HKO **DOES kill Heavy, Ground Smasher, Gatling Gunner, Monkey Tamer and every untiered enemy** — it is a raw slash, not a tier-gated special, so no ≤-tier rule applies. **Bosses only** survive (they take slash-dmg 0 above 10% HP, like the other specials). | the one special that ignores weight/tier — its cost is the tiny 5 s window |
| Underdog — Vaporize | close radius **3.0 wu** instant-kill (**drops nothing**, sniper-style; resolves §2.4 [ITERATE]); then **+20% to all dmg for 30 s**; **refreshes, does not stack** | |
| Boss execution (all specials) | only ≤10% boss HP shows the execute prompt | LOCKED (`BOSSES.md` §1) |

---

## 4. Enemies — all 17 (HP · damage · speed · weight · timings)

> **Damage** reconciles to **tier × 7.5** where a tier exists. Untiered/TBD units are assigned an **effective
> damage tier** (noted) so nothing is blank. **Weight:** L/M **stagger** to a dash; **H floors the player**.

| # | Enemy | Tier | HP | Contact/Attack dmg | Move (wu/s) | Weight | Per-enemy timings |
|---|---|---|---|---|---|---|---|
| 1 | **Zombie** | T0 | **30** (body dmg only) | **0** (grab only; mash 6 taps to break, 1.0 s window) | 3.0 | M-stagger | headshot-made lasts **10 s** then drops; pod-spawned dies to any finisher; grab cooldown 2 s |
| 2 | **Regular Melee** | T1 | **40** | **7.5** (punch/jump-kick/slide-kick) | 6.5 | L-stagger | melee windup **100 ms**; slide-kick closes from 4 wu |
| 3 | **Swarmer** | T1b | **12** | **1.5** (chip; LOCKED 1–2) | 8.5 | L-stagger | pod of **5**; spawns on 2–4 sides; **exceeds** the 8-cap as a special swarm (resolves §2.12 [ITERATE]) |
| 4 | **Anti-Aircraft** | T1a | **40** | **7.5** (rock) | 5.0 | M-stagger | rock throw every **2.5 s**, arc telegraph 0.5 s; **20% accuracy vs boomerang** (baits it) |
| 5 | **Head-Thrower** | T2-eff | **45** | **15** (head-grenade); **fire→2 s→BOOM = player death** | 5.5 | M-stagger | throw cooldown **3.0 s**; survives the throw, **regrows head in 4 s** (resolves §2.1 [ITERATE]) |
| 6 | **Snapper (Sword-Maker)** | T2 | **70** | **15** (sword) | 6.0 | M-stagger | sword windup **175 ms**; no T1 nearby → calls in a T1 every **4 s**, max 2 pending; sword decays after 8 hits |
| 7 | **Sniper** | T3-eff | **50** | apex shot → **player to 20 HP** (kill if <25) | 4.0 | L-stagger | **scoped scan 3.0 s → rifle-down 2.0 s** cycle; can't hit a grounded player (resolves §2.14 [ITERATE]); 1 at a time |
| 8 | **Flying Monkey** | T2-eff | **35** | **7.5** (swoop melee) | 7.5 (air) | L-stagger | swoops only when **<2 grounded enemies**; swoop cooldown **3.0 s**; sky-tally exempt |
| 9 | **Monkey Tamer** | untiered | **60** | **0** direct (monkeys deal it) | **4.0** (slower than player) | M-stagger | whistle every **5 s**; **up to 2 monkeys** live; respawn **3 s** after one dies; monkeys deactivate instantly on his death |
| 10 | **Monkey (economy)** | untiered | **30** | **5** (flail) | 6.0 | L-stagger | drops the Monkey-Merc summon (needs a dime); flees at <50% HP |
| 11 | **Arm-Ripper** | T2a | **70** | **15** total (2 pistols, **7.5/shot**) | 6.0 | M-stagger | fire **2 shots/s** from ≤4 wu; **reload 2 s after 6 shots**; disarmed T1 becomes headbutt-only (dmg 7.5) |
| 12 | **Ninja** | T3a | **100** | **22.5** melee · **shuriken 12** | 7.0 | L-stagger | teleport cooldown **3 s**, smoke tell 0.3 s; **2 shuriken per stripped limb**; stars are the telegraphed thrown exception |
| 13 | **Pickpocket** | untiered | **25** | **5** (bump) + steals **all wallet coins** | **9.0** (fastest) | L-stagger | darts in, steals, flees; **kill = 2× coins back** (drops the doubled pile on death) |
| 14 | **Boomergunner** | T2-eff | **80** | **15** across a pass (**5/shot**, up to 3) | 6.0 | M-stagger | throws Boomerang Gun on a fixed orbit; returns in **2.5 s**; can be caught mid-orbit (resolves §2.17 [ITERATE]: catchable) |
| 15 | **Gatling Gunner** | T3 | **110** | **1 HP/hit** stream (LOCKED; ~2 s to live in it) | 4.5 | H-**floors** | contort telegraph **2.0 s** (vulnerable); **1 s burst every 2.5 s**; drops to melee (22.5) inside 3 wu |
| 16 | **Ground Smasher (Zoner)** | T3-eff | **130** | **22.5** (lane shockwave) | 3.0 | H-**floors** | smash every **4.0 s**; overhead windup **1000 ms**; **only 1 shockwave active field-wide**; shockwave travels 12 wu down its Z-row at 10 wu/s |
| 17 | **Heavy ("Bold"/Burly)** | untiered | **220** | **22.5** (extended-reach punch, +0.8 wu reach; emits gust like player) | 5.0 | **H-floors** | punch windup **250 ms**; **max 2 at once**, never flank; **immune to sniper ricochet & headshot-pick** (LOCKED) |

**Pods (shared spawner for Zombie & Swarmer):** HP **50**, destroyable; spits **1 unit every 3 s** up to a
field cap of 6 pod-spawned units; sits at the back Z-edge of the encounter (resolves §2.8/§2.12 [ITERATE]).

**Zombie grab resolution (LOCKED):** on contact the Zombie **grabs and holds** (deals 0 on the grab itself).
While held: the player is **rooted**, cannot move/attack, and **takes full damage from any *other* enemy**
(the grab is a positioning-death setup, not direct damage). **Break-free = mash any 6 attack inputs within a
1.0 s window** (§4 row 1). Outcomes:
- **Break in time →** shove the Zombie back **1.0 wu** (M-stagger), player free, Zombie enters its **2 s grab
  cooldown** before it can re-grab.
- **Fail the mash →** the hold **re-arms for another 1.0 s window** (you get another mash attempt) — the Zombie
  never one-shots you; the danger is the **free hits other enemies land** while you're pinned. A lone Zombie
  with no support is therefore harmless — you always eventually break out.
- **Headshot-made Zombie** expires after **10 s** regardless (releases any grab on expiry); **pod-spawned**
  Zombie dies to any finisher.

**Headshot economy (LOCKED):** pistol/revolver head-lineups and the gatling auto-kill finisher have a **10%**
chance to spawn a 10 s zombie instead of killing. **Sniper special is exempt** (always clean).

### 4.1 Enemy AI edge-case resolutions — **[LOCKED]** (the "what does it do when…" table)

> Resolves the per-enemy `[ITERATE]` fallbacks so a build never hits an undefined behavior.

| Situation | Resolution |
|---|---|
| **Arm-Ripper spawns with no T1 fodder to disarm** | it **arrives already armed** with its own akimbo pistols (it ripped its arms off-screen); the "rip a nearby T1" is a **flavor animation only when a T1 is adjacent** — never a spawn dependency. |
| **Gatling Gunner spawns with no fodder to contort** | same — it **spawns with the gatling in hand**; the "2×T1 / 1×T2 → gatling" line is the *diegetic origin*, not a runtime requirement. Both are **self-sufficient on spawn**. |
| **Monkey Tamer's melee monkeys — stats** | each summoned monkey: **HP 20, contact dmg 5, speed 6.0 wu/s, L-stagger**; **max 2 live**; **deactivate instantly on the Tamer's death** (§4 row 9). They are lighter than the economy Monkey (row 10). |
| **Pickpocket escapes with your coins** | if it **reaches a screen edge**, the stolen coins are **lost permanently** (the risk). Killing it before it exits **drops 2× the stolen pile**. It only steals **once per life**, then flees. |
| **Boomergunner's gun is caught mid-orbit** | catching it (walk into the returning arc) **destroys the gun for that enemy** (it must re-loot/melee) and **staggers the Boomergunner 0.55 s**; the player does **not** gain the gun (it's the enemy's body-part, shatters on catch). |
| **Head-Thrower's thrown head** | uses **grenade fastball physics** (`WEAPONS.md` §3.2) — flat line-drive, **explodes on contact or after 8 wu**; the thrower **regrows its head in 4 s** (§4 row 5) and cannot throw again until it does. |
| **Sniper with the player already downed/grounded** | **holds fire** (can't hit a grounded player, §4 row 7) and **re-scans**; it only fires at an airborne/jumping player (apex punish). |
| **Flying Monkey when ≥2 grounded enemies exist** | **circles/harasses without swooping** until the grounded count drops below 2 (§4 row 8); never idles off-screen. |
| **Enemy would exceed the 8-pursuer cap** | it **holds at a spawn edge** (visible, not attacking) until a slot frees — except Swarmer pods (§0 exception). |

---

## 5. Damage model cross-check (vs. Player HP = 100)

| Source | Dmg | Hits to down player from full | 
|---|---|---|
| T1 | 7.5 | ~13 |
| T2 | 15 | ~7 |
| T3 | 22.5 | ~4 |
| T4 (cap) | 30 | ~4 (rounds to 3.3) |
| Swarm | 1.5 | ~66 (positioning threat, not damage) |
| Gatling stream | 1/hit | dead in ~2 s of unbroken fire (LOCKED) |
| Sniper apex | →20 HP | 1 (and a kill if you were <25) |
| Grenade self-hit / fall-off / fire-boom | instant death or 40 | one-off specials |

---

## 6. Weapons — all 16 (damage · durability/ammo · warm-up · E-fire)

> **Melee combo hits do fist-strength (10)** with the weapon in hand; the **finisher (hit 4) is free melee (35)**.
> **`E` spends ammo/durability** (fire/throw/cast). Throwables: **tap `E` during wind-up** — more taps = flatter.

| Weapon | Tier | E-fire dmg | Durability / Ammo | Warm-up | E-fire behavior |
|---|---|---|---|---|---|
| **Fists** | — | — | ∞ | 0 s | always-ready; the fallback |
| **Sword** | T2 | melee **18/swing**, finisher **45** | **8 connecting hits** (of 5–10) then shatters | 0.20 s | no E-fire; pure melee; blade chips at 6/4/2 left |
| **Shotgun** | T3 | blast **40 + 6 wu knockback** | **5 spine segments** (of 4–6) = 5 shots | 0.25 s | `E` fires + cocks + ejects a spine segment |
| **Boomerang** | T1 | throw hit **8** + **2 s stun** | **lost on first enemy hit** (retrievable on ground) | 0.15 s | `E` throws; misses return to hand |
| **Pistol** | T1 | shot **12**, pierces 3 (**12/6/3** halving) | **mag 8**, then discarded | 0.25 s | **`E` fires any target, any HP**; the **secret-combo execution** (`COMBOS.md` §3) is the only <20%-gated path |
| **Revolver** | T1 | shot **30**, no pierce | **mag 6**, then discarded | 0.30 s | same: `E` fires freely; only the secret-combo execution is <20%-gated |
| **Grenade** | T4 | lob blast **60** (r 3 wu) · fastball blast **35** (r 2 wu) | **1 use** | tap-`E` wind-up | few taps = high lob (bounces 3×→boom); many taps = fastball (boom at 8 wu or after 8 enemies); **self-dmg 40** |
| **Ball & Chain** | T2 | **80/swing** | **3 uses** | 0.40 s | tap-`E` trajectory; **carrying slows player 20%** (move only, not attack); finisher = §COMBOS secret combos |
| **Whip** | T2 | **14/hit**; finisher = head-rip→grenade | **11 connecting hits** (of 10–12) | 0.25 s | up=arc / fwd=pull (drags enemy to you 3 wu) / down=line; finisher auto-dashes you back 4 wu |
| **Staff** | T3 | Ice: **8** +freeze 3 s · Fire: **6/s ×3 s** (18) · Lightning: **12** +stun 1 s +slow | **6 casts** then breaks | 0.35 s | element fixed at pickup; `E` casts; Fire on a Head-Thrower → walking bomb (2 s→boom) |
| **Gatling Gun** | T3 | finisher **0.5 s auto-kill** barrage | **no ammo**; overheats after **5 finisher-bursts OR 20 s equipped** (resolves §3.6 [ITERATE]) | 0.40 s spin-up | melee bludgeon 8 (slow cadence); **no i-frames during barrage** |
| **Monkey Merc** | T4 | pistol shots **8/shot @ 2/s** | **costs 1 dime**; **3 summons/level** then none | 0.5 s summon | 1=pistol/20 s · 2=shotguns/10 s · 3=rockets/5 s; adding a monkey **re-arms all & resets their timers** (resolves §3.7 [ITERATE]) |
| **Club** | T1 | melee **14** + knockback | **10 hits** (resolves §3.7c [ITERATE]) | 0.15 s | no E-fire; short reach, big knockback |
| **Bat** | T2 | melee **12**; reflect | **12 hits**; **reflect window 0.20 s** | 0.15 s | swing-timed reflect of thrown heads/shots back at attacker (resolves §3.7b [ITERATE]) |
| **Boomerang Gun** | T2 | **8/shot** | **10 bullets, 4/pass** (~3 passes) | 0.20 s | `E` throws on a fixed orbit auto-firing; **fists only while out**; throw cooldown 1 s; shot-down = lose remaining bullets |
| **Rocket Launcher** | T4 | blast **70** (r 3 wu) | **3 rockets** (world pickup) | 0.50 s | `E` fires; **self-dmg 35** like grenade (resolves §3.8b [ITERATE]) |

**Tier drop-rate table (per non-swarm kill; coin roll only in Area 3+, §6.1):**

> **Drop *chance* by enemy band is here; *which* weapon rolls is filtered by the area-gate in §6.1.** The
> "weighted toward" column says which tier the roll favors — but a weapon only appears once its area has
> unlocked it (§6.1), so early kills yield the basic-melee pool regardless of band.

| Enemy level band | Weapon-drop chance | Roll weighted toward (within the area-unlocked pool, §6.1) |
|---|---|---|
| T0–T1 | 18% | tier-1 weapons (Sword, Boomerang; +Club/guns once Area 2 unlocks them) |
| T2 | 22% | tier-2 weapons (Whip, Bat, Staff, Ball & Chain, Boomerang Gun — as unlocked) |
| T3 | 26% | tier-3 weapons (Shotgun, Gatling — as unlocked) |
| T4 / miniboss | 35% | tier-4 weapons (Grenade + the strongest unlocked pool) |

*The **Rocket Launcher is a world pickup only** (not in any random pool, `WEAPONS.md` §3.8b), and the
**Monkey Merc drops only from the Monkey stick figure** (`ENEMIES.md` §2.2) — neither is a tier drop. At
low HP (≤25) all weapon-drop chances **double**.

**Ammo sourcing & the no-reload rule (LOCKED — diegetic corpse economy):** every ranged weapon is a **body
part** and arrives **pre-loaded with exactly the ammo in the table** — there is **no reload, no ammo pickup,
no magazine refill.** When the count hits zero the weapon is **spent and auto-discarded** (you drop empty-handed
to fists), so ammo management is "use it or lose the drop," never a resource-hunt.
- **Pistol / Revolver** = the akimbo guns an **Arm-Ripper** carries; killing/disarming one, or the random T1
  drop, hands you **one arm's worth** (mag 8 / 6). Pick up the *second* arm to dual-hand? **No** — one gun at a
  time (picking up a weapon while armed destroys the current, `WEAPONS.md` §1).
- **Shotgun** = a **spine**; its "mag" is its **5 vertebra segments** (§6 row), ejected one per shot — the
  diegetic magazine you can *see* deplete.
- **Boomerang Gun / Ball & Chain / Grenade / Rocket / Staff / Gatling** each carry their listed fixed uses and
  then break/expire the same way. The **Monkey Merc** is the only "ammo from currency" case (costs a dime).
- **Consequence for the build:** no ammo-pickup entities or reload animations need to exist. A gun sprite only
  needs **in-hand → fire → (per-shot ammo readout tick) → empty-discard**.

### 6.1 Area-gated weapon drop pools — **[LOCKED]** (resolves the "first stages hand out only basic melee" rule)

> The §6 tier table says *what a given enemy CAN drop*; this table says *which weapons are UNLOCKED into the
> pool by area*, so the early game stays melee-simple (`ENEMIES.md` §1, `STAGES.md` §3, `WEAPONS.md` §3.9). A
> weapon can only drop once its area is reached, **even if the enemy's tier would otherwise roll it.**

| Area | Weapons UNLOCKED into the drop pool (cumulative) | Notes |
|---|---|---|
| **Area 1** (suburbs/mall) | **Sword, Boomerang** only | basic melee + the throw toy; **no guns yet** (matches "only basic melee early") |
| **Area 2** (Sacramento/airport) | + **Pistol, Revolver, Whip, Bat, Staff, Club** | guns arrive; **Club is a world/airport pickup from Stage 5** (`WEAPONS.md` §3.7c), not a corpse drop |
| **Area 3** (hills/Dixon) | + **Ball & Chain, Grenade, Shotgun** | heavier kit as tier-2/3 enemies appear |
| **Area 4** (Vallejo→SF) | + **Boomerang Gun, Gatling** | full roster live |
| **World pickups (any area, placed)** | **Rocket Launcher** (`WEAPONS.md` §3.8b) — placed near Tank (Stage 9) & SF gauntlet (Stage 12) | never in a random pool |
| **Currency-only** | **Monkey Merc** — from a Monkey + a dime, Area 3 on | not a tier drop |

- **The tier roll (§6) is filtered by this table:** e.g. a Snapper (T2) killed in Area 2 can drop Whip/Bat/
  Staff but **not** Ball & Chain (Area-3-gated) yet. In Area 3+ the full T2 pool is available.
- **No money in Areas 1–2 (`WEAPONS.md` §3.9):** the **12% coin roll (§6) is DISABLED in Area 1 and Area 2**;
  coins begin dropping in **Area 3** with the dime/monkey economy. (Weapon drops still happen in Areas 1–2 per
  this table.) This resolves the "first half has no money" rule that the global 12% line implied everywhere.

### 6.2 Environmental hazard damage — **[LOCKED]** (all hazard numbers, one place)

| Hazard | Damage to player | Damage to enemies | Notes |
|---|---|---|---|
| **Car** (suburb lane) | **40** + knockdown | **40** + knockdown | vehicles **DO hit enemies** (resolves `AREAS.md` §1.4 [ITERATE]); telegraphed by horn + 0.6 s |
| **School bus** (suburb) | **60** + knockdown | **60** + knockdown | bigger, slower, wider lane coverage |
| **Trolley / cable-car** (SF) | **instant death** | **instant kill** (flattens) | except the **Heavy steps aside** (`VIGNETTES.md`); telegraphed by bell + 0.8 s |
| **Jet blast** (airport boss) | **0 dmg**, pushes player **3 wu** | pushes enemies 3 wu | positional only, not damage |
| **Roller-coaster car** (Vallejo) | **50** + knockdown | **50** | on-rail, fixed timing telegraph |
| **Causeway water** (Stage 6) | **10 chip** + respawn on last platform | enemies that fall are **removed (count as killed)** | no drowning death for the player |
| **Pond/puddle** (farm) | **0** (slows movement 30% while in it) | same slow | soft terrain, not damage |
| **Grenade self-blast / rocket self-blast** | **40 / 35** | full blast | your own ordnance (§6) |
| **Head-Thrower fire-boom** (staff-lit) | **instant death if adjacent** | kills the lit enemy | the walking-bomb interaction (`WEAPONS.md` §3.5) |
| **Fall off Salesforce rooftop** | **instant death** | enemies knocked off = killed | Phil arena only |

---

## 7. Bosses — all 7 bespoke + big-version rule

> Fight-length target **< 2:00** for every boss **except Phil**. HP tuned so a competent player hits the target;
> difficulty is pressure/reads, not HP bloat (`BOSSES.md` §1).

| Boss | Area | HP | Phase thresholds | Attack dmg | Win condition / objective count | Length target |
|---|---|---|---|---|---|---|
| **Sandwich Bros / big Tier-1** | 1 (suburbs) | **160** (2× kit, big-version) | 50% | punch **11** | HP depletion; **solo = 1 big T1; 2P = 2 + a miniboss** | 1:15 |
| **Burly Macho Guy** | 1 (dept store) | **300** | 66% (200) · 33% (100) | ground-spike **22.5** · **enemy-toss 40** | HP depletion | 1:45 |
| **Colossus** | 2 (Sacramento) | **240** = **6 pieces ×40** | shed at 4 & 2 pieces (speeds up) | body swipe **22.5** | **whip off 6 stick-figure pieces**; torn pieces become T1 adds | 1:50 |
| **Helicopter** | 2 (airport) | **objective** (not HP-depleted) | after **3 hits** it descends lower & fires faster | thrown heads **15** (max 2 on screen) | **6 reflected heads OR 4 lobbed grenades = down** (each reflect/lob = 1 objective hit; a lobbed grenade counts as **1.5** so 4 finish it); main-boss-only | 1:40 |
| **Monkey Boss** | 3 (farm) | **200** (only your mercs damage him) | 60% · 30% (throws dimes faster) | **0** direct; his mercs (T1 pistol 7.5) | proxy war: catch dimes → your mercs shoot him down; boss mercs ignore the 3-death cap | 1:55 |
| **big Arm-Ripper** | 3 (Dixon) | **280** (boss-scale) | 66% · 33% | pistols **7.5/shot @ 2/s** | HP depletion; caps the Dixon boss rush | 1:50 |
| **Tank** | 4 (Vallejo) | objective (**2 grenade drops**) | **after drop 1** (MG pattern intensifies) | MG stream **1/hit**; direct hit while mounting **22.5** | **climb + drop grenade in hatch ×2**; arena adds drop only grenades | 1:50 |
| **Boomergunner boss** | 4 (Marin) | **320** (boss-scale, 80×4) | 66% · 33% (2 guns orbiting) | boomerang-gun shots **5/shot** (base — see ranged note) | HP depletion | 1:45 |
| **Gatling Gun Guy** | 4 (Golden Gate) | **260** | 66% · 33% | **barrage = instant death if caught in the open** (LOCKED); melee 22.5 | HP depletion; **hide behind cars** on the **~5 s "BARRAGE INCOMING"** cycle; Shield-Rush the fodder version | 1:55 |
| **Phil (FINAL)** | Finale | **500**, gated behind sharpen windows | 100→75→50→25→**execute** (5 windows) | contact **15** · summons deal their own dmg · **fall off tower = instant death** | invuln while drawing; **~4 s sharpen window** (of 3–5) is the only opening; per-window damage cap **~100 (20%)**; killed **only** by the scripted **pencil-laser finisher** | **exempt** (~5–8 min) |

**Big-version scaling rule (concrete):**

| Class | Size | HP multiplier | Damage multiplier |
|---|---|---|---|
| **Miniboss** | **1.2×** | **×2.0** | **×1.25** |
| **Boss** | **2.0×** | **×4.0** | **×1.5** |

(e.g. Regular Melee 40 HP → big-version boss 160 HP, 7.5→11 dmg — matches Sandwich Bros above.)

- **Ranged big-version bosses keep BASE per-shot damage — [LOCKED override].** The ×1.5 damage multiplier
  applies to **melee/contact** attacks only. For **ranged** big-version bosses (**big Arm-Ripper** 7.5/shot,
  **Boomergunner boss** 5/shot) the **per-shot number stays at base**; they scale their threat through **higher
  HP (×4), faster fire, and more projectiles**, not bigger bullets — so a ranged boss can never chip you to
  death in two hits. This is why the §7 table lists base per-shot values for those two. **Melee** big-version
  bosses (Sandwich Bros 7.5→11) do take the ×1.5.
- **The §7 table is authoritative for the 9 placed bosses;** the formula above is for **auto-generated**
  big-versions (catch-up minibosses, Endless elites) and for cross-checking the placed ones.

**Boss & meter rules (LOCKED, restated):** specials only work ≤10% boss HP (execute prompt); unspent meter
**carries over**; sniper visibly dodges above 10%. **The 10% rule covers EVERY area-capping boss** — the 7
bespoke bosses **and** the big-version area bosses (Sandwich Bros, big Arm-Ripper, Boomergunner). Only
**catch-up minibosses / big-version *non-boss* elites** are sniper-killable like normal enemies (resolves
`BOSSES.md` §4 [ITERATE]). *(So the sniper can never one-shot an area boss.)*

---

## 8. Meta — checkpoints, continues, Endless, catch-up

### 8.1 Checkpoints & continues

| Field | Value | Notes |
|---|---|---|
| **Checkpoint cadence** | **1 at stage start + 1 mid-stage + 1 at the boss door** | ~2–3 per ~15–18 min stage (`ENCOUNTERS.md` §0); bossless stages = start + mid |
| Heal on checkpoint | **full HP restore** | forgiving over a 3–4 hr run |
| Money on checkpoint | resets each **stage** (LOCKED, `UI.md` §3.4) | not stored across checkpoints |
| **Continues per run** | **3** | then game-over → title **(tunable)** |
| Continue cost | resets to the last checkpoint; **wallet cleared**; special meter emptied | forgiving-but-not-free |
| Lives before a continue | 1 (death → spend a continue) | no separate life stock |

### 8.2 Catch-up miniboss trigger (concrete "too fast" metric)

| Field | Value |
|---|---|
| **Metric** | rolling **average kill interval** over the last 10 kills |
| **Trigger** | average kill interval **< 3.0 s** (i.e. clearing faster than 1 kill / 3 s) **for 20 s straight** |
| Injection | spawn **1 recurring miniboss** (big-version enemy or scaled-down boss) at the front Z-edge |
| Re-arm cooldown | **90 s** before the trigger can fire again |
| Cap | max **1 catch-up miniboss active** at a time |

### 8.3 Endless Mode scaling curve

> Base start (difficulty 0), never lets up; **spawns a fresh pod whenever only 2 enemies remain** (LOCKED).

| Field | Value | Notes |
|---|---|---|
| Refill trigger | on-screen enemies **≤ 2** | LOCKED |
| Pod size (spawn) | **max(3, 2 + floor(minute/1))** | +1 to the pod each elapsed minute |
| Concurrent enemy cap | **8 + floor(minute/2)** | grows 1 every 2 min (swarms still exceed it) |
| **Tier ramp** | unlock next tier every **3 min**: T0–1 (0–3m) → +T2 (3m) → +T3 (6m) → +T4/untiered (9m) → full roster (12m+) | |
| Enemy stat ramp | **+5% HP and +3% damage per minute**, capped at +150% HP / +90% dmg | HP/dmg creep past the roster unlock |
| Miniboss cadence | inject one every **5 min** (recurring big-versions) | |
| Boss cadence | inject a main boss every **10 min** | at boss-scale, from the placed pool |
| Spawn interval floor | never faster than a new pod every **4 s** | keeps it readable (`VFX.md` bullet budget) |
| Economy/weapon rules | **campaign rules apply** (coins, dimes, monkeys, decay) | Endless is the sanctioned playtest sandbox |
| End condition | endless until death; score = kills × time-survived multiplier | leaderboard **(tunable)** |

---

## 9. One-line assumptions (where a value was inferred, not stated)

1. **Shotgunner move speed ×0.92** — bulk implies a small speed tax; the doc locks only "bigger/bulkier," so a
   modest penalty is assumed (all other characters' speeds are locked).
2. **Green meter tier = one stronger 45-kill shot** (not banked extra shots) — chosen as the read most
   consistent with "each fill AMPLIFIES, doesn't bank extra shots" (`GAMEPLAY_LOOP.md` §4.3).
3. **Untiered/TBD enemy damage** assigned an *effective tier* on the tier × 7.5 ladder (marked "-eff") so no
   damage cell is blank; the LOCKED per-enemy exceptions (Zombie 0, Swarm 1.5, Gatling 1, Sniper→20) override.
4. **Every area-capping boss (bespoke + big-version) carries the 10% rule; only catch-up minibosses/non-boss
   elites are sniper-killable** — the reading most
   consistent with "specials only work on **a boss** under 10%."
