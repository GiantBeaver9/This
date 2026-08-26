using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Melee enemy AI with the true beat-'em-up spacing rhythm (GAMEPLAY_LOOP §8):
    /// most of the crowd HOLDS a standoff ring and circles; only a few hold an
    /// attacker slot and commit a telegraphed swing; and after swinging an enemy
    /// BACKS OFF before re-approaching. Each pursuer biases to a different depth so
    /// the horde spreads across Z-rows instead of stacking. This keeps a big crowd
    /// readable and fair (you never eat several overlapping hits at once), while
    /// still feeling like a swarm. On death it drops a decaying weapon (nothing on
    /// a sniper-special kill).
    /// </summary>
    public sealed class EnemyController : Actor, ISpecialKillable, IStaggerable
    {
        private enum State { Pursue, Windup, Recover, Backoff, Stagger, Dead }

        [System.NonSerialized] public EnemyDef Def;

        private static int _activeAttackers;
        private static int _reshuffleGen;      // bumps every EnemyReshuffleSeconds to rotate who attacks
        private static float _nextReshuffleAt;
        private State _state = State.Pursue;
        private float _stateTimer;
        private float _cooldown;
        private bool _hasSlot;
        private int _slotGen;          // the reshuffle generation this slot was taken in
        private bool _killedBySpecial;
        private float _zBias;          // this pursuer's preferred depth offset (Z-spread)
        private bool _relentless;      // swarmers rush without standoff/backoff
        private bool _zombie;          // rose again from a gun headshot (§3.1) — a slower green HOSTILE

        public void Init(EnemyDef def)
        {
            Def = def;
            Team = Team.Enemy;
            Hp = MaxHp = def.Hp * DifficultySettings.EnemyHpMult;
            _relentless = def.Id == "swarmer";
            _zBias = Random.Range(-Tuning.ZSpreadBias, Tuning.ZSpreadBias);
            var set = SpriteLibrary.Load(def.SpriteDir, def.SpriteActor);
            if (Anim == null) Anim = GetComponent<SpriteAnimator>();
            Anim.Set = set;
            Anim.Play("idle", true);
            Shadow.Attach(this, def.Id == "swarmer" ? Shadow.SmallTier : Shadow.MediumTier);
        }

        private void Update()
        {
            if (_state == State.Dead) return;
            float dt = Time.deltaTime;
            _cooldown = Mathf.Max(0f, _cooldown - dt);

            // Global reshuffle: every few seconds pass attack priority so different
            // enemies get a turn (idempotent — first enemy past the mark advances it).
            if (Time.time >= _nextReshuffleAt)
            {
                _reshuffleGen++;
                _nextReshuffleAt = Time.time + Tuning.EnemyReshuffleSeconds;
            }

            var player = PlayerController.Nearest(WorldX, Z);
            if (player == null || !player.Alive) { Anim.Play("idle", true); return; }

            Facing = player.WorldX >= WorldX ? 1 : -1;
            float targetZ = Mathf.Clamp(player.Z + _zBias, 0f, Tuning.ZBandDepth);

            // Telegraph: flash the wind-up so an incoming hit is unmistakable.
            if (Sr != null)
            {
                if (_state == State.Windup)
                {
                    float t = Mathf.PingPong(Time.time * 9f, 1f);
                    Sr.color = Color.Lerp(new Color(1f, 0.85f, 0.35f), new Color(1f, 0.25f, 0.2f), t);
                }
                else if (_state != State.Dead) Sr.color = _zombie ? ZombieTint : Color.white;
            }

            switch (_state)
            {
                case State.Stagger:
                    _stateTimer -= dt;
                    if (_stateTimer <= 0f) _state = State.Pursue;
                    break;

                case State.Windup:
                    _stateTimer -= dt;
                    if (_stateTimer <= 0f)
                    {
                        float dx = Mathf.Abs(player.WorldX - WorldX);
                        if (dx <= Def.Reach + 0.35f && Playfield.WithinZ(player.Z, Z, Tuning.EnemyAttackZTolerance))
                            player.TakeDamage(Def.Damage * DifficultySettings.EnemyDamageMult, this);
                        _cooldown = Def.AttackCooldown;
                        _state = State.Recover;
                        _stateTimer = 0.2f;
                    }
                    break;

                case State.Recover:
                    _stateTimer -= dt;
                    if (_stateTimer <= 0f)
                    {
                        ReleaseSlot();
                        if (_relentless) { _state = State.Pursue; }
                        else { _state = State.Backoff; _stateTimer = Random.Range(Tuning.BackoffMin, Tuning.BackoffMax); }
                    }
                    break;

                case State.Backoff:
                    _stateTimer -= dt;
                    Backoff(player, targetZ, dt);
                    if (_stateTimer <= 0f) _state = State.Pursue;
                    break;

                case State.Pursue:
                    Pursue(player, targetZ, dt);
                    break;
            }

            Separate();
        }

        private void Pursue(Actor player, float standoffZ, float dt)
        {
            // Reshuffle: if we held a slot into a new generation, yield it so someone else attacks.
            if (_hasSlot && _slotGen != _reshuffleGen) ReleaseSlot();

            float dx = player.WorldX - WorldX;

            // Eligible = allowed to go for the hit this cycle. Eligible enemies home to the
            // player's EXACT depth row and close INSIDE reach so they actually connect; the
            // rest hold the standoff ring off-row (the readable circling crowd).
            bool eligible = _cooldown <= 0f && (_hasSlot || _activeAttackers < Tuning.MaxAttackers);

            bool inAttackRange = Mathf.Abs(dx) <= Def.Reach
                              && Playfield.WithinZ(player.Z, Z, Tuning.EnemyAttackZTolerance);

            if (eligible && inAttackRange && TryTakeSlot())
            {
                _state = State.Windup;
                _stateTimer = Def.WindupSeconds;
                Anim.Play("attack_side", false, restart: true);
                return;
            }

            float targetZ = eligible ? player.Z : standoffZ;
            float hold = _relentless ? Def.Reach * 0.6f
                       : eligible ? Def.Reach * 0.7f     // close well inside reach so the swing lands
                       : Tuning.StandoffRing;

            float side = Mathf.Sign(player.WorldX - WorldX);
            if (side == 0f) side = -Facing;
            float desiredX = player.WorldX - side * hold;
            MoveTo(desiredX, targetZ, EffSpeed, dt);
        }

        private float EffSpeed => Def.Speed * Tuning.EnemySpeedMult * (_zombie ? 0.6f : 1f); // zombies shamble

        private static readonly Color ZombieTint = new(0.55f, 0.9f, 0.5f);

        /// <summary>
        /// A gun headshot KILL rolled zombify (§3.1): don't die — rise again as a slower green
        /// HOSTILE. It keeps hunting the player (still Team.Enemy), so headshotting is a real
        /// risk/reward, and it drops no loot when finally put down. Can't re-zombify.
        /// </summary>
        public void Zombify()
        {
            if (_zombie || _state == State.Dead) return;
            _zombie = true;
            ReleaseSlot();
            _state = State.Pursue;
            Hp = MaxHp = Mathf.Max(1f, MaxHp * 0.6f);   // comes back on its last legs
            if (Sr != null) Sr.color = ZombieTint;
            Anim.Play("hurt", false, restart: true);     // a lurch back to its feet
            Vfx.DeathBurst(WorldX, Z);                    // gory reanimation puff
            Sfx.Play("pod_spawn_burst");
        }

        /// <summary>Roll zombify against a would-be-lethal gun hit. Returns true if it rose again
        /// (the shot is "spent" on the head — the caller should NOT also apply the killing damage).</summary>
        public bool TryZombifyOnLethal(float incomingDamage, float chance)
        {
            if (_zombie || _state == State.Dead || !Alive) return false;
            if (Hp > incomingDamage) return false;        // only a KILLING shot can zombify
            if (Random.value > chance) return false;
            Zombify();
            return true;
        }

        private void Backoff(Actor player, float targetZ, float dt)
        {
            float side = Mathf.Sign(WorldX - player.WorldX);
            if (side == 0f) side = -Facing;
            float awayX = player.WorldX + side * (Tuning.StandoffRing + 0.5f);
            MoveTo(awayX, targetZ, EffSpeed * Tuning.BackoffSpeedMult, dt);
        }

        private void MoveTo(float tx, float tz, float speed, float dt)
        {
            float ex = tx - WorldX, ez = tz - Z;
            float d = Mathf.Sqrt(ex * ex + ez * ez);
            if (d > 0.15f)
            {
                WorldX += ex / d * speed * dt;
                Z += ez / d * speed * dt;
                Anim.Play("walk", true);
            }
            else Anim.Play("idle", true);
        }

        /// <summary>Soft hard-separation: push apart from other enemies within 1.0 wu.</summary>
        private void Separate()
        {
            foreach (var a in Actor.All)
            {
                if (a == this || !a.Alive || a.Team != Team.Enemy) continue;
                float dx = WorldX - a.WorldX;
                float dz = Z - a.Z;
                float d = Mathf.Sqrt(dx * dx + dz * dz);
                if (d < Tuning.PursuerSeparation && d > 0.0001f)
                {
                    float push = (Tuning.PursuerSeparation - d) * 0.5f;
                    WorldX += (dx / d) * push;
                    Z += (dz / d) * push;
                }
            }
        }

        private bool TryTakeSlot()
        {
            if (_hasSlot) return true;
            if (_activeAttackers >= Tuning.MaxAttackers) return false;
            _activeAttackers++;
            _hasSlot = true;
            _slotGen = _reshuffleGen;
            return true;
        }

        private void ReleaseSlot()
        {
            if (!_hasSlot) return;
            _activeAttackers = Mathf.Max(0, _activeAttackers - 1);
            _hasSlot = false;
        }

        public void KillBySpecial(Actor source)
        {
            _killedBySpecial = true;
            TakeDamage(9999f, source);
        }

        public void ApplyStagger(float seconds)
        {
            if (_state == State.Dead) return;
            ReleaseSlot();
            _state = State.Stagger;
            _stateTimer = seconds;
            Anim.Play("hurt", false, restart: true);
        }

        public override bool TakeDamage(float amount, Actor source)
        {
            if (_state != State.Dead && _state != State.Stagger && Alive)
                Anim.Play("hurt", false, restart: true);
            return base.TakeDamage(amount, source);
        }

        protected override void OnDeath(Actor source)
        {
            ReleaseSlot();
            _state = State.Dead;
            EnemySpawner.NotifyKill();
            Anim.Play("death", false, restart: true);
            Vfx.DeathBurst(WorldX, Z);
            Sfx.Play("knockdown_thud");

            if (!_killedBySpecial && !_zombie)   // a re-killed zombie already gave its loot the first time
            {
                if (Def.Loot != LootTier.None)
                {
                    var kind = LootTable.Roll(Def.Loot);
                    if (kind.HasValue) Pickup.SpawnWeapon(kind.Value, WorldX, Z);
                }
                HealPickup.MaybeDrop(WorldX, Z);
            }
            Destroy(gameObject, 1.0f); // let the death frames play out
        }
    }
}
