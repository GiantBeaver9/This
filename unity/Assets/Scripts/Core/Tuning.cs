namespace ThisL
{
    /// <summary>
    /// The single source of gameplay numbers, mirrored from the design bible's
    /// authoritative <c>TUNING.md</c>. Every value here cites the section it comes
    /// from so the code and the doc can be reconciled. Distances are in world
    /// units (wu); 1 wu = 24 px at the internal 640x360 render (§0 / §1).
    /// </summary>
    public static class Tuning
    {
        // ---- §1 World & camera (Z-band) --------------------------------------
        public const float PixelsPerUnit = 24f;          // 1 wu = 24 px
        public const int InternalWidth = 640;            // internal render width (px)
        public const int InternalHeight = 360;           // internal render height (px)
        public const float ScreenWidthUnits = 26.6667f;  // 640 / 24
        public const float PlayfieldBandShare = 0.60f;   // bottom 60% is the playfield
        public const float ZBandDepth = 6.0f;            // near Z=0 .. far Z=6 (standard)
        public const float ZToScreenPxPerUnit = 24f;     // +1 wu of Z lifts screen-Y by 24 px
        public const float DepthScaleNear = 1.00f;       // 100% at Z=0
        public const float DepthScaleFar = 0.80f;        // 80% at Z=6 (linear, floor 0.80)
        public const float HitboxZTolerance = 0.4f;      // enemy/projectile depth tolerance (kept tight = fair)
        public const float PlayerHitZTolerance = 0.65f;  // up/down strike depth window
        public const float SideArcZTolerance = 1.7f;     // WIDE ~90° punch arc in front (creator: open it up, not just dead-ahead)
        public const float SweepZTolerance = 1.3f;       // combo sweep wide arc
        public const float PursuerSeparation = 1.0f;     // enemy<->enemy min center distance
        public const int MaxAttackers = 4;               // melee ring: how many commit a swing at once (more trading)
        public const float StandoffRing = 2.8f;          // the rest circle at this distance
        public const float EnemySpeedMult = 0.8f;        // global 20% slower enemy movement (pace)
        public const float EnemyAttackZTolerance = 0.6f; // forgiving depth window when an enemy commits its hit
        public const float EnemyReshuffleSeconds = 2.5f; // pass attack priority to different enemies this often
        public const float DashKnockback = 9.0f;         // wu/s shove dashing gives enemies it plows
        public const float DashPlowRadius = 1.1f;        // contact radius for the dash shove
        public const int MaxPursuers = 8;                // (legacy) baseline pursue cap

        // ---- Beat-'em-up horde feel (back-off rhythm + ramp) -----------------
        public const float BackoffMin = 0.3f;            // after a swing, retreat this long (min) — re-engage fast to trade
        public const float BackoffMax = 0.65f;           // ...max (randomised per enemy)
        public const float BackoffSpeedMult = 0.85f;     // retreat a touch slower than approach
        public const float ZSpreadBias = 1.6f;           // ± depth offset each pursuer holds (Z-spread)

        // Ramping horde spawner — escalates by STAGE: +2 on-screen enemies per stage,
        // climbing from the opener to a 30-enemy wall at the finale (creator ruling).
        public const int HordeStartCount = 16;           // on-screen target at stage 1
        public const int HordePerStage = 2;              // added each stage
        public const int HordeMaxCount = 30;             // ceiling at the final stage
        public const float HordeRampSeconds = 8f;        // (legacy) practice ramp cadence
        public const float HordeSpawnInterval = 0.6f;    // how fast the field refills toward target

        // Tuning philosophy (creator ruling): START HARD at each stage's peak so the
        // tension points are visible, then pare the numbers DOWN — don't build up.
        // Flip to false to get the gentle warm-up ramp instead.
        public static readonly bool StartHardMode = true;

        // Item drops (reward + survivability while it's tuned hard).
        public const float WeaponDropChance = 0.09f;     // ~9% of loot-bearing kills drop a weapon (dense stages like Sacramento were carpeted)
        public const float WeaponPickupLifetime = 15f;   // dropped weapons despawn after 15s (creator ruling)
        public const float HealDropChance = 0.12f;       // base heal-drop chance on a kill
        public const float HealDropChanceLowHp = 0.30f;  // when the player is <=25% HP

        public const float SpriteHeightUnits = 2.0f;     // player sprite = 2.0 wu tall

        // ---- §2.1 Fist damage -------------------------------------------------
        public const int DmgPunch1 = 10;
        public const int DmgPunch2 = 10;
        public const int DmgSweep = 12;                  // knocks the enemy DOWN
        public const int DmgFinisher = 35;               // only lands on a downed enemy
        public const int DmgAirSide = 8;
        public const int DmgUpStrike = 10;
        public const int DmgDownStrike = 10;
        public const int DmgDashAttack = 0;              // stagger only

        // ---- §2.1 Melee reach -------------------------------------------------
        public const float FistReach = 1.8f;             // +0.3 wu reach (creator: extend 5-10px) -> 2.4 with gust
        public const float GustBonus = 0.6f;
        public const float SweepReach = 1.9f;
        public const float SwordReach = 2.1f;

        // Directional attacks (horde-hell): each strike hits a focused lane in the
        // PRESSED direction — ←/→ in X, ↑/↓ into depth (Z) — with a narrow
        // perpendicular window, so you can't clear all around you at once.
        public const float StrikeZReach = 1.4f;          // up/down strike reach into depth
        public const float StrikePerpX = 0.55f;          // up/down strike half-width in X
        public const float AttackFrameMult = 1.15f;      // pace player attacks a touch slower (tunable)

        // ---- §2.2 Movement, dash, jump ---------------------------------------
        public const float RunSpeed = 5.6f;              // wu/s on X and Z (−20% pace pass)
        public const float WalkSpeed = 3.6f;
        public const float DashDistance = 4.0f;          // dash keeps its reach (evasive tool)
        public const float DashDuration = 0.22f;         // a hair slower burst; no i-frames
        public const float DashCooldown = 0.50f;         // min gap between consecutive dashes
        public const int DashMaxCharges = 3;             // dash charges available at once
        public const float DashChargeWindow = 5.0f;      // 3 dashes per 5s: charges regen at Max/Window per sec
        public const float AirDashDistance = 3.5f;
        public const float AirDashDuration = 0.18f;
        public const float JumpHeight = 3.0f;
        public const float JumpDuration = 0.80f;
        public const float AirSpeed = 5.0f;              // horizontal air-control speed (−20%)
        public const float LandingRecovery = 0.08f;
        public const float WeaponWarmup = 0.25f;         // baseline per weapon
        public const float HitstunDuration = 0.25f;      // taking a hit; no i-frames
        public const float LowHpThreshold = 25f;         // rubber-band <=25 HP

        // ---- §2.4 Special meter ----------------------------------------------
        public const float MeterFull = 100f;             // one fill (yellow)
        public const float MeterMax = 300f;              // green cap (3 fills)
        public const float MeterPerFistHit = 3.34f;      // ~30 hits per fill
        public const float MeterPerWeaponHit = 1.67f;    // half the fist rate
        public const float ComboDropTimeout = 2.0f;      // 2s without a hit resets combo
        public const float SniperRifleMeter = 100f;      // killed-Sniper pickup = one fill

        // ---- §2.5 Player attack frame data (authoritative @ 60 fps) -----------
        public const float FrameSeconds = 1f / 60f;
        // Punch 1 (jab): 4f startup / 3f active / 8f recovery
        public const float Punch1Startup = 4 * FrameSeconds;
        public const float Punch1Active = 3 * FrameSeconds;
        public const float Punch1Recovery = 8 * FrameSeconds;
        // Punch 2 (cross): 5 / 3 / 9
        public const float Punch2Startup = 5 * FrameSeconds;
        public const float Punch2Active = 3 * FrameSeconds;
        public const float Punch2Recovery = 9 * FrameSeconds;
        // Sweep (hit 3): 8 / 4 / 14
        public const float SweepStartup = 8 * FrameSeconds;
        public const float SweepActive = 4 * FrameSeconds;
        public const float SweepRecovery = 14 * FrameSeconds;
        // Finisher (hit 4): 6 / 4 / 16
        // The STOMP finisher is a deliberate COMMITMENT: a real wind-up + a long, VULNERABLE
        // recovery (no i-frames) so you can't button-mash executions in a crowd — you have to
        // create space first (creator ruling). ~0.85s total at 60fps × AttackFrameMult.
        public const float FinisherStartup = 9 * FrameSeconds;
        public const float FinisherActive = 5 * FrameSeconds;
        public const float FinisherRecovery = 30 * FrameSeconds;
        public const float InputBuffer = 9 * FrameSeconds; // ~0.15s buffer

        // ---- Player -----------------------------------------------------------
        public const int PlayerMaxHp = 100;
        public const int HealRestore = 25;               // flat +25 per pickup, no full heals

        // ---- Lives (shared team pool, co-op + solo) ---------------------------
        // Start with 3 shared lives; +1 on each AREA clear (~4 area transitions over the
        // 13-stage campaign). A player death spends a life and respawns them; empty pool
        // on a death that leaves nobody standing = GAME OVER (creator spec).
        public const int StartingLives = 3;              // shared pool at run start
        public const int LivesPerAreaClear = 1;          // awarded when an area is finished
        public const float RespawnDelay = 1.5f;          // downed -> respawn beat
        public const float RespawnInvuln = 2.0f;         // i-frames granted on respawn

        // ---- Animation --------------------------------------------------------
        public const int AnimFps = 12;                   // sprites animate at 12 fps
    }
}
