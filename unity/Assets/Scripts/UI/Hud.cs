using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Minimal placeholder HUD drawn with IMGUI (top band only — the bottom half
    /// stays sacred for the playfield, GAMEPLAY_LOOP §6). Health + special meter per
    /// player, then two ICONIC rows so the read is instant, not wordy: LIVES as a row
    /// of the hero's FACE (head crop of the idle sprite, like char-select in GameFlow)
    /// and the held ITEM as its pickup PICTURE (assets/sprites/weapons/&lt;kind&gt;/&lt;kind&gt;_pickup.png).
    /// A real uGUI HUD with the bespoke art (UI.md) replaces this.
    /// </summary>
    public sealed class Hud : MonoBehaviour
    {
        private GUIStyle _label;   // big (game over)
        private GUIStyle _cap;     // small bar caption

        private void OnGUI()
        {
            var all = PlayerController.All;
            if (all.Count == 0) return;

            _label ??= new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            _cap   ??= new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold };

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

                // Iconic row UNDER the bars. P2 mirrors to the right so its icons sit under its own bars.
                const float iconY = 48f;
                float cursor = bx;

                // LIVES as hero-face icons (shared pool, so only P1 draws it).
                if (!isP2)
                {
                    int lives = Mathf.Max(0, Lives.Count);
                    int show = Mathf.Min(lives, 6);            // cap the row; +N handles overflow
                    for (int k = 0; k < show; k++)
                    {
                        DrawFace(cursor, iconY, 18f, p);
                        cursor += 20f;
                    }
                    if (lives > show) { GUI.color = Color.white; GUI.Label(new Rect(cursor, iconY + 2f, 30f, 16f), $"+{lives - show}", _cap); cursor += 22f; }
                    cursor += 6f;
                }

                // Held ITEM as its pickup picture (skip fists — empty hands read as "no weapon").
                if (p.CurrentWeapon != null && !p.CurrentWeapon.IsFists)
                {
                    var wtex = WeaponIcon(p.CurrentWeapon.Kind);
                    if (wtex != null)
                    {
                        float ih = 20f, iw = ih * wtex.width / Mathf.Max(1, wtex.height);
                        float wx = isP2 ? (bx + 180f - iw) : cursor;   // P2: right-align under its bars
                        GUI.color = Color.white;
                        GUI.DrawTexture(new Rect(wx, iconY - 1f, iw, ih), wtex);
                    }
                    else // Gatling etc. with no pickup art: tiny label fallback
                    {
                        GUI.color = Color.white;
                        GUI.Label(new Rect(isP2 ? bx + 120f : cursor, iconY + 2f, 80f, 16f), p.CurrentWeapon.Kind.ToString(), _cap);
                    }
                }

                if (!p.Alive)
                    GUI.Label(new Rect(bx + 2, 66, 180, 18), "DOWN", _label);
            }

            // Game over only once EVERYONE is down and the pool is spent.
            if (!PlayerController.AnyAlive && Lives.Count <= 0)
                GUI.Label(new Rect(w / 2f - 60, 150, 200, 30), "GAME OVER", _label);
        }

        // Draw the hero's head (top crop of the idle sprite) as a small square life pip.
        private void DrawFace(float x, float y, float size, PlayerController p)
        {
            var c = p != null ? p.Character : null;
            var set = c != null ? SpriteLibrary.Load(c.SpriteDir, c.SpriteActor) : null;
            var sp = set != null ? (set.FirstOf("idle") ?? set.First) : null;
            if (sp == null || sp.texture == null)
            {
                GUI.color = new Color(0.3f, 0.85f, 0.3f);
                GUI.DrawTexture(new Rect(x, y, size, size), Texture2D.whiteTexture);
                GUI.color = Color.white;
                return;
            }
            var tex = sp.texture; var rr = sp.rect;
            const float frac = 0.42f;                          // top ~42% ≈ head + shoulders
            var uv = new Rect(rr.x / tex.width, (rr.y + rr.height * (1f - frac)) / tex.height,
                              rr.width / tex.width, rr.height * frac / tex.height);
            GUI.color = Color.white;
            GUI.DrawTextureWithTexCoords(new Rect(x, y, size, size), tex, uv);
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

        // --- weapon pickup icons: load assets/sprites/weapons/<kind>/<kind>_pickup.png once, cache. ---
        private static readonly Dictionary<WeaponKind, Texture2D> _wIcons = new();
        private static Texture2D WeaponIcon(WeaponKind kind)
        {
            if (_wIcons.TryGetValue(kind, out var cached)) return cached;
            Texture2D tex = null;
            try
            {
                string name = kind.ToString().ToLowerInvariant();
                string path = Path.Combine(SpriteLibrary.AssetsRoot, "sprites", "weapons", name, name + "_pickup.png");
                if (File.Exists(path))
                {
                    tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    tex.LoadImage(File.ReadAllBytes(path));
                    tex.Apply();
                }
            }
            catch { tex = null; }
            _wIcons[kind] = tex;   // cache the null too, so we don't retry the disk hit every frame
            return tex;
        }
    }
}
