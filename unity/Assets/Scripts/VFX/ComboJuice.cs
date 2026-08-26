using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Escalating combat feedback (the "chained crowd-wipe feels chaotic" juice):
    /// camera-shake and hit-VFX that scale with the live combo count. Reuses the
    /// LOCKED <see cref="CameraShake"/> presets (VFX.md §8) and <see cref="Vfx"/> /
    /// <see cref="SpecialFx"/> effects, so nothing new needs art. The scale curve
    /// mirrors the meter's combo tiers (5 / 10 / 15+, TUNING §2.4) so the screen
    /// ramps up exactly as the multiplier does.
    /// </summary>
    public static class ComboJuice
    {
        /// <summary>Combo-tier multiplier for shake amplitude (mirrors the meter tiers).</summary>
        public static float TierScale(int combo) =>
            combo >= 15 ? 2.0f : combo >= 10 ? 1.6f : combo >= 5 ? 1.3f : 1.0f;

        /// <summary>
        /// One escalating impact: a combo-scaled shake plus bonus sparks/rings at the
        /// milestones. <paramref name="x"/>/<paramref name="z"/> is the contact point;
        /// <paramref name="heavy"/> uses the Medium preset (sweep / big move) instead
        /// of Light.
        /// </summary>
        public static void Impact(float x, float z, int combo, bool heavy)
        {
            Vector2 preset = heavy ? CameraShake.Medium : CameraShake.Light;
            float k = TierScale(combo);
            CameraShake.Add(preset.x * k, preset.y);

            // Extra chaos as the streak climbs — bonus sparks, then expanding rings.
            if (combo >= 5)
                Vfx.HitSpark(x + Random.Range(-0.35f, 0.35f), z + Random.Range(-0.2f, 0.2f));
            if (combo >= 10)
                SpecialFx.Ring(x, z, 0.8f, new Color(1f, 0.9f, 0.45f, 0.85f), 0.25f);
            if (combo >= 15)
                SpecialFx.Ring(x, z, 1.25f, new Color(1f, 0.5f, 0.2f, 0.9f), 0.30f);
        }
    }
}
