using UnityEngine;

namespace ThisL
{
    public enum LootTier { None, T1, T2, T3, T4 }

    /// <summary>
    /// The lootable weapon roster (WEAPONS.md §2–3, numbers in TUNING.md §6).
    /// Values appended after the original four so existing references stay stable.
    /// </summary>
    public enum WeaponKind
    {
        Fists, Sword, Shotgun, Boomerang,
        // --- roster expansion (WEAPONS.md §3) ---
        Pistol, Revolver,      // §3.1 straight-line guns
        Whip,                  // §3.4 directional crowd melee
        Staff,                 // §3.5 magic caster (element fixed at pickup)
        Bat,                   // §3.7b projectile reflector
        Club,                  // §3.7c basic heavy melee
        Grenade,               // §3.2 thrown, physics blast
        BallChain,             // §3.3 heavy directional launcher
        Gatling,               // §3.6 heavy risk/reward barrage
    }

    /// <summary>The magic element a Staff is locked to at pickup (WEAPONS.md §3.5).</summary>
    public enum StaffElement { Ice, Fire, Lightning }

    /// <summary>
    /// An equipped weapon's live state (WEAPONS.md). Weapons are looted from
    /// corpses and decay — a spend-it-before-it's-gone resource. Melee weapons
    /// swing through the player's combo (using <see cref="Reach"/>/<see cref="Damage"/>);
    /// ranged weapons carry their own fire behaviour in <see cref="FireImpl"/> so
    /// the roster can grow without touching the player. The decay indicator is
    /// diegetic (sword wear, shotgun spine segments) over the hit/ammo count here.
    /// </summary>
    public sealed class Weapon
    {
        public WeaponKind Kind;
        public float Reach;
        public int Damage;
        public int HitsRemaining;   // melee durability / ranged ammo
        public float Warmup;        // seconds to ready before first use
        public bool IsRanged;       // fired via Fire()/E, not swung through the combo
        public float FireCooldown;  // between-shots gap, ticked by the owner

        /// <summary>Element a Staff is locked to (Ice/Fire/Lightning); ignored by other kinds.</summary>
        public StaffElement Element;

        /// <summary>Ranged fire behaviour: returns true if a shot actually fired.</summary>
        public System.Func<PlayerController, bool> FireImpl;

        public bool IsFists => Kind == WeaponKind.Fists;

        public void Tick(float dt) => FireCooldown = Mathf.Max(0f, FireCooldown - dt);

        /// <summary>Fire a ranged weapon; no-op (returns false) for melee or while on cooldown.</summary>
        public bool TryFire(PlayerController p) =>
            IsRanged && FireCooldown <= 0f && FireImpl != null && FireImpl(p);

        /// <summary>Spend one melee durability point. Returns true if it just broke.</summary>
        public bool Spend()
        {
            if (IsFists) return false;
            HitsRemaining--;
            return HitsRemaining <= 0;
        }

        // ---- Factories --------------------------------------------------------
        /// <summary>Build the equipped Weapon for a looted kind (extend as the roster grows).</summary>
        public static Weapon Create(WeaponKind kind) => kind switch
        {
            WeaponKind.Sword => Sword(),
            WeaponKind.Shotgun => Shotgun(),
            WeaponKind.Boomerang => Boomerang(),
            WeaponKind.Pistol => Pistol(),
            WeaponKind.Revolver => Revolver(),
            WeaponKind.Whip => Whip(),
            WeaponKind.Staff => Staff(),
            WeaponKind.Bat => Bat(),
            WeaponKind.Club => Club(),
            WeaponKind.Grenade => Grenade(),
            WeaponKind.BallChain => BallChain(),
            WeaponKind.Gatling => Gatling(),
            _ => Fists(),
        };

        public static Weapon Fists() => new()
        {
            Kind = WeaponKind.Fists, Reach = Tuning.FistReach, Damage = Tuning.DmgPunch1,
            HitsRemaining = int.MaxValue, Warmup = 0f,
        };

        // ---- Melee (swung through the combo; no FireImpl) ---------------------

        public static Weapon Sword() => new()
        {
            Kind = WeaponKind.Sword, Reach = Tuning.SwordReach, Damage = 18,
            HitsRemaining = 8, Warmup = Tuning.WeaponWarmup, // 5-10 hits, midpoint 8 (§6)
        };

        /// <summary>Whip — directional crowd melee (§3.4/§6): 14/hit, 11 hits, fwd-pull reach 3.0 wu.</summary>
        public static Weapon Whip() => new()
        {
            Kind = WeaponKind.Whip, Reach = 3.0f, Damage = 14,
            HitsRemaining = 11, Warmup = 0.25f, // 10-12 hit range, midpoint 11 (§6)
        };

        /// <summary>Bat — projectile reflector (§3.7b/§6): 12/hit, 12 hits, short reach.
        /// The 0.20 s swing-timed reflect window is a PlayerController concern.</summary>
        public static Weapon Bat() => new()
        {
            Kind = WeaponKind.Bat, Reach = 1.6f, Damage = 12,
            HitsRemaining = 12, Warmup = 0.15f,
        };

        /// <summary>Club — basic heavy melee (§3.7c/§6): 14/hit + knockback, 10 hits, short reach.</summary>
        public static Weapon Club() => new()
        {
            Kind = WeaponKind.Club, Reach = 1.4f, Damage = 14,
            HitsRemaining = 10, Warmup = 0.15f,
        };

        /// <summary>Ball &amp; Chain — heavy launcher (§3.3/§6): swings as 20/hit heavy melee,
        /// 3 uses. The directional E-launch shapes and 20% carry-slow are wired in
        /// PlayerController (COMBOS.md §3); modelled here as a slow 3-durability melee.</summary>
        public static Weapon BallChain() => new()
        {
            Kind = WeaponKind.BallChain, Reach = 2.5f, Damage = 20,
            HitsRemaining = 3, Warmup = 0.40f,
        };

        /// <summary>Staff — magic caster (§3.5/§6). Element is randomly locked at pickup;
        /// per-hit value = that element's base (Ice 8 / Fire 6 / Lightning 12), 6 casts.
        /// Kept as a swung melee per the roster wiring; the freeze/DoT/stun status effects
        /// are enemy-side and are not modelled at the Weapon seam.</summary>
        public static Weapon Staff()
        {
            var el = (StaffElement)Random.Range(0, 3);
            int dmg = el switch { StaffElement.Ice => 8, StaffElement.Fire => 6, _ => 12 };
            return new Weapon
            {
                Kind = WeaponKind.Staff, Reach = 1.8f, Damage = dmg,
                HitsRemaining = 6, Warmup = 0.35f, Element = el,
            };
        }

        // ---- Ranged (fired via E; carry a FireImpl) --------------------------

        public static Weapon Shotgun()
        {
            var w = new Weapon
            {
                Kind = WeaponKind.Shotgun, Reach = 0f, Damage = 40,
                HitsRemaining = 5, Warmup = 0.25f, IsRanged = true, // 5 spine segments (§6)
            };
            w.FireImpl = p => WeaponFx.FireShotgun(p, w);
            return w;
        }

        public static Weapon Boomerang()
        {
            var w = new Weapon
            {
                Kind = WeaponKind.Boomerang, Reach = 0f, Damage = 8,
                HitsRemaining = int.MaxValue, Warmup = 0.15f, IsRanged = true,
            };
            w.FireImpl = p => WeaponFx.ThrowBoomerang(p, w);
            return w;
        }

        /// <summary>Pistol (§3.1/§6): 12 dmg, pierces up to 3 (12/6/3 halving), mag 8.</summary>
        public static Weapon Pistol()
        {
            var w = new Weapon
            {
                Kind = WeaponKind.Pistol, Reach = 0f, Damage = 12,
                HitsRemaining = 8, Warmup = 0.25f, IsRanged = true,
            };
            w.FireImpl = p => WeaponFx.FirePistol(p, w);
            return w;
        }

        /// <summary>Revolver (§3.1/§6): 30 dmg, no pierce, mag 6.</summary>
        public static Weapon Revolver()
        {
            var w = new Weapon
            {
                Kind = WeaponKind.Revolver, Reach = 0f, Damage = 30,
                HitsRemaining = 6, Warmup = 0.30f, IsRanged = true,
            };
            w.FireImpl = p => WeaponFx.FireRevolver(p, w);
            return w;
        }

        /// <summary>Grenade (§3.2/§6): a single thrown fastball, blast 35 (r 2 wu), self-dmg 40.</summary>
        public static Weapon Grenade()
        {
            var w = new Weapon
            {
                Kind = WeaponKind.Grenade, Reach = 0f, Damage = 35,
                HitsRemaining = 1, Warmup = 0.40f, IsRanged = true,
            };
            w.FireImpl = p => WeaponFx.ThrowGrenade(p, w);
            return w;
        }

        /// <summary>Gatling (§3.6/§6): E-barrage 45 on the nearest enemy ahead; no ammo,
        /// overheats after 5 barrages (the 20 s cumulative limit is a PlayerController concern).</summary>
        public static Weapon Gatling()
        {
            var w = new Weapon
            {
                Kind = WeaponKind.Gatling, Reach = 0f, Damage = 45,
                HitsRemaining = 5, Warmup = 0.40f, IsRanged = true, // 5 barrages then overheats
            };
            w.FireImpl = p => WeaponFx.FireGatling(p, w);
            return w;
        }
    }

    /// <summary>
    /// Ranged-weapon fire behaviours, kept off the player so the weapon roster can
    /// grow here. Each takes the firing player + the weapon instance, spawns the
    /// Z-aware shots, plays feedback, and manages the weapon's own ammo/cooldown.
    /// On empty, the weapon reverts to <see cref="Weapon.Fists"/> (auto-discard, §6).
    /// </summary>
    public static class WeaponFx
    {
        public static bool FireShotgun(PlayerController p, Weapon w)
        {
            var col = new Color(1f, 0.75f, 0.3f);
            foreach (float dz in new[] { -0.5f, 0f, 0.5f })
                Projectile.Spawn(Team.Player, p.WorldX + p.Facing * 0.6f,
                                 Mathf.Clamp(p.Z + dz, 0f, Tuning.ZBandDepth),
                                 p.Facing, 16f, w.Damage, col);
            Vfx.MuzzleFlash(p.WorldX + p.Facing * 0.9f, p.Z, p.Facing);
            Sfx.Play("shotgun_blast");
            CameraShake.Add(CameraShake.Light);
            w.FireCooldown = 0.35f; // cock time between shots
            if (w.Spend()) { Sfx.Play("shotgun_cock"); p.CurrentWeapon = Weapon.Fists(); }
            return true;
        }

        public static bool ThrowBoomerang(PlayerController p, Weapon w)
        {
            var proj = Projectile.Spawn(Team.Player, p.WorldX + p.Facing * 0.6f, p.Z, p.Facing, 18f, 0f,
                                        new Color(0.8f, 1f, 0.4f));
            proj.StunSeconds = 2f;
            proj.Life = 8f / 18f;                                    // 8 wu out then returns (§6.3)
            proj.OnConnect = () => p.CurrentWeapon = Weapon.Fists(); // lost the moment it connects
            w.FireCooldown = proj.Life + 0.1f;                       // re-throw after a miss returns
            Sfx.Play("boomerang_throw");
            return true;
        }

        /// <summary>Pistol: a piercing round straight ahead — 12/6/3 through up to 3 enemies (§3.1).</summary>
        public static bool FirePistol(PlayerController p, Weapon w)
        {
            PierceShot.Spawn(Team.Player, p.WorldX + p.Facing * 0.6f, p.Z, p.Facing,
                             40f, w.Damage, 3, 0.5f, new Color(1f, 0.95f, 0.5f))  // 40 wu/s, 12 wu range (§6.3)
                      .ZombifyChance = 0.10f;                                     // ~10% zombify on a headshot kill (§3.1)
            Vfx.MuzzleFlash(p.WorldX + p.Facing * 0.9f, p.Z, p.Facing);
            Sfx.Play("pistol");
            CameraShake.Add(CameraShake.Light);
            w.FireCooldown = 0.18f;
            if (w.Spend()) p.CurrentWeapon = Weapon.Fists();         // mag empty -> auto-discard
            return true;
        }

        /// <summary>Revolver: a single hard 30-dmg round straight ahead, no pierce (§3.1).</summary>
        public static bool FireRevolver(PlayerController p, Weapon w)
        {
            Projectile.Spawn(Team.Player, p.WorldX + p.Facing * 0.6f, p.Z, p.Facing,
                             40f, w.Damage, new Color(1f, 0.8f, 0.35f))
                      .ZombifyChance = 0.10f;                    // ~10% zombify on a headshot kill (§3.1)
            Vfx.MuzzleFlash(p.WorldX + p.Facing * 0.9f, p.Z, p.Facing);
            Sfx.Play("revolver");
            CameraShake.Add(CameraShake.Medium);
            w.FireCooldown = 0.30f;
            if (w.Spend()) p.CurrentWeapon = Weapon.Fists();
            return true;
        }

        /// <summary>Grenade: one thrown fastball that plows forward and detonates on contact or
        /// at 8 wu (§3.2). Blast 35 (r 2 wu) to enemies; 40 self-damage if you're too close.</summary>
        public static bool ThrowGrenade(PlayerController p, Weapon w)
        {
            GrenadeProjectile.Spawn(Team.Player, p.WorldX + p.Facing * 0.6f, p.Z, p.Facing,
                                    20f, 8f / 20f,             // 20 wu/s, detonate at 8 wu (§6.3)
                                    2f, w.Damage, 40f,         // r 2 wu, blast 35, self-dmg 40
                                    owner: p);                 // thrower: excluded from blast, pays self-dmg
            Sfx.Play("grenade_throw");
            w.FireCooldown = 0.5f;
            if (w.Spend()) p.CurrentWeapon = Weapon.Fists();   // 1 use -> gone
            return true;
        }

        /// <summary>Gatling: a ~0.5 s barrage into the nearest enemy ahead — a flat 45-dmg shot
        /// on your row within 8 wu (§3.6). No ammo; overheats after 5 barrages.</summary>
        public static bool FireGatling(PlayerController p, Weapon w)
        {
            Projectile.Spawn(Team.Player, p.WorldX + p.Facing * 0.6f, p.Z, p.Facing,
                             50f, w.Damage, new Color(1f, 0.6f, 0.2f))  // 45 to the nearest ahead
                      .ZombifyChance = 0.10f;                            // ~10% zombify on a headshot kill (§3.6)
            Vfx.MuzzleFlash(p.WorldX + p.Facing * 0.9f, p.Z, p.Facing);
            Sfx.Play("gatling_barrage");
            CameraShake.Add(CameraShake.Medium);
            w.FireCooldown = 0.5f;                              // no i-frames; locked through the barrage
            if (w.Spend()) p.CurrentWeapon = Weapon.Fists();   // overheat -> discard
            return true;
        }
    }

    /// <summary>Maps an enemy's level band to a (randomised) weapon drop (WEAPONS.md §4, TUNING §6).</summary>
    public static class LootTable
    {
        public static WeaponKind? Roll(LootTier tier)
        {
            // Most kills drop NOTHING (creator: weapon drop rate was way too high — the ground
            // was carpeted). Only ~WeaponDropChance of loot-bearing kills actually drop a weapon.
            if (tier == LootTier.None || Random.value > Tuning.WeaponDropChance) return null;
            switch (tier)
            {
                case LootTier.None:
                    return null;
                case LootTier.T1: // early, low commitment: throw toy + the T1 guns + starter sword
                    return RandomOf(WeaponKind.Sword, WeaponKind.Boomerang,
                                    WeaponKind.Pistol, WeaponKind.Revolver);
                case LootTier.T2: // the workhorses
                    return RandomOf(WeaponKind.Sword, WeaponKind.Whip, WeaponKind.Bat,
                                    WeaponKind.Staff, WeaponKind.Club, WeaponKind.BallChain);
                case LootTier.T3: // powerful, scarcer
                    return RandomOf(WeaponKind.Shotgun, WeaponKind.Gatling);
                case LootTier.T4: // spice / situational
                    return RandomOf(WeaponKind.Grenade, WeaponKind.Shotgun, WeaponKind.Gatling);
                default:
                    return Random.value < 0.6f ? WeaponKind.Shotgun : WeaponKind.Sword;
            }
        }

        private static WeaponKind RandomOf(params WeaponKind[] kinds) => kinds[Random.Range(0, kinds.Length)];
    }
}
