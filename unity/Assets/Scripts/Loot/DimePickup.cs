using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A dime lobbed by the Monkey Boss (BOSSES.md §5.7). Catch it (walk over the landing spot before it
    /// fades) to summon a player Monkey Merc directly — this bypasses the coin cost and the 3-summons/level
    /// cap, but the arena still caps at 3 live mercs. Miss it and the boss fields his own merc (the
    /// <c>onMiss</c> callback). Distinct from field coins: a dime here summons directly, no wallet needed.
    /// </summary>
    public sealed class DimePickup : MonoBehaviour
    {
        public float WorldX, Z;
        private SpriteRenderer _sr;
        private float _age, _bob;
        private System.Action _onMiss;
        private bool _resolved;
        private const float Life = 4f;     // catch window after it lands (2-5s so misses/enemy-mercs stay rare)

        public static void Spawn(float x, float z, System.Action onMiss)
        {
            var go = new GameObject("dime");
            var d = go.AddComponent<DimePickup>();
            d.WorldX = x; d.Z = z; d._onMiss = onMiss;
            d._sr = go.AddComponent<SpriteRenderer>();
            d._sr.sprite = DimeSprite();
            d._sr.color = new Color(0.9f, 0.9f, 0.95f);   // silver
        }

        private void Update()
        {
            _age += Time.deltaTime;
            if (_sr != null) _sr.color = Color.Lerp(new Color(0.9f, 0.9f, 0.95f), Color.white, Mathf.PingPong(Time.time * 6f, 1f));

            if (!_resolved)
                foreach (var player in PlayerController.All)
                {
                    if (player == null || !player.Alive) continue;
                    float dx = player.WorldX - WorldX, dz = player.Z - Z;
                    if (dx * dx + dz * dz <= 1.1f * 1.1f)      // CAUGHT
                    {
                        _resolved = true;
                        if (MercController.LiveCount < 3) MercController.Spawn(WorldX, Z);
                        Sfx.Play("coin");
                        Destroy(gameObject);
                        return;
                    }
                }

            if (_age >= Life)
            {
                if (!_resolved) _onMiss?.Invoke();            // MISSED → boss fields his own merc
                Destroy(gameObject);
                return;
            }
            _bob = Mathf.Sin(Time.time * 6f) * 0.05f;
        }

        private void LateUpdate() => Playfield.Place(transform, WorldX, Z + _bob, _sr);

        private static Sprite _dime;
        private static Sprite DimeSprite()
        {
            if (_dime != null) return _dime;
            const int d = 11;
            var tex = new Texture2D(d, d, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[d * d];
            float r = d / 2f;
            for (int y = 0; y < d; y++)
                for (int x = 0; x < d; x++)
                {
                    float dx = x - r + 0.5f, dy = y - r + 0.5f;
                    px[y * d + x] = dx * dx + dy * dy <= r * r ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                }
            tex.SetPixels32(px); tex.Apply();
            _dime = Sprite.Create(tex, new Rect(0, 0, d, d), new Vector2(0.5f, 0f), Tuning.PixelsPerUnit);
            return _dime;
        }
    }
}
