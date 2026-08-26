using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Plays a between-stage vignette (VIGNETTES.md story-beat interstitials): a
    /// full-screen IMGUI panel — dim background, the beat's text, a "press any key /
    /// Enter to continue" prompt — that advances beat-by-beat on input, fires each
    /// beat's audio cue, and calls back when finished (STORY.md §5, delivery vehicle
    /// "3–5 s stage vignette" + the rooftop monologue). Matches the IMGUI-overlay
    /// style of <see cref="Hud"/>.
    ///
    /// Gameplay time is frozen for the duration (<c>Time.timeScale = 0</c>, restored
    /// on finish); input is polled unscaled so the panel still advances while frozen.
    /// Self-initializing like <see cref="Music"/>/<see cref="Sfx"/>: the lead can call
    /// <see cref="Play(string, Action)"/> straight off <see cref="Instance"/> with no
    /// scene wiring. Content comes from <see cref="VignetteScripts.Catalog"/>.
    /// </summary>
    public sealed class VignettePlayer : MonoBehaviour
    {
        // Ignore input for this long (unscaled) after a beat first shows, so the key
        // that launched the vignette (or advanced the previous beat) can't bleed
        // through and skip the new beat on the same/next frame.
        private const float InputLockSeconds = 0.2f;

        private static VignettePlayer _instance;

        /// <summary>
        /// The shared player, lazily creating a hidden, scene-persistent host the
        /// first time it's touched (mirrors <see cref="Music"/>/<see cref="Sfx"/>).
        /// </summary>
        public static VignettePlayer Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var go = new GameObject("~ThisL.VignettePlayer");
                go.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<VignettePlayer>();
                return _instance;
            }
        }

        /// <summary>True while a vignette is on screen (gameplay frozen).</summary>
        public bool IsPlaying { get; private set; }

        private Vignette _current;
        private int _index;
        private Action _onDone;
        private float _prevTimeScale = 1f;
        private float _beatShownAtUnscaled;

        private readonly Dictionary<string, Texture2D> _imageCache = new();

        // Lazily-built IMGUI styles (created in OnGUI like Hud does).
        private GUIStyle _titleStyle, _bodyStyle, _promptStyle, _counterStyle;
        private Texture2D _dimTex;

        // ---- Public API ------------------------------------------------------

        /// <summary>
        /// Play the vignette registered under <paramref name="vignetteId"/> in
        /// <see cref="VignetteScripts.Catalog"/>. <paramref name="onDone"/> is invoked
        /// once, after the last beat is dismissed (or immediately if the id is unknown
        /// or empty), with gameplay time already restored. A no-op with a warning if a
        /// vignette is already playing.
        /// </summary>
        public void Play(string vignetteId, Action onDone)
        {
            if (IsPlaying)
            {
                Debug.LogWarning($"[VignettePlayer] Already playing '{_current?.Id}'; ignoring Play('{vignetteId}').");
                return;
            }

            var v = VignetteScripts.Catalog.Get(vignetteId);
            if (v == null)
            {
                Debug.LogWarning($"[VignettePlayer] Unknown vignette id '{vignetteId}'; skipping straight to onDone.");
                onDone?.Invoke();
                return;
            }

            PlayVignette(v, onDone);
        }

        /// <summary>
        /// Play an in-memory vignette directly (bypasses the catalog) — handy for
        /// tests or one-off beats the lead builds by hand.
        /// </summary>
        public void PlayVignette(Vignette vignette, Action onDone)
        {
            if (IsPlaying)
            {
                Debug.LogWarning($"[VignettePlayer] Already playing; ignoring PlayVignette('{vignette?.Id}').");
                return;
            }
            if (vignette == null || vignette.BeatCount == 0)
            {
                onDone?.Invoke();
                return;
            }

            _current = vignette;
            _onDone = onDone;
            _index = 0;
            IsPlaying = true;

            // Restore target must be a RUNNING speed. If a hit-stop (or any freeze) has
            // momentarily driven timeScale to ~0 as this vignette opens, capturing that 0
            // would make Finish() "restore" the game into a permanent freeze. Clamp to 1
            // whenever the captured scale isn't clearly running.
            _prevTimeScale = Time.timeScale > 0.1f ? Time.timeScale : 1f;
            Time.timeScale = 0f; // freeze gameplay while the panel is up

            ShowBeat(0);
        }

        /// <summary>Immediately end the current vignette (restores time, fires onDone).</summary>
        public void Skip()
        {
            if (IsPlaying) Finish();
        }

        // ---- Playback --------------------------------------------------------

        private void ShowBeat(int i)
        {
            _index = i;
            _beatShownAtUnscaled = Time.unscaledTime;
            FireCue(_current.Beats[i]);
        }

        private void Advance()
        {
            if (_index + 1 >= _current.BeatCount)
            {
                Finish();
                return;
            }
            ShowBeat(_index + 1);
        }

        private void Finish()
        {
            var cb = _onDone;

            IsPlaying = false;
            _current = null;
            _onDone = null;
            Time.timeScale = _prevTimeScale; // restore BEFORE the callback, so onDone runs at normal time

            // Invoke last; guard so a throwing callback can't leave us wedged.
            try { cb?.Invoke(); }
            catch (Exception e) { Debug.LogError($"[VignettePlayer] onDone threw: {e}"); }
        }

        private static void FireCue(VignetteBeat beat)
        {
            if (beat == null || string.IsNullOrEmpty(beat.Cue)) return;
            switch (beat.CueBank)
            {
                case VignetteCueBank.Stinger:    Music.Stinger(beat.Cue); break;
                case VignetteCueBank.StageMusic: Music.PlayStage(beat.Cue); break;
                case VignetteCueBank.Ambient:    Music.PlayAmbient(beat.Cue); break;
                case VignetteCueBank.Sfx:        Sfx.Play(beat.Cue); break;
                case VignetteCueBank.None:       break;
            }
        }

        private void Update()
        {
            if (!IsPlaying) return;

            // Unscaled so input still lands while Time.timeScale == 0.
            if (Time.unscaledTime - _beatShownAtUnscaled < InputLockSeconds) return;

            // "press any key / Enter to continue" — anyKeyDown covers keyboard + mouse.
            if (Input.anyKeyDown)
                Advance();
        }

        // ---- Rendering (IMGUI overlay, Hud-style) ----------------------------

        private void OnGUI()
        {
            if (!IsPlaying || _current == null) return;

            GUI.depth = -1000; // draw on top of the Hud and any other IMGUI overlay

            EnsureStyles();

            float scale = Screen.height / 360f; // same 360px design height as Hud
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            float w = Screen.width / scale;
            float h = 360f;

            var beat = _current.Beats[_index];

            // Optional still behind the dim (none exist yet — see _INTEGRATION.md).
            var img = ResolveImage(beat.ImageKey);
            if (img != null)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(0, 0, w, h), img, ScaleMode.ScaleAndCrop);
                GUI.color = new Color(0.03f, 0.03f, 0.05f, 0.45f); // lighter scrim over art
            }
            else
            {
                GUI.color = new Color(0.04f, 0.04f, 0.06f, 0.92f); // solid dim panel
            }
            GUI.DrawTexture(new Rect(0, 0, w, h), _dimTex);
            GUI.color = Color.white;

            // Title (small, warm, above the body).
            if (!string.IsNullOrEmpty(_current.Title))
                GUI.Label(new Rect(0, 40, w, 24), _current.Title, _titleStyle);

            // Body text — centered block occupying the middle band.
            string body = string.Join("\n", beat.Lines);
            GUI.Label(new Rect(w * 0.1f, 72, w * 0.8f, 210), body, _bodyStyle);

            // Advance prompt (bottom, gently pulsing).
            float pulse = 0.55f + 0.45f * Mathf.PingPong(Time.unscaledTime * 0.9f, 1f);
            _promptStyle.normal.textColor = new Color(0.85f, 0.85f, 0.9f, pulse);
            bool last = _index + 1 >= _current.BeatCount;
            GUI.Label(new Rect(0, h - 34, w, 22),
                last ? "▶ press any key / Enter to begin" : "▶ press any key / Enter to continue",
                _promptStyle);

            // Beat counter (bottom-right).
            GUI.Label(new Rect(w - 70, h - 22, 60, 18), $"{_index + 1} / {_current.BeatCount}", _counterStyle);
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null) return;

            _dimTex = Texture2D.whiteTexture;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
            };
            _titleStyle.normal.textColor = new Color(0.95f, 0.82f, 0.45f); // warm, homemade

            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
                richText = false,
            };
            _bodyStyle.normal.textColor = new Color(0.96f, 0.96f, 0.98f);

            _promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
            };

            _counterStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleRight,
            };
            _counterStyle.normal.textColor = new Color(0.6f, 0.6f, 0.65f);
        }

        // ---- Optional still-image loading (off disk, mirrors Backdrop) --------

        /// <summary>
        /// Resolve an <see cref="VignetteBeat.ImageKey"/> to a texture off disk, or
        /// null. Tries a couple of asset-tree locations; caches results (including
        /// misses as null). No bespoke vignette stills ship today, so this returns
        /// null for every campaign beat and the player draws a solid dim panel.
        /// </summary>
        private Texture2D ResolveImage(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (_imageCache.TryGetValue(key, out var cached)) return cached;

            Texture2D tex = null;
            try
            {
                string root = SpriteLibrary.AssetsRoot;
                string[] candidates =
                {
                    Path.Combine(root, "vignettes", key + ".png"),
                    Path.Combine(root, "backdrops", key, key + "_preview_640x360.png"),
                    Path.Combine(root, "backdrops", key + ".png"),
                };
                foreach (var path in candidates)
                {
                    if (!File.Exists(path)) continue;
                    var bytes = File.ReadAllBytes(path);
                    tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    tex.LoadImage(bytes);
                    tex.filterMode = FilterMode.Point;
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.Apply();
                    break;
                }
                if (tex == null)
                    Debug.LogWarning($"[VignettePlayer] No still found for image key '{key}'; using solid panel.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[VignettePlayer] Failed to load still '{key}': {e.Message}");
                tex = null;
            }

            _imageCache[key] = tex;
            return tex;
        }
    }
}
