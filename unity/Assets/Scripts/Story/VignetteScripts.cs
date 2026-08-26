namespace ThisL
{
    /// <summary>
    /// The authored campaign vignettes, transcribed from VIGNETTES.md (the per-stage
    /// teaching set-pieces + the Salesforce-rooftop finale) and STORY.md (§6 verbatim
    /// VO copy for the intro cinematic, Phil's rooftop monologue, and the outro line).
    ///
    /// Faithfulness: the intro / Phil / outro beats reproduce STORY.md §6's draft VO
    /// verbatim (that doc marks it "creator re-voices / may re-word"). The twelve
    /// per-stage vignettes are wordless pantomime in the design (VIGNETTES.md legend:
    /// enemies are *puppeteered* to act out a mechanic), so their on-screen caption
    /// text is invented stand-in copy — marked <c>// FIRST-PASS</c> — that stands in
    /// for the acted set-piece until the bespoke animation/art lands (see
    /// _INTEGRATION.md "Art gap"). Nothing here invents characters beyond STORY.md.
    ///
    /// Audio cues reuse existing banks: area stingers ("aN_stinger", "finale_stinger")
    /// via <see cref="Music.Stinger"/>; the finale approach loop / Phil's realized cue
    /// via <see cref="Music.PlayStage"/>; Phil/UI one-shots via <see cref="Sfx"/>.
    /// All cue names were verified against assets/audio (music/stingers, music/other,
    /// music/boss_cues, sfx/phil_finale, sfx/player_states).
    /// </summary>
    public static class VignetteScripts
    {
        private static VignetteCatalog _catalog;

        /// <summary>The campaign vignette catalog (built once, in campaign order).</summary>
        public static VignetteCatalog Catalog => _catalog ??= Build();

        // Convenience id constants so callers avoid stringly-typed typos.
        public const string Intro          = "intro";
        public const string A1LincolnOpener = "a1_lincoln_opener"; // Stage 1
        public const string A1Galleria     = "a1_galleria";        // Stage 3
        public const string A2Sacramento   = "a2_sacramento";      // Stage 4
        public const string A2Airport      = "a2_airport";         // Stage 5
        public const string A3Causeway     = "a3_causeway";        // Stage 6
        public const string A3Farm         = "a3_farm";            // Stage 7
        public const string A3Dixon        = "a3_dixon";           // Stage 8
        public const string A4Vallejo      = "a4_vallejo";         // Stage 9
        public const string A4Marin        = "a4_marin";           // Stage 10
        public const string A4GoldenGate   = "a4_golden_gate";     // Stage 11
        public const string A4SfStreets    = "a4_sf_streets";      // Stage 12
        public const string FinaleRooftop  = "finale_rooftop";     // Stage 13
        public const string Outro          = "outro";

        private static VignetteCatalog Build()
        {
            var c = new VignetteCatalog();

            // =================================================================
            // INTRO CINEMATIC — STORY.md §6 (five ~20 s hand-drawn still-clips,
            // creator voice). Clips 1 & 5 are locked verbatim; 2–4 are the drafted
            // fill. Reproduced verbatim here.
            // =================================================================
            c.Add(new Vignette(Intro, "this.l",
                new VignetteBeat(new[]
                {
                    "In the beginning,",
                    "there was just this.",
                }, cue: "title_theme", cueBank: VignetteCueBank.StageMusic),

                new VignetteBeat(new[]
                {
                    "Two friends. One notebook. One pencil —",
                    "passing a drawing back and forth,",
                    "the way we always did.",
                    "",
                    "Until one of the doodles got ideas.",
                    "A skinny little guy in a top hat. Phil.",
                    "",
                    "Turns out that pencil was magic...",
                    "and Phil figured out he could pick it up —",
                    "and walk right off the page.",
                }),

                new VignetteBeat(new[]
                {
                    "But a magic pencil still runs out of lead.",
                    "So Phil went hunting — for the Holy Sharpener,",
                    "the one thing that'd let him draw forever.",
                    "",
                    "And every mile of the way, he scratched up",
                    "something new and nasty to keep us off his back.",
                }),

                new VignetteBeat(new[]
                {
                    "And the scribbles didn't stay on the paper.",
                    "They came crawling off the flat,",
                    "out of two dimensions and into ours —",
                    "a whole hand-drawn army spilling into the real world.",
                    "",
                    "Somebody's got to put it all back where it belongs.",
                }),

                new VignetteBeat(new[]
                {
                    "Your mission:",
                    "defeat Phil.",
                }, cue: "confirm", cueBank: VignetteCueBank.Sfx)));

            // =================================================================
            // PER-STAGE TEACHING VIGNETTES — VIGNETTES.md table. Wordless pantomime
            // in design; caption copy below is FIRST-PASS stand-in text.
            // =================================================================

            // A1 · Lincoln suburbs (opener) — teaches the punch. VIGNETTES.md.
            c.Add(new Vignette(A1LincolnOpener, "Lincoln, California",
                new VignetteBeat(new[]
                {
                    "A dancing Zebra struts up to a thug...",   // FIRST-PASS
                    "and cracks him with a single punch.",      // FIRST-PASS
                    "",
                    "That's your whole vocabulary here: the punch.", // FIRST-PASS
                }, cue: "a1_stinger", cueBank: VignetteCueBank.Stinger)));

            // A1 · Galleria mall — zombie-on-shot + grab. VIGNETTES.md.
            c.Add(new Vignette(A1Galleria, "Galleria Mall",
                new VignetteBeat(new[]
                {
                    "A security guard panics and shoots a thug —",   // FIRST-PASS
                    "so it zombifies, grabs him, and they both go down.", // FIRST-PASS
                    "",
                    "Shots turn thugs into grabbers. Don't get grabbed.", // FIRST-PASS
                }, cue: "a1_stinger", cueBank: VignetteCueBank.Stinger)));

            // A2 · Sacramento (Victorian) — the Whip's pull / crowd control. VIGNETTES.md.
            c.Add(new Vignette(A2Sacramento, "Sacramento — Old Town",
                new VignetteBeat(new[]
                {
                    "One enemy whips another, pulls him off his feet —", // FIRST-PASS
                    "then both of them spot you and turn.",              // FIRST-PASS
                    "",
                    "The Whip reaches out and drags you in. Mind the range.", // FIRST-PASS
                }, cue: "a2_stinger", cueBank: VignetteCueBank.Stinger)));

            // A2 · Airport — head-grenades + bat-reflect. VIGNETTES.md.
            c.Add(new Vignette(A2Airport, "Sacramento International",
                new VignetteBeat(new[]
                {
                    "Enemies lob their own heads like grenades at the planes —", // FIRST-PASS
                    "then a Bat enemy swats a fastball head into a Cessna. Boom.", // FIRST-PASS
                    "",
                    "Head-grenades arc in; the Bat will knock them right back.",  // FIRST-PASS
                }, cue: "a2_stinger", cueBank: VignetteCueBank.Stinger)));

            // A3 · Causeway — Sniper apex punish + dime→whistle→monkey. VIGNETTES.md.
            c.Add(new Vignette(A3Causeway, "Yolo Causeway",
                new VignetteBeat(new[]
                {
                    "Two thugs dive for a dropped dime.",                    // FIRST-PASS
                    "One leaps high — a Sniper shoots him out of the air.",  // FIRST-PASS
                    "The other grabs the dime, whistles,",                   // FIRST-PASS
                    "and a monkey drags him clean off the screen.",          // FIRST-PASS
                    "",
                    "Don't hang in the air. And that dime? Grab it first.",  // FIRST-PASS
                }, cue: "a3_stinger", cueBank: VignetteCueBank.Stinger)));

            // A3 · Farm/Ranch — only your mercs damage the Monkey Boss. VIGNETTES.md.
            c.Add(new Vignette(A3Farm, "The Ranch",
                new VignetteBeat(new[]
                {
                    "The Monkey Boss flips a dime into the air.",            // FIRST-PASS
                    "A thug catches it — a monkey merc pops out",            // FIRST-PASS
                    "and unloads on the boss.",                              // FIRST-PASS
                    "",
                    "Your punches don't faze him. Only the monkeys do.",     // FIRST-PASS
                }, cue: "a3_stinger", cueBank: VignetteCueBank.Stinger)));

            // A3 · Dixon (boss rush) — Arm-Ripper: rip arms -> akimbo guns. VIGNETTES.md.
            c.Add(new Vignette(A3Dixon, "Dixon",
                new VignetteBeat(new[]
                {
                    "An Arm-Ripper tears the arms off the guy next to him —", // FIRST-PASS
                    "and opens fire with a pair of akimbo pistols.",          // FIRST-PASS
                    "",
                    "Rip first, shoot second. Those stolen guns hurt.",       // FIRST-PASS
                }, cue: "a3_stinger", cueBank: VignetteCueBank.Stinger)));

            // A4 · Vallejo (Six Flags) — Ninja teleport-kill + Pickpocket 2x. VIGNETTES.md.
            c.Add(new Vignette(A4Vallejo, "Vallejo — Six Flags",
                new VignetteBeat(new[]
                {
                    "A Pickpocket lifts a Ninja's coins and bolts.",         // FIRST-PASS
                    "The Ninja blinks across the screen and cuts him down —", // FIRST-PASS
                    "and the stolen ten cents doubles to twenty.",           // FIRST-PASS
                    "",
                    "Ninjas teleport onto you. Kill a Pickpocket for double.", // FIRST-PASS
                }, cue: "a4_stinger", cueBank: VignetteCueBank.Stinger)));

            // A4 · Marin redwoods — Boomergunner. VIGNETTES.md.
            c.Add(new Vignette(A4Marin, "Marin Redwoods",
                new VignetteBeat(new[]
                {
                    "A Boomergunner terrorizing civilians hurls his boomerang gun —", // FIRST-PASS
                    "it shoots a bystander on the way out, then loops back to his hand.", // FIRST-PASS
                    "",
                    "The gun comes and goes. It's dangerous both directions.",  // FIRST-PASS
                }, cue: "a4_stinger", cueBank: VignetteCueBank.Stinger)));

            // A4 · Golden Gate — zoner + Gatling barrage + car cover. VIGNETTES.md.
            c.Add(new Vignette(A4GoldenGate, "Golden Gate Bridge",
                new VignetteBeat(new[]
                {
                    "A thug pushes forward — a Ground Smasher slams the deck,",  // FIRST-PASS
                    "knocking him off-balance — and a Gatling barrage",          // FIRST-PASS
                    "eviscerates him where he stands.",                          // FIRST-PASS
                    "But the guy crouched behind a car? Not a scratch.",         // FIRST-PASS
                    "",
                    "When the barrage opens up, get a car between you and it.",  // FIRST-PASS
                }, cue: "a4_stinger", cueBank: VignetteCueBank.Stinger)));

            // A4 · SF streets — trolley flattens all except the immovable Heavy. VIGNETTES.md.
            c.Add(new Vignette(A4SfStreets, "San Francisco Streets",
                new VignetteBeat(new[]
                {
                    "The trolley barrels down the lane and flattens a thug —",   // FIRST-PASS
                    "but the Heavy doesn't even look up. He just steps aside,",  // FIRST-PASS
                    "and the trolley rolls on.",                                 // FIRST-PASS
                    "",
                    "The trolley kills everything. Everything that can move.",   // FIRST-PASS
                }, cue: "a4_stinger", cueBank: VignetteCueBank.Stinger)));

            // =================================================================
            // FINALE — Phil's rooftop monologue. VIGNETTES.md finale row + STORY.md
            // §6 "Phil's rooftop monologue" (verbatim). Menacing laughter, tower sway
            // foreshadow. Music: the finale approach loop; the phil_realized boss cue
            // lands on the reveal; sharpen/scribble one-shots punctuate.
            // =================================================================
            c.Add(new Vignette(FinaleRooftop, "Salesforce Tower — Rooftop",
                new VignetteBeat(new[]
                {
                    "Heh. Heh-heh-heh...",
                    "You actually made it.",
                    "All this way — up the whole coast — for a pencil.",
                }, cue: "finale_rooftop_approach", cueBank: VignetteCueBank.StageMusic),

                new VignetteBeat(new[]
                {
                    "You still don't get it, do you?",
                    "I found the Holy Sharpener.",
                    "Right up here, at the very top.",
                    "No more running dry. No more counting my lead.",
                }, cue: "sharpen_scrape", cueBank: VignetteCueBank.Sfx),

                new VignetteBeat(new[]
                {
                    "I can draw forever now —",
                    "every monster, every bad dream,",
                    "every ugly little idea I ever had,",
                    "straight out of my head and down onto your nice clean streets.",
                }, cue: "phil_realized", cueBank: VignetteCueBank.StageMusic),

                new VignetteBeat(new[]
                {
                    "I'm going to bring 2D chaos to this 3D planet,",
                    "and there aren't enough of you left to erase me.",
                    "",
                    "Feel that? The tower's already swaying.",
                    "The whole world's starting to smudge at the edges.",
                }),

                new VignetteBeat(new[]
                {
                    "Come on up, then —",
                    "let me draw you one last friend.",
                }, cue: "pencil_draw_scribble", cueBank: VignetteCueBank.Sfx)));

            // =================================================================
            // OUTRO — STORY.md §3/§6 (epilogue still-clip, ~10 s, creator voice).
            // Verbatim. The lead plays this after the pencil-laser finisher, before
            // credits. FIRST-PASS: the ambient wind bed under it is an added touch.
            // =================================================================
            c.Add(new Vignette(Outro, "this.l",
                new VignetteBeat(new[]
                {
                    "And just like that...",
                    "the pencil came home.",
                    "",
                    "Phil's back on the page, right where we drew him —",
                    "and it's just two friends again.",
                    "One notebook. One drawing.",
                    "Passing it back and forth.",
                }, cue: "finale_rooftop_wind", cueBank: VignetteCueBank.Ambient))); // FIRST-PASS: wind bed choice

            return c;
        }
    }
}
