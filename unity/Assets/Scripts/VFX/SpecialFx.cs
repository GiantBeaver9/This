using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Programmatic flourishes for the character specials (CHARACTERS.md §2) so the
    /// power moment reads even before the bespoke frames land: a rising power GLOW
    /// (werewolf transform / underdog empower) and an expanding RADIUS RING (the
    /// underdog vaporize / a shockwave). Cheap generated sprites, self-destroying.
    /// </summary>
    public static class SpecialFx
    {
        /// <summary>An expanding, fading ring that shows a radius (wu) at a world point.</summary>
        public static void Ring(float worldX, float z, float radiusWu, Color color, float duration = 0.4f)
        {
            var go = new GameObject("fx_ring");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RingSprite();
            sr.color = color;
            sr.sortingOrder = 700;
            go.AddComponent<ExpandRing>().Init(worldX, z, radiusWu, duration, sr);
        }

        /// <summary>A glow aura that swells around an actor and fades (transform / empower).</summary>
        public static void Glow(Actor owner, Color color, float duration = 0.6f)
        {
            if (owner == null) return;
            var go = new GameObject("fx_glow");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GlowSprite();
            sr.color = color;
            sr.sortingOrder = -1; // just behind the actor
            go.AddComponent<GlowAura>().Init(owner, duration, sr);
        }

        // ---- generated sprites ------------------------------------------------
        private static Sprite _ring, _glow;

        private static Sprite RingSprite()
        {
            if (_ring != null) return _ring;
            const int N = 64;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color32[N * N];
            float c = (N - 1) / 2f, outer = c, inner = c - 4f;
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    byte a = (byte)(d <= outer && d >= inner ? 255 : 0);
                    px[y * N + x] = new Color32(255, 255, 255, a);
                }
            tex.SetPixels32(px); tex.Apply();
            _ring = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), N); // 1 unit = full tex
            return _ring;
        }

        private static Sprite GlowSprite()
        {
            if (_glow != null) return _glow;
            const int N = 64;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color32[N * N];
            float c = (N - 1) / 2f, r = c;
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / r;
                    byte a = (byte)(Mathf.Clamp01(1f - d) * 255);
                    px[y * N + x] = new Color32(255, 255, 255, a);
                }
            tex.SetPixels32(px); tex.Apply();
            _glow = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), N);
            return _glow;
        }

        private sealed class ExpandRing : MonoBehaviour
        {
            private float _x, _z, _radius, _dur, _t;
            private SpriteRenderer _sr;
            public void Init(float x, float z, float radius, float dur, SpriteRenderer sr)
            { _x = x; _z = z; _radius = radius; _dur = dur; _sr = sr; }
            private void Update()
            {
                _t += Time.deltaTime;
                float k = Mathf.Clamp01(_t / _dur);
                float scale = Mathf.Lerp(0.2f, _radius * 2f, k); // sprite is 1 unit wide -> diameter
                transform.localScale = new Vector3(scale, scale, 1f);
                var c = _sr.color; c.a = 1f - k; _sr.color = c;
                if (k >= 1f) Destroy(gameObject);
            }
            private void LateUpdate() =>
                transform.position = new Vector3(_x, Playfield.FeetY(_z) + 0.5f, 0f);
        }

        private sealed class GlowAura : MonoBehaviour
        {
            private Actor _owner;
            private float _dur, _t;
            private SpriteRenderer _sr;
            public void Init(Actor owner, float dur, SpriteRenderer sr)
            { _owner = owner; _dur = dur; _sr = sr; }
            private void Update()
            {
                _t += Time.deltaTime;
                float k = Mathf.Clamp01(_t / _dur);
                float pulse = 2.2f + Mathf.Sin(_t * 18f) * 0.25f;
                transform.localScale = new Vector3(pulse, pulse, 1f);
                var c = _sr.color; c.a = (1f - k) * 0.8f; _sr.color = c;
                if (_owner == null || k >= 1f) { Destroy(gameObject); return; }
            }
            private void LateUpdate()
            {
                if (_owner != null)
                    transform.position = new Vector3(_owner.WorldX, Playfield.FeetY(_owner.Z) + 1f, 0f);
            }
        }
    }
}
