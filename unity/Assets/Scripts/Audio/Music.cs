using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Streamed music + ambient beds (AUDIO.md §2 twelve stage loops, §3 nine boss
    /// cues, the title theme, §4 twelve ambient beds, and the five area stingers).
    /// Static and self-initializing: the first call creates a hidden,
    /// scene-persistent GameObject with a two-source crossfade pair for the main
    /// loop, a low-volume looping source for the ambient bed (AUDIO.md §6 priority
    /// 6, −18 dB under the mix), and a one-shot source for stingers played over the
    /// loop. Tracks resolve by filename stem across <c>assets/audio/music/**</c>,
    /// so a caller passes just the loop name (e.g. "a1_surfrock_opener",
    /// "phil_realized", "lincoln_birds_traffic"). Switching the main loop does a
    /// ~0.5 s linear crossfade. Missing files no-op silently.
    /// </summary>
    public static class Music
    {
        private const float MainVolume = 0.8f;
        private const float AmbientVolume = 0.13f;  // AUDIO.md §6: ambient beds ~ −18 dB under the mix
        private const float CrossfadeSeconds = 0.5f;

        private static bool _init;
        private static MusicRunner _runner;
        private static AudioSource _mainA, _mainB;  // crossfade pair
        private static AudioSource _ambient;
        private static AudioSource _stinger;
        private static bool _usingA = true;         // which of the pair currently carries the live loop
        private static readonly Dictionary<string, string> _paths = new();
        private static readonly Dictionary<string, AudioClip> _clips = new();

        // ---- Public API ------------------------------------------------------

        /// <summary>Start (or crossfade to) a looping stage loop, e.g. "a1_surfrock_opener".</summary>
        public static void PlayStage(string clipName) => SwitchMain(clipName);

        /// <summary>Start (or crossfade to) a looping boss cue, e.g. "burly" / "phil_realized".</summary>
        public static void PlayBoss(string clipName) => SwitchMain(clipName);

        /// <summary>Start (or crossfade to) the looping title theme.</summary>
        public static void PlayTitle() => SwitchMain("title_theme");

        /// <summary>Play a low-volume looping ambient bed under the music, e.g. "lincoln_birds_traffic".</summary>
        public static void PlayAmbient(string clipName)
        {
            EnsureInit();
            var clip = Resolve(clipName);
            if (clip == null) return;
            _ambient.clip = clip;
            _ambient.volume = AmbientVolume;
            _ambient.loop = true;
            _ambient.Play();
        }

        /// <summary>Fire a one-shot stinger over the running loop, e.g. "a1_stinger".</summary>
        public static void Stinger(string clipName)
        {
            EnsureInit();
            var clip = Resolve(clipName);
            if (clip == null) return;
            _stinger.PlayOneShot(clip, MainVolume);
        }

        /// <summary>Fade out and stop everything (main loop, ambient, stingers).</summary>
        public static void Stop()
        {
            if (!_init) return;
            _runner.FadeOutAll(CrossfadeSeconds);
        }

        // ---- Internals -------------------------------------------------------

        private static void SwitchMain(string clipName)
        {
            EnsureInit();
            var clip = Resolve(clipName);
            if (clip == null) return;

            var incoming = _usingA ? _mainB : _mainA;
            var outgoing = _usingA ? _mainA : _mainB;

            // Already playing this exact clip on the live source? Leave it be.
            if (outgoing.isPlaying && outgoing.clip == clip) return;

            incoming.clip = clip;
            incoming.loop = true;
            incoming.volume = 0f;
            incoming.pitch = 1f;
            incoming.Play();

            _usingA = !_usingA;
            _runner.Crossfade(incoming, outgoing, MainVolume, CrossfadeSeconds);
        }

        private static AudioClip Resolve(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (_clips.TryGetValue(name, out var cached)) return cached;

            if (!_paths.TryGetValue(name, out var path))
            {
                Debug.LogWarning($"[Music] Unknown track '{name}' (no file under assets/audio/music).");
                _clips[name] = null;
                return null;
            }
            var clip = WavLoader.Load(path);
            _clips[name] = clip;
            return clip;
        }

        private static void EnsureInit()
        {
            if (_init) return;
            _init = true;

            var go = new GameObject("~ThisL.Music");
            go.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(go);

            _mainA = MakeSource(go, loop: true);
            _mainB = MakeSource(go, loop: true);
            _ambient = MakeSource(go, loop: true);
            _stinger = MakeSource(go, loop: false);
            _runner = go.AddComponent<MusicRunner>();

            BuildIndex();
        }

        private static AudioSource MakeSource(GameObject go, bool loop)
        {
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = loop;
            s.spatialBlend = 0f; // 2D stream
            s.volume = 0f;
            return s;
        }

        /// <summary>Scan assets/audio/music once; map each filename stem to its absolute path.</summary>
        private static void BuildIndex()
        {
            try
            {
                string dir = Path.Combine(SpriteLibrary.AssetsRoot, "audio", "music");
                if (!Directory.Exists(dir))
                {
                    Debug.LogWarning($"[Music] Music folder not found: {dir}");
                    return;
                }
                foreach (var file in Directory.GetFiles(dir, "*.wav", SearchOption.AllDirectories))
                {
                    string key = Path.GetFileNameWithoutExtension(file);
                    if (!_paths.ContainsKey(key)) _paths[key] = file;
                }
                Debug.Log($"[Music] Indexed {_paths.Count} tracks from {dir}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Music] Failed to index music: {e.Message}");
            }
        }

        /// <summary>
        /// Hidden driver that runs the linear crossfades each frame — a static class
        /// can't own a coroutine, so this MonoBehaviour carries the fade state.
        /// </summary>
        private sealed class MusicRunner : MonoBehaviour
        {
            private AudioSource _fadeIn, _fadeOut;
            private float _targetIn, _startInVol, _startOutVol;
            private float _dur, _t;
            private bool _fading;

            private bool _fadingAll;
            private float _allDur, _allT;
            private AudioSource[] _all;

            public void Crossfade(AudioSource fadeIn, AudioSource fadeOut, float targetVol, float seconds)
            {
                _fadeIn = fadeIn;
                _fadeOut = fadeOut;
                _targetIn = targetVol;
                _startInVol = fadeIn != null ? fadeIn.volume : 0f;
                _startOutVol = fadeOut != null ? fadeOut.volume : 0f;
                _dur = Mathf.Max(0.0001f, seconds);
                _t = 0f;
                _fading = true;
                _fadingAll = false;
            }

            public void FadeOutAll(float seconds)
            {
                _all = GetComponents<AudioSource>();
                _allDur = Mathf.Max(0.0001f, seconds);
                _allT = 0f;
                _fadingAll = true;
                _fading = false;
            }

            private void Update()
            {
                float dt = Time.unscaledDeltaTime; // music mix should ignore time-slow

                if (_fading)
                {
                    _t += dt;
                    float k = Mathf.Clamp01(_t / _dur);
                    if (_fadeIn != null) _fadeIn.volume = Mathf.Lerp(_startInVol, _targetIn, k);
                    if (_fadeOut != null) _fadeOut.volume = Mathf.Lerp(_startOutVol, 0f, k);
                    if (k >= 1f)
                    {
                        if (_fadeOut != null) { _fadeOut.Stop(); _fadeOut.volume = 0f; }
                        _fading = false;
                    }
                }

                if (_fadingAll)
                {
                    _allT += dt;
                    float k = Mathf.Clamp01(_allT / _allDur);
                    if (_all != null)
                        foreach (var s in _all)
                            if (s != null) s.volume = Mathf.Lerp(s.volume, 0f, k);
                    if (k >= 1f)
                    {
                        if (_all != null)
                            foreach (var s in _all)
                                if (s != null) { s.Stop(); s.volume = 0f; }
                        _fadingAll = false;
                    }
                }
            }
        }
    }
}
