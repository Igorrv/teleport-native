using UnityEngine;
using TeleportNative.Core;

namespace TeleportNative.UI
{
    /// <summary>
    /// IHaptics. Implementacao leve: vibra curto para eventos fortes (Handheld.Vibrate e o unico
    /// built-in cross-platform, porem longo). Para haptica fina (selection/impact) em producao,
    /// plugar um plugin nativo iOS/Android (ex.: Nice Vibrations). No-op + log em dev e o fallback.
    /// </summary>
    public sealed class Haptics : IHaptics
    {
        public void Trigger(HapticType type)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.Log($"[Haptics] {type}");
#endif
            switch (type)
            {
                case HapticType.Warning:
                case HapticType.Error:
                case HapticType.ImpactMedium:
#if UNITY_IOS || UNITY_ANDROID
                    Handheld.Vibrate(); // grosseiro; substituir por plugin nativo
#endif
                    break;
            }
        }
    }
}
