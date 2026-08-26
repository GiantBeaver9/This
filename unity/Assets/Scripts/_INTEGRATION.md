# Enemy roster expansion — integration notes

New **additive** files (no existing file touched). Namespace `ThisL`, assembly
`ThisL.Runtime`. All numbers cite `TUNING.md` §4 / `ENEMIES.md` §2. Every new
archetype reuses the `enemy_regular` atlas (the only stick-body art on disk) —
bespoke controllers tint `Actor.Sr` for readability; the data-only ones you can
tint yourself after `Init` (see the art-gap table).

## Files added
| File | What it is |
|---|---|
| `Actors/EnemyRoster.cs` | Static `EnemyDef` factories (data only) for 8 archetypes. |
| `Combat/ArcProjectile.cs` | Lobbed/arcing projectile with a Z-aware landing splash. |
| `Actors/AntiAircraftController.cs` | Rock-lobber AI (arc throw at the player's spot). |
| `Actors/SnapperController.cs` | T2 sword-zoner; calls-in/snaps a T1 to re-arm. |
| `Actors/NinjaController.cs` | Shuriken volleys + smoke teleport. |

## How to spawn each archetype

The existing controllers use the spawner pattern (`EnemySpawner.NewEnemyGo` →
`AddComponent<SpriteRenderer>()` + `AddComponent<SpriteAnimator>()` +
`AddComponent<Controller>()` + `controller.Init(def)`). All the new ones follow
it exactly. A generic helper:

```csharp
static T SpawnEnemy<T>(EnemyDef def, float x, float z) where T : Actor
{
    var go = new GameObject(def.Id);
    go.AddComponent<SpriteRenderer>();
    go.AddComponent<SpriteAnimator>();
    var c = go.AddComponent<T>();
    c.WorldX = x;
    c.Z = Mathf.Clamp(z, 0f, Tuning.ZBandDepth);
    // c.Init(def) below — Init is declared on each concrete controller, so call it typed:
    return c;
}
```

Because `Init(EnemyDef)` is declared on each concrete controller (not a shared
virtual), call it on the concrete type. Mapping:

| Archetype | Factory | Controller | Notes |
|---|---|---|---|
| **Snapper** | `EnemyRoster.Snapper()` | `SnapperController` | Long-reach sword; re-arm loop built in. |
| **Heavy** | `EnemyRoster.Heavy()` | `EnemyController` | Weight `H` in the def; cap 2 at once in your wave table. |
| **Pickpocket** | `EnemyRoster.Pickpocket()` | `EnemyController` | Fast light poke (steal/flee is a gap). |
| **Monkey (economy)** | `EnemyRoster.EconomyMonkey()` | `EnemyController` | Weak flail (flee/Merc-drop is a gap). |
| **Arm-Ripper** | `EnemyRoster.ArmRipper()` | `RangedEnemyController` | 2 shots/s straight, holds ≤4 wu. |
| **Head-Thrower** | `EnemyRoster.HeadThrower()` | `RangedEnemyController` *or* `AntiAircraftController` | Straight lob stand-in; use the AA controller for a real arc. |
| **Anti-Aircraft** | `EnemyRoster.AntiAircraft()` | `AntiAircraftController` | True arc via `ArcProjectile`; `RangedEnemyController` is a straight fallback. |
| **Ninja** | `EnemyRoster.Ninja()` | `NinjaController` | 2 shuriken/volley + teleport; spawns combat-ready. |

Example wave lines:

```csharp
SpawnEnemy<SnapperController>(...).Init(EnemyRoster.Snapper());
var heavy = SpawnEnemy<EnemyController>(...); heavy.Init(EnemyRoster.Heavy());
SpawnEnemy<AntiAircraftController>(...).Init(EnemyRoster.AntiAircraft());
SpawnEnemy<RangedEnemyController>(...).Init(EnemyRoster.ArmRipper());
SpawnEnemy<NinjaController>(...).Init(EnemyRoster.Ninja());
```

`EnemySpawner` (existing, untouched) currently only fields regulars + a gunner +
a Pod. To roster these in, add cases to your encounter/wave director — none of
this requires editing `EnemySpawner`, `EnemyController`, `RangedEnemyController`,
or `PlayerController`.

### Loot & specials (already wired)
- All controllers drop via `LootTable.Roll(Def.Loot)` on death, and drop nothing
  on a sniper-special kill (`ISpecialKillable.KillBySpecial`). Loot bands per
  `ENEMIES.md` §6: Snapper/Arm-Ripper/Head-Thrower = T2, Heavy/Ninja = T3,
  Anti-Aircraft = T1, Pickpocket/Monkey = None (economy).
- The Snapper additionally drops a **guaranteed full Sword** on death (ENEMIES §6
  "the enemy IS the weapon"), plus its T2 roll.
- All implement `IStaggerable` (sweep / boomerang / dash-attack) and
  `ISpecialKillable` (sniper ricochet), so existing combat hooks Just Work.

## Behavior faithfully modelled vs. gaps

**Modelled:** Anti-Aircraft arc lob at the player's spot with 0.5 s telegraph;
Snapper sword decay after 8 hits → keep-away → call-in-a-T1-every-4s (max 2) →
snap an adjacent T1 to re-arm; Ninja 2-shuriken volley on 3 s + 22.5 close
slash + smoke-teleport every 3 s (0.3 s tell); all standoff / separation /
loot / stagger / special-kill behaviour.

**Deferred (bespoke systems, not in this pack):**
- Anti-Aircraft **boomerang-bait** counterplay (20% intercept of thrown toys).
- Head-Thrower **fire→2 s→BOOM walking-bomb** + head-regrow + true grenade arc
  (route it through `AntiAircraftController` for the arc in the meantime).
- Arm-Ripper **reload after 6 shots** + disarmed-T1-headbutt donor state.
- Heavy **H-weight super-armor** (no flinch on normals) and **floor-the-dash** —
  these are player-side reaction rules; the def already carries `Weight = H`.
- Pickpocket **steal-all-coins / kill-for-2×** and Monkey **flee / Monkey-Merc
  drop** — need the wallet/economy system.

## Art gaps — archetypes still needing bespoke atlases

All eight reuse `sprites/enemies/enemy_regular` today. Bespoke atlases wanted:

| Archetype | Current | Bespoke art needed | Interim tint (applied by controller / suggested) |
|---|---|---|---|
| Anti-Aircraft | `enemy_regular` | rock-throw overhand set + rock/telegraph VFX | earthy brown `(0.72, 0.56, 0.34)` (applied) |
| Snapper | `enemy_regular` | snap-a-T1 + sword-swing signature set | steel blue `(0.60, 0.72, 0.85)` (applied) |
| Ninja | `enemy_regular` | shuriken-throw + smoke-teleport puff | dark slate `(0.35, 0.35, 0.45)` (applied) |
| Heavy | `enemy_regular` | **thicker/bulked silhouette** + extended punch + gust | suggest `Sr.color = (0.85,0.7,0.55)` after `Init` |
| Arm-Ripper | `enemy_regular` | dual-pistol arms + muzzle set | suggest a muted tint after `Init` |
| Head-Thrower | `enemy_regular` | self-decapitation throw + headless/regrow + blink-bomb | suggest a tint after `Init` |
| Pickpocket | `enemy_regular` | **hooded grey/green** smaller figure + coin-pouch tell | suggest `Sr.color = (0.5,0.55,0.45)` after `Init` |
| Monkey (economy) | `enemy_regular` | monkey figure + flail | suggest a tint after `Init` |

Shared VFX still placeholdered: **smoke-teleport puff** and **rock/dust impact**
currently reuse `Vfx.DeathBurst`; add dedicated `vfx` atlas clips when available.
`ArcProjectile` and shuriken use generated blob/dot sprites (same as
`Projectile`).
