using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Plays a clip of atlas frames on a SpriteRenderer at the design's 12 fps.
    /// Clips either loop (idle/walk) or play once and hold the last frame
    /// (attacks, hurt, death). Facing is applied by flipping X.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteAnimator : MonoBehaviour
    {
        [System.NonSerialized] public SpriteLibrary.ActorSprites Set;

        /// <summary>
        /// Optional weapon skin (the "swing a dead stick figure" art): a partial set that only
        /// carries <c>idle</c> (holding the weapon) and <c>swing</c> (attacking with it). When set,
        /// the player's idle uses the overlay idle and any attack/sweep uses the overlay swing;
        /// everything else (walk/hurt/death/…) falls through to <see cref="Set"/>. Null = no weapon.
        /// </summary>
        [System.NonSerialized] public SpriteLibrary.ActorSprites Overlay;

        /// <summary>When false, the overlay is used ONLY for attack/swing frames — idle and walk fall
        /// through to the base <see cref="Set"/>. Ranged weapons set this off so the gun character just
        /// carries the base melee idle/walk (rifle slung) and only snaps to the aim/fire pose when
        /// shooting (creator: "with the gun use the same melee animation, only aim & fire when shooting").</summary>
        [System.NonSerialized] public bool OverlayIdleWalk = true;

        public int Fps = Tuning.AnimFps;

        private SpriteRenderer _sr;
        private Sprite[] _frames;
        private string _clip;
        private float _t;
        private bool _loop;
        private bool _finished;

        public bool Finished => _finished;
        public string CurrentClip => _clip;

        /// <summary>The placeholder attack frames read backwards (the extend looks like a
        /// retract — a "pelvic thrust" instead of a punch), so attack_* clips are played in
        /// reverse. Set false once bespoke attack art (already in the right order) lands.</summary>
        public static bool ReverseAttackClips = true;

        private void Awake() => _sr = GetComponent<SpriteRenderer>();

        public void SetFacing(int dir) // -1 left, +1 right
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            _sr.flipX = dir < 0;
        }

        /// <summary>Start a clip. No-op if the same looping clip is already playing.</summary>
        public void Play(string clip, bool loop, bool restart = false)
        {
            if (!restart && loop && _clip == clip && !_finished) return;

            var frames = ResolveFrames(clip, out bool fromOverlay);
            if (frames == null || frames.Length == 0)
            {
                // Fall back to idle/first so we always show something.
                frames = Set?.FirstOf(clip) != null ? new[] { Set.FirstOf(clip) }
                       : Set?.First != null ? new[] { Set.First } : null;
                if (frames == null) return;
                fromOverlay = false;
            }
            _clip = clip;
            // Base placeholder attacks are authored reversed; bespoke weapon swing art is not.
            bool reverse = !fromOverlay && ReverseAttackClips && (Set == null || Set.ReverseAttacks);
            _frames = (reverse && clip != null && clip.Contains("attack")) ? Reversed(frames) : frames;
            // A weapon SWING must END on its strike and HOLD it. Authored swings trail back to a
            // rest/guard pose (the widest, most-extended frame sits mid-clip), so a once-through play
            // flashes the hit then settles low — reading as the swing "retracting" / playing in reverse
            // (creator: the sword "plays in reverse"). Trim the frames AFTER the most-extended one so a
            // one-shot overlay swing freezes on the strike. Applies to every hero+weapon overlay.
            if (fromOverlay && !loop && _frames.Length > 2)
                _frames = TrimToStrike(_frames);
            _loop = loop;
            _t = 0f;
            _finished = false;
            _sr.sprite = _frames[0];
        }

        /// <summary>Pick the frames for a clip, preferring the weapon <see cref="Overlay"/> (idle→idle,
        /// any attack/sweep→swing) and falling back to the base <see cref="Set"/>.</summary>
        private Sprite[] ResolveFrames(string clip, out bool fromOverlay)
        {
            fromOverlay = false;
            if (Overlay != null && clip != null)
            {
                // idle keeps the weapon in hand; a real attack/sweep uses the weapon swing. Walk uses
                // the weapon's walk clip IF it exists, else falls through to the base walk (moving
                // legs) rather than a static held pose that slides. Dash/charge fall through to base
                // (mapping them to the swing produced a horrific sliding-swing sprite).
                string oclip = (clip == "idle" && OverlayIdleWalk) ? "idle"
                             : (clip == "walk" && OverlayIdleWalk) ? (Overlay.Clips.ContainsKey("walk") ? "walk" : null)
                             : (clip.Contains("attack") || clip == "sweep") ? "swing"
                             : null;
                if (oclip != null && Overlay.Clips.TryGetValue(oclip, out var of) && of != null && of.Length > 0)
                {
                    fromOverlay = true;
                    return of;
                }
            }
            if (Set != null && Set.Clips.TryGetValue(clip, out var frames) && frames != null && frames.Length > 0)
                return frames;
            return null;
        }

        private static Sprite[] Reversed(Sprite[] src)
        {
            var r = new Sprite[src.Length];
            for (int i = 0; i < src.Length; i++) r[i] = src[src.Length - 1 - i];
            return r;
        }

        /// <summary>Keep frames up to and INCLUDING the most-extended (widest) one — the strike — and
        /// drop the trailing retract-to-rest frames, so a one-shot swing freezes on the hit. If the peak
        /// is the very first frame (nothing to wind up), keep the clip as-is rather than collapsing to one.</summary>
        private static Sprite[] TrimToStrike(Sprite[] src)
        {
            int peak = 0;
            float wMax = src[0] != null ? src[0].rect.width : 0f;
            for (int i = 1; i < src.Length; i++)
            {
                float w = src[i] != null ? src[i].rect.width : 0f;
                if (w > wMax) { wMax = w; peak = i; }
            }
            if (peak <= 0 || peak == src.Length - 1) return src; // peak already last (or first) → unchanged
            var r = new Sprite[peak + 1];
            System.Array.Copy(src, r, peak + 1);
            return r;
        }

        private void Update()
        {
            if (_frames == null || _frames.Length == 0) return;
            if (_finished && !_loop) return;

            _t += Time.deltaTime * Fps;
            int idx = Mathf.FloorToInt(_t);
            if (_loop)
            {
                idx %= _frames.Length;
            }
            else if (idx >= _frames.Length)
            {
                idx = _frames.Length - 1;
                _finished = true;
            }
            _sr.sprite = _frames[idx];
        }
    }
}
