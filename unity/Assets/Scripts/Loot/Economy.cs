namespace ThisL
{
    /// <summary>
    /// The Area-3+ coin / Monkey-Merc economy (WEAPONS.md §3.9 currency, §3.7 mercs).
    /// Enemies drop coins that accumulate in a per-stage wallet (RESETS each stage); 10¢ = a dime =
    /// the cost of one Monkey-Merc summon, capped at 3 summons per level. Off in Areas 1–2 (the
    /// "second-half reveal" — money debuts Area 3, the causeway/farm/dixon block).
    /// </summary>
    public static class Economy
    {
        public const int DimeCost = 10;         // 10¢ = a dime = one Monkey-Merc summon (§3.7/§3.9)
        public const int SummonsPerLevel = 3;   // hard cap on summons made per level (§3.7)
        public const int MaxCents = 30;         // you can bank up to 30¢ (= 3 dimes) (creator ruling)

        /// <summary>Coin value dropped per enemy kind (creator: reg 10¢, shotgun-monkey 20¢, rocket-monkey 30¢).</summary>
        public const int CentsRegular = 10;
        public const int CentsShotgunMonkey = 20;
        public const int CentsRocketMonkey = 30;

        public static bool Active;              // only Area 3+ (money hidden in the first half)
        public static int  Cents;               // current-stage wallet (resets per stage)
        public static int  SummonsThisLevel;    // toward the SummonsPerLevel cap

        /// <summary>Enough saved for a dime AND under the per-level summon cap.</summary>
        public static bool CanSummon => Active && Cents >= DimeCost && SummonsThisLevel < SummonsPerLevel;

        /// <summary>Fresh wallet at each stage start. <paramref name="area"/> 1..4 — economy lights up at 3.</summary>
        public static void ResetStage(int area)
        {
            Active = area >= 3;
            Cents = 0;
            SummonsThisLevel = 0;
        }

        public static void Award(int cents) { if (Active && cents > 0) { Cents += cents; if (Cents > MaxCents) Cents = MaxCents; } }

        /// <summary>Coin value an enemy drops, by kind (creator: reg 10 / gun 20 / heavy·rocket 30).
        /// Unknown ids default to the 10¢ regular tier; swarmers drop nothing.</summary>
        public static int CoinValueFor(string enemyId) => enemyId switch
        {
            "swarmer" => 0,
            "heavy" or "arm_ripper" or "gatling_gunner" or "gatling" or "boomergunner" or "anti_aircraft" => CentsRocketMonkey, // 30
            "gunner" or "sniper" or "snapper" or "ninja" or "head_thrower" => CentsShotgunMonkey, // 20
            _ => CentsRegular, // 10
        };

        /// <summary>Spend a dime for a normal (coin-bought) summon. Returns false if not allowed.
        /// Boss-fight dime CATCHES bypass this (they don't touch the wallet or the cap — BOSSES.md §5.7).</summary>
        public static bool SpendDime()
        {
            if (!CanSummon) return false;
            Cents -= DimeCost;
            SummonsThisLevel++;
            return true;
        }
    }
}
