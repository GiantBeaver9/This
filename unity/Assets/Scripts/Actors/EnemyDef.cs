namespace ThisL
{
    public enum StaggerWeight { L, M, H } // light / medium / heavy-floors

    /// <summary>
    /// Data for one enemy archetype (TUNING §4 roster). Plain data so the roster
    /// can grow without touching the AI. Speeds/reaches in wu, damage out of 100 HP.
    /// </summary>
    public sealed class EnemyDef
    {
        public string Id;
        public string SpriteDir;       // e.g. "sprites/enemies/enemy_regular"
        public string SpriteActor;     // e.g. "enemy_regular"
        public string Tier = "T1";
        public float Hp = 40f;
        public float Damage = 7.5f;    // per hit
        public float Speed = 6.5f;     // wu/s
        public float Reach = 1.0f;     // melee reach in wu
        public float WindupSeconds = 0.10f;   // telegraph before the hitbox
        public float AttackCooldown = 1.2f;
        public StaggerWeight Weight = StaggerWeight.L;
        public LootTier Loot = LootTier.T1;

        // Ranged behaviour (RangedEnemyController). IsRanged flips the AI to
        // hold-at-standoff + fire Z-aware shots instead of closing to melee.
        public bool IsRanged;
        public float HoldDistance = 5f;    // wu it keeps from the player
        public float FireRange = 10f;      // max X distance it will shoot from
        public float ProjectileSpeed = 12f;
        public float FireInterval = 0.5f;  // seconds between shots

        // ---- Roster (P0) --------------------------------------------------
        public static EnemyDef RegularMelee() => new()
        {
            Id = "regular_melee",
            SpriteDir = "sprites/enemies/enemy_regular",
            SpriteActor = "enemy_regular",
            Tier = "T1", Hp = 40f, Damage = 7.5f, Speed = 6.5f, Reach = 1.1f,
            WindupSeconds = 0.45f, AttackCooldown = 1.3f, Weight = StaggerWeight.L, // readable wind-up
            Loot = LootTier.T1,
        };

        public static EnemyDef Swarmer() => new()
        {
            Id = "swarmer",
            SpriteDir = "sprites/enemies/enemy_swarmer",
            SpriteActor = "enemy_swarmer",
            Tier = "T1b", Hp = 12f, Damage = 1.5f, Speed = 8.5f, Reach = 0.7f,
            WindupSeconds = 0.18f, AttackCooldown = 1.0f, Weight = StaggerWeight.L, // tiny lunge tell
            Loot = LootTier.None,
        };

        public static EnemyDef Gunner() => new()
        {
            // A short-range shooter (stands in for Arm-Ripper-class ranged, ENEMIES §4):
            // holds a standoff, lines up the player's Z-row, and fires straight shots.
            Id = "gunner",
            SpriteDir = "sprites/enemies/enemy_gunner",
            SpriteActor = "enemy_gunner",
            Tier = "T2", Hp = 55f, Damage = 7.5f, Speed = 5.0f, Reach = 1.0f,
            WindupSeconds = 0.40f, AttackCooldown = 0.7f, Weight = StaggerWeight.M, // aim tell
            Loot = LootTier.T2,
            IsRanged = true, HoldDistance = 5.5f, FireRange = 11f,
            ProjectileSpeed = 9f, FireInterval = 0.9f,   // slower, dodgeable shots
        };

        public static EnemyDef Zombie() => new()
        {
            Id = "zombie",
            SpriteDir = "sprites/enemies/enemy_zombie",
            SpriteActor = "enemy_zombie",
            Tier = "T0", Hp = 30f, Damage = 0f, Speed = 3.0f, Reach = 1.0f,
            WindupSeconds = 0.55f, AttackCooldown = 2.0f, Weight = StaggerWeight.M, // slow lurching grab
            Loot = LootTier.None,
        };
    }
}
