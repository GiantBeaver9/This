using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Floating "N HIT!" combo popup + kill-milestone flash (juice pass). The base
    /// <see cref="Hud"/> no longer shows the combo count (creator ruling: HUD is just
    /// health + special), so this is a separate, self-installing IMGUI overlay that
    /// pops and fades as <see cref="SpecialMeter.Combo"/> climbs, escalating its size
    /// and colour at the 5 / 10 / 15+ meter tiers. It owns no gameplay state — it only
    /// reads <see cref="PlayerController.Instance"/> — and self-installs so it needs no
    /// wiring in the Core bootstrap (which this pass doesn't own).
    ///
    /// All timers run on UNSCALED time, so the popup keeps animating through hit-stop.
    /// </summary>
    public sealed class ComboHud : MonoBehaviour
    {
        // ---- self-install (no Core edits) -----------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (_instance != null) return;
            var go = new GameObject("ComboHud");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<ComboHud>();
        }

        private static ComboHud _instance;

        // ---- per-player combo popup state (0 = P1 left/warm, 1 = P2 right/blue) ----
        private readonly int[] _lastCombo = new int[2];
        private readonly int[] _shownCombo = new int[2];
        private readonly float[] _pop = new float[2];
        private readonly float[] _hold = new float[2];
        private readonly float[] _fade = new float[2];

        private const float HoldTime = 0.9f;
        private const float FadeTime = 0.35f;

        // ---- kill-milestone flash -------------------------------------------
        private static int _kills;
        private float _flash;       // 0..1 screen-flash intensity, decays
        private int _flashMilestone;
        private const int MilestoneEvery = 10;

        private GUIStyle _style;

        /// <summary>Count a player kill; every 10th fires a brief screen flash + banner.</summary>
        public static void RegisterKill()
        {
            _kills++;
            if (_instance != null && _kills % MilestoneEvery == 0)
            {
                _instance._flash = 1f;
                _instance._flashMilestone = _kills;
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            var all = PlayerController.All;
            for (int i = 0; i < 2; i++)
            {
                int combo = (i < all.Count && all[i] != null) ? all[i].Meter.Combo : 0;
                if (combo > _lastCombo[i] && combo >= 2)
                {
                    _shownCombo[i] = combo; _pop[i] = 1f; _hold[i] = HoldTime; _fade[i] = 0f;
                }
                else if (combo == 0 && _lastCombo[i] > 0)
                {
                    _hold[i] = 0f; // streak ended — fade out
                }
                _lastCombo[i] = combo;

                _pop[i] = Mathf.Max(0f, _pop[i] - dt * 6f);
                if (_hold[i] > 0f) _hold[i] -= dt;
                else if (_shownCombo[i] >= 2) _fade[i] = Mathf.Min(1f, _fade[i] + dt / FadeTime);
                if (_fade[i] >= 1f) _shownCombo[i] = 0;
            }

            if (_flash > 0f) _flash = Mathf.Max(0f, _flash - dt * 1.6f);
        }

        private void OnGUI()
        {
            float s = Screen.height / 360f;

            // Kill-milestone flash: a quick white wash + banner across the top band.
            if (_flash > 0f)
            {
                var prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.35f * _flash);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                // Keep the screen-flash JUICE but no words (creator: "just the combos, no
                // 'execute'/'down' text") — the combo "N HIT!" popup below is the only text.
                GUI.color = prev;
            }

            // Per-player combo popups — P1 LEFT (warm/yellow), P2 RIGHT (blue), kept
            // visually separate (creator ruling). Each escalates size with its streak.
            for (int i = 0; i < 2; i++)
            {
                if (_shownCombo[i] < 2) continue;
                int combo = _shownCombo[i];
                int baseSize = combo >= 15 ? 40 : combo >= 10 ? 32 : combo >= 5 ? 26 : 22;
                float kick = 1f + _pop[i] * 0.6f;             // scale punch on each fresh hit
                int size = Mathf.RoundToInt(baseSize * s * kick);
                bool p1 = i == 0;
                Color col = p1
                    ? (combo >= 15 ? new Color(1f, 0.45f, 0.2f)   : combo >= 10 ? new Color(1f, 0.55f, 0.15f)
                      : combo >= 5  ? new Color(1f, 0.85f, 0.25f) : new Color(0.98f, 0.9f, 0.6f))   // P1 warm
                    : (combo >= 15 ? new Color(0.3f, 0.5f, 1f)    : combo >= 10 ? new Color(0.35f, 0.62f, 1f)
                      : combo >= 5  ? new Color(0.45f, 0.72f, 1f) : new Color(0.7f, 0.85f, 1f));    // P2 blue
                col.a = 1f - _fade[i];
                float y = Screen.height * 0.30f;

                var prevC = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, col.a * 0.7f);   // drop shadow
                DrawSide($"{combo} HIT!", y + 2f, size, p1);
                GUI.color = col;
                DrawSide($"{combo} HIT!", y, size, p1);
                GUI.color = prevC;
            }
        }

        // P1 = left half (left-aligned), P2 = right half (right-aligned).
        private void DrawSide(string text, float y, int fontSize, bool left)
        {
            _style ??= new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            _style.fontSize = fontSize;
            _style.alignment = left ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            _style.normal.textColor = GUI.color;
            const float pad = 30f;
            var r = left ? new Rect(pad, y, Screen.width * 0.5f - pad, fontSize + 10)
                         : new Rect(Screen.width * 0.5f, y, Screen.width * 0.5f - pad, fontSize + 10);
            GUI.Label(r, text, _style);
        }

        private void OnDisable() { if (_instance == this) _instance = null; }
    }
}
