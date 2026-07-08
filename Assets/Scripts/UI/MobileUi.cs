using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TeleportNative.UI
{
    /// <summary>Configura Canvas, EventSystem e safe area para iPhone (UI legivel + toque).</summary>
    public static class MobileUi
    {
        public static bool IsPhone =>
            Application.isMobilePlatform ||
            (Screen.width < Screen.height && Screen.width <= 520);

        public static void Configure(Canvas canvas)
        {
            if (canvas == null) return;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(430f, 932f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0f;
                scaler.referencePixelsPerUnit = 100f;
            }

            canvas.pixelPerfect = false;
            EnsureTouchInput();
        }

        private static void EnsureTouchInput()
        {
            var es = Object.FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                _ = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                return;
            }

            if (es.GetComponent<StandaloneInputModule>() == null)
                es.gameObject.AddComponent<StandaloneInputModule>();

            es.sendNavigationEvents = false;
        }

        public static float TouchMin => IsPhone ? 52f : Core.DesignTokens.TouchTarget;
    }
}
