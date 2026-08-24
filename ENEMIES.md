# this.l — Enemy Roster

> **Scope:** the stick-figure enemies — identity, level system, per-type behavior, how they route/attack,
> what they drop, and per-enemy **asset needs**. Boss designs live in `BOSSES.md`; enemy *routing rules*
> extend `GAMEPLAY_LOOP.md` §8.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[ITERATE]** flesh out next · **[LATER]** parked.

---

## 1. Enemy system rules

- **[LOCKED] Most enemies share ONE stick-figure body — their *ability* defines them, not their looks.**
  The majority are the same thin silhouette; what makes a "type" is its **moveset/ability**, not a new
  character. **Huge asset win:** one base body + rig, with **ability-specific attack sets layered on top.**
  A few signature enemies get unique bits, but the default is *shared-body-plus-ability*.
- **[LOCKED] Enemies have a *level*.** An enemy's level sets its **HP, damage, weight, and loot tier** —
  higher level = tougher and drops rarer/longer-lasting weapons (`WEAPONS.md` §4).
- **[LOCKED] Weight matters** (from `PLAYER.md`): light/medium enemies **stagger** to a dash attack;
  **heavy** ones **floor the player** instead. Weight scales with level/type.
- **[LOCKED] Loot on death:** most enemies can drop a **weapon** (random but **constrained per stage** —
  early stages hand out early weapons) and/or a **coin** (1¢, currency `WEAPONS.md` §3.9). **Coin drop rate
  ≈ 5% per tier** (scales with tier); **Swarmers drop 0** (fodder never feeds the economy). **Sniper-special
  kills drop nothing.**
- **[LOCKED] Types are introduced progressively by stage.** Stage 1 is mostly **basic melee**; new types
  unlock as you climb (e.g. **grenade / Head-Throwers arrive ~stage 2**), so difficulty ramps by *roster*,
  not just numbers. *(Specific per-stage rosters get defined later, alongside stages.)*
- **[LOCKED] Catch-up minibosses:** if the player is **clearing too fast**, a **miniboss** is injected to
  re-apply pressure (dynamic pacing). *(Miniboss designs → `BOSSES.md`; the spawn trigger is a stage/pacing
  rule.)*
- **[PROPOSED] Diegetic weapon sources** (head-gone → sword, spine → shotgun) stay as **flavor on top of**
  the per-stage random pool — a themed corpse read, not a guaranteed type→weapon lock.
- **Routing** follows `GAMEPLAY_LOOP.md` §8: up to **8 pursuers**, hard separation (no stacked multi-hits),
  standoff rings, Z-spread. Per-type specifics below.
- **[LOCKED] Ability tier rule:** an enemy that uses an ability **on another enemy** can only target a unit
  **at least one tier below it** (a tier-2 can weaponize a tier-1, never another tier-2). Enemies **loot
  each other** the same way the player loots corpses. **Tiers span 0..N** — a tier-3 can spend **2×tier-1
  or 1×tier-2**; **tier-0** units are the lowest fodder.
- **[DATA] Attack windups (telegraph), collected as we spec:** regular melee **~100ms**, sword
  **~150–200ms**, **ground smash ~1000ms** (slight variance). Convention: **more reach/damage → longer,
  more readable windup.**
- **[LOCKED] Enemy ranged is short-range (no sniping).** Enemy guns/projectiles only connect from **close
  range**, so shooters must **close in** — which keeps every threat **dodgeable** by move/dash/jump. The
  bullet-hell pressure comes from *many close shooters*, never from off-screen snipers.
- **[LOCKED] Some enemies are outside the tier system** — special units (e.g. summoners like the Monkey
  Tamer) that don't rank up/down and don't feed the ability-tier rule; they're **priority/utility** threats
  judged on their own terms.
- **[LOCKED] No "hiding," no cheap frustration.** No enemy should turtle, camp, or evade in a way that
  feels cheap — every threat stays **approachable and fairly killable**, and every attack stays fairly
  dodgeable. Evasive kits (Ninja teleport, Snapper/Tamer keep-away, Burly's can't-be-picked-off) must
  always leave the player a clear, fair way in.
- **[LOCKED] Sky / flying enemies are their own category** — they occupy the air and **do not count toward
  grounded-enemy tallies** (e.g. the Flying Monkey's "<2 grounded enemies" trigger, §2.15). Air and ground
  pressure are balanced separately. Together with the **Sniper** (§2.14), sky enemies exist to keep the
  player's jump honest — neither always-safe nor always-punished.

---

## 2. Known / locked enemies (surfaced during weapon design)

### 2.1 Grenade Enemy ("Head-Thrower") — **[LOCKED core]**
- **[LOCKED]** **Pulls off its own head and throws it at the player as a grenade.**
- **[LOCKED] Fire interaction:** if set alight (fire staff), it **starts blinking, then after ~2s BOOM** —
  a small blast that **kills the player** if caught in it (turns it into a walking bomb).
- **[ITERATE]** throw arc/telegraph, does the thrown head follow grenade §3.2 physics, cooldown, does it
  die when it throws its head or grow a new one, its HP/level range.

### 2.2 Monkey Stick Figure — **[LOCKED core]**
- **[LOCKED]** Drops the **Monkey Merc** summon, claimable only if the player holds a **dime** (`WEAPONS.md`
  §3.7).
- **[ITERATE]** does it fight (and how) before dying, rarity, whether it flees.

### 2.3 Regular Melee — **[LOCKED core]** (the stage-1 staple)
- The plain stick figure — shared base body, no special ability.
- **Tries to close on the player**, then attacks with **punch**, **jump kick**, and **slide kick** (a low,
  gap-closing approach).
- Fills role **A (basic melee)**. Windup **~100ms**. **[ITERATE]** which attack it picks at which range,
  damage, HP, approach speed, how aggressively it mixes the three.

### 2.4 Snapper (Sword-Maker) — **[LOCKED core]** *(name TBD)* — **Tier 2**
- **Ability:** grabs a **tier-1** enemy and **"snaps" them like a whip, turning them into a sword**, then
  **swordfights the player** with **much longer reach** than fists.
- **Tier rule (§1):** can only snap a **tier-1** — never another tier-2.
- **Telegraph:** sword swings **wind up ~150–200ms** (vs. ~100ms fists) — long reach, but **readable and
  punishable.**
- Fills a **melee-zoner** niche (reach + threat that forces you to respect spacing).
- **[LOCKED] No tier-1 available:** he **stays away from the player and calls in tier-1 enemies** to snap —
  a keep-away support that manufactures its own weapon supply.
- **[ITERATE]** does his sword decay; does killing him **drop the sword** for the player; call-in cooldown /
  how many he can summon.

### 2.5 Arm-Ripper (Dual Pistols) — **[LOCKED core]** *(name TBD)* — **Tier 2a**
- **Ability:** rips the **arms off a tier-1 Regular Melee** and **dual-wields them as pistols.**
- **The disarmed tier-1** is left as a **headbutt-only enemy** (no arms → just lunges to headbutt you) — an
  emergent enemy state, not a separate character.
- **Short-range guns (§1):** his pistols only hit from **close range**, so he must **close in** — his
  approach is your window to **dodge / dash / jump** out of the line.
- **Tier rule (§1):** only disarms a **tier-1.**
- Fills the **gunner** role (a close-range, approachable one). **[ITERATE]** fire cadence, does he run dry /
  reload, what he does with no tier-1 to disarm (call one in like the Snapper?), does killing him drop
  pistols for the player.

### 2.6 Monkey Tamer — **[LOCKED core]** — **outside the tier system**
- **Summoner:** uses a **whistle** to **drag in enemy Monkey Mercs** — **melee-only** (distinct from the
  *player's* gun-toting Monkey Merc, `WEAPONS.md` §3.7). Spawns **up to 2 at a time**, **infinitely
  respawning.**
- **[LOCKED] Kill him and it all stops:** his monkeys **deactivate immediately on his death** — so **he's
  the priority target**, not the monkeys.
- **Low mobility:** **less mobile than the player**, so you can outmaneuver the swarm to reach him.
- **[LOCKED] Outside the tier system** — not tier-1/2; a special summoner unit.
- **[ITERATE]** whistle telegraph/cooldown, monkey HP & attacks, respawn delay, what he does if you corner
  him (fight or flee?).

### 2.7 Gatling Gunner — **[LOCKED core]** — **Tier 3**
- **Ability:** grabs **2 tier-1s OR 1 tier-2** and, over **~2 seconds**, **contorts them into a gatling
  gun** — a clean showcase of the ability tier rule (a tier-3 spends units below it).
- **Fire pattern:** **1-second bursts every 2–3 seconds** — rhythmic windows to close or dodge between bursts.
- **Close-range switch:** if the **player gets within pistol range**, it **drops to melee** instead of firing.
- **[ITERATE]** the ~2s contort telegraph & how vulnerable he is during it, burst spread/damage, what he
  does with no valid fodder (call in like the Snapper?), does killing him drop a gatling for the player.

### 2.8 Zombie — **[LOCKED core]** — **Tier 0** (new lowest tier)
- **Headshots don't kill it — they hollow it.** A headshot **empties the head** (filled head → see-through
  outline) but the zombie **keeps marching.**
- **Slow march** straight at the player.
- **Grab at close range:** it can **grab the player** (no bite — no mouth); the player **mashes/taps to
  break free.** A tempo trap, not burst damage.
- **[LOCKED] How zombies die:** **regular body damage** kills them, or they **time out.**
  - **Headshot-created zombies last ~10 seconds**, then drop on their own.
  - **Pod-spawned zombies** die to **any 3-hit combo finisher** (a small finisher is enough).
- **[LOCKED] Sources:**
  1. **Created by a headshot** — a headshot **hollows the head and spawns a ~10s zombie** instead of a
     clean kill.
  2. **Pods** — a **spawner** that pumps out zombies. **[ITERATE]** what a Pod is (destroyable spawner?
     count/rate, where it sits).
- **[LOCKED — headshot economy]** Any **headshot-kill weapon** — **pistol/revolver** head-lineups and the
  **gatling** auto-kill finisher — has a **~10% chance to spawn a ~10s zombie instead of killing**: a
  small, ever-present downside to leaning on headshots. **The sniper special is exempt — it *always*
  cleanly kills** (ricochet headshots never spawn zombies).

### 2.9 Ninja — **[LOCKED core]** — **Tier 3a**
- **Teleport:** uses **smoke bombs to "teleport"** (blink/reposition) — hard to pin down, flits around the
  player.
- **Ninja stars:** throws **shuriken** — but **stars must be made by stripping legs/arms off lower-tier
  enemies** (the cannibalize pattern; per the tier rule a 3a spends tier-1/2). Its **ammo = harvested limbs.**
- **[ITERATE]** teleport cooldown & telegraph (smoke-puff tell), stars per limb, do stars obey the
  short-range rule (§1) or count as a telegraphed thrown exception, star damage, what it does with no
  fodder to strip (call in / melee?).

### 2.10 Anti-Aircraft — **[LOCKED core]** — **Tier 1a**
- A **basic enemy that throws rocks** at the player (a ranged lobber).
- **[LOCKED] Boomerang distraction (counterplay):** if the player **throws a boomerang**, the AA enemy
  **actively throws rocks at the boomerang** (~**20% accuracy** — mostly whiffs), which **distracts it**,
  opening a window to attack. Bait it with a throw.
- **[ITERATE]** do rocks obey the short-range rule (§1) or arc in from farther; do **other airborne things**
  (boomerang gun, thrown grenade, ninja stars) also bait it; rock damage & telegraph; throw cadence.

### 2.11 Heavy ("Bold" / Burly) — **[LOCKED core]** — **outside the tier system**
- A **BOLD, burly** stick figure — visibly thicker/heavier than the rest (the one place the silhouette bulks up).
- **High HP / tanky.** **[LOCKED] Heavy weight:** **dash-attacking him floors the player** (`PLAYER.md`
  weight rule), punishing lazy dashes.
- **[LOCKED] Can't be "picked off":** ranged pick tools **can't eliminate him from afar** — the **sniper
  special's ricochet skips / can't kill him**, and headshot-pick tools won't drop him. You must **engage
  him directly.**
- **[LOCKED] Only 2 spawn at a time**, and they **never flank — they approach as directly as possible**, so
  the **player always keeps 2 escape routes** around them (deliberate anti-corner design).
- **[LOCKED] Extended-reach punch** — a punch **like the player's**, with **longer reach** than normal
  enemies, so you can't poke him safely from your usual spacing.
- **[LOCKED] Outside the tier system.** Fills the **Heavy / bruiser** role.
- **[ITERATE]** exact HP, punch windup/telegraph, whether his punch emits an air-gust like the player's,
  what he drops.

### 2.12 Swarmer — **[LOCKED core]** — **Tier 1b** — fast swarm (fills the gap)
- **Half-sized** stick figures — small, **weak**, and **many.**
- **Spawn in larger pods** and **appear on multiple sides at once**, so they **surround** and pressure the
  player from several angles — a **positioning** threat, not a damage one.
- Prime **sniper-ricochet fodder**; great for punishing greedy looting.
- **Pods** are the shared spawner (also spawns Zombies, §2.8) — **[ITERATE]** promote *Pod* to its own
  spec (destroyable? spawn rate/size, where it sits).
- **[ITERATE]** do swarmers count against the 8-pursuer cap (`GAMEPLAY_LOOP.md` §8.2) or exceed it as a
  special swarm; their attack (contact / tiny melee); pod size; move speed.

### 2.13 Ground Smasher (the Zoner) — **[LOCKED core]** *(tier TBD)*
- Carries a **large club on its shoulder** and **slowly walks toward the player**, smashing the ground
  **every 3–5 seconds.** Each smash: club **overhead ~1s** (telegraph), then a **shockwave straight down
  its lane** (its Z-row).
- **[LOCKED] Only one ground smash at a time** — across the whole field **at most one shockwave is ever
  active**, so you're never buried under overlapping lane-denials (a fairness cap, per the no-cheap rule).
- **Counterplay:** slow approach + 1s windup + lane-limited shockwave = **step out of the lane** (change
  depth) to dodge. Rewards Z-movement; never a cheap hit.
- Fills the **zoner** role — its lane shockwaves are the "thread the space" pressure (in place of
  projectile patterns).
- **Telegraph data:** ground smash **~1000ms**.
- **[ITERATE]** shockwave speed/range/damage, knockdown, club as a possible player drop, tier, HP, whether
  the one-smash cap counts only Ground Smashers or all shockwaves.

### 2.14 Sniper — **[LOCKED]** — anti-jump *(tier TBD)* — **debuts Area 3 (causeway)**
- **Look:** a stick figure **with a beret and a large sniper rifle** — a clear silhouette so you can instantly
  tell who he is. **[LOCKED] Jumping is punishable, not disabled** — the instant the player
  jumps, a **red dot paints the player's head** (clear telegraph). If the player **rides the jump to its
  apex**, the Sniper **smacks them out of the sky:** health **drops to ~20%**, and if the player was
  **already under 25%, it's a kill.**
- **Counterplay:** the red dot is fair warning — **don't ride a jump to apex** while he's up (short hops /
  bail early), **stay grounded**, or **rush him down.** A hard read, never a cheap off-screen hit.
- **[LOCKED] Deliberate exception to the short-range-gun rule (§1)** — the one ranged enemy, gated to the
  predictable jump apex.
- **[ITERATE]** does a partial/short hop stay safe (apex-only?); can he hit a grounded player at all; red-dot
  → shot timing; reposition behavior; HP/tier; how many at once.

### 2.15 Flying Monkey — **[LOCKED core]** — sky harasser *(tier TBD)*
- **Melee only**, **airborne.** **[LOCKED] Holds off** and **only swoops to attack when there are fewer than
  2 *grounded* enemies on screen** — it waits its turn instead of piling on.
- **[LOCKED] Sky enemies don't count** toward that tally (only **grounded** enemies do) — otherwise flying
  monkeys would count each other and never come down.
- Gives **late-fight air pressure** as the ground clears and a reason to use **jumps / air attacks** — in
  direct tension with the **Sniper** (who punishes jumping), so the two create a push-pull.
- **[ITERATE]** swoop/attack pattern, HP, how many at once, exactly how you hit it (air attacks; and does
  jumping to reach it expose you to a Sniper), tier.

---

## 3. Roster — **[PROPOSED baseline] + your named enemies**

> The **specific characters get defined later** (your call — you have your own enemy ideas). Below is a
> **role-coverage baseline** so encounter design has each function filled; we fold **your named enemies**
> onto these roles the way we did weapons.

| # | Role | Behavior sketch | Notes |
|---|---|---|---|
| A | **Basic melee** | walks in, throws punches; the bread-and-butter body (stage 1 staple) | drops early pool |
| B | **Gunner** | holds a standoff, fires straight/aimed shots | the bullet-dodging layer |
| C | **Zoner / Patterner** | stationary-ish, emits a fixed bullet pattern | thread-the-needle |
| D | **Heavy** | slow, high-HP, **floors your dash attack**; best loot | a sniper target |
| E | **Swarm** | weak, fast, many | sniper-ricochet fodder |
| F | **Head-Thrower** (§2.1) | self-decapitating grenade lobber | fire = walking bomb |
| G | **Monkey** (§2.2) | drops the merc | needs a dime |
| H | **Wallet Runner** | flees rather than fights; drops ¢ if caught | feeds the dime economy |

**[PROPOSED]** these are **roles to fill, not final characters** — each realized by one or more of your
named enemy types.

---

## 4. Rank (level) system — **[LOCKED approach], [LATER] specifics**

- **[LOCKED] Ranks are subtle.** A higher-rank enemy looks almost identical — the tell is a **small marker
  (e.g. a colored wristband)**, not a bigger body or new props. Reads as "same guy, tougher."
- Each rank up = more **HP / damage / weight** and a better loot roll within the stage's constrained pool.
- **[LATER]** how many ranks, the color code, stat curves, which ranks appear where.

---

## 5. Per-enemy asset needs → feeds `ASSET_MANIFEST.md`

For **each enemy type**: idle · walk (mirror L/R) · attack(s) · hurt/stagger · **death** (and its
**corpse/part drop** — headless body, ejectable spine, etc.) · any projectile/telegraph VFX. Plus **level
variants** (recolor + prop overlays). Special enemies add signature anims (head-throw, blink-and-explode,
wallet-drop, monkey flair).

---

## 6. Status & next step — **ROSTER COMPLETE (v1)** — all roles filled, open to additions

**Locked system rules:** shared-body-plus-ability; ability tier rule (target ≥1 tier below; enemies loot
each other; tiers span 0..N); telegraph timing (100ms fists / 150–200ms sword); **enemy guns short-range/
dodgeable**; outside-the-tier specials; progressive type-introduction by stage; per-stage constrained
random loot; catch-up minibosses; subtle wristband ranks; **headshot economy** (~10% zombify, sniper exempt).

**Defined enemies (10):**
| Enemy | Tier | Role it fills |
|---|---|---|
| Zombie | T0 | grab / tempo-trap; headshot-resistant |
| Regular Melee | T1 | basic melee (punch/jump-kick/slide-kick) |
| Anti-Aircraft | T1a | rock lobber; boomerang-baitable |
| Snapper (Sword-Maker) | T2 | melee-zoner (snaps a T1 → sword) |
| Arm-Ripper | T2a | close gunner (T1 arms → dual pistols) + Headbutt state |
| Gatling Gunner | T3 | suppression (2×T1 / 1×T2 → gatling) |
| Ninja | T3a | mobile harasser (teleport + limb-shuriken) |
| Monkey Tamer | untiered | summoner (melee monkeys, priority target) |
| Heavy ("Bold"/Burly) | untiered | **bruiser** — tanky, extended-reach punch, floors your dash, **can't be picked off** |
| Swarmer | T1b | **fast swarm** — half-size, weak, pod-spawned on multiple sides |
| Ground Smasher (Zoner) | TBD | **zoner** — slow approach, club→**lane shockwave** every 3–5s, one at a time |
| Sniper | TBD | **anti-jump** — red dot on jump; ride to apex → shot to 20% HP (dead if <25%) |
| Flying Monkey | TBD | **sky harasser** — melee, swoops only when <2 grounded enemies |
| Head-Thrower | — | self-decapitating grenade; fire→walking bomb |
| Monkey | — | economy (drops the player's Merc, needs dime) |

**Open role gaps to fill on resume:**
- ~~Bullet-pattern zoner~~ — ✅ filled by **Ground Smasher** (lane shockwaves instead of projectile patterns).
- ~~Heavy / bruiser~~ — ✅ filled by **Heavy ("Bold"/Burly)**.
- ~~Fast swarm~~ — ✅ filled by **Swarmer**.
- ~~Wallet Runner~~ — ✅ **cut**; enemies drop **coins** directly (~5%/tier, 0 for Swarmers) instead.

**All combat roles filled, including air/anti-air** (Sniper checks jump-spam; Flying Monkeys reward air).
Roster at 14 types for v1, open to additions anytime.

**Also parked:** the many per-enemy `[ITERATE]` details (fallbacks, cooldowns, drops), rank specifics,
and per-stage rosters (with the stage designs).

**Next — capture your named enemies.** You have specific enemy ideas; dump them and we'll (a) spec each one
§2-style, and (b) map it onto the role baseline so we know coverage. Specific per-stage rosters and rank
details come later, alongside the stage designs.
