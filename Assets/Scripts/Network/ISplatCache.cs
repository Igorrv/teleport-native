using System;
using System.Threading;
using TeleportNative.Core;
using UnityEngine;

namespace TeleportNative.Network
{
    /// <summary>Cache local de splats baixados da nuvem. Devolve caminho local pronto p/ o viewer.</summary>
    public interface ISplatCache
    {
        bool IsCached(string cacheKey);
        string LocalPath(string cacheKey);
        Awaitable<Result<string>> EnsureLocalAsync(string url, string cacheKey, IProgress<float> progress, CancellationToken ct);
    }
}
