# this.l — Audio Direction

> **Scope:** the concrete audio plan — per-area music style, boss-theme approach, the core SFX list (with a
> concrete count), the intro VO plan, and mix priorities. Fills the `ASSET_MANIFEST.md` §8 "MOSTLY UNSPEC"
> flag with definite decisions.
>
> **Anchors:** chunky-arcade beat-'em-up energy (`UI.md` §1) · comedic transient tone (`VFX.md` §1) · a real
> NorCal road trip Lincoln → SF (`STAGES.md` §1a) · the creator's own voice for the intro (`STAGES.md` §1b) ·
> lightweight-first, runs on low-end (`AREAS.md` §0) — so **streamed music + a compact SFX bank**, no huge
> real-time audio engine.
>
> **[LOCKED] Ownership:** **VO and SFX are creator-produced** (recorded by the dev — this doc is the
> *shot-list / direction* to record against, not an outsourcing spec). **Music = creator-composed** in a
> DAW/tracker (default, matching the VO/SFX ownership and the homemade tone); **fallback if time-boxed:** a
> **royalty-free / CC0 library** matched to each area's genre (§2). Either way the 23-track plan (§2) is the
> spec. *(This is a P2 audio decision — it does not block the build; the game ships with placeholder loops if
> music runs late.)*

---

## 1. Overall direction

- **Style pillar:** **arcade beat-'em-up meets a Californian road-trip mixtape** — punchy, tuneful, genre-hops
  by region so travel *sounds* like progress, mirroring the visual biodiversity.
- **Format:** looping streamed tracks (**OGG Vorbis, 44.1 kHz stereo, ~128 kbps**) for music; **short mono
  WAV/OGG one-shots** for SFX. Target total audio footprint **< 80 MB** to stay lightweight.
- **Tempo language:** BPM rises with danger. Exploration/early ~**110–120 BPM**; area finales/bosses **140–165
  BPM**; Phil's finale **170 BPM**.
- **Loudness target:** master **−14 LUFS integrated**, true-peak **−1 dBTP**.

---

## 2. Music — style per area (one genre/mood each)

| Area / stage | Genre | Mood | BPM |
|---|---|---|---|
| **A1 · Lincoln suburbs (opener)** | **sunny surf-rock** (reverb guitar, tambourine) | carefree, "skipping school," warm | 116 |
| **A1 · Roseville Galleria (mall)** | **frantic synth-punk** | panic-comedy, shoppers screaming | 150 |
| **A2 · Sacramento (Victorian downtown)** | **ragtime-tinged garage rock** | old-town charm turning ugly | 128 |
| **A2 · Sacramento Airport** | **tense industrial electronic** (jet-engine drones) | mechanical, exposed tarmac | 140 |
| **A3 · Hills & Davis causeway** | **spaghetti-western twang** (whistle, twang guitar, wide reverb) | open road, lonesome, sniper tension | 120 |
| **A3 · Farm / Ranch (Monkey Boss)** | **hoedown bluegrass-rock** (banjo, stomp) | chaotic barnyard | 150 |
| **A3 · Dixon (boss rush)** | **sparse ominous western dread** (low drone, sparse percussion) | deserted-town wall, "first big wall" | 135 |
| **A4 · Vallejo (Six Flags)** | **manic circus-rock** (calliope + distorted guitar) | carnival-gone-wrong | 155 |
| **A4 · Marin redwoods** | **atmospheric psych-rock** (delay, tremolo, forest reverb) | filtered light, drifting, eerie calm | 130 |
| **A4 · Golden Gate Bridge** | **driving orchestral-rock** (strings + double-kick) | epic, wind-and-barrage crossing | 160 |
| **A4 · San Francisco streets** | **fast electro-punk** (arcade synths, sirens) | climactic city gauntlet | 165 |
| **Finale · Salesforce rooftop (approach)** | **swelling orchestral build** (leitmotif fragments, no full choir yet) | the elevator climb + rooftop reveal + Phil's monologue — tension before the fight | 150→170 |

- **Endless Mode:** an **adaptive layered version of the SF electro-punk track** — stems (drums / bass / lead /
  screamer synth) **add in every 3 minutes** as the tier ramp climbs (`TUNING.md` §8.3), so intensity tracks
  difficulty. Single track, ~4 stems.
- **Transitions:** area cards (`UI.md` §5) get a **2-second stinger** in the incoming area's genre before its
  loop starts. **[LOCKED] Count = 5 stingers** — one per area entry (A1, A2, A3, A4, Finale) — each **derived
  from the incoming area's loop** (a 2 s intro flourish, no new melodic authoring). **Not counted in the 23
  music assets** (they're sub-2 s cues cut from existing loops), listed here so the builder knows to author 5.

**Music track count:** **12 area/stage loops + 1 title theme + 1 Endless layered track + 9 boss cues = 23
music assets** — the 9 boss cues are **8 per-boss orchestrations of one shared "Phil's Army" motif** (Burly,
Colossus, Helicopter, Monkey Boss, big Arm-Ripper, Tank, Gatling Gun Guy, Boomergunner) **+ Phil's fully
realized version** (§3). Because they all re-orchestrate a single 4-bar motif, authoring is far cheaper than 9
from scratch. *(Sandwich Bros is a **big-version Tier-1**, `BOSSES.md`; it reuses the **Area 1 stage loop**, not
its own cue — so the boss-cue count is 8 sub-bosses + Phil, not counting Sandwich Bros.)*
- **[LOCKED] Stage → loop mapping (all 13 stages covered by the 12 loops; two Area-1 stages share).**
  | Stage | Loop used |
  |---|---|
  | 1 Lincoln suburbs · 2 Rocklin/Old Hwy 65 | **A1 surf-rock opener** (both suburb stages share it) |
  | 3 Roseville Galleria (mall) | **A1 synth-punk (mall)** |
  | 4 Sacramento | **A2 ragtime garage-rock** |
  | 5 Airport | **A2 industrial electronic** |
  | 6 Hills & causeway | **A3 spaghetti-western** |
  | 7 Farm (Monkey Boss) | **A3 hoedown bluegrass** |
  | 8 Dixon (boss rush) | **A3 western dread** |
  | 9 Vallejo | **A4 circus-rock** |
  | 10 Marin redwoods | **A4 psych-rock** |
  | 11 Golden Gate | **A4 orchestral-rock** |
  | 12 SF streets | **A4 electro-punk** |
  | 13 Finale rooftop | **Finale approach loop** + Phil boss cue |
  The **12 ambient beds** map 1:1 to the 12 loops the same way (Stage 2 shares the Lincoln suburb bed). No stage
  is unscored. (Sandwich Bros, the Stage-1/2 boss, reuses the A1 opener as its fight music — no separate cue.)
- **[LOCKED] The Finale is TWO distinct tracks, not one — no double-count.** The **"Finale rooftop (approach)"
  stage loop** (§2, the 12th stage loop) plays over the **elevator climb + rooftop reveal + Phil's monologue**
  (pre-fight); **Phil's boss cue** (the 9th boss cue, §3 — the motif *fully realized* with full orchestra +
  choir) starts **when the fight begins.** Stage 13 has a real pre-boss beat (the climb), so both slots are
  used. They are separate assets: 12 stage loops **and** 9 boss cues both count in full.

---

## 3. Boss-theme approach

- **One shared "Phil's Army" boss motif** — a recognizable 4-bar villain leitmotif — **re-orchestrated per
  boss** rather than 8 unrelated tracks. This is cheap (matches the "big version reuses art" thrift) and ties
  every boss to Phil as the source.
  - **Burly Macho Guy:** motif on **brass + gang-shout chants**.
  - **Colossus:** motif on **massed low strings** (many-figures-as-one).
  - **Helicopter:** motif over a **rotor-blade percussion loop**.
  - **Monkey Boss:** motif on **calliope + bongos** (the circus/monkey color).
  - **big Arm-Ripper (Dixon):** motif stripped to **lone western guitar + gunshot hits**.
  - **Tank:** motif on **military snare + engine drone**.
  - **Gatling Gun Guy:** motif punctuated by the **~5 s "BARRAGE" swell** (audio telegraph aligned to the
    warning, `BOSSES.md` §5.6).
  - **Boomergunner (Marin):** motif with **panning delay** (the orbiting-return gag).
- **Phil (final):** the motif **fully realized** with **orchestra + choir**, and it **quotes each area's theme**
  in sequence during his greatest-hits **reprise summons** — the score does the "greatest-hits gauntlet" too.
- **Phase shift = musical shift:** at each boss HP threshold (`TUNING.md` §7) the track **jumps to a higher-
  intensity section** (add double-kick / raise a semitone) so escalation is audible.
- **Sniper time-slow:** ducks music to a **low-pass, half-speed wash** for the special's duration — the audio
  peak matching the visual peak (`VFX.md` §6).

---

## 4. Core SFX list — **95 one-shots (v1)**

Grouped with concrete per-group counts. Each is a short arcade one-shot; pitch-randomized ±2 semitones at
playback for variety without extra assets. **Counting convention (exact):**
- each **comma-item is one sound**;
- a **`+` inside an item marks two distinct sounds** (e.g. "throw+return" = 2 — two separate one-shots);
- a **slash inside an item is TWO SPELLINGS OF ONE sound** (e.g. "hurt/grunt", "car/bus pass-by", "block/soak"
  = **1** each) — **UNLESS** an explicit **(N)** is attached, which overrides to that count (e.g. "staff cast
  ice/fire/lightning **(3)**" = 3 real elemental variants);
- a trailing **(N)** on an item states its own sound count directly.

Group counts below reflect this convention exactly, and the per-group sums add to the 95 total (the row math in
the Total line is authoritative).

| Group | Count | Contents |
|---|---|---|
| **Player melee** | 8 | punch (×2 var = 2), sweep, **finisher (heavy)**, air-hit, dash-whoosh, jump, land |
| **Player states** | 6 | hurt/grunt, death, weapon-pickup, **heal-pickup chime** (distinct positive cue), weapon-break puff, shield-rush scrape |
| **Impacts / juice** | 6 | hit-spark, finisher-crunch (hitstop cue), enemy stagger, knockdown thud, block/soak, screen-shake boom |
| **Weapon fire** | 24 | sword swing+break (2), shotgun blast+cock (2), boomerang throw+return (2), pistol, revolver, grenade throw+explode (2), ball&chain launch+impact (2), whip crack, **whip head-rip pop**, staff cast ice/fire/lightning (3), gatling barrage, boomerang-gun spin, **boomerang-gun shot-down break**, rocket launch+blast (2), club whack, bat reflect-ping |
| **Enemy signature** | 20 | zombie moan+grab (2), swarmer chitter, head-throw (self-decapitate), fire-blink→BOOM, snapper snap-to-sword, arm-rip, gatling contort, ninja smoke-teleport, sniper scope-in+shot (2), ground-smash overhead+shockwave (2), whistle (tamer), monkey merc chatter, **AA rock-throw**, **ninja shuriken throw**, **boomergunner gun-throw**, **Pod spawn-burst**, **Tank MG stream** (the Tank boss's machine-gun fire, `BOSSES.md` §5.3) |
| **Meter / specials** | 8 | meter tick-up, meter **armed "ready" chime**, sniper time-slow enter, time-resume whoosh, **Werewolf transform-howl**, **Werewolf auto-slash** (the 4/s slash loop during the transform, `CHARACTERS.md` §2.3), **Giant Shotgun boom**, **Underdog Vaporize whomp** |
| **Phil / finale** | 3 | **pencil-draw scribble** (summon), **sharpen scrape** (vulnerable window), **pencil-laser fire** (the scripted kill) |
| **UI** | 7 | menu move, confirm, cancel, coin pickup, **full-dime highlight**, combo-popup pips, **"BARRAGE INCOMING" alarm** |
| **Economy / misc** | 3 | pickpocket steal, coins-doubled jingle, checkpoint chime |
| **Hazards** | 10 | car/bus pass-by, **car horn** (the LOCKED 0.6 s telegraph, `TUNING.md` §6.2), plane jet-blast, **taxiing-plane engine-whine** (the LOCKED 0.8 s telegraph, `TUNING.md` §6.2), cow moo (path-block), **SF trolley bell+rumble (2)**, tower-sway creak, **roller-coaster pass** (Stage 9), **causeway water splash** (fall) |
| **Total** | **95** | 8+6+6+24+20+8+3+7+3+10 |

- **Ambient beds (not counted in the 95 SFX):** **1 looping bed per stage-music slot = 12**, one-to-one with
  the §2 loops: **1** birds/traffic (Lincoln suburbs) · **2** mall murmur + shopper panic (Galleria) · **3**
  old-town street tone (Sacramento downtown) · **4** tarmac hum + jet drone (Airport) · **5** marsh/wind
  (Hills & causeway) · **6** barnyard/livestock (Farm) · **7** dry deserted-town wind (Dixon) · **8** carnival
  crowd + rides (Vallejo) · **9** redwood forest + birdsong (Marin) · **10** bridge wind + cabling hum (Golden
  Gate) · **11** city crowd + sirens (SF streets) · **12** high rooftop wind (Finale). Low in the mix (§6).

---

## 5. Intro VO plan

- **Voice:** the **creator's own voice-over**, recorded via their audio interface (LOCKED, `STAGES.md` §1b) —
  deliberately intimate/homemade, matching the "two friends passing a drawing" origin.
- **Delivery:** narration over **~20 s hand-drawn still picture-clips** (LOCKED). Pin **5 clips**, ~20 s each
  (~1:40 total):
  1. *"In the beginning, there was just **this**."* — the drawings on the page.
  2. Phil escapes and **captures the pencil**.
  3. Phil **hunts the Holy Sharpener** — "he only has so much lead before it runs out."
  4. The stakes — his army begins to spill into the world.
  5. *"Your mission: defeat Phil."* → loads into the game.
- **In-game tutorial VO:** none scripted — the first ~10 s teaches dodge + attack via **on-screen prompts**
  only (weapons are learn-by-use, LOCKED). Keeps VO reserved for the intro and Phil.
- **Phil VO:** the **rooftop monologue** (menacing laughter; "found the Holy Sharpener," **"bring 2D chaos to
  this 3D planet"**) — same creator voice, pitched-down + reverb for villainy (LOCKED, `VIGNETTES.md`). Pin
  **~30 s**, delivered during the elevator climb.
- **Outro VO:** **one last creator-voiced line** over the epilogue/credits (`STORY.md` §3) — same voice/spec,
  pin **~10 s**. This is the only VO besides the intro clips and Phil's monologue.
- **Recording spec:** mono, 44.1 kHz, cleaned/normalized to **−16 LUFS** (sits above music duck, §6).
- **No in-GAMEPLAY spoken dialogue** in v1 — enemies/bosses are non-verbal (grunts, chatter); the only VO is the
  **intro clips, Phil's monologue, and the one outro line** (above). Keeps the VO scope tiny.

---

## 6. Mix priorities

Priority = what wins when the mix gets crowded. Higher ducks lower.

| Priority | Layer | Behavior |
|---|---|---|
| **1 (highest)** | **Intro/Phil VO** | ducks music **−8 dB** while speaking |
| **2** | **Telegraph & warning SFX** (barrage alarm, ground-smash overhead, sniper scope-in, boss tells) | **never masked** — the audio equivalent of `VFX.md` §1 "bullets always readable"; always audible over everything below |
| **3** | **Player action SFX** (attacks, dash, special, finisher) | full level; the player's own feedback |
| **4** | **Enemy SFX** | slightly lower; **voice-limited to 6 concurrent** enemy one-shots, nearest-first |
| **5** | **Music** | ducks to VO; low-passes during time-slow; jumps sections on boss phases |
| **6 (lowest)** | **Ambient beds** | −18 dB under the mix; purely atmospheric |

- **Voice cap:** **24 total concurrent SFX voices** (lightweight budget); when exceeded, drop **lowest-priority
  + oldest** first, so a telegraph never gets stolen by a crowd of enemy chitters.
- **Time-slow bus:** during the sniper special, everything except the special's own SFX routes through a
  **low-pass + 0.5× pitch** bus, restored by the "time-resume whoosh."
- **Ducking is priority-based, not sidechain-pumping** — clean arcade clarity over rhythmic gloss.

---

## 7. Asset summary → feeds `ASSET_MANIFEST.md` §8

- **Music:** 23 tracks (12 area/stage loops · 1 title · 1 Endless layered w/ 4 stems · 9 boss cues — 8 per-boss
  orchestrations of one shared motif + Phil's full realization) + **5 area-transition stingers** (2 s each, cut
  from the incoming loops, not in the 23).
- **Ambient:** 12 area beds (§4, one per stage-music slot).
- **SFX:** **95 core one-shots** (§4).
- **VO:** 5 intro clips (creator voice) + Phil rooftop monologue (~30 s) + **1 outro line** (~10 s, over the epilogue/credits, `STORY.md` §3 / §5) — the only VO besides the intro clips and Phil's monologue.
- **Priority:** Intro VO + core SFX = **P1**; per-area music, ambient beds, boss themes, UI sounds = **P2**
  (matches `ASSET_MANIFEST.md` §8 phasing).
