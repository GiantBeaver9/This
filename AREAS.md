# this.l — Area-by-Area Detail (Act 1 → Finale)

> **Purpose:** fully lock each area's *look and contents* — backdrop, parallax layers, ambient actors
> (people, animals, trees, clouds), props, terrain funnels, hazards, enemy set, and boss — so nothing is
> added late (late additions can disturb rendering/classes). Structure/pacing live in `STAGES.md`; this is
> the **art & population** pass. We hammer these down **one act at a time.**
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[ITERATE]** flesh out · **[LATER]** parked.

---

## 0. Performance & art direction — **[LOCKED target]**

- **[LOCKED] Lightweight first.** The game must **run great on modest hardware** (e.g. a 3050/2090), not
  only high-end (5090). **Pixel-art 2.5D** keeps it cheap: **2D sprites, simple parallax, no heavy 3D/shaders.**
- **[LOCKED] Design goal:** *more lightweight and more fun* — a player on a low-end rig has just as good a
  time as one on a high-end rig.
- **Implications we honor per area:** bounded on-screen sprite counts (enemy cap already 8 + swarms),
  **layered parallax backdrops** (a few scrolling layers, not dense geometry), **atlas-friendly** ambient
  props, and effects within the bullet-hell-safe/readable budget (`VFX.md`).
- **[ITERATE → art specs]** canonical **resolution, sprite pixel sizes, palette, animation fps/frame counts,
  atlas/naming** — lock these before mass asset generation (they keep every asset consistent).

---

## 1. Act 1 — Suburbs (Lincoln, CA) — **[PROPOSED, let's hammer it]**

*Tier 0–1 enemies only (lead economy). ~2–3 stages, ends at a parody fast-food joint. Traffic hazard.*

### 1.1 Backdrop & parallax
- **Setting:** California **suburban streets** — single-story stucco houses, garages, driveways, front lawns,
  sidewalks, the road (the lane floor).
- **Parallax layers (light):** far — **rolling-hill horizon + drifting clouds + California sky**; mid —
  **houses, fences, power lines, trees**; near — **sidewalk/road** (play band) with foreground props.
- **Ambient sky:** **clouds** (slow parallax), **birds**, sun; **[ITERATE]** time of day (midday? sunset?).

### 1.2 Ambient actors (people & animals)
- **People:** fleeing **suburban civilians**, a **mail carrier**, **kids on bikes**, a jogger — background,
  scatter when the fight nears.
- **Animals:** a **dog** (barks/runs), maybe a **cat** on a fence, **birds**.
- **[ITERATE]** which are pure decoration vs. reactive (flee) vs. hittable clutter.

### 1.3 Props & terrain funnels
- **Props:** parked cars, **trash cans, fire hydrants, mailboxes, hedges, picket fences, lawn signs, porches.**
- **Funnels:** parked cars / hedges **narrow the lane** into tighter fighting pockets in spots.
- **Trees:** suburban **oaks + California palms** (mid-layer + occasional foreground).

### 1.4 Hazards
- **[LOCKED] Traffic:** **cars & school buses** drive down the road — **dodge to avoid damage** (`STAGES.md`
  §4.1). **[ITERATE]** do they also flatten enemies (usable hazard)?

### 1.5 Enemies present
- **Tier 0–1 only:** **Zombie** (T0), **Regular Melee** (T1), **Swarmer** (T1b, in pods), **Anti-Aircraft**
  (T1a) appearing later in the act. **[ITERATE]** exact debut per stage.

### 1.6 Stages & boss
- **[PROPOSED]** Stage 1 opens **at/near Lincoln High** (where Phil escapes) → into the **suburb streets**;
  Stage 2 goes **deeper suburb / a park**; the act **ends at a parody fast-food joint** for the boss.
- **[PROPOSED] Fast-food boss:** a **made-up brand** (no real trademarks) — e.g. a mascot-themed miniboss or
  a "big version" enemy. **[ITERATE]** which boss, the restaurant's name/mascot, the branch fork here.

### 1.7 Act 1 asset list (→ `ASSET_MANIFEST.md`)
Backdrop layers (sky+clouds, hills, houses, street) · trees (oak, palm) · ambient people (civilian, mail
carrier, kid+bike, jogger) · animals (dog, cat, birds) · props (parked car, trash can, hydrant, mailbox,
hedge, fence, mailbox, porch, lawn sign) · **hazard vehicles** (car, school bus) · fast-food building +
mascot + interior · funnel obstacles.

**→ Your turn: what am I missing or want changed for Act 1?** (specific trees/clouds/animals, the fast-food
mascot, whether we open at Lincoln High, time of day.) Then we lock it and move to Act 2.
