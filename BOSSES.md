# this.l — Bosses & Minibosses

> **Scope:** boss fights and the catch-up minibosses — system rules, structure, arena behavior, and per-boss
> specs (filled in as you dump ideas, like `ENEMIES.md`). Ties to `GAMEPLAY_LOOP.md` §7 (stage endpoints)
> and the enemy/weapon systems.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[ITERATE]** flesh out next · **[LATER]** parked.

---

## 1. System rules

- **[LOCKED — GLOBAL] No boss or miniboss can be swept or knocked down.** They have permanent super-armor vs.
  the sweep (hit 3); the **player's finisher double-tap does nothing special to them** — its hits land as
  normal melee (`TUNING.md` §2.6). This applies to **all 10 boss encounters** (7 bespoke + the 3 big-versions
  Sandwich Bros / big Arm-Ripper / Boomergunner) **and every catch-up miniboss**. Bosses are defeated only by
  **HP depletion, their objective, or a scripted kill** — plus the ≤10% special execution below.
- **[LOCKED] Specials execute a boss only at **≤10% HP** — for ALL characters.** Above 10%, a boss **negates
  the special** (the Tactical's sniper visibly **dodges**; Werewolf / Shotgun / Underdog specials do nothing).
  At **≤10% a prompt appears and the special *executes* the boss** — the one time any special ends a boss.
  **The `≤10%` boundary is inclusive** (exactly 10% executes); one rule, all four characters.
- **[LOCKED] The execute rule applies ONLY to the 5 pure HP-depletion bosses** (Burly, big Arm-Ripper,
  Boomergunner, Gatling Gun Guy, Sandwich Bros) — the ones whose HP bar can actually reach ≤10%. **The other 5
  have NO execute:** **Colossus** (whip-objective — HP is 6 discrete pieces, can never sit in the 0–10% band,
  §5.4), **Tank** & **Helicopter** (objective), **Monkey Boss** (proxy — a player special can't damage him at
  all), and **Phil** (scripted pencil-laser only, §5.1). Specials fired at these five simply whiff (the sniper
  dodges as usual). **This is the authoritative scope** — it matches `TUNING.md` §9 item 4 (the two docs agree:
  only the 5 pure HP-depletion bosses execute; the 5 objective/proxy bosses do not).
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
- **[LOCKED] Arena add economy (default for the standard bosses).** Weapon-gate / fodder adds obey: **max 2
  adds alive at once**, **one respawns 3 s after an add dies or is consumed** (thrown/whipped/ripped), from the
  arena's add-port(s) (`ENCOUNTERS.md` arena table). This covers **Burly** (2 toss-fodder Regulars),
  **Colossus** (2 whip-gate adds — *separate* from the torn pieces that become adds, §5.4), **Helicopter**,
  **Tank**, and **Gatling Gun Guy** (his "1–2 Regular fodder between barrages", §5.6). **Exceptions with their
  own cadence:** **Monkey Boss** (dime cadence, §5.7) and **Phil** (his
  lead-cost draw cadence, up to **8 adds**, §5.1) — these override the 2-cap. Colossus's **torn-piece adds do
  NOT count against the 2 gate-adds** (they're the objective's byproduct). Adds **stop spawning** once the boss
  is defeated. **The 3 big-version bosses fight SOLO — NO adds:** **Sandwich Bros**, **big Arm-Ripper**, and
  **Boomergunner boss** spawn no arena adds (their "add ports" in the `ENCOUNTERS.md` arena table are unused —
  they're pure 1-v-1 HP-depletion fights). Only the 5 standard bosses above (+ the 2 override bosses) have
  adds. **This add cadence is difficulty-independent** — the `TUNING.md` §8.4 spawn multiplier scales
  *stage waves*, **not** boss-arena adds (the 2-cap holds on every difficulty).
- **[LOCKED] Weapon-gated boss arenas guarantee the weapon.** When a boss requires a specific weapon (Tank →
  grenade, Colossus → whip), the arena's **2 tier-1 adds drop ONLY that weapon** on death — so the player can
  always re-arm within the 3 s respawn cadence above. The **Helicopter arena needs both tools** (reflect heads
  with a Bat *and* lob grenades up), so its **two adds split by fixed assignment: one add always drops a Bat,
  the other always drops a Grenade** — never a random roll, so both objective tools are guaranteed present at
  once. This
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

## 3. Boss UI — **[LOCKED]** (full spec in `UI.md` §3.5c)

- **[LOCKED] Boss health bar** — a big dedicated bar (top of screen, under the HUD band), named boss,
  segmented by **phase**, chunky-arcade. **Objective/proxy bosses show a progress readout instead** (Helicopter
  6-pip, Tank 2-pip, Colossus 6-segment, Monkey Boss HP-200, Phil HP+4-notches) — see `UI.md` §3.5c.
- **[LOCKED]** phase-change flash + name card on entry.

---

## 4. Miniboss framework — **[PROPOSED]**

- **Trigger:** injected when **pace is too fast** (`ENEMIES.md`). **[ITERATE]** the exact pace metric
  (time-to-clear? kill rate?).
- **[LOCKED] Minibosses come in two flavors** (both exist): **scaled-down bosses** (a gimmick + a short
  phase) and **"big version" enemies** (§1 — a regular enemy rendered ~20% bigger and buffed).
- **[LOCKED] Minibosses recur:** once a miniboss has been encountered, it **can spawn again any time after**
  its debut (not a one-time fight) — part of the difficulty ramp in later areas (e.g. the Dixon boss rush).
- **[LOCKED, resolved]** minibosses **drop guaranteed loot** (a T2+ weapon from the current area pool), **can
  appear more than once a stage** (recurring, §above), and are **NOT sniper-immune** — they are big-version
  *elites*, sniper-killable like normal enemies. Only the **10 bosses** resist the sniper (`TUNING.md` §7).

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
- **[LOCKED, resolved]** the **low-HP special execution does *not* apply to Phil.** Every other boss dies to a
  special once at ≤10% HP; Phil is the sole exception — specials never execute him, and the **scripted
  pencil-laser finisher is the only kill** (above). This is deliberate: it forces the player to reach the final
  finisher rather than melting him with a banked meter.
- **[LOCKED] Full fight script (authority for the finale beats; `ENCOUNTERS.md` defers here):**
  - **HP 500, gated behind sharpen windows** (`TUNING.md` §7): thresholds at **100% → 75% → 50% → 25%**. You
    can only damage him **during a sharpen window**; the **per-window damage cap is 125 HP (25%)**, so a clean
    run needs **exactly 4 windows** (4 × 125 = 500). Hit the cap and the window ends early; **the window that
    takes his gated HP to ≤0 IS the execute window** — there is no separate extra window. On a *clean* run
    that's the **4th** window; under-damage earlier windows and it simply takes more (each still capped at 125).
  - **Lead pool & the dry-out clock (LOCKED costs):** each draw cycle he starts with **12 lead-points**. Per-
    summon cost: **Regular = 2 · Swarmer pod = 3 · reprise miniboss = 6 · Heavy = 4.** He keeps drawing (one
    summon whenever he has the lead and a free add-slot, ~every 1.5 s) until he **can't afford the next
    summon** → he runs dry and must sharpen. **Killing his adds faster empties the field, so he keeps spending
    lead to refill → dries out sooner** (fewer live adds = more draws = faster to the window). This is the core
    loop lever: aggressive add-clearing shortens the invuln phase. (Killing an add gives Phil nothing back;
    it just accelerates his spend.)
  - **Draw phase (invuln):** he sketches adds at the arena's back edge. **Summon roster by threshold** (the
    greatest-hits reprise): 100–75% → **Regulars + Swarmer pods**; 75–50% → **+ a reprise miniboss**; 50–25% →
    **+ a second reprise miniboss**; 25–0% → **+ Heavies**. **Reprise-miniboss selection:** **random from the
    threshold's pool, never repeating the last one** — 75–50% pool = {big Snapper, big Head-Thrower}; 50–25%
    pool = {big Arm-Ripper, big Ninja}. Max **8 adds** on screen (Phil's own cap, overrides the §1 2-cap). **A
    drawn Swarmer pod counts as its live swarmers toward the 8** (and may briefly exceed it, the standard pod
    exception, `TUNING.md` §4) — Phil won't draw a new pod while at the cap.
  - **Sharpen window (vulnerable 3–5 s):** when dry he **stops, hunches, and sharpens** — open and bleeding.
    Deal up to the **125-HP cap**; the window **ends early if you hit the cap**, else closes at **5 s** (a fast
    player caps it in ~3 s, a slow one gets the full 5). Matches `TUNING.md` §7.
  - **[LOCKED] Under-damaging a window:** the **cap is a ceiling, not a requirement** — if you deal *less* than
    125 in a window, Phil just re-arms with **whatever HP you left him**; the thresholds (75/50/25%) are only
    *waypoints* for the summon-roster escalation, not gates you must land exactly on. The **execute window is
    simply the one where his HP reaches ≤0 of the gated pool** — you keep getting sharpen windows until then.
    (Because the cap ≥ one threshold's worth, a clean run is 4 windows; a sloppy run just takes more windows.)
  - **Arena — [LOCKED]:** a **play-band brawler on the swaying rooftop** (30 × 8 wu, `ENCOUNTERS.md`), **not**
    a giant upper-screen boss. **Sway/slippage** shifts your footing by up to **±1.5 wu** on a **~6 s sine**
    (telegraphed by the skybox tilting); **two edges have no railing → fall = instant death** (`TUNING.md`
    §6.2). He never falls; adds can be knocked off.
  - **The kill:** Phil is **never swept/knocked down** (bosses can't be, `TUNING.md` §2.6). **The sharpen window
    in which his gated HP reaches ≤0 becomes his finisher-able state** — during that window a **finisher input
    (a `→→`/`↓↓` double-tap toward him, `PLAYER.md` §3) triggers the scripted pencil-laser**: the finisher
    animation is replaced by the player firing a laser from the pencil. (You reach ≤0 by landing the last 125
    of damage; the same window then accepts the laser input — no waiting for a further window.) **Specials never
    execute Phil** (the special-exemption rule above); this scripted finisher is the only kill.
  - **Length:** exempt from <2:00; target **~5–8 min** (`TUNING.md` §7).

### 5.2 Burly Macho Guy — **boss** — **[LOCKED core]** — **caps Area 1 (department store)**
- **Space-denier bruiser** in the vein of the Heavy/Burly (`ENEMIES.md` §2.11), boss-scale.
- **Ground-spike punch:** punches the ground **fairly quickly** → **spikes erupt near him** that hurt the
  player (a fast close-range AoE — unlike the slow Ground Smasher). Keeps you from face-tanking him.
- **Enemy toss:** **grabs any enemy of any tier and throws it at the player for massive damage** (the
  cannibalize/grab theme at boss scale — ignores the normal tier rule).
- **[LOCKED] Attack pattern** (HP 300, `TUNING.md` §7):
  - **Ground-spike:** windup **0.6 s** (raises fist, ground glows) → spikes erupt in a **4 wu radius** for
    22.5 dmg; cooldown **2.5 s**. Telegraphed glow lets you dash out.
  - **Enemy-toss:** grabs an add and hurls it (windup **0.8 s**, a clear over-the-head pose) — **40 dmg**,
    travels the lane; **dodge by changing Z-row.** Only usable when an add is alive.
  - **Charge (Phase 2, ≤66%):** a **shoulder rush** across the lane at 12 wu/s, floors on contact (H-weight).
  - **Phase 3 (≤33%):** spike cooldown drops to **1.5 s** and he pairs spike→charge.
- **Adds:** 2 Regulars stream in so he always has toss fodder (`ENCOUNTERS.md` arena). **Main boss** (Area-1
  cap), not a miniboss. Psychologically hard, short (<2 min).

### 5.3 Tank — **objective boss** — **[LOCKED core]** — **mid Area 4 (Vallejo — military nearby)** *(Gatling Gun Guy caps Area 4)*
- **It's a literal tank.** You **fight regular enemies while dodging its machine-gun fire.**
- **[LOCKED] Win condition = grenades:** when you have a **grenade**, you **climb on top and drop it in the
  hatch. 2 grenade drops = kill.** An objective/puzzle boss, not a health-bar slugfest.
- **Relies on the Grenade** (`WEAPONS.md` §3.2) — grenades are supplied by the **weapon-gated arena rule**
  (§1: tier-1 adds drop only grenades).
- **[LOCKED] Fight pattern:**
  - **MG fire:** sweeps a **horizontal beam** across one Z-row at a time, telegraphed by the turret rotating
    **0.7 s** before it fires; **1 dmg/hit** stream for **1.5 s**, then re-aims to a new row. Stay off the
    lit row.
  - **Mounting:** approach the **rear tread** (a glowing prompt appears when you're within 2 wu **and holding
    a grenade**) → **hold `F` 0.5 s** to climb → auto-walk to the hatch → **`E`** drops the grenade in.
  - **After drop 1 (Phase 2):** the tank **reverses and repositions** once, and the MG adds a **second
    sweeping row** (two lit rows). Adds keep dropping grenades so you can re-arm.
  - **Drop 2 = kill.** It does not move otherwise (it's a turret-puzzle). ~1:50 cap (`TUNING.md` §7).

### 5.4 The Colossus — **boss** — **[LOCKED core]** *(name TBD)* — **mid Area 2 (Sacramento, whip)** *(Helicopter caps Area 2)*
- A **giant stick figure built out of many smaller stick figures.**
- **[LOCKED] Win condition = whip:** you **rip the smaller stick figures off it one at a time with the Whip**
  (`WEAPONS.md` §3.4 — its pull/grab), slowly dismantling the giant piece by piece.
- Weapon-gated → the **weapon-gated arena rule** (§1) supplies whips (tier-1 adds drop only whips).
- **[LOCKED] Structure (HP = 6 pieces, `TUNING.md` §7):** a **giant upper-screen boss** reaching down into the
  play-band. **Pieces are removed ONLY by the whip-pull** — **one successful forward whip-pull rips off one
  piece** (regular attacks do **not** chip pieces; the whip is required, which is why the arena is whip
  weapon-gated). The "6 × 40" in `TUNING.md` §7 is just the **HP-bar representation** (240 shown as 6 segments)
  — mechanically it is **6 pulls to win**, not a damage race. Each torn piece **becomes a T1 add** on the
  ground (crowd pressure as you dismantle).
- **[LOCKED] Colossus is an OBJECTIVE boss (no ≤10% execute).** Because its "HP" is 6 discrete pieces, it can
  never sit in the 0–10% band, so **no special executes it** (`BOSSES.md` §1) — you must strip all 6 pieces.
  The general "everything except the sniper works in a boss fight" (§1) is **overridden here**: only the
  **whip-pull** removes pieces; other weapons/combos hit the adds and let you survive, but do not advance the
  win. (The meter is still worth building for the adds and the *next* fight.)
- **[LOCKED] Attacks:**
  - **Body swipe:** a slow overhead arm sweep across the lane, windup **0.9 s**, **22.5 dmg**, cooldown 3 s.
  - **Piece-spit:** flings a loose stick-figure at you (like Burly's toss but weaker, **15 dmg**), every 4 s.
  - **At 4 pieces left:** swipe cooldown → 2.5 s. **At 2 pieces left:** it **flails faster** (swipe 2 s) and
    spits two at once — the "cornered giant" phase.
- Weapon-gated arena supplies whips. Psychologically hard, short (<2 min).

### 5.5 Helicopter (Monkey Chopper) — **boss** — **[LOCKED core]** *(name TBD)* — **caps Area 2 (airport)**
- A **monkey flying a helicopter**, strafing the player. **Shoots stick-figure heads** as projectiles —
  **max 2 on screen at once.**
- **[LOCKED] Two ways to beat it** (both via tier-1 add drops, §1 weapon-gated rule with **two options**):
  - **Bat it back:** grab a **Bat** and **knock the incoming heads back up into the chopper.**
  - **Lob it up:** grab a **Grenade** and **lob it upward** at the chopper.
- **[LOCKED] Never a miniboss** (§1). Psychologically hard, short (<2 min).
- **[NEW WEAPON — Bat]** a projectile-reflecting melee weapon — spec'd at `WEAPONS.md` §3.7b.
- **[LOCKED] Objective, not HP (`TUNING.md` §7):** you don't chip a health bar — **6 reflected heads OR 4
  lobbed grenades bring it down** (a lobbed grenade counts as 1.5, so 4 finish it). Mixed reflects/lobs add up.
- **[LOCKED] Pattern:**
  - **Altitude:** hovers at **~5 wu up** in Phase 1 (the top band), **descending to ~3 wu** in Phase 2 — within
    the grenade anti-air lob's ~6 wu reach (`WEAPONS.md` §3.2) and the batted-head return arc at both altitudes.
  - **Strafe:** flies **left↔right across the top band** at 8 wu/s, dipping toward whichever Z-row you're on.
  - **Head-fire:** lobs stick-figure heads (max **2 airborne**) at your position, **one every 2.5 s**, arced
    telegraph 0.5 s — **bat them back up** (reflect window 0.20 s) to score a hit, or **lob a grenade up**.
  - **Phase-2 trigger = 3 objective *events* landed** (a reflect or a lob each count as **1 event** toward the
    phase change — this is the event count, distinct from the **6-pip win meter** where a grenade is worth 1.5,
    `UI.md` §3.5c). At Phase 2 it **descends to ~3 wu**, head-fire cadence → 1.8 s, and adds a short horizontal
    **rotor-gust** pushing you toward a Z-edge.
- **Never a miniboss** (§1). Psychologically hard, short (<2 min).

### 5.6 Gatling Gun Guy — **boss** — **[LOCKED core]**
- Boss-scale version of the **Gatling Gunner** enemy (`ENEMIES.md` §2.7): heavy **machine-gun suppression**
  you must dodge while closing in.
- **[LOCKED] Two separate threats, two separate counters:**
  - The **regular Gatling Gunner enemy** (`ENEMIES.md`) fires a **1-HP/hit stream** — that stream is what the
    **Shield Rush** soaks (up to 40 dmg, `TUNING.md` §2.3) so you can close the gap. Shield Rush is the counter
    to the *enemy's stream*, and to this **boss between barrages**.
  - The **boss's `BARRAGE`** is a different move: **instant death in the open — a Shield Rush does NOT beat it**
    (the shield soaks a chip stream, not an instant-kill barrage; a human shield can't stop it). The **only**
    counter to the barrage is **hard cover (a parked car)** on the ~5 s "BARRAGE INCOMING" telegraph. So: **cars
    for the barrage, Shield Rush for the between-barrage stream/fodder.** No overlap, no ambiguity.
- **[LOCKED] Pattern (HP 260, `TUNING.md` §7):**
  - **Barrage cycle:** **~5 s** between barrages, each preceded by the **"BARRAGE INCOMING"** warning
    (2 s lead). The barrage is **instant death in the open** — you must be **behind a car** (hard cover) or
    off his firing row. Barrage lasts 1.5 s.
  - **Between barrages:** he **repositions** one car-length and **spawns 1–2 Regular fodder** you can
    **Shield-Rush** behind to close distance (`PLAYER.md` §3) and land your fist string (~32) or a weapon hit.
  - **Phase 2 (≤66%):** barrage cadence → 4 s. **Phase 3 (≤33%):** he fires **two rows** per barrage, forcing
    a specific car.
- **Caps Area 4** (`STAGES.md` §4). Psychologically hard, short (<2 min).

### 5.7 Monkey Boss — **boss** — **[LOCKED core]**
- **Throws dimes into the air**; the player **catches them to summon their own Monkey Mercs** (`WEAPONS.md`
  §3.7).
- **[LOCKED] The player can't damage him directly — only the player's summoned mercs can hurt him.** A
  **proxy war:** win the dimes, field your monkeys, let them shoot him down.
- **[LOCKED] Lose the race, feed the enemy:** if the player **doesn't catch a dime in time, the Monkey Boss
  summons his OWN mercs** — the same gun-monkeys, but **never above tier 1** (kept fair).
- **[LOCKED] No soft-lock:** the boss throws **actual dimes** (not 1¢ coins to accumulate) — **catching one
  summons a merc directly** (boss-specific; no monkey-stick-figure or saved-up change needed), and these
  boss-fight mercs are **OUTSIDE the 3-summons-per-level cap** (`WEAPONS.md` §3.7). You can always
  keep fielding monkeys to damage him.
- **[LOCKED] Live-merc ceiling in this fight = 3 at once.** The dime bypass lifts the *per-level summon count*
  cap, not the squad-size model: **at most 3 mercs are alive simultaneously** (the `WEAPONS.md` §3.7 tier table
  only defines 1/2/3 live). Catching a dime while **3 are already alive** spawns nothing extra — it instead
  **refreshes the squad** (re-arms all three to their current tier and resets their lifespan timers), so a
  steady catch-rate keeps 3 rockets-tier mercs up rather than stacking a 4th.
- **[LOCKED] Pattern (HP 200, only your mercs damage him, `TUNING.md` §7):**
  - **Dime toss:** he lobs a **dime** in a high arc **every 4 s**, landing at a telegraphed spot — **run under
    it and catch (`F`)** to spawn a merc directly (boss-fight mercs ignore the 3-summons cap, but still cap at
    3 alive at once — above).
  - **He does not attack the player directly** (0 direct dmg) — the threat is **his own mercs** (T1 pistol,
    7.5) if you lose the dime race, plus positioning.
  - **Phase 2 (≤60%):** dime cadence → 3 s (faster race). **Phase 3 (≤30%):** he throws **two dimes at once**
    to opposite sides, forcing a choice.
- **[LOCKED] Merc math (why it's paced, not a melt):** boss-fight mercs are **hard-capped at PISTOL tier**
  (8/shot @ 2/s = 16 DPS each; `BOSSES.md` above — *his* mercs stay ≤T1 and so do yours in this fight, to keep
  the proxy race legible). They **never escalate to shotgun/rocket tier here** — the §3.7 tier ladder is
  suspended for the Monkey Boss arena precisely so a lucky rocket squad can't delete him in a second. So 3 live
  pistol mercs = 48 DPS; clearing 200 HP takes **~4 s of uninterrupted fire**, but you keep losing and
  re-earning the squad as the dime race swings — which is what stretches it to the ~1:55 length window.
  Psychologically hard, short (<2 min).

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
Plus **[LOCKED]** the **"big version" rule** (any enemy → ~1.2× miniboss / ~2× boss, no new art).
**10 boss encounters total = 7 bespoke** (Phil, Burly, Tank, Colossus, Helicopter, Gatling Gun Guy, Monkey
Boss) **+ 3 big-version, no new art** (Sandwich Bros, big Arm-Ripper, Boomergunner boss).

**[LOCKED, resolved]:** boss HP display = **a big named bar + name card** (fully specced in `UI.md` §3.5c —
incl. the progress readouts for objective bosses: Helicopter 6-pip, Tank 2-pip, Colossus 6-segment; top of screen);
**miniboss pace trigger** = `TUNING.md` §8.2 (avg kill-interval < 3 s for 20 s); **minibosses are NOT
sniper-immune** — they are big-version *elites*, sniper-killable like normal enemies (only the 10 *bosses*
resist the sniper, `TUNING.md` §7). Per-boss patterns are pinned in §5.1–5.7 above.
