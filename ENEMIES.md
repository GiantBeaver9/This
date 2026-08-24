# this.l — Enemy Roster

> **Scope:** the stick-figure enemies — identity, level system, per-type behavior, how they route/attack,
> what they drop, and per-enemy **asset needs**. Boss designs live in `BOSSES.md`; enemy *routing rules*
> extend `GAMEPLAY_LOOP.md` §8.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[ITERATE]** flesh out next · **[LATER]** parked.

---

## 1. Enemy system rules

- **[LOCKED] All enemies are stick figures** — thin, simple silhouettes that contrast the pixel-art Human.
  Variety comes from **props, size, color, and behavior**, not from redrawing whole characters.
- **[LOCKED] Enemies have a *level*.** An enemy's level sets its **HP, damage, weight, and loot tier** —
  higher level = tougher and drops rarer/longer-lasting weapons (`WEAPONS.md` §4).
- **[LOCKED] Weight matters** (from `PLAYER.md`): light/medium enemies **stagger** to a dash attack;
  **heavy** ones **floor the player** instead. Weight scales with level/type.
- **[LOCKED] Loot on death:** most enemies can drop a **weapon** (random within their tier) or a **wallet**
  (1¢, currency §`WEAPONS.md` 3.9). **Sniper-special kills drop nothing.**
- **[LOCKED] Diegetic weapon sources:** some parts map to weapons — **head-gone → sword**, **spine →
  shotgun ammo**. **[ITERATE]** whether specific enemy *types* always yield specific weapons, or drops
  stay random-by-tier with the corpse-part just being flavor.
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

---

## 3. Proposed core roster — **[PROPOSED]**, iterate one at a time

Split by combat role so encounters mix melee pressure with the bullet-hell layer. React keep/cut/tweak;
we then iterate each into a §2-style spec.

| # | Enemy | Role | Behavior sketch | Notes |
|---|---|---|---|---|
| A | **Rusher** | basic melee | walks in, throws punches; the bread-and-butter body | drops T1–T2 |
| B | **Gunner** | ranged / bullet-hell | holds a standoff, fires straight/aimed shots | the dodging layer |
| C | **Patterner** | ranged / zoner | stationary-ish, emits a fixed bullet pattern | thread-the-needle |
| D | **Bruiser** | heavy | slow, high-HP, **floors your dash attack**; best loot | a sniper target |
| E | **Swarm** | crowd | weak, fast, many | sniper-ricochet fodder |
| F | **Head-Thrower** (§2.1) | special | self-decapitating grenade lobber | fire = walking bomb |
| G | **Monkey** (§2.2) | special | drops the merc | needs a dime |
| H | **Wallet Runner** | economy | flees rather than fights; drops multiple ¢ if caught | feeds the dime economy |

**[PROPOSED]** each of A–H at multiple **levels** (recolor/prop-up for higher ranks) covers a lot of ground
without a huge unique-character count.

---

## 4. Level system — **[PROPOSED]**

- **[PROPOSED]** Levels 1–4 (or 1–5) per archetype: each step up = more HP/damage/weight and a higher loot
  tier, shown by **color and added props** (a bigger head, a helmet, an extra limb) rather than a new body.
- Ties directly to `WEAPONS.md` §4 tiers (enemy level → weapon tier it can drop).
- **[LATER]** exact stat curves per level, which levels appear in which stages.

---

## 5. Per-enemy asset needs → feeds `ASSET_MANIFEST.md`

For **each enemy type**: idle · walk (mirror L/R) · attack(s) · hurt/stagger · **death** (and its
**corpse/part drop** — headless body, ejectable spine, etc.) · any projectile/telegraph VFX. Plus **level
variants** (recolor + prop overlays). Special enemies add signature anims (head-throw, blink-and-explode,
wallet-drop, monkey flair).

---

## 6. Status & next step

**Locked so far:** system rules; Head-Thrower and Monkey enemies (cores); the proposed A–H roster and a
level system to react to.

**Next — iterate one at a time** (like weapons). Suggested order: **Rusher → Gunner → Patterner → Bruiser
→ Swarm → Wallet Runner**, then finish detailing **Head-Thrower / Monkey**. First decisions I need:
1. **Loot mapping (§1):** do specific enemy types always drop specific weapons, or random-by-tier?
2. **Level system (§4):** ~4 levels per archetype via recolor+props — good, or a different scheme?
3. **Roster (§3):** are A–H the right starting set, or add/cut?
