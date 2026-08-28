namespace ThisL
{
    public enum Difficulty { Easy, Medium, Hard }

    /// <summary>
    /// Global difficulty. The current tuning IS Hard (creator ruling: "start hard,
    /// pare down") — Hard applies no scaling, Medium and Easy scale the enemy
    /// pressure DOWN from that baseline (fewer enemies, less damage, less HP). Read
    /// by the spawner (count) and enemy controllers (damage dealt + HP).
    /// </summary>
    public static class DifficultySettings
    {
        public static Difficulty Current = Difficulty.Hard;

        /// <summary>Time-based ramp for Endless: the chosen difficulty is the CEILING, and the run
        /// starts gentler and climbs toward (then past) it, so Endless isn't "instant hard". Scales
        /// enemy count + damage on top of the difficulty base. Stays 1 in the campaign. Driven by
        /// <see cref="StageDirector"/> while endless is live; reset to 1 on every world build.</summary>
        public static float EndlessPressure = 1f;

        /// <summary>Multiplier on how many enemies are present (× the Endless ramp).</summary>
        public static float EnemyCountMult => BaseCountMult * EndlessPressure;

        private static float BaseCountMult => Current switch
        {
            Difficulty.Easy => 0.55f,
            Difficulty.Medium => 0.8f,
            _ => 1f,
        };

        /// <summary>Multiplier on damage enemies deal to the player (× the Endless ramp).</summary>
        public static float EnemyDamageMult => BaseDamageMult * EndlessPressure;

        private static float BaseDamageMult => Current switch
        {
            Difficulty.Easy => 0.5f,
            Difficulty.Medium => 0.75f,
            _ => 1f,
        };

        /// <summary>Multiplier on enemy HP (lower = they die faster).</summary>
        public static float EnemyHpMult => Current switch
        {
            Difficulty.Easy => 0.7f,
            Difficulty.Medium => 0.85f,
            _ => 1f,
        };

        public static string Label => Current switch
        {
            Difficulty.Easy => "EASY",
            Difficulty.Medium => "MEDIUM",
            _ => "HARD",
        };

        /// <summary>Scale a count by difficulty, never below a floor.</summary>
        public static int ScaleCount(int n, int floor = 1) =>
            UnityEngine.Mathf.Max(floor, UnityEngine.Mathf.RoundToInt(n * EnemyCountMult));
    }
}
