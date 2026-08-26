using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A car that barrels across the lane on one Z-row (AREAS.md Area-1 hazard,
    /// car_bus_passby / car_horn). It flattens ANYONE on its row — the player gets
    /// hit + knocked down, enemies get bowled over (heavy damage + long stagger).
    /// You see it coming across the screen; that traversal IS the telegraph.
    /// </summary>
    public sealed class CarHazard : MonoBehaviour
    {
        public float WorldX;
        public float Z;
        public float VelX;
        public float Damage = 22f;

        private SpriteRenderer _sr;
        private float _life = 4f;
        private readonly System.Collections.Generic.HashSet<Actor> _hit = new();

        private const float HalfLenX = 1.6f;   // car half-length for contact
        private const float HitZ = 0.7f;       // depth tolerance on its row

        public static CarHazard Spawn(float fromX, float z, float speed, Color color)
        {
            var go = new GameObject("hazard_car");
            var h = go.AddComponent<CarHazard>();
            h.WorldX = fromX; h.Z = z; h.VelX = speed;
            h._sr = go.AddComponent<SpriteRenderer>();
            h._sr.sprite = CarSprite();
            h._sr.color = _usingRealCar ? Color.white : color; // don't tint the real pixel-art car
            h._sr.flipX = speed > 0f;                           // art faces -X; flip when driving right so it never looks reversed
            Sfx.Play("car_horn");
            return h;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            WorldX += VelX * dt;
            _life -= dt;

            foreach (var a in Actor.All)
            {
                if (!a.Alive || _hit.Contains(a)) continue;
                if (Mathf.Abs(a.WorldX - WorldX) > HalfLenX) continue;
                if (!Playfield.WithinZ(a.Z, Z, HitZ)) continue;
                _hit.Add(a);
                Vfx.HitSpark(a.WorldX, a.Z);
                if (a is PlayerController pl) pl.TakeDamage(Damage, null);
                else
                {
                    if (a is IStaggerable s) s.ApplyStagger(2.0f);
                    a.TakeDamage(60f, null);         // cars flatten enemies
                    a.WorldX += Mathf.Sign(VelX) * 1.5f;
                }
            }

            if (_life <= 0f) Destroy(gameObject);
        }

        private static bool _usingRealCar;

        private void LateUpdate()
        {
            // Draw on its row, in front of actors on that row.
            Playfield.Place(transform, WorldX, Z, _sr);
            if (!_usingRealCar) transform.localScale = new Vector3(3.2f, 1.5f, 1f); // upscale the tiny procedural box
            if (_sr != null) _sr.sortingOrder = Playfield.SortingOrder(Z) + 5;
        }

        private static Sprite _car;
        private static Sprite CarSprite()
        {
            if (_car != null) return _car;

            // Real pixel-art car if present (assets/sprites/props/car.png).
            try
            {
                string path = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "sprites", "props", "car.png");
                if (System.IO.File.Exists(path))
                {
                    var t = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    t.LoadImage(System.IO.File.ReadAllBytes(path));
                    t.Apply();
                    _usingRealCar = true;
                    _car = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.12f), 30f); // ~car-sized
                    return _car;
                }
            }
            catch { /* fall through to procedural */ }
            // A little chunky car: body + darker roof band + windows.
            const int W = 32, H = 16;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    Color32 c = new(230, 60, 55, 255);                 // body
                    if (y > H * 0.55f && x > 4 && x < W - 4) c = new Color32(180, 40, 38, 255); // roof
                    if (y > H * 0.6f && ((x > 7 && x < 13) || (x > 19 && x < 25))) c = new Color32(120, 200, 230, 255); // windows
                    if (y < 3 && (x < 6 || x > W - 6)) c = new Color32(20, 20, 24, 255); // wheels
                    px[y * W + x] = c;
                }
            tex.SetPixels32(px); tex.Apply();
            _car = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.2f), 12f);
            return _car;
        }
    }
}
