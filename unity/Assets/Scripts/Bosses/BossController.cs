using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Abstract base for every boss encounter (BOSSES.md §1–§5). An <see cref="Actor"/>
    /// subclass that owns the shared boss contract:
    ///   * a big, phase-segmented HP/objective readout (HUD reads <see cref="Hp"/>/
    ///     <see cref="MaxHp"/>, <see cref="PhaseCount"/>, <see cref="CurrentPhase"/>);
    ///   * a simple telegraph-then-strike attack scheduler over Unity coroutines
    ///     (every attack is telegraphed and fairly dodgeable, BOSSES.md §1 "no cheap
    ///     frustration");
    ///   * a large blob shadow (<see cref="Shadow"/> LargeTier) and a ~2× render scale
    ///     (BOSSES.md §1 "full bosses ~2× size");
    ///   * the boss cue music on spawn (AUDIO.md §3 / assets/audio/music/boss_cues);
    ///   * <see cref="IStaggerable"/> — bosses have permanent super-armor, so stagger is
    ///     a no-op (BOSSES.md §1 "No boss can be swept or knocked down");
    ///   * <see cref="ISpecialKillable"/> plus the low-HP execute rule — a player special
    ///     only ends a boss at <b>≤10% HP</b>, and only for the 5 pure HP-depletion
    ///     bosses; above 10% (or on any objective/proxy boss) the special is negated /
    ///     dodged (BOSSES.md §1, TUNING.md §7). <see cref="CanBeExecuted"/> is the HUD
    ///     prompt hook.
    ///
    /// Art gap: no bespoke boss atlases exist yet (BOSSES.md §6 lists all 7 as un-arted),
    /// so bosses reuse the shipped <c>enemy_regular</c> atlas, tinted per boss, at boss
    /// scale — replace by pointing <see cref="InitBoss"/> at a real atlas when it lands.
    /// </summary>
    public abstract class BossController : Actor, IStaggerable, ISpecialKillable
    {
        // ---- Identity ---------------------------------------------------------
        public string BossId { get; protected set; }
        public string DisplayName { get; protected set; }
        public string BossCue { get; protected set; }   // music stem under boss_cues (may be null)

        // ---- Classification (BOSSES.md §1) ------------------------------------
        /// <summary>True = a pure HP-depletion boss, the only kind the ≤10% special can execute.
        /// Objective/proxy bosses (Colossus, Tank, Helicopter, Monkey Boss, Phil) set this false.</summary>
        public bool IsHpDepletion { get; protected set; } = true;

        /// <summary>True if plain weapons/combos chip a real HP bar. Objective/proxy bosses win
        /// through their own objective API instead, so they absorb contact damage as a no-op.</summary>
        protected virtual bool TakesContactDamage => IsHpDepletion;

        // ---- Phases (fractions of MaxHp, DESCENDING, e.g. {0.66f, 0.33f}) -----
        protected float[] PhaseThresholds = System.Array.Empty<float>();
        /// <summary>Number of HP-bar segments / phase bands (thresholds + 1). HUD reads this.</summary>
        public int PhaseCount => PhaseThresholds.Length + 1;
        /// <summary>Current 1-based phase (escalates as HP drops past each threshold).</summary>
        public int CurrentPhase { get; private set; } = 1;

        // ---- Execute rule (BOSSES.md §1: "≤10% boundary is inclusive") --------
        public const float ExecuteFraction = 0.10f;
        // A hit at/above this magnitude is unmistakably a special / instakill blast
        // (sniper, giant-shotgun, werewolf, vaporize all deal 9999) rather than chip
        // damage — so it is routed through the execute gate, never applied raw.
        protected const float SpecialLethalThreshold = 1000f;

        /// <summary>HUD/flow hook: the ≤10% execute prompt is live (a special now ends the boss).
        /// Always false for objective/proxy bosses (they have no execute, BOSSES.md §1).</summary>
        public bool CanBeExecuted => Alive && IsHpDepletion && Hp <= MaxHp * ExecuteFraction;

        // ---- Attack scheduler -------------------------------------------------
        /// <summary>Countdown to the next scheduled action; concrete bosses read/refill it.</summary>
        protected float AttackTimer;
        /// <summary>True while a telegraph→strike routine is mid-flight (blocks re-scheduling).</summary>
        protected bool Busy { get; private set; }

        /// <summary>Pinned reposition/walk speed (wu/s); set per boss in <see cref="InitBoss"/>.</summary>
        protected float MoveSpeed = 4.5f;
        /// <summary>Render multiplier over the depth-scaled base (~2× for a full boss).</summary>
        protected float SizeScale = 2.0f;

        private bool _cueStarted;

        // ---- Setup ------------------------------------------------------------

        /// <summary>
        /// Shared boss bring-up: team/HP/position, the (tinted, placeholder) atlas, the
        /// large shadow, and the boss cue. Concrete bosses call this from their own
        /// <c>Init(x,z)</c> then set <see cref="PhaseThresholds"/> / classification.
        /// </summary>
        protected void InitBoss(string id, string display, string cue, float maxHp,
                                float x, float z, Color tint, float moveSpeed, float sizeScale = 2.0f)
        {
            BossId = id;
            DisplayName = display;
            BossCue = cue;
            Team = Team.Enemy;
            Hp = MaxHp = maxHp;
            MoveSpeed = moveSpeed;
            SizeScale = sizeScale;
            WorldX = x;
            Z = Mathf.Clamp(z, 0f, Tuning.ZBandDepth);

            if (Sr == null) Sr = GetComponent<SpriteRenderer>();
            if (Anim == null) Anim = GetComponent<SpriteAnimator>();
            // Bespoke boss atlas if present (sprites/bosses/<id>, e.g. hand-built Phil) — shown
            // untinted; otherwise fall back to the tinted Regular placeholder (BOSSES.md §6).
            string bossDir = "sprites/bosses/" + id;
            if (SpriteLibrary.HasAtlas(bossDir, id))
            {
                Anim.Set = SpriteLibrary.Load(bossDir, id);
                if (Sr != null) Sr.color = Color.white;
            }
            else
            {
                Anim.Set = SpriteLibrary.Load("sprites/enemies/enemy_regular", "enemy_regular");
                if (Sr != null) Sr.color = tint;
            }
            Anim.Play("idle", true);

            Shadow.Attach(this, Shadow.LargeTier);

            if (!_cueStarted && !string.IsNullOrEmpty(cue)) { Music.PlayBoss(cue); _cueStarted = true; }

            OnBossInit();
        }

        /// <summary>Per-boss extra setup after the shared bring-up (optional).</summary>
        protected virtual void OnBossInit() { }

        // ---- Main loop --------------------------------------------------------

        private void Update()
        {
            if (!Alive) return;
            float dt = Time.deltaTime;

            UpdatePhase();

            var player = PlayerController.Instance;
            if (player != null && player.Alive)
                Facing = player.WorldX >= WorldX ? 1 : -1;

            if (!Busy) BossUpdate(dt, player);
        }

        /// <summary>Per-boss behaviour when not mid-attack (movement + attack scheduling).</summary>
        protected abstract void BossUpdate(float dt, PlayerController player);

        private void UpdatePhase()
        {
            float frac = MaxHp > 0f ? Hp / MaxHp : 0f;
            int p = 1;
            for (int i = 0; i < PhaseThresholds.Length; i++)
                if (frac <= PhaseThresholds[i]) p = i + 2;

            if (p != CurrentPhase)
            {
                int from = CurrentPhase;
                CurrentPhase = p;
                OnPhaseChanged(from, p);
            }
        }

        /// <summary>Phase-change flash + shake (BOSSES.md §3 "phase-change flash").</summary>
        protected virtual void OnPhaseChanged(int from, int to)
        {
            Vfx.FinisherFlash(WorldX, Z);
            CameraShake.Add(CameraShake.Medium);
            Sfx.Play("boss_phase_change"); // missing sfx no-ops (Sfx warns once)
            Debug.Log($"[Boss:{BossId}] Phase {from} -> {to} (HP {Hp:0}/{MaxHp:0}).");
        }

        // ---- Damage / execute gate (the boss contract) ------------------------

        public override bool TakeDamage(float amount, Actor source)
        {
            if (!Alive) return false;

            // A special / instakill blast is an EXECUTE ATTEMPT, never raw chip damage.
            if (amount >= SpecialLethalThreshold)
                return TryExecute(source);

            // Objective/proxy bosses have no chip-able HP bar — absorb and flash only.
            if (!TakesContactDamage) { FlashHurt(); return false; }

            FlashHurt();
            return base.TakeDamage(amount, source);
        }

        /// <summary>
        /// The one place a special can end a boss: only for HP-depletion bosses and only
        /// at ≤10% HP (inclusive). Otherwise it is negated — the sniper visibly dodges,
        /// the other specials whiff (BOSSES.md §1, TUNING.md §7).
        /// </summary>
        private bool TryExecute(Actor source)
        {
            if (IsHpDepletion && Hp <= MaxHp * ExecuteFraction)
            {
                Vfx.FinisherFlash(WorldX, Z);
                CameraShake.Add(CameraShake.Heavy);
                Sfx.Play("finisher_crunch");
                return base.TakeDamage(9999f, source); // -> OnDeath
            }
            // Negated. No damage, no drop; the boss dodges.
            Sfx.Play("sniper_dodge"); // missing sfx no-ops
            Debug.Log($"[Boss:{BossId}] Special NEGATED (HP {Hp:0}/{MaxHp:0}; execute needs ≤ {MaxHp * ExecuteFraction:0}).");
            return false;
        }

        /// <summary>Sniper ricochet / Vaporize path — routed through the same execute gate.</summary>
        public void KillBySpecial(Actor source) => TryExecute(source);

        /// <summary>
        /// Force the boss defeated, bypassing the execute/negation gate. Objective/proxy
        /// bosses (whose win condition is a completed objective, not a special or chip
        /// damage) and Phil's scripted finisher call this to die. Runs the shared death
        /// path; the director advances on <see cref="Actor.Alive"/> going false.
        /// </summary>
        protected void ForceDefeat(Actor source)
        {
            if (!Alive) return;
            Hp = 0f;
            Alive = false;
            OnDeath(source);
        }

        /// <summary>Super-armor: bosses cannot be staggered / knocked down (BOSSES.md §1).</summary>
        public void ApplyStagger(float seconds) { /* no-op by design */ }

        protected override void OnDeath(Actor source)
        {
            Anim.Play("death", false, restart: true);
            Vfx.DeathBurst(WorldX, Z);
            Vfx.FinisherFlash(WorldX, Z);
            CameraShake.Add(CameraShake.Heavy);
            Sfx.Play("knockdown_thud");
            Music.Stop(); // boss cue ends; the director/flow starts the next bed
            OnBossDefeated(source);
            Destroy(gameObject, 1.2f); // let death frames play (director watches !Alive first)
        }

        /// <summary>Per-boss defeat hook (stop adds, spawn reward, etc.).</summary>
        protected virtual void OnBossDefeated(Actor source) { }

        // ---- Boss render scale (over the depth-scaled base) -------------------

        protected override void LateUpdate()
        {
            base.LateUpdate(); // clamps Z, projects, applies depth scale to localScale
            if (SizeScale != 1f)
            {
                var s = transform.localScale;
                transform.localScale = new Vector3(s.x * SizeScale, s.y * SizeScale, 1f);
            }
        }

        // ---- Shared attack helpers (for concrete bosses) ----------------------

        /// <summary>Run a telegraph→strike routine, marking the boss Busy for its duration.</summary>
        protected void RunAttack(IEnumerator routine) => StartCoroutine(Wrap(routine));

        private IEnumerator Wrap(IEnumerator inner)
        {
            Busy = true;
            yield return StartCoroutine(inner);
            Busy = false;
        }

        /// <summary>Wait that bails immediately if the boss dies mid-telegraph.</summary>
        protected IEnumerator Telegraph(float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                if (!Alive) yield break;
                t += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>Deal <paramref name="dmg"/> to the player if within a radius of the boss center.</summary>
        protected void HitPlayerIfInRange(float radius, float dmg)
        {
            var p = PlayerController.Instance;
            if (p == null || !p.Alive) return;
            if (p.DistanceTo(this) <= radius) p.TakeDamage(dmg, this);
        }

        /// <summary>Deal <paramref name="dmg"/> to the player if on a given Z-row within an X range ahead.</summary>
        protected void HitPlayerOnRow(float rowZ, float zTol, float xRange, float dmg)
        {
            var p = PlayerController.Instance;
            if (p == null || !p.Alive) return;
            if (Mathf.Abs(p.Z - rowZ) > zTol) return;
            if (Mathf.Abs(p.WorldX - WorldX) > xRange) return;
            p.TakeDamage(dmg, this);
        }

        /// <summary>Walk toward the player on X (and ease onto their Z-row), keeping a spacing gap.</summary>
        protected void Reposition(Actor target, float dt, float keep, float speed)
        {
            if (target == null) { Anim.Play("idle", true); return; }
            float dx = target.WorldX - WorldX;
            if (Mathf.Abs(dx) > keep)
            {
                WorldX += Mathf.Sign(dx) * speed * dt;
                Anim.Play("walk", true);
            }
            else Anim.Play("idle", true);

            float dz = target.Z - Z;
            Z += Mathf.Clamp(dz, -speed * dt, speed * dt);
        }

        private void FlashHurt()
        {
            if (Alive && (Anim == null || Anim.CurrentClip != "death"))
                Vfx.HitSpark(WorldX, Z);
        }

        // ---- Add economy helpers (BOSSES.md §1 arena add rule) ----------------

        /// <summary>The nearest live arena add (a non-boss enemy), or null. Used by grab/toss bosses.</summary>
        protected Actor NearestAdd()
        {
            Actor best = null;
            float bestD = float.MaxValue;
            foreach (var a in Actor.All)
            {
                if (!a.Alive || a.Team != Team.Enemy || a == this) continue;
                if (a is BossController) continue;
                float d = DistanceTo(a);
                if (d < bestD) { bestD = d; best = a; }
            }
            return best;
        }

        /// <summary>Count live arena adds (non-boss enemies) for the 2-cap.</summary>
        protected static int CountAdds()
        {
            int n = 0;
            foreach (var a in Actor.All)
                if (a.Alive && a.Team == Team.Enemy && a is not BossController) n++;
            return n;
        }

        /// <summary>Spawn one arena add at the boss's back edge (respects the caller's own cap).</summary>
        protected Actor SpawnAdd(EnemyArchetype archetype)
        {
            float side = Random.value < 0.5f ? -1f : 1f;
            float x = WorldX + side * 3f;
            float z = Mathf.Clamp(Tuning.ZBandDepth - 0.5f, 0f, Tuning.ZBandDepth);
            return StageEnemyFactory.Spawn(archetype, x, z);
        }
    }
}
