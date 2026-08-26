namespace ThisL
{
    /// <summary>An enemy the sniper special can one-shot (no loot drop).</summary>
    public interface ISpecialKillable { void KillBySpecial(Actor source); }

    /// <summary>An enemy that can be staggered/knocked down (sweep, boomerang, dash-attack).</summary>
    public interface IStaggerable { void ApplyStagger(float seconds); }
}
