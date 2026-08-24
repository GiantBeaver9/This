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
- **[PROPOSED] Carry = single slot.** You hold **one weapon at a time**; picking up a new one **replaces**
  the current (dropped/lost). Fists are the permanent fallback. *(Confirm — alternative is a 2-slot swap.)*
- **[PROPOSED] Pickup = auto on walk-over**, no grab animation (saves frames). *(Or a dedicated grab.)*

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

## 3. Proposed expansion roster — **[PROPOSED]**, pick what makes v1

Grounded in stick-figure anatomy, spread across roles so the loot pool stays varied. React with
keep/cut/tweak; we only build the ones you greenlight.

| # | Weapon | Corpse part | Role | Behavior sketch | Decay/ammo |
|---|---|---|---|---|---|
| A | **Bone Club** | femur / thick limb | heavy melee | slow, huge knockback; finisher **launches** | breaks after ~6–8 hits |
| B | **Bo-Staff / Spear** | long limb | spacing melee | long reach thrusts, low dmg, very safe | decays ~10–12 hits |
| C | **Rib Shield** | ribcage | defensive | hold to **block/reduce** damage; **shield-bash** finisher | cracks, shatters after absorbing X dmg |
| D | **Skull Bomb** | head | throwable AoE | lob an explosive skull; small blast radius | **1–3 charges**, then gone |
| E | **Sinew Whip** | sinew / gut | crowd melee | long **horizontal line** hit, tags several enemies | decays ~8 hits |
| F | **Chain-Heads** (nunchaku) | two heads on cord | flashy fast melee | rapid hits → **fills the meter fast** (combo engine) | decays fast (~12 quick hits) |
| G | **Bone Pistol** | arm | ranged | finisher fires a **single accurate shot** | small mag (~3–5 shots) |
| H | **Teeth Caltrops** | teeth | area denial | scatter a hazard patch; enemies crossing take dmg/slow | one patch per pickup |

**[PROPOSED] Role balance for v1:** keep at least one of each — a **heavy** (A), a **spacer** (B), a
**defensive** (C), a **throwable** (D), a **crowd** (E/whip), and a **meter-engine** (F). Guns (G) plus the
shotgun cover ranged. That's a tight, characterful ~8–10 weapon pool without over-scoping your art.

---

## 4. Tier ↔ enemy-level mapping — **[PROPOSED]**

| Tier | Drops from | Example weapons | Feel |
|---|---|---|---|
| **T1 common** | low-level stick figures | fists+ (nothing), boomerang, bo-staff | early, low commitment |
| **T2 uncommon** | mid enemies | sword, club, whip, chain-heads | the workhorses |
| **T3 rare** | high/elite enemies | shotgun, bone pistol, rib shield | powerful, scarcer |
| **T4 special** | minibosses / rare spawns | skull bomb, caltrops, one-offs | spice, situational |

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

## 6. Decisions I need

1. **Roster size (§3):** which of A–H are in for v1? (My rec: A, B, C, D, E, F, G — cut/keep as you like.)
2. **Carry rule (§1):** single-slot auto-swap (rec), or a 2-slot weapon swap?
3. **Pickup (§1):** auto on walk-over (rec), or a dedicated grab button/animation?
4. **Boomerang lost-weapon (§2.3):** does it lie on the ground to re-grab, or just vanish on hit?

**[LATER]:** exact durability numbers, tier drop rates, per-archetype loot restrictions, weapon-specific
finisher damage.
