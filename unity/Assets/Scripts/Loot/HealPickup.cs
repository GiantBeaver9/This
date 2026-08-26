using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A dropped heal (TUNING §2.2: flat +25 HP, capped at 100 — no full heals).
    /// Unlike weapons, a heal is grabbed automatically by walking over it (you don't
    /// "equip" it). Drops at a small chance on a kill, higher when the player is low.
    /// </summary>
    public sealed class HealPickup : MonoBehaviour
    {
        public float WorldX;
        public float Z;
        private SpriteRenderer _sr;
        private float _bob;

        private const float GrabRadius = 0.8f;

        /// <summary>Roll a heal drop on a kill — likelier when the player is low.</summary>
        public static void MaybeDrop(float x, float z)
        {
            var p = PlayerController.Instance;
            float chance = (p != null && p.Hp <= p.MaxHp * (Tuning.LowHpThreshold / Tuning.PlayerMaxHp))
                ? Tuning.HealDropChanceLowHp : Tuning.HealDropChance;
            if (Random.value < chance) Spawn(x, z);
        }

        public static HealPickup Spawn(float x, float z)
        {
            var go = new GameObject("pickup_heal");
            var p = go.AddComponent<HealPickup>();
            p.WorldX = x;
            p.Z = z;
            p._sr = go.AddComponent<SpriteRenderer>();
            p._sr.sprite = Marker();
            p._sr.color = new Color(0.4f, 1f, 0.5f); // green cross
            return p;
        }

        private void Update()
        {
            var player = PlayerController.Instance;
            if (player != null && player.Alive)
            {
                float dx = player.WorldX - WorldX, dz = player.Z - Z;
                if (dx * dx + dz * dz <= GrabRadius * GrabRadius)
                {
                    player.Heal(Tuning.HealRestore);
                    Sfx.Play("heal_pickup_chime");
                    Destroy(gameObject);
                    return;
                }
            }
            _bob = Mathf.Sin(Time.time * 4f) * 0.06f;
        }

        private void LateUpdate() => Playfield.Place(transform, WorldX, Z + _bob, _sr);

        private static Sprite _square;
        private static Sprite Marker()
        {
            if (_square == null)
            {
                var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                var px = new Color32[64];
                for (int i = 0; i < px.Length; i++) px[i] = Color.white;
                tex.SetPixels32(px); tex.Apply();
                _square = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0f), Tuning.PixelsPerUnit);
            }
            return _square;
        }
    }
}
