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
- **[LOCKED] Ability tier rule:** an enemy that uses an ability **on another enemy** can only target a unit
  **at least one tier below it** (a tier-2 can weaponize a tier-1, never another tier-2). Enemies **loot
  each other** the same way the player loots corpses.
- **[DATA] Attack windups (telegraph), collected as we spec:** regular melee **~100ms**, sword
  **~150–200ms** (slight variance). Convention: **more reach/damage → longer, more readable windup.**
- **[LOCKED] Enemy ranged is short-range (no sniping).** Enemy guns/projectiles only connect from **close
  range**, so shooters must **close in** — which keeps every threat **dodgeable** by move/dash/jump. The
  bullet-hell pressure comes from *many close shooters*, never from off-screen snipers.

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
- Fills role **A (basic melee)**. Windup **~100ms**. **[ITERATE]** which attack it picks at which range,
  damage, HP, approach speed, how aggressively it mixes the three.

### 2.4 Snapper (Sword-Maker) — **[LOCKED core]** *(name TBD)* — **Tier 2**
- **Ability:** grabs a **tier-1** enemy and **"snaps" them like a whip, turning them into a sword**, then
  **swordfights the player** with **much longer reach** than fists.
- **Tier rule (§1):** can only snap a **tier-1** — never another tier-2.
- **Telegraph:** sword swings **wind up ~150–200ms** (vs. ~100ms fists) — long reach, but **readable and
  punishable.**
- Fills a **melee-zoner** niche (reach + threat that forces you to respect spacing).
- **[LOCKED] No tier-1 available:** he **stays away from the player and calls in tier-1 enemies** to snap —
  a keep-away support that manufactures its own weapon supply.
- **[ITERATE]** does his sword decay; does killing him **drop the sword** for the player; call-in cooldown /
  how many he can summon.

### 2.5 Arm-Ripper (Dual Pistols) — **[LOCKED core]** *(name TBD)* — **Tier 2a**
- **Ability:** rips the **arms off a tier-1 Regular Melee** and **dual-wields them as pistols.**
- **The disarmed tier-1** is left as a **headbutt-only enemy** (no arms → just lunges to headbutt you) — an
  emergent enemy state, not a separate character.
- **Short-range guns (§1):** his pistols only hit from **close range**, so he must **close in** — his
  approach is your window to **dodge / dash / jump** out of the line.
- **Tier rule (§1):** only disarms a **tier-1.**
- Fills the **gunner** role (a close-range, approachable one). **[ITERATE]** fire cadence, does he run dry /
  reload, what he does with no tier-1 to disarm (call one in like the Snapper?), does killing him drop
  pistols for the player.

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

**Locked so far:** system rules (incl. shared-body-plus-ability, ability tier rule, telegraph timing);
progressive type-introduction by stage; per-stage constrained random loot; catch-up minibosses; subtle
wristband ranks; **enemy guns are short-range/dodgeable**. **Defined enemies:** Head-Thrower, Monkey,
Regular Melee, Snapper (Sword-Maker, T2), Arm-Ripper (Dual Pistols, T2a) + its disarmed Headbutt state.

**Next — capture your named enemies.** You have specific enemy ideas; dump them and we'll (a) spec each one
§2-style, and (b) map it onto the role baseline so we know coverage. Specific per-stage rosters and rank
details come later, alongside the stage designs.
