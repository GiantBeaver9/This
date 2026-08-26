using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    public enum Team { Player, Enemy }

    /// <summary>
    /// Base for everything that stands on the playfield. Holds the logical
    /// position (WorldX + depth Z) and drives the 2.5D projection every frame
    /// via <see cref="Playfield"/>. Movement systems write WorldX/Z; the actor
    /// turns that into a scaled, correctly-sorted sprite transform.
    /// Actors register in <see cref="All"/> so AI and combat can query the field.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Actor : MonoBehaviour
    {
        public static readonly List<Actor> All = new();

        public Team Team = Team.Enemy;
        public float WorldX;
        public float Z;                 // depth 0 (near) .. 6 (far)
        public int Facing = 1;          // -1 left, +1 right
        public float Hp = 40f;
        public float MaxHp = 40f;
        public bool Alive = true;
        public float ScaleMult = 1f;    // silhouette size (Bert short, Heavy big, swarmer small)

        [NonSerialized] public SpriteRenderer Sr;
        [NonSerialized] public SpriteAnimator Anim;

        public event Action<Actor, float> Damaged; // (self, amount)
        public event Action<Actor> Died;

        protected virtual void Awake()
        {
            Sr = GetComponent<SpriteRenderer>();
            Anim = GetComponent<SpriteAnimator>();
        }

        protected virtual void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        protected virtual void OnDisable() => All.Remove(this);

        /// <summary>Clamp Z into the band and push the logical position onto the sprite transform.</summary>
        protected virtual void LateUpdate()
        {
            Z = Mathf.Clamp(Z, 0f, Tuning.ZBandDepth);
            Playfield.Place(transform, WorldX, Z, Sr);
            if (ScaleMult != 1f)
            {
                var ls = transform.localScale;
                transform.localScale = new Vector3(ls.x * ScaleMult, ls.y * ScaleMult, 1f);
            }
            if (Anim != null) Anim.SetFacing(Facing);
        }

        public float DistanceTo(Actor other)
        {
            float dx = other.WorldX - WorldX;
            float dz = other.Z - Z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>Apply damage. Returns true if this hit killed the actor.</summary>
        public virtual bool TakeDamage(float amount, Actor source)
        {
            if (!Alive) return false;
            Hp = Mathf.Max(0f, Hp - amount);
            Damaged?.Invoke(this, amount);
            if (Hp <= 0)
            {
                Alive = false;
                Died?.Invoke(this);
                OnDeath(source);
                return true;
            }
            return false;
        }

        protected virtual void OnDeath(Actor source) { }
    }
}
