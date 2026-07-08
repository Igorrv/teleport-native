using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace TeleportNative.Capture
{
    /// <summary>Reinicia sessao AR apos permissao de camera concedida.</summary>
    public sealed class ArSessionStarter : MonoBehaviour
    {
        private ARSession _session;

        private void Awake()
        {
            _session = GetComponent<ARSession>();
            if (_session == null)
                _session = FindFirstObjectByType<ARSession>();
        }

        public void EnsureRunning()
        {
            if (_session == null)
            {
                Debug.LogWarning("[Teleport] ARSession ausente — verifique cena Main + ARKit.");
                return;
            }

            _session.enabled = true;
            if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
                ARSession.CheckAvailability();
            ARSession.Reset();
            Debug.Log("[Teleport] ARSession reset (" + ARSession.state + ")");
        }
    }
}
