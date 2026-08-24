# this.l — Gameplay Loop

> **Scope of this document:** the *gameplay loop* only — what the player does moment-to-moment,
> the enemies, the goals, the UI, and enemy logic/routing. Story, art direction, meta-progression,
> audio, monetization, and full weapon/enemy rosters are **out of scope** here and get their own docs.
>
> **Legend:**
> - **[LOCKED]** — decided, stated directly by design.
> - **[PROPOSED]** — a concrete suggestion to react to; not final.
> - **[LATER]** — an intentional slot we fill in a future pass.

---

## 1. One-line pitch of the loop

A 2.5D side-scrolling **beat-'em-up × bullet-hell** where you punch stick-figures to death,
**loot their corpses for decaying weapons**, and bank kills into a **screen-clearing sniper special** —
while a stage silently grades your performance and **branches** based on how you finished it.

---

## 2. The core loop (moment-to-moment)

The repeating cycle the player runs hundreds of times per stage:

```
        ┌─────────────────────────────────────────────────────────┐
        │                                                         │
        ▼                                                         │
  READ THE SPACE ──► POSITION (dash, no iframes) ──► ATTACK ──► KILL
  (melee threats +        weave the bullet-hell        punch /      enemy
   bullet patterns)       lanes, close distance        weapon       dies
        ▲                                                             │
        │                                                             ▼
        │                                                      LOOT DECISION
        │                                                   (grab dropped weapon?
        │                                                    build special meter?)
        │                                                             │
        └──────────────── SPEND (weapon decays / special banked) ◄────┘
```

**[LOCKED] The tension that makes the loop work:** the dash has **no i-frames**. It is
*repositioning, not invincibility.* You cannot dash *through* danger — you dash *around* it.
This is the seam where "beat-'em-up" (get in close and hit) meets "bullet-hell" (the space
between you and the enemy is full of danger you must physically route around).

---

## 3. Space & camera — the 2.5D playfield

**[LOCKED]**
- **2.5D**, side-scrolling, viewed **from the front**. The Z-axis (depth) is **simulated** —
  characters can move "toward"/"away" as well as left/right, but we render them flat/front-on.
- **Core gameplay lives in the bottom ~half of the screen.** That band is the walkable ground plane.
- Player and enemies are **semi-small** sprites — this keeps many actors + dense bullet patterns
  readable at once, which the bullet-hell side demands.

```
 ┌───────────────────────────────────── screen ─────────────────────────────────────┐
 │  BACKGROUND / SKYBOX  (parallax, non-interactive)                                  │
 │  ······································· HUD lives up here (§6) ···················· │
 │                                                                                    │
 ├──────────────────────────────── horizon line ─────────────────────────────────────┤
 │                                                                                    │
 │   ░░░░░  PLAYFIELD (bottom ~half) ░░░░░                                             │
 │      Z (far)  →  enemies, bullets, pickups, the fight                              │
 │   ▓▓▓▓▓  X = left/right scroll   ·   Z = shallow depth band  ▓▓▓▓▓                  │
 │   Z (near) ────────────────────────────────────────────────────────────────────── │
 └────────────────────────────────────────────────────────────────────────────────────┘
```

**[PROPOSED] Depth rules to keep it readable:**
- The Z band is **shallow** (a few "rows" deep), not a full 3D field. Enough to dodge a straight
  shot by stepping up/down; not so much that the bullet-hell becomes a top-down twin-stick game.
- **Collision is Z-aware:** a bullet on a far row does not hit a player on a near row. This is what
  makes "route around the bullets" physically true instead of cosmetic.
- **[LATER]** Exact number of depth rows / whether Z is continuous or snapped to lanes.

---

## 4. What the player does / interacts with

### 4.1 Base kit — the Human

**[LOCKED]**
- **Punch** — the default attack. Basic melee; against an armed pickup it's replaced by the weapon.
- **Dodge dash** — a burst of movement. **No i-frames.** Pure repositioning to escape enemy
  proximity and slip between bullet lanes.
- **Special meter** — fills from combat (see §4.3). Cannot be spent unless **full**.

### 4.2 Weapons — looted from corpses, and they decay

**[LOCKED]** Enemies are stick-figure variants. **An enemy's *level* determines which random ability/
weapon it drops on death.** Weapons are temporary — every one is a *spend-it-before-it's-gone* resource.
This keeps the player cycling back to fists and back into danger to re-loot.

Confirmed weapons (the roster expands **[LATER]**):

| Weapon | Source (diegetic) | How it's used | Runs out when… |
|---|---|---|---|
| **Sword** | enemy dies, **head is gone** → pick up | melee, bigger reach/damage than fists | **5–10 hits**, then it decays |
| **Shotgun** | enemy dies → pick up | shoot, then **cock** between shots; a **piece of the enemy (spine) ejects** each cock | spine is spent (ammo = visible spine segments) |
| **Boomerang** | enemy dies → pick up | **throw infinitely**… but on hitting an enemy it **bounces off and you lose it**; that enemy is **stunned 2s** | you lose it the moment it connects (retrieve/re-loot) |

**[LOCKED] Design throughline:** the **ammo/decay indicator is the enemy's own body** — spine segments
for the shotgun, the headless corpse for the sword. Resource management is *diegetic*, read off the
weapon itself rather than a number. (More weapons follow this same "made of the corpse" language. **[LATER]**)

### 4.3 The Special — sniper time-stop

**[LOCKED]**
- Press special **only when the meter is full**.
- Player pulls a **sniper rifle**; **time slows**.
- The shot is a **ricochet headshot** — it bounces between enemies, **one-shot headshot-killing each**
  enemy it touches as it caroms across the screen.
- **Risk/reward:** enemies killed by the sniper **drop no weapons — they just die.** You trade the loot
  economy for a guaranteed multi-kill / panic-button clear.
- **[LOCKED] Bosses are immune to the sniper shot.** The special cannot be used to skip boss fights.

**[PROPOSED] Meter fill sources** (tune **[LATER]**): melee kills fill most; weapon kills fill some;
taking hits fills nothing (or drains). Intent: reward aggressive fists-first play, since that's the
risky option the loot loop pushes you away from.

---

## 5. Enemies

### 5.1 Identity
**[LOCKED]** All enemies are **variants of stick figures**, tiered by **level**. An enemy's level sets
(a) how hard it is and (b) **which weapon/ability it drops.** Higher-level corpses = better loot.

### 5.2 Type taxonomy — **[PROPOSED]** starter set (full roster **[LATER]**)

Because the game is beat-'em-up *and* bullet-hell, enemies split along that axis:

| Role | Behavior | Pressures the player to… |
|---|---|---|
| **Rusher** | closes to melee range, swings | keep moving; don't get surrounded |
| **Gunner** | holds distance, fires straight/aimed shots (the bullet-hell contribution) | route around lanes, close the gap |
| **Spreader / Patterner** | stationary-ish, emits fixed bullet patterns | read the pattern, thread it on the Z band |
| **Bruiser** | slow, high-HP, armored; drops the best loot | commit hits / spend a weapon; a sniper target |
| **Swarm** | weak, many, fast | prime sniper-ricochet fodder; punish greedy looting |

**[PROPOSED] Level ↔ loot mapping** is deliberately *random within a level band* (as stated: "gets a
**random** ability from the enemy"), so the player can't perfectly plan which weapon they'll get — they
adapt to what drops.

### 5.3 **[LATER]**
- Exact stat curves, per-type bullet patterns, elite/miniboss variants.
- Full boss designs (only the **sniper-immune** rule is locked so far).

---

## 6. UI / HUD

**[LOCKED principle]** The **bottom half is sacred** — it's the playfield. HUD lives in the **top band**
and stays minimal so dense bullet patterns never fight the UI for the player's eyes.

**[PROPOSED] HUD elements (top band):**
- **Health** — top-left.
- **Special meter** — prominent (top-center or top-left under health); clearly reads **empty → full**,
  and visibly "armed" the instant it's usable. This is the single most important gauge because the
  special is a timing decision.
- **Current weapon + its decay state** — but prefer **diegetic** display on the sprite/weapon itself:
  - Sword: **hits-remaining** shown by wear/glow, or a tiny pip counter near the weapon.
  - Shotgun: **spine segments** *are* the ammo readout — no separate number needed.
  - Boomerang: simply "in hand / in flight / lost."
- **Combo / performance feedback** — lightweight, because performance **drives branching** (§7).
  The player should sense they're doing well without a wall of text.

**[LATER]** Damage numbers on/off, minimap need (probably none — it's a lane), pause/menu, tutorial prompts.

---

## 7. Goals

### 7.1 Per-stage goal
**[LOCKED]**
- The game is **stage-based**.
- **You do not pick stages.** Where you go next is determined by **in-level branching** and **how you
  finished the level** ("depending on the ending to each level, it can unlock different paths … based on
  level performance and branching in level").

### 7.2 What "performance" and "ending" mean — **[PROPOSED]** (needs your ruling)
To make branching real, the game must *measure* something. Candidate signals to grade a run on:
- **Reached which exit** (branch chosen *inside* the level — a physical fork on the lane).
- **Kill style** (how many sniper-clears vs. fists vs. weapons — ties to the risk/reward economy).
- **Damage taken / no-hit segments.**
- **Speed / time.**
- **Secrets or optional rooms cleared.**

These feed a per-stage **"ending"** (e.g. clean clear vs. messy clear vs. secret exit) which unlocks a
**different next stage.** *This is the loop's macro layer and I'd like your call on which signals count.*

### 7.3 Session goal — **[LATER]**
Run length, win condition (final boss? endless? branching tree with multiple endings?), and any
meta-progression are out of scope for this pass.

---

## 8. Enemy logic & routing

This is where 2.5D + bullet-hell get technical. Kept as **[PROPOSED]** design intent; exact tuning **[LATER]**.

### 8.1 Movement space
- Enemies navigate the **same shallow Z-band + X-scroll** the player does.
- **Routing is toward *engagement range*, not toward the player's exact pixel:**
  - **Rushers** path to melee range, then commit a swing and back off/reposition (so they don't just
    pile into one square).
  - **Gunners** path to a **preferred standoff distance** and a **clear firing lane** (a Z-row the player
    is on or crossing), then hold and fire. They *retreat* if the player closes.

### 8.2 Spacing & anti-clumping — **[PROPOSED]**
Beat-'em-ups die when every enemy stacks on the player. Rules to prevent it:
- **Attack tokens:** only **N** enemies may be in "attacking" state at once; others circle/wait at
  standoff. Keeps fights readable and fair, classic beat-'em-up trick.
- **Z-spread:** enemies bias toward **different depth rows** so their shots come from varied lanes —
  this *is* the bullet-hell layer emerging from placement, not scripted patterns.
- **Soft separation** so bodies don't overlap into a blob.

### 8.3 Bullet-hell layer
- Gunner/Patterner fire is **Z-aware** (see §3): a bullet occupies a depth row; the player dodges by
  **stepping rows** and **dashing along X**, never by phasing through (no i-frames).
- **[PROPOSED]** Pattern density scales with enemy level and stage depth, not with raw enemy count, so
  branches that funnel you into "harder" stages *feel* harder via patterns, not just HP sponges.

### 8.4 Reactions to the player's kit
- **Boomerang** — a stunned enemy (2s) is visibly frozen; AI resumes cleanly after, no queued attacks.
- **Sniper special** — during time-slow, enemy logic runs at the slowed clock too (so it reads as *time*
  slowing, not the player speeding up); struck enemies die without dropping loot.
- **[LATER]** Whether enemies flee/panic at low numbers, target-priority nuances, boss AI.

---

## 9. Open decisions I need from you (highest-leverage first)

1. **Branching signals (§7.2):** which performance signals define a stage's "ending" and unlock paths?
   *(This is the biggest one — it's the difference between "levels in a row" and "a real branching game.")*
2. **Z-band shape (§3):** continuous depth, or a small fixed number of lanes/rows? Affects every dodge.
3. **Special meter fill rule (§4.3):** should melee kills fill it far faster than weapon kills, to push
   risky fists-first play?
4. **Attack-token count (§8.2):** how aggressive should fights feel — how many enemies swing at once?

Everything under **[LATER]** stays parked until you want to open it.
