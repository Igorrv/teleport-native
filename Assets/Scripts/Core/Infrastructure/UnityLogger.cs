using UnityEngine;

namespace TeleportNative.Core
{
    /// <summary>ILogger sobre UnityEngine.Debug, com prefixo [tag] para filtragem.</summary>
    public sealed class UnityLogger : ILogger
    {
        public static readonly ILogger Instance = new UnityLogger();

        public void Info(string tag, string message) => Debug.Log($"[{tag}] {message}");
        public void Warn(string tag, string message) => Debug.LogWarning($"[{tag}] {message}");
        public void Error(string tag, string message) => Debug.LogError($"[{tag}] {message}");
    }
}
