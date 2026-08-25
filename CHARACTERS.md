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
  — `ENEMIES.md` §2.11, `TUNING.md` §4 — and **bosses only die to it under 10% HP**, `BOSSES.md` §1. "Everything"
  = every sweepable/targetable enemy, not the immune Heavy or a healthy boss.)*
- **Shotgunner** — **massive damage + knockback**, **wipes up to tier 3** (T3-and-below; the untiered Heavy/Monkey-Tamer and bosses survive), and **enemies
  it kills still drop loot on the ground.** Less absolute, keeps the economy.
- **Werewolf** — **5s of i-frame slash-everything**; you **still get money/drops** from the kills. Melee
  kill-all with loot, but you must physically slash in the window.

### 2.1 The Tactical *(you)* — **[LOCKED core]**
- **Faster** movement. **Less punch damage**, but **special meter fills faster** and **more weapon damage.**
  A speed/precision/weapons build.
- **Special — Sniper time-slow:** `PLAYER.md` §6 / `GAMEPLAY_LOOP.md` §4.3 — ricochet headshots **wipe
  everything**, boss dodge, low-HP boss execution. **No drops** from the special (§2.0).

### 2.2 The Shotgunner *(redheaded friend; the bulky one)* — **[LOCKED core]**
- **Bigger / bulkier.** Passives: **more punch damage** and **better shotgun damage.**
- **Special — Giant Shotgun:** whips out a **giant shotgun and blasts everything off the screen** — instead
  of an auto-kill it does **massive damage + knockback**, wiping **up to tier 3** (**T3-and-below; untiered Heavy/Tamer and bosses survive**), and
  **you get the drops on the ground.** *(Assuming this is the same person as the earlier "bulky friend" —
  flag if he's actually the 4th.)*
- **[ITERATE]** stats trade-offs, meter fill, the exact blast/knockback.

### 2.3 The Werewolf *(Gabe)* — **[LOCKED core]**
- **Special — Werewolf transformation:** **turns into a werewolf** for **~5 seconds** with **full i-frames**;
  you **slash everything** and **everything is a one-hit kill** — and you **still collect money/drops** from
  the kills. A berserk melee wipe (vs. the Tactical's ranged precision).
- **[ITERATE]** base stats, whether the meter fills the same way, duration/cooldown tuning, what the
  one-hit-kill does to bosses (immune? only in the sharpen-window?).

### 2.4 The Underdog *(the short friend — hard mode)* — **[LOCKED core]**
- The group's **butt-of-the-jokes**, **shorter** than the rest — designed as **hard mode:** **less damage**,
  **same move speed** as everyone else (no speed bump to offset the damage penalty).
- **Special — Vaporize + Empower:** **instantly vaporizes anything in a close radius** around him, then
  grants **"power attacks" for ~30 seconds** — **everything hits ~20% harder** (all attacks/weapons,
  whatever the type). A **buff/utility** special, not a screen-wipe — fitting the underdog framing.
- **[LOCKED, resolved in `TUNING.md` §3.1]** the close Vaporize **drops nothing** (sniper-style); **radius
  3.0 wu**; the **+20% buff lasts 30 s and *refreshes*, does not stack**; base stats = move ×1.00, punch ×0.80,
  weapon ×0.80 (the hard-mode penalty, `TUNING.md` §3).

---

## 3. Art & asset approach — **[LOCKED direction], [ITERATE] pipeline**

- **[LOCKED] Each character is designed separately / visually distinct** — you can **tell at a glance which
  character you're playing** (own silhouette, colors, features). This matters especially for future
  **multiplayer** (§5), where several are on screen at once.
- **[ITERATE] Animation pipeline (still open):** distinct *designs* don't force distinct *animation* —
  **recommended: a shared moveset/weapon-animation skeleton with each character's distinct skin**, and
  **unique art only for each Special.** The alternative — fully bespoke per character — is ~4× the pile.
- Plus: a **character-select screen** (UI) and each character's **Special VFX/anim** (`VFX.md`).

---

## 4. Decisions — status

**All 4 characters now defined:** Tactical (sniper wipe, no drops) · Shotgunner (giant-shotgun knockback,
≤tier-3, keeps drops) · Werewolf (5s i-frame slash-all, keeps drops) · Underdog (hard mode; close vaporize
+ 30s +20% damage buff).

**Locked:** all 4 designed as **visually distinct** characters; **single-player is the v1 target** (see §5).

**Still open:**
1. **Animation pipeline (§3):** shared-skeleton (distinct skins) — rec — or fully bespoke per character?
2. ~~Werewolf vs. bosses~~ — ✅ **Resolved:** **all** specials (Sniper, Werewolf, Shotgun, Underdog) only
   affect a boss **under 10% HP** (execution); above that the boss negates it (`BOSSES.md` §1).
3. Confirm the **Shotgunner = the earlier "bulky friend"** (§2.2), not a 5th person.

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
4. Per-character loot rules for the Underdog's vaporize and the exact stat trade-offs.
