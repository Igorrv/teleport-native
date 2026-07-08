using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;

namespace TeleportNative.UI
{
    /// <summary>Componentes visuais compartilhados (header, chips, painel inferior).</summary>
    public static class UiChrome
    {
        public static RectTransform Header(Transform parent, string title, string subtitle = null)
        {
            var go = new GameObject("header", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, string.IsNullOrEmpty(subtitle) ? 72 : 96);
            go.GetComponent<Image>().sprite = UIFactory.White();
            go.GetComponent<Image>().color = new Color(T.Background.r, T.Background.g, T.Background.b, 0.92f);

            var titleT = UIFactory.Text(rt, title, T.HeadingSize, T.Text, TextAnchor.MiddleLeft, fit: false);
            titleT.rectTransform.anchorMin = new Vector2(0, 1); titleT.rectTransform.anchorMax = new Vector2(1, 1);
            titleT.rectTransform.pivot = new Vector2(0, 1);
            titleT.rectTransform.anchoredPosition = new Vector2(T.SpaceL, -T.SpaceM);
            titleT.rectTransform.sizeDelta = new Vector2(-T.SpaceL * 2, 32);

            if (!string.IsNullOrEmpty(subtitle))
            {
                var sub = UIFactory.Text(rt, subtitle, T.CaptionSize, T.TextMuted, TextAnchor.MiddleLeft, fit: false);
                sub.rectTransform.anchorMin = new Vector2(0, 1); sub.rectTransform.anchorMax = new Vector2(1, 1);
                sub.rectTransform.pivot = new Vector2(0, 1);
                sub.rectTransform.anchoredPosition = new Vector2(T.SpaceL, -44);
                sub.rectTransform.sizeDelta = new Vector2(-T.SpaceL * 2, 24);
            }
            return rt;
        }

        public static RectTransform BottomSheet(Transform parent, float height)
        {
            var rt = UIFactory.Panel(parent, "bottomSheet", new Color(T.Surface.r, T.Surface.g, T.Surface.b, 0.94f));
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(0, 0); rt.offsetMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, height);
            return rt;
        }

        public static Text StepChip(Transform parent, string label, bool active)
        {
            var go = new GameObject("chip", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(0, 28);
            var img = go.GetComponent<Image>();
            img.sprite = UIFactory.White();
            img.color = active ? T.Primary : T.SurfaceRaised;
            var t = UIFactory.Text(rt, label, T.CaptionSize, active ? Color.white : T.TextMuted, TextAnchor.MiddleCenter, fit: false);
            UIFactory.Stretch(t.rectTransform);
            return t;
        }
    }
}
