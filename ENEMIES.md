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
  ≈ 12%** per non-swarm kill (raised from 5% so dimes are reachable); **Swarmers drop 0**. **Sniper-special
  kills drop nothing.**
- **[LOCKED] Types are introduced progressively by stage.** Stage 1 is mostly **basic melee**; new types
  unlock as you climb (e.g. **Head-Throwers arrive Area 2 (airport)**), so difficulty ramps by *roster*,
  not just numbers. *(Specific per-stage rosters get defined later, alongside stages.)*
- **[LOCKED] Catch-up minibosses:** if the player is **clearing too fast**, a **miniboss** is injected to
  re-apply pressure (dynamic pacing). *(Miniboss designs → `BOSSES.md`; the spawn trigger is a stage/pacing
  rule.)*
- **[PROPOSED] Diegetic weapon sources** (head-gone → sword, spine → shotgun) stay as **flavor on top of**
  the per-stage random pool — a themed corpse read, not a guaranteed type→weapon lock.
- **Routing** follows `GAMEPLAY_LOOP.md` §8: up to **8 pursuers**, hard separation (no stacked multi-hits),
  standoff rings, Z-spread. Per-type specifics below.
- **[LOCKED] Ability tier rule:** an enemy that uses an ability **on another enemy** can only target a unit
  **at least one tier below it** (a tier-2 can weaponize a tier-1, never another tier-2). **"Enemies loot each
  other" is a runtime system for exactly the enemies whose ability IS that** — the **Snapper** (snaps a T1 →
  sword) and the **Arm-Ripper** (rips a T1's arms → pistols); it is **not** a generic loot-pickup behavior on
  every enemy. **Tiers span 0..N** — a tier-3 can spend **2×tier-1 or 1×tier-2**; **tier-0** units are the
  lowest fodder.
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

> **⚠ [RESOLVED-ELSEWHERE BANNER] Every per-enemy `[ITERATE]` below is now PINNED** — read the concrete values
> in **`TUNING.md` §4** (HP · damage · speed · weight · per-enemy timings), **`TUNING.md` §4.1** (AI edge-case
> resolutions), **`TUNING.md` §6.3** (projectile speeds), and **`ENCOUNTERS.md`** (per-stage rosters). The
> `[ITERATE]` tags in §2.1–§2.18 are **historical design notes, not open questions** — the numbers doc
> supersedes them. (E.g. Regular Melee's attack-selection: **closes to ≤1.0 wu → punch; from 1–4 wu →
> slide-kick; airborne player → jump-kick**; Snapper sword-drop, Head-Thrower regrow, Arm-Ripper reload — all in
> §4/§4.1.)

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

### 2.5 Arm-Ripper (Dual Pistols) — **[LOCKED core]** *(name TBD)* — **Tier 2a** — **debuts Dixon (big-version = Dixon boss)**
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

### 2.7 Gatling Gunner — **[LOCKED core]** — **Tier 3** — **debuts Area 4**
- **Ability:** grabs **2 tier-1s OR 1 tier-2** and, over **~2 seconds**, **contorts them into a gatling
  gun** — a clean showcase of the ability tier rule (a tier-3 spends units below it).
- **Fire pattern:** **1-second bursts every 2.5 s** — rhythmic windows to close or dodge between bursts
  (`TUNING.md` §4 row 15).
- **Close-range switch:** if the **player gets within 3 wu**, it **drops to melee (22.5)** instead of firing
  (`TUNING.md` §4 row 15 — the pinned melee-switch distance).
- **[RESOLVED, see `TUNING.md`]** contort telegraph **2.0 s** (vulnerable throughout), burst = the 1/hit stream
  (`TUNING.md` §4 row 15 / §6.2); **no valid fodder → he spawns already holding the gatling** (self-sufficient,
  `TUNING.md` §4.1 — never a spawn dependency).
- **[LOCKED] Guaranteed Gatling drop (dual-channel, like the Snapper's sword / Arm-Ripper's pistols):** because
  the Gatling Gunner **IS** his weapon, **killing him ALWAYS drops a Gatling** for the player (a 100% guaranteed
  drop, not the 26% T3 roll) — *plus* his normal §6 T3-band roll on top. This is the same "the enemy is the
  weapon → guaranteed drop" exception the Snapper and Arm-Ripper use (`TUNING.md` §6 note).

### 2.8 Zombie — **[LOCKED core]** — **Tier 0** (new lowest tier)
- **A Zombie's head is already hollow — it can't be head-killed.** The **hollow (see-through) head is the
  Zombie's permanent look**: a headshot-*created* Zombie is born with its head emptied by the shot that made it
  (`TUNING.md` §4 headshot economy), and it keeps that hollow head while it marches. **Shooting an existing
  Zombie does NOT re-hollow or head-kill it** — the head is inert, so every gun hit on a Zombie is a **body shot
  dealing normal damage** (LOCKED block below). You kill a Zombie by depleting its 30 body-HP or letting it time
  out, never by a headshot.
- **Slow march** straight at the player.
- **Grab at close range:** it can **grab the player** (no bite — no mouth); the player **mashes/taps to
  break free.** A tempo trap, not burst damage.
- **[LOCKED] How zombies die:** **regular body damage** kills them (30 HP, `TUNING.md` §4 row 1), or they
  **time out.**
  - **Headshot-created zombies last ~10 seconds**, then drop on their own.
  - **Pod-spawned zombies** die to **any combo finisher** (the 4th-hit finisher is enough).
- **[LOCKED] Guns vs. the Zombie (resolves the gun/head interaction):** because a Zombie's head is inert,
  **guns never headshot a Zombie** — every gun hit lands as a **body shot dealing its normal damage** to the
  30 HP: **pistol 12** (chips — 3 shots), **revolver 30** (one-shots the body), **gatling barrage** deals its
  **flat 45** (a Zombie is not a "standing regular auto-kill" target for zombify purposes — it's already a
  zombie, so **no zombify roll** ever fires on a Zombie). A gun shot on a *live enemy's* head is what may create
  a Zombie (headshot economy); a gun shot *on a Zombie* just damages its body. The **sniper special destroys a
  Zombie cleanly** (overrides the head-inert rule, `TUNING.md` §3.1).
- **[LOCKED] Sources:**
  1. **Created by a headshot** — a headshot **hollows the head and spawns a ~10s zombie** instead of a
     clean kill.
  2. **Pods** — a **spawner** that pumps out zombies. **[ITERATE]** what a Pod is (destroyable spawner?
     count/rate, where it sits).
- **[LOCKED — headshot economy]** Any **headshot-kill weapon** — **pistol/revolver** head-lineups and the
  **gatling** `E`-barrage auto-kill — has a **~10% chance to spawn a ~10s zombie instead of killing**: a
  small, ever-present downside to leaning on headshots. **The sniper special is exempt — it *always*
  cleanly kills** (ricochet headshots never spawn zombies).

### 2.9 Ninja — **[LOCKED core]** — **Tier 3a** — **debuts Area 4 (Vallejo)**
- **Teleport:** uses **smoke bombs to "teleport"** (blink/reposition) — hard to pin down, flits around the
  player.
- **Ninja stars:** throws **shuriken**. The "stripping legs/arms off lower-tier enemies" is **diegetic flavor
  for the animation, NOT a finite ammo model** — per `TUNING.md` §4/§4.1 the Ninja **throws 2 shuriken per
  volley on a 3 s cooldown, effectively unlimited** (self-restocks; it never runs dry and never needs adjacent
  fodder to throw). Spawns combat-ready.
- **[RESOLVED in `TUNING.md` §4/§4.1]** teleport cooldown **3 s** & smoke tell **0.3 s**, 2 shuriken/volley; do stars obey the
  short-range rule (§1) or count as a telegraphed thrown exception, star damage, what it does with no
  fodder to strip (call in / melee?).

### 2.10 Anti-Aircraft — **[LOCKED core]** — **Tier 1a** — **debuts Area 2 (Airport)**
- A **basic enemy that throws rocks** at the player (a ranged lobber).
- **[LOCKED] Boomerang distraction (counterplay):** if the player **throws a boomerang**, the AA enemy
  **actively throws rocks at the boomerang** (~**20% accuracy** — mostly whiffs), which **distracts it**,
  opening a window to attack. Bait it with a throw.
- **[LOCKED] On a hit (the ~20% case): the rock knocks the boomerang out of the air.** The struck boomerang
  **stops mid-flight and drops to the ground where it fell** (it does **not** return to hand) — the player must
  **walk over and re-loot it** (`WEAPONS.md` §2.3). So the bait carries a real risk: a lucky rock costs you the
  toy until you retrieve it. The **thrown Boomerang Gun** is likewise shot-downable by an AA rock while it
  crosses the rock's line (`WEAPONS.md` §3.8 shot-down rule) — that loses its remaining bullets.
- **[LOCKED] rocks arc in from up to 10 wu** (`TUNING.md` §1 reach table / §6.3), longer than the melee
  short-range rule; **other slow airborne things also bait it** (thrown grenade, ninja shuriken) on the same
  ~20% intercept. Rock damage **7.5**, arc telegraph **0.5 s**, throw cadence **2.5 s** (`TUNING.md` §4).

### 2.11 Heavy ("Bold" / Burly) — **[LOCKED core]** — **outside the tier system** — **debuts Area 4 (SF streets)**
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

### 2.13 Ground Smasher (the Zoner) — **[LOCKED core]** *(tier TBD)* — **debuts Area 4 (Golden Gate Bridge)**
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
  tell who he is.
- **[LOCKED] Only dangerous while scoped.** He shoots **only when actively looking down the scope.** He
  **periodically lowers the rifle for ~2 seconds** (and doesn't snap it back up) — a **safe window to jump** —
  then **raises it and scans** again. This rhythm makes him **fair, never cheap.**
- **[LOCKED] The punish:** while scoped, a jump **paints a red dot on the player's head**; **riding the jump
  to its apex** gets you **smacked out of the sky** — health **drops to ~20%** (a **kill** if you were already
  **under 25%**).
- **[LOCKED] Two answers:** be **patient** — jump during his ~2s rifle-down windows; or **rush him down** and
  kill him before he re-scopes/escapes.
- **[LOCKED] Kill reward:** killing the Sniper **drops a sniper rifle** that grants **+100 meter-points = one
  full tier fill** (`TUNING.md` §2.4) — downing him hands you **one tier** of Special, not necessarily a full
  green bar.
  - **[LOCKED] Pickup mechanics:** the rifle is a **ground pickup, claimed by walking over it** (automatic, no
    `F`, no dime) — it is a **meter item, not a held weapon** (it never occupies your weapon slot; it just adds
    the +100 and vanishes). It **persists 12 s** on the ground then despawns (the standard pickup lifetime,
    `TUNING.md` §6). If the meter is **already at green (max)** when claimed, the +100 is **discarded** (overfill
    cap, §2.4). It drops from **any** Sniper kill (fists, weapon, hazard) — this is a fixed drop, not a % roll.
- **[LOCKED] Exception to the short-range-gun rule (§1)** — the one ranged enemy, gated to the predictable
  apex + the scope rhythm.
- **[ITERATE]** can he hit a grounded player at all; reposition/escape behavior; HP/tier; how many at once.

### 2.15 Flying Monkey — **[LOCKED core]** — sky harasser *(tier TBD)* — **debuts Area 3 (causeway)**
- **Melee only**, **airborne.** **[LOCKED] Holds off** and **only swoops to attack when there are fewer than
  2 *grounded* enemies on screen** — it waits its turn instead of piling on.
- **[LOCKED] Sky enemies don't count** toward that tally (only **grounded** enemies do) — otherwise flying
  monkeys would count each other and never come down.
- Gives **late-fight air pressure** as the ground clears and a reason to use **jumps / air attacks** — in
  direct tension with the **Sniper** (who punishes jumping), so the two create a push-pull.
- **[ITERATE]** swoop/attack pattern, HP, how many at once, exactly how you hit it (air attacks; and does
  jumping to reach it expose you to a Sniper), tier.

### 2.16 Pickpocket — **[LOCKED core]** — economy enemy — **debuts Area 4 (Vallejo)**
- A **smaller, differently-colored** stick figure that **darts up and steals all the coins in your wallet**
  (currency, `WEAPONS.md` §3.9).
- **[LOCKED] Risk/reward:** **kill it and you get DOUBLE the coins back.** So you can **let it rob you, then
  chase it down for 2× your money**, or **kill it first** to avoid losing anything — your call.
- Fills the **economy-interaction** role (the opposite of a coin drop). **[ITERATE]** speed/escape behavior,
  does it flee after stealing, does it drop the doubled coins on death or credit directly, HP.

### 2.17 Boomergunner — **[LOCKED core]** — **debuts Area 4 (Marin/redwoods)**
- Enemies who **wield Boomerang Guns** (`WEAPONS.md` §3.8) — they **throw the boomerang gun at you**; it
  **orbits auto-firing and returns** to them. Ranged pressure from a spinning, circling threat.
- **[LOCKED] Marin/redwoods caps with a Boomergunner boss** (a **big-version**, `BOSSES.md` §1).
- **[ITERATE]** how many they field, can you catch/steal the gun, tier, behavior vs. the player's version.

---

## 3. Roster — **[SUPERSEDED baseline]** (kept for history)

> ⚠ **SUPERSEDED by §2/§6 (the real 17-enemy roster).** This early role-coverage sketch predates the final
> roster and still mentions a standoff "Gunner," a stationary "Patterner," and a "Wallet Runner" that the
> locked rules replaced (short-range guns; Ground-Smasher zoner; direct coin drops). **Ignore for build —
> §6 is authoritative.**

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

## 4. Rank (level) system — **[CUT for v1]**

- **[CUT]** The separate "subtle rank / wristband" sub-tier is **removed from v1** — it overlapped the **tier
  ladder** (T0–T3, which already scales HP/damage/weight/loot by *type* across areas) and had no encounter
  hooks. The campaign's difficulty ramp is delivered by **(a) the tier ladder** (roster introduced by area),
  **(b) spawn density + the difficulty modes** (`TUNING.md` §8.4), and **(c) Endless's per-minute stat-creep**
  (`TUNING.md` §8.3). No per-enemy rank stat, no wristband marker, no rank asset. *(A future "+more enemies"
  update could reintroduce ranks; not v1.)*

### 4b. Damage model — **[LOCKED]**
- **Player HP = 100.**
- **[LOCKED] Enemy damage = tier × 7.5** (out of 100): **T1 = 7.5 · T2 = 15 · T3 = 22.5 · T4 = 30** (the 30 cap).
- **Exceptions:** **Swarm** ~**1–2** (chip); **Zombie (T0)** deals **no hit damage** (grab only); **Gatling**
  **1 HP/hit @ 25 hits/s = 25 dmg/s** → **~4 s of continuous fire to down a 100-HP player** (LOCKED, `TUNING.md`
  §4 row 15); cover/closing is mandatory.
- Ties: the **Sniper** apex shot drops you to **~20 HP** (kill if already <25); **grenade self-damage** and
  **fall-off instant death** are separate one-offs.
- **[LOCKED] Healing = random small drops.** Enemies occasionally drop a **small health pickup** (~**5%**
  drop). **At low HP the game rubber-bands:** the **health-drop rate jumps to ~20%**, and **all other drop
  rates double** (coins, weapons) — a built-in mercy that never fully bails you out.
- **[ITERATE]** player/**human weapon** damage & durability (pin later); boss HP; exact "low HP" threshold.

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
random loot; catch-up minibosses; **headshot economy** (~10% zombify, sniper exempt).

**Defined enemies (17) — all placed with a debut area:**
| Enemy | Tier | Debuts | Role it fills |
|---|---|---|---|
| Zombie | T0 | Area 1 (mall) | grab / tempo-trap; headshot-resistant |
| Regular Melee | T1 | Area 1 (suburbs) | basic melee (punch/jump-kick/slide-kick) |
| Swarmer | T1b | Area 1 (mall) | fast swarm — half-size, pod-spawned |
| Anti-Aircraft | T1a | Area 2 (airport) | rock/stone lobber; boomerang-baitable |
| Head-Thrower | — | Area 2 (airport) | self-decapitating grenade; fire→walking bomb |
| Snapper (Sword-Maker) | T2 | Area 2 (Sacramento) | melee-zoner (snaps a T1 → sword) |
| Sniper | TBD | Area 3 (causeway) | anti-jump; scope rhythm; kill → free special |
| Flying Monkey | TBD | Area 3 (causeway) | sky harasser; swoops when <2 grounded |
| Monkey Tamer | untiered | Area 3 | summoner (melee monkeys, priority target) |
| Monkey | — | Area 3 | economy (drops player's Merc, needs dime) |
| Arm-Ripper | T2a | Area 3 (Dixon) | close gunner (arms → pistols); big-ver = Dixon boss |
| Ninja | T3a | Area 4 (Vallejo) | mobile harasser (teleport + limb-shuriken) |
| Pickpocket | — | Area 4 (Vallejo) | economy — steals coins; kill for 2× back |
| Boomergunner | — | Area 4 (Marin) | throws Boomerang Guns; big-ver = Marin boss |
| Gatling Gunner | T3 | Area 4 | suppression (2×T1 / 1×T2 → gatling) |
| Ground Smasher (Zoner) | TBD | Area 4 (Golden Gate) | zoner — lane shockwaves |
| Heavy ("Bold"/Burly) | untiered | Area 4 (SF streets) | bruiser — floors dash, can't be picked off |

**All combat roles filled** (melee, gunner, zoner, heavy, swarm, air/anti-air, summoner, economy), **and every
enemy has a debut area.** Economy enemies (Monkey, Monkey Tamer, Pickpocket) are second-half only.

**Extensible by design:** the tier ladder, cannibalize rule, big-version boss scaling, and vignette teaching
all scale — so new enemies can **extend this game or seed a harder sequel / "more enemies" follow-up** without
reworking the systems.
**Roster = 17 types** for v1 (see the §6 table), open to additions anytime. *(The "Bat enemy" seen in the
airport vignette is a **vignette-only demo actor** — just an enemy holding a bat to show off the reflect —
**not** a rostered type; it never appears outside that Sacramento/airport bit.)*

**[LOCKED — SUPERSEDES the per-enemy `[ITERATE]` markers above]:** every per-enemy **timing, cooldown, HP,
damage, speed, weight, and fallback** is now pinned authoritatively in **`TUNING.md` §4** (the 17-enemy stat
table) and **`TUNING.md` §4.1** (the AI edge-case resolutions: no-fodder Arm-Ripper/Gatling, Monkey stats,
Pickpocket escape, Boomergunner catch, Head-Thrower head physics, Sniper-vs-grounded, Flying-Monkey gating,
8-cap overflow). Per-stage rosters/counts are in **`ENCOUNTERS.md`**. Read those as the resolution of any
`[ITERATE]` left in §2 above. **Specific still-open flavor** not covered there:
- **Snapper's sword-drop (single model, LOCKED):** the Snapper **snaps a T1 INTO a sword and wields it
  himself** (§2.4 — the T1 is *consumed*, it becomes the blade, no separate killable unit remains). **Killing
  the Snapper drops that sword** for the player to grab (a normal Sword pickup), *plus* his §6.1 T2 pool roll.
  *(Supersedes the earlier "kill the armed T1" wording — under the wield-it-himself model there is no separate
  T1.)*
  - **[LOCKED] Dropped-sword durability = FULL 8 hits** (a fresh Sword pickup, `TUNING.md` §6). The Snapper's
    own "sword decays after 8 hits" (`TUNING.md` §4 row 6) is **his internal AI weapon state**, tracked
    separately from what he drops on death — the player always gets a full-durability Sword regardless of how
    many swings the Snapper took.
  - **[LOCKED] If the Snapper's own sword decays mid-fight** (he lands 8 swings), he **loses the blade and
    immediately re-snaps** — snapping an adjacent T1 if one is in range, else **calling one in** (his no-fodder
    behavior, §2.4 / `TUNING.md` §4 row 6: call-in every 4 s, max 2 pending). He never fights unarmed; the
    re-snap is his refresh loop.
- **Head-Thrower fire-boom AoE:** a staff-fire-lit Head-Thrower's explosion **also damages other enemies**
  within its blast (r 2 wu) — friendly fire among enemies (`WEAPONS.md` §3.5).
- **Untiered-elite drops (LOCKED, resolves the Heavy/Tamer "what he drops"):** the **Heavy and Monkey Tamer
  roll the T3 loot band (26%)**; the **Monkey and Pickpocket are economy enemies** and don't roll the weapon
  table (Monkey → Monkey Merc, Pickpocket → 2× coins) — pinned in `TUNING.md` §6 drop table.

**Roster is complete and locked.** Per-stage rosters live in `ENCOUNTERS.md`; all per-enemy numbers in
`TUNING.md` §4/§4.1. **AA bait rule (flavor):** the Anti-Aircraft targets the **player's thrown boomerang**
(its bait) and an **airborne/jumping player**; it does **not** waste shots on ambient birds/planes.
