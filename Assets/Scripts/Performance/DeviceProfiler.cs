using UnityEngine;

namespace TeleportNative.Performance
{
    /// <summary>
    /// IDeviceProfiler baseado em heuristica (RAM/GPU) + reacao em runtime a queda de FPS.
    /// Thermal: Unity nao expor estado termico cross-platform; estimamos pela degradacao do
    /// frame time (sustentado acima de 60fps-alvo = reduzir budget/resolucao). Hook nativo
    /// (ProcessInfo.thermalState iOS / PowerManager Android) pode refinar no futuro.
    /// </summary>
    public sealed class DeviceProfiler : IDeviceProfiler
    {
        private const float TargetFrameMs = 1000f / 60f;       // 16.67
        private const float HotFrameMs = TargetFrameMs * 1.05f; // ~17.5
        private const float CoolFrameMs = TargetFrameMs * 0.8f; // ~13.3
        private const float MinThermal = 0.6f;                  // nunca abaixo de 60% do budget

        public Tier Tier { get; private set; }
        public int BaseSplatBudget => global::TeleportNative.Performance.SplatBudget.For(Tier);
        public int SplatBudget { get; private set; }
        public float DynamicResolutionScale { get; private set; } = 1f;
        public float ThermalFactor { get; private set; } = 1f;
        public bool IsThrottled => ThermalFactor < 0.99f;

        private float _frameEma;

        public DeviceProfiler() => DetectTier();

        public void OverrideTier(Tier tier)
        {
            Tier = tier;
            SplatBudget = BaseSplatBudget;
            Debug.Log($"[DeviceProfiler] tier manual = {Tier} (budget {SplatBudget:N0})");
        }

        public void Tick(float deltaTime, float frameTimeMs)
        {
            // EMA do frame time (suaviza picos).
            _frameEma = _frameEma <= 0 ? frameTimeMs : Mathf.Lerp(_frameEma, frameTimeMs, 0.05f);

            // Reacao termica: esquenta -> reduz; esfria -> recupera (lento p/ evitar oscilacao).
            if (_frameEma > HotFrameMs)
            {
                ThermalFactor = Mathf.Max(MinThermal, ThermalFactor - deltaTime * 0.25f);
                DynamicResolutionScale = Mathf.Max(0.6f, DynamicResolutionScale - deltaTime * 0.25f);
            }
            else if (_frameEma < CoolFrameMs)
            {
                ThermalFactor = Mathf.Min(1f, ThermalFactor + deltaTime * 0.10f);
                DynamicResolutionScale = Mathf.Min(1f, DynamicResolutionScale + deltaTime * 0.10f);
            }

            SplatBudget = Mathf.RoundToInt(BaseSplatBudget * ThermalFactor);
        }

        private void DetectTier()
        {
            int ram = SystemInfo.systemMemorySize;       // MB
            int gpuMem = SystemInfo.graphicsMemorySize;  // MB (0 em alguns mobile)

            Tier = ram <= 3500 ? Tier.Low
                 : (ram >= 7000 || gpuMem >= 3000) ? Tier.High
                 : Tier.Mid;

            SplatBudget = BaseSplatBudget;
            Debug.Log($"[DeviceProfiler] detectado {Tier} | ram={ram}MB gpu={gpuMem}MB | budget {SplatBudget:N0}");
        }
    }
}
