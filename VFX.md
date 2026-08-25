# this.l — VFX & Juice

> **Scope:** every visual effect, feedback flourish, and camera/time trick surfaced across the design.
> This is both a design reference and a big slice of the **effects asset list**. UI-specific juice (health
> explosions, meter pulse, combo popup) is owned by `UI.md` and only cross-referenced here.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[LATER]** parked.
>
> **[AUTHORITY BANNER]** VFX timing values (hitstop frames, shake presets, time-slow) are LOCKED in §8 and
> `TUNING.md` §2.6. Any `[PROPOSED]`/`[LATER]` on a *visual treatment* below is a **look note**, not an open
> mechanical question. No marker below blocks the build.

---

## 1. Principles — **[PROPOSED]**

- **[LOCKED] Readability beats spectacle.** Threats stay clear — **enemy projectiles ALWAYS render above/
  through any VFX and are never hidden** by effects. Effects never obscure a bullet, a telegraph, or the player.
- **[LOCKED] Comedic, transient gore.** **Red pixels fly** on hits and deaths, but **nothing stays** — no
  pooling, no decals; the red **disappears at the end of its animation.** Chunky-arcade, stylized
  stick-figure violence, never realistic viscera.
- **[PROPOSED] Juice serves feedback.** Every hit, kill, pickup, and decay gets a distinct effect so the
  player *feels* the game state without reading the HUD.
- **[PROPOSED] Reserve the biggest effects for the biggest moments** — the sniper time-slow is the visual
  peak; nothing routine should out-shout it.

---

## 2. Player & movement VFX

| Effect | Trigger | Notes | Priority |
|---|---|---|---|
| **Air-punch gust** | every punch/attack | the reach-extender wind off the fist; per direction | P0 |
| **Dash dust** | grounded dash | kick-up at start + streak | P0 |
| **Jump puff / landing puff** | jump / land | small poofs | P0 |
| **Air-dash streak** | air-dash | distinct from ground dash | P1 |
| **Fall-over + getup dust** | dash into a heavy/boss | you bounce & floor (`PLAYER.md`) | P1 |
| **Hit spark** | melee connects | impact flash at contact point | P0 |
| **Finisher flash** | combo finisher (4th hit) | stronger spark + small shake | P0 |

---

## 3. Impact & camera juice — **[LOCKED intensity]**

- **[LOCKED] Scaled impact:** heavy shake/hitstop on **finishers, explosions, ground-slams**; light-to-none
  on normal hits — impactful where it counts, and always **within the bullet-hell-safe budget** (never
  enough to spoil dodging, per §1).
- **[PROPOSED] Hitstop (freeze-frame):** a few frames of freeze on meaningful hits (finishers, kills) — the
  single cheapest "weight" trick. Scales with hit strength.
- **[PROPOSED] Screen shake:** small on finishers, **big on explosions / ground-slams**, per the scaled rule.
- **[PROPOSED] Knockback + hit-flash on enemies:** white flash + stagger on every hit.
- **[PROPOSED] Kill pop:** stick-figure death burst (see §5 gore).

---

## 4. Weapon VFX → per `WEAPONS.md`

| Weapon | Effects |
|---|---|
| **Sword** | swing trail · **wear/chip states** · break shatter |
| **Shotgun** | muzzle flash · **spine-eject bit** · cock |
| **Boomerang** | spinning in-flight · return arc · **2s stun** VFX on struck enemy · ground pickup |
| **Pistol / Revolver** | muzzle flash · **straight tracer** · headshot pop · **pierce** hits (halving) · **cigarette-flick** flourish · casings |
| **Gatling** | **~0.5s sustained muzzle barrage** · spin · auto-kill headshot pop |
| **Staff — Ice** | freeze crystals / frost aura on enemies |
| **Staff — Fire** | flames / burn DoT · **grenade-enemy blink → BOOM** |
| **Staff — Lightning** | arc bolt · stun sparks · slow shimmer |
| **Grenade** | **ground bounce-marker** · lob arc vs. **fastball path-plow** trail · **big vs. small explosion** · self-hit |
| **Boomerang Gun** | fixed-orbit trail · auto-fire muzzle · **shot-down break** |
| **Whip** | crack · arc/line/pull variants · **head-rip → head-grenade** · auto-dash streak |
| **Ball & Chain** | launch trail · **heavy impact** + shake · secret-combo finisher FX (**[LATER]**) |
| **Monkey Merc** | **summon poof** · pistol/shotgun/**rocket** fire · **expire/vanish** |
| **Sniper Special** | **see §6** |

---

## 5. Enemy VFX → per `ENEMIES.md`

- **[LOCKED] Gore & death (comedic, transient):** death = a **burst of flying red pixels** that **clears
  completely** when the animation ends (no lingering blood). The corpse can still yield a **part drop**
  (headless body, ejectable spine, dropped weapon) and **coins** — the persistent things are
  drops/pickups, never blood.
- **Transformations (the cannibalize thread):**
  - **Snapper:** grab → **snap a T1 into a sword**.
  - **Arm-Ripper:** **rip arms → dual pistols**; leaves a **disarmed headbutt** body.
  - **Gatling Gunner:** **~2s contort** fodder into a gatling (telegraph).
  - **Ninja:** **smoke-bomb teleport** puff · **limb-strip → shuriken** · star throw.
- **Head-Thrower:** self-decapitate → **thrown head** · **fire blink → explosion** (walking bomb).
- **Monkey Tamer:** **whistle** cue · monkey **spawn poof** · **deactivate poof** on his death.
- **Zombie:** **headshot → hollow-head** effect (filled → see-through) · grab (mash-to-escape) · **Pod**
  spawn burst · **~10s timeout** dissolve.
- **[PROPOSED] Hurt flash / stagger** on every enemy hit; **weight-based stagger vs. floor-the-player** read.

---

## 6. The Special (sniper time-slow) — **[LOCKED behavior], FX [PROPOSED]** — the visual peak

- **[LOCKED] Time-slow** overlay when the special fires (world slows, not player speeds — `PLAYER.md` §6).
- **[PROPOSED]** desaturate/tint the screen, add scanline/vignette, slow-mo motion blur on bullets.
- **[LOCKED] sniper special FX set:** **draw flourish** (weapon-raise) · **tracer that caroms head-to-head**
  (follows the auto-chain order) · a **headshot pop** per kill. **The "aim line" is CUT — superseded by the
  LOCKED red-dot chain** (`UI.md` §3.5b): the red dots already show the full ricochet order, so a separate aim
  line is redundant. Do not author an aim-line asset.
- **[LOCKED] Boss dodge:** the boss plays a **dodge** and the tracer misses (`PLAYER.md` §6).
- **[PROPOSED]** a brief **time-resume whoosh** as it ends.

---

## 7. VFX asset list → feeds `ASSET_MANIFEST.md`

Grouped: **movement** (gust, dusts, streaks, getup), **impact** (sparks, finisher flash, hit-flash,
hitstop is code), **camera** (shake presets, time-slow overlay), **weapon FX** (§4 per weapon), **enemy FX**
(§5 transforms, gore, hollow-head, poofs, telegraphs), **special** (§6 full set), **pickups** (weapon
glints, coin, dime highlight). Priorities inherit each system's P0/P1.

---

## 8. Decisions — status

**Resolved (now [LOCKED]):** **comedic transient gore** (red pixels fly, nothing persists); **scaled**
shake/hitstop (heavy on finishers/explosions, light on normals); **bullet-readability locked** (projectiles
always render above/through VFX).

**[LOCKED] Hitstop frame counts** (`TUNING.md` §2.6): **3f** on finishers · **5f** on any kill · **0f** on
normal hits.

**[LOCKED] Screen-shake presets** (amplitude in px at the 640×360 internal res, `ASSET_MANIFEST.md` §0; decay
over the listed duration): **light = 2 px / 0.10 s** (weapon hits, dashes) · **medium = 5 px / 0.15 s**
(knockdowns, big weapon impacts) · **heavy = 10 px / 0.20 s** (finishers, explosions, ground-slams, boss
phase-changes, Ball & Chain launch). The **Options "reduce screen-shake" toggle** (`UI.md` §5) **halves all
three amplitudes** (light 1 / medium 2.5 / heavy 5) — it never fully disables (some feedback is retained).

**[LOCKED] Time-slow overlay** (sniper special, `TUNING.md` §3.1): a **desaturated blue tint + slight radial
blur at the screen edges** for the 2.5 s at 0.2× speed; the targeting **red-dot** (`UI.md`) and tracer render
at full saturation on top. **Secret-combo finisher FX** are pinned in `COMBOS.md` §5.
