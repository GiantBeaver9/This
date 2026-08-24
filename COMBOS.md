# this.l — Secret Combos

> **Scope:** the "Secret Combos" system flagged by `WEAPONS.md` §3.3 (Ball & Chain finisher) and §3.1
> (Pistol/Revolver per-direction finishers). These are **directional-input strings** that trigger special
> finishers and effects. This doc pins the **concrete input strings** (arrow-key scheme) and their effects.
> No placeholders.
>
> **Ties into LOCKED rules:** attacks are on the **Arrow keys** (8-directional, independent of WASD movement,
> `PLAYER.md` §2). `E` fires/throws (spends ammo); the **finisher is the free 4th melee hit**. The combo string
> is **punch → punch → sweep → finisher**; a **secret combo is entered on the finisher step** (hit 4) by
> completing a directional motion instead of a single tap. Damage numbers reference `TUNING.md` §6.

---

## 1. Notation & rules

- **Arrows:** `←` `→` `↑` `↓` and the four diagonals `↖ ↗ ↘ ↙`. "Forward/back" resolve to the direction the
  Human currently faces (`→` = forward when facing right).
- **A secret combo = a motion of 2–3 arrow presses completed within `0.35 s` per step** (a `0.5 s` total
  buffer). Miss the timing and it resolves as a plain directional attack.
- **Entry point:** you may start a secret motion **any time the weapon can act**. For the **Ball & Chain** and
  the **guns**, the motion's *final* press is the finisher/fire input; the lead-in presses arm the flourish.
- **Gate (guns only):** the gun's shot half of a finisher **only discharges if the target is < 20% HP**
  (LOCKED, `WEAPONS.md` §3.1). Above 20%, the same input plays as its **melee** flourish (no bullet spent).
- **Universality:** these strings are **weapon-specific**. Empty-handed and other weapons ignore them and just
  attack in the pressed direction.
- **Cost:** a secret-combo finisher spends the same durability/use as that weapon's normal `E`-fire (Ball &
  Chain: 1 of its 3 uses; guns: 1 mag round). It never costs extra.

---

## 2. Ball & Chain secret combos (`WEAPONS.md` §3.3)

The Ball & Chain launches on its chain for **80 dmg/swing** and has **3 uses**. Each secret combo shapes that
launch differently. Plays like the grenade throw (tap-`E` alters flatness), but the **arrow motion selects the
finisher shape**; the final press releases it.

| # | Input string | Name | Effect | Reach / dmg |
|---|---|---|---|---|
| 1 | `↓ ↘ →` | **Meteor Line-Drive** | flat, fast line-drive down the lane; plows every enemy along the chain's path, knocking them down | 8 wu line · **80** to each hit |
| 2 | `↓ ↑` | **Wrecking Uppercut** | ball whips straight up; anti-air + launches a grounded enemy into a juggle | 4 wu up · **80** + knock-up |
| 3 | `→ →` | **Comet Rush** | dash-cancel into a forward launch; you advance 3 wu behind the ball, closing distance while it clears the row | 6 wu fwd · **80**, pierces the row |
| 4 | `↓ ↓` | **Ground Zero** | slams the ball down at your feet for a **radial shockwave** (its own lane + both neighbor Z-rows) | r 3 wu · **80** + 0.5 s knockdown |
| 5 | `← ↓ →` | **Full Swing (360)** | a full sweeping orbit of the ball around the Human — hits everything in melee range on all sides | r 2.5 wu ring · **80** all around |

- **Default (no motion):** a plain combo finisher is a **free melee swing** (no ammo, per `WEAPONS.md` §1) —
  it does **not** spend a use. **Launching the ball** (any of the 5 finishers below) requires the secret
  **motion input**, and each spends 1 of 3 uses. *(Historical note: an earlier draft had the plain tap auto-launch
  a Meteor Line-Drive — superseded; the plain finisher is now a free melee swing.)*
- Each of the 5 above spends **1 of 3 uses**. The **20% carry-slow** applies throughout (movement only).

---

## 3. Pistol & Revolver per-direction finishers (`WEAPONS.md` §3.1)

Each **direction has its own stylish finisher**. The shot half fires **only vs a target < 20% HP** (execution);
otherwise the same input is a melee pistol-whip flourish. Pistol **pierces 3 (12/6/3)**; Revolver **30, no
pierce**. Every one ends on a **cigarette-flick** flourish (the signature `VFX.md` §4 casing/cigarette bit).

| # | Input string | Direction | Name | Effect (executes < 20% HP; else melee) |
|---|---|---|---|---|
| 1 | `→` (tap on finisher) | forward | **Quickdraw** | straight horizontal shot at the head; Pistol pierces the row (12/6/3), Revolver one clean 30; flick cigarette |
| 2 | `↓ →` | forward-low | **Fan the Hammer** *(Revolver)* / **Double-Tap** *(Pistol)* | two fast shots — Revolver empties 2 rounds for **30+30**; Pistol fires 2 piercing rounds; wider execution window |
| 3 | `↑` | up | **Skyshot** | fires upward — executes an airborne/launched or juggled enemy (pairs with Wrecking Uppercut / up-air) |
| 4 | `↓` | down | **Coup de Grâce** | point-blank execution into a **downed** enemy (post-sweep) — guaranteed headshot; the classic finisher |
| 5 | `← ↙ ↓` | back / spin | **No-Look** | Human spins and fires **behind** without turning to face — punishes an enemy flanking from the rear |

- **Headshot / zombie tax (LOCKED):** any of these that lands as a **headshot kill** has the **10% chance to
  spawn a 10 s zombie** instead (`ENEMIES.md` §2.8, `TUNING.md` §4). The **sniper special is exempt**; these
  hand-guns are not.
- **Melee fallback:** above 20% HP, `Quickdraw`/`Coup de Grâce`/etc. play as pistol-whip strikes at fist
  strength (10) — the gun still bludgeons through the combo (`WEAPONS.md` §1), no bullet spent.
- **Ammo:** an executed shot spends **1 mag round** (Pistol mag 8 / Revolver mag 6). A whiffed melee flourish
  spends none.

---

## 4. Shared secret-combo FX

Per `VFX.md` §4 (secret-combo finisher FX marked [LATER] there — pinned here as concrete cues):

| Combo family | FX cue |
|---|---|
| Ball & Chain launches | heavy launch trail + **big impact spark + screen shake (heavy preset)** + 3-frame hitstop on connect |
| Gun executions | muzzle flash + straight tracer + **headshot pop** + the **cigarette-flick** arc + ejected casing |
| Both | a brief **gold input-flash** on the HUD combo popup reading the combo's **name** (e.g. `METEOR!`, `COUP!`) to confirm the secret input registered |

---

## 5. Extensibility

The motion vocabulary is deliberately small and reusable so new weapons can adopt it cheaply:
- `↓↘→` = "line-drive / forward power" · `↓↑` = "launcher" · `→→` = "rush" · `↓↓` = "radial slam" ·
  `←↓→` = "sweep-all" · `←↙↓` = "reverse."
- Any future weapon that wants a directional finisher should map onto these six motions rather than inventing
  new ones, keeping the input language consistent across the roster.
