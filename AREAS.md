# this.l — Area-by-Area Detail (Act 1 → Finale)

> **Purpose:** fully lock each area's *look and contents* — backdrop, parallax layers, ambient actors
> (people, animals, trees, clouds), props, terrain funnels, hazards, enemy set, and boss — so nothing is
> added late (late additions can disturb rendering/classes). Structure/pacing live in `STAGES.md`; this is
> the **art & population** pass. We hammer these down **one act at a time.**
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[ITERATE]** flesh out · **[LATER]** parked.

---

## 0. Performance & art direction — **[LOCKED target]**

- **[LOCKED] Lightweight first.** The game must **run great on modest hardware** (e.g. a GTX 1650 / RTX 3050), not
  only high-end (5090). **Pixel-art 2.5D** keeps it cheap: **2D sprites, simple parallax, no heavy 3D/shaders.**
- **[LOCKED] Design goal:** *more lightweight and more fun* — a player on a low-end rig has just as good a
  time as one on a high-end rig.
- **Implications we honor per area:** bounded on-screen sprite counts (enemy cap already 8 + swarms),
  **layered parallax backdrops** (a few scrolling layers, not dense geometry), **atlas-friendly** ambient
  props, and effects within the bullet-hell-safe/readable budget (`VFX.md`).
- **[LOCKED → `ASSET_MANIFEST.md` §0]** canonical **resolution (640×360), sprite sizes (48px), palette (32-color), fps (12), atlas/naming**
  are all pinned there — no longer open.

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
  §4.1). **[LOCKED, resolved]** they **do flatten enemies too** (a usable hazard): car **40**, bus **60**, both
  with knockdown, to player *and* enemies (`TUNING.md` §6.2). Bait an enemy into the lane on a telegraphed pass.

### 1.5 Enemies present — **[LOCKED] gradual intro**
- **Lincoln suburbs:** **Regular Melee (T1)** basics + the big-Tier-1 boss. Very light (the lead economy).
- **Galleria Mall:** **Swarmers (T1b) and Zombies (T0) debut** — the mall is where the roster starts filling.
- **[LOCKED] Slow build:** enemies introduce **gradually** by area (`ENEMIES.md` §6 is the debut authority) —
  **most types are in play by Area 3; the full roster completes in Area 4** (Ninja/Pickpocket at Vallejo,
  Boomergunner at Marin, Gatling/Ground Smasher at Golden Gate, Heavy on the SF streets), then
  difficulty just **ramps** from there.

### 1.6 Route, stages & boss — **[LOCKED]**
- **[LOCKED] Route:** follows the **real back way from Lincoln High to the fast food off old Highway 65 in
  Lincoln, CA** — old-town / semi-rural suburban character along the way (art reference for the backdrop).
- **Stage 1** opens **at Lincoln High** (Phil's escape; **dancing zebra** mascot, §1.2) → **suburb streets /
  the old Hwy 65 back way**; **Stage 2** continues toward the restaurant and **ends at the Sandwich Bros fight**
  (a **mid-act** boss, `ENCOUNTERS.md` Stage 2) — the act then continues through **Rocklin → the Galleria**,
  where **Burly caps Area 1** (§1.9). *(The Sandwich Bros fight ends the Lincoln/suburb *segment*, not the act.)*
- **[LOCKED] Fast food = "Sandwich Bros"** (our made-up brand — no real trademarks). The **boss fight is
  outside** the restaurant.
- **[LOCKED] Boss = Phil draws a "bigger-than-normal person"** — a **big-version Tier-1** (Regular Melee at
  ~2× scale, `BOSSES.md` §1): the first boss-scale taste and first on-screen proof of the pencil.
  - **Solo:** one big Tier-1. **2-player:** **two** + a **miniboss.**
- **[ITERATE]** the Area-1 stage pacing, exact stage count, specific old-Hwy-65 landmarks.

### 1.8 Rocklin (transition) — **[LOCKED]**
- The Lincoln→Roseville stretch passes through **Rocklin** — more suburban / old-town NorCal, connective
  tissue between the suburbs and the mall — **older strip-mall + low storefronts, wider streets, sparse trees** (a plainer, grayer suburb than Lincoln). A brief connective stretch, no set piece.

### 1.9 Roseville Galleria — the Mall (Area 1 finale) — **[LOCKED core]**
- **[LOCKED] Intro vignette (§`STAGES.md` 1c):** a **security guard shoots a Tier-1 enemy → it turns into a
  Zombie → the zombie grabs the guard → they fall over → the camera pans to the player.** Teaches the
  **zombie-on-shot + grab** mechanic right before the mall floods the player with them.
- **Interior, frantic tone:** **terrified shoppers peek and cower** in storefront windows; **random people
  run through**, fleeing the enemies — panic atmosphere, the mall is chaos.
- **Backdrop:** mall interior — storefronts (windows full of cowering shoppers), atrium, tile floor, kiosks,
  planters, benches, escalators, **skylights** (the top-band "sky" becomes the **skylight/ceiling** here).
- **Enemies (debut):** **swarm-heavy** — **Swarmers and Zombies** arrive; swarms **chase the fleeing people**
  then turn on you. The **headshot→Zombie** rule is shown here only via the **scripted guard vignette**
  (the guard shoots a T1, `VIGNETTES.md`) — the **player** has no headshot-capable weapon yet (Area-1 pool is
  Sword + Boomerang only, `TUNING.md` §6.1), so *player-driven* headshot-zombies begin in Area 2 when guns unlock.
- **Terrain funnels:** kiosks, planters, benches, and escalators pinch the play space.
- **[LOCKED] Area 1 boss = Burly Macho Guy** (`BOSSES.md` §5.2) — a bruiser brawl **in the department store**
  (his ground-spikes + enemy-toss fit the confined space). **[ITERATE]** the stage pacing out of Area 1.
- **[ITERATE]** mall ambient set (mall cop? food-court props?), store signage (made-up brands), hazards.

### 1.10 Area 1 asset list (→ `ASSET_MANIFEST.md`)
**Suburbs:** backdrop layers (clear blue sky + rare wispy cloud, hill-horizon hint, houses, street) ·
**trees (mulberry + tall older trees, mostly trunk; a few skinny/small)** · ambient people (fleeing
civilian, mail carrier, kid+bike, jogger) · animals (dog, cat, birds) · props (parked car, trash can,
hydrant, mailbox, hedge, picket fence, porch, lawn sign) · **hazard vehicles** (car, school bus) ·
**Lincoln High** exterior · **dancing Zebra mascot** · **"Sandwich Bros"** building + signage/mascot ·
**big-version Tier-1** boss (reuses Regular Melee at ~2× scale) · funnel obstacles.
**Mall (Galleria):** mall-interior backdrop (storefronts + **cowering shoppers in windows**, atrium,
skylight ceiling) · **fleeing civilians** · kiosks/planters/benches/escalators (funnels) · made-up store
signage · **department-store boss** arena.

**Area 1 LOCKED.** (Smalls: stage pacing, Rocklin landmarks. Boss = Burly Macho Guy, §1.9.)

---

## 2. Area 2 — Sacramento & Airport (Sacramento → Sac. Airport) — **[LOCKED core]**

*Two stages: (1) **Sacramento Victorian downtown** — debuts the **Whip** (whip-pull vignette); (2) the
**Airport** — debuts **Head-Throwers** + the **Bat**/grenade, capped by the **Helicopter** boss.*

### 2.1 Sacramento — Victorian old town (stage 1) — **[LOCKED core]**
- **Backdrop:** a **medium-sized city** of **older Victorian-style housing** (Sacramento downtown-inspired) —
  ornate two-story homes, porches, bay windows, iron fences. **Clear sky, no clouds** (same framing: bottom
  scene / top HUD sky).
- **[LOCKED] Intro vignette (§`STAGES.md` 1c):** one **enemy uses a Whip to pull down another enemy** (showing
  the whip's **pull / crowd-control**), then they **spot the player and turn to attack.** Teaches the Whip.
- **[LOCKED] Debut: the Whip** weapon spawns here. **[ITERATE]** whether a dedicated **whip-wielding enemy**
  exists (the "enemy composed of another" — the cannibalize pattern) or the vignette uses existing enemies.
- **[LOCKED] Stage boss = the Colossus** (`BOSSES.md` §5.4 — the whip-to-dismantle giant); the Whip taught
  here is exactly what beats it.
- **Ambient:** fleeing pedestrians, a **streetcar / light-rail**, lamp posts, benches, newspaper boxes. **[ITERATE].**
- **Enemies:** the **tier-2 layer begins** (Snapper, etc.). **[ITERATE]** exact debut split.

### 2.2 Sacramento Airport — terminal & tarmac (stage 2)
- **[LOCKED] Intro vignette (§`STAGES.md` 1c):** entering the **terminal**, the player **sees the mechanic
  acted out** — enemies **throw head-grenades that hit planes in the air**, and a **Bat enemy swats a
  grenade into a small plane**, exploding it. Teaches **head-grenades + bat-reflect** before you use them.
- **[LOCKED] Debuts:** **Head-Throwers** (grenade-from-head), the **Bat** weapon (Area-2 drop pool onward,
  `TUNING.md` §6.1), and **Anti-Aircraft** (stone-throwers — seen **pelting the planes**).
- **[LOCKED] Grenades at the airport = the Helicopter boss-arena weapon-gate, NOT the general pool.** Grenades
  are otherwise **Area-3-gated** (`TUNING.md` §6.1); the exception is the **Helicopter arena**, which — like the
  Tank's grenade gate — has its **tier-1 adds drop bats + grenades** so the player has the exact two tools the
  boss needs (bat the heads back / lob grenades up). The general Area-2 stage pool has **bats but not
  grenades**; the grenade practice is inside the boss arena. (Resolves the AREAS-vs-TUNING drop-pool conflict.)
- **Backdrop:** terminal interior → **tarmac** with **taxiing planes**, ground crew **marshalling planes**,
  luggage carts. **Hazards:** taxiing planes / jet blast **[ITERATE]**.
- **[LOCKED] Area boss = the Helicopter** (`BOSSES.md` §5.5): the airport **taught bat + grenade**, and the
  Helicopter is beaten with exactly those (**bat its heads back / lob grenades up**). Teaching → tools → test.

### 2.3 Area 2 asset list (→ `ASSET_MANIFEST.md`)
Sacramento **Victorian houses** (2-story, porches, bay windows, iron fences) · city ambient (fleeing
pedestrians, **streetcar/light-rail**, lamp posts, benches, newspaper boxes) · **Whip** weapon pickup +
whip-pull vignette · (whip **boss/Colossus** arena) · airport **terminal interior** · **tarmac + taxiing
planes** · **ground crew + luggage carts** · **small planes** (vignette targets / hazard) · **Head-Thrower**
enemy · **Bat** weapon pickup · **Helicopter** boss · funnel props.

**[RESOLVED]** tier-2 split = Snapper (Sacramento) / Head-Thrower+AA (airport) per `ENEMIES.md` §6; the whip is
a weapon (no whip *enemy*); Area-2 bosses = Colossus (Sacramento) + Helicopter (airport); **taxiing-plane
hazard = 50 dmg + knockdown** (`TUNING.md` §6.2). Fully locked.

---

## 3. Area 3 — Hills, the Causeway & Dixon — **[LOCKED core]**

*Two parts: **(1) The Road** — rolling hills → Causeway/Davis → the **farm/ranch** (Monkey Boss); **(2)
Dixon** — a nearly-deserted mid-2000s small town run as a **boss rush** (exactly **4 minibosses** → 1 big boss, `ENCOUNTERS.md` Stage 8).*

### 3.1 Part 1 — The Road: hills → causeway → farm (water platforming) — **[LOCKED core]**
- **Backdrop:** golden **rolling hills & farmland**, then a **water area / causeway** — the Yolo-Causeway-style
  stretch **toward Davis**: open water, marsh, the elevated causeway; ending in **farm country** (the ranch).
- **[LOCKED] Platforming section:** the water stretch is **more platform-heavy** — **platforming between
  fights** as you cross the causeway/water. Adds variety while **reusing the same mechanics.**
- **[LOCKED] Sniper debuts here and punishes your jumps** (red dot → apex shot, `ENEMIES.md` §2.14) — so the
  platforming is **tense**: you must clear gaps without riding a jump to full apex while he's up.
- **[LOCKED] Combined intro vignette (§1c) — teaches TWO concepts at once:** two characters go for a **dime**;
  one **jumps really high** for it → the **Sniper shoots the jumper out of the air** → the **other grabs the
  dime** → **whistle** → an **enemy monkey appears and takes that character away.** Teaches
  **sniper-punishes-high-jumps** + **dime→whistle→monkey** together.
- **[LOCKED] Terrain funnels:** **ponds/puddles**, a **cow blocking the path**, plus the causeway's narrow platforms.
- **Ambient / animals:** cows, farm animals (goats, chickens), crows, barns, tractors, hay bales; marsh birds
  over the water.
- **Enemies:** **tier-3 layer** + **Monkey Tamer** + **Sniper (beret + big rifle)**; **Flying Monkey debuts
  during / at the end of the causeway run.** **[ITERATE]** debut split.

### 3.2 Farm / Ranch — Monkey Boss (end of Part 1) — **[LOCKED core]**
- **Setting:** a **farm / ranch** with **cows and animals all over.**
- **[LOCKED] Boss = the Monkey Boss** (`BOSSES.md` §5.7): **you vs. the Tamer, racing to grab the dimes first**
  — win dimes to field your own monkeys (only your mercs damage him); lose the race and he summons his.
  **The animals are obstacles** that block/funnel your dime grabs.
- **[ITERATE]** animal-obstacle behavior, arena layout.

### 3.2a Part 2 — Dixon: boss rush (the first big wall) — **[LOCKED core]**
- **Setting:** the small town of **Dixon, mid-2000s** (pre-2010) — tiny and **nearly deserted** (almost
  nobody around): main street, water tower, feed store, old storefronts, a quiet square. The **emptiness is
  the mood.**
- **[LOCKED] Shorter but brutal — a difficulty spike.** Dixon is a **short** level **packed with heavy-duty
  enemies** — the **first big wall** of the game.
- **[LOCKED] Boss rush:** **4 minibosses** (big-version enemies of the crew you've met) → **1 big boss** (`ENCOUNTERS.md` Stage 8).
- **[LOCKED] Debut + big boss = Arm-Ripper.** The **Arm-Ripper** debuts here, and the **big-version
  Arm-Ripper** is **Dixon's big boss.**
- **[LOCKED] The 4 minibosses** are pinned in `ENCOUNTERS.md` Stage 8 (big Snapper, big Head-Thrower, big
  Flying Monkey, big Arm-Ripper elite) → then the **big Arm-Ripper** boss. Dixon's vignette = the Arm-Ripper
  arm-rip demo (`VIGNETTES.md`). **[ITERATE]** only Area-3 fine pacing.

### 3.3 Area 3 asset list (→ `ASSET_MANIFEST.md`)
Rolling-hills/farmland backdrop · **water/causeway** backdrop + **platforms** + marsh + marsh birds · **Dixon**
town (mid-2000s: main street, water tower, feed store, old storefronts, quiet square) · **cows + farm animals**
(goats, chickens, crows — obstacles) · **ponds/puddles** (funnels) · tractors, hay bales, fences · **dime**
pickup + **whistle** cue · **Sniper** (beret + large rifle) · **Flying Monkey** · **Monkey Boss** + enemy
monkeys · **Arm-Ripper** (+ **big-version** Dixon boss) · tier-3 enemies · boss-rush miniboss set.

**[ITERATE]** the Dixon miniboss set, animal-obstacle rules, stage pacing.

---

## 4. Area 4 — Vallejo → the Bay → Marin → Golden Gate → SF — **[LOCKED core]**

*The home stretch: **Vallejo** (Six-Flags roller-coaster backdrop) → a causeway/bridge run **across the bay
to Marin** → the **Golden Gate Bridge** → **San Francisco** → the **Phil** finale. Debuts the last heavy
hitters — **Gatling Gunner** and **Ninja**.*

### 4.1 Vallejo — the amusement park (Six Flags)
- **Backdrop:** **Vallejo** with a **Six-Flags-style amusement park** — **roller coasters**, midway, ferris
  wheel, game booths. **[ITERATE]** park set pieces, any coaster hazard.
- **[LOCKED] Debuts:** the **Ninja** (teleport shuriken) and the **Pickpocket** (`ENEMIES.md` §2.16 — steals
  your coins; kill it for **2× back**). **[ITERATE]** a vignette for each.
- The **Gatling Gunner debuts at the Golden Gate (Stage 11)**, not Vallejo (`ENEMIES.md` §6, `ENCOUNTERS.md`
  Stage 11) — it does **not** appear in Vallejo/Marin.
- **[LOCKED] Vallejo boss = the Tank** (`BOSSES.md` §5.3) — **military is nearby** (Travis-AFB flavor), so a
  tank rampaging the park fits; beat it by dropping **grenades** (from the weapon-gated adds) in the hatch — 2 drops.

### 4.1b Marin County & the redwoods — **[LOCKED core]**
- **Backdrop:** **Marin County** — a **redwood forest** (towering redwoods, ferns, filtered light, drifting
  mist), winding forest paths, heading out toward the bay/bridge.
- **[LOCKED] Debut: the Boomergunner** (`ENEMIES.md` §2.17) — enemies who **throw Boomerang Guns** at you.
- **[LOCKED] Marin/redwoods boss = a Boomergunner** (big-version) at the end.
- **[ITERATE]** redwoods hazards (falling logs? mist?), a Boomergunner vignette.

### 4.2 Across the bay to Marin, then the Golden Gate (Area-4 cap) — **[LOCKED core]**
- **[LOCKED] Bay causeway run** across to **Marin County** (water platforming again), then the **Golden Gate
  Bridge** crossing — **Stage 11 of 13**, the **Area-4 capping boss** (Gatling Gun Guy). The **SF streets
  (Stage 12)** and the **Salesforce rooftop (Stage 13 / Phil)** follow it as the finale approach + finale.
- **[LOCKED] Golden Gate boss = the Gatling Gun Guy** (`BOSSES.md` §5.6): he **shoots you like crazy** with a
  **barrage every ~5 seconds.** A **"BARRAGE INCOMING" warning** flashes on screen; you must **hide behind the
  cars** on the bridge — **everything caught in the open gets eviscerated** (enemies included). Cover + timing.
- **[LOCKED] Enemy: the Ground Smasher (Zoner) debuts here** — its **lane shockwaves** thread the bridge amid
  the barrage chaos.
- **[ITERATE]** Marin look, bridge fog/wind, car-cover layout, the barrage pattern.

### 4.3 San Francisco → the Tower → Phil finale (last level) — **[LOCKED core]**
- **[LOCKED] The SF slog:** the **longest combat stage, ~18 min** (the top of the ~15–18 min band, `STAGES.md`
  §2 — the authority on minutes-per-stage) with the **full roster** — **every enemy type** coming at you. You
  **follow the SF trolley / cable-car path** back and forth through the city.
- **[LOCKED] Trolley hazard:** the **trolley comes through and plows straight down the middle** — it **can't
  tell friend from foe** and **flattens whatever's in its lane** (enemies *and* you). **[LOCKED] No cars in SF**
  (too much going on) — the **trolley is the signature hazard** that caps the game.
- **[LOCKED] The Heavy ("Bold") debuts on the SF streets** — a fresh tanky wall to keep the long slog alive.
- **[LOCKED]** at the end, an **elevator up Salesforce Tower** (one of SF's tallest) → **fight Phil on the roof.**
- **[LOCKED] Phil finale** (`BOSSES.md` §5.1, `STAGES.md` §1a): dull pencil → **sharpen-window** (invulnerable
  until he runs dry and must sharpen, ~3–5s open) → greatest-hits **re-summons** of earlier bosses.
- **[LOCKED] Shifting winds / tower sway:** the tower **sways back and forth** — during the elevator ride /
  Phil's intro cutscene you **see things slowly shift** one way then the other, **foreshadowing** it. In the
  fight this causes **slight slippage** (you slide with the sway), and **falling off = instant death.** Real
  positioning tension.
- **[ITERATE]** sway timing/strength, edge & fall zones, which bosses Phil reprises, exact staging.

### 4.4 Area 4 asset list (→ `ASSET_MANIFEST.md`)
Vallejo **amusement park** (roller coasters, ferris wheel, midway) · bay **causeway/bridge + platforms** ·
**Golden Gate Bridge** · **San Francisco** cityscape + **skyscraper** · **Gatling Gunner** + **Ninja**
enemies · **Tank** (Vallejo) · **Boomergunner** (Marin) · **Gatling Gun Guy** (Golden Gate) · **Phil** boss
(top-hat zombie + pencil, sharpen anim).

**[ITERATE]** Area 4 stage split, bridge hazards, Phil staging.
