using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Scatters STATIC obstacles down the lane so there's stuff in the way to weave around
    /// (creator: "throw some generic ones in for now… the mall has kiosks, the first stage
    /// parked cars, etc."). Places one every ~<see cref="SpacingWu"/> just off the right edge
    /// at a random depth lane (never blocking the whole lane), typed by the current stage.
    /// Clears + reseeds on a stage change. Generic placeholders for now.
    /// </summary>
    public sealed class ObstacleDirector : MonoBehaviour
    {
        public float SpacingWu = 52f;
        public float FirstAtWu = 40f;

        private float _nextX = float.NaN;
        private float _stageStartX;        // player's X when the current stage began (density-ramp origin)
        private int _lastStage = -1;

        private void Update()
        {
            if (!PlayerController.AnyAlive) return;
            var p = PlayerController.Primary;
            if (p == null || !p.Alive) return;

            int stage = CampaignRunner.Instance != null ? CampaignRunner.Instance.CurrentStage : 0;
            if (stage != _lastStage) { ClearAll(); _lastStage = stage; _stageStartX = p.WorldX; _nextX = p.WorldX + FirstAtWu; }

            // Place upcoming obstacles as they come within ~half a screen off the right edge.
            float horizon = p.WorldX + Tuning.ScreenWidthUnits * 0.5f + 4f;
            int guard = 0;
            while (_nextX <= horizon && guard++ < 24)
            {
                PlaceAt(_nextX, stage);
                _nextX += SpacingFor(stage, p.WorldX);
            }
            CullBehind(p.WorldX - 34f);   // keep the live count bounded (dense parked-car rows)
        }

        /// <summary>Obstacles RAMP from sparse at the stage start to dense near the end (creator: "~2 at
        /// the beginning, ~7-8 at the end"). Progress = fraction of the way down the ACTUAL lane — so
        /// the ramp stretches across the whole stage instead of maxing out in the first 1400 wu of a
        /// long one and reading dense the rest of the way.</summary>
        private float SpacingFor(int stage, float playerX)
        {
            float rampLen = Mathf.Max(600f, StageDirector.ActiveLaneLengthWu);
            float progress = Mathf.Clamp01((playerX - _stageStartX) / rampLen);
            // lv1 drops 2 cars (both curbs) per step, so its spacing is wider than single-obstacle stages.
            // It also packs TIGHT near the end — a wall of parked cars approaching Sandwich Bros (creator).
            float startSp = stage == 0 ? 46f : 72f;
            float endSp   = stage == 0 ? 6f  : 22f;
            return Mathf.Lerp(startSp, endSp, progress);
        }

        private static void CullBehind(float cutoffX)
        {
            for (int i = Obstacle.All.Count - 1; i >= 0; i--)
            {
                var o = Obstacle.All[i];
                if (o != null && o.X < cutoffX) Destroy(o.gameObject);
            }
        }

        private void OnDisable() => ClearAll();

        private static void ClearAll()
        {
            for (int i = Obstacle.All.Count - 1; i >= 0; i--)
                if (Obstacle.All[i] != null) Destroy(Obstacle.All[i].gameObject);
        }

        private static void PlaceAt(float x, int stage)
        {
            if (stage == 0)
            {
                // Lincoln: the UPPER car parks up on the far sidewalk against the houses; the LOWER car
                // sits out in the MIDDLE of the street (creator), leaving the near sidewalk clear so the
                // player can walk below it. Stagger them so the two aren't perfectly abreast.
                // Pulled DOWN toward the player (creator): the near car sits at the lower curb, the far
                // car just above mid-street — both low on screen, not up against the houses.
                SpawnCar(x,      0.6f);                    // lower curb, right by the near sidewalk
                SpawnCar(x + 8f, 4.8f);                    // upper car ONE LANE UP (far lane, off mid-street)
                return;
            }
            // MALL: a concourse kiosk parked in the MIDDLE of the lane (creator: "kiosk … in the middle
            // vs sides for parked cars") — the player weaves above or below it.
            if (IsMallStage(stage)) { SpawnKiosk(x); return; }
            float z = Random.Range(1.2f, Tuning.ZBandDepth - 1.2f);   // leave a lane open on either side
            Obstacle.Spawn(CrateSprite(), x, z, 0.7f, 0.6f, new Vector2(1.8f, 1.8f)); // generic crates
        }

        /// <summary>True when the given campaign stage index uses the mall interior (so obstacles are
        /// centre-lane kiosks, not curb-side cars). Theme-driven so it survives stage reordering.</summary>
        private static bool IsMallStage(int stage)
        {
            var data = StageDatabase.Get(stage);
            return data != null && data.BackdropTheme == "area1_mall";
        }

        /// <summary>Drop one random parked car on a curb SIDE, normalised to ~6.6 wu wide — 50% bigger
        /// than before (creator: "parked cars should be scaled up 50%").</summary>
        private static void SpawnCar(float x, float z)
        {
            var spr = CarSprite();
            float natW = spr.rect.width / 30f;                // cars load at 30 ppu
            float sc = natW > 0.1f ? 6.6f / natW : 2.4f;      // was 4.4 / 1.6 → ×1.5
            Obstacle.Spawn(spr, x, z, 1.6f, 0.5f, new Vector2(sc, sc));
        }

        /// <summary>Drop one random mall kiosk in the MIDDLE lane, normalised to ~2.8 wu wide.</summary>
        private static void SpawnKiosk(float x)
        {
            var spr = KioskSprite();
            float natW = spr.rect.width / Tuning.PixelsPerUnit;   // kiosks load at 24 ppu
            float sc = natW > 0.1f ? 2.8f / natW : 2.2f;
            float z = Tuning.ZBandDepth * 0.5f + Random.Range(-0.6f, 0.6f); // centre of the concourse
            Obstacle.Spawn(spr, x, z, 1.0f, 0.8f, new Vector2(sc, sc));
        }

        // ---- Prop sprites (real art from assets/sprites/props, procedural fallback) ----------
        private static Sprite _kiosk, _crate;
        private static Sprite[] _cars, _kiosks;

        /// <summary>A RANDOM mall kiosk from assets/sprites/props/kiosk*.png (variety, like the car set);
        /// falls back to the procedural stall if the art is absent.</summary>
        private static Sprite KioskSprite()
        {
            if (_kiosks == null)
            {
                var list = new System.Collections.Generic.List<Sprite>();
                try
                {
                    string dir = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "sprites", "props");
                    if (System.IO.Directory.Exists(dir))
                        foreach (var path in System.IO.Directory.GetFiles(dir, "kiosk*.png"))
                        {
                            var s = LoadProp(System.IO.Path.GetFileName(path), Tuning.PixelsPerUnit, 0.05f);
                            if (s != null) list.Add(s);
                        }
                }
                catch { }
                if (list.Count == 0) list.Add(ProceduralKiosk());
                _kiosks = list.ToArray();
            }
            return _kiosks[Random.Range(0, _kiosks.Length)];
        }

        /// <summary>Load a bottom-anchored prop PNG at the given ppu. Null if missing/unreadable.</summary>
        private static Sprite LoadProp(string file, float ppu, float pivotY)
        {
            try
            {
                string path = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "sprites", "props", file);
                if (System.IO.File.Exists(path))
                {
                    var t = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    t.LoadImage(System.IO.File.ReadAllBytes(path)); t.Apply();
                    return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, pivotY), ppu);
                }
            }
            catch { }
            return null;
        }

        /// <summary>A RANDOM parked-car type — variety along the curb (creator: "gen a few more types
        /// of cars"). Loads every car_*.png (+ the original car.png) once; falls back to a crate.</summary>
        private static Sprite CarSprite()
        {
            if (_cars == null)
            {
                var list = new System.Collections.Generic.List<Sprite>();
                try
                {
                    string dir = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "sprites", "props");
                    if (System.IO.Directory.Exists(dir))
                        foreach (var path in System.IO.Directory.GetFiles(dir, "car*.png")) // every car type
                        {
                            var s = LoadCar(System.IO.Path.GetFileName(path));
                            if (s != null) list.Add(s);
                        }
                }
                catch { }
                if (list.Count == 0) list.Add(CrateSprite());
                _cars = list.ToArray();
            }
            return _cars[Random.Range(0, _cars.Length)];
        }

        private static Sprite LoadCar(string file)
        {
            try
            {
                string path = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "sprites", "props", file);
                if (System.IO.File.Exists(path))
                {
                    var t = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    t.LoadImage(System.IO.File.ReadAllBytes(path)); t.Apply();
                    return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.12f), 30f);
                }
            }
            catch { }
            return null;
        }

        private static Sprite ProceduralKiosk()
        {
            if (_kiosk != null) return _kiosk;
            const int W = 22, H = 26;
            var px = new Color32[W * H];
            Color32 wood = new(150, 110, 70, 255), post = new(110, 80, 50, 255),
                    canopyA = new(220, 70, 70, 255), canopyB = new(240, 240, 240, 255), counter = new(190, 160, 120, 255);
            void S(int x, int y, Color32 c) { if (x >= 0 && x < W && y >= 0 && y < H) px[y * W + x] = c; }
            for (int y = 0; y < 15; y++) for (int x = 3; x < W - 3; x++) S(x, y, y < 5 ? counter : wood); // stall body + counter top
            for (int y = 0; y < 15; y++) { S(3, y, post); S(W - 4, y, post); }                            // corner posts
            for (int y = 16; y < 22; y++) for (int x = 1; x < W - 1; x++) S(x, y, ((x / 3) % 2 == 0) ? canopyA : canopyB); // striped canopy
            for (int x = 0; x < W; x++) S(x, 22, post);                                                    // canopy lip
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixels32(px); tex.Apply();
            _kiosk = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.05f), Tuning.PixelsPerUnit);
            return _kiosk;
        }

        private static Sprite CrateSprite()
        {
            if (_crate != null) return _crate;
            const int N = 18;
            var px = new Color32[N * N];
            Color32 wood = new(165, 120, 70, 255), edge = new(110, 78, 44, 255);
            void S(int x, int y, Color32 c) { if (x >= 0 && x < N && y >= 0 && y < N) px[y * N + x] = c; }
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    bool border = x == 0 || y == 0 || x == N - 1 || y == N - 1;
                    bool brace = Mathf.Abs(x - y) <= 1 || Mathf.Abs(x - (N - 1 - y)) <= 1; // X brace
                    S(x, y, (border || brace) ? edge : wood);
                }
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixels32(px); tex.Apply();
            _crate = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.05f), Tuning.PixelsPerUnit);
            return _crate;
        }
    }
}
