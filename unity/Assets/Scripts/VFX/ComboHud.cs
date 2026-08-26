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

        // ---- combo popup state ----------------------------------------------
        private int _lastCombo;
        private int _shownCombo;
        private float _pop;         // 0..1 punch that decays (scale kick on each hit)
        private float _hold;        // seconds the current popup stays fully visible
        private float _fade;        // 0..1 fade-out progress once the streak ends

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
            var p = PlayerController.Instance;
            int combo = p != null ? p.Meter.Combo : 0;

            if (combo > _lastCombo && combo >= 2)
            {
                _shownCombo = combo;
                _pop = 1f;
                _hold = HoldTime;
                _fade = 0f;
            }
            else if (combo == 0 && _lastCombo > 0)
            {
                // streak ended — let the last popup fade out.
                _hold = 0f;
            }
            _lastCombo = combo;

            _pop = Mathf.Max(0f, _pop - dt * 6f);
            if (_hold > 0f) _hold -= dt;
            else if (_shownCombo >= 2) _fade = Mathf.Min(1f, _fade + dt / FadeTime);
            if (_fade >= 1f) _shownCombo = 0;

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
                GUI.color = new Color(1f, 0.95f, 0.5f, _flash);
                DrawCentered($"{_flashMilestone} DOWN!", Screen.height * 0.24f, Mathf.RoundToInt(34 * s));
                GUI.color = prev;
            }

            if (_shownCombo < 2) return;

            // Escalate size + colour with the streak (mirrors the meter tiers).
            int combo = _shownCombo;
            int baseSize = combo >= 15 ? 40 : combo >= 10 ? 32 : combo >= 5 ? 26 : 22;
            float kick = 1f + _pop * 0.6f;             // scale punch on each fresh hit
            int size = Mathf.RoundToInt(baseSize * s * kick);
            Color col = combo >= 15 ? new Color(1f, 0.45f, 0.2f)   // fiery orange
                      : combo >= 10 ? new Color(1f, 0.55f, 0.15f)  // orange
                      : combo >= 5  ? new Color(1f, 0.85f, 0.25f)  // gold
                      :               new Color(0.95f, 0.95f, 0.95f); // white
            col.a = 1f - _fade;

            var prevC = GUI.color;
            // drop shadow for legibility over the busy field
            GUI.color = new Color(0f, 0f, 0f, col.a * 0.7f);
            DrawCentered($"{combo} HIT!", Screen.height * 0.30f + 2f, size);
            GUI.color = col;
            DrawCentered($"{combo} HIT!", Screen.height * 0.30f, size);
            GUI.color = prevC;
        }

        private void DrawCentered(string text, float y, int fontSize)
        {
            _style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            _style.fontSize = fontSize;
            _style.normal.textColor = GUI.color;
            GUI.Label(new Rect(0, y, Screen.width, fontSize + 8), text, _style);
        }

        private void OnDisable() { if (_instance == this) _instance = null; }
    }
}
