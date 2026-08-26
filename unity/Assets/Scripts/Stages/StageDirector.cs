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
            _rng = new System.Random(data.Id); // deterministic seed = stage id (ENCOUNTERS.md §0)

            // Keep the shared HUD statics (EnemySpawner.StageLabel / KillsThisStage) authoritative:
            // other systems read them and the melee/ranged controllers still call NotifyKill().
            EnemySpawner.StageLabel = $"STAGE {data.Id}/{StageDatabase.StageCount}  ·  {data.Area}  ·  {data.DisplayName}";
            EnemySpawner.KillsThisStage = 0;

            _originX = PlayerController.Instance != null ? PlayerController.Instance.WorldX : 0f;
            ApplyAudio(data);
            ApplyBackdrop(data.BackdropTheme);
            ResolveCameraRig();

            _waves = ExpandWaves(data);
            _gateCount = CountGatingWaves(_waves);
            _gateOrdinal = 0;

            // Lock the camera to the head of the lane until the first wave is placed.
            SetCameraBounds(_originX, GateX(0));

            _waveIndex = -1;
            _boss = null;
            _state = DirectorState.Idle;
            Debug.Log($"[StageDirector] Starting stage {data.Id} \"{data.DisplayName}\" ({_waves.Count} waves, {_gateCount} gates).");
            NextWave();
        }

        // ---- State machine ------------------------------------------------------

        private enum DirectorState { Idle, Vignette, Spawning, AwaitClear, Boss, Complete }

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

                case DirectorState.Spawning:
                    TickSpawning();
                    break;

                case DirectorState.AwaitClear:
                    if (CountLiveEnemies() == 0)
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

        private void BeginSpawnWave(Wave wave)
        {
            // Camera hard-locks at this wave's gate X until the field clears (ENCOUNTERS.md §0).
            float gateX = GateX(_gateOrdinal);
            SetCameraBounds(_originX, gateX);
            _gateOrdinal++;

            _pending.Clear();
            foreach (var entry in wave.Spawns)
                for (int i = 0; i < entry.Count; i++)
                    _pending.Enqueue(new PendingUnit { Archetype = entry.Archetype, Side = entry.Side });

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
            if (!string.IsNullOrEmpty(_data.BossMusicClip)) Music.PlayBoss(_data.BossMusicClip);
            // Boss arena caps the tail (ENCOUNTERS.md §0): lock camera to the arena head.
            float bossX = _originX + Mathf.Max(0f, _data.LaneLengthWu - 10f);
            SetCameraBounds(_originX, bossX);

            Debug.Log($"[StageDirector] {wave.Label} — SpawnBoss(\"{_data.BossId}\").");
            _boss = SpawnBoss(_data.BossId);
            _state = DirectorState.Boss;

            // Placeholder returned nothing: resolve the gate now so stage-complete still chains.
            if (_boss == null)
            {
                Debug.LogWarning($"[StageDirector] Boss \"{_data.BossId}\" is a placeholder (no actor). " +
                                 "Bosses agent fills SpawnBoss(); treating the boss gate as cleared.");
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
        public Actor SpawnBoss(string bossId)
        {
            if (string.IsNullOrEmpty(bossId)) return null;
            // Place the boss at the arena head (the tail of the lane the player just cleared).
            float x = _originX + Mathf.Max(6f, _data.LaneLengthWu - 6f);
            float z = Tuning.ZBandDepth * 0.5f;
            return Bosses.Spawn(bossId, x, z);
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
        }

        private void LateUpdate()
        {
            if (!_endlessRunning) return;

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

        private void SpawnUnit(EnemyArchetype archetype, SpawnSide side)
        {
            float anchorX = GateX(Mathf.Max(0, _gateOrdinal - 1));
            if (PlayerController.Primary != null) anchorX = PlayerController.MidX(); // enter the shared frame

            float half = Tuning.ScreenWidthUnits * 0.5f + 1f;
            float x, z;
            switch (side)
            {
                case SpawnSide.L: x = anchorX - half; z = RandNear(); break;
                case SpawnSide.R: x = anchorX + half; z = RandNear(); break;
                case SpawnSide.B: x = anchorX + Rand(-2f, 2f); z = Tuning.ZBandDepth - 0.5f; break;
                case SpawnSide.Air: x = anchorX + Rand(-2f, 2f); z = Tuning.ZBandDepth - 0.5f; break;
                case SpawnSide.A: x = anchorX + (Rand(0f, 1f) < 0.5f ? -half : half); z = RandNear(); break;
                default: x = anchorX + half; z = RandNear(); break;
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
            var cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            _rig = cam != null ? cam.GetComponent<CameraRig>() : FindAnyObjectByType<CameraRig>();
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

                int count = MidpointRoundedUp(w.FillerMinWaves, w.FillerMaxWaves); // §0: midpoint, rounded up
                var pool = BuildFillerPool(data);
                for (int i = 0; i < count; i++)
                {
                    // Size ramps linearly 4 → 6 across the block (§0).
                    int size = count <= 1 ? 4 : Mathf.RoundToInt(Mathf.Lerp(4f, 6f, (float)i / (count - 1)));
                    size = Mathf.Min(size, Tuning.MaxPursuers);
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
