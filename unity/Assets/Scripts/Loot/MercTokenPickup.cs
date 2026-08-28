using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A Merc-claim token dropped by a Monkey stick figure (WEAPONS.md §3.7). Walk over it while you
    /// hold a dime (≥10¢ AND under the 3-summon/level cap) → the dime is spent and a Monkey Merc poofs
    /// in. If you can't afford it, the token pulses grey and lingers ~5 s, then fades.
    /// </summary>
    public sealed class MercTokenPickup : MonoBehaviour
    {
        public float WorldX, Z;
        private SpriteRenderer _sr;
        private float _age, _bob;
        private const float Life = 5f;

        public static void Spawn(float x, float z)
        {
            if (!Economy.Active) return;
            var go = new GameObject("merc_token");
            var t = go.AddComponent<MercTokenPickup>();
            t.WorldX = x; t.Z = z;
            t._sr = go.AddComponent<SpriteRenderer>();
            t._sr.sprite = TokenSprite();
        }

        private void Update()
        {
            _age += Time.deltaTime;
            if (_age >= Life) { Destroy(gameObject); return; }

            // Pulse bright-green when you can actually claim it; dull grey when you can't afford it.
            if (_sr != null)
                _sr.color = Economy.CanSummon
                    ? Color.Lerp(new Color(0.55f, 1f, 0.55f), Color.white, Mathf.PingPong(Time.time * 4f, 1f))
                    : new Color(0.55f, 0.55f, 0.55f);

            foreach (var player in PlayerController.All)
            {
                if (player == null || !player.Alive) continue;
                float dx = player.WorldX - WorldX, dz = player.Z - Z;
                if (dx * dx + dz * dz <= 0.9f * 0.9f && Economy.CanSummon)
                {
                    Economy.SpendDime();
                    MercController.Spawn(WorldX, Z);
                    Destroy(gameObject);
                    return;
                }
            }
            _bob = Mathf.Sin(Time.time * 5f) * 0.06f;
        }

        private void LateUpdate() => Playfield.Place(transform, WorldX, Z + _bob, _sr);

        private static Sprite _tok;
        private static Sprite TokenSprite()
        {
            if (_tok != null) return _tok;
            const int d = 10;
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
            _tok = Sprite.Create(tex, new Rect(0, 0, d, d), new Vector2(0.5f, 0f), Tuning.PixelsPerUnit);
            return _tok;
        }
    }
}
