using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;
using TeleportNative.Core;
using ScreenId = TeleportNative.Core.AppScreen;

namespace TeleportNative.UI
{
    /// <summary>Onboarding: explica camera/movimento e libera o app. (1o uso.)</summary>
    public sealed class OnboardingScreen : AppScreen
    {
        protected override void Build()
        {
            UIFactory.Panel(Root, "bg", T.Background);
            var c = UIFactory.ScreenLayout(Root, "col", TextAnchor.MiddleCenter, T.SpaceM, T.SpaceXL);

            UIFactory.Text(c, "Teleport Native", T.TitleSize, T.Primary, TextAnchor.MiddleCenter);
            UIFactory.Text(c, "Capture ambientes reais com a camera do iPhone e navegue em 3D fluido.",
                T.BodySize, T.TextMuted, TextAnchor.MiddleCenter, true);

            UIFactory.Spacer(c, T.SpaceM);
            Bullet(c, "Camera AR + guia 360 (cobertura em tempo real)");
            Bullet(c, "Reconstrucao 3D na nuvem (Gaussian Splatting)");
            Bullet(c, "Viewer GPU no celular — estilo Teleport 360");

            UIFactory.Spacer(c, T.SpaceXL);
            UIFactory.Button(c, "Permitir camera e capturar", OnStart, true);
        }

        private static void Bullet(Transform parent, string s)
        {
            UIFactory.Text(parent, "•  " + s, T.BodySize, T.Text, TextAnchor.MiddleLeft, true);
            UIFactory.Spacer(parent, T.SpaceS);
        }

        private void OnStart()
        {
            PlayerPrefs.SetInt("tn_onboarded", 1);
            Ctx.Haptics.Trigger(HapticType.Success);
            // iPhone: vai direto para captura AR; desktop sem AR cai na biblioteca.
            var next = Ctx.Capture != null ? ScreenId.Capture : ScreenId.Library;
            Ctx.Flow.Request(next);
        }
    }
}
