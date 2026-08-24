# this.l — Bosses & Minibosses

> **Scope:** boss fights and the catch-up minibosses — system rules, structure, arena behavior, and per-boss
> specs (filled in as you dump ideas, like `ENEMIES.md`). Ties to `GAMEPLAY_LOOP.md` §7 (stage endpoints)
> and the enemy/weapon systems.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[ITERATE]** flesh out next · **[LATER]** parked.

---

## 1. System rules

- **[LOCKED] Specials only work on a boss under 10% HP — for ALL characters.** Above 10%, a boss **negates
  the special** (the Tactical's sniper visibly **dodges**; Werewolf / Shotgun / Underdog specials simply do
  nothing to the boss). **At/under 10% a prompt appears and the special *executes* the boss** — the one time
  any special ends a boss. One rule across all four characters.
- **[LOCKED] The meter is never wasted in a boss fight.** A charge you don't spend **carries over** (usable
  on any adds, or banked for after the boss). So you can **hold it for the low-HP execution** or spend it on
  adds — your call.
- **[LOCKED] Catch-up minibosses.** If the player is **clearing a stage too fast**, a **miniboss** is
  injected to re-apply pressure (dynamic pacing, `ENEMIES.md` §1).
- **[LOCKED] No cheap frustration** (inherits the enemy rule): every boss attack is **telegraphed and
  fairly dodgeable**; no unreadable one-shots, no hiding.
- **[LOCKED] Two boss classes:**
  - **Minibosses** — mid-stage, catch-up; shorter fights. **Both flavors exist** — some are **scaled-down
    bosses** (a gimmick + a phase or two), others are **elite enemies** (buffed regulars).
  - **Main bosses** — **area-end** (the campaign is linear; no branch gate).
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
  the player can always re-arm. Some arenas offer **two options** (Helicopter → **Bat OR Grenade**). This
  resolves "where does the weapon come from" for every objective boss. **[LOCKED] Intentional exception** to
  the loot-tier rule (`ENEMIES.md` §1): these arena adds drop the **required weapon regardless of its normal
  tier** (e.g. the Tank's T4 grenade off tier-1 adds).
- **[LOCKED] Some bosses are never minibosses.** **Phil** and the **Helicopter** are **main-boss-only** —
  never injected as catch-up minibosses, and never reprised as adds (even by Phil).
- **[LOCKED] "Big version" of every enemy.** Any enemy scales up into a tougher fight: **minibosses render
  ~20% bigger** (same art, scaled, buffed), **full bosses ~2× size.** This yields a large miniboss/boss
  pool **cheaply from existing enemy art** — no new sprites, just scale + stat/behavior bumps.

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
- **[LOCKED] Minibosses come in two flavors** (both exist): **scaled-down bosses** (a gimmick + a short
  phase) and **"big version" enemies** (§1 — a regular enemy rendered ~20% bigger and buffed).
- **[LOCKED] Minibosses recur:** once a miniboss has been encountered, it **can spawn again any time after**
  its debut (not a one-time fight) — part of the difficulty ramp in later areas (e.g. the Dixon boss rush).
- **[ITERATE]** do they drop guaranteed loot; can they appear more than once a stage; are they sniper-immune
  like main bosses, or killable by it?

---

## 5. Boss roster

### 5.1 Phil — **FINAL BOSS** — **[LOCKED core]**
- **Look:** like a **Zombie** (`ENEMIES.md` §2.8) but with a **top hat.**
- **The Pencil:** carries a pencil and **draws new enemies into existence** to fight you — an
  artist-summoner.
- **Elusive** — hard to pin down (still fairly reachable, per the no-cheap rule).
- **[LOCKED] Rooftop reveal:** his intro monologue (menacing laughter) reveals he's **found the Holy
  Sharpener** and will **"bring 2D chaos to this 3D planet."** That sharpener is *why* he can keep drawing —
  and **sharpening it is his only vulnerable window** (below).
- **Reprise summons:** can **spawn earlier bosses and minibosses mid-fight** — a greatest-hits gauntlet.
- **[LOCKED] Vulnerability = the sharpen window.** Phil is **invulnerable while he has lead** (drawing his
  army). When he **runs out of lead he must *sharpen* the pencil** — a **3–5s self-stun**, **open and
  bleeding** — **the only time you can damage him.** Cycle: **draw (invuln) → run dry → sharpen (vulnerable
  3–5s) → repeat.** The dull pencil is why he runs dry (`STAGES.md` §1a lead economy).
- **[LOCKED] Arena — Salesforce Tower rooftop (SF):** the tower **sways** — **slight slippage** during the
  fight (you slide with the wind), and **falling off = instant death** (foreshadowed by things shifting during
  his intro cutscene).
- **[LOCKED] Phil is killed by a FINISHER — no other way.** You whittle him during the sharpen-windows;
  the **killing blow must be a finisher**, and at the climax that finisher is **replaced** — the player
  **"shoots" Phil with a laser that fires from the pencil**. This scripted **pencil-laser finisher** is the
  only thing that ends him (it's not gated on the 10% special rule the other bosses use).
- **[LOCKED] The endgame** — the absolute final fight, **exempt from the <2-min rule**, meant to be
  **brutally hard.**
- **[ITERATE]** how lead depletes (does clearing his summons drain it faster?), phases, arena (play-band vs.
  giant), whether the low-HP sniper execution applies to the final boss.

### 5.2 Burly Macho Guy — **boss** — **[LOCKED core]** — **caps Area 1 (department store)**
- **Space-denier bruiser** in the vein of the Heavy/Burly (`ENEMIES.md` §2.11), boss-scale.
- **Ground-spike punch:** punches the ground **fairly quickly** → **spikes erupt near him** that hurt the
  player (a fast close-range AoE — unlike the slow Ground Smasher). Keeps you from face-tanking him.
- **Enemy toss:** **grabs any enemy of any tier and throws it at the player for massive damage** (the
  cannibalize/grab theme at boss scale — ignores the normal tier rule).
- **Psychologically hard, short (<2 min).** **[ITERATE]** spike telegraph/range, throw telegraph, phases,
  miniboss vs. main, HP.

### 5.3 Tank — **objective boss** — **[LOCKED core]** — **caps Area 4 (Vallejo — military nearby)**
- **It's a literal tank.** You **fight regular enemies while dodging its machine-gun fire.**
- **[LOCKED] Win condition = grenades:** when you have a **grenade**, you **climb on top and drop it in the
  hatch. 2 grenade drops = kill.** An objective/puzzle boss, not a health-bar slugfest.
- **Relies on the Grenade** (`WEAPONS.md` §3.2) — grenades are supplied by the **weapon-gated arena rule**
  (§1: tier-1 adds drop only grenades). **[ITERATE]** how you mount it (prompt / climb), the MG fire
  pattern, does it move, what changes between the 1st and 2nd drop, the ~2-min cap.

### 5.4 The Colossus — **boss** — **[LOCKED core]** *(name TBD)* — **caps Area 2 (Sacramento, whip)**
- A **giant stick figure built out of many smaller stick figures.**
- **[LOCKED] Win condition = whip:** you **rip the smaller stick figures off it one at a time with the Whip**
  (`WEAPONS.md` §3.4 — its pull/grab), slowly dismantling the giant piece by piece.
- Weapon-gated → the **weapon-gated arena rule** (§1) supplies whips (tier-1 adds drop only whips).
- **Psychologically hard, short (<2 min).** **[ITERATE]** how many pieces to strip, do torn-off pieces
  become adds, does dismantling change its attacks/phases, how it fights back, arena (likely a giant
  upper-screen boss).

### 5.5 Helicopter (Monkey Chopper) — **boss** — **[LOCKED core]** *(name TBD)* — **caps Area 2 (airport)**
- A **monkey flying a helicopter**, strafing the player. **Shoots stick-figure heads** as projectiles —
  **max 2 on screen at once.**
- **[LOCKED] Two ways to beat it** (both via tier-1 add drops, §1 weapon-gated rule with **two options**):
  - **Bat it back:** grab a **Bat** and **knock the incoming heads back up into the chopper.**
  - **Lob it up:** grab a **Grenade** and **lob it upward** at the chopper.
- **[LOCKED] Never a miniboss** (§1). Psychologically hard, short (<2 min).
- **[NEW WEAPON — Bat]** a projectile-reflecting melee weapon — spec'd at `WEAPONS.md` §3.7b.
- **[ITERATE]** chopper strafe/movement pattern, head-fire cadence, hits to down it, phases (does it descend?).

### 5.6 Gatling Gun Guy — **boss** — **[LOCKED core]**
- Boss-scale version of the **Gatling Gunner** enemy (`ENEMIES.md` §2.7): heavy **machine-gun suppression**
  you must dodge while closing in.
- **[LOCKED] Countered by the Shield Rush** (`PLAYER.md` §3) — **double-tap dodge forward** to advance
  behind an enemy damage-sponge through the gunfire and reach him. The intended "close the gap" answer.
- **[LOCKED] Golden Gate Bridge fight (Area 4 penultimate):** a **barrage every ~5s** with a **"BARRAGE
  INCOMING" on-screen warning**; **hide behind the bridge's cars** or get **eviscerated** (enemies too) —
  cover + timing here rather than the Shield Rush.
- **Psychologically hard, short (<2 min).** **[ITERATE]** fire patterns/phases, does he reposition, HP,
  does he spawn fodder to Shield-Rush behind.

### 5.7 Monkey Boss — **boss** — **[LOCKED core]**
- **Throws dimes into the air**; the player **catches them to summon their own Monkey Mercs** (`WEAPONS.md`
  §3.7).
- **[LOCKED] The player can't damage him directly — only the player's summoned mercs can hurt him.** A
  **proxy war:** win the dimes, field your monkeys, let them shoot him down.
- **[LOCKED] Lose the race, feed the enemy:** if the player **doesn't catch a dime in time, the Monkey Boss
  summons his OWN mercs** — the same gun-monkeys, but **never above tier 1** (kept fair).
- **[LOCKED] No soft-lock:** the boss throws **actual dimes** (not 1¢ coins to accumulate) — **catching one
  summons a merc directly** (boss-specific; no monkey-stick-figure or saved-up change needed), and these
  boss-fight mercs are **OUTSIDE the "3 dead = no more" per-level cap** (`WEAPONS.md` §3.7). You can always
  keep fielding monkeys to damage him.
- **Psychologically hard, short (<2 min).** **[ITERATE]** dime cadence/arc, how many monkeys per side, does
  he move/attack directly at all, phases, HP.

*More bosses welcome — same §5 format.*

---

## 6. Asset needs (per boss) → feeds `ASSET_MANIFEST.md`

For **each *bespoke* boss** (Phil, Tank, Colossus, Helicopter, Monkey Boss, etc.): idle/move · each attack +
**telegraph** · phase-transition · **hurt** · **death** · **sniper-dodge** anim · summoned adds/hazards ·
boss HP bar + name card. **"Big version" bosses/minibosses need NO new art** — they reuse the enemy's
sprites at ~20% (miniboss) or ~2× (boss) scale.

---

## 7. Framework — status

**Resolved (now [LOCKED]):** arena **varies per boss** (play-band brawlers *and* giant upper-screen
threats); **multi-phase** HP-threshold bosses; minibosses are **both** scaled-down bosses **and** elite
enemies; the meter **carries over** in boss fights and becomes a **low-HP sniper execution** (with a prompt).

Plus **[LOCKED]** the "psychologically hard, <2 min (except Phil)" rule.

**Defined bosses (7):** **Phil** (final, artist-summoner, exempt from the length cap), **Burly Macho Guy**
(ground-spikes + enemy-toss bruiser), **Tank** (objective — 2 grenade drops), **Colossus** (whip off its
pieces), **Helicopter** (bat heads back / lob grenades up), **Gatling Gun Guy** (suppression, countered by
Shield Rush), **Monkey Boss** (dime-race, only your mercs damage him). Objective bosses use the
**weapon-gated arena rule**; Phil & Helicopter are **main-boss-only**.
Open to more. Plus **[LOCKED]** the **"big version" rule** (any enemy → ~20% miniboss / ~2× boss, no new art).
**7 bespoke bosses** total: Phil, Burly Macho Guy, Tank, Colossus, Helicopter, Gatling Gun Guy, Monkey Boss.

**Still open (small):** boss HP display style (§3 — big named bar recommended); exact miniboss pace trigger;
whether minibosses are sniper-immune; each boss's `[ITERATE]` details.
