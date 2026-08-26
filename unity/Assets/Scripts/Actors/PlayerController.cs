using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The player's base kit (PLAYER.md, COMBOS.md, TUNING §2), keyboard-first:
    /// LEFT HAND = WASD movement on the X/Z band (double-tap or LeftShift to dash;
    /// double-tap/LeftShift again IN THE AIR = a one-per-jump air-dash; Space to
    /// jump, E to fire a looted gun, F to pick up the nearest ground weapon);
    /// RIGHT HAND = arrow keys for 8-DIRECTIONAL attacks that resolve to the four
    /// verbs by DOMINANT CARDINAL (horizontal wins ties, PLAYER.md §3): any arrow
    /// with a horizontal component is a SIDE strike (and sets facing); only a
    /// straight Up/Down gives the up/down strike. Side presses drive the PRIMED
    /// string — P1→P2 connect PRIMES it, the next directional press is the SWEEP
    /// (knockdown / up-launch), then a tap toward the downed enemy is the FINISHER
    /// (auto-acquires the closest within 5 wu and steps onto it, COMBOS.md).
    /// Attacking WHILE AIRBORNE plays the AIR variant (side/up-launcher/down-spike,
    /// Z stays locked); attacking DURING A GROUND DASH is a dash attack (0 damage,
    /// weight-stagger only). With a ranged weapon equipped, a fresh horizontal press
    /// fires it. F picks up, E uses the item, Q fires the special. All timings use the §2.5 frame
    /// data. Combat hits build the meter; a full meter fires the sniper.
    /// </summary>
    public sealed class PlayerController : Actor
    {
        // ---- Multi-player roster (local co-op) -------------------------------
        /// <summary>Every live player, in join order (All[0] = P1). Replaces the old singleton.
        /// Intentionally hides <see cref="Actor.All"/> within PlayerController scope; the base
        /// list is always referenced as <c>Actor.All</c> where needed.</summary>
        public static new readonly List<PlayerController> All = new();

        /// <summary>The primary player (P1) — first in the roster. Camera/stage anchors use this.</summary>
        public static PlayerController Primary => All.Count > 0 ? All[0] : null;

        /// <summary>Back-compat alias: legacy readers of <c>Instance</c> get the primary player.</summary>
        public static PlayerController Instance => Primary;

        /// <summary>True if any player is alive (stage-clear / game-over gates read this).</summary>
        public static bool AnyAlive
        {
            get { foreach (var p in All) if (p != null && p.Alive) return true; return false; }
        }

        /// <summary>Nearest ALIVE player to (worldX,z); falls back to <see cref="Primary"/> if none alive.</summary>
        public static PlayerController Nearest(float worldX, float z)
        {
            PlayerController best = null;
            float bestSq = float.MaxValue;
            foreach (var p in All)
            {
                if (p == null || !p.Alive) continue;
                float dx = p.WorldX - worldX, dz = p.Z - z;
                float d = dx * dx + dz * dz;
                if (d < bestSq) { bestSq = d; best = p; }
            }
            return best ?? Primary;
        }

        /// <summary>Midpoint X of the living players (single-player = that player's X).</summary>
        public static float MidX()
        {
            float mn = float.MaxValue, mx = float.MinValue;
            foreach (var p in All)
            {
                if (p == null || !p.Alive) continue;
                if (p.WorldX < mn) mn = p.WorldX;
                if (p.WorldX > mx) mx = p.WorldX;
            }
            if (mn == float.MaxValue) { var pr = Primary; return pr != null ? pr.WorldX : 0f; }
            return (mn + mx) * 0.5f;
        }

        // ---- Per-player input source (keyboard / gamepad) --------------------
        private IPlayerInput _input;
        /// <summary>Inject this player's control surface (call after <see cref="Configure"/>/<see cref="Init"/>).</summary>
        public void SetInput(IPlayerInput src) => _input = src ?? new KeyboardInput();

        [System.NonSerialized] public Weapon CurrentWeapon = Weapon.Fists();
        public readonly SpecialMeter Meter = new();
        [System.NonSerialized] public CharacterDef Character;   // stat multipliers + special

        // Special-driven buffs
        private float _invuln;             // Werewolf transform i-frames, etc.
        private float _dmgBuffMult = 1f;   // Underdog Vaporize +20% window
        public static bool GodMode;        // TEMP DEBUG: K toggles invincible + infinite special
        private float _dmgBuffTimer;

        // Movement / dash / jump
        private bool _dashing;
        private bool _dashPushing;   // true while the dash is actually shoving an enemy → shoulder-charge anim
        private float _dashTimer, _dashCooldown, _dashDirX, _dashDirZ;
        private bool _airborne;
        private float _jumpTimer, _jumpOffset;
        private bool _airDashing;          // one X-only air-dash per airtime (TUNING §2.2)
        private float _airDashTimer, _airDashDirX;
        private bool _airDashUsed;
        private readonly HashSet<Actor> _dashHit = new(); // enemies already staggered by this dash
        private float _dashCharges = Tuning.DashMaxCharges; // 3-per-5s rate limit (anti-spam)
        private float _hitstun;
        private float _weaponReady;       // warm-up countdown before a looted weapon can fire
        private float _fireLock;          // brief root-in-place after firing a ranged weapon (anti-spam)
        private float _aimTimer;          // >0 = winding up an aimed shot (pistol); the shot leaves at 0
        private bool _wasArmed;            // edge-detect the meter arming for the chime

        // Downed / respawn (shared-life system). On death we don't destroy the player;
        // if a team life is available we respawn them after a short beat.
        private bool _awaitingRespawn;
        private float _respawnTimer;

        // Shield Rush (PLAYER.md §2, TUNING §2.3): a forward double-tap INTO a grabbable
        // enemy grabs it as a moving damage-sponge and rushes forward; otherwise the
        // same input falls through to a normal dash (it never no-ops).
        private bool _shieldRushing;
        private Actor _shield;            // the grabbed enemy (held just ahead)
        private float _shieldSoaked;      // cumulative dmg absorbed this rush (cap = ShieldRushSoakMax)
        private float _shieldRushCooldown;
        private float _rushStartX;        // grab point, for the max-travel cap
        private float _rushTime;          // elapsed, for the min-commit before release cancels
        private int _rushDirX;            // +1 / -1 travel direction

        private const float ShieldRushRange = 2.0f;          // grab target ahead within 2.0 wu (§2.3)
        private const float ShieldRushAheadZ = 0.8f;         // "directly ahead" depth window (TUNING §1)
        private const float ShieldRushSpeed = 9.0f;          // wu/s — faster than run, closes gaps (§2.3)
        private const float ShieldRushMaxDist = 8.0f;        // hard travel cap (§2.3 term (c))
        private const float ShieldRushSoakMax = 40f;         // damage budget the shield eats (§2.3)
        private const float ShieldRushShove = 1.0f;          // shove the enemy forward on release (§2.3)
        private const float ShieldRushReleaseStagger = 0.55f; // M-stagger on release (§2.3)
        private const float ShieldRushCooldown = 1.5f;       // starts when the rush ends (§2.3)
        private const float ShieldRushMinCommit = 0.15f;     // ignore forward-release for this long (feel)

        // Attack state machine
        private enum Phase { None, Startup, Active, Recovery }
        private enum AttackKind { Side, Sweep, Finisher, Up, Down, AirSide, AirUp, AirDown, Dash }
        private enum AttackDir { Left, Right, Up, Down }
        private Phase _phase = Phase.None;
        private AttackKind _attackKind = AttackKind.Side;
        private int _combo = -1;          // Side string index: 0=P1 1=P2 (sweep/finisher are their own kinds)
        private float _phaseTimer;
        private bool _hitResolved;

        // Primed-combo state (COMBOS.md, TUNING §2.5): P1→P2 connect PRIMES the
        // sweep; the sweep landing arms the finisher on the downed enemy.
        private bool _primed;             // next directional press = the sweep
        private float _primedTimer;
        private bool _finisherReady;      // sweep connected: a tap finishes the downed target
        private float _finisherTimer;
        private bool _p1Connected, _p2Connected, _sweepConnected;
        private bool _bufferedAttack;     // a press buffered during startup/active/recovery
        private AttackDir _bufferedDir;

        private const float PrimeWindow = 0.35f;    // COMBOS §1 same-direction double-tap window
        private const float FinisherWindow = 1.2f;  // TUNING §2.6 knockdown duration
        private const float FinisherAcquire = 5.0f; // PLAYER.md §3 finisher auto-acquire radius
        private const float PickupRadius = 0.9f;    // F grab reach (PLAYER.md §2)

        // Double-tap dash tracking (WASD)
        private float _tapA, _tapD, _tapW, _tapS;
        private const float DoubleTapWindow = 0.28f;

        public event System.Action<int> SpecialFired; // tier

        // ---- Tutorial detection hooks (read-only observers; see TutorialController) ----
        /// <summary>Fires the frame a grounded dash starts (the dash-plow shove). Tutorial gate.</summary>
        public event System.Action Dashed;
        /// <summary>Fires when a finisher/execute connects on a target (ResolveFinisher). Tutorial gate.</summary>
        public event System.Action FinisherLanded;
        /// <summary>Fires when a weapon is equipped (e.g. a ground pickup). Lets the tutorial pop the "press E" prompt.</summary>
        public event System.Action<WeaponKind> WeaponEquipped;

        protected override void Awake()
        {
            base.Awake();
            if (!All.Contains(this)) All.Add(this);
            _input ??= new KeyboardInput();   // P1 default; GameFlow may replace via SetInput
            Team = Team.Player;
            Hp = MaxHp = Tuning.PlayerMaxHp;
            Character ??= CharacterDef.Tactical();
        }

        /// <summary>Pick the playable character (stats + special). Call before <see cref="Init"/>.</summary>
        public void Configure(CharacterDef def) => Character = def ?? CharacterDef.Tactical();

        public void Init()
        {
            Character ??= CharacterDef.Tactical();
            var set = SpriteLibrary.Load(Character.SpriteDir, Character.SpriteActor);
            if (Anim == null) Anim = GetComponent<SpriteAnimator>();
            Anim.Set = set;
            Anim.Play("idle", true);
            ScaleMult = Character.Scale;   // Bert stands short
            Shadow.Attach(this, Shadow.MediumTier);
        }

        public float DashCharges => _dashCharges;         // for the HUD (0..DashMaxCharges)

        public void Heal(int amount) => Hp = Mathf.Min(MaxHp, Hp + amount);

        public void SetInvuln(float seconds) => _invuln = Mathf.Max(_invuln, seconds);
        public void SetDamageBuff(float mult, float seconds) { _dmgBuffMult = mult; _dmgBuffTimer = seconds; }

        private void OnDestroy() { All.Remove(this); }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Downed: tick the respawn beat (the only thing a dead player does).
            if (!Alive)
            {
                if (_awaitingRespawn)
                {
                    _respawnTimer -= dt;
                    if (_respawnTimer <= 0f) Respawn();
                }
                return;
            }

            _input.Tick();   // latch this frame's analog edges before any read
            _dashCooldown = Mathf.Max(0f, _dashCooldown - dt);
            _shieldRushCooldown = Mathf.Max(0f, _shieldRushCooldown - dt);
            _dashCharges = Mathf.Min(Tuning.DashMaxCharges,
                _dashCharges + (Tuning.DashMaxCharges / Tuning.DashChargeWindow) * dt);
            _hitstun = Mathf.Max(0f, _hitstun - dt);
            _weaponReady = Mathf.Max(0f, _weaponReady - dt);
            _fireLock = Mathf.Max(0f, _fireLock - dt);

            // Aimed-shot wind-up (pistol §3.1): the trigger starts a brief aim; the shot leaves when
            // the timer runs out (more precise than a snap shot). A hit mid-aim cancels it.
            if (_aimTimer > 0f)
            {
                if (_hitstun > 0f || !Alive) { _aimTimer = 0f; }
                else
                {
                    _aimTimer -= dt;
                    if (_aimTimer <= 0f)
                    {
                        _aimTimer = 0f;
                        if (CurrentWeapon.IsRanged && CurrentWeapon.FireImpl != null && CurrentWeapon.FireCooldown <= 0f)
                            CurrentWeapon.FireImpl(this);   // release the aimed shot (fires, spends ammo, sets cd)
                    }
                }
            }
            _invuln = Mathf.Max(0f, _invuln - dt);
            if (_dmgBuffTimer > 0f) { _dmgBuffTimer -= dt; if (_dmgBuffTimer <= 0f) _dmgBuffMult = 1f; }
            CurrentWeapon.Tick(dt);
            Meter.Tick(dt);
            TickComboWindows(dt);

            // ---- TEMP DEBUG keys (deliberate, non-gameplay keys; delete before ship) ----
            // Keyboard-only + P1-only so a second player's Update doesn't double-fire them.
            if (this == Primary)
            {
                if (Input.GetKeyDown(KeyCode.I)) Meter.Award(Tuning.MeterMax);          // I = fill special once
                if (Input.GetKeyDown(KeyCode.O)) CampaignRunner.Instance?.SkipToNext(); // O = skip to next stage
                if (Input.GetKeyDown(KeyCode.K))                                        // K = toggle GOD MODE
                {
                    GodMode = !GodMode;
                    Sfx.Play(GodMode ? "armed_ready_chime" : "cancel");
                }
                if (Input.GetKeyDown(KeyCode.J))                                        // J = cycle weapon (debug)
                {
                    var kinds = (WeaponKind[])System.Enum.GetValues(typeof(WeaponKind));
                    int next = (System.Array.IndexOf(kinds, CurrentWeapon.Kind) + 1) % kinds.Length;
                    Equip(kinds[next]);                                                 // fresh full-durability weapon
                    Sfx.Play("armed_ready_chime");
                }
            }
            if (GodMode) { Hp = MaxHp; Meter.Award(Tuning.MeterMax); }     // invincible + infinite special (both players)
            if (Meter.CanFire && !_wasArmed) Sfx.Play("armed_ready_chime");
            _wasArmed = Meter.CanFire;

            ReadDashTaps();

            if (_hitstun > 0f) { Anim.Play("hurt", false); return; }

            // Shield Rush fully commits the player: it drives its own motion and locks
            // out attacks/jumps/dashes/normal movement until it ends.
            if (_shieldRushing) { TickShieldRush(dt); Anim.Play("dash", false); return; }

            TickAttack(dt);
            TickJump(dt);
            TickDash(dt);
            TickAirDash(dt);

            // Movement is locked during a dash and during a GROUNDED attack's
            // startup+active; air attacks keep their mid-air steering (§2.5).
            bool groundedSwing = !_airborne && (_phase == Phase.Startup || _phase == Phase.Active);
            bool moveLocked = _dashing || _airDashing || groundedSwing;
            if (!moveLocked) Move(dt);

            HandleActionInput();
            UpdateAnimation();
        }

        // ---- Movement (left hand: keyboard WASD / gamepad left stick) ---------
        private float MoveX() => _input.MoveX;
        private float MoveZ() => _input.MoveZ;

        /// <summary>Holding UP (into the far row) — used by the grenade's anti-air lob (§3.2).</summary>
        public bool HoldingUp => _input != null && _input.MoveZ > 0.4f;
        /// <summary>Holding DOWN (toward the near row) — Ball &amp; Chain Ground-Zero shape (§3.3).</summary>
        public bool HoldingDown => _input != null && _input.MoveZ < -0.4f;
        /// <summary>Holding BACK (opposite the way you face) — Ball &amp; Chain Full-Swing shape (§3.3).</summary>
        public bool HoldingBack => _input != null && (Facing > 0 ? _input.MoveX < -0.4f : _input.MoveX > 0.4f);

        private void Move(float dt)
        {
            float ix = MoveX(), iz = MoveZ();
            if (ix != 0) Facing = ix > 0 ? 1 : -1;
            if (_fireLock > 0f) return;   // ROOTED while firing a ranged weapon — a 0.2s commitment so you can't run-and-gun spam (creator ruling)

            if (_airborne)
            {
                WorldX += ix * Tuning.AirSpeed * dt; // X air-control only; Z is locked
                return;
            }

            if (ix == 0 && iz == 0) return;
            Vector2 dir = new Vector2(ix, iz).normalized;
            float speed = (_input.WalkHeld ? Tuning.WalkSpeed : Tuning.RunSpeed) * Character.MoveSpeedMult;
            WorldX += dir.x * speed * dt;
            Z += dir.y * speed * dt;

            // Weave around static obstacles (parked cars, kiosks, crates) — pushed out of their footprint.
            float nx = WorldX, nz = Z;
            Obstacle.Resolve(ref nx, ref nz, 0.45f);
            WorldX = nx; Z = nz;
        }

        // ---- Dash (double-tap WASD or LeftShift) -----------------------------
        private void ReadDashTaps()
        {
            if (_input.MoveLeftDown) TryTap(ref _tapA, -1, 0);
            if (_input.MoveRightDown) TryTap(ref _tapD, 1, 0);
            if (_input.MoveUpDown) TryTap(ref _tapW, 0, 1);
            if (_input.MoveDownDown) TryTap(ref _tapS, 0, -1);
            if (_input.DashDown) RequestDash(HeldDashDir());
        }

        private void TryTap(ref float last, int dx, int dz)
        {
            if (Time.time - last < DoubleTapWindow) RequestDash(new Vector2(dx, dz));
            last = Time.time;
        }

        private Vector2 HeldDashDir()
        {
            float ix = MoveX(), iz = MoveZ();
            if (ix == 0 && iz == 0) return new Vector2(Facing, 0);
            return new Vector2(ix, iz);
        }

        /// <summary>
        /// Route a dash request: airborne = air-dash; grounded, a forward double-tap
        /// INTO a grabbable enemy = Shield Rush (§2.3), otherwise a normal ground dash.
        /// Shield Rush only INTERCEPTS a grabbable target directly ahead; every other
        /// case (no target, H-weight/boss ahead, on cooldown) falls through to the dash,
        /// so the input never no-ops (PLAYER.md §2 LOCKED resolution).
        /// </summary>
        private void RequestDash(Vector2 dir)
        {
            if (_airborne) { TryAirDash(dir); return; }
            if (TryShieldRush(dir)) return; // grabbable enemy ahead -> rush intercepts the dash
            StartDash(dir);
        }

        private void StartDash(Vector2 dir)
        {
            if (_dashing || _shieldRushing || _dashCooldown > 0f || _airborne || _dashCharges < 1f) return;
            _dashCharges -= 1f; // consume a charge (3-per-5s bucket)
            // Dominant-cardinal resolution; horizontal wins ties (TUNING §2.2).
            if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y) && dir.x != 0) dir = new Vector2(Mathf.Sign(dir.x), 0);
            else if (dir.y != 0) dir = new Vector2(0, Mathf.Sign(dir.y));
            else dir = new Vector2(Facing, 0);

            _dashing = true;
            _dashTimer = Tuning.DashDuration;
            _dashDirX = dir.x;
            _dashDirZ = dir.y;
            _dashHit.Clear();
            if (dir.x != 0) Facing = dir.x > 0 ? 1 : -1;
            CancelSwing();      // dash cancels an attack + drops the string
            Vfx.DashDust(WorldX, Z);
            Sfx.Play("dash_whoosh");
            Dashed?.Invoke();   // tutorial dash-push gate
        }

        private void TickDash(float dt)
        {
            if (!_dashing) return;
            float speed = Tuning.DashDistance / Tuning.DashDuration; // ~18 wu/s
            WorldX += _dashDirX * speed * dt;
            Z += _dashDirZ * speed * dt;
            _dashPushing = false;
            DashPlow(_dashDirX, dt);
            _dashTimer -= dt;
            if (_dashTimer <= 0f) { _dashing = false; _dashCooldown = Tuning.DashCooldown; }
        }

        /// <summary>Dashing plows through enemies: shoves anyone in contact along the dash
        /// and knocks them down on first contact (creator ruling). No damage — pure repositioning.</summary>
        private void DashPlow(float dirX, float dt)
        {
            float dir = dirX != 0f ? Mathf.Sign(dirX) : Facing;
            float r2 = Tuning.DashPlowRadius * Tuning.DashPlowRadius;
            foreach (var a in Actor.All)
            {
                if (!a.Alive || a.Team == Team.Player) continue;
                float dx = a.WorldX - WorldX, dz = a.Z - Z;
                if (dx * dx + dz * dz > r2) continue;
                _dashPushing = true;                                              // in contact → shoulder-charge
                a.WorldX += dir * Tuning.DashKnockback * dt;                       // shove along the dash
                a.Z += (dz >= 0f ? 1f : -1f) * Tuning.DashKnockback * 0.4f * dt;   // and nudge aside
                if (_dashHit.Add(a))
                {
                    if (a is IStaggerable s) s.ApplyStagger(0.5f);
                    Vfx.HitSpark(a.WorldX, a.Z);
                    Sfx.Play("enemy_stagger");
                }
            }
        }

        // ---- Air-dash (once per airtime, X-only; Z stays locked, TUNING §2.2) -
        private void TryAirDash(Vector2 dir)
        {
            if (_airDashing || _airDashUsed || !_airborne) return;
            // X-only: a horizontal hold picks the direction, otherwise dash toward facing.
            float dx = (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y) && dir.x != 0) ? Mathf.Sign(dir.x) : Facing;
            _airDashing = true;
            _airDashUsed = true;
            _airDashTimer = Tuning.AirDashDuration;
            _airDashDirX = dx;
            _dashHit.Clear();
            Facing = dx > 0 ? 1 : -1;
            CancelSwing();
            Vfx.DashDust(WorldX, Z);
            Sfx.Play("dash_whoosh");
        }

        private void TickAirDash(float dt)
        {
            if (!_airDashing) return;
            float speed = Tuning.AirDashDistance / Tuning.AirDashDuration; // ~19.4 wu/s
            WorldX += _airDashDirX * speed * dt;                           // X only; Z locked
            DashPlow(_airDashDirX, dt);
            _airDashTimer -= dt;
            if (_airDashTimer <= 0f) _airDashing = false;
        }

        // ---- Shield Rush (forward double-tap into a grabbable enemy, §2.3) ----

        /// <summary>
        /// If a forward (horizontal) double-tap points at a grabbable enemy directly
        /// ahead within 2.0 wu and the move is off cooldown, grab it and start the rush.
        /// Returns true if it intercepted; false to fall through to a normal dash.
        /// </summary>
        private bool TryShieldRush(Vector2 dir)
        {
            if (_shieldRushing || _dashing || _shieldRushCooldown > 0f) return false;
            // Only a horizontal double-tap can aim "ahead" (a pure W/S tap has no facing lane).
            int dx = (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y) && dir.x != 0) ? (int)Mathf.Sign(dir.x) : 0;
            if (dx == 0) return false;
            var target = AcquireShieldTarget(dx);
            if (target == null) return false;   // nothing grabbable ahead -> caller dashes
            BeginShieldRush(target, dx);
            return true;
        }

        /// <summary>Nearest grabbable enemy directly ahead in <paramref name="dx"/> within 2.0 wu.</summary>
        private Actor AcquireShieldTarget(int dx)
        {
            Actor best = null;
            float bestAhead = float.MaxValue;
            foreach (var a in Actor.All)
            {
                if (!IsGrabbable(a)) continue;
                float ahead = (a.WorldX - WorldX) * dx;                 // >0 = in front along the tap
                if (ahead <= 0.1f || ahead > ShieldRushRange) continue;
                if (Mathf.Abs(a.Z - Z) > ShieldRushAheadZ) continue;    // "directly ahead" depth window
                if (ahead < bestAhead) { bestAhead = ahead; best = a; }
            }
            return best;
        }

        /// <summary>
        /// Grabbable = a regular enemy of L/M weight. H-weight (Heavy, Ground Smasher,
        /// Gatling Gunner) resolve to a plain dash (§2.3 tier limit). Bosses/minibosses
        /// use their own controllers (not <see cref="EnemyController"/>) so they're
        /// excluded here automatically.
        /// TODO: when a distinct miniboss controller lands, exclude it explicitly too.
        /// </summary>
        private static bool IsGrabbable(Actor a)
        {
            if (a is not EnemyController ec || !ec.Alive || ec.Team == Team.Player) return false;
            if (ec.Def != null && ec.Def.Weight == StaggerWeight.H) return false; // H-weight not grabbable
            return true;
        }

        private void BeginShieldRush(Actor target, int dx)
        {
            CancelSwing();                 // a rush cancels any live swing + the string
            _shieldRushing = true;
            _shield = target;
            _shieldSoaked = 0f;
            _rushDirX = dx;
            _rushStartX = WorldX;
            _rushTime = 0f;
            Facing = dx > 0 ? 1 : -1;
            _dashHit.Clear();
            Vfx.DashDust(WorldX, Z);
            Sfx.Play("dash_whoosh");
            Sfx.Play("enemy_stagger");     // the grab
            if (_shield is IStaggerable s) s.ApplyStagger(ShieldRushMaxDist); // held/limp for the ride (refreshed on release)
        }

        private void TickShieldRush(float dt)
        {
            _rushTime += dt;

            // Drive forward; carry the shield just ahead of the body, pinned to our depth.
            float step = ShieldRushSpeed * dt;
            WorldX += _rushDirX * step;
            if (_shield != null && _shield.Alive)
            {
                _shield.WorldX = WorldX + _rushDirX * Tuning.FistReach;
                _shield.Z = Z;
                ShieldPlow(dt);            // shove OTHER enemies the shield rams into
            }

            // --- termination, at the first of (§2.3) ---
            if (_shield == null || !_shield.Alive) { EndShieldRush(); return; }         // (b) shield died
            if (_shieldSoaked >= ShieldRushSoakMax) { EndShieldRush(); return; }         // (a) 40 dmg soaked
            if (Mathf.Abs(WorldX - _rushStartX) >= ShieldRushMaxDist) { EndShieldRush(); return; } // (c) max travel
            if (BlockedAhead()) { EndShieldRush(); return; }                             // (e) hit an ungrabbable body
            bool holdingForward = _rushDirX > 0 ? _input.MoveX > 0.3f : _input.MoveX < -0.3f;
            if (!holdingForward && _rushTime >= ShieldRushMinCommit) { EndShieldRush(); return; } // (d) released forward
        }

        /// <summary>The shield body shoves other grabbable enemies aside as it plows through.</summary>
        private void ShieldPlow(float dt)
        {
            float r2 = Tuning.DashPlowRadius * Tuning.DashPlowRadius;
            foreach (var a in Actor.All)
            {
                if (a == _shield || !a.Alive || a.Team == Team.Player) continue;
                float ddx = a.WorldX - _shield.WorldX, ddz = a.Z - _shield.Z;
                if (ddx * ddx + ddz * ddz > r2) continue;
                a.WorldX += _rushDirX * Tuning.DashKnockback * dt;
                a.Z += (ddz >= 0f ? 1f : -1f) * Tuning.DashKnockback * 0.4f * dt;
                if (_dashHit.Add(a) && a is IStaggerable s) { s.ApplyStagger(0.5f); Vfx.HitSpark(a.WorldX, a.Z); }
            }
        }

        /// <summary>An ungrabbable enemy body (H-weight/boss) blocking directly ahead of the shield.</summary>
        private bool BlockedAhead()
        {
            if (_shield == null) return false;
            foreach (var a in Actor.All)
            {
                if (a == _shield || a == this || !a.Alive || a.Team == Team.Player) continue;
                if (IsGrabbable(a)) continue;              // grabbable enemies get plowed, not blocked
                float ahead = (a.WorldX - _shield.WorldX) * _rushDirX;
                if (ahead > 0f && ahead < 0.7f && Mathf.Abs(a.Z - Z) <= ShieldRushAheadZ) return true;
            }
            return false;
        }

        private void EndShieldRush()
        {
            if (!_shieldRushing) return;
            _shieldRushing = false;
            _shieldRushCooldown = ShieldRushCooldown;   // cooldown starts on end (§2.3)
            if (_shield != null && _shield.Alive)
            {
                _shield.WorldX += _rushDirX * ShieldRushShove;            // shove forward 1.0 wu
                if (_shield is IStaggerable s) s.ApplyStagger(ShieldRushReleaseStagger); // released staggered 0.55s
                Vfx.HitSpark(_shield.WorldX, _shield.Z);
                Sfx.Play("enemy_stagger");
            }
            _shield = null;
            Vfx.LandPuff(WorldX, Z);
        }

        // ---- Jump (Space) ----------------------------------------------------
        private void TickJump(float dt)
        {
            if (!_airborne) return;
            _jumpTimer += dt;
            // ASYMMETRIC arc (creator: "come down 2x as fast as jumping up"). Peak at 2/3 of
            // the airtime so the fall (final 1/3) is half the rise duration = 2x faster down.
            float T = Tuning.JumpDuration;
            float tp = T * (2f / 3f);
            if (_jumpTimer <= tp)
            {
                float u = _jumpTimer / tp;                    // 0..1 rising
                _jumpOffset = Tuning.JumpHeight * (1f - (1f - u) * (1f - u));   // ease up to the peak
            }
            else
            {
                float d = Mathf.Clamp01((_jumpTimer - tp) / (T - tp));          // 0..1 falling (2x faster)
                _jumpOffset = Tuning.JumpHeight * (1f - d * d);                 // accelerate down
            }
            if (_jumpTimer >= Tuning.JumpDuration)
            {
                _airborne = false; _jumpOffset = 0f; _airDashing = false;
                Vfx.LandPuff(WorldX, Z);
                Sfx.Play("land");
            }
        }

        private void StartJump()
        {
            if (_airborne || _dashing) return;
            _airborne = true;
            _jumpTimer = 0f;
            _airDashUsed = false;   // refresh the one air-dash for this airtime
            CancelSwing();
            Vfx.JumpPuff(WorldX, Z);
            Sfx.Play("jump");
        }

        // ---- Input dispatch --------------------------------------------------
        private void HandleActionInput()
        {
            if (_input.JumpDown) StartJump();
            if (_input.FireDown) FireWeapon();   // E / East = use/fire the held item
            if (_input.PickupDown) TryPickup();  // F / North = pick up
            if (_input.SpecialDown) FireSpecial(); // Q / right shoulder = special

            // Right hand: 8-directional attacks (arrows / right stick) → dominant cardinal.
            if (_input.AttackDown)
                PressAttack(ResolveAttackDir());
        }

        /// <summary>
        /// Resolve the currently-held arrow keys to ONE cardinal verb: any horizontal
        /// component wins (so ↖/↗/↙/↘ are all SIDE), only a pure ↑/↓ gives up/down
        /// (PLAYER.md §3 dominant-cardinal). Facing is set by the horizontal half.
        /// </summary>
        private AttackDir ResolveAttackDir()
        {
            int ax = _input.AimX > 0.5f ? 1 : _input.AimX < -0.5f ? -1 : 0;
            int ay = _input.AimZ > 0.5f ? 1 : _input.AimZ < -0.5f ? -1 : 0;
            if (ax != 0 && Mathf.Abs(ax) >= Mathf.Abs(ay)) return ax > 0 ? AttackDir.Right : AttackDir.Left;
            if (ay > 0) return AttackDir.Up;
            if (ay < 0) return AttackDir.Down;
            return Facing > 0 ? AttackDir.Right : AttackDir.Left; // released same frame: fall back to facing
        }

        // ---- Attacks: top-level press routing --------------------------------
        private void PressAttack(AttackDir d)
        {
            if (_hitstun > 0f) return;
            if (_airborne) { AirPress(d); return; }
            if (_dashing) { StartDashAttack(d); return; } // dash attack (0 dmg, stagger)

            if (_phase != Phase.None) { _bufferedAttack = true; _bufferedDir = d; return; }
            GroundPress(d);
        }

        /// <summary>Grounded, idle: resolve a press against the primed state machine.</summary>
        private void GroundPress(AttackDir d)
        {
            if (d == AttackDir.Left) Facing = -1;
            else if (d == AttackDir.Right) Facing = 1;
            bool horizontal = d == AttackDir.Left || d == AttackDir.Right;

            // 1) Sweep already landed -> this tap executes the downed target.
            if (_finisherReady) { StartFinisher(d); return; }
            // 2) String primed by P1→P2 -> this press is the sweep.
            if (_primed) { StartSweep(d); return; }
            // 3) Fresh input: a horizontal press fires an equipped gun, else opens
            //    the melee string with P1; a pure up/down is a standalone normal.
            if (horizontal)
            {
                // Guns fire on E ONLY; an arrow press always PUNCHES (creator: guns don't
                // touch melee). Boomerang (a thrown weapon) still throws on the arrow.
                if (CurrentWeapon.Kind == WeaponKind.Boomerang) { FireWeapon(); return; }
                // Point-blank pistol-whip EXECUTE (§3.1): a gun-bash finishes a near-dead enemy
                // in melee range with no bullet — the up-close payoff for a spent mag.
                if (TryPistolWhipExecute(d)) return;
                StartSide(0);
            }
            else StartStrike(d == AttackDir.Up ? AttackKind.Up : AttackKind.Down);
        }

        /// <summary>Airborne press -> the matching air variant (Z stays locked).</summary>
        private void AirPress(AttackDir d)
        {
            if (_phase != Phase.None) return;
            if (d == AttackDir.Left) Facing = -1;
            else if (d == AttackDir.Right) Facing = 1;

            AttackKind k = d == AttackDir.Up ? AttackKind.AirUp
                         : d == AttackDir.Down ? AttackKind.AirDown
                         : AttackKind.AirSide;
            _attackKind = k;
            _combo = -1;
            _hitResolved = false;
            _bufferedAttack = false;
            _phase = Phase.Startup;
            _phaseTimer = PhaseStartup();
            string clip = k == AttackKind.AirUp ? "air_up" : k == AttackKind.AirDown ? "air_down" : "air_side";
            Anim.Play(clip, false, restart: true);
            Sfx.Play("swing_whoosh"); // swing sound (miss = just this)
        }

        private void StartDashAttack(AttackDir d)
        {
            if (_phase != Phase.None) return;
            if (d == AttackDir.Left) Facing = -1;
            else if (d == AttackDir.Right) Facing = 1;
            _attackKind = AttackKind.Dash;
            _combo = -1;
            _hitResolved = false;
            _bufferedAttack = false;
            _phase = Phase.Startup;
            _phaseTimer = PhaseStartup();
            // Reuse the directional swing clips (no bespoke dash-attack clips yet, PLAYER.md §7).
            // Down = the crouch-and-punch-down clip; side = the jab (see StartStrike).
            string clip = d == AttackDir.Up ? "attack_up" : d == AttackDir.Down ? "punch_down" : "attack_side";
            Anim.Play(clip, false, restart: true);
            Sfx.Play("dash_whoosh");
        }

        private void StartSide(int index)
        {
            _attackKind = AttackKind.Side;
            _combo = Mathf.Clamp(index, 0, 1);
            if (_combo == 0) { _p1Connected = _p2Connected = _sweepConnected = false; }
            _phase = Phase.Startup;
            _phaseTimer = PhaseStartup();
            _hitResolved = false;
            _bufferedAttack = false;
            Anim.Play("attack_side", false, restart: true);
            Sfx.Play("swing_whoosh");
        }

        private void StartSweep(AttackDir d)
        {
            _primed = false;
            if (d == AttackDir.Left) Facing = -1;
            else if (d == AttackDir.Right) Facing = 1;
            _bufferedDir = d;               // remembered for the up-launch vs. knockdown variant
            _attackKind = AttackKind.Sweep;
            _combo = 2;
            _phase = Phase.Startup;
            _phaseTimer = PhaseStartup();
            _hitResolved = false;
            _bufferedAttack = false;
            Anim.Play("sweep", false, restart: true);   // #3 = the leg-sweep knockdown (was a plain punch)
            Sfx.Play("swing_whoosh");
        }

        private void StartFinisher(AttackDir d)
        {
            _finisherReady = false;
            if (d == AttackDir.Left) Facing = -1;
            else if (d == AttackDir.Right) Facing = 1;
            _attackKind = AttackKind.Finisher;
            _combo = 3;
            _phase = Phase.Startup;
            _phaseTimer = PhaseStartup();
            _hitResolved = false;
            _bufferedAttack = false;
            Anim.Play("stomp", false, restart: true); // finisher = a downward STOMP on the floored enemy
            Sfx.Play("finisher_crunch");
        }

        private void StartStrike(AttackKind kind) // standalone up/down normal (not part of the string)
        {
            _attackKind = kind;
            _combo = -1;
            _phase = Phase.Startup;
            _phaseTimer = PhaseStartup();
            _hitResolved = false;
            _bufferedAttack = false;
            // Up = the uppercut (rising strike). Down = the bespoke "punch_down": the hero crouches
            // and drives a fist down at an enemy in the near lane — so it reads as hitting someone
            // BELOW/in front, not the old roundhouse that swung at the empty floor.
            Anim.Play(kind == AttackKind.Up ? "attack_up" : "punch_down", false, restart: true);
            Sfx.Play("swing_whoosh");
        }

        /// <summary>End the current swing (phase only); leaves the primed/finisher flags intact.</summary>
        private void EndSwing() { _phase = Phase.None; }

        /// <summary>Drop the whole combo string state (dash/jump/hurt cancels, or a lapse).</summary>
        private void ClearString()
        {
            _primed = false; _finisherReady = false; _bufferedAttack = false;
            _combo = -1; _p1Connected = _p2Connected = _sweepConnected = false;
        }

        /// <summary>Cancel a live swing AND drop the string (used by dash/jump/hurt).</summary>
        private void CancelSwing() { EndSwing(); ClearString(); }

        private void TickComboWindows(float dt)
        {
            if (_primed) { _primedTimer -= dt; if (_primedTimer <= 0f) { _primed = false; if (_phase == Phase.None) ClearString(); } }
            if (_finisherReady) { _finisherTimer -= dt; if (_finisherTimer <= 0f) { _finisherReady = false; if (_phase == Phase.None) ClearString(); } }
        }

        private void TickAttack(float dt)
        {
            if (_phase == Phase.None) return;
            _phaseTimer -= dt;

            if (_phase == Phase.Active && !_hitResolved)
            {
                if (_attackKind == AttackKind.Finisher) ResolveFinisher();
                else ResolveSwing();
                _hitResolved = true;
            }

            // Bat's signature: the swing's active frames reflect incoming enemy shots (WEAPONS.md §3.7).
            if (_phase == Phase.Active && CurrentWeapon != null && CurrentWeapon.Kind == WeaponKind.Bat)
                BatReflectSweep();

            if (_phaseTimer > 0f) return;

            switch (_phase)
            {
                case Phase.Startup:
                    _phase = Phase.Active;
                    _phaseTimer = PhaseActive();
                    break;
                case Phase.Active:
                    _phase = Phase.Recovery;
                    _phaseTimer = PhaseRecovery();
                    break;
                case Phase.Recovery:
                    EndOfRecovery();
                    break;
            }
        }

        /// <summary>
        /// Bat parry (WEAPONS.md §3.7): during the swing's active frames, knock enemy shots in
        /// the arc in front of the bat back the way they came — flat bullets fly straight back as
        /// ours, arced heads (chopper) go home and score the boss pip. Reflect-only; the swing's
        /// own melee hit still lands via ResolveSwing.
        /// </summary>
        private void BatReflectSweep()
        {
            const float reachX = 1.9f, reachZ = 1.1f;

            foreach (var pr in Object.FindObjectsByType<Projectile>(FindObjectsInactive.Exclude))
            {
                if (pr == null || pr.OwnerTeam != Team.Enemy) continue;
                float dx = pr.WorldX - WorldX;
                if (Facing > 0 ? dx < -0.3f : dx > 0.3f) continue;          // must be in front of the swing
                if (Mathf.Abs(dx) > reachX || !Playfield.WithinZ(pr.Z, Z, reachZ)) continue;
                pr.Reflect(Team.Player, Facing);
                Vfx.HitSpark(pr.WorldX, pr.Z);
                Sfx.Play("hit_spark");
            }

            foreach (var arc in Object.FindObjectsByType<ArcProjectile>(FindObjectsInactive.Exclude))
            {
                if (arc == null || arc.Reflected || arc.OwnerTeam != Team.Enemy) continue;
                float dx = arc.CurX - WorldX;
                if (Facing > 0 ? dx < -0.3f : dx > 0.3f) continue;
                if (Mathf.Abs(dx) > reachX || !Playfield.WithinZ(arc.CurZ, Z, reachZ)) continue;
                arc.ReflectHome(this);
                Vfx.HitSpark(arc.CurX, arc.CurZ);
                Sfx.Play("crunch");   // heavier thock for batting a whole head back
            }
        }

        /// <summary>Resolve string progression + any buffered follow-up at recovery's end.</summary>
        private void EndOfRecovery()
        {
            switch (_attackKind)
            {
                case AttackKind.Side when _combo == 0: // after P1
                    EndSwing();
                    if (_bufferedAttack && IsHorizontal(_bufferedDir)) { _bufferedAttack = false; StartSide(1); }
                    else { ClearString(); DispatchBuffer(); }
                    break;

                case AttackKind.Side: // after P2 -> PRIME the sweep if both punches connected
                    EndSwing();
                    if (_p1Connected && _p2Connected) { _primed = true; _primedTimer = PrimeWindow; }
                    else ClearString();
                    DispatchBuffer();
                    break;

                case AttackKind.Sweep: // arm the finisher if the sweep floored someone
                    EndSwing();
                    if (_sweepConnected) { _finisherReady = true; _finisherTimer = FinisherWindow; }
                    else ClearString();
                    DispatchBuffer();
                    break;

                case AttackKind.Finisher:
                    CancelSwing();
                    DispatchBuffer();
                    break;

                default: // standalone normals (up/down), air variants, dash attack
                    EndSwing();
                    DispatchBuffer();
                    break;
            }
        }

        private static bool IsGun(WeaponKind k) =>
            k == WeaponKind.Pistol || k == WeaponKind.Revolver || k == WeaponKind.Gatling;

        /// <summary>
        /// Pistol-whip execute (§3.1): with a gun equipped, a point-blank attack press on a
        /// near-dead enemy (≤20% HP) in the pressed direction bashes it out — no bullet spent.
        /// Returns true if it executed so the caller skips the normal punch. Feel note: the spec
        /// frames this as a double-tap; this first pass triggers on a single point-blank press.
        /// </summary>
        private bool TryPistolWhipExecute(AttackDir d)
        {
            if (CurrentWeapon == null || !IsGun(CurrentWeapon.Kind)) return false;
            if (d == AttackDir.Left) Facing = -1; else if (d == AttackDir.Right) Facing = 1;

            const float reachX = 1.4f, perpZ = 0.9f;
            Actor victim = null;
            foreach (var a in Actor.All)
            {
                if (a == null || !a.Alive || a.Team != Team.Enemy) continue;
                float dx = a.WorldX - WorldX;
                if (Facing > 0 ? dx < 0f : dx > 0f) continue;          // must be in the pressed direction
                if (Mathf.Abs(dx) > reachX || !Playfield.WithinZ(a.Z, Z, perpZ)) continue;
                if (a.Hp > a.MaxHp * 0.20f) continue;                  // only near-dead targets get the bash
                victim = a; break;
            }
            if (victim == null) return false;

            _attackKind = AttackKind.Side;
            _combo = -1;
            _hitResolved = true;                 // we apply the hit here; TickAttack won't re-swing
            _bufferedAttack = false;
            _phase = Phase.Startup;
            _phaseTimer = PhaseStartup();
            Anim.Play("attack_side", false, restart: true);

            victim.TakeDamage(9999f, this);
            Vfx.HitSpark(victim.WorldX, victim.Z);
            ComboJuice.Impact(victim.WorldX, victim.Z, Meter.Combo, true);
            HitStop.Freeze(HitStop.Kill);
            ComboHud.RegisterKill();
            Sfx.Play("finisher_crunch");
            return true;
        }

        private void DispatchBuffer()
        {
            if (!_bufferedAttack) return;
            var d = _bufferedDir;
            _bufferedAttack = false;
            if (_airborne) AirPress(d);
            else if (_dashing) StartDashAttack(d);
            else GroundPress(d);
        }

        // ---- Hit resolution --------------------------------------------------
        private void ResolveSwing()
        {
            // A gun/ranged weapon (reach ~0) melees as a plain FIST punch — it never shrinks
            // your attack or whiffs; it only FIRES on E (creator: "guns shouldn't do anything to melee").
            bool isFist = CurrentWeapon.IsFists || CurrentWeapon.Reach <= 0.05f;
            bool dashStagger = _attackKind == AttackKind.Dash;
            float dmgMult = Meter.DamageMultiplier * _dmgBuffMult;
            float reach, perp;
            int dmg;
            Vector2 dir = new(Facing, 0f);   // default: horizontal, facing side
            float fistReach = Tuning.FistReach + Tuning.GustBonus;

            switch (_attackKind)
            {
                case AttackKind.Side:
                    reach = CurrentWeapon.Reach + (isFist ? Tuning.GustBonus : 0f);
                    if (isFist) { dmg = ComboDamage(_combo); dmgMult *= Character.PunchDmgMult; }
                    else { dmg = CurrentWeapon.Damage; dmgMult *= Character.WeaponDmgMult; }
                    perp = Tuning.SideArcZTolerance;  // a decent depth arc so hits land
                    break;
                case AttackKind.Sweep:
                    reach = Mathf.Max(CurrentWeapon.Reach + (isFist ? Tuning.GustBonus : 0f), Tuning.SweepReach);
                    if (isFist) { dmg = Tuning.DmgSweep; dmgMult *= Character.PunchDmgMult; }
                    else { dmg = CurrentWeapon.Damage; dmgMult *= Character.WeaponDmgMult; }
                    perp = Tuning.SweepZTolerance; // the ONE wider crowd move
                    break;
                case AttackKind.Up:       // strike into the FAR depth row
                    dir = new Vector2(0f, 1f); perp = Tuning.StrikePerpX;
                    if (CurrentWeapon.Kind == WeaponKind.Whip)  // whip up-arc: its own reach/damage (§3.4)
                    { reach = 2.5f; dmg = CurrentWeapon.Damage; dmgMult *= Character.WeaponDmgMult; }
                    else
                    { reach = Tuning.StrikeZReach; dmg = Tuning.DmgUpStrike; dmgMult *= Character.PunchDmgMult; }
                    break;
                case AttackKind.Down:     // strike into the NEAR depth row
                    dir = new Vector2(0f, -1f); perp = Tuning.StrikePerpX;
                    if (CurrentWeapon.Kind == WeaponKind.Whip)  // whip down-line: long crowd reach (§3.4)
                    { reach = 4.0f; dmg = CurrentWeapon.Damage; dmgMult *= Character.WeaponDmgMult; }
                    else
                    { reach = Tuning.StrikeZReach; dmg = Tuning.DmgDownStrike; dmgMult *= Character.PunchDmgMult; }
                    break;
                case AttackKind.AirSide:
                    reach = fistReach; dmg = Tuning.DmgAirSide; perp = Tuning.SideArcZTolerance;
                    dmgMult *= Character.PunchDmgMult;
                    break;
                case AttackKind.AirUp:
                    dir = new Vector2(0f, 1f); reach = Tuning.StrikeZReach; perp = Tuning.StrikePerpX;
                    dmg = Tuning.DmgAirSide; dmgMult *= Character.PunchDmgMult;
                    break;
                case AttackKind.AirDown:
                    dir = new Vector2(0f, -1f); reach = Tuning.StrikeZReach; perp = Tuning.StrikePerpX;
                    dmg = 12; dmgMult *= Character.PunchDmgMult; // §2.1 air down / spike
                    break;
                case AttackKind.Dash:
                    reach = fistReach; dmg = Tuning.DmgDashAttack; perp = Tuning.PlayerHitZTolerance; // 0
                    dmgMult *= Character.PunchDmgMult;
                    break;
                default:
                    return;
            }

            int applied = dashStagger ? 0 : dmg;
            // Up/Down strikes sweep an ANGULAR ARC instead of a thin depth-strip: the uppercut fans
            // from directly in front (0°) up to "up" (+90°); the down-strike mirrors it. They overlap
            // the side attack by ~10° at the front so there are no dead angles (creator: fluid 8-way).
            float arcReach = Mathf.Max(reach, Tuning.FistReach + 0.6f);
            List<Actor> hits =
                (_attackKind == AttackKind.Up || _attackKind == AttackKind.AirUp)
                    ? Combat.MeleeHitArc(this, 45f, 50f, arcReach, applied, dmgMult)
              : (_attackKind == AttackKind.Down || _attackKind == AttackKind.AirDown)
                    ? Combat.MeleeHitArc(this, -45f, 50f, arcReach, applied, dmgMult)
                    : Combat.MeleeHitDirectional(this, dir, reach, perp, applied, dmgMult);

            // Track the connections the primed string depends on.
            if (_attackKind == AttackKind.Side && _combo == 0) _p1Connected = hits.Count > 0;
            else if (_attackKind == AttackKind.Side && _combo == 1) _p2Connected = hits.Count > 0;
            else if (_attackKind == AttackKind.Sweep) _sweepConnected = hits.Count > 0;

            if (hits.Count == 0) return;

            // Dash attacks are a positioning tool, not damage: they don't build meter.
            if (!dashStagger) Meter.RegisterHit(isFist, Character.MeterFillMult);
            if (isFist) Vfx.Gust(WorldX + Facing * reach, Z, Facing); // the air reach-extender (PLAYER.md §1)
            foreach (var a in hits) Vfx.HitSpark(a.WorldX, a.Z);
            // Impact sound — plays ONLY on a connect (miss = just the swing whoosh).
            Sfx.Play(_attackKind switch
            {
                AttackKind.Sweep => "sweep_hit",
                AttackKind.AirSide or AttackKind.AirUp or AttackKind.AirDown => "air_hit",
                AttackKind.Side => (_combo == 1 ? "punch_2" : "punch_1"),
                _ => "punch_1", // up/down strikes
            });

            // --- JUICE: escalating feedback + hit-stop (scale by attack & combo) ---
            int killCount = 0;
            foreach (var a in hits) if (!a.Alive) { killCount++; ComboHud.RegisterKill(); }
            bool heavyHit = _attackKind == AttackKind.Sweep;
            ComboJuice.Impact(WorldX + Facing * reach, Z, Meter.Combo, heavyHit); // combo-scaled shake + sparks
            if (!dashStagger)
            {
                float freeze = _attackKind == AttackKind.Sweep ? HitStop.Sweep
                             : (_attackKind == AttackKind.Side && _combo == 1) ? HitStop.Normal
                             : HitStop.Jab;
                if (killCount > 0) freeze = Mathf.Max(freeze, HitStop.Kill); // kill takes precedence (§2.6)
                HitStop.Freeze(freeze);
            }

            // Reaction states (TUNING §2.6). Weight isn't readable here, so dash uses a
            // mid L/M value and the H-weight "floors the player" case is a TODO.
            if (dashStagger)
            {
                foreach (var a in hits) if (a is IStaggerable s) s.ApplyStagger(0.5f);
                // TODO: H-weight/boss should FLOOR the player (0.70s down, non-invuln) instead —
                //       needs a readable enemy weight; wire when EnemyController exposes it.
            }
            else if (_attackKind == AttackKind.Sweep)
            {
                foreach (var a in hits) if (a is IStaggerable s) s.ApplyStagger(1.2f); // knockdown / up-launch
            }
            else if (_attackKind == AttackKind.Up || _attackKind == AttackKind.AirUp)
            {
                // UPPERCUT LAUNCH (creator liked this from the JS build): pop enemies into the air.
                foreach (var a in hits)
                {
                    if (a is EnemyController ec) ec.Launch(14f, Facing * 3f);
                    else if (a is IStaggerable s) s.ApplyStagger(0.5f);   // bosses etc. just stagger
                }
            }
            else if (_attackKind == AttackKind.AirDown)
            {
                foreach (var a in hits) if (a is IStaggerable s) s.ApplyStagger(0.5f); // spike (down) stays
            }

            // WHIP PULL-DRAG (§3.4): a forward whip swing yanks the crowd ~3 wu toward you (its
            // signature over plain long reach) — leaving a small gap so they land in punch range.
            if (CurrentWeapon.Kind == WeaponKind.Whip && _attackKind == AttackKind.Side)
            {
                foreach (var a in hits)
                {
                    if (a is not EnemyController ec) continue;
                    float gap = (a.WorldX - WorldX) * Facing;             // >0 = ahead of you
                    float pull = Mathf.Clamp(gap - 0.9f, 0f, 3.0f);       // drag toward you, keep 0.9 wu
                    if (pull > 0f) { ec.WorldX -= Facing * pull; if (a is IStaggerable s) s.ApplyStagger(0.3f); }
                }
            }

            // CLUB KNOCKBACK (§3.7c): the club's whole signature over fists — a heavy shove that
            // knocks the struck enemy back a step (fists leave them planted).
            if (CurrentWeapon.Kind == WeaponKind.Club &&
                (_attackKind == AttackKind.Side || _attackKind == AttackKind.Sweep))
            {
                foreach (var a in hits)
                {
                    if (a is not EnemyController ec) continue;
                    ec.WorldX += Facing * 1.2f;                    // shove away from you
                    if (a is IStaggerable s) s.ApplyStagger(0.4f);
                }
            }

            // Only the melee string (P1/P2/sweep) spends a MELEE weapon's durability. Ranged hybrids
            // (Ball & Chain, Staff) swing as free melee — their HitsRemaining is E-fire charges (launches
            // / casts), spent in WeaponFx, NOT by an arrow swing. Guarding on !IsRanged stops a couple of
            // normal swings from silently burning all 3 Ball & Chain launches (§3.3: "string spends none").
            if (!isFist && !CurrentWeapon.IsRanged &&
                (_attackKind == AttackKind.Side || _attackKind == AttackKind.Sweep) && CurrentWeapon.Spend())
            {
                Sfx.Play("weapon_break_puff");
                CurrentWeapon = Weapon.Fists();
            }
        }

        /// <summary>
        /// The finisher (hit 4): auto-acquire the closest enemy in the tapped
        /// direction within 5 wu, step onto it, and land a free-melee 35 (COMBOS §1,
        /// PLAYER.md §3). One finisher = one target. Free melee = no ammo/durability.
        /// </summary>
        private void ResolveFinisher()
        {
            var target = AcquireFinisherTarget();
            if (target == null) return; // whiffed (no downed body in range)

            // WEAPON EXECUTIONS (COMBOS §4): the finisher with a signature melee weapon takes the HEAD
            // off. Whip RIPS it into a live grenade; sword LOPS it off to tumble; ball & chain SMASHES
            // it to pulp (no flying head); bat LAUNCHES it clean off like a home run.
            var wk = CurrentWeapon != null ? CurrentWeapon.Kind : WeaponKind.Fists;
            if (wk is WeaponKind.Whip or WeaponKind.Sword or WeaponKind.BallChain or WeaponKind.Bat)
            {
                target.TakeDamage(9999f, this);            // execution = instant kill
                Vfx.DeathBurst(target.WorldX, target.Z);   // neck spray
                switch (wk)
                {
                    case WeaponKind.Whip:                  // ripped off → a LIVE grenade head
                    {
                        var head = FlingHead(target, Facing * 4.5f, 30f, 3f);
                        head.SplashRadius = 2f;
                        head.OnLand = () => { Sfx.Play("grenade_explode"); CameraShake.Add(CameraShake.Heavy); };
                        break;
                    }
                    case WeaponKind.Sword:                 // lopped off → tumbles a short way
                        FlingHead(target, Facing * 2.5f, 20f, 2.2f);
                        break;
                    case WeaponKind.Bat:                   // HOME RUN → launched far and fast
                        FlingHead(target, Facing * 12f, 32f, 5f);
                        Sfx.Play("air_hit");
                        break;
                    case WeaponKind.BallChain:             // SMASHED → no flying head, pulp + a big shake
                        Vfx.DeathBurst(target.WorldX + 0.2f, target.Z + 0.2f);
                        Sfx.Play("ground_smash");
                        break;
                }
                Sfx.Play("finisher_crunch");
                FinisherKillJuice(target);
                return;
            }

            WorldX = target.WorldX - Facing * Tuning.FistReach; // step onto the target
            Z = target.Z;

            float mult = Meter.DamageMultiplier * _dmgBuffMult * Character.PunchDmgMult;
            bool killed = target.TakeDamage(Mathf.RoundToInt(Tuning.DmgFinisher * mult), this);
            Meter.RegisterHit(true, Character.MeterFillMult);

            Vfx.FinisherFlash(target.WorldX, target.Z);
            Sfx.Play("finisher_crunch");
            CameraShake.Add(CameraShake.Heavy);
            // JUICE: freeze-frame — 5f on a kill, 3f on a non-killing finisher (§2.6).
            HitStop.Freeze(killed ? HitStop.Kill : HitStop.Finisher);
            if (killed) ComboHud.RegisterKill();
            ComboJuice.Impact(target.WorldX, target.Z, Meter.Combo, heavy: true);
            FinisherLanded?.Invoke();   // tutorial execute/finisher gate
        }

        /// <summary>Fling a severed head from a target: a pale arc projectile that thuds on landing.</summary>
        private ArcProjectile FlingHead(Actor t, float dxWu, float speed, float arcHeight)
        {
            var head = ArcProjectile.Spawn(Team.Player, t.WorldX, t.Z + 0.3f, t.WorldX + dxWu, t.Z,
                                           speed, new Color(0.95f, 0.9f, 0.8f), airTime: 0.7f);
            head.ArcHeight = arcHeight;
            head.OnLand = () => Sfx.Play("knockdown_thud");
            return head;
        }

        /// <summary>The shared kill-finisher juice: flash, shake, freeze, combo tick, tutorial gate.</summary>
        private void FinisherKillJuice(Actor t)
        {
            Vfx.FinisherFlash(t.WorldX, t.Z);
            CameraShake.Add(CameraShake.Heavy);
            HitStop.Freeze(HitStop.Kill);
            ComboHud.RegisterKill();
            ComboJuice.Impact(t.WorldX, t.Z, Meter.Combo, heavy: true);
            FinisherLanded?.Invoke();
        }

        private Actor AcquireFinisherTarget()
        {
            Actor best = null;
            float bestDx = float.MaxValue;
            foreach (var a in Actor.All)
            {
                if (a == this || !a.Alive || a.Team == Team) continue;
                float dx = (a.WorldX - WorldX) * Facing; // >0 = in front
                if (dx < -0.5f || dx > FinisherAcquire) continue;
                if (Mathf.Abs(a.Z - Z) > Tuning.SweepZTolerance + 0.5f) continue;
                float d = Mathf.Abs(dx);
                if (d < bestDx) { bestDx = d; best = a; }
            }
            return best;
        }

        // ---- Special (Q) — payload is per-character ---------------------------
        /// <summary>
        /// Tutorial hook: while true, the player's own Q press is ignored so the paused
        /// "unleash your special" showcase can control exactly when it fires (after it has
        /// restored Time.timeScale to 1). Always cleared by the tutorial before it force-fires.
        /// </summary>
        [System.NonSerialized] public bool SpecialLocked;

        /// <summary>Tutorial hook: fire the special on command (bypasses <see cref="SpecialLocked"/>).
        /// Call only after Time.timeScale is back to 1f so the special's payload runs normally.</summary>
        public void FireSpecialNow()
        {
            bool wasLocked = SpecialLocked;
            SpecialLocked = false;
            FireSpecial();
            SpecialLocked = wasLocked;
        }

        private void FireSpecial()
        {
            if (SpecialLocked) return;   // tutorial pause owns the trigger this moment
            if (!Meter.CanFire) return;
            int tier = Meter.Fire();
            if (tier == 0) return;
            CameraShake.Add(CameraShake.Heavy);
            Character.Special?.Fire(this, tier);
            SpecialFired?.Invoke(tier);
            // JUICE: a big cast freeze. Guarded in HitStop — this is a NO-OP if the cast
            // just started the sniper slow-mo (timeScale already 0.28), so no conflict.
            HitStop.Freeze(HitStop.Special);
        }

        // ---- Weapons ---------------------------------------------------------
        public void Equip(WeaponKind kind)
        {
            CurrentWeapon = Weapon.Create(kind); // full roster (WEAPONS.md); single-slot swap
            _weaponReady = CurrentWeapon.Warmup;
            WeaponEquipped?.Invoke(kind);   // lets the tutorial pop the "press E to fire" prompt
        }

        /// <summary>Fire a ranged weapon (E or a fresh horizontal attack arrow). Melee weapons ignore this.</summary>
        private void FireWeapon()
        {
            if (_airborne || _dashing || _hitstun > 0f || _weaponReady > 0f || _fireLock > 0f) return;
            if (_aimTimer > 0f) return;   // already winding up an aimed shot

            // Aimed weapons (pistol) commit to a brief AIM before the shot leaves — a deliberate,
            // precise shot rather than a snap fire. The Update loop releases it when _aimTimer hits 0.
            if (CurrentWeapon.IsRanged && CurrentWeapon.AimTime > 0f &&
                CurrentWeapon.FireCooldown <= 0f && CurrentWeapon.FireImpl != null)
            {
                _aimTimer = CurrentWeapon.AimTime;
                _fireLock = CurrentWeapon.AimTime + 0.12f;   // rooted through the aim + a short recover
                Anim.Play("attack_side", false, restart: true); // aim pose (placeholder until a bespoke aim clip)
                Sfx.Play("swing_whoosh");                        // soft aim tell
                return;
            }

            if (CurrentWeapon.TryFire(this))
            {
                Anim.Play("attack_side", false, restart: true);
                _fireLock = 0.2f;   // root in place for 0.2s — firing is a deliberate, non-spammable commitment
            }
        }

        /// <summary>
        /// F = pick up the nearest ground weapon (PLAYER.md §2; grabbing while armed
        /// destroys the current weapon, which <see cref="Equip"/> already does). Prefers
        /// the weapons owner's <c>Pickup.NearestWithin</c>/<c>GrabBy</c> API once it lands;
        /// until then scans the live pickups for the nearest within ~0.9 wu.
        /// </summary>
        private void TryPickup()
        {
            var best = Pickup.NearestWithin(WorldX, Z, PickupRadius);
            if (best != null) best.GrabBy(this);
        }

        // ---- Damage ----------------------------------------------------------
        public override bool TakeDamage(float amount, Actor source)
        {
            if (!Alive) return false;
            if (GodMode) return false;   // TEMP DEBUG: K toggles invincibility

            // Shield Rush soak (§2.3): every hit that would strike the player-behind-shield
            // instead lands on the shield body (100% of the damage) and counts toward the
            // 40-dmg budget. The rush ends when the budget is spent or the shield dies.
            if (_shieldRushing && _shield != null && _shield.Alive)
            {
                _shieldSoaked += amount;
                _shield.TakeDamage(amount, source);        // shielded enemy takes the hit
                Vfx.HitSpark(_shield.WorldX, _shield.Z);
                Sfx.Play("enemy_stagger");
                if (_shieldSoaked >= ShieldRushSoakMax || !_shield.Alive) EndShieldRush();
                return false;                               // ...the player takes none
            }

            if (_invuln > 0f) return false; // i-frames (Werewolf transform, etc.)
            _hitstun = Tuning.HitstunDuration;
            CancelSwing();          // a hit breaks the swing + the combo string
            Meter.OnDamaged();
            bool dead = base.TakeDamage(amount, source);
            CameraShake.Add(CameraShake.Light);
            if (dead) { Anim.Play("death", false, restart: true); Sfx.Play("death"); OnDowned(); }
            else { Anim.Play("hurt", false, restart: true); Sfx.Play("hurt_grunt"); }
            return dead;
        }

        // ---- Downed / respawn (shared-life system, creator spec) -------------
        /// <summary>
        /// A player just went down. Spend a team life to queue a respawn; if the pool is
        /// empty and this death leaves NOBODY standing, it's GAME OVER (back to title).
        /// </summary>
        private void OnDowned()
        {
            _shieldRushing = false; _shield = null;   // drop any live rush cleanly
            if (Lives.TryConsume())
            {
                _awaitingRespawn = true;
                _respawnTimer = Tuning.RespawnDelay;
            }
            else
            {
                _awaitingRespawn = false;
                // Game over only if nobody is alive AND nobody is mid-respawn (a teammate who
                // already spent a life is still coming back).
                bool teammateReturning = false;
                foreach (var p in All)
                    if (p != null && p != this && p._awaitingRespawn) { teammateReturning = true; break; }
                if (!AnyAlive && !teammateReturning) GameFlow.Instance?.TriggerGameOver();
            }
        }

        /// <summary>
        /// Co-op mercy hook (called on each stage clear): bring back a player who has been
        /// sitting DOWNED because the shared life pool was empty when they fell. A teammate
        /// who kept the run alive gets their partner back for the next stage. No-op for a
        /// living player or one already queued to respawn (their beat is still ticking).
        /// </summary>
        public void ReviveIfDownedOut()
        {
            if (Alive || _awaitingRespawn) return;
            Respawn();
        }

        /// <summary>Bring a downed player back: full HP, brief i-frames, next to a living teammate.</summary>
        private void Respawn()
        {
            _awaitingRespawn = false;
            Alive = true;
            Hp = MaxHp;

            // Clear combat/movement state so we come back clean.
            _hitstun = 0f; _dashing = false; _airDashing = false; _airborne = false;
            _jumpOffset = 0f; _shieldRushing = false; _shield = null;
            _phase = Phase.None; ClearString();
            _invuln = Mathf.Max(_invuln, Tuning.RespawnInvuln);

            // Reposition beside a living teammate (co-op); otherwise respawn in place (solo).
            var mate = NearestLivingTeammate();
            if (mate != null)
            {
                WorldX = mate.WorldX + (mate.Facing >= 0 ? -1.5f : 1.5f);
                Z = mate.Z;
            }

            Anim.Play("idle", true);
            Vfx.LandPuff(WorldX, Z);
            Sfx.Play("confirm");
        }

        private PlayerController NearestLivingTeammate()
        {
            PlayerController best = null; float bestSq = float.MaxValue;
            foreach (var p in All)
            {
                if (p == null || p == this || !p.Alive) continue;
                float dx = p.WorldX - WorldX, dz = p.Z - Z;
                float d = dx * dx + dz * dz;
                if (d < bestSq) { bestSq = d; best = p; }
            }
            return best;
        }

        // ---- Weapon skin (the "swing a dead stick figure" art) ---------------
        private WeaponKind _overlayKind = WeaponKind.Fists;
        private float _overlayScale = 1f;   // shrink the (96px-canvas) weapon art to match the 68px base

        /// <summary>Point the animator at this hero's per-weapon idle/swing atlas when one exists
        /// (assets/sprites/characters/&lt;hero&gt;_&lt;weapon&gt;); clear it for fists / un-arted weapons.
        /// Reactive to any equip/discard path since it keys off the current weapon each frame.</summary>
        private void RefreshWeaponOverlay()
        {
            var kind = CurrentWeapon?.Kind ?? WeaponKind.Fists;
            if (kind == _overlayKind) return;
            _overlayKind = kind;
            if (Anim == null || Character == null) return;

            if (kind == WeaponKind.Fists) { Anim.Overlay = null; _overlayScale = 1f; return; }
            string wname = kind.ToString().ToLowerInvariant();
            string dir = $"sprites/characters/{Character.SpriteActor}_{wname}";
            string actor = $"{Character.SpriteActor}_{wname}";
            Anim.Overlay = SpriteLibrary.HasAtlas(dir, actor) ? SpriteLibrary.Load(dir, actor) : null;

            // The weapon art was drawn on a 96px canvas vs the 68px base, so its frames are ~1.25x
            // taller — without correction you become a giant when armed. Scale the sprite down by
            // base-idle-height / overlay-idle-height so the character stays base-sized (weapon still
            // extends past the body). Feet stay grounded (bottom-centre pivot).
            _overlayScale = Anim.Overlay != null
                ? SafeRatio(ClipHeight(Anim.Set, "idle"), ClipHeight(Anim.Overlay, "idle"))
                : 1f;
        }

        private static float ClipHeight(SpriteLibrary.ActorSprites set, string clip)
            => set != null && set.Clips != null && set.Clips.TryGetValue(clip, out var f) && f != null && f.Length > 0
               ? f[0].rect.height : 0f;
        private static float SafeRatio(float baseH, float overlayH)
            => baseH > 1f && overlayH > 1f ? Mathf.Clamp(baseH / overlayH, 0.4f, 1f) : 1f;

        // ---- Held-weapon sprite (drawn in the hand; keeps the base character on-model) --------
        // Pixellab drifts the character when it redraws a full "hold" pose, so instead the base
        // sprite is left untouched and the weapon's own sprite is pinned to the hand. The pickup PNGs
        // are big "on-the-ground" art, so we DON'T scale by a fixed factor (that made a full-body
        // sword) — we normalise every weapon to the same small world height and drop it at the hand.
        private const float HeldTargetH = 0.85f;   // world-units tall the weapon renders, regardless of PNG size
        private const float HeldFwd     = 0.30f;   // hand offset forward of the body centre
        private const float HeldUp      = 0.55f;   // hand height (grip sits here; blade rises above)
        private SpriteRenderer _heldWeaponSr;
        private static readonly Dictionary<WeaponKind, Sprite> _heldSprites = new();

        private void UpdateHeldWeapon()
        {
            // If a bespoke weapon OVERLAY is active (the hero drawn holding the black stick-figure
            // fragment), it already shows the weapon — don't ALSO pin the pickup sprite (double weapon).
            // The pinned sprite is the fallback for weapons/heroes that have no overlay art yet.
            if (Anim != null && Anim.Overlay != null) { if (_heldWeaponSr != null) _heldWeaponSr.enabled = false; return; }

            var kind = CurrentWeapon?.Kind ?? WeaponKind.Fists;
            Sprite spr = (Alive && kind != WeaponKind.Fists) ? HeldWeaponSprite(kind) : null;
            if (spr == null) { if (_heldWeaponSr != null) _heldWeaponSr.enabled = false; return; }

            if (_heldWeaponSr == null)
            {
                var go = new GameObject("HeldWeapon");
                go.transform.SetParent(transform, false);
                _heldWeaponSr = go.AddComponent<SpriteRenderer>();
            }
            _heldWeaponSr.enabled = true;
            _heldWeaponSr.sprite = spr;
            _heldWeaponSr.flipX = Facing < 0;
            // Normalise: whatever the pickup PNG's pixel size, render it HeldTargetH world-units tall.
            float natH = spr.rect.height / Tuning.PixelsPerUnit;
            float sc = natH > 0.05f ? HeldTargetH / natH : 0.4f;
            _heldWeaponSr.transform.localPosition = new Vector3(Facing * HeldFwd, HeldUp, 0f);
            _heldWeaponSr.transform.localScale = new Vector3(sc, sc, 1f);
            if (Sr != null) _heldWeaponSr.sortingOrder = Sr.sortingOrder + 1;   // in front of the body
        }

        private static Sprite HeldWeaponSprite(WeaponKind kind)
        {
            if (_heldSprites.TryGetValue(kind, out var cached)) return cached;
            Sprite s = null;
            try
            {
                string name = kind.ToString().ToLowerInvariant();
                string path = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "sprites", "weapons", name, name + "_pickup.png");
                if (System.IO.File.Exists(path))
                {
                    var t = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    t.LoadImage(System.IO.File.ReadAllBytes(path));
                    t.Apply();
                    // Pivot at the grip end (bottom-centre) so the handle sits at the hand and the
                    // blade/barrel rises above it, instead of the whole sprite centring on the wrist.
                    s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.15f), Tuning.PixelsPerUnit);
                }
            }
            catch { s = null; }
            _heldSprites[kind] = s;   // cache null too (skip the disk hit next frame)
            return s;
        }

        // ---- Animation & projection -----------------------------------------
        private void UpdateAnimation()
        {
            RefreshWeaponOverlay();
            if (_phase != Phase.None) return; // attack clip already playing
            if (_airDashing) { Anim.Play("dash", false); return; }
            if (_airborne) { Anim.Play("jump", false); return; }
            // Shoving an enemy → a looping shoulder-charge sprint; a clean dash → the slide.
            if (_dashing) { if (_dashPushing) Anim.Play("charge", true); else Anim.Play("dash", false); return; }
            Anim.Play(MoveX() != 0 || MoveZ() != 0 ? "walk" : "idle", true);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (_jumpOffset != 0f)
            {
                var p = transform.position;
                p.y += _jumpOffset;
                transform.position = p;
            }
            // Shrink the oversized weapon-hold art back to the base character's size (feet grounded).
            if (_overlayScale != 1f) transform.localScale *= _overlayScale;

            UpdateHeldWeapon();   // draw the held weapon's sprite pinned to the hand
        }

        // ---- Frame-data lookups (TUNING §2.5) --------------------------------
        private static float F => Tuning.FrameSeconds;
        private float WeaponFrameTax => CurrentWeapon.IsFists ? 0f : 2 * F;

        private float PhaseStartup() => Tuning.AttackFrameMult * (_attackKind switch
        {
            AttackKind.Side => SideStartup(_combo),
            AttackKind.Sweep => Tuning.SweepStartup + WeaponFrameTax,
            AttackKind.Finisher => Tuning.FinisherStartup,   // free melee: no weapon tax
            AttackKind.Up or AttackKind.Down => 6 * F,
            AttackKind.AirSide => 5 * F,
            AttackKind.AirUp => 6 * F,
            AttackKind.AirDown => 7 * F,
            AttackKind.Dash => 5 * F,
            _ => 6 * F,
        });

        private float PhaseActive() => _attackKind switch
        {
            AttackKind.Side => SideActive(_combo),
            AttackKind.Sweep => Tuning.SweepActive,
            AttackKind.Finisher => Tuning.FinisherActive,
            AttackKind.Up or AttackKind.Down => 4 * F,
            AttackKind.AirSide => 4 * F,
            AttackKind.AirUp => 5 * F,
            AttackKind.AirDown => 5 * F,
            AttackKind.Dash => 6 * F,          // the lunge
            _ => 4 * F,
        };

        private float PhaseRecovery() => Tuning.AttackFrameMult * (_attackKind switch
        {
            AttackKind.Side => SideRecovery(_combo),
            AttackKind.Sweep => Tuning.SweepRecovery + WeaponFrameTax,
            AttackKind.Finisher => Tuning.FinisherRecovery,
            AttackKind.Up or AttackKind.Down => 12 * F,
            AttackKind.AirSide => 10 * F,
            AttackKind.AirUp => 12 * F,
            AttackKind.AirDown => 8 * F,        // landing lag
            AttackKind.Dash => 10 * F,
            _ => 12 * F,
        });

        private float SideStartup(int c) => (c == 0 ? Tuning.Punch1Startup : Tuning.Punch2Startup) + WeaponFrameTax;
        private float SideActive(int c) => c == 0 ? Tuning.Punch1Active : Tuning.Punch2Active;
        private float SideRecovery(int c) => (c == 0 ? Tuning.Punch1Recovery : Tuning.Punch2Recovery) + WeaponFrameTax;

        private int ComboDamage(int c) => c == 0 ? Tuning.DmgPunch1 : Tuning.DmgPunch2;

        private static bool IsHorizontal(AttackDir d) => d == AttackDir.Left || d == AttackDir.Right;

        // ---- Input helpers ---------------------------------------------------
        // TEMP DEBUG overlay: the deliberate test-key legend + a GOD MODE banner.
        private void OnGUI()
        {
            if (this != Primary) return; // draw the debug overlay once (P1)
            if (GameFlow.Instance != null && GameFlow.Instance.Current != GameFlow.State.Playing) return;
            float scale = Screen.height / 360f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            float w = Screen.width / scale;

            GUI.color = new Color(1f, 1f, 1f, 0.32f);
            GUI.Label(new Rect(6, 342, 360, 16), "DEBUG   I: fill special    O: skip stage    K: god mode");

            if (GodMode)
            {
                GUI.color = new Color(1f, 0.85f, 0.2f, 0.92f);
                var s = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(0, 4, w, 20), "◆ GOD MODE ◆", s);
            }
            GUI.color = Color.white;
        }
    }
}
