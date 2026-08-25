# this.l — UI / HUD

> **Scope:** the in-combat HUD (the priority) plus a light scaffold of the other screens. Pulls together
> every readout surfaced in `PLAYER.md`, `WEAPONS.md`, `ENEMIES.md`, `GAMEPLAY_LOOP.md`.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[LATER]** parked.
>
> **[AUTHORITY BANNER]** HUD data values come from `TUNING.md`; screens are LOCKED in §5. The `[PROPOSED]`
> markers on HUD *art/layout* below are **cosmetic placement notes**, not open data questions — the **data**
> each element shows is locked. No marker below blocks the build.

---

## 1. Principles — **[LOCKED]**

- **[LOCKED] The bottom half is sacred.** It's the playfield (`GAMEPLAY_LOOP.md` §3). **All persistent HUD
  lives in the top band**, so the busy lower playfield never fights the UI for the player's eyes.
- **[LOCKED] Diegetic-first.** Prefer reading state **off the sprites** over HUD numbers — weapon decay is
  shown on the weapon itself (sword wear, shotgun spine segments), not a bar. HUD carries only what the
  sprite can't.
- **[LOCKED] Chunky-arcade vibe.** Bold pixel frames, thick outlines, loud colors — leans into beat-'em-up
  arcade energy while keeping the persistent HUD in the top band. The **special meter** stays the most
  instantly-legible element (it's a timing decision).

---

## 2. HUD layout — **[PROPOSED]**

```
┌─ TOP BAND (HUD) ─────────────────────────────────────────────────────────────┐
│  ❤ HEALTH ▓▓▓▓▓▓░░ [🗡type]    ┌ combo ┐          $ 0.07     🐒×2  ⏱10s        │
│  ⚡ SPECIAL ▮▮▮▮▮▮ (green)     │ 7 HIT!│                      (merc timers)     │
│                               └───────┘  ← transient, near-center              │
├─ horizon ─────────────────────────────────────────────────────────────────────┤
│                                                                                │
│                        ░░░ PLAYFIELD (bottom half) ░░░                          │
└────────────────────────────────────────────────────────────────────────────────┘
```

- **Top-left:** Health, and directly under it the **Special meter** (the two survival gauges together).
- **Top-right:** **Money** (`$0.07`) and **Monkey Merc** status (count + each one's countdown).
- **Center, transient:** the **combo popup** (`7 HIT!`) — flashes and fades, never persistent.
- **Held weapon:** a **weapon-type icon is shown** (so you always know what you're holding) but **no
  hits-left / ammo counter** — durability & ammo stay **diegetic on the sprite.** **[LOCKED] Icon sits in the
  top-left cluster** by health/meter (not floating above the player).

---

## 3. HUD elements

### 3.1 Health — **[LOCKED]**
- A **pixel bar** (chunky-arcade), top-left.
- **[LOCKED] Damage juice:** when you take a hit, the **pixels you're losing enlarge and then vanish under
  little "explosions"** — each hit reads as a satisfying chunk blown off the bar.
- **[LOCKED] Color states by remaining % (no gaps):** **green ≥50%** → **yellow <50% && ≥20%** → **red <20%**
  (one hit from death). The **20%** yellow→red line is set below the **≤25% rubber-band threshold** (`TUNING.md`
  §2.2) so the bar goes red just after low-HP drops kick in — a readable "danger now" cue.
- **[LOCKED] Max HP = 100** (`ENEMIES.md` §4b damage model).
- **[LOCKED] Low-HP warning:** at **red (<20%)** the bar **pulses** and a subtle **screen-edge vignette pulse**
  turns on (off at ≥20%). Recovery is **heal drops** (rate `ENEMIES.md` §4b, **flat +25% each, no full heals**
  `TUNING.md` §2.2) and checkpoint respawns — no passive regen.

### 3.2 Special meter — **[LOCKED data], [PROPOSED] art**
- Fills from combat: **fists ~30 hits**, weapons ~half that rate, **rapid combos multiply** the fill
  (`GAMEPLAY_LOOP.md` §4.3).
- **[LOCKED] Color tiers:** **yellow** (charged once) → **blue** (twice) → **green** (full / max). Must read
  its **armed** state the instant it's usable.
- **[PROPOSED]** a bold horizontal bar under health that **changes color by tier** and **pulses/gleams when
  green** (ready). This is the HUD's hero element.

### 3.3 Combo popup — **[LOCKED behavior], [PROPOSED] style**
- Flashes `1 HIT!`, `2 HIT!`, … as you chain quickly; **surges the meter** (`PLAYER.md` §3).
- **[PROPOSED]** punchy pixel type near center-screen (above the player), scales up on higher counts,
  **fades on combo drop.** Never persistent.

### 3.4 Money / currency — **[LOCKED data], [PROPOSED] placement**
- Enemies drop **coins** (1¢ each, ~12% chance); **10¢ = a dime**, the Monkey Merc cost (`WEAPONS.md` §3.9).
- **[PROPOSED]** small `$0.07` counter top-right; **[PROPOSED]** it **highlights when you hit a full dime**
  (you can now afford a monkey).
- **[LOCKED] Resets each stage** — spend-it-or-lose-it; monkeys stay a tactical in-stage choice, no
  meta-banking. **Wallet cap = 99¢** (the counter maxes at `$0.99`; in practice you rarely hold >1 dime before
  spending it on a Monkey Merc). **[LOCKED] Overflow is a hard clamp — coins past 99¢ are discarded, never
  wrapped.** So a Pickpocket kill that would pay out **2× the stolen amount** (`ENEMIES.md` §2.16) — e.g. steal
  60¢, kill, +120¢ — **fills to 99¢ and drops the rest** (you don't lose progress, you just can't exceed the
  cap). Any coin pickup while already at 99¢ is a no-op (the coin sprite still despawns).
- **[LOCKED] Hidden until Area 3.** The money counter (and the whole coin/dime cluster) is **not shown in
  Areas 1–2** — coins don't drop there (`TUNING.md` §6.1), and the economy is a **second-half reveal**
  (`WEAPONS.md` §3.9). The counter **fades in when the first coin drops in Area 3.**

### 3.5 Monkey Merc status — **[LOCKED data], [PROPOSED] display**
- Up to **3 summons per level** (`WEAPONS.md` §3.7 — cap on summons made, not deaths); stacking sets their weapon & lifespan (pistol 20s / shotgun 10s /
  rocket 5s), all at 2 shots/sec (`WEAPONS.md` §3.7).
- **[PROPOSED]** top-right cluster: **monkey icons ×count**, each with a **shrinking timer ring**; a spent
  "3 summons used — no more this level" state shown greyed out.

### 3.5b Sniper special targeting — **[LOCKED]**
- During the **sniper time-slow** (`TUNING.md` §3.1), a **red targeting dot** appears on each enemy the
  auto-chain ricochet will pass through, **in the order the ricochet has already picked** (there is no manual
  aiming — the chain auto-selects nearest-un-hit-head greedily, `TUNING.md` §3.1; the dots just *show* that
  computed order as the slow plays out). The readout that makes the wipe feel aimed. Renders at full saturation
  over the desaturated time-slow overlay (`VFX.md` §8). Clears when the special ends.

### 3.5d Execute prompt — **[LOCKED]**
- When an HP-depletion boss drops to **≤10% HP** (`BOSSES.md` §1), a **`▶ SPECIAL` execute prompt** flashes
  over the boss (and the special-meter HUD pulses gold) telling the player their charged special will now
  **execute**. Clears if the boss's HP somehow rises above 10% (it can't) or on the kill. Objective/proxy
  bosses never show it (they have no execute).

### 3.5c Boss HP bar & name card — **[LOCKED]**
- On a boss fight, a **big dedicated bar spans the top of the screen** (under the HUD band) with the **boss's
  name card** on entry (chunky-arcade, same palette). This is the one time a large HP readout is warranted
  (`BOSSES.md` §3).
- **Segmented by phase** — the bar shows the phase thresholds (e.g. Burly 66%/33%) as notches; it flashes on a
  phase transition.
- **[LOCKED] Objective/proxy bosses show a PROGRESS readout, not an HP bar:**
  - **Helicopter** → **6 pips** (reflected-head / lobbed-grenade progress, a grenade = 1.5 pips).
  - **Tank** → **2 pips** (grenade drops).
  - **Colossus** → **6 segments** (pieces remaining).
  - **Monkey Boss** → a normal HP bar (200), since your mercs deplete it.
  - **Phil** → HP bar with the **4 sharpen-threshold notches** (100/75/50/25); the bar only moves during a
    sharpen window (`BOSSES.md` §5.1).
- **[LOCKED] Style:** chunky pixel bar with a bold outline, boss name in the arcade font, a small boss-face
  icon at the left cap.

### 3.6 Weapon-specific readouts — **[PROPOSED]**
Most decay is diegetic, but a few weapons have HUD-worthy state:
- **Boomerang Gun:** **10 bullets** (4/pass) — **[PROPOSED]** show remaining bullets only while equipped.
- **Ball & Chain:** **3 uses** + the ~20% slow — **[PROPOSED]** a tiny use-pip cluster.
- Everything else (sword wear, shotgun spine, shotgun/pistol ammo, gatling no-ammo) stays **diegetic on the
  sprite**.
- **[LOCKED] Pistol/Revolver diegetic ammo readout (what the artist draws):** there is **no numeric counter**.
  The read is **the ejected casing per shot + the discard on empty** — each `E`-fire **flips out a spent casing**
  (a tiny casing sprite arcs off, `VFX.md`), and when the mag empties (8 / 6) the gun is **thrown away with a
  visible toss** and the character snaps back to fists. So "rounds left" is felt as *how many casings have flown*
  and the imminent discard, not a bar. The **Revolver additionally shows its cylinder** (6 chambers) — no need
  to animate each, the discard is the definitive tell. This is the diegetic pistol asset: **in-hand → fire +
  casing-eject → empty-discard toss.**

---

## 4. Diegetic vs. HUD split — **[PROPOSED]** summary

| State | Where it's read |
|---|---|
| Weapon durability/ammo (sword, shotgun spine, pistol mag) | **Diegetic** (on the held weapon) |
| Which weapon you hold (type) | **HUD icon** (type only, no counters) + diegetic in-hand |
| Health, Special meter, Money, Monkey timers, Combo | **HUD** (top band) |
| Boomerang-gun bullets, Ball&Chain uses | **HUD, only while equipped** |

---

## 5. Other screens — **[LOCKED] concrete scaffold** (outside the tight loop)

All are **chunky-arcade**, same palette/fonts as the HUD, controller+keyboard navigable. Minimal art (text +
a few sprites), so they stay lightweight.

| Screen | Contents & behavior — **[LOCKED]** |
|---|---|
| **Title / main menu** | game logo + the `this.l` drawing motif; menu: **Start · Continue · Endless · Options · Quit**. **[LOCKED] "Start"** launches the **full new-run flow — Character Select → Difficulty Select → begin** (the locked `character → difficulty → start` order, see Difficulty Select below). There is **no standalone "Character Select" menu item**: character selection is simply the **first screen of Start**, so the menu list and the flow agree. **"Continue"** resumes a **FRESH run at the beginning of the furthest stage reached** (per the save block below — full HP, 3 continues, empty wallet/meter, fists); it is greyed out when no stage past Stage 1 has been unlocked. Background = a slow parallax of the SF skyline. |
| **Character Select** | **the first screen of the Start flow** (reached only via **Start**, not a separate menu item): the **4 characters** (`CHARACTERS.md`) as portraits + name + one-line Special; left/right to pick, confirm to **advance to Difficulty Select**. Shows each one's Special icon. |
| **Difficulty Select** | **Easy · Normal · Hard** (`TUNING.md` §8.4), one-line each (Easy = fewer, softer enemies; Hard = 2× the crowd, +50% enemy damage). Default **Normal**. **[LOCKED] Flow order = character → difficulty → start:** Difficulty Select comes **AFTER** Character Select — you pick a character, then a difficulty, then the run starts. The chosen difficulty shows as a small HUD badge in-run. |
| **Pause** (mid-run) | dims the frame; **Resume · Restart checkpoint · Options · Quit to title**. Freezes the sim. **"Restart checkpoint" is a voluntary respawn — it COSTS a continue** (same as dying) and restores full HP. This closes the free-heal loophole: full HP only ever comes with a continue spent (`TUNING.md` §8.1). If no continues remain, the option is greyed out. **[LOCKED] In Endless the Pause menu shows only Resume · Options · Quit to title — NO "Restart checkpoint" option** (Endless has no checkpoints and no continues, `TUNING.md` §8.3; the item is omitted entirely, not greyed). |
| **Options** | volume (music/SFX sliders), fullscreen/windowed, integer-scale toggle, **rebinding** (the `PLAYER.md` §2 [ITERATE] lives here), a "reduce screen-shake" accessibility toggle. **[LOCKED] Rebinding screen:** lists **every action — move, jump, attack-directions, dash, `E`, `F`, special, pause** — in a table with **remappable keyboard-key + controller-button columns** (one row per action, each cell re-bindable) and a **Reset to default** action. |
| **Area card** (stage transition) | a 2-second card: the **next area's name** (linear, `STAGES.md`), with the incoming genre stinger (`AUDIO.md` §2). No path choice (linear). |
| **Results / grade** (post-stage) | **cosmetic** score: enemies felled, best combo, time, a letter grade — **no gameplay effect** (`STAGES.md`). Advances on confirm. **[LOCKED] Grade formula (cosmetic):** `stage_points = kills×10 + best_combo×5 + time_bonus`, where `time_bonus = max(0, 2000 − seconds_taken×5)`. Letter cutoffs: **S ≥ 1200 · A ≥ 900 · B ≥ 600 · C ≥ 300 · D < 300** **(tunable)**. Nothing about the grade gates progression or unlocks. |
| **Game over** | appears when **all continues are spent** (`TUNING.md` §8.1), or on the **first death in Endless** (no continues there, `TUNING.md` §8.3). Options: **Restart the current stage from its start** (fresh continue count) · **Quit to title** *(Endless: **Retry** restarts a fresh Endless run)*. **In Endless it also shows the run's FINAL SCORE + best score** (`TUNING.md` §8.3). *(Mid-stage campaign deaths that still have continues left respawn at the last checkpoint without this screen.)* |
| **Controls / tutorial prompts** | contextual button-prompt overlays during the first stage's vignette (the teaching device), not a separate screen. |

- **[LOCKED] Save:** a single **auto-save at each checkpoint + area boundary** (records **furthest stage
  reached** and **Endless best score** only). **All 4 characters are available from the start — there are no
  character unlocks** (`CHARACTERS.md`); the save has no progression to gate. No manual save slots
  (arcade-style). *(This resolves the `ASSET_MANIFEST.md` §10 save/settings open item.)*
- **[LOCKED] Resume semantics (what the save does and does NOT restore).** The save is a **stage bookmark, not
  a mid-run snapshot** — a *run* (HP, wallet, held weapon, continue count, active meter, current checkpoint
  within a stage) **lives only in memory and is never serialized.**
  - **Quitting to title mid-stage (or mid-run) discards that run entirely.** On next launch the title's
    **"Continue" starts a FRESH run at the beginning of the furthest stage reached** — full HP, **3 continues
    restored**, empty wallet, fists, empty meter. You never resume mid-stage or keep a partial continue count
    across a quit; you only keep *which stage you unlocked.*
  - **The in-stage checkpoint (`TUNING.md` §8.1) is a within-session respawn point only** — it is not written
    to disk, so it is lost on quit (you restart that stage from its start, exactly like the Game-Over
    "Restart the current stage" option, but with the furthest-stage bookmark intact).
  - **Endless** persists **best score** only; each Endless entry is a fresh run.

---

## 6. UI asset list → feeds `ASSET_MANIFEST.md`

All in the **chunky-arcade** style (bold pixel frames, thick outlines).
- **Health bar** — full→empty pixels, **damage animation** (losing pixels enlarge → blow off under small
  **explosion VFX**), and **green / yellow / red** color states.
- **Special meter** — yellow / blue / green tiers + **armed pulse/gleam** when green.
- **Combo popup** — number + `HIT!`, scale-up states, fade.
- **Weapon-type icons** — one per weapon (type indicator, no counters).
- **Money counter** — glyphs + **full-dime highlight**.
- **Monkey status cluster** — monkey icon, **timer ring**, spent/greyed state.
- **Boomerang-gun bullet pips**, **Ball & Chain use pips** (equip-only).
- **Sniper red-dot** — **two variants:** (a) **player-special** dots (the ricochet-chain readout over enemies,
  §3.5b) and (b) the **enemy-Sniper's red dot painted on the player's head** while an airborne player is scoped
  (`ENEMIES.md` §2.14 / `AREAS.md` §4) — same dot sprite, different owner.
- **Boss HP bar + name card + small boss-face icon** — the boss-face icon (one per boss, capping the bar,
  §3.5c) is drawn from the boss's own portrait; **objective-boss progress readouts** (Helicopter 6-pip, Tank
  2-pip, Colossus 6-segment, Monkey Boss HP-200, Phil HP+4-notches, §3.5c).
- **Execute prompt** — the `▶ SPECIAL` glyph over a ≤10% executable boss (§3.5d).
- **"BARRAGE INCOMING" warning** banner (`BOSSES.md` §5.6 — the barrage warning behavior lives there; §3.5b is the sniper red-dot spec, not this).
- **Character-select Special icons** — **one icon per character's Special** (Sniper / Giant Shotgun / Werewolf
  / Vaporize) shown on the select screen (§5).
- **Non-HUD screens (§5, now LOCKED):** title/main-menu, character-select, pause, options (+rebinding), area
  card, results/grade, game-over — chunky-arcade, text + a few sprites. Fonts + button-prompt glyph set.

---

## 7. Decisions — status

**Resolved (now [LOCKED]):** pixel health bar with chunk-enlarge → explosion damage juice and green/
yellow/red states; **chunky-arcade** HUD vibe; **weapon-type icon shown, no ammo/durability counters**
(those stay diegetic); **money resets each stage.**

**Still open (cosmetic only):** whether the full-dime highlight needs a stronger "you can summon" cue.
*(Weapon-icon placement locked top-left §2; max HP = 100 §3.1.)*
**Non-HUD screens (§5): now LOCKED** (title, character-select, pause, options+rebinding, area card,
results/grade, game-over, auto-save).
