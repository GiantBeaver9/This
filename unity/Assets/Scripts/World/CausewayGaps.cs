using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The Yolo Causeway's semi-platformer first half (creator: "lv 5 semi platformer for first half,
    /// then goes into farmland"). Over the FIRST HALF of the causeway lane it lays a run of road
    /// PLATFORMS separated by WATER GAPS: you must JUMP the gaps. Standing (grounded) over a gap drops
    /// you in the marsh — a splash, a little chip damage, and a shove back onto the nearest platform edge
    /// (never lethal, never a soft-lock). The SECOND HALF is solid ground (the farmland transition), so
    /// no gaps are placed past mid-lane. Causeway stage only; rebuilds on a stage change.
    /// </summary>
    public sealed class CausewayGaps : MonoBehaviour
    {
        private const float PlatformW = 11f;   // solid road section
        private const float GapW = 4.0f;        // water gap to clear with a jump
        private const float FallDamage = 5f;

        private int _lastStage = -1;
        private readonly List<Vector2> _gaps = new();       // (startX, endX) world ranges
        private readonly List<GameObject> _visuals = new();
        private readonly Dictionary<PlayerController, float> _recover = new();

        private void Update()
        {
            if (CampaignRunner.Instance == null) return;
            int stage = CampaignRunner.Instance.CurrentStage;
            if (stage != _lastStage) { _lastStage = stage; Rebuild(stage); }
            if (_gaps.Count == 0) return;

            float dt = Time.deltaTime;
            foreach (var p in PlayerController.All)
            {
                if (p == null || !p.Alive) continue;
                if (_recover.TryGetValue(p, out float cd) && cd > 0f) { _recover[p] = cd - dt; continue; }
                if (p.Airborne) continue;                       // jumped the gap → safe

                int gi = GapIndexAt(p.WorldX);
                if (gi < 0) continue;

                // Fell in: splash, chip, and shove to the nearest platform edge.
                var g = _gaps[gi];
                p.WorldX = (p.WorldX - g.x) < (g.y - p.WorldX) ? g.x - 0.9f : g.y + 0.9f;
                Sfx.Play("knockdown_thud");
                Vfx.LandPuff(p.WorldX, p.Z);
                p.TakeDamage(FallDamage, null);
                _recover[p] = 0.7f;
            }
        }

        private int GapIndexAt(float x)
        {
            for (int i = 0; i < _gaps.Count; i++)
                if (x > _gaps[i].x && x < _gaps[i].y) return i;
            return -1;
        }

        private void Rebuild(int stage)
        {
            Clear();
            var data = StageDatabase.Get(stage);
            if (data == null || data.BackdropTheme != "area3_causeway") return;
            var p = PlayerController.Primary;
            if (p == null) { _lastStage = -1; return; }   // retry next frame once the player exists

            float startX = p.WorldX;
            float firstHalfEnd = startX + StageDirector.ActiveLaneLengthWu * 0.5f;

            // platform, gap, platform, gap … across the first half; start a bit in so you never spawn on a gap.
            float x = startX + 12f;
            while (x + GapW < firstHalfEnd)
            {
                _gaps.Add(new Vector2(x, x + GapW));
                SpawnWater(x, x + GapW);
                x += GapW + PlatformW;
            }
        }

        private void SpawnWater(float x0, float x1)
        {
            float top = Playfield.FeetY(Tuning.ZBandDepth);   // far edge of the walkable band
            float bot = Playfield.GroundBaselineY;             // near edge
            var go = new GameObject("causeway_gap");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = WaterSprite();
            sr.sortingOrder = -120;                            // over the road band, behind the fighters
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            go.transform.position = new Vector3((x0 + x1) * 0.5f, (top + bot) * 0.5f, 0f);
            sr.size = new Vector2(x1 - x0, top - bot);
            _visuals.Add(go);

            // A pale plank edge on each platform side so the gap reads as a real drop.
            AddEdge(x0);
            AddEdge(x1);
        }

        private void AddEdge(float x)
        {
            float top = Playfield.FeetY(Tuning.ZBandDepth);
            float bot = Playfield.GroundBaselineY;
            var go = new GameObject("causeway_edge");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = EdgeSprite();
            sr.sortingOrder = -119;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            go.transform.position = new Vector3(x, (top + bot) * 0.5f, 0f);
            sr.size = new Vector2(0.35f, top - bot);
            _visuals.Add(go);
        }

        private void Clear()
        {
            foreach (var g in _visuals) if (g != null) Destroy(g);
            _visuals.Clear();
            _gaps.Clear();
            _recover.Clear();
        }

        private void OnDisable() => Clear();

        // ---- Procedural sprites -------------------------------------------------
        private static Sprite _water, _edge;
        private static Sprite WaterSprite()
        {
            if (_water != null) return _water;
            const int W = 16, H = 16;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            var px = new Color32[W * H];
            Color32 deep = new(46, 92, 120, 255), mid = new(64, 118, 150, 255), glint = new(150, 194, 210, 255);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    px[y * W + x] = ((y + x) % 6 == 0) ? mid : deep;
            for (int x = 0; x < W; x += 4) px[(H - 2) * W + ((x + 1) % W)] = glint;   // ripple glints near the top
            for (int x = 0; x < W; x++) px[(H - 1) * W + x] = mid;                     // lighter waterline at the top
            tex.SetPixels32(px); tex.Apply();
            _water = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), Tuning.PixelsPerUnit, 0, SpriteMeshType.FullRect);
            return _water;
        }

        private static Sprite EdgeSprite()
        {
            if (_edge != null) return _edge;
            const int W = 4, H = 8;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[W * H];
            Color32 plank = new(150, 132, 96, 255), dark = new(96, 82, 58, 255);
            for (int i = 0; i < px.Length; i++) px[i] = (i % W == 0) ? dark : plank;
            tex.SetPixels32(px); tex.Apply();
            _edge = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), Tuning.PixelsPerUnit, 0, SpriteMeshType.FullRect);
            return _edge;
        }
    }
}
