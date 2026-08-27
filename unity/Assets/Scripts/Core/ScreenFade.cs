using System.Collections;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Full-screen black fade for scene/stage transitions (creator: "fight the boss, then it fades
    /// out — I can do VO for that"). Draws a black quad over everything via OnGUI (unscaled time, so
    /// it works even while HitStop has frozen Time.timeScale). Call <see cref="FadeOutIn"/> to fade
    /// to black, run a callback at full black (swap the stage), hold for a beat (VO room), then fade
    /// back in. A lightweight singleton — created on demand.
    /// </summary>
    public sealed class ScreenFade : MonoBehaviour
    {
        private static ScreenFade _inst;
        private float _alpha;
        private Texture2D _tex;
        private bool _busy;

        public static bool Busy => _inst != null && _inst._busy;

        public static ScreenFade Ensure()
        {
            if (_inst == null)
            {
                var go = new GameObject("ScreenFade");
                DontDestroyOnLoad(go);
                _inst = go.AddComponent<ScreenFade>();
            }
            return _inst;
        }

        private void Awake()
        {
            if (_inst != null && _inst != this) { Destroy(this); return; }
            _inst = this;
            _tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _tex.SetPixel(0, 0, Color.black); _tex.Apply();
        }

        private void OnDestroy() { if (_inst == this) _inst = null; }

        /// <summary>Fade to black over <paramref name="outDur"/>, invoke <paramref name="atBlack"/> at full
        /// black, hold <paramref name="holdDur"/> (a window for VO), then fade back in over <paramref name="inDur"/>.</summary>
        public static void FadeOutIn(float outDur, float holdDur, float inDur, System.Action atBlack)
        {
            var f = Ensure();
            f.StopAllCoroutines();
            f.StartCoroutine(f.Run(outDur, holdDur, inDur, atBlack));
        }

        /// <summary>Just fade to black and hold there (caller controls the rest).</summary>
        public static void FadeToBlack(float dur) { var f = Ensure(); f.StopAllCoroutines(); f.StartCoroutine(f.To(1f, dur)); }
        /// <summary>Fade from black back to clear.</summary>
        public static void FadeIn(float dur) { var f = Ensure(); f.StopAllCoroutines(); f.StartCoroutine(f.To(0f, dur)); }

        private IEnumerator Run(float outDur, float holdDur, float inDur, System.Action atBlack)
        {
            _busy = true;
            yield return To(1f, outDur);
            atBlack?.Invoke();
            float t = holdDur;
            while (t > 0f) { t -= Time.unscaledDeltaTime; yield return null; }
            yield return To(0f, inDur);
            _busy = false;
        }

        private IEnumerator To(float target, float dur)
        {
            float start = _alpha;
            if (dur <= 0f) { _alpha = target; yield break; }
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                _alpha = Mathf.Lerp(start, target, Mathf.Clamp01(t / dur));
                yield return null;
            }
            _alpha = target;
        }

        private void OnGUI()
        {
            if (_alpha <= 0.001f || _tex == null) return;
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, _alpha);
            GUI.depth = -1000;                                  // in front of everything
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _tex);
            GUI.color = prev;
        }
    }
}
