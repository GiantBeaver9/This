using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Static boss registry (BOSSES.md §5, StageDatabase boss ids). The single hook the
    /// <see cref="StageDirector.SpawnBoss"/> body calls:
    /// <c>return Bosses.Spawn(bossId, x, z);</c> — it instantiates the right
    /// <see cref="BossController"/> (GameObject + SpriteRenderer + SpriteAnimator +
    /// controller + Init) at the arena position and returns its <see cref="Actor"/>
    /// (which the director watches for <c>!Alive</c>). Unknown ids log an error and
    /// return null (the director then treats the gate as cleared, so the campaign still
    /// chains). See <c>_INTEGRATION.md</c> for the id→boss→stage map and art/mech gaps.
    /// </summary>
    public static class Bosses
    {
        /// <summary>Spawn the boss for <paramref name="bossId"/> at (x, z). Null (logged) for unknown ids.</summary>
        public static Actor Spawn(string bossId, float x, float z)
        {
            switch (bossId)
            {
                // ---- The 5 pure HP-depletion bosses (executable at ≤10%) ----
                case "burly":         { var b = New<BurlyBoss>("boss_burly");                b.Init(x, z); return b; }
                case "gatlinggunguy": { var b = New<GatlingGunGuyBoss>("boss_gatlinggunguy"); b.Init(x, z); return b; }
                case "sandwich_bros": { var b = New<BigVersionBoss>("boss_sandwich_bros");    b.Init(BigVersionBoss.SandwichBros, x, z); return b; }
                case "road_bruiser":  { var b = New<BigVersionBoss>("boss_road_bruiser");     b.Init(BigVersionBoss.RoadBruiser, x, z); return b; }
                case "big_armripper": { var b = New<BigVersionBoss>("boss_big_armripper");    b.Init(BigVersionBoss.BigArmRipper, x, z); return b; }
                case "boomergunner":  { var b = New<BigVersionBoss>("boss_boomergunner");     b.Init(BigVersionBoss.Boomergunner, x, z); return b; }

                // ---- The 5 objective/proxy/scripted bosses (NO execute) ----
                case "tank":          { var b = New<TankBoss>("boss_tank");            b.Init(x, z); return b; }
                case "colossus":      { var b = New<ColossusBoss>("boss_colossus");    b.Init(x, z); return b; }
                case "helicopter":    { var b = New<HelicopterBoss>("boss_helicopter"); b.Init(x, z); return b; }
                case "monkey_boss":   { var b = New<MonkeyBoss>("boss_monkey_boss");   b.Init(x, z); return b; }
                case "phil":          { var b = New<PhilBoss>("boss_phil");            b.Init(x, z); return b; }

                default:
                    Debug.LogError($"[Bosses] Unknown boss id \"{bossId}\" — no boss spawned (gate treated as cleared).");
                    return null;
            }
        }

        /// <summary>Build the GameObject the EnemySpawner/StageEnemyFactory way and attach the controller.</summary>
        private static T New<T>(string name) where T : BossController
        {
            var go = new GameObject(name);
            go.AddComponent<SpriteRenderer>();   // Actor/SpriteAnimator RequireComponent
            go.AddComponent<SpriteAnimator>();
            return go.AddComponent<T>();
        }
    }
}
