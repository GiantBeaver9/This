using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Loads character-select PORTRAIT illustrations off disk at runtime from
    /// <c>assets/portraits/&lt;characterId&gt;.png</c> (e.g. tactical.png,
    /// shotgunner.png, werewolf.png, underdog.png). Unlike the pixel sprites these
    /// are full-res art shown big on the fighter-select card. Returns null (cached)
    /// when the file isn't in yet, so the screen falls back to the stat block.
    /// </summary>
    public static class PortraitLibrary
    {
        private static readonly Dictionary<string, Texture2D> _cache = new();

        public static Texture2D Get(string characterId)
        {
            if (characterId == null) return null;
            if (_cache.TryGetValue(characterId, out var tex)) return tex;

            tex = null;
            try
            {
                string path = Path.Combine(SpriteLibrary.AssetsRoot, "portraits", characterId + ".png");
                if (File.Exists(path))
                {
                    tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
                    tex.LoadImage(File.ReadAllBytes(path));
                    tex.Apply();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[PortraitLibrary] failed to load '{characterId}': {e.Message}");
                tex = null;
            }

            _cache[characterId] = tex;
            return tex;
        }
    }
}
