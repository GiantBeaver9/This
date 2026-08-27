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

        private const float SharpenSeconds = 10f;  // vulnerable window (creator: "sharpens for 10s")

        private bool _sharpening;                // true = vulnerable window open
        private float _windowDamage;             // damage dealt in the current window
        private bool _cycleRunning;
        private bool _onRight;                    // which of his two sides he's currently on
        private float _leftX, _rightX, _backZ;    // his two teleport spots + the draw row

        /// <summary>HUD hook: the finisher (pencil-laser) input is live this window.</summary>
        public bool CanBeFinished { get; private set; }

        public void Init(float x, float z)
        {
            InitBoss("phil", "Phil", "phil_realized", 500f, x, z,
                     new Color(0.55f, 0.5f, 0.6f), moveSpeed: 5.0f, sizeScale: 1.9f);
            IsHpDepletion = false;               // finisher-only; specials whiff (BOSSES.md §5.1)
            PhaseThresholds = new[] { 0.75f, 0.50f, 0.25f };
            // Two sides of the rooftop he teleports between (kept inside the locked finale view).
            _leftX = x - 7f; _rightX = x + 3f;
            _backZ = Tuning.ZBandDepth - 0.5f;
            _onRight = true; WorldX = _rightX; Z = _backZ;
        }

        protected override void BossUpdate(float dt, PlayerController player)
        {
            if (!_cycleRunning) RunAttack(DrawSharpenCycle(player));
        }

        /// <summary>Phil's whole loop (creator): DRAW a pod on one side → TELEPORT to his other side →
        /// DRAW another pod → SHARPEN the pencil for 10 s, wide open. He's untouchable except during the
        /// sharpen window (capped at 125 HP so a burst build can't skip the fight).</summary>
        private IEnumerator DrawSharpenCycle(PlayerController p)
        {
            _cycleRunning = true;
            _sharpening = false;
            CanBeFinished = false;

            yield return DrawPod();               // pod on this side
            yield return TeleportToOtherSide();   // vanish → reappear on the far side
            yield return DrawPod();               // pod on the far side
            if (!Alive) { _cycleRunning = false; yield break; }

            // SHARPEN — vulnerable for 10 s (ends early only if you burst the 125-HP window cap).
            _sharpening = true;
            _windowDamage = 0f;
            Vfx.FinisherFlash(WorldX, Z);
            Sfx.Play("phil_sharpen");
            if (Anim != null) Anim.Play("sharpen", true, restart: true);
            float wT = 0f;
            while (wT < SharpenSeconds && Alive && _windowDamage < WindowCap)
            {
                wT += Time.deltaTime;
                yield return null;
            }
            _sharpening = false;
            if (Alive && Anim != null) Anim.Play("idle", true);
            _cycleRunning = false;
        }

        /// <summary>Scribble a pod onto the paper on his current side — it draws out one enemy every
        /// half second for ~3 s (creator: "he spawns 1 enemy every half second").</summary>
        private IEnumerator DrawPod()
        {
            Sfx.Play("phil_draw");
            if (Anim != null) Anim.Play("attack_side", false, restart: true);   // scribbling pose
            float drawTime = 3f, spawnT = 0f;
            for (float t = 0f; t < drawTime && Alive; t += Time.deltaTime)
            {
                spawnT -= Time.deltaTime;
                if (spawnT <= 0f)
                {
                    if (CountAdds() < 10) { SpawnAdd(EnemyArchetype.Regular); Vfx.FinisherFlash(WorldX, _backZ); Sfx.Play("phil_draw"); }
                    spawnT = 0.5f;   // one enemy per half second
                }
                yield return null;
            }
        }

        /// <summary>He has two sides — blink out and reappear on the other one.</summary>
        private IEnumerator TeleportToOtherSide()
        {
            Sfx.Play("phil_teleport");            // synth fallback
            Vfx.DeathBurst(WorldX, Z, 1.3f);      // vanish poof
            yield return Telegraph(0.15f);
            _onRight = !_onRight;
            WorldX = _onRight ? _rightX : _leftX;
            Z = _backZ;
            Facing = _onRight ? -1 : 1;
            Vfx.FinisherFlash(WorldX, Z);
            yield return Telegraph(0.2f);
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
