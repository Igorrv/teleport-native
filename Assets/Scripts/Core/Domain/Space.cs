using System;

namespace TeleportNative.Core
{
    /// <summary>Item da Biblioteca (Library): um espaco capturado/reconstruido.</summary>
    [Serializable]
    public sealed class Space
    {
        public string Id;            // guid local
        public string Name;
        public string Status;        // ReconstructionStatus (string p/ serializacao simples)
        public string ThumbnailPath; // caminho local (cache)
        public string SplatPath;     // caminho local do .ksplat/.splat baixado
        public string SceneUrl;      // URL publica do HomeView /world/:id (compartilhar)
        public long CreatedAtUnix;   // ordenacao/ordenar por data
        public long SizeBytes;       // tamanho estimado (gestao de cache)
    }
}
