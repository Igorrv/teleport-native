namespace TeleportNative.Core
{
    /// <summary>Logger estruturado e trocavel (testes/silent). Evita UnityEngine.Debug espalhado.</summary>
    public interface ILogger
    {
        void Info(string tag, string message);
        void Warn(string tag, string message);
        void Error(string tag, string message);
    }
}
