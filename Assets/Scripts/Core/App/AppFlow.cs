using System;

namespace TeleportNative.Core
{
    /// <summary>
    /// Maquina de estados pura (testavel em EditMode). Valida transicoes e notifica a UI.
    /// Regras impedem saltos invalidos (ex.: Viewer so apos Processing pronto).
    /// </summary>
    public sealed class AppFlow
    {
        public AppScreen Current { get; private set; } = AppScreen.Onboarding;
        public event Action<AppScreen, AppScreen> Transitioned; // (from, to)

        public bool CanRequest(AppScreen target) => IsValidTransition(Current, target);

        public bool Request(AppScreen target)
        {
            if (target == Current || !IsValidTransition(Current, target)) return false;
            var from = Current;
            Current = target;
            Transitioned?.Invoke(from, target);
            return true;
        }

        /// <summary>Forca o estado inicial (usado pelo bootstrap; nao dispara evento).</summary>
        public void ResetTo(AppScreen screen) => Current = screen;

        public static bool IsValidTransition(AppScreen from, AppScreen to)
        {
            if (from == to) return false;
            return from switch
            {
                AppScreen.Onboarding  => to == AppScreen.Library,
                AppScreen.Library     => to == AppScreen.Capture || to == AppScreen.Viewer || to == AppScreen.Share,
                AppScreen.Capture     => to == AppScreen.Processing || to == AppScreen.Library,
                AppScreen.Processing  => to == AppScreen.Viewer || to == AppScreen.Library || to == AppScreen.Capture,
                AppScreen.Viewer      => to == AppScreen.Share || to == AppScreen.Library,
                AppScreen.Share       => to == AppScreen.Library || to == AppScreen.Viewer,
                _ => false
            };
        }
    }
}
