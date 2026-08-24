# this.l — Per-Stage Encounter / Wave Tables

> **Purpose:** the concrete **spawn script for every stage** — which enemies, how many, in what order, at what
> cadence, where the checkpoint sits, and what gate advances the player. This is the doc the build reads to
> populate each level; nothing here is left to the builder's guess.
>
> **Reads from:** enemy stats/timings (`TUNING.md` §4), boss stats (`TUNING.md` §7 / `BOSSES.md`), stage list
> (`STAGES.md` §4), vignettes (`VIGNETTES.md`), areas/art (`AREAS.md`).
>
> **Legend:** **[LOCKED]** — all 12 stages scripted below. Counts are for **1-player normal**; multiplayer
> multiplies per `CHARACTERS.md` (2P ≈ ×2.5) and Endless overrides everything (`TUNING.md` §8.3).

---

## 0. Universal spawn rules — **[LOCKED]**

- **On-screen pursuer cap = 8** (`GAMEPLAY_LOOP.md`): never more than 8 enemies actively pursuing at once.
  Extra roster for a wave **queues** and streams in as pursuers die (hard-separation still applies). **Swarmer
  pods are the sole exception** — a pod's 5 swarmers may push the field past 8 briefly (`TUNING.md` §4).
- **A "wave" = a spawn batch.** The stage **gates** (invisible wall / camera lock) until the current wave's
  **kill-quota** is met, then scrolls to the next.
- **Spawn sides:** `L` left edge, `R` right edge, `B` back-Z, `A` ambush (door/window/manhole per area). Default
  is L/R along the lane.
- **Cadence:** enemies in a wave enter over the **stagger** time listed (e.g. "drip 0.8 s" = one every 0.8 s),
  not all at once, so the 8-cap breathes.
- **Checkpoint:** one **mid-stage checkpoint** per stage at the marked wave (`TUNING.md` §8.1). Boss arenas have
  a checkpoint **at the door** (retry the boss, not the stage).
- **Stage length target:** **~3–4 min** of combat per non-boss stage; boss stages add the **<2:00** fight
  (`TUNING.md` §7). Full run ≈ 3–4 hrs (`STAGES.md` §4).
- **Difficulty knob:** each stage lists a **Threat Budget** = sum of (enemy count × tier) as a sanity check the
  ramp climbs monotonically. It is a design guardrail, not a runtime value.

---

## ACT 1 — Placer Suburbs & Mall  ·  *tier 0–1 → intro tier-2*

### Stage 1 — Lincoln suburbs (opener) — **[LOCKED]**
- **Teaches:** the **punch only** (`VIGNETTES.md`: dancing Zebra punches a regular enemy). No weapons yet.
- **Pool:** Regular Melee (T1) only.
- **Hazard:** cars & school buses cross the lane (dodge; `STAGES.md` §4).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | Zebra + 1 Regular (scripted) | — | — | auto |
| 1 | 2 Regular | L | drip 1.0 s | kill 2 |
| 2 | 3 Regular | L, R | drip 0.9 s | kill 3 |
| **CHECKPOINT** | — | — | — | — |
| 3 | 4 Regular (1 bus pass mid-wave) | L, R | drip 0.8 s | kill 4 |
| 4 | 5 Regular | L, R, B | drip 0.8 s | kill 5 → exit |
- **Threat Budget:** 14. **No boss** (ramp stage). Ends by walking off-screen right to Rocklin.

### Stage 2 — Rocklin streets → Swarm & Zombie intro — **[LOCKED]**
- **Teaches:** **Swarmer pods** (positioning) and **Zombie grab** (the mall vignette actually plays at stage
  head here: guard shoots a T1 → zombifies → grabs → fall). Break-free mash is taught by the first grab.
- **Pool:** Regular (T1), Swarmer (T1b, in pods of 5), Zombie (T0, from a Pod), **Pod** (HP 50).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | guard→zombie→grab (scripted) | A | — | auto |
| 1 | 3 Regular | L, R | drip 0.9 s | kill 3 |
| 2 | 1 Pod (spits Swarmers, cap 6) + 2 Regular | B + L | pod drips 1/3 s | destroy Pod |
| **CHECKPOINT** | — | — | — | — |
| 3 | 1 Zombie + 3 Regular | A + L,R | drip 0.8 s | kill all (first grab happens here) |
| 4 | 2 Pods (1 Swarm, 1 Zombie) + 2 Regular | B, B + L | — | destroy both Pods + kill Regulars |
- **Threat Budget:** ~20. **No boss.** Exit right toward the Galleria.

### Stage 3 — Roseville Galleria mall + **Snapper** intro → **BOSS: Burly Macho Guy** — **[LOCKED]**
- **Teaches (mid-stage vignette):** **Snapper** makes a sword for a T1 (the weapon-gate primer; `BOSSES.md`
  arena adds spawn only the needed weapon). Cowering shoppers as set-dressing (`AREAS.md`).
- **Pool:** Regular (T1), Swarmer pods, Snapper (T2, first tier-2), Zombie (occasional).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| 1 | 4 Regular | L, R | drip 0.8 s | kill 4 |
| 2 | 1 Snapper + 2 Regular (Snapper arms a Regular with a sword) | B + L | — | kill Snapper + 2 |
| **CHECKPOINT** (atrium) | — | — | — | — |
| 3 | 1 Pod (Swarm) + 1 Snapper + 3 Regular | B + L,R | drip 0.8 s | clear field |
| 4 (funnel to dept store) | 4 Regular + 1 Zombie Pod | L, R, A | drip 0.7 s | clear field → boss door |
| **BOSS** | **Burly Macho Guy** (HP 300, `TUNING.md` §7) | dept-store arena | — | defeat |
- **Threat Budget:** ~26 + boss. **Act 1 cap = Burly Macho Guy.**

---

## ACT 2 — Sacramento & Airport  ·  *+ tier-2/3*

### Stage 4 — Sacramento Victorian old-town + **Whip/Head-Thrower** → **BOSS: The Colossus** — **[LOCKED]**
- **Teaches (vignette):** an enemy **whips and pulls down** another — the Whip's crowd-control pull
  (`VIGNETTES.md`). Head-Thrower head-grenades debut mid-stage.
- **Pool:** Regular (T1), Snapper (T2), **Head-Thrower** (T2-eff, head-grenades), Whip-armed Regular (via
  Snapper-style hand-off or drop).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | whip-pull demo (scripted) | — | — | auto |
| 1 | 3 Regular + 1 Snapper | L, R + B | drip 0.9 s | clear |
| 2 | 2 Head-Thrower + 2 Regular | B + L,R | thrower cd 3.0 s | kill throwers |
| **CHECKPOINT** (streetcar stop) | — | — | — | — |
| 3 | 1 Snapper + 2 Head-Thrower + 2 Regular | mixed | drip 0.8 s | clear field |
| 4 (funnel) | 4 Regular + 1 Head-Thrower | L,R,A | drip 0.7 s | clear → boss |
| **BOSS** | **The Colossus** (whip, `BOSSES.md` §5.4) | Victorian plaza | — | strip pieces + defeat |
- **Threat Budget:** ~30 + boss.

### Stage 5 — Sacramento Airport terminal + tarmac + **Anti-Aircraft** → **BOSS: Helicopter** — **[LOCKED]**
- **Teaches (vignette):** enemies **throw head-grenades at planes**; a **Bat demo-actor** swats a fastball into
  a small plane (`VIGNETTES.md`) — plus **Anti-Aircraft** rock-throwers debut (user-locked: AA debuts here).
  **Club** becomes a pickup weapon starting this stage (post-vignette, `WEAPONS.md`/`TUNING.md`).
- **Pool:** Regular (T1), **Anti-Aircraft** (T1a, rocks), Head-Thrower (T2-eff), Snapper (T2).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | head-grenade + bat-a-plane (scripted) | — | — | auto |
| 1 | 3 Anti-Aircraft (rock arcs) + 2 Regular | B + L,R | AA throw 2.5 s | kill AA |
| 2 | 2 Head-Thrower + 1 Snapper + 2 Regular | mixed | drip 0.8 s | clear |
| **CHECKPOINT** (gate lounge) | — | — | — | — |
| 3 | 3 Anti-Aircraft + 2 Head-Thrower (planes taxiing hazard) | B, B + L,R | — | clear field |
| 4 (tarmac funnel) | 4 Regular + 2 AA | L,R,B | drip 0.7 s | clear → boss |
| **BOSS** | **Helicopter (Monkey Chopper)** (`BOSSES.md` §5.5) | open tarmac | — | down the chopper |
- **Threat Budget:** ~34 + boss. **Act 2 cap = Helicopter.**

---

## ACT 3 — Hills, Causeway & Dixon  ·  *+ tier-3; Sniper, Flying Monkey, Arm-Ripper*

### Stage 6 — Rolling hills + Yolo causeway (platforming) + **Sniper** → no boss — **[LOCKED]**
- **Teaches (vignette):** two go for a **dime**; one jumps → **Sniper** shoots them out of the air; the other
  grabs the dime → **whistle → Monkey** carries them off (`VIGNETTES.md`). Sniper apex-punish + dime→monkey.
- **Pool:** Regular (T1), **Sniper** (T3-eff, 1 at a time), **Monkey** (economy), Anti-Aircraft (T1a).
- **Terrain:** causeway platforms + water (fall = reset to platform, chip only; `STAGES.md` hazard).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | sniper + dime + monkey (scripted) | — | — | auto |
| 1 | 1 Sniper (perched) + 3 Regular | B(perch) + L,R | sniper cycle 5 s | kill the 3 (sniper optional) |
| 2 (platforms) | 4 Regular + 1 Monkey | L,R + A | drip 0.9 s | cross + clear |
| **CHECKPOINT** (mid-causeway) | — | — | — | — |
| 3 | 1 Sniper + 2 AA + 2 Regular | perch + B + L,R | — | clear field |
| 4 | 5 Regular + 1 Monkey (dime drop) | L,R,B | drip 0.7 s | clear → farm |
- **Threat Budget:** ~30. **No boss** (traversal stage).

### Stage 7 — Farm / Ranch + **Monkey Tamer / Flying Monkey** → **BOSS: Monkey Boss** — **[LOCKED]**
- **Teaches (vignette):** Monkey Boss tosses a **dime**; an enemy catches it, a **Monkey Merc pops out and
  shoots the boss** — only *your* mercs damage him (`VIGNETTES.md`).
- **Pool:** **Monkey Tamer** (untiered, whistles enemy monkeys), **Flying Monkey** (T2-eff, swoops), **Monkey**
  (economy), Regular (T1).
- **Hazard:** cow blocks path / ponds (`AREAS.md`, `STAGES.md`).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | dime → merc shoots boss (scripted) | — | — | auto |
| 1 | 1 Monkey Tamer (2 monkeys) + 2 Regular | B + L,R | whistle 5 s | kill Tamer |
| 2 | 2 Flying Monkey + 3 Regular | air + L,R | swoop 3 s | clear |
| **CHECKPOINT** (barn) | — | — | — | — |
| 3 | 1 Tamer + 2 Flying Monkey + 2 Regular | mixed | — | clear field |
| 4 (funnel) | 4 Regular + 1 Monkey (dime) | L,R,A | drip 0.7 s | clear → boss |
| **BOSS** | **Monkey Boss** (dime→merc-only kill, `BOSSES.md` §5.7) | ranch yard | — | merc him down |
- **Threat Budget:** ~30 + boss.

### Stage 8 — **Dixon boss rush** + **Arm-Ripper** intro → **BOSS: big Arm-Ripper** — **[LOCKED]**
- **Teaches (vignette):** an **Arm-Ripper rips a guy's arms off and opens fire with akimbo pistols** — rip →
  guns (`VIGNETTES.md`). Also the **first big wall** (`STAGES.md` §4).
- **Structure:** a **boss rush** — recurring minibosses back-to-back, then the big Arm-Ripper.
- **Pool:** **Arm-Ripper** (T2a, akimbo pistols), Snapper (T2), Regular (T1), + miniboss reprises (big
  versions, `BOSSES.md` §miniboss rule).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | arm-rip → akimbo fire (scripted) | — | — | auto |
| 1 | 2 Arm-Ripper + 2 Regular | B + L,R | fire 2/s | kill Rippers |
| 2 (miniboss) | **Miniboss: big Snapper** (~1.2×) + 2 Regular adds | arena + L,R | — | defeat miniboss |
| **CHECKPOINT** (main street) | — | — | — | — |
| 3 (miniboss) | **Miniboss: big Head-Thrower** (~1.2×) + 2 AA | arena + B | — | defeat miniboss |
| 4 | 3 Arm-Ripper + 2 Regular | L,R,B | drip 0.8 s | clear → boss |
| **BOSS** | **big Arm-Ripper** (Dixon wall, `BOSSES.md` §5) | town square | — | defeat |
- **Threat Budget:** ~34 + 2 minibosses + boss. **Act 3 cap = big Arm-Ripper.**

---

## ACT 4 — Vallejo to the City  ·  *full roster; Gatling Gunner, Ninja, Ground Smasher, Boomergunner, Pickpocket, Heavy debut*

### Stage 9 — Vallejo Six Flags + **Pickpocket / Ninja** → **BOSS: Tank** — **[LOCKED]**
- **Teaches (vignette):** a **Pickpocket steals a Ninja's coins and runs**; the **Ninja teleports and kills
  him → coins double** (`VIGNETTES.md`). Ninja teleport-kill + Pickpocket 2× reward.
- **Pool:** **Ninja** (T3a, teleport/shuriken), **Pickpocket** (untiered, steals), Regular (T1), Snapper (T2).
- **Hazard:** roller-coaster / midway set-dressing (`AREAS.md`).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | pickpocket→ninja→2× coins (scripted) | — | — | auto |
| 1 | 2 Pickpocket + 3 Regular | A + L,R | pickpocket darts | kill/clear |
| 2 | 2 Ninja + 2 Regular | teleport-in + L,R | tp cd 3 s | kill Ninjas |
| **CHECKPOINT** (midway) | — | — | — | — |
| 3 | 1 Ninja + 2 Pickpocket + 2 Snapper | mixed | — | clear field |
| 4 (funnel) | 4 Regular + 1 Ninja | L,R,B | drip 0.7 s | clear → boss |
| **BOSS** | **Tank** (mount + MG, tier-1 adds drop grenades, `BOSSES.md` §5.3) | Six-Flags lot | — | disable it |
- **Threat Budget:** ~34 + boss.

### Stage 10 — Bay causeway → Marin redwoods + **Boomergunner / Ground Smasher** → **BOSS: Boomergunner** — **[LOCKED]**
- **Teaches (vignette):** a **Boomergunner** throws his gun; it **shoots a civilian and returns** — Boomergunner
  intro (`VIGNETTES.md`). Ground Smasher (zoner) debuts mid-stage.
- **Pool:** **Boomergunner** (T2-eff, orbiting gun), **Ground Smasher** (T3-eff, H-floors, shockwave), Ninja
  (T3a), Regular (T1).
- **Terrain:** redwoods + ferns + mist; bay bridge span.

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | boomergun shoots + returns (scripted) | — | — | auto |
| 1 | 2 Boomergunner + 2 Regular | B + L,R | boomerang 2.5 s | kill gunners |
| 2 | 1 Ground Smasher + 3 Regular | B + L,R | smash 4 s | kill Smasher |
| **CHECKPOINT** (redwood clearing) | — | — | — | — |
| 3 | 2 Boomergunner + 1 Ground Smasher + 1 Ninja | mixed | — | clear field |
| 4 (funnel) | 4 Regular + 1 Boomergunner | L,R,B | drip 0.7 s | clear → boss |
| **BOSS** | **Boomergunner boss** (Marin, `BOSSES.md` §5) | forest clearing | — | defeat |
- **Threat Budget:** ~36 + boss.

### Stage 11 — Golden Gate Bridge + **Gatling Gunner** + car cover → **BOSS: Gatling Gun Guy** — **[LOCKED]**
- **Teaches (vignette):** enemy advances → **Ground Smasher stuns → Gatling barrage eviscerates**, but anyone
  **behind a car is unharmed** (`VIGNETTES.md`). Zoner-stun + barrage + **car cover** (the boss mechanic primer).
- **Pool:** **Gatling Gunner** (T3, H-floors, 1-HP stream), Ground Smasher (T3-eff), Ninja (T3a), Regular (T1).
- **Terrain:** bridge deck with **parked cars = cover** (line-of-sight blockers vs. barrages).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | stun → barrage → car-cover demo (scripted) | — | — | auto |
| 1 | 2 Gatling Gunner (contort tell 2 s) + 2 Regular | B + L,R | burst 2.5 s | kill gunners (use cover) |
| 2 | 1 Ground Smasher + 2 Ninja + 2 Regular | mixed | — | clear |
| **CHECKPOINT** (mid-span) | — | — | — | — |
| 3 | 2 Gatling + 1 Ground Smasher + 2 Ninja | mixed | — | clear field |
| 4 (funnel) | 4 Regular + 1 Gatling | L,R,B | drip 0.7 s | clear → boss |
| **BOSS** | **Gatling Gun Guy** (barrage + car cover, `BOSSES.md` §5.6) | bridge deck (cars) | — | defeat behind cover |
- **Threat Budget:** ~40 + boss. **Act 4 cap = Gatling Gun Guy** (the mechanical wall before the finale run).

### Stage 12 — San Francisco streets + **Heavy** intro + trolley hazard → funnel to the Tower — **[LOCKED]**
- **Teaches (vignette):** the **trolley plows through a regular enemy** — but the **Heavy just steps aside**;
  trolley flattens everything **except the immovable Heavy** (`VIGNETTES.md`). **Heavy debuts here** (user-locked).
- **Pool:** **Heavy** (untiered, 220 HP, H-floors, max 2), **full roster mix** (Ninja, Gatling, Ground Smasher,
  Boomergunner, Regular), trolley hazard.

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | trolley vs. Heavy (scripted) | — | — | auto |
| 1 | 1 Heavy + 3 Regular | B + L,R | Heavy punch 250 ms | down the Heavy |
| 2 | 2 Ninja + 1 Gatling + 2 Regular (trolley pass) | mixed | — | clear |
| **CHECKPOINT** (tower plaza) | — | — | — | — |
| 3 | 2 Heavy + 2 Ninja | B + teleport | — | down both Heavies |
| 4 (elevator funnel) | 1 Ground Smasher + 1 Boomergunner + 3 Regular | L,R,B | drip 0.7 s | clear → **elevator to rooftop** |
- **Threat Budget:** ~44 (peak). **No mid-stage boss** — this is the gauntlet run into the finale.

---

## FINALE — Salesforce rooftop — **[LOCKED]**

### Stage 13 (Finale) — **BOSS: Phil**
- **Teaches (vignette):** **Phil's monologue** during the climb (Holy Sharpener, "2D chaos"); the tower **sways**
  (`VIGNETTES.md`). Establishes tower-sway + fall = instant death.
- **Structure:** **no enemy waves before the boss** — you step off the elevator into the arena. Phil **draws**
  his own adds mid-fight (reprise summons), so the "waves" are boss-driven, not scripted here.
- **Arena hazard:** rooftop **sway/slippage**, fall = instant death (§Boss arenas below).
- **BOSS:** **Phil** — draw (invuln) → run dry → **sharpen (vulnerable 3–5 s)** → repeat; **killed only by the
  pencil-laser finisher** (`BOSSES.md` §5.1, `TUNING.md` §7). Exempt from the <2:00 rule; brutally hard.

---

## Boss arena layouts — **[LOCKED]**

> Each boss fight is a **camera-locked arena** (no scroll). Dimensions in **world-units (wu)**; the play-band is
> the depth of the Z-lane the player can walk (`TUNING.md` §1). "Add ports" = where boss-summoned adds enter.

| Boss | Arena size (W × Z-depth) | Cover / terrain | Add ports | Hazard | Special layout notes |
|---|---|---|---|---|---|
| **Sandwich Bros / big T1** | 24 × 6 wu | open suburb lane | L, R | car/bus cross | smallest arena; a warm-up ring |
| **Burly Macho Guy** | 28 × 7 wu | dept-store floor: 2 display islands (soft cover) | B (stockroom door) | thrown-enemy projectiles land as debris | islands break line for his ground-spike |
| **The Colossus** | 30 × 7 wu | Victorian plaza, open | L, R | streetcar edge (side KO line) | big-version footprint; strip-pieces spacing needs room |
| **Helicopter** | 32 × 8 wu | open tarmac, 2 parked luggage carts (cover) | B (hangar) | jet-blast gust pushes player | **airborne boss** — vertical space matters; carts block head-fire |
| **Monkey Boss** | 28 × 7 wu | ranch yard, hay-bale cover (2) | L, R (barn) | pond (side hazard) | dime-catch zones marked; merc-only damage |
| **big Arm-Ripper** | 30 × 7 wu | Dixon town square, open + water-tower base pillar | L, R, B | none | pillar = one hard cover vs. akimbo fire |
| **Tank** | 34 × 8 wu | Six-Flags lot, ride-support pillars (cover) | B (military gate) | MG sweep line | wide — you circle to mount; tier-1 adds drop grenades (the weapon-gate) |
| **Boomergunner boss** | 28 × 7 wu | redwood clearing, 3 tree trunks (cover) | L, R | mist (soft vision) | trunks break the orbiting-gun return path |
| **Gatling Gun Guy** | 32 × 7 wu | GG bridge deck, **4 parked cars = hard cover** | L, R | barrage line + wind gust | **cover is the mechanic** — "BARRAGE INCOMING" warning, hide behind a car |
| **Phil (finale)** | 30 × 8 wu | Salesforce rooftop, HVAC block (1 cover) | drawn anywhere (pencil) | **sway/slippage** + **fall = instant death** (no railing on 2 edges) | reprise summons appear via draw-in; sharpen-window is the only damage window |

- **Fall-off arenas (KO line):** only **Phil's rooftop** has an instant-death fall edge; **Colossus** and
  **Gatling Gun Guy** have **side KO lines** (streetcar / bridge edge) that only apply to *knocked-back enemies*,
  not the player. All others are walled.
- **Weapon-gated arenas:** where a boss needs a specific weapon (e.g. **Tank** needs grenades), the arena's
  **tier-1 adds drop only that weapon** (`BOSSES.md` §1, overriding the normal drop pool for that arena).
- **Miniboss arenas** reuse the **stage's current lane** (no bespoke arena) — they are big-version enemies, not
  set-piece bosses (`BOSSES.md` miniboss rule).
