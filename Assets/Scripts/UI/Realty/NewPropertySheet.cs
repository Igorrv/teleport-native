using System.Collections.Generic;
using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;
using TeleportNative.Core;

namespace TeleportNative.UI
{
    /// <summary>
    /// Modal premium "Novo imovel": titulo + endereco + selecao multipla de comodos. Ao confirmar,
    /// cria um <see cref="RealtyDraft"/> em Ctx e dispara a captura do primeiro comodo. Esta e a
    /// porta de entrada do fluxo de corretores/imobiliarias (escanear comodos -> tour 3D).
    /// Adicionado como overlay (filho do Root da tela), fecha destruindo o GameObject.
    /// </summary>
    public sealed class NewPropertySheet : MonoBehaviour
    {
        private AppContext _ctx;
        private System.Action<RealtyDraft> _onStart;
        private readonly List<Row> _rows = new();
        private InputField _title;
        private InputField _address;

        private sealed class Row
        {
            public RoomType Type;
            public Image Bg, Dot;
            public Text Label;
            public bool Selected;
        }

        public RectTransform Root => (RectTransform)transform;

        public static NewPropertySheet Open(Transform parent, AppContext ctx, System.Action<RealtyDraft> onStart)
        {
            var go = new GameObject("NewPropertySheet", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            UIFactory.Stretch(rt);
            var dim = go.GetComponent<Image>();
            dim.sprite = UIFactory.White();
            dim.color = T.Overlay;

            var sheet = go.AddComponent<NewPropertySheet>();
            sheet._ctx = ctx;
            sheet._onStart = onStart;
            sheet.Build();
            return sheet;
        }

        private void Build()
        {
            const float H = 720f;

            var card = UIFactory.Card(Root, "card", T.Surface, T.RadiusXL, elevation: true);
            var crt = card;
            crt.anchorMin = new Vector2(0, 0); crt.anchorMax = new Vector2(1, 0);
            crt.pivot = new Vector2(0.5f, 0f); crt.sizeDelta = new Vector2(0, H);

            float cursor = T.SpaceL; // distancia do topo (cresce p/ baixo)

            var title = UIFactory.Text(card, "Novo imóvel", T.TitleSize, T.Text, TextAnchor.MiddleLeft, fit: false);
            cursor = PlaceTop(title.rectTransform, cursor, 40);

            _title = UIFactory.TextField(card, "Título (ex.: Casa — Rua das Flores)");
            cursor = PlaceTop((RectTransform)_title.transform, cursor, T.ButtonHeight);

            _address = UIFactory.TextField(card, "Endereço (opcional)");
            cursor = PlaceTop((RectTransform)_address.transform, cursor, T.ButtonHeight);
            cursor += T.SpaceM;

            var roomsLbl = UIFactory.Text(card, "CÔMODOS PARA ESCANEAR", T.CaptionSize, T.TextMuted, TextAnchor.MiddleLeft, fit: false);
            float listTop = cursor + 24;
            PlaceTop(roomsLbl.rectTransform, cursor, 22);

            var list = UIFactory.ScrollList(card, "rooms", listTop, 120, T.Surface);

            var common = new HashSet<RoomType>
            {
                RoomType.Sala, RoomType.Cozinha, RoomType.Banheiro, RoomType.Quarto
            };
            foreach (var info in RoomCatalog.All)
                AddRow(list, info, common.Contains(info.Type));

            var cancel = UIFactory.Button(card, "Cancelar", Close, primary: false);
            PlaceBottom(cancel, left: true);
            var start = UIFactory.Button(card, "Começar captura", OnStart, primary: true);
            PlaceBottom(start, left: false);
        }

        private void AddRow(RectTransform parent, RoomCatalog.Info info, bool selected)
        {
            var row = UIFactory.Card(parent, "r", selected ? T.PrimarySoft : T.SurfaceRaised, T.RadiusS, elevation: false);
            row.sizeDelta = new Vector2(0, T.TouchTarget);
            var btn = row.gameObject.AddComponent<Button>();

            var lbl = UIFactory.Text(row, info.Label, T.BodySize, selected ? T.Text : T.TextSecondary,
                                     TextAnchor.MiddleLeft, fit: false);
            var lrt = lbl.rectTransform;
            lrt.anchorMin = new Vector2(0, 0); lrt.anchorMax = new Vector2(1, 1);
            lrt.pivot = new Vector2(0, 0.5f);
            lrt.offsetMin = new Vector2(T.SpaceM, 0); lrt.offsetMax = new Vector2(-56, 0);

            var dotGo = new GameObject("dot", typeof(Image));
            dotGo.transform.SetParent(row, false);
            var dot = dotGo.GetComponent<Image>();
            dot.sprite = UIFactory.RoundedSprite(9);
            dot.type = global::UnityEngine.UI.Image.Type.Sliced;
            dot.color = selected ? T.Primary : T.Divider;
            var drt = (RectTransform)dotGo.transform;
            drt.anchorMin = new Vector2(1, 0.5f); drt.anchorMax = new Vector2(1, 0.5f);
            drt.pivot = new Vector2(1, 0.5f);
            drt.sizeDelta = new Vector2(20, 20);
            drt.anchoredPosition = new Vector2(-T.SpaceM, 0);

            var state = new Row
            {
                Type = info.Type,
                Bg = row.GetComponent<Image>(),
                Dot = dot,
                Label = lbl,
                Selected = selected
            };
            btn.onClick.AddListener(() => Toggle(state));
            _rows.Add(state);
        }

        private void Toggle(Row r)
        {
            r.Selected = !r.Selected;
            r.Bg.color = r.Selected ? T.PrimarySoft : T.SurfaceRaised;
            r.Dot.color = r.Selected ? T.Primary : T.Divider;
            r.Label.color = r.Selected ? T.Text : T.TextSecondary;
            _ctx.Haptics.Trigger(HapticType.Selection);
        }

        private void OnStart()
        {
            var title = string.IsNullOrWhiteSpace(_title.text) ? "Imóvel sem título" : _title.text.Trim();
            var selected = new List<RoomType>();
            foreach (var r in _rows) if (r.Selected) selected.Add(r.Type);
            if (selected.Count == 0) // fallback: comodos mais comuns
                selected = new List<RoomType> { RoomType.Sala, RoomType.Cozinha, RoomType.Banheiro, RoomType.Quarto };

            var draft = new RealtyDraft(title, _address.text, selected);
            _ctx.RealtyDraft = draft;
            _ctx.PendingName = draft.NextLabel();
            _ctx.Haptics.Trigger(HapticType.Success);

            var cb = _onStart;
            Close();
            cb?.Invoke(draft);
        }

        private void Close()
        {
            _ctx.Haptics.Trigger(HapticType.Selection);
            Destroy(gameObject);
        }

        // ---- helpers de layout ancorado (topo/base do card) ----
        private static float PlaceTop(RectTransform rt, float fromTop, float h)
        {
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);
            rt.offsetMin = new Vector2(T.SpaceL, -(fromTop + h));
            rt.offsetMax = new Vector2(-T.SpaceL, -fromTop);
            return fromTop + h + T.SpaceS;
        }

        private static void PlaceBottom(RectTransform rt, bool left)
        {
            rt.anchorMin = new Vector2(left ? 0f : 0.5f, 0f);
            rt.anchorMax = new Vector2(left ? 0.5f : 1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.offsetMin = new Vector2(T.SpaceS, T.SpaceS);
            rt.offsetMax = new Vector2(-T.SpaceS, T.ButtonHeight + T.SpaceS);
        }
    }
}
