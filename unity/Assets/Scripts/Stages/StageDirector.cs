using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Runs one stage of the LINEAR campaign (STAGES.md / ENCOUNTERS.md). A
    /// drop-in replacement for EnemySpawner: add it to the "Systems" object and
    /// call <see cref="StartStage"/> (or set <see cref="AutoStartStage"/> ≥ 0 before
    /// it Start()s). It reads a <see cref="StageData"/> from <see cref="StageDatabase"/>,
    /// sets the area music + ambient bed + backdrop theme, then walks the wave list:
    /// spawns each batch the EnemySpawner way (via <see cref="StageEnemyFactory"/>),
    /// hard-locks the camera (CameraRig.MaxX) until the field clears (watching
    /// <see cref="Actor.All"/>), then extends MaxX and scrolls to the next arena
    /// (ENCOUNTERS.md §0). Filler markers expand into seeded sub-waves (§0). Boss
    /// waves call the <see cref="SpawnBoss"/> hook (placeholder — the bosses agent
    /// fills it). Fires <see cref="OnStageComplete"/> when the final wave/boss ends.
    /// </summary>
    public sealed class StageDirector : MonoBehaviour
    {
        // ---- Public entry points ------------------------------------------------

        /// <summary>Set ≥ 0 to auto-start that stage index on Start() (true drop-in for EnemySpawner).</summary>
        public int AutoStartStage = -1;

        /// <summary>Fired once when the stage's final wave (or boss gate) resolves.</summary>
        public event System.Action OnStageComplete;

        /// <summary>The index (0-based) of the stage currently running, or -1.</summary>
        public int CurrentStageIndex { get; private set; } = -1;

        /// <summary>The running stage's lane length in wu (StageData.LaneLengthWu), published so the
        /// world-fixed set-piece placers (StageBackdropZones, StageFinaleProps) and the obstacle
        /// density ramp anchor to the ACTUAL lane end instead of a stale hardcoded 1600 — otherwise
        /// the finale store / zones landed at 1600 and the level "ended immediately" once the lane
        /// was lengthened.</summary>
        public static float ActiveLaneLengthWu { get; private set; } = 1600f;

        /// <summary>True while a stage is actively running (not idle/complete).</summary>
        public bool Running => _state != DirectorState.Idle && _state != DirectorState.Complete;

        /// <summary>Convenience: find/create the Systems-side director and start a stage.</summary>
        public static StageDirector Begin(int stageIndex)
        {
            var dir = FindAnyObjectByType<StageDirector>();
            if (dir == null)
            {
                var go = new GameObject("StageDirector");
                dir = go.AddComponent<StageDirector>();
            }
            dir.StartStage(stageIndex);
            return dir;
        }

        /// <summary>Start the given campaign stage (0 = Stage 1). Sets audio/backdrop/camera and runs it.</summary>
        public void StartStage(int index)
        {
            var data = StageDatabase.Get(index);
            if (data == null)
            {
                Debug.LogError($"[StageDirector] No stage at index {index} (campaign has {StageDatabase.StageCount}).");
                return;
            }

            _data = data;
            CurrentStageIndex = index;
            ActiveLaneLengthWu = data.LaneLengthWu; // publish for the world-fixed placers + density ramp
            _rng = new System.Random(data.Id); // deterministic seed = stage id (ENCOUNTERS.md §0)

            // Keep the shared HUD statics (EnemySpawner.StageLabel / KillsThisStage) authoritative:
            // other systems read them and the melee/ranged controllers still call NotifyKill().
            EnemySpawner.StageLabel = $"STAGE {data.Id}/{StageDatabase.StageCount}  ·  {data.Area}  ·  {data.DisplayName}";
            EnemySpawner.KillsThisStage = 0;
            // Coin/merc economy debuts Area 3 (stages 6-8 = causeway/farm/dixon); fresh wallet per stage.
            Economy.ResetStage(data.Id <= 3 ? 1 : data.Id <= 5 ? 2 : data.Id <= 8 ? 3 : 4);

            _originX = PlayerController.Instance != null ? PlayerController.Instance.WorldX : 0f;
            ApplyAudio(data);
            ApplyBackdrop(data.BackdropTheme);
            ResolveCameraRig();

            _waves = ExpandWaves(data);
            _gateCount = CountGatingWaves(_waves);
            _gateOrdinal = 0;

            // Lock the camera to the head of the lane until the first wave is placed.
            SetCameraBounds(_originX, GateX(0));

            // Guaranteed weapon drop (e.g. the Whip before the Colossus) — a pickup just ahead of the
            // player so a boss that REQUIRES a weapon can't soft-lock on the random loot roll.
            if (data.GuaranteedWeapon.HasValue)
            {
                float px = PlayerController.Instance != null ? PlayerController.Instance.WorldX : _originX;
                float pz = PlayerController.Instance != null ? PlayerController.Instance.Z : 0f;
                Pickup.SpawnWeapon(data.GuaranteedWeapon.Value, px + 4.5f, pz);
            }

            _waveIndex = -1;
            _boss = null;
            _state = DirectorState.Idle;
            Debug.Log($"[StageDirector] Starting stage {data.Id} \"{data.DisplayName}\" ({_waves.Count} waves, {_gateCount} gates).");
            NextWave();
        }

        // ---- State machine ------------------------------------------------------

        private enum DirectorState { Idle, Vignette, Travel, Spawning, AwaitClear, Boss, Complete }

        private StageData _data;
        private List<Wave> _waves;
        private int _waveIndex;
        private DirectorState _state = DirectorState.Idle;
        private System.Random _rng;

        private float _originX;
        private int _gateCount;      // number of camera-gating waves (Spawn + Boss)
        private int _gateOrdinal;    // how many gates consumed so far

        private CameraRig _rig;
        private Actor _boss;

        // Drip-spawn queue for the current Spawn wave.
        private readonly Queue<PendingUnit> _pending = new();
        private float _dripTimer;
        private float _currentDrip;
        private float _vignetteTimer;

        // Acted-vignette staging (VignetteStaging/VignetteActs): when a stage's Vignette
        // wave has an authored acted sequence we puppeteer it out and advance when it
        // finishes; the timer stays as a fallback. No act → original timer-only wait.
        private VignetteStaging _staging;
        private bool _vignetteHasAct;
        private bool _vignetteActing;

        private struct PendingUnit { public EnemyArchetype Archetype; public SpawnSide Side; }

        private const float VignetteSeconds = 3.5f; // STAGES.md §1c: brief 3–5s vignette

        /// <summary>Global multiplier on every spawn count (creator: "~15% more enemies overall").
        /// Applied per spawn-entry when a wave is queued, so it lifts authored AND filler waves.</summary>
        private const float EnemyCountScale = 1.15f;

        private void Start()
        {
            if (AutoStartStage >= 0) StartStage(AutoStartStage);
        }

        private void Update()
        {
            switch (_state)
            {
                case DirectorState.Vignette:
                    _vignetteTimer -= Time.deltaTime;
                    // Acted: advance when the pantomime finishes (or the fallback timer wins).
                    // Plain: wait the timer out, exactly as before.
                    bool vignetteDone = _vignetteHasAct
                        ? (!_vignetteActing || _vignetteTimer <= 0f)
                        : (_vignetteTimer <= 0f);
                    if (vignetteDone)
                    {
                        if (_staging != null) _staging.Abort(); // no-op if it already finished
                        NextWave();
                    }
                    break;

                case DirectorState.Travel:
                    TickTravel();
                    break;

                case DirectorState.Spawning:
                    TickSpawning();
                    break;

                case DirectorState.AwaitClear:
                    // An arena clears when its PURSUERS are down — pods + their swarmers (position-
                    // triggered by PodDirector, roaming the travel stretches) are excluded so a pod
                    // you left behind never blocks the next arena. Creator: pods give a break, not a gate.
                    if (CountPursuers() == 0)
                    {
                        AdvanceCameraAfterWave();
                        NextWave();
                    }
                    break;

                case DirectorState.Boss:
                    // Placeholder bosses return null → the gate is already resolved in NextWave().
                    if (_boss != null && !_boss.Alive)
                    {
                        _boss = null;
                        AdvanceCameraAfterWave();
                        NextWave();
                    }
                    break;
            }
        }

        // ---- Wave dispatch ------------------------------------------------------

        private void NextWave()
        {
            _waveIndex++;
            if (_waves == null || _waveIndex >= _waves.Count) { Finish(); return; }

            var wave = _waves[_waveIndex];
            switch (wave.Kind)
            {
                case WaveKind.Vignette:
                    _state = DirectorState.Vignette;
                    _vignetteHasAct = VignetteActs.Has(CurrentStageIndex);
                    if (_vignetteHasAct)
                    {
                        // Puppeteer the acted set-piece; timer becomes a long fallback.
                        if (_staging == null) _staging = gameObject.AddComponent<VignetteStaging>();
                        _vignetteActing = true;
                        _vignetteTimer = VignetteActs.MaxSeconds + 1f;
                        _staging.Begin(CurrentStageIndex, () => _vignetteActing = false);
                    }
                    else
                    {
                        _vignetteTimer = VignetteSeconds; // original: brief timer-only wait
                    }
                    Debug.Log($"[StageDirector] {wave.Label}");
                    break;

                case WaveKind.Checkpoint:
                    // Visible marker is level-dressing; here we log + chime (ENCOUNTERS.md §0 checkpoint rule).
                    Sfx.Play("checkpoint_chime");
                    Debug.Log($"[StageDirector] {wave.Label} reached.");
                    NextWave();
                    break;

                case WaveKind.Spawn:
                    BeginSpawnWave(wave);
                    break;

                case WaveKind.Boss:
                    BeginBossWave(wave);
                    break;

                case WaveKind.Filler:
                    // ExpandWaves() should have replaced fillers; skip defensively.
                    NextWave();
                    break;
            }
        }

        // ---- Travel-to-gate (the level is a JOURNEY, not a stationary grind) ----
        private Wave _travelWave;
        private float _travelTargetX, _travelTimeout;
        private bool _travelBoss;      // travelling to the BOSS arena (spawn the boss on arrival, not a wave)

        /// <summary>Open the wall to this wave's gate and make the player WALK UP to it before the wave
        /// spawns — so a stage reads as moving THROUGH the place (past the school, courts, diner, …),
        /// not every wave materialising on top of you (creator: "still the tiny area… no change from
        /// one scene to the next"). Lock + spawn on arrival (see <see cref="TickTravel"/>).</summary>
        private void BeginSpawnWave(Wave wave)
        {
            float gateX = GateX(_gateOrdinal);
            SetCameraBounds(_originX, gateX);       // wall opens up to the gate; player advances into it
            _travelWave = wave;
            _travelTargetX = gateX;
            _travelTimeout = 30f;                   // safety: spawn anyway if the player dawdles
            _state = DirectorState.Travel;
            Debug.Log($"[StageDirector] Travel to gate {gateX:0.0} for {wave.Label}.");
        }

        private void TickTravel()
        {
            _travelTimeout -= Time.deltaTime;
            float px = PlayerController.Primary != null ? PlayerController.MidX() : _originX;
            // Spawn once the player has walked to within ~half a screen of the gate (or dawdled out).
            if (px >= _travelTargetX - Tuning.ScreenWidthUnits * 0.5f || _travelTimeout <= 0f)
            {
                if (_travelBoss) { _travelBoss = false; SpawnBossOnArrival(); }
                else DoSpawnWave(_travelWave);
            }
        }

        private void DoSpawnWave(Wave wave)
        {
            // Camera hard-locks at this wave's gate X until the field clears (ENCOUNTERS.md §0).
            float gateX = GateX(_gateOrdinal);
            SetCameraBounds(_originX, gateX);
            _gateOrdinal++;

            _pending.Clear();
            foreach (var entry in wave.Spawns)
            {
                int n = Mathf.Max(1, Mathf.RoundToInt(entry.Count * EnemyCountScale)); // creator: ~+15% overall
                for (int i = 0; i < n; i++)
                    _pending.Enqueue(new PendingUnit { Archetype = entry.Archetype, Side = entry.Side });
            }

            _currentDrip = Mathf.Max(0.05f, wave.DripSeconds);
            _dripTimer = 0f; // spawn the first unit immediately

            if (_pending.Count == 0) { _state = DirectorState.AwaitClear; return; }
            _state = DirectorState.Spawning;
            Debug.Log($"[StageDirector] {wave.Label}: {_pending.Count} to spawn @ gate {gateX:0.0}.");
        }

        private void TickSpawning()
        {
            _dripTimer -= Time.deltaTime;
            if (_dripTimer > 0f) return;

            // Respect the 8-pursuer cap (Tuning.MaxPursuers): let the field breathe (ENCOUNTERS.md §0).
            if (CountPursuers() >= Tuning.MaxPursuers) { _dripTimer = 0.2f; return; }

            if (_pending.Count == 0) { _state = DirectorState.AwaitClear; return; }

            var unit = _pending.Dequeue();
            SpawnUnit(unit.Archetype, unit.Side);
            _dripTimer = _currentDrip;

            if (_pending.Count == 0) _state = DirectorState.AwaitClear;
        }

        private void BeginBossWave(Wave wave)
        {
            // Don't spawn the boss the instant the last wave clears — open the wall to the arena (the
            // Sandwich Bros lot) and let the player WALK IN; the boss appears ON ARRIVAL (creator). The
            // small pre-boss pod is just an ordinary Spawn wave authored right before this one.
            float arenaX = BossArenaX();
            SetCameraBounds(_originX, arenaX);
            _travelTargetX = arenaX;
            _travelTimeout = 30f;
            _travelBoss = true;
            _state = DirectorState.Travel;
            Debug.Log($"[StageDirector] Travel to boss arena {arenaX:0.0} for {wave.Label}.");
        }

        /// <summary>Spawn the stage boss once the player has walked into the arena (see TickTravel).</summary>
        private void SpawnBossOnArrival()
        {
            if (!string.IsNullOrEmpty(_data.BossMusicClip)) Music.PlayBoss(_data.BossMusicClip);
            float arenaX = BossArenaX();
            SetCameraBounds(_originX, arenaX);

            // Drop a FRESH copy of the required weapon at the arena entrance — the stage-start one may
            // be spent/discarded by now, and the Colossus can't be beaten without a live Whip.
            if (_data.GuaranteedWeapon.HasValue)
                Pickup.SpawnWeapon(_data.GuaranteedWeapon.Value, arenaX, Tuning.ZBandDepth * 0.5f);

            Debug.Log($"[StageDirector] SpawnBoss(\"{_data.BossId}\") @ arena {arenaX:0.0}.");
            _boss = SpawnBoss(_data.BossId, arenaX);
            _state = DirectorState.Boss;

            // Placeholder returned nothing: resolve the gate now so stage-complete still chains.
            if (_boss == null)
            {
                Debug.LogWarning($"[StageDirector] Boss \"{_data.BossId}\" is a placeholder (no actor). " +
                                 "treating the boss gate as cleared.");
                AdvanceCameraAfterWave();
                NextWave();
            }
        }

        private void Finish()
        {
            // Open the far wall so the player can walk to the exit / next-stage trigger.
            if (_rig != null) _rig.MaxX = _originX + _data.LaneLengthWu;
            _state = DirectorState.Complete;
            Debug.Log($"[StageDirector] Stage {_data.Id} complete.");
            OnStageComplete?.Invoke();
        }

        // ---- Boss hook ----------------------------------------------------------

        /// <summary>
        /// Spawn the stage boss and return its root Actor (the director waits for
        /// <c>!boss.Alive</c> to advance, so the stage completes when the boss dies).
        /// Delegates to the <see cref="Bosses"/> registry (BOSSES.md §5.x): it builds
        /// the right <see cref="BossController"/> at the arena head and returns its
        /// <see cref="Actor"/>. An unknown/unimplemented id logs an error and returns
        /// null there, which the director treats as an instantly-cleared gate so the
        /// campaign still chains end-to-end (STAGES.md deliverable §3).
        /// </summary>
        public Actor SpawnBoss(string bossId) => SpawnBoss(bossId, BossArenaX());

        /// <summary>Spawn the boss for the given arena-lock X (see <see cref="BossArenaX"/>).
        /// Places it on the right of the locked view — visibly in front of the player as they
        /// walk into the room, and inside the wall the camera lock puts up (CameraRig.EdgeMargin),
        /// so it is always reachable — rather than at the raw far end of the (now very long) lane.</summary>
        public Actor SpawnBoss(string bossId, float arenaX)
        {
            if (string.IsNullOrEmpty(bossId)) return null;
            // ~8 wu ahead of the lock: inside the ~13.3 wu half-view (Tuning.ScreenWidthUnits/2),
            // so the boss reads as "standing on the right side of the room", not at the edge.
            float x = arenaX + Tuning.ScreenWidthUnits * 0.30f;
            float z = Tuning.ZBandDepth * 0.5f;
            return Bosses.Spawn(bossId, x, z);
        }

        /// <summary>
        /// Camera-lock X for the boss room: one screen-width past the last cleared gate (the
        /// player's actual end position), clamped so it never runs past the authored lane tail.
        /// For a stage with no spawn gates before the boss (pure boss arena, e.g. the finale),
        /// it sits one screen ahead of the lane head. This replaces the old raw `LaneLengthWu - 10`
        /// anchor, which — after the lane was lengthened ~8× — dumped the boss far off-screen.
        /// </summary>
        private float BossArenaX()
        {
            float laneTail = _originX + Mathf.Max(8f, _data.LaneLengthWu - 10f);
            // Finale stages (Sandwich Bros) put the boss at the lane end, by the store — the player
            // clears the last wave then walks the final stretch (over the railroad) into the fight.
            if (_data.BossAtLaneEnd) return laneTail;

            // When the boss wave begins, _gateOrdinal == the number of spawn gates already
            // consumed, so GateX(_gateOrdinal - 1) is the LAST spawn gate the player cleared.
            float lastGateX = _gateOrdinal > 0 ? GateX(_gateOrdinal - 1) : _originX + 8f;
            float arenaX = lastGateX + Tuning.ScreenWidthUnits; // one screen: a distinct room, short walk
            return Mathf.Min(arenaX, laneTail);
        }

        // ---- Endless Mode (STAGES.md §7b) ---------------------------------------

        private EndlessDescriptor _endless;
        private bool _endlessRunning;
        private int _endlessWaveSize;
        private float _endlessRampTimer;

        /// <summary>Start Endless Mode: full roster, refill at 2 remaining, scaling wave size (STAGES.md §7b).</summary>
        public void StartEndless()
        {
            _endless = StageDatabase.Endless();
            _originX = PlayerController.Instance != null ? PlayerController.Instance.WorldX : 0f;
            Music.PlayStage(_endless.MusicClip);
            Music.PlayAmbient(_endless.AmbientClip);
            ApplyBackdrop(_endless.BackdropTheme);
            ResolveCameraRig();
            _endlessWaveSize = _endless.StartWaveSize;
            _endlessRampTimer = _endless.RampEverySeconds;
            _endlessRunning = true;
            _state = DirectorState.Idle; // campaign machine is inert during Endless

            // Endless HUD tally + difficulty ramp: start gentle regardless of the chosen difficulty
            // (which is the ceiling) and climb toward/past it over the first few minutes.
            EnemySpawner.EndlessMode = true;
            EnemySpawner.KillsThisStage = 0;
            Economy.ResetStage(1);   // no coin economy in Endless (Area-1 suburb theme)
            _endlessElapsed = 0f;
            DifficultySettings.EndlessPressure = EndlessPressureStart;
        }

        // The Endless ramp: pressure (enemy count + damage) starts at 0.5× and climbs ~+1.0 over
        // 240s, capping at 1.75× — so even Hard opens at half pressure and escalates endlessly.
        private const float EndlessPressureStart = 0.5f;
        private const float EndlessPressureCap = 1.75f;
        private const float EndlessPressurePer240s = 1.25f; // slope: +1.25 pressure across 240s
        private float _endlessElapsed;

        private void LateUpdate()
        {
            if (!_endlessRunning) return;

            _endlessElapsed += Time.deltaTime;
            DifficultySettings.EndlessPressure = Mathf.Min(
                EndlessPressureCap, EndlessPressureStart + _endlessElapsed / 240f * EndlessPressurePer240s);

            _endlessRampTimer -= Time.deltaTime;
            if (_endlessRampTimer <= 0f)
            {
                _endlessRampTimer = _endless.RampEverySeconds;
                _endlessWaveSize = Mathf.Min(_endless.MaxWaveSize, _endlessWaveSize + 1); // scale up (STAGES.md §7b)
            }

            if (CountLiveEnemies() <= _endless.RefillThreshold)
            {
                int n = Mathf.Min(_endlessWaveSize, Tuning.MaxPursuers - CountPursuers());
                for (int i = 0; i < n; i++)
                {
                    var arch = _endless.Pool[_rng != null ? _rng.Next(_endless.Pool.Length) : Random.Range(0, _endless.Pool.Length)];
                    SpawnUnit(arch, (SpawnSide)((i) % 2)); // alternate L/R
                }
            }
        }

        // ---- Spawning helpers ---------------------------------------------------

        // Share of flank spawns that come from AHEAD (the +X advance direction). The player pushes
        // right through the lane, so -X is the already-cleared ground "behind" them; authoring often
        // used SpawnSide.L which put most enemies back there (creator: "most enemies spawn behind the
        // player"). We override L/R/A to a front-weighted coin-flip so the majority arrive in front,
        // with a minority still flanking from behind ("some are okay").
        private const float FrontSpawnBias = 0.72f;

        private void SpawnUnit(EnemyArchetype archetype, SpawnSide side)
        {
            float anchorX = GateX(Mathf.Max(0, _gateOrdinal - 1));
            if (PlayerController.Primary != null) anchorX = PlayerController.MidX(); // enter the shared frame

            float half = Tuning.ScreenWidthUnits * 0.5f + 1f;
            const float front = +1f; // the advance direction is +X
            float x, z;
            switch (side)
            {
                case SpawnSide.B:
                case SpawnSide.Air:
                    // Far-depth funnel (bus pass / top of screen): keep as authored.
                    x = anchorX + Rand(-2f, 2f); z = Tuning.ZBandDepth - 0.5f; break;
                default: // L / R / A → front-biased flank so most come from ahead, not the cleared side
                    float dir = (Rand(0f, 1f) < FrontSpawnBias) ? front : -front;
                    x = anchorX + dir * half; z = RandNear(); break;
            }
            StageEnemyFactory.Spawn(archetype, x, Mathf.Clamp(z, 0f, Tuning.ZBandDepth));
        }

        private float RandNear() => Rand(1f, Tuning.ZBandDepth - 0.5f);
        private float Rand(float a, float b) => a + (float)(_rng != null ? _rng.NextDouble() : Random.value) * (b - a);

        // ---- Field queries (watch Actor.All) ------------------------------------

        private static int CountLiveEnemies()
        {
            int n = 0;
            foreach (var a in Actor.All)
                if (a.Alive && a.Team == Team.Enemy) n++;
            return n;
        }

        /// <summary>Pursuers for the 8-cap: living enemies excluding Pods and pod-spawned swarmers
        /// (ENEMIES.md swarm exception), matching EnemySpawner's rule.</summary>
        private static int CountPursuers()
        {
            int n = 0;
            foreach (var a in Actor.All)
            {
                if (!a.Alive || a.Team != Team.Enemy) continue;
                if (a is Pod) continue;
                if (a is EnemyController e && e.Def != null && e.Def.Id == "swarmer") continue;
                n++;
            }
            return n;
        }

        // ---- Camera & lane geometry (ENCOUNTERS.md §0) --------------------------

        private void ResolveCameraRig()
        {
            if (_rig != null) return;
            // Get the rig from Camera.main if it has one, but ALWAYS fall back to a scene-wide search:
            // a stray/second camera can make Camera.main the wrong (rig-less) camera, which used to
            // leave _rig null and SILENTLY DISABLE ALL GATING (no wave lock, boss never reached). The
            // rig lives on our code camera regardless of which camera .main resolves to.
            var cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            _rig = cam != null ? cam.GetComponent<CameraRig>() : null;
            if (_rig == null) _rig = FindAnyObjectByType<CameraRig>();
            if (_rig == null) Debug.LogWarning("[StageDirector] No CameraRig found — camera gating disabled.");
        }

        private void SetCameraBounds(float minX, float maxX)
        {
            if (_rig == null) return;
            _rig.MinX = minX;            // never scroll left of the lane head (ENCOUNTERS.md §0)
            _rig.MaxX = Mathf.Max(minX, maxX);
        }

        /// <summary>Gate X for the ordinal gating wave: first at ~8 wu in, then evenly along the lane (§0).</summary>
        private float GateX(int ordinal)
        {
            float head = _originX + 8f;
            if (_gateCount <= 1) return head;
            float span = Mathf.Max(0f, _data.LaneLengthWu - 8f - 10f); // reserve ~10 wu tail for the boss arena
            float step = span / (_gateCount - 1);
            return head + Mathf.Clamp(ordinal, 0, _gateCount - 1) * step;
        }

        private void AdvanceCameraAfterWave()
        {
            // Unlock forward: open the wall to the next gate (or the lane end on the last gate).
            float nextX = _gateOrdinal < _gateCount ? GateX(_gateOrdinal) : _originX + _data.LaneLengthWu;
            SetCameraBounds(_originX, nextX);
        }

        private static int CountGatingWaves(List<Wave> waves)
        {
            int n = 0;
            foreach (var w in waves)
                if (w.Kind == WaveKind.Spawn || w.Kind == WaveKind.Boss) n++;
            return n;
        }

        // ---- Audio & backdrop ---------------------------------------------------

        private static void ApplyAudio(StageData data)
        {
            if (!string.IsNullOrEmpty(data.MusicClip)) Music.PlayStage(data.MusicClip);
            if (!string.IsNullOrEmpty(data.AmbientClip)) Music.PlayAmbient(data.AmbientClip);
        }

        /// <summary>
        /// Point the backdrop at the stage's theme. The tiered Backdrop supports
        /// per-area palettes; <see cref="Backdrop.SetTheme"/> maps the theme stem
        /// (e.g. "area2_airport") to its area index and rebuilds the bands. Sets the
        /// shared area static BEFORE ensuring a Backdrop exists so a freshly created
        /// one inherits the right palette.
        /// </summary>
        private void ApplyBackdrop(string theme)
        {
            Backdrop.SetTheme(theme);                       // sets shared area + rebuilds if a Backdrop exists
            if (FindAnyObjectByType<Backdrop>() == null) Backdrop.Create();
        }

        // ---- Filler expansion (ENCOUNTERS.md §0) --------------------------------

        /// <summary>Expand Filler markers into concrete seeded sub-waves; pass everything else through.</summary>
        private List<Wave> ExpandWaves(StageData data)
        {
            var outWaves = new List<Wave>(data.Waves.Count + 16);
            foreach (var w in data.Waves)
            {
                if (w.Kind != WaveKind.Filler) { outWaves.Add(w); continue; }

                // FEWER, MEATIER arenas. The old expansion made ~12 micro-gates only a few wu apart,
                // so the camera barely moved between locks and it read as one endless fight, not
                // distinct rooms (creator: "you basically made a 400wu area"). Collapse the block into
                // 3–5 bigger arenas: each is a proper screen-lock room (its total drips in past the
                // 8-on-screen cap) with real travel between, matching the JS-version feel.
                // Arena count SCALES WITH LANE LENGTH so a longer stage stays paced: one filler arena
                // per ArenaSpacingWu (creator: "locking camera every 6k or so"), leaving real travel
                // (obstacles + pods) between distinct screen-lock rooms rather than 14 micro-gates a
                // few wu apart. (Short/legacy lanes clamp back to 3; very long ones cap at 20.)
                float spacing = Mathf.Max(100f, data.ArenaSpacingWu);
                int count = Mathf.Clamp(Mathf.RoundToInt(data.LaneLengthWu / spacing), 3, 20);
                var pool = BuildFillerPool(data);
                for (int i = 0; i < count; i++)
                {
                    // Total per arena ramps 7 → 12 across the block (NOT capped at the 8 on-screen
                    // limit — that cap is enforced live by the drip, so the room holds more overall).
                    int size = count <= 1 ? 10 : Mathf.RoundToInt(Mathf.Lerp(7f, 12f, (float)i / (count - 1)));
                    var wave = Wave.Spawn($"{w.Label} #{i + 1}", 0.8f, BuildFillerBatch(data, pool, size));
                    outWaves.Add(wave);
                }
            }
            return outWaves;
        }

        private static int MidpointRoundedUp(int lo, int hi) => (lo + hi + 1) / 2;

        /// <summary>Weighted pool: 60% toward the newest type, 40% split across the rest (§0).</summary>
        private List<EnemyArchetype> BuildFillerPool(StageData data)
        {
            // Derive the "rest" from the union of archetypes named in the stage's spawn waves.
            var rest = new List<EnemyArchetype>();
            foreach (var w in data.Waves)
                if (w.Kind == WaveKind.Spawn)
                    foreach (var e in w.Spawns)
                        if (e.Archetype != data.NewestArchetype && e.Archetype != EnemyArchetype.Pod && !rest.Contains(e.Archetype))
                            rest.Add(e.Archetype);
            if (rest.Count == 0) rest.Add(EnemyArchetype.Regular);

            // A 10-slot weighted bag: 6 newest, 4 spread across the rest.
            var bag = new List<EnemyArchetype>(10);
            for (int i = 0; i < 6; i++) bag.Add(data.NewestArchetype);
            for (int i = 0; i < 4; i++) bag.Add(rest[i % rest.Count]);
            return bag;
        }

        private SpawnEntry[] BuildFillerBatch(StageData data, List<EnemyArchetype> bag, int size)
        {
            // Draw `size` from the weighted bag; group identical picks into entries, alternating sides.
            var counts = new Dictionary<EnemyArchetype, int>();
            for (int i = 0; i < size; i++)
            {
                var pick = bag[_rng.Next(bag.Count)];
                counts.TryGetValue(pick, out int c);
                counts[pick] = c + 1;
            }
            var entries = new List<SpawnEntry>(counts.Count);
            int idx = 0;
            foreach (var kv in counts)
            {
                var side = (idx++ % 2 == 0) ? SpawnSide.L : SpawnSide.R;
                entries.Add(new SpawnEntry(kv.Key, kv.Value, side));
            }
            return entries.ToArray();
        }
    }
}
