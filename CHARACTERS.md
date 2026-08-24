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

### 2.1 The Tactical *(you)* — **[LOCKED core]**
- **Faster** movement. **Less punch damage**, but **special meter fills faster** and **more weapon damage.**
  A speed/precision/weapons build.
- **Special — Sniper time-slow:** the one specced in `PLAYER.md` §6 / `GAMEPLAY_LOOP.md` §4.3 — ricochet
  headshots, boss dodge, low-HP boss execution.

### 2.2 The Bruiser *(bulky friend)* — **[LOCKED core]**
- **Bigger / bulkier.** **More punch damage** and **better shotgun damage.** A raw-power build.
- **Special — [TBD].** Not yet defined. **[ITERATE]** his special; likely a speed/meter trade-off vs. the Tactical.

### 2.3 The Werewolf *(friend)* — **[LOCKED core]**
- **Special — Werewolf transformation:** instead of a sniper, he **turns into a werewolf** for **~5 seconds**
  with **full i-frames** and **everything is a one-hit kill** — a berserk burst (the opposite of the
  Tactical's ranged precision).
- **[ITERATE]** his base stats, whether his meter fills the same way, transformation duration/cooldown tuning,
  what a one-hit-kill does to bosses (nothing? the sharpen-window still applies?).

### 2.4 The Fourth Friend — **[LATER]**
- A fourth character; **specialty intentionally undisclosed for now.** Placeholder — slot reserved.

---

## 3. Asset implications — **[ITERATE — important, sets player art scale]**

Four characters multiply the (already bespoke) player art. **Decision needed:**
- **Shared skeleton, reskinned (recommended):** all four **reuse the same moveset/weapon animations** with a
  **different sprite skin**, and only each **Special** (sniper vs. werewolf vs. …) gets **unique art.** Keeps
  the pile sane.
- **Fully bespoke per character:** each character fully hand-animated — gorgeous, but ~4× the enormous
  bespoke player pile.

Plus: a **character-select screen** (UI) and each character's **Special VFX/anim** (`VFX.md`).

---

## 4. Decisions I need
1. **Art approach (§3):** shared-skeleton reskin (rec) or fully bespoke per character?
2. **Bruiser's Special (§2.2):** what is it?
3. **Werewolf vs. bosses (§2.3):** does one-hit-kill work on bosses, or do bosses still need the
   sharpen-window / are immune?
