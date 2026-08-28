using System.Collections;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Monkey Boss — Area-3 (farm) proxy boss (BOSSES.md §5.7, TUNING.md §7). FIRST-PASS.
    /// HP 200, NOT executable. A proxy war: he throws dimes; the player catches them to
    /// field their own Monkey Mercs, and <b>only those mercs damage him</b> (0 direct
    /// player damage). Miss a dime and he summons his own pistol merc (T1 only, cap 3).
    ///   * Dime toss — a high arc every 4 s (3 s in phase 2; two dimes at once in phase 3).
    ///   * No direct attack (0 contact dmg); the threat is his mercs + positioning.
    /// The dime-catch / player-merc summon system is not implemented — see the gap in
    /// <c>_INTEGRATION.md</c>. FIRST-PASS stand-in: his HP depletes from any player-side
    /// damage (proxy for merc fire) and missed dimes spawn enemy Monkey mercs (cap 3).
    /// </summary>
    public sealed class MonkeyBoss : BossController
    {
        private float _dimeTimer = 4f;

        // FIRST-PASS: without the merc subsystem, let player-side damage deplete him so the
        // fight is winnable; the merc-only rule is noted as a gap. Specials still whiff
        // (IsHpDepletion=false routes 9999 to the negating execute gate).
        protected override bool TakesContactDamage => true;

        public void Init(float x, float z)
        {
            InitBoss("monkey_boss", "Monkey Boss", "monkeyboss", 200f, x, z,
                     new Color(0.7f, 0.55f, 0.4f), moveSpeed: 5.0f, sizeScale: 2.0f);
            IsHpDepletion = false;                 // proxy — no ≤10% execute (BOSSES.md §1)
            PhaseThresholds = new[] { 0.6f, 0.3f }; // dime cadence ramps at 60% / 30%
        }

        protected override void BossUpdate(float dt, PlayerController player)
        {
            _dimeTimer -= dt;
            // Repositions between tosses; deals no contact damage.
            Reposition(player, dt, keep: 5f, speed: MoveSpeed);
            if (player == null || !player.Alive) return;

            if (_dimeTimer <= 0f)
            {
                RunAttack(DimeToss(player));
                _dimeTimer = CurrentPhase >= 2 ? 3f : 4f;
            }
        }

        private IEnumerator DimeToss(PlayerController p)
        {
            Anim.Play("attack_up", false, restart: true);
            Sfx.Play("coin_toss");
            // Phase 3 throws two dimes to opposite sides (BOSSES.md §5.7).
            int dimes = CurrentPhase >= 3 ? 2 : 1;
            float[] spots = dimes == 2
                ? new[] { WorldX - 4f, WorldX + 4f }
                : new[] { p.WorldX + Random.Range(-2f, 2f) };

            yield return Telegraph(0.8f);          // high, telegraphed arc
            if (!Alive) yield break;

            foreach (float spot in spots)
            {
                Vfx.JumpPuff(spot, p.Z);           // landing marker
                // Real catchable dime (§5.7): catch it → summon a PLAYER merc directly; miss it → the
                // boss fields his own pistol merc (cap 3). The dime bypasses the coin cost + level cap.
                DimePickup.Spawn(spot, p.Z, () => { if (CountEnemyMercs() < 3) SpawnAdd(EnemyArchetype.Monkey); });
            }
        }

        private static int CountEnemyMercs()
        {
            int n = 0;
            foreach (var a in Actor.All)
                if (a.Alive && a is EnemyController e && e.Def != null && e.Def.Id == "monkey") n++;
            return n;
        }
    }
}
