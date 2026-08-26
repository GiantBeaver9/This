using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A dropped weapon lying on the ground. Pickup is <b>F-based</b> (PLAYER.md §2
    /// "F = pick up"): there is no auto-grab on overlap. The player polls
    /// <see cref="NearestWithin"/> for the closest live drop on its depth band and
    /// calls <see cref="GrabBy"/> to take it (destroying the current weapon, §1
    /// single-slot rule). Uses a colour-coded marker for now; real ground-pickup art
    /// wires in later.
    /// </summary>
    public sealed class Pickup : MonoBehaviour
    {
        public WeaponKind Kind;
        public float WorldX;
        public float Z;
        private SpriteRenderer _sr;
        private float _bob;

        /// <summary>Live pickups, for the player's F-key nearest-drop query.</summary>
        private static readonly List<Pickup> _all = new();

        public static Pickup SpawnWeapon(WeaponKind kind, float x, float z)
        {
            var go = new GameObject($"pickup_{kind}");
            var p = go.AddComponent<Pickup>();
            p.Kind = kind;
            p.WorldX = x;
            p.Z = z;
            p._sr = go.AddComponent<SpriteRenderer>();
            // Real stick-figure-weapon art if present (assets/sprites/weapons/<kind>/<kind>_pickup.png,
            // the "dead stick figure reshaped into a weapon" premise); else the tinted marker.
            var art = SpriteFor(kind);
            p._sr.sprite = art ?? Marker();
            p._sr.color = art != null ? Color.white : ColorFor(kind);
            return p;
        }

        private static readonly Dictionary<WeaponKind, Sprite> _artCache = new();
        private static Sprite SpriteFor(WeaponKind kind)
        {
            if (_artCache.TryGetValue(kind, out var cached)) return cached;
            Sprite s = null;
            try
            {
                string name = kind.ToString().ToLowerInvariant();
                string path = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "sprites", "weapons", name, name + "_pickup.png");
                if (System.IO.File.Exists(path))
                {
                    var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    tex.LoadImage(System.IO.File.ReadAllBytes(path));
                    tex.Apply();
                    s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0f), Tuning.PixelsPerUnit);
                }
            }
            catch { s = null; }
            _artCache[kind] = s;
            return s;
        }

        /// <summary>
        /// The nearest live pickup to (<paramref name="x"/>,<paramref name="z"/>) within
        /// radius <paramref name="r"/> and on a similar depth band — the target the player's
        /// F key grabs. Returns null when nothing is in reach.
        /// </summary>
        public static Pickup NearestWithin(float x, float z, float r)
        {
            Pickup best = null;
            float bestSq = r * r;
            foreach (var p in _all)
            {
                if (p == null) continue;
                float dz = p.Z - z;
                if (Mathf.Abs(dz) > 1.0f) continue;   // same Z-ish: one player-depth of tolerance
                float dx = p.WorldX - x;
                float dsq = dx * dx + dz * dz;
                if (dsq <= bestSq) { bestSq = dsq; best = p; }
            }
            return best;
        }

        /// <summary>
        /// Hand this weapon to <paramref name="p"/>: equip the kind (destroying whatever
        /// was held, §1), play the pickup cue, and remove the drop from the world.
        /// </summary>
        public void GrabBy(PlayerController p)
        {
            if (p == null) return;
            p.Equip(Kind);
            Sfx.Play("weapon_pickup");
            Destroy(gameObject);
        }

        private void OnEnable() { if (!_all.Contains(this)) _all.Add(this); }
        private void OnDisable() => _all.Remove(this);

        private void Update()
        {
            // Auto-grab ONLY when a player is empty-handed (fists); while armed you press
            // F to swap (PLAYER.md §2 single-slot rule). Any empty-handed player nearby grabs.
            foreach (var player in PlayerController.All)
            {
                if (player == null || !player.Alive) continue;
                if (player.CurrentWeapon == null || !player.CurrentWeapon.IsFists) continue;
                float dx = player.WorldX - WorldX, dz = player.Z - Z;
                if (dx * dx + dz * dz <= 0.8f * 0.8f) { GrabBy(player); return; }
            }
            _bob = Mathf.Sin(Time.time * 4f) * 0.06f;
        }

        private void LateUpdate()
        {
            Playfield.Place(transform, WorldX, Z + _bob, _sr);
        }

        private static Color ColorFor(WeaponKind k) => k switch
        {
            WeaponKind.Sword => new Color(0.6f, 0.9f, 1f),
            WeaponKind.Shotgun => new Color(1f, 0.6f, 0.2f),
            WeaponKind.Boomerang => new Color(0.8f, 1f, 0.4f),
            WeaponKind.Pistol => new Color(0.85f, 0.85f, 0.9f),   // gunmetal
            WeaponKind.Revolver => new Color(0.7f, 0.7f, 0.75f),  // darker gunmetal
            WeaponKind.Whip => new Color(0.8f, 0.5f, 0.3f),       // leather
            WeaponKind.Staff => new Color(0.7f, 0.5f, 1f),        // arcane violet
            WeaponKind.Bat => new Color(0.85f, 0.6f, 0.4f),       // wood
            WeaponKind.Club => new Color(0.6f, 0.45f, 0.3f),      // heavy wood
            WeaponKind.Grenade => new Color(0.4f, 0.7f, 0.35f),   // olive
            WeaponKind.BallChain => new Color(0.5f, 0.5f, 0.55f), // iron
            WeaponKind.Gatling => new Color(1f, 0.55f, 0.15f),    // hot barrel
            _ => Color.white,
        };

        private static Sprite _cachedSquare;
        private static Sprite Marker()
        {
            // One shared 8x8 white square, tinted per pickup via the renderer color.
            if (_cachedSquare == null)
            {
                var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                var px = new Color32[64];
                for (int i = 0; i < px.Length; i++) px[i] = Color.white;
                tex.SetPixels32(px);
                tex.Apply();
                _cachedSquare = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0f), Tuning.PixelsPerUnit);
            }
            return _cachedSquare;
        }
    }
}
