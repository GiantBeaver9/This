using System.Collections.Generic;

namespace ThisL
{
    /// <summary>
    /// Which audio bank a beat's cue is routed through when the beat shows
    /// (VIGNETTES.md set-pieces reuse existing audio: area stingers + boss cues in
    /// the <see cref="Music"/> bank, Phil/UI one-shots in the <see cref="Sfx"/> bank).
    /// </summary>
    public enum VignetteCueBank
    {
        None,       // no audio on this beat
        Stinger,    // Music.Stinger(cue)      e.g. "a1_stinger", "finale_stinger"
        Sfx,        // Sfx.Play(cue)           e.g. "sharpen_scrape", "confirm"
        StageMusic, // Music.PlayStage(cue)    e.g. "finale_rooftop_approach", "phil_realized"
        Ambient,    // Music.PlayAmbient(cue)  e.g. "finale_rooftop_wind"
    }

    /// <summary>
    /// One screen of a vignette: the text lines shown, an optional still/backdrop
    /// image key, and one optional audio cue fired the frame the beat appears
    /// (VIGNETTES.md — the between-stage story-beat interstitials; STORY.md §5 "where
    /// story is delivered"). Plain data, no Unity types, so scripts author freely.
    /// </summary>
    public sealed class VignetteBeat
    {
        /// <summary>The text lines to show (drawn centered, one under the next). Never null.</summary>
        public readonly string[] Lines;

        /// <summary>
        /// Optional image/backdrop key resolved off disk by the player (e.g. a
        /// backdrop-theme folder). Null = no bespoke still; the player draws a solid
        /// dim panel instead. No bespoke vignette art exists yet (see _INTEGRATION.md
        /// "Art gap"), so campaign beats leave this null.
        /// </summary>
        public readonly string ImageKey;

        /// <summary>Which bank <see cref="Cue"/> plays through when the beat shows.</summary>
        public readonly VignetteCueBank CueBank;

        /// <summary>The audio cue name for <see cref="CueBank"/>, or null for silence.</summary>
        public readonly string Cue;

        public VignetteBeat(
            string[] lines,
            string cue = null,
            VignetteCueBank cueBank = VignetteCueBank.None,
            string imageKey = null)
        {
            Lines = lines ?? System.Array.Empty<string>();
            Cue = cue;
            CueBank = cue == null ? VignetteCueBank.None : cueBank;
            ImageKey = imageKey;
        }
    }

    /// <summary>
    /// A single named vignette — an ordered list of <see cref="VignetteBeat"/> the
    /// player steps through, advancing on input (VIGNETTES.md; the campaign set is
    /// authored in <see cref="VignetteScripts"/>). Looked up by <see cref="Id"/>
    /// from <see cref="VignettePlayer.Play(string, System.Action)"/>.
    /// </summary>
    public sealed class Vignette
    {
        /// <summary>Stable lookup id (e.g. "intro", "a1_galleria", "finale_rooftop").</summary>
        public readonly string Id;

        /// <summary>Short display title shown small above the beat text (may be null).</summary>
        public readonly string Title;

        /// <summary>The ordered beats. Never null; may be empty (then the player finishes at once).</summary>
        public readonly VignetteBeat[] Beats;

        public Vignette(string id, string title, params VignetteBeat[] beats)
        {
            Id = id;
            Title = title;
            Beats = beats ?? System.Array.Empty<VignetteBeat>();
        }

        public int BeatCount => Beats.Length;
    }

    /// <summary>
    /// A registry of vignettes keyed by id. <see cref="VignetteScripts"/> builds the
    /// campaign catalog once; <see cref="VignettePlayer"/> resolves ids against it.
    /// </summary>
    public sealed class VignetteCatalog
    {
        private readonly Dictionary<string, Vignette> _byId = new();

        /// <summary>Every vignette in insertion order (campaign order).</summary>
        public readonly List<Vignette> Ordered = new();

        public void Add(Vignette v)
        {
            if (v == null || string.IsNullOrEmpty(v.Id)) return;
            _byId[v.Id] = v;
            Ordered.Add(v);
        }

        public Vignette Get(string id) =>
            id != null && _byId.TryGetValue(id, out var v) ? v : null;

        public bool Contains(string id) => id != null && _byId.ContainsKey(id);

        public int Count => Ordered.Count;
    }
}
