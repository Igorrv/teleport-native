using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;

namespace TeleportNative.UI
{
    /// <summary>Componentes visuais compartilhados (header, chips, painel inferior, progresso).</summary>
    public static class UiChrome
    {
        public static RectTransform Header(Transform parent, string title, string subtitle = null)
        {
            var go = new GameObject("header", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, string.IsNullOrEmpty(subtitle) ? 80 : 108);
            var bg = go.GetComponent<Image>();
            bg.sprite = UIFactory.White();
            bg.color = new Color(T.Background.r, T.Background.g, T.Background.b, 0.94f);

            // Linha accent inferior (identidade visual)
            var line = UIFactory.Image(rt, "accent-line", T.Primary);
            var lrt = line.rectTransform;
            lrt.anchorMin = new Vector2(0, 0); lrt.anchorMax = new Vector2(1, 0);
            lrt.pivot = new Vector2(0.5f, 0);
            lrt.sizeDelta = new Vector2(0, 2f);
            lrt.anchoredPosition = Vector2.zero;

            var titleT = UIFactory.Text(rt, title, T.HeadingSize, T.Text, TextAnchor.MiddleLeft, fit: false);
            titleT.rectTransform.anchorMin = new Vector2(0, 1); titleT.rectTransform.anchorMax = new Vector2(1, 1);
            titleT.rectTransform.pivot = new Vector2(0, 1);
            titleT.rectTransform.anchoredPosition = new Vector2(T.SpaceL, -T.SpaceM);
            titleT.rectTransform.sizeDelta = new Vector2(-T.SpaceL * 2, 34);

            if (!string.IsNullOrEmpty(subtitle))
            {
                var sub = UIFactory.Text(rt, subtitle, T.CaptionSize, T.TextMuted, TextAnchor.MiddleLeft, fit: false);
                sub.rectTransform.anchorMin = new Vector2(0, 1); sub.rectTransform.anchorMax = new Vector2(1, 1);
                sub.rectTransform.pivot = new Vector2(0, 1);
                sub.rectTransform.anchoredPosition = new Vector2(T.SpaceL, -50);
                sub.rectTransform.sizeDelta = new Vector2(-T.SpaceL * 2, 26);
            }
            return rt;
        }

        /// <summary>Atualiza titulo/subtitulo de um header ja criado (nome "header").</summary>
        public static void SetHeaderTexts(Transform header, string title, string subtitle)
        {
            if (header == null) return;
            var texts = header.GetComponentsInChildren<Text>(true);
            if (texts.Length > 0) texts[0].text = title ?? "";
            if (texts.Length > 1) texts[1].text = subtitle ?? "";
        }

        public static RectTransform BottomSheet(Transform parent, float height)
        {
            var go = new GameObject("bottomSheet", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, height);
            rt.anchoredPosition = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.sprite = UIFactory.RoundedSprite(Mathf.RoundToInt(T.RadiusL));
            img.type = Image.Type.Sliced;
            img.color = new Color(T.Surface.r, T.Surface.g, T.Surface.b, 0.96f);
            img.raycastTarget = true;

            // Handle visual (pill) no topo do sheet
            var handle = UIFactory.Image(rt, "handle", T.Divider);
            var hrt = handle.rectTransform;
            hrt.anchorMin = new Vector2(0.5f, 1); hrt.anchorMax = new Vector2(0.5f, 1);
            hrt.pivot = new Vector2(0.5f, 1);
            hrt.sizeDelta = new Vector2(40, 4);
            hrt.anchoredPosition = new Vector2(0, -10);
            handle.sprite = UIFactory.RoundedSprite(4);
            handle.type = Image.Type.Sliced;
            return rt;
        }

        /// <summary>Barra de progresso horizontal (track + fill). Devolve o fill Image.</summary>
        public static Image ProgressBar(Transform parent, float height = 6f)
        {
            var trackGo = new GameObject("progress", typeof(RectTransform), typeof(Image));
            var track = (RectTransform)trackGo.transform;
            track.SetParent(parent, false);
            track.sizeDelta = new Vector2(0, height);
            var trackImg = trackGo.GetComponent<Image>();
            trackImg.sprite = UIFactory.RoundedSprite(Mathf.RoundToInt(height));
            trackImg.type = Image.Type.Sliced;
            trackImg.color = T.SurfaceRaised;

            var fillGo = new GameObject("fill", typeof(RectTransform), typeof(Image));
            var fillRt = (RectTransform)fillGo.transform;
            fillRt.SetParent(track, false);
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;
            var fill = fillGo.GetComponent<Image>();
            fill.sprite = UIFactory.RoundedSprite(Mathf.RoundToInt(height));
            fill.type = Image.Type.Sliced;
            fill.color = T.Primary;
            return fill;
        }

        public static void SetProgress(Image fill, float t01)
        {
            if (fill == null) return;
            var rt = fill.rectTransform;
            rt.anchorMax = new Vector2(Mathf.Clamp01(t01), 1f);
        }

        public static Text StepChip(Transform parent, string label, bool active)
        {
            var go = new GameObject("chip", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(0, 28);
            var img = go.GetComponent<Image>();
            img.sprite = UIFactory.RoundedSprite(Mathf.RoundToInt(T.RadiusS));
            img.type = Image.Type.Sliced;
            img.color = active ? T.Primary : T.SurfaceRaised;
            var t = UIFactory.Text(rt, label, T.CaptionSize, active ? Color.white : T.TextMuted, TextAnchor.MiddleCenter, fit: false);
            UIFactory.Stretch(t.rectTransform);
            return t;
        }
    }
}
