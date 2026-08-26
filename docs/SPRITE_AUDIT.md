# Sprite Audit — this.l (2026-08-25)

Snapshot of what sprite art exists vs. what's still missing, for the road to a polished demo.

## ✅ DONE (bespoke pixel art, in-game, verified)

- **Heroes (4/4)** — `player_tactical`, `player_shotgunner`, `player_werewolf` (Mexican Gabe), `player_underdog`. Full clip set each: idle, walk, attack_side, attack_up, **attack_down**, sweep, **dash**, hurt, death (9 clips).
- **Enemies (19/19 archetypes)** — every `EnemyArchetype` has a bespoke stick-figure atlas: regular, gunner, zombie, swarmer, heavy, snapper, ninja (bandana tails), sniper, arm-ripper, pickpocket, head-thrower, ground-smasher, anti-aircraft, monkey (9-tail), monkey-tamer, flying-monkey (9-tail), gatling, boomergunner, pod. (5 clips each; pod is an object.)
- **Boss: Phil (1/10)** — hand-built: hollow ring head + top hat + double-sharp pencil; idle/walk/attack/hurt/death + **sharpen** (Holy Sharpener) clip. Auto-loads via `BossController.InitBoss` (bespoke boss atlas if `sprites/bosses/<id>` exists).
- **Weapon pickups (stick-figure-weapon premise)** — 11 hand-built: sword, pistol, revolver, club, bat, staff, whip, boomerang, grenade, ballchain (+ shotgun realistic special). Shown on the ground via `Pickup` sprite loader.
- **Pickups/VFX** — coin, dime, heal, merc-token, sniper-rifle pickups; vfx atlas (hit spark, dash dust, death burst, muzzle, puffs, etc.); blob shadows; car prop.
- **Area props** — `area{1-4}_props/{house,tree}.png` (Victorian / barn / SF rowhouse per area, trimmed so they don't float).

## ❌ MISSING / TODO (priority order)

1. **Bosses — 9/10 missing.** Only Phil is arted; the rest use tinted `enemy_regular`. Ids: `sandwich_bros`, `burly`, `colossus`, `helicopter`, `monkey_boss`, `big_armripper`, `tank`, `boomergunner`, `gatlinggunguy`. Some are "big versions" of enemies (`big_armripper`, `monkey_boss`, `boomergunner`) → could reuse a scaled enemy atlas; the rest need distinct art. **NEEDS STYLE DECISION: big stick-figures vs. distinct?** (Hand-build like Phil is the reliable path for iconic ones.)
2. **Tileable backgrounds (creator ask).** Replace the discrete floating-prop row with a SEAMLESS TILEABLE background strip per area (houses/scenery baked to the ground line, no floating). IN PROGRESS.
3. **Weapon-hold states.** Heroes holding + swinging each weapon (the stick-figure weapons in-hand). Bigger animation pass (create_character_state or hand-built). Specials stay realistic.
4. **Gatling weapon pickup PNG** (minor — only weapon kind without a pickup sprite).
5. **Signature special animations** (optional) — currently code VFX (glow/ring/slow-mo); bespoke sprites would add polish but aren't blocking.
6. **UI art** — Title/character-select/HUD are functional IMGUI placeholders; bespoke UI art is a later polish pass.
7. **Phil polish** (optional) — attack_up/down variants, more frames, a death that reads bigger.

## Notes
- Every enemy/boss atlas auto-activates via `SpriteLibrary.HasAtlas` fallback — dropping a new atlas on disk lights it up with no code change.
- The stick-figure-weapon premise (weapons ARE dead stick figures) applies to weapon-hold states too; only specials are realistic.
