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
- **[LOCKED] Bosses are *psychologically* hard, not long.** Every boss **except Phil** should be a **tense,
  demanding fight that stays SHORT — under ~2 minutes.** Past that it reads as annoying, not hard;
  difficulty comes from **pressure and reads, not HP bloat.** **Phil (the final boss) is the exception** —
  the endgame gauntlet, allowed to be long and brutal.
- **[LOCKED] Weapon-gated boss arenas guarantee the weapon.** When a boss requires a specific weapon (Tank →
  grenade, Colossus → whip), the arena spawns **tier-1 enemies that, on death, drop ONLY that weapon** — so
  the player can always re-arm. This resolves "where does the weapon come from" for every objective boss.

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

## 5. Boss roster

### 5.1 Phil — **FINAL BOSS** — **[LOCKED core]**
- **Look:** like a **Zombie** (`ENEMIES.md` §2.8) but with a **top hat.**
- **The Pencil:** carries a pencil and **draws new enemies into existence** to fight you — an
  artist-summoner.
- **Elusive** — hard to pin down (still fairly reachable, per the no-cheap rule).
- **Reprise summons:** can **spawn earlier bosses and minibosses mid-fight** — a greatest-hits gauntlet.
- **[LOCKED] The endgame** — the absolute final fight, **exempt from the <2-min rule**, meant to be
  **brutally hard.**
- **[ITERATE]** phases, how drawing works (telegraph / cooldown / what he can draw), how you damage an
  elusive boss, arena (play-band vs. giant), whether the low-HP sniper execution applies to the final boss.

### 5.2 Burly Macho Guy — **boss/miniboss** — **[LOCKED core]**
- **Space-denier bruiser** in the vein of the Heavy/Burly (`ENEMIES.md` §2.11), boss-scale.
- **Ground-spike punch:** punches the ground **fairly quickly** → **spikes erupt near him** that hurt the
  player (a fast close-range AoE — unlike the slow Ground Smasher). Keeps you from face-tanking him.
- **Enemy toss:** **grabs any enemy of any tier and throws it at the player for massive damage** (the
  cannibalize/grab theme at boss scale — ignores the normal tier rule).
- **Psychologically hard, short (<2 min).** **[ITERATE]** spike telegraph/range, throw telegraph, phases,
  miniboss vs. main, HP.

### 5.3 Tank — **objective boss** — **[LOCKED core]**
- **It's a literal tank.** You **fight regular enemies while dodging its machine-gun fire.**
- **[LOCKED] Win condition = grenades:** when you have a **grenade**, you **climb on top and drop it in the
  hatch. 2 grenade drops = kill.** An objective/puzzle boss, not a health-bar slugfest.
- **Relies on the Grenade** (`WEAPONS.md` §3.2) — grenades are supplied by the **weapon-gated arena rule**
  (§1: tier-1 adds drop only grenades). **[ITERATE]** how you mount it (prompt / climb), the MG fire
  pattern, does it move, what changes between the 1st and 2nd drop, the ~2-min cap.

### 5.4 The Colossus — **boss** — **[LOCKED core]** *(name TBD)*
- A **giant stick figure built out of many smaller stick figures.**
- **[LOCKED] Win condition = whip:** you **rip the smaller stick figures off it one at a time with the Whip**
  (`WEAPONS.md` §3.4 — its pull/grab), slowly dismantling the giant piece by piece.
- Weapon-gated → the **weapon-gated arena rule** (§1) supplies whips (tier-1 adds drop only whips).
- **Psychologically hard, short (<2 min).** **[ITERATE]** how many pieces to strip, do torn-off pieces
  become adds, does dismantling change its attacks/phases, how it fights back, arena (likely a giant
  upper-screen boss).

*More bosses welcome — same §5 format.*

---

## 6. Asset needs (per boss) → feeds `ASSET_MANIFEST.md`

For **each boss**: idle/move · each attack + **telegraph** · phase-transition · **hurt** · **death** ·
**sniper-dodge** anim · any summoned adds/hazards · boss HP bar + name card. Bigger sprites than enemies.

---

## 7. Framework — status

**Resolved (now [LOCKED]):** arena **varies per boss** (play-band brawlers *and* giant upper-screen
threats); **multi-phase** HP-threshold bosses; minibosses are **both** scaled-down bosses **and** elite
enemies; the meter **carries over** in boss fights and becomes a **low-HP sniper execution** (with a prompt).

Plus **[LOCKED]** the "psychologically hard, <2 min (except Phil)" rule.

**Defined bosses (4):** **Phil** (final, artist-summoner, exempt from the length cap), **Burly Macho Guy**
(ground-spikes + enemy-toss bruiser), **Tank** (objective — 2 grenade drops), **Colossus** (whip off its
stick-figure pieces). Two objective bosses use the **weapon-gated arena rule**. Open to more.

**Still open (small):** boss HP display style (§3 — big named bar recommended); exact miniboss pace trigger;
whether minibosses are sniper-immune; each boss's `[ITERATE]` details.
