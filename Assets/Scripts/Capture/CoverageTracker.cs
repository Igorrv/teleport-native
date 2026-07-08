using UnityEngine;

namespace TeleportNative.Capture
{
    /// <summary>Cobertura 360 do scan (setores de heading ja cobertos). Guia o usuario na captura.</summary>
    public struct CoverageState
    {
        public int SectorsCovered;
        public int TotalSectors;
        public float Coverage01; // 0..1
        public bool IsComplete => Coverage01 >= 0.999f;
    }

    /// <summary>
    /// Marca setores de yaw (heading) conforme a camera aponta para direcoes. Logica pura
    /// (testavel). 12 setores = 30 graus cada; completo quando todos cobertos (volta 360).
    /// </summary>
    public sealed class CoverageTracker
    {
        private readonly bool[] _bins;
        private int _covered;

        public CoverageTracker(int sectors = 12)
        {
            _bins = new bool[sectors];
            TotalSectors = sectors;
        }

        public int TotalSectors { get; }

        public CoverageState Register(Vector3 cameraForward)
        {
            // yaw a partir da projecao horizontal do forward (-180..180)
            float yaw = Mathf.Atan2(cameraForward.x, cameraForward.z) * Mathf.Rad2Deg;
            int idx = Mathf.FloorToInt((yaw + 180f) / 360f * TotalSectors) % TotalSectors;
            if (idx < 0) idx += TotalSectors;
            if (!_bins[idx]) { _bins[idx] = true; _covered++; }

            return Current;
        }

        public CoverageState Current => new()
        {
            SectorsCovered = _covered,
            TotalSectors = TotalSectors,
            Coverage01 = _covered / (float)TotalSectors
        };

        public void Reset() { for (int i = 0; i < _bins.Length; i++) _bins[i] = false; _covered = 0; }
    }
}
