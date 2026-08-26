using System.Collections;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The three "big-version" bosses (BOSSES.md §1/§7, TUNING.md §7) — Sandwich Bros,
    /// big Arm-Ripper, and Boomergunner boss. FIRST-PASS. All are pure HP-depletion,
    /// executable at ≤10%, and fight SOLO (no arena adds, BOSSES.md §1). No new art —
    /// they reuse the enemy atlas at ~2× boss scale, tinted per boss. One config-driven
    /// controller covers all three: a melee variant (Sandwich Bros) and a ranged variant
    /// (Arm-Ripper akimbo pistols, Boomergunner shots). Per-shot damage stays at BASE for
    /// ranged big-versions (TUNING §7 ranged-override); melee takes the ×1.5 (already
    /// baked into the pinned numbers). Phase escalation is simplified to a cadence bump.
    /// </summary>
    public sealed class BigVersionBoss : BossController
    {
        public enum Mode { Melee, Ranged }

        public struct Config
        {
            public string Id, Display, Cue;
            public float Hp, Move, Dmg, FireInterval;
            public int ShotsPerReload;
            public Mode Mode;
            public float[] Thresholds;
            public Color Tint;
        }

        private Config _cfg;
        private float _attack;       // ranged fire timer
        private int _shots;          // shots since last reload
        private bool _reloading;

        // ---- Pinned configs (TUNING.md §7 rows) -------------------------------

        public static Config SandwichBros => new()
        {
            Id = "sandwich_bros", Display = "Sandwich Bros", Cue = null, // no dedicated cue
            Hp = 160f, Move = 5.9f, Dmg = 11f, Mode = Mode.Melee,
            Thresholds = new[] { 0.5f }, Tint = new Color(0.85f, 0.7f, 0.4f),
        };

        public static Config BigArmRipper => new()
        {
            Id = "big_armripper", Display = "big Arm-Ripper", Cue = "big_armripper",
            Hp = 280f, Move = 6.5f, Dmg = 7.5f, Mode = Mode.Ranged,
            FireInterval = 0.5f, ShotsPerReload = 6,           // 2 s reload after every 6 shots
            Thresholds = new[] { 0.66f, 0.33f }, Tint = new Color(0.7f, 0.4f, 0.4f),
        };

        public static Config Boomergunner => new()
        {
            Id = "boomergunner", Display = "Boomergunner", Cue = "boomergunner",
            Hp = 320f, Move = 5.0f, Dmg = 5f, Mode = Mode.Ranged,
            FireInterval = 0.5f, ShotsPerReload = 8,
            Thresholds = new[] { 0.66f, 0.33f }, Tint = new Color(0.5f, 0.5f, 0.75f),
        };

        public void Init(Config cfg, float x, float z)
        {
            _cfg = cfg;
            InitBoss(cfg.Id, cfg.Display, cfg.Cue, cfg.Hp, x, z, cfg.Tint, cfg.Move);
            IsHpDepletion = true;                              // executable at ≤10%
            PhaseThresholds = cfg.Thresholds;
            _attack = cfg.FireInterval;
        }

        protected override void BossUpdate(float dt, PlayerController player)
        {
            if (player == null || !player.Alive) { Anim.Play("idle", true); return; }

            if (_cfg.Mode == Mode.Melee) MeleeBehaviour(dt, player);
            else RangedBehaviour(dt, player);
        }

        // ---- Melee (Sandwich Bros) -------------------------------------------

        private void MeleeBehaviour(float dt, PlayerController player)
        {
            AttackTimer -= dt;
            Reposition(player, dt, keep: 1.4f, speed: MoveSpeed);
            if (AttackTimer > 0f) return;
            if (player.DistanceTo(this) <= 2.2f)
            {
                RunAttack(LungePunch());
                // Phase 2 "faster" = +20% cadence (BOSSES.md §7 Sandwich note).
                AttackTimer = CurrentPhase >= 2 ? 1.0f : 1.25f;
            }
        }

        private IEnumerator LungePunch()
        {
            Anim.Play("attack_side", false, restart: true);
            Sfx.Play("boss_windup");
            yield return Telegraph(CurrentPhase >= 2 ? 0.22f : 0.3f);
            if (!Alive) yield break;
            Sfx.Play("punch_2");
            HitPlayerIfInRange(2.2f, _cfg.Dmg);
            CameraShake.Add(CameraShake.Medium);
            yield return Telegraph(0.2f);
        }

        // ---- Ranged (Arm-Ripper akimbo / Boomergunner) -----------------------

        private void RangedBehaviour(float dt, PlayerController player)
        {
            // Hold a standoff, easing onto the player's Z-row.
            Steering.KeepDistance(this, player.WorldX, Z, 8f, MoveSpeed, dt);
            Z += Mathf.Clamp(player.Z - Z, -MoveSpeed * dt, MoveSpeed * dt);

            // Phase 3: a rolling reposition between volleys (big Arm-Ripper, BOSSES.md §7).
            if (_reloading) { Anim.Play("walk", true); return; }

            _attack -= dt;
            if (_attack > 0f) { Anim.Play("idle", true); return; }

            // Fire rate ramps by phase: base 2/s -> 3/s at ≤66% (Arm-Ripper).
            float interval = CurrentPhase >= 2 ? _cfg.FireInterval * 0.66f : _cfg.FireInterval;
            FireShot(player);
            _attack = interval;

            _shots++;
            if (_cfg.ShotsPerReload > 0 && _shots >= _cfg.ShotsPerReload)
                RunAttack(Reload());   // the 2 s reload is the punish window
        }

        private void FireShot(PlayerController player)
        {
            Anim.Play("attack_side", false, restart: true);
            Vfx.MuzzleFlash(WorldX + Facing * 0.8f, Z, Facing);
            Sfx.Play("pistol");
            // Ranged big-versions keep BASE per-shot damage (TUNING §7 override).
            Projectile.Spawn(Team.Enemy, WorldX + Facing * 0.8f, Z, Facing,
                             12f, _cfg.Dmg, new Color(1f, 0.85f, 0.3f));
        }

        private IEnumerator Reload()
        {
            _reloading = true;
            Anim.Play("hurt", false, restart: true);
            Sfx.Play("reload");
            // Phase 3 rolls to a new position while reloading.
            float rollDir = Random.value < 0.5f ? -1f : 1f;
            float t = 0f;
            while (t < 2f && Alive)
            {
                if (CurrentPhase >= 3) WorldX += rollDir * MoveSpeed * Time.deltaTime;
                t += Time.deltaTime;
                yield return null;
            }
            _shots = 0;
            _reloading = false;
        }
    }
}
