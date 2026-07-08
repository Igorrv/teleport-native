using T = TeleportNative.Core.DesignTokens;
using UnityEngine;

namespace TeleportNative.UI
{
    /// <summary>
    /// Tween leve (sem dependencias) para "animacoes fluidas". Pop = entrada com escala + ease
    /// (efeito mola/cubic-out). Aplique a cards/elementos ao entrarem na tela para sensacao premium.
    /// </summary>
    public sealed class UITween : MonoBehaviour
    {
        private const float StartScale = 0.94f;

        private float _t, _dur, _delay;
        private bool _playing;

        public static void Pop(Transform t, float delay = 0f, float dur = T.MotionNormal)
        {
            var tw = t.gameObject.GetComponent<UITween>() ?? t.gameObject.AddComponent<UITween>();
            tw.Run(delay, dur);
        }

        private void Run(float delay, float dur)
        {
            _delay = delay;
            _dur = Mathf.Max(0.01f, dur);
            _t = 0f;
            _playing = true;
            transform.localScale = Vector3.one * StartScale;
            enabled = true;
        }

        private void Update()
        {
            if (!_playing) return;
            if (_delay > 0f) { _delay -= Time.deltaTime; return; }
            _t += Time.deltaTime / _dur;
            float e = EaseOutCubic(Mathf.Clamp01(_t));
            transform.localScale = Vector3.LerpUnclamped(Vector3.one * StartScale, Vector3.one, e);
            if (_t >= 1f) { transform.localScale = Vector3.one; _playing = false; enabled = false; }
        }

        private static float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
    }
}
