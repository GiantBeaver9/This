using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The paced, interactive first-minute tutorial — the creator's design:
    /// "SHOW, DON'T TELL — as fun as possible." A scripted ZEBRA demonstrator ACTS
    /// each mechanic out with puppeted thugs, then the game FORCES the player to copy
    /// it before the sequence advances (every step is gated). On-screen words stay
    /// terse — a big pulsing arrow plus 2-4 words — the acting carries the lesson.
    ///
    /// Gated steps, in order (zebra demos → player must perform → advance):
    ///   1. PUNCH (directional): zebra punches forward then turns and punches back;
    ///      player must land a → hit then a ← hit on two dummies.
    ///   2. EXECUTE / FINISHER + vulnerability: the zebra knocks a thug down and STOMPS
    ///      it to death — and a second thug plainly CLOCKS the zebra mid-stomp (hurt clip,
    ///      hit-spark on the zebra, recoil, hurt sfx) so "finishers kill but leave you open"
    ///      reads unmistakably. Then the player must land a finisher on a small cluster.
    ///   3. DASH-PUSH: the zebra dashes into a clustered crowd and shoves them back; the
    ///      player must dash once (Shift / double-tap) to plow a cluster of their own.
    ///   4. SPECIAL (the triumphant final beat): a horde closes in and surrounds the player,
    ///      the meter is filled, TIME FREEZES, and a big "UNLEASH YOUR SPECIAL — press Q"
    ///      prompt forces the shot; firing it eviscerates the whole horde.
    ///
    /// A contextual "press E to fire" weapon prompt is separate from the gated sequence —
    /// see <see cref="ShowWeaponPrompt"/> / <see cref="WeaponPromptOverlay"/>.
    ///
    /// ROBUSTNESS: every gate has an unscaled per-step timeout that auto-advances if the
    /// player is stuck, so a real playthrough that never acts (or a headless smoke that
    /// drives no input) can never hardlock. Time.timeScale is ALWAYS handed back to 1f on
    /// completion, teardown, or any early exit — the frozen showcase can never stick.
    ///
    /// GameFlow owns the lifecycle: it <see cref="Run(Action)"/>s this between the intro and
    /// the stage-1 opener and only starts the wave spawner on completion. The component lives
    /// on the run's Systems object, so a return-to-menu teardown disposes it and its dummies.
    /// </summary>
    public sealed class TutorialController : MonoBehaviour
    {
        /// <summary>Fires once when the tutorial finishes (in addition to the Run callback).</summary>
        public event Action OnComplete;

        // ---- Tunables (kept together so the whole tutorial is easy to re-time) --------
        private const float StepTimeout = 15f;         // per-gate auto-advance (never hardlock)
        private const float FreezeTimeout = 15f;       // paused-showcase auto-fire (unscaled)
        private const float DemoZ = 1.8f;              // depth the acted demos play out on
        private const int HordeCount = 10;             // ring of thugs for the special showcase

        private Action _onDone;
        private bool _running;

        // Practice dummies for the punch step (kept as fields so Finish can sweep them).
        private PassiveDummy _right, _left;

        // Player action flags, set by the PlayerController hooks (reset before each gate).
        private PlayerController _p;
        private bool _sawDash, _sawFinisher, _sawSpecial;

        // Arrow overlay state.
        private bool _showArrow;
        private string _arrow = "→";   // → / ←
        private string _caption = "";

        // Big dramatic prompt (the frozen special showcase).
        private bool _bigPrompt;
        private string _bigTitle = "", _bigSub = "";

        private GUIStyle _arrowStyle, _captionStyle, _bigTitleStyle, _bigSubStyle;

        /// <summary>Run the tutorial; <paramref name="onDone"/> fires once when it completes.</summary>
        public void Run(Action onDone)
        {
            if (_running) { onDone?.Invoke(); return; }
            _running = true;
            _onDone = onDone;
            Subscribe();
            StartCoroutine(Sequence());
        }

        /// <summary>
        /// Contextual weapon prompt (separate from the gated sequence): pops a brief,
        /// self-dismissing "press E to fire" caption. Static + self-contained so it works
        /// even after this component has finished and destroyed itself — the orchestrator
        /// can call it on a Stage-1 weapon pickup (or subscribe to
        /// <see cref="PlayerController.WeaponEquipped"/> and forward to here).
        /// </summary>
        public static void ShowWeaponPrompt(string text = "press E to fire", float seconds = 2.5f)
            => WeaponPromptOverlay.Show(text, seconds);

        // ---- Master sequence ---------------------------------------------------------
        private IEnumerator Sequence()
        {
            yield return StartCoroutine(DemoPunch());
            yield return StartCoroutine(StepPunch());

            yield return StartCoroutine(DemoFinisher());
            yield return StartCoroutine(StepFinisher());

            yield return StartCoroutine(DemoDash());
            yield return StartCoroutine(StepDash());

            yield return StartCoroutine(StepSpecial());   // demo is folded into the showcase

            _showArrow = false;
            Finish();
        }

        // =====================================================================
        // STEP 1 — PUNCH (directional)
        // =====================================================================
        private IEnumerator DemoPunch()
        {
            var demo = SpawnDummy(-4.0f, DemoZ, 1, "sprites/characters/zebra_mascot", "zebra_mascot");
            var front = SpawnDummy(-2.8f, DemoZ, -1);      // to its right  (→)
            var back = SpawnDummy(-5.2f, DemoZ, 1);        // to its left   (←)
            yield return Wait(0.5f);

            // Punch forward (→).
            demo.Facing = 1;
            SetArrow("→", "attack forward");
            PlayIf(demo, "attack_side");
            Sfx.Play("punch_1");
            yield return Wait(0.15f);
            if (front != null) { front.TakeDamage(front.MaxHp + 1f, demo); Vfx.HitSpark(front.WorldX, front.Z); }
            yield return Wait(0.75f);

            // Turn and punch back (←).
            demo.Facing = -1;
            SetArrow("←", "and behind you");
            PlayIf(demo, "attack_side");
            Sfx.Play("punch_2");
            yield return Wait(0.15f);
            if (back != null) { back.TakeDamage(back.MaxHp + 1f, demo); Vfx.HitSpark(back.WorldX, back.Z); }
            yield return Wait(0.75f);

            _showArrow = false;
            DestroyDummy(demo); DestroyDummy(front); DestroyDummy(back);
            yield return Wait(0.3f);
        }

        private IEnumerator StepPunch()
        {
            PlayerPos(out float px, out float pz);
            _right = SpawnDummy(px + 1.2f, pz, -1);   // just inside fist reach, faces the player
            _left = SpawnDummy(px - 1.2f, pz, 1);

            SetArrow("→", "punch forward");
            yield return Gate(() => _right == null || _right.WasHit);
            Sfx.Play("confirm");

            SetArrow("←", "punch back");
            yield return Gate(() => _left == null || _left.WasHit);
            Sfx.Play("confirm");

            _showArrow = false;
            DestroyDummy(_right); DestroyDummy(_left); _right = _left = null;
            yield return Wait(0.25f);
        }

        // =====================================================================
        // STEP 2 — EXECUTE / FINISHER + "it leaves you open"
        // =====================================================================
        private IEnumerator DemoFinisher()
        {
            var zebra = SpawnDummy(-4.0f, DemoZ, 1, "sprites/characters/zebra_mascot", "zebra_mascot");
            var victim = SpawnDummy(-2.9f, DemoZ, -1);      // thug the zebra will floor + execute
            var attacker = SpawnDummy(-5.4f, DemoZ, 1);     // second thug, waiting BEHIND the zebra
            yield return Wait(0.5f);

            // 1) Knock the victim DOWN — with a HORSE MULE-KICK (creator: the zebra's knockdown is a
            //    kick like a horse). It faces AWAY so the back-kick lashes into the victim behind it.
            SetArrow("↓", "knock them down");
            zebra.Facing = -1;
            PlayIf(zebra, "kick");
            Sfx.Play("knockdown_thud");
            yield return Wait(0.15f);
            if (victim != null)
            {
                PlayIf(victim, "hurt");
                Vfx.HitSpark(victim.WorldX, victim.Z);
                Sfx.Play("enemy_stagger");
            }
            yield return Wait(0.55f);

            // 2) STOMP to execute — and get CLOCKED mid-stomp (the whole point of this step).
            SetArrow("↓", "then FINISH");
            zebra.Facing = 1;                    // turn back to face the downed victim for the stomp
            PlayIf(zebra, "attack_down");
            Sfx.Play("finisher_crunch");
            Vfx.FinisherFlash(victim != null ? victim.WorldX : zebra.WorldX + 1f, DemoZ);
            CameraShake.Add(CameraShake.Heavy);
            yield return Wait(0.12f);
            if (victim != null) { victim.TakeDamage(victim.MaxHp + 1f, zebra); Vfx.DeathBurst(victim.WorldX, victim.Z); }

            // The exposed zebra gets plainly punched by the second thug — unmistakable.
            if (attacker != null)
            {
                attacker.Facing = 1;
                PlayIf(attacker, "attack_side");
                Sfx.Play("punch_2");
            }
            yield return Wait(0.10f);
            if (zebra != null)
            {
                PlayIf(zebra, "hurt");
                Vfx.HitSpark(zebra.WorldX, zebra.Z);          // spark ON the zebra
                Sfx.Play("hurt_grunt");
                CameraShake.Add(CameraShake.Medium);
                yield return StartCoroutine(Recoil(zebra, +1, 0.5f, 6f)); // shoved away from the hit
            }
            SetArrow("↓", "...but you're OPEN!");
            yield return Wait(0.9f);

            _showArrow = false;
            DestroyDummy(zebra); DestroyDummy(victim); DestroyDummy(attacker);
            yield return Wait(0.3f);
        }

        private IEnumerator StepFinisher()
        {
            PlayerPos(out float px, out float pz);
            // A small cluster so the string (punch → punch → sweep → finish) always has bodies.
            var cluster = new List<PassiveDummy>
            {
                SpawnDummy(px + 1.2f, pz, -1),
                SpawnDummy(px + 1.7f, pz + 0.4f, -1),
                SpawnDummy(px + 1.5f, pz - 0.4f, -1),
            };
            // High HP so P1→P2→sweep can't kill them before the FINISH lands (default dummies
            // are 12 HP and die to two punches, which broke the string and made the finisher
            // feel unimplemented). They survive the whole string; the finisher tap ends them.
            foreach (var d in cluster) if (d != null) { d.MaxHp = 500f; d.Hp = 500f; }

            _sawFinisher = false;
            SetArrow("→", "combo, then FINISH");
            yield return Gate(() => _sawFinisher);
            Sfx.Play("confirm");

            _showArrow = false;
            DestroyAll(cluster);
            yield return Wait(0.25f);
        }

        // =====================================================================
        // STEP 3 — DASH-PUSH
        // =====================================================================
        private IEnumerator DemoDash()
        {
            var zebra = SpawnDummy(-5.6f, DemoZ, 1, "sprites/characters/zebra_mascot", "zebra_mascot");
            var crowd = new List<PassiveDummy>();
            for (int i = 0; i < 3; i++)
                crowd.Add(SpawnDummy(-3.6f + i * 0.5f, DemoZ + (i - 1) * 0.35f, -1));
            yield return Wait(0.5f);

            SetArrow("»", "dash to shove");   // »
            Sfx.Play("dash_whoosh");
            Vfx.DashDust(zebra.WorldX, zebra.Z);

            const float target = -2.6f;
            while (zebra != null && zebra.WorldX < target)
            {
                float step = 14f * Time.deltaTime;   // ~dash speed
                zebra.WorldX += step;
                foreach (var c in crowd)
                    if (c != null && Mathf.Abs(c.WorldX - zebra.WorldX) < 0.75f)
                    {
                        c.WorldX += step * 1.4f;     // shoved along the dash
                        PlayIf(c, "hurt");
                    }
                yield return null;
            }
            foreach (var c in crowd) if (c != null) Vfx.HitSpark(c.WorldX, c.Z);
            Sfx.Play("enemy_stagger");
            CameraShake.Add(CameraShake.Medium);
            yield return Wait(0.7f);

            _showArrow = false;
            DestroyDummy(zebra); DestroyAll(crowd);
            yield return Wait(0.3f);
        }

        private IEnumerator StepDash()
        {
            PlayerPos(out float px, out float pz);
            var cluster = new List<PassiveDummy>();
            for (int i = 0; i < 3; i++)
                cluster.Add(SpawnDummy(px + 1.4f + i * 0.45f, pz + (i - 1) * 0.35f, -1));

            _sawDash = false;
            SetArrow("»", "DASH! (double-tap a direction)");
            yield return Gate(() => _sawDash);
            // Sell the plow on the cluster even if the dash didn't quite reach them.
            foreach (var c in cluster) if (c != null) Vfx.HitSpark(c.WorldX, c.Z);
            Sfx.Play("confirm");

            _showArrow = false;
            DestroyAll(cluster);
            yield return Wait(0.25f);
        }

        // =====================================================================
        // STEP 4 — SPECIAL: paused forced-unleash showcase (the final beat)
        // =====================================================================
        private IEnumerator StepSpecial()
        {
            var p = PlayerController.Instance;
            PlayerPos(out float px, out float pz);

            // 1) A horde CLOSES IN and surrounds the player (ring, staggered in depth).
            var horde = new List<PassiveDummy>();
            for (int i = 0; i < HordeCount; i++)
            {
                float ang = (Mathf.PI * 2f) * i / HordeCount;
                float sx = px + Mathf.Cos(ang) * 1.9f;
                float sz = pz + Mathf.Sin(ang) * 1.1f;
                int face = sx >= px ? -1 : 1;                 // look at the player
                var d = SpawnDummy(sx, sz, face);
                // 1 HP each so EVERY character's special reliably KILLS the ring — not just
                // knocks it down (Aaron's shotgun does low damage; this guarantees the wipe
                // reads as an evisceration, and Gabe's werewolf swipe insta-kills too).
                if (d != null) { d.MaxHp = 1f; d.Hp = 1f; }
                horde.Add(d);
            }
            SetArrow("◆", "surrounded!");                // ◆
            yield return Wait(0.6f);

            // 2) Grant the meter, FREEZE time, throw the big dramatic prompt.
            if (p != null) { p.Meter.Award(Tuning.MeterMax); p.SpecialLocked = true; }
            _showArrow = false;
            ShowBigPrompt("UNLEASH YOUR SPECIAL", "press Q");
            Sfx.Play("armed_ready_chime");
            Time.timeScale = 0f;

            // 3) Wait — on UNSCALED time — for Q, with a hard unscaled timeout.
            float t0 = Time.unscaledTime;
            while (Time.unscaledTime - t0 < FreezeTimeout)
            {
                if (Input.GetKeyDown(KeyCode.Q)) break;
                yield return null;   // coroutines still tick at timeScale 0
            }

            // 4) Restore time FIRST (always to 1f — never fight the special's own slow-mo),
            //    unlock, THEN force-fire so the payload runs at normal speed.
            Time.timeScale = 1f;
            HideBigPrompt();
            if (p != null)
            {
                p.SpecialLocked = false;
                _sawSpecial = false;
                p.FireSpecialNow();      // eviscerates the horde (auto-fires on timeout too)
                CameraShake.Add(CameraShake.Heavy);
            }

            // 5) Guarantee the wipe reads as the special's doing: cascade-kill any straggler.
            yield return new WaitForSecondsRealtime(0.35f);
            foreach (var d in horde)
            {
                if (d == null || !d.Alive) continue;
                d.TakeDamage(d.MaxHp + 1f, p);
                Vfx.HitSpark(d.WorldX, d.Z);
                yield return new WaitForSecondsRealtime(0.05f);
            }
            yield return new WaitForSecondsRealtime(0.6f);
            DestroyAll(horde);
        }

        // =====================================================================
        // Gating + helpers
        // =====================================================================

        /// <summary>Yield until <paramref name="done"/> is true OR the per-step timeout
        /// elapses (unscaled, so a hit-stop / freeze can never stall the safety clock).</summary>
        private IEnumerator Gate(Func<bool> done, float timeout = StepTimeout)
        {
            float t = 0f;
            while (!done() && t < timeout)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        /// <summary>Smoothly shove an actor along <paramref name="dir"/> over <paramref name="dist"/>
        /// world units (unscaled so it plays even through a hit-stop).</summary>
        private static IEnumerator Recoil(Actor a, int dir, float dist, float speed)
        {
            float moved = 0f;
            while (a != null && moved < dist)
            {
                float step = speed * Time.unscaledDeltaTime;
                a.WorldX += dir * step;
                moved += step;
                yield return null;
            }
        }

        private static WaitForSeconds Wait(float s) => new WaitForSeconds(s);

        private static void PlayIf(PassiveDummy d, string clip)
        {
            if (d != null && d.Anim != null) d.Anim.Play(clip, false, restart: true);
        }

        private static void PlayerPos(out float x, out float z)
        {
            var p = PlayerController.Instance;
            x = p != null ? p.WorldX : 0f;
            z = p != null ? p.Z : 2.5f;
        }

        private PassiveDummy SpawnDummy(float x, float z, int facing, string spriteDir = null, string spriteActor = null)
        {
            var go = new GameObject("tutorial_dummy");
            go.transform.SetParent(transform);   // under Systems -> swept by GameFlow teardown
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<SpriteAnimator>();
            var d = go.AddComponent<PassiveDummy>();
            d.Init(x, z, facing, spriteDir, spriteActor);
            return d;
        }

        private static void DestroyDummy(PassiveDummy d) { if (d != null) Destroy(d.gameObject); }

        private static void DestroyAll(List<PassiveDummy> list)
        {
            if (list == null) return;
            foreach (var d in list) DestroyDummy(d);
            list.Clear();
        }

        private void SetArrow(string arrow, string caption)
        {
            _arrow = arrow;
            _caption = caption;
            _showArrow = true;
        }

        private void ShowBigPrompt(string title, string sub)
        {
            _bigTitle = title;
            _bigSub = sub;
            _bigPrompt = true;
            _showArrow = false;
        }

        private void HideBigPrompt() => _bigPrompt = false;

        // ---- Player-action detection hooks ----------------------------------
        private void Subscribe()
        {
            _p = PlayerController.Instance;
            if (_p == null) return;
            _p.Dashed += OnDashed;
            _p.FinisherLanded += OnFinisher;
            _p.SpecialFired += OnSpecial;
        }

        private void Unsubscribe()
        {
            if (_p == null) return;
            _p.Dashed -= OnDashed;
            _p.FinisherLanded -= OnFinisher;
            _p.SpecialFired -= OnSpecial;
            _p = null;
        }

        private void OnDashed() => _sawDash = true;
        private void OnFinisher() => _sawFinisher = true;
        private void OnSpecial(int tier) => _sawSpecial = true;

        private void Finish()
        {
            if (!_running) return;
            _running = false;

            RestoreSafeState();

            // Practice dummies shouldn't loiter into the live stage.
            DestroyDummy(_right); DestroyDummy(_left);
            _right = _left = null;

            var cb = _onDone;
            _onDone = null;
            try { OnComplete?.Invoke(); } catch (Exception e) { Debug.LogError($"[Tutorial] OnComplete threw: {e}"); }
            try { cb?.Invoke(); } catch (Exception e) { Debug.LogError($"[Tutorial] onDone threw: {e}"); }

            Destroy(this);   // one-shot: remove the component, leave Systems intact
        }

        /// <summary>Never leave the game frozen or the player's special locked, whatever happens.</summary>
        private void RestoreSafeState()
        {
            if (Time.timeScale != 1f) Time.timeScale = 1f;
            if (_p != null) _p.SpecialLocked = false;
        }

        private void OnDestroy()
        {
            RestoreSafeState();   // teardown mid-freeze can never stick
            Unsubscribe();
        }

        // ---- Arrow / big-prompt overlay (IMGUI, Hud-style 360px design height) ----
        private void OnGUI()
        {
            if (!_showArrow && !_bigPrompt) return;
            EnsureStyles();

            GUI.depth = -100;   // above the Hud, below the vignette panel
            float scale = Screen.height / 360f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            float w = Screen.width / scale;

            float pulse = 0.55f + 0.45f * Mathf.PingPong(Time.unscaledTime * 2.2f, 1f);

            if (_bigPrompt)
            {
                // Darken the frozen field so the call-to-action pops.
                GUI.color = new Color(0f, 0f, 0f, 0.55f);
                GUI.DrawTexture(new Rect(0, 0, w, 360), Texture2D.whiteTexture);
                GUI.color = new Color(1f, 0.9f, 0.35f, pulse);
                GUI.Label(new Rect(0, 120, w, 60), _bigTitle, _bigTitleStyle);
                GUI.color = new Color(1f, 1f, 1f, 0.95f);
                GUI.Label(new Rect(0, 188, w, 40), _bigSub, _bigSubStyle);
                GUI.color = Color.white;
                return;
            }

            GUI.color = new Color(1f, 0.95f, 0.55f, pulse);
            GUI.Label(new Rect(0, 96, w, 130), _arrow, _arrowStyle);
            GUI.color = new Color(1f, 1f, 1f, 0.95f);
            GUI.Label(new Rect(0, 232, w, 28), _caption, _captionStyle);
            GUI.color = Color.white;
        }

        private void EnsureStyles()
        {
            if (_arrowStyle != null) return;
            _arrowStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 96, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter, wordWrap = false,
            };
            _captionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter, wordWrap = false,
            };
            _bigTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 40, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter, wordWrap = false,
            };
            _bigSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter, wordWrap = false,
            };
        }
    }

    /// <summary>
    /// Standalone, self-dismissing "press E to fire" prompt used for the contextual
    /// weapon lesson. It lives on its OWN GameObject so it survives the tutorial
    /// component finishing/destroying itself — the orchestrator pops it when the player
    /// picks up a weapon during Stage 1 (via <see cref="TutorialController.ShowWeaponPrompt"/>).
    /// Runs on unscaled time so a hit-stop can't stall its lifetime.
    /// </summary>
    public sealed class WeaponPromptOverlay : MonoBehaviour
    {
        private float _life;
        private string _text = "press E to fire";
        private GUIStyle _style;

        public static void Show(string text, float seconds)
        {
            var go = new GameObject("weapon_prompt");
            var o = go.AddComponent<WeaponPromptOverlay>();
            o._text = text;
            o._life = Mathf.Max(0.5f, seconds);
        }

        private void Update()
        {
            _life -= Time.unscaledDeltaTime;
            if (_life <= 0f) Destroy(gameObject);
        }

        private void OnGUI()
        {
            _style ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 20, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter, wordWrap = false,
            };

            GUI.depth = -90;
            float scale = Screen.height / 360f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            float w = Screen.width / scale;

            float pulse = 0.55f + 0.45f * Mathf.PingPong(Time.unscaledTime * 2.4f, 1f);
            GUI.color = new Color(0.7f, 1f, 0.8f, pulse);
            GUI.Label(new Rect(0, 288, w, 28), _text, _style);
            GUI.color = Color.white;
        }
    }
}
