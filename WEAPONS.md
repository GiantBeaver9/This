# this.l — Weapon Roster

> **Scope:** every weapon the Human can loot — behavior, decay/ammo economy, how it interacts with the
> combo/finisher rule, and per-weapon **asset needs**. Player *animation pipeline* lives in `PLAYER.md`.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[LATER]** parked.
>
> **[AUTHORITY BANNER]** This is a **design/rationale** doc. **Concrete truth lives in the authority docs:**
> `TUNING.md` (every number — damage, ammo, warm-up, kinematics), `COMBOS.md` (finishers), `ENCOUNTERS.md`
> (placements). Any `[ITERATE]`/`[PROPOSED]`/`[LATER]` marker below is a **historical note** — if the item is
> pinned in an authority doc, THAT is the locked answer. No marker below blocks the build.

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
  finisher" — **firing is on `E`; the plain finisher is free melee.** Read the per-weapon **fire/throw/cast**
  description as the **`E`-fire** action. **Two exceptions are genuine finisher-path moves, not `E`-fire:** the
  **gun `<20%` executions** (`COMBOS.md` §2) and the **Whip head-rip extraction** (`COMBOS.md` §4) — those fire
  on the finisher double-tap, per COMBOS.
- **[LOCKED] Carry = single slot**, fists as the permanent fallback.
  - **Empty-handed:** walking over a drop **auto-picks** it.
  - **Already armed:** auto-pickup is suppressed; **tap the swap key** to take the weapon on the ground —
    and your **current weapon disappears** (destroyed, not dropped). No ground-hoarding or juggling.

---

## 2. Confirmed weapons — **[LOCKED]** (full spec)

### 2.1 Sword — *from a head-gone corpse*
- **Type:** melee (real swing kit — full directional + air attacks).
- **Behavior:** bigger reach & damage than fists; the go-to upgrade.
- **Decay:** **8 connecting hits** (of a 5–10 range), then it shatters/decays; blade chips at **6 / 4 / 2**
  left (LOCKED, `TUNING.md` §6).
- **Diegetic readout:** blade **visibly wears/chips** as hits deplete; final hit it breaks.
- **Assets:** sword-in-hand idle/walk/jump · directional swing set (side/up/down + air) · wear states
  (fresh → chipped → breaking) · break VFX.

### 2.2 Shotgun — *spine = ammo*
- **Type:** ranged, but **melee'd through the combo**; **fires a shell on `E`** (the free 4th-hit finisher is melee).
- **Behavior:** **`E` fires the blast** (short-range spread, big damage / knockback — NOT the combo finisher;
  the finisher is the free melee, per §1's supersede rule). After firing, the Human **cocks it** and a
  **spine segment ejects** — the **remaining spine = shots left.**
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
- **[LOCKED] Headshot = the buildable predicate in `TUNING.md` §4:** guns have **no manual aim** — every shot
  flies **straight ahead on the player's Z-row at a fixed plane = head height**. A hit is a headshot iff it
  **kills a standing regular non-boss** (airborne/downed enemies are body shots; bosses/minibosses/Heavy are
  headshot-immune). A headshot kill is the thing that rolls the zombify tax below.
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
    knocking down enemies near its path**, and **explodes at 8 wu (or after hitting 8 enemies)** (LOCKED,
    below) with a **smaller blast.**
- **[LOCKED] Self-damage is real** — your own explosion **can catch you** if you're too close. Spacing is
  the price of the payload.
- **[LOCKED] Tap→trajectory mapping (concrete):** count the `E`-taps during the **~0.4 s wind-up** — **4
  discrete steps**: **0 taps = full lob** (highest arc, bounces 3× → the 60 blast) · **1 tap = high arc** ·
  **2 taps = mid/liner** · **3+ taps = flat fastball** (the 35 blast, plows the ground). It is **stepped, not
  continuous** — each tap lowers the arc one notch, capping at fastball. The same 0→3 stepping drives the Ball
  & Chain's two arced shapes (`WEAPONS.md` §3.3). *(This resolves the "more taps = flatter" ambiguity.)*
- **[LOCKED] Numbers (`TUNING.md` §6):** lob blast **60 (r 3 wu)**, fastball blast **35 (r 2 wu)**; fastball
  detonates at **8 wu or after 8 enemies**; **self-damage 40**. **Knockdown from the blast = the standard
  1.2 s** (`TUNING.md` §2.6, applied to caught *regular* enemies — bosses/minibosses are immune, §2.6).
  **[LOCKED] Fastball's in-flight plow knocks down enemies within 1.0 wu of its travel line (±1 Z-row)** as it
  crosses them (before the blast); the terminal blast then knocks down everything in its r 2 wu.
- **[LOCKED] Lob & intermediate-step distances** (the ground-marker where it first lands): **0 taps = 6 wu**
  ahead (highest arc) · **1 tap = 8 wu** · **2 taps = 10 wu** (liner) · **3+ taps = flat fastball** (travels
  until 8 wu / 8 enemies). The lob **bounces 3× at ~2 wu spacing** past its landing marker, then explodes; the
  liner bounces once; the fastball doesn't bounce. Each step's arc peak drops ~30%.
- **[LOCKED] Anti-air lob (`↑`-held throw):** holding **`↑` (up)** while pressing `E` throws the grenade **in a
  high vertical arc at an airborne target** (up to **~6 wu high**) — this is the shape used vs. the
  **Helicopter** (`BOSSES.md` §5.5: "lob it up"). Direction held picks the arc's target (forward = ground lob;
  up = anti-air); tap-count still flattens within the chosen arc. Resolves the "no upward throw exists" gap.

### 3.3 Ball & Chain — *heavy directional launcher* **[LOCKED core]**
- **[LOCKED] How the two inputs compose:** you **hold a direction + press `E`** to pick the **launch SHAPE**
  (the 4 shapes in `COMBOS.md` §3); **for the two *arced/forward* shapes** (Meteor Line-Drive fwd, Wrecking
  Uppercut ↑) **extra `E`-taps during the wind-up flatten that shape's arc** (more taps = flatter/faster line,
  fewer = higher/slower — exactly the grenade's lob↔fastball feel). **The radial shapes (Ground Zero ↓, Full
  Swing back) ignore tap-count** (they have no arc to flatten). So: **direction = shape, tap-count = arc**, no
  ambiguity. The ball does a **ridiculous amount of damage (80/swing)**.
- **Carrying it slows the player ~20%** — the weight is the tax; a heavy **commitment** weapon.
- **Only 3 uses**, then it's gone — each swing is precious.
- **[LOCKED] Directional `E`-launches (`COMBOS.md` §3):** forward **Meteor Line-Drive**, ↑ **Wrecking
  Uppercut**, ↓ **Ground Zero** (radial knockdown), back **Full Swing (360)**. Each spends 1 of the 3 uses.
  The **normal combo string** (when you're not launching) swings the ball as **heavy melee at 20/hit**; the
  **combo finisher** (double-tap on a swept enemy) with the Ball &
  Chain equipped is a **free ground-slam at 50** (= 20 × 2.5, `TUNING.md` §6) — launching is the `E`-fire and
  the only thing that spends a use; the normal string and finisher spend none.
- **[LOCKED]** reach is **fixed per launch shape** (`COMBOS.md` §3: Meteor 8 wu line · Uppercut 4 wu up ·
  Ground Zero r 3 wu · Full Swing r 2.5 wu ring) — **taps only flatten the arc, they do NOT change reach**.
  Launches **hit everything along the chain's path** (each hit 80). The **20% slow is movement-only** (attack
  speed is unaffected, `TUNING.md` §6).

### 3.4 Whip — *directional crowd melee* **[LOCKED core]**
- **[LOCKED] Input mapping — one unambiguous rule (uses the standard combo model, `PLAYER.md` §3):** the whip
  runs the **normal P1→P2→sweep→finisher string on FORWARD/neutral attack presses**, exactly like fists, and its
  **up-arc and down-line are the UP-arrow and DOWN-arrow directional attacks** (the whip's versions of the fist's
  up/down attacks). There is **no separate "pull" input** — the pull IS what **P1 (the string's first forward
  hit)** does. So:
  - **P1** (first forward attack) = **pull-crack: 14 dmg + drags a regular enemy 3 wu toward you** (the combo
    starter; the drag is CC, so H-weight/minibosses/bosses take the 14 but are **not** pulled, §2.6).
  - **P2** (second forward attack) = **forward crack, 14**.
  - **Sweep** (hit 3, the primed same-direction double-tap, `PLAYER.md` §3) = **whip sweep, 14 + the standard
    1.2 s knockdown** — the whip **can** knock a regular down (it is a melee weapon, not a boss, so §2.6's
    no-boss-sweep doesn't apply to regulars).
  - **Finisher** (hit 4) = **the head-rip extraction** (below), replacing the free-melee finisher.
  - **UP-arrow attack** = **overhead arc** (2.5 wu, flank/above); **DOWN-arrow attack** = **horizontal line**
    (4.0 wu, hits the target's full-width Z-slice — the crowd tool). These are standalone directional attacks,
    not part of the forward string; pressing up/down does the arc/line, pressing forward runs the string.
  - Reach per tool = `TUNING.md` §1 whip row (up 2.5 / fwd-pull 3.0 / down-line 4.0 wu).
- **Finisher — "the extraction":** the whip **chases an enemy, wraps its neck**, the player **rips the head
  clean off**, the **head becomes a live grenade**, and the player **auto-dashes backward** (opposite the
  grenade) to clear the blast. A self-made bomb with a built-in escape.
- **[LOCKED] Whip finisher on a headless target:** a target with **no head to rip** — a Head-Thrower in its 4 s
  regrow window, or a hollow-head Zombie — **cannot be head-ripped**; the finisher instead lands as a **plain
  free-melee finisher dealing 35 damage** (no head-grenade). This is **damage, not a guaranteed execution** — it
  kills only if 35 ≥ the target's remaining HP (a downed full-HP Heavy survives it). The head-grenade extraction
  only fires when there's a real head to extract.
- **Decay:** breaks after **11 connecting hits** (of a 10–12 range, `TUNING.md` §6; fray states like the sword).
- **[LOCKED] pull drags the ENEMY to you** (not you to them); the **head-grenade reuses the grenade fastball
  blast (35, r 2 wu, `WEAPONS.md` §3.2)**; **finisher target priority = the nearest swept/downed enemy in
  front** (same as any finisher, `COMBOS.md` §4).

### 3.5 Staff — *magic caster* **[LOCKED core]**
- **Element is set at pickup — randomly one of three: Ice, Fire, Lightning.** A given staff stays that
  one element for its whole life. **`E` casts** the element's effect (NOT the combo finisher — the finisher is
  the free melee, per §1's supersede rule):
  - **Ice** — crowd control: **8 damage + freezes the enemy solid for 3 s** (`TUNING.md` §6). Lockdown tool.
    **[LOCKED] Freeze interaction rules:** (a) a frozen enemy is **fully inert** — its current action is
    **interrupted** and it cannot move/attack/telegraph until thaw; (b) **damage does NOT break the freeze** —
    you can freely wail on a frozen enemy and it stays frozen the full 3 s (freeze ends only on its **timer**);
    (c) a frozen enemy **can still be swept and finished** — the sweep (hit 3) knocks the frozen body down and
    the finisher connects normally (freeze doesn't protect it); (d) if the enemy **dies while frozen** it
    **shatters** (normal death, drops roll as usual); (e) **H-weight, minibosses, and bosses are immune** — they
    take the 8 damage but never freeze (status-immunity, `TUNING.md` §2.6).
  - **Lightning** — **12 damage + a 1 s stun, then a −40% movement slow for 2 s** after the stun ends
    (`TUNING.md` §6). Tempo/control. **H-weight and bosses/minibosses are immune to both the stun and the
    slow** (status-immunity, `TUNING.md` §2.6) — they take only the 12 damage.
  - **Fire** — **burns** enemies (damage over time). **Signature interaction:** burning a **grenade enemy**
    (the stick figure that pulls off its own head to throw at you) makes it **start blinking, then after
    ~2s BOOM** — a small blast that **kills the player** if caught in it. Great damage, but it turns that
    enemy into a walking bomb you must not be next to. *(Grenade enemy specced in `ENEMIES.md`.)*
    **[LOCKED] Fire DoT = 6/s for 3 s (18 total); a re-cast on an already-burning enemy REFRESHES the 3 s timer,
    it does not stack** (damage stays 6/s, never doubles) — one burn instance per enemy. A burn on a non-grenade
    enemy just ticks; on a **regular Head-Thrower** it triggers the walking-bomb.
    **[LOCKED] The walking-bomb CONVERSION does NOT apply to a big Head-Thrower miniboss** (or any boss) — the
    conversion is a status effect, and minibosses/bosses are status-immune (`TUNING.md` §2.6); they still take
    the 6/s burn *damage*, but they never become a walking bomb (no one-shot on an elite). Only regular
    Head-Throwers convert.
- **[LOCKED] Aim & decay:** `E` **fires straight ahead** in your facing direction (like the guns — no arrow
  aiming; the element is fixed at pickup). **6 casts** then the staff breaks (`TUNING.md` §6). **Cast warm-up
  0.35 s** (§6). The **Fire walking-bomb explosion damages other enemies** in its r 2 wu blast for **35** (enemy friendly
  fire, the same value as the grenade fastball blast at r 2 wu, `TUNING.md` §6/§6.2) — the lit enemy itself dies
  in the boom, and the player is killed outright if caught in the r 2 wu. Freeze/stun/slow are single-target on
  the hit enemy.
- **[SUPERSEDES]** the earlier "each direction = a different spell" — element is now fixed per pickup.

### 3.6 Gatling Gun — *heavy risk/reward* **[LOCKED core]**
- **No ammo count** — it doesn't deplete per shot.
- **Slow combo** — its attack cadence is noticeably **slower** (heavy weapon).
- **[LOCKED] The barrage is an `E`-fire, NOT the combo finisher** (per §1's supersede rule): **press `E`** to
  unload **~0.5s of point-blank fire** into the nearest enemy ahead — a guaranteed **auto-kill/headshot** on any
  standing **regular** non-boss. The combo finisher with the gatling equipped is a plain
  free melee blow. *(Older text called this a "finisher"; read it as the `E`-barrage.)*
- **[LOCKED] Barrage vs. armored targets = fixed 45 damage, no auto-kill** (the ONE rule, authoritative copy in
  `TUNING.md` §6). Against **all H-weight enemies (Heavy, Ground Smasher, Gatling Gunner)**, **any miniboss**,
  and **the 5 HP-depletion bosses** (Burly, big Arm-Ripper, Boomergunner, Gatling Gun Guy, Sandwich Bros), one
  barrage deals a flat **45 damage** instead of an instant kill (too much HP to cheese; the gatling is a weapon,
  so it *does* damage HP-bar bosses — unlike a character special). So a Heavy (220 HP) takes ~5 barrages; a
  big-version T2 miniboss (90–160 HP after the ×2 scale) takes 2–4; an HP-depletion boss can be chipped but
  never soloed — the gun overheats after **5 barrages = 225 damage**, below every boss's HP. The **5
  objective/proxy bosses (Colossus, Tank, Helicopter, Monkey Boss, Phil) take 0** — no normal HP bar to chip.
  The 10% zombify roll applies **only** to a barrage that auto-kills a regular standing enemy, never to the
  flat-45 case.
- **No i-frames — the player is locked and vulnerable** through the ~0.5s barrage: the kill is paid for in
  **exposure**, so throwing it out in a crowd gets you hit.
- **[LOCKED] Zombie risk:** an `E`-barrage that lands as a **headshot kill** has a **~10% chance to spawn a
  ~10s zombie instead** (`ENEMIES.md` §2.8).
- **[LOCKED] Loss & targeting (`TUNING.md` §6):** **no ammo**; overheats after **5 barrages OR 20 s cumulative
  equipped** then discards; targets the **nearest enemy ahead within 8 wu** (one target, no pierce);
  **0.40 s spin-up**.
- **[SUPERSEDES]** the earlier "20–32 shots" — no ammo tracking now.

### 3.7 Monkey Merc — *summon, costs a dime (10¢, see §3.9)* **[LOCKED core]**
**[LOCKED] Second-half only** (debuts ~Area 3 with the monkey economy, `STAGES.md` §1c).
- **[LOCKED] Acquisition flow (not a weapon slot):** a **Monkey stick figure** drops a **Merc-claim token** on
  death. **Walk over it while holding a dime** → the dime is **spent immediately** and the merc **poofs in
  (0.5 s summon)** and fights on its own. If you have **no dime**, the token can't be claimed (it lingers ~5 s
  then fades). The merc is **NOT a held weapon** — it doesn't occupy your weapon slot or fire on `E`; your
  hands stay free for fists/weapons. (`TUNING.md` §6's "0.5 s summon" is this poof.)
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
- **[LOCKED] Per-level cap = 3 SUMMONS per level** (each costs a dime). They **stack while alive** (the table
  above upgrades the squad's weapon by *live* count); **once you've summoned 3 total this level — whether they
  died or expired — no more**, regardless of dimes held. (Matches `TUNING.md` §6 "3 summons/level"; supersedes
  the older "3 deaths" wording — the cap is on *summons made*, not deaths.)
- **[LOCKED] Re-arm:** adding a monkey **re-arms the whole squad to the new tier and resets all their timers**
  (`TUNING.md` §6 Monkey Merc row). They **cannot be healed** (they just expire on their timer).
- **[LOCKED] No friendly fire:** the player's summoned monkeys — **including the rocket monkeys** — **never
  damage the player** (their rockets pass through you; only enemies take the blast). The screen-wipe is safe to
  stand in. *(This is why the 3-rocket-monkey high-roll is pure payoff, not a self-risk.)*

### 3.7b Bat — *projectile reflector* **[LOCKED]** *(surfaced by the Helicopter boss)*
- A **melee weapon that reflects projectiles** — bat incoming shots (e.g. the Helicopter's thrown heads)
  **back at the attacker.**
- **[LOCKED] Availability (no ordering conflict):** a **full roster weapon**. It enters the **corpse-drop pool
  at the start of Stage 5** (the airport), **right after its head-of-stage vignette teaches it** — so it *can*
  drop from Stage-5 corpses **before** you reach the Helicopter arena at the stage's end. The **Helicopter
  arena additionally guarantees** a bat supply via its weapon-gate (so you're never without one for the boss).
  Vignette → corpse drops → guaranteed at the boss = teach→tools→test intact. It is **not** boss-arena-only
  (`TUNING.md` §6.1).
- **[LOCKED] Stats (`TUNING.md` §6):** melee **12/hit**, **12 connecting hits**, **reflect window 0.20 s**.
- **[LOCKED] What it reflects (complete list):** thrown heads, head-grenades, boomerang-gun shots,
  arm-ripper/pistol rounds, shuriken, **AA rocks** (arced, slow — reflectable), and the **Helicopter's thrown
  heads** — any **telegraphed slow-to-medium single projectile.** It **CANNOT** reflect: the **gatling/barrage
  stream** (too dense), the **Tank MG stream** (also a dense hitscan stream, not a discrete projectile), the
  **enemy Sniper's hitscan apex shot** (instant, no travel), or **melee.** A reflected shot deals the **original
  attacker's damage** back.

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
  fire cadence within a pass **0.15 s/shot**, **throw cooldown 1 s**.
- **[LOCKED] Orbit path:** a **horizontal oval loop ~5 wu wide × ~3 wu deep**, extending **forward from the
  throw point** in the direction you're facing, traveled **clockwise**; it **auto-fires at any enemy inside the
  loop** as it sweeps, then **returns to hand after one full loop (~2.5 s)**. You **aim it by where you stand
  and which way you face** when you throw — not homing.
- **[LOCKED] Shot down:** the airborne gun is a small object with **no HP — a single enemy hit destroys it**.
  **Only enemies that fire while it crosses their line** can hit it (Arm-Ripper rounds, AA rocks, thrown heads,
  shuriken, the Helicopter's heads); melee enemies can't reach it. Destroyed = you **lose it + the remaining
  bullets** (§above). It's the risk of throwing it into a firing line.

### 3.8b Rocket Launcher — *world-pickup heavy* **[LOCKED core]**
- A **big-hitting explosive** weapon. **[LOCKED] Comes from a specific world PICKUP**, not from a corpse —
  it **appears along the way** (often when you're up against **bigger enemies**) for a burst of extra
  firepower. First of the **non-enemy weapon source** (§1).
- Distinct from the **Monkey Merc rocket launchers** (§3.7) — this one is the player's own.
- **[LOCKED] Stats & placement (`TUNING.md` §6/§6.1):** **3 rockets**, blast **70 (r 3 wu)**, **self-damage 35**
  (like the grenade), **warm-up 0.50 s**, **`E` free-fire** (not finisher-gated — it's a fired weapon). **The
  blast knocks down every caught regular enemy in the r 3 wu (standard 1.2 s knockdown; bosses/minibosses/
  H-weight immune, `TUNING.md` §2.6)**, same as the grenade.
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
| **T3 rare** | high/elite enemies | shotgun, gatling gun | powerful, scarcer |
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
