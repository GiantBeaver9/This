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
        private int _lastStage = -1;

        private void Update()
        {
            if (!PlayerController.AnyAlive) return;
            var p = PlayerController.Primary;
            if (p == null || !p.Alive) return;

            int stage = CampaignRunner.Instance != null ? CampaignRunner.Instance.CurrentStage : 0;
            if (stage != _lastStage) { ClearAll(); _lastStage = stage; _nextX = p.WorldX + FirstAtWu; }

            // Place upcoming obstacles as they come within ~half a screen off the right edge.
            float horizon = p.WorldX + Tuning.ScreenWidthUnits * 0.5f + 4f;
            int guard = 0;
            while (_nextX <= horizon && guard++ < 8)
            {
                PlaceAt(_nextX, stage);
                _nextX += SpacingWu;
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
            float z = Random.Range(1.2f, Tuning.ZBandDepth - 1.2f);   // leave a lane open on either side
            switch (stage)
            {
                case 0:  Obstacle.Spawn(CarSprite(),   x, z, 1.5f, 0.5f, new Vector2(3.0f, 1.4f)); break; // parked cars
                case 2:  Obstacle.Spawn(KioskSprite(), x, z, 0.8f, 0.7f, new Vector2(2.2f, 2.2f)); break; // mall kiosks
                default: Obstacle.Spawn(CrateSprite(), x, z, 0.7f, 0.6f, new Vector2(1.8f, 1.8f)); break; // generic crates
            }
        }

        // ---- Placeholder sprites (real prop art can replace these later) ----------
        private static Sprite _car, _kiosk, _crate;

        private static Sprite CarSprite()
        {
            if (_car != null) return _car;
            try
            {
                string path = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "sprites", "props", "car.png");
                if (System.IO.File.Exists(path))
                {
                    var t = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    t.LoadImage(System.IO.File.ReadAllBytes(path)); t.Apply();
                    _car = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.12f), 30f);
                    return _car;
                }
            }
            catch { }
            _car = CrateSprite(); // fallback
            return _car;
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
