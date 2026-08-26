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
        private GUIStyle _label;   // big (game over)
        private GUIStyle _cap;     // small bar caption
        private GUIStyle _info;    // small info line (lives / weapon)

        private void OnGUI()
        {
            var all = PlayerController.All;
            if (all.Count == 0) return;

            _label ??= new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            _cap   ??= new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold };
            _info  ??= new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Bold };

            float scale = Screen.height / 360f; // scale HUD to the 360px design height
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            float w = Screen.width / scale;

            // One HP + special block per player: P1 top-left, P2 mirrored top-right.
            for (int i = 0; i < all.Count; i++)
            {
                var p = all[i];
                if (p == null) continue;
                bool isP2 = i >= 1;
                float bx = isP2 ? (w - 192f) : 12f;

                DrawBar(bx, 12, 180, 16, p.Hp / p.MaxHp, HealthColor(p.Hp / p.MaxHp), isP2 ? "P2" : "P1");
                DrawBar(bx, 32, 180, 11, p.Meter.Fraction01, MeterColor(p.Meter.FullTier),
                        p.Meter.CanFire ? "SPECIAL — ARMED" : "SPECIAL");

                // Info line UNDER the counters: LIVES (shared, under P1 only) + the currently-held item.
                string wpn = p.CurrentWeapon == null || p.CurrentWeapon.IsFists ? "FISTS" : p.CurrentWeapon.Kind.ToString();
                string livesTag = isP2 ? "" : $"LIVES {Mathf.Max(0, Lives.Count)}    ";
                GUI.Label(new Rect(bx + 2, 46, 190, 16), $"{livesTag}ITEM: {wpn}", _info);
                if (!p.Alive)
                    GUI.Label(new Rect(bx + 2, 60, 180, 18), "DOWN", _label);
            }

            // Game over only once EVERYONE is down and the pool is spent.
            if (!PlayerController.AnyAlive && Lives.Count <= 0)
                GUI.Label(new Rect(w / 2f - 60, 150, 200, 30), "GAME OVER", _label);
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
            GUI.Label(new Rect(x + 4, y - 2, w, h + 3), caption, _cap);
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
