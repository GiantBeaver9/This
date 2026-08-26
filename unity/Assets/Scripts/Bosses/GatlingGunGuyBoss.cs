using System.Collections;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Gatling Gun Guy — Area-4 (Golden Gate) cap, a pure HP-depletion suppression boss
    /// (BOSSES.md §5.6, TUNING.md §7). HP 260, base move 4.0 wu/s. Two separate threats
    /// with two separate counters:
    ///   * BARRAGE — the ~5 s cycle (4 s in phase 2). A 2 s "BARRAGE INCOMING" warning
    ///     locks the player's current Z-row(s); then a 1.5 s barrage is <b>instant death
    ///     in the open</b> on those rows (dodge = step off the row / hard cover). Phase 3
    ///     locks two rows at once.
    ///   * CHIP STREAM — between barrages he repositions, keeps 1–2 Regular fodder alive,
    ///     and fires 1-HP/hit bursts sweeping the player's row (Shield-Rush soaks it).
    ///     Inside 3 wu he drops to a 22.5 melee.
    /// Executable at ≤10% HP. Cover (parked cars) is not modelled here — see the art/mech
    /// gap in <c>_INTEGRATION.md</c>; stepping off the lit row is the available dodge.
    /// </summary>
    public sealed class GatlingGunGuyBoss : BossController
    {
        private float _barrageTimer = 5f;
        private float _chipTimer = 2.5f;
        private float _fodderTimer = 3f;
        private float _repositionTimer = 4f;

        public void Init(float x, float z)
        {
            InitBoss("gatlinggunguy", "Gatling Gun Guy", "gatlinggunguy", 260f, x, z,
                     new Color(0.55f, 0.6f, 0.7f), moveSpeed: 4.0f);
            IsHpDepletion = true;
            PhaseThresholds = new[] { 0.66f, 0.33f };
        }

        protected override void BossUpdate(float dt, PlayerController player)
        {
            _barrageTimer -= dt;
            _chipTimer -= dt;
            _fodderTimer -= dt;
            _repositionTimer -= dt;

            if (player == null || !player.Alive) { Anim.Play("idle", true); return; }

            // Inside 3 wu he drops to melee (TUNING.md §7 base-move note).
            if (player.DistanceTo(this) <= 3f && _chipTimer <= 0f)
            {
                RunAttack(MeleeSwipe());
                _chipTimer = 1.5f;
                return;
            }

            // Barrage cadence: 5 s (phase 1) / 4 s (phase 2+).
            if (_barrageTimer <= 0f)
            {
                RunAttack(Barrage(player));
                _barrageTimer = CurrentPhase >= 2 ? 4f : 5f;
                return;
            }

            // Between barrages: suppressive chip stream sweeping the player's row.
            if (_chipTimer <= 0f)
            {
                RunAttack(ChipBurst(player));
                _chipTimer = 2.5f;
                return;
            }

            // Keep 1–2 Regular fodder alive (2-cap, BOSSES.md §1 arena add rule).
            if (_fodderTimer <= 0f)
            {
                if (CountAdds() < 2) SpawnAdd(EnemyArchetype.Regular);
                _fodderTimer = 3f;
            }

            // Reposition one "car length" between barrages.
            if (_repositionTimer <= 0f)
            {
                float dir = player.WorldX >= WorldX ? 1f : -1f;
                WorldX += dir * MoveSpeed * dt;
                Anim.Play("walk", true);
            }
            else Anim.Play("idle", true);
        }

        private IEnumerator Barrage(PlayerController p)
        {
            // Lock the target row(s) at warning time — the player must vacate during the 2 s tell.
            float row0 = p.Z;
            float row1 = Mathf.Clamp(p.Z + (p.Z > Tuning.ZBandDepth * 0.5f ? -2f : 2f), 0f, Tuning.ZBandDepth);
            bool twoRows = CurrentPhase >= 3;

            Sfx.Play("barrage_incoming");   // "BARRAGE INCOMING" (UI shows the banner; missing sfx no-ops)
            Anim.Play("attack_side", false, restart: true);
            for (float t = 0f; t < 2f && Alive; t += Time.deltaTime)
            {
                Vfx.Gust(WorldX, Z, Facing); // pulsing tell
                yield return null;
            }
            if (!Alive) yield break;

            Sfx.Play("gatling_barrage");
            for (float t = 0f; t < 1.5f && Alive; t += Time.deltaTime)
            {
                Vfx.MuzzleFlash(WorldX + Facing * 0.9f, Z, Facing);
                HitPlayerOnRow(row0, 0.4f, 32f, 9999f);          // instant death in the open
                if (twoRows) HitPlayerOnRow(row1, 0.4f, 32f, 9999f);
                CameraShake.Add(CameraShake.Light);
                yield return null;
            }
            yield return Telegraph(0.3f);
        }

        private IEnumerator ChipBurst(PlayerController p)
        {
            Anim.Play("attack_side", false, restart: true);
            float row = p.Z;
            // 1 s of 1-HP/hit shots down the row (Shield-Rush soaks this, BOSSES.md §5.6).
            for (float t = 0f; t < 1f && Alive; t += 0.12f)
            {
                Vfx.MuzzleFlash(WorldX + Facing * 0.9f, Z, Facing);
                Sfx.Play("gatling_gun");
                Projectile.Spawn(Team.Enemy, WorldX + Facing * 0.9f, row, Facing,
                                 16f, 1f, new Color(1f, 0.85f, 0.3f));
                yield return Telegraph(0.12f);
            }
        }

        private IEnumerator MeleeSwipe()
        {
            Anim.Play("attack_side", false, restart: true);
            Sfx.Play("boss_windup");
            yield return Telegraph(0.25f);
            if (!Alive) yield break;
            Sfx.Play("hit_spark");
            HitPlayerIfInRange(2f, 22.5f);
            CameraShake.Add(CameraShake.Medium);
            yield return Telegraph(0.2f);
        }
    }
}
