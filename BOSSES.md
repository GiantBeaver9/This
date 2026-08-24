# this.l — Bosses & Minibosses

> **Scope:** boss fights and the catch-up minibosses — system rules, structure, arena behavior, and per-boss
> specs (filled in as you dump ideas, like `ENEMIES.md`). Ties to `GAMEPLAY_LOOP.md` §7 (stage endpoints)
> and the enemy/weapon systems.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[ITERATE]** flesh out next · **[LATER]** parked.

---

## 1. System rules

- **[LOCKED] Bosses dodge the sniper special.** The ricochet-headshot special **cannot damage a boss** — it
  plays a **dodge animation** and misses (`PLAYER.md` §6). Your panic button is off the table; bosses are
  fought honestly.
- **[LOCKED] Catch-up minibosses.** If the player is **clearing a stage too fast**, a **miniboss** is
  injected to re-apply pressure (dynamic pacing, `ENEMIES.md` §1).
- **[LOCKED] No cheap frustration** (inherits the enemy rule): every boss attack is **telegraphed and
  fairly dodgeable**; no unreadable one-shots, no hiding.
- **[PROPOSED] Two boss classes:**
  - **Minibosses** — mid-stage, catch-up or branch-gate; tough but shorter fights.
  - **Main bosses** — **stage-end**, gate the branch/ending (`GAMEPLAY_LOOP.md` §7).
- **[PROPOSED] Phases:** main bosses shift behavior at **HP thresholds** (e.g. new attacks / faster) so the
  fight escalates.
- **[PROPOSED] Other weapons still work** — only the sniper special is negated. Looted weapons, combos, and
  the meter (for its non-sniper value?) all apply. **[ITERATE]** does the meter do anything vs. a boss.

---

## 2. Arena & the playfield — **[PROPOSED]**

- **[PROPOSED]** Bosses can be **bigger** than normal enemies and may **use more of the screen** than the
  sacred bottom-half playfield — e.g. a tall boss occupying the upper band while its **hittable/threat zones
  come down into the play band.** The player still fights in the bottom half; the boss reaches into it.
- **[PROPOSED]** Boss arenas are likely **fixed rooms** (scroll stops), not scrolling lanes.
- **[ITERATE]** per boss: exact footprint, whether the Z-band changes, hazards.

---

## 3. Boss UI — **[PROPOSED]** (cross-cuts `UI.md`)

- **[PROPOSED] Boss health bar** — a big dedicated bar (top of screen, under the HUD band), named boss,
  segmented by **phase**. This is the one time a large HP readout is warranted.
- **[PROPOSED]** phase-change flash / name card on entry.
- **[LATER]** exact style (chunky-arcade to match `UI.md`).

---

## 4. Miniboss framework — **[PROPOSED]**

- **Trigger:** injected when **pace is too fast** (`ENEMIES.md`). **[ITERATE]** the exact pace metric
  (time-to-clear? kill rate?).
- **[PROPOSED]** Minibosses are **scaled-down bosses** (one gimmick, a short phase or two) rather than just
  beefy regular enemies — a real skill check, not a stat sponge.
- **[ITERATE]** do they drop guaranteed loot; can they appear more than once a stage; are they sniper-immune
  like main bosses, or killable by it?

---

## 5. Boss roster — **your ideas** (to fill, §2-style like enemies)

> Dump boss concepts here — theme, attacks/phases, gimmick, which stage/branch, arena. I'll spec each and
> map it to minor/main + how it interacts with the weapon/enemy systems. **None defined yet.**

---

## 6. Asset needs (per boss) → feeds `ASSET_MANIFEST.md`

For **each boss**: idle/move · each attack + **telegraph** · phase-transition · **hurt** · **death** ·
**sniper-dodge** anim · any summoned adds/hazards · boss HP bar + name card. Bigger sprites than enemies.

---

## 7. Framework decisions I need (before specing bosses)
1. **Arena (§2):** can bosses break the bottom-half rule (use the upper screen) while threatening the play
   band, or must everything stay in the bottom half?
2. **Boss HP (§3):** big named boss health bar (rec), or something more diegetic?
3. **Phases (§1):** multi-phase HP-threshold bosses (rec), or single-phase fights?
4. **Miniboss nature (§4):** scaled-down bosses (rec) vs. just tougher elite enemies?
5. **Meter vs. boss (§1):** since the sniper's negated, does the special meter do anything else in a boss
   fight, or is it just dead weight there?
