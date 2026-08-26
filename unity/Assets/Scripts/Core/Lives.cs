using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// The shared team life pool (creator spec). One pool for the whole run, drawn from by
    /// either player: start with <see cref="Tuning.StartingLives"/>, gain one on each AREA
    /// clear (<see cref="CampaignRunner"/> awards it when a completed stage's area differs
    /// from the next), spend one to respawn a downed player. When a death would draw from an
    /// empty pool and nobody is left standing, it's GAME OVER.
    ///
    /// In single-player this is simply "3 lives + area-clear bonuses" — a fairer run of a
    /// very hard game. Owned by <see cref="GameFlow"/> for the run lifecycle.
    /// </summary>
    public static class Lives
    {
        public static int Count { get; private set; }

        /// <summary>Raised whenever the count changes (HUD listens; polling also works).</summary>
        public static event System.Action Changed;

        public static void Reset(int n)
        {
            Count = Mathf.Max(0, n);
            Changed?.Invoke();
        }

        /// <summary>Spend a life if any remain. Returns false when the pool is empty.</summary>
        public static bool TryConsume()
        {
            if (Count <= 0) return false;
            Count--;
            Changed?.Invoke();
            return true;
        }

        public static void Award(int n = 1)
        {
            if (n <= 0) return;
            Count += n;
            Changed?.Invoke();
        }
    }
}
