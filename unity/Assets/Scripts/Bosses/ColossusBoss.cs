using System.Collections;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The Colossus — mid Area-2 (Sacramento) objective boss (BOSSES.md §5.4, TUNING.md §7).
    /// A giant stick-figure BUILT of 6 regular stick figures. You don't chip it down — you take
    /// it APART: a connecting forward WHIP crack (<see cref="IWhipPullable"/>) grabs one figure
    /// and RIPS it out, dropping it as a live Regular add you then kill, and knocking one segment
    /// off the giant's 6-piece bar. Six pulls and it collapses. Whip-less players can still loosen
    /// a piece by pummelling it (a much slower fallback so the fight can never soft-lock).
    ///   * Body swipe — 0.9 s windup, 22.5, cooldown 3 s (2.5 s at 4 pieces, 2 s at 2).
    ///   * Piece-spit — flings a loose figure for 15 every 4 s (ArcProjectile).
    /// </summary>
    public sealed class ColossusBoss : BossController, IWhipPullable
    {
        private const float LoosenPerPiece = 60f;   // brute-force fallback: ~60 dmg jars a piece loose

        private float _swipeTimer = 3f;
        private float _spitTimer = 4f;
        private float _loosen;                       // accumulated non-whip damage toward the next piece

        public void Init(float x, float z)
        {
            // OBJECTIVE boss: the 6-piece bar is stripped by whip-pulls, not chipped by HP (§5.4).
            InitBoss("colossus", "The Colossus", "colossus", 6f, x, z,
                     new Color(0.8f, 0.8f, 0.85f), moveSpeed: 3.5f, sizeScale: 2.6f);
            IsHpDepletion = false;                          // won by objective (whip-pull), never a special
            PhaseThresholds = new[] { 4f / 6f, 2f / 6f };   // speed up at 4 & 2 pieces remaining
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

        // ---- Whip-pull objective (the win condition) --------------------------

        /// <summary>Non-whip hits don't chip an objective boss — but they LOOSEN a piece, so a
        /// whip-less player still has a (much slower) path and the fight can't soft-lock.</summary>
        public override bool TakeDamage(float amount, Actor source)
        {
            if (Alive && amount > 0f && amount < SpecialLethalThreshold)
            {
                _loosen += amount;
                if (_loosen >= LoosenPerPiece) { _loosen -= LoosenPerPiece; StripPiece(source); }
            }
            return base.TakeDamage(amount, source); // still flashes; objective boss absorbs the HP
        }

        /// <summary>Whip system hook: a connecting forward whip crack rips a piece off instantly.</summary>
        public void RegisterWhipPull(Actor source) => StripPiece(source);

        /// <summary>Tear one stick figure out of the giant: drop it as a live Regular add flung
        /// toward the player, knock a segment off the 6-piece bar, and collapse at zero.</summary>
        private void StripPiece(Actor source)
        {
            if (!Alive) return;
            Hp = Mathf.Max(0f, Hp - 1f);

            // Rip a regular stick figure OUT of the giant, toward the puller, and stagger it so it
            // reads as "just torn loose" before it turns and fights.
            var add = SpawnAdd(EnemyArchetype.Regular);
            if (add != null)
            {
                float toward = source != null ? Mathf.Sign(source.WorldX - WorldX) : Facing;
                if (toward == 0f) toward = 1f;
                add.WorldX = WorldX + toward * 2.2f;
                add.Z = Z;
                if (add is IStaggerable s) s.ApplyStagger(0.5f);
            }

            Vfx.DeathBurst(WorldX, Z, 1.6f);
            Vfx.FinisherFlash(WorldX, Z);
            Sfx.Play("finisher_crunch");
            CameraShake.Add(CameraShake.Medium);
            Debug.Log($"[Boss:colossus] Piece ripped off ({Hp:0}/6 left).");
            if (Hp <= 0f) ForceDefeat(source); // all 6 pieces stripped -> the giant falls apart
        }
    }
}
