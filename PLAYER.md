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

### Combo model — **[LOCKED]**
- **One hit per press** — each arrow press is a single strike in that direction (no per-direction
  auto-string). The *variety* comes from **which directions you chain**, on the ground and in the air.
- **A rolling combo counter** tracks consecutive hits **regardless of direction**, and the **finisher
  (the 3rd hit** in a chain — **[PROPOSED]** N=3) is a **stronger move**, thrown in whatever direction you
  pressed for that hit.
  - *Example:* `↓ ↓ ↓` = punch, punch, **strong back kick** (finisher on hit 3). `→ → ↓` = front punch,
    front punch, **strong back kick** — same finisher, you just faced forward for the first two. Mix
    freely; only the **position in the chain** decides strength.
- Every hit feeds the meter's rapid-hit multiplier; letting the rhythm lapse resets the counter.
- **[LOCKED]** Works empty-handed (fists) with the air reach-extender on each hit.
- **[LATER]** exact finisher index (3 vs 4), damage/knockback per hit, cancel windows.

### Dash & momentum attacks — **[LOCKED]**
Attacking during a **dash** = a **dash attack**, with **per-direction variants** (each directional attack
has a dash version that inherits the burst). Key twist:
- **[LOCKED] Dash attacks deal *no damage* — they *stagger* by enemy weight.** A dash-hit **knocks
  light/medium enemies off-balance** (opening them for a follow-up), but it's a **positioning/setup tool,
  not a damage tool.**
- **[LOCKED] Heavy targets punish it.** Dash into a **heavy enemy or a boss** and **you bounce off and
  fall over**, wasting getup frames **while *not* invincible** — a real, readable risk. Weight decides
  who staggers vs. who floors you.
- **[LOCKED] Movement-cancel tech (combo & mobility depth):**
  - **dash → jump** and **jump → dash** each cancel into the other, opening different combo/aerial routes,
  - **attack → dash** cancel (bail a swing into a dash),
  - **air-dash** exists (aerial dash/dodge with its own frames).
- **[LOCKED] Shield Rush (double-tap dodge FORWARD):** grabs an enemy ahead and **runs forward using them
  as a damage sponge**, soaking incoming fire (e.g. gatling bullets) to **close the gap** on ranged threats.
  The intended counter to suppression enemies/bosses. **[ITERATE]** which enemy it grabs (nearest ahead?),
  does it kill/consume them or just shove, how much it soaks, any tier limit, cooldown.
- **Weapon dash attacks:** melee weapons get a real dash-swing; ranged weapons dash-hit as a bludgeon.
  **[LATER]** whether a weapon dash attack is weight-stagger only, or a weapon can fire on it.

---

## 4. Movement & air states — **[LOCKED intent], frames [PROPOSED]**

- **Idle** (weapon-aware), **Walk/Run** (mirror L/R; depth reuses the cycle).
- **Dash** — grounded lunge + recovery + dust VFX.
- **Jump** — **rise → peak → fall** (3 poses min), plus **Land** recovery.
- **Hurt / hitstun** (no i-frames → you take these), **Death**.
- **[PROPOSED]** Pick-up/grab, or auto-pickup on walk-over (no anim). Leaning auto-pickup to save frames.

---

## 5. Weapons & the bespoke-animation reality — **[LOCKED pipeline] + [PROPOSED] scoping**

**[LOCKED] Pipeline = Option B (bespoke):** every weapon gets **fully custom body animation** — the full
directional + air matrix, same as fists. Prettiest and most consistent; the honest cost is that each
weapon is its own mini animated character.

**[LOCKED] Ranged weapons are bludgeons through the combo, and only *fire* on the finisher.** When you
attack with a gun (or any non-melee weapon), it's **swung as a melee weapon** through the combo — those
hits do **fist-strength** damage and reuse the fist/melee motion **with the weapon in hand**. The weapon's
**real effect fires on the combo finisher** (e.g. the shotgun discharges its shell as the strong 3rd hit;
the spine/cock economy in `WEAPONS.md` advances per finisher). Consequences for art:
- **Melee weapons (sword, etc.)** = genuinely new directional attack art — a real swing kit.
- **Ranged weapons (shotgun, etc.)** = mostly the **fist combo re-drawn holding the weapon** + a **unique
  finisher/fire** animation. Cheaper than a full bespoke melee weapon, still fully animated.
- **[LATER] reconcile in `WEAPONS.md`:** whether a ranged weapon can *also* fire outside the combo, or
  firing is strictly the finisher (the player-side default above).

Weapon-by-weapon animation lists get pinned in `WEAPONS.md` alongside each weapon's behavior.

---

## 6. The Special sequence — **[LOCKED behavior], anim [PROPOSED]**

A short cinematic beat with dedicated frames:
1. **Draw** — plant + pull the sniper rifle; **time slows** (world VFX in `VFX.md`).
2. **Aim** — brief settle; **[PROPOSED]** an aim line telegraphs the ricochet path.
3. **Fire** — one shot; ricochets headshot-to-headshot (kills, **no drops**).
4. **Recover** — lower weapon, time resumes, meter empties.

**[LOCKED]** Against a **boss the shot is normally dodged** — the boss plays a **dodge animation** and the
bullet misses; the special still ricochet-kills any normal enemies present in the arena. **Exception: at
low boss HP a prompt appears and the special *executes* the boss** (see `BOSSES.md` §1). A charge you don't
spend on a boss **carries over.** The boss dodge is a **boss asset**.

---

## 7. Player asset list (bespoke pipeline) — feeds `ASSET_MANIFEST.md`

**Priority: P0 = prototype the core loop · P1 = vertical slice · P2 = polish.**

### Base Human — fists (the full moveset)
| Asset | Frames (est) | Priority |
|---|---|---|
| Idle (empty-hand) | 2–4 loop | P0 |
| Walk/Run (mirror L/R) | 6–8 loop | P0 |
| Dash lunge + recover | 3–4 | P0 |
| **Dash attacks** — per direction (side/up/down + air) | 3–4 ×dir | P0 |
| **Air-dash** | 2–3 | P1 |
| **Shield Rush** (grab enemy + forward run) | 4–5 | P1 |
| **Fall-over + getup** (dash into a heavy target) | 4–6 | P1 |
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

### Per weapon (bespoke, full matrix per §5)
- **Melee weapon:** full directional + air attack kit (new swing art) · hold-idle · hold-walk · jump-hold.
- **Ranged weapon:** fist combo re-drawn holding the weapon (fist-strength bludgeon) · **unique
  finisher/fire** anim · hold-idle · hold-walk. (Enumerated per weapon in `WEAPONS.md`.)

### Player-owned VFX (see `VFX.md`)
Air-punch gust · dash dust · jump/landing puff · hit-impact spark · muzzle flash · spine-eject bit ·
time-slow tint · sniper tracer/ricochet line.

---

## 8. Decisions — status

**Resolved (now [LOCKED] above):** pixel-art human protagonist; **short-gust** air reach-extender; WASD
move + arrow-key **directional** attacks; **single hit per press** with a rolling **mixed-direction combo**
and a strong **finisher**; **dash + jump** with a full air moveset; **per-direction dash attacks that deal
no damage but weight-stagger** (heavy targets floor you, non-invincible getup); **jump↔dash cancels,
attack→dash cancel, and air-dash** all in; **bespoke animation for every weapon**; **ranged weapons
bludgeon through the combo and fire on the finisher**; **boss dodges** the sniper special.

**[LATER]:** finisher index (3rd vs 4th hit), exact keybinds, damage/knockback numbers, whether a weapon
dash attack can fire, palette, pick-up handling. Pinned in the relevant system docs.
