# this.l — Playable Characters

> **Scope:** the **4 selectable characters** (the four friends). They **share the moveset** in `PLAYER.md`
> and differ in **stats, Special, and appearance.** You **pick one each run** (replayability).
>
> **Legend:** **[LOCKED]** decided · **[ITERATE]** flesh out next · **[LATER]** parked.

---

## 1. The roster — **[LOCKED core]**

- **[LOCKED] Four characters; pick a different one each run.**
- **[LOCKED] Shared kit** (all four): the full `PLAYER.md` moveset — WASD move, arrow directional attacks +
  mixed-direction combo, dash, jump, air-dash, dash attacks, **Shield Rush**, weapon pickup/use, and the
  **air-punch reach-extender.**
- **[LOCKED] They differ in:** base **stats** (speed, punch damage, meter fill, weapon damage), their
  **unique Special**, and **appearance** (each a different friend).

---

## 2. Characters

### 2.0 The specials & the drops trade-off — **[LOCKED core]**
Each Special clears a crowd differently, balanced by **what loot survives** — the core axis is
**completeness vs. loot retention:**
- **Tactical / Sniper** — **one-shots every targetable enemy** it can lock (up to the tier cap 15/30/45), but
  **drops NOTHING.** Cleanest wipe, zero loot. *(Exceptions: the **Heavy is immune to the ricochet/headshot-pick**
  — `ENEMIES.md` §2.11, `TUNING.md` §4 — and **bosses only die to it at ≤10% HP**, `BOSSES.md` §1. "Everything"
  = every sweepable/targetable enemy, not the immune Heavy or a healthy boss.)*
- **Shotgunner** — **massive damage + knockback**, **wipes up to tier 3** (T3-and-below; the untiered Heavy/Monkey-Tamer and bosses survive), and **enemies
  it kills still drop loot on the ground.** Less absolute, keeps the economy.
- **Werewolf** — **5s of i-frame slash-everything**; you **still get money/drops** from the kills. Melee
  kill-all with loot, but you must physically slash in the window.

### 2.1 The Tactical *(you)* — **[LOCKED core]**
- **Faster** movement. **Less punch damage**, but **special meter fills faster** and **more weapon damage.**
  A speed/precision/weapons build.
- **Special — Sniper time-slow:** `PLAYER.md` §6 / `GAMEPLAY_LOOP.md` §4.3 — ricochet headshots **wipe
  everything**, boss dodge, low-HP execution (HP-depletion bosses only, `BOSSES.md` §1). **No drops** from the special (§2.0).

### 2.2 The Shotgunner *(redheaded friend; the bulky one)* — **[LOCKED core]**
- **Bigger / bulkier.** Passives: **more punch damage** and **better shotgun damage.**
- **Special — Giant Shotgun:** whips out a **giant shotgun and blasts everything off the screen** — instead
  of an auto-kill it does **massive damage + knockback**, wiping **up to tier 3** (**T3-and-below; untiered Heavy/Tamer and bosses survive**), and
  **you get the drops on the ground.** *(The Shotgunner **is** the "bulky friend" — one of the 4, not a 5th.)*
- **[LOCKED] Meter-tier scaling (`TUNING.md` §3.1):** blast arc **6→8→10 wu** and knockback **8→11→14 wu** at
  yellow/blue/green fills; ≤T3 instakill holds at every tier.

### 2.3 The Werewolf *(Gabe)* — **[LOCKED core]**
- **Special — Werewolf transformation:** **turns into a werewolf** for **~5 seconds** with **full i-frames**;
  you **slash everything** and **everything is a one-hit kill** — and you **still collect money/drops** from
  the kills. A berserk melee wipe (vs. the Tactical's ranged precision).
- **[LOCKED] Meter-tier scaling (`TUNING.md` §3.1):** transform lasts **5→7→9 s** at yellow/blue/green fills
  (i-frames + 1HKO throughout). **Vs. bosses:** slash dmg = 0 above 10% HP (executes only ≤10%, like all
  specials, `BOSSES.md` §1); **vs. Heavy/untiered it still 1HKOs** (`TUNING.md` §3.1). Meter fills the standard
  way (`TUNING.md` §2.4).
- **[LOCKED] Werewolf-form kit (during the transform):** **WASD moves** at **×1.2 speed**; the werewolf
  **auto-slashes in the facing direction** (no combo string, no aiming) at **~4 slashes/s, reach 2.0 wu**,
  every slash a **1HKO**. **No weapon use, no `E`/`Q`.** **Jump and dash ARE available** (for repositioning),
  but there are **no dedicated jump-attacks** — the auto-slash simply continues in the air. Full i-frames
  throughout. Reverts to the current character at the end of the timer. **A held weapon is suppressed during
  the transform** (claws, not the weapon) and **returns to hand on revert** (if it hasn't expired/broken).

### 2.4 The Underdog *(the short friend — hard mode)* — **[LOCKED core]**
- The group's **butt-of-the-jokes**, **shorter** than the rest — designed as **hard mode:** **less damage**,
  a **base move speed of ×1.00** and **no compensating speed bump** to offset the damage penalty (unlike the
  Tactical's ×1.12) — that's the hard-mode point: he's plain-statted with weaker hits (`TUNING.md` §3).
- **Special — Vaporize + Empower:** **instantly vaporizes T3-and-below in a close radius** around him (untiered
  Heavy/Tamer & bosses survive — only the Werewolf **special** kills Heavies among the specials, `TUNING.md` §3.1), then
  grants **"power attacks" for ~30 seconds** — **everything hits ~20% harder** (all attacks/weapons,
  whatever the type). A **buff/utility** special, not a screen-wipe — fitting the underdog framing.
- **[LOCKED, resolved in `TUNING.md` §3.1]** the close Vaporize **drops nothing** (sniper-style); **radius
  3.0 wu** and buff **+20%/30 s** at 1 fill, **scaling to 4.0 wu / +25%** (2 fills) and **5.0 wu / +30%**
  (3 fills); the buff **refreshes, does not stack**; base stats = move ×1.00, punch ×0.80, weapon ×0.80 (the
  hard-mode penalty, `TUNING.md` §3).

### 2.5 Visual specs & palettes — **[LOCKED]** (concrete looks for the 4 bespoke sets, `ASSET_MANIFEST.md` §1)

> Each is a **48 px pixel-art person** (`ASSET_MANIFEST.md` §0), instantly distinct by silhouette + a **3-color
> character accent** drawn from the shared 32-color palette. This is the art brief for the ×4 bespoke pipeline.

| Character | Silhouette / look | Accent colors |
|---|---|---|
| **Tactical (you)** | lean; **cargo pants + tactical vest + backwards cap**; the sniper case on the back | **olive-green / black / orange** |
| **Shotgunner** | **bulky, redheaded, bearded**; flannel + jeans; broad shoulders (widest silhouette) | **rust-red / denim-blue / cream** |
| **Werewolf (Gabe)** | scruffy, medium build, band tee + jacket; **transforms into a hunched brown wolf** (bespoke transform + wolf set) | **brown / grey / yellow-eyes** |
| **Underdog** | **short, slight**; oversized hoodie (comically outsized); the shortest silhouette | **purple / white / lime** |

- **Readability:** the four are separable by **silhouette alone** (lean / broad / medium / short) so co-op
  (future) and character-select read instantly. **Palette:** each uses the shared base 32-color palette + its
  3-color accent (`ASSET_MANIFEST.md` §0) — this **supersedes `PLAYER.md` §8's palette [LATER]**.

---

## 3. Art & asset approach — **[LOCKED]**

- **[LOCKED] Each character is designed separately / visually distinct** — you can **tell at a glance which
  character you're playing** (own silhouette, colors, features). This matters especially for future
  **multiplayer** (§5), where several are on screen at once.
- **[LOCKED] Animation pipeline = FULLY BESPOKE per character.** Each of the 4 characters gets its **own
  hand-made animation set** (moveset, weapon holds, hurt/death, Special) — **not** a shared-skeleton reskin.
  Maximum individuality; the cost is ~4× the animation pile, which is accepted. Plan asset generation for
  **4 complete character animation sets** (`ASSET_MANIFEST.md` §1).
- Plus: a **character-select screen** (UI) and each character's **Special VFX/anim** (`VFX.md`).

---

## 4. Decisions — status

**All 4 characters now defined:** Tactical (sniper wipe, no drops) · Shotgunner (giant-shotgun knockback,
≤tier-3, keeps drops) · Werewolf (5s i-frame slash-all, keeps drops) · Underdog (hard mode; close vaporize
+ 30s +20% damage buff).

**Locked:** all 4 designed as **visually distinct** characters; **single-player is the v1 target** (see §5).

**Still open:**
1. ~~Animation pipeline~~ — ✅ **Resolved (§3): FULLY BESPOKE per character** (4 complete animation sets, no
   shared skeleton).
2. ~~Werewolf vs. bosses~~ — ✅ **Resolved:** **all** specials (Sniper, Werewolf, Shotgun, Underdog) only
   affect a boss **at ≤10% HP** (execution); above that the boss negates it (`BOSSES.md` §1).
3. ~~Shotgunner = bulky friend?~~ — ✅ **Confirmed:** the Shotgunner **is** the bulky friend (§2.2), one of the 4.

---

## 5. Multiplayer — **[LATER / planned — NOT in the v1 / overnight build]**

> **[LOCKED scope] The overnight build is SINGLE-PLAYER ONLY.** Co-op is a *future* addition; **do not
> implement it in v1.** All encounter counts (`ENCOUNTERS.md`) and tuning (`TUNING.md`) are authored for
> **1 player**. The figures below are a **forward-looking note**, not a v1 requirement — a build agent should
> **ignore them** and any stray "2P" mention elsewhere (e.g. `AREAS.md` §1.6's "2-player: two + a miniboss"
> is the same future note, not a v1 spec).

- **[LOCKED intent]** Co-op is a **planned future** addition: friends **each pick a different character**.
- **[FUTURE, not v1] Difficulty scaling:** with multiplayer, spawn **~2.5× the enemies**.
- **[ITERATE / LATER]** player count; shared vs. per-player economy/lives/checkpoints; local vs. online; how
  bosses scale; camera for multiple players. **Parked until single-player ships.**

*(Per-character loot rules and stat trade-offs are LOCKED: Underdog Vaporize drops nothing (§2.4); all stat
multipliers in `TUNING.md` §3.)*
