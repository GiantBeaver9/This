using System.Collections.Generic;

namespace ThisL
{
    /// <summary>
    /// The authored LINEAR campaign — 13 stages (12 combat + finale) from
    /// STAGES.md §4.1 and the per-stage wave tables in ENCOUNTERS.md, with areas /
    /// backdrop themes / music from AREAS.md and the audio index. Each stage's
    /// spine (vignette → scripted waves → checkpoints → funnel → boss) is
    /// transcribed here; filler blocks are markers the director expands per
    /// ENCOUNTERS.md §0 (seeded, count = midpoint of the listed range). Enemy
    /// archetypes without a real EnemyDef yet resolve to FIRST-PASS placeholders
    /// (see StageEnemyFactory). Also holds the Endless descriptor (STAGES.md §7b).
    /// </summary>
    public static class StageDatabase
    {
        public const int StageCount = 13;

        private static List<StageData> _stages;

        /// <summary>The ordered campaign. Index 0 = Stage 1 (Lincoln High).</summary>
        public static IReadOnlyList<StageData> Stages => _stages ??= Build();

        public static StageData Get(int index)
        {
            var list = Stages;
            if (index < 0 || index >= list.Count) return null;
            return list[index];
        }

        // Shorthand for readable spawn tables.
        private static SpawnEntry E(EnemyArchetype a, int n, SpawnSide s) => new(a, n, s);

        private static List<StageData> Build()
        {
            var s = new List<StageData>(StageCount);

            // =========================== ACT 1 — Placer Suburbs & Mall ===========================

            // --- Stage 1 — THE NEIGHBORHOOD: houses → school → courts → diner → railroad → Sandwich Bros.
            // Culminates at the Sandwich Bros store, where you fight the BIG-CHARGER boss (a big version of
            // the neighborhood Regulars with a charge attack). Creator: "the only thing that ends levels is
            // defeating the boss… sandwich bros is the end of stage zero of the neighborhood." ---
            {
                var st = new StageData
                {
                    Id = 1,
                    DisplayName = "The Neighborhood → Sandwich Bros",
                    Area = "Placer Suburbs & Mall",
                    BackdropTheme = "area1_suburb",
                    MusicClip = "a1_surfrock_opener",
                    AmbientClip = "lincoln_birds_traffic",
                    NewestArchetype = EnemyArchetype.Regular,
                    BossId = "sandwich_bros",  // big version of the Regulars + charge (BigVersionBoss.SandwichBros)
                    BossMusicClip = null,
                    BossAtLaneEnd = true,      // fight at the store: clear waves → walk over the railroad → boss
                };
                st.Waves.Add(Wave.Vignette("Vignette: dancing Zebra punches a Regular"));
                st.Waves.Add(Wave.Spawn("Wave 1", 1.0f, E(EnemyArchetype.Regular, 2, SpawnSide.L)));
                st.Waves.Add(Wave.Spawn("Wave 2", 0.9f, E(EnemyArchetype.Regular, 2, SpawnSide.L), E(EnemyArchetype.Regular, 1, SpawnSide.R)));
                st.Waves.Add(Wave.Checkpoint("CHECKPOINT (mid)"));
                st.Waves.Add(Wave.Filler("Filler (Regulars)", 10, 14));
                st.Waves.Add(Wave.Spawn("Wave 3 (funnel, bus pass)", 0.8f, E(EnemyArchetype.Regular, 5, SpawnSide.B)));
                // Walk into the Sandwich Bros lot: a SMALL POD first, THEN the boss barrels out on arrival
                // (creator: "there should be a small pod, then he spawns"). Boss only spawns once you walk in.
                st.Waves.Add(Wave.Spawn("Sandwich Bros lot — pod", 0.7f, E(EnemyArchetype.Regular, 3, SpawnSide.L)));
                st.Waves.Add(Wave.Boss("BOSS: Sandwich Bros (big charger)"));
                s.Add(st);
            }

            // --- Stage 2 — THE MALL (Roseville Galleria). Creator: "Lv 2 is just Lv 1 again — needs to be
            // in a mall." Distinct storefront backdrop (area1_mall) vs the suburb; mall-security guards
            // shove through (HazardDirector stage 1 = Guard); ends on the Road Bruiser (big Regular +
            // charge) so it closes on a boss. ---
            {
                var st = new StageData
                {
                    Id = 2,
                    DisplayName = "Roseville Galleria (Mall)",
                    Area = "Placer Suburbs & Mall",
                    BackdropTheme = "area1_mall",
                    Stashed = true,            // creator: "stash L2, point L1 to L3" — skipped by the campaign
                    MusicClip = "a1_surfrock_opener",
                    AmbientClip = "lincoln_birds_traffic",
                    NewestArchetype = EnemyArchetype.Regular,
                    BossId = "road_bruiser",   // big Regular + charge (BigVersionBoss.RoadBruiser)
                    BossMusicClip = null,
                };
                st.Waves.Add(Wave.Spawn("Wave 1", 0.9f, E(EnemyArchetype.Regular, 3, SpawnSide.L)));
                st.Waves.Add(Wave.Checkpoint("CHECKPOINT (mid)"));
                st.Waves.Add(Wave.Filler("Filler (Regulars)", 10, 12));
                st.Waves.Add(Wave.Spawn("Wave 2 (funnel to the mall lot)", 0.7f,
                    E(EnemyArchetype.Regular, 5, SpawnSide.L), E(EnemyArchetype.Regular, 4, SpawnSide.R)));
                st.Waves.Add(Wave.Boss("BOSS: Road Bruiser (big charger)"));
                s.Add(st);
            }

            // --- Stage 3 — Rocklin → Galleria mall + Zombie/Swarmer debut → BOSS: Burly. ENCOUNTERS.md Stage 3. ---
            {
                var st = new StageData
                {
                    Id = 3,
                    DisplayName = "Roseville Galleria → Burly Macho Guy",
                    Area = "Placer Suburbs & Mall",
                    BackdropTheme = "area1_mall", // FIRST-PASS: mall theme (Backdrop.cs ships Area-1 suburb only)
                    MusicClip = "a1_synthpunk_mall",
                    AmbientClip = "galleria_murmur",
                    NewestArchetype = EnemyArchetype.Swarmer,
                    BossId = "burly",
                    BossMusicClip = "burly",
                };
                st.Waves.Add(Wave.Vignette("Vignette: guard shoots T1 → zombifies → grab → fall"));
                st.Waves.Add(Wave.Spawn("Wave 1", 0.9f, E(EnemyArchetype.Regular, 3, SpawnSide.L)));
                st.Waves.Add(Wave.Spawn("Wave 2 (Regulars)", 0.8f, E(EnemyArchetype.Regular, 3, SpawnSide.L))); // pods now come from PodDirector (position-triggered), not the wave flood
                st.Waves.Add(Wave.Checkpoint("CHECKPOINT (atrium)"));
                st.Waves.Add(Wave.Spawn("Wave 3 (first Zombie)", 0.8f, E(EnemyArchetype.Zombie, 1, SpawnSide.A), E(EnemyArchetype.Regular, 3, SpawnSide.L)));
                st.Waves.Add(Wave.Filler("Filler (Regular/Swarmer-pod/Zombie)", 12, 16));
                st.Waves.Add(Wave.Spawn("Wave 4 (funnel to dept store)", 0.7f, E(EnemyArchetype.Regular, 4, SpawnSide.L), E(EnemyArchetype.Zombie, 1, SpawnSide.A)));
                st.Waves.Add(Wave.Boss("BOSS: Burly Macho Guy"));
                s.Add(st);
            }

            // =========================== ACT 2 — Sacramento & Airport ===========================

            // --- Stage 4 — Sacramento Victorian + Snapper debut → BOSS: Colossus. ENCOUNTERS.md Stage 4. ---
            {
                var st = new StageData
                {
                    Id = 4,
                    DisplayName = "Sacramento Old-Town → The Colossus",
                    Area = "Sacramento & Airport",
                    BackdropTheme = "area2_sacramento",
                    MusicClip = "a2_ragtime_garagerock",
                    AmbientClip = "sacramento_oldtown",
                    NewestArchetype = EnemyArchetype.Snapper,
                    BossId = "colossus",
                    BossMusicClip = "colossus",
                };
                st.Waves.Add(Wave.Vignette("Vignette: whip-pull demo (teaches the Whip)"));
                st.Waves.Add(Wave.Spawn("Wave 1", 0.9f, E(EnemyArchetype.Regular, 3, SpawnSide.L)));
                st.Waves.Add(Wave.Spawn("Wave 2 (Snapper debut)", 0.8f, E(EnemyArchetype.Snapper, 1, SpawnSide.B), E(EnemyArchetype.Regular, 2, SpawnSide.L)));
                st.Waves.Add(Wave.Checkpoint("CHECKPOINT (streetcar stop)"));
                st.Waves.Add(Wave.Filler("Filler (Regular/Snapper)", 12, 14));
                st.Waves.Add(Wave.Spawn("Wave 3 (funnel)", 0.7f, E(EnemyArchetype.Regular, 4, SpawnSide.L), E(EnemyArchetype.Snapper, 1, SpawnSide.A)));
                st.Waves.Add(Wave.Boss("BOSS: The Colossus"));
                s.Add(st);
            }

            // --- Stage 5 — Airport + AA/Head-Thrower debut → BOSS: Helicopter. ENCOUNTERS.md Stage 5. ---
            {
                var st = new StageData
                {
                    Id = 5,
                    DisplayName = "Sacramento Airport → Helicopter",
                    Area = "Sacramento & Airport",
                    BackdropTheme = "area2_airport",
                    MusicClip = "a2_industrial_electronic",
                    AmbientClip = "airport_tarmac_jet",
                    NewestArchetype = EnemyArchetype.HeadThrower,
                    BossId = "helicopter",
                    BossMusicClip = "helicopter",
                };
                st.Waves.Add(Wave.Vignette("Vignette: head-grenade + bat-a-plane"));
                st.Waves.Add(Wave.Spawn("Wave 1 (AA debut)", 0.8f, E(EnemyArchetype.AntiAircraft, 3, SpawnSide.B), E(EnemyArchetype.Regular, 2, SpawnSide.L)));
                st.Waves.Add(Wave.Spawn("Wave 2 (Head-Thrower debut)", 0.8f, E(EnemyArchetype.HeadThrower, 2, SpawnSide.B), E(EnemyArchetype.Snapper, 1, SpawnSide.L)));
                st.Waves.Add(Wave.Checkpoint("CHECKPOINT (gate lounge)"));
                st.Waves.Add(Wave.Filler("Filler (Regular/AA/Head-Thrower/Snapper)", 12, 16));
                st.Waves.Add(Wave.Spawn("Wave 3 (tarmac funnel)", 0.7f, E(EnemyArchetype.Regular, 4, SpawnSide.L), E(EnemyArchetype.AntiAircraft, 2, SpawnSide.B)));
                st.Waves.Add(Wave.Boss("BOSS: Helicopter (Monkey Chopper)"));
                s.Add(st);
            }

            // =========================== ACT 3 — Hills, Causeway & Dixon ===========================

            // --- Stage 6 — Hills + causeway + Sniper/Flying Monkey debut. No boss. ENCOUNTERS.md Stage 6. ---
            {
                var st = new StageData
                {
                    Id = 6,
                    DisplayName = "Rolling Hills + Yolo Causeway",
                    Area = "Hills, Causeway & Dixon",
                    BackdropTheme = "area3_causeway",
                    MusicClip = "a3_hoedown_bluegrass",
                    AmbientClip = "causeway_marsh_wind",
                    NewestArchetype = EnemyArchetype.Sniper,
                };
                st.Waves.Add(Wave.Vignette("Vignette: sniper apex-punish + dime→whistle→monkey"));
                st.Waves.Add(Wave.Spawn("Wave 1 (perched Sniper)", 0.8f, E(EnemyArchetype.Sniper, 1, SpawnSide.B), E(EnemyArchetype.Regular, 3, SpawnSide.L)));
                st.Waves.Add(Wave.Spawn("Wave 2 (platforms)", 0.9f, E(EnemyArchetype.FlyingMonkey, 2, SpawnSide.Air), E(EnemyArchetype.Regular, 3, SpawnSide.L), E(EnemyArchetype.Monkey, 1, SpawnSide.A)));
                st.Waves.Add(Wave.Checkpoint("CHECKPOINT (mid-causeway)"));
                st.Waves.Add(Wave.Filler("Filler (Regular/Flying Monkey/AA/Sniper)", 10, 14));
                st.Waves.Add(Wave.Spawn("Wave 3 (funnel to farm)", 0.7f, E(EnemyArchetype.Regular, 5, SpawnSide.L), E(EnemyArchetype.Monkey, 1, SpawnSide.A)));
                s.Add(st);
            }

            // --- Stage 7 — Farm/Ranch + Monkey Tamer → BOSS: Monkey Boss. ENCOUNTERS.md Stage 7. ---
            {
                var st = new StageData
                {
                    Id = 7,
                    DisplayName = "Farm / Ranch → Monkey Boss",
                    Area = "Hills, Causeway & Dixon",
                    BackdropTheme = "area3_farm",
                    MusicClip = "a3_spaghetti_western",
                    AmbientClip = "farm_barnyard",
                    NewestArchetype = EnemyArchetype.MonkeyTamer,
                    BossId = "monkey_boss",
                    BossMusicClip = "monkeyboss",
                };
                st.Waves.Add(Wave.Vignette("Vignette: dime → Monkey Merc shoots the boss"));
                st.Waves.Add(Wave.Spawn("Wave 1 (Monkey Tamer)", 0.8f, E(EnemyArchetype.MonkeyTamer, 1, SpawnSide.B), E(EnemyArchetype.Regular, 2, SpawnSide.L)));
                st.Waves.Add(Wave.Spawn("Wave 2", 0.9f, E(EnemyArchetype.FlyingMonkey, 2, SpawnSide.Air), E(EnemyArchetype.Regular, 3, SpawnSide.L)));
                st.Waves.Add(Wave.Checkpoint("CHECKPOINT (barn)"));
                st.Waves.Add(Wave.Filler("Filler (Regular/Flying Monkey/Tamer/Monkey)", 10, 14));
                st.Waves.Add(Wave.Spawn("Wave 3 (funnel)", 0.7f, E(EnemyArchetype.Regular, 4, SpawnSide.L), E(EnemyArchetype.Monkey, 1, SpawnSide.A)));
                st.Waves.Add(Wave.Boss("BOSS: Monkey Boss"));
                s.Add(st);
            }

            // --- Stage 8 — Dixon boss rush + Arm-Ripper debut → BOSS: big Arm-Ripper. ENCOUNTERS.md Stage 8. ---
            {
                var st = new StageData
                {
                    Id = 8,
                    DisplayName = "Dixon Boss Rush → big Arm-Ripper",
                    Area = "Hills, Causeway & Dixon",
                    BackdropTheme = "area3_dixon",
                    MusicClip = "a3_western_dread",
                    AmbientClip = "dixon_town_wind",
                    NewestArchetype = EnemyArchetype.ArmRipper,
                    BossId = "big_armripper",
                    BossMusicClip = "big_armripper",
                };
                st.Waves.Add(Wave.Vignette("Vignette: arm-rip → akimbo fire"));
                st.Waves.Add(Wave.Spawn("Wave 1 (Arm-Ripper debut)", 0.8f, E(EnemyArchetype.ArmRipper, 2, SpawnSide.B), E(EnemyArchetype.Regular, 2, SpawnSide.L)));
                // FIRST-PASS: the 4 boss-rush minibosses are big-version reprises; until the bosses agent
                // provides real "big" scaling, they spawn as their base archetypes with adds.
                st.Waves.Add(Wave.Spawn("Miniboss 1: big Snapper", 0.8f, E(EnemyArchetype.Snapper, 1, SpawnSide.B), E(EnemyArchetype.Regular, 2, SpawnSide.L))); // FIRST-PASS
                st.Waves.Add(Wave.Spawn("Miniboss 2: big Head-Thrower", 0.8f, E(EnemyArchetype.HeadThrower, 1, SpawnSide.B), E(EnemyArchetype.AntiAircraft, 2, SpawnSide.B))); // FIRST-PASS
                st.Waves.Add(Wave.Checkpoint("CHECKPOINT (main street)"));
                st.Waves.Add(Wave.Spawn("Miniboss 3: big Flying Monkey", 0.8f, E(EnemyArchetype.FlyingMonkey, 1, SpawnSide.Air), E(EnemyArchetype.Regular, 2, SpawnSide.L))); // FIRST-PASS
                st.Waves.Add(Wave.Spawn("Miniboss 4: big Arm-Ripper elite", 0.8f, E(EnemyArchetype.ArmRipper, 1, SpawnSide.B), E(EnemyArchetype.Regular, 2, SpawnSide.L))); // FIRST-PASS
                st.Waves.Add(Wave.Filler("Filler (Arm-Ripper/Snapper/Regular)", 8, 10));
                st.Waves.Add(Wave.Spawn("Wave 2 (funnel)", 0.8f, E(EnemyArchetype.ArmRipper, 3, SpawnSide.B), E(EnemyArchetype.Regular, 2, SpawnSide.L)));
                st.Waves.Add(Wave.Boss("BOSS: big Arm-Ripper"));
                s.Add(st);
            }

            // =========================== ACT 4 — Vallejo to the City ===========================

            // --- Stage 9 — Vallejo Six Flags + Ninja/Pickpocket debut → BOSS: Tank. ENCOUNTERS.md Stage 9. ---
            {
                var st = new StageData
                {
                    Id = 9,
                    DisplayName = "Vallejo Six Flags → Tank",
                    Area = "Vallejo → GG → SF",
                    BackdropTheme = "area4_vallejo",
                    MusicClip = "a4_circusrock",
                    AmbientClip = "vallejo_carnival",
                    NewestArchetype = EnemyArchetype.Ninja,
                    BossId = "tank",
                    BossMusicClip = "tank",
                };
                st.Waves.Add(Wave.Vignette("Vignette: pickpocket → ninja teleport-kill → 2× coins"));
                st.Waves.Add(Wave.Spawn("Wave 1 (Pickpocket debut)", 0.8f, E(EnemyArchetype.Pickpocket, 2, SpawnSide.A), E(EnemyArchetype.Regular, 3, SpawnSide.L)));
                st.Waves.Add(Wave.Spawn("Wave 2 (Ninja debut)", 0.8f, E(EnemyArchetype.Ninja, 2, SpawnSide.A), E(EnemyArchetype.Regular, 2, SpawnSide.L)));
                st.Waves.Add(Wave.Checkpoint("CHECKPOINT (midway)"));
                st.Waves.Add(Wave.Filler("Filler (Regular/Ninja/Pickpocket/Snapper)", 12, 16));
                st.Waves.Add(Wave.Spawn("Wave 3 (funnel)", 0.7f, E(EnemyArchetype.Regular, 4, SpawnSide.L), E(EnemyArchetype.Ninja, 1, SpawnSide.A)));
                st.Waves.Add(Wave.Boss("BOSS: Tank"));
                s.Add(st);
            }

            // --- Stage 10 — Bay causeway → Marin redwoods + Boomergunner debut → BOSS: Boomergunner. ENCOUNTERS.md Stage 10. ---
            {
                var st = new StageData
                {
                    Id = 10,
                    DisplayName = "Marin Redwoods → Boomergunner",
                    Area = "Vallejo → GG → SF",
                    BackdropTheme = "area4_marin",
                    MusicClip = "a4_psychrock",
                    AmbientClip = "marin_redwood",
                    NewestArchetype = EnemyArchetype.Boomergunner,
                    BossId = "boomergunner",
                    BossMusicClip = "boomergunner",
                };
                st.Waves.Add(Wave.Vignette("Vignette: boomer-gun shoots a civilian + returns"));
                st.Waves.Add(Wave.Spawn("Wave 1 (Boomergunner debut)", 0.8f, E(EnemyArchetype.Boomergunner, 2, SpawnSide.B), E(EnemyArchetype.Regular, 2, SpawnSide.L)));
                st.Waves.Add(Wave.Spawn("Wave 2", 0.8f, E(EnemyArchetype.Boomergunner, 1, SpawnSide.B), E(EnemyArchetype.Ninja, 2, SpawnSide.A), E(EnemyArchetype.Regular, 2, SpawnSide.L)));
                st.Waves.Add(Wave.Checkpoint("CHECKPOINT (redwood clearing)"));
                st.Waves.Add(Wave.Filler("Filler (Regular/Boomergunner/Ninja)", 12, 14));
                st.Waves.Add(Wave.Spawn("Wave 3 (funnel)", 0.7f, E(EnemyArchetype.Regular, 4, SpawnSide.L), E(EnemyArchetype.Boomergunner, 1, SpawnSide.B)));
                st.Waves.Add(Wave.Boss("BOSS: Boomergunner"));
                s.Add(st);
            }

            // --- Stage 11 — Golden Gate + Gatling/Ground Smasher debut → BOSS: Gatling Gun Guy. ENCOUNTERS.md Stage 11. ---
            {
                var st = new StageData
                {
                    Id = 11,
                    DisplayName = "Golden Gate Bridge → Gatling Gun Guy",
                    Area = "Vallejo → GG → SF",
                    BackdropTheme = "area4_goldengate",
                    MusicClip = "a4_orchestralrock",
                    AmbientClip = "goldengate_bridge_wind",
                    NewestArchetype = EnemyArchetype.GatlingGunner,
                    BossId = "gatlinggunguy",
                    BossMusicClip = "gatlinggunguy",
                };
                st.Waves.Add(Wave.Vignette("Vignette: stun → barrage → car-cover demo"));
                st.Waves.Add(Wave.Spawn("Wave 1 (Gatling debut)", 0.8f, E(EnemyArchetype.GatlingGunner, 2, SpawnSide.B), E(EnemyArchetype.Regular, 2, SpawnSide.L)));
                st.Waves.Add(Wave.Spawn("Wave 2 (Ground Smasher debut)", 0.8f, E(EnemyArchetype.GroundSmasher, 1, SpawnSide.B), E(EnemyArchetype.Ninja, 2, SpawnSide.A), E(EnemyArchetype.Regular, 2, SpawnSide.L)));
                st.Waves.Add(Wave.Checkpoint("CHECKPOINT (mid-span)"));
                st.Waves.Add(Wave.Filler("Filler (Regular/Gatling/Ground Smasher/Ninja)", 12, 16));
                st.Waves.Add(Wave.Spawn("Wave 3 (funnel)", 0.7f, E(EnemyArchetype.Regular, 4, SpawnSide.L), E(EnemyArchetype.GatlingGunner, 1, SpawnSide.B)));
                st.Waves.Add(Wave.Boss("BOSS: Gatling Gun Guy"));
                s.Add(st);
            }

            // --- Stage 12 — SF streets + Heavy debut + trolley → funnel to Tower. No boss. ENCOUNTERS.md Stage 12. ---
            {
                var st = new StageData
                {
                    Id = 12,
                    DisplayName = "San Francisco Streets → the Tower",
                    Area = "Vallejo → GG → SF",
                    BackdropTheme = "area4_sf",
                    MusicClip = "a4_electropunk",
                    AmbientClip = "sf_city_crowd",
                    NewestArchetype = EnemyArchetype.Heavy,
                };
                st.Waves.Add(Wave.Vignette("Vignette: trolley plows a Regular but the Heavy steps aside"));
                st.Waves.Add(Wave.Spawn("Wave 1 (Heavy debut)", 0.8f, E(EnemyArchetype.Heavy, 1, SpawnSide.B), E(EnemyArchetype.Regular, 3, SpawnSide.L)));
                st.Waves.Add(Wave.Spawn("Wave 2 (trolley pass)", 0.8f, E(EnemyArchetype.Ninja, 2, SpawnSide.A), E(EnemyArchetype.GatlingGunner, 1, SpawnSide.B), E(EnemyArchetype.Regular, 2, SpawnSide.L)));
                st.Waves.Add(Wave.Checkpoint("CHECKPOINT (tower plaza)"));
                st.Waves.Add(Wave.Filler("Filler (full-roster mix)", 14, 18));
                st.Waves.Add(Wave.Spawn("Wave 3 (two Heavies)", 0.8f, E(EnemyArchetype.Heavy, 2, SpawnSide.B), E(EnemyArchetype.Ninja, 2, SpawnSide.A)));
                st.Waves.Add(Wave.Spawn("Wave 4 (elevator funnel)", 0.7f, E(EnemyArchetype.GroundSmasher, 1, SpawnSide.B), E(EnemyArchetype.Boomergunner, 1, SpawnSide.B), E(EnemyArchetype.Regular, 3, SpawnSide.L)));
                s.Add(st);
            }

            // =========================== FINALE — Salesforce rooftop ===========================

            // --- Stage 13 (Finale) — BOSS: Phil. No waves before the boss. ENCOUNTERS.md Stage 13. ---
            {
                var st = new StageData
                {
                    Id = 13,
                    DisplayName = "Salesforce Rooftop → Phil",
                    Area = "Finale",
                    BackdropTheme = "finale_rooftop",
                    MusicClip = "finale_rooftop_approach",
                    AmbientClip = "finale_rooftop_wind",
                    LaneLengthWu = 30f, // pure boss arena (ENCOUNTERS.md boss-arena table: Phil 30 × 8 wu)
                    NewestArchetype = EnemyArchetype.Heavy,
                    BossId = "phil",
                    BossMusicClip = "phil_realized",
                };
                st.Waves.Add(Wave.Vignette("Vignette: Phil's monologue + tower sway (climb)"));
                st.Waves.Add(Wave.Boss("BOSS: Phil (sharpen-window finale)"));
                s.Add(st);
            }

            return s;
        }

        /// <summary>Endless Mode (STAGES.md §7b): full roster, refill at 2 remaining, scaling ramp.</summary>
        public static EndlessDescriptor Endless() => new()
        {
            MusicClip = "endless_layered",
            AmbientClip = "sf_city_crowd",
            BackdropTheme = "area1_suburb",
            RefillThreshold = 2,
            StartWaveSize = 3,
            MaxWaveSize = 8,
            RampEverySeconds = 30f,
            Pool = new[]
            {
                EnemyArchetype.Regular, EnemyArchetype.Swarmer, EnemyArchetype.Zombie,
                EnemyArchetype.Snapper, EnemyArchetype.HeadThrower, EnemyArchetype.AntiAircraft,
                EnemyArchetype.Sniper, EnemyArchetype.FlyingMonkey, EnemyArchetype.Monkey,
                EnemyArchetype.MonkeyTamer, EnemyArchetype.ArmRipper, EnemyArchetype.Ninja,
                EnemyArchetype.Pickpocket, EnemyArchetype.Boomergunner, EnemyArchetype.GatlingGunner,
                EnemyArchetype.GroundSmasher, EnemyArchetype.Heavy,
            },
        };
    }
}
