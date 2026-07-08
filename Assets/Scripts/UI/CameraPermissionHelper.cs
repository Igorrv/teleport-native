using System.Collections;
using UnityEngine;

namespace TeleportNative.UI
{
    /// <summary>Pede permissao de camera (iOS/Android) antes da captura AR.</summary>
    public static class CameraPermissionHelper
    {
        public static bool HasCamera =>
#if UNITY_IOS || UNITY_ANDROID
            Application.HasUserAuthorization(UserAuthorization.WebCam);
#else
            true;
#endif

        public static IEnumerator EnsureCamera(System.Action<bool> onDone = null)
        {
#if UNITY_IOS || UNITY_ANDROID
            if (!HasCamera)
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            onDone?.Invoke(HasCamera);
#else
            onDone?.Invoke(true);
            yield break;
#endif
        }
    }
}
