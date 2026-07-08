using System.IO;
using UnityEngine;
using TeleportNative.Core;

namespace TeleportNative.Network
{
    /// <summary>IStorage sobre Application.persistentDataPath.</summary>
    public sealed class LocalStorage : IStorage
    {
        public string PersistentRoot => Application.persistentDataPath;

        public string FullPath(string relPath) => Path.Combine(PersistentRoot, relPath);

        public bool Exists(string relPath) => File.Exists(FullPath(relPath));

        public Result<string> WriteBytes(string relPath, byte[] data)
        {
            try
            {
                string path = FullPath(relPath);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, data);
                return Result<string>.Ok(path);
            }
            catch (System.Exception e) { return Result<string>.Fail(e.Message); }
        }

        public Result<byte[]> ReadBytes(string relPath)
        {
            try { return Result<byte[]>.Ok(File.ReadAllBytes(FullPath(relPath))); }
            catch (System.Exception e) { return Result<byte[]>.Fail(e.Message); }
        }

        public Result<string> WriteText(string relPath, string text) => WriteBytes(relPath, System.Text.Encoding.UTF8.GetBytes(text));

        public Result<string> ReadText(string relPath)
        {
            var r = ReadBytes(relPath);
            return r.IsSuccess ? Result<string>.Ok(System.Text.Encoding.UTF8.GetString(r.Value)) : Result<string>.Fail(r.Error);
        }

        public void Delete(string relPath)
        {
            string path = FullPath(relPath);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
