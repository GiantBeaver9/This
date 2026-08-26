using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A Z-aware straight shot — the readable "bullet" of the spacing game
    /// (GAMEPLAY_LOOP §3, §8.3). A projectile occupies one depth (Z) and travels
    /// along X; it only connects with a target within ±0.4 wu of its depth
    /// (TUNING §1 bullet Z-tolerance), so the player dodges by stepping off the
    /// row — never by phasing through (no i-frames).
    /// </summary>
    public sealed class Projectile : MonoBehaviour
    {
        public Team OwnerTeam;
        public float WorldX, Z, VelX, Damage;
        public float Life = 3f;
        public float StunSeconds;              // >0 => stagger instead of damage (boomerang)
        public float ZombifyChance;            // >0 on gun rounds: a lethal hit may raise a zombie (§3.1)
        public StaffElement? StaffEffect;      // set on a staff cast: applies freeze/burn/stun on hit (§3.5)
        public System.Action OnConnect;        // fired when it hits a target
        private SpriteRenderer _sr;

        private const float HitRadiusX = 0.4f;

        public static Projectile Spawn(Team ownerTeam, float x, float z, float dirX,
                                       float speed, float damage, Color color)
        {
            var go = new GameObject("projectile");
            var p = go.AddComponent<Projectile>();
            p.OwnerTeam = ownerTeam;
            p.WorldX = x; p.Z = z;
            // Enemy bullets fly ~0.6x speed (creator ruling — fairness/dodgeability). Player shots unchanged.
            if (ownerTeam == Team.Enemy) speed *= 0.6f;
            p.VelX = Mathf.Sign(dirX) * speed;
            p.Damage = damage;
            p._sr = go.AddComponent<SpriteRenderer>();
            p._sr.sprite = Dot();
            p._sr.color = color;
            return p;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            WorldX += VelX * dt;
            Life -= dt;
            if (Life <= 0f) { Destroy(gameObject); return; }

            foreach (var a in Actor.All)
            {
                if (!a.Alive || a.Team == OwnerTeam) continue;
                if (Mathf.Abs(a.WorldX - WorldX) > HitRadiusX) continue;
                if (!Playfield.WithinZ(a.Z, Z, Tuning.HitboxZTolerance)) continue;
                if (StunSeconds > 0f) { if (a is IStaggerable s) s.ApplyStagger(StunSeconds); }
                else if (!(ZombifyChance > 0f && a is EnemyController ec && ec.TryZombifyOnLethal(Damage, ZombifyChance)))
                {
                    a.TakeDamage(Damage, null);
                    if (StaffEffect.HasValue && a is EnemyController se && se.Alive) se.ApplyStaffStatus(StaffEffect.Value);
                }
                OnConnect?.Invoke();
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// Bat/parry reflect (WEAPONS.md §3.7 Bat): flip this shot to the reflector's team and
        /// send it back along <paramref name="dirX"/>. Reflected player shots regain full speed
        /// (enemy bullets spawn at 0.6× for fairness) and recolor cool-blue so the bat-back reads.
        /// </summary>
        public void Reflect(Team newTeam, float dirX)
        {
            OwnerTeam = newTeam;
            float dir = dirX != 0f ? Mathf.Sign(dirX) : Mathf.Sign(VelX);
            float speed = Mathf.Abs(VelX);
            if (newTeam == Team.Player) speed = Mathf.Max(speed, 20f); // undo the enemy 0.6× slow
            VelX = dir * speed;
            Life = Mathf.Max(Life, 1.2f);
            StunSeconds = 0f;                          // a batted bullet deals damage, not stun
            if (_sr != null) _sr.color = new Color(0.6f, 0.9f, 1f);
        }

        // Bullets fly at ~mid-character height, not along the ground (collision is
        // still Z-band logical, so this is purely where the shot is drawn).
        private const float MuzzleHeight = 1.0f; // ~half a 2wu character

        private void LateUpdate()
        {
            Playfield.Place(transform, WorldX, Z, _sr);
            transform.position += Vector3.up * 1.0f;   // bullets at mid-body height, not feet
            if (_sr != null) _sr.sortingOrder = 900; // projectiles always on top (see them coming)
            var p = transform.position;
            p.y += MuzzleHeight * Playfield.DepthScale(Z);
            transform.position = p;
        }

        private static Sprite _dot;
        private static Sprite Dot()
        {
            if (_dot != null) return _dot;
            var tex = new Texture2D(6, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[24];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels32(px); tex.Apply();
            _dot = Sprite.Create(tex, new Rect(0, 0, 6, 4), new Vector2(0.5f, 0.5f), Tuning.PixelsPerUnit);
            return _dot;
        }
    }
}
