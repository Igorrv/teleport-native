using UnityEngine;

namespace TeleportNative.Performance
{
    /// <summary>Mede FPS/frame-time (EMA + amostra p/ HUD de dev). Sem alocacao no loop.</summary>
    public sealed class FramePacer
    {
        private float _frameTimeEma = 16.67f;
        private float _hudAccum;
        private int _hudFrames;

        public float FrameTimeMs => _frameTimeEma;
        public int Fps => Mathf.RoundToInt(1000f / Mathf.Max(0.001f, _frameTimeEma));

        /// <summary>Atualiza e devolve FPS amostrado para HUD a cada ~0.25s (evita flicker).</summary>
        public int Tick(float dt, out int hudFps, out float hudMs)
        {
            _frameTimeEma = Mathf.Lerp(_frameTimeEma, dt * 1000f, 0.1f);
            _hudAccum += dt; _hudFrames++;
            if (_hudAccum >= 0.25f)
            {
                hudMs = (_hudAccum * 1000f) / Mathf.Max(1, _hudFrames);
                hudFps = Mathf.RoundToInt(1f / Mathf.Max(0.001f, _hudAccum / Mathf.Max(1, _hudFrames)));
                _hudAccum = 0; _hudFrames = 0;
                return hudFps;
            }
            hudFps = Fps; hudMs = _frameTimeEma;
            return hudFps;
        }
    }
}
