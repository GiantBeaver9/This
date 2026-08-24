# this.l — VFX & Juice

> **Scope:** every visual effect, feedback flourish, and camera/time trick surfaced across the design.
> This is both a design reference and a big slice of the **effects asset list**. UI-specific juice (health
> explosions, meter pulse, combo popup) is owned by `UI.md` and only cross-referenced here.
>
> **Legend:** **[LOCKED]** decided · **[PROPOSED]** react to it · **[LATER]** parked.

---

## 1. Principles — **[PROPOSED]**

- **[PROPOSED] Readability beats spectacle.** It's a bullet-hell — **enemy projectiles must always read
  clearly through any VFX.** Effects never obscure a bullet, a telegraph, or the player.
- **[PROPOSED] Juice serves feedback.** Every hit, kill, pickup, and decay gets a distinct effect so the
  player *feels* the game state without reading the HUD.
- **[PROPOSED] Chunky-arcade, consistent with UI** — bold, punchy pixel effects; gore is stylized
  (stick-figure), not realistic.
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
| **Finisher flash** | combo finisher (3rd hit) | stronger spark + small shake | P0 |

---

## 3. Impact & camera juice — **[PROPOSED]**

- **[PROPOSED] Hitstop (freeze-frame):** a few frames of freeze on meaningful hits (finishers, kills) — the
  single cheapest "weight" trick. Scales with hit strength.
- **[PROPOSED] Screen shake:** small on finishers, **big on explosions / ground-slams**. **Must stay within
  a bullet-hell-safe budget** (too much shake ruins dodging — see §1).
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

- **[PROPOSED] Gore & death:** stick-figure death burst + **corpse** that can yield a **part drop**
  (headless body, ejectable spine, dropped weapon), plus **wallet** drops. Gore level = §8 Q1.
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
- **[PROPOSED]** sniper **draw** flourish · **aim line** telegraphing the ricochet path · **tracer** that
  **caroms head-to-head** · a **headshot pop** per kill.
- **[LOCKED] Boss dodge:** the boss plays a **dodge** and the tracer misses (`PLAYER.md` §6).
- **[PROPOSED]** a brief **time-resume whoosh** as it ends.

---

## 7. VFX asset list → feeds `ASSET_MANIFEST.md`

Grouped: **movement** (gust, dusts, streaks, getup), **impact** (sparks, finisher flash, hit-flash,
hitstop is code), **camera** (shake presets, time-slow overlay), **weapon FX** (§4 per weapon), **enemy FX**
(§5 transforms, gore, hollow-head, poofs, telegraphs), **special** (§6 full set), **pickups** (weapon
glints, wallet, dime highlight). Priorities inherit each system's P0/P1.

---

## 8. Decisions I need (tone-setting)
1. **Gore level:** how far does stick-figure violence go — **comedic/cartoony** (poofs, minimal blood),
   **stylized red** (clear but stylized gore), or **over-the-top** (limbs, spray — matches head-ripping)?
2. **Screen-shake & hitstop intensity:** **juicy-heavy** (big impact, risk to bullet-dodging clarity) vs.
   **restrained** (precise, bullet-hell-safe) vs. **scaled** (heavy on finishers/explosions, light on
   normals)?
3. **Bullet readability rule:** confirm **enemy projectiles always render above/through VFX** so they're
   never hidden (recommended lock)?
