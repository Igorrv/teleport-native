using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;
using TeleportNative.Core;
using CoreSpace = TeleportNative.Core.Space;
using ScreenId = TeleportNative.Core.AppScreen;

namespace TeleportNative.UI
{
    /// <summary>Home: espacos capturados + CTA principal para camera AR.</summary>
    public sealed class LibraryScreen : AppScreen
    {
        private RectTransform _list;
        private Text _empty;

        protected override void Build()
        {
            UIFactory.Panel(Root, "bg", T.Background);
            UiChrome.Header(Root, "Seus espacos", "Capture ambientes reais e navegue em 3D");

            FooterButton("Capturar com câmera", OnNewSpace, accent: true);
#if UNITY_EDITOR
            FooterButton("Demo: viewer sem AR", OnDemoViewer, top: 120, accent: false);
#endif

            _list = UIFactory.ScrollList(Root, "list", 100,
#if UNITY_EDITOR
                240,
#else
                130,
#endif
                T.Background);

            _empty = UIFactory.Text(Root,
                "Nenhum espaco ainda.\n\nToque em Capturar com camera para comecar um scan 360 do ambiente.",
                T.BodySize, T.TextMuted, TextAnchor.MiddleCenter, true);
            Center(_empty.rectTransform);
        }

        public override void OnShow() => Rebuild();

        private void OnNewSpace()
        {
            Ctx.Haptics.Trigger(HapticType.ImpactLight);
            Ctx.Flow.Request(ScreenId.Capture);
        }

#if UNITY_EDITOR
        private void OnDemoViewer()
        {
            Ctx.CurrentSpace = new CoreSpace
            {
                Id = "demo",
                Name = "Demo (Editor Splat Asset)",
                Status = "Ready",
                SplatPath = "",
                CreatedAtUnix = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            Ctx.Haptics.Trigger(HapticType.Success);
            Ctx.Flow.Request(ScreenId.Viewer);
        }
#endif

        private void Rebuild()
        {
            for (int i = _list.childCount - 1; i >= 0; i--) Destroy(_list.GetChild(i).gameObject);
            var spaces = Ctx.Library.All();
            _empty.gameObject.SetActive(spaces.Count == 0);
            foreach (var s in spaces) AddCard(s);
        }

        private void AddCard(CoreSpace s)
        {
            var rt = UIFactory.Card(_list, "card", T.Surface, T.RadiusM, elevation: true);
            rt.sizeDelta = new Vector2(0, T.CardHeight);
            rt.gameObject.AddComponent<Button>().onClick.AddListener(() => Open(s));
            UITween.Pop(rt, (_list.childCount - 1) * 0.05f);

            var accent = UIFactory.Image(rt, "accent", s.Status == "Ready" ? T.Success : T.Primary);
            var art = accent.rectTransform;
            art.anchorMin = new Vector2(0, 0); art.anchorMax = new Vector2(0, 1);
            art.pivot = new Vector2(0, 0.5f); art.sizeDelta = new Vector2(4, 0);

            var name = UIFactory.Text(rt, s.Name, T.BodySize, T.Text, TextAnchor.MiddleLeft, fit: false);
            name.rectTransform.anchorMin = new Vector2(0, 0.55f); name.rectTransform.anchorMax = new Vector2(0.75f, 0.55f);
            name.rectTransform.pivot = new Vector2(0, 0.5f);
            name.rectTransform.anchoredPosition = new Vector2(T.SpaceM + 8, 0);
            name.rectTransform.sizeDelta = new Vector2(0, 28);

            var sub = UIFactory.Text(rt, FormatDate(s), T.CaptionSize, T.TextMuted, TextAnchor.MiddleLeft, fit: false);
            sub.rectTransform.anchorMin = new Vector2(0, 0.35f); sub.rectTransform.anchorMax = new Vector2(0.75f, 0.35f);
            sub.rectTransform.pivot = new Vector2(0, 0.5f);
            sub.rectTransform.anchoredPosition = new Vector2(T.SpaceM + 8, 0);
            sub.rectTransform.sizeDelta = new Vector2(0, 20);

            var badge = StatusBadgeView.Create(rt, StatusLabel(s.Status), StatusColor(s.Status));
            badge.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0.5f);
            badge.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.5f);
            badge.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);
            badge.GetComponent<RectTransform>().anchoredPosition = new Vector2(-T.SpaceM, 0);
            badge.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 26);
        }

        private void Open(CoreSpace s)
        {
            Ctx.CurrentSpace = s;
            Ctx.Haptics.Trigger(HapticType.Selection);
            Ctx.Flow.Request(ScreenId.Viewer);
        }

        private static string FormatDate(CoreSpace s)
        {
            if (s.CreatedAtUnix <= 0) return "Espaco capturado";
            return System.DateTimeOffset.FromUnixTimeSeconds(s.CreatedAtUnix).LocalDateTime.ToString("dd/MM/yyyy HH:mm");
        }

        private static string StatusLabel(string status) =>
            status == "Ready" ? "Pronto" : status == "Failed" ? "Falhou" : "Processando";

        private static Color StatusColor(string status) =>
            status == "Ready" ? T.Success : status == "Failed" ? T.Danger : T.Warning;

        private void FooterButton(string label, UnityEngine.Events.UnityAction onClick, float top = 0, bool accent = true)
        {
            var go = new GameObject("footer", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(Root, false);
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0); rt.sizeDelta = new Vector2(0, MobileUi.IsPhone ? 130f : 110f);
            rt.anchoredPosition = new Vector2(0, top);
            var btn = UIFactory.Button(rt, label, onClick, accent);
            Stretch(btn);
            btn.offsetMin = new Vector2(T.SpaceL, T.SpaceS);
            btn.offsetMax = new Vector2(-T.SpaceL, -T.SpaceS);
        }

        private static void Center(RectTransform rt)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(520, 220);
        }
    }
}
