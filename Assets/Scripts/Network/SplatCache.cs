using System;
using System.Threading;
using TeleportNative.Core;
using UnityEngine;

namespace TeleportNative.Network
{
    /// <summary>ISplatCache que baixa via RestClient e grava com IStorage. Extensao por cacheKey.</summary>
    public sealed class SplatCache : ISplatCache
    {
        private const string Dir = "splats";
        private readonly RestClient _rest;
        private readonly IStorage _storage;

        public SplatCache(RestClient rest, IStorage storage) { _rest = rest; _storage = storage; }

        public bool IsCached(string cacheKey) => _storage.Exists($"{Dir}/{cacheKey}.ksplat") || _storage.Exists($"{Dir}/{cacheKey}.splat");

        public string LocalPath(string cacheKey)
        {
            string ksplat = $"{Dir}/{cacheKey}.ksplat";
            return _storage.Exists(ksplat) ? _storage.FullPath(ksplat) : _storage.FullPath($"{Dir}/{cacheKey}.splat");
        }

        public async Awaitable<Result<string>> EnsureLocalAsync(string url, string cacheKey, IProgress<float> progress, CancellationToken ct)
        {
            // Preserva a extensao vinda da URL quando possivel (.ksplat/.splat/.ply).
            string ext = ".ksplat";
            if (!string.IsNullOrEmpty(url))
            {
                int q = url.IndexOf('?');
                string path = q >= 0 ? url.Substring(0, q) : url;
                int d = path.LastIndexOf('.');
                if (d >= 0) ext = path.Substring(d);
            }
            string rel = $"{Dir}/{cacheKey}{ext}";
            if (_storage.Exists(rel)) return Result<string>.Ok(_storage.FullPath(rel));

            var data = await _rest.GetBytesAsync(url, null, progress, ct);
            if (!data.IsSuccess) return Result<string>.Fail(data.Error);

            var w = _storage.WriteBytes(rel, data.Value);
            return w.IsSuccess ? Result<string>.Ok(w.Value) : Result<string>.Fail(w.Error);
        }
    }
}
