# this.l — Weapon Roster

> **Scope:** every weapon the Human can loot — behavior, decay/ammo economy, how it interacts with the
> combo/finisher rule, and per-weapon **asset needs**. Player *animation pipeline* lives in `PLAYER.md`.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[LATER]** parked.

---

## 1. Weapon system rules — **[LOCKED] core, [PROPOSED] where noted**

- **[LOCKED] Corpse-sourced.** Weapons come off dead **stick figures** — each is made of a **body part**
  (head, spine, limb-bone…). The corpse *is* the ammo/durability readout (diegetic, no HUD number).
- **[LOCKED] Everything decays.** No weapon is permanent; each has a **hits / shots / durability** budget,
  then it's gone and you're back to **fists**. This keeps you cycling into danger to re-loot.
- **[LOCKED] Enemy level → random loot.** A dead enemy's **level** sets the **tier** of the random weapon
  it can drop. Higher-level stick figures drop rarer / longer-lasting weapons. **[PROPOSED]** tier table
  in §4.
- **[LOCKED] Sniper-killed enemies drop nothing** (from the special — risk/reward).
- **[LOCKED] Not all weapons come from enemies.** Some **bigger weapons spawn as world pickups** placed
  along a stage (e.g. the **Rocket Launcher**, §3.8b) — often near tougher fights — for diversity beyond
  corpse-loot.
- **[LOCKED] Weapons are learn-by-use.** They're simple enough to need **no tutorial** — pick one up and
  figure it out; the long campaign gives room to ramp up naturally.
- **[LOCKED] `E` fires the equipped weapon (spends ammo); the combo finisher is FREE melee.** Attacking with
  a weapon runs the **melee combo** at fist strength; the **finisher is the regular 4th hit** — a strong melee
  blow that **costs no ammo**. To actually **fire / throw / cast** the weapon (spending ammo/durability) you
  press **`E` (use weapon)** (`PLAYER.md` §2). **[SUPERSEDES]** the earlier "ranged weapons fire on the
  finisher" — **firing is on `E`; the finisher is free melee.** Read every per-weapon "fire/finisher"
  description below as the **`E`-fire** action.
- **[LOCKED] Carry = single slot**, fists as the permanent fallback.
  - **Empty-handed:** walking over a drop **auto-picks** it.
  - **Already armed:** auto-pickup is suppressed; **tap the swap key** to take the weapon on the ground —
    and your **current weapon disappears** (destroyed, not dropped). No ground-hoarding or juggling.

---

## 2. Confirmed weapons — **[LOCKED]** (full spec)

### 2.1 Sword — *from a head-gone corpse*
- **Type:** melee (real swing kit — full directional + air attacks).
- **Behavior:** bigger reach & damage than fists; the go-to upgrade.
- **Decay:** **5–10 connecting hits**, then it shatters/decays. **[PROPOSED]** exact number by tier.
- **Diegetic readout:** blade **visibly wears/chips** as hits deplete; final hit it breaks.
- **Assets:** sword-in-hand idle/walk/jump · directional swing set (side/up/down + air) · wear states
  (fresh → chipped → breaking) · break VFX.

### 2.2 Shotgun — *spine = ammo*
- **Type:** ranged, but **melee'd through the combo**; **fires a shell on `E`** (the free 4th-hit finisher is melee).
- **Behavior:** finisher = **blast** (short-range spread, big damage / knockback). After firing, the Human
  **cocks it** and a **spine segment ejects** — the **remaining spine = shots left.**
- **Ammo:** number of **spine segments** (e.g. 4–6). When spine is spent, gun is gone.
- **Diegetic readout:** the **spine magazine** shrinks segment-by-segment; no HUD ammo counter needed.
- **Warm-up:** slight aim before the blast (per weapon warm-up rule).
- **Assets:** shotgun-in-hand idle/walk · bludgeon combo (reuses fist body holding gun) · **finisher:
  fire + cock + spine-eject** · muzzle flash · spine-segment bit VFX · empty/discard.

### 2.3 Boomerang — *bent limb-bone*
- **Type:** thrown.
- **Behavior:** **throw infinitely** — on a **miss it returns** to hand; on **hitting an enemy it bounces
  off and you lose it**, and that enemy is **stunned for 2s**. So it's infinite *only if you keep missing*;
  landing a hit trades the weapon for a 2-second stun (a setup tool, not a damage tool).
- **Decay:** not durability-based — you **lose it on the first enemy hit** (retrieve the dropped one, or
  re-loot). **[PROPOSED]** the dropped boomerang lies on the ground to pick back up.
- **Assets:** boomerang-in-hand · **throw** anim · spinning in-flight sprite (reused as projectile) ·
  return arc · stun VFX on the struck enemy · grounded pickup sprite.

---

## 3. Full roster — listed to iterate

> **Theming note:** weapons are **function-first** — we define what each *does*; flavor is secondary. Only
> the locked sword / spine-shotgun / boomerang lean on the corpse-part gag; the rest don't have to.
>
> **[ITERATE]** = captured, we flesh it out next.

**Locked & spec'd (§2):** Sword · Shotgun (spine ammo) · Boomerang

### 3.1 Pistol & Revolver — *precise straight-line guns* **[LOCKED core]**
Two variants of the same 1-v-1 idea (shotgun = crowd control; these = single-target). **No aiming — they
fire straight ahead** (horizontal); lining the shot up with an enemy's **head** is the skill, and part of
the feel.
- **[LOCKED] Normal fire is on `E` — any target, any HP.** Per the §1 supersede rule, you **press `E` to
  fire** the gun straight ahead (spending 1 mag round) at an enemy of **any** health. Pistol pierces the row
  (12/6/3), Revolver hits one for 30. This is the everyday shot — the pierce and headshot lineups work on
  full-HP enemies. **The `E`-fire is NOT gated on HP.**
- **[LOCKED] The `<20% HP` gate applies ONLY to the *finisher execute*** (`COMBOS.md` §2) — the
  **same-direction double-tap** (`→→ ←← ↑↑ ↓↓`) that fires into a **swept, downed** enemy (Quickdraw, Coup de
  Grâce, Skyshot, No-Look). The shot only discharges if that downed enemy is **< 20% HP**; otherwise the
  double-tap is a **melee pistol-whip finisher** (no bullet spent). So there are two ways to shoot: **plain `E`
  (any standing target, any HP, spends a round)** and the **finisher execute (a swept enemy < 20% HP, also
  spends a round)**. **Execute can never hit a random standing enemy — you must sweep it down first**
  (`PLAYER.md` §3). *(This resolves the old "only discharges on the finisher, only <20%" line — superseded; it
  now describes only the double-tap execute.)*
- **Pistol:** **more bullets, less damage**, and **pierces up to 3 enemies**, damage **halving through
  each** — a lined-up shot can drop a whole row.
- **Revolver:** **more damage, no pierce**, fewer bullets — the heavy single-target hitter.
- **Headshot:** if the straight shot lines up with a head it lands as a headshot (kill/bonus on weak enemies).
- **[LOCKED] Zombie risk:** a headshot kill has a **~10% chance to spawn a ~10s zombie instead** (see
  `ENEMIES.md` §2.8) — the small tax on headshot-leaning play.
- **[LOCKED] Mags & decay:** **Pistol mag = 8, Revolver mag = 6** (`TUNING.md` §6); when the mag empties the
  gun is **spent and auto-discarded** (no reload, `TUNING.md` §6 ammo rule). The **<20% execution gate is
  pistol/revolver-specific** (other guns fire freely on `E`).

### 3.2 Grenade / Bomb — *thrown, physics-based* **[LOCKED core]**
Enemy-dropped; **1 per pickup** (scarce — save it for a cluster).
- **Thrown with `E`** (like all weapons) — **tap `E` during the wind-up to change the trajectory:** **more
  taps = flatter line-drive (fastball); fewer taps = high lob.** *(Resolves the arrow-tap ambiguity: arrows
  are melee, `E` throws/fires.)* So:
  - **Fewer presses → high lob:** arcs up high and comes down for a **bigger explosion** (heavy blast,
    precise placement). A **ground marker shows where it first lands**; it **bounces 3×, then explodes.**
  - **More presses → fastball:** the reverse — a **fast, flat throw** that **plows along the ground,
    knocking down enemies near its path**, and **explodes at a set distance (or after hitting ~5–10
    enemies)** with a **smaller blast.**
- **[LOCKED] Self-damage is real** — your own explosion **can catch you** if you're too close. Spacing is
  the price of the payload.
- **[LOCKED] Numbers (`TUNING.md` §6):** lob blast **60 (r 3 wu)**, fastball blast **35 (r 2 wu)**; fastball
  detonates at **8 wu or after 8 enemies**; **self-damage 40**. **Knockdown from the blast lasts 0.8 s**
  (`TUNING.md` §2.6 standard knockdown, applied to caught *regular* enemies — bosses/minibosses are immune,
  §2.6). The lob **bounces 3× then explodes**.

### 3.3 Ball & Chain — *heavy directional launcher* **[LOCKED core]**
- Plays **like the grenade's throw:** **tap `E` during the wind-up to change trajectory** (more taps =
  flatter line-drive). The ball launches out on its chain and does a **ridiculous amount of damage.**
- **Carrying it slows the player ~20%** — the weight is the tax; a heavy **commitment** weapon.
- **Only 3 uses**, then it's gone — each swing is precious.
- **[LOCKED] Directional `E`-launches (`COMBOS.md` §3):** hold a direction + `E` to shape the launch — forward
  **Meteor Line-Drive**, ↑ **Wrecking Uppercut**, ↓ **Ground Zero** (radial knockdown), back **Full Swing
  (360)**. Each spends 1 of the 3 uses. The **combo finisher** (double-tap on a swept enemy) with the Ball &
  Chain equipped is just the **free melee slam** — launching is the `E`-fire, not the finisher.
- **[ITERATE]** exact reach per tap; whether it hits everything along the chain's path; knockback; whether
  the 20% slow also touches attack speed.

### 3.4 Whip — *directional crowd melee* **[LOCKED core]**
Each attack direction is a different tool:
- **Up = overhead arc** — sweeps an arc (good when flanked / hitting above).
- **Forward = pull** — snags an enemy and yanks (reposition / combo starter).
- **Down = horizontal line** — long straight crack hitting everything in the line (spacing / crowd).
- **Finisher — "the extraction":** the whip **chases an enemy, wraps its neck**, the player **rips the head
  clean off**, the **head becomes a live grenade**, and the player **auto-dashes backward** (opposite the
  grenade) to clear the blast. A self-made bomb with a built-in escape.
- **Decay:** breaks after **~10–12 connecting hits** (fray states, like the sword).
- **[ITERATE]** pull distance and whether it drags the enemy to you or you to them; the head-grenade's
  blast (reuse §3.2 tuning?); finisher target priority; exact hit count by tier.

### 3.5 Staff — *magic caster* **[LOCKED core]**
- **Element is set at pickup — randomly one of three: Ice, Fire, Lightning.** A given staff stays that
  one element for its whole life. The **finisher casts** the element's effect:
  - **Ice** — crowd control: **freezes** enemies, **less damage**. Lockdown tool.
  - **Lightning** — **stun damage** + **slows** enemies. Tempo/control.
  - **Fire** — **burns** enemies (damage over time). **Signature interaction:** burning a **grenade enemy**
    (the stick figure that pulls off its own head to throw at you) makes it **start blinking, then after
    ~2s BOOM** — a small blast that **kills the player** if caught in it. Great damage, but it turns that
    enemy into a walking bomb you must not be next to. *(Grenade enemy specced in `ENEMIES.md`.)*
- **[LOCKED] Aim & decay:** `E` **fires straight ahead** in your facing direction (like the guns — no arrow
  aiming; the element is fixed at pickup). **6 casts** then the staff breaks (`TUNING.md` §6). **Cast warm-up
  0.35 s** (§6). The **Fire walking-bomb explosion damages other enemies** in its r 2 wu blast (enemy friendly
  fire, like the head-grenade). Freeze/stun/slow are single-target on the hit enemy.
- **[SUPERSEDES]** the earlier "each direction = a different spell" — element is now fixed per pickup.

### 3.6 Gatling Gun — *heavy risk/reward* **[LOCKED core]**
- **No ammo count** — it doesn't deplete per shot.
- **Slow combo** — its attack cadence is noticeably **slower** (heavy weapon).
- **Finisher = ~0.5s of repeated fire into the enemy** — the player unloads point-blank for about half a
  second; a guaranteed **auto-kill / headshot** on a normal enemy.
- **No i-frames — the player is locked and vulnerable** through that ~0.5s barrage: the guaranteed kill is
  paid for in **exposure**, so throwing it out in a crowd gets you hit.
- **[LOCKED] Zombie risk:** the auto-kill headshot finisher has a **~10% chance to spawn a ~10s zombie
  instead of killing** (see `ENEMIES.md` §2.8).
- **[ITERATE]** how it's eventually lost (no ammo → overheat? time limit? N finishers?); does the headshot
  hit one target or pierce/chain; warm-up/spin-up.
- **[SUPERSEDES]** the earlier "20–32 shots" — no ammo tracking now.

### 3.7 Monkey Merc — *summon, costs a dime (10¢, see §3.9)* **[LOCKED core]**
**[LOCKED] Second-half only** (debuts ~Area 3 with the monkey economy, `STAGES.md` §1c).
Dropped by a **monkey stick figure**; you can only claim/summon it if you hold a **dime (10¢).**
- **Own aggro & attacks:** fights independently with a **pistol, infinite ammo**, but a **max fire rate of
  2 shots/second** (it's "aiming").
- **Stacking upgrades the whole squad's weapon** — more monkeys = bigger guns, shorter lives:

  | Active monkeys | Weapon | Lifespan each |
  |---|---|---|
  | 1 | Pistol | 20s |
  | 2 | Shotguns | 10s |
  | 3 | Rocket launchers | 5s |

  Fire rate stays **2/sec at every tier**, so **3 rocket-launcher monkeys can wipe the whole screen** in
  their short window — a high-roll payoff for saving up dimes.
- **Per-level cap:** summon **up to 3 over a level** (resummon as they die); **once 3 have died, no more
  monkeys this level.**
- **[LOCKED] Re-arm:** adding a monkey **re-arms the whole squad to the new tier and resets all their timers**
  (`TUNING.md` §6 Monkey Merc row). They **cannot be healed** (they just expire on their timer).
- **[LOCKED] No friendly fire:** the player's summoned monkeys — **including the rocket monkeys** — **never
  damage the player** (their rockets pass through you; only enemies take the blast). The screen-wipe is safe to
  stand in. *(This is why the 3-rocket-monkey high-roll is pure payoff, not a self-risk.)*

### 3.7b Bat — *projectile reflector* **[LOCKED]** *(surfaced by the Helicopter boss)*
- A **melee weapon that reflects projectiles** — bat incoming shots (e.g. the Helicopter's thrown heads)
  **back at the attacker.**
- **[LOCKED] Availability:** a **full roster weapon**, in the **Area-2 drop pool onward** (`TUNING.md` §6.1) —
  it **debuts** as the Helicopter arena's weapon-gated drop, then remains a normal T2 corpse drop thereafter.
  It is **not** boss-arena-only.
- **[LOCKED] Stats (`TUNING.md` §6):** melee **12/hit**, **12 connecting hits**, **reflect window 0.20 s**.
- **[LOCKED] What it reflects:** thrown heads, head-grenades, boomerang-gun shots, arm-ripper/pistol rounds,
  shuriken — any **telegraphed slow-to-medium projectile.** It **cannot** reflect the **gatling/barrage
  stream** (too dense) or **melee.** A reflected shot deals the **original attacker's damage** back.

### 3.7c Club — *basic heavy melee* **[LOCKED]** — **pickup, debuts Area 2 (airport, after the vignette)**
- A simple **blunt melee** weapon — an early workhorse alongside the Sword. Bigger knockback than fists,
  short reach. Appears as a **world/airport pickup** starting at the airport (not a corpse drop, `TUNING.md` §6.1).
- **[LOCKED] Stats (`TUNING.md` §6):** melee **14/hit** + knockback, **10 connecting hits**, warm-up 0.15 s,
  no E-fire.

### 3.8 Boomerang Gun — *thrown auto-fire* **[LOCKED core]**
- A **gun you throw**; it flies a **fixed orbit arc** (a set boomerang loop) and **shoots whatever it
  passes**, then **returns**. Not auto-homing — you aim it by **where you position and throw.**
- **While it's out you're free to move/dash, but only fists are available** until it returns to hand.
- **Ammo = 10 bullets total — the only resource.** It fires **up to 4 shots per pass** (so ~3 passes to
  empty). **A throw that fires no bullets costs nothing** — only spent bullets count.
- **[LOCKED] It can be shot down mid-flight** — an enemy destroying it **loses you the remaining bullets.**
- **[LOCKED] Shot down = gone for good** — if an enemy destroys it mid-flight you **lose it and the remaining
  bullets** (no drop to re-grab); back to fists. **Stats (`TUNING.md` §6):** **10 bullets, 4/pass** (~3 passes),
  fire cadence within a pass ~**0.15 s/shot**, **throw cooldown 1 s**, fixed orbit ~**5 wu** loop.

### 3.8b Rocket Launcher — *world-pickup heavy* **[LOCKED core]**
- A **big-hitting explosive** weapon. **[LOCKED] Comes from a specific world PICKUP**, not from a corpse —
  it **appears along the way** (often when you're up against **bigger enemies**) for a burst of extra
  firepower. First of the **non-enemy weapon source** (§1).
- Distinct from the **Monkey Merc rocket launchers** (§3.7) — this one is the player's own.
- **[LOCKED] Stats & placement (`TUNING.md` §6/§6.1):** **3 rockets**, blast **70 (r 3 wu)**, **self-damage 35**
  (like the grenade), **warm-up 0.50 s**, **`E` free-fire** (not finisher-gated — it's a fired weapon).
  **Placed pickups:** near the **Tank** fight (Stage 9) and in the **SF gauntlet** (Stage 12) — never in a
  random drop pool.

### 3.9 Currency system — **[LOCKED core], [ITERATE] scope** *(cross-cuts `UI.md`)*
- **[LOCKED] Second-half reveal:** **no money appears in the first half** (~Areas 1–2); the whole coin/monkey
  economy **debuts ~Area 3** (`STAGES.md` §1c) — the game keeps unfolding new systems.
- Enemies **randomly drop coins**; each = **1 cent**. Money is **shown in the UI** and **resets each stage**
  (`UI.md` §3.4).
- **[LOCKED] Coin drop rate ≈ 12%** per non-swarm kill (raised from 5% so dimes are actually reachable);
  **Swarmers drop 0** (fodder never feeds the economy).
- **10¢ = a dime**, the cost to take/summon a **Monkey Merc** (§3.7).
- **[ITERATE]** exact per-tier rates & any cap; money only for monkeys or a broader economy (shop? other
  buys?). If it grows past monkeys it earns its own `ECONOMY.md`.

*More weapons welcome — this list is meant to grow; we iterate each `[ITERATE]` into a full §2-style spec.*

---

## 4. Tier ↔ enemy-level mapping — **[PROPOSED]**

| Tier | Drops from | Example weapons | Feel |
|---|---|---|---|
| **T1 common** | low-level stick figures | boomerang, pistol / revolver | early, low commitment |
| **T2 uncommon** | mid enemies | sword, whip, ball & chain | the workhorses |
| **T3 rare** | high/elite enemies | shotgun, staff, gatling gun | powerful, scarcer |
| **T4 special** | minibosses / specific spawns | grenade, **monkey merc** (needs a dime) | spice, situational |

**[LOCKED]** the *specific* weapon within a tier is **random** (you adapt to what drops). **[LATER]** exact
tier contents, drop rates, whether some weapons only come from specific enemy archetypes.

---

## 5. Per-weapon asset summary → feeds `ASSET_MANIFEST.md`

For **each greenlit weapon**:
- **Melee weapon:** in-hand idle/walk/jump · directional swing kit (side/up/down + air) · wear/decay
  states · break/discard VFX.
- **Ranged weapon:** in-hand idle/walk · bludgeon combo (fist body + weapon) · **finisher fire** anim ·
  muzzle/projectile VFX · ammo-readout bits (e.g. spine segments) · empty/discard.
- **Throwable/utility:** in-hand · throw/deploy anim · in-flight/placed sprite · effect VFX · pickup or
  spent state.

Shared: every dropped weapon needs a **ground pickup sprite** and a **decay/break puff**.

---

## 6. Status & next step

**Resolved (now [LOCKED]):** single-slot carry (auto-pick when empty; swap-key destroys the old weapon
when armed); function-first theming; the roster list above; grenade press-to-throw physics; the currency
system core (wallets → cents → dime → monkey merc).

**Iterating one at a time (together).** ✅ All listed weapons specced: **Sword, Shotgun, Boomerang** (§2)
+ **Staff, Gatling, Pistol & Revolver, Grenade, Boomerang Gun, Whip, Ball & Chain, Monkey Merc** (§3). The
roster stays **open** — new weapons drop in as §3-style entries any time.

**New cross-cutting system flagged — *Secret Combos*:** specific directional-input strings trigger special
finishers/effects (ball & chain uses these; pistol/revolver have per-direction finishers too). **[LATER]**
gets its own `COMBOS.md` when we define the input strings.

**[LATER]:** durability numbers, tier drop rates, per-archetype loot restrictions, finisher damage,
whether currency grows into a full economy.
