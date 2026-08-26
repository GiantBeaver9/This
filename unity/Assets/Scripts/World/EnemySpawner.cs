using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Position-triggered encounter runner (STAGES.md §2/§4, creator feedback:
    /// "levels need spawn points that trigger upon reaching a certain area").
    /// Replaces the old time-based horde. Each stage is a short LANE of encounter
    /// GATES at increasing world-X. The player walks right freely; when they reach
    /// the next gate the camera hard-locks there (<see cref="CameraRig.MaxX"/>) and
    /// that gate's WAVE arrives from the screen edges. Only a wave's worth of enemies
    /// are on-screen at once (a fraction of the stage peak) — the wave refills as they
    /// die until its quota is met. Clear the field and the camera unlocks so the
    /// player can advance to the next gate; clear the last gate and the stage advances
    /// (keeping the 12-stage table + per-stage music/area + Area-1 no-shooters rule).
    /// All values are first-pass and tunable.
    /// </summary>
    public sealed class EnemySpawner : MonoBehaviour
    {
        // Kept for other callers (EnemyController/RangedEnemyController call NotifyKill;
        // Hud/other systems may read the label/count). Now a running per-stage tally.
        public static int KillsThisStage;
        public static string StageLabel = "";

        public static void NotifyKill() => KillsThisStage++;

        // ---- Tunable knobs ------------------------------------------------------
        [Header("Lane / gates")]
        public float GateSpacingWu = 14f;   // distance between gates (creator: ~12-16 wu)
        public int   GatesPerStage = 3;     // Areas 3-4 get +1 (see GateCountFor)
        public float LockInSeconds = 0.6f;  // brief beat between reaching a gate and the wave arriving

        [Header("Wave sizing (fractions of the stage peak)")]
        public float ConcurrentFractionHard = 0.60f; // on-screen at once (hard mode)
        public float ConcurrentFractionEasy = 0.45f; // on-screen at once (easy mode)
        public int   MinConcurrent = 3;
        public float QuotaMinFraction = 0.70f;  // first gate's total ≈ 70% of peak
        public float QuotaMaxFraction = 1.10f;  // last gate's total ≈ 110% of peak
        public float GunnerChance = 0.22f;      // share of a wave that spawns as gunners (Area 2+)

        // ---- Stage table (unchanged: per-stage peak / shooters / area / music) --
        private sealed class StageProfile
        {
            public int Area;
            public string Name;
            public int Start;          // concurrent target early in the stage (legacy floor)
            public int Peak;           // concurrent target by the end of the ramp -> wave sizing basis
            public float Warmup;       // (legacy) unused by gate model, kept for data parity
            public float Ramp;         // (legacy) unused by gate model, kept for data parity
            public bool Shooters;      // gunners allowed (Area 2+)
            public int KillsToAdvance; // (legacy) informs total quota sanity; advance is now gate-driven
            public string Music, Ambient;
        }

        // 12 combat stages (Area 1 = 3, Area 2 = 2, Area 3 = 3, Area 4 = 4). Finale (Phil) is a boss, not here.
        private static readonly StageProfile[] Stages =
        {
            new(){ Area=1, Name="Lincoln High",       Start=2,  Peak=12, Warmup=80, Ramp=150, Shooters=false, KillsToAdvance=32, Music="a1_surfrock_opener",   Ambient="lincoln_birds_traffic" },
            new(){ Area=1, Name="Rocklin",            Start=4,  Peak=12, Warmup=30, Ramp=120, Shooters=false, KillsToAdvance=34, Music="a1_surfrock_opener",   Ambient="lincoln_birds_traffic" },
            new(){ Area=1, Name="Roseville Galleria", Start=5,  Peak=14, Warmup=20, Ramp=120, Shooters=false, KillsToAdvance=36, Music="a1_synthpunk_mall",    Ambient="galleria_murmur" },
            new(){ Area=2, Name="Sacramento Old-Town",Start=6,  Peak=14, Warmup=15, Ramp=120, Shooters=true,  KillsToAdvance=38, Music="a2_ragtime_garagerock",Ambient="sacramento_oldtown" },
            new(){ Area=2, Name="Sacramento Airport", Start=6,  Peak=16, Warmup=15, Ramp=120, Shooters=true,  KillsToAdvance=40, Music="a2_industrial_electronic",Ambient="airport_tarmac_jet" },
            new(){ Area=3, Name="Hills",              Start=7,  Peak=16, Warmup=15, Ramp=120, Shooters=true,  KillsToAdvance=42, Music="a3_spaghetti_western",  Ambient="marin_redwood" },
            new(){ Area=3, Name="Davis Causeway",     Start=7,  Peak=16, Warmup=15, Ramp=120, Shooters=true,  KillsToAdvance=42, Music="a3_western_dread",      Ambient="causeway_marsh_wind" },
            new(){ Area=3, Name="Dixon",              Start=8,  Peak=18, Warmup=12, Ramp=120, Shooters=true,  KillsToAdvance=44, Music="a3_hoedown_bluegrass",  Ambient="farm_barnyard" },
            new(){ Area=4, Name="Vallejo",            Start=9,  Peak=18, Warmup=12, Ramp=120, Shooters=true,  KillsToAdvance=46, Music="a4_circusrock",        Ambient="vallejo_carnival" },
            new(){ Area=4, Name="Marin",              Start=10, Peak=20, Warmup=12, Ramp=120, Shooters=true,  KillsToAdvance=48, Music="a4_electropunk",       Ambient="marin_redwood" },
            new(){ Area=4, Name="Golden Gate Bridge", Start=10, Peak=20, Warmup=12, Ramp=120, Shooters=true,  KillsToAdvance=50, Music="a4_electropunk",       Ambient="goldengate_bridge_wind" },
            new(){ Area=4, Name="San Francisco",      Start=12, Peak=22, Warmup=10, Ramp=120, Shooters=true,  KillsToAdvance=52, Music="a4_circusrock",        Ambient="sf_city_crowd" },
        };

        // ---- Runtime state ------------------------------------------------------
        private enum Phase { Advancing, LockIn, Fighting, StageClear }

        private int _stage;
        private Phase _phase;
        private float _originX;     // player's X when this stage began; gates are laid out ahead of it
        private int _gateCount;
        private int _gate;          // index of the gate currently being approached / fought
        private float _minLeash;    // running MinX (never decreases → no backtracking past cleared ground)

        private float _lockTimer;   // LockIn beat before the wave arrives
        private float _spawnTimer;

        // Current wave bookkeeping.
        private int _waveQuota;     // total enemies this wave will spawn before it can clear
        private int _waveSpawned;   // spawned so far this wave
        private int _waveConcurrent;// max on-screen at once for this wave
        private bool _wavePodDue;   // this (bigger) gate should drop a Pod
        private bool _wavePodPlaced;

        private CameraRig _rig;

        private void Start() => EnterStage(0);

        // ---- Stage lifecycle ----------------------------------------------------
        private void EnterStage(int i)
        {
            _stage = Mathf.Clamp(i, 0, Stages.Length - 1);
            var s = Stages[_stage];

            ResolveRig();
            KillsThisStage = 0;

            var player = PlayerController.Instance;
            _originX = player != null ? player.WorldX : 0f;
            _gateCount = GateCountFor(s);
            _gate = 0;
            _minLeash = _originX - Tuning.ScreenWidthUnits; // allow a screen of run-up room behind
            ApplyBounds(GateX(0));                          // open the lane to the first gate
            BeginAdvancing();

            StageLabel = $"STAGE {_stage + 1}/12  ·  AREA {s.Area}  ·  {s.Name}";
            if (!string.IsNullOrEmpty(s.Music)) Music.PlayStage(s.Music);
            if (!string.IsNullOrEmpty(s.Ambient)) Music.PlayAmbient(s.Ambient);
            Debug.Log($"[Stage] {StageLabel} — {_gateCount} gates every {GateSpacingWu:0.#}wu (peak {s.Peak}, shooters {s.Shooters})");
        }

        private int GateCountFor(StageProfile s) => Mathf.Max(1, GatesPerStage + (s.Area >= 3 ? 1 : 0));

        // Gate g sits g+1 spacings ahead of where the stage began.
        private float GateX(int g) => _originX + GateSpacingWu * (g + 1);

        private void BeginAdvancing()
        {
            _phase = Phase.Advancing;
        }

        // ---- Frame loop ---------------------------------------------------------
        private void Update()
        {
            var player = PlayerController.Instance;
            if (player == null || !player.Alive) return;

            float dt = Time.deltaTime;

            // Trail MinX behind the player: cleared ground stays behind a wall.
            _minLeash = Mathf.Max(_minLeash, player.WorldX - Tuning.ScreenWidthUnits);

            switch (_phase)
            {
                case Phase.Advancing:  TickAdvancing(player); break;
                case Phase.LockIn:     TickLockIn(dt); break;
                case Phase.Fighting:   TickFighting(player, dt); break;
                case Phase.StageClear: break; // terminal (last stage): field open, nothing spawns
            }
        }

        // Walk right until the player reaches the next gate line, then lock in the wave.
        private void TickAdvancing(PlayerController player)
        {
            float gateX = GateX(_gate);
            ApplyBounds(gateX); // camera already stops at the gate; MinX trails the player
            if (player.WorldX >= gateX)
            {
                _phase = Phase.LockIn;
                _lockTimer = LockInSeconds;
                ConfigureWave();
                Sfx.Play("checkpoint_chime");
                Debug.Log($"[Gate] Stage {_stage + 1} gate {_gate + 1}/{_gateCount} reached @ X{gateX:0.0} " +
                          $"— wave {_waveSpawned}/{_waveQuota} (max {_waveConcurrent} at once).");
            }
        }

        private void TickLockIn(float dt)
        {
            ApplyBounds(GateX(_gate)); // hard lock at the gate during the beat
            _lockTimer -= dt;
            if (_lockTimer <= 0f)
            {
                _phase = Phase.Fighting;
                _spawnTimer = 0f;
            }
        }

        // Refill the wave up to its concurrent cap until the quota is spawned; clear
        // when the quota is met AND the field is empty, then unlock and advance.
        private void TickFighting(PlayerController player, float dt)
        {
            ApplyBounds(GateX(_gate)); // stay locked at the gate for the whole fight

            // The (bigger) gate's Pod: one sac, dropped once the fight is underway.
            if (_wavePodDue && !_wavePodPlaced && CountPods() < 1)
            {
                PlacePod(player.WorldX + Random.Range(-6f, 6f), Random.Range(1f, Tuning.ZBandDepth - 1f));
                _wavePodPlaced = true;
            }

            var s = Stages[_stage];

            _spawnTimer -= dt;
            if (_spawnTimer <= 0f)
            {
                _spawnTimer = Tuning.HordeSpawnInterval;

                bool roomOnScreen = CountPursuers() < Mathf.Min(_waveConcurrent, Tuning.MaxPursuers);
                if (_waveSpawned < _waveQuota && roomOnScreen)
                {
                    if (s.Shooters && Random.value < GunnerChance) SpawnGunner(player);
                    else SpawnRegular(player);
                    _waveSpawned++;
                }
            }

            // Wave cleared: full quota spawned and nothing left alive (pods + swarmers included).
            if (_waveSpawned >= _waveQuota && CountLiveEnemies() == 0)
                OnWaveCleared();
        }

        private void OnWaveCleared()
        {
            _gate++;
            if (_gate >= _gateCount)
            {
                // Last gate down: hand off to the next stage (or park on the finale).
                if (_stage < Stages.Length - 1) { EnterStage(_stage + 1); return; }
                _phase = Phase.StageClear;
                if (_rig != null) _rig.MaxX = _originX + GateSpacingWu * (_gateCount + 2); // open the far wall
                Debug.Log("[Stage] Final combat stage cleared.");
                return;
            }

            // Unlock forward to the next gate and let the player advance.
            ApplyBounds(GateX(_gate));
            BeginAdvancing();
            Debug.Log($"[Gate] Wave cleared — advancing to gate {_gate + 1}/{_gateCount}.");
        }

        // Size the wave off the stage peak: quota ramps across gates, only a fraction
        // is ever on-screen at once. The bigger (later) gates earn a Pod.
        private void ConfigureWave()
        {
            var s = Stages[_stage];
            float peak = s.Peak * DifficultySettings.EnemyCountMult; // Hard = ×1; Easy/Medium pare it down

            float cFrac = Tuning.StartHardMode ? ConcurrentFractionHard : ConcurrentFractionEasy;
            _waveConcurrent = Mathf.Clamp(Mathf.CeilToInt(peak * cFrac), MinConcurrent, Tuning.MaxPursuers);

            float t = _gateCount <= 1 ? 1f : (float)_gate / (_gateCount - 1);
            float qFrac = Mathf.Lerp(QuotaMinFraction, QuotaMaxFraction, t);
            _waveQuota = Mathf.Max(_waveConcurrent, Mathf.RoundToInt(peak * qFrac));

            _waveSpawned = 0;
            _wavePodPlaced = false;
            // Pod on the back half of the lane once the stage is crowd-heavy enough.
            _wavePodDue = s.Peak >= 12 && _gate >= _gateCount - Mathf.Max(1, _gateCount / 2);
        }

        // ---- Camera bounds ------------------------------------------------------
        private void ApplyBounds(float maxX)
        {
            if (_rig == null) { ResolveRig(); if (_rig == null) return; }
            _rig.MinX = _minLeash;
            _rig.MaxX = Mathf.Max(_minLeash, maxX);
        }

        private void ResolveRig()
        {
            if (_rig != null) return;
            var cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            _rig = cam != null ? cam.GetComponent<CameraRig>() : FindAnyObjectByType<CameraRig>();
            if (_rig == null) Debug.LogWarning("[EnemySpawner] No CameraRig found — gate locking disabled.");
        }

        // ---- Field queries ------------------------------------------------------
        private static int CountLiveEnemies()
        {
            int n = 0;
            foreach (var a in Actor.All)
                if (a.Alive && a.Team == Team.Enemy) n++;
            return n;
        }

        // Pursuers for the on-screen cap: living enemies excluding Pods and their
        // swarmers (swarm is bonus, matching the original rule).
        private static int CountPursuers()
        {
            int n = 0;
            foreach (var a in Actor.All)
            {
                if (!a.Alive || a.Team != Team.Enemy) continue;
                if (a is Pod) continue;
                if (a is EnemyController e && e.Def != null && e.Def.Id == "swarmer") continue; // swarm is bonus
                n++;
            }
            return n;
        }

        private static int CountPods()
        {
            int n = 0;
            foreach (var a in Actor.All) if (a.Alive && a is Pod) n++;
            return n;
        }

        // ---- Spawn helpers (unchanged public surface; other code references them) --
        private static float EdgeX(PlayerController player) =>
            player.WorldX + (Random.value < 0.5f ? -1f : 1f) * (Tuning.ScreenWidthUnits * 0.5f + Random.Range(1f, 4f));

        public static EnemyController SpawnRegular(PlayerController player)
        {
            var go = NewEnemyGo("enemy_regular");
            var e = go.AddComponent<EnemyController>();
            e.WorldX = EdgeX(player);
            e.Z = Random.Range(0.5f, Tuning.ZBandDepth - 0.5f);
            e.Init(EnemyDef.RegularMelee());
            return e;
        }

        public static RangedEnemyController SpawnGunner(PlayerController player)
        {
            var go = NewEnemyGo("enemy_gunner");
            var e = go.AddComponent<RangedEnemyController>();
            e.WorldX = EdgeX(player);
            e.Z = Random.Range(0.5f, Tuning.ZBandDepth - 0.5f);
            e.Init(EnemyDef.Gunner());
            return e;
        }

        public static Pod PlacePod(float x, float z)
        {
            var go = NewEnemyGo("enemy_pod");
            var pod = go.AddComponent<Pod>();
            pod.Init(x, Mathf.Clamp(z, 0f, Tuning.ZBandDepth));
            return pod;
        }

        private static GameObject NewEnemyGo(string name)
        {
            var go = new GameObject(name);
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<SpriteAnimator>();
            return go;
        }
    }
}
