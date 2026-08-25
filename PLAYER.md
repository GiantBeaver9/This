# this.l — Player Character ("the Human")

> **Scope:** the player avatar — visual identity, the directional attack system, all movement/air states,
> and the **asset list** that falls out. Weapon *behavior* lives in `WEAPONS.md`; here we only cover how
> the Human *holds and swings* each weapon (the animation side).
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[LATER]** parked.

---

## 1. Visual identity — **[LOCKED]**

- **[LOCKED] Four playable characters.** The player picks one of **four friends** each run; they **share the
  moveset in this doc** but differ in stats and **Special** — see `CHARACTERS.md`. "The Human" below is the
  shared template; the **Sniper special (§6) is specifically the Tactical character's** (others differ,
  e.g. a werewolf transformation).
- Each is a **pixel-art representation of a person** — clearly the protagonist, visually distinct from the
  thin **stick-figure** enemies. Semi-small for readability.
- **[LOCKED] "Air" punch reach-extender:** when the Human attacks, the punch **emits an "air" effect**
  (a burst/gust animation off the fist). This **extends the player's hitbox slightly past the enemies'
  reach** — a built-in spacing advantage: you can hit stick figures from just outside their range.
  - This is both a **mechanic** (reach) and an **asset** (the air/gust VFX, per attack direction).
  - **[PROPOSED]** the air burst is purely a melee reach-extender (very short), *not* a travelling
    projectile — so it doesn't turn fists into a gun. (Flag if you want it to travel a little.)
- **[LATER]** palette, damage/blood state on the sprite, any customization.

---

## 2. Inputs — **[LOCKED] split-stick scheme**

- **[LOCKED] Move = WASD** — **8-directional** on the ground plane (`A/D` = left/right, `W/S` = depth, plus
  diagonals). Continuous.
- **[LOCKED] Attack = Arrow keys** — **8-directional**, **independent of movement/facing** (attack left while
  running right). The core expressiveness: many attacks, not a fixed combo.
- **[LOCKED] Keybinds:** **`Q` = use Special** · **`E` = use equipped weapon** (its primary action —
  fire/throw, distinct from the arrow melee) · **`F` = pick up** a weapon (picking one up while armed
  **destroys** the current, `WEAPONS.md` §1).
- **[LOCKED] Jump = `Space`.** **[LOCKED] Dash = double-tap a WASD direction.** Air-dash = double-tap in the
  air. **Shield Rush is the same forward double-tap *when an enemy is directly ahead*** (you grab them as a
  shield, §3); **no enemy ahead → it's a plain forward dash.** Same input, context decides.
- **[LOCKED] Gamepad = twin-stick:** **left stick = 8-dir move · right stick = 8-dir attack** (this frees the
  face buttons for actions). Mapping:
  - **Jump = A / ✕** (bottom face button) · **Special = Y / △** · **Dash = B / ○** *(or double-tap left stick)*
  - **Use weapon (`E`) = right trigger** · **Pick-up (`F`) = right bumper** (paired on the same side)
- **[LOCKED] `E` vs. the finisher (reconciled):** `E` **fires/throws** the equipped weapon at any time (spends
  ammo/use); the **combo finisher (4th hit) is a *free melee* swing** — it never auto-fires the gun and costs no
  ammo (`WEAPONS.md` §1, `COMBOS.md` §1). A ranged weapon may therefore fire **outside** the combo (`E`) *and*
  bludgeon **through** it (arrow melee) — these do not conflict. **[ITERATE]** a **rebinding** UI (cosmetic, post-slice).

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
- **One hit per press** — each arrow press is a single strike in that direction. Two systems coexist:
  - **Standalone directional normals** (up-strike, down-strike, air attacks) — free-form pokes/juggle tools you
    can throw in any order; they **do not** advance the auto-string (`TUNING.md` §2.5).
  - **The auto-string** `punch → punch → sweep → finisher`, which advances on **side/forward** presses (the
    bread-and-butter combo). *Variety* comes from mixing the standalone normals **around** the string, not from
    the string itself accepting any direction.
- **[LOCKED] The string is `punch → punch → sweep → finisher`** (4 hits), attacking in the direction pressed.
  - **[LOCKED] The last two hits (sweep + finisher) are a SAME-DIRECTION DOUBLE-TAP** — `→→`, `←←`, `↑↑`, or
    `↓↓` (any of the four directions). **The first tap is the SWEEP** (hit 3, **knocks the enemy DOWN**); **the
    second tap of the same direction is the FINISHER/EXECUTE**, which lands into the now-downed body.
  - **[LOCKED] Execute only lands on a SWEPT (knocked-down) enemy — never a random standing one.** If the
    target is still standing when you press the second tap (you never swept it, or it's an unsweepable target),
    that press is **just a normal directional hit** — no finisher, no execute. You **must sweep first.** (The
    sweep→finisher double-tap is the *only* route to the finisher; there is no auto 4th hit.)
  - The **direction of the double-tap picks the finisher's facing** (forward `→→`, back `←←`, up `↑↑` for a
    launched/juggled target, down `↓↓` for the classic grounded execute). Weapon-specific finisher *variants*
    are keyed to these same four double-taps (`COMBOS.md`).
  - **Finisher = strong, FREE melee (no ammo);** weapon-fire is separate (`E`, §2 / `WEAPONS.md` §1). With a
    **gun equipped**, the finisher into a **downed enemy under 20% HP** becomes a **gun execution** (fires a
    round, cinematic) — otherwise it stays a free melee blow (`COMBOS.md` §2). Either way it **requires the
    sweep-down first.**
    *(Phil's kill is a finisher on a swept-down Phil — the pencil-laser, `BOSSES.md` §5.1.)*
- Every hit feeds the meter's rapid-hit multiplier; letting the rhythm lapse resets the counter.
- **[LOCKED]** Works empty-handed (fists) with the air reach-extender on each hit.
- **[LATER]** damage/knockback per hit, cancel windows. *(Finisher = the 4th hit, LOCKED above.)*

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
  **[LOCKED] Weight-stagger only, no fire.** A weapon dash attack follows the universal dash-attack rule
  (`TUNING.md` §2.5: **0 dmg, weight-stagger only**) — it **never fires the weapon** (firing is always the
  separate `E` action). So dashing with a gun still just staggers; you press `E` to shoot. Consistent across
  every weapon.

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

**[LOCKED] Ranged weapons bludgeon through the combo; you FIRE them with `E`.** When you attack with a gun
(or any non-melee weapon) via the arrow-key combo, it's **swung as a melee weapon** — those hits do
**fist-strength** damage and reuse the fist/melee motion **with the weapon in hand**, and the **4th-hit
finisher is a free melee blow (no ammo)**. To actually **fire/throw** the weapon (the shell, shot, grenade,
cast — spending ammo) you press **`E`** (§2, `WEAPONS.md` §1). Consequences for art:
- **Melee weapons (sword, etc.)** = genuinely new directional attack art — a real swing kit.
- **Ranged weapons (shotgun, etc.)** = mostly the **fist combo re-drawn holding the weapon** + a **unique
  finisher/fire** animation. Cheaper than a full bespoke melee weapon, still fully animated.
- **[LOCKED, reconciled]** a ranged weapon **does** fire outside the combo — `E` fires/throws at any time
  (spends ammo), and the finisher stays a **free melee** swing (no ammo). Firing is **not** locked to the
  finisher. (`WEAPONS.md` §1, `COMBOS.md` §1.)

Weapon-by-weapon animation lists get pinned in `WEAPONS.md` alongside each weapon's behavior.

---

## 6. The Special sequence — **[LOCKED behavior], anim [PROPOSED]**

> **This sniper special belongs to the Tactical character** (`CHARACTERS.md` §2.1). Other characters have
> different specials (e.g. the Werewolf's 5s one-hit-kill transformation) — same meter, different payoff.

A short cinematic beat with dedicated frames:
1. **Draw** — plant + pull the sniper rifle; **time slows** (world VFX in `VFX.md`).
2. **Aim** — brief settle; **[PROPOSED]** an aim line telegraphs the ricochet path.
3. **Fire** — one shot; ricochets headshot-to-headshot (kills, **no drops**).
4. **Recover** — lower weapon, time resumes, meter empties.

**[LOCKED]** Against a **boss the shot is normally dodged** — the boss plays a **dodge animation** and the
bullet misses; the special still ricochet-kills any normal enemies present in the arena. **Exception: at
**under 10% boss HP** a prompt appears and the special *executes* the boss** (same rule for all characters,
`BOSSES.md` §1). A charge you don't
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
bludgeon through the combo, fired with `E`** (finisher = free 4th-hit melee); **boss dodges** the sniper special.

**[RESOLVED]:** damage/knockback numbers (`TUNING.md` §2.1/§2.5), weapon dash attack = stagger-only-no-fire
(§3), `E`-use action (§2, `WEAPONS.md` §1). **[LATER]:** palette only. *(Keybinds/gamepad LOCKED in §2.)*
