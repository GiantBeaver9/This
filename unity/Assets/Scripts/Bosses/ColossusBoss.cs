using System.Collections;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The Colossus — mid Area-2 (Sacramento) objective boss (BOSSES.md §5.4, TUNING.md §7).
    /// FIRST-PASS. A giant stick-figure built of 6 smaller ones; NOT executable. You win by
    /// ripping all 6 pieces off with the Whip — each forward whip-pull strips one piece
    /// (regular attacks do NOT chip pieces), and each torn piece becomes a T1 add on the
    /// ground. Progress is a 6-segment readout (Hp 6→0).
    ///   * Body swipe — 0.9 s windup, 22.5, cooldown 3 s (2.5 s at 4 pieces, 2 s at 2).
    ///   * Piece-spit — flings a loose figure for 15 every 4 s (ArcProjectile).
    /// The Whip weapon/pull is not implemented yet — see the gap in <c>_INTEGRATION.md</c>;
    /// call <see cref="RegisterWhipPull"/> from the whip system to strip a piece.
    /// </summary>
    public sealed class ColossusBoss : BossController
    {
        private float _swipeTimer = 3f;
        private float _spitTimer = 4f;

        public void Init(float x, float z)
        {
            InitBoss("colossus", "The Colossus", "colossus", 6f, x, z,
                     new Color(0.8f, 0.8f, 0.85f), moveSpeed: 3.5f, sizeScale: 2.6f);
            IsHpDepletion = false;                          // objective — no execute (BOSSES.md §1)
            PhaseThresholds = new[] { 4f / 6f, 2f / 6f };   // speed up at 4 & 2 pieces
        }

        protected override void BossUpdate(float dt, PlayerController player)
        {
            _swipeTimer -= dt;
            _spitTimer -= dt;

            // A lumbering giant: drift toward the player but keep its footprint.
            Reposition(player, dt, keep: 5f, speed: MoveSpeed);
            if (player == null || !player.Alive) return;

            if (_spitTimer <= 0f)
            {
                RunAttack(PieceSpit(player));
                _spitTimer = 4f;
                return;
            }
            if (_swipeTimer <= 0f)
            {
                RunAttack(BodySwipe());
                // cooldown tightens as pieces come off (BOSSES.md §5.4)
                _swipeTimer = CurrentPhase >= 3 ? 2f : CurrentPhase >= 2 ? 2.5f : 3f;
            }
        }

        private IEnumerator BodySwipe()
        {
            Anim.Play("attack_side", false, restart: true);
            Sfx.Play("boss_windup");
            yield return Telegraph(0.9f);
            if (!Alive) yield break;
            Sfx.Play("whoosh_heavy");
            HitPlayerIfInRange(3.5f, 22.5f);
            CameraShake.Add(CameraShake.Medium);
            yield return Telegraph(0.3f);
        }

        private IEnumerator PieceSpit(PlayerController p)
        {
            Anim.Play("attack_up", false, restart: true);
            Sfx.Play("boss_windup");
            yield return Telegraph(0.5f);
            if (!Alive || p == null) yield break;
            int spits = CurrentPhase >= 3 ? 2 : 1;          // "cornered giant" spits two at once
            for (int i = 0; i < spits; i++)
                ArcProjectile.Spawn(Team.Enemy, WorldX, Z, p.WorldX, p.Z, 15f,
                                    new Color(0.9f, 0.9f, 0.95f), airTime: 0.8f);
            yield return Telegraph(0.2f);
        }

        /// <summary>Objective hook — the whip system calls this on a successful forward pull.
        /// Strips one piece, drops a T1 add, and downs the giant at 0 pieces.</summary>
        public void RegisterWhipPull(Actor source)
        {
            if (!Alive) return;
            Hp = Mathf.Max(0f, Hp - 1f);
            Vfx.DeathBurst(WorldX, Z);
            Sfx.Play("hit_spark");
            SpawnAdd(EnemyArchetype.Regular);               // torn piece becomes a T1 add
            Debug.Log($"[Boss:colossus] Piece stripped ({Hp:0} left).");
            if (Hp <= 0f) ForceDefeat(source); // all 6 pieces stripped -> defeated
        }
    }
}
