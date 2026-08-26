using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Anti-Aircraft rock-lobber AI (ENEMIES.md §2.10, TUNING §4 row 4). Holds at
    /// its pinned standoff (8 wu), telegraphs an overhead throw for ~0.5 s, then
    /// lobs an arcing <see cref="ArcProjectile"/> at the player's spot — dodged by
    /// stepping off the landing point. Backs off if the player closes inside the
    /// standoff. Drops loot on death like <see cref="EnemyController"/> (nothing
    /// on a sniper-special kill). Boomerang-bait counterplay (ENEMIES §2.10) is a
    /// gap; the straight-shot fallback is <see cref="RangedEnemyController"/>.
    /// </summary>
    public sealed class AntiAircraftController : Actor, ISpecialKillable, IStaggerable
    {
        private enum State { Hold, Windup, Dead }

        [System.NonSerialized] public EnemyDef Def;
        private State _state = State.Hold;
        private float _cooldown;
        private float _windup;
        private float _stagger;
        private float _targetX, _targetZ;
        private bool _killedBySpecial;

        public void Init(EnemyDef def)
        {
            Def = def;
            Team = Team.Enemy;
            Hp = MaxHp = def.Hp;
            if (Anim == null) Anim = GetComponent<SpriteAnimator>();
            Anim.Set = SpriteLibrary.Load(def.SpriteDir, def.SpriteActor);
            Anim.Play("idle", true);
            if (Sr != null) Sr.color = new Color(0.72f, 0.56f, 0.34f); // earthy tint (art gap)
            Shadow.Attach(this, Shadow.MediumTier);
        }

        private void Update()
        {
            if (_state == State.Dead) return;
            float dt = Time.deltaTime;
            _cooldown = Mathf.Max(0f, _cooldown - dt);

            if (_stagger > 0f) { _stagger -= dt; Anim.Play("hurt", false); Steering.Separate(this); return; }

            var player = PlayerController.Instance;
            if (player == null || !player.Alive) { Anim.Play("idle", true); return; }

            Facing = player.WorldX >= WorldX ? 1 : -1;

            if (_state == State.Windup)
            {
                _windup -= dt;
                if (_windup <= 0f)
                {
                    // Lob at the spot the player was standing when the throw committed.
                    ArcProjectile.Spawn(Team.Enemy, WorldX, Z, _targetX, _targetZ,
                                        Def.Damage, new Color(0.65f, 0.55f, 0.45f), airTime: 0.9f);
                    Sfx.Play("knockdown_thud");
                    _cooldown = Def.AttackCooldown;
                    _state = State.Hold;
                }
                return;
            }

            // Hold the standoff, slide onto the player's row, keep separation.
            Steering.KeepDistance(this, player.WorldX, Z, Def.HoldDistance, Def.Speed, dt);
            float dz = player.Z - Z;
            Z += Mathf.Clamp(dz, -Def.Speed * dt, Def.Speed * dt);
            Steering.Separate(this);

            float dx = Mathf.Abs(player.WorldX - WorldX);
            if (dx <= Def.FireRange && _cooldown <= 0f)
            {
                _state = State.Windup;
                _windup = Def.WindupSeconds;
                _targetX = player.WorldX;
                _targetZ = player.Z;
                Anim.Play("attack_side", false, restart: true);
            }
            else
            {
                Anim.Play(Mathf.Abs(dz) > 0.35f || dx > Def.FireRange ? "walk" : "idle", true);
            }
        }

        public void ApplyStagger(float seconds)
        {
            if (_state == State.Dead) return;
            _stagger = seconds;
            _state = State.Hold;
            Anim.Play("hurt", false, restart: true);
        }

        public void KillBySpecial(Actor source) { _killedBySpecial = true; TakeDamage(9999f, source); }

        public override bool TakeDamage(float amount, Actor source)
        {
            if (_state != State.Dead && Alive) Anim.Play("hurt", false, restart: true);
            return base.TakeDamage(amount, source);
        }

        protected override void OnDeath(Actor source)
        {
            _state = State.Dead;
            Anim.Play("death", false, restart: true);
            Vfx.DeathBurst(WorldX, Z);
            Sfx.Play("knockdown_thud");
            if (!_killedBySpecial && Def.Loot != LootTier.None)
            {
                var kind = LootTable.Roll(Def.Loot);
                if (kind.HasValue) Pickup.SpawnWeapon(kind.Value, WorldX, Z);
            }
            Destroy(gameObject, 1.0f);
        }
    }
}
