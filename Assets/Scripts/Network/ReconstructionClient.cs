using System;
using System.Threading;
using TeleportNative.Core;
using UnityEngine;
using CoreSpace = TeleportNative.Core.Space;
using IAppLogger = TeleportNative.Core.ILogger;

namespace TeleportNative.Network
{
    /// <summary>
    /// Orquestra o pipeline completo: criar job -> pollar ate Ready (backoff) -> baixar splat
    /// p/ cache -> persistir Space na Library. Reporta progresso via IProgress<ReconstructionStatus>.
    /// Classe pura (nao-MonoBehaviour); chamada a partir de um contexto async (Awaitable).
    /// </summary>
    public sealed class ReconstructionClient
    {
        private readonly IReconstructionProvider _provider;
        private readonly ISplatCache _cache;
        private readonly ISceneRepository _repo;
        private readonly IAppLogger _log;

        public ReconstructionClient(IReconstructionProvider provider, ISplatCache cache, ISceneRepository repo, IAppLogger log)
        { _provider = provider; _cache = cache; _repo = repo; _log = log; }

        public async Awaitable<Result<CoreSpace>> RunAsync(
            ReconstructionRequest req, string spaceName,
            IProgress<ReconstructionStatus> progress, CancellationToken ct)
        {
            _log.Info("Recon", $"criando job ({req.Frames.Count} keyframes, provider={req.ProviderKey})...");
            progress?.Report(new ReconstructionStatus { State = ReconstructionState.Uploading, Progress = 0.05f });

            var job = await _provider.CreateJobAsync(req, ct);
            if (!job.IsSuccess) return Result<CoreSpace>.Fail($"upload: {job.Error}");
            progress?.Report(new ReconstructionStatus { JobId = job.Value, State = ReconstructionState.Queued, Progress = 0.10f });

            var polled = await PollAsync(req, job.Value, progress, ct);
            if (!polled.IsSuccess) return Result<CoreSpace>.Fail(polled.Error);

            var scene = await _provider.GetSceneAsync(req, job.Value, ct);
            if (!scene.IsSuccess) return Result<CoreSpace>.Fail($"resultado: {scene.Error}");
            if (string.IsNullOrEmpty(scene.Value.SplatUrl))
                return Result<CoreSpace>.Fail("resultado sem splat_url");

            progress?.Report(new ReconstructionStatus { JobId = job.Value, State = ReconstructionState.Ready, Progress = 0.95f });
            var local = await _cache.EnsureLocalAsync(scene.Value.SplatUrl, job.Value, null, ct);
            if (!local.IsSuccess) return Result<CoreSpace>.Fail($"download splat: {local.Error}");

            var space = new CoreSpace
            {
                Id = job.Value,
                Name = string.IsNullOrEmpty(spaceName) ? DefaultName() : spaceName,
                Status = ReconstructionState.Ready.ToString(),
                SplatPath = local.Value,
                ThumbnailPath = "",
                SceneUrl = $"{req.BackendBaseUrl}/world/{job.Value}",
                CreatedAtUnix = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds(),
                SizeBytes = 0
            };
            _repo.Save(space);
            _log.Info("Recon", $"pronto: {space.Id} -> {space.SplatPath}");
            progress?.Report(new ReconstructionStatus { JobId = job.Value, State = ReconstructionState.Ready, Progress = 1f });
            return Result<CoreSpace>.Ok(space);
        }

        private async Awaitable<Result<ReconstructionStatus>> PollAsync(
            ReconstructionRequest req, string jobId, IProgress<ReconstructionStatus> progress, CancellationToken ct)
        {
            float delay = 2f;
            while (true)
            {
                var s = await _provider.GetStatusAsync(req, jobId, ct);
                if (!s.IsSuccess) return Result<ReconstructionStatus>.Fail(s.Error);

                var st = s.Value;
                st.JobId = jobId;
                progress?.Report(st);

                if (st.State == ReconstructionState.Ready) return Result<ReconstructionStatus>.Ok(st);
                if (st.State == ReconstructionState.Failed) return Result<ReconstructionStatus>.Fail(st.Error ?? "reconstrucao falhou");

                await Awaitable.WaitForSecondsAsync(delay, ct);
                delay = Mathf.Min(delay * 1.5f, 15f);
            }
        }

        private static string DefaultName() => "Espaco " + DateTime.UtcNow.ToString("dd/MM HH:mm");
    }
}
