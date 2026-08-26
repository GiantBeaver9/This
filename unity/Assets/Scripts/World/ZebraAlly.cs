using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The Fighting Zebra fights ALONGSIDE the player for the opening ~15s of the run, then waves and
    /// runs off (creator: "have the zebra fight the enemies with the players for the first 10-20s, then
    /// it runs off — ease the player into it"). A lightweight helper (not a real player/Actor): it homes
    /// the nearest enemy, jabs it, and can't be hurt. When its timer runs out it cheers and sprints off
    /// the right edge, then despawns.
    /// </summary>
    public sealed class ZebraAlly : MonoBehaviour
    {
        public float FightSeconds = 15f;

        public float WorldX, Z;
        private int _facing = 1;
        private float _t, _atkCd;
        private bool _fleeing;
        private SpriteRenderer _sr;
        private SpriteAnimator _anim;

        private const float MoveSpeed = 5.5f;
        private const float Reach = 1.5f;

        public static ZebraAlly Spawn(float x, float z)
        {
            var go = new GameObject("ZebraAlly");
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<SpriteAnimator>();
            var za = go.AddComponent<ZebraAlly>();
            za.WorldX = x; za.Z = z;
            return za;
        }

        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
            _anim = GetComponent<SpriteAnimator>();
            if (SpriteLibrary.HasAtlas("sprites/characters/zebra_mascot", "zebra_mascot"))
                _anim.Set = SpriteLibrary.Load("sprites/characters/zebra_mascot", "zebra_mascot");
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _t += dt; _atkCd -= dt;

            if (!_fleeing && _t >= FightSeconds) { _fleeing = true; _anim.Play("cheer", false, restart: true); _cheerT = 0.7f; }
            if (_fleeing) { Flee(dt); return; }

            var foe = NearestEnemy();
            if (foe == null) { _anim.Play("idle", true); return; }

            float dx = foe.WorldX - WorldX, dz = foe.Z - Z;
            _facing = dx >= 0f ? 1 : -1;
            if (Mathf.Abs(dx) > Reach || Mathf.Abs(dz) > 0.6f)
            {
                WorldX += Mathf.Sign(dx) * MoveSpeed * dt;
                if (Mathf.Abs(dz) > 0.1f) Z += Mathf.Sign(dz) * MoveSpeed * dt;
                _anim.Play("walk", true);
            }
            else if (_atkCd <= 0f)
            {
                _atkCd = 0.85f;
                _anim.Play("attack_side", false, restart: true);
                foe.TakeDamage(11f, null);
                Vfx.HitSpark(foe.WorldX, foe.Z);
                if (foe is IStaggerable s) s.ApplyStagger(0.25f);
                Sfx.Play("punch_1");
            }
        }

        private float _cheerT;

        private void Flee(float dt)
        {
            if (_cheerT > 0f) { _cheerT -= dt; return; }   // brief "see ya!" wave before bolting
            _facing = 1;
            WorldX += 10f * dt;                            // sprint off to the right
            _anim.Play("walk", true);
            var cam = Camera.main;
            float edge = (cam != null ? cam.transform.position.x : WorldX) + Tuning.ScreenWidthUnits * 0.5f + 3f;
            if (WorldX > edge || _t > FightSeconds + 6f) Destroy(gameObject);
        }

        private Actor NearestEnemy()
        {
            Actor best = null; float bestD = 14f * 14f;    // only engage foes within ~a screen
            foreach (var a in Actor.All)
            {
                if (a == null || !a.Alive || a.Team != Team.Enemy) continue;
                float ddx = a.WorldX - WorldX, ddz = a.Z - Z, d = ddx * ddx + ddz * ddz;
                if (d < bestD) { bestD = d; best = a; }
            }
            return best;
        }

        private void LateUpdate()
        {
            Playfield.Place(transform, WorldX, Z, _sr);
            if (_anim != null) _anim.SetFacing(_facing);
        }
    }
}
