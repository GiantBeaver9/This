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
| Player sprite height | 2.0 wu | reference scale for all sprites; **= 48 px at 1 wu = 24 px** (`ASSET_MANIFEST.md` §0) |
| Visible screen width | **~26.7 wu** | 640 px internal render ÷ **24 px/wu** = 26.67 wu (`ASSET_MANIFEST.md` §0); ~13 player-heights across |
| Playfield band (vertical screen share) | **bottom 60%** | **HUD/sky top 40%** (`AREAS.md` §1.1 LOCKED — matched; not tunable) |
| **Z-band depth (near→far)** | **6.0 wu** standard | continuous, analog; near edge Z=0.0, far edge Z=6.0. **Boss arenas may set a deeper band** (listed per arena in `ENCOUNTERS.md`, up to **8.0 wu**) to fit big/airborne bosses — the band widens for that fight, then returns to 6.0. Depth-scaling (below) rescales to the arena's far edge. |
| Player X-speed on band | see §3 | Z-movement uses same speed value |
| **Jump kinematics (LOCKED)** | **height 3.0 wu** (matches §2.2, apex clears a normal enemy) · **airtime 0.8 s · horizontal distance 5.0 wu** at full run | a plain forward jump covers **5.0 wu** horizontally (the horizontal air speed is a fixed **6.25 wu/s**, *not* the 7.0 run speed — the small air-control tax); this is the authoritative jump distance and **supersedes the "≈4 wu" shorthand** in `ENCOUNTERS.md` §0. So a **4 wu causeway gap clears on a plain jump** with margin. |
| **Air-dash reach (LOCKED)** | **+3.5 wu** horizontal | one air-dash per jump; jump 5.0 + air-dash 3.5 = **8.5 wu total air reach** (clears any 3–5 wu gap trivially) |
| **Sprite depth-scaling** | **100% at Z=0 → 80% at Z=6** | linear −3.33%/wu; floor 80% (`GAMEPLAY_LOOP.md` §3) |
| Ground shadow / Z-marker | ON, 1 blob shadow per actor | reads exact Z (resolves the §3 [LATER]) |
| Bullet/hitbox Z-tolerance | ±0.4 wu | a shot connects only within 0.4 wu depth of target |
| **[LOCKED] Z → screen-Y projection** | **24 px of screen-Y per 1.0 wu of Z** (= the same 24 px/wu as X) | an actor at Z=0 (near) sits at the **bottom of the play band**; each +1.0 wu of Z lifts its screen-Y by 24 px, so the full 6 wu band spans **144 px** of the 216 px play band, leaving headroom for sprite height. This is the core 2.5D constant (resolves `GAMEPLAY_LOOP.md` §3 [LATER]); it stacks with depth-scaling (row above) — far actors are both higher on screen and smaller. |
| **[LOCKED] "Z-row" = 1.0 wu; per-attack Z-reach is pinned INDIVIDUALLY (they do NOT all equal one row)** | see list | The analog Z-band is diced into 1.0 wu conceptual rows only for *describing* attacks; each attack's actual Z-reach is one of these fixed half-widths, measured from the attacker's Z: **melee punch/P1/P2/normal = ±0.4 wu** (same as the projectile tolerance) · **combo sweep = ±1.0 wu** (the wide arc) · **Ball & Chain Ground Zero = ±1.5 wu** (its lane + one full row each side = 3 rows / 3.0 wu Z-span) · **Whip down-line = ±0.4 wu** (one row) but across its **full 4.0 wu X-length** · **Tank MG sweep = one 1.0 wu row at a time (±0.4 wu)** · **projectile connect tolerance = ±0.4 wu**. "±1 Z-row" as loose prose elsewhere means *the sweep's ±1.0 wu* unless a doc says otherwise. |
| **[LOCKED] Facing** | **set by the most recent of: last horizontal MOVE, or last horizontal ATTACK arrow** (whichever happened later) | facing is **left or right only** (the Z-axis doesn't flip the sprite). `E`-fire, gun shots, Shield Rush, the whip pull, and "directly ahead" targeting all use this facing. Pressing an attack arrow left while running right **turns the character to face left** (attack direction wins for that frame and sets facing). Neutral (no input) holds the last facing. |
| **[LOCKED] "Directly ahead" targeting cone** | a target counts as "directly ahead" if it is **on the facing side (X) AND within ±0.8 wu of the player's Z** (its own tolerance — a little wider than a punch so aiming feels forgiving; not a "Z-row") | used by Shield Rush (within 2.0 wu) and the gatling barrage (within 8 wu on-row) — one shared definition of "ahead". |
| **Pursuer separation radius** | **1.0 wu** min center-to-center | hard-separation (`GAMEPLAY_LOOP.md` §8.2) — pursuers push apart to keep this gap, so **you never eat two overlapping hitboxes at once** |
| **Attacker slots (melee ring)** | **max 2 enemies attack at once**; the rest hold a **standoff ring at ~2.5 wu** | the "circle and wait" behavior; others step in as a slot frees (still within the 8-pursuer cap) |
| **Ranged standoff distance** | **each ranged enemy holds at ITS OWN pinned hold-distance** (≤ its max reach; it fires from here and backs off if the player closes inside it): **AA rock 8 wu** (max reach 10 wu, §6.3 — holds at 8 to keep margin), **Head-Thrower 7 wu**, **Sniper 12 wu** (max range = whole screen, holds far), **Arm-Ripper ≤4 wu** (must close, §1 short-range rule), **Boomergunner 6 wu** (its thrown-gun orbit is an oval 5 wu wide × 3 wu deep, §6.3) | not one global number — each ranged enemy has a single pinned hold-distance, always ≤ its firing range |
| Boss arena width | **per-boss, 24–34 wu** (`ENCOUNTERS.md`) | **"camera-locked" = the level stops advancing** (no forward scroll to new ground); if the arena is wider than the ~26.7 wu screen the **camera pans within the arena box** (bounded, ≤ ±3.7 wu). Giant bosses reach down into the band. |

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

**[LOCKED] Melee reach & hitbox sizes** (horizontal reach from the actor's center; the gust adds +0.6 wu to
the player's fist/weapon):

| Attacker / weapon | Base reach | Hitbox height (Z + vertical) | Notes |
|---|---|---|---|
| Player **fist / punch** | **1.2 wu** (→ **1.8** with gust) | Z-reach **±0.4 wu**, ~1.5 wu tall | the baseline all "+0.6 gust" and weapon reaches build on |
| Player **Sword** | **1.8 wu** (→2.4 gust) | as fist | the reach upgrade |
| Player **Club / Bat** | **1.0 / 1.4 wu** | as fist | club short + knockback; bat swing arc |
| Player **Whip** | up=arc **2.5 wu** / fwd=pull **3.0 wu** / down=line **4.0 wu** | line hits the whole Z-row | the long-reach crowd tool |
| Player **sweep (hit 3)** | **1.5 wu**, wider arc | Z-reach **±1.0 wu** (catches a small clump) | the knockdown setter |
| **Regular Melee** enemy | **1.0 wu** (slide-kick closes from 4 wu) | as fist | inside the player's 1.2/1.8 — the spacing edge |
| **Snapper** (sword) | **1.7 wu** | as fist | "longer reach" pinned |
| **Heavy** punch | **1.8 wu** (+0.8 reach, emits gust) | tall | out-reaches the player's bare fist |
| **AA rock** throw range | up to **10 wu** (arc) | lands in a 1 wu splash | the long lobber |
| **Ninja shuriken** | **12 wu** straight | thin | telegraphed thrown exception |
| **Sniper** | full screen (hitscan, apex only) | head-line | can't hit grounded |
| Enemy **Boomergunner** orbit | **oval 5 wu wide × 3 wu deep** (`WEAPONS.md` §3.8 — not a 5 wu *radius*) | shots along the loop | catchable mid-orbit |

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
| **Heal pickup restore amount** | **flat +25 HP (25% of max)** per pickup — **no full heals exist** (LOCKED) | the *drop rate* is 5%→20% at low HP (`ENEMIES.md` §4b); every heal restores the same flat 25, capped at 100. There is **no big/full heal** anywhere (not in boss arenas, not at checkpoints — a checkpoint respawn restores to full only on a *death*, not as a pickup). |
| Hitstun (taking a hit) | 0.25 s | no i-frames |

### 2.3 Shield Rush (forward double-tap into an enemy)

| Field | Value | Notes |
|---|---|---|
| Grab target | **nearest enemy directly ahead within 2.0 wu** | resolves `PLAYER.md` §3 [ITERATE] |
| Consumes the enemy? | **No — shoves & releases staggered** | unless soak damage kills them (below) |
| Damage it soaks | **up to 40 dmg from ANY source** (incoming projectiles AND melee swings that hit the shield body) absorbed before it drops | e.g. eats gatling stream or a punch to close in; every hit that would have struck the player-behind-shield instead lands on the shield and counts toward the 40 |
| Shielded enemy takes | 100% of soaked damage | dies if its own HP is exceeded, then rush ends |
| Tier limit | **cannot grab any H-weight enemy (Heavy, Ground Smasher, Gatling Gunner), any miniboss, or any boss** | grabbing one = you **bounce off & fall** (0.70 s H-floors the player, §2.6) — the weight rule; **all L/M-weight regulars are grabbable** |
| Rush speed | 9.0 wu/s | faster than run, to close gaps |
| **[LOCKED] Duration & termination — the rush ends at the FIRST of:** | (a) the shield **absorbs 40 cumulative dmg** and drops; (b) the shield's **own HP is exceeded** by soaked damage (it dies, you release); (c) you travel a **max 8.0 wu** from the grab point; (d) you **release the forward input**; (e) you **hit a wall / arena edge / an ungrabbable enemy** (H-weight/boss body). On ANY exit the grabbed enemy is **shoved forward 1.0 wu and released staggered (M-stagger 0.55 s)**, and the player returns to neutral. | a hard cap so the move can't be held indefinitely |
| Cooldown | **1.5 s** (starts when the rush ends) | |

### 2.4 Special meter

| Field | Value | Notes |
|---|---|---|
| One full fill (yellow) | **100 meter-points** | |
| **Fist hit value** | **+3.34 pts** | → ~30 punch-hits per fill (LOCKED) |
| Weapon-hit value | **+1.67 pts / hit** | **half the fist rate** = double effort (per HIT, same event as fists — resolves the hit-vs-kill mismatch) |
| **Combo multiplier curve** | 1–4 hits ×1.0 · 5–9 ×1.25 · 10–14 ×1.5 · **15+ ×2.0** | "~15 quick hits surges it" (LOCKED) |
| Combo-drop timeout | 2.0 s without a hit resets the counter | |
| Killed-Sniper rifle pickup | **instant +100 pts (fills one tier)** | free special (`ENEMIES.md` §2.14) |
| **Overfill cap** | meter **caps at green (300 pts / 3 fills)** — excess is **discarded** | if the +100 rifle lands while already at green, it's wasted (no 4th tier); the meter never banks past max |
| Taking damage | breaks the combo counter; does **NOT** drain the meter | resolves §4.3 [LATER] |
| **Tier yellow (1 fill)** | +10% dmg · sniper wipes **15** | |
| **Tier blue (2 fills)** | +20% dmg · sniper wipes **30** | LOCKED 15→30 |
| **Tier green (3 fills / max)** | +30% dmg · sniper wipes **45 (whole screen)** | green = one stronger shot, not banked extras (resolves §4.3 [LATER]) |

> **[NOTE] The sniper's 15/30/45 kill-count tiers are primarily an ENDLESS / dense-crowd feature.** In the
> campaign the on-screen max is ~14 (8 pursuers + 6 pod-swarmers), so **yellow (15) already full-clears** — the
> blue/green kill-count tiers mostly matter in **Endless** (concurrent cap grows, §8.3). In campaign, the value
> of charging past yellow is the **+10%→+30% passive damage buff** (which helps against bosses), not extra
> sniper kills. This is intentional, not a bug: the tiers scale into the mode that needs them.

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
- **[LOCKED] Combo string & the execute gate (the "primed" state machine, `PLAYER.md` §3):** the string is
  `P1 → P2 → sweep → finisher`. After **P1→P2 connect the string is PRIMED**; the next directional presses are
  read as the **sweep + finisher SAME-DIRECTION DOUBLE-TAP** (`→→ ←← ↑↑ ↓↓`), **not** as standalone normals.
  The **first tap sweeps** — `→→/←←/↓↓` knock the enemy **DOWN**, `↑↑` **launches** it — and the **second tap
  finishes** into that state. **Outside a primed string**, `↑`/`↓`/air presses are **standalone normals**
  (up-strike launcher etc.) that do NOT advance the string. So the same key means different things primed vs.
  neutral — that is the resolution, no conflict. **Execute lands only on a swept/launched enemy**; a standing
  target's second tap is a normal hit. **Already-downed** enemies take a **single-tap** finish (no re-sweep).
  Dropping the string returns you to P1. The **gun `<20%` execution** (`COMBOS.md` §2) is a finisher variant
  and inherits all of this.
- **[LOCKED] Warm-up vs. frame data (no conflict):** the **~0.25 s weapon warm-up** (§2.2, `GAMEPLAY_LOOP.md`
  §4.1) is the aim/ready delay before an **`E`-fire/throw/cast discharges** — it applies to the **E action
  only**. The **+2f on the fist frames** (table above) is the **melee swing** when you bludgeon *through the
  combo* with a weapon in hand. Two different actions: `E` = warm-up then fire; arrow = fist-frames+2 melee.
  They never both apply to the same input.
- **[LOCKED] Reading the §6 "warm-up" column for pure-melee weapons (Sword 0.20, Whip 0.25, Club 0.15, Bat 0.15,
  Ball & Chain 0.40):** these weapons have **no `E`-fire**, so their warm-up value is the **swing start-up on the
  weapon's first combo hit** (the wind-up before the blade/club connects) — the melee equivalent of the E-fire
  warm-up, layered on the fist-frame timing (it *replaces* the "+2f" for that weapon, it does not stack). So a
  Sword's first swing has a 0.20 s start-up; subsequent chained hits use the combo frame data (§2.5). For
  `E`-fire weapons the column is the E discharge delay as above. One column, two readings by weapon type — never
  both for one weapon.

### 2.6 Universal reaction states — **[LOCKED]** (who freezes, how long)

> One table so every hit reaction reads the same across all 17 enemies + player. Durations in seconds
> (frames in parens @ 60 fps). "Weight" (L/M/H) is the per-enemy class in §4.

| State | Duration | Applies to | Notes |
|---|---|---|---|
| **Enemy hitstun** (normal hit) | **0.18 s (11f)** | L/M enemies | brief flinch + white flash (`VFX.md` hit-flash); H-weight enemies **do not flinch** (super-armor on normals) |
| **L-stagger** (dash-hit, light) | **0.40 s (24f)** | L-weight | stumbles back **1.0 wu**, upright; actionable after |
| **M-stagger** (dash-hit, medium) | **0.55 s (33f)** | M-weight | stumbles back **1.5 wu**; longer opening |
| **H-floors-the-PLAYER** | player down **0.70 s (42f)** | player, on dashing a H-weight/boss | the "wasted getup, **not** invincible" risk (`PLAYER.md` §3) |
| **Knockdown** (sweep, hit 3) | enemy down **1.2 s (72f)** | **regular enemies only — NEVER bosses or minibosses (LOCKED, global)** | the **finisher window** — the enemy is finisher-able this whole 1.2 s (then auto-gets-up, **0.3 s** getup). Two entry paths (no conflict with the 0.35 s double-tap timing): **(a)** sweep a *standing* enemy with the primed double-tap (`→→` etc., the two taps ≤ **0.35 s** apart, `COMBOS.md` §1) — the second tap finishes; **(b)** an **already-downed** enemy (from an earlier sweep still in its 1.2 s, or a Ground-Zero knockdown) is finished by a **single tap** toward it, any time inside the 1.2 s. The 0.35 s is the *double-tap* timing; the 1.2 s is how long a downed enemy stays finisher-able. |
| **Getup** (after any knockdown) | **0.30 s (18f)** | player & enemy | **no i-frames** on either (LOCKED — no-iframe rule) |
| **Launch / juggle hang** (up-air, up-strike, Wrecking Uppercut) | **0.50 s (30f)** airborne | L/M enemies | juggle window; H-weight can't be launched |
| **Hitstop (freeze-frame)** | **3f** on finishers · **5f** on any kill · **0f** on normals | both actors | `VFX.md` §4; scales screen-shake |
| **Player hitstun** (taking a hit) | **0.25 s (15f)** | player | from §2.2; **no i-frames after** |

- **Chip/interrupt rule:** a **normal hit** (hitstun 0.18 s) can be interrupted by the player's next combo hit,
  so juggles/strings work; a **downed** enemy (in its 1.2 s knockdown) **takes no further normal hits** — only
  the **finisher** connects on it (a single tap toward it, §2.5). This is why Ground Zero's mass-knockdown sets
  up single-tap finishes rather than re-sweeps.
- **[LOCKED] What the down-immunity blocks vs. doesn't.** The "no further hits while downed" rule blocks **only
  the combo's own melee normals** (punch/punch/sweep) — it exists so juggle-spam can't chain-lock a floored
  enemy; the **finisher** is the sanctioned way to hit a downed body. **Everything else still damages a downed
  enemy normally:**
  - **AoE / physics attacks** — **grenade & rocket blasts, Ball & Chain arc/slam, the gatling stream, hazards
    (cars/trolley/fire), reflected projectiles** — all hit downed enemies for full damage (they're area/physics,
    not the combo normal). A grenade thrown into a pile of downed enemies kills the pile.
  - **The sniper special** ricochet **also hits downed enemies** (it targets any valid head-pick; being downed
    doesn't exempt an enemy from the wipe).
  - **Straight gun shots** (pistol/revolver/gatling `E`) still *travel through* and *damage* a downed enemy on
    their plane, but as a **body shot** (no headshot/zombify — the head is off the shot plane, headshot predicate
    §4). This is the one "gun vs downed" nuance: damage yes, headshot no.
- **H-weight super-armor:** Gatling Gunner, Ground Smasher, and Heavy **shrug off normal-hit flinch** but still
  take damage and still **knock down to a sweep** (they are floored like anyone else by hit 3) — this is what
  makes the sweep the answer to armored *regular* units.
- **[LOCKED — GLOBAL] Status-effect immunity.** **Bosses, minibosses, and H-weight enemies (Heavy, Ground
  Smasher, Gatling Gunner) are IMMUNE to all crowd-control** — **freeze (staff ice), stun (staff lightning /
  boomerang 2 s), slow, and the whip's pull-as-CC (yanking them out of position)** do **not** apply to them
  (they still take the *damage* portion). Only **regular L/M-weight enemies** get frozen/stunned/slowed/pulled.
  This mirrors the knockdown-immunity rule and prevents a frozen-boss balance break. (CC lands fully on normal
  enemies.)
  - **[LOCKED] Scripted-objective exception — this immunity blocks CC, NOT an objective interaction.** Where a
    boss's *win condition* is a weapon interaction, that interaction is an **objective hit, not crowd control**,
    and it always lands: the **Colossus's forward whip-pull tears off one stick-figure piece** (`BOSSES.md`
    §5.4) — this is the objective and is **never** negated by the CC-immunity above (the immunity only stops the
    whip from *repositioning* the Colossus, which it doesn't do anyway — it removes a piece in place). Likewise
    the **Helicopter's reflected heads / lobbed grenades** register as objective hits. CC-immunity only ever
    blocks the *status/displacement* rider of an attack, never a boss's scripted objective mechanic.
- **[LOCKED — GLOBAL] No boss or miniboss can ever be swept or knocked down.** The sweep→finisher route is for
  **regular enemies only** (H-weight Heavy included — it is a regular enemy, not a boss). **Every boss encounter
  (all 10: the 7 bespoke + the 3 big-versions Sandwich Bros / big Arm-Ripper / Boomergunner) and every catch-up
  miniboss is immune to knockdown/sweep** — they have permanent super-armor vs. the sweep. Bosses are defeated
  only by **HP depletion, their objective, or their scripted kill** (Phil's pencil-laser); the low-HP special
  execution (≤10% HP) is the only "finisher-like" thing that touches them (`BOSSES.md` §1). So the finisher
  double-tap does nothing special to a boss — its hits land as normal melee.

---

## 3. Character stat modifiers

> Multipliers apply to the §2 base. All four share the moveset; they differ in stats + Special (`CHARACTERS.md`).

| Character | Move speed | Punch dmg | Meter-fill rate | Weapon dmg | Special |
|---|---|---|---|---|---|
| **Tactical (you)** | **×1.12** | **×0.85** | **×1.25** | **×1.15** | Sniper time-slow (no drops) |
| **Shotgunner** | **×0.92** *(tunable — bulk)* | **×1.20** | ×1.00 | **×1.20** (shotgun ×1.35) | Giant Shotgun: wipe ≤T3, keep drops |
| **Werewolf (Gabe)** | ×1.00 | ×1.00 | ×1.00 | ×1.00 | 5 s i-frame slash-all 1HKO, keep drops |
| **Underdog** | **×1.00** (LOCKED: no bump) | **×0.80** | ×1.00 | ×0.80 | Vaporize radius + 30 s +20% buff |

- **[LOCKED] Which multiplier applies to what:**
  - **Punch dmg** ×  → the **fist combo** (P1/P2/sweep/finisher at fist values) **and every gun-bludgeon hit**
    (ranged weapons swung through the combo at fist-strength 10, `TUNING.md` §6 / `PLAYER.md` §5). A bludgeon is
    a melee hit, so it rides the **Punch** multiplier, **not** Weapon dmg. The **free-melee finisher (35)** that
    ranged weapons use is likewise a Punch-multiplier hit.
  - **Weapon dmg** ×  → only a **genuine melee weapon's own swing/finisher values** (Sword 18/45, Whip 14,
    Club 14, Bat 12, Ball & Chain 20/50 & its 80 launch, Staff casts) and a fired weapon's projectile/blast
    payload. So a Shotgunner firing the shotgun gets ×1.35, but pistol-whipping gets his ×1.20 **Punch**, not
    weapon, bonus.
  - **Enemy HP, status durations, and the §2.4 meter buff** are never touched by these per-character knobs.

### 3.1 Special payload numbers

| Special | Value | Notes |
|---|---|---|
| Tactical — Sniper | wipes 15/30/45 by tier (§2.4); **drops nothing**; boss dodges >10% HP | LOCKED |
| Shotgunner — Giant Shotgun | **RULE: instakills every T3-and-below on screen** (ignores HP) + **8 wu knockback**; **also kills untiered *fodder* (Pickpocket, economy Monkey) and the T2-eff Flying Monkey**. **Survivors: the Heavy, all H-weight (Ground Smasher, Gatling Gunner), the Monkey Tamer, and every MINIBOSS take 45 dmg + knockback** (not instakill — minibosses are big-version elites above the ≤T3 instakill gate; the sniper still one-shots them but this AoE does not). **Bosses take NOTHING above 10% HP** (all specials are negated above 10%, `BOSSES.md` §1 — no chip, no knockback on a boss; at ≤10% the 5 HP-depletion bosses execute). **Drops stay**. **Arc geometry:** a **forward cone, 6/8/10 wu long × ~4 wu wide** at yellow/blue/green fills (the blast fills the cone; on-screen kills outside the cone still die — the "off the screen" wipe — but the cone is what draws + knocks back) | LOCKED — survivors = H-weight/Tamer/miniboss/boss |
| Werewolf | **5.0 s** transform, **full i-frames**, every slash = 1HKO, **drops stay**; slash dmg vs boss = 0 above 10% | cooldown = the meter |
| **Werewolf vs. Heavy/untiered** | the 1HKO **DOES kill Heavy, Ground Smasher, Gatling Gunner, Monkey Tamer and every untiered enemy** — it is a raw slash, not a tier-gated special, so no ≤-tier rule applies. **Bosses only** survive (they take slash-dmg 0 above 10% HP, like the other specials). | the one special that ignores weight/tier — its cost is the tiny 5 s window |
| Underdog — Vaporize | close radius **3.0 wu** instant-kill of **T3-and-below + untiered fodder** (Pickpocket, economy Monkey; the Flying Monkey is T2-eff, already covered) — **drops nothing**, sniper-style; **only H-weight (Heavy, Ground Smasher, Gatling Gunner), the Monkey Tamer, minibosses & bosses survive** (only the Werewolf **special** 1HKOs Heavies — keeps it unique); then **+20% to all dmg for 30 s**; **refreshes, does not stack (with itself)** | survivors: **H-weight, Monkey Tamer & every miniboss take 45** (in radius); **bosses take NOTHING above 10%** (negated like all specials, `BOSSES.md` §1) — same survivor rule as the Shotgunner. **The Vaporize buff (+20/25/30%) and the passive meter buff (+10/20/30%, §2.4) are separate sources and STACK multiplicatively** (green×green = ×1.3×1.3 ≈ ×1.69). |
| Boss execution (all specials) | only ≤10% boss HP shows the execute prompt | LOCKED (`BOSSES.md` §1) |

**[LOCKED] Meter-tier scaling — EACH special scales its own signature axis** (yellow = 1 fill · blue = 2 ·
green = 3). The passive **+10 / +20 / +30% damage buff** rides on top for all characters (§2.4); on top of that:

| Special | Yellow (1 fill) | Blue (2 fills) | Green (3 fills / max) |
|---|---|---|---|
| **Tactical — Sniper** | wipe **15** | wipe **30** | wipe **45** (whole screen) |
| **Shotgunner — Giant Shotgun** | blast arc **6 wu** + **8 wu** knockback | arc **8 wu** + **11 wu** knockback | arc **10 wu** + **14 wu** knockback (instakill ≤T3 at every tier) |
| **Werewolf** | **5.0 s** transform | **7.0 s** | **9.0 s** (full i-frames + 1HKO throughout) |
| **Underdog — Vaporize** | radius **3.0 wu**, buff **+20% / 30 s** | radius **4.0 wu**, buff **+25% / 30 s** | radius **5.0 wu**, buff **+30% / 30 s** |

*(The Shotgunner's ≤T3 instakill and the Werewolf's 1HKO are rules, not damage numbers — they hold at every
tier; what scales is reach/knockback and duration. The Underdog's buff still refreshes-not-stacks at any tier.)*

**[LOCKED] Sniper special — full execution spec** (the Tactical's marquee move; supersedes the [PROPOSED] in
`VFX.md` §6):

| Field | Value |
|---|---|
| Time-slow factor | game slows to **0.2×** (enemies + projectiles; the player aims at 1×) |
| Slow duration | **2.5 s** wall-clock (the aim/fire sequence) |
| Targets hit | **15 / 30 / 45** by fill tier (§2.4) — the ricochet auto-chains that many kills |
| Ricochet target order | **nearest un-hit enemy head first**, then the next-nearest to the last hit, greedily, until the tier count is reached or no valid targets remain |
| Fewer targets than cap | if the field has fewer enemies than the cap, it **hits them all and ends** (no wasted bounces; leftover count is lost, not banked) |
| Range | **whole screen** — no per-bounce range cap (it's a screen-clear) |
| Exemptions | **Heavy** (ricochet-immune, `TUNING.md` §4) and **bosses > 10% HP** (dodge) are the ONLY units the ricochet skips. **Every other enemy is a valid target** — including **Zombies**: the sniper special is a **clean kill that destroys the Zombie outright** (it is NOT a normal headshot, so the "headshots only hollow a Zombie" rule, `ENEMIES.md` §2.8, does **not** apply — the special overrides it). No "head lineup" predicate; the auto-chain picks the nearest un-hit enemy. **Drops nothing** from any sniper kill. |
| Zombie tax | **exempt** — sniper kills are always clean (no 10% zombify, unlike hand-guns) |
| **[LOCKED] Player safety during the slow** | the player is **fully invulnerable for the whole 2.5 s** — one of the **two sanctioned i-frame windows** in the game (the other is the Werewolf transform, §3.1 Werewolf row); both are *special payoffs*, not dodges. Enemies and their in-flight projectiles crawl at 0.2× and **cannot damage the player** during the aim/fire; anything mid-flight that would have connected is simply survived. This does **not** contradict the global "no i-frames" rule (which governs *dashes/getups*) — that rule is about movement, these two are cinematic specials. Minibosses/bosses continue their (slowed) patterns but deal no damage in the window. |
| Cooldown | = re-earning the meter (no separate cooldown) |

---

## 4. Enemies — all 17 (HP · damage · speed · weight · timings)

> **Damage** reconciles to **tier × 7.5** where a tier exists. Untiered/TBD units are assigned an **effective
> damage tier** (noted) so nothing is blank. **Weight:** L/M **stagger** to a dash; **H floors the player**.

| # | Enemy | Tier | HP | Contact/Attack dmg | Move (wu/s) | Weight | Per-enemy timings |
|---|---|---|---|---|---|---|---|
| 1 | **Zombie** | T0 | **30** (body dmg only) | **0** (grab only; mash 6 taps to break, 1.0 s window) | 3.0 | M-stagger | headshot-made lasts **10 s** then drops; pod-spawned dies to any finisher; grab cooldown 2 s |
| 2 | **Regular Melee** | T1 | **40** | **7.5** (punch/jump-kick/slide-kick) | 6.5 | L-stagger | melee windup **100 ms**; slide-kick closes from 4 wu |
| 3 | **Swarmer** | T1b | **12** | **1.5** (chip; LOCKED 1–2) | 8.5 | L-stagger | **spawned only from a Pod** (below) — the Pod spits **1 Swarmer every 3 s up to a field cap of 6** pod-spawned; that pod-swarm is the one thing allowed to **exceed the 8-pursuer cap**. *(Single model — the older "pod of 5, 2–4 sides" is superseded by the Pod entity.)* |
| 4 | **Anti-Aircraft** | T1a | **40** | **7.5** (rock) | 5.0 | M-stagger | rock throw every **2.5 s**, arc telegraph 0.5 s; **20% accuracy vs boomerang** (baits it) |
| 5 | **Head-Thrower** | T2-eff | **45** | **15** (head-grenade); **fire→2 s→BOOM = player death** | 5.5 | M-stagger | throw cooldown **3.0 s**; survives the throw, **regrows head in 4 s** (resolves §2.1 [ITERATE]) |
| 6 | **Snapper (Sword-Maker)** | T2 | **70** | **15** (sword) | 6.0 | M-stagger | sword windup **175 ms**; no T1 nearby → calls in a T1 every **4 s**, max 2 pending; sword decays after 8 hits |
| 7 | **Sniper** | T3-eff | **50** | apex shot → **player to 20 HP** (kill if <25) | 4.0 | L-stagger | **scoped scan 3.0 s → rifle-down 2.0 s** cycle; can't hit a grounded player (resolves §2.14 [ITERATE]); 1 at a time |
| 8 | **Flying Monkey** | T2-eff | **35** | **7.5** (swoop melee) | 7.5 (air) | L-stagger | swoops only when **<2 grounded enemies**; swoop cooldown **3.0 s**; sky-tally exempt |
| 9 | **Monkey Tamer** | untiered | **60** | **0** direct (monkeys deal it) | **4.0** (slower than player) | M-stagger | whistle every **5 s**; **up to 2 monkeys** live; respawn **3 s** after one dies; monkeys deactivate instantly on his death |
| 10 | **Monkey (economy)** | untiered | **30** | **5** (flail) | 6.0 | L-stagger | drops the Monkey-Merc summon (needs a dime); flees at <50% HP |
| 11 | **Arm-Ripper** | T2a | **70** | **15** total (2 pistols, **7.5/shot**) | 6.0 | M-stagger | fire **2 shots/s** from ≤4 wu; **reload 2 s after 6 shots**; disarmed T1 becomes headbutt-only (dmg 7.5) |
| 12 | **Ninja** | T3a | **100** | **22.5** melee · **shuriken 12** | 7.0 | L-stagger | teleport cooldown **3 s**, smoke tell 0.3 s; **throws 2 shuriken per volley, cooldown 3 s, effectively unlimited** (self-restocks — the "per stripped limb" is flavor, not finite ammo, §4.1); stars are the telegraphed thrown exception |
| 13 | **Pickpocket** | untiered | **25** | **5** (bump) + steals **all wallet coins** | **9.0** (fastest) | L-stagger | darts in, steals, flees; **kill = 2× coins back** (drops the doubled pile on death) |
| 14 | **Boomergunner** | T2-eff | **80** | **15** across a pass (**5/shot**, up to 3) | 6.0 | M-stagger | throws Boomerang Gun on a fixed orbit; returns in **2.5 s**; can be caught mid-orbit (resolves §2.17 [ITERATE]: catchable) |
| 15 | **Gatling Gunner** | T3 | **110** | **1 HP/hit** stream (LOCKED; ~2 s to live in it) | 4.5 | H-**floors** | contort telegraph **2.0 s** (vulnerable); **1 s burst every 2.5 s**; drops to melee (22.5) inside 3 wu |
| 16 | **Ground Smasher (Zoner)** | T3-eff | **130** | **22.5** (lane shockwave) | 3.0 | H-**floors** | smash every **4.0 s**; overhead windup **1000 ms**; **only 1 shockwave active field-wide**; shockwave travels 12 wu down its Z-row at 10 wu/s |
| 17 | **Heavy ("Bold"/Burly)** | untiered | **220** | **22.5** (extended-reach punch, +0.8 wu reach; emits gust like player) | 5.0 | **H-floors** | punch windup **250 ms**; **max 2 at once**, never flank; **immune to sniper ricochet & headshot-pick** (LOCKED) |

**Pods (shared spawner for Zombie & Swarmer):** HP **50**, destroyable; spits **1 unit every 3 s** up to a
field cap of 6 pod-spawned units; sits at the back Z-edge of the encounter (resolves §2.8/§2.12 [ITERATE]).
- **[LOCKED] Pod typing = fixed per instance, set at placement.** A Pod is **either a Swarmer Pod OR a Zombie
  Pod** — it spits **only its one type** for its whole life, never a mix. The `ENCOUNTERS.md` wave table names
  each placed Pod's type ("Swarmer pod" / "Zombie Pod"); a build reads that label as the Pod's fixed emit-type.
  In **Endless** (§8.3) each spawned Pod is assigned a type at spawn (50/50 Swarmer/Zombie). The **6-unit field
  cap is shared across all Pods** (total pod-spawned units on screen ≤ 6, not per-Pod).

**[LOCKED] Enemy attack cadence (the moment-to-moment pacing — one place).** Each attacking enemy runs
**windup → active hit → recovery → cooldown**, then may attack again. The windups are the §4 table's "timings";
the **re-attack cooldowns** are pinned here:
| Enemy | Attack | Re-attack cooldown (after recovery) |
|---|---|---|
| **Regular Melee** | punch / jump-kick / slide-kick | **1.2 s** |
| **Snapper** | sword swing | **1.5 s** (call-in a T1 every 4 s if unarmed) |
| **Heavy** | extended punch | **1.6 s** |
| **Ground Smasher** | shockwave | **4.0 s** (per §4 row 16) |
| **Swarmer** | **contact-tick** — deals its **1.5 on touch, then a 1.0 s per-Swarmer touch cooldown** (can't chain-tick faster); it has **no windup/swing**, the body IS the hitbox | 1.0 s per Swarmer |
| **economy Monkey (flail) / Monkey-Tamer's monkeys** | melee flail **5** | windup **0.3 s**, cooldown **1.5 s** |
| **Monkey Tamer (cornered flail)** | melee flail **5** | windup **0.3 s**, cooldown **1.5 s** (§4.1 cornered rule) |
- **Ranged enemies** use their §4-row throw/fire cadence directly (AA 2.5 s, Head-Thrower 3.0 s, Sniper the
  3 s-scope/2 s-down cycle, Arm-Ripper 2/s + 2 s reload after 6, Boomergunner 2.5 s orbit, Gatling Gunner 1 s
  burst / 2.5 s). No separate cooldown needed — the row value IS the cadence.
- **[LOCKED] Attacker-slot rotation:** of the ≤8 pursuers, **max 2 hold an attack slot at once**. A slot is
  **held from windup start until the attack's recovery ends**, then **released**; the nearest ring-waiting enemy
  claims the freed slot on the next frame (there is no extra hold delay — the cooldown above keeps the same
  enemy from immediately re-slotting, so slots naturally rotate through the crowd). Ring-waiters sidestep to
  maintain the 2.5 wu standoff and the 1.0 wu separation.

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

**Headshot — the buildable predicate (LOCKED).** There is **no manual aim**: every gun (pistol, revolver,
gatling) fires **straight ahead along the player's current Z-row, on a flat horizontal plane fixed at gun height
= head height** (`WEAPONS.md` §3.1). A shot is a **headshot when, and only when, all of these hold at the moment
of the killing hit:**
1. the target is a **standing** enemy (upright, not airborne/launched and not downed/swept — its head is on the
   fixed shot plane), **and**
2. the target is a **regular non-boss** unit — **all three H-weight enemies (Gatling Gunner row 15, Ground
   Smasher row 16, Heavy row 17), every miniboss, and every boss are headshot-IMMUNE** (a gun kill on them, if
   the damage even gets there, is never a headshot and never zombifies; the Heavy is additionally immune to the
   sniper ricochet, `TUNING.md` §4 row 17). Only L/M-weight regulars can be headshot, **and**
3. the shot's damage **kills** the target on that hit (a headshot that does *not* kill just deals its damage —
   e.g. pistol 12 into a 40-HP Regular is a body-plane hit that chips, no zombify roll).
- **Predicate result:** a qualifying headshot **kill** rolls the **10% zombify** — spawn a 10 s Zombie instead
  of a clean kill (`ENEMIES.md` §2.8). **Airborne (launched/jump-kicking) or downed/swept** enemies are struck
  as **body shots** — they take normal damage, **never headshot, never zombify** (the head is off the plane).
- **Pistol pierce (12/6/3):** the headshot test is applied **per pierced target independently** — the first row
  enemy killed can zombify while a deeper pierced kill rolls its own 10%.
- **Gatling `E`-barrage:** its guaranteed auto-kill on a standing regular **is** a headshot (rolls 10%); its
  **flat 45-chunk** hit on H-weight, minibosses, and bosses is **not** a kill-by-headshot, so **no roll**
  (`WEAPONS.md` §3.6 / `TUNING.md` §6).
- **Sniper special is exempt** — its ricochet headshots **always kill cleanly**, never zombify.

### 4.1 Enemy AI edge-case resolutions — **[LOCKED]** (the "what does it do when…" table)

> Resolves the per-enemy `[ITERATE]` fallbacks so a build never hits an undefined behavior.

| Situation | Resolution |
|---|---|
| **Arm-Ripper spawns with no T1 fodder to disarm** | it **arrives already armed** with its own akimbo pistols (it ripped its arms off-screen); the "rip a nearby T1" is a **flavor animation only when a T1 is adjacent** — never a spawn dependency. |
| **Gatling Gunner spawns with no fodder to contort** | same — it **spawns with the gatling in hand**; the "2×T1 / 1×T2 → gatling" line is the *diegetic origin*, not a runtime requirement. Both are **self-sufficient on spawn**. |
| **Ninja needs no fodder** | the Ninja is **fully self-contained** — teleport + shuriken are its own kit; **throws 2 shuriken per volley, cooldown 3 s, effectively unlimited** (self-restocks — the "2 per stripped limb" was flavor, not a finite ammo count). Spawns combat-ready. |
| **Regular Melee attack selection** | by range: **≤1.0 wu → punch** (7.5); **1–4 wu → slide-kick** (gap-closer, 7.5); **player airborne within 3 wu → jump-kick** (7.5). Picks the fitting one for the current range each attack cycle (windup 100 ms). |
| **Monkey Tamer's melee monkeys — stats** | each summoned monkey: **HP 20, contact dmg 5, speed 6.0 wu/s, L-stagger**; **max 2 live**; **deactivate instantly on the Tamer's death** (§4 row 9). They are lighter than the economy Monkey (row 10). |
| **Pickpocket escapes with your coins** | if it **reaches a screen edge**, the stolen coins are **lost permanently** (the risk). Killing it before it exits **drops 2× the stolen pile**. It only steals **once per life**, then flees. |
| **Boomergunner's gun is caught mid-orbit** | catching it (walk into the returning arc) **destroys the gun for that enemy** (it must re-loot/melee) and **staggers the Boomergunner 0.55 s**; the player does **not** gain the gun (it's the enemy's body-part, shatters on catch). **This applies to the Boomergunner *boss* too:** each of its orbiting guns is **individually catchable** — catching one **destroys that loop** (the boss loses that orbit) and staggers the boss 0.55 s. So in its 2-loop phase (≤66%) the player can pick off one loop at a time; the boss re-throws a fresh loop on its normal cadence. Bosses are **status-immune** (§2.6) so the catch never *stuns* the boss beyond the 0.55 s stagger, and the guns still **cannot be kept** by the player. |
| **Head-Thrower's thrown head** | uses **grenade fastball physics** (`WEAPONS.md` §3.2) — flat line-drive, **explodes on contact or after 8 wu**; the thrower **regrows its head in 4 s** (§4 row 5) and cannot throw again until it does. |
| **Sniper with the player already downed/grounded** | **holds fire** (can't hit a grounded player, §4 row 7) and **re-scans**; it only fires at an airborne/jumping player (apex punish). |
| **Sniper reposition / escape when the player closes in** | when the player gets **within 6 wu**, the Sniper **lowers the rifle and back-steps** (at his 4.0 wu/s speed) toward the **farthest perch/spawn-edge on his side**, trying to re-open range; if **cornered (no room to retreat)** he **fights with a weak melee pistol-whip (7.5, windup 0.2 s, cooldown 1.5 s)** — never un-attackable. He re-scopes only once he's ≥8 wu away again. He **perches at the back Z-edge** (the `B(perch)` marker in `ENCOUNTERS.md`). |
| **big-version catch-up miniboss would be a degenerate type** | the §8.2 random-seen-type pick **excludes Zombie, Swarmer, Pickpocket, and economy Monkey** (they'd make a broken miniboss — 0-damage, fodder, or steal-and-flee). If the roll lands on one, **re-roll**; if the whole seen-pool is only those, fall back to **big Regular Melee** (the Areas-1–2 fallback, extended to any all-degenerate pool). |
| **Flying Monkey swoop path (the dive itself)** | from its sky-band hover (~4 wu up), on swoop it **dives in a fast arc to the player's current ground position at 9.0 wu/s**, its **body-hitbox live for the descent** (contact **7.5**), then **climbs back to the hover band** over ~0.5 s. Miss or hit, it returns to circling; **3.0 s swoop cooldown** before the next dive. Only hittable by **air attacks or anti-air** while diving/hovering (a grounded melee can't reach the hover band). |
| **Flying Monkey when ≥2 grounded enemies exist** | **circles/harasses without swooping** until the grounded count drops below 2 (§4 row 8); never idles off-screen. **Max 2 Flying Monkeys airborne at once** (they're the sky-category cap, separate from the 8 grounded pursuers). |
| **big / miniboss Flying Monkey (catch-up injection or placed miniboss)** | **[LOCKED] ignores the `<2 grounded` swoop gate — it swoops on cooldown regardless of the grounded count** (a miniboss is always a live threat, `ENCOUNTERS.md` §Dixon miniboss 3). It still respects the **3.0 s swoop cooldown** and the **max-2-airborne** sky cap; the gate override is the only difference from the fodder version. |
| **Catch-up miniboss during a boss fight or vignette** | **suppressed** — the §8.2 catch-up trigger **never fires inside a boss arena or during a scripted vignette**; it only injects in normal stage waves. |
| **Monkey Tamer cornered (no room to keep-away)** | he **stops fleeing and fights with a weak melee flail (5 dmg)** while still whistling — he never becomes un-attackable; cornering him is the intended kill window (his monkeys deactivate on his death, §4 row 9). |
| **Enemy would exceed the 8-pursuer cap** | it **holds at a spawn edge** (visible, not attacking) until a slot frees — except Swarmer pods (`ENCOUNTERS.md` §0 exception). |

**[LOCKED] Drop-roll & interaction resolutions (the last small "what about…" set):**
- **Drop rolls are INDEPENDENT per kill.** On a qualifying kill the game rolls **each drop channel separately**:
  **weapon** (§6 band %), **coin** (12%), and **heal** (5%, →20% at low HP). A single kill can therefore drop a
  weapon *and* a coin *and* a heal (rare), or nothing. They are not mutually exclusive and do not share a roll.
- **Swarmers drop NOTHING** — no coin (already LOCKED, `WEAPONS.md` §3.9), and **also no weapon and no heal**
  (pure fodder never feeds any economy). The heal-drop channel is **suppressed on Swarmer kills** exactly like
  the coin channel.
- **Sniper-special kills drop nothing** on any channel (LOCKED) — weapon, coin, and heal all suppressed.
- **Pods ARE valid sniper-ricochet targets** — a Pod is a destroyable body (50 HP); the ricochet may chain
  through it and destroy it like any enemy (it counts toward the tier kill-count). Pods are **not** headshot/
  zombify targets (they're structures, not heads).
- **Werewolf slashes hit downed enemies** — the 5 s 1HKO is a raw slash, so a **downed/swept enemy is killed by
  it too** (the "no normal hits while downed" rule, §2.6, blocks only *combo normals*, not the Werewolf
  special). Same for any AoE/special (§2.6 downed-immunity block list).
- **The meter DOES fill during a special's own hits** for the Shotgunner/Werewolf/Underdog (each connecting
  slash/blast feeds the meter at the weapon-hit rate, §2.4) — this is how you start re-earning the next special
  mid-special. The **Sniper special is the exception: its kills give no meter** (it would otherwise loop
  infinitely), consistent with "sniper kills drop nothing."
- **Specials vs. Pods (LOCKED):** the **Shotgunner ≤T3 instakill, Werewolf 1HKO, and Underdog Vaporize all
  destroy a Pod** if it's in their area/reach (a Pod is a 50-HP structure below the T3 line — it dies to the
  instakill/1HKO like any ≤T3 target; Vaporize destroys it in radius). This clears the spawner, not just its
  spawns. (The sniper ricochet also hits Pods, above.)
- **[LOCKED] Kill attribution — what a kill credits the player.** A kill counts toward the player's **meter,
  drop rolls, wave-clear gates, AND the §8.2 kill-interval metric** when the killing blow is the **player's own
  action OR a player-owned proxy**: fists/weapons/thrown items, **reflected projectiles** (Bat), **grenade/
  rocket/Ball&Chain/staff/gatling**, **Monkey-Merc shots**, and **specials** (except the sniper's meter carve-
  out above). A kill **does NOT credit the player** (no meter, no drop, but it DOES still satisfy a wave-clear
  gate — the field must empty regardless) when it's an **environmental hazard** (car/trolley/fire-boom/fall) or
  **enemy-on-enemy** (a fire-boom catching another enemy, a Snapper's called add killed by another hazard). So:
  hazard/enemy-cross kills clear waves but pay no loot or meter; everything the player or their proxy kills pays
  out normally.
- **[LOCKED] Ground-pickup persistence.** Dropped **weapons, coins, and heals persist on the ground for 12 s**
  then despawn (a soft fade begins at 9 s); **the Monkey-Merc claim token uses its own ~5 s** (`WEAPONS.md`
  §3.7). Crossing a **checkpoint or clearing the stage despawns all un-grabbed pickups** (they don't carry to
  the next segment). A **death/respawn clears the field's pickups** too (fresh from the checkpoint). Pickups
  never persist across a quit (nothing but the stage bookmark is saved, `UI.md` §5).

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

> **Ranged weapons bludgeon at fist-strength (10) through the combo, finisher = free melee 35** (the fist
> baseline, §2.1). **Genuine melee weapons swing at their OWN per-hit / finisher values** (below — Sword
> 18/45, Whip 14, Club 14, Bat 12); they are real swing kits, not fist re-skins.
> So "the finisher is 35" applies to **fists and gun-bludgeons only** — melee weapons use their row's numbers.
> **[LOCKED] Melee-weapon finisher damage = per-hit × 2.5** (Sword 18→**45**, Club 14→**35**, Bat 12→**30**,
> **Ball & Chain 20→50**);
> the **Whip finisher is the head-rip extraction** (`COMBOS.md` §4) — it **kills a downed enemy at any HP** (no
> damage number, an execution). **Ball & Chain — normal-string melee:** when NOT `E`-launching, the ball swings
> as **heavy melee at 20/hit** (slow cadence, `TUNING.md` §6 warm-up 0.40 s) through the P1→P2→sweep string;
> the **combo finisher is a free ground-slam at 50** (= 20 × 2.5, spends **no** use — only the `E`-launch spends
> a use). Ranged weapons'
> finisher = the free **35** melee.
> **`E` spends ammo/durability** (fire/throw/cast). Throwables: **tap `E` during wind-up** — more taps = flatter.

| Weapon | Tier | E-fire dmg | Durability / Ammo | Warm-up | E-fire behavior |
|---|---|---|---|---|---|
| **Fists** | — | — | ∞ | 0 s | always-ready; the fallback |
| **Sword** | T2 | melee **18/swing**, finisher **45** | **8 connecting hits** (of 5–10) then shatters | 0.20 s | no E-fire; pure melee; blade chips at 6/4/2 left |
| **Shotgun** | T3 | blast **40 + 6 wu knockback** | **5 spine segments** (of 4–6) = 5 shots | 0.25 s | `E` fires + cocks + ejects a spine segment |
| **Boomerang** | T1 | throw hit **8** + **2 s stun** | **lost on first enemy hit** (retrievable on ground) | 0.15 s | `E` throws; misses return to hand |
| **Pistol** | T1 | shot **12**, pierces 3 (**12/6/3** halving) | **mag 8**, then discarded | 0.25 s | **`E` fires any target, any HP**; the **double-tap execute on a swept target** (`COMBOS.md` §2) is the only <20%-gated path |
| **Revolver** | T1 | shot **30**, no pierce | **mag 6**, then discarded | 0.30 s | same: `E` fires freely; only the double-tap execute on a swept target is <20%-gated |
| **Grenade** | T4 | lob blast **60** (r 3 wu) · fastball blast **35** (r 2 wu) | **1 use** | tap-`E` wind-up | few taps = high lob (bounces 3×→boom); many taps = fastball (boom at 8 wu or after 8 enemies); **self-dmg 40** |
| **Ball & Chain** | T2 | **`E`-launch 80/swing** · **normal string 20/hit, finisher 50** | **3 uses** (launch only) | 0.40 s | tap-`E` trajectory; **carrying slows player 20%** (move only, not attack); `E`-launch shapes = `COMBOS.md` §3; the normal combo string & finisher spend **no** use |
| **Whip** | T2 | **14/hit**; finisher = head-rip→grenade | **11 connecting hits** (of 10–12) | 0.25 s | **no E-fire** (pure melee); **arrow-melee directions**: up=arc / fwd=pull (drags enemy 3 wu) / down=line. **Finisher = the head-rip extraction** (a free-melee finisher variant, `COMBOS.md` §4; auto-dashes you back 4 wu) |
| **Staff** | T2 | Ice: **8** +freeze 3 s · Fire: **6/s ×3 s** (18) · Lightning: **12** +stun 1 s +slow **−40% move for 2 s** (`WEAPONS.md` §3.5) | **6 casts** then breaks | 0.35 s | element fixed at pickup; `E` casts; Fire on a Head-Thrower → walking bomb (2 s→boom) |
| **Gatling Gun** | T3 | **`E`-barrage 0.5 s** on the **nearest enemy directly ahead within 8 wu on your row** — **auto-kill vs. regular fodder, flat 45/barrage vs. everything armored** | **no ammo**; overheats after **5 barrages OR 20 s cumulative equipped time** (whichever first) then discards | 0.40 s spin-up | melee bludgeon **10** (fist-strength per the §6 header rule; **slow cadence** — the heavy weapon swings slower, but the per-hit value is the standard bludgeon 10, not a special number); **no i-frames during barrage**. **Barrage damage rule (ONE rule, LOCKED — matches `WEAPONS.md` §3.6):** (a) **standing regular non-boss** (Regular, Snapper, Head-Thrower, Ninja, Pickpocket, Monkey, etc.) → **auto-kill** (headshot, rolls 10% zombify); (b) **all H-weight (Heavy, Ground Smasher, Gatling Gunner)** and **any miniboss** → **flat 45/barrage, no auto-kill, no zombify**; (c) **the 5 HP-depletion bosses** (Burly, big Arm-Ripper, Boomergunner, Gatling Gun Guy, Sandwich Bros) → **flat 45/barrage** (a capped chunk — 5 barrages = 225, so the gatling can't solo any boss; never an auto-kill, never zombify); (d) **the 5 objective/proxy bosses** (Colossus, Tank, Helicopter, Monkey Boss, Phil) → **0** — their HP isn't a normal bar (pieces/pips/proxy/script, `BOSSES.md` §1), so the barrage does nothing to them, exactly like it can't shortcut their objective. Airborne/downed enemies are struck as body shots (no headshot) |
| **Monkey Merc** | T4 | **pistol 8/shot** · **shotgun ~18/blast** · **rocket ~40/rocket** — all **@ 2 shots/s** | **costs 1 dime**; **3 summons/level** then none | 0.5 s summon | 1=pistol/20 s · 2=shotguns/10 s · 3=rockets/5 s; adding a monkey **re-arms all to the new tier & resets timers**; **no friendly fire** (`WEAPONS.md` §3.7) |
| **Club** | T1 | melee **14** + **6 wu knockback** | **10 hits** | 0.15 s | no E-fire; short reach, big knockback |
| **Bat** | T2 | melee **12**; reflect | **12 hits**; **reflect window 0.20 s** | 0.15 s | swing-timed reflect of thrown heads/shots back at attacker (resolves §3.7b [ITERATE]) |
| **Boomerang Gun** | T2 | **8/shot** | **10 bullets, 4/pass** (~3 passes) | 0.20 s | `E` throws on a fixed orbit auto-firing; **fists only while out**; throw cooldown 1 s; shot-down = lose remaining bullets |
| **Rocket Launcher** | T4 | blast **70** (r 3 wu) | **3 rockets** (world pickup) | 0.50 s | `E` fires; **self-dmg 35** like grenade (resolves §3.8b [ITERATE]) |

**Tier drop-rate table (per non-swarm kill; coin roll only in Area 3+, §6.1):**

> **Drop *chance* by enemy band is here; *which* weapon rolls is filtered by the area-gate in §6.1.** The
> "weighted toward" column says which tier the roll favors — but a weapon only appears once its area has
> unlocked it (§6.1), so early kills yield the basic-melee pool regardless of band.
>
> **[LOCKED] Guaranteed "the enemy IS its weapon" drops (100%, dual-channel).** Three enemies are their own
> weapon, so **killing them ALWAYS drops that weapon** (a guaranteed drop *in addition to* their normal band
> roll below): **Snapper → Sword** (§6/`ENEMIES.md` §6), **Arm-Ripper → Pistol** (one arm's mag), **Gatling
> Gunner → Gatling** (`ENEMIES.md` §2.7). These override the % roll for that specific weapon — the band %
> still rolls for a *bonus* second weapon on top.

> **Note on Sword's tier:** the Sword is a **T2-strength** weapon (`WEAPONS.md` §2.1, §6 table) but is
> **unlocked in Area 1** as the intentional **starter upgrade** — availability (§6.1 area gate) is independent
> of combat tier. So a T0–T1 kill in Area 1 can still roll a Sword even though it out-tiers the band.

| Enemy level band | Weapon-drop chance | Roll weighted toward (within the area-unlocked pool, §6.1) |
|---|---|---|
| T0–T1 | 18% | the area-unlocked **corpse-drop** basics — **Area 1: Sword (T2-strength starter) + Boomerang**; Pistol/Revolver join the corpse pool in Area 2 (Sacramento). *(The **Club is a placed pickup, not a corpse drop** — it never rolls on this table, §6.1.)* |
| T2 | 22% | tier-2 weapons (Whip, Bat, Staff, Ball & Chain, Boomerang Gun — as unlocked) |
| T3 | 26% | tier-3 weapons (Shotgun, Gatling — as unlocked) |
| T4 (regular tier-4 enemy) | 35% | tier-4 weapons (Grenade + the strongest unlocked pool) |
| **Miniboss** (big-version elite) | **100% — GUARANTEED drop** | a **T2+ weapon from the current area pool** (`BOSSES.md` §4) — minibosses always drop, unlike the 35% T4 *enemy* roll; the guaranteed drop is part of what makes a miniboss worth fighting |
| **Untiered combat elites** (Heavy, Monkey Tamer) | **26%** (roll on the **T3 band**) | treated as T3-strength for loot — they're tough kills, so they reward from the strongest-unlocked pool like a T3. The **Gatling Gunner is T3** already, so killing it drops a Gatling per §6.1; the **Heavy drops a T3-band weapon** (resolves `ENEMIES.md` §2.11's open drop item). |
| **Economy enemies** (Monkey, Pickpocket) | **do NOT roll this table** | the **Monkey** drops the **Monkey Merc** stick (its only source, `ENEMIES.md` §2.2); the **Pickpocket** drops **coins** (2× what it stole, `ENEMIES.md` §2.16) — neither drops a weapon. |

**[LOCKED] Within-band weapon selection (quantified — resolves "weighted toward").** Once a band's %-roll
succeeds, pick the actual weapon this way: build the **candidate pool** = every weapon of the band's own tier
that the current area has unlocked (§6.1), plus every weapon **one tier below** it that's unlocked. Then roll
**70% → the band's own tier, 30% → one tier below**; **within the chosen tier, pick uniformly** among its
unlocked candidates. If the band's own tier has **no** unlocked weapon yet (early areas), the whole roll falls
to the next-lower unlocked tier (uniform). Example: a **T2 kill in Area 2** rolls 70% among {Whip, Staff}
(Bat/Ball&Chain/Boomerang-Gun not yet unlocked) and 30% among the unlocked T1 pool {Boomerang, Pistol,
Revolver, Sword}. This is the single selection rule for every band row above.

*The **Rocket Launcher is a world pickup only** (not in any random pool, `WEAPONS.md` §3.8b), and the
**Monkey Merc drops only from the Monkey stick figure** (`ENEMIES.md` §2.2) — neither is a tier drop. At
low HP (≤25) all weapon-drop chances **double**.
- **[LOCKED] Pod destruction drops NOTHING** — a Pod is a spawner structure, not an enemy kill: destroying it
  rolls no weapon/coin/heal channel and gives no meter (it does count as a sniper-ricochet target for the tier
  wipe-count, §3.1). Only the units it *emitted* roll drops on their own deaths (and Swarmers drop nothing).

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
>
> **[LOCKED — GLOBAL, cumulative availability] Unlocks only ADD; they NEVER expire.** Once a weapon becomes
> available in an area/stage, it **stays available in EVERY subsequent area and stage for the rest of the run**
> — the pool only grows. This holds for **both** sourcing methods: a **corpse-drop** weapon keeps rolling off
> valid enemies in all later areas (a Sword can still drop in Area 4), and a **placed-pickup** weapon (Club,
> Rocket Launcher) keeps appearing as a placed pickup in all later stages from its intro stage onward (the Club,
> introduced at the airport, keeps spawning as a stage pickup through Areas 3–4 and the finale run-up). The
> "cumulative" column below is literal: each row is **everything unlocked so far**, not just that area's new
> additions. No weapon is ever "for that level only."

> **Reading the table:** an entry means the weapon **becomes AVAILABLE from that area on**, by its own sourcing
> method — most enter the **corpse-drop roll** (§6 tier table), but the two **placed-pickup** weapons (**Club**,
> world/airport pickups; **Rocket Launcher**, world pickup) are listed here only to mark *when they start
> appearing as placed pickups* — they **never enter the corpse-roll pool** (§6 note). So "Airport + Club" =
> "Club pickups start spawning at the airport," not "Club can now roll off a corpse."

| Area | Weapons AVAILABLE from this area on (cumulative) | Notes |
|---|---|---|
| **Area 1** (suburbs/mall) | **Sword, Boomerang** only | basic melee + the throw toy; **no guns yet** (matches "only basic melee early") |
| **Area 2 — Sacramento (Stage 4)** | + **Pistol, Revolver, Whip, Staff** | guns + whip arrive with the Snapper/tier-2 layer |
| **Area 2 — Airport (Stage 5)** | + **Bat, Club** | both **gated to the airport specifically** (after their Stage-5 vignette teaches them, `STAGES.md` §1c): **Bat** = corpse-drop from Stage 5 start, and also guaranteed in the Helicopter arena (`WEAPONS.md` §3.7b); **Club** = world/airport pickup (`WEAPONS.md` §3.7c). Neither drops in Sacramento (Stage 4) — preserving teach→tools→test |
| **Area 3** (hills/Dixon) | + **Ball & Chain, Grenade, Shotgun** | heavier kit as tier-2/3 enemies appear |
| **Area 4** (Vallejo→SF) | + **Boomerang Gun, Gatling** | full roster live |
| **World pickups (any area, placed)** | **Rocket Launcher** — placed near Tank (Stage 9) & SF gauntlet (Stage 12). **Club** — placed pickups **one per stage from Stage 5 on** (at each stage's mid-checkpoint), the reliable heavy-melee option (`WEAPONS.md` §3.7c) | never in a random pool |
| **Currency-only** | **Monkey Merc** — from a Monkey + a dime, Area 3 on | not a tier drop |

- **The tier roll (§6) is filtered by this table:** e.g. a Snapper (T2) killed in Area 2 can drop Whip/Bat/
  Staff but **not** Ball & Chain (Area-3-gated) yet. In Area 3+ the full T2 pool is available.
- **Boss-arena weapon-supply guarantees (the arena force-drops the objective weapon, `BOSSES.md` §1):** the
  **Helicopter** arena (Area 2) guarantees **grenades** and **bats**; the **Tank** arena (Area 4) guarantees
  **grenades**; the **Colossus** arena (Area 2) guarantees **whips** — each supplies exactly the weapon its
  objective needs, regardless of the normal drop roll.
  - Of these, **only the Helicopter arena's grenades appear *ahead of* their area gate** — grenades are
    Area-3-gated (§6.1 table) but the Helicopter is in Area 2, so the arena overrides the gate for grenades
    only. The Helicopter's **bats** (Area-2-gated, same stage), the Tank's **grenades** (Area 4 = already
    past the Area-3 grenade gate), and the Colossus's **whips** (Area-2-gated, same Sacramento stage) are all
    **already within their area gate** — the arena merely *guarantees* the drop, it doesn't jump the gate.
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
| **Taxiing plane** (airport stage) | **50** + knockdown | **50** | crosses the tarmac on a fixed path (like the suburb cars); engine-whine + 0.8 s telegraph (resolves `AREAS.md` §2.2 [ITERATE]) |
| **Mall escalator / kiosk, city crowd** (Areas 1/4) | **0** (funnel/blocker only) | **0** | terrain funnels, not damaging (`STAGES.md` §7c) — they pinch the lane, no hazard damage |
| **Ground Smasher shockwave** (enemy attack) | **22.5** + **1.2 s knockdown** | — | the lane shockwave floors you (standard knockdown, §2.6); it is an enemy attack, listed here for the knockdown value |
| **Roller-coaster car** (Vallejo) | **50** + knockdown | **50** | on-rail, fixed timing telegraph |
| **Causeway water** (Stage 6) | **10 chip** + respawn on last platform | enemies that fall are **removed (count as killed)** | no drowning death for the player |
| **Pond/puddle** (farm) | **0** (slows movement 30% while in it) | same slow | soft terrain, not damage |
| **Grenade self-blast / rocket self-blast** | **40 / 35** | full blast | your own ordnance (§6) |
| **Head-Thrower fire-boom** (staff-lit) | **instant death within r 2 wu** | kills the lit enemy | the walking-bomb interaction (`WEAPONS.md` §3.5); "adjacent" = the r 2 wu blast |
| **Golden Gate wind gust** (GGG arena) | **0 dmg**, pushes player **1.5 wu** toward a Z-edge every ~4 s | pushes enemies too | positional pressure on the bridge (arena table, `ENCOUNTERS.md`) |
| **Fall off Salesforce rooftop** | **instant death** | enemies knocked off = killed | Phil arena only |

**[LOCKED] Hazard pass frequency** (how often the on-rail hazards cross): **cars/buses** (Stages 1–2) every
**6–9 s**, random side, one at a time; **taxiing planes** (Stage 5) every **8–12 s**; **SF trolley** (Stage 12)
every **10 s** on a fixed track; **roller-coaster** (Stage 9) every **7 s** on its rail. All telegraphed per
§6.2. (Boss-arena hazards fire on their boss's own cadence, `BOSSES.md` §5.)

### 6.3 Projectile kinematics — **[LOCKED]** (speeds & ranges — the systemic gap closed)

> Every projectile's **travel speed** (wu/s) and **max range** (wu before it despawns/falls). Damage is in §6
> (weapons) / §4 (enemies); reach/hitbox in §2.1. Enemy shots obey the **short-range rule** (`ENEMIES.md` §1)
> — they connect only within their listed max range, so you can always dash out.

| Projectile | Speed | Max range | Notes |
|---|---|---|---|
| **Pistol / Revolver round** | 40 wu/s | **12 wu** then despawns | player guns; pierces per §6 |
| **Shotgun blast** | instant (hitscan cone) | **6 wu cone**, ~4 wu wide | short-range spread |
| **Staff cast** (ice/fire/lightning bolt) | 22 wu/s | **10 wu** | straight, facing dir |
| **Boomerang (thrown)** | 18 wu/s out | **8 wu** then returns | returns on miss |
| **Boomerang Gun** | orbits at 14 wu/s | 5×3 wu loop (§WEAPONS §3.8) | auto-fires inward |
| **Grenade fastball** | 20 wu/s | 8 wu / 8 enemies | lob = arced, lands 6/8/10 wu (`WEAPONS.md` §3.2) |
| **Rocket** | 16 wu/s | 14 wu | blast r 3 wu |
| **AA rock** (enemy) | 12 wu/s arc | **10 wu** | arced lobber; boomerang-baitable |
| **Arm-Ripper pistol** (enemy) | 30 wu/s | **≤4 wu** (must close, §4) | close gunner |
| **Ninja shuriken** (enemy) | 26 wu/s | **12 wu** | the telegraphed thrown exception |
| **Head-grenade** (enemy) | fastball physics | 8 wu | `WEAPONS.md` §3.2 |
| **Helicopter head** (boss) | 14 wu/s (falls toward player) | drops from top band | max 2 airborne; bat/lob to counter |
| **Boomergunner gun-shot** (enemy) | 16 wu/s along the orbit | oval 5 wu wide × 3 wu deep | 5/shot |
| **Sniper (both player special & enemy)** | **hitscan** (instant) | full screen | no travel |
| **Gatling stream** | hitscan, 8 wu | 8 wu | 1/hit chip |

---

## 7. Bosses — all 7 bespoke + big-version rule

> Fight-length target **< 2:00** for every boss **except Phil**. HP tuned so a competent player hits the target;
> difficulty is pressure/reads, not HP bloat (`BOSSES.md` §1).

| Boss | Area | HP | Phase thresholds | Attack dmg | Win condition / objective count | Length target |
|---|---|---|---|---|---|---|
| **Sandwich Bros / big Tier-1** | 1 (suburbs) | **160** (2× kit, big-version) | **50% → adds a jump-kick + slide-kick (uses the full Regular kit, faster)** | punch **11** | HP depletion; **solo = 1 big T1** | 1:15 |
| **Burly Macho Guy** | 1 (dept store) | **300** | phase 2 at **200 HP** · phase 3 at **100 HP** (exact HP triggers, ≈66% / ≈33%) | ground-spike **22.5** · **enemy-toss 40** | HP depletion | 1:45 |
| **Colossus** | 2 (Sacramento) | **240** = **6 pieces ×40** | shed at 4 & 2 pieces (speeds up) | body swipe **22.5** | **whip off 6 stick-figure pieces**; torn pieces become T1 adds | 1:50 |
| **Helicopter** | 2 (airport) | **objective — 6 damage-pips** (not HP-depleted) | after **3 pips** it descends lower & fires faster | thrown heads **15** (max 2 on screen) | fill a **6-pip** bar: a **reflected head = 1 pip**, a **lobbed grenade = 1.5 pips** — so **6 heads**, or **4 grenades** (4 × 1.5 = 6), or any mix summing to 6, downs it; main-boss-only | 1:40 |
| **Monkey Boss** | 3 (farm) | **200** (only your mercs damage him) | 60% · 30% (throws dimes faster) | **0** direct; his mercs (T1 pistol 7.5) | proxy war: catch dimes → your mercs shoot him down; boss mercs ignore the 3-summons cap | 1:55 |
| **big Arm-Ripper** | 3 (Dixon) | **280** (boss-scale) | **66% → fires 3 shots/s (from 2); 33% → adds a rolling reposition between volleys** | pistols **7.5/shot @ 2/s** (base) | HP depletion; caps the Dixon boss rush. **Keeps the enemy Arm-Ripper's reload cadence — 2 s reload after every 6 shots** (`TUNING.md` §4 row 11), at all phases; the faster fire rate just reaches the 6-shot reload sooner (the reload is the punish window) | 1:50 |
| **Tank** | 4 (Vallejo) | objective (**2 grenade drops**) | **after drop 1** (MG pattern intensifies) | MG stream **1/hit**; direct hit while mounting **22.5** | **climb + drop grenade in hatch ×2**; arena adds drop only grenades | 1:50 |
| **Boomergunner boss** | 4 (Marin) | **320** (boss-scale, 80×4) | **66% → throws a 2nd orbiting gun (2 loops at once); 33% → both loops tighten toward the player** | boomerang-gun shots **5/shot** (base — see ranged note) | HP depletion | 1:45 |
| **Gatling Gun Guy** | 4 (Golden Gate) | **260** | 66% · 33% | **barrage = instant death if caught in the open** (LOCKED); melee 22.5 | HP depletion; **hide behind cars** on the **~5 s "BARRAGE INCOMING"** cycle; Shield-Rush the fodder version | 1:55 |
| **Phil (FINAL)** | Finale | **500**, gated behind sharpen windows | 100%→75%→50%→25%→**execute** = **4 damage windows of 125 HP (25%) each** | contact **15** · summons deal their own dmg · **fall off tower = instant death** | invuln while drawing; **sharpen window 3–5 s** (ends early if the window's cap is hit) is the only opening; **per-window damage cap = 125 (25%)** → exactly one threshold per window; killed **only** by the scripted **pencil-laser finisher**, input in the window that takes his gated HP to ≤0 (the 4th on a clean run — no separate extra window) | **exempt** (~5–8 min) |

**Big-version scaling rule (concrete):**

| Class | Size | HP multiplier | Damage multiplier |
|---|---|---|---|
| **Miniboss** | **1.2×** | **×2.0** | **×1.25** |
| **Boss** | **2.0×** | **×4.0** | **×1.5** |

(e.g. Regular Melee 40 HP → big-version boss 160 HP, 7.5→11 dmg — matches Sandwich Bros above.)

- **[LOCKED — GLOBAL rounding rule] Every computed value rounds to the nearest integer (0.5 rounds up); HP
  rounds to the nearest whole HP.** This applies to all multiplier products anywhere in the bible — big-version
  scaling (7.5 × 1.5 = 11.25 → **11**; 7.5 × 1.25 = 9.375 → **9**), character stat multipliers (§3), difficulty
  multipliers (§8.4), and the meter damage buff (§2.4). The placed §7 table values are already rounded; the
  **auto-generated** catch-up minibosses (§8.2) and Endless elites (§8.3) apply this same rule at spawn. Enemy
  HP is always an integer; damage is always an integer.

- **Ranged big-version bosses keep BASE per-shot damage — [LOCKED override].** The ×1.5 damage multiplier
  applies to **melee/contact** attacks only. For **ranged** big-version bosses (**big Arm-Ripper** 7.5/shot,
  **Boomergunner boss** 5/shot) the **per-shot number stays at base**; they scale their threat through **higher
  HP (×4), faster fire, and more projectiles**, not bigger bullets — so a ranged boss can never chip you to
  death in two hits. This is why the §7 table lists base per-shot values for those two. **Melee** big-version
  bosses (Sandwich Bros 7.5→11) do take the ×1.5.
- **The §7 table is authoritative for the 10 placed boss encounters (7 bespoke + 3 big-version);** the formula above is for **auto-generated**
  big-versions (catch-up minibosses, Endless elites) and for cross-checking the placed ones.

**Boss & meter rules (LOCKED, restated):** unspent meter **carries over**; the sniper visibly **dodges** every
boss above the execute threshold. **The ≤10% execute applies ONLY to the 5 pure HP-depletion bosses** (Burly,
big Arm-Ripper, Boomergunner, Gatling Gun Guy, Sandwich Bros — `BOSSES.md` §1 is authoritative). The other 5
(**Colossus** whip-objective, **Tank**/**Helicopter** objective, **Monkey Boss** proxy, **Phil** scripted) have
**no execute** — specials whiff on them. **No** boss (all 10) is sniper-one-shottable above 10%. Only **catch-up
minibosses / big-version *non-boss* elites** are sniper-killable like normal enemies.

---

## 8. Meta — checkpoints, continues, Endless, catch-up

### 8.1 Checkpoints & continues

| Field | Value | Notes |
|---|---|---|
| **Checkpoint cadence** | **1 at stage start + 1 mid-stage + 1 at the boss door** | ~2–3 per ~15–18 min stage (`ENCOUNTERS.md` §0); bossless stages = start + mid |
| Heal on checkpoint | **only a death-respawn restores full HP** (`TUNING.md` §2.2) — **reaching** a checkpoint does **NOT** heal | consistent with "no full-heal pickups"; you respawn full only after dying |
| **Weapon/loadout after respawn** | **fists only** — a death (or pause-restart) **drops any held weapon**; you respawn empty-handed at the checkpoint | keeps death a real setback; re-loot from the re-run |
| **Meter after respawn** | **emptied** (death spends a continue, which clears the meter — below) | banked special is LOST on death; part of the continue cost |
| Money on checkpoint | resets each **stage** (LOCKED, `UI.md` §3.4) | not stored across checkpoints |
| **Continues per run** | **3** | when all 3 are spent → **the Game-Over screen** (`UI.md` §5), which offers **Restart the current stage from its start (fresh continue count)** or **Quit to title** **(tunable)** |
| Continue cost (every death) | respawn at last checkpoint at **full HP**, but **wallet cleared + special meter emptied + weapon dropped (fists)** | one consistent respawn cost — full HP is the *only* thing you get back |
| Lives before a continue | 1 (death → spend a continue) | no separate life stock; **every death = one continue spent** |

### 8.2 Catch-up miniboss trigger (concrete "too fast" metric)

| Field | Value |
|---|---|
| **Metric** | rolling **average kill interval** over the last 10 kills |
| **Trigger** | average kill interval **< 3.0 s** (i.e. clearing faster than 1 kill / 3 s) **for 20 s straight** |
| Injection | spawn **1 recurring miniboss** (a **big-version enemy**, generated by the §7 boss-scaling formula) at the front Z-edge |
| **Which enemy** (LOCKED selection) | the big-version is the **big-version of a random enemy type the player has *already encountered* this run** (any non-boss type that has appeared in a wave up to this point), chosen uniformly at spawn. **Areas 1–2 fallback:** if the pool of seen types is empty or all-fodder (Zombie/Swarmer only), spawn a **big Regular Melee** — the guaranteed early-game default |
| Stats | HP & damage from the §7 auto-generated big-version formula (2× kit); **immune to sweep/knockdown & the ≤10% execute** (it's a miniboss, §2.6), but **sniper-killable** like any non-boss elite (§8.2 note) |
| Re-arm cooldown | **90 s** before the trigger can fire again |
| Cap | max **1 catch-up miniboss active** at a time |

### 8.3 Endless Mode scaling curve

> Base start (difficulty 0), never lets up; **spawns a fresh pod whenever only 2 enemies remain** (LOCKED).

| Field | Value | Notes |
|---|---|---|
| Refill trigger | on-screen enemies **≤ 2** | LOCKED |
| **"pod" = a spawn BATCH here (not the Pod entity)** | a refill spawns a **batch of `max(3, 2 + floor(minute))` enemies directly** at the arena edges | to avoid the name clash: this batch is *not* a Pod-spawner entity. A **Pod entity** only appears when the batch's composition roll actually yields the Zombie/Swarmer type (then a real Pod spawns and emits per §4). "Pod size" below = this batch size |
| Pod (batch) size (spawn) | **max(3, 2 + floor(minute/1))** | +1 to the batch each elapsed minute |
| Concurrent enemy cap | **8 + floor(minute/2)** | grows 1 every 2 min (swarms still exceed it) |
| **Tier ramp** | unlock next tier every **3 min**: T0–1 (0–3m) → +T2 (3m) → +T3 (6m) → +T4/untiered (9m) → full roster (12m+) | |
| Enemy stat ramp | **+5% HP and +3% damage per minute**, capped at +150% HP / +90% dmg | HP/dmg creep past the roster unlock |
| Miniboss cadence | inject one every **5 min** (recurring big-versions) — **selected like the catch-up miniboss (§8.2): a random big-version of a currently-unlocked non-degenerate enemy type** (excludes Zombie/Swarmer/Pickpocket/Monkey; falls back to big Regular). Spawns at a **back-Z edge** | one selection rule shared with campaign catch-up |
| Boss cadence | inject a main boss every **10 min** | at boss-scale, from the placed pool |
| Spawn interval floor | never faster than a new pod every **4 s** | keeps it readable (`VFX.md` bullet budget) |
| **Wave composition** | each refill spawns a **type-weighted pod drawn from the currently-unlocked tiers**: **40% current-top-tier · 60% below it** (mirrors the campaign filler weighting, `ENCOUNTERS.md` §0), picked by a **per-run seed** so a session is reproducible for playtest. Each spawned **Pod's emit-type is 50/50 Swarmer/Zombie** (§4 Pod typing) | one weighting rule, seeded like the campaign |
| **Club (placed-pickup weapon)** | since Endless has no stages/mid-checkpoints, the **Club spawns as a periodic placed pickup every ~90 s** (same treatment as the Rocket Launcher) — the "all corpse-drop weapons unlocked" line covers corpse drops only; the two placed-pickup weapons (Club, Rocket) get this timed-spawn instead | resolves the placed-pickup gap in Endless |
| **Catch-up trigger in Endless** | the §8.2 catch-up-miniboss trigger is **OFF in Endless** — Endless has its own **5-min miniboss cadence** (below), so the "clearing too fast" injector is campaign-only (it would double up with the cadence) | one miniboss source in Endless |
| Economy/weapon rules | **all corpse-drop weapons unlocked from the start** (Endless has no areas, so the area-gate §6.1 doesn't apply to drops); the **Rocket Launcher stays world-pickup-only** — it spawns as a **periodic placed pickup every ~2 min** (never a corpse roll, per §6.1); **coins ON from minute 0** (Area-1–2 coin suppression is campaign-only); dimes/monkeys/decay as normal | Endless is the sanctioned playtest sandbox |
| **Backdrop / arena** | Endless runs in a **single fixed flat arena = a stylized SF-streets loop** (reuses the Area-4 SF-streets backdrop + city crowd bed, `AUDIO.md` §4) — no scrolling, camera-locked, 26.7 wu wide; matches the Endless music (the SF electro-punk layered track, `AUDIO.md` §2). **No new art** | one reused backdrop, no new asset |
| Injectable bosses | **only ungated HP-depletion bosses: Burly, big Arm-Ripper, Boomergunner, Sandwich Bros.** **Excluded** = every boss that needs a specific weapon, terrain, or script that Endless can't guarantee: **Colossus** (needs whips), **Gatling Gun Guy** (needs car cover), **Tank / Helicopter** (objective + weapon-gated adds), **Monkey Boss** (dime proxy), **Phil** (scripted). This prevents an un-winnable injection | resolves the Endless-boss + Colossus-softlock ambiguity |
| End condition | endless until death; score = kills × time-survived multiplier | leaderboard **(tunable)** |

### 8.4 Difficulty modes — **[LOCKED]** (chosen at the title / character-select, `UI.md` §5)

> Three difficulties multiply two independent knobs. **Everything else is identical** (same stages, bosses,
> economy, drops, frame data). Difficulty is picked once per run and shown on the HUD.

| Difficulty | Spawn multiplier | Enemy-damage multiplier (dmg dealt TO the player) |
|---|---|---|
| **Easy** | **×0.7** | **×0.5** |
| **Normal** (default) | **×1.0** | **×1.0** |
| **Hard** | **×2.0** | **×1.5** |

- **Spawn multiplier** scales **every wave's enemy count *and* the filler-wave count** (`ENCOUNTERS.md` §0),
  rounded to the nearest whole enemy (min 1 per listed spawn). The **8-pursuer cap still holds** — on Hard the
  extra enemies **queue and stream in faster**, they don't all crowd on screen at once (swarms still except).
  Boss/miniboss *counts* are unchanged (a boss is a boss); **boss-arena adds are ALSO unchanged** — the
  2-add cap (`BOSSES.md` §1) holds on every difficulty (weapon-gate arenas must not break). Only **stage wave
  fodder** scales.
- **Enemy-damage multiplier** scales **only the *variable* HP damage enemies deal to the player** (contact
  punches, standard projectiles, enemy-driven variable hazards). **Player→enemy damage, enemy HP, and all
  timings are unchanged** across difficulties.
- **[LOCKED] Fixed / instant-death sources are EXEMPT from the multiplier** — they are lethal or set-value on
  every difficulty, unscaled:
  - **Instant-death hazards:** trolley/cable-car flatten, fall-off-tower (Phil), grenade self-blast (40 is a
    fixed self-hit, not enemy damage), **Gatling Gun Guy boss barrage caught in the open**, **Head-Thrower /
    Staff-fire head-bomb** (`fire → 2 s → BOOM = player death`). These **kill outright at ×0.5 and ×1.5 alike**.
  - **The enemy Sniper's apex shot** is a **fixed set-value, not a subtraction, unscaled by difficulty** — the
    single coherent rule (matches §4 row 7 / §5): **if the player is at ≥25 HP the shot sets them to 20 HP; if
    the player is at <25 HP the shot KILLS** (you were already low, the sniper finishes you). Same on every
    difficulty (never scaled). There is **no separate "below 20 does nothing" case** — below 25 is the kill
    band, at/above 25 is the set-to-20 band; the two together cover all HP with no gap.
  - Rationale: these are **binary "you got caught" punishes**; multiplying them would make Easy trivialize a
    scripted death or Hard double an already-instant one. The difficulty knob only ever moves **survivable
    chip damage**.
- **Player HP stays 100** on every difficulty; the low-HP rubber-band (§2.2) is unchanged. So Easy = fewer,
  softer-hitting enemies; Hard = a denser crowd hitting 50% harder than Normal.
- **Endless** runs at its own curve (§8.3); the difficulty pick still applies its two multipliers on top.

---

## 9. One-line assumptions (where a value was inferred, not stated)

1. **Shotgunner move speed ×0.92** — bulk implies a small speed tax; the doc locks only "bigger/bulkier," so a
   modest penalty is assumed (all other characters' speeds are locked).
2. **Green meter tier = one stronger 45-kill shot** (not banked extra shots) — chosen as the read most
   consistent with "each fill AMPLIFIES, doesn't bank extra shots" (`GAMEPLAY_LOOP.md` §4.3).
3. **Untiered/TBD enemy damage** assigned an *effective tier* on the tier × 7.5 ladder (marked "-eff") so no
   damage cell is blank; the LOCKED per-enemy exceptions (Zombie 0, Swarm 1.5, Gatling 1, Sniper→20) override.
4. **No boss (all 10) is sniper-killable above 10%; only catch-up minibosses/non-boss elites are
   sniper-killable.** The **≤10% execute itself applies only to the 5 pure HP-depletion bosses** (Burly, big
   Arm-Ripper, Boomergunner, Gatling Gun Guy, Sandwich Bros); the 5 objective/proxy bosses (Colossus, Tank,
   Helicopter, Monkey Boss, Phil) have **no execute** (`BOSSES.md` §1 is authoritative).
