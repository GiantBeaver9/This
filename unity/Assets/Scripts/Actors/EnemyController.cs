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

        // Staff status effects (§3.5): Ice freezes, Fire burns (DoT + walking-bomb on death),
        // Lightning stuns + slows. All decay on their own timers.
        private float _freezeTimer;
        private float _burnTimer, _burnTick;
        private float _slowTimer;
        private bool _ignited;         // once set on fire, dying pops a walking-bomb blast

        // Uppercut LAUNCH (ports the JS 'up' attack launch:true): pop up, arc under gravity, crash
        // down knocked-down. A satisfying juggle — the creator liked this from the JS build.
        private bool _launched;
        private float _airOffset, _launchVy, _launchVx;
        private const float LaunchGravity = 50f;   // wu/s^2

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
            _slowTimer = Mathf.Max(0f, _slowTimer - dt);

            // Launched by an uppercut: arc through the air (no AI), then crash down knocked-down.
            if (_launched) { TickLaunch(dt); return; }

            // Ice: frozen solid — no AI, icy tint, until it thaws.
            if (_freezeTimer > 0f)
            {
                _freezeTimer -= dt;
                ReleaseSlot();
                if (Sr != null) Sr.color = new Color(0.6f, 0.85f, 1f);
                Anim.Play("idle", true);
                return;
            }

            // Fire: burn DoT in 0.5 s ticks (6/s). A burning enemy is "ignited" → pops on death.
            if (_burnTimer > 0f)
            {
                _burnTimer -= dt;
                _burnTick -= dt;
                if (_burnTick <= 0f)
                {
                    _burnTick = 0.5f;
                    Vfx.HitSpark(WorldX, Z);
                    TakeDamage(3f, null);
                    if (!Alive) return; // burned to death → OnDeath pops the walking bomb
                }
            }

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
                else if (_state != State.Dead)
                    Sr.color = _burnTimer > 0f
                        ? Color.Lerp(new Color(1f, 0.5f, 0.12f), Color.white, Mathf.PingPong(Time.time * 12f, 1f))
                        : Color.white;   // zombies keep their normal colors — the hollow middle is the tell
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

        private float EffSpeed => Def.Speed * Tuning.EnemySpeedMult
            * (_zombie ? 0.6f : 1f)                 // zombies shamble
            * (_slowTimer > 0f ? 0.5f : 1f);        // lightning slow (§3.5)

        /// <summary>
        /// Apply a Staff element's status (§3.5) — the cast projectile already dealt its base
        /// damage; this adds the EFFECT only. Ice = freeze 3 s; Fire = ignite (burn DoT + a
        /// walking-bomb blast if it dies burning); Lightning = brief stun + a 3 s move-slow.
        /// </summary>
        public void ApplyStaffStatus(StaffElement el)
        {
            if (_state == State.Dead || !Alive) return;
            switch (el)
            {
                case StaffElement.Ice:
                    _freezeTimer = 3f;
                    break;
                case StaffElement.Fire:
                    _burnTimer = Mathf.Max(_burnTimer, 3f);
                    _ignited = true;
                    break;
                default: // Lightning
                    _slowTimer = 3f;
                    ApplyStagger(1.0f);
                    break;
            }
        }

        // The shared doodle-stick-figure zombie set (built once, reused by every zombie).
        private static SpriteLibrary.ActorSprites s_zombieDoodle;

        /// <summary>
        /// A gun headshot KILL rolled zombify (§3.1): don't die — rise again as a slower HOSTILE.
        /// It keeps hunting the player (still Team.Enemy), so headshotting is a real risk/reward,
        /// and it drops no loot when finally put down. Can't re-zombify.
        /// </summary>

        public void Zombify()
        {
            if (_zombie || _state == State.Dead) return;
            _zombie = true;
            ReleaseSlot();
            _state = State.Pursue;
            Hp = MaxHp = Mathf.Max(1f, MaxHp * 0.6f);   // comes back on its last legs

            // Zombie look (creator): a plain doodle STICK FIGURE like Phil — a hollow ring head + thin
            // stick body/arms/legs (see-through) — but WITHOUT Phil's top hat or pencil.
            if (Anim != null) Anim.Set = ZombieDoodleSet();
            if (Sr != null) Sr.color = Color.white;

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

        // ---- Zombie "doodle stick figure" sprite (like Phil, minus hat + pencil) --

        /// <summary>The shared zombie look: a hand-drawn STICK FIGURE — a hollow ring head and thin
        /// stick body/arms/legs (see-through), in Phil's black doodle style but with NO top hat and NO
        /// pencil. Built once (idle + a 2-frame shamble walk); all other clips fall back to idle.</summary>
        private static SpriteLibrary.ActorSprites ZombieDoodleSet()
        {
            if (s_zombieDoodle != null) return s_zombieDoodle;
            var set = new SpriteLibrary.ActorSprites { Actor = "zombie_doodle", ReverseAttacks = false };
            var f0 = DoodleFrame(0);
            var f1 = DoodleFrame(1);
            set.Clips["idle"] = new[] { f0 };
            set.Clips["walk"] = new[] { f0, f1 };   // subtle leg shift → a shamble
            set.Clips["hurt"] = new[] { f0 };
            s_zombieDoodle = set;
            return s_zombieDoodle;
        }

        /// <summary>Draw one black doodle stick figure frame (bottom-centre pivot). frame 1 shifts the
        /// legs for the walk shamble.</summary>
        private static Sprite DoodleFrame(int frame)
        {
            const int W = 30, H = 64;
            var px = new Color32[W * H];                 // transparent
            var ink = new Color32(24, 24, 28, 255);
            void Dot(int x, int y) { if (x >= 0 && x < W && y >= 0 && y < H) px[y * W + x] = ink; }
            void Line(int x0, int y0, int x1, int y1)
            {   // 2px-thick Bresenham
                int dx = Mathf.Abs(x1 - x0), dy = -Mathf.Abs(y1 - y0);
                int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1, err = dx + dy;
                while (true)
                {
                    Dot(x0, y0); Dot(x0 + 1, y0); Dot(x0, y0 + 1);
                    if (x0 == x1 && y0 == y1) break;
                    int e2 = 2 * err;
                    if (e2 >= dy) { err += dy; x0 += sx; }
                    if (e2 <= dx) { err += dx; y0 += sy; }
                }
            }

            int cx = 14;
            // Head: a HOLLOW ring (outline only → see-through).
            int hcx = cx, hcy = 50, r = 7;
            for (int a = 0; a < 360; a += 6)
            {
                float rad = a * Mathf.Deg2Rad;
                int x = hcx + Mathf.RoundToInt(Mathf.Cos(rad) * r);
                int y = hcy + Mathf.RoundToInt(Mathf.Sin(rad) * r);
                Dot(x, y); Dot(x + 1, y);
            }
            Line(cx, 42, cx, 18);                        // spine
            Line(cx, 40, cx - 8, 31); Line(cx, 40, cx + 8, 31); // arms
            if (frame == 0) { Line(cx, 18, cx - 7, 2); Line(cx, 18, cx + 7, 2); }   // stance
            else            { Line(cx, 18, cx - 3, 2); Line(cx, 18, cx + 9, 3); }   // mid-shamble stride

            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixels32(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.03f), Tuning.PixelsPerUnit);
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

        /// <summary>
        /// Uppercut launch (ports JS 'up' launch:true): pop the enemy into the air with an upward
        /// velocity + a little backward drift; it arcs under gravity and crashes down knocked-down.
        /// Heavy (H-weight) enemies resist — they just take a short stagger instead of flying.
        /// </summary>
        public void Launch(float upVel, float backVel)
        {
            if (_state == State.Dead) return;
            if (Def != null && Def.Weight == StaggerWeight.H) { ApplyStagger(0.4f); return; }
            ReleaseSlot();
            _launched = true;
            _launchVy = upVel;
            _launchVx = backVel;
            _airOffset = 0.01f;
            _state = State.Pursue;        // cleared to normal once it lands (via ApplyStagger)
            Anim.Play("hurt", false, restart: true);
            Vfx.HitSpark(WorldX, Z);
            Sfx.Play("air_hit");
        }

        private void TickLaunch(float dt)
        {
            _launchVy -= LaunchGravity * dt;
            _airOffset += _launchVy * dt;
            WorldX += _launchVx * dt;
            _launchVx = Mathf.MoveTowards(_launchVx, 0f, 8f * dt);   // drag out the backward drift
            if (_airOffset <= 0f)
            {
                _airOffset = 0f;
                _launched = false;
                Vfx.DeathBurst(WorldX, Z);         // dust puff on impact
                Sfx.Play("knockdown_thud");
                ApplyStagger(1.1f);                 // lands knocked down — a juggle/execute window
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();                       // Playfield.Place sets the ground position
            if (_airOffset > 0f)
            {
                var p = transform.position;
                p.y += _airOffset;                   // lift the sprite by its air height
                transform.position = p;
            }
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
            // Flinch on a hit — but NOT while winding up a punch: a committed swing plays through a
            // light hit so enemies actually TRADE blows with you (creator: "no back-and-forthing").
            // A heavy hit still interrupts by going through ApplyStagger/Launch, which sets the state.
            if (_state != State.Dead && _state != State.Stagger && _state != State.Windup && Alive)
                Anim.Play("hurt", false, restart: true);
            return base.TakeDamage(amount, source);
        }

        protected override void OnDeath(Actor source)
        {
            ReleaseSlot();
            _launched = false; _airOffset = 0f;   // don't leave a corpse hanging mid-launch
            _state = State.Dead;
            EnemySpawner.NotifyKill();
            Anim.Play("death", false, restart: true);
            Vfx.DeathBurst(WorldX, Z);
            Sfx.Play("knockdown_thud");

            // Fire walking-bomb (§3.5): an enemy that dies while ignited detonates, hitting
            // BOTH nearby enemies and the player (friendlyFire) — a chain-reaction payoff.
            if (_ignited)
            {
                Explosion.Blast(Team.Enemy, WorldX, Z, 1.6f, 15f, friendlyFire: true, except: this);
                Sfx.Play("grenade_explode");
                CameraShake.Add(CameraShake.Medium);
            }

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
