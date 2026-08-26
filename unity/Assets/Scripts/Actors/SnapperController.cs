using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Snapper / Sword-Maker AI (ENEMIES.md §2.4 / §6, TUNING §4 row 6). A T2
    /// melee-zoner that fights with a long-reach sword (1.7 wu, windup 175 ms).
    /// Its sword decays after 8 swings; when disarmed it becomes a keep-away
    /// support that <b>calls in a tier-1</b> every 4 s (max 2 pending), then
    /// "snaps" an adjacent T1 to re-arm (the T1 is consumed — modelled as a
    /// special-kill so no loot and its attacker-slot releases cleanly). Never
    /// fights unarmed. Killing it drops its §6.1 T2 pool roll like any enemy.
    /// (The guaranteed fresh-Sword drop, ENEMIES §6, is a loot-table nicety left
    /// to the drop system — noted in _INTEGRATION.md.)
    /// </summary>
    public sealed class SnapperController : Actor, ISpecialKillable, IStaggerable
    {
        private enum State { Pursue, Windup, Recover, KeepAway, Stagger, Dead }

        private const int SwordHits = 8;         // TUNING §4 row 6: decays after 8 hits
        private const float CallInterval = 4f;   // calls a T1 every 4 s when unarmed
        private const int MaxPending = 2;
        private const float SnapRange = 1.6f;    // grab an adjacent T1 within this
        private const float KeepAwayDist = 3.0f;

        [System.NonSerialized] public EnemyDef Def;
        private State _state = State.Pursue;
        private float _stateTimer;
        private float _cooldown;
        private float _callTimer;
        private int _pending;
        private int _swordLeft = SwordHits;
        private bool _killedBySpecial;

        public void Init(EnemyDef def)
        {
            Def = def;
            Team = Team.Enemy;
            Hp = MaxHp = def.Hp;
            if (Anim == null) Anim = GetComponent<SpriteAnimator>();
            Anim.Set = SpriteLibrary.Load(def.SpriteDir, def.SpriteActor);
            Anim.Play("idle", true);
            if (Sr != null) Sr.color = new Color(0.6f, 0.72f, 0.85f); // steel tint (art gap)
            Shadow.Attach(this, Shadow.MediumTier);
        }

        private bool Armed => _swordLeft > 0;

        private void Update()
        {
            if (_state == State.Dead) return;
            float dt = Time.deltaTime;
            _cooldown = Mathf.Max(0f, _cooldown - dt);
            _callTimer = Mathf.Max(0f, _callTimer - dt);

            var player = PlayerController.Nearest(WorldX, Z);
            if (player == null || !player.Alive) { Anim.Play("idle", true); return; }
            Facing = player.WorldX >= WorldX ? 1 : -1;

            switch (_state)
            {
                case State.Stagger:
                    _stateTimer -= dt;
                    if (_stateTimer <= 0f) _state = Armed ? State.Pursue : State.KeepAway;
                    break;

                case State.Windup:
                    _stateTimer -= dt;
                    if (_stateTimer <= 0f)
                    {
                        float dx = Mathf.Abs(player.WorldX - WorldX);
                        if (dx <= Def.Reach + 0.2f && Playfield.WithinZ(player.Z, Z, Tuning.HitboxZTolerance))
                            player.TakeDamage(Def.Damage, this);
                        if (--_swordLeft <= 0) { _state = State.KeepAway; break; } // sword decayed
                        _cooldown = Def.AttackCooldown;
                        _state = State.Recover;
                        _stateTimer = 0.2f;
                    }
                    break;

                case State.Recover:
                    _stateTimer -= dt;
                    if (_stateTimer <= 0f) _state = State.Pursue;
                    break;

                case State.Pursue:
                    if (!Armed) { _state = State.KeepAway; break; }
                    Pursue(player, dt);
                    break;

                case State.KeepAway:
                    KeepAway(player, dt);
                    break;
            }

            Steering.Separate(this);
        }

        private void Pursue(Actor player, float dt)
        {
            bool inRange = Mathf.Abs(player.WorldX - WorldX) <= Def.Reach
                        && Playfield.WithinZ(player.Z, Z, Tuning.HitboxZTolerance);
            if (inRange && _cooldown <= 0f)
            {
                _state = State.Windup;
                _stateTimer = Def.WindupSeconds;
                Anim.Play("attack_side", false, restart: true);
                return;
            }
            if (DistanceTo(player) > Def.Reach * 0.9f)
            {
                Steering.MoveToward(this, player.WorldX, player.Z, Def.Speed, Def.Reach * 0.85f, dt);
                Anim.Play("walk", true);
            }
            else Anim.Play("idle", true);
        }

        /// <summary>Disarmed: hold distance from the player, call in and snap T1s to re-arm.</summary>
        private void KeepAway(Actor player, float dt)
        {
            Steering.KeepDistance(this, player.WorldX, player.Z, KeepAwayDist, Def.Speed, dt);
            Anim.Play("walk", true);

            // Snap an adjacent tier-1 (consume it) to re-arm.
            var t1 = NearestCallable();
            if (t1 != null && DistanceTo(t1) <= SnapRange)
            {
                if (t1 is ISpecialKillable k) k.KillBySpecial(this); // consumed -> becomes the blade
                else Destroy(t1.gameObject);
                _pending = Mathf.Max(0, _pending - 1);
                _swordLeft = SwordHits;
                _state = State.Pursue;
                Sfx.Play("weapon_pickup");
                return;
            }

            // No T1 to snap: call one in on cadence, up to the pending cap.
            if (_pending < MaxPending && _callTimer <= 0f)
            {
                CallInRegular(WorldX + Facing * -1.5f, Z);
                _pending++;
                _callTimer = CallInterval;
                Sfx.Play("armed_ready_chime");
            }
        }

        /// <summary>Nearest live Regular-Melee (tier-1) this Snapper may snap.</summary>
        private Actor NearestCallable()
        {
            Actor best = null; float bestD = float.MaxValue;
            foreach (var a in Actor.All)
            {
                if (!a.Alive || a == this || a.Team != Team.Enemy) continue;
                if (a is EnemyController e && e.Def != null && e.Def.Id == "regular_melee")
                {
                    float d = DistanceTo(a);
                    if (d < bestD) { bestD = d; best = a; }
                }
            }
            return best;
        }

        private static void CallInRegular(float x, float z)
        {
            var go = new GameObject("enemy_regular");
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<SpriteAnimator>();
            var e = go.AddComponent<EnemyController>();
            e.WorldX = x;
            e.Z = Mathf.Clamp(z, 0f, Tuning.ZBandDepth);
            e.Init(EnemyDef.RegularMelee());
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
            if (!_killedBySpecial)
            {
                // Snapper "is" its weapon -> guaranteed fresh Sword, plus the T2 roll.
                Pickup.SpawnWeapon(WeaponKind.Sword, WorldX, Z);
                if (Def.Loot != LootTier.None)
                {
                    var kind = LootTable.Roll(Def.Loot);
                    if (kind.HasValue) Pickup.SpawnWeapon(kind.Value, WorldX + 0.5f, Z);
                }
            }
            Destroy(gameObject, 1.0f);
        }
    }
}
