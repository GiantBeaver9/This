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
            if (Set == null || !Set.Clips.TryGetValue(clip, out var frames) || frames == null || frames.Length == 0)
            {
                // Fall back to idle/first so we always show something.
                frames = Set?.FirstOf(clip) != null ? new[] { Set.FirstOf(clip) }
                       : Set?.First != null ? new[] { Set.First } : null;
                if (frames == null) return;
            }
            _clip = clip;
            bool reverse = ReverseAttackClips && (Set == null || Set.ReverseAttacks);
            _frames = (reverse && clip != null && clip.Contains("attack")) ? Reversed(frames) : frames;
            _loop = loop;
            _t = 0f;
            _finished = false;
            _sr.sprite = _frames[0];
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
