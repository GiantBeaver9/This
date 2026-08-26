# Audio integration — call sites for the lead

New audio system, code-first and self-initializing. Nothing needs scene wiring; the
first `Sfx.Play(...)` / `Music.PlayStage(...)` call builds its own hidden persistent
GameObject and indexes the WAV bank off disk (mirrors `SpriteLibrary`).

- `Sfx.Play(string name, float volume = 1f)` — one-shot, pitch-jittered ±2 semitones.
- `Sfx.PlayAt(string name, float worldX, float volume = 1f)` — same, panned by on-screen X.
- `Music.PlayStage(name)` / `PlayBoss(name)` / `PlayTitle()` — looping main track (0.5 s crossfade on switch).
- `Music.PlayAmbient(name)` — low-volume looping bed under the music.
- `Music.Stinger(name)` — one-shot over the running loop.
- `Music.Stop()` — fade out everything.

Names are the WAV filename **stem** (no folder, no extension). All names below were
verified against the actual files under `assets/audio/sfx/**` and `assets/audio/music/**`.
A missing/unknown name logs one warning and no-ops — it never throws into gameplay.

---

## PlayerController.cs

- **ResolveSwing()** — on a landed hit (`hits.Count > 0`):
  - always: `Sfx.Play("hit_spark")`
  - if `_combo == 3` (finisher): also `Sfx.Play("finisher_heavy")` and `Sfx.Play("finisher_crunch")` (the hitstop cue)
  - if `_combo == 2` (sweep, per-enemy knockdown): `Sfx.Play("knockdown_thud")`
  - weapon-specific swing cue can key off `CurrentWeapon` (see weapon table below); fists need none beyond `hit_spark`.
  - when `CurrentWeapon.Spend()` breaks the weapon: `Sfx.Play("weapon_break_puff")` (sword variant `sword_break`).
- **StartCombo(int index)** — on the *swing whoosh* at attack start:
  - `_combo == 0 || 1`: `Sfx.Play(_combo == 0 ? "punch_1" : "punch_2")`
  - `_combo == 2`: `Sfx.Play("sweep")`
  - `_combo == 3`: (finisher whoosh) — the impact `finisher_heavy` fires in ResolveSwing; optional windup swing here.
  - with a melee weapon equipped, swap the punch whoosh for the weapon's swing (e.g. `sword_swing`).
- **StartDash(Vector2)** — `Sfx.Play("dash_whoosh")`.
- **StartJump()** — `Sfx.Play("jump")`. On landing (`TickJump` sets `_airborne = false`): `Sfx.Play("land")`.
- **Airborne attack** (attack while `_airborne`, if added): `Sfx.Play("air_hit")` on hit.
- **TakeDamage(...)** — hurt: `Sfx.Play("hurt_grunt")`; on death (`dead == true`): `Sfx.Play("death")`.
- **FireSpecial()** — the sniper ricochet special (`tier > 0`):
  - `Sfx.Play("sniper_scope_in")` then `Sfx.Play("sniper_shot")`
  - time-slow enter cue: `Sfx.Play("sniper_timeslow_enter")`
  - on time resume (when the slow ends): `Sfx.Play("time_resume_whoosh")`
- **Special becomes armed** — detect `Meter.FullTier` crossing 0 → ≥1 (cache the previous tier around the
  `Meter.RegisterHit(...)` / `Meter.Award(...)` calls in ResolveSwing): `Sfx.Play("armed_ready_chime")`.
  Optional per-hit meter feedback: `Sfx.Play("meter_tick_up", 0.5f)` after `RegisterHit`.

## SpecialMeter.cs (alternative home for meter cues)

- Simplest place for the armed chime is where `FullTier` first reaches ≥1. If you'd rather keep SFX out of
  the plain data class, do the 0 → ≥1 edge check in PlayerController (above). Either works; pick one to avoid
  a double chime.

## EnemyController.cs

- **Pursue() → Windup entry** (`_state = State.Windup`): windup telegraph — `Sfx.PlayAt("enemy_stagger"...)`
  is *not* the tell; use the per-enemy signature cue from `Def` if present (e.g. `sniper_scope_in`,
  `groundsmash_overhead`, `zombie_moan`). For a generic melee enemy a light windup is optional.
- **Windup commit** (`player.TakeDamage(Def.Damage, this)`): the hit itself is voiced by the player's
  `hit_spark`/`hurt_grunt`; add `Sfx.PlayAt("block_soak", WorldX)` only if a block path exists.
- **ApplyStagger(seconds)** — `Sfx.PlayAt("enemy_stagger", WorldX)` (sweep already plays `knockdown_thud`
  on the player side; keep one or the other to taste).
- **OnDeath(source)** — enemy death thud: `Sfx.PlayAt("knockdown_thud", WorldX)` (or a per-enemy death cue).
- Per-enemy signature SFX (fire from the enemy's own AI when those enemies land): `zombie_moan`,
  `zombie_grab`, `swarmer_chitter`, `head_throw`, `fire_blink_boom`, `snapper_snap_to_sword`, `arm_rip`,
  `gatling_contort`, `ninja_smoke_teleport`, `ninja_shuriken_throw`, `sniper_scope_in`, `sniper_shot`,
  `groundsmash_overhead`, `groundsmash_shockwave`, `tamer_whistle`, `monkey_merc_chatter`, `aa_rock_throw`,
  `boomergunner_gun_throw`, `pod_spawn_burst`, `tank_mg_stream`.

## Pickup.cs

- **Update()** on auto-grab (`player.Equip(Kind)` before `Destroy`):
  - weapon pickup: `Sfx.Play("weapon_pickup")`
  - if you add a heal/coin pickup kind: `Sfx.Play("heal_pickup_chime")` / `Sfx.Play("coin_pickup")`.

## GameBootstrap.cs (Boot, at the end of the method)

- Start Area 1 music + bed:
  - `Music.PlayStage("a1_surfrock_opener");`   // Stage 1 Lincoln suburbs / Stage 2 Old Hwy 65 (shared loop)
  - `Music.PlayAmbient("lincoln_birds_traffic");`  // Stage-1 ambient bed
- When a title screen exists, call `Music.PlayTitle()` there instead, and start the stage loop on level entry.

---

## Weapon → fire/swing SFX (for ResolveSwing / weapon use)

| Weapon        | Primary cue(s) (stems) |
|---------------|------------------------|
| Sword         | `sword_swing`, break → `sword_break` |
| Shotgun       | `shotgun_blast`, `shotgun_cock` |
| Boomerang     | `boomerang_throw`, `boomerang_return` |
| Pistol        | `pistol` |
| Revolver      | `revolver` |
| Grenade       | `grenade_throw`, `grenade_explode` |
| Ball & chain  | `ballchain_launch`, `ballchain_impact` |
| Whip          | `whip_crack`, head-rip → `whip_headrip_pop` |
| Staff         | `staff_ice` / `staff_fire` / `staff_lightning` |
| Gatling       | `gatling_barrage` |
| Boomerang-gun | `boomeranggun_spin`, shot-down → `boomeranggun_shotdown_break` |
| Rocket        | `rocket_launch`, `rocket_blast` |
| Club          | `club_whack` |
| Bat           | `bat_reflect_ping` |

## Specials / transforms

`werewolf_transform_howl`, `werewolf_auto_slash` (4/s loop during transform), `giant_shotgun_boom`,
`underdog_vaporize_whomp`.

## UI / economy / hazards

- UI: `menu_move`, `confirm`, `cancel`, `coin_pickup`, `full_dime_highlight`, `combo_popup_pips`,
  `barrage_incoming_alarm` (boss barrage telegraph).
- Economy: `pickpocket_steal`, `coins_doubled_jingle`, `checkpoint_chime`.
- Hazards: `car_bus_passby`, `car_horn`, `plane_jet_blast`, `taxiing_plane_whine`, `cow_moo`,
  `trolley_bell`, `trolley_rumble`, `tower_sway_creak`, `rollercoaster_pass`, `causeway_water_splash`.

## Music track names (stems, resolve regardless of subfolder)

- Stage loops: `a1_surfrock_opener`, `a1_synthpunk_mall`, `a2_ragtime_garagerock`,
  `a2_industrial_electronic`, `a3_spaghetti_western`, `a3_hoedown_bluegrass`, `a3_western_dread`,
  `a4_circusrock`, `a4_psychrock`, `a4_orchestralrock`, `a4_electropunk`, `finale_rooftop_approach`.
- Boss cues (`Music.PlayBoss`): `burly`, `colossus`, `helicopter`, `monkeyboss`, `big_armripper`,
  `tank`, `gatlinggunguy`, `boomergunner`, `phil_realized`. (Sandwich Bros reuses `a1_surfrock_opener`.)
- Ambient beds (`Music.PlayAmbient`): `lincoln_birds_traffic`, `galleria_murmur`, `sacramento_oldtown`,
  `airport_tarmac_jet`, `causeway_marsh_wind`, `farm_barnyard`, `dixon_town_wind`, `vallejo_carnival`,
  `marin_redwood`, `goldengate_bridge_wind`, `sf_city_crowd`, `finale_rooftop_wind`.
- Stingers (`Music.Stinger`): `a1_stinger`, `a2_stinger`, `a3_stinger`, `a4_stinger`, `finale_stinger`.
- Other: `title_theme` (`Music.PlayTitle`), `endless_layered`.
