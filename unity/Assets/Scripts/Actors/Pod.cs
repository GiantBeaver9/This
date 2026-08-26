using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The Pod spawner (TUNING §4, ENEMIES): a stationary, destroyable HP-50 sac
    /// that spits one Swarmer every 3s up to a shared field cap of 6 pod-spawned
    /// units (the one thing allowed to exceed the 8-pursuer cap). Fixed emit-type
    /// per instance; this build emits Swarmers.
    /// </summary>
    public sealed class Pod : Actor
    {
        public const int FieldCap = 6;
        public const float SpitInterval = 3f;

        private float _timer = SpitInterval;
        private bool _dead;

        public void Init(float x, float z)
        {
            Team = Team.Enemy;
            WorldX = x; Z = z;
            Hp = MaxHp = 50f * DifficultySettings.EnemyHpMult;
            if (Anim == null) Anim = GetComponent<SpriteAnimator>();
            Anim.Set = SpriteLibrary.Load("sprites/enemies/enemy_pod", "enemy_pod");
            Anim.Play("idle", true);
            Shadow.Attach(this, Shadow.LargeTier);
        }

        private void Update()
        {
            if (_dead) return;
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = SpitInterval;
                if (CountSwarmers() < FieldCap) Spit();
            }
        }

        private void Spit()
        {
            Anim.Play("spit", false, restart: true);
            Sfx.Play("pod_spawn_burst");
            var go = new GameObject("enemy_swarmer");
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<SpriteAnimator>();
            var e = go.AddComponent<EnemyController>();
            e.WorldX = WorldX + Random.Range(-0.6f, 0.6f);
            e.Z = Mathf.Clamp(Z + Random.Range(-0.6f, 0.6f), 0f, Tuning.ZBandDepth);
            e.Init(EnemyDef.Swarmer());
        }

        private static int CountSwarmers()
        {
            int n = 0;
            foreach (var a in Actor.All)
                if (a.Alive && a is EnemyController e && e.Def != null && e.Def.Id == "swarmer") n++;
            return n;
        }

        protected override void OnDeath(Actor source)
        {
            _dead = true;
            Anim.Play("death", false, restart: true);
            Vfx.DeathBurst(WorldX, Z);
            Sfx.Play("knockdown_thud");
            Destroy(gameObject, 1.0f);
        }
    }
}
