using UnityEngine;

namespace ThisL
{
    /// <summary>A playable character's special-move payload (CHARACTERS.md, TUNING §3.1).</summary>
    public interface ICharacterSpecial
    {
        string Name { get; }
        void Fire(PlayerController p, int tier);
    }

    /// <summary>
    /// A playable character (CHARACTERS.md, TUNING §3). All four share the moveset;
    /// they differ in stat multipliers and their Special. Stats/specials here are a
    /// first-pass scaffold from the design tables — the bespoke art and final tuning
    /// come later. Sprite atlases live under assets/sprites/characters/&lt;actor&gt;.
    /// </summary>
    public sealed class CharacterDef
    {
        public string Id;
        public string DisplayName;
        public string SpriteDir;
        public string SpriteActor;
        public float MoveSpeedMult = 1f;
        public float PunchDmgMult = 1f;
        public float MeterFillMult = 1f;
        public float WeaponDmgMult = 1f;
        public float Scale = 1f;            // in-game silhouette size (Bert is short)
        public ICharacterSpecial Special;

        public static CharacterDef[] Roster() => new[] { Tactical(), Shotgunner(), Werewolf(), Underdog() };

        // TUNING §3 multiplier columns: MoveSpeed · PunchDmg · MeterFill · WeaponDmg
        public static CharacterDef Tactical() => new()
        {
            Id = "tactical", DisplayName = "Adam",
            SpriteDir = "sprites/characters/player_tactical", SpriteActor = "player_tactical",
            MoveSpeedMult = 1.12f, PunchDmgMult = 0.85f, MeterFillMult = 1.25f, WeaponDmgMult = 1.15f,
            Special = new SniperSpecial(),
        };

        public static CharacterDef Shotgunner() => new()
        {
            Id = "shotgunner", DisplayName = "Aaron",
            SpriteDir = "sprites/characters/player_shotgunner", SpriteActor = "player_shotgunner",
            MoveSpeedMult = 0.92f, PunchDmgMult = 1.20f, MeterFillMult = 1.00f, WeaponDmgMult = 1.20f,
            Special = new GiantShotgunSpecial(),
        };

        public static CharacterDef Werewolf() => new()
        {
            Id = "werewolf", DisplayName = "Gabe",
            SpriteDir = "sprites/characters/player_werewolf", SpriteActor = "player_werewolf",
            MoveSpeedMult = 1.00f, PunchDmgMult = 1.00f, MeterFillMult = 1.00f, WeaponDmgMult = 1.00f,
            Special = new WerewolfSpecial(),
        };

        public static CharacterDef Underdog() => new()
        {
            Id = "underdog", DisplayName = "Bert",
            SpriteDir = "sprites/characters/player_underdog", SpriteActor = "player_underdog",
            MoveSpeedMult = 1.00f, PunchDmgMult = 0.80f, MeterFillMult = 1.00f, WeaponDmgMult = 0.80f,
            Scale = 0.78f, // the short friend (hard mode)
            Special = new VaporizeSpecial(),
        };
    }

    // ---- Specials (first-pass; TUNING §3.1 has the full survivor/tier rules) ----

    /// <summary>Tactical — sniper ricochet. One-shots 15/30/45 nearest enemies, drops nothing.</summary>
    public sealed class SniperSpecial : ICharacterSpecial
    {
        public string Name => "Sniper";
        public void Fire(PlayerController p, int tier)
        {
            Sfx.Play("sniper_scope_in");
            Sfx.Play("sniper_shot");
            p.Anim?.Play("special", false, restart: true); // the gun-spin+fire art slots in here
            // Time slows and the shot caroms enemy-to-enemy, one kill at a time.
            SpecialSequences.SniperRicochet(p, SpecialMeter.SniperKills(tier));
            Debug.Log($"[Special] Sniper tier {tier}: slow-mo ricochet up to {SpecialMeter.SniperKills(tier)}.");
        }
    }

    /// <summary>Shotgunner — Giant Shotgun (first-pass): heavy forward-cone blast, keeps drops.</summary>
    public sealed class GiantShotgunSpecial : ICharacterSpecial
    {
        public string Name => "Giant Shotgun";
        public void Fire(PlayerController p, int tier)
        {
            float length = tier <= 1 ? 6f : tier == 2 ? 8f : 10f; // cone length by fill
            Sfx.Play("giant_shotgun_boom");
            p.Anim?.Play("special", false, restart: true); // gun-spin+fire art
            Vfx.MuzzleFlash(p.WorldX + p.Facing * 1.0f, p.Z, p.Facing);
            SpecialFx.Ring(p.WorldX + p.Facing * length * 0.4f, p.Z, length * 0.5f, new Color(1f, 0.7f, 0.25f));
            CameraShake.Add(CameraShake.Heavy);
            float knock = tier <= 1 ? 8f : tier == 2 ? 11f : 14f; // large pushback by fill
            int hit = 0;
            foreach (var e in Combat.EnemiesInFrontCone(p, length, 4f))
            {
                Vfx.HitSpark(e.WorldX, e.Z);
                e.WorldX += p.Facing * knock;                       // blasted back
                if (e is IStaggerable s) s.ApplyStagger(3.6f);       // knocked down 3x the normal 1.2s
                e.TakeDamage(45f, p);                                // massive damage, keeps drops
                hit++;
            }
            Debug.Log($"[Special] Giant Shotgun tier {tier}: {hit} blasted back + floored 3.6s.");
        }
    }

    /// <summary>Werewolf — 5s i-frame slash-all 1HKO (first-pass), keeps drops.</summary>
    public sealed class WerewolfSpecial : ICharacterSpecial
    {
        public string Name => "Werewolf";
        public void Fire(PlayerController p, int tier)
        {
            Sfx.Play("werewolf_transform_howl");
            p.Anim?.Play("transform", false, restart: true); // power-lean + arch-back + scream art
            SpecialFx.Ring(p.WorldX, p.Z, 2.6f, new Color(1f, 0.8f, 0.3f));
            float dur = tier <= 1 ? 5f : tier == 2 ? 7f : 9f;  // transform window by fill
            // Hold the bigger, glowing wolf form (i-frames + auto-slash), then fade back to human.
            SpecialSequences.Werewolf(p, dur);
            Debug.Log($"[Special] Werewolf tier {tier}: {dur}s transform, fades back.");
        }
    }

    /// <summary>Underdog — Vaporize (first-pass): 3wu instakill (no drops) + 30s +20% damage.</summary>
    public sealed class VaporizeSpecial : ICharacterSpecial
    {
        public string Name => "Vaporize";
        public void Fire(PlayerController p, int tier)
        {
            Sfx.Play("underdog_vaporize_whomp");
            float radius = tier <= 1 ? 3.0f : tier == 2 ? 4.0f : 5.0f; // wider radius per fill
            p.Anim?.Play("special", false, restart: true);
            SpecialFx.Ring(p.WorldX, p.Z, radius, new Color(0.75f, 0.5f, 1f));   // the vaporize radius
            SpecialFx.Glow(p, new Color(0.75f, 0.5f, 1f), 0.6f);                 // the empower glow
            int kills = 0;
            var doomed = new System.Collections.Generic.List<Actor>();
            foreach (var a in Actor.All)
                if (a.Alive && a.Team == Team.Enemy && p.DistanceTo(a) <= radius) doomed.Add(a);
            foreach (var a in doomed)
            {
                Vfx.DeathBurst(a.WorldX, a.Z);
                if (a is ISpecialKillable k) k.KillBySpecial(p); else a.TakeDamage(9999f, p); // no drops
                kills++;
            }
            p.SetDamageBuff(1.2f, 30f);
            Debug.Log($"[Special] Vaporize tier {tier}: {kills} vaporized, +20% dmg 30s.");
        }
    }
}
