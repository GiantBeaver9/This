using System.Collections;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The Sniper — boss of the Rolling Hills / Yolo Causeway level (creator: "the sniper is the
    /// boss"). A pure HP-depletion perch-and-punish boss (executable at ≤10%). It holds long range
    /// and refuses to be cornered:
    ///   * SCOPE &amp; FIRE — paints a red laser reticle on your spot, holds ~1.1 s (0.8 s + leads you
    ///     in phase 2), then fires a hitscan shot for 30 (40 in P2). Dodge by leaving the painted spot.
    ///   * RELOCATE — periodically dashes to a fresh firing position so you can't camp its lane.
    ///   * SPOTTER — keeps a Regular add alive to flush you into the open (2-cap).
    ///   * RIFLE-BUTT — point-blank, a 22.5 melee so rushing it in isn't free.
    /// Reuses the enemy_sniper look until a bespoke boss atlas lands.
    /// </summary>
    public sealed class SniperBoss : BossController
    {
        private float _scopeTimer = 1.6f;
        private float _repositionTimer = 6f;
        private float _spotterTimer = 5f;
        private const float HoldRange = 10f;   // preferred standoff

        public void Init(float x, float z)
        {
            InitBoss("sniper_boss", "The Sniper", "sniper", 170f, x, z,
                     new Color(0.52f, 0.62f, 0.5f), moveSpeed: 6.5f);
            IsHpDepletion = true;
            PhaseThresholds = new[] { 0.5f };
            // Wear the sniper enemy's art (bespoke boss atlas is a later pass).
            if (SpriteLibrary.HasAtlas("sprites/enemies/enemy_sniper", "enemy_sniper"))
            {
                Anim.Set = SpriteLibrary.Load("sprites/enemies/enemy_sniper", "enemy_sniper");
                if (Sr != null) Sr.color = Color.white;
            }
        }

        protected override void BossUpdate(float dt, PlayerController player)
        {
            _scopeTimer -= dt;
            _repositionTimer -= dt;
            _spotterTimer -= dt;

            if (player == null || !player.Alive) { Anim.Play("idle", true); return; }

            float dist = Mathf.Abs(player.WorldX - WorldX);

            // Point-blank: rifle-butt so rushing it down isn't a free ride.
            if (player.DistanceTo(this) <= 2.2f && _scopeTimer <= 0.5f)
            {
                RunAttack(RifleButt());
                _scopeTimer = 1.4f;
                return;
            }

            // Scope & fire on the cadence.
            if (_scopeTimer <= 0f)
            {
                RunAttack(ScopeAndFire(player));
                _scopeTimer = CurrentPhase >= 2 ? 2.0f : 2.8f;
                return;
            }

            // Relocate to a fresh firing spot (a sniper never sits still).
            if (_repositionTimer <= 0f)
            {
                RunAttack(Relocate(player));
                _repositionTimer = CurrentPhase >= 2 ? 5f : 7f;
                return;
            }

            // Keep a spotter alive to flush the player out of cover (2-cap).
            if (_spotterTimer <= 0f)
            {
                if (CountAdds() < 1) SpawnAdd(EnemyArchetype.Regular);
                _spotterTimer = 6f;
            }

            // Hold the standoff: back away if the player closes, otherwise settle.
            if (dist < HoldRange)
            {
                float away = Mathf.Sign(WorldX - player.WorldX);
                if (away == 0f) away = 1f;
                WorldX += away * MoveSpeed * dt;
                Anim.Play("walk", true);
            }
            else Anim.Play("idle", true);
        }

        private IEnumerator ScopeAndFire(PlayerController p)
        {
            Anim.Play("attack_side", false, restart: true);
            Sfx.Play("sniper_scope_in");
            float tx = p.WorldX, tz = p.Z;                 // lock the spot at scope-in
            float warn = CurrentPhase >= 2 ? 0.8f : 1.1f;
            var reticle = SniperReticle.Spawn();
            for (float t = 0f; t < warn && Alive; t += Time.deltaTime)
            {
                // Phase 2 slowly LEADS the target (harder to juke); phase 1 paints a fixed spot.
                if (CurrentPhase >= 2 && p != null && p.Alive)
                {
                    tx = Mathf.Lerp(tx, p.WorldX, 0.05f);
                    tz = Mathf.Lerp(tz, p.Z, 0.05f);
                }
                if (reticle != null) reticle.Place(tx, tz);
                yield return null;
            }
            if (reticle != null) reticle.Kill();
            if (!Alive) yield break;

            // FIRE — muzzle, beam, shake, and a hitscan at the painted spot.
            Sfx.Play("sniper_shot");
            Facing = tx >= WorldX ? 1 : -1;
            Vfx.MuzzleFlash(WorldX + Facing * 0.9f, Z, Facing);
            SniperReticle.Beam(WorldX + Facing * 0.9f, Z, tx, tz);
            CameraShake.Add(CameraShake.Medium);
            Vfx.HitSpark(tx, tz);

            var target = PlayerController.Nearest(tx, tz);
            if (target != null && target.Alive &&
                Mathf.Abs(target.WorldX - tx) <= 1.0f && Playfield.WithinZ(target.Z, tz, 0.9f))
                target.TakeDamage(CurrentPhase >= 2 ? 40f : 30f, this);

            yield return Telegraph(0.3f);
        }

        private IEnumerator Relocate(PlayerController p)
        {
            Sfx.Play("dash_whoosh");
            Vfx.DashDust(WorldX, Z);
            // Jump to the opposite side of the player, back at range, on a fresh depth row.
            float side = p.WorldX >= WorldX ? -1f : 1f;
            float newX = p.WorldX + side * (HoldRange + 2f);
            float newZ = Mathf.Clamp(Tuning.ZBandDepth - 0.6f - Random.value * (Tuning.ZBandDepth - 1.2f),
                                     0f, Tuning.ZBandDepth);
            Anim.Play("walk", true, restart: true);
            for (float t = 0f; t < 0.35f && Alive; t += Time.deltaTime)
            {
                WorldX = Mathf.Lerp(WorldX, newX, 0.25f);
                Z = Mathf.Lerp(Z, newZ, 0.25f);
                yield return null;
            }
            WorldX = newX; Z = newZ;
            Vfx.DashDust(WorldX, Z);
        }

        private IEnumerator RifleButt()
        {
            Anim.Play("attack_side", false, restart: true);
            Sfx.Play("boss_windup");
            yield return Telegraph(0.22f);
            if (!Alive) yield break;
            Sfx.Play("hit_spark");
            HitPlayerIfInRange(2.4f, 22.5f);
            CameraShake.Add(CameraShake.Medium);
            yield return Telegraph(0.2f);
        }
    }

    /// <summary>The sniper's laser reticle (a red crosshair painted on your spot during the scope tell)
    /// and its shot beam. Purely visual; the hit is resolved by <see cref="SniperBoss"/>.</summary>
    internal sealed class SniperReticle : MonoBehaviour
    {
        private SpriteRenderer _sr;

        public static SniperReticle Spawn()
        {
            var go = new GameObject("sniper_reticle");
            var r = go.AddComponent<SniperReticle>();
            r._sr = go.AddComponent<SpriteRenderer>();
            r._sr.sprite = ReticleSprite();
            r._sr.sortingOrder = 940;
            return r;
        }

        public void Place(float x, float z)
        {
            Playfield.Place(transform, x, z, null);
            float pulse = 0.6f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * 18f));
            if (_sr != null) _sr.color = new Color(1f, 0.15f, 0.12f, pulse);
            transform.localScale = Vector3.one * 1.1f;
        }

        public void Kill() { if (this != null) Destroy(gameObject); }

        /// <summary>A brief red tracer line from the muzzle to the impact.</summary>
        public static void Beam(float x0, float z0, float x1, float z1)
        {
            var go = new GameObject("sniper_beam");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelSprite();
            sr.color = new Color(1f, 0.3f, 0.2f, 0.9f);
            sr.sortingOrder = 945;
            Vector3 a = new(x0, Playfield.FeetY(z0) + 0.8f, 0f);
            Vector3 b = new(x1, Playfield.FeetY(z1) + 0.8f, 0f);
            Vector3 mid = (a + b) * 0.5f;
            float len = Vector3.Distance(a, b);
            float ang = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
            go.transform.position = mid;
            go.transform.rotation = Quaternion.Euler(0, 0, ang);
            go.transform.localScale = new Vector3(len, 0.07f, 1f);
            Object.Destroy(go, 0.12f);
        }

        private static Sprite _reticle, _pixel;
        private static Sprite ReticleSprite()
        {
            if (_reticle != null) return _reticle;
            const int d = 15;
            var tex = new Texture2D(d, d, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[d * d];
            var red = new Color32(255, 40, 30, 255); var clear = new Color32(0, 0, 0, 0);
            int c = d / 2;
            for (int y = 0; y < d; y++)
                for (int x = 0; x < d; x++)
                {
                    float dx = x - c, dy = y - c, r = Mathf.Sqrt(dx * dx + dy * dy);
                    bool ring = r > 4.5f && r < 6.5f;
                    bool cross = (x == c && Mathf.Abs(dy) <= 6) || (y == c && Mathf.Abs(dx) <= 6);
                    px[y * d + x] = (ring || cross) ? red : clear;
                }
            tex.SetPixels32(px); tex.Apply();
            _reticle = Sprite.Create(tex, new Rect(0, 0, d, d), new Vector2(0.5f, 0.5f), Tuning.PixelsPerUnit);
            return _reticle;
        }

        private static Sprite PixelSprite()
        {
            if (_pixel != null) return _pixel;
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixel(0, 0, Color.white); tex.Apply();
            _pixel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _pixel;
        }
    }
}
