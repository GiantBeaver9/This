using System.Collections;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Helicopter (Monkey Chopper) — Area-2 (airport) cap, main-boss-only objective boss
    /// (BOSSES.md §5.5, TUNING.md §7). FIRST-PASS. NOT executable. An airborne monkey
    /// strafing the top band, firing stick-figure heads. You down it by filling a 6-pip
    /// bar: a reflected head = 1 pip, a lobbed grenade = 1.5 pips (6 heads, or 4 grenades,
    /// or any mix). HUD reads <see cref="Hp"/>/<see cref="MaxHp"/> as 6→0 pips remaining.
    ///   * Strafe — left↔right across the top band at 8 wu/s, dipping toward the player's row.
    ///   * Head-fire — arced heads (max 2 airborne) at the player, one every 2.5 s (1.8 s in
    ///     phase 2), 15 dmg; bat them back (Bat) or lob a grenade up to score.
    ///   * Phase 2 (at 3 pips) — descends lower, faster head-fire, adds a 0-dmg rotor-gust.
    /// The Bat-reflect / grenade-lob weapons are not implemented — see <c>_INTEGRATION.md</c>;
    /// call <see cref="RegisterHeadReflect"/> / <see cref="RegisterGrenadeLob"/> to score pips.
    /// </summary>
    public sealed class HelicopterBoss : BossController
    {
        private float _fireTimer = 2.5f;
        private float _gustTimer = 3f;
        private int _strafeDir = 1;
        private float _minX, _maxX;

        public void Init(float x, float z)
        {
            InitBoss("helicopter", "Monkey Chopper", "helicopter", 6f, x, z,
                     new Color(0.6f, 0.75f, 0.6f), moveSpeed: 8.0f, sizeScale: 2.2f);
            IsHpDepletion = false;                 // objective — no execute (BOSSES.md §1)
            PhaseThresholds = new[] { 0.5f };      // phase 2 at 3 pips (half of 6)
            _minX = x - 8f;
            _maxX = x + 8f;
            Z = Tuning.ZBandDepth - 0.5f;          // hovers at the back/top band
        }

        protected override void BossUpdate(float dt, PlayerController player)
        {
            _fireTimer -= dt;
            _gustTimer -= dt;

            // Strafe left<->right across the top band, bouncing at the arena bounds.
            WorldX += _strafeDir * MoveSpeed * dt;
            if (WorldX >= _maxX) { WorldX = _maxX; _strafeDir = -1; }
            else if (WorldX <= _minX) { WorldX = _minX; _strafeDir = 1; }
            Anim.Play("walk", true);

            if (player == null || !player.Alive) return;

            if (_fireTimer <= 0f && AirborneHeads() < 2)
            {
                RunAttack(HeadFire(player));
                _fireTimer = CurrentPhase >= 2 ? 1.8f : 2.5f;
            }

            // Phase-2 rotor-gust: 0-dmg positional push toward a Z-edge (BOSSES.md §5.5).
            if (CurrentPhase >= 2 && _gustTimer <= 0f)
            {
                float push = player.Z > Tuning.ZBandDepth * 0.5f ? 3f : -3f;
                player.Z = Mathf.Clamp(player.Z + push, 0f, Tuning.ZBandDepth);
                Vfx.Gust(player.WorldX, player.Z, 0);
                _gustTimer = 3f;
            }
        }

        private IEnumerator HeadFire(PlayerController p)
        {
            Sfx.Play("boss_windup");
            yield return Telegraph(0.5f);          // arced telegraph
            if (!Alive || p == null) yield break;
            var head = ArcProjectile.Spawn(Team.Enemy, WorldX, Z, p.WorldX, p.Z, 15f,
                                           new Color(0.95f, 0.9f, 0.8f), airTime: 0.9f);
            if (head != null) head.name = "chopper_head";
            Sfx.Play("head_throw");
        }

        private static int AirborneHeads()
        {
            int n = 0;
            foreach (var go in Object.FindObjectsByType<ArcProjectile>(FindObjectsInactive.Exclude))
                if (go != null && go.name == "chopper_head") n++;
            return n;
        }

        /// <summary>Objective hook — a batted-back head lands 1 pip (BOSSES.md §5.5).</summary>
        public void RegisterHeadReflect(Actor source) => ScorePips(1f, source);

        /// <summary>Objective hook — a lobbed grenade lands 1.5 pips (BOSSES.md §5.5).</summary>
        public void RegisterGrenadeLob(Actor source) => ScorePips(1.5f, source);

        private void ScorePips(float pips, Actor source)
        {
            if (!Alive) return;
            Hp = Mathf.Max(0f, Hp - pips);
            Vfx.HitSpark(WorldX, Z);
            Sfx.Play("explosion");
            CameraShake.Add(CameraShake.Medium);
            // Phase 2 descends lower (BOSSES.md §5.5).
            if (CurrentPhase >= 2) Z = Mathf.Min(Z, 3f);
            Debug.Log($"[Boss:helicopter] +{pips} pips ({Hp:0.0}/6 remaining).");
            if (Hp <= 0f) ForceDefeat(source); // 6-pip bar filled -> downed
        }
    }
}
