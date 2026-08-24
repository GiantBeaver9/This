# this.l — Bosses & Minibosses

> **Scope:** boss fights and the catch-up minibosses — system rules, structure, arena behavior, and per-boss
> specs (filled in as you dump ideas, like `ENEMIES.md`). Ties to `GAMEPLAY_LOOP.md` §7 (stage endpoints)
> and the enemy/weapon systems.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[ITERATE]** flesh out next · **[LATER]** parked.

---

## 1. System rules

- **[LOCKED] Bosses dodge the sniper special — *until they're low.*** At normal HP the ricochet-headshot
  special **can't hit a boss** (it plays a **dodge** and misses, `PLAYER.md` §6). **But at low boss HP a
  prompt appears to use it, and the special *executes* the boss** — a cinematic finisher, the one time it
  works on a boss.
- **[LOCKED] The meter is never wasted in a boss fight.** A charge you don't spend **carries over** (usable
  on any adds, or banked for after the boss). So you can **hold it for the low-HP execution** or spend it on
  adds — your call.
- **[LOCKED] Catch-up minibosses.** If the player is **clearing a stage too fast**, a **miniboss** is
  injected to re-apply pressure (dynamic pacing, `ENEMIES.md` §1).
- **[LOCKED] No cheap frustration** (inherits the enemy rule): every boss attack is **telegraphed and
  fairly dodgeable**; no unreadable one-shots, no hiding.
- **[LOCKED] Two boss classes:**
  - **Minibosses** — mid-stage, catch-up or branch-gate; shorter fights. **Both flavors exist** — some are
    **scaled-down bosses** (a gimmick + a phase or two), others are **elite enemies** (buffed regulars).
  - **Main bosses** — **stage-end**, gate the branch/ending (`GAMEPLAY_LOOP.md` §7).
- **[LOCKED] Multi-phase.** Bosses **shift behavior at HP thresholds** — new/faster attacks as they drop — so
  the fight escalates.
- **[LOCKED] Everything except the sniper works** during the fight — looted weapons, combos, and the meter
  (per the carry-over / low-HP-execution rule above).

---

## 2. Arena & the playfield — **[PROPOSED]**

- **[LOCKED] Arena varies per boss (mix):** some bosses are **play-band brawlers** (fought in the sacred
  bottom half like enemies), others are **giant upper-screen threats** that occupy the upper band and reach
  their attacks/hittable zones **down into the play band.** Chosen per boss.
- **[PROPOSED]** Boss arenas are **fixed rooms** (scroll stops), not scrolling lanes.
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

## 7. Framework — status

**Resolved (now [LOCKED]):** arena **varies per boss** (play-band brawlers *and* giant upper-screen
threats); **multi-phase** HP-threshold bosses; minibosses are **both** scaled-down bosses **and** elite
enemies; the meter **carries over** in boss fights and becomes a **low-HP sniper execution** (with a prompt).

**Still open (small):** boss HP display style (§3 — big named bar recommended); the exact miniboss pace
trigger; whether minibosses are sniper-immune like main bosses. **Next: your boss concepts (§5).**
