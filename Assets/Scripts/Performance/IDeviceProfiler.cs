namespace TeleportNative.Performance
{
    /// <summary>
    /// "Motor" de desempenho (interface): classifica o aparelho, define o budget de splats e
    /// reage a thermal throttling / queda de FPS. Injetavel (substitui o antigo static DeviceTier).
    /// </summary>
    public interface IDeviceProfiler
    {
        Tier Tier { get; }
        int BaseSplatBudget { get; }        // nominal do tier (sem scaling termico)
        int SplatBudget { get; }            // alvo atual, ja escalado por thermal/FPS
        float DynamicResolutionScale { get; } // 0.5..1 (URP ScalableBuffer)
        float ThermalFactor { get; }        // 1 = ok, tendendo a ~0.6 quando esquenta
        bool IsThrottled { get; }

        void Tick(float deltaTime, float frameTimeMs);
        void OverrideTier(Tier tier);
    }
}
