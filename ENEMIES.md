# this.l — Enemy Roster

> **Scope:** the stick-figure enemies — identity, level system, per-type behavior, how they route/attack,
> what they drop, and per-enemy **asset needs**. Boss designs live in `BOSSES.md`; enemy *routing rules*
> extend `GAMEPLAY_LOOP.md` §8.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[ITERATE]** flesh out next · **[LATER]** parked.

---

## 1. Enemy system rules

- **[LOCKED] Most enemies share ONE stick-figure body — their *ability* defines them, not their looks.**
  The majority are the same thin silhouette; what makes a "type" is its **moveset/ability**, not a new
  character. **Huge asset win:** one base body + rig, with **ability-specific attack sets layered on top.**
  A few signature enemies get unique bits, but the default is *shared-body-plus-ability*.
- **[LOCKED] Enemies have a *level*.** An enemy's level sets its **HP, damage, weight, and loot tier** —
  higher level = tougher and drops rarer/longer-lasting weapons (`WEAPONS.md` §4).
- **[LOCKED] Weight matters** (from `PLAYER.md`): light/medium enemies **stagger** to a dash attack;
  **heavy** ones **floor the player** instead. Weight scales with level/type.
- **[LOCKED] Loot on death:** most enemies drop a **weapon** or a **wallet** (1¢, currency `WEAPONS.md`
  §3.9). Weapon drops are **random but constrained per stage** — the pool is limited to what that stage
  offers, so early stages hand out early weapons. **Sniper-special kills drop nothing.**
- **[LOCKED] Types are introduced progressively by stage.** Stage 1 is mostly **basic melee**; new types
  unlock as you climb (e.g. **grenade / Head-Throwers arrive ~stage 2**), so difficulty ramps by *roster*,
  not just numbers. *(Specific per-stage rosters get defined later, alongside stages.)*
- **[LOCKED] Catch-up minibosses:** if the player is **clearing too fast**, a **miniboss** is injected to
  re-apply pressure (dynamic pacing). *(Miniboss designs → `BOSSES.md`; the spawn trigger is a stage/pacing
  rule.)*
- **[PROPOSED] Diegetic weapon sources** (head-gone → sword, spine → shotgun) stay as **flavor on top of**
  the per-stage random pool — a themed corpse read, not a guaranteed type→weapon lock.
- **Routing** follows `GAMEPLAY_LOOP.md` §8: up to **8 pursuers**, hard separation (no stacked multi-hits),
  standoff rings, Z-spread. Per-type specifics below.

---

## 2. Known / locked enemies (surfaced during weapon design)

### 2.1 Grenade Enemy ("Head-Thrower") — **[LOCKED core]**
- **[LOCKED]** **Pulls off its own head and throws it at the player as a grenade.**
- **[LOCKED] Fire interaction:** if set alight (fire staff), it **starts blinking, then after ~2s BOOM** —
  a small blast that **kills the player** if caught in it (turns it into a walking bomb).
- **[ITERATE]** throw arc/telegraph, does the thrown head follow grenade §3.2 physics, cooldown, does it
  die when it throws its head or grow a new one, its HP/level range.

### 2.2 Monkey Stick Figure — **[LOCKED core]**
- **[LOCKED]** Drops the **Monkey Merc** summon, claimable only if the player holds a **dime** (`WEAPONS.md`
  §3.7).
- **[ITERATE]** does it fight (and how) before dying, rarity, whether it flees.

### 2.3 Regular Melee — **[LOCKED core]** (the stage-1 staple)
- The plain stick figure — shared base body, no special ability.
- **Tries to close on the player**, then attacks with **punch**, **jump kick**, and **slide kick** (a low,
  gap-closing approach).
- Fills role **A (basic melee)**. **[ITERATE]** which attack it picks at which range, damage, HP, approach
  speed, how aggressively it mixes the three.

---

## 3. Roster — **[PROPOSED baseline] + your named enemies**

> The **specific characters get defined later** (your call — you have your own enemy ideas). Below is a
> **role-coverage baseline** so encounter design has each function filled; we fold **your named enemies**
> onto these roles the way we did weapons.

| # | Role | Behavior sketch | Notes |
|---|---|---|---|
| A | **Basic melee** | walks in, throws punches; the bread-and-butter body (stage 1 staple) | drops early pool |
| B | **Gunner** | holds a standoff, fires straight/aimed shots | the bullet-dodging layer |
| C | **Zoner / Patterner** | stationary-ish, emits a fixed bullet pattern | thread-the-needle |
| D | **Heavy** | slow, high-HP, **floors your dash attack**; best loot | a sniper target |
| E | **Swarm** | weak, fast, many | sniper-ricochet fodder |
| F | **Head-Thrower** (§2.1) | self-decapitating grenade lobber | fire = walking bomb |
| G | **Monkey** (§2.2) | drops the merc | needs a dime |
| H | **Wallet Runner** | flees rather than fights; drops ¢ if caught | feeds the dime economy |

**[PROPOSED]** these are **roles to fill, not final characters** — each realized by one or more of your
named enemy types.

---

## 4. Rank (level) system — **[LOCKED approach], [LATER] specifics**

- **[LOCKED] Ranks are subtle.** A higher-rank enemy looks almost identical — the tell is a **small marker
  (e.g. a colored wristband)**, not a bigger body or new props. Reads as "same guy, tougher."
- Each rank up = more **HP / damage / weight** and a better loot roll within the stage's constrained pool.
- **[LATER]** how many ranks, the color code, stat curves, which ranks appear where.

---

## 5. Per-enemy asset needs → feeds `ASSET_MANIFEST.md`

For **each enemy type**: idle · walk (mirror L/R) · attack(s) · hurt/stagger · **death** (and its
**corpse/part drop** — headless body, ejectable spine, etc.) · any projectile/telegraph VFX. Plus **level
variants** (recolor + prop overlays). Special enemies add signature anims (head-throw, blink-and-explode,
wallet-drop, monkey flair).

---

## 6. Status & next step

**Locked so far:** system rules; progressive type-introduction by stage; per-stage constrained random
loot; catch-up minibosses; subtle wristband ranks; Head-Thrower and Monkey enemy cores; the role-coverage
baseline (A–H).

**Next — capture your named enemies.** You have specific enemy ideas; dump them and we'll (a) spec each one
§2-style, and (b) map it onto the role baseline so we know coverage. Specific per-stage rosters and rank
details come later, alongside the stage designs.
