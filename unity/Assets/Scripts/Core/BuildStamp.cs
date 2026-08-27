using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Tiny corner label stamping the current code build, so it's obvious at a glance whether the
    /// running game is FRESH or stale (Unity sometimes plays a cached assembly until a recompile).
    /// Bump <see cref="Tag"/> whenever a change should be visibly confirmable in-game.
    /// </summary>
    public sealed class BuildStamp : MonoBehaviour
    {
        // Bump this string on meaningful gameplay changes so the creator can confirm a fresh build.
        public const string Tag = "traversal+fade+bosses";

        private GUIStyle _style;

        private void OnGUI()
        {
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.LowerRight };
            _style.normal.textColor = new Color(1f, 1f, 1f, 0.55f);
            var r = new Rect(Screen.width - 220, Screen.height - 18, 214, 16);
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), "build: " + Tag, _style);
            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            GUI.Label(r, "build: " + Tag, _style);
            GUI.color = prev;
        }
    }
}
