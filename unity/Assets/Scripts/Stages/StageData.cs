using System.Collections.Generic;

namespace ThisL
{
    /// <summary>
    /// Plain-data model for the LINEAR campaign (STAGES.md §2/§4.1: Lincoln → SF →
    /// Phil, 13 stages). A <see cref="StageData"/> = one authored level: its area
    /// name + backdrop theme (AREAS.md), its stage/ambient music, an ordered list
    /// of <see cref="Wave"/>s (the ENCOUNTERS.md spine + filler markers + boss),
    /// and an optional boss id. Contains no Unity types so the roster can be
    /// authored/tested without a scene; <see cref="StageDirector"/> turns it into
    /// live spawns. See ENCOUNTERS.md §0 for the spine-vs-filler / gate rules this
    /// mirrors.
    /// </summary>
    public sealed class StageData
    {
        public int Id;                       // 1..13 (campaign order)
        public string DisplayName;           // e.g. "Lincoln High + Suburb Streets"
        public string Area;                  // AREAS.md area, e.g. "Placer Suburbs & Mall"
        public string BackdropTheme;         // backdrop dir stem, e.g. "area1_suburb" (Backdrop.cs theme)
        public string MusicClip;             // Music.PlayStage stem (assets/audio/music/stage_loops/**)
        public string AmbientClip;           // Music.PlayAmbient stem (assets/audio/music/ambient/**)

        /// <summary>Combat lane length in world-units. Longer than the original 140 so the arenas
        /// have real WALKING distance between them (creator: JS-version feel, "reach an area, lock,
        /// then move on") and there's room to drop obstacles later. Finale overrides to ~30 (boss).</summary>
        public float LaneLengthWu = 200f;

        /// <summary>The ordered spine: vignette / spawn / checkpoint / filler-marker / boss waves.</summary>
        public readonly List<Wave> Waves = new();

        /// <summary>The stage's newest enemy type — filler weights 60% toward it (ENCOUNTERS.md §0).</summary>
        public EnemyArchetype NewestArchetype = EnemyArchetype.Regular;

        /// <summary>Boss id for the tail boss wave, or null for a bossless stage (Stages 1, 6, 12).
        /// The <see cref="StageDirector.SpawnBoss"/> hook (filled by the bosses agent) resolves it.</summary>
        public string BossId;

        /// <summary>Boss music cue stem (assets/audio/music/boss_cues/**), or null to keep the stage loop
        /// (Sandwich Bros has no dedicated cue).</summary>
        public string BossMusicClip;
    }

    /// <summary>What a <see cref="Wave"/> is. Vignette/Checkpoint auto-advance (no kill-gate);
    /// Spawn gates on a cleared field (ENCOUNTERS.md §0 named-target rule = empty field);
    /// Filler is a marker the director expands into N seeded sub-waves; Boss calls the hook.</summary>
    public enum WaveKind { Vignette, Spawn, Checkpoint, Filler, Boss }

    /// <summary>
    /// One entry in the campaign roster. `Regular/Swarmer/Zombie/Gunner` map to the
    /// real EnemyDef factories in the foundation; every other archetype is a
    /// FIRST-PASS placeholder built by <see cref="StageEnemyFactory"/> from the
    /// nearest real def until its bespoke enemy lands (ENEMIES.md roster).
    /// </summary>
    public enum EnemyArchetype
    {
        Regular, Swarmer, Zombie, Pod, Gunner,
        Snapper, HeadThrower, AntiAircraft,
        Sniper, FlyingMonkey, Monkey, MonkeyTamer, ArmRipper,
        Ninja, Pickpocket, Boomergunner, GatlingGunner, GroundSmasher, Heavy
    }

    /// <summary>Where a batch enters (ENCOUNTERS.md §0): L/R lane edges, B back-Z,
    /// A ambush (door/window/manhole), Air airborne swoop-in.</summary>
    public enum SpawnSide { L, R, B, A, Air }

    /// <summary>One (archetype, count, side) spawn in a wave's batch.</summary>
    public sealed class SpawnEntry
    {
        public EnemyArchetype Archetype;
        public int Count;
        public SpawnSide Side;

        public SpawnEntry(EnemyArchetype archetype, int count, SpawnSide side)
        {
            Archetype = archetype;
            Count = count;
            Side = side;
        }
    }

    /// <summary>
    /// One scripted beat. For <see cref="WaveKind.Spawn"/> it carries the batch +
    /// drip cadence and gates on a cleared field. For <see cref="WaveKind.Filler"/>
    /// it carries the pool + wave-count range the director expands per ENCOUNTERS.md
    /// §0 (weighted seeded draw, size ramp 4→6, count = midpoint rounded up). For
    /// <see cref="WaveKind.Boss"/> the <see cref="StageData.BossId"/> is used.
    /// </summary>
    public sealed class Wave
    {
        public WaveKind Kind;
        public string Label;                 // human label, e.g. "Wave 2", "CHECKPOINT (atrium)"
        public readonly List<SpawnEntry> Spawns = new();
        public float DripSeconds = 0.8f;     // one unit every N seconds so the 8-cap breathes

        // Filler-only: the range from the table; the director fills the midpoint (rounded up).
        public int FillerMinWaves;
        public int FillerMaxWaves;

        private Wave(WaveKind kind, string label) { Kind = kind; Label = label; }

        public static Wave Vignette(string label) => new(WaveKind.Vignette, label);
        public static Wave Checkpoint(string label) => new(WaveKind.Checkpoint, label);
        public static Wave Boss(string label) => new(WaveKind.Boss, label);

        public static Wave Filler(string label, int minWaves, int maxWaves)
            => new(WaveKind.Filler, label) { FillerMinWaves = minWaves, FillerMaxWaves = maxWaves };

        public static Wave Spawn(string label, float dripSeconds, params SpawnEntry[] spawns)
        {
            var w = new Wave(WaveKind.Spawn, label) { DripSeconds = dripSeconds };
            if (spawns != null) w.Spawns.AddRange(spawns);
            return w;
        }
    }

    /// <summary>
    /// Endless Mode descriptor (STAGES.md §7b): survival from base difficulty using
    /// the full roster, topping the field up whenever only <see cref="RefillThreshold"/>
    /// enemies remain and ramping wave size/tier as it goes. Data only — the
    /// director's <see cref="StageDirector.StartEndless"/> runs it.
    /// </summary>
    public sealed class EndlessDescriptor
    {
        public string MusicClip = "endless_layered";
        public string AmbientClip = "sf_city_crowd";
        public string BackdropTheme = "area1_suburb";
        public int RefillThreshold = 2;      // spawn more when only 2 remain (STAGES.md §7b)
        public int StartWaveSize = 3;
        public int MaxWaveSize = 8;          // clamped to the 8-pursuer cap (Tuning.MaxPursuers)
        public float RampEverySeconds = 30f; // grow wave size / tier band each interval
        public EnemyArchetype[] Pool;        // full roster (StageDatabase.Endless sets it)
    }
}
