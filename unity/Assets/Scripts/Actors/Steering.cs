using UnityEngine;

namespace ThisL
{
    /// <summary>Shared enemy steering: pursue-to-range and hard separation (GAMEPLAY_LOOP §8.2).</summary>
    public static class Steering
    {
        /// <summary>Move <paramref name="a"/> toward a target (x,z) at speed, stopping at stopDist.</summary>
        public static void MoveToward(Actor a, float tx, float tz, float speed, float stopDist, float dt)
        {
            Vector2 to = new(tx - a.WorldX, tz - a.Z);
            if (to.magnitude <= stopDist) return;
            to.Normalize();
            a.WorldX += to.x * speed * dt;
            a.Z += to.y * speed * dt;
        }

        /// <summary>Move away from a target to reach a standoff distance (ranged keep-away).</summary>
        public static void KeepDistance(Actor a, float tx, float tz, float hold, float speed, float dt)
        {
            Vector2 from = new(a.WorldX - tx, a.Z - tz);
            float d = from.magnitude;
            if (d < 0.001f) { a.WorldX += speed * dt; return; }
            if (d < hold - 0.2f)        // too close -> back off
            {
                from.Normalize();
                a.WorldX += from.x * speed * dt;
                a.Z += from.y * speed * dt;
            }
            else if (d > hold + 0.2f)   // too far -> close in
            {
                from.Normalize();
                a.WorldX -= from.x * speed * dt;
                a.Z -= from.y * speed * dt;
            }
        }

        /// <summary>Soft hard-separation from other enemies within the 1.0 wu radius.</summary>
        public static void Separate(Actor a)
        {
            foreach (var o in Actor.All)
            {
                if (o == a || !o.Alive || o.Team != Team.Enemy) continue;
                float dx = a.WorldX - o.WorldX;
                float dz = a.Z - o.Z;
                float d = Mathf.Sqrt(dx * dx + dz * dz);
                if (d < Tuning.PursuerSeparation && d > 0.0001f)
                {
                    float push = (Tuning.PursuerSeparation - d) * 0.5f;
                    a.WorldX += (dx / d) * push;
                    a.Z += (dz / d) * push;
                }
            }
        }
    }
}
