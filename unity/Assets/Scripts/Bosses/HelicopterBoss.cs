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
        private float _dropTimer = 2f;
        private int _strafeDir = 1;
        private float _minX, _maxX;

        public void Init(float x, float z)
        {
            // HP = 4 grenades (creator: "needs 4 grenades to die, and only gets hurt from grenades").
            InitBoss("helicopter", "Monkey Chopper", "helicopter", 4f * GrenadeHit, x, z,
                     new Color(0.6f, 0.75f, 0.6f), moveSpeed: 8.0f, sizeScale: 1.5f);
            // Real HELICOPTER art — a side-view chopper with a spinning rotor + monkey pilot.
            if (SpriteLibrary.HasAtlas("sprites/enemies/boss_helicopter", "boss_helicopter"))
            {
                Anim.Set = SpriteLibrary.Load("sprites/enemies/boss_helicopter", "boss_helicopter");
                if (Sr != null) Sr.color = Color.white;
                Anim.Play("walk", true, restart: true);
            }
            // OBJECTIVE boss: ONLY grenade lobs hurt it (see TakeDamage). It FLIES (hover, LateUpdate) and
            // you bring it down by LOBBING grenades up at it — the grenades come from the troops it drops.
            IsHpDepletion = false;
            PhaseThresholds = new[] { 0.5f };
            _minX = x - 15f;
            _maxX = x + 3f;
            Z = Tuning.ZBandDepth - 0.5f;          // hovers at the back/top band
        }

        protected override void BossUpdate(float dt, PlayerController player)
        {
            _fireTimer -= dt;
            _dropTimer -= dt;

            // Strafe left<->right across the top band, bouncing at the arena bounds.
            WorldX += _strafeDir * MoveSpeed * dt;
            if (WorldX >= _maxX) { WorldX = _maxX; _strafeDir = -1; }
            else if (WorldX <= _minX) { WorldX = _minX; _strafeDir = 1; }
            Anim.Play("walk", true);

            if (player == null || !player.Alive) return;

            // DROP TROOPS + a grenade: it throws out an enemy and kicks a grenade pickup down to the deck
            // — grab it and LOB it up at the chopper to damage it (creator).
            if (_dropTimer <= 0f)
            {
                if (CountAdds() < 3) SpawnAdd(EnemyArchetype.Regular);
                float gx = Mathf.Clamp(player.WorldX + Random.Range(-3f, 3f), _minX - 4f, _maxX + 8f);
                Pickup.SpawnWeapon(WeaponKind.Grenade, gx, Mathf.Clamp(player.Z, 0f, Tuning.ZBandDepth));
                Vfx.DeathBurst(WorldX, Z, 1.0f);
                Sfx.Play("head_throw");
                _dropTimer = CurrentPhase >= 2 ? 3.5f : 5f;
            }

            if (_fireTimer <= 0f && AirborneHeads() < 2)
            {
                RunAttack(HeadFire(player));
                _fireTimer = CurrentPhase >= 2 ? 1.8f : 2.5f;
            }
        }

        // FLY: lift the chopper well above the tarmac with a gentle hover bob (its logical X/Z stay on
        // the ground plane, so a lobbed grenade that lands on its column still hits it).
        protected override void LateUpdate()
        {
            base.LateUpdate();
            transform.position += Vector3.up * (3.6f + Mathf.Sin(Time.time * 2.2f) * 0.35f);
        }

        private IEnumerator HeadFire(PlayerController p)
        {
            Sfx.Play("boss_windup");
            yield return Telegraph(0.5f);          // arced telegraph
            if (!Alive || p == null) yield break;
            var head = ArcProjectile.Spawn(Team.Enemy, WorldX, Z, p.WorldX, p.Z, 15f,
                                           new Color(0.10f, 0.09f, 0.12f), airTime: 0.9f); // BLACK stick-figure head (creator)
            if (head != null)
            {
                head.name = "chopper_head";
                head.OnReflected = src => RegisterHeadReflect(src); // batted-back head = 1 pip (§5.5)
            }
            Sfx.Play("head_throw");
        }

        private static int AirborneHeads()
        {
            int n = 0;
            foreach (var go in Object.FindObjectsByType<ArcProjectile>(FindObjectsInactive.Exclude))
                if (go != null && go.name == "chopper_head") n++;
            return n;
        }

        private const float GrenadeHit = 35f;   // one lobbed grenade = one quarter of its HP

        /// <summary>ONLY a lobbed grenade brings it down (creator). A grenade blast lands ~35; bullets/
        /// melee (much less) just spark off the hull; specials whiff. Exactly 4 grenades down it.</summary>
        public override bool TakeDamage(float amount, Actor source)
        {
            if (!Alive) return false;
            if (amount < GrenadeHit - 5f)          // anything weaker than a grenade blast pings off
            {
                Vfx.HitSpark(WorldX, Z);
                return false;
            }
            Hp = Mathf.Max(0f, Hp - GrenadeHit);   // fixed chip so it's always exactly 4 grenades
            Vfx.DeathBurst(WorldX, Z, 1.4f);
            Sfx.Play("explosion");
            CameraShake.Add(CameraShake.Medium);
            if (Hp <= 0f) { ForceDefeat(source); return true; }
            return false;
        }

        /// <summary>Batting a head back is no longer a damage source — grenades only (creator).</summary>
        public void RegisterHeadReflect(Actor source) { }
    }
}
