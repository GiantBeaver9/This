using System.Collections;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Phil — the FINAL boss (BOSSES.md §5.1, TUNING.md §7). FIRST-PASS. HP 500, gated
    /// behind sharpen windows; exempt from the &lt;2-min rule; NOT executable by any
    /// special (finisher-only). The artist-summoner cycle:
    ///   draw (invulnerable, sketches adds at the back edge) → runs out of lead →
    ///   sharpen (vulnerable 3–5 s, open and bleeding) → repeat.
    /// You can only damage him during a sharpen window, capped at 125 HP (25%) per window;
    /// the window whose damage takes his gated HP to ≤0 becomes the finisher window (the
    /// scripted pencil-laser). Specials never execute him (the sole exception, §5.1).
    ///   * Summon roster is cumulative by HP band (100–75% Regulars/pods; +minibosses at
    ///     75–50% / 50–25%; +Heavies at 25–0%). FIRST-PASS approximates with Regulars +
    ///     the placeholder miniboss/Heavy archetypes; the exact lead-economy draw-selection
    ///     and 8-add ceiling are simplified — see the gap in <c>_INTEGRATION.md</c>.
    ///   * The scripted pencil-laser finisher input and rooftop sway/fall-death are noted
    ///     gaps; FIRST-PASS downs him when gated HP reaches ≤0 in a window.
    /// </summary>
    public sealed class PhilBoss : BossController
    {
        private const float WindowCap = 125f;    // per-window damage cap = 25% (TUNING §7)

        private bool _sharpening;                // true = vulnerable window open
        private float _windowDamage;             // damage dealt in the current window
        private bool _cycleRunning;

        /// <summary>HUD hook: the finisher (pencil-laser) input is live this window.</summary>
        public bool CanBeFinished { get; private set; }

        public void Init(float x, float z)
        {
            InitBoss("phil", "Phil", "phil_realized", 500f, x, z,
                     new Color(0.55f, 0.5f, 0.6f), moveSpeed: 5.0f, sizeScale: 1.9f);
            IsHpDepletion = false;               // finisher-only; specials whiff (BOSSES.md §5.1)
            // Visual waypoints for the summon-roster escalation (not damage gates).
            PhaseThresholds = new[] { 0.75f, 0.50f, 0.25f };
        }

        protected override void BossUpdate(float dt, PlayerController player)
        {
            Anim.Play(_sharpening ? "hurt" : "idle", true); // hunched/bleeding while sharpening
            if (!_cycleRunning) RunAttack(DrawSharpenCycle(player));
        }

        private IEnumerator DrawSharpenCycle(PlayerController p)
        {
            _cycleRunning = true;

            // --- Draw phase: invulnerable, sketch adds until "out of lead" (FIRST-PASS ~4 s). ---
            _sharpening = false;
            CanBeFinished = false;
            Sfx.Play("phil_draw");
            float drawTime = 4f;
            float drawT = 0f, summonT = 0f;
            while (drawT < drawTime && Alive)
            {
                drawT += Time.deltaTime;
                summonT -= Time.deltaTime;
                if (summonT <= 0f && CountAdds() < 6)
                {
                    SpawnAdd(PickSummon());   // cumulative-by-band roster (FIRST-PASS)
                    summonT = 1.5f;           // one draw every 1.5 s (BOSSES.md §5.1)
                }
                yield return null;
            }
            if (!Alive) { _cycleRunning = false; yield break; }

            // --- Sharpen window: vulnerable 3–5 s, ends early at the 125 cap. ---
            _sharpening = true;
            _windowDamage = 0f;
            Vfx.FinisherFlash(WorldX, Z);
            Sfx.Play("phil_sharpen");
            if (Anim != null) Anim.Play("sharpen", true);   // Holy Sharpener loops through the window
            float wT = 0f;
            while (wT < 5f && Alive && _windowDamage < WindowCap)
            {
                wT += Time.deltaTime;
                yield return null;
            }
            _sharpening = false;
            if (Alive && Anim != null) Anim.Play("idle", true);
            _cycleRunning = false;
        }

        /// <summary>Cumulative summon roster by HP band (BOSSES.md §5.1), FIRST-PASS mapping.</summary>
        private EnemyArchetype PickSummon()
        {
            float frac = Hp / MaxHp;
            float r = Random.value;
            if (frac <= 0.25f && r < 0.25f) return EnemyArchetype.Heavy;
            if (frac <= 0.50f && r < 0.35f) return EnemyArchetype.ArmRipper;   // Pool-B reprise stand-in
            if (frac <= 0.75f && r < 0.35f) return EnemyArchetype.Snapper;     // Pool-A reprise stand-in
            return r < 0.3f ? EnemyArchetype.Swarmer : EnemyArchetype.Regular; // Regulars + Swarmer pods
        }

        public override bool TakeDamage(float amount, Actor source)
        {
            if (!Alive) return false;

            // Specials NEVER execute Phil (BOSSES.md §5.1) — negate the instakill blast.
            if (amount >= SpecialLethalThreshold)
            {
                Sfx.Play("sniper_dodge");
                Debug.Log("[Boss:phil] Special negated (finisher-only).");
                return false;
            }

            // Invulnerable while drawing; damageable only in a sharpen window.
            if (!_sharpening) { Vfx.HitSpark(WorldX, Z); return false; }

            float applied = Mathf.Min(amount, Mathf.Min(WindowCap - _windowDamage, Hp));
            if (applied <= 0f) return false;
            _windowDamage += applied;
            Hp = Mathf.Max(0f, Hp - applied);
            Vfx.HitSpark(WorldX, Z);

            if (Hp <= 0f)
            {
                // The window that takes gated HP to ≤0 IS the finisher window (pencil-laser).
                CanBeFinished = true;
                Sfx.Play("finisher_crunch");
                ForceDefeat(source);   // FIRST-PASS: scripted finisher; real input hook is a gap
                return true;
            }
            return false;
        }
    }
}
