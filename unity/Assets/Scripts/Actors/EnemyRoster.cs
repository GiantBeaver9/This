namespace ThisL
{
    /// <summary>
    /// Additional enemy archetype factories (ENEMIES.md §2 / §6, TUNING §4 stat
    /// table) beyond the P0 set on <see cref="EnemyDef"/>. Plain-data only — each
    /// factory returns the authoritative HP / damage / speed / weight / timing
    /// numbers and the fields the AI reads. Behaviour comes from a controller:
    /// the melee ones map to <see cref="EnemyController"/>, the straight-shooters
    /// to <see cref="RangedEnemyController"/>, and the three exotic ones to the
    /// bespoke controllers added alongside this file
    /// (<see cref="AntiAircraftController"/>, <see cref="SnapperController"/>,
    /// <see cref="NinjaController"/>). See <c>_INTEGRATION.md</c> for the mapping.
    ///
    /// Every archetype here reuses the <c>enemy_regular</c> atlas (the only
    /// stick-body art on disk); bespoke atlases are an art gap listed in the
    /// integration doc. A controller may tint <see cref="Actor.Sr"/> to
    /// distinguish them at a glance.
    /// </summary>
    public static class EnemyRoster
    {
        // ---- Melee (map to EnemyController) ---------------------------------

        /// <summary>Snapper / Sword-Maker — TUNING §4 row 6 (T2, HP 70, sword 15,
        /// reach 1.7 wu, windup 175 ms). Melee-zoner; the call-in-a-T1 refresh
        /// loop lives in <see cref="SnapperController"/>.</summary>
        public static EnemyDef Snapper() => new()
        {
            Id = "snapper",
            SpriteDir = "sprites/enemies/enemy_regular",
            SpriteActor = "enemy_regular",
            Tier = "T2", Hp = 70f, Damage = 15f, Speed = 6.0f, Reach = 1.7f,
            WindupSeconds = 0.175f, AttackCooldown = 1.5f, Weight = StaggerWeight.M,
            Loot = LootTier.T2,
        };

        /// <summary>Heavy ("Bold"/Burly) — TUNING §4 row 17 (untiered bruiser,
        /// HP 220, extended-reach punch 22.5 @ 1.8 wu, windup 250 ms, H-floors).
        /// Maps to <see cref="EnemyController"/>; the H-weight super-armor /
        /// floor-the-dash rule is a player-side interaction (art/AI gap noted).</summary>
        public static EnemyDef Heavy() => new()
        {
            Id = "heavy",
            SpriteDir = "sprites/enemies/enemy_regular",
            SpriteActor = "enemy_regular",
            Tier = "untiered", Hp = 220f, Damage = 22.5f, Speed = 5.0f, Reach = 1.8f,
            WindupSeconds = 0.25f, AttackCooldown = 1.6f, Weight = StaggerWeight.H,
            Loot = LootTier.T3,
        };

        /// <summary>Pickpocket — TUNING §4 row 13 (untiered economy, HP 25,
        /// bump 5, fastest at 9.0 wu/s, L). Maps to <see cref="EnemyController"/>
        /// as a fast light poke; the steal-and-flee economy loop is a gap.</summary>
        public static EnemyDef Pickpocket() => new()
        {
            Id = "pickpocket",
            SpriteDir = "sprites/enemies/enemy_regular",
            SpriteActor = "enemy_regular",
            Tier = "untiered", Hp = 25f, Damage = 5f, Speed = 9.0f, Reach = 0.9f,
            WindupSeconds = 0.10f, AttackCooldown = 1.2f, Weight = StaggerWeight.L,
            Loot = LootTier.None,
        };

        /// <summary>Monkey (economy) — TUNING §4 row 10 (untiered, HP 30, flail 5,
        /// 6.0 wu/s, L). Maps to <see cref="EnemyController"/>; the flee-at-&lt;50%
        /// and Monkey-Merc drop are gaps.</summary>
        public static EnemyDef EconomyMonkey() => new()
        {
            Id = "economy_monkey",
            SpriteDir = "sprites/enemies/enemy_regular",
            SpriteActor = "enemy_regular",
            Tier = "untiered", Hp = 30f, Damage = 5f, Speed = 6.0f, Reach = 1.0f,
            WindupSeconds = 0.30f, AttackCooldown = 1.5f, Weight = StaggerWeight.L,
            Loot = LootTier.None,
        };

        // ---- Straight shooters (map to RangedEnemyController) ---------------

        /// <summary>Arm-Ripper — TUNING §4 row 11 (T2a, HP 70, 2 pistols
        /// 7.5/shot @ 2 shots/s, holds ≤4 wu). Maps to
        /// <see cref="RangedEnemyController"/>. Reload-after-6 and the
        /// disarmed-T1 headbutt state are gaps.</summary>
        public static EnemyDef ArmRipper() => new()
        {
            Id = "arm_ripper",
            SpriteDir = "sprites/enemies/enemy_regular",
            SpriteActor = "enemy_regular",
            Tier = "T2a", Hp = 70f, Damage = 7.5f, Speed = 6.0f, Reach = 1.0f,
            WindupSeconds = 0.10f, AttackCooldown = 0.5f, Weight = StaggerWeight.M,
            Loot = LootTier.T2,
            IsRanged = true, HoldDistance = 4f, FireRange = 4.5f,
            ProjectileSpeed = 12f, FireInterval = 0.5f,
        };

        /// <summary>Head-Thrower — TUNING §4 row 5 (T2-eff, HP 45, head-grenade
        /// 15, holds 7 wu, throw every 3.0 s). Maps to
        /// <see cref="RangedEnemyController"/> as a straight lob stand-in; for a
        /// real arc drive it with <see cref="AntiAircraftController"/> instead.
        /// The fire→2 s→BOOM walking-bomb state and head-regrow are gaps.</summary>
        public static EnemyDef HeadThrower() => new()
        {
            Id = "head_thrower",
            SpriteDir = "sprites/enemies/enemy_regular",
            SpriteActor = "enemy_regular",
            Tier = "T2-eff", Hp = 45f, Damage = 15f, Speed = 5.5f, Reach = 1.0f,
            WindupSeconds = 0.45f, AttackCooldown = 3.0f, Weight = StaggerWeight.M,  // longer telegraph: rip head off
            Loot = LootTier.T2,
            IsRanged = true, HoldDistance = 7f, FireRange = 8f,
            ProjectileSpeed = 9f, FireInterval = 3.0f,
        };

        // ---- Exotic (map to bespoke controllers in this pack) ---------------

        /// <summary>Anti-Aircraft — TUNING §4 row 4 (T1a, HP 40, rock 7.5, holds
        /// 8 wu, throw every 2.5 s, arc telegraph 0.5 s). Rocks ARC, so drive
        /// this with <see cref="AntiAircraftController"/> (+ <see cref="ArcProjectile"/>);
        /// it also works on <see cref="RangedEnemyController"/> as a straight-shot
        /// fallback. Boomerang-bait counterplay is a gap.</summary>
        public static EnemyDef AntiAircraft() => new()
        {
            Id = "anti_aircraft",
            SpriteDir = "sprites/enemies/enemy_regular",
            SpriteActor = "enemy_regular",
            Tier = "T1a", Hp = 40f, Damage = 7.5f, Speed = 5.0f, Reach = 1.0f,
            WindupSeconds = 0.5f, AttackCooldown = 2.5f, Weight = StaggerWeight.M,
            Loot = LootTier.T1,
            IsRanged = true, HoldDistance = 8f, FireRange = 10f,
            ProjectileSpeed = 9f, FireInterval = 2.5f,
        };

        /// <summary>Ninja — TUNING §4 row 12 (T3a, HP 100, melee 22.5, shuriken 12,
        /// teleport cd 3 s / smoke tell 0.3 s, 2 shuriken/volley on 3 s). Drive
        /// with <see cref="NinjaController"/>. FireRange/ProjectileSpeed carry the
        /// 12 wu straight shuriken; melee damage is <see cref="EnemyDef.Damage"/>.</summary>
        public static EnemyDef Ninja() => new()
        {
            Id = "ninja",
            SpriteDir = "sprites/enemies/enemy_regular",
            SpriteActor = "enemy_regular",
            Tier = "T3a", Hp = 100f, Damage = 22.5f, Speed = 7.0f, Reach = 1.2f,
            WindupSeconds = 0.2f, AttackCooldown = 1.5f, Weight = StaggerWeight.L,
            Loot = LootTier.T3,
            IsRanged = true, HoldDistance = 4f, FireRange = 12f,
            ProjectileSpeed = 16f, FireInterval = 3.0f,
        };
    }
}
