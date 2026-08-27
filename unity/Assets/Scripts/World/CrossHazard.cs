using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A generic environmental hazard that traverses the lane on one Z-row and flattens
    /// anyone on it — the shared body behind the per-level "nuisances" (cars on Lincoln,
    /// security guards on Rocklin, planes at the airport, …). Cars keep their own bespoke
    /// <see cref="CarHazard"/>; this covers the rest. The traversal across the screen IS
    /// the telegraph (HazardDirector still blinks a warning arrow first).
    /// </summary>
    public sealed class CrossHazard : MonoBehaviour
    {
        public float WorldX, Z, VelX;
        public float Damage = 20f;          // to the player (0 = a harmless shove, e.g. the guard)
        public float EnemyDamage = 60f;     // to enemies (0 = shove only, don't clear the crowd)
        public float PushX = 0f;            // one-shot shove (wu) applied to EVERYONE it touches, in
                                            // travel direction — a guard that "just pushes everyone back"
        public float StaggerSeconds = 2.0f; // how long struck enemies are knocked out of their attack
        public float HalfLenX = 1.6f;       // contact half-length in X
        public float HitZ = 0.7f;           // depth tolerance on its row
        public float YOffset = 0f;          // lift above the row (planes ride high)
        public float Life = 4f;

        private SpriteRenderer _sr;
        private Vector2 _scale = Vector2.one;
        private readonly HashSet<Actor> _hit = new();

        public static CrossHazard Spawn(Sprite spr, float fromX, float z, float speed, float dmg,
                                        Vector2 scale, float halfLenX, float yOffset, string sfx = null)
        {
            var go = new GameObject("hazard_cross");
            var h = go.AddComponent<CrossHazard>();
            h.WorldX = fromX; h.Z = z; h.VelX = speed; h.Damage = dmg;
            h.HalfLenX = halfLenX; h.YOffset = yOffset; h._scale = scale;
            h._sr = go.AddComponent<SpriteRenderer>();
            h._sr.sprite = spr;
            h._sr.flipX = speed > 0f;    // art faces -X; flip when moving right
            if (!string.IsNullOrEmpty(sfx)) Sfx.Play(sfx);
            return h;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            WorldX += VelX * dt;
            Life -= dt;

            foreach (var a in Actor.All)
            {
                if (!a.Alive || _hit.Contains(a)) continue;
                if (Mathf.Abs(a.WorldX - WorldX) > HalfLenX) continue;
                if (!Playfield.WithinZ(a.Z, Z, HitZ)) continue;
                _hit.Add(a);
                Vfx.HitSpark(a.WorldX, a.Z);
                float dir = Mathf.Sign(VelX);
                if (a is PlayerController pl)
                {
                    if (Damage > 0f) pl.TakeDamage(Damage, null);
                    if (PushX > 0f) pl.WorldX += dir * PushX;   // shove the player along too
                }
                else
                {
                    if (a is IStaggerable s) s.ApplyStagger(StaggerSeconds);
                    if (EnemyDamage > 0f) a.TakeDamage(EnemyDamage, null);
                    a.WorldX += dir * Mathf.Max(1.5f, PushX);   // bowl them over in the travel direction
                }
            }

            if (Life <= 0f) Destroy(gameObject);
        }

        private void LateUpdate()
        {
            Playfield.Place(transform, WorldX, Z, _sr);
            transform.position += Vector3.up * YOffset;
            transform.localScale = new Vector3(_scale.x, _scale.y, 1f);
            if (_sr != null) _sr.sortingOrder = Playfield.SortingOrder(Z) + 6; // in front of its row
        }
    }
}
