using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Gives a stage visible PROGRESSION — as you walk you pass distinct landmarks instead of the same
    /// repeating row (creator: "houses, then a school, then basketball courts, then the restaurant… a
    /// full intersection and lights. Make it pop"). Places big world-fixed set-piece sprites at fixed
    /// fractions along the lane at stage start; the tiled house row stays as the connective backdrop
    /// between them. Art lives in assets/backdrops/zones/ (graceful: missing files are skipped).
    /// Props clear + re-place on a stage change / teardown.
    /// </summary>
    public sealed class StageBackdropZones : MonoBehaviour
    {
        public float LaneLen = 1600f;

        private int _lastStage = -1;
        private readonly List<GameObject> _props = new();

        private struct Zone
        {
            public float Frac;   // position along the lane (0..1)
            public string File;
            public float Tall;   // world-units tall
            public float Z;      // depth row
            public bool Foreground;
        }

        private const float Far = 5.7f; // horizon row for buildings

        // Per-stage landmark layout. Stage 1 (index 0): school -> intersection (crosswalk + lights) ->
        // basketball courts -> diner, laid over the house row.
        private static Zone[] ZonesFor(int stage) => stage switch
        {
            0 => new[]
            {
                new Zone { Frac = 0.28f, File = "school.png",           Tall = 16f, Z = Far,  Foreground = false },
                new Zone { Frac = 0.48f, File = "crosswalk.png",        Tall = 1.2f, Z = 2.6f, Foreground = false }, // stripes on the road
                new Zone { Frac = 0.47f, File = "traffic_light.png",    Tall = 6f,  Z = 0.6f, Foreground = true },   // near light
                new Zone { Frac = 0.50f, File = "traffic_light.png",    Tall = 5f,  Z = Far,  Foreground = false },  // far light
                new Zone { Frac = 0.63f, File = "basketball_court.png", Tall = 9f,  Z = Far,  Foreground = false },
                new Zone { Frac = 0.85f, File = "diner.png",            Tall = 12f, Z = Far,  Foreground = false },
            },
            _ => System.Array.Empty<Zone>(),
        };

        private void Update()
        {
            var p = PlayerController.Primary;
            if (p == null) return;
            int stage = CampaignRunner.Instance != null ? CampaignRunner.Instance.CurrentStage : -1;
            if (stage == _lastStage) return;

            Clear();
            _lastStage = stage;
            float startX = p.WorldX;
            foreach (var z in ZonesFor(stage))
                Place(z.File, startX + z.Frac * LaneLen, z.Tall, z.Z, z.Foreground);
        }

        private void OnDisable() => Clear();

        private void Clear()
        {
            foreach (var g in _props) if (g != null) Destroy(g);
            _props.Clear();
        }

        private void Place(string file, float worldX, float tallWu, float z, bool foreground)
        {
            var spr = LoadZone(file);
            if (spr == null) return;
            var go = new GameObject("zone_" + file);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            float natTall = spr.rect.height / Tuning.PixelsPerUnit;
            float scale = natTall > 0.01f ? tallWu / natTall : 1f;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            Playfield.Place(go.transform, worldX, z, sr);              // world-fixed: the camera scrolls to it
            // Buildings sit far behind the fighters; a crosswalk lies on the road (still behind actors);
            // a traffic light is a near foreground pole (in front).
            sr.sortingOrder = foreground ? Playfield.SortingOrder(z) + 3 : (file.Contains("crosswalk") ? -20 : -300);
            _props.Add(go);
        }

        private static readonly Dictionary<string, Sprite> s_cache = new();
        private static Sprite LoadZone(string file)
        {
            if (s_cache.TryGetValue(file, out var c)) return c;
            Sprite s = null;
            try
            {
                string path = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "backdrops", "zones", file);
                if (System.IO.File.Exists(path))
                {
                    var t = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    t.LoadImage(System.IO.File.ReadAllBytes(path)); t.Apply();
                    s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0f), Tuning.PixelsPerUnit);
                }
            }
            catch { s = null; }
            s_cache[file] = s;
            return s;
        }
    }
}
