using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Anti-Aircraft rock-lobber AI (ENEMIES.md §2.10, TUNING §4 row 4). Holds at
    /// its pinned standoff (8 wu), telegraphs an overhead throw for ~0.5 s, then
    /// lobs an arcing <see cref="ArcProjectile"/> at the player's spot — dodged by
    /// stepping off the landing point. Backs off if the player closes inside the
    /// standoff. Drops loot on death like <see cref="EnemyController"/> (nothing
    /// on a sniper-special kill). Boomerang-bait counterplay (ENEMIES §2.10) is a
    /// gap; the straight-shot fallback is <see cref="RangedEnemyController"/>.
    /// </summary>
    public sealed class AntiAircraftController : Actor, ISpecialKillable, IStaggerable
    {
        private enum State { Hold, Windup, Chase, Dead }

        [System.NonSerialized] public EnemyDef Def;
        private State _state = State.Hold;
        private float _cooldown;
        private float _windup;
        private float _stagger;
        private float _targetX, _targetZ;
        private bool _killedBySpecial;
        private bool _isHead;    // HeadThrower: rips its OWN HEAD off and lobs it as a live bomb (creator)
        private bool _headless;  // already threw its head -> a headless body that chases + melees
        private GameObject _wiggle; // the head held overhead, wiggling, during the throw wind-up
        private float _meleeCd;

        public void Init(EnemyDef def)
        {
            Def = def;
            Team = Team.Enemy;
            Hp = MaxHp = def.Hp;
            _isHead = def.Id == "head_thrower";
            if (Anim == null) Anim = GetComponent<SpriteAnimator>();
            Anim.Set = SpriteLibrary.Load(def.SpriteDir, def.SpriteActor);
            Anim.Play("idle", true);
            // No rock-head tint (creator: "the rock head enemies shouldn't have rock heads") — plain look.
            Shadow.Attach(this, Shadow.MediumTier);
        }

        private void Update()
        {
            if (_state == State.Dead) return;
            float dt = Time.deltaTime;
            _cooldown = Mathf.Max(0f, _cooldown - dt);

            if (_stagger > 0f) { _stagger -= dt; Anim.Play("hurt", false); Steering.Separate(this); return; }

            var player = PlayerController.Nearest(WorldX, Z);
            if (player == null || !player.Alive) { Anim.Play("idle", true); return; }

            Facing = player.WorldX >= WorldX ? 1 : -1;

            // HEADLESS body: chase the player and melee (creator: "then the headless body chases").
            if (_state == State.Chase)
            {
                _meleeCd = Mathf.Max(0f, _meleeCd - dt);
                float cx = player.WorldX - WorldX;
                Facing = cx >= 0f ? 1 : -1;
                float chaseSpeed = Def.Speed * 1.6f;   // the headless body is RELENTLESS — runs you down
                bool inReach = Mathf.Abs(cx) <= Def.Reach + 0.3f && Playfield.WithinZ(player.Z, Z, 0.9f);
                if (!inReach)
                {
                    WorldX += Mathf.Sign(cx) * chaseSpeed * dt;
                    Z += Mathf.Clamp(player.Z - Z, -chaseSpeed * dt, chaseSpeed * dt);
                    Steering.Separate(this);
                    Anim.Play("walk", true);
                }
                else if (_meleeCd <= 0f)
                {
                    _meleeCd = 0.8f;
                    Anim.Play("attack_side", false, restart: true);
                    if (player.DistanceTo(this) <= Def.Reach + 0.5f) player.TakeDamage(Def.Damage, this);
                }
                else Anim.Play("attack_side", false);  // stay swinging, never relax to a standing idle
                return;
            }

            if (_state == State.Windup)
            {
                _windup -= dt;
                // Wiggle the head held OVERHEAD in both hands during the wind-up (creator: "wiggle their
                // heads with their arms, then chuck it").
                if (_isHead && _wiggle != null)
                {
                    Playfield.Place(_wiggle.transform, WorldX + Mathf.Sin(Time.time * 24f) * 0.22f, Z, null);
                    _wiggle.transform.position += Vector3.up * 2.1f;
                }
                if (_windup <= 0f)
                {
                    if (_isHead)
                    {
                        // CHUCK the head from the hands as a live BOMB, then go headless + chase.
                        float hx = WorldX, hz = Z;
                        if (_wiggle != null) { Destroy(_wiggle); _wiggle = null; }
                        Vfx.DeathBurst(hx, hz, 1.0f);
                        Sfx.Play("finisher_crunch");
                        float lx = _targetX, lz = _targetZ;
                        var head = ArcProjectile.Spawn(Team.Enemy, hx, hz + 1.8f, lx, lz,
                                                       Def.Damage, Color.white, airTime: 0.9f,
                                                       sprite: HeadSprite(), spinDegPerSec: 400f);
                        head.SplashRadius = 1.8f;                              // it's a BOMB — AoE
                        head.ArcHeight = 3.2f;
                        head.OnLand = () => { Vfx.DeathBurst(lx, lz, 2.2f); Sfx.Play("grenade_explode"); CameraShake.Add(CameraShake.Medium); };
                        GoHeadless();
                        _state = State.Chase;
                    }
                    else
                    {
                        // Lob a rock at the spot the player was standing when the throw committed.
                        ArcProjectile.Spawn(Team.Enemy, WorldX, Z, _targetX, _targetZ,
                                            Def.Damage, new Color(0.65f, 0.55f, 0.45f), airTime: 0.9f);
                        Sfx.Play("knockdown_thud");
                        _cooldown = Def.AttackCooldown;
                        _state = State.Hold;
                    }
                }
                return;
            }

            // Hold the standoff, slide onto the player's row, keep separation.
            Steering.KeepDistance(this, player.WorldX, Z, Def.HoldDistance, Def.Speed, dt);
            float dz = player.Z - Z;
            Z += Mathf.Clamp(dz, -Def.Speed * dt, Def.Speed * dt);
            Steering.Separate(this);

            float dx = Mathf.Abs(player.WorldX - WorldX);
            if (dx <= Def.FireRange && _cooldown <= 0f)
            {
                _state = State.Windup;
                _windup = Def.WindupSeconds;
                _targetX = player.WorldX;
                _targetZ = player.Z;
                Anim.Play("attack_side", false, restart: true);   // arms up working the head (regular has no attack_up)
                if (_isHead)
                {
                    _wiggle = MakeWiggleHead();     // the head, held overhead, about to be wiggled + chucked
                    Sfx.Play("guard_whistle");      // a "grab" tell (missing sfx no-ops)
                }
            }
            else
            {
                Anim.Play(Mathf.Abs(dz) > 0.35f || dx > Def.FireRange ? "walk" : "idle", true);
            }
        }

        public void ApplyStagger(float seconds)
        {
            if (_state == State.Dead) return;
            _stagger = seconds;
            if (_wiggle != null) { Destroy(_wiggle); _wiggle = null; } // drop the head if hit mid-wind-up
            _state = _headless ? State.Chase : State.Hold;             // a headless body can't re-throw
            Anim.Play("hurt", false, restart: true);
        }

        // ---- Head-throw helpers -------------------------------------------------
        private void GoHeadless()
        {
            _headless = true;
            if (SpriteLibrary.HasAtlas("sprites/enemies/enemy_headless", "enemy_headless"))
                Anim.Set = SpriteLibrary.Load("sprites/enemies/enemy_headless", "enemy_headless");
            if (Sr != null) Sr.color = Color.white;
            Anim.Play("walk", true, restart: true);
        }

        private GameObject MakeWiggleHead()
        {
            var go = new GameObject("wiggle_head");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = HeadSprite();
            sr.sortingOrder = Playfield.SortingOrder(Z) + 5;
            return go;
        }

        private static Sprite _headSprite;
        private static Sprite HeadSprite()
        {
            if (_headSprite != null) return _headSprite;
            const int d = 13;
            var tex = new Texture2D(d, d, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[d * d];
            var skin = new Color32(224, 178, 140, 255);           // a real HEAD, not a pale bomb
            var hair = new Color32(70, 52, 44, 255);
            var dark = new Color32(30, 26, 24, 255);
            float r = d / 2f;
            for (int y = 0; y < d; y++)
                for (int x = 0; x < d; x++)
                {
                    float dx = x - r + 0.5f, dy = y - r + 0.5f;
                    if (dx * dx + dy * dy > r * r) { px[y * d + x] = new Color32(0, 0, 0, 0); continue; }
                    px[y * d + x] = y >= d - 4 ? hair : skin;     // hair on top (y grows up)
                }
            void Set(int x, int y, Color32 c) { if (x >= 0 && x < d && y >= 0 && y < d) px[y * d + x] = c; }
            Set(4, 7, dark); Set(8, 7, dark);                     // two eyes
            Set(5, 4, dark); Set(6, 4, dark); Set(7, 4, dark);    // mouth
            tex.SetPixels32(px); tex.Apply();
            _headSprite = Sprite.Create(tex, new Rect(0, 0, d, d), new Vector2(0.5f, 0.5f), Tuning.PixelsPerUnit);
            return _headSprite;
        }

        public void KillBySpecial(Actor source) { _killedBySpecial = true; TakeDamage(9999f, source); }

        public override bool TakeDamage(float amount, Actor source)
        {
            if (_state != State.Dead && Alive) Anim.Play("hurt", false, restart: true);
            return base.TakeDamage(amount, source);
        }

        protected override void OnDeath(Actor source)
        {
            _state = State.Dead;
            if (_wiggle != null) { Destroy(_wiggle); _wiggle = null; }
            Anim.Play("death", false, restart: true);
            Vfx.DeathBurst(WorldX, Z);
            Sfx.Play("knockdown_thud");
            if (!_killedBySpecial && Def.Loot != LootTier.None)
            {
                var kind = LootTable.Roll(Def.Loot);
                if (kind.HasValue) Pickup.SpawnWeapon(kind.Value, WorldX, Z);
            }
            Destroy(gameObject, 1.0f);
        }
    }
}
