using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A lobbed, arcing projectile — the Anti-Aircraft rock and any thrown-head /
    /// grenade lob (ENEMIES.md §2.10, TUNING §4 row 4 / §6.3). Unlike the flat
    /// <see cref="Projectile"/> it travels from a launch point to a fixed target
    /// (x,z) over a set airtime, rising in a parabola for readability, then lands
    /// and deals a small Z-aware splash at the target — so the player dodges by
    /// not being where it lands (step off the spot / the row), never by i-frames.
    /// Purely visual arc via a screen-Y offset; the logical hit is at the landing
    /// point (TUNING §1 Z-tolerance still governs depth).
    /// </summary>
    public sealed class ArcProjectile : MonoBehaviour
    {
        public Team OwnerTeam;
        public float StartX, StartZ, TargetX, TargetZ;
        public float Damage;
        public float SplashRadius = 1.0f;   // wu around the landing point (§4 "1 wu splash")
        public float AirTime = 0.9f;
        public float ArcHeight = 2.5f;       // peak visual lift in wu
        public System.Action OnLand;         // fired the moment it lands
        public System.Action<Actor> OnReflected;  // fired when a bat knocks it back (boss pip hook)

        /// <summary>True once a bat has knocked this arc back — don't re-reflect.</summary>
        public bool Reflected { get; private set; }

        /// <summary>Current interpolated world X/Z (for proximity checks by the bat).</summary>
        public float CurX { get { float u = Mathf.Clamp01(_t / AirTime); return Mathf.Lerp(StartX, TargetX, u); } }
        public float CurZ { get { float u = Mathf.Clamp01(_t / AirTime); return Mathf.Lerp(StartZ, TargetZ, u); } }

        /// <summary>
        /// Bat it back (WEAPONS.md §3.7 / BOSSES.md §5.5 chopper head): retarget to the launch
        /// point, flip to the reflector's team, restart the airtime, and fire OnReflected so a
        /// boss can score the pip. The return splash now spares the reflector's team and hits
        /// the original thrower's.
        /// </summary>
        public void ReflectHome(Actor reflector)
        {
            if (Reflected) return;
            Reflected = true;
            float cx = CurX, cz = CurZ;
            TargetX = StartX; TargetZ = StartZ;   // send it home
            StartX = cx; StartZ = cz;
            OwnerTeam = reflector != null ? reflector.Team : Team.Player;
            _t = 0f;
            OnReflected?.Invoke(reflector);
        }

        private float _t;
        private SpriteRenderer _sr;

        public static ArcProjectile Spawn(Team ownerTeam, float sx, float sz,
                                          float tx, float tz, float damage, Color color,
                                          float airTime = 0.9f)
        {
            var go = new GameObject("arc_projectile");
            var p = go.AddComponent<ArcProjectile>();
            p.OwnerTeam = ownerTeam;
            p.StartX = sx; p.StartZ = Mathf.Clamp(sz, 0f, Tuning.ZBandDepth);
            p.TargetX = tx; p.TargetZ = Mathf.Clamp(tz, 0f, Tuning.ZBandDepth);
            p.Damage = damage;
            p.AirTime = Mathf.Max(0.1f, airTime);
            p._sr = go.AddComponent<SpriteRenderer>();
            p._sr.sprite = Blob();
            p._sr.color = color;
            return p;
        }

        private void Update()
        {
            _t += Time.deltaTime;
            if (_t >= AirTime)
            {
                Land();
                Destroy(gameObject);
            }
        }

        private void Land()
        {
            Vfx.DeathBurst(TargetX, TargetZ); // reuse the burst as a dust puff placeholder
            foreach (var a in Actor.All)
            {
                if (!a.Alive || a.Team == OwnerTeam) continue;
                if (Mathf.Abs(a.WorldX - TargetX) > SplashRadius) continue;
                if (!Playfield.WithinZ(a.Z, TargetZ, SplashRadius)) continue;
                a.TakeDamage(Damage, null);
            }
            OnLand?.Invoke();
        }

        private void LateUpdate()
        {
            float u = Mathf.Clamp01(_t / AirTime);
            float x = Mathf.Lerp(StartX, TargetX, u);
            float z = Mathf.Lerp(StartZ, TargetZ, u);
            Playfield.Place(transform, x, z, _sr);
            var pos = transform.position;
            pos.y += ArcHeight * 4f * u * (1f - u); // 0 -> peak -> 0 parabola
            transform.position = pos;
            if (_sr != null) _sr.sortingOrder = 900; // ALL projectiles render on top (creator: you must see them coming, even if depth says otherwise)
        }

        private static Sprite _blob;
        private static Sprite Blob()
        {
            if (_blob != null) return _blob;
            var tex = new Texture2D(6, 6, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[36];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels32(px); tex.Apply();
            _blob = Sprite.Create(tex, new Rect(0, 0, 6, 6), new Vector2(0.5f, 0.5f), Tuning.PixelsPerUnit);
            return _blob;
        }
    }
}
