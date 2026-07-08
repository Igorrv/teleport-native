namespace TeleportNative.Performance
{
    /// <summary>Budget nominal de splats por tier (milenhos). Valores do briefing (AGENTS.md).</summary>
    public static class SplatBudget
    {
        public const int Low  = 1_500_000;
        public const int Mid  = 4_000_000;
        public const int High = 8_000_000;

        public static int For(Tier tier) => tier switch
        {
            Tier.Low => Low,
            Tier.High => High,
            _ => Mid
        };
    }
}
