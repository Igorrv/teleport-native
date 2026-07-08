using UnityEngine;
using UnityEngine.UI;
using TeleportNative.Capture;
using TeleportNative.Core;
using TeleportNative.Network;
using TeleportNative.Performance;
using TeleportNative.Rendering;

namespace TeleportNative.UI
{
    /// <summary>
    /// Raiz de composicao (entry point). No Awake monta o AppContext (todos os servicos puros +
    /// os MonoBehaviour da cena), vincula o viewer e inicia o ScreenManager/AppFlow.
    /// Coloque este componente numa unica cena ("Main"), referenciando os rigs da cena.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class AppBootstrap : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Canvas _uiRoot;            // Canvas Screen Space - Overlay

        [Header("Rigs da cena")]
        [SerializeField] private ARCaptureSession _capture; // AR camera (ICaptureSession)
        [SerializeField] private SplatViewerController _viewer;
        [SerializeField] private SplatCameraController _camera;
        [SerializeField] private GameObject _arRig;         // GameObject do rig AR (liga/desliga)
        [SerializeField] private GameObject _viewerRig;     // GameObject do rig do viewer

        private void Awake() => Build();

        private void Build()
        {
            var log = UnityLogger.Instance;
            var config = RuntimeConfig.Load();
            var haptics = new Haptics();
            var profiler = new DeviceProfiler();
            var pacer = new FramePacer();
            var storage = new LocalStorage();
            var rest = new RestClient();
            var provider = new HomeViewReconstructionProvider(rest);
            var cache = new SplatCache(rest, storage);
            var library = new SceneRepository(storage);
            var recon = new ReconstructionClient(provider, cache, library, log);
            var flow = new AppFlow();

            _viewer.Bind(profiler, pacer, null); // adapter interno ao viewer

            var ctx = new AppContext(
                log, config, haptics, profiler, pacer, flow,
                storage, rest, provider, cache, library, recon,
                _capture, _viewer, _camera, _arRig, _viewerRig);

            _ = new ScreenManager(ctx, _uiRoot);
        }
    }
}
