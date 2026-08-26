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
        /// the beginning, ~7-8 at the end"). Progress = how far into the ~1400 wu ramp the player is.</summary>
        private float SpacingFor(int stage, float playerX)
        {
            float progress = Mathf.Clamp01((playerX - _stageStartX) / 1400f);
            // lv1 drops 2 cars (both curbs) per step, so its spacing is wider than single-obstacle stages.
            float startSp = stage == 0 ? 50f : 72f;
            float endSp   = stage == 0 ? 10f : 22f;
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
                // Lincoln: parked cars line BOTH curbs (near + far row), the road stays open down the
                // middle. Stagger the far car so the two sides don't sit perfectly abreast.
                SpawnCar(x,      0.7f);
                SpawnCar(x + 8f, Tuning.ZBandDepth - 0.7f);
                return;
            }
            float z = Random.Range(1.2f, Tuning.ZBandDepth - 1.2f);   // leave a lane open on either side
            if (stage == 2) Obstacle.Spawn(KioskSprite(), x, z, 0.8f, 0.7f, new Vector2(2.2f, 2.2f)); // mall kiosks
            else            Obstacle.Spawn(CrateSprite(), x, z, 0.7f, 0.6f, new Vector2(1.8f, 1.8f)); // generic crates
        }

        /// <summary>Drop one random parked car, normalised to ~4.4 wu wide regardless of source size.</summary>
        private static void SpawnCar(float x, float z)
        {
            var spr = CarSprite();
            float natW = spr.rect.width / 30f;                // cars load at 30 ppu
            float sc = natW > 0.1f ? 4.4f / natW : 1.6f;
            Obstacle.Spawn(spr, x, z, 1.6f, 0.5f, new Vector2(sc, sc));
        }

        // ---- Placeholder sprites (real prop art can replace these later) ----------
        private static Sprite _kiosk, _crate;
        private static Sprite[] _cars;

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

        private static Sprite KioskSprite()
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
