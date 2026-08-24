# this.l — UI / HUD

> **Scope:** the in-combat HUD (the priority) plus a light scaffold of the other screens. Pulls together
> every readout surfaced in `PLAYER.md`, `WEAPONS.md`, `ENEMIES.md`, `GAMEPLAY_LOOP.md`.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[LATER]** parked.

---

## 1. Principles — **[LOCKED]**

- **[LOCKED] The bottom half is sacred.** It's the playfield (`GAMEPLAY_LOOP.md` §3). **All persistent HUD
  lives in the top band**, so dense bullet-hell never fights the UI for the player's eyes.
- **[LOCKED] Diegetic-first.** Prefer reading state **off the sprites** over HUD numbers — weapon decay is
  shown on the weapon itself (sword wear, shotgun spine segments), not a bar. HUD carries only what the
  sprite can't.
- **[PROPOSED] Minimal & glanceable.** A few bold, high-contrast readouts; the **special meter** is the
  one element that must always be instantly legible (it's a timing decision).

---

## 2. HUD layout — **[PROPOSED]**

```
┌─ TOP BAND (HUD) ─────────────────────────────────────────────────────────────┐
│  ❤ HEALTH ▓▓▓▓▓▓░░           ┌ combo ┐            $ 0.07     🐒×2  ⏱10s        │
│  ⚡ SPECIAL ▮▮▮▮▮▮ (green)    │ 7 HIT!│                        (merc timers)    │
│                              └───────┘  ← transient, near-center               │
├─ horizon ─────────────────────────────────────────────────────────────────────┤
│                                                                                │
│                        ░░░ PLAYFIELD (bottom half) ░░░                          │
└────────────────────────────────────────────────────────────────────────────────┘
```

- **Top-left:** Health, and directly under it the **Special meter** (the two survival gauges together).
- **Top-right:** **Money** (`$0.07`) and **Monkey Merc** status (count + each one's countdown).
- **Center, transient:** the **combo popup** (`7 HIT!`) — flashes and fades, never persistent.
- **Held weapon:** intentionally **not** a big HUD panel — decay is diegetic on the sprite; **[PROPOSED]** a
  tiny weapon icon by the health only if playtests show the sprite alone isn't readable.

---

## 3. HUD elements

### 3.1 Health — **[PROPOSED]**
- The player takes real hits (no i-frames), so health must read at a glance.
- **[PROPOSED]** a **segmented bar** (chunks, so you feel each hit) top-left. Options in §7 Q1.
- **[LATER]** exact max, regen/heal sources, low-health warning (screen-edge pulse?).

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
- Wallets drop **1¢** each; **10¢ = a dime**, the Monkey Merc cost (`WEAPONS.md` §3.9).
- **[PROPOSED]** small `$0.07` counter top-right; **[PROPOSED]** it **highlights when you hit a full dime**
  (you can now afford a monkey). **[LATER]** does it persist between stages (ties to economy scope).

### 3.5 Monkey Merc status — **[LOCKED data], [PROPOSED] display**
- Up to **3 per level** (death-limited); stacking sets their weapon & lifespan (pistol 20s / shotgun 10s /
  rocket 5s), all at 2 shots/sec (`WEAPONS.md` §3.7).
- **[PROPOSED]** top-right cluster: **monkey icons ×count**, each with a **shrinking timer ring**; a spent
  "3 have died — no more this level" state shown greyed out.

### 3.6 Weapon-specific readouts — **[PROPOSED]**
Most decay is diegetic, but a few weapons have HUD-worthy state:
- **Boomerang Gun:** **10 bullets** (4/pass) — **[PROPOSED]** show remaining bullets only while equipped.
- **Ball & Chain:** **3 uses** + the ~20% slow — **[PROPOSED]** a tiny use-pip cluster.
- Everything else (sword wear, shotgun spine, shotgun/pistol ammo, gatling no-ammo) stays **diegetic on the
  sprite**.

---

## 4. Diegetic vs. HUD split — **[PROPOSED]** summary

| State | Where it's read |
|---|---|
| Weapon durability/ammo (sword, shotgun spine, pistol mag) | **Diegetic** (on the held weapon) |
| Which weapon you hold | **Diegetic** (you can see it in-hand) |
| Health, Special meter, Money, Monkey timers, Combo | **HUD** (top band) |
| Boomerang-gun bullets, Ball&Chain uses | **HUD, only while equipped** |

---

## 5. Other screens — **[PROPOSED] scaffold** (outside the tight loop)

Listed so the asset manifest knows they exist; detailed later.
- **Title / main menu** · **Pause** · **Stage-transition / branch reveal** (shows which path unlocked,
  ties to `GAMEPLAY_LOOP.md` §7) · **Results / grade** (performance → ending) · **Game over** ·
  **Controls/tutorial prompts.** **[LATER]** full designs.

---

## 6. UI asset list → feeds `ASSET_MANIFEST.md`

- **Health bar** (full → empty segments, low-health state).
- **Special meter** (yellow / blue / green tiers, armed pulse/gleam).
- **Combo popup** type (number + `HIT!`, scale states).
- **Money counter** (glyphs, full-dime highlight).
- **Monkey status cluster** (icon, timer ring, spent state).
- **Boomerang-gun bullet pips**, **Ball&Chain use pips**.
- **[LATER]** menu/pause/results/game-over screen art, fonts, button prompts.

---

## 7. Decisions I need (asset-blocking)
1. **Health style (§3.1):** segmented chunk bar / hearts / a single bar / numeric?
2. **HUD art vibe:** clean-minimal (thin, unobtrusive) vs. chunky-arcade (bold pixel frames)? Sets the look
   of every element above.
3. **Held-weapon HUD (§2):** trust the diegetic sprite alone (rec), or add a small weapon icon by health?
4. **Money persistence (§3.4):** does cash carry between stages, or reset each stage?
