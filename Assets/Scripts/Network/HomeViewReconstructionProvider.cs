using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeleportNative.Capture;
using TeleportNative.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace TeleportNative.Network
{
    /// <summary>
    /// IReconstructionProvider sobre o backend HomeView: POST /api/scan (multipart com frames +
    /// metadados de pose/intrinsics) -> polling /status -> /result (SceneContract com splat_url).
    /// </summary>
    public sealed class HomeViewReconstructionProvider : IReconstructionProvider
    {
        public string Key => "homeview";

        private readonly RestClient _rest;

        public HomeViewReconstructionProvider(RestClient rest) => _rest = rest;

        public async Awaitable<Result<string>> CreateJobAsync(ReconstructionRequest req, CancellationToken ct)
        {
            var form = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("provider", req.ProviderKey ?? "luma")
            };

            var meta = new FrameMeta[req.Frames.Count];
            for (int i = 0; i < req.Frames.Count; i++)
            {
                var f = req.Frames[i];
                form.Add(new MultipartFormFileSection("frames", f.Jpeg, $"frame_{i:D3}.jpg", "image/jpeg"));
                meta[i] = ToMeta(f);
            }
            form.Add(new MultipartFormDataSection("metadata", JsonConvert.SerializeObject(meta)));

            var resp = await _rest.PostMultipartAsync($"{req.BackendBaseUrl}/api/scan", form, Auth(req), ct);
            if (!resp.IsSuccess) return Result<string>.Fail(resp.Error ?? $"HTTP {resp.Code}");

            try
            {
                var jo = JObject.Parse(resp.Text);
                string jobId = jo["jobId"]?.ToString() ?? jo["job_id"]?.ToString() ?? jo["id"]?.ToString();
                return string.IsNullOrEmpty(jobId) ? Result<string>.Fail("resposta sem jobId") : Result<string>.Ok(jobId);
            }
            catch (System.Exception e) { return Result<string>.Fail($"JSON invalido: {e.Message}"); }
        }

        public async Awaitable<Result<ReconstructionStatus>> GetStatusAsync(ReconstructionRequest req, string jobId, CancellationToken ct)
        {
            var r = await _rest.GetJsonAsync($"{req.BackendBaseUrl}/api/scan/{jobId}/status", Auth(req), ct);
            if (!r.IsSuccess) return Result<ReconstructionStatus>.Fail(r.Error);
            try
            {
                var jo = JObject.Parse(r.Value);
                return Result<ReconstructionStatus>.Ok(new ReconstructionStatus
                {
                    JobId = jobId,
                    State = MapState(jo["status"]?.ToString() ?? ""),
                    Progress = (float)(jo["progress"] ?? 0),
                    Error = jo["error_message"]?.ToString()
                });
            }
            catch (System.Exception e) { return Result<ReconstructionStatus>.Fail($"JSON invalido: {e.Message}"); }
        }

        public async Awaitable<Result<SceneContract>> GetSceneAsync(ReconstructionRequest req, string jobId, CancellationToken ct)
        {
            var r = await _rest.GetJsonAsync($"{req.BackendBaseUrl}/api/scan/{jobId}/result", Auth(req), ct);
            if (!r.IsSuccess) return Result<SceneContract>.Fail(r.Error);
            try { return Result<SceneContract>.Ok(JsonConvert.DeserializeObject<SceneContract>(r.Value)); }
            catch (System.Exception e) { return Result<SceneContract>.Fail($"JSON invalido: {e.Message}"); }
        }

        private static Dictionary<string, string> Auth(ReconstructionRequest req)
        {
            var h = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(req.ApiKey)) h["X-Api-Key"] = req.ApiKey;
            return h;
        }

        private static FrameMeta ToMeta(CapturedFrame f)
        {
            var m = new FrameMeta { Ts = f.Timestamp, Intrinsics = new double[] { f.Intrinsics.x, f.Intrinsics.y, f.Intrinsics.z, f.Intrinsics.w } };
            var p = new double[16];
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    p[r * 4 + c] = f.Pose[r, c];
            m.Pose = p;
            return m;
        }

        private static ReconstructionState MapState(string s)
        {
            if (string.IsNullOrEmpty(s)) return ReconstructionState.Processing;
            string u = s.ToUpperInvariant();
            if (u.Contains("ENVIADO") || u.Contains("UPLOAD") || u.Contains("QUEUED")) return ReconstructionState.Uploading;
            if (u.Contains("PROCESS")) return ReconstructionState.Processing;
            if (u.Contains("GERANDO") || u.Contains("GENERAT") || u.Contains("TRAIN")) return ReconstructionState.Generating3D;
            if (u.Contains("FINALIZADO") || u.Contains("DONE") || u.Contains("READY") || u.Contains("COMPLETE")) return ReconstructionState.Ready;
            if (u.Contains("ERROR") || u.Contains("FAIL") || u.Contains("FALH")) return ReconstructionState.Failed;
            return ReconstructionState.Processing;
        }

        private sealed class FrameMeta
        {
            [JsonProperty("pose")] public double[] Pose;
            [JsonProperty("intrinsics")] public double[] Intrinsics;
            [JsonProperty("ts")] public double Ts;
        }
    }
}
