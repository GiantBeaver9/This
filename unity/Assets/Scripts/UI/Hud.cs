using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Minimal placeholder HUD drawn with IMGUI (top band only — the bottom half
    /// stays sacred for the playfield, GAMEPLAY_LOOP §6). Just health top-left and
    /// the special meter under it with tier colour (creator ruling: "UI is
    /// unintuitive, just health and special meter"). A real uGUI HUD with the
    /// bespoke art (UI.md) replaces this.
    /// </summary>
    public sealed class Hud : MonoBehaviour
    {
        private GUIStyle _label;

        private void OnGUI()
        {
            var p = PlayerController.Instance;
            if (p == null) return;

            _label ??= new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };

            float scale = Screen.height / 360f; // scale HUD to the 360px design height
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

            // Health bar
            DrawBar(12, 12, 180, 16, p.Hp / p.MaxHp, HealthColor(p.Hp / p.MaxHp), "HP");

            // Special meter (0..300, tier-coloured)
            float frac = p.Meter.Fraction01;
            DrawBar(12, 36, 180, 12, frac, MeterColor(p.Meter.FullTier), $"SPECIAL {(p.Meter.CanFire ? "ARMED" : "")}");

            if (!p.Alive)
                GUI.Label(new Rect(Screen.width / scale / 2f - 60, 150, 200, 30), "YOU DIED", _label);
        }

        private void DrawBar(float x, float y, float w, float h, float frac, Color fill, string caption)
        {
            frac = Mathf.Clamp01(frac);
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(x - 2, y - 2, w + 4, h + 4), Texture2D.whiteTexture);
            GUI.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = fill;
            GUI.DrawTexture(new Rect(x, y, w * frac, h), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(x + 4, y - 1, w, h + 2), caption);
        }

        private static Color HealthColor(float f) =>
            f > 0.5f ? new Color(0.3f, 0.85f, 0.3f) : f > 0.25f ? new Color(0.9f, 0.8f, 0.2f) : new Color(0.9f, 0.25f, 0.2f);

        private static Color MeterColor(int tier) => tier switch
        {
            3 => new Color(0.3f, 0.9f, 0.4f),   // green
            2 => new Color(0.35f, 0.55f, 0.95f), // blue
            1 => new Color(0.95f, 0.85f, 0.25f), // yellow
            _ => new Color(0.6f, 0.6f, 0.6f),
        };
    }
}
