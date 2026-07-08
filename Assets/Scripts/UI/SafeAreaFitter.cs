using UnityEngine;

namespace TeleportNative.UI
{
    /// <summary>Aplica safe area do iPhone (notch/home indicator) ao Canvas.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect _last;

        private void Awake() => _rt = (RectTransform)transform;

        private void OnEnable() => Apply();

        private void Update()
        {
            if (_last != Screen.safeArea)
                Apply();
        }

        private void Apply()
        {
            _last = Screen.safeArea;
            var sa = Screen.safeArea;
            var full = new Rect(0, 0, Screen.width, Screen.height);
            if (sa == full) return;

            var canvas = GetComponentInParent<Canvas>();
            var scale = canvas != null && canvas.scaleFactor > 0 ? canvas.scaleFactor : 1f;

            var min = sa.position / scale;
            var max = (sa.position + sa.size) / scale;
            _rt.anchorMin = Vector2.zero;
            _rt.anchorMax = Vector2.one;
            _rt.offsetMin = new Vector2(min.x, min.y);
            _rt.offsetMax = new Vector2(-(Screen.width / scale - max.x), -(Screen.height / scale - max.y));
        }
    }
}
