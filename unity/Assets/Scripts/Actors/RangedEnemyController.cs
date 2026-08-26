using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Short-range shooter AI (GAMEPLAY_LOOP §8.1, ENEMIES §4). It advances to
    /// its own pinned standoff distance, lines up the player's Z-row, then fires
    /// straight Z-aware shots down that row — the player dodges by stepping off
    /// the row (no i-frames). Backs off if the player closes inside the standoff.
    /// </summary>
    public sealed class RangedEnemyController : Actor, ISpecialKillable, IStaggerable
    {
        [System.NonSerialized] public EnemyDef Def;
        private float _fireTimer;
        private bool _aiming;
        private float _aimTimer;
        private float _stagger;
        private bool _killedBySpecial;
        private bool _dead;

        public void Init(EnemyDef def)
        {
            Def = def;
            Team = Team.Enemy;
            Hp = MaxHp = def.Hp * DifficultySettings.EnemyHpMult;
            if (Anim == null) Anim = GetComponent<SpriteAnimator>();
            Anim.Set = SpriteLibrary.Load(def.SpriteDir, def.SpriteActor);
            Anim.Play("idle", true);
            Shadow.Attach(this, Shadow.MediumTier);
        }

        private void Update()
        {
            if (_dead) return;
            float dt = Time.deltaTime;
            _fireTimer = Mathf.Max(0f, _fireTimer - dt);

            if (_stagger > 0f) { _stagger -= dt; Anim.Play("hurt", false); Steering.Separate(this); return; }

            var player = PlayerController.Instance;
            if (player == null || !player.Alive) { Anim.Play("idle", true); return; }

            Facing = player.WorldX >= WorldX ? 1 : -1;
            float speed = Def.Speed * Tuning.EnemySpeedMult;

            bool linedUp = Mathf.Abs(player.Z - Z) < 0.35f;
            bool inRange = Mathf.Abs(player.WorldX - WorldX) <= Def.FireRange;

            // Aim telegraph: once lined up + off cooldown, plant and flash before firing.
            if (_aiming)
            {
                _aimTimer -= dt;
                float t = Mathf.PingPong(Time.time * 9f, 1f);
                if (Sr != null) Sr.color = Color.Lerp(new Color(1f, 0.85f, 0.35f), new Color(1f, 0.3f, 0.2f), t);
                if (_aimTimer <= 0f)
                {
                    _aiming = false;
                    if (Sr != null) Sr.color = Color.white;
                    _fireTimer = Def.FireInterval;
                    Vfx.MuzzleFlash(WorldX + Facing * 0.6f, Z, Facing);
                    Sfx.Play("pistol");
                    Projectile.Spawn(Team.Enemy, WorldX + Facing * 0.6f, Z, Facing,
                                     Def.ProjectileSpeed, Def.Damage * DifficultySettings.EnemyDamageMult,
                                     new Color(1f, 0.85f, 0.3f));
                }
                return;
            }

            // Keep the pinned X standoff while sliding onto the player's Z-row.
            Steering.KeepDistance(this, player.WorldX, Z, Def.HoldDistance, speed, dt);
            float dz = player.Z - Z;
            Z += Mathf.Clamp(dz, -speed * dt, speed * dt);
            Steering.Separate(this);
            if (Sr != null) Sr.color = Color.white;

            if (linedUp && inRange && _fireTimer <= 0f)
            {
                _aiming = true;
                _aimTimer = Def.WindupSeconds;
                Anim.Play("attack_side", false, restart: true);
            }
            else if (Mathf.Abs(dz) > 0.35f || !inRange) Anim.Play("walk", true);
            else Anim.Play("idle", true);
        }

        public void ApplyStagger(float seconds) { if (!_dead) { _stagger = seconds; Anim.Play("hurt", false, restart: true); } }
        public void KillBySpecial(Actor source) { _killedBySpecial = true; TakeDamage(9999f, source); }

        public override bool TakeDamage(float amount, Actor source)
        {
            if (!_dead && Alive) Anim.Play("hurt", false, restart: true);
            return base.TakeDamage(amount, source);
        }

        protected override void OnDeath(Actor source)
        {
            _dead = true;
            EnemySpawner.NotifyKill();
            Anim.Play("death", false, restart: true);
            Vfx.DeathBurst(WorldX, Z);
            Sfx.Play("knockdown_thud");
            if (!_killedBySpecial)
            {
                if (Def.Loot != LootTier.None)
                {
                    var kind = LootTable.Roll(Def.Loot);
                    if (kind.HasValue) Pickup.SpawnWeapon(kind.Value, WorldX, Z);
                }
                HealPickup.MaybeDrop(WorldX, Z);
            }
            Destroy(gameObject, 1.0f);
        }
    }
}
