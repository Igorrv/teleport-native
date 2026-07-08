using System.Collections.Generic;
using System.Threading;
using TeleportNative.Capture;
using TeleportNative.Core;
using UnityEngine;

namespace TeleportNative.Network
{
    /// <summary>Parametros de uma requisicao de reconstrucao (frames + config do provider).</summary>
    public sealed class ReconstructionRequest
    {
        public IReadOnlyList<CapturedFrame> Frames;
        public string BackendBaseUrl;  // gateway publico HTTPS do HomeView
        public string ProviderKey;     // luma | worldlabs | meshy
        public string ApiKey;
    }

    /// <summary>
    /// Provider de reconstrucao 3DGS. Implementacao padrao fala com o backend HomeView
    /// (/api/scan), que por dentro chama o provider externo. Trocavel p/ testes/outro backend.
    /// </summary>
    public interface IReconstructionProvider
    {
        string Key { get; }
        Awaitable<Result<string>> CreateJobAsync(ReconstructionRequest req, CancellationToken ct);
        Awaitable<Result<ReconstructionStatus>> GetStatusAsync(ReconstructionRequest req, string jobId, CancellationToken ct);
        Awaitable<Result<SceneContract>> GetSceneAsync(ReconstructionRequest req, string jobId, CancellationToken ct);
    }
}
