using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Drops the one-off set-piece props for a stage's FINALE. Right now: Stage 2 ends by crossing a
    /// RAILROAD, then reaching the SANDWICH BROS store — you fight the last wave in front of it
    /// (creator: "sandwich bros should be the backdrop, the store, you fight in front of it"). The
    /// store is a big world-fixed building near the lane end; the crossing sits a bit before it.
    /// Graceful: does nothing if the art is missing. Props clear on a stage change / teardown.
    /// </summary>
    public sealed class StageFinaleProps : MonoBehaviour
    {
        private const int SandwichBrosStage = 0; // Stage 1 / index 0 — the neighborhood culminates here
                                                 // (railroad → Sandwich Bros store → big-charger boss).
        // The finale anchors to the ACTUAL running lane length (StageDirector publishes it), not a
        // hardcoded 1600 — otherwise the store landed at 1600 and "the end was there immediately"
        // once the lane grew. Falls back to 1600 if no stage is running yet.
        private float LaneLen => StageDirector.ActiveLaneLengthWu;

        private int _lastStage = -1;
        private float _startX;
        private bool _placed;
        private readonly List<GameObject> _props = new();

        private void Update()
        {
            var p = PlayerController.Primary;
            if (p == null || !p.Alive) return;

            int stage = CampaignRunner.Instance != null ? CampaignRunner.Instance.CurrentStage : -1;
            if (stage != _lastStage) { Clear(); _lastStage = stage; _startX = p.WorldX; _placed = false; }
            if (_placed || stage != SandwichBrosStage) return;

            float endX = _startX + LaneLen;
            if (p.WorldX >= endX - 120f)   // once you're approaching the finale
            {
                // Railroad crossing removed (creator: "get rid of the train track thing") — it read as a
                // red/black striped beam. Just the store now.
                // The store is HUGE (~3x) so the finale reads as its parking lot — you fight at its base.
                Place("sandwich_bros_store.png", endX - 8f,  25f,  foreground: false);  // the big store
                _placed = true;
            }
        }

        private void OnDisable() => Clear();

        private void Clear()
        {
            foreach (var g in _props) if (g != null) Destroy(g);
            _props.Clear();
        }

        private void Place(string file, float worldX, float targetTallWu, bool foreground)
        {
            var spr = LoadFinale(file);
            if (spr == null) return;
            var go = new GameObject("finale_" + file);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            float natTall = spr.rect.height / Tuning.PixelsPerUnit;
            float scale = natTall > 0.01f ? targetTallWu / natTall : 1f;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            // Store = a far background building (behind the fighters); crossing = a near foreground marker.
            float z = foreground ? 0.5f : Tuning.ZBandDepth - 0.3f;
            Playfield.Place(go.transform, worldX, z, sr);           // world-fixed: the camera scrolls up to it
            sr.sortingOrder = foreground ? Playfield.SortingOrder(z) + 3 : -300;
            _props.Add(go);
        }

        // ---- Art (assets/backdrops/finale/*.png) ----------------------------------
        private static readonly Dictionary<string, Sprite> s_cache = new();
        private static Sprite LoadFinale(string file)
        {
            if (s_cache.TryGetValue(file, out var c)) return c;
            Sprite s = null;
            try
            {
                string path = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "backdrops", "finale", file);
                if (System.IO.File.Exists(path))
                {
                    var t = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    t.LoadImage(System.IO.File.ReadAllBytes(path)); t.Apply();
                    s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0f), Tuning.PixelsPerUnit); // bottom-centre
                }
            }
            catch { s = null; }
            s_cache[file] = s;
            return s;
        }
    }
}
