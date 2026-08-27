using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Makes campaign PROGRESSION visible (creator's #1 "feels endless" complaint).
    /// The gating logic already lives in <see cref="StageDirector"/> — it hard-locks the
    /// camera at a gate until the field clears, then scrolls on — but the player never
    /// SEES it. This paints that state:
    ///   * a flashing BARRICADE planted at the current camera wall while enemies remain
    ///     ("clear the area to pass"),
    ///   * a big "GO →" sweep the instant the wall opens (a wave cleared),
    ///   * a brief STAGE banner on entry and a "STAGE CLEAR!" flash on the last gate.
    /// It is autonomous: it watches the <see cref="CameraRig"/> wall + live-enemy count,
    /// so the director only calls <see cref="Banner"/> for the entry/clear text. Active
    /// during the campaign only (harmless in Endless/tutorial — no rig gate to show).
    /// </summary>
    public sealed class StageMarkers : MonoBehaviour
    {
        private static StageMarkers _inst;

        /// <summary>Find or create the persistent markers object.</summary>
        public static StageMarkers Ensure()
        {
            if (_inst == null)
            {
                var go = new GameObject("StageMarkers");
                _inst = go.AddComponent<StageMarkers>();
            }
            return _inst;
        }

        /// <summary>Flash a centered banner for <paramref name="seconds"/> (stage entry / clear).</summary>
        public static void Banner(string text, float seconds = 2.2f)
        {
            var m = Ensure();
            m._banner = text;
            m._bannerT = seconds;
        }

        // ---- barricade (world-space) --------------------------------------------
        private CameraRig _rig;
        private SpriteRenderer _post;
        private const float PostZ = 1.4f;          // plant it near the front for a clean read
        private float _prevMaxX = float.NaN;
        private bool _hadWall;

        // ---- overlay (screen-space) ---------------------------------------------
        private float _goT;                        // "GO →" flash timer
        private string _banner;
        private float _bannerT;
        private string _shownLabel;                // last StageLabel we auto-bannered
        private GUIStyle _goStyle, _bannerStyle, _hintStyle;

        private void Awake()
        {
            // Only destroy the duplicate COMPONENT — this may share a GameObject with
            // CampaignRunner, so never Destroy(gameObject) here.
            if (_inst != null && _inst != this) { Destroy(this); return; }
            _inst = this;
        }

        private void OnDestroy() { if (_inst == this) _inst = null; }

        private void EnsurePost()
        {
            if (_post != null) return;
            var go = new GameObject("stage_barricade");
            go.transform.SetParent(transform, false);
            _post = go.AddComponent<SpriteRenderer>();
            _post.sprite = BarricadeSprite();
            _post.enabled = false;
        }

        private void LateUpdate()
        {
            // Campaign-only: no runner → nothing to gate.
            bool campaign = CampaignRunner.Instance != null;
            if (_rig == null) _rig = FindAnyObjectByType<CameraRig>();

            // Auto-banner when the director publishes a new stage label.
            if (campaign && EnemySpawner.StageLabel != _shownLabel && !string.IsNullOrEmpty(EnemySpawner.StageLabel))
            {
                _shownLabel = EnemySpawner.StageLabel;
                Banner(_shownLabel, 2.4f);
            }

            if (!campaign || _rig == null) { if (_post != null) _post.enabled = false; return; }

            EnsurePost();

            int enemies = CountLiveEnemies();
            bool walled = enemies > 0;             // a wave is holding the wall

            // Wall just OPENED (was holding, now clear): the road ahead unlocked → GO.
            if (!float.IsNaN(_prevMaxX) && _rig.MaxX > _prevMaxX + 0.25f && _hadWall && !walled)
                _goT = 1.4f;
            _prevMaxX = _rig.MaxX;
            _hadWall = walled;

            // The visible barricade POST is removed (creator: "get rid of the pillar" — it read as a
            // red/black striped pillar planted at the camera wall). The camera lock still walls the
            // player; the "GO →" flash (OnGUI) still signals when a wave clears.
            if (_post != null) _post.enabled = false;
        }

        private static int CountLiveEnemies()
        {
            int n = 0;
            foreach (var a in Actor.All)
                if (a != null && a.Alive && a.Team == Team.Enemy) n++;
            return n;
        }

        private void OnGUI()
        {
            if (_goT <= 0f && _bannerT <= 0f) return;

            float scale = Screen.height / 360f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            float w = Screen.width / scale;

            _goStyle ??= new GUIStyle(GUI.skin.label)
            { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _bannerStyle ??= new GUIStyle(GUI.skin.label)
            { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };

            // Entry / clear banner, top-center.
            if (_bannerT > 0f)
            {
                _bannerT -= Time.unscaledDeltaTime;
                float a = Mathf.Clamp01(_bannerT);            // fade out over the last second
                Shadowed(new Rect(0, 40, w, 30), _banner ?? "", _bannerStyle, new Color(1f, 0.95f, 0.5f, a));
            }

            // "GO →" sweep, center — pulses and drifts right as the wall opens.
            if (_goT > 0f)
            {
                _goT -= Time.unscaledDeltaTime;
                float k = 1.4f - _goT;                        // 0 → 1.4 over the flash
                float a = Mathf.Clamp01(_goT / 0.6f);
                float drift = Mathf.Clamp01(k / 1.4f) * 60f;
                Shadowed(new Rect(w * 0.5f - 120 + drift, 120, 240, 50), "GO  →", _goStyle,
                         new Color(0.4f, 1f, 0.5f, a));
            }
        }

        private static void Shadowed(Rect r, string text, GUIStyle style, Color color)
        {
            var shadow = new Rect(r.x + 2, r.y + 2, r.width, r.height);
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, color.a * 0.6f);
            GUI.Label(shadow, text, style);
            GUI.color = color;
            GUI.Label(r, text, style);
            GUI.color = prev;
        }

        // ---- procedural barricade art (hazard-striped post) ---------------------
        private static Sprite _bar;
        private static Sprite BarricadeSprite()
        {
            if (_bar != null) return _bar;
            const int W = 16, H = 64;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    // Diagonal yellow/black hazard stripes; edges slightly darker for a "post" read.
                    bool stripe = (((x + y) / 5) & 1) == 0;
                    Color32 c = stripe ? new Color32(250, 210, 40, 255) : new Color32(30, 30, 34, 255);
                    if (x == 0 || x == W - 1) c = new Color32(20, 20, 24, 255);
                    px[y * W + x] = c;
                }
            tex.SetPixels32(px); tex.Apply();
            _bar = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0f), 12f); // pivot at the foot
            return _bar;
        }
    }
}
