using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A player-summoned Monkey Merc (WEAPONS.md §3.7). A friendly gun-monkey that hunts the nearest
    /// enemy and fires at 2 shots/s. The whole squad's weapon tier scales with the LIVE count
    /// (1 = pistol / 2 = shotguns / 3 = rockets), and adding one RE-ARMS all + resets every lifespan
    /// (20 / 10 / 5 s). Never friendly-fires (shots are Team.Player; the rocket blast spares own team).
    /// Can't be healed — it just expires on its timer.
    /// </summary>
    public sealed class MercController : MonoBehaviour
    {
        public float WorldX, Z;
        public int Facing = 1;
        private SpriteRenderer _sr;
        private float _life, _fireCd, _bob;

        private static readonly List<MercController> _live = new();
        public static int LiveCount => _live.Count;

        private enum Tier { Pistol, Shotgun, Rocket }
        private static Tier CurrentTier => _live.Count >= 3 ? Tier.Rocket : _live.Count >= 2 ? Tier.Shotgun : Tier.Pistol;
        private static float LifeForTier(Tier t) => t == Tier.Rocket ? 5f : t == Tier.Shotgun ? 10f : 20f;

        public static MercController Spawn(float x, float z)
        {
            var go = new GameObject("monkey_merc");
            var m = go.AddComponent<MercController>();
            m.WorldX = x; m.Z = z;
            m._sr = go.AddComponent<SpriteRenderer>();
            var set = SpriteLibrary.Load("sprites/enemies/enemy_monkey", "enemy_monkey");
            m._sr.sprite = set != null ? (set.FirstOf("idle") ?? set.First) : null;
            m._sr.color = Color.white;   // use the real monkey stick-figure sprite as-is (no green tint)
            _live.Add(m);
            ReArmSquad();                                // re-arm the whole squad + reset all timers (§3.7)
            Vfx.DeathBurst(x, z, 0.8f);                  // 0.5s "poof in"
            Sfx.Play("armed_ready_chime");
            return m;
        }

        /// <summary>Re-arm the squad to the current live tier and reset every merc's lifespan.</summary>
        private static void ReArmSquad()
        {
            float life = LifeForTier(CurrentTier);
            foreach (var m in _live) if (m != null) m._life = life;
        }

        private void OnDisable()
        {
            _live.Remove(this);
            if (_live.Count > 0) ReArmSquad();           // fewer mercs → the squad drops a tier
        }

        private void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0f) { Vfx.DeathBurst(WorldX, Z, 0.7f); Destroy(gameObject); return; }
            _fireCd -= Time.deltaTime;

            var target = NearestEnemy();
            if (target != null)
            {
                float dx = target.WorldX - WorldX;
                Facing = dx >= 0f ? 1 : -1;
                const float range = 6f;
                if (Mathf.Abs(dx) > range) WorldX += Mathf.Sign(dx) * 3.2f * Time.deltaTime;
                Z += Mathf.Clamp(target.Z - Z, -2f * Time.deltaTime, 2f * Time.deltaTime);
                if (_fireCd <= 0f && Mathf.Abs(dx) <= range + 1.5f)
                {
                    _fireCd = 0.5f;                       // 2 shots/sec at every tier
                    Fire(CurrentTier);
                }
            }
            else
            {
                // No enemies: loiter near the player.
                var p = PlayerController.Instance;
                if (p != null) { float dx = p.WorldX - WorldX; if (Mathf.Abs(dx) > 2f) WorldX += Mathf.Sign(dx) * 3.2f * Time.deltaTime; }
            }
            _bob = Mathf.Sin(Time.time * 7f) * 0.05f;
        }

        private void Fire(Tier t)
        {
            float x = WorldX + Facing * 0.5f;
            switch (t)
            {
                case Tier.Pistol:
                    Sfx.Play("pistol");
                    Projectile.Spawn(Team.Player, x, Z, Facing, 18f, 8f, new Color(1f, 0.9f, 0.4f));
                    break;
                case Tier.Shotgun:
                    Sfx.Play("shotgun");
                    for (int i = -2; i <= 2; i++)
                    {
                        var pel = Projectile.Spawn(Team.Player, x, Z, Facing, 16f, 5f, new Color(1f, 0.8f, 0.4f));
                        pel.VelZ = i * 3f; pel.Life = 0.5f;
                    }
                    break;
                case Tier.Rocket:
                    Sfx.Play("grenade_explode");
                    var r = Projectile.Spawn(Team.Player, x, Z, Facing, 14f, 14f, new Color(1f, 0.5f, 0.2f));
                    r.OnConnect = () => Explosion.Blast(Team.Player, r.WorldX, r.Z, 2.2f, 22f); // no FF: spares own team
                    break;
            }
        }

        private Actor NearestEnemy()
        {
            Actor best = null; float bestD = 999f;
            foreach (var a in Actor.All)
            {
                if (a == null || !a.Alive || a.Team != Team.Enemy) continue;
                float d = Mathf.Abs(a.WorldX - WorldX) + Mathf.Abs(a.Z - Z) * 0.5f;
                if (d < bestD) { bestD = d; best = a; }
            }
            return best;
        }

        private void LateUpdate() => Playfield.Place(transform, WorldX, Z + _bob, _sr);
    }
}
