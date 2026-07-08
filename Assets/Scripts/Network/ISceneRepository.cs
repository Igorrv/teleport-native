using System.Collections.Generic;
using TeleportNative.Core;

namespace TeleportNative.Network
{
    /// <summary>Biblioteca (Library) de espacos capturados. Persiste em library.json (via IStorage).</summary>
    public interface ISceneRepository
    {
        IReadOnlyList<Space> All();
        Space Get(string id);
        void Save(Space space);
        void Delete(string id);
        void Rename(string id, string name);
    }
}
