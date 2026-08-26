# Vignette / story-beat player — integration

New, self-contained module under `unity/Assets/Scripts/Story/` (namespace `ThisL`,
assembly `ThisL.Runtime`). Builds on the existing IMGUI/audio foundation
(`UI/Hud.cs`, `Audio/Music.cs`, `Audio/Sfx.cs`). No existing files were edited.

## Files
- **`VignetteData.cs`** — plain-data model: `Vignette` (id + title + ordered
  `VignetteBeat[]`), `VignetteBeat` (text `Lines` + optional `ImageKey` + one optional
  audio cue tagged by `VignetteCueBank`), and `VignetteCatalog` (id→vignette registry).
- **`VignetteScripts.cs`** — the authored campaign catalog
  (`VignetteScripts.Catalog`), transcribed from `VIGNETTES.md` + `STORY.md`.
- **`VignettePlayer.cs`** — the `MonoBehaviour` that plays a vignette.

## How to trigger a vignette (the lead, between stages)

`VignettePlayer` is self-initializing (like `Music`/`Sfx`) — no scene wiring needed.
Call `Play` off the shared `Instance`:

```csharp
// Between-stage handoff: play the vignette, then start the next stage in onDone.
VignettePlayer.Instance.Play(VignetteScripts.A1Galleria, () =>
{
    // <- runs once the player dismisses the last beat, at NORMAL time scale.
    StartStage(nextStageId);
});
```

- `Play(string vignetteId, System.Action onDone)` — looks the id up in
  `VignetteScripts.Catalog`. Ids are exposed as constants on `VignetteScripts`
  (`Intro`, `A1LincolnOpener`, `A1Galleria`, … `FinaleRooftop`, `Outro`) — prefer
  those over raw strings.
- If the id is unknown or empty, `onDone` fires **immediately** (gameplay never
  blocks on a missing beat) and a warning is logged.
- `PlayVignette(Vignette v, System.Action onDone)` — same, but plays an in-memory
  `Vignette` directly (tests / one-off beats), bypassing the catalog.

### Suggested placement in the flow
Per `UI.md §5`, the stage transition already shows an **Area card** (2 s, plays the
area stinger). Play the vignette **after** the Area card and **before** handing
control to gameplay:

`… stage clear → Results/grade → Area card → VignettePlayer.Play(nextVignette, startNextStage) →` play.

`intro` plays before Stage 1; `finale_rooftop` before the Phil fight (Stage 13);
`outro` after the pencil-laser finisher, before credits.

## How `onDone` chains back to gameplay
- While a vignette is up, `IsPlaying == true` and **`Time.timeScale` is forced to 0**
  (gameplay frozen). Input is polled **unscaled**, so the panel still advances.
- The player advances **one beat per key/mouse press** ("press any key / Enter to
  continue"); a 0.2 s unscaled input-lock at each beat swallows the launching press
  so it can't double-skip.
- On the last beat's dismissal the player **restores the previous `Time.timeScale`
  first**, then invokes `onDone` exactly once — so your callback runs at normal time
  and can immediately spawn/enable the next stage.
- `IsPlaying` lets gameplay code gate itself (e.g. skip a frame of AI). Calling
  `Play` again while one is on screen is a no-op (warns). `Skip()` force-ends the
  current vignette (restores time, fires `onDone`).
- Each beat fires its cue when shown: `Stinger`→`Music.Stinger`,
  `StageMusic`→`Music.PlayStage`, `Ambient`→`Music.PlayAmbient`, `Sfx`→`Sfx.Play`.
  Cue names are verified against `assets/audio` (`music/stingers`, `music/other`,
  `music/boss_cues`, `sfx/phil_finale`, `sfx/player_states`).

## Rendering
Full-screen IMGUI overlay in the same style as `Hud` (scaled to the 360 px design
height, `GUI.depth = -1000` so it sits above the HUD): a solid dim panel, the beat's
centered text, a pulsing advance prompt, and a `N / total` beat counter.

## Art gap (note for the lead / art)
- **The per-stage vignettes are wordless pantomime in the design** (`VIGNETTES.md`:
  enemies are *puppeteered* to act out a mechanic). No bespoke vignette art or acted
  set-pieces exist yet, so this first pass renders **text-panel stand-ins** — short
  caption copy describing the action, marked `// FIRST-PASS` in `VignetteScripts.cs`.
  Replace those beats with the scripted actor animation when art lands; the
  intro/Phil/outro **VO copy is verbatim** from `STORY.md §6` and can stay as text.
- `VignetteBeat.ImageKey` is a ready hook for a full-screen still: `VignettePlayer`
  looks for `assets/vignettes/<key>.png`, then a backdrop preview, else draws the
  solid panel. Every campaign beat currently leaves `ImageKey` null (no art), so all
  panels are solid today. Point a key at a PNG once stills exist — no code change.

## Authored vignettes (14)
`intro`, `a1_lincoln_opener`, `a1_galleria`, `a2_sacramento`, `a2_airport`,
`a3_causeway`, `a3_farm`, `a3_dixon`, `a4_vallejo`, `a4_marin`, `a4_golden_gate`,
`a4_sf_streets`, `finale_rooftop`, `outro` — i.e. the intro cinematic, all 12
`VIGNETTES.md` per-stage/finale set-pieces, and the outro epilogue.
