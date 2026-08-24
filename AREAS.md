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

## 1. Area 1 — Placer Suburbs & Mall (Lincoln → Rocklin → Roseville Galleria) — **[LOCKED]**

*Lincoln suburbs (basic tier-1, Sandwich Bros) → Rocklin → the **Roseville Galleria** mall (swarms & zombies
debut, frantic tone) → **department-store** area boss. Lead economy keeps it light early.*

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

### 1.5 Enemies present — **[LOCKED] gradual intro**
- **Lincoln suburbs:** **Regular Melee (T1)** basics + the big-Tier-1 boss. Very light (the lead economy).
- **Galleria Mall:** **Swarmers (T1b) and Zombies (T0) debut** — the mall is where the roster starts filling.
- **[LOCKED] Slow build:** enemies introduce **gradually**; by ~**3 areas in, all types are in play**, then
  difficulty just **ramps** from there.

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
- **[ITERATE]** the Area-1 branch fork, exact stage count, specific old-Hwy-65 landmarks.

### 1.8 Rocklin (transition) — **[ITERATE]**
- The Lincoln→Roseville stretch passes through **Rocklin** — more suburban / old-town NorCal, connective
  tissue between the suburbs and the mall. **[ITERATE]** specific look/landmarks, any set piece.

### 1.9 Roseville Galleria — the Mall (Area 1 finale) — **[LOCKED core]**
- **[LOCKED] Intro vignette (§`STAGES.md` 1c):** a **security guard shoots a Tier-1 enemy → it turns into a
  Zombie → the zombie grabs the guard → they fall over → the camera pans to the player.** Teaches the
  **zombie-on-shot + grab** mechanic right before the mall floods the player with them.
- **Interior, frantic tone:** **terrified shoppers peek and cower** in storefront windows; **random people
  run through**, fleeing the enemies — panic atmosphere, the mall is chaos.
- **Backdrop:** mall interior — storefronts (windows full of cowering shoppers), atrium, tile floor, kiosks,
  planters, benches, escalators, **skylights** (the top-band "sky" becomes the **skylight/ceiling** here).
- **Enemies (debut):** **swarm-heavy** — **Swarmers and Zombies** arrive; swarms **chase the fleeing people**
  then turn on you; **headshot a Tier-1 and it becomes a Zombie** (the headshot economy on display).
- **Terrain funnels:** kiosks, planters, benches, and escalators pinch the play space.
- **[LOCKED] Area 1 boss:** the **department store** fight. **[ITERATE]** which boss (big-version vs. bespoke),
  the branch fork out of Area 1.
- **[ITERATE]** mall ambient set (mall cop? food-court props?), store signage (made-up brands), hazards.

### 1.7 Area 1 asset list (→ `ASSET_MANIFEST.md`)
**Suburbs:** backdrop layers (clear blue sky + rare wispy cloud, hill-horizon hint, houses, street) ·
**trees (mulberry + tall older trees, mostly trunk; a few skinny/small)** · ambient people (fleeing
civilian, mail carrier, kid+bike, jogger) · animals (dog, cat, birds) · props (parked car, trash can,
hydrant, mailbox, hedge, picket fence, porch, lawn sign) · **hazard vehicles** (car, school bus) ·
**Lincoln High** exterior · **dancing Zebra mascot** · **"Sandwich Bros"** building + signage/mascot ·
**big-version Tier-1** boss (reuses Regular Melee at ~2× scale) · funnel obstacles.
**Mall (Galleria):** mall-interior backdrop (storefronts + **cowering shoppers in windows**, atrium,
skylight ceiling) · **fleeing civilians** · kiosks/planters/benches/escalators (funnels) · made-up store
signage · **department-store boss** arena.

**Area 1 LOCKED.** (Smalls: branch fork, department-store boss pick, Rocklin landmarks.)

---

## 2. Area 2 — Sacramento & Airport (Sacramento → Sac. Airport) — **[LOCKED core]**

*Medium city (Victorian old town, clear sky) → the airport terminal & tarmac. Debuts **Head-Throwers** + the
**Bat**; teaches bat+grenade via a vignette; caps with the **Helicopter** boss.*

### 2.1 Sacramento — Victorian old town
- **Backdrop:** a **medium-sized city** of **older Victorian-style housing** (Sacramento-inspired) — ornate
  two-story homes, porches, bay windows, iron fences. **Clear sky, no clouds** (same framing: bottom scene /
  top HUD sky).
- **Ambient:** fleeing city pedestrians, maybe a **streetcar / light-rail**, urban props (lamp posts,
  hydrants, benches, mailboxes, newspaper boxes). **[ITERATE]** specific set.
- **Enemies:** the **tier-2 layer begins.** **[ITERATE]** exact debuts here vs. at the airport.

### 2.2 Sacramento Airport — terminal & tarmac
- **[LOCKED] Intro vignette (§`STAGES.md` 1c):** entering the **terminal**, the player **sees the mechanic
  acted out** — enemies **throw head-grenades that hit planes in the air**, and a **Bat enemy swats a
  grenade into a small plane**, exploding it. Teaches **head-grenades + bat-reflect** before you use them.
- **[LOCKED] Debuts:** **Head-Throwers** (grenade-from-head) and the **Bat** weapon — and grenades + bats
  **spawn** here so the player can practice the exact tools the boss needs.
- **Backdrop:** terminal interior → **tarmac** with **taxiing planes**, ground crew **marshalling planes**,
  luggage carts. **Hazards:** taxiing planes / jet blast **[ITERATE]**.
- **[LOCKED] Area boss = the Helicopter** (`BOSSES.md` §5.5): the airport **taught bat + grenade**, and the
  Helicopter is beaten with exactly those (**bat its heads back / lob grenades up**). Teaching → tools → test.

### 2.3 Area 2 asset list (→ `ASSET_MANIFEST.md`)
Sacramento **Victorian houses** (2-story, porches, bay windows, iron fences) · city ambient (fleeing
pedestrians, streetcar?, lamp posts, benches, newspaper boxes) · airport **terminal interior** · **tarmac +
taxiing planes** · **ground crew + luggage carts** · **small planes** (vignette targets / hazard) ·
**Head-Thrower** enemy · **Bat** weapon pickup · **Helicopter** boss · funnel props.

**[ITERATE]** tier-2 debut split (city vs airport), city ambient specifics, plane-hazard rules, Area 2 branch
fork. **Ready for Area 3 (Hills & Dixon) whenever you are.**
