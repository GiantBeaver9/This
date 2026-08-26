using System.Collections;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Burly Macho Guy — Area-1 (dept store) cap, a pure HP-depletion brawler
    /// (BOSSES.md §5.2, TUNING.md §7). Space-denier bruiser: HP 300, base move 4.5 wu/s.
    /// Three telegraphed attacks, escalating by phase (phase 2 at 200 HP, phase 3 at
    /// 100 HP):
    ///   * Ground-spike — 0.6 s windup (raised fist + ground glow), then a 4 wu-radius
    ///     AoE for 22.5, cooldown 2.5 s (1.5 s in phase 3);
    ///   * Enemy-toss — 0.8 s over-the-head windup; grabs a live arena add (any tier,
    ///     ignoring the normal tier rule) and hurls it for 40, dodged by changing Z-row;
    ///   * Charge (phase 2+) — 0.6 s rear-up telegraph, then a 12 wu/s shoulder rush
    ///     across the lane for 30 on contact, cooldown 3 s.
    /// Phase 3 pairs spike→charge on a 4 s cadence. Executable at ≤10% HP.
    /// </summary>
    public sealed class BurlyBoss : BossController
    {
        private float _tossReady;

        public void Init(float x, float z)
        {
            InitBoss("burly", "Burly Macho Guy", "burly", 300f, x, z,
                     new Color(0.95f, 0.55f, 0.45f), moveSpeed: 4.5f);
            IsHpDepletion = true;                       // executable at ≤10% (BOSSES.md §1)
            PhaseThresholds = new[] { 200f / 300f, 100f / 300f }; // ≈66% / ≈33%
        }

        protected override void BossUpdate(float dt, PlayerController player)
        {
            _tossReady = Mathf.Max(0f, _tossReady - dt);
            AttackTimer -= dt;

            Reposition(player, dt, keep: 3f, speed: MoveSpeed);
            if (player == null || !player.Alive || AttackTimer > 0f) return;

            // Phase 3: pair spike -> charge on a 4 s cadence (BOSSES.md §5.2).
            if (CurrentPhase >= 3)
            {
                RunAttack(SpikeThenCharge(player));
                AttackTimer = 4.0f;
                return;
            }

            // Opportunistic enemy-toss whenever an add is alive and off cooldown.
            if (_tossReady <= 0f && NearestAdd() != null)
            {
                RunAttack(EnemyToss(player));
                _tossReady = 6f;
                AttackTimer = 1.0f;
                return;
            }

            // Phase 2: mix in the charge.
            if (CurrentPhase >= 2 && Random.value < 0.4f)
            {
                RunAttack(Charge(player));
                AttackTimer = 3.0f;
                return;
            }

            RunAttack(GroundSpike(player));
            AttackTimer = CurrentPhase >= 3 ? 1.5f : 2.5f;
        }

        private IEnumerator GroundSpike(PlayerController p)
        {
            Anim.Play("attack_up", false, restart: true);
            Vfx.Gust(WorldX, Z, Facing);        // placeholder for the raised-fist / ground glow tell
            Sfx.Play("boss_windup");
            yield return Telegraph(0.6f);
            if (!Alive) yield break;

            Vfx.FinisherFlash(WorldX, Z);
            Sfx.Play("ground_smash");
            CameraShake.Add(CameraShake.Medium);
            HitPlayerIfInRange(4f, 22.5f);      // fast close-range AoE (BOSSES.md §5.2)
            yield return Telegraph(0.2f);
        }

        private IEnumerator EnemyToss(PlayerController p)
        {
            var add = NearestAdd();
            Anim.Play("attack_side", false, restart: true);
            Sfx.Play("boss_windup");
            yield return Telegraph(0.8f);       // clear over-the-head pose
            if (!Alive) yield break;

            if (add != null && add.Alive && p != null)
            {
                // Grab any add of any tier and hurl it — dodge by changing Z-row (splash lands at p).
                ArcProjectile.Spawn(Team.Enemy, WorldX, Z, p.WorldX, p.Z, 40f,
                                    new Color(1f, 0.6f, 0.3f), airTime: 0.7f);
                add.TakeDamage(9999f, this);    // the thrown add is consumed
                Sfx.Play("enemy_toss");
            }
            yield return Telegraph(0.2f);
        }

        private IEnumerator Charge(PlayerController p)
        {
            Anim.Play("attack_side", false, restart: true);
            int dir = (p != null && p.WorldX >= WorldX) ? 1 : -1;
            Facing = dir;
            Sfx.Play("boss_windup");
            yield return Telegraph(0.6f);       // rear-up pose: dash off his Z-row to dodge
            if (!Alive) yield break;

            Sfx.Play("dash_whoosh");
            float travelled = 0f, maxDist = 14f, speed = 12f;
            bool hit = false;
            while (travelled < maxDist && Alive)
            {
                float step = speed * Time.deltaTime;
                WorldX += dir * step;
                travelled += step;
                if (!hit && p != null && p.Alive && p.DistanceTo(this) <= 1.6f)
                {
                    p.TakeDamage(30f, this);    // floors on contact (H-weight)
                    CameraShake.Add(CameraShake.Heavy);
                    hit = true;
                }
                yield return null;
            }
            yield return Telegraph(0.3f);
        }

        private IEnumerator SpikeThenCharge(PlayerController p)
        {
            yield return GroundSpike(p);        // the spike leads
            if (!Alive) yield break;
            yield return Telegraph(0.2f);
            yield return Charge(p);             // the charge follows within the pair
        }
    }
}
