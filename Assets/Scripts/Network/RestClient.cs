using System;
using System.Collections.Generic;
using System.Threading;
using TeleportNative.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace TeleportNative.Network
{
    public readonly struct HttpResponse
    {
        public readonly bool IsSuccess;
        public readonly string Text;
        public readonly byte[] Data;
        public readonly long Code;
        public readonly string Error;
        public HttpResponse(bool ok, string text, byte[] data, long code, string error)
        { IsSuccess = ok; Text = text; Data = data; Code = code; Error = error; }
    }

    /// <summary>
    /// Wrapper sobre UnityWebRequest com JSON, multipart, retry/backoff exponencial (so erros
    /// transitores; 4xx nao retenta) e progresso. Tudo assincrono via Awaitable (Unity 6).
    /// </summary>
    public sealed class RestClient
    {
        private readonly int _maxAttempts;
        private readonly float _baseDelay;
        private readonly float _maxDelay;

        public RestClient(int maxAttempts = 3, float baseDelay = 1.5f, float maxDelay = 15f)
        {
            _maxAttempts = Math.Max(1, maxAttempts);
            _baseDelay = baseDelay;
            _maxDelay = maxDelay;
        }

        public async Awaitable<Result<string>> GetJsonAsync(string url, Dictionary<string, string> headers, CancellationToken ct)
        {
            var r = await SendAsync(() => UnityWebRequest.Get(url), headers, retry: true, progress: null, ct);
            return r.IsSuccess ? Result<string>.Ok(r.Text) : Result<string>.Fail(r.Error ?? $"HTTP {r.Code}");
        }

        public async Awaitable<Result<byte[]>> GetBytesAsync(string url, Dictionary<string, string> headers, IProgress<float> progress, CancellationToken ct)
        {
            var r = await SendAsync(() => UnityWebRequest.Get(url), headers, retry: true, progress, ct);
            return r.IsSuccess ? Result<byte[]>.Ok(r.Data) : Result<byte[]>.Fail(r.Error ?? $"HTTP {r.Code}");
        }

        public Awaitable<HttpResponse> PostJsonAsync(string url, string jsonBody, Dictionary<string, string> headers, CancellationToken ct)
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            return SendAsync(() =>
            {
                var req = new UnityWebRequest(url, "POST");
                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                return req;
            }, headers, retry: false, progress: null, ct);
        }

        public Awaitable<HttpResponse> PostMultipartAsync(string url, List<IMultipartFormSection> form, Dictionary<string, string> headers, CancellationToken ct)
        {
            return SendAsync(() => UnityWebRequest.Post(url, form), headers, retry: false, progress: null, ct);
        }

        private async Awaitable<HttpResponse> SendAsync(
            Func<UnityWebRequest> factory, Dictionary<string, string> headers,
            bool retry, IProgress<float> progress, CancellationToken ct)
        {
            float delay = _baseDelay;
            for (int attempt = 1; ; attempt++)
            {
                UnityWebRequest req = factory();
                ApplyHeaders(req, headers);

                UnityWebRequestAsyncOperation op = req.SendWebRequest();
                try
                {
                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Dispose(); ct.ThrowIfCancellationRequested(); }
                        progress?.Report(req.downloadProgress);
                        await Awaitable.NextFrameAsync(ct);
                    }
                }
                catch (OperationCanceledException) { req.Dispose(); throw; }

                bool ok = req.result == UnityWebRequest.Result.Success;
                byte[] data = req.downloadHandler is DownloadHandlerBuffer b ? b.data : null;
                var resp = new HttpResponse(ok, req.downloadHandler?.text, data, req.responseCode, req.error);
                req.Dispose();

                if (ok) { progress?.Report(1f); return resp; }

                bool clientError = resp.Code >= 400 && resp.Code < 500 && resp.Code != 408 && resp.Code != 429;
                if (!retry || clientError || attempt >= _maxAttempts) return resp;

                await Awaitable.WaitForSecondsAsync(delay, ct);
                delay = Math.Min(delay * 2f, _maxDelay);
            }
        }

        private static void ApplyHeaders(UnityWebRequest req, Dictionary<string, string> headers)
        {
            if (headers == null) return;
            foreach (var kv in headers) req.SetRequestHeader(kv.Key, kv.Value);
        }
    }
}
