using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The 2.5D projection. Actors live in a world of (X = left/right scroll,
    /// Z = depth 0..6 wu). This maps that logical position onto the flat,
    /// front-on render (TUNING §1):
    ///   * +1.0 wu of Z lifts the sprite's screen-Y by 1.0 wu (24 px Z per wu,
    ///     same 24 px/wu as X), so the near edge sits low and the far edge high.
    ///   * sprites depth-scale 100% (Z=0) -> 80% (Z=6), linear, floored at 80%.
    ///   * nearer actors sort in front.
    /// Unity world units ARE world units (wu); an orthographic camera of size
    /// <see cref="CameraOrthoSize"/> reproduces the 640x360 / 24 ppu framing.
    /// </summary>
    public static class Playfield
    {
        // Camera shows 360 px / 24 = 15 wu vertically; half is 7.5.
        public const float CameraOrthoSize = 7.5f;

        // With the camera centered at y=0 the screen spans y in [-7.5, 7.5].
        // The playfield is the bottom 60% (9 wu): y in [-7.5, 1.5]. Plant the
        // near ground line (Z=0 feet) 1 wu above the screen bottom for foot room.
        public const float GroundBaselineY = -6.5f;

        /// <summary>Screen-Y (world Y for the sprite transform) of an actor's feet at depth z.</summary>
        public static float FeetY(float z)
        {
            return GroundBaselineY + z; // 24 px Z per wu == 1 wu of vertical screen space
        }

        /// <summary>Depth foreshortening: 1.0 near -> 0.8 far, floored.</summary>
        public static float DepthScale(float z)
        {
            float t = Mathf.Clamp01(z / Tuning.ZBandDepth);
            return Mathf.Lerp(Tuning.DepthScaleNear, Tuning.DepthScaleFar, t);
        }

        /// <summary>Sprite sorting order for a depth: nearer (small z) draws in front.</summary>
        public static int SortingOrder(float z)
        {
            // z in [0,6] -> order in [600,0]; add small floor to stay positive.
            return Mathf.RoundToInt((Tuning.ZBandDepth - Mathf.Clamp(z, 0f, Tuning.ZBandDepth)) * 100f);
        }

        /// <summary>Places a sprite transform for a logical (worldX, z) ground position.</summary>
        public static void Place(Transform t, float worldX, float z, SpriteRenderer sr = null)
        {
            t.position = new Vector3(worldX, FeetY(z), 0f);
            float s = DepthScale(z);
            t.localScale = new Vector3(s, s, 1f);
            if (sr != null) sr.sortingOrder = SortingOrder(z);
        }

        /// <summary>True if two actors are close enough in depth for a melee/projectile hit.</summary>
        public static bool WithinZ(float za, float zb, float tolerance)
        {
            return Mathf.Abs(za - zb) <= tolerance;
        }
    }
}
