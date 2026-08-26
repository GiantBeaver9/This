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
                a.TakeDamage(Damage, null);
                _spent.Add(a);
                Damage *= Falloff;
                if (--HitsLeft <= 0) { Destroy(gameObject); return; }
            }
        }

        private void LateUpdate() => Playfield.Place(transform, WorldX, Z, _sr);
    }

    /// <summary>
    /// A thrown grenade fastball (WEAPONS.md §3.2): it plows forward along X on the
    /// thrower's Z-row and detonates on the first enemy contact or when it reaches its
    /// range, dealing a radial blast to enemies and self-damage to the player if caught.
    /// </summary>
    public sealed class GrenadeProjectile : MonoBehaviour
    {
        public Team OwnerTeam;
        public float WorldX, Z, VelX, Life;
        public float BlastRadius, BlastDamage, SelfDamage;

        private SpriteRenderer _sr;
        private bool _spent;
        private const float ContactRadiusX = 0.5f;

        public static GrenadeProjectile Spawn(Team ownerTeam, float x, float z, float dirX,
                                              float speed, float life,
                                              float blastRadius, float blastDamage, float selfDamage)
        {
            var go = new GameObject("grenade");
            var g = go.AddComponent<GrenadeProjectile>();
            g.OwnerTeam = ownerTeam;
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
                Detonate();
                return;
            }

            if (Life <= 0f) Detonate();
        }

        private void Detonate()
        {
            if (_spent) return;
            _spent = true;

            Explosion.Blast(OwnerTeam, WorldX, Z, BlastRadius, BlastDamage);

            // Self-damage: the enemy sweep in Blast() skips the owner team, so any own-team
            // player caught in the blast is handled here — spacing is the price of the
            // payload (§3.2). Co-op: BOTH players can be caught.
            if (SelfDamage > 0f)
            {
                foreach (var me in PlayerController.All)
                {
                    if (me == null || !me.Alive || me.Team != OwnerTeam) continue;
                    float dx = me.WorldX - WorldX, dz = me.Z - Z;
                    if (dx * dx + dz * dz <= BlastRadius * BlastRadius) me.TakeDamage(SelfDamage, null);
                }
            }

            Sfx.Play("grenade_explode");
            CameraShake.Add(CameraShake.Heavy);
            Destroy(gameObject);
        }

        private void LateUpdate()
        {
            if (!_spent) Playfield.Place(transform, WorldX, Z, _sr);
        }
    }

    /// <summary>Radial area damage used by explosive weapons (grenade, and reusable by rockets).</summary>
    public static class Explosion
    {
        /// <summary>Damage every live actor NOT on <paramref name="owner"/>'s team within the radius.</summary>
        public static void Blast(Team owner, float x, float z, float radius, float damage)
        {
            float r2 = radius * radius;
            // Snapshot: TakeDamage can mutate Actor.All (deaths), so iterate a copy.
            var actors = new List<Actor>(Actor.All);
            foreach (var a in actors)
            {
                if (a == null || !a.Alive || a.Team == owner) continue;
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
