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

### 1.1 Backdrop, framing & sky — **[LOCKED]**
- **Time of day:** **late morning → early afternoon (~11:30–3)** — the "leaving school / lunch" hour. Sun
  high and bright.
- **Sky:** clear **California blue** with only the **occasional wispy cloud** (CA has almost no clouds).
- **[LOCKED] Framing:** the **bottom ~50–60% of the screen is the scene** (play band + sidewalk + the
  houses/trees forming the back wall); the **top ~40% is sky** that **doubles as the backdrop for the HUD**
  (health, meter, money, etc.). Focus stays on the action; only a sliver of sky shows above the rooftops.
  *(Per-area: indoor themes swap this top backdrop for a themed one — the HUD band is always there.)*
- **Parallax (light):** far — sky + rare wispy cloud + a hint of rolling-hill horizon; mid — **houses &
  tall trees as the back wall**; near — **sidewalk + road** (the play band) with foreground props.

### 1.2 Ambient actors (people & animals)
- **People:** fleeing **suburban civilians**, a **mail carrier**, **kids on bikes**, a jogger — background,
  scatter when the fight nears.
- **Animals:** a **dog** (barks/runs), maybe a **cat** on a fence, **birds**.
- **[LOCKED] Signature ambient — the Lincoln High Zebra:** as you leave the school, a **dancing zebra** (the
  school's Fighting-Zebras mascot) **hops around** in the background — a fun Easter-egg actor.
- **[ITERATE]** which are pure decoration vs. reactive (flee) vs. hittable clutter.

### 1.3 Trees, houses, props & funnels — **[LOCKED]**
- **Trees:** lots of **mulberry and older, tall trees** typical of the area — **tall, mostly trunk with a
  little foliage down low**; a few **skinny / smaller** trees scattered. They tower over the scene.
- **Houses:** form the **backdrop / back wall.** You can **hop up onto the sidewalk** (**not** onto lawns) and
  get almost to the trees/houses — they're the **back edge** of the play space, a little sky above them.
- **Props:** parked cars, trash cans, fire hydrants, mailboxes, hedges, picket fences, lawn signs, porches.
- **Funnels:** parked cars / hedges **narrow the lane** into tighter fighting pockets in spots.

### 1.4 Hazards
- **[LOCKED] Traffic:** **cars & school buses** drive down the road — **dodge to avoid damage** (`STAGES.md`
  §4.1). **[ITERATE]** do they also flatten enemies (usable hazard)?

### 1.5 Enemies present
- **Tier 0–1 only:** **Zombie** (T0), **Regular Melee** (T1), **Swarmer** (T1b, in pods), **Anti-Aircraft**
  (T1a) appearing later in the act. **[ITERATE]** exact debut per stage.

### 1.6 Route, stages & boss — **[LOCKED]**
- **[LOCKED] Route:** follows the **real back way from Lincoln High to the fast food off old Highway 65 in
  Lincoln, CA** — old-town / semi-rural suburban character along the way (art reference for the backdrop).
- **Stage 1** opens **at Lincoln High** (Phil's escape; **dancing zebra** mascot, §1.2) → **suburb streets /
  the old Hwy 65 back way**; **Stage 2** continues toward the restaurant; the act **ends OUTSIDE Sandwich Bros.**
- **[LOCKED] Fast food = "Sandwich Bros"** (our made-up brand — no real trademarks). The **boss fight is
  outside** the restaurant.
- **[LOCKED] Boss = Phil draws a "bigger-than-normal person"** — a **big-version Tier-1** (Regular Melee at
  ~2× scale, `BOSSES.md` §1): the first boss-scale taste and first on-screen proof of the pencil.
  - **Solo:** one big Tier-1. **2-player:** **two** + a **miniboss.**
- **[ITERATE]** the Act-1 branch fork, exact stage count, specific old-Hwy-65 landmarks.

### 1.7 Act 1 asset list (→ `ASSET_MANIFEST.md`)
Backdrop layers (clear blue sky + rare wispy cloud, hill-horizon hint, houses, street) · **trees (mulberry
+ tall older trees, mostly trunk; a few skinny/small)** · ambient people (fleeing civilian, mail carrier,
kid+bike, jogger) · animals (dog, cat, birds) · props (parked car, trash can, hydrant, mailbox, hedge,
picket fence, porch, lawn sign) · **hazard vehicles** (car, school bus) · **Lincoln High** exterior ·
**dancing Zebra mascot** · **"Sandwich Bros"** building + signage/mascot · **big-version Tier-1** boss
(reuses Regular Melee at ~2× scale) · funnel obstacles.

**Act 1 LOCKED.** (Only the Act-1 branch fork and specific old-Hwy-65 landmarks remain as smalls.)
**Ready for Act 2 (the Mall) whenever you are.**
