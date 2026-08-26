using System;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The special meter + combo counter (TUNING §2.4). Fists fill fastest
    /// (+3.34/hit), weapon hits half (+1.67). A rising combo multiplies the fill
    /// (x1 / x1.25 / x1.5 / x2 at 1-4 / 5-9 / 10-14 / 15+ hits). The combo resets
    /// after 2s without a hit, or when the player takes damage (that breaks the
    /// combo but does NOT drain the meter). The meter caps at green (300).
    /// Firing drains the whole meter to 0.
    /// </summary>
    [Serializable]
    public sealed class SpecialMeter
    {
        public float Points;      // 0..300
        public int Combo;         // hit-streak count
        private float _comboTimer;

        public event Action ComboChanged;

        /// <summary>Tier the meter can currently fire at: 0 none, 1 yellow, 2 blue, 3 green.</summary>
        public int FullTier => Points >= Tuning.MeterMax ? 3
                             : Points >= 200f ? 2
                             : Points >= Tuning.MeterFull ? 1 : 0;

        public bool CanFire => FullTier >= 1;
        public float Fraction01 => Mathf.Clamp01(Points / Tuning.MeterMax);

        public void Tick(float dt)
        {
            if (Combo > 0)
            {
                _comboTimer += dt;
                if (_comboTimer >= Tuning.ComboDropTimeout) ResetCombo();
            }
        }

        public void RegisterHit(bool isFist, float rateMult = 1f)
        {
            Combo++;
            _comboTimer = 0f;
            float baseVal = isFist ? Tuning.MeterPerFistHit : Tuning.MeterPerWeaponHit;
            Points = Mathf.Min(Tuning.MeterMax, Points + baseVal * ComboMultiplier(Combo) * rateMult);
            ComboChanged?.Invoke();
        }

        /// <summary>A flat meter award (e.g. killed-Sniper rifle = +100), overfill discarded.</summary>
        public void Award(float points)
        {
            Points = Mathf.Min(Tuning.MeterMax, Points + points);
        }

        public void OnDamaged() => ResetCombo(); // breaks combo, keeps meter

        public void ResetCombo()
        {
            if (Combo == 0) return;
            Combo = 0;
            _comboTimer = 0f;
            ComboChanged?.Invoke();
        }

        /// <summary>Fire the special: returns the tier fired at (0 if it couldn't) and drains to 0.</summary>
        public int Fire()
        {
            int tier = FullTier;
            if (tier == 0) return 0;
            Points = 0f;
            ResetCombo();
            return tier;
        }

        public static float ComboMultiplier(int combo) =>
            combo >= 15 ? 2.0f : combo >= 10 ? 1.5f : combo >= 5 ? 1.25f : 1.0f;

        /// <summary>Sniper ricochet kill count by tier (TUNING §2.4): 15 / 30 / 45.</summary>
        public static int SniperKills(int tier) => tier <= 1 ? 15 : tier == 2 ? 30 : 45;

        /// <summary>Passive damage buff while charged: +10% per fill.</summary>
        public float DamageMultiplier => 1f + 0.10f * FullTier;
    }
}
