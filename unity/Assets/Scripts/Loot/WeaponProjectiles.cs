using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A Z-aware round that pierces up to <see cref="HitsLeft"/> enemies, its damage
    /// halving at each pass — the Pistol's line-clear (WEAPONS.md §3.1, 12/6/3). Like
    /// <see cref="Projectile"/> it occupies one depth (Z) and travels along X, only
    /// connecting within ±0.4 wu of its depth (no phasing dodge). Each actor is hit at
    /// most once; the shot expires when it runs out of pierces, range, or lifetime.
    /// </summary>
    public sealed class PierceShot : MonoBehaviour
    {
        public Team OwnerTeam;
        public float WorldX, Z, VelX, Life = 0.3f;
        public float Damage;            // current pass damage (halves per pierce)
        public float Falloff = 0.5f;    // multiplier applied after each connect
        public int HitsLeft = 3;
        public float ZombifyChance;     // >0 on gun rounds: a lethal hit may raise a zombie (§3.1)

        private SpriteRenderer _sr;
        private readonly HashSet<Actor> _spent = new();
        private readonly List<Actor> _frameHits = new();
        private const float HitRadiusX = 0.4f;

        public static PierceShot Spawn(Team ownerTeam, float x, float z, float dirX,
                                       float speed, float damage, int pierce, float falloff, Color color)
        {
            var go = new GameObject("pierce_shot");
            var p = go.AddComponent<PierceShot>();
            p.OwnerTeam = ownerTeam;
            p.WorldX = x; p.Z = z;
            p.VelX = Mathf.Sign(dirX) * speed;
            p.Damage = damage;
            p.HitsLeft = Mathf.Max(1, pierce);
            p.Falloff = falloff;
            p._sr = go.AddComponent<SpriteRenderer>();
            p._sr.sprite = WeaponProjectileArt.Dot();
            p._sr.color = color;
            return p;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            WorldX += VelX * dt;
            Life -= dt;
            if (Life <= 0f) { Destroy(gameObject); return; }

            // Gather this frame's fresh overlaps first, THEN damage — TakeDamage can kill,
            // and a dying actor may drop out of Actor.All, so we never mutate it mid-scan.
            _frameHits.Clear();
            foreach (var a in Actor.All)
            {
                if (a == null || !a.Alive || a.Team == OwnerTeam || _spent.Contains(a)) continue;
                if (Mathf.Abs(a.WorldX - WorldX) > HitRadiusX) continue;
                if (!Playfield.WithinZ(a.Z, Z, Tuning.HitboxZTolerance)) continue;
                _frameHits.Add(a);
            }

            foreach (var a in _frameHits)
            {
                // Gun headshot zombify (§3.1): a killing round may raise the target instead of dropping it.
                if (!(ZombifyChance > 0f && a is EnemyController ec && ec.TryZombifyOnLethal(Damage, ZombifyChance)))
                    a.TakeDamage(Damage, null);
                _spent.Add(a);
                Damage *= Falloff;
                if (--HitsLeft <= 0) { Destroy(gameObject); return; }
            }
        }

        private void LateUpdate() { Playfield.Place(transform, WorldX, Z, _sr); transform.position += Vector3.up * 1.0f; if (_sr != null) _sr.sortingOrder = 900; } // bullets at mid-body (not feet), on top
    }

    /// <summary>
    /// A thrown grenade fastball (WEAPONS.md §3.2): it plows forward along X on the
    /// thrower's Z-row and detonates on the first enemy contact or when it reaches its
    /// range, dealing a radial blast to enemies and self-damage to the player if caught.
    /// </summary>
    public sealed class GrenadeProjectile : MonoBehaviour
    {
        public Team OwnerTeam;
        public PlayerController Owner;   // the thrower (for self-damage vs teammate friendly-fire split)
        public float WorldX, Z, VelX, Life;
        public float BlastRadius, BlastDamage, SelfDamage;

        private SpriteRenderer _sr;
        private bool _spent;
        private const float ContactRadiusX = 0.5f;
        // The fastball PLOWS (§3.2): it knocks each enemy in its path down (once) and keeps going,
        // detonating at the end of its throw rather than popping on the first body it touches.
        private readonly HashSet<Actor> _plowed = new();

        public static GrenadeProjectile Spawn(Team ownerTeam, float x, float z, float dirX,
                                              float speed, float life,
                                              float blastRadius, float blastDamage, float selfDamage,
                                              PlayerController owner = null)
        {
            var go = new GameObject("grenade");
            var g = go.AddComponent<GrenadeProjectile>();
            g.OwnerTeam = ownerTeam;
            g.Owner = owner;
            g.WorldX = x; g.Z = z;
            g.VelX = Mathf.Sign(dirX) * speed;
            g.Life = life;
            g.BlastRadius = blastRadius;
            g.BlastDamage = blastDamage;
            g.SelfDamage = selfDamage;
            g._sr = go.AddComponent<SpriteRenderer>();
            g._sr.sprite = WeaponProjectileArt.Dot();
            g._sr.color = new Color(0.5f, 0.8f, 0.4f);
            return g;
        }

        private void Update()
        {
            if (_spent) return;
            float dt = Time.deltaTime;
            WorldX += VelX * dt;
            Life -= dt;

            foreach (var a in Actor.All)
            {
                if (!a.Alive || a.Team == OwnerTeam) continue;
                if (Mathf.Abs(a.WorldX - WorldX) > ContactRadiusX) continue;
                if (!Playfield.WithinZ(a.Z, Z, Tuning.HitboxZTolerance)) continue;
                // Plow THROUGH: knock each enemy down once and keep flying (don't detonate on contact).
                if (_plowed.Add(a))
                {
                    a.TakeDamage(8f, Owner);
                    if (a is IStaggerable s) s.ApplyStagger(0.8f);   // knockdown in the path
                }
            }

            if (Life <= 0f) Detonate();   // blast at the end of the throw
        }

        private void Detonate()
        {
            if (_spent) return;
            _spent = true;

            // Explosives are the SOLE co-op friendly-fire exception (creator ruling): a player's
            // blast damages TEAMMATES like enemies. The thrower is excluded here and instead pays
            // the (steeper) self-damage below — spacing is the price of the payload (§3.2).
            bool playerOwned = OwnerTeam == Team.Player;
            Explosion.Blast(OwnerTeam, WorldX, Z, BlastRadius, BlastDamage,
                            friendlyFire: playerOwned, except: Owner);

            // Self-damage: the thrower alone, at the steeper self rate.
            if (SelfDamage > 0f && Owner != null && Owner.Alive)
            {
                float dx = Owner.WorldX - WorldX, dz = Owner.Z - Z;
                if (dx * dx + dz * dz <= BlastRadius * BlastRadius) Owner.TakeDamage(SelfDamage, null);
            }

            Sfx.Play("grenade_explode");
            CameraShake.Add(CameraShake.Heavy);
            Destroy(gameObject);
        }

        private void LateUpdate()
        {
            if (!_spent) { Playfield.Place(transform, WorldX, Z, _sr); if (_sr != null) _sr.sortingOrder = 900; } // grenade on top too
        }
    }

    /// <summary>
    /// A boomerang (WEAPONS.md §3): flies out along X to its range, then curves BACK to the
    /// thrower's current position. It stuns each enemy it passes (once). MISS → it returns to
    /// hand and you keep throwing (infinite); HIT → you lose it (OnFirstHit), the classic
    /// "bounces away" rule. Spins for readability.
    /// </summary>
    public sealed class BoomerangProjectile : MonoBehaviour
    {
        public Team OwnerTeam;
        public PlayerController Thrower;
        public float StartX, StartZ, Dir = 1f;
        public float Range = 12f, LoopWidth = 4f, FlightTime = 1.7f;
        public float Damage = 15f, StunSeconds = 0.6f;
        public System.Action OnDone;          // fired when the throw completes (used for the 3-charge discard)

        private float WorldX, Z, _t;
        private SpriteRenderer _sr;
        private readonly HashSet<Actor> _hit = new();
        private const float HitRadiusX = 0.7f;

        public static BoomerangProjectile Spawn(Team team, float x, float z, float dirX, PlayerController thrower)
        {
            var go = new GameObject("boomerang");
            var b = go.AddComponent<BoomerangProjectile>();
            b.OwnerTeam = team; b.Thrower = thrower;
            b.StartX = x; b.StartZ = z; b.WorldX = x; b.Z = z;
            b.Dir = Mathf.Sign(dirX == 0 ? 1 : dirX);
            b._sr = go.AddComponent<SpriteRenderer>();
            b._sr.sprite = Frames()[0];
            return b;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _t += dt;
            float u = _t / FlightTime;
            if (u >= 1f) { OnDone?.Invoke(); Destroy(gameObject); return; }

            // TEARDROP flight: out-and-back in X (sin → narrow at the thrower, far at the middle) with a
            // lateral loop in Z (½·sin 2u → bulges one way out, the other coming back).
            WorldX = StartX + Dir * Range * Mathf.Sin(u * Mathf.PI);
            Z = Mathf.Clamp(StartZ + LoopWidth * 0.5f * Mathf.Sin(u * 2f * Mathf.PI), 0f, Tuning.ZBandDepth);

            // Mows through the enemies it passes — hits ~2-4 across the arc (creator).
            foreach (var a in Actor.All)
            {
                if (a == null || !a.Alive || a.Team == OwnerTeam || _hit.Contains(a)) continue;
                if (Mathf.Abs(a.WorldX - WorldX) > HitRadiusX) continue;
                if (!Playfield.WithinZ(a.Z, Z, 0.9f)) continue;
                _hit.Add(a);
                a.TakeDamage(Damage, null);
                if (a is IStaggerable s) s.ApplyStagger(StunSeconds);
                Vfx.HitSpark(a.WorldX, a.Z);
            }
        }

        private void LateUpdate()
        {
            Playfield.Place(transform, WorldX, Z, _sr);
            transform.position += Vector3.up * 1.0f;
            if (_sr != null)
            {
                _sr.sprite = Frames()[(int)(_t * 18f) % 4];   // cycle the 4 spin sprites (creator)
                _sr.sortingOrder = 900;
            }
        }

        // The 4 boomerang spin frames (assets/sprites/props/boomerang_0..3.png); a dot if art is missing.
        private static Sprite[] _frames;
        private static Sprite[] Frames()
        {
            if (_frames != null) return _frames;
            var list = new System.Collections.Generic.List<Sprite>();
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    string path = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "sprites", "props", "boomerang_" + i + ".png");
                    if (!System.IO.File.Exists(path)) continue;
                    var t = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    t.LoadImage(System.IO.File.ReadAllBytes(path)); t.Apply();
                    list.Add(Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), Tuning.PixelsPerUnit));
                }
            }
            catch { }
            if (list.Count == 0) list.Add(WeaponProjectileArt.Dot());
            _frames = list.ToArray();
            return _frames;
        }
    }

    /// <summary>Radial area damage used by explosive weapons (grenade, and reusable by rockets).</summary>
    public static class Explosion
    {
        /// <summary>
        /// Damage every live actor within the radius. By default same-team actors are spared
        /// (enemies never friendly-fire each other). Pass <paramref name="friendlyFire"/> to also
        /// hit own-team actors (player explosives — the sole co-op friendly-fire exception), and
        /// <paramref name="except"/> to spare one actor (the thrower, who takes self-damage instead).
        /// </summary>
        public static void Blast(Team owner, float x, float z, float radius, float damage,
                                 bool friendlyFire = false, Actor except = null)
        {
            float r2 = radius * radius;
            // Snapshot: TakeDamage can mutate Actor.All (deaths), so iterate a copy.
            var actors = new List<Actor>(Actor.All);
            foreach (var a in actors)
            {
                if (a == null || !a.Alive || a == except) continue;
                if (a.Team == owner && !friendlyFire) continue;
                float dx = a.WorldX - x, dz = a.Z - z;
                if (dx * dx + dz * dz <= r2) a.TakeDamage(damage, null);
            }
            Vfx.FinisherFlash(x, z);
        }
    }

    /// <summary>Shared generated sprite for the Loot-authored projectiles (point-filtered dot).</summary>
    internal static class WeaponProjectileArt
    {
        private static Sprite _dot;

        public static Sprite Dot()
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
