# this.l — Weapon Roster

> **Scope:** every weapon the Human can loot — behavior, decay/ammo economy, how it interacts with the
> combo/finisher rule, and per-weapon **asset needs**. Player *animation pipeline* lives in `PLAYER.md`.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[LATER]** parked.

---

## 1. Weapon system rules — **[LOCKED] core, [PROPOSED] where noted**

- **[LOCKED] Corpse-sourced.** Weapons come off dead **stick figures** — each is made of a **body part**
  (head, spine, limb-bone…). The corpse *is* the ammo/durability readout (diegetic, no HUD number).
- **[LOCKED] Everything decays.** No weapon is permanent; each has a **hits / shots / durability** budget,
  then it's gone and you're back to **fists**. This keeps you cycling into danger to re-loot.
- **[LOCKED] Enemy level → random loot.** A dead enemy's **level** sets the **tier** of the random weapon
  it can drop. Higher-level stick figures drop rarer / longer-lasting weapons. **[PROPOSED]** tier table
  in §4.
- **[LOCKED] Sniper-killed enemies drop nothing** (from the special — risk/reward).
- **[LOCKED] Ranged weapons fire on the combo *finisher*** (per `PLAYER.md`): mid-combo they bludgeon at
  fist strength; the real shot/effect lands on the strong 3rd hit and advances that weapon's ammo economy.
- **[LOCKED] Carry = single slot**, fists as the permanent fallback.
  - **Empty-handed:** walking over a drop **auto-picks** it.
  - **Already armed:** auto-pickup is suppressed; **tap the swap key** to take the weapon on the ground —
    and your **current weapon disappears** (destroyed, not dropped). No ground-hoarding or juggling.

---

## 2. Confirmed weapons — **[LOCKED]** (full spec)

### 2.1 Sword — *from a head-gone corpse*
- **Type:** melee (real swing kit — full directional + air attacks).
- **Behavior:** bigger reach & damage than fists; the go-to upgrade.
- **Decay:** **5–10 connecting hits**, then it shatters/decays. **[PROPOSED]** exact number by tier.
- **Diegetic readout:** blade **visibly wears/chips** as hits deplete; final hit it breaks.
- **Assets:** sword-in-hand idle/walk/jump · directional swing set (side/up/down + air) · wear states
  (fresh → chipped → breaking) · break VFX.

### 2.2 Shotgun — *spine = ammo*
- **Type:** ranged, but **melee'd through the combo**; **fires a shell on the finisher**.
- **Behavior:** finisher = **blast** (short-range spread, big damage / knockback). After firing, the Human
  **cocks it** and a **spine segment ejects** — the **remaining spine = shots left.**
- **Ammo:** number of **spine segments** (e.g. 4–6). When spine is spent, gun is gone.
- **Diegetic readout:** the **spine magazine** shrinks segment-by-segment; no HUD ammo counter needed.
- **Warm-up:** slight aim before the blast (per weapon warm-up rule).
- **Assets:** shotgun-in-hand idle/walk · bludgeon combo (reuses fist body holding gun) · **finisher:
  fire + cock + spine-eject** · muzzle flash · spine-segment bit VFX · empty/discard.

### 2.3 Boomerang — *bent limb-bone*
- **Type:** thrown.
- **Behavior:** **throw infinitely** — on a **miss it returns** to hand; on **hitting an enemy it bounces
  off and you lose it**, and that enemy is **stunned for 2s**. So it's infinite *only if you keep missing*;
  landing a hit trades the weapon for a 2-second stun (a setup tool, not a damage tool).
- **Decay:** not durability-based — you **lose it on the first enemy hit** (retrieve the dropped one, or
  re-loot). **[PROPOSED]** the dropped boomerang lies on the ground to pick back up.
- **Assets:** boomerang-in-hand · **throw** anim · spinning in-flight sprite (reused as projectile) ·
  return arc · stun VFX on the struck enemy · grounded pickup sprite.

---

## 3. Full roster — listed to iterate

> **Theming note:** weapons are **function-first** — we define what each *does*; flavor is secondary. Only
> the locked sword / spine-shotgun / boomerang lean on the corpse-part gag; the rest don't have to.
>
> **[ITERATE]** = captured, we flesh it out next.

**Locked & spec'd (§2):** Sword · Shotgun (spine ammo) · Boomerang

### 3.1 Pistol — *ranged, single-target* **[ITERATE]**
- **Less damage but more precise** than the shotgun. **Shotgun = crowd control; pistol = the 1-v-1 tool.**
- Fires on finisher (per §1 rule).
- **[ITERATE]** mag size, decay, accuracy/aim model.

### 3.2 Grenade / Bomb — *thrown, physics-based* **[ITERATE tuning]**
Enemy-dropped. Already richly designed:
- **Thrown by repeatedly tapping the attack button** — **press count sets the throw:**
  - **Fewer presses → high lob** (short, arcing); **more presses → fast, flat throw.**
- A **ground marker shows where it first bounces** (aim telegraph).
- **Always bounces 3×, then explodes** (AoE).
- **A fast throw knocks down all enemies near the flight path** — the projectile's travel line is itself dangerous.
- **[ITERATE]** blast radius & damage; whether lob vs. fast changes the blast; charges per pickup.

### 3.3 Ball & Chain — *heavy melee* **[ITERATE]**
- Windup, wide arc/reach, big knockback. **[ITERATE]** swing behavior, decay, momentum/spin mechanic.

### 3.4 Whip — *long crowd melee* **[ITERATE]**
- Long horizontal reach, tags multiple enemies in a line. **[ITERATE]** multi-hit, any pull/grab, decay.

### 3.5 Staff — *magic caster* **[LOCKED core]**
- **Element is set at pickup — randomly one of three: Ice, Fire, Lightning.** A given staff stays that
  one element for its whole life. The **finisher casts** the element's effect:
  - **Ice** — crowd control: **freezes** enemies, **less damage**. Lockdown tool.
  - **Lightning** — **stun damage** + **slows** enemies. Tempo/control.
  - **Fire** — **burns** enemies (damage over time). **Signature interaction:** burning a **grenade enemy**
    (the stick figure that pulls off its own head to throw at you) makes it **start blinking, then after
    ~2s BOOM** — a small blast that **kills the player** if caught in it. Great damage, but it turns that
    enemy into a walking bomb you must not be next to. *(Grenade enemy specced in `ENEMIES.md`.)*
- **[ITERATE]** whether the arrow direction aims/shapes the cast or it's fixed; cast warm-up; staff decay
  (casts before it breaks); does fire's chain-explosion also damage other enemies?
- **[SUPERSEDES]** the earlier "each direction = a different spell" — element is now fixed per pickup.

### 3.6 Gatling Gun — *heavy risk/reward* **[LOCKED core]**
- **No ammo count** — it doesn't deplete per shot.
- **Slow combo** — its attack cadence is noticeably **slower** (heavy weapon).
- **Finisher = an auto-kill headshot** — a guaranteed kill on a normal enemy.
- **No i-frames — the player is vulnerable** through the slow finisher: the guaranteed kill is paid for in
  **exposure**, so throwing it out in a crowd gets you hit.
- **[ITERATE]** how it's eventually lost (no ammo → overheat? time limit? N finishers?); does the headshot
  hit one target or pierce/chain; warm-up/spin-up.
- **[SUPERSEDES]** the earlier "20–32 shots" — no ammo tracking now.

### 3.7 Monkey Merc — *summon, costs currency (see §3.8)* **[ITERATE]**
- Dropped by a **monkey stick figure**, but you can **only take it if you hold a dime (10¢).**
- Summons a **monkey merc that fights for you for 20 seconds, or until it's killed.**
- **[ITERATE]** monkey attacks/AI, how easily it dies, whether more than one can be active, cooldown.

### 3.8 Currency system — **[LOCKED core], [ITERATE] scope** *(cross-cuts `UI.md`)*
- Enemies **sometimes drop wallets**; each = **1 cent**. Money is **shown in the UI.**
- **10¢ = a dime**, the cost to take/summon a **Monkey Merc** (§3.7).
- **[ITERATE]** money only for monkeys, or a broader economy (between-stage shop? other buys?); does it
  persist across stages/runs; wallet drop rate; any cap. If it grows past monkeys it earns its own
  `ECONOMY.md`.

*More weapons welcome — this list is meant to grow; we iterate each `[ITERATE]` into a full §2-style spec.*

---

## 4. Tier ↔ enemy-level mapping — **[PROPOSED]**

| Tier | Drops from | Example weapons | Feel |
|---|---|---|---|
| **T1 common** | low-level stick figures | boomerang, pistol | early, low commitment |
| **T2 uncommon** | mid enemies | sword, whip, ball & chain | the workhorses |
| **T3 rare** | high/elite enemies | shotgun, staff, gatling gun | powerful, scarcer |
| **T4 special** | minibosses / specific spawns | grenade, **monkey merc** (needs a dime) | spice, situational |

**[LOCKED]** the *specific* weapon within a tier is **random** (you adapt to what drops). **[LATER]** exact
tier contents, drop rates, whether some weapons only come from specific enemy archetypes.

---

## 5. Per-weapon asset summary → feeds `ASSET_MANIFEST.md`

For **each greenlit weapon**:
- **Melee weapon:** in-hand idle/walk/jump · directional swing kit (side/up/down + air) · wear/decay
  states · break/discard VFX.
- **Ranged weapon:** in-hand idle/walk · bludgeon combo (fist body + weapon) · **finisher fire** anim ·
  muzzle/projectile VFX · ammo-readout bits (e.g. spine segments) · empty/discard.
- **Throwable/utility:** in-hand · throw/deploy anim · in-flight/placed sprite · effect VFX · pickup or
  spent state.

Shared: every dropped weapon needs a **ground pickup sprite** and a **decay/break puff**.

---

## 6. Status & next step

**Resolved (now [LOCKED]):** single-slot carry (auto-pick when empty; swap-key destroys the old weapon
when armed); function-first theming; the roster list above; grenade press-to-throw physics; the currency
system core (wallets → cents → dime → monkey merc).

**Iterating one at a time (together).** Done so far: **Staff** (element set at pickup: ice/fire/lightning,
fire's grenade-enemy chain-explosion) and **Gatling** (no ammo, slow combo, auto-kill finisher, no
i-frames). Remaining: **Pistol → Grenade tuning → Whip → Ball & Chain → Monkey Merc**, then keep adding.

**[LATER]:** durability numbers, tier drop rates, per-archetype loot restrictions, finisher damage,
whether currency grows into a full economy.
