using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The one-shot SFX bank (AUDIO.md §4 "95 core one-shots", §6 mix budget of
    /// 24 concurrent voices). Static and self-initializing: the first call spins up
    /// a hidden, scene-persistent GameObject holding a pool of AudioSources, and
    /// scans <c>assets/audio/sfx</c> recursively once to build a name→path map
    /// (filename without extension = the key, e.g. "punch_1", "hit_spark",
    /// "armed_ready_chime").
    ///
    /// The creator records the real WAVs later; until a matching WAV exists on disk
    /// every name is served by a PROCEDURAL fallback synthesized in code (see the
    /// Synth region below), so nothing is ever silent and an unknown name still
    /// makes a sensible sound. Resolution order per name: real WAV via
    /// <see cref="WavLoader"/> if present, else a code-synthesized clip. Both are
    /// cached by name, so synthesis happens at most once per sound. Per AUDIO.md §4
    /// each shot is pitch-randomized ±2 semitones at playback for cheap variety, so
    /// repeats of the same clip never sound identical. Audio never throws into
    /// gameplay: any failure caches a silent no-op.
    ///
    /// Design note (creator guidance): the loudest, most distinct sounds are the
    /// melee impacts, and a HIT must sound clearly different from a MISS. So every
    /// impact ("punch_*", "*_hit", "hit_spark", "*_thud", "*smash", "finisher_*")
    /// carries a low body THUMP plus a sharp smack transient, while every whiff
    /// ("swing_whoosh", "dash_whoosh", "sweep", "*_throw"...) is airy band-passed
    /// noise with NO low end — connect vs. miss is unmistakable by ear.
    /// </summary>
    public static class Sfx
    {
        private const int VoiceCount = 24;             // AUDIO.md §6 concurrent-voice budget
        private const float PitchJitterSemitones = 2f; // AUDIO.md §4 ±2 semitones
        private const int SampleRate = 44100;          // AUDIO.md §1 SFX = 44.1 kHz mono

        private static bool _init;
        private static AudioSource[] _sources;
        private static int _next;
        private static readonly Dictionary<string, string> _paths = new();      // name -> absolute path
        private static readonly Dictionary<string, AudioClip> _clips = new();    // name -> clip (null = known-silent)

        // ---- Public API ------------------------------------------------------

        /// <summary>Play a one-shot by name (e.g. "punch_1"). Unknown names fall back to a synth blip, never silence.</summary>
        public static void Play(string name, float volume = 1f)
        {
            PlayInternal(name, volume, 0f);
        }

        /// <summary>
        /// Play a one-shot positioned by world-X, panned in stereo by its on-screen
        /// position (AUDIO.md keeps the mix 2D — this is a light pan, not 3D audio).
        /// </summary>
        public static void PlayAt(string name, float worldX, float volume = 1f)
        {
            float pan = 0f;
            var cam = Camera.main;
            if (cam != null)
            {
                // Orthographic scene: viewport-x 0..1 -> pan -1..1.
                Vector3 vp = cam.WorldToViewportPoint(new Vector3(worldX, cam.transform.position.y, 0f));
                pan = Mathf.Clamp((vp.x - 0.5f) * 2f, -1f, 1f);
            }
            PlayInternal(name, volume, pan);
        }

        // ---- Internals -------------------------------------------------------

        private static void PlayInternal(string name, float volume, float pan)
        {
            if (string.IsNullOrEmpty(name)) return;
            EnsureInit();

            var clip = Resolve(name);
            if (clip == null) return; // known-silent (synth failed); never throws

            var src = NextFreeSource();
            src.clip = clip;
            src.volume = Mathf.Clamp01(volume);
            src.panStereo = pan;
            src.pitch = RandomPitch();
            src.Play();
        }

        /// <summary>
        /// Name → clip. A real WAV on disk wins; otherwise a procedural fallback is
        /// synthesized. Result (including a null on failure) is cached forever, so
        /// this is allocation-free on every play after the first for a given name.
        /// </summary>
        private static AudioClip Resolve(string name)
        {
            if (_clips.TryGetValue(name, out var cached)) return cached; // includes cached null

            AudioClip clip = null;
            if (_paths.TryGetValue(name, out var path))
                clip = WavLoader.Load(path); // real recorded asset, if the creator shipped one

            if (clip == null)
                clip = Synthesize(name);     // code fallback — keyed by name, never silent when it can help it

            _clips[name] = clip;
            return clip;
        }

        private static AudioSource NextFreeSource()
        {
            // Prefer an idle voice; otherwise steal the oldest (round-robin) one.
            for (int i = 0; i < _sources.Length; i++)
            {
                var s = _sources[(_next + i) % _sources.Length];
                if (!s.isPlaying)
                {
                    _next = (_next + i + 1) % _sources.Length;
                    return s;
                }
            }
            var steal = _sources[_next];
            _next = (_next + 1) % _sources.Length;
            return steal;
        }

        private static float RandomPitch()
        {
            float semis = Random.Range(-PitchJitterSemitones, PitchJitterSemitones);
            return Mathf.Pow(2f, semis / 12f);
        }

        private static void EnsureInit()
        {
            if (_init) return;
            _init = true;

            var go = new GameObject("~ThisL.Sfx");
            go.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(go);

            _sources = new AudioSource[VoiceCount];
            for (int i = 0; i < VoiceCount; i++)
            {
                var s = go.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.loop = false;
                s.spatialBlend = 0f; // 2D
                _sources[i] = s;
            }

            BuildIndex();
        }

        /// <summary>Scan assets/audio/sfx once; map each filename stem to its absolute path.</summary>
        private static void BuildIndex()
        {
            try
            {
                string dir = Path.Combine(SpriteLibrary.AssetsRoot, "audio", "sfx");
                if (!Directory.Exists(dir))
                {
                    // Not an error: the bank is creator-produced and may not exist yet.
                    // Everything falls back to synthesis (see Resolve / Synthesize).
                    Debug.Log($"[Sfx] No WAV bank at {dir}; using procedural fallback for all sounds.");
                    return;
                }
                foreach (var file in Directory.GetFiles(dir, "*.wav", SearchOption.AllDirectories))
                {
                    string key = Path.GetFileNameWithoutExtension(file);
                    if (!_paths.ContainsKey(key)) _paths[key] = file; // first wins on the rare dup
                }
                Debug.Log($"[Sfx] Indexed {_paths.Count} SFX from {dir} (missing names synthesized).");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Sfx] Failed to index SFX: {e.Message}");
            }
        }

        // ================= Procedural synth (fallback bank) ===================
        //
        // Everything below bakes a mono float buffer and wraps it in an AudioClip
        // exactly like WavLoader does (AudioClip.Create + SetData). One clip per
        // name, cached; per-play variety comes from the ±2-semitone pitch jitter
        // above. Names are routed to a sound "family" by keyword, so both the exact
        // names the code uses today and any close variant (or a brand-new vignette
        // cue) land on a fitting sound. Truly unknown names get a neutral blip.
        //
        // The hit/miss contract: impacts have a LOW THUMP + sharp transient; whiffs
        // are airy band-passed noise with NO low end.

        private static AudioClip Synthesize(string name)
        {
            try
            {
                var rng = new System.Random(StableHash(name));
                float[] buf = BuildBuffer(name.ToLowerInvariant(), rng);
                if (buf == null || buf.Length == 0) return null;

                // Prevent hard clipping without squashing intended loudness differences.
                float peak = 0f;
                for (int i = 0; i < buf.Length; i++) { float a = buf[i] < 0 ? -buf[i] : buf[i]; if (a > peak) peak = a; }
                float scale = peak > 0.98f ? 0.98f / peak : 1f;
                for (int i = 0; i < buf.Length; i++)
                {
                    float v = buf[i] * scale;
                    buf[i] = v > 1f ? 1f : (v < -1f ? -1f : v);
                }

                var clip = AudioClip.Create("~synth_" + name, buf.Length, 1, SampleRate, false);
                clip.SetData(buf, 0);
                return clip;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Sfx] Procedural synth failed for '{name}': {e.Message}");
                return null;
            }
        }

        /// <summary>Route a (lowercased) name to its sound family. Order = most specific first.</summary>
        private static float[] BuildBuffer(string n, System.Random rng)
        {
            // Melee/impact connect — meaty (low thump + smack). Checked first so "*_hit" wins.
            if (Contains(n, "punch", "hit", "smash", "thud", "crunch", "stagger", "impact", "finisher"))
                return Impact(n, rng);

            // Mechanical racks/breaks (before Boom so "shotgun_cock" isn't a boom).
            if (Contains(n, "cock", "reload", "rack") || n.Contains("break") || n.Contains("puff"))
                return Mech(n, rng);

            // Big explosive body (before Gun so "shotgun_blast" isn't a pistol crack).
            if (Contains(n, "boom", "blast", "explos", "whomp", "burst", "shotgun", "phase"))
                return Boom(n, rng);

            // Rapid gun train.
            if (n.Contains("barrage") && !n.Contains("incoming"))
                return Barrage(rng);

            // Single sharp gunshot ("sniper" alone is NOT a gun — sniper_dodge is a whiff).
            if (Contains(n, "pistol", "revolver", "sniper_shot", "gatling_gun", "gunshot")
                || (n.Contains("shot") && !n.Contains("shotgun")))
                return Gun(n, rng);

            // Whiff / swoosh — airy, NO low thump. This is the "miss" against Impact's "hit".
            if (Contains(n, "whoosh", "swing", "dash", "sweep", "throw", "toss", "dodge", "swish"))
                return Whiff(n, rng);

            if (n.Contains("jump")) return Jump(rng);
            if (Contains(n, "land", "footstep", "step")) return Land(rng);
            if (Contains(n, "hurt", "grunt", "death", "pain")) return Hurt(n, rng);

            // Positive chimes (before Pickup so "heal_pickup_chime" is a chime, not a blip).
            if (Contains(n, "chime", "heal")) return Chime(n, rng);
            if (Contains(n, "pickup", "coin")) return Pickup(n, rng);

            if (Contains(n, "confirm", "cancel", "menu", "tick", "meter")) return Ui(n, rng);
            if (Contains(n, "alarm", "incoming", "warning")) return Alarm(rng);
            if (n.Contains("horn")) return Horn(rng);
            if (n.Contains("windup")) return Windup(rng);

            var special = Special(n, rng);
            if (special != null) return special;

            return Generic(rng); // neutral, audible fallback for anything unrecognized
        }

        // ---- Families --------------------------------------------------------

        private static float[] Impact(string n, System.Random rng)
        {
            bool heavy = Contains(n, "sweep", "finisher", "knockdown", "smash", "heavy", "crunch", "ground");
            bool light = Contains(n, "spark", "tick");
            bool big2 = n.Contains("punch_2");

            float dur = heavy ? 0.30f : light ? 0.12f : 0.18f;
            var buf = Alloc(dur + 0.03f);

            // Low body thump — the presence of this is what makes it read as a CONNECT.
            float f0 = heavy ? 150f : light ? 110f : big2 ? 115f : 130f;
            float thAmp = heavy ? 0.95f : light ? 0.35f : big2 ? 0.90f : 0.75f;
            float thDecay = heavy ? 12f : light ? 26f : 20f;
            AddOsc(buf, 0f, dur, f0, f0 * 0.42f, thAmp, 0f, thDecay, 0, rng);

            // Sharp smack transient (band-passed noise, very fast decay).
            AddNoise(buf, 0f, heavy ? 0.10f : 0.06f, light ? 0.60f : 0.50f, 0f, heavy ? 45f : 60f, 3500f, 1200f, 300f, rng);

            if (light) AddNoise(buf, 0f, 0.03f, 0.50f, 0f, 120f, 9000f, 6000f, 2500f, rng); // hit-spark sparkle
            if (n.Contains("stagger")) AddOsc(buf, 0f, 0.12f, 180f, 120f, 0.30f, 0.005f, 18f, 1, rng); // small grunt
            return buf;
        }

        private static float[] Whiff(string n, System.Random rng)
        {
            bool heavy = Contains(n, "heavy", "sweep", "dash");
            float dur = heavy ? 0.30f : 0.20f;
            var buf = Alloc(dur + 0.03f);

            // Amplitude swells then fades (attack then decay) while the band opens:
            // a pure "air" swish, deliberately WITHOUT any low thump so a miss can
            // never be mistaken for a hit.
            AddNoise(buf, 0f, dur, heavy ? 0.50f : 0.40f, dur * 0.35f, 5.5f / dur,
                     heavy ? 400f : 700f, heavy ? 2400f : 3800f, heavy ? 150f : 280f, rng);
            return buf;
        }

        private static float[] Gun(string n, System.Random rng)
        {
            bool sniper = n.Contains("sniper");
            bool big = n.Contains("revolver");
            float dur = sniper ? 0.24f : 0.16f;
            var buf = Alloc(dur + 0.03f);

            AddNoise(buf, 0f, 0.006f, 1.0f, 0f, 700f, 9000f, 9000f, 1800f, rng);              // sharp click
            AddNoise(buf, 0.001f, dur, big ? 0.75f : 0.60f, 0f, sniper ? 22f : 32f, 5000f, 700f, 400f, rng); // tail
            AddOsc(buf, 0f, 0.09f, 130f, 55f, big ? 0.45f : 0.32f, 0f, 30f, 0, rng);          // small body
            if (sniper) AddNoise(buf, 0.02f, dur, 0.40f, 0f, 14f, 3000f, 1500f, 900f, rng);   // rifle crack
            return buf;
        }

        private static float[] Boom(string n, System.Random rng)
        {
            bool giant = Contains(n, "giant", "big");
            bool whomp = n.Contains("whomp");
            bool phase = n.Contains("phase");
            float dur = giant ? 0.65f : 0.50f;
            var buf = Alloc(dur + 0.30f);

            float start = 0f;
            if (phase) { AddOsc(buf, 0f, 0.25f, 200f, 900f, 0.40f, 0.01f, 4f, 2, rng); start = 0.18f; } // rising tell

            float f0 = whomp ? 220f : 90f, f1 = whomp ? 38f : 33f;
            AddOsc(buf, start, dur, f0, f1, 0.95f, 0.004f, giant ? 5f : 6.5f, 0, rng);
            AddOsc(buf, start, dur, f0 * 0.6f, f1 * 0.8f, 0.50f, 0.004f, giant ? 4.5f : 6f, 0, rng);
            AddNoise(buf, start, dur, 0.70f, 0.004f, giant ? 6f : 8f, 4000f, 250f, 55f, rng);
            return buf;
        }

        private static float[] Barrage(System.Random rng)
        {
            const int shots = 7;
            const float gap = 0.06f;
            var buf = Alloc(shots * gap + 0.15f);
            for (int i = 0; i < shots; i++)
            {
                float t = i * gap;
                AddNoise(buf, t, 0.005f, 0.90f, 0f, 700f, 9000f, 9000f, 1800f, rng);
                AddNoise(buf, t + 0.001f, 0.05f, 0.50f, 0f, 40f, 5000f, 700f, 400f, rng);
                AddOsc(buf, t, 0.05f, 120f, 55f, 0.30f, 0f, 34f, 0, rng);
            }
            return buf;
        }

        private static float[] Mech(string n, System.Random rng)
        {
            if (n.Contains("cock")) { var b = Alloc(0.24f); Clack(b, 0f, rng); Clack(b, 0.11f, rng); return b; } // chk-chk
            if (n.Contains("reload")) { var b = Alloc(0.14f); Clack(b, 0f, rng); return b; }

            // weapon_break_puff / sword_break
            var buf = Alloc(0.20f);
            if (n.Contains("sword")) // metallic ting
            {
                AddOsc(buf, 0f, 0.18f, 1200f, 1180f, 0.30f, 0f, 10f, 0, rng);
                AddOsc(buf, 0f, 0.18f, 1790f, 1770f, 0.20f, 0f, 11f, 0, rng);
            }
            AddNoise(buf, 0f, 0.16f, 0.40f, 0.004f, 16f, 2600f, 900f, 500f, rng); // puff
            AddOsc(buf, 0f, 0.10f, 300f, 140f, 0.25f, 0f, 18f, 0, rng);
            return buf;
        }

        private static void Clack(float[] buf, float t, System.Random rng)
        {
            AddNoise(buf, t, 0.008f, 0.70f, 0f, 500f, 7000f, 4000f, 2000f, rng);
            AddOsc(buf, t, 0.05f, 320f, 180f, 0.30f, 0f, 24f, 2, rng);
        }

        private static float[] Jump(System.Random rng)
        {
            var b = Alloc(0.15f);
            AddOsc(b, 0f, 0.12f, 300f, 760f, 0.45f, 0.004f, 13f, 1, rng); // upward blip
            AddNoise(b, 0f, 0.05f, 0.20f, 0f, 40f, 3000f, 1500f, 600f, rng);
            return b;
        }

        private static float[] Land(System.Random rng)
        {
            var b = Alloc(0.22f);
            AddOsc(b, 0f, 0.18f, 150f, 60f, 0.60f, 0.003f, 16f, 0, rng); // soft low thud
            AddNoise(b, 0f, 0.08f, 0.35f, 0.003f, 26f, 1800f, 700f, 300f, rng);
            return b;
        }

        private static float[] Hurt(string n, System.Random rng)
        {
            bool death = n.Contains("death");
            float dur = death ? 0.35f : 0.16f;
            var b = Alloc(dur + 0.03f);
            AddOsc(b, 0f, dur, death ? 220f : 270f, death ? 90f : 185f, 0.50f, 0.006f, death ? 5.5f : 10f, 1, rng); // vocal bend
            AddNoise(b, 0f, dur * 0.7f, 0.20f, 0.006f, death ? 6f : 12f, 2200f, 900f, 500f, rng);                    // breath
            return b;
        }

        private static float[] Pickup(string n, System.Random rng)
        {
            var b = Alloc(0.18f);
            if (n.Contains("coin")) // classic two-note jingle
            {
                AddOsc(b, 0f, 0.06f, 988f, 988f, 0.40f, 0.002f, 20f, 0, rng);
                AddOsc(b, 0.045f, 0.10f, 1319f, 1319f, 0.40f, 0.002f, 16f, 0, rng);
            }
            else // bright rising blip
            {
                AddOsc(b, 0f, 0.14f, 520f, 940f, 0.40f, 0.004f, 12f, 0, rng);
            }
            return b;
        }

        private static float[] Chime(string n, System.Random rng)
        {
            if (n.Contains("heal")) // soft major triad
            {
                var b = Alloc(0.55f);
                AddOsc(b, 0f, 0.50f, 523f, 523f, 0.32f, 0.02f, 4.5f, 0, rng);
                AddOsc(b, 0.02f, 0.50f, 659f, 659f, 0.26f, 0.02f, 4.5f, 0, rng);
                AddOsc(b, 0.04f, 0.50f, 784f, 784f, 0.22f, 0.02f, 4.5f, 0, rng);
                return b;
            }
            // armed_ready / checkpoint: bright ascending two-note
            var c = Alloc(0.35f);
            AddOsc(c, 0f, 0.14f, 784f, 784f, 0.34f, 0.004f, 10f, 1, rng);
            AddOsc(c, 0.09f, 0.22f, 1047f, 1047f, 0.34f, 0.004f, 8f, 1, rng);
            return c;
        }

        private static float[] Ui(string n, System.Random rng)
        {
            if (n.Contains("confirm")) // ascending
            {
                var b = Alloc(0.16f);
                AddOsc(b, 0f, 0.05f, 660f, 660f, 0.35f, 0.003f, 16f, 0, rng);
                AddOsc(b, 0.05f, 0.08f, 880f, 880f, 0.35f, 0.003f, 14f, 0, rng);
                return b;
            }
            if (n.Contains("cancel")) // descending + buzzy
            {
                var b = Alloc(0.16f);
                AddOsc(b, 0f, 0.05f, 440f, 440f, 0.32f, 0.003f, 16f, 2, rng);
                AddOsc(b, 0.05f, 0.09f, 320f, 300f, 0.32f, 0.003f, 13f, 2, rng);
                return b;
            }
            if (Contains(n, "tick", "meter")) // tiny high tick
            {
                var b = Alloc(0.05f);
                AddOsc(b, 0f, 0.035f, 1250f, 1250f, 0.28f, 0.001f, 40f, 0, rng);
                return b;
            }
            // menu_move: single soft blip
            var m = Alloc(0.06f);
            AddOsc(m, 0f, 0.045f, 540f, 560f, 0.28f, 0.002f, 26f, 0, rng);
            return m;
        }

        private static float[] Alarm(System.Random rng)
        {
            var b = Alloc(0.66f);
            for (int i = 0; i < 3; i++) // beep-boop x3
            {
                float t = i * 0.2f;
                AddOsc(b, t, 0.09f, 988f, 988f, 0.34f, 0.004f, 7f, 2, rng);
                AddOsc(b, t + 0.10f, 0.09f, 660f, 660f, 0.34f, 0.004f, 7f, 2, rng);
            }
            return b;
        }

        private static float[] Horn(System.Random rng)
        {
            var b = Alloc(0.60f);
            AddOsc(b, 0f, 0.55f, 440f, 440f, 0.32f, 0.02f, 1.2f, 2, rng); // dual-tone car horn
            AddOsc(b, 0f, 0.55f, 554f, 554f, 0.28f, 0.02f, 1.2f, 2, rng);
            return b;
        }

        private static float[] Windup(System.Random rng)
        {
            var b = Alloc(0.50f);
            AddOsc(b, 0f, 0.45f, 60f, 120f, 0.50f, 0.05f, 1.6f, 0, rng);        // rising rumble tell
            AddNoise(b, 0f, 0.45f, 0.18f, 0.10f, 2f, 300f, 900f, 60f, rng);
            return b;
        }

        private static float[] Special(string n, System.Random rng)
        {
            if (n.Contains("scope")) // sniper_scope_in: thin rising tone
            {
                var b = Alloc(0.32f);
                AddOsc(b, 0f, 0.30f, 600f, 1400f, 0.25f, 0.02f, 2.5f, 0, rng);
                return b;
            }
            if (n.Contains("timeslow")) // descending wobble
            {
                var b = Alloc(0.45f);
                AddOsc(b, 0f, 0.42f, 700f, 200f, 0.32f, 0.01f, 2f, 1, rng);
                return b;
            }
            if (Contains(n, "howl", "transform")) // werewolf: tonal glide + growl
            {
                var b = Alloc(0.60f);
                AddOsc(b, 0f, 0.55f, 200f, 520f, 0.35f, 0.03f, 1.4f, 1, rng);
                AddOsc(b, 0.25f, 0.35f, 520f, 300f, 0.30f, 0.01f, 3f, 1, rng);
                AddNoise(b, 0f, 0.55f, 0.25f, 0.05f, 1.6f, 500f, 2500f, 200f, rng);
                return b;
            }
            if (Contains(n, "draw", "scribble", "sharpen", "scrape", "sharp")) // pencil scratch / sharpen rasp
            {
                var b = Alloc(0.40f);
                for (int i = 0; i < 5; i++)
                    AddNoise(b, i * 0.07f, 0.05f, 0.40f, 0.005f, 22f, 1500f, 4500f, 800f, rng);
                return b;
            }
            return null;
        }

        private static float[] Generic(System.Random rng)
        {
            var b = Alloc(0.14f);
            AddOsc(b, 0f, 0.12f, 440f, 430f, 0.32f, 0.004f, 16f, 0, rng);
            AddNoise(b, 0f, 0.04f, 0.15f, 0f, 40f, 3000f, 1500f, 700f, rng);
            return b;
        }

        // ---- Synth primitives ------------------------------------------------

        private static float[] Alloc(float seconds) => new float[(int)(seconds * SampleRate) + 1];

        /// <summary>
        /// Add an oscillator with an exponential pitch glide (f0→f1) and an
        /// exp-decay envelope (optionally with a short linear attack).
        /// wave: 0 sine, 1 triangle, 2 soft square.
        /// </summary>
        private static void AddOsc(float[] buf, float start, float dur, float f0, float f1,
                                   float amp, float attack, float decay, int wave, System.Random rng)
        {
            int i0 = (int)(start * SampleRate);
            int n = (int)(dur * SampleRate);
            if (n <= 0) return;
            double dt = 1.0 / SampleRate;
            double phase = rng.NextDouble() * Mathf.PI * 2f;

            for (int k = 0; k < n; k++)
            {
                int idx = i0 + k;
                if (idx < 0) continue;
                if (idx >= buf.Length) break;

                float u = (float)k / n;
                float f = f0 * Mathf.Pow(f1 / f0, u);       // exponential glide
                phase += 2.0 * Mathf.PI * f * dt;

                float s;
                switch (wave)
                {
                    case 1:  s = 2f / Mathf.PI * Mathf.Asin(Mathf.Sin((float)phase)); break; // triangle
                    case 2:  s = Mathf.Sign(Mathf.Sin((float)phase)) * 0.8f; break;          // soft square
                    default: s = Mathf.Sin((float)phase); break;                              // sine
                }

                float t = (float)k / SampleRate;
                float env = Mathf.Exp(-decay * t);
                if (attack > 0f) env *= Mathf.Min(1f, t / attack);
                buf[idx] += s * amp * env;
            }
        }

        /// <summary>
        /// Add band-passed white noise: a one-pole low-pass whose cutoff glides
        /// lpStart→lpEnd, then a one-pole high-pass (hp) removes the low end
        /// (hp &lt;= 0 skips it). Envelope = exp decay with optional linear attack.
        /// </summary>
        private static void AddNoise(float[] buf, float start, float dur, float amp,
                                     float attack, float decay, float lpStart, float lpEnd, float hp,
                                     System.Random rng)
        {
            int i0 = (int)(start * SampleRate);
            int n = (int)(dur * SampleRate);
            if (n <= 0) return;
            float dt = 1f / SampleRate;
            float nyq = SampleRate * 0.45f;
            float lpY = 0f, hpY = 0f;

            for (int k = 0; k < n; k++)
            {
                int idx = i0 + k;
                if (idx < 0) continue;
                if (idx >= buf.Length) break;

                float u = (float)k / n;
                float x = (float)(rng.NextDouble() * 2.0 - 1.0);

                float fc = Mathf.Min(lpStart * Mathf.Pow(lpEnd / lpStart, u), nyq);
                float aLP = 1f - Mathf.Exp(-2f * Mathf.PI * fc * dt);
                lpY += aLP * (x - lpY);

                float outp = lpY;
                if (hp > 0f)
                {
                    float aHP = 1f - Mathf.Exp(-2f * Mathf.PI * hp * dt);
                    hpY += aHP * (lpY - hpY);
                    outp = lpY - hpY;
                }

                float t = (float)k / SampleRate;
                float env = Mathf.Exp(-decay * t);
                if (attack > 0f) env *= Mathf.Min(1f, t / attack);
                buf[idx] += outp * amp * env;
            }
        }

        private static bool Contains(string n, params string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
                if (n.Contains(keys[i])) return true;
            return false;
        }

        /// <summary>Process-stable FNV-1a hash so a given name always synthesizes the same clip.</summary>
        private static int StableHash(string s)
        {
            unchecked
            {
                int h = (int)2166136261;
                for (int i = 0; i < s.Length; i++) h = (h ^ s[i]) * 16777619;
                return h;
            }
        }
    }
}
