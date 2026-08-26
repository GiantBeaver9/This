using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Ninja AI (ENEMIES.md §2.9, TUNING §4 row 12 / §4.1). A mobile T3a harasser:
    /// throws <b>2 shuriken per volley</b> (straight, Z-aware <see cref="Projectile"/>,
    /// 12 dmg, 12 wu reach) on a 3 s cooldown, closes for a 22.5 melee slash inside
    /// reach, and <b>smoke-teleports</b> every 3 s (0.3 s smoke tell) to reposition
    /// ~4 wu from the player. Self-restocking — never runs dry, never needs fodder.
    /// Drops loot on death like <see cref="EnemyController"/> (nothing on a
    /// sniper-special kill). Spawns combat-ready.
    /// </summary>
    public sealed class NinjaController : Actor, ISpecialKillable, IStaggerable
    {
        private enum State { Pursue, MeleeWindup, MeleeRecover, SmokeTell, Stagger, Dead }

        private const float ShurikenDamage = 12f;   // §4 row 12
        private const float TeleportCooldown = 3f;   // §4 row 12
        private const float SmokeTell = 0.3f;        // §4.1
        private const float ShurikenGap = 0.14f;     // spacing between the 2 stars

        [System.NonSerialized] public EnemyDef Def;
        private State _state = State.Pursue;
        private float _stateTimer;
        private float _meleeCooldown;
        private float _shurikenTimer = 1.0f;
        private float _teleportTimer = TeleportCooldown;
        private float _stagger;
        private int _pendingStars;
        private float _starGap;
        private bool _killedBySpecial;

        public void Init(EnemyDef def)
        {
            Def = def;
            Team = Team.Enemy;
            Hp = MaxHp = def.Hp;
            if (Anim == null) Anim = GetComponent<SpriteAnimator>();
            Anim.Set = SpriteLibrary.Load(def.SpriteDir, def.SpriteActor);
            Anim.Play("idle", true);
            if (Sr != null) Sr.color = new Color(0.35f, 0.35f, 0.45f); // dark tint (art gap)
            Shadow.Attach(this, Shadow.MediumTier);
        }

        private void Update()
        {
            if (_state == State.Dead) return;
            float dt = Time.deltaTime;
            _meleeCooldown = Mathf.Max(0f, _meleeCooldown - dt);
            _shurikenTimer = Mathf.Max(0f, _shurikenTimer - dt);
            _teleportTimer = Mathf.Max(0f, _teleportTimer - dt);

            // Fire the queued second shuriken of a volley.
            if (_pendingStars > 0)
            {
                _starGap -= dt;
                if (_starGap <= 0f) { FireShuriken(); _pendingStars--; _starGap = ShurikenGap; }
            }

            var player = PlayerController.Instance;
            if (player == null || !player.Alive) { Anim.Play("idle", true); return; }
            Facing = player.WorldX >= WorldX ? 1 : -1;

            switch (_state)
            {
                case State.Stagger:
                    _stateTimer -= dt;
                    if (_stateTimer <= 0f) _state = State.Pursue;
                    Steering.Separate(this);
                    return;

                case State.SmokeTell:
                    _stateTimer -= dt;
                    if (_stateTimer <= 0f)
                    {
                        // Blink to a spot ~4 wu from the player on a random side/row.
                        int side = Random.value < 0.5f ? -1 : 1;
                        WorldX = player.WorldX + side * Def.HoldDistance;
                        Z = Mathf.Clamp(player.Z + Random.Range(-1.5f, 1.5f), 0f, Tuning.ZBandDepth);
                        Sfx.Play("dash_whoosh");
                        _teleportTimer = TeleportCooldown;
                        _state = State.Pursue;
                    }
                    return;

                case State.MeleeWindup:
                    _stateTimer -= dt;
                    if (_stateTimer <= 0f)
                    {
                        float dx = Mathf.Abs(player.WorldX - WorldX);
                        if (dx <= Def.Reach + 0.2f && Playfield.WithinZ(player.Z, Z, Tuning.HitboxZTolerance))
                            player.TakeDamage(Def.Damage, this);
                        _meleeCooldown = Def.AttackCooldown;
                        _state = State.MeleeRecover;
                        _stateTimer = 0.2f;
                    }
                    return;

                case State.MeleeRecover:
                    _stateTimer -= dt;
                    if (_stateTimer <= 0f) _state = State.Pursue;
                    return;

                case State.Pursue:
                    Pursue(player, dt);
                    break;
            }

            Steering.Separate(this);
        }

        private void Pursue(Actor player, float dt)
        {
            float dx = Mathf.Abs(player.WorldX - WorldX);
            bool inMelee = dx <= Def.Reach && Playfield.WithinZ(player.Z, Z, Tuning.HitboxZTolerance);

            // Reposition on cadence (the flit-around-the-player teleport).
            if (_teleportTimer <= 0f && !inMelee)
            {
                _state = State.SmokeTell;
                _stateTimer = SmokeTell;
                Vfx.DeathBurst(WorldX, Z); // smoke puff placeholder (art gap)
                Anim.Play("hurt", false, restart: true);
                return;
            }

            if (inMelee && _meleeCooldown <= 0f)
            {
                _state = State.MeleeWindup;
                _stateTimer = Def.WindupSeconds;
                Anim.Play("attack_side", false, restart: true);
                return;
            }

            // Throw a 2-star volley when roughly lined up on the player's row.
            bool linedUp = Playfield.WithinZ(player.Z, Z, 0.5f);
            if (linedUp && dx <= Def.FireRange && _shurikenTimer <= 0f && _pendingStars == 0)
            {
                _shurikenTimer = Def.FireInterval;
                Anim.Play("attack_side", false, restart: true);
                FireShuriken();
                _pendingStars = 1;      // one now, one after the gap
                _starGap = ShurikenGap;
                return;
            }

            // Otherwise drift toward the player.
            Steering.MoveToward(this, player.WorldX, player.Z, Def.Speed, Def.Reach * 0.85f, dt);
            Anim.Play("walk", true);
        }

        private void FireShuriken()
        {
            Vfx.MuzzleFlash(WorldX + Facing * 0.4f, Z, Facing);
            Sfx.Play("boomerang_throw");
            Projectile.Spawn(Team.Enemy, WorldX + Facing * 0.4f, Z, Facing,
                             Def.ProjectileSpeed, ShurikenDamage, new Color(0.85f, 0.85f, 0.9f));
        }

        public void ApplyStagger(float seconds)
        {
            if (_state == State.Dead) return;
            _state = State.Stagger;
            _stateTimer = seconds;
            Anim.Play("hurt", false, restart: true);
        }

        public void KillBySpecial(Actor source) { _killedBySpecial = true; TakeDamage(9999f, source); }

        public override bool TakeDamage(float amount, Actor source)
        {
            if (_state != State.Dead && _state != State.Stagger && Alive)
                Anim.Play("hurt", false, restart: true);
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
