# this.l — Finishers & Secret Combos

> **Scope:** the **execute/finisher** input and its weapon-specific variants (the system flagged by
> `WEAPONS.md` §3.1 guns and §3.3 Ball & Chain). This doc pins the **concrete inputs** and effects. No
> placeholders.
>
> **Ties into LOCKED rules (`PLAYER.md` §3, `TUNING.md` §2.5):**
> - The combo string is **`punch → punch → sweep → finisher`**.
> - **The last two hits are a SAME-DIRECTION DOUBLE-TAP** — `→→`, `←←`, `↑↑`, or `↓↓`. The **first tap sweeps**
>   (hit 3, knocks the enemy DOWN); the **second tap of that direction is the finisher/EXECUTE** into the
>   downed body.
> - **[LOCKED] Execute lands ONLY on a SWEPT (knocked-down) enemy — never a random standing one.** If the
>   target is still standing on the second tap, that press is just a normal directional hit (no finisher, no
>   execute). You must sweep first.
> - **The direction of the double-tap picks the finisher variant** (forward/back/up/down). Weapon-specific
>   finishers below are keyed to those same four double-taps.
> - Damage numbers reference `TUNING.md` §6.

---

## 1. Notation & rules — **[LOCKED]**

- **The four execute inputs:** `→→` (forward), `←←` (back), `↑↑` (up), `↓↓` (down). "Forward/back" resolve to
  the direction the Human faces (`→→` = forward when facing right). **Each is a same-direction double-tap** —
  the first press is the **sweep**, the second is the **finisher/execute**.
- **Timing:** the two presses must land within **`0.35 s`** of each other; a **`0.15 s` input buffer** lets the
  second press register slightly before the sweep recovers (fighting-game leniency). Miss the window and the
  second press is a fresh normal attack.
- **[LOCKED] Gamepad registration:** on the **right stick** (the attack stick, `PLAYER.md` §2) a "same-direction
  double-tap" = **flick to a direction (past the `0.5` outer deadzone) → return inside the `0.25` inner deadzone
  → flick the same direction again**, both flicks within the `0.35 s` window. The inner-deadzone return is what
  separates the two taps (so holding a direction is one press, not a rapid repeat). Same rule for the
  **left-stick dash double-tap** (`PLAYER.md` §2).
- **The swept gate (LOCKED):** the finisher/execute **only resolves on a knocked-down / launched enemy**.
  **Two entry paths** (`PLAYER.md` §3, `TUNING.md` §2.6): **(a)** the **primed double-tap** sweeps a *standing*
  enemy then finishes it (the two taps ≤ **0.35 s** apart); **(b)** an enemy **already down** (e.g. from a Ball
  & Chain Ground Zero, §3, or still inside its 1.2 s knockdown) takes a **single tap** to finish — no re-sweep.
  Whiff the sweep on a standing enemy and there is no execute. Bosses are finished only where their spec allows
  (Phil only via the scripted pencil-laser in his execute window, `BOSSES.md` §5.1 — he is **never** swept;
  unsweepable H-weight enemies **are** floored by the sweep like anyone else, `TUNING.md` §2.6).
- **Plain finisher = FREE melee (no ammo):** by default the second tap is a strong melee blow into the downed
  enemy — **no ammo/durability spent** (`WEAPONS.md` §1). Weapon-specific *executes* below only trigger under
  their stated condition (e.g. a gun round only if the downed target is **< 20% HP**); otherwise the finisher
  stays the free melee blow.
- **`E`-fire is separate:** pressing **`E`** fires/throws the equipped weapon at **any** standing target, any
  HP — that is **not** an execute and does not require a sweep (`WEAPONS.md` §1/§3.1). The execute is only ever
  the second tap of the double-tap on a downed enemy.
- **Universality:** the four double-taps work with any weapon; the *variant* that plays is weapon-specific.
  Empty-handed, all four are melee finishers.

---

## 2. Pistol & Revolver executes (`WEAPONS.md` §3.1)

**Requires a swept, downed enemy (§1). The shot only fires if that downed enemy is < 20% HP** (execution);
otherwise the double-tap is a **melee pistol-whip finisher** (fist strength 10, no bullet spent). Pistol
**pierces 3 (12/6/3)**; Revolver **30, no pierce**. Each ends on the signature **cigarette-flick** (`VFX.md` §4).

| Input | Direction | Name | Effect (executes a downed < 20% HP target; else melee finisher) |
|---|---|---|---|
| `→→` | forward | **Quickdraw** | straight horizontal shot into the downed body; Pistol pierces the row (12/6/3), Revolver one clean 30; flick cigarette |
| `↓↓` | down | **Coup de Grâce** | point-blank muzzle-to-head execution into the downed enemy — the classic finisher |
| `↑↑` | up | **Skyshot** | executes a **launched/juggled** enemy (pairs with an up-air or up-strike knock-up — that airborne enemy counts as "downed" for the gate) |
| `←←` | back | **No-Look** | Human fires **behind** without turning to face — executes a downed enemy flanking from the rear |

- **Ammo:** an executed shot spends **1 mag round** (Pistol mag 8 / Revolver mag 6). A melee finisher spends none.
- **Headshot / zombie tax (LOCKED):** a gun execution that lands as a **headshot kill** has the **10% chance to
  spawn a 10 s zombie** instead (`ENEMIES.md` §2.8, `TUNING.md` §4). The **sniper special is exempt**; hand-guns
  are not.

---

## 3. Ball & Chain — directional launches (`WEAPONS.md` §3.3)

> The Ball & Chain is different: its big **launch is an `E`-fire**, not the combo finisher. You **hold a
> direction and press `E`** (tapping `E` during the wind-up flattens the arc, like the grenade, `WEAPONS.md`
> §3.3). This shapes the launch. **3 uses**, **80 dmg/swing**. When **not** launching, the ball swings as **heavy
> melee at 20/hit** through the normal P1→P2→sweep string; the **combo finisher** (a double-tap on a swept
> enemy) with the Ball & Chain equipped is a **free ground-slam at 50** (= 20 × 2.5, `TUNING.md` §6) — the
> `E`-launch costs a use, the normal string and finisher do **not**.

| `E` + direction | Name | Effect | Reach / dmg |
|---|---|---|---|
| `E` + forward | **Meteor Line-Drive** | flat, fast line-drive down the lane; plows every enemy along the chain's path, knocking them down | 8 wu line · **80** each hit |
| `E` + ↑ | **Wrecking Uppercut** | ball whips straight up; anti-air + launches a grounded enemy into a juggle | 4 wu up · **80** + knock-up |
| `E` + ↓ | **Ground Zero** | slams the ball down at your feet for a **radial shockwave** — **radius 3 wu in X**, **Z-reach ±1.5 wu** (its lane + one full row each side = 3 rows, `TUNING.md` §1); **knocks enemies down** (so it can set up a finisher) | r 3 wu (X) × ±1.5 wu (Z) · **80** + standard 1.2 s knockdown |
| `E` + back | **Full Swing (360)** | a full sweeping orbit around the Human — hits everything in melee range on all sides | r 2.5 wu ring · **80** all around |

- Each `E`-launch spends **1 of 3 uses**. The **20% carry-slow** applies throughout (movement only).
- **Ground Zero synergy:** because it leaves enemies **already downed**, you finish one with a **single tap**
  toward it (no re-sweep — the already-downed path, §1) — free melee, or a gun-execute if you swap.

---

## 4. Whip finisher — the head-rip extraction (`WEAPONS.md` §3.4)

> The Whip is **pure melee** (no `E`-fire). Its **arrow-melee directions** are up=arc / fwd=pull / down=line
> (they swing through the combo and spend the 11-hit durability like any melee hit). Its **finisher** — reached
> the normal way, a **double-tap on a swept enemy** (or single-tap on an already-downed one, §1) — is the
> signature **head-rip extraction**, a **free-melee finisher variant** (no durability spent beyond the finisher
> itself).

| Input | Effect |
|---|---|
| any finisher double-tap (`→→ ←← ↓↓`) on a swept enemy | the whip **wraps the neck, rips the head off**, the **head becomes a live grenade** (grenade fastball physics, `WEAPONS.md` §3.2/§3.5), and the Human **auto-dashes back 4 wu** to clear the blast. `↑↑` on a launched enemy rips it out of the air. |

- **Gate:** like all finishers, it needs a **swept/launched** target (`TUNING.md` §2.6). It has **no HP gate**
  (unlike the gun `<20%` execution) — the extraction works at any HP of a downed enemy.
- **The head-grenade** then explodes on contact / after 8 wu, damaging **enemies** in its blast (r 2 wu) — a
  self-made bomb with a built-in escape (the back-dash).

---

## 5. Shared execute FX

Per `VFX.md` §4 (finisher/execute FX — pinned here as concrete cues):

| Family | FX cue |
|---|---|
| Melee finisher | **finisher flash** + 3-frame hitstop + heavy hit-spark on the downed body (`VFX.md`) |
| Gun execution | muzzle flash + straight tracer + **headshot pop** + the **cigarette-flick** arc + ejected casing |
| Ball & Chain launch | heavy launch trail + big impact spark + **screen shake (heavy preset)** + 3-frame hitstop on connect |
| Whip extraction | neck-wrap + **head-rip pop** + the head arcs off as a lit grenade + the back-dash dust |
| All | a brief **gold flash** on the HUD combo popup naming the execute (e.g. `COUP!`, `METEOR!`, `RIP!`) to confirm it registered |

---

## 6. Extensibility

The input language is deliberately tiny: **four same-direction double-taps** (`→→ ←← ↑↑ ↓↓`) for
finishers/executes, plus **`E` + direction** for weapons (like the Ball & Chain) that fire a shaped
projectile. Any future weapon maps onto these — a directional **execute** variant on the four double-taps, or a
directional **`E`-fire** — so the control language stays consistent across the whole roster.
