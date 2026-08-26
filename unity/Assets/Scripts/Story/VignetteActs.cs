using System.Collections;

namespace ThisL
{
    /// <summary>
    /// The authored per-stage ACTED vignettes — the "show don't tell" set-pieces from
    /// VIGNETTES.md, staged as short wordless pantomimes on the field via the
    /// <see cref="VignetteStaging"/> verb library (creator north star: "SHOW DON'T TELL
    /// — as fun as possible"; the meaning is carried by the ACTION, with at most one
    /// short caption line pulled from <see cref="VignetteScripts"/>).
    ///
    /// One iterator per stage (stages 3–12 = 0-based indices 2–11; stage 1's opener is
    /// the tutorial's job, and stages 2/13 have no acted teaching beat). Each act is
    /// self-contained and tuned by the local consts at its head, so they are cheap to
    /// re-stage and iterate. <see cref="ForStage"/> maps a stage index to its act; the
    /// director runs it through <see cref="VignetteStaging.Begin"/>.
    ///
    /// Offsets are in world-units relative to the playlet anchor (<see cref="VignetteStaging.Ax"/>,
    /// a few units ahead of the player): negative = screen-left, positive = screen-right.
    /// </summary>
    public static class VignetteActs
    {
        /// <summary>Fallback ceiling (seconds) the director arms its timer to; every act
        /// finishes well inside this, so the pantomime plays out naturally.</summary>
        public const float MaxSeconds = 9f;

        /// <summary>True if stage <paramref name="stageIndex"/> (0-based) has an acted vignette.</summary>
        public static bool Has(int stageIndex) => stageIndex >= 2 && stageIndex <= 11;

        /// <summary>The acted sequence for a stage, or an empty one if none is authored.</summary>
        public static IEnumerator ForStage(VignetteStaging s, int stageIndex)
        {
            switch (stageIndex)
            {
                case 2:  return Galleria(s);     // Stage 3
                case 3:  return Sacramento(s);   // Stage 4
                case 4:  return Airport(s);      // Stage 5
                case 5:  return Causeway(s);     // Stage 6
                case 6:  return Farm(s);         // Stage 7
                case 7:  return Dixon(s);        // Stage 8
                case 8:  return Vallejo(s);      // Stage 9
                case 9:  return Marin(s);        // Stage 10
                case 10: return GoldenGate(s);   // Stage 11
                case 11: return SfStreets(s);    // Stage 12
                default: return Empty();
            }
        }

        private static IEnumerator Empty() { yield break; }

        // ================================================================
        // Stage 3 · Galleria — guard shoots a thug → it zombifies → grabs
        // him → both fall. Teaches: shots turn thugs into grabbers.
        // ================================================================
        private static IEnumerator Galleria(VignetteStaging s)
        {
            s.Stinger(VignetteScripts.A1Galleria);
            s.Teach(VignetteScripts.A1Galleria);

            var guard = s.Spawn(-2.2f, VignetteStaging.GroundZ, +1);
            var thug  = s.Spawn(+2.2f, VignetteStaging.GroundZ, -1);
            yield return VignetteStaging.Wait(0.6f);

            // Guard panics and shoots — thug staggers but doesn't die (it turns).
            s.Shoot(guard, thug, kill: false, sfx: "pistol");
            yield return s.Knockback(thug, +1, 0.8f, 0.15f);
            yield return VignetteStaging.Wait(0.5f);

            // It zombifies and lurches back at the guard.
            s.PlayClip(thug, "idle", true);
            Sfx.Play("zombie_turn");
            yield return s.MoveTo(thug, -1.4f, VignetteStaging.GroundZ, 3.2f);

            // Grabs him — they both go down.
            s.Punch(thug, guard, kill: true, sfx: "grab");
            s.Kill(thug);
            yield return VignetteStaging.Wait(1.0f);
        }

        // ================================================================
        // Stage 4 · Sacramento — one enemy whips another off his feet, then
        // both spot you and turn. Teaches: the Whip drags you in.
        // ================================================================
        private static IEnumerator Sacramento(VignetteStaging s)
        {
            s.Stinger(VignetteScripts.A2Sacramento);
            s.Teach(VignetteScripts.A2Sacramento);

            var whip   = s.Spawn(-2.5f, VignetteStaging.GroundZ, +1);
            var target = s.Spawn(+2.8f, VignetteStaging.GroundZ, -1);
            yield return VignetteStaging.Wait(0.6f);

            // Whip crack — the target is yanked hard toward the whipper and knocked down.
            s.PlayClip(whip, "attack_side");
            Sfx.Play("whip_crack");
            yield return VignetteStaging.Wait(0.1f);
            s.Hurt(target);
            yield return s.Knockback(target, -1, 3.4f, 0.16f);
            yield return VignetteStaging.Wait(0.5f);

            // Both spot you and turn toward the camera/player.
            s.PlayClip(target, "idle", true);
            s.Face(whip, -1);
            s.Face(target, -1);
            s.PlayClip(whip, "attack_side");
            Sfx.Play("alarm");
            yield return VignetteStaging.Wait(0.9f);
        }

        // ================================================================
        // Stage 5 · Airport — enemies lob head-grenades at planes; a Bat swats
        // a fastball head into a Cessna. Teaches: heads arc in, the Bat reflects.
        // ================================================================
        private static IEnumerator Airport(VignetteStaging s)
        {
            s.Stinger(VignetteScripts.A2Airport);
            s.Teach(VignetteScripts.A2Airport);

            var thrower = s.Spawn(-2.6f, VignetteStaging.GroundZ, +1);
            var bat     = s.Spawn(+1.0f, VignetteStaging.GroundZ, +1);
            yield return VignetteStaging.Wait(0.5f);

            // Thrower lobs its own head — an arcing tell across the sky.
            s.Toss(thrower, "head_throw");
            yield return VignetteStaging.Wait(0.08f);
            s.Spark(-1.6f, 4.6f);
            yield return VignetteStaging.Wait(0.12f);
            s.Spark(-0.4f, 5.4f);
            yield return VignetteStaging.Wait(0.12f);
            s.Spark(+0.8f, 5.0f);
            yield return VignetteStaging.Wait(0.1f);

            // The Bat swats the fastball head — it slams a small plane offscreen: boom.
            s.Face(bat, +1);
            s.PlayClip(bat, "attack_side");
            Sfx.Play("bat_swing");
            yield return VignetteStaging.Wait(0.12f);
            s.Boom(+4.5f, 5.2f, "boom");
            yield return VignetteStaging.Wait(0.9f);
        }

        // ================================================================
        // Stage 6 · Causeway — two dive for a dime; one leaps and a Sniper
        // shoots him from the air; the other grabs the dime, whistles, and a
        // monkey drags him off. Teaches: don't hang in the air; grab it first.
        // ================================================================
        private static IEnumerator Causeway(VignetteStaging s)
        {
            s.Stinger(VignetteScripts.A3Causeway);
            s.Teach(VignetteScripts.A3Causeway);

            var leaper  = s.Spawn(-2.4f, VignetteStaging.GroundZ, +1);
            var grabber = s.Spawn(+2.4f, VignetteStaging.GroundZ, -1);
            var sniper  = s.Spawn(+5.5f, VignetteStaging.GroundZ - 0.6f, -1);
            s.Spark(0f, 0.6f); // the dime glints on the ground at centre
            yield return VignetteStaging.Wait(0.4f);

            // Both dive for it.
            var toDime = s.MoveTo(leaper, -0.6f, VignetteStaging.GroundZ, 5f);
            yield return s.MoveTo(grabber, +0.6f, VignetteStaging.GroundZ, 5f);
            yield return toDime;

            // Leaper jumps high — the Sniper picks him out of the air.
            s.PlayClip(leaper, "attack_side");
            Sfx.Play("jump");
            leaper.Z = VignetteStaging.GroundZ + 2.4f;
            yield return VignetteStaging.Wait(0.18f);
            Sfx.Play("sniper_scope_in");
            s.Shoot(sniper, leaper, kill: true, sfx: "sniper_shot");
            yield return VignetteStaging.Wait(0.5f);

            // The other snatches the dime, whistles, and a monkey hauls him off.
            s.PlayClip(grabber, "attack_side");
            Sfx.Play("coin_pickup");
            yield return VignetteStaging.Wait(0.2f);
            Sfx.Play("whistle");
            var monkey = s.Spawn(+7.5f, VignetteStaging.GroundZ, -1, "sprites/enemies/enemy_monkey", "enemy_monkey");
            yield return s.MoveTo(monkey, +1.2f, VignetteStaging.GroundZ, 8f);
            var drag1 = s.MoveTo(grabber, +9f, VignetteStaging.GroundZ, 7f);
            yield return s.MoveTo(monkey, +9.6f, VignetteStaging.GroundZ, 7f);
            yield return drag1;
        }

        // ================================================================
        // Stage 7 · Farm — the Monkey Boss flips a dime up; a thug catches it,
        // a monkey merc pops out and shoots the boss; a plain punch does nothing.
        // Teaches: only your mercs damage him.
        // ================================================================
        private static IEnumerator Farm(VignetteStaging s)
        {
            s.Stinger(VignetteScripts.A3Farm);
            s.Teach(VignetteScripts.A3Farm);

            var boss = s.Spawn(+3.2f, VignetteStaging.GroundZ, -1);
            s.Scale(boss, 1.5f);
            var thug = s.Spawn(-0.5f, VignetteStaging.GroundZ, +1);
            yield return VignetteStaging.Wait(0.5f);

            // A plain punch on the boss — he shrugs it off (spark, no flinch).
            var puncher = s.Spawn(+1.6f, VignetteStaging.GroundZ, +1);
            s.Punch(puncher, boss, kill: false, sfx: "punch_1");
            s.Spark(+3.0f, VignetteStaging.GroundZ + 0.8f);
            yield return VignetteStaging.Wait(0.5f);
            s.Kill(puncher); // shooed off

            // Boss flips a dime up; the thug catches it.
            s.Toss(boss, "coin_pickup");
            s.Spark(+3.0f, 5.2f);
            yield return VignetteStaging.Wait(0.25f);
            s.PlayClip(thug, "attack_side");
            Sfx.Play("coin_pickup");
            yield return VignetteStaging.Wait(0.2f);

            // A monkey merc pops out and unloads on the boss — THAT lands.
            var merc = s.Spawn(-1.1f, VignetteStaging.GroundZ, +1, "sprites/enemies/enemy_monkey", "enemy_monkey");
            s.Shoot(merc, boss, kill: false, sfx: "pistol");
            s.Hurt(boss);
            yield return VignetteStaging.Wait(0.9f);
        }

        // ================================================================
        // Stage 8 · Dixon — an Arm-Ripper tears a guy's arms off and opens fire
        // with akimbo pistols. Teaches: rip first, shoot second.
        // ================================================================
        private static IEnumerator Dixon(VignetteStaging s)
        {
            s.Stinger(VignetteScripts.A3Dixon);
            s.Teach(VignetteScripts.A3Dixon);

            var ripper = s.Spawn(-1.2f, VignetteStaging.GroundZ, +1);
            var victim = s.Spawn(+1.2f, VignetteStaging.GroundZ, -1);
            yield return VignetteStaging.Wait(0.5f);

            // Rip the arms off the guy next door — he drops.
            s.PlayClip(ripper, "attack_side");
            Sfx.Play("crunch");
            yield return VignetteStaging.Wait(0.12f);
            s.Kill(victim);
            yield return VignetteStaging.Wait(0.5f);

            // Akimbo: opens fire toward the player, twice.
            s.Fire(ripper, -1, "pistol");
            yield return VignetteStaging.Wait(0.18f);
            s.Fire(ripper, -1, "pistol");
            yield return VignetteStaging.Wait(0.9f);
        }

        // ================================================================
        // Stage 9 · Vallejo — a Pickpocket lifts a Ninja's coins and bolts; the
        // Ninja blinks across and cuts him down; the loot doubles. Teaches:
        // Ninjas teleport onto you, kill a Pickpocket for double.
        // ================================================================
        private static IEnumerator Vallejo(VignetteStaging s)
        {
            s.Stinger(VignetteScripts.A4Vallejo);
            s.Teach(VignetteScripts.A4Vallejo);

            var ninja = s.Spawn(0f, VignetteStaging.GroundZ, +1,
                "sprites/enemies/enemy_ninja", "enemy_ninja");
            var pick  = s.Spawn(-3.2f, VignetteStaging.GroundZ, +1,
                "sprites/enemies/enemy_pickpocket", "enemy_pickpocket");
            yield return VignetteStaging.Wait(0.4f);

            // Pickpocket darts in, lifts the coins, and bolts.
            yield return s.MoveTo(pick, -0.7f, VignetteStaging.GroundZ, 7f);
            s.PlayClip(pick, "attack_side");
            Sfx.Play("coin_pickup");
            s.Spark(0f, VignetteStaging.GroundZ + 0.8f);
            yield return VignetteStaging.Wait(0.2f);
            var flee = s.MoveTo(pick, +4f, VignetteStaging.GroundZ, 9f);

            // Ninja blinks across the screen and cuts him down.
            yield return VignetteStaging.Wait(0.25f);
            yield return s.Blink(ninja, +3.2f, VignetteStaging.GroundZ);
            yield return flee;
            s.Punch(ninja, pick, kill: true, sfx: "sword_slash");

            // The stolen ten cents doubles to twenty.
            yield return VignetteStaging.Wait(0.15f);
            Sfx.Play("coin_pickup");
            s.Spark(+3.2f, VignetteStaging.GroundZ + 0.8f);
            Sfx.Play("coin_pickup");
            s.Spark(+3.6f, VignetteStaging.GroundZ + 1.1f);
            yield return VignetteStaging.Wait(0.8f);
        }

        // ================================================================
        // Stage 10 · Marin — a Boomergunner hurls his boomerang gun; it shoots a
        // bystander on the way out, then loops back to his hand. Teaches: the gun
        // is dangerous both directions.
        // ================================================================
        private static IEnumerator Marin(VignetteStaging s)
        {
            s.Stinger(VignetteScripts.A4Marin);
            s.Teach(VignetteScripts.A4Marin);

            var gunner = s.Spawn(-2.6f, VignetteStaging.GroundZ, +1);
            var civ    = s.Spawn(+2.4f, VignetteStaging.GroundZ, -1);
            yield return VignetteStaging.Wait(0.5f);

            // Hurls the boomerang gun — it flies out to the right.
            s.Toss(gunner, "boomerang_throw");
            for (float x = -2f; x <= 2.4f; x += 1.1f)
            {
                s.Spark(x, VignetteStaging.GroundZ + 0.9f);
                yield return VignetteStaging.Wait(0.06f);
            }

            // Shoots the bystander at the apex of its arc.
            s.Shoot(gunner, civ, kill: false, sfx: "pistol");
            yield return s.Knockback(civ, +1, 0.6f, 0.12f);
            yield return VignetteStaging.Wait(0.15f);

            // Loops back to his hand.
            for (float x = 2.4f; x >= -2.4f; x -= 1.1f)
            {
                s.Spark(x, VignetteStaging.GroundZ + 0.9f);
                yield return VignetteStaging.Wait(0.06f);
            }
            s.PlayClip(gunner, "attack_side");
            Sfx.Play("boomerang_throw");
            yield return VignetteStaging.Wait(0.8f);
        }

        // ================================================================
        // Stage 11 · Golden Gate — an advancer is slammed off-balance by a Ground
        // Smasher, then a Gatling barrage eviscerates him — but the guy behind a
        // car is untouched. Teaches: get a car between you and the barrage.
        // ================================================================
        private static IEnumerator GoldenGate(VignetteStaging s)
        {
            s.Stinger(VignetteScripts.A4GoldenGate);
            s.Teach(VignetteScripts.A4GoldenGate);

            var advancer = s.Spawn(-1.4f, VignetteStaging.GroundZ, +1);
            var smasher  = s.Spawn(+3.0f, VignetteStaging.GroundZ, -1);
            s.Scale(smasher, 1.3f);
            var gatling  = s.Spawn(+5.2f, VignetteStaging.GroundZ - 0.6f, -1);
            var cover    = s.Spawn(+1.6f, 1.0f, +1); // crouched behind a car, up front
            yield return VignetteStaging.Wait(0.5f);

            // Advancer pushes forward.
            yield return s.MoveTo(advancer, +0.4f, VignetteStaging.GroundZ, 3.5f);

            // Ground Smasher slams the deck — advancer is knocked off-balance.
            s.PlayClip(smasher, "attack_side");
            s.Boom(+2.6f, 0.4f, "ground_smash");
            yield return VignetteStaging.Wait(0.06f);
            s.Hurt(advancer);
            yield return s.Knockback(advancer, -1, 0.9f, 0.16f);
            yield return VignetteStaging.Wait(0.2f);

            // Gatling barrage eviscerates him where he stands.
            yield return s.Barrage(gatling, +0.4f, advancer != null ? advancer.Z : VignetteStaging.GroundZ, 7, 0.06f);
            s.Kill(advancer);

            // The guy behind the car? Not a scratch — he just stays put.
            s.PlayClip(cover, "idle", true);
            yield return VignetteStaging.Wait(0.9f);
        }

        // ================================================================
        // Stage 12 · SF streets — the trolley flattens a thug, but the Heavy just
        // steps aside and it rolls on. Teaches: the trolley kills everything that
        // can move.
        // ================================================================
        private static IEnumerator SfStreets(VignetteStaging s)
        {
            s.Stinger(VignetteScripts.A4SfStreets);
            s.Teach(VignetteScripts.A4SfStreets);

            var thug  = s.Spawn(0f, VignetteStaging.GroundZ, -1);
            var heavy = s.Spawn(+1.8f, VignetteStaging.GroundZ, -1);
            s.Scale(heavy, 1.5f);
            yield return VignetteStaging.Wait(0.5f);

            // The Heavy reads the horn and steps aside early; the thug doesn't.
            Sfx.Play("horn");
            yield return VignetteStaging.Wait(0.25f);
            yield return s.MoveTo(heavy, +1.8f, VignetteStaging.GroundZ - 1.6f, 4f);

            // The trolley barrels down the lane (a fast dust sweep + rumble).
            for (float x = -6f; x <= 8f; x += 1.4f)
            {
                s.Dust(x, VignetteStaging.GroundZ);
                if (x >= -0.7f && thug != null && thug.Alive)
                {
                    s.Kill(thug);                 // flattened as the trolley passes
                    s.Boom(0f, VignetteStaging.GroundZ, "impact");
                }
                yield return VignetteStaging.Wait(0.05f);
            }

            // The Heavy didn't even look up.
            s.PlayClip(heavy, "idle", true);
            yield return VignetteStaging.Wait(0.9f);
        }
    }
}
