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
  **main character chases Phil** across the city to recover it and stop the coming chaos — Phil's goal is to
  **draw his army into the world.**
- **[LOCKED] The lead economy = why enemies stay sparse early.** Phil **lacks the Holy Sharpener**, so the
  pencil has **limited lead** — he can only draw **so many** enemies. Counts stay low and **grow as you
  progress** — the in-fiction justification for the layering ramp (§4).
- **[LOCKED] Phil finale mechanic:** by the final fight the **pencil is dull.** Phil is **invulnerable while
  he still has lead** (busy drawing his army). **When he runs out of lead he must stop and *sharpen* the
  pencil** — a **3–5s self-stun** where he's **open and bleeding** — **that's your window to burst him down.**
  Cycle: **draw (invuln) → run dry → sharpen (vulnerable 3–5s) → repeat.** (Full spec in `BOSSES.md` §5.1.)
- **[LATER]** full story beats → `STORY.md`.

---

## 2. Run structure — **[LOCKED]**

**[LOCKED] Acts → stages → branches.** A run is a **branching tree of stages** grouped into **~4 acts +
finale**; any single playthrough passes through **~10–12 stages** (one path). **Balanced pacing** — medium
stage length *and* count (each stage ~15–20 min) — to land **3–4 hrs.**

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

### 4.1 Act 1 — The City (Area 1) — **[LOCKED core]**
Opens after the cinematic. **Tier 0–1 enemies only** (lead economy — few and weak). **Two stages** make
Area 1, then branching widens:
- **Stage 1 — US Suburbs:** houses, streets, sidewalks; you fight on the road and walkways.
  - **[LOCKED] Traffic hazard:** **cars and school buses drive through the lane**; **dodge out of their way
    to avoid damage.** **[ITERATE]** do they also hit/kill enemies (a usable hazard) or only threaten the player?
  - **Boss:** a fight at a **parody fast-food joint** — a **made-up brand, no real trademarks** (`[LATER]` name).
- **Stage 2 — Downtown:** denser city core; caps Area 1. From here **branching widens.**
- **[ITERATE]** Downtown's boss/miniboss, Area 1's branch fork, exact enemy mix per stage.

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

Each stage/act needs a **background theme** (parallax backdrop for the top band; the lane floor below).
**Act 1 locked:** **Suburbs** (houses/streets/sidewalks, car+bus traffic hazard) → **Downtown** (dense city
core). **Dump more theme ideas** for Acts 2–4 and we'll map them. Assets: per-theme background + parallax
layers + lane floor + hazards (e.g. Act 1's cars/buses).

---

## 8. Asset needs → feeds `ASSET_MANIFEST.md`
- **Per theme:** background + parallax layers, lane floor, set dressing, stage hazards.
- **Transition/branch screens** (`UI.md` §5): branch-reveal map, results/grade, act cards.
- **Boss arenas** (`BOSSES.md` §2): fixed-room backdrops.

---

## 9. Decisions — status

**Resolved (now [LOCKED]):** ~10–12 stages/path across ~4 acts + finale; **balanced** stage length/count;
**checkpoints + limited continues.**

**Next: your stage themes (§7)** — dump settings and we'll map them to acts. Then per-fork branch recipes,
exact per-stage rosters, checkpoint/continue specifics.
