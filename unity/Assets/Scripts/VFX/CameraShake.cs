using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Decaying random screen-shake, per the LOCKED presets in VFX.md §8 /
    /// TUNING.md §2.6 (amplitudes given in px at the 640×360 internal res):
    ///   light  = 2 px / 0.10 s (weapon hits, dashes)
    ///   medium = 5 px / 0.15 s (knockdowns, big weapon impacts)
    ///   heavy  = 10 px / 0.20 s (finishers, explosions, ground-slams, Ball &amp; Chain).
    /// The Options "reduce screen-shake" toggle halves every amplitude (never
    /// fully disables) — set <see cref="ReduceShake"/> to honor it.
    ///
    /// Applied as an additive local offset on the main camera in LateUpdate,
    /// removed at the start of the next frame so it never accumulates. The
    /// component self-installs on Camera.main (or the MainCamera-tagged object)
    /// and is meant to run AFTER the camera's own follow (see the .meta
    /// executionOrder) so the shake survives the follow write.
    /// </summary>
    public sealed class CameraShake : MonoBehaviour
    {
        // Presets as (px, seconds); px -> world units via PixelsPerUnit.
        public static readonly Vector2 Light = new(2f, 0.10f);
        public static readonly Vector2 Medium = new(5f, 0.15f);
        public static readonly Vector2 Heavy = new(10f, 0.20f);

        /// <summary>Options toggle: halves all amplitudes (UI.md §5). Never fully disables.</summary>
        public static bool ReduceShake = false;

        private struct Impulse
        {
            public float MagnitudePx;   // starting amplitude in internal px
            public float Duration;
            public float Elapsed;
        }

        private static CameraShake _instance;

        private readonly List<Impulse> _impulses = new();
        private Vector3 _appliedOffset = Vector3.zero;

        /// <summary>Kick the camera with a shake impulse (magnitude in internal px).</summary>
        public static void Add(float magnitudePx, float duration)
        {
            if (magnitudePx <= 0f || duration <= 0f) return;
            var inst = Ensure();
            if (inst == null) return;
            if (ReduceShake) magnitudePx *= 0.5f;
            inst._impulses.Add(new Impulse { MagnitudePx = magnitudePx, Duration = duration, Elapsed = 0f });
        }

        /// <summary>Convenience overload taking a (px, seconds) preset.</summary>
        public static void Add(Vector2 preset) => Add(preset.x, preset.y);

        private static CameraShake Ensure()
        {
            if (_instance != null) return _instance;

            Camera cam = Camera.main;
            if (cam == null)
            {
                var tagged = GameObject.FindWithTag("MainCamera");
                if (tagged != null) cam = tagged.GetComponent<Camera>();
            }
            if (cam == null) return null;

            _instance = cam.GetComponent<CameraShake>();
            if (_instance == null) _instance = cam.gameObject.AddComponent<CameraShake>();
            return _instance;
        }

        private void OnDisable()
        {
            // Leave the transform clean if we get torn down mid-shake.
            transform.position -= _appliedOffset;
            _appliedOffset = Vector3.zero;
            if (_instance == this) _instance = null;
        }

        private void LateUpdate()
        {
            // Remove last frame's offset first so this is order-independent and
            // never permanently accumulates on the transform.
            transform.position -= _appliedOffset;
            _appliedOffset = Vector3.zero;

            if (_impulses.Count == 0) return;

            float pxToWu = 1f / Tuning.PixelsPerUnit;
            float amp = 0f; // combined current amplitude (px): strongest active impulse wins
            for (int i = _impulses.Count - 1; i >= 0; i--)
            {
                var imp = _impulses[i];
                imp.Elapsed += Time.unscaledDeltaTime; // shake even during hitstop/time-slow
                if (imp.Elapsed >= imp.Duration)
                {
                    _impulses.RemoveAt(i);
                    continue;
                }
                float k = 1f - (imp.Elapsed / imp.Duration); // linear decay
                float cur = imp.MagnitudePx * k;
                if (cur > amp) amp = cur;
                _impulses[i] = imp;
            }

            if (amp <= 0f) return;

            Vector2 dir = Random.insideUnitCircle;
            _appliedOffset = new Vector3(dir.x, dir.y, 0f) * (amp * pxToWu);
            transform.position += _appliedOffset;
        }
    }
}
