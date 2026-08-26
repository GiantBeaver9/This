using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Global hit-stop / freeze-frame (the "hits hit" juice, VFX.md §4 / TUNING §2.6).
    /// A short <see cref="Time.timeScale"/> dip, counted down on UNSCALED time and
    /// restored afterwards, so an impactful connect briefly freezes the whole field.
    ///
    /// Tier durations below are the JUICE-pass values requested by the lead. They are
    /// intentionally a touch stronger than TUNING §2.6's LOCKED table (which specifies
    /// 0f on plain normals, 3f finisher, 5f kill) so jabs get a hint of crunch — set
    /// <see cref="Jab"/>/<see cref="Normal"/> to 0f to restore the LOCKED "no hitstop on
    /// normals" rule. Finisher/kill/heavy stay aligned with the 3f/5f references.
    ///
    /// Coexistence with other time owners (CRITICAL): the sniper special runs at
    /// <c>Time.timeScale = 0.28</c> (SpecialSequences) and vignettes run at 0. This
    /// system NEVER engages while another owner already slowed/froze time (it only
    /// arms when time is running ~normal), and on release it only restores time if it
    /// still owns the freeze (timeScale is still 0), so it can never clobber the
    /// sniper slow-mo or a vignette that started mid-freeze. Overlapping freezes
    /// EXTEND (max), they don't stack/deadlock.
    /// </summary>
    public sealed class HitStop : MonoBehaviour
    {
        private const float F = 1f / 60f;

        // Per-tier freeze durations (seconds). Scale by attack per the lead's brief.
        public const float Jab      = 0.03f;      // light connect (LOCKED §2.6 = 0f; knob to taste)
        public const float Normal   = 0.04f;      // P2 / standard connect
        public const float Sweep    = 3f * F;     // ~0.05s — the crowd move
        public const float Finisher = 3f * F;     // ~0.05s — §2.6 non-killing finisher
        public const float Kill     = 5f * F;     // ~0.083s — §2.6 any kill (precedence)
        public const float Heavy    = 0.10f;      // ground-slam / big weapon / special payload
        public const float Special  = 0.12f;      // the biggest freeze (crowd-wipe specials)

        // Only arm when time is running roughly normal — leaves the sniper slow-mo
        // (0.28) and vignette freeze (0) untouched.
        private const float NormalScaleFloor = 0.9f;

        private static HitStop _instance;
        private bool _active;
        private float _timer;
        private float _restoreScale = 1f;
        private float _zeroTimeReal;   // realtime seconds timeScale has been pinned ~0 with no owner

        /// <summary>Freeze the field for <paramref name="seconds"/> (unscaled). No-op if
        /// another system already owns a slow/freeze, or on a non-positive duration.</summary>
        public static void Freeze(float seconds)
        {
            if (seconds <= 0f) return;
            var inst = Ensure();
            if (inst != null) inst.Engage(seconds);
        }

        private void Engage(float seconds)
        {
            // Don't fight another time-scale owner (sniper slow-mo / vignette).
            if (!_active && Time.timeScale < NormalScaleFloor) return;
            // Never hit-stop while a vignette owns time — it captures the current scale as
            // its restore target, so a hit-stop's 0 would get "restored" into a freeze.
            if (VignetteOwnsTime()) return;
            if (!_active) { _restoreScale = Time.timeScale; _active = true; }
            Time.timeScale = 0f;
            _timer = Mathf.Max(_timer, seconds); // extend, never stack
        }

        private void Update()
        {
            // Safety watchdog: the game must NEVER stay frozen. If timeScale is pinned
            // ~0 while no vignette owns it and we aren't mid hit-stop, some owner leaked
            // its freeze (e.g. a callback threw before restoring) — hand time back.
            if (!_active && Time.timeScale < 0.01f && !VignetteOwnsTime())
            {
                _zeroTimeReal += Time.unscaledDeltaTime;
                if (_zeroTimeReal > 1.5f) { Time.timeScale = 1f; _zeroTimeReal = 0f; }
            }
            else _zeroTimeReal = 0f;

            if (!_active) return;
            _timer -= Time.unscaledDeltaTime;
            if (_timer > 0f) return;

            // Hand time back only if we still own the freeze AND no vignette has since
            // taken over time (if one opened during our freeze it now owns timeScale and
            // will restore it itself — us writing here would un-freeze its panel).
            if (Time.timeScale == 0f && !VignetteOwnsTime()) Time.timeScale = _restoreScale;
            _active = false;
        }

        /// <summary>True while a vignette panel is up (it drives Time.timeScale to 0 itself).</summary>
        private static bool VignetteOwnsTime()
            => VignettePlayer.Instance != null && VignettePlayer.Instance.IsPlaying;

        private void OnDisable()
        {
            // Never leave the game frozen if we get torn down mid-freeze.
            if (_active && Time.timeScale == 0f) Time.timeScale = _restoreScale;
            _active = false;
            if (_instance == this) _instance = null;
        }

        private static HitStop Ensure()
        {
            if (_instance != null) return _instance;
            var go = new GameObject("HitStop");
            Object.DontDestroyOnLoad(go);
            _instance = go.AddComponent<HitStop>();
            return _instance;
        }
    }
}
