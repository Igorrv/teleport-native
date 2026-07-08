using System.Collections.Generic;
using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;

namespace TeleportNative.UI
{
    /// <summary>
    /// Fabrica de UI em codigo (uGUI) sobre DesignTokens premium. Cantos arredondados gerados em
    /// runtime (SDF -> sprite 9-slice), cards com elevacao, glass translucido. Permite telas
    /// premium sem autorar prefabs/sprites.
    /// </summary>
    public static class UIFactory
    {
        private static Sprite _white;
        private static readonly Dictionary<int, Sprite> _rounded = new();

        public static Sprite White()
        {
            if (_white == null)
            {
                var t = new Texture2D(1, 1);
                t.SetPixel(0, 0, Color.white);
                t.Apply();
                _white = Sprite.Create(t, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
            }
            return _white;
        }

        /// <summary>Sprite 9-slice com cantos arredondados (anti-alias via SDF). Cache por raio.</summary>
        public static Sprite RoundedSprite(int radius)
        {
            radius = Mathf.Max(2, radius);
            if (_rounded.TryGetValue(radius, out var cached)) return cached;

            int size = radius * 2 + 4;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var cols = new Color32[size * size];
            float c = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // SDF de rounded-box: q = |p-c| - half + r ; sd = len(max(q,0)) + min(max(qx,qy),0) - r
                    float qx = Mathf.Abs(x + 0.5f - c) - c + radius;
                    float qy = Mathf.Abs(y + 0.5f - c) - c + radius;
                    float ax = Mathf.Max(qx, 0f), ay = Mathf.Max(qy, 0f);
                    float sd = Mathf.Sqrt(ax * ax + ay * ay) + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                    float a = Mathf.Clamp01(0.5f - sd);
                    cols[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            }
            tex.SetPixels32(cols);
            tex.Apply();
            var border = new Vector4(radius, radius, radius, radius);
            var sp = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100, 1, SpriteMeshType.FullRect, border);
            _rounded[radius] = sp;
            return sp;
        }

        public static Font DefaultFont()
        {
            var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return f ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero; rt.pivot = new Vector2(0.5f, 0.5f);
        }

        public static RectTransform Panel(Transform parent, string name, Color bg, float padding = 0)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            Stretch(rt);
            var img = go.GetComponent<Image>();
            img.sprite = White(); img.color = bg;
            if (padding > 0) rt.offsetMin = rt.offsetMax = new Vector2(padding, padding);
            return rt;
        }

        /// <summary>Card arredondado com elevacao (sombra discreta). Define tamanho depois.</summary>
        public static RectTransform Card(Transform parent, string name, Color bg, float radius = T.RadiusM, bool elevation = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = RoundedSprite(Mathf.RoundToInt(radius));
            img.type = UnityEngine.UI.Image.Type.Sliced;
            img.color = bg;
            if (elevation)
            {
                var sh = go.AddComponent<Shadow>();
                sh.effectColor = new Color(0f, 0f, 0f, 0.45f);
                sh.effectDistance = new Vector2(0f, -6f);
            }
            return rt;
        }

        /// <summary>Painel "vidro" translucido com highlight superior (glassmorphism discreto).</summary>
        public static RectTransform Glass(Transform parent, string name, float radius = T.RadiusL)
        {
            var rt = Card(parent, name, T.Glass, radius, elevation: false);
            var img = rt.GetComponent<Image>();
            var hl = Image(rt, "glass-hl", T.GlassHighlight);
            var hrt = (RectTransform)hl.transform;
            hrt.anchorMin = new Vector2(0f, 1f); hrt.anchorMax = new Vector2(1f, 1f);
            hrt.pivot = new Vector2(0.5f, 1f); hrt.sizeDelta = new Vector2(0f, 1.5f);
            return rt;
        }

        public static Image Image(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = White(); img.color = color;
            return img;
        }

        public static Text Text(Transform parent, string content, float size, Color color, TextAnchor align = TextAnchor.UpperLeft, bool wrap = false, bool fit = true)
        {
            var go = new GameObject("txt", typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = content;
            t.font = DefaultFont();
            t.fontSize = Mathf.RoundToInt(size);
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            if (fit)
            {
                var csf = go.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            return t;
        }

        /// <summary>Layout vertical de tela cheia (stretch). childControlWidth on, height off.</summary>
        public static RectTransform ScreenLayout(Transform parent, string name, TextAnchor align, float spacing, float padding)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            Stretch(rt);
            var vg = go.GetComponent<VerticalLayoutGroup>();
            vg.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
            vg.spacing = spacing;
            vg.childAlignment = align;
            vg.childControlWidth = true;
            vg.childControlHeight = false;
            vg.childForceExpandWidth = true;
            vg.childForceExpandHeight = false;
            return rt;
        }

        /// <summary>Botao primario/secundario arredondado com sombra sutil. Devolve o root.</summary>
        public static RectTransform Button(Transform parent, string label, UnityEngine.Events.UnityAction onClick, bool primary = true)
        {
            var go = new GameObject("btn", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(0, T.ButtonHeight);
            var img = go.GetComponent<Image>();
            img.sprite = RoundedSprite(Mathf.RoundToInt(T.RadiusL));
            img.type = UnityEngine.UI.Image.Type.Sliced;
            img.color = primary ? T.Primary : T.SurfaceRaised;
            if (primary)
            {
                var sh = go.AddComponent<Shadow>();
                sh.effectColor = new Color(0.43f, 0.36f, 0.98f, 0.35f);
                sh.effectDistance = new Vector2(0f, -6f);
            }
            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(onClick);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = T.ButtonHeight;
            le.preferredHeight = T.ButtonHeight;

            var lbl = Text(rt, label, T.BodySize, primary ? Color.white : T.Text, TextAnchor.MiddleCenter, wrap: false, fit: false);
            Stretch((RectTransform)lbl.transform);
            return rt;
        }

        /// <summary>Campo de texto (uGUI InputField) arredondado, consistente com o tema premium.</summary>
        public static InputField TextField(Transform parent, string placeholder)
        {
            var go = new GameObject("input", typeof(RectTransform), typeof(Image), typeof(InputField));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(0, T.ButtonHeight);
            var img = go.GetComponent<UnityEngine.UI.Image>();
            img.sprite = RoundedSprite(Mathf.RoundToInt(T.RadiusS));
            img.type = UnityEngine.UI.Image.Type.Sliced;
            img.color = T.SurfaceRaised;

            var ph = TextFieldChild(rt, placeholder, T.TextMuted);
            var content = TextFieldChild(rt, string.Empty, T.Text);
            var inf = go.GetComponent<InputField>();
            inf.textComponent = content;
            inf.placeholder = ph;
            inf.text = string.Empty;
            inf.caretColor = T.Text;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = T.ButtonHeight;
            return inf;
        }

        private static Text TextFieldChild(RectTransform parent, string text, Color color)
        {
            var go = new GameObject(string.IsNullOrEmpty(text) ? "text" : "ph", typeof(Text));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            Stretch(rt);
            rt.offsetMin = new Vector2(T.SpaceM, 8);
            rt.offsetMax = new Vector2(-T.SpaceM, -8);
            var t = go.GetComponent<Text>();
            t.font = DefaultFont();
            t.fontSize = Mathf.RoundToInt(T.BodySize);
            t.color = color;
            t.alignment = TextAnchor.MiddleLeft;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.supportRichText = false;
            return t;
        }

        public static RectTransform Spacer(Transform parent, float height)
        {
            var go = new GameObject("spacer", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(0, height);
            return rt;
        }

        /// <summary>Lista rolavel: ScrollRect + Mask + coluna de conteudo. Devolve o content.</summary>
        public static RectTransform ScrollList(Transform parent, string name, float topInset, float bottomInset, Color bg)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            Stretch(rt);
            rt.offsetMax = new Vector2(0, -topInset);
            rt.offsetMin = new Vector2(0, bottomInset);
            var img = go.GetComponent<Image>();
            img.sprite = White(); img.color = bg;
            var sr = go.GetComponent<ScrollRect>();
            sr.horizontal = false; sr.scrollSensitivity = 20f;

            var vp = new GameObject("viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            var vrt = (RectTransform)vp.transform;
            vrt.SetParent(rt, false);
            Stretch(vrt);
            var vimg = vp.GetComponent<Image>(); vimg.sprite = White(); vimg.color = Color.clear;
            sr.viewport = vrt;

            var content = Column(vrt, "content", T.SpaceM, T.SpaceM);
            sr.content = content;
            return content;
        }

        /// <summary>Coluna com layout vertical simples (VerticalLayoutGroup).</summary>
        public static RectTransform Column(Transform parent, string name, float padding, float spacing)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            var vg = go.GetComponent<VerticalLayoutGroup>();
            vg.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
            vg.spacing = spacing;
            vg.childAlignment = TextAnchor.UpperCenter;
            vg.childControlWidth = true; vg.childForceExpandWidth = true;
            var csf = go.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rt;
        }
    }
}
