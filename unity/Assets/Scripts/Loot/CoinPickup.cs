using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A dropped coin (Area 3+ economy, WEAPONS.md §3.9). Auto-collected on player contact into the
    /// <see cref="Economy"/> wallet. Bronze / silver / gold by value (10 / 20 / 30¢). Times out like a
    /// weapon drop so the ground doesn't litter.
    /// </summary>
    public sealed class CoinPickup : MonoBehaviour
    {
        public float WorldX, Z;
        public int Cents = Economy.CentsRegular;
        private SpriteRenderer _sr;
        private float _bob, _age;
        private const float Life = 12f;

        public static void Spawn(float x, float z, int cents)
        {
            if (!Economy.Active) return;              // no money in the first half
            var go = new GameObject("coin");
            var c = go.AddComponent<CoinPickup>();
            c.WorldX = x; c.Z = z; c.Cents = cents;
            c._sr = go.AddComponent<SpriteRenderer>();
            c._sr.sprite = CoinSprite();
            c._sr.color = ColorFor(cents);
        }

        private void Update()
        {
            _age += Time.deltaTime;
            if (_age >= Life) { Destroy(gameObject); return; }
            if (_sr != null) _sr.enabled = _age < Life - 3f || ((int)(_age * 6f) % 2 == 0);

            foreach (var player in PlayerController.All)
            {
                if (player == null || !player.Alive) continue;
                float dx = player.WorldX - WorldX, dz = player.Z - Z;
                if (dx * dx + dz * dz <= 0.9f * 0.9f)     // auto-pickup on contact
                {
                    Economy.Award(Cents);
                    Sfx.Play("coin");
                    Destroy(gameObject);
                    return;
                }
            }
            _bob = Mathf.Sin(Time.time * 5f) * 0.05f;
        }

        private void LateUpdate() => Playfield.Place(transform, WorldX, Z + _bob, _sr);

        private static Color ColorFor(int cents) =>
            cents >= Economy.CentsRocketMonkey  ? new Color(1f, 0.85f, 0.3f)     // gold (30)
          : cents >= Economy.CentsShotgunMonkey ? new Color(0.85f, 0.85f, 0.92f) // silver (20)
          :                                       new Color(0.82f, 0.52f, 0.26f); // bronze penny (10)

        private static Sprite _coin;
        private static Sprite CoinSprite()
        {
            if (_coin != null) return _coin;
            const int d = 9;
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
            _coin = Sprite.Create(tex, new Rect(0, 0, d, d), new Vector2(0.5f, 0f), Tuning.PixelsPerUnit);
            return _coin;
        }
    }
}
