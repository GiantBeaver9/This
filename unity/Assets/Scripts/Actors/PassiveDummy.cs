using System;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A training-dummy actor for the show-don't-tell tutorial. It just stands there:
    /// it never pursues or attacks, it only flinches (hurt) when struck and drops on
    /// a hit or two (low HP). It registers on <see cref="Team.Enemy"/> so the player's
    /// directional melee connects with it exactly like a real foe, but it has no AI
    /// <c>Update</c> at all — nothing here moves on its own.
    ///
    /// Owned by the tutorial: <see cref="TutorialController"/> spawns/destroys these
    /// and reads <see cref="WasHit"/> (set the first time the dummy takes damage) to
    /// know when the player has landed a directional punch on it.
    /// </summary>
    public sealed class PassiveDummy : Actor, IStaggerable
    {
        /// <summary>Set true the first time this dummy takes any damage (never cleared).</summary>
        public bool WasHit { get; private set; }

        /// <summary>Fires once, the first time the dummy is struck.</summary>
        public event Action<PassiveDummy> Hit;

        private const float DummyHp = 12f;   // a punch or two drops it

        /// <summary>Place the dummy on the field. Optionally give it a bespoke look
        /// (e.g. the tutorial "zebra" demonstrator) — falls back to the regular-enemy
        /// sprite if that atlas isn't on disk yet.</summary>
        public void Init(float worldX, float z, int facing, string spriteDir = null, string spriteActor = null)
        {
            Team = Team.Enemy;
            Hp = MaxHp = DummyHp;
            WorldX = worldX;
            Z = z;
            Facing = facing;

            bool useOverride = spriteDir != null && spriteActor != null && SpriteLibrary.HasAtlas(spriteDir, spriteActor);
            var set = useOverride
                ? SpriteLibrary.Load(spriteDir, spriteActor)
                : SpriteLibrary.Load("sprites/enemies/enemy_regular", "enemy_regular");
            if (Anim == null) Anim = GetComponent<SpriteAnimator>();
            Anim.Set = set;
            Anim.Play("idle", true);
            Shadow.Attach(this, Shadow.MediumTier);
        }

        public override bool TakeDamage(float amount, Actor source)
        {
            if (!WasHit) { WasHit = true; Hit?.Invoke(this); }

            bool dead = base.TakeDamage(amount, source);
            if (Anim != null) Anim.Play(dead ? "death" : "hurt", false, restart: true);
            Vfx.HitSpark(WorldX, Z);
            Sfx.Play("hit_spark");
            return dead;
        }

        // Reacts to sweep/launch reactions with a flinch, but is otherwise inert.
        public void ApplyStagger(float seconds)
        {
            if (Alive && Anim != null) Anim.Play("hurt", false, restart: true);
        }

        protected override void OnDeath(Actor source)
        {
            Vfx.DeathBurst(WorldX, Z);
            Sfx.Play("knockdown_thud");
            // The corpse frame just holds; TutorialController / GameFlow teardown
            // destroys the GameObject.
        }
    }
}
