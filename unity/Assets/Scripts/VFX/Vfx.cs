using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Static facade for the one-shot combat/movement effects described in
    /// <c>VFX.md</c> §2 (hit spark, dash dust, jump/land puff, finisher flash,
    /// red-pixel death burst, muzzle flash, air-punch gust). Each call spawns a
    /// short-lived, point-filtered, depth-scaled sprite at a world (x,z) via the
    /// <see cref="Playfield"/> projection and destroys it when the clip ends —
    /// per §1 "nothing stays": no pooling of the visual itself, the effect clears
    /// completely at the end of its animation.
    ///
    /// Self-initializing: the atlas is loaded lazily off disk from
    /// <c>&lt;repo&gt;/assets/sprites/vfx/vfx</c> (reusing
    /// <see cref="SpriteLibrary.AssetsRoot"/>, same File.ReadAllBytes +
    /// Texture2D.LoadImage + point-filter path). If a file is missing every clip
    /// degrades to a generated colored quad so nothing crashes.
    /// </summary>
    public static class Vfx
    {
        // Ground clips pivot on their bottom edge (feet line); impact/air clips
        // pivot on center and float up the body a little.
        private static readonly HashSet<string> GroundClips = new()
        {
            "dash_dust", "jump_puff", "land_puff",
        };

        // Roughly torso height (wu) for impact effects that read at contact point.
        private const float BodyMidY = 1.0f;

        // Fallback tint per clip when the atlas is unavailable (VFX.md palette cues).
        private static readonly Dictionary<string, Color32> FallbackTint = new()
        {
            { "hit_spark",      new Color32(255, 240, 170, 255) },
            { "finisher_flash", new Color32(255, 235, 90, 255) },
            { "death_burst",    new Color32(220, 40, 40, 255) },   // §5 flying red pixels
            { "dash_dust",      new Color32(200, 190, 170, 235) },
            { "jump_puff",      new Color32(225, 225, 225, 230) },
            { "land_puff",      new Color32(210, 210, 205, 235) },
            { "muzzle_flash",   new Color32(255, 190, 80, 255) },
            { "gust",           new Color32(235, 240, 245, 200) },
        };

        private static SpriteLibrary.ActorSprites _set;   // "vfx" atlas, clips keyed by name
        private static bool _loaded;

        // -- Public API -------------------------------------------------------

        /// <summary>Melee impact flash at the contact point (VFX.md §2, P0).</summary>
        public static void HitSpark(float x, float z) => Spawn("hit_spark", x, z, 0);

        /// <summary>Combo-finisher flash — stronger spark; caller adds the shake (§2/§3).</summary>
        public static void FinisherFlash(float x, float z) => Spawn("finisher_flash", x, z, 0);

        /// <summary>Comedic transient gore: a burst of flying red pixels that clears (§5).</summary>
        public static void DeathBurst(float x, float z) => Spawn("death_burst", x, z, 0);

        /// <summary>A death burst scaled up (execution head-pop wants a BIG one — creator).</summary>
        public static void DeathBurst(float x, float z, float scale) => Spawn("death_burst", x, z, 0, scale);

        /// <summary>Grounded-dash kick-up dust (§2, P0).</summary>
        public static void DashDust(float x, float z) => Spawn("dash_dust", x, z, 0);

        /// <summary>Small poof on jump (§2, P0).</summary>
        public static void JumpPuff(float x, float z) => Spawn("jump_puff", x, z, 0);

        /// <summary>Small poof on landing (§2, P0).</summary>
        public static void LandPuff(float x, float z) => Spawn("land_puff", x, z, 0);

        /// <summary>Gun muzzle flash; <paramref name="facing"/> flips it to point the barrel.</summary>
        public static void MuzzleFlash(float x, float z, int facing) => Spawn("muzzle_flash", x, z, facing);

        /// <summary>Air-punch gust — the reach-extender wind off the fist (§2, P0), per direction.</summary>
        public static void Gust(float x, float z, int facing) => Spawn("gust", x, z, facing);

        // -- Spawn ------------------------------------------------------------

        private static void Spawn(string clip, float x, float z, int facing) => Spawn(clip, x, z, facing, 1f);

        private static void Spawn(string clip, float x, float z, int facing, float scale)
        {
            EnsureLoaded();
            if (_set == null) return;

            var go = new GameObject($"vfx_{clip}");
            var sr = go.AddComponent<SpriteRenderer>();
            var anim = go.AddComponent<SpriteAnimator>();
            anim.Set = _set;
            anim.Fps = Tuning.AnimFps;

            // Project onto the band: feet position, depth-scaled, sorted for Z.
            Playfield.Place(go.transform, x, z, sr);
            if (scale != 1f) go.transform.localScale *= scale;         // Place set a depth scale; blow it up
            var p = go.transform.position;
            if (!GroundClips.Contains(clip)) p.y += BodyMidY;            // float impacts to torso
            // Nudge sideways so directional effects read off the fist/barrel.
            if (facing != 0) p.x += 0.35f * Mathf.Sign(facing);
            go.transform.position = p;

            // Draw just in front of the actor at this depth (still under projectiles,
            // which the combat/projectile layers keep on top — VFX.md §1).
            sr.sortingOrder = Playfield.SortingOrder(z) + 1;

            if (facing < 0) sr.flipX = true;                            // point directional clips

            anim.Play(clip, loop: false, restart: true);

            // Self-destruct when the clip finishes (with a hard safety cap).
            var life = go.AddComponent<VfxOneShot>();
            life.Init(anim);
        }

        // -- Loading ----------------------------------------------------------

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            try
            {
                string dir = Path.Combine(SpriteLibrary.AssetsRoot,
                    "sprites", "vfx", "vfx");
                string jsonPath = Path.Combine(dir, "vfx.json");
                string pngPath = Path.Combine(dir, "vfx_atlas.png");

                if (File.Exists(jsonPath) && File.Exists(pngPath))
                {
                    var tex = LoadTexture(pngPath);
                    var root = JsonUtility.FromJson<VfxAtlasRoot>(File.ReadAllText(jsonPath));
                    _set = Slice(tex, root);
                }
                else
                {
                    Debug.LogWarning($"[Vfx] Missing atlas at {dir}; using generated placeholders.");
                    _set = BuildFallbackSet();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Vfx] Failed to load atlas: {e.Message}");
                _set = BuildFallbackSet();
            }
        }

        private static SpriteLibrary.ActorSprites Slice(Texture2D tex, VfxAtlasRoot root)
        {
            var set = new SpriteLibrary.ActorSprites { Actor = "vfx", Texture = tex };
            if (root?.atlas?.frames == null) return BuildFallbackSet();

            int texH = tex.height;
            const string prefix = "vfx_";
            var perClip = new Dictionary<string, List<(string name, Sprite sprite)>>();

            foreach (var f in root.atlas.frames)
            {
                if (f.rect == null || f.rect.Length < 4 || string.IsNullOrEmpty(f.name)) continue;
                string name = f.name.EndsWith(".png") ? f.name.Substring(0, f.name.Length - 4) : f.name;
                if (name.StartsWith(prefix)) name = name.Substring(prefix.Length);
                int us = name.LastIndexOf('_');
                string clip = us > 0 ? name.Substring(0, us) : name;   // strip trailing _NN

                int x = f.rect[0], yTop = f.rect[1], w = f.rect[2], h = f.rect[3];
                int yBottom = texH - (yTop + h);                        // JSON top-left -> Unity bottom-left
                var pivot = GroundClips.Contains(clip) ? new Vector2(0.5f, 0f)
                                                       : new Vector2(0.5f, 0.5f);
                var sprite = Sprite.Create(
                    tex, new Rect(x, yBottom, w, h), pivot,
                    Tuning.PixelsPerUnit, 0, SpriteMeshType.FullRect);
                sprite.name = $"vfx_{name}";

                if (!perClip.TryGetValue(clip, out var list))
                    perClip[clip] = list = new List<(string, Sprite)>();
                list.Add((name, sprite));
            }

            foreach (var kv in perClip)
            {
                kv.Value.Sort((a, b) => string.CompareOrdinal(a.name, b.name)); // _00, _01, ...
                var frames = new Sprite[kv.Value.Count];
                for (int i = 0; i < frames.Length; i++) frames[i] = kv.Value[i].sprite;
                set.Clips[kv.Key] = frames;
            }
            return set;
        }

        /// <summary>Generated colored-quad clips so effects still play with no atlas on disk.</summary>
        private static SpriteLibrary.ActorSprites BuildFallbackSet()
        {
            var set = new SpriteLibrary.ActorSprites { Actor = "vfx" };
            foreach (var kv in FallbackTint)
            {
                var pivot = GroundClips.Contains(kv.Key) ? new Vector2(0.5f, 0f)
                                                         : new Vector2(0.5f, 0.5f);
                // A tiny 3-frame fade so VfxOneShot has something to finish on.
                var frames = new Sprite[3];
                for (int i = 0; i < 3; i++)
                {
                    var c = kv.Value;
                    c.a = (byte)(c.a * (3 - i) / 3);                    // fade out
                    frames[i] = SolidSprite(16, c, pivot);
                }
                set.Clips[kv.Key] = frames;
            }
            return set;
        }

        private static Sprite SolidSprite(int size, Color32 color, Vector2 pivot)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            var px = new Color32[size * size];
            for (int i = 0; i < px.Length; i++) px[i] = color;
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), pivot,
                Tuning.PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Texture2D LoadTexture(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.LoadImage(bytes);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return tex;
        }

        // ---- JSON model (matches assets/sprites/vfx/vfx/vfx.json) ------------
        [Serializable] private class VfxAtlasRoot { public string actor; public VfxAtlas atlas; }
        [Serializable] private class VfxAtlas { public string file; public VfxFrame[] frames; public int[] size; }
        [Serializable] private class VfxFrame { public string name; public int[] rect; }
    }

    /// <summary>
    /// Tiny lifetime driver for a spawned <see cref="Vfx"/> effect: destroys the
    /// GameObject once its <see cref="SpriteAnimator"/> reaches the last frame,
    /// with a hard cap so a stuck clip can never linger (VFX.md §1, "nothing stays").
    /// </summary>
    public sealed class VfxOneShot : MonoBehaviour
    {
        private SpriteAnimator _anim;
        private float _hardCap = 1.5f;
        private float _age;

        public void Init(SpriteAnimator anim, float hardCap = 1.5f)
        {
            _anim = anim;
            _hardCap = hardCap;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            if ((_anim != null && _anim.Finished) || _age >= _hardCap)
                Destroy(gameObject);
        }
    }
}
