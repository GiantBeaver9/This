namespace ThisL
{
    /// <summary>An enemy the sniper special can one-shot (no loot drop).</summary>
    public interface ISpecialKillable { void KillBySpecial(Actor source); }

    /// <summary>An enemy that can be staggered/knocked down (sweep, boomerang, dash-attack).</summary>
    public interface IStaggerable { void ApplyStagger(float seconds); }

    /// <summary>A target a forward WHIP swing can grab-and-rip instead of merely pulling toward you —
    /// the Colossus loses one stick-figure "piece" (and a chunk of its bar) per connecting whip crack.</summary>
    public interface IWhipPullable { void RegisterWhipPull(Actor source); }
}
