using TeleportNative.Core;

namespace TeleportNative.Network
{
    /// <summary>Camada de arquivos local (persistentDataPath). Trocavel p/ testes/fake.</summary>
    public interface IStorage
    {
        string PersistentRoot { get; }
        string FullPath(string relPath);
        bool Exists(string relPath);
        Result<string> WriteBytes(string relPath, byte[] data);
        Result<byte[]> ReadBytes(string relPath);
        Result<string> WriteText(string relPath, string text);
        Result<string> ReadText(string relPath);
        void Delete(string relPath);
    }
}
