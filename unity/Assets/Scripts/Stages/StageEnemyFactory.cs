using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Turns an <see cref="EnemyArchetype"/> into a live actor the same way
    /// EnemySpawner does (GameObject + SpriteRenderer + SpriteAnimator + controller +
    /// <c>.Init(def)</c>). Each archetype is routed to the closest real controller so
    /// the exotic roster actually behaves (ENEMIES.md §2 / §6):
    /// <list type="bullet">
    /// <item>plain melee (Regular/Swarmer/Zombie/Monkey/FlyingMonkey/MonkeyTamer/
    ///   GroundSmasher/Heavy/Pickpocket) → <see cref="EnemyController"/>;</item>
    /// <item>Snapper (sword-zoner, calls in + snaps T1s) → <see cref="SnapperController"/>;</item>
    /// <item>Ninja (teleport + shuriken) → <see cref="NinjaController"/>;</item>
    /// <item>Anti-Aircraft &amp; Head-Thrower (arcing lobs) → <see cref="AntiAircraftController"/>;</item>
    /// <item>straight shooters (Gunner/Sniper/Arm-Ripper/Boomergunner/Gatling) →
    ///   <see cref="RangedEnemyController"/>.</item>
    /// </list>
    /// Stats come from the real <see cref="EnemyDef"/> / <see cref="EnemyRoster"/>
    /// factories where they exist; the few still-missing archetypes get a tuned
    /// clone (ENEMIES.md tiers). The six enemies with bespoke atlases now on the
    /// pipeline (enemy_heavy/snapper/ninja/sniper/armripper/pickpocket) point at
    /// their own art and fall back to the enemy_regular stick body until each atlas
    /// lands (see <see cref="ApplyArt"/>). Replace a clone by adding a real
    /// <c>EnemyDef.Xxx()</c> and routing it in <see cref="Resolve"/>.
    /// </summary>
    public static class StageEnemyFactory
    {
        /// <summary>Spawn one actor of the archetype at (worldX, z). Returns the Actor (Pod included).</summary>
        public static Actor Spawn(EnemyArchetype archetype, float worldX, float z)
        {
            if (archetype == EnemyArchetype.Pod)
                return EnemySpawner.PlacePod(worldX, z);

            var def = Resolve(archetype);
            var go = NewEnemyGo(def.Id);
            z = Mathf.Clamp(z, 0f, Tuning.ZBandDepth);

            Actor actor;
            switch (archetype)
            {
                // ---- Exotic bespoke controllers ----
                case EnemyArchetype.Snapper:
                {
                    var e = go.AddComponent<SnapperController>();
                    e.WorldX = worldX; e.Z = z; e.Init(def); actor = e; break;
                }
                case EnemyArchetype.Ninja:
                {
                    var e = go.AddComponent<NinjaController>();
                    e.WorldX = worldX; e.Z = z; e.Init(def); actor = e; break;
                }
                case EnemyArchetype.AntiAircraft:
                case EnemyArchetype.HeadThrower:
                {
                    // Both lob an ARCING projectile (rock / head-grenade) — the AA controller
                    // drives the overhead-throw telegraph + ArcProjectile drop.
                    var e = go.AddComponent<AntiAircraftController>();
                    e.WorldX = worldX; e.Z = z; e.Init(def); actor = e; break;
                }

                // ---- Straight shooters ----
                case EnemyArchetype.Gunner:
                case EnemyArchetype.Sniper:
                case EnemyArchetype.ArmRipper:
                case EnemyArchetype.Boomergunner:
                case EnemyArchetype.GatlingGunner:
                {
                    var e = go.AddComponent<RangedEnemyController>();
                    e.WorldX = worldX; e.Z = z; e.Init(def); actor = e; break;
                }

                // ---- Plain melee (default) ----
                default:
                {
                    var e = go.AddComponent<EnemyController>();
                    e.WorldX = worldX; e.Z = z; e.Init(def); actor = e; break;
                }
            }

            // Silhouette scaling (Actor.ScaleMult, TUNING §4: Heavy/Ground-Smasher read big).
            if (archetype == EnemyArchetype.Heavy) actor.ScaleMult = 1.5f;
            else if (archetype == EnemyArchetype.GroundSmasher) actor.ScaleMult = 1.3f;

            return actor;
        }

        private static GameObject NewEnemyGo(string name)
        {
            var go = new GameObject("enemy_" + name);
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<SpriteAnimator>();
            return go;
        }

        /// <summary>Archetype → EnemyDef. Real defs where they exist; tuned clones otherwise
        /// (ENEMIES.md tiers). Bespoke-atlas archetypes get their own art via <see cref="ApplyArt"/>.</summary>
        public static EnemyDef Resolve(EnemyArchetype a)
        {
            switch (a)
            {
                // ---- Real foundation / roster defs ----
                case EnemyArchetype.Regular: return EnemyDef.RegularMelee();
                case EnemyArchetype.Swarmer: return EnemyDef.Swarmer();
                case EnemyArchetype.Zombie:  return EnemyDef.Zombie();
                case EnemyArchetype.Gunner:  return EnemyDef.Gunner();

                case EnemyArchetype.Snapper:      return ApplyArt(EnemyRoster.Snapper(),     "enemy_snapper");
                case EnemyArchetype.Heavy:        return ApplyArt(EnemyRoster.Heavy(),       "enemy_heavy");
                case EnemyArchetype.Pickpocket:   return ApplyArt(EnemyRoster.Pickpocket(),  "enemy_pickpocket");
                case EnemyArchetype.ArmRipper:    return ApplyArt(EnemyRoster.ArmRipper(),   "enemy_armripper");
                case EnemyArchetype.Ninja:        return ApplyArt(EnemyRoster.Ninja(),       "enemy_ninja");
                case EnemyArchetype.HeadThrower:  return ApplyArt(EnemyRoster.HeadThrower(),   "enemy_headthrower");
                case EnemyArchetype.AntiAircraft: return ApplyArt(EnemyRoster.AntiAircraft(),  "enemy_antiaircraft");
                case EnemyArchetype.Monkey:       return ApplyArt(EnemyRoster.EconomyMonkey(), "enemy_monkey");

                // ---- Tuned clones (no bespoke def yet; ENEMIES.md tiers). Each is routed
                // through ApplyArt so its bespoke stick atlas lights up automatically the
                // moment it lands on disk (graceful fall back to enemy_regular until then). ----
                case EnemyArchetype.Sniper:       return ApplyArt(
                    Ranged("sniper", "T3", hp: 30f, dmg: 20f, fireEvery: 5.0f, fireRange: 22f, hold: 12f, loot: LootTier.T3),
                    "enemy_sniper");
                case EnemyArchetype.FlyingMonkey: return ApplyArt(
                    Melee("flying_monkey", "T2", hp: 30f, dmg: 6f, speed: 8.0f, weight: StaggerWeight.L, loot: LootTier.None),
                    "enemy_flyingmonkey");
                case EnemyArchetype.MonkeyTamer:  return ApplyArt(
                    Melee("monkey_tamer", "T2", hp: 50f, dmg: 7f, speed: 5.5f, weight: StaggerWeight.M, loot: LootTier.T2),
                    "enemy_monkeytamer");
                case EnemyArchetype.Boomergunner:  return ApplyArt(
                    Ranged("boomergunner", "T2", hp: 40f, dmg: 8f, fireEvery: 2.5f, fireRange: 12f, loot: LootTier.T2),
                    "enemy_boomergunner");
                case EnemyArchetype.GatlingGunner: return ApplyArt(
                    Ranged("gatling_gunner", "T3", hp: 45f, dmg: 3f, fireEvery: 0.25f, fireRange: 16f, hold: 8f, loot: LootTier.T3, weight: StaggerWeight.H),
                    "enemy_gatling");
                case EnemyArchetype.GroundSmasher: return ApplyArt(
                    Melee("ground_smasher", "T3", hp: 55f, dmg: 12f, speed: 5.0f, weight: StaggerWeight.H, loot: LootTier.T3),
                    "enemy_groundsmasher");

                default: return EnemyDef.RegularMelee();
            }
        }

        /// <summary>Point a def at its bespoke enemy atlas if present; otherwise leave it on the
        /// enemy_regular stick body (SpriteLibrary would else fall through to a magenta placeholder).</summary>
        private static EnemyDef ApplyArt(EnemyDef def, string atlasActor)
        {
            string dir = "sprites/enemies/" + atlasActor;
            if (SpriteLibrary.HasAtlas(dir, atlasActor))
            {
                def.SpriteDir = dir;
                def.SpriteActor = atlasActor;
            }
            else
            {
                def.SpriteDir = "sprites/enemies/enemy_regular";
                def.SpriteActor = "enemy_regular";
            }
            return def;
        }

        // A Regular-melee clone with tuned stats and a distinct id; sprite falls back to enemy_regular.
        private static EnemyDef Melee(string id, string tier, float hp, float dmg, float speed,
                                      StaggerWeight weight, LootTier loot)
        {
            return new EnemyDef
            {
                Id = id,
                SpriteDir = "sprites/enemies/enemy_regular",
                SpriteActor = "enemy_regular",
                Tier = tier, Hp = hp, Damage = dmg, Speed = speed, Reach = 1.0f,
                WindupSeconds = 0.12f, AttackCooldown = 1.1f, Weight = weight, Loot = loot,
            };
        }

        // A Gunner clone (RangedEnemyController) with tuned stats and a distinct id.
        private static EnemyDef Ranged(string id, string tier, float hp, float dmg, float fireEvery,
                                       float fireRange, float hold = 5f, LootTier loot = LootTier.T2,
                                       StaggerWeight weight = StaggerWeight.M)
        {
            return new EnemyDef
            {
                Id = id,
                SpriteDir = "sprites/enemies/enemy_regular",
                SpriteActor = "enemy_regular",
                Tier = tier, Hp = hp, Damage = dmg, Speed = 5.0f, Reach = 1.0f,
                WindupSeconds = 0.15f, AttackCooldown = 0.5f, Weight = weight, Loot = loot,
                IsRanged = true, HoldDistance = hold, FireRange = fireRange,
                ProjectileSpeed = 12f, FireInterval = fireEvery,
            };
        }
    }
}
