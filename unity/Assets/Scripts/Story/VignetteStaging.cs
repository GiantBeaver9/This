using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The reusable "show don't tell" staging rig for the per-stage teaching
    /// vignettes (VIGNETTES.md). Where <see cref="VignettePlayer"/> shows a text
    /// panel between stages, this ACTS the set-piece out on the field: it puppeteers
    /// throwaway demonstrator actors (the tutorial's <see cref="PassiveDummy"/>, the
    /// same pattern as <see cref="TutorialController"/>'s Demo) to physically perform
    /// the mechanic — a guard shoots a thug, a whip yanks a body, a sniper picks a
    /// leaper out of the air — juiced with <see cref="Vfx"/> sparks/bursts,
    /// <see cref="Sfx"/> one-shots and short knockbacks so it reads at a glance, under
    /// at most ONE short caption line (creator: "SHOW DON'T TELL — as fun as possible").
    ///
    /// It is a thin verb library: <see cref="VignetteActs"/> authors each stage's
    /// wordless pantomime by calling these primitives (Spawn / MoveTo / Punch / Shoot /
    /// Knockback / Kill / Boom / Teach …). The director drives it: <see cref="Begin"/>
    /// runs the acted sequence for a stage index and fires <paramref name="onDone"/>
    /// when the pantomime finishes; <see cref="Abort"/> tears it down early if the
    /// director's fallback timer wins. Either way <see cref="Cleanup"/> destroys every
    /// spawned actor so nothing loiters into the live wave.
    ///
    /// The component lives on the StageDirector object (added on demand) and reuses a
    /// single instance across stages. Nothing here freezes gameplay — the vignette
    /// fires before the first wave spawns, so the little playlet stages a few units
    /// ahead of the player and reads as a cutaway.
    /// </summary>
    public sealed class VignetteStaging : MonoBehaviour
    {
        /// <summary>True while an acted sequence is on the field.</summary>
        public bool IsActing { get; private set; }

        /// <summary>World-X the current playlet is centred on (a few units ahead of the player).</summary>
        public float Ax { get; private set; }

        /// <summary>The depth the actors stand on (mid-band so full bodies read).</summary>
        public const float GroundZ = 3.2f;

        private readonly List<Actor> _cast = new();
        private Coroutine _run;
        private Action _onDone;

        private string _caption = "";
        private float _captionAlpha;   // eased in/out
        private GUIStyle _captionStyle;
        private Texture2D _strip;

        // ---- Director entry points ------------------------------------------

        /// <summary>Run the acted sequence for <paramref name="stageIndex"/> (0-based);
        /// <paramref name="onDone"/> fires once when the pantomime completes naturally.</summary>
        public void Begin(int stageIndex, Action onDone)
        {
            Abort();                 // clear any prior playlet first
            _onDone = onDone;
            Ax = ResolveAnchorX();
            IsActing = true;
            _run = StartCoroutine(Run(stageIndex));
        }

        /// <summary>Stop the pantomime immediately and destroy the cast (no onDone).</summary>
        public void Abort()
        {
            if (_run != null) { StopCoroutine(_run); _run = null; }
            _onDone = null;
            Cleanup();
            IsActing = false;
        }

        private IEnumerator Run(int stageIndex)
        {
            IEnumerator act = null;
            try { act = VignetteActs.ForStage(this, stageIndex); }
            catch (Exception e) { Debug.LogError($"[VignetteStaging] Act build threw for stage {stageIndex}: {e}"); }

            if (act != null) yield return StartCoroutine(act);

            // Let the last beat breathe, then strike the set.
            yield return Wait(0.35f);
            Cleanup();
            IsActing = false;
            _run = null;

            var cb = _onDone; _onDone = null;
            try { cb?.Invoke(); }
            catch (Exception e) { Debug.LogError($"[VignetteStaging] onDone threw: {e}"); }
        }

        // ---- Actor verbs (called by VignetteActs) ---------------------------

        /// <summary>Spawn a demonstrator at (Ax + offsetX, z) facing <paramref name="facing"/>.
        /// Defaults to the stick enemy; pass a dir/actor (e.g. "sprites/characters/zebra")
        /// for a bespoke look, falling back to the stick body if that atlas isn't on disk.</summary>
        public PassiveDummy Spawn(float offsetX, float z, int facing, string spriteDir = null, string spriteActor = null)
        {
            var go = new GameObject("vignette_actor");
            go.transform.SetParent(transform);
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<SpriteAnimator>();
            var d = go.AddComponent<PassiveDummy>();
            d.Init(Ax + offsetX, z, facing, spriteDir, spriteActor);
            _cast.Add(d);
            return d;
        }

        /// <summary>Silhouette scale for big demonstrators (Heavy/Boss read large — TUNING §4).</summary>
        public void Scale(Actor a, float mult) { if (a != null) a.ScaleMult = mult; }

        public void Face(Actor a, int facing) { if (a != null) a.Facing = facing; }

        /// <summary>Play a clip on an actor (no-op if it's been destroyed / killed off).</summary>
        public void PlayClip(Actor a, string clip, bool loop = false)
        {
            if (a != null && a.Alive && a.Anim != null) a.Anim.Play(clip, loop, restart: true);
        }

        /// <summary>Walk an actor to (Ax + offsetX, z) in real time, facing the way it moves.</summary>
        public IEnumerator MoveTo(Actor a, float offsetX, float z, float speed = 6f)
        {
            if (a == null) yield break;
            float tx = Ax + offsetX;
            PlayClip(a, "walk", true);
            while (a != null && (Mathf.Abs(a.WorldX - tx) > 0.04f || Mathf.Abs(a.Z - z) > 0.04f))
            {
                float step = speed * Time.deltaTime;
                if (a.WorldX < tx - 0.001f) a.Facing = 1; else if (a.WorldX > tx + 0.001f) a.Facing = -1;
                a.WorldX = Mathf.MoveTowards(a.WorldX, tx, step);
                a.Z = Mathf.MoveTowards(a.Z, z, step);
                yield return null;
            }
            if (a != null && a.Alive) PlayClip(a, "idle", true);
        }

        /// <summary>Melee an actor: attacker faces the victim, throws the attack clip, spark + sfx;
        /// kills (drops) the victim when <paramref name="kill"/>, else just flinches it.</summary>
        public void Punch(Actor attacker, Actor victim, bool kill = true, string sfx = "punch_1")
        {
            if (attacker != null && victim != null)
                attacker.Facing = victim.WorldX >= attacker.WorldX ? 1 : -1;
            PlayClip(attacker, "attack_side");
            Sfx.Play(sfx);
            if (victim == null) return;
            if (kill) Kill(victim); else Hurt(victim);
        }

        /// <summary>Fire a gun from <paramref name="shooter"/> at <paramref name="victim"/>:
        /// muzzle flash + shot sfx at the barrel, hit spark on the target, optional kill.</summary>
        public void Shoot(Actor shooter, Actor victim, bool kill = true, string sfx = "pistol")
        {
            if (shooter != null && victim != null)
                shooter.Facing = victim.WorldX >= shooter.WorldX ? 1 : -1;
            PlayClip(shooter, "attack_side");
            if (shooter != null)
            {
                Vfx.MuzzleFlash(shooter.WorldX, shooter.Z, shooter.Facing);
                Sfx.PlayAt(sfx, shooter.WorldX);
            }
            if (victim == null) return;
            Vfx.HitSpark(victim.WorldX, victim.Z);
            if (kill) Kill(victim); else Hurt(victim);
        }

        /// <summary>An overhead toss / lob tell (head-grenade, dime flip): attack clip + airy sfx.</summary>
        public void Toss(Actor a, string sfx = "head_throw")
        {
            PlayClip(a, "attack_side");
            Sfx.Play(sfx);
        }

        /// <summary>Fire a gun in a direction with no on-field target (e.g. akimbo at the player):
        /// face the way, attack clip, muzzle flash + shot sfx.</summary>
        public void Fire(Actor shooter, int dir, string sfx = "pistol")
        {
            if (shooter == null) return;
            shooter.Facing = dir >= 0 ? 1 : -1;
            PlayClip(shooter, "attack_side");
            Vfx.MuzzleFlash(shooter.WorldX, shooter.Z, shooter.Facing);
            Sfx.PlayAt(sfx, shooter.WorldX);
        }

        /// <summary>Drop an actor for good (death anim + burst + thud, via PassiveDummy).</summary>
        public void Kill(Actor a)
        {
            if (a != null && a.Alive) a.TakeDamage(a.MaxHp + 1f, null);
        }

        /// <summary>Flinch an actor without dropping it (stays puppeteerable afterwards).</summary>
        public void Hurt(Actor a)
        {
            if (a != null && a.Alive)
            {
                PlayClip(a, "hurt");
                Vfx.HitSpark(a.WorldX, a.Z);
                Sfx.Play("hit_spark");
            }
        }

        /// <summary>Shove an actor sideways over <paramref name="time"/>s — cheap knockback juice.</summary>
        public IEnumerator Knockback(Actor a, int dir, float dist = 1.4f, float time = 0.18f)
        {
            if (a == null) yield break;
            float from = a.WorldX, to = a.WorldX + dir * dist, t = 0f;
            while (a != null && t < time)
            {
                t += Time.deltaTime;
                a.WorldX = Mathf.Lerp(from, to, Mathf.Clamp01(t / time));
                yield return null;
            }
        }

        /// <summary>Teleport blink: vanish (poof), reposition, reappear (poof) — Ninja/CC reads.</summary>
        public IEnumerator Blink(Actor a, float offsetX, float z)
        {
            if (a == null) yield break;
            Poof(a.WorldX - Ax, a.Z);
            if (a.Sr != null) a.Sr.enabled = false;
            Sfx.Play("dash_whoosh");
            yield return Wait(0.12f);
            a.WorldX = Ax + offsetX; a.Z = z;
            if (a.Sr != null) a.Sr.enabled = true;
            Poof(offsetX, z);
        }

        // ---- One-shot juice --------------------------------------------------

        public void Spark(float offsetX, float z) => Vfx.HitSpark(Ax + offsetX, z);
        public void Poof(float offsetX, float z) => Vfx.JumpPuff(Ax + offsetX, z);
        public void Dust(float offsetX, float z) => Vfx.DashDust(Ax + offsetX, z);

        /// <summary>Big explosive read (plane hit, gatling kill, trolley smash).</summary>
        public void Boom(float offsetX, float z, string sfx = "boom")
        {
            Vfx.DeathBurst(Ax + offsetX, z);
            Sfx.PlayAt(sfx, Ax + offsetX);
        }

        /// <summary>Rapid muzzle train for a barrage read (gatling): N flashes + one barrage sfx.</summary>
        public IEnumerator Barrage(Actor from, float toOffsetX, float z, int shots = 6, float gap = 0.06f)
        {
            Sfx.Play("gatling_barrage");
            for (int i = 0; i < shots; i++)
            {
                if (from != null) Vfx.MuzzleFlash(from.WorldX, from.Z, from.Facing);
                Vfx.HitSpark(Ax + toOffsetX, z);
                yield return Wait(gap);
            }
        }

        // ---- Audio & caption -------------------------------------------------

        /// <summary>Fire the area stinger / cue authored on beat 0 of a script id (reuses the bank).</summary>
        public void Stinger(string vignetteId)
        {
            var v = VignetteScripts.Catalog.Get(vignetteId);
            if (v == null || v.BeatCount == 0) return;
            var b = v.Beats[0];
            if (string.IsNullOrEmpty(b.Cue)) return;
            switch (b.CueBank)
            {
                case VignetteCueBank.Stinger:    Music.Stinger(b.Cue); break;
                case VignetteCueBank.StageMusic: Music.PlayStage(b.Cue); break;
                case VignetteCueBank.Ambient:    Music.PlayAmbient(b.Cue); break;
                case VignetteCueBank.Sfx:        Sfx.Play(b.Cue); break;
                case VignetteCueBank.None:       break;
            }
        }

        /// <summary>Show the ONE short teaching line for a script id (its beat-0 takeaway —
        /// the last non-empty line). Keeps captions to a single glanceable line by design.</summary>
        public void Teach(string vignetteId)
        {
            var v = VignetteScripts.Catalog.Get(vignetteId);
            if (v == null || v.BeatCount == 0) { Caption(""); return; }
            var lines = v.Beats[0].Lines;
            string pick = "";
            for (int i = lines.Length - 1; i >= 0; i--)
                if (!string.IsNullOrWhiteSpace(lines[i])) { pick = lines[i].Trim(); break; }
            Caption(pick);
        }

        /// <summary>Set the single bottom caption line (empty clears it).</summary>
        public void Caption(string text) => _caption = text ?? "";

        public void ClearCaption() => _caption = "";

        public static WaitForSeconds Wait(float seconds) => new WaitForSeconds(seconds);

        // ---- Teardown --------------------------------------------------------

        private void Cleanup()
        {
            foreach (var a in _cast)
                if (a != null) Destroy(a.gameObject);
            _cast.Clear();
            _caption = "";
        }

        private void OnDisable() => Cleanup();

        private static float ResolveAnchorX()
        {
            // Stage the playlet a few units ahead of the player, on-screen inside the
            // camera's locked head-of-lane window, so it reads as a framed cutaway.
            float px = PlayerController.Instance != null ? PlayerController.Instance.WorldX : 0f;
            return px + 4f;
        }

        // ---- Caption overlay (IMGUI, Hud/tutorial 360px design height) --------

        private void OnGUI()
        {
            // Ease the caption strip toward visible while a line is set.
            float target = (IsActing && !string.IsNullOrEmpty(_caption)) ? 1f : 0f;
            _captionAlpha = Mathf.MoveTowards(_captionAlpha, target, Time.unscaledDeltaTime * 4f);
            if (_captionAlpha <= 0.001f) return;

            EnsureStyles();

            GUI.depth = -90; // above the Hud, below the VignettePlayer panel (-1000)
            float scale = Screen.height / 360f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            float w = Screen.width / scale;

            // A slim dim strip behind the line for readability.
            var prev = GUI.color;
            GUI.color = new Color(0.04f, 0.04f, 0.06f, 0.66f * _captionAlpha);
            GUI.DrawTexture(new Rect(0, 300, w, 40), _strip);

            GUI.color = new Color(1f, 1f, 1f, 0.96f * _captionAlpha);
            GUI.Label(new Rect(0, 308, w, 26), _caption, _captionStyle);
            GUI.color = prev;
        }

        private void EnsureStyles()
        {
            if (_captionStyle != null) return;
            _strip = Texture2D.whiteTexture;
            _captionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
            };
        }
    }
}
