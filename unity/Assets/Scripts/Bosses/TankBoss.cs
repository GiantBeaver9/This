using System.Collections;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Tank — mid Area-4 (Vallejo) objective/puzzle boss (BOSSES.md §5.3, TUNING.md §7).
    /// NOT a health-bar slugfest and NOT executable: you dodge its machine-gun fire and
    /// win by <b>climbing on and dropping a grenade in the hatch ×2</b>. Progress is a
    /// 2-pip readout (HUD reads <see cref="Hp"/>/<see cref="MaxHp"/> as 2→0).
    ///   * MG fire — a horizontal beam sweeps one Z-row at a time, telegraphed by the
    ///     turret rotating 0.7 s, then a 1-HP/hit stream for 1.5 s, then it re-aims to a
    ///     new row. Phase 2 (after drop 1) adds a second lit row and one reposition.
    ///   * The objective — the arena's tier-1 adds drop only grenades (weapon-gate,
    ///     BOSSES.md §1). Call <see cref="RegisterGrenadeDrop"/> when the player lands a
    ///     hatch drop; 2 drops kill it.
    /// Grenade weapon/climb plumbing does not exist yet — see the gap in
    /// <c>_INTEGRATION.md</c>. The MG dodging gameplay is fully live; the objective is
    /// driven through the public API for the grenade system to call.
    /// </summary>
    public sealed class TankBoss : BossController
    {
        private int _drops;
        private float _mgTimer = 1f;
        private bool _repositioned;

        /// <summary>True when the player is at the rear tread and may mount (HUD prompt hook).
        /// FIRST-PASS: proximity only — the real gate also needs "holding a grenade".</summary>
        public bool CanMount { get; private set; }

        public void Init(float x, float z)
        {
            InitBoss("tank", "Tank", "tank", 2f, x, z,
                     new Color(0.5f, 0.55f, 0.45f), moveSpeed: 3.0f, sizeScale: 2.2f);
            IsHpDepletion = false;                 // objective boss — no ≤10% execute (BOSSES.md §1)
            PhaseThresholds = new[] { 0.5f };      // phase 2 after drop 1 (Hp 1 of 2)
        }

        protected override void BossUpdate(float dt, PlayerController player)
        {
            Anim.Play("idle", true);               // a stationary turret-puzzle
            _mgTimer -= dt;

            if (player != null && player.Alive)
            {
                float dx = player.WorldX - WorldX;
                CanMount = Mathf.Abs(dx) <= 2.5f && Mathf.Abs(player.Z - Z) <= 2f;
            }
            else CanMount = false;

            if (_mgTimer <= 0f && player != null && player.Alive)
            {
                RunAttack(MachineGunSweep(player));
                _mgTimer = 2.2f;
            }
        }

        private IEnumerator MachineGunSweep(PlayerController p)
        {
            // Aim: phase 1 one row, phase 2 two rows (BOSSES.md §5.3).
            float row0 = p.Z;
            float row1 = Mathf.Clamp(p.Z + (p.Z > Tuning.ZBandDepth * 0.5f ? -2f : 2f), 0f, Tuning.ZBandDepth);
            bool twoRows = CurrentPhase >= 2;

            // Turret rotates 0.7 s before firing (the tell).
            Anim.Play("attack_side", false, restart: true);
            Sfx.Play("boss_windup");
            yield return Telegraph(0.7f);
            if (!Alive) yield break;

            // 1 dmg/hit stream for 1.5 s on the lit row(s).
            for (float t = 0f; t < 1.5f && Alive; t += 0.12f)
            {
                Vfx.MuzzleFlash(WorldX + Facing * 0.9f, Z, Facing);
                Sfx.Play("gatling_gun");
                HitPlayerOnRow(row0, 0.4f, 30f, 1f);
                if (twoRows) HitPlayerOnRow(row1, 0.4f, 30f, 1f);
                yield return Telegraph(0.12f);
            }
        }

        /// <summary>
        /// Objective hook — the grenade system calls this on a successful hatch drop.
        /// Two drops kill the tank; drop 1 flips to phase 2 (MG intensifies + repositions).
        /// </summary>
        public void RegisterGrenadeDrop(Actor source)
        {
            if (!Alive) return;
            _drops++;
            Hp = Mathf.Max(0f, MaxHp - _drops);
            Vfx.FinisherFlash(WorldX, Z);
            Sfx.Play("explosion");
            CameraShake.Add(CameraShake.Heavy);
            Debug.Log($"[Boss:tank] Grenade drop {_drops}/2.");

            if (_drops == 1 && !_repositioned)
            {
                _repositioned = true;
                WorldX += (Random.value < 0.5f ? -1f : 1f) * 3f; // the one Phase-2 reposition
            }
            if (_drops >= 2) ForceDefeat(source); // objective complete -> defeated
        }
    }
}
