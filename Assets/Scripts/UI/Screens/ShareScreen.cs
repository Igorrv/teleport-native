using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;
using TeleportNative.Core;
using ScreenId = TeleportNative.Core.AppScreen;

namespace TeleportNative.UI
{
    /// <summary>Compartilhar: link publico, WhatsApp, copiar, abrir, exportar caminho.</summary>
    public sealed class ShareScreen : AppScreen
    {
        private Text _title;
        private Text _url;
        private Text _feedback;
        private string _link = "";

        protected override void Build()
        {
            UIFactory.Panel(Root, "bg", T.Background);
            UiChrome.Header(Root, "Compartilhar", "Envie o tour para o cliente");

            var c = UIFactory.ScreenLayout(Root, "col", TextAnchor.UpperCenter, T.SpaceM, T.SpaceL);
            c.offsetMin = new Vector2(0, 120);
            c.offsetMax = new Vector2(0, -40);

            var card = UIFactory.Card(c, "linkCard", T.Surface, T.RadiusL, elevation: true);
            card.sizeDelta = new Vector2(0, 140);
            var le = card.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 140;

            _title = UIFactory.Text(card, "", T.BodySize, T.Text, TextAnchor.MiddleLeft, fit: false);
            _title.rectTransform.anchorMin = new Vector2(0, 0.55f);
            _title.rectTransform.anchorMax = new Vector2(1, 1);
            _title.rectTransform.offsetMin = new Vector2(T.SpaceM, 0);
            _title.rectTransform.offsetMax = new Vector2(-T.SpaceM, -T.SpaceS);

            _url = UIFactory.Text(card, "", T.CaptionSize, T.Accent, TextAnchor.UpperLeft, wrap: true, fit: false);
            _url.rectTransform.anchorMin = new Vector2(0, 0);
            _url.rectTransform.anchorMax = new Vector2(1, 0.55f);
            _url.rectTransform.offsetMin = new Vector2(T.SpaceM, T.SpaceS);
            _url.rectTransform.offsetMax = new Vector2(-T.SpaceM, 0);

            UIFactory.Spacer(c, T.SpaceS);
            UIFactory.Button(c, "WhatsApp", OnWhatsApp, true).sizeDelta = new Vector2(0, T.ButtonHeight);
            UIFactory.Button(c, "Copiar link", OnCopy, false).sizeDelta = new Vector2(0, T.ButtonHeight);
            UIFactory.Button(c, "Abrir no navegador", OnOpen, false).sizeDelta = new Vector2(0, T.ButtonHeight);
            UIFactory.Button(c, "Copiar caminho .splat", OnExport, false).sizeDelta = new Vector2(0, T.ButtonHeight);

            UIFactory.Spacer(c, T.SpaceM);
            _feedback = UIFactory.Text(c, "", T.CaptionSize, T.Success, TextAnchor.MiddleCenter);
            UIFactory.Spacer(c, T.SpaceL);
            UIFactory.Button(c, "‹  Biblioteca", () => Ctx.Flow.Request(ScreenId.Library), false)
                .sizeDelta = new Vector2(0, T.ButtonHeight);
        }

        public override void OnShow()
        {
            string name = Ctx.CurrentSpace != null ? Ctx.CurrentSpace.Name : "Espaço";
            _link = Ctx.CurrentSpace != null && !string.IsNullOrEmpty(Ctx.CurrentSpace.SceneUrl)
                ? Ctx.CurrentSpace.SceneUrl
                : Ctx.Config.BackendBaseUrl.TrimEnd('/') + "/world/" + (Ctx.CurrentSpace?.Id ?? "");
            _title.text = name;
            _url.text = _link;
            if (_feedback != null) _feedback.text = "";
        }

        private void OnWhatsApp()
        {
            Ctx.Haptics.Trigger(HapticType.ImpactLight);
            var name = Ctx.CurrentSpace?.Name ?? "tour";
            var msg = UnityEngine.Networking.UnityWebRequest.EscapeURL(
                $"Olá! Confira o tour 3D de {name}: {_link}");
            Application.OpenURL("https://wa.me/?text=" + msg);
            if (_feedback != null) _feedback.text = "Abrindo WhatsApp…";
        }

        private void OnCopy()
        {
            Ctx.Haptics.Trigger(HapticType.Selection);
            GUIUtility.systemCopyBuffer = _link;
            if (_feedback != null) { _feedback.color = T.Success; _feedback.text = "Link copiado"; }
        }

        private void OnOpen()
        {
            if (string.IsNullOrEmpty(_link)) return;
            Ctx.Haptics.Trigger(HapticType.Selection);
            Application.OpenURL(_link);
        }

        private void OnExport()
        {
            if (Ctx.CurrentSpace == null || string.IsNullOrEmpty(Ctx.CurrentSpace.SplatPath))
            {
                if (_feedback != null) { _feedback.color = T.Warning; _feedback.text = "Splat ainda não disponível"; }
                return;
            }
            GUIUtility.systemCopyBuffer = Ctx.CurrentSpace.SplatPath;
            Ctx.Haptics.Trigger(HapticType.Selection);
            if (_feedback != null) { _feedback.color = T.Success; _feedback.text = "Caminho copiado"; }
        }
    }
}
