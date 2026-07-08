using UnityEngine;

namespace TeleportNative.Capture
{
    /// <summary>
    /// Decide se um frame vira keyframe (descarta blur / pose redundante). Classe pura
    /// (nao-MonoBehaviour) p/ ser testada em EditMode. Menos frames bons > muitos ruins.
    /// </summary>
    public interface IFrameSelector
    {
        void Reset();
        bool IsPoseNovel(Matrix4x4 pose);   // checape barato (sem imagem) p/ pular trabalho
        bool ShouldKeep(Matrix4x4 pose, float sharpness); // checa blur + commit
    }
}
