using System.Collections.Generic;
using TeleportNative.Capture;
using TeleportNative.Core;
using TeleportNative.Network;
using TeleportNative.Performance;
using TeleportNative.Rendering;
using UnityEngine;
using IAppLogger = TeleportNative.Core.ILogger;
using CoreSpace = TeleportNative.Core.Space;

namespace TeleportNative.UI
{
    /// <summary>
    /// Raiz de composicao (Dependency Holder): instancia unica montada pelo AppBootstrap com
    /// todos os serviços. Evita singletons espalhados; telas/controladores recehem isto.
    /// </summary>
    public sealed class AppContext
    {
        // Cross-cutting / puro
        public readonly IAppLogger Log;
        public readonly IRuntimeConfig Config;
        public readonly IHaptics Haptics;
        public readonly IDeviceProfiler Profiler;
        public readonly FramePacer Pacer;
        public readonly AppFlow Flow;

        // Infrastructure / Network
        public readonly IStorage Storage;
        public readonly RestClient Rest;
        public readonly IReconstructionProvider Provider;
        public readonly ISplatCache SplatCache;
        public readonly ISceneRepository Library;
        public readonly ReconstructionClient Reconstruction;

        // Scene-bound (MonoBehaviour)
        public readonly ICaptureSession Capture;
        public readonly ISplatViewer Viewer;
        public readonly SplatCameraController Camera;
        public readonly GameObject ArRig;
        public readonly GameObject ViewerRig;

        // Runtime state compartilhado entre telas
        public IReadOnlyList<CapturedFrame> LastCapture;
        public string PendingName;
        public CoreSpace CurrentSpace;

        public AppContext(
            IAppLogger log, IRuntimeConfig config, IHaptics haptics,
            IDeviceProfiler profiler, FramePacer pacer, AppFlow flow,
            IStorage storage, RestClient rest, IReconstructionProvider provider,
            ISplatCache splatCache, ISceneRepository library, ReconstructionClient reconstruction,
            ICaptureSession capture, ISplatViewer viewer, SplatCameraController camera,
            GameObject arRig, GameObject viewerRig)
        {
            Log = log; Config = config; Haptics = haptics;
            Profiler = profiler; Pacer = pacer; Flow = flow;
            Storage = storage; Rest = rest; Provider = provider;
            SplatCache = splatCache; Library = library; Reconstruction = reconstruction;
            Capture = capture; Viewer = viewer; Camera = camera;
            ArRig = arRig; ViewerRig = viewerRig;
        }
    }
}
