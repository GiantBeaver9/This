using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>Shared melee/special hit resolution against the actor registry.</summary>
    public static class Combat
    {
        private static readonly List<Actor> _scratch = new();

        /// <summary>
        /// Resolve a melee swing in front of <paramref name="attacker"/>: hits every
        /// live opposing actor on the facing side within <paramref name="reach"/> in X
        /// and <paramref name="zTol"/> in depth. Returns the actors hit.
        /// </summary>
        public static List<Actor> MeleeHitFront(Actor attacker, float reach, float zTol, int dmg,
                                                float dmgMult = 1f)
        {
            _scratch.Clear();
            foreach (var a in Actor.All)
            {
                if (a == attacker || !a.Alive || a.Team == attacker.Team) continue;
                float dx = (a.WorldX - attacker.WorldX) * attacker.Facing; // >0 means in front
                if (dx < -0.2f || dx > reach) continue;
                if (!Playfield.WithinZ(a.Z, attacker.Z, zTol)) continue;
                _scratch.Add(a);
            }
            int applied = Mathf.RoundToInt(dmg * dmgMult);
            foreach (var a in _scratch) a.TakeDamage(applied, attacker);
            return _scratch;
        }

        /// <summary>
        /// A FOCUSED directional strike (horde-hell): hits live opposing actors in the
        /// <paramref name="dir"/> direction within <paramref name="reach"/> along it and
        /// <paramref name="perpHalf"/> to either side. Works for any cardinal — ←/→ hit
        /// in X, ↑/↓ hit into depth (Z) — so you strike one lane of the mob, not all around.
        /// </summary>
        public static List<Actor> MeleeHitDirectional(Actor attacker, Vector2 dir, float reach,
                                                      float perpHalf, int dmg, float dmgMult = 1f)
        {
            _scratch.Clear();
            if (dir.sqrMagnitude < 0.0001f) dir = new Vector2(attacker.Facing, 0f);
            dir = dir.normalized;
            Vector2 perpDir = new(-dir.y, dir.x);
            foreach (var a in Actor.All)
            {
                if (a == attacker || !a.Alive || a.Team == attacker.Team) continue;
                Vector2 delta = new(a.WorldX - attacker.WorldX, a.Z - attacker.Z);
                float along = Vector2.Dot(delta, dir);
                if (along < -0.2f || along > reach) continue;
                if (Mathf.Abs(Vector2.Dot(delta, perpDir)) > perpHalf) continue;
                _scratch.Add(a);
            }
            int applied = Mathf.RoundToInt(dmg * dmgMult);
            foreach (var a in _scratch) a.TakeDamage(applied, attacker);
            return _scratch;
        }

        /// <summary>
        /// An ANGULAR ARC strike. A target's bearing from the attacker is measured in the X-Z plane
        /// with Facing baked in: 0° = directly in front, +90° = "up" (far depth), −90° = "down"
        /// (near depth). Hits every live opponent whose bearing is within [<paramref name="centerDeg"/>
        /// ± <paramref name="halfDeg"/>] and within <paramref name="reach"/>. Overlapping fans (set
        /// halfDeg a few ° wide) remove the dead angles between the old thin cardinal strips, so the
        /// 8-way attacks feel fluid. e.g. the uppercut sweeps front→up as center 45°, half 50°.
        /// </summary>
        public static List<Actor> MeleeHitArc(Actor attacker, float centerDeg, float halfDeg,
                                              float reach, int dmg, float dmgMult = 1f)
        {
            _scratch.Clear();
            foreach (var a in Actor.All)
            {
                if (a == attacker || !a.Alive || a.Team == attacker.Team) continue;
                float dx = (a.WorldX - attacker.WorldX) * attacker.Facing; // forward-positive
                float dz = a.Z - attacker.Z;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);
                if (dist > reach) continue;
                if (dx < -0.25f && dist > 0.35f) continue;                 // never reach behind (bar point-blank)
                float ang = Mathf.Atan2(dz, dx) * Mathf.Rad2Deg;
                if (Mathf.Abs(Mathf.DeltaAngle(centerDeg, ang)) > halfDeg) continue;
                _scratch.Add(a);
            }
            int applied = Mathf.RoundToInt(dmg * dmgMult);
            foreach (var a in _scratch) a.TakeDamage(applied, attacker);
            return _scratch;
        }

        /// <summary>Live enemies inside a forward cone: on the facing side within
        /// <paramref name="length"/> in X and <paramref name="halfWidthZ"/> in depth.</summary>
        public static List<Actor> EnemiesInFrontCone(Actor from, float length, float halfWidthZ)
        {
            var list = new List<Actor>();
            foreach (var a in Actor.All)
            {
                if (!a.Alive || a.Team == from.Team) continue;
                float dx = (a.WorldX - from.WorldX) * from.Facing;
                if (dx < -0.2f || dx > length) continue;
                if (Mathf.Abs(a.Z - from.Z) > halfWidthZ) continue;
                list.Add(a);
            }
            return list;
        }

        /// <summary>Nearest live enemy to <paramref name="from"/>, or null.</summary>
        public static Actor NearestEnemy(Actor from)
        {
            Actor best = null;
            float bestD = float.MaxValue;
            foreach (var a in Actor.All)
            {
                if (!a.Alive || a.Team == from.Team) continue;
                float d = from.DistanceTo(a);
                if (d < bestD) { bestD = d; best = a; }
            }
            return best;
        }

        /// <summary>
        /// The sniper special: caroms across the field, one-shot-killing up to
        /// <paramref name="maxKills"/> nearest enemies. Killed this way they drop
        /// nothing (handled by passing killedBySpecial=true to death).
        /// </summary>
        public static int SniperRicochet(Actor from, int maxKills)
        {
            var targets = new List<Actor>();
            foreach (var a in Actor.All)
                if (a.Alive && a.Team != from.Team) targets.Add(a);
            targets.Sort((x, y) => from.DistanceTo(x).CompareTo(from.DistanceTo(y)));

            int kills = 0;
            for (int i = 0; i < targets.Count && kills < maxKills; i++)
            {
                if (targets[i] is ISpecialKillable k) k.KillBySpecial(from);
                else targets[i].TakeDamage(9999, from);
                kills++;
            }
            return kills;
        }
    }
}
