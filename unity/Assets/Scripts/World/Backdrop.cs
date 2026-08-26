using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The tiered side-scroller backdrop (AREAS.md §1.1 framing): a stack of
    /// horizontal parallax bands, far → near — (1) <c>sky</c> (gradient + wispy
    /// cloud, doubles as the HUD backdrop), (2) <c>houses</c>/rooftops on a grass
    /// strip, (3) a <c>fence</c> line, (4) a concrete retaining <c>wall</c>, (5) the
    /// <c>road</c> with dashed lane markings, and (6) the near <c>sidewalk</c> the
    /// fighters stand on. Every band is a procedurally-generated, point-filtered,
    /// wrap-Repeat texture (the CarHazard / SpriteLibrary pattern) drawn Tiled so it
    /// repeats seamlessly and never runs out horizontally.
    ///
    /// Parallax: far bands barely move (they mostly track the camera, so they appear
    /// to scroll slowly), the sidewalk is world-fixed (1:1). Each layer is re-centred
    /// under the camera every frame in whole-tile steps, so the tiling is effectively
    /// infinite no matter how far the player advances across the 13-stage run.
    ///
    /// Sorting: all bands park a large negative order in (-1000, 0) — IN FRONT of the
    /// GameBootstrap "Ground" fill (-1000) / "GroundFarLine" (-999) which they replace,
    /// yet well BEHIND every actor (actors use <see cref="Playfield.SortingOrder"/>,
    /// min 0). Far bands sit furthest back, the sidewalk nearest.
    ///
    /// Per-area palette (AREAS.md 4 areas): <see cref="SetTheme"/> / <see cref="SetArea"/>
    /// recolour + rebuild the bands so Suburbs (1), Sacramento/Airport (2), Hills/Dixon
    /// (3) and Bay/GG/SF (4) each read distinctly. Defaults to Area 1.
    /// </summary>
    public sealed class Backdrop : MonoBehaviour
    {
        // ---- Area selection (shared so a freshly Create()d backdrop inherits it) ---
        private static int s_area = 1;
        private int _area = 1;
        private bool _started;

        private struct LayerInstance
        {
            public Transform Transform;
            public float Parallax;   // 0.1 far .. 1.0 near (world-fixed)
            public float BaseY;      // vertical band centre (world Y)
            public float TileWorld;  // one texture-tile width in world units (for infinite wrap)
        }

        private LayerInstance[] _instances;
        private Camera _cam;

        /// <summary>Spawns a GameObject carrying the backdrop (alternative to adding the component directly).</summary>
        public static Backdrop Create()
        {
            var go = new GameObject("Backdrop");
            return go.AddComponent<Backdrop>();
        }

        // ---- Public area API (call from StageDirector/CampaignRunner) --------------

        /// <summary>Set the current area (1..4) and rebuild the live backdrop to its palette.</summary>
        public static void SetArea(int area)
        {
            s_area = Mathf.Clamp(area, 1, 4);
            var inst = FindAnyObjectByType<Backdrop>();
            if (inst != null) inst.ApplyArea(s_area);
        }

        /// <summary>Map a StageData.BackdropTheme stem ("area2_airport", "area1_mall", …) to its area and apply it.</summary>
        public static void SetTheme(string themeStem) => SetArea(AreaFromTheme(themeStem));

        /// <summary>Parse the area index out of a theme stem; defaults to Area 1.</summary>
        public static int AreaFromTheme(string themeStem)
        {
            if (string.IsNullOrEmpty(themeStem)) return 1;
            string t = themeStem.Trim().ToLowerInvariant();
            for (int a = 4; a >= 1; a--)
                if (t.Contains("area" + a)) return a;
            return 1;
        }

        /// <summary>Instance form of <see cref="SetArea"/>: recolour + rebuild this backdrop.</summary>
        public void ApplyArea(int area)
        {
            int a = Mathf.Clamp(area, 1, 4);
            s_area = a;
            _area = a;
            if (_started) BuildBands();
        }

        // ---- Lifecycle -------------------------------------------------------------

        private void Start()
        {
            _cam = ResolveCamera();
            _area = s_area;
            BuildBands();
            _started = true;
        }

        private void LateUpdate()
        {
            if (_instances == null) return;
            if (_cam == null) _cam = ResolveCamera();
            float camX = _cam != null ? _cam.transform.position.x : 0f;

            foreach (var l in _instances)
            {
                // Apparent scroll offset: far (small parallax) tracks the camera most,
                // so it slides least; the sidewalk (1.0) is world-fixed. Re-centre in
                // whole-tile steps so a seamless tile always covers the view — infinite
                // scroll with a modest band width, however far the player has advanced.
                float raw = camX * (1f - l.Parallax);
                float x = raw;
                if (l.TileWorld > 0.01f)
                {
                    float k = Mathf.Round((camX - raw) / l.TileWorld);
                    x = raw + k * l.TileWorld;
                }
                l.Transform.position = new Vector3(x, l.BaseY, 0f);
            }
        }

        private static Camera ResolveCamera()
        {
            return Camera.main != null ? Camera.main : Object.FindAnyObjectByType<Camera>();
        }

        // ---- Band assembly ---------------------------------------------------------

        private delegate Sprite BandBuilder(in Palette p, int heightPx);

        private struct BandDef
        {
            public string Name;
            public float Parallax;
            public int Order;
            public float BottomY;
            public float TopY;
            public BandBuilder Build;
        }

        /// <summary>
        /// The six bands, back-to-front, with world-Y extents derived from the
        /// Playfield so they line up with the actors' feet: the walkable ground plane
        /// runs from the near feet line (<see cref="Playfield.GroundBaselineY"/>, Z=0)
        /// up to the far feet line (<c>FeetY(ZBandDepth)</c>, the horizon), with the
        /// road behind and the sidewalk in front; the wall/fence/houses/sky stack up
        /// from the horizon.
        /// </summary>
        private static BandDef[] MakeBandDefs()
        {
            float top = Playfield.CameraOrthoSize;                    //  7.5 (screen top)
            float horizon = Playfield.FeetY(Tuning.ZBandDepth);      // -0.5 (far feet / top of ground plane)

            float bottom = -top;                                     // -7.5 (screen bottom)

            // Far -> near. The real house/tree sprites are a discrete PROP ROW (BuildPropRow,
            // order -994) between sky and fence. The GROUND is ONE cohesive pavement filling
            // the whole play area (horizon down to the screen bottom) so the fighters read as
            // standing ON it — not on a thin strip above a big empty slab. A thin fence/wall
            // sits at the horizon; the low sidewalk/road split that pushed the ground "too
            // low" is gone.
            // Structured street: a DARK asphalt road strip (with a curb + yellow dashes)
            // runs from the horizon down to `roadBottom`, then a LIGHTER sidewalk fills the
            // near play area below it. The tonal contrast + curb line break up the flat gray
            // ("concrete hell") and read as a real street the fighters stand on.
            float roadBottom = -2.6f;
            return new[]
            {
                new BandDef { Name = "sky",   Parallax = 0.10f, Order = -996, BottomY = horizon - 1.0f,  TopY = top,             Build = BuildSky },
                new BandDef { Name = "fence", Parallax = 0.30f, Order = -992, BottomY = horizon - 0.15f, TopY = horizon + 1.30f, Build = BuildFence },
                new BandDef { Name = "road",  Parallax = 1.00f, Order = -988, BottomY = roadBottom,      TopY = horizon,            Build = BuildRoad },
                new BandDef { Name = "ground",Parallax = 1.00f, Order = -984, BottomY = bottom,          TopY = roadBottom + 0.05f, Build = BuildGround },
            };
        }

        private void BuildBands()
        {
            // Tear down any previous bands (area switch rebuilds).
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            Palette palette = PaletteFor(_area);
            BandDef[] defs = MakeBandDefs();
            var built = new List<LayerInstance>(defs.Length);

            foreach (var def in defs)
            {
                float heightWu = def.TopY - def.BottomY;
                if (heightWu <= 0f) continue;
                int hpx = Mathf.Max(2, Mathf.RoundToInt(heightWu * Tuning.PixelsPerUnit));

                Sprite sprite = def.Build(in palette, hpx);
                if (sprite == null) continue;

                var go = new GameObject("band_" + def.Name);
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = def.Order;
                sr.drawMode = SpriteDrawMode.Tiled;         // seamless horizontal tiling
                sr.tileMode = SpriteTileMode.Continuous;

                float tileWorld = sprite.rect.width / Tuning.PixelsPerUnit;
                // Generous over-cover (3 screens + 4 tiles): with per-frame whole-tile
                // re-centring the band can never uncover a screen edge (that gap was
                // revealing GameBootstrap's dark ground fill on the right).
                float sizeX = Tuning.ScreenWidthUnits * 3f + tileWorld * 4f;
                sr.size = new Vector2(sizeX, heightWu);

                float baseY = (def.TopY + def.BottomY) * 0.5f;
                built.Add(new LayerInstance
                {
                    Transform = go.transform,
                    Parallax = def.Parallax,
                    BaseY = baseY,
                    TileWorld = tileWorld,
                });
            }

            BuildPropRow(built);          // real house/tree sprites at the horizon
            BuildLandmarkRow(built);      // per-area signature landmarks (tower/plane/bridge/skyline)
            _instances = built.ToArray();
        }

        // ---- Real environment props (house/tree/… sprites) -------------------------

        // Cache loaded prop sprites by absolute path (shared across rebuilds/areas).
        private static readonly Dictionary<string, Sprite> s_propCache = new();

        /// <summary>Area N's prop folder, falling back to Area 1 until per-area art exists.</summary>
        private static string PropsDirForArea(int area)
        {
            string root = SpriteLibrary.AssetsRoot;
            string dir = System.IO.Path.Combine(root, "backdrops", "area" + Mathf.Clamp(area, 1, 4) + "_props");
            if (System.IO.Directory.Exists(dir) && System.IO.File.Exists(System.IO.Path.Combine(dir, "house.png")))
                return dir;
            return System.IO.Path.Combine(root, "backdrops", "area1_props"); // placeholder until this area's props land
        }

        /// <summary>Load a bottom-centre-pivoted prop sprite (so it seats on the horizon line). Null if missing.</summary>
        private static Sprite LoadProp(string dir, string file)
        {
            string path = System.IO.Path.Combine(dir, file);
            if (s_propCache.TryGetValue(path, out var cached)) return cached;
            Sprite s = null;
            try
            {
                if (System.IO.File.Exists(path))
                {
                    var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    tex.LoadImage(System.IO.File.ReadAllBytes(path));
                    tex.Apply();
                    s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0f), Tuning.PixelsPerUnit, 0, SpriteMeshType.FullRect); // bottom-centre pivot
                }
            }
            catch { s = null; }
            s_propCache[path] = s;
            return s;
        }

        /// <summary>
        /// Lay the real house/tree sprites in a repeating, varied row along the horizon
        /// (far parallax). Discrete objects — NOT a tiled procedural strip — so they read
        /// as real buildings. The whole row rides one parent transform that parallax-
        /// scrolls and whole-tile-wraps exactly like the tiled bands.
        /// </summary>
        private void BuildPropRow(List<LayerInstance> built)
        {
            string dir = PropsDirForArea(_area);
            Sprite house = LoadProp(dir, "house.png");
            Sprite tree = LoadProp(dir, "tree.png");
            if (house == null && tree == null) return;         // no real props for this area yet

            float horizon = Playfield.FeetY(Tuning.ZBandDepth);
            float rowWidth = Tuning.ScreenWidthUnits * 3f;     // 3 screens so recentring always covers the view

            var parent = new GameObject("band_props");
            parent.transform.SetParent(transform, false);

            // Place across [-rowWidth/2 .. +rowWidth/2] (centred on the parent so recentring
            // keeps props on both sides of the camera).
            float x = -rowWidth * 0.5f;
            int i = 0;
            while (x < rowWidth * 0.5f)
            {
                bool useTree = (i % 4 == 3) && tree != null;   // trees are the occasional break
                Sprite s = useTree ? tree : (house ?? tree);
                if (s == null) break;

                // Small + DENSE like the reference: houses ~2.2 wu tall, near-touching, so the
                // suburb reads as a tight tiled row (not big, sparse, floating).
                float targetTall = useTree ? 2.7f : 2.2f;
                float natTall = s.rect.height / Tuning.PixelsPerUnit;
                float scale = natTall > 0.01f ? targetTall / natTall : 1f;
                scale *= 1f + ((i % 4) - 1.5f) * 0.04f;         // subtle size variation
                float wWu = (s.rect.width / Tuning.PixelsPerUnit) * scale;

                var go = new GameObject(useTree ? "tree" : "house");
                go.transform.SetParent(parent.transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = s;
                sr.sortingOrder = -994;                          // behind fence/wall, in front of sky
                bool flip = (i % 2) == 0;
                go.transform.localScale = new Vector3(flip ? -scale : scale, scale, 1f);
                go.transform.localPosition = new Vector3(x + wWu * 0.5f, horizon, 0f);

                float gap = useTree ? 0.5f : 0.12f;   // houses near-touching (dense suburb row)
                x += wWu + gap;
                i++;
            }

            built.Add(new LayerInstance
            {
                Transform = parent.transform,
                Parallax = 0.24f,          // far layer
                BaseY = 0f,                // props carry their own horizon Y in localPosition
                TileWorld = rowWidth,
            });
        }

        /// <summary>The signature landmark sprites for an area (empty = none yet). These make each
        /// area read differently at a glance — the "levels all look the same" fix.</summary>
        private static string[] LandmarksForArea(int area) => area switch
        {
            2 => new[] { "control_tower.png", "plane.png" },      // Sacramento & Airport
            3 => new[] { "causeway.png" },                        // Hills, Causeway & Dixon
            4 => new[] { "golden_gate.png", "skyline_tower.png" },// Vallejo → GG → SF
            _ => System.Array.Empty<string>(),
        };

        /// <summary>
        /// A far, sparse row of the area's big landmark props (bridge, tower, plane, skyline),
        /// wider-spaced and larger than the house row so each area has a recognizable skyline.
        /// The plane rides high in the sky; the rest seat on the horizon. Own parallax layer.
        /// </summary>
        private void BuildLandmarkRow(List<LayerInstance> built)
        {
            var names = LandmarksForArea(_area);
            if (names.Length == 0) return;
            string dir = PropsDirForArea(_area);

            var sprites = new List<(Sprite s, bool sky)>();
            foreach (var n in names)
            {
                var s = LoadProp(dir, n);
                if (s != null) sprites.Add((s, n.Contains("plane")));  // planes fly; everything else sits
            }
            if (sprites.Count == 0) return;

            float horizon = Playfield.FeetY(Tuning.ZBandDepth);
            float rowWidth = Tuning.ScreenWidthUnits * 3f;

            var parent = new GameObject("band_landmarks");
            parent.transform.SetParent(transform, false);

            // Wide spacing so a landmark is an occasional hero element, not a repeating fence.
            float x = -rowWidth * 0.5f;
            int i = 0;
            const float spacing = 16f;
            while (x < rowWidth * 0.5f)
            {
                var (s, sky) = sprites[i % sprites.Count];
                float targetTall = sky ? 2.0f : 6.5f;      // big skyline landmarks; smaller planes
                float natTall = s.rect.height / Tuning.PixelsPerUnit;
                float scale = natTall > 0.01f ? targetTall / natTall : 1f;

                var go = new GameObject("landmark");
                go.transform.SetParent(parent.transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = s;
                sr.sortingOrder = sky ? -992 : -996;        // planes over the sky; skyline behind houses
                float y = sky ? horizon + 4.5f : horizon;   // planes ride high
                go.transform.localScale = new Vector3(scale, scale, 1f);
                go.transform.localPosition = new Vector3(x, y, 0f);

                x += spacing;
                i++;
            }

            built.Add(new LayerInstance
            {
                Transform = parent.transform,
                Parallax = 0.15f,          // farther than the house row (0.24) → reads as distant/huge
                BaseY = 0f,
                TileWorld = rowWidth,
            });
        }

        // ---- Texture helpers -------------------------------------------------------

        private static Sprite MakeSprite(int w, int h, Color32[] px)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixels32(px);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;          // seamless tiling
            tex.Apply();
            return Sprite.Create(
                tex,
                new Rect(0, 0, w, h),
                new Vector2(0.5f, 0.5f),                    // centred pivot: band centres on its base Y
                Tuning.PixelsPerUnit,
                0,
                SpriteMeshType.FullRect);                   // FullRect required for Tiled draw mode
        }

        private static void SetPx(Color32[] px, int w, int h, int x, int y, Color32 c)
        {
            if (x < 0 || x >= w || y < 0 || y >= h) return;
            px[y * w + x] = c;
        }

        private static Color32 Lerp32(Color32 a, Color32 b, float t)
        {
            return new Color32(
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.r, b.r, t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.g, b.g, t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.b, b.b, t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.a, b.a, t)));
        }

        private static Color32 Blend(Color32 dst, Color32 src, float a)
        {
            a = Mathf.Clamp01(a);
            return new Color32(
                (byte)Mathf.RoundToInt(dst.r + (src.r - dst.r) * a),
                (byte)Mathf.RoundToInt(dst.g + (src.g - dst.g) * a),
                (byte)Mathf.RoundToInt(dst.b + (src.b - dst.b) * a),
                255);
        }

        // ---- Band generators (each seamless left↔right) ----------------------------

        private static Sprite BuildSky(in Palette p, int h)
        {
            const int w = 128;
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                float t = h > 1 ? (float)y / (h - 1) : 0f;  // 0 bottom -> 1 top
                Color32 c = Lerp32(p.SkyBottom, p.SkyTop, t);
                for (int x = 0; x < w; x++) px[y * w + x] = c;
            }
            // One soft wispy cloud parked in the middle third of the tile (edges stay
            // pure sky, so the tile still wraps seamlessly).
            int cx = w / 2, cy = Mathf.RoundToInt(h * 0.72f);
            for (int y = cy - 4; y <= cy + 4; y++)
            {
                for (int x = cx - 18; x <= cx + 18; x++)
                {
                    if (y < 0 || y >= h || x < 0 || x >= w) continue;
                    float dx = (x - cx) / 18f, dy = (y - cy) / 4f;
                    float d = dx * dx + dy * dy;
                    if (d < 1f) px[y * w + x] = Blend(px[y * w + x], p.Cloud, (1f - d) * p.CloudStrength);
                }
            }
            return MakeSprite(w, h, px);
        }

        private static Sprite BuildHouses(in Palette p, int h)
        {
            const int w = 120;
            var px = new Color32[w * h];                    // default transparent above the grass
            int grassTop = Mathf.RoundToInt(h * 0.28f);
            for (int y = 0; y <= grassTop; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = p.Grass;

            // Two houses on the grass, both inset from the tile edges so cols near 0
            // and w-1 stay grass/transparent -> seamless.
            DrawHouse(px, w, h, in p, 8, grassTop, 44, Mathf.RoundToInt(h * 0.66f));
            DrawHouse(px, w, h, in p, 66, grassTop, 40, Mathf.RoundToInt(h * 0.74f));
            return MakeSprite(w, h, px);
        }

        private static void DrawHouse(Color32[] px, int w, int h, in Palette p, int x0, int baseY, int width, int roofTop)
        {
            int span = Mathf.Max(1, roofTop - baseY);
            int wallTop = baseY + Mathf.RoundToInt(span * 0.62f);
            for (int y = baseY; y <= wallTop; y++)
                for (int x = x0; x < x0 + width; x++)
                    SetPx(px, w, h, x, y, p.HouseWall);

            // Trapezoid roof narrowing toward the top.
            int roofSpan = Mathf.Max(1, roofTop - wallTop);
            for (int y = wallTop; y <= roofTop; y++)
            {
                float t = (float)(y - wallTop) / roofSpan;
                int inset = Mathf.RoundToInt(t * (width * 0.5f));
                for (int x = x0 + inset; x < x0 + width - inset; x++)
                    SetPx(px, w, h, x, y, p.Roof);
            }

            // A couple of lit windows on the wall.
            int wy = baseY + (wallTop - baseY) / 2;
            for (int i = 0; i < 2; i++)
            {
                int wx = x0 + 8 + i * (width / 2);
                for (int yy = wy; yy < wy + 4; yy++)
                    for (int xx = wx; xx < wx + 5; xx++)
                        SetPx(px, w, h, xx, yy, p.Window);
            }
        }

        private static Sprite BuildFence(in Palette p, int h)
        {
            const int w = 16;                               // two pickets per tile (period 8)
            var px = new Color32[w * h];                    // transparent gaps show the houses behind
            int top = Mathf.RoundToInt(h * 0.88f);
            for (int bx = 0; bx < w; bx += 8)
            {
                for (int y = 0; y <= top; y++)
                {
                    int taper = Mathf.Max(0, y - (top - 3)); // point the picket tip
                    int lo = bx + taper, hi = bx + 4 - taper;
                    for (int x = lo; x < hi; x++) SetPx(px, w, h, x, y, p.Fence);
                }
            }
            // Two horizontal rails across the full width.
            int[] rails = { Mathf.RoundToInt(h * 0.30f), Mathf.RoundToInt(h * 0.62f) };
            foreach (int ry in rails)
                for (int y = ry; y < ry + 2; y++)
                    for (int x = 0; x < w; x++)
                        SetPx(px, w, h, x, y, p.Fence);
            return MakeSprite(w, h, px);
        }

        private static Sprite BuildWall(in Palette p, int h)
        {
            const int w = 40;                               // concrete panels, period 20
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = p.Wall;
            // Lighter cap along the top.
            for (int y = Mathf.Max(0, h - 2); y < h; y++)
                for (int x = 0; x < w; x++)
                    SetPx(px, w, h, x, y, p.WallCap);
            // Vertical panel seams + a horizontal groove.
            for (int x = 0; x < w; x += 20)
                for (int y = 0; y < h - 2; y++)
                    SetPx(px, w, h, x, y, p.WallShade);
            int gy = Mathf.RoundToInt(h * 0.5f);
            for (int x = 0; x < w; x++) SetPx(px, w, h, x, gy, p.WallShade);
            return MakeSprite(w, h, px);
        }

        private static Sprite BuildRoad(in Palette p, int h)
        {
            const int w = 64;                               // dash 32 + gap 32 -> seamless dashes
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = p.Road;
            // Lighter curb along the top edge (meets the wall/horizon).
            for (int x = 0; x < w; x++)
            {
                SetPx(px, w, h, x, h - 1, p.RoadEdge);
                SetPx(px, w, h, x, h - 2, p.RoadEdge);
            }
            // Dashed centre line.
            int cy = Mathf.RoundToInt(h * 0.5f);
            for (int x = 0; x < 32; x++)
                for (int y = cy - 1; y <= cy + 1; y++)
                    SetPx(px, w, h, x, y, p.RoadDash);
            return MakeSprite(w, h, px);
        }

        private static Sprite BuildGround(in Palette p, int h)
        {
            const int w = 64;
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = p.Walk;

            // HORIZONTAL slab seams (full-width single rows) so the surface reads as flat
            // ground receding into the distance — not vertical joints that look like a wall.
            // Spaced closer toward the top (far) for a hint of perspective.
            int[] seams = { (int)(h * 0.90f), (int)(h * 0.72f), (int)(h * 0.50f), (int)(h * 0.26f) };
            foreach (int sy in seams)
                for (int x = 0; x < w; x++) SetPx(px, w, h, x, sy, p.WalkCrack);

            // Darker curb where the pavement meets the road/horizon (top edge).
            for (int x = 0; x < w; x++)
            {
                SetPx(px, w, h, x, h - 1, p.WalkFront);
                SetPx(px, w, h, x, h - 2, p.WalkFront);
            }

            // Faint speckle for texture (deterministic, seamless: wraps in x via % w).
            for (int i = 0; i < (w * h) / 20; i++)
            {
                int x = (i * 37) % w, y = (i * 61) % h;
                px[y * w + x] = Blend(px[y * w + x], p.WalkCrack, 0.22f);
            }
            return MakeSprite(w, h, px);
        }

        // ---- Per-area palettes (AREAS.md 4 areas) ----------------------------------

        private struct Palette
        {
            public Color32 SkyTop, SkyBottom, Cloud;
            public float CloudStrength;
            public Color32 Grass, HouseWall, Roof, Window;
            public Color32 Fence;
            public Color32 Wall, WallCap, WallShade;
            public Color32 Road, RoadEdge, RoadDash;
            public Color32 Walk, WalkCrack, WalkFront;
        }

        private static Palette PaletteFor(int area)
        {
            switch (Mathf.Clamp(area, 1, 4))
            {
                // Area 2 — Sacramento & Airport: hazier urban blue, Victorian cream houses, dark iron fence.
                case 2:
                    return new Palette
                    {
                        SkyTop = new Color32(95, 150, 195, 255), SkyBottom = new Color32(185, 205, 215, 255),
                        Cloud = new Color32(245, 245, 245, 255), CloudStrength = 0.35f,
                        Grass = new Color32(120, 140, 95, 255), HouseWall = new Color32(225, 205, 180, 255),
                        Roof = new Color32(110, 70, 80, 255), Window = new Color32(90, 120, 150, 255),
                        Fence = new Color32(60, 60, 70, 255),
                        Wall = new Color32(155, 152, 150, 255), WallCap = new Color32(180, 178, 176, 255), WallShade = new Color32(125, 122, 120, 255),
                        Road = new Color32(65, 66, 70, 255), RoadEdge = new Color32(110, 112, 116, 255), RoadDash = new Color32(220, 210, 190, 255),
                        Walk = new Color32(165, 162, 160, 255), WalkCrack = new Color32(125, 122, 120, 255), WalkFront = new Color32(145, 142, 140, 255),
                    };

                // Area 3 — Hills, Causeway & Dixon: warm golden hills, weathered wood, dirt road.
                case 3:
                    return new Palette
                    {
                        SkyTop = new Color32(120, 180, 210, 255), SkyBottom = new Color32(225, 220, 180, 255),
                        Cloud = new Color32(255, 250, 235, 255), CloudStrength = 0.45f,
                        Grass = new Color32(200, 175, 95, 255), HouseWall = new Color32(185, 150, 110, 255),
                        Roof = new Color32(135, 70, 50, 255), Window = new Color32(100, 110, 90, 255),
                        Fence = new Color32(150, 110, 70, 255),
                        Wall = new Color32(175, 160, 130, 255), WallCap = new Color32(200, 185, 155, 255), WallShade = new Color32(140, 125, 100, 255),
                        Road = new Color32(135, 110, 80, 255), RoadEdge = new Color32(160, 135, 100, 255), RoadDash = new Color32(165, 145, 108, 255),
                        Walk = new Color32(185, 170, 140, 255), WalkCrack = new Color32(150, 135, 110, 255), WalkFront = new Color32(165, 150, 125, 255),
                    };

                // Area 4 — Vallejo → Bay → GG → SF: foggy coastal grey-blue, city greys, GG-orange rooftops.
                case 4:
                    return new Palette
                    {
                        SkyTop = new Color32(150, 170, 185, 255), SkyBottom = new Color32(205, 215, 220, 255),
                        Cloud = new Color32(228, 232, 236, 255), CloudStrength = 0.6f,
                        Grass = new Color32(110, 125, 120, 255), HouseWall = new Color32(150, 155, 165, 255),
                        Roof = new Color32(190, 85, 55, 255), Window = new Color32(110, 140, 160, 255),
                        Fence = new Color32(90, 95, 105, 255),
                        Wall = new Color32(145, 148, 155, 255), WallCap = new Color32(170, 173, 180, 255), WallShade = new Color32(115, 118, 125, 255),
                        Road = new Color32(60, 62, 68, 255), RoadEdge = new Color32(100, 104, 112, 255), RoadDash = new Color32(215, 215, 210, 255),
                        Walk = new Color32(155, 158, 164, 255), WalkCrack = new Color32(118, 121, 128, 255), WalkFront = new Color32(138, 141, 148, 255),
                    };

                // Area 1 — Placer Suburbs & Mall (default): clear CA blue, green lawns, tan houses, white pickets.
                default:
                    return new Palette
                    {
                        SkyTop = new Color32(60, 150, 220, 255), SkyBottom = new Color32(150, 205, 235, 255),
                        Cloud = new Color32(255, 255, 255, 255), CloudStrength = 0.5f,
                        Grass = new Color32(95, 150, 70, 255), HouseWall = new Color32(205, 180, 140, 255),
                        Roof = new Color32(150, 80, 60, 255), Window = new Color32(120, 180, 210, 255),
                        Fence = new Color32(240, 240, 235, 255),
                        Wall = new Color32(170, 170, 165, 255), WallCap = new Color32(195, 195, 190, 255), WallShade = new Color32(140, 140, 135, 255),
                        Road = new Color32(70, 72, 78, 255), RoadEdge = new Color32(120, 122, 128, 255), RoadDash = new Color32(225, 200, 70, 255),
                        Walk = new Color32(175, 172, 168, 255), WalkCrack = new Color32(135, 132, 128, 255), WalkFront = new Color32(150, 148, 144, 255),
                    };
            }
        }
    }
}
