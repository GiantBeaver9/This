# this.l — Per-Stage Encounter / Wave Tables

> **Purpose:** the concrete **spawn spine for every stage** — which enemies, in what order, at what cadence,
> where checkpoints sit, and what gate advances the player. This is the doc the build reads to populate each
> level's *scripted* beats; the length-fill rule (§0) turns each spine into a full-length stage.
>
> **Authority hierarchy (LOCKED — how conflicts resolve):** for **enemy debut area** the authority is
> **`ENEMIES.md` §6**; for **area/stage structure and the act-capping boss** it is **`STAGES.md` §4** /
> **`AREAS.md`**; for **vignette placement** it is **`VIGNETTES.md`**; for **every number** it is
> **`TUNING.md`**. **This doc conforms to those** — where an earlier draft of this file drifted, it has been
> corrected to match them. If any residual disagreement is found, those source docs win over this one.
>
> **Legend:** **[LOCKED]** — all 13 stages (12 + finale) scripted below. Counts are **1-player normal**;
> multiplayer is **parked / not in the overnight build** (`CHARACTERS.md` §5), so the ×2.5 co-op figures live
> in those docs as a future note only. Endless overrides everything (`TUNING.md` §8.3).

---

## 0. Universal spawn & length rules — **[LOCKED]**

- **On-screen pursuer cap = 8** (`GAMEPLAY_LOOP.md`): never more than 8 enemies actively pursuing. Extra
  roster **queues** and streams in as pursuers die. **Pod-spawned Swarmers are the sole exception** — a Pod
  spits **1 Swarmer every 3 s up to a field cap of 6** pod-spawned, which may briefly push past 8 (`TUNING.md`
  §4, single model).
- **A "wave" = a spawn batch.** The stage **gates** (camera lock) until the wave's **kill-quota** is met,
  then scrolls on.
- **Spawn sides:** `L` left, `R` right, `B` back-Z, `A` ambush (door/window/manhole per area). Default L/R.
- **Cadence:** "drip 0.8 s" = one enemy every 0.8 s, so the 8-cap breathes (not all at once).
- **The spine vs. the full stage (length reconciliation) — [LOCKED]:** the tables below are the **mandatory
  encounter spine** — the vignette, the teaching beats, the debut waves, and the boss. Each **combat stage
  runs ~15–18 minutes** (`STAGES.md` §2: meatier stages, ~13 stages → 3–4 hr). To reach that length the stage
  **pads between spine waves with filler waves** drawn **only from that stage's pool**, at the listed cadence:
  **~10–16 filler waves per stage** (avg 4–6 enemies each), inserted after the spine's teaching wave and
  before its funnel. Filler waves carry **no new enemy types** and **no new mechanic** — they are the "ramp."
  So each stage = **spine (scripted) + filler (procedural from pool) + funnel + boss**.
- **[LOCKED] Filler-wave composition rule (so it's not hand-waved):** each filler wave = **weighted random draw
  from the stage pool**, weighted **60% toward the stage's *newest* enemy type** (reinforce what it teaches),
  **40% split across the rest**, capped at the 8-pursuer limit. Wave size **ramps linearly** from **4 → 6**
  enemies across the filler block. **No two consecutive waves are identical.** Deterministic seed = stage index
  (so a stage plays the same each run).
- **[LOCKED] Exact filler-wave COUNT per stage = the midpoint of the listed range**, rounded up (e.g. "10–14"
  → **12**; "12–16" → **14**; "8–10" → **9**). The range in each table is illustrative; the midpoint is the
  fixed count the seed fills. So the whole stage is reproducible with **zero per-wave authoring** — count is
  pinned, composition is seeded.
- **[LOCKED] Stage geometry (so the level isn't invented from prose):** each **combat stage lane = ~140 wu
  long** (spine + filler + funnel; the vignette plays at the head, the boss arena caps the tail). The camera
  scrolls forward as waves clear (gated per §0). **Prop/funnel/cover placement follows `AREAS.md`** per theme
  (parked cars & hedges pinch the lane into fighting pockets; cars-as-cover on the Golden Gate). **Causeway
  platforming stretches (Stages 6, 10) — [LOCKED default layout]:** a **linear run of exactly 6 platforms**,
  each **10 wu** long with **4 wu gaps** between them (the pinned midpoints of the 5–7 / 8–12 / 3–5 design
  ranges; **(tunable)** per stage). Gaps are jumpable: jump distance ≈ 4 wu + air-dash 3.5 wu = ~7.5 wu reach,
  so a **4 wu gap clears on a plain jump**. Water between platforms (fall = 10 HP chip + respawn on last
  platform, §Stage 6). Exact decorative prop coordinates are level-editor polish, not a gameplay value — but
  the **6×10 wu platforms with 4 wu gaps is the concrete buildable default**, not a range to pick from.
- **Checkpoints — [LOCKED] (matches `TUNING.md` §8.1):** **one at stage start** (respawn point on continue) +
  **one mid-stage** (marked below, roughly halfway through the filler block) + **one at the boss door** (retry
  the boss, not the stage). Bossless stages get start + mid only.
- **Threat Budget** = Σ(spine fodder count × tier), **+ the boss** on boss stages (a boss counts as its HP/50,
  so Sandwich Bros 160 ≈ +3.2, Burly 300 ≈ +6). It **trends upward across the campaign** measured **per act**
  (Act 1 < Act 2 < … each act's total ≥ the last); it is **not** required to rise on every single stage —
  e.g. Stage 2's fodder spine (7) dips below Stage 1's (10) because Stage 2 spends its budget on the Sandwich
  Bros boss instead. Filler is excluded. A design guardrail, not a runtime value.

---

## ACT 1 — Placer Suburbs & Mall  ·  *tier 0–1 (first T2 enemy debuts Area 2)*

> **Area-1 structure (`AREAS.md` §1.6/§1.9):** Lincoln High → suburb/old-Hwy-65 → **Sandwich Bros fight** →
> Rocklin → **Roseville Galleria (Area-1 finale, Burly)**. Debuts here (`ENEMIES.md` §6): **Regular (suburbs),
> Zombie + Swarmer (mall).**

### Stage 1 — Lincoln High + suburb streets (opener) — **[LOCKED]**
- **Teaches:** the **punch only** (`VIGNETTES.md`: dancing Zebra punches a regular enemy). No weapons.
- **Pool:** Regular Melee (T1) only. **Hazard:** cars & school buses cross the lane (dodge).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | Zebra + 1 Regular (scripted) | — | — | auto |
| 1 | 2 Regular | L | drip 1.0 s | kill 2 |
| 2 | 3 Regular | L, R | drip 0.9 s | kill 3 |
| **CHECKPOINT** (mid) | — | — | — | — |
| *filler* | Regular only, 10–14 waves | L,R,B | drip 0.8 s | clear each |
| 3 (funnel) | 5 Regular (1 bus pass) | L, R, B | drip 0.8 s | kill 5 → exit |
- **Threat Budget (spine):** 10. **No boss** (opener ramp). Walk off-screen right toward old Hwy 65.

### Stage 2 — Old Hwy 65 → **BOSS: Sandwich Bros (big Tier-1)** — **[LOCKED]**
- **Teaches:** the **"big version"** concept — the first boss-scale enemy and first on-screen proof of the
  pencil (`AREAS.md` §1.6). No new fodder type (still Regulars).
- **Pool:** Regular Melee (T1). **Hazard:** roadside traffic tapering off toward the restaurant.

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| 1 | 3 Regular | L, R | drip 0.9 s | kill 3 |
| **CHECKPOINT** (mid) | — | — | — | — |
| *filler* | Regular only, 10–12 waves | L,R,B | drip 0.8 s | clear each |
| 2 (funnel to restaurant) | 4 Regular | L,R,B | drip 0.8 s | clear → boss door |
| **BOSS** | **Sandwich Bros / big Tier-1** (HP 160, `TUNING.md` §7) | outside Sandwich Bros | — | defeat |
- **Threat Budget (spine):** 7 + boss. **Mid-area boss** (the first-boss taste; not the act cap).

### Stage 3 — Rocklin → Roseville Galleria mall + **Zombie/Swarmer** debut → **BOSS: Burly Macho Guy** — **[LOCKED]**
- **Teaches (vignette):** **guard shoots a T1 → it zombifies → grabs the guard → they fall** (`VIGNETTES.md`,
  staged at the Galleria per `AREAS.md` §1.9). This is the **Zombie grab + Swarmer** debut (`ENEMIES.md` §6:
  both debut Area 1 mall). Cowering shoppers as set-dressing.
- **Pool:** Regular (T1), **Swarmer** (T1b, Pod-spawned 1/3 s, cap 6), **Zombie** (T0, from a Pod), **Pod** (HP 50).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | guard→zombie→grab (scripted) | A | — | auto |
| 1 | 3 Regular | L, R | drip 0.9 s | kill 3 |
| 2 | 1 Pod (Swarmers, cap 6) + 2 Regular | B + L | pod 1/3 s | destroy Pod |
| **CHECKPOINT** (atrium) | — | — | — | — |
| 3 | 1 Zombie (Pod-spawned) + 3 Regular | A + L,R | drip 0.8 s | clear (first grab here) |
| *filler* | Regular / Swarmer-pod / occasional Zombie, 12–16 waves | L,R,B | drip 0.8 s | clear each |
| 4 (funnel to dept store) | 4 Regular + 1 Zombie Pod | L,R,A | drip 0.7 s | clear → boss door |
| **BOSS** | **Burly Macho Guy** (HP 300, `TUNING.md` §7) | dept-store arena | — | defeat |
- **Threat Budget (spine):** ~12 + boss. **Act 1 cap = Burly Macho Guy** (`STAGES.md` §4).

---

## ACT 2 — Sacramento & Airport  ·  *+ tier-2/3*

> Debuts (`ENEMIES.md` §6): **Snapper (Sacramento)**, then **Anti-Aircraft + Head-Thrower (airport)**.

### Stage 4 — Sacramento Victorian old-town + **Snapper** debut → **BOSS: The Colossus** — **[LOCKED]**
- **Teaches (vignette):** an enemy **whips and pulls down** another — the Whip's crowd-control pull
  (`VIGNETTES.md`). **Snapper** (first tier-2) debuts mid-stage, snapping a T1 → sword.
- **Pool:** Regular (T1), **Snapper** (T2). *(The **Whip** is introduced here as a **weapon** — the Colossus
  arena is whip weapon-gated, `BOSSES.md` §5.4, and whips enter the Area-2 drop pool, `TUNING.md` §6.1. The
  whip-wielding figure in the vignette is a **demo actor**, not a roster enemy — like the airport Bat demo,
  `ENEMIES.md` §6.)*

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | whip-pull demo (scripted) | — | — | auto |
| 1 | 3 Regular | L, R | drip 0.9 s | clear |
| 2 | 1 Snapper (snaps a Regular into a sword he wields) + 2 Regular | B + L | — | kill Snapper + 2 |
| **CHECKPOINT** (streetcar stop) | — | — | — | — |
| *filler* | Regular / Snapper, 12–14 waves | L,R,B | drip 0.8 s | clear each |
| 3 (funnel) | 4 Regular + 1 Snapper | L,R,A | drip 0.7 s | clear → boss door |
| **BOSS** | **The Colossus** (whip, `BOSSES.md` §5.4) | Victorian plaza | — | strip pieces + defeat |
- **Threat Budget (spine):** ~14 + boss. **Mid-Act-2 boss** (not the act cap).

### Stage 5 — Sacramento Airport terminal + tarmac + **Anti-Aircraft / Head-Thrower** debut → **BOSS: Helicopter** — **[LOCKED]**
- **Teaches (vignette):** enemies **throw head-grenades at planes**; a **Bat demo-actor** swats a fastball into
  a small plane (`VIGNETTES.md`) — **Anti-Aircraft** rock-throwers and **Head-Thrower** head-grenades both
  debut here (`ENEMIES.md` §6). The **Club** becomes a pickup weapon starting this stage (post-vignette,
  `WEAPONS.md` §3.7c / area loot table `TUNING.md` §6.1).
- **Pool:** Regular (T1), **Anti-Aircraft** (T1a, rocks), **Head-Thrower** (T2-eff), Snapper (T2).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | head-grenade + bat-a-plane (scripted) | — | — | auto |
| 1 | 3 Anti-Aircraft (rock arcs) + 2 Regular | B + L,R | AA throw 2.5 s | kill AA |
| 2 | 2 Head-Thrower + 1 Snapper | B + L | thrower cd 3.0 s | clear |
| **CHECKPOINT** (gate lounge) | — | — | — | — |
| *filler* | Regular / AA / Head-Thrower / Snapper, 12–16 waves | L,R,B | drip 0.8 s | clear each |
| 3 (tarmac funnel) | 4 Regular + 2 AA | L,R,B | drip 0.7 s | clear → boss door |
| **BOSS** | **Helicopter (Monkey Chopper)** (`BOSSES.md` §5.5) | open tarmac | — | down the chopper |
- **Threat Budget (spine):** ~18 + boss. **Act 2 cap = Helicopter** (`STAGES.md` §4).

---

## ACT 3 — Hills, Causeway & Dixon  ·  *+ tier-3; Sniper, Flying Monkey, Arm-Ripper*

> Debuts (`ENEMIES.md` §6): **Sniper + Flying Monkey (causeway)**, **Monkey Tamer + Monkey (Area 3)**,
> **Arm-Ripper (Dixon)**.

### Stage 6 — Rolling hills + Yolo causeway (platforming) + **Sniper / Flying Monkey** debut — no boss — **[LOCKED]**
- **Teaches (vignette):** two go for a **dime**; one jumps → **Sniper** shoots them out of the air; the other
  grabs the dime → **whistle → Monkey** carries them off (`VIGNETTES.md`). Sniper apex-punish + dime→monkey.
- **Pool:** Regular (T1), **Sniper** (T3-eff, 1 at a time), **Flying Monkey** (T2-eff), **Monkey** (economy),
  Anti-Aircraft (T1a).
- **Terrain:** causeway platforms + water — **falling in = respawn on the last platform + 10 HP chip** (no
  drowning death; resolves the "chip only" value).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | sniper + dime + monkey (scripted) | — | — | auto |
| 1 | 1 Sniper (perched) + 3 Regular | B(perch) + L,R | sniper cycle 5 s | kill the 3 |
| 2 (platforms) | 2 Flying Monkey + 3 Regular + 1 Monkey | air + L,R + A | drip 0.9 s | cross + clear |
| **CHECKPOINT** (mid-causeway) | — | — | — | — |
| *filler* | Regular / Flying Monkey / AA / occasional Sniper, 10–14 waves | perch + L,R,B,air | — | clear each |
| 3 (funnel) | 5 Regular + 1 Monkey (drops the Merc claim — needs a held dime) | L,R,B | drip 0.7 s | clear → farm |
- **Threat Budget (spine):** ~22. **No boss** (traversal stage).

### Stage 7 — Farm / Ranch + **Monkey Tamer** → **BOSS: Monkey Boss** — **[LOCKED]**
- **Teaches (vignette):** Monkey Boss tosses a **dime**; an enemy catches it, a **Monkey Merc pops out and
  shoots the boss** — only *your* mercs damage him (`VIGNETTES.md`).
- **Pool:** **Monkey Tamer** (untiered, whistles enemy monkeys), **Flying Monkey** (T2-eff), **Monkey**
  (economy), Regular (T1). **Hazard:** cow blocks path / ponds (`AREAS.md`).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | dime → merc shoots boss (scripted) | — | — | auto |
| 1 | 1 Monkey Tamer (2 monkeys) + 2 Regular | B + L,R | whistle 5 s | kill Tamer |
| 2 | 2 Flying Monkey + 3 Regular | air + L,R | swoop 3 s | clear |
| **CHECKPOINT** (barn) | — | — | — | — |
| *filler* | Regular / Flying Monkey / Tamer / Monkey, 10–14 waves | mixed | — | clear each |
| 3 (funnel) | 4 Regular + 1 Monkey (drops the Merc claim) | L,R,A | drip 0.7 s | clear → boss door |
| **BOSS** | **Monkey Boss** (dime→merc-only kill, `BOSSES.md` §5.7) | ranch yard | — | merc him down |
- **Threat Budget (spine):** ~24 + boss. **Mid-Act-3 boss.**

### Stage 8 — **Dixon boss rush** + **Arm-Ripper** debut → **BOSS: big Arm-Ripper** — **[LOCKED]**
- **Teaches (vignette):** an **Arm-Ripper rips a guy's arms off and opens fire with akimbo pistols** — rip →
  guns (`VIGNETTES.md`). The **first big wall** (`STAGES.md` §4).
- **Structure — [LOCKED] (`AREAS.md` §3.2a):** a **boss rush = exactly 4 minibosses (big-version enemies of crews
  you've met) → 1 big boss.** Minibosses reuse the stage lane (no bespoke arena, `BOSSES.md` miniboss rule).
- **Pool:** **Arm-Ripper** (T2a, akimbo pistols), Snapper (T2), Regular (T1), + the 4 miniboss reprises.

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | arm-rip → akimbo fire (scripted) | — | — | auto |
| 1 | 2 Arm-Ripper + 2 Regular | B + L,R | fire 2/s | kill Rippers |
| **Miniboss 1** | **big Snapper** (~1.2×) + 2 Regular adds | lane + L,R | — | defeat |
| **Miniboss 2** | **big Head-Thrower** (~1.2×) + 2 AA | lane + B | — | defeat |
| **CHECKPOINT** (main street) | — | — | — | — |
| **Miniboss 3** | **big Flying Monkey** (~1.2×, swoops — threatens a grounded player, unlike the Sniper) + 2 Regular | air + L,R | — | defeat |
| **Miniboss 4** | **big Arm-Ripper elite** (~1.2×, akimbo close fire) + 2 Regular | B + L,R | — | defeat |
| *filler* | Arm-Ripper / Snapper / Regular, 8–10 waves | L,R,B | drip 0.8 s | clear each |
| 2 (funnel) | 3 Arm-Ripper + 2 Regular | L,R,B | drip 0.8 s | clear → boss door |
| **BOSS** | **big Arm-Ripper** (Dixon wall, `BOSSES.md` §5) | town square | — | defeat |
- **Threat Budget (spine):** ~26 + **4 minibosses** + boss. **Act 3 cap = big Arm-Ripper** (`STAGES.md` §4).

---

## ACT 4 — Vallejo to the City  ·  *full roster; Ninja/Pickpocket (Vallejo), Boomergunner (Marin), Gatling Gunner + Ground Smasher (Golden Gate), Heavy (SF streets)*

### Stage 9 — Vallejo Six Flags + **Ninja / Pickpocket** debut → **BOSS: Tank** — **[LOCKED]**
- **Teaches (vignette):** a **Pickpocket steals a Ninja's coins and runs**; the **Ninja teleports and kills
  him → coins double** (`VIGNETTES.md`). Ninja teleport-kill + Pickpocket 2× reward.
- **Pool:** **Ninja** (T3a, teleport/shuriken), **Pickpocket** (untiered, steals), Regular (T1), Snapper (T2).
- **Hazard:** **roller-coaster cars** run damaging on-rail passes (50 dmg + knockdown, `TUNING.md` §6.2) + midway set-dressing (`AREAS.md`).
- **World pickup:** a **Rocket Launcher** is placed **near the Tank arena entrance** (mid-stage, just before
  the boss door) — the extra firepower for the objective fight (`WEAPONS.md` §3.8b, `TUNING.md` §6.1).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | pickpocket→ninja→2× coins (scripted) | — | — | auto |
| 1 | 2 Pickpocket + 3 Regular | A + L,R | pickpocket darts | clear |
| 2 | 2 Ninja + 2 Regular | teleport-in + L,R | tp cd 3 s | kill Ninjas |
| **CHECKPOINT** (midway) | — | — | — | — |
| *filler* | Regular / Ninja / Pickpocket / Snapper, 12–16 waves | mixed | — | clear each |
| 3 (funnel) | 4 Regular + 1 Ninja | L,R,B | drip 0.7 s | clear → boss door |
| **BOSS** | **Tank** (mount + MG; tier-1 adds drop grenades, `BOSSES.md` §5.3) | Six-Flags lot | — | disable it |
- **Threat Budget (spine):** ~28 + boss. **Mid-Act-4 boss.**

### Stage 10 — Bay causeway → Marin redwoods + **Boomergunner** debut → **BOSS: Boomergunner** — **[LOCKED]**
- **Teaches (vignette):** a **Boomergunner** throws his gun; it **shoots a civilian and returns**
  (`VIGNETTES.md`). Boomergunner debuts here (`ENEMIES.md` §6). *(Ground Smasher does **not** appear yet — it
  debuts Stage 11, Golden Gate.)*
- **Pool:** **Boomergunner** (T2-eff, orbiting gun), Ninja (T3a), Regular (T1), Snapper (T2).
- **Terrain:** redwoods + ferns + mist; bay bridge span.

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | boomergun shoots + returns (scripted) | — | — | auto |
| 1 | 2 Boomergunner + 2 Regular | B + L,R | boomerang 2.5 s | kill gunners |
| 2 | 1 Boomergunner + 2 Ninja + 2 Regular | mixed | — | clear |
| **CHECKPOINT** (redwood clearing) | — | — | — | — |
| *filler* | Regular / Boomergunner / Ninja, 12–14 waves | L,R,B | drip 0.8 s | clear each |
| 3 (funnel) | 4 Regular + 1 Boomergunner | L,R,B | drip 0.7 s | clear → boss door |
| **BOSS** | **Boomergunner boss** (Marin, `BOSSES.md` §5) | forest clearing | — | defeat |
- **Threat Budget (spine):** ~30 + boss. **Mid-Act-4 boss.**

### Stage 11 — Golden Gate Bridge + **Gatling Gunner / Ground Smasher** debut + car cover → **BOSS: Gatling Gun Guy** — **[LOCKED]**
- **Teaches (vignette):** enemy advances → **Ground Smasher stuns → Gatling barrage eviscerates**, but anyone
  **behind a car is unharmed** (`VIGNETTES.md`). Zoner-stun + barrage + **car cover** (the boss-mechanic
  primer). **Gatling Gunner** and **Ground Smasher** both debut here (`ENEMIES.md` §6).
- **Pool:** **Gatling Gunner** (T3, H-floors, 1-HP stream), **Ground Smasher** (T3-eff, H-floors), Ninja
  (T3a), Regular (T1).
- **Terrain:** bridge deck with **parked cars = cover** (line-of-sight blockers vs. barrages).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | stun → barrage → car-cover demo (scripted) | — | — | auto |
| 1 | 2 Gatling Gunner (contort tell 2 s) + 2 Regular | B + L,R | burst 2.5 s | kill gunners (use cover) |
| 2 | 1 Ground Smasher + 2 Ninja + 2 Regular | mixed | smash 4 s | clear |
| **CHECKPOINT** (mid-span) | — | — | — | — |
| *filler* | Regular / Gatling / Ground Smasher / Ninja, 12–16 waves | mixed | — | clear each |
| 3 (funnel) | 4 Regular + 1 Gatling | L,R,B | drip 0.7 s | clear → boss door |
| **BOSS** | **Gatling Gun Guy** (barrage + car cover, `BOSSES.md` §5.6) | bridge deck (cars) | — | defeat behind cover |
- **Threat Budget (spine):** ~34 + boss. **Act 4 cap = Gatling Gun Guy** (`STAGES.md` §4) — the mechanical
  wall before the finale approach.

### Stage 12 — San Francisco streets + **Heavy** debut + trolley hazard → funnel to the Tower — **[LOCKED]**
- **Teaches (vignette):** the **trolley plows through a regular enemy** — but the **Heavy just steps aside**;
  trolley flattens everything **except the immovable Heavy** (`VIGNETTES.md`). **Heavy debuts here**
  (`ENEMIES.md` §6). This SF-streets slog is the **finale approach**, a long gauntlet (`AREAS.md` §4.3).
- **Pool:** **Heavy** (untiered, 220 HP, H-floors, max 2), **full roster mix** (Ninja, Gatling, Ground
  Smasher, Boomergunner, Regular), trolley hazard.
- **World pickup:** a **Rocket Launcher** is placed **in the mid-stage filler stretch** (after the tower-plaza
  checkpoint) — for the Heavy-heavy gauntlet (`WEAPONS.md` §3.8b, `TUNING.md` §6.1).

| Wave | Spawns | Sides | Cadence | Gate |
|---|---|---|---|---|
| Vignette | trolley vs. Heavy (scripted) | — | — | auto |
| 1 | 1 Heavy + 3 Regular | B + L,R | Heavy punch 250 ms | down the Heavy |
| 2 | 2 Ninja + 1 Gatling + 2 Regular (trolley pass) | mixed | — | clear |
| **CHECKPOINT** (tower plaza) | — | — | — | — |
| *filler* | full-roster mix, 14–18 waves (longest stage, `AREAS.md` §4.3) | L,R,B | drip 0.7 s | clear each |
| 3 | 2 Heavy + 2 Ninja | B + teleport | — | down both Heavies |
| 4 (elevator funnel) | 1 Ground Smasher + 1 Boomergunner + 3 Regular | L,R,B | drip 0.7 s | clear → **elevator to rooftop** |
- **Threat Budget (spine):** ~40 (peak). **No mid-stage boss** — the gauntlet run into the finale (this is why
  `STAGES.md` §2's "each area ends in a main boss" is satisfied by **Gatling Gun Guy** as the Area-4 cap;
  SF-streets belongs to the **finale approach**, not a fifth boss stage).

---

## FINALE — Salesforce rooftop — **[LOCKED]**

### Stage 13 (Finale) — **BOSS: Phil**
- **Teaches (vignette):** **Phil's monologue** during the climb (Holy Sharpener, "2D chaos"); the tower
  **sways** (`VIGNETTES.md`).
- **Structure:** **no enemy waves before the boss** — you step off the elevator into the arena. Phil **draws**
  his own adds mid-fight. The **full Phil fight script** (draw cadence, lead-pool size, sharpen-window timing,
  reprise-summon roster, sway strength, edge zones) is now specced in **`BOSSES.md` §5.1** — that is the
  authority for the finale's beats; this doc defers to it.
- **Arena hazard:** rooftop **sway/slippage**, fall = instant death (§Boss arenas below).
- **Checkpoint:** one **at the rooftop door** (the elevator arrival) — dying to Phil retries **the Phil fight**,
  not the SF-streets stage (matches the boss-door checkpoint rule, §0 / `TUNING.md` §8.1).
- **Kill condition:** **the pencil-laser finisher only** (`BOSSES.md` §5.1, `TUNING.md` §7). Exempt from the
  <2:00 rule; brutally hard.

---

## Boss arena layouts — **[LOCKED]**

> Each boss fight is a **camera-locked arena** — the **level stops advancing** (no forward scroll to new
> ground). Arenas wider than the ~26.7 wu screen (`TUNING.md` §1) let the **camera pan within the arena box**
> (bounded), so the listed widths are the *arena bounds*, not the screen. Dimensions in **world-units (wu)**;
> the play-band is the Z-lane depth the player can walk. **Arena Z-depths of 7–8 wu deepen the standard 6.0 wu
> band for that boss fight** (allowed per `TUNING.md` §1 — big/airborne bosses get a wider band, then it
> returns to 6.0). "Add ports" = where summoned adds enter.

| Boss | Arena size (W × Z-depth) | Cover / terrain | Add ports | Hazard | Special layout notes |
|---|---|---|---|---|---|
| **Sandwich Bros / big T1** | 24 × 6 wu | open lane outside the restaurant | L, R | roadside traffic tapering | smallest arena; a warm-up ring |
| **Burly Macho Guy** | 28 × 7 wu | dept-store floor: 2 display islands (soft cover) | B (stockroom door) | thrown-enemy debris | islands break line for his ground-spike |
| **The Colossus** | 30 × 7 wu | Victorian plaza, open | L, R | streetcar side-KO line (enemies only) | big footprint; strip-pieces spacing needs room |
| **Helicopter** | 32 × 8 wu | open tarmac, 2 luggage carts (cover) | B (hangar) | jet-blast gust pushes player | **airborne boss** — vertical space matters; carts block head-fire |
| **Monkey Boss** | 28 × 7 wu | ranch yard, 2 hay-bale covers | L, R (barn) | pond (side hazard) | dime-catch zones marked; merc-only damage |
| **big Arm-Ripper** | 30 × 7 wu | Dixon town square + water-tower base pillar | L, R, B | none | pillar = one hard cover vs. akimbo fire |
| **Tank** | 34 × 8 wu | Six-Flags lot, ride-support pillars (cover) | B (military gate) | MG sweep line | wide — circle to mount; tier-1 adds drop grenades (weapon-gate) |
| **Boomergunner boss** | 28 × 7 wu | redwood clearing, 3 tree trunks (cover) | L, R | mist (soft vision) | trunks break the orbiting-gun return path |
| **Gatling Gun Guy** | 32 × 7 wu | GG bridge deck, **4 parked cars = hard cover** | L, R | barrage line + wind gust | **cover is the mechanic** — "BARRAGE INCOMING" warning, hide behind a car |
| **Phil (finale)** | 30 × 8 wu | Salesforce rooftop, 1 HVAC-block cover | drawn anywhere (pencil) | **sway/slippage** + **fall = instant death** (2 railless edges) | reprise summons draw in; sharpen-window is the only damage window (`BOSSES.md` §5.1) |

- **Fall-off arenas (KO line):** only **Phil's rooftop** has an instant-death fall edge for the *player*;
  **Colossus** and **Gatling Gun Guy** have **side KO lines** (streetcar / bridge edge) that apply only to
  *knocked-back enemies*, not the player. All others are walled.
- **Weapon-gated arenas:** where a boss needs a specific weapon (e.g. **Tank** needs grenades), the arena's
  **tier-1 adds drop only that weapon** (`BOSSES.md` §1, overriding the normal drop pool for that arena).
- **Miniboss arenas** reuse the **stage's current lane** (no bespoke arena) — big-version enemies, not
  set-piece bosses (`BOSSES.md` miniboss rule).
- **[LOCKED] Cosmetic-only arena hazards (0 dmg, atmosphere):** **"mist (soft vision)"** (Boomergunner arena)
  = a light fog overlay, **no mechanical effect**; **"thrown-enemy debris"** (Burly arena) = visual bits from
  his enemy-tosses, **not a damaging hazard**. The **streetcar / bridge side-KO lines** (Colossus, GGG) only
  affect **knocked-back enemies**, never the player, and fire whenever an enemy is shoved past the edge (no
  cadence). Everything with a damage number is in `TUNING.md` §6.2.
