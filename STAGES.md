# this.l — Stages, Goals & Branching

> **Scope:** run structure, the branching tree, per-stage anatomy, the enemy-layering schedule, goals/win,
> death handling, and environment themes (→ background assets). Extends `GAMEPLAY_LOOP.md` §7 and pulls the
> enemy/boss rosters into a play order.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[ITERATE]** flesh out next · **[LATER]** parked.

---

## 1. Goals & scope — **[LOCKED target]**

- **[LOCKED] ~3–4 hours per playthrough.** A single run through one path should run **3–4 hours.**
- **[LOCKED] Freshness via *layering*, not size.** The game **doesn't need to be huge** — each stage stays
  fresh by **introducing a new enemy** (or a new *combination*) on top of the ones you know
  (`ENEMIES.md` §1 progressive introduction). New pairings = new problems, cheaply.
- **[LOCKED] Win = beat Phil.** The run ends at the final boss, **Phil** (`BOSSES.md` §5.1).
- **Replay** comes from **branching** (below) — different paths, endings, and enemy mixes each run.

---

## 1a. Premise & the "lead economy" — **[LOCKED core]** (drives pacing)

*(Full narrative gets its own doc later; captured here because it drives stage pacing.)*
- **[LOCKED]** After a **cinematic opening**, **Phil steals a magic pencil that draws things to life.** The
  **main character chases Phil** to recover it and stop the coming chaos — Phil's goal is to **draw his army
  into the world.**
- **[LOCKED] Setting = a real NorCal road trip.** **Phil escapes from Lincoln High (Lincoln, CA)** and
  **runs to San Francisco** — the whole game travels **from the Sacramento area to SF.** This gives the
  backdrops real **biodiversity**: suburbs → **rolling hills & farmland** → an **airport** and a
  **lake/delta** → **coastal hills** → the **city.**
- *(Origin flavor: the game grew from two friends passing a drawing back and forth in high school — which is
  literally Phil's draw-it-to-life pencil, tying the title and mechanic together. → `STORY.md` later.)*
- **[LOCKED] The lead economy = why enemies stay sparse early.** Phil **lacks the Holy Sharpener**, so the
  pencil has **limited lead** — he can only draw **so many** enemies. Counts stay low and **grow as you
  progress** — the in-fiction justification for the layering ramp (§4).
- **[LOCKED] Finale reveal:** at the rooftop, Phil's **monologue** (menacing laughter) reveals he still has
  the pencil and has now **FOUND the Holy Sharpener** — so his army is no longer rate-limited (the
  escalation). His vow: **"bring 2D chaos to this 3D planet"** (the game's tagline). The **sharpen-window is
  literally him using the sharpener** — invulnerable while drawing, open the ~3–5s he sharpens.
- **[LOCKED] Phil finale mechanic:** by the final fight the **pencil is dull.** Phil is **invulnerable while
  he still has lead** (busy drawing his army). **When he runs out of lead he must stop and *sharpen* the
  pencil** — a **3–5s self-stun** where he's **open and bleeding** — **that's your window to burst him down.**
  Cycle: **draw (invuln) → run dry → sharpen (vulnerable 3–5s) → repeat.** (Full spec in `BOSSES.md` §5.1.)
- **[LATER]** full story beats → `STORY.md`.

---

## 1b. Opening cinematic & tutorial — **[LOCKED core]**

**Opening cinematic** — a series of **~20-second picture clips** (still hand-drawn frames) with the
creator's **own voice-over** (recorded via their audio interface). Beats:
1. *"In the beginning, there was just **this**."* — shows drawings on the page.
2. **Phil escapes and captures the pencil.**
3. **Phil hunts the Holy Pencil Sharpener** — he only has so much **lead** before it runs out.
4. **"Your mission: defeat Phil."** → loads into the game.
- Assets: the still picture clips + a VO track. **[ITERATE]** number of clips, exact script/timing.

**In-game tutorial** — the **first ~10 seconds** of play teach the basics: **dodge** and **attack in every
direction**. Then it hands off to normal play.
- **[LOCKED] Weapons need no tutorial** — they're simple; you **learn each by using it** as you pick it up.
  The long campaign gives the player room to ramp up skills naturally.

---

## 1c. Mechanic intro vignettes — **[LOCKED core]** (the teaching device)

- **[LOCKED]** At the **start of every stage after the very first**, a **brief 3–5s vignette** plays that
  **demonstrates the new enemy/mechanic** the stage will test — enemies acting it out so the player *sees*
  what's going on and what beats it. Examples:
  - a **Head-Thrower** lobs a head-grenade that kills nearby enemies;
  - a Head-Thrower **fastballs** a grenade and a **Bat enemy swats it away**, launching it to explode.
- **[LOCKED]** The **very first stage gets only a light vignette** — the **dancing Zebra punches a regular
  enemy** to demo the **punch** (the only mechanic there). Full vignette plan → **`VIGNETTES.md`**.
- **[LOCKED] Weapon-availability ramp:** the first stages hand out **only basic melee** (swords, clubs); each
  new stage **introduces its mechanic via the vignette, then lets the matching weapons spawn.** So it's
  **teaching → tools → test** every stage, and it pairs with the boss (e.g. airport teaches bat+grenade →
  Helicopter boss).
- **[LOCKED] The economy is a second-half reveal:** **money/coins and the whole monkey economy** (Monkey
  Merc, Monkey Tamer, Monkey Boss, Pickpocket) **do NOT appear in the first half** (~Areas 1–2) — they debut
  **~Area 3 onward.** The game keeps introducing new systems the entire way, so it always feels fresh.
- Assets: a short scripted vignette per introduced mechanic (**reuses** enemy/weapon art). **[ITERATE]** the
  full vignette list (one per new mechanic/weapon).

---

## 2. Run structure — **[LOCKED]**

**[LOCKED] Areas → stages → branches.** A run is a **branching tree** grouped into **~4 areas + finale**,
**~2 stages per area** (~**8–10 stages** on a path). Stages are **meatier** (~20–25 min each) to land
**3–4 hrs** — length comes from **stage depth + branching**, not stage count.

```
        ACT 1 ──► ACT 2 ──►  ACT 3  ──►  ACT 4  ──► FINALE
        [1a]      [2a]        [3a]        [4a]
          \      /   \       /   \       /   \
           [1b]      [2b]        [3b]        [4b]      ── Phil ──► win
          /      \   /       \   /       \   /
        (branch chosen by performance/exit/secret — GAMEPLAY_LOOP §7)
```

- **[LOCKED]** Each **act ends in a main boss** that gates the branch into the next act.
- **[LOCKED]** A single run sees a **subset** of the tree; the full tree holds more stages than any one path
  (that's the replay).
- **[ITERATE]** exact stage count within 10–12, act widths, how wide branching gets, path tuning to 3–4 hrs.

---

## 3. Stage anatomy — **[PROPOSED]**

A typical stage:
1. **Scroll + encounters** — waves of the stage's enemy mix in the 2.5D lane (`GAMEPLAY_LOOP.md`).
2. **Optional catch-up miniboss** — injected if you're clearing too fast (`BOSSES.md` §4).
3. **A branch fork** — a physical exit and/or a graded outcome (§5).
4. **Act-end stages add a main boss.**

- **[LOCKED] Per-stage loot pool** is **constrained to that stage** (`WEAPONS.md` §4 / `ENEMIES.md` §1).
- **[ITERATE]** wave counts, encounter pacing, how long a stage runs, mid-stage checkpoints.

---

## 4. Enemy-layering schedule — **[PROPOSED]** (the freshness engine)

New types **debut** act by act, then **recombine** with everything prior. (Roster from `ENEMIES.md`.)

| Act | Debuts (new) | Now in the mix | Main boss (branch gate) |
|---|---|---|---|
| **1** | Regular Melee, Zombie, Swarmer | basics + fodder | *(miniboss only)* |
| **2** | Anti-Aircraft, Head-Thrower, Snapper | + rocks, grenades, sword-snap | e.g. **Tank** |
| **3** | Ground Smasher (zoner), Sniper, Heavy | + lane-denial, anti-jump, bruiser | e.g. **Colossus** |
| **4** | Arm-Ripper, Gatling Gunner, Ninja, Monkey Tamer, Flying Monkey | + guns, teleports, summons, air | e.g. **Helicopter / Gatling Guy / Monkey Boss** |
| **Finale** | — | everything | **Phil** |

- **[PROPOSED]** "Big version" minibosses (`BOSSES.md` §1) of already-seen enemies sprinkle through later
  acts to re-test old foes at higher stakes.
- **[ITERATE]** exact debut stage per enemy; which boss gates which branch; per-stage rosters.

### 4.1 Acts & themes — **[LOCKED core]** (~2–3 stages each, act-end boss gates the branch)

Route: **Lincoln → Rocklin → Roseville → Sacramento → Dixon → Marin → San Francisco** — a real NorCal drive.
Goal = **visible progression & diversity**: suburb → mall → city → airport → hills → small town → bridge → the city.

| Area | Real route | Theme / set pieces | Enemy ramp |
|---|---|---|---|
| **1 — Placer Suburbs & Mall** | Lincoln → Rocklin → Roseville | **Lincoln High** + suburbs (**Sandwich Bros** fight, car/bus hazard), Rocklin, the **Roseville Galleria** mall (**department-store** area boss) | tier 0–1 → intro tier-2 |
| **2 — Sacramento & Airport** | Sacramento → Sac. airport | medium city, **Victorian old-town housing** (clear sky), then the **Sacramento Airport** set piece | + tier-2/3 |
| **3 — Hills, Causeway & Dixon** | hills → Davis causeway → Dixon | rolling hills → **Yolo-style causeway** (platforming) → **farm** (Monkey Boss) → **Dixon boss rush** (first big wall, **big Arm-Ripper**) | + tier-3; Sniper, Flying Monkey, Arm-Ripper |
| **4 — Vallejo to the City** | Vallejo → bay → Marin → SF | **Vallejo** (Six-Flags coasters) → **bay causeway to Marin** → **Golden Gate Bridge** → **San Francisco** (skyscraper) | full roster; **Gatling Gunner, Ninja** debut |
| **Finale** | San Francisco | dull pencil → **sharpen-window** Phil fight (§1a) | everything |

- **[LOCKED] Traffic hazard (Act 1):** cars & school buses drive through the lane — **dodge to avoid damage.**
  **[ITERATE]** do they also hit enemies (usable hazard)?
- **[LATER]** exact stage list per act, which boss caps each act, per-stage enemy mixes, branch forks,
  per-act hazards, and parody-brand naming.

---

## 5. Branching — goals → ending → path — **[LOCKED approach]** (from `GAMEPLAY_LOOP.md` §7)

- Branch signal is a **flexible mix**: **physical exit** + **performance grade** + **secret conditions**.
- Branch **effect** ranges from light (fewer/more spawns, a better/worse starting weapon) to a **different
  next stage** — chosen per fork.
- **[ITERATE]** each fork's concrete recipe; how many endings; whether some branches are "harder path,
  better reward."
- **Feeds `UI.md` §5:** the **results/grade screen** and the **branch-reveal** on stage transition.

---

## 6. Death, continues & length — **[LOCKED]**

- **[LOCKED] Checkpoints + limited continues.** A **checkpoint** within/between stages and a **limited
  number of continues** before game-over — forgiving enough to respect a 3–4 hr run without being free.
- **[ITERATE]** checkpoint frequency (per stage? per act?); how many continues; heal/recovery between
  stages; whether branching state persists on continue; any cost to continuing.

---

## 7. Environments / themes — **[PROPOSED]**, your ideas

All areas are locked (§4.1) along the **Lincoln → Rocklin → Roseville → Sacramento → Dixon → Marin → SF**
route: **Placer Suburbs & Mall → Sacramento & Airport → Hills & Dixon → Marin/Golden Gate/SF → Phil.**
Detailed per-area art/population lives in **`AREAS.md`** (Area 1 fully locked). Each needs a background theme
(parallax backdrop for the top band; lane floor below) + ambient actors + hazards. **[ITERATE]** individual
stage backdrops within each area.

---

## 7b. Endless Mode — **[LOCKED core]** (extra mode)

- A separate **survival mode**: **starts from zero** (base difficulty) and **never lets up** — it **spawns
  more enemies whenever only 2 remain on screen**, scaling difficulty as it goes.
- Uses the full enemy roster; doubles as a great **playtest sandbox** and replay hook.
- **[ITERATE]** the scaling curve; does it layer in tiers/bosses over time; scoring/leaderboard; do campaign
  economy/weapon rules apply; is it purely endless-until-death.

## 7c. Environmental detail, ambient actors & hazards — **[LOCKED core]** (per theme)

Each theme is dressed with **area-specific ambient sprites, terrain, and hazards** so every backdrop feels
alive and *plays* differently:

- **Ambient actors (people & animals), themed to the area** — some pure background, some interactive/hazard:
  - **Airport:** ground crew **marshalling / bringing in planes**, luggage carts, taxiing planes.
  - **Rolling hills:** a **cow that blocks your path**, grazing animals, farm props.
  - (Each theme gets its own set — suburb pedestrians, mall shoppers, SF city crowds, etc.)
- **Terrain that funnels the player:** **ponds / puddles**, obstacles (the cow), fences — **constrict the
  play space into smaller areas**, changing the fight moment-to-moment and forcing new positioning.
- **Environmental hazards, per theme** (on top of the dressing):
  - Suburbs: **cars & school buses** (§4.1).
  - Airport: **taxiing planes / jet blast** *(ITERATE)*.
  - Hills: pond/puddle funnels, animals in the way.
  - Mall / City: escalators, traffic, etc. *(ITERATE)*.
- **[LOCKED] Design goal:** fully-fledged, lived-in stages so the world reads as a real place Phil's chaos
  is spilling into — and varied terrain keeps encounters fresh across the 3–4 hrs.
- **Assets:** per-theme **ambient-actor sprites** (people + animals), **hazard sprites/anims**, **terrain
  features** (ponds, obstacles). → `ASSET_MANIFEST.md`.
- **[ITERATE]** the exact ambient/hazard/terrain set per theme; which are decorative vs. damaging vs. blocking.

## 8. Asset needs → feeds `ASSET_MANIFEST.md`
- **Per theme:** background + parallax layers, lane floor, set dressing, stage hazards.
- **Transition/branch screens** (`UI.md` §5): branch-reveal map, results/grade, act cards.
- **Boss arenas** (`BOSSES.md` §2): fixed-room backdrops.

---

## 9. Decisions — status

**Resolved (now [LOCKED]):** ~10–12 stages/path across ~4 acts + finale; **balanced** stage length/count;
**checkpoints + limited continues.**

**Act themes locked (§4.1):** Suburbs → Mall → The Journey → Big City → Phil (~2–3 stages each). **Endless
Mode** added (§7b).

**Next / [ITERATE]:** per-fork branch recipes, exact per-stage rosters & backdrops, which boss caps each
act, checkpoint/continue specifics, parody-brand naming.
