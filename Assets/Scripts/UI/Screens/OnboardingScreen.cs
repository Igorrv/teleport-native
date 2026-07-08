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

            // Glow sutil no topo (atmosfera premium)
            var glow = UIFactory.ColorBlock(Root, "glow", new Color(T.Primary.r, T.Primary.g, T.Primary.b, 0.12f));
            var grt = glow.rectTransform;
            grt.anchorMin = new Vector2(0.1f, 0.55f); grt.anchorMax = new Vector2(0.9f, 1f);
            grt.offsetMin = grt.offsetMax = Vector2.zero;
            glow.raycastTarget = false;

            var c = UIFactory.ScreenLayout(Root, "col", TextAnchor.MiddleCenter, T.SpaceM, T.SpaceXL);

            UIFactory.Text(c, "TELEPORT", T.MicroSize, T.Accent, TextAnchor.MiddleCenter);
            UIFactory.Text(c, "Tours 3D para imóveis", T.DisplaySize, T.Text, TextAnchor.MiddleCenter, wrap: true);
            UIFactory.Text(c, "Escaneie cômodos com o iPhone e compartilhe um tour profissional com seus clientes.",
                T.BodySize, T.TextMuted, TextAnchor.MiddleCenter, true);

            UIFactory.Spacer(c, T.SpaceM);
            FeatureRow(c, "01", "Captura AR guiada", "Cobertura 360° com feedback em tempo real");
            FeatureRow(c, "02", "Reconstrução na nuvem", "Gaussian Splatting sem treinar no celular");
            FeatureRow(c, "03", "Viewer fluido", "Navegação 3D a 60 fps no device");

            UIFactory.Spacer(c, T.SpaceXL);
            _status = UIFactory.Text(c, "", T.CaptionSize, T.TextMuted, TextAnchor.MiddleCenter, wrap: true);
            var btnRt = UIFactory.Button(c, "Permitir câmera e começar", OnStart, true);
            btnRt.sizeDelta = new Vector2(0, T.ButtonHeight);
            _btn = btnRt.GetComponent<Button>();
            UITween.Pop(btnRt, 0.08f);
        }

        private static void FeatureRow(Transform parent, string num, string title, string sub)
        {
            var row = UIFactory.Card(parent, "feat", T.Surface, T.RadiusM, elevation: false);
            row.sizeDelta = new Vector2(0, 64);
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 64;

            var n = UIFactory.Text(row, num, T.CaptionSize, T.Primary, TextAnchor.MiddleLeft, fit: false);
            n.rectTransform.anchorMin = new Vector2(0, 0); n.rectTransform.anchorMax = new Vector2(0, 1);
            n.rectTransform.pivot = new Vector2(0, 0.5f);
            n.rectTransform.anchoredPosition = new Vector2(T.SpaceM, 0);
            n.rectTransform.sizeDelta = new Vector2(36, 0);

            var t = UIFactory.Text(row, title, T.BodySize, T.Text, TextAnchor.MiddleLeft, fit: false);
            t.rectTransform.anchorMin = new Vector2(0, 0.5f); t.rectTransform.anchorMax = new Vector2(1, 1);
            t.rectTransform.offsetMin = new Vector2(56, 0); t.rectTransform.offsetMax = new Vector2(-T.SpaceM, -6);

            var s = UIFactory.Text(row, sub, T.CaptionSize, T.TextMuted, TextAnchor.MiddleLeft, fit: false);
            s.rectTransform.anchorMin = new Vector2(0, 0); s.rectTransform.anchorMax = new Vector2(1, 0.5f);
            s.rectTransform.offsetMin = new Vector2(56, 6); s.rectTransform.offsetMax = new Vector2(-T.SpaceM, 0);

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
            // Sempre Library: fluxo imobiliário começa por "Novo imóvel".
            Ctx.Flow.Request(ScreenId.Library);
        }
    }
}
