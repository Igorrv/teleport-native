using System.Collections;
using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;

namespace TeleportNative.UI
{
    /// <summary>Banner/toast efemero para feedback sem reler telas ou abrir dialogs.</summary>
    public sealed class ToastView : MonoBehaviour
    {
        private Text _label;
        private Image _bg;
        private Coroutine _hide;

        public static ToastView Attach(RectTransform root)
        {
            var go = new GameObject("toast", typeof(RectTransform), typeof(Image), typeof(ToastView));
            var rt = (RectTransform)go.transform;
            rt.SetParent(root, false);
            rt.anchorMin = new Vector2(0.5f, 0); rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(680, 72);
            rt.anchoredPosition = new Vector2(0, T.SpaceL + 120);

            var view = go.GetComponent<ToastView>();
            view._bg = go.GetComponent<Image>();
            view._bg.sprite = UIFactory.White();
            view._bg.color = new Color(T.SurfaceRaised.r, T.SurfaceRaised.g, T.SurfaceRaised.b, 0.96f);
            view._label = UIFactory.Text(rt, "", T.CaptionSize, T.Text, TextAnchor.MiddleCenter, wrap: true, fit: false);
            UIFactory.Stretch(view._label.rectTransform);
            view._label.rectTransform.offsetMin = new Vector2(T.SpaceM, T.SpaceS);
            view._label.rectTransform.offsetMax = new Vector2(-T.SpaceM, -T.SpaceS);
            go.SetActive(false);
            return view;
        }

        public void Show(string message, Color? color = null, float seconds = 3f)
        {
            _label.text = message;
            _label.color = color ?? T.Text;
            gameObject.SetActive(true);
            if (_hide != null) StopCoroutine(_hide);
            _hide = StartCoroutine(HideAfter(seconds));
        }

        private IEnumerator HideAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            gameObject.SetActive(false);
            _hide = null;
        }
    }
}
