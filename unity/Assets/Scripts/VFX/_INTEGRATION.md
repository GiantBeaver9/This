# VFX + Screen-Shake — integration guide (for the lead)

New, self-contained files under `unity/Assets/Scripts/VFX/`:

- `Vfx.cs` — static facade (`Vfx.HitSpark`, `FinisherFlash`, `DeathBurst`, `DashDust`,
  `JumpPuff`, `LandPuff`, `MuzzleFlash`, `Gust`). Lazy-loads
  `assets/sprites/vfx/vfx/vfx_atlas.png` + `vfx.json` at runtime, point-filtered,
  depth-scaled and Z-sorted; each effect auto-destroys when its clip finishes
  (`VfxOneShot`, with a 1.5 s hard cap). No scene wiring needed — just call it.
- `Shadow.cs` — `Shadow.Attach(Actor owner, int sizeTier)` blob shadow; tiers
  `Shadow.SmallTier` / `MediumTier` / `LargeTier` map to
  `shadow_small.png` / `shadow_regular.png` / `shadow_boss.png`. Follows the owner's
  logical (WorldX, Z), sorts at ~ -900 (above the -1000/-999 ground, below actors),
  and destroys itself when the owner is gone.
- `CameraShake.cs` — `CameraShake.Add(px, seconds)` or `Add(CameraShake.Light|Medium|Heavy)`.
  Self-installs on `Camera.main`. Its `.meta` sets `executionOrder: 10000` so it runs
  after `CameraRig.LateUpdate`. Honor the Options toggle via `CameraShake.ReduceShake = true`.

All effects/shadows degrade to generated textures if the PNGs are missing — nothing crashes.

> NOTE: every edit below is in an **existing** foundation file the lead owns. I did not
> touch them. Insert exactly as described. All positions use the actor's logical
> `WorldX` / `Z` (not the transform), which is what `Vfx.*` and `Shadow.Attach` expect.

---

## 1. Melee hits, finisher, gust, knockdown shake
File: `Actors/PlayerController.cs`, method `ResolveSwing()`.

After the early-out `if (hits.Count == 0) return;`, add per-hit sparks + scaled shake:

```csharp
bool isFinisher = _combo == 3;   // 0=P1 1=P2 2=Sweep 3=Finisher
foreach (var a in hits)
{
    if (isFinisher) Vfx.FinisherFlash(a.WorldX, a.Z);
    else            Vfx.HitSpark(a.WorldX, a.Z);
}
if (isFinisher)      CameraShake.Add(CameraShake.Heavy);   // §8: finisher = 10px/0.20s
else if (_combo == 2) CameraShake.Add(CameraShake.Medium); // sweep knockdown = 5px/0.15s
else if (!isFist)    CameraShake.Add(CameraShake.Light);   // weapon hit = 2px/0.10s (fists: none)
```
(`isFist` is already computed at the top of `ResolveSwing`.)

Air-punch **gust** (VFX.md §2, "every punch/attack"): in `StartCombo(int index)`, after
`Anim.Play(AttackClip(_combo), ...)`, add:
```csharp
Vfx.Gust(WorldX + Facing * (CurrentWeapon.Reach), Z, Facing);
```

## 2. Dash dust + dash shake
File: `Actors/PlayerController.cs`, method `StartDash(Vector2 dir)` — at the end (after
`_phase = Phase.None; _combo = -1;`):
```csharp
Vfx.DashDust(WorldX, Z);
CameraShake.Add(CameraShake.Light);
```

## 3. Jump / land puffs
File: `Actors/PlayerController.cs`.
- In `StartJump()`, after `_airborne = true;`:  `Vfx.JumpPuff(WorldX, Z);`
- In `TickJump(float dt)`, inside the landing branch
  `if (_jumpTimer >= Tuning.JumpDuration) { _airborne = false; _jumpOffset = 0f; }`,
  add `Vfx.LandPuff(WorldX, Z);` before the closing brace.

## 4. Enemy death burst (red pixels — VFX.md §5)
- File: `Actors/EnemyController.cs`, `OnDeath(Actor source)` — first line inside:
  `Vfx.DeathBurst(WorldX, Z);`
- File: `Actors/RangedEnemyController.cs`, `OnDeath(Actor source)` — first line inside:
  `Vfx.DeathBurst(WorldX, Z);`
- File: `Actors/Pod.cs`, at its death/despawn — `Vfx.DeathBurst(WorldX, Z);` (optional).

## 5. Muzzle flash on enemy fire
File: `Actors/RangedEnemyController.cs`, in `Update()` right after the existing
`Projectile.Spawn(Team.Enemy, WorldX + Facing * 0.6f, Z, Facing, ...)` call:
```csharp
Vfx.MuzzleFlash(WorldX + Facing * 0.6f, Z, Facing);
```
(Do the same at any future player-gun fire site with the player's WorldX/Z/Facing.)

## 6. Sniper special shake (the visual peak — §3/§6)
File: `Actors/PlayerController.cs`, `FireSpecial()`, after `SpecialFired?.Invoke(tier);`:
```csharp
CameraShake.Add(CameraShake.Heavy);
```
(Per-kill headshot pops can reuse `Vfx.FinisherFlash` / `Vfx.DeathBurst` on each killed
enemy inside `Combat.SniperRicochet` when that FX pass is built.)

## 7. Blob shadows — one per actor (VFX.md §2, "1 blob shadow per actor")
Attach once at spawn (the shadow self-destructs with its owner):
- Player — File: `Core/GameBootstrap.cs`, after `player.Init();`:
  `Shadow.Attach(player, Shadow.MediumTier);`
- Regular / gunner enemies — File: `World/EnemySpawner.cs`, in `SpawnRegular` and
  `SpawnGunner`, after `e.Init(...)`:  `Shadow.Attach(e, Shadow.SmallTier);`
- Pod — File: `World/EnemySpawner.cs`, in `PlacePod`, after `pod.Init(...)`:
  `Shadow.Attach(pod, Shadow.LargeTier);`
- Bosses (when added): `Shadow.Attach(boss, Shadow.LargeTier);`

## 8. Options "reduce screen-shake" toggle (UI.md §5)
Wherever the Options toggle is read (e.g. `UI/Hud.cs` or the settings screen), set:
`CameraShake.ReduceShake = optionEnabled;`  (halves all three presets; never fully disables.)

---

### Notes
- Hitstop (3f/5f, TUNING §2.6) and the sniper time-slow overlay are **not** in this system
  (time-scale / post-processing is code the lead owns); shake uses `unscaledDeltaTime` so it
  still animates during hitstop/time-slow.
- Projectiles must stay on top of VFX (VFX.md §1). VFX sort at `SortingOrder(z)+1`
  (front of actors); keep `Projectile` on a higher order if any overlap is ever visible.
