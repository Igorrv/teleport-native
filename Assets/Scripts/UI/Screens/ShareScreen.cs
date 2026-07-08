using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;
using TeleportNative.Core;
using ScreenId = TeleportNative.Core.AppScreen;

namespace TeleportNative.UI
{
    /// <summary>Compartilhar: link publico do espaco (/world/:id do HomeView), copiar, abrir, exportar.</summary>
    public sealed class ShareScreen : AppScreen
    {
        private Text _url;

        protected override void Build()
        {
            UIFactory.Panel(Root, "bg", T.Background);
            var c = UIFactory.ScreenLayout(Root, "col", TextAnchor.MiddleCenter, T.SpaceM, T.SpaceXL);

            UIFactory.Text(c, "Compartilhar espaco", T.HeadingSize, T.Text, TextAnchor.MiddleCenter);
            UIFactory.Spacer(c, T.SpaceS);
            _url = UIFactory.Text(c, "", T.BodySize, T.Accent, TextAnchor.MiddleCenter, true);
            UIFactory.Spacer(c, T.SpaceL);

            UIFactory.Button(c, "Copiar link", OnCopy, true);
            UIFactory.Spacer(c, T.SpaceS);
            UIFactory.Button(c, "Abrir no navegador", OnOpen, false);
            UIFactory.Spacer(c, T.SpaceS);
            UIFactory.Button(c, "Exportar .splat (caminho)", OnExport, false);
            UIFactory.Spacer(c, T.SpaceXL);
            UIFactory.Button(c, "Voltar a biblioteca", () => Ctx.Flow.Request(ScreenId.Library), false);
        }

        public override void OnShow()
        {
            string name = Ctx.CurrentSpace != null ? Ctx.CurrentSpace.Name : "Espaco";
            string url = Ctx.CurrentSpace != null && !string.IsNullOrEmpty(Ctx.CurrentSpace.SceneUrl)
                ? Ctx.CurrentSpace.SceneUrl
                : Ctx.Config.BackendBaseUrl + "/world/" + (Ctx.CurrentSpace?.Id ?? "");
            _url.text = name + "\n\n" + url;
        }

        private void OnCopy()
        {
            Ctx.Haptics.Trigger(HapticType.Selection);
            GUIUtility.systemCopyBuffer = _url.text;
        }

        private void OnOpen()
        {
            if (Ctx.CurrentSpace != null && !string.IsNullOrEmpty(Ctx.CurrentSpace.SceneUrl))
                Application.OpenURL(Ctx.CurrentSpace.SceneUrl);
        }

        private void OnExport()
        {
            // Export nativo (sheet de share) exige plugin. MVP: copia o caminho local do splat.
            if (Ctx.CurrentSpace != null)
                GUIUtility.systemCopyBuffer = Ctx.CurrentSpace.SplatPath;
        }
    }
}
