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

A 2.5D side-scrolling **beat-'em-up / spacing brawler** where you punch stick-figures to death,
**loot their corpses for decaying weapons**, and bank kills into a **screen-clearing sniper special** —
across a **linear** run of stages from the suburbs to San Francisco.

> **[LOCKED] Genre note:** "bullet-hell" was **inspiration, not a literal requirement** — there are **no
> dense projectile patterns / pattern-emitters.** Threats are **melee + short-range shooters + hazards +
> the Z-band dodge game.** Where older sections below say "bullet-hell," read it as **"keep threats
> readable, dodge via the Z-band."** The final enemy roster (`ENEMIES.md`) is authoritative over the early
> §5/§8 taxonomy here.

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
- Player and enemies are **semi-small** sprites — this keeps many actors + short-range projectiles
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

**[LOCKED] Depth rules:**
- The Z band is **continuous and semi-deep** — free analog up/down movement, *not* snapped to lanes
  (too snappy) and *not* ultra-shallow (not enough room for interesting bullet patterns). A middle
  band with real dodging space that still reads as a side-scroller, not a top-down twin-stick.
- **Collision is Z-aware:** a bullet at one depth does not hit a player at a different depth. This is
  what makes "route around the bullets" physically true instead of cosmetic — and continuous depth
  makes threading and near-misses **analog and skill-expressive.**
- **[LATER]** Exact band height in world units; sprite depth-scaling (how much smaller a far actor
  draws); whether shadows/ground markers are needed to read a character's exact Z.

---

## 4. What the player does / interacts with

### 4.1 Base kit — the Human

**[LOCKED]**
- **Punch** — the default attack. Basic melee; against an armed pickup it's replaced by the weapon.
- **Dodge dash** — a burst of movement. **No i-frames.** Pure repositioning to escape enemy
  proximity and slip between bullet lanes.
- **Special meter** — fills from combat (see §4.3). Cannot be fired until charged.

**[LOCKED] Response feel:** punches are **immediate** (no wind-up) — fists are the always-ready,
lowest-commitment option. Every looted **weapon has a short warm-up** before it fires/swings
(**~0.25s** baseline to aim/ready, varies per weapon). Picking up a weapon trades raw responsiveness
for power/range: fists for reaction, weapons for when you've bought yourself space.

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
- **[LOCKED] The special can't skip a boss.** A boss **negates/dodges** it above **10% HP**; at **≤10%**
  a prompt lets any character's special **execute** the boss (`BOSSES.md` §1). No mid-fight cheese.

**[LOCKED] Meter fill — rewards fast, fists-first aggression:**
- **Fists fill fastest** — roughly **30 punch-hits** of combat to earn a charge (**+3.34 pts/hit**, `TUNING.md` §2.4).
- **Weapon/item *hits* fill ~half as fast** — **+1.67 pts/hit**, about **double** the effort of fists. *(Per
  **hit**, same event as fists — not per kill; this is the LOCKED resolution in `TUNING.md` §2.4, superseding
  the older "weapon kills" phrasing.)*
- **Rapid kills multiply the fill.** Chaining hits quickly builds a **combo**; a **combo popup** flashes
  on screen — `1 HIT!`, `2 HIT!`, … — and hitting ~**15 hits quickly** really surges the meter.
- **[LOCKED] Charge tiers AMPLIFY (they don't bank extra shots):** **yellow** (1 fill) → **blue** (2 fills /
  "double fill") → **green** (max). **Each fill = +10% damage** while charged, and the **special itself
  scales**: the sniper ricochet kills **up to 15 enemies at a single fill, doubling to 30 at a double fill.**
- **Instant-fill pickup:** a **killed Sniper enemy drops a rifle that fills the meter instantly**
  (`ENEMIES.md` §2.14) — a burst reward for aggressively downing him.
- **[LATER]** Exact multiplier curve; whether blue/green **bank multiple** sniper shots vs. fire **one
  stronger** shot; whether taking damage drains the meter or just breaks the combo.

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
and stays minimal so the busy lower playfield never fights the UI for the player's eyes.

**[PROPOSED] HUD elements (top band):**
- **Health** — top-left.
- **Special meter** — prominent (top-center or top-left under health); clearly reads **empty → full**,
  and visibly "armed" the instant it's usable. This is the single most important gauge because the
  special is a timing decision.
- **Current weapon + its decay state** — but prefer **diegetic** display on the sprite/weapon itself:
  - Sword: **hits-remaining** shown by wear/glow, or a tiny pip counter near the weapon.
  - Shotgun: **spine segments** *are* the ammo readout — no separate number needed.
  - Boomerang: simply "in hand / in flight / lost."
- **Combo / performance feedback** — lightweight combo popups so the player senses they're doing well
  (cosmetic; performance no longer changes the path — §7).

**[LATER]** Damage numbers on/off, minimap need (probably none — it's a lane), pause/menu, tutorial prompts.

---

## 7. Goals

### 7.1 Goal — **[LOCKED] linear campaign**
- The game is **stage-based and LINEAR** — a single fixed path Lincoln → San Francisco, ending at **Phil**
  (`STAGES.md`, `AREAS.md`). **No level branching.**
- **[LOCKED] Branching is CUT** (was performance→ending→alternate-path). **Replay comes instead from the 4
  playable characters** (each plays differently, `CHARACTERS.md`) **and Endless Mode** (`STAGES.md` §7b) —
  not from divergent stage paths.

### 7.2 Performance feedback (cosmetic only) — **[PROPOSED]**
Performance can still be *shown* (combo popups, an optional end-of-stage grade for score/bragging), but it
**no longer changes the path** — everyone plays the same stage order. **[LATER]** whether an end-of-stage
grade exists at all.

### 7.3 Session goal — **[LATER]**
Win condition = **beat Phil** (linear campaign); Endless Mode is separate. Meta-progression and any
meta-progression are out of scope for this pass.

---

## 8. Enemy logic & routing

This is where 2.5D + bullet-hell get technical. Kept as **[PROPOSED]** design intent; exact tuning **[LATER]**.

### 8.1 Movement space
- Enemies navigate the **same shallow Z-band + X-scroll** the player does.
- **Routing is toward *engagement range*, not toward the player's exact pixel:**
  - **Rushers** path to melee range, then commit a swing and back off/reposition (so they don't just
    pile into one square).
  - **Shooters** (short-range only, per `ENEMIES.md` §1 — they must **close in**) advance to their **fire
    range** and a **clear Z-row**, then fire; they don't camp at long standoff. *(No keep-away, no sniping —
    `ENEMIES.md` is authoritative over this early sketch.)*

### 8.2 Spacing & anti-clumping — **[LOCKED] rule + [PROPOSED] mechanics**
**[LOCKED]** Up to **8 enemies** actively pursue the player at once. But **hard separation** is
enforced: **no two enemies occupy the same space**, and they stay **far enough apart that you can
never be hit by multiple enemies in one overlapping hitbox.** Getting swarmed is a *positioning*
threat (surrounded, cut off), never a cheap-shot pileup where three hits land as one.
- **[PROPOSED] Standoff rings:** pursuers that can't reach a legal attack slot **circle at a standoff
  distance** and wait for an opening instead of crowding in.
- **[PROPOSED] Z-spread:** enemies bias toward **different depths** so shots arrive from varied angles —
  the bullet-hell layer emerging from placement, not scripted patterns.

### 8.3 Bullet-hell layer
- Gunner/Patterner fire is **Z-aware** (see §3): a bullet occupies a depth row; the player dodges by
  **stepping rows** and **dashing along X**, never by phasing through (no i-frames).
- **[PROPOSED]** Pattern density scales with enemy level and stage depth, not with raw enemy count, so
  later areas *feel* harder via denser enemy mixes and hazards, not just HP sponges.

### 8.4 Reactions to the player's kit
- **Boomerang** — a stunned enemy (2s) is visibly frozen; AI resumes cleanly after, no queued attacks.
- **Sniper special** — during time-slow, enemy logic runs at the slowed clock too (so it reads as *time*
  slowing, not the player speeding up); struck enemies die without dropping loot.
- **[LATER]** Whether enemies flee/panic at low numbers, target-priority nuances, boss AI.

---

## 9. Decisions — status

**Resolved (now [LOCKED] above):**
1. **Branching (§7)** — **CUT.** The game is a **linear** campaign; replay = the 4 characters + Endless.
2. **Z-band (§3)** — continuous, semi-deep.
3. **Special meter (§4.3)** — fists fill fastest (~30 hits), weapon *hits* ~half; rapid combos multiply fill;
   yellow → blue → green charge tiers; on-screen combo popup.
4. **Aggression (§8.2)** — up to 8 pursuers with hard separation, so no stacked multi-hits.

**Still parked ([LATER]):** exact tuning numbers (Z band height, multiplier curve, per-weapon warm-up),
whether charge tiers bank multiple sniper shots vs. one stronger shot — each pinned in the relevant system
doc, not here.
