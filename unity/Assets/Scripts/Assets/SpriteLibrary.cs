using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Loads the placeholder sprite atlases straight off disk (the repo's
    /// <c>assets/sprites/...</c> tree) at runtime, so the core-loop prototype
    /// needs no Unity sprite-import step. Each actor has an <c>&lt;actor&gt;_atlas.png</c>
    /// plus an <c>&lt;actor&gt;.json</c> describing frame rects; we slice sprites
    /// per animation clip (idle/walk/attack_side/...), point-filtered, ppu=24,
    /// bottom-center pivot (feet on the ground line).
    ///
    /// Assets aren't imported by Unity, so they carry no GUIDs; if the tree is
    /// missing (e.g. a headless run without the repo assets) we fall back to a
    /// solid placeholder so the game still boots.
    /// </summary>
    public static class SpriteLibrary
    {
        private static string _assetsRoot;
        private static readonly Dictionary<string, ActorSprites> _actors = new();

        public static string AssetsRoot
        {
            get
            {
                if (_assetsRoot == null)
                {
                    // Application.dataPath = <repo>/unity/Assets  ->  <repo>/assets
                    string byProject = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "assets"));
                    string byStreaming = Path.Combine(Application.streamingAssetsPath, "assets");
                    _assetsRoot = Directory.Exists(byProject) ? byProject
                                : Directory.Exists(byStreaming) ? byStreaming
                                : byProject; // report the expected path even if absent
                }
                return _assetsRoot;
            }
        }

        /// <summary>All clips for one actor, keyed by clip name (e.g. "walk", "attack_side").</summary>
        public sealed class ActorSprites
        {
            public string Actor;
            public Texture2D Texture;
            public bool ReverseAttacks = true; // placeholder stick attacks read backwards; real (pixellab) art plays forward
            public readonly Dictionary<string, Sprite[]> Clips = new();
            public Sprite First => FirstOf("idle") ?? FirstOf("walk");
            public Sprite FirstOf(string clip) =>
                Clips.TryGetValue(clip, out var f) && f.Length > 0 ? f[0] : null;
        }

        /// <summary>True if an atlas (png + json) actually exists on disk for this actor
        /// — lets callers try a bespoke sprite and fall back if the art isn't in yet.</summary>
        public static bool HasAtlas(string relativeDir, string actor)
        {
            string dir = Path.Combine(AssetsRoot, relativeDir.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(Path.Combine(dir, actor + ".json"))
                && File.Exists(Path.Combine(dir, actor + "_atlas.png"));
        }

        /// <param name="relativeDir">e.g. "sprites/characters/player_tactical"</param>
        /// <param name="actor">e.g. "player_tactical"</param>
        public static ActorSprites Load(string relativeDir, string actor)
        {
            if (_actors.TryGetValue(actor, out var cached)) return cached;

            var result = new ActorSprites { Actor = actor };
            try
            {
                string dir = Path.Combine(AssetsRoot, relativeDir.Replace('/', Path.DirectorySeparatorChar));
                string jsonPath = Path.Combine(dir, actor + ".json");
                string pngPath = Path.Combine(dir, actor + "_atlas.png");

                if (File.Exists(jsonPath) && File.Exists(pngPath))
                {
                    var tex = LoadTexture(pngPath);
                    result.Texture = tex;
                    var root = JsonUtility.FromJson<AtlasRoot>(File.ReadAllText(jsonPath));
                    result.ReverseAttacks = root.source != "pixellab"; // pixellab art is already in order
                    SliceClips(actor, tex, root, result);
                }
                else
                {
                    result.Texture = PlaceholderTexture();
                    result.Clips["idle"] = new[] { PlaceholderSprite(result.Texture) };
                    Debug.LogWarning($"[SpriteLibrary] Missing atlas for '{actor}' at {dir}; using placeholder.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SpriteLibrary] Failed to load '{actor}': {e.Message}");
                result.Texture = PlaceholderTexture();
                result.Clips["idle"] = new[] { PlaceholderSprite(result.Texture) };
            }

            _actors[actor] = result;
            return result;
        }

        private static void SliceClips(string actor, Texture2D tex, AtlasRoot root, ActorSprites result)
        {
            if (root?.atlas?.frames == null) return;
            int texH = tex.height;
            string prefix = actor + "_";
            var perClip = new Dictionary<string, List<Sprite>>();

            foreach (var f in root.atlas.frames)
            {
                if (f.rect == null || f.rect.Length < 4 || string.IsNullOrEmpty(f.name)) continue;
                string name = f.name.EndsWith(".png") ? f.name.Substring(0, f.name.Length - 4) : f.name;
                if (name.StartsWith(prefix)) name = name.Substring(prefix.Length);
                int us = name.LastIndexOf('_');
                string clip = us > 0 ? name.Substring(0, us) : name; // strip trailing _NN

                int x = f.rect[0], yTop = f.rect[1], w = f.rect[2], h = f.rect[3];
                int yBottom = texH - (yTop + h); // JSON is top-left origin; Unity texture is bottom-left
                var sprite = Sprite.Create(
                    tex,
                    new Rect(x, yBottom, w, h),
                    new Vector2(0.5f, 0f),      // bottom-center pivot = feet on the ground line
                    Tuning.PixelsPerUnit,
                    0,
                    SpriteMeshType.FullRect);
                sprite.name = $"{actor}_{clip}";

                if (!perClip.TryGetValue(clip, out var list)) perClip[clip] = list = new List<Sprite>();
                list.Add(sprite);
            }

            foreach (var kv in perClip) result.Clips[kv.Key] = kv.Value.ToArray();
        }

        private static Texture2D LoadTexture(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.LoadImage(bytes); // resizes to the PNG's dimensions
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return tex;
        }

        private static Texture2D _placeholder;
        private static Texture2D PlaceholderTexture()
        {
            if (_placeholder != null) return _placeholder;
            _placeholder = new Texture2D(24, 48, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[24 * 48];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 0, 255, 255);
            _placeholder.SetPixels32(px);
            _placeholder.Apply();
            return _placeholder;
        }

        private static Sprite PlaceholderSprite(Texture2D tex) =>
            Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0f), Tuning.PixelsPerUnit);

        // ---- JSON model (matches assets/sprites/.../<actor>.json) --------------
        [Serializable] private class AtlasRoot { public string actor; public Atlas atlas; public string source; }
        [Serializable] private class Atlas { public string file; public Frame[] frames; public int[] size; }
        [Serializable] private class Frame { public string name; public int[] rect; }
    }
}
