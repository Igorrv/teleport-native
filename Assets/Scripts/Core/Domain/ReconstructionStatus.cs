using System;

namespace TeleportNative.Core
{
    /// <summary>Ciclo de vida de uma reconstrucao 3DGS. Espelha o pipeline da nuvem (ver tela Processing).</summary>
    public enum ReconstructionState
    {
        Queued,        // ENVIADO (na fila do provider)
        Uploading,     // subindo frames+poses
        Processing,    // PROCESSANDO (COLMAP/SfM)
        Generating3D,  // GERANDO_3D (treino 3DGS)
        Ready,         // FINALIZADO (splat disponivel)
        Failed         // erro (retry/backoff)
    }

    [Serializable]
    public sealed class ReconstructionStatus
    {
        public string JobId;
        public ReconstructionState State;
        [NonSerialized] public float Progress; // 0..1 (pode vir do provider)
        public string Error;
        public string SplatUrl; // preenchido em Ready

        public bool IsTerminal => State == ReconstructionState.Ready || State == ReconstructionState.Failed;

        public static string Label(ReconstructionState s) => s switch
        {
            ReconstructionState.Queued => "Na fila",
            ReconstructionState.Uploading => "Enviando fotos",
            ReconstructionState.Processing => "Processando",
            ReconstructionState.Generating3D => "Gerando 3D",
            ReconstructionState.Ready => "Finalizado",
            ReconstructionState.Failed => "Falhou",
            _ => s.ToString()
        };
    }
}
