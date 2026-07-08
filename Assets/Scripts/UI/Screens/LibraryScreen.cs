using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;
using TeleportNative.Core;
using CoreSpace = TeleportNative.Core.Space;
using ScreenId = TeleportNative.Core.AppScreen;

namespace TeleportNative.UI
{
    /// <summary>Home: imoveis capturados + CTA Novo imovel + draft em construcao.</summary>
    public sealed class LibraryScreen : AppScreen
    {
        private RectTransform _list;
        private GameObject _emptyRoot;

        protected override void Build()
        {
            UIFactory.Panel(Root, "bg", T.Background);
            UiChrome.Header(Root, "Seus imóveis", "Capture cômodos e gere tours 3D para seus clientes");

            FooterButton("Novo imóvel", OnNewProperty, accent: true);
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

            BuildEmptyState();
        }

        private void BuildEmptyState()
        {
            _emptyRoot = new GameObject("empty", typeof(RectTransform));
            var ert = (RectTransform)_emptyRoot.transform;
            ert.SetParent(Root, false);
            ert.anchorMin = ert.anchorMax = new Vector2(0.5f, 0.5f);
            ert.pivot = new Vector2(0.5f, 0.5f);
            ert.sizeDelta = new Vector2(520, 280);

            var card = UIFactory.Card(ert, "emptyCard", T.Surface, T.RadiusL, elevation: true);
            Stretch(card);
            card.offsetMin = new Vector2(T.SpaceL, 0);
            card.offsetMax = new Vector2(-T.SpaceL, 0);

            var col = UIFactory.Column(card, "col", T.SpaceM, T.SpaceL);
            Stretch(col);

            UIFactory.Text(col, "Nenhum imóvel ainda", T.HeadingSize, T.Text, TextAnchor.MiddleCenter);
            UIFactory.Text(col,
                "Toque em \"Novo imóvel\" para escolher os cômodos, escanear e gerar o tour 3D.",
                T.BodySize, T.TextMuted, TextAnchor.MiddleCenter, true);
            UIFactory.Button(col, "Criar primeiro imóvel", OnNewProperty, true)
                .sizeDelta = new Vector2(0, T.ButtonHeight);
        }

        public override void OnShow() => Rebuild();

        private void OnNewProperty()
        {
            Ctx.Haptics.Trigger(HapticType.ImpactLight);
            NewPropertySheet.Open(Root, Ctx, _ => Ctx.Flow.Request(ScreenId.Capture));
        }

        private void OnCaptureRoom()
        {
            var d = Ctx.RealtyDraft;
            if (d == null || !d.HasNext) return;
            Ctx.PendingName = d.NextLabel();
            Ctx.Haptics.Trigger(HapticType.ImpactLight);
            Ctx.Flow.Request(ScreenId.Capture);
        }

        private void OnAdvanceRoom()
        {
            var d = Ctx.RealtyDraft;
            if (d == null) return;
            d.Advance();
            Ctx.Haptics.Trigger(HapticType.Selection);
            if (!d.HasNext) { OnFinalizeDraft(); return; }
            Rebuild();
        }

        private void OnFinalizeDraft()
        {
            Ctx.RealtyDraft = null;
            Ctx.Haptics.Trigger(HapticType.Success);
            Rebuild();
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
            var hasDraft = Ctx.RealtyDraft != null && Ctx.RealtyDraft.HasNext;
            if (_emptyRoot != null) _emptyRoot.SetActive(spaces.Count == 0 && !hasDraft);
            if (hasDraft) AddDraftBanner();
            foreach (var s in spaces) AddCard(s);
        }

        private void AddDraftBanner()
        {
            var d = Ctx.RealtyDraft;
            var rt = UIFactory.Glass(_list, "draft", T.RadiusM);
            rt.sizeDelta = new Vector2(0, 168);
            UITween.Pop(rt, 0f);

            var accent = UIFactory.ColorBlock(rt, "accent", T.Accent);
            var art = accent.rectTransform;
            art.anchorMin = new Vector2(0, 0); art.anchorMax = new Vector2(0, 1);
            art.pivot = new Vector2(0, 0.5f); art.sizeDelta = new Vector2(4, 0);

            var badge = UIFactory.Text(rt, "EM CONSTRUÇÃO", T.MicroSize, T.Accent, TextAnchor.MiddleLeft, fit: false);
            badge.rectTransform.anchorMin = new Vector2(0, 1); badge.rectTransform.anchorMax = new Vector2(1, 1);
            badge.rectTransform.offsetMin = new Vector2(T.SpaceM + 10, -22);
            badge.rectTransform.offsetMax = new Vector2(-T.SpaceM, -4);

            var name = UIFactory.Text(rt, d.Title, T.BodySize, T.Text, TextAnchor.MiddleLeft, fit: false);
            name.rectTransform.anchorMin = new Vector2(0, 1); name.rectTransform.anchorMax = new Vector2(1, 1);
            name.rectTransform.offsetMin = new Vector2(T.SpaceM + 10, -48);
            name.rectTransform.offsetMax = new Vector2(-T.SpaceM, -24);

            var info = RoomCatalog.Of(d.NextType);
            float pct = d.Total > 0 ? (float)d.Done / d.Total : 0f;
            var fill = UiChrome.ProgressBar(rt, 6f);
            var prt = fill.rectTransform.parent as RectTransform;
            prt.anchorMin = new Vector2(0, 1); prt.anchorMax = new Vector2(1, 1);
            prt.pivot = new Vector2(0.5f, 1f);
            prt.anchoredPosition = new Vector2(0, -62);
            prt.offsetMin = new Vector2(T.SpaceM + 10, prt.offsetMin.y);
            prt.offsetMax = new Vector2(-T.SpaceM, prt.offsetMax.y);
            prt.sizeDelta = new Vector2(0, 6);
            UiChrome.SetProgress(fill, pct);

            var sub = UIFactory.Text(rt, $"{d.Done}/{d.Total} · próximo: {info.Label}",
                T.CaptionSize, T.TextSecondary, TextAnchor.MiddleLeft, fit: false);
            sub.rectTransform.anchorMin = new Vector2(0, 1); sub.rectTransform.anchorMax = new Vector2(1, 1);
            sub.rectTransform.offsetMin = new Vector2(T.SpaceM + 10, -86);
            sub.rectTransform.offsetMax = new Vector2(-T.SpaceM, -66);

            var capture = UIFactory.Button(rt, "Capturar " + info.Label, OnCaptureRoom, true);
            capture.anchorMin = new Vector2(0, 0); capture.anchorMax = new Vector2(0.58f, 0);
            capture.offsetMin = new Vector2(T.SpaceM, 12f);
            capture.offsetMax = new Vector2(-4f, 12f + T.TouchTarget);

            var advance = UIFactory.Button(rt, "Pular ›", OnAdvanceRoom, false);
            advance.anchorMin = new Vector2(0.60f, 0); advance.anchorMax = new Vector2(1, 0);
            advance.offsetMin = new Vector2(4f, 12f);
            advance.offsetMax = new Vector2(-T.SpaceM, 12f + T.TouchTarget);
        }

        private void AddCard(CoreSpace s)
        {
            var rt = UIFactory.Card(_list, "card", T.Surface, T.RadiusM, elevation: true);
            rt.sizeDelta = new Vector2(0, T.CardHeight + 8);
            rt.gameObject.AddComponent<Button>().onClick.AddListener(() => Open(s));
            UITween.Pop(rt, (_list.childCount - 1) * 0.05f);

            var accent = UIFactory.ColorBlock(rt, "accent", s.Status == "Ready" ? T.Success : T.Primary);
            var art = accent.rectTransform;
            art.anchorMin = new Vector2(0, 0); art.anchorMax = new Vector2(0, 1);
            art.pivot = new Vector2(0, 0.5f); art.sizeDelta = new Vector2(4, 0);

            var name = UIFactory.Text(rt, s.Name, T.BodySize, T.Text, TextAnchor.MiddleLeft, fit: false);
            name.rectTransform.anchorMin = new Vector2(0, 0.58f); name.rectTransform.anchorMax = new Vector2(0.72f, 0.58f);
            name.rectTransform.pivot = new Vector2(0, 0.5f);
            name.rectTransform.anchoredPosition = new Vector2(T.SpaceM + 8, 0);
            name.rectTransform.sizeDelta = new Vector2(0, 28);

            var sub = UIFactory.Text(rt, FormatDate(s), T.CaptionSize, T.TextMuted, TextAnchor.MiddleLeft, fit: false);
            sub.rectTransform.anchorMin = new Vector2(0, 0.32f); sub.rectTransform.anchorMax = new Vector2(0.72f, 0.32f);
            sub.rectTransform.pivot = new Vector2(0, 0.5f);
            sub.rectTransform.anchoredPosition = new Vector2(T.SpaceM + 8, 0);
            sub.rectTransform.sizeDelta = new Vector2(0, 20);

            var badge = StatusBadgeView.Create(rt, StatusLabel(s.Status), StatusColor(s.Status));
            var brt = badge.GetComponent<RectTransform>();
            brt.anchorMin = brt.anchorMax = new Vector2(1, 0.62f);
            brt.pivot = new Vector2(1, 0.5f);
            brt.anchoredPosition = new Vector2(-T.SpaceM, 0);
            brt.sizeDelta = new Vector2(110, 26);

            var del = UIFactory.Button(rt, "Excluir", () => DeleteSpace(s), false);
            del.anchorMin = del.anchorMax = new Vector2(1, 0.28f);
            del.pivot = new Vector2(1, 0.5f);
            del.anchoredPosition = new Vector2(-T.SpaceM, 0);
            del.sizeDelta = new Vector2(88, 32);
        }

        private void DeleteSpace(CoreSpace s)
        {
            if (s == null || string.IsNullOrEmpty(s.Id)) return;
            Ctx.Library.Delete(s.Id);
            if (Ctx.CurrentSpace != null && Ctx.CurrentSpace.Id == s.Id)
                Ctx.CurrentSpace = null;
            Ctx.Haptics.Trigger(HapticType.Warning);
            Rebuild();
        }

        private void Open(CoreSpace s)
        {
            Ctx.CurrentSpace = s;
            Ctx.Haptics.Trigger(HapticType.Selection);
            Ctx.Flow.Request(ScreenId.Viewer);
        }

        private static string FormatDate(CoreSpace s)
        {
            if (s.CreatedAtUnix <= 0) return "Espaço capturado";
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
    }
}
