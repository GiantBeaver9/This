# Sprite Brief — playable-character generation reference

Everything needed to generate the 4 playable characters (and the tutorial "zebra") so they
drop straight into the game. **You send me the frames; I assemble the atlas + JSON and drop
them in — you never touch code.**

---

## Global specs (match these so it slots in)

- **Style:** pixel art, **side-view** — the character faces **RIGHT** by default; the game flips it
  for left. Flat / front-on shading (the 2.5D look), clean readable silhouette.
- **Size:** character ≈ **48 px tall** (1 world-unit = 24 px). Frame canvas ~**54 × 65 px**,
  **transparent background**, feet on the **bottom-center** (that's the pivot). Generating larger and
  downscaling/pixelating to ~48px tall for touch-up is totally fine.
- **Palette:** the shared **32-color base** (below) + each character's **3-color accent**. Staying on
  palette is what makes them read as one game.
- **Playback:** **12 fps** (anime-on-2s). So a "5-frame" attack ≈ 0.42s of art.
- **Deliver either:** a **horizontal strip per animation** (frames left→right), **or** individual
  numbered PNGs per animation (`walk_00.png … walk_07.png`). Per character, one folder of these.

## Animation set (per character) — the creator's priority list

Draw the left column; the middle column is the game clip name your frames drop into
(I map them). **Punch forward/back is ONE art set, mirrored by the game.**

| What you draw | Game clip | Frames | Notes |
|---|---|---|---|
| **Idle** | `idle` | 2-4 loop | resting |
| **Forward walk** | `walk` | **3-4 loop** | fluid cycle; flipped for walking left (back) |
| **Stop from movement** | `land` | 1-2 | quick skid/settle when you stop |
| **Punch forward** | `attack_side` | 3-5 | horizontal punch; **punch BACK = same art mirrored** (game flips by facing) |
| **Upper cut** | `attack_up` | 3-4 | the up strike |
| **Punch down** | `attack_down` | 3-4 | the down strike |
| **Diagonals** | — | 0 | share the cardinals above (dominant-cardinal, no extra art) |
| **Leg sweep** | `sweep` | 4-5 | the knockdown-setter (combo hit 3) |
| **Stomp** (unarmed execution) | `finisher` | 3 | the no-weapon execution on a downed enemy |
| **Weapon execution** (one per weapon) | `execute_<weapon>` | 3-4 ea | cinematic finisher per weapon (sword/shotgun/…) |
| **Hurt** / **Death** | `hurt` / `death` | 2 / 4-6 | flinch / die |
| (optional 1st pass) dash / jump / air punch | `dash` / `jump` / `air_side` | 2-3 ea | fill in later |

**Code note:** `sweep`, `finisher`, and the per-weapon `execute_*` clips currently reuse
`attack_side` in-game — I'll wire them to play on those specific actions the moment your art
for them lands (one small hook each). Everything else is already read live.

## Special-move animations (one per character)

The special *mechanics* already fire in-game; these clips play on top of them (the game
already plays the clip name in the middle column, and layers a programmatic glow/ring so it
reads until your frames arrive).

| Character | What you draw | Game clip | Frames |
|---|---|---|---|
| **Adam** (sniper) | spins the sniper around and fires | `special` | 4-6 |
| **Aaron** (shotgun) | spins the shotgun around and fires | `special` | 4-6 |
| **Gabe** (werewolf) | power-lean → arch back → **scream** with a power glow as he changes into the wolf | `transform` | 5-7 |
| **Bert** (underdog) | power-up burst — instantly vaporizes everything in a **wide radius** around him | `special` | 3-4 |

Notes: Adam & Aaron share the exact same "spin the gun and fire" motion (different guns).
Gabe's `transform` leads into his separate **wolf sub-set** (above). Bert's is a single
empower/vaporize beat — the wide-radius pop is drawn as a burst around him (the game already
draws the expanding radius ring, sized to his fill tier: 3 / 4 / 5 wu).

## The 4 characters (identity · silhouette · accent · special)

| Character | actor id | Look / silhouette | 3-color accent | Special (needs its own anim) |
|---|---|---|---|---|
| **Tactical** *(you)* | `player_tactical` | **lean**; cargo pants + tactical vest + **backwards cap**; sniper case on back | olive-green / black / orange | **Sniper time-slow**: draw → aim → fire → recover |
| **Shotgunner** *(redheaded, bulky)* | `player_shotgunner` | **bulky, bearded**; flannel + jeans; **broadest** silhouette | rust-red / denim-blue / cream | **Giant Shotgun**: pull huge shotgun, blast the crowd off-screen |
| **Werewolf (Gabe)** | `player_werewolf` | scruffy, medium; band tee + jacket → **transforms into a hunched brown wolf** | brown / grey / yellow-eyes | **Transform** + wolf sub-set (below) |
| **Underdog** *(short — hard mode)* | `player_underdog` | **short, slight**; comically **oversized hoodie**; shortest silhouette | purple / white / lime | **Vaporize**: close-radius pop + 30s power-up glow |

**They must read by silhouette alone** — lean / broad / medium / short.

### Werewolf wolf sub-set (extra, only for Gabe)
Beyond the human set above, the transform needs: `transform` 5 · `wolf_idle` 3 (loop) · `wolf_run` 6 (loop)
· `wolf_slash` 4 · `wolf_air_slash` 3 · `revert` 4. A hunched brown wolf, glowing eyes.

## The zebra (tutorial demonstrator) — optional, drop-in
Put an atlas at `assets/sprites/characters/zebra/` (`zebra_atlas.png` + `zebra.json`, or just send me
frames) with at least `idle`, `walk`, `attack_side`. The tutorial's demonstrator auto-uses it the moment
it exists (falls back to the stick figure until then).

## The 32-color base palette (hex — the single source of truth)
```
Ink/mono:  #0D0B0E #2A2A33 #4A4A57 #7A7A88 #B8B8C4 #F4F2EC
Reds:      #B31E2B (gore) #E8433F #F5794F #7A1420
Oran/yel:  #F2A03D #FFD24A #C77A2A #FFF2B0
Greens:    #234D2C #3A7D44 #6CBF5A #A8D98B
Blue/cyan: #1B3A5C #2E6FB0 #4AA3D8 #9FD6EF #CFEAF7
Purp/pink: #6A3D8A #C86FA8
Brown:     #3F2A17 #6B4423 #9C6B3F #C99A6A
Skin:      #8A5A3A #D99A6C #E6B88A
```

## Handing them to me
Send a character's frames (strip or numbered PNGs). I build `<actor>_atlas.png` + `<actor>.json` at
`assets/sprites/characters/<actor>/`, and it loads live (Point filter, ppu 24) — no rebuild needed.
One flag note: attack frames currently play **reversed** (placeholder art was backwards); the moment your
correctly-ordered attack art is in, I flip `SpriteAnimator.ReverseAttackClips` to false.

*(Enemies & bosses have their own descriptions in `ENEMIES.md` / `BOSSES.md` if you decide to upgrade them
past the stick figures later — just say the word and I'll surface those too. Environmental effects / hazards
for the "more hectic" pass live in `VFX.md` + `AREAS.md`.)*
