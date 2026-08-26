# Bosses — Integration Notes (for the lead)

New, self-contained files under `unity/Assets/Scripts/Bosses/` (namespace `ThisL`,
assembly `ThisL.Runtime`). **Nothing existing was edited.** Built against the real
foundation API (`Actor`, `Projectile`/`ArcProjectile`, `Shadow`, `SpriteLibrary`,
`Music`, `Sfx`, `Vfx`, `CameraShake`, `Steering`, `StageEnemyFactory`,
`PlayerController`). Cites `BOSSES.md` §1–§5 and `TUNING.md` §7.

| File | What it is |
|---|---|
| `BossController.cs` | Abstract `Actor` base. Big phase-segmented HP/objective readout, coroutine telegraph→strike scheduler, LargeTier shadow, ~2× render scale, boss-cue music on spawn. Implements `IStaggerable` (super-armor no-op) and `ISpecialKillable` + the **≤10% execute gate** (`CanBeExecuted`). |
| `BurlyBoss.cs` | **Full.** Area-1 brawler (HP 300): ground-spike / enemy-toss / charge, 3 phases. |
| `GatlingGunGuyBoss.cs` | **Full.** Area-4 suppression (HP 260): row-locked instant-death barrage + chip stream + fodder + inside-3wu melee, 3 phases. |
| `TankBoss.cs` | **Full objective.** MG-sweep dodging fully live; 2-grenade-drop objective via `RegisterGrenadeDrop`. |
| `ColossusBoss.cs` | FIRST-PASS objective. Body-swipe / piece-spit; 6-piece whip objective via `RegisterWhipPull`. |
| `HelicopterBoss.cs` | FIRST-PASS objective. Strafe + arced head-fire + phase-2 rotor-gust; 6-pip objective via `RegisterHeadReflect` / `RegisterGrenadeLob`. |
| `MonkeyBoss.cs` | FIRST-PASS proxy. Dime-toss + enemy-merc-on-miss. |
| `PhilBoss.cs` | FIRST-PASS scripted finale. Draw(invuln)→sharpen(vulnerable, 125 cap) cycle; finisher-only kill. |
| `BigVersionBoss.cs` | FIRST-PASS. Config-driven melee/ranged HP-depletion boss covering **Sandwich Bros, big Arm-Ripper, Boomergunner**. |
| `Bosses.cs` | Static registry — `public static Actor Spawn(string bossId, float x, float z)`. The hook `StageDirector.SpawnBoss` calls. |

## Wiring the director (the ONE edit you make)

`StageDirector.SpawnBoss(string bossId)` currently returns `null` (placeholder). Point
it at the registry — it already computes the arena head `bossX` in `BeginBossWave`:

```csharp
public Actor SpawnBoss(string bossId)
{
    float x = PlayerController.Instance != null
        ? PlayerController.Instance.WorldX + 8f          // spawn ahead in the arena
        : 0f;
    float z = Tuning.ZBandDepth * 0.5f;                  // mid-band
    return Bosses.Spawn(bossId, x, z);
}
```

The director already: plays the boss cue, locks the arena camera, and advances on
`!boss.Alive` (`BossController` sets `Alive=false` on defeat). Unknown ids return `null`
(logged) → the director treats the gate as cleared, exactly as today.

## bossId → boss → stage → executable → art

All 10 placed encounters (`StageDatabase` boss ids). Executable = the ≤10% special
execute applies (the 5 pure HP-depletion bosses only, `BOSSES.md` §1 / `TUNING.md` §7).

| bossId | Boss | Stage (DisplayName) | Class | Executable ≤10%? | Cue present? |
|---|---|---|---|---|---|
| `burly` | Burly Macho Guy | Roseville Galleria | HP-depletion | **YES** | `burly` ✓ |
| `gatlinggunguy` | Gatling Gun Guy | Golden Gate Bridge | HP-depletion | **YES** | `gatlinggunguy` ✓ |
| `sandwich_bros` | Sandwich Bros | Old Hwy 65 | HP-depletion (big-ver) | **YES** | none — stays on stage loop |
| `big_armripper` | big Arm-Ripper | Dixon Boss Rush | HP-depletion (big-ver) | **YES** | `big_armripper` ✓ |
| `boomergunner` | Boomergunner | Marin Redwoods | HP-depletion (big-ver) | **YES** | `boomergunner` ✓ |
| `tank` | Tank | Vallejo Six Flags | objective | no | `tank` ✓ |
| `colossus` | The Colossus | Sacramento Old-Town | objective | no | `colossus` ✓ |
| `helicopter` | Monkey Chopper | Sacramento Airport | objective | no | `helicopter` ✓ |
| `monkey_boss` | Monkey Boss | Farm / Ranch | proxy | no | `monkeyboss` ✓ |
| `phil` | Phil | Salesforce Rooftop | scripted finale | no (finisher-only) | `phil_realized` ✓ |

## HUD / flow hooks (read these off the returned `Actor` cast to `BossController`)

- `Hp` / `MaxHp` — big bar. For objective bosses this is the progress readout
  (Tank 2→0 pips, Colossus 6→0 pieces, Helicopter 6→0 pips, Phil gated 500).
- `PhaseCount` / `CurrentPhase` — segment the bar + fire the name-card / phase flash.
- `DisplayName`, `BossId`, `IsHpDepletion`, `CanBeExecuted` — the **≤10% execute prompt**:
  when `CanBeExecuted` is true and the player fires a special, the boss dies.
- Objective advance APIs (call from the weapon/objective systems when they land):
  `TankBoss.RegisterGrenadeDrop`, `TankBoss.CanMount`;
  `ColossusBoss.RegisterWhipPull`; `HelicopterBoss.RegisterHeadReflect` /
  `RegisterGrenadeLob`; `PhilBoss.CanBeFinished`.

## Execute mechanic — how it plugs into the existing specials

The 4 character specials (`Characters.cs`) all resolve a boss as either
`ISpecialKillable.KillBySpecial` (Sniper, Vaporize) **or** a raw `TakeDamage(9999f)`
(Giant Shotgun, Werewolf). `BossController` intercepts **both**: any hit ≥ 1000 is
treated as an execute attempt, and `KillBySpecial` routes to the same gate. Above 10%
(or on any objective/proxy boss, or Phil) it is **negated** — no damage, no drop, the
boss "dodges". At ≤10% on a HP-depletion boss it **kills**. No existing file changed.

- **Flow note (not a code change):** `SpecialMeter.Fire()` drains the meter even when a
  boss negates the special. `BOSSES.md` §1 says the meter "carries over / is never
  wasted", so the intended UX is the HUD showing the `CanBeExecuted` prompt and the
  player choosing to fire *then*. The negate path here matches "the sniper visibly
  dodges"; gating the meter *spend* on the prompt is a HUD/PlayerController concern,
  left to that agent.

## Art gap (BOSSES.md §6 — all 7 bespoke bosses un-arted)

There is **no `assets/sprites/bosses/` art**. Every boss reuses the shipped
`sprites/enemies/enemy_regular` atlas, **tinted per boss** and rendered at ~2× boss
scale (`SizeScale`), via `SpriteLibrary.Load` (which already falls back to a magenta
placeholder if even that is missing). No filenames were invented. When bespoke atlases
land (`BOSSES.md` §6: idle/move/each attack+telegraph/phase/hurt/death/sniper-dodge),
point each boss's `InitBoss(...)` `SpriteLibrary.Load` call at the new dir/actor and drop
the tint. Boss **audio cues already exist** under `assets/audio/music/boss_cues/` for 9
of the 10 (Sandwich Bros has none by design and stays on the stage loop).

## Mechanic gaps deferred to their owning systems (all marked `// FIRST-PASS`)

- **Grenade / Whip / Bat weapons** don't exist yet (`WEAPONS.md`). Tank, Colossus, and
  Helicopter therefore expose public objective-advance APIs (above) for those systems to
  call; the **dodging gameplay is fully live**, only the win-input is stubbed.
- **Monkey Boss** merc subsystem (`WEAPONS.md` §3.7) is absent — FIRST-PASS lets any
  player-side damage deplete his HP (proxy for merc fire) and spawns enemy mercs on a
  missed dime; the "only-your-mercs-damage-him" rule and the dime-catch summon are gaps.
- **Phil** — the scripted pencil-laser **finisher input**, the rooftop **sway/fall-death**,
  and the exact **lead-economy draw-selection** (per-summon cost, 8-add ceiling,
  no-repeat miniboss, 2-reprise / 2-Heavy caps) are simplified; FIRST-PASS downs him when
  gated HP reaches ≤0 in a sharpen window and approximates the cumulative summon roster.
- **Gatling barrage cover** — parked-car hard cover isn't modelled; stepping off the
  row-locked barrage lane is the available dodge (the barrage deals lethal damage on the
  lit row, matching "instant death in the open").
- **Arena adds** use `StageEnemyFactory` directly; the strict 2-alive / 3s-respawn cadence
  (`BOSSES.md` §1) is approximated with a simple `CountAdds()` cap.
