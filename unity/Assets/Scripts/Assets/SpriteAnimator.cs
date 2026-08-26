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
                string oclip = clip == "idle" ? "idle"
                             : (clip.Contains("attack") || clip == "sweep" || clip == "dash") ? "swing"
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
