# Stages — Integration Notes (for the lead)

New, self-contained files under `unity/Assets/Scripts/Stages/` (namespace `ThisL`,
assembly `ThisL.Runtime`). Nothing existing was edited. Built against the real
foundation API (EnemySpawner / CameraRig / Actor / Backdrop / Music).

| File | What it is |
|---|---|
| `StageData.cs` | Plain-data model: `StageData`, `Wave`, `SpawnEntry`, enums (`EnemyArchetype`, `SpawnSide`, `WaveKind`), and `EndlessDescriptor`. No Unity types. |
| `StageDatabase.cs` | The authored **linear campaign** — 13 stages from `STAGES.md` §4.1 + `ENCOUNTERS.md` wave tables + `AREAS.md` themes/music. Also `Endless()`. |
| `StageEnemyFactory.cs` | Archetype → live actor, spawned the EnemySpawner way. Real defs for Regular/Swarmer/Zombie/Gunner/Pod; FIRST-PASS placeholders for the rest of the roster. |
| `StageDirector.cs` | `MonoBehaviour` that runs a stage. Drop-in replacement for `EnemySpawner`. |

## Starting a stage

`StageDirector` replaces `EnemySpawner`. In `GameBootstrap` (or wherever you build
the Systems object), instead of:

```csharp
sys.AddComponent<EnemySpawner>();
```

do:

```csharp
var director = sys.AddComponent<StageDirector>();
director.StartStage(0);   // 0 = Stage 1 (Lincoln High). Index is 0-based.
```

Or the truly drop-in form (auto-runs on Start, like EnemySpawner did):

```csharp
var director = sys.AddComponent<StageDirector>();
director.AutoStartStage = 0;   // set before Start() fires
```

Or the one-liner static helper (finds/creates the director):

```csharp
StageDirector.Begin(0);
```

`StartStage(index)` does all of: set stage music (`Music.PlayStage`) + ambient bed
(`Music.PlayAmbient`), request the backdrop theme, lock the camera to the lane head,
then walk the wave list.

## How stage-complete chains to the next stage

`StageDirector` fires `public event System.Action OnStageComplete` when the final
wave / boss gate resolves. Chain the campaign like this:

```csharp
void HookCampaign(StageDirector dir, int index)
{
    dir.OnStageComplete += () =>
    {
        int next = index + 1;
        if (next < StageDatabase.StageCount)
        {
            // (optional) play the area transition card / results grade here
            dir.OnStageComplete = null;   // clear this stage's handler
            HookCampaign(dir, next);
            dir.StartStage(next);
        }
        else
        {
            // Campaign finished (Phil down) — go to the win/credits screen.
        }
    };
}

// kick off:
var dir = sys.AddComponent<StageDirector>();
HookCampaign(dir, 0);
dir.StartStage(0);
```

The same `StageDirector` instance is reused across stages; `StartStage` re-anchors
the lane to the player's current X, so stages flow continuously.

## The boss hook (for the bosses agent)

Boss waves call `public Actor SpawnBoss(string bossId)` on `StageDirector`. It is a
**placeholder that returns `null`** today — the director treats a null boss as an
instantly-cleared gate so the campaign chains end-to-end without bosses.

The bosses agent fills the body: switch on `bossId`, build the boss GameObject +
controller, place it in the arena, and **return its root `Actor`**. The director
then waits for `!boss.Alive` before advancing. Boss ids used by the database:

`sandwich_bros`, `burly`, `colossus`, `helicopter`, `monkey_boss`, `big_armripper`,
`tank`, `boomergunner`, `gatlinggunguy`, `phil` (bossless stages: 1, 6, 12).

Boss music cue stems are pre-wired in `StageData.BossMusicClip` (played on the boss
wave) and match `assets/audio/music/boss_cues/**`. Sandwich Bros has no cue and
stays on the stage loop.

## What the director actually does per wave (ENCOUNTERS.md §0)

- **Vignette** waves: 3.5 s pause (STAGES.md §1c), then advance. Non-combat.
- **Checkpoint** waves: chime + log, then advance. (Visible flag/beacon marker is
  level dressing — add per `AREAS.md`.)
- **Spawn** waves: hard-lock the camera at this wave's gate X (`CameraRig.MaxX`),
  drip-spawn the batch (respecting the 8-pursuer cap `Tuning.MaxPursuers`), then
  gate on a **cleared field** (`Actor.All`, `Team.Enemy`) — matches §0's rule that
  a named-target gate still clears the *whole* wave. On clear, extend `MaxX` to the
  next gate and continue.
- **Filler** markers: expanded at `StartStage` into concrete seeded sub-waves —
  count = midpoint of the listed range rounded up, size ramps 4→6, weighted 60%
  toward the stage's newest type, deterministic seed = stage id (§0).
- **Boss** waves: play the boss cue, lock the arena, call `SpawnBoss`.

Camera gates are distributed evenly along `StageData.LaneLengthWu` (~140 wu for
combat stages; 30 wu finale arena). The director only ever extends `MaxX` forward
and pins `MinX` to the lane head (never scrolls backward), per §0.

## Endless Mode (STAGES.md §7b)

`director.StartEndless()` runs the survival sandbox: full roster, refills the field
whenever ≤ 2 enemies remain, wave size ramps up every 30 s (clamped to the
8-pursuer cap). It uses the `EndlessDescriptor` from `StageDatabase.Endless()`.

## FIRST-PASS items to revisit

- **Enemy archetypes without a real `EnemyDef`** (everything except
  Regular/Swarmer/Zombie/Gunner/Pod) spawn as tuned clones of the melee-Regular or
  ranged-Gunner behaviour, with distinct ids and `enemy_regular` art. Marked
  `FIRST-PASS` in `StageEnemyFactory.Resolve`. Add real defs and route them there.
- **Dixon boss-rush minibosses (Stage 8)** are authored as normal spawn waves of
  their base archetypes (marked `// FIRST-PASS`); real "big-version" scaling is a
  bosses/enemies-agent job.
- **Backdrop theme swapping**: `Backdrop.cs` ships a hardcoded Area-1 suburb theme,
  so `StageData.BackdropTheme` (e.g. `area2_airport`) is carried as data and logged;
  wire it once `Backdrop` accepts a settable theme. See `ApplyBackdrop` in the
  director.
- **Traffic / trolley / roller-coaster hazards and terrain funnels** (`AREAS.md`,
  `TUNING.md` §6.2) are not spawned by the director — they are level dressing to add
  per theme.
