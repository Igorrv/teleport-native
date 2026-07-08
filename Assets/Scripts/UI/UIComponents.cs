using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TeleportNative.UI
{
    /// <summary>Barra de progresso (0..1). Track + fill ancorado a esquerda.</summary>
    public sealed class ProgressBarView : MonoBehaviour
    {
        private RectTransform _fill;
        public float Value { set => _fill.anchorMax = new Vector2(Mathf.Clamp01(value), 1f); }

        public static ProgressBarView Create(Transform parent)
        {
            var root = UIFactory.Panel(parent, "progress", T.SurfaceRaised);
            var fillGo = new GameObject("fill", typeof(Image));
            fillGo.transform.SetParent(root, false);
            var frt = (RectTransform)fillGo.transform;
            frt.anchorMin = Vector2.zero; frt.anchorMax = new Vector2(0, 1f);
            frt.offsetMin = frt.offsetMax = Vector2.zero; frt.pivot = new Vector2(0f, 0.5f);
            fillGo.GetComponent<Image>().color = T.Primary;
            var bar = root.gameObject.AddComponent<ProgressBarView>();
            bar._fill = frt;
            root.anchorMin = new Vector2(0, 0.5f); root.anchorMax = new Vector2(1, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f); root.sizeDelta = new Vector2(0, 8);
            return bar;
        }
    }

    /// <summary>Badge de status (ex.: "PROCESSANDO") com cor semantica.</summary>
    public sealed class StatusBadgeView : MonoBehaviour
    {
        private Image _bg; private Text _label;
        public void Set(string text, Color color) { _label.text = text; _bg.color = color; }

        public static StatusBadgeView Create(Transform parent, string text, Color color)
        {
            var go = new GameObject("badge", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(-1, 26);
            var bg = go.GetComponent<Image>(); bg.sprite = UIFactory.White(); bg.color = color;
            var lbl = UIFactory.Text(rt, text, T.CaptionSize, Color.white, TextAnchor.MiddleCenter, wrap: false, fit: false);
            UIFactory.Stretch((RectTransform)lbl.transform);
            var b = go.AddComponent<StatusBadgeView>(); b._bg = bg; b._label = lbl;
            return b;
        }
    }

    /// <summary>Joystick virtual flutuante para caminhada no viewer. Expoe Value (x,y em -1..1).</summary>
    public sealed class JoystickView : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        private RectTransform _bg, _knob;
        private Vector2 _origin;
        public Vector2 Value { get; private set; }

        public static JoystickView Create(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject("joystick", typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(40, 40); rt.offsetMax = new Vector2(-40, -40);
            var bg = go.GetComponent<Image>(); bg.sprite = UIFactory.White(); bg.color = T.Overlay;

            var knobGo = new GameObject("knob", typeof(Image));
            var krt = (RectTransform)knobGo.transform;
            krt.SetParent(rt, false);
            krt.anchorMin = krt.anchorMax = new Vector2(0.5f, 0.5f);
            krt.sizeDelta = new Vector2(70, 70);
            knobGo.GetComponent<Image>().color = T.Primary;

            var j = go.AddComponent<JoystickView>(); j._bg = rt; j._knob = krt;
            return j;
        }

        public void OnPointerDown(PointerEventData e) { _origin = _bg.position; UpdateKnob(e); }
        public void OnDrag(PointerEventData e) => UpdateKnob(e);
        public void OnPointerUp(PointerEventData e) { Value = Vector2.zero; _knob.anchoredPosition = Vector2.zero; }

        private void UpdateKnob(PointerEventData e)
        {
            Vector2 delta = (Vector2)_bg.position - _origin;
            float radius = _bg.rect.width * 0.5f;
            Vector2 local = (e.position - _origin);
            if (local.magnitude > radius) local = local.normalized * radius;
            Value = local / radius;
            _knob.anchoredPosition = local;
        }
    }
}
