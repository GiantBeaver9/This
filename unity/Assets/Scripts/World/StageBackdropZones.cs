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
        // Landmarks are placed at FRACTIONS of the running lane, so they must use the ACTUAL lane
        // length (StageDirector publishes it) — a hardcoded 1600 would bunch every landmark into the
        // first 1600 wu of a longer stage and leave the rest bare. Falls back to 1600 pre-stage.
        private float LaneLen => StageDirector.ActiveLaneLengthWu;

        // A cluster (the campus) fills edge-to-edge, so on a long lane its fraction-span could demand
        // ~150 buildings; cap it to a compact, readable campus and let the house row carry the rest.
        private const int MaxClusterBuildings = 16;

        private int _lastStage = -1;
        private readonly List<GameObject> _props = new();

        private struct Zone
        {
            public float Frac;      // position along the lane (0..1); with FracEnd → span start
            public float FracEnd;   // >0 → a CLUSTER filling [Frac..FracEnd] with the buildings in folder `File`
            public string File;     // a png name, OR (cluster) a subfolder of zones/ to fill the span from
            public float Tall;      // world-units tall (buildings)
            public float Z;         // depth row
            public bool Foreground;
        }

        private const float Far = 5.7f; // horizon row for buildings

        // Per-stage layout. Stage 1 (index 0): a full LINCOLN HIGH campus (cluster) -> intersection
        // (crosswalk + lights) -> basketball courts -> diner, laid over the house row.
        private static Zone[] ZonesFor(int stage) => stage switch
        {
            0 => new[]
            {
                new Zone { Frac = 0.16f, FracEnd = 0.44f, File = "school", Tall = 15f, Z = Far, Foreground = false }, // whole campus
                new Zone { Frac = 0.50f, File = "crosswalk.png",        Tall = 1.2f, Z = 2.6f, Foreground = false }, // stripes on the road
                new Zone { Frac = 0.49f, File = "traffic_light.png",    Tall = 6f,  Z = 0.6f, Foreground = true },   // near light
                new Zone { Frac = 0.52f, File = "traffic_light.png",    Tall = 5f,  Z = Far,  Foreground = false },  // far light
                new Zone { Frac = 0.64f, File = "basketball_court.png", Tall = 9f,  Z = Far,  Foreground = false },
                new Zone { Frac = 0.86f, File = "diner.png",            Tall = 12f, Z = Far,  Foreground = false },
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
            {
                if (z.FracEnd > z.Frac) PlaceCluster(z, startX);
                else Place(z.File, startX + z.Frac * LaneLen, z.Tall, z.Z, z.Foreground);
            }
        }

        /// <summary>Fill [Frac..FracEnd] with the buildings in zones/&lt;File&gt;/, edge-to-edge (varied), so the
        /// area reads FULL — a whole campus rather than a single building (creator: "each area full enough").</summary>
        private void PlaceCluster(Zone z, float startX)
        {
            var sprites = LoadFolder(z.File);
            if (sprites.Count == 0) return;
            float x = startX + z.Frac * LaneLen;
            float endX = startX + z.FracEnd * LaneLen;
            int i = 0;
            while (x < endX && i < MaxClusterBuildings)
            {
                var spr = sprites[i % sprites.Count];
                float natTall = spr.rect.height / Tuning.PixelsPerUnit;
                // vary the height a touch per building around the zone's target
                float tall = z.Tall * (0.82f + 0.10f * (i % 3));
                float scale = natTall > 0.01f ? tall / natTall : 1f;
                float wWu = (spr.rect.width / Tuning.PixelsPerUnit) * scale;
                var go = MakeProp(spr, x + wWu * 0.5f, z.Z, scale, foreground: false, isCrosswalk: false);
                _props.Add(go);
                x += wWu + 0.4f;   // near-touching campus row
                i++;
            }
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
            float natTall = spr.rect.height / Tuning.PixelsPerUnit;
            float scale = natTall > 0.01f ? tallWu / natTall : 1f;
            _props.Add(MakeProp(spr, worldX, z, scale, foreground, file.Contains("crosswalk")));
        }

        /// <summary>Spawn one world-fixed backdrop prop. Buildings sort far behind the fighters; a
        /// crosswalk lies on the road (still behind actors); a foreground prop (traffic light) is in front.</summary>
        private GameObject MakeProp(Sprite spr, float worldX, float z, float scale, bool foreground, bool isCrosswalk)
        {
            var go = new GameObject("zone_prop");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            Playfield.Place(go.transform, worldX, z, sr);              // world-fixed: the camera scrolls to it
            sr.sortingOrder = foreground ? Playfield.SortingOrder(z) + 3 : (isCrosswalk ? -20 : -300);
            return go;
        }

        // Cache the buildings in a zones/<folder>/ for cluster fills.
        private static readonly Dictionary<string, List<Sprite>> s_folderCache = new();
        private static List<Sprite> LoadFolder(string folder)
        {
            if (s_folderCache.TryGetValue(folder, out var cached)) return cached;
            var list = new List<Sprite>();
            try
            {
                string dir = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "backdrops", "zones", folder);
                if (System.IO.Directory.Exists(dir))
                    foreach (var path in System.IO.Directory.GetFiles(dir, "*.png"))
                    {
                        var s = LoadZone(folder + "/" + System.IO.Path.GetFileName(path));
                        if (s != null) list.Add(s);
                    }
            }
            catch { }
            s_folderCache[folder] = list;
            return list;
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
