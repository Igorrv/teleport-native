using System.Collections;
using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;
using TeleportNative.Core;
using ScreenId = TeleportNative.Core.AppScreen;

namespace TeleportNative.UI
{
    /// <summary>Onboarding: pede permissao de camera e inicia captura AR.</summary>
    public sealed class OnboardingScreen : AppScreen
    {
        private Text _status;
        private Button _btn;

        protected override void Build()
        {
            UIFactory.Panel(Root, "bg", T.Background);
            var c = UIFactory.ScreenLayout(Root, "col", TextAnchor.MiddleCenter, T.SpaceM, T.SpaceXL);

            UIFactory.Text(c, "Teleport Native", T.TitleSize, T.Primary, TextAnchor.MiddleCenter);
            UIFactory.Text(c, "Capture ambientes reais com a câmera do iPhone e navegue em 3D fluido.",
                T.BodySize, T.TextMuted, TextAnchor.MiddleCenter, true);

            UIFactory.Spacer(c, T.SpaceM);
            Bullet(c, "Câmera AR + guia 360 (cobertura em tempo real)");
            Bullet(c, "Reconstrução 3D na nuvem (Gaussian Splatting)");
            Bullet(c, "Viewer GPU no celular — estilo Teleport 360");

            UIFactory.Spacer(c, T.SpaceXL);
            _status = UIFactory.Text(c, "", T.CaptionSize, T.TextMuted, TextAnchor.MiddleCenter, wrap: true);
            var btnRt = UIFactory.Button(c, "Permitir câmera e começar", OnStart, true);
            btnRt.sizeDelta = new Vector2(0, T.ButtonHeight);
            _btn = btnRt.GetComponent<Button>();
        }

        private static void Bullet(Transform parent, string s)
        {
            UIFactory.Text(parent, "•  " + s, T.BodySize, T.Text, TextAnchor.MiddleLeft, true);
            UIFactory.Spacer(parent, T.SpaceS);
        }

        private void OnStart()
        {
            if (_btn != null) _btn.interactable = false;
            StartCoroutine(StartFlow());
        }

        private IEnumerator StartFlow()
        {
            _status.text = "Solicitando acesso à câmera...";
            yield return CameraPermissionHelper.EnsureCamera();

            if (!CameraPermissionHelper.HasCamera)
            {
                _status.text = "Permissão negada. Ajustes → Privacidade → Câmera → Teleport Native.";
                if (_btn != null) _btn.interactable = true;
                yield break;
            }

            PlayerPrefs.SetInt("tn_onboarded", 1);
            Ctx.Haptics.Trigger(HapticType.Success);
            var next = Ctx.Capture != null ? ScreenId.Capture : ScreenId.Library;
            Ctx.Flow.Request(next);
        }
    }
}
