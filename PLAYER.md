# this.l — Player Character ("the Human")

> **Scope:** the player avatar — visual identity, the directional attack system, all movement/air states,
> and the **asset list** that falls out. Weapon *behavior* lives in `WEAPONS.md`; here we only cover how
> the Human *holds and swings* each weapon (the animation side).
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[LATER]** parked.

---

## 1. Visual identity — **[LOCKED]**

- The Human is a **pixel-art representation of a person** — clearly the protagonist, visually distinct
  from the thin **stick-figure** enemies. Semi-small for readability.
- **[LOCKED] "Air" punch reach-extender:** when the Human attacks, the punch **emits an "air" effect**
  (a burst/gust animation off the fist). This **extends the player's hitbox slightly past the enemies'
  reach** — a built-in spacing advantage: you can hit stick figures from just outside their range.
  - This is both a **mechanic** (reach) and an **asset** (the air/gust VFX, per attack direction).
  - **[PROPOSED]** the air burst is purely a melee reach-extender (very short), *not* a travelling
    projectile — so it doesn't turn fists into a gun. (Flag if you want it to travel a little.)
- **[LATER]** palette, damage/blood state on the sprite, any customization.

---

## 2. Inputs — **[LOCKED] split-stick scheme**

- **Move = WASD.** `A/D` = left/right, `W/S` = depth (up/down the Z-band). Continuous.
- **Attack = Arrow keys, directional.** Attack direction is **independent of movement/facing** — you can
  **attack left while running right.** This is the core expressiveness: many attacks, not a fixed combo.
- **Jump** and **Dash** and **Special** = dedicated buttons. **[PROPOSED]** Space = Jump, a modifier
  (e.g. Left-Shift) = Dash, and one key (e.g. Enter/`.`) = Special. *(Confirm the exact keys later; the
  mapping doesn't affect assets.)*

**[LOCKED] Dash + Jump both exist.** Dash = grounded evasive burst (no i-frames). Jump adds a full
**air layer** — you can attack in the air, so the moveset roughly **doubles** (ground set + air set).

---

## 3. The attack system — **[LOCKED] directional, [PROPOSED] details**

Attacks are **directional** (arrow keys), on the **ground** and in the **air**. Think Smash-style
directional normals rather than a single canned combo string.

### Ground attacks
| Input | Attack | Role |
|---|---|---|
| ← / → | **Side strike** | bread-and-butter horizontal hit; carries the air reach-extender |
| ↑ | **Up strike** | anti-air / launcher — hits enemies above or knocks one up |
| ↓ | **Down strike** | low sweep / ground-pound — hits low, good vs. crouched/small enemies |

### Air attacks (while jumping)
| Input | Attack | Role |
|---|---|---|
| ← / → | **Air side** ("neutral air") | horizontal aerial poke |
| ↑ | **Up air** | juggle / hit above |
| ↓ | **Down air / spike** | slam downward; can spike/bounce, combo starter on landing |

- **[PROPOSED] Chaining:** tapping a direction repeatedly does a **short 2–3 hit micro-combo** in that
  direction (e.g. side-side-side = jab/jab/cross), and you can cancel between directions for flow. Every
  hit feeds the meter's rapid-hit multiplier. Keeps it a *rhythm* without a rigid single string.
- **[LOCKED]** All of this works empty-handed (fists) with the air reach-extender.
- **[LATER]** exact hit counts, damage, knockback per direction; any special-cancel rules.

---

## 4. Movement & air states — **[LOCKED intent], frames [PROPOSED]**

- **Idle** (weapon-aware), **Walk/Run** (mirror L/R; depth reuses the cycle).
- **Dash** — grounded lunge + recovery + dust VFX.
- **Jump** — **rise → peak → fall** (3 poses min), plus **Land** recovery.
- **Hurt / hitstun** (no i-frames → you take these), **Death**.
- **[PROPOSED]** Pick-up/grab, or auto-pickup on walk-over (no anim). Leaning auto-pickup to save frames.

---

## 5. Weapons & the bespoke-animation reality — **[LOCKED pipeline] + [PROPOSED] scoping**

**[LOCKED] Pipeline = Option B (bespoke):** every weapon gets **fully custom body animation**, not a
prop pinned to a shared body. Prettiest result — and the honest cost is that **each weapon is its own
mini animated character.**

**The asset math (why we must scope this):** with bespoke + directional + air, a *fully* animated weapon
wants roughly:
`idle + walk + jump(3) + land + 3 ground attacks + 3 air attacks (+dash-hold) ≈ 12–16 animations each.`
Across a large weapon roster that's **hundreds** of hand-drawn animations.

**[PROPOSED] Scoping rule to keep it human-doable — "fists are the virtuoso, weapons are focused":**
- **Fists (base Human)** get the **full** directional + air moveset — this is the star and you'll use it
  most.
- **Each weapon** gets a **tight signature set**, not the whole matrix. Suggested minimum per weapon:
  **hold-idle, hold-walk, one primary attack, one air attack**, and only the *extra* directional attacks
  that define that weapon (e.g. sword gets an up-launcher; shotgun gets a down-blast). Weapons inherit the
  Human's jump/dash/hurt/death poses (drawn once, weapon prop composited or redrawn only if it reads wrong).
- This turns each new weapon into ~**4–6 animations** instead of ~15. *Flag if you'd rather every weapon
  be fully animated — totally valid, just a much bigger pile.*

Weapon-by-weapon animation specifics get pinned in `WEAPONS.md` alongside each weapon's behavior.

---

## 6. The Special sequence — **[LOCKED behavior], anim [PROPOSED]**

A short cinematic beat with dedicated frames:
1. **Draw** — plant + pull the sniper rifle; **time slows** (world VFX in `VFX.md`).
2. **Aim** — brief settle; **[PROPOSED]** an aim line telegraphs the ricochet path.
3. **Fire** — one shot; ricochets headshot-to-headshot (kills, **no drops**).
4. **Recover** — lower weapon, time resumes, meter empties.

**[LOCKED]** No effect on bosses — the anim still plays, the shot just doesn't kill them. **[PROPOSED]**
it staggers/pings a boss rather than whiffing entirely (your call, affects boss feel).

---

## 7. Player asset list (bespoke pipeline) — feeds `ASSET_MANIFEST.md`

**Priority: P0 = prototype the core loop · P1 = vertical slice · P2 = polish.**

### Base Human — fists (the full moveset)
| Asset | Frames (est) | Priority |
|---|---|---|
| Idle (empty-hand) | 2–4 loop | P0 |
| Walk/Run (mirror L/R) | 6–8 loop | P0 |
| Dash lunge + recover | 3–4 | P0 |
| Jump: rise / peak / fall | 3 | P0 |
| Land recovery | 2 | P0 |
| Ground attack: **side** (2–3 hit micro-combo) | 6–9 | P0 |
| Ground attack: **up** (launcher) | 3–4 | P0 |
| Ground attack: **down** (sweep/pound) | 3–4 | P0 |
| Air attack: **side** | 3 | P0 |
| Air attack: **up** | 3 | P1 |
| Air attack: **down / spike** | 3–4 | P1 |
| **Air reach-extender VFX** (gust off fist, per direction) | 2–3 ×dir | P0 |
| Hurt / hitstun | 2 | P0 |
| Death | 4–6 | P1 |
| Special: draw → aim → fire → recover | 3+2+2+2 | P1 |
| Pick-up (or none if auto-pickup) | 0–2 | P2 |

### Per weapon (bespoke, **scoped set** per §5)
For **each** weapon: hold-idle · hold-walk · primary attack · one air attack · its 1–2 signature
directional attacks · weapon-specific VFX. (Enumerated per weapon in `WEAPONS.md`.)

### Player-owned VFX (see `VFX.md`)
Air-punch gust · dash dust · jump/landing puff · hit-impact spark · muzzle flash · spine-eject bit ·
time-slow tint · sniper tracer/ricochet line.

---

## 8. Decisions — status

**Resolved (now [LOCKED] above):** pixel-art human protagonist; air-punch reach-extender; WASD move +
arrow-key directional attacks; dash **and** jump with a full air moveset; bespoke weapon animation.

**Open / want your call:**
1. **Weapon scoping (§5):** OK to give weapons a *tight signature set* while fists get the full matrix,
   to keep the art pile sane? Or fully animate every weapon?
2. **Air reach-extender (§1):** short melee-only gust (recommended), or does it travel a little?
3. **Micro-combo (§3):** 2–3 hit taps per direction good, or single hit per press?
4. **Boss vs. special (§6):** sniper *staggers* a boss, or fully whiffs?

**[LATER]:** exact keybinds, damage/knockback numbers, palette, pick-up handling.
