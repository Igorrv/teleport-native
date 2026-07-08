using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TeleportNative.Core;

namespace TeleportNative.Network
{
    /// <summary>ISceneRepository persistido em disco (library.json). Mantem lista em memoria.</summary>
    public sealed class SceneRepository : ISceneRepository
    {
        private const string IndexFile = "library.json";
        private readonly IStorage _storage;
        private readonly List<Space> _spaces = new();

        public SceneRepository(IStorage storage)
        {
            _storage = storage;
            Load();
        }

        public IReadOnlyList<Space> All() => _spaces.OrderByDescending(s => s.CreatedAtUnix).ToList();
        public Space Get(string id) => _spaces.Find(s => s.Id == id);

        public void Save(Space space)
        {
            int i = _spaces.FindIndex(s => s.Id == space.Id);
            if (i >= 0) _spaces[i] = space; else _spaces.Add(space);
            Persist();
        }

        public void Delete(string id)
        {
            int i = _spaces.FindIndex(s => s.Id == id);
            if (i < 0) return;
            _spaces.RemoveAt(i);
            Persist();
        }

        public void Rename(string id, string name)
        {
            var s = Get(id);
            if (s == null) return;
            s.Name = name;
            Persist();
        }

        private void Load()
        {
            var r = _storage.ReadText(IndexFile);
            if (!r.IsSuccess) return;
            try { _spaces.Clear(); _spaces.AddRange(JsonConvert.DeserializeObject<List<Space>>(r.Value) ?? new()); }
            catch { /* indice corrompido: comeca limpo */ }
        }

        private void Persist()
        {
            _storage.WriteText(IndexFile, JsonConvert.SerializeObject(_spaces));
        }
    }
}
