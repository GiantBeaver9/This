using System;
using System.IO;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The blob ground-shadow / Z-marker from VFX.md §2 — "1 blob shadow per
    /// actor (the most-instanced sprite)", reading the actor's exact Z on the
    /// band. A flat sprite pinned to the owner's (WorldX, Z) feet position,
    /// depth-scaled like the owner, drawn just above the ground and below all
    /// actors (sortingOrder ~ -900). Comes in three size tiers (small / medium /
    /// large) mapping to the placeholder art
    /// <c>assets/sprites/vfx/blob_shadow/shadow_{small|regular|boss}.png</c>.
    ///
    /// Loads its textures at runtime off disk (reusing
    /// <see cref="SpriteLibrary.AssetsRoot"/>, point-filtered) and degrades to a
    /// generated soft quad if a file is missing.
    /// </summary>
    public sealed class Shadow : MonoBehaviour
    {
        public const int SmallTier = 0;
        public const int MediumTier = 1;
        public const int LargeTier = 2;

        // Just above the ground, below every actor (actors sort in [0, 600]).
        private const int ShadowSortBase = -900;
        private const float ShadowAlpha = 110f / 255f; // blob_shadow.json opacity

        private static readonly Sprite[] _tierSprites = new Sprite[3];
        private static bool _spritesLoaded;

        private Actor _owner;
        private SpriteRenderer _sr;

        /// <summary>Create a blob shadow that follows <paramref name="owner"/>.</summary>
        /// <param name="sizeTier">0 small, 1 medium, 2 large (boss).</param>
        public static Shadow Attach(Actor owner, int sizeTier)
        {
            if (owner == null) return null;
            var go = new GameObject($"shadow_{owner.name}");
            var shadow = go.AddComponent<Shadow>();
            shadow.Setup(owner, sizeTier);
            return shadow;
        }

        private void Setup(Actor owner, int sizeTier)
        {
            _owner = owner;
            _sr = gameObject.GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();

            EnsureSprites();
            int tier = Mathf.Clamp(sizeTier, 0, 2);
            _sr.sprite = _tierSprites[tier];
            _sr.color = new Color(1f, 1f, 1f, ShadowAlpha);
            Follow(); // place immediately so it never pops in at the origin
        }

        private void LateUpdate()
        {
            // Owner destroyed (Unity-null) -> the blob goes with it (§1 nothing lingers).
            if (_owner == null)
            {
                Destroy(gameObject);
                return;
            }
            Follow();
        }

        private void Follow()
        {
            float z = _owner.Z;
            // Sit on the ground line at the owner's logical position, depth-scaled.
            Playfield.Place(transform, _owner.WorldX, z, _sr);
            // Below actors; nearer shadows draw over farther ones but stay < 0.
            _sr.sortingOrder = ShadowSortBase - Mathf.RoundToInt(Mathf.Clamp(z, 0f, Tuning.ZBandDepth));
            _sr.enabled = _owner.Alive; // hide once the owner is dead but not yet cleaned up
        }

        // -- Loading ----------------------------------------------------------

        private static void EnsureSprites()
        {
            if (_spritesLoaded) return;
            _spritesLoaded = true;

            string[] files = { "shadow_small.png", "shadow_regular.png", "shadow_boss.png" };
            string dir = Path.Combine(SpriteLibrary.AssetsRoot, "sprites", "vfx", "blob_shadow");

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    string path = Path.Combine(dir, files[i]);
                    if (File.Exists(path))
                    {
                        var tex = LoadTexture(path);
                        _tierSprites[i] = Sprite.Create(
                            tex, new Rect(0, 0, tex.width, tex.height),
                            new Vector2(0.5f, 0.5f), // centered on the feet/ground line
                            Tuning.PixelsPerUnit, 0, SpriteMeshType.FullRect);
                        _tierSprites[i].name = files[i];
                    }
                    else
                    {
                        Debug.LogWarning($"[Shadow] Missing {path}; using generated blob.");
                        _tierSprites[i] = FallbackBlob(8 + i * 6);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Shadow] Failed to load tier {i}: {e.Message}");
                    _tierSprites[i] = FallbackBlob(8 + i * 6);
                }
            }
        }

        /// <summary>Soft dark ellipse used when the art is unavailable.</summary>
        private static Sprite FallbackBlob(int diameter)
        {
            int w = diameter, h = Mathf.Max(4, diameter / 2);
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            var px = new Color32[w * h];
            float cx = (w - 1) * 0.5f, cy = (h - 1) * 0.5f;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) / (cx + 0.001f);
                float dy = (y - cy) / (cy + 0.001f);
                bool inside = dx * dx + dy * dy <= 1f;
                px[y * w + x] = inside ? new Color32(0, 0, 0, 255) : new Color32(0, 0, 0, 0);
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h),
                new Vector2(0.5f, 0.5f), Tuning.PixelsPerUnit, 0, SpriteMeshType.FullRect);
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
    }
}
